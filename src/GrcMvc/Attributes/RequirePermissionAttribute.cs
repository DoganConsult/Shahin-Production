using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using GrcMvc.Services.Interfaces;

namespace GrcMvc.Attributes;

/// <summary>
/// Custom authorization attribute for checking permissions
/// Usage: [RequirePermission("tenant.users.create")]
/// </summary>
public class RequirePermissionAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string[] _permissions;
    private readonly bool _requireAll;

    /// <summary>
    /// Require at least one of the specified permissions
    /// </summary>
    /// <param name="permissions">Permissions to check</param>
    public RequirePermissionAttribute(params string[] permissions)
    {
        _permissions = permissions ?? throw new ArgumentNullException(nameof(permissions));
        _requireAll = false;
    }

    /// <summary>
    /// Require all specified permissions
    /// </summary>
    /// <param name="requireAll">Whether to require all permissions</param>
    /// <param name="permissions">Permissions to check</param>
    public RequirePermissionAttribute(bool requireAll, params string[] permissions)
    {
        _permissions = permissions ?? throw new ArgumentNullException(nameof(permissions));
        _requireAll = requireAll;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // Skip if AllowAnonymous attribute is present
        if (context.ActionDescriptor.EndpointMetadata.Any(em => em is AllowAnonymousAttribute))
        {
            return;
        }

        // Get user ID from claims
        var userIdClaim = context.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) ??
                         context.HttpContext.User.FindFirst("sub");

        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Get permission service
        var permissionService = context.HttpContext.RequestServices.GetService<IPermissionService>();
        if (permissionService == null)
        {
            context.Result = new StatusCodeResult(500);
            return;
        }

        // Check permissions
        bool hasPermission;
        if (_requireAll)
        {
            hasPermission = await permissionService.HasAllPermissionsAsync(userId, _permissions);
        }
        else
        {
            hasPermission = await permissionService.HasAnyPermissionAsync(userId, _permissions);
        }

        if (!hasPermission)
        {
            context.Result = new ForbidResult();
        }
    }
}

/// <summary>
/// Require all specified permissions
/// Usage: [RequireAllPermissions("tenant.users.create", "tenant.users.update")]
/// </summary>
public class RequireAllPermissionsAttribute : RequirePermissionAttribute
{
    public RequireAllPermissionsAttribute(params string[] permissions)
        : base(true, permissions)
    {
    }
}

/// <summary>
/// Require tenant admin permissions
/// Usage: [RequireTenantAdmin]
/// </summary>
public class RequireTenantAdminAttribute : RequirePermissionAttribute
{
    public RequireTenantAdminAttribute()
        : base("tenant.view", "tenant.update", "tenant.users.read", "tenant.users.update")
    {
    }
}

/// <summary>
/// Require platform admin permissions
/// Usage: [RequirePlatformAdmin]
/// </summary>
public class RequirePlatformAdminAttribute : RequirePermissionAttribute
{
    public RequirePlatformAdminAttribute()
        : base("platform.admin")
    {
    }
}

/// <summary>
/// Require onboarding permissions
/// Usage: [RequireOnboardingAccess]
/// </summary>
public class RequireOnboardingAccessAttribute : RequirePermissionAttribute
{
    public RequireOnboardingAccessAttribute()
        : base("onboarding.view", "onboarding.update")
    {
    }
}
