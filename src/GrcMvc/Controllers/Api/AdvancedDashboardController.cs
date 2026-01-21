using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrcMvc.Services.Interfaces;
using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Localization;
using GrcMvc.Resources;
namespace GrcMvc.Controllers.Api;

/// <summary>
/// Advanced Monitoring Dashboard API
/// Provides Executive, Operations, Security, and Data Quality dashboard endpoints
/// </summary>
[ApiController]
[Route("api/v1/dashboards")]
[Authorize]
public class AdvancedDashboardController : ControllerBase
{
    private readonly IAdvancedDashboardService _dashboardService;
    private readonly ITenantContextService _tenantContext;
    private readonly ILogger<AdvancedDashboardController> _logger;

    public AdvancedDashboardController(
        IAdvancedDashboardService dashboardService,
        ITenantContextService tenantContext,
        ILogger<AdvancedDashboardController> logger)
    {
        _dashboardService = dashboardService;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    #region Executive Dashboard

    /// <summary>
    /// Get executive dashboard (C-level view)
    /// KPIs, trends, audit readiness, and executive alerts
    /// </summary>
    [HttpGet("executive")]
    [Authorize(Policy = "DashboardExecutiveView")]
    public async Task<IActionResult> GetExecutiveDashboard()
    {
        try
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            if (tenantId == Guid.Empty)
                return BadRequest(new { error = "Tenant context required" });

            var dashboard = await _dashboardService.GetExecutiveDashboardAsync(tenantId);
            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting executive dashboard");
            return StatusCode(500, new { error = "Failed to load executive dashboard" });
        }
    }

