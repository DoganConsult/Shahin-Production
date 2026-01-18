using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Models.Enums;
using GrcMvc.Exceptions;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Evidence Approval Workflow Service
    /// Handles: Submit → Review → Approve → Archive workflow
    /// Uses state machine for valid transitions and proper async notification handling.
    /// </summary>
    public class EvidenceWorkflowService : IEvidenceWorkflowService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EvidenceWorkflowService> _logger;
        private readonly INotificationService _notificationService;
        private readonly IUserDirectoryService _userDirectoryService;

        // Roles that can review evidence
        private static readonly string[] ReviewerRoles = new[]
        {
            "ComplianceManager", "ComplianceOfficer", "Auditor", "EvidenceOfficer"
        };

        public EvidenceWorkflowService(
            IUnitOfWork unitOfWork,
            ILogger<EvidenceWorkflowService> logger,
            INotificationService notificationService,
            IUserDirectoryService userDirectoryService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _notificationService = notificationService;
            _userDirectoryService = userDirectoryService;
        }

        /// <summary>
        /// Submit evidence for review.
        /// Uses state machine to validate transition.
        /// </summary>
        public async Task<Evidence> SubmitForReviewAsync(Guid evidenceId, string submittedBy)
        {
            var evidence = await _unitOfWork.Evidences.GetByIdAsync(evidenceId);
            if (evidence == null)
                throw new WorkflowNotFoundException("Evidence", evidenceId);

            // Validate state transition using state machine
            var currentStatus = evidence.VerificationStatus.ToEvidenceStatus();
            var targetStatus = EvidenceVerificationStatus.UnderReview;

            // Draft and Pending can both transition to UnderReview (submit for review)
            if (currentStatus != EvidenceVerificationStatus.Draft &&
                currentStatus != EvidenceVerificationStatus.Pending)
            {
                var validSources = new[] {
                    EvidenceVerificationStatus.Draft.ToStatusString(),
                    EvidenceVerificationStatus.Pending.ToStatusString()
                };
                throw new InvalidStateTransitionException(
                    evidence.VerificationStatus ?? "null",
                    targetStatus.ToStatusString(),
                    validSources);
            }

            evidence.VerificationStatus = targetStatus.ToStatusString();
            evidence.ModifiedDate = DateTime.UtcNow;
            evidence.ModifiedBy = submittedBy;

            await _unitOfWork.Evidences.UpdateAsync(evidence);
            await _unitOfWork.SaveChangesAsync();

            // Notify reviewers - properly awaited with error tracking
            var notificationResult = await NotifyReviewersSafeAsync(evidence, "Evidence submitted for review");
            if (!notificationResult.Success)
            {
                _logger.LogWarning("Notification delivery incomplete for evidence {EvidenceId}: {Reason}",
                    evidenceId, notificationResult.ErrorMessage);
            }

            _logger.LogInformation("Evidence {EvidenceId} submitted for review by {User}", evidenceId, submittedBy);
            return evidence;
        }

        /// <summary>
        /// Approve evidence.
        /// Uses state machine to validate transition.
        /// </summary>
        public async Task<Evidence> ApproveAsync(Guid evidenceId, string approvedBy, string? comments = null)
        {
            var evidence = await _unitOfWork.Evidences.GetByIdAsync(evidenceId);
            if (evidence == null)
                throw new WorkflowNotFoundException("Evidence", evidenceId);

            // Validate state transition using state machine
            var currentStatus = evidence.VerificationStatus.ToEvidenceStatus();
            var targetStatus = EvidenceVerificationStatus.Verified;

            if (!EvidenceStateMachine.CanTransition(currentStatus, targetStatus))
            {
                var validTargets = EvidenceStateMachine.GetValidTransitions(currentStatus)
                    .Select(s => s.ToStatusString());
                throw new InvalidStateTransitionException(
                    evidence.VerificationStatus ?? "null",
                    targetStatus.ToStatusString(),
                    validTargets);
            }

            evidence.VerificationStatus = targetStatus.ToStatusString();
            evidence.VerifiedBy = approvedBy;
            evidence.VerificationDate = DateTime.UtcNow;
            evidence.Comments = comments ?? evidence.Comments;
            evidence.ModifiedDate = DateTime.UtcNow;
            evidence.ModifiedBy = approvedBy;

            await _unitOfWork.Evidences.UpdateAsync(evidence);
            await _unitOfWork.SaveChangesAsync();

            // Notify submitter - properly awaited with error tracking
            var notificationResult = await NotifySubmitterSafeAsync(evidence, "Evidence approved");
            if (!notificationResult.Success)
            {
                _logger.LogWarning("Notification delivery failed for evidence {EvidenceId}: {Reason}",
                    evidenceId, notificationResult.ErrorMessage);
            }

            _logger.LogInformation("Evidence {EvidenceId} approved by {User}", evidenceId, approvedBy);
            return evidence;
        }

        /// <summary>
        /// Reject evidence.
        /// Uses state machine to validate transition.
        /// </summary>
        public async Task<Evidence> RejectAsync(Guid evidenceId, string rejectedBy, string reason)
        {
            var evidence = await _unitOfWork.Evidences.GetByIdAsync(evidenceId);
            if (evidence == null)
                throw new WorkflowNotFoundException("Evidence", evidenceId);

            // Validate state transition using state machine
            var currentStatus = evidence.VerificationStatus.ToEvidenceStatus();
            var targetStatus = EvidenceVerificationStatus.Rejected;

            if (!EvidenceStateMachine.CanTransition(currentStatus, targetStatus))
            {
                var validTargets = EvidenceStateMachine.GetValidTransitions(currentStatus)
                    .Select(s => s.ToStatusString());
                throw new InvalidStateTransitionException(
                    evidence.VerificationStatus ?? "null",
                    targetStatus.ToStatusString(),
                    validTargets);
            }

            evidence.VerificationStatus = targetStatus.ToStatusString();
            evidence.VerifiedBy = rejectedBy;
            evidence.VerificationDate = DateTime.UtcNow;
            evidence.Comments = reason;
            evidence.ModifiedDate = DateTime.UtcNow;
            evidence.ModifiedBy = rejectedBy;

            await _unitOfWork.Evidences.UpdateAsync(evidence);
            await _unitOfWork.SaveChangesAsync();

            // Notify submitter - properly awaited with error tracking
            var notificationResult = await NotifySubmitterSafeAsync(evidence, $"Evidence rejected: {reason}");
            if (!notificationResult.Success)
            {
                _logger.LogWarning("Notification delivery failed for evidence {EvidenceId}: {Reason}",
                    evidenceId, notificationResult.ErrorMessage);
            }

            _logger.LogInformation("Evidence {EvidenceId} rejected by {User}: {Reason}", evidenceId, rejectedBy, reason);
            return evidence;
        }

        /// <summary>
        /// Archive evidence (final state).
        /// Uses state machine to validate transition.
        /// </summary>
        public async Task<Evidence> ArchiveAsync(Guid evidenceId, string archivedBy)
        {
            var evidence = await _unitOfWork.Evidences.GetByIdAsync(evidenceId);
            if (evidence == null)
                throw new WorkflowNotFoundException("Evidence", evidenceId);

            // Validate state transition using state machine
            var currentStatus = evidence.VerificationStatus.ToEvidenceStatus();
            var targetStatus = EvidenceVerificationStatus.Archived;

            if (!EvidenceStateMachine.CanTransition(currentStatus, targetStatus))
            {
                var validTargets = EvidenceStateMachine.GetValidTransitions(currentStatus)
                    .Select(s => s.ToStatusString());
                throw new InvalidStateTransitionException(
                    evidence.VerificationStatus ?? "null",
                    targetStatus.ToStatusString(),
                    validTargets);
            }

            evidence.VerificationStatus = targetStatus.ToStatusString();
            evidence.ModifiedDate = DateTime.UtcNow;
            evidence.ModifiedBy = archivedBy;

            await _unitOfWork.Evidences.UpdateAsync(evidence);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Evidence {EvidenceId} archived by {User}", evidenceId, archivedBy);
            return evidence;
        }

        /// <summary>
        /// Resubmit rejected evidence for review.
        /// Allows users to fix issues and try again.
        /// </summary>
        public async Task<Evidence> ResubmitAsync(Guid evidenceId, string submittedBy)
        {
            var evidence = await _unitOfWork.Evidences.GetByIdAsync(evidenceId);
            if (evidence == null)
                throw new WorkflowNotFoundException("Evidence", evidenceId);

            // Validate state transition: Rejected -> Pending (for resubmission)
            var currentStatus = evidence.VerificationStatus.ToEvidenceStatus();
            var targetStatus = EvidenceVerificationStatus.Pending;

            if (!EvidenceStateMachine.CanTransition(currentStatus, targetStatus))
            {
                var validTargets = EvidenceStateMachine.GetValidTransitions(currentStatus)
                    .Select(s => s.ToStatusString());
                throw new InvalidStateTransitionException(
                    evidence.VerificationStatus ?? "null",
                    targetStatus.ToStatusString(),
                    validTargets);
            }

            evidence.VerificationStatus = targetStatus.ToStatusString();
            evidence.Comments = string.Empty; // Clear previous rejection reason
            evidence.ModifiedDate = DateTime.UtcNow;
            evidence.ModifiedBy = submittedBy;

            await _unitOfWork.Evidences.UpdateAsync(evidence);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Evidence {EvidenceId} resubmitted by {User}", evidenceId, submittedBy);
            return evidence;
        }

        private async Task NotifyReviewersAsync(Evidence evidence, string message)
        {
            try
            {
                // Get all users in reviewer roles
                var reviewers = new List<ApplicationUser>();

                foreach (var roleName in ReviewerRoles)
                {
                    var usersInRole = await _userDirectoryService.GetUsersInRoleAsync(roleName);
                    reviewers.AddRange(usersInRole);
                }

                // Remove duplicates
                reviewers = reviewers.DistinctBy(u => u.Id).ToList();

                _logger.LogInformation("Notifying {Count} reviewers for Evidence {EvidenceId}: {Message}",
                    reviewers.Count, evidence.Id, message);

                // Send notification to each reviewer
                foreach (var reviewer in reviewers)
                {
                    await _notificationService.SendNotificationAsync(
                        workflowInstanceId: Guid.Empty,
                        recipientUserId: reviewer.Id.ToString(),
                        notificationType: "EvidenceReview",
                        subject: $"Evidence Review Required: {evidence.Title}",
                        body: message,
                        priority: "Normal",
                        tenantId: evidence.TenantId ?? Guid.Empty);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send reviewer notification for evidence {EvidenceId}", evidence.Id);
            }
        }

        private async Task<(bool Success, string? ErrorMessage)> NotifyReviewersSafeAsync(Evidence evidence, string message)
        {
            try
            {
                await NotifyReviewersAsync(evidence, message);
                return (true, (string?)null);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send reviewer notification for evidence {EvidenceId}", evidence.Id);
                return (false, ex.Message);
            }
        }

        private async Task<(bool Success, string? ErrorMessage)> NotifySubmitterSafeAsync(Evidence evidence, string message)
        {
            try
            {
                await NotifySubmitterAsync(evidence, message);
                return (true, (string?)null);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send submitter notification for evidence {EvidenceId}", evidence.Id);
                return (false, ex.Message);
            }
        }

        private async Task NotifySubmitterAsync(Evidence evidence, string message)
        {
            try
            {
                // Find the submitter by CollectedBy field
                if (string.IsNullOrEmpty(evidence.CollectedBy))
                {
                    _logger.LogWarning("Evidence {EvidenceId} has no CollectedBy set, cannot send notification", evidence.Id);
                    return;
                }

                // Try to find submitter by email first, then search by name/term
                var submitter = await _userDirectoryService.GetUserByEmailAsync(evidence.CollectedBy);
                if (submitter == null)
                {
                    // Try searching by term (username/name)
                    var searchResults = await _userDirectoryService.SearchUsersAsync(evidence.CollectedBy, 1);
                    submitter = searchResults.FirstOrDefault();
                }

                if (submitter != null)
                {
                    await _notificationService.SendNotificationAsync(
                        workflowInstanceId: Guid.Empty,
                        recipientUserId: submitter.Id.ToString(),
                        notificationType: "EvidenceStatusUpdate",
                        subject: $"Evidence Status Update: {evidence.Title}",
                        body: message,
                        priority: "Normal",
                        tenantId: evidence.TenantId ?? Guid.Empty);

                    _logger.LogInformation("Notification sent to submitter {Submitter} for Evidence {EvidenceId}",
                        evidence.CollectedBy, evidence.Id);
                }
                else
                {
                    _logger.LogWarning("Could not find user for evidence submitter: {Submitter}", evidence.CollectedBy);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send notification to submitter for evidence {EvidenceId}", evidence.Id);
            }
        }
    }
}
