using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GrcMvc.Models.Entities;

namespace GrcMvc.Models.ViewModels
{
    /// <summary>
    /// View model for platform admin dashboard
    /// </summary>
    public class PlatformAdminDashboardViewModel
    {
        public string AdminName { get; set; }
        public string AdminLevel { get; set; }
        
        // Statistics
        public int TotalTenants { get; set; }
        public int ActiveTenants { get; set; }
        public int TrialTenants { get; set; }
        public int SuspendedTenants { get; set; }
        public int TenantsCreatedToday { get; set; }
        public int TenantsCreatedThisMonth { get; set; }
        
        // Recent tenants
        public List<TenantSummaryViewModel> RecentTenants { get; set; } = new();
        
        // Permissions
        public bool CanCreateTenants { get; set; }
        public bool CanManageTenants { get; set; }
        public bool CanDeleteTenants { get; set; }
    }

    /// <summary>
    /// Summary view of a tenant for lists
    /// </summary>
    public class TenantSummaryViewModel
    {
        public Guid Id { get; set; }
        public string TenantSlug { get; set; }
        public string OrganizationName { get; set; }
        public string Status { get; set; }
        public string SubscriptionTier { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>
    /// View model for tenant list page
    /// </summary>
    public class TenantListPageViewModel
    {
        public List<TenantListViewModel> Tenants { get; set; } = new();
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public string SearchTerm { get; set; }
        public string StatusFilter { get; set; }
    }

    /// <summary>
    /// Individual tenant in list
    /// </summary>
    public class TenantListViewModel
    {
        public Guid Id { get; set; }
        public string TenantSlug { get; set; }
        public string TenantCode { get; set; }
        public string OrganizationName { get; set; }
        public string Industry { get; set; }
        public string Status { get; set; }
        public string SubscriptionTier { get; set; }
        public string AdminEmail { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsTrial { get; set; }
        public DateTime? TrialEndsAt { get; set; }
    }

    /// <summary>
    /// Create tenant form model
    /// </summary>
    public class CreateTenantFormViewModel
    {
        [Required(ErrorMessage = "Organization name is required")]
        [Display(Name = "Organization Name")]
        [StringLength(200, MinimumLength = 3)]
        public string OrganizationName { get; set; }

        [Required(ErrorMessage = "Tenant slug is required")]
        [Display(Name = "Tenant Slug (URL identifier)")]
        [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Slug can only contain lowercase letters, numbers, and hyphens")]
        [StringLength(50, MinimumLength = 3)]
        public string TenantSlug { get; set; }

        [Display(Name = "Tenant Code")]
        [StringLength(10, MinimumLength = 2)]
        [RegularExpression(@"^[A-Z0-9]+$", ErrorMessage = "Code must be uppercase letters and numbers only")]
        public string TenantCode { get; set; }

        [Required(ErrorMessage = "Industry is required")]
        [Display(Name = "Industry")]
        public string Industry { get; set; }

        [Display(Name = "Company Website")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        public string CompanyWebsite { get; set; }

        [Required]
        [Display(Name = "Company Size")]
        public string CompanySize { get; set; }

        [Display(Name = "Sub-Industry")]
        public string SubIndustry { get; set; }

        // Location
        [Display(Name = "Country")]
        public string Country { get; set; } = "Saudi Arabia";

        [Display(Name = "State/Province")]
        public string State { get; set; }

        [Display(Name = "City")]
        public string City { get; set; }

        [Display(Name = "Address")]
        public string Address { get; set; }

        // Business Info
        [Display(Name = "Business Registration Number")]
        public string BusinessRegistrationNumber { get; set; }

        [Display(Name = "Tax ID")]
        public string TaxId { get; set; }

        // Admin User Details
        [Required(ErrorMessage = "Admin first name is required")]
        [Display(Name = "Admin First Name")]
        [StringLength(100)]
        public string AdminFirstName { get; set; }

        [Required(ErrorMessage = "Admin last name is required")]
        [Display(Name = "Admin Last Name")]
        [StringLength(100)]
        public string AdminLastName { get; set; }

        [Required(ErrorMessage = "Admin email is required")]
        [Display(Name = "Admin Email")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string AdminEmail { get; set; }

        [Display(Name = "Admin Phone")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        public string AdminPhone { get; set; }

        [Display(Name = "Job Title")]
        public string AdminJobTitle { get; set; }

        [Display(Name = "Department")]
        public string AdminDepartment { get; set; }

        // Subscription
        [Required]
        [Display(Name = "Subscription Tier")]
        public string SubscriptionTier { get; set; }

        [Required]
        [Display(Name = "Number of Licenses")]
        [Range(1, 10000)]
        public int LicenseCount { get; set; } = 10;

        [Display(Name = "Subscription End Date")]
        [DataType(DataType.Date)]
        public DateTime? SubscriptionEndDate { get; set; }

        [Display(Name = "Bypass Payment")]
        public bool BypassPayment { get; set; } = true;

        // For dropdown lists
        public List<string> AvailableIndustries { get; set; } = new()
        {
            "Financial Services",
            "Healthcare",
            "Government",
            "Technology",
            "Manufacturing",
            "Retail",
            "Education",
            "Energy",
            "Construction",
            "Other"
        };

        public List<string> AvailableTiers { get; set; } = new()
        {
            "MVP",
            "Professional",
            "Enterprise"
        };

        public List<string> AvailableCompanySizes { get; set; } = new()
        {
            "1-50",
            "51-200",
            "201-500",
            "501-1000",
            "1000-5000",
            "5000-10000",
            "10000+"
        };
    }

    /// <summary>
    /// Platform tenant details view model
    /// </summary>
    public class PlatformTenantDetailsViewModel
    {
        public Models.Entities.Tenant Tenant { get; set; }
        public ApplicationUser AdminUser { get; set; }
        public Models.Entities.Workspace Workspace { get; set; }
        public string LoginUrl { get; set; }
        
        // Computed properties
        public int DaysActive => Tenant?.CreatedDate != null ? 
            (int)(DateTime.UtcNow - Tenant.CreatedDate).TotalDays : 0;
        
        public int DaysUntilExpiry => Tenant?.SubscriptionEndDate != null ? 
            (int)(Tenant.SubscriptionEndDate.Value - DateTime.UtcNow).TotalDays : 0;
    }

    /// <summary>
    /// Platform reset password view model
    /// </summary>
    public class PlatformResetPasswordViewModel
    {
        public Guid TenantId { get; set; }
        public string TenantName { get; set; }
        public string AdminEmail { get; set; }
        
        [Display(Name = "Send email notification")]
        public bool SendEmail { get; set; } = true;
        
        [Display(Name = "Force password change on next login")]
        public bool ForcePasswordChange { get; set; } = true;
        
        [Display(Name = "Password expiry (hours)")]
        [Range(1, 168)]
        public int ExpiryHours { get; set; } = 72;
    }
}
