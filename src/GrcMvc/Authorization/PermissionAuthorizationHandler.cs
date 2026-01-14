using GrcMvc.Services.Interfaces.RBAC;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace GrcMvc.Authorization;

/// <summary>
/// Authorization handler that checks if user has the required permission.
/// Checks in order: 1) Explicit denials, 2) Claims, 3) Database (RBAC), 4) Role-based fallback.
/// Supports both "Grc.Module.Action" and "Module.Action" permission formats.
/// </summary>
public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ILogger<PermissionAuthorizationHandler> _logger;
    private readonly IServiceProvider _serviceProvider;

    // Roles that have all permissions as a fallback (only after claims and database checks)
    private static readonly string[] SuperAdminRoles = ["PlatformAdmin"];
    private static readonly string[] TenantAdminRoles = ["Admin", "Owner"];

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
        var userName = context.User.Identity?.Name ?? "Unknown";

        // 1. Check for explicit denial claims first (security override)
        if (HasExplicitDenial(context.User, permission))
        {
            _logger.LogWarning("Permission explicitly denied. Permission={Permission}, User={User}", permission, userName);
            return; // Do not succeed - permission explicitly revoked
        }

        // 2. Check for permission claim (supports multiple claim types)
        if (CheckPermissionClaims(context.User, permission))
        {
            context.Succeed(requirement);
            _logger.LogDebug("Permission granted via claims. Permission={Permission}, User={User}", permission, userName);
            return;
        }

        // 3. Check database via RBAC service (proper permission management)
        if (!string.IsNullOrEmpty(userId))
        {
            var (hasPermission, isDenied) = await CheckDatabasePermissionAsync(userId, permission, context.User);

            if (isDenied)
            {
                _logger.LogWarning("Permission denied via RBAC database. Permission={Permission}, User={User}", permission, userName);
                return; // Explicit denial in database
            }

            if (hasPermission)
            {
                context.Succeed(requirement);
                _logger.LogDebug("Permission granted via RBAC database. Permission={Permission}, User={User}", permission, userName);
                return;
            }
        }

        // 4. Role-based fallback (ONLY after claims and database checks)
        // PlatformAdmin: Full platform access
        if (SuperAdminRoles.Any(role => context.User.IsInRole(role)))
        {
            context.Succeed(requirement);
            _logger.LogDebug("Permission granted via PlatformAdmin role fallback. Permission={Permission}, User={User}", permission, userName);
            return;
        }

        // Tenant Admin/Owner: Full tenant access (but check tenant context)
        if (TenantAdminRoles.Any(role => context.User.IsInRole(role)))
        {
            var tenantIdClaim = context.User.FindFirstValue("tenant_id") ?? context.User.FindFirstValue("TenantId");
            if (!string.IsNullOrEmpty(tenantIdClaim))
            {
                context.Succeed(requirement);
                _logger.LogDebug("Permission granted via {Role} role fallback within tenant. Permission={Permission}, User={User}, TenantId={TenantId}",
                    context.User.IsInRole("Admin") ? "Admin" : "Owner", permission, userName, tenantIdClaim);
                return;
            }
        }

        _logger.LogDebug("Permission check failed - no matching permission found. Permission={Permission}, User={User}", permission, userName);
    }

    /// <summary>
    /// Check for explicit denial claims (permission_denied, deny_permission)
    /// </summary>
    private bool HasExplicitDenial(ClaimsPrincipal user, string permission)
    {
        var permissionVariants = GetPermissionVariants(permission);

        return user.Claims.Any(c =>
            (c.Type == "permission_denied" || c.Type == "deny_permission") &&
            permissionVariants.Any(p => c.Value.Equals(p, StringComparison.OrdinalIgnoreCase)));
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
    /// Check permission in database via RBAC service.
    /// Returns a tuple of (hasPermission, isExplicitlyDenied).
    /// </summary>
    private async Task<(bool hasPermission, bool isDenied)> CheckDatabasePermissionAsync(
        string userId, string permission, ClaimsPrincipal user)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var permissionService = scope.ServiceProvider.GetService<IPermissionService>();

            if (permissionService == null)
            {
                _logger.LogDebug("RBAC PermissionService not available, skipping database check");
                return (false, false);
            }

            // Get tenant ID from claims
            var tenantIdClaim = user.FindFirstValue("tenant_id") ?? user.FindFirstValue("TenantId");
            if (!Guid.TryParse(tenantIdClaim, out var tenantId))
            {
                _logger.LogDebug("No valid tenant ID in claims, skipping database permission check");
                return (false, false);
            }

            // Check all permission variants
            var variants = GetPermissionVariants(permission);
            foreach (var variant in variants)
            {
                // Check for explicit denial first (if the service supports it)
                if (permissionService is IPermissionDenialService denialService)
                {
                    if (await denialService.IsPermissionDeniedAsync(userId, variant, tenantId))
                    {
                        _logger.LogDebug("Permission explicitly denied in database. Permission={Permission}, Variant={Variant}", permission, variant);
                        return (false, true);
                    }
                }

                // Check for granted permission
                if (await permissionService.HasPermissionAsync(userId, variant, tenantId))
                {
                    _logger.LogDebug("Permission granted via database. Permission={Permission}, Variant={Variant}", permission, variant);
                    return (true, false);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error checking permission in database. Permission={Permission}", permission);
        }

        return (false, false);
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

/// <summary>
/// Interface for services that support explicit permission denial checking.
/// Implement this alongside IPermissionService for fine-grained access control.
/// </summary>
public interface IPermissionDenialService
{
    /// <summary>
    /// Check if a permission is explicitly denied for a user.
    /// </summary>
    Task<bool> IsPermissionDeniedAsync(string userId, string permission, Guid tenantId);
}
