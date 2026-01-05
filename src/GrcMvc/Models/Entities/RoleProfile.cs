using System;
using System.Collections.Generic;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Represents a predefined role profile in the system with scope, responsibilities, and approval levels
    /// </summary>
    public class RoleProfile : BaseEntity
    {
        /// <summary>
        /// Unique role identifier (e.g., "RISK_MANAGER", "COMPLIANCE_OFFICER")
        /// </summary>
        public string RoleCode { get; set; } = null!;

        /// <summary>
        /// Human-readable role name (e.g., "Risk Manager")
        /// </summary>
        public string RoleName { get; set; } = null!;

        /// <summary>
        /// Organizational layer (Executive, Management, Operational, Support)
        /// </summary>
        public string Layer { get; set; } = null!;

        /// <summary>
        /// Department or functional area
        /// </summary>
        public string Department { get; set; } = null!;

        /// <summary>
        /// Role description and purpose
        /// </summary>
        public string Description { get; set; } = null!;

        /// <summary>
        /// Scope and areas of responsibility (comma-separated or JSON)
        /// </summary>
        public string Scope { get; set; } = null!;

        /// <summary>
        /// Key responsibilities (JSON array format)
        /// Example: ["Risk Assessment", "Control Evaluation", "Report Generation"]
        /// </summary>
        public string Responsibilities { get; set; } = null!;

        /// <summary>
        /// Approval authority level (0=None, 1=Own Items, 2=Team Items, 3=Department, 4=Organization)
        /// </summary>
        public int ApprovalLevel { get; set; }

        /// <summary>
        /// Maximum approval amount/value this role can approve (if applicable)
        /// </summary>
        public decimal? ApprovalAuthority { get; set; }

        /// <summary>
        /// Can this role escalate tasks to management?
        /// </summary>
        public bool CanEscalate { get; set; } = false;

        /// <summary>
        /// Can this role approve workflow tasks?
        /// </summary>
        public bool CanApprove { get; set; } = false;

        /// <summary>
        /// Can this role reject tasks?
        /// </summary>
        public bool CanReject { get; set; } = false;

        /// <summary>
        /// Can this role reassign tasks?
        /// </summary>
        public bool CanReassign { get; set; } = false;

        /// <summary>
        /// Workflows this role participates in (comma-separated workflow numbers)
        /// Example: "WF-NCA-ECC-001,WF-SAMA-CSF-001,WF-ERM-001"
        /// </summary>
        public string? ParticipatingWorkflows { get; set; }

        /// <summary>
        /// Is this role active?
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Optional: Tenant-specific role (null = organization-wide)
        /// </summary>
        public Guid? TenantId { get; set; }

        /// <summary>
        /// Sort order for UI display
        /// </summary>
        public int DisplayOrder { get; set; } = 0;

        /// <summary>
        /// Related users in this role (optional)
        /// </summary>
        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    }
}
