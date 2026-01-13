using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GrcMvc.Models.Entities;

#region NextBestAction Agent

/// <summary>
/// NextBestAction Agent - Recommends optimal next actions based on context and engagement metrics.
/// Part of the fullplan specification for intelligent GRC guidance.
/// </summary>
[Table("NextBestActionRecommendations")]
[Index(nameof(TenantId), nameof(Status), Name = "IX_NBA_Tenant_Status")]
[Index(nameof(TargetUserId), nameof(CreatedDate), Name = "IX_NBA_User_Date")]
public class NextBestActionRecommendation : BaseEntity
{
    [Required]
    public Guid TenantId { get; set; }

    /// <summary>
    /// User this recommendation is for
    /// </summary>
    public Guid? TargetUserId { get; set; }

    /// <summary>
    /// Role code if recommendation is role-based
    /// </summary>
    [StringLength(50)]
    public string? TargetRoleCode { get; set; }

    /// <summary>
    /// Unique action identifier
    /// </summary>
    [Required]
    [StringLength(100)]
    public string ActionId { get; set; } = null!;

    /// <summary>
    /// Type of recommended action
    /// </summary>
    [Required]
    [StringLength(50)]
    public string ActionType { get; set; } = null!; // Remind, Reassign, SplitTask, AutoCollect, ReduceScope, Escalate, PauseExplain, Complete, Review

    /// <summary>
    /// Human-readable action description
    /// </summary>
    [Required]
    [StringLength(500)]
    public string Description { get; set; } = null!;

    [StringLength(500)]
    public string? DescriptionAr { get; set; }

    /// <summary>
    /// Confidence score for this recommendation (0-100)
    /// </summary>
    [Range(0, 100)]
    public int ConfidenceScore { get; set; } = 50;

    /// <summary>
    /// Priority ranking (1 = highest)
    /// </summary>
    [Range(1, 100)]
    public int Priority { get; set; } = 50;

    /// <summary>
    /// Why this action is recommended
    /// </summary>
    [StringLength(2000)]
    public string? Rationale { get; set; }

    /// <summary>
    /// Expected impact if action is taken
    /// </summary>
    [StringLength(1000)]
    public string? ExpectedImpact { get; set; }

    /// <summary>
    /// Related entity type (Task, Evidence, Control, Risk, etc.)
    /// </summary>
    [StringLength(50)]
    public string? RelatedEntityType { get; set; }

    /// <summary>
    /// Related entity ID
    /// </summary>
    public Guid? RelatedEntityId { get; set; }

    /// <summary>
    /// JSON object with action-specific parameters
    /// </summary>
    public string? ActionParametersJson { get; set; }

    /// <summary>
    /// Context data used to generate this recommendation
    /// </summary>
    public string? ContextDataJson { get; set; }

    /// <summary>
    /// Conditional rules that triggered this recommendation
    /// </summary>
    public string? TriggerConditionsJson { get; set; }

    /// <summary>
    /// Status of this recommendation
    /// </summary>
    [Required]
    [StringLength(30)]
    public string Status { get; set; } = "Pending"; // Pending, Accepted, Rejected, Expired, Executed, Dismissed

    /// <summary>
    /// When this recommendation expires
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// User who acted on this recommendation
    /// </summary>
    public Guid? ActedByUserId { get; set; }

    /// <summary>
    /// When user acted on this recommendation
    /// </summary>
    public DateTime? ActedAt { get; set; }

    /// <summary>
    /// User feedback on recommendation quality
    /// </summary>
    [StringLength(500)]
    public string? UserFeedback { get; set; }

    /// <summary>
    /// Rating given by user (1-5)
    /// </summary>
    [Range(1, 5)]
    public int? UserRating { get; set; }
}

/// <summary>
/// Standard action types for NextBestAction recommendations
/// </summary>
public static class NbaActionTypes
{
    public const string Remind = "Remind";
    public const string Reassign = "Reassign";
    public const string SplitTask = "SplitTask";
    public const string AutoCollect = "AutoCollect";
    public const string ReduceScope = "ReduceScope";
    public const string Escalate = "Escalate";
    public const string PauseExplain = "PauseExplain";
    public const string Complete = "Complete";
    public const string Review = "Review";
    public const string Approve = "Approve";
    public const string SubmitEvidence = "SubmitEvidence";
    public const string UpdateStatus = "UpdateStatus";
    public const string RequestHelp = "RequestHelp";
    public const string ScheduleMeeting = "ScheduleMeeting";
    public const string GenerateReport = "GenerateReport";

