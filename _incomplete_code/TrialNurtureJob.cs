using GrcMvc.Services.Interfaces;
using Hangfire;

namespace GrcMvc.BackgroundJobs
{
    /// <summary>
    /// Background job for processing trial nurture emails
    /// Runs periodically to send scheduled emails based on trial day
    /// </summary>
    public class TrialNurtureJob
    {
        private readonly ITrialLifecycleService _trialService;
        private readonly ILogger<TrialNurtureJob> _logger;

        public TrialNurtureJob(
            ITrialLifecycleService trialService,
            ILogger<TrialNurtureJob> logger)
        {
            _trialService = trialService;
            _logger = logger;
        }

        /// <summary>
        /// Process all pending nurture emails
        /// Scheduled to run every hour
        /// </summary>
        [AutomaticRetry(Attempts = 3)]
        public async Task ProcessNurtureEmailsAsync()
        {
            _logger.LogInformation("Starting trial nurture email processing");

            try
            {
                var emailsSent = await _trialService.ProcessNurtureEmailsAsync();
                _logger.LogInformation("Trial nurture job completed: {Count} emails sent", emailsSent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing trial nurture emails");
                throw; // Rethrow to trigger Hangfire retry
            }
        }

        /// <summary>
        /// Send specific nurture email to a tenant
        /// Can be enqueued for immediate or delayed execution
        /// </summary>
        [AutomaticRetry(Attempts = 3)]
        public async Task SendNurtureEmailAsync(Guid tenantId, TrialEmailType emailType)
        {
            _logger.LogInformation("Sending {EmailType} email to tenant {TenantId}", emailType, tenantId);

            try
            {
                var success = await _trialService.SendNurtureEmailAsync(tenantId, emailType);

                if (success)
                    _logger.LogInformation("Successfully sent {EmailType} to tenant {TenantId}", emailType, tenantId);
                else
                    _logger.LogWarning("Failed to send {EmailType} to tenant {TenantId}", emailType, tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending {EmailType} to tenant {TenantId}", emailType, tenantId);
                throw;
            }
        }

        /// <summary>
        /// Check trials expiring soon and send escalation emails
        /// Runs daily
        /// </summary>
        [AutomaticRetry(Attempts = 2)]
        public async Task CheckExpiringTrialsAsync()
        {
            _logger.LogInformation("Checking for expiring trials");

            try
            {
                var pendingEmails = await _trialService.GetPendingNurtureEmailsAsync();
                var escalations = pendingEmails.Where(e =>
                    e.EmailType == TrialEmailType.Escalation ||
                    e.EmailType == TrialEmailType.Expired).ToList();

                foreach (var email in escalations)
                {
                    await _trialService.SendNurtureEmailAsync(email.TenantId, email.EmailType);
                }

                _logger.LogInformation("Processed {Count} expiring trial notifications", escalations.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking expiring trials");
                throw;
            }
        }

        /// <summary>
        /// Send winback emails to expired trials
        /// Runs weekly
        /// </summary>
        [AutomaticRetry(Attempts = 2)]
        public async Task SendWinbackEmailsAsync()
        {
            _logger.LogInformation("Processing winback emails for expired trials");

            try
            {
                var pendingEmails = await _trialService.GetPendingNurtureEmailsAsync();
                var winbacks = pendingEmails.Where(e => e.EmailType == TrialEmailType.Winback).ToList();

                foreach (var email in winbacks)
                {
                    await _trialService.SendNurtureEmailAsync(email.TenantId, email.EmailType);
                }

                _logger.LogInformation("Sent {Count} winback emails", winbacks.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending winback emails");
                throw;
            }
        }
    }

    /// <summary>
    /// Extension methods for registering trial jobs
    /// </summary>
    public static class TrialJobExtensions
    {
        /// <summary>
        /// Register recurring trial jobs with Hangfire
        /// </summary>
        public static void RegisterTrialJobs(this IServiceProvider services)
        {
            // Process nurture emails every hour
            RecurringJob.AddOrUpdate<TrialNurtureJob>(
                "trial-nurture-hourly",
                job => job.ProcessNurtureEmailsAsync(),
                Cron.Hourly);

            // Check expiring trials daily at 9 AM
            RecurringJob.AddOrUpdate<TrialNurtureJob>(
                "trial-expiring-daily",
                job => job.CheckExpiringTrialsAsync(),
                "0 9 * * *"); // 9 AM daily

            // Send winback emails weekly on Monday at 10 AM
            RecurringJob.AddOrUpdate<TrialNurtureJob>(
                "trial-winback-weekly",
                job => job.SendWinbackEmailsAsync(),
                "0 10 * * 1"); // 10 AM every Monday
        }

        /// <summary>
        /// Schedule welcome email for new trial
        /// </summary>
        public static void ScheduleTrialWelcome(this IBackgroundJobClient client, Guid tenantId)
        {
            // Send welcome email immediately
            client.Enqueue<TrialNurtureJob>(job =>
                job.SendNurtureEmailAsync(tenantId, TrialEmailType.Welcome));

            // Schedule nudge for 24 hours later
            client.Schedule<TrialNurtureJob>(job =>
                job.SendNurtureEmailAsync(tenantId, TrialEmailType.Nudge24h),
                TimeSpan.FromHours(24));

            // Schedule value push for 72 hours later
            client.Schedule<TrialNurtureJob>(job =>
                job.SendNurtureEmailAsync(tenantId, TrialEmailType.ValuePush72h),
                TimeSpan.FromHours(72));

            // Schedule midpoint for 5 days later
            client.Schedule<TrialNurtureJob>(job =>
                job.SendNurtureEmailAsync(tenantId, TrialEmailType.Midpoint),
                TimeSpan.FromDays(5));

            // Schedule escalation for 6 days later
            client.Schedule<TrialNurtureJob>(job =>
                job.SendNurtureEmailAsync(tenantId, TrialEmailType.Escalation),
                TimeSpan.FromDays(6));

            // Schedule expired for 7 days later
            client.Schedule<TrialNurtureJob>(job =>
                job.SendNurtureEmailAsync(tenantId, TrialEmailType.Expired),
                TimeSpan.FromDays(7));

            // Schedule winback for 14 days later
            client.Schedule<TrialNurtureJob>(job =>
                job.SendNurtureEmailAsync(tenantId, TrialEmailType.Winback),
                TimeSpan.FromDays(14));
        }
    }
}
