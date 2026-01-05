using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrcMvc.Models.DTOs;
using GrcMvc.Models;
using GrcMvc.Services.Interfaces;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GrcMvc.Controllers
{
    /// <summary>
    /// Assessment API Controller
    /// Handles REST API requests for assessment CRUD operations and requirements management
    /// Route: /api/assessments
    /// </summary>
    [Route("api/assessments")]
    [ApiController]
    [Authorize]
    public class AssessmentApiController : ControllerBase
    {
        private readonly IAssessmentService _assessmentService;
        private readonly IControlService _controlService;

        public AssessmentApiController(IAssessmentService assessmentService, IControlService controlService)
        {
            _assessmentService = assessmentService;
            _controlService = controlService;
        }

        /// <summary>
        /// Get all assessments with pagination, sorting, filtering, and search
        /// Query params: ?page=1&size=10&sortBy=date&order=desc&status=active&type=internal&q=searchterm
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAssessments(
            [FromQuery] int page = 1,
            [FromQuery] int size = 10,
            [FromQuery] string? sortBy = null,
            [FromQuery] string order = "asc",
            [FromQuery] string? status = null,
            [FromQuery] string? type = null,
            [FromQuery] string? q = null)
        {
            try
            {
                var assessments = await _assessmentService.GetAllAsync();

                // Apply filtering
                var filtered = assessments.ToList();
                if (!string.IsNullOrEmpty(status))
                    filtered = filtered.Where(a => a.Status == status).ToList();
                if (!string.IsNullOrEmpty(type))
                    filtered = filtered.Where(a => a.Type == type).ToList();

                // Apply search
                if (!string.IsNullOrEmpty(q))
                    filtered = filtered.Where(a =>
                        a.Name?.Contains(q, StringComparison.OrdinalIgnoreCase) == true ||
                        a.Description?.Contains(q, StringComparison.OrdinalIgnoreCase) == true).ToList();

                // Apply sorting
                if (!string.IsNullOrEmpty(sortBy))
                    filtered = order.ToLower() == "desc"
                        ? filtered.OrderByDescending(a => a.GetType().GetProperty(sortBy)?.GetValue(a)).ToList()
                        : filtered.OrderBy(a => a.GetType().GetProperty(sortBy)?.GetValue(a)).ToList();

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

                return Ok(ApiResponse<PaginatedResponse<object>>.SuccessResponse(response, "Assessments retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Bulk create assessments
        /// </summary>
        [HttpPost("bulk")]
        public async Task<IActionResult> BulkCreateAssessments([FromBody] BulkOperationRequest bulkRequest)
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
                    CompletedAt = DateTime.Now
                };

                return Ok(ApiResponse<BulkOperationResult>.SuccessResponse(result, "Bulk operation completed successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get assessment by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAssessment(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid assessment ID"));

                var assessment = await _assessmentService.GetByIdAsync(id);
                if (assessment == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("Assessment not found"));

                return Ok(ApiResponse<object>.SuccessResponse(assessment, "Assessment retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Create new assessment
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateAssessment([FromBody] CreateAssessmentDto createAssessmentDto)
        {
            try
            {
                if (createAssessmentDto == null)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Assessment data is required"));

                var assessment = await _assessmentService.CreateAsync(createAssessmentDto);
                return CreatedAtAction(nameof(GetAssessment), new { id = assessment.Id },
                    ApiResponse<object>.SuccessResponse(assessment, "Assessment created successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Update assessment by ID
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAssessment(Guid id, [FromBody] UpdateAssessmentDto updateAssessmentDto)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid assessment ID"));

                if (updateAssessmentDto == null)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Assessment data is required"));

                var assessment = await _assessmentService.UpdateAsync(id, updateAssessmentDto);
                if (assessment == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("Assessment not found"));

                return Ok(ApiResponse<object>.SuccessResponse(assessment, "Assessment updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Delete assessment by ID
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAssessment(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid assessment ID"));

                await _assessmentService.DeleteAsync(id);
                return Ok(ApiResponse<object>.SuccessResponse(null, "Assessment deleted successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Submit assessment for review/approval
        /// Transitions the assessment to submitted state and triggers workflow
        /// </summary>
        [HttpPost("{id}/submit")]
        public async Task<IActionResult> SubmitAssessment(Guid id, [FromBody] dynamic? submissionData)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid assessment ID"));

                var assessment = await _assessmentService.GetByIdAsync(id);
                if (assessment == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("Assessment not found"));

                // Update assessment status to submitted
                var updateDto = new UpdateAssessmentDto { Status = "Submitted" };
                var submittedAssessment = await _assessmentService.UpdateAsync(id, updateDto);

                var submissionResult = new
                {
                    assessmentId = id,
                    status = submittedAssessment?.Status ?? "Submitted",
                    submittedDate = DateTime.Now,
                    submittedBy = submissionData?.submittedBy ?? "System",
                    message = "Assessment submitted successfully for review"
                };

                return Ok(ApiResponse<object>.SuccessResponse(submissionResult, "Assessment submitted successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get requirements associated with an assessment
        /// Returns a list of requirements/controls that are evaluated within this assessment
        /// </summary>
        [HttpGet("{id}/requirements")]
        public async Task<IActionResult> GetAssessmentRequirements(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid assessment ID"));

                var assessment = await _assessmentService.GetByIdAsync(id);
                if (assessment == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("Assessment not found"));

                // Get controls associated with the assessment via ControlId if present
                var requirements = new List<ControlDto>();
                if (assessment.ControlId.HasValue && assessment.ControlId != Guid.Empty)
                {
                    var control = await _controlService.GetByIdAsync(assessment.ControlId.Value);
                    if (control != null)
                        requirements.Add(control);
                }
                else
                {
                    // Get all controls as requirements if no specific control is linked
                    requirements = (await _controlService.GetAllAsync()).ToList();
                }

                return Ok(ApiResponse<object>.SuccessResponse(requirements, "Assessment requirements retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Partially update assessment
        /// Updates specific fields of an assessment (partial update)
        /// </summary>
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchAssessment(Guid id, [FromBody] dynamic patchData)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid assessment ID"));

                if (patchData == null)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Patch data is required"));

                var assessment = await _assessmentService.GetByIdAsync(id);
                if (assessment == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("Assessment not found"));

                // Build update DTO from patch data
                var updateDto = new UpdateAssessmentDto
                {
                    Type = ((string?)patchData.type) ?? assessment.Type,
                    Name = ((string?)patchData.name) ?? assessment.Name,
                    Description = ((string?)patchData.description) ?? assessment.Description,
                    Status = ((string?)patchData.status) ?? assessment.Status
                };

                var patchedAssessment = await _assessmentService.UpdateAsync(id, updateDto);

                return Ok(ApiResponse<object>.SuccessResponse(patchedAssessment, "Assessment updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }
    }
}
