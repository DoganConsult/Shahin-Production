using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using GrcMvc.Services;
using GrcMvc.Data;
using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Localization;
using GrcMvc.Resources;
namespace GrcMvc.Controllers.Api
{
    /// <summary>
    /// REST API Controller for User Inbox Management
    /// Provides task inbox, pending actions, and task operations
    /// 
    /// ASP.NET Best Practice: RESTful endpoints with proper authorization
    /// ABP Pattern: Service-based controller with proper DI
    /// </summary>
    [ApiController]
    [Route("api/inbox")]
    [Authorize]
    public class InboxApiController : ControllerBase
    {
        private readonly IInboxService _inboxService;
        private readonly GrcDbContext _context;
        private readonly ILogger<InboxApiController> _logger;

        public InboxApiController(
            IInboxService inboxService,
            GrcDbContext context,
            ILogger<InboxApiController> logger)
        {
            _inboxService = inboxService;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// GET /api/inbox - Get current user's inbox
        /// Returns all pending tasks, approvals, and action items
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUserInbox()
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, error = "User ID not found" });

                var tenantId = GetUserTenantId();
                if (tenantId == Guid.Empty)
                    return BadRequest(new { success = false, error = "Tenant ID not found" });

                var inbox = await _inboxService.GetUserInboxAsync(userId, tenantId);

                return Ok(new
                {
                    success = true,
                    data = inbox
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user inbox");
                return StatusCode(500, new { success = false, error = "Error retrieving inbox" });
            }
        }

