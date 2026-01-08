using GrcMvc.Models.Entities;

namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Unified AI Service - Multi-provider, Multi-tenant, Dynamic configuration
/// Replaces hardcoded Claude-only service with flexible provider support
/// </summary>
public interface IUnifiedAiService
{
    // ===== Provider Management =====
    
    /// <summary>
    /// Check if AI service is available for the tenant
    /// </summary>
    Task<bool> IsAvailableAsync(Guid? tenantId = null, CancellationToken ct = default);
    
    /// <summary>
    /// Get available providers for tenant
    /// </summary>
    Task<List<AiProviderInfo>> GetAvailableProvidersAsync(Guid? tenantId = null, CancellationToken ct = default);
    
    /// <summary>
    /// Test provider connection
    /// </summary>
    Task<AiTestResult> TestProviderAsync(Guid configurationId, CancellationToken ct = default);
    
    // ===== Generic AI Calls =====
    
    /// <summary>
    /// Send a message to AI and get response
    /// </summary>
    Task<AiResponse> ChatAsync(
        string message,
        string? systemPrompt = null,
        Guid? tenantId = null,
        string? preferredProvider = null,
        CancellationToken ct = default);
    
    /// <summary>
    /// Send structured prompt and parse JSON response
    /// </summary>
    Task<T?> PromptAsync<T>(
        string prompt,
        string? systemPrompt = null,
        Guid? tenantId = null,
        string? preferredProvider = null,
        CancellationToken ct = default) where T : class;
    
    /// <summary>
    /// Multi-turn conversation
    /// </summary>
    Task<AiResponse> ConversationAsync(
        List<AiMessage> messages,
        string? systemPrompt = null,
        Guid? tenantId = null,
        string? preferredProvider = null,
        CancellationToken ct = default);
    
    // ===== GRC-Specific AI Functions =====
    
    /// <summary>
    /// Analyze compliance status for a framework
    /// </summary>
    Task<ComplianceAiResult> AnalyzeComplianceAsync(
        string frameworkCode,
        Guid? assessmentId = null,
        Guid? tenantId = null,
        CancellationToken ct = default);
    
    /// <summary>
    /// Analyze and score a risk
    /// </summary>
    Task<RiskAiResult> AnalyzeRiskAsync(
        string riskDescription,
        Dictionary<string, object>? context = null,
        Guid? tenantId = null,
        CancellationToken ct = default);
    
    /// <summary>
    /// Analyze audit and provide findings
    /// </summary>
    Task<AuditAiResult> AnalyzeAuditAsync(
        Guid auditId,
        Guid? tenantId = null,
        CancellationToken ct = default);
    
    /// <summary>
    /// Analyze policy document quality
    /// </summary>
    Task<PolicyAiResult> AnalyzePolicyAsync(
        string policyContent,
        string? frameworkCode = null,
        Guid? tenantId = null,
        CancellationToken ct = default);
    
    /// <summary>
    /// Generate executive insights from data
    /// </summary>
    Task<InsightsAiResult> GenerateInsightsAsync(
        string dataType,
        Dictionary<string, object>? data = null,
        Guid? tenantId = null,
        CancellationToken ct = default);
    
    /// <summary>
    /// Generate a formatted report
    /// </summary>
    Task<ReportAiResult> GenerateReportAsync(
        string reportType,
        Dictionary<string, object>? parameters = null,
        Guid? tenantId = null,
        CancellationToken ct = default);
    
    /// <summary>
    /// Assess control effectiveness
    /// </summary>
    Task<ControlAiResult> AssessControlAsync(
        Guid controlId,
        string? evidenceDescription = null,
        Guid? tenantId = null,
        CancellationToken ct = default);
    
    /// <summary>
    /// Analyze evidence quality and relevance
    /// </summary>
    Task<EvidenceAiResult> AnalyzeEvidenceAsync(
        Guid evidenceId,
        string? content = null,
        Guid? tenantId = null,
        CancellationToken ct = default);
    
    /// <summary>
    /// Arabic language compliance assistant
    /// </summary>
    Task<AiResponse> ArabicAssistantAsync(
        string query,
        string? context = null,
        Guid? tenantId = null,
        CancellationToken ct = default);
}

