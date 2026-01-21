using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Advanced Monitoring Dashboard Service Interface
/// Provides Executive, Operations, Security, and Data Quality dashboards
/// for enterprise-grade platform monitoring
/// </summary>
public interface IAdvancedDashboardService
{
    #region Executive Dashboard (C-Level View)
    
    /// <summary>
    /// Get executive dashboard with KPIs, trends, and alerts
    /// "Are we compliant? Where are the risks? Are we audit-ready?"
    /// </summary>
    Task<AdvancedExecutiveDashboardDto> GetExecutiveDashboardAsync(Guid tenantId);
    
    /// <summary>
    /// Get audit readiness score with breakdown
    /// </summary>
    Task<AuditReadinessDto> GetAuditReadinessAsync(Guid tenantId);
    
    /// <summary>
    /// Get executive alerts (critical items requiring attention)
    /// </summary>
    Task<List<ExecutiveAlertDto>> GetExecutiveAlertsAsync(Guid tenantId, int limit = 10);
    
    #endregion

    #region Operations Dashboard (GRC Ops View)
    
    /// <summary>
    /// Get operations dashboard with workflow status, SLAs, and workload
    /// </summary>
    Task<OperationsDashboardDto> GetOperationsDashboardAsync(Guid tenantId);
    
    /// <summary>
    /// Get SLA breaches and at-risk items
    /// </summary>
    Task<List<SlaBreachDto>> GetSlaBreachesAsync(Guid tenantId, int limit = 20);
    
    /// <summary>
    /// Get owner workload distribution
    /// </summary>
    Task<List<OwnerWorkloadDto>> GetOwnerWorkloadAsync(Guid tenantId);
    
    /// <summary>
    /// Get evidence gaps (requirements missing evidence)
    /// </summary>
    Task<List<EvidenceGapDto>> GetEvidenceGapsAsync(Guid tenantId, int limit = 20);
    
    /// <summary>
    /// Get work queue by status
    /// </summary>
    Task<WorkQueueSummaryDto> GetWorkQueueAsync(Guid tenantId);
    
    #endregion

    #region Security Dashboard (SOC/NOC View)
    
    /// <summary>
    /// Get security dashboard with auth anomalies, integration health, and activity
    /// </summary>
    Task<SecurityDashboardDto> GetSecurityDashboardAsync(Guid tenantId);
    
    /// <summary>
    /// Get authentication anomaly KPIs
    /// </summary>
    Task<AuthAnomalyKpisDto> GetAuthAnomalyKpisAsync(Guid tenantId, int hours = 24);
    
    /// <summary>
    /// Get integration health status
    /// </summary>
    Task<List<IntegrationHealthDto>> GetIntegrationHealthAsync(Guid tenantId);
    
    /// <summary>
    /// Get API latency and error metrics
    /// </summary>
    Task<ApiMetricsDto> GetApiMetricsAsync(Guid tenantId, int hours = 24);
    
    /// <summary>
    /// Get tenant activity spikes
    /// </summary>
    Task<List<ActivitySpikeDto>> GetActivitySpikesAsync(Guid tenantId, int hours = 24);
    
    #endregion

    #region Data Quality Dashboard
    
    /// <summary>
    /// Get data quality dashboard with freshness, mappings, and orphans
    /// </summary>
    Task<DataQualityDashboardDto> GetDataQualityDashboardAsync(Guid tenantId);
    
    /// <summary>
    /// Get data freshness by entity type
    /// </summary>
    Task<List<DataFreshnessDto>> GetDataFreshnessAsync(Guid tenantId);
    
    /// <summary>
    /// Get missing mappings (risk-control-evidence links)
    /// </summary>
    Task<List<MissingMappingDto>> GetMissingMappingsAsync(Guid tenantId, int limit = 20);
    
    /// <summary>
    /// Get orphan records report
    /// </summary>
    Task<OrphanRecordsReportDto> GetOrphanRecordsAsync(Guid tenantId);
    
    #endregion

    #region Tenant Drilldown (Multi-tenant)
    
