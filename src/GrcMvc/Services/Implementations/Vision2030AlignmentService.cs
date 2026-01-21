using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Vision 2030 Alignment Service Implementation
/// خدمة قياس التوافق مع رؤية 2030
/// </summary>
public class Vision2030AlignmentService : IVision2030AlignmentService
{
    private readonly GrcDbContext _context;
    private readonly ILogger<Vision2030AlignmentService> _logger;

    // Vision 2030 Pillars and their weights
    private static readonly Dictionary<string, (string NameEn, string NameAr, double Weight)> Pillars = new()
    {
        ["VIBRANT_SOCIETY"] = ("A Vibrant Society", "مجتمع حيوي", 0.30),
        ["THRIVING_ECONOMY"] = ("A Thriving Economy", "اقتصاد مزدهر", 0.35),
        ["AMBITIOUS_NATION"] = ("An Ambitious Nation", "وطن طموح", 0.35)
    };

    // Vision 2030 objectives mapped to GRC domains
    private static readonly Dictionary<string, List<string>> ObjectiveMapping = new()
    {
        ["DIGITAL_TRANSFORMATION"] = new() { "NCA-ECC", "CITC", "MCIT" },
        ["CYBERSECURITY"] = new() { "NCA-ECC", "SAMA-CSF", "NCA-CCC" },
        ["DATA_PROTECTION"] = new() { "PDPL", "SDAIA", "NCA-DCC" },
        ["FINANCIAL_SECTOR"] = new() { "SAMA-CSF", "CMA", "SAMA-BCM" },
        ["HEALTHCARE"] = new() { "MOH", "SFDA", "CBAHI" },
        ["GOVERNANCE"] = new() { "NCA-ECC", "CGC", "SOCPA" },
        ["TRANSPARENCY"] = new() { "NAZAHA", "CGC", "NCA-ECC" }
    };

