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
    /// </summary>
    public class TenantContextService : ITenantContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly GrcDbContext _context; // Master DB for tenant metadata
        private readonly ILogger<TenantContextService>? _logger;
        private readonly IServiceProvider _serviceProvider;
        private Guid? _cachedTenantId;

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
            if (_cachedTenantId.HasValue)
                return _cachedTenantId.Value;

            // 1. Try Domain/Subdomain first (for public access - e.g., acme.grcsystem.com)
            var tenantId = ResolveFromDomain();
            if (tenantId != Guid.Empty)
            {
                _cachedTenantId = tenantId;
                return tenantId;
            }

            // 2. Try Claims (for authenticated users - fastest - 0ms)
            tenantId = ResolveFromClaims();
            if (tenantId != Guid.Empty)
            {
                _cachedTenantId = tenantId;
                return tenantId;
            }
            
            // 3. Fallback to database lookup (existing logic - ~50ms)
            // Only happens if domain and claim missing (edge case)
            tenantId = ResolveFromDatabase();
            if (tenantId != Guid.Empty)
            {
                _cachedTenantId = tenantId;
                return tenantId;
            }

            _logger?.LogWarning("Could not resolve tenant from domain, claims, or database");
            return Guid.Empty;
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

            var tenantUser = _context.TenantUsers
                .AsNoTracking()
                .FirstOrDefault(tu => tu.UserId == userId && tu.Status == "Active" && !tu.IsDeleted);

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
