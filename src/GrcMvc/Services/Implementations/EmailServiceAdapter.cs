using GrcMvc.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Adapter that implements IEmailService using ISmtpEmailService
    /// Bridges the gap between the simple IEmailService interface and the full-featured SMTP service
    /// </summary>
    public class EmailServiceAdapter : IEmailService
    {
        private readonly ISmtpEmailService _smtpService;
        private readonly ILogger<EmailServiceAdapter> _logger;

        public EmailServiceAdapter(
            ISmtpEmailService smtpService,
            ILogger<EmailServiceAdapter> logger)
        {
            _smtpService = smtpService ?? throw new ArgumentNullException(nameof(smtpService));
            _logger = logger;
        }

        /// <summary>
        /// Send an email
        /// </summary>
        public async Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(to))
                {
                    _logger.LogWarning("Attempted to send email to empty recipient");
                    return;
                }

                if (!_smtpService.IsValidEmail(to))
                {
                    _logger.LogWarning("Invalid email address: {Email}", to);
                    throw new ArgumentException($"Invalid email address: {to}", nameof(to));
                }

                await _smtpService.SendEmailAsync(to, subject, htmlBody, isHtml: true);
                _logger.LogInformation("Email sent successfully to {Email} with subject: {Subject}", to, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}: {Message}", to, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Send an email to multiple recipients
        /// </summary>
        public async Task SendEmailBatchAsync(string[] recipients, string subject, string htmlBody)
        {
            if (recipients == null || recipients.Length == 0)
            {
                _logger.LogWarning("Attempted to send batch email to empty recipient list");
                return;
            }

            var validRecipients = new List<string>();
            foreach (var recipient in recipients)
            {
                if (string.IsNullOrWhiteSpace(recipient))
                    continue;

                if (!_smtpService.IsValidEmail(recipient))
                {
                    _logger.LogWarning("Invalid email address in batch: {Email}", recipient);
                    continue;
                }

                validRecipients.Add(recipient);
            }

            if (validRecipients.Count == 0)
            {
                _logger.LogWarning("No valid recipients in batch email");
                return;
            }

            try
            {
                // Send emails in parallel (with reasonable concurrency limit)
                var semaphore = new System.Threading.SemaphoreSlim(10); // Max 10 concurrent sends
                var tasks = validRecipients.Select(async recipient =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        await _smtpService.SendEmailAsync(recipient, subject, htmlBody, isHtml: true);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                await Task.WhenAll(tasks);
                _logger.LogInformation("Batch email sent successfully to {Count} recipients with subject: {Subject}", 
                    validRecipients.Count, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send batch email to {Count} recipients: {Message}", 
                    validRecipients.Count, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Send templated email
        /// </summary>
        public async Task SendTemplatedEmailAsync(string to, string templateId, Dictionary<string, string> templateData)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(to))
                {
                    _logger.LogWarning("Attempted to send templated email to empty recipient");
                    return;
                }

                if (!_smtpService.IsValidEmail(to))
                {
                    _logger.LogWarning("Invalid email address: {Email}", to);
                    throw new ArgumentException($"Invalid email address: {to}", nameof(to));
                }

                if (string.IsNullOrWhiteSpace(templateId))
                {
                    _logger.LogWarning("Template ID is empty");
                    throw new ArgumentException("Template ID cannot be empty", nameof(templateId));
                }

                // Convert Dictionary<string, string> to Dictionary<string, object> for SmtpEmailService
                var templateDataObj = new Dictionary<string, object>();
                if (templateData != null)
                {
                    foreach (var kvp in templateData)
                    {
                        templateDataObj[kvp.Key] = kvp.Value;
                    }
                }

                // Extract subject from template data or use default
                var subject = templateData?.TryGetValue("Subject", out var subj) == true 
                    ? subj 
                    : "Notification from GRC System";

                await _smtpService.SendTemplatedEmailAsync(to, subject, templateId, templateDataObj);
                _logger.LogInformation("Templated email '{Template}' sent successfully to {Email}", templateId, to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send templated email '{Template}' to {Email}: {Message}", 
                    templateId, to, ex.Message);
                throw;
            }
        }
    }
}
