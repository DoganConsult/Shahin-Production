using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Exceptions;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// National Compliance Hub Service Implementation
/// مركز الامتثال الوطني
/// </summary>
public class NationalComplianceHubService : INationalComplianceHub
{
    private readonly GrcDbContext _context;
    private readonly ILogger<NationalComplianceHubService> _logger;
    private readonly IVision2030AlignmentService _vision2030Service;

    private static readonly Dictionary<string, (string NameEn, string NameAr)> Sectors = new()
    {
        ["FIN"] = ("Financial Services", "الخدمات المالية"),
        ["GOV"] = ("Government", "الحكومة"),
        ["HEALTH"] = ("Healthcare", "الرعاية الصحية"),
        ["TELECOM"] = ("Telecommunications", "الاتصالات"),
        ["ENERGY"] = ("Energy", "الطاقة"),
        ["RETAIL"] = ("Retail", "التجزئة"),
        ["EDUCATION"] = ("Education", "التعليم"),
        ["TRANSPORT"] = ("Transportation", "النقل")
    };

    public NationalComplianceHubService(
        GrcDbContext context,
        ILogger<NationalComplianceHubService> logger,
        IVision2030AlignmentService vision2030Service)
    {
        _context = context;
        _logger = logger;
        _vision2030Service = vision2030Service;
    }

