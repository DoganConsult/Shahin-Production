using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using GrcMvc.Services.Interfaces;
using System.Security.Cryptography;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// SMS-based MFA Service using Twilio
/// Sends verification codes via SMS
/// </summary>
public class SmsMfaService : ISmsMfaService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmsMfaService> _logger;
    private readonly IMemoryCache _cache;
    private readonly string? _accountSid;
    private readonly string? _authToken;
    private readonly string? _fromPhoneNumber;
    private readonly bool _isEnabled;

    private const string MFA_CODE_PREFIX = "sms_mfa_code_";
    private const int CODE_LENGTH = 6;
    private const int CODE_EXPIRY_MINUTES = 10;
    private const int MAX_ATTEMPTS = 3;

    public SmsMfaService(
        IConfiguration configuration,
        ILogger<SmsMfaService> logger,
        IMemoryCache cache)
    {
        _configuration = configuration;
        _logger = logger;
        _cache = cache;

        _accountSid = _configuration["Twilio:AccountSid"] ?? Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID");
        _authToken = _configuration["Twilio:AuthToken"] ?? Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN");
        _fromPhoneNumber = _configuration["Twilio:FromPhoneNumber"] ?? Environment.GetEnvironmentVariable("TWILIO_FROM_PHONE");

        _isEnabled = !string.IsNullOrEmpty(_accountSid) && 
                    !string.IsNullOrEmpty(_authToken) && 
                    !string.IsNullOrEmpty(_fromPhoneNumber);

        if (_isEnabled)
        {
            TwilioClient.Init(_accountSid, _authToken);
            _logger.LogInformation("SMS MFA service initialized with Twilio");
        }
        else
        {
            _logger.LogWarning("SMS MFA service disabled - Twilio credentials not configured");
        }
    }

    public bool IsEnabled => _isEnabled;

    /// <summary>
    /// Send MFA code via SMS
    /// </summary>
    public async Task<bool> SendMfaCodeAsync(string userId, string phoneNumber)
    {
        if (!_isEnabled)
        {
            _logger.LogWarning("SMS MFA attempted but service is not enabled");
            return false;
        }

        try
        {
            // Generate secure 6-digit code
            var code = GenerateSecureCode();

            // Store code in cache with expiry
            var cacheKey = GetCacheKey(userId);
            var mfaData = new SmsMfaCodeData
            {
                Code = code,
                PhoneNumber = phoneNumber,
                Attempts = 0,
                CreatedAt = DateTime.UtcNow
            };

            _cache.Set(cacheKey, mfaData, TimeSpan.FromMinutes(CODE_EXPIRY_MINUTES));

            // Send SMS via Twilio
            var message = await MessageResource.CreateAsync(
                body: $"Your Shahin GRC verification code is: {code}. Valid for {CODE_EXPIRY_MINUTES} minutes.",
                from: new PhoneNumber(_fromPhoneNumber),
                to: new PhoneNumber(phoneNumber)
            );

            _logger.LogInformation("SMS MFA code sent to {PhoneNumber} for user {UserId}, Message SID: {MessageSid}", 
                phoneNumber, userId, message.Sid);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS MFA code to {PhoneNumber}", phoneNumber);
            return false;
        }
    }

    /// <summary>
    /// Verify the SMS MFA code entered by user
    /// </summary>
    public async Task<SmsMfaVerificationResult> VerifyCodeAsync(string userId, string enteredCode)
    {
        var cacheKey = GetCacheKey(userId);

        if (!_cache.TryGetValue<SmsMfaCodeData>(cacheKey, out var mfaData))
        {
            _logger.LogWarning("SMS MFA code not found or expired for user {UserId}", userId);
            return new SmsMfaVerificationResult
            {
                Success = false,
                ErrorMessage = "Verification code expired or not found. Please request a new code."
            };
        }

        // Check max attempts
        if (mfaData.Attempts >= MAX_ATTEMPTS)
        {
            _cache.Remove(cacheKey);
            _logger.LogWarning("Max SMS MFA attempts exceeded for user {UserId}", userId);
            return new SmsMfaVerificationResult
            {
                Success = false,
                ErrorMessage = "Maximum attempts exceeded. Please request a new code.",
                MaxAttemptsExceeded = true
            };
        }

        // Verify code
        if (string.Equals(mfaData.Code, enteredCode?.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            _cache.Remove(cacheKey);
            _logger.LogInformation("SMS MFA code verified successfully for user {UserId}", userId);
            return new SmsMfaVerificationResult
            {
                Success = true
            };
        }

        // Increment attempts
        mfaData.Attempts++;
        _cache.Set(cacheKey, mfaData, TimeSpan.FromMinutes(CODE_EXPIRY_MINUTES));

        var remainingAttempts = MAX_ATTEMPTS - mfaData.Attempts;
        _logger.LogWarning("Invalid SMS MFA code for user {UserId}. Remaining attempts: {Remaining}", userId, remainingAttempts);

        return new SmsMfaVerificationResult
        {
            Success = false,
            AttemptsRemaining = remainingAttempts,
            ErrorMessage = $"Invalid verification code. Remaining attempts: {remainingAttempts}"
        };
    }

    /// <summary>
    /// Check if user has pending SMS MFA verification
    /// </summary>
    public bool HasPendingMfa(string userId)
    {
        var cacheKey = GetCacheKey(userId);
        return _cache.TryGetValue<SmsMfaCodeData>(cacheKey, out _);
    }

    /// <summary>
    /// Cancel pending SMS MFA for user
    /// </summary>
    public void CancelMfa(string userId)
    {
        var cacheKey = GetCacheKey(userId);
        _cache.Remove(cacheKey);
        _logger.LogInformation("SMS MFA cancelled for user {UserId}", userId);
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

    private class SmsMfaCodeData
    {
        public string Code { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public int Attempts { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

/// <summary>
/// SMS MFA Service Interface
/// </summary>
public interface ISmsMfaService
{
    bool IsEnabled { get; }
    Task<bool> SendMfaCodeAsync(string userId, string phoneNumber);
    Task<SmsMfaVerificationResult> VerifyCodeAsync(string userId, string enteredCode);
    bool HasPendingMfa(string userId);
    void CancelMfa(string userId);
}

/// <summary>
/// SMS MFA Verification Result
/// </summary>
public class SmsMfaVerificationResult
{
    public bool Success { get; set; }
    public int AttemptsRemaining { get; set; }
    public bool MaxAttemptsExceeded { get; set; }
    public string? ErrorMessage { get; set; }
}
