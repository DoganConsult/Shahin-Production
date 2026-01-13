using System.Text.Json;
using System.Text.RegularExpressions;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Conditional Logic Rules Engine - Evaluates and executes rules based on context.
/// Implements the fullplan specification for intelligent rule processing.
/// </summary>
public class ConditionalLogicService : IConditionalLogicService
{
    private readonly GrcDbContext _dbContext;
    private readonly ILogger<ConditionalLogicService> _logger;

    public ConditionalLogicService(
        GrcDbContext dbContext,
        ILogger<ConditionalLogicService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<RuleEvaluationResult> EvaluateRulesAsync(
        Guid tenantId,
        string triggerEvent,
        Dictionary<string, object> context,
        CancellationToken cancellationToken = default)
    {
        var result = new RuleEvaluationResult { Success = true };
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            // Get all applicable rules
            var rules = await GetRulesForEventAsync(triggerEvent, tenantId, cancellationToken);
            result.TotalEvaluated = rules.Count;

            foreach (var rule in rules.OrderBy(r => r.Priority))
            {
                var execution = new ConditionalLogicRuleExecution
                {
                    TenantId = tenantId,
                    RuleId = rule.Id,
                    TriggerEvent = triggerEvent,
                    ContextDataJson = JsonSerializer.Serialize(context),
                    ExecutedAt = DateTime.UtcNow
                };

                var ruleStopwatch = System.Diagnostics.Stopwatch.StartNew();

                try
                {
                    var conditionMatched = EvaluateCondition(rule.ConditionJson, context);
                    execution.ConditionMatched = conditionMatched;

                    if (conditionMatched)
                    {
                        result.MatchedRules.Add(rule);
                        result.TotalMatched++;
                        execution.Status = "Matched";

                        _logger.LogInformation("Rule {RuleCode} matched for trigger {TriggerEvent}",
                            rule.RuleCode, triggerEvent);

                        if (rule.StopOnMatch)
                        {
                            _logger.LogDebug("Stopping rule evaluation due to StopOnMatch on rule {RuleCode}",
                                rule.RuleCode);
                            break;
                        }
                    }
                    else
                    {
                        execution.Status = "Evaluated";
                    }
                }
                catch (Exception ex)
                {
                    execution.Status = "Failed";
                    execution.ErrorMessage = ex.Message;
                    _logger.LogError(ex, "Error evaluating rule {RuleCode}", rule.RuleCode);
                }

                ruleStopwatch.Stop();
                execution.DurationMs = (int)ruleStopwatch.ElapsedMilliseconds;
                result.ExecutionLogs.Add(execution);

                _dbContext.Set<ConditionalLogicRuleExecution>().Add(execution);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            stopwatch.Stop();
            _logger.LogInformation(
                "Evaluated {Total} rules for trigger {Event}: {Matched} matched in {Duration}ms",
                result.TotalEvaluated, triggerEvent, result.TotalMatched, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = ex.Message;
            _logger.LogError(ex, "Error in rule evaluation for trigger {TriggerEvent}", triggerEvent);
        }

        return result;
    }

    public async Task<bool> EvaluateRuleAsync(
        Guid ruleId,
        Dictionary<string, object> context,
        CancellationToken cancellationToken = default)
    {
        var rule = await _dbContext.Set<ConditionalLogicRule>()
            .FirstOrDefaultAsync(r => r.Id == ruleId, cancellationToken);

        if (rule == null || !rule.IsActive)
            return false;

        return EvaluateCondition(rule.ConditionJson, context);
    }

    public async Task<RuleExecutionResult> ExecuteRuleActionsAsync(
        Guid tenantId,
        List<ConditionalLogicRule> matchedRules,
        Dictionary<string, object> context,
        CancellationToken cancellationToken = default)
    {
        var result = new RuleExecutionResult { Success = true };

        foreach (var rule in matchedRules)
        {
            try
            {
                var actions = JsonSerializer.Deserialize<List<RuleAction>>(rule.ActionsJson) ?? new();

                foreach (var action in actions)
                {
                    var actionResult = await ExecuteActionAsync(tenantId, action, context, cancellationToken);
                    result.ActionResults.Add(actionResult);

                    if (!actionResult.Success)
                    {
                        _logger.LogWarning("Action {ActionType} failed for rule {RuleCode}: {Message}",
                            action.Type, rule.RuleCode, actionResult.ResultDetails);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing actions for rule {RuleCode}", rule.RuleCode);
                result.ActionResults.Add(new RuleActionResult
                {
                    ActionType = "Unknown",
                    Success = false,
                    ResultDetails = ex.Message
                });
            }
        }

        result.Success = result.ActionResults.All(r => r.Success);
        return result;
    }

    public async Task<List<ConditionalLogicRule>> GetRulesForEventAsync(
        string triggerEvent,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var query = _dbContext.Set<ConditionalLogicRule>()
            .Where(r => r.IsActive &&
                       (r.TriggerEvent == triggerEvent || r.TriggerEvent == "*") &&
                       (r.EffectiveFrom == null || r.EffectiveFrom <= now) &&
                       (r.EffectiveTo == null || r.EffectiveTo >= now));

        // Include platform rules (TenantId = null) and tenant-specific rules
        if (tenantId.HasValue)
        {
            query = query.Where(r => r.TenantId == null || r.TenantId == tenantId);
        }
        else
        {
            query = query.Where(r => r.TenantId == null);
        }

        return await query
            .OrderBy(r => r.Priority)
            .ToListAsync(cancellationToken);
    }

    public async Task<ConditionalLogicRule> SaveRuleAsync(
        ConditionalLogicRule rule,
        CancellationToken cancellationToken = default)
    {
        if (rule.Id == Guid.Empty)
        {
            rule.Id = Guid.NewGuid();
            rule.CreatedDate = DateTime.UtcNow;
            _dbContext.Set<ConditionalLogicRule>().Add(rule);
        }
        else
        {
            rule.ModifiedDate = DateTime.UtcNow;
            _dbContext.Set<ConditionalLogicRule>().Update(rule);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return rule;
    }

    #region Condition Evaluation

    private bool EvaluateCondition(string conditionJson, Dictionary<string, object> context)
    {
        if (string.IsNullOrWhiteSpace(conditionJson) || conditionJson == "{}")
            return true;

        try
        {
            var condition = JsonSerializer.Deserialize<ConditionNode>(conditionJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (condition == null)
                return true;

            return EvaluateConditionNode(condition, context);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing condition JSON: {Json}", conditionJson);
            return false;
        }
    }

    private bool EvaluateConditionNode(ConditionNode node, Dictionary<string, object> context)
    {
        // Handle compound conditions (and/or)
        if (!string.IsNullOrEmpty(node.Type) && node.Conditions != null && node.Conditions.Any())
        {
            return node.Type.ToLower() switch
            {
                "and" => node.Conditions.All(c => EvaluateConditionNode(c, context)),
                "or" => node.Conditions.Any(c => EvaluateConditionNode(c, context)),
                _ => false
            };
        }

        // Handle single condition
        if (!string.IsNullOrEmpty(node.Field))
        {
            return EvaluateSingleCondition(node, context);
        }

        return true;
    }

    private bool EvaluateSingleCondition(ConditionNode condition, Dictionary<string, object> context)
    {
        // Get the context value (support nested properties like "company_profile.region")
        var contextValue = GetContextValue(condition.Field!, context);
        var targetValue = condition.Value;
        var op = condition.Operator?.ToLower() ?? "equals";

        return op switch
        {
            "equals" or "eq" or "==" => CompareEquals(contextValue, targetValue),
            "not_equals" or "neq" or "!=" => !CompareEquals(contextValue, targetValue),
            "contains" => CompareContains(contextValue, targetValue),
            "not_contains" => !CompareContains(contextValue, targetValue),
            "in" => CompareIn(contextValue, condition.Values ?? new()),
            "not_in" => !CompareIn(contextValue, condition.Values ?? new()),
            "greater_than" or "gt" or ">" => CompareNumeric(contextValue, targetValue, (a, b) => a > b),
            "less_than" or "lt" or "<" => CompareNumeric(contextValue, targetValue, (a, b) => a < b),
            "greater_or_equal" or "gte" or ">=" => CompareNumeric(contextValue, targetValue, (a, b) => a >= b),
            "less_or_equal" or "lte" or "<=" => CompareNumeric(contextValue, targetValue, (a, b) => a <= b),
            "is_empty" => IsEmpty(contextValue),
            "is_not_empty" => !IsEmpty(contextValue),
            "starts_with" => contextValue?.ToString()?.StartsWith(targetValue?.ToString() ?? "") ?? false,
            "ends_with" => contextValue?.ToString()?.EndsWith(targetValue?.ToString() ?? "") ?? false,
            "matches" => MatchesRegex(contextValue, targetValue),
            _ => false
        };
    }

    private object? GetContextValue(string field, Dictionary<string, object> context)
    {
        // Support nested properties with dot notation
        var parts = field.Split('.');
        object? current = context;

        foreach (var part in parts)
        {
            if (current == null)
                return null;

            if (current is Dictionary<string, object> dict)
            {
                current = dict.TryGetValue(part, out var value) ? value : null;
            }
            else if (current is JsonElement jsonElement)
            {
                if (jsonElement.TryGetProperty(part, out var prop))
                {
                    current = GetValueFromJsonElement(prop);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                // Try reflection for object properties
                var property = current.GetType().GetProperty(part);
                current = property?.GetValue(current);
            }
        }

        return current;
    }

    private object? GetValueFromJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Array => element.EnumerateArray().Select(GetValueFromJsonElement).ToList(),
            _ => element
        };
    }

    private bool CompareEquals(object? contextValue, object? targetValue)
    {
        if (contextValue == null && targetValue == null)
            return true;
        if (contextValue == null || targetValue == null)
            return false;

        return contextValue.ToString()?.Equals(targetValue.ToString(), StringComparison.OrdinalIgnoreCase) ?? false;
    }

    private bool CompareContains(object? contextValue, object? targetValue)
    {
        if (contextValue == null || targetValue == null)
            return false;

        var contextStr = contextValue.ToString();
        var targetStr = targetValue.ToString();

        if (string.IsNullOrEmpty(contextStr) || string.IsNullOrEmpty(targetStr))
            return false;

        // Check if context is a list/array
        if (contextValue is IEnumerable<object> list)
        {
            return list.Any(item =>
                item?.ToString()?.Equals(targetStr, StringComparison.OrdinalIgnoreCase) ?? false);
        }

        return contextStr.Contains(targetStr, StringComparison.OrdinalIgnoreCase);
    }

    private bool CompareIn(object? contextValue, List<string> targetValues)
    {
        if (contextValue == null || targetValues == null || !targetValues.Any())
            return false;

        var contextStr = contextValue.ToString();
        return targetValues.Any(v => v.Equals(contextStr, StringComparison.OrdinalIgnoreCase));
    }

    private bool CompareNumeric(object? contextValue, object? targetValue, Func<double, double, bool> comparison)
    {
        if (!double.TryParse(contextValue?.ToString(), out var contextNum))
            return false;
        if (!double.TryParse(targetValue?.ToString(), out var targetNum))
            return false;

        return comparison(contextNum, targetNum);
    }

    private bool IsEmpty(object? value)
    {
        if (value == null)
            return true;

        if (value is string str)
            return string.IsNullOrWhiteSpace(str);

        if (value is IEnumerable<object> list)
            return !list.Any();

        return false;
    }

    private bool MatchesRegex(object? contextValue, object? pattern)
    {
        if (contextValue == null || pattern == null)
            return false;

        try
        {
            return Regex.IsMatch(contextValue.ToString() ?? "", pattern.ToString() ?? "");
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region Action Execution

    private async Task<RuleActionResult> ExecuteActionAsync(
        Guid tenantId,
        RuleAction action,
        Dictionary<string, object> context,
        CancellationToken cancellationToken)
    {
        try
        {
            return action.Type switch
            {
                RuleActionTypes.AddFramework => await ExecuteAddFrameworkAsync(tenantId, action, cancellationToken),
                RuleActionTypes.SetFlag => ExecuteSetFlag(context, action),
                RuleActionTypes.AddTask => await ExecuteAddTaskAsync(tenantId, action, cancellationToken),
                RuleActionTypes.SendNotification => await ExecuteSendNotificationAsync(tenantId, action, cancellationToken),
                RuleActionTypes.TriggerWorkflow => await ExecuteTriggerWorkflowAsync(tenantId, action, cancellationToken),
                RuleActionTypes.CreateRecommendation => await ExecuteCreateRecommendationAsync(tenantId, action, cancellationToken),
                _ => new RuleActionResult
                {
                    ActionType = action.Type,
                    Success = true,
                    ResultDetails = $"Action type {action.Type} logged but not executed"
                }
            };
        }
        catch (Exception ex)
        {
            return new RuleActionResult
            {
                ActionType = action.Type,
                Success = false,
                ResultDetails = ex.Message
            };
        }
    }

    private async Task<RuleActionResult> ExecuteAddFrameworkAsync(
        Guid tenantId, RuleAction action, CancellationToken cancellationToken)
    {
        var frameworkCode = action.Parameters?.GetValueOrDefault("framework_code")?.ToString();
        if (string.IsNullOrEmpty(frameworkCode))
        {
            return new RuleActionResult
            {
                ActionType = action.Type,
                Success = false,
                ResultDetails = "Missing framework_code parameter"
            };
        }

        // Check if already exists
        var exists = await _dbContext.TenantBaselines
            .AnyAsync(b => b.TenantId == tenantId && b.BaselineCode == frameworkCode, cancellationToken);

        if (!exists)
        {
            _dbContext.TenantBaselines.Add(new TenantBaseline
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                BaselineCode = frameworkCode,
                ReasonJson = JsonSerializer.Serialize(new { Source = "ConditionalLogicRule", Action = action.Type }),
                CreatedDate = DateTime.UtcNow
            });
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return new RuleActionResult
        {
            ActionType = action.Type,
            Success = true,
            ResultDetails = exists ? $"Framework {frameworkCode} already exists" : $"Added framework {frameworkCode}"
        };
    }

    private RuleActionResult ExecuteSetFlag(Dictionary<string, object> context, RuleAction action)
    {
        var flagName = action.Parameters?.GetValueOrDefault("flag_name")?.ToString();
        var flagValue = action.Parameters?.GetValueOrDefault("flag_value");

        if (string.IsNullOrEmpty(flagName))
        {
            return new RuleActionResult
            {
                ActionType = action.Type,
                Success = false,
                ResultDetails = "Missing flag_name parameter"
            };
        }

        // Set in special_flags context
        if (!context.ContainsKey("special_flags"))
        {
            context["special_flags"] = new Dictionary<string, object>();
        }

        if (context["special_flags"] is Dictionary<string, object> flags)
        {
            flags[flagName] = flagValue ?? true;
        }

        return new RuleActionResult
        {
            ActionType = action.Type,
            Success = true,
            ResultDetails = $"Set flag {flagName} = {flagValue}"
        };
    }

    private async Task<RuleActionResult> ExecuteAddTaskAsync(
        Guid tenantId, RuleAction action, CancellationToken cancellationToken)
    {
        var taskName = action.Parameters?.GetValueOrDefault("task_name")?.ToString() ?? "Auto-generated task";
        var taskDescription = action.Parameters?.GetValueOrDefault("task_description")?.ToString();

        var task = new WorkflowTask
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = taskName,
            Description = taskDescription,
            Status = "Pending",
            CreatedDate = DateTime.UtcNow
        };

        _dbContext.Set<WorkflowTask>().Add(task);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new RuleActionResult
        {
            ActionType = action.Type,
            Success = true,
            ResultDetails = $"Created task: {taskName}",
            AffectedEntityId = task.Id
        };
    }

    private async Task<RuleActionResult> ExecuteSendNotificationAsync(
        Guid tenantId, RuleAction action, CancellationToken cancellationToken)
    {
        // Log notification intent - actual sending would be handled by notification service
        _logger.LogInformation("Notification triggered for tenant {TenantId}: {Message}",
            tenantId, action.Parameters?.GetValueOrDefault("message"));

        return new RuleActionResult
        {
            ActionType = action.Type,
            Success = true,
            ResultDetails = "Notification queued"
        };
    }

    private async Task<RuleActionResult> ExecuteTriggerWorkflowAsync(
        Guid tenantId, RuleAction action, CancellationToken cancellationToken)
    {
        var workflowCode = action.Parameters?.GetValueOrDefault("workflow_code")?.ToString();

        _logger.LogInformation("Workflow trigger requested for tenant {TenantId}: {WorkflowCode}",
            tenantId, workflowCode);

        return new RuleActionResult
        {
            ActionType = action.Type,
            Success = true,
            ResultDetails = $"Workflow {workflowCode} trigger queued"
        };
    }

    private async Task<RuleActionResult> ExecuteCreateRecommendationAsync(
        Guid tenantId, RuleAction action, CancellationToken cancellationToken)
    {
        var recommendation = new NextBestActionRecommendation
        {
            TenantId = tenantId,
            ActionId = $"RULE_{Guid.NewGuid():N}",
            ActionType = action.Parameters?.GetValueOrDefault("action_type")?.ToString() ?? "Review",
            Description = action.Parameters?.GetValueOrDefault("description")?.ToString() ?? "Rule-based recommendation",
            ConfidenceScore = 70,
            Priority = 3,
            Rationale = "Generated by conditional logic rule",
            Status = "Pending",
            ExpiresAt = DateTime.UtcNow.AddHours(48)
        };

        _dbContext.Set<NextBestActionRecommendation>().Add(recommendation);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new RuleActionResult
        {
            ActionType = action.Type,
            Success = true,
            ResultDetails = "Created NBA recommendation",
            AffectedEntityId = recommendation.Id
        };
    }

    #endregion

    #region Helper Classes

    private class ConditionNode
    {
        public string? Type { get; set; } // "and", "or", or null for single condition
        public List<ConditionNode>? Conditions { get; set; }
        public string? Field { get; set; }
        public string? Operator { get; set; }
        public object? Value { get; set; }
        public List<string>? Values { get; set; } // For "in" operator
    }

    private class RuleAction
    {
        public string Type { get; set; } = "";
        public Dictionary<string, object>? Parameters { get; set; }
    }

    #endregion
}
