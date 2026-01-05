using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GrcMvc.Models.Entities;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// PHASE 8: Evidence Lifecycle Service Interface
    /// Handles evidence submission, review, scoring, and approval workflow
    /// </summary>
    public interface IEvidenceLifecycleService
    {
        #region Evidence Submission

        /// <summary>
        /// Submit evidence for a requirement
        /// </summary>
        Task<Evidence> SubmitEvidenceAsync(
            Guid tenantId,
            Guid assessmentRequirementId,
            string evidenceTypeCode,
            string title,
            string description,
            string filePath,
            string fileName,
            long fileSize,
            string mimeType,
            string submittedBy);

        /// <summary>
        /// Get evidence by ID
        /// </summary>
        Task<Evidence?> GetEvidenceAsync(Guid evidenceId);

        /// <summary>
        /// Get all evidence for a requirement
        /// </summary>
        Task<List<Evidence>> GetEvidenceByRequirementAsync(Guid assessmentRequirementId);

        /// <summary>
        /// Get all evidence for an assessment
        /// </summary>
        Task<List<Evidence>> GetEvidenceByAssessmentAsync(Guid assessmentId);

        #endregion

        #region Review Workflow

        /// <summary>
        /// Start review workflow for evidence
        /// </summary>
        Task<WorkflowInstance> StartReviewWorkflowAsync(Guid evidenceId, string startedBy);

        /// <summary>
        /// Submit evidence for review (SUBMITTED → IN_REVIEW)
        /// </summary>
        Task<Evidence> SubmitForReviewAsync(Guid evidenceId, string submittedBy);

        /// <summary>
        /// Request changes on evidence (IN_REVIEW → CHANGES_REQUESTED)
        /// </summary>
        Task<Evidence> RequestChangesAsync(Guid evidenceId, string comments, string requestedBy);

        /// <summary>
        /// Resubmit evidence after changes (CHANGES_REQUESTED → IN_REVIEW)
        /// </summary>
        Task<Evidence> ResubmitAsync(Guid evidenceId, string comments, string resubmittedBy);

        #endregion

        #region Scoring

        /// <summary>
        /// Score evidence (0-100)
        /// </summary>
        Task<EvidenceScore> ScoreEvidenceAsync(
            Guid evidenceId,
            int score,
            string scoringCriteria,
            string comments,
            string scoredBy);

        /// <summary>
        /// Get evidence score
        /// </summary>
        Task<EvidenceScore?> GetEvidenceScoreAsync(Guid evidenceId);

        /// <summary>
        /// Get scoring history for evidence
        /// </summary>
        Task<List<EvidenceScore>> GetScoringHistoryAsync(Guid evidenceId);

        #endregion

        #region Approval

        /// <summary>
        /// Approve evidence (IN_REVIEW → APPROVED)
        /// </summary>
        Task<Evidence> ApproveEvidenceAsync(Guid evidenceId, string comments, string approvedBy);

        /// <summary>
        /// Reject evidence (IN_REVIEW → REJECTED)
        /// </summary>
        Task<Evidence> RejectEvidenceAsync(Guid evidenceId, string reason, string rejectedBy);

        #endregion

        #region Derived Status

        /// <summary>
        /// Calculate and update requirement status based on evidence scores
        /// </summary>
        Task<AssessmentRequirement> UpdateRequirementStatusAsync(Guid assessmentRequirementId);

        /// <summary>
        /// Calculate overall assessment compliance score
        /// </summary>
        Task<decimal> CalculateAssessmentScoreAsync(Guid assessmentId);

        #endregion

        #region Statistics

        /// <summary>
        /// Get evidence statistics for a tenant
        /// </summary>
        Task<EvidenceStatistics> GetStatisticsAsync(Guid tenantId);

        #endregion
    }

    /// <summary>
    /// Evidence score entity
    /// </summary>
    public class EvidenceScore : BaseEntity
    {
        public Guid EvidenceId { get; set; }
        public int Score { get; set; } // 0-100
        public string ScoringCriteria { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
        public string ScoredBy { get; set; } = string.Empty;
        public DateTime ScoredAt { get; set; }
        public bool IsFinal { get; set; } = false;

        public virtual Evidence Evidence { get; set; } = null!;
    }

    /// <summary>
    /// Evidence statistics DTO
    /// </summary>
    public class EvidenceStatistics
    {
        public int TotalEvidence { get; set; }
        public int Submitted { get; set; }
        public int InReview { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }
        public int ChangesRequested { get; set; }
        public decimal AverageScore { get; set; }
        public int PendingReview { get; set; }
        public int OverdueReview { get; set; }
    }
}
