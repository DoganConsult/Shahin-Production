using GrcMvc.BackgroundJobs;
using GrcMvc.Data;
using GrcMvc.Data.Seeds;
using GrcMvc.Exceptions;
using GrcMvc.Security;
using GrcMvc.Services.Implementations;
using GrcMvc.Services.Implementations.Workflows;
using GrcMvc.Services.Interfaces;
using GrcMvc.Services.Interfaces.Workflows;
using GrcMvc.Services.Interfaces.RBAC;
using GrcMvc.Services.Implementations.RBAC;
// Hangfire for background jobs
using Hangfire;
using Hangfire.PostgreSql;
// MassTransit for message queue
using MassTransit;
using GrcMvc.Messaging.Consumers;
using GrcMvc.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Extensions.Http;
using GrcMvc.Data.Repositories;
using GrcMvc.Models.Entities;
using GrcMvc.Configuration;
using GrcMvc.Services;
using GrcMvc.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using System.Text;
using FluentValidation.AspNetCore;
using FluentValidation;
using GrcMvc.Validators;
using GrcMvc.Models.DTOs;
using Npgsql;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;
using Serilog;
using Serilog.Events;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.DataProtection;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using GrcMvc.Resources;

var builder = WebApplication.CreateBuilder(args);

// ══════════════════════════════════════════════════════════════
// Load Environment Variables from .env file (Production Security)
// ══════════════════════════════════════════════════════════════
var envFile = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (!File.Exists(envFile))
{
    envFile = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".env");
}
if (!File.Exists(envFile))
{
    envFile = "/home/dogan/grc-system/.env";
}

if (File.Exists(envFile))
{
    foreach (var line in File.ReadAllLines(envFile))
    {
        if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
            continue;

        var parts = line.Split('=', 2);
        if (parts.Length == 2)
        {
            var envKey = parts[0].Trim();
            var envValue = parts[1].Trim();
            Environment.SetEnvironmentVariable(envKey, envValue);
        }
    }
    Console.WriteLine($"[ENV] Loaded environment variables from: {envFile}");
}

// ══════════════════════════════════════════════════════════════
// Connection String Configuration (ASP.NET Core Best Practices)
// ══════════════════════════════════════════════════════════════
// ASP.NET Core automatically loads configuration in this order:
// 1. appsettings.json
// 2. appsettings.{Environment}.json
// 3. Environment variables (ConnectionStrings__DefaultConnection)
// 4. Command-line arguments
//
// We support multiple formats for flexibility:
// - Standard: ConnectionStrings__DefaultConnection (Docker Compose)
// - Individual: DB_HOST, DB_PORT, DB_NAME, DB_USER, DB_PASSWORD
// - Fallback: appsettings.json defaults

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// If not set via standard configuration, try building from individual DB_* variables
if (string.IsNullOrWhiteSpace(connectionString))
{
    var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
    var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
    var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "GrcMvcDb";
    var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "postgres";
    // SECURITY: No fallback for password - must be explicitly set
    var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
    
    // Only build if DB_HOST and DB_PASSWORD are explicitly set
    if (!string.IsNullOrWhiteSpace(dbHost) && !string.IsNullOrWhiteSpace(dbPassword))
    {
        connectionString = $"Host={dbHost};Database={dbName};Username={dbUser};Password={dbPassword};Port={dbPort}";
        builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;
    }
}

// Get auth connection string (or use default if not set)
string? authConnectionString = builder.Configuration.GetConnectionString("GrcAuthDb") ?? connectionString;
if (string.IsNullOrWhiteSpace(authConnectionString) && !string.IsNullOrWhiteSpace(connectionString))
{
    builder.Configuration["ConnectionStrings:GrcAuthDb"] = connectionString;
}
// CRITICAL SECURITY FIX: JWT secret must be provided via environment variable - no fallback in any environment
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
if (string.IsNullOrWhiteSpace(jwtSecret))
{
    throw new InvalidOperationException("JWT_SECRET environment variable is required. Set it before starting the application.");
}
builder.Configuration["JwtSettings:Secret"] = jwtSecret;
builder.Configuration["ClaudeAgents:ApiKey"] = Environment.GetEnvironmentVariable("CLAUDE_API_KEY") ?? "";
builder.Configuration["ClaudeAgents:Model"] = Environment.GetEnvironmentVariable("CLAUDE_MODEL") ?? "claude-sonnet-4-20250514";
builder.Configuration["ClaudeAgents:MaxTokens"] = Environment.GetEnvironmentVariable("CLAUDE_MAX_TOKENS") ?? "4096";
builder.Configuration["ClaudeAgents:Enabled"] = Environment.GetEnvironmentVariable("CLAUDE_ENABLED") ?? "true";

// SMTP Settings - Uses AZURE_TENANT_ID for OAuth2
builder.Configuration["SmtpSettings:TenantId"] = Environment.GetEnvironmentVariable("AZURE_TENANT_ID") ?? "";
builder.Configuration["SmtpSettings:ClientId"] = Environment.GetEnvironmentVariable("SMTP_CLIENT_ID") ?? "";
builder.Configuration["SmtpSettings:ClientSecret"] = Environment.GetEnvironmentVariable("SMTP_CLIENT_SECRET") ?? "";
builder.Configuration["SmtpSettings:FromEmail"] = Environment.GetEnvironmentVariable("SMTP_FROM_EMAIL") ?? "";
builder.Configuration["SmtpSettings:Username"] = Environment.GetEnvironmentVariable("SMTP_USERNAME") ?? "";
builder.Configuration["SmtpSettings:Password"] = Environment.GetEnvironmentVariable("SMTP_PASSWORD") ?? "";

// Microsoft Graph API - Uses MSGRAPH_* variables
builder.Configuration["EmailOperations:MicrosoftGraph:TenantId"] = Environment.GetEnvironmentVariable("AZURE_TENANT_ID") ?? "";
builder.Configuration["EmailOperations:MicrosoftGraph:ClientId"] = Environment.GetEnvironmentVariable("MSGRAPH_CLIENT_ID") ?? "";
builder.Configuration["EmailOperations:MicrosoftGraph:ClientSecret"] = Environment.GetEnvironmentVariable("MSGRAPH_CLIENT_SECRET") ?? "";
builder.Configuration["EmailOperations:MicrosoftGraph:ApplicationIdUri"] = Environment.GetEnvironmentVariable("MSGRAPH_APP_ID_URI") ?? "";

// Copilot Agent - Uses AZURE_TENANT_ID
builder.Configuration["CopilotAgent:TenantId"] = Environment.GetEnvironmentVariable("AZURE_TENANT_ID") ?? "";
builder.Configuration["CopilotAgent:ClientId"] = Environment.GetEnvironmentVariable("COPILOT_CLIENT_ID") ?? "";
builder.Configuration["CopilotAgent:ClientSecret"] = Environment.GetEnvironmentVariable("COPILOT_CLIENT_SECRET") ?? "";
builder.Configuration["CopilotAgent:ApplicationIdUri"] = Environment.GetEnvironmentVariable("COPILOT_APP_ID_URI") ?? "";
builder.Configuration["CopilotAgent:Enabled"] = Environment.GetEnvironmentVariable("COPILOT_ENABLED") ?? "false";

// Camunda BPM (Optional) - SECURITY: No default password
builder.Configuration["Camunda:BaseUrl"] = Environment.GetEnvironmentVariable("CAMUNDA_BASE_URL") ?? "http://localhost:8085/camunda";
builder.Configuration["Camunda:Username"] = Environment.GetEnvironmentVariable("CAMUNDA_USERNAME") ?? "admin";
builder.Configuration["Camunda:Password"] = Environment.GetEnvironmentVariable("CAMUNDA_PASSWORD") ?? "";
builder.Configuration["Camunda:Enabled"] = Environment.GetEnvironmentVariable("CAMUNDA_ENABLED") ?? "false";

// Kafka (Optional)
builder.Configuration["Kafka:BootstrapServers"] = Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS") ?? "localhost:9092";
builder.Configuration["Kafka:Enabled"] = Environment.GetEnvironmentVariable("KAFKA_ENABLED") ?? "false";

