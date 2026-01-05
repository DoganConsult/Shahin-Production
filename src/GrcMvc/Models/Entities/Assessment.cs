using System;
using System.Collections.Generic;

namespace GrcMvc.Models.Entities
{
    public class Assessment : BaseEntity
    {
        public string AssessmentNumber { get; set; } = string.Empty;
        public string AssessmentCode { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // Risk, Control, Compliance
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string Status { get; set; } = "Planned"; // Planned, InProgress, Completed, Cancelled
        public string AssignedTo { get; set; } = string.Empty;
        public string ReviewedBy { get; set; } = string.Empty;
        public int? ComplianceScore { get; set; }
        public int Score { get; set; }
        public string Findings { get; set; } = string.Empty;
        public string Recommendations { get; set; } = string.Empty;

        // Template/Framework linkage
        public string TemplateCode { get; set; } = string.Empty;
        public string FrameworkCode { get; set; } = string.Empty;

        // Navigation properties
        public Guid? PlanId { get; set; }
        public virtual Plan? Plan { get; set; }
        public Guid? RiskId { get; set; }
        public virtual Risk? Risk { get; set; }
        public Guid? ControlId { get; set; }
        public virtual Control? Control { get; set; }
        public virtual ICollection<Evidence> Evidences { get; set; } = new List<Evidence>();
        public virtual ICollection<AssessmentRequirement> Requirements { get; set; } = new List<AssessmentRequirement>();
    }
}