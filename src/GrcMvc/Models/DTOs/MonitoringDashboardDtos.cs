using System;
using System.Collections.Generic;

namespace GrcMvc.Models.DTOs;

#region Common Dashboard DTOs

public record DashboardKpiDto(
    string Key,
    decimal Value,
    string Unit,
    string Trend,        // "up", "down", "stable"
    string Severity,     // "success", "warning", "danger", "info"
    string? DrillDownUrl = null
);

public record TrendPointDto(DateTime Date, decimal Value);

public record GaugeDto(decimal Value, decimal Max, string Label, string Severity);

public record AlertDto(
    string Id,
    string Title,
    string Description,
    string Severity,      // "critical", "high", "medium", "low"
    DateTimeOffset At,
    string? Link,
    string Category       // "Compliance", "Risk", "Audit", "Security", "System"
);

public record HeatmapDto(IReadOnlyList<HeatCellDto> Cells);

public record HeatCellDto(int Likelihood, int Impact, int Count, string Severity);

public record GappedAreaDto(string Domain, int OpenGaps, string Severity, string? DrillDownUrl);

#endregion

#region Executive Dashboard

public record ExecutiveDashboardDto(
    IReadOnlyList<DashboardKpiDto> Kpis,
    IReadOnlyList<TrendPointDto> ComplianceTrend,
    IReadOnlyList<GappedAreaDto> TopGaps,
    HeatmapDto RiskHeatmap,
    GaugeDto AuditReadiness,
    IReadOnlyList<AlertDto> Alerts,
    DateTime LastRefreshed,
    string DataFreshness   // "Real-time", "5 min ago", "Stale"
);

#endregion

#region Operations Dashboard

public record OperationsDashboardDto(
    IReadOnlyList<DashboardKpiDto> Kpis,
    IReadOnlyList<WorkQueueItemDto> WorkQueue,
    IReadOnlyList<SlaBreachDto> SlaBreaches,
    IReadOnlyList<OwnerWorkloadDto> OwnerWorkloads,
    IReadOnlyList<EvidenceGapDto> EvidenceGaps,
    IReadOnlyList<CalendarEventDto> UpcomingTests,
    IReadOnlyList<ChangeLogDto> RecentChanges,
    DateTime LastRefreshed
);

public record WorkQueueItemDto(
    Guid Id,
    string Title,
    string Status,         // "Pending", "InProgress", "Blocked", "Overdue"
    string AssignedTo,
    string WorkflowType,
    DateTime? DueDate,
    int? DaysInQueue,
    string Priority
);

public record SlaBreachDto(
    Guid Id,
    string Title,
    string BreachType,     // "Response", "Resolution", "Evidence"
    DateTime BreachDate,
    int HoursOverdue,
    string Owner,
    string Severity
);

public record OwnerWorkloadDto(
    string OwnerId,
    string OwnerName,
    string Role,
    int TotalTasks,
    int OpenTasks,
    int OverdueTasks,
    decimal AvgCompletionDays,
    string WorkloadLevel   // "Light", "Normal", "Heavy", "Critical"
);

public record EvidenceGapDto(
    Guid ControlId,
    string ControlNumber,
    string ControlTitle,
    string Domain,
    int RequiredEvidence,
    int CurrentEvidence,
    int GapCount,
    string Owner,
    DateTime? LastUpdated
);

public record CalendarEventDto(
    Guid Id,
    string Title,
    string EventType,      // "ControlTest", "AuditStart", "ReviewDue", "CertificationExpiry"
    DateTime ScheduledDate,
    string Status,
    string Owner
);

public record ChangeLogDto(
    Guid Id,
    string EntityType,
    string EntityName,
    string Action,         // "Created", "Updated", "Deleted", "StatusChanged"
    string ChangedBy,
    DateTime ChangedAt,
    string? Details
);

#endregion

#region Security Dashboard (SOC/NOC Style)

public record SecurityDashboardDto(
    IReadOnlyList<DashboardKpiDto> Kpis,
    IReadOnlyList<AuthAnomalyDto> AuthAnomalies,
    IReadOnlyList<SuspiciousSessionDto> SuspiciousSessions,
    IReadOnlyList<IntegrationHealthDto> IntegrationHealth,
    IReadOnlyList<JobFailureDto> JobFailures,
    ApiHealthDto ApiHealth,
    IReadOnlyList<TenantActivityDto> TenantActivitySpikes,
    DateTime LastRefreshed
);

public record AuthAnomalyDto(
    Guid Id,
    string UserId,
    string UserEmail,
    string AnomalyType,    // "FailedLogins", "Lockout", "UnusualGeo", "UnusualTime", "BruteForce"
    int Count,
    DateTime DetectedAt,
    string? IpAddress,
    string? GeoLocation,
    string Severity
);

