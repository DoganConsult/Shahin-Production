using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using GrcMvc.Configuration;
using GrcMvc.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Service for sending SMS notifications via Twilio REST API
    /// </summary>
    public class TwilioSmsService : ISmsNotificationService
    {
        private readonly TwilioSettings _settings;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<TwilioSmsService> _logger;
        private static readonly Regex PhoneRegex = new(@"^\+[1-9]\d{1,14}$", RegexOptions.Compiled);

        public TwilioSmsService(
            IOptions<TwilioSettings> settings,
            IHttpClientFactory httpClientFactory,
            ILogger<TwilioSmsService> logger)
        {
            _settings = settings.Value;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public bool IsEnabled => _settings.Enabled &&
                                 !string.IsNullOrEmpty(_settings.AccountSid) &&
                                 !string.IsNullOrEmpty(_settings.AuthToken) &&
                                 !string.IsNullOrEmpty(_settings.FromNumber);

        public async Task<SmsResult> SendSmsAsync(string phoneNumber, string message)
        {
            if (!IsEnabled)
            {
                _logger.LogDebug("SMS notifications disabled, skipping message");
                return new SmsResult
                {
                    Success = false,
                    PhoneNumber = phoneNumber,
                    ErrorMessage = "SMS notifications are disabled"
                };
            }

            if (!ValidatePhoneNumber(phoneNumber))
            {
                return new SmsResult
                {
                    Success = false,
                    PhoneNumber = phoneNumber,
                    ErrorCode = "INVALID_PHONE",
                    ErrorMessage = "Invalid phone number format. Use E.164 format (e.g., +15551234567)"
                };
            }

            // Truncate message if too long
            if (message.Length > _settings.MaxMessageLength)
            {
                message = message[..(_settings.MaxMessageLength - 3)] + "...";
            }

            try
            {
                using var client = _httpClientFactory.CreateClient("TwilioClient");
                client.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);

                // Set up Basic Auth
                var authBytes = Encoding.ASCII.GetBytes($"{_settings.AccountSid}:{_settings.AuthToken}");
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));

                var url = $"https://api.twilio.com/2010-04-01/Accounts/{_settings.AccountSid}/Messages.json";

                var formData = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("To", phoneNumber),
                    new KeyValuePair<string, string>("From", _settings.FromNumber),
                    new KeyValuePair<string, string>("Body", message)
                });

                var response = await client.PostAsync(url, formData);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var twilioResponse = JsonSerializer.Deserialize<TwilioMessageResponse>(responseBody,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    _logger.LogInformation(
                        "SMS sent successfully to {PhoneNumber}, SID: {MessageSid}",
                        phoneNumber, twilioResponse?.Sid);

                    return new SmsResult
                    {
                        Success = true,
                        PhoneNumber = phoneNumber,
                        MessageSid = twilioResponse?.Sid,
                        SentAt = DateTime.UtcNow
                    };
                }

                var errorResponse = JsonSerializer.Deserialize<TwilioErrorResponse>(responseBody,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                _logger.LogWarning(
                    "Twilio SMS failed: {Code} - {Message}",
                    errorResponse?.Code, errorResponse?.Message);

                return new SmsResult
                {
                    Success = false,
                    PhoneNumber = phoneNumber,
                    ErrorCode = errorResponse?.Code?.ToString(),
                    ErrorMessage = errorResponse?.Message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SMS to {PhoneNumber}: {Message}", phoneNumber, ex.Message);
                return new SmsResult
                {
                    Success = false,
                    PhoneNumber = phoneNumber,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<List<SmsResult>> SendBulkSmsAsync(IEnumerable<string> phoneNumbers, string message)
        {
            var results = new List<SmsResult>();

            foreach (var phoneNumber in phoneNumbers)
            {
                var result = await SendSmsAsync(phoneNumber, message);
                results.Add(result);

                // Small delay to avoid rate limiting
                await Task.Delay(100);
            }

            return results;
        }

        public async Task<SmsResult> SendAlertSmsAsync(string phoneNumber, string title, string message, AlertSeverity severity)
        {
            var severityPrefix = severity switch
            {
                AlertSeverity.Info => "[INFO]",
                AlertSeverity.Warning => "[WARNING]",
                AlertSeverity.Error => "[ERROR]",
                AlertSeverity.Critical => "[CRITICAL]",
                _ => "[ALERT]"
            };

            var fullMessage = $"{severityPrefix} {title}\n{message}";
            return await SendSmsAsync(phoneNumber, fullMessage);
        }

        public bool ValidatePhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            return PhoneRegex.IsMatch(phoneNumber);
        }

        private class TwilioMessageResponse
        {
            public string? Sid { get; set; }
            public string? Status { get; set; }
        }

        private class TwilioErrorResponse
        {
            public int? Code { get; set; }
            public string? Message { get; set; }
            public string? MoreInfo { get; set; }
        }
    }
}
