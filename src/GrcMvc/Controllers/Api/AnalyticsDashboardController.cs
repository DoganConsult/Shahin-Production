using GrcMvc.Services.Analytics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GrcMvc.Controllers.Api
{
    /// <summary>
    /// API Controller for analytics dashboard data from ClickHouse OLAP
    /// Provides fast, pre-aggregated metrics for dashboard widgets
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AnalyticsDashboardController : ControllerBase
    {
        private readonly IClickHouseService _clickHouse;
        private readonly IDashboardProjector _projector;
        private readonly ILogger<AnalyticsDashboardController> _logger;

        public AnalyticsDashboardController(
            IClickHouseService clickHouse,
            IDashboardProjector projector,
            ILogger<AnalyticsDashboardController> logger)
        {
            _clickHouse = clickHouse;
            _projector = projector;
            _logger = logger;
        }

        private Guid GetTenantId()
        {
            var tenantClaim = User.FindFirst("TenantId")?.Value;
            return Guid.TryParse(tenantClaim, out var tenantId) ? tenantId : Guid.Empty;
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
        /// </summary>
        [HttpGet("executive-summary")]
        public async Task<IActionResult> GetExecutiveSummary()
        {
            var tenantId = GetTenantId();
            if (tenantId == Guid.Empty)
                return BadRequest("Invalid tenant");

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
                GeneratedAt = DateTime.UtcNow
            });
        }
    }
}
