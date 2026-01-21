using System;

namespace GrcMvc.Models.DTOs
{
    public class RegulatorDto
    {
        public Guid Id { get; set; }
        public string RegulatorCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Jurisdiction { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string DataClassification { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    public class CreateRegulatorDto
    {
        public string RegulatorCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Jurisdiction { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public bool IsPrimary { get; set; } = false;
        public string Notes { get; set; } = string.Empty;
        public string DataClassification { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
    }

    public class UpdateRegulatorDto : CreateRegulatorDto
    {
        public Guid Id { get; set; }
    }
}
