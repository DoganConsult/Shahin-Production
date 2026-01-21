using GrcMvc.Models.DTOs;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Microsoft.Extensions.Localization;
using GrcMvc.Resources;
namespace GrcMvc.Controllers.Api
{
    /// <summary>
    /// Assessment Execution API Controller
    /// Handles requirement status updates, scoring, evidence uploads, and notes
    /// </summary>
    [Route("api/assessment-execution")]
    [ApiController]
    [Authorize]
    public class AssessmentExecutionController : ControllerBase
    {
        private readonly IAssessmentExecutionService _executionService;
        private readonly ILogger<AssessmentExecutionController> _logger;

        public AssessmentExecutionController(
            IAssessmentExecutionService executionService,
            ILogger<AssessmentExecutionController> logger)
        {
            _executionService = executionService;
            _logger = logger;
        }

        /// <summary>
        /// Get complete assessment execution data
        /// GET /api/assessment-execution/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetExecutionData(Guid id, [FromQuery] string lang = "en")
        {
            try
            {
                var viewModel = await _executionService.GetExecutionViewModelAsync(id, lang);
                if (viewModel == null)
                    return NotFound(new { error = $"Assessment {id} not found" });

                return Ok(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting assessment execution data for {AssessmentId}", id);
                return StatusCode(500, new { error = "An error occurred while loading assessment data" });
            }
        }

        /// <summary>
        /// Get progress statistics for an assessment
        /// GET /api/assessment-execution/{id}/progress
        /// </summary>
        [HttpGet("{id}/progress")]
        public async Task<IActionResult> GetProgress(Guid id)
        {
            try
            {
                var progress = await _executionService.CalculateProgressAsync(id);
                return Ok(progress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating progress for assessment {AssessmentId}", id);
                return StatusCode(500, new { error = "An error occurred while calculating progress" });
            }
        }

        /// <summary>
        /// Get progress for a specific domain
        /// GET /api/assessment-execution/{id}/progress/{domain}
        /// </summary>
        [HttpGet("{id}/progress/{domain}")]
        public async Task<IActionResult> GetDomainProgress(Guid id, string domain)
        {
            try
            {
                var progress = await _executionService.CalculateDomainProgressAsync(id, domain);
                if (progress == null)
                    return NotFound(new { error = $"Domain '{domain}' not found in assessment" });

                return Ok(progress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating domain progress for {Domain} in assessment {AssessmentId}", domain, id);
                return StatusCode(500, new { error = "An error occurred while calculating domain progress" });
            }
        }

        /// <summary>
        /// Update requirement status
        /// PUT /api/assessment-execution/requirement/{id}/status
        /// </summary>
        [HttpPut("requirement/{id}/status")]
        public async Task<IActionResult> UpdateRequirementStatus(Guid id, [FromBody] UpdateRequirementStatusRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userName = User.Identity?.Name ?? "System";
                var result = await _executionService.UpdateStatusAsync(id, request.Status, userName);
                if (result == null)
                    return NotFound(new { error = $"Requirement {id} not found" });
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for requirement {RequirementId}", id);
                return StatusCode(500, new { error = "An error occurred while updating status" });
            }
        }

        /// <summary>
        /// Update requirement score
        /// PUT /api/assessment-execution/requirement/{id}/score
        /// </summary>
        [HttpPut("requirement/{id}/score")]
        public async Task<IActionResult> UpdateRequirementScore(Guid id, [FromBody] UpdateRequirementScoreRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userName = User.Identity?.Name ?? "System";
                var result = await _executionService.UpdateScoreAsync(id, request.Score, request.ScoreRationale, userName);
                if (result == null)
                    return NotFound(new { error = $"Requirement {id} not found" });
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating score for requirement {RequirementId}", id);
                return StatusCode(500, new { error = "An error occurred while updating score" });
            }
        }

        /// <summary>
        /// Get requirement details
        /// GET /api/assessment-execution/requirement/{id}
        /// </summary>
        [HttpGet("requirement/{id}")]
        public async Task<IActionResult> GetRequirement(Guid id, [FromQuery] string lang = "en")
        {
            try
            {
                var requirement = await _executionService.GetRequirementCardAsync(id, lang);
                if (requirement == null)
                    return NotFound(new { error = $"Requirement {id} not found" });

                return Ok(requirement);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting requirement {RequirementId}", id);
                return StatusCode(500, new { error = "An error occurred while loading requirement" });
            }
        }

        /// <summary>
        /// Add note to a requirement
        /// POST /api/assessment-execution/requirement/{id}/notes
        /// </summary>
        [HttpPost("requirement/{id}/notes")]
        public async Task<IActionResult> AddNote(Guid id, [FromBody] AddRequirementNoteRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                request.RequirementId = id;
                var userName = User.Identity?.Name ?? "System";
                var note = await _executionService.AddNoteAsync(request, userName);
                if (note == null)
                    return NotFound(new { error = $"Requirement {id} not found" });
                return Ok(note);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding note to requirement {RequirementId}", id);
                return StatusCode(500, new { error = "An error occurred while adding note" });
            }
        }

