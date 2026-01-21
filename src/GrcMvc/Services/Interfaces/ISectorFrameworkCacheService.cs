using GrcMvc.Models.Entities;
using GrcMvc.Services.Implementations;

namespace GrcMvc.Services.Interfaces;

/// <summary>
/// High-performance caching service for sector → framework → evidence mappings
/// </summary>
public interface ISectorFrameworkCacheService
{
    /// <summary>
    /// Get frameworks for a sector (cached, sub-millisecond)
    /// </summary>
    Task<List<SectorFrameworkMapping>> GetFrameworksForSectorAsync(string sectorCode, string? orgType = null);

    /// <summary>
    /// Get evidence scoring criteria (cached)
    /// </summary>
    Task<List<EvidenceScoringCriteria>> GetEvidenceScoringCriteriaAsync(string? sector = null, string? framework = null);

    /// <summary>
    /// Get complete sector blueprint with all mappings (cached)
    /// </summary>
    Task<SectorComplianceBundle> GetSectorBundleAsync(string sectorCode, string? orgType = null);

    /// <summary>
    /// Invalidate all caches
    /// </summary>
    void InvalidateCache();

    /// <summary>
    /// Pre-warm the cache
    /// </summary>
    Task WarmCacheAsync();
}
