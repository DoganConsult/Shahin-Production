using System;

namespace GrcMvc.Models.DTOs
{
    public class VendorDto
    {
        public Guid Id { get; set; }
        public string VendorCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string ContactName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string RiskLevel { get; set; } = string.Empty;
        public DateTime? LastAssessmentDate { get; set; }
        public DateTime? NextAssessmentDate { get; set; }
        public string AssessmentStatus { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string DataClassification { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    public class CreateVendorDto
    {
        public string VendorCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public string ContactName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string RiskLevel { get; set; } = "Medium";
        public DateTime? NextAssessmentDate { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string DataClassification { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
    }

    public class UpdateVendorDto : CreateVendorDto
    {
        public Guid Id { get; set; }
        public DateTime? LastAssessmentDate { get; set; }
        public string AssessmentStatus { get; set; } = string.Empty;
    }
}
