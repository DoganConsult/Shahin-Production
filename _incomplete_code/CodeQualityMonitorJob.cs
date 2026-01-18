using GrcMvc.Agents;
using GrcMvc.Services.Interfaces;
using Hangfire;

namespace GrcMvc.BackgroundJobs;

/// <summary>
/// Background job for continuous code quality monitoring
/// Runs periodically to analyze code changes and send alerts
/// </summary>
public class CodeQualityMonitorJob
{
    private readonly ICodeQualityService _codeQualityService;
    private readonly IAlertService _alertService;
    private readonly ILogger<CodeQualityMonitorJob> _logger;
    private readonly IConfiguration _configuration;

    public CodeQualityMonitorJob(
        ICodeQualityService codeQualityService,
        IAlertService alertService,
        ILogger<CodeQualityMonitorJob> logger,
        IConfiguration configuration)
    {
        _codeQualityService = codeQualityService;
        _alertService = alertService;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Execute scheduled code quality scan
    /// </summary>
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 300, 900 })]
    [Queue("default")]
    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Starting scheduled code quality scan at {Time}", DateTime.UtcNow);

        try
        {
            var projectPath = _configuration["CodeQuality:ProjectPath"] ?? "/app";
            var filePatterns = _configuration.GetSection("CodeQuality:FilePatterns").Get<string[]>()
                ?? new[] { "*.cs", "*.razor", "*.cshtml" };

            // Get list of recently modified files (last 24 hours)
            var modifiedFiles = GetRecentlyModifiedFiles(projectPath, filePatterns, TimeSpan.FromHours(24));

            _logger.LogInformation("Found {FileCount} recently modified files to analyze", modifiedFiles.Count);

            if (!modifiedFiles.Any())
            {
                _logger.LogInformation("No recently modified files found, skipping analysis");
                return;
            }

            var results = new List<CodeAnalysisResult>();
            var alertsTriggered = 0;

            foreach (var filePath in modifiedFiles)
            {
                try
                {
                    // Run security scan on each file
                    var securityResult = await _codeQualityService.AnalyzeFileAsync(
                        filePath,
                        CodeQualityAgentConfig.AgentTypes.SecurityScanner);

                    if (securityResult.AlertTriggered)
                        alertsTriggered++;

                    results.Add(securityResult);

                    // Rate limiting - avoid overwhelming the API
                    await Task.Delay(500);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to analyze file: {FilePath}", filePath);
                }
            }

            // Generate summary report
            await GenerateSummaryReportAsync(results, alertsTriggered);

            _logger.LogInformation(
                "Code quality scan completed. Files: {FileCount}, Alerts: {AlertCount}",
                results.Count, alertsTriggered);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during scheduled code quality scan");
            throw;
        }
    }

    /// <summary>
    /// Execute full project security audit
    /// </summary>
    [AutomaticRetry(Attempts = 2)]
    [Queue("critical")]
    public async Task ExecuteFullSecurityAuditAsync()
    {
        _logger.LogInformation("Starting full security audit at {Time}", DateTime.UtcNow);

        try
        {
            var projectPath = _configuration["CodeQuality:ProjectPath"] ?? "/app";
            var filePatterns = new[] { "*.cs", "*.razor", "*.cshtml", "*.json", "*.yml", "*.yaml" };

            var allFiles = GetAllFiles(projectPath, filePatterns);
            _logger.LogInformation("Full audit scanning {FileCount} files", allFiles.Count);

            var criticalIssues = new List<CodeIssue>();
            var highIssues = new List<CodeIssue>();

            foreach (var filePath in allFiles)
            {
                try
                {
                    var result = await _codeQualityService.AnalyzeFileAsync(
                        filePath,
                        CodeQualityAgentConfig.AgentTypes.SecurityScanner);

                    criticalIssues.AddRange(result.Issues.Where(i =>
                        i.Severity.Equals("critical", StringComparison.OrdinalIgnoreCase)));
                    highIssues.AddRange(result.Issues.Where(i =>
                        i.Severity.Equals("high", StringComparison.OrdinalIgnoreCase)));

                    await Task.Delay(200);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to audit file: {FilePath}", filePath);
                }
            }

            // Send consolidated alert if critical issues found
            if (criticalIssues.Any() || highIssues.Count > 10)
            {
                var alert = new CodeQualityAlert
                {
                    Severity = criticalIssues.Any() ? "critical" : "high",
                    Title = "Security Audit Alert: Issues Found",
                    Message = $"Full security audit completed.\n" +
                             $"Critical Issues: {criticalIssues.Count}\n" +
                             $"High Issues: {highIssues.Count}\n" +
                             $"Files Scanned: {allFiles.Count}",
                    AgentType = "full-security-audit",
                    IssueCount = criticalIssues.Count + highIssues.Count,
                    TopIssues = criticalIssues.Take(5).Concat(highIssues.Take(5)).ToList()
                };

                await _alertService.SendAlertAsync(alert);
            }

            _logger.LogInformation(
                "Full security audit completed. Critical: {Critical}, High: {High}",
                criticalIssues.Count, highIssues.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during full security audit");
            throw;
        }
    }

    /// <summary>
    /// Analyze specific commit/PR
    /// </summary>
    [Queue("critical")]
    public async Task AnalyzeCommitAsync(string commitSha, string branch, string[] changedFiles)
    {
        _logger.LogInformation("Analyzing commit {CommitSha} on branch {Branch}", commitSha, branch);

        var results = new List<CodeAnalysisResult>();
        var alertsTriggered = 0;

        foreach (var filePath in changedFiles)
        {
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("File not found: {FilePath}", filePath);
                continue;
            }

            try
            {
                // Run full scan on changed files
                var code = await File.ReadAllTextAsync(filePath);
                var scanResults = await _codeQualityService.RunFullScanAsync(code, filePath);

                foreach (var result in scanResults)
                {
                    if (result.AlertTriggered)
                    {
                        alertsTriggered++;
                        // Enrich alert with commit info
                        var alert = _alertService.CreateAlert(result, null, commitSha, branch);
                        await _alertService.SendAlertAsync(alert);
                    }
                    results.Add(result);
                }

                await Task.Delay(300);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to analyze commit file: {FilePath}", filePath);
            }
        }

        _logger.LogInformation(
            "Commit analysis completed. Files: {FileCount}, Alerts: {AlertCount}",
            changedFiles.Length, alertsTriggered);
    }

    /// <summary>
    /// Generate daily quality report
    /// </summary>
    [Queue("low")]
    public async Task GenerateDailyReportAsync()
    {
        _logger.LogInformation("Generating daily code quality report");

        try
        {
            var metrics = await _codeQualityService.GetQualityMetricsAsync(
                DateTime.UtcNow.AddDays(-1),
                DateTime.UtcNow);

            var alert = new CodeQualityAlert
            {
                Severity = metrics.CriticalIssues > 0 ? "high" : "info",
                Title = "Daily Code Quality Report",
                Message = $"**Daily Summary ({DateTime.UtcNow:yyyy-MM-dd})**\n\n" +
                         $"Files Analyzed: {metrics.TotalFilesAnalyzed}\n" +
                         $"Total Issues: {metrics.TotalIssuesFound}\n" +
                         $"  - Critical: {metrics.CriticalIssues}\n" +
                         $"  - High: {metrics.HighIssues}\n" +
                         $"  - Medium: {metrics.MediumIssues}\n" +
                         $"  - Low: {metrics.LowIssues}\n\n" +
                         $"Average Quality Score: {metrics.AverageQualityScore:F1}/100",
                AgentType = "daily-report",
                Score = (int)metrics.AverageQualityScore,
                IssueCount = metrics.TotalIssuesFound
            };

            await _alertService.SendAlertAsync(alert);

            _logger.LogInformation("Daily report generated and sent");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating daily report");
            throw;
        }
    }

    private List<string> GetRecentlyModifiedFiles(string path, string[] patterns, TimeSpan within)
    {
        var cutoff = DateTime.UtcNow - within;
        var files = new List<string>();

        if (!Directory.Exists(path))
            return files;

        foreach (var pattern in patterns)
        {
            try
            {
                var matchingFiles = Directory.GetFiles(path, pattern, SearchOption.AllDirectories)
                    .Where(f => File.GetLastWriteTimeUtc(f) >= cutoff)
                    .Where(f => !ShouldExcludeFile(f));

                files.AddRange(matchingFiles);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error searching for pattern {Pattern} in {Path}", pattern, path);
            }
        }

        return files.Distinct().ToList();
    }

    private List<string> GetAllFiles(string path, string[] patterns)
    {
        var files = new List<string>();

        if (!Directory.Exists(path))
            return files;

        foreach (var pattern in patterns)
        {
            try
            {
                var matchingFiles = Directory.GetFiles(path, pattern, SearchOption.AllDirectories)
                    .Where(f => !ShouldExcludeFile(f));

                files.AddRange(matchingFiles);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error searching for pattern {Pattern} in {Path}", pattern, path);
            }
        }

        return files.Distinct().ToList();
    }

    private bool ShouldExcludeFile(string filePath)
    {
        var excludePatterns = new[]
        {
            "/obj/", "/bin/", "/node_modules/", "/.git/",
            "/Migrations/", "/packages/", "/TestResults/",
            ".Designer.cs", ".g.cs", ".generated.cs"
        };

        return excludePatterns.Any(p => filePath.Contains(p, StringComparison.OrdinalIgnoreCase));
    }

    private async Task GenerateSummaryReportAsync(List<CodeAnalysisResult> results, int alertsTriggered)
    {
        if (!results.Any())
            return;

        var avgScore = results.Average(r => r.Score);
        var totalIssues = results.Sum(r => r.Issues.Count);
        var criticalCount = results.Sum(r => r.Issues.Count(i =>
            i.Severity.Equals("critical", StringComparison.OrdinalIgnoreCase)));

        // Only send summary if there are concerning results
        if (criticalCount > 0 || avgScore < 60 || alertsTriggered > 0)
        {
            var alert = new CodeQualityAlert
            {
                Severity = criticalCount > 0 ? "critical" : avgScore < 50 ? "high" : "medium",
                Title = "Code Quality Scan Summary",
                Message = $"Scan completed at {DateTime.UtcNow:HH:mm:ss} UTC\n\n" +
                         $"Files Scanned: {results.Count}\n" +
                         $"Average Score: {avgScore:F1}/100\n" +
                         $"Total Issues: {totalIssues}\n" +
                         $"Critical Issues: {criticalCount}\n" +
                         $"Alerts Triggered: {alertsTriggered}",
                AgentType = "scan-summary",
                Score = (int)avgScore,
                IssueCount = totalIssues
            };

            await _alertService.SendAlertAsync(alert);
        }
    }
}
