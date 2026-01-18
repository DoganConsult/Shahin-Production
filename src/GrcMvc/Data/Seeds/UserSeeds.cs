using GrcMvc.Models.Entities;
using GrcMvc.Models.Entities.Catalogs;
using GrcMvc.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Text.Json;

namespace GrcMvc.Data.Seeds;

/// <summary>
/// Seeds predefined users (Admin, Manager) and links them to the default tenant
/// </summary>
public static class UserSeeds
{
    public static async Task SeedUsersAsync(
        GrcDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger logger)
    {
        try
        {
            logger.LogInformation("🌱 Starting user seeding...");

            // Get default tenant
            var defaultTenant = await context.Tenants
                .FirstOrDefaultAsync(t => t.TenantSlug == "default" && !t.IsDeleted);

            if (defaultTenant == null)
            {
                logger.LogWarning("⚠️ Default tenant not found. Cannot seed users.");
                return;
            }

            var defaultTenantId = defaultTenant.Id;
            logger.LogInformation($"✅ Found default tenant: {defaultTenant.OrganizationName} (ID: {defaultTenantId})");

            // Seed Admin User
            await SeedAdminUserAsync(context, userManager, defaultTenantId, logger);

            // Seed Manager User
            await SeedManagerUserAsync(context, userManager, defaultTenantId, logger);

            logger.LogInformation("✅ User seeding completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error during user seeding");
            throw;
        }
    }

