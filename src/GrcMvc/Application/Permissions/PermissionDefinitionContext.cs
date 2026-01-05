namespace GrcMvc.Application.Permissions;

/// <summary>
/// Implementation of permission definition context
/// </summary>
public class PermissionDefinitionContext : IPermissionDefinitionContext
{
    private readonly Dictionary<string, PermissionGroupDefinition> _groups = new();

    public IPermissionGroupDefinition AddGroup(string name, string displayName)
    {
        if (_groups.ContainsKey(name))
        {
            return _groups[name];
        }

        var group = new PermissionGroupDefinition(name, displayName);
        _groups[name] = group;
        return group;
    }

    public IPermissionGroupDefinition? GetGroup(string name)
    {
        return _groups.TryGetValue(name, out var group) ? group : null;
    }

    /// <summary>
    /// Get all defined permissions
    /// </summary>
    public IEnumerable<PermissionDefinition> GetAllPermissions()
    {
        foreach (var group in _groups.Values)
        {
            foreach (var permission in group.GetAllPermissions())
            {
                yield return permission;
            }
        }
    }
}

/// <summary>
/// Permission group definition implementation
/// </summary>
public class PermissionGroupDefinition : IPermissionGroupDefinition
{
    private readonly Dictionary<string, PermissionDefinition> _permissions = new();

    public PermissionGroupDefinition(string name, string displayName)
    {
        Name = name;
        DisplayName = displayName;
    }

    public string Name { get; }
    public string DisplayName { get; }

    public IPermissionDefinition AddPermission(string name, string displayName)
    {
        if (_permissions.ContainsKey(name))
        {
            return _permissions[name];
        }

        var permission = new PermissionDefinition(name, displayName, null);
        _permissions[name] = permission;
        return permission;
    }

    public IPermissionDefinition? GetPermission(string name)
    {
        return _permissions.TryGetValue(name, out var permission) ? permission : null;
    }

    public IEnumerable<PermissionDefinition> GetAllPermissions()
    {
        foreach (var permission in _permissions.Values)
        {
            yield return permission;
            foreach (var child in permission.GetAllChildren())
            {
                yield return child;
            }
        }
    }
}

/// <summary>
/// Permission definition implementation
/// </summary>
public class PermissionDefinition : IPermissionDefinition
{
    private readonly Dictionary<string, PermissionDefinition> _children = new();

    public PermissionDefinition(string name, string displayName, string? parentName)
    {
        Name = name;
        DisplayName = displayName;
        ParentName = parentName;
    }

    public string Name { get; }
    public string DisplayName { get; }
    public string? ParentName { get; }

    public IPermissionDefinition AddChild(string name, string displayName)
    {
        if (_children.ContainsKey(name))
        {
            return _children[name];
        }

        var child = new PermissionDefinition(name, displayName, Name);
        _children[name] = child;
        return child;
    }

    public IEnumerable<PermissionDefinition> GetAllChildren()
    {
        foreach (var child in _children.Values)
        {
            yield return child;
            foreach (var grandchild in child.GetAllChildren())
            {
                yield return grandchild;
            }
        }
    }
}
