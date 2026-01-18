using System;
using System.ComponentModel.DataAnnotations;

namespace GrcMvc.Models.Entities.Compliance
{
    /// <summary>
    /// Layer 32: Tenant overlay selections.
    /// Overlays add additional controls based on jurisdiction, sector, data types, or technology.
    /// </summary>
    public class TenantOverlay : BaseEntity
    {
        /// <summary>
        /// Overlay type: JURISDICTION, SECTOR, DATA_TYPE, TECHNOLOGY, CUSTOM
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string OverlayType { get; set; } = string.Empty;

        /// <summary>
        /// Overlay code (e.g., OVL_KSA_BANKING, OVL_PCI, OVL_CLOUD)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string OverlayCode { get; set; } = string.Empty;

        /// <summary>
        /// Overlay name for display
        /// </summary>
        [MaxLength(255)]
        public string OverlayName { get; set; } = string.Empty;

        /// <summary>
        /// Arabic name
        /// </summary>
        [MaxLength(255)]
        public string? OverlayNameAr { get; set; }

        /// <summary>
        /// Selection type: AUTO (derived from rules), MANUAL (user chose)
        /// </summary>
        [MaxLength(20)]
        public string SelectionType { get; set; } = "AUTO";

        /// <summary>
        /// Reference to the rules evaluation that selected this overlay
        /// </summary>
        public Guid? RulesEvaluationLogId { get; set; }

        /// <summary>
        /// Reason for overlay application
        /// </summary>
        public string ApplicationReason { get; set; } = string.Empty;

        /// <summary>
        /// Which input triggered this overlay (e.g., "CountryOfIncorporation=SA")
        /// </summary>
        public string TriggerCondition { get; set; } = string.Empty;

        /// <summary>
        /// Additional controls added by this overlay (JSON array of control IDs/codes)
        /// </summary>
        public string AdditionalControlsJson { get; set; } = "[]";

        /// <summary>
        /// Number of additional controls
        /// </summary>
        public int AdditionalControlCount { get; set; } = 0;

        /// <summary>
        /// Modified controls (JSON array of {controlId, modification})
        /// </summary>
        public string ModifiedControlsJson { get; set; } = "[]";

        /// <summary>
        /// Priority (lower = applied first)
        /// </summary>
        public int Priority { get; set; } = 10;

        /// <summary>
        /// When this overlay was applied
        /// </summary>
        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Is this overlay currently active?
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Navigation
        public virtual Tenant Tenant { get; set; } = null!;
    }

    /// <summary>
    /// Overlay types
    /// </summary>
    public static class OverlayTypes
    {
        public const string Jurisdiction = "JURISDICTION";
        public const string Sector = "SECTOR";
        public const string DataType = "DATA_TYPE";
        public const string Technology = "TECHNOLOGY";
        public const string Regulator = "REGULATOR";
        public const string Custom = "CUSTOM";
    }
}
