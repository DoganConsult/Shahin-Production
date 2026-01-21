using System;
using System.ComponentModel.DataAnnotations;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Webhook subscription configuration for outbound event delivery.
    /// Allows tenants to receive real-time notifications when specific events occur.
    /// </summary>
    public class WebhookSubscription : BaseEntity
    {
        /// <summary>
        /// Human-readable name for the webhook (e.g., "SIEM Integration", "Slack Alerts")
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Target URL that will receive the webhook POST requests
        /// Must be HTTPS in production
        /// </summary>
        [Required]
        [StringLength(500)]
        [Url]
        public string TargetUrl { get; set; } = string.Empty;

        /// <summary>
        /// Secret key for HMAC-SHA256 signature verification
        /// Stored encrypted; used to sign payloads for receiver validation
        /// </summary>
        [Required]
        [StringLength(256)]
        public string Secret { get; set; } = string.Empty;

        /// <summary>
        /// Comma-separated list of event types to subscribe to
        /// Examples: "Risk.Created,Control.Updated,Assessment.Completed,SLA.Breached"
        /// Use "*" to subscribe to all events
        /// </summary>
        [Required]
        [StringLength(1000)]
        public string EventTypes { get; set; } = "*";

        /// <summary>
        /// Whether this webhook is currently active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Optional description of the webhook purpose
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Custom headers to include with webhook requests (JSON format)
        /// Example: {"Authorization": "Bearer xyz", "X-Custom-Header": "value"}
        /// </summary>
        [StringLength(2000)]
        public string? CustomHeaders { get; set; }

        /// <summary>
        /// Timeout in seconds for webhook delivery (default: 30)
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Maximum retry attempts for failed deliveries (default: 5)
        /// </summary>
        public int MaxRetries { get; set; } = 5;

        /// <summary>
        /// Retry delay in seconds (exponential backoff: 10, 30, 120, 600, 3600)
        /// </summary>
        [StringLength(100)]
        public string RetryDelays { get; set; } = "10,30,120,600,3600";

        /// <summary>
        /// Content type for webhook payload (default: application/json)
        /// </summary>
        [StringLength(50)]
        public string ContentType { get; set; } = "application/json";

        /// <summary>
        /// Last successful delivery timestamp
        /// </summary>
        public DateTime? LastSuccessAt { get; set; }

        /// <summary>
        /// Last failed delivery timestamp
        /// </summary>
        public DateTime? LastFailureAt { get; set; }

        /// <summary>
        /// Last error message from failed delivery
        /// </summary>
        [StringLength(1000)]
        public string? LastErrorMessage { get; set; }

        /// <summary>
        /// Total successful deliveries count
        /// </summary>
        public long SuccessCount { get; set; } = 0;

        /// <summary>
        /// Total failed deliveries count
        /// </summary>
        public long FailureCount { get; set; } = 0;

        /// <summary>
        /// Auto-disable after N consecutive failures (0 = never)
        /// </summary>
        public int DisableAfterFailures { get; set; } = 10;

        /// <summary>
        /// Current consecutive failure count
        /// </summary>
        public int ConsecutiveFailures { get; set; } = 0;

        /// <summary>
        /// When the webhook was disabled due to failures
        /// </summary>
        public DateTime? DisabledAt { get; set; }

        /// <summary>
        /// Reason for being disabled
        /// </summary>
        [StringLength(500)]
        public string? DisabledReason { get; set; }

        // Navigation properties
        public virtual Tenant? Tenant { get; set; }
    }
}
