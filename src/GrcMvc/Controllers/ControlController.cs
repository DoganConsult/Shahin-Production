using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GrcMvc.Services.Interfaces;
using GrcMvc.Models.DTOs;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Controllers
{
    [Authorize]
    public class ControlController : Controller
    {
        private readonly IControlService _controlService;
        private readonly IRiskService _riskService;
        private readonly ILogger<ControlController> _logger;

        public ControlController(
            IControlService controlService,
            IRiskService riskService,
            ILogger<ControlController> logger)
        {
            _controlService = controlService;
            _riskService = riskService;
            _logger = logger;
        }

        // GET: Control
        public async Task<IActionResult> Index()
        {
            var controls = await _controlService.GetAllAsync();
            return View(controls);
        }

        // GET: Control/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            var control = await _controlService.GetByIdAsync(id);
            if (control == null)
            {
                return NotFound();
            }
            return View(control);
        }

        // GET: Control/Create
        public async Task<IActionResult> Create(Guid? riskId = null)
        {
            var model = new CreateControlDto();

            if (riskId.HasValue)
            {
                model.RiskId = riskId.Value;
                ViewBag.RiskName = (await _riskService.GetByIdAsync(riskId.Value))?.Name;
            }

            ViewBag.Risks = await _riskService.GetAllAsync();
            return View(model);
        }

        // POST: Control/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateControlDto createControlDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var control = await _controlService.CreateAsync(createControlDto);
                    TempData["SuccessMessage"] = "Control created successfully.";
                    return RedirectToAction(nameof(Details), new { id = control.Id });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating control");
                    ModelState.AddModelError(string.Empty, "An error occurred while creating the control.");
                }
            }

            ViewBag.Risks = await _riskService.GetAllAsync();
            return View(createControlDto);
        }

        // GET: Control/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            var control = await _controlService.GetByIdAsync(id);
            if (control == null)
            {
                return NotFound();
            }

            var updateDto = new UpdateControlDto
            {
                Name = control.Name,
                Description = control.Description,
                Type = control.Type,
                Frequency = control.Frequency,
                Effectiveness = control.Effectiveness,
                Status = control.Status,
                Owner = control.Owner,
                RiskId = control.RiskId,
                ImplementationDate = control.ImplementationDate,
                LastReviewDate = control.LastReviewDate,
                NextReviewDate = control.NextReviewDate
            };

            ViewBag.Risks = await _riskService.GetAllAsync();
            return View(updateDto);
        }

        // POST: Control/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, UpdateControlDto updateControlDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var control = await _controlService.UpdateAsync(id, updateControlDto);
                    if (control == null)
                    {
                        return NotFound();
                    }

                    TempData["SuccessMessage"] = "Control updated successfully.";
                    return RedirectToAction(nameof(Details), new { id = control.Id });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating control");
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the control.");
                }
            }

            ViewBag.Risks = await _riskService.GetAllAsync();
            return View(updateControlDto);
        }

        // GET: Control/Delete/5
        public async Task<IActionResult> Delete(Guid id)
        {
            var control = await _controlService.GetByIdAsync(id);
            if (control == null)
            {
                return NotFound();
            }
            return View(control);
        }

        // POST: Control/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                await _controlService.DeleteAsync(id);
                TempData["SuccessMessage"] = "Control deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting control");
                TempData["ErrorMessage"] = "An error occurred while deleting the control.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Control/Matrix
        public async Task<IActionResult> Matrix()
        {
            var controls = await _controlService.GetAllAsync();
            var statistics = await _controlService.GetStatisticsAsync();

            ViewBag.Statistics = statistics;
            return View(controls);
        }

        // GET: Control/ByRisk/5
        public async Task<IActionResult> ByRisk(Guid riskId)
        {
            var controls = await _controlService.GetByRiskIdAsync(riskId);
            var risk = await _riskService.GetByIdAsync(riskId);

            ViewBag.Risk = risk;
            return View(controls);
        }

        // GET: Controls/Assess
        public async Task<IActionResult> Assess(Guid? controlId = null)
        {
            if (controlId.HasValue)
            {
                var control = await _controlService.GetByIdAsync(controlId.Value);
                if (control == null)
                {
                    return NotFound();
                }
                ViewBag.ControlId = controlId.Value;
                ViewBag.ControlName = control.Name;
            }
            return View();
        }
    }
}