using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Stores configuration for regulatory portal connections (NCA-ISR, SAMA, PDPL, CITC, MOC).
    /// Credentials are encrypted at rest.
    /// </summary>
    public class RegulatoryPortalConnection : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string PortalType { get; set; } = string.Empty; // NCA-ISR, SAMA-EFILING, PDPL, CITC, MOC

        [Required]
        [MaxLength(200)]
        public string PortalName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? PortalNameAr { get; set; }

        [Required]
        [MaxLength(500)]
        public string BaseUrl { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string AuthType { get; set; } = "ApiKey"; // ApiKey, OAuth2, Certificate, BasicAuth

        /// <summary>
        /// Encrypted API key
        /// </summary>
        [MaxLength(1000)]
        public string? EncryptedApiKey { get; set; }

        /// <summary>
        /// OAuth2 Client ID
        /// </summary>
        [MaxLength(500)]
        public string? ClientId { get; set; }

        /// <summary>
        /// Encrypted OAuth2 Client Secret
        /// </summary>
        [MaxLength(1000)]
        public string? EncryptedClientSecret { get; set; }

        /// <summary>
        /// Certificate thumbprint for certificate-based auth
        /// </summary>
        [MaxLength(100)]
        public string? CertificateThumbprint { get; set; }

        /// <summary>
        /// Username for basic auth
        /// </summary>
        [MaxLength(200)]
        public string? Username { get; set; }

        /// <summary>
        /// Encrypted password for basic auth
        /// </summary>
        [MaxLength(1000)]
        public string? EncryptedPassword { get; set; }

        /// <summary>
        /// Organization identifier in the external portal
        /// </summary>
        [MaxLength(200)]
        public string? OrganizationId { get; set; }

        /// <summary>
        /// JSON-serialized additional settings
        /// </summary>
        public string? AdditionalSettingsJson { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime? LastConnectedAt { get; set; }
        public string? LastConnectionStatus { get; set; }
        public string? DeactivationReason { get; set; }
        public DateTime? DeactivatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<RegulatorySubmission> Submissions { get; set; } = new List<RegulatorySubmission>();
    }
}
