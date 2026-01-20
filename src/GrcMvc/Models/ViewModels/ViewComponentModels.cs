using System.ComponentModel.DataAnnotations;

namespace GrcMvc.ViewComponents
{
    /// <summary>
    /// Model for ActivityFeed ViewComponent
    /// </summary>
    public class ActivityItem
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public string? Color { get; set; }
    }

    /// <summary>
    /// Model for Breadcrumb ViewComponent
    /// </summary>
    public class BreadcrumbItem
    {
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Model for DashboardWidget ViewComponent
    /// </summary>
    public class DashboardWidgetModel
    {
        public string Title { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }

    /// <summary>
    /// Model for DataTable ViewComponent
    /// </summary>
    public class DataTableModel
    {
        public List<string> Headers { get; set; } = new();
        public List<List<string>> Rows { get; set; } = new();
        public bool HasActions { get; set; }
        public string? ActionColumnTitle { get; set; }
    }

    /// <summary>
    /// Model for NotificationBell ViewComponent
    /// </summary>
    public class NotificationBellModel
    {
        public int UnreadCount { get; set; }
        public List<NotificationItem> Notifications { get; set; } = new();
    }

    /// <summary>
    /// Model for Timeline ViewComponent
    /// </summary>
    public class TimelineModel
    {
        public List<TimelineItem> Items { get; set; } = new();
    }

    /// <summary>
    /// Notification Item for NotificationBell
    /// </summary>
    public class NotificationItem
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public bool IsRead { get; set; }
        public string? Url { get; set; }
    }

    /// <summary>
    /// Timeline Item for Timeline
    /// </summary>
    public class TimelineItem
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }
}