    public static readonly Dictionary<string, NbaActionTypeInfo> Types = new()
    {
        [Remind] = new NbaActionTypeInfo { Code = Remind, Name = "Send Reminder", Description = "Send reminder notification to task owner", AutoExecutable = true },
        [Reassign] = new NbaActionTypeInfo { Code = Reassign, Name = "Reassign Task", Description = "Transfer task to another owner", AutoExecutable = false },
        [SplitTask] = new NbaActionTypeInfo { Code = SplitTask, Name = "Split Task", Description = "Break large task into smaller subtasks", AutoExecutable = false },
        [AutoCollect] = new NbaActionTypeInfo { Code = AutoCollect, Name = "Auto-Collect Evidence", Description = "Trigger automated evidence collection", AutoExecutable = true },
        [ReduceScope] = new NbaActionTypeInfo { Code = ReduceScope, Name = "Reduce Scope", Description = "Defer non-mandatory controls or reduce complexity", AutoExecutable = false },
        [Escalate] = new NbaActionTypeInfo { Code = Escalate, Name = "Escalate", Description = "Escalate to manager or admin", AutoExecutable = true },
        [PauseExplain] = new NbaActionTypeInfo { Code = PauseExplain, Name = "Pause & Explain", Description = "Pause workflow and provide explanation", AutoExecutable = false },
        [Complete] = new NbaActionTypeInfo { Code = Complete, Name = "Complete Task", Description = "Mark task as completed", AutoExecutable = false },
        [Review] = new NbaActionTypeInfo { Code = Review, Name = "Review Item", Description = "Review pending item", AutoExecutable = false },
        [Approve] = new NbaActionTypeInfo { Code = Approve, Name = "Approve", Description = "Approve pending approval request", AutoExecutable = false },
        [SubmitEvidence] = new NbaActionTypeInfo { Code = SubmitEvidence, Name = "Submit Evidence", Description = "Submit required evidence for control", AutoExecutable = false },
        [UpdateStatus] = new NbaActionTypeInfo { Code = UpdateStatus, Name = "Update Status", Description = "Update item status", AutoExecutable = false },
        [RequestHelp] = new NbaActionTypeInfo { Code = RequestHelp, Name = "Request Help", Description = "Request assistance from support", AutoExecutable = false },
        [ScheduleMeeting] = new NbaActionTypeInfo { Code = ScheduleMeeting, Name = "Schedule Meeting", Description = "Schedule coordination meeting", AutoExecutable = false },
        [GenerateReport] = new NbaActionTypeInfo { Code = GenerateReport, Name = "Generate Report", Description = "Generate progress or compliance report", AutoExecutable = true }
    };
}

public class NbaActionTypeInfo
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public bool AutoExecutable { get; set; }
}

#endregion

#region Progress Certainty Index (PCI)

/// <summary>
/// Progress Certainty Index - Predictive score indicating likelihood of on-time completion.
/// Tracks velocity, rejection rates, SLA adherence, and other factors.
/// </summary>
[Table("ProgressCertaintyIndexes")]
[Index(nameof(TenantId), nameof(CalculatedAt), Name = "IX_PCI_Tenant_Date")]
[Index(nameof(EntityType), nameof(EntityId), Name = "IX_PCI_Entity")]
public class ProgressCertaintyIndex : BaseEntity
{
    [Required]
    public Guid TenantId { get; set; }

    /// <summary>
    /// Entity type this PCI is for (Plan, Mission, Assessment, Tenant)
    /// </summary>
    [Required]
    [StringLength(50)]
    public string EntityType { get; set; } = "Tenant";

    /// <summary>
    /// Entity ID if applicable
    /// </summary>
    public Guid? EntityId { get; set; }

    /// <summary>
    /// Overall PCI score (0-100)
    /// </summary>
    [Required]
    [Range(0, 100)]
    public int Score { get; set; } = 50;

    /// <summary>
    /// Risk band classification
    /// </summary>
    [Required]
    [StringLength(20)]
    public string RiskBand { get; set; } = "Medium"; // VeryLow, Low, Medium, High, Critical

    /// <summary>
    /// Confidence level in this score
    /// </summary>
    [Range(0, 100)]
    public int ConfidenceLevel { get; set; } = 50;

    #region Input Metrics

    /// <summary>
    /// Percentage of tasks completed (0-100)
    /// </summary>
    [Range(0, 100)]
    public double TasksCompletedPercent { get; set; }

    /// <summary>
    /// Task completion velocity (tasks per week)
    /// </summary>
    public double TaskVelocity { get; set; }

