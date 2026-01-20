using Volo.Abp.FeatureManagement;
using Microsoft.Extensions.DependencyInjection;

namespace GrcMvc.Configuration
{
    public static class FeatureManagementConfiguration
    {
        public static void ConfigureFeatureManagement(this IServiceCollection services)
        {
            services.Configure<FeatureManagementOptions>(options =>
            {
                // Disable saving static features to database to avoid CurrentTenant null issue
                // This prevents the NullReferenceException during startup
                // Re-enable after fixing tenant context initialization
                options.SaveStaticFeaturesToDatabase = false;
                
                // Additional options for production
                // Feature providers are already configured by ABP Framework
                // No need to manually add providers
            });
        }
    }
}
