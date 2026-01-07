using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrcMvc.Services.Interfaces;
using GrcMvc.Models.DTOs;
using GrcMvc.Application.Permissions;
using GrcMvc.Application.Policy;
using GrcMvc.Authorization;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace GrcMvc.Controllers
{
    [Authorize]
    [RequireTenant]
    public class ActionPlansController : Controller
    {
        private readonly IActionPlanService _actionPlanService;
        private readonly ILogger<ActionPlansController> _logger;
        private readonly PolicyEnforcementHelper _policyHelper;

        public ActionPlansController(
            IActionPlanService actionPlanService,
            ILogger<ActionPlansController> logger,
            PolicyEnforcementHelper policyHelper)
        {
            _actionPlanService = actionPlanService;
            _logger = logger;
            _policyHelper = policyHelper;
        }

        [Authorize(GrcPermissions.ActionPlans.View)]
        public async Task<IActionResult> Index()
        {
            try
            {
                var actionPlans = await _actionPlanService.GetAllAsync();
                return View(actionPlans);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading action plans");
                TempData["Error"] = "Error loading action plans. Please try again.";
                return View(new List<ActionPlanDto>());
            }
        }

        [Authorize(GrcPermissions.ActionPlans.View)]
        public async Task<IActionResult> Details(Guid id)
        {
            var actionPlan = await _actionPlanService.GetByIdAsync(id);
            if (actionPlan == null) return NotFound();
            return View(actionPlan);
        }

        [Authorize(GrcPermissions.ActionPlans.Manage)]
        public IActionResult Create() => View(new CreateActionPlanDto());

        [HttpPost, ValidateAntiForgeryToken, Authorize(GrcPermissions.ActionPlans.Manage)]
        public async Task<IActionResult> Create(CreateActionPlanDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _policyHelper.EnforceCreateAsync("ActionPlan", dto, dataClassification: dto.DataClassification, owner: dto.Owner);
                    var actionPlan = await _actionPlanService.CreateAsync(dto);
                    TempData["Success"] = "Action plan created successfully";
                    return RedirectToAction(nameof(Details), new { id = actionPlan.Id });
                }
                catch (PolicyViolationException pex)
                {
                    _logger.LogWarning(pex, "Policy violation creating action plan");
                    ModelState.AddModelError("", $"Policy Violation: {pex.Message}");
                    if (!string.IsNullOrEmpty(pex.RemediationHint)) ModelState.AddModelError("", $"Remediation: {pex.RemediationHint}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating action plan");
                    ModelState.AddModelError("", "Error creating action plan. Please try again.");
                }
            }
            return View(dto);
        }

        [Authorize(GrcPermissions.ActionPlans.Manage)]
        public async Task<IActionResult> Edit(Guid id)
        {
            var actionPlan = await _actionPlanService.GetByIdAsync(id);
            if (actionPlan == null) return NotFound();

            var updateDto = new UpdateActionPlanDto
            {
                Id = actionPlan.Id,
                PlanNumber = actionPlan.PlanNumber,
                Title = actionPlan.Title,
                Description = actionPlan.Description,
                Category = actionPlan.Category,
                Status = actionPlan.Status,
                Priority = actionPlan.Priority,
                AssignedTo = actionPlan.AssignedTo,
                StartDate = actionPlan.StartDate,
                DueDate = actionPlan.DueDate,
                CompletedDate = actionPlan.CompletedDate,
                Notes = actionPlan.Notes,
                RelatedRiskId = actionPlan.RelatedRiskId,
                RelatedAuditId = actionPlan.RelatedAuditId,
                RelatedAssessmentId = actionPlan.RelatedAssessmentId,
                RelatedControlId = actionPlan.RelatedControlId,
                DataClassification = actionPlan.DataClassification,
                Owner = actionPlan.Owner
            };

            return View(updateDto);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(GrcPermissions.ActionPlans.Manage)]
        public async Task<IActionResult> Edit(Guid id, UpdateActionPlanDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _policyHelper.EnforceUpdateAsync("ActionPlan", dto, dataClassification: dto.DataClassification, owner: dto.Owner);
                    var actionPlan = await _actionPlanService.UpdateAsync(id, dto);
                    if (actionPlan == null) return NotFound();
                    TempData["Success"] = "Action plan updated successfully";
                    return RedirectToAction(nameof(Details), new { id = actionPlan.Id });
                }
                catch (PolicyViolationException pex)
                {
                    _logger.LogWarning(pex, "Policy violation updating action plan {ActionPlanId}", id);
                    ModelState.AddModelError("", $"Policy Violation: {pex.Message}");
                    if (!string.IsNullOrEmpty(pex.RemediationHint)) ModelState.AddModelError("", $"Remediation: {pex.RemediationHint}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating action plan {ActionPlanId}", id);
                    ModelState.AddModelError("", "Error updating action plan. Please try again.");
                }
            }
            return View(dto);
        }

        [Authorize(GrcPermissions.ActionPlans.Manage)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var actionPlan = await _actionPlanService.GetByIdAsync(id);
            if (actionPlan == null) return NotFound();
            return View(actionPlan);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken, Authorize(GrcPermissions.ActionPlans.Manage)]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                var actionPlan = await _actionPlanService.GetByIdAsync(id);
                if (actionPlan != null)
                {
                    await _policyHelper.EnforceAsync("delete", "ActionPlan", actionPlan, dataClassification: actionPlan.DataClassification, owner: actionPlan.Owner);
                }

                await _actionPlanService.DeleteAsync(id);
                TempData["Success"] = "Action plan deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (PolicyViolationException pex)
            {
                _logger.LogWarning(pex, "Policy violation deleting action plan {ActionPlanId}", id);
                TempData["Error"] = $"Policy Violation: {pex.Message}. {pex.RemediationHint}";
                return RedirectToAction(nameof(Delete), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting action plan {ActionPlanId}", id);
                TempData["Error"] = "Error deleting action plan. Please try again.";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(GrcPermissions.ActionPlans.Close)]
        public async Task<IActionResult> Close(Guid id)
        {
            try
            {
                var actionPlan = await _actionPlanService.GetByIdAsync(id);
                if (actionPlan == null) return NotFound();
                await _policyHelper.EnforceAsync("close", "ActionPlan", actionPlan, dataClassification: actionPlan.DataClassification, owner: actionPlan.Owner);
                await _actionPlanService.CloseAsync(id);
                TempData["Success"] = "Action plan closed successfully";
            }
            catch (PolicyViolationException pex)
            {
                _logger.LogWarning(pex, "Policy violation closing action plan {ActionPlanId}", id);
                TempData["Error"] = $"Policy Violation: {pex.Message}. {pex.RemediationHint}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing action plan {ActionPlanId}", id);
                TempData["Error"] = "Error closing action plan. Please try again.";
            }
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
