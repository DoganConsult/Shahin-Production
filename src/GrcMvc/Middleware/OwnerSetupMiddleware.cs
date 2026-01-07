using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

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
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IOwnerSetupService ownerSetupService)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";

            // Skip middleware for:
            // - OwnerSetup controller (to avoid redirect loop) - case insensitive
            // - Static files
            // - API endpoints
            // - Health checks
            if (path.StartsWith("/ownersetup", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/_") ||
                path.StartsWith("/css/") ||
                path.StartsWith("/js/") ||
                path.StartsWith("/lib/") ||
                path.StartsWith("/images/") ||
                path.StartsWith("/health", StringComparison.OrdinalIgnoreCase) ||
                path.Equals("/favicon.ico", StringComparison.OrdinalIgnoreCase))
            {
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
                }
                else
                {
                    // #region agent log
                    System.IO.File.AppendAllText("/home/dogan/grc-system/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "owner-setup", runId = "middleware-check", hypothesisId = "D", location = "OwnerSetupMiddleware.InvokeAsync:check-owner", message = "Checking owner existence in middleware", data = new { path }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
                    // #endregion

                    ownerExists = await ownerSetupService.OwnerExistsAsync();
                    _ownerExistsCache = ownerExists;
                    _cacheExpiry = DateTime.UtcNow.Add(CacheDuration);
                }

                // If no owner exists and user is not already on setup page, redirect
                if (!ownerExists && !path.StartsWith("/account/login"))
                {
                    // #region agent log
                    System.IO.File.AppendAllText("/home/dogan/grc-system/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "owner-setup", runId = "middleware-check", hypothesisId = "D", location = "OwnerSetupMiddleware.InvokeAsync:redirect", message = "Redirecting to owner setup", data = new { path, ownerExists }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
                    // #endregion

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
                _logger.LogError(ex, "Error in OwnerSetupMiddleware");
                // On error, continue to next middleware (don't block the app)
            }

            await _next(context);
        }
    }
}
