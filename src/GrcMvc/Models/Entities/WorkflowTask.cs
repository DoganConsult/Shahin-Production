using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// WorkflowTask - EF Core persistence entity for workflow tasks.
    /// Inherits Guid Id from BaseEntity. This is the DB-backed type.
    /// For workflow engine/runtime logic, use Models.Workflows.WorkflowRuntimeTask.
    /// </summary>
    public class WorkflowTask : BaseEntity
    {
        // Multi-tenant isolation
        public Guid TenantId { get; set; }
        public Guid WorkflowInstanceId { get; set; }

        // Task identification
        public string TaskName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Alias for TaskName (service compatibility)
        /// </summary>
        [NotMapped]
        public string Name
        {
            get => TaskName;
            set => TaskName = value;
        }

        /// <summary>
        /// Completion percentage (0-100)
        /// </summary>
        public int CompletionPercentage { get; set; }

        // Assignment
        public Guid? AssignedToUserId { get; set; }
        public string? AssignedToUserName { get; set; }

        // SLA and Priority
        public DateTime? DueDate { get; set; }
        public int Priority { get; set; } = 2; // 1=High, 2=Medium, 3=Low

        // Task Status
        public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed, Rejected

        // Completion tracking
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public Guid? CompletedByUserId { get; set; }
        public string? CompletionNotes { get; set; }

        // Escalation tracking
        public bool IsEscalated { get; set; } = false;
        public int EscalationLevel { get; set; } = 0;
        public Guid? EscalatedToUserId { get; set; }
        public DateTime? LastEscalatedAt { get; set; }

        // Metadata for agent assignment and delegation tracking
        public string? Metadata { get; set; } // JSON: {AgentType, DelegatedFrom, etc.}

        // Navigation
        public virtual WorkflowInstance WorkflowInstance { get; set; } = null!;
        public virtual ICollection<TaskDelegation> Delegations { get; set; } = new List<TaskDelegation>();
    }
}
