using System;
using System.ComponentModel.DataAnnotations;

namespace GrcMvc.Models.DTOs
{
    /// <summary>
    /// DTO for creating a tenant with full features (bypass payment)
    /// </summary>
    public class CreateOwnerTenantDto
    {
        [Required(ErrorMessage = "Organization name is required")]
        [Display(Name = "Organization Name")]
        public string OrganizationName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Admin email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Admin Email")]
        public string AdminEmail { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Tenant slug is required")]
        [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Tenant slug can only contain lowercase letters, numbers, and hyphens")]
        [Display(Name = "Tenant Slug")]
        public string TenantSlug { get; set; } = string.Empty;
        
        [Range(7, 90, ErrorMessage = "Expiration days must be between 7 and 90")]
        [Display(Name = "Credential Expiration (Days)")]
        public int ExpirationDays { get; set; } = 14;
    }

    /// <summary>
    /// DTO for tenant admin credentials (shown only once)
    /// </summary>
    public class TenantAdminCredentialsDto
    {
        public Guid TenantId { get; set; }
        public string TenantSlug { get; set; } = string.Empty;
        public string OrganizationName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty; // Only shown once
        public DateTime ExpiresAt { get; set; }
        public string LoginUrl { get; set; } = string.Empty;
        public bool MustChangePasswordOnFirstLogin { get; set; } = true;
    }

    /// <summary>
    /// DTO for generating tenant admin account
    /// </summary>
    public class GenerateAdminAccountDto
    {
        [Range(7, 90, ErrorMessage = "Expiration days must be between 7 and 90")]
        [Display(Name = "Credential Expiration (Days)")]
        public int ExpirationDays { get; set; } = 14;
    }

    /// <summary>
    /// DTO for extending credential expiration
    /// </summary>
    public class ExtendExpirationDto
    {
        [Range(1, 90, ErrorMessage = "Additional days must be between 1 and 90")]
        [Display(Name = "Additional Days")]
        public int AdditionalDays { get; set; } = 14;
    }

    /// <summary>
    /// DTO for credential delivery
    /// </summary>
    public class DeliverCredentialsDto
    {
        [Required(ErrorMessage = "Delivery method is required")]
        [Display(Name = "Delivery Method")]
        public string DeliveryMethod { get; set; } = "Manual"; // Email, Manual, Download
        
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Recipient Email (for email delivery)")]
        public string? RecipientEmail { get; set; }
    }
}
