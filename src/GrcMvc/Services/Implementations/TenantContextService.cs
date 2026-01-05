using System;
using System.Security.Claims;
using GrcMvc.Data;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Service to get current tenant context from authenticated user
    /// </summary>
    public class TenantContextService : ITenantContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly GrcDbContext _context;
        private Guid? _cachedTenantId;

        public TenantContextService(IHttpContextAccessor httpContextAccessor, GrcDbContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        public Guid GetCurrentTenantId()
        {
            if (_cachedTenantId.HasValue)
                return _cachedTenantId.Value;

            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Guid.Empty;

            // Get tenant from TenantUser mapping
            var tenantUser = _context.TenantUsers
                .AsNoTracking()
                .FirstOrDefault(tu => tu.UserId == userId && tu.Status == "Active" && !tu.IsDeleted);

            if (tenantUser != null)
            {
                _cachedTenantId = tenantUser.TenantId;
                return tenantUser.TenantId;
            }

            // Fallback: get first active tenant
            var tenant = _context.Tenants
                .AsNoTracking()
                .FirstOrDefault(t => !t.IsDeleted);

            _cachedTenantId = tenant?.Id ?? Guid.Empty;
            return _cachedTenantId.Value;
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
    }
}
