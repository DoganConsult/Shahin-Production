using System;
using System.Collections.Generic;

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

    /// <summary>
    /// DTO for starting a workflow instance
    /// </summary>
    public class StartWorkflowDto
    {
        public Guid WorkflowDefinitionId { get; set; }
        public Dictionary<string, object>? InputVariables { get; set; }
    }

    /// <summary>
    /// DTO for workflow instance
    /// </summary>
    public class WorkflowInstanceDto
    {
        public Guid Id { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public string WorkflowType { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public string CurrentState { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<WorkflowTaskDto> Tasks { get; set; } = new();
    }

    /// <summary>
    /// DTO for workflow task
    /// </summary>
    public class WorkflowTaskDto
    {
        public Guid Id { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid? AssignedToUserId { get; set; }
        public string? AssignedToUserName { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public int Priority { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    /// <summary>
    /// DTO for completing a task
    /// </summary>
    public class CompleteTaskDto
    {
        public Guid TaskId { get; set; }
        public Dictionary<string, object>? OutputData { get; set; }
        public string? Notes { get; set; }
    }
}