    /// <summary>
    /// Velocity trend: Improving, Stable, Declining
    /// </summary>
    [StringLength(20)]
    public string? VelocityTrend { get; set; }

    /// <summary>
    /// Evidence rejection rate (0-100)
    /// </summary>
    [Range(0, 100)]
    public double EvidenceRejectionRate { get; set; }

    /// <summary>
    /// SLA breach frequency (count in period)
    /// </summary>
    public int SlaBreachCount { get; set; }

    /// <summary>
    /// SLA adherence percentage (0-100)
    /// </summary>
    [Range(0, 100)]
    public double SlaAdherencePercent { get; set; }

    /// <summary>
    /// Average days tasks are overdue
    /// </summary>
    public double AverageOverdueDays { get; set; }

    /// <summary>
    /// Organization maturity score from onboarding
    /// </summary>
    [Range(1, 5)]
    public int? OrgMaturityLevel { get; set; }

    /// <summary>
    /// Mission/plan complexity score
    /// </summary>
    [Range(1, 10)]
    public int? ComplexityScore { get; set; }

    /// <summary>
    /// Total tasks in scope
    /// </summary>
    public int TotalTasks { get; set; }

    /// <summary>
    /// Tasks completed
    /// </summary>
    public int CompletedTasks { get; set; }

    /// <summary>
    /// Tasks currently overdue
    /// </summary>
    public int OverdueTasks { get; set; }

    /// <summary>
    /// Tasks at risk of being overdue
    /// </summary>
    public int AtRiskTasks { get; set; }

    #endregion

    /// <summary>
    /// JSON array of primary risk factors
    /// </summary>
    public string? PrimaryRiskFactorsJson { get; set; }

    /// <summary>
    /// JSON array of contributing factors with weights
    /// </summary>
    public string? FactorBreakdownJson { get; set; }

    /// <summary>
    /// Recommended intervention based on PCI
    /// </summary>
    [StringLength(500)]
    public string? RecommendedIntervention { get; set; }

    /// <summary>
    /// Predicted completion date based on current velocity
    /// </summary>
    public DateTime? PredictedCompletionDate { get; set; }

    /// <summary>
    /// Target completion date
    /// </summary>
    public DateTime? TargetCompletionDate { get; set; }

    /// <summary>
    /// Days ahead (+) or behind (-) schedule
    /// </summary>
    public int? DaysFromBaseline { get; set; }

    /// <summary>
    /// When this PCI was calculated
    /// </summary>
    [Required]
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Previous PCI score for trend tracking
    /// </summary>
    public int? PreviousScore { get; set; }

    /// <summary>
    /// Score change from previous calculation
    /// </summary>
    public int? ScoreChange { get; set; }

    [NotMapped]
    public List<string> PrimaryRiskFactors
    {
        get => string.IsNullOrEmpty(PrimaryRiskFactorsJson)
            ? new List<string>()
            : System.Text.Json.JsonSerializer.Deserialize<List<string>>(PrimaryRiskFactorsJson) ?? new();
        set => PrimaryRiskFactorsJson = System.Text.Json.JsonSerializer.Serialize(value);
    }
}

/// <summary>
/// Risk band thresholds for PCI scoring
/// </summary>
public static class PciRiskBands
{
    public const string VeryLow = "VeryLow";   // 80-100
    public const string Low = "Low";           // 60-79
    public const string Medium = "Medium";     // 40-59
    public const string High = "High";         // 20-39
    public const string Critical = "Critical"; // 0-19

    public static string GetRiskBand(int score) => score switch
    {
        >= 80 => VeryLow,
        >= 60 => Low,
        >= 40 => Medium,
        >= 20 => High,
        _ => Critical
    };
}

#endregion

#region Engagement Metrics

/// <summary>
/// Real-time engagement metrics for a user/tenant tracking confidence, fatigue, and momentum.
/// </summary>
[Table("EngagementMetrics")]
[Index(nameof(TenantId), nameof(UserId), Name = "IX_Engagement_Tenant_User")]
[Index(nameof(RecordedAt), Name = "IX_Engagement_Date")]
public class EngagementMetrics : BaseEntity
{
    [Required]
    public Guid TenantId { get; set; }

    /// <summary>
    /// User these metrics are for (null = tenant-wide)
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// User confidence level (0-100)
    /// </summary>
    [Range(0, 100)]
    public int ConfidenceScore { get; set; } = 50;

    /// <summary>
    /// User fatigue/disengagement level (0-100, higher = more fatigued)
    /// </summary>
    [Range(0, 100)]
    public int FatigueScore { get; set; } = 0;

