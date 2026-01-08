using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GrcMvc.Models.DTOs;
using GrcMvc.Models.Entities;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Service interface for Resilience assessments
    /// </summary>
    public interface IResilienceService
    {
        // ===== Operational Resilience =====
        Task<Resilience> CreateResilienceAsync(Guid tenantId, CreateResilienceDto input);
        Task<Resilience> UpdateResilienceAsync(Guid tenantId, Guid id, UpdateResilienceDto input);
        Task<ResilienceDto> GetResilienceAsync(Guid tenantId, Guid id);
        Task<List<ResilienceDto>> GetResiliencesAsync(Guid tenantId, int page = 1, int pageSize = 20);
        Task<bool> DeleteResilienceAsync(Guid tenantId, Guid id);
        Task<Resilience> AssessResilienceAsync(Guid tenantId, Guid id, ResilienceAssessmentRequestDto? request = null);

        // ===== Risk Resilience =====
        Task<RiskResilience> CreateRiskResilienceAsync(Guid tenantId, CreateRiskResilienceDto input);
        Task<RiskResilienceDto> GetRiskResilienceAsync(Guid tenantId, Guid id);
        Task<List<RiskResilienceDto>> GetRiskResiliencesAsync(Guid tenantId, int page = 1, int pageSize = 20);
        Task<RiskResilience> AssessRiskResilienceAsync(Guid tenantId, Guid id);

        // ===== Incident Response (NEW) =====
        /// <summary>
        /// Create a new incident
        /// </summary>
        Task<IncidentDto> CreateIncidentAsync(Guid tenantId, CreateIncidentDto input);

        /// <summary>
        /// Update incident status and details
        /// </summary>
        Task<IncidentDto> UpdateIncidentAsync(Guid tenantId, Guid incidentId, UpdateIncidentDto input);

        /// <summary>
        /// Get incident by ID
        /// </summary>
        Task<IncidentDto?> GetIncidentAsync(Guid tenantId, Guid incidentId);

        /// <summary>
        /// Get all incidents for a tenant
        /// </summary>
        Task<List<IncidentDto>> GetIncidentsAsync(Guid tenantId, string? status = null, int page = 1, int pageSize = 20);

        /// <summary>
        /// Escalate an incident
        /// </summary>
        Task<IncidentDto> EscalateIncidentAsync(Guid tenantId, Guid incidentId, string escalatedTo, string reason);

        /// <summary>
        /// Resolve an incident
        /// </summary>
        Task<IncidentDto> ResolveIncidentAsync(Guid tenantId, Guid incidentId, string resolvedBy, string resolution);

        /// <summary>
        /// Close an incident with lessons learned
        /// </summary>
        Task<IncidentDto> CloseIncidentAsync(Guid tenantId, Guid incidentId, string closedBy, string lessonsLearned);

        /// <summary>
        /// Get incident response metrics
        /// </summary>
        Task<IncidentMetricsDto> GetIncidentMetricsAsync(Guid tenantId);

        // ===== Business Continuity (NEW) =====
        /// <summary>
        /// Get BCM (Business Continuity Management) score
        /// </summary>
        Task<BcmScoreDto> GetBcmScoreAsync(Guid tenantId);

        /// <summary>
        /// Get DR (Disaster Recovery) readiness score
        /// </summary>
        Task<DrReadinessDto> GetDrReadinessAsync(Guid tenantId);

        /// <summary>
        /// Get overall resilience dashboard
        /// </summary>
        Task<ResilienceDashboardDto> GetResilienceDashboardAsync(Guid tenantId);
    }

    // ===== Incident Response DTOs =====

    public class IncidentDto
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public string IncidentCode { get; set; } = string.Empty; // e.g., ACME-INC-2026-000001
        public string Title { get; set; } = string.Empty;
        public string TitleAr { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty; // Critical, High, Medium, Low
        public string Category { get; set; } = string.Empty; // Security, Availability, Data Breach, etc.
        public string Status { get; set; } = string.Empty; // Open, Investigating, Escalated, Resolved, Closed
        public string StatusAr { get; set; } = string.Empty;
        public DateTime ReportedAt { get; set; }
        public string ReportedBy { get; set; } = string.Empty;
        public DateTime? DetectedAt { get; set; }
        public DateTime? ContainedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public string AssignedTo { get; set; } = string.Empty;
        public string EscalatedTo { get; set; } = string.Empty;
        public string Resolution { get; set; } = string.Empty;
        public string LessonsLearned { get; set; } = string.Empty;
        public string RootCause { get; set; } = string.Empty;
        public int ImpactedUsers { get; set; }
        public decimal FinancialImpact { get; set; }
        public List<string> AffectedSystems { get; set; } = new();
        public List<string> RelatedRiskIds { get; set; } = new();
        public double ResponseTimeHours { get; set; }
        public double ResolutionTimeHours { get; set; }
    }

    public class CreateIncidentDto
    {
        public string Title { get; set; } = string.Empty;
        public string TitleAr { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = "Medium";
        public string Category { get; set; } = string.Empty;
        public string ReportedBy { get; set; } = string.Empty;
        public DateTime? DetectedAt { get; set; }
        public List<string> AffectedSystems { get; set; } = new();
    }

    public class UpdateIncidentDto
    {
        public string? Status { get; set; }
        public string? AssignedTo { get; set; }
        public string? Resolution { get; set; }
        public string? RootCause { get; set; }
        public int? ImpactedUsers { get; set; }
        public decimal? FinancialImpact { get; set; }
    }

    public class IncidentMetricsDto
    {
        public Guid TenantId { get; set; }
        public int TotalIncidents { get; set; }
        public int OpenIncidents { get; set; }
        public int ResolvedIncidents { get; set; }
        public int CriticalIncidents { get; set; }
        public double AverageResponseTimeHours { get; set; }
        public double AverageResolutionTimeHours { get; set; }
        public decimal MttrHours { get; set; } // Mean Time To Resolve
        public decimal MttdHours { get; set; } // Mean Time To Detect
        public int IncidentsThisMonth { get; set; }
        public int IncidentsLastMonth { get; set; }
        public string Trend { get; set; } = string.Empty; // Improving, Stable, Deteriorating
        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    }

    // ===== Business Continuity DTOs =====

    public class BcmScoreDto
    {
        public Guid TenantId { get; set; }
        public decimal OverallScore { get; set; } // 0-100
        public decimal BiaScore { get; set; } // Business Impact Analysis
        public decimal BcpScore { get; set; } // Business Continuity Plan
        public decimal TestingScore { get; set; } // Regular testing
        public decimal TrainingScore { get; set; } // Staff awareness
        public string MaturityLevel { get; set; } = string.Empty; // Initial, Developing, Defined, Managed, Optimized
        public string MaturityLevelAr { get; set; } = string.Empty;
        public DateTime? LastBcpTestDate { get; set; }
        public DateTime? NextBcpTestDate { get; set; }
        public int CriticalProcessesIdentified { get; set; }
        public int CriticalProcessesCovered { get; set; }
        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    }

    public class DrReadinessDto
    {
        public Guid TenantId { get; set; }
        public decimal OverallScore { get; set; } // 0-100
        public decimal RtoCompliance { get; set; } // Recovery Time Objective compliance
        public decimal RpoCompliance { get; set; } // Recovery Point Objective compliance
        public decimal BackupScore { get; set; }
        public decimal FailoverScore { get; set; }
        public string ReadinessLevel { get; set; } = string.Empty; // NotReady, PartiallyReady, Ready, FullyReady
        public string ReadinessLevelAr { get; set; } = string.Empty;
        public DateTime? LastDrTestDate { get; set; }
        public DateTime? NextDrTestDate { get; set; }
        public int SystemsWithDrPlan { get; set; }
        public int TotalCriticalSystems { get; set; }
        public decimal AverageRtoHours { get; set; }
        public decimal AverageRpoHours { get; set; }
        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    }

    public class ResilienceDashboardDto
    {
        public Guid TenantId { get; set; }
        public decimal OverallResilienceScore { get; set; } // 0-100
        public BcmScoreDto BusinessContinuity { get; set; } = new();
        public DrReadinessDto DisasterRecovery { get; set; } = new();
        public IncidentMetricsDto IncidentResponse { get; set; } = new();
        public decimal CyberResilienceScore { get; set; }
        public decimal OperationalResilienceScore { get; set; }
        public string OverallMaturity { get; set; } = string.Empty;
        public string OverallMaturityAr { get; set; } = string.Empty;
        public List<ResilienceRecommendationDto> Recommendations { get; set; } = new();
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    public class ResilienceRecommendationDto
    {
        public string Area { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Recommendation { get; set; } = string.Empty;
        public string RecommendationAr { get; set; } = string.Empty;
        public string Impact { get; set; } = string.Empty;
    }
}
