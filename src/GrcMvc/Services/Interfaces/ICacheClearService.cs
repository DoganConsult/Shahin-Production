namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Service for clearing all caches to ensure fresh configuration
/// </summary>
public interface ICacheClearService
{
    /// <summary>
    /// Clear all caches (memory cache, configuration cache, etc.)
    /// </summary>
    Task ClearAllCachesAsync();

    /// <summary>
    /// Clear configuration-related caches only
    /// </summary>
    Task ClearConfigurationCacheAsync();

    /// <summary>
    /// Clear tenant-specific caches
    /// </summary>
    Task ClearTenantCachesAsync(Guid? tenantId = null);

    /// <summary>
    /// Get cache statistics
    /// </summary>
    Task<CacheStatistics> GetCacheStatisticsAsync();
}

public class CacheStatistics
{
    public int TotalCacheEntries { get; set; }
    public int ConfigurationCacheEntries { get; set; }
    public int TenantCacheEntries { get; set; }
    public int CatalogCacheEntries { get; set; }
    public long MemoryUsedBytes { get; set; }
    public DateTime LastCleared { get; set; }
}
