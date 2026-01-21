using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities;

/// <summary>
/// Security baseline profile for an organization
/// Represents a tailored security baseline (e.g., NIST 800-53 Moderate)
/// </summary>
public class BaselineProfile : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Code { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Source framework (NIST, ISO, NCA, etc.)
    /// </summary>
    [MaxLength(50)]
    public string? SourceFramework { get; set; }

    /// <summary>
    /// Impact level (Low, Moderate, High)
    /// </summary>
    [MaxLength(20)]
    public string ImpactLevel { get; set; } = "Moderate";

    public bool IsActive { get; set; } = true;

    public DateTime? EffectiveDate { get; set; }

    public virtual ICollection<TailoringDecision> TailoringDecisions { get; set; } = new List<TailoringDecision>();
}

/// <summary>
/// Control overlay for customizing baseline controls
/// </summary>
public class ControlOverlay : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Code { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Type: Privacy, Cloud, Industrial, etc.
    /// </summary>
    [MaxLength(50)]
    public string OverlayType { get; set; } = "General";

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// JSON configuration for overlay parameters
    /// </summary>
    public string? ConfigurationJson { get; set; }
}

/// <summary>
/// Tailoring decision for a specific control in a baseline
/// </summary>
public class TailoringDecision : BaseEntity
{
    public Guid BaselineProfileId { get; set; }

    [ForeignKey(nameof(BaselineProfileId))]
    public virtual BaselineProfile? BaselineProfile { get; set; }

    public Guid? ControlId { get; set; }

    [ForeignKey(nameof(ControlId))]
    public virtual Control? Control { get; set; }

    /// <summary>
    /// Decision: Include, Exclude, Modify, Compensate
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Decision { get; set; } = "Include";

    [MaxLength(2000)]
    public string? Justification { get; set; }

    /// <summary>
    /// If modified, the modified requirement text
    /// </summary>
    public string? ModifiedRequirement { get; set; }

    /// <summary>
    /// If compensating, reference to compensating control
    /// </summary>
    public Guid? CompensatingControlId { get; set; }

    public DateTime DecisionDate { get; set; } = DateTime.UtcNow;

    [MaxLength(200)]
    public string? ApprovedBy { get; set; }
}

/// <summary>
/// Mission Assurance Profile - organizational security profile
/// </summary>
public class MAPProfile : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Code { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Mission criticality level (1-5)
    /// </summary>
    public int CriticalityLevel { get; set; } = 3;

    /// <summary>
    /// Confidentiality impact (Low, Moderate, High)
    /// </summary>
    [MaxLength(20)]
    public string ConfidentialityImpact { get; set; } = "Moderate";

    /// <summary>
    /// Integrity impact
    /// </summary>
    [MaxLength(20)]
    public string IntegrityImpact { get; set; } = "Moderate";

    /// <summary>
    /// Availability impact
    /// </summary>
    [MaxLength(20)]
    public string AvailabilityImpact { get; set; } = "Moderate";

    public bool IsActive { get; set; } = true;

    public virtual ICollection<MAPControlImplementation> ControlImplementations { get; set; } = new List<MAPControlImplementation>();
}

/// <summary>
/// MAP control implementation details
/// </summary>
public class MAPControlImplementation : BaseEntity
{
    public Guid MAPProfileId { get; set; }

    [ForeignKey(nameof(MAPProfileId))]
    public virtual MAPProfile? MAPProfile { get; set; }

    public Guid? ControlId { get; set; }

    [ForeignKey(nameof(ControlId))]
    public virtual Control? Control { get; set; }

    /// <summary>
    /// Implementation status: Planned, In Progress, Implemented, Not Applicable
    /// </summary>
    [MaxLength(50)]
    public string Status { get; set; } = "Planned";

    /// <summary>
    /// Implementation description
    /// </summary>
    public string? ImplementationDescription { get; set; }

    /// <summary>
    /// Responsible party
    /// </summary>
    [MaxLength(200)]
    public string? ResponsibleParty { get; set; }

    public DateTime? TargetDate { get; set; }

    public DateTime? CompletionDate { get; set; }

    public virtual ICollection<MAPParameter> Parameters { get; set; } = new List<MAPParameter>();
}

/// <summary>
/// MAP parameter for control implementation
/// </summary>
public class MAPParameter : BaseEntity
{
    public Guid MAPControlImplementationId { get; set; }

    [ForeignKey(nameof(MAPControlImplementationId))]
    public virtual MAPControlImplementation? MAPControlImplementation { get; set; }

    [Required]
    [MaxLength(100)]
    public string ParameterName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? ParameterValue { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Data type: String, Number, Date, List, etc.
    /// </summary>
    [MaxLength(20)]
    public string DataType { get; set; } = "String";
}

/// <summary>
/// Canonical control mapping between frameworks
/// </summary>
public class CanonicalControlMapping : BaseEntity
{
    /// <summary>
    /// Source control ID
    /// </summary>
    public Guid SourceControlId { get; set; }

    /// <summary>
    /// Target control ID
    /// </summary>
    public Guid TargetControlId { get; set; }

