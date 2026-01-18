using System;
using System.Collections.Generic;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Platform Administrator - manages multiple tenants across the GRC platform.
    /// This is the "Owner" or "Super Admin" level above individual tenant admins.
    /// Layer 0: Platform Level (above tenant context)
    /// </summary>
    public class PlatformAdmin : BaseEntity
    {
        public Guid UserId { get; set; } // ApplicationUser.Id (ABP Identity Guid)

        /// <summary>
        /// Admin level: Owner (full access), CoOwner, Support, ReadOnly
        /// </summary>
        public PlatformAdminLevel AdminLevel { get; set; } = PlatformAdminLevel.Support;

        /// <summary>
        /// Display name for platform admin
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Contact email (may differ from login email)
        /// </summary>
        public string ContactEmail { get; set; } = string.Empty;

        /// <summary>
        /// Phone for critical alerts
        /// </summary>
        public string? ContactPhone { get; set; }

        // =============================================================================
        // PERMISSIONS
        // =============================================================================

        /// <summary>
        /// Can create new tenants
        /// </summary>
        public bool CanCreateTenants { get; set; } = false;

        /// <summary>
        /// Can suspend/activate tenants
        /// </summary>
        public bool CanManageTenants { get; set; } = false;

        /// <summary>
        /// Can delete tenants (dangerous - requires Owner level)
        /// </summary>
        public bool CanDeleteTenants { get; set; } = false;

        /// <summary>
        /// Can manage platform billing and subscriptions
        /// </summary>
        public bool CanManageBilling { get; set; } = false;

        /// <summary>
        /// Can view/manage all tenant data (support access)
        /// </summary>
        public bool CanAccessTenantData { get; set; } = false;

        /// <summary>
        /// Can manage platform catalogs (Layer 1 - global data)
        /// </summary>
        public bool CanManageCatalogs { get; set; } = false;

        /// <summary>
        /// Can manage other platform admins
        /// </summary>
        public bool CanManagePlatformAdmins { get; set; } = false;

        /// <summary>
        /// Can view platform analytics and reports
        /// </summary>
        public bool CanViewAnalytics { get; set; } = true;

        /// <summary>
        /// Can manage platform configuration
        /// </summary>
        public bool CanManageConfiguration { get; set; } = false;

        /// <summary>
        /// Can impersonate tenant users for support
        /// </summary>
        public bool CanImpersonateUsers { get; set; } = false;

        // =============================================================================
        // SCOPE RESTRICTIONS (optional - for regional/partner admins)
        // =============================================================================

        /// <summary>
        /// If set, admin can only manage tenants in these regions
        /// Empty = all regions
        /// </summary>
        public string? AllowedRegions { get; set; } // JSON array: ["SA", "AE", "BH"]

        /// <summary>
        /// If set, admin can only manage these specific tenants
        /// Empty = all tenants (based on other permissions)
        /// </summary>
        public string? AllowedTenantIds { get; set; } // JSON array of Guid strings

        /// <summary>
        /// Maximum number of tenants this admin can create (0 = unlimited)
        /// </summary>
        public int MaxTenantsAllowed { get; set; } = 0;

        // =============================================================================
        // ACTIVITY TRACKING
        // =============================================================================

        public DateTime? LastLoginAt { get; set; }
        public string? LastLoginIp { get; set; }
        public int TotalTenantsCreated { get; set; } = 0;
        public DateTime? LastTenantCreatedAt { get; set; }

        /// <summary>
        /// Account status: Active, Suspended, Revoked
        /// </summary>
        public string Status { get; set; } = "Active";

        /// <summary>
        /// Reason for suspension/revocation
        /// </summary>
        public string? StatusReason { get; set; }

        /// <summary>
        /// MFA required for this admin
        /// </summary>
        public bool MfaRequired { get; set; } = true;

        /// <summary>
        /// Session timeout in minutes (0 = use default)
        /// </summary>
        public int SessionTimeoutMinutes { get; set; } = 30;

        // =============================================================================
        // PASSWORD & ACCOUNT MANAGEMENT
        // =============================================================================

        /// <summary>
        /// Can reset own password
        /// </summary>
        public bool CanResetOwnPassword { get; set; } = true;

        /// <summary>
        /// Can reset passwords for tenant admins
        /// </summary>
        public bool CanResetTenantAdminPasswords { get; set; } = false;

        /// <summary>
        /// Can restart/reset tenant admin accounts
        /// </summary>
        public bool CanRestartTenantAdminAccounts { get; set; } = false;

        /// <summary>
        /// Last password change date
        /// </summary>
        public DateTime? LastPasswordChangedAt { get; set; }

        /// <summary>
        /// Force password change on next login
        /// </summary>
        public bool ForcePasswordChange { get; set; } = false;

        // =============================================================================
        // AUDIT
        // =============================================================================

        public string? CreatedByAdminId { get; set; } // Who created this admin
        public string? Notes { get; set; } // Internal notes about this admin

        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual ICollection<OwnerTenantCreation> TenantCreations { get; set; } = new List<OwnerTenantCreation>();
    }

    /// <summary>
    /// Platform admin privilege levels
    /// </summary>
    public enum PlatformAdminLevel
    {
        /// <summary>
        /// Read-only access to platform data
        /// </summary>
        ReadOnly = 0,

        /// <summary>
        /// Support staff - can view tenant data and assist
        /// </summary>
        Support = 10,

        /// <summary>
        /// Regional/Partner admin - can manage assigned tenants
        /// </summary>
        Partner = 20,

        /// <summary>
        /// Co-Owner - full access except managing other owners
        /// </summary>
        CoOwner = 90,

        /// <summary>
        /// Owner - full platform access, can manage everything
        /// </summary>
        Owner = 100
    }
}
