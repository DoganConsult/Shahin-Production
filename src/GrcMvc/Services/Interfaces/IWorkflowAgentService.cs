using GrcMvc.Models.Entities;

namespace GrcMvc.Services.Interfaces;

/// <summary>
/// AI-powered workflow optimization agent service.
/// Capabilities: OptimizeWorkflow, IdentifyBottlenecks, SuggestImprovements, AutomateRouting, ManageDeadlines, EscalateOverdue
/// Data Sources: Workflows, Tasks, SLAs, ProcessMetrics, UserWorkload
/// </summary>
public interface IWorkflowAgentService
{
    /// <summary>
    /// Analyze and optimize a workflow definition or instance
    /// </summary>
    Task<WorkflowAgentOptimizationResult> OptimizeWorkflowAsync(
        Guid workflowId,
        bool isInstance = true,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Identify bottlenecks in workflow execution
    /// </summary>
    Task<BottleneckAnalysis> IdentifyBottlenecksAsync(
        Guid? workflowId = null,
        int daysBack = 30,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get AI-generated improvement suggestions for workflows
    /// </summary>
    Task<List<WorkflowImprovement>> SuggestImprovementsAsync(
        Guid? workflowDefinitionId = null,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Automatically route a task to the most appropriate user based on workload and skills
    /// </summary>
    Task<TaskRoutingResult> AutoRouteTaskAsync(
        Guid taskId,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyze and manage upcoming deadlines
    /// </summary>
    Task<DeadlineManagementResult> ManageDeadlinesAsync(
        int daysAhead = 7,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Identify and escalate overdue tasks
    /// </summary>
    Task<EscalationResult> EscalateOverdueTasksAsync(
        bool autoEscalate = false,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get workflow performance metrics and analytics
    /// </summary>
    Task<WorkflowMetrics> GetWorkflowMetricsAsync(
        Guid? workflowDefinitionId = null,
        int daysBack = 30,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyze user workload distribution
    /// </summary>
    Task<UserWorkloadAnalysis> AnalyzeUserWorkloadAsync(
        Guid? userId = null,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send reminders for upcoming deadlines
    /// </summary>
    Task<ReminderResult> SendDeadlineRemindersAsync(
        int hoursBeforeDeadline = 24,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate workflow status report
    /// </summary>
    Task<WorkflowStatusReport> GenerateStatusReportAsync(
        Guid? workflowId = null,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default);
}

#region Result DTOs

/// <summary>
/// Result of workflow optimization analysis (WorkflowAgentService specific)
/// </summary>
public class WorkflowAgentOptimizationResult
{
    public Guid WorkflowId { get; set; }
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
    public string WorkflowName { get; set; } = string.Empty;
    public int CurrentEfficiencyScore { get; set; } // 0-100
    public int PotentialEfficiencyScore { get; set; } // 0-100 after optimization
    public List<OptimizationRecommendation> Recommendations { get; set; } = new();
    public List<string> IdentifiedIssues { get; set; } = new();
    public Dictionary<string, object>? CurrentMetrics { get; set; }
    public Dictionary<string, object>? ProjectedMetrics { get; set; }
    public string Summary { get; set; } = string.Empty;
}

/// <summary>
/// Optimization recommendation
/// </summary>
public class OptimizationRecommendation
{
    public string Category { get; set; } = string.Empty; // "routing", "sla", "parallelization", "automation"
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium"; // Critical, High, Medium, Low
    public int ImpactScore { get; set; } // 1-10
    public int EffortScore { get; set; } // 1-10 (lower is easier)
    public List<string> Steps { get; set; } = new();
    public bool RequiresApproval { get; set; } = true;
}

/// <summary>
/// Bottleneck analysis result
/// </summary>
public class BottleneckAnalysis
{
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan AnalysisPeriod { get; set; }
    public int TotalWorkflowsAnalyzed { get; set; }
    public int TotalTasksAnalyzed { get; set; }
    public List<Bottleneck> Bottlenecks { get; set; } = new();
    public List<SlowStep> SlowestSteps { get; set; } = new();
    public List<OverloadedUser> OverloadedUsers { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
    public List<string> RecommendedActions { get; set; } = new();
}

/// <summary>
/// Identified bottleneck
/// </summary>
public class Bottleneck
{
    public string BottleneckType { get; set; } = string.Empty; // "user", "step", "sla", "approval"
    public string Location { get; set; } = string.Empty; // Step name or user name
    public string Description { get; set; } = string.Empty;
    public int Severity { get; set; } // 1-10
    public double AverageDelayHours { get; set; }
    public int AffectedWorkflows { get; set; }
    public string? SuggestedFix { get; set; }
}

/// <summary>
/// Slow workflow step
/// </summary>
public class SlowStep
{
    public string StepName { get; set; } = string.Empty;
    public string WorkflowType { get; set; } = string.Empty;
    public double AverageCompletionHours { get; set; }
    public double ExpectedCompletionHours { get; set; }
    public int OccurrenceCount { get; set; }
    public string? CommonCause { get; set; }
}

/// <summary>
/// User with excessive workload
/// </summary>
public class OverloadedUser
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int ActiveTaskCount { get; set; }
    public int OverdueTaskCount { get; set; }
    public double AverageTasksPerDay { get; set; }
    public double WorkloadScore { get; set; } // 0-100, >80 is overloaded
}

/// <summary>
/// Workflow improvement suggestion
/// </summary>
public class WorkflowImprovement
{
    public string Category { get; set; } = string.Empty; // "efficiency", "quality", "compliance", "ux"
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CurrentState { get; set; } = string.Empty;
    public string ProposedState { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium";
    public int EstimatedImpact { get; set; } // 1-10
    public List<string> AffectedWorkflows { get; set; } = new();
    public List<string> ImplementationSteps { get; set; } = new();
    public bool RequiresApproval { get; set; } = true;
}

/// <summary>
/// Result of automatic task routing
/// </summary>
public class TaskRoutingResult
{
    public Guid TaskId { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public DateTime RoutedAt { get; set; } = DateTime.UtcNow;
    public bool Success { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }
    public string RoutingReason { get; set; } = string.Empty;
    public List<RoutingCandidate> ConsideredCandidates { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Candidate user for task routing
/// </summary>
public class RoutingCandidate
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int CurrentWorkload { get; set; }
    public int SkillMatch { get; set; } // 0-100
    public int AvailabilityScore { get; set; } // 0-100
    public int OverallScore { get; set; } // Combined score
    public string? Notes { get; set; }
}

/// <summary>
/// Result of deadline management analysis
/// </summary>
public class DeadlineManagementResult
{
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
    public int DaysAhead { get; set; }
    public List<WorkflowTaskDeadline> UpcomingDeadlines { get; set; } = new();
    public List<DeadlineRisk> AtRiskDeadlines { get; set; } = new();
    public int OverdueCount { get; set; }
    public int DueTodayCount { get; set; }
    public int DueThisWeekCount { get; set; }
    public List<string> Recommendations { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
}

/// <summary>
/// Upcoming deadline information (WorkflowAgentService specific)
/// </summary>
public class WorkflowTaskDeadline
{
    public Guid TaskId { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public Guid? WorkflowInstanceId { get; set; }
    public string WorkflowName { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public TimeSpan TimeRemaining { get; set; }
    public string AssignedTo { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string Status { get; set; } = string.Empty;
    public int CompletionPercentage { get; set; }
}

/// <summary>
/// Deadline at risk of being missed
/// </summary>
public class DeadlineRisk
{
    public Guid TaskId { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public string RiskLevel { get; set; } = "Medium"; // High, Medium, Low
    public string RiskReason { get; set; } = string.Empty;
    public int CompletionPercentage { get; set; }
    public double EstimatedHoursRemaining { get; set; }
    public string? SuggestedAction { get; set; }
}

/// <summary>
/// Result of task escalation
/// </summary>
public class EscalationResult
{
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    public int TotalOverdueTasks { get; set; }
    public int TasksEscalated { get; set; }
    public int TasksAlreadyEscalated { get; set; }
    public List<EscalatedTask> EscalatedTasks { get; set; } = new();
    public List<EscalatedTask> PendingEscalation { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
}

/// <summary>
/// Escalated task details
/// </summary>
public class EscalatedTask
{
    public Guid TaskId { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public TimeSpan OverdueBy { get; set; }
    public int EscalationLevel { get; set; }
    public Guid OriginalAssignee { get; set; }
    public string OriginalAssigneeName { get; set; } = string.Empty;
    public Guid? EscalatedTo { get; set; }
    public string? EscalatedToName { get; set; }
    public string EscalationReason { get; set; } = string.Empty;
}

/// <summary>
/// Workflow performance metrics
/// </summary>
public class WorkflowMetrics
{
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan AnalysisPeriod { get; set; }
    public Guid? WorkflowDefinitionId { get; set; }
    public string? WorkflowName { get; set; }

    // Volume metrics
    public int TotalInstances { get; set; }
    public int CompletedInstances { get; set; }
    public int ActiveInstances { get; set; }
    public int CancelledInstances { get; set; }

    // Time metrics
    public double AverageCompletionDays { get; set; }
    public double MedianCompletionDays { get; set; }
    public double MinCompletionDays { get; set; }
    public double MaxCompletionDays { get; set; }

    // SLA metrics
    public int SlaBreachedCount { get; set; }
    public double SlaComplianceRate { get; set; } // percentage

    // Task metrics
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int OverdueTasks { get; set; }
    public int EscalatedTasks { get; set; }

    // Trend
    public string Trend { get; set; } = "stable"; // improving, declining, stable
    public Dictionary<string, object>? AdditionalMetrics { get; set; }
}

/// <summary>
/// User workload analysis
/// </summary>
public class UserWorkloadAnalysis
{
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
    public Guid? UserId { get; set; }
    public int TotalUsersAnalyzed { get; set; }
    public List<UserWorkloadDetail> UserWorkloads { get; set; } = new();
    public WorkloadDistribution Distribution { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
}

/// <summary>
/// Detailed user workload
/// </summary>
public class UserWorkloadDetail
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int ActiveTasks { get; set; }
    public int PendingTasks { get; set; }
    public int OverdueTasks { get; set; }
    public int CompletedThisWeek { get; set; }
    public double AverageCompletionHours { get; set; }
    public double WorkloadScore { get; set; } // 0-100
    public string WorkloadLevel { get; set; } = "Normal"; // Light, Normal, Heavy, Overloaded
    public int Capacity { get; set; } // Estimated additional tasks
}

/// <summary>
/// Workload distribution across team
/// </summary>
public class WorkloadDistribution
{
    public int TotalActiveTasks { get; set; }
    public double AverageTasksPerUser { get; set; }
    public double StandardDeviation { get; set; }
    public int UnderutilizedUsers { get; set; }
    public int OverloadedUsers { get; set; }
    public double DistributionScore { get; set; } // 0-100, higher is better balanced
}

/// <summary>
/// Result of sending deadline reminders
/// </summary>
public class ReminderResult
{
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    public int RemindersSent { get; set; }
    public int TasksWithUpcomingDeadlines { get; set; }
    public List<ReminderSent> Reminders { get; set; } = new();
    public List<string>? Errors { get; set; }
}

/// <summary>
/// Individual reminder sent
/// </summary>
public class ReminderSent
{
    public Guid TaskId { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public string ReminderType { get; set; } = string.Empty; // "24h", "48h", "1week"
    public bool Success { get; set; }
}

/// <summary>
/// Workflow status report
/// </summary>
public class WorkflowStatusReport
{
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public Guid? WorkflowId { get; set; }
    public string? WorkflowName { get; set; }

    // Overall status
    public string OverallStatus { get; set; } = "Unknown"; // Healthy, AtRisk, Critical
    public int HealthScore { get; set; } // 0-100

    // Summary counts
    public int ActiveWorkflows { get; set; }
    public int PendingTasks { get; set; }
    public int OverdueTasks { get; set; }
    public int TasksDueToday { get; set; }

    // Key issues
    public List<WorkflowIssue> Issues { get; set; } = new();

    // AI-generated summary
    public string ExecutiveSummary { get; set; } = string.Empty;
    public List<string> KeyHighlights { get; set; } = new();
    public List<string> RecommendedActions { get; set; } = new();
}

/// <summary>
/// Workflow issue
/// </summary>
public class WorkflowIssue
{
    public string Category { get; set; } = string.Empty; // "sla", "bottleneck", "escalation", "capacity"
    public string Severity { get; set; } = "Medium";
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? AffectedWorkflow { get; set; }
    public Guid? AffectedTaskId { get; set; }
    public string? SuggestedAction { get; set; }
}

#endregion
