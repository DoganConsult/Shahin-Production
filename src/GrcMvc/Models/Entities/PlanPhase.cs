using System;
using System.Collections.Generic;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Represents a phase within a compliance plan.
    /// Example phases: QuickScan, DetailedAssessment, Remediation, Follow-up
    /// Layer 3: Operational
    /// </summary>
    public class PlanPhase : BaseEntity
    {
        public Guid PlanId { get; set; }
        
        public string PhaseCode { get; set; } = string.Empty; // e.g., PHASE_QUICK_SCAN, PHASE_REMEDIATION
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Sequence of this phase within the plan
        /// </summary>
        public int Sequence { get; set; }
        
        /// <summary>
        /// Status: Planned, InProgress, Completed, Skipped, Cancelled
        /// </summary>
        public string Status { get; set; } = "Planned";
        
        /// <summary>
        /// When this phase should start
        /// </summary>
        public DateTime PlannedStartDate { get; set; }
        
        /// <summary>
        /// When this phase should end
        /// </summary>
        public DateTime PlannedEndDate { get; set; }
        
        /// <summary>
        /// When this phase actually started
        /// </summary>
        public DateTime? ActualStartDate { get; set; }
        
        /// <summary>
        /// When this phase actually ended
        /// </summary>
        public DateTime? ActualEndDate { get; set; }
        
        /// <summary>
        /// Owner responsible for this phase
        /// </summary>
        public string Owner { get; set; } = string.Empty;
        
        /// <summary>
        /// Overall progress as percentage
        /// </summary>
        public int ProgressPercentage { get; set; } = 0;
        
        // Navigation properties
        public virtual Plan Plan { get; set; } = null!;
        public virtual ICollection<Assessment> Assessments { get; set; } = new List<Assessment>();
    }
}
