using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GrcMvc.Services.Interfaces;
using GrcMvc.Application.Permissions;
using GrcMvc.Application.Policy;
using GrcMvc.Authorization;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Controllers
{
    /// <summary>
    /// Benchmarking Controller - Stage 5: Excellence & Benchmarking
    /// Manages industry benchmarking, peer comparison, and sector analysis
    /// </summary>
    [Authorize]
    [RequireTenant]
    public class BenchmarkingController : Controller
    {
        private readonly IGrcProcessOrchestrator _orchestrator;
        private readonly ISustainabilityService _sustainabilityService;
        private readonly ILogger<BenchmarkingController> _logger;
        private readonly IWorkspaceContextService? _workspaceContext;

        public BenchmarkingController(
            IGrcProcessOrchestrator orchestrator,
            ISustainabilityService sustainabilityService,
            ILogger<BenchmarkingController> logger,
            IWorkspaceContextService? workspaceContext = null)
        {
            _orchestrator = orchestrator;
            _sustainabilityService = sustainabilityService;
            _logger = logger;
            _workspaceContext = workspaceContext;
        }

        // GET: Benchmarking/Dashboard
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var tenantId = (_workspaceContext?.GetCurrentTenantId() ?? Guid.Empty);
                if (tenantId == Guid.Empty)
                {
                    _logger.LogWarning("Tenant ID not found in context");
                    return RedirectToAction("Index", "Home");
                }

                var benchmarks = await _sustainabilityService.GetBenchmarksAsync(tenantId);
                var ksaBenchmarks = await _sustainabilityService.GetKsaBenchmarksAsync(tenantId);
                
                ViewBag.Benchmarks = benchmarks;
                ViewBag.KsaBenchmarks = ksaBenchmarks;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading benchmarking dashboard");
                TempData["ErrorMessage"] = "Failed to load benchmarking dashboard.";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Benchmarking/Industry
        [HttpGet]
        public async Task<IActionResult> Industry(string? sectorCode = null)
        {
            try
            {
                var tenantId = (_workspaceContext?.GetCurrentTenantId() ?? Guid.Empty);
                if (tenantId == Guid.Empty)
                {
                    _logger.LogWarning("Tenant ID not found in context");
                    return RedirectToAction("Index", "Home");
                }

                if (string.IsNullOrEmpty(sectorCode))
                {
                    var generalBenchmarks = await _sustainabilityService.GetBenchmarksAsync(tenantId);
                    ViewBag.SectorCode = sectorCode;
                    return View(generalBenchmarks);
                }
                else
                {
                    var sectorBenchmarks = await _orchestrator.BenchmarkAgainstSectorAsync(tenantId, sectorCode);
                    ViewBag.SectorCode = sectorCode;
                    return View("SectorBenchmark", sectorBenchmarks);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading industry benchmarks");
                TempData["ErrorMessage"] = "Failed to load industry benchmarks.";
                return RedirectToAction(nameof(Dashboard));
            }
        }

        // GET: Benchmarking/Peers
        [HttpGet]
        public async Task<IActionResult> Peers()
        {
            try
            {
                var tenantId = (_workspaceContext?.GetCurrentTenantId() ?? Guid.Empty);
                if (tenantId == Guid.Empty)
                {
                    _logger.LogWarning("Tenant ID not found in context");
                    return RedirectToAction("Index", "Home");
                }

                // TODO: Load peer comparison data
                ViewBag.TenantId = tenantId;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading peer benchmarks");
                TempData["ErrorMessage"] = "Failed to load peer benchmarks.";
                return RedirectToAction(nameof(Dashboard));
            }
        }

        // GET: Benchmarking/Report
        [HttpGet]
        public async Task<IActionResult> Report()
        {
            try
            {
                var tenantId = (_workspaceContext?.GetCurrentTenantId() ?? Guid.Empty);
                if (tenantId == Guid.Empty)
                {
                    _logger.LogWarning("Tenant ID not found in context");
                    return RedirectToAction("Index", "Home");
                }

                var benchmarks = await _sustainabilityService.GetBenchmarksAsync(tenantId);
                var ksaBenchmarks = await _sustainabilityService.GetKsaBenchmarksAsync(tenantId);
                var maturityScore = await _sustainabilityService.GetMaturityScoreAsync(tenantId);

                ViewBag.Benchmarks = benchmarks;
                ViewBag.KsaBenchmarks = ksaBenchmarks;
                ViewBag.MaturityScore = maturityScore;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating benchmarking report");
                TempData["ErrorMessage"] = "Failed to generate benchmarking report.";
                return RedirectToAction(nameof(Dashboard));
            }
        }
    }
}