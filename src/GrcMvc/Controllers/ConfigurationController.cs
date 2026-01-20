using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrcMvc.Controllers;

/// <summary>
/// Configuration Management Controller - Manage app settings from UI
/// Platform Admin only
/// </summary>
[Authorize(Roles = "PlatformAdmin")]
[Route("platform-admin/configuration")]
public class ConfigurationController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ConfigurationController> _logger;

    public ConfigurationController(
        IConfiguration configuration,
        IWebHostEnvironment environment,
        ILogger<ConfigurationController> logger)
    {
        _configuration = configuration;
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// Configuration Dashboard
    /// </summary>
    [HttpGet]
    public IActionResult Index()
    {
        var model = new ConfigurationViewModel
        {
            Environment = _environment.EnvironmentName,
            
            // SMTP Settings
            SmtpHost = _configuration["SmtpSettings:Host"] ?? "",
            SmtpPort = _configuration["SmtpSettings:Port"] ?? "587",
            SmtpUsername = _configuration["SmtpSettings:Username"] ?? "",
            SmtpFromEmail = _configuration["SmtpSettings:FromEmail"] ?? "",
            SmtpPasswordSet = !string.IsNullOrEmpty(GetEnvOrConfig("SMTP_PASSWORD", "SmtpSettings:Password")),
            
            // Database
            DbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "(from connection string)",
            DbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "GrcMvcDb",
            DbPasswordSet = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DB_PASSWORD")),
            
            // JWT
            JwtSecretSet = !string.IsNullOrEmpty(GetEnvOrConfig("JWT_SECRET", "JwtSettings:Secret")),
            JwtIssuer = _configuration["JwtSettings:Issuer"] ?? "",
            JwtAudience = _configuration["JwtSettings:Audience"] ?? "",
            
            // Azure
            AzureTenantId = MaskValue(Environment.GetEnvironmentVariable("AZURE_TENANT_ID")),
            MsGraphClientId = MaskValue(Environment.GetEnvironmentVariable("MSGRAPH_CLIENT_ID")),
            MsGraphSecretSet = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MSGRAPH_CLIENT_SECRET")),
            
            // AI
            ClaudeEnabled = _configuration.GetValue<bool>("ClaudeAgents:Enabled", false),
            ClaudeApiKeySet = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CLAUDE_API_KEY")),
            ClaudeModel = _configuration["ClaudeAgents:Model"] ?? "claude-sonnet-4-20250514",
            
            // App Info
            AppName = _configuration["AppInfo:Name"] ?? "Shahin",
            AppVersion = _configuration["AppInfo:Version"] ?? "1.0.0",
            BaseUrl = _configuration["App:BaseUrl"] ?? ""
        };

        return View("~/Views/PlatformAdmin/Configuration.cshtml", model);
    }

    /// <summary>
    /// Update SMTP Settings
    /// </summary>
    [HttpPost("smtp")]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateSmtp(SmtpUpdateModel model)
    {
        try
        {
            // Set environment variables (runtime only - not persisted)
            if (!string.IsNullOrEmpty(model.Host))
                Environment.SetEnvironmentVariable("SMTP_HOST", model.Host);
            if (!string.IsNullOrEmpty(model.Username))
                Environment.SetEnvironmentVariable("SMTP_USERNAME", model.Username);
            if (!string.IsNullOrEmpty(model.FromEmail))
                Environment.SetEnvironmentVariable("SMTP_FROM_EMAIL", model.FromEmail);
            if (!string.IsNullOrEmpty(model.Password))
                Environment.SetEnvironmentVariable("SMTP_PASSWORD", model.Password);

            _logger.LogInformation("SMTP settings updated by admin");
            TempData["Success"] = "SMTP settings updated (runtime only - restart will reset)";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating SMTP settings");
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Update AI Settings
    /// </summary>
    [HttpPost("ai")]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateAi(AiUpdateModel model)
    {
        try
        {
            if (!string.IsNullOrEmpty(model.ApiKey))
                Environment.SetEnvironmentVariable("CLAUDE_API_KEY", model.ApiKey);
            if (!string.IsNullOrEmpty(model.ModelName))
                Environment.SetEnvironmentVariable("CLAUDE_MODEL", model.ModelName);
            Environment.SetEnvironmentVariable("CLAUDE_ENABLED", model.Enabled.ToString().ToLower());

            _logger.LogInformation("AI settings updated by admin");
            TempData["Success"] = "AI settings updated (runtime only)";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating AI settings");
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Test SMTP Connection
    /// </summary>
    [HttpPost("test-smtp")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TestSmtp(string testEmail)
    {
        try
        {
            var smtpHost = GetEnvOrConfig("SMTP_HOST", "SmtpSettings:Host") ?? "smtp.office365.com";
            var smtpPort = int.Parse(GetEnvOrConfig("SMTP_PORT", "SmtpSettings:Port") ?? "587");
            var username = GetEnvOrConfig("SMTP_USERNAME", "SmtpSettings:Username") ?? "";
            var password = GetEnvOrConfig("SMTP_PASSWORD", "SmtpSettings:Password") ?? "";
            var fromEmail = GetEnvOrConfig("SMTP_FROM_EMAIL", "SmtpSettings:FromEmail") ?? username;

            using var client = new System.Net.Mail.SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new System.Net.NetworkCredential(username, password),
                EnableSsl = true,
                Timeout = 30000
            };

            var message = new System.Net.Mail.MailMessage
            {
                From = new System.Net.Mail.MailAddress(fromEmail, "Shahin GRC"),
                Subject = "Shahin GRC - SMTP Test",
                Body = $"SMTP test successful!\n\nTime: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC",
                IsBodyHtml = false
            };
            message.To.Add(testEmail);

            await client.SendMailAsync(message);

            TempData["Success"] = $"Test email sent to {testEmail}";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"SMTP test failed: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Export current configuration
    /// </summary>
    [HttpGet("export")]
    public IActionResult Export()
    {
        var config = new
        {
            Environment = _environment.EnvironmentName,
            Timestamp = DateTime.UtcNow,
            Settings = new
            {
                Smtp = new { Host = _configuration["SmtpSettings:Host"], Port = _configuration["SmtpSettings:Port"] },
                Jwt = new { Issuer = _configuration["JwtSettings:Issuer"], Audience = _configuration["JwtSettings:Audience"] },
                App = new { Name = _configuration["AppInfo:Name"], Version = _configuration["AppInfo:Version"] }
            },
            EnvVarsSet = new
            {
                SMTP_PASSWORD = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SMTP_PASSWORD")),
                JWT_SECRET = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JWT_SECRET")),
                DB_PASSWORD = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DB_PASSWORD")),
                CLAUDE_API_KEY = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CLAUDE_API_KEY"))
            }
        };

        return Json(config);
    }

    #region Helpers

    private string? GetEnvOrConfig(string envVar, string configKey)
    {
        return Environment.GetEnvironmentVariable(envVar) ?? _configuration[configKey];
    }

    private string MaskValue(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "(not set)";
        if (value.Length <= 8) return "****";
        return value.Substring(0, 4) + "****" + value.Substring(value.Length - 4);
    }

    #endregion
}

public class ConfigurationViewModel
{
    public string Environment { get; set; } = "";
    
    // SMTP
    public string SmtpHost { get; set; } = "";
    public string SmtpPort { get; set; } = "";
    public string SmtpUsername { get; set; } = "";
    public string SmtpFromEmail { get; set; } = "";
    public bool SmtpPasswordSet { get; set; }
    
    // Database
    public string DbHost { get; set; } = "";
    public string DbName { get; set; } = "";
    public bool DbPasswordSet { get; set; }
    
    // JWT
    public bool JwtSecretSet { get; set; }
    public string JwtIssuer { get; set; } = "";
    public string JwtAudience { get; set; } = "";
    
    // Azure
    public string AzureTenantId { get; set; } = "";
    public string MsGraphClientId { get; set; } = "";
    public bool MsGraphSecretSet { get; set; }
    
    // AI
    public bool ClaudeEnabled { get; set; }
    public bool ClaudeApiKeySet { get; set; }
    public string ClaudeModel { get; set; } = "";
    
    // App
    public string AppName { get; set; } = "";
    public string AppVersion { get; set; } = "";
    public string BaseUrl { get; set; } = "";
}

public class SmtpUpdateModel
{
    public string? Host { get; set; }
    public string? Port { get; set; }
    public string? Username { get; set; }
    public string? FromEmail { get; set; }
    public string? Password { get; set; }
}

public class AiUpdateModel
{
    public string? ApiKey { get; set; }
    public string? ModelName { get; set; }
    public bool Enabled { get; set; }
}
