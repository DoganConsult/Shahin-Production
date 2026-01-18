using Microsoft.AspNetCore.Http;
using GrcMvc.Services.Interfaces;
using System.Security.Claims;

namespace GrcMvc.Middleware
{
    /// <summary>
    /// Trial Enforcement Middleware
    /// Blocks access for tenants with expired trial periods
    /// CRITICAL FIX: Issue #3 - Enforce trial expiration
    /// </summary>
    public class TrialEnforcementMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TrialEnforcementMiddleware> _logger;

        // Paths that should not be blocked even for expired trials
        private static readonly HashSet<string> _allowedPaths = new(StringComparer.OrdinalIgnoreCase)
        {
            "/api/payment",
            "/api/subscription",
            "/subscription/plans",
            "/subscription/expired",
            "/account/login",
            "/account/logout",
            "/account/accessdenied",
            "/health",
            "/healthcheck"
        };

        public TrialEnforcementMiddleware(
            RequestDelegate next,
            ILogger<TrialEnforcementMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(
            HttpContext context,
            ITrialLifecycleService trialService)
        {
            // Skip for anonymous requests
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                await _next(context);
                return;
            }

            // Skip for allowed paths
            var path = context.Request.Path.Value?.ToLower() ?? "";
            if (_allowedPaths.Any(allowedPath => path.StartsWith(allowedPath, StringComparison.OrdinalIgnoreCase)))
            {
                await _next(context);
                return;
            }

            // Skip for static files
            if (path.Contains("/css/") || path.Contains("/js/") || 
                path.Contains("/images/") || path.Contains("/lib/"))
            {
                await _next(context);
                return;
            }

            // Get tenant from claims
            var tenantIdClaim = context.User.FindFirst("TenantId")?.Value
                             ?? context.User.FindFirst("tenant_id")?.Value;
            
            if (string.IsNullOrEmpty(tenantIdClaim))
            {
                // No tenant claim - might be platform admin or single-tenant mode
                await _next(context);
                return;
            }

            if (!Guid.TryParse(tenantIdClaim, out var tenantId))
            {
                _logger.LogWarning(
                    "Invalid tenant ID claim: {TenantIdClaim}",
                    tenantIdClaim);
                await _next(context);
                return;
            }

            try
            {
                // Check trial status
                var isTrialExpired = await trialService.IsTrialExpiredAsync(tenantId);
                
                if (isTrialExpired)
                {
                    var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var userEmail = context.User.FindFirstValue(ClaimTypes.Email);
                    
                    _logger.LogWarning(
                        "TRIAL EXPIRED: Access blocked for tenant {TenantId}, " +
                        "user {UserId} ({Email}), path: {Path}",
                        tenantId, userId, userEmail, path);

                    // Return different response based on request type
                    if (context.Request.Path.StartsWithSegments("/api"))
                    {
                        // API request - return 402 Payment Required
                        context.Response.StatusCode = StatusCodes.Status402PaymentRequired;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsJsonAsync(new
                        {
                            error = "Trial Expired",
                            message = "Your trial period has ended. Please subscribe to continue using the service.",
                            subscriptionUrl = "/subscription/plans",
                            tenantId
                        });
                        return;
                    }
                    else
                    {
                        // Web request - redirect to subscription page
                        context.Response.Redirect("/subscription/expired");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error checking trial status for tenant {TenantId}",
                    tenantId);
                
                // On error, allow access (fail open for availability)
                // Log the error for investigation
                _logger.LogWarning(
                    "Trial enforcement check failed, allowing access as fallback");
            }

            await _next(context);
        }
    }

    /// <summary>
    /// Extension method for easy middleware registration
    /// </summary>
    public static class TrialEnforcementMiddlewareExtensions
    {
        public static IApplicationBuilder UseTrialEnforcement(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TrialEnforcementMiddleware>();
        }
    }
}
