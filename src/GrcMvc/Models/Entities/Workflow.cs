using System;
using System.Collections.Generic;

namespace GrcMvc.Models.Entities
{
    public class Workflow : BaseEntity
    {
        public string WorkflowNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // Approval, Review, Investigation, Remediation
        public string EntityType { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string TriggerType { get; set; } = string.Empty; // Manual, Scheduled, EventBased
        public string Status { get; set; } = "Active"; // Active, Inactive, Draft
        public bool IsActive { get; set; } = true;
        public string Definition { get; set; } = string.Empty; // JSON workflow definition
        public string Steps { get; set; } = string.Empty; // JSON steps definition
        public int Version { get; set; } = 1;
        public bool IsTemplate { get; set; } = false;

        // Navigation properties
        public virtual ICollection<WorkflowExecution> Executions { get; set; } = new List<WorkflowExecution>();
    }
}