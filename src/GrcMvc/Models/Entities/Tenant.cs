using System;
using System.Collections.Generic;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Represents a tenant (organization) in the multi-tenant GRC platform.
    /// Layer 2: Tenant Context
    /// </summary>
    public class Tenant : BaseEntity
    {
        public string TenantSlug { get; set; } = string.Empty;
        public string OrganizationName { get; set; } = string.Empty;
        public string AdminEmail { get; set; } = string.Empty;

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
        /// When onboarding was completed
        /// </summary>
        public DateTime? OnboardingCompletedAt { get; set; }

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
