using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrcMvc.Services.Interfaces;

/// <summary>
/// GRC Process Orchestrator - Complete lifecycle management for KSA compliance
/// Coordinates: Assessment → Compliance → Resilience → Excellence
/// </summary>
public interface IGrcProcessOrchestrator
{
    #region Dashboard & Status
    
    /// <summary>
    /// Get complete GRC dashboard with all metrics
    /// </summary>
    Task<GrcDashboardDto> GetDashboardAsync(Guid tenantId);
    
    /// <summary>
    /// Get GRC maturity score (CMM Levels 1-5)
    /// </summary>
    Task<GrcMaturityScoreDto> GetMaturityScoreAsync(Guid tenantId);
    
    #endregion

    #region Assessment Lifecycle
    
    /// <summary>
    /// Initialize assessment cycle for a framework
    /// </summary>
    Task<AssessmentCycleDto> InitializeAssessmentCycleAsync(Guid tenantId, string frameworkCode);
    
    /// <summary>
    /// Get assessment lifecycle status
    /// </summary>
    Task<AssessmentLifecycleDto> GetAssessmentLifecycleAsync(Guid tenantId, Guid assessmentId);
    
    /// <summary>
    /// Execute assessment with scoring
    /// </summary>
    Task<AssessmentResultDto> ExecuteAssessmentAsync(Guid tenantId, Guid assessmentId);
    
    #endregion

    #region Compliance Monitoring
    
    /// <summary>
    /// Get compliance score per framework
    /// </summary>
    Task<ComplianceScoreDto> GetComplianceScoreAsync(Guid tenantId, string? frameworkCode = null);
    
    /// <summary>
    /// Get compliance gaps requiring remediation
    /// </summary>
    Task<List<ComplianceGapDto>> GetComplianceGapsAsync(Guid tenantId);
    
    /// <summary>
    /// Get regulatory deadlines by regulator
    /// </summary>
    Task<List<RegulatoryDeadlineDto>> GetRegulatoryDeadlinesAsync(Guid tenantId, string? regulatorCode = null);
    
    /// <summary>
    /// Generate regulator-specific report (NCA, SAMA, SDAIA)
    /// </summary>
    Task<RegulatoryReportDto> GenerateRegulatoryReportAsync(Guid tenantId, string regulatorCode);
    
    #endregion

    #region Risk & Control Integration
    
    /// <summary>
    /// Get risk posture summary with control effectiveness
    /// </summary>
    Task<RiskPostureDto> GetRiskPostureAsync(Guid tenantId);
    
    /// <summary>
    /// Calculate control effectiveness impacting risks
    /// </summary>
    Task<ControlEffectivenessDto> CalculateControlEffectivenessAsync(Guid tenantId, Guid controlId);
    
    /// <summary>
    /// Get risk-control mapping analysis
    /// </summary>
    Task<RiskControlMappingAnalysisDto> GetRiskControlMappingAsync(Guid tenantId);
    
    #endregion

    #region Resilience Tracking
    
    /// <summary>
    /// Get resilience score (BCM, DR, Incident Response)
    /// </summary>
    Task<ResilienceScoreDto> GetResilienceScoreAsync(Guid tenantId);
    
    /// <summary>
    /// Assess continuous improvement status
    /// </summary>
    Task<ContinuousImprovementDto> GetContinuousImprovementStatusAsync(Guid tenantId);
    
    #endregion

    #region Excellence & Benchmarking
    
    /// <summary>
    /// Get excellence score and KSA sector ranking
    /// </summary>
    Task<ExcellenceScoreDto> GetExcellenceScoreAsync(Guid tenantId);
    
    /// <summary>
    /// Benchmark against KSA sector peers
    /// </summary>
    Task<SectorBenchmarkDto> BenchmarkAgainstSectorAsync(Guid tenantId, string sectorCode);
    
    /// <summary>
    /// Get certification readiness for a framework
    /// </summary>
    Task<CertificationReadinessDto> GetCertificationReadinessAsync(Guid tenantId, string frameworkCode);
    
    #endregion
}

#region DTOs

/// <summary>
/// Complete GRC Dashboard
/// </summary>
public class GrcDashboardDto
{
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    
    // Scores (0-100)
    public int OverallGrcScore { get; set; }
    public int ComplianceScore { get; set; }
    public int RiskScore { get; set; }
    public int ResilienceScore { get; set; }
    public int MaturityLevel { get; set; } // 1-5 CMM
    
    // KSA Regulatory Summary
    public Dictionary<string, FrameworkStatusSummary> FrameworkStatuses { get; set; } = new();
    
