using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrcMvc.Models.DTOs;
using GrcMvc.Models;
using GrcMvc.Services.Interfaces;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace GrcMvc.Controllers
{
    /// <summary>
    /// Plans API Controller
    /// Handles REST API requests for compliance and remediation plans
    /// Route: /api/plans
    /// </summary>
    [Route("api/plans")]
    [ApiController]
    [Authorize]
    public class PlansApiController : ControllerBase
    {
        private readonly IPlanService _planService;

        public PlansApiController(IPlanService planService)
        {
            _planService = planService;
        }

        /// <summary>
        /// Get plan by ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPlanById(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid plan ID"));

                var plan = await _planService.GetPlanAsync(id);
                if (plan == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("Plan not found"));

                return Ok(ApiResponse<object>.SuccessResponse(plan, "Plan retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }

        /// <summary>
        /// Get tenant plans
        /// Returns all plans for a specific tenant with pagination
        /// </summary>
        [HttpGet("tenant/{tenantId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTenantPlans(
            Guid tenantId,
            [FromQuery] int page = 1,
            [FromQuery] int size = 10)
        {
            try
            {
                if (tenantId == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid tenant ID"));

                var plans = await _planService.GetTenantPlansAsync(tenantId, page, size);
                return Ok(ApiResponse<object>.SuccessResponse(plans, "Tenant plans retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }

        /// <summary>
        /// Create a new plan
        /// Creates a new compliance/remediation plan from scope definition
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreatePlan([FromBody] CreatePlanDto request)
        {
            try
            {
                if (request == null)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid plan request"));

                var plan = await _planService.CreatePlanAsync(request, User.Identity?.Name ?? "System");
                return CreatedAtAction(nameof(GetPlanById), new { id = plan.Id }, 
                    ApiResponse<object>.SuccessResponse(plan, "Plan created successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }

        /// <summary>
        /// Update plan status
        /// Updates the status of a compliance plan
        /// </summary>
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdatePlanStatus(Guid id, [FromBody] dynamic statusUpdate)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid plan ID"));

                string status = statusUpdate?.status ?? "Pending";
                var plan = await _planService.UpdatePlanStatusAsync(id, status, User.Identity?.Name ?? "System");
                return Ok(ApiResponse<object>.SuccessResponse(plan, "Plan status updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }

        /// <summary>
        /// Get plan phases
        /// Returns all phases of a specific plan
        /// </summary>
        [HttpGet("{id}/phases")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPlanPhases(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid plan ID"));

                var phases = await _planService.GetPlanPhasesAsync(id);
                return Ok(ApiResponse<object>.SuccessResponse(phases, "Plan phases retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }

        /// <summary>
        /// Update phase progress
        /// Updates the progress and status of a specific plan phase
        /// </summary>
        [HttpPut("phases/{phaseId}")]
        public async Task<IActionResult> UpdatePhase(Guid phaseId, [FromBody] dynamic phaseUpdate)
        {
            try
            {
                if (phaseId == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid phase ID"));

                string status = phaseUpdate?.status ?? "In Progress";
                int progress = (int)(phaseUpdate?.progressPercentage ?? 0);
                var phase = await _planService.UpdatePhaseAsync(phaseId, status, progress, User.Identity?.Name ?? "System");
                return Ok(ApiResponse<object>.SuccessResponse(phase, "Phase updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }

        /// <summary>
        /// Get plan phases with filter
        /// Returns phases filtered by status
        /// </summary>
        [HttpGet("phases/status/{status}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPhasesByStatus(Guid planId, string status)
        {
            try
            {
                if (planId == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid plan ID"));

                var phases = await _planService.GetPlanPhasesAsync(planId);
                var filteredPhases = phases.Where(p => p.Status == status).ToList();
                
                return Ok(ApiResponse<object>.SuccessResponse(filteredPhases, "Phases retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }

        /// <summary>
        /// Get tenant plan statistics
        /// </summary>
        [HttpGet("tenant/{tenantId}/statistics")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTenantPlanStatistics(Guid tenantId)
        {
            try
            {
                if (tenantId == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid tenant ID"));

                var plans = await _planService.GetTenantPlansAsync(tenantId, 1, 1000);
                var stats = new
                {
                    totalPlans = plans.Count(),
                    activePlans = plans.Count(p => p.Status == "Active"),
                    completedPlans = plans.Count(p => p.Status == "Completed"),
                    pendingPlans = plans.Count(p => p.Status == "Pending"),
                    timestamp = DateTime.UtcNow
                };

                return Ok(ApiResponse<object>.SuccessResponse(stats, "Plan statistics retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));
            }
        }
    }
}
