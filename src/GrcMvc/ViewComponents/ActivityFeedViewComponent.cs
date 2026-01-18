using Microsoft.AspNetCore.Mvc;

namespace GrcMvc.ViewComponents
{
    /// <summary>
    /// Renders a recent activity feed
    /// Usage: @await Component.InvokeAsync("ActivityFeed", new { maxItems = 10 })
    /// </summary>
    public class ActivityFeedViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(int maxItems = 10, string? entityType = null)
        {
            // In real implementation, fetch from audit log/activity service
            var activities = GetRecentActivities(maxItems, entityType);
            return View(activities);
        }

        private List<ActivityItem> GetRecentActivities(int max, string? entityType)
        {
            // Placeholder - replace with actual data
            var activities = new List<ActivityItem>
            {
                new() { Id = "1", Action = "Created", EntityType = "Risk", EntityName = "Data Breach Risk", User = "Ahmed Ali", UserAvatar = null, Timestamp = DateTime.UtcNow.AddMinutes(-15), Icon = "bi-plus-circle", Color = "success" },
                new() { Id = "2", Action = "Updated", EntityType = "Control", EntityName = "Access Control Policy", User = "Sara Mohammed", Timestamp = DateTime.UtcNow.AddHours(-1), Icon = "bi-pencil", Color = "primary" },
                new() { Id = "3", Action = "Approved", EntityType = "Assessment", EntityName = "Q1 Security Assessment", User = "Khalid Hassan", Timestamp = DateTime.UtcNow.AddHours(-3), Icon = "bi-check-circle", Color = "success" },
                new() { Id = "4", Action = "Commented", EntityType = "Audit", EntityName = "External Audit 2024", User = "Fatima Al-Rashid", Timestamp = DateTime.UtcNow.AddHours(-5), Icon = "bi-chat", Color = "info" },
                new() { Id = "5", Action = "Deleted", EntityType = "Policy", EntityName = "Old Security Policy v1", User = "Omar Yusuf", Timestamp = DateTime.UtcNow.AddDays(-1), Icon = "bi-trash", Color = "danger" }
            };

            if (!string.IsNullOrEmpty(entityType))
                activities = activities.Where(a => a.EntityType.Equals(entityType, StringComparison.OrdinalIgnoreCase)).ToList();

            return activities.Take(max).ToList();
        }
    }

    public class ActivityItem
    {
        public string Id { get; set; } = "";
        public string Action { get; set; } = "";
        public string EntityType { get; set; } = "";
        public string EntityName { get; set; } = "";
        public string? EntityUrl { get; set; }
        public string User { get; set; } = "";
        public string? UserAvatar { get; set; }
        public DateTime Timestamp { get; set; }
        public string Icon { get; set; } = "bi-activity";
        public string Color { get; set; } = "secondary";
    }
}
