using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrcMvc.Services.Interfaces;

/// <summary>
/// National Compliance Hub Interface
/// مركز الامتثال الوطني
/// Cross-entity compliance view for government ministries and sectors
/// </summary>
public interface INationalComplianceHub
{
    /// <summary>
    /// Get sector-wide compliance overview
    /// </summary>
    Task<SectorComplianceReport> GetSectorComplianceAsync(string sectorCode);

    /// <summary>
    /// Get cross-entity compliance benchmarking
    /// </summary>
    Task<BenchmarkReport> BenchmarkAgainstSectorAsync(Guid tenantId);

    /// <summary>
    /// Get national compliance statistics
    /// </summary>
    Task<NationalComplianceStats> GetNationalStatsAsync();

    /// <summary>
    /// Get ministry-level dashboard data
    /// </summary>
    Task<MinisterialDashboard> GetMinisterialDashboardAsync(string ministryCode);

    /// <summary>
    /// Get inter-governmental compliance report
    /// </summary>
    Task<G2GComplianceReport> GetG2GReportAsync(Guid tenantId, string targetMinistry);

    /// <summary>
    /// Get regulatory coverage analysis
    /// </summary>
    Task<RegulatoryCoverageAnalysis> GetRegulatoryCoverageAsync(Guid tenantId);
}

