using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Tracks usage records for tenant resource consumption
    /// Used for billing, limits enforcement, and analytics
    /// </summary>
    [Table("TenantUsageRecords")]
    public class TenantUsageRecord : BaseEntity
    {
        /// <summary>Tenant that owns this usage record</summary>
        public Guid TenantId { get; set; }

        /// <summary>Type of usage: AI_CALL, STORAGE, FRAMEWORK_ACTIVATION, REPORT_GENERATED, API_CALL</summary>
        [Required]
        [MaxLength(50)]
        public string UsageType { get; set; } = string.Empty;

        /// <summary>Quantity/count for this usage (e.g., 1 for AI call, bytes for storage)</summary>
        public int Quantity { get; set; } = 1;

        /// <summary>JSON metadata about the usage (model name, tokens, file name, etc.)</summary>
        public string? Metadata { get; set; }

        /// <summary>When this usage was recorded</summary>
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Optional: User who triggered this usage</summary>
        [MaxLength(100)]
        public string? UserId { get; set; }

        /// <summary>Optional: Session ID for tracking</summary>
        [MaxLength(100)]
        public string? SessionId { get; set; }

        /// <summary>Optional: IP address for security tracking</summary>
        [MaxLength(45)]
        public string? IpAddress { get; set; }

        // Navigation
        [ForeignKey("TenantId")]
        public virtual Tenant? Tenant { get; set; }
    }
}