    /// <summary>
    /// Get audit readiness score with breakdown
    /// </summary>
    [HttpGet("executive/audit-readiness")]
    [Authorize(Policy = "DashboardExecutiveView")]
    public async Task<IActionResult> GetAuditReadiness()
    {
        try
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            if (tenantId == Guid.Empty)
                return BadRequest(new { error = "Tenant context required" });

            var readiness = await _dashboardService.GetAuditReadinessAsync(tenantId);
            return Ok(readiness);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit readiness");
            return StatusCode(500, new { error = "Failed to load audit readiness" });
        }
    }

    /// <summary>
    /// Get executive alerts
    /// </summary>
    [HttpGet("executive/alerts")]
    [Authorize(Policy = "DashboardExecutiveView")]
    public async Task<IActionResult> GetExecutiveAlerts([FromQuery] int limit = 10)
    {
        try
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            if (tenantId == Guid.Empty)
                return BadRequest(new { error = "Tenant context required" });

            var alerts = await _dashboardService.GetExecutiveAlertsAsync(tenantId, limit);
            return Ok(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting executive alerts");
            return StatusCode(500, new { error = "Failed to load alerts" });
        }
    }

    #endregion

    #region Operations Dashboard

    /// <summary>
    /// Get operations dashboard (GRC ops view)
    /// Work queues, SLA breaches, owner workload, evidence gaps
    /// </summary>
    [HttpGet("operations")]
    [Authorize(Policy = "DashboardOperationsView")]
    public async Task<IActionResult> GetOperationsDashboard()
    {
        try
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            if (tenantId == Guid.Empty)
                return BadRequest(new { error = "Tenant context required" });

            var dashboard = await _dashboardService.GetOperationsDashboardAsync(tenantId);
            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting operations dashboard");
            return StatusCode(500, new { error = "Failed to load operations dashboard" });
        }
    }

    /// <summary>
    /// Get work queue summary
    /// </summary>
    [HttpGet("operations/work-queue")]
    [Authorize(Policy = "DashboardOperationsView")]
    public async Task<IActionResult> GetWorkQueue()
    {
        try
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            if (tenantId == Guid.Empty)
                return BadRequest(new { error = "Tenant context required" });

            var queue = await _dashboardService.GetWorkQueueAsync(tenantId);
            return Ok(queue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting work queue");
            return StatusCode(500, new { error = "Failed to load work queue" });
        }
    }

    /// <summary>
    /// Get SLA breaches
    /// </summary>
    [HttpGet("operations/sla-breaches")]
    [Authorize(Policy = "DashboardOperationsView")]
    public async Task<IActionResult> GetSlaBreaches([FromQuery] int limit = 20)
    {
        try
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            if (tenantId == Guid.Empty)
                return BadRequest(new { error = "Tenant context required" });

            var breaches = await _dashboardService.GetSlaBreachesAsync(tenantId, limit);
            return Ok(breaches);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SLA breaches");
            return StatusCode(500, new { error = "Failed to load SLA breaches" });
        }
    }

    /// <summary>
    /// Get owner workload distribution
    /// </summary>
    [HttpGet("operations/owner-workload")]
    [Authorize(Policy = "DashboardOperationsView")]
    public async Task<IActionResult> GetOwnerWorkload()
    {
        try
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            if (tenantId == Guid.Empty)
                return BadRequest(new { error = "Tenant context required" });

            var workload = await _dashboardService.GetOwnerWorkloadAsync(tenantId);
            return Ok(workload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting owner workload");
            return StatusCode(500, new { error = "Failed to load owner workload" });
        }
    }

    /// <summary>
    /// Get evidence gaps
    /// </summary>
    [HttpGet("operations/evidence-gaps")]
    [Authorize(Policy = "DashboardOperationsView")]
    public async Task<IActionResult> GetEvidenceGaps([FromQuery] int limit = 20)
    {
        try
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            if (tenantId == Guid.Empty)
                return BadRequest(new { error = "Tenant context required" });

            var gaps = await _dashboardService.GetEvidenceGapsAsync(tenantId, limit);
            return Ok(gaps);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting evidence gaps");
            return StatusCode(500, new { error = "Failed to load evidence gaps" });
        }
    }

    #endregion

    #region Security Dashboard

    /// <summary>
    /// Get security dashboard (SOC/NOC view)
    /// Auth anomalies, integration health, API metrics, activity spikes
    /// </summary>
    [HttpGet("security")]
    [Authorize(Policy = "DashboardSecurityView")]
    public async Task<IActionResult> GetSecurityDashboard()
    {
        try
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            if (tenantId == Guid.Empty)
                return BadRequest(new { error = "Tenant context required" });

            var dashboard = await _dashboardService.GetSecurityDashboardAsync(tenantId);
            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting security dashboard");
            return StatusCode(500, new { error = "Failed to load security dashboard" });
        }
    }

    /// <summary>
    /// Get authentication anomaly KPIs
    /// </summary>
    [HttpGet("security/auth-anomalies")]
    [Authorize(Policy = "DashboardSecurityView")]
    public async Task<IActionResult> GetAuthAnomalies([FromQuery] int hours = 24)
    {
        try
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            if (tenantId == Guid.Empty)
                return BadRequest(new { error = "Tenant context required" });

            var anomalies = await _dashboardService.GetAuthAnomalyKpisAsync(tenantId, hours);
            return Ok(anomalies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting auth anomalies");
            return StatusCode(500, new { error = "Failed to load auth anomalies" });
        }
    }

    /// <summary>
    /// Get integration health status
    /// </summary>
    [HttpGet("security/integration-health")]
    [Authorize(Policy = "DashboardSecurityView")]
    public async Task<IActionResult> GetIntegrationHealth()
    {
        try
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            if (tenantId == Guid.Empty)
                return BadRequest(new { error = "Tenant context required" });

            var health = await _dashboardService.GetIntegrationHealthAsync(tenantId);
            return Ok(health);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting integration health");
            return StatusCode(500, new { error = "Failed to load integration health" });
        }
    }

    /// <summary>
    /// Get API metrics
    /// </summary>
    [HttpGet("security/api-metrics")]
    [Authorize(Policy = "DashboardSecurityView")]
    public async Task<IActionResult> GetApiMetrics([FromQuery] int hours = 24)
    {
        try
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            if (tenantId == Guid.Empty)
                return BadRequest(new { error = "Tenant context required" });

            var metrics = await _dashboardService.GetApiMetricsAsync(tenantId, hours);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting API metrics");
            return StatusCode(500, new { error = "Failed to load API metrics" });
        }
    }

    #endregion

    #region Data Quality Dashboard

    /// <summary>
    /// Get data quality dashboard
    /// Data freshness, missing mappings, orphan records
    /// </summary>
    [HttpGet("data-quality")]
    [Authorize(Policy = "DashboardDataQualityView")]
    public async Task<IActionResult> GetDataQualityDashboard()
    {
        try
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            if (tenantId == Guid.Empty)
                return BadRequest(new { error = "Tenant context required" });

            var dashboard = await _dashboardService.GetDataQualityDashboardAsync(tenantId);
            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting data quality dashboard");
            return StatusCode(500, new { error = "Failed to load data quality dashboard" });
        }
    }

    /// <summary>
    /// Get data freshness by entity type
    /// </summary>
    [HttpGet("data-quality/freshness")]
    [Authorize(Policy = "DashboardDataQualityView")]
    public async Task<IActionResult> GetDataFreshness()
    {
        try
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            if (tenantId == Guid.Empty)
                return BadRequest(new { error = "Tenant context required" });

            var freshness = await _dashboardService.GetDataFreshnessAsync(tenantId);
            return Ok(freshness);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting data freshness");
            return StatusCode(500, new { error = "Failed to load data freshness" });
        }
    }

    /// <summary>
    /// Get missing mappings
    /// </summary>
    [HttpGet("data-quality/missing-mappings")]
    [Authorize(Policy = "DashboardDataQualityView")]
    public async Task<IActionResult> GetMissingMappings([FromQuery] int limit = 20)
    {
        try
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            if (tenantId == Guid.Empty)
                return BadRequest(new { error = "Tenant context required" });

            var mappings = await _dashboardService.GetMissingMappingsAsync(tenantId, limit);
            return Ok(mappings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting missing mappings");
            return StatusCode(500, new { error = "Failed to load missing mappings" });
        }
    }

    /// <summary>
    /// Get orphan records report
    /// </summary>
    [HttpGet("data-quality/orphan-records")]
    [Authorize(Policy = "DashboardDataQualityView")]
    public async Task<IActionResult> GetOrphanRecords()
    {
        try
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            if (tenantId == Guid.Empty)
                return BadRequest(new { error = "Tenant context required" });

            var orphans = await _dashboardService.GetOrphanRecordsAsync(tenantId);
            return Ok(orphans);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orphan records");
            return StatusCode(500, new { error = "Failed to load orphan records" });
        }
    }

    #endregion

    #region Tenant Overview (Platform Admin)

    /// <summary>
    /// Get tenant overview for a specific tenant
    /// </summary>
    [HttpGet("tenant/{tenantId}")]
    [Authorize(Policy = "PlatformAdmin")]
    public async Task<IActionResult> GetTenantOverview(Guid tenantId)
    {
        try
        {
            var overview = await _dashboardService.GetTenantOverviewAsync(tenantId);
            return Ok(overview);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tenant overview for {TenantId}", tenantId);
            return StatusCode(500, new { error = "Failed to load tenant overview" });
        }
    }

    /// <summary>
    /// Get all tenants summary (platform admin only)
    /// </summary>
    [HttpGet("tenants/summary")]
    [Authorize(Policy = "PlatformAdmin")]
    public async Task<IActionResult> GetAllTenantsSummary()
    {
        try
        {
            var summaries = await _dashboardService.GetAllTenantsSummaryAsync();
            return Ok(summaries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all tenants summary");
            return StatusCode(500, new { error = "Failed to load tenants summary" });
        }
    }

    #endregion
}
