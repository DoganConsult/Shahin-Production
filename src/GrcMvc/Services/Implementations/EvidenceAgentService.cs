using GrcMvc.Configuration;
using GrcMvc.Data;
using GrcMvc.Services.Interfaces;
using GrcMvc.Common.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Evidence Agent Service - AI-powered evidence analysis and recommendations
/// Uses Claude Sonnet 4.5 for intelligent evidence analysis
/// </summary>
public class EvidenceAgentService : IEvidenceAgentService
{
    private readonly ILogger<EvidenceAgentService> _logger;
    private readonly GrcDbContext _context;
    private readonly ClaudeApiSettings _settings;
    private readonly HttpClient _httpClient;

    public EvidenceAgentService(
        ILogger<EvidenceAgentService> logger,
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

    public async Task<Result<EvidenceQualityAnalysis>> AnalyzeEvidenceQualityAsync(
        Guid evidenceId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Analyzing evidence quality for evidence {EvidenceId}", evidenceId);

        var evidence = await _context.Evidences
            .Include(e => e.AssessmentRequirement)
            .FirstOrDefaultAsync(e => e.Id == evidenceId, cancellationToken);

        if (evidence == null)
        {
            return Result<EvidenceQualityAnalysis>.Failure(Error.NotFound("Evidence", evidenceId));
        }

        if (!await IsAvailableAsync(cancellationToken))
        {
            return Result<EvidenceQualityAnalysis>.Success(CreateFallbackQualityAnalysis(evidenceId));
        }

        try
        {
            var prompt = $@"Analyze the quality of this compliance evidence:

Title: {evidence.Title}
Type: {evidence.Type}
Description: {evidence.Description}
Verification Status: {evidence.VerificationStatus}
Created: {evidence.CreatedDate:yyyy-MM-dd}
Collection Date: {evidence.CollectionDate:yyyy-MM-dd}
File: {evidence.FileName} ({evidence.FileSize} bytes)

Linked Requirement: {evidence.AssessmentRequirement?.RequirementText ?? "None"}

Evaluate the evidence quality across these dimensions:
1. Completeness - Is the evidence comprehensive?
2. Relevance - Does it address the requirement?
3. Currency - Is it recent enough?
4. Authenticity - Does it appear legitimate?
5. Traceability - Can it be verified?

Respond with JSON:
{{
  ""qualityRating"": ""Good"",
  ""qualityScore"": 75,
  ""findings"": [
    {{
      ""dimension"": ""Completeness"",
      ""severity"": ""Medium"",
      ""description"": ""....."",
      ""recommendation"": ""...""
    }}
  ],
  ""recommendations"": [""......""],
  ""dimensionScores"": {{
    ""Completeness"": 80,
    ""Relevance"": 85,
    ""Currency"": 70,
    ""Authenticity"": 75,
    ""Traceability"": 65
  }}
}}";

            var responseResult = await CallClaudeApiAsync(prompt, cancellationToken);
            if (!responseResult.IsSuccess)
            {
                return Result<EvidenceQualityAnalysis>.Success(CreateFallbackQualityAnalysis(evidenceId));
            }
            
            var result = JsonSerializer.Deserialize<EvidenceQualityAnalysis>(responseResult.Value!);

            if (result != null)
            {
                result.EvidenceId = evidenceId;
                result.AnalyzedAt = DateTime.UtcNow;
                _logger.LogInformation("Evidence quality analysis completed for {EvidenceId}: {Rating}",
                    evidenceId, result.QualityRating);
                return Result<EvidenceQualityAnalysis>.Success(result);
            }

            return Result<EvidenceQualityAnalysis>.Success(CreateFallbackQualityAnalysis(evidenceId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing evidence quality for {EvidenceId}", evidenceId);
            return Result<EvidenceQualityAnalysis>.Success(CreateFallbackQualityAnalysis(evidenceId));
        }
    }

    public async Task<EvidenceGapAnalysis> DetectEvidenceGapsAsync(
        Guid tenantId,
        string? frameworkCode = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Detecting evidence gaps for tenant {TenantId}, framework {Framework}",
            tenantId, frameworkCode);

        // Get controls for the tenant
        var controlsQuery = _context.Controls
            .Where(c => c.TenantId == tenantId && !c.IsDeleted);

        if (!string.IsNullOrWhiteSpace(frameworkCode))
        {
            controlsQuery = controlsQuery.Where(c => c.SourceFrameworkCode == frameworkCode);
        }

        var controls = await controlsQuery
            .Select(c => new { c.Id, c.ControlCode, c.Name, c.Category, EvidenceCount = c.Evidences.Count })
            .ToListAsync(cancellationToken);

        var controlsWithEvidence = controls.Count(c => c.EvidenceCount > 0);
        var controlsWithoutEvidence = controls.Count(c => c.EvidenceCount == 0);

        var gaps = controls
            .Where(c => c.EvidenceCount == 0)
            .Select(c => new EvidenceGap
            {
                ControlId = c.Id,
                ControlCode = c.ControlCode ?? c.Id.ToString(),
                ControlName = c.Name ?? "Unnamed Control",
                Priority = DeterminePriority(c.Category),
                SuggestedEvidenceTypes = GetSuggestedEvidenceTypes(c.Category),
                Rationale = $"Control {c.ControlCode ?? c.Name} has no linked evidence"
            })
            .ToList();

        return new EvidenceGapAnalysis
        {
            TenantId = tenantId,
            FrameworkCode = frameworkCode,
            TotalControls = controls.Count,
            ControlsWithEvidence = controlsWithEvidence,
            ControlsWithoutEvidence = controlsWithoutEvidence,
            CoveragePercentage = controls.Count > 0
                ? (decimal)controlsWithEvidence / controls.Count * 100
                : 0,
            Gaps = gaps.Take(20).ToList(), // Limit to top 20 gaps
            PriorityRecommendations = GenerateGapRecommendations(gaps),
            AnalyzedAt = DateTime.UtcNow
        };
    }

    public async Task<Result<EvidenceMatchingResult>> SuggestEvidenceMatchesAsync(
        Guid evidenceId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Suggesting evidence matches for {EvidenceId}", evidenceId);

        var evidence = await _context.Evidences
            .FirstOrDefaultAsync(e => e.Id == evidenceId, cancellationToken);

        if (evidence == null)
        {
            return Result<EvidenceMatchingResult>.Failure(Error.NotFound("Evidence", evidenceId));
        }

        // Get unlinked controls that might match
        var controls = await _context.Controls
            .Where(c => c.TenantId == evidence.TenantId && !c.IsDeleted)
            .Select(c => new { c.Id, c.ControlCode, c.Name, c.Category, c.Description })
            .Take(100)
            .ToListAsync(cancellationToken);

        var matchedControls = controls
            .Where(c => IsLikelyMatch(evidence, c.Category, c.Description))
            .Select(c => new MatchedControl
            {
                ControlId = c.Id,
                ControlCode = c.ControlCode ?? c.Id.ToString(),
                ControlName = c.Name ?? "Unnamed Control",
                MatchConfidence = CalculateMatchConfidence(evidence, c.Category, c.Description),
                MatchReason = $"Category and type alignment with {c.Category}"
            })
            .OrderByDescending(m => m.MatchConfidence)
            .Take(10)
            .ToList();

        return Result<EvidenceMatchingResult>.Success(new EvidenceMatchingResult
        {
            EvidenceId = evidenceId,
            MatchedRequirements = new List<MatchedRequirement>(),
            MatchedControls = matchedControls,
            AnalyzedAt = DateTime.UtcNow
        });
    }

    public async Task<EvidenceExpirationRisk> AnalyzeExpirationRisksAsync(
        Guid tenantId,
        int lookAheadDays = 90,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Analyzing expiration risks for tenant {TenantId}, {Days} days ahead",
            tenantId, lookAheadDays);

        var cutoffDate = DateTime.UtcNow.AddDays(lookAheadDays);

        var expiringEvidence = await _context.Evidences
            .Where(e => e.TenantId == tenantId && !e.IsDeleted)
            .Where(e => e.RetentionEndDate.HasValue && e.RetentionEndDate.Value <= cutoffDate)
            .OrderBy(e => e.RetentionEndDate)
            .Select(e => new
            {
                e.Id,
                e.Title,
                EvidenceType = e.Type,
                ValidUntil = e.RetentionEndDate,
                e.ControlId
            })
            .Take(50)
            .ToListAsync(cancellationToken);

        var expiring = expiringEvidence.Select(e => new ExpiringEvidence
        {
            EvidenceId = e.Id,
            Title = e.Title ?? "Untitled Evidence",
            EvidenceType = e.EvidenceType ?? "General",
            ExpirationDate = e.ValidUntil ?? DateTime.UtcNow,
            DaysUntilExpiration = e.ValidUntil.HasValue
                ? (int)(e.ValidUntil.Value - DateTime.UtcNow).TotalDays
                : 0,
            RiskLevel = DetermineExpirationRiskLevel(e.ValidUntil),
            AffectedControls = new List<string>(),
            RenewalRecommendation = "Schedule evidence renewal before expiration"
        }).ToList();

        var criticalCount = expiring.Count(e => e.RiskLevel == "Critical");
        var highCount = expiring.Count(e => e.RiskLevel == "High");

        return new EvidenceExpirationRisk
        {
            TenantId = tenantId,
            LookAheadDays = lookAheadDays,
            TotalAtRisk = expiring.Count,
            OverallRiskLevel = criticalCount > 0 ? "Critical" : highCount > 0 ? "High" : "Medium",
            ExpiringEvidence = expiring,
            Recommendations = GenerateExpirationRecommendations(expiring),
            AnalyzedAt = DateTime.UtcNow
        };
    }

    public async Task<EvidenceCollectionPlan> GenerateCollectionPlanAsync(
        Guid tenantId,
        string frameworkCode,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating evidence collection plan for tenant {TenantId}, framework {Framework}",
            tenantId, frameworkCode);

        var controls = await _context.Controls
            .Where(c => c.TenantId == tenantId && !c.IsDeleted)
            .Where(c => c.SourceFrameworkCode == frameworkCode)
            .Select(c => new { c.Id, c.Category, c.Name, EvidenceCount = c.Evidences.Count })
            .ToListAsync(cancellationToken);

        var currentEvidenceCount = controls.Sum(c => c.EvidenceCount);
        var totalRequired = controls.Count * 2; // Assume 2 evidence items per control as baseline

        var tasks = controls
            .Where(c => c.EvidenceCount == 0)
            .GroupBy(c => c.Category ?? "General")
            .Select(g => new EvidenceCollectionTask
            {
                ControlDomain = g.Key,
                EvidenceType = GetDefaultEvidenceType(g.Key),
                Description = $"Collect evidence for {g.Count()} controls in {g.Key} domain",
                Priority = DeterminePriority(g.Key),
                SuggestedSources = GetSuggestedSources(g.Key),
                RelatedControlIds = g.Select(c => c.Id).ToList()
            })
            .OrderBy(t => t.Priority == "Critical" ? 0 : t.Priority == "High" ? 1 : 2)
            .ToList();

        return new EvidenceCollectionPlan
        {
            TenantId = tenantId,
            FrameworkCode = frameworkCode,
            TotalRequiredEvidenceItems = totalRequired,
            CurrentEvidenceCount = currentEvidenceCount,
            CompletionPercentage = totalRequired > 0
                ? (decimal)currentEvidenceCount / totalRequired * 100
                : 0,
            Tasks = tasks,
            QuickWins = GenerateQuickWins(tasks),
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<Result<EvidenceCategorizationResult>> CategorizeEvidenceAsync(
        Guid evidenceId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Categorizing evidence {EvidenceId}", evidenceId);

        var evidence = await _context.Evidences
            .FirstOrDefaultAsync(e => e.Id == evidenceId, cancellationToken);

        if (evidence == null)
        {
            return Result<EvidenceCategorizationResult>.Failure(Error.NotFound("Evidence", evidenceId));
        }

        if (!await IsAvailableAsync(cancellationToken))
        {
            return Result<EvidenceCategorizationResult>.Success(CreateFallbackCategorization(evidenceId, evidence));
        }

        try
        {
            var prompt = $@"Categorize this compliance evidence:

Title: {evidence.Title}
Description: {evidence.Description}
Current Type: {evidence.Type}
File Name: {evidence.FileName}

Suggest the best category, type, tags, and related domains.

Respond with JSON:
{{
  ""suggestedCategory"": ""Technical"",
  ""suggestedType"": ""Configuration Document"",
  ""suggestedTags"": [""security"", ""access-control"", ""policy""],
  ""relatedDomains"": [""Access Management"", ""Identity Management""],
  ""categorizationConfidence"": 85
}}";

            var responseResult = await CallClaudeApiAsync(prompt, cancellationToken);
            if (!responseResult.IsSuccess)
            {
                return Result<EvidenceCategorizationResult>.Success(CreateFallbackCategorization(evidenceId, evidence));
            }
            
            var result = JsonSerializer.Deserialize<EvidenceCategorizationResult>(responseResult.Value!);

            if (result != null)
            {
                result.EvidenceId = evidenceId;
                result.AnalyzedAt = DateTime.UtcNow;
                return Result<EvidenceCategorizationResult>.Success(result);
            }

            return Result<EvidenceCategorizationResult>.Success(CreateFallbackCategorization(evidenceId, evidence));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error categorizing evidence {EvidenceId}", evidenceId);
            return Result<EvidenceCategorizationResult>.Success(CreateFallbackCategorization(evidenceId, evidence));
        }
    }

    #region Private Helper Methods

    private async Task<Result<string>> CallClaudeApiAsync(string prompt, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            return Result<string>.Failure(Error.InvalidOperation("Claude API key is not configured"));
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

        return Result<string>.Success(claudeResponse?.Content?.FirstOrDefault()?.Text ?? "{}");
    }

    private string DeterminePriority(string? category)
    {
        return category?.ToLowerInvariant() switch
        {
            "access control" => "Critical",
            "security" => "Critical",
            "data protection" => "High",
            "incident management" => "High",
            "risk management" => "High",
            "change management" => "Medium",
            "business continuity" => "Medium",
            _ => "Medium"
        };
    }

    private List<string> GetSuggestedEvidenceTypes(string? category)
    {
        return category?.ToLowerInvariant() switch
        {
            "access control" => new List<string> { "Access Control Matrix", "User Access Review", "Authentication Logs" },
            "security" => new List<string> { "Security Policy", "Vulnerability Scan", "Penetration Test Report" },
            "data protection" => new List<string> { "Encryption Policy", "Data Classification", "Privacy Impact Assessment" },
            "incident management" => new List<string> { "Incident Report", "Root Cause Analysis", "Lessons Learned" },
            "risk management" => new List<string> { "Risk Assessment", "Risk Register", "Treatment Plan" },
            _ => new List<string> { "Policy Document", "Procedure Document", "Evidence Screenshot" }
        };
    }

    private List<string> GenerateGapRecommendations(List<EvidenceGap> gaps)
    {
        var recommendations = new List<string>();

        var criticalGaps = gaps.Count(g => g.Priority == "Critical");
        var highGaps = gaps.Count(g => g.Priority == "High");

        if (criticalGaps > 0)
        {
            recommendations.Add($"Prioritize evidence collection for {criticalGaps} critical controls immediately");
        }
        if (highGaps > 0)
        {
            recommendations.Add($"Schedule evidence collection for {highGaps} high-priority controls");
        }
        if (gaps.Count > 10)
        {
            recommendations.Add("Consider a dedicated evidence collection sprint to address multiple gaps");
        }

        return recommendations;
    }

    private bool IsLikelyMatch(Models.Entities.Evidence evidence, string? category, string? description)
    {
        if (string.IsNullOrWhiteSpace(category) && string.IsNullOrWhiteSpace(description))
            return false;

        var evidenceText = $"{evidence.Title} {evidence.Description} {evidence.Type}".ToLowerInvariant();
        var controlText = $"{category} {description}".ToLowerInvariant();

        // Simple keyword matching
        var evidenceWords = evidenceText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var controlWords = controlText.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var matchingWords = evidenceWords.Intersect(controlWords).Count();
        return matchingWords >= 2;
    }

    private int CalculateMatchConfidence(Models.Entities.Evidence evidence, string? category, string? description)
    {
        var evidenceText = $"{evidence.Title} {evidence.Description} {evidence.Type}".ToLowerInvariant();
        var controlText = $"{category} {description}".ToLowerInvariant();

        var evidenceWords = evidenceText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var controlWords = controlText.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var matchingWords = evidenceWords.Intersect(controlWords).Count();
        var totalWords = Math.Max(evidenceWords.Length, controlWords.Length);

        return totalWords > 0 ? Math.Min(100, matchingWords * 20) : 0;
    }

    private string DetermineExpirationRiskLevel(DateTime? validUntil)
    {
        if (!validUntil.HasValue) return "Low";

        var daysUntil = (validUntil.Value - DateTime.UtcNow).TotalDays;

        return daysUntil switch
        {
            <= 7 => "Critical",
            <= 30 => "High",
            <= 60 => "Medium",
            _ => "Low"
        };
    }

    private List<string> GenerateExpirationRecommendations(List<ExpiringEvidence> expiring)
    {
        var recommendations = new List<string>();

        var critical = expiring.Where(e => e.RiskLevel == "Critical").ToList();
        var high = expiring.Where(e => e.RiskLevel == "High").ToList();

        if (critical.Any())
        {
            recommendations.Add($"URGENT: {critical.Count} evidence items expire within 7 days - immediate action required");
        }
        if (high.Any())
        {
            recommendations.Add($"Schedule renewal for {high.Count} evidence items expiring within 30 days");
        }
        if (expiring.Count > 5)
        {
            recommendations.Add("Consider implementing automated evidence renewal reminders");
        }

        return recommendations;
    }

    private string GetDefaultEvidenceType(string domain)
    {
        return domain.ToLowerInvariant() switch
        {
            "access control" => "Access Control Matrix",
            "security" => "Security Assessment",
            "data protection" => "Data Classification Document",
            _ => "Policy/Procedure Document"
        };
    }

    private List<string> GetSuggestedSources(string domain)
    {
        return domain.ToLowerInvariant() switch
        {
            "access control" => new List<string> { "IAM System", "Active Directory", "SSO Provider" },
            "security" => new List<string> { "SIEM", "Vulnerability Scanner", "Security Team" },
            "data protection" => new List<string> { "DLP System", "Data Governance Team", "Privacy Office" },
            _ => new List<string> { "Document Management System", "Process Owner", "IT Team" }
        };
    }

    private List<string> GenerateQuickWins(List<EvidenceCollectionTask> tasks)
    {
        var quickWins = new List<string>();

        var policyTasks = tasks.Where(t => t.EvidenceType.Contains("Policy")).ToList();
        if (policyTasks.Any())
        {
            quickWins.Add($"Collect existing policy documents for {policyTasks.Count} domains");
        }

        quickWins.Add("Export system configurations as evidence artifacts");
        quickWins.Add("Screenshot audit logs for recent compliance activities");

        return quickWins;
    }

    private EvidenceQualityAnalysis CreateFallbackQualityAnalysis(Guid evidenceId)
    {
        return new EvidenceQualityAnalysis
        {
            EvidenceId = evidenceId,
            QualityRating = "Pending Review",
            QualityScore = 50,
            Findings = new List<QualityFinding>
            {
                new QualityFinding
                {
                    Dimension = "AI Analysis",
                    Severity = "Low",
                    Description = "AI-powered quality analysis not available",
                    Recommendation = "Configure Claude API key for enhanced evidence analysis"
                }
            },
            Recommendations = new List<string>
            {
                "Enable AI-powered evidence analysis for detailed quality assessment",
                "Manually review evidence against requirement criteria"
            },
            DimensionScores = new Dictionary<string, int>
            {
                { "Completeness", 50 },
                { "Relevance", 50 },
                { "Currency", 50 },
                { "Authenticity", 50 },
                { "Traceability", 50 }
            },
            AnalyzedAt = DateTime.UtcNow
        };
    }

    private EvidenceCategorizationResult CreateFallbackCategorization(Guid evidenceId, Models.Entities.Evidence evidence)
    {
        return new EvidenceCategorizationResult
        {
            EvidenceId = evidenceId,
            SuggestedCategory = evidence.Type ?? "General",
            SuggestedType = evidence.Type ?? "Document",
            SuggestedTags = new List<string> { "compliance", "evidence" },
            RelatedDomains = new List<string> { "General Compliance" },
            CategorizationConfidence = 30,
            AnalyzedAt = DateTime.UtcNow
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

    #endregion
}
