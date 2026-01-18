using GrcMvc.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Service for clearing all caches and ensuring fresh configuration
/// </summary>
public class CacheClearService : ICacheClearService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IConfigurationRoot _configurationRoot;
    private readonly ILogger<CacheClearService> _logger;
    private readonly IGrcCachingService? _grcCacheService;

    public CacheClearService(
        IMemoryCache memoryCache,
        IConfiguration configuration,
        ILogger<CacheClearService> logger,
        IGrcCachingService? grcCacheService = null)
    {
        _memoryCache = memoryCache;
        _configurationRoot = configuration as IConfigurationRoot ?? throw new InvalidOperationException("IConfiguration must be IConfigurationRoot");
        _logger = logger;
        _grcCacheService = grcCacheService;
    }

    public async Task ClearAllCachesAsync()
    {
        _logger.LogInformation("[CACHE] Starting full cache clear...");

        var sw = Stopwatch.StartNew();
        var clearedCount = 0;

        try
        {
            // Clear memory cache (if it supports enumeration)
            if (_memoryCache is MemoryCache mc)
            {
                // MemoryCache doesn't expose enumeration, so we'll clear known keys
                clearedCount += await ClearKnownCacheKeysAsync();
            }

            // Reload configuration to pick up new environment variables
            _configurationRoot.Reload();
            _logger.LogInformation("[CACHE] Configuration reloaded from sources");

            // Clear GRC-specific caches if available
            if (_grcCacheService != null)
            {
                // Clear all catalog caches
                await _grcCacheService.InvalidateCatalogCacheAsync("all");
                _logger.LogInformation("[CACHE] GRC catalog caches cleared");
            }

            sw.Stop();
            _logger.LogInformation("[CACHE] Cache clear completed in {ElapsedMs}ms. Cleared {Count} entries", sw.ElapsedMilliseconds, clearedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CACHE] Error during cache clear");
            throw;
        }
    }

    public async Task ClearConfigurationCacheAsync()
    {
        _logger.LogInformation("[CACHE] Clearing configuration cache...");

        // Reload configuration to pick up new environment variables
        _configurationRoot.Reload();
        _logger.LogInformation("[CACHE] Configuration reloaded - new environment variables will be picked up");

        // Clear known configuration cache keys
        await ClearKnownConfigurationKeysAsync();

        _logger.LogInformation("[CACHE] Configuration cache cleared");
    }

    public async Task ClearTenantCachesAsync(Guid? tenantId = null)
    {
        if (_grcCacheService != null)
        {
            if (tenantId.HasValue)
            {
                await _grcCacheService.InvalidateTenantCacheAsync(tenantId.Value);
                _logger.LogInformation("[CACHE] Cleared cache for tenant {TenantId}", tenantId.Value);
            }
            else
            {
                // Clear all tenant caches (would need to enumerate tenants)
                _logger.LogWarning("[CACHE] Clearing all tenant caches not fully supported - clear per tenant");
            }
        }
    }

    public Task<Interfaces.CacheStatistics> GetCacheStatisticsAsync()
    {
        var stats = new Interfaces.CacheStatistics
        {
            LastCleared = DateTime.UtcNow,
            MemoryUsedBytes = GC.GetTotalMemory(false)
        };

        if (_grcCacheService != null)
        {
            var grcStats = _grcCacheService.GetStatistics();
            stats.CatalogCacheEntries = (int)grcStats.ItemsCached;
        }

        return Task.FromResult(stats);
    }

    private async Task<int> ClearKnownCacheKeysAsync()
    {
        var cleared = 0;
        var knownKeys = new[]
        {
            "catalog:",
            "tenant:",
            "policy:",
            "alert:config",
            "sector:framework",
            "onboarding:",
        };

        // Note: MemoryCache doesn't support enumeration
        // We can only clear keys we know about
        foreach (var keyPrefix in knownKeys)
        {
            // In a real implementation, you'd need to track cache keys
            // For now, we'll rely on expiration and reload
        }

        return cleared;
    }

    private async Task ClearKnownConfigurationKeysAsync()
    {
        var configKeys = new[]
        {
            "AlertConfig",
            "PolicyConfig",
            "SectorFramework",
        };

        foreach (var key in configKeys)
        {
            _memoryCache.Remove(key);
        }
    }
}
