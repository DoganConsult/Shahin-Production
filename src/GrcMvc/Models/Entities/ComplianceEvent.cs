using System;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Compliance Event entity for compliance calendar and deadline tracking
    /// </summary>
    public class ComplianceEvent : BaseEntity
    {
        public override string ResourceType => "ComplianceEvent";

        public Guid? WorkspaceId { get; set; }
        public string EventNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty; // Submission, Review, Assessment, Audit, etc.
        public string Category { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ReminderDate { get; set; }
        public string Status { get; set; } = "Scheduled"; // Scheduled, InProgress, Completed, Overdue, Cancelled
        public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical
        public string AssignedTo { get; set; } = string.Empty;
        public Guid? RelatedRegulatorId { get; set; }
        public Guid? RelatedFrameworkId { get; set; }
        public Guid? RelatedAssessmentId { get; set; }
        public string RecurrencePattern { get; set; } = string.Empty; // None, Monthly, Quarterly, Annually
        public string Notes { get; set; } = string.Empty;
    }
}
