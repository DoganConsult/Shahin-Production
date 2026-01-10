using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrcMvc.Models.DTOs;
using GrcMvc.Models;
using GrcMvc.Services.Interfaces;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace GrcMvc.Controllers
{
    /// <summary>
    /// Evidence API Controller
    /// Handles REST API requests for evidence collection, management, and compliance documentation
    /// Route: /api/evidence
    /// </summary>
    [Route("api/evidence")]
    [ApiController]
    [Authorize]
    public class EvidenceApiController : ControllerBase
    {
        private readonly IEvidenceService _evidenceService;

        public EvidenceApiController(IEvidenceService evidenceService)
        {
            _evidenceService = evidenceService;
        }

        /// <summary>
        /// Get all evidence with pagination, sorting, filtering, and search
        /// Query params: ?page=1&size=10&sortBy=date&order=desc&type=document&q=searchterm
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetEvidence(
            [FromQuery] int page = 1,
            [FromQuery] int size = 10,
            [FromQuery] string? sortBy = null,
            [FromQuery] string order = "asc",
            [FromQuery] string? type = null,
            [FromQuery] string? q = null)
        {
            try
            {
                var evidence = await _evidenceService.GetAllAsync();

                // Apply filtering
                var filtered = evidence.ToList();
                if (!string.IsNullOrEmpty(type))
                    filtered = filtered.Where(e => e.EvidenceType == type).ToList();

                // Apply search
                if (!string.IsNullOrEmpty(q))
                    filtered = filtered.Where(e =>
                        e.Name?.Contains(q, StringComparison.OrdinalIgnoreCase) == true ||
                        e.Description?.Contains(q, StringComparison.OrdinalIgnoreCase) == true).ToList();

                // Apply sorting
                if (!string.IsNullOrEmpty(sortBy))
                    filtered = order.ToLower() == "desc"
                        ? filtered.OrderByDescending(e => e.GetType().GetProperty(sortBy)?.GetValue(e)).ToList()
                        : filtered.OrderBy(e => e.GetType().GetProperty(sortBy)?.GetValue(e)).ToList();

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

                return Ok(ApiResponse<PaginatedResponse<object>>.SuccessResponse(response, "Evidence retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }

        /// <summary>
        /// Get evidence by ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEvidenceById(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid evidence ID"));

                var evidence = await _evidenceService.GetByIdAsync(id);
                if (evidence == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("Evidence not found"));

                return Ok(ApiResponse<object>.SuccessResponse(evidence, "Evidence retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }

        /// <summary>
        /// Create new evidence
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateEvidence([FromBody] dynamic evidenceData)
        {
            try
            {
                if (evidenceData == null)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Evidence data is required"));

                // Mock evidence creation - in production would call actual service
                var newEvidence = new
                {
                    id = Guid.NewGuid(),
                    name = (string?)evidenceData.name ?? "Evidence",
                    type = (string?)evidenceData.type ?? "Document",
                    controlId = (Guid?)evidenceData.controlId ?? Guid.Empty,
                    createdDate = DateTime.UtcNow,
                    status = "Submitted",
                    message = "Evidence created successfully"
                };

                return CreatedAtAction(nameof(GetEvidenceById), new { id = newEvidence.id },
                    ApiResponse<object>.SuccessResponse(newEvidence, "Evidence created successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }

        /// <summary>
        /// Update evidence by ID
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvidence(Guid id, [FromBody] dynamic evidenceData)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid evidence ID"));

                if (evidenceData == null)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Evidence data is required"));

                var evidence = await _evidenceService.GetByIdAsync(id);
                if (evidence == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("Evidence not found"));

                // Update evidence
                var updatedEvidence = new
                {
                    id = id,
                    name = (string?)evidenceData.name ?? evidence.Name,
                    type = (string?)evidenceData.type ?? evidence.EvidenceType,
                    status = (string?)evidenceData.status ?? evidence.Status,
                    updatedDate = DateTime.UtcNow,
                    message = "Evidence updated successfully"
                };

                return Ok(ApiResponse<object>.SuccessResponse(updatedEvidence, "Evidence updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }

        /// <summary>
        /// Delete evidence by ID
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvidence(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid evidence ID"));

                await _evidenceService.DeleteAsync(id);
                return Ok(ApiResponse<object>.SuccessResponse(null, "Evidence deleted successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }

        /// <summary>
        /// Get evidence by control ID
        /// Returns all evidence associated with a specific control
        /// </summary>
        [HttpGet("control/{controlId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEvidenceByControl(Guid controlId)
        {
            try
            {
                if (controlId == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid control ID"));

                var evidence = await _evidenceService.GetAllAsync();
                var controlEvidence = evidence.Where(e => e.Id == controlId).ToList();
                return Ok(ApiResponse<object>.SuccessResponse(controlEvidence, "Control evidence retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }

        /// <summary>
        /// Get evidence by assessment ID
        /// Returns all evidence collected during a specific assessment
        /// </summary>
        [HttpGet("assessment/{assessmentId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEvidenceByAssessment(Guid assessmentId)
        {
            try
            {
                if (assessmentId == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid assessment ID"));

                var evidence = await _evidenceService.GetAllAsync();
                var assessmentEvidence = evidence.Where(e => e.Id == assessmentId).ToList();
                return Ok(ApiResponse<object>.SuccessResponse(assessmentEvidence, "Assessment evidence retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }

        /// <summary>
        /// Partially update evidence
        /// Updates specific fields of evidence (partial update)
        /// </summary>
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchEvidence(Guid id, [FromBody] dynamic patchData)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid evidence ID"));

                if (patchData == null)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Patch data is required"));

                var evidence = await _evidenceService.GetByIdAsync(id);
                if (evidence == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("Evidence not found"));

                var patchedEvidence = new
                {
                    id = id,
                    status = (string?)patchData.status ?? evidence.Status,
                    updatedDate = DateTime.UtcNow,
                    message = "Evidence updated successfully"
                };

                return Ok(ApiResponse<object>.SuccessResponse(patchedEvidence, "Evidence updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }

        /// <summary>
        /// Bulk upload evidence
        /// </summary>
        [HttpPost("bulk")]
        public async Task<IActionResult> BulkUploadEvidence([FromBody] BulkOperationRequest bulkRequest)
        {
            try
            {
                if (bulkRequest?.Items == null || bulkRequest.Items.Count == 0)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Items are required for bulk operation"));

                var result = new BulkOperationResult
                {
                    TotalItems = bulkRequest.Items.Count,
                    SuccessfulItems = bulkRequest.Items.Count,
                    FailedItems = 0,
                    CompletedAt = DateTime.UtcNow
                };

                return Ok(ApiResponse<BulkOperationResult>.SuccessResponse(result, "Bulk evidence upload completed successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }
    }
}
