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
    /// Sustainability Controller - Stage 6: Continuous Sustainability
    /// Manages long-term compliance sustainability, continuous improvement, and KPIs
    /// </summary>
    [Authorize]
    [RequireTenant]
    public class SustainabilityController : Controller
    {
        private readonly ISustainabilityService _sustainabilityService;
        private readonly ILogger<SustainabilityController> _logger;
        private readonly IWorkspaceContextService? _workspaceContext;

        public SustainabilityController(
            ISustainabilityService sustainabilityService,
            ILogger<SustainabilityController> logger,
            IWorkspaceContextService? workspaceContext = null)
        {
            _sustainabilityService = sustainabilityService;
            _logger = logger;
            _workspaceContext = workspaceContext;
        }

        // GET: Sustainability/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var tenantId = (_workspaceContext?.GetCurrentTenantId() ?? Guid.Empty);
                if (tenantId == Guid.Empty)
                {
                    _logger.LogWarning("Tenant ID not found in context");
                    return RedirectToAction("Index", "Home");
                }

                var initiatives = await _sustainabilityService.GetImprovementInitiativesAsync(tenantId);
                return View(initiatives);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading sustainability index");
                TempData["ErrorMessage"] = "Failed to load sustainability data.";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Sustainability/Dashboard
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

                var dashboard = await _sustainabilityService.GetDashboardAsync(tenantId);
                return View(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading sustainability dashboard");
                TempData["ErrorMessage"] = "Failed to load sustainability dashboard.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Sustainability/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateImprovementDto());
        }

        // POST: Sustainability/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateImprovementDto model)
        {
            try
            {
                var tenantId = (_workspaceContext?.GetCurrentTenantId() ?? Guid.Empty);
                if (tenantId == Guid.Empty)
                {
                    return RedirectToAction("Index", "Home");
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var result = await _sustainabilityService.CreateImprovementInitiativeAsync(tenantId, model);
                _logger.LogInformation("Created sustainability initiative {InitiativeId} for tenant {TenantId}",
                    result.Id, tenantId);

                TempData["SuccessMessage"] = "Sustainability initiative created successfully.";
                return RedirectToAction(nameof(Details), new { id = result.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating sustainability initiative");
                TempData["ErrorMessage"] = "Failed to create sustainability initiative.";
                return View(model);
            }
        }

        // GET: Sustainability/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var tenantId = (_workspaceContext?.GetCurrentTenantId() ?? Guid.Empty);
                if (tenantId == Guid.Empty)
                {
                    return RedirectToAction("Index", "Home");
                }

                var initiatives = await _sustainabilityService.GetImprovementInitiativesAsync(tenantId);
                var initiative = initiatives.FirstOrDefault(i => i.Id == id);

                if (initiative == null)
                {
                    TempData["ErrorMessage"] = "Initiative not found.";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.InitiativeId = id;
                return View(initiative);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading sustainability initiative {InitiativeId} for edit", id);
                TempData["ErrorMessage"] = "Failed to load sustainability initiative.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Sustainability/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, SustainabilityImprovementDto model)
        {
            try
            {
                var tenantId = (_workspaceContext?.GetCurrentTenantId() ?? Guid.Empty);
                if (tenantId == Guid.Empty)
                {
                    return RedirectToAction("Index", "Home");
                }

                // Update progress
                var result = await _sustainabilityService.UpdateImprovementProgressAsync(
                    tenantId, id, model.PercentComplete, model.Outcomes);

                // If completed, mark as complete
                if (model.Status == "Completed" && model.PercentComplete >= 100)
                {
                    await _sustainabilityService.CompleteImprovementAsync(
                        tenantId, id, model.Owner, model.Outcomes);
                }

                _logger.LogInformation("Updated sustainability initiative {InitiativeId} for tenant {TenantId}",
                    id, tenantId);

                TempData["SuccessMessage"] = "Sustainability initiative updated successfully.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating sustainability initiative {InitiativeId}", id);
                TempData["ErrorMessage"] = "Failed to update sustainability initiative.";
                ViewBag.InitiativeId = id;
                return View(model);
            }
        }

        // GET: Sustainability/Details/{id}
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var tenantId = (_workspaceContext?.GetCurrentTenantId() ?? Guid.Empty);
                if (tenantId == Guid.Empty)
                {
                    return RedirectToAction("Index", "Home");
                }

                var initiatives = await _sustainabilityService.GetImprovementInitiativesAsync(tenantId);
                var initiative = initiatives.FirstOrDefault(i => i.Id == id);

                if (initiative == null)
                {
                    TempData["ErrorMessage"] = "Initiative not found.";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.InitiativeId = id;
                return View(initiative);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading sustainability initiative {InitiativeId} details", id);
                TempData["ErrorMessage"] = "Failed to load sustainability initiative details.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
