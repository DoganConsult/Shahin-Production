using System;
using System.ComponentModel.DataAnnotations;

namespace GrcMvc.Models.Entities.Onboarding
{
    /// <summary>
    /// Layer 17: Human-readable explanation payloads.
    /// Answers "Why was this framework/control/baseline selected?"
    /// Used for audit defense and user understanding.
    /// </summary>
    public class ExplainabilityPayload : BaseEntity
    {
        /// <summary>
        /// What type of decision is being explained:
        /// FRAMEWORK_SELECTION, CONTROL_APPLICABILITY, BASELINE_DERIVATION,
        /// OVERLAY_APPLICATION, EVIDENCE_REQUIREMENT, SLA_ASSIGNMENT
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string DecisionType { get; set; } = string.Empty;

        /// <summary>
        /// The entity being explained (e.g., framework code, control ID)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string SubjectCode { get; set; } = string.Empty;

        /// <summary>
        /// Subject name for display
        /// </summary>
        [MaxLength(255)]
        public string SubjectName { get; set; } = string.Empty;

        /// <summary>
        /// Reference to the OnboardingWizard (optional - for onboarding decisions)
        /// </summary>
        public Guid? OnboardingWizardId { get; set; }

        /// <summary>
        /// Reference to the rules evaluation that produced this explanation
        /// </summary>
        public Guid? RulesEvaluationLogId { get; set; }

        /// <summary>
        /// The decision made: INCLUDED, EXCLUDED, MANDATORY, RECOMMENDED, OPTIONAL
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Decision { get; set; } = string.Empty;

        /// <summary>
        /// Primary reason for the decision (simple text)
        /// </summary>
        [Required]
        public string PrimaryReason { get; set; } = string.Empty;

        /// <summary>
        /// Primary reason in Arabic
        /// </summary>
        public string? PrimaryReasonAr { get; set; }

        /// <summary>
        /// Detailed explanation JSON with structured reasoning
        /// { "factors": [...], "inputs": {...}, "logic": "...", "confidence": 0.95 }
        /// </summary>
        public string DetailedExplanationJson { get; set; } = "{}";

        /// <summary>
        /// Supporting evidence/citations (e.g., regulatory references)
        /// </summary>
        public string SupportingReferencesJson { get; set; } = "[]";

        /// <summary>
        /// What inputs led to this decision (field names and values)
        /// </summary>
        public string InputFactorsJson { get; set; } = "{}";

        /// <summary>
        /// If the decision can be overridden by user
        /// </summary>
        public bool IsOverridable { get; set; } = false;

        /// <summary>
        /// If overridden, who overrode it
        /// </summary>
        [MaxLength(100)]
        public string? OverriddenByUserId { get; set; }

        /// <summary>
        /// If overridden, when
        /// </summary>
        public DateTime? OverriddenAt { get; set; }

        /// <summary>
        /// If overridden, the new decision
        /// </summary>
        [MaxLength(20)]
        public string? OverrideDecision { get; set; }

        /// <summary>
        /// If overridden, justification provided
        /// </summary>
        public string? OverrideJustification { get; set; }

        /// <summary>
        /// When this explanation was generated
        /// </summary>
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public virtual OnboardingWizard? OnboardingWizard { get; set; }
        public virtual RulesEvaluationLog? RulesEvaluationLog { get; set; }
    }

    /// <summary>
    /// Decision types for explainability
    /// </summary>
    public static class ExplainabilityDecisionTypes
    {
        public const string FrameworkSelection = "FRAMEWORK_SELECTION";
        public const string ControlApplicability = "CONTROL_APPLICABILITY";
        public const string BaselineDerivation = "BASELINE_DERIVATION";
        public const string OverlayApplication = "OVERLAY_APPLICATION";
        public const string EvidenceRequirement = "EVIDENCE_REQUIREMENT";
        public const string SlaAssignment = "SLA_ASSIGNMENT";
        public const string TeamAssignment = "TEAM_ASSIGNMENT";
        public const string WorkflowRouting = "WORKFLOW_ROUTING";
    }
}