    /// <summary>
    /// Source framework code
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string SourceFramework { get; set; } = string.Empty;

    /// <summary>
    /// Target framework code
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string TargetFramework { get; set; } = string.Empty;

    /// <summary>
    /// Mapping strength: Exact, Strong, Partial, Weak
    /// </summary>
    [MaxLength(20)]
    public string MappingStrength { get; set; } = "Partial";

    [MaxLength(1000)]
    public string? Notes { get; set; }

    /// <summary>
    /// Confidence score (0-100)
    /// </summary>
    public int ConfidenceScore { get; set; } = 80;

    /// <summary>
    /// Is this mapping verified by expert?
    /// </summary>
    public bool IsVerified { get; set; }

    [MaxLength(200)]
    public string? VerifiedBy { get; set; }

    public DateTime? VerifiedAt { get; set; }
}

/// <summary>
/// Risk taxonomy for categorizing risks
/// </summary>
public class RiskTaxonomy : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Code { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Parent taxonomy for hierarchical structure
    /// </summary>
    public Guid? ParentId { get; set; }

    [ForeignKey(nameof(ParentId))]
    public virtual RiskTaxonomy? Parent { get; set; }

    /// <summary>
    /// Level in hierarchy (1 = top level)
    /// </summary>
    public int Level { get; set; } = 1;

    /// <summary>
    /// Sort order within parent
    /// </summary>
    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public virtual ICollection<RiskTaxonomy> Children { get; set; } = new List<RiskTaxonomy>();
}

/// <summary>
/// Risk scenario for threat modeling
/// </summary>
public class RiskScenario : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Code { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    /// <summary>
    /// Threat actor type: Nation-state, Criminal, Insider, etc.
    /// </summary>
    [MaxLength(50)]
    public string? ThreatActorType { get; set; }

    /// <summary>
    /// Attack vector: Phishing, Malware, Social Engineering, etc.
    /// </summary>
    [MaxLength(100)]
    public string? AttackVector { get; set; }

    /// <summary>
    /// Target asset type
    /// </summary>
    [MaxLength(100)]
    public string? TargetAssetType { get; set; }

    /// <summary>
    /// Likelihood (1-5)
    /// </summary>
    public int Likelihood { get; set; } = 3;

    /// <summary>
    /// Impact (1-5)
    /// </summary>
    public int Impact { get; set; } = 3;

    /// <summary>
    /// Inherent risk score
    /// </summary>
    public decimal InherentRiskScore { get; set; }