    // Alerts & Actions
    public List<GrcAlertDto> Alerts { get; set; } = new();
    public List<GrcActionItemDto> ActionItems { get; set; } = new();
    
    // Trends (last 12 months)
    public List<GrcTrendPointDto> ComplianceTrend { get; set; } = new();
    public List<GrcTrendPointDto> RiskTrend { get; set; } = new();
}

public class FrameworkStatusSummary
{
    public string FrameworkCode { get; set; } = string.Empty;
    public string FrameworkName { get; set; } = string.Empty;
    public string FrameworkNameAr { get; set; } = string.Empty;
    public int CompliancePercentage { get; set; }
    public int TotalControls { get; set; }
    public int ImplementedControls { get; set; }
    public int GapCount { get; set; }
    public DateTime? NextDeadline { get; set; }
    public string Status { get; set; } = "InProgress"; // Compliant, InProgress, AtRisk, NonCompliant
}

public class GrcAlertDto
{
    public string AlertId { get; set; } = Guid.NewGuid().ToString();
    public string Severity { get; set; } = "Medium"; // Critical, High, Medium, Low
    public string Message { get; set; } = string.Empty;
    public string MessageAr { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // Compliance, Risk, Deadline, Evidence
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? ActionUrl { get; set; }
}

public class GrcActionItemDto
{
    public string ActionId { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium";
    public DateTime? DueDate { get; set; }
    public string AssignedTo { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public string Category { get; set; } = string.Empty;
}

public class GrcTrendPointDto
{
    public DateTime Date { get; set; }
    public int Value { get; set; }
}

/// <summary>
/// GRC Maturity Score (CMM 1-5)
/// </summary>
public class GrcMaturityScoreDto
{
    public Guid TenantId { get; set; }
    public int OverallLevel { get; set; } // 1-5
    public string LevelName { get; set; } = "Initial"; // Initial, Managed, Defined, Quantitatively Managed, Optimizing
    public string LevelNameAr { get; set; } = "أولي";
    public int Score { get; set; } // 0-100
    
    // Dimension Scores
    public MaturityDimensionScore Governance { get; set; } = new();
    public MaturityDimensionScore RiskManagement { get; set; } = new();
    public MaturityDimensionScore Compliance { get; set; } = new();
    public MaturityDimensionScore Operations { get; set; } = new();
    public MaturityDimensionScore Technology { get; set; } = new();
    
    // Recommendations
    public List<MaturityRecommendation> Recommendations { get; set; } = new();
}

public class MaturityDimensionScore
{
    public string DimensionName { get; set; } = string.Empty;
    public string DimensionNameAr { get; set; } = string.Empty;
    public int Level { get; set; }
    public int Score { get; set; }
    public List<string> Strengths { get; set; } = new();
    public List<string> Weaknesses { get; set; } = new();
}

public class MaturityRecommendation
{
    public string Title { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium";
    public int ImpactOnLevel { get; set; } // Potential level improvement
}

/// <summary>
/// Assessment Cycle Management
/// </summary>
public class AssessmentCycleDto
{
    public Guid CycleId { get; set; }
    public Guid TenantId { get; set; }
    public string FrameworkCode { get; set; } = string.Empty;
    public string CycleName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = "Active";
    public int TotalAssessments { get; set; }
    public int CompletedAssessments { get; set; }
    public int Progress => TotalAssessments > 0 ? (CompletedAssessments * 100 / TotalAssessments) : 0;
}

public class AssessmentLifecycleDto
{
    public Guid AssessmentId { get; set; }
    public string CurrentStage { get; set; } = "Draft"; // Draft, InProgress, Review, Submitted, Approved, Archived
    public string CurrentStageAr { get; set; } = "مسودة";
    public List<LifecycleStageDto> Stages { get; set; } = new();
    public List<string> AvailableActions { get; set; } = new();
    public DateTime? NextDeadline { get; set; }
    public int CompletionPercentage { get; set; }
}

public class LifecycleStageDto
{
    public string StageName { get; set; } = string.Empty;
    public string StageNameAr { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed, Skipped
    public DateTime? CompletedAt { get; set; }
    public string? CompletedBy { get; set; }
}

public class AssessmentResultDto
{
    public Guid AssessmentId { get; set; }
    public int OverallScore { get; set; }
    public string Grade { get; set; } = "Pass"; // Pass, Fail, Partial
    public Dictionary<string, int> DomainScores { get; set; } = new();
    public List<AssessmentFindingDto> Findings { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

public class AssessmentFindingDto
{
    public string FindingId { get; set; } = string.Empty;
    public string ControlId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Severity { get; set; } = "Medium";
    public string Description { get; set; } = string.Empty;
    public string Remediation { get; set; } = string.Empty;
}

/// <summary>
/// Compliance Score
/// </summary>
public class ComplianceScoreDto
{
    public Guid TenantId { get; set; }
    public int OverallScore { get; set; } // 0-100
    public string Status { get; set; } = "InProgress"; // Compliant, InProgress, AtRisk, NonCompliant
    public Dictionary<string, FrameworkComplianceDto> ByFramework { get; set; } = new();
    public int TotalControls { get; set; }
    public int CompliantControls { get; set; }
    public int PartiallyCompliantControls { get; set; }
    public int NonCompliantControls { get; set; }
    public int NotApplicableControls { get; set; }
}

public class FrameworkComplianceDto
{
    public string FrameworkCode { get; set; } = string.Empty;
    public string FrameworkName { get; set; } = string.Empty;
    public int Score { get; set; }
    public int TotalControls { get; set; }
    public int CompliantControls { get; set; }
    public List<string> TopGaps { get; set; } = new();
}

/// <summary>
/// Compliance Gap
/// </summary>
public class ComplianceGapDto
{
    public Guid GapId { get; set; }
    public string ControlId { get; set; } = string.Empty;
    public string ControlTitle { get; set; } = string.Empty;
    public string FrameworkCode { get; set; } = string.Empty;
    public string GapType { get; set; } = "NotImplemented"; // NotImplemented, PartiallyImplemented, Ineffective
    public string Severity { get; set; } = "Medium";
    public string Description { get; set; } = string.Empty;
    public string RemediationPlan { get; set; } = string.Empty;
    public string Status { get; set; } = "Open"; // Open, InRemediation, Closed, Accepted
    public DateTime? TargetDate { get; set; }
    public string Owner { get; set; } = string.Empty;
}

/// <summary>
/// Regulatory Deadline
/// </summary>
public class RegulatoryDeadlineDto
{
    public Guid DeadlineId { get; set; }
    public string RegulatorCode { get; set; } = string.Empty;
    public string RegulatorName { get; set; } = string.Empty;
    public string RegulatorNameAr { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public int DaysRemaining { get; set; }
    public string Priority { get; set; } = "Medium";
    public string Status { get; set; } = "Pending";
    public bool IsOverdue => DueDate < DateTime.UtcNow && Status != "Completed";
}

/// <summary>
/// Regulatory Report
/// </summary>
public class RegulatoryReportDto
{
    public Guid ReportId { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string RegulatorCode { get; set; } = string.Empty;
    public string RegulatorName { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public string ReportPeriod { get; set; } = string.Empty;
    public int ComplianceScore { get; set; }
    public Dictionary<string, object> Sections { get; set; } = new();
    public byte[]? PdfContent { get; set; }
    public string? ExcelUrl { get; set; }
}

/// <summary>
/// Risk Posture
/// </summary>
public class RiskPostureDto
{
    public Guid TenantId { get; set; }
    public int RiskScore { get; set; } // 0-100 (lower is better)
    public string PostureStatus { get; set; } = "Moderate"; // Strong, Moderate, Weak, Critical
    public int TotalRisks { get; set; }
    public int CriticalRisks { get; set; }
    public int HighRisks { get; set; }
    public int MediumRisks { get; set; }
    public int LowRisks { get; set; }
    public int MitigatedRisks { get; set; }
    public int AcceptedRisks { get; set; }
    public decimal TotalResidualExposure { get; set; }
    public List<TopRiskDto> TopRisks { get; set; } = new();
}

public class TopRiskDto
{
    public Guid RiskId { get; set; }
    public string RiskName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int InherentScore { get; set; }
    public int ResidualScore { get; set; }
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Control Effectiveness
/// </summary>
public class ControlEffectivenessDto
{
    public Guid ControlId { get; set; }
    public string ControlTitle { get; set; } = string.Empty;
    public int EffectivenessScore { get; set; } // 0-100
    public string Rating { get; set; } = "Moderate"; // Effective, Moderate, Weak, Ineffective
    public int TestsPassed { get; set; }
    public int TestsFailed { get; set; }
    public DateTime? LastTestedDate { get; set; }
    public List<Guid> MitigatedRisks { get; set; } = new();
    public int RiskReductionImpact { get; set; }
}

/// <summary>
/// Risk-Control Mapping Analysis
/// </summary>
public class RiskControlMappingAnalysisDto
{
    public Guid TenantId { get; set; }
    public int TotalRisks { get; set; }
    public int TotalControls { get; set; }
    public int MappedRisks { get; set; }
    public int UnmappedRisks { get; set; }
    public int ControlsWithNoRisks { get; set; }
    public decimal AverageCoverage { get; set; }
    public List<RiskControlCoverageDto> ByCatogory { get; set; } = new();
}

public class RiskControlCoverageDto
{
    public string Category { get; set; } = string.Empty;
    public int RiskCount { get; set; }
    public int ControlCount { get; set; }
    public int CoveragePercentage { get; set; }
}

/// <summary>
/// Resilience Score
/// </summary>
public class ResilienceScoreDto
{
    public Guid TenantId { get; set; }
    public int OverallScore { get; set; } // 0-100
    public string Status { get; set; } = "Moderate"; // Strong, Moderate, Weak
    
    // Component Scores
    public int BusinessContinuityScore { get; set; }
    public int DisasterRecoveryScore { get; set; }
    public int IncidentResponseScore { get; set; }
    public int CrisisManagementScore { get; set; }
    
    // KPIs
    public decimal RecoveryTimeObjective { get; set; } // Hours
    public decimal RecoveryPointObjective { get; set; } // Hours
    public DateTime? LastDrillDate { get; set; }
    public int DrillsCompleted { get; set; }
    public int IncidentsLastYear { get; set; }
}

/// <summary>
/// Continuous Improvement
/// </summary>
public class ContinuousImprovementDto
{
    public Guid TenantId { get; set; }
    public int ImprovementScore { get; set; }
    public int OpenActionItems { get; set; }
    public int ClosedActionItems { get; set; }
    public int OverdueItems { get; set; }
    public List<ImprovementInitiativeDto> ActiveInitiatives { get; set; } = new();
    public List<GrcTrendPointDto> ImprovementTrend { get; set; } = new();
}

public class ImprovementInitiativeDto
{
    public Guid InitiativeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Progress { get; set; }
    public DateTime? TargetDate { get; set; }
    public string Status { get; set; } = "InProgress";
}

/// <summary>
/// Excellence Score
/// </summary>
public class ExcellenceScoreDto
{
    public Guid TenantId { get; set; }
    public int ExcellenceScore { get; set; } // 0-100
    public string ExcellenceLevel { get; set; } = "Developing"; // Developing, Proficient, Advanced, Excellence, World-Class
    public string ExcellenceLevelAr { get; set; } = "قيد التطوير";
    public int SectorRank { get; set; }
    public int TotalInSector { get; set; }
    public string Percentile { get; set; } = "Top 50%";
    
    // Excellence Dimensions
    public Dictionary<string, int> DimensionScores { get; set; } = new();
    public List<string> Certifications { get; set; } = new();
    public List<string> Awards { get; set; } = new();
}

/// <summary>
/// Sector Benchmark
/// </summary>
public class SectorBenchmarkDto
{
    public string SectorCode { get; set; } = string.Empty;
    public string SectorName { get; set; } = string.Empty;
    public string SectorNameAr { get; set; } = string.Empty;
    public int YourScore { get; set; }
    public int SectorAverage { get; set; }
    public int SectorBest { get; set; }
    public int SectorMedian { get; set; }
    public string YourRanking { get; set; } = "Above Average";
    public Dictionary<string, BenchmarkComparisonDto> ByDimension { get; set; } = new();
}

public class BenchmarkComparisonDto
{
    public string Dimension { get; set; } = string.Empty;
    public int YourScore { get; set; }
    public int SectorAverage { get; set; }
    public int Difference { get; set; }
    public string Status { get; set; } = "AtPar"; // Leading, AboveAverage, AtPar, BelowAverage, Lagging
}

/// <summary>
/// Certification Readiness
/// </summary>
public class CertificationReadinessDto
{
    public Guid TenantId { get; set; }
    public string FrameworkCode { get; set; } = string.Empty;
    public string FrameworkName { get; set; } = string.Empty;
    public int ReadinessScore { get; set; } // 0-100
    public string ReadinessLevel { get; set; } = "NotReady"; // NotReady, NearlyReady, Ready, Certified
    public string ReadinessLevelAr { get; set; } = "غير جاهز";
    public int ControlsReady { get; set; }
    public int ControlsNotReady { get; set; }
    public int EvidenceReady { get; set; }
    public int EvidenceMissing { get; set; }
    public List<CertificationBlockerDto> Blockers { get; set; } = new();
    public DateTime? EstimatedReadyDate { get; set; }
}

public class CertificationBlockerDto
{
    public string BlockerType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Impact { get; set; } = "High";
    public string Remediation { get; set; } = string.Empty;
    public int EstimatedDays { get; set; }
}

#endregion
