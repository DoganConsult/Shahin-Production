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
    public class AssessmentController : Controller
    {
        private readonly IAssessmentService _assessmentService;
        private readonly IRiskService _riskService;
        private readonly IControlService _controlService;
        private readonly ILogger<AssessmentController> _logger;

        public AssessmentController(
            IAssessmentService assessmentService,
            IRiskService riskService,
            IControlService controlService,
            ILogger<AssessmentController> logger)
        {
            _assessmentService = assessmentService;
            _riskService = riskService;
            _controlService = controlService;
            _logger = logger;
        }

        // GET: Assessment
        public async Task<IActionResult> Index()
        {
            var assessments = await _assessmentService.GetAllAsync();
            return View(assessments);
        }

        // GET: Assessment/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            var assessment = await _assessmentService.GetByIdAsync(id);
            if (assessment == null)
            {
                return NotFound();
            }
            return View(assessment);
        }

        // GET: Assessment/Create
        public async Task<IActionResult> Create(Guid? riskId = null, Guid? controlId = null)
        {
            var model = new CreateAssessmentDto
            {
                RiskId = riskId,
                ControlId = controlId,
                StartDate = DateTime.Today,
                ScheduledDate = DateTime.Today.AddDays(7)
            };

            await PopulateViewBags(riskId, controlId);
            return View(model);
        }

        // POST: Assessment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAssessmentDto createAssessmentDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var assessment = await _assessmentService.CreateAsync(createAssessmentDto);
                    TempData["Success"] = "Assessment created successfully";
                    return RedirectToAction(nameof(Details), new { id = assessment.Id });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating assessment");
                    ModelState.AddModelError("", "Error creating assessment. Please try again.");
                }
            }

            await PopulateViewBags(createAssessmentDto.RiskId, createAssessmentDto.ControlId);
            return View(createAssessmentDto);
        }

        // GET: Assessment/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            var assessment = await _assessmentService.GetByIdAsync(id);
            if (assessment == null)
            {
                return NotFound();
            }

            var updateDto = new UpdateAssessmentDto
            {
                AssessmentNumber = assessment.AssessmentNumber,
                Type = assessment.Type,
                Name = assessment.Name,
                Description = assessment.Description,
                StartDate = assessment.StartDate,
                AssignedTo = assessment.AssignedTo,
                RiskId = assessment.RiskId,
                ControlId = assessment.ControlId,
                EndDate = assessment.EndDate,
                Status = assessment.Status,
                ReviewedBy = assessment.ReviewedBy,
                ComplianceScore = assessment.ComplianceScore,
                Results = assessment.Results,
                Findings = assessment.Findings,
                Recommendations = assessment.Recommendations
            };

            await PopulateViewBags(assessment.RiskId, assessment.ControlId);
            return View(updateDto);
        }

        // POST: Assessment/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, UpdateAssessmentDto updateAssessmentDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var assessment = await _assessmentService.UpdateAsync(id, updateAssessmentDto);
                    if (assessment == null)
                    {
                        return NotFound();
                    }
                    TempData["Success"] = "Assessment updated successfully";
                    return RedirectToAction(nameof(Details), new { id = assessment.Id });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating assessment with ID {AssessmentId}", id);
                    ModelState.AddModelError("", "Error updating assessment. Please try again.");
                }
            }

            await PopulateViewBags(updateAssessmentDto.RiskId, updateAssessmentDto.ControlId);
            return View(updateAssessmentDto);
        }

        private async Task PopulateViewBags(Guid? selectedRiskId = null, Guid? selectedControlId = null)
        {
            var risks = await _riskService.GetAllAsync();
            var controls = await _controlService.GetAllAsync();

            ViewBag.RiskId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(risks, "Id", "Name", selectedRiskId);
            ViewBag.ControlId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(controls, "Id", "Name", selectedControlId);
        }

        // GET: Assessment/Delete/5
        public async Task<IActionResult> Delete(Guid id)
        {
            var assessment = await _assessmentService.GetByIdAsync(id);
            if (assessment == null)
            {
                return NotFound();
            }
            return View(assessment);
        }

        // POST: Assessment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                await _assessmentService.DeleteAsync(id);
                TempData["Success"] = "Assessment deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting assessment with ID {AssessmentId}", id);
                TempData["Error"] = "Error deleting assessment. Please try again.";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        // GET: Assessment/Statistics
        public async Task<IActionResult> Statistics()
        {
            try
            {
                var statistics = await _assessmentService.GetStatisticsAsync();
                return View(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting assessment statistics");
                TempData["Error"] = "Error loading statistics. Please try again.";
                return View(new AssessmentStatisticsDto());
            }
        }

        // GET: Assessment/ByControl/5
        public async Task<IActionResult> ByControl(Guid controlId)
        {
            try
            {
                var assessments = await _assessmentService.GetByControlIdAsync(controlId);
                ViewBag.ControlId = controlId;
                return View(assessments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting assessments for control ID {ControlId}", controlId);
                TempData["Error"] = "Error loading assessments. Please try again.";
                return View(new List<AssessmentDto>());
            }
        }

        // GET: Assessment/Upcoming
        public async Task<IActionResult> Upcoming()
        {
            try
            {
                var assessments = await _assessmentService.GetUpcomingAssessmentsAsync(30);
                return View(assessments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting upcoming assessments");
                TempData["Error"] = "Error loading upcoming assessments. Please try again.";
                return View(new List<AssessmentDto>());
            }
        }
    }
}
