using System;

namespace GrcMvc.Models.DTOs
{
    public class ComplianceEventDto
    {
        public Guid Id { get; set; }
        public string EventNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ReminderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string AssignedTo { get; set; } = string.Empty;
        public Guid? RelatedRegulatorId { get; set; }
        public Guid? RelatedFrameworkId { get; set; }
        public Guid? RelatedAssessmentId { get; set; }
        public string RecurrencePattern { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string DataClassification { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    public class CreateComplianceEventDto
    {
        public string EventNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ReminderDate { get; set; }
        public string Status { get; set; } = "Scheduled";
        public string Priority { get; set; } = "Medium";
        public string AssignedTo { get; set; } = string.Empty;
        public Guid? RelatedRegulatorId { get; set; }
        public Guid? RelatedFrameworkId { get; set; }
        public Guid? RelatedAssessmentId { get; set; }
        public string RecurrencePattern { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string DataClassification { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
    }

    public class UpdateComplianceEventDto : CreateComplianceEventDto
    {
        public Guid Id { get; set; }
    }
}
