using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GrcMvc.Services.Interfaces;
using GrcMvc.Models.DTOs;
using GrcMvc.Application.Permissions;
using GrcMvc.Application.Policy;
using GrcMvc.Authorization;
using GrcMvc.Data;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Controllers
{
    [Authorize]
    [RequireTenant]
    public class AssessmentController : Controller
    {
        private readonly IAssessmentService _assessmentService;
        private readonly IRiskService _riskService;
        private readonly IControlService _controlService;
        private readonly ILogger<AssessmentController> _logger;
        private readonly IWorkspaceContextService? _workspaceContext;
        private readonly PolicyEnforcementHelper _policyHelper;
        private readonly IUnitOfWork _unitOfWork;

        public AssessmentController(
            IAssessmentService assessmentService,
            IRiskService riskService,
            IControlService controlService,
            ILogger<AssessmentController> logger,
            PolicyEnforcementHelper policyHelper,
            IUnitOfWork unitOfWork,
            IWorkspaceContextService? workspaceContext = null)
        {
            _assessmentService = assessmentService;
            _riskService = riskService;
            _controlService = controlService;
            _logger = logger;
            _policyHelper = policyHelper;
            _unitOfWork = unitOfWork;
            _workspaceContext = workspaceContext;
        }

        [Authorize(GrcPermissions.Assessments.View)]
        public async Task<IActionResult> Index()
        {
            var assessments = await _assessmentService.GetAllAsync();
            return View(assessments);
        }

        [Authorize(GrcPermissions.Assessments.View)]
        public async Task<IActionResult> Details(Guid id)
        {
            var assessment = await _assessmentService.GetByIdAsync(id);
            if (assessment == null) return NotFound();
            return View(assessment);
        }

        [Authorize(GrcPermissions.Assessments.Create)]
        public async Task<IActionResult> Create(Guid? riskId = null, Guid? controlId = null)
        {
            var model = new CreateAssessmentDto { RiskId = riskId, ControlId = controlId, StartDate = DateTime.Today, ScheduledDate = DateTime.Today.AddDays(7) };
            await PopulateViewBags(riskId, controlId);
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(GrcPermissions.Assessments.Create)]
        public async Task<IActionResult> Create(CreateAssessmentDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _policyHelper.EnforceCreateAsync("Assessment", dto, dataClassification: dto.DataClassification, owner: dto.Owner);
                    var assessment = await _assessmentService.CreateAsync(dto);
                    TempData["Success"] = "Assessment created successfully";
                    return RedirectToAction(nameof(Details), new { id = assessment.Id });
                }
                catch (PolicyViolationException pex)
                {
                    _logger.LogWarning(pex, "Policy violation creating assessment");
                    ModelState.AddModelError("", "A policy violation occurred. Please review the requirements.");
                    if (!string.IsNullOrEmpty(pex.RemediationHint)) ModelState.AddModelError("", $"Remediation: {pex.RemediationHint}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating assessment");
                    ModelState.AddModelError("", "Error creating assessment. Please try again.");
                }
            }
            await PopulateViewBags(dto.RiskId, dto.ControlId);
            return View(dto);
        }

        [Authorize(GrcPermissions.Assessments.Update)]
        public async Task<IActionResult> Edit(Guid id)
        {
            var assessment = await _assessmentService.GetByIdAsync(id);
            if (assessment == null) return NotFound();

            var updateDto = new UpdateAssessmentDto
            {
                Id = assessment.Id,
                Name = assessment.Name,
                Description = assessment.Description,
                AssessmentType = assessment.AssessmentType,
                RiskId = assessment.RiskId,
                ControlId = assessment.ControlId,
                Status = assessment.Status,
                StartDate = assessment.StartDate,
                ScheduledDate = assessment.ScheduledDate,
                CompletedDate = assessment.CompletedDate,
                AssessorId = assessment.AssessorId,
                Notes = assessment.Notes,
                DataClassification = assessment.DataClassification,
                Owner = assessment.Owner
            };

            await PopulateViewBags(assessment.RiskId, assessment.ControlId);
            return View(updateDto);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(GrcPermissions.Assessments.Update)]
        public async Task<IActionResult> Edit(Guid id, UpdateAssessmentDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _policyHelper.EnforceUpdateAsync("Assessment", dto, dataClassification: dto.DataClassification, owner: dto.Owner);
                    var assessment = await _assessmentService.UpdateAsync(id, dto);
                    if (assessment == null) return NotFound();
                    TempData["Success"] = "Assessment updated successfully";
                    return RedirectToAction(nameof(Details), new { id = assessment.Id });
                }
                catch (PolicyViolationException pex)
                {
                    _logger.LogWarning(pex, "Policy violation updating assessment {AssessmentId}", id);
                    ModelState.AddModelError("", "A policy violation occurred. Please review the requirements.");
                    if (!string.IsNullOrEmpty(pex.RemediationHint)) ModelState.AddModelError("", $"Remediation: {pex.RemediationHint}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating assessment {AssessmentId}", id);
                    ModelState.AddModelError("", "Error updating assessment. Please try again.");
                }
            }
            await PopulateViewBags(dto.RiskId, dto.ControlId);
            return View(dto);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(GrcPermissions.Assessments.Submit)]
        public async Task<IActionResult> Submit(Guid id)
        {
            try
            {
                var assessment = await _assessmentService.GetByIdAsync(id);
                if (assessment == null) return NotFound();

                // Get entity for policy enforcement (DTO may not have all properties)
                var assessmentEntity = await _unitOfWork.Assessments.GetByIdAsync(id);
                if (assessmentEntity == null) return NotFound();

                await _policyHelper.EnforceSubmitAsync("Assessment", assessmentEntity, 
                    dataClassification: assessmentEntity.DataClassification, 
                    owner: assessmentEntity.Owner);
                
                await _assessmentService.SubmitAsync(id);
                TempData["Success"] = "Assessment submitted successfully";
            }
            catch (PolicyViolationException pex)
            {
                _logger.LogWarning(pex, "Policy violation submitting assessment {AssessmentId}", id);
                TempData["Error"] = "A policy violation occurred. Please review the requirements.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting assessment {AssessmentId}", id);
                TempData["Error"] = "Error submitting assessment. Please try again.";
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(GrcPermissions.Assessments.Approve)]
        public async Task<IActionResult> Approve(Guid id)
        {
            try
            {
                var assessment = await _assessmentService.GetByIdAsync(id);
                if (assessment == null) return NotFound();

                // Get entity for policy enforcement (DTO may not have all properties)
                var assessmentEntity = await _unitOfWork.Assessments.GetByIdAsync(id);
                if (assessmentEntity == null) return NotFound();

                await _policyHelper.EnforceApproveAsync("Assessment", assessmentEntity, 
                    dataClassification: assessmentEntity.DataClassification, 
                    owner: assessmentEntity.Owner);
                
                await _assessmentService.ApproveAsync(id);
                TempData["Success"] = "Assessment approved successfully";
            }
            catch (PolicyViolationException pex)
            {
                _logger.LogWarning(pex, "Policy violation approving assessment {AssessmentId}", id);
                TempData["Error"] = "A policy violation occurred. Please review the requirements.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving assessment {AssessmentId}", id);
                TempData["Error"] = "Error approving assessment. Please try again.";
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        private async Task PopulateViewBags(Guid? riskId, Guid? controlId)
        {
            if (riskId.HasValue)
            {
                var riskResult = await _riskService.GetByIdAsync(riskId.Value);
                ViewBag.RiskName = riskResult.IsSuccess ? riskResult.Value?.Name : null;
            }
            if (controlId.HasValue) ViewBag.ControlName = (await _controlService.GetByIdAsync(controlId.Value))?.Name;
            var risksResult = await _riskService.GetAllAsync();
            ViewBag.Risks = risksResult.IsSuccess ? risksResult.Value : Enumerable.Empty<GrcMvc.Models.DTOs.RiskDto>();
            ViewBag.Controls = await _controlService.GetAllAsync();
        }
    }
}
