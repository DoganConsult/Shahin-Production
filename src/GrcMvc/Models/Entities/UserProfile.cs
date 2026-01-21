using System;
using System.Collections.Generic;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// User Profile entity - defines role-based profiles with permissions
    /// Maps to the 14 predefined GRC profiles
    /// </summary>
    public class UserProfile : BaseEntity
    {
        public string ProfileCode { get; set; } = string.Empty;
        public string ProfileName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // Admin, Compliance, Risk, Audit, etc.
        public int DisplayOrder { get; set; }
        public bool IsSystemProfile { get; set; } = true; // System profiles cannot be deleted
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// JSON array of permission codes
        /// </summary>
        public string PermissionsJson { get; set; } = "[]";

        /// <summary>
        /// JSON array of workflow role codes this profile can perform
        /// </summary>
        public string WorkflowRolesJson { get; set; } = "[]";

        /// <summary>
        /// JSON object for UI access configuration
        /// </summary>
        public string UiAccessJson { get; set; } = "{}";

        // Navigation
        public virtual ICollection<UserProfileAssignment> Assignments { get; set; } = new List<UserProfileAssignment>();
    }

    /// <summary>
    /// Links users to profiles within a tenant
    /// </summary>
    public class UserProfileAssignment : BaseEntity
    {
        public Guid TenantId { get; set; }
        public string UserId { get; set; } = string.Empty; // AspNetUsers.Id
        public Guid UserProfileId { get; set; }
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public string AssignedBy { get; set; } = string.Empty;
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation
        public virtual Tenant Tenant { get; set; } = null!;
        public virtual UserProfile UserProfile { get; set; } = null!;
    }
}
