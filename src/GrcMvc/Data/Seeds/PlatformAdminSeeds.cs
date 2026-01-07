using GrcMvc.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GrcMvc.Data.Seeds;

/// <summary>
/// Seeds the initial Platform Administrator (Owner level)
/// This is the super admin who manages all tenants
/// </summary>
public static class PlatformAdminSeeds
{
    private const string OwnerEmail = "Dooganlap@gmail.com";
    private const string OwnerDisplayName = "Platform Owner";
    private const string DefaultPassword = "Platform@2026!";

    public static async Task SeedPlatformAdminAsync(
        GrcDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger logger)
    {
        logger.LogInformation("🔐 Seeding Platform Admin...");

        // Check if platform admin already exists
        var existingAdmin = await context.PlatformAdmins
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.ContactEmail == OwnerEmail);

        if (existingAdmin != null)
        {
            logger.LogInformation("✅ Platform Admin already exists: {Email}", OwnerEmail);
            return;
        }

        // Find or create the user account
        var user = await userManager.FindByEmailAsync(OwnerEmail);

        if (user == null)
        {
            logger.LogInformation("Creating user account for Platform Owner: {Email}", OwnerEmail);

            user = new ApplicationUser
            {
                UserName = OwnerEmail,
                Email = OwnerEmail,
                EmailConfirmed = true,
                FirstName = "Platform",
                LastName = "Owner",
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                MustChangePassword = true  // Force password change on first login
            };

            var createResult = await userManager.CreateAsync(user, DefaultPassword);
            if (!createResult.Succeeded)
            {
                logger.LogError("❌ Failed to create Platform Owner user: {Errors}",
                    string.Join(", ", createResult.Errors.Select(e => e.Description)));
                return;
            }

            // Add to PlatformAdmin role
            if (!await userManager.IsInRoleAsync(user, "PlatformAdmin"))
            {
                await userManager.AddToRoleAsync(user, "PlatformAdmin");
            }

            logger.LogInformation("✅ Platform Owner user created: {Email}", OwnerEmail);
        }

        // Create Platform Admin record with full Owner permissions
        var platformAdmin = new PlatformAdmin
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            DisplayName = OwnerDisplayName,
            ContactEmail = OwnerEmail,
            AdminLevel = PlatformAdminLevel.Owner,
            Status = "Active",

            // Full permissions for Owner
            CanCreateTenants = true,
            CanManageTenants = true,
            CanDeleteTenants = true,
            CanManageBilling = true,
            CanAccessTenantData = true,
            CanManageCatalogs = true,
            CanManagePlatformAdmins = true,
            CanViewAnalytics = true,
            CanManageConfiguration = true,
            CanImpersonateUsers = true,

            // Password management
            CanResetOwnPassword = true,
            CanResetTenantAdminPasswords = true,
            CanRestartTenantAdminAccounts = true,

            // No restrictions
            AllowedRegions = null,
            AllowedTenantIds = null,
            MaxTenantsAllowed = 0, // Unlimited

            // Security
            MfaRequired = false, // Can be enabled later
            SessionTimeoutMinutes = 60,

            // Audit
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System",
            Notes = "Initial Platform Owner - Full access to all platform features"
        };

        context.PlatformAdmins.Add(platformAdmin);
        await context.SaveChangesAsync();

        logger.LogInformation("✅ Platform Admin created successfully: {Email} with Owner level access", OwnerEmail);
        logger.LogInformation("🔑 Default password: {Password} (change immediately!)", DefaultPassword);
    }

    /// <summary>
    /// Reset password for a platform admin
    /// </summary>
    public static async Task<bool> ResetPlatformAdminPasswordAsync(
        GrcDbContext context,
        UserManager<ApplicationUser> userManager,
        string adminEmail,
        string newPassword,
        ILogger logger)
    {
        var admin = await context.PlatformAdmins
            .FirstOrDefaultAsync(p => p.ContactEmail == adminEmail);

        if (admin == null)
        {
            logger.LogWarning("Platform Admin not found: {Email}", adminEmail);
            return false;
        }

        var user = await userManager.FindByIdAsync(admin.UserId);
        if (user == null)
        {
            logger.LogWarning("User not found for Platform Admin: {Email}", adminEmail);
            return false;
        }

        // Reset password
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, token, newPassword);

        if (result.Succeeded)
        {
            admin.LastPasswordChangedAt = DateTime.UtcNow;
            admin.ForcePasswordChange = false;
            await context.SaveChangesAsync();

            logger.LogInformation("✅ Password reset successful for: {Email}", adminEmail);
            return true;
        }

        logger.LogError("❌ Password reset failed: {Errors}",
            string.Join(", ", result.Errors.Select(e => e.Description)));
        return false;
    }

    /// <summary>
    /// Force password reset on next login
    /// </summary>
    public static async Task<bool> ForcePasswordResetAsync(
        GrcDbContext context,
        string adminEmail,
        ILogger logger)
    {
        var admin = await context.PlatformAdmins
            .FirstOrDefaultAsync(p => p.ContactEmail == adminEmail);

        if (admin == null)
        {
            logger.LogWarning("Platform Admin not found: {Email}", adminEmail);
            return false;
        }

        admin.ForcePasswordChange = true;
        admin.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        logger.LogInformation("✅ Force password reset set for: {Email}", adminEmail);
        return true;
    }
}
