using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Caching service for GRC data with audit trail support.
    /// Caches frequently accessed data like catalogs, org profiles, and derived scope.
    /// </summary>
    public interface IGrcCachingService
    {
        // Catalog caching (Layer 1 - rarely changes)
        Task<List<T>> GetCatalogAsync<T>(string cacheKey, Func<Task<List<T>>> dataLoader) where T : class;
        Task InvalidateCatalogCacheAsync(string catalogType);

        // Tenant scope caching (Layer 2 - changes on scope refresh)
        Task<T?> GetTenantDataAsync<T>(Guid tenantId, string dataType, Func<Task<T?>> dataLoader) where T : class;
        Task InvalidateTenantCacheAsync(Guid tenantId, string? dataType = null);

        // Policy decision caching with audit
        Task<PolicyDecision> GetOrCreatePolicyDecisionAsync(Guid tenantId, string policyType, string context, Func<Task<PolicyDecisionResult>> evaluator);
        Task<List<PolicyDecision>> GetPolicyDecisionHistoryAsync(Guid tenantId, string? policyType = null, int limit = 100);

        // Cache statistics
        CacheStatistics GetStatistics();
    }

    public class GrcCachingService : IGrcCachingService
    {
        private readonly IMemoryCache _cache;
        private readonly GrcDbContext _context;
        private readonly ILogger<GrcCachingService> _logger;
        private readonly CacheStatistics _stats = new();

        // Cache duration settings
        private static readonly TimeSpan CatalogCacheDuration = TimeSpan.FromHours(24);
        private static readonly TimeSpan TenantDataCacheDuration = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan PolicyDecisionCacheDuration = TimeSpan.FromMinutes(15);

        public GrcCachingService(
            IMemoryCache cache,
            GrcDbContext context,
            ILogger<GrcCachingService> logger)
        {
            _cache = cache;
            _context = context;
            _logger = logger;
        }

        #region Catalog Caching (Layer 1)

        public async Task<List<T>> GetCatalogAsync<T>(string cacheKey, Func<Task<List<T>>> dataLoader) where T : class
        {
            var fullKey = $"catalog:{cacheKey}";

            if (_cache.TryGetValue(fullKey, out List<T>? cached) && cached != null)
            {
                _stats.CacheHits++;
                _logger.LogDebug("Cache HIT for catalog {CacheKey}", cacheKey);
                return cached;
            }

            _stats.CacheMisses++;
            _logger.LogDebug("Cache MISS for catalog {CacheKey}, loading from database", cacheKey);

            var data = await dataLoader();

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(CatalogCacheDuration)
                .SetPriority(CacheItemPriority.High);

            _cache.Set(fullKey, data, cacheOptions);
            _stats.ItemsCached++;

            return data;
        }

        public Task InvalidateCatalogCacheAsync(string catalogType)
        {
            var key = $"catalog:{catalogType}";
            _cache.Remove(key);
            _stats.Invalidations++;
            _logger.LogInformation("Invalidated catalog cache: {CatalogType}", catalogType);
            return Task.CompletedTask;
        }

        #endregion

        #region Tenant Data Caching (Layer 2)

        public async Task<T?> GetTenantDataAsync<T>(Guid tenantId, string dataType, Func<Task<T?>> dataLoader) where T : class
        {
            var fullKey = $"tenant:{tenantId}:{dataType}";

            if (_cache.TryGetValue(fullKey, out T? cached))
            {
                _stats.CacheHits++;
                _logger.LogDebug("Cache HIT for tenant data {TenantId}:{DataType}", tenantId, dataType);
                return cached;
            }

            _stats.CacheMisses++;
            _logger.LogDebug("Cache MISS for tenant data {TenantId}:{DataType}, loading", tenantId, dataType);

            var data = await dataLoader();

            if (data != null)
            {
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TenantDataCacheDuration)
                    .SetPriority(CacheItemPriority.Normal);

                _cache.Set(fullKey, data, cacheOptions);
                _stats.ItemsCached++;
            }

            return data;
        }

        public Task InvalidateTenantCacheAsync(Guid tenantId, string? dataType = null)
        {
            if (string.IsNullOrEmpty(dataType))
            {
                // Invalidate all tenant data - use pattern matching
                var patterns = new[] { "OrgProfile", "Scope", "Teams", "RACI", "Workflows" };
                foreach (var pattern in patterns)
                {
                    _cache.Remove($"tenant:{tenantId}:{pattern}");
                }
                _logger.LogInformation("Invalidated all cache for tenant {TenantId}", tenantId);
            }
            else
            {
                _cache.Remove($"tenant:{tenantId}:{dataType}");
                _logger.LogInformation("Invalidated cache {DataType} for tenant {TenantId}", dataType, tenantId);
            }
            _stats.Invalidations++;
            return Task.CompletedTask;
        }

        #endregion

        #region Policy Decision Caching & Audit

        public async Task<PolicyDecision> GetOrCreatePolicyDecisionAsync(
            Guid tenantId,
            string policyType,
            string context,
            Func<Task<PolicyDecisionResult>> evaluator)
        {
            var contextHash = ComputeContextHash(context);
            var cacheKey = $"policy:{tenantId}:{policyType}:{contextHash}";

            // Check cache first
            if (_cache.TryGetValue(cacheKey, out PolicyDecision? cachedDecision) && cachedDecision != null)
            {
                _stats.CacheHits++;
                _logger.LogDebug("Policy decision cache HIT: {PolicyType} for tenant {TenantId}", policyType, tenantId);
                return cachedDecision;
            }

            _stats.CacheMisses++;

            // Evaluate policy
            var result = await evaluator();

            // Create audit record
            var decision = new PolicyDecision
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                PolicyType = policyType,
                PolicyVersion = result.PolicyVersion,
                ContextHash = contextHash,
                ContextJson = context,
                Decision = result.Decision,
                Reason = result.Reason,
                RulesEvaluated = result.RulesEvaluated,
                RulesMatched = result.RulesMatched,
                ConfidenceScore = result.ConfidenceScore,
                EvaluatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.Add(PolicyDecisionCacheDuration),
                IsCached = true,
                CreatedDate = DateTime.UtcNow
            };

            // Persist to audit trail
            _context.PolicyDecisions.Add(decision);
            await _context.SaveChangesAsync();

            // Cache the decision
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(PolicyDecisionCacheDuration)
                .SetPriority(CacheItemPriority.Normal);

            _cache.Set(cacheKey, decision, cacheOptions);
            _stats.ItemsCached++;

            _logger.LogInformation(
                "Policy decision made: {PolicyType} = {Decision} for tenant {TenantId} (Confidence: {Confidence}%)",
                policyType, result.Decision, tenantId, result.ConfidenceScore);

            return decision;
        }

        public async Task<List<PolicyDecision>> GetPolicyDecisionHistoryAsync(
            Guid tenantId,
            string? policyType = null,
            int limit = 100)
        {
            var query = _context.PolicyDecisions
                .Where(p => p.TenantId == tenantId);

            if (!string.IsNullOrEmpty(policyType))
            {
                query = query.Where(p => p.PolicyType == policyType);
            }

            return await query
                .OrderByDescending(p => p.EvaluatedAt)
                .Take(limit)
                .ToListAsync();
        }

        private string ComputeContextHash(string context)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(context);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash)[..16];
        }

        #endregion

        #region Statistics

        public CacheStatistics GetStatistics()
        {
            _stats.HitRatio = _stats.CacheHits + _stats.CacheMisses > 0
                ? (double)_stats.CacheHits / (_stats.CacheHits + _stats.CacheMisses) * 100
                : 0;
            return _stats;
        }

        #endregion
    }

    #region Supporting Types

    public class PolicyDecisionResult
    {
        public string Decision { get; set; } = string.Empty; // Allow, Deny, Require, Skip
        public string Reason { get; set; } = string.Empty;
        public string PolicyVersion { get; set; } = "1.0";
        public int RulesEvaluated { get; set; }
        public int RulesMatched { get; set; }
        public int ConfidenceScore { get; set; } // 0-100
    }

    public class CacheStatistics
    {
        public long CacheHits { get; set; }
        public long CacheMisses { get; set; }
        public long ItemsCached { get; set; }
        public long Invalidations { get; set; }
        public double HitRatio { get; set; }
    }

    #endregion
}
