using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GrcMvc.Services.Interfaces;
using GrcMvc.Models.DTOs;
using GrcMvc.Application.Permissions;
using GrcMvc.Application.Policy;
using GrcMvc.Authorization;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Controllers
{
    /// <summary>
    /// Excellence Controller - Stage 5: Excellence & Benchmarking
    /// Manages organizational excellence, maturity assessment, and benchmarking
    /// </summary>
    [Authorize]
    [RequireTenant]
    public class ExcellenceController : Controller
    {
        private readonly IGrcProcessOrchestrator _orchestrator;
        private readonly ISustainabilityService _sustainabilityService;
        private readonly ILogger<ExcellenceController> _logger;
        private readonly IWorkspaceContextService? _workspaceContext;

        public ExcellenceController(
            IGrcProcessOrchestrator orchestrator,
            ISustainabilityService sustainabilityService,
            ILogger<ExcellenceController> logger,
            IWorkspaceContextService? workspaceContext = null)
        {
            _orchestrator = orchestrator;
            _sustainabilityService = sustainabilityService;
            _logger = logger;
            _workspaceContext = workspaceContext;
        }

        // GET: Excellence/Index
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

                // Get excellence score for ViewBag
                var excellenceScore = await _orchestrator.GetExcellenceScoreAsync(tenantId);
                ViewBag.ExcellenceScore = excellenceScore;

                // Get improvement initiatives and map to ExcellenceInitiativeDto
                var improvements = await _sustainabilityService.GetImprovementInitiativesAsync(tenantId);
                var initiatives = improvements.Select(i => new ExcellenceInitiativeDto
                {
                    Id = i.Id,
                    Name = i.Title,
                    Description = i.Description,
                    Type = i.Category,
                    Category = i.Category,
                    Owner = i.Owner,
                    Progress = i.PercentComplete,
                    TargetDate = i.TargetDate,
                    Status = i.Status
                }).ToList();

                return View(initiatives);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading excellence index");
                TempData["ErrorMessage"] = "Failed to load excellence data.";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Excellence/Dashboard
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

                var maturityScore = await _sustainabilityService.GetMaturityScoreAsync(tenantId);
                var dashboard = await _sustainabilityService.GetDashboardAsync(tenantId);

                ViewBag.MaturityScore = maturityScore;
                return View(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading excellence dashboard");
                TempData["ErrorMessage"] = "Failed to load excellence dashboard.";
                return RedirectToAction("Index");
            }
        }

        // GET: Excellence/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View(new ExcellenceInitiativeDto());
        }

        // POST: Excellence/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExcellenceInitiativeDto model)
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

                // Map to CreateImprovementDto and use sustainability service
                var createDto = new CreateImprovementDto
                {
                    Title = model.Name,
                    Description = model.Description ?? string.Empty,
                    Category = model.Category,
                    Priority = "Medium",
                    Owner = model.Owner,
                    TargetDate = model.TargetDate,
                    ExpectedImpact = 0
                };

                var result = await _sustainabilityService.CreateImprovementInitiativeAsync(tenantId, createDto);
                _logger.LogInformation("Created excellence initiative {InitiativeId} for tenant {TenantId}",
                    result.Id, tenantId);

                TempData["SuccessMessage"] = "Excellence initiative created successfully.";
                return RedirectToAction(nameof(Details), new { id = result.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating excellence initiative");
                TempData["ErrorMessage"] = "Failed to create excellence initiative.";
                return View(model);
            }
        }

        // GET: Excellence/Edit/{id}
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

                // Get initiatives and find the one to edit
                var initiatives = await _sustainabilityService.GetImprovementInitiativesAsync(tenantId);
                var initiative = initiatives.FirstOrDefault(i => i.Id == id);

                if (initiative == null)
                {
                    TempData["ErrorMessage"] = "Initiative not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Map to ExcellenceInitiativeDto
                var dto = new ExcellenceInitiativeDto
                {
                    Id = initiative.Id,
                    Name = initiative.Title,
                    Description = initiative.Description,
                    Type = initiative.Category,
                    Category = initiative.Category,
                    Owner = initiative.Owner,
                    Progress = initiative.PercentComplete,
                    TargetDate = initiative.TargetDate,
                    Status = initiative.Status
                };

                ViewBag.InitiativeId = id;
                return View(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading excellence initiative {InitiativeId} for edit", id);
                TempData["ErrorMessage"] = "Failed to load excellence initiative.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Excellence/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ExcellenceInitiativeDto model)
        {
            try
            {
                var tenantId = (_workspaceContext?.GetCurrentTenantId() ?? Guid.Empty);
                if (tenantId == Guid.Empty)
                {
                    return RedirectToAction("Index", "Home");
                }

                // Update progress using sustainability service
                var result = await _sustainabilityService.UpdateImprovementProgressAsync(
                    tenantId, id, model.Progress ?? 0, null);

                // If completed, mark as complete
                if (model.Status == "Completed" && (model.Progress ?? 0) >= 100)
                {
                    await _sustainabilityService.CompleteImprovementAsync(
                        tenantId, id, model.Owner, "Completed via Excellence module");
                }

                _logger.LogInformation("Updated excellence initiative {InitiativeId} for tenant {TenantId}",
                    id, tenantId);

                TempData["SuccessMessage"] = "Excellence initiative updated successfully.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating excellence initiative {InitiativeId}", id);
                TempData["ErrorMessage"] = "Failed to update excellence initiative.";
                ViewBag.InitiativeId = id;
                return View(model);
            }
        }

        // GET: Excellence/Details/{id}
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

                // Get initiatives and find the one to display
                var initiatives = await _sustainabilityService.GetImprovementInitiativesAsync(tenantId);
                var initiative = initiatives.FirstOrDefault(i => i.Id == id);

                if (initiative == null)
                {
                    TempData["ErrorMessage"] = "Initiative not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Map to ExcellenceInitiativeDto
                var dto = new ExcellenceInitiativeDto
                {
                    Id = initiative.Id,
                    Name = initiative.Title,
                    Description = initiative.Description,
                    Type = initiative.Category,
                    Category = initiative.Category,
                    Owner = initiative.Owner,
                    Progress = initiative.PercentComplete,
                    TargetDate = initiative.TargetDate,
                    Status = initiative.Status
                };

                ViewBag.InitiativeId = id;
                return View(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading excellence initiative {InitiativeId} details", id);
                TempData["ErrorMessage"] = "Failed to load excellence initiative details.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
