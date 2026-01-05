using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Implementation of Workflow Engine Service
    /// Orchestrates workflow execution, state transitions, and task management
    /// Now with IMemoryCache for improved performance
    /// </summary>
    public class WorkflowEngineService : IWorkflowEngineService
    {
        private readonly GrcDbContext _context;
        private readonly ILogger<WorkflowEngineService> _logger;
        private readonly IMemoryCache _cache;
        private readonly BpmnParser _bpmnParser;
        private readonly WorkflowAssigneeResolver _assigneeResolver;
        private readonly IWorkflowAuditService _auditService;

        // Cache keys and expiration settings
        private const string WorkflowDefinitionCacheKey = "WorkflowDef_";
        private const string WorkflowStatsCacheKey = "WorkflowStats_";
        private static readonly TimeSpan DefinitionCacheExpiration = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan StatsCacheExpiration = TimeSpan.FromMinutes(2);

        public WorkflowEngineService(
            GrcDbContext context,
            ILogger<WorkflowEngineService> logger,
            IMemoryCache cache,
            BpmnParser bpmnParser,
            WorkflowAssigneeResolver assigneeResolver,
            IWorkflowAuditService auditService)
        {
            _context = context;
            _logger = logger;
            _cache = cache;
            _bpmnParser = bpmnParser;
            _assigneeResolver = assigneeResolver;
            _auditService = auditService;
        }

        // ============ Workflow Creation & Initialization ============

        /// <summary>
        /// Start a workflow instance from a definition with BPMN parsing and task creation
        /// Implements the specification: StartWorkflowAsync with full task creation
        /// </summary>
        public async Task<WorkflowInstance> StartWorkflowAsync(
            Guid tenantId,
            Guid definitionId,
            Guid? initiatedByUserId,
            Dictionary<string, object>? inputVariables = null)
        {
            // STEP 1: Validate workflow definition is active
            var definition = await GetWorkflowDefinitionAsync(tenantId, definitionId);
            if (definition == null)
                throw new InvalidOperationException($"Workflow definition {definitionId} not found");

            if (definition.Status != "Active" && definition.IsActive != true)
                throw new InvalidOperationException($"Workflow definition {definitionId} is not active");

            // STEP 2: Create instance with tenant isolation
            var instance = new WorkflowInstance
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                WorkflowDefinitionId = definitionId,
                Status = "Pending",
                CurrentState = "Pending",
                StartedAt = DateTime.UtcNow,
                InitiatedByUserId = initiatedByUserId,
                Variables = inputVariables != null ? JsonSerializer.Serialize(inputVariables) : null
            };

            // STEP 3: Start instance (Pending → InProgress)
            instance.Status = "InProgress";
            instance.CurrentState = "InProgress";

            // STEP 4: Parse BPMN XML and create tasks
            var bpmnWorkflow = !string.IsNullOrWhiteSpace(definition.BpmnXml)
                ? _bpmnParser.Parse(definition.BpmnXml)
                : ParseStepsFromJson(definition.Steps);

            if (bpmnWorkflow.Steps.Any())
            {
                foreach (var step in bpmnWorkflow.Steps.Where(s => s.Type == BpmnStepType.Task))
                {
                    // Validate assignee exists (multi-team support via role-based assignment)
                    // AssigneeRule is available in step definitions but not in parsed BPMN steps
                    // For now, use role-based resolution which supports multi-team via roles
                    var assigneeUserId = await _assigneeResolver.ResolveAssigneeAsync(
                        tenantId,
                        step.Assignee ?? definition.DefaultAssignee,
                        initiatedByUserId);

                    if (!assigneeUserId.HasValue)
                    {
                        _logger.LogWarning("Could not resolve assignee for step {StepName}, skipping", step.Name);
                        continue;
                    }

                    // Create task with SLA
                    var dueDate = step.DueDateOffsetDays.HasValue
                        ? DateTime.UtcNow.AddDays(step.DueDateOffsetDays.Value)
                        : (DateTime?)null;

                    var task = new WorkflowTask
                    {
                        Id = Guid.NewGuid(),
                        TenantId = tenantId,
                        WorkflowInstanceId = instance.Id,
                        TaskName = step.Name,
                        Description = step.Description ?? string.Empty,
                        AssignedToUserId = assigneeUserId.Value,
                        Status = "Pending",
                        Priority = step.Priority,
                        DueDate = dueDate,
                        StartedAt = null,
                        CompletedAt = null
                    };

                    instance.Tasks.Add(task);
                }
            }

            // STEP 5: Persist to database
            _context.WorkflowInstances.Add(instance);
            await _context.SaveChangesAsync();

            // STEP 6: Record audit trail
            await _auditService.RecordInstanceEventAsync(
                instance,
                "InstanceStarted",
                null,
                $"Workflow started with {instance.Tasks.Count} tasks");

            _logger.LogInformation("✅ Started workflow {InstanceId} from definition {DefinitionName} with {TaskCount} tasks",
                instance.Id, definition.Name, instance.Tasks.Count);

            return instance;
        }

        /// <summary>
        /// Parse steps from JSON fallback (when BPMN XML is not available)
        /// </summary>
        private BpmnWorkflow ParseStepsFromJson(string stepsJson)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(stepsJson) || stepsJson == "[]")
                    return new BpmnWorkflow { Steps = new List<BpmnStep>() };

                var stepDefinitions = JsonSerializer.Deserialize<List<WorkflowStepDefinition>>(stepsJson);
                if (stepDefinitions == null)
                    return new BpmnWorkflow { Steps = new List<BpmnStep>() };

                var steps = stepDefinitions.Select(s => new BpmnStep
                {
                    Id = s.id,
                    Name = s.name,
                    Type = s.type switch
                    {
                        "startEvent" => BpmnStepType.Start,
                        "endEvent" => BpmnStepType.End,
                        _ => BpmnStepType.Task
                    },
                    Sequence = s.stepNumber,
                    Assignee = s.assignee,
                    DueDateOffsetDays = s.daysToComplete,
                    Priority = 2, // Default medium
                    Description = s.description
                }).ToList();

                return new BpmnWorkflow { Steps = steps };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing steps from JSON");
                return new BpmnWorkflow { Steps = new List<BpmnStep>() };
            }
        }

        /// <summary>
        /// WorkflowStepDefinition for JSON deserialization
        /// </summary>
        private class WorkflowStepDefinition
        {
            public string id { get; set; } = string.Empty;
            public string name { get; set; } = string.Empty;
            public string type { get; set; } = string.Empty;
            public int stepNumber { get; set; }
            public string? assignee { get; set; }
            public int? daysToComplete { get; set; }
            public string? description { get; set; }
        }

        public async Task<WorkflowInstance> CreateWorkflowAsync(Guid tenantId, Guid definitionId, string priority = "Medium", string createdBy = "System")
        {
            try
            {
                // Use cached definition lookup
                var definition = await GetWorkflowDefinitionAsync(tenantId, definitionId);

                if (definition == null)
                    throw new InvalidOperationException($"Workflow definition {definitionId} not found");

                var instance = new WorkflowInstance
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    WorkflowDefinitionId = definitionId,
                    Status = "Pending",
                    StartedAt = DateTime.UtcNow,
                    InitiatedByUserName = createdBy
                };

                _context.WorkflowInstances.Add(instance);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Created workflow {instance.Id} from definition {definition.Name}");
                return instance;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error creating workflow: {ex.Message}");
                throw;
            }
        }

        // ============ Cached Definition Lookup ============

        /// <summary>
        /// Get workflow definition with caching (10 minute expiration)
        /// Supports both tenant-specific and global (TenantId = null) definitions
        /// </summary>
        private async Task<WorkflowDefinition?> GetWorkflowDefinitionAsync(Guid tenantId, Guid definitionId)
        {
            var cacheKey = $"{WorkflowDefinitionCacheKey}{tenantId}_{definitionId}";

            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.SlidingExpiration = DefinitionCacheExpiration;
                _logger.LogDebug("Cache miss for workflow definition {DefinitionId}", definitionId);

                // Support both tenant-specific and global (TenantId = null) definitions
                return await _context.WorkflowDefinitions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.Id == definitionId && (d.TenantId == tenantId || d.TenantId == null));
            });
        }

        /// <summary>
        /// Invalidate workflow definition cache
        /// </summary>
        public void InvalidateDefinitionCache(Guid tenantId, Guid definitionId)
        {
            var cacheKey = $"{WorkflowDefinitionCacheKey}{tenantId}_{definitionId}";
            _cache.Remove(cacheKey);
            _logger.LogDebug("Cache invalidated for workflow definition {DefinitionId}", definitionId);
        }

        // ============ Workflow Execution & Retrieval ============

        public async Task<WorkflowInstance> GetWorkflowAsync(Guid tenantId, Guid workflowId)
        {
            return await _context.WorkflowInstances
                .AsNoTracking()
                .Where(w => w.Id == workflowId && w.TenantId == tenantId)
                .Include(w => w.WorkflowDefinition)
                .Include(w => w.Tasks)
                .FirstOrDefaultAsync();
        }

        public async Task<List<WorkflowInstance>> GetUserWorkflowsAsync(Guid tenantId, int page = 1, int pageSize = 20)
        {
            return await _context.WorkflowInstances
                .AsNoTracking()
                .Where(w => w.TenantId == tenantId)
                .Include(w => w.WorkflowDefinition)
                .Include(w => w.Tasks)
                .OrderByDescending(w => w.StartedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // ============ Workflow State Transitions ============

        public async Task<bool> ApproveWorkflowAsync(Guid tenantId, Guid workflowId, string reason = "", string approvedBy = "")
        {
            try
            {
                var workflow = await _context.WorkflowInstances
                    .FirstOrDefaultAsync(w => w.Id == workflowId && w.TenantId == tenantId);

                if (workflow == null)
                    return false;

                workflow.Status = "InApproval";

                var auditEntry = new WorkflowAuditEntry
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    WorkflowInstanceId = workflowId,
                    EventType = "ApprovalApproved",
                    SourceEntity = "WorkflowInstance",
                    SourceEntityId = workflowId,
                    OldStatus = "Pending",
                    NewStatus = "InApproval",
                    ActingUserName = approvedBy,
                    Description = $"Workflow approved. Reason: {reason}",
                    EventTime = DateTime.UtcNow
                };

                _context.WorkflowAuditEntries.Add(auditEntry);
                _context.WorkflowInstances.Update(workflow);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Approved workflow {workflowId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error approving workflow: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RejectWorkflowAsync(Guid tenantId, Guid workflowId, string reason = "", string rejectedBy = "")
        {
            try
            {
                var workflow = await _context.WorkflowInstances
                    .FirstOrDefaultAsync(w => w.Id == workflowId && w.TenantId == tenantId);

                if (workflow == null)
                    return false;

                workflow.Status = "Rejected";

                var auditEntry = new WorkflowAuditEntry
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    WorkflowInstanceId = workflowId,
                    EventType = "ApprovalRejected",
                    SourceEntity = "WorkflowInstance",
                    SourceEntityId = workflowId,
                    OldStatus = "Pending",
                    NewStatus = "Rejected",
                    ActingUserName = rejectedBy,
                    Description = $"Workflow rejected. Reason: {reason}",
                    EventTime = DateTime.UtcNow
                };

                _context.WorkflowAuditEntries.Add(auditEntry);
                _context.WorkflowInstances.Update(workflow);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Rejected workflow {workflowId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error rejecting workflow: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CompleteWorkflowAsync(Guid tenantId, Guid workflowId)
        {
            try
            {
                var workflow = await _context.WorkflowInstances
                    .FirstOrDefaultAsync(w => w.Id == workflowId && w.TenantId == tenantId);

                if (workflow == null)
                    return false;

                workflow.Status = "Completed";
                workflow.CompletedAt = DateTime.UtcNow;

                _context.WorkflowInstances.Update(workflow);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Completed workflow {workflowId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error completing workflow: {ex.Message}");
                return false;
            }
        }

        // ============ Task Management ============

        public async Task<WorkflowTask> GetTaskAsync(Guid tenantId, Guid taskId)
        {
            return await _context.WorkflowTasks
                .AsNoTracking()
                .Where(t => t.Id == taskId && t.TenantId == tenantId)
                .FirstOrDefaultAsync();
        }

        public async Task<List<WorkflowTask>> GetWorkflowTasksAsync(Guid tenantId, Guid workflowId)
        {
            return await _context.WorkflowTasks
                .AsNoTracking()
                .Where(t => t.WorkflowInstanceId == workflowId && t.TenantId == tenantId)
                .OrderBy(t => t.Id)
                .ToListAsync();
        }

        public async Task<bool> CompleteTaskAsync(Guid tenantId, Guid taskId, string notes = "")
        {
            // Legacy method - calls the enhanced version
            return await CompleteTaskAsync(tenantId, taskId, Guid.Empty, null, notes);
        }

        /// <summary>
        /// Complete a task and evaluate workflow completion
        /// Implements the specification: CompleteTaskAsync with workflow evaluation
        /// </summary>
        public async Task<bool> CompleteTaskAsync(
            Guid tenantId,
            Guid taskId,
            Guid userId,
            Dictionary<string, object>? outputData = null,
            string? notes = null)
        {
            try
            {
                var instance = await _context.WorkflowInstances
                    .Include(i => i.Tasks)
                    .FirstOrDefaultAsync(i => i.Tasks.Any(t => t.Id == taskId) && i.TenantId == tenantId);

                if (instance == null)
                {
                    _logger.LogWarning("Workflow instance not found for task {TaskId}", taskId);
                    return false;
                }

                var task = instance.Tasks.FirstOrDefault(t => t.Id == taskId);
                if (task == null)
                {
                    _logger.LogWarning("Task {TaskId} not found", taskId);
                    return false;
                }

                // Mark task as Approved (completed successfully)
                var oldStatus = task.Status;
                task.Status = "Approved"; // Using "Approved" as per spec (not "Completed")
                task.CompletedAt = DateTime.UtcNow;
                task.CompletedByUserId = userId;
                task.CompletionNotes = notes;

                // Record task completion audit
                await _auditService.RecordTaskEventAsync(
                    task,
                    "TaskCompleted",
                    oldStatus,
                    $"Task completed by user {userId}. Notes: {notes ?? "None"}");

                // Evaluate if workflow should complete
                await EvaluateWorkflowCompletionAsync(instance);

                _context.WorkflowInstances.Update(instance);
                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ Completed task {TaskId} for workflow {InstanceId}", taskId, instance.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error completing task {TaskId}", taskId);
                return false;
            }
        }

        /// <summary>
        /// Evaluate workflow completion based on task statuses
        /// Implements the specification: EvaluateNextStepsWithAiAsync logic
        /// </summary>
        private async Task EvaluateWorkflowCompletionAsync(WorkflowInstance instance)
        {
            // Count task statuses
            var pendingOrInProgressTasks = instance.Tasks.Count(t =>
                t.Status == "Pending" || t.Status == "InProgress");
            var rejectedTasks = instance.Tasks.Count(t => t.Status == "Rejected");
            var approvedTasks = instance.Tasks.Count(t => t.Status == "Approved");

            // RULE 1: If any tasks rejected and no pending work → REJECT workflow
            if (rejectedTasks > 0 && pendingOrInProgressTasks == 0)
            {
                instance.Status = "Rejected";
                instance.CurrentState = "Rejected";
                instance.CompletedAt = DateTime.UtcNow;

                await _auditService.RecordInstanceEventAsync(
                    instance,
                    "InstanceRejected",
                    "InProgress",
                    $"Workflow rejected: {rejectedTasks} task(s) were rejected");

                _logger.LogInformation("Workflow {InstanceId} rejected: {RejectedTasks} task(s) were rejected",
                    instance.Id, rejectedTasks);
                return;
            }

            // RULE 2: If all tasks done (no pending) and at least one task → COMPLETE workflow
            if (pendingOrInProgressTasks == 0 && instance.Tasks.Any())
            {
                instance.Status = "Completed";
                instance.CurrentState = "Completed";
                instance.CompletedAt = DateTime.UtcNow;
                instance.CompletedByUserId = instance.InitiatedByUserId;

                await _auditService.RecordInstanceEventAsync(
                    instance,
                    "InstanceCompleted",
                    "InProgress",
                    $"Workflow completed successfully with {approvedTasks} approved task(s)");

                _logger.LogInformation("Workflow {InstanceId} completed successfully with {ApprovedTasks} approved task(s)",
                    instance.Id, approvedTasks);
            }
            // RULE 3: Still have pending tasks → continue workflow
            else if (pendingOrInProgressTasks > 0)
            {
                _logger.LogDebug("Workflow {InstanceId} has {PendingTasks} pending task(s), continuing",
                    instance.Id, pendingOrInProgressTasks);
            }
        }

        // ============ Statistics (Cached) ============

        public async Task<WorkflowStats> GetStatisticsAsync(Guid tenantId)
        {
            var cacheKey = $"{WorkflowStatsCacheKey}{tenantId}";

            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = StatsCacheExpiration;
                _logger.LogDebug("Cache miss for workflow statistics, tenant {TenantId}", tenantId);

                var stats = new WorkflowStats
                {
                    TotalWorkflows = await _context.WorkflowInstances.CountAsync(w => w.TenantId == tenantId),
                    ActiveWorkflows = await _context.WorkflowInstances.CountAsync(w => w.TenantId == tenantId && (w.Status == "InProgress" || w.Status == "InApproval")),
                    PendingWorkflows = await _context.WorkflowInstances.CountAsync(w => w.TenantId == tenantId && w.Status == "Pending"),
                    CompletedWorkflows = await _context.WorkflowInstances.CountAsync(w => w.TenantId == tenantId && w.Status == "Completed"),
                    RejectedWorkflows = await _context.WorkflowInstances.CountAsync(w => w.TenantId == tenantId && w.Status == "Rejected")
                };

                // Calculate average completion time
                var completedWorkflows = await _context.WorkflowInstances
                    .AsNoTracking()
                    .Where(w => w.TenantId == tenantId && w.Status == "Completed" && w.CompletedAt.HasValue)
                    .ToListAsync();

                if (completedWorkflows.Count > 0)
                {
                    var totalHours = completedWorkflows
                        .Sum(w => (w.CompletedAt.Value - w.StartedAt).TotalHours);
                    stats.AverageCompletionTimeHours = totalHours / completedWorkflows.Count;
                }

                return stats;
            }) ?? new WorkflowStats();
        }

        /// <summary>
        /// Invalidate statistics cache (call after workflow state changes)
        /// </summary>
        public void InvalidateStatsCache(Guid tenantId)
        {
            var cacheKey = $"{WorkflowStatsCacheKey}{tenantId}";
            _cache.Remove(cacheKey);
            _logger.LogDebug("Cache invalidated for workflow statistics, tenant {TenantId}", tenantId);
        }
    }
}
