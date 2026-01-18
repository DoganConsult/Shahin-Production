using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Models.Workflows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WorkflowInstance = GrcMvc.Models.Entities.WorkflowInstance;
using WorkflowTask = GrcMvc.Models.Entities.WorkflowTask;
using WorkflowEscalation = GrcMvc.Models.Entities.WorkflowEscalation;

namespace GrcMvc.BackgroundJobs
{
    /// <summary>
    /// Background job for monitoring SLA compliance
    /// Runs every 30 minutes to check for upcoming SLA breaches and send warnings
    /// </summary>
    public class SlaMonitorJob
    {
        private readonly GrcDbContext _context;
        private readonly ILogger<SlaMonitorJob> _logger;

        public SlaMonitorJob(
            GrcDbContext context,
            ILogger<SlaMonitorJob> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Main job execution method - called by Hangfire scheduler
        /// </summary>
        [Hangfire.AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 300, 900 })]
        public async Task ExecuteAsync()
        {
            _logger.LogInformation("SlaMonitorJob started at {Time}", DateTime.UtcNow);

            try
            {
                var stats = new SlaStats();

                // Get all active tenants
                var tenants = await _context.Tenants
                    .AsNoTracking()
                    .Where(t => t.IsActive)
                    .Select(t => t.Id)
                    .ToListAsync();

                foreach (var tenantId in tenants)
                {
                    var tenantStats = await MonitorTenantSlaAsync(tenantId);
                    stats.Add(tenantStats);
                }

                _logger.LogInformation(
                    "SlaMonitorJob completed. Warnings: {Warnings}, Critical: {Critical}, Breached: {Breached}",
                    stats.Warnings, stats.Critical, stats.Breached);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SlaMonitorJob failed with error: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Monitor SLA for a specific tenant
        /// </summary>
        private async Task<SlaStats> MonitorTenantSlaAsync(Guid tenantId)
        {
            var stats = new SlaStats();
            var now = DateTime.UtcNow;

            try
            {
                // Get active workflows with SLA dates
                var workflows = await _context.WorkflowInstances
                    .Where(w => w.TenantId == tenantId)
                    .Where(w => w.Status != "Completed" && w.Status != "Cancelled")
                    .Where(w => w.SlaDueDate.HasValue)
                    .ToListAsync();

                foreach (var workflow in workflows)
                {
                    var slaStatus = CalculateSlaStatus(workflow.SlaDueDate!.Value, now);

                    switch (slaStatus)
                    {
                        case SlaStatus.Warning:
                            await SendSlaWarningAsync(workflow, "WARNING: Approaching SLA deadline");
                            stats.Warnings++;
                            break;

                        case SlaStatus.Critical:
                            await SendSlaCriticalWarningAsync(workflow);
                            stats.Critical++;
                            break;

                        case SlaStatus.Breached:
                            if (!workflow.SlaBreached)
                            {
                                await ProcessSlaBreachAsync(workflow);
                                stats.Breached++;
                            }
                            break;
                    }
                }

                // Monitor task-level SLAs
                await MonitorTaskSlaAsync(tenantId, stats);

                _logger.LogDebug(
                    "Tenant {TenantId} SLA stats - Warnings: {W}, Critical: {C}, Breached: {B}",
                    tenantId, stats.Warnings, stats.Critical, stats.Breached);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error monitoring SLA for tenant {TenantId}: {Message}", tenantId, ex.Message);
            }

            return stats;
        }

        /// <summary>
        /// Monitor task-level SLAs
        /// </summary>
        private async Task MonitorTaskSlaAsync(Guid tenantId, SlaStats stats)
        {
            var now = DateTime.UtcNow;

            var tasks = await _context.WorkflowTasks
                .Include(t => t.WorkflowInstance)
                .Where(t => t.TenantId == tenantId)
                .Where(t => t.Status == "Pending" || t.Status == "InProgress")
                .Where(t => t.DueDate.HasValue)
                .ToListAsync();

            foreach (var task in tasks)
            {
                var slaStatus = CalculateSlaStatus(task.DueDate!.Value, now);

                if (slaStatus == SlaStatus.Warning || slaStatus == SlaStatus.Critical)
                {
                    await SendTaskSlaWarningAsync(task, slaStatus);
                    stats.Warnings++;
                }
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Calculate SLA status based on due date
        /// </summary>
        private SlaStatus CalculateSlaStatus(DateTime dueDate, DateTime now)
        {
            var timeRemaining = dueDate - now;

            if (timeRemaining.TotalHours <= 0)
                return SlaStatus.Breached;

            if (timeRemaining.TotalHours <= 4)
                return SlaStatus.Critical;

            if (timeRemaining.TotalHours <= 24)
                return SlaStatus.Warning;

            return SlaStatus.OnTrack;
        }

        /// <summary>
        /// Send SLA warning notification
        /// </summary>
        private async Task SendSlaWarningAsync(WorkflowInstance workflow, string message)
        {
            // Don't send duplicate warnings
            var existingWarning = await _context.WorkflowNotifications
                .AnyAsync(n => n.WorkflowInstanceId == workflow.Id
                            && n.NotificationType == "SLA_Warning"
                            && n.CreatedAt > DateTime.UtcNow.AddHours(-24));

            if (existingWarning)
                return;

            var notification = new WorkflowNotification
            {
                WorkflowInstanceId = workflow.Id,
                TenantId = workflow.TenantId,
                NotificationType = "SLA_Warning",
                RecipientUserId = workflow.InitiatedByUserId?.ToString() ?? "system",
                Recipient = workflow.InitiatedByUserId?.ToString() ?? "system",
                Subject = $"[SLA WARNING] Workflow #{workflow.Id} - {workflow.WorkflowType}",
                Message = $"{message}\n\nWorkflow: {workflow.WorkflowType}\nCurrent Status: {workflow.Status}",
                Body = $"{message}\n\n" +
                       $"Workflow: {workflow.WorkflowType}\n" +
                       $"Current Status: {workflow.Status}\n" +
                       $"SLA Due: {workflow.SlaDueDate:yyyy-MM-dd HH:mm} UTC\n\n" +
                       $"Please take action to avoid SLA breach.",
                Priority = "Medium",
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                RequiresEmail = true
            };

            _context.WorkflowNotifications.Add(notification);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "SLA warning sent for workflow {WorkflowId}, due {DueDate}",
                workflow.Id, workflow.SlaDueDate);
        }

        /// <summary>
        /// Send critical SLA warning
        /// </summary>
        private async Task SendSlaCriticalWarningAsync(WorkflowInstance workflow)
        {
            // Don't send duplicate critical warnings
            var existingWarning = await _context.WorkflowNotifications
                .AnyAsync(n => n.WorkflowInstanceId == workflow.Id
                            && n.NotificationType == "SLA_Critical"
                            && n.CreatedAt > DateTime.UtcNow.AddHours(-4));

            if (existingWarning)
                return;

            // Get supervisor/compliance officer
            var supervisorId = await GetSupervisorAsync(workflow.InitiatedByUserId?.ToString(), workflow.TenantId);

            var timeRemaining = workflow.SlaDueDate!.Value - DateTime.UtcNow;

            var notification = new WorkflowNotification
            {
                WorkflowInstanceId = workflow.Id,
                TenantId = workflow.TenantId,
                NotificationType = "SLA_Critical",
                RecipientUserId = supervisorId,
                Recipient = supervisorId,
                Subject = $"[CRITICAL] SLA Breach Imminent - Workflow #{workflow.Id}",
                Message = $"CRITICAL: SLA breach imminent for {workflow.WorkflowType}",
                Body = $"CRITICAL: SLA breach imminent!\n\n" +
                       $"Workflow: {workflow.WorkflowType} (#{workflow.Id})\n" +
                       $"Current Status: {workflow.Status}\n" +
                       $"SLA Due: {workflow.SlaDueDate:yyyy-MM-dd HH:mm} UTC\n" +
                       $"Time Remaining: {timeRemaining.Hours}h {timeRemaining.Minutes}m\n\n" +
                       $"IMMEDIATE action required to prevent SLA breach.",
                Priority = "Critical",
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                RequiresEmail = true
            };

            _context.WorkflowNotifications.Add(notification);

            // Also notify the workflow owner
            var ownerNotification = new WorkflowNotification
            {
                WorkflowInstanceId = workflow.Id,
                TenantId = workflow.TenantId,
                NotificationType = "SLA_Critical",
                RecipientUserId = workflow.InitiatedByUserId?.ToString() ?? "system",
                Recipient = workflow.InitiatedByUserId?.ToString() ?? "system",
                Subject = notification.Subject,
                Message = notification.Message,
                Body = notification.Body,
                Priority = "Critical",
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                RequiresEmail = true
            };

            _context.WorkflowNotifications.Add(ownerNotification);
            await _context.SaveChangesAsync();

            _logger.LogWarning(
                "CRITICAL SLA warning sent for workflow {WorkflowId}, {TimeRemaining} remaining",
                workflow.Id, timeRemaining);
        }

        /// <summary>
        /// Process an actual SLA breach
        /// </summary>
        private async Task ProcessSlaBreachAsync(WorkflowInstance workflow)
        {
            workflow.SlaBreached = true;
            workflow.SlaBreachedAt = DateTime.UtcNow;

            // Create escalation record
            var complianceOfficer = await GetComplianceOfficerAsync(workflow.TenantId);
            var escalation = new WorkflowEscalation
            {
                WorkflowInstanceId = workflow.Id,
                EscalationLevel = 4, // Highest level for SLA breach
                EscalationReason = $"SLA BREACHED: Due date was {workflow.SlaDueDate:yyyy-MM-dd HH:mm} UTC",
                EscalatedAt = DateTime.UtcNow,
                TenantId = workflow.TenantId,
                Status = "Pending",
                OriginalAssignee = workflow.InitiatedByUserId ?? Guid.Empty,
                EscalatedToUserId = Guid.TryParse(complianceOfficer, out var coGuid) ? coGuid : (Guid?)null
            };

            _context.WorkflowEscalations.Add(escalation);

            // Create breach notification
            var notification = new WorkflowNotification
            {
                WorkflowInstanceId = workflow.Id,
                TenantId = workflow.TenantId,
                NotificationType = "SLA_Breach",
                RecipientUserId = complianceOfficer,
                Recipient = complianceOfficer,
                Subject = $"[SLA BREACH] Workflow #{workflow.Id} - IMMEDIATE ACTION REQUIRED",
                Message = $"SLA HAS BEEN BREACHED for {workflow.WorkflowType}",
                Body = $"SLA HAS BEEN BREACHED\n\n" +
                       $"Workflow: {workflow.WorkflowType} (#{workflow.Id})\n" +
                       $"Current Status: {workflow.Status}\n" +
                       $"SLA Due Date: {workflow.SlaDueDate:yyyy-MM-dd HH:mm} UTC\n" +
                       $"Breach Time: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC\n" +
                       $"Initiated By: {workflow.InitiatedByUserId}\n\n" +
                       $"This breach will be logged for compliance reporting.",
                Priority = "Critical",
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                RequiresEmail = true
            };

            _context.WorkflowNotifications.Add(notification);

            // Log the breach for compliance
            _logger.LogWarning(
                "SLA BREACH logged for workflow {WorkflowId}: Type={WorkflowType}, DueDate={DueDate}",
                workflow.Id, workflow.WorkflowType, workflow.SlaDueDate);
            await _context.SaveChangesAsync();

            _logger.LogError(
                "SLA BREACH: Workflow {WorkflowId} breached at {Time}. Due was {DueDate}",
                workflow.Id, DateTime.UtcNow, workflow.SlaDueDate);
        }

        /// <summary>
        /// Send task-level SLA warning
        /// </summary>
        private async Task SendTaskSlaWarningAsync(WorkflowTask task, SlaStatus status)
        {
            var priority = status == SlaStatus.Critical ? "Critical" : "High";
            var prefix = status == SlaStatus.Critical ? "[CRITICAL]" : "[WARNING]";

            var notification = new WorkflowNotification
            {
                WorkflowInstanceId = task.WorkflowInstanceId,
                TenantId = task.TenantId,
                NotificationType = status == SlaStatus.Critical ? "SLA_Critical" : "SLA_Warning",
                RecipientUserId = task.AssignedToUserId?.ToString() ?? "system",
                Recipient = task.AssignedToUserId?.ToString() ?? "system",
                Subject = $"{prefix} Task Due Soon: {task.TaskName}",
                Message = $"Task approaching deadline: {task.TaskName}",
                Body = $"Your assigned task is approaching its deadline.\n\n" +
                       $"Task: {task.TaskName}\n" +
                       $"Due Date: {task.DueDate:yyyy-MM-dd HH:mm} UTC\n" +
                       $"Status: {task.Status}\n\n" +
                       $"Please complete this task to avoid escalation.",
                Priority = priority,
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                RequiresEmail = true
            };

            _context.WorkflowNotifications.Add(notification);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Get supervisor for a user - looks up team lead or manager from TeamMember hierarchy
        /// </summary>
        private async Task<string> GetSupervisorAsync(string? userId, Guid tenantId)
        {
            if (string.IsNullOrEmpty(userId))
                return await GetComplianceOfficerAsync(tenantId);

            // Try to find team member's supervisor through team hierarchy
            if (Guid.TryParse(userId, out var userGuid))
            {
                var teamMember = await _context.TeamMembers
                    .Include(tm => tm.Team)
                    .FirstOrDefaultAsync(tm => tm.UserId == userGuid &&
                                               tm.TenantId == tenantId &&
                                               tm.IsActive && !tm.IsDeleted);

                if (teamMember?.Team != null)
                {
                    // Find the team lead (primary member with manager/lead role)
                    var teamLead = await _context.TeamMembers
                        .Where(tm => tm.TeamId == teamMember.TeamId &&
                                    tm.TenantId == tenantId &&
                                    tm.IsPrimaryForRole &&
                                    tm.IsActive && !tm.IsDeleted &&
                                    (tm.RoleCode == "GRC_MANAGER" ||
                                     tm.RoleCode == "TEAM_LEAD" ||
                                     tm.RoleCode == "COMPLIANCE_OFFICER") &&
                                    tm.UserId != userGuid)
                        .FirstOrDefaultAsync();

                    if (teamLead != null)
                        return teamLead.UserId.ToString();
                }
            }

            // Fallback to compliance officer
            return await GetComplianceOfficerAsync(tenantId);
        }

        /// <summary>
        /// Get compliance officer for tenant - looks up user with COMPLIANCE_OFFICER role
        /// </summary>
        private async Task<string> GetComplianceOfficerAsync(Guid tenantId)
        {
            // Find compliance officer through RACI assignments or team members
            var complianceOfficer = await _context.TeamMembers
                .Where(tm => tm.TenantId == tenantId &&
                            tm.RoleCode == "COMPLIANCE_OFFICER" &&
                            tm.IsPrimaryForRole &&
                            tm.IsActive && !tm.IsDeleted)
                .FirstOrDefaultAsync();

            if (complianceOfficer != null)
                return complianceOfficer.UserId.ToString();

            // Fallback: Try to find any GRC manager
            var grcManager = await _context.TeamMembers
                .Where(tm => tm.TenantId == tenantId &&
                            tm.RoleCode == "GRC_MANAGER" &&
                            tm.IsActive && !tm.IsDeleted)
                .FirstOrDefaultAsync();

            if (grcManager != null)
                return grcManager.UserId.ToString();

            // Final fallback: Find tenant admin
            var tenantAdmin = await _context.TenantUsers
                .Where(tu => tu.TenantId == tenantId &&
                            tu.Status == "Active" && !tu.IsDeleted &&
                            tu.RoleCode == "ADMIN")
                .FirstOrDefaultAsync();

            if (tenantAdmin != null)
                return tenantAdmin.UserId.ToString();

            return "system-admin";
        }

        private enum SlaStatus
        {
            OnTrack,
            Warning,
            Critical,
            Breached
        }

        private class SlaStats
        {
            public int Warnings { get; set; }
            public int Critical { get; set; }
            public int Breached { get; set; }

            public void Add(SlaStats other)
            {
                Warnings += other.Warnings;
                Critical += other.Critical;
                Breached += other.Breached;
            }
        }
    }
}
