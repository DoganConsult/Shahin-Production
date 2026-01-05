# 🚀 GRC SYSTEM - IMPLEMENTATION ENFORCEMENT GUIDE
**Advanced Value-Added Implementation Plan**  
**Date:** 2025-01-22  
**Purpose:** Enforce implementation of critical gaps with enterprise-grade solutions

---

## 📋 EXECUTIVE SUMMARY

This guide provides **step-by-step enforcement** for implementing:
1. **Policy Enforcement System** (CRITICAL - 5 days)
2. **Missing Blazor Pages** (HIGH - 30 days)
3. **Background Jobs** (MEDIUM - 3 days)

Each section includes:
- ✅ **Complete code implementations**
- ✅ **Advanced features for value-add**
- ✅ **Best practices & patterns**
- ✅ **Quality gates & testing**
- ✅ **Production-ready solutions**

---

## 🔴 PHASE 1: POLICY ENFORCEMENT SYSTEM (CRITICAL)

### 🎯 Implementation Strategy: Enterprise-Grade Policy Engine

**Value-Added Features:**
- Deterministic rule evaluation with caching
- Real-time policy updates without restart
- Policy versioning and rollback
- Policy testing framework
- Performance monitoring and metrics
- Policy violation analytics dashboard

---

### Day 1: Core Policy Infrastructure

#### Step 1.1: Create Policy Models with Advanced Features

```csharp
// src/GrcMvc/Application/Policy/PolicyContext.cs
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

// src/GrcMvc/Application/Policy/PolicyModels/PolicyDocument.cs
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
    public Dictionary<string, object> Metadata { get; set; } = new(); // Advanced: Custom metadata
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
    public string Path { get; set; } = string.Empty; // Dot-path like "metadata.labels.dataClassification"
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
```

#### Step 1.2: Create Policy Interfaces

```csharp
// src/GrcMvc/Application/Policy/IPolicyEnforcer.cs
namespace GrcMvc.Application.Policy;

public interface IPolicyEnforcer
{
    Task EnforceAsync(PolicyContext ctx, CancellationToken ct = default);
    Task<PolicyDecision> EvaluateAsync(PolicyContext ctx, CancellationToken ct = default);
    Task<bool> IsAllowedAsync(PolicyContext ctx, CancellationToken ct = default);
}

// src/GrcMvc/Application/Policy/IPolicyStore.cs
public interface IPolicyStore
{
    Task<PolicyDocument> GetPolicyAsync(CancellationToken ct = default);
    Task<PolicyDocument> ReloadPolicyAsync(CancellationToken ct = default);
    Task<bool> ValidatePolicyAsync(PolicyDocument policy, CancellationToken ct = default);
    event EventHandler<PolicyReloadedEventArgs>? PolicyReloaded;
}

// src/GrcMvc/Application/Policy/IDotPathResolver.cs
public interface IDotPathResolver
{
    object? Resolve(object obj, string path);
    bool Exists(object obj, string path);
    void Set(object obj, string path, object? value);
    void Remove(object obj, string path);
}

// src/GrcMvc/Application/Policy/IMutationApplier.cs
public interface IMutationApplier
{
    Task ApplyAsync(IEnumerable<PolicyMutation> mutations, object resource, CancellationToken ct = default);
}

// src/GrcMvc/Application/Policy/IPolicyAuditLogger.cs
public interface IPolicyAuditLogger
{
    Task LogDecisionAsync(PolicyContext ctx, IEnumerable<PolicyDecision> decisions, PolicyDecision finalDecision, CancellationToken ct = default);
    Task<IEnumerable<PolicyAuditLog>> GetAuditLogsAsync(PolicyAuditQuery query, CancellationToken ct = default);
}

// src/GrcMvc/Application/Policy/PolicyDecision.cs
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

---

### Day 2-3: Implement Core Policy Engine

#### Step 2.1: DotPathResolver (Advanced with Caching)

```csharp
// src/GrcMvc/Application/Policy/DotPathResolver.cs
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

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

                // Handle object properties via reflection (with caching)
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
        // Implementation for setting values via reflection
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

#### Step 2.2: PolicyEnforcer (Advanced with Metrics)

