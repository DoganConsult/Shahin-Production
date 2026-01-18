using System;
using System.ComponentModel.DataAnnotations;

namespace GrcMvc.Models.Entities.Compliance
{
    /// <summary>
    /// Layer 34: Tenant's scope boundary.
    /// Defines what is in-scope and out-of-scope for compliance.
    /// Controls are filtered based on scope applicability.
    /// </summary>
    public class TenantScopeBoundary : BaseEntity
    {
        /// <summary>
        /// Scope item type: LEGAL_ENTITY, BUSINESS_UNIT, SYSTEM, PROCESS, LOCATION, DATA_CENTER
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string ScopeType { get; set; } = string.Empty;

        /// <summary>
        /// Unique code for this scope item (e.g., SYS_ERP_MAIN, LOC_RIYADH_DC1)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string ScopeCode { get; set; } = string.Empty;

        /// <summary>
        /// Name for display
        /// </summary>
        [MaxLength(255)]
        public string ScopeName { get; set; } = string.Empty;

        /// <summary>
        /// Arabic name
        /// </summary>
        [MaxLength(255)]
        public string? ScopeNameAr { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Is this item in-scope or excluded?
        /// </summary>
        public bool IsInScope { get; set; } = true;

        /// <summary>
        /// If excluded, rationale (required for audit)
        /// </summary>
        public string? ExclusionRationale { get; set; }

        /// <summary>
        /// Who approved the exclusion
        /// </summary>
        [MaxLength(100)]
        public string? ExclusionApprovedBy { get; set; }

        /// <summary>
        /// When exclusion was approved
        /// </summary>
        public DateTime? ExclusionApprovedAt { get; set; }

        /// <summary>
        /// Criticality tier: TIER_1 (critical), TIER_2 (important), TIER_3 (standard)
        /// </summary>
        [MaxLength(20)]
        public string CriticalityTier { get; set; } = "TIER_3";

        /// <summary>
        /// Parent scope item (for hierarchical scope)
        /// </summary>
        public Guid? ParentScopeId { get; set; }

        /// <summary>
        /// Which controls apply to this scope item (JSON array of control codes)
        /// </summary>
        public string ApplicableControlsJson { get; set; } = "[]";

        /// <summary>
        /// Tags for filtering (JSON array)
        /// </summary>
        public string TagsJson { get; set; } = "[]";

        /// <summary>
        /// Metadata JSON (system-specific attributes)
        /// </summary>
        public string MetadataJson { get; set; } = "{}";

        /// <summary>
        /// Owner team
        /// </summary>
        [MaxLength(100)]
        public string? OwnerTeam { get; set; }

        /// <summary>
        /// Owner user ID
        /// </summary>
        public Guid? OwnerUserId { get; set; }

        /// <summary>
        /// Environment: PRODUCTION, NON_PRODUCTION, BOTH
        /// </summary>
        [MaxLength(20)]
        public string Environment { get; set; } = "PRODUCTION";

        /// <summary>
        /// Location/region
        /// </summary>
        [MaxLength(100)]
        public string? Location { get; set; }

        /// <summary>
        /// When this scope item was added
        /// </summary>
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Is this scope item currently active?
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Navigation
        public virtual Tenant Tenant { get; set; } = null!;
        public virtual TenantScopeBoundary? ParentScope { get; set; }
    }

    /// <summary>
    /// Scope item types
    /// </summary>
    public static class ScopeTypes
    {
        public const string LegalEntity = "LEGAL_ENTITY";
        public const string BusinessUnit = "BUSINESS_UNIT";
        public const string System = "SYSTEM";
        public const string Application = "APPLICATION";
        public const string Process = "PROCESS";
        public const string Location = "LOCATION";
        public const string DataCenter = "DATA_CENTER";
        public const string CloudRegion = "CLOUD_REGION";
        public const string ThirdParty = "THIRD_PARTY";
    }

    /// <summary>
    /// Criticality tiers
    /// </summary>
    public static class CriticalityTiers
    {
        public const string Tier1 = "TIER_1";  // Critical - highest SLAs
        public const string Tier2 = "TIER_2";  // Important - elevated SLAs
        public const string Tier3 = "TIER_3";  // Standard - normal SLAs
    }
}
