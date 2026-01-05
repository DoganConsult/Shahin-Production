using System.Text;
using System.Text.Json;
using GrcMvc.Agents;
using GrcMvc.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Alert service for code quality notifications
/// </summary>
public class AlertService : IAlertService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AlertService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;
    private readonly IAppEmailSender _emailSender;

    private const string AlertConfigCacheKey = "alert_configuration";
    private const string AlertHistoryCacheKey = "alert_history";

    public AlertService(
        HttpClient httpClient,
        ILogger<AlertService> logger,
        IConfiguration configuration,
        IMemoryCache cache,
        IAppEmailSender emailSender)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
        _cache = cache;
        _emailSender = emailSender;
    }

    public async Task<bool> SendAlertAsync(CodeQualityAlert alert, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending code quality alert: {Severity} - {Title}", alert.Severity, alert.Title);

        var config = await GetConfigurationAsync();
        var success = true;

        try
        {
            // Send to all configured channels
            var tasks = new List<Task<bool>>();

            if (config.EnableEmailAlerts && config.EmailRecipients.Any())
            {
                tasks.Add(SendEmailAlertAsync(alert, config.EmailRecipients, cancellationToken));
            }

            if (config.EnableSlackAlerts && !string.IsNullOrEmpty(config.SlackWebhookUrl))
            {
                tasks.Add(SendSlackAlertAsync(alert, config.SlackWebhookUrl, cancellationToken));
            }

            if (config.EnableTeamsAlerts && !string.IsNullOrEmpty(config.TeamsWebhookUrl))
            {
                tasks.Add(SendTeamsAlertAsync(alert, config.TeamsWebhookUrl, cancellationToken));
            }

            if (config.EnableWebhookAlerts && !string.IsNullOrEmpty(config.CustomWebhookUrl))
            {
                tasks.Add(SendWebhookAlertAsync(alert, config.CustomWebhookUrl, cancellationToken));
            }

            if (tasks.Any())
            {
                var results = await Task.WhenAll(tasks);
                success = results.All(r => r);
            }

            // Store in alert history
            await StoreAlertAsync(alert);

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending alert: {AlertId}", alert.Id);
            return false;
        }
    }

    public bool ShouldTriggerAlert(CodeAnalysisResult result, AlertConfiguration? config = null)
    {
        config ??= GetConfigurationAsync().GetAwaiter().GetResult();

        // Check severity
        if (config.AlertOnSeverities.Contains(result.Severity, StringComparer.OrdinalIgnoreCase))
            return true;

        // Check score threshold
        if (result.Score < config.MinScoreThreshold)
            return true;

        // Check for critical issues
        var criticalCount = result.Issues.Count(i => i.Severity.Equals("critical", StringComparison.OrdinalIgnoreCase));
        if (criticalCount > CodeQualityAgentConfig.AlertThresholds.MaxCriticalIssues)
            return true;

        // Check for high severity issues
        var highCount = result.Issues.Count(i => i.Severity.Equals("high", StringComparison.OrdinalIgnoreCase));
        if (highCount > CodeQualityAgentConfig.AlertThresholds.MaxHighIssues)
            return true;

        return false;
    }

    public CodeQualityAlert CreateAlert(CodeAnalysisResult result, string? repositoryUrl = null, string? commitSha = null, string? branch = null)
    {
        var criticalCount = result.Issues.Count(i => i.Severity.Equals("critical", StringComparison.OrdinalIgnoreCase));
        var highCount = result.Issues.Count(i => i.Severity.Equals("high", StringComparison.OrdinalIgnoreCase));

        var severity = criticalCount > 0 ? "critical" :
                      highCount > 0 ? "high" :
                      result.Score < 50 ? "high" :
                      result.Score < 70 ? "medium" : "low";

        var title = $"Code Quality Alert: {result.AgentType} - Score {result.Score}/100";

        var message = new StringBuilder();
        message.AppendLine($"**File**: {result.FilePath}");
        message.AppendLine($"**Score**: {result.Score}/100");
        message.AppendLine($"**Issues Found**: {result.Issues.Count}");
        message.AppendLine($"  - Critical: {criticalCount}");
        message.AppendLine($"  - High: {highCount}");
        message.AppendLine($"  - Medium: {result.Issues.Count(i => i.Severity.Equals("medium", StringComparison.OrdinalIgnoreCase))}");
        message.AppendLine($"  - Low: {result.Issues.Count(i => i.Severity.Equals("low", StringComparison.OrdinalIgnoreCase))}");

        if (!string.IsNullOrEmpty(result.Summary))
        {
            message.AppendLine();
            message.AppendLine($"**Summary**: {result.Summary}");
        }

        return new CodeQualityAlert
        {
            Severity = severity,
            Title = title,
            Message = message.ToString(),
            FilePath = result.FilePath,
            AgentType = result.AgentType,
            Score = result.Score,
            IssueCount = result.Issues.Count,
            TopIssues = result.Issues.Take(5).ToList(),
            RepositoryUrl = repositoryUrl,
            CommitSha = commitSha,
            Branch = branch
        };
    }

    public async Task<bool> SendEmailAlertAsync(CodeQualityAlert alert, List<string> recipients, CancellationToken cancellationToken = default)
    {
        try
        {
            var subject = $"[{alert.Severity.ToUpper()}] {alert.Title}";
            var body = BuildEmailBody(alert);

            foreach (var recipient in recipients)
            {
                await _emailSender.SendEmailAsync(recipient, subject, body);
            }

            _logger.LogInformation("Email alert sent to {RecipientCount} recipients", recipients.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email alert");
            return false;
        }
    }

    public async Task<bool> SendSlackAlertAsync(CodeQualityAlert alert, string webhookUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            var color = alert.Severity switch
            {
                "critical" => "#dc3545",
                "high" => "#fd7e14",
                "medium" => "#ffc107",
                "low" => "#17a2b8",
                _ => "#6c757d"
            };

            var payload = new
            {
                attachments = new[]
                {
                    new
                    {
                        color,
                        title = alert.Title,
                        text = alert.Message,
                        fields = new[]
                        {
                            new { title = "File", value = alert.FilePath, @short = true },
                            new { title = "Score", value = $"{alert.Score}/100", @short = true },
                            new { title = "Issues", value = alert.IssueCount.ToString(), @short = true },
                            new { title = "Agent", value = alert.AgentType, @short = true }
                        },
                        footer = "GRC Code Quality Monitor",
                        ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                    }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(webhookUrl, content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Slack alert sent successfully");
                return true;
            }

            _logger.LogWarning("Slack alert failed with status: {StatusCode}", response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send Slack alert");
            return false;
        }
    }

    public async Task<bool> SendTeamsAlertAsync(CodeQualityAlert alert, string webhookUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            var themeColor = alert.Severity switch
            {
                "critical" => "dc3545",
                "high" => "fd7e14",
                "medium" => "ffc107",
                "low" => "17a2b8",
                _ => "6c757d"
            };

            var payload = new
            {
                @type = "MessageCard",
                @context = "http://schema.org/extensions",
                themeColor,
                summary = alert.Title,
                sections = new[]
                {
                    new
                    {
                        activityTitle = alert.Title,
                        activitySubtitle = $"Analyzed at {alert.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC",
                        facts = new[]
                        {
                            new { name = "File", value = alert.FilePath },
                            new { name = "Score", value = $"{alert.Score}/100" },
                            new { name = "Issues", value = alert.IssueCount.ToString() },
                            new { name = "Agent", value = alert.AgentType },
                            new { name = "Severity", value = alert.Severity.ToUpper() }
                        },
                        markdown = true,
                        text = alert.Message
                    }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(webhookUrl, content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Teams alert sent successfully");
                return true;
            }

            _logger.LogWarning("Teams alert failed with status: {StatusCode}", response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send Teams alert");
            return false;
        }
    }

    public async Task<bool> SendWebhookAlertAsync(CodeQualityAlert alert, string webhookUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            var content = new StringContent(JsonSerializer.Serialize(alert), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(webhookUrl, content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Webhook alert sent successfully to {Url}", webhookUrl);
                return true;
            }

            _logger.LogWarning("Webhook alert failed with status: {StatusCode}", response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send webhook alert");
            return false;
        }
    }

    public Task<List<CodeQualityAlert>> GetAlertHistoryAsync(DateTime? from = null, DateTime? to = null, string? severity = null)
    {
        var history = _cache.GetOrCreate(AlertHistoryCacheKey, entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromHours(24);
            return new List<CodeQualityAlert>();
        }) ?? new List<CodeQualityAlert>();

        var filtered = history.AsQueryable();

        if (from.HasValue)
            filtered = filtered.Where(a => a.CreatedAt >= from.Value);

        if (to.HasValue)
            filtered = filtered.Where(a => a.CreatedAt <= to.Value);

        if (!string.IsNullOrEmpty(severity))
            filtered = filtered.Where(a => a.Severity.Equals(severity, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(filtered.OrderByDescending(a => a.CreatedAt).ToList());
    }

    public Task<AlertConfiguration> GetConfigurationAsync()
    {
        var config = _cache.GetOrCreate(AlertConfigCacheKey, entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromHours(1);

            return new AlertConfiguration
            {
                EnableEmailAlerts = _configuration.GetValue("Alerts:EnableEmail", true),
                EnableSlackAlerts = _configuration.GetValue("Alerts:EnableSlack", false),
                EnableTeamsAlerts = _configuration.GetValue("Alerts:EnableTeams", false),
                EnableWebhookAlerts = _configuration.GetValue("Alerts:EnableWebhook", true),
                EmailRecipients = _configuration.GetSection("Alerts:EmailRecipients").Get<List<string>>() ?? new List<string>(),
                SlackWebhookUrl = _configuration["Alerts:SlackWebhookUrl"],
                TeamsWebhookUrl = _configuration["Alerts:TeamsWebhookUrl"],
                CustomWebhookUrl = _configuration["Alerts:CustomWebhookUrl"],
                AlertOnSeverities = _configuration.GetSection("Alerts:AlertOnSeverities").Get<List<string>>() ?? new List<string> { "critical", "high" },
                MinScoreThreshold = _configuration.GetValue("Alerts:MinScoreThreshold", 50)
            };
        });

        return Task.FromResult(config ?? new AlertConfiguration());
    }

    public Task UpdateConfigurationAsync(AlertConfiguration config)
    {
        _cache.Set(AlertConfigCacheKey, config, TimeSpan.FromHours(24));
        _logger.LogInformation("Alert configuration updated");
        return Task.CompletedTask;
    }

    private Task StoreAlertAsync(CodeQualityAlert alert)
    {
        var history = _cache.GetOrCreate(AlertHistoryCacheKey, entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromHours(24);
            return new List<CodeQualityAlert>();
        }) ?? new List<CodeQualityAlert>();

        history.Insert(0, alert);

        // Keep only last 1000 alerts
        if (history.Count > 1000)
            history = history.Take(1000).ToList();

        _cache.Set(AlertHistoryCacheKey, history, TimeSpan.FromHours(24));

        return Task.CompletedTask;
    }

    private string BuildEmailBody(CodeQualityAlert alert)
    {
        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html><head><style>");
        html.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; }");
        html.AppendLine(".header { padding: 20px; color: white; border-radius: 5px; }");
        html.AppendLine(".critical { background-color: #dc3545; }");
        html.AppendLine(".high { background-color: #fd7e14; }");
        html.AppendLine(".medium { background-color: #ffc107; color: black; }");
        html.AppendLine(".low { background-color: #17a2b8; }");
        html.AppendLine(".info { background-color: #6c757d; }");
        html.AppendLine(".section { margin: 20px 0; padding: 15px; border: 1px solid #ddd; border-radius: 5px; }");
        html.AppendLine(".issue { padding: 10px; margin: 5px 0; border-left: 4px solid; }");
        html.AppendLine(".issue-critical { border-color: #dc3545; background: #f8d7da; }");
        html.AppendLine(".issue-high { border-color: #fd7e14; background: #fff3cd; }");
        html.AppendLine(".issue-medium { border-color: #ffc107; background: #fff3cd; }");
        html.AppendLine(".issue-low { border-color: #17a2b8; background: #d1ecf1; }");
        html.AppendLine("table { border-collapse: collapse; width: 100%; }");
        html.AppendLine("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
        html.AppendLine("th { background-color: #f2f2f2; }");
        html.AppendLine("</style></head><body>");

        html.AppendLine($"<div class='header {alert.Severity}'>");
        html.AppendLine($"<h1>{alert.Title}</h1>");
        html.AppendLine($"<p>Generated: {alert.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC</p>");
        html.AppendLine("</div>");

        html.AppendLine("<div class='section'>");
        html.AppendLine("<h2>Summary</h2>");
        html.AppendLine("<table>");
        html.AppendLine($"<tr><th>File</th><td>{alert.FilePath}</td></tr>");
        html.AppendLine($"<tr><th>Score</th><td>{alert.Score}/100</td></tr>");
        html.AppendLine($"<tr><th>Issues Found</th><td>{alert.IssueCount}</td></tr>");
        html.AppendLine($"<tr><th>Agent</th><td>{alert.AgentType}</td></tr>");
        html.AppendLine($"<tr><th>Severity</th><td>{alert.Severity.ToUpper()}</td></tr>");
        if (!string.IsNullOrEmpty(alert.Branch))
            html.AppendLine($"<tr><th>Branch</th><td>{alert.Branch}</td></tr>");
        if (!string.IsNullOrEmpty(alert.CommitSha))
            html.AppendLine($"<tr><th>Commit</th><td>{alert.CommitSha}</td></tr>");
        html.AppendLine("</table>");
        html.AppendLine("</div>");

        if (alert.TopIssues.Any())
        {
            html.AppendLine("<div class='section'>");
            html.AppendLine("<h2>Top Issues</h2>");
            foreach (var issue in alert.TopIssues)
            {
                html.AppendLine($"<div class='issue issue-{issue.Severity}'>");
                html.AppendLine($"<strong>[{issue.Severity.ToUpper()}]</strong> ");
                if (issue.Line.HasValue)
                    html.AppendLine($"Line {issue.Line}: ");
                html.AppendLine($"{issue.Description}");
                if (!string.IsNullOrEmpty(issue.Suggestion))
                    html.AppendLine($"<br/><em>Suggestion: {issue.Suggestion}</em>");
                html.AppendLine("</div>");
            }
            html.AppendLine("</div>");
        }

        html.AppendLine("<div class='section'>");
        html.AppendLine("<p><small>This alert was generated by GRC Code Quality Monitor.</small></p>");
        html.AppendLine("</div>");

        html.AppendLine("</body></html>");

        return html.ToString();
    }
}
