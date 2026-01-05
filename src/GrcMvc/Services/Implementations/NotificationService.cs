using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Models.Workflows;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Service for managing and delivering notifications
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly GrcDbContext _context;
        private readonly ISmtpEmailService _emailService;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            GrcDbContext context,
            ISmtpEmailService emailService,
            ILogger<NotificationService> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        /// <summary>
        /// Create a notification record in the database
        /// </summary>
        public async Task<WorkflowNotification> CreateNotificationAsync(
            Guid workflowInstanceId,
            string recipientUserId,
            string notificationType,
            string subject,
            string body,
            string priority = "Normal",
            Guid tenantId = default,
            bool requiresEmail = true,
            bool requiresSms = false)
        {
            var notification = new WorkflowNotification
            {
                WorkflowInstanceId = workflowInstanceId,
                TenantId = tenantId,
                RecipientUserId = recipientUserId,
                Recipient = recipientUserId,
                NotificationType = notificationType,
                Subject = subject,
                Message = subject,
                Body = body,
                Priority = priority,
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                RequiresEmail = requiresEmail
            };

            _context.WorkflowNotifications.Add(notification);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Notification created: {NotificationId} for user {UserId}",
                notification.Id, recipientUserId);

            return notification;
        }

        /// <summary>
        /// Create and immediately send a notification
        /// </summary>
        public async Task<NotificationResult> SendNotificationAsync(
            Guid workflowInstanceId,
            string recipientUserId,
            string notificationType,
            string subject,
            string body,
            string priority = "Normal",
            Guid tenantId = default,
            Dictionary<string, object>? templateData = null)
        {
            var result = new NotificationResult();

            try
            {
                // Create the notification record
                var notification = await CreateNotificationAsync(
                    workflowInstanceId,
                    recipientUserId,
                    notificationType,
                    subject,
                    body,
                    priority,
                    tenantId);

                result.NotificationId = notification.Id;

                // Check user preferences
                var preferences = await GetUserPreferencesAsync(recipientUserId, tenantId);

                if (preferences?.EmailEnabled == false)
                {
                    result.IsSuccess = true;
                    result.Message = "Notification created but email disabled by user preference";
                    notification.DeliveryNote = "Email disabled by user preference";
                    await _context.SaveChangesAsync();
                    return result;
                }

                // Get recipient email
                var email = await GetUserEmailAsync(recipientUserId);

                if (string.IsNullOrEmpty(email))
                {
                    result.IsSuccess = false;
                    result.Message = "Recipient email not found";
                    notification.DeliveryError = "Email not found";
                    await _context.SaveChangesAsync();
                    return result;
                }

                // Send email
                var templateName = GetTemplateForType(notificationType);
                var emailData = templateData ?? new Dictionary<string, object>
                {
                    { "Subject", subject },
                    { "Body", body },
                    { "Priority", priority },
                    { "NotificationType", notificationType }
                };

                await _emailService.SendTemplatedEmailAsync(email, subject, templateName, emailData);

                // Update notification as delivered
                notification.IsDelivered = true;
                notification.DeliveredAt = DateTime.UtcNow;
                notification.DeliveryAttempts = 1;
                await _context.SaveChangesAsync();

                result.IsSuccess = true;
                result.Message = "Notification sent successfully";

                _logger.LogInformation(
                    "Notification {NotificationId} sent to {Email}",
                    notification.Id, email);
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = ex.Message;

                _logger.LogError(ex,
                    "Failed to send notification to user {UserId}: {Message}",
                    recipientUserId, ex.Message);
            }

            return result;
        }

        /// <summary>
        /// Get pending notifications that need delivery
        /// </summary>
        public async Task<List<WorkflowNotification>> GetPendingNotificationsAsync(Guid? tenantId = null)
        {
            var query = _context.WorkflowNotifications
                .Where(n => !n.IsRead)
                .Where(n => n.RequiresEmail);

            if (tenantId.HasValue && tenantId.Value != Guid.Empty)
            {
                query = query.Where(n => n.TenantId == tenantId.Value);
            }

            return await query
                .OrderBy(n => n.CreatedAt)
                .Take(100)
                .ToListAsync();
        }

        /// <summary>
        /// Get notifications for a specific user
        /// </summary>
        public async Task<List<WorkflowNotification>> GetUserNotificationsAsync(
            string userId,
            Guid tenantId,
            bool unreadOnly = false,
            int page = 1,
            int pageSize = 20)
        {
            var query = _context.WorkflowNotifications
                .AsNoTracking()
                .Where(n => n.RecipientUserId == userId)
                .Where(n => n.TenantId == tenantId);

            if (unreadOnly)
            {
                query = query.Where(n => !n.IsRead);
            }

            return await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// Get unread notification count for a user
        /// </summary>
        public async Task<int> GetUnreadCountAsync(string userId, Guid tenantId)
        {
            return await _context.WorkflowNotifications
                .AsNoTracking()
                .Where(n => n.RecipientUserId == userId)
                .Where(n => n.TenantId == tenantId)
                .Where(n => !n.IsRead)
                .CountAsync();
        }

        /// <summary>
        /// Mark a notification as read
        /// </summary>
        public async Task MarkAsReadAsync(Guid notificationId, string userId)
        {
            var notification = await _context.WorkflowNotifications
                .Where(n => n.Id == notificationId)
                .Where(n => n.RecipientUserId == userId)
                .FirstOrDefaultAsync();

            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Mark all notifications as read for a user
        /// </summary>
        public async Task MarkAllAsReadAsync(string userId, Guid tenantId)
        {
            var notifications = await _context.WorkflowNotifications
                .Where(n => n.RecipientUserId == userId)
                .Where(n => n.TenantId == tenantId)
                .Where(n => !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Mark notification as delivered
        /// </summary>
        public async Task MarkAsDeliveredAsync(Guid notificationId)
        {
            var notification = await _context.WorkflowNotifications
                .FindAsync(notificationId);

            if (notification != null)
            {
                notification.IsDelivered = true;
                notification.DeliveredAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Update notification preferences for a user
        /// </summary>
        public async Task UpdatePreferencesAsync(
            string userId,
            Guid tenantId,
            bool emailEnabled = true,
            bool smsEnabled = false,
            List<string>? enabledNotificationTypes = null)
        {
            // TODO: Implement when UserNotificationPreferences DbSet is added
            await Task.CompletedTask;
        }

        /// <summary>
        /// Get user notification preferences
        /// </summary>
        public async Task<UserNotificationPreference?> GetUserPreferencesAsync(string userId, Guid tenantId)
        {
            // TODO: Implement when UserNotificationPreferences DbSet is added
            return await Task.FromResult<UserNotificationPreference?>(null);
        }

        /// <summary>
        /// Create bulk notifications (e.g., for team announcements)
        /// </summary>
        public async Task<int> CreateBulkNotificationsAsync(
            Guid workflowInstanceId,
            List<string> recipientUserIds,
            string notificationType,
            string subject,
            string body,
            string priority = "Normal",
            Guid tenantId = default)
        {
            var notifications = recipientUserIds.Select(userId => new WorkflowNotification
            {
                WorkflowInstanceId = workflowInstanceId,
                TenantId = tenantId,
                RecipientUserId = userId,
                Recipient = userId,
                NotificationType = notificationType,
                Subject = subject,
                Message = subject,
                Body = body,
                Priority = priority,
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                RequiresEmail = true
            }).ToList();

            _context.WorkflowNotifications.AddRange(notifications);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Created {Count} bulk notifications for workflow {WorkflowId}",
                notifications.Count, workflowInstanceId);

            return notifications.Count;
        }

        /// <summary>
        /// Get user email address
        /// </summary>
        private async Task<string?> GetUserEmailAsync(string userId)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => u.Email)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Get template name for notification type
        /// </summary>
        private string GetTemplateForType(string notificationType)
        {
            return notificationType switch
            {
                "TaskAssigned" => "TaskAssigned",
                "ApprovalRequired" => "ApprovalRequired",
                "WorkflowCompleted" => "WorkflowCompleted",
                "Escalation" => "EscalationAlert",
                "SLA_Breach" => "SlaBreachWarning",
                "SLA_Warning" => "SlaBreachWarning",
                "SLA_Critical" => "SlaBreachWarning",
                _ => "TaskAssigned"
            };
        }
    }

    /// <summary>
    /// Result of a notification send operation
    /// </summary>
    public class NotificationResult
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public Guid NotificationId { get; set; }
    }

    /// <summary>
    /// Interface for notification service
    /// </summary>
    public interface INotificationService
    {
        Task<WorkflowNotification> CreateNotificationAsync(
            Guid workflowInstanceId,
            string recipientUserId,
            string notificationType,
            string subject,
            string body,
            string priority = "Normal",
            Guid tenantId = default,
            bool requiresEmail = true,
            bool requiresSms = false);

        Task<NotificationResult> SendNotificationAsync(
            Guid workflowInstanceId,
            string recipientUserId,
            string notificationType,
            string subject,
            string body,
            string priority = "Normal",
            Guid tenantId = default,
            Dictionary<string, object>? templateData = null);

        Task<List<WorkflowNotification>> GetPendingNotificationsAsync(Guid? tenantId = null);
        Task<List<WorkflowNotification>> GetUserNotificationsAsync(string userId, Guid tenantId, bool unreadOnly = false, int page = 1, int pageSize = 20);
        Task<int> GetUnreadCountAsync(string userId, Guid tenantId);
        Task MarkAsReadAsync(Guid notificationId, string userId);
        Task MarkAllAsReadAsync(string userId, Guid tenantId);
        Task MarkAsDeliveredAsync(Guid notificationId);
        Task UpdatePreferencesAsync(string userId, Guid tenantId, bool emailEnabled = true, bool smsEnabled = false, List<string>? enabledNotificationTypes = null);
        Task<UserNotificationPreference?> GetUserPreferencesAsync(string userId, Guid tenantId);
        Task<int> CreateBulkNotificationsAsync(Guid workflowInstanceId, List<string> recipientUserIds, string notificationType, string subject, string body, string priority = "Normal", Guid tenantId = default);
    }
}
