namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Dispatcher for user notifications across multiple channels
/// Supports: Email, In-App, Push, SMS, Webhook
/// </summary>
public interface IUserNotificationDispatcher
{
    /// <summary>
    /// Dispatch notification to all configured channels for a user
    /// </summary>
    Task DispatchToUserAsync(string userId, UserNotification notification);

    /// <summary>
    /// Dispatch notification to multiple users
    /// </summary>
    Task DispatchToUsersAsync(IEnumerable<string> userIds, UserNotification notification);

    /// <summary>
    /// Dispatch notification to a role (all users with that role)
    /// </summary>
    Task DispatchToRoleAsync(string role, UserNotification notification, Guid? tenantId = null);

    /// <summary>
    /// Dispatch notification to a tenant (all users in tenant)
    /// </summary>
    Task DispatchToTenantAsync(Guid tenantId, UserNotification notification);

    /// <summary>
    /// Dispatch a general notification (goes to configured default channels)
    /// </summary>
    Task DispatchAsync(UserNotification notification);

    /// <summary>
    /// Schedule a notification for future delivery
    /// </summary>
    Task ScheduleAsync(UserNotification notification, DateTime scheduledAt);

    /// <summary>
    /// Get user notification preferences
    /// </summary>
    Task<NotificationPreferences> GetUserPreferencesAsync(string userId);

    /// <summary>
    /// Update user notification preferences
    /// </summary>
    Task UpdateUserPreferencesAsync(string userId, NotificationPreferences preferences);
}

public class UserNotification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Type { get; set; } = "General";
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public string? Priority { get; set; } = "Normal"; // Low, Normal, High, Urgent
    public string? Category { get; set; }
    public string? ActionUrl { get; set; }
    public string? ActionLabel { get; set; }
    public Dictionary<string, object>? Data { get; set; }
    public List<string>? Channels { get; set; } // Email, InApp, Push, SMS, Webhook
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
}

public class NotificationPreferences
{
    public bool EmailEnabled { get; set; } = true;
    public bool InAppEnabled { get; set; } = true;
    public bool PushEnabled { get; set; } = true;
    public bool SmsEnabled { get; set; } = false;
    public bool DigestEnabled { get; set; } = false;
    public string DigestFrequency { get; set; } = "Daily"; // Immediate, Daily, Weekly
    public List<string> MutedCategories { get; set; } = new();
    public Dictionary<string, string> ChannelOverrides { get; set; } = new(); // Category -> Channel
    public string? PreferredEmail { get; set; }
    public string? PreferredPhone { get; set; }
    public string QuietHoursStart { get; set; } = "22:00";
    public string QuietHoursEnd { get; set; } = "08:00";
    public bool QuietHoursEnabled { get; set; } = false;
}