    /// <summary>
    /// User momentum/engagement progress (0-100)
    /// </summary>
    [Range(0, 100)]
    public int MomentumScore { get; set; } = 50;

    /// <summary>
    /// Overall engagement score (calculated)
    /// </summary>
    [Range(0, 100)]
    public int OverallEngagementScore { get; set; } = 50;

    #region Contributing Factors

    /// <summary>
    /// Session duration today (minutes)
    /// </summary>
    public int SessionDurationMinutes { get; set; }

    /// <summary>
    /// Actions taken in current session
    /// </summary>
    public int ActionsInSession { get; set; }

    /// <summary>
    /// Tasks completed today
    /// </summary>
    public int TasksCompletedToday { get; set; }

    /// <summary>
    /// Evidence submitted today
    /// </summary>
    public int EvidenceSubmittedToday { get; set; }

    /// <summary>
    /// Consecutive days active
    /// </summary>
    public int ConsecutiveActiveDays { get; set; }

    /// <summary>
    /// Error/rejection encounters today
    /// </summary>
    public int ErrorsEncounteredToday { get; set; }

    /// <summary>
    /// Help requests today
    /// </summary>
    public int HelpRequestsToday { get; set; }

    /// <summary>
    /// Average response time to notifications (minutes)
    /// </summary>
    public double AverageResponseTimeMinutes { get; set; }

    #endregion

    /// <summary>
    /// JSON breakdown of factor contributions
    /// </summary>
    public string? FactorBreakdownJson { get; set; }

    /// <summary>
    /// Detected engagement state
    /// </summary>
    [StringLength(30)]
    public string? EngagementState { get; set; } // Highly_Engaged, Engaged, Neutral, Disengaged, At_Risk

    /// <summary>
    /// Recommended action based on engagement state
    /// </summary>
    [StringLength(500)]
    public string? RecommendedAction { get; set; }

    /// <summary>
    /// When these metrics were recorded
    /// </summary>
    [Required]
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Engagement states
/// </summary>
public static class EngagementStates
{
    public const string HighlyEngaged = "Highly_Engaged";
    public const string Engaged = "Engaged";
    public const string Neutral = "Neutral";
    public const string Disengaged = "Disengaged";
    public const string AtRisk = "At_Risk";

    public static string GetState(int engagementScore) => engagementScore switch
    {
        >= 80 => HighlyEngaged,
        >= 60 => Engaged,
        >= 40 => Neutral,
        >= 20 => Disengaged,
        _ => AtRisk
    };
}

#endregion

#region Motivation Scoring

/// <summary>
/// Non-gamified intrinsic motivation scoring model based on interaction quality,
/// control alignment, and perceived task impact.
/// </summary>
[Table("MotivationScores")]
[Index(nameof(TenantId), nameof(UserId), Name = "IX_Motivation_Tenant_User")]
public class MotivationScore : BaseEntity
{
    [Required]
    public Guid TenantId { get; set; }

    public Guid? UserId { get; set; }

    /// <summary>
    /// Overall motivation score (0-100)
    /// </summary>
    [Required]
    [Range(0, 100)]
    public int Score { get; set; } = 50;

    #region Factor Scores

    /// <summary>
    /// Interaction quality score (clarity, responsiveness)
    /// </summary>
    [Range(0, 100)]
    public int InteractionQualityScore { get; set; } = 50;

    /// <summary>
    /// Control alignment score (user autonomy/control feeling)
    /// </summary>
    [Range(0, 100)]
    public int ControlAlignmentScore { get; set; } = 50;

    /// <summary>
    /// Task impact score (meaningfulness of progress)
    /// </summary>
    [Range(0, 100)]
    public int TaskImpactScore { get; set; } = 50;

    /// <summary>
    /// Progress visibility score
    /// </summary>
    [Range(0, 100)]
    public int ProgressVisibilityScore { get; set; } = 50;

    /// <summary>
    /// Achievement recognition score
    /// </summary>
    [Range(0, 100)]
    public int AchievementRecognitionScore { get; set; } = 50;

    #endregion

    /// <summary>
    /// JSON audit trail of score changes
    /// </summary>
    public string? AuditTrailJson { get; set; }

    /// <summary>
    /// Motivation level classification
    /// </summary>
    [StringLength(30)]
    public string? MotivationLevel { get; set; } // VeryHigh, High, Moderate, Low, VeryLow

