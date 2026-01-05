using System.ComponentModel.DataAnnotations;

namespace GrcMvc.Configuration
{
    public class EmailSettings
    {
        public const string SectionName = "EmailSettings";

        [Required]
        public string SmtpServer { get; set; } = string.Empty;

        [Range(1, 65535)]
        public int SmtpPort { get; set; } = 587;

        [Required]
        public string SenderName { get; set; } = "GRC System";

        [Required]
        [EmailAddress]
        public string SenderEmail { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = true;
    }
}
