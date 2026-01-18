using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Tracks all login attempts for security monitoring and anomaly detection
    /// </summary>
    [Table("LoginAttempts")]
    public class LoginAttempt
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// User ID (null if user not found)
        /// Kept as string for compatibility - will migrate to Guid in future deployment
        /// </summary>
        [MaxLength(450)]
        public string? UserId { get; set; }

        /// <summary>
        /// Email/username attempted (even if user doesn't exist - for account enumeration protection analysis)
        /// </summary>
        [MaxLength(256)]
        public string? AttemptedEmail { get; set; }

        /// <summary>
        /// Whether login was successful
        /// </summary>
        [Required]
        public bool Success { get; set; }

        /// <summary>
        /// IP address of the login attempt
        /// </summary>
        [Required]
        [MaxLength(45)] // IPv6 max length
        public string IpAddress { get; set; } = string.Empty;

        /// <summary>
        /// Country code (from geolocation lookup)
        /// </summary>
        [MaxLength(2)] // ISO 3166-1 alpha-2
        public string? Country { get; set; }

        /// <summary>
        /// City name (from geolocation lookup)
        /// </summary>
        [MaxLength(100)]
        public string? City { get; set; }

        /// <summary>
        /// User agent of the device/browser
        /// </summary>
        [MaxLength(500)]
        public string? UserAgent { get; set; }

        /// <summary>
        /// Device fingerprint (browser/device characteristics)
        /// </summary>
        [MaxLength(256)]
        public string? DeviceFingerprint { get; set; }

        /// <summary>
        /// Failure reason: "Invalid credentials", "Account locked", "Account disabled", "2FA required", etc.
        /// </summary>
        [MaxLength(200)]
        public string? FailureReason { get; set; }

        /// <summary>
        /// Timestamp of the login attempt
        /// </summary>
        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Whether this attempt triggered account lockout
        /// </summary>
        public bool TriggeredLockout { get; set; }

        /// <summary>
        /// Whether this attempt was flagged as suspicious
        /// </summary>
        public bool FlaggedSuspicious { get; set; }

        /// <summary>
        /// Suspicious activity flags (comma-separated): "ImpossibleTravel", "UnusualTime", "BruteForce", "KnownBot"
        /// </summary>
        [MaxLength(200)]
        public string? SuspiciousFlags { get; set; }

        // Navigation property
        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser? User { get; set; }
    }
}
