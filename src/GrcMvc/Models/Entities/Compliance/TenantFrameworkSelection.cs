using System;
using System.ComponentModel.DataAnnotations;

namespace GrcMvc.Models.Entities.Compliance
{
    /// <summary>
    /// Layer 31: Tenant's framework selections.
    /// Records which frameworks a tenant must comply with and why.
    /// </summary>
    public class TenantFrameworkSelection : BaseEntity
    {
        /// <summary>
        /// Reference to the framework from catalog (LookupFramework.Code)
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string FrameworkCode { get; set; } = string.Empty;

        /// <summary>
        /// Framework name for display
        /// </summary>
        [MaxLength(255)]
        public string FrameworkName { get; set; } = string.Empty;

        /// <summary>
        /// Arabic name
        /// </summary>
        [MaxLength(255)]
        public string? FrameworkNameAr { get; set; }

        /// <summary>
        /// Version of framework being used
        /// </summary>
        [MaxLength(20)]
        public string? FrameworkVersion { get; set; }

        /// <summary>
        /// Selection type: MANDATORY (derived from rules), VOLUNTARY (user chose)
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string SelectionType { get; set; } = "MANDATORY";

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
        /// Reference to the rules evaluation that selected this framework
        /// </summary>
        public Guid? RulesEvaluationLogId { get; set; }

        /// <summary>
        /// Reference to explainability payload
        /// </summary>
        public Guid? ExplainabilityPayloadId { get; set; }

        /// <summary>
        /// Reason for selection (human-readable)
        /// </summary>
        public string SelectionReason { get; set; } = string.Empty;

        /// <summary>
        /// Which regulator mandates this (if applicable)
        /// </summary>
        [MaxLength(50)]
        public string? RegulatorCode { get; set; }

        /// <summary>
        /// Estimated number of controls in this framework
        /// </summary>
        public int EstimatedControlCount { get; set; } = 0;

        /// <summary>
        /// Estimated implementation months
        /// </summary>
        public int EstimatedImplementationMonths { get; set; } = 0;

        /// <summary>
        /// Compliance deadline (if known)
        /// </summary>
        public DateTime? ComplianceDeadline { get; set; }

        /// <summary>
        /// Current compliance status: NOT_STARTED, IN_PROGRESS, COMPLIANT, NON_COMPLIANT
        /// </summary>
        [MaxLength(20)]
        public string ComplianceStatus { get; set; } = "NOT_STARTED";

        /// <summary>
        /// Compliance score (0-100%)
        /// </summary>
        public decimal ComplianceScore { get; set; } = 0;

        /// <summary>
        /// When this selection was made
        /// </summary>
        public DateTime SelectedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Is this selection currently active?
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// If deactivated, when
        /// </summary>
        public DateTime? DeactivatedAt { get; set; }

        /// <summary>
        /// If deactivated, reason
        /// </summary>
        public string? DeactivationReason { get; set; }

        // Navigation
        public virtual Tenant Tenant { get; set; } = null!;
    }

    /// <summary>
    /// Framework selection types
    /// </summary>
    public static class FrameworkSelectionTypes
    {
        public const string Mandatory = "MANDATORY";
        public const string Voluntary = "VOLUNTARY";
        public const string Recommended = "RECOMMENDED";
    }
}
