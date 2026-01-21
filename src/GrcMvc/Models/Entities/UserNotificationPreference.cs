using System;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// User notification preferences per tenant
    /// </summary>
    public class UserNotificationPreference : BaseEntity
    {
        public Guid TenantId { get; set; }
        public string UserId { get; set; } = string.Empty;

        // Channel preferences
        public bool EmailEnabled { get; set; } = true;
        public bool SmsEnabled { get; set; } = false;
        public bool InAppEnabled { get; set; } = true;
        public bool PushEnabled { get; set; } = false;

        // Notification type preferences (JSON array of enabled types)
        public string EnabledTypesJson { get; set; } = "[\"TaskAssigned\",\"TaskDueSoon\",\"TaskOverdue\",\"ApprovalRequested\",\"WorkflowCompleted\"]";

        // Frequency settings
        public string DigestFrequency { get; set; } = "Immediate"; // Immediate, Daily, Weekly
        public string PreferredTime { get; set; } = "09:00"; // For digest emails
        public string Timezone { get; set; } = "UTC";

        // Quiet hours
        public bool QuietHoursEnabled { get; set; } = false;
        public string QuietHoursStart { get; set; } = "22:00";
        public string QuietHoursEnd { get; set; } = "08:00";

        // Navigation
        public virtual Tenant Tenant { get; set; } = null!;
    }
}
