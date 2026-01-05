using System;
using System.Collections.Generic;

namespace GrcMvc.Models.Dtos
{
    /// <summary>
    /// DTO for approval details
    /// </summary>
    public class ApprovalDto
    {
        public Guid Id { get; set; }
        public Guid WorkflowId { get; set; }
        public string? WorkflowName { get; set; }
        public int ApprovalLevel { get; set; }
        public string? ApprovalType { get; set; } // Sequential, Parallel
        public string? Status { get; set; } // Pending, Approved, Rejected, Delegated
        public string? SubmittedBy { get; set; }
        public string? SubmittedByName { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string? AssignedTo { get; set; }
        public string? AssignedToName { get; set; }
        public DateTime DueDate { get; set; }
        public int DaysRemaining { get; set; }
        public string? Priority { get; set; }
    }

    /// <summary>
    /// DTO for approval list item (summary)
    /// </summary>
    public class ApprovalListItemDto
    {
        public Guid Id { get; set; }
        public string? WorkflowName { get; set; }
        public string? Status { get; set; }
        public DateTime DueDate { get; set; }
        public string? Priority { get; set; }
    }

    /// <summary>
    /// DTO for approval levels in chain
    /// </summary>
    public class ApprovalLevelDto
    {
        public int Level { get; set; }
        public string? Name { get; set; }
        public string? ApprovalType { get; set; } // Sequential, Parallel
        public List<string>? ApproverIds { get; set; }
        public List<string>? ApproverNames { get; set; }
        public string? RequiredPermission { get; set; }
        public int SlaHours { get; set; }
    }

    /// <summary>
    /// DTO for approval history entry
    /// </summary>
    public class ApprovalHistoryDto
    {
        public Guid Id { get; set; }
        public int ApprovalLevel { get; set; }
        public string? Action { get; set; } // Approved, Rejected, Delegated
        public string? ActionBy { get; set; }
        public string? ActionByName { get; set; }
        public DateTime ActionAt { get; set; }
        public string? Comments { get; set; }
        public string? DelegatedTo { get; set; }
    }

    /// <summary>
    /// DTO for approval statistics
    /// </summary>
    public class ApprovalStatsDto
    {
        public int TotalPending { get; set; }
        public int Overdue { get; set; }
        public int AverageTurnaroundHours { get; set; }
        public Dictionary<string, int>? ByApprover { get; set; }
        public Dictionary<string, int>? ByStatus { get; set; }
        public double CompletionRate { get; set; }
    }
}
