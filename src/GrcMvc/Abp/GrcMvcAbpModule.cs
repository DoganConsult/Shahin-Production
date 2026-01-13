using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Autofac;
using Volo.Abp.AuditLogging;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.PostgreSql;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.TenantManagement;
using Microsoft.Extensions.DependencyInjection;

namespace GrcMvc.Abp;

/// <summary>
/// ABP Framework Module for Shahin GRC Platform
///
/// This module integrates ABP's enterprise features:
/// - Multi-tenancy with tenant isolation
/// - Audit logging for compliance
/// - Feature management per tenant/edition
/// - Identity management foundation
///
/// All packages used are FREE open-source (no license required)
/// </summary>
[DependsOn(
    // ABP Core
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreMvcModule),

    // Database
    typeof(AbpEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCorePostgreSqlModule),

    // Multi-tenancy & Features
    typeof(AbpTenantManagementDomainModule),
    typeof(AbpFeatureManagementDomainModule),

    // Identity & Audit
    typeof(AbpIdentityDomainModule),
    typeof(AbpAuditLoggingDomainModule)
)]
public class GrcMvcAbpModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        // Pre-configuration before services are registered
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        // Configure ABP Entity Framework Core
        Configure<AbpDbContextOptions>(options =>
        {
            options.UseNpgsql();
        });

        // Multi-tenancy is handled by existing TenantResolutionMiddleware
        // Features are defined in GrcFeatureDefinitionProvider

        // Register ABP services with existing DI container
        context.Services.AddSingleton<IFeatureCheckService, FeatureCheckService>();
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        // ABP middleware is integrated with existing pipeline
        // No additional middleware needed - existing GRC middleware handles routing
    }

    public override void OnPostApplicationInitialization(ApplicationInitializationContext context)
    {
        // Seed edition/feature data after application starts
        var serviceProvider = context.ServiceProvider;

        using var scope = serviceProvider.CreateScope();
        var seeder = scope.ServiceProvider.GetService<GrcEditionDataSeeder>();
        seeder?.SeedAsync().GetAwaiter().GetResult();
    }
}
