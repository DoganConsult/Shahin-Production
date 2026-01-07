using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Risk Acceptance Workflow Service
    /// Handles: Assess → Accept/Reject → Monitor workflow
    /// </summary>
    public class RiskWorkflowService : IRiskWorkflowService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RiskWorkflowService> _logger;
        private readonly INotificationService _notificationService;

        public RiskWorkflowService(
            IUnitOfWork unitOfWork,
            ILogger<RiskWorkflowService> logger,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Accept a risk (acknowledge and monitor)
        /// </summary>
        public async Task<Risk> AcceptAsync(Guid riskId, string acceptedBy, string? comments = null)
        {
            var risk = await _unitOfWork.Risks.GetByIdAsync(riskId);
            if (risk == null)
                throw new InvalidOperationException($"Risk {riskId} not found");

            if (risk.Status == "Accepted" || risk.Status == "Mitigated")
                throw new InvalidOperationException($"Risk in status '{risk.Status}' cannot be accepted again.");

            risk.Status = "Accepted";
            risk.ModifiedDate = DateTime.UtcNow;
            risk.ModifiedBy = acceptedBy;
            risk.ReviewDate = DateTime.UtcNow;

            await _unitOfWork.Risks.UpdateAsync(risk);
            await _unitOfWork.SaveChangesAsync();

            // Notify stakeholders
            await NotifyStakeholdersAsync(risk, $"Risk accepted by {acceptedBy}");

            _logger.LogInformation("Risk {RiskId} accepted by {User}", riskId, acceptedBy);
            return risk;
        }

        /// <summary>
        /// Reject risk acceptance (requires mitigation)
        /// </summary>
        public async Task<Risk> RejectAcceptanceAsync(Guid riskId, string rejectedBy, string reason)
        {
            var risk = await _unitOfWork.Risks.GetByIdAsync(riskId);
            if (risk == null)
                throw new InvalidOperationException($"Risk {riskId} not found");

            risk.Status = "Requires Mitigation";
            risk.ModifiedDate = DateTime.UtcNow;
            risk.ModifiedBy = rejectedBy;
            risk.MitigationStrategy = reason;

            await _unitOfWork.Risks.UpdateAsync(risk);
            await _unitOfWork.SaveChangesAsync();

            // Notify risk owner
            await NotifyRiskOwnerAsync(risk, $"Risk acceptance rejected: {reason}");

            _logger.LogInformation("Risk {RiskId} acceptance rejected by {User}: {Reason}", riskId, rejectedBy, reason);
            return risk;
        }

        /// <summary>
        /// Mark risk as mitigated
        /// </summary>
        public async Task<Risk> MarkMitigatedAsync(Guid riskId, string mitigatedBy, string mitigationDetails)
        {
            var risk = await _unitOfWork.Risks.GetByIdAsync(riskId);
            if (risk == null)
                throw new InvalidOperationException($"Risk {riskId} not found");

            risk.Status = "Mitigated";
            risk.MitigationStrategy = mitigationDetails;
            risk.ModifiedDate = DateTime.UtcNow;
            risk.ModifiedBy = mitigatedBy;
            risk.ReviewDate = DateTime.UtcNow;

            await _unitOfWork.Risks.UpdateAsync(risk);
            await _unitOfWork.SaveChangesAsync();

            // Notify stakeholders
            await NotifyStakeholdersAsync(risk, "Risk has been mitigated");

            _logger.LogInformation("Risk {RiskId} marked as mitigated by {User}", riskId, mitigatedBy);
            return risk;
        }

        private async Task NotifyStakeholdersAsync(Risk risk, string message)
        {
            try
            {
                // TODO: Get stakeholders from role/permission system
                _logger.LogInformation("Notification: {Message} for Risk {RiskId}", message, risk.Id);
                // await _notificationService.SendNotificationAsync(...);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send notification for risk {RiskId}", risk.Id);
            }
        }

        private async Task NotifyRiskOwnerAsync(Risk risk, string message)
        {
            try
            {
                // TODO: Notify the risk owner (risk.Owner)
                _logger.LogInformation("Notification: {Message} to {Owner} for Risk {RiskId}", 
                    message, risk.Owner, risk.Id);
                // await _notificationService.SendNotificationAsync(...);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send notification for risk {RiskId}", risk.Id);
            }
        }
    }
}
