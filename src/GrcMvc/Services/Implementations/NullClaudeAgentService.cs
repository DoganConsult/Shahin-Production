using GrcMvc.Services.Interfaces;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Null/stub implementation of IClaudeAgentService for when Claude AI is disabled.
/// Returns empty/default results for all operations.
/// </summary>
public class NullClaudeAgentService : IClaudeAgentService
{
    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(false);

    public Task<ComplianceAnalysisResult> AnalyzeComplianceAsync(
        string frameworkCode,
        Guid? assessmentId = null,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
        => Task.FromResult(new ComplianceAnalysisResult
        {
            Success = false,
            Summary = "Claude AI is not configured"
        });

    public Task<RiskAnalysisResult> AnalyzeRiskAsync(
        string riskDescription,
        Dictionary<string, object>? context = null,
        CancellationToken cancellationToken = default)
        => Task.FromResult(new RiskAnalysisResult
        {
            Success = false,
            Analysis = "Claude AI is not configured"
        });

    public Task<AuditAnalysisResult> AnalyzeAuditAsync(
        Guid auditId,
        CancellationToken cancellationToken = default)
        => Task.FromResult(new AuditAnalysisResult
        {
            Success = false,
            Summary = "Claude AI is not configured"
        });

    public Task<PolicyAnalysisResult> AnalyzePolicyAsync(
        string policyContent,
        string? frameworkCode = null,
        CancellationToken cancellationToken = default)
        => Task.FromResult(new PolicyAnalysisResult
        {
            Success = false,
            Summary = "Claude AI is not configured"
        });

    public Task<AnalyticsResult> GenerateInsightsAsync(
        string dataType,
        Dictionary<string, object>? filters = null,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
        => Task.FromResult(new AnalyticsResult
        {
            Success = false,
            Summary = "Claude AI is not configured"
        });

    public Task<ReportGenerationResult> GenerateReportAsync(
        string reportType,
        Dictionary<string, object>? parameters = null,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
        => Task.FromResult(new ReportGenerationResult
        {
            Success = false,
            Content = "Claude AI is not configured"
        });

    public Task<ChatResponse> ChatAsync(
        string message,
        List<ChatMessage>? conversationHistory = null,
        string? context = null,
        CancellationToken cancellationToken = default)
        => Task.FromResult(new ChatResponse
        {
            Success = false,
            Response = "Claude AI is not configured. Please set CLAUDE_API_KEY environment variable."
        });

    public Task<ControlAssessmentResult> AssessControlAsync(
        Guid controlId,
        string? evidenceDescription = null,
        CancellationToken cancellationToken = default)
        => Task.FromResult(new ControlAssessmentResult
        {
            Success = false,
            Analysis = "Claude AI is not configured"
        });

    public Task<EvidenceAnalysisResult> AnalyzeEvidenceAsync(
        Guid evidenceId,
        string? content = null,
        CancellationToken cancellationToken = default)
        => Task.FromResult(new EvidenceAnalysisResult
        {
            Success = false,
            Analysis = "Claude AI is not configured"
        });

    public Task<WorkflowOptimizationResult> OptimizeWorkflowAsync(
        string workflowType,
        Dictionary<string, object>? currentMetrics = null,
        CancellationToken cancellationToken = default)
        => Task.FromResult(new WorkflowOptimizationResult
        {
            Success = false,
            Summary = "Claude AI is not configured"
        });
}
