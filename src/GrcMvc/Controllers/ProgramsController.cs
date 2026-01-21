using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrcMvc.Services.Interfaces;
using GrcMvc.Application.Permissions;
using GrcMvc.Authorization;

namespace GrcMvc.Controllers
{
    /// <summary>
    /// Programs Controller - Stage 5: GRC Program Management
    /// Manages GRC programs, initiatives, budgets, and execution tracking
    /// </summary>
    [Authorize]
    [RequireTenant]
    public class ProgramsController : Controller
    {
        private readonly IGrcProcessOrchestrator _orchestrator;
        private readonly ISustainabilityService _sustainabilityService;
        private readonly IWorkspaceContextService? _workspaceContext;
        private readonly ILogger<ProgramsController> _logger;

        public ProgramsController(
            IGrcProcessOrchestrator orchestrator,
            ISustainabilityService sustainabilityService,
            IWorkspaceContextService? workspaceContext,
            ILogger<ProgramsController> logger)
        {
            _orchestrator = orchestrator;
            _sustainabilityService = sustainabilityService;
            _workspaceContext = workspaceContext;
            _logger = logger;
        }

        // GET: Programs/Definition
        [Authorize(GrcPermissions.Sustainability.View)]
        public async Task<IActionResult> Definition()
        {
            var tenantId = (_workspaceContext?.GetCurrentTenantId() ?? Guid.Empty);
            if (tenantId == Guid.Empty)
            {
                _logger.LogWarning("Tenant ID not found in context");
                return RedirectToAction("Index", "Home");
            }

            try
            {
                // Get program definitions
                ViewBag.TenantId = tenantId;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading program definitions for tenant {TenantId}", tenantId);
                TempData["Error"] = "Failed to load program definitions";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Programs/Initiatives
        [Authorize(GrcPermissions.Sustainability.View)]
        public async Task<IActionResult> Initiatives()
        {
            var tenantId = (_workspaceContext?.GetCurrentTenantId() ?? Guid.Empty);
            if (tenantId == Guid.Empty)
            {
                _logger.LogWarning("Tenant ID not found in context");
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var initiatives = await _sustainabilityService.GetInitiativesAsync(tenantId);
                return View(initiatives);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading program initiatives for tenant {TenantId}", tenantId);
                TempData["Error"] = "Failed to load program initiatives";
                return RedirectToAction("Definition");
            }
        }

        // GET: Programs/Budget
        [Authorize(GrcPermissions.Sustainability.View)]
        public async Task<IActionResult> Budget()
        {
            var tenantId = (_workspaceContext?.GetCurrentTenantId() ?? Guid.Empty);
            if (tenantId == Guid.Empty)
            {
                _logger.LogWarning("Tenant ID not found in context");
                return RedirectToAction("Index", "Home");
            }

            try
            {
                // Get budget data
                ViewBag.TenantId = tenantId;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading program budget for tenant {TenantId}", tenantId);
                TempData["Error"] = "Failed to load program budget";
                return RedirectToAction("Definition");
            }
        }

        // GET: Programs/Execution
        [Authorize(GrcPermissions.Sustainability.View)]
        public async Task<IActionResult> Execution()
        {
            var tenantId = (_workspaceContext?.GetCurrentTenantId() ?? Guid.Empty);
            if (tenantId == Guid.Empty)
            {
                _logger.LogWarning("Tenant ID not found in context");
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var executionData = await _sustainabilityService.GetProgramExecutionAsync(tenantId);
                return View(executionData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading program execution for tenant {TenantId}", tenantId);
                TempData["Error"] = "Failed to load program execution";
                return RedirectToAction("Definition");
            }
        }
    }
}
