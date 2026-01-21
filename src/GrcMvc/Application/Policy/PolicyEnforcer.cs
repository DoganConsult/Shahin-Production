using GrcMvc.Application.Policy.PolicyModels;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace GrcMvc.Application.Policy;

/// <summary>
/// Core policy enforcement engine with deterministic evaluation and performance metrics
/// </summary>
public class PolicyEnforcer : IPolicyEnforcer
{
    private readonly IPolicyStore _policyStore;
    private readonly IDotPathResolver _pathResolver;
    private readonly IMutationApplier _mutationApplier;
    private readonly IPolicyAuditLogger _auditLogger;
    private readonly ILogger<PolicyEnforcer> _logger;
    
    // Advanced: Performance metrics for monitoring
    private readonly ConcurrentDictionary<string, PolicyMetrics> _metrics = new();

    public PolicyEnforcer(
        IPolicyStore policyStore,
        IDotPathResolver pathResolver,
        IMutationApplier mutationApplier,
        IPolicyAuditLogger auditLogger,
        ILogger<PolicyEnforcer> logger)
    {
        _policyStore = policyStore;
        _pathResolver = pathResolver;
        _mutationApplier = mutationApplier;
        _auditLogger = auditLogger;
        _logger = logger;
    }

    public async Task EnforceAsync(PolicyContext ctx, CancellationToken ct = default)
    {
        var decision = await EvaluateAsync(ctx, ct);
        
        if (decision.Effect == "deny")
        {
            throw new PolicyViolationException(
                decision.Message ?? "Policy violation",
                decision.MatchedRuleId ?? "unknown",
                decision.RemediationHint ?? "Contact administrator");
        }
    }

