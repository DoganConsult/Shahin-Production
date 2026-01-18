using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.BackgroundJobs
{
    /// <summary>
    /// Stage 3: Evidence Expiry - Marks expired evidence and creates renewal requests.
    /// Runs daily at 6 AM
    /// </summary>
    public class EvidenceExpiryJob
    {
        private readonly GrcDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly ILogger<EvidenceExpiryJob> _logger;

        public EvidenceExpiryJob(GrcDbContext context, INotificationService notificationService, ILogger<EvidenceExpiryJob> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _logger = logger;
        }

        [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 300, 900 })]
        public async Task ProcessExpiredEvidenceAsync()
        {
            _logger.LogInformation("EvidenceExpiryJob started at {Time}", DateTime.UtcNow);
            var tenants = await _context.Tenants.AsNoTracking().Where(t => t.IsActive).Select(t => t.Id).ToListAsync();
            var totalExpired = 0;
            var totalRenewed = 0;
            foreach (var tenantId in tenants) { var (e, r) = await ProcessTenantAsync(tenantId); totalExpired += e; totalRenewed += r; }
            _logger.LogInformation("EvidenceExpiryJob completed. Expired {Expired}, renewed {Renewed}", totalExpired, totalRenewed);
        }

        private async Task<(int expired, int renewed)> ProcessTenantAsync(Guid tenantId)
        {
            var expiredCount = 0;
            var renewedCount = 0;
            var today = DateTime.UtcNow.Date;
            var expired = await _context.EvidencePacks.Where(e => e.TenantId == tenantId && (e.Status == "Approved" || e.Status == "Valid") && e.ValidUntil.HasValue && e.ValidUntil.Value.Date < today).ToListAsync();
            foreach (var e in expired)
            {
                e.Status = "Expired";
                e.ModifiedDate = DateTime.UtcNow;
                expiredCount++;
                if (e.ControlId.HasValue)
                {
                    var control = await _context.TenantControlSets.FindAsync(e.ControlId);
                    if (control != null) { control.ComplianceStatus = "EVIDENCE_EXPIRED"; control.ModifiedDate = DateTime.UtcNow; }
                }
                var renewal = new EvidencePack { Id = Guid.NewGuid(), TenantId = tenantId, ControlId = e.ControlId, ControlCode = e.ControlCode, ControlName = e.ControlName, Title = $"[RENEWAL] {e.Title}", Status = "Requested", Priority = "High", Frequency = e.Frequency, AssignedToUserId = e.AssignedToUserId, DueDate = DateTime.UtcNow.AddDays(14), ValidUntil = DateTime.UtcNow.AddMonths(e.Frequency == "MONTHLY" ? 1 : 3), PreviousEvidenceId = e.Id, CreatedDate = DateTime.UtcNow, CreatedBy = "System:EvidenceExpiryJob", IsActive = true };
                _context.EvidencePacks.Add(renewal);
                renewedCount++;
                if (e.AssignedToUserId.HasValue)
                {
                    await _notificationService.SendNotificationAsync(renewal.Id, e.AssignedToUserId.Value.ToString(), "EvidenceExpired", $"[ACTION] Evidence Expired: {e.ControlCode}", $"Evidence expired. Renewal due: {renewal.DueDate:yyyy-MM-dd}", "High", tenantId);
                }
            }
            await _context.SaveChangesAsync();
            return (expiredCount, renewedCount);
        }
    }
}
