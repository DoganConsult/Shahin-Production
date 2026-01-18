using GrcMvc.Configuration;
using GrcMvc.Filters;
using GrcMvc.Resources;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Threading.RateLimiting;
using OpenIddict.Validation.AspNetCore;

namespace GrcMvc.Extensions;

/// <summary>
/// Extension methods for authentication and security configuration
/// </summary>
public static class AuthenticationExtensions
{
    /// <summary>
    /// Configure ABP OpenIddict authentication
    /// ABP OpenIddict provides OAuth2/OpenID Connect authentication
    /// This method configures the validation handler for API endpoints
    /// </summary>
    public static IServiceCollection AddOpenIddictAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        // ABP OpenIddict is configured in GrcMvcAbpModule
        // This method now configures OpenIddict validation for API endpoints

        services.AddAuthentication(options =>
        {
            // Use OpenIddict for API authentication
            options.DefaultScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            options.DefaultAuthenticateScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
        });

        return services;
    }

    /// <summary>
    /// Configure authorization policies
    /// </summary>
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
            options.AddPolicy("ComplianceOfficer", policy => policy.RequireRole("ComplianceOfficer", "Admin"));
            options.AddPolicy("RiskManager", policy => policy.RequireRole("RiskManager", "Admin"));
            options.AddPolicy("Auditor", policy => policy.RequireRole("Auditor", "Admin"));

            // Platform Admin policy
            options.AddPolicy("ActivePlatformAdmin", policy =>
                policy.RequireRole("PlatformAdmin")
                      .AddRequirements(new Authorization.ActivePlatformAdminRequirement()));

            // Tenant Admin policy
            options.AddPolicy("ActiveTenantAdmin", policy =>
                policy.RequireRole("TenantAdmin")
                      .AddRequirements(new Authorization.ActiveTenantAdminRequirement()));
        });

        // Register authorization handlers
        services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler,
            Authorization.ActivePlatformAdminHandler>();
        services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler,
            Authorization.ActiveTenantAdminHandler>();

        // Dynamic permission policy provider
        services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationPolicyProvider,
            Authorization.PermissionPolicyProvider>();
        services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler,
            Authorization.PermissionAuthorizationHandler>();

        return services;
    }

    /// <summary>
    /// Configure rate limiting
    /// </summary>
    public static IServiceCollection AddRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var rateLimitConfig = configuration.GetSection(Services.Security.RateLimitingOptions.SectionName)
            .Get<Services.Security.RateLimitingOptions>() ?? new Services.Security.RateLimitingOptions();

        services.AddRateLimiter(options =>
        {
            // Global rate limit per IP/User
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.User?.Identity?.Name ?? 
                                context.Connection.RemoteIpAddress?.ToString() ?? 
                                "unknown",
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = rateLimitConfig.ApiRequests.PermitLimit,
                        QueueLimit = rateLimitConfig.ApiRequests.QueueLimit,
                        Window = rateLimitConfig.ApiRequests.Window
                    }));

            // API endpoints
            options.AddFixedWindowLimiter("api", limiterOptions =>
            {
                limiterOptions.PermitLimit = rateLimitConfig.ApiRequests.PermitLimit;
                limiterOptions.Window = rateLimitConfig.ApiRequests.Window;
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = rateLimitConfig.ApiRequests.QueueLimit;
            });

            // Authentication endpoints - prevent brute force
            options.AddFixedWindowLimiter("auth", limiterOptions =>
            {
                limiterOptions.PermitLimit = rateLimitConfig.Login.PermitLimit;
                limiterOptions.Window = rateLimitConfig.Login.Window;
                limiterOptions.QueueLimit = rateLimitConfig.Login.QueueLimit;
            });

            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.HttpContext.Response.WriteAsync(
                    "Too many requests. Please try again later.", cancellationToken: token);
            };
        });

        return services;
    }

    /// <summary>
    /// Configure data protection for multi-replica deployments
    /// </summary>
    public static IServiceCollection AddDataProtectionConfiguration(
        this IServiceCollection services, 
        IConfiguration configuration, 
        IWebHostEnvironment environment)
    {
        var dataProtectionBuilder = services.AddDataProtection()
            .SetApplicationName("ShahinGRC")
            .SetDefaultKeyLifetime(TimeSpan.FromDays(90));

        if (environment.IsProduction())
        {
            var keysPath = configuration["DataProtection:KeysPath"] ?? "/app/keys";
            
            try
            {
                Directory.CreateDirectory(keysPath);
                dataProtectionBuilder.PersistKeysToFileSystem(new DirectoryInfo(keysPath));
                Console.WriteLine($"[DATA_PROTECTION] Keys persisted to: {keysPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DATA_PROTECTION] Warning: Could not persist keys to {keysPath}: {ex.Message}");
            }
        }

        return services;
    }

    /// <summary>
    /// Configure session with security settings
    /// </summary>
    public static IServiceCollection AddSecureSession(this IServiceCollection services, IWebHostEnvironment environment)
    {
        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(20);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.SecurePolicy = environment.IsDevelopment() 
                ? CookieSecurePolicy.SameAsRequest 
                : CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Lax;
        });

        return services;
    }

    /// <summary>
    /// Configure anti-forgery tokens
    /// </summary>
    public static IServiceCollection AddAntiForgeryConfiguration(this IServiceCollection services, IWebHostEnvironment environment)
    {
        services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-CSRF-TOKEN";
            options.Cookie.Name = "X-CSRF-TOKEN";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = environment.IsDevelopment() 
                ? CookieSecurePolicy.SameAsRequest 
                : CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Lax;
        });

        return services;
    }

    /// <summary>
    /// Configure cookie policy with security settings
    /// </summary>
    public static IServiceCollection AddSecureCookiePolicy(this IServiceCollection services, IWebHostEnvironment environment)
    {
        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = environment.IsDevelopment() 
                ? CookieSecurePolicy.SameAsRequest 
                : CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
            options.LoginPath = "/Account/Login";
            options.AccessDeniedPath = "/Account/AccessDenied";
            options.LogoutPath = "/Account/Logout";
            options.SlidingExpiration = true;
        });

        return services;
    }

    /// <summary>
    /// Configure localization for Arabic, English, and Turkish
    /// </summary>
    public static IServiceCollection AddLocalizationConfiguration(this IServiceCollection services)
    {
        services.AddLocalization();
        services.Configure<RequestLocalizationOptions>(options =>
        {
            var supportedCultures = new[]
            {
                new CultureInfo("ar"), // Arabic - Default (RTL)
                new CultureInfo("en"), // English - Secondary (LTR)
                new CultureInfo("tr")  // Turkish (LTR)
            };

            options.DefaultRequestCulture = new RequestCulture("ar");
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;

            options.RequestCultureProviders.Insert(0, new CookieRequestCultureProvider
            {
                CookieName = "GrcMvc.Culture",
                Options = options
            });
        });

        return services;
    }
}
