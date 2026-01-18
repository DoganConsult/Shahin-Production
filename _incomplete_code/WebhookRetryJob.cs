using GrcMvc.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GrcMvc.BackgroundJobs
{
    /// <summary>
    /// Background job for retrying failed webhook deliveries.
    /// Runs every 2 minutes to process pending retries.
    /// </summary>
    public class WebhookRetryJob
    {
        private readonly IWebhookService _webhookService;
        private readonly ILogger<WebhookRetryJob> _logger;

        public WebhookRetryJob(
            IWebhookService webhookService,
            ILogger<WebhookRetryJob> logger)
        {
            _webhookService = webhookService;
            _logger = logger;
        }

        /// <summary>
        /// Main job execution method - called by Hangfire scheduler
        /// </summary>
        [Hangfire.AutomaticRetry(Attempts = 2, DelaysInSeconds = new[] { 30, 60 })]
        public async Task ExecuteAsync()
        {
            _logger.LogDebug("WebhookRetryJob started at {Time}", DateTime.UtcNow);

            try
            {
                await _webhookService.RetryFailedDeliveriesAsync();

                _logger.LogDebug("WebhookRetryJob completed at {Time}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WebhookRetryJob failed with error: {Message}", ex.Message);
                throw;
            }
        }
    }
}
