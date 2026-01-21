using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using GrcMvc.Services;
using GrcMvc.Services.Interfaces;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Localization;
using GrcMvc.Resources;
namespace GrcMvc.Controllers.Api
{
    /// <summary>
    /// REST API Controller for Workflow Management (Phase 1)
    /// Provides complete CRUD operations and workflow state management
    /// 8 Core Endpoints: GET, POST, Approve, Reject, Complete, Status, History, Stats
    /// </summary>
    [ApiController]
    [Route("api/workflows")]
    [Authorize]
    public class WorkflowApiController : ControllerBase
    {
        private readonly IWorkflowEngineService _workflowService;
        private readonly IUserWorkspaceService _workspaceService;
        private readonly IInboxService _inboxService;
        private readonly GrcDbContext _context;
        private readonly ILogger<WorkflowApiController> _logger;

        public WorkflowApiController(
            IWorkflowEngineService workflowService,
            IUserWorkspaceService workspaceService,
            IInboxService inboxService,
            GrcDbContext context,
            ILogger<WorkflowApiController> logger)
        {
            _workflowService = workflowService;
            _workspaceService = workspaceService;
            _inboxService = inboxService;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// GET /api/workflows - Get all workflows for current user
        /// Returns paginated list with task progress and status
        /// </summary>
        [HttpGet]
        [ResponseCache(Duration = 30, VaryByQueryKeys = new[] { "page", "pageSize" })]
        public async Task<IActionResult> GetUserWorkflows([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, error = "User ID not found" });

                var tenantId = GetUserTenantId();

                var workflows = await _context.WorkflowInstances
                    .Where(w => w.TenantId == tenantId)
                    .Include(w => w.WorkflowDefinition)
                    .Include(w => w.Tasks)
                    .OrderByDescending(w => w.StartedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(w => new
                    {
                        w.Id,
                        w.Status,
                        WorkflowName = w.WorkflowDefinition.Name,
                        w.StartedAt,
                        w.CompletedAt,
                        TasksCompleted = w.Tasks.Count(t => t.Status == "Completed"),
                        TotalTasks = w.Tasks.Count,
                        Progress = w.Tasks.Count > 0 ? Math.Round((decimal)w.Tasks.Count(t => t.Status == "Completed") / w.Tasks.Count * 100, 2) : 0
                    })
                    .ToListAsync();

                var total = await _context.WorkflowInstances.CountAsync(w => w.TenantId == tenantId);

                _logger.LogInformation($"✅ Retrieved {workflows.Count} workflows for user {userId}");

                return Ok(new
                {
                    success = true,
                    data = workflows,
                    pagination = new { page, pageSize, total, pages = (int)Math.Ceiling((double)total / pageSize) }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error getting workflows: {ex.Message}");
                return StatusCode(500, new { success = false, error = "An internal error occurred." });
            }
        }

        /// <summary>
        /// GET /api/workflows/{id} - Get specific workflow details
        /// Includes all tasks and their status
        /// </summary>
        [HttpGet("{id}")]
        [ResponseCache(Duration = 60)]
        public async Task<IActionResult> GetWorkflow(Guid id)
        {
            try
            {
                var tenantId = GetUserTenantId();
                var workflow = await _context.WorkflowInstances
                    .Where(w => w.Id == id && w.TenantId == tenantId)
                    .Include(w => w.WorkflowDefinition)
                    .Include(w => w.Tasks)
                    .FirstOrDefaultAsync();

                if (workflow == null)
                    return NotFound(new { success = false, error = "Workflow not found" });

                var tasks = workflow.Tasks.Select(t => new
                {
                    t.Id,
                    t.TaskName,
                    t.Status,
                    t.DueDate,
                    AssignedTo = t.AssignedToUserName,
                    t.StartedAt,
                    t.CompletedAt
                }).ToList();

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        workflow.Id,
                        workflow.Status,
                        WorkflowName = workflow.WorkflowDefinition.Name,
                        workflow.StartedAt,
                        workflow.CompletedAt,
                        TasksCompleted = workflow.Tasks.Count(t => t.Status == "Completed"),
                        TotalTasks = workflow.Tasks.Count,
                        Progress = workflow.Tasks.Count > 0 ? Math.Round((decimal)workflow.Tasks.Count(t => t.Status == "Completed") / workflow.Tasks.Count * 100, 2) : 0,
                        Tasks = tasks
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error getting workflow: {ex.Message}");
                return StatusCode(500, new { success = false, error = "An internal error occurred." });
            }
        }

        /// <summary>
        /// POST /api/workflows/start - Start new workflow instance from definition
        /// Uses enhanced StartWorkflowAsync with BPMN parsing and task creation
        /// </summary>
        [HttpPost("start")]
        public async Task<IActionResult> StartWorkflow([FromBody] StartWorkflowRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userIdClaim = User.FindFirst("sub")?.Value ?? User.Identity?.Name;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                    return Unauthorized(new { success = false, error = "User ID not found" });

                var tenantId = GetUserTenantId();
                if (tenantId == Guid.Empty)
                    return BadRequest(new { success = false, error = "Tenant ID not found" });

                // Start workflow instance with task creation
                var instance = await _workflowService.StartWorkflowAsync(
                    tenantId,
                    request.WorkflowDefinitionId,
                    userId,
                    request.InputVariables);

                _logger.LogInformation($"✅ Started workflow instance {instance.Id} with {instance.Tasks?.Count ?? 0} tasks");

                return CreatedAtAction(nameof(GetWorkflow), new { id = instance.Id }, new
                {
                    success = true,
                    message = "Workflow started successfully",
                    data = new
                    {
                        instance.Id,
                        instance.Status,
                        instance.CurrentState,
                        TaskCount = instance.Tasks?.Count ?? 0,
                        Tasks = instance.Tasks?.Select(t => new
                        {
                            t.Id,
                            t.TaskName,
                            t.Status,
                            t.AssignedToUserId,
                            t.DueDate
                        }).ToList()
                    }
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"⚠️ Workflow start failed: {ex.Message}");
                return BadRequest(new { success = false, error = "An error occurred processing your request." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error starting workflow");
                return StatusCode(500, new { success = false, error = "An internal error occurred." });
            }
        }

        /// <summary>
        /// POST /api/workflows - Create new workflow instance from definition (legacy)
        /// Initializes workflow with pending status
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateWorkflow([FromBody] CreateWorkflowRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name;
                var tenantId = GetUserTenantId();

                // Get workflow definition
                var definition = await _context.WorkflowDefinitions
                    .FirstOrDefaultAsync(d => d.Id == request.WorkflowDefinitionId && d.TenantId == tenantId);

                if (definition == null)
                    return NotFound(new { success = false, error = "Workflow definition not found" });

                // Create workflow instance
                var instance = new WorkflowInstance
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    WorkflowDefinitionId = request.WorkflowDefinitionId,
                    Status = "Pending",
                    StartedAt = DateTime.UtcNow,
                    InitiatedByUserName = userId
                };

                _context.WorkflowInstances.Add(instance);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Created workflow instance {instance.Id} from definition {definition.Name}");

                return CreatedAtAction(nameof(GetWorkflow), new { id = instance.Id }, new
                {
                    success = true,
                    message = "Workflow created successfully",
                    data = new { instance.Id, instance.Status }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error creating workflow: {ex.Message}");
                return StatusCode(500, new { success = false, error = "An internal error occurred." });
            }
        }

        /// <summary>
        /// POST /api/workflows/{id}/approve - Approve workflow (move to next approval stage)
        /// Transitions status to InApproval and logs audit entry
        /// </summary>
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> ApproveWorkflow(Guid id, [FromBody] ApprovalRequest? request)
        {
            try
            {
                var tenantId = GetUserTenantId();
                var workflow = await _context.WorkflowInstances
                    .Where(w => w.Id == id && w.TenantId == tenantId)
                    .Include(w => w.Tasks)
                    .FirstOrDefaultAsync();

                if (workflow == null)
                    return NotFound(new { success = false, error = "Workflow not found" });

                workflow.Status = "InApproval";

                var auditEntry = new WorkflowAuditEntry
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    WorkflowInstanceId = id,
                    EventType = "ApprovalApproved",
                    SourceEntity = "WorkflowInstance",
                    SourceEntityId = id,
                    OldStatus = "Pending",
                    NewStatus = "InApproval",
                    ActingUserName = User.Identity?.Name ?? "Unknown",
                    Description = request?.Reason,
                    EventTime = DateTime.UtcNow
                };

                _context.WorkflowAuditEntries.Add(auditEntry);
                _context.WorkflowInstances.Update(workflow);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Approved workflow {id}");

                return Ok(new
                {
                    success = true,
                    message = "Workflow approved successfully",
                    data = new { workflow.Id, workflow.Status }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error approving workflow: {ex.Message}");
                return StatusCode(500, new { success = false, error = "An internal error occurred." });
            }
        }

        /// <summary>
        /// POST /api/workflows/{id}/reject - Reject workflow (return to previous stage)
        /// Transitions status to Rejected with reason logged
        /// </summary>
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> RejectWorkflow(Guid id, [FromBody] ApprovalRequest? request)
        {
            try
            {
                var tenantId = GetUserTenantId();
                var workflow = await _context.WorkflowInstances
                    .Where(w => w.Id == id && w.TenantId == tenantId)
                    .FirstOrDefaultAsync();

                if (workflow == null)
                    return NotFound(new { success = false, error = "Workflow not found" });

                workflow.Status = "Rejected";

                var auditEntry = new WorkflowAuditEntry
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    WorkflowInstanceId = id,
                    EventType = "ApprovalRejected",
                    SourceEntity = "WorkflowInstance",
                    SourceEntityId = id,
                    OldStatus = "Pending",
                    NewStatus = "Rejected",
                    ActingUserName = User.Identity?.Name ?? "Unknown",
                    Description = request?.Reason ?? "No reason provided",
                    EventTime = DateTime.UtcNow
                };

                _context.WorkflowAuditEntries.Add(auditEntry);
                _context.WorkflowInstances.Update(workflow);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Rejected workflow {id}");

                return Ok(new
                {
                    success = true,
                    message = "Workflow rejected successfully",
                    data = new { workflow.Id, workflow.Status }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error rejecting workflow: {ex.Message}");
                return StatusCode(500, new { success = false, error = "An internal error occurred." });
            }
        }

        /// <summary>
        /// POST /api/workflows/{id}/task/{taskId}/complete - Complete a workflow task
        /// Uses enhanced CompleteTaskAsync with workflow evaluation
        /// </summary>
        [HttpPost("{id}/task/{taskId}/complete")]
        public async Task<IActionResult> CompleteTask(Guid id, Guid taskId, [FromBody] TaskCompleteRequest? request)
        {
            try
            {
                var userIdClaim = User.FindFirst("sub")?.Value ?? User.Identity?.Name;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                    return Unauthorized(new { success = false, error = "User ID not found" });

                var tenantId = GetUserTenantId();
                if (tenantId == Guid.Empty)
                    return BadRequest(new { success = false, error = "Tenant ID not found" });

                // Use enhanced CompleteTaskAsync with workflow evaluation
                var success = await _workflowService.CompleteTaskAsync(
                    tenantId,
                    taskId,
                    userId,
                    null, // outputData
                    request?.Notes);

                if (!success)
                    return NotFound(new { success = false, error = "Task not found or could not be completed" });

                // Get updated workflow status
                var workflow = await _workflowService.GetWorkflowAsync(tenantId, id);
                if (workflow == null)
                    return NotFound(new { success = false, error = "Workflow not found" });

                return Ok(new
                {
                    success = true,
                    message = "Task completed successfully",
                    data = new
                    {
                        TaskId = taskId,
                        WorkflowId = id,
                        WorkflowStatus = workflow.Status,
                        WorkflowComplete = workflow.Status == "Completed"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error completing task {taskId}");
                return StatusCode(500, new { success = false, error = "An internal error occurred." });
            }
        }

        /// <summary>
        /// GET /api/workflows/{id}/status - Get workflow status and progress
        /// Returns percentage completion and task breakdown
        /// </summary>
        [HttpGet("{id}/status")]
        [ResponseCache(Duration = 30)]
        public async Task<IActionResult> GetWorkflowStatus(Guid id)
        {
            try
            {
                var tenantId = GetUserTenantId();
                var workflow = await _context.WorkflowInstances
                    .Where(w => w.Id == id && w.TenantId == tenantId)
                    .Include(w => w.Tasks)
                    .FirstOrDefaultAsync();

                if (workflow == null)
                    return NotFound(new { success = false, error = "Workflow not found" });

                var completedTasks = workflow.Tasks.Count(t => t.Status == "Completed");
                var totalTasks = workflow.Tasks.Count;
                var progress = totalTasks > 0 ? Math.Round((decimal)completedTasks / totalTasks * 100, 2) : 0;

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        workflow.Id,
                        workflow.Status,
                        workflow.StartedAt,
                        workflow.CompletedAt,
                        CompletedTasks = completedTasks,
                        TotalTasks = totalTasks,
                        ProgressPercentage = progress
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error getting workflow status: {ex.Message}");
                return StatusCode(500, new { success = false, error = "An internal error occurred." });
            }
        }

        /// <summary>
        /// GET /api/workflows/{id}/history - Get workflow audit trail
        /// Shows all state changes and approvals
        /// </summary>
        [HttpGet("{id}/history")]
        [ResponseCache(Duration = 60)]
        public async Task<IActionResult> GetWorkflowHistory(Guid id)
        {
            try
            {
                var tenantId = GetUserTenantId();
                var history = await _context.WorkflowAuditEntries
                    .Where(e => e.WorkflowInstanceId == id && e.TenantId == tenantId)
                    .OrderByDescending(e => e.EventTime)
                    .Select(e => new
                    {
                        e.Id,
                        e.EventType,
                        e.ActingUserName,
                        e.Description,
                        e.EventTime
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = history });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error getting workflow history: {ex.Message}");
                return StatusCode(500, new { success = false, error = "An internal error occurred." });
            }
        }

        /// <summary>
        /// GET /api/workflows/definitions/available - Get available workflow definitions to start
        /// </summary>
        [HttpGet("definitions/available")]
        [ResponseCache(Duration = 300)]
        public async Task<IActionResult> GetAvailableWorkflowDefinitions()
        {
            try
            {
                var tenantId = GetUserTenantId();
                var definitions = await _context.WorkflowDefinitions
                    .Where(d => d.TenantId == tenantId && !d.IsDeleted)
                    .Select(d => new
                    {
                        d.Id,
                        d.Name,
                        d.WorkflowNumber,
                        d.Type,
                        d.Description
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = definitions });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error getting workflow definitions: {ex.Message}");
                return StatusCode(500, new { success = false, error = "An internal error occurred." });
            }
        }

        /// <summary>
        /// GET /api/workflows/stats/dashboard - Get workflow statistics for dashboard
        /// </summary>
        [HttpGet("stats/dashboard")]
        [ResponseCache(Duration = 60)]
        public async Task<IActionResult> GetWorkflowStats()
        {
            try
            {
                var tenantId = GetUserTenantId();

                var stats = new
                {
                    TotalWorkflows = await _context.WorkflowInstances.CountAsync(w => w.TenantId == tenantId),
                    ActiveWorkflows = await _context.WorkflowInstances.CountAsync(w => w.TenantId == tenantId && (w.Status == "InProgress" || w.Status == "InApproval")),
                    PendingWorkflows = await _context.WorkflowInstances.CountAsync(w => w.TenantId == tenantId && w.Status == "Pending"),
                    CompletedWorkflows = await _context.WorkflowInstances.CountAsync(w => w.TenantId == tenantId && w.Status == "Completed"),
                    RejectedWorkflows = await _context.WorkflowInstances.CountAsync(w => w.TenantId == tenantId && w.Status == "Rejected")
                };

                return Ok(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error getting workflow stats: {ex.Message}");
                return StatusCode(500, new { success = false, error = "An internal error occurred." });
            }
        }

        // Helper method to get user's tenant ID
        private Guid GetUserTenantId()
        {
            var tenantId = User.FindFirst("tenant_id")?.Value;
            return Guid.TryParse(tenantId, out var id) ? id : Guid.Empty;
        }
    }

    // ============ Request/Response DTOs ============

    /// <summary>
    /// Request to start a new workflow instance (enhanced with BPMN parsing)
    /// </summary>
    public class StartWorkflowRequest
    {
        public Guid WorkflowDefinitionId { get; set; }
        public Dictionary<string, object>? InputVariables { get; set; }
    }

    /// <summary>
    /// Request to create a new workflow instance (legacy)
    /// </summary>
    public class CreateWorkflowRequest
    {
        public Guid WorkflowDefinitionId { get; set; }
        public string? Priority { get; set; } = "Medium";
        public DateTime? DueDate { get; set; }
    }

    /// <summary>
    /// Request for approval/rejection of workflows
    /// </summary>
    public class ApprovalRequest
    {
        public string? Reason { get; set; }
    }

    /// <summary>
    /// Request to complete a workflow task
    /// </summary>
    public class TaskCompleteRequest
    {
        public string? Notes { get; set; }
    }
}
