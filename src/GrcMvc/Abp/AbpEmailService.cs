using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GrcMvc.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Emailing;

namespace GrcMvc.Abp;

/// <summary>
/// ABP-based email service implementation.
/// Uses ABP's IEmailSender which supports:
/// - Background email queue
/// - SMTP configuration via settings
/// - Template support
/// - Multi-tenant email settings
/// </summary>
public class AbpEmailService : IEmailService, ITransientDependency
{
    private readonly IEmailSender _abpEmailSender;
    private readonly ILogger<AbpEmailService> _logger;

    public AbpEmailService(
        IEmailSender abpEmailSender,
        ILogger<AbpEmailService> logger)
    {
        _abpEmailSender = abpEmailSender;
        _logger = logger;
    }

    /// <summary>
    /// Send an email using ABP's email sender
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

            await _abpEmailSender.SendAsync(
                to: to,
                subject: subject,
                body: htmlBody,
                isBodyHtml: true
            );

            _logger.LogInformation("Email sent via ABP to {Email} with subject: {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email via ABP to {Email}: {Message}", to, ex.Message);
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
            _logger.LogWarning("Attempted to send batch email with no recipients");
            return;
        }

        foreach (var recipient in recipients)
        {
            try
            {
                await SendEmailAsync(recipient, subject, htmlBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send batch email to {Email}", recipient);
                // Continue sending to other recipients
            }
        }
    }

    /// <summary>
    /// Send templated email - replaces placeholders in template
    /// </summary>
    public async Task SendTemplatedEmailAsync(string to, string templateId, Dictionary<string, string> templateData)
    {
        try
        {
            // Build email from template data
            var subject = templateData.TryGetValue("Subject", out var subj) ? subj : "Notification";
            var body = new StringBuilder();

            // Get template body or use default
            if (templateData.TryGetValue("Body", out var templateBody))
            {
                body.Append(templateBody);
                // Replace placeholders
                foreach (var kvp in templateData)
                {
                    body.Replace($"{{{{{kvp.Key}}}}}", kvp.Value);
                    body.Replace($"{{${kvp.Key}}}", kvp.Value);
                }
            }
            else
            {
                body.Append($"<p>Template: {templateId}</p>");
                foreach (var kvp in templateData)
                {
                    body.Append($"<p><strong>{kvp.Key}:</strong> {kvp.Value}</p>");
                }
            }

            await SendEmailAsync(to, subject, body.ToString());
            _logger.LogInformation("Templated email sent via ABP to {Email} using template {Template}", to, templateId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send templated email to {Email}", to);
            throw;
        }
    }
}
