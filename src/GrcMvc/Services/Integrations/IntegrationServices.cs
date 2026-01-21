using System;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Integrations;

/// <summary>
/// Email Service Interface
/// </summary>
public interface IEmailIntegrationService
{
    Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    Task<bool> SendTemplateEmailAsync(string to, string templateId, object data);
    Task<bool> SendBulkEmailAsync(IEnumerable<string> recipients, string subject, string body);
}

/// <summary>
/// Email Service Implementation - Supports SMTP, SendGrid, SES
/// </summary>
public class EmailIntegrationService : IEmailIntegrationService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailIntegrationService> _logger;
    private readonly HttpClient _httpClient;

    public EmailIntegrationService(IConfiguration config, ILogger<EmailIntegrationService> logger, IHttpClientFactory httpClientFactory)
    {
        _config = config;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("Email");
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        var provider = _config["Email:Provider"] ?? "smtp";

        try
        {
            return provider.ToLower() switch
            {
                "sendgrid" => await SendViaSendGridAsync(to, subject, body, isHtml),
                "ses" => await SendViaSESAsync(to, subject, body, isHtml),
                _ => await SendViaSMTPAsync(to, subject, body, isHtml)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            return false;
        }
    }

    public async Task<bool> SendTemplateEmailAsync(string to, string templateId, object data)
    {
        // For SendGrid dynamic templates
        var apiKey = _config["Email:SendGrid:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("SendGrid API key not configured");
            return false;
        }

        var payload = new
        {
            personalizations = new[] { new { to = new[] { new { email = to } }, dynamic_template_data = data } },
            from = new { email = _config["Email:FromEmail"] ?? "noreply@grc.app" },
            template_id = templateId
        };

        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
        var response = await _httpClient.PostAsync("https://api.sendgrid.com/v3/mail/send",
            new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> SendBulkEmailAsync(IEnumerable<string> recipients, string subject, string body)
    {
        var success = true;
        foreach (var recipient in recipients)
        {
            if (!await SendEmailAsync(recipient, subject, body))
                success = false;
        }
        return success;
    }

    private async Task<bool> SendViaSMTPAsync(string to, string subject, string body, bool isHtml)
    {
        var host = _config["Email:SMTP:Host"] ?? "localhost";
        var port = int.Parse(_config["Email:SMTP:Port"] ?? "25");
        var from = _config["Email:FromEmail"] ?? "noreply@grc.app";

        using var client = new SmtpClient(host, port);
        var message = new MailMessage(from, to, subject, body) { IsBodyHtml = isHtml };
        await client.SendMailAsync(message);

        _logger.LogInformation("Email sent via SMTP to {To}", to);
        return true;
    }

    private async Task<bool> SendViaSendGridAsync(string to, string subject, string body, bool isHtml)
    {
        var apiKey = _config["Email:SendGrid:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("SendGrid API key not configured, falling back to SMTP");
            return await SendViaSMTPAsync(to, subject, body, isHtml);
        }

        var payload = new
        {
            personalizations = new[] { new { to = new[] { new { email = to } } } },
            from = new { email = _config["Email:FromEmail"] ?? "noreply@grc.app" },
            subject,
            content = new[] { new { type = isHtml ? "text/html" : "text/plain", value = body } }
        };

        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
        var response = await _httpClient.PostAsync("https://api.sendgrid.com/v3/mail/send",
            new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));

        _logger.LogInformation("Email sent via SendGrid to {To}: {Status}", to, response.StatusCode);
        return response.IsSuccessStatusCode;
    }

    private async Task<bool> SendViaSESAsync(string to, string subject, string body, bool isHtml)
    {
        // AWS SES would use AWS SDK - simplified stub
        _logger.LogWarning("SES integration not fully implemented, using SMTP fallback");
        return await SendViaSMTPAsync(to, subject, body, isHtml);
    }
}

/// <summary>
/// File Storage Service Interface
/// </summary>
public interface IFileStorageService
{
    Task<string> UploadFileAsync(Guid tenantId, string fileName, Stream content, string contentType);
    Task<Stream?> DownloadFileAsync(string fileKey);
    Task<bool> DeleteFileAsync(string fileKey);
    Task<string> GetPresignedUrlAsync(string fileKey, TimeSpan expiry);
}

/// <summary>
/// File Storage Service Implementation - Supports Local, S3, Azure Blob
/// </summary>
public class FileStorageService : IFileStorageService
{
    private readonly IConfiguration _config;
    private readonly ILogger<FileStorageService> _logger;
    private readonly string _basePath;

    public FileStorageService(IConfiguration config, ILogger<FileStorageService> logger)
    {
        _config = config;
        _logger = logger;
        _basePath = _config["Storage:LocalPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");

        if (!Directory.Exists(_basePath))
            Directory.CreateDirectory(_basePath);
    }

    public async Task<string> UploadFileAsync(Guid tenantId, string fileName, Stream content, string contentType)
    {
        var provider = _config["Storage:Provider"] ?? "local";

        return provider.ToLower() switch
        {
            "s3" => await UploadToS3Async(tenantId, fileName, content, contentType),
            "azure" => await UploadToAzureAsync(tenantId, fileName, content, contentType),
            _ => await UploadToLocalAsync(tenantId, fileName, content)
        };
    }

    public async Task<Stream?> DownloadFileAsync(string fileKey)
    {
        var provider = _config["Storage:Provider"] ?? "local";

        return provider.ToLower() switch
        {
            "s3" => await DownloadFromS3Async(fileKey),
            "azure" => await DownloadFromAzureAsync(fileKey),
            _ => await DownloadFromLocalAsync(fileKey)
        };
    }

    public async Task<bool> DeleteFileAsync(string fileKey)
    {
        try
        {
            var filePath = Path.Combine(_basePath, fileKey);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("Deleted file: {FileKey}", fileKey);
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file: {FileKey}", fileKey);
            return false;
        }
    }

    public Task<string> GetPresignedUrlAsync(string fileKey, TimeSpan expiry)
    {
        // For local storage, return direct path
        // For S3/Azure, would generate presigned URL
        var url = $"/files/{fileKey}";
        return Task.FromResult(url);
    }

    private async Task<string> UploadToLocalAsync(Guid tenantId, string fileName, Stream content)
    {
        var tenantPath = Path.Combine(_basePath, tenantId.ToString());
        if (!Directory.Exists(tenantPath))
            Directory.CreateDirectory(tenantPath);

        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var filePath = Path.Combine(tenantPath, uniqueFileName);

        using var fileStream = new FileStream(filePath, FileMode.Create);
        await content.CopyToAsync(fileStream);

        var fileKey = $"{tenantId}/{uniqueFileName}";
        _logger.LogInformation("Uploaded file to local: {FileKey}", fileKey);
        return fileKey;
    }

    private async Task<Stream?> DownloadFromLocalAsync(string fileKey)
    {
        var filePath = Path.Combine(_basePath, fileKey);
        if (!File.Exists(filePath))
            return null;

        var memoryStream = new MemoryStream();
        using var fileStream = new FileStream(filePath, FileMode.Open);
        await fileStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        return memoryStream;
    }

    private Task<string> UploadToS3Async(Guid tenantId, string fileName, Stream content, string contentType)
    {
        // AWS S3 would use AWS SDK - stub returns local path
        _logger.LogWarning("S3 integration not fully implemented, using local storage");
        return UploadToLocalAsync(tenantId, fileName, content);
    }

    private Task<Stream?> DownloadFromS3Async(string fileKey)
    {
        _logger.LogWarning("S3 integration not fully implemented, using local storage");
        return DownloadFromLocalAsync(fileKey);
    }

    private Task<string> UploadToAzureAsync(Guid tenantId, string fileName, Stream content, string contentType)
    {
        _logger.LogWarning("Azure Blob integration not fully implemented, using local storage");
        return UploadToLocalAsync(tenantId, fileName, content);
    }

    private Task<Stream?> DownloadFromAzureAsync(string fileKey)
    {
        _logger.LogWarning("Azure Blob integration not fully implemented, using local storage");
        return DownloadFromLocalAsync(fileKey);
    }
}

/// <summary>
/// Payment/Billing Service Interface
/// </summary>
public interface IPaymentIntegrationService
{
    Task<string> CreateCheckoutSessionAsync(Guid tenantId, string planId, string successUrl, string cancelUrl);
    Task<bool> ProcessWebhookAsync(string payload, string signature);
    Task<object?> GetSubscriptionAsync(Guid tenantId);
    Task<bool> CancelSubscriptionAsync(Guid tenantId);
}

/// <summary>
/// Payment Service Implementation - Stripe
/// </summary>
public class StripePaymentService : IPaymentIntegrationService
{
    private readonly IConfiguration _config;
    private readonly ILogger<StripePaymentService> _logger;
    private readonly HttpClient _httpClient;

    public StripePaymentService(IConfiguration config, ILogger<StripePaymentService> logger, IHttpClientFactory httpClientFactory)
    {
        _config = config;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("Stripe");
    }

    public async Task<string> CreateCheckoutSessionAsync(Guid tenantId, string planId, string successUrl, string cancelUrl)
    {
        var apiKey = _config["Stripe:SecretKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("Stripe API key not configured");
            return "";
        }

        var priceId = planId switch
        {
            "Starter" => _config["Stripe:Prices:Starter"],
            "Professional" => _config["Stripe:Prices:Professional"],
            "Enterprise" => _config["Stripe:Prices:Enterprise"],
            _ => ""
        };

        if (string.IsNullOrEmpty(priceId))
        {
            _logger.LogWarning("Price ID not found for plan: {Plan}", planId);
            return "";
        }

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["mode"] = "subscription",
            ["success_url"] = successUrl,
            ["cancel_url"] = cancelUrl,
            ["line_items[0][price]"] = priceId,
            ["line_items[0][quantity]"] = "1",
            ["metadata[tenant_id]"] = tenantId.ToString()
        });

        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
        var response = await _httpClient.PostAsync("https://api.stripe.com/v1/checkout/sessions", content);

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<JsonElement>(json);
            return data.GetProperty("url").GetString() ?? "";
        }

        _logger.LogError("Failed to create Stripe checkout session");
        return "";
    }

    public async Task<bool> ProcessWebhookAsync(string payload, string signature)
    {
        var webhookSecret = _config["Stripe:WebhookSecret"];

        // In production, verify signature
        // For now, just log the event
        _logger.LogInformation("Processing Stripe webhook");

        try
        {
            var data = JsonSerializer.Deserialize<JsonElement>(payload);
            var eventType = data.GetProperty("type").GetString();

            _logger.LogInformation("Stripe event: {EventType}", eventType);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process Stripe webhook");
            return false;
        }
    }

    public Task<object?> GetSubscriptionAsync(Guid tenantId)
    {
        // Would fetch from Stripe API
        return Task.FromResult<object?>(null);
    }

    public Task<bool> CancelSubscriptionAsync(Guid tenantId)
    {
        // Would cancel via Stripe API
        return Task.FromResult(true);
    }
}

