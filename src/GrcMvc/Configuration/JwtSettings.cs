using System.ComponentModel.DataAnnotations;

namespace GrcMvc.Configuration
{
    /// <summary>
    /// Strongly-typed JWT configuration settings
    /// </summary>
    public sealed class JwtSettings
    {
        public const string SectionName = "JwtSettings";

        [Required]
        [MinLength(32, ErrorMessage = "JWT Secret must be at least 32 characters")]
        public string Secret { get; init; } = string.Empty;

        [Required]
        public string Issuer { get; init; } = string.Empty;

        [Required]
        public string Audience { get; init; } = string.Empty;

        [Range(1, 1440, ErrorMessage = "Expiration must be between 1 and 1440 minutes")]
        public int ExpirationInMinutes { get; init; } = 60;

        /// <summary>
        /// Validates the JWT settings
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Secret) &&
                   Secret.Length >= 32 &&
                   !string.IsNullOrWhiteSpace(Issuer) &&
                   !string.IsNullOrWhiteSpace(Audience) &&
                   ExpirationInMinutes > 0 &&
                   ExpirationInMinutes <= 1440;
        }
    }
}