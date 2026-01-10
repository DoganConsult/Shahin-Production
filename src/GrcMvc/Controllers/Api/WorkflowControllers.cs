using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces.Workflows;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Microsoft.Extensions.Localization;
using GrcMvc.Resources;
namespace GrcMvc.Controllers.Api
{
    /// <summary>
    /// API endpoints for managing Control Implementation Workflows
    /// </summary>
    [ApiController]
    [Route("api/workflows/control-implementation")]
    [Authorize]
    public class ControlImplementationWorkflowController : ControllerBase
    {
        private readonly IControlImplementationWorkflowService _workflow;
        private readonly ILogger<ControlImplementationWorkflowController> _logger;

        public ControlImplementationWorkflowController(
            IControlImplementationWorkflowService workflow,
            ILogger<ControlImplementationWorkflowController> logger)
        {
            _workflow = workflow;
            _logger = logger;
        }

        /// <summary>Initiate control implementation workflow</summary>
        [HttpPost("initiate")]
        public async Task<IActionResult> InitiateWorkflow([FromBody] InitiateWorkflowRequest request)
        {
            try
            {
                var workflow = await _workflow.InitiateControlImplementationAsync(
                    request.ControlId,
                    request.TenantId,
                    User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "system");

                _logger.LogInformation($"Control implementation workflow initiated for control {request.ControlId}");

                return CreatedAtAction(nameof(GetWorkflow),
                    new { id = workflow.Id }, workflow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating workflow");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>Get workflow status</summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetWorkflow(Guid id)
        {
            var workflow = await _workflow.GetWorkflowAsync(id);
            if (workflow == null)
                return NotFound();

            return Ok(workflow);
        }

        /// <summary>Get pending tasks</summary>
        [HttpGet("{id:guid}/tasks")]
        public async Task<IActionResult> GetPendingTasks(Guid id)
        {
            var tasks = await _workflow.GetPendingTasksAsync(id);
            return Ok(tasks);
        }

        /// <summary>Move to planning phase</summary>
        [HttpPost("{id:guid}/planning")]
        public async Task<IActionResult> MoveToPlanning(Guid id, [FromBody] PlanningRequest request)
        {
            var result = await _workflow.MoveToPlanning(id, request.Notes);
            if (!result)
                return BadRequest("Failed to move to planning");

            return Ok(new { message = "Moved to planning phase" });
        }

        /// <summary>Move to implementation phase</summary>
        [HttpPost("{id:guid}/implementation")]
        public async Task<IActionResult> MoveToImplementation(Guid id, [FromBody] ImplementationRequest request)
        {
            var result = await _workflow.MoveToImplementation(id, request.Details);
            if (!result)
                return BadRequest("Failed to move to implementation");

            return Ok(new { message = "Moved to implementation phase" });
        }

        /// <summary>Submit for review</summary>
        [HttpPost("{id:guid}/submit-review")]
        public async Task<IActionResult> SubmitForReview(Guid id)
        {
            var userId = User.FindFirst("sub")?.Value ?? "system";
            var result = await _workflow.SubmitForReview(id, userId);
            if (!result)
                return BadRequest("Failed to submit for review");

            return Ok(new { message = "Submitted for review" });
        }

        /// <summary>Approve implementation</summary>
        [HttpPost("{id:guid}/approve")]
        [Authorize(Roles = "Admin,ComplianceOfficer")]
        public async Task<IActionResult> ApproveImplementation(Guid id, [FromBody] ApprovalRequest request)
        {
            var userId = User.FindFirst("sub")?.Value ?? "system";
            var result = await _workflow.ApproveImplementation(id, userId, request.Reason);
            if (!result)
                return BadRequest("Failed to approve");

            return Ok(new { message = "Implementation approved" });
        }

        /// <summary>Deploy control</summary>
        [HttpPost("{id:guid}/deploy")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeployControl(Guid id)
        {
            var result = await _workflow.DeployControl(id);
            if (!result)
                return BadRequest("Failed to deploy");

            return Ok(new { message = "Control deployed" });
        }

        /// <summary>Start monitoring</summary>
        [HttpPost("{id:guid}/monitor")]
        public async Task<IActionResult> StartMonitoring(Guid id)
        {
            var result = await _workflow.StartMonitoring(id);
            if (!result)
                return BadRequest("Failed to start monitoring");

            return Ok(new { message = "Monitoring started" });
        }

        /// <summary>Complete workflow</summary>
        [HttpPost("{id:guid}/complete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CompleteWorkflow(Guid id)
        {
            var result = await _workflow.CompleteWorkflow(id);
            if (!result)
                return BadRequest("Failed to complete");

            return Ok(new { message = "Workflow completed" });
        }
    }

    /// <summary>API endpoints for Approval/Sign-off Workflows</summary>
    [ApiController]
    [Route("api/workflows/approval")]
    [Authorize]
    public class ApprovalWorkflowController : ControllerBase
    {
        private readonly IApprovalWorkflowService _workflow;
        private readonly ILogger<ApprovalWorkflowController> _logger;

        public ApprovalWorkflowController(
            IApprovalWorkflowService workflow,
            ILogger<ApprovalWorkflowController> logger)
        {
            _workflow = workflow;
            _logger = logger;
        }

        /// <summary>Submit for approval</summary>
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitForApproval([FromBody] SubmitApprovalRequest request)
        {
            try
            {
                var workflow = await _workflow.SubmitForApprovalAsync(
                    request.EntityId,
                    request.EntityType,
                    request.TenantId,
                    User.Identity?.Name ?? "system");

                return CreatedAtAction(nameof(GetApprovalHistory),
                    new { id = workflow.Id }, workflow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting for approval");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>Manager approval</summary>
        [HttpPost("{id:guid}/approve-manager")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> ApproveAsManager(Guid id, [FromBody] ManagerApprovalRequest request)
        {
            var result = await _workflow.ApproveAsManagerAsync(
                id, User.Identity?.Name ?? "system", request.Comments);

            if (!result)
                return BadRequest("Failed to approve");

            return Ok(new { message = "Manager approval recorded" });
        }

        /// <summary>Compliance approval</summary>
        [HttpPost("{id:guid}/approve-compliance")]
        [Authorize(Roles = "Admin,ComplianceOfficer")]
        public async Task<IActionResult> ApproveAsCompliance(Guid id, [FromBody] ComplianceApprovalRequest request)
        {
            var result = await _workflow.ApproveAsComplianceAsync(
                id, User.Identity?.Name ?? "system", request.Comments);

            if (!result)
                return BadRequest("Failed to approve");

            return Ok(new { message = "Compliance approval recorded" });
        }

        /// <summary>Executive sign-off</summary>
        [HttpPost("{id:guid}/approve-executive")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveAsExecutive(Guid id, [FromBody] ExecutiveApprovalRequest request)
        {
            var result = await _workflow.ApproveAsExecutiveAsync(
                id, User.Identity?.Name ?? "system", request.Comments);

            if (!result)
                return BadRequest("Failed to approve");

            return Ok(new { message = "Executive approval recorded" });
        }

        /// <summary>Get approval history</summary>
        [HttpGet("{id:guid}/history")]
        public async Task<IActionResult> GetApprovalHistory(Guid id)
        {
            var history = await _workflow.GetApprovalHistoryAsync(id);
            return Ok(history);
        }

        /// <summary>Get current approval level</summary>
        [HttpGet("{id:guid}/current-level")]
        public async Task<IActionResult> GetCurrentLevel(Guid id)
        {
            var level = await _workflow.GetCurrentApprovalLevelAsync(id);
            return Ok(new { currentLevel = level });
        }

        /// <summary>Request revision</summary>
        [HttpPost("{id:guid}/request-revision")]
        public async Task<IActionResult> RequestRevision(Guid id, [FromBody] RevisionRequest request)
        {
            var result = await _workflow.RequestRevisionAsync(
                id, User.Identity?.Name ?? "system", request.RevisionNotes);

            if (!result)
                return BadRequest("Failed to request revision");

            return Ok(new { message = "Revision requested" });
        }

        /// <summary>Finalize approval</summary>
        [HttpPost("{id:guid}/finalize")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> FinalizeApproval(Guid id)
        {
            var result = await _workflow.FinalizeApprovalAsync(id);
            if (!result)
                return BadRequest("Failed to finalize");

            return Ok(new { message = "Approval finalized" });
        }
    }

    // ===== Request/Response DTOs =====

    public class InitiateWorkflowRequest
    {
        public Guid ControlId { get; set; }
        public Guid TenantId { get; set; }
    }

    public class PlanningRequest
    {
        public string Notes { get; set; }
    }

    public class ImplementationRequest
    {
        public string Details { get; set; }
    }

    public class SubmitApprovalRequest
    {
        public Guid EntityId { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public Guid TenantId { get; set; }
    }

    public class ManagerApprovalRequest
    {
        public string Comments { get; set; }
    }

    public class ComplianceApprovalRequest
    {
        public string Comments { get; set; }
    }

    public class ExecutiveApprovalRequest
    {
        public string Comments { get; set; }
    }

    public class RevisionRequest
    {
        public string RevisionNotes { get; set; }
    }
}
