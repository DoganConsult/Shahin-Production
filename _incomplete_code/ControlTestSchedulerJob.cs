using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.BackgroundJobs
{
    /// <summary>
    /// Stage 3: Control Test Scheduler - Schedules control tests based on frequency.
    /// Runs daily at 7 AM
    /// </summary>
    public class ControlTestSchedulerJob
    {
        private readonly GrcDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly ILogger<ControlTestSchedulerJob> _logger;

        public ControlTestSchedulerJob(GrcDbContext context, INotificationService notificationService, ILogger<ControlTestSchedulerJob> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _logger = logger;
        }

        [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 300, 900 })]
        public async Task ScheduleDueTestsAsync()
        {
            _logger.LogInformation("ControlTestSchedulerJob started at {Time}", DateTime.UtcNow);
            var tenants = await _context.Tenants.AsNoTracking().Where(t => t.IsActive && t.OnboardingStatus == "COMPLETED").Select(t => t.Id).ToListAsync();
            var total = 0;
            foreach (var tenantId in tenants) { total += await ProcessTenantAsync(tenantId); }
            _logger.LogInformation("ControlTestSchedulerJob completed. Scheduled {Count} tests", total);
        }

        private async Task<int> ProcessTenantAsync(Guid tenantId)
        {
            var count = 0;
            var today = DateTime.UtcNow.Date;
            var controls = await _context.TenantControlSets.Where(c => c.TenantId == tenantId && c.IsActive && c.ApplicabilityStatus == "Applicable").ToListAsync();
            foreach (var control in controls)
            {
                var freq = control.EvidenceFrequency ?? "QUARTERLY";
                var days = control.LastTestedAt.HasValue ? (today - control.LastTestedAt.Value.Date).TotalDays : 999;
                var due = freq.ToUpper() switch { "DAILY" => days >= 1, "WEEKLY" => days >= 7, "MONTHLY" => days >= 28, "QUARTERLY" => days >= 85, _ => days >= 350 };
                if (!due) continue;
                var exists = await _context.ControlTests.AnyAsync(t => t.TenantId == tenantId && t.ControlId == control.Id && t.Status != "Completed" && t.Status != "Cancelled");
                if (exists) continue;
                var test = new ControlTest { Id = Guid.NewGuid(), TenantId = tenantId, ControlId = control.Id, ControlCode = control.ControlCode, ControlName = control.ControlName, TestType = freq == "CONTINUOUS" ? "Automated" : "FullTest", Status = "Scheduled", ScheduledDate = DateTime.UtcNow, DueDate = DateTime.UtcNow.AddDays(freq == "MONTHLY" ? 21 : 30), AssignedToUserId = control.PrimaryOwnerId, Priority = freq is "CONTINUOUS" or "DAILY" or "WEEKLY" ? "High" : "Medium", CreatedDate = DateTime.UtcNow, CreatedBy = "System:ControlTestSchedulerJob", IsActive = true };
                _context.ControlTests.Add(test);
                count++;
            }
            await _context.SaveChangesAsync();
            return count;
        }
    }
}
