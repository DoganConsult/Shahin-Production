using GrcMvc.BackgroundJobs;
using GrcMvc.Data;
using GrcMvc.Exceptions;
using GrcMvc.Security;
using GrcMvc.Services.Implementations;
using GrcMvc.Services.Implementations.Workflows;
using GrcMvc.Services.Interfaces;
using GrcMvc.Services.Interfaces.Workflows;
using GrcMvc.Services.Interfaces.RBAC;
using GrcMvc.Services.Implementations.RBAC;
using GrcMvc.Services.Implementations.UserProfiles;
using Hangfire;
using Hangfire.PostgreSql;
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

var builder = WebApplication.CreateBuilder(args);

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

// Add services to the container with FluentValidation
builder.Services.AddControllersWithViews(options =>
{
    // Add global anti-forgery token validation
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
}).AddRazorRuntimeCompilation();
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
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection(EmailSettings.SectionName));

// Validate configuration at startup
builder.Services.AddSingleton<IValidateOptions<JwtSettings>, JwtSettingsValidator>();
builder.Services.AddSingleton<IValidateOptions<ApplicationSettings>, ApplicationSettingsValidator>();

// Configure Entity Framework with PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Database connection string 'DefaultConnection' not found. " +
        "Please set it via environment variable: ConnectionStrings__DefaultConnection");
}

builder.Services.AddDbContext<GrcDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(
        connectionString!,
        name: "database",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "db", "postgresql" })
    .AddCheck("self", () => HealthCheckResult.Healthy("Application is running"),
        tags: new[] { "api" });

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
                PermitLimit = 100,
                QueueLimit = 0,
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
.AddEntityFrameworkStores<GrcDbContext>()
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
});

// Add session support with enhanced security
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20); // Reduced from 30
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// Configure anti-forgery tokens
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "X-CSRF-TOKEN";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Register repositories and Unit of Work
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register services
builder.Services.AddScoped<IRiskService, RiskService>();
builder.Services.AddScoped<IControlService, ControlService>();
builder.Services.AddScoped<IAssessmentService, AssessmentService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IPolicyService, PolicyService>();
builder.Services.AddScoped<IWorkflowService, WorkflowService>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddTransient<IAppEmailSender, SmtpEmailSender>();

// PHASE 1: Register critical services for Framework Data, HRIS, Audit Trail, and Rules Engine
builder.Services.AddScoped<IFrameworkService, Phase1FrameworkService>();
builder.Services.AddScoped<IHRISService, HRISService>();
builder.Services.AddScoped<IAuditTrailService, AuditTrailService>();
builder.Services.AddScoped<IRulesEngineService, StubRulesEngineService>();

// Register new STAGE 1 services
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IOnboardingService, OnboardingService>();
builder.Services.AddScoped<IAuditEventService, AuditEventService>();
builder.Services.AddScoped<IEmailService, StubEmailService>();
builder.Services.AddScoped<IPlanService, PlanService>();

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
builder.Services.AddScoped<IUserProfileService, UserProfileService>();

// Register STAGE 2 Enterprise LLM service
builder.Services.AddScoped<ILlmService, LlmService>();
builder.Services.AddHttpClient<ILlmService, LlmService>();

// Register Subscription & Billing service
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();

// Register Evidence and Report services
builder.Services.AddScoped<IEvidenceService, EvidenceService>();
builder.Services.AddScoped<IReportService, ReportService>();

// Register Authentication and Authorization services
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();

// Register Application Initializer for seed data
builder.Services.AddScoped<ApplicationInitializer>();

// Register Catalog Seeder Service
builder.Services.AddScoped<CatalogSeederService>();

// Register Workflow Definition Seeder Service
builder.Services.AddScoped<WorkflowDefinitionSeederService>();

// Register Framework Control Import Service
builder.Services.AddScoped<FrameworkControlImportService>();

// PHASE 6: User Invitation Service
builder.Services.AddScoped<IUserInvitationService, UserInvitationService>();

