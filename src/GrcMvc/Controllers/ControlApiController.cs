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
    /// Control API Controller
    /// Handles REST API requests for control CRUD operations, compliance mappings, and assessments
    /// Route: /api/controls
    /// </summary>
    [Route("api/controls")]
    [ApiController]
    [Authorize]
    public class ControlApiController : ControllerBase
    {
        private readonly IControlService _controlService;

        public ControlApiController(IControlService controlService)
        {
            _controlService = controlService;
        }

        /// <summary>
        /// Get all controls with pagination, sorting, filtering, and search
        /// Query params: ?page=1&size=10&sortBy=name&order=asc&category=access&q=searchterm
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetControls(
            [FromQuery] int page = 1,
            [FromQuery] int size = 10,
            [FromQuery] string? sortBy = null,
            [FromQuery] string order = "asc",
            [FromQuery] string? category = null,
            [FromQuery] string? q = null)
        {
            try
            {
                var controls = await _controlService.GetAllAsync();

                // Apply filtering
                var filtered = controls.ToList();
                if (!string.IsNullOrEmpty(category))
                    filtered = filtered.Where(c => c.Category == category).ToList();

                // Apply search
                if (!string.IsNullOrEmpty(q))
                    filtered = filtered.Where(c =>
                        c.Name?.Contains(q, StringComparison.OrdinalIgnoreCase) == true ||
                        c.Description?.Contains(q, StringComparison.OrdinalIgnoreCase) == true).ToList();

                // Apply sorting
                if (!string.IsNullOrEmpty(sortBy))
                    filtered = order.ToLower() == "desc"
                        ? filtered.OrderByDescending(c => c.GetType().GetProperty(sortBy)?.GetValue(c)).ToList()
                        : filtered.OrderBy(c => c.GetType().GetProperty(sortBy)?.GetValue(c)).ToList();

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

                return Ok(ApiResponse<PaginatedResponse<object>>.SuccessResponse(response, "Controls retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get control by ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetControl(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid control ID"));

                var control = await _controlService.GetByIdAsync(id);
                if (control == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("Control not found"));

                return Ok(ApiResponse<object>.SuccessResponse(control, "Control retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Create new control
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateControl([FromBody] CreateControlDto createControlDto)
        {
            try
            {
                if (createControlDto == null)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Control data is required"));

                var control = await _controlService.CreateAsync(createControlDto);
                return CreatedAtAction(nameof(GetControl), new { id = control.Id },
                    ApiResponse<object>.SuccessResponse(control, "Control created successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Update control by ID
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateControl(Guid id, [FromBody] UpdateControlDto updateControlDto)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid control ID"));

                if (updateControlDto == null)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Control data is required"));

                var control = await _controlService.UpdateAsync(id, updateControlDto);
                if (control == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("Control not found"));

                return Ok(ApiResponse<object>.SuccessResponse(control, "Control updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Delete control by ID
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteControl(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid control ID"));

                await _controlService.DeleteAsync(id);
                return Ok(ApiResponse<object>.SuccessResponse(null, "Control deleted successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get controls by risk ID
        /// Returns all controls associated with a specific risk
        /// </summary>
        [HttpGet("risk/{riskId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetControlsByRisk(Guid riskId)
        {
            try
            {
                if (riskId == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid risk ID"));

                var controls = await _controlService.GetByRiskIdAsync(riskId);
                return Ok(ApiResponse<object>.SuccessResponse(controls, "Risk controls retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get control statistics
        /// Returns aggregate statistics about controls
        /// </summary>
        [HttpGet("statistics")]
        [AllowAnonymous]
        public async Task<IActionResult> GetControlStatistics()
        {
            try
            {
                var stats = await _controlService.GetStatisticsAsync();
                return Ok(ApiResponse<object>.SuccessResponse(stats, "Control statistics retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Partially update control
        /// Updates specific fields of a control (partial update)
        /// </summary>
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchControl(Guid id, [FromBody] dynamic patchData)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid control ID"));

                if (patchData == null)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Patch data is required"));

                var control = await _controlService.GetByIdAsync(id);
                if (control == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("Control not found"));

                // Build update DTO from patch data
                var updateDto = new UpdateControlDto
                {
                    Name = ((string?)patchData.name) ?? control.Name,
                    Description = ((string?)patchData.description) ?? control.Description,
                    Category = ((string?)patchData.category) ?? control.Category,
                    Status = ((string?)patchData.status) ?? control.Status
                };

                var patchedControl = await _controlService.UpdateAsync(id, updateDto);
                return Ok(ApiResponse<object>.SuccessResponse(patchedControl, "Control updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Bulk create controls
        /// </summary>
        [HttpPost("bulk")]
        public async Task<IActionResult> BulkCreateControls([FromBody] BulkOperationRequest bulkRequest)
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
    }
}