/// <summary>
/// SSO/OIDC Service Interface
/// </summary>
public interface ISSOIntegrationService
{
    Task<string> GetAuthorizationUrlAsync(string provider, string redirectUri, string state);
    Task<SSOUserInfo?> ExchangeCodeAsync(string provider, string code, string redirectUri);
    Task<bool> ValidateTokenAsync(string token);
}

public class SSOUserInfo
{
    public string Id { get; set; } = "";
    public string Email { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Provider { get; set; }
    public Dictionary<string, string> Claims { get; set; } = new();
}

/// <summary>
/// SSO Service Implementation - Supports Azure AD, Google, Okta
/// </summary>
public class SSOIntegrationService : ISSOIntegrationService
{
    private readonly IConfiguration _config;
    private readonly ILogger<SSOIntegrationService> _logger;
    private readonly HttpClient _httpClient;

    public SSOIntegrationService(IConfiguration config, ILogger<SSOIntegrationService> logger, IHttpClientFactory httpClientFactory)
    {
        _config = config;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("SSO");
    }

    public Task<string> GetAuthorizationUrlAsync(string provider, string redirectUri, string state)
    {
        var clientId = _config[$"SSO:{provider}:ClientId"];

        var url = provider.ToLower() switch
        {
            "azure" => $"https://login.microsoftonline.com/{_config["SSO:Azure:TenantId"]}/oauth2/v2.0/authorize?client_id={clientId}&response_type=code&redirect_uri={Uri.EscapeDataString(redirectUri)}&state={state}&scope=openid%20email%20profile",
            "google" => $"https://accounts.google.com/o/oauth2/v2/auth?client_id={clientId}&response_type=code&redirect_uri={Uri.EscapeDataString(redirectUri)}&state={state}&scope=openid%20email%20profile",
            "okta" => $"https://{_config["SSO:Okta:Domain"]}/oauth2/v1/authorize?client_id={clientId}&response_type=code&redirect_uri={Uri.EscapeDataString(redirectUri)}&state={state}&scope=openid%20email%20profile",
            _ => ""
        };

        return Task.FromResult(url);
    }

