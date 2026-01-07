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
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
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
}).AddRazorRuntimeCompilation()
.AddViewLocalization()
.AddDataAnnotationsLocalization(options =>
{
    options.DataAnnotationLocalizerProvider = (type, factory) =>
        factory.Create(typeof(GrcMvc.Resources.SharedResource));
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

// Register master DbContext for tenant metadata (uses default connection)
builder.Services.AddDbContext<GrcDbContext>(options =>
    options.UseNpgsql(connectionString), ServiceLifetime.Scoped);

// Register Auth DbContext for Identity (separate database)
var authConnectionString = builder.Configuration.GetConnectionString("GrcAuthDb") ?? connectionString;
builder.Services.AddDbContext<GrcAuthDbContext>(options =>
    options.UseNpgsql(authConnectionString), ServiceLifetime.Scoped);

// Register tenant database resolver
builder.Services.AddScoped<ITenantDatabaseResolver, TenantDatabaseResolver>();

// Register tenant-aware DbContext factory
builder.Services.AddScoped<IDbContextFactory<GrcDbContext>, TenantAwareDbContextFactory>();

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(
        connectionString!,
        name: "master-database",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "db", "postgresql", "master" })
    .AddCheck<GrcMvc.HealthChecks.TenantDatabaseHealthCheck>(
        name: "tenant-database",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "db", "postgresql", "tenant" })
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Application is running"),
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
// RiskService migrated to IDbContextFactory for tenant database isolation
builder.Services.AddScoped<IRiskService, RiskService>();
builder.Services.AddScoped<IControlService, ControlService>();
builder.Services.AddScoped<IAssessmentService, AssessmentService>();
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

// Register new STAGE 1 services
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IOnboardingService, OnboardingService>();
builder.Services.AddScoped<IOnboardingWizardService, OnboardingWizardService>();
builder.Services.AddScoped<IAuditEventService, AuditEventService>();
// Use real SMTP email service via adapter (replaces StubEmailService)
builder.Services.AddScoped<IEmailService, EmailServiceAdapter>();
builder.Services.AddScoped<IPlanService, PlanService>();

// Workflow Services
builder.Services.AddScoped<GrcMvc.Services.Interfaces.IEvidenceWorkflowService, GrcMvc.Services.Implementations.EvidenceWorkflowService>();
builder.Services.AddScoped<GrcMvc.Services.Interfaces.IRiskWorkflowService, GrcMvc.Services.Implementations.RiskWorkflowService>();

// Owner tenant management
builder.Services.AddScoped<IOwnerTenantService, OwnerTenantService>();
builder.Services.AddScoped<IOwnerSetupService, OwnerSetupService>();
builder.Services.AddScoped<ICredentialDeliveryService, CredentialDeliveryService>();

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

// Register Menu Service (RBAC-based navigation)
builder.Services.AddScoped<GrcMvc.Data.Menu.GrcMenuContributor>();
builder.Services.AddScoped<IMenuService, MenuService>();

// Register Authentication and Authorization services
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
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
// TODO: Add Microsoft.Extensions.Caching.StackExchangeRedis package to enable
var redisEnabled = builder.Configuration.GetValue<bool>("Redis:Enabled", false);
if (redisEnabled)
{
    // var redisConnectionString = builder.Configuration.GetValue<string>("Redis:ConnectionString") ?? "localhost:6379";
    // builder.Services.AddStackExchangeRedisCache(options =>
    // {
    //     options.Configuration = redisConnectionString;
    //     options.InstanceName = builder.Configuration.GetValue<string>("Redis:InstanceName") ?? "GrcCache_";
    // });
    // Redis caching disabled - missing Microsoft.Extensions.Caching.StackExchangeRedis package
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

    // Use Redis backplane for SignalR if enabled
    // TODO: Add Microsoft.AspNetCore.SignalR.StackExchangeRedis package to enable
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

var enableHangfire = builder.Configuration.GetValue<bool>("Hangfire:Enabled", true);

if (enableHangfire)
{
    try
    {
        // Test database connection before configuring Hangfire
        using var testConnection = new NpgsqlConnection(connectionString);
        testConnection.Open();
        testConnection.Close();

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

// Diagnostic agent service
builder.Services.AddScoped<IDiagnosticAgentService, DiagnosticAgentService>();
builder.Services.Configure<ClaudeApiSettings>(builder.Configuration.GetSection(ClaudeApiSettings.SectionName));

// Multi-channel notification services
builder.Services.AddScoped<ISlackNotificationService, SlackNotificationService>();
builder.Services.AddScoped<ITeamsNotificationService, TeamsNotificationService>();
builder.Services.AddScoped<ISmsNotificationService, TwilioSmsService>();

// Configuration bindings
builder.Services.Configure<WebhookSettings>(builder.Configuration.GetSection("Webhooks"));
builder.Services.Configure<SlackSettings>(builder.Configuration.GetSection("Slack"));
builder.Services.Configure<TeamsSettings>(builder.Configuration.GetSection("Teams"));
builder.Services.Configure<TwilioSettings>(builder.Configuration.GetSection("Twilio"));

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

// Configure Request Localization (must be before UseStaticFiles and UseRouting)
var localizationOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
app.UseRequestLocalization(localizationOptions);

// Policy Violation Exception Middleware (early in pipeline for API error handling)
// Owner Setup Middleware (must run early, before authentication)
app.UseMiddleware<GrcMvc.Middleware.OwnerSetupMiddleware>();

app.UseMiddleware<GrcMvc.Middleware.PolicyViolationExceptionMiddleware>();

// Optional: Domain-based tenant resolution middleware
// Only uncomment if you're using subdomains (e.g., acme.grcsystem.com)
// app.UseMiddleware<GrcMvc.Middleware.TenantResolutionMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Session (required for workspace context storage)
app.UseSession();

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

// Default route
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
    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string FromEmail { get; set; } = "noreply@grcsystem.com";
    public string FromName { get; set; } = "GRC System";
    public string? Username { get; set; }
    public string? Password { get; set; }
}
