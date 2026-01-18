using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Auditing;
using Volo.Abp.Autofac;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.PostgreSql;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Settings;
// ABP Identity
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
// ABP Permission Management
using Volo.Abp.PermissionManagement;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
// ABP Audit Logging
using Volo.Abp.AuditLogging;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
// ABP Feature Management
using Volo.Abp.FeatureManagement;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
// ABP Tenant Management
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;
// ABP Setting Management
using Volo.Abp.SettingManagement;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
// ABP OpenIddict
using Volo.Abp.OpenIddict;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using OpenIddict.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using GrcMvc.Data;
using GrcMvc.Settings;

namespace GrcMvc.Abp;

/// <summary>
/// ABP Framework Module for Shahin GRC Platform
///
/// This module integrates ABP's core features with custom implementations:
/// - Multi-tenancy via custom TenantContextService (subdomain-based)
/// - Identity via ASP.NET Core Identity (custom implementation)
/// - Audit logging via custom AuditEventService
/// - Feature management via custom FeatureCheckService
///
/// 43-Layer Architecture Note:
/// Custom implementations provide Layers 1-12 (Platform Layer):
/// - Tenants (Layer 1) - Custom Tenant entity + TenantContextService
/// - Users (Layer 2) - ASP.NET Core Identity
/// - Editions (Layer 3) - Custom Edition entity
/// - Roles (Layer 4) - Custom RoleProfile entity
/// - Permissions (Layer 5) - Custom PermissionCatalog entity
/// - Features (Layer 6) - Custom FeatureCheckService
/// - Settings (Layer 7) - Custom TenantSettings entity
/// - Audit Logs (Layer 8) - Custom AuditEventService
/// - Background Jobs (Layer 9) - Hangfire
/// - Data Dictionary (Layer 10) - Custom Lookup tables
/// - Blob Storage (Layer 11) - Azure Blob Storage
/// - Notifications (Layer 12) - Custom notification system
///
/// ABP provides infrastructure support (DI, EF Core, validation)
/// without requiring ABP's domain database tables.
/// </summary>
[DependsOn(
    // ABP Core - DI container and MVC integration
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreMvcModule),

    // Database - EF Core integration
    typeof(AbpEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCorePostgreSqlModule),

    // ABP Identity - User management and authentication
    typeof(AbpIdentityDomainModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpIdentityEntityFrameworkCoreModule),

    // ABP Permission Management - Authorization
    typeof(AbpPermissionManagementDomainModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpPermissionManagementEntityFrameworkCoreModule),

    // ABP Audit Logging - Compliance-grade audit trails
    typeof(AbpAuditLoggingDomainModule),
    typeof(AbpAuditLoggingEntityFrameworkCoreModule),

    // ABP Feature Management - Feature flags per tenant
    typeof(AbpFeatureManagementDomainModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpFeatureManagementEntityFrameworkCoreModule),

    // ABP Tenant Management - Multi-tenant support
    typeof(AbpTenantManagementDomainModule),
    typeof(AbpTenantManagementApplicationModule),
    typeof(AbpTenantManagementEntityFrameworkCoreModule),

    // ABP Setting Management - Tenant/User settings
    typeof(AbpSettingManagementDomainModule),
    typeof(AbpSettingManagementApplicationModule),
    typeof(AbpSettingManagementEntityFrameworkCoreModule),

    // ABP OpenIddict - OAuth2/OpenID Connect authentication
    typeof(AbpOpenIddictDomainModule),
    typeof(AbpOpenIddictEntityFrameworkCoreModule)
)]
public class GrcMvcAbpModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        // Configure OpenIddict for ABP
        PreConfigure<OpenIddictBuilder>(builder =>
        {
            builder.AddValidation(options =>
            {
                options.AddAudiences("GrcMvc");
                options.UseLocalServer();
                options.UseAspNetCore();
            });
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        // Disable ABP background workers (OpenIddict has null logger bug in v8.2.2)
        // Hangfire handles all background processing instead
        Configure<AbpBackgroundWorkerOptions>(options =>
        {
            options.IsEnabled = false; // DISABLED - OpenIddict null logger bug
        });

        // Configure ABP Entity Framework Core
        Configure<AbpDbContextOptions>(options =>
        {
            options.UseNpgsql(npgsqlOptions =>
            {
                // Command timeout for migrations (large schema needs more time)
                npgsqlOptions.CommandTimeout(300); // 5 minutes for migration operations
                
                // Retry logic for transient failures
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
            });
        });

        // Register GrcDbContext with ABP (Main Application Database)
        context.Services.AddAbpDbContext<GrcDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
        });

        // Register GrcAuthDbContext with ABP (Identity/Authentication Database)
        context.Services.AddAbpDbContext<GrcAuthDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
            // This enables IRepository<ApplicationUser, Guid> and other identity repositories
        });

        // Multi-tenancy enabled via ABP's ICurrentTenant
        // Custom TenantResolutionMiddleware resolves tenant and calls ICurrentTenant.Change()
        Configure<AbpMultiTenancyOptions>(options =>
        {
            options.IsEnabled = true; // ABP multi-tenancy enabled for automatic tenant filtering
        });

        // ABP Auditing enabled for automatic audit logging
        // Custom AuditEventService still used for compliance-specific logging
        // ABP handles: standard request/response logging, entity changes
        // Custom handles: Explainability integration (Layer 17), compliance-grade payloads
        Configure<AbpAuditingOptions>(options =>
        {
            options.IsEnabled = true;
            options.ApplicationName = "ShahinGRC";
            options.IsEnabledForAnonymousUsers = false;
            options.IsEnabledForGetRequests = false; // Only log writes for performance
        });

        // Register custom feature check service
        context.Services.AddSingleton<IFeatureCheckService, FeatureCheckService>();

        // Configure ABP Settings
        Configure<AbpSettingOptions>(options =>
        {
            // Order matters: Database providers first, then environment variables, then default
            options.ValueProviders.Add<ConnectionStringSettingValueProvider>();
            options.ValueProviders.Add<EnvironmentVariableSettingValueProvider>();
        });
    }

    public override async void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        // #region agent log
        try { System.IO.File.AppendAllText(@"c:\Shahin-ai\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new{location="GrcMvcAbpModule.cs:OnApplicationInitialization",message="ABP module initialization started",data=new{},timestamp=DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),sessionId="debug-session",hypothesisId="D"})+"\n"); } catch {}
        // #endregion

        // ABP middleware is not used
        // Custom middleware is registered in Program.cs:
        // - TenantResolutionMiddleware (Layer 1)
        // - Custom authentication middleware (Layer 2)

        // ABP background workers disabled - using Hangfire instead
        // TODO: Re-enable when OpenIddict background worker null logger issue is fixed
        // try
        // {
        //     await context.AddBackgroundWorkerAsync<BackgroundWorkers.TrialExpirationWorker>();
        // }
        // catch (Exception ex)
        // {
        //     // Log error
        // }
    }

    public override void OnPostApplicationInitialization(ApplicationInitializationContext context)
    {
        // #region agent log
        try
        {
            System.IO.File.AppendAllText(@"c:\Shahin-ai\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new{location="GrcMvcAbpModule.cs:OnPostApplicationInitialization",message="ABP post-initialization completed",data=new{modulesLoaded=true},timestamp=DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),sessionId="debug-session",hypothesisId="A"})+"\n");
        }
        catch {}
        // #endregion
        
        // Custom initialization is handled by ApplicationInitializer
        // which seeds:
        // - Tenants (Layer 1)
        // - Users (Layer 2)
        // - Roles (Layer 4)
        // - Permissions (Layer 5)
        // - Lookups (Layer 10)
    }
}
