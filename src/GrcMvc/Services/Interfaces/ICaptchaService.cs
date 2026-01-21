using System.Threading.Tasks;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Service for CAPTCHA validation to prevent automated attacks
    /// SECURITY: Protects registration, login, and password reset endpoints from bots
    /// </summary>
    public interface ICaptchaService
    {
        /// <summary>
        /// Validate a CAPTCHA response token
        /// </summary>
        /// <param name="captchaResponse">The CAPTCHA response token from the client</param>
        /// <param name="remoteIp">The client's IP address (for enhanced validation)</param>
        /// <returns>True if the CAPTCHA is valid</returns>
        Task<bool> ValidateCaptchaAsync(string captchaResponse, string? remoteIp = null);

        /// <summary>
        /// Get the CAPTCHA site key for client-side rendering
        /// </summary>
        /// <returns>The site key</returns>
        string GetSiteKey();

        /// <summary>
        /// Check if CAPTCHA is enabled
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Get the CAPTCHA provider type (e.g., "reCAPTCHA", "hCaptcha")
        /// </summary>
        string ProviderType { get; }
    }
}
