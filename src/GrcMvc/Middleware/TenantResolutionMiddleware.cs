using GrcMvc.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Volo.Abp.MultiTenancy;

namespace GrcMvc.Middleware
{
    /// <summary>
    /// Middleware to resolve tenant from domain/subdomain and integrate with ABP multi-tenancy.
    /// This provides early tenant resolution before controllers are reached.
    /// Uses both custom ITenantContextService and ABP's ICurrentTenant for compatibility.
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

        public async Task InvokeAsync(
            HttpContext context,
            ITenantContextService tenantContext,
            ICurrentTenant currentTenant)
        {
            // OPTIMIZATION: Skip tenant resolution for admin and login paths (no tenant needed)
            // Check host directly first (runs before HostRoutingMiddleware, so check host here)
            var host = context.Request.Host.Host.ToLowerInvariant();
            if (host == "admin.shahin-ai.com" || host == "login.shahin-ai.com")
            {
                var pathType = host == "admin.shahin-ai.com" ? "Platform Admin" : "Login";
                _logger.LogDebug("[GOLDEN_PATH] Skipping tenant resolution for {PathType} path. Host={Host}, Path={Path}", 
                    pathType, host, context.Request.Path);
                // Set flag for downstream middleware and services
                context.Items["SkipTenantResolution"] = true;
                if (host == "admin.shahin-ai.com")
                    context.Items["IsPlatformAdminPath"] = true;
                else
                    context.Items["IsLoginPath"] = true;
                await _next(context);
                return;
            }

            // Also check if flag was set by HostRoutingMiddleware (if it runs before this)
            if (context.Items.ContainsKey("SkipTenantResolution") && 
                context.Items["SkipTenantResolution"] is bool skip && skip)
            {
                var pathType = context.Items.ContainsKey("IsPlatformAdminPath") ? "Platform Admin" :
                              context.Items.ContainsKey("IsLoginPath") ? "Login" : "Unknown";
                _logger.LogDebug("[GOLDEN_PATH] Skipping tenant resolution for {PathType} path. Path={Path}", 
                    pathType, context.Request.Path);
                await _next(context);
                return;
            }

            // Resolve tenant from domain using custom service
            // OPTIMIZATION: TenantContextService will check SkipTenantResolution flag internally
            var tenantId = tenantContext.GetCurrentTenantId();

            if (tenantId != Guid.Empty)
            {
                // Store in HttpContext.Items for fast access (backward compatibility)
                context.Items["TenantId"] = tenantId;

                // Set ABP's current tenant context
                using (currentTenant.Change(tenantId))
                {
                    _logger.LogDebug("Tenant {TenantId} resolved and set in both HttpContext and ABP ICurrentTenant", tenantId);
                    await _next(context);
                }
                return;
            }

            // No tenant context - proceed without tenant isolation
            await _next(context);
        }
    }
}
