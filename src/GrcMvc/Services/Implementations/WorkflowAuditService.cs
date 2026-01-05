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
    /// Records all workflow events for audit trail
    /// </summary>
    public class WorkflowAuditService : IWorkflowAuditService
    {
        private readonly GrcDbContext _context;
        private readonly ILogger<WorkflowAuditService> _logger;

        public WorkflowAuditService(
            GrcDbContext context,
            ILogger<WorkflowAuditService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task RecordInstanceEventAsync(
            WorkflowInstance instance,
            string eventType,
            string? oldStatus,
            string description)
        {
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

                _logger.LogDebug("Recorded workflow instance event: {EventType} for instance {InstanceId}", 
                    eventType, instance.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording workflow instance event: {EventType}", eventType);
                // Don't throw - audit failures shouldn't break workflow execution
            }
        }

        public async Task RecordTaskEventAsync(
            WorkflowTask task,
            string eventType,
            string? oldStatus,
            string description)
        {
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

                _logger.LogDebug("Recorded workflow task event: {EventType} for task {TaskId}", 
                    eventType, task.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording workflow task event: {EventType}", eventType);
                // Don't throw - audit failures shouldn't break workflow execution
            }
        }
    }
}
