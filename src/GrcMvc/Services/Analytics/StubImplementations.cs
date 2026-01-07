using GrcMvc.Messaging.Messages;

namespace GrcMvc.Services.Analytics
{
    /// <summary>
    /// Stub implementation of ClickHouse service when ClickHouse is disabled
    /// Falls back to returning empty data - dashboards will use PostgreSQL directly
    /// </summary>
    public class StubClickHouseService : IClickHouseService
    {
        private readonly ILogger<StubClickHouseService> _logger;

        public StubClickHouseService(ILogger<StubClickHouseService> logger)
        {
            _logger = logger;
            _logger.LogInformation("Using stub ClickHouse service - ClickHouse is disabled");
        }

        public Task<DashboardSnapshotDto?> GetLatestSnapshotAsync(Guid tenantId) => Task.FromResult<DashboardSnapshotDto?>(null);
        public Task<List<DashboardSnapshotDto>> GetSnapshotHistoryAsync(Guid tenantId, DateTime from, DateTime to) => Task.FromResult(new List<DashboardSnapshotDto>());
        public Task UpsertSnapshotAsync(DashboardSnapshotDto snapshot) => Task.CompletedTask;
        public Task<List<ComplianceTrendDto>> GetComplianceTrendsAsync(Guid tenantId, int months = 12) => Task.FromResult(new List<ComplianceTrendDto>());
        public Task<List<ComplianceTrendDto>> GetComplianceTrendsByFrameworkAsync(Guid tenantId, string frameworkCode, int months = 12) => Task.FromResult(new List<ComplianceTrendDto>());
        public Task UpsertComplianceTrendAsync(ComplianceTrendDto trend) => Task.CompletedTask;
        public Task<List<RiskHeatmapCell>> GetRiskHeatmapAsync(Guid tenantId) => Task.FromResult(new List<RiskHeatmapCell>());
        public Task UpsertRiskHeatmapAsync(Guid tenantId, List<RiskHeatmapCell> cells) => Task.CompletedTask;
        public Task<List<FrameworkComparisonDto>> GetFrameworkComparisonAsync(Guid tenantId) => Task.FromResult(new List<FrameworkComparisonDto>());
        public Task UpsertFrameworkComparisonAsync(FrameworkComparisonDto comparison) => Task.CompletedTask;
        public Task<List<TaskMetricsByRoleDto>> GetTaskMetricsByRoleAsync(Guid tenantId) => Task.FromResult(new List<TaskMetricsByRoleDto>());
        public Task UpsertTaskMetricsByRoleAsync(TaskMetricsByRoleDto metrics) => Task.CompletedTask;
        public Task<List<EvidenceMetricsDto>> GetEvidenceMetricsAsync(Guid tenantId) => Task.FromResult(new List<EvidenceMetricsDto>());
        public Task UpsertEvidenceMetricsAsync(EvidenceMetricsDto metrics) => Task.CompletedTask;
        public Task<List<TopActionDto>> GetTopActionsAsync(Guid tenantId, int limit = 10) => Task.FromResult(new List<TopActionDto>());
        public Task UpsertTopActionsAsync(Guid tenantId, List<TopActionDto> actions) => Task.CompletedTask;
        public Task<List<UserActivityDto>> GetUserActivityAsync(Guid tenantId, DateTime from, DateTime to) => Task.FromResult(new List<UserActivityDto>());
        public Task UpsertUserActivityAsync(UserActivityDto activity) => Task.CompletedTask;
        public Task InsertEventAsync(AnalyticsEventDto analyticsEvent) => Task.CompletedTask;
        public Task<List<AnalyticsEventDto>> GetRecentEventsAsync(Guid tenantId, int limit = 100) => Task.FromResult(new List<AnalyticsEventDto>());
        public Task<bool> IsHealthyAsync() => Task.FromResult(false);
    }

    /// <summary>
    /// Stub implementation of Dashboard Projector when ClickHouse is disabled
    /// Does nothing - dashboards will query PostgreSQL directly
    /// </summary>
    public class StubDashboardProjector : IDashboardProjector
    {
        private readonly ILogger<StubDashboardProjector> _logger;

        public StubDashboardProjector(ILogger<StubDashboardProjector> logger)
        {
            _logger = logger;
            _logger.LogInformation("Using stub dashboard projector - ClickHouse is disabled");
        }

        public Task ProjectSnapshotAsync(Guid tenantId) => Task.CompletedTask;
        public Task ProjectComplianceTrendsAsync(Guid tenantId) => Task.CompletedTask;
        public Task ProjectRiskHeatmapAsync(Guid tenantId) => Task.CompletedTask;
        public Task ProjectFrameworkComparisonAsync(Guid tenantId) => Task.CompletedTask;
        public Task ProjectTaskMetricsAsync(Guid tenantId) => Task.CompletedTask;
        public Task ProjectEvidenceMetricsAsync(Guid tenantId) => Task.CompletedTask;
        public Task ProjectTopActionsAsync(Guid tenantId) => Task.CompletedTask;
        public Task ProjectAllAsync(Guid tenantId) => Task.CompletedTask;
        public Task HandleEventAsync(IGrcEvent domainEvent) => Task.CompletedTask;
    }
}
