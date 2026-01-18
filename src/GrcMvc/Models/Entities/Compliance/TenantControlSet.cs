using System;
using System.ComponentModel.DataAnnotations;

namespace GrcMvc.Models.Entities.Compliance
{
    /// <summary>
    /// Layer 33: Tenant's resolved control set.
    /// The final list of controls applicable to this tenant after applying baselines and overlays.
    /// This is the "work units" the tenant must implement/assess.
    /// </summary>
    public class TenantControlSet : BaseEntity
    {
        /// <summary>
        /// Reference to the control from catalog (Control.Id or Control.Code)
        /// </summary>
        public Guid? CatalogControlId { get; set; }

        /// <summary>
        /// Control code (e.g., NCA-ECC-1-1, SAMA-CSF-AC-01)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string ControlCode { get; set; } = string.Empty;

        /// <summary>
        /// Control name for display
        /// </summary>
        [MaxLength(500)]
        public string ControlName { get; set; } = string.Empty;

        /// <summary>
        /// Arabic name
        /// </summary>
        [MaxLength(500)]
        public string? ControlNameAr { get; set; }

        /// <summary>
        /// Which framework this control belongs to
        /// </summary>
        [MaxLength(50)]
        public string FrameworkCode { get; set; } = string.Empty;

        /// <summary>
        /// Control domain (e.g., Access Control, Cryptography)
        /// </summary>
        [MaxLength(100)]
        public string? ControlDomain { get; set; }

        /// <summary>
        /// Control family/category
        /// </summary>
        [MaxLength(100)]
        public string? ControlFamily { get; set; }

        /// <summary>
        /// Source: BASELINE (from baseline), OVERLAY (from overlay), CUSTOM (client-specific)
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Source { get; set; } = "BASELINE";

        /// <summary>
        /// Reference to which baseline/overlay added this control
        /// </summary>
        [MaxLength(100)]
        public string? SourceCode { get; set; }

        /// <summary>
        /// Applicability status: APPLICABLE, NOT_APPLICABLE, COMPENSATING, INHERITED
        /// </summary>
        [MaxLength(20)]
        public string ApplicabilityStatus { get; set; } = "APPLICABLE";

        /// <summary>
        /// If not applicable, reason
        /// </summary>
        public string? ApplicabilityReason { get; set; }

        /// <summary>
        /// Is this control mandatory?
        /// </summary>
        public bool IsMandatory { get; set; } = true;

        /// <summary>
        /// Priority for implementation (1 = highest)
        /// </summary>
        public int Priority { get; set; } = 5;

        /// <summary>
        /// Assigned owner team
        /// </summary>
        [MaxLength(100)]
        public string? OwnerTeam { get; set; }

        /// <summary>
        /// Assigned owner user ID
        /// </summary>
        public Guid? OwnerUserId { get; set; }

        /// <summary>
        /// Evidence collection frequency: CONTINUOUS, MONTHLY, QUARTERLY, ANNUAL
        /// </summary>
        [MaxLength(20)]
        public string EvidenceFrequency { get; set; } = "QUARTERLY";

        /// <summary>
        /// Evidence types required (JSON array)
        /// </summary>
        public string EvidenceTypesJson { get; set; } = "[]";

        /// <summary>
        /// Current implementation status: NOT_STARTED, IN_PROGRESS, IMPLEMENTED, NOT_IMPLEMENTED
        /// </summary>
        [MaxLength(20)]
        public string ImplementationStatus { get; set; } = "NOT_STARTED";

        /// <summary>
        /// Current compliance status: NOT_ASSESSED, COMPLIANT, PARTIALLY_COMPLIANT, NON_COMPLIANT
        /// </summary>
        [MaxLength(25)]
        public string ComplianceStatus { get; set; } = "NOT_ASSESSED";

        /// <summary>
        /// Last assessment date
        /// </summary>
        public DateTime? LastAssessedAt { get; set; }

        /// <summary>
        /// Next assessment due date
        /// </summary>
        public DateTime? NextAssessmentDue { get; set; }

        /// <summary>
        /// When this control was added to tenant's set
        /// </summary>
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Is this control currently active?
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Reference to explainability payload for why this control applies
        /// </summary>
        public Guid? ExplainabilityPayloadId { get; set; }

        // Navigation
        public virtual Tenant Tenant { get; set; } = null!;
        public virtual Control? CatalogControl { get; set; }
    }

    /// <summary>
    /// Control applicability statuses
    /// </summary>
    public static class ControlApplicabilityStatuses
    {
        public const string Applicable = "APPLICABLE";
        public const string NotApplicable = "NOT_APPLICABLE";
        public const string Compensating = "COMPENSATING";
        public const string Inherited = "INHERITED";
    }

    /// <summary>
    /// Control sources
    /// </summary>
    public static class ControlSources
    {
        public const string Baseline = "BASELINE";
        public const string Overlay = "OVERLAY";
        public const string Custom = "CUSTOM";
        public const string Regulator = "REGULATOR";
    }
}
