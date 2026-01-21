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
    public class ControlController : Controller
    {
        private readonly IControlService _controlService;
        private readonly IRiskService _riskService;
        private readonly ILogger<ControlController> _logger;
        private readonly IWorkspaceContextService? _workspaceContext;
        private readonly PolicyEnforcementHelper _policyHelper;

        public ControlController(IControlService controlService, IRiskService riskService, ILogger<ControlController> logger, PolicyEnforcementHelper policyHelper, IWorkspaceContextService? workspaceContext = null)
        {
            _controlService = controlService;
            _riskService = riskService;
            _logger = logger;
            _policyHelper = policyHelper;
            _workspaceContext = workspaceContext;
        }

        [Authorize(GrcPermissions.Frameworks.View)] // Controls are part of frameworks
        public async Task<IActionResult> Index()
        {
            var controls = await _controlService.GetAllAsync();
            return View(controls);
        }

        [Authorize(GrcPermissions.Frameworks.View)]
        public async Task<IActionResult> Details(Guid id)
        {
            var control = await _controlService.GetByIdAsync(id);
            if (control == null) return NotFound();
            return View(control);
        }

        [Authorize(GrcPermissions.Frameworks.Create)]
        public async Task<IActionResult> Create(Guid? riskId = null)
        {
            var model = new CreateControlDto();
            if (riskId.HasValue)
            {
                model.RiskId = riskId.Value;
                var riskResult = await _riskService.GetByIdAsync(riskId.Value);
                ViewBag.RiskName = riskResult.IsSuccess ? riskResult.Value?.Name : null;
            }
            var risksResult = await _riskService.GetAllAsync();
            ViewBag.Risks = risksResult.IsSuccess ? risksResult.Value : Enumerable.Empty<GrcMvc.Models.DTOs.RiskDto>();
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(GrcPermissions.Frameworks.Create)]
        public async Task<IActionResult> Create(CreateControlDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _policyHelper.EnforceCreateAsync("Control", dto, dataClassification: dto.DataClassification, owner: dto.Owner);
                    var control = await _controlService.CreateAsync(dto);
                    TempData["SuccessMessage"] = "Control created successfully.";
                    return RedirectToAction(nameof(Details), new { id = control.Id });
                }
                catch (PolicyViolationException pex)
                {
                    _logger.LogWarning(pex, "Policy violation creating control");
                    ModelState.AddModelError("", "A policy violation occurred. Please review the requirements.");
                    if (!string.IsNullOrEmpty(pex.RemediationHint)) ModelState.AddModelError("", $"Remediation: {pex.RemediationHint}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating control");
                    ModelState.AddModelError(string.Empty, "An error occurred while creating the control.");
                }
            }
            ViewBag.Risks = await _riskService.GetAllAsync();
            return View(dto);
        }

        [Authorize(GrcPermissions.Frameworks.Update)]
        public async Task<IActionResult> Edit(Guid id)
        {
            var control = await _controlService.GetByIdAsync(id);
            if (control == null) return NotFound();

            var updateDto = new UpdateControlDto
            {
                Id = control.Id,
                ControlNumber = control.ControlNumber,
                Name = control.Name,
                Description = control.Description,
                ControlType = control.ControlType,
                Category = control.Category,
                Status = control.Status,
                Owner = control.Owner,
                DataClassification = control.DataClassification,
                RiskId = control.RiskId,
                Effectiveness = control.Effectiveness,
                TestingFrequency = control.TestingFrequency,
                LastTestedDate = control.LastTestedDate
            };

            ViewBag.Risks = await _riskService.GetAllAsync();
            return View(updateDto);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(GrcPermissions.Frameworks.Update)]
        public async Task<IActionResult> Edit(Guid id, UpdateControlDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _policyHelper.EnforceUpdateAsync("Control", dto, dataClassification: dto.DataClassification, owner: dto.Owner);
                    var control = await _controlService.UpdateAsync(id, dto);
                    if (control == null) return NotFound();
                    TempData["SuccessMessage"] = "Control updated successfully.";
                    return RedirectToAction(nameof(Details), new { id = control.Id });
                }
                catch (PolicyViolationException pex)
                {
                    _logger.LogWarning(pex, "Policy violation updating control {ControlId}", id);
                    ModelState.AddModelError("", "A policy violation occurred. Please review the requirements.");
                    if (!string.IsNullOrEmpty(pex.RemediationHint)) ModelState.AddModelError("", $"Remediation: {pex.RemediationHint}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating control {ControlId}", id);
                    ModelState.AddModelError("", "Error updating control. Please try again.");
                }
            }
            ViewBag.Risks = await _riskService.GetAllAsync();
            return View(dto);
        }

        [Authorize(GrcPermissions.Frameworks.Update)]
        public async Task<IActionResult> UpdateControl(Guid id, [FromBody] UpdateControlDto updateControlDto)
        {
            var controlDto = await _controlService.UpdateAsync(id, updateControlDto);
            if (controlDto == null) return NotFound();
            return Ok(controlDto);
        }

        [Authorize(GrcPermissions.Frameworks.Delete)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var control = await _controlService.GetByIdAsync(id);
            if (control == null) return NotFound();
            return View(control);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken, Authorize(GrcPermissions.Frameworks.Delete)]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                var control = await _controlService.GetByIdAsync(id);
                if (control != null)
                {
                    await _policyHelper.EnforceAsync("delete", "Control", control, dataClassification: control.DataClassification, owner: control.Owner);
                }

                await _controlService.DeleteAsync(id);
                TempData["SuccessMessage"] = "Control deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (PolicyViolationException pex)
            {
                _logger.LogWarning(pex, "Policy violation deleting control {ControlId}", id);
                TempData["ErrorMessage"] = "A policy violation occurred. Please review the requirements.";
                return RedirectToAction(nameof(Delete), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting control {ControlId}", id);
                TempData["ErrorMessage"] = "Error deleting control. Please try again.";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }
    }
}
