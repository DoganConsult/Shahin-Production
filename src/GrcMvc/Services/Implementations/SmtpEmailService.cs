using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GrcMvc.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using RazorLight;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// SMTP Email Service with Razor template support
    /// </summary>
    public class SmtpEmailService : ISmtpEmailService
    {
        private readonly SmtpSettings _settings;
        private readonly ILogger<SmtpEmailService> _logger;
        private readonly RazorLightEngine _razorEngine;
        private readonly string _templatePath;

        public SmtpEmailService(
            IOptions<SmtpSettings> settings,
            ILogger<SmtpEmailService> logger,
            IWebHostEnvironment environment)
        {
            _settings = settings.Value;
            _logger = logger;
            _templatePath = Path.Combine(environment.ContentRootPath, "Views", "EmailTemplates");

            // Initialize Razor template engine
            _razorEngine = new RazorLightEngineBuilder()
                .UseFileSystemProject(_templatePath)
                .UseMemoryCachingProvider()
                .Build();
        }

        /// <summary>
        /// Send a simple text email
        /// </summary>
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            await SendEmailAsync(toEmail, subject, body, isHtml: false);
        }

        /// <summary>
        /// Send an email with HTML or plain text
        /// </summary>
        public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
                message.To.Add(MailboxAddress.Parse(toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder();
                if (isHtml)
                {
                    bodyBuilder.HtmlBody = body;
                    bodyBuilder.TextBody = StripHtml(body); // Plain text fallback
                }
                else
                {
                    bodyBuilder.TextBody = body;
                }

                message.Body = bodyBuilder.ToMessageBody();

                await SendMessageAsync(message);

                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}: {Message}", toEmail, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Send a templated email using Razor templates
        /// </summary>
        public async Task SendTemplatedEmailAsync(
            string toEmail,
            string subject,
            string templateName,
            Dictionary<string, object> templateData)
        {
            try
            {
                // Render the template
                var htmlBody = await RenderTemplateAsync(templateName, templateData);

                // Send the email
                await SendEmailAsync(toEmail, subject, htmlBody, isHtml: true);

                _logger.LogInformation(
                    "Templated email '{Template}' sent to {Email}",
                    templateName, toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send templated email '{Template}' to {Email}: {Message}",
                    templateName, toEmail, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Send email to multiple recipients
        /// </summary>
        public async Task SendBulkEmailAsync(
            List<string> toEmails,
            string subject,
            string templateName,
            Dictionary<string, object> templateData)
        {
            var htmlBody = await RenderTemplateAsync(templateName, templateData);

            var tasks = toEmails.Select(email =>
                SendEmailAsync(email, subject, htmlBody, isHtml: true));

            await Task.WhenAll(tasks);

            _logger.LogInformation(
                "Bulk email sent to {Count} recipients using template '{Template}'",
                toEmails.Count, templateName);
        }

        /// <summary>
        /// Send email with attachments
        /// </summary>
        public async Task SendEmailWithAttachmentAsync(
            string toEmail,
            string subject,
            string body,
            List<EmailAttachment> attachments)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
                message.To.Add(MailboxAddress.Parse(toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = body,
                    TextBody = StripHtml(body)
                };

                // Add attachments
                foreach (var attachment in attachments)
                {
                    if (attachment.Content != null)
                    {
                        bodyBuilder.Attachments.Add(
                            attachment.FileName,
                            attachment.Content,
                            ContentType.Parse(attachment.ContentType ?? "application/octet-stream"));
                    }
                    else if (!string.IsNullOrEmpty(attachment.FilePath))
                    {
                        bodyBuilder.Attachments.Add(attachment.FilePath);
                    }
                }

                message.Body = bodyBuilder.ToMessageBody();

                await SendMessageAsync(message);

                _logger.LogInformation(
                    "Email with {Count} attachments sent to {Email}",
                    attachments.Count, toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send email with attachments to {Email}: {Message}",
                    toEmail, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Render a Razor template to HTML string
        /// </summary>
        public async Task<string> RenderTemplateAsync(string templateName, Dictionary<string, object> data)
        {
            try
            {
                // Create dynamic model from dictionary
                var model = new DynamicTemplateModel(data);

                // Get template file path
                var templateFileName = templateName.EndsWith(".cshtml")
                    ? templateName
                    : $"{templateName}.cshtml";

                // Render template
                var result = await _razorEngine.CompileRenderAsync(templateFileName, model);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to render template '{Template}': {Message}", templateName, ex.Message);

                // Fallback to simple template
                return RenderFallbackTemplate(templateName, data);
            }
        }

        /// <summary>
        /// Render a simple fallback template when Razor fails
        /// </summary>
        private string RenderFallbackTemplate(string templateName, Dictionary<string, object> data)
        {
            var subject = data.TryGetValue("Subject", out var s) ? s?.ToString() : "Notification";
            var body = data.TryGetValue("Body", out var b) ? b?.ToString() : "";
            var priority = data.TryGetValue("Priority", out var p) ? p?.ToString() : "Normal";

            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; }}
        .header {{ background-color: #0066cc; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f5f5f5; }}
        .footer {{ padding: 10px; text-align: center; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>{subject}</h1>
        </div>
        <div class='content'>
            <p>Priority: {priority}</p>
            <p>{body}</p>
        </div>
        <div class='footer'>
            <p>GRC Management System</p>
        </div>
    </div>
</body>
</html>";
        }

        /// <summary>
        /// Send the MimeMessage via SMTP
        /// </summary>
        private async Task SendMessageAsync(MimeMessage message)
        {
            using var client = new MailKit.Net.Smtp.SmtpClient();

            try
            {
                // Connect to SMTP server
                await client.ConnectAsync(
                    _settings.Host,
                    _settings.Port,
                    _settings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);

                // Authenticate if credentials provided
                if (!string.IsNullOrEmpty(_settings.Username) && !string.IsNullOrEmpty(_settings.Password))
                {
                    await client.AuthenticateAsync(_settings.Username, _settings.Password);
                }

                // Send message
                await client.SendAsync(message);
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }

        /// <summary>
        /// Strip HTML tags from content
        /// </summary>
        private string StripHtml(string html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            // Simple HTML strip - for production use HtmlAgilityPack or similar
            return System.Text.RegularExpressions.Regex.Replace(html, "<[^>]*>", string.Empty);
        }

        /// <summary>
        /// Validate email address format
        /// </summary>
        public bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Dynamic model for Razor templates
    /// </summary>
    public class DynamicTemplateModel : System.Dynamic.DynamicObject
    {
        private readonly Dictionary<string, object> _data;

        public DynamicTemplateModel(Dictionary<string, object> data)
        {
            _data = data ?? new Dictionary<string, object>();
        }

        public override bool TryGetMember(System.Dynamic.GetMemberBinder binder, out object? result)
        {
            if (_data.TryGetValue(binder.Name, out var value))
            {
                result = value;
                return true;
            }

            result = null;
            return true; // Return null for missing properties instead of failing
        }

        public object? this[string key]
        {
            get => _data.TryGetValue(key, out var value) ? value : null;
        }
    }

    /// <summary>
    /// Email attachment model
    /// </summary>
    public class EmailAttachment
    {
        public string FileName { get; set; } = string.Empty;
        public string? FilePath { get; set; }
        public byte[]? Content { get; set; }
        public string? ContentType { get; set; }
    }

    /// <summary>
    /// Interface for SMTP email service
    /// </summary>
    public interface ISmtpEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml);
        Task SendTemplatedEmailAsync(string toEmail, string subject, string templateName, Dictionary<string, object> templateData);
        Task SendBulkEmailAsync(List<string> toEmails, string subject, string templateName, Dictionary<string, object> templateData);
        Task SendEmailWithAttachmentAsync(string toEmail, string subject, string body, List<EmailAttachment> attachments);
        Task<string> RenderTemplateAsync(string templateName, Dictionary<string, object> data);
        bool IsValidEmail(string email);
    }
}
