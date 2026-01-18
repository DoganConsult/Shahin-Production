using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Tracks password history to prevent password reuse (GRC compliance requirement)
    /// </summary>
    [Table("PasswordHistory")]
    public class PasswordHistory
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// User ID (foreign key to ApplicationUser)
        /// Kept as string for compatibility - will migrate to Guid in future deployment
        /// </summary>
        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Password hash (stored, never plain text)
        /// </summary>
        [Required]
        [MaxLength(256)]
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// When the password was changed
        /// </summary>
        [Required]
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// User ID who initiated the change (for admin resets)
        /// Kept as string for compatibility - will migrate to Guid in future deployment
        /// </summary>
        [MaxLength(450)]
        public string? ChangedByUserId { get; set; }

        /// <summary>
        /// Reason for password change: "User initiated", "Admin reset", "Expired", "Policy enforcement"
        /// </summary>
        [MaxLength(100)]
        public string? Reason { get; set; }

        /// <summary>
        /// IP address where password change occurred
        /// </summary>
        [MaxLength(45)] // IPv6 max length
        public string? IpAddress { get; set; }

        /// <summary>
        /// User agent of the device/browser used
        /// </summary>
        [MaxLength(500)]
        public string? UserAgent { get; set; }

        // Navigation property
        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser? User { get; set; }
    }
}
