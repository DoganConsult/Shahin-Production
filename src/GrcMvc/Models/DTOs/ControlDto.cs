using System;

namespace GrcMvc.Models.DTOs
{
    public class ControlDto
    {
        public Guid Id { get; set; }
        public string ControlId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int EffectivenessScore { get; set; }
        public int Effectiveness { get; set; }
        public DateTime? ImplementationDate { get; set; }
        public DateTime? LastReviewDate { get; set; }
        public DateTime? NextReviewDate { get; set; }
        public DateTime? LastTestDate { get; set; }
        public DateTime? NextTestDate { get; set; }
        public Guid? RiskId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    public class CreateControlDto
    {
        public string ControlId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public int EffectivenessScore { get; set; }
        public DateTime? LastTestDate { get; set; }
        public DateTime? NextTestDate { get; set; }
        public Guid? RiskId { get; set; }
    }

    public class UpdateControlDto
    {
        public string ControlId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int EffectivenessScore { get; set; }
        public int Effectiveness { get; set; }
        public DateTime? ImplementationDate { get; set; }
        public DateTime? LastReviewDate { get; set; }
        public DateTime? NextReviewDate { get; set; }
        public DateTime? LastTestDate { get; set; }
        public DateTime? NextTestDate { get; set; }
        public Guid? RiskId { get; set; }
    }
}