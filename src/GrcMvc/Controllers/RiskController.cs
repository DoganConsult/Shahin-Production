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
            var risks = await _riskService.GetAllAsync();
            return View(risks);
        }

        [Authorize(GrcPermissions.Risks.View)]
        public async Task<IActionResult> Details(Guid id)
        {
            var risk = await _riskService.GetByIdAsync(id);
            if (risk == null) return NotFound();
            return View(risk);
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
                    var risk = await _riskService.CreateAsync(dto);
                    TempData["SuccessMessage"] = "Risk created successfully.";
                    return RedirectToAction(nameof(Details), new { id = risk.Id });
                }
                catch (PolicyViolationException pex)
                {
                    _logger.LogWarning(pex, "Policy violation creating risk");
                    ModelState.AddModelError("", $"Policy Violation: {pex.Message}");
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
            var risk = await _riskService.GetByIdAsync(id);
            if (risk == null) return NotFound();

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
                    var risk = await _riskService.UpdateAsync(id, dto);
                    if (risk == null) return NotFound();
                    TempData["SuccessMessage"] = "Risk updated successfully.";
                    return RedirectToAction(nameof(Details), new { id = risk.Id });
                }
                catch (PolicyViolationException pex)
                {
                    _logger.LogWarning(pex, "Policy violation updating risk {RiskId}", id);
                    ModelState.AddModelError("", $"Policy Violation: {pex.Message}");
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
                var risk = await _riskService.GetByIdAsync(id);
                if (risk == null) return NotFound();
                await _policyHelper.EnforceAsync("accept", "Risk", risk, dataClassification: risk.DataClassification, owner: risk.Owner);
                await _riskService.AcceptAsync(id);
                TempData["SuccessMessage"] = "Risk accepted successfully.";
            }
            catch (PolicyViolationException pex)
            {
                _logger.LogWarning(pex, "Policy violation accepting risk {RiskId}", id);
                TempData["ErrorMessage"] = $"Policy Violation: {pex.Message}. {pex.RemediationHint}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting risk {RiskId}", id);
                TempData["ErrorMessage"] = "An error occurred while accepting the risk.";
            }
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
