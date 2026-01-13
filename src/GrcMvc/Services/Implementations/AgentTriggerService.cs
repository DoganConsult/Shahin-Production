using System.Text.Json;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Agent Event Trigger Service - Manages event-driven agent activation.
/// Implements the fullplan specification for automated agent triggering.
/// </summary>
public class AgentTriggerService : IAgentTriggerService
{
    private readonly GrcDbContext _dbContext;
    private readonly ILogger<AgentTriggerService> _logger;
    private readonly IClaudeAgentService _agentService;

    public AgentTriggerService(
        GrcDbContext dbContext,
        ILogger<AgentTriggerService> logger,
        IClaudeAgentService agentService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _agentService = agentService;
    }

    public async Task<TriggerProcessingResult> ProcessEventAsync(
        string eventType,
        Guid? tenantId,
        Guid? sourceEntityId,
        string? sourceEntityType,
        Dictionary<string, object>? eventPayload = null,
        CancellationToken cancellationToken = default)
    {
        var result = new TriggerProcessingResult { Success = true };
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            // Get applicable triggers
            var triggers = await GetTriggersForEventAsync(eventType, tenantId, cancellationToken);
            result.TriggersEvaluated = triggers.Count;

            _logger.LogInformation("Processing event {EventType} for tenant {TenantId}: {TriggerCount} triggers found",
                eventType, tenantId, triggers.Count);

            foreach (var trigger in triggers.OrderBy(t => t.Priority))
            {
                var execution = new AgentTriggerExecution
                {
                    TenantId = tenantId,
                    TriggerId = trigger.Id,
                    EventType = eventType,
                    SourceEntityType = sourceEntityType,
                    SourceEntityId = sourceEntityId,
                    EventPayloadJson = eventPayload != null ? JsonSerializer.Serialize(eventPayload) : null,
                    ExecutedAt = DateTime.UtcNow
                };

                var triggerStopwatch = System.Diagnostics.Stopwatch.StartNew();

                try
                {
                    // Check rate limiting
                    if (!await CheckRateLimitAsync(trigger, tenantId, sourceEntityId, cancellationToken))
                    {
                        execution.Status = "Skipped";
                        execution.ErrorMessage = "Rate limit exceeded";
                        _logger.LogDebug("Trigger {TriggerCode} skipped due to rate limit", trigger.TriggerCode);
                        continue;
                    }

                    // Evaluate condition if present
                    var conditionMet = true;
                    if (!string.IsNullOrEmpty(trigger.ConditionJson) && trigger.ConditionJson != "{}")
                    {
                        conditionMet = EvaluateCondition(trigger.ConditionJson, eventPayload ?? new());
                    }

                    if (!conditionMet)
                    {
                        execution.Status = "ConditionNotMet";
                        _logger.LogDebug("Trigger {TriggerCode} condition not met", trigger.TriggerCode);
                        continue;
                    }

                    // Apply delay if configured
                    if (trigger.DelaySeconds > 0)
                    {
                        _logger.LogDebug("Trigger {TriggerCode} delayed by {Delay} seconds",
                            trigger.TriggerCode, trigger.DelaySeconds);
                        await Task.Delay(TimeSpan.FromSeconds(trigger.DelaySeconds), cancellationToken);
                    }

                    // Invoke the agent
                    var agentResult = await InvokeAgentAsync(
                        trigger.AgentCode,
                        trigger.AgentAction,
                        tenantId,
                        trigger.ParametersJson,
                        eventPayload,
                        cancellationToken);

                    execution.AgentInvoked = true;
                    execution.Status = agentResult.Success ? "Completed" : "Failed";
                    execution.ErrorMessage = agentResult.Success ? null : agentResult.Message;
                    result.TriggersActivated++;

                    _logger.LogInformation("Agent {AgentCode} invoked for trigger {TriggerCode}: {Status}",
                        trigger.AgentCode, trigger.TriggerCode, execution.Status);
                }
                catch (Exception ex)
                {
                    execution.Status = "Failed";
                    execution.ErrorMessage = ex.Message;
                    _logger.LogError(ex, "Error processing trigger {TriggerCode}", trigger.TriggerCode);
                }

                triggerStopwatch.Stop();
                execution.DurationMs = (int)triggerStopwatch.ElapsedMilliseconds;
                result.Executions.Add(execution);

                _dbContext.Set<AgentTriggerExecution>().Add(execution);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            stopwatch.Stop();
            _logger.LogInformation(
                "Event {EventType} processed: {Evaluated} triggers evaluated, {Activated} activated in {Duration}ms",
                eventType, result.TriggersEvaluated, result.TriggersActivated, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = ex.Message;
            _logger.LogError(ex, "Error processing event {EventType}", eventType);
        }

        return result;
    }

    public async Task<List<AgentEventTrigger>> GetTriggersForEventAsync(
        string eventType,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<AgentEventTrigger>()
            .Where(t => t.IsActive && t.EventType == eventType);

        // Include platform triggers and tenant-specific triggers
        if (tenantId.HasValue)
        {
            query = query.Where(t => t.TenantId == null || t.TenantId == tenantId);
        }
        else
        {
            query = query.Where(t => t.TenantId == null);
        }

        return await query
            .OrderBy(t => t.Priority)
            .ToListAsync(cancellationToken);
    }

    public async Task<AgentEventTrigger> RegisterTriggerAsync(
        AgentEventTrigger trigger,
        CancellationToken cancellationToken = default)
    {
        if (trigger.Id == Guid.Empty)
        {
            trigger.Id = Guid.NewGuid();
            trigger.CreatedDate = DateTime.UtcNow;
            _dbContext.Set<AgentEventTrigger>().Add(trigger);
        }
        else
        {
            trigger.ModifiedDate = DateTime.UtcNow;
            _dbContext.Set<AgentEventTrigger>().Update(trigger);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Registered trigger {TriggerCode} for event {EventType}",
            trigger.TriggerCode, trigger.EventType);

        return trigger;
    }

    public async Task<bool> SetTriggerActiveAsync(
        Guid triggerId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var trigger = await _dbContext.Set<AgentEventTrigger>()
            .FirstOrDefaultAsync(t => t.Id == triggerId, cancellationToken);

        if (trigger == null)
            return false;

        trigger.IsActive = isActive;
        trigger.ModifiedDate = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Trigger {TriggerCode} set to active={IsActive}",
            trigger.TriggerCode, isActive);

        return true;
    }

