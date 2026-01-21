using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GrcMvc.Data;
using GrcMvc.Services.Interfaces;
using System.Text.Json;

using Microsoft.Extensions.Localization;
using GrcMvc.Resources;
namespace GrcMvc.Controllers.Api;

/// <summary>
/// Diagnostics API Controller
/// Collects error logs and visitor analytics for compliance monitoring
/// </summary>
[ApiController]
[Route("api/diagnostics")]
public class DiagnosticsController : ControllerBase
{
    private readonly GrcDbContext _context;
    private readonly ILogger<DiagnosticsController> _logger;
    private readonly IEmailService _emailService;
    private static readonly List<VisitorLog> _visitorLogs = new();
    private static readonly List<ErrorLog> _errorLogs = new();
    private static readonly object _lock = new();

    public DiagnosticsController(
        GrcDbContext context,
        ILogger<DiagnosticsController> logger,
        IEmailService emailService)
    {
        _context = context;
        _logger = logger;
        _emailService = emailService;
    }

    /// <summary>
    /// Log a visitor event (page view, action, etc.)
    /// </summary>
    [HttpPost("visitor")]
    [AllowAnonymous]
    public IActionResult LogVisitor([FromBody] VisitorLogRequest request)
    {
        try
        {
            var log = new VisitorLog
            {
                Id = Guid.NewGuid(),
                SessionId = request.SessionId ?? Guid.NewGuid().ToString(),
                Page = request.Page,
                Action = request.Action,
                UserAgent = Request.Headers["User-Agent"].ToString(),
                IpAddress = GetClientIp(),
                Referrer = Request.Headers["Referer"].ToString(),
                Timestamp = DateTime.UtcNow,
                Metadata = request.Metadata
            };

            lock (_lock)
            {
                _visitorLogs.Add(log);
                // Keep only last 10000 entries
                if (_visitorLogs.Count > 10000)
                {
                    _visitorLogs.RemoveRange(0, _visitorLogs.Count - 10000);
                }
            }

            _logger.LogInformation("Visitor: {Page} - {Action} - {Ip}", 
                request.Page, request.Action, log.IpAddress);

            return Ok(new { success = true, logId = log.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging visitor event");
            return Ok(new { success = false });
        }
    }

    /// <summary>
    /// Log a client-side error
    /// </summary>
    [HttpPost("error")]
    [AllowAnonymous]
    public IActionResult LogError([FromBody] ErrorLogRequest request)
    {
        try
        {
            var log = new ErrorLog
            {
                Id = Guid.NewGuid(),
                SessionId = request.SessionId,
                Page = request.Page,
                ErrorMessage = request.ErrorMessage,
                StackTrace = request.StackTrace,
                ErrorType = request.ErrorType ?? "UnknownError",
                UserAgent = Request.Headers["User-Agent"].ToString(),
                IpAddress = GetClientIp(),
                Timestamp = DateTime.UtcNow,
                Metadata = request.Metadata
            };

            lock (_lock)
            {
                _errorLogs.Add(log);
                // Keep only last 5000 error entries
                if (_errorLogs.Count > 5000)
                {
                    _errorLogs.RemoveRange(0, _errorLogs.Count - 5000);
                }
            }

            _logger.LogWarning("Client Error on {Page}: {Error}", 
                request.Page, request.ErrorMessage);

            return Ok(new { success = true, errorId = log.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging client error");
            return Ok(new { success = false });
        }
    }

    /// <summary>
    /// Get visitor analytics summary (admin only)
    /// </summary>
    [HttpGet("analytics")]
    [Authorize(Roles = "PlatformAdmin,Admin")]
    public IActionResult GetAnalytics([FromQuery] int hours = 24)
    {
        var since = DateTime.UtcNow.AddHours(-hours);

        lock (_lock)
        {
            var recentVisitors = _visitorLogs.Where(v => v.Timestamp >= since).ToList();
            var recentErrors = _errorLogs.Where(e => e.Timestamp >= since).ToList();

            var analytics = new
            {
                timeRange = $"Last {hours} hours",
                visitors = new
                {
                    total = recentVisitors.Count,
                    uniqueSessions = recentVisitors.Select(v => v.SessionId).Distinct().Count(),
                    topPages = recentVisitors
                        .GroupBy(v => v.Page)
                        .OrderByDescending(g => g.Count())
                        .Take(10)
                        .Select(g => new { page = g.Key, views = g.Count() }),
                    topActions = recentVisitors
                        .Where(v => !string.IsNullOrEmpty(v.Action))
                        .GroupBy(v => v.Action)
                        .OrderByDescending(g => g.Count())
                        .Take(10)
                        .Select(g => new { action = g.Key, count = g.Count() })
                },
                errors = new
                {
                    total = recentErrors.Count,
                    byType = recentErrors
                        .GroupBy(e => e.ErrorType)
                        .OrderByDescending(g => g.Count())
                        .Select(g => new { type = g.Key, count = g.Count() }),
                    byPage = recentErrors
                        .GroupBy(e => e.Page)
                        .OrderByDescending(g => g.Count())
                        .Take(10)
                        .Select(g => new { page = g.Key, count = g.Count() }),
                    recent = recentErrors
                        .OrderByDescending(e => e.Timestamp)
                        .Take(20)
                        .Select(e => new
                        {
                            id = e.Id,
                            page = e.Page,
                            error = e.ErrorMessage?.Substring(0, Math.Min(100, e.ErrorMessage?.Length ?? 0)),
                            type = e.ErrorType,
                            timestamp = e.Timestamp
                        })
                }
            };

            return Ok(analytics);
        }
    }

    /// <summary>
    /// Get detailed error log (admin only)
    /// </summary>
    [HttpGet("error/{id:guid}")]
    [Authorize(Roles = "PlatformAdmin,Admin")]
    public IActionResult GetError(Guid id)
    {
        lock (_lock)
        {
            var error = _errorLogs.FirstOrDefault(e => e.Id == id);
            if (error == null) return NotFound();
            return Ok(error);
        }
    }

    /// <summary>
    /// Get all errors for export (admin only)
    /// </summary>
    [HttpGet("errors/export")]
    [Authorize(Roles = "PlatformAdmin,Admin")]
    public IActionResult ExportErrors([FromQuery] int hours = 24)
    {
        var since = DateTime.UtcNow.AddHours(-hours);

        lock (_lock)
        {
            var errors = _errorLogs
                .Where(e => e.Timestamp >= since)
                .OrderByDescending(e => e.Timestamp)
                .ToList();

            return Ok(errors);
        }
    }

    /// <summary>
    /// Health check for diagnostics service
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health()
    {
        lock (_lock)
        {
            return Ok(new
            {
                status = "Healthy",
                timestamp = DateTime.UtcNow,
                statistics = new
                {
                    visitorLogsCount = _visitorLogs.Count,
                    errorLogsCount = _errorLogs.Count,
                    oldestVisitorLog = _visitorLogs.FirstOrDefault()?.Timestamp,
                    newestVisitorLog = _visitorLogs.LastOrDefault()?.Timestamp,
                    oldestErrorLog = _errorLogs.FirstOrDefault()?.Timestamp,
                    newestErrorLog = _errorLogs.LastOrDefault()?.Timestamp
                }
            });
        }
    }

    /// <summary>
    /// Test email sending with Office 365 OAuth2
    /// </summary>
    [HttpPost("test-email")]
    [Authorize(Roles = "PlatformAdmin,Admin")]
    public async Task<IActionResult> TestEmail([FromBody] TestEmailRequest request)
    {
        try
        {
            var toEmail = request.ToEmail ?? "info@shahin-ai.com";
            
            await _emailService.SendEmailAsync(
                toEmail,
                "🔧 Shahin AI - اختبار الإيميل",
                $@"<div style='font-family: Arial, sans-serif; direction: rtl; text-align: right;'>
                    <h2 style='color: #0066cc;'>✅ تم إرسال الإيميل بنجاح!</h2>
                    <p>هذا إيميل اختبار من نظام <strong>Shahin AI GRC</strong></p>
                    <hr style='border: 1px solid #eee;'/>
                    <p><strong>التاريخ:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>
                    <p><strong>الخادم:</strong> {Environment.MachineName}</p>
                    <p><strong>طريقة المصادقة:</strong> Office 365 OAuth2</p>
                    <hr style='border: 1px solid #eee;'/>
                    <p style='color: #666; font-size: 12px;'>
                        Powered by <a href='https://www.doganconsult.com'>Dogan Consult</a>
                    </p>
                </div>");

            _logger.LogInformation("Test email sent successfully to {Email}", toEmail);
            
            return Ok(new
            {
                success = true,
                message = $"Email sent successfully to {toEmail}",
                timestamp = DateTime.UtcNow,
                authMethod = "Office 365 OAuth2"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send test email");
            
            return Ok(new
            {
                success = false,
                error = "An error occurred.",
                innerError = ex.InnerException?.Message,
                hint = "تأكد من إضافة صلاحية Mail.Send في Azure Portal وتفعيل Grant admin consent",
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Check email configuration status
    /// </summary>
    [HttpGet("email-config")]
    [Authorize(Roles = "PlatformAdmin,Admin")]
    public IActionResult GetEmailConfig()
    {
        var config = HttpContext.RequestServices.GetService<Microsoft.Extensions.Options.IOptions<SmtpSettings>>();
        var settings = config?.Value;

        if (settings == null)
        {
            return Ok(new { configured = false, error = "SmtpSettings not found" });
        }

        return Ok(new
        {
            configured = true,
            host = settings.Host,
            port = settings.Port,
            enableSsl = settings.EnableSsl,
            fromEmail = settings.FromEmail,
            fromName = settings.FromName,
            username = settings.Username,
            useOAuth2 = settings.UseOAuth2,
            tenantIdConfigured = !string.IsNullOrEmpty(settings.TenantId),
            clientIdConfigured = !string.IsNullOrEmpty(settings.ClientId),
            clientSecretConfigured = !string.IsNullOrEmpty(settings.ClientSecret),
            passwordConfigured = !string.IsNullOrEmpty(settings.Password)
        });
    }

    /// <summary>
    /// Test AI agents configuration and connectivity
    /// </summary>
    [HttpPost("test-ai-agents")]
    [Authorize(Roles = "PlatformAdmin")]
    public async Task<IActionResult> TestAiAgents([FromBody] TestAiAgentsRequest? request)
    {
        var config = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var results = new List<object>();

        // Check Claude Agent configuration
        var claudeEnabled = config.GetValue<bool>("ClaudeAgents:Enabled", false);
        var claudeApiKey = config.GetValue<string>("ClaudeAgents:ApiKey") ?? "";
        var claudeModel = config.GetValue<string>("ClaudeAgents:Model") ?? "claude-sonnet-4-20250514";

        var claudeResult = new
        {
            agentName = "Claude AI Agent",
            enabled = claudeEnabled,
            model = claudeModel,
            apiKeyConfigured = !string.IsNullOrEmpty(claudeApiKey),
            status = !claudeEnabled ? "Disabled" 
                   : string.IsNullOrEmpty(claudeApiKey) ? "Error - No API Key" 
                   : "Configured",
            message = !claudeEnabled ? "Set ClaudeAgents:Enabled=true to enable"
                    : string.IsNullOrEmpty(claudeApiKey) ? "Set CLAUDE_API_KEY environment variable"
                    : "API key configured"
        };
        results.Add(claudeResult);

        // Check Copilot Agent configuration
        var copilotEnabled = config.GetValue<bool>("CopilotAgent:Enabled", false);
        var copilotClientId = config.GetValue<string>("CopilotAgent:ClientId") ?? "";

        var copilotResult = new
        {
            agentName = "Microsoft Copilot Agent",
            enabled = copilotEnabled,
            clientIdConfigured = !string.IsNullOrEmpty(copilotClientId),
            status = !copilotEnabled ? "Disabled"
                   : string.IsNullOrEmpty(copilotClientId) ? "Error - No Client ID"
                   : "Configured",
            message = !copilotEnabled ? "Set CopilotAgent:Enabled=true to enable"
                    : string.IsNullOrEmpty(copilotClientId) ? "Configure CopilotAgent:ClientId"
                    : "Client ID configured"
        };
        results.Add(copilotResult);

        // Run live test if requested
        string? liveTestResult = null;
        if (request?.RunLiveTest == true && claudeEnabled && !string.IsNullOrEmpty(claudeApiKey))
        {
            try
            {
                var claudeService = HttpContext.RequestServices.GetService<IClaudeAgentService>();
                if (claudeService != null)
                {
                    var testPrompt = request.TestPrompt ?? "Respond with only the word 'success' in Arabic and English.";
                    var chatResponse = await claudeService.ChatAsync(testPrompt);
                    liveTestResult = chatResponse.Response.Length > 500 ? chatResponse.Response.Substring(0, 500) + "..." : chatResponse.Response;
                }
                else
                {
                    liveTestResult = "Error: IClaudeAgentService not registered in DI";
                }
            }
            catch (Exception ex)
            {
                liveTestResult = $"Error: {ex.Message}";
            }
        }

        return Ok(new
        {
            success = true,
            testedAt = DateTime.UtcNow,
            agents = results,
            liveTest = request?.RunLiveTest == true ? new
            {
                requested = true,
                result = liveTestResult
            } : null
        });
    }

    /// <summary>
    /// Bulk test AI agents with multiple prompts
    /// </summary>
    [HttpPost("bulk-test-agents")]
    [Authorize(Roles = "PlatformAdmin")]
    public async Task<IActionResult> BulkTestAgents([FromBody] BulkAgentTestRequest request)
    {
        if (request.Prompts == null || request.Prompts.Count == 0)
        {
            return BadRequest(new { success = false, message = "At least one prompt is required" });
        }

        var config = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var claudeEnabled = config.GetValue<bool>("ClaudeAgents:Enabled", false);
        var claudeApiKey = config.GetValue<string>("ClaudeAgents:ApiKey") ?? "";

        if (!claudeEnabled || string.IsNullOrEmpty(claudeApiKey))
        {
            return Ok(new
            {
                success = false,
                message = "Claude agent is not configured. Set ClaudeAgents:Enabled=true and CLAUDE_API_KEY environment variable."
            });
        }

        var claudeService = HttpContext.RequestServices.GetService<IClaudeAgentService>();
        if (claudeService == null)
        {
            return Ok(new
            {
                success = false,
                message = "IClaudeAgentService not registered in DI"
            });
        }

        var results = new List<object>();
        var startTime = DateTime.UtcNow;

        foreach (var prompt in request.Prompts.Take(10)) // Limit to 10 prompts
        {
            var promptStart = DateTime.UtcNow;
            try
            {
                var chatResponse = await claudeService.ChatAsync(prompt);
                results.Add(new
                {
                    prompt = prompt.Length > 100 ? prompt.Substring(0, 100) + "..." : prompt,
                    success = true,
                    response = chatResponse.Response.Length > 500 ? chatResponse.Response.Substring(0, 500) + "..." : chatResponse.Response,
                    responseTimeMs = (int)(DateTime.UtcNow - promptStart).TotalMilliseconds
                });
            }
            catch (Exception ex)
            {
                results.Add(new
                {
                    prompt = prompt.Length > 100 ? prompt.Substring(0, 100) + "..." : prompt,
                    success = false,
                    error = ex.Message,
                    responseTimeMs = (int)(DateTime.UtcNow - promptStart).TotalMilliseconds
                });
            }
        }

        return Ok(new
        {
            success = true,
            testedAt = DateTime.UtcNow,
            totalPrompts = results.Count,
            successfulTests = results.Count(r => ((dynamic)r).success),
            totalTimeMs = (int)(DateTime.UtcNow - startTime).TotalMilliseconds,
            results
        });
    }

    private string GetClientIp()
    {
        var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}

#region Request/Response Models

public class VisitorLogRequest
{
    public string? SessionId { get; set; }
    public string Page { get; set; } = string.Empty;
    public string? Action { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class ErrorLogRequest
{
    public string? SessionId { get; set; }
    public string Page { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public string? ErrorType { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class VisitorLog
{
    public Guid Id { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public string Page { get; set; } = string.Empty;
    public string? Action { get; set; }
    public string? UserAgent { get; set; }
    public string? IpAddress { get; set; }
    public string? Referrer { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class ErrorLog
{
    public Guid Id { get; set; }
    public string? SessionId { get; set; }
    public string Page { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public string? ErrorType { get; set; }
    public string? UserAgent { get; set; }
    public string? IpAddress { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class TestEmailRequest
{
    public string? ToEmail { get; set; }
}

public class TestAiAgentsRequest
{
    public bool RunLiveTest { get; set; } = false;
    public string? TestPrompt { get; set; }
}

public class BulkAgentTestRequest
{
    public List<string> Prompts { get; set; } = new();
}

#endregion

// Localization diagnostic endpoint - temporary for debugging
