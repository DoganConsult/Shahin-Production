using GrcMvc.BackgroundJobs;
using GrcMvc.Services.Implementations;
using GrcMvc.Services.Interfaces;
using Hangfire;

namespace GrcMvc.Extensions;

/// <summary>
/// Extension methods for configuring Code Quality Monitoring services
/// </summary>
public static class CodeQualityServiceExtensions
{
    /// <summary>
    /// Add Code Quality Monitoring services to the DI container
    /// </summary>
    public static IServiceCollection AddCodeQualityMonitoring(this IServiceCollection services, IConfiguration configuration)
    {
        // Register core services
        services.AddHttpClient<ICodeQualityService, CodeQualityService>();
        services.AddHttpClient<IAlertService, AlertService>();

        // Register background jobs
        services.AddScoped<CodeQualityMonitorJob>();

        return services;
    }

    /// <summary>
    /// Configure Code Quality background jobs
    /// </summary>
    public static IApplicationBuilder UseCodeQualityMonitoring(this IApplicationBuilder app, IConfiguration configuration)
    {
        var enabled = configuration.GetValue("CodeQuality:Enabled", true);
        if (!enabled)
            return app;

        // Configure recurring jobs
        ConfigureRecurringJobs(configuration);

        return app;
    }

    private static void ConfigureRecurringJobs(IConfiguration configuration)
    {
        // Daily code quality scan
        if (configuration.GetValue("BackgroundJobs:CodeQualityScan:Enabled", true))
        {
            var cronSchedule = configuration["BackgroundJobs:CodeQualityScan:CronSchedule"] ?? "0 2 * * *";
            RecurringJob.AddOrUpdate<CodeQualityMonitorJob>(
                "code-quality-scan",
                job => job.ExecuteAsync(),
                cronSchedule,
                new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });
        }

        // Weekly security audit
        if (configuration.GetValue("BackgroundJobs:SecurityAudit:Enabled", true))
        {
            var cronSchedule = configuration["BackgroundJobs:SecurityAudit:CronSchedule"] ?? "0 3 * * 0";
            RecurringJob.AddOrUpdate<CodeQualityMonitorJob>(
                "security-audit",
                job => job.ExecuteFullSecurityAuditAsync(),
                cronSchedule,
                new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });
        }

        // Daily report
        if (configuration.GetValue("BackgroundJobs:DailyReport:Enabled", true))
        {
            var cronSchedule = configuration["BackgroundJobs:DailyReport:CronSchedule"] ?? "0 8 * * 1-5";
            RecurringJob.AddOrUpdate<CodeQualityMonitorJob>(
                "daily-quality-report",
                job => job.GenerateDailyReportAsync(),
                cronSchedule,
                new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });
        }
    }
}
