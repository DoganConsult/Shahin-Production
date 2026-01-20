using GrcMvc.Configuration;
using GrcMvc.Constants;
using GrcMvc.Data;
using GrcMvc.Models.DTOs;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Text.Json;
using OtpNet;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// AM-04: Step-Up Authentication Service implementation.
    /// Uses distributed cache for step-up auth state with configurable TTL.
    /// </summary>
    public class StepUpAuthService : IStepUpAuthService
    {
        private readonly IDistributedCache _cache;
        private readonly GrcDbContext _dbContext;
        private readonly IAccessManagementAuditService _auditService;
        private readonly MfaEnforcementOptions _options;
        private readonly ILogger<StepUpAuthService> _logger;

        private const string StepUpCacheKeyPrefix = "stepup:";

        public StepUpAuthService(
            IDistributedCache cache,
            GrcDbContext dbContext,
            IAccessManagementAuditService auditService,
            IOptions<AccessManagementOptions> options,
            ILogger<StepUpAuthService> logger)
        {
            _cache = cache;
            _dbContext = dbContext;
            _auditService = auditService;
            _options = options.Value.MfaEnforcement;
            _logger = logger;
        }

        public async Task<bool> HasValidStepUpAuthAsync(Guid userId, Guid sessionId, string action)
        {
            try
            {
                var cacheKey = GetCacheKey(sessionId, action);
                var cached = await _cache.GetStringAsync(cacheKey);

                if (string.IsNullOrEmpty(cached))
                    return false;

                var stepUpData = JsonSerializer.Deserialize<StepUpAuthData>(cached);
                if (stepUpData == null || stepUpData.UserId != userId)
                    return false;

                // Check if still valid
                return stepUpData.ExpiresAt > DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking step-up auth for user {UserId}, session {SessionId}", userId, sessionId);
                return false;
            }
        }

        public async Task RecordStepUpAuthAsync(Guid userId, Guid sessionId, string action, string method)
        {
            try
            {
                var expiresAt = DateTime.UtcNow.AddMinutes(_options.StepUpValidityMinutes);
                var stepUpData = new StepUpAuthData
                {
                    UserId = userId,
                    SessionId = sessionId,
                    Action = action,
                    Method = method,
                    AuthenticatedAt = DateTime.UtcNow,
                    ExpiresAt = expiresAt
                };

                var cacheKey = GetCacheKey(sessionId, action);
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = expiresAt
                };

                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(stepUpData), options);

                // Log the step-up completion
                await _auditService.LogStepUpCompletedAsync(userId, null, action, method, null);

                _logger.LogInformation(
                    "Step-up auth recorded for user {UserId}, session {SessionId}, action {Action}, valid until {ExpiresAt}",
                    userId, sessionId, action, expiresAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording step-up auth for user {UserId}", userId);
                throw;
            }
        }

        public bool RequiresStepUpAuth(string action)
        {
            if (string.IsNullOrEmpty(action))
                return false;

            // Check against configured step-up actions
            return _options.StepUpActions?.Contains(action, StringComparer.OrdinalIgnoreCase) == true;
        }

        public async Task<TimeSpan?> GetStepUpAuthValidityAsync(Guid userId, Guid sessionId, string action)
        {
            try
            {
                var cacheKey = GetCacheKey(sessionId, action);
                var cached = await _cache.GetStringAsync(cacheKey);

                if (string.IsNullOrEmpty(cached))
                    return null;

                var stepUpData = JsonSerializer.Deserialize<StepUpAuthData>(cached);
                if (stepUpData == null || stepUpData.UserId != userId)
                    return null;

                var remaining = stepUpData.ExpiresAt - DateTime.UtcNow;
                return remaining > TimeSpan.Zero ? remaining : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting step-up validity for user {UserId}", userId);
                return null;
            }
        }

        public async Task InvalidateStepUpAuthAsync(Guid sessionId)
        {
            try
            {
                // Invalidate all step-up actions for this session
                foreach (var action in _options.StepUpActions ?? Array.Empty<string>())
                {
                    var cacheKey = GetCacheKey(sessionId, action);
                    await _cache.RemoveAsync(cacheKey);
                }

                _logger.LogInformation("Step-up auth invalidated for session {SessionId}", sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating step-up auth for session {SessionId}", sessionId);
            }
        }

        public async Task<bool> UserHasMfaEnabledAsync(Guid userId)
        {
            try
            {
                var user = await _dbContext.Users
                    .Where(u => u.Id == userId.ToString())
                    .Select(u => new { u.TwoFactorEnabled })
                    .FirstOrDefaultAsync();

                return user?.TwoFactorEnabled == true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking MFA status for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> IsMfaRequiredForUserAsync(Guid userId, Guid tenantId)
        {
            try
            {
                // Get user's roles in the tenant
                var userIdString = userId.ToString();
                var userRoles = await _dbContext.TenantUsers
                    .Where(tu => tu.UserId == userIdString && tu.TenantId == tenantId)
                    .Select(tu => tu.Role)
                    .ToListAsync();

                if (!userRoles.Any())
                    return false;

                // Check if any role requires MFA
                var requiredRoles = _options.RequiredForRoles ?? Array.Empty<string>();
                return userRoles.Any(role => requiredRoles.Contains(role, StringComparer.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking MFA requirement for user {UserId} in tenant {TenantId}", userId, tenantId);
                return false;
            }
        }

        public async Task<MfaVerificationResult> VerifyMfaCodeAsync(Guid userId, string code)
        {
            try
            {
                // Check for lockout first
                var lockoutKey = $"mfa_lockout:{userId}";
                var lockoutData = await _cache.GetStringAsync(lockoutKey);
                if (!string.IsNullOrEmpty(lockoutData))
                {
                    var lockoutEnd = JsonSerializer.Deserialize<DateTime>(lockoutData);
                    if (lockoutEnd > DateTime.UtcNow)
                    {
                        await _auditService.LogMfaFailedAsync(userId, null, "TOTP", null);
                        return MfaVerificationResult.LockedOut(lockoutEnd);
                    }
                }

                // Get user and verify TOTP code
                var user = await _dbContext.Users.FindAsync(userId.ToString());
                if (user == null)
                {
                    return MfaVerificationResult.Failed("User not found");
                }

                // In a real implementation, this would verify the TOTP code
                // For now, we'll use a placeholder that checks the authenticator key
                var isValid = await VerifyTotpCodeAsync(user, code);

                if (isValid)
                {
                    // Clear failure counter
                    await _cache.RemoveAsync($"mfa_failures:{userId}");
                    await _auditService.LogMfaVerifiedAsync(userId, user?.Email ?? string.Empty, "TOTP", null);
                    return MfaVerificationResult.Succeeded();
                }

                // Track failure
                var failureKey = $"mfa_failures:{userId}";
                var failures = await IncrementFailureCountAsync(failureKey);

                if (failures >= 5)
                {
                    // Lock out for 15 minutes
                    var lockoutUntil = DateTime.UtcNow.AddMinutes(15);
                    await _cache.SetStringAsync(lockoutKey,
                        JsonSerializer.Serialize(lockoutUntil),
                        new DistributedCacheEntryOptions { AbsoluteExpiration = lockoutUntil });

                    await _auditService.LogMfaFailedAsync(userId, null, "TOTP", null);
                    return MfaVerificationResult.LockedOut(lockoutUntil);
                }

                await _auditService.LogMfaFailedAsync(userId, null, "TOTP", null);
                return MfaVerificationResult.Failed("Invalid verification code", 5 - failures);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying MFA code for user {UserId}", userId);
                return MfaVerificationResult.Failed("Verification failed due to an error");
            }
        }

        private async Task<bool> VerifyTotpCodeAsync(dynamic user, string code)
        {
            // Implement proper TOTP verification using Otp.NET
            if (string.IsNullOrEmpty(code) || code.Length != 6)
                return false;

            try
            {
                // Get the authenticator key from user
                string? authenticatorKey = null;
                if (user != null)
                {
                    // Try to get AuthenticatorKey property
                    var keyProperty = user.GetType().GetProperty("AuthenticatorKey");
                    if (keyProperty != null)
                    {
                        authenticatorKey = keyProperty.GetValue(user)?.ToString();
                    }
                }

                if (string.IsNullOrEmpty(authenticatorKey))
                {
                    _logger.LogWarning("No authenticator key found for user, TOTP verification failed");
                    return false;
                }

                // Decode base32 secret key
                var secretBytes = Base32Encoding.ToBytes(authenticatorKey);
                var totp = new OtpNet.Totp(secretBytes);

                // Verify the code with a time step tolerance (default is 1 step = 30 seconds)
                // Allow 1 step before and after current time for clock skew
                var isValid = totp.VerifyTotp(code, out var timeStepMatched, new OtpNet.VerificationWindow(1, 1));

                if (isValid)
                {
                    _logger.LogDebug("TOTP code verified successfully for user. Time step: {TimeStep}", timeStepMatched);
                    return true;
                }

                _logger.LogWarning("TOTP code verification failed for user");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying TOTP code");
                return false;
            }
        }

        private async Task<int> IncrementFailureCountAsync(string key)
        {
            var current = await _cache.GetStringAsync(key);
            var count = string.IsNullOrEmpty(current) ? 0 : int.Parse(current);
            count++;

            await _cache.SetStringAsync(key, count.ToString(), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
            });

            return count;
        }

        private static string GetCacheKey(Guid sessionId, string action)
        {
            return $"{StepUpCacheKeyPrefix}{sessionId}:{action}";
        }

        /// <summary>
        /// Internal data structure for step-up auth state.
        /// </summary>
        private class StepUpAuthData
        {
            public Guid UserId { get; set; }
            public Guid SessionId { get; set; }
            public string Action { get; set; } = string.Empty;
            public string Method { get; set; } = string.Empty;
            public DateTime AuthenticatedAt { get; set; }
            public DateTime ExpiresAt { get; set; }
        }
    }
}