// PHASE 8: Evidence Lifecycle Service
builder.Services.AddScoped<IEvidenceLifecycleService, EvidenceLifecycleService>();

// PHASE 9: Dashboard Service
builder.Services.AddScoped<IDashboardService, DashboardService>();

// PHASE 10: Admin Catalog Management Service
builder.Services.AddScoped<IAdminCatalogService, AdminCatalogService>();

// Register validators
builder.Services.AddScoped<IValidator<CreateRiskDto>, CreateRiskDtoValidator>();
builder.Services.AddScoped<IValidator<UpdateRiskDto>, UpdateRiskDtoValidator>();

// Configure cookie policy with enhanced security
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
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

builder.Services.AddHangfire(config =>
{
    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
          .UseSimpleAssemblyNameTypeSerializer()
          .UseRecommendedSerializerSettings()
          .UsePostgreSqlStorage(options =>
          {
              options.UseNpgsqlConnection(connectionString);
          });
});

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = Environment.ProcessorCount * 2;
    options.Queues = new[] { "critical", "default", "low" };
});

// Register background job classes
builder.Services.AddScoped<EscalationJob>();
builder.Services.AddScoped<NotificationDeliveryJob>();
builder.Services.AddScoped<SlaMonitorJob>();

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

// Workflow services (add your existing workflow services here)
// builder.Services.AddScoped<IControlImplementationService, ControlImplementationService>();
// builder.Services.AddScoped<IApprovalWorkflowService, ApprovalWorkflowService>();
// ... etc.

// =============================================================================
// 8. MVC & API CONFIGURATION
// =============================================================================

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// API versioning (optional)
builder.Services.AddEndpointsApiExplorer();

// =============================================================================
// 9. AUTHENTICATION & AUTHORIZATION
// =============================================================================

builder.Services.AddAuthentication();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ComplianceOfficer", policy => policy.RequireRole("Admin", "ComplianceOfficer"));
    options.AddPolicy("Auditor", policy => policy.RequireRole("Admin", "Auditor"));
    options.AddPolicy("RiskManager", policy => policy.RequireRole("Admin", "RiskManager"));
});

// =============================================================================
// 10. CORS CONFIGURATION
// =============================================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins(
                builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? new[] { "https://localhost:5001" })
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// =============================================================================
// BUILD APPLICATION
// =============================================================================

var app = builder.Build();

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

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// CORS
app.UseCors("AllowSpecificOrigins");

// Response caching
app.UseResponseCaching();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// =============================================================================
// 12. HANGFIRE DASHBOARD
// =============================================================================

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthFilter() },
    DashboardTitle = "GRC Background Jobs",
    DisplayStorageConnectionString = false
});

// =============================================================================
// 13. CONFIGURE RECURRING JOBS
// =============================================================================

// Configure recurring background jobs
RecurringJob.AddOrUpdate<EscalationJob>(
    "process-escalations",
    job => job.ExecuteAsync(),
    Cron.Hourly,
    new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

RecurringJob.AddOrUpdate<NotificationDeliveryJob>(
    "deliver-notifications",
    job => job.ExecuteAsync(),
    "*/5 * * * *", // Every 5 minutes
    new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

RecurringJob.AddOrUpdate<SlaMonitorJob>(
    "monitor-sla",
    job => job.ExecuteAsync(),
    "*/30 * * * *", // Every 30 minutes
    new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

// =============================================================================
// 14. ENDPOINT MAPPING
// =============================================================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    timestamp = DateTime.UtcNow,
    version = "2.0.0"
}));

// =============================================================================
// 15. RUN APPLICATION
// =============================================================================

app.Run();

// =============================================================================
// SMTP SETTINGS CLASS
// =============================================================================

public class SmtpSettings
{
    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string FromEmail { get; set; } = "noreply@grcsystem.com";
    public string FromName { get; set; } = "GRC System";
    public string? Username { get; set; }
    public string? Password { get; set; }
}
