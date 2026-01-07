using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace GrcMvc.Application.Permissions;

/// <summary>
/// Helper class for permission checks in controllers and services
/// </summary>
public static class PermissionHelper
{
    /// <summary>
    /// Check if current user has a specific permission
    /// </summary>
    public static bool HasPermission(HttpContext? httpContext, string permission)
    {
        if (httpContext == null || !(httpContext.User.Identity?.IsAuthenticated ?? false))
            return false;

        // Check if user has the permission claim
        return httpContext.User.HasClaim("Permission", permission) ||
               httpContext.User.IsInRole("Admin"); // Admin has all permissions
    }

    /// <summary>
    /// Check if current user has any of the specified permissions
    /// </summary>
    public static bool HasAnyPermission(HttpContext? httpContext, params string[] permissions)
    {
        if (httpContext == null || !(httpContext.User.Identity?.IsAuthenticated ?? false))
            return false;

        if (httpContext.User.IsInRole("Admin"))
            return true;

        foreach (var permission in permissions)
        {
            if (httpContext.User.HasClaim("Permission", permission))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Check if current user has all of the specified permissions
    /// </summary>
    public static bool HasAllPermissions(HttpContext? httpContext, params string[] permissions)
    {
        if (httpContext == null || !(httpContext.User.Identity?.IsAuthenticated ?? false))
            return false;

        if (httpContext.User.IsInRole("Admin"))
            return true;

        foreach (var permission in permissions)
        {
            if (!httpContext.User.HasClaim("Permission", permission))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Get all permissions for the current user
    /// </summary>
    public static IEnumerable<string> GetUserPermissions(HttpContext? httpContext)
    {
        if (httpContext == null || !(httpContext.User.Identity?.IsAuthenticated ?? false))
            return Enumerable.Empty<string>();

        return httpContext.User
            .FindAll("Permission")
            .Select(c => c.Value)
            .Distinct();
    }
}
