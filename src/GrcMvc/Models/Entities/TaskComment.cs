using System;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Task communication/comments for collaboration on workflow tasks
    /// </summary>
    public class TaskComment : BaseEntity
    {
        public Guid WorkflowTaskId { get; set; }
        public Guid? TenantId { get; set; }

        public string CommentedByUserId { get; set; } = string.Empty;
        public string CommentedByUserName { get; set; } = string.Empty;

        public string Comment { get; set; } = string.Empty;
        public string? AttachmentUrl { get; set; }

        public DateTime CommentedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public virtual WorkflowTask? WorkflowTask { get; set; }
    }
}
