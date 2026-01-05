using System;
using System.Collections.Generic;

namespace GrcMvc.Models.Entities
{
    public class Audit : BaseEntity
    {
        public string AuditNumber { get; set; } = string.Empty;
        public string AuditCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // Internal, External, Regulatory
        public string Scope { get; set; } = string.Empty;
        public string Objectives { get; set; } = string.Empty;
        public DateTime PlannedStartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public string Status { get; set; } = "Planned"; // Planned, InProgress, Completed, Cancelled
        public string LeadAuditor { get; set; } = string.Empty;
        public string AuditTeam { get; set; } = string.Empty;
        public string RiskRating { get; set; } = string.Empty;
        public string ExecutiveSummary { get; set; } = string.Empty;
        public string KeyFindings { get; set; } = string.Empty;
        public string ManagementResponse { get; set; } = string.Empty;

        // Navigation properties
        public virtual ICollection<AuditFinding> Findings { get; set; } = new List<AuditFinding>();
        public virtual ICollection<Evidence> Evidences { get; set; } = new List<Evidence>();
    }
}