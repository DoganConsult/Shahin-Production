using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities;

#region Applicability Matrix

/// <summary>
/// Applicability Matrix Entry - Determines what controls/requirements apply to a specific scope
/// Each entry answers: "Does this control apply to this scope? Why or why not?"
/// </summary>
public class ApplicabilityEntry : BaseEntity
{
    public Guid TenantId { get; set; }

    [ForeignKey("TenantId")]
    public virtual Tenant Tenant { get; set; } = null!;

    /// <summary>
    /// Assessment or scope this entry belongs to
    /// </summary>
    public Guid? AssessmentId { get; set; }

    [ForeignKey("AssessmentId")]
    public virtual Assessment? Assessment { get; set; }

    /// <summary>
    /// The control being evaluated for applicability
    /// </summary>
    public Guid ControlId { get; set; }

    [ForeignKey("ControlId")]
    public virtual CanonicalControl Control { get; set; } = null!;

    /// <summary>
    /// The requirement being evaluated (optional, for requirement-level applicability)
    /// </summary>
    public Guid? RequirementId { get; set; }

    [ForeignKey("RequirementId")]
    public virtual RegulatoryRequirement? Requirement { get; set; }

    /// <summary>
    /// Applicability status: Applicable, NotApplicable, Inherited, Exception
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Applicable";

    /// <summary>
    /// Reason for the applicability decision (required for NotApplicable/Exception)
    /// </summary>
    [StringLength(1000)]
    public string? Reason { get; set; }

    /// <summary>
    /// If Inherited, which parent control/entity provides coverage
    /// </summary>
    [StringLength(255)]
    public string? InheritedFrom { get; set; }

    /// <summary>
    /// If Exception, reference to exception approval
    /// </summary>
    [StringLength(100)]
    public string? ExceptionReference { get; set; }

    /// <summary>
    /// Exception expiry date (if applicable)
    /// </summary>
    public DateTime? ExceptionExpiryDate { get; set; }

    // Applicability drivers (why this decision was made)

    /// <summary>
    /// Jurisdiction driver: KSA, GCC, EU, US, etc.
    /// </summary>
    [StringLength(50)]
    public string? JurisdictionDriver { get; set; }

    /// <summary>
    /// Business line driver: Retail Banking, Corporate, Treasury, etc.
    /// </summary>
    [StringLength(100)]
    public string? BusinessLineDriver { get; set; }

    /// <summary>
    /// System tier driver: Tier1, Tier2, Tier3
    /// </summary>
    [StringLength(20)]
    public string? SystemTierDriver { get; set; }

    /// <summary>
    /// Data type driver: PII, PCI, PHI, Classified, etc.
    /// </summary>
    [StringLength(100)]
    public string? DataTypeDriver { get; set; }

    /// <summary>
    /// Hosting model driver: Cloud, OnPrem, Hybrid
    /// </summary>
    [StringLength(20)]
    public string? HostingModelDriver { get; set; }

    /// <summary>
    /// Evidence pack assigned for this control
    /// </summary>
    public Guid? EvidencePackId { get; set; }

    [ForeignKey("EvidencePackId")]
    public virtual EvidencePack? EvidencePack { get; set; }

    /// <summary>
    /// Control owner for this scope
    /// </summary>
    [StringLength(100)]
    public string? ControlOwnerId { get; set; }

    [StringLength(255)]
    public string? ControlOwnerName { get; set; }

    /// <summary>
    /// Approved by
    /// </summary>
    [StringLength(100)]
    public string? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public bool IsApproved { get; set; } = false;
}

#endregion

#region Standard Evidence Packs by Control Family

