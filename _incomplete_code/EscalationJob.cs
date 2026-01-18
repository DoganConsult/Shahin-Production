using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Implementations;
using GrcMvc.Services.Interfaces;
using GrcMvc.Services.Interfaces.Workflows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WorkflowEscalation = GrcMvc.Models.Entities.WorkflowEscalation;
using WorkflowNotification = GrcMvc.Models.Workflows.WorkflowNotification;

namespace GrcMvc.BackgroundJobs
{
    /// <summary>
    /// Background job for processing workflow escalations
    /// Runs every hour to check for overdue workflows and escalate them
    /// </summary>
    public class EscalationJob
    {
        private readonly GrcDbContext _context;
        private readonly IEscalationService _escalationService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<EscalationJob> _logger;

        public EscalationJob(
            GrcDbContext context,
            IEscalationService escalationService,
            INotificationService notificationService,
            ILogger<EscalationJob> logger)
        {
            _context = context;
            _escalationService = escalationService;
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// Main job execution method - called by Hangfire scheduler
        /// </summary>
        [Hangfire.AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 300, 900 })]
        public async Task ExecuteAsync()
        {
            _logger.LogInformation("EscalationJob started at {Time}", DateTime.UtcNow);

            try
            {
                // Get all active tenants
                var tenants = await _context.Tenants
                    .AsNoTracking()
                    .Where(t => t.IsActive)
                    .Select(t => t.Id)
                    .ToListAsync();

                var totalEscalations = 0;

                foreach (var tenantId in tenants)
                {
                    var escalationCount = await ProcessTenantEscalationsAsync(tenantId);
                    totalEscalations += escalationCount;
                }

                _logger.LogInformation(
                    "EscalationJob completed. Processed {Count} escalations across {TenantCount} tenants",
                    totalEscalations, tenants.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EscalationJob failed with error: {Message}", ex.Message);
                throw; // Re-throw to let Hangfire handle retry
            }
        }

        /// <summary>
        /// Process escalations for a specific tenant
        /// </summary>
        private async Task<int> ProcessTenantEscalationsAsync(Guid tenantId)
        {
            var escalationCount = 0;

            try
            {
                // Get overdue workflow tasks
                var overdueTasks = await _context.WorkflowTasks
                    .Include(t => t.WorkflowInstance)
                    .Where(t => t.TenantId == tenantId)
                    .Where(t => t.Status == "Pending" || t.Status == "InProgress")
                    .Where(t => t.DueDate.HasValue && t.DueDate.Value < DateTime.UtcNow)
                    .Where(t => t.Status != "Escalated")
                    .ToListAsync();

                foreach (var task in overdueTasks)
                {
                    await ProcessTaskEscalationAsync(task);
                    escalationCount++;
                }

                // Get workflows with SLA breaches
                var slaBreaches = await GetSlaBreachWorkflowsAsync(tenantId);
                foreach (var workflow in slaBreaches)
                {
                    await ProcessSlaEscalationAsync(workflow);
                    escalationCount++;
                }

                _logger.LogDebug(
                    "Tenant {TenantId}: Processed {Count} escalations",
                    tenantId, escalationCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error processing escalations for tenant {TenantId}: {Message}",
                    tenantId, ex.Message);
            }

            return escalationCount;
        }

        /// <summary>
        /// Process escalation for an overdue task
        /// </summary>
        private async Task ProcessTaskEscalationAsync(WorkflowTask task)
        {
            try
            {
                // Calculate hours overdue
                var hoursOverdue = (DateTime.UtcNow - task.DueDate!.Value).TotalHours;

                // Determine escalation level based on hours overdue
                var escalationLevel = hoursOverdue switch
                {
                    < 24 => 1,  // Level 1: Less than 24 hours
                    < 48 => 2,  // Level 2: 24-48 hours
                    < 72 => 3,  // Level 3: 48-72 hours
                    _ => 4      // Level 4: More than 72 hours (critical)
                };

                // Create escalation record
                var escalation = new WorkflowEscalation
                {
                    WorkflowInstanceId = task.WorkflowInstanceId,
                    TaskId = task.Id,
                    EscalationLevel = escalationLevel,
                    EscalationReason = $"Task overdue by {hoursOverdue:F1} hours",
                    EscalatedAt = DateTime.UtcNow,
                    TenantId = task.TenantId,
                    Status = "Pending",
                    OriginalAssignee = task.AssignedToUserId ?? Guid.Empty
                };

                // Get escalation target (supervisor/manager)
                var escalationTarget = await GetEscalationTargetAsync(
                    task.AssignedToUserId?.ToString(), escalationLevel, task.TenantId);
                escalation.EscalatedToUserId = Guid.TryParse(escalationTarget, out var targetGuid) ? targetGuid : (Guid?)null;

                _context.WorkflowEscalations.Add(escalation);

                // Mark task as escalated
                task.Status = "Escalated";
                task.IsEscalated = true;

                // Create notification for escalation
                await CreateEscalationNotificationAsync(escalation, task);

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Task {TaskId} escalated to level {Level}. Assigned to {UserId}",
                    task.Id, escalationLevel, escalation.EscalatedToUserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error escalating task {TaskId}: {Message}", task.Id, ex.Message);
            }
        }

        /// <summary>
        /// Get workflows with SLA breaches
        /// </summary>
        private async Task<List<WorkflowInstance>> GetSlaBreachWorkflowsAsync(Guid tenantId)
        {
            var now = DateTime.UtcNow;

            return await _context.WorkflowInstances
                .Where(w => w.TenantId == tenantId)
                .Where(w => w.Status != "Completed" && w.Status != "Cancelled")
                .Where(w => w.SlaDueDate.HasValue && w.SlaDueDate.Value < now)
                .Where(w => !w.SlaBreached)
                .ToListAsync();
        }

        /// <summary>
        /// Process SLA breach escalation
        /// </summary>
        private async Task ProcessSlaEscalationAsync(WorkflowInstance workflow)
        {
            try
            {
                workflow.SlaBreached = true;
                workflow.SlaBreachedAt = DateTime.UtcNow;

                var escalation = new WorkflowEscalation
                {
                    WorkflowInstanceId = workflow.Id,
                    EscalationLevel = 3, // SLA breaches are high priority
                    EscalationReason = $"SLA breach: Workflow due date was {workflow.SlaDueDate}",
                    EscalatedAt = DateTime.UtcNow,
                    TenantId = workflow.TenantId,
                    Status = "Pending",
                    OriginalAssignee = workflow.InitiatedByUserId ?? Guid.Empty
                };

                // Escalate to compliance officer or admin
                var slaTarget = await GetSlaEscalationTargetAsync(workflow.TenantId);
                escalation.EscalatedToUserId = Guid.TryParse(slaTarget, out var slaTargetGuid) ? slaTargetGuid : (Guid?)null;

                _context.WorkflowEscalations.Add(escalation);

                // Create critical notification
                await CreateSlaBreachNotificationAsync(workflow, escalation);

                await _context.SaveChangesAsync();

                _logger.LogWarning(
                    "Workflow {WorkflowId} SLA breached. Escalated to {UserId}",
                    workflow.Id, escalation.EscalatedToUserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing SLA escalation for workflow {WorkflowId}", workflow.Id);
            }
        }

        /// <summary>
        /// Get the target user for escalation based on level
        /// </summary>
        private async Task<string> GetEscalationTargetAsync(string? originalUserId, int level, Guid tenantId)
        {
            // Return system admin as default escalation target
            // In production, this would query user management system
            return await Task.FromResult("system-admin");
        }

        /// <summary>
        /// Get target for SLA breach escalation
        /// </summary>
        private async Task<string> GetSlaEscalationTargetAsync(Guid tenantId)
        {
            // Return system admin as default SLA escalation target
            // In production, this would query user management system
            return await Task.FromResult("system-admin");
        }

        /// <summary>
        /// Create notification for task escalation
        /// </summary>
        private async Task CreateEscalationNotificationAsync(WorkflowEscalation escalation, WorkflowTask task)
        {
            var recipientId = escalation.EscalatedToUserId?.ToString() ?? "system-admin";
            var subject = $"[ESCALATION L{escalation.EscalationLevel}] Task Requires Attention";
            var body = $"Task '{task.TaskName}' has been escalated to you.\n\n" +
                       $"Reason: {escalation.EscalationReason}\n" +
                       $"Original Assignee: {escalation.OriginalAssignee}\n" +
                       $"Escalation Level: {escalation.EscalationLevel}";
            var priority = escalation.EscalationLevel >= 3 ? "High" : "Medium";

            await _notificationService.SendNotificationAsync(
                escalation.WorkflowInstanceId,
                recipientId,
                "Escalation",
                subject,
                body,
                priority,
                escalation.TenantId);
        }

        /// <summary>
        /// Create notification for SLA breach
        /// </summary>
        private async Task CreateSlaBreachNotificationAsync(WorkflowInstance workflow, WorkflowEscalation escalation)
        {
            var slaRecipientId = escalation.EscalatedToUserId?.ToString() ?? "system-admin";
            var subject = $"[CRITICAL] SLA Breach - Workflow #{workflow.Id}";
            var body = $"CRITICAL: SLA has been breached for workflow #{workflow.Id}.\n\n" +
                       $"Workflow Type: {workflow.WorkflowType}\n" +
                       $"SLA Due Date: {workflow.SlaDueDate}\n" +
                       $"Created By: {workflow.InitiatedByUserId}\n\n" +
                       $"Immediate action required.";

            await _notificationService.SendNotificationAsync(
                workflow.Id,
                slaRecipientId,
                "SLA_Breach",
                subject,
                body,
                "Critical",
                workflow.TenantId);
        }
    }
}
