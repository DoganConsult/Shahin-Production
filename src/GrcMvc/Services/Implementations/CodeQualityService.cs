using System.Text;
using System.Text.Json;
using GrcMvc.Agents;
using GrcMvc.Data;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Code quality analysis service using Claude sub-agents
/// </summary>
public class CodeQualityService : ICodeQualityService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CodeQualityService> _logger;
    private readonly IAlertService _alertService;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;
    private readonly GrcDbContext _dbContext;

    private readonly string _claudeApiKey;
    private readonly string _claudeApiUrl = "https://api.anthropic.com/v1/messages";
    private readonly string _claudeModel = "claude-sonnet-4-20250514";

    public CodeQualityService(
        HttpClient httpClient,
        ILogger<CodeQualityService> logger,
        IAlertService alertService,
        IConfiguration configuration,
        IMemoryCache cache,
        GrcDbContext dbContext)
    {
        _httpClient = httpClient;
        _logger = logger;
        _alertService = alertService;
        _configuration = configuration;
        _cache = cache;
        _dbContext = dbContext;

        _claudeApiKey = configuration["Claude:ApiKey"] ??
                        configuration["ClaudeAgents:ApiKey"] ??
                        Environment.GetEnvironmentVariable("CLAUDE_API_KEY") ?? "";

        _claudeModel = configuration["Claude:Model"] ?? _claudeModel;
    }

    public async Task<CodeAnalysisResult> AnalyzeCodeAsync(CodeAnalysisRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting code analysis with agent: {AgentType} for file: {FilePath}",
            request.AgentType, request.FilePath ?? "inline");

        try
        {
            var prompt = GetPromptForAgentType(request.AgentType);
            var systemPrompt = $"{prompt}\n\nAnalyze the following code and return ONLY valid JSON:";

            var userMessage = new StringBuilder();
            if (!string.IsNullOrEmpty(request.FilePath))
                userMessage.AppendLine($"File: {request.FilePath}");
            if (!string.IsNullOrEmpty(request.Language))
                userMessage.AppendLine($"Language: {request.Language}");
            if (!string.IsNullOrEmpty(request.Context))
                userMessage.AppendLine($"Context: {request.Context}");
            userMessage.AppendLine("\n```");
            userMessage.AppendLine(request.Code);
            userMessage.AppendLine("```");

            var response = await CallClaudeApiAsync(systemPrompt, userMessage.ToString(), cancellationToken);
            var result = ParseAnalysisResponse(response, request);

            // Check if alert should be triggered
            if (request.SendAlerts && await _alertService.ShouldTriggerAlertAsync(result))
            {
                var alert = _alertService.CreateAlert(result);
                result.AlertTriggered = true;
                result.AlertMessage = alert.Message;
                await _alertService.SendAlertAsync(alert, cancellationToken);
            }

            // Cache result
            var cacheKey = $"analysis_{request.AgentType}_{request.FilePath?.GetHashCode() ?? request.Code.GetHashCode()}";
            _cache.Set(cacheKey, result, TimeSpan.FromHours(1));

            _logger.LogInformation("Code analysis completed. Score: {Score}, Issues: {IssueCount}",
                result.Score, result.Issues.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during code analysis with agent {AgentType}", request.AgentType);
            throw;
        }
    }

    public async Task<List<CodeAnalysisResult>> RunFullScanAsync(string code, string? filePath = null, CancellationToken cancellationToken = default)
    {
        var results = new List<CodeAnalysisResult>();

        var agentTypes = new[]
        {
            CodeQualityAgentConfig.AgentTypes.CodeReviewer,
            CodeQualityAgentConfig.AgentTypes.SecurityScanner,
            CodeQualityAgentConfig.AgentTypes.PerformanceAnalyzer,
            CodeQualityAgentConfig.AgentTypes.ArchitectureAnalyzer
        };

        // Run agents in parallel
        var tasks = agentTypes.Select(agentType => AnalyzeCodeAsync(new CodeAnalysisRequest
        {
            AgentType = agentType,
            Code = code,
            FilePath = filePath,
            SendAlerts = true
        }, cancellationToken));

        var taskResults = await Task.WhenAll(tasks);
        results.AddRange(taskResults);

        return results;
    }

    public async Task<CodeAnalysisResult> AnalyzeFileAsync(string filePath, string agentType, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        var code = await File.ReadAllTextAsync(filePath, cancellationToken);
        var language = GetLanguageFromExtension(Path.GetExtension(filePath));

        return await AnalyzeCodeAsync(new CodeAnalysisRequest
        {
            AgentType = agentType,
            Code = code,
            FilePath = filePath,
            Language = language,
            SendAlerts = true
        }, cancellationToken);
    }

    public async Task<List<CodeAnalysisResult>> AnalyzeDirectoryAsync(string directoryPath, string[] filePatterns, CancellationToken cancellationToken = default)
    {
        var results = new List<CodeAnalysisResult>();

        if (!Directory.Exists(directoryPath))
            throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");

        var files = new List<string>();
        foreach (var pattern in filePatterns)
        {
            files.AddRange(Directory.GetFiles(directoryPath, pattern, SearchOption.AllDirectories));
        }

        _logger.LogInformation("Analyzing {FileCount} files in {Directory}", files.Count, directoryPath);

        // Analyze files in batches to avoid overwhelming the API
        var batchSize = 5;
        for (int i = 0; i < files.Count; i += batchSize)
        {
            var batch = files.Skip(i).Take(batchSize);
            var tasks = batch.Select(file => AnalyzeFileAsync(file, CodeQualityAgentConfig.AgentTypes.CodeReviewer, cancellationToken));

            try
            {
                var batchResults = await Task.WhenAll(tasks);
                results.AddRange(batchResults);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error analyzing batch starting at index {Index}", i);
            }

            // Rate limiting delay
            if (i + batchSize < files.Count)
                await Task.Delay(1000, cancellationToken);
        }

        return results;
    }

    public Task<List<CodeAnalysisResult>> GetAnalysisHistoryAsync(string? filePath = null, DateTime? from = null, DateTime? to = null)
    {
        // This would typically come from a database
        // For now, return empty list - implement with database storage
        return Task.FromResult(new List<CodeAnalysisResult>());
    }

    public async Task<CodeQualityMetrics> GetQualityMetricsAsync(DateTime? from = null, DateTime? to = null)
    {
        var history = await GetAnalysisHistoryAsync(null, from, to);

        return new CodeQualityMetrics
        {
            GeneratedAt = DateTime.UtcNow,
            TotalFilesAnalyzed = history.Count,
            TotalIssuesFound = history.Sum(r => r.Issues.Count),
            CriticalIssues = history.Sum(r => r.Issues.Count(i => i.Severity == "critical")),
            HighIssues = history.Sum(r => r.Issues.Count(i => i.Severity == "high")),
            MediumIssues = history.Sum(r => r.Issues.Count(i => i.Severity == "medium")),
            LowIssues = history.Sum(r => r.Issues.Count(i => i.Severity == "low")),
            AverageQualityScore = history.Any() ? history.Average(r => r.Score) : 0,
            WorstFiles = history
                .OrderBy(r => r.Score)
                .Take(10)
                .Select(r => new FileQualitySummary
                {
                    FilePath = r.FilePath,
                    Score = r.Score,
                    IssueCount = r.Issues.Count,
                    TopIssue = r.Issues.FirstOrDefault()?.Description ?? "None"
                })
                .ToList()
        };
    }

    private async Task<string> CallClaudeApiAsync(string systemPrompt, string userMessage, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_claudeApiKey))
        {
            _logger.LogWarning("Claude API key not configured, returning mock response");
            return GetMockResponse();
        }

        var request = new
        {
            model = _claudeModel,
            max_tokens = 4096,
            system = systemPrompt,
            messages = new[]
            {
                new { role = "user", content = userMessage }
            }
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, _claudeApiUrl)
        {
            Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
        };

        httpRequest.Headers.Add("x-api-key", _claudeApiKey);
        httpRequest.Headers.Add("anthropic-version", "2023-06-01");

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var responseJson = JsonDocument.Parse(responseContent);

        // Extract text from Claude response
        var content = responseJson.RootElement
            .GetProperty("content")
            .EnumerateArray()
            .FirstOrDefault()
            .GetProperty("text")
            .GetString();

        return content ?? "";
    }

    private string GetMockResponse()
    {
        return JsonSerializer.Serialize(new
        {
            severity = "medium",
            score = 75,
            summary = "Code analysis completed (mock response - API key not configured)",
            issues = new[]
            {
                new
                {
                    line = 1,
                    type = "info",
                    severity = "info",
                    description = "Configure Claude API key for real analysis",
                    suggestion = "Set CLAUDE_API_KEY environment variable"
                }
            },
            recommendations = new[] { "Configure Claude API key for production use" }
        });
    }

    private CodeAnalysisResult ParseAnalysisResponse(string response, CodeAnalysisRequest request)
    {
        try
        {
            // Extract JSON from response (it might be wrapped in markdown code blocks)
            var jsonStart = response.IndexOf('{');
            var jsonEnd = response.LastIndexOf('}');
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                response = response.Substring(jsonStart, jsonEnd - jsonStart + 1);
            }

            using var doc = JsonDocument.Parse(response);
            var root = doc.RootElement;

            var result = new CodeAnalysisResult
            {
                AgentType = request.AgentType,
                FilePath = request.FilePath ?? "inline",
                AnalyzedAt = DateTime.UtcNow,
                Severity = root.TryGetProperty("severity", out var sev) ? sev.GetString() ?? "info" : "info",
                Score = root.TryGetProperty("score", out var score) ? score.GetInt32() :
                       root.TryGetProperty("riskScore", out var risk) ? 100 - risk.GetInt32() :
                       root.TryGetProperty("performanceScore", out var perf) ? perf.GetInt32() :
                       root.TryGetProperty("architectureScore", out var arch) ? arch.GetInt32() : 50,
                Summary = root.TryGetProperty("summary", out var sum) ? sum.GetString() ?? "" : ""
            };

            // Parse issues array
            if (root.TryGetProperty("issues", out var issues) || root.TryGetProperty("vulnerabilities", out issues) || root.TryGetProperty("violations", out issues))
            {
                foreach (var issue in issues.EnumerateArray())
                {
                    result.Issues.Add(new CodeIssue
                    {
                        Line = issue.TryGetProperty("line", out var line) ? line.GetInt32() : null,
                        Type = issue.TryGetProperty("type", out var type) ? type.GetString() ?? "" : "",
                        Severity = issue.TryGetProperty("severity", out var isev) ? isev.GetString() ?? "info" : "info",
                        Description = issue.TryGetProperty("description", out var desc) ? desc.GetString() ?? "" : "",
                        Suggestion = issue.TryGetProperty("suggestion", out var sugg) ? sugg.GetString() :
                                    issue.TryGetProperty("remediation", out var rem) ? rem.GetString() :
                                    issue.TryGetProperty("recommendation", out var rec) ? rec.GetString() : null,
                        CweId = issue.TryGetProperty("cwe", out var cwe) ? cwe.GetString() : null
                    });
                }
            }

            // Parse recommendations
            if (root.TryGetProperty("recommendations", out var recs))
            {
                foreach (var rec in recs.EnumerateArray())
                {
                    if (rec.ValueKind == JsonValueKind.String)
                        result.Recommendations.Add(rec.GetString() ?? "");
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse Claude response, returning default result");
            return new CodeAnalysisResult
            {
                AgentType = request.AgentType,
                FilePath = request.FilePath ?? "inline",
                AnalyzedAt = DateTime.UtcNow,
                Severity = "info",
                Score = 50,
                Summary = "Analysis completed but response parsing failed",
                Issues = new List<CodeIssue>
                {
                    new CodeIssue
                    {
                        Type = "parse_error",
                        Severity = "info",
                        Description = $"Could not parse analysis response: {ex.Message}"
                    }
                }
            };
        }
    }

    private string GetPromptForAgentType(string agentType) => agentType switch
    {
        CodeQualityAgentConfig.AgentTypes.CodeReviewer => CodeQualityAgentConfig.AgentPrompts.CodeReviewerPrompt,
        CodeQualityAgentConfig.AgentTypes.SecurityScanner => CodeQualityAgentConfig.AgentPrompts.SecurityScannerPrompt,
        CodeQualityAgentConfig.AgentTypes.PerformanceAnalyzer => CodeQualityAgentConfig.AgentPrompts.PerformanceAnalyzerPrompt,
        CodeQualityAgentConfig.AgentTypes.DependencyChecker => CodeQualityAgentConfig.AgentPrompts.DependencyCheckerPrompt,
        CodeQualityAgentConfig.AgentTypes.ArchitectureAnalyzer => CodeQualityAgentConfig.AgentPrompts.ArchitectureAnalyzerPrompt,
        _ => CodeQualityAgentConfig.AgentPrompts.CodeReviewerPrompt
    };

    private string GetLanguageFromExtension(string extension) => extension.ToLower() switch
    {
        ".cs" => "C#",
        ".js" => "JavaScript",
        ".ts" => "TypeScript",
        ".py" => "Python",
        ".java" => "Java",
        ".go" => "Go",
        ".rs" => "Rust",
        ".rb" => "Ruby",
        ".php" => "PHP",
        ".sql" => "SQL",
        ".html" => "HTML",
        ".css" => "CSS",
        ".razor" => "Razor/Blazor",
        ".cshtml" => "Razor/MVC",
        _ => "Unknown"
    };
}
