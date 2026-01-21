using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Models.Enums;
using GrcMvc.Exceptions;
using GrcMvc.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Risk Acceptance Workflow Service
    /// Handles: Assess → Accept/Reject → Monitor workflow
    /// Uses state machine for valid transitions and proper async notification handling.
    /// </summary>
    public class RiskWorkflowService : IRiskWorkflowService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RiskWorkflowService> _logger;
        private readonly INotificationService _notificationService;
        private readonly IUserDirectoryService _userDirectoryService;

        public RiskWorkflowService(
            IUnitOfWork unitOfWork,
            ILogger<RiskWorkflowService> logger,
            INotificationService notificationService,
            IUserDirectoryService userDirectoryService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _notificationService = notificationService;
            _userDirectoryService = userDirectoryService;
        }

        /// <summary>
        /// Accept a risk (acknowledge and monitor).
        /// Uses state machine to validate transition.
        /// </summary>
        public async Task<Risk> AcceptAsync(Guid riskId, string acceptedBy, string? comments = null)
        {
            var risk = await _unitOfWork.Risks.GetByIdAsync(riskId);
            if (risk == null)
                throw new WorkflowNotFoundException("Risk", riskId);

            // Validate state transition using state machine
            var currentStatus = risk.Status.ToRiskStatus();
            var targetStatus = RiskWorkflowStatus.Accepted;

            if (!RiskStateMachine.CanTransition(currentStatus, targetStatus))
            {
                var validTargets = RiskStateMachine.GetValidTransitions(currentStatus)
                    .Select(s => s.ToStatusString());
                throw new InvalidStateTransitionException(
                    risk.Status ?? "null",
                    targetStatus.ToStatusString(),
                    validTargets);
            }

            risk.Status = targetStatus.ToStatusString();
            risk.ModifiedDate = DateTime.UtcNow;
            risk.ModifiedBy = acceptedBy;
            risk.ReviewDate = DateTime.UtcNow;

            await _unitOfWork.Risks.UpdateAsync(risk);
            await _unitOfWork.SaveChangesAsync();

            // Notify stakeholders - properly awaited with error tracking
            var notificationResult = await NotifyStakeholdersSafeAsync(risk, $"Risk accepted by {acceptedBy}");
            if (!notificationResult.Success)
            {
                _logger.LogWarning("Notification delivery incomplete for risk {RiskId}: {Reason}",
                    riskId, notificationResult.ErrorMessage);
            }

            _logger.LogInformation("Risk {RiskId} accepted by {User}", riskId, acceptedBy);
            return risk;
        }

        /// <summary>
        /// Reject risk acceptance (requires mitigation).
        /// Uses state machine to validate transition.
        /// </summary>
        public async Task<Risk> RejectAcceptanceAsync(Guid riskId, string rejectedBy, string reason)
        {
            var risk = await _unitOfWork.Risks.GetByIdAsync(riskId);
            if (risk == null)
                throw new WorkflowNotFoundException("Risk", riskId);

            // Validate state transition using state machine
            var currentStatus = risk.Status.ToRiskStatus();
            var targetStatus = RiskWorkflowStatus.RequiresMitigation;

            if (!RiskStateMachine.CanTransition(currentStatus, targetStatus))
            {
                var validTargets = RiskStateMachine.GetValidTransitions(currentStatus)
                    .Select(s => s.ToStatusString());
                throw new InvalidStateTransitionException(
                    risk.Status ?? "null",
                    targetStatus.ToStatusString(),
                    validTargets);
            }

            risk.Status = targetStatus.ToStatusString();
            risk.ModifiedDate = DateTime.UtcNow;
            risk.ModifiedBy = rejectedBy;
            risk.MitigationStrategy = reason;

            await _unitOfWork.Risks.UpdateAsync(risk);
            await _unitOfWork.SaveChangesAsync();

            // Notify risk owner - properly awaited with error tracking
            var notificationResult = await NotifyRiskOwnerSafeAsync(risk, $"Risk acceptance rejected: {reason}");
            if (!notificationResult.Success)
            {
                _logger.LogWarning("Notification delivery failed for risk {RiskId}: {Reason}",
                    riskId, notificationResult.ErrorMessage);
            }

            _logger.LogInformation("Risk {RiskId} acceptance rejected by {User}: {Reason}", riskId, rejectedBy, reason);
            return risk;
        }

        /// <summary>
        /// Mark risk as mitigated.
        /// Uses state machine to validate transition.
        /// </summary>
        public async Task<Risk> MarkMitigatedAsync(Guid riskId, string mitigatedBy, string mitigationDetails)
        {
            var risk = await _unitOfWork.Risks.GetByIdAsync(riskId);
            if (risk == null)
                throw new WorkflowNotFoundException("Risk", riskId);

            // Validate state transition using state machine
            var currentStatus = risk.Status.ToRiskStatus();
            var targetStatus = RiskWorkflowStatus.Mitigated;

            if (!RiskStateMachine.CanTransition(currentStatus, targetStatus))
            {
                var validTargets = RiskStateMachine.GetValidTransitions(currentStatus)
                    .Select(s => s.ToStatusString());
                throw new InvalidStateTransitionException(
                    risk.Status ?? "null",
                    targetStatus.ToStatusString(),
                    validTargets);
            }

            risk.Status = targetStatus.ToStatusString();
            risk.MitigationStrategy = mitigationDetails;
            risk.ModifiedDate = DateTime.UtcNow;
            risk.ModifiedBy = mitigatedBy;
            risk.ReviewDate = DateTime.UtcNow;

            await _unitOfWork.Risks.UpdateAsync(risk);
            await _unitOfWork.SaveChangesAsync();

            // Notify stakeholders - properly awaited with error tracking
            var notificationResult = await NotifyStakeholdersSafeAsync(risk, "Risk has been mitigated");
            if (!notificationResult.Success)
            {
                _logger.LogWarning("Notification delivery incomplete for risk {RiskId}: {Reason}",
                    riskId, notificationResult.ErrorMessage);
            }

            _logger.LogInformation("Risk {RiskId} marked as mitigated by {User}", riskId, mitigatedBy);
            return risk;
        }

        /// <summary>
        /// Start monitoring an accepted or mitigated risk.
        /// </summary>
        public async Task<Risk> StartMonitoringAsync(Guid riskId, string monitoredBy)
        {
            var risk = await _unitOfWork.Risks.GetByIdAsync(riskId);
            if (risk == null)
                throw new WorkflowNotFoundException("Risk", riskId);

            var currentStatus = risk.Status.ToRiskStatus();
            var targetStatus = RiskWorkflowStatus.Monitoring;

            if (!RiskStateMachine.CanTransition(currentStatus, targetStatus))
            {
                var validTargets = RiskStateMachine.GetValidTransitions(currentStatus)
                    .Select(s => s.ToStatusString());
                throw new InvalidStateTransitionException(
                    risk.Status ?? "null",
                    targetStatus.ToStatusString(),
                    validTargets);
            }

            risk.Status = targetStatus.ToStatusString();
            risk.ModifiedDate = DateTime.UtcNow;
            risk.ModifiedBy = monitoredBy;

            await _unitOfWork.Risks.UpdateAsync(risk);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Risk {RiskId} now being monitored by {User}", riskId, monitoredBy);
            return risk;
        }

        /// <summary>
        /// Close a risk (final state).
        /// </summary>
        public async Task<Risk> CloseAsync(Guid riskId, string closedBy, string? closureNotes = null)
        {
            var risk = await _unitOfWork.Risks.GetByIdAsync(riskId);
            if (risk == null)
                throw new WorkflowNotFoundException("Risk", riskId);

            var currentStatus = risk.Status.ToRiskStatus();
            var targetStatus = RiskWorkflowStatus.Closed;

            if (!RiskStateMachine.CanTransition(currentStatus, targetStatus))
            {
                var validTargets = RiskStateMachine.GetValidTransitions(currentStatus)
                    .Select(s => s.ToStatusString());
                throw new InvalidStateTransitionException(
                    risk.Status ?? "null",
                    targetStatus.ToStatusString(),
                    validTargets);
            }

            risk.Status = targetStatus.ToStatusString();
            risk.ModifiedDate = DateTime.UtcNow;
            risk.ModifiedBy = closedBy;

            await _unitOfWork.Risks.UpdateAsync(risk);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Risk {RiskId} closed by {User}", riskId, closedBy);
            return risk;
        }

        private async Task<(bool Success, string? ErrorMessage)> NotifyStakeholdersSafeAsync(Risk risk, string message)
        {
            try
            {
                await NotifyStakeholdersAsync(risk, message);
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send stakeholder notification for risk {RiskId}", risk.Id);
                return (false, ex.Message);
            }
        }

        private async Task<(bool Success, string? ErrorMessage)> NotifyRiskOwnerSafeAsync(Risk risk, string message)
        {
            try
            {
                await NotifyRiskOwnerAsync(risk, message);
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send risk owner notification for risk {RiskId}", risk.Id);
                return (false, ex.Message);
            }
        }

        private async Task NotifyStakeholdersAsync(Risk risk, string message)
        {
            try
            {
                // Get stakeholders based on risk level
                var stakeholderRoles = GetStakeholderRolesForRiskLevel(risk.RiskLevel);
                var stakeholders = new List<ApplicationUser>();

                foreach (var roleName in stakeholderRoles)
                {
                    var usersInRole = await _userDirectoryService.GetUsersInRoleAsync(roleName);
                    stakeholders.AddRange(usersInRole);
                }

                // Remove duplicates
                stakeholders = stakeholders.DistinctBy(u => u.Id).ToList();

                _logger.LogInformation("Notifying {Count} stakeholders for Risk {RiskId}: {Message}",
                    stakeholders.Count, risk.Id, message);

                // Send notification to each stakeholder
                foreach (var stakeholder in stakeholders)
                {
                    await _notificationService.SendNotificationAsync(
                        workflowInstanceId: Guid.Empty, // No workflow instance for direct notifications
                        recipientUserId: stakeholder.Id,
                        notificationType: "RiskUpdate",
                        subject: $"Risk Update: {risk.Name}",
                        body: message,
                        priority: risk.RiskLevel == "Critical" ? "High" : "Normal",
                        tenantId: risk.TenantId ?? Guid.Empty);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send stakeholder notification for risk {RiskId}", risk.Id);
            }
        }

        private async Task NotifyRiskOwnerAsync(Risk risk, string message)
        {
            try
            {
                // Find the risk owner by username/email
                if (string.IsNullOrEmpty(risk.Owner))
                {
                    _logger.LogWarning("Risk {RiskId} has no owner set, cannot send notification", risk.Id);
                    return;
                }

                // Try to find owner by email first, then search by name/term
                var owner = await _userDirectoryService.GetUserByEmailAsync(risk.Owner);
                if (owner == null)
                {
                    // Try searching by term (username/name)
                    var searchResults = await _userDirectoryService.SearchUsersAsync(risk.Owner, 1);
                    owner = searchResults.FirstOrDefault();
                }

                if (owner != null)
                {
                    await _notificationService.SendNotificationAsync(
                        workflowInstanceId: Guid.Empty,
                        recipientUserId: owner.Id,
                        notificationType: "RiskOwnerNotification",
                        subject: $"Risk Action Required: {risk.Name}",
                        body: message,
                        priority: "High",
                        tenantId: risk.TenantId ?? Guid.Empty);

                    _logger.LogInformation("Notification sent to owner {Owner} for Risk {RiskId}",
                        risk.Owner, risk.Id);
                }
                else
                {
                    _logger.LogWarning("Could not find user for risk owner: {Owner}", risk.Owner);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send notification to risk owner for risk {RiskId}", risk.Id);
            }
        }

        /// <summary>
        /// Get stakeholder roles based on risk level.
        /// Higher risk levels involve more senior stakeholders.
        /// </summary>
        private static string[] GetStakeholderRolesForRiskLevel(string riskLevel)
        {
            return riskLevel switch
            {
                "Critical" => new[] { "RiskManager", "ComplianceManager", "PlatformAdmin", "BoardMember" },
                "High" => new[] { "RiskManager", "ComplianceManager" },
                "Medium" => new[] { "RiskManager", "RiskAnalyst" },
                _ => new[] { "RiskManager" } // Low
            };
        }
    }
}
