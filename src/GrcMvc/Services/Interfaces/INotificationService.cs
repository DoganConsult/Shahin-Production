using GrcMvc.Models.Entities;
using GrcMvc.Models.Workflows;

namespace GrcMvc.Services.Interfaces
{
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
        Task<Models.Entities.UserNotificationPreference?> GetUserPreferencesAsync(string userId, Guid tenantId);
        Task<int> CreateBulkNotificationsAsync(Guid workflowInstanceId, List<string> recipientUserIds, string notificationType, string subject, string body, string priority = "Normal", Guid tenantId = default);
    }

    /// <summary>
    /// Notification result model
    /// </summary>
    public class NotificationResult
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public Guid NotificationId { get; set; }
    }
}
