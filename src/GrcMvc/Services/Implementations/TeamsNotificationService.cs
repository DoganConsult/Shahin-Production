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
    /// Service for sending notifications to Microsoft Teams via incoming webhooks
    /// </summary>
    public class TeamsNotificationService : ITeamsNotificationService
    {
        private readonly TeamsSettings _settings;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<TeamsNotificationService> _logger;

        public TeamsNotificationService(
            IOptions<TeamsSettings> settings,
            IHttpClientFactory httpClientFactory,
            ILogger<TeamsNotificationService> logger)
        {
            _settings = settings.Value;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public bool IsEnabled => _settings.Enabled && !string.IsNullOrEmpty(_settings.WebhookUrl);

        public async Task<bool> SendMessageAsync(string message)
        {
            if (!IsEnabled)
            {
                _logger.LogDebug("Teams notifications disabled, skipping message");
                return false;
            }

            var payload = new
            {
                @type = "MessageCard",
                context = "http://schema.org/extensions",
                themeColor = _settings.ThemeColor,
                summary = message,
                text = message
            };

            return await SendPayloadAsync(payload);
        }

        public async Task<bool> SendNotificationAsync(TeamsNotification notification)
        {
            if (!IsEnabled)
            {
                _logger.LogDebug("Teams notifications disabled, skipping notification");
                return false;
            }

            var payload = new
            {
                @type = "MessageCard",
                context = "http://schema.org/extensions",
                themeColor = notification.ThemeColor ?? _settings.ThemeColor,
                summary = notification.Summary ?? notification.Title,
                title = notification.Title,
                text = notification.Text,
                sections = notification.Sections.Select(s => new
                {
                    title = s.Title,
                    text = s.Text,
                    activityTitle = s.ActivityTitle,
                    activitySubtitle = s.ActivitySubtitle,
                    activityImage = s.ActivityImage,
                    facts = s.Facts.Select(f => new
                    {
                        name = f.Name,
                        value = f.Value
                    }).ToArray(),
                    markdown = s.Markdown
                }).ToArray(),
                potentialAction = notification.Actions.Select(a => new
                {
                    @type = a.Type,
                    name = a.Name,
                    targets = a.Targets.Select(t => new
                    {
                        os = t.Os,
                        uri = t.Uri
                    }).ToArray()
                }).ToArray()
            };

            return await SendPayloadAsync(payload);
        }

        public async Task<bool> SendAlertAsync(string title, string message, AlertSeverity severity)
        {
            var themeColor = severity switch
            {
                AlertSeverity.Info => "0076D7",     // blue
                AlertSeverity.Warning => "FFC107",  // amber
                AlertSeverity.Error => "FF5722",    // deep orange
                AlertSeverity.Critical => "D32F2F", // red
                _ => "808080"                       // gray
            };

            var severityText = severity switch
            {
                AlertSeverity.Info => "ℹ️ Information",
                AlertSeverity.Warning => "⚠️ Warning",
                AlertSeverity.Error => "❌ Error",
                AlertSeverity.Critical => "🚨 Critical Alert",
                _ => "📋 Notification"
            };

            var notification = new TeamsNotification
            {
                Title = title,
                Summary = $"{severity}: {title}",
                ThemeColor = themeColor,
                Sections = new List<TeamsSection>
                {
                    new TeamsSection
                    {
                        Title = severityText,
                        Text = message,
                        Facts = new List<TeamsFact>
                        {
                            new TeamsFact { Name = "Source", Value = "Shahin GRC System" },
                            new TeamsFact { Name = "Time", Value = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC") }
                        }
                    }
                }
            };

            return await SendNotificationAsync(notification);
        }

        private async Task<bool> SendPayloadAsync(object payload)
        {
            try
            {
                using var client = _httpClientFactory.CreateClient("TeamsClient");
                client.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);

                var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(_settings.WebhookUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogDebug("Teams message sent successfully");
                    return true;
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogWarning(
                    "Teams webhook failed: {StatusCode} - {Response}",
                    response.StatusCode, responseBody);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending Teams notification: {Message}", ex.Message);
                return false;
            }
        }
    }
}
