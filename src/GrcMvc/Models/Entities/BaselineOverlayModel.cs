using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities;

#region Baseline Controls

/// <summary>
/// Baseline Control Set - Universal controls that apply to almost everyone
/// These are the foundation controls regardless of sector/jurisdiction
/// </summary>
public class BaselineControlSet : BaseEntity
{
    [Required]
    [StringLength(30)]
    public string BaselineCode { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [StringLength(255)]
    public string? NameAr { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Baseline type: Core, Extended, Minimal
    /// </summary>
    [StringLength(20)]
    public string BaselineType { get; set; } = "Core";

    /// <summary>
    /// Version for change tracking
    /// </summary>
    [StringLength(20)]
    public string Version { get; set; } = "1.0";

    public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    // Navigation
    public virtual ICollection<BaselineControlMapping> ControlMappings { get; set; } = new List<BaselineControlMapping>();
}

/// <summary>
/// Links Baseline to Canonical Controls
/// </summary>
public class BaselineControlMapping : BaseEntity
{
    public Guid BaselineSetId { get; set; }

    [ForeignKey("BaselineSetId")]
    public virtual BaselineControlSet BaselineSet { get; set; } = null!;

    public Guid ControlId { get; set; }

    [ForeignKey("ControlId")]
    public virtual CanonicalControl Control { get; set; } = null!;

    /// <summary>
    /// Is this control mandatory in the baseline?
    /// </summary>
    public bool IsMandatory { get; set; } = true;

    /// <summary>
    /// Default parameters for this control in baseline
    /// </summary>
    public string? DefaultParametersJson { get; set; }

    public int DisplayOrder { get; set; }
}

#endregion

#region Overlays

/// <summary>
/// Overlay Catalog - Conditional add-ons based on sector, jurisdiction, data type, or technology
/// Overlays ADD, TIGHTEN, or PARAMETERIZE the baseline - they don't replace it
/// </summary>
public class OverlayCatalog : BaseEntity
{
    [Required]
    [StringLength(30)]
    public string OverlayCode { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [StringLength(255)]
    public string? NameAr { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Overlay type: Sector, Jurisdiction, DataType, Technology
    /// </summary>
    [Required]
    [StringLength(30)]
    public string OverlayType { get; set; } = string.Empty;

    /// <summary>
    /// Specific value this overlay applies to (e.g., "Banking", "KSA", "PCI", "Cloud")
    /// </summary>
    [Required]
    [StringLength(100)]
    public string AppliesTo { get; set; } = string.Empty;

    /// <summary>
    /// Priority when multiple overlays apply (lower = higher priority)
    /// </summary>
    public int Priority { get; set; } = 100;

    /// <summary>
    /// Version for change tracking
    /// </summary>
    [StringLength(20)]
    public string Version { get; set; } = "1.0";

    public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    // Navigation
    public virtual ICollection<OverlayControlMapping> ControlMappings { get; set; } = new List<OverlayControlMapping>();
    public virtual ICollection<OverlayParameterOverride> ParameterOverrides { get; set; } = new List<OverlayParameterOverride>();
}

/// <summary>
/// Links Overlay to additional controls (beyond baseline)
/// </summary>
public class OverlayControlMapping : BaseEntity
{
    public Guid OverlayId { get; set; }

    [ForeignKey("OverlayId")]
    public virtual OverlayCatalog Overlay { get; set; } = null!;

    public Guid ControlId { get; set; }

    [ForeignKey("ControlId")]
    public virtual CanonicalControl Control { get; set; } = null!;

    /// <summary>
    /// Action: Add, Tighten, Remove
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Action { get; set; } = "Add";

    /// <summary>
    /// Is this control mandatory when overlay applies?
    /// </summary>
    public bool IsMandatory { get; set; } = true;

    /// <summary>
    /// Reason why this control is added by this overlay
    /// </summary>
    [StringLength(500)]
    public string? Reason { get; set; }

    public int DisplayOrder { get; set; }
}

/// <summary>
/// Parameter overrides when an overlay applies (e.g., longer retention, higher frequency)
/// </summary>
public class OverlayParameterOverride : BaseEntity
{
    public Guid OverlayId { get; set; }

    [ForeignKey("OverlayId")]
    public virtual OverlayCatalog Overlay { get; set; } = null!;

    public Guid? ControlId { get; set; }

    [ForeignKey("ControlId")]
    public virtual CanonicalControl? Control { get; set; }

    /// <summary>
    /// Parameter name being overridden
    /// </summary>
    [Required]
    [StringLength(100)]
    public string ParameterName { get; set; } = string.Empty;

    /// <summary>
    /// Original value (from baseline)
    /// </summary>
    [StringLength(255)]
    public string? OriginalValue { get; set; }

    /// <summary>
    /// Override value (from overlay)
    /// </summary>
    [Required]
    [StringLength(255)]
    public string OverrideValue { get; set; } = string.Empty;

    /// <summary>
    /// Reason for override
    /// </summary>
    [StringLength(500)]
    public string? Reason { get; set; }
}

#endregion

#region Rules Catalog

/// <summary>
/// Rules Catalog - Maps onboarding answers to overlays/parameters
/// This is what enables "suite generation" automatically
/// </summary>
public class ApplicabilityRuleCatalog : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string RuleCode { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Rule category: SectorRule, JurisdictionRule, DataRule, TechnologyRule, RiskRule
    /// </summary>
    [Required]
    [StringLength(30)]
    public string RuleCategory { get; set; } = string.Empty;

    /// <summary>
    /// Condition expression (simplified DSL or JSON)
    /// e.g., "sector = 'Banking'" or "dataTypes contains 'PCI'"
    /// </summary>
    [Required]
    public string ConditionExpression { get; set; } = string.Empty;

    /// <summary>
    /// Action when condition is true: ApplyOverlay, ApplyControl, SetParameter, ApplyEvidencePack
    /// </summary>
    [Required]
    [StringLength(30)]
    public string ActionType { get; set; } = "ApplyOverlay";

    /// <summary>
    /// Target of the action (overlay code, control ID, parameter name)
    /// </summary>
    [Required]
    [StringLength(100)]
    public string ActionTarget { get; set; } = string.Empty;

    /// <summary>
    /// Additional action parameters (JSON)
    /// </summary>
    public string? ActionParametersJson { get; set; }

    /// <summary>
    /// Priority for rule evaluation (lower = evaluated first)
    /// </summary>
    public int Priority { get; set; } = 100;

    /// <summary>
    /// Is this a blocking rule (must be satisfied)?
    /// </summary>
    public bool IsBlocking { get; set; } = false;

    /// <summary>
    /// Version for change tracking
    /// </summary>
    [StringLength(20)]
    public string Version { get; set; } = "1.0";

    public bool IsActive { get; set; } = true;
}

#endregion

#region Generated Control Suite

/// <summary>
/// Generated Control Suite - The output of applying baseline + overlays to an org profile
/// This is what the organization actually needs to implement
/// </summary>
public class GeneratedControlSuite : BaseEntity
{
    public Guid TenantId { get; set; }

    [ForeignKey("TenantId")]
    public virtual Tenant Tenant { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string SuiteCode { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The baseline set used as foundation
    /// </summary>
    public Guid BaselineSetId { get; set; }

    [ForeignKey("BaselineSetId")]
    public virtual BaselineControlSet BaselineSet { get; set; } = null!;

    /// <summary>
    /// Applied overlays (comma-separated codes or JSON)
    /// </summary>
    public string? AppliedOverlaysJson { get; set; }

    /// <summary>
    /// Total controls in suite
    /// </summary>
    public int TotalControls { get; set; }

    /// <summary>
    /// Mandatory controls count
    /// </summary>
    public int MandatoryControls { get; set; }

    /// <summary>
    /// Optional controls count
    /// </summary>
    public int OptionalControls { get; set; }

    /// <summary>
    /// Generation timestamp
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Generated by (user or system)
    /// </summary>
    [StringLength(100)]
    public string? GeneratedBy { get; set; }

    /// <summary>
    /// Version of the suite
    /// </summary>
    [StringLength(20)]
    public string Version { get; set; } = "1.0";

    /// <summary>
    /// Onboarding profile snapshot used for generation (JSON)
    /// </summary>
    public string? ProfileSnapshotJson { get; set; }

    /// <summary>
    /// Rules execution log (which rules fired)
    /// </summary>
    public string? RulesExecutionLogJson { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation
    public virtual ICollection<SuiteControlEntry> ControlEntries { get; set; } = new List<SuiteControlEntry>();
    public virtual ICollection<SuiteEvidenceRequest> EvidenceRequests { get; set; } = new List<SuiteEvidenceRequest>();
}

/// <summary>
/// Individual control entry in a generated suite
/// </summary>
public class SuiteControlEntry : BaseEntity
{
    public Guid SuiteId { get; set; }

    [ForeignKey("SuiteId")]
    public virtual GeneratedControlSuite Suite { get; set; } = null!;

    public Guid ControlId { get; set; }

    [ForeignKey("ControlId")]
    public virtual CanonicalControl Control { get; set; } = null!;

    /// <summary>
    /// Source: Baseline, Overlay, Manual
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Source { get; set; } = "Baseline";

    /// <summary>
    /// If from overlay, which one
    /// </summary>
    [StringLength(30)]
    public string? SourceOverlayCode { get; set; }

    /// <summary>
    /// Is mandatory in this suite?
    /// </summary>
    public bool IsMandatory { get; set; } = true;

    /// <summary>
    /// Applied parameters (may differ from baseline due to overlays)
    /// </summary>
    public string? AppliedParametersJson { get; set; }

    /// <summary>
    /// Reason for inclusion
    /// </summary>
    [StringLength(500)]
    public string? InclusionReason { get; set; }

    /// <summary>
    /// Assigned owner (role code)
    /// </summary>
    [StringLength(50)]
    public string? AssignedOwnerRoleCode { get; set; }

    public int DisplayOrder { get; set; }
}

/// <summary>
/// Evidence request generated for a suite
/// </summary>
public class SuiteEvidenceRequest : BaseEntity
{
    public Guid SuiteId { get; set; }

    [ForeignKey("SuiteId")]
    public virtual GeneratedControlSuite Suite { get; set; } = null!;

    public Guid ControlId { get; set; }

    [ForeignKey("ControlId")]
    public virtual CanonicalControl Control { get; set; } = null!;

    public Guid? EvidencePackId { get; set; }

    [ForeignKey("EvidencePackId")]
    public virtual EvidencePack? EvidencePack { get; set; }

    /// <summary>
    /// Evidence item code
    /// </summary>
    [Required]
    [StringLength(50)]
    public string EvidenceItemCode { get; set; } = string.Empty;

    /// <summary>
    /// Evidence item name
    /// </summary>
    [Required]
    [StringLength(255)]
    public string EvidenceItemName { get; set; } = string.Empty;

    /// <summary>
    /// Required frequency (may be overridden by overlay)
    /// </summary>
    [StringLength(20)]
    public string RequiredFrequency { get; set; } = "Quarterly";

    /// <summary>
    /// Retention period in months (may be overridden by overlay)
    /// </summary>
    public int RetentionMonths { get; set; } = 84;

    /// <summary>
    /// Assigned owner
    /// </summary>
    [StringLength(100)]
    public string? AssignedOwnerId { get; set; }

    [StringLength(255)]
    public string? AssignedOwnerName { get; set; }

    /// <summary>
    /// Status: NotStarted, InProgress, Collected, Reviewed, Approved
    /// </summary>
    [StringLength(20)]
    public string Status { get; set; } = "NotStarted";

    public DateTime? DueDate { get; set; }
}

#endregion

#region Multi-Entity Support

/// <summary>
/// Organization Entity - For multi-sector organizations with different business units
/// Each entity can have different overlays applied
/// </summary>
public class OrganizationEntity : BaseEntity
{
    public Guid TenantId { get; set; }

    [ForeignKey("TenantId")]
    public virtual Tenant Tenant { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string EntityCode { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [StringLength(255)]
    public string? NameAr { get; set; }

    /// <summary>
    /// Entity type: LegalEntity, BusinessUnit, Subsidiary, Branch, Department
    /// </summary>
    [Required]
    [StringLength(30)]
    public string EntityType { get; set; } = "BusinessUnit";

    /// <summary>
    /// Parent entity (for hierarchy)
    /// </summary>
    public Guid? ParentEntityId { get; set; }

    [ForeignKey("ParentEntityId")]
    public virtual OrganizationEntity? ParentEntity { get; set; }

    /// <summary>
    /// Sectors this entity operates in (comma-separated)
    /// </summary>
    [StringLength(255)]
    public string? Sectors { get; set; }

    /// <summary>
    /// Jurisdictions this entity operates in (comma-separated)
    /// </summary>
    [StringLength(255)]
    public string? Jurisdictions { get; set; }

    /// <summary>
    /// Data types this entity handles (comma-separated)
    /// </summary>
    [StringLength(255)]
    public string? DataTypes { get; set; }

    /// <summary>
    /// Technology profile (Cloud, OnPrem, Hybrid, OT/ICS)
    /// </summary>
    [StringLength(100)]
    public string? TechnologyProfile { get; set; }

    /// <summary>
    /// Criticality tier: Tier1, Tier2, Tier3
    /// </summary>
    [StringLength(10)]
    public string CriticalityTier { get; set; } = "Tier2";

    /// <summary>
    /// Does this entity inherit controls from parent?
    /// </summary>
    public bool InheritsFromParent { get; set; } = true;

    /// <summary>
    /// Specific overlays applied to this entity (comma-separated codes)
    /// </summary>
    [StringLength(500)]
    public string? AppliedOverlays { get; set; }

    /// <summary>
    /// Generated suite for this entity
    /// </summary>
    public Guid? GeneratedSuiteId { get; set; }

    [ForeignKey("GeneratedSuiteId")]
    public virtual GeneratedControlSuite? GeneratedSuite { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation
    public virtual ICollection<OrganizationEntity> ChildEntities { get; set; } = new List<OrganizationEntity>();
}

#endregion
