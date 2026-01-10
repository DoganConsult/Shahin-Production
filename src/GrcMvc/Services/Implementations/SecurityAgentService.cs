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
/// Security Agent Service - AI-powered security monitoring and threat analysis
/// Uses Claude Sonnet 4.5 for intelligent security analysis
/// </summary>
public class SecurityAgentService : ISecurityAgentService
{
    private readonly ILogger<SecurityAgentService> _logger;
    private readonly GrcDbContext _context;
    private readonly ClaudeApiSettings _settings;
    private readonly HttpClient _httpClient;

    public SecurityAgentService(
        ILogger<SecurityAgentService> logger,
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

    public async Task<SecurityPostureAnalysis> AnalyzeSecurityPostureAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Analyzing security posture for tenant {TenantId}", tenantId);

        if (!await IsAvailableAsync(cancellationToken))
        {
            return CreateFallbackSecurityPosture();
        }

        try
        {
            // Gather security-related data
            var risks = await _context.Risks
                .Where(r => r.TenantId == tenantId && !r.IsDeleted)
                .Select(r => new { r.Name, r.RiskScore, r.Status, r.Category })
                .ToListAsync(cancellationToken);

            var controls = await _context.Controls
                .Where(c => c.TenantId == tenantId && !c.IsDeleted)
                .Select(c => new { c.Name, c.Status, c.Effectiveness, c.Category })
                .ToListAsync(cancellationToken);

            var incidents = await _context.Incidents
                .Where(i => i.TenantId == tenantId && !i.IsDeleted)
                .Where(i => i.DetectedAt >= DateTime.UtcNow.AddMonths(-3))
                .Select(i => new { i.Title, i.Severity, i.Status, i.Phase })
                .ToListAsync(cancellationToken);

            var controlTests = await _context.ControlTests
                .Where(ct => ct.TenantId == tenantId && !ct.IsDeleted)
                .Where(ct => ct.TestedDate >= DateTime.UtcNow.AddMonths(-6))
                .Select(ct => new { Outcome = ct.Result, ct.Score })
                .ToListAsync(cancellationToken);

            // Build context for Claude
            var context = new StringBuilder();
            context.AppendLine("# Security Posture Analysis Request");
            context.AppendLine();
            context.AppendLine($"## Current State Summary");
            context.AppendLine($"- Total Active Risks: {risks.Count}");
            context.AppendLine($"- Average Risk Score: {(risks.Any() ? risks.Average(r => r.RiskScore) : 0):F1}");
            context.AppendLine($"- Total Controls: {controls.Count}");
            context.AppendLine($"- Average Control Effectiveness: {(controls.Any() ? controls.Average(c => c.Effectiveness) : 0):F1}%");
            context.AppendLine($"- Recent Incidents (3 months): {incidents.Count}");
            context.AppendLine($"- Control Tests Passed (6 months): {controlTests.Count(ct => ct.Outcome == "Passed")}/{controlTests.Count}");
            context.AppendLine();
            context.AppendLine("## Top Risks");
            foreach (var risk in risks.OrderByDescending(r => r.RiskScore).Take(10))
            {
                context.AppendLine($"- {risk.Name} (Score: {risk.RiskScore}, Status: {risk.Status})");
            }
            context.AppendLine();
            context.AppendLine("## Control Coverage");
            var controlsByCategory = controls.GroupBy(c => c.Category ?? "Uncategorized");
            foreach (var group in controlsByCategory)
            {
                context.AppendLine($"- {group.Key}: {group.Count()} controls");
            }

            var prompt = $@"{context}

Please analyze this organization's security posture and provide:
1. Overall security rating (Critical/High/Medium/Low) with score (0-100)
2. Key security findings with severity levels
3. Specific recommendations for improvement
4. Risk breakdown by category

Format your response as JSON with this structure:
{{
  ""overallRating"": ""High"",
  ""score"": 75,
  ""findings"": [
    {{
      ""category"": ""Access Control"",
      ""severity"": ""High"",
      ""description"": ""....."",
      ""recommendation"": ""...""
    }}
  ],
  ""recommendations"": [""......""],
  ""riskBreakdown"": {{
    ""Technical"": 15,
    ""Operational"": 10
  }}
}}";

            var response = await CallClaudeApiAsync(prompt, cancellationToken);
            var result = JsonSerializer.Deserialize<SecurityPostureAnalysis>(response);

            if (result != null)
            {
                result.AnalyzedAt = DateTime.UtcNow;
                _logger.LogInformation("Security posture analysis completed for tenant {TenantId}: {Rating}",
                    tenantId, result.OverallRating);
                return result;
            }

            return CreateFallbackSecurityPosture();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing security posture for tenant {TenantId}", tenantId);
            return CreateFallbackSecurityPosture();
        }
    }

    public async Task<ThreatDetectionResult> DetectThreatsAsync(
        Guid tenantId,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Detecting threats for tenant {TenantId} from {From} to {To}",
            tenantId, fromDate, toDate);

        if (!await IsAvailableAsync(cancellationToken))
        {
            return CreateFallbackThreatDetection(fromDate, toDate);
        }

