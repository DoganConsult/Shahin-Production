using System;

namespace GrcMvc.Models.Dtos
{
    /// <summary>
    /// DTO for task in inbox list
    /// </summary>
    public class InboxTaskListItemDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string AssignedByName { get; set; } = string.Empty;
        public string AssignedToName { get; set; } = string.Empty;
        public string WorkflowName { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public bool IsOverdue { get; set; }
    }

    /// <summary>
    /// DTO for task detail view
    /// </summary>
    public class InboxTaskDetailDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string AssignedByName { get; set; } = string.Empty;
        public string AssignedToName { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public bool IsOverdue => DueDate.HasValue && DueDate.Value < DateTime.Now;
        public List<TaskCommentDto> Comments { get; set; } = new();
    }

    /// <summary>
    /// DTO for task comment
    /// </summary>
    public class TaskCommentDto
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
