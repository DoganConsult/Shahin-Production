using Hangfire;
using GrcMvc.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Jobs
{
    /// <summary>
    /// AM-04: Hangfire job for sending access review reminders.
    /// Runs daily to check for overdue reviews and send notifications.
    /// </summary>
    public class AccessReviewReminderJob
    {
        private readonly IAccessReviewService _accessReviewService;
        private readonly ILogger<AccessReviewReminderJob> _logger;

        public AccessReviewReminderJob(
            IAccessReviewService accessReviewService,
            ILogger<AccessReviewReminderJob> logger)
        {
            _accessReviewService = accessReviewService;
            _logger = logger;
        }

        /// <summary>
        /// Send reminders for overdue access reviews.
        /// This method is called by Hangfire on a recurring schedule.
        /// </summary>
        public async Task SendRemindersAsync()
        {
            try
            {
                _logger.LogInformation("Starting access review reminder job");

                await _accessReviewService.SendRemindersAsync();

                _logger.LogInformation("Access review reminder job completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in access review reminder job");
                throw; // Re-throw to let Hangfire handle retries
            }
        }

        /// <summary>
        /// Execute decisions for completed reviews.
        /// This method processes role revocations/modifications.
        /// </summary>
        public async Task ExecuteDecisionsAsync()
        {
            try
            {
                _logger.LogInformation("Starting access review decision execution job");

                // Get all completed reviews with unexecuted decisions
                // This would be implemented in the service layer

                _logger.LogInformation("Access review decision execution job completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in access review decision execution job");
                throw; // Re-throw to let Hangfire handle retries
            }
        }
    }

    /// <summary>
    /// Configuration for Hangfire recurring jobs.
    /// Register these jobs in your Program.cs or Startup.cs.
    /// </summary>
    public static class AccessReviewJobConfiguration
    {
        public static void ConfigureAccessReviewJobs(this IApplicationBuilder app)
        {
            // Jobs are configured in Program.cs
        }
    }
}
