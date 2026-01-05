using System;
using System.Collections.Generic;

namespace GrcMvc.Models.Dtos
{
    /// <summary>
    /// Policy list item DTO
    /// </summary>
    public class PolicyListItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // Active, Inactive, Pending Review
        public DateTime LastReviewDate { get; set; }
        public int ViolationCount { get; set; }
        public string Category { get; set; } = string.Empty;
    }

    /// <summary>
    /// Policy detail DTO
    /// </summary>
    public class PolicyDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public DateTime EffectiveDate { get; set; }
        public DateTime LastReviewDate { get; set; }
        public DateTime? NextReviewDate { get; set; }
        public int ViolationCount { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<PolicyViolationDto> Violations { get; set; } = new();
    }

    /// <summary>
    /// Policy create DTO
    /// </summary>
    public class PolicyCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public DateTime EffectiveDate { get; set; }
    }

    /// <summary>
    /// Policy edit DTO
    /// </summary>
    public class PolicyEditDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public DateTime EffectiveDate { get; set; }
        public DateTime? NextReviewDate { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>
    /// Policy violation DTO
    /// </summary>
    public class PolicyViolationDto
    {
        public Guid Id { get; set; }
        public string ViolationType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // Open, Remediated, Waived
        public string ReportedBy { get; set; } = string.Empty;
        public DateTime ReportedDate { get; set; }
        public DateTime? RemediationDueDate { get; set; }
        public string RemediationPlan { get; set; } = string.Empty;
    }
}
