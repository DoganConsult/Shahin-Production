using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// AM-08: Secure password reset token entity.
    /// Single-use tokens with configurable TTL for password recovery.
    /// </summary>
    [Table("PasswordResetTokens")]
    public class PasswordResetToken
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// User ID this token belongs to.
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// Email address the reset was requested for (for audit).
        /// </summary>
        [Required]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Hash of the reset token (never store plaintext).
        /// </summary>
        [Required]
        [MaxLength(128)]
        public string TokenHash { get; set; } = string.Empty;

        /// <summary>
        /// Token creation timestamp.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Token expiration timestamp.
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Whether the token has been used.
        /// </summary>
        public bool IsUsed { get; set; } = false;

        /// <summary>
        /// When the token was used (if applicable).
        /// </summary>
        public DateTime? UsedAt { get; set; }

        /// <summary>
        /// IP address that requested the reset.
        /// </summary>
        [MaxLength(45)]
        public string? RequestIpAddress { get; set; }

        /// <summary>
        /// User agent of the reset request.
        /// </summary>
        [MaxLength(500)]
        public string? RequestUserAgent { get; set; }

        /// <summary>
        /// IP address that completed the reset (if used).
        /// </summary>
        [MaxLength(45)]
        public string? CompletionIpAddress { get; set; }

        /// <summary>
        /// Whether the token was explicitly revoked.
        /// </summary>
        public bool IsRevoked { get; set; } = false;

        /// <summary>
        /// When the token was revoked (if applicable).
        /// </summary>
        public DateTime? RevokedAt { get; set; }

        /// <summary>
        /// Reason for revocation (if applicable).
        /// </summary>
        [MaxLength(200)]
        public string? RevocationReason { get; set; }

        /// <summary>
        /// Tenant ID for multi-tenant context.
        /// </summary>
        public Guid? TenantId { get; set; }

        /// <summary>
        /// Navigation property to User (optional).
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser? User { get; set; }

        /// <summary>
        /// Check if the token is valid (not expired, not used, not revoked).
        /// </summary>
        public bool IsValid => !IsUsed && !IsRevoked && ExpiresAt > DateTime.UtcNow;

        /// <summary>
        /// Create a new password reset token with secure random value.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="email">User email</param>
        /// <param name="ttlMinutes">Token TTL in minutes</param>
        /// <returns>Tuple of (token entity, plaintext token for email)</returns>
        public static (PasswordResetToken Token, string PlaintextToken) Create(
            Guid userId,
            string email,
            int ttlMinutes = 60)
        {
            // Generate cryptographically secure random token
            var tokenBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(tokenBytes);
            }
            var plaintextToken = Convert.ToBase64String(tokenBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');

            // Hash the token for storage
            var tokenHash = HashToken(plaintextToken);

            var token = new PasswordResetToken
            {
                UserId = userId,
                Email = email,
                TokenHash = tokenHash,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(ttlMinutes)
            };

            return (token, plaintextToken);
        }

        /// <summary>
        /// Hash a token using SHA-256.
        /// </summary>
        public static string HashToken(string plaintextToken)
        {
            using var sha256 = SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(plaintextToken);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }

        /// <summary>
        /// Verify a plaintext token against this token's hash.
        /// </summary>
        public bool VerifyToken(string plaintextToken)
        {
            var hash = HashToken(plaintextToken);
            return string.Equals(TokenHash, hash, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Mark the token as used.
        /// </summary>
        public void MarkUsed(string? completionIpAddress = null)
        {
            IsUsed = true;
            UsedAt = DateTime.UtcNow;
            CompletionIpAddress = completionIpAddress;
        }

        /// <summary>
        /// Revoke the token.
        /// </summary>
        public void Revoke(string reason)
        {
            IsRevoked = true;
            RevokedAt = DateTime.UtcNow;
            RevocationReason = reason;
        }
    }
}
