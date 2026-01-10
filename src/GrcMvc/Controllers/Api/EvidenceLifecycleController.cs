using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Microsoft.Extensions.Localization;
using GrcMvc.Resources;
namespace GrcMvc.Controllers.Api
{
    /// <summary>
    /// PHASE 8: Evidence Lifecycle API Controller
    /// Handles evidence submission, review, scoring, and approval workflow
    /// </summary>
    [Route("api/evidence")]
    [ApiController]
    [IgnoreAntiforgeryToken]
    public class EvidenceLifecycleController : ControllerBase
    {
        private readonly IEvidenceLifecycleService _evidenceService;
        private readonly ILogger<EvidenceLifecycleController> _logger;

        public EvidenceLifecycleController(
            IEvidenceLifecycleService evidenceService,
            ILogger<EvidenceLifecycleController> logger)
        {
            _evidenceService = evidenceService;
            _logger = logger;
        }

        #region Submission

        /// <summary>
        /// Submit evidence for a requirement
        /// POST /api/evidence
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SubmitEvidence([FromBody] SubmitEvidenceRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var submittedBy = User.Identity?.Name ?? "System";
                var evidence = await _evidenceService.SubmitEvidenceAsync(
                    request.TenantId,
                    request.AssessmentRequirementId,
                    request.EvidenceTypeCode,
                    request.Title,
                    request.Description,
                    request.FilePath,
                    request.FileName,
                    request.FileSize,
                    request.MimeType,
                    submittedBy);

                return Ok(new
                {
                    success = true,
                    evidenceId = evidence.Id,
                    evidenceNumber = evidence.EvidenceNumber,
                    status = evidence.VerificationStatus
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting evidence");
                return StatusCode(500, new { error = "An error occurred" });
            }
        }

        /// <summary>
        /// Get evidence by ID
        /// GET /api/evidence/{id}
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetEvidence(Guid id)
        {
            try
            {
                var evidence = await _evidenceService.GetEvidenceAsync(id);
                if (evidence == null)
                    return NotFound(new { error = "Evidence not found" });

                return Ok(new
                {
                    evidence.Id,
                    evidence.EvidenceNumber,
                    evidence.Title,
                    evidence.Description,
                    evidence.Type,
                    evidence.FileName,
                    evidence.FileSize,
                    evidence.MimeType,
                    evidence.CollectionDate,
                    evidence.CollectedBy,
                    evidence.VerificationStatus,
                    evidence.VerifiedBy,
                    evidence.VerificationDate,
                    evidence.Comments,
                    evidence.AssessmentId,
                    evidence.ControlId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting evidence");
                return StatusCode(500, new { error = "An error occurred" });
            }
        }

        /// <summary>
        /// Get evidence by requirement
        /// GET /api/evidence/by-requirement/{requirementId}
        /// </summary>
        [HttpGet("by-requirement/{requirementId}")]
        [Authorize]
        public async Task<IActionResult> GetByRequirement(Guid requirementId)
        {
            try
            {
                var evidences = await _evidenceService.GetEvidenceByRequirementAsync(requirementId);

                return Ok(new
                {
                    total = evidences.Count,
                    evidences = evidences.Select(e => new
                    {
                        e.Id,
                        e.EvidenceNumber,
                        e.Title,
                        e.Type,
                        e.FileName,
                        e.VerificationStatus,
                        e.CollectionDate
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting evidence by requirement");
                return StatusCode(500, new { error = "An error occurred" });
            }
        }

        /// <summary>
        /// Get evidence by assessment
        /// GET /api/evidence/by-assessment/{assessmentId}
        /// </summary>
        [HttpGet("by-assessment/{assessmentId}")]
        [Authorize]
        public async Task<IActionResult> GetByAssessment(Guid assessmentId)
        {
            try
            {
                var evidences = await _evidenceService.GetEvidenceByAssessmentAsync(assessmentId);

                return Ok(new
                {
                    total = evidences.Count,
                    evidences = evidences.Select(e => new
                    {
                        e.Id,
                        e.EvidenceNumber,
                        e.Title,
                        e.Type,
                        e.FileName,
                        e.VerificationStatus,
                        e.CollectionDate,
                        e.ControlId
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting evidence by assessment");
                return StatusCode(500, new { error = "An error occurred" });
            }
        }

        #endregion

        #region Review Workflow

        /// <summary>
        /// Submit evidence for review
        /// POST /api/evidence/{id}/submit-for-review
        /// </summary>
        [HttpPost("{id}/submit-for-review")]
        [Authorize]
        public async Task<IActionResult> SubmitForReview(Guid id)
        {
            try
            {
                var submittedBy = User.Identity?.Name ?? "System";
                var evidence = await _evidenceService.SubmitForReviewAsync(id, submittedBy);

                return Ok(new
                {
                    success = true,
                    evidenceId = evidence.Id,
                    status = evidence.VerificationStatus
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting for review");
                return StatusCode(500, new { error = "An error occurred" });
            }
        }

        /// <summary>
        /// Start review workflow
        /// POST /api/evidence/{id}/start-review
        /// </summary>
        [HttpPost("{id}/start-review")]
        [Authorize]
        public async Task<IActionResult> StartReview(Guid id)
        {
            try
            {
                var startedBy = User.Identity?.Name ?? "System";
                var workflow = await _evidenceService.StartReviewWorkflowAsync(id, startedBy);

                return Ok(new
                {
                    success = true,
                    workflowInstanceId = workflow.Id,
                    workflowNumber = workflow.InstanceNumber
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting review workflow");
                return StatusCode(500, new { error = "An error occurred" });
            }
        }

        /// <summary>
        /// Request changes on evidence
        /// POST /api/evidence/{id}/request-changes
        /// </summary>
        [HttpPost("{id}/request-changes")]
        [Authorize]
        public async Task<IActionResult> RequestChanges(Guid id, [FromBody] CommentsRequest request)
        {
            try
            {
                var requestedBy = User.Identity?.Name ?? "System";
                var evidence = await _evidenceService.RequestChangesAsync(id, request.Comments, requestedBy);

                return Ok(new
                {
                    success = true,
                    evidenceId = evidence.Id,
                    status = evidence.VerificationStatus
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting changes");
                return StatusCode(500, new { error = "An error occurred" });
            }
        }

        /// <summary>
        /// Resubmit evidence after changes
        /// POST /api/evidence/{id}/resubmit
        /// </summary>
        [HttpPost("{id}/resubmit")]
        [Authorize]
        public async Task<IActionResult> Resubmit(Guid id, [FromBody] CommentsRequest request)
        {
            try
            {
                var resubmittedBy = User.Identity?.Name ?? "System";
                var evidence = await _evidenceService.ResubmitAsync(id, request.Comments, resubmittedBy);

                return Ok(new
                {
                    success = true,
                    evidenceId = evidence.Id,
                    status = evidence.VerificationStatus
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resubmitting evidence");
                return StatusCode(500, new { error = "An error occurred" });
            }
        }

        #endregion

        #region Scoring

        /// <summary>
        /// Score evidence
        /// POST /api/evidence/{id}/score
        /// </summary>
        [HttpPost("{id}/score")]
        [Authorize]
        public async Task<IActionResult> ScoreEvidence(Guid id, [FromBody] ScoreEvidenceRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var scoredBy = User.Identity?.Name ?? "System";
                var score = await _evidenceService.ScoreEvidenceAsync(
                    id,
                    request.Score,
                    request.ScoringCriteria,
                    request.Comments,
                    scoredBy);

                return Ok(new
                {
                    success = true,
                    scoreId = score.Id,
                    score = score.Score,
                    scoredAt = score.ScoredAt
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scoring evidence");
                return StatusCode(500, new { error = "An error occurred" });
            }
        }

        /// <summary>
        /// Get evidence score
        /// GET /api/evidence/{id}/score
        /// </summary>
        [HttpGet("{id}/score")]
        [Authorize]
        public async Task<IActionResult> GetScore(Guid id)
        {
            try
            {
                var score = await _evidenceService.GetEvidenceScoreAsync(id);
                if (score == null)
                    return NotFound(new { error = "No score found for this evidence" });

                return Ok(new
                {
                    score.Id,
                    score.Score,
                    score.ScoringCriteria,
                    score.Comments,
                    score.ScoredBy,
                    score.ScoredAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting evidence score");
                return StatusCode(500, new { error = "An error occurred" });
            }
        }

        /// <summary>
        /// Get scoring history
        /// GET /api/evidence/{id}/score/history
        /// </summary>
        [HttpGet("{id}/score/history")]
        [Authorize]
        public async Task<IActionResult> GetScoringHistory(Guid id)
        {
            try
            {
                var history = await _evidenceService.GetScoringHistoryAsync(id);

                return Ok(new
                {
                    total = history.Count,
                    scores = history.Select(s => new
                    {
                        s.Id,
                        s.Score,
                        s.ScoringCriteria,
                        s.Comments,
                        s.ScoredBy,
                        s.ScoredAt,
                        s.IsFinal
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting scoring history");
                return StatusCode(500, new { error = "An error occurred" });
            }
        }

        #endregion

        #region Approval

        /// <summary>
        /// Approve evidence
        /// POST /api/evidence/{id}/approve
        /// </summary>
        [HttpPost("{id}/approve")]
        [Authorize]
        public async Task<IActionResult> Approve(Guid id, [FromBody] CommentsRequest? request)
        {
            try
            {
                var approvedBy = User.Identity?.Name ?? "System";
                var evidence = await _evidenceService.ApproveEvidenceAsync(
                    id,
                    request?.Comments ?? "",
                    approvedBy);

                return Ok(new
                {
                    success = true,
                    evidenceId = evidence.Id,
                    status = evidence.VerificationStatus,
                    verifiedBy = evidence.VerifiedBy,
                    verificationDate = evidence.VerificationDate
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving evidence");
                return StatusCode(500, new { error = "An error occurred" });
            }
        }

        /// <summary>
        /// Reject evidence
        /// POST /api/evidence/{id}/reject
        /// </summary>
        [HttpPost("{id}/reject")]
        [Authorize]
        public async Task<IActionResult> Reject(Guid id, [FromBody] RejectRequest request)
        {
            if (string.IsNullOrEmpty(request.Reason))
                return BadRequest(new { error = "Rejection reason is required" });

            try
            {
                var rejectedBy = User.Identity?.Name ?? "System";
                var evidence = await _evidenceService.RejectEvidenceAsync(id, request.Reason, rejectedBy);

                return Ok(new
                {
                    success = true,
                    evidenceId = evidence.Id,
                    status = evidence.VerificationStatus
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting evidence");
                return StatusCode(500, new { error = "An error occurred" });
            }
        }

        #endregion

        #region Statistics

        /// <summary>
        /// Get evidence statistics for tenant
        /// GET /api/evidence/statistics/{tenantId}
        /// </summary>
        [HttpGet("statistics/{tenantId}")]
        [Authorize]
        public async Task<IActionResult> GetStatistics(Guid tenantId)
        {
            try
            {
                var stats = await _evidenceService.GetStatisticsAsync(tenantId);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting evidence statistics");
                return StatusCode(500, new { error = "An error occurred" });
            }
        }

        /// <summary>
        /// Calculate assessment score
        /// POST /api/evidence/calculate-assessment-score/{assessmentId}
        /// </summary>
        [HttpPost("calculate-assessment-score/{assessmentId}")]
        [Authorize]
        public async Task<IActionResult> CalculateAssessmentScore(Guid assessmentId)
        {
            try
            {
                var score = await _evidenceService.CalculateAssessmentScoreAsync(assessmentId);

                return Ok(new
                {
                    assessmentId,
                    score,
                    calculatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating assessment score");
                return StatusCode(500, new { error = "An error occurred" });
            }
        }

        #endregion
    }

    #region Request DTOs

    public class SubmitEvidenceRequest
    {
        public Guid TenantId { get; set; }
        public Guid AssessmentRequirementId { get; set; }
        public string EvidenceTypeCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string MimeType { get; set; } = string.Empty;
    }

    public class ScoreEvidenceRequest
    {
        public int Score { get; set; }
        public string ScoringCriteria { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
    }

    public class CommentsRequest
    {
        public string Comments { get; set; } = string.Empty;
    }

    public class RejectRequest
    {
        public string Reason { get; set; } = string.Empty;
    }

    #endregion
}
