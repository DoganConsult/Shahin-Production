using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities;

#region MAP Framework (Map-Apply-Prove)

/// <summary>
/// MAP Framework Configuration - The universal compliance framework
/// MAP = Map (Requirement→Objective→Control) + Apply (Scope Rules) + Prove (Evidence+Tests)
/// </summary>
public class MAPFrameworkConfig : BaseEntity
{
    public Guid TenantId { get; set; }

    [ForeignKey("TenantId")]
    public virtual Tenant Tenant { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string ConfigCode { get; set; } = "MAP-DEFAULT";

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = "MAP Compliance Framework";

    [StringLength(255)]
    public string? NameAr { get; set; } = "إطار الامتثال MAP";

    /// <summary>
    /// Framework tagline
    /// </summary>
    [StringLength(500)]
    public string Tagline { get; set; } = "Map requirements. Apply scope rules. Prove with evidence.";

    /// <summary>
    /// Baseline control set ID
    /// </summary>
    public Guid? BaselineSetId { get; set; }

    [ForeignKey("BaselineSetId")]
    public virtual BaselineControlSet? BaselineSet { get; set; }

    /// <summary>
    /// Active overlays (comma-separated codes)
    /// </summary>
    [StringLength(500)]
    public string? ActiveOverlays { get; set; }

    /// <summary>
    /// Governance rhythm configuration (JSON)
    /// </summary>
    public string? GovernanceRhythmJson { get; set; }

    /// <summary>
    /// Evidence naming standard
    /// </summary>
    [StringLength(500)]
    public string EvidenceNamingStandard { get; set; } = "{ControlID}-{System}-{Period}-{EvidenceType}-v{Version}";

    /// <summary>
    /// Version
    /// </summary>
    [StringLength(20)]
    public string Version { get; set; } = "1.0";

    public DateTime ActivatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;
}

#endregion

#region Plain-Language Control Standards

/// <summary>
/// Plain-Language Control Template - Ensures controls are readable and testable
/// Format: Short (1-3 lines), Testable, Role-based, Time-bound, Evidence-defined
/// </summary>
public class PlainLanguageControl : BaseEntity
{
    public Guid ControlId { get; set; }

    [ForeignKey("ControlId")]
    public virtual CanonicalControl Control { get; set; } = null!;

    /// <summary>
    /// Plain language statement (1-3 lines, clear pass/fail)
    /// </summary>
    [Required]
    [StringLength(500)]
    public string PlainStatement { get; set; } = string.Empty;

    /// <summary>
    /// Plain language statement in Arabic
    /// </summary>
    [StringLength(500)]
    public string? PlainStatementAr { get; set; }

    /// <summary>
    /// Who performs this control (role)
    /// </summary>
    [Required]
    [StringLength(100)]
    public string WhoPerforms { get; set; } = string.Empty;

    /// <summary>
    /// How often (frequency in plain language)
    /// </summary>
    [Required]
    [StringLength(50)]
    public string HowOften { get; set; } = string.Empty;

    /// <summary>
    /// What proves it (evidence summary)
    /// </summary>
    [Required]
    [StringLength(500)]
    public string WhatProvesIt { get; set; } = string.Empty;

    /// <summary>
    /// Pass criteria (clear, testable)
    /// </summary>
    [Required]
    [StringLength(500)]
    public string PassCriteria { get; set; } = string.Empty;

    /// <summary>
    /// Fail criteria (what triggers exception)
    /// </summary>
    [StringLength(500)]
    public string? FailCriteria { get; set; }

    /// <summary>
    /// Example: Full plain-language control statement
    /// e.g., "Privileged access is granted via approved request and reviewed quarterly.
    /// Evidence: access review report + approvals + remediation tickets."
    /// </summary>
    [StringLength(1000)]
    public string? FullExample { get; set; }

    public bool IsApproved { get; set; } = false;

    [StringLength(100)]
    public string? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }
}

#endregion

#region Universal Evidence Packs

