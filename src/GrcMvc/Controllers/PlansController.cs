using System;
using System.Threading.Tasks;
using GrcMvc.Models.DTOs;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Controllers
{
    /// <summary>
    /// API endpoints for assessment plan management.
    /// Handles plan creation, phase management, and progress tracking.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PlansController : ControllerBase
    {
        private readonly IPlanService _planService;
        private readonly ILogger<PlansController> _logger;

        public PlansController(
            IPlanService planService,
            ILogger<PlansController> logger)
        {
            _planService = planService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new assessment plan from derived scope.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreatePlanAsync([FromBody] CreatePlanDto request)
        {
            try
            {
                if (request.TenantId == Guid.Empty ||
                    string.IsNullOrWhiteSpace(request.PlanCode) ||
                    string.IsNullOrWhiteSpace(request.Name))
                {
                    return BadRequest("TenantId, PlanCode, and Name are required.");
                }

                var userId = User?.FindFirst("sub")?.Value ?? "SYSTEM";
                var plan = await _planService.CreatePlanAsync(request, userId);

                return CreatedAtAction(nameof(GetPlanAsync), new { planId = plan.Id }, new
                {
                    plan.Id,
                    plan.PlanCode,
                    plan.Status,
                    message = "Plan created successfully with phases."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating plan");
                return BadRequest("An error occurred processing your request.");
            }
        }

        /// <summary>
        /// Get plan by ID with phases and scope snapshot.
        /// </summary>
        [HttpGet("{planId:guid}")]
        public async Task<IActionResult> GetPlanAsync(Guid planId)
        {
            try
            {
                var plan = await _planService.GetPlanAsync(planId);
                if (plan == null)
                {
                    return NotFound("Plan not found.");
                }

                return Ok(plan);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting plan");
                return BadRequest("An error occurred processing your request.");
            }
        }

        /// <summary>
        /// Get all plans for a tenant.
        /// </summary>
        [HttpGet("tenant/{tenantId:guid}")]
        public async Task<IActionResult> GetTenantPlansAsync(Guid tenantId, int pageNumber = 1, int pageSize = 100)
        {
            try
            {
                var plansList = await _planService.GetTenantPlansAsync(tenantId, pageNumber, pageSize);
                var planArray = plansList.ToList();
                return Ok(new { planCount = planArray.Count, plans = planArray });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tenant plans");
                return BadRequest("An error occurred processing your request.");
            }
        }

        /// <summary>
        /// Update plan status (Draft → Active → Paused → Completed).
        /// </summary>
        [HttpPut("{planId:guid}/status")]
        public async Task<IActionResult> UpdatePlanStatusAsync(Guid planId, [FromBody] UpdatePlanStatusRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Status))
                {
                    return BadRequest("Status is required.");
                }

                var userId = User?.FindFirst("sub")?.Value ?? "SYSTEM";
                var plan = await _planService.UpdatePlanStatusAsync(planId, request.Status, userId);

                return Ok(new
                {
                    plan.Id,
                    plan.Status,
                    message = "Plan status updated."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating plan status");
                return BadRequest("An error occurred processing your request.");
            }
        }

        /// <summary>
        /// Get phases for a plan.
        /// </summary>
        [HttpGet("{planId:guid}/phases")]
        public async Task<IActionResult> GetPlanPhasesAsync(Guid planId)
        {
            try
            {
                var phasesList = await _planService.GetPlanPhasesAsync(planId);
                var phaseArray = phasesList.ToList();
                return Ok(new { phaseCount = phaseArray.Count, phases = phaseArray });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting plan phases");
                return BadRequest("An error occurred processing your request.");
            }
        }

        /// <summary>
        /// Update phase progress and status.
        /// </summary>
        [HttpPut("phases/{phaseId:guid}")]
        public async Task<IActionResult> UpdatePhaseAsync(Guid phaseId, [FromBody] UpdatePhaseRequest request)
        {
            try
            {
                var userId = User?.FindFirst("sub")?.Value ?? "SYSTEM";
                var phase = await _planService.UpdatePhaseAsync(phaseId, request.Status, request.ProgressPercentage, userId);

                return Ok(new
                {
                    phase.Id,
                    phase.Status,
                    phase.ProgressPercentage,
                    message = "Phase updated."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating phase");
                return BadRequest("An error occurred processing your request.");
            }
        }
    }

    /// <summary>
    /// Request model for updating plan status.
    /// </summary>
    public class UpdatePlanStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model for updating phase.
    /// </summary>
    public class UpdatePhaseRequest
    {
        public string Status { get; set; } = string.Empty;
        public int ProgressPercentage { get; set; }
    }
}
