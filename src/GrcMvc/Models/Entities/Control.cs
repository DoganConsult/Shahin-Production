using System;
using System.Collections.Generic;

namespace GrcMvc.Models.Entities
{
    public class Control : BaseEntity
    {
        public string ControlId { get; set; } = string.Empty;
        public string ControlCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // Preventive, Detective, Corrective
        public string Frequency { get; set; } = string.Empty; // Daily, Weekly, Monthly, Quarterly, Annual
        public string Owner { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public int EffectivenessScore { get; set; }
        public int Effectiveness { get; set; }
        public DateTime? LastTestDate { get; set; }
        public DateTime? NextTestDate { get; set; }

        // Navigation properties
        public Guid? RiskId { get; set; }
        public virtual Risk? Risk { get; set; }
        public virtual ICollection<Assessment> Assessments { get; set; } = new List<Assessment>();
        public virtual ICollection<Evidence> Evidences { get; set; } = new List<Evidence>();
    }
}