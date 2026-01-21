using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Sustainability & Excellence Service Interface
    /// Manages continuous improvement, maturity progression, and long-term compliance sustainability
    /// GRC Lifecycle Stage 5: Continuous Sustainability (المرحلة 5: الاستدامة المستمرة)
    /// </summary>
    public interface ISustainabilityService
    {
        // ===== Maturity Assessment =====
        /// <summary>
        /// Get overall GRC maturity score for a tenant
        /// </summary>
        Task<MaturityScoreDto> GetMaturityScoreAsync(Guid tenantId);

        /// <summary>
        /// Get maturity history for trend analysis
        /// </summary>
        Task<List<MaturityHistoryDto>> GetMaturityHistoryAsync(Guid tenantId, int months = 12);

        /// <summary>
        /// Calculate maturity improvement roadmap
        /// </summary>
        Task<MaturityRoadmapDto> GetMaturityRoadmapAsync(Guid tenantId);

        // ===== Continuous Improvement =====
        /// <summary>
        /// Create a continuous improvement initiative
        /// </summary>
        Task<SustainabilityImprovementDto> CreateImprovementInitiativeAsync(Guid tenantId, CreateImprovementDto input);

        /// <summary>
        /// Get all improvement initiatives
        /// </summary>
        Task<List<SustainabilityImprovementDto>> GetImprovementInitiativesAsync(Guid tenantId, string? status = null);

        /// <summary>
        /// Update improvement initiative progress
        /// </summary>
        Task<SustainabilityImprovementDto> UpdateImprovementProgressAsync(Guid tenantId, Guid initiativeId, decimal percentComplete, string? notes = null);

        /// <summary>
        /// Complete an improvement initiative
        /// </summary>
        Task<SustainabilityImprovementDto> CompleteImprovementAsync(Guid tenantId, Guid initiativeId, string completedBy, string outcomes);

        // ===== KPI Tracking =====
        /// <summary>
        /// Get GRC KPIs for a tenant
        /// </summary>
        Task<GrcKpisDto> GetKpisAsync(Guid tenantId);

        /// <summary>
        /// Get KPI trend data
        /// </summary>
        Task<List<KpiTrendDto>> GetKpiTrendsAsync(Guid tenantId, string kpiName, int months = 12);

        // ===== Compliance Health =====
        /// <summary>
        /// Get overall compliance health status
        /// </summary>
        Task<ComplianceHealthDto> GetComplianceHealthAsync(Guid tenantId);

        /// <summary>
        /// Get compliance sustainability forecast
        /// </summary>
        Task<ComplianceForecastDto> GetComplianceForecastAsync(Guid tenantId, int monthsAhead = 6);

        // ===== Excellence Benchmarking =====
        /// <summary>
        /// Get industry benchmarks comparison
        /// </summary>
        Task<SustainabilityBenchmarkDto> GetBenchmarksAsync(Guid tenantId);

        /// <summary>
        /// Get KSA regulatory compliance benchmarks
        /// </summary>
        Task<KsaBenchmarkDto> GetKsaBenchmarksAsync(Guid tenantId);

        // ===== Sustainability Dashboard =====
        /// <summary>
        /// Get comprehensive sustainability dashboard
        /// </summary>
        Task<SustainabilityDashboardDto> GetDashboardAsync(Guid tenantId);

        // ===== Program Management (Stage 5) =====
        /// <summary>
        /// Get all sustainability/improvement initiatives for a tenant
        /// </summary>
        Task<List<SustainabilityImprovementDto>> GetInitiativesAsync(Guid tenantId);

        /// <summary>
        /// Get program execution metrics and status
        /// </summary>
        Task<ProgramExecutionDto> GetProgramExecutionAsync(Guid tenantId);
    }

    // ===== Program Execution DTO =====
    
    public class ProgramExecutionDto
    {
        public Guid TenantId { get; set; }
        public int TotalPrograms { get; set; }
        public int ActivePrograms { get; set; }
        public int CompletedPrograms { get; set; }
        public decimal OverallProgress { get; set; }
        public decimal BudgetUtilization { get; set; }
        public List<ProgramStatusDto> Programs { get; set; } = new();
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    public class ProgramStatusDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // Active, Completed, OnHold, Cancelled
        public decimal Progress { get; set; }
        public DateTime? TargetDate { get; set; }
        public string Owner { get; set; } = string.Empty;
    }

    // ===== Maturity DTOs =====

    public class MaturityScoreDto
    {
        public Guid TenantId { get; set; }
        public decimal OverallScore { get; set; } // 0-100
        public string MaturityLevel { get; set; } = string.Empty; // Initial, Developing, Defined, Managed, Optimizing
        public string MaturityLevelAr { get; set; } = string.Empty;
        public int MaturityTier { get; set; } // 1-5
        public MaturityDimensionScoreDto Governance { get; set; } = new();
        public MaturityDimensionScoreDto Risk { get; set; } = new();
        public MaturityDimensionScoreDto Compliance { get; set; } = new();
        public MaturityDimensionScoreDto Resilience { get; set; } = new();
        public MaturityDimensionScoreDto Excellence { get; set; } = new();
        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    }

    public class MaturityDimensionScoreDto
    {
        public string Dimension { get; set; } = string.Empty;
        public string DimensionAr { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public string Level { get; set; } = string.Empty;
        public string LevelAr { get; set; } = string.Empty;
        public List<string> Strengths { get; set; } = new();
        public List<string> Gaps { get; set; } = new();
    }

    public class MaturityHistoryDto
    {
        public DateTime Date { get; set; }
        public decimal OverallScore { get; set; }
        public string MaturityLevel { get; set; } = string.Empty;
        public decimal GovernanceScore { get; set; }
        public decimal RiskScore { get; set; }
        public decimal ComplianceScore { get; set; }
    }

    public class MaturityRoadmapDto
    {
        public Guid TenantId { get; set; }
        public string CurrentLevel { get; set; } = string.Empty;
        public string TargetLevel { get; set; } = string.Empty;
        public int EstimatedMonthsToTarget { get; set; }
        public List<RoadmapPhaseDto> Phases { get; set; } = new();
        public List<QuickWinDto> QuickWins { get; set; } = new();
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    public class RoadmapPhaseDto
    {
        public int Phase { get; set; }
        public string Name { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DurationMonths { get; set; }
        public List<string> KeyActivities { get; set; } = new();
        public string TargetLevel { get; set; } = string.Empty;
    }

    public class QuickWinDto
    {
        public string Activity { get; set; } = string.Empty;
        public string ActivityAr { get; set; } = string.Empty;
        public string Impact { get; set; } = string.Empty;
        public int EffortDays { get; set; }
        public decimal MaturityImpact { get; set; }
    }

    // ===== Improvement DTOs =====

    public class CreateImprovementDto
    {
        public string Title { get; set; } = string.Empty;
        public string TitleAr { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // Process, Technology, People, Policy
        public string Priority { get; set; } = "Medium";
        public string Owner { get; set; } = string.Empty;
        public DateTime? TargetDate { get; set; }
        public decimal ExpectedImpact { get; set; }
    }

    public class SustainabilityImprovementDto
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public string InitiativeCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string TitleAr { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // Planned, InProgress, Completed, OnHold, Cancelled
        public string StatusAr { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public decimal PercentComplete { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? TargetDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public decimal ExpectedImpact { get; set; }
        public decimal ActualImpact { get; set; }
        public string Outcomes { get; set; } = string.Empty;
    }

    // ===== KPI DTOs =====

    public class GrcKpisDto
    {
        public Guid TenantId { get; set; }
        public decimal ComplianceRate { get; set; }
        public decimal RiskMitigationRate { get; set; }
        public decimal ControlEffectiveness { get; set; }
        public decimal PolicyCompliance { get; set; }
        public decimal AuditFindingsResolved { get; set; }
        public decimal IncidentResponseTime { get; set; }
        public decimal TrainingCompletion { get; set; }
        public decimal AssessmentCoverage { get; set; }
        public decimal VendorCompliance { get; set; }
        public decimal DataProtectionScore { get; set; }
        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    }

    public class KpiTrendDto
    {
        public DateTime Date { get; set; }
        public string KpiName { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public decimal Target { get; set; }
        public bool OnTarget { get; set; }
    }

    // ===== Compliance Health DTOs =====

    public class ComplianceHealthDto
    {
        public Guid TenantId { get; set; }
        public decimal OverallHealth { get; set; } // 0-100
        public string HealthStatus { get; set; } = string.Empty; // Excellent, Good, Fair, Poor, Critical
        public string HealthStatusAr { get; set; } = string.Empty;
        public int ActiveFrameworks { get; set; }
        public int ComplianceGaps { get; set; }
        public int OverdueItems { get; set; }
        public int UpcomingDeadlines { get; set; }
        public decimal CertificationCoverage { get; set; }
        public List<FrameworkHealthDto> FrameworkHealth { get; set; } = new();
        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    }

    public class FrameworkHealthDto
    {
        public string FrameworkCode { get; set; } = string.Empty;
        public string FrameworkName { get; set; } = string.Empty;
        public decimal ComplianceRate { get; set; }
        public int TotalControls { get; set; }
        public int ImplementedControls { get; set; }
        public int Gaps { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class ComplianceForecastDto
    {
        public Guid TenantId { get; set; }
        public List<ForecastPointDto> Forecast { get; set; } = new();
        public decimal ProjectedComplianceRate { get; set; }
        public List<string> Risks { get; set; } = new();
        public List<string> Opportunities { get; set; } = new();
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    public class ForecastPointDto
    {
        public DateTime Date { get; set; }
        public decimal ProjectedCompliance { get; set; }
        public string Confidence { get; set; } = string.Empty;
    }

    // ===== Benchmark DTOs =====

    public class SustainabilityBenchmarkDto
    {
        public Guid TenantId { get; set; }
        public string Industry { get; set; } = string.Empty;
        public string IndustryAr { get; set; } = string.Empty;
        public decimal TenantScore { get; set; }
        public decimal IndustryAverage { get; set; }
        public decimal IndustryTop25 { get; set; }
        public string Percentile { get; set; } = string.Empty;
        public List<BenchmarkCategoryDto> Categories { get; set; } = new();
        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    }

    public class BenchmarkCategoryDto
    {
        public string Category { get; set; } = string.Empty;
        public string CategoryAr { get; set; } = string.Empty;
        public decimal TenantScore { get; set; }
        public decimal IndustryAverage { get; set; }
        public decimal Gap { get; set; }
    }

    public class KsaBenchmarkDto
    {
        public Guid TenantId { get; set; }
        public decimal NcaEccCompliance { get; set; }
        public decimal SamaCsfCompliance { get; set; }
        public decimal PdplCompliance { get; set; }
        public decimal Vision2030Alignment { get; set; }
        public decimal NdmoCompliance { get; set; }
        public string OverallKsaReadiness { get; set; } = string.Empty; // Ready, Partial, NotReady
        public string OverallKsaReadinessAr { get; set; } = string.Empty;
        public List<RegulatoryGapDto> Gaps { get; set; } = new();
        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    }

    public class RegulatoryGapDto
    {
        public string Framework { get; set; } = string.Empty;
        public string Requirement { get; set; } = string.Empty;
        public string Gap { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Remediation { get; set; } = string.Empty;
    }

    // ===== Dashboard DTO =====

    public class SustainabilityDashboardDto
    {
        public Guid TenantId { get; set; }
        public MaturityScoreDto Maturity { get; set; } = new();
        public GrcKpisDto Kpis { get; set; } = new();
        public ComplianceHealthDto Health { get; set; } = new();
        public int ActiveImprovements { get; set; }
        public int CompletedImprovements { get; set; }
        public decimal ImprovementVelocity { get; set; }
        public string OverallTrend { get; set; } = string.Empty; // Improving, Stable, Declining
        public string OverallTrendAr { get; set; } = string.Empty;
        public List<string> TopPriorities { get; set; } = new();
        public List<string> Achievements { get; set; } = new();
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }
}
