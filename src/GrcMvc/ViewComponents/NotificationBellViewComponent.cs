using Microsoft.AspNetCore.Mvc;

namespace GrcMvc.ViewComponents
{
    /// <summary>
    /// Renders a notification bell dropdown
    /// Usage: @await Component.InvokeAsync("NotificationBell")
    /// </summary>
    public class NotificationBellViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(int maxItems = 5)
        {
            // In real implementation, fetch from database/service
            var notifications = GetRecentNotifications(maxItems);
            var unreadCount = notifications.Count(n => !n.IsRead);

            var model = new NotificationBellModel
            {
                Notifications = notifications,
                UnreadCount = unreadCount,
                MaxItems = maxItems
            };

            return View(model);
        }

        private List<NotificationItem> GetRecentNotifications(int max)
        {
            // Placeholder - replace with actual data service
            return new List<NotificationItem>
            {
                new() { Id = "1", Title = "New Assessment Due", Message = "Security Assessment Q1 is due in 3 days", Icon = "bi-calendar-event", Type = "warning", CreatedAt = DateTime.UtcNow.AddHours(-2), IsRead = false },
                new() { Id = "2", Title = "Control Approved", Message = "MFA Control has been approved", Icon = "bi-check-circle", Type = "success", CreatedAt = DateTime.UtcNow.AddHours(-5), IsRead = false },
                new() { Id = "3", Title = "Risk Updated", Message = "Data Breach risk level changed to High", Icon = "bi-exclamation-triangle", Type = "danger", CreatedAt = DateTime.UtcNow.AddDays(-1), IsRead = true }
            }.Take(max).ToList();
        }
    }

    public class NotificationBellModel
    {
        public List<NotificationItem> Notifications { get; set; } = new();
        public int UnreadCount { get; set; }
        public int MaxItems { get; set; }
    }

    public class NotificationItem
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public string Icon { get; set; } = "bi-bell";
        public string Type { get; set; } = "info"; // success, warning, danger, info
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public string? ActionUrl { get; set; }
    }
}
