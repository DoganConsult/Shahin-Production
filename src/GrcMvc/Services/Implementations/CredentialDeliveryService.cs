using System;
using System.Text;
using System.Threading.Tasks;
using GrcMvc.Models.DTOs;
using GrcMvc.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Service for delivering tenant admin credentials
    /// </summary>
    public class CredentialDeliveryService : ICredentialDeliveryService
    {
        private readonly IAppEmailSender _emailSender;
        private readonly ILogger<CredentialDeliveryService> _logger;

        public CredentialDeliveryService(
            IAppEmailSender emailSender,
            ILogger<CredentialDeliveryService> logger)
        {
            _emailSender = emailSender;
            _logger = logger;
        }

        /// <summary>
        /// Send credentials via email
        /// </summary>
        public async Task<bool> SendCredentialsViaEmailAsync(
            Guid tenantId,
            string recipientEmail,
            TenantAdminCredentialsDto credentials)
        {
            try
            {
                var subject = "Your GRC Platform Admin Credentials - Action Required";
                var htmlMessage = GenerateEmailBody(credentials);

                await _emailSender.SendEmailAsync(recipientEmail, subject, htmlMessage);

                _logger.LogInformation("Sent credentials email to {Email} for tenant {TenantId}", 
                    recipientEmail, tenantId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending credentials email to {Email} for tenant {TenantId}", 
                    recipientEmail, tenantId);
                return false;
            }
        }

        /// <summary>
        /// Generate credentials PDF for download
        /// </summary>
        public async Task<byte[]> GenerateCredentialsPdfAsync(
            Guid tenantId,
            TenantAdminCredentialsDto credentials)
        {
            try
            {
                // For now, return a simple text-based PDF representation
                // In production, use a library like QuestPDF or iTextSharp
                var content = GenerateCredentialsText(credentials);
                var pdfBytes = Encoding.UTF8.GetBytes(content);

                _logger.LogInformation("Generated credentials PDF for tenant {TenantId}", tenantId);

                return await Task.FromResult(pdfBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating credentials PDF for tenant {TenantId}", tenantId);
                throw;
            }
        }

        /// <summary>
        /// Get credentials formatted for manual sharing (copy/paste)
        /// </summary>
        public async Task<string> GetCredentialsForManualShareAsync(
            Guid tenantId,
            TenantAdminCredentialsDto credentials)
        {
            try
            {
                var text = GenerateCredentialsText(credentials);

                _logger.LogInformation("Generated credentials text for manual share for tenant {TenantId}", tenantId);

                return await Task.FromResult(text);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating credentials text for tenant {TenantId}", tenantId);
                throw;
            }
        }

        /// <summary>
        /// Generate email body HTML
        /// </summary>
        private string GenerateEmailBody(TenantAdminCredentialsDto credentials)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head><meta charset='utf-8'><title>GRC Platform Admin Credentials</title></head>");
            sb.AppendLine("<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>");
            sb.AppendLine("<div style='max-width: 600px; margin: 0 auto; padding: 20px;'>");
            sb.AppendLine("<h2 style='color: #2c3e50;'>Your GRC Platform Admin Credentials</h2>");
            sb.AppendLine("<p>Dear Administrator,</p>");
            sb.AppendLine("<p>Your admin account has been created for <strong>" + credentials.OrganizationName + "</strong>.</p>");
            sb.AppendLine("<div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>");
            sb.AppendLine("<h3 style='margin-top: 0;'>Login Information</h3>");
            sb.AppendLine("<p><strong>Tenant ID:</strong> " + credentials.TenantId + "</p>");
            sb.AppendLine("<p><strong>Username:</strong> " + credentials.Username + "</p>");
            sb.AppendLine("<p><strong>Password:</strong> " + credentials.Password + "</p>");
            sb.AppendLine("<p><strong>Login URL:</strong> <a href='" + credentials.LoginUrl + "'>" + credentials.LoginUrl + "</a></p>");
            sb.AppendLine("<p><strong>Credentials Expire:</strong> " + credentials.ExpiresAt.ToString("yyyy-MM-dd HH:mm:ss UTC") + "</p>");
            sb.AppendLine("</div>");
            sb.AppendLine("<div style='background-color: #fff3cd; padding: 15px; border-radius: 5px; margin: 20px 0; border-left: 4px solid #ffc107;'>");
            sb.AppendLine("<h4 style='margin-top: 0; color: #856404;'>⚠️ Important Security Instructions</h4>");
            sb.AppendLine("<ul style='margin: 10px 0; padding-left: 20px;'>");
            sb.AppendLine("<li>These credentials are temporary and will expire on " + credentials.ExpiresAt.ToString("yyyy-MM-dd") + "</li>");
            if (credentials.MustChangePasswordOnFirstLogin)
            {
                sb.AppendLine("<li><strong>You must change your password on first login</strong></li>");
            }
            sb.AppendLine("<li>Do not share these credentials with anyone</li>");
            sb.AppendLine("<li>Use a secure connection (HTTPS) when logging in</li>");
            sb.AppendLine("<li>If you did not request these credentials, please contact support immediately</li>");
            sb.AppendLine("</ul>");
            sb.AppendLine("</div>");
            sb.AppendLine("<p>Best regards,<br>GRC Platform Team</p>");
            sb.AppendLine("</div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        /// <summary>
        /// Generate credentials as plain text
        /// </summary>
        private string GenerateCredentialsText(TenantAdminCredentialsDto credentials)
        {
            var sb = new StringBuilder();
            sb.AppendLine("GRC Platform Admin Credentials");
            sb.AppendLine("==============================");
            sb.AppendLine();
            sb.AppendLine("Organization: " + credentials.OrganizationName);
            sb.AppendLine("Tenant ID: " + credentials.TenantId);
            sb.AppendLine("Username: " + credentials.Username);
            sb.AppendLine("Password: " + credentials.Password);
            sb.AppendLine("Login URL: " + credentials.LoginUrl);
            sb.AppendLine("Credentials Expire: " + credentials.ExpiresAt.ToString("yyyy-MM-dd HH:mm:ss UTC"));
            sb.AppendLine();
            sb.AppendLine("IMPORTANT SECURITY INSTRUCTIONS:");
            sb.AppendLine("- These credentials are temporary and will expire on " + credentials.ExpiresAt.ToString("yyyy-MM-dd"));
            if (credentials.MustChangePasswordOnFirstLogin)
            {
                sb.AppendLine("- You MUST change your password on first login");
            }
            sb.AppendLine("- Do not share these credentials with anyone");
            sb.AppendLine("- Use a secure connection (HTTPS) when logging in");
            sb.AppendLine();
            sb.AppendLine("Generated: " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"));

            return sb.ToString();
        }
    }
}
