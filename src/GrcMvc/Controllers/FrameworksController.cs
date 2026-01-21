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
    public class FrameworksController : Controller
    {
        private readonly IFrameworkManagementService _frameworkService;
        private readonly ILogger<FrameworksController> _logger;
        private readonly PolicyEnforcementHelper _policyHelper;

        public FrameworksController(
            IFrameworkManagementService frameworkService,
            ILogger<FrameworksController> logger,
            PolicyEnforcementHelper policyHelper)
        {
            _frameworkService = frameworkService;
            _logger = logger;
            _policyHelper = policyHelper;
        }

        [Authorize(GrcPermissions.Frameworks.View)]
        public async Task<IActionResult> Index()
        {
            try
            {
                var frameworks = await _frameworkService.GetAllAsync();
                return View(frameworks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading frameworks");
                TempData["Error"] = "Error loading frameworks. Please try again.";
                return View(new List<FrameworkDto>());
            }
        }

        [Authorize(GrcPermissions.Frameworks.View)]
        public async Task<IActionResult> Details(Guid id)
        {
            var framework = await _frameworkService.GetByIdAsync(id);
            if (framework == null) return NotFound();
            return View(framework);
        }

        [Authorize(GrcPermissions.Frameworks.Create)]
        public IActionResult Create() => View(new CreateFrameworkDto());

        [HttpPost, ValidateAntiForgeryToken, Authorize(GrcPermissions.Frameworks.Create)]
        public async Task<IActionResult> Create(CreateFrameworkDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _policyHelper.EnforceCreateAsync("RegulatoryFramework", dto, dataClassification: dto.DataClassification, owner: dto.Owner);
                    var framework = await _frameworkService.CreateAsync(dto);
                    TempData["Success"] = "Framework created successfully";
                    return RedirectToAction(nameof(Details), new { id = framework.Id });
                }
                catch (PolicyViolationException pex)
                {
                    _logger.LogWarning(pex, "Policy violation creating framework");
                    ModelState.AddModelError("", "A policy violation occurred. Please review the requirements.");
                    if (!string.IsNullOrEmpty(pex.RemediationHint)) ModelState.AddModelError("", $"Remediation: {pex.RemediationHint}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating framework");
                    ModelState.AddModelError("", "Error creating framework. Please try again.");
                }
            }
            return View(dto);
        }

        [Authorize(GrcPermissions.Frameworks.Update)]
        public async Task<IActionResult> Edit(Guid id)
        {
            var framework = await _frameworkService.GetByIdAsync(id);
            if (framework == null) return NotFound();

            var updateDto = new UpdateFrameworkDto
            {
                Id = framework.Id,
                FrameworkCode = framework.FrameworkCode,
                Name = framework.Name,
                NameAr = framework.NameAr,
                Description = framework.Description,
                Version = framework.Version,
                Jurisdiction = framework.Jurisdiction,
                Type = framework.Type,
                Status = framework.Status,
                EffectiveDate = framework.EffectiveDate,
                ExpirationDate = framework.ExpirationDate,
                Website = framework.Website,
                Notes = framework.Notes,
                IsMandatory = framework.IsMandatory,
                DataClassification = framework.DataClassification,
                Owner = framework.Owner
            };

            return View(updateDto);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(GrcPermissions.Frameworks.Update)]
        public async Task<IActionResult> Edit(Guid id, UpdateFrameworkDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _policyHelper.EnforceUpdateAsync("RegulatoryFramework", dto, dataClassification: dto.DataClassification, owner: dto.Owner);
                    var framework = await _frameworkService.UpdateAsync(id, dto);
                    if (framework == null) return NotFound();
                    TempData["Success"] = "Framework updated successfully";
                    return RedirectToAction(nameof(Details), new { id = framework.Id });
                }
                catch (PolicyViolationException pex)
                {
                    _logger.LogWarning(pex, "Policy violation updating framework {FrameworkId}", id);
                    ModelState.AddModelError("", "A policy violation occurred. Please review the requirements.");
                    if (!string.IsNullOrEmpty(pex.RemediationHint)) ModelState.AddModelError("", $"Remediation: {pex.RemediationHint}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating framework {FrameworkId}", id);
                    ModelState.AddModelError("", "Error updating framework. Please try again.");
                }
            }
            return View(dto);
        }

        [Authorize(GrcPermissions.Frameworks.Delete)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var framework = await _frameworkService.GetByIdAsync(id);
            if (framework == null) return NotFound();
            return View(framework);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken, Authorize(GrcPermissions.Frameworks.Delete)]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                var framework = await _frameworkService.GetByIdAsync(id);
                if (framework != null)
                {
                    await _policyHelper.EnforceAsync("delete", "RegulatoryFramework", framework, dataClassification: framework.DataClassification, owner: framework.Owner);
                }

                await _frameworkService.DeleteAsync(id);
                TempData["Success"] = "Framework deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (PolicyViolationException pex)
            {
                _logger.LogWarning(pex, "Policy violation deleting framework {FrameworkId}", id);
                TempData["Error"] = "A policy violation occurred. Please review the requirements.";
                return RedirectToAction(nameof(Delete), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting framework {FrameworkId}", id);
                TempData["Error"] = "Error deleting framework. Please try again.";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }
    }
}
