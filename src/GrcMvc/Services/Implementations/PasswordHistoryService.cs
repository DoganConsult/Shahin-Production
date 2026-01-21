using System;
using System.Linq;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Password History Service - Prevents password reuse for GRC compliance
    /// SECURITY: Checks new passwords against historical hashes using Identity's password hasher
    /// </summary>
    public class PasswordHistoryService : IPasswordHistoryService
    {
        private readonly GrcAuthDbContext _authContext;
        private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
        private readonly ILogger<PasswordHistoryService> _logger;
        private readonly int _defaultHistoryCount;
        private readonly int _retentionLimit;

        public PasswordHistoryService(
            GrcAuthDbContext authContext,
            IPasswordHasher<ApplicationUser> passwordHasher,
            ILogger<PasswordHistoryService> logger,
            IConfiguration configuration)
        {
            _authContext = authContext;
            _passwordHasher = passwordHasher;
            _logger = logger;
            
            // Configuration for password history
            _defaultHistoryCount = configuration.GetValue<int>("Security:PasswordHistory:HistoryCount", 5);
            _retentionLimit = configuration.GetValue<int>("Security:PasswordHistory:RetentionLimit", 10);
        }

        /// <summary>
        /// Check if the new password matches any of the recent password hashes
        /// SECURITY: Uses Identity's password hasher to verify against stored hashes
        /// </summary>
        public async Task<bool> IsPasswordInHistoryAsync(string userId, string newPassword, int historyCount = 0)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(newPassword))
            {
                return false;
            }

            // Use default if not specified
            if (historyCount <= 0)
            {
                historyCount = _defaultHistoryCount;
            }

            try
            {
                // Get the most recent password hashes for this user
                var recentPasswordHashes = await _authContext.PasswordHistory
                    .Where(ph => ph.UserId == userId)
                    .OrderByDescending(ph => ph.ChangedAt)
                    .Take(historyCount)
                    .Select(ph => ph.PasswordHash)
                    .ToListAsync();

                if (!recentPasswordHashes.Any())
                {
                    _logger.LogDebug("No password history found for user {UserId}", userId);
                    return false;
                }

                // Create a dummy user for password verification
                var dummyUser = new ApplicationUser { Id = userId };

                // Check if new password matches any historical hash
                foreach (var historicalHash in recentPasswordHashes)
                {
                    var verificationResult = _passwordHasher.VerifyHashedPassword(
                        dummyUser, 
                        historicalHash, 
                        newPassword);

                    if (verificationResult == PasswordVerificationResult.Success ||
                        verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
                    {
                        _logger.LogWarning("Password reuse detected for user {UserId}", userId);
                        return true; // Password was found in history - reject it
                    }
                }

                return false; // Password is not in history - allow it
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking password history for user {UserId}", userId);
                // On error, don't block the password change - log and continue
                return false;
            }
        }

        /// <summary>
        /// Add a password hash to the user's history
        /// </summary>
        public async Task AddPasswordToHistoryAsync(
            string userId,
            string passwordHash,
            string? changedByUserId = null,
            string? reason = null,
            string? ipAddress = null,
            string? userAgent = null)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(passwordHash))
            {
                _logger.LogWarning("Cannot add empty userId or passwordHash to history");
                return;
            }

            try
            {
                var historyEntry = new PasswordHistory
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    PasswordHash = passwordHash,
                    ChangedAt = DateTime.UtcNow,
                    ChangedByUserId = changedByUserId ?? userId,
                    Reason = reason ?? "Password change",
                    IpAddress = ipAddress,
                    UserAgent = userAgent
                };

                _authContext.PasswordHistory.Add(historyEntry);
                await _authContext.SaveChangesAsync();

                _logger.LogInformation("Password history entry added for user {UserId}", userId);

                // Clean up old entries if we're over the retention limit
                await CleanupOldHistoryAsync(userId, _retentionLimit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding password to history for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Get the count of password history entries for a user
        /// </summary>
        public async Task<int> GetPasswordHistoryCountAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return 0;
            }

            return await _authContext.PasswordHistory
                .CountAsync(ph => ph.UserId == userId);
        }

        /// <summary>
        /// Remove old password history entries beyond the retention limit
        /// </summary>
        public async Task CleanupOldHistoryAsync(string userId, int retainCount = 0)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return;
            }

            if (retainCount <= 0)
            {
                retainCount = _retentionLimit;
            }

            try
            {
                // Get IDs of entries to keep (most recent)
                var idsToKeep = await _authContext.PasswordHistory
                    .Where(ph => ph.UserId == userId)
                    .OrderByDescending(ph => ph.ChangedAt)
                    .Take(retainCount)
                    .Select(ph => ph.Id)
                    .ToListAsync();

                // Delete older entries
                var entriesToDelete = await _authContext.PasswordHistory
                    .Where(ph => ph.UserId == userId && !idsToKeep.Contains(ph.Id))
                    .ToListAsync();

                if (entriesToDelete.Any())
                {
                    _authContext.PasswordHistory.RemoveRange(entriesToDelete);
                    await _authContext.SaveChangesAsync();
                    _logger.LogDebug("Cleaned up {Count} old password history entries for user {UserId}", 
                        entriesToDelete.Count, userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error cleaning up password history for user {UserId}", userId);
                // Don't throw - cleanup failure shouldn't block operations
            }
        }
    }
}
