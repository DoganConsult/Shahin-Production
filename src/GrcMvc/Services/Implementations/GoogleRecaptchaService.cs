using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using GrcMvc.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Google reCAPTCHA v2/v3 validation service
    /// SECURITY: Validates CAPTCHA responses to prevent automated attacks
    /// </summary>
    public class GoogleRecaptchaService : ICaptchaService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GoogleRecaptchaService> _logger;
        private readonly string _siteKey;
        private readonly string _secretKey;
        private readonly bool _isEnabled;
        private readonly float _minimumScore;
        private readonly string _verifyUrl;

        public GoogleRecaptchaService(
            HttpClient httpClient,
            ILogger<GoogleRecaptchaService> logger,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;

            var captchaSection = configuration.GetSection("Security:Captcha");
            _siteKey = captchaSection.GetValue<string>("SiteKey") ?? "";
            _secretKey = captchaSection.GetValue<string>("SecretKey") ?? "";
            _isEnabled = captchaSection.GetValue<bool>("Enabled", false);
            _minimumScore = captchaSection.GetValue<float>("MinimumScore", 0.5f);
            _verifyUrl = captchaSection.GetValue<string>("VerifyUrl") 
                ?? "https://www.google.com/recaptcha/api/siteverify";

            if (_isEnabled && (string.IsNullOrEmpty(_siteKey) || string.IsNullOrEmpty(_secretKey)))
            {
                _logger.LogWarning("CAPTCHA is enabled but site key or secret key is not configured");
                _isEnabled = false;
            }
        }

        public bool IsEnabled => _isEnabled;

        public string ProviderType => "reCAPTCHA";

        public string GetSiteKey() => _siteKey;

        /// <summary>
        /// Validate a CAPTCHA response token with Google's reCAPTCHA API
        /// </summary>
        public async Task<bool> ValidateCaptchaAsync(string captchaResponse, string? remoteIp = null)
        {
            if (!_isEnabled)
            {
                _logger.LogDebug("CAPTCHA validation skipped - CAPTCHA is disabled");
                return true; // Allow request if CAPTCHA is disabled
            }

            if (string.IsNullOrEmpty(captchaResponse))
            {
                _logger.LogWarning("CAPTCHA validation failed - empty response token");
                return false;
            }

            try
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("secret", _secretKey),
                    new KeyValuePair<string, string>("response", captchaResponse),
                    new KeyValuePair<string, string>("remoteip", remoteIp ?? "")
                });

                var response = await _httpClient.PostAsync(_verifyUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("CAPTCHA API returned non-success status: {StatusCode}", response.StatusCode);
                    return false;
                }

                var result = JsonSerializer.Deserialize<RecaptchaResponse>(responseContent);
                if (result == null)
                {
                    _logger.LogWarning("CAPTCHA API returned invalid JSON response");
                    return false;
                }

                if (!result.Success)
                {
                    _logger.LogWarning("CAPTCHA validation failed. Error codes: {Errors}", 
                        string.Join(", ", result.ErrorCodes ?? Array.Empty<string>()));
                    return false;
                }

                // For reCAPTCHA v3, check the score
                if (result.Score.HasValue && result.Score.Value < _minimumScore)
                {
                    _logger.LogWarning("CAPTCHA score too low: {Score} (minimum: {MinScore})", 
                        result.Score, _minimumScore);
                    return false;
                }

                _logger.LogDebug("CAPTCHA validation successful. Score: {Score}", result.Score);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CAPTCHA validation failed due to exception");
                return false;
            }
        }

        /// <summary>
        /// Response from Google reCAPTCHA API
        /// </summary>
        private class RecaptchaResponse
        {
            [JsonPropertyName("success")]
            public bool Success { get; set; }
            
            [JsonPropertyName("score")]
            public float? Score { get; set; }
            
            [JsonPropertyName("action")]
            public string? Action { get; set; }
            
            [JsonPropertyName("challenge_ts")]
            public DateTime? ChallengeTs { get; set; }
            
            [JsonPropertyName("hostname")]
            public string? Hostname { get; set; }
            
            [JsonPropertyName("error-codes")]
            public string[]? ErrorCodes { get; set; }
        }
    }
}
