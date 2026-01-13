using System.Security.Cryptography;

namespace GrcMvc.Middleware;

/// <summary>
/// Middleware to add security headers to all HTTP responses
/// Implements OWASP security best practices with nonce-based CSP
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityHeadersMiddleware> _logger;

    public SecurityHeadersMiddleware(RequestDelegate next, ILogger<SecurityHeadersMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Generate cryptographically secure nonce for CSP
        var nonce = GenerateNonce();
        context.Items["CSPNonce"] = nonce;

        // Remove server header to avoid information disclosure
        context.Response.Headers.Remove("Server");
        context.Response.Headers.Remove("X-Powered-By");

        // Prevent clickjacking attacks
        context.Response.Headers.Append("X-Frame-Options", "DENY");

        // Prevent MIME type sniffing
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

        // Enable XSS filter in browsers
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");

        // Control referrer information
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

        // Disable dangerous browser features
        context.Response.Headers.Append("Permissions-Policy",
            "accelerometer=(), camera=(), geolocation=(), gyroscope=(), " +
            "magnetometer=(), microphone=(), payment=(), usb=()");

        // Content Security Policy - Nonce-based (XSS protection)
        // REMOVED: 'unsafe-inline' and 'unsafe-eval' - replaced with nonce-based CSP
        context.Response.Headers.Append("Content-Security-Policy",
            "default-src 'self'; " +
            $"script-src 'self' 'nonce-{nonce}' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com; " +
            "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com https://fonts.googleapis.com; " +
            "font-src 'self' https://fonts.gstatic.com https://cdnjs.cloudflare.com https://cdn.jsdelivr.net; " +
            "img-src 'self' data: https:; " +
            "connect-src 'self' https://api.anthropic.com; " +
            "frame-ancestors 'none'; " +
            "base-uri 'self'; " +
            "form-action 'self'");

        // Remove additional ASP.NET headers
        context.Response.Headers.Remove("X-AspNet-Version");
        context.Response.Headers.Remove("X-AspNetMvc-Version");

        // Strict Transport Security (HSTS) - Only over HTTPS
        if (context.Request.IsHttps)
        {
            context.Response.Headers.Append("Strict-Transport-Security",
                "max-age=31536000; includeSubDomains; preload");
        }

        await _next(context);
    }

    /// <summary>
    /// Generates a cryptographically secure nonce for CSP
    /// </summary>
    private static string GenerateNonce()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}

/// <summary>
/// Extension methods for SecurityHeadersMiddleware
/// </summary>
public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SecurityHeadersMiddleware>();
    }
}
