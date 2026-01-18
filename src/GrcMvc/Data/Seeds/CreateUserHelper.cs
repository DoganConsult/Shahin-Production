using GrcMvc.Models.Entities;
using GrcMvc.Models.Entities.Catalogs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Data.Seeds;

/// <summary>
/// Helper class to create users programmatically
/// Usage: Can be called from a controller, API endpoint, or console command
/// </summary>
public static class CreateUserHelper
{
    /// <summary>
    /// Creates a new user in the system
    /// </summary>
    /// <param name="userManager">UserManager instance</param>
    /// <param name="context">GrcDbContext for tenant linkage</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="firstName">User's first name</param>
    /// <param name="lastName">User's last name</param>
    /// <param name="email">User's email (also used as username)</param>
    /// <param name="password">User's password (must meet password requirements)</param>
    /// <param name="department">User's department (optional)</param>
    /// <param name="jobTitle">User's job title (optional)</param>
    /// <param name="roleName">Role to assign (e.g., "TenantAdmin", "ComplianceManager") - optional</param>
    /// <param name="tenantId">Tenant ID to link user to (optional, will use default tenant if not provided)</param>
    /// <returns>Created ApplicationUser or null if creation failed</returns>
    public static async Task<ApplicationUser?> CreateUserAsync(
        UserManager<ApplicationUser> userManager,
        GrcDbContext context,
        ILogger logger,
        string firstName,
        string lastName,
        string email,
        string password,
        string? department = null,
        string? jobTitle = null,
        string? roleName = null,
        Guid? tenantId = null)
    {
        try
        {
            logger.LogInformation($"Creating user: {firstName} {lastName} ({email})");

            // Check if user already exists
            var existingUser = await userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                logger.LogWarning($"User already exists: {email}");
                return existingUser;
            }

            // Get or use default tenant
            Guid targetTenantId;
            if (tenantId.HasValue)
            {
                targetTenantId = tenantId.Value;
            }
            else
            {
                var defaultTenant = await context.Tenants
                    .FirstOrDefaultAsync(t => t.TenantSlug == "default" && !t.IsDeleted);
                if (defaultTenant == null)
                {
                    logger.LogError("Default tenant not found. Cannot create user without tenant.");
                    return null;
                }
                targetTenantId = defaultTenant.Id;
            }

            // Create user
            var user = new ApplicationUser
            {
                FirstName = firstName,
                LastName = lastName,
                Department = department ?? "General",
                JobTitle = jobTitle ?? "User",
                CreatedDate = DateTime.UtcNow,
                MustChangePassword = true // Force password change on first login for security
            };

            var createResult = await userManager.CreateAsync(user, password);
            
            if (createResult.Succeeded)
            {
                // Set UserName, Email, and confirm email using UserManager methods
                await userManager.SetUserNameAsync(user, email);
                await userManager.SetEmailAsync(user, email);
                var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                await userManager.ConfirmEmailAsync(user, token);
            }
            
            if (!createResult.Succeeded)
            {
                logger.LogError($"Failed to create user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                return null;
            }

            // Assign role if provided
            if (!string.IsNullOrEmpty(roleName))
            {
                try
                {
                    var role = await context.Set<Microsoft.AspNetCore.Identity.IdentityRole>()
                        .FirstOrDefaultAsync(r => r.Name == roleName);
                    if (role != null)
                    {
                        await userManager.AddToRoleAsync(user, roleName);
                        logger.LogInformation($"✅ Assigned role '{roleName}' to user {email}");
                    }
                    else
                    {
                        logger.LogWarning($"Role '{roleName}' not found. User created without role assignment.");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning($"Could not assign role '{roleName}': {ex.Message}");
                }
            }

            // Link user to tenant using the same pattern as UserSeeds
            try
            {
                var roleCode = await GetOrCreateRoleCodeAsync(context, "USER", "User", logger);
                var titleCode = await GetOrCreateTitleCodeAsync(context, "USER_TITLE", jobTitle ?? "User", logger);
                await LinkUserToTenantAsync(context, user.Id.ToString(), targetTenantId, roleCode, titleCode, logger);
            }
            catch (Exception ex)
            {
                logger.LogWarning($"Could not link user to tenant: {ex.Message}");
            }

            logger.LogInformation($"✅ User created successfully: {email}");
            return user;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error creating user {email}");
            return null;
        }
    }

    private static async Task<string> GetOrCreateRoleCodeAsync(
        GrcDbContext context,
        string roleCode,
        string roleName,
        ILogger logger)
    {
        var existingRole = await context.RoleCatalogs
            .FirstOrDefaultAsync(r => r.RoleCode == roleCode && r.IsActive);

        if (existingRole == null)
        {
            existingRole = new RoleCatalog
            {
                Id = Guid.NewGuid(),
                RoleCode = roleCode,
                RoleName = roleName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            context.RoleCatalogs.Add(existingRole);
            await context.SaveChangesAsync();
        }

        return existingRole.RoleCode;
    }

    private static async Task<string> GetOrCreateTitleCodeAsync(
        GrcDbContext context,
        string titleCode,
        string titleName,
        ILogger logger)
    {
        var existingTitle = await context.TitleCatalogs
            .FirstOrDefaultAsync(t => t.TitleCode == titleCode && t.IsActive);

        if (existingTitle == null)
        {
            // Get or create a default role catalog for titles
            var defaultRole = await context.RoleCatalogs
                .FirstOrDefaultAsync(r => r.RoleCode == "DEFAULT" || r.RoleName.Contains("Default"));

            if (defaultRole == null)
            {
                // Create a default role if none exists
                defaultRole = new RoleCatalog
                {
                    Id = Guid.NewGuid(),
                    RoleCode = "DEFAULT",
                    RoleName = "Default Role",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                context.RoleCatalogs.Add(defaultRole);
                await context.SaveChangesAsync();
            }

            existingTitle = new TitleCatalog
            {
                Id = Guid.NewGuid(),
                TitleCode = titleCode,
                TitleName = titleName,
                RoleCatalogId = defaultRole.Id, // Set required foreign key
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            context.TitleCatalogs.Add(existingTitle);
            await context.SaveChangesAsync();
        }

        return existingTitle.TitleCode;
    }

    private static async Task LinkUserToTenantAsync(
        GrcDbContext context,
        string userId,
        Guid tenantId,
        string roleCode,
        string titleCode,
        ILogger logger)
    {
        var existingLink = await context.Set<TenantUser>()
            .FirstOrDefaultAsync(tu => tu.UserId == userId && tu.TenantId == tenantId);

        if (existingLink == null)
        {
            var tenantUser = new TenantUser
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TenantId = tenantId,
                RoleCode = roleCode,
                TitleCode = titleCode,
                CreatedAt = DateTime.UtcNow
            };

            context.Set<TenantUser>().Add(tenantUser);
            await context.SaveChangesAsync();
            logger.LogInformation($"✅ Linked user to tenant");
        }
    }
}
