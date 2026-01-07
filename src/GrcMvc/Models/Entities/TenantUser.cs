using System;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Links an ApplicationUser to a Tenant with a specific Role and Title.
    /// Enables multi-tenant user assignments.
    /// </summary>
    public class TenantUser : BaseEntity
    {
        public Guid TenantId { get; set; }
        public string UserId { get; set; } = string.Empty; // Foreign key to AspNetUsers
        
        /// <summary>
        /// RoleCode from Catalog (e.g., COMPLIANCE_OFFICER, SECURITY_LEAD)
        /// Not an individual permission; used for workflow task assignment
        /// </summary>
        public string RoleCode { get; set; } = string.Empty;
        
        /// <summary>
        /// TitleCode from Catalog (e.g., COMPLIANCE_OFFICER_MANAGER, COMPLIANCE_OFFICER_ANALYST)
        /// Allows role specialization without creating new roles
        /// </summary>
        public string TitleCode { get; set; } = string.Empty;
        
        /// <summary>
        /// Status: Pending (invited, not activated), Active, Suspended, Inactive
        /// </summary>
        public string Status { get; set; } = "Pending";
        
        public string InvitationToken { get; set; } = string.Empty;
        public DateTime? InvitedAt { get; set; }
        public DateTime? ActivatedAt { get; set; }
        public string InvitedBy { get; set; } = string.Empty;
        
        /// <summary>
        /// Owner-generated account tracking
        /// </summary>
        public bool IsOwnerGenerated { get; set; } = false; // Marks owner-generated admin accounts
        public string? GeneratedByOwnerId { get; set; } // Which owner (ApplicationUser.Id) generated this account (string from Identity)
        public DateTime? CredentialExpiresAt { get; set; } // Expiration for this specific account
        public bool MustChangePasswordOnFirstLogin { get; set; } = false; // Security requirement (default: true for owner-generated)
        
        // Navigation properties
        public virtual Tenant Tenant { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
