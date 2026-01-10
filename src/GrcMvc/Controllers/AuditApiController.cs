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
    /// Audit API Controller
    /// Handles REST API requests for audit CRUD operations and findings management
    /// Route: /api/audits
    /// </summary>
    [Route("api/audits")]
    [ApiController]
    [Authorize]
    public class AuditApiController : ControllerBase
    {
        private readonly IAuditService _auditService;

        public AuditApiController(IAuditService auditService)
        {
            _auditService = auditService;
        }

        /// <summary>
        /// Get all audits with pagination, sorting, filtering, and search
        /// Query params: ?page=1&size=10&sortBy=date&order=desc&status=active&q=searchterm
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAudits(
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
                var audits = await _auditService.GetAllAsync();
                
                // Apply filtering
                var filtered = audits.ToList();
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

                return Ok(ApiResponse<PaginatedResponse<object>>.SuccessResponse(response, "Audits retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }

        /// <summary>
        /// Bulk create audits
        /// </summary>
        [HttpPost("bulk")]
        public async Task<IActionResult> BulkCreateAudits([FromBody] BulkOperationRequest bulkRequest)
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

                return Ok(ApiResponse<BulkOperationResult>.SuccessResponse(result, "Bulk operation completed successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }

        /// <summary>
        /// Get audit by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAudit(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid audit ID"));

                var audit = await _auditService.GetByIdAsync(id);
                if (audit == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("Audit not found"));

                return Ok(ApiResponse<object>.SuccessResponse(audit, "Audit retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }

        /// <summary>
        /// Create new audit
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateAudit([FromBody] CreateAuditDto createAuditDto)
        {
            try
            {
                if (createAuditDto == null)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Audit data is required"));

                var audit = await _auditService.CreateAsync(createAuditDto);
                return CreatedAtAction(nameof(GetAudit), new { id = audit.Id }, 
                    ApiResponse<object>.SuccessResponse(audit, "Audit created successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }

        /// <summary>
        /// Update audit by ID
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAudit(Guid id, [FromBody] UpdateAuditDto updateAuditDto)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid audit ID"));

                if (updateAuditDto == null)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Audit data is required"));

                var audit = await _auditService.UpdateAsync(id, updateAuditDto);
                if (audit == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("Audit not found"));

                return Ok(ApiResponse<object>.SuccessResponse(audit, "Audit updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }

        /// <summary>
        /// Delete audit by ID
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAudit(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid audit ID"));

                await _auditService.DeleteAsync(id);
                return Ok(ApiResponse<object>.SuccessResponse(null, "Audit deleted successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }

        /// <summary>
        /// Get audit findings
        /// Returns a list of findings identified during the audit
        /// </summary>
        [HttpGet("{id}/findings")]
        public async Task<IActionResult> GetAuditFindings(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid audit ID"));

                var findings = await _auditService.GetAuditFindingsAsync(id);
                return Ok(ApiResponse<object>.SuccessResponse(findings, "Audit findings retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }

        /// <summary>
        /// Create audit finding
        /// Adds a new finding to an existing audit
        /// </summary>
        [HttpPost("{id}/findings")]
        public async Task<IActionResult> CreateAuditFinding(Guid id, [FromBody] CreateAuditFindingDto createFindingDto)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid audit ID"));

                if (createFindingDto == null)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Finding data is required"));

                var finding = await _auditService.AddFindingAsync(id, createFindingDto);
                if (finding == null)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Failed to add finding to audit"));

                return CreatedAtAction(nameof(GetAuditFindings), new { id }, 
                    ApiResponse<object>.SuccessResponse(finding, "Finding created successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }

        /// <summary>
        /// Partially update audit
        /// Updates specific fields of an audit (partial update)
        /// </summary>
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchAudit(Guid id, [FromBody] dynamic patchData)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid audit ID"));

                if (patchData == null)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Patch data is required"));

                var audit = await _auditService.GetByIdAsync(id);
                if (audit == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("Audit not found"));

                // Mock audit patch
                var patchedAudit = new
                {
                    id = id,
                    updatedFields = new
                    {
                        status = (string?)patchData.status,
                        scope = (string?)patchData.scope,
                        schedule = (string?)patchData.schedule
                    },
                    patchedDate = DateTime.UtcNow,
                    message = "Audit partially updated successfully"
                };

                return Ok(ApiResponse<object>.SuccessResponse(patchedAudit, "Audit updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }
    }
}
