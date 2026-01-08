using System;
using System.Collections.Generic;

namespace GrcMvc.Models.DTOs
{
    /// <summary>
    /// Assessment list item DTO
    /// </summary>
    public class AssessmentListItemDto
    {
        public Guid Id { get; set; }
        public string AssessmentNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // Planned, InProgress, Completed
        public string Type { get; set; } = string.Empty; // Risk, Control, Compliance
        public string AssignedTo { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Score { get; set; }
        public bool IsOverdue { get; set; }
    }

    /// <summary>
    /// Assessment detail DTO
    /// </summary>
    public class AssessmentDetailDto
    {
        public Guid Id { get; set; }
        public string AssessmentNumber { get; set; } = string.Empty;
        public string AssessmentCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string AssignedTo { get; set; } = string.Empty;
        public string ReviewedBy { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? ComplianceScore { get; set; }
        public int Score { get; set; }
        public string Findings { get; set; } = string.Empty;
        public string Recommendations { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsOverdue { get; set; }
        public List<EvidenceListItemDto> Evidences { get; set; } = new();

        // MEDIUM FIX: Added missing fields for framework/template linkage
        public string TemplateCode { get; set; } = string.Empty;
        public string FrameworkCode { get; set; } = string.Empty;
        public Guid? PlanId { get; set; }
    }

    /// <summary>
    /// Assessment create DTO
    /// </summary>
    public class AssessmentCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // Risk, Control, Compliance
        public DateTime StartDate { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public string AssignedTo { get; set; } = string.Empty;
        public string? RiskId { get; set; }
        public string? ControlId { get; set; }
    }

    /// <summary>
    /// Assessment edit DTO
    /// </summary>
    public class AssessmentEditDto
    {
        public Guid Id { get; set; }
        public string AssessmentNumber { get; set; } = string.Empty; // Auto-generated
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string AssignedTo { get; set; } = string.Empty;
        public string ReviewedBy { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Score { get; set; }
        public string Findings { get; set; } = string.Empty;
        public string Recommendations { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }
}
