using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities;

#region ERP System Configuration

/// <summary>
/// ERP System Configuration - Connection to SAP, Oracle, Dynamics, etc.
/// </summary>
public class ERPSystemConfig : BaseEntity
{
    public Guid TenantId { get; set; }

    [ForeignKey("TenantId")]
    public virtual Tenant Tenant { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string SystemCode { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// ERP type: SAP_S4, SAP_ECC, Oracle_EBS, Oracle_Cloud, Dynamics365, Other
    /// </summary>
    [Required]
    [StringLength(30)]
    public string ERPType { get; set; } = string.Empty;

    /// <summary>
    /// Environment: Production, UAT, Dev
    /// </summary>
    [StringLength(20)]
    public string Environment { get; set; } = "Production";

    /// <summary>
    /// Connection method: API, ODATA, RFC, DirectDB, DataWarehouse
    /// </summary>
    [Required]
    [StringLength(30)]
    public string ConnectionMethod { get; set; } = "API";

    /// <summary>
    /// Connection configuration (encrypted JSON)
    /// </summary>
    public string? ConnectionConfigJson { get; set; }

    /// <summary>
    /// Service account used
    /// </summary>
    [StringLength(100)]
    public string? ServiceAccountId { get; set; }

    /// <summary>
    /// Is this a read-only replica/DW feed?
    /// </summary>
    public bool IsReadOnlyReplica { get; set; } = true;

    /// <summary>
    /// Connection status: Connected, Disconnected, Error
    /// </summary>
    [StringLength(20)]
    public string ConnectionStatus { get; set; } = "Disconnected";

    /// <summary>
    /// Last health check
    /// </summary>
    public DateTime? LastHealthCheck { get; set; }

    /// <summary>
    /// Modules available: FI, MM, SD, HR, etc. (comma-separated)
    /// </summary>
    [StringLength(255)]
    public string? AvailableModules { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation
    public virtual ICollection<ERPExtractConfig> ExtractConfigs { get; set; } = new List<ERPExtractConfig>();
}

/// <summary>
/// ERP Extract Configuration - Defines what data to extract from ERP
/// </summary>
public class ERPExtractConfig : BaseEntity
{
    public Guid ERPSystemId { get; set; }

    [ForeignKey("ERPSystemId")]
    public virtual ERPSystemConfig ERPSystem { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string ExtractCode { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Process area: P2P, R2R, O2C, IAM, VendorMaster, JournalEntry
    /// </summary>
    [Required]
    [StringLength(30)]
    public string ProcessArea { get; set; } = string.Empty;

    /// <summary>
    /// Data source (table, view, API endpoint)
    /// </summary>
    [Required]
    [StringLength(500)]
    public string DataSource { get; set; } = string.Empty;

    /// <summary>
    /// Query/filter expression
    /// </summary>
    public string? QueryExpression { get; set; }

    /// <summary>
    /// Field mappings to canonical model (JSON)
    /// </summary>
    public string? FieldMappingsJson { get; set; }

    /// <summary>
    /// Extract frequency: RealTime, Hourly, Daily, Weekly, Monthly
    /// </summary>
    [StringLength(20)]
    public string Frequency { get; set; } = "Daily";

    /// <summary>
    /// Cron expression for scheduling
    /// </summary>
    [StringLength(50)]
    public string? CronExpression { get; set; }

    /// <summary>
    /// Last extract timestamp
    /// </summary>
    public DateTime? LastExtractAt { get; set; }

    /// <summary>
    /// Last extract record count
    /// </summary>
    public int? LastExtractRecordCount { get; set; }

    /// <summary>
    /// Next scheduled extract
    /// </summary>
    public DateTime? NextExtractAt { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation
    public virtual ICollection<ERPExtractExecution> Executions { get; set; } = new List<ERPExtractExecution>();
}

/// <summary>
/// ERP Extract Execution - Record of each extract run
/// </summary>
public class ERPExtractExecution : BaseEntity
{
    public Guid ExtractConfigId { get; set; }

    [ForeignKey("ExtractConfigId")]
    public virtual ERPExtractConfig ExtractConfig { get; set; } = null!;

    /// <summary>
    /// Execution status: Running, Completed, Failed
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Running";

    /// <summary>
    /// Period start for this extract
    /// </summary>
    public DateTime PeriodStart { get; set; }

    /// <summary>
    /// Period end for this extract
    /// </summary>
    public DateTime PeriodEnd { get; set; }

    /// <summary>
    /// Records extracted
    /// </summary>
    public int RecordsExtracted { get; set; } = 0;

    /// <summary>
    /// Records passed to CCM
    /// </summary>
    public int RecordsPassedToCCM { get; set; } = 0;

    /// <summary>
    /// Storage location of extract file
    /// </summary>
    [StringLength(500)]
    public string? StorageLocation { get; set; }

    /// <summary>
    /// File hash for integrity
    /// </summary>
    [StringLength(128)]
    public string? FileHash { get; set; }

    /// <summary>
    /// Duration in seconds
    /// </summary>
    public int? DurationSeconds { get; set; }

    /// <summary>
    /// Error message if failed
    /// </summary>
    [StringLength(2000)]
    public string? ErrorMessage { get; set; }

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }
}

#endregion

#region Continuous Controls Monitoring (CCM)

/// <summary>
/// CCM Control Test Definition - Control test as code
/// Defines population, rule, threshold, frequency for each control
/// </summary>
public class CCMControlTest : BaseEntity
{
    public Guid TenantId { get; set; }

    [ForeignKey("TenantId")]
    public virtual Tenant Tenant { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string TestCode { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [StringLength(255)]
    public string? NameAr { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Related canonical control
    /// </summary>
    public Guid? ControlId { get; set; }

    [ForeignKey("ControlId")]
    public virtual CanonicalControl? Control { get; set; }

    /// <summary>
    /// Process area: P2P, R2R, O2C, IAM, VendorMaster, SoD
    /// </summary>
    [Required]
    [StringLength(30)]
    public string ProcessArea { get; set; } = string.Empty;

    /// <summary>
    /// Test category: Access, SoD, Approval, DataIntegrity, Fraud
    /// </summary>
    [Required]
    [StringLength(30)]
    public string TestCategory { get; set; } = string.Empty;

    /// <summary>
    /// ERP system this test applies to
    /// </summary>
    public Guid? ERPSystemId { get; set; }

    [ForeignKey("ERPSystemId")]
    public virtual ERPSystemConfig? ERPSystem { get; set; }

    /// <summary>
    /// Population definition (what transactions/users are in scope)
    /// </summary>
    [Required]
    public string PopulationDefinitionJson { get; set; } = "{}";

    /// <summary>
    /// Rule definition (what "pass" means) - SQL-like or expression
    /// </summary>
    [Required]
    public string RuleDefinitionJson { get; set; } = "{}";

    /// <summary>
    /// Threshold settings (JSON)
    /// </summary>
    public string? ThresholdSettingsJson { get; set; }

    /// <summary>
    /// Test frequency: Daily, Weekly, Monthly, Quarterly
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Frequency { get; set; } = "Daily";

    /// <summary>
    /// Risk level: Critical, High, Medium, Low
    /// </summary>
    [StringLength(20)]
    public string RiskLevel { get; set; } = "High";

    /// <summary>
    /// Exception owner role code
    /// </summary>
    [StringLength(50)]
    public string? ExceptionOwnerRoleCode { get; set; }

    /// <summary>
    /// SLA for exception resolution (days)
    /// </summary>
    public int ExceptionSLADays { get; set; } = 5;

    /// <summary>
    /// Auto-create ITSM ticket for exceptions?
    /// </summary>
    public bool AutoCreateTicket { get; set; } = true;

    /// <summary>
    /// Send Teams notification for exceptions?
    /// </summary>
    public bool SendTeamsNotification { get; set; } = true;

    /// <summary>
    /// Last test execution
    /// </summary>
    public DateTime? LastExecutionAt { get; set; }

    /// <summary>
    /// Last pass rate
    /// </summary>
    public decimal? LastPassRate { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation
    public virtual ICollection<CCMTestExecution> Executions { get; set; } = new List<CCMTestExecution>();
}

/// <summary>
/// CCM Test Execution - Record of each control test run
/// </summary>
public class CCMTestExecution : BaseEntity
{
    public Guid TestId { get; set; }

    [ForeignKey("TestId")]
    public virtual CCMControlTest Test { get; set; } = null!;

    /// <summary>
    /// Execution status: Running, Completed, Failed
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Running";

    /// <summary>
    /// Test period start
    /// </summary>
    public DateTime PeriodStart { get; set; }

    /// <summary>
    /// Test period end
    /// </summary>
    public DateTime PeriodEnd { get; set; }

    /// <summary>
    /// Population count (total items tested)
    /// </summary>
    public int PopulationCount { get; set; } = 0;

    /// <summary>
    /// Items passed
    /// </summary>
    public int PassedCount { get; set; } = 0;

    /// <summary>
    /// Items failed (exceptions)
    /// </summary>
    public int FailedCount { get; set; } = 0;

    /// <summary>
    /// Pass rate percentage
    /// </summary>
    public decimal PassRate { get; set; } = 0;

    /// <summary>
    /// Result status: Pass, Fail, Warning, Error
    /// </summary>
    [Required]
    [StringLength(20)]
    public string ResultStatus { get; set; } = "Pass";

    /// <summary>
    /// Evidence snapshot location
    /// </summary>
    [StringLength(500)]
    public string? EvidenceSnapshotLocation { get; set; }

    /// <summary>
    /// Evidence snapshot hash
    /// </summary>
    [StringLength(128)]
    public string? EvidenceSnapshotHash { get; set; }

    /// <summary>
    /// Duration in seconds
    /// </summary>
    public int? DurationSeconds { get; set; }

    /// <summary>
    /// Error message if failed
    /// </summary>
    [StringLength(2000)]
    public string? ErrorMessage { get; set; }

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }

    // Navigation
    public virtual ICollection<CCMException> Exceptions { get; set; } = new List<CCMException>();
}

/// <summary>
/// CCM Exception - Individual exception from control test
/// </summary>
public class CCMException : BaseEntity
{
    public Guid TestExecutionId { get; set; }

    [ForeignKey("TestExecutionId")]
    public virtual CCMTestExecution TestExecution { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string ExceptionCode { get; set; } = string.Empty;

    /// <summary>
    /// Exception type: Violation, Anomaly, Missing, Override
    /// </summary>
    [Required]
    [StringLength(30)]
    public string ExceptionType { get; set; } = "Violation";

    /// <summary>
    /// Severity: Critical, High, Medium, Low
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Severity { get; set; } = "High";

    /// <summary>
    /// Exception summary
    /// </summary>
    [Required]
    [StringLength(500)]
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description
    /// </summary>
    [StringLength(2000)]
    public string? Details { get; set; }

    /// <summary>
    /// Affected entity (user, vendor, document, etc.)
    /// </summary>
    [StringLength(255)]
    public string? AffectedEntity { get; set; }

    /// <summary>
    /// Affected entity ID in ERP
    /// </summary>
    [StringLength(100)]
    public string? AffectedEntityId { get; set; }

    /// <summary>
    /// Transaction/document reference
    /// </summary>
    [StringLength(100)]
    public string? TransactionReference { get; set; }

    /// <summary>
    /// Amount involved (if financial)
    /// </summary>
    public decimal? AmountInvolved { get; set; }

    /// <summary>
    /// Currency
    /// </summary>
    [StringLength(10)]
    public string? Currency { get; set; }

    /// <summary>
    /// Raw data/evidence (JSON)
    /// </summary>
    public string? RawDataJson { get; set; }

    /// <summary>
    /// Status: Open, InProgress, Resolved, FalsePositive, Escalated
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Open";

    /// <summary>
    /// Assigned owner
    /// </summary>
    [StringLength(100)]
    public string? AssignedToId { get; set; }

    [StringLength(255)]
    public string? AssignedToName { get; set; }

    /// <summary>
    /// ITSM ticket ID
    /// </summary>
    [StringLength(100)]
    public string? ITSMTicketId { get; set; }

    /// <summary>
    /// ITSM ticket URL
    /// </summary>
    [StringLength(500)]
    public string? ITSMTicketUrl { get; set; }

    /// <summary>
    /// Teams notification sent?
    /// </summary>
    public bool TeamsNotificationSent { get; set; } = false;

    /// <summary>
    /// Due date for resolution
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Resolution notes
    /// </summary>
    [StringLength(2000)]
    public string? ResolutionNotes { get; set; }

    /// <summary>
    /// Resolved by
    /// </summary>
    [StringLength(100)]
    public string? ResolvedBy { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
}

#endregion

#region Segregation of Duties (SoD)

/// <summary>
/// SoD Rule Definition - Defines conflicting access combinations
/// </summary>
public class SoDRuleDefinition : BaseEntity
{
    public Guid TenantId { get; set; }

    [ForeignKey("TenantId")]
    public virtual Tenant Tenant { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string RuleCode { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Process area: P2P, R2R, O2C, HR
    /// </summary>
    [Required]
    [StringLength(30)]
    public string ProcessArea { get; set; } = string.Empty;

    /// <summary>
    /// Risk level: Critical, High, Medium, Low
    /// </summary>
    [Required]
    [StringLength(20)]
    public string RiskLevel { get; set; } = "High";

    /// <summary>
    /// Function 1 (conflicting with Function 2)
    /// </summary>
    [Required]
    [StringLength(255)]
    public string Function1 { get; set; } = string.Empty;

    /// <summary>
    /// Function 1 description
    /// </summary>
    [StringLength(500)]
    public string? Function1Description { get; set; }

    /// <summary>
    /// Function 1 access patterns (JSON array of roles/transactions)
    /// </summary>
    public string? Function1AccessPatternsJson { get; set; }

    /// <summary>
    /// Function 2 (conflicting with Function 1)
    /// </summary>
    [Required]
    [StringLength(255)]
    public string Function2 { get; set; } = string.Empty;

    /// <summary>
    /// Function 2 description
    /// </summary>
    [StringLength(500)]
    public string? Function2Description { get; set; }

    /// <summary>
    /// Function 2 access patterns (JSON array of roles/transactions)
    /// </summary>
    public string? Function2AccessPatternsJson { get; set; }

    /// <summary>
    /// Business risk if both functions held
    /// </summary>
    [StringLength(1000)]
    public string? BusinessRiskDescription { get; set; }

    /// <summary>
    /// Recommended mitigating controls
    /// </summary>
    [StringLength(1000)]
    public string? MitigatingControls { get; set; }

    /// <summary>
    /// ERP system this rule applies to
    /// </summary>
    public Guid? ERPSystemId { get; set; }

    [ForeignKey("ERPSystemId")]
    public virtual ERPSystemConfig? ERPSystem { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// SoD Conflict - Detected SoD violation
/// </summary>
public class SoDConflict : BaseEntity
{
    public Guid TenantId { get; set; }

    [ForeignKey("TenantId")]
    public virtual Tenant Tenant { get; set; } = null!;

    public Guid RuleId { get; set; }

    [ForeignKey("RuleId")]
    public virtual SoDRuleDefinition Rule { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string ConflictCode { get; set; } = string.Empty;

    /// <summary>
    /// User with conflict
    /// </summary>
    [Required]
    [StringLength(100)]
    public string UserId { get; set; } = string.Empty;

    [StringLength(255)]
    public string? UserName { get; set; }

    /// <summary>
    /// User's department
    /// </summary>
    [StringLength(100)]
    public string? UserDepartment { get; set; }

    /// <summary>
    /// Function 1 access details (JSON)
    /// </summary>
    public string? Function1AccessJson { get; set; }

    /// <summary>
    /// Function 2 access details (JSON)
    /// </summary>
    public string? Function2AccessJson { get; set; }

    /// <summary>
    /// Status: Open, Mitigated, Accepted, Removed, FalsePositive
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Open";

    /// <summary>
    /// Has mitigating control?
    /// </summary>
    public bool HasMitigatingControl { get; set; } = false;

    /// <summary>
    /// Mitigating control description
    /// </summary>
    [StringLength(1000)]
    public string? MitigatingControlDescription { get; set; }

    /// <summary>
    /// Risk acceptance owner (if accepted)
    /// </summary>
    [StringLength(100)]
    public string? RiskAcceptanceOwnerId { get; set; }

    [StringLength(255)]
    public string? RiskAcceptanceOwnerName { get; set; }

    /// <summary>
    /// Acceptance expiry date
    /// </summary>
    public DateTime? AcceptanceExpiryDate { get; set; }

    /// <summary>
    /// ITSM ticket ID
    /// </summary>
    [StringLength(100)]
    public string? ITSMTicketId { get; set; }

    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ResolvedAt { get; set; }
}

#endregion

#region Auto-Tagged Evidence

/// <summary>
/// Auto-Tagged Evidence - Evidence with canonical metadata for auto-linking
/// </summary>
public class AutoTaggedEvidence : BaseEntity
{
    public Guid TenantId { get; set; }

    [ForeignKey("TenantId")]
    public virtual Tenant Tenant { get; set; } = null!;

    /// <summary>
    /// Control ID (mandatory for auto-linking)
    /// </summary>
    public Guid ControlId { get; set; }

    [ForeignKey("ControlId")]
    public virtual CanonicalControl Control { get; set; } = null!;

    /// <summary>
    /// Process: P2P, R2R, IAM, VendorMaster, SoD
    /// </summary>
    [Required]
    [StringLength(30)]
    public string Process { get; set; } = string.Empty;

    /// <summary>
    /// System (ERP instance code)
    /// </summary>
    [Required]
    [StringLength(50)]
    public string System { get; set; } = string.Empty;

    /// <summary>
    /// Period: Month/Quarter or specific date
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Period { get; set; } = string.Empty;

    /// <summary>
    /// Period start date
    /// </summary>
    public DateTime PeriodStart { get; set; }

    /// <summary>
    /// Period end date
    /// </summary>
    public DateTime PeriodEnd { get; set; }

    /// <summary>
    /// Owner (user ID)
    /// </summary>
    [StringLength(100)]
    public string? OwnerId { get; set; }

    [StringLength(255)]
    public string? OwnerName { get; set; }

    /// <summary>
    /// Evidence type: Extract, Report, Log, Screenshot, Attestation
    /// </summary>
    [Required]
    [StringLength(30)]
    public string EvidenceType { get; set; } = "Extract";

    /// <summary>
    /// Title
    /// </summary>
    [Required]
    [StringLength(255)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Description
    /// </summary>
    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Storage location
    /// </summary>
    [Required]
    [StringLength(500)]
    public string StorageLocation { get; set; } = string.Empty;

    /// <summary>
    /// File hash/checksum for integrity
    /// </summary>
    [StringLength(128)]
    public string? FileHash { get; set; }

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long? FileSizeBytes { get; set; }

    /// <summary>
    /// MIME type
    /// </summary>
    [StringLength(100)]
    public string? MimeType { get; set; }

    /// <summary>
    /// Version number
    /// </summary>
    public int VersionNumber { get; set; } = 1;

    /// <summary>
    /// Is current version?
    /// </summary>
    public bool IsCurrent { get; set; } = true;

    /// <summary>
    /// Source: Automated, Manual, Import
    /// </summary>
    [StringLength(20)]
    public string Source { get; set; } = "Automated";

    /// <summary>
    /// CCM test execution that generated this evidence
    /// </summary>
    public Guid? CCMTestExecutionId { get; set; }

    [ForeignKey("CCMTestExecutionId")]
    public virtual CCMTestExecution? CCMTestExecution { get; set; }

    /// <summary>
    /// Status: Collected, Reviewed, Approved, Rejected
    /// </summary>
    [StringLength(20)]
    public string Status { get; set; } = "Collected";

    /// <summary>
    /// Reviewed by
    /// </summary>
    [StringLength(100)]
    public string? ReviewedBy { get; set; }

    public DateTime? ReviewedAt { get; set; }

    /// <summary>
    /// Retention until
    /// </summary>
    public DateTime? RetentionUntil { get; set; }

    /// <summary>
    /// Capture timestamp (immutable)
    /// </summary>
    public DateTime CapturedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Captured by (system or user)
    /// </summary>
    [StringLength(100)]
    public string CapturedBy { get; set; } = "SYSTEM";
}

#endregion