    /// <summary>
    /// Get tenant overview for platform admin
    /// </summary>
    Task<TenantOverviewDto> GetTenantOverviewAsync(Guid tenantId);
    
    /// <summary>
    /// Get all tenants summary (platform admin only)
    /// </summary>
    Task<List<TenantSummaryDto>> GetAllTenantsSummaryAsync();
    
    #endregion
}

#region Executive Dashboard DTOs

public record DashboardKpiDto(
    string Key,
    string Label,
    string LabelAr,
    decimal Value,
    string Unit,
    string Trend,        // Up, Down, Stable
    decimal TrendValue,  // Percentage change
    string Severity      // Critical, High, Medium, Low, Good
);

public class AdvancedExecutiveDashboardDto
{
    public Guid TenantId { get; set; }
    public List<DashboardKpiDto> Kpis { get; set; } = new();
    public List<TrendPointDto> ComplianceTrend { get; set; } = new();
    public List<GappedAreaDto> TopGaps { get; set; } = new();
    public RiskHeatmapDto RiskHeatmap { get; set; } = new();
    public AuditReadinessDto AuditReadiness { get; set; } = new();
    public List<ExecutiveAlertDto> Alerts { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DataAsOf { get; set; }
}

public record TrendPointDto(DateTime Date, decimal Value, string Label = "");

public record GappedAreaDto(
    string Domain,
    string DomainAr,
    int OpenGaps,
    int TotalControls,
    decimal GapPercentage,
    string Severity,
    string TopGapDescription
);

public class RiskHeatmapDto
{
    public List<HeatmapCellDto> Cells { get; set; } = new();
    public int TotalRisks { get; set; }
    public int CriticalCount { get; set; }
    public int HighCount { get; set; }
}

public record HeatmapCellDto(
    int Likelihood,
    int Impact,
    int Count,
    string RiskLevel,
    List<string> RiskNames
);

public class AuditReadinessDto
{
    public decimal OverallScore { get; set; }
    public string ReadinessLevel { get; set; } = "NotReady"; // Ready, AlmostReady, InProgress, NotReady
    public string ReadinessLevelAr { get; set; } = "غير جاهز";
    
    // Component scores
    public decimal EvidenceCompletenessScore { get; set; }
    public decimal ControlTestingScore { get; set; }
    public decimal OverdueRateScore { get; set; }
    public decimal ExceptionRateScore { get; set; }
    public decimal DocumentationScore { get; set; }
    
    // Details
    public int TotalRequirements { get; set; }
    public int RequirementsWithEvidence { get; set; }
    public int ControlsTested { get; set; }
    public int TotalControls { get; set; }
    public int OverdueItems { get; set; }
    public int OpenExceptions { get; set; }
    
