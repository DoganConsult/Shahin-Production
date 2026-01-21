using GrcMvc.Models.DTOs;

namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Security Agent Service - AI-powered security monitoring and threat analysis
/// </summary>
public interface ISecurityAgentService
{
    /// <summary>
    /// Analyze security posture for a tenant
    /// </summary>
    Task<SecurityPostureAnalysis> AnalyzeSecurityPostureAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Detect potential security threats in audit logs
    /// </summary>
    Task<ThreatDetectionResult> DetectThreatsAsync(
        Guid tenantId,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyze access patterns for anomalies
    /// </summary>
    Task<AccessAnomalyResult> AnalyzeAccessPatternsAsync(
        Guid tenantId,
        Guid? userId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Recommend security controls based on risks
    /// </summary>
    Task<SecurityControlRecommendation> RecommendSecurityControlsAsync(
        Guid tenantId,
        string? frameworkCode = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyze incident response effectiveness
    /// </summary>
    Task<IncidentResponseAnalysis> AnalyzeIncidentResponseAsync(
        Guid incidentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if service is available
    /// </summary>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Security posture analysis result
/// </summary>
public class SecurityPostureAnalysis
{
    public required string OverallRating { get; set; } // Critical, High, Medium, Low
    public int Score { get; set; } // 0-100
    public required List<SecurityFinding> Findings { get; set; }
    public required List<string> Recommendations { get; set; }
    public required Dictionary<string, int> RiskBreakdown { get; set; }
    public DateTime AnalyzedAt { get; set; }
}

/// <summary>
/// Security finding
/// </summary>
public class SecurityFinding
{
    public required string Category { get; set; }
    public required string Severity { get; set; }
    public required string Description { get; set; }
    public required string Recommendation { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }
}

/// <summary>
/// Threat detection result
/// </summary>
public class ThreatDetectionResult
{
    public int TotalThreatsDetected { get; set; }
    public required List<ThreatIndicator> Threats { get; set; }
    public required List<string> RecommendedActions { get; set; }
    public DateTime AnalyzedFrom { get; set; }
    public DateTime AnalyzedTo { get; set; }
}

/// <summary>
/// Threat indicator
/// </summary>
public class ThreatIndicator
{
    public required string ThreatType { get; set; }
    public required string Severity { get; set; }
    public required string Description { get; set; }
    public DateTime DetectedAt { get; set; }
    public required List<string> Indicators { get; set; }
    public int ConfidenceScore { get; set; } // 0-100
}

/// <summary>
/// Access anomaly detection result
/// </summary>
public class AccessAnomalyResult
{
    public int TotalAnomalies { get; set; }
    public required List<AccessAnomaly> Anomalies { get; set; }
    public required string RiskLevel { get; set; }
    public DateTime AnalyzedAt { get; set; }
}

/// <summary>
/// Access anomaly
/// </summary>
public class AccessAnomaly
{
    public required string AnomalyType { get; set; }
    public required string Description { get; set; }
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public DateTime DetectedAt { get; set; }
    public required List<string> SuspiciousActivities { get; set; }
    public int RiskScore { get; set; } // 0-100
}

/// <summary>
/// Security control recommendation
/// </summary>
public class SecurityControlRecommendation
{
    public required List<RecommendedControl> Controls { get; set; }
    public int TotalRisksAddressed { get; set; }
    public required string Framework { get; set; }
    public DateTime GeneratedAt { get; set; }
}

/// <summary>
/// Recommended control
/// </summary>
public class RecommendedControl
{
    public required string ControlId { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Priority { get; set; }
    public int EstimatedEffectiveness { get; set; } // 0-100
    public required List<Guid> AddressedRiskIds { get; set; }
}

/// <summary>
/// Incident response analysis
/// </summary>
public class IncidentResponseAnalysis
{
    public Guid IncidentId { get; set; }
    public required string EffectivenessRating { get; set; }
    public int ResponseScore { get; set; } // 0-100
    public TimeSpan DetectionTime { get; set; }
    public TimeSpan ContainmentTime { get; set; }
    public TimeSpan ResolutionTime { get; set; }
    public required List<string> StrengthsIdentified { get; set; }
    public required List<string> ImprovementAreas { get; set; }
    public required List<string> LessonsLearned { get; set; }
}
