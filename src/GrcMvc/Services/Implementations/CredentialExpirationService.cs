using System;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Service to check and enforce credential expiration for tenant users.
    /// HIGH FIX: Implements credential expiration enforcement that was previously unused.
    /// </summary>
    public class CredentialExpirationService : ICredentialExpirationService
    {
        private readonly GrcDbContext _context;
        private readonly ILogger<CredentialExpirationService> _logger;

        public CredentialExpirationService(
            GrcDbContext context,
            ILogger<CredentialExpirationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Check if user's credentials have expired for a specific tenant.
        /// </summary>
        public async Task<CredentialStatus> CheckCredentialStatusAsync(string userId, Guid tenantId)
        {
            var tenantUser = await _context.TenantUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(tu => tu.UserId == userId && tu.TenantId == tenantId && !tu.IsDeleted);

            if (tenantUser == null)
            {
                return new CredentialStatus
                {
                    IsValid = false,
                    Reason = "User not found in tenant"
                };
            }

            // Check if credentials have expired
            if (tenantUser.CredentialExpiresAt.HasValue && tenantUser.CredentialExpiresAt.Value < DateTime.UtcNow)
            {
                _logger.LogWarning("Credentials expired for user {UserId} in tenant {TenantId}. Expired at: {ExpiresAt}",
                    userId, tenantId, tenantUser.CredentialExpiresAt.Value);

                return new CredentialStatus
                {
                    IsValid = false,
                    IsExpired = true,
                    ExpiresAt = tenantUser.CredentialExpiresAt.Value,
                    Reason = "Credentials have expired",
                    RequiresPasswordChange = true
                };
            }

            // Check if password change is required on first login
            if (tenantUser.MustChangePasswordOnFirstLogin)
            {
                _logger.LogInformation("User {UserId} must change password on first login for tenant {TenantId}",
                    userId, tenantId);

                return new CredentialStatus
                {
                    IsValid = true,
                    RequiresPasswordChange = true,
                    Reason = "Password change required on first login"
                };
            }

            // Check if credentials are expiring soon (within 7 days)
            if (tenantUser.CredentialExpiresAt.HasValue)
            {
                var daysUntilExpiry = (tenantUser.CredentialExpiresAt.Value - DateTime.UtcNow).TotalDays;
                if (daysUntilExpiry <= 7 && daysUntilExpiry > 0)
                {
                    return new CredentialStatus
                    {
                        IsValid = true,
                        IsExpiringSoon = true,
                        ExpiresAt = tenantUser.CredentialExpiresAt.Value,
                        DaysUntilExpiry = (int)Math.Ceiling(daysUntilExpiry),
                        Reason = $"Credentials expire in {(int)Math.Ceiling(daysUntilExpiry)} days"
                    };
                }
            }

            return new CredentialStatus
            {
                IsValid = true,
                ExpiresAt = tenantUser.CredentialExpiresAt
            };
        }

        /// <summary>
        /// Mark that user has changed their password (clears MustChangePasswordOnFirstLogin).
        /// </summary>
        public async Task<bool> MarkPasswordChangedAsync(string userId, Guid tenantId)
        {
            var tenantUser = await _context.TenantUsers
                .FirstOrDefaultAsync(tu => tu.UserId == userId && tu.TenantId == tenantId && !tu.IsDeleted);

            if (tenantUser == null)
                return false;

            tenantUser.MustChangePasswordOnFirstLogin = false;
            tenantUser.ModifiedDate = DateTime.UtcNow;
            tenantUser.ModifiedBy = userId;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Password change requirement cleared for user {UserId} in tenant {TenantId}",
                userId, tenantId);

            return true;
        }

        /// <summary>
        /// Extend credential expiration for a user.
        /// </summary>
        public async Task<bool> ExtendCredentialExpirationAsync(string userId, Guid tenantId, int additionalDays, string extendedBy)
        {
            var tenantUser = await _context.TenantUsers
                .FirstOrDefaultAsync(tu => tu.UserId == userId && tu.TenantId == tenantId && !tu.IsDeleted);

            if (tenantUser == null)
                return false;

            var currentExpiration = tenantUser.CredentialExpiresAt ?? DateTime.UtcNow;
            var newExpiration = currentExpiration.AddDays(additionalDays);

            tenantUser.CredentialExpiresAt = newExpiration;
            tenantUser.ModifiedDate = DateTime.UtcNow;
            tenantUser.ModifiedBy = extendedBy;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Credential expiration extended for user {UserId} in tenant {TenantId}. New expiration: {NewExpiration}",
                userId, tenantId, newExpiration);

            return true;
        }
    }

    /// <summary>
    /// Status of user credentials.
    /// </summary>
    public class CredentialStatus
    {
        public bool IsValid { get; set; }
        public bool IsExpired { get; set; }
        public bool IsExpiringSoon { get; set; }
        public bool RequiresPasswordChange { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public int? DaysUntilExpiry { get; set; }
        public string? Reason { get; set; }
    }
}
