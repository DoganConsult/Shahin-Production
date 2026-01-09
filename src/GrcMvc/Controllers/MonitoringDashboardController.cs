using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrcMvc.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace GrcMvc.Controllers;

/// <summary>
/// Advanced Monitoring Dashboard Controller
/// Routes: /dashboard/executive, /dashboard/operations, /dashboard/security, /dashboard/data-quality
/// </summary>
[Authorize]
[Route("dashboard")]
public class MonitoringDashboardController : Controller
{
    private readonly IAdvancedDashboardService _dashboardService;
    private readonly ITenantContextService _tenantContext;
    private readonly ILogger<MonitoringDashboardController> _logger;

    public MonitoringDashboardController(
        IAdvancedDashboardService dashboardService,
        ITenantContextService tenantContext,
        ILogger<MonitoringDashboardController> logger)
    {
        _dashboardService = dashboardService;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    /// <summary>
    /// Executive Dashboard - C-Level View
    /// "Are we compliant? Where are the risks? Are we audit-ready?"
    /// </summary>
    [HttpGet("executive")]
    [Authorize(Policy = "DashboardExecutiveView")]
    public async Task<IActionResult> Executive()
    {
        try
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            if (tenantId == Guid.Empty)
                return RedirectToAction("Index", "Home");

            var dashboard = await _dashboardService.GetExecutiveDashboardAsync(tenantId);
            return View("Executive", dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading executive dashboard");
            ViewBag.Error = "Unable to load dashboard data";
            return View("Executive");
        }
    }

    /// <summary>
    /// Operations Dashboard - GRC Ops View
    /// Workflows, SLA breaches, owner workload, evidence gaps
    /// </summary>
    [HttpGet("operations")]
    [Authorize(Policy = "DashboardOperationsView")]
    public async Task<IActionResult> Operations()
    {
        try
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            if (tenantId == Guid.Empty)
                return RedirectToAction("Index", "Home");

            var dashboard = await _dashboardService.GetOperationsDashboardAsync(tenantId);
            return View("Operations", dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading operations dashboard");
            ViewBag.Error = "Unable to load dashboard data";
            return View("Operations");
        }
    }

    /// <summary>
    /// Security Dashboard - SOC/NOC View
    /// Auth anomalies, integration health, API metrics
    /// </summary>
    [HttpGet("security")]
    [Authorize(Policy = "DashboardSecurityView")]
    public async Task<IActionResult> Security()
    {
        try
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            if (tenantId == Guid.Empty)
                return RedirectToAction("Index", "Home");

            var dashboard = await _dashboardService.GetSecurityDashboardAsync(tenantId);
            return View("Security", dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading security dashboard");
            ViewBag.Error = "Unable to load dashboard data";
            return View("Security");
        }
    }

    /// <summary>
    /// Data Quality Dashboard
    /// Data freshness, missing mappings, orphan records
    /// </summary>
    [HttpGet("data-quality")]
    [Authorize(Policy = "DashboardDataQualityView")]
    public async Task<IActionResult> DataQuality()
    {
        try
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            if (tenantId == Guid.Empty)
                return RedirectToAction("Index", "Home");

            var dashboard = await _dashboardService.GetDataQualityDashboardAsync(tenantId);
            return View("DataQuality", dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading data quality dashboard");
            ViewBag.Error = "Unable to load dashboard data";
            return View("DataQuality");
        }
    }

    /// <summary>
    /// Tenant Overview - Platform Admin View
    /// </summary>
    [HttpGet("tenant/{tenantId}")]
    [Authorize(Policy = "PlatformAdmin")]
    public async Task<IActionResult> TenantOverview(Guid tenantId)
    {
        try
        {
            var overview = await _dashboardService.GetTenantOverviewAsync(tenantId);
            return View("TenantOverview", overview);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading tenant overview for {TenantId}", tenantId);
            ViewBag.Error = "Unable to load tenant data";
            return View("TenantOverview");
        }
    }

    /// <summary>
    /// All Tenants Summary - Platform Admin View
    /// </summary>
    [HttpGet("tenants")]
    [Authorize(Policy = "PlatformAdmin")]
    public async Task<IActionResult> TenantsSummary()
    {
        try
        {
            var summaries = await _dashboardService.GetAllTenantsSummaryAsync();
            return View("TenantsSummary", summaries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading tenants summary");
            ViewBag.Error = "Unable to load tenants data";
            return View("TenantsSummary");
        }
    }
}
