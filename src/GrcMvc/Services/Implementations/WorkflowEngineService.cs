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

        // Cache keys and expiration settings
        private const string WorkflowDefinitionCacheKey = "WorkflowDef_";
        private const string WorkflowStatsCacheKey = "WorkflowStats_";
        private static readonly TimeSpan DefinitionCacheExpiration = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan StatsCacheExpiration = TimeSpan.FromMinutes(2);

        public WorkflowEngineService(GrcDbContext context, ILogger<WorkflowEngineService> logger, IMemoryCache cache)
        {
            _context = context;
            _logger = logger;
            _cache = cache;
        }

        // ============ Workflow Creation & Initialization ============

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
        /// </summary>
        private async Task<WorkflowDefinition?> GetWorkflowDefinitionAsync(Guid tenantId, Guid definitionId)
        {
            var cacheKey = $"{WorkflowDefinitionCacheKey}{tenantId}_{definitionId}";

            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.SlidingExpiration = DefinitionCacheExpiration;
                _logger.LogDebug("Cache miss for workflow definition {DefinitionId}", definitionId);

                return await _context.WorkflowDefinitions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.Id == definitionId && d.TenantId == tenantId);
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
            try
            {
                var task = await _context.WorkflowTasks
                    .FirstOrDefaultAsync(t => t.Id == taskId && t.TenantId == tenantId);

                if (task == null)
                    return false;

                task.Status = "Completed";
                task.CompletedAt = DateTime.UtcNow;

                _context.WorkflowTasks.Update(task);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Completed task {taskId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error completing task: {ex.Message}");
                return false;
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
