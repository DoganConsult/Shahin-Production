using GrcMvc.Models.Entities;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Service for managing Platform Administrators (Layer 0 - above tenant context)
    /// </summary>
    public interface IPlatformAdminService
    {
        // Query
        Task<PlatformAdmin?> GetByIdAsync(Guid id);
        Task<PlatformAdmin?> GetByUserIdAsync(string userId);
        Task<List<PlatformAdmin>> GetAllAsync();
        Task<List<PlatformAdmin>> GetByLevelAsync(PlatformAdminLevel level);
        Task<List<PlatformAdmin>> GetActiveAdminsAsync();
        Task<bool> IsUserPlatformAdminAsync(string userId);
        Task<bool> HasPermissionAsync(string userId, PlatformPermission permission);

        // Commands
        Task<PlatformAdmin> CreateAsync(CreatePlatformAdminDto dto, string createdByAdminId);
        Task<PlatformAdmin> UpdateAsync(Guid id, UpdatePlatformAdminDto dto);
        Task<bool> SuspendAsync(Guid id, string reason);
        Task<bool> ReactivateAsync(Guid id);
        Task<bool> DeleteAsync(Guid id);

        // Activity tracking
        Task RecordLoginAsync(string userId, string ipAddress);
        Task IncrementTenantCreatedCountAsync(string userId);

        // Validation
        Task<bool> CanCreateTenantAsync(string userId);
        Task<bool> CanManageTenantAsync(string userId, Guid tenantId);
        Task<List<Guid>> GetAllowedTenantIdsAsync(string userId);

        // Password Management
        Task<bool> ResetPasswordAsync(string adminUserId, string targetUserId, string newPassword);
        Task<bool> ForcePasswordChangeAsync(string adminUserId, string targetUserId);
        Task<(bool Success, string? NewPassword)> RestartTenantAdminAccountAsync(string adminUserId, Guid tenantId);
    }

    public enum PlatformPermission
    {
        CreateTenants,
        ManageTenants,
        DeleteTenants,
        ManageBilling,
        AccessTenantData,
        ManageCatalogs,
        ManagePlatformAdmins,
        ViewAnalytics,
        ManageConfiguration,
        ImpersonateUsers
    }

    public class CreatePlatformAdminDto
    {
        public string UserId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string? ContactPhone { get; set; }
        public PlatformAdminLevel AdminLevel { get; set; } = PlatformAdminLevel.Support;

        // Permissions
        public bool CanCreateTenants { get; set; }
        public bool CanManageTenants { get; set; }
        public bool CanDeleteTenants { get; set; }
        public bool CanManageBilling { get; set; }
        public bool CanAccessTenantData { get; set; }
        public bool CanManageCatalogs { get; set; }
        public bool CanManagePlatformAdmins { get; set; }
        public bool CanViewAnalytics { get; set; } = true;
        public bool CanManageConfiguration { get; set; }
        public bool CanImpersonateUsers { get; set; }

        // Scope
        public List<string>? AllowedRegions { get; set; }
        public List<Guid>? AllowedTenantIds { get; set; }
        public int MaxTenantsAllowed { get; set; }

        // Security
        public bool MfaRequired { get; set; } = true;
        public int SessionTimeoutMinutes { get; set; } = 30;

        public string? Notes { get; set; }
    }

    public class UpdatePlatformAdminDto
    {
        public string? DisplayName { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public PlatformAdminLevel? AdminLevel { get; set; }

        // Permissions
        public bool? CanCreateTenants { get; set; }
        public bool? CanManageTenants { get; set; }
        public bool? CanDeleteTenants { get; set; }
        public bool? CanManageBilling { get; set; }
        public bool? CanAccessTenantData { get; set; }
        public bool? CanManageCatalogs { get; set; }
        public bool? CanManagePlatformAdmins { get; set; }
        public bool? CanViewAnalytics { get; set; }
        public bool? CanManageConfiguration { get; set; }
        public bool? CanImpersonateUsers { get; set; }

        // Scope
        public List<string>? AllowedRegions { get; set; }
        public List<Guid>? AllowedTenantIds { get; set; }
        public int? MaxTenantsAllowed { get; set; }

        // Security
        public bool? MfaRequired { get; set; }
        public int? SessionTimeoutMinutes { get; set; }

        public string? Notes { get; set; }
    }

    public class PlatformAdminDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string? ContactPhone { get; set; }
        public PlatformAdminLevel AdminLevel { get; set; }
        public string AdminLevelName => AdminLevel.ToString();
        public string Status { get; set; } = string.Empty;

        // Permissions summary
        public bool CanCreateTenants { get; set; }
        public bool CanManageTenants { get; set; }
        public bool CanManagePlatformAdmins { get; set; }

        // Activity
        public DateTime? LastLoginAt { get; set; }
        public int TotalTenantsCreated { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
