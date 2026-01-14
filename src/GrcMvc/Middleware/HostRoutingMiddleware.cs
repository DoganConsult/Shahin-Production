using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace GrcMvc.Middleware;

/// <summary>
/// Middleware to route requests based on hostname:
/// - login.shahin-ai.com → Admin Portal (/admin/login)
/// - shahin-ai.com, www.shahin-ai.com → Landing Page (/)
/// - portal.shahin-ai.com, app.shahin-ai.com → Main App
/// </summary>
public class HostRoutingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<HostRoutingMiddleware> _logger;

    public HostRoutingMiddleware(RequestDelegate next, ILogger<HostRoutingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var host = context.Request.Host.Host.ToLowerInvariant();
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "/";


        // login.shahin-ai.com → Redirect to Admin Portal
        if (host == "login.shahin-ai.com")
        {
            // If accessing root, redirect to admin login
            if (path == "/" || path == "")
            {
                context.Response.Redirect("/admin/login", permanent: false);
                return;
            }
            
            // If not already on /admin path, redirect
            if (!path.StartsWith("/admin"))
            {
                context.Response.Redirect("/admin/login", permanent: false);
                return;
            }
        }

        // shahin-ai.com or www.shahin-ai.com → Landing pages only
        // (Allow access to landing, trial, about, pricing, etc. but redirect /admin to login.shahin-ai.com)
        if (host == "shahin-ai.com" || host == "www.shahin-ai.com")
        {
            // Redirect /admin/* to login.shahin-ai.com
            if (path.StartsWith("/admin"))
            {
                var redirectUrl = $"https://login.shahin-ai.com{path}";
                context.Response.Redirect(redirectUrl, permanent: false);
                return;
            }

            // Redirect /owner/* to portal
            if (path.StartsWith("/owner"))
            {
                var redirectUrl = $"https://portal.shahin-ai.com{path}";
                context.Response.Redirect(redirectUrl, permanent: false);
                return;
            }

            // Redirect dashboard/workspace routes to portal
            if (path.StartsWith("/dashboard") || path.StartsWith("/workspace") || 
                path.StartsWith("/tenant") || path.StartsWith("/onboarding"))
            {
                var redirectUrl = $"https://portal.shahin-ai.com{path}";
                context.Response.Redirect(redirectUrl, permanent: false);
                return;
            }
        }


        await _next(context);
    }
}

/// <summary>
/// Extension method to add HostRoutingMiddleware to the pipeline
/// </summary>
public static class HostRoutingMiddlewareExtensions
{
    public static IApplicationBuilder UseHostRouting(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<HostRoutingMiddleware>();
    }
}