// Configure Serilog for structured logging
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .Enrich.WithProperty("Application", "GrcMvc")
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "/app/logs/grcmvc-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}",
        shared: true)
    .WriteTo.File(
        path: "/app/logs/grcmvc-errors-.log",
        rollingInterval: RollingInterval.Day,
        restrictedToMinimumLevel: LogEventLevel.Warning,
        retainedFileCountLimit: 60)
);

// Configure Kestrel for HTTPS and security
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.AddServerHeader = false; // Remove Server header

    serverOptions.ConfigureHttpsDefaults(httpsOptions =>
    {
        var certPath = builder.Configuration["Certificates:Path"];
        var certPassword = builder.Configuration["Certificates:Password"];

        if (!string.IsNullOrEmpty(certPath) && File.Exists(certPath))
        {
            httpsOptions.ServerCertificate = new X509Certificate2(certPath, certPassword);
        }
    });

    // Request size limits to prevent DoS
    serverOptions.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10MB
    serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(1);
    serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
});

// Add CORS for API access (if needed for SPA)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowApiClients", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins")?.Get<string[]>();

        if (allowedOrigins != null && allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        }
        else
        {
            // Default: Allow localhost for development
            policy.WithOrigins("http://localhost:3000", "http://localhost:5137")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        }
    });
});

// Add HttpContextAccessor for Blazor components
builder.Services.AddHttpContextAccessor();

// Add Localization services (Arabic default, English secondary)
// Note: ResourcesPath is NOT set because the SharedResource class namespace already includes "Resources"
// The resources are embedded as GrcMvc.Resources.SharedResource which matches the class namespace
builder.Services.AddLocalization();
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("ar"), // Arabic - Default (RTL)
        new CultureInfo("en")  // English - Secondary (LTR)
    };

    options.DefaultRequestCulture = new RequestCulture("ar"); // Arabic as default
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;

    // Store culture preference in cookie
    options.RequestCultureProviders.Insert(0, new CookieRequestCultureProvider
    {
        CookieName = "GrcMvc.Culture",
        Options = options
    });
});

// Add services to the container with FluentValidation
builder.Services.AddControllersWithViews(options =>
{
    // Add global anti-forgery token validation
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());

    // Add duplicate property binding detection filter
    // This detects when both canonical and alias properties are bound from form submissions
    // API requests: returns 400 BadRequest if duplicates detected
    // MVC requests: logs warning but allows action to proceed (backward compatibility)
    options.Filters.Add<GrcMvc.Filters.DuplicatePropertyBindingFilter>();
}).AddRazorRuntimeCompilation()
.AddViewLocalization()
.AddDataAnnotationsLocalization(options =>
{
    options.DataAnnotationLocalizerProvider = (type, factory) =>
        factory.Create(typeof(SharedResource));
});
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

// Register validators
builder.Services.AddValidatorsFromAssemblyContaining<CreateRiskDtoValidator>();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Bind strongly-typed configuration
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.Configure<ApplicationSettings>(
    builder.Configuration.GetSection(ApplicationSettings.SectionName));
// EmailSettings with proper binding (uses nullable port to handle empty env var override)
// Defaults are set in EmailSettings record, GetSmtpPort() provides safe fallback
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection(EmailSettings.SectionName));

builder.Services.Configure<SiteSettings>(
    builder.Configuration.GetSection(SiteSettings.SectionName));

// Register SiteSettings service
builder.Services.AddScoped<ISiteSettingsService, SiteSettingsService>();

// Validate configuration at startup
builder.Services.AddSingleton<IValidateOptions<JwtSettings>, JwtSettingsValidator>();
builder.Services.AddSingleton<IValidateOptions<ApplicationSettings>, ApplicationSettingsValidator>();

// ══════════════════════════════════════════════════════════════
// Entity Framework Configuration
// ══════════════════════════════════════════════════════════════
// Use GetConnectionString() which respects all configuration sources:
// 1. Environment variables (ConnectionStrings__DefaultConnection)
// 2. appsettings.{Environment}.json
// 3. appsettings.json

// Validate connection string is available
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Database connection string 'DefaultConnection' not found. " +
        "Please set it via one of the following methods:\n" +
        "1. Environment variable: ConnectionStrings__DefaultConnection\n" +
        "2. Environment variables: DB_HOST, DB_NAME, DB_USER, DB_PASSWORD\n" +
        "3. appsettings.{Environment}.json: ConnectionStrings.DefaultConnection");
}

// Register master DbContext for tenant metadata (uses default connection)
builder.Services.AddDbContext<GrcDbContext>(options =>
    options.UseNpgsql(connectionString!), ServiceLifetime.Scoped);

// Register Auth DbContext for Identity (separate database)
var finalAuthConnectionString = authConnectionString ?? connectionString!;
builder.Services.AddDbContext<GrcAuthDbContext>(options =>
    options.UseNpgsql(finalAuthConnectionString), ServiceLifetime.Scoped);

// Register tenant database resolver
builder.Services.AddScoped<ITenantDatabaseResolver, TenantDatabaseResolver>();

// Register tenant-aware DbContext factory
builder.Services.AddScoped<IDbContextFactory<GrcDbContext>, TenantAwareDbContextFactory>();

// Add Health Checks
// ══════════════════════════════════════════════════════════════
// Enhanced Health Checks Configuration
// ══════════════════════════════════════════════════════════════
builder.Services.AddHealthChecks()
    .AddNpgSql(
        connectionString ?? throw new InvalidOperationException("Connection string not configured"),
        name: "master-database",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "db", "postgresql", "master", "critical" },
        timeout: TimeSpan.FromSeconds(5))
    .AddCheck<GrcMvc.HealthChecks.TenantDatabaseHealthCheck>(
        name: "tenant-database",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "db", "postgresql", "tenant", "critical" },
        timeout: TimeSpan.FromSeconds(5))
    .AddHangfire(
        options =>
        {
            options.MinimumAvailableServers = 1;
        },
        name: "hangfire",
        failureStatus: HealthStatus.Degraded,
        tags: new[] { "jobs", "hangfire" },
        timeout: TimeSpan.FromSeconds(3))
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Application is running"),
        tags: new[] { "api", "self" });

Console.WriteLine("[HEALTH] Enhanced health checks configured (Database, Hangfire, Self)");

// Configure Data Protection
builder.Services.AddDataProtection()
    .SetApplicationName("GrcMvc")
    .SetDefaultKeyLifetime(TimeSpan.FromDays(90));

// Add Rate Limiting to prevent abuse
builder.Services.AddRateLimiter(options =>
{
    // Global rate limit per IP/User
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User?.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 500, // Increased from 100 to handle legitimate traffic
                QueueLimit = 10,
                Window = TimeSpan.FromMinutes(1)
            }));

    // API endpoints - stricter limits
    options.AddFixedWindowLimiter("api", limiterOptions =>
    {
        limiterOptions.PermitLimit = 30;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 5;
    });

    // Authentication endpoints - prevent brute force
    options.AddFixedWindowLimiter("auth", limiterOptions =>
    {
        limiterOptions.PermitLimit = 5;
        limiterOptions.Window = TimeSpan.FromMinutes(5);
        limiterOptions.QueueLimit = 0;
    });

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsync(
            "Too many requests. Please try again later.", cancellationToken: token);
    };
});

// Configure Identity with enhanced security
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings - Strengthened
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 12; // Increased from 8
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;

    // Lockout settings - More restrictive
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15); // Increased
    options.Lockout.MaxFailedAccessAttempts = 3; // Reduced from 5
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;

    // Sign-in settings - Email confirmation enabled for production security
    options.SignIn.RequireConfirmedEmail = builder.Environment.IsProduction();
    options.SignIn.RequireConfirmedAccount = builder.Environment.IsProduction();
})
.AddEntityFrameworkStores<GrcAuthDbContext>()
.AddDefaultTokenProviders();

// Configure JWT Authentication (for API endpoints)
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>();
if (jwtSettings == null || !jwtSettings.IsValid())
{
    throw new InvalidOperationException(
        "JWT settings are invalid or missing. " +
        "Please set JwtSettings__Secret (min 32 chars) via environment variable or User Secrets.");
}

var key = Encoding.UTF8.GetBytes(jwtSettings.Secret);

