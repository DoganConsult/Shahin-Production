using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities;

#region Control Domain & Objectives

/// <summary>
/// Control Domain - Top-level grouping (e.g., Access Control, Cryptography, Incident Response)
/// Approximately 12-20 domains
/// </summary>
public class ControlDomain : BaseEntity
{
    [Required]
    [StringLength(20)]
    public string DomainCode { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [StringLength(255)]
    public string? NameAr { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    [StringLength(1000)]
    public string? DescriptionAr { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation
    public virtual ICollection<ControlObjective> Objectives { get; set; } = new List<ControlObjective>();
}

/// <summary>
/// Control Objective - What must be achieved (e.g., "Ensure only authorized users access systems")
/// Approximately 60-120 objectives across all domains
/// </summary>
public class ControlObjective : BaseEntity
{
    [Required]
    [StringLength(30)]
    public string ObjectiveCode { get; set; } = string.Empty;

    public Guid DomainId { get; set; }

    [ForeignKey("DomainId")]
    public virtual ControlDomain Domain { get; set; } = null!;

    [Required]
    [StringLength(500)]
    public string ObjectiveStatement { get; set; } = string.Empty;

    [StringLength(500)]
    public string? ObjectiveStatementAr { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation
    public virtual ICollection<CanonicalControl> Controls { get; set; } = new List<CanonicalControl>();
    public virtual ICollection<RequirementMapping> RequirementMappings { get; set; } = new List<RequirementMapping>();
}

#endregion

#region Canonical Control Library

/// <summary>
/// Canonical Control - The unified, deduplicated control that all frameworks map to
/// This is the "One Map for All" - a single control can satisfy multiple framework requirements
/// </summary>
public class CanonicalControl : BaseEntity
{
    [Required]
    [StringLength(30)]
    public string ControlId { get; set; } = string.Empty; // Stable, never reused (e.g., CCL-AC-001)

    public Guid ObjectiveId { get; set; }

    [ForeignKey("ObjectiveId")]
    public virtual ControlObjective Objective { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string ControlName { get; set; } = string.Empty;

    [StringLength(255)]
    public string? ControlNameAr { get; set; }

    /// <summary>
    /// Control statement - Must/shall wording; testable
    /// </summary>
    [Required]
    [StringLength(2000)]
    public string ControlStatement { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? ControlStatementAr { get; set; }

    /// <summary>
    /// Control type: Preventive, Detective, Corrective
    /// </summary>
    [Required]
    [StringLength(20)]
    public string ControlType { get; set; } = "Preventive";

    /// <summary>
    /// Nature: Manual, Automated, Hybrid
    /// </summary>
    [Required]
    [StringLength(20)]
    public string ControlNature { get; set; } = "Manual";

    /// <summary>
    /// Frequency: EventDriven, Daily, Weekly, Monthly, Quarterly, Annual, Continuous
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Frequency { get; set; } = "Continuous";

    /// <summary>
    /// Risk rating: Critical, High, Medium, Low
    /// </summary>
    [StringLength(20)]
    public string RiskRating { get; set; } = "Medium";

    /// <summary>
    /// Implementation guidance
    /// </summary>
    [StringLength(4000)]
    public string? ImplementationGuidance { get; set; }

    /// <summary>
    /// Version for change tracking
    /// </summary>
    [StringLength(20)]
    public string Version { get; set; } = "1.0";

    public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;

    public DateTime? SunsetDate { get; set; }

    public bool IsActive { get; set; } = true;

    // Applicability parameters (JSON for flexibility)
    public string? ApplicabilityJson { get; set; }

    // Navigation
    public virtual ICollection<RequirementMapping> RequirementMappings { get; set; } = new List<RequirementMapping>();
    public virtual ICollection<ControlEvidencePack> EvidencePacks { get; set; } = new List<ControlEvidencePack>();
    public virtual ICollection<ControlTestProcedure> TestProcedures { get; set; } = new List<ControlTestProcedure>();
}

#endregion

#region Regulatory Requirements

/// <summary>
/// Regulatory Requirement - Atomic statement from a law, regulation, or standard
/// This is the source requirement from 120+ regulators and 240+ frameworks
/// </summary>
public class RegulatoryRequirement : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string RequirementCode { get; set; } = string.Empty; // e.g., NCA-ECC-1-1-1

    [Required]
    [StringLength(50)]
    public string RegulatorCode { get; set; } = string.Empty; // e.g., NCA, SAMA, PDPL

    [Required]
    [StringLength(50)]
    public string FrameworkCode { get; set; } = string.Empty; // e.g., ECC-2024, SAMA-CSF-2.0

    [StringLength(255)]
    public string? FrameworkVersion { get; set; }

    [StringLength(100)]
    public string? Section { get; set; } // e.g., "1.1.1" or "Article 5"

    [Required]
    [StringLength(2000)]
    public string RequirementText { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? RequirementTextAr { get; set; }

    /// <summary>
    /// Requirement type: Mandatory, Recommended, Optional
    /// </summary>
    [StringLength(20)]
    public string RequirementType { get; set; } = "Mandatory";

    /// <summary>
    /// Jurisdiction codes (comma-separated): KSA, GCC, EU, US, INTL
    /// </summary>
    [StringLength(100)]
    public string? Jurisdictions { get; set; }

    /// <summary>
    /// Industry applicability (comma-separated): Banking, Healthcare, Government, All
    /// </summary>
    [StringLength(255)]
    public string? Industries { get; set; }

    /// <summary>
    /// Data types applicable (comma-separated): PII, PCI, PHI, Classified
    /// </summary>
    [StringLength(255)]
    public string? DataTypes { get; set; }

    public DateTime EffectiveDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation
    public virtual ICollection<RequirementMapping> ControlMappings { get; set; } = new List<RequirementMapping>();
}

/// <summary>
/// Mapping between Regulatory Requirement and Canonical Control
/// Many-to-many with confidence level and rationale
/// </summary>
public class RequirementMapping : BaseEntity
{
    public Guid RequirementId { get; set; }

    [ForeignKey("RequirementId")]
    public virtual RegulatoryRequirement Requirement { get; set; } = null!;

    public Guid ControlId { get; set; }

    [ForeignKey("ControlId")]
    public virtual CanonicalControl Control { get; set; } = null!;

    public Guid? ObjectiveId { get; set; }

    [ForeignKey("ObjectiveId")]
    public virtual ControlObjective? Objective { get; set; }

    /// <summary>
    /// Mapping confidence: Full, Partial, Related
    /// </summary>
    [StringLength(20)]
    public string MappingType { get; set; } = "Full";

    /// <summary>
    /// Confidence percentage (0-100)
    /// </summary>
    public int ConfidenceLevel { get; set; } = 100;

    /// <summary>
    /// Rationale for the mapping
    /// </summary>
    [StringLength(1000)]
    public string? Rationale { get; set; }

    /// <summary>
    /// Who approved this mapping
    /// </summary>
    [StringLength(100)]
    public string? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public bool IsActive { get; set; } = true;
}

#endregion

#region Evidence Catalog

/// <summary>
/// Evidence Pack - Standard evidence package required for a control
/// One evidence pack can be reused across multiple controls
/// </summary>
public class EvidencePack : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string PackCode { get; set; } = string.Empty; // e.g., EVP-ACCESS-REVIEW

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [StringLength(255)]
    public string? NameAr { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    [StringLength(1000)]
    public string? DescriptionAr { get; set; }

    /// <summary>
    /// Evidence items in this pack (JSON array)
    /// </summary>
    public string? EvidenceItemsJson { get; set; }

    /// <summary>
    /// Evidence items in Arabic (JSON array)
    /// </summary>
    public string? EvidenceItemsArJson { get; set; }

    /// <summary>
    /// Required frequency: Continuous, Daily, Weekly, Monthly, Quarterly, Annual
    /// </summary>
    [StringLength(20)]
    public string RequiredFrequency { get; set; } = "Quarterly";

    /// <summary>
    /// Retention period in months
    /// </summary>
    public int RetentionMonths { get; set; } = 84; // 7 years default

    public bool IsActive { get; set; } = true;

    // Navigation
    public virtual ICollection<ControlEvidencePack> ControlMappings { get; set; } = new List<ControlEvidencePack>();
}

/// <summary>
/// Link between Control and Evidence Pack
/// </summary>
public class ControlEvidencePack : BaseEntity
{
    public Guid ControlId { get; set; }

    [ForeignKey("ControlId")]
    public virtual CanonicalControl Control { get; set; } = null!;

    public Guid EvidencePackId { get; set; }

    [ForeignKey("EvidencePackId")]
    public virtual EvidencePack EvidencePack { get; set; } = null!;

    public bool IsPrimary { get; set; } = true;

    [StringLength(500)]
    public string? Notes { get; set; }
}

#endregion

#region Test Procedures

/// <summary>
/// Test Procedure - How to verify design and operating effectiveness
/// </summary>
public class TestProcedure : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string TestCode { get; set; } = string.Empty; // e.g., TST-ACCESS-001

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [StringLength(255)]
    public string? NameAr { get; set; }

    /// <summary>
    /// Test type: Design, OperatingEffectiveness, Both
    /// </summary>
    [Required]
    [StringLength(30)]
    public string TestType { get; set; } = "Both";

    /// <summary>
    /// Detailed test steps (JSON array)
    /// </summary>
    public string? TestStepsJson { get; set; }

    /// <summary>
    /// Expected results
    /// </summary>
    [StringLength(2000)]
    public string? ExpectedResults { get; set; }

    /// <summary>
    /// Sample size guidance
    /// </summary>
    [StringLength(500)]
    public string? SampleSizeGuidance { get; set; }

    /// <summary>
    /// Required frequency
    /// </summary>
    [StringLength(20)]
    public string Frequency { get; set; } = "Annual";

    public bool IsActive { get; set; } = true;

    // Navigation
    public virtual ICollection<ControlTestProcedure> ControlMappings { get; set; } = new List<ControlTestProcedure>();
}

/// <summary>
/// Link between Control and Test Procedure
/// </summary>
public class ControlTestProcedure : BaseEntity
{
    public Guid ControlId { get; set; }

    [ForeignKey("ControlId")]
    public virtual CanonicalControl Control { get; set; } = null!;

    public Guid TestProcedureId { get; set; }

    [ForeignKey("TestProcedureId")]
    public virtual TestProcedure TestProcedure { get; set; } = null!;

    public bool IsPrimary { get; set; } = true;

    [StringLength(500)]
    public string? Notes { get; set; }
}

#endregion

#region Applicability Rules

/// <summary>
/// Applicability Rule - Defines when a control applies based on attributes
/// </summary>
public class ApplicabilityRule : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string RuleCode { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Rule type: Include, Exclude
    /// </summary>
    [Required]
    [StringLength(20)]
    public string RuleType { get; set; } = "Include";

    /// <summary>
    /// Attribute: Jurisdiction, Industry, DataType, SystemType, RiskTier, Size
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Attribute { get; set; } = string.Empty;

    /// <summary>
    /// Operator: Equals, Contains, In, NotIn
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Operator { get; set; } = "Equals";

    /// <summary>
    /// Value(s) to match (comma-separated for In/NotIn)
    /// </summary>
    [Required]
    [StringLength(500)]
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Priority for rule evaluation (lower = higher priority)
    /// </summary>
    public int Priority { get; set; } = 100;

    public bool IsActive { get; set; } = true;

    // Can be linked to specific controls or apply globally
    public Guid? ControlId { get; set; }

    [ForeignKey("ControlId")]
    public virtual CanonicalControl? Control { get; set; }
}

#endregion

#region Version Control & Change Management

/// <summary>
/// Control Change History - Tracks all changes to controls, mappings, requirements
/// </summary>
public class ControlChangeHistory : BaseEntity
{
    /// <summary>
    /// Entity type: Control, Requirement, Mapping, EvidencePack, TestProcedure
    /// </summary>
    [Required]
    [StringLength(50)]
    public string EntityType { get; set; } = string.Empty;

    public Guid EntityId { get; set; }

    /// <summary>
    /// Change type: Create, Update, Delete, Activate, Deactivate
    /// </summary>
    [Required]
    [StringLength(20)]
    public string ChangeType { get; set; } = string.Empty;

    /// <summary>
    /// Previous version (JSON snapshot)
    /// </summary>
    public string? PreviousValueJson { get; set; }

    /// <summary>
    /// New version (JSON snapshot)
    /// </summary>
    public string? NewValueJson { get; set; }

    /// <summary>
    /// Change reason/rationale
    /// </summary>
    [StringLength(1000)]
    public string? ChangeReason { get; set; }

    /// <summary>
    /// Change request reference
    /// </summary>
    [StringLength(100)]
    public string? ChangeRequestId { get; set; }

    /// <summary>
    /// Who made the change
    /// </summary>
    [Required]
    [StringLength(100)]
    public string ChangedBy { get; set; } = string.Empty;

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Approval status: Pending, Approved, Rejected
    /// </summary>
    [StringLength(20)]
    public string ApprovalStatus { get; set; } = "Approved";

    [StringLength(100)]
    public string? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }
}

#endregion