    /// <summary>
    /// Residual risk score after controls
    /// </summary>
    public decimal ResidualRiskScore { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Risk appetite statement
/// </summary>
public class RiskAppetiteStatement : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Risk category this applies to
    /// </summary>
    [MaxLength(100)]
    public string? RiskCategory { get; set; }

    [MaxLength(2000)]
    public string? Statement { get; set; }

    /// <summary>
    /// Appetite level: Averse, Minimal, Cautious, Open, Hungry
    /// </summary>
    [MaxLength(20)]
    public string AppetiteLevel { get; set; } = "Cautious";

    /// <summary>
    /// Threshold value (if quantitative)
    /// </summary>
    public decimal? ThresholdValue { get; set; }

    /// <summary>
    /// Threshold unit (%, $, count, etc.)
    /// </summary>
    [MaxLength(20)]
    public string? ThresholdUnit { get; set; }

    public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;

    public DateTime? ExpiryDate { get; set; }

    [MaxLength(200)]
    public string? ApprovedBy { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Threat profile for threat intelligence
/// </summary>
public class ThreatProfile : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Code { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    /// <summary>
    /// Threat type: APT, Malware, Insider, Physical, etc.
    /// </summary>
    [MaxLength(50)]
    public string ThreatType { get; set; } = "Cyber";

    /// <summary>
    /// Threat actor: Nation-state, Criminal Organization, Hacktivist, etc.
    /// </summary>
    [MaxLength(100)]
    public string? ThreatActor { get; set; }

    /// <summary>
    /// Capability level (1-5)
    /// </summary>
    public int CapabilityLevel { get; set; } = 3;

    /// <summary>
    /// Intent level (1-5)
    /// </summary>
    public int IntentLevel { get; set; } = 3;

    /// <summary>
    /// Targeting likelihood (1-5)
    /// </summary>
    public int TargetingLikelihood { get; set; } = 3;

    /// <summary>
    /// MITRE ATT&CK techniques (JSON array)
    /// </summary>
    public string? MitreAttackTechniquesJson { get; set; }

    /// <summary>
    /// Industry sectors targeted
    /// </summary>
    [MaxLength(500)]
    public string? TargetedSectors { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? LastUpdated { get; set; }
}

/// <summary>
/// Vulnerability profile for vulnerability management
/// </summary>
public class VulnerabilityProfile : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// CVE ID if applicable
    /// </summary>
    [MaxLength(50)]
    public string? CveId { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    /// <summary>
    /// CVSS score (0-10)
    /// </summary>
    public decimal? CvssScore { get; set; }

    /// <summary>
    /// Severity: Critical, High, Medium, Low, Info
    /// </summary>
    [MaxLength(20)]
    public string Severity { get; set; } = "Medium";

    /// <summary>
    /// Affected systems/products
    /// </summary>
    [MaxLength(500)]
    public string? AffectedSystems { get; set; }

    /// <summary>
    /// Exploit availability: None, PoC, Weaponized
    /// </summary>
    [MaxLength(20)]
    public string ExploitAvailability { get; set; } = "None";

    /// <summary>
    /// Remediation status: Open, In Progress, Remediated, Accepted
    /// </summary>
    [MaxLength(20)]
    public string RemediationStatus { get; set; } = "Open";

    [MaxLength(2000)]
    public string? RemediationPlan { get; set; }

    public DateTime? DiscoveredAt { get; set; }

    public DateTime? RemediationDeadline { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Resilience capability for business continuity
/// </summary>
public class ResilienceCapability : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Code { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Capability type: Recovery, Continuity, Backup, Failover, etc.
    /// </summary>
    [MaxLength(50)]
    public string CapabilityType { get; set; } = "Recovery";

    /// <summary>
    /// Recovery Time Objective (hours)
    /// </summary>
    public int? RtoHours { get; set; }

    /// <summary>
    /// Recovery Point Objective (hours)
    /// </summary>
    public int? RpoHours { get; set; }

    /// <summary>
    /// Maturity level (1-5)
    /// </summary>
    public int MaturityLevel { get; set; } = 2;

    /// <summary>
    /// Last test date
    /// </summary>
    public DateTime? LastTestedAt { get; set; }

    /// <summary>
    /// Test result: Pass, Partial, Fail
    /// </summary>
    [MaxLength(20)]
    public string? LastTestResult { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Control applicability rule for automated scoping
/// </summary>
public class ControlApplicabilityRule : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Rule type: Include, Exclude
    /// </summary>
    [MaxLength(20)]
    public string RuleType { get; set; } = "Include";

    /// <summary>
    /// Condition field (e.g., "AssetType", "DataClassification", "Location")
    /// </summary>
    [MaxLength(100)]
    public string ConditionField { get; set; } = string.Empty;

    /// <summary>
    /// Condition operator: Equals, Contains, StartsWith, In, etc.
    /// </summary>
    [MaxLength(20)]
    public string ConditionOperator { get; set; } = "Equals";

    /// <summary>
    /// Condition value(s) - JSON array for "In" operator
    /// </summary>
    [MaxLength(500)]
    public string ConditionValue { get; set; } = string.Empty;

    /// <summary>
    /// Target control ID if specific control
    /// </summary>
    public Guid? TargetControlId { get; set; }

    /// <summary>
    /// Target framework if all controls in framework
    /// </summary>
    [MaxLength(50)]
    public string? TargetFramework { get; set; }

    public int Priority { get; set; } = 100;

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Applicability decision record
/// </summary>
public class ApplicabilityDecision : BaseEntity
{
    /// <summary>
    /// Control this decision applies to
    /// </summary>
    public Guid ControlId { get; set; }

    [ForeignKey(nameof(ControlId))]
    public virtual Control? Control { get; set; }

    /// <summary>
    /// Asset or scope item this decision applies to
    /// </summary>
    public Guid? AssetId { get; set; }

    /// <summary>
    /// Decision: Applicable, NotApplicable, Inherited
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Decision { get; set; } = "Applicable";

    [MaxLength(2000)]
    public string? Justification { get; set; }

    /// <summary>
    /// Rule that triggered this decision (if automated)
    /// </summary>
    public Guid? ApplicabilityRuleId { get; set; }

    [ForeignKey(nameof(ApplicabilityRuleId))]
    public virtual ControlApplicabilityRule? ApplicabilityRule { get; set; }

    /// <summary>
    /// Is this an automated decision?
    /// </summary>
    public bool IsAutomated { get; set; }

    public DateTime DecisionDate { get; set; } = DateTime.UtcNow;

    [MaxLength(200)]
    public string? DecidedBy { get; set; }

    /// <summary>
    /// Review date for periodic review
    /// </summary>
    public DateTime? NextReviewDate { get; set; }
}

/// <summary>
/// Control inheritance relationship
/// </summary>
public class ControlInheritance : BaseEntity
{
    /// <summary>
    /// Child control that inherits
    /// </summary>
    public Guid ChildControlId { get; set; }

    /// <summary>
    /// Parent control being inherited from
    /// </summary>
    public Guid ParentControlId { get; set; }

    /// <summary>
    /// Inheritance type: Full, Partial, Hybrid
    /// </summary>
    [MaxLength(20)]
    public string InheritanceType { get; set; } = "Full";

    /// <summary>
    /// For partial inheritance, what percentage (0-100)
    /// </summary>
    public int? InheritancePercentage { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    /// <summary>
    /// If hybrid, what is inherited (JSON)
    /// </summary>
    public string? InheritedAspectsJson { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;

    public DateTime? ExpiryDate { get; set; }
}
