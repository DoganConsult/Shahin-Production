using System;
using System.ComponentModel.DataAnnotations;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Newsletter subscription entity for marketing communications
    /// الاشتراك في النشرة الإخبارية للتواصل التسويقي
    /// 
    /// ABP Best Practice: Follows entity design pattern with proper annotations
    /// </summary>
    public class NewsletterSubscription : BaseEntity
    {
        /// <summary>
        /// Subscriber email address (unique identifier)
        /// </summary>
        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Subscriber name (optional)
        /// </summary>
        [StringLength(100)]
        public string? Name { get; set; }

        /// <summary>
        /// Whether subscription is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Locale preference: en, ar
        /// </summary>
        [StringLength(10)]
        public string Locale { get; set; } = "ar";

        /// <summary>
        /// Source of subscription: LandingPage, Checkout, Import, API
        /// </summary>
        [StringLength(50)]
        public string Source { get; set; } = "LandingPage";

        /// <summary>
        /// IP address at subscription time
        /// </summary>
        [StringLength(50)]
        public string? IpAddress { get; set; }

        /// <summary>
        /// Timestamp when subscription was created
        /// </summary>
        public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Timestamp when unsubscribed (null if still active)
        /// </summary>
        public DateTime? UnsubscribedAt { get; set; }

        /// <summary>
        /// Timestamp when resubscribed (if previously unsubscribed)
        /// </summary>
        public DateTime? ResubscribedAt { get; set; }

        /// <summary>
        /// Last email sent timestamp
        /// </summary>
        public DateTime? LastEmailSentAt { get; set; }

        /// <summary>
        /// Count of emails sent to this subscriber
        /// </summary>
        public int EmailsSentCount { get; set; } = 0;

        /// <summary>
        /// Count of emails opened by subscriber
        /// </summary>
        public int EmailsOpenedCount { get; set; } = 0;

        /// <summary>
        /// Comma-separated list of interest topics
        /// </summary>
        [StringLength(500)]
        public string? Interests { get; set; }

        /// <summary>
        /// Unsubscribe token for secure unsubscribe links
        /// </summary>
        [StringLength(100)]
        public string? UnsubscribeToken { get; set; }
    }
}
