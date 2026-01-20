using GrcMvc.Configuration;
using GrcMvc.Constants;
using GrcMvc.Data;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GrcMvc.BackgroundJobs
{
    /// <summary>
    /// AM-06: Background job for processing invitation expiry.
    /// Runs periodically to expire invitations past their TTL.
    /// </summary>
    public class InvitationExpiryJob
    {
        private readonly GrcDbContext _dbContext;
        private readonly IAccessManagementAuditService _auditService;
        private readonly InvitationOptions _options;
        private readonly ILogger<InvitationExpiryJob> _logger;

        public InvitationExpiryJob(
            GrcDbContext dbContext,
            IAccessManagementAuditService auditService,
            IOptions<AccessManagementOptions> options,
            ILogger<InvitationExpiryJob> logger)
        {
            _dbContext = dbContext;
            _auditService = auditService;
            _options = options.Value.Invitation;
            _logger = logger;
        }

        /// <summary>
        /// Execute the invitation expiry job.
        /// Called by Hangfire on schedule.
        /// </summary>
        public async Task ExecuteAsync()
        {
            _logger.LogInformation("Starting invitation expiry job");

            try
            {
                var now = DateTime.UtcNow;
                var expiredCount = 0;

                // Find pending invitations that have expired
                var expiredInvitations = await _dbContext.TenantUsers
                    .Where(tu => tu.Status == "PendingInvitation")
                    .Where(tu => tu.InvitationExpiresAt != null && tu.InvitationExpiresAt <= now)
                    .ToListAsync();

                foreach (var invitation in expiredInvitations)
                {
                    try
                    {
                        // Update status
                        invitation.Status = "InvitationExpired";
                        invitation.UpdatedAt = now;

                        // Log the expiry
                        await _auditService.LogInvitationExpiredAsync(invitation.Id, invitation.Email, invitation.TenantId);

                        expiredCount++;

                        _logger.LogDebug(
                            "Invitation expired for user {UserId} in tenant {TenantId}",
                            invitation.UserId, invitation.TenantId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Error expiring invitation for user {UserId}",
                            invitation.UserId);
                    }
                }

                if (expiredCount > 0)
                {
                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation("Expired {Count} invitations", expiredCount);
                }
                else
                {
                    _logger.LogDebug("No invitations to expire");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in invitation expiry job");
                throw;
            }
        }

        /// <summary>
        /// Process expired invitations for a specific tenant.
        /// </summary>
        public async Task ProcessTenantExpiriesAsync(Guid tenantId)
        {
            var now = DateTime.UtcNow;

            var expiredInvitations = await _dbContext.TenantUsers
                .Where(tu => tu.TenantId == tenantId)
                .Where(tu => tu.Status == "PendingInvitation")
                .Where(tu => tu.InvitationExpiresAt != null && tu.InvitationExpiresAt <= now)
                .ToListAsync();

            foreach (var invitation in expiredInvitations)
            {
                invitation.Status = "InvitationExpired";
                invitation.UpdatedAt = now;

                await _auditService.LogInvitationExpiredAsync(Guid.Parse(invitation.UserId), invitation.Email, invitation.TenantId);
            }

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation(
                "Expired {Count} invitations for tenant {TenantId}",
                expiredInvitations.Count, tenantId);
        }

        /// <summary>
        /// Check if an invitation is valid (not expired).
        /// </summary>
        public async Task<bool> IsInvitationValidAsync(Guid userId, Guid tenantId)
        {
            var invitation = await _dbContext.TenantUsers
                .Where(tu => tu.UserId == userId.ToString() && tu.TenantId == tenantId)
                .Where(tu => tu.Status == "PendingInvitation")
                .Select(tu => new { tu.InvitationExpiresAt })
                .FirstOrDefaultAsync();

            if (invitation == null)
                return false;

            return invitation.InvitationExpiresAt == null ||
                   invitation.InvitationExpiresAt > DateTime.UtcNow;
        }

        /// <summary>
        /// Get invitation statistics for monitoring.
        /// </summary>
        public async Task<InvitationExpiryStats> GetStatisticsAsync()
        {
            var now = DateTime.UtcNow;
            var lastDay = now.AddDays(-1);
            var lastWeek = now.AddDays(-7);

            var stats = await _dbContext.TenantUsers
                .Where(tu => tu.Status == "PendingInvitation" || tu.Status == "InvitationExpired")
                .GroupBy(tu => tu.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var pendingExpiringSoon = await _dbContext.TenantUsers
                .Where(tu => tu.Status == "PendingInvitation")
                .Where(tu => tu.InvitationExpiresAt != null)
                .Where(tu => tu.InvitationExpiresAt <= now.AddHours(24))
                .CountAsync();

            return new InvitationExpiryStats
            {
                TotalPending = stats.FirstOrDefault(s => s.Status == "PendingInvitation")?.Count ?? 0,
                TotalExpired = stats.FirstOrDefault(s => s.Status == "InvitationExpired")?.Count ?? 0,
                ExpiringSoon = pendingExpiringSoon,
                LastRunAt = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Statistics for invitation expiry monitoring.
    /// </summary>
    public class InvitationExpiryStats
    {
        public int TotalPending { get; set; }
        public int TotalExpired { get; set; }
        public int ExpiringSoon { get; set; } // Expiring in next 24 hours
        public DateTime LastRunAt { get; set; }
    }

    /// <summary>
    /// AM-05: Background job for processing user inactivity suspension.
    /// </summary>
    public class InactivitySuspensionJob
    {
        private readonly GrcDbContext _dbContext;
        private readonly IAccessManagementAuditService _auditService;
        private readonly InactivitySuspensionOptions _options;
        private readonly ILogger<InactivitySuspensionJob> _logger;

        public InactivitySuspensionJob(
            GrcDbContext dbContext,
            IAccessManagementAuditService auditService,
            IOptions<AccessManagementOptions> options,
            ILogger<InactivitySuspensionJob> logger)
        {
            _dbContext = dbContext;
            _auditService = auditService;
            _options = options.Value.InactivitySuspension;
            _logger = logger;
        }

        /// <summary>
        /// Execute the inactivity suspension job.
        /// </summary>
        public async Task ExecuteAsync()
        {
            if (!_options.Enabled)
            {
                _logger.LogDebug("Inactivity suspension is disabled");
                return;
            }

            _logger.LogInformation("Starting inactivity suspension job");

            try
            {
                var now = DateTime.UtcNow;
                var inactivityThreshold = now.AddDays(-_options.InactiveDays);
                var warningThreshold = now.AddDays(-(_options.InactiveDays - _options.WarningDays));

                // Send warnings first
                await SendInactivityWarningsAsync(warningThreshold, inactivityThreshold);

                // Then suspend inactive users
                await SuspendInactiveUsersAsync(inactivityThreshold);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in inactivity suspension job");
                throw;
            }
        }

        private async Task SendInactivityWarningsAsync(DateTime warningThreshold, DateTime suspensionThreshold)
        {
            // Find users who should receive warnings (inactive but not yet at suspension threshold)
            var usersToWarn = await _dbContext.TenantUsers
                .Where(tu => tu.Status == "Active")
                .Where(tu => tu.LastLoginAt != null)
                .Where(tu => tu.LastLoginAt <= warningThreshold)
                .Where(tu => tu.LastLoginAt > suspensionThreshold)
                .ToListAsync();

            foreach (var user in usersToWarn)
            {
                // Skip setting InactivityWarningAt - field doesn't exist
                // user.InactivityWarningAt = DateTime.UtcNow;

                await _auditService.LogInactivityWarningAsync(
                    Guid.Parse(user.UserId),
                    user.Email,
                    user.TenantId,
                    _options.InactiveDays);

                _logger.LogInformation(
                    "Sent inactivity warning to user {UserId} in tenant {TenantId}",
                    user.UserId, user.TenantId);

                // TODO: Send email notification
            }

            if (usersToWarn.Any())
            {
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Sent inactivity warnings to {Count} users", usersToWarn.Count);
            }
        }

        private async Task SuspendInactiveUsersAsync(DateTime threshold)
        {
            var usersToSuspend = await _dbContext.TenantUsers
                .Where(tu => tu.Status == "Active")
                .Where(tu => tu.LastLoginAt != null)
                .Where(tu => tu.LastLoginAt <= threshold)
                .ToListAsync();

            foreach (var user in usersToSuspend)
            {
                user.Status = "InactiveSuspended";
                // Fields don't exist yet - skip for now
                // user.SuspendedAt = DateTime.UtcNow;
                // user.SuspendedReason = $"Inactive for {_options.InactiveDays}+ days";
                user.UpdatedAt = DateTime.UtcNow;

                await _auditService.LogInactivitySuspensionAsync(
                    Guid.Parse(user.UserId),
                    user.Email,
                    user.TenantId,
                    _options.InactiveDays);

                _logger.LogInformation(
                    "Suspended inactive user {UserId} in tenant {TenantId}",
                    user.UserId, user.TenantId);
            }

            if (usersToSuspend.Any())
            {
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Suspended {Count} inactive users", usersToSuspend.Count);
            }
        }
    }
}
