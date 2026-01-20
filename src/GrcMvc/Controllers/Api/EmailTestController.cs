using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using GrcMvc.Configuration;

namespace GrcMvc.Controllers.Api;

/// <summary>
/// Email Test Controller - Test SMTP connectivity
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "PlatformAdmin")]
public class EmailTestController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailTestController> _logger;

    public EmailTestController(
        IConfiguration configuration,
        ILogger<EmailTestController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Test SMTP connection and send test email
    /// </summary>
    [HttpPost("test")]
    public async Task<IActionResult> TestEmail([FromBody] SendTestEmailRequest request)
    {
        try
        {
            var smtpHost = _configuration["SmtpSettings:Host"] ?? "smtp.office365.com";
            var smtpPort = int.Parse(_configuration["SmtpSettings:Port"] ?? "587");
            var username = _configuration["SmtpSettings:Username"] ?? "";
            var password = GetSmtpPassword();
            var fromEmail = _configuration["SmtpSettings:FromEmail"] ?? username;
            var fromName = _configuration["SmtpSettings:FromName"] ?? "Shahin GRC";
            var enableSsl = bool.Parse(_configuration["SmtpSettings:EnableSsl"] ?? "true");

            _logger.LogInformation("Testing SMTP connection to {Host}:{Port}", smtpHost, smtpPort);

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = enableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 30000
            };

            var message = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = request.Subject ?? "Shahin GRC - Test Email",
                Body = request.Body ?? $"This is a test email from Shahin GRC.\n\nSent at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC",
                IsBodyHtml = false
            };
            message.To.Add(request.To);

            await client.SendMailAsync(message);

            _logger.LogInformation("✅ Test email sent successfully to {To}", request.To);

            return Ok(new
            {
                success = true,
                message = $"Email sent successfully to {request.To}",
                timestamp = DateTime.UtcNow,
                smtpHost,
                smtpPort,
                fromEmail
            });
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex, "SMTP Error: {Message}", ex.Message);
            return BadRequest(new
            {
                success = false,
                error = "SMTP Error",
                message = ex.Message,
                statusCode = ex.StatusCode.ToString()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email test failed: {Message}", ex.Message);
            return BadRequest(new
            {
                success = false,
                error = ex.GetType().Name,
                message = ex.Message
            });
        }
    }

    /// <summary>
    /// Check SMTP configuration status
    /// </summary>
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        var smtpHost = _configuration["SmtpSettings:Host"];
        var smtpPort = _configuration["SmtpSettings:Port"];
        var username = _configuration["SmtpSettings:Username"];
        var password = GetSmtpPassword();
        var fromEmail = _configuration["SmtpSettings:FromEmail"];

        return Ok(new
        {
            configured = !string.IsNullOrEmpty(smtpHost) && !string.IsNullOrEmpty(password),
            smtpHost,
            smtpPort,
            username,
            fromEmail,
            passwordSet = !string.IsNullOrEmpty(password),
            passwordSource = GetPasswordSource()
        });
    }

    private string GetSmtpPassword()
    {
        // Priority: Environment variable > appsettings
        var envPassword = Environment.GetEnvironmentVariable("SMTP_PASSWORD");
        if (!string.IsNullOrEmpty(envPassword))
            return envPassword;

        var configPassword = _configuration["SmtpSettings:Password"];
        if (!string.IsNullOrEmpty(configPassword) && !configPassword.StartsWith("${"))
            return configPassword;

        return string.Empty;
    }

    private string GetPasswordSource()
    {
        var envPassword = Environment.GetEnvironmentVariable("SMTP_PASSWORD");
        if (!string.IsNullOrEmpty(envPassword))
            return "Environment Variable (SMTP_PASSWORD)";

        var configPassword = _configuration["SmtpSettings:Password"];
        if (!string.IsNullOrEmpty(configPassword) && !configPassword.StartsWith("${"))
            return "appsettings.json";

        return "Not configured";
    }
}

public class SendTestEmailRequest
{
    public string To { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string? Body { get; set; }
}