builder.Services.AddAuthentication(options =>
{
    // Use cookie authentication as default for MVC web app
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(1) // Allow 1 minute clock skew
    };
});

// Add authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ComplianceOfficer", policy => policy.RequireRole("ComplianceOfficer", "Admin"));
    options.AddPolicy("RiskManager", policy => policy.RequireRole("RiskManager", "Admin"));
    options.AddPolicy("Auditor", policy => policy.RequireRole("Auditor", "Admin"));

    // Platform Admin policy: requires PlatformAdmin role AND active PlatformAdmin record
    options.AddPolicy("ActivePlatformAdmin", policy =>
        policy.RequireRole("PlatformAdmin")
              .AddRequirements(new GrcMvc.Authorization.ActivePlatformAdminRequirement()));

    // Tenant Admin policy: requires TenantAdmin role AND active TenantAdmin record
    options.AddPolicy("ActiveTenantAdmin", policy =>
        policy.RequireRole("TenantAdmin")
              .AddRequirements(new GrcMvc.Authorization.ActiveTenantAdminRequirement()));
});

// Register ActivePlatformAdmin authorization handler
builder.Services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler,
    GrcMvc.Authorization.ActivePlatformAdminHandler>();

// Register ActiveTenantAdmin authorization handler
builder.Services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler,
    GrcMvc.Authorization.ActiveTenantAdminHandler>();

// Dynamic permission policy provider for [Authorize("Grc.*")] attributes
// This resolves policies on-demand without manual registration for each permission
builder.Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationPolicyProvider,
    GrcMvc.Authorization.PermissionPolicyProvider>();
builder.Services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler,
    GrcMvc.Authorization.PermissionAuthorizationHandler>();

// Add session support with enhanced security
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20); // Reduced from 30
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    // SECURITY: Use Always in production, SameAsRequest in development
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() 
        ? CookieSecurePolicy.SameAsRequest 
        : CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// Configure anti-forgery tokens
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "X-CSRF-TOKEN";
    options.Cookie.HttpOnly = true;
    // SECURITY: Use Always in production, SameAsRequest in development
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() 
        ? CookieSecurePolicy.SameAsRequest 
        : CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Register repositories and Unit of Work
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
// Use TenantAwareUnitOfWork for proper multi-tenant database isolation
// This uses IDbContextFactory to create tenant-specific database connections
builder.Services.AddScoped<IUnitOfWork, TenantAwareUnitOfWork>();

// Register services

// App Info Service (centralized branding & version info)
builder.Services.AddSingleton<IAppInfoService, AppInfoService>();

// RiskService migrated to IDbContextFactory for tenant database isolation
builder.Services.AddScoped<IRiskService, RiskService>();
builder.Services.AddScoped<IControlService, ControlService>();
builder.Services.AddScoped<IAssessmentService, AssessmentService>();
builder.Services.AddScoped<IAssessmentExecutionService, AssessmentExecutionService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IPolicyService, PolicyService>();
builder.Services.AddScoped<IWorkflowService, WorkflowService>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<IActionPlanService, ActionPlanService>();
builder.Services.AddScoped<IVendorService, VendorService>();
builder.Services.AddScoped<IRegulatorService, RegulatorService>();
builder.Services.AddScoped<IComplianceCalendarService, ComplianceCalendarService>();
builder.Services.AddScoped<IFrameworkManagementService, FrameworkManagementService>();
builder.Services.AddTransient<IAppEmailSender, SmtpEmailSender>();

// PHASE 1: Register critical services for Framework Data, HRIS, Audit Trail, and Rules Engine
builder.Services.AddScoped<IFrameworkService, Phase1FrameworkService>();
builder.Services.AddScoped<IHRISService, HRISService>();
builder.Services.AddScoped<IAuditTrailService, AuditTrailService>();
// Use Phase1RulesEngineService (with asset-based recognition) instead of stub
builder.Services.AddScoped<IRulesEngineService, Phase1RulesEngineService>();

// Serial Code Service - 6-Stage GRC Serial Code System
// Provides unified, tenant-aware, auditable serial code generation
// Format: {PREFIX}-{TENANT}-{STAGE}-{YEAR}-{SEQUENCE}-{VERSION}
builder.Services.AddScoped<ISerialCodeService, SerialCodeService>();

// Register new STAGE 1 services
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IOnboardingService, OnboardingService>();
builder.Services.AddScoped<IOnboardingWizardService, OnboardingWizardService>();
builder.Services.AddScoped<IAuditEventService, AuditEventService>();

// Owner Dashboard Service - replaces direct DbContext access in OwnerController
builder.Services.AddScoped<IOwnerDashboardService, OwnerDashboardService>();

// Post-Login Routing Service for role-based redirection
builder.Services.AddScoped<IPostLoginRoutingService, PostLoginRoutingService>();
// Use real SMTP email service via adapter (replaces StubEmailService)
builder.Services.AddScoped<IEmailService, EmailServiceAdapter>();
// GRC-specific email service with templates
builder.Services.AddScoped<IGrcEmailService, GrcEmailService>();
// Email-based MFA service
builder.Services.AddScoped<IEmailMfaService, EmailMfaService>();
builder.Services.AddScoped<IPlanService, PlanService>();

// Workflow Services
builder.Services.AddScoped<GrcMvc.Services.Interfaces.IEvidenceWorkflowService, GrcMvc.Services.Implementations.EvidenceWorkflowService>();
builder.Services.AddScoped<GrcMvc.Services.Interfaces.IRiskWorkflowService, GrcMvc.Services.Implementations.RiskWorkflowService>();

// Resilience & Sustainability Services (GRC Lifecycle Stages 4 & 5)
builder.Services.AddScoped<IResilienceService, ResilienceService>();
builder.Services.AddScoped<ISustainabilityService, SustainabilityService>();

// Owner tenant management
builder.Services.AddScoped<IOwnerTenantService, OwnerTenantService>();
builder.Services.AddScoped<IOwnerSetupService, OwnerSetupService>();
builder.Services.AddScoped<ICredentialDeliveryService, CredentialDeliveryService>();
builder.Services.AddScoped<ICredentialExpirationService, CredentialExpirationService>(); // HIGH FIX: Credential expiration checks

// Platform Admin (Multi-Tenant Administration - Layer 0)
builder.Services.AddScoped<IPlatformAdminService, PlatformAdminService>();

// Serial Number Service (system-generated document numbers)
builder.Services.AddScoped<ISerialNumberService, SerialNumberService>();

// Caching & Policy Decision Audit Service
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IGrcCachingService, GrcCachingService>();

// Onboarding Provisioning Service (auto-creates default teams + RACI)
builder.Services.AddScoped<IOnboardingProvisioningService, OnboardingProvisioningService>();

// Asset Service (asset management for scope derivation)
builder.Services.AddScoped<IAssetService, AssetService>();

// Register PHASE 2 - 10 WORKFLOW TYPES
builder.Services.AddScoped<IControlImplementationWorkflowService, ControlImplementationWorkflowService>();
builder.Services.AddScoped<IRiskAssessmentWorkflowService, RiskAssessmentWorkflowService>();
builder.Services.AddScoped<IApprovalWorkflowService, ApprovalWorkflowService>();
builder.Services.AddScoped<IEvidenceCollectionWorkflowService, EvidenceCollectionWorkflowService>();
builder.Services.AddScoped<IComplianceTestingWorkflowService, ComplianceTestingWorkflowService>();
builder.Services.AddScoped<IRemediationWorkflowService, RemediationWorkflowService>();
builder.Services.AddScoped<IPolicyReviewWorkflowService, PolicyReviewWorkflowService>();
builder.Services.AddScoped<ITrainingAssignmentWorkflowService, TrainingAssignmentWorkflowService>();
builder.Services.AddScoped<IAuditWorkflowService, AuditWorkflowService>();
builder.Services.AddScoped<IExceptionHandlingWorkflowService, ExceptionHandlingWorkflowService>();

