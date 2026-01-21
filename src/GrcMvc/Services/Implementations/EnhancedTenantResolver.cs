using GrcMvc.Data;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Enhanced tenant resolver - deterministic tenant selection for multi-tenant users
/// Fixes bug: never use FirstOrDefault() without explicit ordering
/// </summary>
public class EnhancedTenantResolver : IEnhancedTenantResolver
{
    private readonly GrcDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEnhancedAuthService _authService;
    private readonly ILogger<EnhancedTenantResolver> _logger;
    
    public EnhancedTenantResolver(
        GrcDbContext context,
        IHttpContextAccessor httpContextAccessor,
        IEnhancedAuthService authService,
        ILogger<EnhancedTenantResolver> logger)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _authService = authService;
        _logger = logger;
    }
    
    public async Task<Guid?> ResolveCurrentTenantAsync(string userId)
    {
        // Priority 1: Route parameter (/t/{tenantSlug}/...)
        var routeTenantId = GetTenantIdFromRoute();
        if (routeTenantId.HasValue)
        {
            _logger.LogDebug("Tenant resolved from route: {TenantId}", routeTenantId);
            await RecordTenantAccessAsync(userId, routeTenantId.Value);
            return routeTenantId;
        }
        
        // Priority 2: Session claim
        var sessionTenantId = await _authService.GetCurrentTenantIdAsync();
        if (sessionTenantId.HasValue)
        {
            _logger.LogDebug("Tenant resolved from session: {TenantId}", sessionTenantId);
            return sessionTenantId;
        }
        
        // Priority 3: Most recently accessed tenant (deterministic)
        var recentTenant = await GetMostRecentlyAccessedTenantAsync(userId);
        if (recentTenant.HasValue)
        {
            _logger.LogDebug("Tenant resolved from recent access: {TenantId}", recentTenant);
            return recentTenant;
        }
        
        // Priority 4: First tenant by creation date (deterministic fallback)
        var firstTenant = await GetFirstTenantByCreationAsync(userId);
        if (firstTenant.HasValue)
        {
            _logger.LogDebug("Tenant resolved from first created: {TenantId}", firstTenant);
            return firstTenant;
        }
        
        _logger.LogWarning("No tenant found for user {UserId}", userId);
        return null;
    }
    
    public async Task<List<TenantAccessInfo>> GetUserTenantsAsync(string userId)
    {
        // Get all tenants for user, ordered by creation date (deterministic)
        var tenants = await _context.TenantUsers
            .Where(tu => tu.UserId == userId && !tu.Tenant.IsDeleted)
            .OrderBy(tu => tu.Tenant.CreatedAt) // Deterministic ordering
            .ThenBy(tu => tu.TenantId)
            .Select(tu => new TenantAccessInfo
            {
                TenantId = tu.TenantId,
                TenantName = tu.Tenant.OrganizationName,
                TenantSlug = tu.Tenant.TenantSlug,
                LastAccessedAt = tu.ActivatedAt, // Use activation date as proxy
                Role = tu.RoleCode
            })
            .ToListAsync();
        
        return tenants;
    }
    
    public async Task RecordTenantAccessAsync(string userId, Guid tenantId)
    {
        // In enhanced version, we'd track access in a separate table
        // For now, this is a no-op to maintain interface compatibility
        await Task.CompletedTask;
        
        _logger.LogDebug("Tenant access tracking placeholder: User {UserId} -> Tenant {TenantId}", 
            userId, tenantId);
    }
    
    private Guid? GetTenantIdFromRoute()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return null;
        
        // Check route values for tenant slug
        if (httpContext.Request.RouteValues.TryGetValue("slug", out var slugObj))
        {
            var slug = slugObj?.ToString();
            if (!string.IsNullOrEmpty(slug))
            {
                // Look up tenant by TenantSlug
                var tenant = _context.Tenants
                    .Where(t => t.TenantSlug == slug && !t.IsDeleted)
                    .Select(t => new { t.Id })
                    .FirstOrDefault();
                
                return tenant?.Id;
            }
        }
        
        // Check query string for tenantId
        if (httpContext.Request.Query.TryGetValue("tenantId", out var tenantIdStr))
        {
            if (Guid.TryParse(tenantIdStr, out var tenantId))
            {
                return tenantId;
            }
        }
        
        return null;
    }
    
    private async Task<Guid?> GetMostRecentlyAccessedTenantAsync(string userId)
    {
        // Use ActivatedAt as proxy for last access
        var tenant = await _context.TenantUsers
            .Where(tu => tu.UserId == userId && !tu.Tenant.IsDeleted && tu.ActivatedAt.HasValue)
            .OrderByDescending(tu => tu.ActivatedAt)
            .ThenBy(tu => tu.Tenant.CreatedAt)
            .Select(tu => tu.TenantId)
            .FirstOrDefaultAsync();
        
        return tenant == Guid.Empty ? null : tenant;
    }
    
    private async Task<Guid?> GetFirstTenantByCreationAsync(string userId)
    {
        var tenant = await _context.TenantUsers
            .Where(tu => tu.UserId == userId && !tu.Tenant.IsDeleted)
            .OrderBy(tu => tu.Tenant.CreatedAt) // Deterministic: oldest first
            .ThenBy(tu => tu.TenantId) // Secondary deterministic tie-breaker
            .Select(tu => tu.TenantId)
            .FirstOrDefaultAsync();
        
        return tenant == Guid.Empty ? null : tenant;
    }
}
