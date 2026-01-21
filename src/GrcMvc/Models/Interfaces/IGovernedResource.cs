namespace GrcMvc.Models.Interfaces;

/// <summary>
/// Interface for entities that are subject to governance and policy enforcement.
/// Entities implementing this interface can be evaluated against policy rules
/// defined in the policy engine (etc/policies/grc-baseline.yml).
/// </summary>
public interface IGovernedResource
{
    /// <summary>
    /// Resource type identifier for policy matching (e.g., "Evidence", "Assessment", "Risk")
    /// Maps to policy rule match.resource.type
    /// </summary>
    string ResourceType { get; }

    /// <summary>
    /// Owner of the resource (team or individual identifier)
    /// Required by REQUIRE_OWNER policy rule
    /// </summary>
    string? Owner { get; set; }

    /// <summary>
    /// Data classification level: public, internal, confidential, restricted
    /// Required by REQUIRE_DATA_CLASSIFICATION policy rule
    /// </summary>
    string? DataClassification { get; set; }

    /// <summary>
    /// Additional metadata labels for policy evaluation
    /// Used for custom policy rules and environment-specific conditions
    /// </summary>
    Dictionary<string, string> Labels { get; }
}
