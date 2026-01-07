namespace GrcMvc.Configuration
{
    /// <summary>
    /// Twilio SMS integration settings
    /// </summary>
    public class TwilioSettings
    {
        /// <summary>
        /// Twilio Account SID
        /// </summary>
        public string AccountSid { get; set; } = string.Empty;

        /// <summary>
        /// Twilio Auth Token
        /// </summary>
        public string AuthToken { get; set; } = string.Empty;

        /// <summary>
        /// Phone number to send SMS from (E.164 format, e.g., +15551234567)
        /// </summary>
        public string FromNumber { get; set; } = string.Empty;

        /// <summary>
        /// Whether SMS notifications are enabled
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Maximum message length (SMS standard is 160 chars, but Twilio supports up to 1600)
        /// </summary>
        public int MaxMessageLength { get; set; } = 1600;

        /// <summary>
        /// Timeout in seconds for API requests
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;
    }
}