    private static async Task SeedAdminUserAsync(
        GrcDbContext context,
        UserManager<ApplicationUser> userManager,
        Guid tenantId,
        ILogger logger)
    {
        const string adminEmail = "support@shahin-ai.com";
        // CRITICAL SECURITY FIX: Use environment variable instead of hardcoded password
        var adminPassword = Environment.GetEnvironmentVariable("GRC_ADMIN_PASSWORD") 
            ?? throw new InvalidOperationException("GRC_ADMIN_PASSWORD environment variable is required for seeding admin user. Set it before running seeds.");

        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            logger.LogInformation("Creating admin user...");

            adminUser = new ApplicationUser
            {
                FirstName = "System",
                LastName = "Administrator",
                Department = "IT",
                JobTitle = "System Administrator",
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                MustChangePassword = true  // Force password change on first login
            };

            var createResult = await userManager.CreateAsync(adminUser, adminPassword);

            if (createResult.Succeeded)
            {
                // Set UserName, Email, and confirm email using UserManager methods
                await userManager.SetUserNameAsync(adminUser, adminEmail);
                await userManager.SetEmailAsync(adminUser, adminEmail);
                var token = await userManager.GenerateEmailConfirmationTokenAsync(adminUser);
                await userManager.ConfirmEmailAsync(adminUser, token);
            }

            if (!createResult.Succeeded)
            {
                logger.LogError($"Failed to create admin user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                return;
            }

            // Add to PlatformAdmin or TenantAdmin role if it exists
            try
            {
                var superAdminRole = await context.Set<Microsoft.AspNetCore.Identity.IdentityRole>()
                    .FirstOrDefaultAsync(r => r.Name == "PlatformAdmin");
                if (superAdminRole != null)
                {
                    var addRoleResult = await userManager.AddToRoleAsync(adminUser, "PlatformAdmin");
                }
                else
                {
                    var tenantAdminRole = await context.Set<Microsoft.AspNetCore.Identity.IdentityRole>()
                        .FirstOrDefaultAsync(r => r.Name == "TenantAdmin");
                    if (tenantAdminRole != null)
                    {
                        var addRoleResult = await userManager.AddToRoleAsync(adminUser, "TenantAdmin");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning($"Could not assign role to admin user: {ex.Message}");
                // Role might not exist yet, that's okay
            }

            logger.LogInformation($"✅ Admin user created: {adminEmail}");
        }
        else
        {
            logger.LogInformation($"Admin user already exists: {adminEmail}");
        }

        // Link admin to tenant - use generic role if specific role doesn't exist
        // LOW PRIORITY FIX: Use RoleConstants instead of magic string
        var adminRoleCode = await GetOrCreateRoleCodeAsync(context, RoleConstants.TenantAdmin, "Administrator", logger);
        var adminTitleCode = await GetOrCreateTitleCodeAsync(context, "SYSTEM_ADMINISTRATOR", "System Administrator", logger);
        await LinkUserToTenantAsync(context, adminUser.Id.ToString(), tenantId, adminRoleCode, adminTitleCode, logger);
    }

    private static async Task SeedManagerUserAsync(
        GrcDbContext context,
        UserManager<ApplicationUser> userManager,
        Guid tenantId,
        ILogger logger)
    {
        const string managerEmail = "manager@grcsystem.com";
        // CRITICAL SECURITY FIX: Use environment variable instead of hardcoded password
        var managerPassword = Environment.GetEnvironmentVariable("GRC_MANAGER_PASSWORD") 
            ?? throw new InvalidOperationException("GRC_MANAGER_PASSWORD environment variable is required for seeding manager user. Set it before running seeds.");

        var managerUser = await userManager.FindByEmailAsync(managerEmail);

        if (managerUser == null)
        {
            logger.LogInformation("Creating manager user...");

            managerUser = new ApplicationUser
            {
                FirstName = "Compliance",
                LastName = "Manager",
                Department = "Compliance",
                JobTitle = "Compliance Manager",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            var createResult = await userManager.CreateAsync(managerUser, managerPassword);

            if (createResult.Succeeded)
            {
                // Set UserName, Email, and confirm email using UserManager methods
                await userManager.SetUserNameAsync(managerUser, managerEmail);
                await userManager.SetEmailAsync(managerUser, managerEmail);
                var token = await userManager.GenerateEmailConfirmationTokenAsync(managerUser);
                await userManager.ConfirmEmailAsync(managerUser, token);
            }

            if (!createResult.Succeeded)
            {
                logger.LogError($"Failed to create manager user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                return;
            }

            // Add to ComplianceManager role if it exists
            try
            {
                var complianceManagerRole = await context.Set<Microsoft.AspNetCore.Identity.IdentityRole>()
                    .FirstOrDefaultAsync(r => r.Name == "ComplianceManager");
                if (complianceManagerRole != null)
                {
                    await userManager.AddToRoleAsync(managerUser, "ComplianceManager");
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning($"Could not assign role to manager user: {ex.Message}");
                // Role might not exist yet, that's okay
            }

            logger.LogInformation($"✅ Manager user created: {managerEmail}");
        }
        else
        {
            logger.LogInformation($"Manager user already exists: {managerEmail}");
        }

        // Link manager to tenant - use generic role if specific role doesn't exist
        var managerRoleCode = await GetOrCreateRoleCodeAsync(context, "COMPLIANCE_MANAGER", "Compliance Manager", logger);
        var managerTitleCode = await GetOrCreateTitleCodeAsync(context, "COMPLIANCE_MANAGER_TITLE", "Compliance Manager", logger);
        await LinkUserToTenantAsync(context, managerUser.Id.ToString(), tenantId, managerRoleCode, managerTitleCode, logger);
    }

    private static async Task<string> GetOrCreateRoleCodeAsync(
        GrcDbContext context,
        string roleCode,
        string roleName,
        ILogger logger)
    {
        var existingRole = await context.RoleCatalogs
            .FirstOrDefaultAsync(r => r.RoleCode == roleCode && r.IsActive);

        if (existingRole != null)
        {
            return roleCode;
        }

        // Create role if it doesn't exist
        logger.LogInformation($"Creating role {roleCode} in catalog...");
        var newRole = new RoleCatalog
        {
            Id = Guid.NewGuid(),
            RoleCode = roleCode,
            RoleName = roleName,
            IsActive = true,
            DisplayOrder = 999,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "System"
        };

        context.RoleCatalogs.Add(newRole);
        await context.SaveChangesAsync();

        logger.LogInformation($"✅ Created role {roleCode} in catalog");
        return roleCode;
    }

    private static async Task<string> GetOrCreateTitleCodeAsync(
        GrcDbContext context,
        string titleCode,
        string titleName,
        ILogger logger)
    {
        var existingTitle = await context.TitleCatalogs
            .FirstOrDefaultAsync(t => t.TitleCode == titleCode && t.IsActive);

        if (existingTitle != null)
        {
            return titleCode;
        }

        // Create title if it doesn't exist
        logger.LogInformation($"Creating title {titleCode} in catalog...");

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
                DisplayOrder = 999,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "System"
            };
            context.RoleCatalogs.Add(defaultRole);
            await context.SaveChangesAsync();
        }

        var newTitle = new TitleCatalog
        {
            Id = Guid.NewGuid(),
            TitleCode = titleCode,
            TitleName = titleName,
            RoleCatalogId = defaultRole.Id, // Set required foreign key
            IsActive = true,
            DisplayOrder = 999,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "System"
        };

        context.TitleCatalogs.Add(newTitle);
        await context.SaveChangesAsync();

        logger.LogInformation($"✅ Created title {titleCode} in catalog");
        return titleCode;
    }

    private static async Task LinkUserToTenantAsync(
        GrcDbContext context,
        string userId,
        Guid tenantId,
        string roleCode,
        string titleCode,
        ILogger logger)
    {
        // Verify user exists in database before linking (handles context synchronization)
        var userExists = await context.Set<ApplicationUser>().AnyAsync(u => u.Id.ToString() == userId);
        if (!userExists)
        {
            logger.LogWarning($"User {userId} not found in database. Skipping tenant link.");
            return;
        }

        // Check if TenantUser already exists
        var existingTenantUser = await context.TenantUsers
            .FirstOrDefaultAsync(tu => tu.TenantId == tenantId && tu.UserId == userId && !tu.IsDeleted);

        if (existingTenantUser != null)
        {
            logger.LogInformation($"User {userId} already linked to tenant {tenantId}");
            return;
        }

        var tenantUser = new TenantUser
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = userId,
            RoleCode = roleCode,
            TitleCode = titleCode ?? "",
            Status = "Active",
            InvitedAt = DateTime.UtcNow,
            ActivatedAt = DateTime.UtcNow,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "System"
        };

        context.TenantUsers.Add(tenantUser);
        await context.SaveChangesAsync();

        logger.LogInformation($"✅ User {userId} linked to tenant {tenantId} with role {roleCode}");
    }
}
