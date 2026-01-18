using GrcMvc.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Volo.Abp.MultiTenancy;

namespace GrcMvc.Middleware
{
    /// <summary>
    /// Middleware to resolve tenant from domain/subdomain and set ABP's ICurrentTenant
    /// This provides early tenant resolution before controllers are reached
    /// Uses ABP's ICurrentTenant.Change() for proper tenant context management
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
            Guid? resolvedTenantId = null;
            
            try
            {
                // Try to resolve tenant from domain (if not already resolved)
                var tenantId = tenantContext.GetCurrentTenantId();
                
                if (tenantId != Guid.Empty)
                {
                    resolvedTenantId = tenantId;
                    // Store in HttpContext.Items for backward compatibility
                    context.Items["TenantId"] = tenantId;
                    _logger.LogDebug("Tenant {TenantId} resolved and stored in HttpContext", tenantId);
                }
                else
                {
                    // For localhost development, use default tenant
                    if (context.Request.Host.Host == "localhost" || context.Request.Host.Host == "127.0.0.1")
                    {
                        // Use a default tenant ID for local development
                        var defaultTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
                        resolvedTenantId = defaultTenantId;
                        context.Items["TenantId"] = defaultTenantId;
                        _logger.LogDebug("Using default tenant for localhost");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error resolving tenant, continuing without tenant context");
                // Don't fail the request, just continue without tenant
            }

        // Use ABP's ICurrentTenant.Change() to set the tenant context
        // This ensures all ABP services automatically use the correct tenant
        // #region agent log
        try { System.IO.File.AppendAllText(@"c:\Shahin-ai\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new{location="TenantResolutionMiddleware.cs:InvokeAsync",message="Before ICurrentTenant.Change()",data=new{resolvedTenantId=resolvedTenantId?.ToString(),currentTenantIdBefore=currentTenant.Id?.ToString()},timestamp=DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),sessionId="debug-session",hypothesisId="E"})+"\n"); } catch {}
        // #endregion
        
        try
        {
            using (currentTenant.Change(resolvedTenantId))
            {
                // #region agent log
                try { System.IO.File.AppendAllText(@"c:\Shahin-ai\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new{location="TenantResolutionMiddleware.cs:InvokeAsync",message="Inside ICurrentTenant.Change() scope",data=new{currentTenantIdInside=currentTenant.Id?.ToString()},timestamp=DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),sessionId="debug-session",hypothesisId="E"})+"\n"); } catch {}
                // #endregion
                
                await _next(context);
            }
        }
        catch (Exception ex)
        {
            // #region agent log
            try { System.IO.File.AppendAllText(@"c:\Shahin-ai\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new{location="TenantResolutionMiddleware.cs:InvokeAsync",message="ICurrentTenant.Change() FAILED",data=new{error=ex.Message,resolvedTenantId=resolvedTenantId?.ToString()},timestamp=DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),sessionId="debug-session",hypothesisId="E"})+"\n"); } catch {}
            // #endregion
            throw;
        }
        }
    }
}