    /// <summary>
    /// When score was last calculated
    /// </summary>
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Previous score for trend tracking
    /// </summary>
    public int? PreviousScore { get; set; }
}

#endregion

#region Conditional Logic Rules Engine

/// <summary>
/// Enhanced conditional logic rule for the fullplan specification.
/// Supports complex conditions with multiple operators.
/// </summary>
[Table("ConditionalLogicRules")]
[Index(nameof(IsActive), nameof(Priority), Name = "IX_CLR_Active_Priority")]
[Index(nameof(TriggerEvent), Name = "IX_CLR_Trigger")]
public class ConditionalLogicRule : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string RuleCode { get; set; } = null!;

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = null!;

    [StringLength(200)]
    public string? NameAr { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Rule category (Onboarding, Framework, Cloud, Region, Company, Policy, Workflow)
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Category { get; set; } = "General";

    /// <summary>
    /// Event that triggers rule evaluation
    /// </summary>
    [StringLength(100)]
    public string? TriggerEvent { get; set; } // OnboardingComplete, FrameworkSelected, RegionChanged, etc.

    /// <summary>
    /// JSON condition expression
    /// Format: { "type": "and|or", "conditions": [{ "field": "", "operator": "", "value": "" }] }
    /// </summary>
    [Required]
    public string ConditionJson { get; set; } = "{}";

    /// <summary>
    /// JSON actions to execute when condition is met
    /// Format: [{ "type": "", "parameters": {} }]
    /// </summary>
    [Required]
    public string ActionsJson { get; set; } = "[]";

    /// <summary>
    /// Priority (1 = highest, evaluated in order)
    /// </summary>
    [Range(1, 1000)]
    public int Priority { get; set; } = 500;

    /// <summary>
    /// Whether rule is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether to stop processing further rules after this one matches
    /// </summary>
    public bool StopOnMatch { get; set; } = false;

    /// <summary>
    /// Tenant ID if tenant-specific rule (null = platform-wide)
    /// </summary>
    public new Guid? TenantId { get; set; }

    /// <summary>
    /// Applicable frameworks (pipe-separated)
    /// </summary>
    [StringLength(500)]
    public string? ApplicableFrameworks { get; set; }

    /// <summary>
    /// Applicable regions (pipe-separated)
    /// </summary>
    [StringLength(500)]
    public string? ApplicableRegions { get; set; }

    /// <summary>
    /// Version for rule evolution tracking
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// Effective date (rule not evaluated before this)
    /// </summary>
    public DateTime? EffectiveFrom { get; set; }

    /// <summary>
    /// Expiry date (rule not evaluated after this)
    /// </summary>
    public DateTime? EffectiveTo { get; set; }
}

/// <summary>
/// Condition operators for rule evaluation
/// </summary>
public static class RuleOperators
{
    public const string Equals = "equals";
    public const string NotEquals = "not_equals";
    public const string Contains = "contains";
    public const string NotContains = "not_contains";
    public const string In = "in";
    public const string NotIn = "not_in";
    public const string GreaterThan = "greater_than";
    public const string LessThan = "less_than";
    public const string GreaterOrEqual = "greater_or_equal";
    public const string LessOrEqual = "less_or_equal";
    public const string IsEmpty = "is_empty";
    public const string IsNotEmpty = "is_not_empty";
    public const string StartsWith = "starts_with";
    public const string EndsWith = "ends_with";
    public const string Matches = "matches"; // Regex
}

/// <summary>
/// Action types for rule execution
/// </summary>
public static class RuleActionTypes
{
    public const string AddFramework = "add_framework";
    public const string RemoveFramework = "remove_framework";
    public const string SetFlag = "set_flag";
    public const string AddTask = "add_task";
    public const string RemoveTask = "remove_task";
    public const string AddControl = "add_control";
    public const string SetTimeline = "set_timeline";
    public const string SendNotification = "send_notification";
    public const string TriggerWorkflow = "trigger_workflow";
    public const string SetApprovalRequired = "set_approval_required";
    public const string AdjustScope = "adjust_scope";
    public const string CreateRecommendation = "create_recommendation";
}

/// <summary>
/// Execution log for conditional logic rules
/// </summary>
[Table("ConditionalLogicRuleExecutions")]
[Index(nameof(TenantId), nameof(ExecutedAt), Name = "IX_CLRE_Tenant_Date")]
public class ConditionalLogicRuleExecution : BaseEntity
{
    [Required]
    public Guid TenantId { get; set; }

    [Required]
    public Guid RuleId { get; set; }

    [ForeignKey("RuleId")]
    public virtual ConditionalLogicRule? Rule { get; set; }

