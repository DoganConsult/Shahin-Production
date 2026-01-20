using GrcMvc.BackgroundJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;

namespace GrcMvc.BackgroundWorkers
{
    /// <summary>
    /// ABP Background Worker for onboarding abandonment detection and recovery emails.
    /// Runs daily to detect incomplete onboarding wizards and send recovery emails.
    /// Uses ABP's AsyncPeriodicBackgroundWorkerBase for proper tenant context handling.
    /// </summary>
    public class OnboardingAbandonmentWorker : AsyncPeriodicBackgroundWorkerBase
    {
        public OnboardingAbandonmentWorker(
            AbpAsyncTimer timer,
            IServiceScopeFactory serviceScopeFactory)
            : base(timer, serviceScopeFactory)
        {
            // Run daily at 9 AM UTC (3600000 ms = 1 hour, but we'll run once per day)
            // For daily execution, we set period to 24 hours
            Timer.Period = 24 * 60 * 60 * 1000; // 24 hours in milliseconds
        }

        protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
        {
            var logger = workerContext.ServiceProvider.GetRequiredService<ILogger<OnboardingAbandonmentWorker>>();
            var abandonmentJob = workerContext.ServiceProvider.GetRequiredService<OnboardingAbandonmentJob>();

            logger.LogInformation("OnboardingAbandonmentWorker: Starting daily abandonment detection");

            try
            {
                // Execute the abandonment detection job
                // This will run in a tenant-agnostic context (processes all tenants)
                await abandonmentJob.ExecuteAsync();

                logger.LogInformation("OnboardingAbandonmentWorker: Completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "OnboardingAbandonmentWorker: Error during execution");
                // Don't rethrow - ABP will retry on next period
            }
        }
    }
}
