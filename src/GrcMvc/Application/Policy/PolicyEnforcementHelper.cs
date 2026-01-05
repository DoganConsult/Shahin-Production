using GrcMvc.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace GrcMvc.Application.Policy;

/// <summary>
/// Helper class to simplify policy enforcement across all services
/// Provides consistent policy evaluation with automatic metadata extraction
/// </summary>
public class PolicyEnforcementHelper
{
    private readonly IPolicyEnforcer _policyEnforcer;
    private readonly ICurrentUserService _currentUser;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PolicyEnforcementHelper> _logger;

    public PolicyEnforcementHelper(
        IPolicyEnforcer policyEnforcer,
        ICurrentUserService currentUser,
        IConfiguration configuration,
        ILogger<PolicyEnforcementHelper> logger)
    {
        _policyEnforcer = policyEnforcer;
        _currentUser = currentUser;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Enforce policy for resource creation with automatic metadata extraction
    /// </summary>
    public async Task EnforceCreateAsync(
        string resourceType,
        object resource,
        string? dataClassification = null,
        string? owner = null,
        Dictionary<string, object>? additionalMetadata = null,
        CancellationToken ct = default)
    {
        await EnforceAsync("create", resourceType, resource, dataClassification, owner, additionalMetadata, ct);
    }

    /// <summary>
    /// Enforce policy for resource update
    /// </summary>
    public async Task EnforceUpdateAsync(
        string resourceType,
        object resource,
        string? dataClassification = null,
        string? owner = null,
        Dictionary<string, object>? additionalMetadata = null,
        CancellationToken ct = default)
    {
        await EnforceAsync("update", resourceType, resource, dataClassification, owner, additionalMetadata, ct);
    }

    /// <summary>
    /// Enforce policy for resource submission
    /// </summary>
    public async Task EnforceSubmitAsync(
        string resourceType,
        object resource,
        string? dataClassification = null,
        string? owner = null,
        Dictionary<string, object>? additionalMetadata = null,
        CancellationToken ct = default)
    {
        await EnforceAsync("submit", resourceType, resource, dataClassification, owner, additionalMetadata, ct);
    }

    /// <summary>
    /// Enforce policy for resource approval
    /// </summary>
    public async Task EnforceApproveAsync(
        string resourceType,
        object resource,
        string? dataClassification = null,
        string? owner = null,
        Dictionary<string, object>? additionalMetadata = null,
        CancellationToken ct = default)
    {
        await EnforceAsync("approve", resourceType, resource, dataClassification, owner, additionalMetadata, ct);
    }

    /// <summary>
    /// Enforce policy for resource publish
    /// </summary>
    public async Task EnforcePublishAsync(
        string resourceType,
        object resource,
        string? dataClassification = null,
        string? owner = null,
        Dictionary<string, object>? additionalMetadata = null,
        CancellationToken ct = default)
    {
        await EnforceAsync("publish", resourceType, resource, dataClassification, owner, additionalMetadata, ct);
    }

    /// <summary>
    /// Core enforcement method with automatic metadata wrapper creation
    /// </summary>
    private async Task EnforceAsync(
        string action,
        string resourceType,
        object resource,
        string? dataClassification,
        string? owner,
        Dictionary<string, object>? additionalMetadata,
        CancellationToken ct)
    {
        try
        {
            // Create policy evaluation wrapper with metadata
            var policyResource = CreatePolicyResource(resource, dataClassification, owner, additionalMetadata);

            var tenantId = _currentUser.GetTenantId();
            var userId = _currentUser.GetUserId().ToString();
            var userRoles = _currentUser.GetRoles();

            var context = new PolicyContext
            {
                Action = action,
                Environment = _configuration["Environment"] ?? "dev",
                ResourceType = resourceType,
                Resource = policyResource,
                TenantId = tenantId,
                PrincipalId = userId,
                PrincipalRoles = userRoles.ToList(), // Convert to IReadOnlyList
                CorrelationId = Guid.NewGuid().ToString(),
                Metadata = additionalMetadata ?? new Dictionary<string, object>()
            };

            await _policyEnforcer.EnforceAsync(context, ct);
        }
        catch (PolicyViolationException)
        {
            // Re-throw policy violations as-is
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enforcing policy for {Action} on {ResourceType}", action, resourceType);
            throw;
        }
    }

    /// <summary>
    /// Create a policy evaluation resource wrapper with metadata
    /// </summary>
    private object CreatePolicyResource(
        object resource,
        string? dataClassification,
        string? owner,
        Dictionary<string, object>? additionalMetadata)
    {
        // Extract ID and title from resource if possible
        var resourceId = ExtractProperty(resource, "Id") ?? Guid.NewGuid();
        var resourceTitle = ExtractProperty(resource, "Title") ?? ExtractProperty(resource, "Name") ?? "Unknown";

        // Build metadata labels
        var labels = new Dictionary<string, string>
        {
            ["dataClassification"] = dataClassification ?? "internal",
            ["owner"] = owner ?? _currentUser.GetUserName() ?? "unknown"
        };

        // Add additional labels from metadata if present
        if (additionalMetadata != null && additionalMetadata.ContainsKey("labels"))
        {
            if (additionalMetadata["labels"] is Dictionary<string, string> additionalLabels)
            {
                foreach (var label in additionalLabels)
                {
                    labels[label.Key] = label.Value;
                }
            }
        }

        // Create mutable policy resource wrapper (allows mutations to work)
        return new PolicyResourceWrapper
        {
            Id = resourceId,
            Title = resourceTitle?.ToString() ?? "Unknown",
            Type = ExtractProperty(resource, "Type")?.ToString() ?? "Unknown",
            Metadata = new PolicyResourceMetadata
            {
                Labels = labels,
                Additional = additionalMetadata ?? new Dictionary<string, object>()
            },
            Resource = resource // Include original resource for advanced path resolution
        };
    }

    /// <summary>
    /// Extract property value from object using reflection
    /// </summary>
    private object? ExtractProperty(object obj, string propertyName)
    {
        if (obj == null) return null;

        var type = obj.GetType();
        var prop = type.GetProperty(propertyName,
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.IgnoreCase);

        return prop?.GetValue(obj);
    }
}
