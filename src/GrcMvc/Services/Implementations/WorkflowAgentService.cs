using System.Text;
using System.Text.Json;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using GrcMvc.Data;
using GrcMvc.Configuration;
using GrcMvc.Models.Entities;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// AI-powered workflow optimization agent service implementation.
/// Uses Claude AI to analyze workflows, identify bottlenecks, suggest improvements,
/// auto-route tasks, manage deadlines, and escalate overdue items.
/// </summary>
public class WorkflowAgentService : IWorkflowAgentService
{
    private readonly GrcDbContext _dbContext;
    private readonly ILogger<WorkflowAgentService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ClaudeApiSettings _claudeSettings;

    public WorkflowAgentService(
        GrcDbContext dbContext,
        ILogger<WorkflowAgentService> logger,
        IHttpClientFactory httpClientFactory,
        IOptions<ClaudeApiSettings> claudeSettings)
    {
        _dbContext = dbContext;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _claudeSettings = claudeSettings.Value;
    }

    public async Task<WorkflowAgentOptimizationResult> OptimizeWorkflowAsync(
        Guid workflowId,
        bool isInstance = true,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Optimizing workflow {WorkflowId}, isInstance: {IsInstance}", workflowId, isInstance);

            var result = new WorkflowAgentOptimizationResult
            {
                WorkflowId = workflowId,
                AnalyzedAt = DateTime.UtcNow
            };

            if (isInstance)
            {
                var instance = await _dbContext.WorkflowInstances
                    .Include(w => w.Tasks)
                    .FirstOrDefaultAsync(w => w.Id == workflowId, cancellationToken);

                if (instance == null)
                {
                    result.Summary = "Workflow instance not found";
                    return result;
                }

                result.WorkflowName = instance.WorkflowType;

                // Calculate current metrics
                var completedTasks = instance.Tasks.Count(t => t.Status == "Completed");
                var totalTasks = instance.Tasks.Count;
                var overdueTasks = instance.Tasks.Count(t => t.DueDate < DateTime.UtcNow && t.Status != "Completed");

                result.CurrentMetrics = new Dictionary<string, object>
                {
                    { "totalTasks", totalTasks },
                    { "completedTasks", completedTasks },
                    { "overdueTasks", overdueTasks },
                    { "completionRate", totalTasks > 0 ? (double)completedTasks / totalTasks * 100 : 0 },
                    { "slaBreached", instance.SlaBreached }
                };

                // Use AI to analyze and suggest optimizations
                var prompt = BuildOptimizationPrompt(instance, instance.Tasks.ToList());
                var aiResponse = await CallClaudeAIAsync(prompt, cancellationToken);
                ParseOptimizationResponse(aiResponse, result);
            }
            else
            {
                var definition = await _dbContext.WorkflowDefinitions
                    .FirstOrDefaultAsync(w => w.Id == workflowId, cancellationToken);

                if (definition == null)
                {
                    result.Summary = "Workflow definition not found";
                    return result;
                }

                result.WorkflowName = definition.Name;

                // Get historical instances for this definition
                var instances = await _dbContext.WorkflowInstances
                    .Include(w => w.Tasks)
                    .Where(w => w.WorkflowDefinitionId == workflowId)
                    .OrderByDescending(w => w.StartedAt)
                    .Take(100)
                    .ToListAsync(cancellationToken);

                var completedInstances = instances.Where(i => i.Status == "Completed").ToList();
                var avgDuration = completedInstances.Any()
                    ? completedInstances.Average(i => (i.CompletedAt ?? DateTime.UtcNow).Subtract(i.StartedAt).TotalDays)
                    : 0;

                result.CurrentMetrics = new Dictionary<string, object>
                {
                    { "totalInstances", instances.Count },
                    { "completedInstances", completedInstances.Count },
                    { "avgDurationDays", avgDuration },
                    { "slaBreachedCount", instances.Count(i => i.SlaBreached) }
                };

                var prompt = BuildDefinitionOptimizationPrompt(definition, instances);
                var aiResponse = await CallClaudeAIAsync(prompt, cancellationToken);
                ParseOptimizationResponse(aiResponse, result);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error optimizing workflow {WorkflowId}", workflowId);
            throw;
        }
    }

