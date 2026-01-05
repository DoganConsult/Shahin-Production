namespace GrcMvc.Application.Permissions;

/// <summary>
/// Provider interface for permission definitions
/// Adapts ABP-style permission system to ASP.NET Core Identity
/// </summary>
public interface IPermissionDefinitionProvider
{
    /// <summary>
    /// Defines all permissions in the system
    /// </summary>
    void Define(IPermissionDefinitionContext context);
}

/// <summary>
/// Context for permission definition
/// </summary>
public interface IPermissionDefinitionContext
{
    /// <summary>
    /// Add a permission group
    /// </summary>
    IPermissionGroupDefinition AddGroup(string name, string displayName);

    /// <summary>
    /// Get an existing group
    /// </summary>
    IPermissionGroupDefinition? GetGroup(string name);
}

/// <summary>
/// Permission group definition
/// </summary>
public interface IPermissionGroupDefinition
{
    string Name { get; }
    string DisplayName { get; }

    /// <summary>
    /// Add a permission to this group
    /// </summary>
    IPermissionDefinition AddPermission(string name, string displayName);

    /// <summary>
    /// Get an existing permission
    /// </summary>
    IPermissionDefinition? GetPermission(string name);
}

/// <summary>
/// Permission definition
/// </summary>
public interface IPermissionDefinition
{
    string Name { get; }
    string DisplayName { get; }
    string? ParentName { get; }

    /// <summary>
    /// Add a child permission
    /// </summary>
    IPermissionDefinition AddChild(string name, string displayName);
}