    public async Task<PolicyDecision> EvaluateAsync(PolicyContext ctx, CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var policy = await _policyStore.GetPolicyAsync(ct);
            
            if (!await _policyStore.ValidatePolicyAsync(policy, ct))
            {
                _logger.LogWarning("Policy validation failed, using default effect");
                return new PolicyDecision
                {
                    Effect = policy.Spec.DefaultEffect,
                    Message = "Policy validation failed"
                };
            }

            // Check if resource type is in target
            if (!policy.Spec.Target.ResourceTypes.Contains("Any") &&
                !policy.Spec.Target.ResourceTypes.Contains(ctx.ResourceType))
            {
                return new PolicyDecision { Effect = policy.Spec.DefaultEffect };
            }

            // Check environment
            if (policy.Spec.Target.Environments.Any() &&
                !policy.Spec.Target.Environments.Contains(ctx.Environment))
            {
                return new PolicyDecision { Effect = policy.Spec.DefaultEffect };
            }

            // Evaluate exceptions first (they override rules)
            var applicableExceptions = policy.Spec.Exceptions
                .Where(e => IsExceptionApplicable(e, ctx))
                .ToList();

            // Evaluate rules in priority order (deterministic)
            var applicableRules = policy.Spec.Rules
                .Where(r => r.Enabled && IsRuleApplicable(r, ctx))
                .OrderBy(r => r.Priority)
                .ThenBy(r => r.Id) // Deterministic ordering
                .ToList();

            // Remove rules covered by exceptions
            var rulesToEvaluate = applicableRules
                .Where(r => !applicableExceptions.Any(e => e.RuleIds.Contains(r.Id)))
                .ToList();

            var decisions = new List<PolicyDecision>();

            foreach (var rule in rulesToEvaluate)
            {
                var decision = await EvaluateRuleAsync(rule, ctx, ct);
                decisions.Add(decision);

                // Apply mutations if effect=mutate
                if (decision.Effect == "mutate")
                {
                    await _mutationApplier.ApplyAsync(rule.Mutations, ctx.Resource, ct);
                    // Continue evaluation after mutation
                    continue;
                }

                // Short-circuit on deny if configured
                if (policy.Spec.Execution.ShortCircuit && decision.Effect == "deny")
                {
                    break;
                }
            }

            // Resolve conflicts using configured strategy
            var finalDecision = ResolveConflict(decisions, policy.Spec.Execution.ConflictStrategy, policy.Spec.DefaultEffect);

            // Audit decision
            await _auditLogger.LogDecisionAsync(ctx, decisions, finalDecision, ct);

            // Update performance metrics
            UpdateMetrics(ctx.ResourceType, ctx.Action, stopwatch.ElapsedMilliseconds, finalDecision.Effect);

            return finalDecision;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating policy for {ResourceType} {Action}", ctx.ResourceType, ctx.Action);
            throw;
        }
        finally
        {
            stopwatch.Stop();
        }
    }

    public async Task<bool> IsAllowedAsync(PolicyContext ctx, CancellationToken ct = default)
    {
        var decision = await EvaluateAsync(ctx, ct);
        return decision.Effect == "allow";
    }

    private bool IsRuleApplicable(PolicyRule rule, PolicyContext ctx)
    {
        // Check resource type match
        if (rule.Match.Resource.Type != "*" && rule.Match.Resource.Type != ctx.ResourceType)
            return false;

        // Check environment match
        if (rule.Match.Environment != "*" && rule.Match.Environment != ctx.Environment)
            return false;

        // Check principal match
        if (rule.Match.Principal != null)
        {
            if (!string.IsNullOrEmpty(rule.Match.Principal.Id) && rule.Match.Principal.Id != ctx.PrincipalId)
                return false;

            if (rule.Match.Principal.Roles.Any() && 
                !rule.Match.Principal.Roles.Any(r => ctx.PrincipalRoles.Contains(r)))
                return false;
        }

        // Evaluate conditions
        foreach (var condition in rule.When)
        {
            if (!EvaluateCondition(condition, ctx.Resource))
                return false;
        }

        return true;
    }

    private bool EvaluateCondition(PolicyCondition condition, object resource)
    {
        var value = _pathResolver.Resolve(resource, condition.Path);

        return condition.Op switch
        {
            "exists" => value != null,
            "equals" => Equals(value, condition.Value),
            "notEquals" => !Equals(value, condition.Value),
            "in" => condition.Value is System.Collections.IEnumerable enumerable && 
                    enumerable.Cast<object>().Contains(value),
            "notIn" => condition.Value is System.Collections.IEnumerable enumerable && 
                       !enumerable.Cast<object>().Contains(value),
            "matches" => value?.ToString() is string str && 
                        System.Text.RegularExpressions.Regex.IsMatch(str, condition.Value?.ToString() ?? ""),
            "notMatches" => value?.ToString() is string str && 
                           !System.Text.RegularExpressions.Regex.IsMatch(str, condition.Value?.ToString() ?? ""),
            _ => false
        };
    }

    private bool IsExceptionApplicable(PolicyException exception, PolicyContext ctx)
    {
        // Check expiry (ensure UTC comparison)
        if (exception.ExpiresAt.HasValue)
        {
            var expiryUtc = exception.ExpiresAt.Value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(exception.ExpiresAt.Value, DateTimeKind.Utc)
                : exception.ExpiresAt.Value.ToUniversalTime();
            
            if (expiryUtc < DateTime.UtcNow)
            {
                _logger.LogDebug("Exception {ExceptionId} has expired at {ExpiryUtc}", exception.Id, expiryUtc);
                return false;
            }
        }

        // Check match criteria (reuse rule matching logic)
        return IsRuleApplicable(new PolicyRule { Match = exception.Match }, ctx);
    }

    private async Task<PolicyDecision> EvaluateRuleAsync(PolicyRule rule, PolicyContext ctx, CancellationToken ct)
    {
        return await Task.FromResult(new PolicyDecision
        {
            Effect = rule.Effect,
            MatchedRuleId = rule.Id,
            Message = rule.Message,
            RemediationHint = rule.Remediation.Hint,
            Severity = rule.Severity,
            Metadata = rule.Metadata
        });
    }

    private PolicyDecision ResolveConflict(
        List<PolicyDecision> decisions, 
        string conflictStrategy, 
        string defaultEffect)
    {
        if (!decisions.Any())
            return new PolicyDecision { Effect = defaultEffect };

        return conflictStrategy switch
        {
            "denyOverrides" => decisions.FirstOrDefault(d => d.Effect == "deny") 
                              ?? decisions.FirstOrDefault(d => d.Effect != "allow") 
                              ?? new PolicyDecision { Effect = defaultEffect },
            "allowOverrides" => decisions.FirstOrDefault(d => d.Effect == "allow") 
                               ?? decisions.FirstOrDefault(d => d.Effect != "deny") 
                               ?? new PolicyDecision { Effect = defaultEffect },
            "highestPriorityWins" => decisions.OrderByDescending(d => 
                d.Severity == "critical" ? 4 : 
                d.Severity == "high" ? 3 : 
                d.Severity == "medium" ? 2 : 1).First(),
            _ => new PolicyDecision { Effect = defaultEffect }
        };
    }

    private void UpdateMetrics(string resourceType, string action, long durationMs, string effect)
    {
        var key = $"{resourceType}:{action}";
        var metrics = _metrics.GetOrAdd(key, _ => new PolicyMetrics());
        
        Interlocked.Increment(ref metrics.TotalEvaluations);
        if (effect == "deny")
            Interlocked.Increment(ref metrics.DeniedCount);
        
        // Update average duration (simplified rolling average)
        var currentAvg = Interlocked.Read(ref metrics.AverageDurationMs);
        var newAvg = currentAvg == 0 ? durationMs : (currentAvg + durationMs) / 2;
        Interlocked.Exchange(ref metrics.AverageDurationMs, newAvg);
    }

    /// <summary>
    /// Get performance metrics for monitoring (advanced feature)
    /// </summary>
    public Dictionary<string, PolicyMetrics> GetMetrics() => 
        _metrics.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
}

/// <summary>
/// Performance metrics for policy evaluation
/// </summary>
public class PolicyMetrics
{
    public long TotalEvaluations;
    public long DeniedCount;
    public long AverageDurationMs;
}
