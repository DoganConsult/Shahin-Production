using System;
using System.Collections.Generic;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// WorkflowInstance - EF Core persistence entity for workflow instances.
    /// Inherits Guid Id from BaseEntity. This is the DB-backed type.
    /// For workflow engine/runtime logic, use Models.Workflows.WorkflowRuntimeInstance.
    /// </summary>
    public class WorkflowInstance : BaseEntity
    {
        // Multi-tenant isolation (Guid for distributed systems)
        public Guid TenantId { get; set; }

        // Instance identification
        public string InstanceNumber { get; set; } = string.Empty; // WF-2026-0001
        public Guid? WorkflowDefinitionId { get; set; }

        // Workflow type and state
        public string WorkflowType { get; set; } = string.Empty; // ControlImplementation, RiskAssessment, etc.
        public string CurrentState { get; set; } = string.Empty; // State machine state
        public string Status { get; set; } = "Active"; // Active, Completed, Suspended, Cancelled

        // Entity reference (polymorphic)
        public string EntityType { get; set; } = string.Empty; // Control, Risk, Policy, etc.
        public Guid EntityId { get; set; }

        // Timing
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public DateTime? SlaDueDate { get; set; }
        public bool SlaBreached { get; set; } = false;
        public DateTime? SlaBreachedAt { get; set; }

        // User tracking
        public Guid? InitiatedByUserId { get; set; }
        public string? InitiatedByUserName { get; set; }
        public Guid? CompletedByUserId { get; set; }

        // JSON data storage
        public string? Variables { get; set; } // JSON workflow variables
        public string? Metadata { get; set; } // JSON metadata
        public string? Result { get; set; } // JSON result

        // Navigation properties
        public virtual WorkflowDefinition? WorkflowDefinition { get; set; }
        public virtual ICollection<WorkflowTask> Tasks { get; set; } = new List<WorkflowTask>();
        public virtual ICollection<WorkflowAuditEntry> AuditEntries { get; set; } = new List<WorkflowAuditEntry>();
    }
}
