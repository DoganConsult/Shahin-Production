using System.Collections.Concurrent;
using System.Text.Json;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// High-performance caching service for sector → framework → evidence mappings
/// Provides sub-millisecond lookups for onboarding and compliance checks
/// </summary>
public class SectorFrameworkCacheService : ISectorFrameworkCacheService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMemoryCache _cache;
    private readonly ILogger<SectorFrameworkCacheService> _logger;

    // Cache keys
    private const string SECTOR_INDEX_KEY = "SectorFrameworkIndex";
    private const string EVIDENCE_CRITERIA_KEY = "EvidenceScoringCriteria";
    private const string SECTOR_BLUEPRINTS_KEY = "SectorBlueprints";

    // Cache duration
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(6);
    private static readonly TimeSpan ShortCacheDuration = TimeSpan.FromMinutes(30);

    // Thread-safe in-memory cache for ultra-fast access
    private static readonly ConcurrentDictionary<string, SectorCacheEntry> _sectorCache = new();
    private static DateTime _lastCacheRefresh = DateTime.MinValue;

    public SectorFrameworkCacheService(
        IServiceProvider serviceProvider,
        IMemoryCache cache,
        ILogger<SectorFrameworkCacheService> logger)
    {
        _serviceProvider = serviceProvider;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Get frameworks for a sector (cached, sub-millisecond)
    /// </summary>
    public async Task<List<SectorFrameworkMapping>> GetFrameworksForSectorAsync(string sectorCode, string? orgType = null)
    {
        var cacheKey = $"{SECTOR_INDEX_KEY}:{sectorCode}:{orgType ?? "ALL"}";

        if (_cache.TryGetValue(cacheKey, out List<SectorFrameworkMapping>? cached) && cached != null)
        {
            return cached;
        }

        // Load from database
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<GrcDbContext>();

        var query = context.Set<SectorFrameworkIndex>()
            .Where(s => s.SectorCode == sectorCode && s.IsActive);

        if (!string.IsNullOrEmpty(orgType))
        {
            query = query.Where(s => s.OrgType == orgType || string.IsNullOrEmpty(s.OrgType));
        }

        var results = await query
            .OrderBy(s => s.Priority)
            .ThenBy(s => s.DisplayOrder)
            .Select(s => new SectorFrameworkMapping
            {
                FrameworkCode = s.FrameworkCode,
                FrameworkNameEn = s.FrameworkNameEn,
                Priority = s.Priority,
                IsMandatory = s.IsMandatory,
                ReasonEn = s.ReasonEn,
                ControlCount = s.ControlCount,
                EvidenceTypeCount = s.EvidenceTypeCount,
                EvidenceTypesJson = s.EvidenceTypesJson,
                ScoringWeight = s.ScoringWeight,
                EstimatedImplementationDays = s.EstimatedImplementationDays
            })
            .ToListAsync();

        // If no indexed data, fall back to ExpertFrameworkMappingService
        if (results.Count == 0)
        {
            var expertService = scope.ServiceProvider.GetRequiredService<IExpertFrameworkMappingService>();
            var blueprint = expertService.GetSectorBlueprint(sectorCode);
            
            results = blueprint.ApplicableFrameworks.Select(f => new SectorFrameworkMapping
            {
                FrameworkCode = f.Code,
                FrameworkNameEn = f.Name,
                Priority = f.Priority,
                IsMandatory = f.Mandatory,
                ReasonEn = f.Reason,
                ControlCount = 0,
                EvidenceTypeCount = 0,
                EvidenceTypesJson = "[]",
                ScoringWeight = 1.0,
                EstimatedImplementationDays = blueprint.EstimatedImplementationMonths * 30
            }).ToList();
        }

        _cache.Set(cacheKey, results, CacheDuration);
        return results;
    }

    /// <summary>
    /// Get evidence scoring criteria (cached)
    /// </summary>
    public async Task<List<EvidenceScoringCriteria>> GetEvidenceScoringCriteriaAsync(string? sector = null, string? framework = null)
    {
        var cacheKey = $"{EVIDENCE_CRITERIA_KEY}:{sector ?? "ALL"}:{framework ?? "ALL"}";

        if (_cache.TryGetValue(cacheKey, out List<EvidenceScoringCriteria>? cached) && cached != null)
        {
            return cached;
        }

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<GrcDbContext>();

        var query = context.Set<EvidenceScoringCriteria>().Where(e => e.IsActive);

        if (!string.IsNullOrEmpty(sector))
        {
            query = query.Where(e => 
                string.IsNullOrEmpty(e.ApplicableSectors) || 
                e.ApplicableSectors.Contains(sector));
        }

        if (!string.IsNullOrEmpty(framework))
        {
            query = query.Where(e => 
                string.IsNullOrEmpty(e.ApplicableFrameworks) || 
                e.ApplicableFrameworks.Contains(framework));
        }

        var results = await query.OrderBy(e => e.DisplayOrder).ToListAsync();

        _cache.Set(cacheKey, results, CacheDuration);
        return results;
    }

    /// <summary>
    /// Get complete sector blueprint with all mappings (cached)
    /// </summary>
    public async Task<SectorComplianceBundle> GetSectorBundleAsync(string sectorCode, string? orgType = null)
    {
        var cacheKey = $"{SECTOR_BLUEPRINTS_KEY}:{sectorCode}:{orgType ?? "ALL"}";

        if (_cache.TryGetValue(cacheKey, out SectorComplianceBundle? cached) && cached != null)
        {
            return cached;
        }

        var frameworks = await GetFrameworksForSectorAsync(sectorCode, orgType);
        var evidenceCriteria = await GetEvidenceScoringCriteriaAsync(sectorCode);

        // Build evidence requirements per framework
        var evidenceByFramework = new Dictionary<string, List<EvidenceRequirementSummary>>();

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<GrcDbContext>();

        foreach (var fw in frameworks)
        {
            // Get distinct evidence types for this framework from controls
            var evidenceTypes = await context.FrameworkControls
                .Where(c => c.FrameworkCode == fw.FrameworkCode && !string.IsNullOrEmpty(c.EvidenceRequirements))
                .Select(c => c.EvidenceRequirements)
                .ToListAsync();

            var distinctEvidence = evidenceTypes
                .SelectMany(e => e.Split('|'))
                .Distinct()
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Select(e => new EvidenceRequirementSummary
                {
                    EvidenceType = e.Trim(),
                    Criteria = evidenceCriteria.FirstOrDefault(c => 
                        c.EvidenceTypeName.Contains(e.Trim(), StringComparison.OrdinalIgnoreCase) ||
                        c.EvidenceTypeCode.Contains(e.Trim().Replace(" ", "_").ToUpper()))
                })
                .ToList();

            evidenceByFramework[fw.FrameworkCode] = distinctEvidence;
        }

        var bundle = new SectorComplianceBundle
        {
            SectorCode = sectorCode,
            OrgType = orgType ?? "ALL",
            Frameworks = frameworks,
            EvidenceByFramework = evidenceByFramework,
            TotalControls = frameworks.Sum(f => f.ControlCount),
            TotalEvidenceTypes = evidenceByFramework.Values.SelectMany(v => v).Select(v => v.EvidenceType).Distinct().Count(),
            ComputedAt = DateTime.UtcNow
        };

        _cache.Set(cacheKey, bundle, CacheDuration);
        return bundle;
    }

    /// <summary>
    /// Invalidate all caches (call after data changes)
    /// </summary>
    public void InvalidateCache()
    {
        _sectorCache.Clear();
        _lastCacheRefresh = DateTime.MinValue;
        _logger.LogInformation("Sector framework cache invalidated");
    }

    /// <summary>
    /// Pre-warm the cache for all sectors
    /// </summary>
    public async Task WarmCacheAsync()
    {
        var sectors = new[] { "Banking", "Healthcare", "Government", "Telecom", "Energy", "Retail", "Technology" };

        foreach (var sector in sectors)
        {
            await GetSectorBundleAsync(sector);
        }

        _logger.LogInformation("Cache warmed for {Count} sectors", sectors.Length);
    }
}

