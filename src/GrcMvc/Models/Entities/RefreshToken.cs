using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Secure refresh token storage with rotation support and revocation tracking
    /// Replaces the plain RefreshToken field in ApplicationUser
    /// </summary>
    [Table("RefreshTokens")]
    public class RefreshToken
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// User ID (foreign key to ApplicationUser)
        /// </summary>
        [Required]
        [MaxLength(450)] // ASP.NET Identity UserId length
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Token hash (store hash, never plain text token)
        /// Generated from: HMACSHA256(token + secret)
        /// </summary>
        [Required]
        [MaxLength(256)]
        public string TokenHash { get; set; } = string.Empty;

        /// <summary>
        /// When the token was created
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When the token expires
        /// </summary>
        [Required]
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// When the token was revoked (if revoked)
        /// </summary>
        public DateTime? RevokedAt { get; set; }

        /// <summary>
        /// Reason for revocation: "User logout", "Password changed", "Suspicious activity", "Admin action"
        /// </summary>
        [MaxLength(200)]
        public string? RevokedReason { get; set; }

        /// <summary>
        /// IP address where token was issued
        /// </summary>
        [MaxLength(45)] // IPv6 max length
        public string? IpAddress { get; set; }

        /// <summary>
        /// User agent of the device/browser
        /// </summary>
        [MaxLength(500)]
        public string? UserAgent { get; set; }

        /// <summary>
        /// Device fingerprint for tracking
        /// </summary>
        [MaxLength(256)]
        public string? DeviceFingerprint { get; set; }

        /// <summary>
        /// If this token was rotated, the ID of the new token that replaced it
        /// </summary>
        public Guid? ReplacedByTokenId { get; set; }

        /// <summary>
        /// If this token is a rotation, the ID of the token it replaced
        /// </summary>
        public Guid? ReplacesTokenId { get; set; }

        /// <summary>
        /// Check if token is active (not expired and not revoked)
        /// </summary>
        [NotMapped]
        public bool IsActive => RevokedAt == null && ExpiresAt > DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser? User { get; set; }

        [ForeignKey(nameof(ReplacedByTokenId))]
        public virtual RefreshToken? ReplacedByToken { get; set; }

        [ForeignKey(nameof(ReplacesTokenId))]
        public virtual RefreshToken? ReplacesToken { get; set; }
    }
}
