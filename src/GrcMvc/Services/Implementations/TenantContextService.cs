using System;
using System.Linq;
using System.Security.Claims;
using GrcMvc.Data;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Service to get current tenant context from authenticated user
    /// Supports multi-layer resolution: Domain/Subdomain → Claims → Database
    /// HIGH FIX: Cache is now stored in HttpContext.Items to avoid stale cache issues
    /// </summary>
    public class TenantContextService : ITenantContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly GrcDbContext _context; // Master DB for tenant metadata
        private readonly ILogger<TenantContextService>? _logger;
        private readonly IServiceProvider _serviceProvider;

        // Cache key for HttpContext.Items (per-request cache instead of instance cache)
        private const string TenantIdCacheKey = "__TenantContextService_TenantId";

        public TenantContextService(
            IHttpContextAccessor httpContextAccessor,
            GrcDbContext context,
            IServiceProvider serviceProvider,
            ILogger<TenantContextService>? logger = null)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public Guid GetCurrentTenantId()
        {
            // HIGH FIX: Use HttpContext.Items for per-request caching instead of instance field
            // This prevents stale cache issues when user changes tenant mid-session
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.Items.TryGetValue(TenantIdCacheKey, out var cached) == true && cached is Guid cachedId)
            {
                return cachedId;
            }

            // 1. Try Domain/Subdomain first (for public access - e.g., acme.grcsystem.com)
            var tenantId = ResolveFromDomain();
            if (tenantId != Guid.Empty)
            {
                CacheTenantId(tenantId);
                return tenantId;
            }

            // 2. Try Claims (for authenticated users - fastest - 0ms)
            tenantId = ResolveFromClaims();
            if (tenantId != Guid.Empty)
            {
                CacheTenantId(tenantId);
                return tenantId;
            }

            // 3. Fallback to database lookup (existing logic - ~50ms)
            // Only happens if domain and claim missing (edge case)
            tenantId = ResolveFromDatabase();
            if (tenantId != Guid.Empty)
            {
                CacheTenantId(tenantId);
                return tenantId;
            }

            _logger?.LogWarning("Could not resolve tenant from domain, claims, or database");
            return Guid.Empty;
        }

        /// <summary>
        /// Cache tenant ID in HttpContext.Items for per-request caching.
        /// </summary>
        private void CacheTenantId(Guid tenantId)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                httpContext.Items[TenantIdCacheKey] = tenantId;
            }
        }

        /// <summary>
        /// Clear cached tenant ID (useful when user switches tenant).
        /// </summary>
        public void ClearTenantCache()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                httpContext.Items.Remove(TenantIdCacheKey);
            }
        }

        private Guid ResolveFromDomain()
        {
            var host = _httpContextAccessor.HttpContext?.Request.Host.Host;
            if (string.IsNullOrEmpty(host))
                return Guid.Empty;

            // Extract subdomain (e.g., "acme" from "acme.grcsystem.com")
            var parts = host.Split('.');
            if (parts.Length < 2)
                return Guid.Empty; // No subdomain

            var subdomain = parts[0].ToLower();

            // Skip common subdomains (www, api, admin, etc.)
            var skipSubdomains = new[] { "www", "api", "admin", "app", "portal", "www2", "localhost" };
            if (skipSubdomains.Contains(subdomain))
                return Guid.Empty;

            try
            {
                // Lookup tenant by slug (subdomain matches TenantSlug)
                var tenant = _context.Tenants
                    .AsNoTracking()
                    .FirstOrDefault(t => t.TenantSlug.ToLower() == subdomain && t.IsActive && !t.IsDeleted);

                if (tenant != null)
                {
                    _logger?.LogDebug("Resolved tenant {TenantId} from domain {Domain}", tenant.Id, host);
                    return tenant.Id;
                }

                _logger?.LogDebug("No tenant found for subdomain {Subdomain}", subdomain);
                return Guid.Empty;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Tenant resolution from domain failed for host {Host}. Returning no-tenant context.", host);
                return Guid.Empty;
            }
        }

        private Guid ResolveFromClaims()
        {
            var tenantIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("TenantId");
            if (!string.IsNullOrEmpty(tenantIdClaim) && Guid.TryParse(tenantIdClaim, out var tenantId))
            {
                _logger?.LogDebug("Resolved tenant {TenantId} from claims", tenantId);
                return tenantId;
            }
            return Guid.Empty;
        }

        private Guid ResolveFromDatabase()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                _logger?.LogWarning("GetCurrentTenantId called for unauthenticated user");
                return Guid.Empty;
            }

            // CRITICAL FIX: Use deterministic ordering to ensure consistent tenant selection
            // when user has multiple tenants (order by activation date - most recently activated first, then by creation date)
            var tenantUser = _context.TenantUsers
                .AsNoTracking()
                .Where(tu => tu.UserId == userId && tu.Status == "Active" && !tu.IsDeleted)
                .OrderByDescending(tu => tu.ActivatedAt ?? tu.CreatedDate) // Most recently activated
                .ThenBy(tu => tu.CreatedDate) // Creation date as tiebreaker
                .FirstOrDefault();

            if (tenantUser != null)
            {
                _logger?.LogDebug("Resolved tenant {TenantId} from database for user {UserId}", tenantUser.TenantId, userId);
                return tenantUser.TenantId;
            }

            _logger?.LogWarning("User {UserId} is not associated with any tenant", userId);
            return Guid.Empty;
        }

        public string GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }

        public string GetCurrentUserName()
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System";
        }

        public bool IsAuthenticated()
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
        }

        public string? GetTenantConnectionString()
        {
            var tenantId = GetCurrentTenantId();
            if (tenantId == Guid.Empty) return null;

            var resolver = _serviceProvider.GetRequiredService<ITenantDatabaseResolver>();
            return resolver.GetConnectionString(tenantId);
        }

        public bool HasTenantContext()
        {
            return GetCurrentTenantId() != Guid.Empty;
        }
    }
}
