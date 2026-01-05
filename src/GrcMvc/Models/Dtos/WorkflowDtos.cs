using System;

namespace GrcMvc.Models.Dtos
{
    /// <summary>
    /// DTO for displaying workflow in list view
    /// </summary>
    public class WorkflowListItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }

    /// <summary>
    /// DTO for creating/updating a workflow
    /// </summary>
    public class CreateWorkflowDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool RequiresApproval { get; set; }
        public string? Approvers { get; set; }
    }

    /// <summary>
    /// DTO for detailed workflow view
    /// </summary>
    public class WorkflowDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Framework { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Version { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public int InstanceCount { get; set; }
    }
}
