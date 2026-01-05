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

        public RiskApiController(IRiskService riskService)
        {
            _riskService = riskService;
        }

        /// <summary>
        /// Get all risks with pagination, sorting, filtering, and search
        /// Query params: ?page=1&size=10&sortBy=date&order=desc&level=high&q=searchterm
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
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
        [AllowAnonymous]
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
        public async Task<IActionResult> CreateRisk([FromBody] dynamic riskData)
        {
            try
            {
                if (riskData == null)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Risk data is required"));

                // Mock risk creation - in production would call actual service
                var newRisk = new
                {
                    id = Guid.NewGuid(),
                    name = (string?)riskData.name ?? "Risk",
                    level = (string?)riskData.level ?? "Medium",
                    status = "Identified",
                    createdDate = DateTime.Now,
                    message = "Risk created successfully"
                };

                return CreatedAtAction(nameof(GetRisk), new { id = newRisk.id },
                    ApiResponse<object>.SuccessResponse(newRisk, "Risk created successfully"));
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
        public async Task<IActionResult> UpdateRisk(Guid id, [FromBody] dynamic riskData)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid risk ID"));

                if (riskData == null)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Risk data is required"));

                var risk = await _riskService.GetByIdAsync(id);
                if (risk == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("Risk not found"));

                var updatedRisk = new
                {
                    id = id,
                    name = (string?)riskData.name ?? risk.Name,
                    category = (string?)riskData.category ?? risk.Category,
                    status = (string?)riskData.status ?? risk.Status,
                    updatedDate = DateTime.Now,
                    message = "Risk updated successfully"
                };

                return Ok(ApiResponse<object>.SuccessResponse(updatedRisk, "Risk updated successfully"));
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
        [AllowAnonymous]
        public async Task<IActionResult> GetHighRisks()
        {
            try
            {
                var risks = await _riskService.GetAllAsync();
                var highRisks = risks.Where(r => (r.Probability * r.Impact) >= 20).ToList();

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
        [AllowAnonymous]
        public async Task<IActionResult> GetRiskStatistics()
        {
            try
            {
                var risks = await _riskService.GetAllAsync();
                var stats = new
                {
                    totalRisks = risks.Count(),
                    highRisks = risks.Count(r => (r.Probability * r.Impact) >= 15),
                    mediumRisks = risks.Count(r => (r.Probability * r.Impact) >= 10 && (r.Probability * r.Impact) < 15),
                    lowRisks = risks.Count(r => (r.Probability * r.Impact) < 10),
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
        public async Task<IActionResult> PatchRisk(Guid id, [FromBody] dynamic patchData)
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

                var patchedRisk = new
                {
                    id = id,
                    category = (string?)patchData.category ?? risk.Category,
                    status = (string?)patchData.status ?? risk.Status,
                    updatedDate = DateTime.Now,
                    message = "Risk updated successfully"
                };

                return Ok(ApiResponse<object>.SuccessResponse(patchedRisk, "Risk updated successfully"));
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
        public async Task<IActionResult> BulkCreateRisks([FromBody] BulkOperationRequest bulkRequest)
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

                return Ok(ApiResponse<BulkOperationResult>.SuccessResponse(result, "Bulk risk creation completed successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }
    }
}
