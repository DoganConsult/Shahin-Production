using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GrcMvc.Services.Interfaces;
using GrcMvc.Application.Permissions;
using GrcMvc.Authorization;

namespace GrcMvc.Controllers
{
    /// <summary>
    /// MVC Controller for Assessment Execution views
    /// </summary>
    [Authorize]
    [RequireTenant]
    public class AssessmentExecutionController : Controller
    {
        private readonly IAssessmentService _assessmentService;
        private readonly ILogger<AssessmentExecutionController> _logger;

        public AssessmentExecutionController(
            IAssessmentService assessmentService,
            ILogger<AssessmentExecutionController> logger)
        {
            _assessmentService = assessmentService;
            _logger = logger;
        }

        /// <summary>
        /// Main assessment execution page
        /// GET /AssessmentExecution/Execute/{id}
        /// </summary>
        [HttpGet]
        [Authorize(GrcPermissions.Assessments.View)]
        public async Task<IActionResult> Execute(Guid id)
        {
            var assessment = await _assessmentService.GetByIdAsync(id);
            if (assessment == null)
            {
                _logger.LogWarning("Assessment {AssessmentId} not found for execution", id);
                return NotFound();
            }

            ViewData["AssessmentId"] = id;
            ViewData["AssessmentName"] = assessment.Name;
            ViewData["Title"] = $"Execute Assessment - {assessment.Name}";

            return View();
        }
    }
}
