using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.BackgroundJobs
{
    /// <summary>
    /// Background job for Stage 4: Resilience Building.
    /// Schedules BC/DR drills, incident response exercises, and sends reminders.
    /// Runs monthly on the 1st at 10 AM
    /// </summary>
    public class DrillSchedulerJob
    {
        private readonly GrcDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly ILogger<DrillSchedulerJob> _logger;

        public DrillSchedulerJob(
            GrcDbContext context,
            INotificationService notificationService,
            ILogger<DrillSchedulerJob> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _logger = logger;
        }

        [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 300, 900 })]
        public async Task CheckUpcomingDrillsAsync()
        {
            _logger.LogInformation("DrillSchedulerJob started at {Time}", DateTime.UtcNow);

            try
            {
                var tenants = await _context.Tenants.AsNoTracking()
                    .Where(t => t.IsActive && t.OnboardingStatus == "COMPLETED")
                    .Select(t => t.Id).ToListAsync();

                var totalDrills = 0;
                var totalReminders = 0;

                foreach (var tenantId in tenants)
                {
                    var (drills, reminders) = await ProcessTenantDrillsAsync(tenantId);
                    totalDrills += drills;
                    totalReminders += reminders;
                }

                _logger.LogInformation("DrillSchedulerJob completed. Scheduled {Drills}, reminders {Reminders}",
                    totalDrills, totalReminders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DrillSchedulerJob failed: {Message}", ex.Message);
                throw;
            }
        }

        private async Task<(int drills, int reminders)> ProcessTenantDrillsAsync(Guid tenantId)
        {
            var drillsScheduled = 0;
            var remindersSent = 0;
            var today = DateTime.UtcNow.Date;

            // Check if any drills need to be scheduled based on cadence
            var bcPlans = await _context.BusinessContinuityPlans
                .Where(p => p.TenantId == tenantId && p.IsActive)
                .ToListAsync();

            foreach (var plan in bcPlans)
            {
                var lastDrill = await _context.ResilienceDrills
                    .Where(d => d.TenantId == tenantId && d.PlanId == plan.Id)
                    .OrderByDescending(d => d.ScheduledDate)
                    .FirstOrDefaultAsync();

                var drillCadence = plan.DrillFrequency ?? "QUARTERLY";
                var daysSinceLastDrill = lastDrill != null 
                    ? (today - lastDrill.ScheduledDate.Date).TotalDays 
                    : 365; // Never drilled

                var needsDrill = drillCadence.ToUpper() switch
                {
                    "MONTHLY" => daysSinceLastDrill >= 25,
                    "QUARTERLY" => daysSinceLastDrill >= 80,
                    "SEMI_ANNUAL" or "SEMIANNUAL" => daysSinceLastDrill >= 160,
                    "ANNUAL" or "YEARLY" => daysSinceLastDrill >= 340,
                    _ => daysSinceLastDrill >= 80
                };

                if (needsDrill)
                {
                    var drill = new ResilienceDrill
                    {
                        Id = Guid.NewGuid(),
                        TenantId = tenantId,
                        PlanId = plan.Id,
                        PlanName = plan.Name,
                        DrillType = plan.PlanType ?? "BC",
                        Title = $"{plan.Name} - {DateTime.UtcNow:MMMM yyyy} Drill",
                        Description = $"Scheduled {drillCadence.ToLower()} drill for {plan.Name}",
                        Status = "Scheduled",
                        ScheduledDate = CalculateDrillDate(today),
                        OwnerId = plan.OwnerId,
                        OwnerName = plan.OwnerName,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = "System:DrillSchedulerJob",
                        IsActive = true
                    };

                    _context.ResilienceDrills.Add(drill);
                    drillsScheduled++;

                    if (plan.OwnerId.HasValue)
                    {
                        await _notificationService.SendNotificationAsync(
                            drill.Id, plan.OwnerId.Value.ToString(), "DrillScheduled",
                            $"Drill Scheduled: {drill.Title}",
                            $"A resilience drill has been scheduled.\n\n" +
                            $"Plan: {plan.Name}\nType: {drill.DrillType}\n" +
                            $"Scheduled: {drill.ScheduledDate:yyyy-MM-dd}\n\n" +
                            $"Please prepare participants and exercise materials.",
                            "Medium", tenantId);
                    }
                }
            }

            // Send reminders for upcoming drills
            var upcomingDrills = await _context.ResilienceDrills
                .Where(d => d.TenantId == tenantId && d.IsActive)
                .Where(d => d.Status == "Scheduled")
                .Where(d => d.ScheduledDate >= today && d.ScheduledDate <= today.AddDays(14))
                .ToListAsync();

            var reminderDays = new[] { 14, 7, 3, 1 };
            foreach (var drill in upcomingDrills)
            {
                var daysUntilDrill = (drill.ScheduledDate.Date - today).Days;
                
                if (reminderDays.Contains(daysUntilDrill) && drill.OwnerId.HasValue &&
                    (drill.LastReminderSent == null || drill.LastReminderSent.Value.Date != today))
                {
                    var urgency = daysUntilDrill <= 1 ? "High" : "Medium";
                    await _notificationService.SendNotificationAsync(
                        drill.Id, drill.OwnerId.Value.ToString(), "DrillReminder",
                        daysUntilDrill == 1 ? $"[TOMORROW] Drill: {drill.Title}" :
                                              $"Drill in {daysUntilDrill} days: {drill.Title}",
                        $"Resilience drill reminder.\n\n" +
                        $"Drill: {drill.Title}\nType: {drill.DrillType}\n" +
                        $"Date: {drill.ScheduledDate:yyyy-MM-dd}",
                        urgency, tenantId);

                    drill.LastReminderSent = DateTime.UtcNow;
                    remindersSent++;
                }
            }

            // Check for drills that need follow-up (completed but no review)
            var completedDrills = await _context.ResilienceDrills
                .Where(d => d.TenantId == tenantId && d.Status == "Completed")
                .Where(d => !d.ReviewCompleted && d.CompletedAt.HasValue && d.CompletedAt.Value < today.AddDays(-7))
                .ToListAsync();

            foreach (var drill in completedDrills)
            {
                if (drill.OwnerId.HasValue)
                {
                    await _notificationService.SendNotificationAsync(
                        drill.Id, drill.OwnerId.Value.ToString(), "DrillReviewNeeded",
                        $"[ACTION] Drill Review Required: {drill.Title}",
                        $"Drill completed but review not submitted.\n\n" +
                        $"Drill: {drill.Title}\nCompleted: {drill.CompletedAt:yyyy-MM-dd}\n\n" +
                        $"Please complete the post-drill review and document lessons learned.",
                        "Medium", tenantId);
                    remindersSent++;
                }
            }

            await _context.SaveChangesAsync();
            return (drillsScheduled, remindersSent);
        }

        private DateTime CalculateDrillDate(DateTime today)
        {
            // Schedule drill for 2 weeks from now, on a weekday
            var drillDate = today.AddDays(14);
            while (drillDate.DayOfWeek == DayOfWeek.Saturday || drillDate.DayOfWeek == DayOfWeek.Sunday)
            {
                drillDate = drillDate.AddDays(1);
            }
            return drillDate;
        }
    }

    /// <summary>
    /// Incident Response automation job - monitors for incidents and triggers response workflows.
    /// Runs every 15 minutes
    /// </summary>
    public class IncidentResponseJob
    {
        private readonly GrcDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly ILogger<IncidentResponseJob> _logger;

        public IncidentResponseJob(
            GrcDbContext context,
            INotificationService notificationService,
            ILogger<IncidentResponseJob> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _logger = logger;
        }

        [AutomaticRetry(Attempts = 3)]
        public async Task ProcessIncidentResponseAsync()
        {
            _logger.LogInformation("IncidentResponseJob started at {Time}", DateTime.UtcNow);

            var tenants = await _context.Tenants.AsNoTracking()
                .Where(t => t.IsActive).Select(t => t.Id).ToListAsync();

            var escalations = 0;
            foreach (var tenantId in tenants)
            {
                escalations += await ProcessTenantIncidentsAsync(tenantId);
            }

            _logger.LogInformation("IncidentResponseJob completed. Escalations: {Count}", escalations);
        }

        private async Task<int> ProcessTenantIncidentsAsync(Guid tenantId)
        {
            var escalationCount = 0;
            var now = DateTime.UtcNow;

            // Check open incidents for SLA breaches
            var openIncidents = await _context.Incidents
                .Where(i => i.TenantId == tenantId && i.IsActive)
                .Where(i => i.Status != "Closed" && i.Status != "Resolved")
                .ToListAsync();

            foreach (var incident in openIncidents)
            {
                var hoursOpen = (now - incident.CreatedDate).TotalHours;

                // Define SLA thresholds by severity
                var (responseSlaDays, resolutionSlaDays) = incident.Severity switch
                {
                    "Critical" => (4, 24),       // 4 hours response, 24 hours resolution
                    "High" => (8, 48),           // 8 hours response, 48 hours resolution
                    "Medium" => (24, 120),       // 1 day response, 5 days resolution
                    _ => (48, 168)               // 2 days response, 7 days resolution
                };

                // Check response SLA
                if (incident.FirstRespondedAt == null && hoursOpen > responseSlaDays)
                {
                    incident.ResponseSlaBreached = true;
                    if (incident.AssignedToId.HasValue)
                    {
                        await _notificationService.SendNotificationAsync(
                            incident.Id, incident.AssignedToId.Value.ToString(), "IncidentSlaBreached",
                            $"[SLA BREACH] Incident Response: {incident.Title}",
                            $"Response SLA has been breached.\n\n" +
                            $"Incident: {incident.Title}\nSeverity: {incident.Severity}\n" +
                            $"Hours Open: {hoursOpen:N0}\nResponse SLA: {responseSlaDays} hours",
                            "Critical", tenantId);
                        escalationCount++;
                    }
                }

                // Check resolution SLA
                if (incident.Status != "Resolved" && hoursOpen > resolutionSlaDays)
                {
                    incident.ResolutionSlaBreached = true;
                    incident.EscalationLevel = (incident.EscalationLevel ?? 0) + 1;
                    
                    if (incident.AssignedToId.HasValue)
                    {
                        await _notificationService.SendNotificationAsync(
                            incident.Id, incident.AssignedToId.Value.ToString(), "IncidentResolutionSlaBreached",
                            $"[CRITICAL] Resolution SLA Breach: {incident.Title}",
                            $"Resolution SLA has been breached.\n\n" +
                            $"Incident: {incident.Title}\nSeverity: {incident.Severity}\n" +
                            $"Hours Open: {hoursOpen:N0}\nResolution SLA: {resolutionSlaDays} hours\n" +
                            $"Escalation Level: {incident.EscalationLevel}",
                            "Critical", tenantId);
                        escalationCount++;
                    }
                }

                incident.ModifiedDate = now;
            }

            await _context.SaveChangesAsync();
            return escalationCount;
        }
    }
}
