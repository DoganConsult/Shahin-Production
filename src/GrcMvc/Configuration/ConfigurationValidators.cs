using Microsoft.Extensions.Options;

namespace GrcMvc.Configuration
{
    /// <summary>
    /// Validator for JWT settings at startup
    /// </summary>
    public class JwtSettingsValidator : IValidateOptions<JwtSettings>
    {
        public ValidateOptionsResult Validate(string? name, JwtSettings options)
        {
            if (options == null)
            {
                return ValidateOptionsResult.Fail("JwtSettings configuration is missing");
            }

            if (string.IsNullOrWhiteSpace(options.Secret))
            {
                return ValidateOptionsResult.Fail("JWT Secret is required. Set via JwtSettings__Secret environment variable");
            }

            if (options.Secret.Length < 32)
            {
                return ValidateOptionsResult.Fail("JWT Secret must be at least 32 characters for security");
            }

            if (string.IsNullOrWhiteSpace(options.Issuer))
            {
                return ValidateOptionsResult.Fail("JWT Issuer is required");
            }

            if (string.IsNullOrWhiteSpace(options.Audience))
            {
                return ValidateOptionsResult.Fail("JWT Audience is required");
            }

            if (options.ExpirationInMinutes < 1 || options.ExpirationInMinutes > 1440)
            {
                return ValidateOptionsResult.Fail("JWT ExpirationInMinutes must be between 1 and 1440 (24 hours)");
            }

            return ValidateOptionsResult.Success;
        }
    }

    /// <summary>
    /// Validator for application settings at startup
    /// </summary>
    public class ApplicationSettingsValidator : IValidateOptions<ApplicationSettings>
    {
        public ValidateOptionsResult Validate(string? name, ApplicationSettings options)
        {
            if (options == null)
            {
                return ValidateOptionsResult.Fail("ApplicationSettings configuration is missing");
            }

            if (string.IsNullOrWhiteSpace(options.ApplicationName))
            {
                return ValidateOptionsResult.Fail("ApplicationName is required");
            }

            if (string.IsNullOrWhiteSpace(options.Version))
            {
                return ValidateOptionsResult.Fail("Version is required");
            }

            if (string.IsNullOrWhiteSpace(options.SupportEmail))
            {
                return ValidateOptionsResult.Fail("SupportEmail is required");
            }

            if (!IsValidEmail(options.SupportEmail))
            {
                return ValidateOptionsResult.Fail($"SupportEmail '{options.SupportEmail}' is not a valid email address");
            }

            if (options.MaxFileUploadSize < 1024)
            {
                return ValidateOptionsResult.Fail("MaxFileUploadSize must be at least 1KB (1024 bytes)");
            }

            if (options.MaxFileUploadSize > 104857600) // 100MB
            {
                return ValidateOptionsResult.Fail("MaxFileUploadSize cannot exceed 100MB (104857600 bytes)");
            }

            if (string.IsNullOrWhiteSpace(options.AllowedFileExtensions))
            {
                return ValidateOptionsResult.Fail("AllowedFileExtensions is required");
            }

            return ValidateOptionsResult.Success;
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}