using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrcMvc.Configuration;
using GrcMvc.Models.DTOs;
using GrcMvc.Models;
using GrcMvc.Services.Interfaces;
using GrcMvc.Exceptions;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

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
                var risks = await _riskService.GetAllAsync();

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

                var risk = await _riskService.GetByIdAsync(id);
                if (risk == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("Risk not found"));

                return Ok(ApiResponse<object>.SuccessResponse(risk, "Risk retrieved successfully"));
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

                var newRisk = await _riskService.CreateAsync(riskData);

                return CreatedAtAction(nameof(GetRisk), new { id = newRisk.Id },
                    ApiResponse<RiskDto>.SuccessResponse(newRisk, "Risk created successfully"));
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

                var updatedRisk = await _riskService.UpdateAsync(id, riskData);

                return Ok(ApiResponse<RiskDto>.SuccessResponse(updatedRisk, "Risk updated successfully"));
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

                await _riskService.DeleteAsync(id);
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
                var risks = await _riskService.GetAllAsync();
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
                var risks = await _riskService.GetAllAsync();
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

                var risk = await _riskService.GetByIdAsync(id);
                if (risk == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("Risk not found"));

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

                var patchedRisk = await _riskService.UpdateAsync(id, updateDto);
                return Ok(ApiResponse<RiskDto>.SuccessResponse(patchedRisk, "Risk updated successfully"));
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
                        var created = await _riskService.CreateAsync(risk);
                        createdRisks.Add(created);
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
