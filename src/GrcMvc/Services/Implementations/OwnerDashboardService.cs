using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Implementation of Owner Dashboard Service
/// Encapsulates all database access for owner/platform admin dashboard
/// </summary>
public class OwnerDashboardService : IOwnerDashboardService
{
    private readonly GrcDbContext _context;
    private readonly ILogger<OwnerDashboardService> _logger;

    public OwnerDashboardService(
        GrcDbContext context,
        ILogger<OwnerDashboardService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Tenant Statistics

    public async Task<int> GetTotalTenantsCountAsync()
    {
        return await _context.Tenants.CountAsync();
    }

    public async Task<int> GetOwnerCreatedTenantsCountAsync()
    {
        return await _context.Tenants.CountAsync(t => t.IsOwnerCreated);
    }

    public async Task<int> GetActiveTenantsCountAsync()
    {
        return await _context.Tenants.CountAsync(t => t.IsActive);
    }

    public async Task<int> GetTenantsWithAdminCountAsync()
    {
        return await _context.Tenants.CountAsync(t => t.AdminAccountGenerated);
    }

    #endregion

    #region Sector & Framework Statistics

    public async Task<int> GetMainSectorsCountAsync()
    {
        return await _context.SectorFrameworkIndexes
            .Select(s => s.SectorCode)
            .Distinct()
            .CountAsync();
    }

    public async Task<int> GetGosiSubSectorsCountAsync()
    {
        return await _context.GrcSubSectorMappings.CountAsync();
    }

    public async Task<int> GetSectorMappingsCountAsync()
    {
        return await _context.SectorFrameworkIndexes.CountAsync();
    }

    #endregion

    #region Regulatory Content Statistics

    public async Task<int> GetRegulatorsCountAsync()
    {
        return await _context.Regulators.CountAsync();
    }

    public async Task<int> GetFrameworksCountAsync()
    {
        return await _context.FrameworkCatalogs.CountAsync();
    }

    public async Task<int> GetControlsCountAsync()
    {
        return await _context.FrameworkControls.CountAsync();
    }

    public async Task<int> GetEvidenceTypesCountAsync()
    {
        return await _context.EvidenceScoringCriteria.CountAsync();
    }

    #endregion

    #region Workflow Statistics

    public async Task<int> GetWorkflowDefinitionsCountAsync()
    {
        return await _context.WorkflowDefinitions.CountAsync();
    }

    public async Task<int> GetWorkflowInstancesCountAsync()
    {
        return await _context.WorkflowInstances.CountAsync();
    }

    #endregion

    #region User Statistics

    public async Task<int> GetTotalUsersCountAsync()
    {
        return await _context.TenantUsers
            .Select(tu => tu.UserId)
            .Distinct()
            .CountAsync();
    }

    #endregion

    #region Recent Activity

    public async Task<List<RecentTenantDto>> GetRecentTenantsAsync(int count = 5)
    {
        return await _context.Tenants
            .OrderByDescending(t => t.CreatedDate)
            .Take(count)
            .Select(t => new RecentTenantDto
            {
                Id = t.Id,
                Name = t.OrganizationName,
                Subdomain = t.TenantSlug,
                IsActive = t.IsActive,
                CreatedDate = t.CreatedDate
            })
            .ToListAsync();
    }

    #endregion

    #region Combined Dashboard

    public async Task<OwnerDashboardStatsDto> GetDashboardStatsAsync()
    {
        _logger.LogInformation("Fetching complete owner dashboard statistics");

        // Execute all counts in parallel for better performance
        var totalTenantsTask = GetTotalTenantsCountAsync();
        var ownerCreatedTask = GetOwnerCreatedTenantsCountAsync();
        var activeTenantsTask = GetActiveTenantsCountAsync();
        var tenantsWithAdminTask = GetTenantsWithAdminCountAsync();
        var mainSectorsTask = GetMainSectorsCountAsync();
        var gosiSubSectorsTask = GetGosiSubSectorsCountAsync();
        var sectorMappingsTask = GetSectorMappingsCountAsync();
        var regulatorsTask = GetRegulatorsCountAsync();
        var frameworksTask = GetFrameworksCountAsync();
        var controlsTask = GetControlsCountAsync();
        var evidenceTypesTask = GetEvidenceTypesCountAsync();
        var workflowDefsTask = GetWorkflowDefinitionsCountAsync();
        var workflowInstancesTask = GetWorkflowInstancesCountAsync();
        var usersTask = GetTotalUsersCountAsync();
        var recentTenantsTask = GetRecentTenantsAsync(5);

        await Task.WhenAll(
            totalTenantsTask, ownerCreatedTask, activeTenantsTask, tenantsWithAdminTask,
            mainSectorsTask, gosiSubSectorsTask, sectorMappingsTask,
            regulatorsTask, frameworksTask, controlsTask, evidenceTypesTask,
            workflowDefsTask, workflowInstancesTask, usersTask, recentTenantsTask
        );

        return new OwnerDashboardStatsDto
        {
            TotalTenants = await totalTenantsTask,
            OwnerCreatedTenants = await ownerCreatedTask,
            ActiveTenants = await activeTenantsTask,
            TenantsWithAdmin = await tenantsWithAdminTask,
            TotalMainSectors = await mainSectorsTask,
            TotalGosiSubSectors = await gosiSubSectorsTask,
            TotalSectorMappings = await sectorMappingsTask,
            TotalRegulators = await regulatorsTask,
            TotalFrameworks = await frameworksTask,
            TotalControls = await controlsTask,
            TotalEvidenceTypes = await evidenceTypesTask,
            TotalWorkflowDefinitions = await workflowDefsTask,
            TotalWorkflowInstances = await workflowInstancesTask,
            TotalUsers = await usersTask,
            RecentTenants = await recentTenantsTask
        };
    }

    #endregion
    
    #region Tenant Management

    public async Task<List<Tenant>> GetAllTenantsAsync()
    {
        return await _context.Tenants
            .OrderByDescending(t => t.CreatedDate)
            .ToListAsync();
    }

    public async Task<Tenant?> GetTenantByIdAsync(Guid id)
    {
        return await _context.Tenants.FindAsync(id);
    }

    public async Task<TenantDetailDto?> GetTenantDetailAsync(Guid id)
    {
        var tenant = await _context.Tenants.FindAsync(id);
        if (tenant == null) return null;

        var ownerUsers = await _context.TenantUsers
            .Include(tu => tu.User)
            .Where(tu => tu.TenantId == id && tu.IsOwnerGenerated)
            .ToListAsync();
        var creationInfo = await GetOwnerTenantCreationAsync(id);

        return new TenantDetailDto
        {
            Id = tenant.Id,
            OrganizationName = tenant.OrganizationName,
            TenantSlug = tenant.TenantSlug,
            IsActive = tenant.IsActive,
            IsOwnerCreated = tenant.IsOwnerCreated,
            AdminAccountGenerated = tenant.AdminAccountGenerated,
            CreatedDate = tenant.CreatedDate,
            OwnerGeneratedUsers = ownerUsers.Select(u => new OwnerTenantUserDto
            {
                UserId = u.UserId,
                Email = u.User?.Email,
                RoleCode = u.RoleCode,
                TitleCode = u.TitleCode,
                IsOwnerGenerated = u.IsOwnerGenerated,
                CreatedDate = u.CreatedDate
            }).ToList(),
            CreationInfo = creationInfo != null ? new OwnerTenantCreationDto
            {
                TenantId = creationInfo.TenantId,
                OwnerId = creationInfo.OwnerId,
                CreatedDate = creationInfo.CreatedDate,
                CredentialsExpiresAt = creationInfo.CredentialsExpiresAt,
                DeliveryMethod = creationInfo.DeliveryMethod,
                CredentialsDelivered = creationInfo.CredentialsDelivered
            } : null
        };
    }

    public async Task<List<TenantUser>> GetOwnerGeneratedUsersAsync(Guid tenantId)
    {
        return await _context.TenantUsers
            .Where(tu => tu.TenantId == tenantId && tu.IsOwnerGenerated)
            .ToListAsync();
    }

    public async Task<OwnerTenantCreation?> GetOwnerTenantCreationAsync(Guid tenantId)
    {
        return await _context.OwnerTenantCreations
            .FirstOrDefaultAsync(o => o.TenantId == tenantId);
    }

    #endregion
}
