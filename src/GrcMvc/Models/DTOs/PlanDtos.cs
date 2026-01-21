using System;
using System.Collections.Generic;

namespace GrcMvc.Models.DTOs
{
    /// <summary>
    /// DTO for assessment plan details
    /// </summary>
    public class PlanDto
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public string PlanCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string PlanType { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime TargetEndDate { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public int ProgressPercentage { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public List<PlanPhaseDto> Phases { get; set; } = new List<PlanPhaseDto>();
    }

    /// <summary>
    /// DTO for plan phase details
    /// </summary>
    public class PlanPhaseDto
    {
        public Guid Id { get; set; }
        public Guid PlanId { get; set; }
        public string PhaseCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int SequenceNumber { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public string Owner { get; set; } = string.Empty;
        public int? ProgressPercentage { get; set; }
        public string Deliverables { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }

    /// <summary>
    /// DTO for updating a plan's status
    /// </summary>
    public class UpdatePlanStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for updating a phase
    /// </summary>
    public class UpdatePhaseDto
    {
        public string Status { get; set; } = string.Empty;
        public int ProgressPercentage { get; set; }
    }

    /// <summary>
    /// DTO for plan list with pagination
    /// </summary>
    public class PlanListDto
    {
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public List<PlanDto> Plans { get; set; } = new List<PlanDto>();
    }
}