    /// <summary>
    /// Trigger event that caused evaluation
    /// </summary>
    [StringLength(100)]
    public string? TriggerEvent { get; set; }

    /// <summary>
    /// Context data used for evaluation
    /// </summary>
    public string? ContextDataJson { get; set; }

    /// <summary>
    /// Whether condition evaluated to true
    /// </summary>
    public bool ConditionMatched { get; set; }

    /// <summary>
    /// Actions executed (if condition matched)
    /// </summary>
    public string? ExecutedActionsJson { get; set; }

    /// <summary>
    /// Execution result
    /// </summary>
    [StringLength(30)]
    public string Status { get; set; } = "Evaluated"; // Evaluated, Matched, Executed, Failed, Skipped

    /// <summary>
    /// Error message if execution failed
    /// </summary>
    [StringLength(2000)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Execution duration in milliseconds
    /// </summary>
    public int DurationMs { get; set; }

    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;

    [StringLength(100)]
    public string? ExecutedBy { get; set; }
}

#endregion

#region Evidence Confidence Scoring

/// <summary>
/// Enhanced evidence confidence scoring with detailed metrics.
/// </summary>
[Table("EvidenceConfidenceScores")]
[Index(nameof(EvidenceId), Name = "IX_ECS_Evidence")]
public class EvidenceConfidenceScore : BaseEntity
{
    [Required]
    public Guid TenantId { get; set; }

    [Required]
    public Guid EvidenceId { get; set; }

    /// <summary>
    /// Overall confidence score (0-100)
    /// </summary>
    [Required]
    [Range(0, 100)]
    public int OverallScore { get; set; } = 50;

    /// <summary>
    /// Confidence level classification
    /// </summary>
    [Required]
    [StringLength(20)]
    public string ConfidenceLevel { get; set; } = "Medium"; // VeryHigh, High, Medium, Low, VeryLow

    #region Component Scores

    /// <summary>
    /// Source credibility score (0-100)
    /// </summary>
    [Range(0, 100)]
    public int SourceCredibilityScore { get; set; } = 50;

    /// <summary>
    /// Completeness score (0-100)
    /// </summary>
    [Range(0, 100)]
    public int CompletenessScore { get; set; } = 50;

    /// <summary>
    /// Relevance score (0-100)
    /// </summary>
    [Range(0, 100)]
    public int RelevanceScore { get; set; } = 50;

    /// <summary>
    /// Timeliness score (0-100)
    /// </summary>
    [Range(0, 100)]
    public int TimelinessScore { get; set; } = 50;

    /// <summary>
    /// Automation coverage percentage (0-100)
    /// </summary>
    [Range(0, 100)]
    public int AutomationCoveragePercent { get; set; } = 0;

    /// <summary>
    /// Cross-verification score (0-100)
    /// </summary>
    [Range(0, 100)]
    public int CrossVerificationScore { get; set; } = 50;

    /// <summary>
    /// Format compliance score (0-100)
    /// </summary>
    [Range(0, 100)]
    public int FormatComplianceScore { get; set; } = 50;

    #endregion

    /// <summary>
    /// SLA adherence (days before/after deadline)
    /// </summary>
    public int? SlaAdherenceDays { get; set; }

    /// <summary>
    /// Whether SLA was met
    /// </summary>
    public bool? SlaMet { get; set; }

    /// <summary>
    /// Collection method (Manual, Automated, Hybrid)
    /// </summary>
    [StringLength(30)]
    public string? CollectionMethod { get; set; }

    /// <summary>
    /// Factors contributing to low confidence
    /// </summary>
    public string? LowConfidenceFactorsJson { get; set; }

    /// <summary>
    /// Recommended action based on confidence
    /// </summary>
    [StringLength(30)]
    public string RecommendedAction { get; set; } = "HumanReview"; // AutoApprove, HumanReview, RequestMore, Reject

    /// <summary>
    /// Whether human review was triggered
    /// </summary>
    public bool HumanReviewTriggered { get; set; }

    /// <summary>
    /// Human reviewer's override (if any)
    /// </summary>
    [StringLength(30)]
    public string? HumanReviewOutcome { get; set; }

    /// <summary>
    /// Reviewer feedback
    /// </summary>
    [StringLength(1000)]
    public string? ReviewerFeedback { get; set; }

    public DateTime ScoredAt { get; set; } = DateTime.UtcNow;