// Register existing Workflow services
builder.Services.AddScoped<BpmnParser>();
builder.Services.AddScoped<WorkflowAssigneeResolver>();
builder.Services.AddScoped<IWorkflowAuditService, WorkflowAuditService>();
builder.Services.AddScoped<IWorkflowEngineService, WorkflowEngineService>();
builder.Services.AddScoped<IEscalationService, EscalationService>();
builder.Services.AddScoped<IUserWorkspaceService, UserWorkspaceService>();
builder.Services.AddScoped<IInboxService, InboxService>();

// Register RBAC Services (Role-Based Access Control)
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IFeatureService, FeatureService>();
builder.Services.AddScoped<ITenantRoleConfigurationService, TenantRoleConfigurationService>();
builder.Services.AddScoped<IUserRoleAssignmentService, UserRoleAssignmentService>();
builder.Services.AddScoped<IAccessControlService, AccessControlService>();
builder.Services.AddScoped<IRbacSeederService, RbacSeederService>();

// Register User Profile Service (14 user profiles)
builder.Services.AddScoped<IUserProfileService, UserProfileServiceImpl>();

// Register Tenant Context Service
builder.Services.AddScoped<ITenantContextService, TenantContextService>();

// Register Claims Transformation (adds TenantId claim automatically)
builder.Services.AddScoped<Microsoft.AspNetCore.Authentication.IClaimsTransformation, GrcMvc.Services.Implementations.ClaimsTransformationService>();

// User Directory Service (batch lookups from Auth DB - replaces cross-DB joins)
builder.Services.AddScoped<IUserDirectoryService, UserDirectoryService>();

// Register Workspace Context Service (for "Workspace inside Tenant" model)
builder.Services.AddScoped<IWorkspaceContextService, WorkspaceContextService>();

// Register Workspace Management Service (for managing workspaces within a tenant)
builder.Services.AddScoped<IWorkspaceManagementService, WorkspaceManagementService>();

// Register Tenant Provisioning Service
builder.Services.AddScoped<ITenantProvisioningService, TenantProvisioningService>();

// Register STAGE 2 Enterprise LLM service
builder.Services.AddScoped<ILlmService, LlmService>();
builder.Services.AddHttpClient<ILlmService, LlmService>();

// Smart Onboarding Service (auto-generates assessment templates and GRC plans)
builder.Services.AddScoped<ISmartOnboardingService, SmartOnboardingService>();

// User Consent & Support Agent Services
builder.Services.AddScoped<IConsentService, ConsentService>();
builder.Services.AddScoped<ISupportAgentService, SupportAgentService>();

// Workspace Service (Role-based pre-mapping)
builder.Services.AddScoped<IWorkspaceService, WorkspaceService>();

// Workflow Routing Service (Role-based assignee resolution - never breaks when teams change)
builder.Services.AddScoped<IWorkflowRoutingService, WorkflowRoutingService>();

// Expert Framework Mapping Service (Sector-driven compliance blueprints)
builder.Services.AddScoped<IExpertFrameworkMappingService, ExpertFrameworkMappingService>();

// Sector-Framework Cache Service (Fast lookups for onboarding)
builder.Services.AddSingleton<ISectorFrameworkCacheService, SectorFrameworkCacheService>();

// Tenant Evidence Provisioning Service (Auto-generate evidence requirements on onboarding)
builder.Services.AddScoped<ITenantEvidenceProvisioningService, TenantEvidenceProvisioningService>();

// Suite Generation Service (Baseline + Overlays model)
builder.Services.AddScoped<ISuiteGenerationService, SuiteGenerationService>();

// Shahin-AI Orchestration Service (MAP, APPLY, PROVE, WATCH, FIX, VAULT)
builder.Services.AddScoped<IShahinAIOrchestrationService, ShahinAIOrchestrationService>();

// Shahin-AI Module Services (MAP, APPLY, PROVE, WATCH, FIX, VAULT)
builder.Services.AddScoped<IMAPService, MAPService>();
builder.Services.AddScoped<IAPPLYService, APPLYService>();
builder.Services.AddScoped<IPROVEService, PROVEService>();
builder.Services.AddScoped<IWATCHService, WATCHService>();
builder.Services.AddScoped<IFIXService, FIXService>();
builder.Services.AddScoped<IVAULTService, VAULTService>();

// Assessment Execution & Workflow Integration Services
builder.Services.AddScoped<IAssessmentExecutionService, AssessmentExecutionService>();
builder.Services.AddScoped<IWorkflowIntegrationService, WorkflowIntegrationService>();

// Role Delegation Service (Human↔Human, Human↔Agent, Agent↔Agent, Multi-Agent)
builder.Services.AddScoped<IRoleDelegationService, RoleDelegationService>();

// Catalog Data Service (Dynamic querying of regulators, frameworks, controls, evidence types)
builder.Services.AddScoped<ICatalogDataService, CatalogDataService>();
// MemoryCache already added above

// Register Resilience Services
builder.Services.AddScoped<IResilienceService, ResilienceService>();

// GAP CLOSURE: Control Testing Service (Control effectiveness, testing, owner management)
builder.Services.AddScoped<IControlTestService, ControlTestService>();

// GAP CLOSURE: Incident Response Service (Resilience stage - Detection, Response, Recovery)
builder.Services.AddScoped<IIncidentResponseService, IncidentResponseService>();

// GAP CLOSURE: Certification Tracking Service (ISO 27001, SOC 2, NCA, PCI-DSS lifecycle)
builder.Services.AddScoped<ICertificationService, CertificationService>();

// Register Subscription & Billing service
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();

// Integration Services (Email, File Storage, Payment, SSO)
builder.Services.AddHttpClient(); // Default HttpClient for services like DiagnosticAgent
builder.Services.AddHttpClient("Email");
builder.Services.AddHttpClient("Stripe");
builder.Services.AddHttpClient("SSO");
builder.Services.AddScoped<GrcMvc.Services.Integrations.IEmailIntegrationService, GrcMvc.Services.Integrations.EmailIntegrationService>();
builder.Services.AddScoped<GrcMvc.Services.Integrations.IPaymentIntegrationService, GrcMvc.Services.Integrations.StripePaymentService>();
builder.Services.AddScoped<GrcMvc.Services.Integrations.ISSOIntegrationService, GrcMvc.Services.Integrations.SSOIntegrationService>();
builder.Services.AddScoped<GrcMvc.Services.Integrations.IEvidenceAutomationService, GrcMvc.Services.Integrations.EvidenceAutomationService>();

// Register Evidence and Report services
builder.Services.AddScoped<IEvidenceService, EvidenceService>();

// Enhanced Report Services with PDF/Excel generation
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<IReportGenerator, ReportGeneratorService>();
builder.Services.AddScoped<IReportService, EnhancedReportServiceFixed>();
builder.Services.AddScoped<IDocumentGenerationService, DocumentGenerationService>(); // Document generation for templates

// ============================================
// Enterprise Government GRC Services
// خدمات الحوكمة والمخاطر والامتثال للقطاع الحكومي
// ============================================

// Vision 2030 Alignment Service - رؤية 2030
builder.Services.AddScoped<IVision2030AlignmentService, Vision2030AlignmentService>();

// National Compliance Hub - مركز الامتثال الوطني
builder.Services.AddScoped<INationalComplianceHub, NationalComplianceHubService>();

// Regulatory Calendar Service - التقويم التنظيمي
builder.Services.AddScoped<IRegulatoryCalendarService, RegulatoryCalendarService>();

// Arabic Compliance Assistant - مساعد الامتثال العربي
builder.Services.AddScoped<IArabicComplianceAssistant, ArabicComplianceAssistantService>();

// Attestation Service - خدمة التصديق والشهادات
builder.Services.AddScoped<IAttestationService, AttestationService>();

// Government Integration Service - التكامل الحكومي (Nafath, Absher, Etimad, etc.)
builder.Services.AddScoped<IGovernmentIntegrationService, GovernmentIntegrationService>();

// GRC Process Orchestrator - منسق عمليات الحوكمة والمخاطر والامتثال
// Complete lifecycle: Assessment → Compliance → Resilience → Excellence
builder.Services.AddScoped<IGrcProcessOrchestrator, GrcProcessOrchestrator>();

// Compliance Gap Management Service - إدارة فجوات الامتثال
// Gap lifecycle: Identify → Plan → Remediate → Validate → Close
builder.Services.AddScoped<IComplianceGapService, ComplianceGapService>();

