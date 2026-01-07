using GrcMvc.Models.Entities;

namespace GrcMvc.Models.ViewModels;

#region Platform Admin ViewModels

/// <summary>
/// Platform Admin Dashboard ViewModel
/// </summary>
public class PlatformDashboardViewModel
{
    public int TotalTenants { get; set; }
    public int ActiveTenants { get; set; }
    public int PendingTenants { get; set; }
    public int SuspendedTenants { get; set; }
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int PlatformAdminCount { get; set; }
    public List<Tenant> RecentTenants { get; set; } = new();
    public List<AuditEvent> RecentActivity { get; set; } = new();
    public List<AuditEvent> RecentAuditEvents { get; set; } = new();
    public PlatformAdmin? Admin { get; set; }
}

/// <summary>
/// Create Tenant ViewModel
/// </summary>
public class CreateTenantViewModel
{
    public string OrganizationName { get; set; } = string.Empty;
    public string TenantSlug { get; set; } = string.Empty;
    public string AdminEmail { get; set; } = string.Empty;
    public string AdminDisplayName { get; set; } = string.Empty;
    public string SubscriptionTier { get; set; } = "MVP";
    public DateTime? SubscriptionEndDate { get; set; }
    public bool BypassPayment { get; set; }
}

/// <summary>
/// Edit Tenant ViewModel
/// </summary>
public class EditTenantViewModel
{
    public Guid Id { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public string TenantSlug { get; set; } = string.Empty;
    public string AdminEmail { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string SubscriptionTier { get; set; } = string.Empty;
    public DateTime? SubscriptionEndDate { get; set; }
    public bool BypassPayment { get; set; }
}

/// <summary>
/// Tenant Details ViewModel
/// </summary>
public class TenantDetailsViewModel
{
    public Tenant Tenant { get; set; } = null!;
    public int UserCount { get; set; }
    public int ActiveUserCount { get; set; }
    public int AssessmentCount { get; set; }
    public int PlanCount { get; set; }
    public List<TenantUser> Users { get; set; } = new();
    public List<AuditEvent> RecentActivity { get; set; } = new();
}

/// <summary>
/// User Details ViewModel (Platform Admin view)
/// </summary>
public class UserDetailsViewModel
{
    public ApplicationUser User { get; set; } = null!;
    public List<TenantUser> TenantMemberships { get; set; } = new();
    public List<string> SystemRoles { get; set; } = new();
    public List<AuditEvent> RecentActivity { get; set; } = new();
}

#endregion

#region Tenant Admin ViewModels

/// <summary>
/// Tenant Admin Dashboard ViewModel
/// </summary>
public class TenantAdminDashboardViewModel
{
    public Tenant Tenant { get; set; } = null!;
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int PendingInvites { get; set; }
    public int TotalPlans { get; set; }
    public int TotalAssessments { get; set; }
    public List<TenantUser> RecentUsers { get; set; } = new();
    public List<AuditEvent> RecentActivity { get; set; } = new();
}

/// <summary>
/// Invite User ViewModel
/// </summary>
public class InviteUserViewModel
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string RoleCode { get; set; } = string.Empty;
    public string? TitleCode { get; set; }
}

/// <summary>
/// Tenant User Details ViewModel
/// </summary>
public class TenantUserDetailsViewModel
{
    public TenantUser TenantUser { get; set; } = null!;
    public List<string> SystemRoles { get; set; } = new();
    public List<AuditEvent> RecentActivity { get; set; } = new();
}

/// <summary>
/// Tenant Settings ViewModel
/// </summary>
public class TenantSettingsViewModel
{
    public string OrganizationName { get; set; } = string.Empty;
    public string AdminEmail { get; set; } = string.Empty;
}

#endregion
