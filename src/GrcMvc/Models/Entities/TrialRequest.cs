using System;
using System.ComponentModel.DataAnnotations;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Trial request for evaluation period
    /// نسخة تجريبية للتحضير للإصدار الكامل
    /// </summary>
    public class TrialRequest : BaseEntity
    {
        [Required]
        [StringLength(255)]
        public string OrganizationName { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string AdminName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string AdminEmail { get; set; } = string.Empty;

        [StringLength(100)]
        public string Country { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Phone { get; set; }

        /// <summary>
        /// Status of trial request
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Submitted";
        // Submitted, PaymentVerified, Provisioned, OnboardingStarted, Rejected, Expired

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public DateTime? TermsAcceptedAt { get; set; }

        /// <summary>
        /// Tenant ID once provisioned
        /// </summary>
        public Guid? ProvisionedTenantId { get; set; }

        /// <summary>
        /// Payment verification status
        /// </summary>
        [StringLength(50)]
        public string? PaymentVerificationStatus { get; set; }

        /// <summary>
        /// Payment provider customer ID
        /// </summary>
        [StringLength(255)]
        public string? ProviderCustomerId { get; set; }

        /// <summary>
        /// Payment method reference
        /// </summary>
        [StringLength(255)]
        public string? PaymentMethodId { get; set; }

        /// <summary>
        /// Source of request (UI, API, Campaign, etc.)
        /// </summary>
        [StringLength(100)]
        public string Source { get; set; } = "UI";

        /// <summary>
        /// IP address of requester
        /// </summary>
        [StringLength(50)]
        public string? RequestIp { get; set; }

        /// <summary>
        /// User agent of requester
        /// </summary>
        [StringLength(500)]
        public string? UserAgent { get; set; }

        /// <summary>
        /// Notes or rejection reason
        /// </summary>
        [StringLength(1000)]
        public string? Notes { get; set; }

        // Navigation
        public virtual Tenant? Tenant { get; set; }
    }
}