// Register Menu Service (RBAC-based navigation)
builder.Services.AddScoped<GrcMvc.Data.Menu.GrcMenuContributor>();
builder.Services.AddScoped<IMenuService, MenuService>();

// CRITICAL FIX: Register Identity-based AuthenticationService instead of mock
// builder.Services.AddScoped<IAuthenticationService, AuthenticationService>(); // OLD: Mock implementation
builder.Services.AddScoped<IAuthenticationService, IdentityAuthenticationService>(); // NEW: Identity-based
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();

// Policy Enforcement System
builder.Services.AddScoped<GrcMvc.Application.Policy.IPolicyEnforcer, GrcMvc.Application.Policy.PolicyEnforcer>();
builder.Services.AddSingleton<GrcMvc.Application.Policy.IPolicyStore, GrcMvc.Application.Policy.PolicyStore>();
builder.Services.AddScoped<GrcMvc.Application.Policy.IDotPathResolver, GrcMvc.Application.Policy.DotPathResolver>();
builder.Services.AddScoped<GrcMvc.Application.Policy.IMutationApplier, GrcMvc.Application.Policy.MutationApplier>();
builder.Services.AddScoped<GrcMvc.Application.Policy.IPolicyAuditLogger, GrcMvc.Application.Policy.PolicyAuditLogger>();
builder.Services.AddScoped<GrcMvc.Application.Policy.PolicyEnforcementHelper>(); // Helper for easy integration
builder.Services.AddHostedService<GrcMvc.Application.Policy.PolicyStore>(); // For hot reload

// Permissions System
builder.Services.AddSingleton<GrcMvc.Application.Permissions.IPermissionDefinitionProvider, GrcMvc.Application.Permissions.GrcPermissionDefinitionProvider>();
builder.Services.AddScoped<GrcMvc.Application.Permissions.PermissionSeederService>();

// Policy Validation Helper (for UX enhancements)
builder.Services.AddScoped<GrcMvc.Application.Policy.PolicyValidationHelper>();

// =============================================================================
// MIGRATION SERVICES (Parallel V2 Architecture - Complete Security Enhancement)
// =============================================================================

// Feature Flags for gradual migration
builder.Services.Configure<GrcFeatureOptions>(
    builder.Configuration.GetSection(GrcFeatureOptions.SectionName));

// Metrics Service (track legacy vs enhanced usage)
builder.Services.AddSingleton<IMetricsService, MetricsService>();

// Enhanced Security Services
builder.Services.AddScoped<ISecurePasswordGenerator, SecurePasswordGenerator>();
builder.Services.AddScoped<IEnhancedAuthService, EnhancedAuthService>();
builder.Services.AddScoped<IEnhancedTenantResolver, EnhancedTenantResolver>();

// User Management Facade (routes between legacy and enhanced)
builder.Services.AddScoped<IUserManagementFacade, UserManagementFacade>();

// Register Application Initializer for seed data
builder.Services.AddScoped<ApplicationInitializer>();

// Register User Seeding Hosted Service (runs on startup)
builder.Services.AddHostedService<UserSeedingHostedService>();

// Register Catalog Seeder Service
builder.Services.AddScoped<CatalogSeederService>();

// Register Workflow Definition Seeder Service
builder.Services.AddScoped<WorkflowDefinitionSeederService>();

// Register Framework Control Import Service
builder.Services.AddScoped<FrameworkControlImportService>();

// Register POC Seeder Service (Shahin-AI demo organization)
builder.Services.AddScoped<IPocSeederService, PocSeederService>();

// PHASE 6: User Invitation Service
builder.Services.AddScoped<IUserInvitationService, UserInvitationService>();

// PHASE 6.1: Tenant Onboarding Provisioner (workspace, assessment template, GRC plan)
builder.Services.AddScoped<ITenantOnboardingProvisioner, TenantOnboardingProvisioner>();

// PHASE 8: Evidence Lifecycle Service
builder.Services.AddScoped<IEvidenceLifecycleService, EvidenceLifecycleService>();

// PHASE 9: Dashboard Service
builder.Services.AddScoped<IDashboardService, DashboardService>();

// =============================================================================
// ANALYTICS SERVICES (ClickHouse OLAP + SignalR + Redis)
// =============================================================================

// Configuration
builder.Services.Configure<GrcMvc.Configuration.ClickHouseSettings>(
    builder.Configuration.GetSection(GrcMvc.Configuration.ClickHouseSettings.SectionName));
builder.Services.Configure<GrcMvc.Configuration.RedisSettings>(
    builder.Configuration.GetSection(GrcMvc.Configuration.RedisSettings.SectionName));
builder.Services.Configure<GrcMvc.Configuration.KafkaSettings>(
    builder.Configuration.GetSection(GrcMvc.Configuration.KafkaSettings.SectionName));
builder.Services.Configure<GrcMvc.Configuration.SignalRSettings>(
    builder.Configuration.GetSection(GrcMvc.Configuration.SignalRSettings.SectionName));
builder.Services.Configure<GrcMvc.Configuration.AnalyticsSettings>(
    builder.Configuration.GetSection(GrcMvc.Configuration.AnalyticsSettings.SectionName));

// ClickHouse Service
var clickHouseEnabled = builder.Configuration.GetValue<bool>("ClickHouse:Enabled", false);
if (clickHouseEnabled)
{
    builder.Services.AddHttpClient<GrcMvc.Services.Analytics.IClickHouseService, GrcMvc.Services.Analytics.ClickHouseService>(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
    });
    builder.Services.AddScoped<GrcMvc.Services.Analytics.IDashboardProjector, GrcMvc.Services.Analytics.DashboardProjector>();
    builder.Services.AddScoped<GrcMvc.BackgroundJobs.AnalyticsProjectionJob>();
}
else
{
    // Register stub implementations when ClickHouse is disabled
    builder.Services.AddScoped<GrcMvc.Services.Analytics.IClickHouseService, GrcMvc.Services.Analytics.StubClickHouseService>();
    builder.Services.AddScoped<GrcMvc.Services.Analytics.IDashboardProjector, GrcMvc.Services.Analytics.StubDashboardProjector>();
}

// Redis Cache (optional - falls back to IMemoryCache)
// Enable with Redis:Enabled=true in configuration
var redisEnabled = builder.Configuration.GetValue<bool>("Redis:Enabled", false);
if (redisEnabled)
{
    var redisConnectionString = builder.Configuration.GetValue<string>("Redis:ConnectionString") ?? "grc-redis:6379";
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnectionString;
        options.InstanceName = builder.Configuration.GetValue<string>("Redis:InstanceName") ?? "GrcCache_";
    });

    // Add Redis health check
    builder.Services.AddHealthChecks()
        .AddRedis(
            redisConnectionString,
            name: "redis-cache",
            failureStatus: HealthStatus.Degraded,
            tags: new[] { "cache", "redis" },
            timeout: TimeSpan.FromSeconds(3));

    Console.WriteLine($"✅ Redis caching enabled: {redisConnectionString}");
    Console.WriteLine("[HEALTH] Redis health check configured");
}
else
{
    Console.WriteLine("ℹ️ Redis disabled - using IMemoryCache fallback");
}

// SignalR Hub
var signalREnabled = builder.Configuration.GetValue<bool>("SignalR:Enabled", true);
if (signalREnabled)
{
    var signalRBuilder = builder.Services.AddSignalR(options =>
    {
        options.EnableDetailedErrors = builder.Environment.IsDevelopment();
        options.KeepAliveInterval = TimeSpan.FromSeconds(
            builder.Configuration.GetValue<int>("SignalR:KeepAliveIntervalSeconds", 15));
        options.ClientTimeoutInterval = TimeSpan.FromSeconds(
            builder.Configuration.GetValue<int>("SignalR:ClientTimeoutSeconds", 30));
        options.MaximumReceiveMessageSize =
            builder.Configuration.GetValue<int>("SignalR:MaximumReceiveMessageSize", 32768);
    });

    // Use Redis backplane for SignalR if enabled (for horizontal scaling)
    // NOTE: Redis backplane requires Microsoft.AspNetCore.SignalR.StackExchangeRedis NuGet package.
    // Currently disabled - SignalR uses in-memory backplane (single server only).
    // To enable: 1) Add package reference, 2) Set SignalR:UseRedisBackplane=true and Redis:Enabled=true in configuration, 3) Uncomment Redis backplane configuration below.
    var useRedisBackplane = builder.Configuration.GetValue<bool>("SignalR:UseRedisBackplane", false);
    if (useRedisBackplane && redisEnabled)
    {
        // var redisConnectionString = builder.Configuration.GetValue<string>("Redis:ConnectionString") ?? "localhost:6379";
        // signalRBuilder.AddStackExchangeRedis(redisConnectionString, options =>
        // {
        //     options.Configuration.ChannelPrefix = "GrcSignalR";
        // });
        // SignalR Redis backplane disabled - missing Microsoft.AspNetCore.SignalR.StackExchangeRedis package
    }
}

