using GrcMvc.Constants;
using GrcMvc.Data;
using Microsoft.EntityFrameworkCore;

namespace GrcMvc.Middleware
{
    /// <summary>
    /// Middleware to redirect authenticated users to onboarding wizard if onboarding is incomplete.
    /// This provides a secondary guard beyond the post-login redirect in AccountController.
    /// </summary>
    public class OnboardingRedirectMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<OnboardingRedirectMiddleware> _logger;

        public OnboardingRedirectMiddleware(
            RequestDelegate next,
            ILogger<OnboardingRedirectMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, GrcDbContext dbContext)
        {
            // GOLDEN PATH TRACE: Middleware entry
            var path = context.Request.Path.Value?.ToLower() ?? "";
            var isAuthenticated = context.User.Identity?.IsAuthenticated == true;

            // Only check authenticated users
            if (!isAuthenticated)
            {
                _logger.LogDebug("[GOLDEN_PATH] OnboardingRedirectMiddleware: User not authenticated. Path={Path}", path);
                await _next(context);
                return;
            }

            // Skip for routes that should always be accessible
            if (ShouldSkip(path))
            {
                _logger.LogDebug("[GOLDEN_PATH] OnboardingRedirectMiddleware: Skipping path. Path={Path}", path);
                await _next(context);
                return;
            }

            try
            {
                // Get tenant ID from claims
                var tenantIdClaim = context.User.FindFirst(ClaimConstants.TenantId)?.Value
                    ?? context.User.FindFirst("tenant_id")?.Value; // Support both claim names

                if (!Guid.TryParse(tenantIdClaim, out var tenantId))
                {
                    // GOLDEN PATH TRACE: No tenant claim
                    _logger.LogDebug(
                        "[GOLDEN_PATH] OnboardingRedirectMiddleware: No tenant claim found. Path={Path}, User={User}",
                        path, context.User.Identity?.Name);
                    // No tenant claim - user might be platform admin or owner
                    await _next(context);
                    return;
                }

                // GOLDEN PATH TRACE: Tenant ID extracted
                _logger.LogDebug(
                    "[GOLDEN_PATH] OnboardingRedirectMiddleware: TenantId extracted. TenantId={TenantId}, Path={Path}",
                    tenantId, path);

                // DB CHECK: Query tenant with AsNoTracking (tests index on Id + OnboardingStatus)
                var tenantQueryStart = DateTime.UtcNow;
                var tenant = await dbContext.Tenants
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == tenantId && !t.IsDeleted);
                var tenantQueryDuration = (DateTime.UtcNow - tenantQueryStart).TotalMilliseconds;

                _logger.LogInformation(
                    "[DB_CHECK] Middleware tenant query. TenantId={TenantId}, Duration={Duration}ms, Found={Found}, OnboardingStatus={Status}",
                    tenantId, tenantQueryDuration, tenant != null, tenant?.OnboardingStatus ?? "NULL");

                if (tenant == null)
                {
                    // GOLDEN PATH TRACE: Tenant not found
                    _logger.LogWarning(
                        "[GOLDEN_PATH] OnboardingRedirectMiddleware: Tenant not found. TenantId={TenantId}, Path={Path}",
                        tenantId, path);
                    // Tenant not found - continue to handle elsewhere
                    await _next(context);
                    return;
                }

                // GOLDEN PATH TRACE: Tenant found, checking status
                var isCompleted = OnboardingStatus.IsCompleted(tenant.OnboardingStatus);
                _logger.LogInformation(
                    "[GOLDEN_PATH] OnboardingRedirectMiddleware: Tenant found. TenantId={TenantId}, Status={Status}, IsCompleted={IsCompleted}, Path={Path}",
                    tenantId, tenant.OnboardingStatus, isCompleted, path);

                // If onboarding is not completed, redirect to wizard
                if (!isCompleted)
                {
                    _logger.LogInformation(
                        "[GOLDEN_PATH] ✅ MIDDLEWARE REDIRECT: User → OnboardingWizard/Index. TenantId={TenantId}, Status={Status}, Path={Path}",
                        tenantId, tenant.OnboardingStatus, path);

                    context.Response.Redirect($"/OnboardingWizard/Index?tenantId={tenantId}");
                    return;
                }

                // GOLDEN PATH TRACE: Onboarding completed, allowing request
                _logger.LogDebug(
                    "[GOLDEN_PATH] OnboardingRedirectMiddleware: Onboarding completed. Allowing request. TenantId={TenantId}, Path={Path}",
                    tenantId, path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GOLDEN_PATH] ERROR in OnboardingRedirectMiddleware. Path={Path}", path);
                // On error, continue to allow request (don't block the app)
            }

            await _next(context);
        }

        /// <summary>
        /// Determines if the path should skip onboarding check
        /// </summary>
        private static bool ShouldSkip(string path)
        {
            // Skip onboarding routes (avoid redirect loop)
            if (path.StartsWith("/onboarding", StringComparison.OrdinalIgnoreCase))
                return true;

            // Skip authentication routes
            if (path.StartsWith("/account", StringComparison.OrdinalIgnoreCase))
                return true;

            // Skip owner routes (platform admin)
            if (path.StartsWith("/owner", StringComparison.OrdinalIgnoreCase))
                return true;

            // Skip API routes (handle separately with proper error responses)
            if (path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
                return true;

            // Skip static files
            if (path.StartsWith("/css/", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/js/", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/lib/", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/images/", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/_", StringComparison.OrdinalIgnoreCase))
                return true;

            // Skip health checks
            if (path.StartsWith("/health", StringComparison.OrdinalIgnoreCase))
                return true;

            // Skip landing/public pages
            if (path.StartsWith("/landing", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/pricing", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/features", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/about", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/contact", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/grc-free-trial", StringComparison.OrdinalIgnoreCase))
                return true;

            // Skip error pages
            if (path.StartsWith("/error", StringComparison.OrdinalIgnoreCase) ||
                path == "/home/error")
                return true;

            // Skip favicon
            if (path == "/favicon.ico")
                return true;

            // Skip root
            if (path == "/" || path == "/home")
                return true;

            return false;
        }
    }
}