#region DTOs

public class SectorFrameworkMapping
{
    public string FrameworkCode { get; set; } = string.Empty;
    public string FrameworkNameEn { get; set; } = string.Empty;
    public int Priority { get; set; }
    public bool IsMandatory { get; set; }
    public string ReasonEn { get; set; } = string.Empty;
    public int ControlCount { get; set; }
    public int EvidenceTypeCount { get; set; }
    public string EvidenceTypesJson { get; set; } = "[]";
    public double ScoringWeight { get; set; }
    public int EstimatedImplementationDays { get; set; }
}

public class SectorComplianceBundle
{
    public string SectorCode { get; set; } = string.Empty;
    public string OrgType { get; set; } = string.Empty;
    public List<SectorFrameworkMapping> Frameworks { get; set; } = new();
    public Dictionary<string, List<EvidenceRequirementSummary>> EvidenceByFramework { get; set; } = new();
    public int TotalControls { get; set; }
    public int TotalEvidenceTypes { get; set; }
    public DateTime ComputedAt { get; set; }
}

public class EvidenceRequirementSummary
{
    public string EvidenceType { get; set; } = string.Empty;
    public EvidenceScoringCriteria? Criteria { get; set; }
}

public class SectorCacheEntry
{
    public SectorComplianceBundle Bundle { get; set; } = new();
    public DateTime CachedAt { get; set; }
}

#endregion