public record SuspiciousSessionDto(
    string SessionId,
    string UserId,
    string UserEmail,
    string IpAddress,
    string? GeoLocation,
    DateTime StartedAt,
    int ActionCount,
    string? SuspiciousReason
);

public record IntegrationHealthDto(
    string IntegrationName,    // "MicrosoftGraph", "Email", "ERP", "Webhook"
    string Status,             // "Healthy", "Degraded", "Down"
    DateTime LastCheck,
    DateTime? LastSuccess,
    int FailureCount24h,
    string? LastError
);

public record JobFailureDto(
    string JobId,
    string JobName,
    string JobType,            // "Hangfire", "Background", "Scheduled"
    DateTime FailedAt,
    string ErrorMessage,
    int RetryCount,
    string Status              // "Failed", "RetryPending", "Abandoned"
);

public record ApiHealthDto(
    decimal AvgLatencyMs,
    int TotalRequests24h,
    int ErrorCount24h,
    decimal ErrorRate,
    IReadOnlyList<EndpointHealthDto> TopSlowEndpoints,
    IReadOnlyList<EndpointHealthDto> TopErrorEndpoints
);

public record EndpointHealthDto(
    string Endpoint,
    int RequestCount,
    decimal AvgLatencyMs,
    int ErrorCount,
    decimal ErrorRate
);

public record TenantActivityDto(
    Guid TenantId,
    string TenantName,
    int ActivityCount24h,
    int ActivityCountPrev24h,
    decimal SpikePercentage,
    string SpikeLevel          // "Normal", "Elevated", "High", "Suspicious"
);

#endregion

#region Data Quality Dashboard

public record DataQualityDashboardDto(
    IReadOnlyList<DashboardKpiDto> Kpis,
    IReadOnlyList<DataFreshnessDto> DataFreshness,
    IReadOnlyList<MissingMappingDto> MissingMappings,
    IReadOnlyList<OrphanRecordDto> OrphanRecords,
    IReadOnlyList<SyncLagDto> SyncLags,
    IReadOnlyList<DataValidationIssueDto> ValidationIssues,
    DateTime LastRefreshed
);

public record DataFreshnessDto(
    string EntityType,
    DateTime? LastUpdated,
    int TotalRecords,
    int StaleRecords,          // Not updated in expected period
    string FreshnessStatus,    // "Fresh", "Aging", "Stale", "Critical"
    TimeSpan ExpectedRefreshInterval
);

public record MissingMappingDto(
    string MappingType,        // "Risk-Control", "Control-Evidence", "Assessment-Control"
    Guid SourceId,
    string SourceName,
    string ExpectedTarget,
    string Owner,
    string Severity
);

public record OrphanRecordDto(
    string EntityType,
    Guid EntityId,
    string EntityName,
    string OrphanReason,       // "NoParent", "DeletedParent", "BrokenReference"
    DateTime CreatedAt
);

public record SyncLagDto(
    string ConnectorName,      // "EmailSync", "ERPSync", "ADSync"
    DateTime LastSyncStart,
    DateTime? LastSyncComplete,
    TimeSpan? CurrentLag,
    string Status,             // "Syncing", "Idle", "Failed", "Stalled"
    int RecordsPending
);

public record DataValidationIssueDto(
    string EntityType,
    Guid EntityId,
    string EntityName,
    string FieldName,
    string IssueType,          // "Missing", "Invalid", "OutOfRange", "Duplicate"
    string CurrentValue,
    string? ExpectedFormat,
    string Severity
);

#endregion

#region Tenant Drilldown Dashboard

public record TenantDashboardDto(
    Guid TenantId,
    string TenantName,
    string SubscriptionTier,
    IReadOnlyList<DashboardKpiDto> Kpis,
    ExecutiveDashboardDto ExecutiveSummary,
    IReadOnlyList<UserActivityDto> TopUsers,
    IReadOnlyList<ModuleUsageDto> ModuleUsage,
    ResourceUsageDto ResourceUsage,
    DateTime LastRefreshed
);

public record UserActivityDto(
    string UserId,
    string UserName,
    string Email,
    int LoginCount30d,
    int ActionCount30d,
    DateTime LastActive
);

public record ModuleUsageDto(
    string ModuleName,
    int AccessCount30d,
    int UniqueUsers,
    decimal AvgSessionMinutes
);

public record ResourceUsageDto(
    long StorageUsedBytes,
    long StorageQuotaBytes,
    int UsersActive,
    int UsersQuota,
    int ApiCallsToday,
    int ApiCallsQuota
);

#endregion