/// <summary>
/// Sector-wide compliance report
/// </summary>
public class SectorComplianceReport
{
    public string SectorCode { get; set; } = string.Empty;
    public string SectorNameEn { get; set; } = string.Empty;
    public string SectorNameAr { get; set; } = string.Empty;
    public double AverageComplianceScore { get; set; }
    public int EntitiesCount { get; set; }
    public int FullyCompliantCount { get; set; }
    public int PartiallyCompliantCount { get; set; }
    public int NonCompliantCount { get; set; }
    public List<EntityComplianceSummary> TopPerformers { get; set; } = new();
    public List<EntityComplianceSummary> NeedingAttention { get; set; } = new();
    public List<FrameworkCoverage> FrameworksCoverage { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}

/// <summary>
/// Entity compliance summary
/// </summary>
public class EntityComplianceSummary
{
    public Guid EntityId { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityNameAr { get; set; } = string.Empty;
    public double ComplianceScore { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ControlsImplemented { get; set; }
    public int ControlsTotal { get; set; }
    public DateTime LastAssessmentDate { get; set; }
}

/// <summary>
/// Benchmark report against sector peers
/// </summary>
public class BenchmarkReport
{
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string SectorCode { get; set; } = string.Empty;
    public double YourScore { get; set; }
    public double SectorAverage { get; set; }
    public double SectorTop25Percent { get; set; }
    public int YourRankInSector { get; set; }
    public int TotalInSector { get; set; }
    public string PerformanceCategory { get; set; } = string.Empty; // Leader, Above Average, Average, Below Average
    public List<BenchmarkMetric> Metrics { get; set; } = new();
    public List<string> StrengthsEn { get; set; } = new();
    public List<string> StrengthsAr { get; set; } = new();
    public List<string> ImprovementAreasEn { get; set; } = new();
    public List<string> ImprovementAreasAr { get; set; } = new();
}

/// <summary>
/// Benchmark metric comparison
/// </summary>
public class BenchmarkMetric
{
    public string MetricCode { get; set; } = string.Empty;
    public string MetricNameEn { get; set; } = string.Empty;
    public string MetricNameAr { get; set; } = string.Empty;
    public double YourValue { get; set; }
    public double SectorAverage { get; set; }
    public double SectorBest { get; set; }
    public string Trend { get; set; } = string.Empty; // Up, Down, Stable
}

/// <summary>
/// National compliance statistics
/// </summary>
public class NationalComplianceStats
{
    public int TotalOrganizations { get; set; }
    public int TotalSectors { get; set; }
    public double NationalAverageScore { get; set; }
    public int FullyCompliantOrgs { get; set; }
    public double ComplianceGrowthPercent { get; set; }
    public List<SectorSummary> SectorSummaries { get; set; } = new();
    public List<FrameworkAdoption> FrameworkAdoptions { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}

/// <summary>
/// Sector summary for national stats
/// </summary>
public class SectorSummary
{
    public string SectorCode { get; set; } = string.Empty;
    public string SectorNameEn { get; set; } = string.Empty;
    public string SectorNameAr { get; set; } = string.Empty;
    public int EntityCount { get; set; }
    public double AverageScore { get; set; }
}

/// <summary>
/// Framework adoption statistics
/// </summary>
public class FrameworkAdoption
{
    public string FrameworkCode { get; set; } = string.Empty;
    public string FrameworkName { get; set; } = string.Empty;
    public int AdoptedByCount { get; set; }
    public double AdoptionPercentage { get; set; }
}

/// <summary>
/// Framework coverage analysis
/// </summary>
public class FrameworkCoverage
{
    public string FrameworkCode { get; set; } = string.Empty;
    public string FrameworkName { get; set; } = string.Empty;
    public int TotalControls { get; set; }
    public int ImplementedControls { get; set; }
    public double CoveragePercent { get; set; }
}

/// <summary>
/// Ministerial dashboard data
/// </summary>
public class MinisterialDashboard
{
    public string MinistryCode { get; set; } = string.Empty;
    public string MinistryNameEn { get; set; } = string.Empty;
    public string MinistryNameAr { get; set; } = string.Empty;

    // Key metrics for minister view
    public double OverallComplianceScore { get; set; }
    public double Vision2030AlignmentScore { get; set; }
    public int CriticalRisksCount { get; set; }
    public int OverdueActionsCount { get; set; }
    public int PendingApprovalsCount { get; set; }

    // Trends
    public double ComplianceTrendPercent { get; set; }
    public string TrendDirection { get; set; } = string.Empty;

    // Subordinate entities
    public int SubordinateEntitiesCount { get; set; }
    public int EntitiesOnTrack { get; set; }
    public int EntitiesNeedingAttention { get; set; }

    // Recent activity
    public List<RecentActivity> RecentActivities { get; set; } = new();

    // Upcoming deadlines
    public List<UpcomingDeadline> UpcomingDeadlines { get; set; } = new();
}

/// <summary>
/// Recent activity item
/// </summary>
public class RecentActivity
{
    public DateTime Timestamp { get; set; }
    public string ActivityType { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
}

/// <summary>
/// Upcoming deadline item
/// </summary>
public class UpcomingDeadline
{
    public DateTime DueDate { get; set; }
    public string TitleEn { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
    public string Regulator { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public int DaysRemaining { get; set; }
}

/// <summary>
/// G2G (Government-to-Government) compliance report
/// </summary>
public class G2GComplianceReport
{
    public Guid SourceEntityId { get; set; }
    public string SourceEntityName { get; set; } = string.Empty;
    public string TargetMinistry { get; set; } = string.Empty;
    public DateTime ReportDate { get; set; }

    public double ComplianceScore { get; set; }
    public string ComplianceStatus { get; set; } = string.Empty;

    public List<G2GRequirement> Requirements { get; set; } = new();
    public string ExecutiveSummaryEn { get; set; } = string.Empty;
    public string ExecutiveSummaryAr { get; set; } = string.Empty;
}

/// <summary>
/// G2G requirement status
/// </summary>
public class G2GRequirement
{
    public string RequirementCode { get; set; } = string.Empty;
    public string RequirementNameEn { get; set; } = string.Empty;
    public string RequirementNameAr { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? CompletedDate { get; set; }
    public string Evidence { get; set; } = string.Empty;
}

/// <summary>
/// Regulatory coverage analysis
/// </summary>
public class RegulatoryCoverageAnalysis
{
    public Guid TenantId { get; set; }
    public int TotalRegulators { get; set; }
    public int ApplicableRegulators { get; set; }
    public int FullyCoveredRegulators { get; set; }
    public int PartiallyCoveredRegulators { get; set; }
    public int NotCoveredRegulators { get; set; }
    public double OverallCoveragePercent { get; set; }
    public List<RegulatorCoverage> RegulatorDetails { get; set; } = new();
}

/// <summary>
/// Individual regulator coverage
/// </summary>
public class RegulatorCoverage
{
    public string RegulatorCode { get; set; } = string.Empty;
    public string RegulatorName { get; set; } = string.Empty;
    public string RegulatorNameAr { get; set; } = string.Empty;
    public bool IsApplicable { get; set; }
    public int TotalRequirements { get; set; }
    public int MetRequirements { get; set; }
    public double CoveragePercent { get; set; }
    public string Status { get; set; } = string.Empty;
}
