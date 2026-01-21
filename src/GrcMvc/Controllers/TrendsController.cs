using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GrcMvc.Services.Interfaces;
using GrcMvc.Application.Permissions;
using GrcMvc.Application.Policy;
using GrcMvc.Authorization;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Controllers
{
    /// <summary>
    /// Trends Controller - Stage 6: Continuous Sustainability
    /// Manages trend analysis, forecasting, and visualization
    /// </summary>
    [Authorize]
    [RequireTenant]
    public class TrendsController : Controller
    {
        private readonly ISustainabilityService _sustainabilityService;
        private readonly ILogger<TrendsController> _logger;
        private readonly IWorkspaceContextService? _workspaceContext;

        public TrendsController(
            ISustainabilityService sustainabilityService,
            ILogger<TrendsController> logger,
            IWorkspaceContextService? workspaceContext = null)
        {
            _sustainabilityService = sustainabilityService;
            _logger = logger;
            _workspaceContext = workspaceContext;
        }

        // GET: Trends/Analysis
        [HttpGet]
        public async Task<IActionResult> Analysis(string? metricType = null, int months = 12)
        {
            try
            {
                var tenantId = (_workspaceContext?.GetCurrentTenantId() ?? Guid.Empty);
                if (tenantId == Guid.Empty)
                {
                    _logger.LogWarning("Tenant ID not found in context");
                    return RedirectToAction("Index", "Home");
                }

                var maturityHistory = await _sustainabilityService.GetMaturityHistoryAsync(tenantId, months);
                var kpis = await _sustainabilityService.GetKpisAsync(tenantId);

                ViewBag.MetricType = metricType;
                ViewBag.Months = months;
                ViewBag.MaturityHistory = maturityHistory;
                ViewBag.KPIs = kpis;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading trend analysis");
                TempData["ErrorMessage"] = "Failed to load trend analysis.";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Trends/Visualization
        [HttpGet]
        public async Task<IActionResult> Visualization(string? chartType = "line", int months = 12)
        {
            try
            {
                var tenantId = (_workspaceContext?.GetCurrentTenantId() ?? Guid.Empty);
                if (tenantId == Guid.Empty)
                {
                    _logger.LogWarning("Tenant ID not found in context");
                    return RedirectToAction("Index", "Home");
                }

                var maturityHistory = await _sustainabilityService.GetMaturityHistoryAsync(tenantId, months);
                var kpis = await _sustainabilityService.GetKpisAsync(tenantId);
                var forecast = await _sustainabilityService.GetComplianceForecastAsync(tenantId, 6);

                ViewBag.ChartType = chartType;
                ViewBag.Months = months;
                ViewBag.MaturityHistory = maturityHistory;
                ViewBag.KPIs = kpis;
                ViewBag.Forecast = forecast;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading trend visualization");
                TempData["ErrorMessage"] = "Failed to load trend visualization.";
                return RedirectToAction(nameof(Analysis));
            }
        }

        // GET: Trends/Forecasting
        [HttpGet]
        public async Task<IActionResult> Forecasting(int monthsAhead = 6)
        {
            try
            {
                var tenantId = (_workspaceContext?.GetCurrentTenantId() ?? Guid.Empty);
                if (tenantId == Guid.Empty)
                {
                    _logger.LogWarning("Tenant ID not found in context");
                    return RedirectToAction("Index", "Home");
                }

                var forecast = await _sustainabilityService.GetComplianceForecastAsync(tenantId, monthsAhead);
                var roadmap = await _sustainabilityService.GetMaturityRoadmapAsync(tenantId);
                var maturityHistory = await _sustainabilityService.GetMaturityHistoryAsync(tenantId, 12);

                ViewBag.MonthsAhead = monthsAhead;
                ViewBag.Forecast = forecast;
                ViewBag.Roadmap = roadmap;
                ViewBag.MaturityHistory = maturityHistory;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading forecasting");
                TempData["ErrorMessage"] = "Failed to load forecasting data.";
                return RedirectToAction(nameof(Analysis));
            }
        }
    }
}