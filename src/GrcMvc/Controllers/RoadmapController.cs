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
    /// Roadmap Controller - Stage 6: Continuous Sustainability
    /// Manages strategic roadmaps and multi-year planning
    /// </summary>
    [Authorize]
    [RequireTenant]
    public class RoadmapController : Controller
    {
        private readonly ISustainabilityService _sustainabilityService;
        private readonly ILogger<RoadmapController> _logger;
        private readonly IWorkspaceContextService? _workspaceContext;

        public RoadmapController(
            ISustainabilityService sustainabilityService,
            ILogger<RoadmapController> logger,
            IWorkspaceContextService? workspaceContext = null)
        {
            _sustainabilityService = sustainabilityService;
            _logger = logger;
            _workspaceContext = workspaceContext;
        }

        [HttpGet]
        public async Task<IActionResult> MultiYear(int years = 3)
        {
            var tenantId = (_workspaceContext?.GetCurrentTenantId() ?? Guid.Empty);
            if (tenantId == Guid.Empty) return RedirectToAction("Index", "Home");
            
            var roadmap = await _sustainabilityService.GetMaturityRoadmapAsync(tenantId);
            ViewBag.Years = years;
            return View(roadmap);
        }

        [HttpGet]
        public async Task<IActionResult> Approval()
        {
            var tenantId = (_workspaceContext?.GetCurrentTenantId() ?? Guid.Empty);
            if (tenantId == Guid.Empty) return RedirectToAction("Index", "Home");
            
            var roadmap = await _sustainabilityService.GetMaturityRoadmapAsync(tenantId);
            return View(roadmap);
        }

        [HttpGet]
        public async Task<IActionResult> Timeline()
        {
            var tenantId = (_workspaceContext?.GetCurrentTenantId() ?? Guid.Empty);
            if (tenantId == Guid.Empty) return RedirectToAction("Index", "Home");
            
            var roadmap = await _sustainabilityService.GetMaturityRoadmapAsync(tenantId);
            var history = await _sustainabilityService.GetMaturityHistoryAsync(tenantId, 24);
            ViewBag.History = history;
            return View(roadmap);
        }
    }
}