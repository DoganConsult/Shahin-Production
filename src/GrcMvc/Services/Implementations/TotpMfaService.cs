using OtpNet;
using Microsoft.Extensions.Logging;
using GrcMvc.Services.Interfaces;
using System.Text;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// TOTP (Time-based One-Time Password) Service for Authenticator Apps
/// Supports Google Authenticator, Microsoft Authenticator, etc.
/// </summary>
public class TotpMfaService : ITotpMfaService
{
    private readonly ILogger<TotpMfaService> _logger;
    private const int StepSeconds = 30; // Standard TOTP step size

    public TotpMfaService(ILogger<TotpMfaService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Generate a secret key for a user (to be stored securely)
    /// </summary>
    public string GenerateSecretKey()
    {
        var secretKey = KeyGeneration.GenerateRandomKey(20); // 160 bits
        return Base32Encoding.ToString(secretKey);
    }

    /// <summary>
    /// Generate QR code URI for authenticator app setup
    /// </summary>
    public string GenerateQrCodeUri(string email, string secretKey, string issuer = "Shahin GRC")
    {
        // Format: otpauth://totp/{issuer}:{email}?secret={secret}&issuer={issuer}
        var encodedIssuer = Uri.EscapeDataString(issuer);
        var encodedEmail = Uri.EscapeDataString(email);
        
        return $"otpauth://totp/{encodedIssuer}:{encodedEmail}?secret={secretKey}&issuer={encodedIssuer}&algorithm=SHA1&digits=6&period={StepSeconds}";
    }

    /// <summary>
    /// Verify TOTP code entered by user
    /// </summary>
    public bool VerifyCode(string secretKey, string code, int timeStepTolerance = 1)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(secretKey) || string.IsNullOrWhiteSpace(code))
            {
                return false;
            }

            // Decode base32 secret key
            var secretBytes = Base32Encoding.ToBytes(secretKey);
            var totp = new Totp(secretBytes, step: StepSeconds);

            // Verify code with time step tolerance (allows codes from previous/next time windows)
            var isValid = totp.VerifyTotp(code, out _, new VerificationWindow(previous: timeStepTolerance, future: timeStepTolerance));

            if (isValid)
            {
                _logger.LogDebug("TOTP code verified successfully");
            }
            else
            {
                _logger.LogWarning("TOTP code verification failed");
            }

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying TOTP code");
            return false;
        }
    }

    /// <summary>
    /// Get current TOTP code (for testing/debugging)
    /// </summary>
    public string GetCurrentCode(string secretKey)
    {
        try
        {
            var secretBytes = Base32Encoding.ToBytes(secretKey);
            var totp = new Totp(secretBytes, step: StepSeconds);
            return totp.ComputeTotp();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating current TOTP code");
            return string.Empty;
        }
    }

    /// <summary>
    /// Get remaining seconds until code expires
    /// </summary>
    public int GetRemainingSeconds()
    {
        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var step = currentTime / StepSeconds;
        var nextStep = (step + 1) * StepSeconds;
        return (int)(nextStep - currentTime);
    }
}

/// <summary>
/// TOTP MFA Service Interface
/// </summary>
public interface ITotpMfaService
{
    string GenerateSecretKey();
    string GenerateQrCodeUri(string email, string secretKey, string issuer = "Shahin GRC");
    bool VerifyCode(string secretKey, string code, int timeStepTolerance = 1);
    string GetCurrentCode(string secretKey);
    int GetRemainingSeconds();
}
