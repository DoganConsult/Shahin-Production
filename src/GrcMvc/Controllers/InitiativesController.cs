using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GrcMvc.Services.Interfaces;
using GrcMvc.Authorization;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Controllers
{
    /// <summary>
    /// Initiatives Controller - Stage 6: Continuous Sustainability
    /// Manages continuous improvement initiatives and backlog
    /// </summary>
    [Authorize]
    [RequireTenant]
    public class InitiativesController : Controller
    {
        private readonly ISustainabilityService _sustainabilityService;
        private readonly ILogger<InitiativesController> _logger;
        private readonly IWorkspaceContextService? _workspaceContext;

        public InitiativesController(
            ISustainabilityService sustainabilityService,
            ILogger<InitiativesController> logger,
            IWorkspaceContextService? workspaceContext = null)
        {
            _sustainabilityService = sustainabilityService;
            _logger = logger;
            _workspaceContext = workspaceContext;
        }

        [HttpGet]
        public async Task<IActionResult> Identification()
        {
            var tenantId = (_workspaceContext?.GetCurrentTenantId() ?? Guid.Empty);
            if (tenantId == Guid.Empty) return RedirectToAction("Index", "Home");
            
            var roadmap = await _sustainabilityService.GetMaturityRoadmapAsync(tenantId);
            return View(roadmap);
        }

        [HttpGet]
        public async Task<IActionResult> Backlog(string? status = null)
        {
            var tenantId = (_workspaceContext?.GetCurrentTenantId() ?? Guid.Empty);
            if (tenantId == Guid.Empty) return RedirectToAction("Index", "Home");
            
            var initiatives = await _sustainabilityService.GetImprovementInitiativesAsync(tenantId, status);
            ViewBag.Status = status;
            return View(initiatives);
        }

        [HttpGet]
        public async Task<IActionResult> Prioritization()
        {
            var tenantId = (_workspaceContext?.GetCurrentTenantId() ?? Guid.Empty);
            if (tenantId == Guid.Empty) return RedirectToAction("Index", "Home");
            
            var initiatives = await _sustainabilityService.GetImprovementInitiativesAsync(tenantId);
            return View(initiatives);
        }
    }
}