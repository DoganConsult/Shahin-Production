using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Marketing lead capture entity - captures initial trial interest from landing pages
    /// تسجيل تجربة مجانية من صفحات الهبوط
    ///
    /// This differs from TrialSignup (in TrialEntities.cs) which is the formal provisioning entity.
    /// MarketingTrialLead is for marketing/lead capture, TrialSignup is for actual provisioning.
    ///
    /// ABP Best Practice: Separate entities for different business concerns
    /// </summary>
    [Table("MarketingTrialLeads")]
    public class MarketingTrialLead : BaseEntity
    {
        /// <summary>
        /// Contact email address
        /// </summary>
        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Full name of the contact
        /// </summary>
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Company/Organization name
        /// </summary>
        [Required]
        [StringLength(200)]
        public string CompanyName { get; set; } = string.Empty;

        /// <summary>
        /// Phone number (optional)
        /// </summary>
        [Phone]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Company size category: 1-10, 11-50, 51-200, 201-500, 500+
        /// </summary>
        [StringLength(20)]
        public string? CompanySize { get; set; }

        /// <summary>
        /// Industry vertical
        /// </summary>
        [StringLength(100)]
        public string? Industry { get; set; }

        /// <summary>
        /// Selected trial plan: STARTER, PROFESSIONAL, ENTERPRISE
        /// </summary>
        [StringLength(50)]
        public string TrialPlan { get; set; } = "STARTER";

        /// <summary>
        /// Status of the signup: Pending, Qualified, Contacted, Converted, Rejected, Expired
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// IP address of the requester
        /// </summary>
        [StringLength(50)]
        public string? IpAddress { get; set; }

        /// <summary>
        /// User agent string
        /// </summary>
        [StringLength(500)]
        public string? UserAgent { get; set; }

        /// <summary>
        /// Locale preference: en, ar
        /// </summary>
        [StringLength(10)]
        public string Locale { get; set; } = "ar";

        /// <summary>
        /// UTM source for marketing attribution
        /// </summary>
        [StringLength(100)]
        public string? UtmSource { get; set; }

        /// <summary>
        /// UTM medium for marketing attribution
        /// </summary>
        [StringLength(100)]
        public string? UtmMedium { get; set; }

        /// <summary>
        /// UTM campaign for marketing attribution
        /// </summary>
        [StringLength(100)]
        public string? UtmCampaign { get; set; }

        /// <summary>
        /// Referring URL
        /// </summary>
        [StringLength(500)]
        public string? ReferrerUrl { get; set; }

        /// <summary>
        /// Landing page URL where signup occurred
        /// </summary>
        [StringLength(500)]
        public string? LandingPageUrl { get; set; }

        /// <summary>
        /// Notes from sales/support team
        /// </summary>
        [StringLength(1000)]
        public string? Notes { get; set; }

        /// <summary>
        /// Date when lead was contacted
        /// </summary>
        public DateTime? ContactedAt { get; set; }

        /// <summary>
        /// Date when lead was converted to TrialRequest
        /// </summary>
        public DateTime? ConvertedAt { get; set; }

        /// <summary>
        /// Associated TrialRequest ID if converted
        /// </summary>
        public Guid? TrialRequestId { get; set; }

        // Navigation
        public virtual TrialRequest? TrialRequest { get; set; }
    }
}
