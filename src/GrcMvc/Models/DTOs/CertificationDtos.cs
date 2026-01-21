using System;
using System.Collections.Generic;

namespace GrcMvc.Models.DTOs
{
    /// <summary>
    /// DTO for creating a new certification initiative
    /// </summary>
    public class CreateCertificationDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CertificationType { get; set; } = string.Empty; // ISO 27001, SOC 2, PCI DSS, etc.
        public string CertificationBody { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty;
        public DateTime? TargetDate { get; set; }
        public DateTime? AuditDate { get; set; }
        public Guid? LeadAuditorId { get; set; }
        public string? Priority { get; set; } // Critical, High, Medium, Low
    }

    /// <summary>
    /// DTO for updating certification initiative
    /// </summary>
    public class UpdateCertificationDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // Planning, InProgress, UnderReview, Certified, Expired
        public decimal? ReadinessScore { get; set; }
        public decimal? ComplianceScore { get; set; }
        public int? GapCount { get; set; }
        public int? ClosedGapCount { get; set; }
        public DateTime? CertificationDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? Findings { get; set; }
        public string? ActionItems { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO for certification initiative response
    /// </summary>
    public class CertificationDto
    {
        public Guid Id { get; set; }
        public string CertificationNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CertificationType { get; set; } = string.Empty;
        public string CertificationBody { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;

        public DateTime? InitiatedDate { get; set; }
        public DateTime? TargetDate { get; set; }
        public DateTime? AuditDate { get; set; }
        public DateTime? CertificationDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? LastAssessmentDate { get; set; }

        public Guid? LeadAuditorId { get; set; }
        public string? LeadAuditorName { get; set; }

        public decimal? ReadinessScore { get; set; }
        public decimal? ComplianceScore { get; set; }
        public decimal? OverallScore { get; set; }

        public int? TotalRequirements { get; set; }
        public int? MetRequirements { get; set; }
        public int? GapCount { get; set; }
        public int? ClosedGapCount { get; set; }
        public int? OpenGapCount { get; set; }

        public string? CurrentPhase { get; set; } // Gap Analysis, Remediation, Pre-Audit, Audit, Certified
        public string? Findings { get; set; }
        public string? ActionItems { get; set; }
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid TenantId { get; set; }
    }

    /// <summary>
    /// DTO for certification readiness assessment
    /// </summary>
    public class CertificationReadinessDto
    {
        public Guid CertificationId { get; set; }
        public string CertificationName { get; set; } = string.Empty;
        public string CertificationType { get; set; } = string.Empty;
        public decimal OverallReadiness { get; set; }
        public string ReadinessLevel { get; set; } = string.Empty; // Not Ready, Partially Ready, Ready, Certified

        public int TotalControls { get; set; }
        public int ImplementedControls { get; set; }
        public int PartiallyImplementedControls { get; set; }
        public int NotImplementedControls { get; set; }

        public int TotalGaps { get; set; }
        public int CriticalGaps { get; set; }
        public int HighGaps { get; set; }
        public int MediumGaps { get; set; }
        public int LowGaps { get; set; }

        public int ClosedGaps { get; set; }
        public int OpenGaps { get; set; }

        public decimal DocumentationScore { get; set; }
        public decimal ImplementationScore { get; set; }
        public decimal EvidenceScore { get; set; }
        public decimal TestingScore { get; set; }

        public DateTime? EstimatedReadyDate { get; set; }
        public DateTime? TargetAuditDate { get; set; }
        public int? DaysUntilAudit { get; set; }

        public List<CertificationGapDto> TopGaps { get; set; } = new();
        public List<CertificationRecommendationDto> Recommendations { get; set; } = new();

        public DateTime CalculatedAt { get; set; }
    }

    /// <summary>
    /// DTO for certification gap
    /// </summary>
    public class CertificationGapDto
    {
        public Guid Id { get; set; }
        public string GapNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty; // Critical, High, Medium, Low
        public string Status { get; set; } = string.Empty; // Open, InProgress, Closed
        public string ControlArea { get; set; } = string.Empty;
        public string? AssignedTo { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ClosedDate { get; set; }
    }

    /// <summary>
    /// DTO for certification recommendation
    /// </summary>
    public class CertificationRecommendationDto
    {
        public string Area { get; set; } = string.Empty;
        public string Recommendation { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Impact { get; set; } = string.Empty;
        public int EstimatedEffortDays { get; set; }
    }

    /// <summary>
    /// DTO for certification audit preparation
    /// </summary>
    public class CertificationAuditPrepDto
    {
        public Guid CertificationId { get; set; }
        public string CertificationName { get; set; } = string.Empty;
        public DateTime? ScheduledAuditDate { get; set; }
        public int DaysUntilAudit { get; set; }

        public decimal ReadinessScore { get; set; }
        public bool IsReady { get; set; }

        public int TotalChecklistItems { get; set; }
        public int CompletedChecklistItems { get; set; }
        public int PendingChecklistItems { get; set; }

        public List<string> CompletedActivities { get; set; } = new();
        public List<string> PendingActivities { get; set; } = new();
        public List<string> CriticalFindings { get; set; } = new();

        public DateTime LastUpdated { get; set; }
    }
}
