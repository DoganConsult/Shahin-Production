using System;

namespace GrcMvc.Models.Entities
{
    public class PolicyViolation : BaseEntity
    {
        public string ViolationNumber { get; set; } = string.Empty;
        public string ViolationCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime ViolationDate { get; set; }
        public string Severity { get; set; } = string.Empty; // Critical, High, Medium, Low
        public string DetectedBy { get; set; } = string.Empty;
        public string ViolatorName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Resolution { get; set; } = string.Empty;
        public string Status { get; set; } = "Open"; // Open, UnderInvestigation, Resolved, Escalated
        public DateTime? ResolutionDate { get; set; }

        // Navigation properties
        public Guid PolicyId { get; set; }
        public virtual Policy Policy { get; set; } = null!;
    }
}