// Dashboard Hub Service (for pushing updates to clients)
builder.Services.AddScoped<GrcMvc.Hubs.IDashboardHubService, GrcMvc.Hubs.DashboardHubService>();

// PHASE 10: Admin Catalog Management Service
builder.Services.AddScoped<IAdminCatalogService, AdminCatalogService>();

// =============================================================================
// ORGANIZATION SETUP SERVICES (Post-Onboarding Configuration)
// =============================================================================
// OrgSetupController uses: GrcDbContext, ICurrentUserService, IOnboardingProvisioningService
// All these services are already registered above

// =============================================================================
// ONBOARDING WIZARD SERVICES (12-Step Wizard)
// =============================================================================
// OnboardingWizardController uses: GrcDbContext, IOnboardingProvisioningService, IRulesEngineService
// All these services are already registered above

// Register validators
builder.Services.AddScoped<IValidator<CreateRiskDto>, CreateRiskDtoValidator>();
builder.Services.AddScoped<IValidator<UpdateRiskDto>, UpdateRiskDtoValidator>();

// Configure cookie policy with enhanced security
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    // SECURITY: Use Always in production, SameAsRequest in development
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() 
        ? CookieSecurePolicy.SameAsRequest 
        : CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax; // Lax for authentication cookies
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.LogoutPath = "/Account/Logout";
    options.SlidingExpiration = true;
});

// =============================================================================
// NOTE: DbContext and Identity configurations are already defined above (lines 145-226)
// Skipping duplicate configuration to avoid conflicts
// =============================================================================

// =============================================================================
// 3. HANGFIRE CONFIGURATION (Background Jobs)
// =============================================================================

var enableHangfire = builder.Configuration.GetValue<bool>("Hangfire:Enabled", true);

if (enableHangfire)
{
    try
    {
        // Get connection string from configuration
        var hangfireConnectionString = connectionString ?? 
            throw new InvalidOperationException("Connection string not configured for Hangfire");
        
        // Test database connection before configuring Hangfire
        using var testConnection = new NpgsqlConnection(hangfireConnectionString);
        testConnection.Open();
        testConnection.Close();

        builder.Services.AddHangfire(config =>
        {
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                  .UseSimpleAssemblyNameTypeSerializer()
                  .UseRecommendedSerializerSettings()
                  .UsePostgreSqlStorage(options =>
                  {
                      options.UseNpgsqlConnection(hangfireConnectionString);
                  });
        });

        builder.Services.AddHangfireServer(options =>
        {
            options.WorkerCount = builder.Configuration.GetValue<int>("Hangfire:WorkerCount", Environment.ProcessorCount * 2);
            options.Queues = new[] { "critical", "default", "low" };
        });

        Console.WriteLine("✅ Hangfire configured successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Hangfire disabled: Database connection test failed - {ex.Message}");
        enableHangfire = false;
    }
}
else
{
    Console.WriteLine("⚠️ Hangfire disabled via configuration");
}

// Register background job classes
builder.Services.AddScoped<EscalationJob>();
builder.Services.AddScoped<NotificationDeliveryJob>();
builder.Services.AddScoped<SlaMonitorJob>();
builder.Services.AddScoped<WebhookRetryJob>();

// =============================================================================
// 3b. MASSTRANSIT CONFIGURATION (Message Queue)
// =============================================================================

var rabbitMqSettings = builder.Configuration.GetSection("RabbitMq").Get<RabbitMqSettings>() ?? new RabbitMqSettings();

if (rabbitMqSettings.Enabled)
{
    builder.Services.AddMassTransit(x =>
    {
        // Register consumers
        x.AddConsumer<NotificationConsumer>();
        x.AddConsumer<WebhookConsumer>();
        x.AddConsumer<GrcEventConsumer>();

        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(rabbitMqSettings.Host, rabbitMqSettings.VirtualHost, h =>
            {
                h.Username(rabbitMqSettings.Username);
                h.Password(rabbitMqSettings.Password);
            });

            cfg.PrefetchCount = rabbitMqSettings.PrefetchCount;

            // Configure retry policy
            cfg.UseMessageRetry(r =>
            {
                r.Intervals(rabbitMqSettings.RetryIntervals.Select(i => TimeSpan.FromSeconds(i)).ToArray());
            });

            // Configure endpoints
            cfg.ReceiveEndpoint("grc-notifications", e =>
            {
                e.ConfigureConsumer<NotificationConsumer>(context);
                e.ConcurrentMessageLimit = rabbitMqSettings.ConcurrencyLimit;
            });

            cfg.ReceiveEndpoint("grc-webhooks", e =>
            {
                e.ConfigureConsumer<WebhookConsumer>(context);
                e.ConcurrentMessageLimit = rabbitMqSettings.ConcurrencyLimit;
            });

            cfg.ReceiveEndpoint("grc-events", e =>
            {
                e.ConfigureConsumer<GrcEventConsumer>(context);
                e.ConcurrentMessageLimit = rabbitMqSettings.ConcurrencyLimit;
            });

            cfg.ConfigureEndpoints(context);
        });
    });

    Console.WriteLine("✅ MassTransit configured with RabbitMQ");
}
else
{
    // Use in-memory transport for development/testing when RabbitMQ is not available
    builder.Services.AddMassTransit(x =>
    {
        x.AddConsumer<NotificationConsumer>();
        x.AddConsumer<WebhookConsumer>();
        x.AddConsumer<GrcEventConsumer>();

        x.UsingInMemory((context, cfg) =>
        {
            cfg.ConfigureEndpoints(context);
        });
    });

    Console.WriteLine("⚠️ MassTransit using in-memory transport (RabbitMQ disabled)");
}

builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMq"));

// =============================================================================
// 4. CACHING CONFIGURATION
// =============================================================================

builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache();

// Response caching
builder.Services.AddResponseCaching();

// =============================================================================
// 5. WORKFLOW SETTINGS
// =============================================================================

builder.Services.Configure<WorkflowSettings>(
    builder.Configuration.GetSection("WorkflowSettings"));

builder.Services.Configure<SmtpSettings>(
    builder.Configuration.GetSection("SmtpSettings"));

// =============================================================================
// 6. HTTP CLIENT WITH POLLY RETRY POLICIES
// =============================================================================

// Retry policy for transient errors
var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(3, retryAttempt =>
        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

// Circuit breaker policy
var circuitBreakerPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));

// Register HTTP clients with policies
builder.Services.AddHttpClient("ExternalServices")
    .AddPolicyHandler(retryPolicy)
    .AddPolicyHandler(circuitBreakerPolicy);

builder.Services.AddHttpClient("EmailService")
    .AddPolicyHandler(retryPolicy);

// =============================================================================
// 7. SERVICE REGISTRATION
// =============================================================================

// Core services
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ISmtpEmailService, SmtpEmailService>();
builder.Services.AddScoped<IWebhookService, WebhookService>();

// Configuration Validation - Validates required settings at startup
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.AddHostedService<ConfigurationValidator>();

// AI Agent services - Multi-provider support (Claude, OpenAI, Azure, Gemini, Local LLMs)
builder.Services.AddScoped<IDiagnosticAgentService, DiagnosticAgentService>();
builder.Services.AddScoped<IClaudeAgentService, ClaudeAgentService>();
builder.Services.AddScoped<ISecurityAgentService, SecurityAgentService>();
builder.Services.AddScoped<IIntegrationAgentService, IntegrationAgentService>();
builder.Services.AddScoped<IUnifiedAiService, UnifiedAiService>();
builder.Services.Configure<ClaudeApiSettings>(builder.Configuration.GetSection(ClaudeApiSettings.SectionName));

