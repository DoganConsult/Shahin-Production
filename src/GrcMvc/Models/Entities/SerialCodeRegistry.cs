using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities;

/// <summary>
/// Serial Code Registry - Stores all generated serial codes with full traceability.
/// Format: {PREFIX}-{TENANT}-{STAGE}-{YEAR}-{SEQUENCE}-{VERSION}
/// Example: ASM-ACME-01-2026-000142-01
/// </summary>
[Table("SerialCodeRegistry")]
public class SerialCodeRegistry
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Full serial code (unique)
    /// </summary>
    [Required]
    [MaxLength(35)]
    public string Code { get; set; } = string.Empty;

    // =========================================================================
    // PARSED COMPONENTS
    // =========================================================================

    /// <summary>
    /// Entity type prefix (3-5 chars): ASM, RSK, CTL, EVD, etc.
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string Prefix { get; set; } = string.Empty;

    /// <summary>
    /// Tenant code (3-6 uppercase alphanumeric)
    /// </summary>
    [Required]
    [MaxLength(6)]
    public string TenantCode { get; set; } = string.Empty;

    /// <summary>
    /// GRC lifecycle stage (1-6)
    /// 1=Assessment, 2=Risk, 3=Compliance, 4=Resilience, 5=Excellence, 6=Sustainability
    /// 0=Cross-stage entities (Evidence, Control, Framework, etc.)
    /// </summary>
    [Range(0, 6)]
    public int Stage { get; set; }

    /// <summary>
    /// Issuance year
    /// </summary>
    [Range(2020, 2100)]
    public int Year { get; set; }

    /// <summary>
    /// Sequence number within prefix+tenant+stage+year scope
    /// </summary>
    [Range(1, 999999)]
    public int Sequence { get; set; }

    /// <summary>
    /// Version number (1-99)
    /// </summary>
    [Range(1, 99)]
    public int Version { get; set; } = 1;

    // =========================================================================
    // ENTITY REFERENCE
    // =========================================================================

    /// <summary>
    /// Entity type name (e.g., Assessment, Risk, Evidence)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Entity ID this serial code references
    /// </summary>
    public Guid EntityId { get; set; }

    // =========================================================================
    // STATUS
    // =========================================================================

    /// <summary>
    /// Serial code status: active, superseded, void, reserved
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "active";

    /// <summary>
    /// Reason for void/supersede (if applicable)
    /// </summary>
    [MaxLength(500)]
    public string? StatusReason { get; set; }

    // =========================================================================
    // METADATA
    // =========================================================================

    /// <summary>
    /// JSON metadata for additional context
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// Reference to previous version code (if this is a new version)
    /// </summary>
    [MaxLength(35)]
    public string? PreviousVersionCode { get; set; }

    // =========================================================================
    // AUDIT
    // =========================================================================

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(256)]
    public string CreatedBy { get; set; } = "System";

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(256)]
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Row version for concurrency control
    /// </summary>
    [Timestamp]
    public byte[]? RowVersion { get; set; }
}

/// <summary>
/// Serial Code Reservation - Temporary reservation for batch operations
/// </summary>
[Table("SerialCodeReservations")]
public class SerialCodeReservation
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Reserved serial code
    /// </summary>
    [Required]
    [MaxLength(35)]
    public string ReservedCode { get; set; } = string.Empty;

    // =========================================================================
    // PARSED COMPONENTS
    // =========================================================================

    [Required]
    [MaxLength(10)]
    public string Prefix { get; set; } = string.Empty;

    [Required]
    [MaxLength(6)]
    public string TenantCode { get; set; } = string.Empty;

    [Range(0, 6)]
    public int Stage { get; set; }

    [Range(2020, 2100)]
    public int Year { get; set; }

    [Range(1, 999999)]
    public int Sequence { get; set; }

    // =========================================================================
    // STATUS
    // =========================================================================

    /// <summary>
    /// Reservation status: reserved, confirmed, expired, cancelled
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "reserved";

    /// <summary>
    /// When the reservation expires
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// When the reservation was confirmed (if applicable)
    /// </summary>
    public DateTime? ConfirmedAt { get; set; }

    /// <summary>
    /// When the reservation was cancelled (if applicable)
    /// </summary>
    public DateTime? CancelledAt { get; set; }

    // =========================================================================
    // AUDIT
    // =========================================================================

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(256)]
    public string CreatedBy { get; set; } = "System";
}

/// <summary>
/// Extended Serial Counter with stage support
/// Extends the existing SerialCounter to support the new format
/// </summary>
[Table("SerialSequenceCounters")]
public class SerialSequenceCounter
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Entity type prefix
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string Prefix { get; set; } = string.Empty;

    /// <summary>
    /// Tenant code
    /// </summary>
    [Required]
    [MaxLength(6)]
    public string TenantCode { get; set; } = string.Empty;

    /// <summary>
    /// GRC lifecycle stage (0-6)
    /// </summary>
    [Range(0, 6)]
    public int Stage { get; set; }

    /// <summary>
    /// Year
    /// </summary>
    [Range(2020, 2100)]
    public int Year { get; set; }

    /// <summary>
    /// Current sequence value (last used)
    /// </summary>
    public int CurrentSequence { get; set; } = 0;

    /// <summary>
    /// Last updated timestamp
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Row version for concurrency control
    /// </summary>
    [Timestamp]
    public byte[]? RowVersion { get; set; }
}

