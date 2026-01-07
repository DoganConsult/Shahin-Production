using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Evidence Approval Workflow Service
    /// Handles: Submit → Review → Approve → Archive workflow
    /// </summary>
    public class EvidenceWorkflowService : IEvidenceWorkflowService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EvidenceWorkflowService> _logger;
        private readonly INotificationService _notificationService;

        public EvidenceWorkflowService(
            IUnitOfWork unitOfWork,
            ILogger<EvidenceWorkflowService> logger,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Submit evidence for review
        /// </summary>
        public async Task<Evidence> SubmitForReviewAsync(Guid evidenceId, string submittedBy)
        {
            var evidence = await _unitOfWork.Evidences.GetByIdAsync(evidenceId);
            if (evidence == null)
                throw new InvalidOperationException($"Evidence {evidenceId} not found");

            if (evidence.VerificationStatus != "Pending" && evidence.VerificationStatus != "Draft")
                throw new InvalidOperationException($"Evidence in status '{evidence.VerificationStatus}' cannot be submitted. Only Pending or Draft evidence can be submitted.");

            evidence.VerificationStatus = "Under Review";
            evidence.ModifiedDate = DateTime.UtcNow;
            evidence.ModifiedBy = submittedBy;

            await _unitOfWork.Evidences.UpdateAsync(evidence);
            await _unitOfWork.SaveChangesAsync();

            // Notify reviewers
            await NotifyReviewersAsync(evidence, "Evidence submitted for review");

            _logger.LogInformation("Evidence {EvidenceId} submitted for review by {User}", evidenceId, submittedBy);
            return evidence;
        }

        /// <summary>
        /// Approve evidence
        /// </summary>
        public async Task<Evidence> ApproveAsync(Guid evidenceId, string approvedBy, string? comments = null)
        {
            var evidence = await _unitOfWork.Evidences.GetByIdAsync(evidenceId);
            if (evidence == null)
                throw new InvalidOperationException($"Evidence {evidenceId} not found");

            if (evidence.VerificationStatus != "Under Review")
                throw new InvalidOperationException($"Evidence in status '{evidence.VerificationStatus}' cannot be approved. Only Under Review evidence can be approved.");

            evidence.VerificationStatus = "Verified";
            evidence.VerifiedBy = approvedBy;
            evidence.VerificationDate = DateTime.UtcNow;
            evidence.Comments = comments ?? evidence.Comments;
            evidence.ModifiedDate = DateTime.UtcNow;
            evidence.ModifiedBy = approvedBy;

            await _unitOfWork.Evidences.UpdateAsync(evidence);
            await _unitOfWork.SaveChangesAsync();

            // Notify submitter
            await NotifySubmitterAsync(evidence, "Evidence approved");

            _logger.LogInformation("Evidence {EvidenceId} approved by {User}", evidenceId, approvedBy);
            return evidence;
        }

        /// <summary>
        /// Reject evidence
        /// </summary>
        public async Task<Evidence> RejectAsync(Guid evidenceId, string rejectedBy, string reason)
        {
            var evidence = await _unitOfWork.Evidences.GetByIdAsync(evidenceId);
            if (evidence == null)
                throw new InvalidOperationException($"Evidence {evidenceId} not found");

            if (evidence.VerificationStatus != "Under Review")
                throw new InvalidOperationException($"Evidence in status '{evidence.VerificationStatus}' cannot be rejected. Only Under Review evidence can be rejected.");

            evidence.VerificationStatus = "Rejected";
            evidence.VerifiedBy = rejectedBy;
            evidence.VerificationDate = DateTime.UtcNow;
            evidence.Comments = reason;
            evidence.ModifiedDate = DateTime.UtcNow;
            evidence.ModifiedBy = rejectedBy;

            await _unitOfWork.Evidences.UpdateAsync(evidence);
            await _unitOfWork.SaveChangesAsync();

            // Notify submitter
            await NotifySubmitterAsync(evidence, $"Evidence rejected: {reason}");

            _logger.LogInformation("Evidence {EvidenceId} rejected by {User}: {Reason}", evidenceId, rejectedBy, reason);
            return evidence;
        }

        /// <summary>
        /// Archive evidence (final state)
        /// </summary>
        public async Task<Evidence> ArchiveAsync(Guid evidenceId, string archivedBy)
        {
            var evidence = await _unitOfWork.Evidences.GetByIdAsync(evidenceId);
            if (evidence == null)
                throw new InvalidOperationException($"Evidence {evidenceId} not found");

            if (evidence.VerificationStatus != "Verified")
                throw new InvalidOperationException($"Evidence in status '{evidence.VerificationStatus}' cannot be archived. Only Verified evidence can be archived.");

            evidence.VerificationStatus = "Archived";
            evidence.ModifiedDate = DateTime.UtcNow;
            evidence.ModifiedBy = archivedBy;

            await _unitOfWork.Evidences.UpdateAsync(evidence);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Evidence {EvidenceId} archived by {User}", evidenceId, archivedBy);
            return evidence;
        }

        private async Task NotifyReviewersAsync(Evidence evidence, string message)
        {
            try
            {
                // TODO: Get reviewers from role/permission system
                // For now, log notification
                _logger.LogInformation("Notification: {Message} for Evidence {EvidenceId}", message, evidence.Id);
                // await _notificationService.SendNotificationAsync(...);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send notification for evidence {EvidenceId}", evidence.Id);
            }
        }

        private async Task NotifySubmitterAsync(Evidence evidence, string message)
        {
            try
            {
                // TODO: Notify the submitter (evidence.CollectedBy)
                _logger.LogInformation("Notification: {Message} to {Submitter} for Evidence {EvidenceId}", 
                    message, evidence.CollectedBy, evidence.Id);
                // await _notificationService.SendNotificationAsync(...);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send notification for evidence {EvidenceId}", evidence.Id);
            }
        }
    }
}
