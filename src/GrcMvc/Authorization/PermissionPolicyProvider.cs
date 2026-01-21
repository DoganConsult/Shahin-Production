using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace GrcMvc.Authorization;

/// <summary>
/// Dynamic authorization policy provider that creates policies on-demand for permission-based authorization.
/// Handles any permission pattern: [Authorize("Grc.Module.Action")] or [Authorize("Module.Action")]
/// without requiring manual policy registration for each permission.
/// </summary>
public sealed class PermissionPolicyProvider : DefaultAuthorizationPolicyProvider
{
    private readonly ConcurrentDictionary<string, AuthorizationPolicy> _cache = new();

    // Known static policies that should NOT be handled dynamically
    private static readonly HashSet<string> StaticPolicies = new(StringComparer.OrdinalIgnoreCase)
    {
        "AdminOnly",
        "ComplianceOfficer",
        "RiskManager",
        "Auditor",
        "ActivePlatformAdmin",
        "ActiveTenantAdmin"
    };

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options) : base(options)
    {
    }

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // Check if it's a static policy first
        if (StaticPolicies.Contains(policyName))
        {
            return await base.GetPolicyAsync(policyName);
        }

        // Check if it's a permission-style policy (contains a dot like "Module.Action" or "Grc.Module.Action")
        if (!IsPermissionPolicy(policyName))
        {
            return await base.GetPolicyAsync(policyName);
        }

        // Create and cache the policy for this permission
        var policy = _cache.GetOrAdd(policyName, name =>
            new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(name))
                .Build());

        return policy;
    }

    /// <summary>
    /// Determines if the policy name looks like a permission (contains dot separator)
    /// Examples: "Grc.Workflow.View", "Workflow.View", "Control.Edit"
    /// </summary>
    private static bool IsPermissionPolicy(string policyName)
    {
        // Must contain at least one dot and have content on both sides
        var dotIndex = policyName.IndexOf('.');
        return dotIndex > 0 && dotIndex < policyName.Length - 1;
    }
}
