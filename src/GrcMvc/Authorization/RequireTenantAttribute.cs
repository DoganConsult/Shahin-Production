using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using GrcMvc.Services.Interfaces;
using System.Security.Claims;

namespace GrcMvc.Authorization
{
    /// <summary>
    /// Validates tenant context from claims against database
    /// Prevents tenant hopping via claim manipulation
    /// CRITICAL FIX: Issue #12 - Tenant context validation
    /// </summary>
    public class RequireTenantAttribute : TypeFilterAttribute
    {
        public RequireTenantAttribute() : base(typeof(RequireTenantFilter))
        {
        }
    }

    public class RequireTenantFilter : IAsyncActionFilter
    {
        private readonly ITenantService _tenantService;
        private readonly ILogger<RequireTenantFilter> _logger;

        public RequireTenantFilter(
            ITenantService tenantService,
            ILogger<RequireTenantFilter> logger)
        {
            _tenantService = tenantService;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            var user = context.HttpContext.User;
            
            // Require authenticated user
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                _logger.LogWarning("Unauthenticated access attempt blocked by RequireTenant");
                context.Result = new UnauthorizedResult();
                return;
            }

            // Get tenant ID from claims
            var tenantIdClaim = user.FindFirst("TenantId")?.Value 
                             ?? user.FindFirst("tenant_id")?.Value;
            
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userEmail = user.FindFirst(ClaimTypes.Email)?.Value;
            
            if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
            {
                _logger.LogWarning(
                    "SECURITY: Tenant ID not found in claims for user {UserId} ({Email})",
                    userId, userEmail);
                
                context.Result = new ForbidResult();
                return;
            }

            try
            {
                // Validate tenant exists and is active
                var tenant = await _tenantService.GetByIdAsync(tenantId);
                if (tenant == null || !tenant.IsActive)
                {
                    _logger.LogWarning(
                        "SECURITY: Tenant validation failed. " +
                        "TenantId: {TenantId}, User: {UserId} ({Email}), " +
                        "TenantExists: {Exists}, TenantActive: {Active}",
                        tenantId, userId, userEmail,
                        tenant != null,
                        tenant?.IsActive ?? false);
                    
                    context.Result = new ForbidResult();
                    return;
                }

                // Validate user belongs to this tenant (database check)
                if (!string.IsNullOrEmpty(userId))
                {
                    var userBelongsToTenant = await _tenantService
                        .UserBelongsToTenantAsync(userId, tenantId);
                    
                    if (!userBelongsToTenant)
                    {
                        var remoteIp = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                        
                        _logger.LogError(
                            "SECURITY ALERT: Tenant hopping attempt detected! " +
                            "User {UserId} ({Email}) attempted to access Tenant {TenantId} " +
                            "without membership. IP: {IP}, Path: {Path}",
                            userId, userEmail, tenantId, remoteIp,
                            context.HttpContext.Request.Path);
                        
                        context.Result = new ForbidResult();
                        return;
                    }
                }

                // Store validated tenant in HttpContext for controllers
                context.HttpContext.Items["ValidatedTenantId"] = tenantId;
                context.HttpContext.Items["ValidatedTenant"] = tenant;
                
                _logger.LogDebug(
                    "Tenant validation passed: TenantId={TenantId}, UserId={UserId}",
                    tenantId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "SECURITY: Error validating tenant {TenantId} for user {UserId}",
                    tenantId, userId);
                
                // Fail closed on errors
                context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
                return;
            }

            await next();
        }
    }
}
