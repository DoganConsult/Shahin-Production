using System;
using System.Collections.Generic;

namespace GrcMvc.Models.DTOs
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

    // ApprovalListItemDto is defined in CommonDtos.cs

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

    // ApprovalHistoryDto is defined in CommonDtos.cs (note: property structure differs slightly)

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