/// <summary>
/// Serial Code Prefix Configuration
/// Defines the mapping between entity types and prefixes
/// </summary>
public static class SerialCodePrefixes
{
    // Stage 1: Assessment & Exploration
    public const string Assessment = "ASM";
    public const string AssessmentQuestion = "ASM-Q";
    public const string AssessmentFinding = "ASM-F";

    // Stage 2: Risk Analysis
    public const string Risk = "RSK";
    public const string RiskTreatment = "RSK-T";
    public const string RiskAsset = "RSK-A";

    // Stage 3: Compliance Monitoring
    public const string Compliance = "CMP";
    public const string ComplianceRequirement = "CMP-R";
    public const string ComplianceGap = "CMP-G";

    // Stage 4: Resilience Building
    public const string Resilience = "RES";
    public const string RecoveryPlan = "RES-P";
    public const string ResilienceTest = "RES-T";

    // Stage 5: Excellence & Benchmarking
    public const string Excellence = "EXC";
    public const string Benchmark = "EXC-B";
    public const string Improvement = "EXC-I";

    // Stage 6: Continuous Sustainability
    public const string Sustainability = "SUS";
    public const string Kpi = "SUS-K";
    public const string Certification = "SUS-C";

    // Cross-Stage Entities (Stage 0)
    public const string Control = "CTL";
    public const string ControlTest = "CTL-T";
    public const string Evidence = "EVD";
    public const string EvidenceRequest = "EVD-R";
    public const string Framework = "FWK";
    public const string FrameworkRequirement = "FWK-R";
    public const string Workflow = "WFL";
    public const string WorkflowTask = "WFL-T";
    public const string Approval = "APR";
    public const string Audit = "AUD";
    public const string Report = "RPT";
    public const string Attestation = "ATT";
    public const string Policy = "POL";
    public const string User = "USR";
    public const string Tenant = "TEN";
    public const string Vendor = "VND";
    public const string ActionPlan = "ACT";

    /// <summary>
    /// Get prefix for entity type
    /// </summary>
    public static string GetPrefix(string entityType)
    {
        return entityType.ToLowerInvariant() switch
        {
            // Stage 1
            "assessment" => Assessment,
            "assessmentquestion" or "assessment_question" => AssessmentQuestion,
            "assessmentfinding" or "assessment_finding" => AssessmentFinding,

            // Stage 2
            "risk" => Risk,
            "risktreatment" or "risk_treatment" => RiskTreatment,
            "riskasset" or "risk_asset" => RiskAsset,

            // Stage 3
            "compliance" => Compliance,
            "compliancerequirement" or "compliance_requirement" => ComplianceRequirement,
            "compliancegap" or "compliance_gap" => ComplianceGap,

            // Stage 4
            "resilience" => Resilience,
            "recoveryplan" or "recovery_plan" => RecoveryPlan,
            "resiliencetest" or "resilience_test" => ResilienceTest,

            // Stage 5
            "excellence" => Excellence,
            "benchmark" => Benchmark,
            "improvement" => Improvement,

            // Stage 6
            "sustainability" => Sustainability,
            "kpi" => Kpi,
            "certification" => Certification,

            // Cross-Stage
            "control" => Control,
            "controltest" or "control_test" => ControlTest,
            "evidence" => Evidence,
            "evidencerequest" or "evidence_request" => EvidenceRequest,
            "framework" => Framework,
            "frameworkrequirement" or "framework_requirement" => FrameworkRequirement,
            "workflow" => Workflow,
            "workflowtask" or "workflow_task" => WorkflowTask,
            "approval" => Approval,
            "audit" => Audit,
            "report" => Report,
            "attestation" => Attestation,
            "policy" => Policy,
            "user" => User,
            "tenant" => Tenant,
            "vendor" => Vendor,
            "actionplan" or "action_plan" => ActionPlan,

            _ => entityType.Length >= 3
                ? entityType[..3].ToUpperInvariant()
                : entityType.ToUpperInvariant()
        };
    }

    /// <summary>
    /// Get stage number for prefix
    /// </summary>
    public static int GetStage(string prefix)
    {
        return prefix.ToUpperInvariant() switch
        {
            // Stage 1: Assessment
            "ASM" or "ASM-Q" or "ASM-F" => 1,

            // Stage 2: Risk
            "RSK" or "RSK-T" or "RSK-A" => 2,

            // Stage 3: Compliance
            "CMP" or "CMP-R" or "CMP-G" => 3,

            // Stage 4: Resilience
            "RES" or "RES-P" or "RES-T" => 4,

            // Stage 5: Excellence
            "EXC" or "EXC-B" or "EXC-I" => 5,

            // Stage 6: Sustainability
            "SUS" or "SUS-K" or "SUS-C" => 6,

            // Cross-Stage entities
            _ => 0
        };
    }

    /// <summary>
    /// Validate tenant code format
    /// </summary>
    public static bool IsValidTenantCode(string tenantCode)
    {
        if (string.IsNullOrWhiteSpace(tenantCode))
            return false;

        if (tenantCode.Length < 3 || tenantCode.Length > 6)
            return false;

        // Must be uppercase alphanumeric
        foreach (char c in tenantCode)
        {
            if (!char.IsLetterOrDigit(c) || (char.IsLetter(c) && !char.IsUpper(c)))
                return false;
        }

        // Check reserved codes
        string[] reserved = { "SYS", "ADM", "ROOT", "NULL", "TEST" };
        if (Array.Exists(reserved, r => r.Equals(tenantCode, StringComparison.OrdinalIgnoreCase)))
            return false;

        return true;
    }
}
