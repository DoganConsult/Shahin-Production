using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Defines permissions in the system (granular access control)
    /// </summary>
    public class Permission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Code { get; set; } // e.g., "Workflow.View", "Workflow.Create", "Workflow.Approve"

        [Required]
        [StringLength(255)]
        public string Name { get; set; } // e.g., "View Workflows"

        [StringLength(1000)]
        public string Description { get; set; }

        [StringLength(100)]
        public string Category { get; set; } // e.g., "Workflow", "Control", "Risk", "Audit"

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<RolePermission> RolePermissions { get; set; }
        public ICollection<FeaturePermission> FeaturePermissions { get; set; }
    }

    /// <summary>
    /// Defines features visible in the system (UI/functionality control)
    /// </summary>
    public class Feature
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Code { get; set; } // e.g., "Workflows", "Controls", "Risks", "Audits"

        [Required]
        [StringLength(255)]
        public string Name { get; set; } // e.g., "Workflow Management"

        [StringLength(1000)]
        public string Description { get; set; }

        [StringLength(100)]
        public string Category { get; set; } // e.g., "GRC", "Compliance", "Reporting"

        public bool IsActive { get; set; } = true;

        public int? DisplayOrder { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<RoleFeature> RoleFeatures { get; set; }
        public ICollection<FeaturePermission> FeaturePermissions { get; set; }
    }

    /// <summary>
    /// Maps roles to permissions (what a role can DO)
    /// </summary>
    public class RolePermission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string RoleId { get; set; } // FK to AspNetRoles.Id

        public int PermissionId { get; set; }
        [ForeignKey("PermissionId")]
        public Permission Permission { get; set; }

        public Guid TenantId { get; set; }
        [ForeignKey("TenantId")]
        public Tenant Tenant { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        public string AssignedBy { get; set; } // UserId who assigned

        // Navigation
        [ForeignKey("RoleId")]
        public IdentityRole Role { get; set; }
    }

    /// <summary>
    /// Maps roles to features (what a role can SEE)
    /// </summary>
    public class RoleFeature
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string RoleId { get; set; } // FK to AspNetRoles.Id

        public int FeatureId { get; set; }
        [ForeignKey("FeatureId")]
        public Feature Feature { get; set; }

        public Guid TenantId { get; set; }
        [ForeignKey("TenantId")]
        public Tenant Tenant { get; set; }

        public bool IsVisible { get; set; } = true;

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        public string AssignedBy { get; set; } // UserId who assigned

        // Navigation
        [ForeignKey("RoleId")]
        public IdentityRole Role { get; set; }
    }

    /// <summary>
    /// Links features to required permissions (e.g., Workflows feature requires View + Create permissions)
    /// </summary>
    public class FeaturePermission
    {
        [Key]
        public int Id { get; set; }

        public int FeatureId { get; set; }
        [ForeignKey("FeatureId")]
        public Feature Feature { get; set; }

        public int PermissionId { get; set; }
        [ForeignKey("PermissionId")]
        public Permission Permission { get; set; }

        public bool IsRequired { get; set; } = true; // Is this permission required to access the feature?

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Per-tenant role configuration (customizable per tenant)
    /// </summary>
    public class TenantRoleConfiguration
    {
        [Key]
        public int Id { get; set; }

        public Guid TenantId { get; set; }
        [ForeignKey("TenantId")]
        public Tenant Tenant { get; set; }

        [Required]
        public string RoleId { get; set; } // FK to AspNetRoles.Id
        [ForeignKey("RoleId")]
        public IdentityRole Role { get; set; }

        public string Description { get; set; }

        public int? MaxUsersWithRole { get; set; } // Limit users per role (null = unlimited)

        public bool CanBeModified { get; set; } = true; // Can tenant admin modify this role?

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<RolePermission> Permissions { get; set; }
        public ICollection<RoleFeature> Features { get; set; }
    }

    /// <summary>
    /// User role and feature visibility per tenant
    /// </summary>
    public class UserRoleAssignment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } // FK to AspNetUsers.Id
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        public Guid TenantId { get; set; }
        [ForeignKey("TenantId")]
        public Tenant Tenant { get; set; }

        [Required]
        public string RoleId { get; set; } // FK to AspNetRoles.Id
        [ForeignKey("RoleId")]
        public IdentityRole Role { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? ExpiresAt { get; set; } // Optional expiration

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        public string AssignedBy { get; set; } // UserId who assigned
    }
}
