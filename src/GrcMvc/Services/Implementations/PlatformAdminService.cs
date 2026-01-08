using GrcMvc.Data;
using GrcMvc.Exceptions;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Service for managing Platform Administrators (Layer 0 - above tenant context)
/// Super power feature - must be bulletproof with no failures
/// </summary>
public class PlatformAdminService : IPlatformAdminService
{
    private readonly GrcDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<PlatformAdminService> _logger;

    public PlatformAdminService(
        GrcDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<PlatformAdminService> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    #region Query Methods

    public async Task<PlatformAdmin?> GetByIdAsync(Guid id)
    {
        return await _context.PlatformAdmins
            .Include(p => p.User)
            .Where(p => !p.IsDeleted)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<PlatformAdmin?> GetByUserIdAsync(string userId)
    {
        return await _context.PlatformAdmins
            .Include(p => p.User)
            .Where(p => !p.IsDeleted)
            .FirstOrDefaultAsync(p => p.UserId == userId);
    }

    public async Task<List<PlatformAdmin>> GetAllAsync()
    {
        return await _context.PlatformAdmins
            .Include(p => p.User)
            .Where(p => !p.IsDeleted)
            .OrderByDescending(p => p.AdminLevel)
            .ThenBy(p => p.DisplayName)
            .ToListAsync();
    }

    public async Task<List<PlatformAdmin>> GetByLevelAsync(PlatformAdminLevel level)
    {
        return await _context.PlatformAdmins
            .Include(p => p.User)
            .Where(p => !p.IsDeleted && p.AdminLevel == level)
            .OrderBy(p => p.DisplayName)
            .ToListAsync();
    }

    public async Task<List<PlatformAdmin>> GetActiveAdminsAsync()
    {
        return await _context.PlatformAdmins
            .Include(p => p.User)
            .Where(p => !p.IsDeleted && p.Status == "Active")
            .OrderByDescending(p => p.AdminLevel)
            .ThenBy(p => p.DisplayName)
            .ToListAsync();
    }

    public async Task<bool> IsUserPlatformAdminAsync(string userId)
    {
        return await _context.PlatformAdmins
            .AnyAsync(p => p.UserId == userId && p.Status == "Active" && !p.IsDeleted);
    }

    public async Task<bool> HasPermissionAsync(string userId, PlatformPermission permission)
    {
        var admin = await GetByUserIdAsync(userId);
        if (admin == null || admin.Status != "Active")
            return false;

        return permission switch
        {
            PlatformPermission.CreateTenants => admin.CanCreateTenants,
            PlatformPermission.ManageTenants => admin.CanManageTenants,
            PlatformPermission.DeleteTenants => admin.CanDeleteTenants,
            PlatformPermission.ManageBilling => admin.CanManageBilling,
            PlatformPermission.AccessTenantData => admin.CanAccessTenantData,
            PlatformPermission.ManageCatalogs => admin.CanManageCatalogs,
            PlatformPermission.ManagePlatformAdmins => admin.CanManagePlatformAdmins,
            PlatformPermission.ViewAnalytics => admin.CanViewAnalytics,
            PlatformPermission.ManageConfiguration => admin.CanManageConfiguration,
            PlatformPermission.ImpersonateUsers => admin.CanImpersonateUsers,
            _ => false
        };
    }

    #endregion

    #region Command Methods

    public async Task<PlatformAdmin> CreateAsync(CreatePlatformAdminDto dto, string createdByAdminId)
    {
        _logger.LogInformation("Creating Platform Admin for user {UserId}", dto.UserId);

        // Validate user exists
        var user = await _userManager.FindByIdAsync(dto.UserId);
        if (user == null)
            throw new UserNotFoundException(dto.UserId);

        // Check if already a platform admin
        var existing = await GetByUserIdAsync(dto.UserId);
        if (existing != null)
            throw new EntityExistsException("PlatformAdmin", "UserId", dto.UserId.ToString());

        // Verify creator has permission
        if (!string.IsNullOrEmpty(createdByAdminId))
        {
            var hasPermission = await HasPermissionAsync(createdByAdminId, PlatformPermission.ManagePlatformAdmins);
            if (!hasPermission)
                throw new UnauthorizedAccessException("You don't have permission to create Platform Admins");
        }

        var admin = new PlatformAdmin
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            DisplayName = dto.DisplayName,
            ContactEmail = dto.ContactEmail,
            ContactPhone = dto.ContactPhone,
            AdminLevel = dto.AdminLevel,
            Status = "Active",

            // Permissions
            CanCreateTenants = dto.CanCreateTenants,
            CanManageTenants = dto.CanManageTenants,
            CanDeleteTenants = dto.CanDeleteTenants,
            CanManageBilling = dto.CanManageBilling,
            CanAccessTenantData = dto.CanAccessTenantData,
            CanManageCatalogs = dto.CanManageCatalogs,
            CanManagePlatformAdmins = dto.CanManagePlatformAdmins,
            CanViewAnalytics = dto.CanViewAnalytics,
            CanManageConfiguration = dto.CanManageConfiguration,
            CanImpersonateUsers = dto.CanImpersonateUsers,

            // Password management
            CanResetOwnPassword = true,
            CanResetTenantAdminPasswords = dto.AdminLevel >= PlatformAdminLevel.CoOwner,
            CanRestartTenantAdminAccounts = dto.AdminLevel >= PlatformAdminLevel.CoOwner,

            // Scope
            AllowedRegions = dto.AllowedRegions != null ? JsonSerializer.Serialize(dto.AllowedRegions) : null,
            AllowedTenantIds = dto.AllowedTenantIds != null ? JsonSerializer.Serialize(dto.AllowedTenantIds) : null,
            MaxTenantsAllowed = dto.MaxTenantsAllowed,

            // Security
            MfaRequired = dto.MfaRequired,
            SessionTimeoutMinutes = dto.SessionTimeoutMinutes,

            // Audit
            CreatedByAdminId = createdByAdminId,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = createdByAdminId ?? "System",
            Notes = dto.Notes
        };

        // Add PlatformAdmin role if not already assigned
        if (!await _userManager.IsInRoleAsync(user, "PlatformAdmin"))
        {
            await _userManager.AddToRoleAsync(user, "PlatformAdmin");
        }

        _context.PlatformAdmins.Add(admin);
        await _context.SaveChangesAsync();

        _logger.LogInformation("✅ Platform Admin created: {Email} with level {Level}",
            dto.ContactEmail, dto.AdminLevel);

        return admin;
    }

    public async Task<PlatformAdmin> UpdateAsync(Guid id, UpdatePlatformAdminDto dto)
    {
        var admin = await GetByIdAsync(id);
        if (admin == null)
            throw new EntityNotFoundException("PlatformAdmin", id);

        // Update fields if provided
        if (!string.IsNullOrEmpty(dto.DisplayName))
            admin.DisplayName = dto.DisplayName;
        if (!string.IsNullOrEmpty(dto.ContactEmail))
            admin.ContactEmail = dto.ContactEmail;
        if (dto.ContactPhone != null)
            admin.ContactPhone = dto.ContactPhone;
        if (dto.AdminLevel.HasValue)
            admin.AdminLevel = dto.AdminLevel.Value;

        // Permissions
        if (dto.CanCreateTenants.HasValue)
            admin.CanCreateTenants = dto.CanCreateTenants.Value;
        if (dto.CanManageTenants.HasValue)
            admin.CanManageTenants = dto.CanManageTenants.Value;
        if (dto.CanDeleteTenants.HasValue)
            admin.CanDeleteTenants = dto.CanDeleteTenants.Value;
        if (dto.CanManageBilling.HasValue)
            admin.CanManageBilling = dto.CanManageBilling.Value;
        if (dto.CanAccessTenantData.HasValue)
            admin.CanAccessTenantData = dto.CanAccessTenantData.Value;
        if (dto.CanManageCatalogs.HasValue)
            admin.CanManageCatalogs = dto.CanManageCatalogs.Value;
        if (dto.CanManagePlatformAdmins.HasValue)
            admin.CanManagePlatformAdmins = dto.CanManagePlatformAdmins.Value;
        if (dto.CanViewAnalytics.HasValue)
            admin.CanViewAnalytics = dto.CanViewAnalytics.Value;
        if (dto.CanManageConfiguration.HasValue)
            admin.CanManageConfiguration = dto.CanManageConfiguration.Value;
        if (dto.CanImpersonateUsers.HasValue)
            admin.CanImpersonateUsers = dto.CanImpersonateUsers.Value;

        // Scope
        if (dto.AllowedRegions != null)
            admin.AllowedRegions = JsonSerializer.Serialize(dto.AllowedRegions);
        if (dto.AllowedTenantIds != null)
            admin.AllowedTenantIds = JsonSerializer.Serialize(dto.AllowedTenantIds);
        if (dto.MaxTenantsAllowed.HasValue)
            admin.MaxTenantsAllowed = dto.MaxTenantsAllowed.Value;

        // Security
        if (dto.MfaRequired.HasValue)
            admin.MfaRequired = dto.MfaRequired.Value;
        if (dto.SessionTimeoutMinutes.HasValue)
            admin.SessionTimeoutMinutes = dto.SessionTimeoutMinutes.Value;

        if (dto.Notes != null)
            admin.Notes = dto.Notes;

        admin.ModifiedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("✅ Platform Admin updated: {Id}", id);
        return admin;
    }

    public async Task<bool> SuspendAsync(Guid id, string reason)
    {
        var admin = await GetByIdAsync(id);
        if (admin == null)
            return false;

        // Cannot suspend Owner level
        if (admin.AdminLevel == PlatformAdminLevel.Owner)
        {
            _logger.LogWarning("Cannot suspend Owner level admin: {Id}", id);
            return false;
        }

        admin.Status = "Suspended";
        admin.StatusReason = reason;
        admin.ModifiedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("⚠️ Platform Admin suspended: {Id}, Reason: {Reason}", id, reason);
        return true;
    }

    public async Task<bool> ReactivateAsync(Guid id)
    {
        var admin = await GetByIdAsync(id);
        if (admin == null)
            return false;

        admin.Status = "Active";
        admin.StatusReason = null;
        admin.ModifiedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("✅ Platform Admin reactivated: {Id}", id);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var admin = await GetByIdAsync(id);
        if (admin == null)
            return false;

        // Cannot delete Owner level
        if (admin.AdminLevel == PlatformAdminLevel.Owner)
        {
            _logger.LogWarning("Cannot delete Owner level admin: {Id}", id);
            return false;
        }

        admin.IsDeleted = true;
        admin.ModifiedDate = DateTime.UtcNow;
        admin.Status = "Revoked";
        await _context.SaveChangesAsync();

        _logger.LogInformation("🗑️ Platform Admin deleted: {Id}", id);
        return true;
    }

    #endregion

    #region Activity Tracking

    public async Task RecordLoginAsync(string userId, string ipAddress)
    {
        var admin = await GetByUserIdAsync(userId);
        if (admin == null) return;

        admin.LastLoginAt = DateTime.UtcNow;
        admin.LastLoginIp = ipAddress;
        await _context.SaveChangesAsync();
    }

    public async Task IncrementTenantCreatedCountAsync(string userId)
    {
        var admin = await GetByUserIdAsync(userId);
        if (admin == null) return;

        admin.TotalTenantsCreated++;
        admin.LastTenantCreatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    #endregion

    #region Validation

    public async Task<bool> CanCreateTenantAsync(string userId)
    {
        var admin = await GetByUserIdAsync(userId);
        if (admin == null || admin.Status != "Active")
            return false;

        if (!admin.CanCreateTenants)
            return false;

        // Check quota
        if (admin.MaxTenantsAllowed > 0 && admin.TotalTenantsCreated >= admin.MaxTenantsAllowed)
            return false;

        return true;
    }

    public async Task<bool> CanManageTenantAsync(string userId, Guid tenantId)
    {
        var admin = await GetByUserIdAsync(userId);
        if (admin == null || admin.Status != "Active")
            return false;

        if (!admin.CanManageTenants)
            return false;

        // Check if tenant is in allowed list (if restricted)
        if (!string.IsNullOrEmpty(admin.AllowedTenantIds))
        {
            var allowedIds = JsonSerializer.Deserialize<List<Guid>>(admin.AllowedTenantIds);
            if (allowedIds != null && !allowedIds.Contains(tenantId))
                return false;
        }

        return true;
    }

    public async Task<List<Guid>> GetAllowedTenantIdsAsync(string userId)
    {
        var admin = await GetByUserIdAsync(userId);
        if (admin == null)
            return new List<Guid>();

        if (string.IsNullOrEmpty(admin.AllowedTenantIds))
        {
            // No restriction - return all tenant IDs
            return await _context.Tenants
                .Where(t => !t.IsDeleted)
                .Select(t => t.Id)
                .ToListAsync();
        }

        return JsonSerializer.Deserialize<List<Guid>>(admin.AllowedTenantIds) ?? new List<Guid>();
    }

    #endregion

    #region Password Management

    /// <summary>
    /// Reset password for a platform admin
    /// </summary>
    public async Task<bool> ResetPasswordAsync(string adminUserId, string targetUserId, string newPassword)
    {
        var requester = await GetByUserIdAsync(adminUserId);
        if (requester == null || requester.Status != "Active")
        {
            _logger.LogWarning("Password reset denied - requester not active: {UserId}", adminUserId);
            return false;
        }

        // Self-reset
        if (adminUserId == targetUserId)
        {
            if (!requester.CanResetOwnPassword)
            {
                _logger.LogWarning("Password reset denied - self-reset not allowed: {UserId}", adminUserId);
                return false;
            }
        }
        else
        {
            // Reset for others requires permission
            if (!requester.CanResetTenantAdminPasswords)
            {
                _logger.LogWarning("Password reset denied - no permission: {UserId}", adminUserId);
                return false;
            }
        }

        var user = await _userManager.FindByIdAsync(targetUserId);
        if (user == null)
        {
            _logger.LogWarning("Password reset failed - user not found: {UserId}", targetUserId);
            return false;
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

        if (result.Succeeded)
        {
            // Update platform admin record if target is also a platform admin
            var targetAdmin = await GetByUserIdAsync(targetUserId);
            if (targetAdmin != null)
            {
                targetAdmin.LastPasswordChangedAt = DateTime.UtcNow;
                targetAdmin.ForcePasswordChange = false;
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("✅ Password reset successful for: {UserId}", targetUserId);
            return true;
        }

        _logger.LogError("❌ Password reset failed: {Errors}",
            string.Join(", ", result.Errors.Select(e => e.Description)));
        return false;
    }

    /// <summary>
    /// Force password change on next login
    /// </summary>
    public async Task<bool> ForcePasswordChangeAsync(string adminUserId, string targetUserId)
    {
        var requester = await GetByUserIdAsync(adminUserId);
        if (requester == null || !requester.CanResetTenantAdminPasswords)
            return false;

        var targetAdmin = await GetByUserIdAsync(targetUserId);
        if (targetAdmin == null)
            return false;

        targetAdmin.ForcePasswordChange = true;
        targetAdmin.ModifiedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("✅ Force password change set for: {UserId}", targetUserId);
        return true;
    }

    /// <summary>
    /// Restart/regenerate tenant admin account with new credentials
    /// </summary>
    public async Task<(bool Success, string? NewPassword)> RestartTenantAdminAccountAsync(
        string adminUserId, Guid tenantId)
    {
        var requester = await GetByUserIdAsync(adminUserId);
        if (requester == null || !requester.CanRestartTenantAdminAccounts)
        {
            _logger.LogWarning("Tenant admin restart denied - no permission: {UserId}", adminUserId);
            return (false, null);
        }

        // Find tenant admin
        var tenant = await _context.Tenants.FindAsync(tenantId);
        if (tenant == null)
        {
            _logger.LogWarning("Tenant not found: {TenantId}", tenantId);
            return (false, null);
        }

        var adminUser = await _userManager.FindByEmailAsync(tenant.AdminEmail);
        if (adminUser == null)
        {
            _logger.LogWarning("Tenant admin user not found: {Email}", tenant.AdminEmail);
            return (false, null);
        }

        // Generate new password
        var newPassword = GenerateSecurePassword();
        var token = await _userManager.GeneratePasswordResetTokenAsync(adminUser);
        var result = await _userManager.ResetPasswordAsync(adminUser, token, newPassword);

        if (result.Succeeded)
        {
            // Update tenant record
            tenant.CredentialExpiresAt = DateTime.UtcNow.AddHours(72);
            await _context.SaveChangesAsync();

            _logger.LogInformation("✅ Tenant admin account restarted: {TenantId}, Admin: {Email}",
                tenantId, tenant.AdminEmail);
            return (true, newPassword);
        }

        _logger.LogError("❌ Tenant admin restart failed: {Errors}",
            string.Join(", ", result.Errors.Select(e => e.Description)));
        return (false, null);
    }

    private static string GenerateSecurePassword()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$%";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 12)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    #endregion
}
