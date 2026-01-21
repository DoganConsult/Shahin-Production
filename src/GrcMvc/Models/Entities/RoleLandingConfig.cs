using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Role-based landing configuration for shared tenant workspace.
    /// Defines dashboards, widgets, quick actions, and navigation per role.
    /// Users don't get their own workspace - they get role-based views of the shared workspace.
    /// </summary>
    public class RoleLandingConfig : BaseEntity
    {
        [Required]
        public new Guid TenantId { get; set; }

        [ForeignKey("TenantId")]
        public virtual Tenant Tenant { get; set; } = null!;

        [Required]
        public Guid WorkspaceId { get; set; }

        [ForeignKey("WorkspaceId")]
        public virtual Workspace Workspace { get; set; } = null!;

        /// <summary>
        /// Role code this config applies to (e.g., COMPLIANCE_OFFICER, RISK_MANAGER)
        /// </summary>
        [Required]
        [StringLength(100)]
        public string RoleCode { get; set; } = string.Empty;

        /// <summary>
        /// Default landing dashboard ID for this role
        /// </summary>
        public Guid? LandingDashboardId { get; set; }

        /// <summary>
        /// Default landing page route (e.g., "/dashboard", "/tasks", "/assessments")
        /// </summary>
        [StringLength(255)]
        public string DefaultLandingPage { get; set; } = "/dashboard";

        /// <summary>
        /// Dashboard widgets visible to this role (JSON array of widget configs)
        /// </summary>
        public string? WidgetsJson { get; set; }

        /// <summary>
        /// Quick actions available to this role (JSON array)
        /// </summary>
        public string? QuickActionsJson { get; set; }

        /// <summary>
        /// Navigation items visible to this role (JSON array)
        /// </summary>
        public string? NavigationJson { get; set; }

        /// <summary>
        /// Default filters for lists/grids (JSON object)
        /// </summary>
        public string? DefaultFiltersJson { get; set; }

        /// <summary>
        /// Favorites/bookmarks for this role (JSON array)
        /// </summary>
        public string? FavoritesJson { get; set; }

        /// <summary>
        /// Notification preferences for this role (JSON object)
        /// </summary>
        public string? NotificationPrefsJson { get; set; }

        /// <summary>
        /// Task types this role can be assigned
        /// </summary>
        public string? AssignableTaskTypesJson { get; set; }

        /// <summary>
        /// Is this config active?
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Display order for role selection
        /// </summary>
        public int DisplayOrder { get; set; } = 0;
    }
}
