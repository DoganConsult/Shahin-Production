using GrcMvc.Messaging.Messages;
using GrcMvc.Services.Interfaces;
using GrcMvc.Services.Implementations;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Messaging.Consumers
{
    /// <summary>
    /// Consumer for notification commands - routes to appropriate notification channel
    /// </summary>
    public class NotificationConsumer : IConsumer<SendNotificationCommand>
    {
        private readonly ISmtpEmailService _emailService;
        private readonly ISlackNotificationService _slackService;
        private readonly ITeamsNotificationService _teamsService;
        private readonly ISmsNotificationService _smsService;
        private readonly ILogger<NotificationConsumer> _logger;

        public NotificationConsumer(
            ISmtpEmailService emailService,
            ISlackNotificationService slackService,
            ITeamsNotificationService teamsService,
            ISmsNotificationService smsService,
            ILogger<NotificationConsumer> logger)
        {
            _emailService = emailService;
            _slackService = slackService;
            _teamsService = teamsService;
            _smsService = smsService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<SendNotificationCommand> context)
        {
            var command = context.Message;

            _logger.LogInformation(
                "Processing notification command {CommandId}: {Channel} to {Recipient}",
                command.CommandId, command.Channel, command.RecipientUserId);

            try
            {
                var success = command.Channel.ToLowerInvariant() switch
                {
                    "email" => await SendEmailAsync(command),
                    "slack" => await SendSlackAsync(command),
                    "teams" => await SendTeamsAsync(command),
                    "sms" => await SendSmsAsync(command),
                    _ => throw new ArgumentException($"Unknown notification channel: {command.Channel}")
                };

                if (success)
                {
                    _logger.LogInformation(
                        "Notification {CommandId} sent successfully via {Channel}",
                        command.CommandId, command.Channel);
                }
                else
                {
                    _logger.LogWarning(
                        "Notification {CommandId} failed via {Channel}",
                        command.CommandId, command.Channel);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error processing notification command {CommandId}: {Message}",
                    command.CommandId, ex.Message);
                throw;
            }
        }

        private async Task<bool> SendEmailAsync(SendNotificationCommand command)
        {
            // Get recipient email from metadata or user lookup
            var recipientEmail = command.Metadata?.GetValueOrDefault("email") ?? command.RecipientUserId;

            if (string.IsNullOrEmpty(recipientEmail) || !recipientEmail.Contains('@'))
            {
                _logger.LogWarning("No valid email for recipient {UserId}", command.RecipientUserId);
                return false;
            }

            await _emailService.SendEmailAsync(recipientEmail, command.Subject, command.Body);
            return true;
        }

        private async Task<bool> SendSlackAsync(SendNotificationCommand command)
        {
            if (!_slackService.IsEnabled)
            {
                _logger.LogDebug("Slack not enabled, skipping notification");
                return false;
            }

            var severity = command.Priority switch
            {
                "Critical" => AlertSeverity.Critical,
                "High" => AlertSeverity.Error,
                "Medium" => AlertSeverity.Warning,
                _ => AlertSeverity.Info
            };

            return await _slackService.SendAlertAsync(command.Subject, command.Body, severity);
        }

        private async Task<bool> SendTeamsAsync(SendNotificationCommand command)
        {
            if (!_teamsService.IsEnabled)
            {
                _logger.LogDebug("Teams not enabled, skipping notification");
                return false;
            }

            var severity = command.Priority switch
            {
                "Critical" => AlertSeverity.Critical,
                "High" => AlertSeverity.Error,
                "Medium" => AlertSeverity.Warning,
                _ => AlertSeverity.Info
            };

            return await _teamsService.SendAlertAsync(command.Subject, command.Body, severity);
        }

        private async Task<bool> SendSmsAsync(SendNotificationCommand command)
        {
            if (!_smsService.IsEnabled)
            {
                _logger.LogDebug("SMS not enabled, skipping notification");
                return false;
            }

            var phoneNumber = command.Metadata?.GetValueOrDefault("phone");
            if (string.IsNullOrEmpty(phoneNumber))
            {
                _logger.LogWarning("No phone number for SMS recipient {UserId}", command.RecipientUserId);
                return false;
            }

            var result = await _smsService.SendSmsAsync(phoneNumber, $"{command.Subject}\n{command.Body}");
            return result.Success;
        }
    }
}
