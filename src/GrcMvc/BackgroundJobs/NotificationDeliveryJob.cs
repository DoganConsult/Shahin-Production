using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Models.Workflows;
using GrcMvc.Services.Interfaces;
using GrcMvc.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GrcMvc.BackgroundJobs
{
    /// <summary>
    /// Background job for delivering notifications via email/SMS
    /// Runs every 5 minutes to process queued notifications
    /// </summary>
    public class NotificationDeliveryJob
    {
        private readonly GrcDbContext _context;
        private readonly ISmtpEmailService _emailService;
        private readonly ILogger<NotificationDeliveryJob> _logger;
        private readonly WorkflowSettings _settings;

        public NotificationDeliveryJob(
            GrcDbContext context,
            ISmtpEmailService emailService,
            ILogger<NotificationDeliveryJob> logger,
            IOptions<WorkflowSettings> settings)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
            _settings = settings.Value;
        }

        /// <summary>
        /// Main job execution method - called by Hangfire scheduler
        /// </summary>
        [Hangfire.AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 30, 120, 300 })]
        public async Task ExecuteAsync()
        {
            _logger.LogInformation("NotificationDeliveryJob started at {Time}", DateTime.UtcNow);

            try
            {
                var stats = new DeliveryStats();

                // Get pending notifications that require email delivery
                var pendingNotifications = await GetPendingNotificationsAsync();

                _logger.LogInformation("Found {Count} pending notifications to deliver", pendingNotifications.Count);

                foreach (var notification in pendingNotifications)
                {
                    var result = await DeliverNotificationAsync(notification);

                    if (result)
                    {
                        stats.Successful++;
                    }
                    else
                    {
                        stats.Failed++;
                    }
                }

                _logger.LogInformation(
                    "NotificationDeliveryJob completed. Delivered: {Success}, Failed: {Failed}",
                    stats.Successful, stats.Failed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "NotificationDeliveryJob failed with error: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Get all pending notifications that need delivery
        /// </summary>
        private async Task<List<WorkflowNotification>> GetPendingNotificationsAsync()
        {
            var maxRetries = _settings.MaxRetryAttempts;

            return await _context.WorkflowNotifications
                .Where(n => n.RequiresEmail)
                .Where(n => !n.IsDelivered)
                .Where(n => n.DeliveryAttempts < maxRetries)
                .OrderByDescending(n => n.Priority == "Critical")
                .ThenByDescending(n => n.Priority == "High")
                .ThenBy(n => n.CreatedAt)
                .Take(100) // Process in batches
                .ToListAsync();
        }

        /// <summary>
        /// Deliver a single notification
        /// </summary>
        private async Task<bool> DeliverNotificationAsync(WorkflowNotification notification)
        {
            try
            {
                // Get recipient email
                var recipientEmail = await GetRecipientEmailAsync(notification.RecipientUserId);

                if (string.IsNullOrEmpty(recipientEmail))
                {
                    _logger.LogWarning(
                        "No email found for recipient {UserId}, notification {NotificationId}",
                        notification.RecipientUserId, notification.Id);

                    notification.DeliveryError = "Recipient email not found";
                    notification.DeliveryAttempts++;
                    await _context.SaveChangesAsync();
                    return false;
                }

                // Check user preferences
                var preferences = await GetUserPreferencesAsync(notification.RecipientUserId, notification.TenantId);

                if (preferences != null && !preferences.EmailEnabled)
                {
                    _logger.LogDebug(
                        "Email disabled for user {UserId}, skipping notification {NotificationId}",
                        notification.RecipientUserId, notification.Id);

                    notification.IsDelivered = true;
                    notification.DeliveredAt = DateTime.UtcNow;
                    notification.DeliveryNote = "Skipped - user has email notifications disabled";
                    await _context.SaveChangesAsync();
                    return true;
                }

                // Attempt email delivery
                if (notification.RequiresEmail)
                {
                    var emailSent = await SendEmailAsync(notification, recipientEmail);

                    if (!emailSent)
                    {
                        await HandleDeliveryFailureAsync(notification);
                        return false;
                    }
                }

                // Mark as delivered
                notification.IsDelivered = true;
                notification.DeliveredAt = DateTime.UtcNow;
                notification.DeliveryAttempts++;

                await _context.SaveChangesAsync();

                _logger.LogDebug(
                    "Notification {NotificationId} delivered to {Email}",
                    notification.Id, recipientEmail);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error delivering notification {NotificationId}: {Message}",
                    notification.Id, ex.Message);

                await HandleDeliveryFailureAsync(notification, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Send email for notification
        /// </summary>
        private async Task<bool> SendEmailAsync(WorkflowNotification notification, string recipientEmail)
        {
            try
            {
                // Determine template based on notification type
                var templateName = GetTemplateForNotificationType(notification.NotificationType);

                // Build template data
                var templateData = new Dictionary<string, object>
                {
                    { "Subject", notification.Subject },
                    { "Body", notification.Body },
                    { "Priority", notification.Priority },
                    { "NotificationType", notification.NotificationType },
                    { "WorkflowId", notification.WorkflowInstanceId },
                    { "CreatedAt", notification.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss UTC") },
                    { "RecipientName", notification.Recipient ?? "User" }
                };

                // Add workflow-specific data if available
                if (notification.WorkflowInstance != null)
                {
                    templateData["WorkflowType"] = notification.WorkflowInstance.WorkflowType;
                    templateData["WorkflowStatus"] = notification.WorkflowInstance.Status;
                }

                // Send templated email
                await _emailService.SendTemplatedEmailAsync(
                    recipientEmail,
                    notification.Subject,
                    templateName,
                    templateData);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}: {Message}", recipientEmail, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Get template name for notification type
        /// </summary>
        private string GetTemplateForNotificationType(string notificationType)
        {
            return notificationType switch
            {
                "TaskAssigned" => "TaskAssigned",
                "ApprovalRequired" => "ApprovalRequired",
                "WorkflowCompleted" => "WorkflowCompleted",
                "Escalation" => "EscalationAlert",
                "SLA_Breach" => "SlaBreachWarning",
                "ReminderDue" => "TaskAssigned",
                "StatusChange" => "WorkflowCompleted",
                _ => "TaskAssigned" // Default template
            };
        }

        /// <summary>
        /// Handle delivery failure with retry logic
        /// </summary>
        private async Task HandleDeliveryFailureAsync(WorkflowNotification notification, string? errorMessage = null)
        {
            notification.DeliveryAttempts++;
            notification.DeliveryError = errorMessage ?? "Delivery failed";
            notification.LastAttemptAt = DateTime.UtcNow;

            // Calculate next retry with exponential backoff
            if (notification.DeliveryAttempts < _settings.MaxRetryAttempts)
            {
                var delayMinutes = Math.Pow(2, notification.DeliveryAttempts) * 5; // 5, 10, 20 minutes
                var nextRetryAt = DateTime.UtcNow.AddMinutes(delayMinutes);

                _logger.LogWarning(
                    "Notification {NotificationId} delivery failed (attempt {Attempt}/{Max}). Retry at {RetryTime}",
                    notification.Id, notification.DeliveryAttempts, _settings.MaxRetryAttempts, nextRetryAt);
            }
            else
            {
                _logger.LogError(
                    "Notification {NotificationId} delivery permanently failed after {Attempts} attempts",
                    notification.Id, notification.DeliveryAttempts);
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Get recipient's email address
        /// </summary>
        private async Task<string?> GetRecipientEmailAsync(string userId)
        {
            var user = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => u.Email)
                .FirstOrDefaultAsync();

            return user;
        }

        /// <summary>
        /// Get user notification preferences
        /// </summary>
        private async Task<UserNotificationPreference?> GetUserPreferencesAsync(string userId, Guid tenantId)
        {
            // TODO: Implement when UserNotificationPreferences DbSet is added
            return await Task.FromResult<UserNotificationPreference?>(null);
        }

        private class DeliveryStats
        {
            public int Successful { get; set; }
            public int Failed { get; set; }
        }
    }

    /// <summary>
    /// Workflow settings configuration
    /// </summary>
    public class WorkflowSettings
    {
        public bool EnableBackgroundJobs { get; set; } = true;
        public int EscalationIntervalHours { get; set; } = 1;
        public int NotificationDeliveryIntervalMinutes { get; set; } = 5;
        public int SlaMonitorIntervalMinutes { get; set; } = 30;
        public int MaxRetryAttempts { get; set; } = 3;
        public int CacheExpiryMinutes { get; set; } = 5;
    }
}
