using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.MultiTenancy;
using Volo.Abp.Autofac;
using Volo.Abp.AuditLogging;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.PostgreSql;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.OpenIddict;
using Volo.Abp.PermissionManagement;
using Volo.Abp.TenantManagement;
using Microsoft.Extensions.DependencyInjection;

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
    // ABP Core
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreMvcModule),

    // Database
    typeof(AbpEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCorePostgreSqlModule),

    // Multi-tenancy
    typeof(AbpAspNetCoreMultiTenancyModule),
    typeof(AbpTenantManagementDomainModule),

    // OpenIddict SSO
    typeof(AbpOpenIddictDomainModule),
    typeof(AbpOpenIddictAspNetCoreModule),

    // Features & Permissions
    typeof(AbpFeatureManagementDomainModule),
    typeof(AbpPermissionManagementDomainModule),

    // Identity & Audit
    typeof(AbpIdentityDomainModule),
    typeof(AbpAuditLoggingDomainModule)
)]
public class GrcMvcAbpModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        // Configure OpenIddict
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

        // Configure ABP Entity Framework Core
        Configure<AbpDbContextOptions>(options =>
        {
            options.UseNpgsql();
        });

        // Configure Multi-Tenancy
        Configure<AbpTenantResolveOptions>(options =>
        {
            // Tenant resolution order (first match wins):
            // 1. Subdomain (e.g., acme.shahin-ai.com)
            // 2. Header (X-Tenant-Id)
            // 3. Query string (?__tenant=acme)
            // 4. Cookie
            options.TenantResolvers.Insert(0, new DomainTenantResolveContributor("{0}.shahin-ai.com"));
        });

        // Configure OpenIddict Server
        Configure<AbpOpenIddictAspNetCoreOptions>(options =>
        {
            options.AddDevelopmentEncryptionAndSigningCertificate = true;
        });

        // Register ABP services with existing DI container
        context.Services.AddSingleton<IFeatureCheckService, FeatureCheckService>();
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        // ABP Multi-tenancy middleware (integrated with existing pipeline)
        app.UseMultiTenancy();

        // ABP middleware is integrated with existing pipeline
        // Existing TenantResolutionMiddleware continues to work alongside ABP's
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
