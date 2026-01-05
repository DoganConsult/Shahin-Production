using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GrcMvc.Models.Entities;
using GrcMvc.Data;

namespace GrcMvc.Services
{
    /// <summary>
    /// Inbox Service - Manages user actions, approvals, and workflow tasks
    /// Similar to Microsoft Dynamics Flow process cards
    /// </summary>
    public interface IInboxService
    {
        Task<UserInboxViewModel> GetUserInboxAsync(string userId, Guid tenantId);
        Task<WorkflowProcessCardViewModel> GetProcessCardAsync(Guid workflowInstanceId);
        Task<List<InboxActionItemViewModel>> GetPendingActionsAsync(string userId, Guid tenantId);
        Task<SlaStatusViewModel> GetTaskSlaStatusAsync(Guid taskId);
        Task AddTaskCommentAsync(Guid taskId, string userId, string userName, string comment);
        Task<List<TaskCommentViewModel>> GetTaskCommentsAsync(Guid taskId);
        Task UpdateTaskStatusAsync(Guid taskId, string status, string userId, string userName, string? comments);
    }

    public class InboxService : IInboxService
    {
        private readonly GrcDbContext _context;
        private readonly ILogger<InboxService> _logger;

        public InboxService(GrcDbContext context, ILogger<InboxService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get complete inbox for user (all pending actions, approvals, tasks)
        /// </summary>
        public async Task<UserInboxViewModel> GetUserInboxAsync(string userId, Guid tenantId)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.RoleProfile)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                    return new UserInboxViewModel();

                // Get all tasks assigned to user
                var assignedTasks = await _context.WorkflowTasks
                    .Include(t => t.WorkflowInstance)
                    .Where(t => t.AssignedToUserId.ToString() == userId &&
                               t.Status != "Completed" &&
                               t.Status != "Rejected")
                    .OrderByDescending(t => t.Priority)
                    .ThenBy(t => t.DueDate)
                    .ToListAsync();

                // Get all workflow instances user can approve
                var approvableTasks = await _context.WorkflowTasks
                    .Include(t => t.WorkflowInstance)
                    .Where(t => t.Status == "Pending" &&
                               t.TenantId == tenantId &&
                               user.RoleProfile != null &&
                               user.RoleProfile.CanApprove)
                    .OrderByDescending(t => t.Priority)
                    .ThenBy(t => t.DueDate)
                    .ToListAsync();

                // Get process cards for all active workflows
                var processTasks = await _context.WorkflowTasks
                    .Include(t => t.WorkflowInstance)
                    .Where(t => t.TenantId == tenantId && t.Status != "Completed")
                    .ToListAsync();

                var processCards = new List<WorkflowProcessCardViewModel>();
                foreach (var instanceId in processTasks.Select(t => t.WorkflowInstanceId).Distinct())
                {
                    var card = await GetProcessCardAsync(instanceId);
                    if (card != null)
                        processCards.Add(card);
                }

                // Count by status
                var pendingCount = assignedTasks.Count(t => t.Status == "Pending");
                var inProgressCount = assignedTasks.Count(t => t.Status == "InProgress");
                var overdueCount = assignedTasks.Count(t => t.DueDate < DateTime.UtcNow && t.Status != "Completed");

                return new UserInboxViewModel
                {
                    UserId = userId,
                    UserName = user.FullName,
                    UserRole = user.RoleProfile?.RoleName ?? "Unassigned",

                    PendingTasks = assignedTasks,
                    PendingCount = pendingCount,
                    InProgressCount = inProgressCount,
                    OverdueCount = overdueCount,

                    ApprovableTasks = approvableTasks,
                    ApprovableCount = approvableTasks.Count,

                    ProcessCards = processCards,
                    ProcessCount = processCards.Count,

                    LastRefreshed = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error getting user inbox: {ex.Message}");
                return new UserInboxViewModel();
            }
        }

