using GrcMvc.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Stub email service for development/testing - logs emails instead of sending
    /// </summary>
    public class StubEmailService : IEmailService
    {
        private readonly ILogger<StubEmailService> _logger;

        public StubEmailService(ILogger<StubEmailService> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            _logger.LogInformation("📧 [STUB] Email to {To}: {Subject}", to, subject);
            return Task.CompletedTask;
        }

        public Task SendEmailBatchAsync(string[] recipients, string subject, string htmlBody)
        {
            _logger.LogInformation("📧 [STUB] Batch email to {Count} recipients: {Subject}", recipients.Length, subject);
            return Task.CompletedTask;
        }

        public Task SendTemplatedEmailAsync(string to, string templateId, Dictionary<string, string> templateData)
        {
            _logger.LogInformation("📧 [STUB] Templated email to {To}: Template={Template}", to, templateId);
            return Task.CompletedTask;
        }
    }
}
