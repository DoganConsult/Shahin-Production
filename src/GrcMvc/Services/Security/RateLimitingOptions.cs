namespace GrcMvc.Services.Security;

/// <summary>
/// Configuration options for rate limiting.
/// Maps to "RateLimiting" section in appsettings.json.
/// </summary>
public class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    /// <summary>Whether rate limiting is enabled</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Rate limit for trial registration endpoint</summary>
    public RateLimitPolicy TrialRegistration { get; set; } = new()
    {
        PermitLimit = 5,
        WindowMinutes = 10,
        QueueLimit = 2
    };

    /// <summary>Rate limit for general API requests</summary>
    public RateLimitPolicy ApiRequests { get; set; } = new()
    {
        PermitLimit = 100,
        WindowMinutes = 1,
        QueueLimit = 10
    };

    /// <summary>Rate limit for login attempts</summary>
    public RateLimitPolicy Login { get; set; } = new()
    {
        PermitLimit = 10,
        WindowMinutes = 5,
        QueueLimit = 5
    };
}

/// <summary>
/// Individual rate limit policy settings.
/// </summary>
public class RateLimitPolicy
{
    /// <summary>Maximum number of requests allowed in the window</summary>
    public int PermitLimit { get; set; }

    /// <summary>Time window in minutes</summary>
    public int WindowMinutes { get; set; }

    /// <summary>Maximum queue size when limit is reached</summary>
    public int QueueLimit { get; set; }

    /// <summary>Get the window as TimeSpan</summary>
    public TimeSpan Window => TimeSpan.FromMinutes(WindowMinutes);
}
