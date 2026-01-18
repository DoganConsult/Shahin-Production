using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.BackgroundJobs
{
    /// <summary>
    /// Stage 1: Assessment Automation - Auto-creates assessments based on onboarding cadence.
    /// Runs daily at 6 AM
    /// </summary>
    public class AssessmentSchedulerJob
    {
        private readonly GrcDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly ILogger<AssessmentSchedulerJob> _logger;

        public AssessmentSchedulerJob(GrcDbContext context, INotificationService notificationService, ILogger<AssessmentSchedulerJob> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _logger = logger;
        }

        [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 300, 900 })]
        public async Task CreateScheduledAssessmentsAsync()
        {
            _logger.LogInformation("AssessmentSchedulerJob started at {Time}", DateTime.UtcNow);
            var tenants = await _context.Tenants.AsNoTracking().Where(t => t.IsActive && t.OnboardingStatus == "COMPLETED").Select(t => t.Id).ToListAsync();
            var total = 0;
            foreach (var tenantId in tenants) { total += await ProcessTenantAsync(tenantId); }
            _logger.LogInformation("AssessmentSchedulerJob completed. Created {Count} assessments", total);
        }

        private async Task<int> ProcessTenantAsync(Guid tenantId)
        {
            var count = 0;
            var frameworks = await _context.TenantFrameworkSelections.Where(f => f.TenantId == tenantId && f.IsActive).ToListAsync();
            foreach (var fw in frameworks)
            {
                var cadence = fw.AssessmentCadence ?? "ANNUAL";
                var lastAssessment = await _context.Assessments.Where(a => a.TenantId == tenantId && a.FrameworkId.ToString() == fw.FrameworkCode).OrderByDescending(a => a.CreatedDate).FirstOrDefaultAsync();
                var days = lastAssessment != null ? (DateTime.UtcNow - lastAssessment.CreatedDate).TotalDays : 999;
                var due = cadence.ToUpper() switch { "MONTHLY" => days >= 28, "QUARTERLY" => days >= 85, "SEMI_ANNUAL" => days >= 170, _ => days >= 350 };
                if (due)
                {
                    var assessment = new Assessment { Id = Guid.NewGuid(), TenantId = tenantId, Name = $"{fw.FrameworkCode} Assessment - {DateTime.UtcNow:yyyy-MM}", Status = "Draft", Type = "Compliance", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(cadence == "MONTHLY" ? 21 : 90), CreatedDate = DateTime.UtcNow, CreatedBy = "System:AssessmentSchedulerJob", IsActive = true };
                    _context.Assessments.Add(assessment);
                    count++;
                }
            }
            await _context.SaveChangesAsync();
            return count;
        }
    }

    /// <summary>
    /// Stage 1: Assessment Reminder - Sends reminders for upcoming assessments.
    /// Runs daily at 8 AM
    /// </summary>
    public class AssessmentReminderJob
    {
        private readonly GrcDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly ILogger<AssessmentReminderJob> _logger;

        public AssessmentReminderJob(GrcDbContext context, INotificationService notificationService, ILogger<AssessmentReminderJob> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _logger = logger;
        }

        [AutomaticRetry(Attempts = 3)]
        public async Task SendAssessmentRemindersAsync()
        {
            _logger.LogInformation("AssessmentReminderJob started at {Time}", DateTime.UtcNow);
            var today = DateTime.UtcNow.Date;
            var reminders = new[] { 14, 7, 3, 1, 0 };
            var assessments = await _context.Assessments.Where(a => a.IsActive && a.Status != "Completed" && a.Status != "Approved" && a.EndDate >= today && a.EndDate <= today.AddDays(14)).ToListAsync();
            var sent = 0;
            foreach (var a in assessments)
            {
                var days = (a.EndDate.Date - today).Days;
                if (reminders.Contains(days) && a.AssessorId.HasValue)
                {
                    await _notificationService.SendNotificationAsync(a.Id, a.AssessorId.Value.ToString(), "AssessmentReminder", days == 0 ? $"[DUE TODAY] {a.Name}" : $"Assessment Due in {days} days: {a.Name}", $"Assessment: {a.Name}\nDue: {a.EndDate:yyyy-MM-dd}", days <= 1 ? "Critical" : "Medium", a.TenantId);
                    sent++;
                }
            }
            _logger.LogInformation("AssessmentReminderJob completed. Sent {Count} reminders", sent);
        }
    }
}