    [NotMapped]
    public List<string> LowConfidenceFactors
    {
        get => string.IsNullOrEmpty(LowConfidenceFactorsJson)
            ? new List<string>()
            : System.Text.Json.JsonSerializer.Deserialize<List<string>>(LowConfidenceFactorsJson) ?? new();
        set => LowConfidenceFactorsJson = System.Text.Json.JsonSerializer.Serialize(value);
    }
}

/// <summary>
/// Confidence level thresholds
/// </summary>
public static class ConfidenceLevels
{
    public const string VeryHigh = "VeryHigh"; // 90-100
    public const string High = "High";         // 75-89
    public const string Medium = "Medium";     // 50-74
    public const string Low = "Low";           // 25-49
    public const string VeryLow = "VeryLow";   // 0-24

    public static string GetLevel(int score) => score switch
    {
        >= 90 => VeryHigh,
        >= 75 => High,
        >= 50 => Medium,
        >= 25 => Low,
        _ => VeryLow
    };
}

#endregion

#region Agent Event Triggers

/// <summary>
/// Event-driven triggers for agent activation.
/// </summary>
[Table("AgentEventTriggers")]
[Index(nameof(EventType), nameof(IsActive), Name = "IX_AET_Event_Active")]
public class AgentEventTrigger : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string TriggerCode { get; set; } = null!;

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Event type that triggers this
    /// </summary>
    [Required]
    [StringLength(100)]
    public string EventType { get; set; } = null!;

    /// <summary>
    /// Agent code to activate
    /// </summary>
    [Required]
    [StringLength(50)]
    public string AgentCode { get; set; } = null!;

    /// <summary>
    /// Action for agent to perform
    /// </summary>
    [Required]
    [StringLength(100)]
    public string AgentAction { get; set; } = null!;

    /// <summary>
    /// Condition that must be met (JSON)
    /// </summary>
    public string? ConditionJson { get; set; }

    /// <summary>
    /// Parameters to pass to agent (JSON)
    /// </summary>
    public string? ParametersJson { get; set; }

    /// <summary>
    /// Delay before triggering (seconds)
    /// </summary>
    public int DelaySeconds { get; set; } = 0;

    /// <summary>
    /// Whether trigger is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Priority (lower = higher priority)
    /// </summary>
    public int Priority { get; set; } = 100;

    /// <summary>
    /// Tenant-specific trigger (null = platform-wide)
    /// </summary>
    public new Guid? TenantId { get; set; }

    /// <summary>
    /// Maximum times this trigger can fire per entity per day
    /// </summary>
    public int? MaxDailyExecutions { get; set; }

    /// <summary>
    /// Cooldown period between executions (seconds)
    /// </summary>
    public int? CooldownSeconds { get; set; }
}

/// <summary>
/// Standard event types for agent triggers
/// </summary>
public static class AgentTriggerEvents
{
    // Onboarding events
    public const string OnboardingStarted = "onboarding.started";
    public const string OnboardingStepCompleted = "onboarding.step_completed";
    public const string OnboardingCompleted = "onboarding.completed";

    // Task events
    public const string TaskCreated = "task.created";
    public const string TaskAssigned = "task.assigned";
    public const string TaskStarted = "task.started";
    public const string TaskCompleted = "task.completed";
    public const string TaskOverdue = "task.overdue";
    public const string TaskEscalated = "task.escalated";

    // Evidence events
    public const string EvidenceSubmitted = "evidence.submitted";
    public const string EvidenceApproved = "evidence.approved";
    public const string EvidenceRejected = "evidence.rejected";
    public const string EvidenceExpiring = "evidence.expiring";

    // Assessment events
    public const string AssessmentStarted = "assessment.started";
    public const string AssessmentCompleted = "assessment.completed";
    public const string AssessmentScoreChanged = "assessment.score_changed";

    // Workflow events
    public const string WorkflowStarted = "workflow.started";
    public const string WorkflowApprovalNeeded = "workflow.approval_needed";
    public const string WorkflowCompleted = "workflow.completed";
    public const string WorkflowFailed = "workflow.failed";

    // Compliance events
    public const string ComplianceDriftDetected = "compliance.drift_detected";
    public const string ComplianceThresholdBreached = "compliance.threshold_breached";

    // SLA events
    public const string SlaWarning = "sla.warning";
    public const string SlaBreach = "sla.breach";

    // User events
    public const string UserLogin = "user.login";
    public const string UserInactive = "user.inactive";
}

/// <summary>
/// Log of agent trigger executions
/// </summary>
[Table("AgentTriggerExecutions")]
[Index(nameof(TenantId), nameof(ExecutedAt), Name = "IX_ATE_Tenant_Date")]
public class AgentTriggerExecution : BaseEntity
{
    public Guid? TenantId { get; set; }

