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
