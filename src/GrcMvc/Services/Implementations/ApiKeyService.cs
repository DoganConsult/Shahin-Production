using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// AM-02: API Key management service implementation.
    /// </summary>
    public class ApiKeyService : IApiKeyService
    {
        private readonly GrcDbContext _dbContext;
        private readonly IDistributedCache _cache;
        private readonly IAccessManagementAuditService _auditService;
        private readonly ILogger<ApiKeyService> _logger;

        private const string RateLimitKeyPrefix = "api_key_rate:";
        private const int DefaultMaxRequestsPerHour = 1000;

        public ApiKeyService(
            GrcDbContext dbContext,
            IDistributedCache cache,
            IAccessManagementAuditService auditService,
            ILogger<ApiKeyService> logger)
        {
            _dbContext = dbContext;
            _cache = cache;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<ApiKeyCreationResult> CreateKeyAsync(ApiKeyCreateRequest request, Guid createdBy)
        {
            try
            {
                // Validate request
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    return ApiKeyCreationResult.Failed("API key name is required.");
                }

                // Create the key
                var (apiKey, plaintextKey) = ApiKey.Create(
                    request.Name,
                    createdBy,
                    request.TenantId,
                    request.ExpiresInDays);

                // Set optional properties
                apiKey.Description = request.Description;
                apiKey.MaxTenantsPerDay = request.MaxTenantsPerDay;
                apiKey.MaxRequestsPerHour = request.MaxRequestsPerHour;

                if (request.AllowedDomains?.Length > 0)
                    apiKey.SetAllowedDomains(request.AllowedDomains);

                if (request.AllowedIps?.Length > 0)
                    apiKey.AllowedIpsJson = JsonSerializer.Serialize(request.AllowedIps);

                if (request.Scopes?.Length > 0)
                    apiKey.SetScopes(request.Scopes);

                _dbContext.Set<ApiKey>().Add(apiKey);
                await _dbContext.SaveChangesAsync();

                // Log the creation
                var allowedDomainsStr = request.AllowedDomains?.Length > 0
                    ? string.Join(",", request.AllowedDomains)
                    : null;
                await _auditService.LogApiKeyCreatedAsync(
                    apiKey.Id,
                    request.Name,
                    createdBy,
                    allowedDomainsStr,
                    null);

                _logger.LogInformation(
                    "API key created: {KeyId} ({Name}) by user {CreatedBy}",
                    apiKey.Id, request.Name, createdBy);

                return ApiKeyCreationResult.Succeeded(apiKey.Id, plaintextKey, apiKey.KeyPrefix);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating API key");
                return ApiKeyCreationResult.Failed("Failed to create API key.");
            }
        }

        public async Task<ApiKeyValidationResult> ValidateKeyAsync(string plaintextKey, string? ipAddress = null)
        {
            try
            {
                if (string.IsNullOrEmpty(plaintextKey))
                {
                    return ApiKeyValidationResult.NotFound();
                }

                // Hash the key for lookup
                var keyHash = ApiKey.HashKey(plaintextKey);

                // Find the key
                var apiKey = await _dbContext.Set<ApiKey>()
                    .Where(k => k.KeyHash == keyHash)
                    .FirstOrDefaultAsync();

                if (apiKey == null)
                {
                    _logger.LogWarning("API key validation failed: key not found (prefix: {Prefix})",
                        plaintextKey.Length > 12 ? plaintextKey.Substring(0, 12) : plaintextKey);
                    return ApiKeyValidationResult.NotFound();
                }

                // Check if revoked
                if (apiKey.RevokedAt != null)
                {
                    _logger.LogWarning("API key validation failed: key revoked ({KeyId})", apiKey.Id);
                    return ApiKeyValidationResult.Revoked();
                }

                // Check if expired
                if (apiKey.ExpiresAt != null && apiKey.ExpiresAt <= DateTime.UtcNow)
                {
                    _logger.LogWarning("API key validation failed: key expired ({KeyId})", apiKey.Id);
                    return ApiKeyValidationResult.Expired();
                }

                // Check IP restrictions
                if (!string.IsNullOrEmpty(ipAddress) && !string.IsNullOrEmpty(apiKey.AllowedIpsJson))
                {
                    var allowedIps = JsonSerializer.Deserialize<string[]>(apiKey.AllowedIpsJson);
                    if (allowedIps?.Length > 0 && !IsIpAllowed(ipAddress, allowedIps))
                    {
                        _logger.LogWarning("API key validation failed: IP not allowed ({KeyId}, IP: {Ip})",
                            apiKey.Id, ipAddress);
                        return ApiKeyValidationResult.IpNotAllowed();
                    }
                }

                // Check rate limit
                var rateLimitStatus = await CheckRateLimitAsync(apiKey.Id);
                if (rateLimitStatus.IsLimited)
                {
                    _logger.LogWarning("API key validation failed: rate limited ({KeyId})", apiKey.Id);
                    return ApiKeyValidationResult.RateLimited();
                }

                // Record usage (don't await, fire-and-forget)
                _ = RecordUsageAsync(apiKey.Id, ipAddress, null);

                return ApiKeyValidationResult.Valid(apiKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating API key");
                return ApiKeyValidationResult.Invalid("validation_error", "Failed to validate API key.");
            }
        }

        public async Task<bool> RevokeKeyAsync(Guid keyId, Guid revokedBy, string reason)
        {
            try
            {
                var apiKey = await _dbContext.Set<ApiKey>().FindAsync(keyId);
                if (apiKey == null)
                {
                    return false;
                }

                apiKey.Revoke(revokedBy, reason);
                await _dbContext.SaveChangesAsync();

                // Log the revocation
                await _auditService.LogApiKeyRevokedAsync(
                    keyId,
                    apiKey.Name,
                    apiKey.CreatedBy,
                    revokedBy,
                    reason,
                    null);

                _logger.LogInformation("API key revoked: {KeyId} by user {RevokedBy}. Reason: {Reason}",
                    keyId, revokedBy, reason);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking API key {KeyId}", keyId);
                return false;
            }
        }

        public async Task<IEnumerable<ApiKeyInfo>> GetKeysByTenantAsync(Guid tenantId, bool includeRevoked = false)
        {
            var query = _dbContext.Set<ApiKey>()
                .Where(k => k.TenantId == tenantId);

            if (!includeRevoked)
            {
                query = query.Where(k => k.RevokedAt == null);
            }

            var keys = await query
                .OrderByDescending(k => k.CreatedAt)
                .ToListAsync();

            return keys.Select(ToApiKeyInfo);
        }

        public async Task<ApiKeyInfo?> GetKeyByIdAsync(Guid keyId)
        {
            var key = await _dbContext.Set<ApiKey>().FindAsync(keyId);
            return key == null ? null : ToApiKeyInfo(key);
        }

        public async Task<ApiKeyRateLimitStatus> CheckRateLimitAsync(Guid keyId)
        {
            var key = await _dbContext.Set<ApiKey>()
                .Where(k => k.Id == keyId)
                .Select(k => new { k.MaxRequestsPerHour })
                .FirstOrDefaultAsync();

            var limit = key?.MaxRequestsPerHour ?? DefaultMaxRequestsPerHour;
            var windowStart = DateTime.UtcNow.Date.AddHours(DateTime.UtcNow.Hour);
            var windowEnd = windowStart.AddHours(1);

            var cacheKey = $"{RateLimitKeyPrefix}{keyId}:{windowStart:yyyyMMddHH}";
            var countStr = await _cache.GetStringAsync(cacheKey);
            var count = string.IsNullOrEmpty(countStr) ? 0 : int.Parse(countStr);

            if (count >= limit)
            {
                return ApiKeyRateLimitStatus.Limited(limit, windowEnd);
            }

            return ApiKeyRateLimitStatus.Allowed(count, limit, windowEnd);
        }

        public async Task RecordUsageAsync(Guid keyId, string? ipAddress, string? endpoint)
        {
            try
            {
                // Update key usage in database
                var key = await _dbContext.Set<ApiKey>().FindAsync(keyId);
                if (key != null)
                {
                    key.RecordUsage(ipAddress);
                    await _dbContext.SaveChangesAsync();
                }

                // Increment rate limit counter
                var windowStart = DateTime.UtcNow.Date.AddHours(DateTime.UtcNow.Hour);
                var cacheKey = $"{RateLimitKeyPrefix}{keyId}:{windowStart:yyyyMMddHH}";

                var countStr = await _cache.GetStringAsync(cacheKey);
                var count = string.IsNullOrEmpty(countStr) ? 0 : int.Parse(countStr);

                await _cache.SetStringAsync(cacheKey, (count + 1).ToString(), new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = windowStart.AddHours(2) // Keep for 2 hours
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording API key usage for {KeyId}", keyId);
            }
        }

        public async Task<ApiKeyUsageStats> GetUsageStatsAsync(Guid keyId, int days = 30)
        {
            var key = await _dbContext.Set<ApiKey>().FindAsync(keyId);

            return new ApiKeyUsageStats
            {
                KeyId = keyId,
                TotalRequests = key?.UsageCount ?? 0,
                UniqueIps = 0, // Would need separate tracking table
                DaysActive = days,
                RequestsByEndpoint = new Dictionary<string, int>(),
                RequestsByDay = new Dictionary<string, int>()
            };
        }

        public async Task<bool> UpdateKeyAsync(Guid keyId, ApiKeyUpdateRequest request, Guid updatedBy)
        {
            try
            {
                var key = await _dbContext.Set<ApiKey>().FindAsync(keyId);
                if (key == null)
                {
                    return false;
                }

                if (!string.IsNullOrEmpty(request.Name))
                    key.Name = request.Name;

                if (request.Description != null)
                    key.Description = request.Description;

                if (request.AllowedDomains != null)
                    key.SetAllowedDomains(request.AllowedDomains);

                if (request.AllowedIps != null)
                    key.AllowedIpsJson = JsonSerializer.Serialize(request.AllowedIps);

                if (request.Scopes != null)
                    key.SetScopes(request.Scopes);

                if (request.MaxTenantsPerDay.HasValue)
                    key.MaxTenantsPerDay = request.MaxTenantsPerDay;

                if (request.MaxRequestsPerHour.HasValue)
                    key.MaxRequestsPerHour = request.MaxRequestsPerHour;

                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("API key updated: {KeyId} by user {UpdatedBy}", keyId, updatedBy);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating API key {KeyId}", keyId);
                return false;
            }
        }

        public async Task<bool> IsDomainAllowedAsync(Guid keyId, string domain)
        {
            var key = await _dbContext.Set<ApiKey>()
                .Where(k => k.Id == keyId)
                .Select(k => new { k.AllowedDomainsJson })
                .FirstOrDefaultAsync();

            if (key == null || string.IsNullOrEmpty(key.AllowedDomainsJson))
            {
                return true; // No restrictions
            }

            var allowedDomains = JsonSerializer.Deserialize<string[]>(key.AllowedDomainsJson);
            if (allowedDomains == null || allowedDomains.Length == 0)
            {
                return true;
            }

            return IsDomainMatch(domain, allowedDomains);
        }

        private static bool IsIpAllowed(string ipAddress, string[] allowedIps)
        {
            foreach (var allowed in allowedIps)
            {
                if (allowed == ipAddress)
                    return true;

                // Simple wildcard support (e.g., "192.168.*.*")
                if (allowed.Contains('*'))
                {
                    var pattern = "^" + Regex.Escape(allowed).Replace("\\*", ".*") + "$";
                    if (Regex.IsMatch(ipAddress, pattern))
                        return true;
                }

                // CIDR support would go here
            }

            return false;
        }

        private static bool IsDomainMatch(string domain, string[] allowedDomains)
        {
            domain = domain.ToLowerInvariant();

            foreach (var allowed in allowedDomains)
            {
                var pattern = allowed.ToLowerInvariant();

                if (pattern == domain)
                    return true;

                // Wildcard support (e.g., "*.example.com")
                if (pattern.StartsWith("*."))
                {
                    var suffix = pattern.Substring(1); // ".example.com"
                    if (domain.EndsWith(suffix) || domain == pattern.Substring(2))
                        return true;
                }
            }

            return false;
        }

        private static ApiKeyInfo ToApiKeyInfo(ApiKey key)
        {
            return new ApiKeyInfo
            {
                Id = key.Id,
                Name = key.Name,
                Description = key.Description,
                KeyPrefix = key.KeyPrefix,
                TenantId = key.TenantId,
                AllowedDomains = key.GetAllowedDomains(),
                Scopes = key.GetScopes(),
                MaxTenantsPerDay = key.MaxTenantsPerDay,
                MaxRequestsPerHour = key.MaxRequestsPerHour,
                CreatedAt = key.CreatedAt,
                ExpiresAt = key.ExpiresAt,
                RevokedAt = key.RevokedAt,
                LastUsedAt = key.LastUsedAt,
                UsageCount = key.UsageCount,
                IsActive = key.IsActive
            };
        }
    }
}