/// <summary>
/// Universal Evidence Pack - Standard pack reused across all audits
/// 10-15 packs covering all control families
/// </summary>
public class UniversalEvidencePack : BaseEntity
{
    [Required]
    [StringLength(30)]
    public string PackCode { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [StringLength(255)]
    public string? NameAr { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Control family: IAM, Logging, Vulnerability, Change, Backup, DR, Incident, Vendor, Privacy, SDLC, ERP
    /// </summary>
    [Required]
    [StringLength(30)]
    public string ControlFamily { get; set; } = string.Empty;

    /// <summary>
    /// Icon class for UI
    /// </summary>
    [StringLength(50)]
    public string? IconClass { get; set; }

    /// <summary>
    /// Evidence items in this pack (JSON array)
    /// </summary>
    public string? EvidenceItemsJson { get; set; }

    /// <summary>
    /// Naming standard for this pack
    /// </summary>
    [StringLength(255)]
    public string? NamingStandard { get; set; }

    /// <summary>
    /// Storage location pattern
    /// </summary>
    [StringLength(255)]
    public string? StorageLocationPattern { get; set; }

    /// <summary>
    /// Minimal test steps (JSON array)
    /// </summary>
    public string? MinimalTestStepsJson { get; set; }

    /// <summary>
    /// Frameworks this pack satisfies (comma-separated)
    /// </summary>
    [StringLength(500)]
    public string? SatisfiesFrameworks { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Evidence Pack Item - Individual evidence item within a pack
/// </summary>
public class UniversalEvidencePackItem : BaseEntity
{
    public Guid PackId { get; set; }

    [ForeignKey("PackId")]
    public virtual UniversalEvidencePack Pack { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string ItemCode { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [StringLength(255)]
    public string? NameAr { get; set; }

    /// <summary>
    /// Evidence type: Report, Screenshot, Log, Configuration, Sample, Attestation, Policy
    /// </summary>
    [Required]
    [StringLength(30)]
    public string EvidenceType { get; set; } = "Report";

    /// <summary>
    /// Collection frequency: Continuous, Daily, Weekly, Monthly, Quarterly, Annual
    /// </summary>
    [StringLength(20)]
    public string Frequency { get; set; } = "Quarterly";

    /// <summary>
    /// Is mandatory in this pack?
    /// </summary>
    public bool IsMandatory { get; set; } = true;

    /// <summary>
    /// Collection guidance
    /// </summary>
    [StringLength(1000)]
    public string? CollectionGuidance { get; set; }

    /// <summary>
    /// Sample file name
    /// </summary>
    [StringLength(255)]
    public string? SampleFileName { get; set; }

    /// <summary>
    /// Retention period in months
    /// </summary>
    public int RetentionMonths { get; set; } = 84;

    public int DisplayOrder { get; set; }
}

#endregion

#region Governance Rhythm

/// <summary>
/// Governance Rhythm Template - Predictable cadence for all markets
/// Weekly → Monthly → Quarterly → Annual
/// </summary>
public class GovernanceRhythmTemplate : BaseEntity
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
    /// Is this the default template?
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// Rhythm items (JSON array)
    /// </summary>
    public string? RhythmItemsJson { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Governance Rhythm Item - Individual activity in the rhythm
/// </summary>
public class GovernanceRhythmItem : BaseEntity
{
    public Guid TemplateId { get; set; }

    [ForeignKey("TemplateId")]
    public virtual GovernanceRhythmTemplate Template { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string ItemCode { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [StringLength(255)]
    public string? NameAr { get; set; }

    /// <summary>
    /// Frequency: Weekly, Monthly, Quarterly, SemiAnnual, Annual
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Frequency { get; set; } = string.Empty;

    /// <summary>
    /// Duration in minutes
    /// </summary>
    public int DurationMinutes { get; set; } = 30;

    /// <summary>
    /// Owner role code
    /// </summary>
    [StringLength(50)]
    public string? OwnerRoleCode { get; set; }

    /// <summary>
    /// Participants (comma-separated role codes)
    /// </summary>
    [StringLength(255)]
    public string? ParticipantRoleCodes { get; set; }

    /// <summary>
    /// Activities (JSON array)
    /// </summary>
    public string? ActivitiesJson { get; set; }

    /// <summary>
    /// Deliverables (JSON array)
    /// </summary>
    public string? DeliverablesJson { get; set; }

    public int DisplayOrder { get; set; }
}

#endregion

#region One-Page Guide

/// <summary>
/// One-Page Guide - Quick reference for each market/team
/// </summary>
public class OnePageGuide : BaseEntity
{
    public Guid TenantId { get; set; }

    [ForeignKey("TenantId")]
    public virtual Tenant Tenant { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string GuideCode { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Title { get; set; } = "How to Use MAP Framework";

    [StringLength(255)]
    public string? TitleAr { get; set; }

    /// <summary>
    /// Target audience: AllTeams, IT, Finance, Compliance, BusinessUnit
    /// </summary>
    [StringLength(50)]
    public string TargetAudience { get; set; } = "AllTeams";

    /// <summary>
    /// Section 1: What is in scope
    /// </summary>
    [StringLength(2000)]
    public string? WhatIsInScope { get; set; }

    /// <summary>
    /// Section 2: How to decide applicability
    /// </summary>
    [StringLength(2000)]
    public string? HowToDecideApplicability { get; set; }

    /// <summary>
    /// Section 3: Where to put evidence
    /// </summary>
    [StringLength(2000)]
    public string? WhereToStoreEvidence { get; set; }

    /// <summary>
    /// Section 4: Who approves exceptions
    /// </summary>
    [StringLength(2000)]
    public string? WhoApprovesExceptions { get; set; }

    /// <summary>
    /// Section 5: How audits are served
    /// </summary>
    [StringLength(2000)]
    public string? HowAuditsAreServed { get; set; }

    /// <summary>
    /// Quick links (JSON array)
    /// </summary>
    public string? QuickLinksJson { get; set; }

    /// <summary>
    /// Contact information
    /// </summary>
    [StringLength(500)]
    public string? ContactInfo { get; set; }

    /// <summary>
    /// Version
    /// </summary>
    [StringLength(20)]
    public string Version { get; set; } = "1.0";

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;
}

#endregion

#region Strategic Capabilities (10-Year Roadmap)

/// <summary>
/// Cryptographic Asset Inventory - For crypto agility and PQC readiness
/// </summary>
public class CryptographicAsset : BaseEntity
{
    public Guid TenantId { get; set; }

    [ForeignKey("TenantId")]
    public virtual Tenant Tenant { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string AssetCode { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Asset type: Certificate, Key, Algorithm, Protocol
    /// </summary>
    [Required]
    [StringLength(30)]
    public string AssetType { get; set; } = string.Empty;

    /// <summary>
    /// System/application using this asset
    /// </summary>
    [StringLength(255)]
    public string? SystemName { get; set; }

    /// <summary>
    /// Current algorithm: RSA-2048, AES-256, SHA-256, etc.
    /// </summary>
    [StringLength(50)]
    public string? CurrentAlgorithm { get; set; }

    /// <summary>
    /// Key size in bits
    /// </summary>
    public int? KeySizeBits { get; set; }

    /// <summary>
    /// Is quantum-vulnerable?
    /// </summary>
    public bool IsQuantumVulnerable { get; set; } = false;

    /// <summary>
    /// PQC migration status: NotStarted, Planning, InProgress, Completed
    /// </summary>
    [StringLength(20)]
    public string PQCMigrationStatus { get; set; } = "NotStarted";

    /// <summary>
    /// Target PQC algorithm: ML-KEM, ML-DSA, SLH-DSA, etc.
    /// </summary>
    [StringLength(50)]
    public string? TargetPQCAlgorithm { get; set; }

    /// <summary>
    /// Migration priority: Critical, High, Medium, Low
    /// </summary>
    [StringLength(20)]
    public string MigrationPriority { get; set; } = "Medium";

    /// <summary>
    /// Expiry date (for certificates)
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>
    /// Owner
    /// </summary>
    [StringLength(100)]
    public string? OwnerId { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Third-Party Concentration - Systemic dependency risk tracking
/// </summary>
public class ThirdPartyConcentration : BaseEntity
{
    public Guid TenantId { get; set; }

    [ForeignKey("TenantId")]
    public virtual Tenant Tenant { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string VendorCode { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string VendorName { get; set; } = string.Empty;

    /// <summary>
    /// Vendor type: CloudProvider, CoreBanking, SaaS, Fintech, DataCenter
    /// </summary>
    [Required]
    [StringLength(30)]
    public string VendorType { get; set; } = string.Empty;

    /// <summary>
    /// Services provided (JSON array)
    /// </summary>
    public string? ServicesProvidedJson { get; set; }

    /// <summary>
    /// Criticality tier: Tier1, Tier2, Tier3
    /// </summary>
    [Required]
    [StringLength(10)]
    public string CriticalityTier { get; set; } = "Tier2";

    /// <summary>
    /// Substitutability: Easy, Moderate, Difficult, Impossible
    /// </summary>
    [StringLength(20)]
    public string Substitutability { get; set; } = "Moderate";

    /// <summary>
    /// Concentration risk score (0-100)
    /// </summary>
    public int ConcentrationRiskScore { get; set; } = 50;

    /// <summary>
    /// Has tested exit plan?
    /// </summary>
    public bool HasTestedExitPlan { get; set; } = false;

    /// <summary>
    /// Exit plan last tested
    /// </summary>
    public DateTime? ExitPlanLastTested { get; set; }

    /// <summary>
    /// Exit time estimate (months)
    /// </summary>
    public int? ExitTimeMonths { get; set; }

    /// <summary>
    /// Continuous assurance available?
    /// </summary>
    public bool HasContinuousAssurance { get; set; } = false;

    /// <summary>
    /// Evidence API available?
    /// </summary>
    public bool HasEvidenceAPI { get; set; } = false;

    /// <summary>
    /// Contract end date
    /// </summary>
    public DateTime? ContractEndDate { get; set; }

    /// <summary>
    /// Owner
    /// </summary>
    [StringLength(100)]
    public string? OwnerId { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Compliance-by-Design Guardrail - Policy-as-code in CI/CD
/// </summary>
public class ComplianceGuardrail : BaseEntity
{
    public Guid TenantId { get; set; }

    [ForeignKey("TenantId")]
    public virtual Tenant Tenant { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string GuardrailCode { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Guardrail type: Policy, Configuration, Security, Access, Data
    /// </summary>
    [Required]
    [StringLength(30)]
    public string GuardrailType { get; set; } = string.Empty;

    /// <summary>
    /// Enforcement point: CICD, CloudPosture, IdentityPosture, Runtime
    /// </summary>
    [Required]
    [StringLength(30)]
    public string EnforcementPoint { get; set; } = string.Empty;

    /// <summary>
    /// Rule definition (policy-as-code)
    /// </summary>
    public string? RuleDefinitionJson { get; set; }

    /// <summary>
    /// Enforcement mode: Audit, Warn, Block
    /// </summary>
    [StringLength(20)]
    public string EnforcementMode { get; set; } = "Audit";

    /// <summary>
    /// Related control ID
    /// </summary>
    public Guid? ControlId { get; set; }

    [ForeignKey("ControlId")]
    public virtual CanonicalControl? Control { get; set; }

    /// <summary>
    /// Last evaluation date
    /// </summary>
    public DateTime? LastEvaluatedAt { get; set; }

    /// <summary>
    /// Last evaluation result: Pass, Fail, Error
    /// </summary>
    [StringLength(20)]
    public string? LastEvaluationResult { get; set; }

    /// <summary>
    /// Violations count (last evaluation)
    /// </summary>
    public int? ViolationsCount { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Strategic Roadmap Milestone - 10-year capability tracking
/// </summary>
public class StrategicRoadmapMilestone : BaseEntity
{
    public Guid TenantId { get; set; }

    [ForeignKey("TenantId")]
    public virtual Tenant Tenant { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string MilestoneCode { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Capability area: CCM, Resilience, ThirdParty, AIGovernance, CryptoAgility, ComplianceByDesign
    /// </summary>
    [Required]
    [StringLength(30)]
    public string CapabilityArea { get; set; } = string.Empty;

    /// <summary>
    /// Phase: 0-12months, 1-3years, 3-10years
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Phase { get; set; } = string.Empty;

    /// <summary>
    /// Target date
    /// </summary>
    public DateTime? TargetDate { get; set; }

    /// <summary>
    /// Status: NotStarted, InProgress, Completed, Delayed
    /// </summary>
    [StringLength(20)]
    public string Status { get; set; } = "NotStarted";

    /// <summary>
    /// Completion percentage
    /// </summary>
    public int CompletionPercent { get; set; } = 0;

    /// <summary>
    /// Owner
    /// </summary>
    [StringLength(100)]
    public string? OwnerId { get; set; }

    /// <summary>
    /// Dependencies (comma-separated milestone codes)
    /// </summary>
    [StringLength(500)]
    public string? Dependencies { get; set; }

    /// <summary>
    /// Success criteria
    /// </summary>
    [StringLength(1000)]
    public string? SuccessCriteria { get; set; }

    public bool IsActive { get; set; } = true;
}

#endregion

#region Standard Governance Rhythm (Seed Data)

/// <summary>
/// Standard governance rhythm for all markets
/// </summary>
public static class StandardGovernanceRhythm
{
    public static readonly List<RhythmItemInfo> Items = new()
    {
        // Weekly
        new RhythmItemInfo
        {
            Code = "WEEKLY-EXCEPTIONS",
            Name = "Exceptions Triage",
            NameAr = "فرز الاستثناءات",
            Frequency = "Weekly",
            DurationMinutes = 30,
            OwnerRole = "ComplianceOfficer",
            Activities = new[] { "Review new exceptions", "Prioritize remediation", "Escalate blockers" }
        },
        // Monthly
        new RhythmItemInfo
        {
            Code = "MONTHLY-CONTROL-OPS",
            Name = "Control Operations Review",
            NameAr = "مراجعة عمليات الضوابط",
            Frequency = "Monthly",
            DurationMinutes = 60,
            OwnerRole = "RiskManager",
            Activities = new[] { "Review KRIs/KPIs", "Check overdue evidence", "Track remediation aging", "Update dashboards" }
        },
        // Quarterly
        new RhythmItemInfo
        {
            Code = "QUARTERLY-ACCESS-REVIEW",
            Name = "Access Reviews & Control Attestations",
            NameAr = "مراجعات الوصول وشهادات الضوابط",
            Frequency = "Quarterly",
            DurationMinutes = 120,
            OwnerRole = "SecurityManager",
            Activities = new[] { "Complete access reviews", "Sign control attestations", "Review SoD conflicts", "Update access policies" }
        },
        // Semi-Annual
        new RhythmItemInfo
        {
            Code = "SEMIANNUAL-DR-TEST",
            Name = "DR Test & Tabletop Exercise",
            NameAr = "اختبار التعافي من الكوارث وتمرين المحاكاة",
            Frequency = "SemiAnnual",
            DurationMinutes = 480,
            OwnerRole = "BCMManager",
            Activities = new[] { "Execute DR test", "Conduct tabletop exercise", "Document lessons learned", "Update recovery procedures" }
        },
        // Annual
        new RhythmItemInfo
        {
            Code = "ANNUAL-POLICY-REFRESH",
            Name = "Policy Review & Refresh",
            NameAr = "مراجعة وتحديث السياسات",
            Frequency = "Annual",
            DurationMinutes = 240,
            OwnerRole = "GRCManager",
            Activities = new[] { "Review all policies", "Update for regulatory changes", "Obtain approvals", "Communicate changes" }
        }
    };
}

public class RhythmItemInfo
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public string OwnerRole { get; set; } = string.Empty;
    public string[] Activities { get; set; } = Array.Empty<string>();
}

#endregion