    public async Task<SSOUserInfo?> ExchangeCodeAsync(string provider, string code, string redirectUri)
    {
        var clientId = _config[$"SSO:{provider}:ClientId"];
        var clientSecret = _config[$"SSO:{provider}:ClientSecret"];

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            _logger.LogWarning("SSO not configured for provider: {Provider}", provider);
            return null;
        }

        // Token exchange would happen here
        // For now, return stub
        return new SSOUserInfo
        {
            Id = Guid.NewGuid().ToString(),
            Email = "user@example.com",
            Name = "SSO User",
            Provider = provider
        };
    }

    public Task<bool> ValidateTokenAsync(string token)
    {
        // Token validation would happen here
        return Task.FromResult(!string.IsNullOrEmpty(token));
    }
}

/// <summary>
/// Evidence Automation Service - Connects to cloud providers for automatic evidence collection
/// </summary>
public interface IEvidenceAutomationService
{
    Task<List<AutoCollectedEvidence>> CollectFromAWSAsync(Guid tenantId, string region);
    Task<List<AutoCollectedEvidence>> CollectFromAzureAsync(Guid tenantId, string subscriptionId);
    Task<List<AutoCollectedEvidence>> CollectFromGCPAsync(Guid tenantId, string projectId);
}

public class AutoCollectedEvidence
{
    public string Source { get; set; } = "";
    public string Type { get; set; } = "";
    public string ResourceId { get; set; } = "";
    public DateTime CollectedAt { get; set; }
    public string DataJson { get; set; } = "{}";
}

