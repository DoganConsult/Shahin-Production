using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Tracks webhook delivery attempts and outcomes.
    /// Provides audit trail for troubleshooting and compliance.
    /// </summary>
    public class WebhookDeliveryLog : BaseEntity
    {
        /// <summary>
        /// Reference to the webhook subscription
        /// </summary>
        [Required]
        public Guid WebhookSubscriptionId { get; set; }

        /// <summary>
        /// Event type that triggered this delivery
        /// </summary>
        [Required]
        [StringLength(100)]
        public string EventType { get; set; } = string.Empty;

        /// <summary>
        /// Unique event ID for correlation with AuditEvent
        /// </summary>
        [Required]
        [StringLength(100)]
        public string EventId { get; set; } = string.Empty;

        /// <summary>
        /// The payload sent to the webhook (JSON)
        /// </summary>
        [Required]
        public string PayloadJson { get; set; } = string.Empty;

        /// <summary>
        /// Delivery status: Pending, Delivered, Failed, Retrying
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// Number of delivery attempts made
        /// </summary>
        public int AttemptCount { get; set; } = 0;

        /// <summary>
        /// HTTP response status code from the last attempt
        /// </summary>
        public int? ResponseStatusCode { get; set; }

        /// <summary>
        /// Response body from the last attempt (truncated to 2000 chars)
        /// </summary>
        [StringLength(2000)]
        public string? ResponseBody { get; set; }

        /// <summary>
        /// Response time in milliseconds
        /// </summary>
        public long? ResponseTimeMs { get; set; }

        /// <summary>
        /// Error message if delivery failed
        /// </summary>
        [StringLength(1000)]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Stack trace if delivery failed with exception
        /// </summary>
        [StringLength(4000)]
        public string? ErrorStackTrace { get; set; }

        /// <summary>
        /// When the first delivery attempt was made
        /// </summary>
        public DateTime? FirstAttemptAt { get; set; }

        /// <summary>
        /// When the last delivery attempt was made
        /// </summary>
        public DateTime? LastAttemptAt { get; set; }

        /// <summary>
        /// When the next retry is scheduled (null if not retrying)
        /// </summary>
        public DateTime? NextRetryAt { get; set; }

        /// <summary>
        /// When the delivery was successfully completed
        /// </summary>
        public DateTime? DeliveredAt { get; set; }

        /// <summary>
        /// HMAC signature sent with the webhook
        /// </summary>
        [StringLength(256)]
        public string? Signature { get; set; }

        /// <summary>
        /// Request headers sent (JSON)
        /// </summary>
        [StringLength(2000)]
        public string? RequestHeaders { get; set; }

        /// <summary>
        /// Response headers received (JSON)
        /// </summary>
        [StringLength(2000)]
        public string? ResponseHeaders { get; set; }

        /// <summary>
        /// The target URL at the time of delivery (in case subscription URL changes)
        /// </summary>
        [Required]
        [StringLength(500)]
        public string TargetUrl { get; set; } = string.Empty;

        // Navigation properties
        [ForeignKey(nameof(WebhookSubscriptionId))]
        public virtual WebhookSubscription? WebhookSubscription { get; set; }

        public virtual Tenant? Tenant { get; set; }
    }

    /// <summary>
    /// Webhook delivery status enum for type safety
    /// </summary>
    public static class WebhookDeliveryStatus
    {
        public const string Pending = "Pending";
        public const string Delivered = "Delivered";
        public const string Failed = "Failed";
        public const string Retrying = "Retrying";
        public const string Cancelled = "Cancelled";
    }
}
