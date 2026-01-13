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
        // #region agent log
        try { await System.IO.File.AppendAllTextAsync("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "B", location = "UserSeeds.cs:14", message = "SeedUsersAsync entry", data = new { timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
        // #endregion
        try
        {
            logger.LogInformation("🌱 Starting user seeding...");

            // Get default tenant
            var defaultTenant = await context.Tenants
                .FirstOrDefaultAsync(t => t.TenantSlug == "default" && !t.IsDeleted);

            // #region agent log
            try { await System.IO.File.AppendAllTextAsync("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "B", location = "UserSeeds.cs:25", message = "SeedUsersAsync default tenant lookup", data = new { defaultTenantId = defaultTenant?.Id.ToString(), defaultTenantName = defaultTenant?.OrganizationName, isNull = defaultTenant == null, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
            // #endregion
            if (defaultTenant == null)
            {
                logger.LogWarning("⚠️ Default tenant not found. Cannot seed users.");
                // #region agent log
                try { await System.IO.File.AppendAllTextAsync("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "B", location = "UserSeeds.cs:29", message = "SeedUsersAsync default tenant is null - exiting", data = new { timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                // #endregion
                return;
            }

            var defaultTenantId = defaultTenant.Id;
            logger.LogInformation($"✅ Found default tenant: {defaultTenant.OrganizationName} (ID: {defaultTenantId})");

            // Seed Admin User
            // #region agent log
            try { await System.IO.File.AppendAllTextAsync("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "C", location = "UserSeeds.cs:37", message = "Before SeedAdminUserAsync", data = new { tenantId = defaultTenantId.ToString(), timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
            // #endregion
            await SeedAdminUserAsync(context, userManager, defaultTenantId, logger);

            // Seed Manager User
            // #region agent log
            try { await System.IO.File.AppendAllTextAsync("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "C", location = "UserSeeds.cs:40", message = "Before SeedManagerUserAsync", data = new { tenantId = defaultTenantId.ToString(), timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
            // #endregion
            await SeedManagerUserAsync(context, userManager, defaultTenantId, logger);

            logger.LogInformation("✅ User seeding completed successfully.");
            // #region agent log
            try { await System.IO.File.AppendAllTextAsync("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "B", location = "UserSeeds.cs:43", message = "SeedUsersAsync exit success", data = new { timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
            // #endregion
        }
        catch (Exception ex)
        {
            // #region agent log
            try { await System.IO.File.AppendAllTextAsync("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "E", location = "UserSeeds.cs:46", message = "SeedUsersAsync exception", data = new { exceptionType = ex.GetType().Name, exceptionMessage = ex.Message, stackTrace = ex.StackTrace?.Substring(0, Math.Min(500, (ex.StackTrace?.Length).GetValueOrDefault(0))), timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
            // #endregion
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
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "System",
                LastName = "Administrator",
                Department = "IT",
                JobTitle = "System Administrator",
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                MustChangePassword = true  // Force password change on first login
            };

            var createResult = await userManager.CreateAsync(adminUser, adminPassword);
            // #region agent log
            try { await System.IO.File.AppendAllTextAsync("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "C", location = "UserSeeds.cs:80", message = "Admin user create result", data = new { succeeded = createResult.Succeeded, errors = createResult.Errors.Select(e => e.Description).ToArray(), userId = adminUser?.Id, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
            // #endregion

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
                // #region agent log
                try { await System.IO.File.AppendAllTextAsync("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "C", location = "UserSeeds.cs:91", message = "PlatformAdmin role lookup", data = new { roleExists = superAdminRole != null, roleId = superAdminRole?.Id, userId = adminUser.Id, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                // #endregion
                if (superAdminRole != null)
                {
                    var addRoleResult = await userManager.AddToRoleAsync(adminUser, "PlatformAdmin");
                    // #region agent log
                    try { await System.IO.File.AppendAllTextAsync("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "C", location = "UserSeeds.cs:95", message = "AddToRoleAsync PlatformAdmin result", data = new { succeeded = addRoleResult.Succeeded, errors = addRoleResult.Errors.Select(e => e.Description).ToArray(), userId = adminUser.Id, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                    // #endregion
                }
                else
                {
                    var tenantAdminRole = await context.Set<Microsoft.AspNetCore.Identity.IdentityRole>()
                        .FirstOrDefaultAsync(r => r.Name == "TenantAdmin");
                    // #region agent log
                    try { await System.IO.File.AppendAllTextAsync("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "C", location = "UserSeeds.cs:99", message = "TenantAdmin role lookup", data = new { roleExists = tenantAdminRole != null, roleId = tenantAdminRole?.Id, userId = adminUser.Id, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                    // #endregion
                    if (tenantAdminRole != null)
                    {
                        var addRoleResult = await userManager.AddToRoleAsync(adminUser, "TenantAdmin");
                        // #region agent log
                        try { await System.IO.File.AppendAllTextAsync("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "C", location = "UserSeeds.cs:103", message = "AddToRoleAsync TenantAdmin result", data = new { succeeded = addRoleResult.Succeeded, errors = addRoleResult.Errors.Select(e => e.Description).ToArray(), userId = adminUser.Id, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                        // #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                // #region agent log
                try { await System.IO.File.AppendAllTextAsync("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "C", location = "UserSeeds.cs:109", message = "Role assignment exception", data = new { exceptionType = ex.GetType().Name, exceptionMessage = ex.Message, userId = adminUser.Id, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                // #endregion
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
        // #region agent log
        try { await System.IO.File.AppendAllTextAsync("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "D", location = "UserSeeds.cs:123", message = "Before LinkUserToTenantAsync admin", data = new { userId = adminUser.Id, tenantId = tenantId.ToString(), timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
        // #endregion
        // LOW PRIORITY FIX: Use RoleConstants instead of magic string
        var adminRoleCode = await GetOrCreateRoleCodeAsync(context, RoleConstants.TenantAdmin, "Administrator", logger);
        // #region agent log
        try { await System.IO.File.AppendAllTextAsync("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "D", location = "UserSeeds.cs:125", message = "GetOrCreateRoleCodeAsync admin result", data = new { roleCode = adminRoleCode, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
        // #endregion
        var adminTitleCode = await GetOrCreateTitleCodeAsync(context, "SYSTEM_ADMINISTRATOR", "System Administrator", logger);
        // #region agent log
        try { await System.IO.File.AppendAllTextAsync("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "D", location = "UserSeeds.cs:127", message = "GetOrCreateTitleCodeAsync admin result", data = new { titleCode = adminTitleCode, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
        // #endregion
        await LinkUserToTenantAsync(context, adminUser.Id, tenantId, adminRoleCode, adminTitleCode, logger);
        // #region agent log
        try { await System.IO.File.AppendAllTextAsync("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "D", location = "UserSeeds.cs:128", message = "After LinkUserToTenantAsync admin", data = new { userId = adminUser.Id, tenantId = tenantId.ToString(), timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
        // #endregion
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
                UserName = managerEmail,
                Email = managerEmail,
                EmailConfirmed = true,
                FirstName = "Compliance",
                LastName = "Manager",
                Department = "Compliance",
                JobTitle = "Compliance Manager",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            var createResult = await userManager.CreateAsync(managerUser, managerPassword);

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
        await LinkUserToTenantAsync(context, managerUser.Id, tenantId, managerRoleCode, managerTitleCode, logger);
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
        var userExists = await context.Set<ApplicationUser>().AnyAsync(u => u.Id == userId);
        if (!userExists)
        {
            logger.LogWarning($"User {userId} not found in database. Skipping tenant link.");
            return;
        }

        // Check if TenantUser already exists
        var existingTenantUser = await context.TenantUsers
            .FirstOrDefaultAsync(tu => tu.TenantId == tenantId && tu.UserId == userId && !tu.IsDeleted);

        // #region agent log
        try { await System.IO.File.AppendAllTextAsync("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "D", location = "UserSeeds.cs:287", message = "Existing TenantUser check", data = new { existingTenantUserId = existingTenantUser?.Id.ToString(), alreadyExists = existingTenantUser != null, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
        // #endregion
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
        // #region agent log
        try { await System.IO.File.AppendAllTextAsync("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "D", location = "UserSeeds.cs:315", message = "Before SaveChangesAsync TenantUser", data = new { tenantUserId = tenantUser.Id.ToString(), userId, tenantId = tenantId.ToString(), roleCode, titleCode, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
        // #endregion
        await context.SaveChangesAsync();
        // #region agent log
        try { await System.IO.File.AppendAllTextAsync("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "D", location = "UserSeeds.cs:317", message = "After SaveChangesAsync TenantUser", data = new { tenantUserId = tenantUser.Id.ToString(), success = true, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
        // #endregion

        logger.LogInformation($"✅ User {userId} linked to tenant {tenantId} with role {roleCode}");
    }
}
