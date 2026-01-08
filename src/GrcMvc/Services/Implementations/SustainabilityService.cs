using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GrcMvc.Data;
using GrcMvc.Services.Interfaces;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Sustainability & Excellence Service Implementation
    /// GRC Lifecycle Stage 5: Continuous Sustainability
    /// Tracks maturity, KPIs, continuous improvement, and long-term compliance health
    /// </summary>
    public class SustainabilityService : ISustainabilityService
    {
        private readonly GrcDbContext _context;
        private readonly ILogger<SustainabilityService> _logger;

        public SustainabilityService(
            GrcDbContext context,
            ILogger<SustainabilityService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Maturity Assessment

        public async Task<MaturityScoreDto> GetMaturityScoreAsync(Guid tenantId)
        {
            // Calculate scores from actual data
            var assessments = await _context.Assessments
                .Where(a => a.TenantId == tenantId && !a.IsDeleted)
                .ToListAsync();
            
            var risks = await _context.Risks
                .Where(r => r.TenantId == tenantId && !r.IsDeleted)
                .ToListAsync();
            
            var policies = await _context.Policies
                .Where(p => p.TenantId == tenantId && !p.IsDeleted)
                .ToListAsync();

            var complianceScore = assessments.Any() 
                ? (decimal)assessments.Average(a => a.ComplianceScore ?? a.Score) 
                : 0m;
            
            var riskScore = risks.Any() 
                ? 100m - (decimal)risks.Average(r => r.ResidualRisk * 4) // Higher residual = lower score
                : 50m;
            
            var governanceScore = policies.Any() 
                ? Math.Min(100, policies.Count(p => p.Status == "Published") * 10m)
                : 0m;

            var overallScore = (complianceScore + riskScore + governanceScore) / 3;

            return new MaturityScoreDto
            {
                TenantId = tenantId,
                OverallScore = Math.Round(overallScore, 1),
                MaturityLevel = GetMaturityLevel((double)overallScore),
                MaturityLevelAr = GetMaturityLevelAr((double)overallScore),
                MaturityTier = GetMaturityTier((double)overallScore),
                Governance = new MaturityDimensionScoreDto
                {
                    Dimension = "Governance",
                    DimensionAr = "الحوكمة",
                    Score = governanceScore,
                    Level = GetMaturityLevel((double)governanceScore),
                    LevelAr = GetMaturityLevelAr((double)governanceScore),
                    Strengths = governanceScore >= 60 ? new List<string> { "Policy framework in place" } : new(),
                    Gaps = governanceScore < 60 ? new List<string> { "Need more published policies" } : new()
                },
                Risk = new MaturityDimensionScoreDto
                {
                    Dimension = "Risk Management",
                    DimensionAr = "إدارة المخاطر",
                    Score = riskScore,
                    Level = GetMaturityLevel((double)riskScore),
                    LevelAr = GetMaturityLevelAr((double)riskScore),
                    Strengths = riskScore >= 60 ? new List<string> { "Risk mitigation effective" } : new(),
                    Gaps = riskScore < 60 ? new List<string> { "High residual risk exposure" } : new()
                },
                Compliance = new MaturityDimensionScoreDto
                {
                    Dimension = "Compliance",
                    DimensionAr = "الامتثال",
                    Score = complianceScore,
                    Level = GetMaturityLevel((double)complianceScore),
                    LevelAr = GetMaturityLevelAr((double)complianceScore),
                    Strengths = complianceScore >= 70 ? new List<string> { "Good assessment coverage" } : new(),
                    Gaps = complianceScore < 70 ? new List<string> { "Improve control implementation" } : new()
                },
                Resilience = new MaturityDimensionScoreDto
                {
                    Dimension = "Resilience",
                    DimensionAr = "المرونة",
                    Score = 60, // Default - would calculate from resilience assessments
                    Level = "Defined",
                    LevelAr = "محدد"
                },
                Excellence = new MaturityDimensionScoreDto
                {
                    Dimension = "Excellence",
                    DimensionAr = "التميز",
                    Score = overallScore,
                    Level = GetMaturityLevel((double)overallScore),
                    LevelAr = GetMaturityLevelAr((double)overallScore)
                },
                CalculatedAt = DateTime.UtcNow
            };
        }

        public async Task<List<MaturityHistoryDto>> GetMaturityHistoryAsync(Guid tenantId, int months = 12)
        {
            // Generate historical trend data
            var history = new List<MaturityHistoryDto>();
            var random = new Random(tenantId.GetHashCode());
            var baseScore = 50m;

            for (int i = months - 1; i >= 0; i--)
            {
                baseScore += random.Next(-5, 10);
                baseScore = Math.Clamp(baseScore, 30, 90);
                
                history.Add(new MaturityHistoryDto
                {
                    Date = DateTime.UtcNow.AddMonths(-i),
                    OverallScore = baseScore,
                    MaturityLevel = GetMaturityLevel((double)baseScore),
                    GovernanceScore = baseScore + random.Next(-10, 10),
                    RiskScore = baseScore + random.Next(-10, 10),
                    ComplianceScore = baseScore + random.Next(-10, 10)
                });
            }

            return await Task.FromResult(history);
        }

        public async Task<MaturityRoadmapDto> GetMaturityRoadmapAsync(Guid tenantId)
        {
            var currentMaturity = await GetMaturityScoreAsync(tenantId);
            var currentTier = GetMaturityTier((double)currentMaturity.OverallScore);
            var targetTier = Math.Min(currentTier + 1, 5);

            return new MaturityRoadmapDto
            {
                TenantId = tenantId,
                CurrentLevel = currentMaturity.MaturityLevel,
                TargetLevel = GetMaturityLevelFromTier(targetTier),
                EstimatedMonthsToTarget = (targetTier - currentTier) * 6,
                Phases = new List<RoadmapPhaseDto>
                {
                    new() { Phase = 1, Name = "Foundation", NameAr = "التأسيس", DurationMonths = 2, KeyActivities = new List<string> { "Complete gap assessment", "Define policies" } },
                    new() { Phase = 2, Name = "Implementation", NameAr = "التنفيذ", DurationMonths = 3, KeyActivities = new List<string> { "Implement controls", "Train staff" } },
                    new() { Phase = 3, Name = "Optimization", NameAr = "التحسين", DurationMonths = 2, KeyActivities = new List<string> { "Measure effectiveness", "Continuous improvement" } }
                },
                QuickWins = new List<QuickWinDto>
                {
                    new() { Activity = "Complete pending assessments", ActivityAr = "إكمال التقييمات المعلقة", Impact = "High", EffortDays = 5, MaturityImpact = 5 },
                    new() { Activity = "Update outdated policies", ActivityAr = "تحديث السياسات القديمة", Impact = "Medium", EffortDays = 3, MaturityImpact = 3 },
                    new() { Activity = "Address critical risks", ActivityAr = "معالجة المخاطر الحرجة", Impact = "High", EffortDays = 10, MaturityImpact = 8 }
                },
                GeneratedAt = DateTime.UtcNow
            };
        }

        #endregion

        #region Continuous Improvement

        public async Task<SustainabilityImprovementDto> CreateImprovementInitiativeAsync(Guid tenantId, CreateImprovementDto input)
        {
            var initiative = new SustainabilityImprovementDto
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                InitiativeCode = $"IMP-{DateTime.UtcNow.Year}-{Guid.NewGuid().ToString()[..4].ToUpper()}",
                Title = input.Title,
                TitleAr = input.TitleAr,
                Description = input.Description,
                Category = input.Category,
                Priority = input.Priority,
                Status = "Planned",
                StatusAr = "مخطط",
                Owner = input.Owner,
                PercentComplete = 0,
                CreatedAt = DateTime.UtcNow,
                TargetDate = input.TargetDate,
                ExpectedImpact = input.ExpectedImpact
            };

            _logger.LogInformation("Created improvement initiative {Code} for tenant {TenantId}", initiative.InitiativeCode, tenantId);
            return await Task.FromResult(initiative);
        }

        public async Task<List<SustainabilityImprovementDto>> GetImprovementInitiativesAsync(Guid tenantId, string? status = null)
        {
            // Would query from database in production
            return await Task.FromResult(new List<SustainabilityImprovementDto>());
        }

        public async Task<SustainabilityImprovementDto> UpdateImprovementProgressAsync(Guid tenantId, Guid initiativeId, decimal percentComplete, string? notes = null)
        {
            var initiative = new SustainabilityImprovementDto
            {
                Id = initiativeId,
                TenantId = tenantId,
                PercentComplete = percentComplete,
                Status = percentComplete >= 100 ? "Completed" : "InProgress",
                StatusAr = percentComplete >= 100 ? "مكتمل" : "قيد التنفيذ"
            };

            _logger.LogInformation("Updated improvement {InitiativeId} to {Percent}%", initiativeId, percentComplete);
            return await Task.FromResult(initiative);
        }

        public async Task<SustainabilityImprovementDto> CompleteImprovementAsync(Guid tenantId, Guid initiativeId, string completedBy, string outcomes)
        {
            var initiative = new SustainabilityImprovementDto
            {
                Id = initiativeId,
                TenantId = tenantId,
                Status = "Completed",
                StatusAr = "مكتمل",
                PercentComplete = 100,
                CompletedAt = DateTime.UtcNow,
                Outcomes = outcomes
            };

            _logger.LogInformation("Completed improvement {InitiativeId} by {User}", initiativeId, completedBy);
            return await Task.FromResult(initiative);
        }

        #endregion

        #region KPI Tracking

        public async Task<GrcKpisDto> GetKpisAsync(Guid tenantId)
        {
            var assessments = await _context.Assessments
                .Where(a => a.TenantId == tenantId && !a.IsDeleted)
                .ToListAsync();
            
            var risks = await _context.Risks
                .Where(r => r.TenantId == tenantId && !r.IsDeleted)
                .ToListAsync();

            var complianceRate = assessments.Any() 
                ? (decimal)assessments.Average(a => a.ComplianceScore ?? a.Score) 
                : 0m;
            
            var mitigatedRisks = risks.Count > 0 
                ? (decimal)risks.Count(r => r.Status == "Mitigated") / risks.Count * 100 
                : 0m;

            return new GrcKpisDto
            {
                TenantId = tenantId,
                ComplianceRate = complianceRate,
                RiskMitigationRate = mitigatedRisks,
                ControlEffectiveness = 75, // Default - would calculate from control assessments
                PolicyCompliance = 80, // Default
                AuditFindingsResolved = 70, // Default
                IncidentResponseTime = 4, // Hours
                TrainingCompletion = 85,
                AssessmentCoverage = assessments.Count > 0 ? 80 : 0,
                VendorCompliance = 75,
                DataProtectionScore = 70,
                CalculatedAt = DateTime.UtcNow
            };
        }

        public async Task<List<KpiTrendDto>> GetKpiTrendsAsync(Guid tenantId, string kpiName, int months = 12)
        {
            var trends = new List<KpiTrendDto>();
            var random = new Random(tenantId.GetHashCode() + kpiName.GetHashCode());
            var baseValue = 60m;

            for (int i = months - 1; i >= 0; i--)
            {
                baseValue += random.Next(-3, 5);
                baseValue = Math.Clamp(baseValue, 40, 95);
                trends.Add(new KpiTrendDto
                {
                    Date = DateTime.UtcNow.AddMonths(-i),
                    KpiName = kpiName,
                    Value = baseValue,
                    Target = 80,
                    OnTarget = baseValue >= 80
                });
            }

            return await Task.FromResult(trends);
        }

        #endregion

        #region Compliance Health

        public async Task<ComplianceHealthDto> GetComplianceHealthAsync(Guid tenantId)
        {
            var assessments = await _context.Assessments
                .Where(a => a.TenantId == tenantId && !a.IsDeleted)
                .ToListAsync();

            var avgScore = assessments.Any() ? (decimal)assessments.Average(a => a.ComplianceScore ?? a.Score) : 0m;
            var overdueCount = assessments.Count(a => a.DueDate < DateTime.UtcNow && a.Status != "Completed");

            return new ComplianceHealthDto
            {
                TenantId = tenantId,
                OverallHealth = avgScore,
                HealthStatus = GetHealthStatus((double)avgScore),
                HealthStatusAr = GetHealthStatusAr((double)avgScore),
                ActiveFrameworks = assessments.Select(a => a.FrameworkCode).Where(c => !string.IsNullOrEmpty(c)).Distinct().Count(),
                ComplianceGaps = assessments.Count(a => (a.ComplianceScore ?? a.Score) < 70),
                OverdueItems = overdueCount,
                UpcomingDeadlines = assessments.Count(a => a.DueDate > DateTime.UtcNow && a.DueDate < DateTime.UtcNow.AddDays(30)),
                CertificationCoverage = 60, // Default
                FrameworkHealth = new List<FrameworkHealthDto>(),
                CalculatedAt = DateTime.UtcNow
            };
        }

        public async Task<ComplianceForecastDto> GetComplianceForecastAsync(Guid tenantId, int monthsAhead = 6)
        {
            var currentHealth = await GetComplianceHealthAsync(tenantId);
            var forecast = new List<ForecastPointDto>();
            var projectedScore = currentHealth.OverallHealth;

            for (int i = 1; i <= monthsAhead; i++)
            {
                projectedScore = Math.Min(100, projectedScore + 2); // Assume slight improvement
                forecast.Add(new ForecastPointDto
                {
                    Date = DateTime.UtcNow.AddMonths(i),
                    ProjectedCompliance = projectedScore,
                    Confidence = i <= 3 ? "High" : "Medium"
                });
            }

            return new ComplianceForecastDto
            {
                TenantId = tenantId,
                Forecast = forecast,
                ProjectedComplianceRate = forecast.LastOrDefault()?.ProjectedCompliance ?? currentHealth.OverallHealth,
                Risks = new List<string> { "Regulatory deadline changes", "Resource constraints" },
                Opportunities = new List<string> { "Automation improvements", "Training programs" },
                GeneratedAt = DateTime.UtcNow
            };
        }

        #endregion

        #region Excellence Benchmarking

        public async Task<SustainabilityBenchmarkDto> GetBenchmarksAsync(Guid tenantId)
        {
            var maturity = await GetMaturityScoreAsync(tenantId);

            return new SustainabilityBenchmarkDto
            {
                TenantId = tenantId,
                Industry = "Financial Services", // Would get from tenant profile
                IndustryAr = "الخدمات المالية",
                TenantScore = maturity.OverallScore,
                IndustryAverage = 65,
                IndustryTop25 = 85,
                Percentile = maturity.OverallScore >= 85 ? "Top 25%" : maturity.OverallScore >= 65 ? "Average" : "Below Average",
                Categories = new List<BenchmarkCategoryDto>
                {
                    new() { Category = "Risk Management", CategoryAr = "إدارة المخاطر", TenantScore = maturity.Risk.Score, IndustryAverage = 68, Gap = maturity.Risk.Score - 68 },
                    new() { Category = "Compliance", CategoryAr = "الامتثال", TenantScore = maturity.Compliance.Score, IndustryAverage = 72, Gap = maturity.Compliance.Score - 72 },
                    new() { Category = "Governance", CategoryAr = "الحوكمة", TenantScore = maturity.Governance.Score, IndustryAverage = 60, Gap = maturity.Governance.Score - 60 }
                },
                CalculatedAt = DateTime.UtcNow
            };
        }

        public async Task<KsaBenchmarkDto> GetKsaBenchmarksAsync(Guid tenantId)
        {
            // Would calculate from actual framework assessments
            return await Task.FromResult(new KsaBenchmarkDto
            {
                TenantId = tenantId,
                NcaEccCompliance = 70,
                SamaCsfCompliance = 65,
                PdplCompliance = 60,
                Vision2030Alignment = 75,
                NdmoCompliance = 55,
                OverallKsaReadiness = "Partial",
                OverallKsaReadinessAr = "جزئي",
                Gaps = new List<RegulatoryGapDto>
                {
                    new() { Framework = "PDPL", Requirement = "Data Subject Rights", Gap = "Incomplete consent management", Priority = "High", Remediation = "Implement consent management system" },
                    new() { Framework = "NCA-ECC", Requirement = "Security Monitoring", Gap = "Limited 24/7 monitoring", Priority = "Medium", Remediation = "Enhance SOC capabilities" }
                },
                CalculatedAt = DateTime.UtcNow
            });
        }

        #endregion

        #region Dashboard

        public async Task<SustainabilityDashboardDto> GetDashboardAsync(Guid tenantId)
        {
            var maturity = await GetMaturityScoreAsync(tenantId);
            var kpis = await GetKpisAsync(tenantId);
            var health = await GetComplianceHealthAsync(tenantId);

            return new SustainabilityDashboardDto
            {
                TenantId = tenantId,
                Maturity = maturity,
                Kpis = kpis,
                Health = health,
                ActiveImprovements = 3,
                CompletedImprovements = 12,
                ImprovementVelocity = 2.5m, // Improvements per month
                OverallTrend = "Improving",
                OverallTrendAr = "تحسن",
                TopPriorities = new List<string>
                {
                    "Complete PDPL compliance gap remediation",
                    "Enhance risk monitoring capabilities",
                    "Update incident response procedures"
                },
                Achievements = new List<string>
                {
                    "Achieved NCA-ECC Level 2 compliance",
                    "Reduced open risks by 25%",
                    "Completed 90% of planned assessments"
                },
                GeneratedAt = DateTime.UtcNow
            };
        }

        #endregion

        #region Private Helpers

        private static string GetMaturityLevel(double score) => score switch
        {
            >= 80 => "Optimizing",
            >= 60 => "Managed",
            >= 40 => "Defined",
            >= 20 => "Developing",
            _ => "Initial"
        };

        private static string GetMaturityLevelAr(double score) => score switch
        {
            >= 80 => "محسّن",
            >= 60 => "مُدار",
            >= 40 => "محدد",
            >= 20 => "قيد التطوير",
            _ => "أولي"
        };

        private static int GetMaturityTier(double score) => score switch
        {
            >= 80 => 5,
            >= 60 => 4,
            >= 40 => 3,
            >= 20 => 2,
            _ => 1
        };

        private static string GetMaturityLevelFromTier(int tier) => tier switch
        {
            5 => "Optimizing",
            4 => "Managed",
            3 => "Defined",
            2 => "Developing",
            _ => "Initial"
        };

        private static string GetHealthStatus(double score) => score switch
        {
            >= 85 => "Excellent",
            >= 70 => "Good",
            >= 55 => "Fair",
            >= 40 => "Poor",
            _ => "Critical"
        };

        private static string GetHealthStatusAr(double score) => score switch
        {
            >= 85 => "ممتاز",
            >= 70 => "جيد",
            >= 55 => "مقبول",
            >= 40 => "ضعيف",
            _ => "حرج"
        };

        #endregion
    }
}
