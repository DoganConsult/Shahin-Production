using GrcMvc.Data;
using GrcMvc.Services.Interfaces;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.BackgroundJobs
{
    /// <summary>
    /// Stage 3: Evidence Reminder - Sends reminders for pending evidence.
    /// Runs daily at 9 AM
    /// </summary>
    public class EvidenceReminderJob
    {
        private readonly GrcDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly ILogger<EvidenceReminderJob> _logger;
        private static readonly int[] ReminderDays = { 7, 3, 1, 0 };

        public EvidenceReminderJob(GrcDbContext context, INotificationService notificationService, ILogger<EvidenceReminderJob> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _logger = logger;
        }

        [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 300, 900 })]
        public async Task SendRemindersAsync()
        {
            _logger.LogInformation("EvidenceReminderJob started at {Time}", DateTime.UtcNow);
            var tenants = await _context.Tenants.AsNoTracking().Where(t => t.IsActive).Select(t => t.Id).ToListAsync();
            var total = 0;
            foreach (var tenantId in tenants) { total += await ProcessTenantAsync(tenantId); }
            _logger.LogInformation("EvidenceReminderJob completed. Sent {Count} reminders", total);
        }

        private async Task<int> ProcessTenantAsync(Guid tenantId)
        {
            var count = 0;
            var today = DateTime.UtcNow.Date;
            var evidence = await _context.EvidencePacks.Where(e => e.TenantId == tenantId && (e.Status == "Pending" || e.Status == "InProgress" || e.Status == "Requested") && e.DueDate.HasValue && e.DueDate.Value.Date >= today).ToListAsync();
            foreach (var e in evidence)
            {
                var days = (e.DueDate!.Value.Date - today).Days;
                if (ReminderDays.Contains(days) && e.AssignedToUserId.HasValue && (e.LastReminderSentAt == null || e.LastReminderSentAt.Value.Date != today))
                {
                    var urgency = days switch { 0 => "Critical", <= 1 => "High", _ => "Medium" };
                    await _notificationService.SendNotificationAsync(e.Id, e.AssignedToUserId.Value.ToString(), "EvidenceReminder", days == 0 ? $"[DUE TODAY] Evidence: {e.ControlCode}" : $"Evidence Due in {days} days: {e.ControlCode}", $"Control: {e.ControlCode}\nEvidence: {e.Title}\nDue: {e.DueDate:yyyy-MM-dd}", urgency, tenantId);
                    e.LastReminderSentAt = DateTime.UtcNow;
                    e.ReminderCount = (e.ReminderCount ?? 0) + 1;
                    count++;
                }
            }
            await _context.SaveChangesAsync();
            return count;
        }
    }
}