        /// <summary>
        /// Get workflow process card (visual representation like Dynamics Flow)
        /// Shows: Process name, current step, progress, status, SLA
        /// </summary>
        public async Task<WorkflowProcessCardViewModel> GetProcessCardAsync(Guid workflowInstanceId)
        {
            try
            {
                var instance = await _context.WorkflowInstances
                    .Include(w => w.WorkflowDefinition)
                    .FirstOrDefaultAsync(w => w.Id == workflowInstanceId);

                if (instance == null)
                    return null;

                var tasks = await _context.WorkflowTasks
                    .Where(t => t.WorkflowInstanceId == workflowInstanceId)
                    .OrderBy(t => t.CreatedDate)
                    .ToListAsync();

                var totalSteps = tasks.Count;
                var completedSteps = tasks.Count(t => t.Status == "Completed");
                var currentTask = tasks.FirstOrDefault(t => t.Status == "InProgress" || t.Status == "Pending");

                // Calculate overall progress
                int progressPercentage = totalSteps > 0 ? (completedSteps * 100) / totalSteps : 0;

                // Get SLA status
                var slaStatus = currentTask != null ?
                    GetSlaStatus(currentTask.DueDate) :
                    SlaStatus.OnTrack;

                return new WorkflowProcessCardViewModel
                {
                    WorkflowInstanceId = workflowInstanceId,
                    WorkflowName = instance.WorkflowDefinition?.Name ?? "Unknown Workflow",
                    WorkflowNumber = instance.WorkflowDefinition?.WorkflowNumber ?? "N/A",
                    Status = instance.Status,

                    ProcessStage = new ProcessStageViewModel
                    {
                        TotalSteps = totalSteps,
                        CompletedSteps = completedSteps,
                        CurrentStepNumber = completedSteps + 1,
                        ProgressPercentage = progressPercentage,
                        CurrentStepName = currentTask?.TaskName ?? "Completed",
                        CurrentStepAssignee = currentTask?.AssignedToUserName ?? "N/A"
                    },

                    SlaStatus = slaStatus,
                    DaysRemaining = currentTask?.DueDate.HasValue == true ?
                        (int)(currentTask.DueDate.Value - DateTime.UtcNow).TotalDays : 0,
                    DueDate = currentTask?.DueDate,

                    StartDate = instance.StartedAt,
                    CompletedDate = instance.CompletedAt,

                    RecentActivity = tasks.OrderByDescending(t => t.CreatedDate)
                        .Take(5)
                        .Select(t => new TaskActivityViewModel
                        {
                            TaskName = t.TaskName,
                            Status = t.Status,
                            AssignedTo = t.AssignedToUserName,
                            StartedAt = t.StartedAt,
                            CompletedAt = t.CompletedAt
                        })
                        .ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error getting process card: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get all pending actions for user
        /// </summary>
        public async Task<List<InboxActionItemViewModel>> GetPendingActionsAsync(string userId, Guid tenantId)
        {
            try
            {
                var tasks = await _context.WorkflowTasks
                    .Include(t => t.WorkflowInstance)
                    .Where(t => (t.AssignedToUserId.ToString() == userId) &&
                               t.Status != "Completed" &&
                               t.Status != "Rejected")
                    .OrderByDescending(t => t.Priority)
                    .ThenBy(t => t.DueDate)
                    .ToListAsync();

                var actions = new List<InboxActionItemViewModel>();

                foreach (var task in tasks)
                {
                    var slaStatus = GetSlaStatus(task.DueDate);
                    var daysOverdue = task.DueDate < DateTime.UtcNow ?
                        (int)(DateTime.UtcNow - task.DueDate.Value).TotalDays : 0;

                    actions.Add(new InboxActionItemViewModel
                    {
                        TaskId = task.Id,
                        WorkflowInstanceId = task.WorkflowInstanceId,
                        WorkflowName = task.WorkflowInstance?.WorkflowDefinition?.WorkflowNumber ?? "Unknown",
                        TaskName = task.TaskName,
                        Description = task.Description,
                        Status = task.Status,
                        Priority = task.Priority,
                        PriorityLabel = GetPriorityLabel(task.Priority),

                        AssignedDate = task.CreatedDate,
                        DueDate = task.DueDate,
                        StartedAt = task.StartedAt,
                        DaysRemaining = task.DueDate.HasValue ?
                            (int)(task.DueDate.Value - DateTime.UtcNow).TotalDays : 0,
                        DaysOverdue = daysOverdue,

                        SlaStatus = slaStatus,
                        IsOverdue = task.DueDate < DateTime.UtcNow,

                        AssignedBy = "System",
                        Comments = task.Description ?? ""
                    });
                }

                return actions;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error getting pending actions: {ex.Message}");
                return new List<InboxActionItemViewModel>();
            }
        }

        /// <summary>
        /// Get SLA status for specific task
        /// </summary>
        public async Task<SlaStatusViewModel> GetTaskSlaStatusAsync(Guid taskId)
        {
            try
            {
                var task = await _context.WorkflowTasks.FirstOrDefaultAsync(t => t.Id == taskId);
                if (task == null)
                    return new SlaStatusViewModel();

                var slaStatus = GetSlaStatus(task.DueDate);
                var daysOverdue = task.DueDate < DateTime.UtcNow ?
                    (int)(DateTime.UtcNow - task.DueDate.Value).TotalDays : 0;
                var daysRemaining = task.DueDate.HasValue ?
                    (int)(task.DueDate.Value - DateTime.UtcNow).TotalDays : 0;

                return new SlaStatusViewModel
                {
                    TaskId = taskId,
                    TaskName = task.TaskName,
                    DueDate = task.DueDate,
                    Status = task.Status,
                    SlaStatus = slaStatus,
                    DaysRemaining = daysRemaining,
                    DaysOverdue = daysOverdue,
                    IsOverdue = task.DueDate < DateTime.UtcNow,
                    PercentageComplete = task.StartedAt.HasValue ? 50 : 0,
                    WarningThreshold = 2,
                    AlertThreshold = 0,
                    SlaBreached = daysOverdue > 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error getting SLA status: {ex.Message}");
                return new SlaStatusViewModel();
            }
        }

        /// <summary>
        /// Add comment to task
        /// </summary>
        public async Task AddTaskCommentAsync(Guid taskId, string userId, string userName, string comment)
        {
            try
            {
                var taskComment = new TaskComment
                {
                    Id = Guid.NewGuid(),
                    WorkflowTaskId = taskId,
                    CommentedByUserId = userId,
                    CommentedByUserName = userName,
                    Comment = comment,
                    CommentedAt = DateTime.UtcNow
                };

                await _context.TaskComments.AddAsync(taskComment);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Comment added to task {taskId} by {userName}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error adding comment: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get all comments for a task
        /// </summary>
        public async Task<List<TaskCommentViewModel>> GetTaskCommentsAsync(Guid taskId)
        {
            try
            {
                var comments = await _context.TaskComments
                    .Where(c => c.WorkflowTaskId == taskId)
                    .OrderByDescending(c => c.CommentedAt)
                    .ToListAsync();

                return comments.Select(c => new TaskCommentViewModel
                {
                    CommentId = c.Id,
                    TaskId = c.WorkflowTaskId,
                    CommentedBy = c.CommentedByUserName,
                    CommentedByUserId = c.CommentedByUserId,
                    Comment = c.Comment,
                    CommentedAt = c.CommentedAt,
                    AttachmentUrl = c.AttachmentUrl
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error getting comments: {ex.Message}");
                return new List<TaskCommentViewModel>();
            }
        }

        /// <summary>
        /// Update task status with comments
        /// </summary>
        public async Task UpdateTaskStatusAsync(Guid taskId, string status, string userId, string userName, string? comments)
        {
            try
            {
                var task = await _context.WorkflowTasks.FirstOrDefaultAsync(t => t.Id == taskId);
                if (task == null)
                    throw new InvalidOperationException("Task not found");

                // Update task status
                task.Status = status;
                if (status == "InProgress" && !task.StartedAt.HasValue)
                    task.StartedAt = DateTime.UtcNow;

                if (status == "Completed" || status == "Approved")
                {
                    task.CompletedAt = DateTime.UtcNow;
                    task.CompletedByUserId = Guid.Parse(userId);
                }

                _context.WorkflowTasks.Update(task);

                // Add comment if provided
                if (!string.IsNullOrEmpty(comments))
                {
                    var comment = new TaskComment
                    {
                        Id = Guid.NewGuid(),
                        WorkflowTaskId = taskId,
                        CommentedByUserId = userId,
                        CommentedByUserName = userName,
                        Comment = $"[{status}] {comments}",
                        CommentedAt = DateTime.UtcNow
                    };
                    await _context.TaskComments.AddAsync(comment);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"✅ Task {taskId} status updated to {status} by {userName}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error updating task status: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Helper: Determine SLA status based on due date
        /// </summary>
        private SlaStatus GetSlaStatus(DateTime? dueDate)
        {
            if (!dueDate.HasValue)
                return SlaStatus.NoDeadline;

            var daysRemaining = (dueDate.Value - DateTime.UtcNow).TotalDays;

            if (daysRemaining < 0)
                return SlaStatus.Breached;
            else if (daysRemaining <= 2)
                return SlaStatus.AtRisk;
            else if (daysRemaining <= 5)
                return SlaStatus.Warning;
            else
                return SlaStatus.OnTrack;
        }

        /// <summary>
        /// Helper: Convert priority number to label
        /// </summary>
        private string GetPriorityLabel(int priority)
        {
            return priority switch
            {
                4 => "🔴 Critical",
                3 => "🟠 High",
                2 => "🟡 Medium",
                1 => "🟢 Low",
                _ => "Normal"
            };
        }
    }

    // ==================== VIEW MODELS ====================

    public class UserInboxViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;

        public List<WorkflowTask> PendingTasks { get; set; } = new();
        public int PendingCount { get; set; }
        public int InProgressCount { get; set; }
        public int OverdueCount { get; set; }

        public List<WorkflowTask> ApprovableTasks { get; set; } = new();
        public int ApprovableCount { get; set; }

        public List<WorkflowProcessCardViewModel> ProcessCards { get; set; } = new();
        public int ProcessCount { get; set; }

        public DateTime LastRefreshed { get; set; }
    }

    public class WorkflowProcessCardViewModel
    {
        public Guid WorkflowInstanceId { get; set; }
        public string WorkflowName { get; set; } = string.Empty;
        public string WorkflowNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        public ProcessStageViewModel ProcessStage { get; set; } = new();
        public SlaStatus SlaStatus { get; set; }
        public int DaysRemaining { get; set; }
        public DateTime? DueDate { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? CompletedDate { get; set; }

        public List<TaskActivityViewModel> RecentActivity { get; set; } = new();
    }

    public class ProcessStageViewModel
    {
        public int TotalSteps { get; set; }
        public int CompletedSteps { get; set; }
        public int CurrentStepNumber { get; set; }
        public int ProgressPercentage { get; set; }
        public string CurrentStepName { get; set; } = string.Empty;
        public string CurrentStepAssignee { get; set; } = string.Empty;
    }

    public class TaskActivityViewModel
    {
        public string TaskName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string AssignedTo { get; set; } = string.Empty;
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    public class InboxActionItemViewModel
    {
        public Guid TaskId { get; set; }
        public Guid WorkflowInstanceId { get; set; }
        public string WorkflowName { get; set; } = string.Empty;
        public string TaskName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int Priority { get; set; }
        public string PriorityLabel { get; set; } = string.Empty;

        public DateTime AssignedDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? StartedAt { get; set; }
        public int DaysRemaining { get; set; }
        public int DaysOverdue { get; set; }

        public SlaStatus SlaStatus { get; set; }
        public bool IsOverdue { get; set; }

        public string AssignedBy { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
    }

    public class SlaStatusViewModel
    {
        public Guid TaskId { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public SlaStatus SlaStatus { get; set; }
        public int DaysRemaining { get; set; }
        public int DaysOverdue { get; set; }
        public bool IsOverdue { get; set; }
        public int PercentageComplete { get; set; }
        public int WarningThreshold { get; set; } = 2; // Days
        public int AlertThreshold { get; set; } = 0; // Days
        public bool SlaBreached { get; set; }
    }

    public class TaskCommentViewModel
    {
        public Guid CommentId { get; set; }
        public Guid TaskId { get; set; }
        public string CommentedBy { get; set; } = string.Empty;
        public string CommentedByUserId { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public DateTime CommentedAt { get; set; }
        public string? AttachmentUrl { get; set; }
    }

    public enum SlaStatus
    {
        OnTrack = 0,      // 🟢 Green - More than 5 days remaining
        Warning = 1,      // 🟡 Yellow - 2-5 days remaining
        AtRisk = 2,       // 🟠 Orange - Less than 2 days remaining
        Breached = 3,     // 🔴 Red - Overdue
        NoDeadline = 4    // ⚪ Gray - No deadline set
    }
}
