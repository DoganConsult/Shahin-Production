using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.BackgroundJobs
{
    /// <summary>
    /// Background job for Stage 6: Continuous Sustainability.
    /// Sends reminders for quarterly GRC reviews and policy refresh cycles.
    /// Runs on the 25th of quarter-end months (March, June, September, December)
    /// </summary>
    public class QuarterlyReviewJob
    {
        private readonly GrcDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly ILogger<QuarterlyReviewJob> _logger;

        public QuarterlyReviewJob(
            GrcDbContext context,
            INotificationService notificationService,
            ILogger<QuarterlyReviewJob> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _logger = logger;
        }

        [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 300, 900 })]
        public async Task SendQuarterlyReviewRemindersAsync()
        {
            _logger.LogInformation("QuarterlyReviewJob started at {Time}", DateTime.UtcNow);

            try
            {
                var tenants = await _context.Tenants.AsNoTracking()
                    .Where(t => t.IsActive && t.OnboardingStatus == "COMPLETED")
                    .Select(t => t.Id).ToListAsync();

                var reminders = 0;
                var tasks = 0;

                foreach (var tenantId in tenants)
                {
                    var (rem, tsk) = await ProcessTenantQuarterlyReviewAsync(tenantId);
                    reminders += rem;
                    tasks += tsk;
                }

                _logger.LogInformation("QuarterlyReviewJob completed. Reminders {Rem}, tasks {Tsk}",
                    reminders, tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "QuarterlyReviewJob failed: {Message}", ex.Message);
                throw;
            }
        }

        private async Task<(int reminders, int tasks)> ProcessTenantQuarterlyReviewAsync(Guid tenantId)
        {
            var remindersSent = 0;
            var tasksCreated = 0;
            var today = DateTime.UtcNow.Date;
            var quarter = (today.Month - 1) / 3 + 1;

            // Create quarterly review task if not exists
            var quarterEnd = new DateTime(today.Year, quarter * 3, DateTime.DaysInMonth(today.Year, quarter * 3));
            var reviewTaskExists = await _context.WorkflowTasks
                .AnyAsync(t => t.TenantId == tenantId && t.TaskType == "QuarterlyReview" &&
                              t.DueDate >= today.AddDays(-30) && t.Status != "Cancelled");

            if (!reviewTaskExists)
            {
                // Get compliance managers
                var managers = await _context.TenantUsers
                    .Where(u => u.TenantId == tenantId && u.IsActive &&
                               (u.Role == "ComplianceManager" || u.Role == "Admin"))
                    .Select(u => new { u.UserId, u.UserName })
                    .FirstOrDefaultAsync();

                var task = new WorkflowTask
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    TaskName = $"Q{quarter} {today.Year} GRC Quarterly Review",
                    Description = $"Quarterly review of GRC program for Q{quarter} {today.Year}.\n\n" +
                                  $"Review items:\n" +
                                  $"1. Risk register review and updates\n" +
                                  $"2. Control effectiveness assessment\n" +
                                  $"3. Policy review and refresh\n" +
                                  $"4. Compliance status review\n" +
                                  $"5. KPI/KRI threshold review\n" +
                                  $"6. Improvement initiatives planning",
                    TaskType = "QuarterlyReview",
                    Status = "Pending",
                    Priority = "High",
                    DueDate = quarterEnd,
                    AssignedToUserId = managers?.UserId,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System:QuarterlyReviewJob",
                    IsActive = true
                };

                _context.WorkflowTasks.Add(task);
                tasksCreated++;

                if (managers?.UserId != null)
                {
                    await _notificationService.SendNotificationAsync(
                        task.Id, managers.UserId.ToString(), "QuarterlyReviewScheduled",
                        $"Quarterly GRC Review Scheduled: Q{quarter} {today.Year}",
                        $"Your quarterly GRC review has been scheduled.\n\n" +
                        $"Due Date: {quarterEnd:yyyy-MM-dd}\n\n" +
                        $"Please review and update all GRC program elements.",
                        "High", tenantId);
                    remindersSent++;
                }
            }

            // Check for policies needing annual review
            var policiesNeedingReview = await _context.Policies
                .Where(p => p.TenantId == tenantId && p.IsActive)
                .Where(p => p.NextReviewDate.HasValue && p.NextReviewDate.Value <= today.AddDays(30))
                .Where(p => !p.ReviewReminderSent)
                .ToListAsync();

            foreach (var policy in policiesNeedingReview)
            {
                var daysUntilReview = (policy.NextReviewDate!.Value.Date - today).Days;

                if (policy.OwnerId.HasValue)
                {
                    await _notificationService.SendNotificationAsync(
                        policy.Id, policy.OwnerId.Value.ToString(), "PolicyReviewDue",
                        daysUntilReview <= 0 ? $"[OVERDUE] Policy Review: {policy.Title}" :
                                              $"Policy Review Due in {daysUntilReview} days: {policy.Title}",
                        $"Policy requires annual review.\n\n" +
                        $"Policy: {policy.Title}\n" +
                        $"Last Reviewed: {policy.LastReviewedAt:yyyy-MM-dd}\n" +
                        $"Review Due: {policy.NextReviewDate:yyyy-MM-dd}",
                        daysUntilReview <= 0 ? "High" : "Medium", tenantId);
                    remindersSent++;
                }

                policy.ReviewReminderSent = true;
            }

            // Check framework selections for annual attestation
            var frameworksNeedingAttestation = await _context.TenantFrameworkSelections
                .Where(f => f.TenantId == tenantId && f.IsActive)
                .Where(f => f.LastAttestationDate == null ||
                           f.LastAttestationDate < today.AddYears(-1))
                .ToListAsync();

            foreach (var framework in frameworksNeedingAttestation)
            {
                var taskExists = await _context.WorkflowTasks
                    .AnyAsync(t => t.TenantId == tenantId &&
                                  t.RelatedEntityId == framework.Id.ToString() &&
                                  t.TaskType == "Attestation" &&
                                  t.Status != "Completed" && t.Status != "Cancelled");

                if (!taskExists)
                {
                    var attestationTask = new WorkflowTask
                    {
                        Id = Guid.NewGuid(),
                        TenantId = tenantId,
                        TaskName = $"Annual Attestation: {framework.FrameworkCode}",
                        Description = $"Annual compliance attestation required for {framework.FrameworkName}.",
                        TaskType = "Attestation",
                        Status = "Pending",
                        Priority = "High",
                        DueDate = today.AddDays(30),
                        RelatedEntityType = "TenantFrameworkSelection",
                        RelatedEntityId = framework.Id.ToString(),
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = "System:QuarterlyReviewJob",
                        IsActive = true
                    };

                    _context.WorkflowTasks.Add(attestationTask);
                    tasksCreated++;
                }
            }

            // Send stakeholder engagement reminders for board reporting
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId);
            if (tenant != null && today.Day == 25 && (today.Month == 3 || today.Month == 6 || today.Month == 9 || today.Month == 12))
            {
                var executives = await _context.TenantUsers
                    .Where(u => u.TenantId == tenantId && u.IsActive &&
                               (u.Role == "Executive" || u.Role == "CISO"))
                    .Select(u => u.UserId)
                    .ToListAsync();

                foreach (var execId in executives)
                {
                    await _notificationService.SendNotificationAsync(
                        tenantId, execId.ToString(), "BoardReportingReminder",
                        $"[REMINDER] Quarterly Board GRC Report",
                        $"Quarterly board reporting due for Q{quarter} {today.Year}.\n\n" +
                        $"Please review the quarterly executive report and prepare board presentation.",
                        "Medium", tenantId);
                    remindersSent++;
                }
            }

            await _context.SaveChangesAsync();
            return (remindersSent, tasksCreated);
        }
    }

    /// <summary>
    /// Continuous Improvement job - tracks improvement initiatives and updates roadmap.
    /// Runs weekly on Monday at 6 AM
    /// </summary>
    public class ContinuousImprovementJob
    {
        private readonly GrcDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly ILogger<ContinuousImprovementJob> _logger;

        public ContinuousImprovementJob(
            GrcDbContext context,
            INotificationService notificationService,
            ILogger<ContinuousImprovementJob> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _logger = logger;
        }

        [AutomaticRetry(Attempts = 3)]
        public async Task TrackImprovementInitiativesAsync()
        {
            _logger.LogInformation("ContinuousImprovementJob started at {Time}", DateTime.UtcNow);

            var tenants = await _context.Tenants.AsNoTracking()
                .Where(t => t.IsActive).Select(t => t.Id).ToListAsync();

            var updates = 0;
            foreach (var tenantId in tenants)
            {
                updates += await ProcessTenantImprovementsAsync(tenantId);
            }

            _logger.LogInformation("ContinuousImprovementJob completed. Updates: {Count}", updates);
        }

        private async Task<int> ProcessTenantImprovementsAsync(Guid tenantId)
        {
            var updateCount = 0;
            var today = DateTime.UtcNow.Date;

            // Get active improvement initiatives
            var initiatives = await _context.ImprovementInitiatives
                .Where(i => i.TenantId == tenantId && i.IsActive)
                .Where(i => i.Status != "Completed" && i.Status != "Cancelled")
                .ToListAsync();

            foreach (var initiative in initiatives)
            {
                // Check for overdue initiatives
                if (initiative.TargetDate.HasValue && initiative.TargetDate.Value < today)
                {
                    if (initiative.Status != "Overdue")
                    {
                        initiative.Status = "Overdue";
                        initiative.ModifiedDate = DateTime.UtcNow;
                        updateCount++;

                        if (initiative.OwnerId.HasValue)
                        {
                            await _notificationService.SendNotificationAsync(
                                initiative.Id, initiative.OwnerId.Value.ToString(), "InitiativeOverdue",
                                $"[OVERDUE] Improvement Initiative: {initiative.Title}",
                                $"Initiative is past target date.\n\n" +
                                $"Initiative: {initiative.Title}\n" +
                                $"Target Date: {initiative.TargetDate:yyyy-MM-dd}",
                                "High", tenantId);
                        }
                    }
                }

                // Send progress reminders for active initiatives
                var daysSinceUpdate = (today - (initiative.LastProgressUpdate ?? initiative.CreatedDate).Date).Days;
                if (daysSinceUpdate >= 14 && initiative.OwnerId.HasValue)
                {
                    await _notificationService.SendNotificationAsync(
                        initiative.Id, initiative.OwnerId.Value.ToString(), "InitiativeProgressReminder",
                        $"Progress Update Needed: {initiative.Title}",
                        $"No progress update in {daysSinceUpdate} days.\n\n" +
                        $"Initiative: {initiative.Title}\n" +
                        $"Current Progress: {initiative.ProgressPercent}%",
                        "Medium", tenantId);
                    updateCount++;
                }
            }

            // Analyze trends and identify new improvement opportunities
            var complianceGaps = await _context.TenantControlSets
                .Where(c => c.TenantId == tenantId && c.IsActive &&
                           c.ComplianceStatus != "COMPLIANT")
                .CountAsync();

            var openRisks = await _context.Risks
                .Where(r => r.TenantId == tenantId && r.IsActive &&
                           r.Status != "Closed" && r.ResidualRiskLevel == "High")
                .CountAsync();

            // If significant gaps exist and no active improvement initiative addresses them
            if (complianceGaps > 10 || openRisks > 5)
            {
                var hasActiveInitiative = await _context.ImprovementInitiatives
                    .AnyAsync(i => i.TenantId == tenantId && i.IsActive &&
                                  i.Status != "Completed" && i.Status != "Cancelled");

                if (!hasActiveInitiative)
                {
                    var managers = await _context.TenantUsers
                        .Where(u => u.TenantId == tenantId && u.IsActive &&
                                   (u.Role == "ComplianceManager" || u.Role == "Admin"))
                        .Select(u => u.UserId)
                        .FirstOrDefaultAsync();

                    if (managers != Guid.Empty)
                    {
                        await _notificationService.SendNotificationAsync(
                            tenantId, managers.ToString(), "ImprovementOpportunity",
                            $"[INSIGHT] Improvement Opportunity Identified",
                            $"Analysis has identified improvement opportunities.\n\n" +
                            $"Compliance Gaps: {complianceGaps}\n" +
                            $"Open High Risks: {openRisks}\n\n" +
                            $"Consider creating an improvement initiative to address these gaps.",
                            "Medium", tenantId);
                        updateCount++;
                    }
                }
            }

            await _context.SaveChangesAsync();
            return updateCount;
        }
    }
}
