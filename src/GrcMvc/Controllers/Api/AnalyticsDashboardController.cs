using GrcMvc.Data;
using GrcMvc.Services.Analytics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

using Microsoft.Extensions.Localization;
using GrcMvc.Resources;
namespace GrcMvc.Controllers.Api
{
    /// <summary>
    /// API Controller for analytics dashboard data from ClickHouse OLAP
    /// Provides fast, pre-aggregated metrics for dashboard widgets
    /// Falls back to SQL Server when ClickHouse is unavailable
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AnalyticsDashboardController : ControllerBase
    {
        private readonly IClickHouseService _clickHouse;
        private readonly IDashboardProjector _projector;
        private readonly GrcDbContext _dbContext;
        private readonly ILogger<AnalyticsDashboardController> _logger;

        public AnalyticsDashboardController(
            IClickHouseService clickHouse,
            IDashboardProjector projector,
            GrcDbContext dbContext,
            ILogger<AnalyticsDashboardController> logger)
        {
            _clickHouse = clickHouse;
            _projector = projector;
            _dbContext = dbContext;
            _logger = logger;
        }

        private Guid GetTenantId()
        {
            var tenantClaim = User.FindFirst("TenantId")?.Value;
            return Guid.TryParse(tenantClaim, out var tenantId) ? tenantId : Guid.Empty;
        }

        private async Task<bool> IsClickHouseAvailable()
        {
            try
            {
                return await _clickHouse.IsHealthyAsync();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get latest dashboard snapshot with all metrics
        /// </summary>
        [HttpGet("snapshot")]
        public async Task<IActionResult> GetSnapshot()
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty)
                return BadRequest("Invalid tenant");

            var snapshot = await _clickHouse.GetLatestSnapshotAsync(tenantId);
            if (snapshot == null)
            {
                // Trigger projection if no snapshot exists
                await _projector.ProjectSnapshotAsync(tenantId);
                snapshot = await _clickHouse.GetLatestSnapshotAsync(tenantId);
            }

            return Ok(snapshot);
        }

        /// <summary>
        /// Get snapshot history for trend analysis
        /// </summary>
        [HttpGet("snapshot/history")]
        public async Task<IActionResult> GetSnapshotHistory(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty)
                return BadRequest("Invalid tenant");

            var fromDate = from ?? DateTime.UtcNow.AddDays(-30);
            var toDate = to ?? DateTime.UtcNow;

            var history = await _clickHouse.GetSnapshotHistoryAsync(tenantId, fromDate, toDate);
            return Ok(history);
        }

