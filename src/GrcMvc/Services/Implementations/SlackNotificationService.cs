using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using GrcMvc.Configuration;
using GrcMvc.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Service for sending notifications to Slack via incoming webhooks
    /// </summary>
    public class SlackNotificationService : ISlackNotificationService
    {
        private readonly SlackSettings _settings;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SlackNotificationService> _logger;

        public SlackNotificationService(
            IOptions<SlackSettings> settings,
            IHttpClientFactory httpClientFactory,
            ILogger<SlackNotificationService> logger)
        {
            _settings = settings.Value;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public bool IsEnabled => _settings.Enabled && !string.IsNullOrEmpty(_settings.WebhookUrl);

        public async Task<bool> SendMessageAsync(string message)
        {
            return await SendMessageAsync(_settings.DefaultChannel, message);
        }

        public async Task<bool> SendMessageAsync(string channel, string message)
        {
            if (!IsEnabled)
            {
                _logger.LogDebug("Slack notifications disabled, skipping message");
                return false;
            }

            var payload = new
            {
                channel = channel,
                username = _settings.BotUsername,
                icon_emoji = _settings.IconEmoji,
                text = message
            };

            return await SendPayloadAsync(payload);
        }

        public async Task<bool> SendNotificationAsync(SlackNotification notification)
        {
            if (!IsEnabled)
            {
                _logger.LogDebug("Slack notifications disabled, skipping notification");
                return false;
            }

            var payload = new
            {
                channel = notification.Channel ?? _settings.DefaultChannel,
                username = _settings.BotUsername,
                icon_emoji = _settings.IconEmoji,
                text = notification.Text,
                mrkdwn = notification.Markdown,
                attachments = notification.Attachments.Select(a => new
                {
                    title = a.Title,
                    title_link = a.TitleLink,
                    text = a.Text,
                    color = a.Color,
                    fields = a.Fields.Select(f => new
                    {
                        title = f.Title,
                        value = f.Value,
                        @short = f.Short
                    }).ToArray(),
                    footer = a.Footer,
                    ts = a.Timestamp
                }).ToArray()
            };

            return await SendPayloadAsync(payload);
        }

        public async Task<bool> SendAlertAsync(string title, string message, AlertSeverity severity)
        {
            var color = severity switch
            {
                AlertSeverity.Info => "#36a64f",      // green
                AlertSeverity.Warning => "#ffcc00",   // yellow
                AlertSeverity.Error => "#ff6600",     // orange
                AlertSeverity.Critical => "#ff0000", // red
                _ => "#808080"                        // gray
            };

            var emoji = severity switch
            {
                AlertSeverity.Info => ":information_source:",
                AlertSeverity.Warning => ":warning:",
                AlertSeverity.Error => ":x:",
                AlertSeverity.Critical => ":rotating_light:",
                _ => ":bell:"
            };

            var notification = new SlackNotification
            {
                Text = $"{emoji} *{severity}*",
                Attachments = new List<SlackAttachment>
                {
                    new SlackAttachment
                    {
                        Title = title,
                        Text = message,
                        Color = color,
                        Footer = "Shahin GRC System",
                        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                    }
                }
            };

            return await SendNotificationAsync(notification);
        }

        private async Task<bool> SendPayloadAsync(object payload)
        {
            try
            {
                using var client = _httpClientFactory.CreateClient("SlackClient");
                client.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);

                var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(_settings.WebhookUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogDebug("Slack message sent successfully");
                    return true;
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogWarning(
                    "Slack webhook failed: {StatusCode} - {Response}",
                    response.StatusCode, responseBody);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending Slack notification: {Message}", ex.Message);
                return false;
            }
        }
    }
}
