using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.TenantManagement;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Represents a tenant (organization) in the multi-tenant GRC platform.
    /// Extends ABP's Tenant entity with GRC-specific business properties.
    /// 
    /// Migration completed: Now inherits from ABP TenantManagement.Tenant
    /// This enables ITenantAppService usage while preserving all custom properties.
    /// </summary>
    public class Tenant : Volo.Abp.TenantManagement.Tenant
    {
        // Add properties that were in BaseEntity but missing in ABP Tenant
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
        
        // Add additional properties from BaseEntity
        public string? ModifiedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? Labels { get; set; }
        
        // Alias properties for backward compatibility with views
        [NotMapped]
        public DateTime CreatedAt
        {
            get => CreatedDate;
            set => CreatedDate = value;
        }
        
        [NotMapped]
        public DateTime? ModifiedDate
        {
            get => UpdatedDate;
            set => UpdatedDate = value;
        }
        
        public string TenantSlug { get; set; } = string.Empty;
        public string OrganizationName { get; set; } = string.Empty;
        public string AdminEmail { get; set; } = string.Empty;

        /// <summary>
        /// Industry/Sector of the organization (e.g., Financial Services, Healthcare, Government)
        /// Captured during self-registration (Issue #4)
        /// </summary>
        public string? Industry { get; set; }

        /// <summary>
        /// Primary contact email for the tenant (used for trial signups)
        /// </summary>
        public string? Email { get; set; }
        
        /// <summary>
        /// Stripe customer ID for payment integration
        /// </summary>
        public string? StripeCustomerId { get; set; }

        /// <summary>
        /// Immutable tenant code used as prefix for all business reference codes.
        /// Format: 2-10 uppercase alphanumeric characters (e.g., ACME, STC, NCA).
        /// This is DIFFERENT from TenantSlug - TenantCode is for auditing, TenantSlug is for URLs.
        /// Once assigned, TenantCode should NEVER change.
        /// </summary>
        public string TenantCode { get; set; } = string.Empty;

        /// <summary>
        /// Business reference code for this tenant.
        /// Format: {TENANTCODE}-TEN-{YYYY}-{SEQUENCE}
        /// Example: ACME-TEN-2026-000001
        /// </summary>
        public string BusinessCode { get; set; } = string.Empty;

        /// <summary>
        /// Status: Pending (awaiting admin activation), Active, Suspended, Deleted
        /// </summary>
        public string Status { get; set; } = "Pending";
        public bool IsActive { get; set; } = true; // Quick flag for active/inactive state

        public string ActivationToken { get; set; } = string.Empty;
        public DateTime? ActivatedAt { get; set; }
        public string ActivatedBy { get; set; } = string.Empty;

        public DateTime SubscriptionStartDate { get; set; } = DateTime.UtcNow;
        public DateTime? SubscriptionEndDate { get; set; }
        public string SubscriptionTier { get; set; } = "MVP"; // MVP, Professional, Enterprise

        /// <summary>
        /// Correlation ID for audit trail and event tracking
        /// </summary>
        public string CorrelationId { get; set; } = string.Empty;

        /// <summary>
        /// Owner-created tenant tracking
        /// </summary>
        public string? CreatedByOwnerId { get; set; } // ApplicationUser.Id of owner who created this tenant (string from Identity)
        public bool IsOwnerCreated { get; set; } = false; // Flag for owner-created tenants
        public bool BypassPayment { get; set; } = false; // Indicates payment bypass
        public DateTime? CredentialExpiresAt { get; set; } // Expiration for admin credentials
        public bool AdminAccountGenerated { get; set; } = false; // Tracks if admin account was generated
        public DateTime? AdminAccountGeneratedAt { get; set; } // When credentials were generated

        // =============================================================================
        // TRIAL EDITION FIELDS
        // =============================================================================

        /// <summary>
        /// Indicates if this is a trial tenant
        /// </summary>
        public bool IsTrial { get; set; } = false;

        /// <summary>
        /// Trial period start date
        /// </summary>
        public DateTime? TrialStartsAt { get; set; }

        /// <summary>
        /// Trial period end date (typically 7 days from start)
        /// </summary>
        public DateTime? TrialEndsAt { get; set; }

        /// <summary>
        /// Billing status: Trialing, Active, Suspended, Expired
        /// </summary>
        public string BillingStatus { get; set; } = "Active";

        // =============================================================================
        // EMAIL VERIFICATION (Phase 1: Self-Registration Security)
        // =============================================================================

        /// <summary>
        /// SHA256 hash of the email verification token (raw token never stored)
        /// </summary>
        public string? EmailVerificationTokenHash { get; set; }

        /// <summary>
        /// When the verification token expires (24 hours from creation)
        /// </summary>
        public DateTime? EmailVerificationTokenExpiresAt { get; set; }

        /// <summary>
        /// Whether the admin email has been verified
        /// </summary>
        public bool IsEmailVerified { get; set; } = false;

        /// <summary>
        /// When the email was verified
        /// </summary>
        public DateTime? EmailVerifiedAt { get; set; }

        /// <summary>
        /// When the last verification email was sent (for rate limiting)
        /// </summary>
        public DateTime? EmailVerificationLastSentAt { get; set; }

        /// <summary>
        /// Number of times verification email has been resent (max 5)
        /// </summary>
        public int EmailVerificationResendCount { get; set; } = 0;

        // =============================================================================
        // ONBOARDING LINKAGE (One workspace per tenant, created during finalization)
        // =============================================================================

        /// <summary>
        /// Default workspace ID - created ONCE during onboarding finalization
        /// </summary>
        public Guid? DefaultWorkspaceId { get; set; }

        /// <summary>
        /// Assessment template ID - auto-generated during onboarding (100Q baseline)
        /// </summary>
        public Guid? AssessmentTemplateId { get; set; }

        /// <summary>
        /// GRC Plan ID - auto-generated during onboarding
        /// </summary>
        public Guid? GrcPlanId { get; set; }

        /// <summary>
        /// Onboarding status: NOT_STARTED, IN_PROGRESS, FAILED, COMPLETED
        /// </summary>
        public string OnboardingStatus { get; set; } = "NOT_STARTED";

        /// <summary>
        /// ID of the first admin user who created this tenant (for targeted onboarding redirects)
        /// </summary>
        public string? FirstAdminUserId { get; set; }

        /// <summary>
        /// When onboarding was started
        /// </summary>
        public DateTime? OnboardingStartedAt { get; set; }

        /// <summary>
        /// When onboarding was completed
        /// </summary>
        public DateTime? OnboardingCompletedAt { get; set; }

        // =============================================================================
        // USAGE TRACKING FIELDS
        // =============================================================================

        /// <summary>
        /// Total storage used by this tenant in bytes
        /// </summary>
        public long? StorageUsedBytes { get; set; } = 0;

        /// <summary>
        /// Current subscription plan code (starter, professional, enterprise)
        /// </summary>
        public string? SubscriptionPlan { get; set; }

        /// <summary>
        /// Data isolation level: Shared, Isolated, Dedicated
        /// </summary>
        public string DataIsolationLevel { get; set; } = "Shared";

        // Navigation properties
        public virtual ICollection<TenantUser> Users { get; set; } = new List<TenantUser>();
        public virtual OrganizationProfile? OrganizationProfile { get; set; }
        public virtual ICollection<Ruleset> Rulesets { get; set; } = new List<Ruleset>();
        public virtual ICollection<TenantBaseline> ApplicableBaselines { get; set; } = new List<TenantBaseline>();
        public virtual ICollection<TenantPackage> ApplicablePackages { get; set; } = new List<TenantPackage>();
        public virtual ICollection<TenantTemplate> ApplicableTemplates { get; set; } = new List<TenantTemplate>();
        public virtual ICollection<Plan> Plans { get; set; } = new List<Plan>();
        public virtual ICollection<AuditEvent> AuditEvents { get; set; } = new List<AuditEvent>();
    }
}
