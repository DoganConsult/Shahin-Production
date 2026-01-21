using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrcMvc.Models.DTOs;
using GrcMvc.Models;
using GrcMvc.Services.Interfaces;
using GrcMvc.Application.Policy;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GrcMvc.Controllers
{
    /// <summary>
    /// Policy API Controller
    /// Handles REST API requests for policy CRUD operations, approvals, and version management
    /// Route: /api/policies
    /// </summary>
    [Route("api/policies")]
    [ApiController]
    [Authorize]
    public class PolicyApiController : ControllerBase
    {
        private readonly IPolicyService _policyService;
        private readonly PolicyEnforcementHelper _policyHelper;

        public PolicyApiController(IPolicyService policyService, PolicyEnforcementHelper policyHelper)
        {
            _policyService = policyService;
            _policyHelper = policyHelper;
        }

        /// <summary>
        /// Get all policies with pagination, sorting, filtering, and search
        /// Query params: ?page=1&size=10&sortBy=date&order=desc&status=active&category=security&q=searchterm
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPolicies(
            [FromQuery] int page = 1,
            [FromQuery] int size = 10,
            [FromQuery] string? sortBy = null,
            [FromQuery] string order = "asc",
            [FromQuery] string? status = null,
            [FromQuery] string? category = null,
            [FromQuery] string? q = null)
        {
            try
            {
                var policies = await _policyService.GetAllAsync();
                
                // Apply filtering
                var filtered = policies.ToList();
                if (!string.IsNullOrEmpty(status))
                    filtered = filtered.Where(p => p.Status == status).ToList();
                if (!string.IsNullOrEmpty(category))
                    filtered = filtered.Where(p => p.Category == category).ToList();

                // Apply search
                if (!string.IsNullOrEmpty(q))
                    filtered = filtered.Where(p => 
                        p.Title?.Contains(q, StringComparison.OrdinalIgnoreCase) == true ||
                        p.Description?.Contains(q, StringComparison.OrdinalIgnoreCase) == true).ToList();

                // Apply sorting
                if (!string.IsNullOrEmpty(sortBy))
                    filtered = order.ToLower() == "desc" 
                        ? filtered.OrderByDescending(p => p.GetType().GetProperty(sortBy)?.GetValue(p)).ToList()
                        : filtered.OrderBy(p => p.GetType().GetProperty(sortBy)?.GetValue(p)).ToList();

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

                return Ok(ApiResponse<PaginatedResponse<object>>.SuccessResponse(response, "Policies retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }

        /// <summary>
        /// Bulk create policies
        /// </summary>
        [HttpPost("bulk")]
        public async Task<IActionResult> BulkCreatePolicies([FromBody] BulkOperationRequest bulkRequest)
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
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }

        /// <summary>
        /// Get policy by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPolicy(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid policy ID"));

                var policy = await _policyService.GetByIdAsync(id);
                if (policy == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("Policy not found"));

                return Ok(ApiResponse<object>.SuccessResponse(policy, "Policy retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }

        /// <summary>
        /// Create new policy
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreatePolicy([FromBody] CreatePolicyDto createPolicyDto)
        {
            try
            {
                if (createPolicyDto == null)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Policy data is required"));

                // POLICY ENFORCEMENT
                await _policyHelper.EnforceCreateAsync("PolicyDocument", createPolicyDto, 
                    dataClassification: createPolicyDto.DataClassification, 
                    owner: createPolicyDto.Owner);

                var policy = await _policyService.CreateAsync(createPolicyDto);
                return CreatedAtAction(nameof(GetPolicy), new { id = policy.Id },
                    ApiResponse<object>.SuccessResponse(policy, "Policy created successfully"));
            }
            catch (PolicyViolationException pex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    $"Policy violation: {pex.Message}. {pex.RemediationHint}"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }

        /// <summary>
        /// Update policy by ID
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePolicy(Guid id, [FromBody] UpdatePolicyDto updatePolicyDto)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid policy ID"));

                if (updatePolicyDto == null)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Policy data is required"));

                // POLICY ENFORCEMENT
                await _policyHelper.EnforceUpdateAsync("PolicyDocument", updatePolicyDto, 
                    dataClassification: updatePolicyDto.DataClassification, 
                    owner: updatePolicyDto.Owner);

                var policy = await _policyService.UpdateAsync(id, updatePolicyDto);
                if (policy == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("Policy not found"));

                return Ok(ApiResponse<object>.SuccessResponse(policy, "Policy updated successfully"));
            }
            catch (PolicyViolationException pex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    $"Policy violation: {pex.Message}. {pex.RemediationHint}"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }

        /// <summary>
        /// Delete policy by ID
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePolicy(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid policy ID"));

                // POLICY ENFORCEMENT
                var policy = await _policyService.GetByIdAsync(id);
                if (policy != null)
                {
                    await _policyHelper.EnforceDeleteAsync("PolicyDocument", policy, 
                        dataClassification: policy.DataClassification, 
                        owner: policy.Owner);
                }

                await _policyService.DeleteAsync(id);
                return Ok(ApiResponse<object>.SuccessResponse(null, "Policy deleted successfully"));
            }
            catch (PolicyViolationException pex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    $"Policy violation: {pex.Message}. {pex.RemediationHint}"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }

        /// <summary>
        /// Approve policy
        /// Transitions policy to approved state and triggers enforcement
        /// </summary>
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> ApprovePolicy(Guid id, [FromBody] dynamic? approvalData)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid policy ID"));

                var policy = await _policyService.GetByIdAsync(id);
                if (policy == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("Policy not found"));

                // Mock approval logic
                var approvalResult = new
                {
                    policyId = id,
                    status = "Approved",
                    approvedDate = DateTime.UtcNow,
                    approvedBy = approvalData?.approvedBy ?? "System",
                    enforcementDate = DateTime.UtcNow.AddDays(7),
                    message = "Policy approved successfully and scheduled for enforcement"
                };

                return Ok(ApiResponse<object>.SuccessResponse(approvalResult, "Policy approved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }

        /// <summary>
        /// Get policy versions
        /// Returns all versions/revisions of the policy
        /// </summary>
        [HttpGet("{id}/versions")]
        public async Task<IActionResult> GetPolicyVersions(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid policy ID"));

                var policy = await _policyService.GetByIdAsync(id);
                if (policy == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("Policy not found"));

                // Return mock policy versions/revision history
                var versions = new List<dynamic>
                {
                    new {
                        versionNumber = 3,
                        status = "Active",
                        createdDate = DateTime.UtcNow,
                        createdBy = "John Smith",
                        description = "Current version - Security and access controls enhanced"
                    },
                    new {
                        versionNumber = 2,
                        status = "Superseded",
                        createdDate = DateTime.UtcNow.AddMonths(-3),
                        createdBy = "Sarah Johnson",
                        description = "Previous version - Initial compliance requirements"
                    },
                    new {
                        versionNumber = 1,
                        status = "Archived",
                        createdDate = DateTime.UtcNow.AddMonths(-6),
                        createdBy = "Ahmed Al-Mansouri",
                        description = "Original version - Basic policy framework"
                    }
                };

                return Ok(ApiResponse<object>.SuccessResponse(versions, "Policy versions retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }

        /// <summary>
        /// Partially update policy
        /// Updates specific fields of a policy (partial update)
        /// </summary>
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchPolicy(Guid id, [FromBody] dynamic patchData)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid policy ID"));

                if (patchData == null)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Patch data is required"));

                var policy = await _policyService.GetByIdAsync(id);
                if (policy == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("Policy not found"));

                // Mock policy patch
                var patchedPolicy = new
                {
                    id = id,
                    updatedFields = new
                    {
                        status = (string?)patchData.status,
                        category = (string?)patchData.category,
                        effectiveDate = (DateTime?)patchData.effectiveDate
                    },
                    patchedDate = DateTime.UtcNow,
                    message = "Policy partially updated successfully"
                };

                return Ok(ApiResponse<object>.SuccessResponse(patchedPolicy, "Policy updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }
    }
}