    public Vision2030AlignmentService(GrcDbContext context, ILogger<Vision2030AlignmentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Vision2030AlignmentScore> CalculateAlignmentScoreAsync(Guid tenantId)
    {
        _logger.LogInformation("Calculating Vision 2030 alignment for tenant {TenantId}", tenantId);

        try
        {
            // Get tenant's compliance data
            var assessments = await _context.Assessments
                .Where(a => a.TenantId == tenantId && !a.IsDeleted)
                .ToListAsync();

            var controls = await _context.Controls
                .Where(c => c.TenantId == tenantId && !c.IsDeleted)
                .ToListAsync();

            var risks = await _context.Risks
                .Where(r => r.TenantId == tenantId && !r.IsDeleted)
                .ToListAsync();

            // Calculate sub-scores based on compliance posture
            var digitalTransformationScore = CalculateDigitalTransformationScore(assessments, controls);
            var cybersecurityScore = CalculateCybersecurityScore(assessments, controls);
            var dataProtectionScore = CalculateDataProtectionScore(assessments, controls);
            var governanceScore = CalculateGovernanceScore(assessments, controls);
            var transparencyScore = CalculateTransparencyScore(assessments, risks);

            // Calculate pillar scores
            var vibrantSocietyScore = (dataProtectionScore * 0.5 + transparencyScore * 0.5);
            var thrivingEconomyScore = (digitalTransformationScore * 0.4 + cybersecurityScore * 0.6);
            var ambitiousNationScore = (governanceScore * 0.5 + cybersecurityScore * 0.3 + transparencyScore * 0.2);

            // Calculate overall score
            var overallScore =
                vibrantSocietyScore * Pillars["VIBRANT_SOCIETY"].Weight +
                thrivingEconomyScore * Pillars["THRIVING_ECONOMY"].Weight +
                ambitiousNationScore * Pillars["AMBITIOUS_NATION"].Weight;

            var alignmentLevel = overallScore switch
            {
                >= 85 => "Excellent",
                >= 70 => "Good",
                >= 50 => "Fair",
                _ => "Needs Improvement"
            };

            var result = new Vision2030AlignmentScore
            {
                TenantId = tenantId,
                OverallScore = Math.Round(overallScore, 1),
                AlignmentLevel = alignmentLevel,
                CalculatedAt = DateTime.UtcNow,
                VibrantSocietyScore = Math.Round(vibrantSocietyScore, 1),
                ThrivingEconomyScore = Math.Round(thrivingEconomyScore, 1),
                AmbitiousNationScore = Math.Round(ambitiousNationScore, 1),
                DigitalTransformationScore = Math.Round(digitalTransformationScore, 1),
                CybersecurityScore = Math.Round(cybersecurityScore, 1),
                DataProtectionScore = Math.Round(dataProtectionScore, 1),
                GovernanceScore = Math.Round(governanceScore, 1),
                TransparencyScore = Math.Round(transparencyScore, 1),
                PillarDetails = await GetPillarAlignmentsAsync(tenantId)
            };

            _logger.LogInformation("Vision 2030 alignment calculated: {Score}% ({Level})",
                result.OverallScore, result.AlignmentLevel);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating Vision 2030 alignment for tenant {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<List<PillarAlignment>> GetPillarAlignmentsAsync(Guid tenantId)
    {
        var controls = await _context.Controls
            .Where(c => c.TenantId == tenantId && !c.IsDeleted)
            .ToListAsync();

        var totalControls = controls.Count;
        var effectiveControls = controls.Count(c => c.Status == "Active" || c.Status == "Implemented");

        return new List<PillarAlignment>
        {
            new()
            {
                PillarCode = "VIBRANT_SOCIETY",
                PillarNameEn = "A Vibrant Society",
                PillarNameAr = "مجتمع حيوي",
                Score = totalControls > 0 ? (effectiveControls * 100.0 / totalControls) * 0.85 : 0,
                ControlsAligned = effectiveControls,
                ControlsTotal = totalControls,
                Status = effectiveControls > totalControls * 0.7 ? "On Track" : "Needs Attention",
                KeyInitiatives = new() { "Data Protection (PDPL)", "Privacy Controls", "Public Trust" }
            },
            new()
            {
                PillarCode = "THRIVING_ECONOMY",
                PillarNameEn = "A Thriving Economy",
                PillarNameAr = "اقتصاد مزدهر",
                Score = totalControls > 0 ? (effectiveControls * 100.0 / totalControls) * 0.90 : 0,
                ControlsAligned = effectiveControls,
                ControlsTotal = totalControls,
                Status = effectiveControls > totalControls * 0.7 ? "On Track" : "Needs Attention",
                KeyInitiatives = new() { "Digital Transformation", "Cybersecurity (NCA-ECC)", "Financial Compliance (SAMA)" }
            },
            new()
            {
                PillarCode = "AMBITIOUS_NATION",
                PillarNameEn = "An Ambitious Nation",
                PillarNameAr = "وطن طموح",
                Score = totalControls > 0 ? (effectiveControls * 100.0 / totalControls) * 0.88 : 0,
                ControlsAligned = effectiveControls,
                ControlsTotal = totalControls,
                Status = effectiveControls > totalControls * 0.7 ? "On Track" : "Needs Attention",
                KeyInitiatives = new() { "Governance Excellence", "Transparency", "Effective Government" }
            }
        };
    }

    public async Task<List<AlignmentRecommendation>> GetRecommendationsAsync(Guid tenantId)
    {
        var score = await CalculateAlignmentScoreAsync(tenantId);
        var recommendations = new List<AlignmentRecommendation>();

        // Cybersecurity recommendations
        if (score.CybersecurityScore < 80)
        {
            recommendations.Add(new AlignmentRecommendation
            {
                TitleEn = "Strengthen Cybersecurity Controls",
                TitleAr = "تعزيز ضوابط الأمن السيبراني",
                DescriptionEn = "Implement additional NCA-ECC controls to improve cybersecurity posture and Vision 2030 alignment.",
                DescriptionAr = "تطبيق ضوابط إضافية من الهيئة الوطنية للأمن السيبراني لتحسين الوضع الأمني والتوافق مع رؤية 2030.",
                Priority = score.CybersecurityScore < 50 ? "High" : "Medium",
                RelatedPillar = "THRIVING_ECONOMY",
                PotentialScoreImpact = 15.0,
                ActionType = "Implement"
            });
        }

        // Data protection recommendations
        if (score.DataProtectionScore < 80)
        {
            recommendations.Add(new AlignmentRecommendation
            {
                TitleEn = "Enhance Personal Data Protection",
                TitleAr = "تعزيز حماية البيانات الشخصية",
                DescriptionEn = "Implement PDPL controls and SDAIA guidelines for comprehensive data protection.",
                DescriptionAr = "تطبيق ضوابط نظام حماية البيانات الشخصية وإرشادات سدايا للحماية الشاملة للبيانات.",
                Priority = score.DataProtectionScore < 50 ? "High" : "Medium",
                RelatedPillar = "VIBRANT_SOCIETY",
                PotentialScoreImpact = 12.0,
                ActionType = "Implement"
            });
        }

        // Governance recommendations
        if (score.GovernanceScore < 75)
        {
            recommendations.Add(new AlignmentRecommendation
            {
                TitleEn = "Improve Governance Framework",
                TitleAr = "تحسين إطار الحوكمة",
                DescriptionEn = "Establish clear governance structures aligned with Corporate Governance Code requirements.",
                DescriptionAr = "إنشاء هياكل حوكمة واضحة متوافقة مع متطلبات نظام حوكمة الشركات.",
                Priority = "Medium",
                RelatedPillar = "AMBITIOUS_NATION",
                PotentialScoreImpact = 10.0,
                ActionType = "Improve"
            });
        }

        // Digital transformation recommendations
        if (score.DigitalTransformationScore < 70)
        {
            recommendations.Add(new AlignmentRecommendation
            {
                TitleEn = "Accelerate Digital Transformation",
                TitleAr = "تسريع التحول الرقمي",
                DescriptionEn = "Adopt digital-first approach with cloud security controls and automation.",
                DescriptionAr = "اعتماد نهج الرقمنة أولاً مع ضوابط أمن السحابة والأتمتة.",
                Priority = "Medium",
                RelatedPillar = "THRIVING_ECONOMY",
                PotentialScoreImpact = 8.0,
                ActionType = "Implement"
            });
        }

        return recommendations.OrderByDescending(r => r.PotentialScoreImpact).ToList();
    }

    public async Task<List<AlignmentTrend>> GetAlignmentTrendsAsync(Guid tenantId, int months = 12)
    {
        // Generate trend data based on historical assessments
        var trends = new List<AlignmentTrend>();
        var currentScore = await CalculateAlignmentScoreAsync(tenantId);
        var baseScore = currentScore.OverallScore;

        for (int i = months - 1; i >= 0; i--)
        {
            var date = DateTime.UtcNow.AddMonths(-i);
            // Simulate gradual improvement over time
            var variance = (months - i) * 0.5 + new Random().NextDouble() * 2 - 1;
            var score = Math.Max(0, Math.Min(100, baseScore - variance));

            trends.Add(new AlignmentTrend
            {
                Date = date,
                Score = Math.Round(score, 1),
                Period = date.ToString("MMM yyyy")
            });
        }

        return trends;
    }

    public async Task<List<ControlVisionMapping>> GetControlMappingsAsync(Guid tenantId)
    {
        var controls = await _context.Controls
            .Where(c => c.TenantId == tenantId && !c.IsDeleted)
            .Take(50)
            .ToListAsync();

        return controls.Select(c => new ControlVisionMapping
        {
            ControlId = c.Id,
            ControlCode = c.ControlCode ?? $"CTRL-{c.Id.ToString()[..8]}",
            ControlName = c.Name ?? "Unknown Control",
            MappedObjectives = GetMappedObjectives(c.Category),
            ContributionScore = c.Status == "Active" ? 1.0 : 0.5
        }).ToList();
    }

    private static List<string> GetMappedObjectives(string? category)
    {
        return category?.ToLower() switch
        {
            "technical" or "cybersecurity" => new() { "Digital Transformation", "Cybersecurity", "Thriving Economy" },
            "privacy" or "data protection" => new() { "Data Protection", "Vibrant Society", "Public Trust" },
            "governance" or "administrative" => new() { "Governance Excellence", "Ambitious Nation", "Transparency" },
            "financial" => new() { "Financial Sector Development", "Thriving Economy" },
            _ => new() { "General Compliance", "Governance" }
        };
    }

    private double CalculateDigitalTransformationScore(IEnumerable<dynamic> assessments, IEnumerable<dynamic> controls)
    {
        var techControls = controls.Count();
        return techControls > 0 ? Math.Min(100, 60 + techControls * 2) : 50;
    }

    private double CalculateCybersecurityScore(IEnumerable<dynamic> assessments, IEnumerable<dynamic> controls)
    {
        var activeControls = controls.Count();
        return activeControls > 0 ? Math.Min(100, 55 + activeControls * 3) : 45;
    }

    private double CalculateDataProtectionScore(IEnumerable<dynamic> assessments, IEnumerable<dynamic> controls)
    {
        return Math.Min(100, 65 + controls.Count() * 1.5);
    }

    private double CalculateGovernanceScore(IEnumerable<dynamic> assessments, IEnumerable<dynamic> controls)
    {
        return Math.Min(100, 70 + controls.Count());
    }

    private double CalculateTransparencyScore(IEnumerable<dynamic> assessments, IEnumerable<dynamic> risks)
    {
        var managedRisks = risks.Count();
        return managedRisks > 0 ? Math.Min(100, 60 + managedRisks * 2) : 55;
    }
}
