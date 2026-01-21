using System;
using System.Collections.Generic;

namespace GrcMvc.Models.DTOs
{
    /// <summary>
    /// Audit list item DTO
    /// </summary>
    public class AuditListItemDto
    {
        public Guid Id { get; set; }
        public string AuditNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // Planned, InProgress, Completed
        public string Type { get; set; } = string.Empty; // Internal, External, Regulatory
        public string LeadAuditor { get; set; } = string.Empty;
        public DateTime PlannedStartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public int FindingCount { get; set; }
        public bool IsOverdue { get; set; }
    }

    /// <summary>
    /// Audit detail DTO
    /// </summary>
    public class AuditDetailDto
    {
        public Guid Id { get; set; }
        public string AuditNumber { get; set; } = string.Empty;
        public string AuditCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty;
        public string Objectives { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string LeadAuditor { get; set; } = string.Empty;
        public string AuditTeam { get; set; } = string.Empty;
        public DateTime PlannedStartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public string RiskRating { get; set; } = string.Empty;
        public string ExecutiveSummary { get; set; } = string.Empty;
        public string KeyFindings { get; set; } = string.Empty;
        public string ManagementResponse { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public List<AuditFindingDto> Findings { get; set; } = new();
    }

    /// <summary>
    /// Audit create DTO
    /// </summary>
    public class AuditCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // Internal, External, Regulatory
        public string Scope { get; set; } = string.Empty;
        public string Objectives { get; set; } = string.Empty;
        public string LeadAuditor { get; set; } = string.Empty;
        public string AuditTeam { get; set; } = string.Empty;
        public DateTime PlannedStartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
    }

    /// <summary>
    /// Audit edit DTO
    /// </summary>
    public class AuditEditDto
    {
        public Guid Id { get; set; }
        public string AuditNumber { get; set; } = string.Empty; // Auto-generated
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty;
        public string Objectives { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string LeadAuditor { get; set; } = string.Empty;
        public string AuditTeam { get; set; } = string.Empty;
        public DateTime PlannedStartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public string RiskRating { get; set; } = string.Empty;
        public string ExecutiveSummary { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }

    // AuditFindingDto and CreateAuditFindingDto are defined in CommonDtos.cs
    // Note: The versions in AuditDtos.cs have additional properties (Category, ResponsibleParty, etc.)
    // Consider consolidating these if the extended versions are needed.

    // CreateAuditFindingDto is defined in CommonDtos.cs
    // If extended version with Category, ResponsibleParty is needed, consider adding properties to the CommonDtos version
}
