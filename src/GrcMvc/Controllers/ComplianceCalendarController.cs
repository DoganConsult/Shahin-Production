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
    public class ComplianceCalendarController : Controller
    {
        private readonly IComplianceCalendarService _complianceCalendarService;
        private readonly ILogger<ComplianceCalendarController> _logger;
        private readonly PolicyEnforcementHelper _policyHelper;

        public ComplianceCalendarController(
            IComplianceCalendarService complianceCalendarService,
            ILogger<ComplianceCalendarController> logger,
            PolicyEnforcementHelper policyHelper)
        {
            _complianceCalendarService = complianceCalendarService;
            _logger = logger;
            _policyHelper = policyHelper;
        }

        [Authorize(GrcPermissions.ComplianceCalendar.View)]
        public async Task<IActionResult> Index()
        {
            try
            {
                var events = await _complianceCalendarService.GetAllAsync();
                return View(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading compliance events");
                TempData["Error"] = "Error loading compliance events. Please try again.";
                return View(new List<ComplianceEventDto>());
            }
        }

        [Authorize(GrcPermissions.ComplianceCalendar.View)]
        public async Task<IActionResult> Details(Guid id)
        {
            var complianceEvent = await _complianceCalendarService.GetByIdAsync(id);
            if (complianceEvent == null) return NotFound();
            return View(complianceEvent);
        }

        [Authorize(GrcPermissions.ComplianceCalendar.Manage)]
        public IActionResult Create() => View(new CreateComplianceEventDto { EventDate = DateTime.Today });

        [HttpPost, ValidateAntiForgeryToken, Authorize(GrcPermissions.ComplianceCalendar.Manage)]
        public async Task<IActionResult> Create(CreateComplianceEventDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _policyHelper.EnforceCreateAsync("ComplianceEvent", dto, dataClassification: dto.DataClassification, owner: dto.Owner);
                    var complianceEvent = await _complianceCalendarService.CreateAsync(dto);
                    TempData["Success"] = "Compliance event created successfully";
                    return RedirectToAction(nameof(Details), new { id = complianceEvent.Id });
                }
                catch (PolicyViolationException pex)
                {
                    _logger.LogWarning(pex, "Policy violation creating compliance event");
                    ModelState.AddModelError("", "A policy violation occurred. Please review the requirements.");
                    if (!string.IsNullOrEmpty(pex.RemediationHint)) ModelState.AddModelError("", $"Remediation: {pex.RemediationHint}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating compliance event");
                    ModelState.AddModelError("", "Error creating compliance event. Please try again.");
                }
            }
            return View(dto);
        }

        [Authorize(GrcPermissions.ComplianceCalendar.Manage)]
        public async Task<IActionResult> Edit(Guid id)
        {
            var complianceEvent = await _complianceCalendarService.GetByIdAsync(id);
            if (complianceEvent == null) return NotFound();

            var updateDto = new UpdateComplianceEventDto
            {
                Id = complianceEvent.Id,
                EventNumber = complianceEvent.EventNumber,
                Title = complianceEvent.Title,
                Description = complianceEvent.Description,
                EventType = complianceEvent.EventType,
                Category = complianceEvent.Category,
                EventDate = complianceEvent.EventDate,
                DueDate = complianceEvent.DueDate,
                ReminderDate = complianceEvent.ReminderDate,
                Status = complianceEvent.Status,
                Priority = complianceEvent.Priority,
                AssignedTo = complianceEvent.AssignedTo,
                RelatedRegulatorId = complianceEvent.RelatedRegulatorId,
                RelatedFrameworkId = complianceEvent.RelatedFrameworkId,
                RelatedAssessmentId = complianceEvent.RelatedAssessmentId,
                RecurrencePattern = complianceEvent.RecurrencePattern,
                Notes = complianceEvent.Notes,
                DataClassification = complianceEvent.DataClassification,
                Owner = complianceEvent.Owner
            };

            return View(updateDto);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(GrcPermissions.ComplianceCalendar.Manage)]
        public async Task<IActionResult> Edit(Guid id, UpdateComplianceEventDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _policyHelper.EnforceUpdateAsync("ComplianceEvent", dto, dataClassification: dto.DataClassification, owner: dto.Owner);
                    var complianceEvent = await _complianceCalendarService.UpdateAsync(id, dto);
                    if (complianceEvent == null) return NotFound();
                    TempData["Success"] = "Compliance event updated successfully";
                    return RedirectToAction(nameof(Details), new { id = complianceEvent.Id });
                }
                catch (PolicyViolationException pex)
                {
                    _logger.LogWarning(pex, "Policy violation updating compliance event {EventId}", id);
                    ModelState.AddModelError("", "A policy violation occurred. Please review the requirements.");
                    if (!string.IsNullOrEmpty(pex.RemediationHint)) ModelState.AddModelError("", $"Remediation: {pex.RemediationHint}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating compliance event {EventId}", id);
                    ModelState.AddModelError("", "Error updating compliance event. Please try again.");
                }
            }
            return View(dto);
        }

        [Authorize(GrcPermissions.ComplianceCalendar.Manage)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var complianceEvent = await _complianceCalendarService.GetByIdAsync(id);
            if (complianceEvent == null) return NotFound();
            return View(complianceEvent);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken, Authorize(GrcPermissions.ComplianceCalendar.Manage)]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                var complianceEvent = await _complianceCalendarService.GetByIdAsync(id);
                if (complianceEvent != null)
                {
                    await _policyHelper.EnforceAsync("delete", "ComplianceEvent", complianceEvent, dataClassification: complianceEvent.DataClassification, owner: complianceEvent.Owner);
                }

                await _complianceCalendarService.DeleteAsync(id);
                TempData["Success"] = "Compliance event deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (PolicyViolationException pex)
            {
                _logger.LogWarning(pex, "Policy violation deleting compliance event {EventId}", id);
                TempData["Error"] = "A policy violation occurred. Please review the requirements.";
                return RedirectToAction(nameof(Delete), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting compliance event {EventId}", id);
                TempData["Error"] = "Error deleting compliance event. Please try again.";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        [Authorize(GrcPermissions.ComplianceCalendar.View)]
        public async Task<IActionResult> Upcoming(int days = 30)
        {
            try
            {
                var events = await _complianceCalendarService.GetUpcomingEventsAsync(days);
                ViewBag.Days = days;
                return View(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading upcoming compliance events");
                TempData["Error"] = "Error loading upcoming events. Please try again.";
                return View(new List<ComplianceEventDto>());
            }
        }
    }
}
