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
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.PermissionManagement;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
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
    typeof(AbpIdentityEntityFrameworkCoreModule),

    // ABP Permission Management - Authorization
    typeof(AbpPermissionManagementDomainModule),
    typeof(AbpPermissionManagementEntityFrameworkCoreModule),

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

        // Disable ABP background workers (we use Hangfire instead)
        Configure<AbpBackgroundWorkerOptions>(options =>
        {
            options.IsEnabled = false;
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

        // Register GrcDbContext with ABP
        context.Services.AddAbpDbContext<GrcDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
        });

        // Multi-tenancy is handled by custom TenantResolutionMiddleware
        // which resolves tenants from: subdomain, header, or JWT claims
        Configure<AbpMultiTenancyOptions>(options =>
        {
            // Disable ABP's multi-tenancy resolver - using custom implementation
            options.IsEnabled = false;
        });

        // Auditing is handled by custom AuditEventService
        // which provides compliance-grade audit logging with:
        // - Full event payloads (JSON)
        // - Tenant isolation
        // - Explainability integration (Layer 17)
        Configure<AbpAuditingOptions>(options =>
        {
            options.IsEnabled = false; // Using custom AuditEventService
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

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        // ABP middleware is not used
        // Custom middleware is registered in Program.cs:
        // - TenantResolutionMiddleware (Layer 1)
        // - Custom authentication middleware (Layer 2)
    }

    public override void OnPostApplicationInitialization(ApplicationInitializationContext context)
    {
        // Custom initialization is handled by ApplicationInitializer
        // which seeds:
        // - Tenants (Layer 1)
        // - Users (Layer 2)
        // - Roles (Layer 4)
        // - Permissions (Layer 5)
        // - Lookups (Layer 10)
    }
}
