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
            // SECURITY: Validate tenantId before use in SQL
            var safeTenantId = EscapeGuid(tenantId);
            var query = $@"
                SELECT *
                FROM {_settings.Database}.dashboard_snapshots
                WHERE tenant_id = '{safeTenantId}'
                ORDER BY snapshot_hour DESC
                LIMIT 1
                FORMAT JSONEachRow";

            var results = await ExecuteQueryAsync<DashboardSnapshotDto>(query);
            return results.FirstOrDefault();
        }

        public async Task<List<DashboardSnapshotDto>> GetSnapshotHistoryAsync(Guid tenantId, DateTime from, DateTime to)
        {
            // SECURITY: Validate tenantId before use in SQL
            var safeTenantId = EscapeGuid(tenantId);
            var query = $@"
                SELECT *
                FROM {_settings.Database}.dashboard_snapshots
                WHERE tenant_id = '{safeTenantId}'
                  AND snapshot_date >= '{from:yyyy-MM-dd}'
                  AND snapshot_date <= '{to:yyyy-MM-dd}'
                ORDER BY snapshot_hour ASC
                FORMAT JSONEachRow";

            return await ExecuteQueryAsync<DashboardSnapshotDto>(query);
        }

        public async Task UpsertSnapshotAsync(DashboardSnapshotDto snapshot)
        {
            // SECURITY: Validate tenantId and numeric values before use in SQL
            var safeTenantId = EscapeGuid(snapshot.TenantId);
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
                    '{safeTenantId}', '{snapshot.SnapshotDate:yyyy-MM-dd}', '{snapshot.SnapshotHour:yyyy-MM-dd HH:mm:ss}',
                    {ValidateNumeric(snapshot.TotalControls)}, {ValidateNumeric(snapshot.CompliantControls)}, {ValidateNumeric(snapshot.PartialControls)}, {ValidateNumeric(snapshot.NonCompliantControls)}, {ValidateNumeric(snapshot.NotStartedControls)}, {ValidateNumeric(snapshot.ComplianceScore)},
                    {ValidateNumeric(snapshot.TotalRisks)}, {ValidateNumeric(snapshot.CriticalRisks)}, {ValidateNumeric(snapshot.HighRisks)}, {ValidateNumeric(snapshot.MediumRisks)}, {ValidateNumeric(snapshot.LowRisks)}, {ValidateNumeric(snapshot.OpenRisks)}, {ValidateNumeric(snapshot.MitigatedRisks)}, {ValidateNumeric(snapshot.RiskScoreAvg)},
                    {ValidateNumeric(snapshot.TotalTasks)}, {ValidateNumeric(snapshot.PendingTasks)}, {ValidateNumeric(snapshot.InProgressTasks)}, {ValidateNumeric(snapshot.CompletedTasks)}, {ValidateNumeric(snapshot.OverdueTasks)}, {ValidateNumeric(snapshot.DueThisWeek)},
                    {ValidateNumeric(snapshot.TotalEvidence)}, {ValidateNumeric(snapshot.EvidenceSubmitted)}, {ValidateNumeric(snapshot.EvidenceApproved)}, {ValidateNumeric(snapshot.EvidenceRejected)}, {ValidateNumeric(snapshot.EvidencePending)},
                    {ValidateNumeric(snapshot.TotalAssessments)}, {ValidateNumeric(snapshot.ActiveAssessments)}, {ValidateNumeric(snapshot.CompletedAssessments)},
                    {ValidateNumeric(snapshot.TotalPlans)}, {ValidateNumeric(snapshot.ActivePlans)}, {ValidateNumeric(snapshot.CompletedPlans)}, {ValidateNumeric(snapshot.OverallPlanProgress)},
                    now(), now()
                )";

            await ExecuteNonQueryAsync(query);
        }

        #endregion

        #region Compliance Trends

        public async Task<List<ComplianceTrendDto>> GetComplianceTrendsAsync(Guid tenantId, int months = 12)
        {
            // SECURITY: Validate tenantId before use in SQL
            var safeTenantId = EscapeGuid(tenantId);
            var fromDate = DateTime.UtcNow.AddMonths(-months);
            var query = $@"
                SELECT
                    tenant_id, framework_code, baseline_code, measure_date, measure_hour,
                    compliance_score, total_controls, compliant_controls, partial_controls, non_compliant_controls,
                    delta_from_previous
                FROM {_settings.Database}.compliance_trends
                WHERE tenant_id = '{safeTenantId}'
                  AND measure_date >= '{fromDate:yyyy-MM-dd}'
                ORDER BY measure_date ASC
                FORMAT JSONEachRow";

            return await ExecuteQueryAsync<ComplianceTrendDto>(query);
        }

        public async Task<List<ComplianceTrendDto>> GetComplianceTrendsByFrameworkAsync(Guid tenantId, string frameworkCode, int months = 12)
        {
            // SECURITY: Validate tenantId before use in SQL
            var safeTenantId = EscapeGuid(tenantId);
            var fromDate = DateTime.UtcNow.AddMonths(-months);
            var query = $@"
                SELECT *
                FROM {_settings.Database}.compliance_trends
                WHERE tenant_id = '{safeTenantId}'
                  AND framework_code = '{EscapeString(frameworkCode)}'
                  AND measure_date >= '{fromDate:yyyy-MM-dd}'
                ORDER BY measure_date ASC
                FORMAT JSONEachRow";

            return await ExecuteQueryAsync<ComplianceTrendDto>(query);
        }

        public async Task UpsertComplianceTrendAsync(ComplianceTrendDto trend)
        {
            // SECURITY: Validate tenantId and numeric values before use in SQL
            var safeTenantId = EscapeGuid(trend.TenantId);
            var query = $@"
                INSERT INTO {_settings.Database}.compliance_trends
                (tenant_id, framework_code, baseline_code, measure_date, measure_hour,
                 compliance_score, total_controls, compliant_controls, partial_controls, non_compliant_controls,
                 delta_from_previous, created_at)
                VALUES (
                    '{safeTenantId}', '{EscapeString(trend.FrameworkCode ?? string.Empty)}', '{EscapeString(trend.BaselineCode ?? string.Empty)}',
                    '{trend.MeasureDate:yyyy-MM-dd}', '{trend.MeasureHour:yyyy-MM-dd HH:mm:ss}',
                    {ValidateNumeric(trend.ComplianceScore)}, {ValidateNumeric(trend.TotalControls)}, {ValidateNumeric(trend.CompliantControls)},
                    {ValidateNumeric(trend.PartialControls)}, {ValidateNumeric(trend.NonCompliantControls)}, {ValidateNumeric(trend.DeltaFromPrevious)}, now()
                )";

            await ExecuteNonQueryAsync(query);
        }

        #endregion

        #region Risk Heatmap

        public async Task<List<RiskHeatmapCell>> GetRiskHeatmapAsync(Guid tenantId)
        {
            // SECURITY: Validate tenantId before use in SQL
            var safeTenantId = EscapeGuid(tenantId);
            var query = $@"
                SELECT likelihood, impact, sum(risk_count) as risk_count
                FROM {_settings.Database}.risk_heatmap
                WHERE tenant_id = '{safeTenantId}'
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
                // SECURITY: Escape all Guid values in array to prevent SQL injection
                var riskIdsArray = cell.RiskIds.Any()
                    ? $"[{string.Join(",", cell.RiskIds.Select(r => $"'{EscapeGuid(r)}'"))}]"
                    : "[]";

                var query = $@"
                    INSERT INTO {_settings.Database}.risk_heatmap
                    (tenant_id, snapshot_date, likelihood, impact, risk_count, risk_ids, created_at)
                    VALUES ('{EscapeGuid(tenantId)}', today(), {ValidateNumeric(cell.Likelihood)}, {ValidateNumeric(cell.Impact)}, {ValidateNumeric(cell.RiskCount)}, {riskIdsArray}, now())";

                await ExecuteNonQueryAsync(query);
            }
        }

        #endregion

        #region Framework Comparison

        public async Task<List<FrameworkComparisonDto>> GetFrameworkComparisonAsync(Guid tenantId)
        {
            // SECURITY: Validate tenantId before use in SQL
            var safeTenantId = EscapeGuid(tenantId);
            var query = $@"
                SELECT *
                FROM {_settings.Database}.framework_comparison
                WHERE tenant_id = '{safeTenantId}'
                  AND snapshot_date = today()
                ORDER BY compliance_score DESC
                FORMAT JSONEachRow";

            return await ExecuteQueryAsync<FrameworkComparisonDto>(query);
        }

        public async Task UpsertFrameworkComparisonAsync(FrameworkComparisonDto comparison)
        {
            // SECURITY: Validate tenantId and numeric values before use in SQL
            var safeTenantId = EscapeGuid(comparison.TenantId);
            var query = $@"
                INSERT INTO {_settings.Database}.framework_comparison
                (tenant_id, snapshot_date, framework_code, framework_name,
                 total_requirements, compliant_count, partial_count, non_compliant_count, not_assessed_count,
                 compliance_score, maturity_level, trend_7d, trend_30d, created_at)
                VALUES (
                    '{safeTenantId}', '{comparison.SnapshotDate:yyyy-MM-dd}',
                    '{EscapeString(comparison.FrameworkCode ?? string.Empty)}', '{EscapeString(comparison.FrameworkName ?? string.Empty)}',
                    {ValidateNumeric(comparison.TotalRequirements)}, {ValidateNumeric(comparison.CompliantCount)}, {ValidateNumeric(comparison.PartialCount)},
                    {ValidateNumeric(comparison.NonCompliantCount)}, {ValidateNumeric(comparison.NotAssessedCount)},
                    {ValidateNumeric(comparison.ComplianceScore)}, {ValidateNumeric(comparison.MaturityLevel)}, {ValidateNumeric(comparison.Trend7d)}, {ValidateNumeric(comparison.Trend30d)}, now()
                )";

            await ExecuteNonQueryAsync(query);
        }

        #endregion

        #region Task Metrics

        public async Task<List<TaskMetricsByRoleDto>> GetTaskMetricsByRoleAsync(Guid tenantId)
        {
            // SECURITY: Validate tenantId before use in SQL
            var safeTenantId = EscapeGuid(tenantId);
            var query = $@"
                SELECT *
                FROM {_settings.Database}.task_metrics_by_role
                WHERE tenant_id = '{safeTenantId}'
                  AND snapshot_date = today()
                ORDER BY total_tasks DESC
                FORMAT JSONEachRow";

            return await ExecuteQueryAsync<TaskMetricsByRoleDto>(query);
        }

        public async Task UpsertTaskMetricsByRoleAsync(TaskMetricsByRoleDto metrics)
        {
            // SECURITY: Validate tenantId, teamId, and numeric values before use in SQL
            var safeTenantId = EscapeGuid(metrics.TenantId);
            var safeTeamId = EscapeGuid(metrics.TeamId);
            var query = $@"
                INSERT INTO {_settings.Database}.task_metrics_by_role
                (tenant_id, snapshot_date, role_code, team_id,
                 total_tasks, pending_tasks, in_progress_tasks, completed_tasks, overdue_tasks,
                 avg_completion_days, sla_compliance_rate, created_at)
                VALUES (
                    '{safeTenantId}', '{metrics.SnapshotDate:yyyy-MM-dd}', '{EscapeString(metrics.RoleCode ?? string.Empty)}', '{safeTeamId}',
                    {ValidateNumeric(metrics.TotalTasks)}, {ValidateNumeric(metrics.PendingTasks)}, {ValidateNumeric(metrics.InProgressTasks)},
                    {ValidateNumeric(metrics.CompletedTasks)}, {ValidateNumeric(metrics.OverdueTasks)},
                    {ValidateNumeric(metrics.AvgCompletionDays)}, {ValidateNumeric(metrics.SlaComplianceRate)}, now()
                )";

            await ExecuteNonQueryAsync(query);
        }

        #endregion

        #region Evidence Metrics

        public async Task<List<EvidenceMetricsDto>> GetEvidenceMetricsAsync(Guid tenantId)
        {
            // SECURITY: Validate tenantId before use in SQL
            var safeTenantId = EscapeGuid(tenantId);
            var query = $@"
                SELECT *
                FROM {_settings.Database}.evidence_metrics
                WHERE tenant_id = '{safeTenantId}'
                  AND snapshot_date = today()
                ORDER BY evidence_type
                FORMAT JSONEachRow";

            return await ExecuteQueryAsync<EvidenceMetricsDto>(query);
        }

        public async Task UpsertEvidenceMetricsAsync(EvidenceMetricsDto metrics)
        {
            // SECURITY: Validate tenantId and numeric values before use in SQL
            var safeTenantId = EscapeGuid(metrics.TenantId);
            var query = $@"
                INSERT INTO {_settings.Database}.evidence_metrics
                (tenant_id, snapshot_date, evidence_type, control_domain,
                 total_required, total_collected, total_approved, total_rejected, total_expired,
                 collection_rate, approval_rate, avg_review_days, created_at)
                VALUES (
                    '{safeTenantId}', '{metrics.SnapshotDate:yyyy-MM-dd}',
                    '{EscapeString(metrics.EvidenceType ?? string.Empty)}', '{EscapeString(metrics.ControlDomain ?? string.Empty)}',
                    {ValidateNumeric(metrics.TotalRequired)}, {ValidateNumeric(metrics.TotalCollected)}, {ValidateNumeric(metrics.TotalApproved)},
                    {ValidateNumeric(metrics.TotalRejected)}, {ValidateNumeric(metrics.TotalExpired)},
                    {ValidateNumeric(metrics.CollectionRate)}, {ValidateNumeric(metrics.ApprovalRate)}, {ValidateNumeric(metrics.AvgReviewDays)}, now()
                )";

            await ExecuteNonQueryAsync(query);
        }

        #endregion

        #region Top Actions

        public async Task<List<TopActionDto>> GetTopActionsAsync(Guid tenantId, int limit = 10)
        {
            // SECURITY: Validate tenantId and limit before use in SQL
            var safeTenantId = EscapeGuid(tenantId);
            var safeLimit = ValidateNumeric(limit);
            var query = $@"
                SELECT *
                FROM {_settings.Database}.top_actions
                WHERE tenant_id = '{safeTenantId}'
                  AND snapshot_date = today()
                ORDER BY action_rank ASC
                LIMIT {safeLimit}
                FORMAT JSONEachRow";

            return await ExecuteQueryAsync<TopActionDto>(query);
        }

        public async Task UpsertTopActionsAsync(Guid tenantId, List<TopActionDto> actions)
        {
            // SECURITY: Validate tenantId before use in SQL
            var safeTenantId = EscapeGuid(tenantId);
            // Clear existing actions for today
            await ExecuteNonQueryAsync($@"
                ALTER TABLE {_settings.Database}.top_actions
                DELETE WHERE tenant_id = '{safeTenantId}' AND snapshot_date = today()");

            foreach (var action in actions)
            {
                var dueDateStr = action.DueDate.HasValue
                    ? $"'{action.DueDate.Value:yyyy-MM-dd HH:mm:ss}'"
                    : "NULL";

                // SECURITY: Validate EntityId and numeric values before use in SQL
                var safeEntityId = EscapeGuid(action.EntityId);
                var query = $@"
                    INSERT INTO {_settings.Database}.top_actions
                    (tenant_id, snapshot_date, action_rank, action_type, action_title, action_description,
                     entity_type, entity_id, urgency, due_date, assigned_to, created_at)
                    VALUES (
                        '{safeTenantId}', today(), {ValidateNumeric(action.ActionRank)}, '{EscapeString(action.ActionType ?? string.Empty)}',
                        '{EscapeString(action.ActionTitle ?? string.Empty)}', '{EscapeString(action.ActionDescription ?? string.Empty)}',
                        '{EscapeString(action.EntityType ?? string.Empty)}', '{safeEntityId}', '{EscapeString(action.Urgency ?? string.Empty)}',
                        {dueDateStr}, '{EscapeString(action.AssignedTo ?? string.Empty)}', now()
                    )";

                await ExecuteNonQueryAsync(query);
            }
        }

        #endregion

        #region User Activity

        public async Task<List<UserActivityDto>> GetUserActivityAsync(Guid tenantId, DateTime from, DateTime to)
        {
            // SECURITY: Validate tenantId before use in SQL
            var safeTenantId = EscapeGuid(tenantId);
            var query = $@"
                SELECT *
                FROM {_settings.Database}.user_activity
                WHERE tenant_id = '{safeTenantId}'
                  AND activity_date >= '{from:yyyy-MM-dd}'
                  AND activity_date <= '{to:yyyy-MM-dd}'
                ORDER BY activity_date DESC, user_id
                FORMAT JSONEachRow";

            return await ExecuteQueryAsync<UserActivityDto>(query);
        }

        public async Task UpsertUserActivityAsync(UserActivityDto activity)
        {
            // SECURITY: Validate tenantId (Guid), userId (string), and numeric values before use in SQL
            var safeTenantId = EscapeGuid(activity.TenantId);
            // UserId is string, not Guid, so use EscapeString
            var safeUserId = EscapeString(activity.UserId ?? string.Empty);
            var query = $@"
                INSERT INTO {_settings.Database}.user_activity
                (tenant_id, user_id, activity_date,
                 login_count, tasks_completed, evidence_submitted, assessments_worked, approvals_given,
                 session_minutes, last_activity, created_at)
                VALUES (
                    '{safeTenantId}', '{safeUserId}', '{activity.ActivityDate:yyyy-MM-dd}',
                    {ValidateNumeric(activity.LoginCount)}, {ValidateNumeric(activity.TasksCompleted)}, {ValidateNumeric(activity.EvidenceSubmitted)},
                    {ValidateNumeric(activity.AssessmentsWorked)}, {ValidateNumeric(activity.ApprovalsGiven)},
                    {ValidateNumeric(activity.SessionMinutes)}, '{activity.LastActivity:yyyy-MM-dd HH:mm:ss}', now()
                )";

            await ExecuteNonQueryAsync(query);
        }

        #endregion

        #region Events

        public async Task InsertEventAsync(AnalyticsEventDto analyticsEvent)
        {
            // SECURITY: Validate EventId, TenantId (Guid), and EntityId (string) before use in SQL
            var safeEventId = EscapeGuid(analyticsEvent.EventId);
            var safeTenantId = EscapeGuid(analyticsEvent.TenantId);
            // EntityId is string, not Guid, so use EscapeString
            var safeEntityId = EscapeString(analyticsEvent.EntityId ?? string.Empty);
            var query = $@"
                INSERT INTO {_settings.Database}.events_raw
                (event_id, tenant_id, event_type, entity_type, entity_id, action, actor, payload, event_timestamp, ingested_at)
                VALUES (
                    '{safeEventId}', '{safeTenantId}', '{EscapeString(analyticsEvent.EventType ?? string.Empty)}',
                    '{EscapeString(analyticsEvent.EntityType ?? string.Empty)}', '{safeEntityId}', '{EscapeString(analyticsEvent.Action ?? string.Empty)}',
                    '{EscapeString(analyticsEvent.Actor ?? string.Empty)}', '{EscapeString(analyticsEvent.Payload ?? string.Empty)}',
                    '{analyticsEvent.EventTimestamp:yyyy-MM-dd HH:mm:ss}', now()
                )";

            await ExecuteNonQueryAsync(query);
        }

        public async Task<List<AnalyticsEventDto>> GetRecentEventsAsync(Guid tenantId, int limit = 100)
        {
            // SECURITY: Validate tenantId and limit before use in SQL
            var safeTenantId = EscapeGuid(tenantId);
            var safeLimit = ValidateNumeric(limit);
            var query = $@"
                SELECT *
                FROM {_settings.Database}.events_raw
                WHERE tenant_id = '{safeTenantId}'
                ORDER BY event_timestamp DESC
                LIMIT {safeLimit}
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

        /// <summary>
        /// Escapes ClickHouse string values to prevent SQL injection
        /// Escapes single quotes and backslashes
        /// </summary>
        private static string EscapeString(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;
            
            // SECURITY: Escape special characters to prevent SQL injection
            // Replace backslash first, then single quote (order matters)
            return value
                .Replace("\\", "\\\\")  // Escape backslashes first
                .Replace("'", "\\'");   // Then escape single quotes
        }

        /// <summary>
        /// Validates and escapes Guid values to prevent SQL injection
        /// SECURITY: Ensures Guid is valid before using in SQL
        /// </summary>
        private static string EscapeGuid(Guid value)
        {
            // SECURITY: Validate Guid format before use
            if (value == Guid.Empty)
                throw new ArgumentException("Guid cannot be empty", nameof(value));
            
            return value.ToString();
        }

        /// <summary>
        /// Validates numeric values to prevent SQL injection
        /// SECURITY: Ensures numeric values are safe for SQL
        /// </summary>
        private static int ValidateNumeric(int value)
        {
            // SECURITY: Basic validation - numeric values are generally safe, but validate range
            if (value < 0)
                throw new ArgumentException("Numeric value cannot be negative", nameof(value));
            
            return value;
        }

        /// <summary>
        /// Validates numeric values to prevent SQL injection
        /// SECURITY: Ensures numeric values are safe for SQL
        /// </summary>
        private static double ValidateNumeric(double value)
        {
            // SECURITY: Basic validation - numeric values are generally safe, but validate range
            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new ArgumentException("Numeric value must be a valid number", nameof(value));
            
            return value;
        }

        /// <summary>
        /// Validates decimal values to prevent SQL injection
        /// SECURITY: Ensures decimal values are safe for SQL
        /// </summary>
        private static decimal ValidateNumeric(decimal value)
        {
            // SECURITY: Basic validation - decimal values are generally safe for SQL
            // No need for range validation as decimal is always valid (unlike double)
            return value;
        }

        #endregion
    }
}