    [Required]
    public Guid TriggerId { get; set; }

    [ForeignKey("TriggerId")]
    public virtual AgentEventTrigger? Trigger { get; set; }

    [Required]
    [StringLength(100)]
    public string EventType { get; set; } = null!;

    /// <summary>
    /// Entity that triggered the event
    /// </summary>
    [StringLength(50)]
    public string? SourceEntityType { get; set; }

    public Guid? SourceEntityId { get; set; }

    /// <summary>
    /// Event payload (JSON)
    /// </summary>
    public string? EventPayloadJson { get; set; }

    /// <summary>
    /// Whether agent was invoked
    /// </summary>
    public bool AgentInvoked { get; set; }

    /// <summary>
    /// Agent action result ID
    /// </summary>
    public Guid? AgentActionId { get; set; }

    /// <summary>
    /// Execution status
    /// </summary>
    [StringLength(30)]
    public string Status { get; set; } = "Triggered"; // Triggered, ConditionNotMet, AgentInvoked, Completed, Failed, Skipped

    /// <summary>
    /// Error message if failed
    /// </summary>
    [StringLength(2000)]
    public string? ErrorMessage { get; set; }

    public int DurationMs { get; set; }

    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
}

#endregion

#region Agent Communication Contracts

/// <summary>
/// Defines communication contract between agents.
/// </summary>
[Table("AgentCommunicationContracts")]
[Index(nameof(FromAgentCode), nameof(ToAgentCode), Name = "IX_ACC_From_To")]
public class AgentCommunicationContract : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string ContractCode { get; set; } = null!;

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Source agent code
    /// </summary>
    [Required]
    [StringLength(50)]
    public string FromAgentCode { get; set; } = null!;

    /// <summary>
    /// Target agent code
    /// </summary>
    [Required]
    [StringLength(50)]
    public string ToAgentCode { get; set; } = null!;

    /// <summary>
    /// Request JSON schema definition
    /// </summary>
    [Required]
    public string RequestSchemaJson { get; set; } = "{}";

    /// <summary>
    /// Response JSON schema definition
    /// </summary>
    [Required]
    public string ResponseSchemaJson { get; set; } = "{}";

    /// <summary>
    /// Expected response description
    /// </summary>
    [StringLength(1000)]
    public string? ExpectedResponse { get; set; }

    /// <summary>
    /// Error handling rules (JSON array)
    /// </summary>
    public string? ErrorHandlingJson { get; set; }

    /// <summary>
    /// Validation rules (JSON array)
    /// </summary>
    public string? ValidationRulesJson { get; set; }

    /// <summary>
    /// Example request/response (JSON)
    /// </summary>
    public string? ExampleJson { get; set; }

    /// <summary>
    /// Timeout for response (seconds)
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Whether contract is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Contract version
    /// </summary>
    public int Version { get; set; } = 1;
}

/// <summary>
/// Log of inter-agent communications
/// </summary>
[Table("AgentCommunicationLogs")]
[Index(nameof(TenantId), nameof(Timestamp), Name = "IX_ACL_Tenant_Time")]
[Index(nameof(CorrelationId), Name = "IX_ACL_Correlation")]
public class AgentCommunicationLog : BaseEntity
{
    public Guid? TenantId { get; set; }

    [Required]
    [StringLength(50)]
    public string CorrelationId { get; set; } = null!;

    public Guid? ContractId { get; set; }

    [ForeignKey("ContractId")]
    public virtual AgentCommunicationContract? Contract { get; set; }

    [Required]
    [StringLength(50)]
    public string FromAgentCode { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string ToAgentCode { get; set; } = null!;

    /// <summary>
    /// Request payload
    /// </summary>
    public string? RequestPayloadJson { get; set; }

    /// <summary>
    /// Response payload
    /// </summary>
    public string? ResponsePayloadJson { get; set; }

    /// <summary>
    /// Communication status
    /// </summary>
    [StringLength(30)]
    public string Status { get; set; } = "Sent"; // Sent, Received, Processed, Failed, Timeout

    /// <summary>
    /// Error code if failed
    /// </summary>
    [StringLength(50)]
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Error message if failed
    /// </summary>
    [StringLength(2000)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Round-trip duration in milliseconds
    /// </summary>
    public int DurationMs { get; set; }

    /// <summary>
    /// Number of retry attempts
    /// </summary>
    public int RetryCount { get; set; } = 0;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public DateTime? ResponseTimestamp { get; set; }
}

#endregion
