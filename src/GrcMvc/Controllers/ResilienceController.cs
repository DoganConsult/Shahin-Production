using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GrcMvc.Services.Interfaces;
using GrcMvc.Models.DTOs;
using GrcMvc.Application.Permissions;
using GrcMvc.Application.Policy;
using GrcMvc.Authorization;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Controllers
{
    [Authorize]
    [RequireTenant]
    public class ResilienceController : Controller
    {
        private readonly IResilienceService _resilienceService;
        private readonly ILogger<ResilienceController> _logger;
        private readonly IWorkspaceContextService? _workspaceContext;
        private readonly PolicyEnforcementHelper _policyHelper;

        public ResilienceController(
            IResilienceService resilienceService,
            ILogger<ResilienceController> logger,
            PolicyEnforcementHelper policyHelper,
            IWorkspaceContextService? workspaceContext = null)
        {
            _resilienceService = resilienceService;
            _logger = logger;
            _policyHelper = policyHelper;
            _workspaceContext = workspaceContext;
        }

        // GET: Resilience/Dashboard
        [Authorize(GrcPermissions.Resilience.View)]
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

                var dashboardData = await _resilienceService.GetResilienceDashboardAsync(tenantId);
                return View(dashboardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading resilience dashboard");
                TempData["ErrorMessage"] = "Failed to load resilience dashboard.";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Resilience/BIA (Business Impact Analysis)
        [Authorize(GrcPermissions.Resilience.View)]
        public async Task<IActionResult> BIA(int page = 1, int pageSize = 20)
        {
            try
            {
                var tenantId = (_workspaceContext?.GetCurrentTenantId() ?? Guid.Empty);
                if (tenantId == Guid.Empty)
                {
                    _logger.LogWarning("Tenant ID not found in context");
                    return RedirectToAction("Index", "Home");
                }

                var resiliences = await _resilienceService.GetResiliencesAsync(tenantId, page, pageSize);
                return View(resiliences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading BIA page");
                TempData["ErrorMessage"] = "Failed to load Business Impact Analysis.";
                return RedirectToAction("Dashboard");
            }
        }

        // GET: Resilience/RTO_RPO
        [Authorize(GrcPermissions.Resilience.View)]
        public async Task<IActionResult> RTO_RPO()
        {
            try
            {
                var tenantId = (_workspaceContext?.GetCurrentTenantId() ?? Guid.Empty);
                if (tenantId == Guid.Empty)
                {
                    _logger.LogWarning("Tenant ID not found in context");
                    return RedirectToAction("Index", "Home");
                }

                var drReadiness = await _resilienceService.GetDrReadinessAsync(tenantId);
                return View(drReadiness);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading RTO/RPO page");
                TempData["ErrorMessage"] = "Failed to load RTO/RPO assessment.";
                return RedirectToAction("Dashboard");
            }
        }

        // GET: Resilience/Drills
        [Authorize(GrcPermissions.Resilience.ManageDrills)]
        public async Task<IActionResult> Drills()
        {
            try
            {
                var tenantId = (_workspaceContext?.GetCurrentTenantId() ?? Guid.Empty);
                if (tenantId == Guid.Empty)
                {
                    _logger.LogWarning("Tenant ID not found in context");
                    return RedirectToAction("Index", "Home");
                }

                var incidents = await _resilienceService.GetIncidentsAsync(tenantId);
                return View(incidents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading drills page");
                TempData["ErrorMessage"] = "Failed to load resilience drills.";
                return RedirectToAction("Dashboard");
            }
        }

        // GET: Resilience/Plans
        [Authorize(GrcPermissions.Resilience.ManagePlans)]
        public async Task<IActionResult> Plans()
        {
            try
            {
                var tenantId = (_workspaceContext?.GetCurrentTenantId() ?? Guid.Empty);
                if (tenantId == Guid.Empty)
                {
                    _logger.LogWarning("Tenant ID not found in context");
                    return RedirectToAction("Index", "Home");
                }

                var bcmScore = await _resilienceService.GetBcmScoreAsync(tenantId);
                return View(bcmScore);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading plans page");
                TempData["ErrorMessage"] = "Failed to load BCP/DRP plans.";
                return RedirectToAction("Dashboard");
            }
        }

        // GET: Resilience/Monitoring
        [Authorize(GrcPermissions.Resilience.Monitor)]
        public async Task<IActionResult> Monitoring()
        {
            try
            {
                var tenantId = (_workspaceContext?.GetCurrentTenantId() ?? Guid.Empty);
                if (tenantId == Guid.Empty)
                {
                    _logger.LogWarning("Tenant ID not found in context");
                    return RedirectToAction("Index", "Home");
                }

                var metrics = await _resilienceService.GetIncidentMetricsAsync(tenantId);
                return View(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading monitoring page");
                TempData["ErrorMessage"] = "Failed to load resilience monitoring.";
                return RedirectToAction("Dashboard");
            }
        }

        // GET: Resilience/Index
        [Authorize(GrcPermissions.Resilience.View)]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 20)
        {
            try
            {
                var tenantId = (_workspaceContext?.GetCurrentTenantId() ?? Guid.Empty);
                if (tenantId == Guid.Empty)
                {
                    _logger.LogWarning("Tenant ID not found in context");
                    return RedirectToAction("Index", "Home");
                }

                var resiliences = await _resilienceService.GetResiliencesAsync(tenantId, page, pageSize);
                return View(resiliences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading resilience index");
                TempData["ErrorMessage"] = "Failed to load resilience initiatives.";
                return RedirectToAction("Dashboard");
            }
        }

        // GET: Resilience/Create
        [Authorize(GrcPermissions.Resilience.Create)]
        public IActionResult Create()
        {
            return View(new CreateResilienceDto());
        }

        // POST: Resilience/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(GrcPermissions.Resilience.Create)]
        public async Task<IActionResult> Create(CreateResilienceDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var tenantId = (_workspaceContext?.GetCurrentTenantId() ?? Guid.Empty);
                    if (tenantId == Guid.Empty)
                    {
                        _logger.LogWarning("Tenant ID not found in context");
                        ModelState.AddModelError(string.Empty, "Tenant context is required.");
                        return View(dto);
                    }

                    await _policyHelper.EnforceCreateAsync("Resilience", dto);
                    var resilience = await _resilienceService.CreateResilienceAsync(tenantId, dto);

                    TempData["SuccessMessage"] = "Resilience initiative created successfully.";
                    return RedirectToAction(nameof(Details), new { id = resilience.Id });
                }
                catch (PolicyViolationException pex)
                {
                    _logger.LogWarning(pex, "Policy violation creating resilience initiative");
                    ModelState.AddModelError("", $"Policy Violation: {pex.Message}");
                    if (!string.IsNullOrEmpty(pex.RemediationHint))
                        ModelState.AddModelError("", $"Remediation: {pex.RemediationHint}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating resilience initiative");
                    ModelState.AddModelError(string.Empty, "An error occurred while creating the resilience initiative.");
                }
            }
            return View(dto);
        }

        // GET: Resilience/Details/{id}
        [Authorize(GrcPermissions.Resilience.View)]
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var tenantId = (_workspaceContext?.GetCurrentTenantId() ?? Guid.Empty);
                if (tenantId == Guid.Empty)
                {
                    _logger.LogWarning("Tenant ID not found in context");
                    return RedirectToAction("Index", "Home");
                }

                var resilience = await _resilienceService.GetResilienceAsync(tenantId, id);
                if (resilience == null)
                {
                    return NotFound();
                }

                return View(resilience);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading resilience details for {ResilienceId}", id);
                TempData["ErrorMessage"] = "Failed to load resilience initiative details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Resilience/Edit/{id}
        [Authorize(GrcPermissions.Resilience.Edit)]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var tenantId = (_workspaceContext?.GetCurrentTenantId() ?? Guid.Empty);
                if (tenantId == Guid.Empty)
                {
                    _logger.LogWarning("Tenant ID not found in context");
                    return RedirectToAction("Index", "Home");
                }

                var resilience = await _resilienceService.GetResilienceAsync(tenantId, id);
                if (resilience == null)
                {
                    return NotFound();
                }

                var updateDto = new UpdateResilienceDto
                {
                    // Map properties from ResilienceDto to UpdateResilienceDto
                    // This will need to be adjusted based on actual DTO structures
                };

                return View(updateDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading resilience for edit {ResilienceId}", id);
                TempData["ErrorMessage"] = "Failed to load resilience initiative for editing.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Resilience/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(GrcPermissions.Resilience.Edit)]
        public async Task<IActionResult> Edit(Guid id, UpdateResilienceDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var tenantId = (_workspaceContext?.GetCurrentTenantId() ?? Guid.Empty);
                    if (tenantId == Guid.Empty)
                    {
                        _logger.LogWarning("Tenant ID not found in context");
                        ModelState.AddModelError(string.Empty, "Tenant context is required.");
                        return View(dto);
                    }

                    await _policyHelper.EnforceUpdateAsync("Resilience", dto);
                    var resilience = await _resilienceService.UpdateResilienceAsync(tenantId, id, dto);

                    TempData["SuccessMessage"] = "Resilience initiative updated successfully.";
                    return RedirectToAction(nameof(Details), new { id = resilience.Id });
                }
                catch (PolicyViolationException pex)
                {
                    _logger.LogWarning(pex, "Policy violation updating resilience {ResilienceId}", id);
                    ModelState.AddModelError("", $"Policy Violation: {pex.Message}");
                    if (!string.IsNullOrEmpty(pex.RemediationHint))
                        ModelState.AddModelError("", $"Remediation: {pex.RemediationHint}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating resilience {ResilienceId}", id);
                    ModelState.AddModelError("", "Error updating resilience initiative. Please try again.");
                }
            }
            return View(dto);
        }

        // POST: Resilience/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(GrcPermissions.Resilience.Delete)]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var tenantId = (_workspaceContext?.GetCurrentTenantId() ?? Guid.Empty);
                if (tenantId == Guid.Empty)
                {
                    _logger.LogWarning("Tenant ID not found in context");
                    TempData["ErrorMessage"] = "Tenant context is required.";
                    return RedirectToAction(nameof(Index));
                }

                await _policyHelper.EnforceDeleteAsync("Resilience", id);
                var result = await _resilienceService.DeleteResilienceAsync(tenantId, id);

                if (result)
                {
                    TempData["SuccessMessage"] = "Resilience initiative deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete resilience initiative.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (PolicyViolationException pex)
            {
                _logger.LogWarning(pex, "Policy violation deleting resilience {ResilienceId}", id);
                TempData["ErrorMessage"] = $"Policy Violation: {pex.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting resilience {ResilienceId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the resilience initiative.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // POST: Resilience/Assess/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(GrcPermissions.Resilience.Manage)]
        public async Task<IActionResult> Assess(Guid id, ResilienceAssessmentRequestDto? request = null)
        {
            try
            {
                var tenantId = (_workspaceContext?.GetCurrentTenantId() ?? Guid.Empty);
                if (tenantId == Guid.Empty)
                {
                    _logger.LogWarning("Tenant ID not found in context");
                    TempData["ErrorMessage"] = "Tenant context is required.";
                    return RedirectToAction(nameof(Index));
                }

                var resilience = await _resilienceService.AssessResilienceAsync(tenantId, id, request);
                TempData["SuccessMessage"] = "Resilience assessment completed successfully.";
                return RedirectToAction(nameof(Details), new { id = resilience.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assessing resilience {ResilienceId}", id);
                TempData["ErrorMessage"] = "Failed to complete resilience assessment.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // GET: Resilience/Incidents
        [Authorize(GrcPermissions.Resilience.Monitor)]
        public async Task<IActionResult> Incidents(string? status = null, int page = 1, int pageSize = 20)
        {
            try
            {
                var tenantId = (_workspaceContext?.GetCurrentTenantId() ?? Guid.Empty);
                if (tenantId == Guid.Empty)
                {
                    _logger.LogWarning("Tenant ID not found in context");
                    return RedirectToAction("Index", "Home");
                }

                var incidents = await _resilienceService.GetIncidentsAsync(tenantId, status, page, pageSize);
                ViewBag.CurrentStatus = status;
                return View(incidents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading incidents");
                TempData["ErrorMessage"] = "Failed to load incidents.";
                return RedirectToAction("Dashboard");
            }
        }

        // GET: Resilience/CreateIncident
        [Authorize(GrcPermissions.Resilience.Manage)]
        public IActionResult CreateIncident()
        {
            return View(new CreateIncidentDto());
        }

        // POST: Resilience/CreateIncident
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(GrcPermissions.Resilience.Manage)]
        public async Task<IActionResult> CreateIncident(CreateIncidentDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var tenantId = (_workspaceContext?.GetCurrentTenantId() ?? Guid.Empty);
                    if (tenantId == Guid.Empty)
                    {
                        _logger.LogWarning("Tenant ID not found in context");
                        ModelState.AddModelError(string.Empty, "Tenant context is required.");
                        return View(dto);
                    }

                    var incident = await _resilienceService.CreateIncidentAsync(tenantId, dto);
                    TempData["SuccessMessage"] = "Incident created successfully.";
                    return RedirectToAction(nameof(Incidents));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating incident");
                    ModelState.AddModelError(string.Empty, "An error occurred while creating the incident.");
                }
            }
            return View(dto);
        }
    }
}
