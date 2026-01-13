using GrcMvc.Configuration;
using GrcMvc.Data;
using GrcMvc.Services.EmailOperations;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace GrcMvc.Controllers.Api;

/// <summary>
/// Integration Test Controller
/// Tests Email, AI Agents, and Azure Services
/// </summary>
[ApiController]
[Route("api/integration-test")]
[Authorize(Roles = "PlatformAdmin,Admin")]
public class IntegrationTestController : ControllerBase
{
    private readonly IClaudeAgentService _claudeAgent;
    private readonly IEmailService _emailService;
    private readonly IMicrosoftGraphEmailService _graphEmailService;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<IntegrationTestController> _logger;
    private readonly ClaudeApiSettings _claudeSettings;

    public IntegrationTestController(
        IClaudeAgentService claudeAgent,
        IEmailService emailService,
        IMicrosoftGraphEmailService graphEmailService,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILogger<IntegrationTestController> logger,
        IOptions<ClaudeApiSettings> claudeSettings)
    {
        _claudeAgent = claudeAgent;
        _emailService = emailService;
        _graphEmailService = graphEmailService;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _claudeSettings = claudeSettings.Value;
    }

    /// <summary>
    /// Get status of all integrations
    /// </summary>
    [HttpGet("status")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllStatus()
    {
        var results = new Dictionary<string, object>();

        // 1. Claude AI Status
        var claudeAvailable = await _claudeAgent.IsAvailableAsync();
        results["claudeAi"] = new
        {
            configured = claudeAvailable,
            apiKeySet = !string.IsNullOrEmpty(_claudeSettings.ApiKey),
            model = _claudeSettings.Model ?? "Not set",
            endpoint = _claudeSettings.ApiEndpoint ?? "Default"
        };

        // 2. Email/SMTP Status
        var smtpServer = _configuration["SmtpSettings:Host"] ?? _configuration["SMTP_SERVER"];
        results["email"] = new
        {
            smtpConfigured = !string.IsNullOrEmpty(smtpServer),
            smtpServer = smtpServer ?? "Not configured",
            smtpPort = _configuration["SmtpSettings:Port"] ?? _configuration["SMTP_PORT"] ?? "587"
        };

        // 3. Microsoft Graph Status
        var tenantId = _configuration["AZURE_TENANT_ID"] ?? _configuration["MicrosoftGraph:TenantId"];
        var clientId = _configuration["AZURE_CLIENT_ID"] ?? _configuration["MicrosoftGraph:ClientId"];
        results["microsoftGraph"] = new
        {
            configured = !string.IsNullOrEmpty(tenantId) && !string.IsNullOrEmpty(clientId),
            tenantId = !string.IsNullOrEmpty(tenantId) ? $"{tenantId.Substring(0, 8)}..." : "Not set",
            clientId = !string.IsNullOrEmpty(clientId) ? $"{clientId.Substring(0, 8)}..." : "Not set"
        };

        // 4. Azure Bot Service Status
        var botSecretKey = _configuration["BOT_SECRET_KEY"];
        results["azureBotService"] = new
        {
            configured = !string.IsNullOrEmpty(botSecretKey),
            botName = _configuration["BOT_SERVICE_NAME"] ?? "Not set"
        };

        // 5. Copilot Agent Status
        var copilotClientId = _configuration["COPILOT_CLIENT_ID"] ?? _configuration["CopilotAgent:ClientId"];
        results["copilotAgent"] = new
        {
            enabled = _configuration.GetValue<bool>("CopilotAgent:Enabled"),
            configured = !string.IsNullOrEmpty(copilotClientId)
        };

        return Ok(new
        {
            timestamp = DateTime.UtcNow,
            integrations = results
        });
    }

    /// <summary>
    /// Test Claude AI Agent
    /// </summary>
    [HttpPost("test-claude")]
    public async Task<IActionResult> TestClaudeAgent([FromBody] IntegrationTestClaudeRequest request)
    {
        try
        {
            var isAvailable = await _claudeAgent.IsAvailableAsync();
            if (!isAvailable)
            {
                return Ok(new
                {
                    success = false,
                    message = "Claude AI is not configured. Please set CLAUDE_API_KEY in .env"
                });
            }

            // Test with a simple compliance analysis
            var result = await _claudeAgent.AnalyzeComplianceAsync(
                request.FrameworkCode ?? "NCA-ECC",
                cancellationToken: HttpContext.RequestAborted);

            return Ok(new
            {
                success = result.Success,
                message = result.Success ? "Claude AI is working!" : "Claude AI returned error",
                summary = result.Summary,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing Claude AI");
            return Ok(new
            {
                success = false,
                message = $"Error: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Test Email (SMTP)
    /// </summary>
    [HttpPost("test-email")]
    public async Task<IActionResult> TestEmail([FromBody] IntegrationTestEmailRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.ToEmail))
            {
                return BadRequest(new { success = false, message = "ToEmail is required" });
            }

            await _emailService.SendEmailAsync(
                request.ToEmail,
                request.Subject ?? "Shahin AI - Test Email",
                request.Body ?? $"<h1>Test Email from Shahin AI GRC</h1><p>This is a test email sent at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p><p>If you received this, your email configuration is working correctly!</p>");

            return Ok(new
            {
                success = true,
                message = $"Test email sent to {request.ToEmail}",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending test email to {Email}", request.ToEmail);
            return Ok(new
            {
                success = false,
                message = $"Failed to send email: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Test Microsoft Graph Email
    /// </summary>
    [HttpPost("test-graph-email")]
    public async Task<IActionResult> TestGraphEmail([FromBody] IntegrationTestGraphEmailRequest request)
    {
        try
        {
            var tenantId = _configuration["AZURE_TENANT_ID"] ?? _configuration["MicrosoftGraph:TenantId"];
            var clientId = _configuration["AZURE_CLIENT_ID"] ?? _configuration["MicrosoftGraph:ClientId"];
            var clientSecret = _configuration["AZURE_CLIENT_SECRET"] ?? _configuration["MicrosoftGraph:ClientSecret"];

            if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                return Ok(new
                {
                    success = false,
                    message = "Microsoft Graph is not fully configured. Check AZURE_TENANT_ID, AZURE_CLIENT_ID, AZURE_CLIENT_SECRET"
                });
            }

            // Get access token
            var accessToken = await _graphEmailService.GetAccessTokenAsync(tenantId, clientId, clientSecret);

            return Ok(new
            {
                success = true,
                message = "Microsoft Graph authentication successful!",
                tokenLength = accessToken?.Length ?? 0,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing Microsoft Graph");
            return Ok(new
            {
                success = false,
                message = $"Microsoft Graph error: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Test Azure Bot Service
    /// </summary>
    [HttpPost("test-bot-service")]
    public async Task<IActionResult> TestBotService()
    {
        try
        {
            var botSecretKey = _configuration["BOT_SECRET_KEY"];
            var botName = _configuration["BOT_SERVICE_NAME"] ?? "Shahin-ai";

            if (string.IsNullOrEmpty(botSecretKey))
            {
                return Ok(new
                {
                    success = false,
                    message = "Azure Bot Service is not configured. Set BOT_SECRET_KEY in .env"
                });
            }

            // Try to get DirectLine token
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", botSecretKey);

            var response = await client.PostAsync(
                "https://directline.botframework.com/v3/directline/tokens/generate",
                new StringContent("{}", Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return Ok(new
                {
                    success = true,
                    message = "Azure Bot Service is connected!",
                    botName = botName,
                    timestamp = DateTime.UtcNow
                });
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                return Ok(new
                {
                    success = false,
                    message = $"Bot Service returned {response.StatusCode}",
                    details = error
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing Azure Bot Service");
            return Ok(new
            {
                success = false,
                message = $"Error: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Test Copilot Agent
    /// </summary>
    [HttpPost("test-copilot")]
    public async Task<IActionResult> TestCopilotAgent()
    {
        try
        {
            var tenantId = _configuration["AZURE_TENANT_ID"] ?? _configuration["CopilotAgent:TenantId"];
            var clientId = _configuration["COPILOT_CLIENT_ID"] ?? _configuration["CopilotAgent:ClientId"];
            var clientSecret = _configuration["COPILOT_CLIENT_SECRET"] ?? _configuration["CopilotAgent:ClientSecret"];

            if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                return Ok(new
                {
                    success = false,
                    message = "Copilot Agent is not configured. Check COPILOT_CLIENT_ID, COPILOT_CLIENT_SECRET"
                });
            }

            // Test OAuth token acquisition
            var client = _httpClientFactory.CreateClient();
            var tokenUrl = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["scope"] = "https://graph.microsoft.com/.default",
                ["grant_type"] = "client_credentials"
            });

            var response = await client.PostAsync(tokenUrl, content);

            if (response.IsSuccessStatusCode)
            {
                return Ok(new
                {
                    success = true,
                    message = "Copilot Agent authentication successful!",
                    timestamp = DateTime.UtcNow
                });
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                return Ok(new
                {
                    success = false,
                    message = $"Copilot authentication failed: {response.StatusCode}",
                    details = error
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing Copilot Agent");
            return Ok(new
            {
                success = false,
                message = $"Error: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Run all integration tests
    /// </summary>
    [HttpPost("run-all")]
    public async Task<IActionResult> RunAllTests([FromBody] IntegrationRunAllTestsRequest? request)
    {
        var results = new Dictionary<string, object>();

        // 1. Claude AI Test
        try
        {
            var claudeAvailable = await _claudeAgent.IsAvailableAsync();
            results["claudeAi"] = new { success = claudeAvailable, message = claudeAvailable ? "Configured" : "Not configured" };
        }
        catch (Exception ex)
        {
            results["claudeAi"] = new { success = false, message = ex.Message };
        }

        // 2. Microsoft Graph Test
        try
        {
            var tenantId = _configuration["AZURE_TENANT_ID"];
            var clientId = _configuration["AZURE_CLIENT_ID"];
            var clientSecret = _configuration["AZURE_CLIENT_SECRET"];

            if (!string.IsNullOrEmpty(tenantId) && !string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret))
            {
                var token = await _graphEmailService.GetAccessTokenAsync(tenantId, clientId, clientSecret);
                results["microsoftGraph"] = new { success = true, message = "Authentication OK" };
            }
            else
            {
                results["microsoftGraph"] = new { success = false, message = "Not configured" };
            }
        }
        catch (Exception ex)
        {
            results["microsoftGraph"] = new { success = false, message = ex.Message };
        }

        // 3. Bot Service Test
        try
        {
            var botSecretKey = _configuration["BOT_SECRET_KEY"];
            if (!string.IsNullOrEmpty(botSecretKey))
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", botSecretKey);
                var response = await client.PostAsync(
                    "https://directline.botframework.com/v3/directline/tokens/generate",
                    new StringContent("{}", Encoding.UTF8, "application/json"));
                results["botService"] = new { success = response.IsSuccessStatusCode, message = response.IsSuccessStatusCode ? "Connected" : $"Error: {response.StatusCode}" };
            }
            else
            {
                results["botService"] = new { success = false, message = "Not configured" };
            }
        }
        catch (Exception ex)
        {
            results["botService"] = new { success = false, message = ex.Message };
        }

        // 4. Copilot Test
        try
        {
            var copilotId = _configuration["COPILOT_CLIENT_ID"];
            var copilotSecret = _configuration["COPILOT_CLIENT_SECRET"];
            results["copilotAgent"] = new
            {
                success = !string.IsNullOrEmpty(copilotId) && !string.IsNullOrEmpty(copilotSecret),
                message = !string.IsNullOrEmpty(copilotId) ? "Configured" : "Not configured"
            };
        }
        catch (Exception ex)
        {
            results["copilotAgent"] = new { success = false, message = ex.Message };
        }

        // 5. Email Test (if email provided)
        if (!string.IsNullOrEmpty(request?.TestEmail))
        {
            try
            {
                await _emailService.SendEmailAsync(
                    request.TestEmail,
                    "Shahin AI - Integration Test",
                    "<h1>Integration Test Successful</h1><p>All systems are operational!</p>");
                results["email"] = new { success = true, message = $"Email sent to {request.TestEmail}" };
            }
            catch (Exception ex)
            {
                results["email"] = new { success = false, message = ex.Message };
            }
        }
        else
        {
            results["email"] = new { success = false, message = "Skipped - no test email provided" };
        }

        var allSuccess = results.Values.Cast<dynamic>().All(r => r.success == true);

        return Ok(new
        {
            timestamp = DateTime.UtcNow,
            allPassed = allSuccess,
            results = results
        });
    }
}

// Request DTOs for Integration Tests
public class IntegrationTestClaudeRequest
{
    public string? FrameworkCode { get; set; }
    public string? Prompt { get; set; }
}

public class IntegrationTestEmailRequest
{
    public string ToEmail { get; set; } = "";
    public string? Subject { get; set; }
    public string? Body { get; set; }
}

public class IntegrationTestGraphEmailRequest
{
    public string? MailboxId { get; set; }
}

public class IntegrationRunAllTestsRequest
{
    public string? TestEmail { get; set; }
}
