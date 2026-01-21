namespace GrcMvc.Services.Security;

/// <summary>
/// Configuration options for fraud detection.
/// Maps to "FraudDetection" section in appsettings.json.
/// </summary>
public class FraudDetectionOptions
{
    public const string SectionName = "FraudDetection";

    /// <summary>Whether fraud detection is enabled</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Maximum failed login attempts before lockout</summary>
    public int MaxFailedLoginAttempts { get; set; } = 5;

    /// <summary>Account lockout duration in minutes</summary>
    public int LockoutDurationMinutes { get; set; } = 30;

    /// <summary>Block registrations from disposable email providers</summary>
    public bool BlockDisposableEmails { get; set; } = true;

    /// <summary>Check IP reputation during registration</summary>
    public bool CheckIpReputation { get; set; } = true;

    /// <summary>Detect geographic location mismatches</summary>
    public bool GeoMismatchDetection { get; set; } = true;

    /// <summary>Maximum operations per hour before velocity abuse flagged</summary>
    public int VelocityAbuseThreshold { get; set; } = 3;

    /// <summary>Actions to take when tenant is quarantined</summary>
    public List<string> QuarantineActions { get; set; } = new()
    {
        "disable_integrations",
        "disable_export",
        "disable_api_access"
    };

    /// <summary>Verification methods required for suspicious accounts</summary>
    public List<string> RequireVerification { get; set; } = new()
    {
        "email",
        "manual_review"
    };

    /// <summary>Get lockout duration as TimeSpan</summary>
    public TimeSpan LockoutDuration => TimeSpan.FromMinutes(LockoutDurationMinutes);
}
