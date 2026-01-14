using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Autofac;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.PostgreSql;
using Volo.Abp.Modularity;
using Microsoft.Extensions.DependencyInjection;
using GrcMvc.Data;

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
    // ABP Core only - minimal dependencies without database requirements
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreMvcModule),

    // Database - EF Core integration
    typeof(AbpEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCorePostgreSqlModule)

    // NOTE: The following modules are disabled as they require ABP database schema:
    // - AbpAspNetCoreMultiTenancyModule (requires AbpTenants table)
    // - AbpTenantManagementDomainModule (requires tenant tables)
    // - AbpOpenIddictDomainModule (requires OpenIddict tables)
    // - AbpFeatureManagementDomainModule (requires feature tables)
    // - AbpPermissionManagementDomainModule (requires permission tables)
    // - AbpIdentityDomainModule (requires identity tables)
    // - AbpAuditLoggingDomainModule (requires audit tables)
    // Use existing custom implementations instead
)]
public class GrcMvcAbpModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        // OpenIddict configuration disabled - requires ABP database schema
        // Using existing ASP.NET Core Identity authentication instead
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        // Disable ABP background workers (they require database connection at startup)
        // Our app uses Hangfire instead for background jobs
        Configure<AbpBackgroundWorkerOptions>(options =>
        {
            options.IsEnabled = false;
        });

        // Configure ABP Entity Framework Core
        Configure<AbpDbContextOptions>(options =>
        {
            options.UseNpgsql();
        });

        // Register GrcDbContext with ABP (replaces AddDbContext in Program.cs)
        context.Services.AddAbpDbContext<GrcDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
        });

        // Multi-tenancy handled by custom TenantResolutionMiddleware
        // ABP tenant configuration disabled (requires database tables)

        // Register ABP services with existing DI container
        context.Services.AddSingleton<IFeatureCheckService, FeatureCheckService>();
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        // Skip ABP middleware - using existing custom tenant resolution middleware
        // ABP's UseMultiTenancy() requires ABP tenant tables which may not exist
        // The existing TenantResolutionMiddleware handles tenant resolution
    }

    public override void OnPostApplicationInitialization(ApplicationInitializationContext context)
    {
        // ABP edition/feature seeding disabled
        // Using existing custom edition management instead
    }
}