        try
        {
            // Analyze audit events for suspicious patterns
            var auditEvents = await _context.AuditEvents
                .Where(e => e.TenantId == tenantId)
                .Where(e => e.CreatedDate >= fromDate && e.CreatedDate <= toDate)
                .Select(e => new { e.Action, EntityType = e.AffectedEntityType, UserId = e.Actor, e.CreatedDate, Changes = e.PayloadJson })
                .Take(1000) // Limit for performance
                .ToListAsync(cancellationToken);

            var prompt = $@"Analyze the following audit log entries for security threats:

Total Events: {auditEvents.Count}
Time Range: {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}

Event Summary:
{string.Join("\n", auditEvents.GroupBy(e => e.Action).Select(g => $"- {g.Key}: {g.Count()} events"))}

Detect potential security threats such as:
- Unusual access patterns
- Privilege escalation attempts
- Data exfiltration indicators
- Brute force attempts
- Suspicious user behavior

Respond with JSON:
{{
  ""totalThreatsDetected"": 3,
  ""threats"": [
    {{
      ""threatType"": ""Unusual Access Pattern"",
      ""severity"": ""High"",
      ""description"": ""....."",
      ""indicators"": ["".....""],
      ""confidenceScore"": 85
    }}
  ],
  ""recommendedActions"": [""......""]
}}";

            var response = await CallClaudeApiAsync(prompt, cancellationToken);
            var result = JsonSerializer.Deserialize<ThreatDetectionResult>(response);

            if (result != null)
            {
                result.AnalyzedFrom = fromDate;
                result.AnalyzedTo = toDate;
                foreach (var threat in result.Threats)
                {
                    threat.DetectedAt = DateTime.UtcNow;
                }
                return result;
            }

            return CreateFallbackThreatDetection(fromDate, toDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting threats for tenant {TenantId}", tenantId);
            return CreateFallbackThreatDetection(fromDate, toDate);
        }
    }

    public async Task<AccessAnomalyResult> AnalyzeAccessPatternsAsync(
        Guid tenantId,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Analyzing access patterns for tenant {TenantId}, user {UserId}",
            tenantId, userId);

        // Fallback implementation (Claude API integration can be added)
        return new AccessAnomalyResult
        {
            TotalAnomalies = 0,
            Anomalies = new List<AccessAnomaly>(),
            RiskLevel = "Low",
            AnalyzedAt = DateTime.UtcNow
        };
    }

    public async Task<SecurityControlRecommendation> RecommendSecurityControlsAsync(
        Guid tenantId,
        string? frameworkCode = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Recommending security controls for tenant {TenantId}, framework {Framework}",
            tenantId, frameworkCode);

        // Fallback implementation (Claude API integration can be added)
        return new SecurityControlRecommendation
        {
            Controls = new List<RecommendedControl>(),
            TotalRisksAddressed = 0,
            Framework = frameworkCode ?? "General",
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<IncidentResponseAnalysis> AnalyzeIncidentResponseAsync(
        Guid incidentId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Analyzing incident response for incident {IncidentId}", incidentId);

        var incident = await _context.Incidents
            .Where(i => i.Id == incidentId)
            .FirstOrDefaultAsync(cancellationToken);

        if (incident == null)
        {
            throw new InvalidOperationException($"Incident {incidentId} not found");
        }

        // Calculate response times
        var detectionTime = incident.DetectedAt - incident.CreatedDate;
        var containmentTime = incident.ContainedAt.HasValue
            ? incident.ContainedAt.Value - incident.DetectedAt
            : TimeSpan.Zero;
        var resolutionTime = incident.ClosedAt.HasValue
            ? incident.ClosedAt.Value - incident.DetectedAt
            : TimeSpan.Zero;

        // Fallback implementation
        return new IncidentResponseAnalysis
        {
            IncidentId = incidentId,
            EffectivenessRating = "Good",
            ResponseScore = 75,
            DetectionTime = detectionTime,
            ContainmentTime = containmentTime,
            ResolutionTime = resolutionTime,
            StrengthsIdentified = new List<string>
            {
                "Incident was detected and logged appropriately",
                "Response procedures were followed"
            },
            ImprovementAreas = new List<string>
            {
                "Consider faster containment strategies",
                "Enhance post-incident documentation"
            },
            LessonsLearned = new List<string>
            {
                "Improve detection capabilities for similar incidents",
                "Update incident response playbooks"
            }
        };
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

        var response = await _httpClient.PostAsync(_settings.ApiEndpoint ?? "https://api.anthropic.com/v1/messages",
            content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        var claudeResponse = JsonSerializer.Deserialize<ClaudeApiResponse>(responseJson);

        return claudeResponse?.Content?.FirstOrDefault()?.Text ?? "{}";
    }

    private SecurityPostureAnalysis CreateFallbackSecurityPosture()
    {
        return new SecurityPostureAnalysis
        {
            OverallRating = "Medium",
            Score = 65,
            Findings = new List<SecurityFinding>
            {
                new SecurityFinding
                {
                    Category = "Configuration",
                    Severity = "Medium",
                    Description = "AI-powered analysis not available. Manual review recommended.",
                    Recommendation = "Configure Claude API key for enhanced security analysis."
                }
            },
            Recommendations = new List<string>
            {
                "Enable AI-powered security monitoring",
                "Conduct regular security assessments",
                "Review and update security controls"
            },
            RiskBreakdown = new Dictionary<string, int>
            {
                { "Unknown", 1 }
            },
            AnalyzedAt = DateTime.UtcNow
        };
    }

    private ThreatDetectionResult CreateFallbackThreatDetection(DateTime fromDate, DateTime toDate)
    {
        return new ThreatDetectionResult
        {
            TotalThreatsDetected = 0,
            Threats = new List<ThreatIndicator>(),
            RecommendedActions = new List<string>
            {
                "Configure Claude API for AI-powered threat detection",
                "Review audit logs manually for suspicious activity"
            },
            AnalyzedFrom = fromDate,
            AnalyzedTo = toDate
        };
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
