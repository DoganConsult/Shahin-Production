namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Unified Claude AI Agent Service for all GRC AI capabilities
/// </summary>
public interface IClaudeAgentService
{
    /// <summary>
    /// Check if the Claude API is configured and available
    /// </summary>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Compliance Agent - Analyze compliance requirements and gaps
    /// </summary>
    Task<ComplianceAnalysisResult> AnalyzeComplianceAsync(
        string frameworkCode,
        Guid? assessmentId = null,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Risk Assessment Agent - Analyze and score risks
    /// </summary>
    Task<RiskAnalysisResult> AnalyzeRiskAsync(
        string riskDescription,
        Dictionary<string, object>? context = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Audit Agent - Analyze audit trails and findings
    /// </summary>
    Task<AuditAnalysisResult> AnalyzeAuditAsync(
        Guid auditId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Policy Agent - Review and recommend policy changes
    /// </summary>
    Task<PolicyAnalysisResult> AnalyzePolicyAsync(
        string policyContent,
        string? frameworkCode = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Analytics Agent - Generate insights from data
    /// </summary>
    Task<AnalyticsResult> GenerateInsightsAsync(
        string dataType,
        Dictionary<string, object>? filters = null,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Report Agent - Generate natural language reports
    /// </summary>
    Task<ReportGenerationResult> GenerateReportAsync(
        string reportType,
        Dictionary<string, object>? parameters = null,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// General chat with context-aware AI
    /// </summary>
    Task<ChatResponse> ChatAsync(
        string message,
        List<ChatMessage>? conversationHistory = null,
        string? context = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Control effectiveness assessment
    /// </summary>
    Task<ControlAssessmentResult> AssessControlAsync(
        Guid controlId,
        string? evidenceDescription = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Evidence quality analysis
    /// </summary>
    Task<EvidenceAnalysisResult> AnalyzeEvidenceAsync(
        Guid evidenceId,
        string? content = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Workflow optimization suggestions
    /// </summary>
    Task<WorkflowOptimizationResult> OptimizeWorkflowAsync(
        string workflowType,
        Dictionary<string, object>? currentMetrics = null,
        CancellationToken cancellationToken = default);
}

#region Result Models

public class ComplianceAnalysisResult
{
    public bool Success { get; set; }
    public string? FrameworkCode { get; set; }
    public double ComplianceScore { get; set; }
    public List<ComplianceGap> Gaps { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public string? Summary { get; set; }
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
}

public class ComplianceGap
{
    public string ControlId { get; set; } = string.Empty;
    public string ControlName { get; set; } = string.Empty;
    public string GapDescription { get; set; } = string.Empty;
    public string Severity { get; set; } = "Medium";
    public string? RemediationSuggestion { get; set; }
    public int EstimatedEffortDays { get; set; }
}

public class RiskAnalysisResult
{
    public bool Success { get; set; }
    public double RiskScore { get; set; }
    public string RiskLevel { get; set; } = "Medium";
    public double LikelihoodScore { get; set; }
    public double ImpactScore { get; set; }
    public List<string> RiskFactors { get; set; } = new();
    public List<string> MitigationStrategies { get; set; } = new();
    public string? Analysis { get; set; }
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
}

public class AuditAnalysisResult
{
    public bool Success { get; set; }
    public Guid AuditId { get; set; }
    public List<AuditFindingItem> Findings { get; set; } = new();
    public List<string> Patterns { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public string? Summary { get; set; }
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
}

public class AuditFindingItem
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = "Medium";
    public string? Impact { get; set; }
    public string? Recommendation { get; set; }
}

public class PolicyAnalysisResult
{
    public bool Success { get; set; }
    public double QualityScore { get; set; }
    public List<PolicyIssue> Issues { get; set; } = new();
    public List<string> SuggestedImprovements { get; set; } = new();
    public List<string> ComplianceAlignments { get; set; } = new();
    public string? Summary { get; set; }
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
}

public class PolicyIssue
{
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = "Medium";
    public string? Suggestion { get; set; }
}

public class AnalyticsResult
{
    public bool Success { get; set; }
    public string DataType { get; set; } = string.Empty;
    public List<InsightItem> Insights { get; set; } = new();
    public List<TrendItem> Trends { get; set; } = new();
    public Dictionary<string, object> Metrics { get; set; } = new();
    public string? Summary { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class InsightItem
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Importance { get; set; } = "Medium";
    public string? ActionSuggestion { get; set; }
}

public class TrendItem
{
    public string Metric { get; set; } = string.Empty;
    public string Direction { get; set; } = "Stable";
    public double ChangePercentage { get; set; }
    public string? Interpretation { get; set; }
}

public class ReportGenerationResult
{
    public bool Success { get; set; }
    public string ReportType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ExecutiveSummary { get; set; }
    public List<string> KeyFindings { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class ChatResponse
{
    public bool Success { get; set; }
    public string Response { get; set; } = string.Empty;
    public List<string>? SuggestedActions { get; set; }
    public Dictionary<string, object>? ExtractedData { get; set; }
    public DateTime ResponseAt { get; set; } = DateTime.UtcNow;
}

public class ChatMessage
{
    public string Role { get; set; } = "user";
    public string Content { get; set; } = string.Empty;
}

public class ControlAssessmentResult
{
    public bool Success { get; set; }
    public Guid ControlId { get; set; }
    public string EffectivenessRating { get; set; } = "Partially Effective";
    public double EffectivenessScore { get; set; }
    public List<string> Strengths { get; set; } = new();
    public List<string> Weaknesses { get; set; } = new();
    public List<string> ImprovementSuggestions { get; set; } = new();
    public string? Analysis { get; set; }
    public DateTime AssessedAt { get; set; } = DateTime.UtcNow;
}

public class EvidenceAnalysisResult
{
    public bool Success { get; set; }
    public Guid EvidenceId { get; set; }
    public double QualityScore { get; set; }
    public bool IsRelevant { get; set; }
    public bool IsSufficient { get; set; }
    public List<string> Issues { get; set; } = new();
    public List<string> Suggestions { get; set; } = new();
    public string? Analysis { get; set; }
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
}

public class WorkflowOptimizationResult
{
    public bool Success { get; set; }
    public string WorkflowType { get; set; } = string.Empty;
    public List<OptimizationSuggestion> Suggestions { get; set; } = new();
    public List<BottleneckItem> Bottlenecks { get; set; } = new();
    public Dictionary<string, object> ProjectedImprovements { get; set; } = new();
    public string? Summary { get; set; }
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
}

public class OptimizationSuggestion
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium";
    public string? ExpectedBenefit { get; set; }
    public int EstimatedEffortHours { get; set; }
}

public class BottleneckItem
{
    public string Stage { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Impact { get; set; } = string.Empty;
    public string? Resolution { get; set; }
}

#endregion
