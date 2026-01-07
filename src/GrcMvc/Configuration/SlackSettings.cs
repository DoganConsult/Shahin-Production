namespace GrcMvc.Configuration
{
    /// <summary>
    /// Slack integration settings for sending notifications via Slack webhooks
    /// </summary>
    public class SlackSettings
    {
        /// <summary>
        /// Default Slack incoming webhook URL
        /// </summary>
        public string WebhookUrl { get; set; } = string.Empty;

        /// <summary>
        /// Default channel for notifications (e.g., #grc-alerts)
        /// </summary>
        public string DefaultChannel { get; set; } = "#grc-alerts";

        /// <summary>
        /// Bot username that will appear in Slack
        /// </summary>
        public string BotUsername { get; set; } = "Shahin GRC Bot";

        /// <summary>
        /// Icon emoji for the bot (e.g., :shield:)
        /// </summary>
        public string IconEmoji { get; set; } = ":shield:";

        /// <summary>
        /// Whether Slack notifications are enabled
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Timeout in seconds for webhook requests
        /// </summary>
        public int TimeoutSeconds { get; set; } = 10;
    }
}
