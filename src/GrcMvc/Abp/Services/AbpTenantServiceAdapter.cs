using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;

namespace GrcMvc.Abp.Services;

/// <summary>
/// Adapter that wraps ABP Tenant Management services.
/// ABP modules = core foundation, custom business logic layered on top.
/// 
/// ABP Services Used:
/// - ITenantAppService: Tenant CRUD operations
/// - ITenantRepository: Direct tenant data access
/// - ITenantManager: Tenant business logic
/// - ICurrentTenant: Current tenant context
/// </summary>
public class AbpTenantServiceAdapter : IAbpTenantServiceAdapter
{
    private readonly ITenantAppService _tenantAppService;
    private readonly ITenantRepository _tenantRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly ILogger<AbpTenantServiceAdapter> _logger;

    public AbpTenantServiceAdapter(
        ITenantAppService tenantAppService,
        ITenantRepository tenantRepository,
        ICurrentTenant currentTenant,
        ILogger<AbpTenantServiceAdapter> logger)
    {
        _tenantAppService = tenantAppService;
        _tenantRepository = tenantRepository;
        _currentTenant = currentTenant;
        _logger = logger;
    }

    /// <summary>
    /// Create a new tenant using ABP's ITenantAppService
    /// </summary>
    public async Task<TenantDto> CreateTenantAsync(string name, string? adminEmailAddress = null)
    {
        var createDto = new TenantCreateDto
        {
            Name = name,
            AdminEmailAddress = adminEmailAddress
        };

        var tenant = await _tenantAppService.CreateAsync(createDto);
        _logger.LogInformation("Tenant {TenantName} created via ABP with ID {TenantId}", name, tenant.Id);
        return tenant;
    }

    /// <summary>
    /// Get tenant by ID
    /// </summary>
    public async Task<TenantDto?> GetTenantByIdAsync(Guid tenantId)
    {
        try
        {
            return await _tenantAppService.GetAsync(tenantId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Tenant {TenantId} not found", tenantId);
            return null;
        }
    }

    /// <summary>
    /// Get tenant by name using repository
    /// </summary>
    public async Task<TenantDto?> GetTenantByNameAsync(string name)
    {
        var tenant = await _tenantRepository.FindByNameAsync(name);
        if (tenant == null) return null;

        return new TenantDto
        {
            Id = tenant.Id,
            Name = tenant.Name
        };
    }

    /// <summary>
    /// Update tenant
    /// </summary>
    public async Task<TenantDto> UpdateTenantAsync(Guid tenantId, TenantUpdateDto updateDto)
    {
        return await _tenantAppService.UpdateAsync(tenantId, updateDto);
    }

    /// <summary>
    /// Delete tenant
    /// </summary>
    public async Task DeleteTenantAsync(Guid tenantId)
    {
        await _tenantAppService.DeleteAsync(tenantId);
        _logger.LogInformation("Tenant {TenantId} deleted via ABP", tenantId);
    }

    /// <summary>
    /// Get paged list of tenants
    /// </summary>
    public async Task<PagedTenantResult> GetTenantsAsync(
        string? filter = null,
        int skipCount = 0,
        int maxResultCount = 10,
        string? sorting = null)
    {
        var input = new GetTenantsInput
        {
            Filter = filter,
            SkipCount = skipCount,
            MaxResultCount = maxResultCount,
            Sorting = sorting
        };

        var result = await _tenantAppService.GetListAsync(input);
        return new PagedTenantResult
        {
            TotalCount = result.TotalCount,
            Items = result.Items.ToList()
        };
    }

    /// <summary>
    /// Get current tenant info
    /// </summary>
    public CurrentTenantInfo GetCurrentTenantInfo()
    {
        return new CurrentTenantInfo
        {
            Id = _currentTenant.Id,
            Name = _currentTenant.Name,
            IsAvailable = _currentTenant.IsAvailable
        };
    }

    /// <summary>
    /// Change current tenant context (for cross-tenant operations)
    /// </summary>
    public IDisposable ChangeTenant(Guid? tenantId)
    {
        return _currentTenant.Change(tenantId);
    }
}

/// <summary>
/// Interface for ABP Tenant service adapter
/// </summary>
public interface IAbpTenantServiceAdapter
{
    Task<TenantDto> CreateTenantAsync(string name, string? adminEmailAddress = null);
    Task<TenantDto?> GetTenantByIdAsync(Guid tenantId);
    Task<TenantDto?> GetTenantByNameAsync(string name);
    Task<TenantDto> UpdateTenantAsync(Guid tenantId, TenantUpdateDto updateDto);
    Task DeleteTenantAsync(Guid tenantId);
    Task<PagedTenantResult> GetTenantsAsync(string? filter = null, int skipCount = 0, int maxResultCount = 10, string? sorting = null);
    CurrentTenantInfo GetCurrentTenantInfo();
    IDisposable ChangeTenant(Guid? tenantId);
}

/// <summary>
/// Current tenant information DTO
/// </summary>
public class CurrentTenantInfo
{
    public Guid? Id { get; set; }
    public string? Name { get; set; }
    public bool IsAvailable { get; set; }
}

/// <summary>
/// Paged tenant result DTO
/// </summary>
public class PagedTenantResult
{
    public long TotalCount { get; set; }
    public List<TenantDto> Items { get; set; } = new();
}
