using System.Data;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using GrcMvc.Configuration;
using Microsoft.Extensions.Options;

namespace GrcMvc.Services.Analytics
{
    /// <summary>
    /// ClickHouse OLAP query service implementation
    /// Uses HTTP interface for queries
    /// </summary>
    public class ClickHouseService : IClickHouseService
    {
        private readonly HttpClient _httpClient;
        private readonly ClickHouseSettings _settings;
        private readonly ILogger<ClickHouseService> _logger;
        private readonly string _baseUrl;

        public ClickHouseService(
            HttpClient httpClient,
            IOptions<ClickHouseSettings> settings,
            ILogger<ClickHouseService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
            _baseUrl = $"http://{_settings.Host}:{_settings.HttpPort}";

            // Set auth header
            var authBytes = Encoding.UTF8.GetBytes($"{_settings.Username}:{_settings.Password}");
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));
        }

        #region Dashboard Snapshots

        public async Task<DashboardSnapshotDto?> GetLatestSnapshotAsync(Guid tenantId)
        {
            var query = $@"
                SELECT *
                FROM {_settings.Database}.dashboard_snapshots
                WHERE tenant_id = '{tenantId}'
                ORDER BY snapshot_hour DESC
                LIMIT 1
                FORMAT JSONEachRow";

            var results = await ExecuteQueryAsync<DashboardSnapshotDto>(query);
            return results.FirstOrDefault();
        }

        public async Task<List<DashboardSnapshotDto>> GetSnapshotHistoryAsync(Guid tenantId, DateTime from, DateTime to)
        {
            var query = $@"
                SELECT *
                FROM {_settings.Database}.dashboard_snapshots
                WHERE tenant_id = '{tenantId}'
                  AND snapshot_date >= '{from:yyyy-MM-dd}'
                  AND snapshot_date <= '{to:yyyy-MM-dd}'
                ORDER BY snapshot_hour ASC
                FORMAT JSONEachRow";

            return await ExecuteQueryAsync<DashboardSnapshotDto>(query);
        }

        public async Task UpsertSnapshotAsync(DashboardSnapshotDto snapshot)
        {
            var query = $@"
                INSERT INTO {_settings.Database}.dashboard_snapshots
                (tenant_id, snapshot_date, snapshot_hour,
                 total_controls, compliant_controls, partial_controls, non_compliant_controls, not_started_controls, compliance_score,
                 total_risks, critical_risks, high_risks, medium_risks, low_risks, open_risks, mitigated_risks, risk_score_avg,
                 total_tasks, pending_tasks, in_progress_tasks, completed_tasks, overdue_tasks, due_this_week,
                 total_evidence, evidence_submitted, evidence_approved, evidence_rejected, evidence_pending,
                 total_assessments, active_assessments, completed_assessments,
                 total_plans, active_plans, completed_plans, overall_plan_progress,
                 created_at, updated_at)
                VALUES (
                    '{snapshot.TenantId}', '{snapshot.SnapshotDate:yyyy-MM-dd}', '{snapshot.SnapshotHour:yyyy-MM-dd HH:mm:ss}',
                    {snapshot.TotalControls}, {snapshot.CompliantControls}, {snapshot.PartialControls}, {snapshot.NonCompliantControls}, {snapshot.NotStartedControls}, {snapshot.ComplianceScore},
                    {snapshot.TotalRisks}, {snapshot.CriticalRisks}, {snapshot.HighRisks}, {snapshot.MediumRisks}, {snapshot.LowRisks}, {snapshot.OpenRisks}, {snapshot.MitigatedRisks}, {snapshot.RiskScoreAvg},
                    {snapshot.TotalTasks}, {snapshot.PendingTasks}, {snapshot.InProgressTasks}, {snapshot.CompletedTasks}, {snapshot.OverdueTasks}, {snapshot.DueThisWeek},
                    {snapshot.TotalEvidence}, {snapshot.EvidenceSubmitted}, {snapshot.EvidenceApproved}, {snapshot.EvidenceRejected}, {snapshot.EvidencePending},
                    {snapshot.TotalAssessments}, {snapshot.ActiveAssessments}, {snapshot.CompletedAssessments},
                    {snapshot.TotalPlans}, {snapshot.ActivePlans}, {snapshot.CompletedPlans}, {snapshot.OverallPlanProgress},
                    now(), now()
                )";

            await ExecuteNonQueryAsync(query);
        }

        #endregion

        #region Compliance Trends

        public async Task<List<ComplianceTrendDto>> GetComplianceTrendsAsync(Guid tenantId, int months = 12)
        {
            var fromDate = DateTime.UtcNow.AddMonths(-months);
            var query = $@"
                SELECT
                    tenant_id, framework_code, baseline_code, measure_date, measure_hour,
                    compliance_score, total_controls, compliant_controls, partial_controls, non_compliant_controls,
                    delta_from_previous
                FROM {_settings.Database}.compliance_trends
                WHERE tenant_id = '{tenantId}'
                  AND measure_date >= '{fromDate:yyyy-MM-dd}'
                ORDER BY measure_date ASC
                FORMAT JSONEachRow";

            return await ExecuteQueryAsync<ComplianceTrendDto>(query);
        }

        public async Task<List<ComplianceTrendDto>> GetComplianceTrendsByFrameworkAsync(Guid tenantId, string frameworkCode, int months = 12)
        {
            var fromDate = DateTime.UtcNow.AddMonths(-months);
            var query = $@"
                SELECT *
                FROM {_settings.Database}.compliance_trends
                WHERE tenant_id = '{tenantId}'
                  AND framework_code = '{frameworkCode}'
                  AND measure_date >= '{fromDate:yyyy-MM-dd}'
                ORDER BY measure_date ASC
                FORMAT JSONEachRow";

            return await ExecuteQueryAsync<ComplianceTrendDto>(query);
        }

        public async Task UpsertComplianceTrendAsync(ComplianceTrendDto trend)
        {
            var query = $@"
                INSERT INTO {_settings.Database}.compliance_trends
                (tenant_id, framework_code, baseline_code, measure_date, measure_hour,
                 compliance_score, total_controls, compliant_controls, partial_controls, non_compliant_controls,
                 delta_from_previous, created_at)
                VALUES (
                    '{trend.TenantId}', '{trend.FrameworkCode}', '{trend.BaselineCode}',
                    '{trend.MeasureDate:yyyy-MM-dd}', '{trend.MeasureHour:yyyy-MM-dd HH:mm:ss}',
                    {trend.ComplianceScore}, {trend.TotalControls}, {trend.CompliantControls},
                    {trend.PartialControls}, {trend.NonCompliantControls}, {trend.DeltaFromPrevious}, now()
                )";

            await ExecuteNonQueryAsync(query);
        }

        #endregion

        #region Risk Heatmap

        public async Task<List<RiskHeatmapCell>> GetRiskHeatmapAsync(Guid tenantId)
        {
            var query = $@"
                SELECT likelihood, impact, sum(risk_count) as risk_count
                FROM {_settings.Database}.risk_heatmap
                WHERE tenant_id = '{tenantId}'
                  AND snapshot_date = today()
                GROUP BY likelihood, impact
                ORDER BY likelihood, impact
                FORMAT JSONEachRow";

            return await ExecuteQueryAsync<RiskHeatmapCell>(query);
        }

        public async Task UpsertRiskHeatmapAsync(Guid tenantId, List<RiskHeatmapCell> cells)
        {
            foreach (var cell in cells)
            {
                var riskIdsArray = cell.RiskIds.Any()
                    ? $"[{string.Join(",", cell.RiskIds.Select(r => $"'{r}'"))}]"
                    : "[]";

                var query = $@"
                    INSERT INTO {_settings.Database}.risk_heatmap
                    (tenant_id, snapshot_date, likelihood, impact, risk_count, risk_ids, created_at)
                    VALUES ('{tenantId}', today(), {cell.Likelihood}, {cell.Impact}, {cell.RiskCount}, {riskIdsArray}, now())";

                await ExecuteNonQueryAsync(query);
            }
        }

        #endregion

        #region Framework Comparison

        public async Task<List<FrameworkComparisonDto>> GetFrameworkComparisonAsync(Guid tenantId)
        {
            var query = $@"
                SELECT *
                FROM {_settings.Database}.framework_comparison
                WHERE tenant_id = '{tenantId}'
                  AND snapshot_date = today()
                ORDER BY compliance_score DESC
                FORMAT JSONEachRow";

            return await ExecuteQueryAsync<FrameworkComparisonDto>(query);
        }

        public async Task UpsertFrameworkComparisonAsync(FrameworkComparisonDto comparison)
        {
            var query = $@"
                INSERT INTO {_settings.Database}.framework_comparison
                (tenant_id, snapshot_date, framework_code, framework_name,
                 total_requirements, compliant_count, partial_count, non_compliant_count, not_assessed_count,
                 compliance_score, maturity_level, trend_7d, trend_30d, created_at)
                VALUES (
                    '{comparison.TenantId}', '{comparison.SnapshotDate:yyyy-MM-dd}',
                    '{comparison.FrameworkCode}', '{comparison.FrameworkName}',
                    {comparison.TotalRequirements}, {comparison.CompliantCount}, {comparison.PartialCount},
                    {comparison.NonCompliantCount}, {comparison.NotAssessedCount},
                    {comparison.ComplianceScore}, {comparison.MaturityLevel}, {comparison.Trend7d}, {comparison.Trend30d}, now()
                )";

            await ExecuteNonQueryAsync(query);
        }

        #endregion

        #region Task Metrics

        public async Task<List<TaskMetricsByRoleDto>> GetTaskMetricsByRoleAsync(Guid tenantId)
        {
            var query = $@"
                SELECT *
                FROM {_settings.Database}.task_metrics_by_role
                WHERE tenant_id = '{tenantId}'
                  AND snapshot_date = today()
                ORDER BY total_tasks DESC
                FORMAT JSONEachRow";

            return await ExecuteQueryAsync<TaskMetricsByRoleDto>(query);
        }

        public async Task UpsertTaskMetricsByRoleAsync(TaskMetricsByRoleDto metrics)
        {
            var query = $@"
                INSERT INTO {_settings.Database}.task_metrics_by_role
                (tenant_id, snapshot_date, role_code, team_id,
                 total_tasks, pending_tasks, in_progress_tasks, completed_tasks, overdue_tasks,
                 avg_completion_days, sla_compliance_rate, created_at)
                VALUES (
                    '{metrics.TenantId}', '{metrics.SnapshotDate:yyyy-MM-dd}', '{metrics.RoleCode}', '{metrics.TeamId}',
                    {metrics.TotalTasks}, {metrics.PendingTasks}, {metrics.InProgressTasks},
                    {metrics.CompletedTasks}, {metrics.OverdueTasks},
                    {metrics.AvgCompletionDays}, {metrics.SlaComplianceRate}, now()
                )";

            await ExecuteNonQueryAsync(query);
        }

        #endregion

        #region Evidence Metrics

        public async Task<List<EvidenceMetricsDto>> GetEvidenceMetricsAsync(Guid tenantId)
        {
            var query = $@"
                SELECT *
                FROM {_settings.Database}.evidence_metrics
                WHERE tenant_id = '{tenantId}'
                  AND snapshot_date = today()
                ORDER BY evidence_type
                FORMAT JSONEachRow";

            return await ExecuteQueryAsync<EvidenceMetricsDto>(query);
        }

        public async Task UpsertEvidenceMetricsAsync(EvidenceMetricsDto metrics)
        {
            var query = $@"
                INSERT INTO {_settings.Database}.evidence_metrics
                (tenant_id, snapshot_date, evidence_type, control_domain,
                 total_required, total_collected, total_approved, total_rejected, total_expired,
                 collection_rate, approval_rate, avg_review_days, created_at)
                VALUES (
                    '{metrics.TenantId}', '{metrics.SnapshotDate:yyyy-MM-dd}',
                    '{metrics.EvidenceType}', '{metrics.ControlDomain}',
                    {metrics.TotalRequired}, {metrics.TotalCollected}, {metrics.TotalApproved},
                    {metrics.TotalRejected}, {metrics.TotalExpired},
                    {metrics.CollectionRate}, {metrics.ApprovalRate}, {metrics.AvgReviewDays}, now()
                )";

            await ExecuteNonQueryAsync(query);
        }

        #endregion

        #region Top Actions

        public async Task<List<TopActionDto>> GetTopActionsAsync(Guid tenantId, int limit = 10)
        {
            var query = $@"
                SELECT *
                FROM {_settings.Database}.top_actions
                WHERE tenant_id = '{tenantId}'
                  AND snapshot_date = today()
                ORDER BY action_rank ASC
                LIMIT {limit}
                FORMAT JSONEachRow";

            return await ExecuteQueryAsync<TopActionDto>(query);
        }

        public async Task UpsertTopActionsAsync(Guid tenantId, List<TopActionDto> actions)
        {
            // Clear existing actions for today
            await ExecuteNonQueryAsync($@"
                ALTER TABLE {_settings.Database}.top_actions
                DELETE WHERE tenant_id = '{tenantId}' AND snapshot_date = today()");

            foreach (var action in actions)
            {
                var dueDateStr = action.DueDate.HasValue
                    ? $"'{action.DueDate.Value:yyyy-MM-dd HH:mm:ss}'"
                    : "NULL";

                var query = $@"
                    INSERT INTO {_settings.Database}.top_actions
                    (tenant_id, snapshot_date, action_rank, action_type, action_title, action_description,
                     entity_type, entity_id, urgency, due_date, assigned_to, created_at)
                    VALUES (
                        '{tenantId}', today(), {action.ActionRank}, '{action.ActionType}',
                        '{EscapeString(action.ActionTitle)}', '{EscapeString(action.ActionDescription)}',
                        '{action.EntityType}', '{action.EntityId}', '{action.Urgency}',
                        {dueDateStr}, '{action.AssignedTo}', now()
                    )";

                await ExecuteNonQueryAsync(query);
            }
        }

        #endregion

        #region User Activity

        public async Task<List<UserActivityDto>> GetUserActivityAsync(Guid tenantId, DateTime from, DateTime to)
        {
            var query = $@"
                SELECT *
                FROM {_settings.Database}.user_activity
                WHERE tenant_id = '{tenantId}'
                  AND activity_date >= '{from:yyyy-MM-dd}'
                  AND activity_date <= '{to:yyyy-MM-dd}'
                ORDER BY activity_date DESC, user_id
                FORMAT JSONEachRow";

            return await ExecuteQueryAsync<UserActivityDto>(query);
        }

        public async Task UpsertUserActivityAsync(UserActivityDto activity)
        {
            var query = $@"
                INSERT INTO {_settings.Database}.user_activity
                (tenant_id, user_id, activity_date,
                 login_count, tasks_completed, evidence_submitted, assessments_worked, approvals_given,
                 session_minutes, last_activity, created_at)
                VALUES (
                    '{activity.TenantId}', '{activity.UserId}', '{activity.ActivityDate:yyyy-MM-dd}',
                    {activity.LoginCount}, {activity.TasksCompleted}, {activity.EvidenceSubmitted},
                    {activity.AssessmentsWorked}, {activity.ApprovalsGiven},
                    {activity.SessionMinutes}, '{activity.LastActivity:yyyy-MM-dd HH:mm:ss}', now()
                )";

            await ExecuteNonQueryAsync(query);
        }

        #endregion

        #region Events

        public async Task InsertEventAsync(AnalyticsEventDto analyticsEvent)
        {
            var query = $@"
                INSERT INTO {_settings.Database}.events_raw
                (event_id, tenant_id, event_type, entity_type, entity_id, action, actor, payload, event_timestamp, ingested_at)
                VALUES (
                    '{analyticsEvent.EventId}', '{analyticsEvent.TenantId}', '{analyticsEvent.EventType}',
                    '{analyticsEvent.EntityType}', '{analyticsEvent.EntityId}', '{analyticsEvent.Action}',
                    '{analyticsEvent.Actor}', '{EscapeString(analyticsEvent.Payload)}',
                    '{analyticsEvent.EventTimestamp:yyyy-MM-dd HH:mm:ss}', now()
                )";

            await ExecuteNonQueryAsync(query);
        }

        public async Task<List<AnalyticsEventDto>> GetRecentEventsAsync(Guid tenantId, int limit = 100)
        {
            var query = $@"
                SELECT *
                FROM {_settings.Database}.events_raw
                WHERE tenant_id = '{tenantId}'
                ORDER BY event_timestamp DESC
                LIMIT {limit}
                FORMAT JSONEachRow";

            return await ExecuteQueryAsync<AnalyticsEventDto>(query);
        }

        #endregion

        #region Health Check

        public async Task<bool> IsHealthyAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/ping");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "ClickHouse health check failed");
                return false;
            }
        }

        #endregion

        #region Private Methods

        private async Task<List<T>> ExecuteQueryAsync<T>(string query) where T : class
        {
            try
            {
                var content = new StringContent(query, Encoding.UTF8, "text/plain");
                var response = await _httpClient.PostAsync($"{_baseUrl}/", content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("ClickHouse query failed: {Error}", error);
                    return new List<T>();
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(jsonResponse))
                    return new List<T>();

                // Parse JSONEachRow format (newline-delimited JSON)
                var results = new List<T>();
                foreach (var line in jsonResponse.Split('\n', StringSplitOptions.RemoveEmptyEntries))
                {
                    var item = JsonSerializer.Deserialize<T>(line, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    if (item != null)
                        results.Add(item);
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing ClickHouse query");
                return new List<T>();
            }
        }

        private async Task ExecuteNonQueryAsync(string query)
        {
            try
            {
                var content = new StringContent(query, Encoding.UTF8, "text/plain");
                var response = await _httpClient.PostAsync($"{_baseUrl}/", content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("ClickHouse command failed: {Error}", error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing ClickHouse command");
            }
        }

        private static string EscapeString(string value)
        {
            return value?.Replace("'", "\\'").Replace("\\", "\\\\") ?? string.Empty;
        }

        #endregion
    }
}