        /// <summary>
        /// Get notes for a requirement
        /// GET /api/assessment-execution/requirement/{id}/notes
        /// </summary>
        [HttpGet("requirement/{id}/notes")]
        public async Task<IActionResult> GetNotes(Guid id)
        {
            try
            {
                var notes = await _executionService.GetNotesHistoryAsync(id);
                return Ok(notes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notes for requirement {RequirementId}", id);
                return StatusCode(500, new { error = "An error occurred while loading notes" });
            }
        }

        /// <summary>
        /// Delete a note
        /// DELETE /api/assessment-execution/notes/{id}
        /// </summary>
        [HttpDelete("notes/{id}")]
        public async Task<IActionResult> DeleteNote(Guid id)
        {
            try
            {
                var userName = User.Identity?.Name ?? "System";
                await _executionService.DeleteNoteAsync(id, userName);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting note {NoteId}", id);
                return StatusCode(500, new { error = "An error occurred while deleting note" });
            }
        }

        /// <summary>
        /// Upload evidence to a requirement
        /// POST /api/assessment-execution/requirement/{id}/evidence
        /// </summary>
        [HttpPost("requirement/{id}/evidence")]
        public async Task<IActionResult> UploadEvidence(Guid id, [FromForm] IFormFile file, [FromForm] string title, [FromForm] string? description)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "No file provided" });

            if (string.IsNullOrWhiteSpace(title))
                return BadRequest(new { error = "Title is required" });

            try
            {
                var userName = User.Identity?.Name ?? "System";
                var evidence = await _executionService.AttachEvidenceAsync(id, file, title, description ?? string.Empty, userName);
                if (evidence == null)
                    return NotFound(new { error = $"Requirement {id} not found" });

                return Ok(new
                {
                    success = true,
                    evidenceId = evidence.Id,
                    evidenceNumber = evidence.EvidenceNumber,
                    fileName = evidence.FileName,
                    status = evidence.VerificationStatus
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = "An error occurred processing your request." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading evidence to requirement {RequirementId}", id);
                return StatusCode(500, new { error = "An error occurred while uploading evidence" });
            }
        }

        /// <summary>
        /// Get evidence for a requirement
        /// GET /api/assessment-execution/requirement/{id}/evidence
        /// </summary>
        [HttpGet("requirement/{id}/evidence")]
        public async Task<IActionResult> GetRequirementEvidence(Guid id)
        {
            try
            {
                var evidences = await _executionService.GetRequirementEvidenceAsync(id);
                return Ok(evidences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting evidence for requirement {RequirementId}", id);
                return StatusCode(500, new { error = "An error occurred while loading evidence" });
            }
        }

        /// <summary>
        /// Get requirements by domain
        /// GET /api/assessment-execution/{id}/domain/{domain}
        /// </summary>
        [HttpGet("{id}/domain/{domain}")]
        public async Task<IActionResult> GetRequirementsByDomain(Guid id, string domain, [FromQuery] string lang = "en")
        {
            try
            {
                var requirements = await _executionService.GetRequirementsByDomainAsync(id, domain, lang);
                return Ok(requirements);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting requirements for domain {Domain} in assessment {AssessmentId}", domain, id);
                return StatusCode(500, new { error = "An error occurred while loading requirements" });
            }
        }

        /// <summary>
        /// Export assessment data
        /// GET /api/assessment-execution/{id}/export
        /// HIGH FIX: Added missing export endpoint that UI calls
        /// </summary>
        [HttpGet("{id}/export")]
        public async Task<IActionResult> ExportAssessment(Guid id, [FromQuery] string format = "xlsx")
        {
            try
            {
                var viewModel = await _executionService.GetExecutionViewModelAsync(id, "en");
                if (viewModel == null)
                    return NotFound(new { error = $"Assessment {id} not found" });

                // For now, return JSON data - can be enhanced with actual Excel/PDF export
                var exportData = new
                {
                    Assessment = new
                    {
                        viewModel.Id,
                        viewModel.Name,
                        viewModel.FrameworkCode,
                        viewModel.FrameworkName,
                        viewModel.Status,
                        viewModel.OverallProgress,
                        viewModel.OverallScore,
                        viewModel.TotalRequirements,
                        viewModel.CompletedRequirements,
                        ExportedAt = DateTime.UtcNow
                    },
                    Domains = viewModel.Domains,
                    Requirements = viewModel.Requirements?.Select(r => new
                    {
                        r.Id,
                        r.ControlNumber,
                        r.ControlTitle,
                        r.Domain,
                        r.Status,
                        r.Score,
                        r.ScoreRationale,
                        r.EvidenceCount,
                        r.NotesCount
                    })
                };

                // Set content disposition for download
                var fileName = $"assessment-{viewModel.Name?.Replace(" ", "-")}-{DateTime.UtcNow:yyyyMMdd}.json";
                Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{fileName}\"");

                return Ok(exportData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting assessment {AssessmentId}", id);
                return StatusCode(500, new { error = "An error occurred while exporting assessment" });
            }
        }
    }
}
