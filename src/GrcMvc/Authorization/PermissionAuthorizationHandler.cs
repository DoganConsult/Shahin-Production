using GrcMvc.Services.Interfaces.RBAC;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace GrcMvc.Authorization;

/// <summary>
/// Authorization handler that checks if user has the required permission.
/// Checks in order: claims, database (RBAC), role-based fallback.
/// Supports both "Grc.Module.Action" and "Module.Action" permission formats.
/// </summary>
public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ILogger<PermissionAuthorizationHandler> _logger;
    private readonly IServiceProvider _serviceProvider;

    public PermissionAuthorizationHandler(
        ILogger<PermissionAuthorizationHandler> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            _logger.LogDebug("Permission check failed: user not authenticated. Permission={Permission}", requirement.Permission);
            return;
        }

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var permission = requirement.Permission;

        // 1. Check for permission claim (supports multiple claim types)
        var hasPermission = CheckPermissionClaims(context.User, permission);

        // 2. Fallback: Admin role has all permissions
        if (!hasPermission && context.User.IsInRole("Admin"))
        {
            hasPermission = true;
            _logger.LogDebug("Permission granted via Admin role. Permission={Permission}", permission);
        }

        // 3. Fallback: Owner role has all permissions
        if (!hasPermission && context.User.IsInRole("Owner"))
        {
            hasPermission = true;
            _logger.LogDebug("Permission granted via Owner role. Permission={Permission}", permission);
        }

        // 4. Fallback: PlatformAdmin role has all permissions
        if (!hasPermission && context.User.IsInRole("PlatformAdmin"))
        {
            hasPermission = true;
            _logger.LogDebug("Permission granted via PlatformAdmin role. Permission={Permission}", permission);
        }

        // 5. Check database via RBAC service if not found in claims
        if (!hasPermission && !string.IsNullOrEmpty(userId))
        {
            hasPermission = await CheckDatabasePermissionAsync(userId, permission, context.User);
        }

        if (hasPermission)
        {
            context.Succeed(requirement);
            _logger.LogDebug("Permission check passed. Permission={Permission}, User={User}",
                permission, context.User.Identity?.Name);
        }
        else
        {
            _logger.LogDebug("Permission check failed. Permission={Permission}, User={User}",
                permission, context.User.Identity?.Name);
        }
    }

    /// <summary>
    /// Check permission claims with support for multiple formats
    /// </summary>
    private bool CheckPermissionClaims(ClaimsPrincipal user, string permission)
    {
        // Normalize permission for comparison
        var permissionVariants = GetPermissionVariants(permission);

        return user.Claims.Any(c =>
            (c.Type == "permission" || c.Type == "permissions" || c.Type == "scope") &&
            permissionVariants.Any(p => c.Value.Equals(p, StringComparison.OrdinalIgnoreCase)));
    }

    /// <summary>
    /// Check permission in database via RBAC service
    /// </summary>
    private async Task<bool> CheckDatabasePermissionAsync(string userId, string permission, ClaimsPrincipal user)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var permissionService = scope.ServiceProvider.GetService<IPermissionService>();

            if (permissionService == null)
            {
                _logger.LogDebug("RBAC PermissionService not available, skipping database check");
                return false;
            }

            // Get tenant ID from claims
            var tenantIdClaim = user.FindFirstValue("tenant_id") ?? user.FindFirstValue("TenantId");
            if (!Guid.TryParse(tenantIdClaim, out var tenantId))
            {
                _logger.LogDebug("No valid tenant ID in claims, skipping database permission check");
                return false;
            }

            // Check all permission variants
            var variants = GetPermissionVariants(permission);
            foreach (var variant in variants)
            {
                if (await permissionService.HasPermissionAsync(userId, variant, tenantId))
                {
                    _logger.LogDebug("Permission granted via database. Permission={Permission}, Variant={Variant}", permission, variant);
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error checking permission in database. Permission={Permission}", permission);
        }

        return false;
    }

    /// <summary>
    /// Get all variants of a permission code for flexible matching.
    /// Handles "Grc.Module.Action" ↔ "Module.Action" conversion.
    /// </summary>
    private static List<string> GetPermissionVariants(string permission)
    {
        var variants = new List<string> { permission };

        if (permission.StartsWith("Grc.", StringComparison.OrdinalIgnoreCase))
        {
            // Add variant without "Grc." prefix
            variants.Add(permission[4..]);
        }
        else
        {
            // Add variant with "Grc." prefix
            variants.Add($"Grc.{permission}");
        }

        return variants;
    }
}
