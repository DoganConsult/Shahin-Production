namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Service for sending notifications to Microsoft Teams channels
    /// </summary>
    public interface ITeamsNotificationService
    {
        /// <summary>
        /// Send a simple text message
        /// </summary>
        Task<bool> SendMessageAsync(string message);

        /// <summary>
        /// Send a rich adaptive card notification
        /// </summary>
        Task<bool> SendNotificationAsync(TeamsNotification notification);

        /// <summary>
        /// Send an alert with severity level
        /// </summary>
        Task<bool> SendAlertAsync(string title, string message, AlertSeverity severity);

        /// <summary>
        /// Check if Teams integration is enabled and configured
        /// </summary>
        bool IsEnabled { get; }
    }

    /// <summary>
    /// Teams notification using MessageCard format
    /// </summary>
    public class TeamsNotification
    {
        public string Title { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public string? Text { get; set; }
        public string? ThemeColor { get; set; }
        public List<TeamsSection> Sections { get; set; } = new();
        public List<TeamsAction> Actions { get; set; } = new();
    }

    /// <summary>
    /// Teams message card section
    /// </summary>
    public class TeamsSection
    {
        public string? Title { get; set; }
        public string? Text { get; set; }
        public string? ActivityTitle { get; set; }
        public string? ActivitySubtitle { get; set; }
        public string? ActivityImage { get; set; }
        public List<TeamsFact> Facts { get; set; } = new();
        public bool Markdown { get; set; } = true;
    }

    /// <summary>
    /// Teams fact (key-value pair)
    /// </summary>
    public class TeamsFact
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    /// <summary>
    /// Teams action button
    /// </summary>
    public class TeamsAction
    {
        public string Type { get; set; } = "OpenUri";
        public string Name { get; set; } = string.Empty;
        public List<TeamsTarget> Targets { get; set; } = new();
    }

    /// <summary>
    /// Teams action target
    /// </summary>
    public class TeamsTarget
    {
        public string Os { get; set; } = "default";
        public string Uri { get; set; } = string.Empty;
    }
}
