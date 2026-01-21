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
    public class VendorsController : Controller
    {
        private readonly IVendorService _vendorService;
        private readonly ILogger<VendorsController> _logger;
        private readonly PolicyEnforcementHelper _policyHelper;

        public VendorsController(
            IVendorService vendorService,
            ILogger<VendorsController> logger,
            PolicyEnforcementHelper policyHelper)
        {
            _vendorService = vendorService;
            _logger = logger;
            _policyHelper = policyHelper;
        }

        [Authorize(GrcPermissions.Vendors.View)]
        public async Task<IActionResult> Index()
        {
            try
            {
                var vendors = await _vendorService.GetAllAsync();
                return View(vendors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading vendors");
                TempData["Error"] = "Error loading vendors. Please try again.";
                return View(new List<VendorDto>());
            }
        }

        [Authorize(GrcPermissions.Vendors.View)]
        public async Task<IActionResult> Details(Guid id)
        {
            var vendor = await _vendorService.GetByIdAsync(id);
            if (vendor == null) return NotFound();
            return View(vendor);
        }

        [Authorize(GrcPermissions.Vendors.Manage)]
        public IActionResult Create() => View(new CreateVendorDto());

        [HttpPost, ValidateAntiForgeryToken, Authorize(GrcPermissions.Vendors.Manage)]
        public async Task<IActionResult> Create(CreateVendorDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _policyHelper.EnforceCreateAsync("Vendor", dto, dataClassification: dto.DataClassification, owner: dto.Owner);
                    var vendor = await _vendorService.CreateAsync(dto);
                    TempData["Success"] = "Vendor created successfully";
                    return RedirectToAction(nameof(Details), new { id = vendor.Id });
                }
                catch (PolicyViolationException pex)
                {
                    _logger.LogWarning(pex, "Policy violation creating vendor");
                    ModelState.AddModelError("", "A policy violation occurred. Please review the requirements.");
                    if (!string.IsNullOrEmpty(pex.RemediationHint)) ModelState.AddModelError("", $"Remediation: {pex.RemediationHint}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating vendor");
                    ModelState.AddModelError("", "Error creating vendor. Please try again.");
                }
            }
            return View(dto);
        }

        [Authorize(GrcPermissions.Vendors.Manage)]
        public async Task<IActionResult> Edit(Guid id)
        {
            var vendor = await _vendorService.GetByIdAsync(id);
            if (vendor == null) return NotFound();

            var updateDto = new UpdateVendorDto
            {
                Id = vendor.Id,
                VendorCode = vendor.VendorCode,
                Name = vendor.Name,
                Description = vendor.Description,
                Category = vendor.Category,
                Status = vendor.Status,
                ContactName = vendor.ContactName,
                ContactEmail = vendor.ContactEmail,
                ContactPhone = vendor.ContactPhone,
                Address = vendor.Address,
                Country = vendor.Country,
                RiskLevel = vendor.RiskLevel,
                LastAssessmentDate = vendor.LastAssessmentDate,
                NextAssessmentDate = vendor.NextAssessmentDate,
                AssessmentStatus = vendor.AssessmentStatus,
                Notes = vendor.Notes,
                DataClassification = vendor.DataClassification,
                Owner = vendor.Owner
            };

            return View(updateDto);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(GrcPermissions.Vendors.Manage)]
        public async Task<IActionResult> Edit(Guid id, UpdateVendorDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _policyHelper.EnforceUpdateAsync("Vendor", dto, dataClassification: dto.DataClassification, owner: dto.Owner);
                    var vendor = await _vendorService.UpdateAsync(id, dto);
                    if (vendor == null) return NotFound();
                    TempData["Success"] = "Vendor updated successfully";
                    return RedirectToAction(nameof(Details), new { id = vendor.Id });
                }
                catch (PolicyViolationException pex)
                {
                    _logger.LogWarning(pex, "Policy violation updating vendor {VendorId}", id);
                    ModelState.AddModelError("", "A policy violation occurred. Please review the requirements.");
                    if (!string.IsNullOrEmpty(pex.RemediationHint)) ModelState.AddModelError("", $"Remediation: {pex.RemediationHint}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating vendor {VendorId}", id);
                    ModelState.AddModelError("", "Error updating vendor. Please try again.");
                }
            }
            return View(dto);
        }

        [Authorize(GrcPermissions.Vendors.Manage)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var vendor = await _vendorService.GetByIdAsync(id);
            if (vendor == null) return NotFound();
            return View(vendor);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken, Authorize(GrcPermissions.Vendors.Manage)]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                var vendor = await _vendorService.GetByIdAsync(id);
                if (vendor != null)
                {
                    await _policyHelper.EnforceAsync("delete", "Vendor", vendor, dataClassification: vendor.DataClassification, owner: vendor.Owner);
                }

                await _vendorService.DeleteAsync(id);
                TempData["Success"] = "Vendor deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (PolicyViolationException pex)
            {
                _logger.LogWarning(pex, "Policy violation deleting vendor {VendorId}", id);
                TempData["Error"] = "A policy violation occurred. Please review the requirements.";
                return RedirectToAction(nameof(Delete), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting vendor {VendorId}", id);
                TempData["Error"] = "Error deleting vendor. Please try again.";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(GrcPermissions.Vendors.Assess)]
        public async Task<IActionResult> Assess(Guid id)
        {
            try
            {
                var vendor = await _vendorService.GetByIdAsync(id);
                if (vendor == null) return NotFound();
                await _policyHelper.EnforceAsync("assess", "Vendor", vendor, dataClassification: vendor.DataClassification, owner: vendor.Owner);
                await _vendorService.AssessAsync(id);
                TempData["Success"] = "Vendor assessment started successfully";
            }
            catch (PolicyViolationException pex)
            {
                _logger.LogWarning(pex, "Policy violation assessing vendor {VendorId}", id);
                TempData["Error"] = "A policy violation occurred. Please review the requirements.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assessing vendor {VendorId}", id);
                TempData["Error"] = "Error assessing vendor. Please try again.";
            }
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
