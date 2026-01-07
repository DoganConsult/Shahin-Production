using System.Text;
using System.Text.Json;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using GrcMvc.Data;
using GrcMvc.Configuration;
using GrcMvc.Models.Entities;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// AI-powered diagnostic agent service implementation
/// Uses Claude AI to analyze errors, conditions, and provide diagnostic insights
/// </summary>
public class DiagnosticAgentService : IDiagnosticAgentService
{
    private readonly GrcDbContext _dbContext;
    private readonly ILogger<DiagnosticAgentService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ClaudeApiSettings _claudeSettings;

    public DiagnosticAgentService(
        GrcDbContext dbContext,
        ILogger<DiagnosticAgentService> logger,
        IHttpClientFactory httpClientFactory,
        IOptions<ClaudeApiSettings> claudeSettings)
    {
        _dbContext = dbContext;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _claudeSettings = claudeSettings.Value;
    }

    public async Task<DiagnosticReport> AnalyzeErrorsAsync(
        int? hoursBack = 24,
        string? severity = null,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var since = DateTime.UtcNow.AddHours(-(hoursBack ?? 24));
            
            // Query error logs from database (assuming AuditEvent or similar table)
            var errorQuery = _dbContext.AuditEvents
                .Where(e => e.EventType == "Error" && e.CreatedAt >= since);

            if (!string.IsNullOrEmpty(severity))
            {
                errorQuery = errorQuery.Where(e => e.Severity == severity);
            }

            if (tenantId.HasValue)
            {
                errorQuery = errorQuery.Where(e => e.TenantId == tenantId.Value);
            }

            var errors = await errorQuery
                .OrderByDescending(e => e.CreatedAt)
                .Take(1000)
                .ToListAsync(cancellationToken);

            // Group errors by type
            var errorGroups = errors
                .GroupBy(e => e.EventType + ":" + (e.ErrorMessage ?? "").Substring(0, Math.Min(100, (e.ErrorMessage ?? "").Length)))
                .Select(g => 
                {
                    var first = g.First();
                    var errorMsg = first.ErrorMessage ?? "";
                    return new ErrorSummary
                    {
                        ErrorType = g.Key,
                        ExceptionType = ExtractExceptionType(errorMsg),
                        OccurrenceCount = g.Count(),
                        FirstOccurrence = g.Min(e => e.CreatedAt),
                        LastOccurrence = g.Max(e => e.CreatedAt),
                        Severity = first.Severity ?? "Medium",
                        AffectedTenants = g.Select(e => e.TenantId.ToString()).Distinct().ToList(),
                        MostCommonMessage = errorMsg,
                        MostCommonStackTrace = ExtractStackTrace(errorMsg)
                    };
                })
                .OrderByDescending(e => e.OccurrenceCount)
                .ToList();

            // Use AI to analyze patterns and provide insights
            var analysisPrompt = BuildErrorAnalysisPrompt(errors, errorGroups);
            var aiAnalysis = await CallClaudeAIAsync(analysisPrompt, cancellationToken);

            var report = new DiagnosticReport
            {
                GeneratedAt = DateTime.UtcNow,
                AnalysisPeriod = TimeSpan.FromHours(hoursBack ?? 24),
                TotalErrors = errors.Count,
                CriticalErrors = errors.Count(e => e.Severity == "Critical"),
                HighErrors = errors.Count(e => e.Severity == "High"),
                MediumErrors = errors.Count(e => e.Severity == "Medium"),
                LowErrors = errors.Count(e => e.Severity == "Low"),
                ErrorSummaries = errorGroups,
                Patterns = ParsePatterns(aiAnalysis),
                Insights = ParseInsights(aiAnalysis),
                Recommendations = ParseRecommendations(aiAnalysis),
                OverallStatus = DetermineOverallStatus(errors),
                HealthScore = CalculateHealthScore(errors)
            };

            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing errors");
            throw;
        }
    }

    public async Task<ErrorDiagnosis> DiagnoseErrorAsync(
        string errorId,
        string? exceptionType = null,
        string? stackTrace = null,
        string? context = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var diagnosisPrompt = $@"
Analyze this error and provide a detailed diagnosis:

Error ID: {errorId}
Exception Type: {exceptionType ?? "Unknown"}
Context: {context ?? "No additional context"}

StackTrace:
{stackTrace ?? "No stack trace available"}

Please provide:
1. Root cause analysis
2. Explanation of what went wrong
3. Contributing factors
4. Specific fix suggestions with code examples if applicable
5. Prevention steps
6. Related code locations if identifiable

Return JSON format:
{{
  ""rootCause"": ""..."",
  ""explanation"": ""..."",
  ""contributingFactors"": [""..."", ""...""],
  ""fixSuggestions"": [
    {{
      ""title"": ""..."",
      ""description"": ""..."",
      ""priority"": ""Critical|High|Medium|Low"",
      ""codeExample"": ""..."",
      ""estimatedEffort"": 30
    }}
  ],
  ""preventionSteps"": [""..."", ""...""],
  ""relatedCodeLocation"": ""...""
}}
";

            var aiResponse = await CallClaudeAIAsync(diagnosisPrompt, cancellationToken);
            var diagnosis = ParseErrorDiagnosis(aiResponse, errorId, exceptionType);

            return diagnosis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error diagnosing error {ErrorId}", errorId);
            throw;
        }
    }

    public async Task<HealthDiagnosis> AnalyzeHealthAsync(
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Collect health metrics
            var recentErrors = await _dbContext.AuditEvents
                .Where(e => e.EventType == "Error" && e.CreatedAt >= DateTime.UtcNow.AddHours(-1))
                .CountAsync(cancellationToken);

            var dbHealth = await CheckDatabaseHealthAsync(cancellationToken);
            var recentPerformance = await GetRecentPerformanceMetricsAsync(cancellationToken);

            var healthPrompt = $@"
Analyze the application health based on these metrics:

Recent Errors (last hour): {recentErrors}
Database Health: {dbHealth.Status}
Performance Metrics: {JsonSerializer.Serialize(recentPerformance)}

Provide:
1. Overall health status (Healthy, Degraded, Unhealthy)
2. Health score (0-100)
3. Specific issues identified
4. Recommendations for improvement

Return JSON format:
{{
  ""overallStatus"": ""Healthy|Degraded|Unhealthy"",
  ""healthScore"": 85,
  ""issues"": [
    {{
      ""category"": ""..."",
      ""title"": ""..."",
      ""description"": ""..."",
      ""severity"": ""Critical|High|Medium|Low"",
      ""impact"": ""..."",
      ""affectedComponent"": ""...""
    }}
  ],
  ""recommendations"": [
    {{
      ""category"": ""..."",
      ""title"": ""..."",
      ""description"": ""..."",
      ""priority"": ""Critical|High|Medium|Low"",
      ""impact"": ""..."",
      ""estimatedEffort"": 15,
      ""steps"": [""..."", ""...""]
    }}
  ]
}}
";

            var aiResponse = await CallClaudeAIAsync(healthPrompt, cancellationToken);
            var diagnosis = ParseHealthDiagnosis(aiResponse, recentErrors, dbHealth);

            return diagnosis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing health");
            throw;
        }
    }

    public async Task<PatternAnalysis> DetectPatternsAsync(
        int daysBack = 7,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var since = DateTime.UtcNow.AddDays(-daysBack);
            var errors = await _dbContext.AuditEvents
                .Where(e => e.EventType == "Error" && e.CreatedAt >= since)
                .OrderBy(e => e.CreatedAt)
                .ToListAsync(cancellationToken);

            var patternPrompt = $@"
Analyze these errors over the past {daysBack} days and detect patterns:

Total Errors: {errors.Count}
Error Types: {string.Join(", ", errors.Select(e => e.EventType).Distinct())}

Please identify:
1. Recurring error patterns
2. Time-based patterns (e.g., errors spike at certain times)
3. Escalating trends
4. Correlations between different error types
5. Tenant-specific patterns (if applicable)

Return JSON format:
{{
  ""patterns"": [
    {{
      ""patternType"": ""recurring|escalating|time-based|tenant-specific"",
      ""description"": ""..."",
      ""frequency"": 10,
      ""trend"": ""increasing|decreasing|stable"",
      ""relatedErrors"": [""..."", ""...""],
      ""rootCause"": ""..."",
      ""suggestedFix"": ""...""
    }}
  ],
  ""trends"": [
    {{
      ""metric"": ""..."",
      ""trend"": ""increasing|decreasing|stable|volatile"",
      ""changePercentage"": 15.5,
      ""interpretation"": ""...""
    }}
  ],
  ""correlations"": [
    {{
      ""event1"": ""..."",
      ""event2"": ""..."",
      ""correlationStrength"": 0.75,
      ""relationship"": ""...""
    }}
  ],
  ""summary"": ""...""
}}
";

            var aiResponse = await CallClaudeAIAsync(patternPrompt, cancellationToken);
            var analysis = ParsePatternAnalysis(aiResponse, daysBack);

            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting patterns");
            throw;
        }
    }

    public async Task<RootCauseAnalysis> AnalyzeRootCauseAsync(
        string problemDescription,
        Dictionary<string, object>? context = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var contextJson = context != null ? JsonSerializer.Serialize(context) : "No additional context";

            var rcaPrompt = $@"
Perform root cause analysis for this problem:

Problem Description: {problemDescription}

Context:
{contextJson}

Please provide:
1. Root cause identification
2. Contributing factors
3. Symptoms observed
4. Specific fixes with code examples
5. Confidence level (High, Medium, Low)
6. Evidence supporting the analysis

Return JSON format:
{{
  ""rootCause"": ""..."",
  ""contributingFactors"": [""..."", ""...""],
  ""symptoms"": [""..."", ""...""],
  ""fixes"": [
    {{
      ""title"": ""..."",
      ""description"": ""..."",
      ""priority"": ""Critical|High|Medium|Low"",
      ""codeExample"": ""..."",
      ""estimatedEffort"": 30
    }}
  ],
  ""confidence"": ""High|Medium|Low"",
  ""evidence"": {{
    ""key1"": ""value1"",
    ""key2"": ""value2""
  }}
}}
";

            var aiResponse = await CallClaudeAIAsync(rcaPrompt, cancellationToken);
            var rca = ParseRootCauseAnalysis(aiResponse, problemDescription);

            return rca;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing root cause");
            throw;
        }
    }

    public async Task<List<DiagnosticRecommendation>> GetRecommendationsAsync(
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var recentErrors = await _dbContext.AuditEvents
                .Where(e => e.EventType == "Error" && e.CreatedAt >= DateTime.UtcNow.AddHours(-24))
                .ToListAsync(cancellationToken);

            var recommendationPrompt = $@"
Based on recent application activity, provide proactive recommendations:

Recent Errors: {recentErrors.Count}
Error Types: {string.Join(", ", recentErrors.Select(e => e.EventType).Distinct())}

Provide actionable recommendations to:
1. Prevent future errors
2. Improve system reliability
3. Optimize performance
4. Enhance security
5. Improve user experience

Return JSON array format:
[
  {{
    ""category"": ""..."",
    ""title"": ""..."",
    ""description"": ""..."",
    ""priority"": ""Critical|High|Medium|Low"",
    ""impact"": ""..."",
    ""estimatedEffort"": 30,
    ""steps"": [""..."", ""...""],
    ""relatedIssue"": ""...""
  }}
]
";

            var aiResponse = await CallClaudeAIAsync(recommendationPrompt, cancellationToken);
            var recommendations = ParseRecommendations(aiResponse);

            return recommendations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recommendations");
            return new List<DiagnosticRecommendation>();
        }
    }

    public async Task<List<DiagnosticAlert>> MonitorConditionsAsync(
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var alerts = new List<DiagnosticAlert>();

            // Check for critical errors
            var criticalErrors = await _dbContext.AuditEvents
                .Where(e => e.EventType == "Error" && 
                           e.Severity == "Critical" && 
                           e.CreatedAt >= DateTime.UtcNow.AddHours(-1))
                .CountAsync(cancellationToken);

            if (criticalErrors > 0)
            {
                alerts.Add(new DiagnosticAlert
                {
                    Severity = "Critical",
                    Category = "Errors",
                    Title = "Critical Errors Detected",
                    Message = $"{criticalErrors} critical error(s) occurred in the last hour",
                    CreatedAt = DateTime.UtcNow
                });
            }

            // Check database health
            var dbHealth = await CheckDatabaseHealthAsync(cancellationToken);
            if (dbHealth.Status != "Healthy")
            {
                alerts.Add(new DiagnosticAlert
                {
                    Severity = "High",
                    Category = "Database",
                    Title = "Database Health Issue",
                    Message = $"Database health check failed: {dbHealth.Message}",
                    AffectedComponent = "Database",
                    CreatedAt = DateTime.UtcNow
                });
            }

            // Use AI to analyze and generate additional alerts
            var alertPrompt = $@"
Analyze current system conditions and generate alerts if needed:

Critical Errors (last hour): {criticalErrors}
Database Status: {dbHealth.Status}

Generate alerts for any concerning conditions. Return JSON array:
[
  {{
    ""severity"": ""Critical|High|Medium|Low"",
    ""category"": ""..."",
    ""title"": ""..."",
    ""message"": ""..."",
    ""affectedComponent"": ""...""
  }}
]
";

            var aiResponse = await CallClaudeAIAsync(alertPrompt, cancellationToken);
            var aiAlerts = ParseAlerts(aiResponse);
            alerts.AddRange(aiAlerts);

            return alerts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error monitoring conditions");
            return new List<DiagnosticAlert>();
        }
    }

    #region Helper Methods

    private async Task<string> CallClaudeAIAsync(string prompt, CancellationToken cancellationToken)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("x-api-key", _claudeSettings.ApiKey);
            httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

            var requestBody = new
            {
                model = "claude-3-5-sonnet-20241022",
                max_tokens = 4096,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("https://api.anthropic.com/v1/messages", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseObj = JsonSerializer.Deserialize<JsonElement>(responseJson);

            return responseObj.GetProperty("content")[0].GetProperty("text").GetString() ?? "";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Claude AI");
            return "{}"; // Return empty JSON on error
        }
    }

    private string BuildErrorAnalysisPrompt(List<AuditEvent> errors, List<ErrorSummary> errorGroups)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Analyze these application errors and provide insights:");
        sb.AppendLine($"Total Errors: {errors.Count}");
        sb.AppendLine($"Error Groups: {errorGroups.Count}");
        sb.AppendLine();
        sb.AppendLine("Top Error Types:");
        foreach (var group in errorGroups.Take(10))
        {
            sb.AppendLine($"- {group.ErrorType}: {group.OccurrenceCount} occurrences");
        }
        sb.AppendLine();
        sb.AppendLine("Provide JSON response with patterns, insights, and recommendations.");
        return sb.ToString();
    }

    private List<ErrorPattern> ParsePatterns(string aiResponse)
    {
        // Parse AI response JSON and extract patterns
        try
        {
            var json = JsonSerializer.Deserialize<JsonElement>(aiResponse);
            if (json.TryGetProperty("patterns", out var patterns))
            {
                return JsonSerializer.Deserialize<List<ErrorPattern>>(patterns.GetRawText()) ?? new List<ErrorPattern>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing patterns from AI response");
        }
        return new List<ErrorPattern>();
    }

    private List<DiagnosticInsight> ParseInsights(string aiResponse)
    {
        try
        {
            var json = JsonSerializer.Deserialize<JsonElement>(aiResponse);
            if (json.TryGetProperty("insights", out var insights))
            {
                return JsonSerializer.Deserialize<List<DiagnosticInsight>>(insights.GetRawText()) ?? new List<DiagnosticInsight>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing insights from AI response");
        }
        return new List<DiagnosticInsight>();
    }

    private List<DiagnosticRecommendation> ParseRecommendations(string aiResponse)
    {
        try
        {
            var json = JsonSerializer.Deserialize<JsonElement>(aiResponse);
            if (json.TryGetProperty("recommendations", out var recommendations))
            {
                return JsonSerializer.Deserialize<List<DiagnosticRecommendation>>(recommendations.GetRawText()) ?? new List<DiagnosticRecommendation>();
            }
            // Try parsing as array directly
            return JsonSerializer.Deserialize<List<DiagnosticRecommendation>>(aiResponse) ?? new List<DiagnosticRecommendation>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing recommendations from AI response");
        }
        return new List<DiagnosticRecommendation>();
    }

    private ErrorDiagnosis ParseErrorDiagnosis(string aiResponse, string errorId, string? exceptionType)
    {
        try
        {
            var json = JsonSerializer.Deserialize<JsonElement>(aiResponse);
            return new ErrorDiagnosis
            {
                ErrorId = errorId,
                DiagnosedAt = DateTime.UtcNow,
                ErrorType = exceptionType ?? "Unknown",
                RootCause = json.TryGetProperty("rootCause", out var rc) ? rc.GetString() : null,
                Explanation = json.TryGetProperty("explanation", out var exp) ? exp.GetString() : null,
                ContributingFactors = json.TryGetProperty("contributingFactors", out var cf) 
                    ? JsonSerializer.Deserialize<List<string>>(cf.GetRawText()) ?? new List<string>()
                    : new List<string>(),
                FixSuggestions = json.TryGetProperty("fixSuggestions", out var fs)
                    ? JsonSerializer.Deserialize<List<FixSuggestion>>(fs.GetRawText()) ?? new List<FixSuggestion>()
                    : new List<FixSuggestion>(),
                PreventionSteps = json.TryGetProperty("preventionSteps", out var ps)
                    ? JsonSerializer.Deserialize<List<string>>(ps.GetRawText()) ?? new List<string>()
                    : new List<string>(),
                RelatedCodeLocation = json.TryGetProperty("relatedCodeLocation", out var rcl) ? rcl.GetString() : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing error diagnosis");
            return new ErrorDiagnosis { ErrorId = errorId, ErrorType = exceptionType ?? "Unknown" };
        }
    }

    private HealthDiagnosis ParseHealthDiagnosis(string aiResponse, int recentErrors, (string Status, string Message) dbHealth)
    {
        try
        {
            var json = JsonSerializer.Deserialize<JsonElement>(aiResponse);
            return new HealthDiagnosis
            {
                DiagnosedAt = DateTime.UtcNow,
                OverallStatus = json.TryGetProperty("overallStatus", out var os) ? os.GetString() ?? "Unknown" : "Unknown",
                HealthScore = json.TryGetProperty("healthScore", out var hs) ? hs.GetInt32() : 50,
                Issues = json.TryGetProperty("issues", out var issues)
                    ? JsonSerializer.Deserialize<List<HealthIssue>>(issues.GetRawText()) ?? new List<HealthIssue>()
                    : new List<HealthIssue>(),
                Recommendations = json.TryGetProperty("recommendations", out var recs)
                    ? JsonSerializer.Deserialize<List<DiagnosticRecommendation>>(recs.GetRawText()) ?? new List<DiagnosticRecommendation>()
                    : new List<DiagnosticRecommendation>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing health diagnosis");
            return new HealthDiagnosis { DiagnosedAt = DateTime.UtcNow, OverallStatus = "Unknown", HealthScore = 50 };
        }
    }

    private PatternAnalysis ParsePatternAnalysis(string aiResponse, int daysBack)
    {
        try
        {
            var json = JsonSerializer.Deserialize<JsonElement>(aiResponse);
            return new PatternAnalysis
            {
                AnalyzedAt = DateTime.UtcNow,
                AnalysisPeriod = TimeSpan.FromDays(daysBack),
                Patterns = json.TryGetProperty("patterns", out var patterns)
                    ? JsonSerializer.Deserialize<List<ErrorPattern>>(patterns.GetRawText()) ?? new List<ErrorPattern>()
                    : new List<ErrorPattern>(),
                Trends = json.TryGetProperty("trends", out var trends)
                    ? JsonSerializer.Deserialize<List<TrendAnalysis>>(trends.GetRawText()) ?? new List<TrendAnalysis>()
                    : new List<TrendAnalysis>(),
                Correlations = json.TryGetProperty("correlations", out var corr)
                    ? JsonSerializer.Deserialize<List<Correlation>>(corr.GetRawText()) ?? new List<Correlation>()
                    : new List<Correlation>(),
                Summary = json.TryGetProperty("summary", out var summary) ? summary.GetString() : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing pattern analysis");
            return new PatternAnalysis { AnalyzedAt = DateTime.UtcNow, AnalysisPeriod = TimeSpan.FromDays(daysBack) };
        }
    }

    private RootCauseAnalysis ParseRootCauseAnalysis(string aiResponse, string problemDescription)
    {
        try
        {
            var json = JsonSerializer.Deserialize<JsonElement>(aiResponse);
            return new RootCauseAnalysis
            {
                AnalyzedAt = DateTime.UtcNow,
                ProblemDescription = problemDescription,
                RootCause = json.TryGetProperty("rootCause", out var rc) ? rc.GetString() : null,
                ContributingFactors = json.TryGetProperty("contributingFactors", out var cf)
                    ? JsonSerializer.Deserialize<List<string>>(cf.GetRawText()) ?? new List<string>()
                    : new List<string>(),
                Symptoms = json.TryGetProperty("symptoms", out var sym)
                    ? JsonSerializer.Deserialize<List<string>>(sym.GetRawText()) ?? new List<string>()
                    : new List<string>(),
                Fixes = json.TryGetProperty("fixes", out var fixes)
                    ? JsonSerializer.Deserialize<List<FixSuggestion>>(fixes.GetRawText()) ?? new List<FixSuggestion>()
                    : new List<FixSuggestion>(),
                Confidence = json.TryGetProperty("confidence", out var conf) ? conf.GetString() ?? "Medium" : "Medium",
                Evidence = json.TryGetProperty("evidence", out var ev) 
                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(ev.GetRawText())
                    : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing root cause analysis");
            return new RootCauseAnalysis { AnalyzedAt = DateTime.UtcNow, ProblemDescription = problemDescription };
        }
    }

    private List<DiagnosticAlert> ParseAlerts(string aiResponse)
    {
        try
        {
            return JsonSerializer.Deserialize<List<DiagnosticAlert>>(aiResponse) ?? new List<DiagnosticAlert>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing alerts");
            return new List<DiagnosticAlert>();
        }
    }

    private string ExtractExceptionType(string details)
    {
        if (string.IsNullOrEmpty(details)) return "Unknown";
        var lines = details.Split('\n');
        return lines.FirstOrDefault()?.Trim() ?? "Unknown";
    }

    private string? ExtractStackTrace(string details)
    {
        if (string.IsNullOrEmpty(details)) return null;
        var stackTraceStart = details.IndexOf("at ", StringComparison.Ordinal);
        return stackTraceStart >= 0 ? details.Substring(stackTraceStart) : null;
    }

    private string DetermineOverallStatus(List<AuditEvent> errors)
    {
        if (errors.Count == 0) return "Healthy";
        if (errors.Any(e => e.Severity == "Critical")) return "Critical";
        if (errors.Any(e => e.Severity == "High")) return "Degraded";
        return "Warning";
    }

    private int CalculateHealthScore(List<AuditEvent> errors)
    {
        if (errors.Count == 0) return 100;
        var criticalCount = errors.Count(e => e.Severity == "Critical");
        var highCount = errors.Count(e => e.Severity == "High");
        var mediumCount = errors.Count(e => e.Severity == "Medium");
        
        var score = 100 - (criticalCount * 20) - (highCount * 10) - (mediumCount * 5);
        return Math.Max(0, Math.Min(100, score));
    }

    private async Task<(string Status, string Message)> CheckDatabaseHealthAsync(CancellationToken cancellationToken)
    {
        try
        {
            var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
            if (canConnect)
            {
                return ("Healthy", "Database connection successful");
            }
            return ("Unhealthy", "Cannot connect to database");
        }
        catch (Exception ex)
        {
            return ("Unhealthy", $"Database health check failed: {ex.Message}");
        }
    }

    private async Task<Dictionary<string, object>> GetRecentPerformanceMetricsAsync(CancellationToken cancellationToken)
    {
        // Placeholder - implement actual performance metrics collection
        return new Dictionary<string, object>
        {
            { "avgResponseTime", "150ms" },
            { "requestCount", 1000 },
            { "errorRate", "0.5%" }
        };
    }

    #endregion
}
