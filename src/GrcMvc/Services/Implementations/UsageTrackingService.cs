using GrcMvc.Data;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Implementation of usage tracking service for tenant trial and subscription management
    /// </summary>
    public class UsageTrackingService : IUsageTrackingService
    {
        private readonly GrcDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UsageTrackingService> _logger;

        public UsageTrackingService(
            GrcDbContext context,
            IHttpContextAccessor httpContextAccessor,
            ILogger<UsageTrackingService> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public bool IsInTrialPeriod()
        {
            var tenantId = GetCurrentTenantId();
            if (!tenantId.HasValue) return false;

            var tenant = _context.Tenants
                .AsNoTracking()
                .FirstOrDefault(t => t.Id == tenantId);

            if (tenant == null) return false;

            return tenant.BillingStatus == "Trialing" || tenant.BillingStatus == "Trial";
        }

        public int GetTrialDaysRemaining()
        {
            var tenantId = GetCurrentTenantId();
            if (!tenantId.HasValue) return 0;

            var tenant = _context.Tenants
                .AsNoTracking()
                .FirstOrDefault(t => t.Id == tenantId);

            if (tenant == null || tenant.TrialEndDate == null) return 0;

            var daysRemaining = (tenant.TrialEndDate.Value - DateTime.UtcNow).Days;
            return Math.Max(0, daysRemaining);
        }

        public bool IsTrialExpired()
        {
            var tenantId = GetCurrentTenantId();
            if (!tenantId.HasValue) return false;

            var tenant = _context.Tenants
                .AsNoTracking()
                .FirstOrDefault(t => t.Id == tenantId);

            if (tenant == null) return false;

            return tenant.BillingStatus == "Expired" || 
                   tenant.BillingStatus == "TrialExpired" ||
                   (tenant.TrialEndDate.HasValue && tenant.TrialEndDate.Value < DateTime.UtcNow);
        }

        public Guid? GetCurrentTenantId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !user.Identity?.IsAuthenticated == true)
                return null;

            var tenantIdClaim = user.FindFirst("TenantId")?.Value;
            if (Guid.TryParse(tenantIdClaim, out var tenantId))
                return tenantId;

            // Try to get from session or other source
            var sessionTenantId = _httpContextAccessor.HttpContext?.Session?.GetString("TenantId");
            if (Guid.TryParse(sessionTenantId, out tenantId))
                return tenantId;

            return null;
        }

        public string GetSubscriptionStatus()
        {
            var tenantId = GetCurrentTenantId();
            if (!tenantId.HasValue) return "Unknown";

            var tenant = _context.Tenants
                .AsNoTracking()
                .FirstOrDefault(t => t.Id == tenantId);

            if (tenant == null) return "Unknown";

            return tenant.BillingStatus ?? "Active";
        }
    }
}
