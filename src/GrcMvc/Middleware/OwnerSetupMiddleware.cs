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
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IOwnerSetupService ownerSetupService)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";

            // #region agent log
            try { System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "A", location = "OwnerSetupMiddleware.cs:31", message = "OwnerSetupMiddleware entry", data = new { path = path, method = context.Request.Method, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
            // #endregion

            // Skip middleware for:
            // - OwnerSetup controller (to avoid redirect loop) - case insensitive
            // - Landing page routes (public marketing pages) - should be accessible without owner
            // - Static files
            // - API endpoints
            // - Health checks
            // Path is already lowercased above, so simple string comparison works
            if (path.StartsWith("/ownersetup") ||
                path == "/" ||
                path == "/home" ||
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
                path == "/favicon.ico")
            {
                // #region agent log
                try { System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "A", location = "OwnerSetupMiddleware.cs:64", message = "OwnerSetupMiddleware skipping", data = new { path = path, skipped = true, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                // #endregion
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
                    try { System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "A", location = "OwnerSetupMiddleware.cs:82", message = "Before OwnerExistsAsync check", data = new { path = path, cacheExpired = true, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                    // #endregion
                    _logger.LogDebug("Checking owner existence in middleware for path: {Path}", path);

                    ownerExists = await ownerSetupService.OwnerExistsAsync();
                    // #region agent log
                    try { System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "A", location = "OwnerSetupMiddleware.cs:87", message = "OwnerExistsAsync result", data = new { path = path, ownerExists = ownerExists, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                    // #endregion
                    _ownerExistsCache = ownerExists;
                    _cacheExpiry = DateTime.UtcNow.Add(CacheDuration);
                }

                // If no owner exists and user is not already on setup page, redirect
                if (!ownerExists && !path.StartsWith("/account/login"))
                {
                    // #region agent log
                    try { System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "A", location = "OwnerSetupMiddleware.cs:92", message = "Redirecting to OwnerSetup", data = new { path = path, ownerExists = ownerExists, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                    // #endregion
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
                _logger.LogError(ex, "Error in OwnerSetupMiddleware");
                // On error, continue to next middleware (don't block the app)
            }

            await _next(context);
        }
    }
}
