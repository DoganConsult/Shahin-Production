namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Service for sending notifications to Slack channels
    /// </summary>
    public interface ISlackNotificationService
    {
        /// <summary>
        /// Send a simple text message to the default channel
        /// </summary>
        Task<bool> SendMessageAsync(string message);

        /// <summary>
        /// Send a message to a specific channel
        /// </summary>
        Task<bool> SendMessageAsync(string channel, string message);

        /// <summary>
        /// Send a rich notification with attachments
        /// </summary>
        Task<bool> SendNotificationAsync(SlackNotification notification);

        /// <summary>
        /// Send an alert with severity level
        /// </summary>
        Task<bool> SendAlertAsync(string title, string message, AlertSeverity severity);

        /// <summary>
        /// Check if Slack integration is enabled and configured
        /// </summary>
        bool IsEnabled { get; }
    }

    /// <summary>
    /// Slack notification with rich formatting
    /// </summary>
    public class SlackNotification
    {
        public string? Channel { get; set; }
        public string Text { get; set; } = string.Empty;
        public List<SlackAttachment> Attachments { get; set; } = new();
        public bool Markdown { get; set; } = true;
    }

    /// <summary>
    /// Slack attachment for rich formatting
    /// </summary>
    public class SlackAttachment
    {
        public string? Title { get; set; }
        public string? TitleLink { get; set; }
        public string? Text { get; set; }
        public string? Color { get; set; } // good, warning, danger, or hex color
        public List<SlackField> Fields { get; set; } = new();
        public string? Footer { get; set; }
        public long? Timestamp { get; set; }
    }

    /// <summary>
    /// Slack field for attachment formatting
    /// </summary>
    public class SlackField
    {
        public string Title { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public bool Short { get; set; } = true;
    }

    /// <summary>
    /// Alert severity levels
    /// </summary>
    public enum AlertSeverity
    {
        Info,
        Warning,
        Error,
        Critical
    }
}