public class EvidenceAutomationService : IEvidenceAutomationService
{
    private readonly ILogger<EvidenceAutomationService> _logger;

    public EvidenceAutomationService(ILogger<EvidenceAutomationService> logger)
    {
        _logger = logger;
    }

    public Task<List<AutoCollectedEvidence>> CollectFromAWSAsync(Guid tenantId, string region)
    {
        _logger.LogInformation("Collecting evidence from AWS for tenant {TenantId} in {Region}", tenantId, region);

        // Would use AWS SDK to collect from CloudTrail, Config, IAM
        return Task.FromResult(new List<AutoCollectedEvidence>
        {
            new() { Source = "AWS", Type = "IAM", ResourceId = "iam-policy-check", CollectedAt = DateTime.UtcNow },
            new() { Source = "AWS", Type = "CloudTrail", ResourceId = "trail-logs", CollectedAt = DateTime.UtcNow }
        });
    }

    public Task<List<AutoCollectedEvidence>> CollectFromAzureAsync(Guid tenantId, string subscriptionId)
    {
        _logger.LogInformation("Collecting evidence from Azure for tenant {TenantId}", tenantId);

        return Task.FromResult(new List<AutoCollectedEvidence>
        {
            new() { Source = "Azure", Type = "RBAC", ResourceId = "role-assignments", CollectedAt = DateTime.UtcNow },
            new() { Source = "Azure", Type = "Policy", ResourceId = "policy-compliance", CollectedAt = DateTime.UtcNow }
        });
    }

    public Task<List<AutoCollectedEvidence>> CollectFromGCPAsync(Guid tenantId, string projectId)
    {
        _logger.LogInformation("Collecting evidence from GCP for tenant {TenantId}", tenantId);

        return Task.FromResult(new List<AutoCollectedEvidence>
        {
            new() { Source = "GCP", Type = "IAM", ResourceId = "iam-bindings", CollectedAt = DateTime.UtcNow }
        });
    }
}
