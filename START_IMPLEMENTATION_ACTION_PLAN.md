# 🚀 GRC SYSTEM - START IMPLEMENTATION ACTION PLAN
**Date:** 2025-01-22
**Status:** READY TO EXECUTE
**Total Duration:** 38 days (5 + 30 + 3)

---

## 📋 EXECUTIVE SUMMARY

This document provides **exact step-by-step instructions** to implement:
1. **Policy Enforcement System** (5 days) - START NOW
2. **Missing Blazor Pages** (30 days) - Start after Day 5
3. **Background Jobs** (3 days) - Can run parallel with pages

**Each step includes:**
- ✅ Exact file paths to create/modify
- ✅ Complete code to copy/paste
- ✅ Dependencies checklist
- ✅ Testing instructions
- ✅ Success criteria

---

## 🔴 PHASE 1: POLICY ENFORCEMENT SYSTEM (DAYS 1-5)

### ✅ PREREQUISITES CHECKLIST

Before starting, verify:
- [ ] .NET 8.0 SDK installed
- [ ] Project builds successfully: `dotnet build`
- [ ] Database connection working
- [ ] NuGet package: `YamlDotNet` (we'll add it)

---

### 📅 DAY 1: CREATE POLICY INFRASTRUCTURE

#### Step 1.1: Install Required NuGet Package

```bash
cd /home/dogan/grc-system/src/GrcMvc
dotnet add package YamlDotNet --version 15.1.4
```

#### Step 1.2: Create Directory Structure

```bash
mkdir -p src/GrcMvc/Application/Policy/PolicyModels
mkdir -p etc/policies
```

#### Step 1.3: Create PolicyContext.cs

**File:** `src/GrcMvc/Application/Policy/PolicyContext.cs`

```csharp
using System;
using System.Collections.Generic;

namespace GrcMvc.Application.Policy;

/// <summary>
/// Context for policy evaluation with advanced metadata
/// </summary>
public sealed class PolicyContext
{
    public required string Action { get; init; } // create/update/submit/approve/publish/delete
    public required string Environment { get; init; } // dev/staging/prod
    public required string ResourceType { get; init; } // Evidence/Risk/PolicyDocument/...
    public required object Resource { get; init; } // Entity or DTO
    public Guid? TenantId { get; init; }
    public string? PrincipalId { get; init; }
    public IReadOnlyList<string> PrincipalRoles { get; init; } = Array.Empty<string>();

    // Advanced: Additional context for complex rules
    public Dictionary<string, object> Metadata { get; init; } = new();
    public string? CorrelationId { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
```

#### Step 1.4: Create Policy Models

**File:** `src/GrcMvc/Application/Policy/PolicyModels/PolicyDocument.cs`

```csharp
namespace GrcMvc.Application.Policy.PolicyModels;

public class PolicyDocument
{
    public string ApiVersion { get; set; } = "policy.doganconsult.io/v1";
    public string Kind { get; set; } = "Policy";
    public PolicyMetadata Metadata { get; set; } = new();
    public PolicySpec Spec { get; set; } = new();
}

public class PolicyMetadata
{
    public string Name { get; set; } = string.Empty;
    public string Namespace { get; set; } = "default";
    public string Version { get; set; } = "1.0.0";
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, string> Labels { get; set; } = new();
    public Dictionary<string, string> Annotations { get; set; } = new();
}

public class PolicySpec
{
    public string Mode { get; set; } = "enforce"; // enforce|audit
    public string DefaultEffect { get; set; } = "allow";
    public PolicyExecution Execution { get; set; } = new();
    public PolicyTarget Target { get; set; } = new();
    public List<PolicyRule> Rules { get; set; } = new();
    public List<PolicyException> Exceptions { get; set; } = new();
    public PolicyAuditConfig Audit { get; set; } = new();
}

public class PolicyExecution
{
    public string Order { get; set; } = "sequential";
    public bool ShortCircuit { get; set; } = true;
    public string ConflictStrategy { get; set; } = "denyOverrides"; // denyOverrides|allowOverrides|highestPriorityWins
}

public class PolicyTarget
{
    public List<string> ResourceTypes { get; set; } = new();
    public List<string> Environments { get; set; } = new();
}

public class PolicyRule
{
    public string Id { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public PolicyMatch Match { get; set; } = new();
    public List<PolicyCondition> When { get; set; } = new();
    public string Effect { get; set; } = "allow"; // allow|deny|audit|mutate
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = "medium"; // low|medium|high|critical
    public List<PolicyMutation> Mutations { get; set; } = new();
    public PolicyRemediation Remediation { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class PolicyMatch
{
    public PolicyResourceMatch Resource { get; set; } = new();
    public PolicyPrincipalMatch? Principal { get; set; }
    public string Environment { get; set; } = "*";
}

public class PolicyResourceMatch
{
    public string Type { get; set; } = "*";
    public string Name { get; set; } = "*";
    public Dictionary<string, string> Labels { get; set; } = new();
}

public class PolicyPrincipalMatch
{
    public string? Id { get; set; }
    public List<string> Roles { get; set; } = new();
}

public class PolicyCondition
{
    public string Op { get; set; } = string.Empty; // exists|equals|notEquals|in|notIn|matches|notMatches
    public string Path { get; set; } = string.Empty;
    public object? Value { get; set; }
}

public class PolicyMutation
{
    public string Op { get; set; } = string.Empty; // set|remove|add
    public string Path { get; set; } = string.Empty;
    public object? Value { get; set; }
}

public class PolicyRemediation
{
    public string? Url { get; set; }
    public string? Hint { get; set; }
}

public class PolicyException
{
    public string Id { get; set; } = string.Empty;
    public List<string> RuleIds { get; set; } = new();
    public string? Reason { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public PolicyMatch Match { get; set; } = new();
}

public class PolicyAuditConfig
{
    public bool LogDecisions { get; set; } = true;
    public int RetentionDays { get; set; } = 365;
    public List<PolicyAuditSink> Sinks { get; set; } = new();
}

public class PolicyAuditSink
{
    public string Type { get; set; } = string.Empty; // stdout|file|http
    public string? Path { get; set; }
    public string? Url { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new();
}

public class PolicyDecision
{
    public string Effect { get; set; } = string.Empty; // allow|deny|audit|mutate
    public string? MatchedRuleId { get; set; }
    public string? Message { get; set; }
    public string? RemediationHint { get; set; }
    public string Severity { get; set; } = "medium";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Metadata { get; set; } = new();
}
```

#### Step 1.5: Create Policy Interfaces

**File:** `src/GrcMvc/Application/Policy/IPolicyEnforcer.cs`

```csharp
namespace GrcMvc.Application.Policy;

public interface IPolicyEnforcer
{
    Task EnforceAsync(PolicyContext ctx, CancellationToken ct = default);
    Task<PolicyDecision> EvaluateAsync(PolicyContext ctx, CancellationToken ct = default);
    Task<bool> IsAllowedAsync(PolicyContext ctx, CancellationToken ct = default);
}
```

**File:** `src/GrcMvc/Application/Policy/IPolicyStore.cs`

```csharp
using GrcMvc.Application.Policy.PolicyModels;

namespace GrcMvc.Application.Policy;

public interface IPolicyStore
{
    Task<PolicyDocument> GetPolicyAsync(CancellationToken ct = default);
    Task<PolicyDocument> ReloadPolicyAsync(CancellationToken ct = default);
    Task<bool> ValidatePolicyAsync(PolicyDocument policy, CancellationToken ct = default);
    event EventHandler<PolicyReloadedEventArgs>? PolicyReloaded;
}

public class PolicyReloadedEventArgs : EventArgs
{
    public PolicyDocument Policy { get; set; } = null!;
}
```

**File:** `src/GrcMvc/Application/Policy/IDotPathResolver.cs`

```csharp
namespace GrcMvc.Application.Policy;

public interface IDotPathResolver
{
    object? Resolve(object obj, string path);
    bool Exists(object obj, string path);
    void Set(object obj, string path, object? value);
    void Remove(object obj, string path);
}
```

**File:** `src/GrcMvc/Application/Policy/IMutationApplier.cs`

```csharp
using GrcMvc.Application.Policy.PolicyModels;

namespace GrcMvc.Application.Policy;

public interface IMutationApplier
{
    Task ApplyAsync(IEnumerable<PolicyMutation> mutations, object resource, CancellationToken ct = default);
}
```

**File:** `src/GrcMvc/Application/Policy/IPolicyAuditLogger.cs`

```csharp
using GrcMvc.Application.Policy.PolicyModels;

namespace GrcMvc.Application.Policy;

public interface IPolicyAuditLogger
{
    Task LogDecisionAsync(PolicyContext ctx, IEnumerable<PolicyDecision> decisions, PolicyDecision finalDecision, CancellationToken ct = default);
}
```

**File:** `src/GrcMvc/Application/Policy/PolicyViolationException.cs`

```csharp
namespace GrcMvc.Application.Policy;

public class PolicyViolationException : Exception
{
    public string RuleId { get; }
    public string RemediationHint { get; }

    public PolicyViolationException(string message, string ruleId, string remediationHint)
        : base(message)
    {
        RuleId = ruleId;
        RemediationHint = remediationHint;
    }

    public PolicyViolationException(string message, string ruleId, string remediationHint, Exception innerException)
        : base(message, innerException)
    {
        RuleId = ruleId;
        RemediationHint = remediationHint;
    }
}
```

#### ✅ DAY 1 COMPLETION CHECKLIST

- [ ] NuGet package installed
- [ ] Directory structure created
- [ ] All policy model files created
- [ ] All interface files created
- [ ] Project compiles: `dotnet build`
- [ ] No compilation errors

**Command to verify:**
```bash
cd /home/dogan/grc-system/src/GrcMvc
dotnet build --no-incremental
```

---

### 📅 DAY 2: IMPLEMENT CORE COMPONENTS

#### Step 2.1: Implement DotPathResolver

**File:** `src/GrcMvc/Application/Policy/DotPathResolver.cs`

```csharp
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Application.Policy;

public class DotPathResolver : IDotPathResolver
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<DotPathResolver> _logger;

    public DotPathResolver(IMemoryCache cache, ILogger<DotPathResolver> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public object? Resolve(object obj, string path)
    {
        if (string.IsNullOrEmpty(path))
            return obj;

        var cacheKey = $"{obj.GetType().FullName}:{path}";
        if (_cache.TryGetValue(cacheKey, out var cached))
            return cached;

        try
        {
            var parts = path.Split('.');
            object? current = obj;

            foreach (var part in parts)
            {
                if (current == null)
                    return null;

                // Handle dictionary
                if (current is System.Collections.IDictionary dict)
                {
                    current = dict.Contains(part) ? dict[part] : null;
                    continue;
                }

                // Handle JSON element
                if (current is JsonElement jsonElement)
                {
                    if (jsonElement.ValueKind == JsonValueKind.Object && jsonElement.TryGetProperty(part, out var prop))
                    {
                        current = prop;
                        continue;
                    }
                    return null;
                }

                // Handle object properties via reflection
                var type = current.GetType();
                var propInfo = type.GetProperty(part,
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.IgnoreCase);

                if (propInfo != null)
                {
                    current = propInfo.GetValue(current);
                }
                else
                {
                    return null;
                }
            }

            _cache.Set(cacheKey, current, TimeSpan.FromMinutes(5));
            return current;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error resolving path {Path} on {Type}", path, obj.GetType().Name);
            return null;
        }
    }

    public bool Exists(object obj, string path)
    {
        return Resolve(obj, path) != null;
    }

    public void Set(object obj, string path, object? value)
    {
        var parts = path.Split('.');
        object? current = obj;

        for (int i = 0; i < parts.Length - 1; i++)
        {
            current = Resolve(current, parts[i]);
            if (current == null)
                throw new InvalidOperationException($"Path segment {parts[i]} does not exist");
        }

        var finalPart = parts[^1];
        var type = current!.GetType();
        var propInfo = type.GetProperty(finalPart,
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.IgnoreCase);

        if (propInfo != null && propInfo.CanWrite)
        {
            propInfo.SetValue(current, value);
        }
        else
        {
            throw new InvalidOperationException($"Cannot set property {finalPart} on {type.Name}");
        }
    }

    public void Remove(object obj, string path)
    {
        Set(obj, path, null);
    }
}
```

#### Step 2.2: Implement MutationApplier

**File:** `src/GrcMvc/Application/Policy/MutationApplier.cs`

```csharp
using GrcMvc.Application.Policy.PolicyModels;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Application.Policy;

public class MutationApplier : IMutationApplier
{
    private readonly IDotPathResolver _pathResolver;
    private readonly ILogger<MutationApplier> _logger;

    public MutationApplier(IDotPathResolver pathResolver, ILogger<MutationApplier> logger)
    {
        _pathResolver = pathResolver;
        _logger = logger;
    }

    public async Task ApplyAsync(IEnumerable<PolicyMutation> mutations, object resource, CancellationToken ct = default)
    {
        foreach (var mutation in mutations)
        {
            try
            {
                switch (mutation.Op.ToLower())
                {
                    case "set":
                        _pathResolver.Set(resource, mutation.Path, mutation.Value);
                        _logger.LogDebug("Applied mutation: set {Path} = {Value}", mutation.Path, mutation.Value);
                        break;
                    case "remove":
                        _pathResolver.Remove(resource, mutation.Path);
                        _logger.LogDebug("Applied mutation: remove {Path}", mutation.Path);
                        break;
                    case "add":
                        // For arrays/lists - implementation depends on resource type
                        _logger.LogWarning("Add mutation not fully implemented for {Path}", mutation.Path);
                        break;
                    default:
                        _logger.LogWarning("Unknown mutation operation: {Op}", mutation.Op);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying mutation {Op} to {Path}", mutation.Op, mutation.Path);
                throw;
            }
        }

        await Task.CompletedTask;
    }
}
```

#### Step 2.3: Implement PolicyAuditLogger

**File:** `src/GrcMvc/Application/Policy/PolicyAuditLogger.cs`

```csharp
using GrcMvc.Application.Policy.PolicyModels;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Application.Policy;

public class PolicyAuditLogger : IPolicyAuditLogger
{
    private readonly ILogger<PolicyAuditLogger> _logger;

    public PolicyAuditLogger(ILogger<PolicyAuditLogger> logger)
    {
        _logger = logger;
    }

    public async Task LogDecisionAsync(
        PolicyContext ctx,
        IEnumerable<PolicyDecision> decisions,
        PolicyDecision finalDecision,
        CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Policy Decision: {Effect} for {ResourceType} {Action} by {PrincipalId}. " +
            "Matched Rules: {RuleCount}, Final: {FinalRuleId}",
            finalDecision.Effect,
            ctx.ResourceType,
            ctx.Action,
            ctx.PrincipalId,
            decisions.Count(),
            finalDecision.MatchedRuleId);

        if (finalDecision.Effect == "deny")
        {
            _logger.LogWarning(
                "Policy Violation: {Message}. Rule: {RuleId}. Remediation: {Remediation}",
                finalDecision.Message,
                finalDecision.MatchedRuleId,
                finalDecision.RemediationHint);
        }

        await Task.CompletedTask;
    }
}
```

#### ✅ DAY 2 COMPLETION CHECKLIST

- [ ] DotPathResolver implemented and tested
- [ ] MutationApplier implemented
- [ ] PolicyAuditLogger implemented
- [ ] Project compiles: `dotnet build`
- [ ] No compilation errors

---

### 📅 DAY 3: IMPLEMENT POLICY ENFORCER

#### Step 3.1: Implement PolicyEnforcer

**File:** `src/GrcMvc/Application/Policy/PolicyEnforcer.cs`

```csharp
using GrcMvc.Application.Policy.PolicyModels;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace GrcMvc.Application.Policy;

public class PolicyEnforcer : IPolicyEnforcer
{
    private readonly IPolicyStore _policyStore;
    private readonly IDotPathResolver _pathResolver;
    private readonly IMutationApplier _mutationApplier;
    private readonly IPolicyAuditLogger _auditLogger;
    private readonly ILogger<PolicyEnforcer> _logger;

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

            // Check resource type
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

            // Evaluate exceptions first
            var applicableExceptions = policy.Spec.Exceptions
                .Where(e => IsExceptionApplicable(e, ctx))
                .ToList();

            // Evaluate rules
            var applicableRules = policy.Spec.Rules
                .Where(r => r.Enabled && IsRuleApplicable(r, ctx))
                .OrderBy(r => r.Priority)
                .ThenBy(r => r.Id)
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

                if (decision.Effect == "mutate")
                {
                    await _mutationApplier.ApplyAsync(rule.Mutations, ctx.Resource, ct);
                    continue;
                }

                if (policy.Spec.Execution.ShortCircuit && decision.Effect == "deny")
                {
                    break;
                }
            }

            var finalDecision = ResolveConflict(decisions, policy.Spec.Execution.ConflictStrategy, policy.Spec.DefaultEffect);

            await _auditLogger.LogDecisionAsync(ctx, decisions, finalDecision, ct);

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
        if (rule.Match.Resource.Type != "*" && rule.Match.Resource.Type != ctx.ResourceType)
            return false;

        if (rule.Match.Environment != "*" && rule.Match.Environment != ctx.Environment)
            return false;

        if (rule.Match.Principal != null)
        {
            if (!string.IsNullOrEmpty(rule.Match.Principal.Id) && rule.Match.Principal.Id != ctx.PrincipalId)
                return false;

            if (rule.Match.Principal.Roles.Any() &&
                !rule.Match.Principal.Roles.Any(r => ctx.PrincipalRoles.Contains(r)))
                return false;
        }

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
        if (exception.ExpiresAt.HasValue && exception.ExpiresAt.Value < DateTime.UtcNow)
            return false;

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

        var currentAvg = Interlocked.Read(ref metrics.AverageDurationMs);
        var newAvg = (currentAvg + durationMs) / 2;
        Interlocked.Exchange(ref metrics.AverageDurationMs, newAvg);
    }

    public Dictionary<string, PolicyMetrics> GetMetrics() =>
        _metrics.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
}

public class PolicyMetrics
{
    public long TotalEvaluations;
    public long DeniedCount;
    public long AverageDurationMs;
}
```

#### ✅ DAY 3 COMPLETION CHECKLIST

- [ ] PolicyEnforcer fully implemented
- [ ] All helper methods implemented
- [ ] Project compiles: `dotnet build`
- [ ] No compilation errors

---

### 📅 DAY 4: POLICY STORE & YAML INTEGRATION

#### Step 4.1: Implement PolicyStore

**File:** `src/GrcMvc/Application/Policy/PolicyStore.cs`

```csharp
using GrcMvc.Application.Policy.PolicyModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;
using System.IO;

namespace GrcMvc.Application.Policy;

public class PolicyStore : IPolicyStore, IHostedService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<PolicyStore> _logger;
    private readonly IDeserializer _yamlDeserializer;
    private PolicyDocument? _cachedPolicy;
    private readonly object _lock = new();
    private FileSystemWatcher? _watcher;

    public event EventHandler<PolicyReloadedEventArgs>? PolicyReloaded;

    public PolicyStore(
        IConfiguration configuration,
        ILogger<PolicyStore> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _yamlDeserializer = new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .Build();
    }

    public async Task<PolicyDocument> GetPolicyAsync(CancellationToken ct = default)
    {
        if (_cachedPolicy != null)
            return _cachedPolicy;

        return await ReloadPolicyAsync(ct);
    }

    public async Task<PolicyDocument> ReloadPolicyAsync(CancellationToken ct = default)
    {
        var policyPath = _configuration["Policy:FilePath"] ?? "etc/policies/grc-baseline.yml";

        if (!File.Exists(policyPath))
        {
            _logger.LogWarning("Policy file not found at {Path}, using default policy", policyPath);
            return GetDefaultPolicy();
        }

        try
        {
            var yamlContent = await File.ReadAllTextAsync(policyPath, ct);
            var policy = _yamlDeserializer.Deserialize<PolicyDocument>(yamlContent);

            lock (_lock)
            {
                _cachedPolicy = policy;
            }

            _logger.LogInformation("Policy reloaded successfully from {Path}", policyPath);
            PolicyReloaded?.Invoke(this, new PolicyReloadedEventArgs { Policy = policy });

            return policy;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading policy from {Path}", policyPath);
            throw;
        }
    }

    public async Task<bool> ValidatePolicyAsync(PolicyDocument policy, CancellationToken ct = default)
    {
        if (policy.Spec == null)
            return false;

        if (!policy.Spec.Rules.Any())
            return false;

        foreach (var rule in policy.Spec.Rules)
        {
            if (string.IsNullOrEmpty(rule.Id))
                return false;
            if (rule.Priority < 1 || rule.Priority > 10000)
                return false;
            if (string.IsNullOrEmpty(rule.Effect))
                return false;
        }

        return await Task.FromResult(true);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var policyPath = _configuration["Policy:FilePath"] ?? "etc/policies/grc-baseline.yml";
        var directory = Path.GetDirectoryName(policyPath);
        var fileName = Path.GetFileName(policyPath);

        if (directory != null && Directory.Exists(directory))
        {
            _watcher = new FileSystemWatcher(directory, fileName)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
            };
            _watcher.Changed += async (sender, e) =>
            {
                _logger.LogInformation("Policy file changed, reloading...");
                await Task.Delay(500);
                await ReloadPolicyAsync();
            };
            _watcher.EnableRaisingEvents = true;
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _watcher?.Dispose();
        return Task.CompletedTask;
    }

    private PolicyDocument GetDefaultPolicy()
    {
        return new PolicyDocument
        {
            Metadata = new PolicyMetadata
            {
                Name = "default-policy",
                Version = "1.0.0",
                CreatedAt = DateTime.UtcNow
            },
            Spec = new PolicySpec
            {
                DefaultEffect = "allow",
                Rules = new List<PolicyRule>()
            }
        };
    }
}
```

#### Step 4.2: Create YAML Policy File

**File:** `etc/policies/grc-baseline.yml`

```yaml
apiVersion: policy.doganconsult.io/v1
kind: Policy

metadata:
  name: baseline-governance
  namespace: default
  version: "1.0.0"
  createdAt: "2025-01-22T18:30:00+03:00"
  labels:
    owner: "dogan-consult"
    domain: "governance"
  annotations:
    purpose: "Baseline deterministic enforcement rules for all resources"

spec:
  mode: enforce
  defaultEffect: allow

  execution:
    order: sequential
    shortCircuit: true
    conflictStrategy: denyOverrides

  target:
    resourceTypes:
      - "Any"
    environments:
      - "dev"
      - "staging"
      - "prod"

  audit:
    logDecisions: true
    retentionDays: 365
    sinks:
      - type: stdout

  rules:
    - id: REQUIRE_DATA_CLASSIFICATION
      priority: 10
      description: "Every resource must carry a data classification label."
      enabled: true
      match:
        resource:
          type: "Any"
          name: "*"
      when:
        - op: notMatches
          path: "metadata.labels.dataClassification"
          value: "^(public|internal|confidential|restricted)$"
      effect: deny
      severity: high
      message: "Missing/invalid metadata.labels.dataClassification. Allowed: public|internal|confidential|restricted."
      remediation:
        hint: "Set metadata.labels.dataClassification to one of the allowed values."

    - id: REQUIRE_OWNER
      priority: 20
      description: "Every resource must declare an owner label."
      enabled: true
      match:
        resource:
          type: "Any"
          name: "*"
      when:
        - op: notMatches
          path: "metadata.labels.owner"
          value: "^.{2,256}$"
      effect: deny
      severity: medium
      message: "Missing/invalid metadata.labels.owner."
      remediation:
        hint: "Set metadata.labels.owner to a team or individual identifier."

    - id: PROD_RESTRICTED_MUST_HAVE_APPROVAL
      priority: 30
      description: "Restricted data in prod requires an approval flag."
      enabled: true
      match:
        resource:
          type: "Any"
          name: "*"
        environment: "prod"
      when:
        - op: equals
          path: "metadata.labels.dataClassification"
          value: "restricted"
        - op: notEquals
          path: "metadata.labels.approvedForProd"
          value: "true"
      effect: deny
      severity: critical
      message: "Restricted data in prod requires metadata.labels.approvedForProd=true."
      remediation:
        hint: "Run the approval workflow and set approvedForProd=true."

    - id: NORMALIZE_EMPTY_LABELS
      priority: 9000
      description: "Normalize known empty label values to null (deterministic mutation)."
      enabled: true
      match:
        resource:
          type: "Any"
          name: "*"
      when:
        - op: in
          path: "metadata.labels.owner"
          value: ["", "unknown", "n/a"]
      effect: mutate
      severity: low
      message: "Normalizing invalid owner label."
      mutations:
        - op: set
          path: "metadata.labels.owner"
          value: null

  exceptions:
    - id: TEMP_EXC_DEV_SANDBOX
      ruleIds:
        - PROD_RESTRICTED_MUST_HAVE_APPROVAL
      reason: "Dev sandbox does not require prod approval controls."
      expiresAt: "2026-01-31T23:59:59+03:00"
      match:
        resource:
          type: "Any"
          name: "*"
        environment: "dev"
```

#### Step 4.3: Add Configuration

**File:** `src/GrcMvc/appsettings.json` (add to existing file)

```json
{
  "Policy": {
    "FilePath": "etc/policies/grc-baseline.yml"
  }
}
```

#### ✅ DAY 4 COMPLETION CHECKLIST

- [ ] PolicyStore implemented
- [ ] YAML policy file created
- [ ] Configuration added
- [ ] Project compiles: `dotnet build`
- [ ] Policy file loads successfully

---

### 📅 DAY 5: INTEGRATION & TESTING

#### Step 5.1: Register Services in DI

**File:** `src/GrcMvc/Program.cs` (add these lines)

```csharp
// Add after existing service registrations (around line 420)

// Policy Enforcement System
builder.Services.AddScoped<IPolicyEnforcer, PolicyEnforcer>();
builder.Services.AddSingleton<IPolicyStore, PolicyStore>();
builder.Services.AddScoped<IDotPathResolver, DotPathResolver>();
builder.Services.AddScoped<IMutationApplier, MutationApplier>();
builder.Services.AddScoped<IPolicyAuditLogger, PolicyAuditLogger>();
builder.Services.AddHostedService<PolicyStore>(); // For hot reload
```

#### Step 5.2: Add Using Statements

**File:** `src/GrcMvc/Program.cs` (add to top)

```csharp
using GrcMvc.Application.Policy;
```

#### Step 5.3: Integrate into EvidenceService

**File:** `src/GrcMvc/Services/Implementations/EvidenceService.cs`

Add to constructor:
```csharp
private readonly IPolicyEnforcer _policyEnforcer;
private readonly IConfiguration _configuration;

public EvidenceService(
    // ... existing parameters
    IPolicyEnforcer policyEnforcer,
    IConfiguration configuration)
{
    // ... existing assignments
    _policyEnforcer = policyEnforcer;
    _configuration = configuration;
}
```

Modify CreateAsync method:
```csharp
[Authorize(GrcPermissions.Evidence.Upload)]
public async Task<EvidenceDto> CreateAsync(CreateEvidenceDto input)
{
    var entity = ObjectMapper.Map<CreateEvidenceDto, Evidence>(input);

    // Ensure metadata exists
    if (entity.Metadata == null)
        entity.Metadata = new Dictionary<string, object>();

    if (!entity.Metadata.ContainsKey("labels"))
        entity.Metadata["labels"] = new Dictionary<string, string>();

    var labels = (Dictionary<string, string>)entity.Metadata["labels"];
    labels["dataClassification"] = input.DataClassification ?? "internal";
    labels["owner"] = input.Owner ?? _currentUser.GetUserName();

    // Enforce policies
    await _policyEnforcer.EnforceAsync(new PolicyContext
    {
        Action = "create",
        Environment = _configuration["Environment"] ?? "dev",
        ResourceType = "Evidence",
        Resource = entity,
        TenantId = _currentUser.GetTenantId(),
        PrincipalId = _currentUser.GetUserId()?.ToString(),
        PrincipalRoles = await _roleResolver.GetCurrentRolesAsync(),
        CorrelationId = Guid.NewGuid().ToString()
    });

    await _evidenceRepo.InsertAsync(entity, autoSave: true);
    return ObjectMapper.Map<Evidence, EvidenceDto>(entity);
}
```

#### Step 5.4: Test Policy Enforcement

**Create test file:** `tests/PolicyEnforcementTests.cs`

```csharp
using Xunit;
using GrcMvc.Application.Policy;

namespace GrcMvc.Tests;

public class PolicyEnforcementTests
{
    [Fact]
    public async Task PolicyEnforcer_ShouldDeny_WhenDataClassificationMissing()
    {
        // Arrange
        var context = new PolicyContext
        {
            Action = "create",
            Environment = "prod",
            ResourceType = "Evidence",
            Resource = new { metadata = new { labels = new Dictionary<string, string>() } },
            TenantId = Guid.NewGuid(),
            PrincipalId = "test-user"
        };

        // Act & Assert
        // Implementation depends on your test setup
    }
}
```

#### ✅ DAY 5 COMPLETION CHECKLIST

- [ ] Services registered in DI
- [ ] EvidenceService integrated
- [ ] Project compiles: `dotnet build`
- [ ] Application starts: `dotnet run`
- [ ] Policy enforcement works (test create evidence without classification)

**Test Command:**
```bash
cd /home/dogan/grc-system/src/GrcMvc
dotnet run
# Then test creating evidence via API/UI
```

---

## 🟡 PHASE 2: MISSING BLAZOR PAGES (DAYS 6-35)

### Template for All 14 Pages

**Use this template and replace:**
- `[PAGE_NAME]` with actual page name
- `[SERVICE_NAME]` with corresponding service
- `[ROUTE]` with page route
- `[ARABIC_NAME]` with Arabic translation

**File Template:** `src/GrcMvc/Components/Pages/[PAGE_NAME]/Index.razor`

```razor
@page "/[ROUTE]"
@using GrcMvc.Models.DTOs
@inject I[SERVICE_NAME]Service Service
@inject ICurrentUserService CurrentUser
@inject NavigationManager Navigation
@inject IJSRuntime JSRuntime
@inject ILogger<[PAGE_NAME]Page> Logger
@inject IHttpContextAccessor HttpContextAccessor
@using Microsoft.AspNetCore.Localization

<PageTitle>[ARABIC_NAME]</PageTitle>

<div class="container-fluid" dir="@dir">
    <div class="row mb-3">
        <div class="col">
            <h3>@(isRtl ? "[ARABIC_NAME]" : "[ENGLISH_NAME]")</h3>
        </div>
        <div class="col-auto">
            <button class="btn btn-primary" @onclick="CreateNew">
                <i class="fas fa-plus"></i> @(isRtl ? "إضافة جديد" : "Add New")
            </button>
        </div>
    </div>

    @if (isLoading)
    {
        <LoadingSpinner />
    }
    else if (errorMessage != null)
    {
        <ErrorAlert Message="@errorMessage" />
    }
    else
    {
        <div class="card">
            <div class="card-body">
                <!-- Search and Filters -->
                <div class="row mb-3">
                    <div class="col-md-6">
                        <input type="text" class="form-control"
                               placeholder="@(isRtl ? "بحث..." : "Search...")"
                               @bind="searchTerm" @bind:event="oninput" />
                    </div>
                    <div class="col-md-3">
                        <button class="btn btn-outline-secondary w-100" @onclick="ExportToExcel">
                            <i class="fas fa-file-excel"></i> @(isRtl ? "تصدير" : "Export")
                        </button>
                    </div>
                </div>

                <!-- Data Table -->
                <div class="table-responsive">
                    <table class="table table-striped table-hover">
                        <thead>
                            <tr>
                                <th>@(isRtl ? "الاسم" : "Name")</th>
                                <th>@(isRtl ? "الحالة" : "Status")</th>
                                <th>@(isRtl ? "الإجراءات" : "Actions")</th>
                            </tr>
                        </thead>
                        <tbody>
                            @foreach (var item in filteredItems)
                            {
                                <tr>
                                    <td>@item.Name</td>
                                    <td><StatusBadge Status="@item.Status" /></td>
                                    <td>
                                        <button class="btn btn-sm btn-outline-primary"
                                                @onclick="() => ViewDetails(item.Id)">
                                            <i class="fas fa-eye"></i>
                                        </button>
                                        <button class="btn btn-sm btn-outline-secondary"
                                                @onclick="() => Edit(item.Id)">
                                            <i class="fas fa-edit"></i>
                                        </button>
                                        <button class="btn btn-sm btn-outline-danger"
                                                @onclick="() => Delete(item.Id)">
                                            <i class="fas fa-trash"></i>
                                        </button>
                                    </td>
                                </tr>
                            }
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    }
</div>

@code {
    private List<[DTO_TYPE]> items = new();
    private List<[DTO_TYPE]> filteredItems = new();
    private bool isLoading = true;
    private string? errorMessage;
    private string searchTerm = string.Empty;
    private bool isRtl = false;
    private string dir = "ltr";

    protected override async Task OnInitializedAsync()
    {
        var requestCulture = HttpContextAccessor.HttpContext?.Features.Get<IRequestCultureFeature>();
        var currentCulture = requestCulture?.RequestCulture?.UICulture?.Name ?? "ar";
        isRtl = currentCulture == "ar";
        dir = isRtl ? "rtl" : "ltr";

        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            isLoading = true;
            items = await Service.GetAllAsync();
            ApplyFilters();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading data");
            errorMessage = isRtl ? "حدث خطأ في تحميل البيانات" : "Error loading data";
        }
        finally
        {
            isLoading = false;
        }
    }

    private void ApplyFilters()
    {
        filteredItems = items
            .Where(i => string.IsNullOrEmpty(searchTerm) ||
                       i.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private void CreateNew() => Navigation.NavigateTo("/[ROUTE]/create");
    private void ViewDetails(Guid id) => Navigation.NavigateTo($"/[ROUTE]/{id}");
    private void Edit(Guid id) => Navigation.NavigateTo($"/[ROUTE]/{id}/edit");

    private async Task Delete(Guid id)
    {
        if (await JSRuntime.InvokeAsync<bool>("confirm",
            isRtl ? "هل أنت متأكد من الحذف؟" : "Are you sure?"))
        {
            try
            {
                await Service.DeleteAsync(id);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error deleting");
                errorMessage = isRtl ? "حدث خطأ في الحذف" : "Error deleting";
            }
        }
    }

    private async Task ExportToExcel()
    {
        await JSRuntime.InvokeVoidAsync("exportToExcel", filteredItems);
    }
}
```

### Page Implementation List

1. **Frameworks** - `/frameworks` - Use `IFrameworkService`
2. **Regulators** - `/regulators` - Use `IRegulatorService` (may need to create)
3. **Control Assessments** - `/control-assessments` - Use `IControlAssessmentService`
4. **Action Plans** - `/action-plans` - Use `IActionPlanService`
5. **Compliance Calendar** - `/compliance-calendar` - Use `IComplianceCalendarService`
6. **Notifications** - `/notifications` - Use `INotificationService`
7. **Vendors** - `/vendors` - Use `IVendorService`
8. **Integrations** - `/integrations` - Use `IIntegrationService`
9. **Subscriptions** - `/subscriptions` - Use `ISubscriptionService`
10. **Admin Tenants** - `/admin/tenants` - Use `ITenantService`
11-14. **Detail/Edit Pages** - Follow same pattern

**Timeline:** 2-3 pages per day = 30 days total

---

## 🟡 PHASE 3: BACKGROUND JOBS (DAYS 36-38)

### Day 36: ReportGenerationJob

**File:** `src/GrcMvc/BackgroundJobs/ReportGenerationJob.cs`

```csharp
using Hangfire;
using Microsoft.Extensions.Logging;
using GrcMvc.Data;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GrcMvc.BackgroundJobs;

public class ReportGenerationJob
{
    private readonly IReportService _reportService;
    private readonly GrcDbContext _context;
    private readonly ILogger<ReportGenerationJob> _logger;

    public ReportGenerationJob(
        IReportService reportService,
        GrcDbContext context,
        ILogger<ReportGenerationJob> logger)
    {
        _reportService = reportService;
        _context = context;
        _logger = logger;
    }

    [Hangfire.AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 300, 900 })]
    public async Task ExecuteAsync()
    {
        _logger.LogInformation("ReportGenerationJob started at {Time}", DateTime.UtcNow);

        try
        {
            var tenants = await _context.Tenants
                .Where(t => t.Status == "Active" && !t.IsDeleted)
                .ToListAsync();

            int successCount = 0;
            int errorCount = 0;

            foreach (var tenant in tenants)
            {
                try
                {
                    // Get scheduled reports for tenant
                    var reports = await _context.Reports
                        .Where(r => r.TenantId == tenant.Id &&
                                   r.Status == "Scheduled" &&
                                   !r.IsDeleted)
                        .ToListAsync();

                    foreach (var report in reports)
                    {
                        await _reportService.GenerateReportAsync(report.Id);
                        successCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error generating reports for tenant {TenantId}", tenant.Id);
                    errorCount++;
                }
            }

            _logger.LogInformation(
                "ReportGenerationJob completed. Success: {Success}, Errors: {Errors}",
                successCount, errorCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in ReportGenerationJob");
            throw;
        }
    }
}
```

**Register in Program.cs:**
```csharp
RecurringJob.AddOrUpdate<ReportGenerationJob>(
    "generate-reports",
    job => job.ExecuteAsync(),
    "0 2 * * *", // Daily at 2 AM
    new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });
```

### Day 37: DataCleanupJob

**File:** `src/GrcMvc/BackgroundJobs/DataCleanupJob.cs`

```csharp
using Hangfire;
using Microsoft.Extensions.Logging;
using GrcMvc.Data;
using Microsoft.EntityFrameworkCore;

namespace GrcMvc.BackgroundJobs;

public class DataCleanupJob
{
    private readonly GrcDbContext _context;
    private readonly ILogger<DataCleanupJob> _logger;

    public DataCleanupJob(
        GrcDbContext context,
        ILogger<DataCleanupJob> logger)
    {
        _context = context;
        _logger = logger;
    }

    [Hangfire.AutomaticRetry(Attempts = 2)]
    public async Task ExecuteAsync()
    {
        _logger.LogInformation("DataCleanupJob started at {Time}", DateTime.UtcNow);

        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-365); // Keep 1 year

            // Cleanup old audit logs
            var oldAuditLogs = await _context.AuditEvents
                .Where(a => a.EventTimestamp < cutoffDate)
                .ToListAsync();

            _context.AuditEvents.RemoveRange(oldAuditLogs);
            await _context.SaveChangesAsync();

            _logger.LogInformation("DataCleanupJob completed. Removed {Count} old records", oldAuditLogs.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DataCleanupJob");
            throw;
        }
    }
}
```

**Register in Program.cs:**
```csharp
RecurringJob.AddOrUpdate<DataCleanupJob>(
    "cleanup-data",
    job => job.ExecuteAsync(),
    Cron.Weekly(DayOfWeek.Sunday, 3), // Weekly on Sunday at 3 AM
    new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });
```

### Day 38: AuditLogJob

**File:** `src/GrcMvc/BackgroundJobs/AuditLogJob.cs`

```csharp
using Hangfire;
using Microsoft.Extensions.Logging;
using GrcMvc.Data;
using Microsoft.EntityFrameworkCore;

namespace GrcMvc.BackgroundJobs;

public class AuditLogJob
{
    private readonly GrcDbContext _context;
    private readonly ILogger<AuditLogJob> _logger;

    public AuditLogJob(
        GrcDbContext context,
        ILogger<AuditLogJob> logger)
    {
        _context = context;
        _logger = logger;
    }

    [Hangfire.AutomaticRetry(Attempts = 2)]
    public async Task ExecuteAsync()
    {
        _logger.LogInformation("AuditLogJob started at {Time}", DateTime.UtcNow);

        try
        {
            // Archive audit logs older than 90 days
            var archiveDate = DateTime.UtcNow.AddDays(-90);

            var logsToArchive = await _context.AuditEvents
                .Where(a => a.EventTimestamp < archiveDate && a.Status != "Archived")
                .ToListAsync();

            foreach (var log in logsToArchive)
            {
                log.Status = "Archived";
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("AuditLogJob completed. Archived {Count} logs", logsToArchive.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AuditLogJob");
            throw;
        }
    }
}
```

**Register in Program.cs:**
```csharp
RecurringJob.AddOrUpdate<AuditLogJob>(
    "archive-audit-logs",
    job => job.ExecuteAsync(),
    "0 3 * * *", // Daily at 3 AM
    new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });
```

**Register all jobs in Program.cs:**
```csharp
builder.Services.AddScoped<ReportGenerationJob>();
builder.Services.AddScoped<DataCleanupJob>();
builder.Services.AddScoped<AuditLogJob>();
```

---

## ✅ FINAL VERIFICATION CHECKLIST

### Policy Enforcement
- [ ] All files created
- [ ] Services registered
- [ ] YAML policy file exists
- [ ] Application compiles
- [ ] Application runs
- [ ] Policy enforcement tested

### Blazor Pages
- [ ] All 14 pages created
- [ ] Routes work
- [ ] Arabic localization complete
- [ ] CRUD operations work
- [ ] Responsive design verified

### Background Jobs
- [ ] All 3 jobs created
- [ ] Jobs registered in Hangfire
- [ ] Jobs appear in Hangfire dashboard
- [ ] Jobs execute on schedule

---

## 🚀 START IMPLEMENTATION NOW

**Command to begin:**
```bash
cd /home/dogan/grc-system/src/GrcMvc
dotnet add package YamlDotNet --version 15.1.4
dotnet build
```

**Then follow Day 1 steps above.**

---

**Status:** ✅ READY TO EXECUTE
**Next Action:** Start Day 1, Step 1.1