// Integration Layer Services - External system sync, events, webhooks
builder.Services.AddScoped<ISyncExecutionService, SyncExecutionService>();
builder.Services.AddScoped<IEventPublisherService, EventPublisherService>();
builder.Services.AddScoped<IEventDispatcherService, EventDispatcherService>();
builder.Services.AddScoped<IWebhookDeliveryService, WebhookDeliveryService>();
builder.Services.AddSingleton<ICredentialEncryptionService, CredentialEncryptionService>();
builder.Services.AddHttpClient("WebhookClient")
    .AddPolicyHandler(retryPolicy)
    .AddPolicyHandler(circuitBreakerPolicy);

// Email Operations Services (Shahin + Dogan Consult)
builder.Services.AddHttpClient<GrcMvc.Services.EmailOperations.IMicrosoftGraphEmailService,
    GrcMvc.Services.EmailOperations.MicrosoftGraphEmailService>();
builder.Services.AddScoped<GrcMvc.Services.EmailOperations.IEmailAiService,
    GrcMvc.Services.EmailOperations.EmailAiService>();
builder.Services.AddScoped<GrcMvc.Services.EmailOperations.IEmailOperationsService,
    GrcMvc.Services.EmailOperations.EmailOperationsService>();
builder.Services.AddScoped<GrcMvc.Services.EmailOperations.EmailProcessingJob>();

// Multi-channel notification services
builder.Services.AddScoped<ISlackNotificationService, SlackNotificationService>();
builder.Services.AddScoped<ITeamsNotificationService, TeamsNotificationService>();
builder.Services.AddScoped<ISmsNotificationService, TwilioSmsService>();

// Configuration bindings
builder.Services.Configure<WebhookSettings>(builder.Configuration.GetSection("Webhooks"));
builder.Services.Configure<SlackSettings>(builder.Configuration.GetSection("Slack"));
builder.Services.Configure<TeamsSettings>(builder.Configuration.GetSection("Teams"));
builder.Services.Configure<TwilioSettings>(builder.Configuration.GetSection("Twilio"));

// =============================================================================
// KAFKA & CAMUNDA WORKFLOW ORCHESTRATION
// =============================================================================

// Kafka Event-Driven Architecture (disabled in Development - requires Kafka server)
builder.Services.Configure<GrcMvc.Services.Kafka.KafkaSettings>(
    builder.Configuration.GetSection(GrcMvc.Services.Kafka.KafkaSettings.SectionName));
builder.Services.AddSingleton<GrcMvc.Services.Kafka.IKafkaProducer, GrcMvc.Services.Kafka.KafkaProducer>();
if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddHostedService<GrcMvc.Services.Kafka.KafkaConsumerService>();
}

// Camunda Workflow Orchestration
builder.Services.Configure<GrcMvc.Services.Camunda.CamundaSettings>(
    builder.Configuration.GetSection(GrcMvc.Services.Camunda.CamundaSettings.SectionName));
builder.Services.AddHttpClient<GrcMvc.Services.Camunda.ICamundaService, GrcMvc.Services.Camunda.CamundaService>(client =>
{
    var camundaSettings = builder.Configuration.GetSection("Camunda");
    client.BaseAddress = new Uri(camundaSettings["BaseUrl"] ?? "http://localhost:8080/camunda");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// =============================================================================
// 8. MVC & API CONFIGURATION
// =============================================================================

// NOTE: AddControllersWithViews() already registered at line ~149 with filters
builder.Services.AddRazorPages();

// API versioning (optional)
builder.Services.AddEndpointsApiExplorer();

// =============================================================================
// 9. AUTHENTICATION & AUTHORIZATION
// =============================================================================

// NOTE: AddAuthentication() and AddAuthorization() already registered at lines ~306 and ~330
// with full JWT and policy configuration. Skipping duplicate registration.

// =============================================================================
// 10. CORS CONFIGURATION
// =============================================================================

// NOTE: AddCors() already registered at line ~99 with "AllowApiClients" policy.
// Adding "AllowSpecificOrigins" policy to existing CORS configuration is done there.

// =============================================================================
// BUILD APPLICATION
// =============================================================================

var app = builder.Build();

// =============================================================================
// AUTO-MIGRATE DATABASE ON STARTUP
// =============================================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        Console.WriteLine("🔄 Applying database migrations...");
        var dbContext = services.GetRequiredService<GrcDbContext>();
        dbContext.Database.Migrate();
        Console.WriteLine("✅ Database migrations applied successfully");
        
        // Also migrate Auth database
        var authContext = services.GetRequiredService<GrcAuthDbContext>();
        authContext.Database.Migrate();
        Console.WriteLine("✅ Auth database migrations applied successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Migration error: {ex.Message}");
    }
}

// =============================================================================
// 11. MIDDLEWARE PIPELINE
// =============================================================================

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Global Exception Handling (must be early in pipeline)
app.UseMiddleware<GrcMvc.Middleware.GlobalExceptionMiddleware>();

// Configure Request Localization (must be before UseStaticFiles and UseRouting)
var localizationOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
app.UseRequestLocalization(localizationOptions);

// Status code pages with re-execute for friendly error pages
app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");

// Policy Violation Exception Middleware (early in pipeline for API error handling)
// Owner Setup Middleware (must run early, before authentication)
app.UseMiddleware<GrcMvc.Middleware.OwnerSetupMiddleware>();

app.UseMiddleware<GrcMvc.Middleware.PolicyViolationExceptionMiddleware>();

// Domain-based tenant resolution middleware
// Resolves tenant from subdomain (e.g., acme.grcsystem.com) and stores in HttpContext.Items
// CRITICAL: Enabled for early tenant resolution - improves performance by avoiding repeated DB lookups
app.UseMiddleware<GrcMvc.Middleware.TenantResolutionMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();

// MEDIUM PRIORITY FIX: Add security headers (CSP, X-Frame-Options, etc.) - must be early in pipeline
app.UseSecurityHeaders();

// Host-based routing (login.shahin-ai.com → Admin, shahin-ai.com → Landing)
app.UseHostRouting();

app.UseRouting();

// Rate Limiting (must be after UseRouting, before UseAuthentication)
app.UseRateLimiter();

// Session (required for workspace context storage)
app.UseSession();

// CORS
app.UseCors("AllowApiClients");

// Response caching
app.UseResponseCaching();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// =============================================================================
// 12. HANGFIRE DASHBOARD
// =============================================================================

var appLogger = app.Services.GetRequiredService<ILogger<Program>>();

if (enableHangfire)
{
    var dashboardPath = builder.Configuration.GetValue<string>("Hangfire:DashboardPath", "/hangfire");
    app.UseHangfireDashboard(dashboardPath, new DashboardOptions
    {
        Authorization = new[] { new HangfireAuthFilter() },
        DashboardTitle = "Shahin GRC - Background Jobs",
        DisplayStorageConnectionString = false
    });
    appLogger.LogInformation("✅ Hangfire dashboard enabled at {Path}", dashboardPath);
}
else
{
    appLogger.LogWarning("⚠️ Hangfire dashboard disabled");
}

// =============================================================================
// 13. CONFIGURE RECURRING JOBS
// =============================================================================

