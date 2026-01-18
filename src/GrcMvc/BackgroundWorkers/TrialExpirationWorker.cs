using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;
using GrcMvc.Data;
using Microsoft.EntityFrameworkCore;

namespace GrcMvc.BackgroundWorkers;

/// <summary>
/// ABP Background Worker that checks for expired trials and updates their status.
/// Runs every hour to identify trials that have expired and marks them appropriately.
/// 
/// This complements Hangfire jobs - ABP workers are good for lightweight periodic tasks,
/// while Hangfire handles heavy jobs with retry logic and persistence.
/// </summary>
public class TrialExpirationWorker : AsyncPeriodicBackgroundWorkerBase
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public TrialExpirationWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory serviceScopeFactory)
        : base(timer, serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
        
        // Run every hour
        Timer.Period = 60 * 60 * 1000; // 1 hour in milliseconds
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        Logger.LogInformation("TrialExpirationWorker: Starting trial expiration check...");

        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GrcDbContext>();

            var now = DateTime.UtcNow;

            // Find expired trials that haven't been marked yet
            var expiredTrials = await dbContext.Tenants
                .Where(t => t.IsTrial 
                         && t.TrialEndsAt != null 
                         && t.TrialEndsAt < now
                         && t.BillingStatus == "Trialing")
                .ToListAsync();

            if (expiredTrials.Any())
            {
                Logger.LogInformation("TrialExpirationWorker: Found {Count} expired trials", expiredTrials.Count);

                foreach (var tenant in expiredTrials)
                {
                    tenant.BillingStatus = "Expired";
                    tenant.Status = "TrialExpired";
                    Logger.LogInformation("TrialExpirationWorker: Marked tenant {TenantId} ({TenantName}) as expired",
                        tenant.Id, tenant.OrganizationName);
                }

                await dbContext.SaveChangesAsync();
                Logger.LogInformation("TrialExpirationWorker: Updated {Count} expired trials", expiredTrials.Count);
            }
            else
            {
                Logger.LogDebug("TrialExpirationWorker: No expired trials found");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "TrialExpirationWorker: Error during trial expiration check");
        }
    }
}