    public async Task<List<AgentTriggerExecution>> GetExecutionHistoryAsync(
        Guid? tenantId,
        Guid? triggerId = null,
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<AgentTriggerExecution>().AsQueryable();

        if (tenantId.HasValue)
        {
            query = query.Where(e => e.TenantId == tenantId);
        }

        if (triggerId.HasValue)
        {
            query = query.Where(e => e.TriggerId == triggerId);
        }

        return await query
            .OrderByDescending(e => e.ExecutedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    #region Private Methods

    private async Task<bool> CheckRateLimitAsync(
        AgentEventTrigger trigger,
        Guid? tenantId,
        Guid? sourceEntityId,
        CancellationToken cancellationToken)
    {
        // Check daily execution limit
        if (trigger.MaxDailyExecutions.HasValue)
        {
            var today = DateTime.UtcNow.Date;
            var executionsToday = await _dbContext.Set<AgentTriggerExecution>()
                .CountAsync(e => e.TriggerId == trigger.Id &&
                                e.TenantId == tenantId &&
                                e.SourceEntityId == sourceEntityId &&
                                e.ExecutedAt >= today,
                    cancellationToken);

            if (executionsToday >= trigger.MaxDailyExecutions.Value)
            {
                return false;
            }
        }

        // Check cooldown period
        if (trigger.CooldownSeconds.HasValue)
        {
            var cooldownStart = DateTime.UtcNow.AddSeconds(-trigger.CooldownSeconds.Value);
            var recentExecution = await _dbContext.Set<AgentTriggerExecution>()
                .AnyAsync(e => e.TriggerId == trigger.Id &&
                              e.TenantId == tenantId &&
                              e.SourceEntityId == sourceEntityId &&
                              e.ExecutedAt >= cooldownStart,
                    cancellationToken);

            if (recentExecution)
            {
                return false;
            }
        }

        return true;
    }

    private bool EvaluateCondition(string conditionJson, Dictionary<string, object> context)
    {
        try
        {
            var condition = JsonSerializer.Deserialize<Dictionary<string, object>>(conditionJson);
            if (condition == null)
                return true;

            // Simple AND evaluation
            foreach (var kvp in condition)
            {
                if (!context.TryGetValue(kvp.Key, out var contextValue))
                    return false;

                var targetValue = kvp.Value?.ToString()?.ToLower();
                var actualValue = contextValue?.ToString()?.ToLower();

                if (targetValue != actualValue)
                    return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error evaluating trigger condition");
            return false;
        }
    }

    private async Task<AgentInvocationResult> InvokeAgentAsync(
        string agentCode,
        string action,
        Guid? tenantId,
        string? parametersJson,
        Dictionary<string, object>? eventPayload,
        CancellationToken cancellationToken)
    {
        try
        {
            var parameters = !string.IsNullOrEmpty(parametersJson)
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(parametersJson)
                : new Dictionary<string, object>();

            // Merge event payload into parameters
            if (eventPayload != null)
            {
                foreach (var kvp in eventPayload)
                {
                    if (!parameters!.ContainsKey(kvp.Key))
                    {
                        parameters[kvp.Key] = kvp.Value;
                    }
                }
            }

            // Route to appropriate agent method based on code and action
            var result = agentCode switch
            {
                "COMPLIANCE_AGENT" => await InvokeComplianceAgentAsync(action, tenantId, parameters!, cancellationToken),
                "RISK_AGENT" => await InvokeRiskAgentAsync(action, tenantId, parameters!, cancellationToken),
                "EVIDENCE_AGENT" => await InvokeEvidenceAgentAsync(action, tenantId, parameters!, cancellationToken),
                "WORKFLOW_AGENT" => await InvokeWorkflowAgentAsync(action, tenantId, parameters!, cancellationToken),
                "ANALYTICS_AGENT" => await InvokeAnalyticsAgentAsync(action, tenantId, parameters!, cancellationToken),
                _ => new AgentInvocationResult { Success = false, Message = $"Unknown agent code: {agentCode}" }
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invoking agent {AgentCode} with action {Action}", agentCode, action);
            return new AgentInvocationResult { Success = false, Message = ex.Message };
        }
    }

    private async Task<AgentInvocationResult> InvokeComplianceAgentAsync(
        string action, Guid? tenantId, Dictionary<string, object> parameters, CancellationToken cancellationToken)
    {
        if (action == "analyze" && parameters.TryGetValue("framework_code", out var frameworkCode))
        {
            var result = await _agentService.AnalyzeComplianceAsync(
                frameworkCode.ToString()!,
                tenantId: tenantId,
                cancellationToken: cancellationToken);
            return new AgentInvocationResult { Success = result.Success, Message = result.Summary };
        }

        return new AgentInvocationResult { Success = true, Message = "Compliance agent invoked" };
    }

    private async Task<AgentInvocationResult> InvokeRiskAgentAsync(
        string action, Guid? tenantId, Dictionary<string, object> parameters, CancellationToken cancellationToken)
    {
        if (action == "assess")
        {
            var result = await _agentService.AnalyzeRiskAsync(
                tenantId: tenantId,
                cancellationToken: cancellationToken);
            return new AgentInvocationResult { Success = result.Success, Message = result.Summary };
        }

        return new AgentInvocationResult { Success = true, Message = "Risk agent invoked" };
    }

    private async Task<AgentInvocationResult> InvokeEvidenceAgentAsync(
        string action, Guid? tenantId, Dictionary<string, object> parameters, CancellationToken cancellationToken)
    {
        if (action == "analyze" && parameters.TryGetValue("evidence_id", out var evidenceId))
        {
            if (Guid.TryParse(evidenceId.ToString(), out var id))
            {
                var result = await _agentService.AnalyzeEvidenceAsync(id, cancellationToken: cancellationToken);
                return new AgentInvocationResult { Success = result.Success, Message = result.Summary };
            }
        }

        return new AgentInvocationResult { Success = true, Message = "Evidence agent invoked" };
    }

    private async Task<AgentInvocationResult> InvokeWorkflowAgentAsync(
        string action, Guid? tenantId, Dictionary<string, object> parameters, CancellationToken cancellationToken)
    {
        if (action == "optimize")
        {
            var result = await _agentService.OptimizeWorkflowAsync(
                tenantId: tenantId,
                cancellationToken: cancellationToken);
            return new AgentInvocationResult { Success = result.Success, Message = result.Summary };
        }

        return new AgentInvocationResult { Success = true, Message = "Workflow agent invoked" };
    }

    private async Task<AgentInvocationResult> InvokeAnalyticsAgentAsync(
        string action, Guid? tenantId, Dictionary<string, object> parameters, CancellationToken cancellationToken)
    {
        if (action == "insights")
        {
            var result = await _agentService.GenerateInsightsAsync(
                tenantId: tenantId,
                cancellationToken: cancellationToken);
            return new AgentInvocationResult { Success = result.Success };
        }

        return new AgentInvocationResult { Success = true, Message = "Analytics agent invoked" };
    }

    #endregion

    private class AgentInvocationResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }
}
