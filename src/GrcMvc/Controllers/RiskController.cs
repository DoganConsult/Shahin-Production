using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GrcMvc.Services.Interfaces;
using GrcMvc.Models.DTOs;
using GrcMvc.Models.ViewModels;
using GrcMvc.Application.Permissions;
using GrcMvc.Application.Policy;
using GrcMvc.Authorization;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Controllers
{
    [Authorize]
    [RequireTenant]
    public class RiskController : Controller
    {
        private readonly IRiskService _riskService;
        private readonly ILogger<RiskController> _logger;
        private readonly IWorkspaceContextService? _workspaceContext;
        private readonly PolicyEnforcementHelper _policyHelper;

        public RiskController(IRiskService riskService, ILogger<RiskController> logger, PolicyEnforcementHelper policyHelper, IWorkspaceContextService? workspaceContext = null)
        {
            _riskService = riskService;
            _logger = logger;
            _policyHelper = policyHelper;
            _workspaceContext = workspaceContext;
        }

        [Authorize(GrcPermissions.Risks.View)]
        public async Task<IActionResult> Index()
        {
            var result = await _riskService.GetAllAsync();
            return View(result.IsSuccess ? result.Value : Enumerable.Empty<RiskDto>());
        }

        [Authorize(GrcPermissions.Risks.View)]
        public async Task<IActionResult> Details(Guid id)
        {
            var result = await _riskService.GetByIdAsync(id);
            if (!result.IsSuccess || result.Value == null) return NotFound();
            return View(result.Value);
        }

        [Authorize(GrcPermissions.Risks.Manage)]
        public IActionResult Create() => View(new CreateRiskDto());

        [HttpPost, ValidateAntiForgeryToken, Authorize(GrcPermissions.Risks.Manage)]
        public async Task<IActionResult> Create(CreateRiskDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _policyHelper.EnforceCreateAsync("Risk", dto, dataClassification: dto.DataClassification, owner: dto.Owner);
                    var result = await _riskService.CreateAsync(dto);
                    if (!result.IsSuccess)
                    {
                        ModelState.AddModelError(string.Empty, result.Error ?? "Failed to create risk.");
                        return View(dto);
                    }
                    TempData["SuccessMessage"] = "Risk created successfully.";
                    return RedirectToAction(nameof(Details), new { id = result.Value.Id });
                }
                catch (PolicyViolationException pex)
                {
                    _logger.LogWarning(pex, "Policy violation creating risk");
                    ModelState.AddModelError("", "A policy violation occurred. Please review the requirements.");
                    if (!string.IsNullOrEmpty(pex.RemediationHint)) ModelState.AddModelError("", $"Remediation: {pex.RemediationHint}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating risk");
                    ModelState.AddModelError(string.Empty, "An error occurred while creating the risk.");
                }
            }
            return View(dto);
        }

        [Authorize(GrcPermissions.Risks.Manage)]
        public async Task<IActionResult> Edit(Guid id)
        {
            var result = await _riskService.GetByIdAsync(id);
            if (!result.IsSuccess || result.Value == null) return NotFound();
            var risk = result.Value;

            var updateDto = new UpdateRiskDto
            {
                Id = risk.Id,
                Name = risk.Name,
                Description = risk.Description,
                Category = risk.Category,
                Impact = risk.Impact,
                Probability = risk.Probability,
                RiskScore = risk.RiskScore,
                Status = risk.Status,
                Owner = risk.Owner,
                DataClassification = risk.DataClassification,
                MitigationStrategy = risk.MitigationStrategy,
                TreatmentPlan = risk.TreatmentPlan
            };

            return View(updateDto);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(GrcPermissions.Risks.Manage)]
        public async Task<IActionResult> Edit(Guid id, UpdateRiskDto dto)
        {
            _logger.LogDebug("Edit risk requested. RiskId={RiskId}, ModelStateValid={IsValid}", id, ModelState.IsValid);

            if (ModelState.IsValid)
            {
                try
                {
                    await _policyHelper.EnforceUpdateAsync("Risk", dto, dataClassification: dto.DataClassification, owner: dto.Owner);
                    var result = await _riskService.UpdateAsync(id, dto);
                    if (!result.IsSuccess || result.Value == null)
                    {
                        ModelState.AddModelError(string.Empty, result.Error ?? "Failed to update risk.");
                        return View(dto);
                    }
                    TempData["SuccessMessage"] = "Risk updated successfully.";
                    return RedirectToAction(nameof(Details), new { id = result.Value.Id });
                }
                catch (PolicyViolationException pex)
                {
                    _logger.LogWarning(pex, "Policy violation updating risk {RiskId}", id);
                    ModelState.AddModelError("", "A policy violation occurred. Please review the requirements.");
                    if (!string.IsNullOrEmpty(pex.RemediationHint)) ModelState.AddModelError("", $"Remediation: {pex.RemediationHint}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating risk {RiskId}", id);
                    ModelState.AddModelError("", "Error updating risk. Please try again.");
                }
            }
            return View(dto);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(GrcPermissions.Risks.Accept)]
        public async Task<IActionResult> Accept(Guid id)
        {
            try
            {
                var riskResult = await _riskService.GetByIdAsync(id);
                if (!riskResult.IsSuccess || riskResult.Value == null) return NotFound();
                var risk = riskResult.Value;
                await _policyHelper.EnforceAsync("accept", "Risk", risk, dataClassification: risk.DataClassification, owner: risk.Owner);
                var acceptResult = await _riskService.AcceptAsync(id);
                if (!acceptResult.IsSuccess)
                {
                    TempData["ErrorMessage"] = acceptResult.Error ?? "Failed to accept risk.";
                    return RedirectToAction(nameof(Details), new { id });
                }
                TempData["SuccessMessage"] = "Risk accepted successfully.";
            }
            catch (PolicyViolationException pex)
            {
                _logger.LogWarning(pex, "Policy violation accepting risk {RiskId}", id);
                TempData["ErrorMessage"] = "A policy violation occurred. Please review the requirements.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting risk {RiskId}", id);
                TempData["ErrorMessage"] = "An error occurred while accepting the risk.";
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        [Authorize(GrcPermissions.Risks.View)]
        public async Task<IActionResult> Statistics()
        {
            try
            {
                var result = await _riskService.GetStatisticsAsync();
                if (!result.IsSuccess || result.Value == null)
                {
                    TempData["ErrorMessage"] = result.Error ?? "Error loading statistics.";
                    return RedirectToAction(nameof(Index));
                }
                return View(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving risk statistics");
                TempData["ErrorMessage"] = "Error loading statistics.";
                return RedirectToAction(nameof(Index));
            }
        }

        [Authorize(GrcPermissions.Risks.View)]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                // Get current tenant ID from HTTP context
                var tenantIdClaim = User.FindFirst("TenantId");
                if (tenantIdClaim == null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
                {
                    _logger.LogWarning("Tenant ID not found in user claims");
                    TempData["ErrorMessage"] = "Unable to load dashboard data.";
                    return RedirectToAction(nameof(Index));
                }

                // Fetch dashboard data
                var statsResult = await _riskService.GetStatisticsAsync();
                var riskPostureResult = await _riskService.GetRiskPostureAsync(tenantId);
                var heatMapResult = await _riskService.GetHeatMapAsync(tenantId);
                var allRisksResult = await _riskService.GetAllAsync();

                if (!allRisksResult.IsSuccess)
                {
                    TempData["ErrorMessage"] = "Unable to load risks data.";
                    return RedirectToAction(nameof(Index));
                }

                var allRisks = allRisksResult.Value;

                // Get top 10 risks by score
                var topRisks = allRisks
                    .OrderByDescending(r => r.RiskScore)
                    .Take(10)
                    .ToList();

                // Get recent risks (created in last 30 days)
                var recentRisks = allRisks
                    .Where(r => r.CreatedDate >= DateTime.UtcNow.AddDays(-30))
                    .OrderByDescending(r => r.CreatedDate)
                    .Take(5)
                    .ToList();

                // Get upcoming reviews (risks with ReviewDate in next 30 days)
                var upcomingReviews = allRisks
                    .Where(r => r.ReviewDate.HasValue &&
                               r.ReviewDate.Value >= DateTime.UtcNow &&
                               r.ReviewDate.Value <= DateTime.UtcNow.AddDays(30))
                    .OrderBy(r => r.ReviewDate)
                    .Take(5)
                    .ToList();

                var viewModel = new RiskDashboardViewModel
                {
                    Statistics = statsResult.IsSuccess ? statsResult.Value : null,
                    RiskPosture = riskPostureResult.IsSuccess ? riskPostureResult.Value : null,
                    HeatMap = heatMapResult.IsSuccess ? heatMapResult.Value : null,
                    TopRisks = topRisks,
                    RecentRisks = recentRisks,
                    UpcomingReviews = upcomingReviews,
                    LoadedAt = DateTime.UtcNow
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading risk dashboard");
                TempData["ErrorMessage"] = "Error loading dashboard. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