#region DTOs

public class AiProviderInfo
{
    public Guid ConfigurationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public int Priority { get; set; }
    public string[] AllowedUseCases { get; set; } = Array.Empty<string>();
    public int? UsageRemaining { get; set; }
}

public class AiTestResult
{
    public bool Success { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string? Model { get; set; }
    public int LatencyMs { get; set; }
    public string? Response { get; set; }
    public string? Error { get; set; }
}

public class AiMessage
{
    public string Role { get; set; } = "user"; // user, assistant, system
    public string Content { get; set; } = string.Empty;
}

public class AiResponse
{
    public bool Success { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int TokensUsed { get; set; }
    public int LatencyMs { get; set; }
    public string? Error { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class ComplianceAiResult : AiResponse
{
    public string? FrameworkCode { get; set; }
    public double ComplianceScore { get; set; }
    public List<GapItem> Gaps { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public string? Summary { get; set; }
    public string? SummaryAr { get; set; }
}

public class GapItem
{
    public string ControlId { get; set; } = string.Empty;
    public string ControlName { get; set; } = string.Empty;
    public string GapDescription { get; set; } = string.Empty;
    public string Severity { get; set; } = "Medium";
    public string? Remediation { get; set; }
    public int EstimatedEffortDays { get; set; }
}

public class RiskAiResult : AiResponse
{
    public double RiskScore { get; set; }
    public string RiskLevel { get; set; } = "Medium";
    public double LikelihoodScore { get; set; }
    public double ImpactScore { get; set; }
    public List<string> RiskFactors { get; set; } = new();
    public List<string> MitigationStrategies { get; set; } = new();
    public string? Analysis { get; set; }
    public string? AnalysisAr { get; set; }
}

public class AuditAiResult : AiResponse
{
    public Guid AuditId { get; set; }
    public List<FindingItem> Findings { get; set; } = new();
    public List<string> Patterns { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public string? Summary { get; set; }
}

public class FindingItem
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = "Medium";
    public string? Impact { get; set; }
    public string? Recommendation { get; set; }
}

public class PolicyAiResult : AiResponse
{
    public double QualityScore { get; set; }
    public List<IssueItem> Issues { get; set; } = new();
    public List<string> Improvements { get; set; } = new();
    public List<string> FrameworkAlignments { get; set; } = new();
    public string? Summary { get; set; }
}

public class IssueItem
{
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = "Medium";
    public string? Suggestion { get; set; }
}

public class InsightsAiResult : AiResponse
{
    public string DataType { get; set; } = string.Empty;
    public List<AiInsightItem> Insights { get; set; } = new();
    public List<AiTrendItem> Trends { get; set; } = new();
    public Dictionary<string, object> Metrics { get; set; } = new();
    public string? Summary { get; set; }
}

public class AiInsightItem
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Importance { get; set; } = "Medium";
    public string? ActionSuggestion { get; set; }
}

public class AiTrendItem
{
    public string Metric { get; set; } = string.Empty;
    public string Direction { get; set; } = "Stable";
    public double ChangePercentage { get; set; }
    public string? Interpretation { get; set; }
}

public class ReportAiResult : AiResponse
{
    public string ReportType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? ExecutiveSummary { get; set; }
    public List<string> KeyFindings { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

public class ControlAiResult : AiResponse
{
    public Guid ControlId { get; set; }
    public string EffectivenessRating { get; set; } = "Partially Effective";
    public double EffectivenessScore { get; set; }
    public List<string> Strengths { get; set; } = new();
    public List<string> Weaknesses { get; set; } = new();
    public List<string> Improvements { get; set; } = new();
    public string? Analysis { get; set; }
}

public class EvidenceAiResult : AiResponse
{
    public Guid EvidenceId { get; set; }
    public double QualityScore { get; set; }
    public bool IsRelevant { get; set; }
    public bool IsSufficient { get; set; }
    public List<string> Issues { get; set; } = new();
    public List<string> Suggestions { get; set; } = new();
    public string? Analysis { get; set; }
}

#endregion
