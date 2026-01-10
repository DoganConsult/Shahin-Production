using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GrcMvc.Models.Entities;

namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Service for Owner/PlatformAdmin dashboard statistics and KPIs
/// Replaces direct DbContext access in OwnerController
/// </summary>
public interface IOwnerDashboardService
{
    #region Tenant Statistics
    
    /// <summary>
    /// Get total tenant count
    /// </summary>
    Task<int> GetTotalTenantsCountAsync();
    
    /// <summary>
    /// Get owner-created tenant count
    /// </summary>
    Task<int> GetOwnerCreatedTenantsCountAsync();
    
    /// <summary>
    /// Get active tenant count
    /// </summary>
    Task<int> GetActiveTenantsCountAsync();
    
    /// <summary>
    /// Get tenants with generated admin accounts
    /// </summary>
    Task<int> GetTenantsWithAdminCountAsync();
    
    #endregion
    
    #region Sector & Framework Statistics
    
    /// <summary>
    /// Get unique main sector count
    /// </summary>
    Task<int> GetMainSectorsCountAsync();
    
    /// <summary>
    /// Get GOSI sub-sector count
    /// </summary>
    Task<int> GetGosiSubSectorsCountAsync();
    
    /// <summary>
    /// Get sector-framework mapping count
    /// </summary>
    Task<int> GetSectorMappingsCountAsync();
    
    #endregion
    
    #region Regulatory Content Statistics
    
    /// <summary>
    /// Get total regulator count
    /// </summary>
    Task<int> GetRegulatorsCountAsync();
    
    /// <summary>
    /// Get total framework count
    /// </summary>
    Task<int> GetFrameworksCountAsync();
    
    /// <summary>
    /// Get total control count
    /// </summary>
    Task<int> GetControlsCountAsync();
    
    /// <summary>
    /// Get evidence types count
    /// </summary>
    Task<int> GetEvidenceTypesCountAsync();
    
    #endregion
    
    #region Workflow Statistics
    
    /// <summary>
    /// Get workflow definition count
    /// </summary>
    Task<int> GetWorkflowDefinitionsCountAsync();
    
    /// <summary>
    /// Get workflow instance count
    /// </summary>
    Task<int> GetWorkflowInstancesCountAsync();
    
    #endregion
    
    #region User Statistics
    
    /// <summary>
    /// Get unique user count across all tenants
    /// </summary>
    Task<int> GetTotalUsersCountAsync();
    
    #endregion
    
    #region Recent Activity
    
    /// <summary>
    /// Get recent tenants for dashboard
    /// </summary>
    Task<List<RecentTenantDto>> GetRecentTenantsAsync(int count = 5);
    
    #endregion
    
    #region Combined Dashboard
    
    /// <summary>
    /// Get complete dashboard statistics in one call
    /// </summary>
    Task<OwnerDashboardStatsDto> GetDashboardStatsAsync();
    
    #endregion
    
    #region Tenant Management
    
    /// <summary>
    /// Get all tenants ordered by creation date
    /// </summary>
    Task<List<Tenant>> GetAllTenantsAsync();
    
    /// <summary>
    /// Get tenant by ID
    /// </summary>
    Task<Tenant?> GetTenantByIdAsync(Guid id);
    
    /// <summary>
    /// Get tenant details with users
    /// </summary>
    Task<TenantDetailDto?> GetTenantDetailAsync(Guid id);
    
    /// <summary>
    /// Get owner-generated users for a tenant
    /// </summary>
    Task<List<TenantUser>> GetOwnerGeneratedUsersAsync(Guid tenantId);
    
    /// <summary>
    /// Get owner tenant creation record
    /// </summary>
    Task<OwnerTenantCreation?> GetOwnerTenantCreationAsync(Guid tenantId);
    
    #endregion
}

#region DTOs

/// <summary>
/// Recent tenant summary for dashboard
/// </summary>
public class RecentTenantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}

/// <summary>
/// Complete owner dashboard statistics
/// </summary>
public class OwnerDashboardStatsDto
{
    // Tenant Statistics
    public int TotalTenants { get; set; }
    public int OwnerCreatedTenants { get; set; }
    public int ActiveTenants { get; set; }
    public int TenantsWithAdmin { get; set; }
    
    // Sector & Framework Statistics
    public int TotalMainSectors { get; set; }
    public int TotalGosiSubSectors { get; set; }
    public int TotalSectorMappings { get; set; }
    
    // Regulatory Content Statistics
    public int TotalRegulators { get; set; }
    public int TotalFrameworks { get; set; }
    public int TotalControls { get; set; }
    public int TotalEvidenceTypes { get; set; }
    
    // Workflow Statistics
    public int TotalWorkflowDefinitions { get; set; }
    public int TotalWorkflowInstances { get; set; }
    
    // User Statistics
    public int TotalUsers { get; set; }
    
    // Recent Activity
    public List<RecentTenantDto> RecentTenants { get; set; } = new();
}

/// <summary>
/// Tenant detail with users and creation info
/// </summary>
public class TenantDetailDto
{
    public Guid Id { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public string TenantSlug { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsOwnerCreated { get; set; }
    public bool AdminAccountGenerated { get; set; }
    public DateTime CreatedDate { get; set; }
    public List<OwnerTenantUserDto> OwnerGeneratedUsers { get; set; } = new();
    public OwnerTenantCreationDto? CreationInfo { get; set; }
}

/// <summary>
/// Tenant user summary for owner dashboard (separate from OrgSetupDtos.TenantUserDto)
/// </summary>
public class OwnerTenantUserDto
{
    public string UserId { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string RoleCode { get; set; } = string.Empty;
    public string TitleCode { get; set; } = string.Empty;
    public bool IsOwnerGenerated { get; set; }
    public DateTime CreatedDate { get; set; }
}

/// <summary>
/// Owner tenant creation info
/// </summary>
public class OwnerTenantCreationDto
{
    public Guid TenantId { get; set; }
    public string OwnerId { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime CredentialsExpiresAt { get; set; }
    public string DeliveryMethod { get; set; } = "Manual";
    public bool CredentialsDelivered { get; set; }
}

#endregion
