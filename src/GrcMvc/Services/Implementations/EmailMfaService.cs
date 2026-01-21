using GrcMvc.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Email-based MFA service for two-factor authentication
    /// </summary>
    public class EmailMfaService : IEmailMfaService
    {
        private readonly IGrcEmailService _emailService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<EmailMfaService> _logger;
        
        // Cache key prefix for MFA codes
        private const string MFA_CODE_PREFIX = "mfa_code_";
        private const int CODE_LENGTH = 6;
        private const int CODE_EXPIRY_MINUTES = 10;
        private const int MAX_ATTEMPTS = 3;

        public EmailMfaService(
            IGrcEmailService emailService,
            IMemoryCache cache,
            ILogger<EmailMfaService> logger)
        {
            _emailService = emailService;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Generate and send MFA code via email
        /// </summary>
        public async Task<bool> SendMfaCodeAsync(string userId, string email, string userName)
        {
            try
            {
                // Generate secure 6-digit code
                var code = GenerateSecureCode();
                
                // Store code in cache with expiry
                var cacheKey = GetCacheKey(userId);
                var mfaData = new MfaCodeData
                {
                    Code = code,
                    Email = email,
                    Attempts = 0,
                    CreatedAt = DateTime.UtcNow
                };
                
                _cache.Set(cacheKey, mfaData, TimeSpan.FromMinutes(CODE_EXPIRY_MINUTES));
                
                // Send email with code
                await _emailService.SendMfaCodeEmailAsync(
                    email, 
                    userName, 
                    code, 
                    CODE_EXPIRY_MINUTES, 
                    isArabic: true);
                
                _logger.LogInformation("MFA code sent to {Email} for user {UserId}", email, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send MFA code to {Email}", email);
                return false;
            }
        }

        /// <summary>
        /// Verify the MFA code entered by user
        /// </summary>
        public async Task<MfaVerificationResult> VerifyCodeAsync(string userId, string enteredCode)
        {
            var cacheKey = GetCacheKey(userId);
            
            if (!_cache.TryGetValue<MfaCodeData>(cacheKey, out var mfaData))
            {
                _logger.LogWarning("MFA code not found or expired for user {UserId}", userId);
                return new MfaVerificationResult
                {
                    IsValid = false,
                    ErrorMessage = "رمز التحقق منتهي الصلاحية أو غير موجود. يرجى طلب رمز جديد."
                };
            }
            
            // Check max attempts
            if (mfaData.Attempts >= MAX_ATTEMPTS)
            {
                _cache.Remove(cacheKey);
                _logger.LogWarning("Max MFA attempts exceeded for user {UserId}", userId);
                return new MfaVerificationResult
                {
                    IsValid = false,
                    ErrorMessage = "تم تجاوز الحد الأقصى للمحاولات. يرجى طلب رمز جديد.",
                    MaxAttemptsExceeded = true
                };
            }
            
            // Verify code
            if (string.Equals(mfaData.Code, enteredCode?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                _cache.Remove(cacheKey);
                _logger.LogInformation("MFA code verified successfully for user {UserId}", userId);
                return new MfaVerificationResult
                {
                    IsValid = true
                };
            }
            
            // Increment attempts
            mfaData.Attempts++;
            _cache.Set(cacheKey, mfaData, TimeSpan.FromMinutes(CODE_EXPIRY_MINUTES));
            
            var remainingAttempts = MAX_ATTEMPTS - mfaData.Attempts;
            _logger.LogWarning("Invalid MFA code for user {UserId}. Remaining attempts: {Remaining}", userId, remainingAttempts);
            
            return new MfaVerificationResult
            {
                IsValid = false,
                RemainingAttempts = remainingAttempts,
                ErrorMessage = $"رمز التحقق غير صحيح. المحاولات المتبقية: {remainingAttempts}"
            };
        }

        /// <summary>
        /// Check if user has pending MFA verification
        /// </summary>
        public bool HasPendingMfa(string userId)
        {
            var cacheKey = GetCacheKey(userId);
            return _cache.TryGetValue<MfaCodeData>(cacheKey, out _);
        }

        /// <summary>
        /// Cancel pending MFA for user
        /// </summary>
        public void CancelMfa(string userId)
        {
            var cacheKey = GetCacheKey(userId);
            _cache.Remove(cacheKey);
            _logger.LogInformation("MFA cancelled for user {UserId}", userId);
        }

        /// <summary>
        /// Get remaining time for MFA code
        /// </summary>
        public TimeSpan? GetRemainingTime(string userId)
        {
            var cacheKey = GetCacheKey(userId);
            if (_cache.TryGetValue<MfaCodeData>(cacheKey, out var mfaData))
            {
                var expiresAt = mfaData.CreatedAt.AddMinutes(CODE_EXPIRY_MINUTES);
                var remaining = expiresAt - DateTime.UtcNow;
                return remaining > TimeSpan.Zero ? remaining : null;
            }
            return null;
        }

        private string GenerateSecureCode()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var number = BitConverter.ToUInt32(bytes, 0) % 1000000;
            return number.ToString("D6");
        }

        private string GetCacheKey(string userId) => $"{MFA_CODE_PREFIX}{userId}";

        private class MfaCodeData
        {
            public string Code { get; set; } = "";
            public string Email { get; set; } = "";
            public int Attempts { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }

    public class MfaVerificationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
        public int RemainingAttempts { get; set; } = 3;
        public bool MaxAttemptsExceeded { get; set; }
    }
}
