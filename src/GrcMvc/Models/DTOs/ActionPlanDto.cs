using System;

namespace GrcMvc.Models.DTOs
{
    public class ActionPlanDto
    {
        public Guid Id { get; set; }
        public string PlanNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string AssignedTo { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string Notes { get; set; } = string.Empty;
        public Guid? RelatedRiskId { get; set; }
        public Guid? RelatedAuditId { get; set; }
        public Guid? RelatedAssessmentId { get; set; }
        public Guid? RelatedControlId { get; set; }
        public string DataClassification { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    public class CreateActionPlanDto
    {
        public string PlanNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Status { get; set; } = "Draft";
        public string Priority { get; set; } = "Medium";
        public string AssignedTo { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string Notes { get; set; } = string.Empty;
        public Guid? RelatedRiskId { get; set; }
        public Guid? RelatedAuditId { get; set; }
        public Guid? RelatedAssessmentId { get; set; }
        public Guid? RelatedControlId { get; set; }
        public string DataClassification { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
    }

    public class UpdateActionPlanDto : CreateActionPlanDto
    {
        public Guid Id { get; set; }
        public DateTime? CompletedDate { get; set; }
    }
}
