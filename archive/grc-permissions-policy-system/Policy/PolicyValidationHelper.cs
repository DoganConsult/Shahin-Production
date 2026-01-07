using GrcMvc.Application.Policy.PolicyModels;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Application.Policy;

/// <summary>
/// Helper for proactive policy validation and user-friendly guidance
/// Enhances UX by validating before submission and providing clear feedback
/// </summary>
public class PolicyValidationHelper
{
    private readonly IPolicyStore _policyStore;
    private readonly IDotPathResolver _pathResolver;
    private readonly ILogger<PolicyValidationHelper> _logger;

    public PolicyValidationHelper(
        IPolicyStore policyStore,
        IDotPathResolver pathResolver,
        ILogger<PolicyValidationHelper> logger)
    {
        _policyStore = policyStore;
        _pathResolver = pathResolver;
        _logger = logger;
    }

    /// <summary>
    /// Validate resource against policies and return user-friendly validation results
    /// Use this for real-time validation in forms
    /// </summary>
    public async Task<PolicyValidationResult> ValidateAsync(
        string resourceType,
        object resource,
        string environment = "dev",
        CancellationToken ct = default)
    {
        var policy = await _policyStore.GetPolicyAsync(ct);
        var violations = new List<PolicyViolationInfo>();
        var suggestions = new List<PolicySuggestion>();

        // Check applicable rules
        var applicableRules = policy.Spec.Rules
            .Where(r => r.Enabled && 
                   (r.Match.Resource.Type == "*" || r.Match.Resource.Type == resourceType) &&
                   (r.Match.Environment == "*" || r.Match.Environment == environment))
            .OrderBy(r => r.Priority)
            .ToList();

        foreach (var rule in applicableRules)
        {
            // Check if rule matches
            if (!EvaluateRuleMatch(rule, resource))
                continue;

            // Check conditions
            var conditionResults = EvaluateConditions(rule.When, resource);
            var failedConditions = conditionResults.Where(c => !c.Passed).ToList();

            if (failedConditions.Any())
            {
                if (rule.Effect == "deny")
                {
                    violations.Add(new PolicyViolationInfo
                    {
                        RuleId = rule.Id,
                        Message = rule.Message,
                        RemediationHint = rule.Remediation.Hint,
                        Severity = rule.Severity,
                        FailedConditions = failedConditions.Select(c => c.Message).ToList()
                    });
                }
            }
            else if (rule.Effect == "mutate" && rule.Mutations.Any())
            {
                // Suggest mutations as improvements
                suggestions.Add(new PolicySuggestion
                {
                    RuleId = rule.Id,
                    Message = rule.Message,
                    SuggestedMutations = rule.Mutations.Select(m => new MutationSuggestion
                    {
                        Path = m.Path,
                        Operation = m.Op,
                        Value = m.Value?.ToString() ?? "null",
                        Reason = $"Policy rule '{rule.Id}' recommends this change"
                    }).ToList()
                });
            }
        }

        return new PolicyValidationResult
        {
            IsValid = !violations.Any(),
            Violations = violations,
            Suggestions = suggestions,
            CanAutoFix = suggestions.Any(s => s.SuggestedMutations.Any(m => m.Operation == "set"))
        };
    }

    /// <summary>
    /// Get smart defaults for a resource type based on user context
    /// </summary>
    public async Task<Dictionary<string, object>> GetSmartDefaultsAsync(
        string resourceType,
        string? userRole,
        string environment = "dev",
        CancellationToken ct = default)
    {
        var defaults = new Dictionary<string, object>();

        // Role-based defaults
        if (userRole == "ComplianceOfficer" || userRole == "Admin")
        {
            defaults["metadata.labels.dataClassification"] = "confidential";
        }
        else
        {
            defaults["metadata.labels.dataClassification"] = "internal";
        }

        // Environment-based defaults
        if (environment == "prod")
        {
            defaults["metadata.labels.approvedForProd"] = "false"; // Requires explicit approval
        }

        return defaults;
    }

    private bool EvaluateRuleMatch(PolicyRule rule, object resource)
    {
        // Simplified match evaluation
        return true; // For now, assume match if resource type matches
    }

    private List<ConditionResult> EvaluateConditions(List<PolicyCondition> conditions, object resource)
    {
        var results = new List<ConditionResult>();

        foreach (var condition in conditions)
        {
            var value = _pathResolver.Resolve(resource, condition.Path);
            var passed = false;
            var message = string.Empty;

            switch (condition.Op.ToLower())
            {
                case "exists":
                    passed = value != null;
                    message = passed ? "Field exists" : $"Field '{condition.Path}' is required";
                    break;

                case "equals":
                    passed = Equals(value, condition.Value);
                    message = passed 
                        ? $"Field '{condition.Path}' has correct value"
                        : $"Field '{condition.Path}' must equal '{condition.Value}'";
                    break;

                case "notmatches":
                    if (value?.ToString() is string str && condition.Value?.ToString() is string pattern)
                    {
                        passed = !System.Text.RegularExpressions.Regex.IsMatch(str, pattern);
                        message = passed
                            ? $"Field '{condition.Path}' matches required pattern"
                            : $"Field '{condition.Path}' must match pattern: {pattern}";
                    }
                    break;

                // Add more condition types as needed
            }

            results.Add(new ConditionResult
            {
                Condition = condition,
                Passed = passed,
                Message = message
            });
        }

        return results;
    }
}

/// <summary>
/// Result of policy validation with user-friendly information
/// </summary>
public class PolicyValidationResult
{
    public bool IsValid { get; set; }
    public List<PolicyViolationInfo> Violations { get; set; } = new();
    public List<PolicySuggestion> Suggestions { get; set; } = new();
    public bool CanAutoFix { get; set; }
}

/// <summary>
/// Information about a policy violation
/// </summary>
public class PolicyViolationInfo
{
    public string RuleId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? RemediationHint { get; set; }
    public string Severity { get; set; } = "medium";
    public List<string> FailedConditions { get; set; } = new();
}

/// <summary>
/// Policy suggestion for improvement
/// </summary>
public class PolicySuggestion
{
    public string RuleId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public List<MutationSuggestion> SuggestedMutations { get; set; } = new();
}

/// <summary>
/// Suggested mutation
/// </summary>
public class MutationSuggestion
{
    public string Path { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Result of condition evaluation
/// </summary>
public class ConditionResult
{
    public PolicyCondition Condition { get; set; } = null!;
    public bool Passed { get; set; }
    public string Message { get; set; } = string.Empty;
}
