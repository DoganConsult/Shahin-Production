using GrcMvc.Agents;
using GrcMvc.BackgroundJobs;
using GrcMvc.Services.Interfaces;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using Microsoft.Extensions.Localization;
using GrcMvc.Resources;
namespace GrcMvc.Controllers.Api;

/// <summary>
/// API Controller for Code Quality Monitoring
/// Provides endpoints for code analysis, alerts, and webhooks
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CodeQualityController : ControllerBase
{
    private readonly ICodeQualityService _codeQualityService;
    private readonly IAlertService _alertService;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly ILogger<CodeQualityController> _logger;

    public CodeQualityController(
        ICodeQualityService codeQualityService,
        IAlertService alertService,
        IBackgroundJobClient backgroundJobClient,
        ILogger<CodeQualityController> logger)
    {
        _codeQualityService = codeQualityService;
        _alertService = alertService;
        _backgroundJobClient = backgroundJobClient;
        _logger = logger;
    }

    /// <summary>
    /// Analyze code snippet
    /// </summary>
    [HttpPost("analyze")]
    [EnableRateLimiting("api")]
    public async Task<ActionResult<CodeAnalysisResult>> AnalyzeCode([FromBody] CodeAnalysisRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            return BadRequest(new { error = "Code is required" });

        try
        {
            var result = await _codeQualityService.AnalyzeCodeAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing code");
            return StatusCode(500, new { error = "Analysis failed", message = ex.Message });
        }
    }

    /// <summary>
    /// Run full scan on code
    /// </summary>
    [HttpPost("scan")]
    [EnableRateLimiting("api")]
    public async Task<ActionResult<List<CodeAnalysisResult>>> FullScan([FromBody] FullScanRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            return BadRequest(new { error = "Code is required" });

        try
        {
            var results = await _codeQualityService.RunFullScanAsync(request.Code, request.FilePath);
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during full scan");
            return StatusCode(500, new { error = "Scan failed", message = ex.Message });
        }
    }

    /// <summary>
    /// Get quality metrics
    /// </summary>
    [HttpGet("metrics")]
    [Authorize(Policy = "ComplianceOfficer")]
    public async Task<ActionResult<CodeQualityMetrics>> GetMetrics([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        try
        {
            var metrics = await _codeQualityService.GetQualityMetricsAsync(from, to);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting metrics");
            return StatusCode(500, new { error = "Failed to get metrics", message = ex.Message });
        }
    }

    /// <summary>
    /// Get alert history
    /// </summary>
    [HttpGet("alerts")]
    [Authorize(Policy = "ComplianceOfficer")]
    public async Task<ActionResult<List<CodeQualityAlert>>> GetAlerts(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? severity)
    {
        try
        {
            var alerts = await _alertService.GetAlertHistoryAsync(from, to, severity);
            return Ok(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting alerts");
            return StatusCode(500, new { error = "Failed to get alerts", message = ex.Message });
        }
    }

    /// <summary>
    /// Get alert configuration
    /// </summary>
    [HttpGet("alerts/config")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<AlertConfiguration>> GetAlertConfig()
    {
        var config = await _alertService.GetConfigurationAsync();
        return Ok(config);
    }

    /// <summary>
    /// Update alert configuration
    /// </summary>
    [HttpPut("alerts/config")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult> UpdateAlertConfig([FromBody] AlertConfiguration config)
    {
        try
        {
            await _alertService.UpdateConfigurationAsync(config);
            return Ok(new { message = "Configuration updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating alert config");
            return StatusCode(500, new { error = "Failed to update configuration", message = ex.Message });
        }
    }

    /// <summary>
    /// Test alert delivery
    /// </summary>
    [HttpPost("alerts/test")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult> TestAlert([FromBody] TestAlertRequest request)
    {
        try
        {
            var alert = new CodeQualityAlert
            {
                Severity = request.Severity ?? "info",
                Title = "Test Alert",
                Message = request.Message ?? "This is a test alert from GRC Code Quality Monitor",
                AgentType = "test",
                Score = 100,
                IssueCount = 0
            };

            var success = await _alertService.SendAlertAsync(alert);
            return success
                ? Ok(new { message = "Test alert sent successfully" })
                : StatusCode(500, new { error = "Failed to send test alert" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending test alert");
            return StatusCode(500, new { error = "Failed to send test alert", message = ex.Message });
        }
    }

    /// <summary>
    /// Trigger manual scan (background job)
    /// </summary>
    [HttpPost("scan/trigger")]
    [Authorize(Policy = "AdminOnly")]
    public ActionResult TriggerScan()
    {
        try
        {
            var jobId = _backgroundJobClient.Enqueue<CodeQualityMonitorJob>(job => job.ExecuteAsync());
            return Accepted(new { jobId, message = "Scan triggered successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering scan");
            return StatusCode(500, new { error = "Failed to trigger scan", message = ex.Message });
        }
    }

    /// <summary>
    /// Trigger full security audit (background job)
    /// </summary>
    [HttpPost("audit/trigger")]
    [Authorize(Policy = "AdminOnly")]
    public ActionResult TriggerSecurityAudit()
    {
        try
        {
            var jobId = _backgroundJobClient.Enqueue<CodeQualityMonitorJob>(job => job.ExecuteFullSecurityAuditAsync());
            return Accepted(new { jobId, message = "Security audit triggered successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering security audit");
            return StatusCode(500, new { error = "Failed to trigger audit", message = ex.Message });
        }
    }

    /// <summary>
    /// Webhook endpoint for CI/CD integration (GitHub, GitLab, etc.)
    /// </summary>
    [HttpPost("webhook/github")]
    [AllowAnonymous]
    public async Task<ActionResult> GitHubWebhook([FromBody] GitHubWebhookPayload payload, [FromHeader(Name = "X-GitHub-Event")] string? eventType)
    {
        _logger.LogInformation("Received GitHub webhook: {EventType}", eventType);

        try
        {
            if (eventType == "push" && payload.Commits?.Any() == true)
            {
                var changedFiles = payload.Commits
                    .SelectMany(c => c.Added.Concat(c.Modified))
                    .Where(f => f.EndsWith(".cs") || f.EndsWith(".razor") || f.EndsWith(".cshtml"))
                    .Distinct()
                    .ToArray();

                if (changedFiles.Any())
                {
                    var commitSha = payload.After ?? "";
                    var branch = payload.Ref?.Replace("refs/heads/", "") ?? "main";
                    _backgroundJobClient.Enqueue<CodeQualityMonitorJob>(
                        job => job.AnalyzeCommitAsync(commitSha, branch, changedFiles));
                }
            }
            else if (eventType == "pull_request" && payload.Action == "opened" || payload.Action == "synchronize")
            {
                // Handle PR events
                _logger.LogInformation("PR event received: {Action}", payload.Action);
            }

            return Ok(new { message = "Webhook processed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GitHub webhook");
            return StatusCode(500, new { error = "Webhook processing failed" });
        }
    }

    /// <summary>
    /// Webhook endpoint for GitLab
    /// </summary>
    [HttpPost("webhook/gitlab")]
    [AllowAnonymous]
    public async Task<ActionResult> GitLabWebhook([FromBody] GitLabWebhookPayload payload, [FromHeader(Name = "X-Gitlab-Event")] string? eventType)
    {
        _logger.LogInformation("Received GitLab webhook: {EventType}", eventType);

        try
        {
            if (eventType == "Push Hook" && payload.Commits?.Any() == true)
            {
                var changedFiles = payload.Commits
                    .SelectMany(c => c.Added.Concat(c.Modified))
                    .Where(f => f.EndsWith(".cs") || f.EndsWith(".razor") || f.EndsWith(".cshtml"))
                    .Distinct()
                    .ToArray();

                if (changedFiles.Any())
                {
                    var commitSha = payload.After ?? "";
                    var branch = payload.Ref?.Replace("refs/heads/", "") ?? "main";
                    _backgroundJobClient.Enqueue<CodeQualityMonitorJob>(
                        job => job.AnalyzeCommitAsync(commitSha, branch, changedFiles));
                }
            }

            return Ok(new { message = "Webhook processed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GitLab webhook");
            return StatusCode(500, new { error = "Webhook processing failed" });
        }
    }

    /// <summary>
    /// Generic webhook endpoint
    /// </summary>
    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<ActionResult> GenericWebhook([FromBody] GenericWebhookPayload payload)
    {
        _logger.LogInformation("Received generic webhook for {Repository}", payload.Repository);

        try
        {
            if (payload.Files?.Any() == true)
            {
                var codeFiles = payload.Files
                    .Where(f => f.EndsWith(".cs") || f.EndsWith(".razor") || f.EndsWith(".cshtml"))
                    .ToArray();

                if (codeFiles.Any())
                {
                    _backgroundJobClient.Enqueue<CodeQualityMonitorJob>(
                        job => job.AnalyzeCommitAsync(
                            payload.CommitSha ?? "",
                            payload.Branch ?? "main",
                            codeFiles));
                }
            }

            return Ok(new { message = "Webhook processed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook");
            return StatusCode(500, new { error = "Webhook processing failed" });
        }
    }

    /// <summary>
    /// Available agent types
    /// </summary>
    [HttpGet("agents")]
    public ActionResult GetAgentTypes()
    {
        return Ok(new
        {
            agents = new[]
            {
                new { type = CodeQualityAgentConfig.AgentTypes.CodeReviewer, description = "General code quality review" },
                new { type = CodeQualityAgentConfig.AgentTypes.SecurityScanner, description = "Security vulnerability scanning" },
                new { type = CodeQualityAgentConfig.AgentTypes.PerformanceAnalyzer, description = "Performance issue detection" },
                new { type = CodeQualityAgentConfig.AgentTypes.DependencyChecker, description = "Dependency vulnerability check" },
                new { type = CodeQualityAgentConfig.AgentTypes.ArchitectureAnalyzer, description = "Architecture and design analysis" },
                new { type = CodeQualityAgentConfig.AgentTypes.CodeStyleChecker, description = "Code style and formatting" },
                new { type = CodeQualityAgentConfig.AgentTypes.DocumentationChecker, description = "Documentation completeness" },
                new { type = CodeQualityAgentConfig.AgentTypes.TestCoverageAnalyzer, description = "Test coverage analysis" }
            }
        });
    }
}

#region Request/Response Models

public class FullScanRequest
{
    public string Code { get; set; } = string.Empty;
    public string? FilePath { get; set; }
}

public class TestAlertRequest
{
    public string? Severity { get; set; }
    public string? Message { get; set; }
}

public class GitHubWebhookPayload
{
    public string? Ref { get; set; }
    public string? Before { get; set; }
    public string? After { get; set; }
    public string? Action { get; set; }
    public List<GitHubCommit>? Commits { get; set; }
}

public class GitHubCommit
{
    public string Id { get; set; } = string.Empty;
    public List<string> Added { get; set; } = new();
    public List<string> Removed { get; set; } = new();
    public List<string> Modified { get; set; } = new();
}

public class GitLabWebhookPayload
{
    public string? Ref { get; set; }
    public string? Before { get; set; }
    public string? After { get; set; }
    public List<GitLabCommit>? Commits { get; set; }
}

public class GitLabCommit
{
    public string Id { get; set; } = string.Empty;
    public List<string> Added { get; set; } = new();
    public List<string> Removed { get; set; } = new();
    public List<string> Modified { get; set; } = new();
}

public class GenericWebhookPayload
{
    public string? Repository { get; set; }
    public string? Branch { get; set; }
    public string? CommitSha { get; set; }
    public List<string>? Files { get; set; }
}

#endregion
