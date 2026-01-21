namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Service for sending SMS notifications via Twilio
    /// </summary>
    public interface ISmsNotificationService
    {
        /// <summary>
        /// Send an SMS message to a phone number
        /// </summary>
        /// <param name="phoneNumber">Phone number in E.164 format (e.g., +15551234567)</param>
        /// <param name="message">Message content</param>
        Task<SmsResult> SendSmsAsync(string phoneNumber, string message);

        /// <summary>
        /// Send SMS to multiple recipients
        /// </summary>
        Task<List<SmsResult>> SendBulkSmsAsync(IEnumerable<string> phoneNumbers, string message);

        /// <summary>
        /// Send an alert SMS
        /// </summary>
        Task<SmsResult> SendAlertSmsAsync(string phoneNumber, string title, string message, AlertSeverity severity);

        /// <summary>
        /// Check if SMS integration is enabled and configured
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Validate phone number format
        /// </summary>
        bool ValidatePhoneNumber(string phoneNumber);
    }

    /// <summary>
    /// Result of SMS send operation
    /// </summary>
    public class SmsResult
    {
        public bool Success { get; set; }
        public string? MessageSid { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
