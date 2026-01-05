using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities;

#region System of Record Definitions

/// <summary>
/// System of Record Definition - Authoritative source for each object type
/// Rule: Other tools may reference, but do not "own" the record
/// </summary>
public class SystemOfRecordDefinition : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string ObjectType { get; set; } = string.Empty; // Control, Task, Evidence, Incident, Asset, User

    [Required]
    [StringLength(50)]
    public string SystemCode { get; set; } = string.Empty; // GRC, ITSM, SharePoint, SIEM, CMDB, IAM

    [Required]
    [StringLength(255)]
    public string SystemName { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Is this the authoritative source?
    /// </summary>
    public bool IsAuthoritative { get; set; } = true;

    /// <summary>
    /// Can other systems create this object type?
    /// </summary>
    public bool AllowExternalCreate { get; set; } = false;

    /// <summary>
    /// Can other systems update this object type?
    /// </summary>
    public bool AllowExternalUpdate { get; set; } = false;

    /// <summary>
    /// API endpoint for this system
    /// </summary>
    [StringLength(500)]
    public string? ApiEndpoint { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Cross-Reference Mapping - Links objects across systems
/// Every object must have a unique ID that is shared across systems
/// </summary>
public class CrossReferenceMapping : BaseEntity
{
    public Guid TenantId { get; set; }

    /// <summary>
    /// Object type: Control, Task, Evidence, Incident, Asset
    /// </summary>
    [Required]
    [StringLength(50)]
    public string ObjectType { get; set; } = string.Empty;

    /// <summary>
    /// Internal GRC system ID
    /// </summary>
    public Guid InternalId { get; set; }

    /// <summary>
    /// Internal object code (human-readable)
    /// </summary>
    [Required]
    [StringLength(100)]
    public string InternalCode { get; set; } = string.Empty;

    /// <summary>
    /// External system code
    /// </summary>
    [Required]
    [StringLength(50)]
    public string ExternalSystemCode { get; set; } = string.Empty;

    /// <summary>
    /// External system ID
    /// </summary>
    [Required]
    [StringLength(255)]
    public string ExternalId { get; set; } = string.Empty;

    /// <summary>
    /// External system URL/link
    /// </summary>
    [StringLength(500)]
    public string? ExternalUrl { get; set; }

    /// <summary>
    /// Last sync timestamp
    /// </summary>
    public DateTime? LastSyncAt { get; set; }

    /// <summary>
    /// Sync status: InSync, Pending, Error
    /// </summary>
    [StringLength(20)]
    public string SyncStatus { get; set; } = "InSync";

    /// <summary>
    /// Last sync error (if any)
    /// </summary>
    [StringLength(1000)]
    public string? LastSyncError { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

#endregion

#region Domain Events

/// <summary>
/// Domain Event - Published when something happens in the system
/// Uses event-driven integration model
/// </summary>
public class DomainEvent : BaseEntity
{
    public Guid TenantId { get; set; }

    /// <summary>
    /// Unique correlation ID for idempotency
    /// </summary>
    [Required]
    [StringLength(50)]
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Event type: ControlCreated, ControlUpdated, ControlTestDue, EvidenceUploaded,
    /// KRIBreached, IncidentOpened, ExceptionApproved, etc.
    /// </summary>
    [Required]
    [StringLength(100)]
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Schema version for backward compatibility
    /// </summary>
    [Required]
    [StringLength(10)]
    public string SchemaVersion { get; set; } = "1.0";

    /// <summary>
    /// Source system that emitted this event
    /// </summary>
    [Required]
    [StringLength(50)]
    public string SourceSystem { get; set; } = "GRC";

    /// <summary>
    /// Object type affected
    /// </summary>
    [Required]
    [StringLength(50)]
    public string ObjectType { get; set; } = string.Empty;

    /// <summary>
    /// Object ID affected
    /// </summary>
    public Guid ObjectId { get; set; }

    /// <summary>
    /// Object code (human-readable)
    /// </summary>
    [StringLength(100)]
    public string? ObjectCode { get; set; }

    /// <summary>
    /// Event payload (JSON)
    /// </summary>
    [Required]
    public string PayloadJson { get; set; } = "{}";

    /// <summary>
    /// Event status: Pending, Published, Processed, Failed
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// Number of processing attempts
    /// </summary>
    public int ProcessingAttempts { get; set; } = 0;

    /// <summary>
    /// Last error message
    /// </summary>
    [StringLength(2000)]
    public string? LastError { get; set; }

    /// <summary>
    /// Timestamp when event occurred
    /// </summary>
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when event was published
    /// </summary>
    public DateTime? PublishedAt { get; set; }

    /// <summary>
    /// Timestamp when event was processed
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// User/system that triggered the event
    /// </summary>
    [StringLength(100)]
    public string? TriggeredBy { get; set; }
}

/// <summary>
/// Event Subscription - Who wants to receive which events
/// </summary>
public class EventSubscription : BaseEntity
{
    public Guid? TenantId { get; set; } // Null = global subscription

    [Required]
    [StringLength(50)]
    public string SubscriptionCode { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Event type pattern (supports wildcards): Control*, Evidence*, *
    /// </summary>
    [Required]
    [StringLength(100)]
    public string EventTypePattern { get; set; } = "*";

    /// <summary>
    /// Subscriber system code
    /// </summary>
    [Required]
    [StringLength(50)]
    public string SubscriberSystem { get; set; } = string.Empty;

    /// <summary>
    /// Delivery method: Webhook, Queue, DirectCall
    /// </summary>
    [Required]
    [StringLength(20)]
    public string DeliveryMethod { get; set; } = "Webhook";

    /// <summary>
    /// Delivery endpoint (URL, queue name, etc.)
    /// </summary>
    [StringLength(500)]
    public string? DeliveryEndpoint { get; set; }

    /// <summary>
    /// Retry policy: None, Linear, Exponential
    /// </summary>
    [StringLength(20)]
    public string RetryPolicy { get; set; } = "Exponential";

    /// <summary>
    /// Max retry attempts
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Filter expression (JSON) for additional filtering
    /// </summary>
    public string? FilterExpression { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Event Delivery Log - Tracks delivery of events to subscribers
/// </summary>
public class EventDeliveryLog : BaseEntity
{
    public Guid EventId { get; set; }

    [ForeignKey("EventId")]
    public virtual DomainEvent Event { get; set; } = null!;

    public Guid SubscriptionId { get; set; }

    [ForeignKey("SubscriptionId")]
    public virtual EventSubscription Subscription { get; set; } = null!;

    /// <summary>
    /// Delivery status: Pending, Delivered, Failed, Skipped
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// Attempt number
    /// </summary>
    public int AttemptNumber { get; set; } = 1;

    /// <summary>
    /// HTTP status code (for webhooks)
    /// </summary>
    public int? HttpStatusCode { get; set; }

    /// <summary>
    /// Response body (truncated)
    /// </summary>
    [StringLength(2000)]
    public string? ResponseBody { get; set; }

    /// <summary>
    /// Error message (if failed)
    /// </summary>
    [StringLength(2000)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Delivery latency in ms
    /// </summary>
    public int? LatencyMs { get; set; }

    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;

    public DateTime? NextRetryAt { get; set; }
}

#endregion

#region Integration Connectors

/// <summary>
/// Integration Connector - Connection to external system
/// </summary>
public class IntegrationConnector : BaseEntity
{
    public Guid? TenantId { get; set; } // Null = global connector

    [Required]
    [StringLength(50)]
    public string ConnectorCode { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Connector type: ITSM, SIEM, IAM, Evidence, Teams, PowerAutomate, Webhook
    /// </summary>
    [Required]
    [StringLength(30)]
    public string ConnectorType { get; set; } = string.Empty;

    /// <summary>
    /// Target system: ServiceNow, Jira, Splunk, AzureAD, SharePoint, Teams
    /// </summary>
    [Required]
    [StringLength(50)]
    public string TargetSystem { get; set; } = string.Empty;

    /// <summary>
    /// Connection configuration (encrypted JSON)
    /// </summary>
    public string? ConnectionConfigJson { get; set; }

    /// <summary>
    /// Authentication type: OAuth, APIKey, Basic, Certificate
    /// </summary>
    [StringLength(20)]
    public string AuthType { get; set; } = "OAuth";

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
    /// Last successful sync
    /// </summary>
    public DateTime? LastSuccessfulSync { get; set; }

    /// <summary>
    /// Error count (resets on successful sync)
    /// </summary>
    public int ErrorCount { get; set; } = 0;

    /// <summary>
    /// Supported operations (JSON array)
    /// </summary>
    public string? SupportedOperationsJson { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Sync Job - Scheduled synchronization between systems
/// </summary>
public class SyncJob : BaseEntity
{
    public Guid? TenantId { get; set; }

    public Guid ConnectorId { get; set; }

    [ForeignKey("ConnectorId")]
    public virtual IntegrationConnector Connector { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string JobCode { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Sync direction: Inbound, Outbound, Bidirectional
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Direction { get; set; } = "Bidirectional";

    /// <summary>
    /// Object type to sync
    /// </summary>
    [Required]
    [StringLength(50)]
    public string ObjectType { get; set; } = string.Empty;

    /// <summary>
    /// Sync frequency: RealTime, Hourly, Daily, Weekly
    /// </summary>
    [StringLength(20)]
    public string Frequency { get; set; } = "Daily";

    /// <summary>
    /// Cron expression (for scheduled jobs)
    /// </summary>
    [StringLength(50)]
    public string? CronExpression { get; set; }

    /// <summary>
    /// Field mapping configuration (JSON)
    /// </summary>
    public string? FieldMappingJson { get; set; }

    /// <summary>
    /// Filter expression (JSON) - what to sync
    /// </summary>
    public string? FilterExpression { get; set; }

    /// <summary>
    /// Use upsert logic (update if exists, else create)
    /// </summary>
    public bool UseUpsert { get; set; } = true;

    /// <summary>
    /// Last run timestamp
    /// </summary>
    public DateTime? LastRunAt { get; set; }

    /// <summary>
    /// Last run status: Success, Partial, Failed
    /// </summary>
    [StringLength(20)]
    public string? LastRunStatus { get; set; }

    /// <summary>
    /// Records synced in last run
    /// </summary>
    public int? LastRunRecordCount { get; set; }

    /// <summary>
    /// Next scheduled run
    /// </summary>
    public DateTime? NextRunAt { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Sync Execution Log - Record of each sync job run
/// </summary>
public class SyncExecutionLog : BaseEntity
{
    public Guid SyncJobId { get; set; }

    [ForeignKey("SyncJobId")]
    public virtual SyncJob SyncJob { get; set; } = null!;

    /// <summary>
    /// Execution status: Running, Completed, Failed, Cancelled
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Running";

    /// <summary>
    /// Records processed
    /// </summary>
    public int RecordsProcessed { get; set; } = 0;

    /// <summary>
    /// Records created
    /// </summary>
    public int RecordsCreated { get; set; } = 0;

    /// <summary>
    /// Records updated
    /// </summary>
    public int RecordsUpdated { get; set; } = 0;

    /// <summary>
    /// Records failed
    /// </summary>
    public int RecordsFailed { get; set; } = 0;

    /// <summary>
    /// Records skipped (duplicates, filtered)
    /// </summary>
    public int RecordsSkipped { get; set; } = 0;

    /// <summary>
    /// Error details (JSON array)
    /// </summary>
    public string? ErrorsJson { get; set; }

    /// <summary>
    /// Duration in seconds
    /// </summary>
    public int? DurationSeconds { get; set; }

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }
}

#endregion

#region Integration Monitoring

/// <summary>
/// Integration Health Metric - For monitoring dashboard
/// </summary>
public class IntegrationHealthMetric : BaseEntity
{
    public Guid ConnectorId { get; set; }

    [ForeignKey("ConnectorId")]
    public virtual IntegrationConnector Connector { get; set; } = null!;

    /// <summary>
    /// Metric type: Availability, Latency, ErrorRate, ThroughPut
    /// </summary>
    [Required]
    [StringLength(30)]
    public string MetricType { get; set; } = string.Empty;

    /// <summary>
    /// Measurement period start
    /// </summary>
    public DateTime PeriodStart { get; set; }

    /// <summary>
    /// Measurement period end
    /// </summary>
    public DateTime PeriodEnd { get; set; }

    /// <summary>
    /// Metric value
    /// </summary>
    public decimal Value { get; set; }

    /// <summary>
    /// Unit: Percentage, Milliseconds, Count, PerSecond
    /// </summary>
    [StringLength(20)]
    public string Unit { get; set; } = string.Empty;

    /// <summary>
    /// Threshold for alerting
    /// </summary>
    public decimal? AlertThreshold { get; set; }

    /// <summary>
    /// Is breaching threshold?
    /// </summary>
    public bool IsBreaching { get; set; } = false;

    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Dead Letter Queue - Failed events that need manual intervention
/// </summary>
public class DeadLetterEntry : BaseEntity
{
    public Guid? EventId { get; set; }

    [ForeignKey("EventId")]
    public virtual DomainEvent? Event { get; set; }

    public Guid? SyncJobId { get; set; }

    [ForeignKey("SyncJobId")]
    public virtual SyncJob? SyncJob { get; set; }

    /// <summary>
    /// Entry type: Event, Sync, Webhook
    /// </summary>
    [Required]
    [StringLength(20)]
    public string EntryType { get; set; } = string.Empty;

    /// <summary>
    /// Original payload (JSON)
    /// </summary>
    [Required]
    public string OriginalPayloadJson { get; set; } = "{}";

    /// <summary>
    /// Error message
    /// </summary>
    [Required]
    [StringLength(2000)]
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Stack trace (if available)
    /// </summary>
    public string? StackTrace { get; set; }

    /// <summary>
    /// Failure count
    /// </summary>
    public int FailureCount { get; set; } = 1;

    /// <summary>
    /// Status: Pending, Retrying, Resolved, Abandoned
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Pending";

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

    public DateTime FailedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastRetryAt { get; set; }
}

#endregion

#region Standard Event Contracts

/// <summary>
/// Event Schema Registry - Defines standard event contracts
/// </summary>
public class EventSchemaRegistry : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string EventType { get; set; } = string.Empty;

    [Required]
    [StringLength(10)]
    public string SchemaVersion { get; set; } = "1.0";

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// JSON Schema definition
    /// </summary>
    [Required]
    public string JsonSchema { get; set; } = "{}";

    /// <summary>
    /// Example payload (JSON)
    /// </summary>
    public string? ExamplePayloadJson { get; set; }

    /// <summary>
    /// Required fields (comma-separated)
    /// </summary>
    [StringLength(500)]
    public string? RequiredFields { get; set; }

    /// <summary>
    /// Is this the current version?
    /// </summary>
    public bool IsCurrent { get; set; } = true;

    public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;

    public DateTime? DeprecatedAt { get; set; }
}

#endregion
