using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Implementation of workflow audit service
    /// Records all workflow events for audit trail.
    /// 
    /// IMPORTANT: Audit failures are now tracked and reported, not silently swallowed.
    /// - Failures are logged at Error level with full context
    /// - A metric/counter tracks audit failures for alerting
    /// - The AuditResult indicates success/failure to callers
    /// </summary>
    public class WorkflowAuditService : IWorkflowAuditService
    {
        private readonly GrcDbContext _context;
        private readonly ILogger<WorkflowAuditService> _logger;
        
        // Track audit failure statistics for monitoring
        private static long _totalAuditAttempts;
        private static long _failedAuditAttempts;

        public WorkflowAuditService(
            GrcDbContext context,
            ILogger<WorkflowAuditService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get audit failure statistics for monitoring/alerting.
        /// </summary>
        public static (long Total, long Failed, double FailureRate) GetAuditStats()
        {
            var total = System.Threading.Interlocked.Read(ref _totalAuditAttempts);
            var failed = System.Threading.Interlocked.Read(ref _failedAuditAttempts);
            var rate = total > 0 ? (double)failed / total : 0.0;
            return (total, failed, rate);
        }

        public async Task RecordInstanceEventAsync(
            WorkflowInstance instance,
            string eventType,
            string? oldStatus,
            string description)
        {
            await RecordInstanceEventAsync(instance, eventType, oldStatus, description, throwOnFailure: false);
        }

        /// <summary>
        /// Record workflow instance event with option to throw on failure.
        /// </summary>
        /// <param name="throwOnFailure">If true, throws AuditFailureException on error</param>
        public async Task<AuditResult> RecordInstanceEventAsync(
            WorkflowInstance instance,
            string eventType,
            string? oldStatus,
            string description,
            bool throwOnFailure)
        {
            System.Threading.Interlocked.Increment(ref _totalAuditAttempts);
            
            try
            {
                var auditEntry = new WorkflowAuditEntry
                {
                    Id = Guid.NewGuid(),
                    TenantId = instance.TenantId,
                    WorkflowInstanceId = instance.Id,
                    EventType = eventType,
                    SourceEntity = "WorkflowInstance",
                    SourceEntityId = instance.Id,
                    OldStatus = oldStatus,
                    NewStatus = instance.Status,
                    ActingUserId = instance.InitiatedByUserId ?? Guid.Empty,
                    ActingUserName = instance.InitiatedByUserName,
                    Description = description,
                    EventTime = DateTime.UtcNow
                };

                _context.WorkflowAuditEntries.Add(auditEntry);
                await _context.SaveChangesAsync();

                _logger.LogDebug("✅ Recorded workflow instance event: {EventType} for instance {InstanceId}", 
                    eventType, instance.Id);
                    
                return AuditResult.Success(auditEntry.Id);
            }
            catch (Exception ex)
            {
                System.Threading.Interlocked.Increment(ref _failedAuditAttempts);
                
                // Log with full context for debugging and alerting
                _logger.LogError(ex, 
                    "❌ AUDIT FAILURE: Failed to record workflow instance event. " +
                    "EventType={EventType}, InstanceId={InstanceId}, TenantId={TenantId}, " +
                    "OldStatus={OldStatus}, NewStatus={NewStatus}",
                    eventType, instance.Id, instance.TenantId, oldStatus, instance.Status);

                if (throwOnFailure)
                {
                    throw new Exceptions.AuditFailureException(eventType, instance.Id, ex);
                }

                return AuditResult.Failure(ex.Message, eventType, instance.Id);
            }
        }

        public async Task RecordTaskEventAsync(
            WorkflowTask task,
            string eventType,
            string? oldStatus,
            string description)
        {
            await RecordTaskEventAsync(task, eventType, oldStatus, description, throwOnFailure: false);
        }

        /// <summary>
        /// Record workflow task event with option to throw on failure.
        /// </summary>
        /// <param name="throwOnFailure">If true, throws AuditFailureException on error</param>
        public async Task<AuditResult> RecordTaskEventAsync(
            WorkflowTask task,
            string eventType,
            string? oldStatus,
            string description,
            bool throwOnFailure)
        {
            System.Threading.Interlocked.Increment(ref _totalAuditAttempts);
            
            try
            {
                var auditEntry = new WorkflowAuditEntry
                {
                    Id = Guid.NewGuid(),
                    TenantId = task.TenantId,
                    WorkflowInstanceId = task.WorkflowInstanceId,
                    EventType = eventType,
                    SourceEntity = "WorkflowTask",
                    SourceEntityId = task.Id,
                    OldStatus = oldStatus,
                    NewStatus = task.Status,
                    ActingUserId = task.CompletedByUserId ?? Guid.Empty,
                    ActingUserName = task.AssignedToUserName,
                    Description = description,
                    EventTime = DateTime.UtcNow
                };

                _context.WorkflowAuditEntries.Add(auditEntry);
                await _context.SaveChangesAsync();

                _logger.LogDebug("✅ Recorded workflow task event: {EventType} for task {TaskId}", 
                    eventType, task.Id);
                    
                return AuditResult.Success(auditEntry.Id);
            }
            catch (Exception ex)
            {
                System.Threading.Interlocked.Increment(ref _failedAuditAttempts);
                
                // Log with full context for debugging and alerting
                _logger.LogError(ex,
                    "❌ AUDIT FAILURE: Failed to record workflow task event. " +
                    "EventType={EventType}, TaskId={TaskId}, InstanceId={InstanceId}, TenantId={TenantId}, " +
                    "OldStatus={OldStatus}, NewStatus={NewStatus}",
                    eventType, task.Id, task.WorkflowInstanceId, task.TenantId, oldStatus, task.Status);

                if (throwOnFailure)
                {
                    throw new Exceptions.AuditFailureException(eventType, task.Id, ex);
                }

                return AuditResult.Failure(ex.Message, eventType, task.Id);
            }
        }

        /// <summary>
        /// Record a generic audit event (not tied to instance or task).
        /// Useful for system events, security events, etc.
        /// </summary>
        public async Task<AuditResult> RecordGenericEventAsync(
            Guid tenantId,
            string eventType,
            string sourceEntity,
            Guid sourceEntityId,
            string description,
            Guid? actingUserId = null,
            string? actingUserName = null,
            bool throwOnFailure = false)
        {
            System.Threading.Interlocked.Increment(ref _totalAuditAttempts);
            
            try
            {
                var auditEntry = new WorkflowAuditEntry
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    EventType = eventType,
                    SourceEntity = sourceEntity,
                    SourceEntityId = sourceEntityId,
                    ActingUserId = actingUserId ?? Guid.Empty,
                    ActingUserName = actingUserName,
                    Description = description,
                    EventTime = DateTime.UtcNow
                };

                _context.WorkflowAuditEntries.Add(auditEntry);
                await _context.SaveChangesAsync();

                _logger.LogDebug("✅ Recorded generic audit event: {EventType} for {SourceEntity}/{SourceEntityId}", 
                    eventType, sourceEntity, sourceEntityId);
                    
                return AuditResult.Success(auditEntry.Id);
            }
            catch (Exception ex)
            {
                System.Threading.Interlocked.Increment(ref _failedAuditAttempts);
                
                _logger.LogError(ex,
                    "❌ AUDIT FAILURE: Failed to record generic event. " +
                    "EventType={EventType}, SourceEntity={SourceEntity}, SourceEntityId={SourceEntityId}",
                    eventType, sourceEntity, sourceEntityId);

                if (throwOnFailure)
                {
                    throw new Exceptions.AuditFailureException(eventType, sourceEntityId, ex);
                }

                return AuditResult.Failure(ex.Message, eventType, sourceEntityId);
            }
        }
    }

    /// <summary>
    /// Result of an audit operation, indicating success or failure with details.
    /// </summary>
    public class AuditResult
    {
        public bool IsSuccess { get; private set; }
        public Guid? AuditEntryId { get; private set; }
        public string? ErrorMessage { get; private set; }
        public string? EventType { get; private set; }
        public Guid? EntityId { get; private set; }

        public static AuditResult Success(Guid auditEntryId)
        {
            return new AuditResult
            {
                IsSuccess = true,
                AuditEntryId = auditEntryId
            };
        }

        public static AuditResult Failure(string errorMessage, string eventType, Guid? entityId)
        {
            return new AuditResult
            {
                IsSuccess = false,
                ErrorMessage = errorMessage,
                EventType = eventType,
                EntityId = entityId
            };
        }
    }
}
