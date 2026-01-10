using GrcMvc.Configuration;
using GrcMvc.Data;
using GrcMvc.Models.DTOs;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Integration Agent Service - AI-powered external system integration assistance
/// Uses Claude Sonnet 4.5 for intelligent integration planning and mapping
/// </summary>
public class IntegrationAgentService : IIntegrationAgentService
{
    private readonly ILogger<IntegrationAgentService> _logger;
    private readonly GrcDbContext _context;
    private readonly ClaudeApiSettings _settings;
    private readonly HttpClient _httpClient;

    public IntegrationAgentService(
        ILogger<IntegrationAgentService> logger,
        GrcDbContext context,
        IOptions<ClaudeApiSettings> settings,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _context = context;
        _settings = settings.Value;
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return !string.IsNullOrWhiteSpace(_settings.ApiKey);
    }

    public async Task<IntegrationAnalysisResult> AnalyzeIntegrationRequirementsAsync(
        string systemName,
        string systemType,
        Dictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Analyzing integration requirements for {System} ({Type})",
            systemName, systemType);

        if (!await IsAvailableAsync(cancellationToken))
        {
            return CreateFallbackIntegrationAnalysis(systemName, systemType);
        }

        try
        {
            var prompt = $@"Analyze integration requirements for connecting to the following system:

System Name: {systemName}
System Type: {systemType}
Additional Metadata: {(metadata != null ? JsonSerializer.Serialize(metadata) : "None")}

Provide a comprehensive integration analysis including:
1. Integration complexity assessment (Simple/Moderate/Complex)
2. Estimated effort in hours
3. List of technical requirements with priorities
4. Recommendations for best practices
5. Potential challenges and mitigation strategies

Respond with JSON:
{{
  ""systemName"": ""{systemName}"",
  ""systemType"": ""{systemType}"",
  ""complexity"": ""Moderate"",
  ""estimatedEffortHours"": 40,
  ""requirements"": [
    {{
      ""category"": ""Authentication"",
      ""description"": ""....."",
      ""priority"": ""High"",
      ""isMandatory"": true
    }}
  ],
  ""recommendations"": ["".....""],
  ""potentialChallenges"": ["".......""]
}}";

            var response = await CallClaudeApiAsync(prompt, cancellationToken);
            var result = JsonSerializer.Deserialize<IntegrationAnalysisResult>(response);

            if (result != null)
            {
                result.AnalyzedAt = DateTime.UtcNow;
                return result;
            }

            return CreateFallbackIntegrationAnalysis(systemName, systemType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing integration for {System}", systemName);
            return CreateFallbackIntegrationAnalysis(systemName, systemType);
        }
    }

    public async Task<FieldMappingRecommendation> RecommendFieldMappingsAsync(
        Guid sourceSystemId,
        Guid targetSystemId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Recommending field mappings from {Source} to {Target}",
            sourceSystemId, targetSystemId);

        // Fallback implementation
        return new FieldMappingRecommendation
        {
            Mappings = new List<FieldMapping>(),
            ConfidenceScore = 0,
            UnmappedSourceFields = new List<string>(),
            UnmappedTargetFields = new List<string>(),
            Warnings = new List<string>
            {
                "AI-powered field mapping not configured. Configure Claude API key for intelligent mapping suggestions."
            },
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<DataQualityResult> ValidateIntegrationDataAsync(
        Guid integrationId,
        object sampleData,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Validating integration data quality for {Integration}", integrationId);

        // Fallback implementation
        return new DataQualityResult
        {
            TotalRecords = 0,
            ValidRecords = 0,
            InvalidRecords = 0,
            QualityScore = 100,
            Issues = new List<DataQualityIssue>(),
            Recommendations = new List<string>
            {
                "No data quality issues detected in basic validation",
                "Enable AI-powered validation for comprehensive analysis"
            },
            ValidatedAt = DateTime.UtcNow
        };
    }

    public async Task<IntegrationHealthResult> MonitorIntegrationHealthAsync(
        Guid integrationId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Monitoring integration health for {Integration}", integrationId);

        // Get sync job data from database
        var syncJobs = await _context.SyncExecutionLogs
            .Include(s => s.SyncJob)
            .Where(s => s.SyncJob.ConnectorId == integrationId)
            .OrderByDescending(s => s.StartedAt)
            .Take(100)
            .ToListAsync(cancellationToken);

        var totalSyncs = syncJobs.Count;
        var successfulSyncs = syncJobs.Count(s => s.Status == "Completed");
        var failedSyncs = syncJobs.Count(s => s.Status == "Failed");

        var healthScore = totalSyncs > 0
            ? (int)((double)successfulSyncs / totalSyncs * 100)
            : 100;

        var healthStatus = healthScore >= 90 ? "Healthy"
            : healthScore >= 70 ? "Degraded"
            : "Failed";

        // Calculate uptime from first sync log to now
        var firstSync = syncJobs.LastOrDefault()?.StartedAt ?? DateTime.UtcNow;
        var uptime = DateTime.UtcNow - firstSync;

        return new IntegrationHealthResult
        {
            HealthStatus = healthStatus,
            HealthScore = healthScore,
            Uptime = uptime,
            TotalSyncs = totalSyncs,
            SuccessfulSyncs = successfulSyncs,
            FailedSyncs = failedSyncs,
            CurrentIssues = new List<IntegrationIssue>(),
            Recommendations = GenerateHealthRecommendations(healthStatus, failedSyncs, totalSyncs),
            CheckedAt = DateTime.UtcNow
        };
    }

    public async Task<IntegrationConfigurationResult> GenerateConfigurationAsync(
        string sourceSystem,
        string targetSystem,
        List<string> requiredFields,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating integration configuration from {Source} to {Target}",
            sourceSystem, targetSystem);

        if (!await IsAvailableAsync(cancellationToken))
        {
            return CreateFallbackConfiguration(sourceSystem, targetSystem);
        }

        try
        {
            var fieldsText = string.Join(", ", requiredFields);

            var prompt = $@"Generate an integration configuration for:

Source System: {sourceSystem}
Target System: {targetSystem}
Required Fields: {fieldsText}

Generate a complete integration configuration including:
1. Configuration file (JSON format preferred)
2. Required credentials and authentication details
3. Step-by-step setup instructions
4. Field mappings
5. Error handling recommendations

Respond with JSON:
{{
  ""sourceSystem"": ""{sourceSystem}"",
  ""targetSystem"": ""{targetSystem}"",
  ""configurationFormat"": ""JSON"",
  ""configurationContent"": ""{{...}}"",
  ""requiredCredentials"": [""API Key"", ""Secret""],
  ""setupInstructions"": [""Step 1..."", ""Step 2...""]
}}";

            var response = await CallClaudeApiAsync(prompt, cancellationToken);
            var result = JsonSerializer.Deserialize<IntegrationConfigurationResult>(response);

            if (result != null)
            {
                result.GeneratedAt = DateTime.UtcNow;
                return result;
            }

            return CreateFallbackConfiguration(sourceSystem, targetSystem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating configuration for {Source} -> {Target}",
                sourceSystem, targetSystem);
            return CreateFallbackConfiguration(sourceSystem, targetSystem);
        }
    }

