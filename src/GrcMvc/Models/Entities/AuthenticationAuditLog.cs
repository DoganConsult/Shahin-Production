using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Comprehensive audit log for all authentication-related events
    /// Required for GRC compliance and security monitoring
    /// </summary>
    [Table("AuthenticationAuditLogs")]
    public class AuthenticationAuditLog
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// User ID (null for failed authentication events where user doesn't exist)
        /// Kept as string for compatibility - will migrate to Guid in future deployment
        /// </summary>
        [MaxLength(450)]
        public string? UserId { get; set; }

        /// <summary>
        /// Email/username (even if user doesn't exist - for failed attempts)
        /// </summary>
        [MaxLength(256)]
        public string? Email { get; set; }

        /// <summary>
        /// Event type: "Login", "Logout", "FailedLogin", "AccountLocked", "PasswordChanged", 
        /// "RoleChanged", "ClaimsModified", "TokenRefresh", "2FAEnabled", "2FADisabled", etc.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string EventType { get; set; } = string.Empty;

        /// <summary>
        /// Whether the event was successful
        /// </summary>
        [Required]
        public bool Success { get; set; }

        /// <summary>
        /// IP address where the event occurred
        /// </summary>
        [MaxLength(45)] // IPv6 max length
        public string? IpAddress { get; set; }

        /// <summary>
        /// User agent of the device/browser
        /// </summary>
        [MaxLength(500)]
        public string? UserAgent { get; set; }

        /// <summary>
        /// Timestamp of the event
        /// </summary>
        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Additional details in JSON format
        /// Example: {"TenantId": "...", "Roles": ["Admin"], "ClaimsAdded": [...], "Reason": "..."}
        /// </summary>
        [Column(TypeName = "jsonb")]
        public string? Details { get; set; }

        /// <summary>
        /// Correlation ID to link related events (e.g., login + token refresh)
        /// </summary>
        [MaxLength(50)]
        public string? CorrelationId { get; set; }

        /// <summary>
        /// Severity level: "Info", "Warning", "Error", "Critical"
        /// </summary>
        [MaxLength(20)]
        public string Severity { get; set; } = "Info";

        /// <summary>
        /// Human-readable message describing the event
        /// </summary>
        [MaxLength(500)]
        public string? Message { get; set; }

        /// <summary>
        /// Related entity ID (e.g., TenantId, RoleId)
        /// </summary>
        [MaxLength(50)]
        public string? RelatedEntityId { get; set; }

        /// <summary>
        /// Related entity type (e.g., "Tenant", "Role", "Claim")
        /// </summary>
        [MaxLength(50)]
        public string? RelatedEntityType { get; set; }

        // Note: Navigation property removed due to type mismatch
        // UserId is string for compatibility, but ApplicationUser.Id is Guid
        // Use explicit joins when querying user data
    }
}
