using System;

namespace GrcMvc.Models.DTOs
{
    public class FrameworkDto
    {
        public Guid Id { get; set; }
        public string FrameworkCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Jurisdiction { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string Website { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public bool IsMandatory { get; set; }
        public string DataClassification { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    public class CreateFrameworkDto
    {
        public string FrameworkCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Jurisdiction { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string Website { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public bool IsMandatory { get; set; } = false;
        public string DataClassification { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
    }

    public class UpdateFrameworkDto : CreateFrameworkDto
    {
        public Guid Id { get; set; }
    }
}
