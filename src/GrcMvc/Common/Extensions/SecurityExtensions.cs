using GrcMvc.Services.Security;
using GrcMvc.Middleware;

namespace GrcMvc.Common.Extensions
{
    /// <summary>
    /// Extension methods for registering security services and middleware.
    /// </summary>
    public static class SecurityExtensions
    {
        /// <summary>
        /// Adds security audit service that checks configuration at startup.
        /// </summary>
        public static IServiceCollection AddSecurityAudit(this IServiceCollection services)
        {
            services.AddHostedService<SecurityAuditService>();
            return services;
        }

        /// <summary>
        /// Adds all security-related services.
        /// </summary>
        public static IServiceCollection AddGrcSecurity(this IServiceCollection services)
        {
            services.AddSecurityAudit();
            return services;
        }

        /// <summary>
        /// Configures the application to use enhanced security middleware.
        /// </summary>
        public static IApplicationBuilder UseGrcSecurity(this IApplicationBuilder app)
        {
            app.UseEnhancedExceptionHandling();
            return app;
        }
    }
}
