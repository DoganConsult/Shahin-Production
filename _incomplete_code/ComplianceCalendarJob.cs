using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.BackgroundJobs
{
    /// <summary>
    /// Background job for managing compliance calendar and regulatory deadlines.
    /// Runs daily at 7 AM
    /// </summary>
    public class ComplianceCalendarJob
    {
        private readonly GrcDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly ILogger<ComplianceCalendarJob> _logger;

        private static readonly Dictionary<string, int> EventLeadTimes = new()
        {
            { "AUDIT", 90 }, { "CERTIFICATION", 120 }, { "RENEWAL", 60 },
            { "SUBMISSION", 30 }, { "REVIEW", 14 }, { "ATTESTATION", 21 }
        };

        public ComplianceCalendarJob(
            GrcDbContext context,
            INotificationService notificationService,
            ILogger<ComplianceCalendarJob> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _logger = logger;
        }

        [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 300, 900 })]
        public async Task ProcessComplianceCalendarAsync()
        {
            _logger.LogInformation("ComplianceCalendarJob started at {Time}", DateTime.UtcNow);

            try
            {
                var tenants = await _context.Tenants.AsNoTracking()
                    .Where(t => t.IsActive && t.OnboardingStatus == "COMPLETED")
                    .Select(t => t.Id).ToListAsync();

                var totalEvents = 0;
                var totalTasks = 0;

                foreach (var tenantId in tenants)
                {
                    var (events, tasks) = await ProcessTenantComplianceCalendarAsync(tenantId);
                    totalEvents += events;
                    totalTasks += tasks;
                }

                _logger.LogInformation("ComplianceCalendarJob completed. Events {Events}, tasks {Tasks}", 
                    totalEvents, totalTasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ComplianceCalendarJob failed: {Message}", ex.Message);
                throw;
            }
        }

        private async Task<(int events, int tasks)> ProcessTenantComplianceCalendarAsync(Guid tenantId)
        {
            var eventsProcessed = 0;
            var tasksCreated = 0;
            var today = DateTime.UtcNow.Date;

            // Get upcoming compliance events
            var upcomingEvents = await _context.ComplianceCalendarEvents
                .Where(e => e.TenantId == tenantId && e.IsActive)
                .Where(e => e.Status != "Completed" && e.Status != "Cancelled")
                .Where(e => e.DueDate >= today && e.DueDate <= today.AddDays(120))
                .ToListAsync();

            var reminderDays = new[] { 90, 60, 30, 14, 7, 3, 1, 0 };

            foreach (var evt in upcomingEvents)
            {
                var daysUntilDue = (evt.DueDate.Date - today).Days;
                var leadTime = EventLeadTimes.TryGetValue(evt.EventType?.ToUpper() ?? "", out var lt) ? lt : 30;

                // Create task if within lead time
                if (daysUntilDue <= leadTime && !evt.TaskCreated)
                {
                    var existingTask = await _context.WorkflowTasks
                        .AnyAsync(t => t.TenantId == tenantId && t.RelatedEntityId == evt.Id.ToString() &&
                                       t.Status != "Completed" && t.Status != "Cancelled");

                    if (!existingTask)
                    {
                        var task = new WorkflowTask
                        {
                            Id = Guid.NewGuid(),
                            TenantId = tenantId,
                            TaskName = $"[{evt.EventType}] {evt.Title}",
                            Description = evt.Description ?? $"Compliance activity due: {evt.Title}",
                            TaskType = "Compliance",
                            Status = "Pending",
                            Priority = daysUntilDue <= 7 ? "Critical" : daysUntilDue <= 14 ? "High" : "Medium",
                            DueDate = evt.DueDate.AddDays(-7),
                            CreatedDate = DateTime.UtcNow,
                            CreatedBy = "System:ComplianceCalendarJob",
                            RelatedEntityType = "ComplianceCalendarEvent",
                            RelatedEntityId = evt.Id.ToString(),
                            AssignedToUserId = evt.OwnerId
                        };

                        _context.WorkflowTasks.Add(task);
                        evt.TaskCreated = true;
                        evt.TaskId = task.Id;
                        tasksCreated++;
                    }
                }

                // Send reminders
                if (reminderDays.Contains(daysUntilDue) && evt.OwnerId.HasValue &&
                    (evt.LastReminderSent == null || evt.LastReminderSent.Value.Date != today))
                {
                    var urgency = daysUntilDue switch { 0 => "Critical", <= 3 => "High", <= 14 => "Medium", _ => "Low" };
                    await _notificationService.SendNotificationAsync(
                        evt.Id, evt.OwnerId.Value.ToString(), "ComplianceReminder",
                        daysUntilDue == 0 ? $"[DUE TODAY] {evt.EventType}: {evt.Title}" :
                                           $"Compliance Event in {daysUntilDue} days: {evt.Title}",
                        $"Compliance reminder.\n\nEvent: {evt.Title}\nType: {evt.EventType}\n" +
                        $"Framework: {evt.FrameworkCode}\nDue: {evt.DueDate:yyyy-MM-dd}",
                        urgency, tenantId);
                    evt.LastReminderSent = DateTime.UtcNow;
                }

                eventsProcessed++;
            }

            // Check framework renewal dates
            var frameworksNeedingRenewal = await _context.TenantFrameworkSelections
                .Where(f => f.TenantId == tenantId && f.IsActive)
                .Where(f => f.NextAssessmentDate.HasValue && f.NextAssessmentDate.Value >= today &&
                           f.NextAssessmentDate.Value <= today.AddDays(90))
                .Where(f => !f.RenewalTaskCreated)
                .ToListAsync();

            foreach (var framework in frameworksNeedingRenewal)
            {
                var evt = new ComplianceCalendarEvent
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    Title = $"{framework.FrameworkCode} Annual Assessment",
                    Description = $"Annual assessment required for {framework.FrameworkName}",
                    EventType = "AUDIT",
                    FrameworkCode = framework.FrameworkCode,
                    DueDate = framework.NextAssessmentDate!.Value,
                    Status = "Scheduled",
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System:ComplianceCalendarJob",
                    IsActive = true
                };

                _context.ComplianceCalendarEvents.Add(evt);
                framework.RenewalTaskCreated = true;
                tasksCreated++;
            }

            await _context.SaveChangesAsync();
            return (eventsProcessed, tasksCreated);
        }
    }

    /// <summary>
    /// Gap Remediation tracking job - monitors remediation progress and escalates.
    /// Runs daily at 8 AM
    /// </summary>
    public class GapRemediationJob
    {
        private readonly GrcDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly ILogger<GapRemediationJob> _logger;

        public GapRemediationJob(
            GrcDbContext context,
            INotificationService notificationService,
            ILogger<GapRemediationJob> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _logger = logger;
        }

        [AutomaticRetry(Attempts = 3)]
        public async Task TrackRemediationProgressAsync()
        {
            _logger.LogInformation("GapRemediationJob started at {Time}", DateTime.UtcNow);

            var tenants = await _context.Tenants.AsNoTracking()
                .Where(t => t.IsActive).Select(t => t.Id).ToListAsync();

            var updates = 0;
            var escalations = 0;

            foreach (var tenantId in tenants)
            {
                var (upd, esc) = await ProcessTenantRemediationAsync(tenantId);
                updates += upd;
                escalations += esc;
            }

            _logger.LogInformation("GapRemediationJob completed. Updates {Updates}, escalations {Escalations}",
                updates, escalations);
        }

        private async Task<(int updates, int escalations)> ProcessTenantRemediationAsync(Guid tenantId)
        {
            var updateCount = 0;
            var escalationCount = 0;
            var today = DateTime.UtcNow.Date;

            // Get open action plans
            var actionPlans = await _context.ActionPlans
                .Where(a => a.TenantId == tenantId && a.IsActive)
                .Where(a => a.Status != "Completed" && a.Status != "Cancelled")
                .ToListAsync();

            foreach (var plan in actionPlans)
            {
                var previousStatus = plan.Status;

                // Check if overdue
                if (plan.DueDate.HasValue && plan.DueDate.Value < today)
                {
                    var daysOverdue = (today - plan.DueDate.Value).Days;

                    if (daysOverdue >= 14 && plan.Status != "CriticallyOverdue")
                    {
                        plan.Status = "CriticallyOverdue";
                        plan.EscalationLevel = 3;
                        updateCount++;

                        if (plan.OwnerId.HasValue)
                        {
                            await _notificationService.SendNotificationAsync(
                                plan.Id, plan.OwnerId.Value.ToString(), "RemediationCritical",
                                $"[CRITICAL] Remediation 14+ Days Overdue: {plan.Title}",
                                $"Action plan is critically overdue.\n\nPlan: {plan.Title}\n" +
                                $"Due: {plan.DueDate:yyyy-MM-dd}\nDays Overdue: {daysOverdue}",
                                "Critical", tenantId);
                            escalationCount++;
                        }
                    }
                    else if (daysOverdue >= 7 && plan.Status != "Overdue" && plan.Status != "CriticallyOverdue")
                    {
                        plan.Status = "Overdue";
                        plan.EscalationLevel = 2;
                        updateCount++;

                        if (plan.OwnerId.HasValue)
                        {
                            await _notificationService.SendNotificationAsync(
                                plan.Id, plan.OwnerId.Value.ToString(), "RemediationOverdue",
                                $"[OVERDUE] Remediation Plan: {plan.Title}",
                                $"Action plan is overdue.\n\nPlan: {plan.Title}\nDays Overdue: {daysOverdue}",
                                "High", tenantId);
                            escalationCount++;
                        }
                    }
                }

                plan.ModifiedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return (updateCount, escalationCount);
        }
    }
}
