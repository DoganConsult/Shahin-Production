using System.ComponentModel.DataAnnotations;

namespace GrcMvc.Configuration
{
    /// <summary>
    /// Email settings configuration with proper validation.
    /// Uses nullable port to handle empty env var overrides gracefully.
    /// </summary>
    public sealed record EmailSettings
    {
        public const string SectionName = "EmailSettings";

        /// <summary>
        /// SMTP server hostname (e.g., smtp.office365.com)
        /// </summary>
        public string SmtpServer { get; init; } = "smtp.office365.com";

        /// <summary>
        /// SMTP port. Nullable to handle empty env var override gracefully.
        /// Use GetSmtpPort() to get the actual value with fallback.
        /// </summary>
        public int? SmtpPort { get; init; } = 587;

        /// <summary>
        /// Gets the SMTP port with proper fallback for empty/null values.
        /// </summary>
        public int GetSmtpPort() => SmtpPort ?? 587;

        /// <summary>
        /// Sender display name
        /// </summary>
        public string SenderName { get; init; } = "Shahin GRC";

        /// <summary>
        /// Sender email address
        /// </summary>
        public string SenderEmail { get; init; } = "noreply@shahin-ai.com";

        /// <summary>
        /// SMTP authentication username (optional)
        /// </summary>
        public string Username { get; init; } = string.Empty;

        /// <summary>
        /// SMTP authentication password (optional)
        /// </summary>
        public string Password { get; init; } = string.Empty;

        /// <summary>
        /// Enable SSL/TLS for SMTP connection
        /// </summary>
        public bool EnableSsl { get; init; } = true;

        /// <summary>
        /// Validates the configuration is usable
        /// </summary>
        public bool IsValid() => !string.IsNullOrWhiteSpace(SmtpServer) && GetSmtpPort() > 0;
    }
}
