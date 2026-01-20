using GrcMvc.Configuration;
using GrcMvc.Constants;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Models.Enums;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Http;
using Volo.Abp.Identity;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// AM-08: Secure Password Reset Service implementation.
    /// Handles password reset flow with rate limiting, token management, and audit logging.
    /// </summary>
    public class SecurePasswordResetService : ISecurePasswordResetService
    {
        private readonly GrcDbContext _dbContext;
        private readonly IIdentityUserAppService _identityUserAppService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDistributedCache _cache;
        private readonly IAccessManagementAuditService _auditService;
        private readonly IGrcEmailService? _emailService;
        private readonly IHttpClientFactory? _httpClientFactory;
        private readonly PasswordPolicyOptions _policyOptions;
        private readonly RateLimitingOptions _rateLimitOptions;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SecurePasswordResetService> _logger;

        private const string RateLimitKeyPrefix = "pwd_reset_rate:";
        private const string LockoutKeyPrefix = "pwd_reset_lockout:";
        private const int MaxFailedResets = 5;
        private const int LockoutMinutes = 15;

        public SecurePasswordResetService(
            GrcDbContext dbContext,
            IIdentityUserAppService identityUserAppService,
            UserManager<ApplicationUser> userManager,
            IDistributedCache cache,
            IAccessManagementAuditService auditService,
            IOptions<AccessManagementOptions> options,
            IConfiguration configuration,
            ILogger<SecurePasswordResetService> logger,
            IGrcEmailService? emailService = null,
            IHttpClientFactory? httpClientFactory = null)
        {
            _dbContext = dbContext;
            _identityUserAppService = identityUserAppService;
            _userManager = userManager;
            _cache = cache;
            _auditService = auditService;
            _emailService = emailService;
            _httpClientFactory = httpClientFactory;
            _policyOptions = options.Value.PasswordPolicy;
            _rateLimitOptions = options.Value.RateLimiting;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<PasswordResetRequestResult> RequestResetAsync(
            string email,
            string? ipAddress,
            string? userAgent)
        {
            try
            {
                // Normalize email
                email = email.Trim().ToLowerInvariant();

                // Check rate limit
                var requestCount = await GetResetRequestCountAsync(email);
                var maxRequests = _rateLimitOptions.PasswordResetLimitPerHour;

                if (requestCount >= maxRequests)
                {
                    await _auditService.LogPasswordResetFailedAsync(
                        Guid.Empty, Guid.Empty, "Rate limit exceeded", "RATE_LIMITED", ipAddress);
                    return PasswordResetRequestResult.RateLimited(0);
                }

                // Find user
                var user = await _userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    // Log internally but return generic message
                    _logger.LogInformation("Password reset requested for non-existent email: {Email}", email);
                    await IncrementRateLimitAsync(email);
                    return PasswordResetRequestResult.UserNotFound();
                }

                var userId = Guid.Parse(user.Id);

                // Check user status
                var userIdString = userId.ToString();
                var tenantUser = await _dbContext.TenantUsers
                    .Where(tu => tu.UserId == userIdString)
                    .FirstOrDefaultAsync();

                if (tenantUser != null)
                {
                    var status = Enum.Parse<UserStatus>(tenantUser.Status);
                    if (status == UserStatus.Deprovisioned)
                    {
                        _logger.LogWarning("Password reset requested for deprovisioned user: {UserId}", userId);
                        return PasswordResetRequestResult.UserNotFound(); // Don't reveal account status
                    }

                    if (status == UserStatus.Suspended)
                    {
                        await _auditService.LogPasswordResetFailedAsync(
                            userId, tenantUser.TenantId, "Account suspended", "ACCOUNT_SUSPENDED", ipAddress);
                        return PasswordResetRequestResult.AccountLocked();
                    }
                }

                // Revoke any existing tokens
                await RevokeAllTokensAsync(userId, "New reset requested");

                // Create new token
                var (token, plaintextToken) = PasswordResetToken.Create(
                    userId,
                    email,
                    _policyOptions.ResetTokenTtlMinutes);

                token.RequestIpAddress = ipAddress;
                token.RequestUserAgent = userAgent;
                token.TenantId = tenantUser?.TenantId;

                _dbContext.Set<PasswordResetToken>().Add(token);
                await _dbContext.SaveChangesAsync();

                // Increment rate limit counter
                await IncrementRateLimitAsync(email);

                // Log the request
                await _auditService.LogPasswordResetRequestedAsync(
                    email,
                    ipAddress);

                // Send email with reset link
                if (_emailService != null && user != null)
                {
                    try
                    {
                        var baseUrl = _policyOptions?.BaseUrl ?? 
                                     _configuration["App:BaseUrl"] ?? 
                                     _configuration["App:SelfUrl"] ?? 
                                     "https://portal.shahin-ai.com";
                        var resetLink = $"{baseUrl}/Account/ResetPassword?token={Uri.EscapeDataString(plaintextToken)}&email={Uri.EscapeDataString(email)}";
                        var userName = $"{user.FirstName} {user.LastName}".Trim();
                        if (string.IsNullOrEmpty(userName))
                            userName = user.Email ?? email;
                        await _emailService.SendPasswordResetEmailAsync(email, userName, resetLink, isArabic: false);
                        _logger.LogInformation("Password reset email sent to {Email}", email);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send password reset email to {Email}", email);
                        // Continue - don't fail the request if email fails
                    }
                }
                else
                {
                    // Fallback: log token (DEV ONLY - REMOVE IN PRODUCTION)
                    _logger.LogWarning(
                        "Password reset token generated for user {UserId} but email service not available. Token (DEV ONLY): {Token}",
                        userId, plaintextToken);
                }

                var remaining = maxRequests - requestCount - 1;
                return PasswordResetRequestResult.Success(remaining > 0 ? remaining : 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing password reset request for {Email}", email);
                return new PasswordResetRequestResult
                {
                    InternalStatus = PasswordResetInternalStatus.Error
                };
            }
        }

        public async Task<PasswordResetValidationResult> ValidateTokenAsync(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                    return PasswordResetValidationResult.NotFound();

                var tokenHash = PasswordResetToken.HashToken(token);

                var resetToken = await _dbContext.Set<PasswordResetToken>()
                    .Where(t => t.TokenHash == tokenHash)
                    .FirstOrDefaultAsync();

                if (resetToken == null)
                    return PasswordResetValidationResult.NotFound();

                if (resetToken.IsUsed)
                    return PasswordResetValidationResult.AlreadyUsed();

                if (resetToken.IsRevoked)
                    return PasswordResetValidationResult.Revoked();

                if (resetToken.ExpiresAt <= DateTime.UtcNow)
                    return PasswordResetValidationResult.Expired();

                return PasswordResetValidationResult.Valid(
                    resetToken.UserId,
                    resetToken.Email,
                    resetToken.ExpiresAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating password reset token");
                return PasswordResetValidationResult.Invalid("An error occurred validating the reset link.");
            }
        }

        public async Task<PasswordResetCompleteResult> CompleteResetAsync(
            string token,
            string newPassword,
            string? ipAddress)
        {
            try
            {
                // Validate token
                var validation = await ValidateTokenAsync(token);
                if (!validation.IsValid || !validation.UserId.HasValue)
                {
                    return PasswordResetCompleteResult.Failed(validation.ErrorMessage ?? "Invalid token");
                }

                var userId = validation.UserId.Value;

                // Validate password policy
                var policyResult = await ValidatePasswordPolicyAsync(newPassword);
                if (!policyResult.IsValid)
                {
                    return PasswordResetCompleteResult.PolicyFailed(policyResult.Violations.ToArray());
                }

                // Get user
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    return PasswordResetCompleteResult.Failed("User not found");
                }

                // Check password history
                if (!await CheckPasswordHistoryAsync(userId, newPassword))
                {
                    return PasswordResetCompleteResult.PolicyFailed(new[]
                    {
                        $"Cannot reuse any of your last {_policyOptions.PasswordHistoryCount} passwords."
                    });
                }

                // Reset the password
                var resetResult = await _userManager.RemovePasswordAsync(user);
                if (!resetResult.Succeeded)
                {
                    var errors = string.Join(", ", resetResult.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to remove old password: {Errors}", errors);
                    return PasswordResetCompleteResult.Failed("Failed to reset password");
                }

                var addResult = await _userManager.AddPasswordAsync(user, newPassword);
                if (!addResult.Succeeded)
                {
                    var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to set new password: {Errors}", errors);
                    return PasswordResetCompleteResult.Failed("Failed to set new password");
                }

                // Mark token as used
                var tokenHash = PasswordResetToken.HashToken(token);
                var resetToken = await _dbContext.Set<PasswordResetToken>()
                    .Where(t => t.TokenHash == tokenHash)
                    .FirstAsync();

                resetToken.MarkUsed(ipAddress);
                await _dbContext.SaveChangesAsync();

                // Save password to history
                await SavePasswordHistoryAsync(userId, newPassword);

                // Log successful reset
                await _auditService.LogPasswordResetCompletedAsync(
                    userId,
                    validation.Email!,
                    ipAddress);

                _logger.LogInformation("Password reset completed for user {UserId}", userId);

                return PasswordResetCompleteResult.Succeeded();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing password reset");
                return PasswordResetCompleteResult.Failed("An error occurred resetting your password.");
            }
        }

        public async Task<PasswordPolicyResult> ValidatePasswordPolicyAsync(string password)
        {
            var violations = new List<string>();

            if (string.IsNullOrEmpty(password))
            {
                return PasswordPolicyResult.Invalid("Password is required.");
            }

            // Length check
            if (password.Length < _policyOptions.MinLength)
            {
                violations.Add($"Password must be at least {_policyOptions.MinLength} characters.");
            }

            // Complexity checks
            if (_policyOptions.RequireDigit && !password.Any(char.IsDigit))
            {
                violations.Add("Password must contain at least one digit.");
            }

            if (_policyOptions.RequireUppercase && !password.Any(char.IsUpper))
            {
                violations.Add("Password must contain at least one uppercase letter.");
            }

            if (_policyOptions.RequireLowercase && !password.Any(char.IsLower))
            {
                violations.Add("Password must contain at least one lowercase letter.");
            }

            if (_policyOptions.RequireNonAlphanumeric && password.All(char.IsLetterOrDigit))
            {
                violations.Add("Password must contain at least one special character.");
            }

            // Check for common patterns
            if (IsCommonPassword(password))
            {
                violations.Add("This password is too common. Please choose a stronger password.");
            }

            // Check breached passwords (if enabled)
            if (_policyOptions.CheckBreachedPasswords)
            {
                var isBreached = await CheckBreachedPasswordAsync(password);
                if (isBreached)
                {
                    return new PasswordPolicyResult
                    {
                        IsValid = false,
                        IsBreached = true,
                        Violations = new List<string>
                        {
                            "This password has been found in known data breaches. Please choose a different password."
                        }
                    };
                }
            }

            if (violations.Any())
            {
                return PasswordPolicyResult.Invalid(violations.ToArray());
            }

            return PasswordPolicyResult.Valid();
        }

        public async Task RevokeAllTokensAsync(Guid userId, string reason)
        {
            var tokens = await _dbContext.Set<PasswordResetToken>()
                .Where(t => t.UserId == userId && !t.IsUsed && !t.IsRevoked)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.Revoke(reason);
            }

            if (tokens.Any())
            {
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Revoked {Count} password reset tokens for user {UserId}", tokens.Count, userId);
            }
        }

        public async Task<PasswordResetLockoutStatus> GetLockoutStatusAsync(string email)
        {
            email = email.Trim().ToLowerInvariant();
            var lockoutKey = $"{LockoutKeyPrefix}{email}";

            var lockoutData = await _cache.GetStringAsync(lockoutKey);
            if (!string.IsNullOrEmpty(lockoutData))
            {
                var lockout = JsonSerializer.Deserialize<LockoutData>(lockoutData);
                if (lockout != null && lockout.LockoutEnd > DateTime.UtcNow)
                {
                    return new PasswordResetLockoutStatus
                    {
                        IsLockedOut = true,
                        LockoutEnd = lockout.LockoutEnd,
                        FailedAttempts = lockout.FailedAttempts,
                        RemainingAttempts = 0
                    };
                }
            }

            var failureKey = $"pwd_reset_failures:{email}";
            var failuresData = await _cache.GetStringAsync(failureKey);
            var failures = string.IsNullOrEmpty(failuresData) ? 0 : int.Parse(failuresData);

            return new PasswordResetLockoutStatus
            {
                IsLockedOut = false,
                FailedAttempts = failures,
                RemainingAttempts = MaxFailedResets - failures
            };
        }

        public async Task<int> GetResetRequestCountAsync(string email, int windowHours = 1)
        {
            email = email.Trim().ToLowerInvariant();
            var key = $"{RateLimitKeyPrefix}{email}";

            var countData = await _cache.GetStringAsync(key);
            return string.IsNullOrEmpty(countData) ? 0 : int.Parse(countData);
        }

        private async Task IncrementRateLimitAsync(string email)
        {
            var key = $"{RateLimitKeyPrefix}{email}";
            var currentData = await _cache.GetStringAsync(key);
            var current = string.IsNullOrEmpty(currentData) ? 0 : int.Parse(currentData);

            await _cache.SetStringAsync(key, (current + 1).ToString(), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            });
        }

        private async Task<bool> CheckPasswordHistoryAsync(Guid userId, string newPassword)
        {
            if (_policyOptions.PasswordHistoryCount <= 0)
                return true;

            // Get password history from cache or database
            var historyKey = $"pwd_history:{userId}";
            var historyData = await _cache.GetStringAsync(historyKey);

            if (string.IsNullOrEmpty(historyData))
                return true;

            var history = JsonSerializer.Deserialize<List<string>>(historyData) ?? new();

            // Check if new password matches any in history
            // Note: In production, store hashed passwords in history
            foreach (var oldPasswordHash in history.Take(_policyOptions.PasswordHistoryCount))
            {
                // This is simplified - in production, use proper password hashing comparison
                if (oldPasswordHash == ComputeSimpleHash(newPassword))
                {
                    return false;
                }
            }

            return true;
        }

        private async Task SavePasswordHistoryAsync(Guid userId, string password)
        {
            if (_policyOptions.PasswordHistoryCount <= 0)
                return;

            var historyKey = $"pwd_history:{userId}";
            var historyData = await _cache.GetStringAsync(historyKey);
            var history = string.IsNullOrEmpty(historyData)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(historyData) ?? new();

            // Add new password hash to front
            history.Insert(0, ComputeSimpleHash(password));

            // Keep only the configured number of passwords
            if (history.Count > _policyOptions.PasswordHistoryCount)
            {
                history = history.Take(_policyOptions.PasswordHistoryCount).ToList();
            }

            await _cache.SetStringAsync(historyKey, JsonSerializer.Serialize(history), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(365)
            });
        }

        private static string ComputeSimpleHash(string input)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(input);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }

        private static bool IsCommonPassword(string password)
        {
            // Check against common passwords list
            var commonPasswords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "password", "123456", "12345678", "qwerty", "abc123",
                "password1", "password123", "letmein", "welcome",
                "admin", "administrator", "root", "master", "login"
            };

            var normalized = password.ToLowerInvariant();

            // Check exact match
            if (commonPasswords.Contains(normalized))
                return true;

            // Check for keyboard patterns
            var keyboardPatterns = new[] { "qwerty", "asdfgh", "zxcvbn", "123456", "654321" };
            foreach (var pattern in keyboardPatterns)
            {
                if (normalized.Contains(pattern))
                    return true;
            }

            // Check for repeated characters
            if (Regex.IsMatch(normalized, @"(.)\1{3,}"))
                return true;

            return false;
        }

        private async Task<bool> CheckBreachedPasswordAsync(string password)
        {
            // Implement HaveIBeenPwned API check using k-anonymity (range API)
            // This uses the first 5 characters of the SHA-1 hash to maintain privacy
            try
            {
                if (_httpClientFactory == null)
                {
                    _logger.LogWarning("HttpClientFactory not available, skipping HIBP check");
                    return false;
                }

                using var sha1 = SHA1.Create();
                var hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(password));
                var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToUpperInvariant();
                
                // Get first 5 characters for k-anonymity
                var hashPrefix = hashString.Substring(0, 5);
                var hashSuffix = hashString.Substring(5);

                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(5);
                client.DefaultRequestHeaders.Add("User-Agent", "Shahin-GRC-Platform/1.0");

                var response = await client.GetStringAsync($"https://api.pwnedpasswords.com/range/{hashPrefix}");
                
                // Check if our hash suffix appears in the response
                var lines = response.Split('\n');
                foreach (var line in lines)
                {
                    var parts = line.Split(':');
                    if (parts.Length >= 1 && parts[0].Equals(hashSuffix, StringComparison.OrdinalIgnoreCase))
                    {
                        var count = parts.Length > 1 ? int.Parse(parts[1].Trim()) : 0;
                        _logger.LogWarning("Password found in HIBP database with {Count} breaches", count);
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking HIBP for password breach");
                // On error, allow the password (fail open for availability)
                return false;
            }
        }

        private class LockoutData
        {
            public DateTime LockoutEnd { get; set; }
            public int FailedAttempts { get; set; }
        }
    }
}
