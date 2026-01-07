using GrcMvc.Services.Interfaces;

namespace GrcMvc.Services.Analytics
{
    /// <summary>
    /// ClickHouse OLAP query service interface
    /// Provides fast analytics queries from pre-aggregated data
    /// </summary>
    public interface IClickHouseService
    {
        // Dashboard snapshots
        Task<DashboardSnapshotDto?> GetLatestSnapshotAsync(Guid tenantId);
        Task<List<DashboardSnapshotDto>> GetSnapshotHistoryAsync(Guid tenantId, DateTime from, DateTime to);
        Task UpsertSnapshotAsync(DashboardSnapshotDto snapshot);

        // Compliance trends
        Task<List<ComplianceTrendDto>> GetComplianceTrendsAsync(Guid tenantId, int months = 12);
        Task<List<ComplianceTrendDto>> GetComplianceTrendsByFrameworkAsync(Guid tenantId, string frameworkCode, int months = 12);
        Task UpsertComplianceTrendAsync(ComplianceTrendDto trend);

        // Risk heatmap
        Task<List<RiskHeatmapCell>> GetRiskHeatmapAsync(Guid tenantId);
        Task UpsertRiskHeatmapAsync(Guid tenantId, List<RiskHeatmapCell> cells);

        // Framework comparison
        Task<List<FrameworkComparisonDto>> GetFrameworkComparisonAsync(Guid tenantId);
        Task UpsertFrameworkComparisonAsync(FrameworkComparisonDto comparison);

        // Task metrics by role
        Task<List<TaskMetricsByRoleDto>> GetTaskMetricsByRoleAsync(Guid tenantId);
        Task UpsertTaskMetricsByRoleAsync(TaskMetricsByRoleDto metrics);

        // Evidence metrics
        Task<List<EvidenceMetricsDto>> GetEvidenceMetricsAsync(Guid tenantId);
        Task UpsertEvidenceMetricsAsync(EvidenceMetricsDto metrics);

        // Top actions
        Task<List<TopActionDto>> GetTopActionsAsync(Guid tenantId, int limit = 10);
        Task UpsertTopActionsAsync(Guid tenantId, List<TopActionDto> actions);

        // User activity
        Task<List<UserActivityDto>> GetUserActivityAsync(Guid tenantId, DateTime from, DateTime to);
        Task UpsertUserActivityAsync(UserActivityDto activity);

        // Raw events
        Task InsertEventAsync(AnalyticsEventDto analyticsEvent);
        Task<List<AnalyticsEventDto>> GetRecentEventsAsync(Guid tenantId, int limit = 100);

        // Health check
        Task<bool> IsHealthyAsync();
    }

    #region DTOs

    public class DashboardSnapshotDto
    {
        public Guid TenantId { get; set; }
        public DateTime SnapshotDate { get; set; }
        public DateTime SnapshotHour { get; set; }

        // Compliance
        public int TotalControls { get; set; }
        public int CompliantControls { get; set; }
        public int PartialControls { get; set; }
        public int NonCompliantControls { get; set; }
        public int NotStartedControls { get; set; }
        public decimal ComplianceScore { get; set; }

        // Risks
        public int TotalRisks { get; set; }
        public int CriticalRisks { get; set; }
        public int HighRisks { get; set; }
        public int MediumRisks { get; set; }
        public int LowRisks { get; set; }
        public int OpenRisks { get; set; }
        public int MitigatedRisks { get; set; }
        public decimal RiskScoreAvg { get; set; }

        // Tasks
        public int TotalTasks { get; set; }
        public int PendingTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int OverdueTasks { get; set; }
        public int DueThisWeek { get; set; }

        // Evidence
        public int TotalEvidence { get; set; }
        public int EvidenceSubmitted { get; set; }
        public int EvidenceApproved { get; set; }
        public int EvidenceRejected { get; set; }
        public int EvidencePending { get; set; }

        // Assessments
        public int TotalAssessments { get; set; }
        public int ActiveAssessments { get; set; }
        public int CompletedAssessments { get; set; }

        // Plans
        public int TotalPlans { get; set; }
        public int ActivePlans { get; set; }
        public int CompletedPlans { get; set; }
        public decimal OverallPlanProgress { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ComplianceTrendDto
    {
        public Guid TenantId { get; set; }
        public string FrameworkCode { get; set; } = string.Empty;
        public string BaselineCode { get; set; } = string.Empty;
        public DateTime MeasureDate { get; set; }
        public DateTime MeasureHour { get; set; }
        public decimal ComplianceScore { get; set; }
        public int TotalControls { get; set; }
        public int CompliantControls { get; set; }
        public int PartialControls { get; set; }
        public int NonCompliantControls { get; set; }
        public decimal DeltaFromPrevious { get; set; }
    }

    public class RiskHeatmapCell
    {
        public int Likelihood { get; set; }
        public int Impact { get; set; }
        public int RiskCount { get; set; }
        public List<Guid> RiskIds { get; set; } = new();
    }

    public class FrameworkComparisonDto
    {
        public Guid TenantId { get; set; }
        public DateTime SnapshotDate { get; set; }
        public string FrameworkCode { get; set; } = string.Empty;
        public string FrameworkName { get; set; } = string.Empty;
        public int TotalRequirements { get; set; }
        public int CompliantCount { get; set; }
        public int PartialCount { get; set; }
        public int NonCompliantCount { get; set; }
        public int NotAssessedCount { get; set; }
        public decimal ComplianceScore { get; set; }
        public int MaturityLevel { get; set; }
        public decimal Trend7d { get; set; }
        public decimal Trend30d { get; set; }
    }

    public class TaskMetricsByRoleDto
    {
        public Guid TenantId { get; set; }
        public DateTime SnapshotDate { get; set; }
        public string RoleCode { get; set; } = string.Empty;
        public Guid TeamId { get; set; }
        public int TotalTasks { get; set; }
        public int PendingTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int OverdueTasks { get; set; }
        public decimal AvgCompletionDays { get; set; }
        public decimal SlaComplianceRate { get; set; }
    }

    public class EvidenceMetricsDto
    {
        public Guid TenantId { get; set; }
        public DateTime SnapshotDate { get; set; }
        public string EvidenceType { get; set; } = string.Empty;
        public string ControlDomain { get; set; } = string.Empty;
        public int TotalRequired { get; set; }
        public int TotalCollected { get; set; }
        public int TotalApproved { get; set; }
        public int TotalRejected { get; set; }
        public int TotalExpired { get; set; }
        public decimal CollectionRate { get; set; }
        public decimal ApprovalRate { get; set; }
        public decimal AvgReviewDays { get; set; }
    }

    public class TopActionDto
    {
        public Guid TenantId { get; set; }
        public DateTime SnapshotDate { get; set; }
        public int ActionRank { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public string ActionTitle { get; set; } = string.Empty;
        public string ActionDescription { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public string Urgency { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public string AssignedTo { get; set; } = string.Empty;
    }

    public class UserActivityDto
    {
        public Guid TenantId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime ActivityDate { get; set; }
        public int LoginCount { get; set; }
        public int TasksCompleted { get; set; }
        public int EvidenceSubmitted { get; set; }
        public int AssessmentsWorked { get; set; }
        public int ApprovalsGiven { get; set; }
        public int SessionMinutes { get; set; }
        public DateTime LastActivity { get; set; }
    }

    public class AnalyticsEventDto
    {
        public Guid EventId { get; set; }
        public Guid TenantId { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Actor { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public DateTime EventTimestamp { get; set; }
    }

    #endregion
}
