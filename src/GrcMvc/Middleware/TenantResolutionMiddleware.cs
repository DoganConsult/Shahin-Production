using GrcMvc.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Middleware
{
    /// <summary>
    /// Middleware to resolve tenant from domain/subdomain and store in HttpContext.Items
    /// This provides early tenant resolution before controllers are reached
    /// </summary>
    public class TenantResolutionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TenantResolutionMiddleware> _logger;

        public TenantResolutionMiddleware(
            RequestDelegate next,
            ILogger<TenantResolutionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ITenantContextService tenantContext)
        {
            // Try to resolve tenant from domain (if not already resolved)
            var tenantId = tenantContext.GetCurrentTenantId();
            
            if (tenantId != Guid.Empty)
            {
                // Store in HttpContext.Items for fast access
                context.Items["TenantId"] = tenantId;
                _logger.LogDebug("Tenant {TenantId} resolved and stored in HttpContext", tenantId);
            }

            await _next(context);
        }
    }
}
