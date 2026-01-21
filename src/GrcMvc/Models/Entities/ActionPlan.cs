using System;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Action Plan entity for tracking remediation and improvement plans
    /// </summary>
    public class ActionPlan : BaseEntity
    {
        public override string ResourceType => "ActionPlan";

        public Guid? WorkspaceId { get; set; }
        public string PlanNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Status { get; set; } = "Draft"; // Draft, InProgress, Completed, Cancelled
        public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical
        public string AssignedTo { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string Notes { get; set; } = string.Empty;
        public Guid? RelatedRiskId { get; set; }
        public Guid? RelatedAuditId { get; set; }
        public Guid? RelatedAssessmentId { get; set; }
        public Guid? RelatedControlId { get; set; }
    }
}
