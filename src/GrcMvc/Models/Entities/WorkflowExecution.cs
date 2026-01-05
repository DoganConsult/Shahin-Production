using System;

namespace GrcMvc.Models.Entities
{
    public class WorkflowExecution : BaseEntity
    {
        public string ExecutionNumber { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Status { get; set; } = "Running"; // Running, Completed, Failed, Cancelled
        public string CurrentStep { get; set; } = string.Empty;
        public string InitiatedBy { get; set; } = string.Empty;
        public string TriggeredBy { get; set; } = string.Empty;
        public string Context { get; set; } = string.Empty; // JSON context data
        public string Result { get; set; } = string.Empty; // JSON result data
        public string ExecutionHistory { get; set; } = string.Empty; // JSON execution history
        public string ErrorMessage { get; set; } = string.Empty;

        // Navigation properties
        public Guid WorkflowId { get; set; }
        public virtual Workflow Workflow { get; set; } = null!;
    }
}