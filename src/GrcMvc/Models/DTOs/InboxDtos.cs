using System;

namespace GrcMvc.Models.DTOs
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

    // InboxTaskDetailDto and TaskCommentDto are defined in CommonDtos.cs
}