    public List<string> ReadinessIssues { get; set; } = new();
    public DateTime? ProjectedReadyDate { get; set; }
}

public record ExecutiveAlertDto(
    Guid Id,
    string Title,
    string TitleAr,
    string Description,
    string Severity,        // Critical, High, Medium, Low
    string Category,        // Compliance, Risk, Evidence, SLA, Security
    DateTime CreatedAt,
    string? EntityType,
    Guid? EntityId,
    string? ActionUrl,
    bool IsAcknowledged
);

#endregion

#region Operations Dashboard DTOs

public class OperationsDashboardDto
{
    public Guid TenantId { get; set; }
    public WorkQueueSummaryDto WorkQueue { get; set; } = new();
    public List<SlaBreachDto> SlaBreaches { get; set; } = new();
    public List<OwnerWorkloadDto> OwnerWorkload { get; set; } = new();
    public List<EvidenceGapDto> EvidenceGaps { get; set; } = new();
    public List<RecentChangeDto> RecentChanges { get; set; } = new();
    public OperationsKpisDto Kpis { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class WorkQueueSummaryDto
{
    public int TotalItems { get; set; }
    public int PendingApproval { get; set; }
    public int InProgress { get; set; }
    public int Blocked { get; set; }
    public int DueToday { get; set; }
    public int DueThisWeek { get; set; }
    public int Overdue { get; set; }
    public List<WorkQueueByTypeDto> ByType { get; set; } = new();
}

public record WorkQueueByTypeDto(
    string Type,
    string TypeAr,
    int Count,
    int Overdue,
    int AvgAgeDays
);

public record SlaBreachDto(
    Guid ItemId,
    string ItemType,
    string Title,
    string Status,
    DateTime DueDate,
    int DaysOverdue,
    string Owner,
    string OwnerEmail,
    string Severity,
    string WorkflowName
);

public record OwnerWorkloadDto(
    string Owner,
    string OwnerEmail,
    string Department,
    int TotalItems,
    int OverdueItems,
    int DueThisWeek,
    int CompletedThisMonth,
    decimal AvgCompletionDays,
    string WorkloadLevel  // High, Medium, Low
);

public record EvidenceGapDto(
    Guid RequirementId,
    string ControlNumber,
    string ControlTitle,
    string FrameworkCode,
    string Domain,
    int EvidenceRequired,
    int EvidenceProvided,
    int GapCount,
    DateTime? DueDate,
    string Owner,
    string Severity
);

public record RecentChangeDto(
    Guid ChangeId,
    string EntityType,
    Guid EntityId,
    string EntityName,
    string ChangeType,   // Created, Updated, Deleted, StatusChanged
    string ChangedBy,
    DateTime ChangedAt,
    string Summary
);

public class OperationsKpisDto
{
    public decimal AvgCycleTime { get; set; }
    public decimal SlaComplianceRate { get; set; }
    public int ItemsProcessedToday { get; set; }
    public int ItemsProcessedThisWeek { get; set; }
    public decimal ThroughputTrend { get; set; }  // % change
}

#endregion

#region Security Dashboard DTOs

public class SecurityDashboardDto
{
    public Guid TenantId { get; set; }
    public AuthAnomalyKpisDto AuthAnomalies { get; set; } = new();
    public List<SuspiciousSessionDto> SuspiciousSessions { get; set; } = new();
    public List<IntegrationHealthDto> IntegrationHealth { get; set; } = new();
    public List<JobFailureDto> JobFailures { get; set; } = new();
    public ApiMetricsDto ApiMetrics { get; set; } = new();
    public List<ActivitySpikeDto> ActivitySpikes { get; set; } = new();
    public SecurityKpisDto Kpis { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class AuthAnomalyKpisDto
{
    public int FailedLoginsLast24h { get; set; }
    public int AccountLockouts { get; set; }
    public int UnusualGeoLogins { get; set; }
    public int PasswordResets { get; set; }
    public int MfaFailures { get; set; }
    public int SuspiciousIpAttempts { get; set; }
    public List<TrendPointDto> FailedLoginTrend { get; set; } = new();
    public string RiskLevel { get; set; } = "Low"; // Critical, High, Medium, Low
}

public record SuspiciousSessionDto(
    Guid SessionId,
    string UserId,
    string UserEmail,
    string IpAddress,
    string Location,
    string UserAgent,
    DateTime StartTime,
    string AnomalyType,   // UnusualLocation, UnusualTime, MultipleDevices, etc.
    string RiskLevel
);

public record IntegrationHealthDto(
    string IntegrationName,
    string IntegrationType,  // Email, ActiveDirectory, SIEM, ERP, Webhook
    string Status,           // Healthy, Degraded, Down, Unknown
    DateTime LastCheckTime,
    DateTime? LastSuccessTime,
    int FailedCallsLast24h,
    decimal AvgLatencyMs,
    string ErrorMessage
);

public record JobFailureDto(
    Guid JobId,
    string JobName,
    string JobType,
    DateTime FailedAt,
    string ErrorMessage,
    int RetryCount,
    string Status,
    string Severity
);

public class ApiMetricsDto
{
    public int TotalRequestsLast24h { get; set; }
    public decimal AvgLatencyMs { get; set; }
    public decimal P95LatencyMs { get; set; }
    public decimal ErrorRate { get; set; }
    public int Http4xxErrors { get; set; }
    public int Http5xxErrors { get; set; }
    public List<TrendPointDto> LatencyTrend { get; set; } = new();
    public List<EndpointMetricsDto> TopSlowEndpoints { get; set; } = new();
}

public record EndpointMetricsDto(
    string Endpoint,
    int RequestCount,
    decimal AvgLatencyMs,
    decimal ErrorRate
);

public record ActivitySpikeDto(
    DateTime Timestamp,
    string ActivityType,
    int Count,
    int BaselineCount,
    decimal DeviationPercent,
    string Severity
);

public class SecurityKpisDto
{
    public decimal OverallSecurityScore { get; set; }
    public int ActiveSessions { get; set; }
    public int TotalUsersActive24h { get; set; }
    public decimal AuthSuccessRate { get; set; }
    public int PendingSecurityAlerts { get; set; }
}

#endregion

#region Data Quality Dashboard DTOs

public class DataQualityDashboardDto
{
    public Guid TenantId { get; set; }
    public DataQualityScoreDto OverallScore { get; set; } = new();
    public List<DataFreshnessDto> Freshness { get; set; } = new();
    public List<MissingMappingDto> MissingMappings { get; set; } = new();
    public OrphanRecordsReportDto OrphanRecords { get; set; } = new();
    public List<DataConsistencyIssueDto> ConsistencyIssues { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class DataQualityScoreDto
{
    public decimal OverallScore { get; set; }
    public decimal CompletenessScore { get; set; }
    public decimal ConsistencyScore { get; set; }
    public decimal FreshnessScore { get; set; }
    public decimal AccuracyScore { get; set; }
    public string QualityLevel { get; set; } = "Unknown"; // Excellent, Good, Fair, Poor
}

public record DataFreshnessDto(
    string EntityType,
    string EntityTypeAr,
    int TotalRecords,
    int StaleRecords,
    DateTime? LastUpdated,
    int AvgAgeDays,
    string FreshnessLevel,  // Fresh, Stale, Critical
    int StalenessThresholdDays
);

public record MissingMappingDto(
    string MappingType,     // RiskControl, ControlEvidence, RequirementEvidence
    Guid SourceEntityId,
    string SourceEntityType,
    string SourceEntityName,
    string ExpectedTarget,
    string Impact,          // High, Medium, Low
    string Recommendation
);

public class OrphanRecordsReportDto
{
    public int TotalOrphans { get; set; }
    public List<OrphanByTypeDto> ByType { get; set; } = new();
    public List<OrphanRecordDto> TopOrphans { get; set; } = new();
}

public record OrphanByTypeDto(
    string EntityType,
    int Count,
    string ImpactLevel
);

public record OrphanRecordDto(
    Guid RecordId,
    string EntityType,
    string Name,
    DateTime CreatedAt,
    string MissingRelation,
    string SuggestedAction
);

public record DataConsistencyIssueDto(
    string IssueType,
    string Description,
    int AffectedRecords,
    string Severity,
    string Resolution
);

#endregion

#region Tenant Overview DTOs

public class TenantOverviewDto
{
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string OrganizationName { get; set; } = string.Empty;
    public string SubscriptionTier { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastActivityAt { get; set; }
    
    // Usage metrics
    public int TotalUsers { get; set; }
    public int ActiveUsersLast30Days { get; set; }
    public int TotalAssessments { get; set; }
    public int TotalRisks { get; set; }
    public int TotalEvidences { get; set; }
    
    // Health metrics
    public decimal ComplianceScore { get; set; }
    public decimal DataQualityScore { get; set; }
    public decimal ActivityScore { get; set; }
    public string HealthStatus { get; set; } = "Unknown"; // Healthy, Warning, Critical
    
    // Alerts
    public int OpenAlerts { get; set; }
    public int CriticalAlerts { get; set; }
}

public record TenantSummaryDto(
    Guid TenantId,
    string TenantName,
    string OrganizationName,
    string SubscriptionTier,
    int UserCount,
    decimal ComplianceScore,
    string HealthStatus,
    DateTime? LastActivityAt,
    int OpenAlerts
);

#endregion
