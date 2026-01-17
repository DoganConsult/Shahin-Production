using GrcMvc.Abp;
using GrcMvc.Configuration;
using GrcMvc.Data;
using GrcMvc.Data.Seeds;
using GrcMvc.Extensions;
using GrcMvc.Services;
using GrcMvc.Services.Implementations;
using GrcMvc.Settings;
using GrcMvc.Services.EmailOperations;
using GrcMvc.Services.Analytics;
using GrcMvc.Data.Repositories;
using GrcMvc.Models.Entities;
using Volo.Abp.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GrcMvc.Extensions;

/// <summary>
/// Extension methods for service registration
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register all application services
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Core services
        services.AddScoped<Services.IAppInfoService, Services.AppInfoService>();
        
        // Repository pattern
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, TenantAwareUnitOfWork>();
        
        // Onboarding services
        services.AddScoped<Services.Interfaces.IOnboardingCoverageService, Services.Implementations.OnboardingCoverageService>();
        services.AddScoped<Services.Interfaces.IFieldRegistryService, Services.Implementations.FieldRegistryService>();
        services.AddScoped<Services.Interfaces.IOnboardingWizardService, Services.Implementations.OnboardingWizardService>();
        services.AddScoped<Services.Interfaces.IOnboardingControlPlaneService, Services.Implementations.OnboardingControlPlaneService>();
        
        // Policy Engine services
        services.AddSingleton<Application.Policy.IPolicyStore, Application.Policy.PolicyStore>();
        services.AddScoped<Application.Policy.IPolicyEnforcer, Application.Policy.PolicyEnforcer>();
        services.AddScoped<Application.Policy.IPolicyAuditLogger, Application.Policy.PolicyAuditLogger>();
        services.AddScoped<Application.Policy.IDotPathResolver, Application.Policy.DotPathResolver>();
        services.AddScoped<Application.Policy.IMutationApplier, Application.Policy.MutationApplier>();
        
        // Database resolvers - commented out due to missing interfaces
        // services.AddScoped<ITenantDatabaseResolver, Services.Implementations.TenantDatabaseResolver>();
        // services.AddScoped<IDbContextFactory<GrcDbContext>, Data.TenantAwareDbContextFactory>();
        
        // GRC Domain Services - commented out due to missing interfaces
        // AddGrcDomainServices(services);
        
        // Workflow Services - commented out due to missing interfaces
        // AddWorkflowServices(services);
        
        // RBAC Services - commented out due to missing interfaces
        // AddRbacServices(services);
        
        // Integration Services - commented out due to missing interfaces
        // AddIntegrationServices(services, configuration);
        
        // Background Jobs
        AddBackgroundJobs(services);
        
        // AI Agent Services - commented out due to missing interfaces
        // AddAiAgentServices(services, configuration);
        
        // Email & Notification Services - commented out due to missing interfaces
        // AddNotificationServices(services);
        
        // Analytics Services - commented out due to missing interfaces
        // AddAnalyticsServices(services, configuration);
        
        return services;
    }
    
    /// <summary>
    /// Configure database contexts
    /// </summary>
    public static IServiceCollection AddDatabaseContexts(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found");
        
        var authConnectionString = configuration.GetConnectionString("GrcAuthDb") ?? connectionString;
        
        // ABP handles GrcDbContext registration via AddAbpDbContext in GrcMvcAbpModule
        // Register Auth DbContext for Identity
        services.AddDbContext<GrcAuthDbContext>(options =>
        {
            options.UseNpgsql(authConnectionString, npgsqlOptions =>
            {
                // Command timeout for migrations (large schema needs more time)
                npgsqlOptions.CommandTimeout(300); // 5 minutes for migration operations
                
                // Retry logic for transient failures
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
            });
        }, ServiceLifetime.Scoped);
        
        return services;
    }
    
    /// <summary>
    /// Configure ASP.NET Core Identity with enhanced security
    /// </summary>
    public static IServiceCollection AddIdentityConfiguration(this IServiceCollection services)
    {
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            // Password settings - Strengthened
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 12;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            
            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 3;
            options.Lockout.AllowedForNewUsers = true;
            
            // User settings
            options.User.RequireUniqueEmail = true;
            
            // Sign-in settings (production only)
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            var isProduction = environment.Equals("Production", StringComparison.OrdinalIgnoreCase);
            options.SignIn.RequireConfirmedEmail = isProduction;
            options.SignIn.RequireConfirmedAccount = isProduction;
        })
        .AddEntityFrameworkStores<GrcAuthDbContext>()
        .AddDefaultTokenProviders();
        
        return services;
    }
    
    #region Private Helper Methods
    
    private static void AddBackgroundJobs(IServiceCollection services)
    {
        services.AddScoped<BackgroundJobs.EscalationJob>();
        services.AddScoped<BackgroundJobs.NotificationDeliveryJob>();
        services.AddScoped<BackgroundJobs.SlaMonitorJob>();
        services.AddScoped<BackgroundJobs.WebhookRetryJob>();
        services.AddScoped<BackgroundJobs.DatabaseBackupJob>();
        services.AddScoped<BackgroundJobs.TrialNurtureJob>();
        services.AddScoped<BackgroundJobs.CodeQualityMonitorJob>();
    }
    
    #endregion
    
    #region ABP Settings Configuration
    
    public static void AddAbpSettings(this IServiceCollection services)
    {
        services.AddOptions<AbpSettingOptions>();
        // ISettingManager is registered by ABP framework automatically when using AbpSettingManagementModule
        // Removing manual registration to avoid conflicts
        services.AddScoped<ISettingDefinitionProvider, GrcMvcSettingDefinitionProvider>();
    }
    
    #endregion
}
