using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GrcMvc.Services.Interfaces;
using GrcMvc.Models.DTOs;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace GrcMvc.Controllers
{
    [Authorize]
    public class RiskController : Controller
    {
        private readonly IRiskService _riskService;
        private readonly ILogger<RiskController> _logger;

        public RiskController(IRiskService riskService, ILogger<RiskController> logger)
        {
            _riskService = riskService;
            _logger = logger;
        }

        // GET: Risk
        public async Task<IActionResult> Index()
        {
            var risks = await _riskService.GetAllAsync();
            return View(risks);
        }

        // GET: Risk/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            var risk = await _riskService.GetByIdAsync(id);
            if (risk == null)
            {
                return NotFound();
            }
            return View(risk);
        }

        // GET: Risk/Create
        public IActionResult Create()
        {
            return View(new CreateRiskDto());
        }

        // POST: Risk/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRiskDto createRiskDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var risk = await _riskService.CreateAsync(createRiskDto);
                    TempData["SuccessMessage"] = "Risk created successfully.";
                    return RedirectToAction(nameof(Details), new { id = risk.Id });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating risk");
                    ModelState.AddModelError(string.Empty, "An error occurred while creating the risk.");
                }
            }
            return View(createRiskDto);
        }

        // GET: Risk/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            var risk = await _riskService.GetByIdAsync(id);
            if (risk == null)
            {
                return NotFound();
            }

            var updateDto = new UpdateRiskDto
            {
                Id = risk.Id,
                Name = risk.Name,
                Description = risk.Description,
                Category = risk.Category,
                Probability = risk.Probability,
                Impact = risk.Impact,
                InherentRisk = risk.InherentRisk,
                ResidualRisk = risk.ResidualRisk,
                MitigationStrategy = risk.MitigationStrategy,
                Owner = risk.Owner,
                Status = risk.Status,
                DueDate = risk.DueDate
            };

            return View(updateDto);
        }

        // POST: Risk/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, UpdateRiskDto updateRiskDto)
        {
            if (id != updateRiskDto.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var risk = await _riskService.UpdateAsync(id, updateRiskDto);
                    TempData["SuccessMessage"] = "Risk updated successfully.";
                    return RedirectToAction(nameof(Details), new { id = risk.Id });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating risk");
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the risk.");
                }
            }
            return View(updateRiskDto);
        }

        // GET: Risk/Delete/5
        public async Task<IActionResult> Delete(Guid id)
        {
            var risk = await _riskService.GetByIdAsync(id);
            if (risk == null)
            {
                return NotFound();
            }
            return View(risk);
        }

        // POST: Risk/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                await _riskService.DeleteAsync(id);
                TempData["SuccessMessage"] = "Risk deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting risk");
                TempData["ErrorMessage"] = "An error occurred while deleting the risk.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Risk/Matrix
        public async Task<IActionResult> Matrix()
        {
            var risks = await _riskService.GetAllAsync();
            return View(risks);
        }

        // GET: Risk/Report
        public async Task<IActionResult> Report()
        {
            var risks = await _riskService.GetAllAsync();
            ViewBag.HighRiskCount = risks.Count(r => r.InherentRisk >= 15);
            ViewBag.MediumRiskCount = risks.Count(r => r.InherentRisk >= 8 && r.InherentRisk < 15);
            ViewBag.LowRiskCount = risks.Count(r => r.InherentRisk < 8);
            ViewBag.ActiveRiskCount = risks.Count(r => r.Status == "Active");
            ViewBag.MitigatedRiskCount = risks.Count(r => r.Status == "Mitigated");
            ViewBag.ClosedRiskCount = risks.Count(r => r.Status == "Closed");
            return View(risks);
        }
    }
}