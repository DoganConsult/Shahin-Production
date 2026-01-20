using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Permission Service for checking user permissions
/// Integrates with ASP.NET Identity for role-based permission checking
/// </summary>
public class PermissionService : IPermissionService
{
    private readonly GrcDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<PermissionService> _logger;

    public PermissionService(
        GrcDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        ILogger<PermissionService> logger)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Check if user has a specific permission
    /// </summary>
    public async Task<bool> HasPermissionAsync(Guid userId, string permission)
    {
        try
        {
            // Get user
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null || !user.IsActive)
            {
                return false;
            }

            // Check custom permissions based on roles
            var hasPermission = await CheckCustomPermissionAsync(user, permission);

            _logger.LogDebug(
                "Permission check for user {UserId}, permission {Permission}: {Result}",
                userId, permission, hasPermission);

            return hasPermission;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission for user {UserId}, permission {Permission}", userId, permission);
            return false;
        }
    }

    /// <summary>
    /// Check if user has any of the specified permissions
    /// </summary>
    public async Task<bool> HasAnyPermissionAsync(Guid userId, params string[] permissions)
    {
        foreach (var permission in permissions)
        {
            if (await HasPermissionAsync(userId, permission))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Check if user has all specified permissions
    /// </summary>
    public async Task<bool> HasAllPermissionsAsync(Guid userId, params string[] permissions)
    {
        foreach (var permission in permissions)
        {
            if (!await HasPermissionAsync(userId, permission))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Get all permissions for a user
    /// </summary>
    public async Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null || !user.IsActive)
            {
                return Enumerable.Empty<string>();
            }

            var permissions = new HashSet<string>();

            // Get user roles and add permissions based on roles
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                var rolePermissions = GetPermissionsForRole(role);
                foreach (var permission in rolePermissions)
                {
                    permissions.Add(permission);
                }
            }

            // Add tenant-specific permissions if user has a tenant
            var userIdString = userId.ToString();
            var tenantUser = await _dbContext.TenantUsers
                .FirstOrDefaultAsync(tu => tu.UserId == userIdString && !tu.IsDeleted);

            if (tenantUser != null)
            {
                var tenantPermissions = GetTenantPermissions();
                foreach (var permission in tenantPermissions)
                {
                    permissions.Add(permission);
                }
            }

            return permissions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting permissions for user {UserId}", userId);
            return Enumerable.Empty<string>();
        }
    }

    /// <summary>
    /// Check custom permission based on user roles and context
    /// </summary>
    private async Task<bool> CheckCustomPermissionAsync(ApplicationUser user, string permission)
    {
        // Platform Admin has all permissions
        if (await _userManager.IsInRoleAsync(user, "PlatformAdmin"))
        {
            return true;
        }

        // Tenant Admin has tenant-specific permissions
        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            return IsTenantPermission(permission) || IsBasicPermission(permission) || IsGrcPermission(permission);
        }

        // Manager has read permissions and basic permissions
        if (await _userManager.IsInRoleAsync(user, "Manager"))
        {
            return IsManagerPermission(permission) || IsBasicPermission(permission);
        }

        // Regular users have basic permissions
        if (await _userManager.IsInRoleAsync(user, "User"))
        {
            return IsBasicPermission(permission);
        }

        return false;
    }

    /// <summary>
    /// Get permissions for a role name
    /// </summary>
    private static IEnumerable<string> GetPermissionsForRole(string roleName)
    {
        return roleName switch
        {
            "PlatformAdmin" => GetAllPermissions(),
            "Admin" => GetAdminPermissions(),
            "Manager" => GetManagerPermissions(),
            "User" => GetUserPermissions(),
            _ => Enumerable.Empty<string>()
        };
    }

    /// <summary>
    /// Get tenant-specific permissions
    /// </summary>
    private static IEnumerable<string> GetTenantPermissions()
    {
        return new[]
        {
            "tenant.view",
            "onboarding.view",
            "onboarding.update"
        };
    }

    // Permission category helpers
    private static bool IsTenantPermission(string permission) =>
        permission.StartsWith("tenant.") || permission.StartsWith("onboarding.");

    private static bool IsBasicPermission(string permission) =>
        permission.StartsWith("profile.") || permission.StartsWith("dashboard.");

    private static bool IsGrcPermission(string permission) =>
        permission.StartsWith("grc.");

    private static bool IsManagerPermission(string permission) =>
        permission.EndsWith(".read") || permission.EndsWith(".view");

    private static IEnumerable<string> GetAllPermissions() =>
        new[]
        {
            // Platform permissions
            "platform.admin",
            "platform.users.create",
            "platform.users.read",
            "platform.users.update",
            "platform.users.delete",
            "platform.tenants.create",
            "platform.tenants.read",
            "platform.tenants.update",
            "platform.tenants.delete",
            "platform.settings.read",
            "platform.settings.update",
            // Tenant permissions
            "tenant.view",
            "tenant.update",
            "tenant.users.create",
            "tenant.users.read",
            "tenant.users.update",
            "tenant.users.delete",
            // Onboarding permissions
            "onboarding.view",
            "onboarding.update",
            "onboarding.admin",
            // GRC permissions
            "grc.controls.read",
            "grc.controls.update",
            "grc.risks.read",
            "grc.risks.update",
            "grc.compliance.read",
            "grc.compliance.update",
            // Basic permissions
            "profile.view",
            "profile.update",
            "dashboard.view"
        };

    private static IEnumerable<string> GetAdminPermissions() =>
        new[]
        {
            "tenant.view",
            "tenant.update",
            "tenant.users.create",
            "tenant.users.read",
            "tenant.users.update",
            "tenant.users.delete",
            "onboarding.view",
            "onboarding.update",
            "onboarding.admin",
            "grc.controls.read",
            "grc.controls.update",
            "grc.risks.read",
            "grc.risks.update",
            "grc.compliance.read",
            "grc.compliance.update",
            "profile.view",
            "profile.update",
            "dashboard.view"
        };

    private static IEnumerable<string> GetManagerPermissions() =>
        new[]
        {
            "tenant.view",
            "tenant.users.read",
            "onboarding.view",
            "grc.controls.read",
            "grc.risks.read",
            "grc.compliance.read",
            "profile.view",
            "profile.update",
            "dashboard.view"
        };

    private static IEnumerable<string> GetUserPermissions() =>
        new[]
        {
            "profile.view",
            "profile.update",
            "dashboard.view"
        };
}