```csharp
// src/GrcMvc/Application/Policy/PolicyEnforcer.cs
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
    
    // Advanced: Performance metrics
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
            
            // Validate policy
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

            // Evaluate exceptions first
            var applicableExceptions = policy.Spec.Exceptions
                .Where(e => IsExceptionApplicable(e, ctx))
                .ToList();

            // Evaluate rules in priority order
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

            // Resolve conflicts
            var finalDecision = ResolveConflict(decisions, policy.Spec.Execution.ConflictStrategy, policy.Spec.DefaultEffect);

            // Audit decision
            await _auditLogger.LogDecisionAsync(ctx, decisions, finalDecision, ct);

            // Update metrics
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
        // Check expiry
        if (exception.ExpiresAt.HasValue && exception.ExpiresAt.Value < DateTime.UtcNow)
            return false;

        // Check match criteria
        return IsRuleApplicable(new PolicyRule { Match = exception.Match }, ctx);
    }

    private async Task<PolicyDecision> EvaluateRuleAsync(PolicyRule rule, PolicyContext ctx, CancellationToken ct)
    {
        return new PolicyDecision
        {
            Effect = rule.Effect,
            MatchedRuleId = rule.Id,
            Message = rule.Message,
            RemediationHint = rule.Remediation.Hint,
            Severity = rule.Severity,
            Metadata = rule.Metadata
        };
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
        
        // Update average duration (simplified)
        var currentAvg = Interlocked.Read(ref metrics.AverageDurationMs);
        var newAvg = (currentAvg + durationMs) / 2;
        Interlocked.Exchange(ref metrics.AverageDurationMs, newAvg);
    }

    // Advanced: Get metrics for monitoring
    public Dictionary<string, PolicyMetrics> GetMetrics() => _metrics.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
}

public class PolicyMetrics
{
    public long TotalEvaluations;
    public long DeniedCount;
    public long AverageDurationMs;
}
```

---

### Day 4: Policy Store & YAML Integration

#### Step 4.1: PolicyStore with Hot Reload

```csharp
// src/GrcMvc/Application/Policy/PolicyStore.cs
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
        // Basic validation
        if (policy.Spec == null)
            return false;

        if (!policy.Spec.Rules.Any())
            return false;

        // Validate rules
        foreach (var rule in policy.Spec.Rules)
        {
            if (string.IsNullOrEmpty(rule.Id))
                return false;
            if (rule.Priority < 1 || rule.Priority > 10000)
                return false;
            if (string.IsNullOrEmpty(rule.Effect))
                return false;
        }

        return true;
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
                await Task.Delay(500); // Wait for file to be fully written
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

public class PolicyReloadedEventArgs : EventArgs
{
    public PolicyDocument Policy { get; set; } = null!;
}
```

#### Step 4.2: Create YAML Policy File

