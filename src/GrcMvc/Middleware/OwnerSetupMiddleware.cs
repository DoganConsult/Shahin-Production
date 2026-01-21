using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;

namespace GrcMvc.Middleware
{
    /// <summary>
    /// Middleware to check if owner setup is required and redirect to setup page
    /// This runs before authentication to allow owner setup when no owner exists
    /// </summary>
    public class OwnerSetupMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<OwnerSetupMiddleware> _logger;
        private static bool? _ownerExistsCache = null;
        private static DateTime _cacheExpiry = DateTime.MinValue;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(1);

        public OwnerSetupMiddleware(
            RequestDelegate next,
            ILogger<OwnerSetupMiddleware> logger)
        {
            // #region agent log - Use ILogger for Docker visibility
            logger.LogInformation("OwnerSetupMiddleware: Constructor called | nextExists={NextExists} | loggerExists={LoggerExists}", 
                next != null, logger != null);
            // #endregion
            _next = next;
            _logger = logger;
            _logger.LogDebug("OwnerSetupMiddleware initialized");
        }

        public async Task InvokeAsync(HttpContext context, IOwnerSetupService ownerSetupService)
        {
            // #region agent log - Use ILogger for Docker visibility
            _logger.LogInformation("OwnerSetupMiddleware: InvokeAsync entry | path={Path} | method={Method} | ownerSetupServiceExists={ServiceExists}", 
                context.Request.Path.Value, context.Request.Method, ownerSetupService != null);
            // #endregion
            var path = context.Request.Path.Value?.ToLower() ?? "";

            // #region agent log - Use ILogger instead of file (works in Docker)
            _logger.LogInformation("OwnerSetupMiddleware: After path lowercasing | path={Path} | originalPath={OriginalPath} | pathLength={PathLength}", 
                path, context.Request.Path.Value, path.Length);
            // #endregion

            _logger.LogDebug("OwnerSetupMiddleware processing path: {Path}", path);

            // Skip middleware for:
            // - OwnerSetup controller (to avoid redirect loop) - case insensitive
            // - Landing page routes (public marketing pages) - should be accessible without owner
            // - Static files
            // - API endpoints
            // - Health checks
            // Path is already lowercased above, so simple string comparison works
            bool pathEqualsSlash = path == "/";
            bool pathStartsWithOwnersetup = path.StartsWith("/ownersetup", StringComparison.OrdinalIgnoreCase);
            
            bool shouldSkip = pathStartsWithOwnersetup ||
                pathEqualsSlash ||
                path == "/home" ||
                path == "/home/error" ||
                path.StartsWith("/error") ||
                path.StartsWith("/landing/") ||
                path.StartsWith("/pricing") ||
                path.StartsWith("/features") ||
                path.StartsWith("/about") ||
                path.StartsWith("/contact") ||
                path.StartsWith("/case-studies") ||
                path.StartsWith("/grc-free-trial") ||
                path.StartsWith("/best-grc-software") ||
                path.StartsWith("/why-our-grc") ||
                path.StartsWith("/grc-for-") ||
                path.StartsWith("/api/") ||
                path.StartsWith("/_") ||
                path.StartsWith("/css/") ||
                path.StartsWith("/js/") ||
                path.StartsWith("/lib/") ||
                path.StartsWith("/images/") ||
                path.StartsWith("/health") ||
                path == "/favicon.ico";

            // #region agent log - Use ILogger for Docker visibility
            _logger.LogInformation("OwnerSetupMiddleware: Path skip check result | path={Path} | shouldSkip={ShouldSkip} | pathEqualsSlash={PathEqualsSlash} | pathStartsWithOwnersetup={PathStartsWithOwnersetup}", 
                path, shouldSkip, pathEqualsSlash, pathStartsWithOwnersetup);
            // #endregion

            if (shouldSkip)
            {
                _logger.LogInformation("OwnerSetupMiddleware: SKIPPING path {Path} - shouldSkip={ShouldSkip}", path, shouldSkip);
                await _next(context);
                return;
            }

            try
            {
                // Check cache first (to avoid DB query on every request)
                bool ownerExists;
                if (_ownerExistsCache.HasValue && DateTime.UtcNow < _cacheExpiry)
                {
                    ownerExists = _ownerExistsCache.Value;
                    _logger.LogDebug("Owner existence from cache: {OwnerExists}", ownerExists);
                }
                else
                {
                    _logger.LogDebug("Checking owner existence in database for path: {Path}", path);
                    ownerExists = await ownerSetupService.OwnerExistsAsync();
                    _ownerExistsCache = ownerExists;
                    _cacheExpiry = DateTime.UtcNow.Add(CacheDuration);
                    _logger.LogDebug("Owner existence check result: {OwnerExists}", ownerExists);
                }

                // If no owner exists and user is not already on setup page, redirect
                if (!ownerExists && !path.StartsWith("/account/login"))
                {
                    _logger.LogInformation("Redirecting to owner setup page. Path: {Path}, OwnerExists: {OwnerExists}", path, ownerExists);
                    context.Response.Redirect("/OwnerSetup");
                    return;
                }

                // Clear cache if owner was just created (to allow immediate access)
                if (ownerExists && _ownerExistsCache.HasValue && !_ownerExistsCache.Value)
                {
                    _ownerExistsCache = null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OwnerSetupMiddleware for path: {Path}", path);
                // On error, continue to next middleware (don't block the app)
            }

            await _next(context);
        }
    }
}