        /// <summary>
        /// GET /api/inbox/pending - Get pending actions for current user
        /// </summary>
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingActions()
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, error = "User ID not found" });

                var tenantId = GetUserTenantId();
                if (tenantId == Guid.Empty)
                    return BadRequest(new { success = false, error = "Tenant ID not found" });

                var pendingActions = await _inboxService.GetPendingActionsAsync(userId, tenantId);

                return Ok(new
                {
                    success = true,
                    data = pendingActions,
                    count = pendingActions.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending actions");
                return StatusCode(500, new { success = false, error = "Error retrieving pending actions" });
            }
        }

        /// <summary>
        /// GET /api/inbox/task/{taskId} - Get task process card
        /// </summary>
        [HttpGet("task/{taskId:guid}/process-card")]
        public async Task<IActionResult> GetTaskProcessCard(Guid taskId)
        {
            try
            {
                var task = await _context.WorkflowTasks.FindAsync(taskId);
                if (task == null)
                    return NotFound(new { success = false, error = "Task not found" });

                var processCard = await _inboxService.GetProcessCardAsync(task.WorkflowInstanceId);
                if (processCard == null)
                    return NotFound(new { success = false, error = "Process card not found" });

                return Ok(new
                {
                    success = true,
                    data = processCard
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting process card for task {TaskId}", taskId);
                return StatusCode(500, new { success = false, error = "Error retrieving process card" });
            }
        }

        /// <summary>
        /// GET /api/inbox/task/{taskId}/sla - Get SLA status for a task
        /// </summary>
        [HttpGet("task/{taskId:guid}/sla")]
        public async Task<IActionResult> GetTaskSlaStatus(Guid taskId)
        {
            try
            {
                var task = await _context.WorkflowTasks.FindAsync(taskId);
                if (task == null)
                    return NotFound(new { success = false, error = "Task not found" });

                var slaStatus = await _inboxService.GetTaskSlaStatusAsync(taskId);

                return Ok(new
                {
                    success = true,
                    data = slaStatus
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting SLA status for task {TaskId}", taskId);
                return StatusCode(500, new { success = false, error = "Error retrieving SLA status" });
            }
        }

        /// <summary>
        /// POST /api/inbox/task/{taskId}/approve - Approve a task
        /// </summary>
        [HttpPost("task/{taskId:guid}/approve")]
        public async Task<IActionResult> ApproveTask(Guid taskId, [FromBody] TaskActionInputDto? input)
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name;
                var userName = User.FindFirst("name")?.Value ?? User.Identity?.Name ?? "Unknown";

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, error = "User ID not found" });

                var task = await _context.WorkflowTasks.FindAsync(taskId);
                if (task == null)
                    return NotFound(new { success = false, error = "Task not found" });

                await _inboxService.UpdateTaskStatusAsync(taskId, "Approved", userId, userName, input?.Comments);

                _logger.LogInformation("Task {TaskId} approved by {UserId}", taskId, userId);

                return Ok(new
                {
                    success = true,
                    message = "Task approved successfully",
                    messageAr = "تمت الموافقة على المهمة بنجاح"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving task {TaskId}", taskId);
                return StatusCode(500, new { success = false, error = "Error approving task" });
            }
        }

        /// <summary>
        /// POST /api/inbox/task/{taskId}/reject - Reject a task
        /// </summary>
        [HttpPost("task/{taskId:guid}/reject")]
        public async Task<IActionResult> RejectTask(Guid taskId, [FromBody] TaskActionInputDto? input)
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name;
                var userName = User.FindFirst("name")?.Value ?? User.Identity?.Name ?? "Unknown";

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, error = "User ID not found" });

                var task = await _context.WorkflowTasks.FindAsync(taskId);
                if (task == null)
                    return NotFound(new { success = false, error = "Task not found" });

                if (string.IsNullOrEmpty(input?.Comments))
                    return BadRequest(new { success = false, error = "Rejection reason is required" });

                await _inboxService.UpdateTaskStatusAsync(taskId, "Rejected", userId, userName, input.Comments);

                _logger.LogInformation("Task {TaskId} rejected by {UserId}", taskId, userId);

                return Ok(new
                {
                    success = true,
                    message = "Task rejected successfully",
                    messageAr = "تم رفض المهمة"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting task {TaskId}", taskId);
                return StatusCode(500, new { success = false, error = "Error rejecting task" });
            }
        }

        /// <summary>
        /// POST /api/inbox/task/{taskId}/escalate - Escalate a task
        /// </summary>
        [HttpPost("task/{taskId:guid}/escalate")]
        public async Task<IActionResult> EscalateTask(Guid taskId, [FromBody] TaskActionInputDto? input)
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name;
                var userName = User.FindFirst("name")?.Value ?? User.Identity?.Name ?? "Unknown";

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, error = "User ID not found" });

                var task = await _context.WorkflowTasks.FindAsync(taskId);
                if (task == null)
                    return NotFound(new { success = false, error = "Task not found" });

                await _inboxService.UpdateTaskStatusAsync(taskId, "Escalated", userId, userName, input?.Comments);

                _logger.LogInformation("Task {TaskId} escalated by {UserId}", taskId, userId);

                return Ok(new
                {
                    success = true,
                    message = "Task escalated successfully",
                    messageAr = "تم تصعيد المهمة"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error escalating task {TaskId}", taskId);
                return StatusCode(500, new { success = false, error = "Error escalating task" });
            }
        }

        /// <summary>
        /// POST /api/inbox/task/{taskId}/comment - Add comment to a task
        /// </summary>
        [HttpPost("task/{taskId:guid}/comment")]
        public async Task<IActionResult> AddTaskComment(Guid taskId, [FromBody] TaskCommentInputDto input)
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name;
                var userName = User.FindFirst("name")?.Value ?? User.Identity?.Name ?? "Unknown";

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, error = "User ID not found" });

                if (string.IsNullOrEmpty(input?.Comment))
                    return BadRequest(new { success = false, error = "Comment is required" });

                var task = await _context.WorkflowTasks.FindAsync(taskId);
                if (task == null)
                    return NotFound(new { success = false, error = "Task not found" });

                await _inboxService.AddTaskCommentAsync(taskId, userId, userName, input.Comment);

                _logger.LogInformation("Comment added to task {TaskId} by {UserId}", taskId, userId);

                return Ok(new
                {
                    success = true,
                    message = "Comment added successfully",
                    messageAr = "تمت إضافة التعليق"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comment to task {TaskId}", taskId);
                return StatusCode(500, new { success = false, error = "Error adding comment" });
            }
        }

        /// <summary>
        /// GET /api/inbox/task/{taskId}/comments - Get comments for a task
        /// </summary>
        [HttpGet("task/{taskId:guid}/comments")]
        public async Task<IActionResult> GetTaskComments(Guid taskId)
        {
            try
            {
                var task = await _context.WorkflowTasks.FindAsync(taskId);
                if (task == null)
                    return NotFound(new { success = false, error = "Task not found" });

                var comments = await _inboxService.GetTaskCommentsAsync(taskId);

                return Ok(new
                {
                    success = true,
                    data = comments,
                    count = comments.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting comments for task {TaskId}", taskId);
                return StatusCode(500, new { success = false, error = "Error retrieving comments" });
            }
        }

        #region Helper Methods

        private Guid GetUserTenantId()
        {
            var tenantClaim = User.FindFirst("tenant_id")?.Value 
                            ?? User.FindFirst("TenantId")?.Value;

            if (Guid.TryParse(tenantClaim, out var tenantId))
                return tenantId;

            return Guid.Empty;
        }

        #endregion
    }

    #region Input DTOs

    /// <summary>
    /// DTO for task action requests (approve, reject, escalate)
    /// </summary>
    public class TaskActionInputDto
    {
        /// <summary>Comments or reason for the action</summary>
        public string? Comments { get; set; }
    }

    /// <summary>
    /// DTO for adding comments to tasks
    /// </summary>
    public class TaskCommentInputDto
    {
        /// <summary>Comment text (required)</summary>
        public string Comment { get; set; } = string.Empty;
    }

    #endregion
}
