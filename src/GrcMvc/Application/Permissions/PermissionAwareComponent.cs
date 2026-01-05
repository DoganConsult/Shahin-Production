using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;

namespace GrcMvc.Application.Permissions;

/// <summary>
/// Base component for permission-aware UI elements
/// Provides consistent UX for permission checks
/// </summary>
public abstract class PermissionAwareComponent : ComponentBase
{
    [Inject] protected IHttpContextAccessor HttpContextAccessor { get; set; } = null!;
    [Inject] protected NavigationManager Navigation { get; set; } = null!;

    /// <summary>
    /// Check if user has permission
    /// </summary>
    protected bool HasPermission(string permission)
    {
        return PermissionHelper.HasPermission(HttpContextAccessor.HttpContext, permission);
    }

    /// <summary>
    /// Check if user has any of the permissions
    /// </summary>
    protected bool HasAnyPermission(params string[] permissions)
    {
        return PermissionHelper.HasAnyPermission(HttpContextAccessor.HttpContext, permissions);
    }

    /// <summary>
    /// Get permission tooltip message
    /// </summary>
    protected string GetPermissionTooltip(string permission, string actionName)
    {
        if (HasPermission(permission))
            return string.Empty;

        var permissionName = permission.Split('.').Last();
        return $"You need '{permissionName}' permission to {actionName}. Contact your administrator to request access.";
    }

    /// <summary>
    /// Get upgrade message for missing permission
    /// </summary>
    protected string GetUpgradeMessage(string requiredRole)
    {
        return $"Upgrade to {requiredRole} role to access this feature. Contact your administrator.";
    }

    /// <summary>
    /// Navigate to permission request page
    /// </summary>
    protected void RequestPermission(string permission)
    {
        Navigation.NavigateTo($"/admin/permissions/request?permission={permission}");
    }
}
