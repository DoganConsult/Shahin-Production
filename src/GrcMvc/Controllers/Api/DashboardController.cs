using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Microsoft.Extensions.Localization;
using GrcMvc.Resources;
namespace GrcMvc.Controllers.Api
{
    /// <summary>
    /// PHASE 9: Dashboard API Controller
    /// Provides compliance dashboards, reporting, and analytics
    /// </summary>
    [Route("api/dashboard")]
    [ApiController]
    [IgnoreAntiforgeryToken]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IDashboardService dashboardService,
            ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        #region Executive Dashboard

        /// <summary>
        /// Get executive summary dashboard
        /// GET /api/dashboard/{tenantId}/executive
        /// </summary>
        [HttpGet("{tenantId}/executive")]
        [Authorize]
        public async Task<IActionResult> GetExecutiveDashboard(Guid tenantId)
        {
            try
            {
                var dashboard = await _dashboardService.GetExecutiveDashboardAsync(tenantId);
                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting executive dashboard");
                return StatusCode(500, new { error = "An error occurred" });
            }
        }

        #endregion

        #region Compliance Dashboard

        /// <summary>
        /// Get compliance overview dashboard
        /// GET /api/dashboard/{tenantId}/compliance
        /// </summary>
        [HttpGet("{tenantId}/compliance")]
        [Authorize]
        public async Task<IActionResult> GetComplianceDashboard(Guid tenantId)
        {
            try
            {
                var dashboard = await _dashboardService.GetComplianceDashboardAsync(tenantId);
                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting compliance dashboard");
                return StatusCode(500, new { error = "An error occurred" });
            }
        }

        /// <summary>
        /// Get compliance by package
        /// GET /api/dashboard/{tenantId}/compliance/by-package
        /// </summary>
        [HttpGet("{tenantId}/compliance/by-package")]
        [Authorize]
        public async Task<IActionResult> GetComplianceByPackage(Guid tenantId)
        {
            try
            {
                var packages = await _dashboardService.GetComplianceByPackageAsync(tenantId);
                return Ok(new { total = packages.Count, packages });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting compliance by package");
                return StatusCode(500, new { error = "An error occurred" });
            }
        }

        /// <summary>
        /// Get compliance trend
        /// GET /api/dashboard/{tenantId}/compliance/trend?months=12
        /// </summary>
        [HttpGet("{tenantId}/compliance/trend")]
        [Authorize]
        public async Task<IActionResult> GetComplianceTrend(Guid tenantId, [FromQuery] int months = 12)
        {
            try
            {
                var trend = await _dashboardService.GetComplianceTrendAsync(tenantId, months);
                return Ok(new { months, dataPoints = trend });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting compliance trend");
                return StatusCode(500, new { error = "An error occurred" });
            }
        }

        #endregion

        #region Plan Progress

        /// <summary>
        /// Get plan progress dashboard
        /// GET /api/dashboard/{tenantId}/plans
        /// </summary>
        [HttpGet("{tenantId}/plans")]
        [Authorize]
        public async Task<IActionResult> GetPlanProgress(Guid tenantId, [FromQuery] Guid? planId = null)
        {
            try
            {
                var dashboard = await _dashboardService.GetPlanProgressAsync(tenantId, planId);
                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting plan progress");
                return StatusCode(500, new { error = "An error occurred" });
            }
        }

        /// <summary>
        /// Get assessment progress for a plan
        /// GET /api/dashboard/plans/{planId}/assessments
        /// </summary>
        [HttpGet("plans/{planId}/assessments")]
        [Authorize]
        public async Task<IActionResult> GetAssessmentProgress(Guid planId)
        {
            try
            {
                var assessments = await _dashboardService.GetAssessmentProgressAsync(planId);
                return Ok(new { total = assessments.Count, assessments });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting assessment progress");
                return StatusCode(500, new { error = "An error occurred" });
            }
        }

        #endregion

        #region Task Dashboard

        /// <summary>
        /// Get task overview dashboard
        /// GET /api/dashboard/{tenantId}/tasks
        /// </summary>
        [HttpGet("{tenantId}/tasks")]
        [Authorize]
        public async Task<IActionResult> GetTaskDashboard(Guid tenantId)
        {
            try
            {
                var dashboard = await _dashboardService.GetTaskDashboardAsync(tenantId);
                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting task dashboard");
                return StatusCode(500, new { error = "An error occurred" });
            }
        }

        /// <summary>
        /// Get overdue tasks
        /// GET /api/dashboard/{tenantId}/tasks/overdue?limit=20
        /// </summary>
        [HttpGet("{tenantId}/tasks/overdue")]
        [Authorize]
        public async Task<IActionResult> GetOverdueTasks(Guid tenantId, [FromQuery] int limit = 20)
        {
            try
            {
                var tasks = await _dashboardService.GetOverdueTasksAsync(tenantId, limit);
                return Ok(new { total = tasks.Count, tasks });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting overdue tasks");
                return StatusCode(500, new { error = "An error occurred" });
            }
        }

