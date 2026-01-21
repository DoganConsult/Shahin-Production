using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Team - Groups of users within an organization, scoped to a workspace.
    /// Core principle: Everything keyed to TenantId + WorkspaceId (org + market/BU scope)
    /// 
    /// Example: "KSA Security Team" vs "UAE Security Team" are different teams in different workspaces.
    /// </summary>
    public class Team : BaseEntity
    {
        [Required]
        public Guid TenantId { get; set; }

        /// <summary>
        /// Workspace this team belongs to (Market/BU scope).
        /// Null = shared team across all workspaces (rare, e.g., "Group IT Ops")
        /// </summary>
        public Guid? WorkspaceId { get; set; }

        [ForeignKey("WorkspaceId")]
        public virtual Workspace? Workspace { get; set; }

        [StringLength(50)]
        public string TeamCode { get; set; } = string.Empty; // TEAM-001, IT-OPS, SEC-OPS

        [StringLength(255)]
        public string Name { get; set; } = string.Empty;

        [StringLength(255)]
        public string NameAr { get; set; } = string.Empty;

        [StringLength(500)]
        public string Purpose { get; set; } = string.Empty; // "Security Operations", "Compliance", "Risk Management"

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Team type: Operational, Governance, Project, External, SharedServices
        /// </summary>
        [StringLength(50)]
        public string TeamType { get; set; } = "Operational";

        /// <summary>
        /// Optional: Business unit or department this team belongs to
        /// </summary>
        [StringLength(255)]
        public string BusinessUnit { get; set; } = string.Empty;

        /// <summary>
        /// Manager user ID
        /// </summary>
        public Guid? ManagerUserId { get; set; }

        /// <summary>
        /// Is this the workspace's default fallback team for unassigned work?
        /// </summary>
        public bool IsDefaultFallback { get; set; } = false;

        /// <summary>
        /// Is this a shared team across all workspaces in the tenant?
        /// </summary>
        public bool IsSharedTeam { get; set; } = false;

        public bool IsActive { get; set; } = true;

        // Navigation
        public virtual Tenant Tenant { get; set; } = null!;
        public virtual ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();
        public virtual ICollection<RACIAssignment> RACIAssignments { get; set; } = new List<RACIAssignment>();
    }

    /// <summary>
    /// TeamMember - Links users to teams with specific roles, workspace-scoped.
    /// Enables: "send approval to role=Approver for team=Security in workspace=KSA"
    /// </summary>
    public class TeamMember : BaseEntity
    {
        [Required]
        public Guid TenantId { get; set; }

        /// <summary>
        /// Workspace this team membership belongs to (inherited from Team).
        /// Denormalized for query performance.
        /// </summary>
        public Guid? WorkspaceId { get; set; }

        [Required]
        public Guid TeamId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// Role within this team (from RoleProfile catalog)
        /// </summary>
        [Required]
        [StringLength(100)]
        public string RoleCode { get; set; } = string.Empty; // CONTROL_OWNER, APPROVER, ASSESSOR, EVIDENCE_CUSTODIAN

        /// <summary>
        /// Is primary contact for this role in the team?
        /// </summary>
        public bool IsPrimaryForRole { get; set; } = false;

        /// <summary>
        /// Can approve on behalf of team?
        /// </summary>
        public bool CanApprove { get; set; } = false;

        /// <summary>
        /// Delegation: Can delegate tasks to others?
        /// </summary>
        public bool CanDelegate { get; set; } = false;

        public DateTime JoinedDate { get; set; } = DateTime.UtcNow;
        public DateTime? LeftDate { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation
        public virtual Team Team { get; set; } = null!;
        public virtual TenantUser User { get; set; } = null!;
        
        [ForeignKey("WorkspaceId")]
        public virtual Workspace? Workspace { get; set; }
    }

    /// <summary>
    /// RACI Assignment - Maps Responsible/Accountable/Consulted/Informed to teams by scope.
    /// Workspace-scoped: different markets can have different RACI assignments.
    /// Enables: "assign evidence task to ControlOwner for control_family=IAM in workspace=KSA"
    /// </summary>
    public class RACIAssignment : BaseEntity
    {
        [Required]
        public Guid TenantId { get; set; }

        /// <summary>
        /// Workspace this RACI applies to.
        /// Null = applies to all workspaces in the tenant (global RACI).
        /// </summary>
        public Guid? WorkspaceId { get; set; }

        [ForeignKey("WorkspaceId")]
        public virtual Workspace? Workspace { get; set; }

        /// <summary>
        /// Scope type: ControlFamily, System, BusinessUnit, Framework, Assessment, Requirement
        /// </summary>
        [Required]
        [StringLength(50)]
        public string ScopeType { get; set; } = string.Empty;

        /// <summary>
        /// Scope identifier (e.g., "IAM", "Payments Systems", "NCA-ECC")
        /// </summary>
        [Required]
        [StringLength(255)]
        public string ScopeId { get; set; } = string.Empty;

        /// <summary>
        /// Team assigned to this scope
        /// </summary>
        [Required]
        public Guid TeamId { get; set; }

        /// <summary>
        /// RACI type: R (Responsible), A (Accountable), C (Consulted), I (Informed)
        /// </summary>
        [Required]
        [StringLength(1)]
        public string RACI { get; set; } = "R";

        /// <summary>
        /// Optional: Specific role within the team
        /// </summary>
        [StringLength(100)]
        public string? RoleCode { get; set; }

        /// <summary>
        /// Priority order when multiple assignments exist
        /// </summary>
        public int Priority { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        // Navigation
        public virtual Team Team { get; set; } = null!;
    }
}
