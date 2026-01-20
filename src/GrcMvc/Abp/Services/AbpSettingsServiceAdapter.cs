using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.SettingManagement;
using Volo.Abp.Settings;

namespace GrcMvc.Abp.Services;

/// <summary>
/// Adapter that wraps ABP Settings Management services.
/// ABP modules = core foundation, custom business logic layered on top.
/// 
/// ABP Services Used:
/// - ISettingManager: Read/write settings with scope (Global, Tenant, User)
/// - ISettingProvider: Read-only setting access
/// </summary>
public class AbpSettingsServiceAdapter : IAbpSettingsServiceAdapter
{
    private readonly ISettingManager _settingManager;
    private readonly ISettingProvider _settingProvider;
    private readonly ILogger<AbpSettingsServiceAdapter> _logger;

    public AbpSettingsServiceAdapter(
        ISettingManager settingManager,
        ISettingProvider settingProvider,
        ILogger<AbpSettingsServiceAdapter> logger)
    {
        _settingManager = settingManager;
        _settingProvider = settingProvider;
        _logger = logger;
    }

    /// <summary>
    /// Get setting value (reads from current scope: User > Tenant > Global)
    /// </summary>
    public async Task<string?> GetAsync(string name)
    {
        return await _settingProvider.GetOrNullAsync(name);
    }

    /// <summary>
    /// Get setting value with default
    /// </summary>
    public async Task<string> GetAsync(string name, string defaultValue)
    {
        var value = await _settingProvider.GetOrNullAsync(name);
        return value ?? defaultValue;
    }

    /// <summary>
    /// Get typed setting value
    /// </summary>
    public async Task<T> GetAsync<T>(string name, T defaultValue) where T : struct
    {
        var value = await _settingProvider.GetOrNullAsync(name);
        if (string.IsNullOrEmpty(value))
            return defaultValue;

        try
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Set setting value for current tenant
    /// </summary>
    public async Task SetForCurrentTenantAsync(string name, string value)
    {
        await _settingManager.SetForCurrentTenantAsync(name, value);
        _logger.LogInformation("Setting {SettingName} updated for current tenant", name);
    }

    /// <summary>
    /// Set setting value for specific tenant
    /// </summary>
    public async Task SetForTenantAsync(Guid tenantId, string name, string value)
    {
        await _settingManager.SetForTenantAsync(tenantId, name, value);
        _logger.LogInformation("Setting {SettingName} updated for tenant {TenantId}", name, tenantId);
    }

    /// <summary>
    /// Set setting value for current user
    /// </summary>
    public async Task SetForCurrentUserAsync(string name, string value)
    {
        await _settingManager.SetForCurrentUserAsync(name, value);
        _logger.LogInformation("Setting {SettingName} updated for current user", name);
    }

    /// <summary>
    /// Set setting value for specific user
    /// </summary>
    public async Task SetForUserAsync(Guid userId, string name, string value)
    {
        await _settingManager.SetForUserAsync(userId, name, value);
        _logger.LogInformation("Setting {SettingName} updated for user {UserId}", name, userId);
    }

    /// <summary>
    /// Set global setting value
    /// </summary>
    public async Task SetGlobalAsync(string name, string value)
    {
        await _settingManager.SetGlobalAsync(name, value);
        _logger.LogInformation("Global setting {SettingName} updated", name);
    }

    /// <summary>
    /// Get all settings for current tenant
    /// </summary>
    public async Task<List<SettingValue>> GetAllForCurrentTenantAsync()
    {
        return await _settingManager.GetAllForCurrentTenantAsync();
    }

    /// <summary>
    /// Get all settings for current user
    /// </summary>
    public async Task<List<SettingValue>> GetAllForCurrentUserAsync()
    {
        return await _settingManager.GetAllForCurrentUserAsync();
    }

    /// <summary>
    /// Get all global settings
    /// </summary>
    public async Task<List<SettingValue>> GetAllGlobalAsync()
    {
        return await _settingManager.GetAllGlobalAsync();
    }
}

/// <summary>
/// Interface for ABP Settings service adapter
/// </summary>
public interface IAbpSettingsServiceAdapter
{
    Task<string?> GetAsync(string name);
    Task<string> GetAsync(string name, string defaultValue);
    Task<T> GetAsync<T>(string name, T defaultValue) where T : struct;
    Task SetForCurrentTenantAsync(string name, string value);
    Task SetForTenantAsync(Guid tenantId, string name, string value);
    Task SetForCurrentUserAsync(string name, string value);
    Task SetForUserAsync(Guid userId, string name, string value);
    Task SetGlobalAsync(string name, string value);
    Task<List<SettingValue>> GetAllForCurrentTenantAsync();
    Task<List<SettingValue>> GetAllForCurrentUserAsync();
    Task<List<SettingValue>> GetAllGlobalAsync();
}
