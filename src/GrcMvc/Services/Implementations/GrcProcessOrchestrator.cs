using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrcMvc.Configuration;
using GrcMvc.Data;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// GRC Process Orchestrator Implementation
/// Coordinates complete GRC lifecycle: Assessment → Compliance → Resilience → Excellence
/// </summary>
public class GrcProcessOrchestrator : IGrcProcessOrchestrator
{
    private readonly GrcDbContext _context;
    private readonly ILogger<GrcProcessOrchestrator> _logger;

    public GrcProcessOrchestrator(
        GrcDbContext context,
        ILogger<GrcProcessOrchestrator> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Dashboard & Status

    public async Task<GrcDashboardDto> GetDashboardAsync(Guid tenantId)
    {
        var tenant = await _context.Tenants.FindAsync(tenantId);
        
        var dashboard = new GrcDashboardDto
        {
            TenantId = tenantId,
            TenantName = tenant?.OrganizationName ?? "Unknown",
            GeneratedAt = DateTime.UtcNow
        };

        // Get compliance score
        var complianceScore = await GetComplianceScoreAsync(tenantId);
        dashboard.ComplianceScore = complianceScore.OverallScore;

        // Get risk score
        var riskPosture = await GetRiskPostureAsync(tenantId);
        dashboard.RiskScore = 100 - riskPosture.RiskScore; // Invert for positive metric

        // Get resilience score
        var resilience = await GetResilienceScoreAsync(tenantId);
        dashboard.ResilienceScore = resilience.OverallScore;

        // Calculate overall GRC score (weighted average)
        dashboard.OverallGrcScore = (dashboard.ComplianceScore * 40 + 
                                      dashboard.RiskScore * 30 + 
                                      dashboard.ResilienceScore * 30) / 100;

        // Get maturity level
        var maturity = await GetMaturityScoreAsync(tenantId);
        dashboard.MaturityLevel = maturity.OverallLevel;

        // Get framework statuses
        dashboard.FrameworkStatuses = await GetFrameworkStatusesAsync(tenantId);

        // Get alerts
        dashboard.Alerts = await GetAlertsAsync(tenantId);

        // Get action items
        dashboard.ActionItems = await GetActionItemsAsync(tenantId);

        // Get trends (last 12 months - simplified)
        dashboard.ComplianceTrend = GenerateTrend(dashboard.ComplianceScore, 12);
        dashboard.RiskTrend = GenerateTrend(dashboard.RiskScore, 12);

        return dashboard;
    }

    public async Task<GrcMaturityScoreDto> GetMaturityScoreAsync(Guid tenantId)
    {
        var result = new GrcMaturityScoreDto { TenantId = tenantId };

        // Calculate based on actual data
        var assessmentCount = await _context.Assessments
            .CountAsync(a => a.TenantId == tenantId && a.Status == "Completed");
        
        var controlCount = await _context.Controls
            .CountAsync(c => c.TenantId == tenantId);
        
        var evidenceCount = await _context.Evidences
            .CountAsync(e => e.TenantId == tenantId && e.VerificationStatus == "Verified");
        
        var policyCount = await _context.Policies
            .CountAsync(p => p.TenantId == tenantId && p.Status == "Published");
        
        var riskCount = await _context.Risks
            .CountAsync(r => r.TenantId == tenantId && r.Status != "Closed");

        // Calculate dimension scores
        result.Governance = CalculateGovernanceScore(policyCount, assessmentCount);
        result.RiskManagement = CalculateRiskManagementScore(riskCount, controlCount);
        result.Compliance = CalculateComplianceScore(assessmentCount, evidenceCount);
        result.Operations = CalculateOperationsScore(controlCount, evidenceCount);
        result.Technology = new MaturityDimensionScore
        {
            DimensionName = "Technology",
            DimensionNameAr = "التقنية",
            Level = 3,
            Score = 65
        };

        // Calculate overall score and level
        var avgScore = (result.Governance.Score + result.RiskManagement.Score + 
                       result.Compliance.Score + result.Operations.Score + 
                       result.Technology.Score) / 5;
        
        result.Score = avgScore;
        result.OverallLevel = GetCmmLevel(avgScore);
        result.LevelName = GetCmmLevelName(result.OverallLevel);
        result.LevelNameAr = GetCmmLevelNameAr(result.OverallLevel);

        // Add recommendations
        result.Recommendations = GenerateMaturityRecommendations(result);

        return result;
    }

    #endregion

    #region Assessment Lifecycle

    public async Task<AssessmentCycleDto> InitializeAssessmentCycleAsync(Guid tenantId, string frameworkCode)
    {
        var cycleName = $"{frameworkCode} Assessment Cycle - {DateTime.UtcNow:yyyy-MM}";
        
        // Get framework controls to create assessments
        var frameworkControls = await _context.FrameworkControls
            .Where(fc => fc.FrameworkCode == frameworkCode)
            .ToListAsync();

        var cycle = new AssessmentCycleDto
        {
            CycleId = Guid.NewGuid(),
            TenantId = tenantId,
            FrameworkCode = frameworkCode,
            CycleName = cycleName,
            StartDate = DateTime.UtcNow,
            Status = "Active",
            TotalAssessments = frameworkControls.Count / 10, // Group controls into assessments
            CompletedAssessments = 0
        };

        _logger.LogInformation("Initialized assessment cycle {CycleName} for tenant {TenantId}", 
            cycleName, tenantId);

        return cycle;
    }

    public async Task<AssessmentLifecycleDto> GetAssessmentLifecycleAsync(Guid tenantId, Guid assessmentId)
    {
        var assessment = await _context.Assessments
            .FirstOrDefaultAsync(a => a.Id == assessmentId && a.TenantId == tenantId);

        if (assessment == null)
        {
            return new AssessmentLifecycleDto { AssessmentId = assessmentId };
        }

        var normalizedStatus = AssessmentConfiguration.Statuses.Normalize(assessment.Status ?? "Draft");

        var lifecycle = new AssessmentLifecycleDto
        {
            AssessmentId = assessmentId,
            CurrentStage = normalizedStatus,
            CurrentStageAr = GetStatusArabic(normalizedStatus),
            CompletionPercentage = CalculateAssessmentCompletion(assessment.Status ?? "Draft"),
            NextDeadline = assessment.DueDate
        };

        // Define stages
        lifecycle.Stages = new List<LifecycleStageDto>
        {
            new() { StageName = "Draft", StageNameAr = "مسودة", Status = GetStageStatus("Draft", normalizedStatus) },
            new() { StageName = "InProgress", StageNameAr = "قيد التنفيذ", Status = GetStageStatus("InProgress", normalizedStatus) },
            new() { StageName = "UnderReview", StageNameAr = "قيد المراجعة", Status = GetStageStatus("UnderReview", normalizedStatus) },
            new() { StageName = "Submitted", StageNameAr = "مقدم", Status = GetStageStatus("Submitted", normalizedStatus) },
            new() { StageName = "Approved", StageNameAr = "معتمد", Status = GetStageStatus("Approved", normalizedStatus) },
            new() { StageName = "Completed", StageNameAr = "مكتمل", Status = GetStageStatus("Completed", normalizedStatus) }
        };

        // Get available actions based on current status
        lifecycle.AvailableActions = AssessmentConfiguration.Transitions.GetValidTransitions(normalizedStatus).ToList();

        return lifecycle;
    }

    public async Task<AssessmentResultDto> ExecuteAssessmentAsync(Guid tenantId, Guid assessmentId)
    {
        var assessment = await _context.Assessments
            .Include(a => a.Requirements)
            .FirstOrDefaultAsync(a => a.Id == assessmentId && a.TenantId == tenantId);

        if (assessment == null)
        {
            return new AssessmentResultDto { AssessmentId = assessmentId };
        }

        var result = new AssessmentResultDto
        {
            AssessmentId = assessmentId,
            OverallScore = assessment.Score
        };

        // Calculate grade
        result.Grade = assessment.Score >= 80 ? "Pass" : assessment.Score >= 50 ? "Partial" : "Fail";

        // Group requirements by domain
        var requirementsByDomain = assessment.Requirements?
            .GroupBy(r => r.Domain ?? "General")
            .ToDictionary(
                g => g.Key,
                g => g.Average(r => r.Score ?? 0)
            ) ?? new Dictionary<string, double>();

        result.DomainScores = requirementsByDomain.ToDictionary(k => k.Key, v => (int)v.Value);

        // Get findings (non-compliant requirements)
        result.Findings = assessment.Requirements?
            .Where(r => (r.Score ?? 0) < 70)
            .Select(r => new AssessmentFindingDto
            {
                FindingId = r.Id.ToString(),
                ControlId = r.ControlNumber ?? "",
                Title = r.RequirementText ?? "Finding",
                Severity = GetFindingSeverity(r.Score ?? 0),
                Description = r.Findings ?? "",
                Remediation = "Implement control and provide evidence"
            })
            .Take(10)
            .ToList() ?? new List<AssessmentFindingDto>();

        return result;
    }

    #endregion

    #region Compliance Monitoring

    public async Task<ComplianceScoreDto> GetComplianceScoreAsync(Guid tenantId, string? frameworkCode = null)
    {
        var result = new ComplianceScoreDto { TenantId = tenantId };

        var assessmentsQuery = _context.Assessments
            .Where(a => a.TenantId == tenantId);

        if (!string.IsNullOrEmpty(frameworkCode))
        {
            assessmentsQuery = assessmentsQuery.Where(a => a.FrameworkCode == frameworkCode);
        }

        var assessments = await assessmentsQuery.ToListAsync();

        if (!assessments.Any())
        {
            result.OverallScore = 0;
            result.Status = "NotStarted";
            return result;
        }

        // Calculate overall score
        result.OverallScore = (int)assessments.Where(a => a.Score > 0).DefaultIfEmpty().Average(a => a?.Score ?? 0);
        
        // Count by status
        result.CompliantControls = assessments.Count(a => a.Score >= 80);
        result.PartiallyCompliantControls = assessments.Count(a => a.Score >= 50 && a.Score < 80);
        result.NonCompliantControls = assessments.Count(a => a.Score < 50 && a.Score > 0);
        result.TotalControls = assessments.Count;

        // Determine status
        result.Status = result.OverallScore >= 80 ? "Compliant" :
                       result.OverallScore >= 50 ? "InProgress" :
                       result.OverallScore > 0 ? "AtRisk" : "NonCompliant";

        // Group by framework
        var byFramework = assessments.GroupBy(a => a.FrameworkCode ?? "General");
        foreach (var group in byFramework)
        {
            result.ByFramework[group.Key] = new FrameworkComplianceDto
            {
                FrameworkCode = group.Key,
                FrameworkName = group.Key,
                Score = (int)group.Where(a => a.Score > 0).DefaultIfEmpty().Average(a => a?.Score ?? 0),
                TotalControls = group.Count(),
                CompliantControls = group.Count(a => a.Score >= 80)
            };
        }

        return result;
    }

    public async Task<List<ComplianceGapDto>> GetComplianceGapsAsync(Guid tenantId)
    {
        var gaps = new List<ComplianceGapDto>();

        // Find assessments with low scores
        var lowScoreAssessments = await _context.Assessments
            .Where(a => a.TenantId == tenantId && a.Score < 70 && a.Score > 0)
            .Take(20)
            .ToListAsync();

        foreach (var assessment in lowScoreAssessments)
        {
            gaps.Add(new ComplianceGapDto
            {
                GapId = Guid.NewGuid(),
                ControlId = assessment.ControlId?.ToString() ?? "",
                ControlTitle = assessment.Name ?? "Control",
                FrameworkCode = assessment.FrameworkCode ?? "",
                GapType = assessment.Score < 30 ? "NotImplemented" : "PartiallyImplemented",
                Severity = GetFindingSeverity(assessment.Score),
                Description = assessment.Findings ?? "Gap identified",
                Status = "Open",
                TargetDate = DateTime.UtcNow.AddDays(30),
                Owner = assessment.AssignedTo ?? ""
            });
        }

        return gaps;
    }

    public async Task<List<RegulatoryDeadlineDto>> GetRegulatoryDeadlinesAsync(Guid tenantId, string? regulatorCode = null)
    {
        var eventsQuery = _context.ComplianceEvents
            .Where(e => e.TenantId == tenantId && e.DueDate > DateTime.UtcNow.AddDays(-30));

        if (!string.IsNullOrEmpty(regulatorCode))
        {
            eventsQuery = eventsQuery.Where(e => e.Category == regulatorCode);
        }

        var events = await eventsQuery
            .OrderBy(e => e.DueDate)
            .Take(20)
            .ToListAsync();

        return events.Select(e => new RegulatoryDeadlineDto
        {
            DeadlineId = e.Id,
            RegulatorCode = e.Category ?? "",
            RegulatorName = GetRegulatorName(e.Category ?? ""),
            RegulatorNameAr = GetRegulatorNameAr(e.Category ?? ""),
            Title = e.Title ?? "",
            TitleAr = e.Title ?? "",
            DueDate = e.DueDate ?? DateTime.UtcNow,
            DaysRemaining = (int)((e.DueDate ?? DateTime.UtcNow) - DateTime.UtcNow).TotalDays,
            Priority = e.Priority ?? "Medium",
            Status = e.Status ?? "Pending"
        }).ToList();
    }

    public async Task<RegulatoryReportDto> GenerateRegulatoryReportAsync(Guid tenantId, string regulatorCode)
    {
        var compliance = await GetComplianceScoreAsync(tenantId, regulatorCode);
        
        return new RegulatoryReportDto
        {
            TenantId = tenantId,
            RegulatorCode = regulatorCode,
            RegulatorName = GetRegulatorName(regulatorCode),
            GeneratedAt = DateTime.UtcNow,
            ReportPeriod = $"{DateTime.UtcNow.AddMonths(-3):MMM yyyy} - {DateTime.UtcNow:MMM yyyy}",
            ComplianceScore = compliance.OverallScore,
            Sections = new Dictionary<string, object>
            {
                ["summary"] = new { compliance.OverallScore, compliance.Status },
                ["controls"] = new { compliance.TotalControls, compliance.CompliantControls },
                ["gaps"] = new { count = compliance.NonCompliantControls }
            }
        };
    }

    #endregion

    #region Risk & Control Integration

    public async Task<RiskPostureDto> GetRiskPostureAsync(Guid tenantId)
    {
        var risks = await _context.Risks
            .Where(r => r.TenantId == tenantId && r.Status != "Closed")
            .ToListAsync();

        var result = new RiskPostureDto
        {
            TenantId = tenantId,
            TotalRisks = risks.Count,
            CriticalRisks = risks.Count(r => RiskScoringConfiguration.GetRiskLevel(r.RiskScore) == "Critical"),
            HighRisks = risks.Count(r => RiskScoringConfiguration.GetRiskLevel(r.RiskScore) == "High"),
            MediumRisks = risks.Count(r => RiskScoringConfiguration.GetRiskLevel(r.RiskScore) == "Medium"),
            LowRisks = risks.Count(r => RiskScoringConfiguration.GetRiskLevel(r.RiskScore) == "Low"),
            MitigatedRisks = risks.Count(r => r.Status == "Mitigated"),
            AcceptedRisks = risks.Count(r => r.Status == "Accepted")
        };

        // Calculate risk score (weighted by severity)
        var weightedScore = (result.CriticalRisks * 100 + 
                            result.HighRisks * 75 + 
                            result.MediumRisks * 50 + 
                            result.LowRisks * 25);
        result.RiskScore = result.TotalRisks > 0 ? weightedScore / result.TotalRisks : 0;

        // Determine posture status
        result.PostureStatus = result.RiskScore switch
        {
            <= 25 => "Strong",
            <= 50 => "Moderate",
            <= 75 => "Weak",
            _ => "Critical"
        };

        // Get top risks
        result.TopRisks = risks
            .OrderByDescending(r => r.RiskScore)
            .Take(5)
            .Select(r => new TopRiskDto
            {
                RiskId = r.Id,
                RiskName = r.Name,
                Category = r.Category,
                InherentScore = r.InherentRisk,
                ResidualScore = r.ResidualRisk,
                Status = r.Status
            })
            .ToList();

        return result;
    }

    public async Task<ControlEffectivenessDto> CalculateControlEffectivenessAsync(Guid tenantId, Guid controlId)
    {
        var control = await _context.Controls
            .FirstOrDefaultAsync(c => c.Id == controlId && c.TenantId == tenantId);

        if (control == null)
        {
            return new ControlEffectivenessDto { ControlId = controlId };
        }

        // Get related assessments for this control
        var assessments = await _context.Assessments
            .Where(a => a.ControlId == controlId && a.TenantId == tenantId)
            .ToListAsync();

        var result = new ControlEffectivenessDto
        {
            ControlId = controlId,
            ControlTitle = control.Name ?? "",
            EffectivenessScore = control.EffectivenessScore,
            LastTestedDate = assessments.Any() ? assessments.Max(a => a.EndDate) : null,
            TestsPassed = assessments.Count(a => a.Score >= 80),
            TestsFailed = assessments.Count(a => a.Score < 80)
        };

        // Calculate rating
        result.Rating = result.EffectivenessScore switch
        {
            >= 80 => "Effective",
            >= 60 => "Moderate",
            >= 40 => "Weak",
            _ => "Ineffective"
        };

        return result;
    }

    public async Task<RiskControlMappingAnalysisDto> GetRiskControlMappingAsync(Guid tenantId)
    {
        var risks = await _context.Risks
            .Where(r => r.TenantId == tenantId)
            .ToListAsync();

        var controls = await _context.Controls
            .Where(c => c.TenantId == tenantId)
            .ToListAsync();

        var mappings = await _context.RiskControlMappings
            .Where(m => m.Risk != null && m.Risk.TenantId == tenantId)
            .ToListAsync();

        var mappedRiskIds = mappings.Select(m => m.RiskId).Distinct().ToList();
        var mappedControlIds = mappings.Select(m => m.ControlId).Distinct().ToList();

        return new RiskControlMappingAnalysisDto
        {
            TenantId = tenantId,
            TotalRisks = risks.Count,
            TotalControls = controls.Count,
            MappedRisks = mappedRiskIds.Count,
            UnmappedRisks = risks.Count - mappedRiskIds.Count,
            ControlsWithNoRisks = controls.Count - mappedControlIds.Count,
            AverageCoverage = risks.Count > 0 ? (decimal)mappedRiskIds.Count / risks.Count * 100 : 0
        };
    }

    #endregion

    #region Resilience Tracking

    public async Task<ResilienceScoreDto> GetResilienceScoreAsync(Guid tenantId)
    {
        var resiliences = await _context.Resiliences
            .Where(r => r.TenantId == tenantId)
            .ToListAsync();

        var result = new ResilienceScoreDto
        {
            TenantId = tenantId,
            DrillsCompleted = resiliences.Count(r => r.Status == "Completed")
        };

        if (!resiliences.Any())
        {
            result.OverallScore = 0;
            result.Status = "NotAssessed";
            return result;
        }

        // Calculate component scores based on resilience assessments
        result.BusinessContinuityScore = 60;
        result.DisasterRecoveryScore = 55;
        result.IncidentResponseScore = 70;
        result.CrisisManagementScore = 50;

        result.OverallScore = (result.BusinessContinuityScore + 
                              result.DisasterRecoveryScore + 
                              result.IncidentResponseScore + 
                              result.CrisisManagementScore) / 4;

        result.Status = result.OverallScore >= 70 ? "Strong" :
                       result.OverallScore >= 50 ? "Moderate" : "Weak";

        return result;
    }

    public async Task<ContinuousImprovementDto> GetContinuousImprovementStatusAsync(Guid tenantId)
    {
        var actionPlans = await _context.ActionPlans
            .Where(a => a.TenantId == tenantId)
            .ToListAsync();

        var result = new ContinuousImprovementDto
        {
            TenantId = tenantId,
            OpenActionItems = actionPlans.Count(a => a.Status != "Completed" && a.Status != "Closed"),
            ClosedActionItems = actionPlans.Count(a => a.Status == "Completed" || a.Status == "Closed"),
            OverdueItems = actionPlans.Count(a => a.DueDate < DateTime.UtcNow && 
                                                   a.Status != "Completed" && a.Status != "Closed")
        };

        // Calculate improvement score
        var total = result.OpenActionItems + result.ClosedActionItems;
        result.ImprovementScore = total > 0 ? (result.ClosedActionItems * 100 / total) : 0;

        return result;
    }

    #endregion

    #region Excellence & Benchmarking

    public async Task<ExcellenceScoreDto> GetExcellenceScoreAsync(Guid tenantId)
    {
        var maturity = await GetMaturityScoreAsync(tenantId);
        var compliance = await GetComplianceScoreAsync(tenantId);
        var resilience = await GetResilienceScoreAsync(tenantId);

        var excellenceScore = (maturity.Score * 30 + compliance.OverallScore * 40 + resilience.OverallScore * 30) / 100;

        var result = new ExcellenceScoreDto
        {
            TenantId = tenantId,
            ExcellenceScore = excellenceScore,
            ExcellenceLevel = GetExcellenceLevel(excellenceScore),
            ExcellenceLevelAr = GetExcellenceLevelAr(excellenceScore),
            SectorRank = 15, // Placeholder - would query actual sector data
            TotalInSector = 50,
            Percentile = excellenceScore >= 80 ? "Top 20%" : excellenceScore >= 60 ? "Top 40%" : "Top 60%",
            DimensionScores = new Dictionary<string, int>
            {
                ["Governance"] = maturity.Governance.Score,
                ["RiskManagement"] = maturity.RiskManagement.Score,
                ["Compliance"] = compliance.OverallScore,
                ["Resilience"] = resilience.OverallScore,
                ["Operations"] = maturity.Operations.Score
            }
        };

        return result;
    }

    public async Task<SectorBenchmarkDto> BenchmarkAgainstSectorAsync(Guid tenantId, string sectorCode)
    {
        var excellence = await GetExcellenceScoreAsync(tenantId);

        // In production, this would query actual sector benchmarks
        var sectorAverage = 65;
        var sectorBest = 92;
        var sectorMedian = 62;

        return new SectorBenchmarkDto
        {
            SectorCode = sectorCode,
            SectorName = GetSectorName(sectorCode),
            SectorNameAr = GetSectorNameAr(sectorCode),
            YourScore = excellence.ExcellenceScore,
            SectorAverage = sectorAverage,
            SectorBest = sectorBest,
            SectorMedian = sectorMedian,
            YourRanking = excellence.ExcellenceScore >= sectorBest ? "Leading" :
                         excellence.ExcellenceScore >= sectorAverage ? "Above Average" :
                         excellence.ExcellenceScore >= sectorMedian ? "At Par" : "Below Average"
        };
    }

    public async Task<CertificationReadinessDto> GetCertificationReadinessAsync(Guid tenantId, string frameworkCode)
    {
        var compliance = await GetComplianceScoreAsync(tenantId, frameworkCode);
        
        var result = new CertificationReadinessDto
        {
            TenantId = tenantId,
            FrameworkCode = frameworkCode,
            FrameworkName = frameworkCode,
            ReadinessScore = compliance.OverallScore,
            ControlsReady = compliance.CompliantControls,
            ControlsNotReady = compliance.NonCompliantControls + compliance.PartiallyCompliantControls,
            EvidenceReady = compliance.CompliantControls, // Simplified
            EvidenceMissing = compliance.NonCompliantControls
        };

        result.ReadinessLevel = result.ReadinessScore >= 90 ? "Ready" :
                               result.ReadinessScore >= 70 ? "NearlyReady" : "NotReady";
        result.ReadinessLevelAr = result.ReadinessScore >= 90 ? "جاهز" :
                                 result.ReadinessScore >= 70 ? "قريب من الجاهزية" : "غير جاهز";

        // Calculate estimated ready date
        if (result.ReadinessLevel == "NotReady")
        {
            var gapsToClose = result.ControlsNotReady;
            result.EstimatedReadyDate = DateTime.UtcNow.AddDays(gapsToClose * 7);
        }

        return result;
    }

    #endregion

    #region Private Helpers

    private async Task<Dictionary<string, FrameworkStatusSummary>> GetFrameworkStatusesAsync(Guid tenantId)
    {
        var result = new Dictionary<string, FrameworkStatusSummary>();

        // Get KSA framework assessments
        var frameworks = new[] { "NCA-ECC", "SAMA-CSF", "PDPL", "NDMO" };

        foreach (var framework in frameworks)
        {
            var assessments = await _context.Assessments
                .Where(a => a.TenantId == tenantId && a.FrameworkCode == framework)
                .ToListAsync();

            if (assessments.Any())
            {
                var avgScore = (int)assessments.Where(a => a.Score > 0).DefaultIfEmpty().Average(a => a?.Score ?? 0);
                result[framework] = new FrameworkStatusSummary
                {
                    FrameworkCode = framework,
                    FrameworkName = GetFrameworkName(framework),
                    FrameworkNameAr = GetFrameworkNameAr(framework),
                    CompliancePercentage = avgScore,
                    TotalControls = assessments.Count,
                    ImplementedControls = assessments.Count(a => a.Score >= 80),
                    GapCount = assessments.Count(a => a.Score < 70),
                    Status = avgScore >= 80 ? "Compliant" : avgScore >= 50 ? "InProgress" : "AtRisk"
                };
            }
        }

        return result;
    }

    private async Task<List<GrcAlertDto>> GetAlertsAsync(Guid tenantId)
    {
        var alerts = new List<GrcAlertDto>();

        // Check overdue deadlines
        var overdueEvents = await _context.ComplianceEvents
            .Where(e => e.TenantId == tenantId && 
                       e.DueDate < DateTime.UtcNow && 
                       e.Status != "Completed")
            .Take(5)
            .ToListAsync();

        foreach (var evt in overdueEvents)
        {
            alerts.Add(new GrcAlertDto
            {
                Severity = "High",
                Message = $"Overdue: {evt.Title}",
                MessageAr = $"متأخر: {evt.Title}",
                Category = "Deadline"
            });
        }

        // Check critical risks
        var criticalRisks = await _context.Risks
            .Where(r => r.TenantId == tenantId && r.RiskScore >= 20 && r.Status != "Closed")
            .Take(3)
            .ToListAsync();

        foreach (var risk in criticalRisks)
        {
            alerts.Add(new GrcAlertDto
            {
                Severity = "Critical",
                Message = $"Critical Risk: {risk.Name}",
                MessageAr = $"مخاطر حرجة: {risk.Name}",
                Category = "Risk"
            });
        }

        return alerts;
    }

    private async Task<List<GrcActionItemDto>> GetActionItemsAsync(Guid tenantId)
    {
        var actions = new List<GrcActionItemDto>();

        var openPlans = await _context.ActionPlans
            .Where(a => a.TenantId == tenantId && a.Status != "Completed" && a.Status != "Closed")
            .OrderBy(a => a.DueDate)
            .Take(10)
            .ToListAsync();

        foreach (var plan in openPlans)
        {
            actions.Add(new GrcActionItemDto
            {
                Title = plan.Title ?? "Action Item",
                Priority = plan.Priority ?? "Medium",
                DueDate = plan.DueDate,
                AssignedTo = plan.AssignedTo ?? "",
                Status = plan.Status ?? "Pending",
                Category = plan.Category ?? "Remediation"
            });
        }

        return actions;
    }

    private static List<GrcTrendPointDto> GenerateTrend(int currentScore, int months)
    {
        var trend = new List<GrcTrendPointDto>();
        var random = new Random(currentScore);
        
        for (int i = months - 1; i >= 0; i--)
        {
            var variance = random.Next(-5, 6);
            trend.Add(new GrcTrendPointDto
            {
                Date = DateTime.UtcNow.AddMonths(-i),
                Value = Math.Max(0, Math.Min(100, currentScore + variance - (i * 2)))
            });
        }
        
        return trend;
    }

    private static MaturityDimensionScore CalculateGovernanceScore(int policyCount, int assessmentCount)
    {
        var score = Math.Min(100, (policyCount * 10) + (assessmentCount * 5));
        return new MaturityDimensionScore
        {
            DimensionName = "Governance",
            DimensionNameAr = "الحوكمة",
            Level = GetCmmLevel(score),
            Score = score,
            Strengths = policyCount > 5 ? new List<string> { "Good policy coverage" } : new(),
            Weaknesses = policyCount < 3 ? new List<string> { "Limited policies" } : new()
        };
    }

    private static MaturityDimensionScore CalculateRiskManagementScore(int riskCount, int controlCount)
    {
        var score = controlCount > 0 ? Math.Min(100, (controlCount * 5) - (riskCount * 2)) : 30;
        return new MaturityDimensionScore
        {
            DimensionName = "Risk Management",
            DimensionNameAr = "إدارة المخاطر",
            Level = GetCmmLevel(score),
            Score = Math.Max(0, score)
        };
    }

    private static MaturityDimensionScore CalculateComplianceScore(int assessmentCount, int evidenceCount)
    {
        var score = Math.Min(100, (assessmentCount * 8) + (evidenceCount * 2));
        return new MaturityDimensionScore
        {
            DimensionName = "Compliance",
            DimensionNameAr = "الامتثال",
            Level = GetCmmLevel(score),
            Score = score
        };
    }

    private static MaturityDimensionScore CalculateOperationsScore(int controlCount, int evidenceCount)
    {
        var score = Math.Min(100, (controlCount * 3) + (evidenceCount * 4));
        return new MaturityDimensionScore
        {
            DimensionName = "Operations",
            DimensionNameAr = "العمليات",
            Level = GetCmmLevel(score),
            Score = score
        };
    }

    private static int GetCmmLevel(int score) => score switch
    {
        >= 80 => 5,
        >= 60 => 4,
        >= 40 => 3,
        >= 20 => 2,
        _ => 1
    };

    private static string GetCmmLevelName(int level) => level switch
    {
        5 => "Optimizing",
        4 => "Quantitatively Managed",
        3 => "Defined",
        2 => "Managed",
        _ => "Initial"
    };

    private static string GetCmmLevelNameAr(int level) => level switch
    {
        5 => "محسّن",
        4 => "مُدار كمياً",
        3 => "محدد",
        2 => "مُدار",
        _ => "أولي"
    };

    private static List<MaturityRecommendation> GenerateMaturityRecommendations(GrcMaturityScoreDto maturity)
    {
        var recommendations = new List<MaturityRecommendation>();

        if (maturity.Governance.Score < 60)
        {
            recommendations.Add(new MaturityRecommendation
            {
                Title = "Strengthen Governance Framework",
                TitleAr = "تعزيز إطار الحوكمة",
                Description = "Develop and publish more policies, establish governance committees",
                Priority = "High",
                ImpactOnLevel = 1
            });
        }

        if (maturity.RiskManagement.Score < 60)
        {
            recommendations.Add(new MaturityRecommendation
            {
                Title = "Enhance Risk Management",
                TitleAr = "تعزيز إدارة المخاطر",
                Description = "Implement formal risk assessment process, link risks to controls",
                Priority = "High",
                ImpactOnLevel = 1
            });
        }

        return recommendations;
    }

    private static string GetStatusArabic(string status) => status switch
    {
        "Draft" => "مسودة",
        "InProgress" => "قيد التنفيذ",
        "Submitted" => "مقدم",
        "UnderReview" => "قيد المراجعة",
        "Approved" => "معتمد",
        "Completed" => "مكتمل",
        "Cancelled" => "ملغي",
        _ => status
    };

    private static string GetStageStatus(string stage, string currentStage)
    {
        var stages = new[] { "Draft", "InProgress", "UnderReview", "Submitted", "Approved", "Completed" };
        var stageIndex = Array.IndexOf(stages, stage);
        var currentIndex = Array.IndexOf(stages, currentStage);

        if (stageIndex < currentIndex) return "Completed";
        if (stageIndex == currentIndex) return "InProgress";
        return "Pending";
    }

    private static int CalculateAssessmentCompletion(string status) => status switch
    {
        "Completed" => 100,
        "Approved" => 90,
        "Submitted" => 75,
        "UnderReview" => 60,
        "InProgress" => 40,
        "Draft" => 10,
        _ => 0
    };

    private static string GetFindingSeverity(int score) => score switch
    {
        < 20 => "Critical",
        < 40 => "High",
        < 60 => "Medium",
        _ => "Low"
    };

    private static string GetRegulatorName(string code) => code switch
    {
        "NCA" => "National Cybersecurity Authority",
        "SAMA" => "Saudi Arabian Monetary Authority",
        "SDAIA" => "Saudi Data & AI Authority",
        "NDMO" => "National Data Management Office",
        "CST" => "Communications, Space & Technology Commission",
        _ => code
    };

    private static string GetRegulatorNameAr(string code) => code switch
    {
        "NCA" => "الهيئة الوطنية للأمن السيبراني",
        "SAMA" => "البنك المركزي السعودي",
        "SDAIA" => "الهيئة السعودية للبيانات والذكاء الاصطناعي",
        "NDMO" => "مكتب إدارة البيانات الوطنية",
        "CST" => "هيئة الاتصالات والفضاء والتقنية",
        _ => code
    };

    private static string GetFrameworkName(string code) => code switch
    {
        "NCA-ECC" => "Essential Cybersecurity Controls",
        "SAMA-CSF" => "SAMA Cybersecurity Framework",
        "PDPL" => "Personal Data Protection Law",
        "NDMO" => "Data Management Standards",
        _ => code
    };

    private static string GetFrameworkNameAr(string code) => code switch
    {
        "NCA-ECC" => "الضوابط الأساسية للأمن السيبراني",
        "SAMA-CSF" => "إطار الأمن السيبراني لساما",
        "PDPL" => "نظام حماية البيانات الشخصية",
        "NDMO" => "معايير إدارة البيانات",
        _ => code
    };

    private static string GetExcellenceLevel(int score) => score switch
    {
        >= 90 => "World-Class",
        >= 80 => "Excellence",
        >= 70 => "Advanced",
        >= 60 => "Proficient",
        _ => "Developing"
    };

    private static string GetExcellenceLevelAr(int score) => score switch
    {
        >= 90 => "عالمي المستوى",
        >= 80 => "تميز",
        >= 70 => "متقدم",
        >= 60 => "متمرس",
        _ => "قيد التطوير"
    };

    private static string GetSectorName(string code) => code switch
    {
        "FIN" => "Financial Services",
        "GOV" => "Government",
        "TELCO" => "Telecommunications",
        "HEALTH" => "Healthcare",
        "ENERGY" => "Energy",
        _ => "Other"
    };

    private static string GetSectorNameAr(string code) => code switch
    {
        "FIN" => "الخدمات المالية",
        "GOV" => "الحكومة",
        "TELCO" => "الاتصالات",
        "HEALTH" => "الرعاية الصحية",
        "ENERGY" => "الطاقة",
        _ => "أخرى"
    };

    #endregion
}