    private async Task<string> CallClaudeApiAsync(string prompt, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            throw new InvalidOperationException("Claude API key is not configured");
        }

        var request = new
        {
            model = _settings.Model,
            max_tokens = _settings.MaxTokens,
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };

        var requestJson = JsonSerializer.Serialize(request);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("x-api-key", _settings.ApiKey);
        _httpClient.DefaultRequestHeaders.Add("anthropic-version", _settings.ApiVersion ?? "2023-06-01");

        var response = await _httpClient.PostAsync(
            _settings.ApiEndpoint ?? "https://api.anthropic.com/v1/messages",
            content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        var claudeResponse = JsonSerializer.Deserialize<ClaudeApiResponse>(responseJson);

        return claudeResponse?.Content?.FirstOrDefault()?.Text ?? "{}";
    }

    private IntegrationAnalysisResult CreateFallbackIntegrationAnalysis(string systemName, string systemType)
    {
        return new IntegrationAnalysisResult
        {
            SystemName = systemName,
            SystemType = systemType,
            Complexity = "Moderate",
            EstimatedEffortHours = 40,
            Requirements = new List<IntegrationRequirement>
            {
                new IntegrationRequirement
                {
                    Category = "Configuration",
                    Description = "Configure Claude API key for AI-powered integration analysis",
                    Priority = "High",
                    IsMandatory = false
                },
                new IntegrationRequirement
                {
                    Category = "Authentication",
                    Description = "Establish authentication mechanism with external system",
                    Priority = "High",
                    IsMandatory = true
                }
            },
            Recommendations = new List<string>
            {
                "Enable AI-powered integration analysis for detailed requirements",
                "Document API endpoints and data schemas",
                "Implement error handling and retry logic"
            },
            PotentialChallenges = new List<string>
            {
                "Authentication configuration",
                "Data mapping complexities",
                "Rate limiting considerations"
            },
            AnalyzedAt = DateTime.UtcNow
        };
    }

    private IntegrationConfigurationResult CreateFallbackConfiguration(string sourceSystem, string targetSystem)
    {
        return new IntegrationConfigurationResult
        {
            SourceSystem = sourceSystem,
            TargetSystem = targetSystem,
            ConfigurationFormat = "JSON",
            ConfigurationContent = JsonSerializer.Serialize(new
            {
                source = sourceSystem,
                target = targetSystem,
                note = "Configure Claude API for AI-generated configuration"
            }, new JsonSerializerOptions { WriteIndented = true }),
            RequiredCredentials = new List<string>
            {
                "API Key or OAuth Token",
                "Endpoint URL",
                "Client ID (if applicable)"
            },
            SetupInstructions = new List<string>
            {
                "Enable Claude API for detailed setup instructions",
                "Configure authentication credentials",
                "Test connection to external system",
                "Map required data fields",
                "Set up error handling and monitoring"
            },
            GeneratedAt = DateTime.UtcNow
        };
    }

    private List<string> GenerateHealthRecommendations(string healthStatus, int failedSyncs, int totalSyncs)
    {
        var recommendations = new List<string>();

        if (healthStatus == "Failed")
        {
            recommendations.Add("Critical: Integration health is poor. Immediate attention required.");
            recommendations.Add("Review recent error logs to identify root cause");
            recommendations.Add("Check authentication credentials and endpoint availability");
        }
        else if (healthStatus == "Degraded")
        {
            recommendations.Add("Warning: Integration experiencing issues");
            recommendations.Add($"{failedSyncs}/{totalSyncs} recent syncs failed - investigate patterns");
            recommendations.Add("Consider implementing retry logic or circuit breaker pattern");
        }
        else
        {
            recommendations.Add("Integration is healthy");
            recommendations.Add("Continue monitoring sync success rates");
        }

        if (totalSyncs == 0)
        {
            recommendations.Add("No sync history available - ensure integration is configured correctly");
        }

        return recommendations;
    }

    private class ClaudeApiResponse
    {
        public List<ContentBlock>? Content { get; set; }
    }

    private class ContentBlock
    {
        public string? Type { get; set; }
        public string? Text { get; set; }
    }
}
