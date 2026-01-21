using System.Collections.Generic;

namespace GrcMvc.Application.Policy;

/// <summary>
/// Mutable wrapper for policy evaluation - allows mutations to work
/// </summary>
public class PolicyResourceWrapper
{
    public object? Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public PolicyResourceMetadata Metadata { get; set; } = new();
    public object? Resource { get; set; } // Original resource for advanced path resolution
}

public class PolicyResourceMetadata
{
    public Dictionary<string, string> Labels { get; set; } = new();
    public Dictionary<string, object> Additional { get; set; } = new();
}
