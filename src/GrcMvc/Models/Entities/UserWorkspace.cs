using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// User workspace - Pre-mapped dashboard and tasks based on user's role
    /// Each team member gets a personalized workspace after onboarding
    /// </summary>
    public class UserWorkspace : BaseEntity
    {
        [Required]
        public Guid TenantId { get; set; }

        [ForeignKey("TenantId")]
        public virtual Tenant Tenant { get; set; } = null!;

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        /// <summary>
        /// Role code from RoleCatalog (e.g., COMPLIANCE_OFFICER, CONTROL_OWNER, RISK_MANAGER)
        /// </summary>
        [Required]
        [StringLength(100)]
        public string RoleCode { get; set; } = string.Empty;

        /// <summary>
        /// Role display name
        /// </summary>
        [StringLength(255)]
        public string RoleName { get; set; } = string.Empty;

        /// <summary>
        /// Arabic role name
        /// </summary>
        [StringLength(255)]
        public string? RoleNameAr { get; set; }

        /// <summary>
        /// Workspace configuration as JSON
        /// Contains: dashboard widgets, default views, quick actions, notifications preferences
        /// </summary>
        public string? WorkspaceConfigJson { get; set; }

        /// <summary>
        /// Assigned frameworks (comma-separated codes)
        /// </summary>
        [StringLength(500)]
        public string? AssignedFrameworks { get; set; }

        /// <summary>
        /// Assigned assessment IDs (for direct access)
        /// </summary>
        public string? AssignedAssessmentIds { get; set; }

        /// <summary>
        /// Default landing page after login
        /// </summary>
        [StringLength(255)]
        public string DefaultLandingPage { get; set; } = "/Dashboard";

        /// <summary>
        /// Quick actions available to this user based on role
        /// </summary>
        public string? QuickActionsJson { get; set; }

        /// <summary>
        /// Dashboard widgets configuration
        /// </summary>
        public string? DashboardWidgetsJson { get; set; }

        /// <summary>
        /// Whether workspace is fully configured
        /// </summary>
        public bool IsConfigured { get; set; } = false;

        /// <summary>
        /// Last time user accessed their workspace
        /// </summary>
        public DateTime? LastAccessedAt { get; set; }

        // Navigation
        public virtual ICollection<UserWorkspaceTask> Tasks { get; set; } = new List<UserWorkspaceTask>();
    }

    /// <summary>
    /// Pre-assigned tasks for a user's workspace based on their role
    /// </summary>
    public class UserWorkspaceTask : BaseEntity
    {
        [Required]
        public Guid WorkspaceId { get; set; }

        [ForeignKey("WorkspaceId")]
        public virtual UserWorkspace Workspace { get; set; } = null!;

        [Required]
        public Guid TenantId { get; set; }

        /// <summary>
        /// Task title
        /// </summary>
        [Required]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Arabic title
        /// </summary>
        [StringLength(255)]
        public string? TitleAr { get; set; }

        /// <summary>
        /// Task description
        /// </summary>
        [StringLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Task type: Assessment, Evidence, Approval, Review, Remediation
        /// </summary>
        [Required]
        [StringLength(50)]
        public string TaskType { get; set; } = "Assessment";

        /// <summary>
        /// Reference to related entity (Assessment, Control, Evidence, etc.)
        /// </summary>
        public Guid? RelatedEntityId { get; set; }

        /// <summary>
        /// Type of related entity
        /// </summary>
        [StringLength(50)]
        public string? RelatedEntityType { get; set; }

        /// <summary>
        /// URL to navigate to for this task
        /// </summary>
        [StringLength(500)]
        public string? ActionUrl { get; set; }

        /// <summary>
        /// Priority: 1=High, 2=Medium, 3=Low
        /// </summary>
        public int Priority { get; set; } = 2;

        /// <summary>
        /// Due date
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Status: Pending, InProgress, Completed, Overdue
        /// </summary>
        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// Framework code this task belongs to
        /// </summary>
        [StringLength(50)]
        public string? FrameworkCode { get; set; }

        /// <summary>
        /// Estimated effort in hours
        /// </summary>
        public int? EstimatedHours { get; set; }

        /// <summary>
        /// Order for display
        /// </summary>
        public int DisplayOrder { get; set; } = 0;
    }

    /// <summary>
    /// Role-based workspace template - defines default configuration per role
    /// </summary>
    public class WorkspaceTemplate : BaseEntity
    {
        /// <summary>
        /// Role code this template applies to
        /// </summary>
        [Required]
        [StringLength(100)]
        public string RoleCode { get; set; } = string.Empty;

        /// <summary>
        /// Template name
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
        /// Description of this workspace template
        /// </summary>
        [StringLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Default landing page for this role
        /// </summary>
        [StringLength(255)]
        public string DefaultLandingPage { get; set; } = "/Dashboard";

        /// <summary>
        /// Dashboard widgets configuration (JSON)
        /// </summary>
        public string? DashboardWidgetsJson { get; set; }

        /// <summary>
        /// Quick actions for this role (JSON)
        /// </summary>
        public string? QuickActionsJson { get; set; }

        /// <summary>
        /// Menu items visible to this role (JSON)
        /// </summary>
        public string? MenuItemsJson { get; set; }

        /// <summary>
        /// Task types this role can be assigned
        /// </summary>
        [StringLength(500)]
        public string? AssignableTaskTypes { get; set; }

        /// <summary>
        /// Whether this is the default template for the role
        /// </summary>
        public bool IsDefault { get; set; } = true;

        /// <summary>
        /// Whether template is active
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