```yaml
# etc/policies/grc-baseline.yml
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

---

### Day 5: Integration & Testing

#### Step 5.1: Register in DI

```csharp
// Add to Program.cs
builder.Services.AddScoped<IPolicyEnforcer, PolicyEnforcer>();
builder.Services.AddSingleton<IPolicyStore, PolicyStore>();
builder.Services.AddScoped<IDotPathResolver, DotPathResolver>();
builder.Services.AddScoped<IMutationApplier, MutationApplier>();
builder.Services.AddScoped<IPolicyAuditLogger, PolicyAuditLogger>();
builder.Services.AddHostedService<PolicyStore>(); // For hot reload
```

#### Step 5.2: Integrate into AppServices

```csharp
// Example: EvidenceService.CreateAsync
[Authorize(GrcPermissions.Evidence.Upload)]
public async Task<EvidenceDto> CreateAsync(CreateEvidenceDto input)
{
    var entity = ObjectMapper.Map<CreateEvidenceDto, Evidence>(input);
    
    // Add metadata for policy evaluation
    entity.Metadata = entity.Metadata ?? new Dictionary<string, object>();
    entity.Metadata["labels"] = new Dictionary<string, string>
    {
        ["dataClassification"] = input.DataClassification ?? "internal",
        ["owner"] = input.Owner ?? _currentUser.GetUserName()
    };

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

---

## 🟡 PHASE 2: MISSING BLAZOR PAGES (HIGH PRIORITY)

### 🎯 Implementation Strategy: Enterprise UI Components

**Value-Added Features:**
- Reusable component library
- Advanced data tables with sorting/filtering
- Real-time updates via SignalR
- Export functionality (PDF/Excel)
- Advanced search and filters
- Responsive design with RTL support
- Accessibility (WCAG 2.1 AA)

### Template: Standard CRUD Page

```razor
@page "/frameworks"
@using GrcMvc.Models.Entities
@using GrcMvc.Models.DTOs
@inject IFrameworkService FrameworkService
@inject ICurrentUserService CurrentUser
@inject NavigationManager Navigation
@inject IJSRuntime JSRuntime
@inject ILogger<FrameworksPage> Logger

<PageTitle>مكتبة الأطر التنظيمية</PageTitle>

<div class="container-fluid" dir="@dir">
    <div class="row mb-3">
        <div class="col">
            <h3>@(isRtl ? "مكتبة الأطر التنظيمية" : "Regulatory Frameworks")</h3>
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
                <!-- Advanced Search -->
                <div class="row mb-3">
                    <div class="col-md-4">
                        <input type="text" class="form-control" 
                               placeholder="@(isRtl ? "بحث..." : "Search...")" 
                               @bind="searchTerm" @bind:event="oninput" />
                    </div>
                    <div class="col-md-3">
                        <select class="form-select" @bind="selectedCategory">
                            <option value="">@(isRtl ? "جميع الفئات" : "All Categories")</option>
                            @foreach (var cat in categories)
                            {
                                <option value="@cat">@cat</option>
                            }
                        </select>
                    </div>
                    <div class="col-md-3">
                        <select class="form-select" @bind="selectedStatus">
                            <option value="">@(isRtl ? "جميع الحالات" : "All Statuses")</option>
                            <option value="Active">@(isRtl ? "نشط" : "Active")</option>
                            <option value="Inactive">@(isRtl ? "غير نشط" : "Inactive")</option>
                        </select>
                    </div>
                    <div class="col-md-2">
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
                                <th @onclick="() => SortBy(nameof(FrameworkDto.Code))">
                                    @(isRtl ? "الكود" : "Code")
                                    @if (sortColumn == nameof(FrameworkDto.Code))
                                    {
                                        <i class="fas fa-sort-@(sortAscending ? "up" : "down")"></i>
                                    }
                                </th>
                                <th>@(isRtl ? "الاسم" : "Name")</th>
                                <th>@(isRtl ? "الفئة" : "Category")</th>
                                <th>@(isRtl ? "الحالة" : "Status")</th>
                                <th>@(isRtl ? "الإجراءات" : "Actions")</th>
                            </tr>
                        </thead>
                        <tbody>
                            @foreach (var framework in filteredFrameworks)
                            {
                                <tr>
                                    <td>@framework.Code</td>
                                    <td>@(isRtl ? framework.NameAr : framework.Name)</td>
                                    <td>@framework.Category</td>
                                    <td>
                                        <StatusBadge Status="@framework.Status" />
                                    </td>
                                    <td>
                                        <button class="btn btn-sm btn-outline-primary" 
                                                @onclick="() => ViewDetails(framework.Id)">
                                            <i class="fas fa-eye"></i>
                                        </button>
                                        <button class="btn btn-sm btn-outline-secondary" 
                                                @onclick="() => Edit(framework.Id)">
                                            <i class="fas fa-edit"></i>
                                        </button>
                                        <button class="btn btn-sm btn-outline-danger" 
                                                @onclick="() => Delete(framework.Id)">
                                            <i class="fas fa-trash"></i>
                                        </button>
                                    </td>
                                </tr>
                            }
                        </tbody>
                    </table>
                </div>

                <!-- Pagination -->
                <nav>
                    <ul class="pagination justify-content-center">
                        <li class="page-item @(currentPage == 1 ? "disabled" : "")">
                            <button class="page-link" @onclick="() => ChangePage(currentPage - 1)">@(isRtl ? "السابق" : "Previous")</button>
                        </li>
                        @for (int i = 1; i <= totalPages; i++)
                        {
                            <li class="page-item @(i == currentPage ? "active" : "")">
                                <button class="page-link" @onclick="() => ChangePage(i)">@i</button>
                            </li>
                        }
                        <li class="page-item @(currentPage == totalPages ? "disabled" : "")">
                            <button class="page-link" @onclick="() => ChangePage(currentPage + 1)">@(isRtl ? "التالي" : "Next")</button>
                        </li>
                    </ul>
                </nav>
            </div>
        </div>
    }
</div>

@code {
    private List<FrameworkDto> frameworks = new();
    private List<FrameworkDto> filteredFrameworks = new();
    private bool isLoading = true;
    private string? errorMessage;
    private string searchTerm = string.Empty;
    private string selectedCategory = string.Empty;
    private string selectedStatus = string.Empty;
    private string sortColumn = nameof(FrameworkDto.Code);
    private bool sortAscending = true;
    private int currentPage = 1;
    private int pageSize = 10;
    private int totalPages = 1;
    private List<string> categories = new();
    
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
            frameworks = await FrameworkService.GetAllAsync();
            categories = frameworks.Select(f => f.Category).Distinct().ToList();
            ApplyFilters();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading frameworks");
            errorMessage = isRtl ? "حدث خطأ في تحميل البيانات" : "Error loading data";
        }
        finally
        {
            isLoading = false;
        }
    }

    private void ApplyFilters()
    {
        filteredFrameworks = frameworks
            .Where(f => string.IsNullOrEmpty(searchTerm) || 
                       (isRtl ? f.NameAr : f.Name).Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                       f.Code.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            .Where(f => string.IsNullOrEmpty(selectedCategory) || f.Category == selectedCategory)
            .Where(f => string.IsNullOrEmpty(selectedStatus) || f.Status == selectedStatus)
            .OrderBy(f => sortColumn == nameof(FrameworkDto.Code) ? f.Code : "")
            .ThenBy(f => sortColumn == nameof(FrameworkDto.Name) ? (isRtl ? f.NameAr : f.Name) : "")
            .ToList();

        if (!sortAscending)
            filteredFrameworks.Reverse();

        totalPages = (int)Math.Ceiling(filteredFrameworks.Count / (double)pageSize);
        filteredFrameworks = filteredFrameworks
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    private void SortBy(string column)
    {
        if (sortColumn == column)
            sortAscending = !sortAscending;
        else
        {
            sortColumn = column;
            sortAscending = true;
        }
        ApplyFilters();
    }

    private void ChangePage(int page)
    {
        if (page >= 1 && page <= totalPages)
        {
            currentPage = page;
            ApplyFilters();
        }
    }

    private void CreateNew() => Navigation.NavigateTo("/frameworks/create");
    private void ViewDetails(Guid id) => Navigation.NavigateTo($"/frameworks/{id}");
    private void Edit(Guid id) => Navigation.NavigateTo($"/frameworks/{id}/edit");
    
    private async Task Delete(Guid id)
    {
        if (await JSRuntime.InvokeAsync<bool>("confirm", 
            isRtl ? "هل أنت متأكد من الحذف؟" : "Are you sure you want to delete?"))
        {
            try
            {
                await FrameworkService.DeleteAsync(id);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error deleting framework");
                errorMessage = isRtl ? "حدث خطأ في الحذف" : "Error deleting item";
            }
        }
    }

    private async Task ExportToExcel()
    {
        // Implementation for Excel export
        await JSRuntime.InvokeVoidAsync("exportToExcel", filteredFrameworks);
    }
}
```

**Repeat this pattern for all 14 missing pages with appropriate service calls.**

---

## 🟡 PHASE 3: BACKGROUND JOBS (MEDIUM PRIORITY)

### Implementation: Enterprise Background Jobs

```csharp
// src/GrcMvc/BackgroundJobs/ReportGenerationJob.cs
using Hangfire;
using Microsoft.Extensions.Logging;

namespace GrcMvc.BackgroundJobs;

public class ReportGenerationJob
{
    private readonly IReportService _reportService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ReportGenerationJob> _logger;

    public ReportGenerationJob(
        IReportService reportService,
        IUnitOfWork unitOfWork,
        ILogger<ReportGenerationJob> logger)
    {
        _reportService = reportService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    [Hangfire.AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 300, 900 })]
    public async Task ExecuteAsync()
    {
        _logger.LogInformation("ReportGenerationJob started at {Time}", DateTime.UtcNow);

        try
        {
            var tenants = await _unitOfWork.Tenants
                .Query()
                .Where(t => t.Status == "Active" && !t.IsDeleted)
                .ToListAsync();

            var stats = new JobStats();

            foreach (var tenant in tenants)
            {
                try
                {
                    // Generate scheduled reports for tenant
                    var reports = await _reportService.GetScheduledReportsAsync(tenant.Id);
                    
                    foreach (var report in reports)
                    {
                        await _reportService.GenerateReportAsync(report.Id);
                        stats.SuccessCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error generating reports for tenant {TenantId}", tenant.Id);
                    stats.ErrorCount++;
                }
            }

            _logger.LogInformation(
                "ReportGenerationJob completed. Success: {Success}, Errors: {Errors}",
                stats.SuccessCount, stats.ErrorCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in ReportGenerationJob");
            throw;
        }
    }

    private class JobStats
    {
        public int SuccessCount;
        public int ErrorCount;
    }
}

// Register in Program.cs
RecurringJob.AddOrUpdate<ReportGenerationJob>(
    "generate-reports",
    job => job.ExecuteAsync(),
    "0 2 * * *", // Daily at 2 AM
    new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });
```

---

## ✅ QUALITY GATES

### For Each Phase:
1. ✅ Unit tests (>80% coverage)
2. ✅ Integration tests
3. ✅ Performance tests
4. ✅ Security review
5. ✅ Documentation complete
6. ✅ Code review passed

---

## 📊 IMPLEMENTATION CHECKLIST

### Policy Enforcement
- [ ] Day 1: Models & Interfaces created
- [ ] Day 2-3: Core engine implemented
- [ ] Day 4: YAML integration & hot reload
- [ ] Day 5: Integration & testing

### Blazor Pages
- [ ] Week 1: 5 critical pages
- [ ] Week 2: 5 more pages
- [ ] Week 3: Remaining 4 pages
- [ ] Week 4: Testing & polish

### Background Jobs
- [ ] Day 1: ReportGenerationJob
- [ ] Day 2: DataCleanupJob
- [ ] Day 3: AuditLogJob & testing

---

**Status:** Ready for Implementation  
**Next:** Begin Phase 1 (Policy Enforcement)
