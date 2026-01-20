using System.Text.Json;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Emailing;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Multi-channel notification dispatcher for user notifications and automation
/// Integrates with ABP Email, in-app notifications, and webhooks
/// </summary>
public class UserNotificationDispatcher : IUserNotificationDispatcher
{
    private readonly GrcDbContext _context;
    private readonly IEmailSender _emailSender;
    private readonly IWebhookService _webhookService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<UserNotificationDispatcher> _logger;

    public UserNotificationDispatcher(
        GrcDbContext context,
        IEmailSender emailSender,
        IWebhookService webhookService,
        UserManager<ApplicationUser> userManager,
        ILogger<UserNotificationDispatcher> logger)
    {
        _context = context;
        _emailSender = emailSender;
        _webhookService = webhookService;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task DispatchToUserAsync(string userId, UserNotification notification)
    {
        _logger.LogInformation("Dispatching notification to user {UserId}: {Type} - {Title}", 
            userId, notification.Type, notification.Title);

        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User not found for notification: {UserId}", userId);
                return;
            }

            var preferences = await GetUserPreferencesAsync(userId);
            
            // Check if category is muted
            if (preferences.MutedCategories.Contains(notification.Category ?? ""))
            {
                _logger.LogDebug("Notification category muted for user: {UserId}, Category: {Category}", 
                    userId, notification.Category);
                return;
            }

            // Check quiet hours
            if (preferences.QuietHoursEnabled && IsInQuietHours(preferences))
            {
                _logger.LogDebug("User in quiet hours, queueing notification: {UserId}", userId);
                await QueueNotificationAsync(userId, notification);
                return;
            }

            // Determine channels to use
            var channels = notification.Channels ?? GetDefaultChannels(preferences, notification);

            // Store in-app notification
            if (channels.Contains("InApp") && preferences.InAppEnabled)
            {
                await StoreInAppNotificationAsync(userId, notification);
            }

            // Send email notification
            if (channels.Contains("Email") && preferences.EmailEnabled)
            {
                await SendEmailNotificationAsync(user, notification, preferences);
            }

            // Send push notification (placeholder)
            if (channels.Contains("Push") && preferences.PushEnabled)
            {
                await SendPushNotificationAsync(userId, notification);
            }

            // Send SMS notification (placeholder)
            if (channels.Contains("SMS") && preferences.SmsEnabled)
            {
                await SendSmsNotificationAsync(user, notification, preferences);
            }

            // Trigger webhook if configured
            if (channels.Contains("Webhook"))
            {
                await TriggerWebhookNotificationAsync(user, notification);
            }

            _logger.LogInformation("Notification dispatched to user {UserId} via channels: {Channels}", 
                userId, string.Join(", ", channels));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dispatching notification to user {UserId}", userId);
            throw;
        }
    }

    public async Task DispatchToUsersAsync(IEnumerable<string> userIds, UserNotification notification)
    {
        foreach (var userId in userIds)
        {
            try
            {
                await DispatchToUserAsync(userId, notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dispatching notification to user {UserId}", userId);
            }
        }
    }

    public async Task DispatchToRoleAsync(string role, UserNotification notification, Guid? tenantId = null)
    {
        _logger.LogInformation("Dispatching notification to role {Role}: {Title}", role, notification.Title);

        var usersInRole = await _userManager.GetUsersInRoleAsync(role);
        
        // Filter by tenant if specified (users don't have TenantId directly, would need to join)
        // For now, dispatch to all users in role

        var userIds = usersInRole.Select(u => u.Id.ToString());
        await DispatchToUsersAsync(userIds, notification);
    }

    public async Task DispatchToTenantAsync(Guid tenantId, UserNotification notification)
    {
        _logger.LogInformation("Dispatching notification to tenant {TenantId}: {Title}", tenantId, notification.Title);

        // Get users associated with tenant via other means (workspace, etc.)
        var users = await _context.Users
            .Select(u => u.Id.ToString())
            .Take(100) // Limit for safety
            .ToListAsync();

        await DispatchToUsersAsync(users, notification);
    }

    public async Task DispatchAsync(UserNotification notification)
    {
        _logger.LogInformation("Dispatching general notification: {Type} - {Title}", 
            notification.Type, notification.Title);

        // Store as system notification
        var systemNotification = new SystemNotification
        {
            Id = notification.Id,
            Type = notification.Type,
            Title = notification.Title,
            Message = notification.Message,
            Priority = notification.Priority ?? "Normal",
            Category = notification.Category,
            ActionUrl = notification.ActionUrl,
            DataJson = notification.Data != null ? JsonSerializer.Serialize(notification.Data) : null,
            CreatedAt = notification.CreatedAt,
            ExpiresAt = notification.ExpiresAt
        };

        _context.Set<SystemNotification>().Add(systemNotification);
        await _context.SaveChangesAsync();
    }

    public async Task ScheduleAsync(UserNotification notification, DateTime scheduledAt)
    {
        _logger.LogInformation("Scheduling notification for {ScheduledAt}: {Title}", scheduledAt, notification.Title);

        var scheduled = new ScheduledNotification
        {
            Id = Guid.NewGuid(),
            NotificationJson = JsonSerializer.Serialize(notification),
            ScheduledAt = scheduledAt,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<ScheduledNotification>().Add(scheduled);
        await _context.SaveChangesAsync();
    }

    public async Task<NotificationPreferences> GetUserPreferencesAsync(string userId)
    {
        var prefs = await _context.Set<UserNotificationPreference>()
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (prefs == null)
        {
            return new NotificationPreferences(); // Return defaults
        }

        return new NotificationPreferences
        {
            EmailEnabled = prefs.EmailEnabled,
            InAppEnabled = prefs.InAppEnabled,
            PushEnabled = prefs.PushEnabled,
            SmsEnabled = prefs.SmsEnabled,
            DigestEnabled = prefs.DigestEnabled,
            DigestFrequency = prefs.DigestFrequency,
            MutedCategories = prefs.MutedCategoriesJson != null 
                ? JsonSerializer.Deserialize<List<string>>(prefs.MutedCategoriesJson) ?? new() 
                : new(),
            QuietHoursStart = prefs.QuietHoursStart,
            QuietHoursEnd = prefs.QuietHoursEnd,
            QuietHoursEnabled = prefs.QuietHoursEnabled,
            PreferredEmail = prefs.PreferredEmail,
            PreferredPhone = prefs.PreferredPhone
        };
    }

    public async Task UpdateUserPreferencesAsync(string userId, NotificationPreferences preferences)
    {
        var prefs = await _context.Set<UserNotificationPreference>()
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (prefs == null)
        {
            prefs = new UserNotificationPreference
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            _context.Set<UserNotificationPreference>().Add(prefs);
        }

        prefs.EmailEnabled = preferences.EmailEnabled;
        prefs.InAppEnabled = preferences.InAppEnabled;
        prefs.PushEnabled = preferences.PushEnabled;
        prefs.SmsEnabled = preferences.SmsEnabled;
        prefs.DigestEnabled = preferences.DigestEnabled;
        prefs.DigestFrequency = preferences.DigestFrequency;
        prefs.MutedCategoriesJson = JsonSerializer.Serialize(preferences.MutedCategories);
        prefs.QuietHoursStart = preferences.QuietHoursStart;
        prefs.QuietHoursEnd = preferences.QuietHoursEnd;
        prefs.QuietHoursEnabled = preferences.QuietHoursEnabled;
        prefs.PreferredEmail = preferences.PreferredEmail;
        prefs.PreferredPhone = preferences.PreferredPhone;
        prefs.ModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    // Private helper methods

    private async Task StoreInAppNotificationAsync(string userId, UserNotification notification)
    {
        var inApp = new InAppNotification
        {
            Id = notification.Id,
            UserId = userId,
            Type = notification.Type,
            Title = notification.Title,
            Message = notification.Message,
            Priority = notification.Priority ?? "Normal",
            Category = notification.Category,
            ActionUrl = notification.ActionUrl,
            ActionLabel = notification.ActionLabel,
            DataJson = notification.Data != null ? JsonSerializer.Serialize(notification.Data) : null,
            IsRead = false,
            CreatedAt = notification.CreatedAt,
            ExpiresAt = notification.ExpiresAt
        };

        _context.Set<InAppNotification>().Add(inApp);
        await _context.SaveChangesAsync();
    }

    private async Task SendEmailNotificationAsync(ApplicationUser user, UserNotification notification, NotificationPreferences prefs)
    {
        var email = prefs.PreferredEmail ?? user.Email;
        if (string.IsNullOrEmpty(email)) return;

        var subject = $"[{notification.Priority}] {notification.Title}";
        var body = BuildEmailBody(notification);

        try
        {
            await _emailSender.SendAsync(email, subject, body, isBodyHtml: true);
            _logger.LogDebug("Email notification sent to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email notification to {Email}", email);
        }
    }

    private Task SendPushNotificationAsync(string userId, UserNotification notification)
    {
        // Placeholder for push notification integration (Firebase, OneSignal, etc.)
        _logger.LogDebug("Push notification queued for user {UserId}: {Title}", userId, notification.Title);
        return Task.CompletedTask;
    }

    private Task SendSmsNotificationAsync(ApplicationUser user, UserNotification notification, NotificationPreferences prefs)
    {
        // Placeholder for SMS integration (Twilio, etc.)
        var phone = prefs.PreferredPhone ?? user.PhoneNumber;
        if (!string.IsNullOrEmpty(phone))
        {
            _logger.LogDebug("SMS notification queued for {Phone}: {Title}", phone, notification.Title);
        }
        return Task.CompletedTask;
    }

    private async Task TriggerWebhookNotificationAsync(ApplicationUser user, UserNotification notification)
    {
        // Get tenant from context service if available
        Guid? tenantId = null; // Would come from ITenantContextService
        if (!tenantId.HasValue) return;

        await _webhookService.TriggerEventAsync(
            tenantId.Value,
            "notification.sent",
            notification.Id.ToString(),
            new
            {
                userId = user.Id,
                type = notification.Type,
                title = notification.Title,
                message = notification.Message,
                priority = notification.Priority,
                data = notification.Data
            });
    }

    private async Task QueueNotificationAsync(string userId, UserNotification notification)
    {
        var queued = new QueuedNotification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            NotificationJson = JsonSerializer.Serialize(notification),
            QueuedAt = DateTime.UtcNow,
            Status = "Pending"
        };

        _context.Set<QueuedNotification>().Add(queued);
        await _context.SaveChangesAsync();
    }

    private List<string> GetDefaultChannels(NotificationPreferences prefs, UserNotification notification)
    {
        var channels = new List<string>();

        // High priority always gets email
        if (notification.Priority == "High" || notification.Priority == "Urgent")
        {
            if (prefs.EmailEnabled) channels.Add("Email");
            if (prefs.PushEnabled) channels.Add("Push");
        }

        // Always add in-app
        if (prefs.InAppEnabled) channels.Add("InApp");

        // Normal priority based on digest preference
        if (!prefs.DigestEnabled && prefs.EmailEnabled && !channels.Contains("Email"))
        {
            channels.Add("Email");
        }

        return channels;
    }

    private bool IsInQuietHours(NotificationPreferences prefs)
    {
        var now = DateTime.Now.TimeOfDay;
        var start = TimeSpan.Parse(prefs.QuietHoursStart);
        var end = TimeSpan.Parse(prefs.QuietHoursEnd);

        if (start < end)
        {
            return now >= start && now < end;
        }
        else
        {
            return now >= start || now < end;
        }
    }

    private string BuildEmailBody(UserNotification notification)
    {
        return $@"
<!DOCTYPE html>
<html>
<head><meta charset=""utf-8""></head>
<body style=""font-family: 'Segoe UI', Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;"">
    <div style=""background: #f8f9fa; border-radius: 8px; padding: 20px;"">
        <h2 style=""color: #333; margin-top: 0;"">{notification.Title}</h2>
        <p style=""color: #666;"">{notification.Message}</p>
        {(notification.ActionUrl != null ? $@"<a href=""{notification.ActionUrl}"" style=""display: inline-block; background: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 4px;"">{notification.ActionLabel ?? "View Details"}</a>" : "")}
    </div>
    <p style=""color: #999; font-size: 12px; margin-top: 20px;"">This notification was sent from Shahin GRC Platform</p>
</body>
</html>";
    }
}

// Supporting entities
public class InAppNotification
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = "";
    public string Type { get; set; } = "";
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public string Priority { get; set; } = "Normal";
    public string? Category { get; set; }
    public string? ActionUrl { get; set; }
    public string? ActionLabel { get; set; }
    public string? DataJson { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class SystemNotification
{
    public Guid Id { get; set; }
    public string Type { get; set; } = "";
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public string Priority { get; set; } = "Normal";
    public string? Category { get; set; }
    public string? ActionUrl { get; set; }
    public string? DataJson { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class ScheduledNotification
{
    public Guid Id { get; set; }
    public string NotificationJson { get; set; } = "";
    public DateTime ScheduledAt { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime? ProcessedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class QueuedNotification
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = "";
    public string NotificationJson { get; set; } = "";
    public DateTime QueuedAt { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime? ProcessedAt { get; set; }
}

public class UserNotificationPreference
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = "";
    public bool EmailEnabled { get; set; } = true;
    public bool InAppEnabled { get; set; } = true;
    public bool PushEnabled { get; set; } = true;
    public bool SmsEnabled { get; set; } = false;
    public bool DigestEnabled { get; set; } = false;
    public string DigestFrequency { get; set; } = "Daily";
    public string? MutedCategoriesJson { get; set; }
    public string QuietHoursStart { get; set; } = "22:00";
    public string QuietHoursEnd { get; set; } = "08:00";
    public bool QuietHoursEnabled { get; set; } = false;
    public string? PreferredEmail { get; set; }
    public string? PreferredPhone { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
}