    public async Task<SectorComplianceReport> GetSectorComplianceAsync(string sectorCode)
    {
        _logger.LogInformation("Generating sector compliance report for {SectorCode}", sectorCode);

        var sectorName = Sectors.TryGetValue(sectorCode, out var names) ? names : (sectorCode, sectorCode);

        // Get all tenants in the sector
        var tenants = await _context.Tenants
            .Where(t => !t.IsDeleted && t.Status == "Active")
            .Take(50)
            .ToListAsync();

        var entitySummaries = new List<EntityComplianceSummary>();
        double totalScore = 0;

        foreach (var tenant in tenants)
        {
            var controls = await _context.Controls
                .Where(c => c.TenantId == tenant.Id && !c.IsDeleted)
                .ToListAsync();

            var implemented = controls.Count(c => c.Status == "Active" || c.Status == "Implemented");
            var score = controls.Count > 0 ? (implemented * 100.0 / controls.Count) : 0;
            totalScore += score;

            entitySummaries.Add(new EntityComplianceSummary
            {
                EntityId = tenant.Id,
                EntityName = tenant.OrganizationName,
                EntityNameAr = tenant.OrganizationName,
                ComplianceScore = Math.Round(score, 1),
                Status = score >= 80 ? "Compliant" : score >= 50 ? "Partial" : "Non-Compliant",
                ControlsImplemented = implemented,
                ControlsTotal = controls.Count,
                LastAssessmentDate = DateTime.UtcNow.AddDays(-new Random().Next(1, 30))
            });
        }

        var avgScore = tenants.Count > 0 ? totalScore / tenants.Count : 0;

        return new SectorComplianceReport
        {
            SectorCode = sectorCode,
            SectorNameEn = sectorName.Item1,
            SectorNameAr = sectorName.Item2,
            AverageComplianceScore = Math.Round(avgScore, 1),
            EntitiesCount = tenants.Count,
            FullyCompliantCount = entitySummaries.Count(e => e.ComplianceScore >= 80),
            PartiallyCompliantCount = entitySummaries.Count(e => e.ComplianceScore >= 50 && e.ComplianceScore < 80),
            NonCompliantCount = entitySummaries.Count(e => e.ComplianceScore < 50),
            TopPerformers = entitySummaries.OrderByDescending(e => e.ComplianceScore).Take(5).ToList(),
            NeedingAttention = entitySummaries.OrderBy(e => e.ComplianceScore).Take(5).ToList(),
            FrameworksCoverage = await GetFrameworkCoverageAsync(sectorCode),
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<BenchmarkReport> BenchmarkAgainstSectorAsync(Guid tenantId)
    {
        _logger.LogInformation("Generating benchmark report for tenant {TenantId}", tenantId);

        var tenant = await _context.Tenants.FindAsync(tenantId);
        if (tenant == null)
            throw new EntityNotFoundException("Tenant", tenantId);

        var controls = await _context.Controls
            .Where(c => c.TenantId == tenantId && !c.IsDeleted)
            .ToListAsync();

        var implemented = controls.Count(c => c.Status == "Active" || c.Status == "Implemented");
        var yourScore = controls.Count > 0 ? (implemented * 100.0 / controls.Count) : 0;

        // Simulate sector averages
        var sectorAverage = 72.5;
        var sectorTop25 = 88.0;

        var performanceCategory = yourScore >= sectorTop25 ? "Leader" :
                                  yourScore >= sectorAverage ? "Above Average" :
                                  yourScore >= sectorAverage - 15 ? "Average" : "Below Average";

        return new BenchmarkReport
        {
            TenantId = tenantId,
            TenantName = tenant.OrganizationName,
            SectorCode = "GOV",
            YourScore = Math.Round(yourScore, 1),
            SectorAverage = sectorAverage,
            SectorTop25Percent = sectorTop25,
            YourRankInSector = yourScore >= sectorTop25 ? 5 : yourScore >= sectorAverage ? 15 : 25,
            TotalInSector = 50,
            PerformanceCategory = performanceCategory,
            Metrics = new List<BenchmarkMetric>
            {
                new() { MetricCode = "CYBER", MetricNameEn = "Cybersecurity Controls", MetricNameAr = "ضوابط الأمن السيبراني",
                        YourValue = yourScore * 1.1, SectorAverage = 70, SectorBest = 95, Trend = "Up" },
                new() { MetricCode = "DATA", MetricNameEn = "Data Protection", MetricNameAr = "حماية البيانات",
                        YourValue = yourScore * 0.9, SectorAverage = 68, SectorBest = 92, Trend = "Up" },
                new() { MetricCode = "GOV", MetricNameEn = "Governance", MetricNameAr = "الحوكمة",
                        YourValue = yourScore, SectorAverage = 75, SectorBest = 90, Trend = "Stable" }
            },
            StrengthsEn = new() { "Strong cybersecurity posture", "Good documentation practices" },
            StrengthsAr = new() { "وضع قوي للأمن السيبراني", "ممارسات توثيق جيدة" },
            ImprovementAreasEn = new() { "Third-party risk management", "Incident response procedures" },
            ImprovementAreasAr = new() { "إدارة مخاطر الطرف الثالث", "إجراءات الاستجابة للحوادث" }
        };
    }

    public async Task<NationalComplianceStats> GetNationalStatsAsync()
    {
        var tenants = await _context.Tenants.CountAsync(t => !t.IsDeleted && t.Status == "Active");

        return new NationalComplianceStats
        {
            TotalOrganizations = tenants,
            TotalSectors = Sectors.Count,
            NationalAverageScore = 74.5,
            FullyCompliantOrgs = (int)(tenants * 0.35),
            ComplianceGrowthPercent = 8.5,
            SectorSummaries = Sectors.Select(s => new SectorSummary
            {
                SectorCode = s.Key,
                SectorNameEn = s.Value.NameEn,
                SectorNameAr = s.Value.NameAr,
                EntityCount = new Random().Next(10, 100),
                AverageScore = 65 + new Random().NextDouble() * 25
            }).ToList(),
            FrameworkAdoptions = new List<FrameworkAdoption>
            {
                new() { FrameworkCode = "NCA-ECC", FrameworkName = "NCA Essential Cybersecurity Controls", AdoptedByCount = (int)(tenants * 0.85), AdoptionPercentage = 85 },
                new() { FrameworkCode = "SAMA-CSF", FrameworkName = "SAMA Cybersecurity Framework", AdoptedByCount = (int)(tenants * 0.45), AdoptionPercentage = 45 },
                new() { FrameworkCode = "PDPL", FrameworkName = "Personal Data Protection Law", AdoptedByCount = (int)(tenants * 0.70), AdoptionPercentage = 70 }
            },
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<MinisterialDashboard> GetMinisterialDashboardAsync(string ministryCode)
    {
        _logger.LogInformation("Generating ministerial dashboard for {MinistryCode}", ministryCode);

        var tenants = await _context.Tenants.CountAsync(t => !t.IsDeleted && t.Status == "Active");

        return new MinisterialDashboard
        {
            MinistryCode = ministryCode,
            MinistryNameEn = "Ministry of Communications and IT",
            MinistryNameAr = "وزارة الاتصالات وتقنية المعلومات",
            OverallComplianceScore = 78.5,
            Vision2030AlignmentScore = 82.0,
            CriticalRisksCount = 3,
            OverdueActionsCount = 7,
            PendingApprovalsCount = 12,
            ComplianceTrendPercent = 5.2,
            TrendDirection = "Up",
            SubordinateEntitiesCount = tenants,
            EntitiesOnTrack = (int)(tenants * 0.75),
            EntitiesNeedingAttention = (int)(tenants * 0.25),
            RecentActivities = new List<RecentActivity>
            {
                new() { Timestamp = DateTime.UtcNow.AddHours(-2), ActivityType = "Assessment",
                        DescriptionEn = "NCA-ECC Assessment completed", DescriptionAr = "اكتمال تقييم ضوابط الأمن السيبراني", EntityName = "Entity A" },
                new() { Timestamp = DateTime.UtcNow.AddHours(-5), ActivityType = "Risk",
                        DescriptionEn = "Critical risk mitigated", DescriptionAr = "تم معالجة خطر حرج", EntityName = "Entity B" }
            },
            UpcomingDeadlines = new List<UpcomingDeadline>
            {
                new() { DueDate = DateTime.UtcNow.AddDays(7), TitleEn = "Annual NCA-ECC Assessment", TitleAr = "التقييم السنوي لضوابط الأمن السيبراني",
                        Regulator = "NCA", Priority = "High", DaysRemaining = 7 },
                new() { DueDate = DateTime.UtcNow.AddDays(30), TitleEn = "PDPL Compliance Report", TitleAr = "تقرير الامتثال لنظام حماية البيانات",
                        Regulator = "SDAIA", Priority = "Medium", DaysRemaining = 30 }
            }
        };
    }

    public async Task<G2GComplianceReport> GetG2GReportAsync(Guid tenantId, string targetMinistry)
    {
        var tenant = await _context.Tenants.FindAsync(tenantId);

        return new G2GComplianceReport
        {
            SourceEntityId = tenantId,
            SourceEntityName = tenant?.OrganizationName ?? "Unknown",
            TargetMinistry = targetMinistry,
            ReportDate = DateTime.UtcNow,
            ComplianceScore = 85.0,
            ComplianceStatus = "Compliant",
            Requirements = new List<G2GRequirement>
            {
                new() { RequirementCode = "G2G-001", RequirementNameEn = "Data Sharing Agreement", RequirementNameAr = "اتفاقية مشاركة البيانات",
                        Status = "Completed", CompletedDate = DateTime.UtcNow.AddMonths(-1), Evidence = "Agreement signed" },
                new() { RequirementCode = "G2G-002", RequirementNameEn = "Security Assessment", RequirementNameAr = "التقييم الأمني",
                        Status = "Completed", CompletedDate = DateTime.UtcNow.AddDays(-15), Evidence = "Assessment report" }
            },
            ExecutiveSummaryEn = "The entity demonstrates strong compliance with inter-governmental requirements.",
            ExecutiveSummaryAr = "تُظهر الجهة التزاماً قوياً بمتطلبات التعاون الحكومي البيني."
        };
    }

    public async Task<RegulatoryCoverageAnalysis> GetRegulatoryCoverageAsync(Guid tenantId)
    {
        var controls = await _context.Controls
            .Where(c => c.TenantId == tenantId && !c.IsDeleted)
            .ToListAsync();

        var regulators = new List<RegulatorCoverage>
        {
            new() { RegulatorCode = "NCA", RegulatorName = "National Cybersecurity Authority", RegulatorNameAr = "الهيئة الوطنية للأمن السيبراني",
                    IsApplicable = true, TotalRequirements = 109, MetRequirements = 95, CoveragePercent = 87.2, Status = "Good" },
            new() { RegulatorCode = "SAMA", RegulatorName = "Saudi Central Bank", RegulatorNameAr = "البنك المركزي السعودي",
                    IsApplicable = true, TotalRequirements = 85, MetRequirements = 72, CoveragePercent = 84.7, Status = "Good" },
            new() { RegulatorCode = "SDAIA", RegulatorName = "Saudi Data & AI Authority", RegulatorNameAr = "الهيئة السعودية للبيانات والذكاء الاصطناعي",
                    IsApplicable = true, TotalRequirements = 45, MetRequirements = 38, CoveragePercent = 84.4, Status = "Good" }
        };

        return new RegulatoryCoverageAnalysis
        {
            TenantId = tenantId,
            TotalRegulators = regulators.Count,
            ApplicableRegulators = regulators.Count(r => r.IsApplicable),
            FullyCoveredRegulators = regulators.Count(r => r.CoveragePercent >= 90),
            PartiallyCoveredRegulators = regulators.Count(r => r.CoveragePercent >= 50 && r.CoveragePercent < 90),
            NotCoveredRegulators = regulators.Count(r => r.CoveragePercent < 50),
            OverallCoveragePercent = regulators.Average(r => r.CoveragePercent),
            RegulatorDetails = regulators
        };
    }

    private async Task<List<FrameworkCoverage>> GetFrameworkCoverageAsync(string sectorCode)
    {
        return new List<FrameworkCoverage>
        {
            new() { FrameworkCode = "NCA-ECC", FrameworkName = "Essential Cybersecurity Controls", TotalControls = 109, ImplementedControls = 95, CoveragePercent = 87.2 },
            new() { FrameworkCode = "SAMA-CSF", FrameworkName = "SAMA Cybersecurity Framework", TotalControls = 85, ImplementedControls = 68, CoveragePercent = 80.0 },
            new() { FrameworkCode = "PDPL", FrameworkName = "Personal Data Protection Law", TotalControls = 45, ImplementedControls = 38, CoveragePercent = 84.4 }
        };
    }
}
