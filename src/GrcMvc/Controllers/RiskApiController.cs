using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrcMvc.Configuration;
using GrcMvc.Models.DTOs;
using GrcMvc.Models;
using GrcMvc.Services.Interfaces;
using GrcMvc.Exceptions;
using GrcMvc.Common;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using static GrcMvc.Services.Interfaces.IRiskService;

namespace GrcMvc.Controllers
{
    /// <summary>
    /// Risk API Controller
    /// Handles REST API requests for risk management, assessment, and mitigation
    /// Route: /api/risks
    /// </summary>
    [Route("api/risks")]
    [ApiController]
    [Authorize]
    public class RiskApiController : ControllerBase
    {
        private readonly IRiskService _riskService;
        private readonly IRiskWorkflowService _riskWorkflowService;

        public RiskApiController(IRiskService riskService, IRiskWorkflowService riskWorkflowService)
        {
            _riskService = riskService;
            _riskWorkflowService = riskWorkflowService;
        }

        /// <summary>
        /// Get all risks with pagination, sorting, filtering, and search
        /// Query params: ?page=1&size=10&sortBy=date&order=desc&level=high&q=searchterm
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetRisks(
            [FromQuery] int page = 1,
            [FromQuery] int size = 10,
            [FromQuery] string? sortBy = null,
            [FromQuery] string order = "asc",
            [FromQuery] string? level = null,
            [FromQuery] string? q = null)
        {
            try
            {
                var risksResult = await _riskService.GetAllAsync();

                if (risksResult.IsFailure)
                    return BadRequest(ApiResponse<object>.ErrorResponse(risksResult.Error));

                var risks = risksResult.Value;

                // Apply filtering
                var filtered = risks.ToList();
                if (!string.IsNullOrEmpty(level))
                    filtered = filtered.Where(r => r.Category == level).ToList();

                // Apply search
                if (!string.IsNullOrEmpty(q))
                    filtered = filtered.Where(r =>
                        r.Name?.Contains(q, StringComparison.OrdinalIgnoreCase) == true ||
                        r.Description?.Contains(q, StringComparison.OrdinalIgnoreCase) == true).ToList();

                // Apply sorting
                if (!string.IsNullOrEmpty(sortBy))
                    filtered = order.ToLower() == "desc"
                        ? filtered.OrderByDescending(r => r.GetType().GetProperty(sortBy)?.GetValue(r)).ToList()
                        : filtered.OrderBy(r => r.GetType().GetProperty(sortBy)?.GetValue(r)).ToList();

                // Apply pagination
                var totalItems = filtered.Count;
                var paginatedItems = filtered.Skip((page - 1) * size).Take(size).ToList();

                var response = new PaginatedResponse<object>
                {
                    Items = paginatedItems.Cast<object>().ToList(),
                    Page = page,
                    Size = size,
                    TotalItems = totalItems
                };

                return Ok(ApiResponse<PaginatedResponse<object>>.SuccessResponse(response, "Risks retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get risk by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRisk(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid risk ID"));

                var riskResult = await _riskService.GetByIdAsync(id);
                if (riskResult.IsFailure || riskResult.Value == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("Risk not found"));

                return Ok(ApiResponse<object>.SuccessResponse(riskResult.Value, "Risk retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Create new risk
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateRisk([FromBody] CreateRiskDto riskData)
        {
            try
            {
                if (riskData == null)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Risk data is required"));

                if (string.IsNullOrWhiteSpace(riskData.Name))
                    return BadRequest(ApiResponse<object>.ErrorResponse("Risk name is required"));

                var newRiskResult = await _riskService.CreateAsync(riskData);

                if (newRiskResult.IsFailure)
                    return BadRequest(ApiResponse<object>.ErrorResponse(newRiskResult.Error));

                return CreatedAtAction(nameof(GetRisk), new { id = newRiskResult.Value.Id },
                    ApiResponse<RiskDto>.SuccessResponse(newRiskResult.Value, "Risk created successfully"));
            }
            catch (ValidationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Update risk by ID
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRisk(Guid id, [FromBody] UpdateRiskDto riskData)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid risk ID"));

                if (riskData == null)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Risk data is required"));

                var updatedRiskResult = await _riskService.UpdateAsync(id, riskData);

                if (updatedRiskResult.IsFailure)
                    return BadRequest(ApiResponse<object>.ErrorResponse(updatedRiskResult.Error));

                return Ok(ApiResponse<RiskDto>.SuccessResponse(updatedRiskResult.Value, "Risk updated successfully"));
            }
            catch (EntityNotFoundException)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Risk not found"));
            }
            catch (ValidationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Delete risk by ID
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRisk(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid risk ID"));

                var deleteResult = await _riskService.DeleteAsync(id);
                if (deleteResult.IsFailure)
                    return BadRequest(ApiResponse<object>.ErrorResponse(deleteResult.Error));

                return Ok(ApiResponse<object>.SuccessResponse(null, "Risk deleted successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get high-risk items
        /// Returns risks with high severity level
        /// </summary>
        [HttpGet("high-risk")]
        public async Task<IActionResult> GetHighRisks()
        {
            try
            {
                var risksResult = await _riskService.GetAllAsync();

                if (risksResult.IsFailure)
                    return BadRequest(ApiResponse<object>.ErrorResponse(risksResult.Error));

                var risks = risksResult.Value;
                // Use centralized thresholds for high risks (Critical + High)
                var highRisks = risks.Where(r => 
                    (r.Probability * r.Impact) >= RiskScoringConfiguration.Thresholds.HighMin).ToList();

                return Ok(ApiResponse<object>.SuccessResponse(highRisks, "High-risk items retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get risk statistics
        /// Returns aggregate statistics about risks
        /// </summary>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetRiskStatistics()
        {
            try
            {
                var risksResult = await _riskService.GetAllAsync();

                if (risksResult.IsFailure)
                    return BadRequest(ApiResponse<object>.ErrorResponse(risksResult.Error));

                var risks = risksResult.Value;
                // Use centralized thresholds for risk statistics
                var stats = new
                {
                    totalRisks = risks.Count(),
                    criticalRisks = risks.Count(r => 
                        (r.Probability * r.Impact) >= RiskScoringConfiguration.Thresholds.CriticalMin),
                    highRisks = risks.Count(r => 
                        (r.Probability * r.Impact) >= RiskScoringConfiguration.Thresholds.HighMin && 
                        (r.Probability * r.Impact) < RiskScoringConfiguration.Thresholds.CriticalMin),
                    mediumRisks = risks.Count(r => 
                        (r.Probability * r.Impact) >= RiskScoringConfiguration.Thresholds.MediumMin && 
                        (r.Probability * r.Impact) < RiskScoringConfiguration.Thresholds.HighMin),
                    lowRisks = risks.Count(r => 
                        (r.Probability * r.Impact) < RiskScoringConfiguration.Thresholds.MediumMin),
                    mitigatedRisks = risks.Count(r => r.Status == "Mitigated"),
                    activeRisks = risks.Count(r => r.Status == "Active")
                };

                return Ok(ApiResponse<object>.SuccessResponse(stats, "Risk statistics retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Partially update risk
        /// Updates specific fields of a risk (partial update)
        /// </summary>
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchRisk(Guid id, [FromBody] PatchRiskDto patchData)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid risk ID"));

                if (patchData == null)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Patch data is required"));

                var riskResult = await _riskService.GetByIdAsync(id);
                if (riskResult.IsFailure || riskResult.Value == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("Risk not found"));

                var risk = riskResult.Value;
                // Apply patch - only update provided fields
                var updateDto = new UpdateRiskDto
                {
                    Id = id,
                    Name = patchData.Name ?? risk.Name,
                    Description = patchData.Description ?? risk.Description,
                    Category = patchData.Category ?? risk.Category,
                    Probability = patchData.Probability ?? risk.Probability,
                    Impact = patchData.Impact ?? risk.Impact,
                    InherentRisk = patchData.InherentRisk ?? risk.InherentRisk,
                    ResidualRisk = patchData.ResidualRisk ?? risk.ResidualRisk,
                    Status = patchData.Status ?? risk.Status,
                    Owner = patchData.Owner ?? risk.Owner,
                    DueDate = patchData.DueDate ?? risk.DueDate,
                    MitigationStrategy = patchData.MitigationStrategy ?? risk.MitigationStrategy,
                    DataClassification = patchData.DataClassification ?? risk.DataClassification
                };

                var patchedRiskResult = await _riskService.UpdateAsync(id, updateDto);
                if (patchedRiskResult.IsFailure)
                    return BadRequest(ApiResponse<object>.ErrorResponse(patchedRiskResult.Error ?? "Update failed"));
                return Ok(ApiResponse<RiskDto>.SuccessResponse(patchedRiskResult.Value!, "Risk updated successfully"));
            }
            catch (EntityNotFoundException)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Risk not found"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Bulk create risks
        /// </summary>
        [HttpPost("bulk")]
        public async Task<IActionResult> BulkCreateRisks([FromBody] List<CreateRiskDto> risks)
        {
            try
            {
                if (risks == null || risks.Count == 0)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Risks are required for bulk operation"));

                var createdRisks = new List<RiskDto>();
                var errors = new List<string>();

                foreach (var risk in risks)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(risk.Name))
                        {
                            errors.Add($"Risk at index {risks.IndexOf(risk)} has no name");
                            continue;
                        }
                        var createdResult = await _riskService.CreateAsync(risk);
                        if (createdResult.IsSuccess)
                            createdRisks.Add(createdResult.Value);
                        else
                            errors.Add($"Risk '{risk.Name}': {createdResult.Error}");
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Risk '{risk.Name}': {ex.Message}");
                    }
                }

                var result = new BulkOperationResult
                {
                    TotalItems = risks.Count,
                    SuccessfulItems = createdRisks.Count,
                    FailedItems = errors.Count,
                    CompletedAt = DateTime.Now,
                    Errors = errors
                };

                return Ok(ApiResponse<BulkOperationResult>.SuccessResponse(result, 
                    $"Bulk risk creation completed: {createdRisks.Count}/{risks.Count} successful"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        // ========================================
        // WORKFLOW ENDPOINTS
        // ========================================

        /// <summary>
        /// Accept a risk (acknowledge and monitor)
        /// </summary>
        [HttpPost("{id}/accept")]
        public async Task<IActionResult> AcceptRisk(Guid id, [FromBody] RiskWorkflowRequest? request)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid risk ID"));

                var acceptedBy = User.Identity?.Name ?? "System";
                var result = await _riskWorkflowService.AcceptAsync(id, acceptedBy, request?.Comments);

                return Ok(ApiResponse<object>.SuccessResponse(new { result.Id, result.Status }, "Risk accepted successfully"));
            }
            catch (WorkflowNotFoundException)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Risk not found"));
            }
            catch (InvalidStateTransitionException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse($"Invalid state transition: {ex.Message}"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Reject risk acceptance (requires mitigation)
        /// </summary>
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> RejectRisk(Guid id, [FromBody] RiskWorkflowRequest request)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid risk ID"));

                if (string.IsNullOrWhiteSpace(request?.Reason))
                    return BadRequest(ApiResponse<object>.ErrorResponse("Rejection reason is required"));

                var rejectedBy = User.Identity?.Name ?? "System";
                var result = await _riskWorkflowService.RejectAcceptanceAsync(id, rejectedBy, request.Reason);

                return Ok(ApiResponse<object>.SuccessResponse(new { result.Id, result.Status }, "Risk acceptance rejected"));
            }
            catch (WorkflowNotFoundException)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Risk not found"));
            }
            catch (InvalidStateTransitionException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse($"Invalid state transition: {ex.Message}"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Mark risk as mitigated
        /// </summary>
        [HttpPost("{id}/mitigate")]
        public async Task<IActionResult> MitigateRisk(Guid id, [FromBody] RiskWorkflowRequest request)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid risk ID"));

                if (string.IsNullOrWhiteSpace(request?.MitigationDetails))
                    return BadRequest(ApiResponse<object>.ErrorResponse("Mitigation details are required"));

                var mitigatedBy = User.Identity?.Name ?? "System";
                var result = await _riskWorkflowService.MarkMitigatedAsync(id, mitigatedBy, request.MitigationDetails);

                return Ok(ApiResponse<object>.SuccessResponse(new { result.Id, result.Status }, "Risk marked as mitigated"));
            }
            catch (WorkflowNotFoundException)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Risk not found"));
            }
            catch (InvalidStateTransitionException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse($"Invalid state transition: {ex.Message}"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Start monitoring a risk
        /// </summary>
        [HttpPost("{id}/monitor")]
        public async Task<IActionResult> StartMonitoring(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid risk ID"));

                var monitoredBy = User.Identity?.Name ?? "System";
                var result = await _riskWorkflowService.StartMonitoringAsync(id, monitoredBy);

                return Ok(ApiResponse<object>.SuccessResponse(new { result.Id, result.Status }, "Risk monitoring started"));
            }
            catch (WorkflowNotFoundException)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Risk not found"));
            }
            catch (InvalidStateTransitionException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse($"Invalid state transition: {ex.Message}"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Close a risk (final state)
        /// </summary>
        [HttpPost("{id}/close")]
        public async Task<IActionResult> CloseRisk(Guid id, [FromBody] RiskWorkflowRequest? request)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid risk ID"));

                var closedBy = User.Identity?.Name ?? "System";
                var result = await _riskWorkflowService.CloseAsync(id, closedBy, request?.Comments);

                return Ok(ApiResponse<object>.SuccessResponse(new { result.Id, result.Status }, "Risk closed successfully"));
            }
            catch (WorkflowNotFoundException)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Risk not found"));
            }
            catch (InvalidStateTransitionException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse($"Invalid state transition: {ex.Message}"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        // ========================================
        // HEAT MAP & ANALYTICS ENDPOINTS
        // ========================================

        /// <summary>
        /// Get risk heat map data for visualization
        /// </summary>
        [HttpGet("heatmap/{tenantId}")]
        public async Task<IActionResult> GetHeatMap(Guid tenantId)
        {
            try
            {
                if (tenantId == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid tenant ID"));

                var heatMapResult = await _riskService.GetHeatMapAsync(tenantId);
                if (heatMapResult.IsFailure)
                    return BadRequest(ApiResponse<object>.ErrorResponse(heatMapResult.Error));

                return Ok(ApiResponse<RiskHeatMapDto>.SuccessResponse(heatMapResult.Value, "Heat map data retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get risk posture summary for a tenant
        /// </summary>
        [HttpGet("posture/{tenantId}")]
        public async Task<IActionResult> GetRiskPosture(Guid tenantId)
        {
            try
            {
                if (tenantId == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid tenant ID"));

                var postureResult = await _riskService.GetRiskPostureAsync(tenantId);
                if (postureResult.IsFailure)
                    return BadRequest(ApiResponse<object>.ErrorResponse(postureResult.Error));

                return Ok(ApiResponse<RiskPostureSummaryDto>.SuccessResponse(postureResult.Value, "Risk posture retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get risk score history for trend analysis
        /// </summary>
        [HttpGet("{id}/history")]
        public async Task<IActionResult> GetScoreHistory(Guid id, [FromQuery] int months = 12)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid risk ID"));

                var historyResult = await _riskService.GetScoreHistoryAsync(id, months);
                if (historyResult.IsFailure)
                    return BadRequest(ApiResponse<object>.ErrorResponse(historyResult.Error));

                return Ok(ApiResponse<List<RiskScoreHistoryDto>>.SuccessResponse(historyResult.Value, "Score history retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Calculate risk score (inherent and residual)
        /// </summary>
        [HttpPost("{id}/calculate-score")]
        public async Task<IActionResult> CalculateRiskScore(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid risk ID"));

                var scoreResult = await _riskService.CalculateRiskScoreAsync(id);
                if (scoreResult.IsFailure)
                    return BadRequest(ApiResponse<object>.ErrorResponse(scoreResult.Error));

                return Ok(ApiResponse<RiskScoreResultDto>.SuccessResponse(scoreResult.Value, "Risk score calculated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        // ========================================
        // CONTROL LINKAGE ENDPOINTS
        // ========================================

        /// <summary>
        /// Link a control to mitigate a risk
        /// </summary>
        [HttpPost("{riskId}/controls/{controlId}")]
        public async Task<IActionResult> LinkControl(Guid riskId, Guid controlId, [FromBody] LinkControlRequest request)
        {
            try
            {
                if (riskId == Guid.Empty || controlId == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid risk or control ID"));

                var mappingResult = await _riskService.LinkControlAsync(riskId, controlId, request?.ExpectedEffectiveness ?? 50);
                if (mappingResult.IsFailure)
                    return BadRequest(ApiResponse<object>.ErrorResponse(mappingResult.Error));

                return Ok(ApiResponse<RiskControlMappingDto>.SuccessResponse(mappingResult.Value, "Control linked successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get all controls linked to a risk
        /// </summary>
        [HttpGet("{id}/controls")]
        public async Task<IActionResult> GetLinkedControls(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid risk ID"));

                var controlsResult = await _riskService.GetLinkedControlsAsync(id);
                if (controlsResult.IsFailure)
                    return BadRequest(ApiResponse<object>.ErrorResponse(controlsResult.Error));

                return Ok(ApiResponse<List<RiskControlMappingDto>>.SuccessResponse(controlsResult.Value, "Linked controls retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Calculate control effectiveness for a risk
        /// </summary>
        [HttpGet("{id}/control-effectiveness")]
        public async Task<IActionResult> GetControlEffectiveness(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid risk ID"));

                var effectivenessResult = await _riskService.CalculateControlEffectivenessAsync(id);
                if (effectivenessResult.IsFailure)
                    return BadRequest(ApiResponse<object>.ErrorResponse(effectivenessResult.Error));

                return Ok(ApiResponse<object>.SuccessResponse(new { effectiveness = effectivenessResult.Value }, "Control effectiveness calculated"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        // ========================================
        // ASSESSMENT LINKAGE ENDPOINTS
        // ========================================

        /// <summary>
        /// Link risk to an assessment
        /// </summary>
        [HttpPost("{riskId}/assessments/{assessmentId}")]
        public async Task<IActionResult> LinkToAssessment(Guid riskId, Guid assessmentId, [FromBody] LinkAssessmentRequest? request)
        {
            try
            {
                if (riskId == Guid.Empty || assessmentId == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid risk or assessment ID"));

                var riskResult = await _riskService.LinkToAssessmentAsync(riskId, assessmentId, request?.FindingReference);
                if (riskResult.IsFailure)
                    return BadRequest(ApiResponse<object>.ErrorResponse(riskResult.Error));

                return Ok(ApiResponse<RiskDto>.SuccessResponse(riskResult.Value, "Risk linked to assessment successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get risks from a specific assessment
        /// </summary>
        [HttpGet("by-assessment/{assessmentId}")]
        public async Task<IActionResult> GetRisksByAssessment(Guid assessmentId)
        {
            try
            {
                if (assessmentId == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid assessment ID"));

                var risksResult = await _riskService.GetRisksByAssessmentAsync(assessmentId);
                if (risksResult.IsFailure)
                    return BadRequest(ApiResponse<object>.ErrorResponse(risksResult.Error));

                return Ok(ApiResponse<List<RiskDto>>.SuccessResponse(risksResult.Value, "Risks retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Auto-generate risks from assessment findings
        /// </summary>
        [HttpPost("generate-from-assessment/{assessmentId}")]
        public async Task<IActionResult> GenerateFromAssessment(Guid assessmentId, [FromQuery] Guid tenantId)
        {
            try
            {
                if (assessmentId == Guid.Empty || tenantId == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid assessment or tenant ID"));

                var risksResult = await _riskService.GenerateRisksFromAssessmentAsync(assessmentId, tenantId);
                if (risksResult.IsFailure)
                    return BadRequest(ApiResponse<object>.ErrorResponse(risksResult.Error));

                return Ok(ApiResponse<List<RiskDto>>.SuccessResponse(risksResult.Value, $"{risksResult.Value.Count} risks generated from assessment"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// GET /api/risks/by-status/{status} - Filter risks by status
        /// </summary>
        [HttpGet("by-status/{status}")]
        public async Task<IActionResult> GetRisksByStatus(string status)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(status))
                    return BadRequest(ApiResponse<object>.ErrorResponse("Status parameter is required"));

                var result = await _riskService.GetByStatusAsync(status);
                if (!result.IsSuccess)
                    return BadRequest(ApiResponse<object>.ErrorResponse(result.Error ?? "Failed to retrieve risks"));

                return Ok(ApiResponse<IEnumerable<RiskDto>>.SuccessResponse(result.Value, $"Risks with status '{status}' retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// GET /api/risks/by-level/{level} - Filter risks by risk level (Critical, High, Medium, Low)
        /// </summary>
        [HttpGet("by-level/{level}")]
        public async Task<IActionResult> GetRisksByLevel(string level)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(level))
                    return BadRequest(ApiResponse<object>.ErrorResponse("Level parameter is required"));

                var result = await _riskService.GetAllAsync();
                if (!result.IsSuccess)
                    return BadRequest(ApiResponse<object>.ErrorResponse(result.Error ?? "Failed to retrieve risks"));

                // Filter by risk level based on score
                var filteredRisks = result.Value.Where(r =>
                {
                    var riskLevel = RiskScoringConfiguration.GetRiskLevel(r.InherentRisk);
                    return riskLevel.Equals(level, StringComparison.OrdinalIgnoreCase);
                }).ToList();

                return Ok(ApiResponse<IEnumerable<RiskDto>>.SuccessResponse(filteredRisks, $"Risks with level '{level}' retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// GET /api/risks/by-category/{categoryId} - Filter risks by category
        /// </summary>
        [HttpGet("by-category/{categoryId}")]
        public async Task<IActionResult> GetRisksByCategory(Guid categoryId)
        {
            try
            {
                if (categoryId == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid category ID"));

                var result = await _riskService.GetAllAsync();
                if (!result.IsSuccess)
                    return BadRequest(ApiResponse<object>.ErrorResponse(result.Error ?? "Failed to retrieve risks"));

                // Filter by category ID (assuming Category field stores category ID or name)
                var filteredRisks = result.Value.Where(r =>
                    r.Category != null && r.Category.Contains(categoryId.ToString(), StringComparison.OrdinalIgnoreCase)
                ).ToList();

                return Ok(ApiResponse<IEnumerable<RiskDto>>.SuccessResponse(filteredRisks, $"Risks in category '{categoryId}' retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// GET /api/risks/{id}/mitigation-plan - Get risk mitigation plan
        /// </summary>
        [HttpGet("{id}/mitigation-plan")]
        public async Task<IActionResult> GetMitigationPlan(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid risk ID"));

                var result = await _riskService.GetByIdAsync(id);
                if (!result.IsSuccess || result.Value == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("Risk not found"));

                var risk = result.Value;

                // Build mitigation plan response
                var mitigationPlan = new
                {
                    RiskId = risk.Id,
                    RiskName = risk.Name,
                    MitigationStrategy = risk.MitigationStrategy,
                    TreatmentPlan = risk.TreatmentPlan,
                    Owner = risk.Owner,
                    TargetDate = risk.DueDate,
                    Status = risk.Status,
                    InherentRisk = risk.InherentRisk,
                    ResidualRisk = risk.ResidualRisk,
                    LinkedControls = new List<object>(), // Would be populated from control mapping
                    MitigationActions = new List<object>
                    {
                        new { Action = risk.MitigationStrategy, Responsible = risk.Owner, Status = risk.Status, DueDate = risk.DueDate }
                    }
                };

                return Ok(ApiResponse<object>.SuccessResponse(mitigationPlan, "Mitigation plan retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }
    }

    /// <summary>
    /// Request DTO for linking controls
    /// </summary>
    public class LinkControlRequest
    {
        public int ExpectedEffectiveness { get; set; } = 50;
    }

    /// <summary>
    /// Request DTO for linking assessments
    /// </summary>
    public class LinkAssessmentRequest
    {
        public string? FindingReference { get; set; }
    }

    /// <summary>
    /// DTO for risk workflow operations
    /// </summary>
    public class RiskWorkflowRequest
    {
        public string? Comments { get; set; }
        public string? Reason { get; set; }
        public string? MitigationDetails { get; set; }
    }

    /// <summary>
    /// DTO for partial risk updates
    /// </summary>
    public class PatchRiskDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public int? Probability { get; set; }
        public int? Impact { get; set; }
        public int? InherentRisk { get; set; }
        public int? ResidualRisk { get; set; }
        public string? Status { get; set; }
        public string? Owner { get; set; }
        public DateTime? DueDate { get; set; }
        public string? MitigationStrategy { get; set; }
        public string? DataClassification { get; set; }
    }
}
