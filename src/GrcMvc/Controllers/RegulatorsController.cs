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
    public class RegulatorsController : Controller
    {
        private readonly IRegulatorService _regulatorService;
        private readonly ILogger<RegulatorsController> _logger;
        private readonly PolicyEnforcementHelper _policyHelper;

        public RegulatorsController(
            IRegulatorService regulatorService,
            ILogger<RegulatorsController> logger,
            PolicyEnforcementHelper policyHelper)
        {
            _regulatorService = regulatorService;
            _logger = logger;
            _policyHelper = policyHelper;
        }

        [Authorize(GrcPermissions.Regulators.View)]
        public async Task<IActionResult> Index()
        {
            try
            {
                var regulators = await _regulatorService.GetAllAsync();
                return View(regulators);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading regulators");
                TempData["Error"] = "Error loading regulators. Please try again.";
                return View(new List<RegulatorDto>());
            }
        }

        [Authorize(GrcPermissions.Regulators.View)]
        public async Task<IActionResult> Details(Guid id)
        {
            var regulator = await _regulatorService.GetByIdAsync(id);
            if (regulator == null) return NotFound();
            return View(regulator);
        }

        [Authorize(GrcPermissions.Regulators.Manage)]
        public IActionResult Create() => View(new CreateRegulatorDto());

        [HttpPost, ValidateAntiForgeryToken, Authorize(GrcPermissions.Regulators.Manage)]
        public async Task<IActionResult> Create(CreateRegulatorDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _policyHelper.EnforceCreateAsync("Regulator", dto, dataClassification: dto.DataClassification, owner: dto.Owner);
                    var regulator = await _regulatorService.CreateAsync(dto);
                    TempData["Success"] = "Regulator created successfully";
                    return RedirectToAction(nameof(Details), new { id = regulator.Id });
                }
                catch (PolicyViolationException pex)
                {
                    _logger.LogWarning(pex, "Policy violation creating regulator");
                    ModelState.AddModelError("", $"Policy Violation: {pex.Message}");
                    if (!string.IsNullOrEmpty(pex.RemediationHint)) ModelState.AddModelError("", $"Remediation: {pex.RemediationHint}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating regulator");
                    ModelState.AddModelError("", "Error creating regulator. Please try again.");
                }
            }
            return View(dto);
        }

        [Authorize(GrcPermissions.Regulators.Manage)]
        public async Task<IActionResult> Edit(Guid id)
        {
            var regulator = await _regulatorService.GetByIdAsync(id);
            if (regulator == null) return NotFound();

            var updateDto = new UpdateRegulatorDto
            {
                Id = regulator.Id,
                RegulatorCode = regulator.RegulatorCode,
                Name = regulator.Name,
                NameAr = regulator.NameAr,
                Description = regulator.Description,
                Jurisdiction = regulator.Jurisdiction,
                Type = regulator.Type,
                Website = regulator.Website,
                ContactEmail = regulator.ContactEmail,
                ContactPhone = regulator.ContactPhone,
                Status = regulator.Status,
                IsPrimary = regulator.IsPrimary,
                Notes = regulator.Notes,
                DataClassification = regulator.DataClassification,
                Owner = regulator.Owner
            };

            return View(updateDto);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(GrcPermissions.Regulators.Manage)]
        public async Task<IActionResult> Edit(Guid id, UpdateRegulatorDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _policyHelper.EnforceUpdateAsync("Regulator", dto, dataClassification: dto.DataClassification, owner: dto.Owner);
                    var regulator = await _regulatorService.UpdateAsync(id, dto);
                    if (regulator == null) return NotFound();
                    TempData["Success"] = "Regulator updated successfully";
                    return RedirectToAction(nameof(Details), new { id = regulator.Id });
                }
                catch (PolicyViolationException pex)
                {
                    _logger.LogWarning(pex, "Policy violation updating regulator {RegulatorId}", id);
                    ModelState.AddModelError("", $"Policy Violation: {pex.Message}");
                    if (!string.IsNullOrEmpty(pex.RemediationHint)) ModelState.AddModelError("", $"Remediation: {pex.RemediationHint}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating regulator {RegulatorId}", id);
                    ModelState.AddModelError("", "Error updating regulator. Please try again.");
                }
            }
            return View(dto);
        }

        [Authorize(GrcPermissions.Regulators.Manage)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var regulator = await _regulatorService.GetByIdAsync(id);
            if (regulator == null) return NotFound();
            return View(regulator);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken, Authorize(GrcPermissions.Regulators.Manage)]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                var regulator = await _regulatorService.GetByIdAsync(id);
                if (regulator != null)
                {
                    await _policyHelper.EnforceAsync("delete", "Regulator", regulator, dataClassification: regulator.DataClassification, owner: regulator.Owner);
                }

                await _regulatorService.DeleteAsync(id);
                TempData["Success"] = "Regulator deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (PolicyViolationException pex)
            {
                _logger.LogWarning(pex, "Policy violation deleting regulator {RegulatorId}", id);
                TempData["Error"] = $"Policy Violation: {pex.Message}. {pex.RemediationHint}";
                return RedirectToAction(nameof(Delete), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting regulator {RegulatorId}", id);
                TempData["Error"] = "Error deleting regulator. Please try again.";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }
    }
}
