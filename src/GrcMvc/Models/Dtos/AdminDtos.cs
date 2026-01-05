using System;
using System.Collections.Generic;

namespace GrcMvc.Models.Dtos
{
    /// <summary>
    /// DTO for role in admin list
    /// </summary>
    public class RoleListItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Permissions { get; set; } = string.Empty;
        public int UserCount { get; set; }
    }

    /// <summary>
    /// DTO for user in admin list
    /// </summary>
    public class UserListItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Roles { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? LastLogin { get; set; }
    }

    /// <summary>
    /// DTO for workflow edit
    /// </summary>
    public class WorkflowEditDto
    {
        public Guid Id { get; set; }
        public string WorkflowNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool RequiresApproval { get; set; }
        public string Approvers { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>
    /// DTO for approval review
    /// </summary>
    public class ApprovalReviewDto
    {
        public Guid Id { get; set; }
        public string WorkflowName { get; set; } = string.Empty;
        public string ApprovalType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string SubmittedByName { get; set; } = string.Empty;
        public DateTime SubmittedDate { get; set; }
    }
}
