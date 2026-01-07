namespace GrcMvc.Configuration
{
    /// <summary>
    /// Microsoft Teams integration settings for sending notifications via Teams webhooks
    /// </summary>
    public class TeamsSettings
    {
        /// <summary>
        /// Default Teams incoming webhook URL
        /// </summary>
        public string WebhookUrl { get; set; } = string.Empty;

        /// <summary>
        /// Whether Teams notifications are enabled
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Timeout in seconds for webhook requests
        /// </summary>
        public int TimeoutSeconds { get; set; } = 10;

        /// <summary>
        /// Theme color for message cards (hex without #)
        /// </summary>
        public string ThemeColor { get; set; } = "0076D7";
    }
}
