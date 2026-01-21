using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Vision 2030 Alignment Service Interface
/// خدمة قياس التوافق مع رؤية 2030
/// Tracks organization alignment with Saudi Vision 2030 objectives
/// </summary>
public interface IVision2030AlignmentService
{
    /// <summary>
    /// Calculate overall Vision 2030 alignment score for a tenant
    /// </summary>
    Task<Vision2030AlignmentScore> CalculateAlignmentScoreAsync(Guid tenantId);

    /// <summary>
    /// Get alignment by specific Vision 2030 pillar
    /// </summary>
    Task<List<PillarAlignment>> GetPillarAlignmentsAsync(Guid tenantId);

    /// <summary>
    /// Get recommended actions to improve alignment
    /// </summary>
    Task<List<AlignmentRecommendation>> GetRecommendationsAsync(Guid tenantId);

    /// <summary>
    /// Track progress over time
    /// </summary>
    Task<List<AlignmentTrend>> GetAlignmentTrendsAsync(Guid tenantId, int months = 12);

    /// <summary>
    /// Map controls to Vision 2030 objectives
    /// </summary>
    Task<List<ControlVisionMapping>> GetControlMappingsAsync(Guid tenantId);
}

/// <summary>
/// Vision 2030 alignment score result
/// </summary>
public class Vision2030AlignmentScore
{
    public Guid TenantId { get; set; }
    public double OverallScore { get; set; } // 0-100
    public string AlignmentLevel { get; set; } = string.Empty; // Excellent, Good, Fair, Needs Improvement
    public DateTime CalculatedAt { get; set; }

    // Pillar scores
    public double VibrantSocietyScore { get; set; } // مجتمع حيوي
    public double ThrivingEconomyScore { get; set; } // اقتصاد مزدهر
    public double AmbitiousNationScore { get; set; } // وطن طموح

    // Sub-scores
    public double DigitalTransformationScore { get; set; }
    public double CybersecurityScore { get; set; }
    public double DataProtectionScore { get; set; }
    public double GovernanceScore { get; set; }
    public double TransparencyScore { get; set; }

    public List<PillarAlignment> PillarDetails { get; set; } = new();
}

/// <summary>
/// Individual pillar alignment details
/// </summary>
public class PillarAlignment
{
    public string PillarCode { get; set; } = string.Empty;
    public string PillarNameEn { get; set; } = string.Empty;
    public string PillarNameAr { get; set; } = string.Empty;
    public double Score { get; set; }
    public int ControlsAligned { get; set; }
    public int ControlsTotal { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<string> KeyInitiatives { get; set; } = new();
}

/// <summary>
/// Recommendation for improving alignment
/// </summary>
public class AlignmentRecommendation
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TitleEn { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty; // High, Medium, Low
    public string RelatedPillar { get; set; } = string.Empty;
    public double PotentialScoreImpact { get; set; }
    public string ActionType { get; set; } = string.Empty; // Implement, Improve, Review
}

/// <summary>
/// Alignment trend over time
/// </summary>
public class AlignmentTrend
{
    public DateTime Date { get; set; }
    public double Score { get; set; }
    public string Period { get; set; } = string.Empty;
}

/// <summary>
/// Control to Vision 2030 objective mapping
/// </summary>
public class ControlVisionMapping
{
    public Guid ControlId { get; set; }
    public string ControlCode { get; set; } = string.Empty;
    public string ControlName { get; set; } = string.Empty;
    public List<string> MappedObjectives { get; set; } = new();
    public double ContributionScore { get; set; }
}
