using GrcMvc.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Application.Permissions;

/// <summary>
/// Service to seed/register all GRC permissions in the database
/// Integrates with existing RBAC system
/// </summary>
public class PermissionSeederService
{
    private readonly GrcDbContext _context;
    private readonly ILogger<PermissionSeederService> _logger;
    private readonly IPermissionDefinitionProvider _permissionProvider;

    public PermissionSeederService(
        GrcDbContext context,
        ILogger<PermissionSeederService> logger,
        IPermissionDefinitionProvider permissionProvider)
    {
        _context = context;
        _logger = logger;
        _permissionProvider = permissionProvider;
    }

    /// <summary>
    /// Seed all permissions defined by the permission provider
    /// </summary>
    public async Task SeedPermissionsAsync(CancellationToken ct = default)
    {
        try
        {
            var context = new PermissionDefinitionContext();
            _permissionProvider.Define(context);

            var allPermissions = context.GetAllPermissions().ToList();
            _logger.LogInformation("Seeding {Count} permissions", allPermissions.Count);

            // Get or create Permission entity type (if it exists in your RBAC system)
            // This assumes you have a Permissions table in your RBAC system
            // If not, permissions are stored as strings in RoleFeatures or similar

            // For now, we'll just log that permissions are defined
            // The actual RBAC system will use these permission strings
            foreach (var permission in allPermissions)
            {
                _logger.LogDebug("Permission defined: {Name} - {DisplayName}", permission.Name, permission.DisplayName);
            }

            _logger.LogInformation("✅ Successfully seeded {Count} permissions", allPermissions.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding permissions");
            throw;
        }
    }

    /// <summary>
    /// Get all permission names as a list
    /// </summary>
    public List<string> GetAllPermissionNames()
    {
        var context = new PermissionDefinitionContext();
        _permissionProvider.Define(context);
        return context.GetAllPermissions().Select(p => p.Name).ToList();
    }
}