        /// <summary>
        /// Get compliance trends over time
        /// </summary>
        [HttpGet("compliance/trends")]
        public async Task<IActionResult> GetComplianceTrends([FromQuery] int months = 12)
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty)
                return BadRequest("Invalid tenant");

            var trends = await _clickHouse.GetComplianceTrendsAsync(tenantId, months);
            return Ok(trends);
        }

        /// <summary>
        /// Get compliance trends for specific framework
        /// </summary>
        [HttpGet("compliance/trends/{frameworkCode}")]
        public async Task<IActionResult> GetComplianceTrendsByFramework(
            string frameworkCode,
            [FromQuery] int months = 12)
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty)
                return BadRequest("Invalid tenant");

            var trends = await _clickHouse.GetComplianceTrendsByFrameworkAsync(tenantId, frameworkCode, months);
            return Ok(trends);
        }

        /// <summary>
        /// Get risk heatmap data (5x5 matrix)
        /// </summary>
        [HttpGet("risk/heatmap")]
        public async Task<IActionResult> GetRiskHeatmap()
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty)
                return BadRequest("Invalid tenant");

            var heatmap = await _clickHouse.GetRiskHeatmapAsync(tenantId);

            // If empty, trigger projection
            if (!heatmap.Any())
            {
                await _projector.ProjectRiskHeatmapAsync(tenantId);
                heatmap = await _clickHouse.GetRiskHeatmapAsync(tenantId);
            }

            return Ok(heatmap);
        }

        /// <summary>
        /// Get framework comparison view
        /// </summary>
        [HttpGet("frameworks/comparison")]
        public async Task<IActionResult> GetFrameworkComparison()
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty)
                return BadRequest("Invalid tenant");

            var comparison = await _clickHouse.GetFrameworkComparisonAsync(tenantId);
            return Ok(comparison);
        }

        /// <summary>
        /// Get task metrics by role
        /// </summary>
        [HttpGet("tasks/by-role")]
        public async Task<IActionResult> GetTaskMetricsByRole()
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty)
                return BadRequest("Invalid tenant");

            var metrics = await _clickHouse.GetTaskMetricsByRoleAsync(tenantId);
            return Ok(metrics);
        }

        /// <summary>
        /// Get evidence collection metrics
        /// </summary>
        [HttpGet("evidence/metrics")]
        public async Task<IActionResult> GetEvidenceMetrics()
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty)
                return BadRequest("Invalid tenant");

            var metrics = await _clickHouse.GetEvidenceMetricsAsync(tenantId);
            return Ok(metrics);
        }

        /// <summary>
        /// Get top priority actions
        /// </summary>
        [HttpGet("top-actions")]
        public async Task<IActionResult> GetTopActions([FromQuery] int limit = 10)
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty)
                return BadRequest("Invalid tenant");

            var actions = await _clickHouse.GetTopActionsAsync(tenantId, limit);

            // If empty, trigger projection
            if (!actions.Any())
            {
                await _projector.ProjectTopActionsAsync(tenantId);
                actions = await _clickHouse.GetTopActionsAsync(tenantId, limit);
            }

            return Ok(actions);
        }

        /// <summary>
        /// Get user activity metrics
        /// </summary>
        [HttpGet("users/activity")]
        public async Task<IActionResult> GetUserActivity(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty)
                return BadRequest("Invalid tenant");

            var fromDate = from ?? DateTime.UtcNow.AddDays(-30);
            var toDate = to ?? DateTime.UtcNow;

            var activity = await _clickHouse.GetUserActivityAsync(tenantId, fromDate, toDate);
            return Ok(activity);
        }

        /// <summary>
        /// Get recent events
        /// </summary>
        [HttpGet("events/recent")]
        public async Task<IActionResult> GetRecentEvents([FromQuery] int limit = 100)
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty)
                return BadRequest("Invalid tenant");

            var events = await _clickHouse.GetRecentEventsAsync(tenantId, limit);
            return Ok(events);
        }

        /// <summary>
        /// Trigger manual projection refresh
        /// </summary>
        [HttpPost("refresh")]
        [Authorize(Roles = "PlatformAdmin,TenantAdmin")]
        public async Task<IActionResult> RefreshAnalytics()
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty)
                return BadRequest("Invalid tenant");

            _logger.LogInformation("Manual analytics refresh triggered for tenant {TenantId}", tenantId);

            await _projector.ProjectAllAsync(tenantId);

            return Ok(new { message = "Analytics refresh completed", timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// Check ClickHouse health status
        /// </summary>
        [HttpGet("health")]
        public async Task<IActionResult> GetHealth()
        {
            var isHealthy = await _clickHouse.IsHealthyAsync();
            return Ok(new
            {
                clickhouse = isHealthy ? "healthy" : "unavailable",
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Get executive summary (all key metrics in one call)
        /// Falls back to SQL Server if ClickHouse unavailable
        /// </summary>
        [HttpGet("executive-summary")]
        public async Task<IActionResult> GetExecutiveSummary()
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty)
                return BadRequest("Invalid tenant");

            // Try ClickHouse first
            if (await IsClickHouseAvailable())
            {
                try
                {
                    var snapshot = await _clickHouse.GetLatestSnapshotAsync(tenantId);
                    var topActions = await _clickHouse.GetTopActionsAsync(tenantId, 5);
                    var frameworks = await _clickHouse.GetFrameworkComparisonAsync(tenantId);
                    var heatmap = await _clickHouse.GetRiskHeatmapAsync(tenantId);

                    return Ok(new
                    {
                        Snapshot = snapshot,
                        TopActions = topActions,
                        Frameworks = frameworks,
                        RiskHeatmap = heatmap,
                        GeneratedAt = DateTime.UtcNow,
                        Source = "ClickHouse"
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "ClickHouse query failed, falling back to SQL Server");
                }
            }

            // Fallback to SQL Server
            return Ok(await GetExecutiveSummaryFromSqlServer(tenantId));
        }

        /// <summary>
        /// Get compliance trends - with SQL Server fallback
        /// </summary>
        [HttpGet("compliance-trends")]
        public async Task<IActionResult> GetComplianceTrendsWithFallback([FromQuery] int months = 12)
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty)
                return BadRequest("Invalid tenant");

            // Try ClickHouse first
            if (await IsClickHouseAvailable())
            {
                try
                {
                    var trends = await _clickHouse.GetComplianceTrendsAsync(tenantId, months);
                    if (trends.Any())
                        return Ok(trends);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "ClickHouse compliance trends query failed");
                }
            }

            // Fallback to SQL Server
            return Ok(await GetComplianceTrendsFromSqlServer(tenantId, months));
        }

        /// <summary>
        /// Get top actions - with SQL Server fallback
        /// </summary>
        [HttpGet("priority-actions")]
        public async Task<IActionResult> GetTopActionsWithFallback([FromQuery] int limit = 10)
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty)
                return BadRequest("Invalid tenant");

            // Try ClickHouse first
            if (await IsClickHouseAvailable())
            {
                try
                {
                    var actions = await _clickHouse.GetTopActionsAsync(tenantId, limit);
                    if (actions.Any())
                        return Ok(actions);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "ClickHouse top actions query failed");
                }
            }

            // Fallback to SQL Server
            return Ok(await GetTopActionsFromSqlServer(tenantId, limit));
        }

        /// <summary>
        /// Get framework comparison - with SQL Server fallback
        /// </summary>
        [HttpGet("framework-comparison")]
        public async Task<IActionResult> GetFrameworkComparisonWithFallback()
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty)
                return BadRequest("Invalid tenant");

            // Try ClickHouse first
            if (await IsClickHouseAvailable())
            {
                try
                {
                    var comparison = await _clickHouse.GetFrameworkComparisonAsync(tenantId);
                    if (comparison.Any())
                        return Ok(comparison);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "ClickHouse framework comparison query failed");
                }
            }

            // Fallback to SQL Server
            return Ok(await GetFrameworkComparisonFromSqlServer(tenantId));
        }

        #region SQL Server Fallback Methods

        private async Task<object> GetExecutiveSummaryFromSqlServer(Guid tenantId)
        {
            // Calculate compliance score from assessments
            var assessments = await _dbContext.Assessments
                .Where(a => a.TenantId == tenantId)
                .ToListAsync();

            var complianceScore = assessments.Any()
                ? assessments.Average(a => (double)a.Score)
                : 0;

            // Count open risks
            var openRisks = await _dbContext.Risks
                .CountAsync(r => r.TenantId == tenantId && r.Status != "Closed" && r.Status != "Mitigated");

            // Count assessments
            var totalAssessments = assessments.Count;

            // Count controls
            var totalControls = await _dbContext.Controls
                .CountAsync(c => c.TenantId == tenantId);

            // Get top actions (overdue tasks)
            var topActions = await _dbContext.WorkflowTasks
                .Where(t => t.TenantId == tenantId && t.Status != "Completed" && t.Status != "Cancelled")
                .OrderBy(t => t.DueDate)
                .Take(5)
                .Select(t => new
                {
                    title = t.TaskName,
                    description = t.Description,
                    priority = t.DueDate < DateTime.UtcNow ? "High" : "Medium",
                    dueDate = t.DueDate
                })
                .ToListAsync();

            // Get framework compliance from baselines
            var frameworks = await _dbContext.TenantBaselines
                .Where(b => b.TenantId == tenantId)
                .Select(b => new
                {
                    name = b.BaselineCode,
                    complianceScore = 0 // Compliance score from assessments, not baselines
                })
                .ToListAsync();

            // Risk heatmap from risks
            var risks = await _dbContext.Risks
                .Where(r => r.TenantId == tenantId && r.Status != "Closed")
                .ToListAsync();

            var heatmap = risks
                .GroupBy(r => new { r.Likelihood, r.Impact })
                .Select(g => new
                {
                    likelihood = g.Key.Likelihood,
                    impact = g.Key.Impact,
                    count = g.Count()
                })
                .ToList();

            return new
            {
                complianceScore = Math.Round(complianceScore, 1),
                openRisks,
                totalAssessments,
                totalControls,
                topActions,
                frameworks,
                riskHeatmap = heatmap,
                GeneratedAt = DateTime.UtcNow,
                Source = "SQLServer"
            };
        }

        private async Task<List<object>> GetComplianceTrendsFromSqlServer(Guid tenantId, int months)
        {
            var startDate = DateTime.UtcNow.AddMonths(-months);

            // Get assessments grouped by month
            var assessments = await _dbContext.Assessments
                .Where(a => a.TenantId == tenantId && a.CreatedDate >= startDate)
                .ToListAsync();

            var trends = new List<object>();
            for (int i = months - 1; i >= 0; i--)
            {
                var monthStart = DateTime.UtcNow.AddMonths(-i).Date.AddDays(1 - DateTime.UtcNow.AddMonths(-i).Day);
                var monthEnd = monthStart.AddMonths(1);
                var monthName = monthStart.ToString("MMM yyyy");

                var monthAssessments = assessments
                    .Where(a => a.CreatedDate >= monthStart && a.CreatedDate < monthEnd)
                    .ToList();

                var score = monthAssessments.Any()
                    ? monthAssessments.Average(a => (double)a.Score)
                    : 0;

                trends.Add(new
                {
                    month = monthName,
                    date = monthStart,
                    score = Math.Round(score, 1),
                    complianceScore = Math.Round(score, 1)
                });
            }

            return trends;
        }

        private async Task<List<object>> GetTopActionsFromSqlServer(Guid tenantId, int limit)
        {
            // Get overdue or upcoming tasks
            var tasks = await _dbContext.WorkflowTasks
                .Where(t => t.TenantId == tenantId && t.Status != "Completed" && t.Status != "Cancelled")
                .OrderBy(t => t.DueDate)
                .Take(limit)
                .Select(t => new
                {
                    title = t.TaskName,
                    description = t.Description,
                    priority = t.DueDate < DateTime.UtcNow ? "High" :
                               t.DueDate < DateTime.UtcNow.AddDays(7) ? "Medium" : "Low",
                    dueDate = t.DueDate,
                    assignedTo = t.AssignedToUserName
                })
                .ToListAsync();

            // Also get high-risk items
            var highRisks = await _dbContext.Risks
                .Where(r => r.TenantId == tenantId && r.Status != "Closed" && r.RiskScore >= 8)
                .OrderByDescending(r => r.RiskScore)
                .Take(limit / 2)
                .Select(r => new
                {
                    title = "Address High Risk: " + (r.Title ?? r.Name),
                    description = r.Description,
                    priority = "Critical",
                    dueDate = (DateTime?)null,
                    assignedTo = r.Owner
                })
                .ToListAsync();

            return tasks.Cast<object>().Concat(highRisks.Cast<object>()).Take(limit).ToList();
        }

        private async Task<List<object>> GetFrameworkComparisonFromSqlServer(Guid tenantId)
        {
            // Get active baselines with their compliance scores
            var baselines = await _dbContext.TenantBaselines
                .Where(b => b.TenantId == tenantId)
                .ToListAsync();

            return baselines.Select(b => (object)new
            {
                name = b.BaselineCode,
                frameworkCode = b.BaselineCode,
                complianceScore = 0,
                score = 0,
                totalControls = 0,
                compliantControls = 0
            }).ToList();
        }

        #endregion
    }
}
