using System;
using System.ComponentModel.DataAnnotations;

namespace GrcMvc.Models.Entities.Onboarding
{
    /// <summary>
    /// Layer 15: Derived outputs from onboarding wizard.
    /// Stores baselines, packages, templates, and other derived artifacts.
    /// Each output links to the rule evaluation that produced it.
    /// </summary>
    public class OnboardingDerivedOutput : BaseEntity
    {
        /// <summary>
        /// Reference to the OnboardingWizard
        /// </summary>
        public Guid OnboardingWizardId { get; set; }

        /// <summary>
        /// Type of derived output: BASELINE, OVERLAY, PACKAGE, TEMPLATE, CONTROL_SET, WORKFLOW_CONFIG
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string OutputType { get; set; } = string.Empty;

        /// <summary>
        /// Code identifier for the output (e.g., BL_SECURITY_BANKING, PKG_NCA_ECC)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string OutputCode { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable name
        /// </summary>
        [MaxLength(255)]
        public string OutputName { get; set; } = string.Empty;

        /// <summary>
        /// Arabic name
        /// </summary>
        [MaxLength(255)]
        public string? OutputNameAr { get; set; }

        /// <summary>
        /// JSON payload of the derived output
        /// </summary>
        public string OutputPayloadJson { get; set; } = "{}";

        /// <summary>
        /// Applicability: MANDATORY, RECOMMENDED, OPTIONAL
        /// </summary>
        [MaxLength(20)]
        public string Applicability { get; set; } = "MANDATORY";

        /// <summary>
        /// Priority for implementation (1 = highest)
        /// </summary>
        public int Priority { get; set; } = 1;

        /// <summary>
        /// Reference to the rule evaluation that produced this output
        /// </summary>
        public Guid? RulesEvaluationLogId { get; set; }

        /// <summary>
        /// Which step triggered this derivation (1-12)
        /// </summary>
        public int DerivedAtStep { get; set; }

        /// <summary>
        /// When this output was derived
        /// </summary>
        public DateTime DerivedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Is this output currently active?
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Version of this output (increments on re-derivation)
        /// </summary>
        public int Version { get; set; } = 1;

        // Navigation
        public virtual OnboardingWizard OnboardingWizard { get; set; } = null!;
        public virtual RulesEvaluationLog? RulesEvaluationLog { get; set; }
    }

    /// <summary>
    /// Types of derived outputs
    /// </summary>
    public static class DerivedOutputTypes
    {
        public const string Baseline = "BASELINE";
        public const string Overlay = "OVERLAY";
        public const string Package = "PACKAGE";
        public const string Template = "TEMPLATE";
        public const string ControlSet = "CONTROL_SET";
        public const string WorkflowConfig = "WORKFLOW_CONFIG";
        public const string EvidenceContract = "EVIDENCE_CONTRACT";
        public const string SlaConfig = "SLA_CONFIG";
        public const string TeamAssignment = "TEAM_ASSIGNMENT";
    }
}
