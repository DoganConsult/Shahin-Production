using System;
using System.Collections.Generic;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Individual requirement within an assessment, linked to framework controls.
    /// Layer 3: Operational data per tenant assessment.
    /// Includes toolkit guidance, scoring guide, and auto-scoring support.
    /// </summary>
    public class AssessmentRequirement : BaseEntity
    {
        public Guid AssessmentId { get; set; }

        // Control reference
        public string ControlNumber { get; set; } = string.Empty;
        public string ControlTitle { get; set; } = string.Empty;
        public string ControlTitleAr { get; set; } = string.Empty;
        public string RequirementText { get; set; } = string.Empty;
        public string RequirementTextAr { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public string ControlType { get; set; } = string.Empty;
        public string MaturityLevel { get; set; } = string.Empty;

        // Assessment status
        public string Status { get; set; } = "NotStarted"; // NotStarted, InProgress, Compliant, PartiallyCompliant, NonCompliant, NotApplicable
        public string EvidenceStatus { get; set; } = "Pending"; // Pending, Submitted, UnderReview, Approved, Rejected

        // ===== TOOLKIT & GUIDANCE =====
        public string ImplementationGuidance { get; set; } = string.Empty;
        public string ImplementationGuidanceAr { get; set; } = string.Empty;
        public string ToolkitReference { get; set; } = string.Empty; // Link to toolkit document/resource
        public string SampleEvidenceDescription { get; set; } = string.Empty;
        public string BestPractices { get; set; } = string.Empty;
        public string CommonGaps { get; set; } = string.Empty;

        // ===== SCORING GUIDE =====
        public string ScoringGuideJson { get; set; } = "[]"; // JSON: [{score: 0, label: "Not Implemented", criteria: "..."}, ...]
        public int WeightPercentage { get; set; } = 100; // Weight for weighted scoring
        public bool IsAutoScorable { get; set; } = false; // Can be auto-scored from evidence
        public string AutoScoreRuleJson { get; set; } = string.Empty; // Rule for auto-scoring

        // ===== SCORING =====
        public int? Score { get; set; }
        public int? MaxScore { get; set; } = 100;
        public string ScoreRationale { get; set; } = string.Empty;
        public bool IsAutoScored { get; set; } = false;
        public DateTime? ScoredAt { get; set; }
        public string ScoredBy { get; set; } = string.Empty;

        // Assignment
        public string OwnerRoleCode { get; set; } = string.Empty;
        public string ReviewerRoleCode { get; set; } = string.Empty;
        public Guid? AssignedToUserId { get; set; }
        public Guid? ReviewedByUserId { get; set; }

        // Dates
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public DateTime? ReviewedDate { get; set; }

        // Findings
        public string Findings { get; set; } = string.Empty;
        public string Recommendations { get; set; } = string.Empty;
        public string RemediationPlan { get; set; } = string.Empty;

        // Navigation
        public virtual Assessment Assessment { get; set; } = null!;

        /// <summary>
        /// Evidence files attached to this specific requirement
        /// </summary>
        public virtual ICollection<Evidence> Evidences { get; set; } = new List<Evidence>();

        /// <summary>
        /// Notes/comments on this requirement
        /// </summary>
        public virtual ICollection<RequirementNote> Notes { get; set; } = new List<RequirementNote>();
    }
}