        /// <summary>
        /// Get upcoming tasks
        /// GET /api/dashboard/{tenantId}/tasks/upcoming?days=7&limit=20
        /// </summary>
        [HttpGet("{tenantId}/tasks/upcoming")]
        [Authorize]
        public async Task<IActionResult> GetUpcomingTasks(
            Guid tenantId,
            [FromQuery] int days = 7,
            [FromQuery] int limit = 20)
        {
            try
            {
                var tasks = await _dashboardService.GetUpcomingTasksAsync(tenantId, days, limit);
                return Ok(new { total = tasks.Count, days, tasks });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting upcoming tasks");
                return StatusCode(500, new { error = "An error occurred" });
            }
        }

        #endregion

        #region Risk Dashboard

        /// <summary>
        /// Get risk overview dashboard
        /// GET /api/dashboard/{tenantId}/risks
        /// </summary>
        [HttpGet("{tenantId}/risks")]
        [Authorize]
        public async Task<IActionResult> GetRiskDashboard(Guid tenantId)
        {
            try
            {
                var dashboard = await _dashboardService.GetRiskDashboardAsync(tenantId);
                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting risk dashboard");
                return StatusCode(500, new { error = "An error occurred" });
            }
        }

        #endregion

        #region Top Actions

        /// <summary>
        /// Get top next actions
        /// GET /api/dashboard/{tenantId}/next-actions?limit=10
        /// </summary>
        [HttpGet("{tenantId}/next-actions")]
        [Authorize]
        public async Task<IActionResult> GetTopNextActions(Guid tenantId, [FromQuery] int limit = 10)
        {
            try
            {
                var actions = await _dashboardService.GetTopNextActionsAsync(tenantId, limit);
                return Ok(new { total = actions.Count, actions });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting next actions");
                return StatusCode(500, new { error = "An error occurred" });
            }
        }

        #endregion

        #region Drill-Down

        /// <summary>
        /// Drill down: Package → Assessments
        /// GET /api/dashboard/{tenantId}/drill-down/package/{packageCode}
        /// </summary>
        [HttpGet("{tenantId}/drill-down/package/{packageCode}")]
        [Authorize]
        public async Task<IActionResult> DrillDownPackage(Guid tenantId, string packageCode)
        {
            try
            {
                var data = await _dashboardService.DrillDownPackageAsync(tenantId, packageCode);
                return Ok(data);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error drilling down package");
                return StatusCode(500, new { error = "An error occurred" });
            }
        }

        /// <summary>
        /// Drill down: Assessment → Requirements
        /// GET /api/dashboard/drill-down/assessment/{assessmentId}
        /// </summary>
        [HttpGet("drill-down/assessment/{assessmentId}")]
        [Authorize]
        public async Task<IActionResult> DrillDownAssessment(Guid assessmentId)
        {
            try
            {
                var data = await _dashboardService.DrillDownAssessmentAsync(assessmentId);
                return Ok(data);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error drilling down assessment");
                return StatusCode(500, new { error = "An error occurred" });
            }
        }

        /// <summary>
        /// Drill down: Requirement → Evidence
        /// GET /api/dashboard/drill-down/requirement/{requirementId}
        /// </summary>
        [HttpGet("drill-down/requirement/{requirementId}")]
        [Authorize]
        public async Task<IActionResult> DrillDownRequirement(Guid requirementId)
        {
            try
            {
                var data = await _dashboardService.DrillDownRequirementAsync(requirementId);
                return Ok(data);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error drilling down requirement");
                return StatusCode(500, new { error = "An error occurred" });
            }
        }

        #endregion

        #region Audit Pack

        /// <summary>
        /// Generate audit pack export
        /// GET /api/dashboard/{tenantId}/audit-pack?assessmentId=
        /// </summary>
        [HttpGet("{tenantId}/audit-pack")]
        [Authorize]
        public async Task<IActionResult> GenerateAuditPack(
            Guid tenantId,
            [FromQuery] Guid? assessmentId = null)
        {
            try
            {
                var auditPack = await _dashboardService.GenerateAuditPackAsync(tenantId, assessmentId);
                return Ok(auditPack);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating audit pack");
                return StatusCode(500, new { error = "An error occurred" });
            }
        }

        /// <summary>
        /// Export audit pack as JSON file
        /// GET /api/dashboard/{tenantId}/audit-pack/export
        /// </summary>
        [HttpGet("{tenantId}/audit-pack/export")]
        [Authorize]
        public async Task<IActionResult> ExportAuditPack(
            Guid tenantId,
            [FromQuery] Guid? assessmentId = null)
        {
            try
            {
                var auditPack = await _dashboardService.GenerateAuditPackAsync(tenantId, assessmentId);
                var json = System.Text.Json.JsonSerializer.Serialize(auditPack, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });

                var fileName = $"audit-pack-{tenantId:N}-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json";
                var bytes = System.Text.Encoding.UTF8.GetBytes(json);

                return File(bytes, "application/json", fileName);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting audit pack");
                return StatusCode(500, new { error = "An error occurred" });
            }
        }

        #endregion
    }
}