/// <summary>
/// Evidence Pack Family - Groups evidence packs by control domain
/// (Governance, IAM, Logging, Vulnerability, Change, Backup, Incident, ThirdParty)
/// </summary>
public class EvidencePackFamily : BaseEntity
{
    [Required]
    [StringLength(30)]
    public string FamilyCode { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [StringLength(255)]
    public string? NameAr { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Icon class for UI display
    /// </summary>
    [StringLength(50)]
    public string? IconClass { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation
    public virtual ICollection<StandardEvidenceItem> EvidenceItems { get; set; } = new List<StandardEvidenceItem>();
}

/// <summary>
/// Standard Evidence Item - Individual evidence artifact required for a control family
/// </summary>
public class StandardEvidenceItem : BaseEntity
{
    public Guid FamilyId { get; set; }

    [ForeignKey("FamilyId")]
    public virtual EvidencePackFamily Family { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string ItemCode { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [StringLength(255)]
    public string? NameAr { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Evidence type: Document, Screenshot, Report, Log, Configuration, Sample, Attestation
    /// </summary>
    [Required]
    [StringLength(30)]
    public string EvidenceType { get; set; } = "Document";

    /// <summary>
    /// Required frequency: Continuous, Daily, Weekly, Monthly, Quarterly, Annual, OnChange
    /// </summary>
    [StringLength(20)]
    public string RequiredFrequency { get; set; } = "Quarterly";

    /// <summary>
    /// Is this evidence mandatory for the control family?
    /// </summary>
    public bool IsMandatory { get; set; } = true;

    /// <summary>
    /// Sample file name or template reference
    /// </summary>
    [StringLength(255)]
    public string? SampleFileName { get; set; }

    /// <summary>
    /// Guidance on what this evidence should contain
    /// </summary>
    [StringLength(2000)]
    public string? CollectionGuidance { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;
}

#endregion

#region Quality Gate for Mappings

/// <summary>
/// Mapping Quality Gate - Validates that a control mapping meets quality standards
/// </summary>
public class MappingQualityGate : BaseEntity
{
    public Guid MappingId { get; set; }

    [ForeignKey("MappingId")]
    public virtual RequirementMapping Mapping { get; set; } = null!;

    /// <summary>
    /// Coverage statement: "This control satisfies requirement X because..."
    /// </summary>
    [StringLength(2000)]
    public string? CoverageStatement { get; set; }

    public bool HasCoverageStatement { get; set; } = false;

    /// <summary>
    /// Evidence linkage: Are evidence items listed and available?
    /// </summary>
    public bool HasEvidenceLinkage { get; set; } = false;

    [StringLength(500)]
    public string? EvidenceLinkageNotes { get; set; }

    /// <summary>
    /// Test method: How an assessor will test (design + operating effectiveness)
    /// </summary>
    [StringLength(2000)]
    public string? TestMethod { get; set; }

    public bool HasTestMethod { get; set; } = false;

    /// <summary>
    /// Gap status: FullyMet, PartiallyMet, NotMet, NotAssessed
    /// </summary>
    [Required]
    [StringLength(20)]
    public string GapStatus { get; set; } = "NotAssessed";

    /// <summary>
    /// If partially met or not met, what remediation is needed?
    /// </summary>
    [StringLength(2000)]
    public string? RemediationRequired { get; set; }

    /// <summary>
    /// Confidence rating: High, Medium, Low
    /// </summary>
    [Required]
    [StringLength(10)]
    public string ConfidenceRating { get; set; } = "Medium";

    /// <summary>
    /// Overall quality score (0-100)
    /// </summary>
    public int QualityScore { get; set; } = 0;

    /// <summary>
    /// Has the mapping passed quality gate?
    /// </summary>
    public bool PassedQualityGate { get; set; } = false;

    /// <summary>
    /// Quality gate reviewer
    /// </summary>
    [StringLength(100)]
    public string? ReviewedBy { get; set; }

    public DateTime? ReviewedAt { get; set; }

    /// <summary>
    /// Review notes/feedback
    /// </summary>
    [StringLength(2000)]
    public string? ReviewNotes { get; set; }
}

#endregion

#region RACI Workflow for Mapping Approval

/// <summary>
/// Mapping Workflow Step - Tracks the approval workflow for a control mapping
/// </summary>
public class MappingWorkflowStep : BaseEntity
{
    public Guid MappingId { get; set; }

    [ForeignKey("MappingId")]
    public virtual RequirementMapping Mapping { get; set; } = null!;

    /// <summary>
    /// Step number in the workflow
    /// </summary>
    public int StepNumber { get; set; }

    /// <summary>
    /// Role for this step: ControlOwner, MappingAnalyst, Assessor, FrameworkOwner, QAReviewer
    /// </summary>
    [Required]
    [StringLength(30)]
    public string RoleCode { get; set; } = string.Empty;

    /// <summary>
    /// RACI type: Responsible, Accountable, Consulted, Informed
    /// </summary>
    [Required]
    [StringLength(15)]
    public string RaciType { get; set; } = "Responsible";

    /// <summary>
    /// Step name
    /// </summary>
    [Required]
    [StringLength(100)]
    public string StepName { get; set; } = string.Empty;

    /// <summary>
    /// Step description
    /// </summary>
    [StringLength(500)]
    public string? StepDescription { get; set; }

    /// <summary>
    /// Assigned user
    /// </summary>
    [StringLength(100)]
    public string? AssignedToUserId { get; set; }

    [StringLength(255)]
    public string? AssignedToUserName { get; set; }

    /// <summary>
    /// Step status: Pending, InProgress, Completed, Rejected, Skipped
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// Decision: Approve, Reject, RequestChanges
    /// </summary>
    [StringLength(20)]
    public string? Decision { get; set; }

    /// <summary>
    /// Comments from the reviewer
    /// </summary>
    [StringLength(2000)]
    public string? Comments { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Due date for this step
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Is this step overdue?
    /// </summary>
    public bool IsOverdue => DueDate.HasValue && DueDate < DateTime.UtcNow && Status != "Completed";
}

/// <summary>
/// Standard workflow template for mapping approval
/// </summary>
public class MappingWorkflowTemplate : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string TemplateCode { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Workflow steps as JSON array
    /// </summary>
    public string? StepsJson { get; set; }

    /// <summary>
    /// Is this the default template?
    /// </summary>
    public bool IsDefault { get; set; } = false;

    public bool IsActive { get; set; } = true;
}

#endregion

#region Scope Definition

/// <summary>
/// Assessment Scope - Defines what is being assessed (entity, system, process)
/// </summary>
public class AssessmentScope : BaseEntity
{
    public Guid TenantId { get; set; }

    [ForeignKey("TenantId")]
    public virtual Tenant Tenant { get; set; } = null!;

    public Guid? AssessmentId { get; set; }

    [ForeignKey("AssessmentId")]
    public virtual Assessment? Assessment { get; set; }

    [Required]
    [StringLength(50)]
    public string ScopeCode { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string ScopeName { get; set; } = string.Empty;

    /// <summary>
    /// Scope type: Enterprise, BusinessUnit, System, Process, Vendor
    /// </summary>
    [Required]
    [StringLength(30)]
    public string ScopeType { get; set; } = "Enterprise";

    /// <summary>
    /// Description of what's in scope
    /// </summary>
    [StringLength(2000)]
    public string? ScopeDescription { get; set; }

    // Scope attributes

    /// <summary>
    /// Jurisdictions in scope (comma-separated)
    /// </summary>
    [StringLength(200)]
    public string? Jurisdictions { get; set; }

    /// <summary>
    /// Business lines in scope (comma-separated)
    /// </summary>
    [StringLength(500)]
    public string? BusinessLines { get; set; }

    /// <summary>
    /// Systems in scope (comma-separated or JSON)
    /// </summary>
    public string? SystemsInScope { get; set; }

    /// <summary>
    /// Data types in scope
    /// </summary>
    [StringLength(200)]
    public string? DataTypes { get; set; }

    /// <summary>
    /// Hosting models in scope
    /// </summary>
    [StringLength(100)]
    public string? HostingModels { get; set; }

    /// <summary>
    /// Third parties in scope
    /// </summary>
    public string? ThirdPartiesInScope { get; set; }

    /// <summary>
    /// What's explicitly out of scope
    /// </summary>
    [StringLength(2000)]
    public string? OutOfScopeDescription { get; set; }

    /// <summary>
    /// Scope approved by
    /// </summary>
    [StringLength(100)]
    public string? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public bool IsApproved { get; set; } = false;
}

#endregion
