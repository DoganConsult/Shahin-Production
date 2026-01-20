using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;

namespace GrcMvc.Middleware;

/// <summary>
/// Middleware to route requests based on hostname:
/// - admin.shahin-ai.com → Platform Admin Portal (/admin/*) - Platform Admins only
/// - login.shahin-ai.com → Login Page (/Account/Login) - All users (tenant users + platform admins)
/// - shahin-ai.com, www.shahin-ai.com → Landing Page (proxied to Next.js frontend)
/// - portal.shahin-ai.com, app.shahin-ai.com → Main App
/// </summary>
public class HostRoutingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<HostRoutingMiddleware> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private static readonly string FrontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "http://localhost:3003";
    private static readonly string FrontendPath = Environment.GetEnvironmentVariable("FRONTEND_PATH") ?? @"C:\Shahin-ai\Shahin-Jan-2026\grc-frontend";

    public HostRoutingMiddleware(
        RequestDelegate next, 
        ILogger<HostRoutingMiddleware> logger,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var host = context.Request.Host.Host.ToLowerInvariant();
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "/";

        // admin.shahin-ai.com → Platform Admin Portal (Platform Admins only)
        // OPTIMIZATION: Skip tenant resolution - Platform Admins don't need tenant context
        if (host == "admin.shahin-ai.com")
        {
            _logger.LogInformation("[ADMIN_PATH] Platform Admin Portal access. Host={Host}, Path={Path}, Method={Method}", 
                host, path, context.Request.Method);
            
            // Mark as admin path to skip tenant resolution
            context.Items["SkipTenantResolution"] = true;
            context.Items["IsPlatformAdminPath"] = true;
            
            // If accessing root, redirect to admin dashboard
            if (path == "/" || path == "")
            {
                context.Response.Redirect("/admin/dashboard", permanent: false);
                return;
            }
            
            // Ensure all requests go to /admin/* routes
            if (!path.StartsWith("/admin"))
            {
                context.Response.Redirect($"/admin{path}", permanent: false);
                return;
            }
            
            // Allow /admin/* routes to proceed (authorization handled by AdminPortalController)
            // No tenant resolution needed - proceed immediately
            await _next(context);
            return;
        }

        // login.shahin-ai.com → Login Page for All Users (tenant users + platform admins)
        // OPTIMIZATION: Skip tenant resolution for login paths - tenant resolved after authentication
        if (host == "login.shahin-ai.com")
        {
            _logger.LogInformation("[GOLDEN_PATH] Login portal access. Host={Host}, Path={Path}, Method={Method}, Query={Query}", 
                host, path, context.Request.Method, context.Request.QueryString);
            
            // Mark as login path to skip tenant resolution (tenant resolved after login)
            context.Items["SkipTenantResolution"] = true;
            context.Items["IsLoginPath"] = true;
            
            // If accessing root, redirect to login page
            if (path == "/" || path == "")
            {
                context.Response.Redirect("/Account/Login", permanent: false);
                return;
            }
            
            // Redirect /admin/* to admin.shahin-ai.com (platform admin portal)
            if (path.StartsWith("/admin"))
            {
                var adminUrl = _configuration["App:AdminUrl"] ?? 
                              _configuration["App:BaseUrl"]?.Replace("portal", "admin") ?? 
                              "https://admin.shahin-ai.com";
                var redirectUrl = $"{adminUrl}{path}";
                context.Response.Redirect(redirectUrl, permanent: false);
                return;
            }
            
            // Allow /Account/Login and other account routes to proceed
            // Redirect dashboard/workspace routes to portal
            if (path.StartsWith("/dashboard") || path.StartsWith("/workspace") || 
                path.StartsWith("/tenant") || path.StartsWith("/onboarding"))
            {
                var baseUrl = _configuration["App:BaseUrl"] ?? "https://portal.shahin-ai.com";
                var redirectUrl = $"{baseUrl}{path}";
                context.Response.Redirect(redirectUrl, permanent: false);
                return;
            }
            
            // Login paths don't need tenant resolution - proceed immediately
            await _next(context);
            return;
        }

        // shahin-ai.com or www.shahin-ai.com → Landing pages only (proxied to Next.js frontend)
        // (Allow access to landing, trial, about, pricing, etc. but redirect /admin to admin.shahin-ai.com)
        if (host == "shahin-ai.com" || host == "www.shahin-ai.com" || host == "localhost")
        {
            // Redirect /admin/* to admin.shahin-ai.com (platform admin portal)
            if (path.StartsWith("/admin"))
            {
                var adminUrl = _configuration["App:AdminUrl"] ?? 
                              _configuration["App:BaseUrl"]?.Replace("portal", "admin") ?? 
                              "https://admin.shahin-ai.com";
                var redirectUrl = $"{adminUrl}{path}";
                context.Response.Redirect(redirectUrl, permanent: false);
                return;
            }
            
            // Redirect /Account/Login to login.shahin-ai.com (all users login)
            if (path.StartsWith("/account/login", StringComparison.OrdinalIgnoreCase))
            {
                var loginUrl = _configuration["App:LoginUrl"] ?? 
                              _configuration["App:BaseUrl"]?.Replace("portal", "login") ?? 
                              "https://login.shahin-ai.com";
                var redirectUrl = $"{loginUrl}{path}";
                context.Response.Redirect(redirectUrl, permanent: false);
                return;
            }

            // Redirect /owner/* to portal
            if (path.StartsWith("/owner"))
            {
                var baseUrl = _configuration["App:BaseUrl"] ?? "https://portal.shahin-ai.com";
                var redirectUrl = $"{baseUrl}{path}";
                context.Response.Redirect(redirectUrl, permanent: false);
                return;
            }

            // Redirect dashboard/workspace routes to portal
            if (path.StartsWith("/dashboard") || path.StartsWith("/workspace") || 
                path.StartsWith("/tenant") || path.StartsWith("/onboarding"))
            {
                var baseUrl = _configuration["App:BaseUrl"] ?? "https://portal.shahin-ai.com";
                var redirectUrl = $"{baseUrl}{path}";
                context.Response.Redirect(redirectUrl, permanent: false);
                return;
            }

            // Proxy landing page requests to Next.js frontend
            // Skip API routes and static files - let backend handle those
            if (!path.StartsWith("/api") && !path.StartsWith("/_next") && 
                !path.StartsWith("/static") && !path.Contains("."))
            {
                try
                {
                    _logger.LogDebug("[GOLDEN_PATH] Proxying landing page request to frontend. Path={Path}, FrontendUrl={FrontendUrl}", 
                        path, FrontendUrl);
                    
                    var client = _httpClientFactory.CreateClient();
                    client.Timeout = TimeSpan.FromSeconds(30);
                    
                    // Build the frontend URL
                    var frontendRequestUrl = $"{FrontendUrl}{context.Request.Path}{context.Request.QueryString}";
                    
                    // Create the request
                    var request = new HttpRequestMessage(HttpMethod.Get, frontendRequestUrl);
                    
                    // Copy headers (except host)
                    foreach (var header in context.Request.Headers)
                    {
                        if (header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase))
                            continue;
                        if (header.Key.Equals("Connection", StringComparison.OrdinalIgnoreCase))
                            continue;
                        try
                        {
                            request.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                        }
                        catch { }
                    }
                    
                    // Send request to frontend
                    var response = await client.SendAsync(request);
                    
                    // Copy response
                    context.Response.StatusCode = (int)response.StatusCode;
                    foreach (var header in response.Headers)
                    {
                        context.Response.Headers[header.Key] = header.Value.ToArray();
                    }
                    foreach (var header in response.Content.Headers)
                    {
                        context.Response.Headers[header.Key] = header.Value.ToArray();
                    }
                    
                    // Copy response body
                    var content = await response.Content.ReadAsByteArrayAsync();
                    await context.Response.Body.WriteAsync(content, 0, content.Length);
                    
                    _logger.LogDebug("[GOLDEN_PATH] Successfully proxied landing page. Path={Path}, StatusCode={StatusCode}", 
                        path, response.StatusCode);
                    
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[GOLDEN_PATH] Failed to proxy to frontend. Path={Path}, FrontendUrl={FrontendUrl}. Falling back to backend.", 
                        path, FrontendUrl);
                    // Fall through to backend if frontend is not available
                }
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
