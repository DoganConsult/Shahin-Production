using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Workspace represents a sub-scope within a Tenant for multi-market/multi-BU operations.
    /// Examples: KSA Market, UAE Market, Retail Banking BU, Corporate Banking BU
    /// 
    /// Hierarchy: Tenant (Organization) → Workspace (Market/BU/Entity) → Teams/Controls/Evidence
    /// </summary>
    public class Workspace : BaseEntity
    {
        [Required]
        public Guid TenantId { get; set; }

        [ForeignKey("TenantId")]
        public virtual Tenant Tenant { get; set; } = null!;

        /// <summary>
        /// Unique code within tenant (e.g., "KSA", "UAE", "RETAIL", "CORPORATE")
        /// </summary>
        [Required]
        [StringLength(50)]
        public string WorkspaceCode { get; set; } = string.Empty;

        /// <summary>
        /// Display name
        /// </summary>
        [Required]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Arabic name
        /// </summary>
        [StringLength(255)]
        public string? NameAr { get; set; }

        /// <summary>
        /// Workspace type: Market, BusinessUnit, Entity, Environment
        /// </summary>
        [Required]
        [StringLength(50)]
        public string WorkspaceType { get; set; } = "Market"; // Market, BusinessUnit, Entity, Environment

        /// <summary>
        /// Jurisdiction/country code (for market-type workspaces)
        /// </summary>
        [StringLength(10)]
        public string? JurisdictionCode { get; set; } // ISO 3166-1 alpha-2

        /// <summary>
        /// Default language for this workspace
        /// </summary>
        [StringLength(10)]
        public string DefaultLanguage { get; set; } = "ar";

        /// <summary>
        /// Timezone for this workspace
        /// </summary>
        [StringLength(50)]
        public string? Timezone { get; set; }

        /// <summary>
        /// Description/purpose of this workspace
        /// </summary>
        [StringLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Whether this is the default workspace for new users
        /// </summary>
        public bool IsDefault { get; set; } = false;

        /// <summary>
        /// Workspace-specific regulators (JSON array)
        /// </summary>
        public string? RegulatorsJson { get; set; }

        /// <summary>
        /// Workspace-specific framework overlays (JSON array of overlay codes)
        /// </summary>
        public string? OverlaysJson { get; set; }

        /// <summary>
        /// Workspace-specific configuration (JSON object)
        /// </summary>
        public string? ConfigJson { get; set; }

        /// <summary>
        /// Status: Active, Inactive, Suspended
        /// </summary>
        [StringLength(20)]
        public string Status { get; set; } = "Active";

        // Navigation properties
        public virtual ICollection<WorkspaceMembership> Memberships { get; set; } = new List<WorkspaceMembership>();
    }

    /// <summary>
    /// Links users to workspaces within a tenant with specific roles
    /// A user can belong to multiple workspaces within the same tenant
    /// </summary>
    public class WorkspaceMembership : BaseEntity
    {
        [Required]
        public Guid TenantId { get; set; }

        [Required]
        public Guid WorkspaceId { get; set; }

        [ForeignKey("WorkspaceId")]
        public virtual Workspace Workspace { get; set; } = null!;

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        /// <summary>
        /// Roles within this workspace (JSON array of role codes)
        /// e.g., ["COMPLIANCE_OFFICER", "APPROVER"]
        /// </summary>
        public string? WorkspaceRolesJson { get; set; }

        /// <summary>
        /// Is this the user's primary workspace?
        /// </summary>
        public bool IsPrimary { get; set; } = false;

        /// <summary>
        /// Is this user a workspace admin?
        /// </summary>
        public bool IsWorkspaceAdmin { get; set; } = false;

        /// <summary>
        /// Membership status: Active, Invited, Disabled
        /// </summary>
        [StringLength(20)]
        public string Status { get; set; } = "Active";

        /// <summary>
        /// Date when user was added to workspace
        /// </summary>
        public DateTime JoinedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Last time user accessed this workspace
        /// </summary>
        public DateTime? LastAccessedAt { get; set; }
    }

    /// <summary>
    /// Workspace-scoped control suite - which controls apply to which workspace
    /// </summary>
    public class WorkspaceControl : BaseEntity
    {
        [Required]
        public Guid TenantId { get; set; }

        [Required]
        public Guid WorkspaceId { get; set; }

        [ForeignKey("WorkspaceId")]
        public virtual Workspace Workspace { get; set; } = null!;

        [Required]
        public Guid ControlId { get; set; }

        [ForeignKey("ControlId")]
        public virtual Control Control { get; set; } = null!;

        /// <summary>
        /// Status: Active, Inactive, Inherited
        /// </summary>
        [StringLength(20)]
        public string Status { get; set; } = "Active";

        /// <summary>
        /// Frequency override for this workspace (if different from default)
        /// </summary>
        [StringLength(20)]
        public string? FrequencyOverride { get; set; }

        /// <summary>
        /// SLA days override for this workspace
        /// </summary>
        public int? SlaDaysOverride { get; set; }

        /// <summary>
        /// Which overlay applied this control
        /// </summary>
        [StringLength(100)]
        public string? OverlaySource { get; set; }

        /// <summary>
        /// Owner team for this control in this workspace
        /// </summary>
        public Guid? OwnerTeamId { get; set; }

        /// <summary>
        /// Owner user for this control in this workspace
        /// </summary>
        public string? OwnerUserId { get; set; }
    }

    /// <summary>
    /// Workspace-scoped approval gate configuration
    /// </summary>
    public class WorkspaceApprovalGate : BaseEntity
    {
        [Required]
        public Guid TenantId { get; set; }

        [Required]
        public Guid WorkspaceId { get; set; }

        [ForeignKey("WorkspaceId")]
        public virtual Workspace Workspace { get; set; } = null!;

        /// <summary>
        /// Gate code (e.g., "EXCEPTION_APPROVAL", "POLICY_PUBLISH")
        /// </summary>
        [Required]
        [StringLength(100)]
        public string GateCode { get; set; } = string.Empty;

        /// <summary>
        /// Gate name
        /// </summary>
        [Required]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;

        [StringLength(255)]
        public string? NameAr { get; set; }

        /// <summary>
        /// Scope type: Global, ControlFamily, System, Process
        /// </summary>
        [StringLength(50)]
        public string ScopeType { get; set; } = "Global";

        /// <summary>
        /// Specific scope value (control family code, system ID, etc.)
        /// </summary>
        [StringLength(255)]
        public string? ScopeValue { get; set; }

        /// <summary>
        /// Minimum number of approvals required
        /// </summary>
        public int MinApprovals { get; set; } = 1;

        /// <summary>
        /// SLA in days for approval
        /// </summary>
        public int SlaDays { get; set; } = 3;

        /// <summary>
        /// Escalation after X days overdue
        /// </summary>
        public int EscalationDays { get; set; } = 2;

        public bool IsActive { get; set; } = true;

        // Navigation
        public virtual ICollection<WorkspaceApprovalGateApprover> Approvers { get; set; } 
            = new List<WorkspaceApprovalGateApprover>();
    }

    /// <summary>
    /// Approvers for a workspace approval gate
    /// </summary>
    public class WorkspaceApprovalGateApprover : BaseEntity
    {
        [Required]
        public Guid TenantId { get; set; }

        [Required]
        public Guid WorkspaceId { get; set; }

        [Required]
        public Guid GateId { get; set; }

        [ForeignKey("GateId")]
        public virtual WorkspaceApprovalGate Gate { get; set; } = null!;

        /// <summary>
        /// Approver type: User, Role, Team
        /// </summary>
        [Required]
        [StringLength(20)]
        public string ApproverType { get; set; } = "Role"; // User, Role, Team

        /// <summary>
        /// Approver reference (user ID, role code, or team ID)
        /// </summary>
        [Required]
        [StringLength(255)]
        public string ApproverReference { get; set; } = string.Empty;

        /// <summary>
        /// Order in approval chain (for sequential approvals)
        /// </summary>
        public int ApprovalOrder { get; set; } = 0;

        /// <summary>
        /// Is this a mandatory approver?
        /// </summary>
        public bool IsMandatory { get; set; } = false;
    }
}
