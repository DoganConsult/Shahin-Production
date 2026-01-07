using System;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Vendor entity for managing third-party vendors and suppliers
    /// </summary>
    public class Vendor : BaseEntity
    {
        public override string ResourceType => "Vendor";

        public Guid? WorkspaceId { get; set; }
        public string VendorCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // Software, Service, Hardware, etc.
        public string Status { get; set; } = "Active"; // Active, Inactive, Suspended
        public string ContactName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string RiskLevel { get; set; } = "Medium"; // Low, Medium, High
        public DateTime? LastAssessmentDate { get; set; }
        public DateTime? NextAssessmentDate { get; set; }
        public string AssessmentStatus { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
}
