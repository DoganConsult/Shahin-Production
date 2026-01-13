using GrcMvc.Models.Entities;

namespace GrcMvc.Services.Interfaces;

#region NextBestAction Service

/// <summary>
/// Service for generating and managing Next Best Action recommendations.
/// </summary>
public interface INextBestActionService
{
    /// <summary>
    /// Generate recommendations for a specific user
    /// </summary>
    Task<NextBestActionResult> GenerateRecommendationsAsync(
        Guid tenantId,
        Guid? userId = null,
        string? roleCode = null,
        int maxRecommendations = 5,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get pending recommendations for a user
    /// </summary>
    Task<List<NextBestActionRecommendation>> GetPendingRecommendationsAsync(
        Guid tenantId,
        Guid? userId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Accept a recommendation
    /// </summary>
    Task<bool> AcceptRecommendationAsync(
        Guid recommendationId,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reject a recommendation with optional feedback
    /// </summary>
    Task<bool> RejectRecommendationAsync(
        Guid recommendationId,
        Guid userId,
        string? feedback = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute an auto-executable recommendation
    /// </summary>
    Task<ExecuteRecommendationResult> ExecuteRecommendationAsync(
        Guid recommendationId,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Rate a recommendation
    /// </summary>
    Task<bool> RateRecommendationAsync(
        Guid recommendationId,
        Guid userId,
        int rating,
        string? feedback = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Dismiss expired recommendations
    /// </summary>
    Task<int> DismissExpiredRecommendationsAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);
}

public class NextBestActionResult
{
    public bool Success { get; set; }
    public List<NextBestActionRecommendation> Recommendations { get; set; } = new();
    public string? Message { get; set; }
    public int TotalGenerated { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class ExecuteRecommendationResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? ResultDetails { get; set; }
    public Guid? ResultEntityId { get; set; }
}

#endregion

#region Progress Certainty Index Service

/// <summary>
/// Service for calculating and managing Progress Certainty Index.
/// </summary>
public interface IProgressCertaintyService
{
    /// <summary>
    /// Calculate PCI for a tenant
    /// </summary>
    Task<ProgressCertaintyIndex> CalculatePciAsync(
        Guid tenantId,
        string entityType = "Tenant",
        Guid? entityId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get latest PCI for a tenant
    /// </summary>
    Task<ProgressCertaintyIndex?> GetLatestPciAsync(
        Guid tenantId,
        string entityType = "Tenant",
        Guid? entityId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get PCI history for trend analysis
    /// </summary>
    Task<List<ProgressCertaintyIndex>> GetPciHistoryAsync(
        Guid tenantId,
        string entityType = "Tenant",
        Guid? entityId = null,
        int days = 30,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get recommended interventions based on PCI
    /// </summary>
    Task<List<string>> GetRecommendedInterventionsAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Predict completion date based on current velocity
    /// </summary>
    Task<PredictionResult> PredictCompletionAsync(
        Guid tenantId,
        Guid? planId = null,
        CancellationToken cancellationToken = default);
}

public class PredictionResult
{
    public bool Success { get; set; }
    public DateTime? PredictedDate { get; set; }
    public DateTime? TargetDate { get; set; }
    public int? DaysVariance { get; set; }
    public double ConfidencePercent { get; set; }
    public string? RiskAssessment { get; set; }
    public List<string> RiskFactors { get; set; } = new();
}

#endregion

#region Engagement Metrics Service

/// <summary>
/// Service for tracking and analyzing user engagement metrics.
/// </summary>
public interface IEngagementMetricsService
{
    /// <summary>
    /// Record engagement metrics for a user session
    /// </summary>
    Task<EngagementMetrics> RecordMetricsAsync(
        Guid tenantId,
        Guid? userId,
        EngagementMetricsInput input,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get current engagement metrics for a user
    /// </summary>
    Task<EngagementMetrics?> GetCurrentMetricsAsync(
        Guid tenantId,
        Guid? userId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get engagement history for trend analysis
    /// </summary>
    Task<List<EngagementMetrics>> GetEngagementHistoryAsync(
        Guid tenantId,
        Guid? userId = null,
        int days = 30,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate motivation score for a user
    /// </summary>
    Task<MotivationScore> CalculateMotivationScoreAsync(
        Guid tenantId,
        Guid? userId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Detect engagement state and recommend actions
    /// </summary>
    Task<EngagementAnalysisResult> AnalyzeEngagementAsync(
        Guid tenantId,
        Guid? userId = null,
        CancellationToken cancellationToken = default);
}

public class EngagementMetricsInput
{
    public int SessionDurationMinutes { get; set; }
    public int ActionsInSession { get; set; }
    public int TasksCompleted { get; set; }
    public int EvidenceSubmitted { get; set; }
    public int ErrorsEncountered { get; set; }
    public int HelpRequests { get; set; }
    public double? ResponseTimeMinutes { get; set; }
}

public class EngagementAnalysisResult
{
    public bool Success { get; set; }
    public string EngagementState { get; set; } = "Neutral";
    public int OverallScore { get; set; }
    public int ConfidenceScore { get; set; }
    public int FatigueScore { get; set; }
    public int MomentumScore { get; set; }
    public List<string> RecommendedActions { get; set; } = new();
    public string? AnalysisSummary { get; set; }
}

#endregion

#region Conditional Logic Rules Service

/// <summary>
/// Service for evaluating and executing conditional logic rules.
/// </summary>
public interface IConditionalLogicService
{
    /// <summary>
    /// Evaluate all applicable rules for a context
    /// </summary>
    Task<RuleEvaluationResult> EvaluateRulesAsync(
        Guid tenantId,
        string triggerEvent,
        Dictionary<string, object> context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Evaluate a specific rule
    /// </summary>
    Task<bool> EvaluateRuleAsync(
        Guid ruleId,
        Dictionary<string, object> context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute actions for matched rules
    /// </summary>
    Task<RuleExecutionResult> ExecuteRuleActionsAsync(
        Guid tenantId,
        List<ConditionalLogicRule> matchedRules,
        Dictionary<string, object> context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all active rules for a trigger event
    /// </summary>
    Task<List<ConditionalLogicRule>> GetRulesForEventAsync(
        string triggerEvent,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create or update a conditional logic rule
    /// </summary>
    Task<ConditionalLogicRule> SaveRuleAsync(
        ConditionalLogicRule rule,
        CancellationToken cancellationToken = default);
}

public class RuleEvaluationResult
{
    public bool Success { get; set; }
    public List<ConditionalLogicRule> MatchedRules { get; set; } = new();
    public List<ConditionalLogicRuleExecution> ExecutionLogs { get; set; } = new();
    public int TotalEvaluated { get; set; }
    public int TotalMatched { get; set; }
    public string? Message { get; set; }
}

public class RuleExecutionResult
{
    public bool Success { get; set; }
    public List<RuleActionResult> ActionResults { get; set; } = new();
    public string? Message { get; set; }
}

public class RuleActionResult
{
    public string ActionType { get; set; } = "";
    public bool Success { get; set; }
    public string? ResultDetails { get; set; }
    public Guid? AffectedEntityId { get; set; }
}

#endregion

#region Evidence Confidence Service

/// <summary>
/// Service for calculating evidence confidence scores.
/// </summary>
public interface IEvidenceConfidenceService
{
    /// <summary>
    /// Calculate confidence score for evidence
    /// </summary>
    Task<EvidenceConfidenceScore> CalculateConfidenceAsync(
        Guid tenantId,
        Guid evidenceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get confidence score for evidence
    /// </summary>
    Task<EvidenceConfidenceScore?> GetConfidenceScoreAsync(
        Guid evidenceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Batch calculate confidence for multiple evidence items
    /// </summary>
    Task<List<EvidenceConfidenceScore>> BatchCalculateConfidenceAsync(
        Guid tenantId,
        List<Guid> evidenceIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Record human review outcome
    /// </summary>
    Task<bool> RecordHumanReviewAsync(
        Guid evidenceConfidenceId,
        string outcome,
        string? feedback,
        Guid reviewerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get evidence items needing human review
    /// </summary>
    Task<List<EvidenceConfidenceScore>> GetItemsNeedingReviewAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);
}

#endregion

#region Agent Event Trigger Service

/// <summary>
/// Service for managing agent event-driven triggers.
/// </summary>
public interface IAgentTriggerService
{
    /// <summary>
    /// Process an event and trigger applicable agents
    /// </summary>
    Task<TriggerProcessingResult> ProcessEventAsync(
        string eventType,
        Guid? tenantId,
        Guid? sourceEntityId,
        string? sourceEntityType,
        Dictionary<string, object>? eventPayload = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get active triggers for an event type
    /// </summary>
    Task<List<AgentEventTrigger>> GetTriggersForEventAsync(
        string eventType,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Register a new trigger
    /// </summary>
    Task<AgentEventTrigger> RegisterTriggerAsync(
        AgentEventTrigger trigger,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Enable/disable a trigger
    /// </summary>
    Task<bool> SetTriggerActiveAsync(
        Guid triggerId,
        bool isActive,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get trigger execution history
    /// </summary>
    Task<List<AgentTriggerExecution>> GetExecutionHistoryAsync(
        Guid? tenantId,
        Guid? triggerId = null,
        int limit = 100,
        CancellationToken cancellationToken = default);
}

public class TriggerProcessingResult
{
    public bool Success { get; set; }
    public int TriggersEvaluated { get; set; }
    public int TriggersActivated { get; set; }
    public List<AgentTriggerExecution> Executions { get; set; } = new();
    public string? Message { get; set; }
}

#endregion

#region Agent Communication Service

/// <summary>
/// Service for managing inter-agent communication contracts.
/// </summary>
public interface IAgentCommunicationService
{
    /// <summary>
    /// Send message from one agent to another
    /// </summary>
    Task<AgentCommunicationResult> SendMessageAsync(
        string fromAgentCode,
        string toAgentCode,
        object requestPayload,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get communication contract between agents
    /// </summary>
    Task<AgentCommunicationContract?> GetContractAsync(
        string fromAgentCode,
        string toAgentCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate payload against contract schema
    /// </summary>
    Task<ValidationResult> ValidatePayloadAsync(
        Guid contractId,
        object payload,
        bool isRequest = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get communication logs
    /// </summary>
    Task<List<AgentCommunicationLog>> GetCommunicationLogsAsync(
        Guid? tenantId = null,
        string? fromAgentCode = null,
        string? toAgentCode = null,
        string? correlationId = null,
        int limit = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Register or update communication contract
    /// </summary>
    Task<AgentCommunicationContract> SaveContractAsync(
        AgentCommunicationContract contract,
        CancellationToken cancellationToken = default);
}

public class AgentCommunicationResult
{
    public bool Success { get; set; }
    public string? CorrelationId { get; set; }
    public object? ResponsePayload { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public int DurationMs { get; set; }
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}

#endregion