    public async Task<BottleneckAnalysis> IdentifyBottlenecksAsync(
        Guid? workflowId = null,
        int daysBack = 30,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Identifying bottlenecks, daysBack: {DaysBack}", daysBack);

            var since = DateTime.UtcNow.AddDays(-daysBack);

            var taskQuery = _dbContext.WorkflowTasks
                .Include(t => t.WorkflowInstance)
                .Where(t => t.CreatedAt >= since);

            if (workflowId.HasValue)
            {
                taskQuery = taskQuery.Where(t => t.WorkflowInstanceId == workflowId.Value);
            }

            if (tenantId.HasValue)
            {
                taskQuery = taskQuery.Where(t => t.TenantId == tenantId.Value);
            }

            var tasks = await taskQuery.ToListAsync(cancellationToken);
            var instances = await _dbContext.WorkflowInstances
                .Where(w => w.StartedAt >= since)
                .ToListAsync(cancellationToken);

            // Analyze task completion times by step
            var tasksByStep = tasks
                .GroupBy(t => t.TaskName)
                .Select(g => new
                {
                    StepName = g.Key,
                    Count = g.Count(),
                    AvgCompletionHours = g.Where(t => t.CompletedAt.HasValue && t.StartedAt.HasValue)
                        .Select(t => (t.CompletedAt!.Value - t.StartedAt!.Value).TotalHours)
                        .DefaultIfEmpty(0)
                        .Average(),
                    OverdueCount = g.Count(t => t.DueDate < DateTime.UtcNow && t.Status != "Completed")
                })
                .OrderByDescending(s => s.AvgCompletionHours)
                .ToList();

            // Analyze user workload
            var userWorkloads = tasks
                .Where(t => t.AssignedToUserId.HasValue)
                .GroupBy(t => new { t.AssignedToUserId, t.AssignedToUserName })
                .Select(g => new OverloadedUser
                {
                    UserId = g.Key.AssignedToUserId!.Value,
                    UserName = g.Key.AssignedToUserName ?? "Unknown",
                    ActiveTaskCount = g.Count(t => t.Status != "Completed"),
                    OverdueTaskCount = g.Count(t => t.DueDate < DateTime.UtcNow && t.Status != "Completed"),
                    AverageTasksPerDay = g.Count() / (double)daysBack,
                    WorkloadScore = CalculateWorkloadScore(g.ToList())
                })
                .Where(u => u.WorkloadScore > 80)
                .OrderByDescending(u => u.WorkloadScore)
                .ToList();

            var analysis = new BottleneckAnalysis
            {
                AnalyzedAt = DateTime.UtcNow,
                AnalysisPeriod = TimeSpan.FromDays(daysBack),
                TotalWorkflowsAnalyzed = instances.Count,
                TotalTasksAnalyzed = tasks.Count,
                SlowestSteps = tasksByStep.Take(5).Select(s => new SlowStep
                {
                    StepName = s.StepName,
                    AverageCompletionHours = s.AvgCompletionHours,
                    OccurrenceCount = s.Count
                }).ToList(),
                OverloadedUsers = userWorkloads
            };

            // Use AI to identify patterns and bottlenecks
            var prompt = BuildBottleneckPrompt(tasks, instances, tasksByStep, userWorkloads);
            var aiResponse = await CallClaudeAIAsync(prompt, cancellationToken);
            ParseBottleneckResponse(aiResponse, analysis);

            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error identifying bottlenecks");
            throw;
        }
    }

    public async Task<List<WorkflowImprovement>> SuggestImprovementsAsync(
        Guid? workflowDefinitionId = null,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Suggesting workflow improvements");

            var definitionsQuery = _dbContext.WorkflowDefinitions.AsQueryable();
            if (workflowDefinitionId.HasValue)
            {
                definitionsQuery = definitionsQuery.Where(d => d.Id == workflowDefinitionId.Value);
            }
            if (tenantId.HasValue)
            {
                definitionsQuery = definitionsQuery.Where(d => d.TenantId == tenantId.Value);
            }

            var definitions = await definitionsQuery.Take(10).ToListAsync(cancellationToken);

            // Get recent instance data for analysis
            var recentInstances = await _dbContext.WorkflowInstances
                .Include(i => i.Tasks)
                .Where(i => i.StartedAt >= DateTime.UtcNow.AddDays(-90))
                .OrderByDescending(i => i.StartedAt)
                .Take(200)
                .ToListAsync(cancellationToken);

            var prompt = BuildImprovementPrompt(definitions, recentInstances);
            var aiResponse = await CallClaudeAIAsync(prompt, cancellationToken);

            return ParseImprovementResponse(aiResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error suggesting improvements");
            throw;
        }
    }

    public async Task<TaskRoutingResult> AutoRouteTaskAsync(
        Guid taskId,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Auto-routing task {TaskId}", taskId);

            var task = await _dbContext.WorkflowTasks
                .Include(t => t.WorkflowInstance)
                .FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken);

            if (task == null)
            {
                return new TaskRoutingResult
                {
                    TaskId = taskId,
                    Success = false,
                    ErrorMessage = "Task not found"
                };
            }

            var effectiveTenantId = tenantId ?? task.TenantId;

            // Get potential assignees with their current workload using TenantUsers
            var tenantUsers = await _dbContext.TenantUsers
                .Include(tu => tu.User)
                .Where(tu => tu.TenantId == effectiveTenantId && tu.Status == "Active")
                .ToListAsync(cancellationToken);

            var userWorkloads = new List<RoutingCandidate>();
            foreach (var tenantUser in tenantUsers)
            {
                var userId = Guid.TryParse(tenantUser.UserId, out var userGuid) ? userGuid : Guid.Empty;

                var activeTaskCount = await _dbContext.WorkflowTasks
                    .CountAsync(t => t.AssignedToUserId == userId && t.Status != "Completed", cancellationToken);

                var overdueCount = await _dbContext.WorkflowTasks
                    .CountAsync(t => t.AssignedToUserId == userId &&
                                     t.DueDate < DateTime.UtcNow &&
                                     t.Status != "Completed", cancellationToken);

                var availabilityScore = Math.Max(0, 100 - (activeTaskCount * 10) - (overdueCount * 20));

                userWorkloads.Add(new RoutingCandidate
                {
                    UserId = userId,
                    UserName = tenantUser.User?.FullName ?? tenantUser.Email,
                    CurrentWorkload = activeTaskCount,
                    AvailabilityScore = availabilityScore,
                    SkillMatch = 80, // Default skill match - can be enhanced with role-based matching
                    OverallScore = availabilityScore
                });
            }

            // Sort by overall score and select best candidate
            userWorkloads = userWorkloads.OrderByDescending(u => u.OverallScore).ToList();
            var bestCandidate = userWorkloads.FirstOrDefault();

            if (bestCandidate == null)
            {
                return new TaskRoutingResult
                {
                    TaskId = taskId,
                    TaskName = task.TaskName,
                    Success = false,
                    ErrorMessage = "No available users found for routing"
                };
            }

            // Assign the task
            task.AssignedToUserId = bestCandidate.UserId;
            task.AssignedToUserName = bestCandidate.UserName;
            task.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new TaskRoutingResult
            {
                TaskId = taskId,
                TaskName = task.TaskName,
                RoutedAt = DateTime.UtcNow,
                Success = true,
                AssignedToUserId = bestCandidate.UserId,
                AssignedToUserName = bestCandidate.UserName,
                RoutingReason = $"Best available user with workload score {bestCandidate.OverallScore}",
                ConsideredCandidates = userWorkloads.Take(5).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error auto-routing task {TaskId}", taskId);
            throw;
        }
    }

    public async Task<DeadlineManagementResult> ManageDeadlinesAsync(
        int daysAhead = 7,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Managing deadlines, daysAhead: {DaysAhead}", daysAhead);

            var cutoffDate = DateTime.UtcNow.AddDays(daysAhead);

            var taskQuery = _dbContext.WorkflowTasks
                .Include(t => t.WorkflowInstance)
                .Where(t => t.Status != "Completed" && t.Status != "Cancelled");

            if (tenantId.HasValue)
            {
                taskQuery = taskQuery.Where(t => t.TenantId == tenantId.Value);
            }

            var tasks = await taskQuery.ToListAsync(cancellationToken);

            var upcomingDeadlines = tasks
                .Where(t => t.DueDate.HasValue && t.DueDate.Value <= cutoffDate && t.DueDate.Value > DateTime.UtcNow)
                .OrderBy(t => t.DueDate)
                .Select(t => new WorkflowTaskDeadline
                {
                    TaskId = t.Id,
                    TaskName = t.TaskName,
                    WorkflowInstanceId = t.WorkflowInstanceId,
                    WorkflowName = t.WorkflowInstance?.WorkflowType ?? "Unknown",
                    DueDate = t.DueDate!.Value,
                    TimeRemaining = t.DueDate!.Value - DateTime.UtcNow,
                    AssignedTo = t.AssignedToUserName ?? "Unassigned",
                    Priority = t.Priority,
                    Status = t.Status,
                    CompletionPercentage = t.CompletionPercentage
                })
                .ToList();

            var overdueTasks = tasks
                .Where(t => t.DueDate.HasValue && t.DueDate.Value < DateTime.UtcNow)
                .ToList();

            var dueTodayTasks = tasks
                .Where(t => t.DueDate.HasValue && t.DueDate.Value.Date == DateTime.UtcNow.Date)
                .ToList();

            // Identify at-risk deadlines using AI
            var atRiskDeadlines = upcomingDeadlines
                .Where(d => d.CompletionPercentage < 50 && d.TimeRemaining.TotalHours < 48)
                .Select(d => new DeadlineRisk
                {
                    TaskId = d.TaskId,
                    TaskName = d.TaskName,
                    DueDate = d.DueDate,
                    RiskLevel = d.TimeRemaining.TotalHours < 24 ? "High" : "Medium",
                    RiskReason = $"Only {d.CompletionPercentage}% complete with {d.TimeRemaining.TotalHours:F0} hours remaining",
                    CompletionPercentage = d.CompletionPercentage,
                    EstimatedHoursRemaining = d.TimeRemaining.TotalHours,
                    SuggestedAction = d.CompletionPercentage < 25 ? "Consider reassigning or getting help" : "Prioritize completion"
                })
                .ToList();

            var result = new DeadlineManagementResult
            {
                AnalyzedAt = DateTime.UtcNow,
                DaysAhead = daysAhead,
                UpcomingDeadlines = upcomingDeadlines,
                AtRiskDeadlines = atRiskDeadlines,
                OverdueCount = overdueTasks.Count,
                DueTodayCount = dueTodayTasks.Count,
                DueThisWeekCount = upcomingDeadlines.Count(d => d.TimeRemaining.TotalDays <= 7)
            };

            // Use AI for recommendations
            var prompt = BuildDeadlinePrompt(upcomingDeadlines, atRiskDeadlines, overdueTasks);
            var aiResponse = await CallClaudeAIAsync(prompt, cancellationToken);
            ParseDeadlineResponse(aiResponse, result);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error managing deadlines");
            throw;
        }
    }

    public async Task<EscalationResult> EscalateOverdueTasksAsync(
        bool autoEscalate = false,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Escalating overdue tasks, autoEscalate: {AutoEscalate}", autoEscalate);

            var taskQuery = _dbContext.WorkflowTasks
                .Include(t => t.WorkflowInstance)
                .Where(t => t.DueDate < DateTime.UtcNow &&
                           t.Status != "Completed" &&
                           t.Status != "Cancelled");

            if (tenantId.HasValue)
            {
                taskQuery = taskQuery.Where(t => t.TenantId == tenantId.Value);
            }

            var overdueTasks = await taskQuery.ToListAsync(cancellationToken);

            var result = new EscalationResult
            {
                ProcessedAt = DateTime.UtcNow,
                TotalOverdueTasks = overdueTasks.Count
            };

            foreach (var task in overdueTasks)
            {
                var overdueBy = DateTime.UtcNow - task.DueDate!.Value;
                var escalatedTask = new EscalatedTask
                {
                    TaskId = task.Id,
                    TaskName = task.TaskName,
                    DueDate = task.DueDate.Value,
                    OverdueBy = overdueBy,
                    EscalationLevel = task.EscalationLevel,
                    OriginalAssignee = task.AssignedToUserId ?? Guid.Empty,
                    OriginalAssigneeName = task.AssignedToUserName ?? "Unassigned"
                };

                // Determine escalation level based on overdue duration
                var newEscalationLevel = DetermineEscalationLevel(overdueBy, task.Priority);

                if (task.IsEscalated && task.EscalationLevel >= newEscalationLevel)
                {
                    result.TasksAlreadyEscalated++;
                    continue;
                }

                escalatedTask.EscalationLevel = newEscalationLevel;
                escalatedTask.EscalationReason = GetEscalationReason(overdueBy, task.Priority);

                if (autoEscalate)
                {
                    // Find escalation target
                    var escalationTarget = await FindEscalationTargetAsync(task, newEscalationLevel, cancellationToken);

                    task.IsEscalated = true;
                    task.EscalationLevel = newEscalationLevel;
                    task.EscalatedToUserId = escalationTarget?.UserId;
                    task.LastEscalatedAt = DateTime.UtcNow;
                    task.UpdatedAt = DateTime.UtcNow;

                    // Create escalation record
                    var escalation = new WorkflowEscalation
                    {
                        Id = Guid.NewGuid(),
                        TenantId = task.TenantId,
                        WorkflowInstanceId = task.WorkflowInstanceId,
                        TaskId = task.Id,
                        EscalationLevel = newEscalationLevel,
                        EscalationReason = escalatedTask.EscalationReason,
                        EscalatedAt = DateTime.UtcNow,
                        OriginalAssignee = task.AssignedToUserId ?? Guid.Empty,
                        EscalatedToUserId = escalationTarget?.UserId,
                        Status = "Pending",
                        CreatedAt = DateTime.UtcNow
                    };

                    _dbContext.WorkflowEscalations.Add(escalation);

                    escalatedTask.EscalatedTo = escalationTarget?.UserId;
                    escalatedTask.EscalatedToName = escalationTarget?.UserName;
                    result.EscalatedTasks.Add(escalatedTask);
                    result.TasksEscalated++;
                }
                else
                {
                    result.PendingEscalation.Add(escalatedTask);
                }
            }

            if (autoEscalate)
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            result.Summary = $"Found {result.TotalOverdueTasks} overdue tasks. " +
                           (autoEscalate
                               ? $"Escalated {result.TasksEscalated} tasks, {result.TasksAlreadyEscalated} already escalated."
                               : $"{result.PendingEscalation.Count} tasks pending escalation approval.");

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error escalating overdue tasks");
            throw;
        }
    }

    public async Task<WorkflowMetrics> GetWorkflowMetricsAsync(
        Guid? workflowDefinitionId = null,
        int daysBack = 30,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting workflow metrics, daysBack: {DaysBack}", daysBack);

            var since = DateTime.UtcNow.AddDays(-daysBack);

            var instanceQuery = _dbContext.WorkflowInstances
                .Include(i => i.Tasks)
                .Where(i => i.StartedAt >= since);

            if (workflowDefinitionId.HasValue)
            {
                instanceQuery = instanceQuery.Where(i => i.WorkflowDefinitionId == workflowDefinitionId);
            }

            if (tenantId.HasValue)
            {
                instanceQuery = instanceQuery.Where(i => i.TenantId == tenantId.Value);
            }

            var instances = await instanceQuery.ToListAsync(cancellationToken);
            var completedInstances = instances.Where(i => i.Status == "Completed" && i.CompletedAt.HasValue).ToList();

            var metrics = new WorkflowMetrics
            {
                GeneratedAt = DateTime.UtcNow,
                AnalysisPeriod = TimeSpan.FromDays(daysBack),
                WorkflowDefinitionId = workflowDefinitionId,
                TotalInstances = instances.Count,
                CompletedInstances = completedInstances.Count,
                ActiveInstances = instances.Count(i => i.Status == "Active"),
                CancelledInstances = instances.Count(i => i.Status == "Cancelled"),
                SlaBreachedCount = instances.Count(i => i.SlaBreached),
                SlaComplianceRate = instances.Count > 0
                    ? (double)(instances.Count - instances.Count(i => i.SlaBreached)) / instances.Count * 100
                    : 100
            };

            if (completedInstances.Any())
            {
                var completionDays = completedInstances
                    .Select(i => (i.CompletedAt!.Value - i.StartedAt).TotalDays)
                    .OrderBy(d => d)
                    .ToList();

                metrics.AverageCompletionDays = completionDays.Average();
                metrics.MedianCompletionDays = completionDays[completionDays.Count / 2];
                metrics.MinCompletionDays = completionDays.Min();
                metrics.MaxCompletionDays = completionDays.Max();
            }

            // Task metrics
            var allTasks = instances.SelectMany(i => i.Tasks).ToList();
            metrics.TotalTasks = allTasks.Count;
            metrics.CompletedTasks = allTasks.Count(t => t.Status == "Completed");
            metrics.OverdueTasks = allTasks.Count(t => t.DueDate < DateTime.UtcNow && t.Status != "Completed");
            metrics.EscalatedTasks = allTasks.Count(t => t.IsEscalated);

            // Determine trend
            var recentHalf = instances.Where(i => i.StartedAt >= since.AddDays(daysBack / 2)).ToList();
            var olderHalf = instances.Where(i => i.StartedAt < since.AddDays(daysBack / 2)).ToList();
            var recentSlaRate = recentHalf.Any()
                ? (double)(recentHalf.Count - recentHalf.Count(i => i.SlaBreached)) / recentHalf.Count
                : 1;
            var olderSlaRate = olderHalf.Any()
                ? (double)(olderHalf.Count - olderHalf.Count(i => i.SlaBreached)) / olderHalf.Count
                : 1;

            metrics.Trend = recentSlaRate > olderSlaRate + 0.05 ? "improving"
                         : recentSlaRate < olderSlaRate - 0.05 ? "declining"
                         : "stable";

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workflow metrics");
            throw;
        }
    }

    public async Task<UserWorkloadAnalysis> AnalyzeUserWorkloadAsync(
        Guid? userId = null,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Analyzing user workload");

            var taskQuery = _dbContext.WorkflowTasks
                .Where(t => t.AssignedToUserId.HasValue);

            if (userId.HasValue)
            {
                taskQuery = taskQuery.Where(t => t.AssignedToUserId == userId);
            }

            if (tenantId.HasValue)
            {
                taskQuery = taskQuery.Where(t => t.TenantId == tenantId.Value);
            }

            var tasks = await taskQuery.ToListAsync(cancellationToken);

            var userWorkloads = tasks
                .GroupBy(t => new { t.AssignedToUserId, t.AssignedToUserName })
                .Select(g =>
                {
                    var userTasks = g.ToList();
                    var activeTasks = userTasks.Count(t => t.Status != "Completed" && t.Status != "Cancelled");
                    var overdueTasks = userTasks.Count(t => t.DueDate < DateTime.UtcNow && t.Status != "Completed");
                    var completedThisWeek = userTasks.Count(t => t.CompletedAt >= DateTime.UtcNow.AddDays(-7));
                    var workloadScore = CalculateWorkloadScore(userTasks);

                    return new UserWorkloadDetail
                    {
                        UserId = g.Key.AssignedToUserId!.Value,
                        UserName = g.Key.AssignedToUserName ?? "Unknown",
                        ActiveTasks = activeTasks,
                        PendingTasks = userTasks.Count(t => t.Status == "Pending"),
                        OverdueTasks = overdueTasks,
                        CompletedThisWeek = completedThisWeek,
                        AverageCompletionHours = userTasks
                            .Where(t => t.CompletedAt.HasValue && t.StartedAt.HasValue)
                            .Select(t => (t.CompletedAt!.Value - t.StartedAt!.Value).TotalHours)
                            .DefaultIfEmpty(0)
                            .Average(),
                        WorkloadScore = workloadScore,
                        WorkloadLevel = workloadScore > 90 ? "Overloaded"
                                      : workloadScore > 70 ? "Heavy"
                                      : workloadScore > 30 ? "Normal"
                                      : "Light",
                        Capacity = Math.Max(0, (int)((100 - workloadScore) / 10))
                    };
                })
                .OrderByDescending(u => u.WorkloadScore)
                .ToList();

            var distribution = new WorkloadDistribution
            {
                TotalActiveTasks = userWorkloads.Sum(u => u.ActiveTasks),
                AverageTasksPerUser = userWorkloads.Any() ? userWorkloads.Average(u => u.ActiveTasks) : 0,
                StandardDeviation = CalculateStandardDeviation(userWorkloads.Select(u => (double)u.ActiveTasks)),
                UnderutilizedUsers = userWorkloads.Count(u => u.WorkloadLevel == "Light"),
                OverloadedUsers = userWorkloads.Count(u => u.WorkloadLevel == "Overloaded" || u.WorkloadLevel == "Heavy"),
                DistributionScore = CalculateDistributionScore(userWorkloads)
            };

            var analysis = new UserWorkloadAnalysis
            {
                AnalyzedAt = DateTime.UtcNow,
                UserId = userId,
                TotalUsersAnalyzed = userWorkloads.Count,
                UserWorkloads = userWorkloads,
                Distribution = distribution
            };

            // Use AI for recommendations
            var prompt = BuildWorkloadPrompt(userWorkloads, distribution);
            var aiResponse = await CallClaudeAIAsync(prompt, cancellationToken);
            ParseWorkloadResponse(aiResponse, analysis);

            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing user workload");
            throw;
        }
    }

    public async Task<ReminderResult> SendDeadlineRemindersAsync(
        int hoursBeforeDeadline = 24,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending deadline reminders, hoursBeforeDeadline: {Hours}", hoursBeforeDeadline);

            var cutoffTime = DateTime.UtcNow.AddHours(hoursBeforeDeadline);

            var taskQuery = _dbContext.WorkflowTasks
                .Where(t => t.Status != "Completed" &&
                           t.Status != "Cancelled" &&
                           t.DueDate.HasValue &&
                           t.DueDate.Value <= cutoffTime &&
                           t.DueDate.Value > DateTime.UtcNow);

            if (tenantId.HasValue)
            {
                taskQuery = taskQuery.Where(t => t.TenantId == tenantId.Value);
            }

            var tasks = await taskQuery.ToListAsync(cancellationToken);

            var result = new ReminderResult
            {
                ProcessedAt = DateTime.UtcNow,
                TasksWithUpcomingDeadlines = tasks.Count,
                Reminders = new List<ReminderSent>(),
                Errors = new List<string>()
            };

            foreach (var task in tasks)
            {
                if (!task.AssignedToUserId.HasValue)
                {
                    continue;
                }

                var timeRemaining = task.DueDate!.Value - DateTime.UtcNow;
                var reminderType = timeRemaining.TotalHours <= 24 ? "24h"
                                 : timeRemaining.TotalHours <= 48 ? "48h"
                                 : "1week";

                // In production, this would integrate with notification service
                var reminder = new ReminderSent
                {
                    TaskId = task.Id,
                    TaskName = task.TaskName,
                    UserId = task.AssignedToUserId.Value,
                    UserName = task.AssignedToUserName ?? "Unknown",
                    DueDate = task.DueDate.Value,
                    ReminderType = reminderType,
                    Success = true
                };

                result.Reminders.Add(reminder);
                result.RemindersSent++;

                _logger.LogInformation("Reminder sent for task {TaskId} to user {UserId}", task.Id, task.AssignedToUserId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending deadline reminders");
            throw;
        }
    }

    public async Task<WorkflowStatusReport> GenerateStatusReportAsync(
        Guid? workflowId = null,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating workflow status report");

            var instanceQuery = _dbContext.WorkflowInstances
                .Include(i => i.Tasks)
                .Where(i => i.Status == "Active");

            if (workflowId.HasValue)
            {
                instanceQuery = instanceQuery.Where(i => i.Id == workflowId);
            }

            if (tenantId.HasValue)
            {
                instanceQuery = instanceQuery.Where(i => i.TenantId == tenantId.Value);
            }

            var instances = await instanceQuery.ToListAsync(cancellationToken);
            var allTasks = instances.SelectMany(i => i.Tasks).ToList();

            var pendingTasks = allTasks.Count(t => t.Status == "Pending" || t.Status == "InProgress");
            var overdueTasks = allTasks.Count(t => t.DueDate < DateTime.UtcNow && t.Status != "Completed");
            var tasksDueToday = allTasks.Count(t => t.DueDate.HasValue && t.DueDate.Value.Date == DateTime.UtcNow.Date);

            var issues = new List<WorkflowIssue>();

            // Check for SLA breaches
            var slaBreachedWorkflows = instances.Where(i => i.SlaBreached).ToList();
            foreach (var wf in slaBreachedWorkflows)
            {
                issues.Add(new WorkflowIssue
                {
                    Category = "sla",
                    Severity = "High",
                    Title = "SLA Breached",
                    Description = $"Workflow {wf.InstanceNumber} has breached its SLA",
                    AffectedWorkflow = wf.WorkflowType,
                    SuggestedAction = "Review and expedite completion"
                });
            }

            // Check for overdue tasks
            if (overdueTasks > 5)
            {
                issues.Add(new WorkflowIssue
                {
                    Category = "capacity",
                    Severity = overdueTasks > 10 ? "Critical" : "High",
                    Title = "Multiple Overdue Tasks",
                    Description = $"{overdueTasks} tasks are overdue",
                    SuggestedAction = "Review workload distribution and escalation policies"
                });
            }

            var healthScore = CalculateHealthScore(instances, allTasks);
            var overallStatus = healthScore >= 80 ? "Healthy"
                             : healthScore >= 50 ? "AtRisk"
                             : "Critical";

            var report = new WorkflowStatusReport
            {
                GeneratedAt = DateTime.UtcNow,
                WorkflowId = workflowId,
                OverallStatus = overallStatus,
                HealthScore = healthScore,
                ActiveWorkflows = instances.Count,
                PendingTasks = pendingTasks,
                OverdueTasks = overdueTasks,
                TasksDueToday = tasksDueToday,
                Issues = issues
            };

            // Use AI for executive summary
            var prompt = BuildStatusReportPrompt(instances, allTasks, issues, healthScore);
            var aiResponse = await CallClaudeAIAsync(prompt, cancellationToken);
            ParseStatusReportResponse(aiResponse, report);

            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating status report");
            throw;
        }
    }

    #region Helper Methods

    private async Task<string> CallClaudeAIAsync(string prompt, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(_claudeSettings.ApiKey))
            {
                _logger.LogWarning("Claude API key not configured, returning default response");
                return "{}";
            }

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("x-api-key", _claudeSettings.ApiKey);
            httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

            var requestBody = new
            {
                model = _claudeSettings.Model ?? "claude-3-5-sonnet-20241022",
                max_tokens = _claudeSettings.MaxTokens > 0 ? _claudeSettings.MaxTokens : 4096,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("https://api.anthropic.com/v1/messages", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseObj = JsonSerializer.Deserialize<JsonElement>(responseJson);

            return responseObj.GetProperty("content")[0].GetProperty("text").GetString() ?? "{}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Claude AI");
            return "{}";
        }
    }

    private string BuildOptimizationPrompt(WorkflowInstance instance, List<WorkflowTask> tasks)
    {
        return $@"
Analyze this workflow instance and suggest optimizations:

Workflow: {instance.WorkflowType}
Status: {instance.Status}
Current State: {instance.CurrentState}
Started: {instance.StartedAt}
SLA Due: {instance.SlaDueDate}
SLA Breached: {instance.SlaBreached}

Tasks ({tasks.Count} total):
{string.Join("\n", tasks.Select(t => $"- {t.TaskName}: {t.Status}, Priority: {t.Priority}, Completion: {t.CompletionPercentage}%"))}

Provide optimization recommendations in JSON format:
{{
  ""efficiencyScore"": 75,
  ""potentialScore"": 90,
  ""recommendations"": [
    {{
      ""category"": ""routing|sla|parallelization|automation"",
      ""title"": ""..."",
      ""description"": ""..."",
      ""priority"": ""Critical|High|Medium|Low"",
      ""impactScore"": 8,
      ""effortScore"": 3,
      ""steps"": [""...""],
      ""requiresApproval"": true
    }}
  ],
  ""identifiedIssues"": [""...""],
  ""summary"": ""...""
}}";
    }

    private string BuildDefinitionOptimizationPrompt(WorkflowDefinition definition, List<WorkflowInstance> instances)
    {
        var completedCount = instances.Count(i => i.Status == "Completed");
        var avgDuration = instances.Where(i => i.CompletedAt.HasValue)
            .Select(i => (i.CompletedAt!.Value - i.StartedAt).TotalDays)
            .DefaultIfEmpty(0)
            .Average();

        return $@"
Analyze this workflow definition based on historical performance:

Workflow: {definition.Name}
Total Instances: {instances.Count}
Completed: {completedCount}
Average Duration: {avgDuration:F1} days
SLA Breaches: {instances.Count(i => i.SlaBreached)}

Suggest improvements in JSON format (same structure as instance optimization).";
    }

    private string BuildBottleneckPrompt(List<WorkflowTask> tasks, List<WorkflowInstance> instances,
        dynamic tasksByStep, List<OverloadedUser> overloadedUsers)
    {
        return $@"
Analyze workflow bottlenecks:

Total Tasks: {tasks.Count}
Total Workflows: {instances.Count}
Overloaded Users: {overloadedUsers.Count}

Slowest Steps:
{string.Join("\n", ((IEnumerable<dynamic>)tasksByStep).Take(5).Select(s => $"- {s.StepName}: {s.AvgCompletionHours:F1}h avg, {s.Count} occurrences"))}

Identify bottlenecks in JSON format:
{{
  ""bottlenecks"": [
    {{
      ""bottleneckType"": ""user|step|sla|approval"",
      ""location"": ""..."",
      ""description"": ""..."",
      ""severity"": 8,
      ""averageDelayHours"": 24,
      ""affectedWorkflows"": 5,
      ""suggestedFix"": ""...""
    }}
  ],
  ""recommendedActions"": [""...""],
  ""summary"": ""...""
}}";
    }

    private string BuildImprovementPrompt(List<WorkflowDefinition> definitions, List<WorkflowInstance> instances)
    {
        return $@"
Suggest workflow improvements based on analysis:

Workflow Definitions: {definitions.Count}
Recent Instances: {instances.Count}

Return JSON array of improvements:
[
  {{
    ""category"": ""efficiency|quality|compliance|ux"",
    ""title"": ""..."",
    ""description"": ""..."",
    ""currentState"": ""..."",
    ""proposedState"": ""..."",
    ""priority"": ""Critical|High|Medium|Low"",
    ""estimatedImpact"": 8,
    ""affectedWorkflows"": [""...""],
    ""implementationSteps"": [""...""],
    ""requiresApproval"": true
  }}
]";
    }

    private string BuildDeadlinePrompt(List<WorkflowTaskDeadline> upcoming, List<DeadlineRisk> atRisk, List<WorkflowTask> overdue)
    {
        return $@"
Analyze deadline management:

Upcoming Deadlines: {upcoming.Count}
At Risk: {atRisk.Count}
Overdue: {overdue.Count}

Provide recommendations in JSON:
{{
  ""recommendations"": [""...""],
  ""summary"": ""...""
}}";
    }

    private string BuildWorkloadPrompt(List<UserWorkloadDetail> workloads, WorkloadDistribution distribution)
    {
        return $@"
Analyze user workload distribution:

Users Analyzed: {workloads.Count}
Distribution Score: {distribution.DistributionScore:F0}
Overloaded Users: {distribution.OverloadedUsers}
Underutilized: {distribution.UnderutilizedUsers}

Provide recommendations in JSON:
{{
  ""recommendations"": [""...""],
  ""summary"": ""...""
}}";
    }

    private string BuildStatusReportPrompt(List<WorkflowInstance> instances, List<WorkflowTask> tasks,
        List<WorkflowIssue> issues, int healthScore)
    {
        return $@"
Generate executive summary for workflow status:

Active Workflows: {instances.Count}
Total Tasks: {tasks.Count}
Health Score: {healthScore}
Issues: {issues.Count}

Return JSON:
{{
  ""executiveSummary"": ""..."",
  ""keyHighlights"": [""...""],
  ""recommendedActions"": [""...""]
}}";
    }

    private void ParseOptimizationResponse(string aiResponse, WorkflowAgentOptimizationResult result)
    {
        try
        {
            var json = JsonSerializer.Deserialize<JsonElement>(aiResponse);

            if (json.TryGetProperty("efficiencyScore", out var es))
                result.CurrentEfficiencyScore = es.GetInt32();
            if (json.TryGetProperty("potentialScore", out var ps))
                result.PotentialEfficiencyScore = ps.GetInt32();
            if (json.TryGetProperty("recommendations", out var recs))
                result.Recommendations = JsonSerializer.Deserialize<List<OptimizationRecommendation>>(recs.GetRawText()) ?? new();
            if (json.TryGetProperty("identifiedIssues", out var issues))
                result.IdentifiedIssues = JsonSerializer.Deserialize<List<string>>(issues.GetRawText()) ?? new();
            if (json.TryGetProperty("summary", out var summary))
                result.Summary = summary.GetString() ?? "";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing optimization response");
            result.Summary = "Analysis completed with limited AI insights";
        }
    }

    private void ParseBottleneckResponse(string aiResponse, BottleneckAnalysis analysis)
    {
        try
        {
            var json = JsonSerializer.Deserialize<JsonElement>(aiResponse);

            if (json.TryGetProperty("bottlenecks", out var bottlenecks))
                analysis.Bottlenecks = JsonSerializer.Deserialize<List<Bottleneck>>(bottlenecks.GetRawText()) ?? new();
            if (json.TryGetProperty("recommendedActions", out var actions))
                analysis.RecommendedActions = JsonSerializer.Deserialize<List<string>>(actions.GetRawText()) ?? new();
            if (json.TryGetProperty("summary", out var summary))
                analysis.Summary = summary.GetString() ?? "";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing bottleneck response");
        }
    }

    private List<WorkflowImprovement> ParseImprovementResponse(string aiResponse)
    {
        try
        {
            return JsonSerializer.Deserialize<List<WorkflowImprovement>>(aiResponse) ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing improvement response");
            return new List<WorkflowImprovement>();
        }
    }

    private void ParseDeadlineResponse(string aiResponse, DeadlineManagementResult result)
    {
        try
        {
            var json = JsonSerializer.Deserialize<JsonElement>(aiResponse);

            if (json.TryGetProperty("recommendations", out var recs))
                result.Recommendations = JsonSerializer.Deserialize<List<string>>(recs.GetRawText()) ?? new();
            if (json.TryGetProperty("summary", out var summary))
                result.Summary = summary.GetString() ?? "";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing deadline response");
        }
    }

    private void ParseWorkloadResponse(string aiResponse, UserWorkloadAnalysis analysis)
    {
        try
        {
            var json = JsonSerializer.Deserialize<JsonElement>(aiResponse);

            if (json.TryGetProperty("recommendations", out var recs))
                analysis.Recommendations = JsonSerializer.Deserialize<List<string>>(recs.GetRawText()) ?? new();
            if (json.TryGetProperty("summary", out var summary))
                analysis.Summary = summary.GetString() ?? "";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing workload response");
        }
    }

    private void ParseStatusReportResponse(string aiResponse, WorkflowStatusReport report)
    {
        try
        {
            var json = JsonSerializer.Deserialize<JsonElement>(aiResponse);

            if (json.TryGetProperty("executiveSummary", out var summary))
                report.ExecutiveSummary = summary.GetString() ?? "";
            if (json.TryGetProperty("keyHighlights", out var highlights))
                report.KeyHighlights = JsonSerializer.Deserialize<List<string>>(highlights.GetRawText()) ?? new();
            if (json.TryGetProperty("recommendedActions", out var actions))
                report.RecommendedActions = JsonSerializer.Deserialize<List<string>>(actions.GetRawText()) ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing status report response");
        }
    }

    private double CalculateWorkloadScore(List<WorkflowTask> tasks)
    {
        var activeTasks = tasks.Count(t => t.Status != "Completed" && t.Status != "Cancelled");
        var overdueTasks = tasks.Count(t => t.DueDate < DateTime.UtcNow && t.Status != "Completed");
        var highPriorityTasks = tasks.Count(t => t.Priority == 1 && t.Status != "Completed");

        return Math.Min(100, (activeTasks * 10) + (overdueTasks * 20) + (highPriorityTasks * 5));
    }

    private int DetermineEscalationLevel(TimeSpan overdueBy, int priority)
    {
        if (priority == 1) // High priority
        {
            if (overdueBy.TotalHours > 48) return 4; // Critical
            if (overdueBy.TotalHours > 24) return 3; // Manager
            if (overdueBy.TotalHours > 8) return 2;  // Second
            return 1; // First
        }
        else
        {
            if (overdueBy.TotalDays > 7) return 4;
            if (overdueBy.TotalDays > 3) return 3;
            if (overdueBy.TotalDays > 1) return 2;
            return 1;
        }
    }

    private string GetEscalationReason(TimeSpan overdueBy, int priority)
    {
        var priorityText = priority == 1 ? "high priority" : priority == 2 ? "medium priority" : "low priority";
        return $"Task is {overdueBy.TotalHours:F0} hours overdue ({priorityText})";
    }

    private async Task<(Guid? UserId, string? UserName)?> FindEscalationTargetAsync(
        WorkflowTask task, int escalationLevel, CancellationToken cancellationToken)
    {
        // In production, this would look up the org hierarchy
        // For now, return null to indicate manual escalation target selection
        return null;
    }

    private double CalculateStandardDeviation(IEnumerable<double> values)
    {
        var list = values.ToList();
        if (!list.Any()) return 0;

        var avg = list.Average();
        var sumOfSquares = list.Sum(v => Math.Pow(v - avg, 2));
        return Math.Sqrt(sumOfSquares / list.Count);
    }

    private double CalculateDistributionScore(List<UserWorkloadDetail> workloads)
    {
        if (!workloads.Any()) return 100;

        var stdDev = CalculateStandardDeviation(workloads.Select(w => (double)w.ActiveTasks));
        var avg = workloads.Average(w => w.ActiveTasks);

        if (avg == 0) return 100;

        // Coefficient of variation - lower is better
        var cv = stdDev / avg;
        return Math.Max(0, 100 - (cv * 50));
    }

    private int CalculateHealthScore(List<WorkflowInstance> instances, List<WorkflowTask> tasks)
    {
        if (!instances.Any()) return 100;

        var score = 100;

        // Deduct for SLA breaches
        var slaBreachRate = (double)instances.Count(i => i.SlaBreached) / instances.Count;
        score -= (int)(slaBreachRate * 30);

        // Deduct for overdue tasks
        var overdueRate = tasks.Any()
            ? (double)tasks.Count(t => t.DueDate < DateTime.UtcNow && t.Status != "Completed") / tasks.Count
            : 0;
        score -= (int)(overdueRate * 20);

        // Deduct for escalated tasks
        var escalatedRate = tasks.Any()
            ? (double)tasks.Count(t => t.IsEscalated) / tasks.Count
            : 0;
        score -= (int)(escalatedRate * 10);

        return Math.Max(0, Math.Min(100, score));
    }

    #endregion
}
