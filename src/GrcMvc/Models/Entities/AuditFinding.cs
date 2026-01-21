using System;

namespace GrcMvc.Models.Entities
{
    public class AuditFinding : BaseEntity
    {
        public string FindingNumber { get; set; } = string.Empty;
        public string FindingCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty; // Critical, High, Medium, Low
        public string Category { get; set; } = string.Empty;
        public string RootCause { get; set; } = string.Empty;
        public string Impact { get; set; } = string.Empty;
        public string Recommendation { get; set; } = string.Empty;
        public string ManagementResponse { get; set; } = string.Empty;
        public string ResponsibleParty { get; set; } = string.Empty;
        public DateTime? TargetDate { get; set; }
        public string Status { get; set; } = "Open"; // Open, InProgress, Closed, Overdue

        // Navigation properties
        public Guid AuditId { get; set; }
        public virtual Audit Audit { get; set; } = null!;
    }
}