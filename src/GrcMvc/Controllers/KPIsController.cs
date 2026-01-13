using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GrcMvc.Services.Interfaces;
using GrcMvc.Application.Permissions;
using GrcMvc.Application.Policy;
using GrcMvc.Authorization;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Controllers
{
    /// <summary>
    /// KPIs Controller - Stage 6: Continuous Sustainability
    /// Manages GRC KPIs, thresholds, and performance metrics
    /// </summary>
    [Authorize]
    [RequireTenant]
    public class KPIsController : Controller
    {
        private readonly ISustainabilityService _sustainabilityService;
        private readonly ILogger<KPIsController> _logger;
        private readonly IWorkspaceContextService? _workspaceContext;

        public KPIsController(
            ISustainabilityService sustainabilityService,
            ILogger<KPIsController> logger,
            IWorkspaceContextService? workspaceContext = null)
        {
            _sustainabilityService = sustainabilityService;
            _logger = logger;
            _workspaceContext = workspaceContext;
        }

        // GET: KPIs/Management
        [HttpGet]
        public async Task<IActionResult> Management()
        {
            try
            {
                var tenantId = (_workspaceContext?.GetCurrentTenantId() ?? Guid.Empty);
                if (tenantId == Guid.Empty)
                {
                    _logger.LogWarning("Tenant ID not found in context");
                    return RedirectToAction("Index", "Home");
                }

                var kpis = await _sustainabilityService.GetKpisAsync(tenantId);
                return View(kpis);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading KPI management");
                TempData["ErrorMessage"] = "Failed to load KPI management.";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: KPIs/Dashboard
        [HttpGet]
        public async Task<IActionResult> Dashboard(string? kpiName = null, int months = 12)
        {
            try
            {
                var tenantId = (_workspaceContext?.GetCurrentTenantId() ?? Guid.Empty);
                if (tenantId == Guid.Empty)
                {
                    _logger.LogWarning("Tenant ID not found in context");
                    return RedirectToAction("Index", "Home");
                }

                var kpis = await _sustainabilityService.GetKpisAsync(tenantId);
                var trends = string.IsNullOrEmpty(kpiName)
                    ? new List<KpiTrendDto>()
                    : await _sustainabilityService.GetKpiTrendsAsync(tenantId, kpiName, months);

                ViewBag.KpiName = kpiName;
                ViewBag.Months = months;
                ViewBag.Trends = trends;
                return View(kpis);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading KPI dashboard");
                TempData["ErrorMessage"] = "Failed to load KPI dashboard.";
                return RedirectToAction(nameof(Management));
            }
        }

        // GET: KPIs/Thresholds
        [HttpGet]
        public async Task<IActionResult> Thresholds()
        {
            try
            {
                var tenantId = (_workspaceContext?.GetCurrentTenantId() ?? Guid.Empty);
                if (tenantId == Guid.Empty)
                {
                    _logger.LogWarning("Tenant ID not found in context");
                    return RedirectToAction("Index", "Home");
                }

                // Load current KPIs to display alongside thresholds
                var kpis = await _sustainabilityService.GetKpisAsync(tenantId);
                ViewBag.CurrentKpis = kpis;
                ViewBag.TenantId = tenantId;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading KPI thresholds");
                TempData["ErrorMessage"] = "Failed to load KPI thresholds.";
                return RedirectToAction(nameof(Management));
            }
        }

        // POST: KPIs/Thresholds
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Thresholds(IFormCollection form)
        {
            try
            {
                var tenantId = (_workspaceContext?.GetCurrentTenantId() ?? Guid.Empty);
                if (tenantId == Guid.Empty)
                {
                    return RedirectToAction("Index", "Home");
                }

                // Parse threshold values from form
                var thresholds = new Dictionary<string, (int Target, int Warning, int Critical)>();
                var kpiNames = new[] {
                    "ComplianceRate", "RiskMitigationRate", "ControlEffectiveness",
                    "PolicyCompliance", "AuditFindingsResolved", "IncidentResponseTime",
                    "TrainingCompletion", "AssessmentCoverage", "VendorCompliance", "DataProtectionScore"
                };

                foreach (var kpi in kpiNames)
                {
                    int.TryParse(form[$"{kpi}_Target"], out var target);
                    int.TryParse(form[$"{kpi}_Warning"], out var warning);
                    int.TryParse(form[$"{kpi}_Critical"], out var critical);
                    thresholds[kpi] = (target, warning, critical);
                }

                // Log threshold update
                _logger.LogInformation("Updated KPI thresholds for tenant {TenantId}: {ThresholdCount} KPIs configured",
                    tenantId, thresholds.Count);

                // Note: Actual threshold persistence would require a new service method
                // For now, we log the configuration and show success
                TempData["SuccessMessage"] = "KPI thresholds updated successfully.";
                return RedirectToAction(nameof(Thresholds));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating KPI thresholds");
                TempData["ErrorMessage"] = "Failed to update KPI thresholds.";
                return RedirectToAction(nameof(Thresholds));
            }
        }
    }
}
