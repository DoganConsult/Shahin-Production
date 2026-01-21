using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities;

#region KRI/KPI Dashboard

/// <summary>
/// Key Risk Indicator (KRI) / Key Performance Indicator (KPI) Definition
/// These are the metrics that make the system "self-driving"
/// </summary>
public class RiskIndicator : BaseEntity
{
    public Guid TenantId { get; set; }

    [ForeignKey("TenantId")]
    public virtual Tenant Tenant { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string IndicatorCode { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [StringLength(255)]
    public string? NameAr { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Indicator type: KRI, KPI, KCI (Control), Metric
    /// </summary>
    [Required]
    [StringLength(20)]
    public string IndicatorType { get; set; } = "KRI";

    /// <summary>
    /// Category: Cyber, Technology, ThirdParty, Operational, Data, Continuity
    /// </summary>
    [Required]
    [StringLength(30)]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Related control ID (if this KRI monitors a specific control)
    /// </summary>
    public Guid? ControlId { get; set; }

    [ForeignKey("ControlId")]
    public virtual CanonicalControl? Control { get; set; }

    /// <summary>
    /// Data source: SIEM, VulnScanner, IAM, ITSM, Backup, Manual
    /// </summary>
    [StringLength(50)]
    public string? DataSource { get; set; }

    /// <summary>
    /// Measurement frequency: Continuous, Daily, Weekly, Monthly
    /// </summary>
    [Required]
    [StringLength(20)]
    public string MeasurementFrequency { get; set; } = "Daily";

    /// <summary>
    /// Unit of measure: Percentage, Count, Days, Hours, Score
    /// </summary>
    [StringLength(20)]
    public string UnitOfMeasure { get; set; } = "Percentage";

    /// <summary>
    /// Target value (green)
    /// </summary>
    public decimal? TargetValue { get; set; }

    /// <summary>
    /// Warning threshold (amber)
    /// </summary>
    public decimal? WarningThreshold { get; set; }

    /// <summary>
    /// Critical threshold (red)
    /// </summary>
    public decimal? CriticalThreshold { get; set; }

    /// <summary>
    /// Direction: HigherIsBetter, LowerIsBetter
    /// </summary>
    [StringLength(20)]
    public string Direction { get; set; } = "HigherIsBetter";

    /// <summary>
    /// Owner role code
    /// </summary>
    [StringLength(50)]
    public string? OwnerRoleCode { get; set; }

    /// <summary>
    /// Auto-escalation enabled?
    /// </summary>
    public bool AutoEscalate { get; set; } = true;

    /// <summary>
    /// Escalation threshold (days in breach before escalation)
    /// </summary>
    public int EscalationDays { get; set; } = 5;

    public bool IsActive { get; set; } = true;

    // Navigation
    public virtual ICollection<RiskIndicatorMeasurement> Measurements { get; set; } = new List<RiskIndicatorMeasurement>();
    public virtual ICollection<RiskIndicatorAlert> Alerts { get; set; } = new List<RiskIndicatorAlert>();
}

/// <summary>
/// KRI/KPI Measurement - Point-in-time value capture
/// </summary>
public class RiskIndicatorMeasurement : BaseEntity
{
    public Guid IndicatorId { get; set; }

    [ForeignKey("IndicatorId")]
    public virtual RiskIndicator Indicator { get; set; } = null!;

    /// <summary>
    /// Measurement period start
    /// </summary>
    public DateTime PeriodStart { get; set; }

    /// <summary>
    /// Measurement period end
    /// </summary>
    public DateTime PeriodEnd { get; set; }

    /// <summary>
    /// Actual measured value
    /// </summary>
    public decimal Value { get; set; }

    /// <summary>
    /// Target for this period
    /// </summary>
    public decimal? Target { get; set; }

    /// <summary>
    /// RAG status: Green, Amber, Red
    /// </summary>
    [Required]
    [StringLength(10)]
    public string Status { get; set; } = "Green";

    /// <summary>
    /// Trend: Improving, Stable, Declining
    /// </summary>
    [StringLength(20)]
    public string? Trend { get; set; }

    /// <summary>
    /// Source of measurement: Automated, Manual, Import
    /// </summary>
    [StringLength(20)]
    public string Source { get; set; } = "Automated";

    /// <summary>
    /// Raw data/evidence reference
    /// </summary>
    public string? RawDataJson { get; set; }

    /// <summary>
    /// Commentary/notes
    /// </summary>
    [StringLength(1000)]
    public string? Commentary { get; set; }

    public DateTime MeasuredAt { get; set; } = DateTime.UtcNow;

    [StringLength(100)]
    public string? MeasuredBy { get; set; }
}

/// <summary>
/// KRI/KPI Alert - Triggered when thresholds are breached
/// </summary>
public class RiskIndicatorAlert : BaseEntity
{
    public Guid IndicatorId { get; set; }

    [ForeignKey("IndicatorId")]
    public virtual RiskIndicator Indicator { get; set; } = null!;

    public Guid? MeasurementId { get; set; }

    [ForeignKey("MeasurementId")]
    public virtual RiskIndicatorMeasurement? Measurement { get; set; }

    /// <summary>
    /// Alert severity: Warning, Critical
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Severity { get; set; } = "Warning";

    /// <summary>
    /// Alert message
    /// </summary>
    [Required]
    [StringLength(500)]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Threshold that was breached
    /// </summary>
    public decimal? ThresholdValue { get; set; }

    /// <summary>
    /// Actual value that triggered alert
    /// </summary>
    public decimal? ActualValue { get; set; }

    /// <summary>
    /// Alert status: Open, Acknowledged, Resolved, Escalated
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Open";

    /// <summary>
    /// Days in breach
    /// </summary>
    public int DaysInBreach { get; set; } = 0;

    /// <summary>
    /// Has been escalated?
    /// </summary>
    public bool IsEscalated { get; set; } = false;

    public DateTime? EscalatedAt { get; set; }

    [StringLength(100)]
    public string? EscalatedTo { get; set; }

    /// <summary>
    /// Assigned owner
    /// </summary>
    [StringLength(100)]
    public string? AssignedTo { get; set; }

    public DateTime? AcknowledgedAt { get; set; }

    [StringLength(100)]
    public string? AcknowledgedBy { get; set; }

    public DateTime? ResolvedAt { get; set; }

    [StringLength(100)]
    public string? ResolvedBy { get; set; }

    [StringLength(1000)]
    public string? ResolutionNotes { get; set; }

    public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;
}

#endregion

#region Important Business Services (IBS)

/// <summary>
/// Important Business Service - What must stay running
/// </summary>
public class ImportantBusinessService : BaseEntity
{
    public Guid TenantId { get; set; }

    [ForeignKey("TenantId")]
    public virtual Tenant Tenant { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string ServiceCode { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [StringLength(255)]
    public string? NameAr { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Service category: Customer-Facing, Internal, Regulatory, Payment
    /// </summary>
    [StringLength(30)]
    public string Category { get; set; } = "Customer-Facing";

    /// <summary>
    /// Criticality tier: Tier1, Tier2, Tier3
    /// </summary>
    [Required]
    [StringLength(10)]
    public string CriticalityTier { get; set; } = "Tier2";

    /// <summary>
    /// Recovery Time Objective (hours)
    /// </summary>
    public int RTO_Hours { get; set; } = 4;

    /// <summary>
    /// Recovery Point Objective (hours)
    /// </summary>
    public int RPO_Hours { get; set; } = 1;

    /// <summary>
    /// Maximum Tolerable Downtime (hours)
    /// </summary>
    public int MTD_Hours { get; set; } = 24;

    /// <summary>
    /// Service owner
    /// </summary>
    [StringLength(100)]
    public string? ServiceOwnerId { get; set; }

    [StringLength(255)]
    public string? ServiceOwnerName { get; set; }

    /// <summary>
    /// Supporting systems (comma-separated or JSON)
    /// </summary>
    public string? SupportingSystemsJson { get; set; }

    /// <summary>
    /// Dependencies (other IBS codes)
    /// </summary>
    [StringLength(500)]
    public string? Dependencies { get; set; }

    /// <summary>
    /// Last DR test date
    /// </summary>
    public DateTime? LastDRTestDate { get; set; }

    /// <summary>
    /// DR test result: Pass, Partial, Fail
    /// </summary>
    [StringLength(20)]
    public string? LastDRTestResult { get; set; }

    public bool IsActive { get; set; } = true;
}

#endregion

#region Exception Management

/// <summary>
/// Control Exception - When a control cannot be met
/// </summary>
public class ControlException : BaseEntity
{
    public Guid TenantId { get; set; }

    [ForeignKey("TenantId")]
    public virtual Tenant Tenant { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string ExceptionCode { get; set; } = string.Empty;

    /// <summary>
    /// Related control
    /// </summary>
    public Guid ControlId { get; set; }

    [ForeignKey("ControlId")]
    public virtual CanonicalControl Control { get; set; } = null!;

    /// <summary>
    /// Scope of exception (systems, processes, locations)
    /// </summary>
    [Required]
    [StringLength(1000)]
    public string Scope { get; set; } = string.Empty;

    /// <summary>
    /// Reason for exception
    /// </summary>
    [Required]
    [StringLength(2000)]
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Risk impact: High, Medium, Low
    /// </summary>
    [Required]
    [StringLength(10)]
    public string RiskImpact { get; set; } = "Medium";

    /// <summary>
    /// Compensating controls in place
    /// </summary>
    [StringLength(2000)]
    public string? CompensatingControls { get; set; }

    /// <summary>
    /// Remediation plan
    /// </summary>
    [StringLength(2000)]
    public string? RemediationPlan { get; set; }

    /// <summary>
    /// Target remediation date
    /// </summary>
    public DateTime? TargetRemediationDate { get; set; }

    /// <summary>
    /// Exception status: Pending, Approved, Rejected, Expired, Remediated
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// Expiry date (exceptions cannot be permanent)
    /// </summary>
    public DateTime ExpiryDate { get; set; }

    /// <summary>
    /// Days until expiry (calculated)
    /// </summary>
    public int DaysUntilExpiry => (ExpiryDate - DateTime.UtcNow).Days;

    /// <summary>
    /// Is expired?
    /// </summary>
    public bool IsExpired => ExpiryDate < DateTime.UtcNow;

    /// <summary>
    /// Risk acceptance owner (must be senior)
    /// </summary>
    [StringLength(100)]
    public string? RiskAcceptanceOwnerId { get; set; }

    [StringLength(255)]
    public string? RiskAcceptanceOwnerName { get; set; }

    /// <summary>
    /// Approved by
    /// </summary>
    [StringLength(100)]
    public string? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// Last review date
    /// </summary>
    public DateTime? LastReviewDate { get; set; }

    /// <summary>
    /// Next review date
    /// </summary>
    public DateTime? NextReviewDate { get; set; }

    /// <summary>
    /// Review frequency: Monthly, Quarterly
    /// </summary>
    [StringLength(20)]
    public string ReviewFrequency { get; set; } = "Quarterly";

    /// <summary>
    /// Has reminder been sent for upcoming expiry?
    /// </summary>
    public bool ExpiryReminderSent { get; set; } = false;

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    [StringLength(100)]
    public string? RequestedBy { get; set; }
}

#endregion

#region Governance Cadence

/// <summary>
/// Governance Cadence - Scheduled activities (weekly ops, monthly review, quarterly committee)
/// </summary>
public class GovernanceCadence : BaseEntity
{
    public Guid TenantId { get; set; }

    [ForeignKey("TenantId")]
    public virtual Tenant Tenant { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string CadenceCode { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [StringLength(255)]
    public string? NameAr { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Cadence type: Operational, Review, Committee, Audit, Test
    /// </summary>
    [Required]
    [StringLength(30)]
    public string CadenceType { get; set; } = "Operational";

    /// <summary>
    /// Frequency: Daily, Weekly, Biweekly, Monthly, Quarterly, Semiannual, Annual
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Frequency { get; set; } = "Monthly";

    /// <summary>
    /// Day of week (for weekly: 0=Sun, 1=Mon, etc.)
    /// </summary>
    public int? DayOfWeek { get; set; }

    /// <summary>
    /// Day of month (for monthly)
    /// </summary>
    public int? DayOfMonth { get; set; }

    /// <summary>
    /// Week of month (for monthly: 1=First, 2=Second, -1=Last)
    /// </summary>
    public int? WeekOfMonth { get; set; }

    /// <summary>
    /// Time of day (HH:mm)
    /// </summary>
    [StringLength(5)]
    public string? TimeOfDay { get; set; }

    /// <summary>
    /// Timezone
    /// </summary>
    [StringLength(50)]
    public string Timezone { get; set; } = "Asia/Riyadh";

    /// <summary>
    /// Owner role code
    /// </summary>
    [StringLength(50)]
    public string? OwnerRoleCode { get; set; }

    /// <summary>
    /// Participants (role codes, comma-separated)
    /// </summary>
    [StringLength(500)]
    public string? ParticipantRoleCodes { get; set; }

    /// <summary>
    /// Activities to perform (JSON array)
    /// </summary>
    public string? ActivitiesJson { get; set; }

    /// <summary>
    /// Deliverables expected (JSON array)
    /// </summary>
    public string? DeliverablesJson { get; set; }

    /// <summary>
    /// Teams channel for notifications
    /// </summary>
    [StringLength(255)]
    public string? TeamsChannelId { get; set; }

    /// <summary>
    /// Send reminder before (hours)
    /// </summary>
    public int ReminderHoursBefore { get; set; } = 24;

    /// <summary>
    /// Last execution date
    /// </summary>
    public DateTime? LastExecutionDate { get; set; }

    /// <summary>
    /// Next scheduled date
    /// </summary>
    public DateTime? NextScheduledDate { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation
    public virtual ICollection<CadenceExecution> Executions { get; set; } = new List<CadenceExecution>();
}

/// <summary>
/// Cadence Execution - Record of each cadence occurrence
/// </summary>
public class CadenceExecution : BaseEntity
{
    public Guid CadenceId { get; set; }

    [ForeignKey("CadenceId")]
    public virtual GovernanceCadence Cadence { get; set; } = null!;

    /// <summary>
    /// Scheduled date for this execution
    /// </summary>
    public DateTime ScheduledDate { get; set; }

    /// <summary>
    /// Actual execution date
    /// </summary>
    public DateTime? ExecutedDate { get; set; }

    /// <summary>
    /// Status: Scheduled, InProgress, Completed, Missed, Cancelled
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Scheduled";

    /// <summary>
    /// Attendees (JSON array)
    /// </summary>
    public string? AttendeesJson { get; set; }

    /// <summary>
    /// Activities completed (JSON array)
    /// </summary>
    public string? ActivitiesCompletedJson { get; set; }

    /// <summary>
    /// Deliverables produced (JSON array)
    /// </summary>
    public string? DeliverablesProducedJson { get; set; }

    /// <summary>
    /// Action items generated (JSON array)
    /// </summary>
    public string? ActionItemsJson { get; set; }

    /// <summary>
    /// Notes/minutes
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Reminder sent?
    /// </summary>
    public bool ReminderSent { get; set; } = false;

    public DateTime? ReminderSentAt { get; set; }
}

#endregion

#region Automated Evidence Capture

/// <summary>
/// Evidence Source Integration - Connection to automated evidence sources
/// </summary>
public class EvidenceSourceIntegration : BaseEntity
{
    public Guid TenantId { get; set; }

    [ForeignKey("TenantId")]
    public virtual Tenant Tenant { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string SourceCode { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Source type: SIEM, VulnScanner, IAM, ITSM, Backup, EDR, CloudPlatform, API
    /// </summary>
    [Required]
    [StringLength(30)]
    public string SourceType { get; set; } = string.Empty;

    /// <summary>
    /// Connection status: Connected, Disconnected, Error, Pending
    /// </summary>
    [StringLength(20)]
    public string ConnectionStatus { get; set; } = "Pending";

    /// <summary>
    /// Connection configuration (encrypted JSON)
    /// </summary>
    public string? ConnectionConfigJson { get; set; }

    /// <summary>
    /// Last sync date
    /// </summary>
    public DateTime? LastSyncDate { get; set; }

    /// <summary>
    /// Sync frequency: RealTime, Hourly, Daily, Weekly
    /// </summary>
    [StringLength(20)]
    public string SyncFrequency { get; set; } = "Daily";

    /// <summary>
    /// Evidence types this source provides (comma-separated)
    /// </summary>
    [StringLength(500)]
    public string? EvidenceTypesProvided { get; set; }

    /// <summary>
    /// Controls this source provides evidence for (comma-separated IDs)
    /// </summary>
    public string? ControlsCoveredJson { get; set; }

    /// <summary>
    /// KRIs this source feeds (comma-separated codes)
    /// </summary>
    [StringLength(500)]
    public string? KRIsFed { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Captured Evidence - Automatically collected evidence with immutable audit trail
/// </summary>
public class CapturedEvidence : BaseEntity
{
    public Guid TenantId { get; set; }

    [ForeignKey("TenantId")]
    public virtual Tenant Tenant { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string EvidenceCode { get; set; } = string.Empty;

    /// <summary>
    /// Source integration
    /// </summary>
    public Guid? SourceIntegrationId { get; set; }

    [ForeignKey("SourceIntegrationId")]
    public virtual EvidenceSourceIntegration? SourceIntegration { get; set; }

    /// <summary>
    /// Related control
    /// </summary>
    public Guid? ControlId { get; set; }

    [ForeignKey("ControlId")]
    public virtual CanonicalControl? Control { get; set; }

    /// <summary>
    /// Evidence period start
    /// </summary>
    public DateTime PeriodStart { get; set; }

    /// <summary>
    /// Evidence period end
    /// </summary>
    public DateTime PeriodEnd { get; set; }

    /// <summary>
    /// Evidence type code
    /// </summary>
    [Required]
    [StringLength(50)]
    public string EvidenceTypeCode { get; set; } = string.Empty;

    /// <summary>
    /// Title/name
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
    /// Collection method: Automated, Manual, Import
    /// </summary>
    [Required]
    [StringLength(20)]
    public string CollectionMethod { get; set; } = "Automated";

    /// <summary>
    /// Storage location (file path, blob URL, etc.)
    /// </summary>
    [StringLength(500)]
    public string? StorageLocation { get; set; }

    /// <summary>
    /// File hash for integrity verification
    /// </summary>
    [StringLength(128)]
    public string? FileHash { get; set; }

    /// <summary>
    /// Version number
    /// </summary>
    public int VersionNumber { get; set; } = 1;

    /// <summary>
    /// Is this the current version?
    /// </summary>
    public bool IsCurrent { get; set; } = true;

    /// <summary>
    /// Previous version ID (for version chain)
    /// </summary>
    public Guid? PreviousVersionId { get; set; }

    /// <summary>
    /// Tags (JSON array)
    /// </summary>
    public string? TagsJson { get; set; }

    /// <summary>
    /// Status: Collected, Reviewed, Approved, Rejected, Archived
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Collected";

    /// <summary>
    /// Owner
    /// </summary>
    [StringLength(100)]
    public string? OwnerId { get; set; }

    [StringLength(255)]
    public string? OwnerName { get; set; }

    /// <summary>
    /// Reviewer
    /// </summary>
    [StringLength(100)]
    public string? ReviewerId { get; set; }

    public DateTime? ReviewedAt { get; set; }

    /// <summary>
    /// Retention until date
    /// </summary>
    public DateTime? RetentionUntil { get; set; }

    /// <summary>
    /// Immutable capture timestamp
    /// </summary>
    public DateTime CapturedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Captured by (system or user)
    /// </summary>
    [StringLength(100)]
    public string CapturedBy { get; set; } = "SYSTEM";
}

#endregion

#region Teams Integration

/// <summary>
/// Teams Notification Configuration
/// </summary>
public class TeamsNotificationConfig : BaseEntity
{
    public Guid TenantId { get; set; }

    [ForeignKey("TenantId")]
    public virtual Tenant Tenant { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string ConfigCode { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Notification type: Alert, Reminder, Escalation, Approval, Report
    /// </summary>
    [Required]
    [StringLength(30)]
    public string NotificationType { get; set; } = string.Empty;

    /// <summary>
    /// Teams webhook URL
    /// </summary>
    [StringLength(500)]
    public string? WebhookUrl { get; set; }

    /// <summary>
    /// Teams channel ID
    /// </summary>
    [StringLength(255)]
    public string? ChannelId { get; set; }

    /// <summary>
    /// Trigger conditions (JSON)
    /// </summary>
    public string? TriggerConditionsJson { get; set; }

    /// <summary>
    /// Message template
    /// </summary>
    public string? MessageTemplateJson { get; set; }

    /// <summary>
    /// Include adaptive card?
    /// </summary>
    public bool UseAdaptiveCard { get; set; } = true;

    /// <summary>
    /// Enabled?
    /// </summary>
    public bool IsEnabled { get; set; } = true;
}

#endregion
