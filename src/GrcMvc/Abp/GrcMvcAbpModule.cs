using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.MultiTenancy;
using Volo.Abp.Auditing;
using Volo.Abp.AuditLogging;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Caching;
using Volo.Abp.Caching.StackExchangeRedis;
using Volo.Abp.Emailing;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.PostgreSql;
using Volo.Abp.FeatureManagement;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.OpenIddict;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.OpenIddict.WildcardDomains;
using Volo.Abp.PermissionManagement;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;
using Volo.Abp.Validation;
using Volo.Abp.Account;
using Volo.Abp.AspNetCore.Mvc.UI.Theming;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using GrcMvc.Data;
using GrcMvc.Data.Repositories;
using GrcMvc.Abp.Services;
using GrcMvc.Services.Interfaces;
using GrcMvc.Services.Implementations;
using GrcMvc.Services.Implementations.Workflows;
using GrcMvc.Services.Implementations.RBAC;
using GrcMvc.Services.Kafka;
using GrcMvc.Filters;
using GrcMvc.Configuration;
using GrcMvc.Validators;
using GrcMvc.Models.DTOs;
using GrcMvc.BackgroundJobs;
using GrcMvc.Hubs;

namespace GrcMvc.Abp;

/// <summary>
/// ABP Framework Module for Shahin GRC Platform
///
/// This module integrates ABP's enterprise features:
/// - Multi-tenancy with tenant isolation (subdomain-based)
/// - OpenIddict SSO Server for enterprise authentication
/// - Audit logging for compliance
/// - Feature management per tenant/edition
/// - Identity and permission management
///
/// All packages used are FREE open-source (no license required)
/// </summary>
[DependsOn(
    // ABP Core modules (2 modules)
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreMvcModule),

    // Database - EF Core integration (2 modules)
    typeof(AbpEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCorePostgreSqlModule),

    // Multi-tenancy module (1 module)
    typeof(AbpAspNetCoreMultiTenancyModule),

    // Tenant Management - Domain + Application + EF Core (3 modules)
    typeof(AbpTenantManagementDomainModule),
    typeof(AbpTenantManagementApplicationModule),
    typeof(AbpTenantManagementEntityFrameworkCoreModule),

    // Identity Management - Domain + Application + EF Core (3 modules)
    typeof(AbpIdentityDomainModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpIdentityEntityFrameworkCoreModule),

    // Permission Management - Domain + Application + EF Core (3 modules)
    typeof(AbpPermissionManagementDomainModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpPermissionManagementEntityFrameworkCoreModule),

    // Feature Management - Domain + Application + EF Core (3 modules)
    typeof(AbpFeatureManagementDomainModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpFeatureManagementEntityFrameworkCoreModule),

    // Audit Logging - Domain + EF Core (2 modules)
    typeof(AbpAuditLoggingDomainModule),
    typeof(AbpAuditLoggingEntityFrameworkCoreModule),

    // Settings Management - Domain + Application + EF Core (3 modules)
    typeof(AbpSettingManagementDomainModule),
    typeof(AbpSettingManagementApplicationModule),
    typeof(AbpSettingManagementEntityFrameworkCoreModule),

    // OpenIddict for SSO - Domain + AspNetCore + EF Core (3 modules)
    typeof(AbpOpenIddictDomainModule),
    typeof(AbpOpenIddictAspNetCoreModule),
    typeof(AbpOpenIddictEntityFrameworkCoreModule),

    // Account Module - Application only (Web module disabled to use custom Account pages)
    // Note: AbpAccountWebModule requires ABP theming - we use custom pages in Pages/Account/
    typeof(AbpAccountApplicationModule),
    // typeof(AbpAccountWebModule), // DISABLED - using custom Razor pages with Layout = null

    // Background Jobs - Domain + EF Core (2 modules)
    typeof(AbpBackgroundJobsDomainModule),
    typeof(AbpBackgroundJobsEntityFrameworkCoreModule),

    // Email Sending (ABP built-in)
    typeof(AbpEmailingModule),

    // Caching (ABP built-in) - Redis enabled
    typeof(AbpCachingModule),
    typeof(AbpCachingStackExchangeRedisModule), // Enabled - Redis caching for distributed scenarios

    // Localization (ABP built-in)
    typeof(AbpLocalizationModule),

    // Validation (ABP built-in)
    typeof(AbpValidationModule)
)]
public class GrcMvcAbpModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        
        // Pre-configure OpenIddict Validation (consolidated here, removed duplicate from ConfigureServices)
        PreConfigure<OpenIddictBuilder>(builder =>
        {
            builder.AddValidation(options =>
            {
                // Set issuer (use configuration or default)
                var issuer = configuration["OpenIddict:Issuer"] ?? "https://grcsystem.com";
                options.SetIssuer(issuer);
                
                // Add audiences for API validation
                options.AddAudiences("grc-api", "GrcMvc");
                
                // Use local server for token validation
                options.UseLocalServer();
                
                // Integrate with ASP.NET Core authentication pipeline
                options.UseAspNetCore();
                
                // Enable token entry validation (validates tokens stored in database)
                options.EnableTokenEntryValidation();
            });
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        // Enable ABP Multi-Tenancy
        Configure<AbpMultiTenancyOptions>(options =>
        {
            options.IsEnabled = true;
        });

        // Note: ABP Theming not configured - AbpAccountWebModule is disabled
        // We use custom Account pages with Layout = null (standalone HTML)
        // Custom Pages/_ViewStart.cshtml overrides ABP's default ViewStart

        // Enable ABP Auditing
        Configure<AbpAuditingOptions>(options =>
        {
            options.IsEnabled = true;
            options.ApplicationName = "ShahinGRC";
            options.IsEnabledForGetRequests = false; // Don't audit GET requests
            options.IsEnabledForAnonymousUsers = false; // Don't audit anonymous users
        });

        // ABP Background Jobs - Disabled (using Hangfire for complex workflows)
        Configure<AbpBackgroundJobOptions>(options =>
        {
            options.IsJobExecutionEnabled = false; // Use Hangfire for complex jobs
        });
        
        // ABP Background Workers - Disabled to avoid OpenIddict token cleanup worker bug in ABP 8.x
        // The bug causes NullReferenceException in BackgroundWorkerBase.get_Logger()
        Configure<AbpBackgroundWorkerOptions>(options =>
        {
            options.IsEnabled = false; // Disabled - use Hangfire for background tasks instead
        });

        // ABP Email uses settings from database/config via ISettingProvider
        // SMTP settings configured in appsettings.json under "Settings" section
        // Or use ABP Setting Management UI

        // Configure ABP Caching with Redis
        Configure<AbpDistributedCacheOptions>(options =>
        {
            options.KeyPrefix = "ShahinGrc:";
        });

        // Configure OpenIddict Server (OAuth2/OIDC endpoints)
        context.Services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                       .UseDbContext<GrcDbContext>();
            })
            .AddServer(options =>
            {
                // OAuth2/OIDC endpoints
                options.SetAuthorizationEndpointUris("/connect/authorize")
                       .SetTokenEndpointUris("/connect/token")
                       .SetUserinfoEndpointUris("/connect/userinfo")
                       .SetLogoutEndpointUris("/connect/logout")
                       .SetIntrospectionEndpointUris("/connect/introspect")
                       .SetRevocationEndpointUris("/connect/revocat");

                // Supported flows
                options.AllowAuthorizationCodeFlow()
                       .AllowRefreshTokenFlow()
                       .AllowClientCredentialsFlow()
                       .AllowPasswordFlow(); // For legacy support

                // Scopes
                options.RegisterScopes("openid", "profile", "email", "offline_access", "roles");

                // ASP.NET Core integration
                options.UseAspNetCore()
                       .EnableAuthorizationEndpointPassthrough()
                       .EnableTokenEndpointPassthrough()
                       .EnableUserinfoEndpointPassthrough()
                       .EnableLogoutEndpointPassthrough()
                       .EnableStatusCodePagesIntegration();
            });
            // Note: Validation is configured in PreConfigureServices to avoid duplication

        // Configure OpenIddict development certificate (for local dev)
        PreConfigure<AbpOpenIddictAspNetCoreOptions>(options =>
        {
            options.AddDevelopmentEncryptionAndSigningCertificate = true;
        });

        // Configure Permission Management
        Configure<PermissionManagementOptions>(options =>
        {
            // Default permission providers
        });

        // Configure Feature Management
        Configure<FeatureManagementOptions>(options =>
        {
            // Default feature providers
        });

        // Note: Exception handling is configured automatically by ABP
        // Custom exception handling can be added via middleware if needed

        // Configure ABP Entity Framework Core
        Configure<AbpDbContextOptions>(options =>
        {
            options.UseNpgsql();
        });

        // Register GrcDbContext with ABP
        context.Services.AddAbpDbContext<GrcDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
        });

        // FeatureCheckService wraps ABP's IFeatureChecker with backward-compatible API
        // Uses IFeatureChecker internally for ABP features, falls back to edition-based features
        context.Services.AddScoped<IFeatureCheckService, FeatureCheckService>();

        // ══════════════════════════════════════════════════════════════════════════════
        // Core Infrastructure Services - Tenant Context & Audit
        // ══════════════════════════════════════════════════════════════════════════════
        // Tenant Context Service - Single source of truth for tenant resolution
        context.Services.AddScoped<ITenantContextService, TenantContextService>();
        
        // Audit Replay Service - For agent code determinism and auditability
        context.Services.AddScoped<IAuditReplayService, AuditReplayService>();

        // ABP Service Adapters - These wrap ABP core services for use by custom business logic
        // Architecture: ABP modules = core foundation, custom services = business layer on top
        context.Services.AddScoped<IAbpIdentityServiceAdapter, AbpIdentityServiceAdapter>();
        context.Services.AddScoped<IAbpTenantServiceAdapter, AbpTenantServiceAdapter>();
        context.Services.AddScoped<IAbpSettingsServiceAdapter, AbpSettingsServiceAdapter>();

        // ══════════════════════════════════════════════════════════════════════════════
        // Business Logic Services - Tenant-aware GRC services
        // All services must inject and validate ITenantContextService
        // ══════════════════════════════════════════════════════════════════════════════
        context.Services.AddScoped<IRiskService, RiskService>();
        context.Services.AddScoped<IControlService, ControlService>();
        context.Services.AddScoped<IAssessmentService, AssessmentService>();
        context.Services.AddScoped<IAssessmentExecutionService, AssessmentExecutionService>();
        context.Services.AddScoped<IAuditService, AuditService>();
        context.Services.AddScoped<IPolicyService, PolicyService>();
        context.Services.AddScoped<IWorkflowService, WorkflowService>();
        context.Services.AddScoped<IFileUploadService, FileUploadService>();
        context.Services.AddScoped<IActionPlanService, ActionPlanService>();
        context.Services.AddScoped<IVendorService, VendorService>();
        context.Services.AddScoped<IRegulatorService, RegulatorService>();
        context.Services.AddScoped<IComplianceCalendarService, ComplianceCalendarService>();
        context.Services.AddScoped<IFrameworkManagementService, FrameworkManagementService>();

        // ══════════════════════════════════════════════════════════════════════════════
        // Email & Communication Services
        // ══════════════════════════════════════════════════════════════════════════════
        context.Services.AddScoped<IAppEmailSender, SmtpEmailSender>();
        context.Services.AddScoped<IEmailService, EmailServiceAdapter>();
        context.Services.AddScoped<IGrcEmailService, GrcEmailService>();
        context.Services.AddScoped<IEmailMfaService, EmailMfaService>();

        // ══════════════════════════════════════════════════════════════════════════════
        // Authentication & Security Services
        // ══════════════════════════════════════════════════════════════════════════════
        context.Services.AddScoped<IAuthenticationService, AuthenticationService>();
        context.Services.AddScoped<IAuthenticationAuditService, AuthenticationAuditService>();
        context.Services.AddScoped<IAccessManagementAuditService, AccessManagementAuditService>();
        context.Services.AddScoped<ISecurePasswordResetService, SecurePasswordResetService>();
        context.Services.AddScoped<IStepUpAuthService, StepUpAuthService>();
        context.Services.AddScoped<IPasswordHistoryService, PasswordHistoryService>();
        context.Services.AddScoped<ISessionManagementService, SessionManagementService>();
        context.Services.AddScoped<IHtmlSanitizerService, HtmlSanitizerService>();

        // ══════════════════════════════════════════════════════════════════════════════
        // Onboarding & Tenant Management Services
        // ══════════════════════════════════════════════════════════════════════════════
        context.Services.AddScoped<ITenantService, TenantService>();
        context.Services.AddScoped<IOnboardingService, OnboardingService>();
        context.Services.AddScoped<IOnboardingWizardService, OnboardingWizardService>();
        context.Services.AddScoped<IOnboardingReferenceDataService, OnboardingReferenceDataService>();
        context.Services.AddScoped<IOnboardingAIRecommendationService, OnboardingAIRecommendationService>();
        context.Services.AddScoped<IOnboardingCoverageService, OnboardingCoverageService>();
        context.Services.AddScoped<IOnboardingProvisioningService, OnboardingProvisioningService>();
        context.Services.AddScoped<IFieldRegistryService, FieldRegistryService>();

        // ══════════════════════════════════════════════════════════════════════════════
        // Support & Platform Admin Services
        // ══════════════════════════════════════════════════════════════════════════════
        context.Services.AddScoped<ISupportTicketService, SupportTicketService>();
        context.Services.AddScoped<IPlatformAdminService, PlatformAdminService>();
        context.Services.AddScoped<IEndpointDiscoveryService, EndpointDiscoveryService>();
        context.Services.AddScoped<IEndpointMonitoringService, EndpointMonitoringService>();

        // ══════════════════════════════════════════════════════════════════════════════
        // Workflow Services - Core Workflow Infrastructure + Implemented Workflows
        // ══════════════════════════════════════════════════════════════════════════════
        // Core workflow service (already registered above at line 304)
        // Additional workflow services (implemented)
        context.Services.AddScoped<GrcMvc.Services.Interfaces.IEvidenceWorkflowService, GrcMvc.Services.Implementations.EvidenceWorkflowService>();
        context.Services.AddScoped<GrcMvc.Services.Interfaces.IRiskWorkflowService, GrcMvc.Services.Implementations.RiskWorkflowService>();
        
        // PHASE 2 - 10 Workflow Types (✅ ALL IMPLEMENTED)
        context.Services.AddScoped<GrcMvc.Services.Interfaces.Workflows.IControlImplementationWorkflowService, GrcMvc.Services.Implementations.Workflows.ControlImplementationWorkflowService>();
        context.Services.AddScoped<GrcMvc.Services.Interfaces.Workflows.IRiskAssessmentWorkflowService, GrcMvc.Services.Implementations.Workflows.RiskAssessmentWorkflowService>();
        context.Services.AddScoped<GrcMvc.Services.Interfaces.Workflows.IApprovalWorkflowService, GrcMvc.Services.Implementations.Workflows.ApprovalWorkflowService>();
        context.Services.AddScoped<GrcMvc.Services.Interfaces.Workflows.IEvidenceCollectionWorkflowService, GrcMvc.Services.Implementations.Workflows.EvidenceCollectionWorkflowService>();
        context.Services.AddScoped<GrcMvc.Services.Interfaces.Workflows.IComplianceTestingWorkflowService, GrcMvc.Services.Implementations.Workflows.ComplianceTestingWorkflowService>();
        context.Services.AddScoped<GrcMvc.Services.Interfaces.Workflows.IRemediationWorkflowService, GrcMvc.Services.Implementations.Workflows.RemediationWorkflowService>();
        context.Services.AddScoped<GrcMvc.Services.Interfaces.Workflows.IPolicyReviewWorkflowService, GrcMvc.Services.Implementations.Workflows.PolicyReviewWorkflowService>();
        context.Services.AddScoped<GrcMvc.Services.Interfaces.Workflows.ITrainingAssignmentWorkflowService, GrcMvc.Services.Implementations.Workflows.TrainingAssignmentWorkflowService>();
        context.Services.AddScoped<GrcMvc.Services.Interfaces.Workflows.IAuditWorkflowService, GrcMvc.Services.Implementations.Workflows.AuditWorkflowService>();
        context.Services.AddScoped<GrcMvc.Services.Interfaces.Workflows.IExceptionHandlingWorkflowService, GrcMvc.Services.Implementations.Workflows.ExceptionHandlingWorkflowService>();
        
        // Core Workflow Infrastructure
        context.Services.AddScoped<BpmnParser>();
        context.Services.AddScoped<WorkflowAssigneeResolver>();
        context.Services.AddScoped<IWorkflowAuditService, WorkflowAuditService>();
        context.Services.AddScoped<IWorkflowEngineService, WorkflowEngineService>();
        context.Services.AddScoped<IEscalationService, EscalationService>();
        context.Services.AddScoped<GrcMvc.Services.IUserWorkspaceService, GrcMvc.Services.UserWorkspaceService>();
        context.Services.AddScoped<GrcMvc.Services.IInboxService, GrcMvc.Services.InboxService>();
        context.Services.AddScoped<IWorkflowRoutingService, WorkflowRoutingService>();
        context.Services.AddScoped<IWorkflowIntegrationService, WorkflowIntegrationService>();

        // ══════════════════════════════════════════════════════════════════════════════
        // RBAC Services (Role-Based Access Control)
        // ══════════════════════════════════════════════════════════════════════════════
        context.Services.AddScoped<GrcMvc.Services.Interfaces.RBAC.IPermissionService, GrcMvc.Services.Implementations.RBAC.PermissionService>();
        // RBAC Services (✅ ALL IMPLEMENTED)
        context.Services.AddScoped<GrcMvc.Services.Interfaces.RBAC.IFeatureService, GrcMvc.Services.Implementations.RBAC.FeatureService>();
        context.Services.AddScoped<GrcMvc.Services.Interfaces.RBAC.ITenantRoleConfigurationService, GrcMvc.Services.Implementations.RBAC.TenantRoleConfigurationService>();
        context.Services.AddScoped<GrcMvc.Services.Interfaces.RBAC.IUserRoleAssignmentService, GrcMvc.Services.Implementations.RBAC.UserRoleAssignmentService>();
        context.Services.AddScoped<GrcMvc.Services.Interfaces.RBAC.IAccessControlService, GrcMvc.Services.Implementations.RBAC.AccessControlService>();
        context.Services.AddScoped<GrcMvc.Services.Interfaces.RBAC.IRbacSeederService, GrcMvc.Services.Implementations.RBAC.RbacSeederService>();
        context.Services.AddScoped<GrcMvc.Services.Interfaces.IPermissionService, GrcMvc.Services.Implementations.PermissionService>();
        context.Services.AddScoped<IUserProfileService, UserProfileServiceImpl>();
        context.Services.AddScoped<IAccessReviewService, AccessReviewService>();

        // ══════════════════════════════════════════════════════════════════════════════
        // Integration Services (Email, Payment, SSO, File Storage)
        // ══════════════════════════════════════════════════════════════════════════════
        context.Services.AddHttpClient(); // Default HttpClient
        context.Services.AddHttpClient("Email");
        context.Services.AddHttpClient("Stripe");
        context.Services.AddHttpClient("SSO");
        context.Services.AddScoped<GrcMvc.Services.Integrations.IEmailIntegrationService, GrcMvc.Services.Integrations.EmailIntegrationService>();
        context.Services.AddScoped<GrcMvc.Services.Integrations.IPaymentIntegrationService, GrcMvc.Services.Integrations.StripePaymentService>();
        context.Services.AddScoped<GrcMvc.Services.Integrations.ISSOIntegrationService, GrcMvc.Services.Integrations.SSOIntegrationService>();
        context.Services.AddScoped<GrcMvc.Services.Integrations.IEvidenceAutomationService, GrcMvc.Services.Integrations.EvidenceAutomationService>();

        // ══════════════════════════════════════════════════════════════════════════════
        // File Storage & Document Services
        // ══════════════════════════════════════════════════════════════════════════════
        context.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
        context.Services.AddScoped<IReportGenerator, ReportGeneratorService>();
        context.Services.AddScoped<IReportService, EnhancedReportServiceFixed>();
        context.Services.AddScoped<IDocumentGenerationService, DocumentGenerationService>();
        context.Services.AddScoped<IEvidenceService, EvidenceService>();

        // ══════════════════════════════════════════════════════════════════════════════
        // Usage Tracking & Analytics Services
        // ══════════════════════════════════════════════════════════════════════════════
        context.Services.AddScoped<IUsageTrackingService, UsageTrackingService>();
        context.Services.AddScoped<ICurrentUserService, GrcMvc.Services.Adapters.AbpCurrentUserAdapter>();

        // ══════════════════════════════════════════════════════════════════════════════
        // Workspace & User Services
        // ══════════════════════════════════════════════════════════════════════════════
        context.Services.AddScoped<IWorkspaceContextService, WorkspaceContextService>();
        context.Services.AddScoped<IWorkspaceManagementService, WorkspaceManagementService>();
        context.Services.AddScoped<IWorkspaceService, WorkspaceService>();
        context.Services.AddScoped<IUserDirectoryService, UserDirectoryService>();
        context.Services.AddScoped<Microsoft.AspNetCore.Authentication.IClaimsTransformation, GrcMvc.Services.Implementations.ClaimsTransformationService>();

        // ══════════════════════════════════════════════════════════════════════════════
        // Serial Code & Number Services
        // ══════════════════════════════════════════════════════════════════════════════
        context.Services.AddScoped<ISerialCodeService, SerialCodeService>();
        context.Services.AddScoped<ISerialNumberService, SerialNumberService>();

        // ══════════════════════════════════════════════════════════════════════════════
        // Dashboard & Metrics Services
        // ══════════════════════════════════════════════════════════════════════════════
        context.Services.AddScoped<IOwnerDashboardService, OwnerDashboardService>();
        context.Services.AddScoped<IDashboardMetricsService, DashboardMetricsService>();
        context.Services.AddScoped<IDashboardService, DashboardService>();
        // PostLoginRoutingService - ✅ IMPLEMENTED
        context.Services.AddScoped<GrcMvc.Services.IPostLoginRoutingService, GrcMvc.Services.PostLoginRoutingService>();

        // ══════════════════════════════════════════════════════════════════════════════
        // Plan & Assessment Services
        // ══════════════════════════════════════════════════════════════════════════════
        context.Services.AddScoped<IPlanService, PlanService>();
        context.Services.AddScoped<IAssetService, AssetService>();
        context.Services.AddScoped<ICatalogDataService, CatalogDataService>();

        // ══════════════════════════════════════════════════════════════════════════════
        // Government & Compliance Services
        // ══════════════════════════════════════════════════════════════════════════════
        context.Services.AddScoped<IVision2030AlignmentService, Vision2030AlignmentService>();
        context.Services.AddScoped<INationalComplianceHub, NationalComplianceHubService>();
        context.Services.AddScoped<IRegulatoryCalendarService, RegulatoryCalendarService>();
        context.Services.AddScoped<IArabicComplianceAssistant, ArabicComplianceAssistantService>();
        context.Services.AddScoped<IAttestationService, AttestationService>();
        context.Services.AddScoped<IGovernmentIntegrationService, GovernmentIntegrationService>();
        context.Services.AddScoped<IGrcProcessOrchestrator, GrcProcessOrchestrator>();
        context.Services.AddScoped<IComplianceGapService, ComplianceGapService>();

        // ══════════════════════════════════════════════════════════════════════════════
        // LLM & AI Services
        // ══════════════════════════════════════════════════════════════════════════════
        // ILlmService - ✅ IMPLEMENTED
        context.Services.AddScoped<GrcMvc.Services.ILlmService, GrcMvc.Services.LlmService>();
        context.Services.AddHttpClient<GrcMvc.Services.ILlmService, GrcMvc.Services.LlmService>();
        context.Services.AddScoped<ISmartOnboardingService, SmartOnboardingService>();
        context.Services.AddScoped<IConsentService, ConsentService>();
        context.Services.AddScoped<ISupportAgentService, SupportAgentService>();
        context.Services.AddScoped<IExpertFrameworkMappingService, ExpertFrameworkMappingService>();
        context.Services.AddSingleton<ISectorFrameworkCacheService, SectorFrameworkCacheService>();
        context.Services.AddScoped<ITenantEvidenceProvisioningService, TenantEvidenceProvisioningService>();
        context.Services.AddScoped<ISuiteGenerationService, SuiteGenerationService>();

        // ══════════════════════════════════════════════════════════════════════════════
        // Shahin-AI Orchestration Services (MAP, APPLY, PROVE, WATCH, FIX, VAULT)
        // ══════════════════════════════════════════════════════════════════════════════
        // IShahinAIOrchestrationService - ✅ IMPLEMENTED
        context.Services.AddScoped<GrcMvc.Services.IShahinAIOrchestrationService, GrcMvc.Services.Implementations.ShahinAIOrchestrationService>();
        context.Services.AddScoped<IMAPService, MAPService>();
        context.Services.AddScoped<IAPPLYService, APPLYService>();
        context.Services.AddScoped<IPROVEService, PROVEService>();
        context.Services.AddScoped<IWATCHService, WATCHService>();
        context.Services.AddScoped<IFIXService, FIXService>();
        context.Services.AddScoped<IVAULTService, VAULTService>();

        // ══════════════════════════════════════════════════════════════════════════════
        // Assessment & Role Delegation Services
        // ══════════════════════════════════════════════════════════════════════════════
        context.Services.AddScoped<IAssessmentExecutionService, AssessmentExecutionService>();
        context.Services.AddScoped<IRoleDelegationService, RoleDelegationService>();

        // ══════════════════════════════════════════════════════════════════════════════
        // Resilience & Certification Services
        // ══════════════════════════════════════════════════════════════════════════════
        context.Services.AddScoped<IControlTestService, ControlTestService>();
        context.Services.AddScoped<IIncidentResponseService, IncidentResponseService>();
        context.Services.AddScoped<ICertificationService, CertificationService>();

        // ══════════════════════════════════════════════════════════════════════════════
        // Subscription & Trial Services
        // ══════════════════════════════════════════════════════════════════════════════
        context.Services.AddScoped<ISubscriptionService, SubscriptionService>();
        context.Services.AddScoped<ITrialLifecycleService, TrialLifecycleService>();
        context.Services.AddScoped<TrialNurtureJob>();

        // ══════════════════════════════════════════════════════════════════════════════
        // Tenant Provisioning & Onboarding Services
        // ══════════════════════════════════════════════════════════════════════════════
        context.Services.AddScoped<ITenantProvisioningService, TenantProvisioningService>();
        context.Services.AddScoped<ITenantOnboardingProvisioner, TenantOnboardingProvisioner>();
        context.Services.AddScoped<IUserInvitationService, UserInvitationService>();
        context.Services.AddScoped<IEvidenceLifecycleService, EvidenceLifecycleService>();

        // ══════════════════════════════════════════════════════════════════════════════
        // Owner & Setup Services
        // ══════════════════════════════════════════════════════════════════════════════
        context.Services.AddScoped<IOwnerTenantService, OwnerTenantService>();
        context.Services.AddScoped<IOwnerSetupService, OwnerSetupService>();
        context.Services.AddScoped<ICredentialDeliveryService, CredentialDeliveryService>();
        context.Services.AddScoped<ICredentialExpirationService, CredentialExpirationService>();

        // ══════════════════════════════════════════════════════════════════════════════
        // Framework & Rules Engine Services
        // ══════════════════════════════════════════════════════════════════════════════
        context.Services.AddScoped<IFrameworkService, Phase1FrameworkService>();
        context.Services.AddScoped<IHRISService, HRISService>();
        context.Services.AddScoped<IAuditTrailService, AuditTrailService>();
        context.Services.AddScoped<IRulesEngineService, Phase1RulesEngineService>();

        // ══════════════════════════════════════════════════════════════════════════════
        // Menu & Navigation Services
        // ══════════════════════════════════════════════════════════════════════════════
        context.Services.AddScoped<GrcMvc.Data.Menu.GrcMenuContributor>();
        context.Services.AddScoped<IMenuService, MenuService>();

        // ══════════════════════════════════════════════════════════════════════════════
        // Caching & Infrastructure Services
        // ══════════════════════════════════════════════════════════════════════════════
        context.Services.AddScoped<IGrcCachingService, GrcCachingService>();

        // ══════════════════════════════════════════════════════════════════════════════
        // Policy Enforcement System
        // ══════════════════════════════════════════════════════════════════════════════
        context.Services.AddScoped<GrcMvc.Application.Policy.IPolicyEnforcer, GrcMvc.Application.Policy.PolicyEnforcer>();
        context.Services.AddSingleton<GrcMvc.Application.Policy.IPolicyStore, GrcMvc.Application.Policy.PolicyStore>();
        context.Services.AddScoped<GrcMvc.Application.Policy.IDotPathResolver, GrcMvc.Application.Policy.DotPathResolver>();
        context.Services.AddScoped<GrcMvc.Application.Policy.IMutationApplier, GrcMvc.Application.Policy.MutationApplier>();
        context.Services.AddScoped<GrcMvc.Application.Policy.IPolicyAuditLogger, GrcMvc.Application.Policy.PolicyAuditLogger>();
        context.Services.AddScoped<GrcMvc.Application.Policy.PolicyEnforcementHelper>();
        context.Services.AddScoped<GrcMvc.Application.Policy.PolicyValidationHelper>();

        // ══════════════════════════════════════════════════════════════════════════════
        // Permissions System
        // ══════════════════════════════════════════════════════════════════════════════
        context.Services.AddSingleton<GrcMvc.Application.Permissions.IPermissionDefinitionProvider, GrcMvc.Application.Permissions.GrcPermissionDefinitionProvider>();
        context.Services.AddScoped<GrcMvc.Application.Permissions.PermissionSeederService>();

        // ══════════════════════════════════════════════════════════════════════════════
        // Migration Services (V2 Architecture)
        // ══════════════════════════════════════════════════════════════════════════════
        context.Services.AddSingleton<IMetricsService, MetricsService>();
        context.Services.AddScoped<ISecurePasswordGenerator, SecurePasswordGenerator>();
        context.Services.AddScoped<IEnhancedAuthService, EnhancedAuthService>();
        context.Services.AddScoped<IEnhancedTenantResolver, EnhancedTenantResolver>();
        context.Services.AddScoped<IUserManagementFacade, UserManagementFacade>();

        // ══════════════════════════════════════════════════════════════════════════════
        // Seeder Services
        // ══════════════════════════════════════════════════════════════════════════════
        context.Services.AddScoped<ApplicationInitializer>();
        context.Services.AddScoped<CatalogSeederService>();
        context.Services.AddScoped<GrcMvc.Data.Seeds.OnboardingQuestionSeeder>();
        context.Services.AddScoped<WorkflowDefinitionSeederService>();
        context.Services.AddScoped<FrameworkControlImportService>();
        // IPocSeederService - ✅ IMPLEMENTED
        context.Services.AddScoped<GrcMvc.Data.Seeds.IPocSeederService, GrcMvc.Data.Seeds.PocSeederService>();

        // ══════════════════════════════════════════════════════════════════════════════
        // Analytics Services (Conditional - based on configuration)
        // ══════════════════════════════════════════════════════════════════════════════
        // Note: Analytics services are conditionally registered based on configuration
        // These should remain in Program.cs as they require configuration evaluation
        // However, we can register the interfaces here and let Program.cs handle conditional logic

        // ══════════════════════════════════════════════════════════════════════════════
        // SignalR & Real-time Services
        // ══════════════════════════════════════════════════════════════════════════════
        context.Services.AddScoped<GrcMvc.Hubs.IDashboardHubService, GrcMvc.Hubs.DashboardHubService>();

        // ══════════════════════════════════════════════════════════════════════════════
        // Admin Catalog Management
        // ══════════════════════════════════════════════════════════════════════════════
        context.Services.AddScoped<IAdminCatalogService, AdminCatalogService>();

        // ══════════════════════════════════════════════════════════════════════════════
        // Site Settings & App Info
        // ══════════════════════════════════════════════════════════════════════════════
        context.Services.AddScoped<ISiteSettingsService, SiteSettingsService>();
        // IAppInfoService - ✅ IMPLEMENTED
        context.Services.AddSingleton<GrcMvc.Services.IAppInfoService, GrcMvc.Services.AppInfoService>();

        // ══════════════════════════════════════════════════════════════════════════════
        // Repositories & Unit of Work
        // ══════════════════════════════════════════════════════════════════════════════
        context.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        context.Services.AddScoped<IUnitOfWork, TenantAwareUnitOfWork>();

        // ══════════════════════════════════════════════════════════════════════════════
        // Additional Services from Program.cs
        // ══════════════════════════════════════════════════════════════════════════════
        // Audit Event Service
        context.Services.AddScoped<IAuditEventService, AuditEventService>();

        // Workflow Services (Evidence & Risk)
        context.Services.AddScoped<GrcMvc.Services.Interfaces.IEvidenceWorkflowService, GrcMvc.Services.Implementations.EvidenceWorkflowService>();
        context.Services.AddScoped<GrcMvc.Services.Interfaces.IRiskWorkflowService, GrcMvc.Services.Implementations.RiskWorkflowService>();

        // Resilience & Sustainability Services
        context.Services.AddScoped<IResilienceService, ResilienceService>();
        context.Services.AddScoped<ISustainabilityService, SustainabilityService>();

        // Authorization Service
        context.Services.AddScoped<GrcMvc.Services.Interfaces.IAuthorizationService, GrcMvc.Services.Implementations.AuthorizationService>();

        // Database Context Factory & Resolver
        context.Services.AddScoped<ITenantDatabaseResolver, TenantDatabaseResolver>();
        context.Services.AddScoped<IDbContextFactory<GrcDbContext>, TenantAwareDbContextFactory>();

        // Validators
        context.Services.AddScoped<FluentValidation.IValidator<GrcMvc.Models.DTOs.CreateRiskDto>, GrcMvc.Validators.CreateRiskDtoValidator>();
        context.Services.AddScoped<FluentValidation.IValidator<GrcMvc.Models.DTOs.UpdateRiskDto>, GrcMvc.Validators.UpdateRiskDtoValidator>();

        // Configuration Validators
        context.Services.AddSingleton<Microsoft.Extensions.Options.IValidateOptions<GrcMvc.Configuration.JwtSettings>, GrcMvc.Configuration.JwtSettingsValidator>();
        context.Services.AddSingleton<Microsoft.Extensions.Options.IValidateOptions<GrcMvc.Configuration.ApplicationSettings>, GrcMvc.Configuration.ApplicationSettingsValidator>();

        // Exception Filter
        context.Services.AddScoped<GrcMvc.Filters.ApiExceptionFilterAttribute>();

        // ══════════════════════════════════════════════════════════════════════════════
        // Hosted Services (Background Services)
        // ══════════════════════════════════════════════════════════════════════════════
        context.Services.AddHostedService<GrcMvc.Application.Policy.PolicyStore>(); // For hot reload
        context.Services.AddHostedService<UserSeedingHostedService>();
        context.Services.AddHostedService<GrcMvc.Services.StartupValidators.OnboardingServicesStartupValidator>();
        
        // Conditional hosted services (Kafka)
        var kafkaEnabled = configuration.GetValue<bool>("Kafka:Enabled", false);
        if (kafkaEnabled)
        {
            context.Services.AddHostedService<GrcMvc.Services.Kafka.KafkaConsumerService>();
        }
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();

        // Enable ABP Multi-Tenancy middleware
        app.UseMultiTenancy();

        // Enable ABP Auditing
        app.UseAuditing();

        // Note: Custom TenantResolutionMiddleware is still used for domain-based tenant resolution
        // ABP's multi-tenancy will use the tenant set by our custom middleware
    }

    public override void OnPostApplicationInitialization(ApplicationInitializationContext context)
    {
        // Register background workers after application initialization
        var workerManager = context.ServiceProvider.GetRequiredService<IBackgroundWorkerManager>();

        // Register onboarding abandonment detection worker (runs daily)
        workerManager.AddAsync(
            context.ServiceProvider.GetRequiredService<GrcMvc.BackgroundWorkers.OnboardingAbandonmentWorker>()
        );
    }
}