if (enableHangfire)
{
    // Notification delivery - every 5 minutes
    RecurringJob.AddOrUpdate<NotificationDeliveryJob>(
        "notification-delivery",
        job => job.ExecuteAsync(),
        "*/5 * * * *",
        new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });

    // Escalation check - every hour
    RecurringJob.AddOrUpdate<EscalationJob>(
        "escalation-check",
        job => job.ExecuteAsync(),
        "0 * * * *",
        new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });

    // SLA monitoring - every 15 minutes
    RecurringJob.AddOrUpdate<SlaMonitorJob>(
        "sla-monitor",
        job => job.ExecuteAsync(),
        "*/15 * * * *",
        new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });

    // Webhook retry - every 2 minutes
    RecurringJob.AddOrUpdate<WebhookRetryJob>(
        "webhook-retry",
        job => job.ExecuteAsync(),
        "*/2 * * * *",
        new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });

    // Email Operations - SLA breach check every hour
    RecurringJob.AddOrUpdate<GrcMvc.Services.EmailOperations.EmailProcessingJob>(
        "email-sla-check",
        job => job.CheckSlaBreachesAsync(),
        "0 * * * *", // Every hour at minute 0
        new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });

    appLogger.LogInformation("✅ Email Operations SLA check job scheduled");

    // Integration Layer - Sync scheduler every 5 minutes
    RecurringJob.AddOrUpdate<SyncSchedulerJob>(
        "sync-scheduler",
        job => job.ProcessScheduledSyncsAsync(),
        "*/5 * * * *",
        new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });

    // Integration Layer - Event dispatcher every 1 minute
    RecurringJob.AddOrUpdate<EventDispatcherJob>(
        "event-dispatcher-pending",
        job => job.ProcessPendingEventsAsync(),
        "*/1 * * * *",
        new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });

    // Integration Layer - Event delivery retry every 5 minutes
    RecurringJob.AddOrUpdate<EventDispatcherJob>(
        "event-dispatcher-retry",
        job => job.RetryFailedEventsAsync(),
        "*/5 * * * *",
        new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });

    // Integration Layer - Dead letter queue cleanup every 30 minutes
    RecurringJob.AddOrUpdate<EventDispatcherJob>(
        "event-dead-letter-queue",
        job => job.MoveToDeadLetterQueueAsync(),
        "*/30 * * * *",
        new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });

    // Integration Layer - Health monitoring every 15 minutes
    RecurringJob.AddOrUpdate<IntegrationHealthMonitorJob>(
        "integration-health-monitor",
        job => job.MonitorAllIntegrationsAsync(),
        "*/15 * * * *",
        new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });

    appLogger.LogInformation("✅ Integration layer background jobs scheduled: sync, events, health monitoring");

    // Analytics projection jobs (only if ClickHouse is enabled)
    var analyticsEnabled = builder.Configuration.GetValue<bool>("Analytics:Enabled", false);
    if (analyticsEnabled)
    {
        // Full analytics projection - every 15 minutes
        RecurringJob.AddOrUpdate<GrcMvc.BackgroundJobs.AnalyticsProjectionJob>(
            "analytics-full-projection",
            job => job.ExecuteAsync(),
            "*/15 * * * *",
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });

        // Snapshot projection - every 5 minutes (lighter weight)
        RecurringJob.AddOrUpdate<GrcMvc.BackgroundJobs.AnalyticsProjectionJob>(
            "analytics-snapshot",
            job => job.ExecuteSnapshotsOnlyAsync(),
            "*/5 * * * *",
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });

        // Top actions - every 2 minutes (real-time feel)
        RecurringJob.AddOrUpdate<GrcMvc.BackgroundJobs.AnalyticsProjectionJob>(
            "analytics-top-actions",
            job => job.ExecuteTopActionsAsync(),
            "*/2 * * * *",
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });

        appLogger.LogInformation("✅ Analytics projection jobs configured");
    }

    appLogger.LogInformation("✅ Recurring jobs configured: notification-delivery, escalation-check, sla-monitor, webhook-retry");
}
// =============================================================================
// 14. ENDPOINT MAPPING
// =============================================================================

// SignalR Hub for real-time dashboard updates
var signalREnabledForHub = app.Configuration.GetValue<bool>("SignalR:Enabled", true);
if (signalREnabledForHub)
{
    app.MapHub<GrcMvc.Hubs.DashboardHub>("/hubs/dashboard");
    appLogger.LogInformation("✅ SignalR Dashboard Hub mapped to /hubs/dashboard");
}

// Platform Admin routes (login.shahin-ai.com) - MUST BE BEFORE OTHER ROUTES
app.MapControllerRoute(
    name: "admin-portal",
    pattern: "admin/{action=Login}/{id?}",
    defaults: new { controller = "AdminPortal" });

// Owner routes (PlatformAdmin only)
app.MapControllerRoute(
    name: "owner",
    pattern: "owner/{controller=Owner}/{action=Index}/{id?}",
    defaults: new { controller = "Owner" });

// Tenant-specific routes
app.MapControllerRoute(
    name: "tenant",
    pattern: "tenant/{slug}/{controller=Home}/{action=Index}/{id?}",
    constraints: new { slug = @"[a-z0-9-]+" },
    defaults: new { controller = "Home" });

// Tenant admin routes
app.MapControllerRoute(
    name: "tenant-admin",
    pattern: "tenant/{slug}/admin/{controller=Dashboard}/{action=Index}/{id?}",
    constraints: new { slug = @"[a-z0-9-]+" },
    defaults: new { controller = "Dashboard" });

// Onboarding Wizard Routes (12-step wizard)
app.MapControllerRoute(
    name: "onboarding-wizard",
    pattern: "OnboardingWizard/{action=Index}/{tenantId?}",
    defaults: new { controller = "OnboardingWizard" });

// Organization Setup Routes (post-onboarding configuration)
app.MapControllerRoute(
    name: "org-setup",
    pattern: "OrgSetup/{action=Index}/{id?}",
    defaults: new { controller = "OrgSetup" });

// Onboarding Routes (legacy flow)
app.MapControllerRoute(
    name: "onboarding",
    pattern: "Onboarding/{action=Index}/{id?}",
    defaults: new { controller = "Onboarding" });

// Login redirect route (maps /login-redirect to AccountController.LoginRedirect)
app.MapControllerRoute(
    name: "login-redirect",
    pattern: "login-redirect",
    defaults: new { controller = "Account", action = "LoginRedirect" });

// Enable attribute routing for API and custom-routed controllers
app.MapControllers();
// #region agent log
app.Logger.LogInformation("✅ Attribute routing enabled via MapControllers()");
// #endregion

// #region agent log - Route registration tracking
app.Logger.LogInformation("Registering routes: MapControllers, Landing route, Default route");
// #endregion

// Landing page route (shahin-ai.com homepage)
// NOTE: This must be after MapControllers() so attribute routes have priority
app.MapControllerRoute(
    name: "landing",
    pattern: "",
    defaults: new { controller = "Landing", action = "Index" });

// Plural route redirects (common convention aliases)
app.MapGet("/Risks", () => Results.Redirect("/Risk"));
app.MapGet("/Policies", () => Results.Redirect("/Policy"));
app.MapGet("/Audits", () => Results.Redirect("/Audit"));
app.MapGet("/Assessments", () => Results.Redirect("/Assessment"));

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// Health check endpoints - ASP.NET Core Best Practice
// /health - Full health check with database connectivity
// /health/ready - Readiness probe for Kubernetes
// /health/live - Liveness probe for Kubernetes
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            version = "2.0.0",
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds
            })
        });
        await context.Response.WriteAsync(result);
    }
});

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("db")
});

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("api")
});

// =============================================================================
// 15. INITIALIZE SEED DATA (Run on startup)
// =============================================================================

// Initialize seed data asynchronously (don't block startup)
var logger = app.Services.GetRequiredService<ILogger<Program>>();
_ = Task.Run(async () =>
{
    try
    {
        await Task.Delay(5000); // Wait for app to be fully ready
        using var scope = app.Services.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<ApplicationInitializer>();
        logger.LogInformation("🚀 Starting application initialization (seed data)...");
        await initializer.InitializeAsync();
        logger.LogInformation("✅ Application initialization completed");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Failed to initialize seed data");
    }
});

// =============================================================================
// 16. RUN APPLICATION
// =============================================================================

app.Run();

// =============================================================================
// SMTP SETTINGS CLASS
// =============================================================================

public class SmtpSettings
{
    public string Host { get; set; } = "smtp.office365.com";
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string FromEmail { get; set; } = "noreply@grcsystem.com";
    public string FromName { get; set; } = "GRC System";
    public string? Username { get; set; }
    public string? Password { get; set; }

    // OAuth2 settings for Office 365 (recommended for production)
    public bool UseOAuth2 { get; set; } = false;
    public string? TenantId { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
}
