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
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
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
        services.AddScoped<Services.Interfaces.IEnvironmentVariableService, Services.Implementations.EnvironmentVariableService>();
        services.AddScoped<Services.Interfaces.ICacheClearService, Services.Implementations.CacheClearService>();
        services.AddScoped<Services.Interfaces.ICurrentUserService, Services.Implementations.CurrentUserService>();

        // Owner setup service (required for initial platform setup)
        services.AddScoped<Services.Interfaces.IOwnerSetupService, Services.Implementations.OwnerSetupService>();

        // OpenIddict data seeder (for OAuth2 application registration)
        services.AddScoped<Data.Seeds.OpenIddictDataSeeder>();

        // Repository pattern
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, TenantAwareUnitOfWork>();
        
        // Onboarding services
        services.AddScoped<Services.Interfaces.IOnboardingCoverageService, Services.Implementations.OnboardingCoverageService>();
        services.AddScoped<Services.Interfaces.IFieldRegistryService, Services.Implementations.FieldRegistryService>();
        services.AddScoped<Services.Interfaces.IOnboardingWizardService, Services.Implementations.OnboardingWizardService>();
        services.AddScoped<Services.Interfaces.IOnboardingControlPlaneService, Services.Implementations.OnboardingControlPlaneService>();

        // GRC Access Management Controls (AM-01 through AM-12)
        services.AddScoped<Services.Interfaces.IAccessReviewService, Services.Implementations.AccessReviewService>();
        
        // Policy Engine services
        services.AddSingleton<Application.Policy.IPolicyStore, Application.Policy.PolicyStore>();
        services.AddScoped<Application.Policy.IPolicyEnforcer, Application.Policy.PolicyEnforcer>();
        services.AddScoped<Application.Policy.IPolicyAuditLogger, Application.Policy.PolicyAuditLogger>();
        services.AddScoped<Application.Policy.IDotPathResolver, Application.Policy.DotPathResolver>();
        services.AddScoped<Application.Policy.IMutationApplier, Application.Policy.MutationApplier>();
        services.AddScoped<Application.Policy.PolicyEnforcementHelper>();
        
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
        // Debug logging for connection string resolution
        Console.WriteLine("[DB] ========================================");
        Console.WriteLine("[DB] Configuring Database Contexts");
        Console.WriteLine("[DB] ========================================");
        
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Console.WriteLine("[DB] ❌ Connection string 'DefaultConnection' not found");
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found");
        }
        
        Console.WriteLine("[DB] ✅ Main Database Connection String: {0}", MaskConnectionString(connectionString));
        
        var authConnectionString = configuration.GetConnectionString("GrcAuthDb") ?? connectionString;
        Console.WriteLine("[DB] ✅ Auth Database Connection String: {0}", MaskConnectionString(authConnectionString));
        
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
        
        Console.WriteLine("[DB] ✅ Database contexts configured");
        Console.WriteLine("[DB] 📍 All DbContext instances will use connection strings from IConfiguration");
        Console.WriteLine("[DB] 🔄 Connection strings are dynamically read (no caching at this layer)");
        Console.WriteLine("[DB] ========================================");
        
        return services;
    }

    private static string MaskConnectionString(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return "[empty]";
        
        try
        {
            var parts = connectionString.Split(';');
            var masked = new List<string>();
            foreach (var part in parts)
            {
                if (part.StartsWith("Password=", StringComparison.OrdinalIgnoreCase) ||
                    part.StartsWith("Pwd=", StringComparison.OrdinalIgnoreCase))
                {
                    masked.Add(part.Split('=')[0] + "=***");
                }
                else
                {
                    masked.Add(part);
                }
            }
            return string.Join(";", masked);
        }
        catch
        {
            return "[invalid format]";
        }
    }
    
    /// <summary>
    /// Identity Configuration - Delegated to ABP Framework
    /// ABP Identity modules handle all identity management automatically
    /// UserManager and SignInManager will be provided by ABP if needed
    /// </summary>
    public static IServiceCollection AddIdentityConfiguration(this IServiceCollection services)
    {
        // ABP Identity is fully configured through AbpIdentityApplicationModule
        // All identity services are automatically registered by ABP:
        // - IIdentityUserAppService (modern service)
        // - UserManager<ApplicationUser> (legacy compatibility)
        // - SignInManager<ApplicationUser> (legacy compatibility)
        // - Password policies, lockout, security features
        
        // No additional configuration needed - ABP handles everything
        
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
        services.AddScoped<BackgroundJobs.AccessReviewReminderJob>(); // AM-11 Access Review Reminders
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
