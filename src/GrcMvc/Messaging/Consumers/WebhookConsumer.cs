using GrcMvc.Messaging.Messages;
using GrcMvc.Services.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Messaging.Consumers
{
    /// <summary>
    /// Consumer for webhook delivery commands
    /// </summary>
    public class WebhookConsumer : IConsumer<ProcessWebhookCommand>
    {
        private readonly IWebhookService _webhookService;
        private readonly ILogger<WebhookConsumer> _logger;

        public WebhookConsumer(
            IWebhookService webhookService,
            ILogger<WebhookConsumer> logger)
        {
            _webhookService = webhookService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ProcessWebhookCommand> context)
        {
            var command = context.Message;

            _logger.LogInformation(
                "Processing webhook command {CommandId} for subscription {SubscriptionId}",
                command.CommandId, command.WebhookSubscriptionId);

            try
            {
                var subscription = await _webhookService.GetSubscriptionAsync(command.WebhookSubscriptionId);

                if (subscription == null)
                {
                    _logger.LogWarning(
                        "Webhook subscription {SubscriptionId} not found",
                        command.WebhookSubscriptionId);
                    return;
                }

                if (!subscription.IsActive)
                {
                    _logger.LogDebug(
                        "Webhook subscription {SubscriptionId} is inactive",
                        command.WebhookSubscriptionId);
                    return;
                }

                await _webhookService.DeliverWebhookAsync(
                    subscription,
                    command.EventType,
                    command.EventId,
                    command.PayloadJson);

                _logger.LogInformation(
                    "Webhook command {CommandId} processed successfully",
                    command.CommandId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error processing webhook command {CommandId}: {Message}",
                    command.CommandId, ex.Message);
                throw;
            }
        }
    }
}
