using GrcMvc.Models.DTOs;

namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Integration Agent Service - AI-powered external system integration and data mapping
/// </summary>
public interface IIntegrationAgentService
{
    /// <summary>
    /// Analyze integration requirements for a system
    /// </summary>
    Task<IntegrationAnalysisResult> AnalyzeIntegrationRequirementsAsync(
        string systemName,
        string systemType,
        Dictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate field mapping recommendations between systems
    /// </summary>
    Task<FieldMappingRecommendation> RecommendFieldMappingsAsync(
        Guid sourceSystemId,
        Guid targetSystemId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate integration data quality
    /// </summary>
    Task<DataQualityResult> ValidateIntegrationDataAsync(
        Guid integrationId,
        object sampleData,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Detect integration anomalies and errors
    /// </summary>
    Task<IntegrationHealthResult> MonitorIntegrationHealthAsync(
        Guid integrationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate integration configuration from requirements
    /// </summary>
    Task<IntegrationConfigurationResult> GenerateConfigurationAsync(
        string sourceSystem,
        string targetSystem,
        List<string> requiredFields,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if service is available
    /// </summary>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Integration analysis result
/// </summary>
public class IntegrationAnalysisResult
{
    public required string SystemName { get; set; }
    public required string SystemType { get; set; }
    public required string Complexity { get; set; } // Simple, Moderate, Complex
    public int EstimatedEffortHours { get; set; }
    public required List<IntegrationRequirement> Requirements { get; set; }
    public required List<string> Recommendations { get; set; }
    public required List<string> PotentialChallenges { get; set; }
    public DateTime AnalyzedAt { get; set; }
}

/// <summary>
/// Integration requirement
/// </summary>
public class IntegrationRequirement
{
    public required string Category { get; set; }
    public required string Description { get; set; }
    public required string Priority { get; set; }
    public bool IsMandatory { get; set; }
}

/// <summary>
/// Field mapping recommendation
/// </summary>
public class FieldMappingRecommendation
{
    public required List<FieldMapping> Mappings { get; set; }
    public int ConfidenceScore { get; set; } // 0-100
    public required List<string> UnmappedSourceFields { get; set; }
    public required List<string> UnmappedTargetFields { get; set; }
    public required List<string> Warnings { get; set; }
    public DateTime GeneratedAt { get; set; }
}

/// <summary>
/// Field mapping
/// </summary>
public class FieldMapping
{
    public required string SourceField { get; set; }
    public required string TargetField { get; set; }
    public required string TransformationType { get; set; } // Direct, Transform, Computed
    public string? TransformationLogic { get; set; }
    public int ConfidenceScore { get; set; }
    public required string DataType { get; set; }
}

/// <summary>
/// Data quality validation result
/// </summary>
public class DataQualityResult
{
    public int TotalRecords { get; set; }
    public int ValidRecords { get; set; }
    public int InvalidRecords { get; set; }
    public double QualityScore { get; set; } // 0-100
    public required List<DataQualityIssue> Issues { get; set; }
    public required List<string> Recommendations { get; set; }
    public DateTime ValidatedAt { get; set; }
}

/// <summary>
/// Data quality issue
/// </summary>
public class DataQualityIssue
{
    public required string IssueType { get; set; }
    public required string Severity { get; set; }
    public required string Description { get; set; }
    public required string FieldName { get; set; }
    public int AffectedRecords { get; set; }
    public required string RecommendedFix { get; set; }
}

/// <summary>
/// Integration health monitoring result
/// </summary>
public class IntegrationHealthResult
{
    public required string HealthStatus { get; set; } // Healthy, Degraded, Failed
    public int HealthScore { get; set; } // 0-100
    public TimeSpan Uptime { get; set; }
    public int TotalSyncs { get; set; }
    public int SuccessfulSyncs { get; set; }
    public int FailedSyncs { get; set; }
    public required List<IntegrationIssue> CurrentIssues { get; set; }
    public required List<string> Recommendations { get; set; }
    public DateTime CheckedAt { get; set; }
}

/// <summary>
/// Integration issue
/// </summary>
public class IntegrationIssue
{
    public required string IssueType { get; set; }
    public required string Severity { get; set; }
    public required string Description { get; set; }
    public DateTime FirstOccurred { get; set; }
    public DateTime LastOccurred { get; set; }
    public int Occurrences { get; set; }
    public required string RecommendedAction { get; set; }
}

/// <summary>
/// Integration configuration generation result
/// </summary>
public class IntegrationConfigurationResult
{
    public required string SourceSystem { get; set; }
    public required string TargetSystem { get; set; }
    public required string ConfigurationFormat { get; set; } // JSON, YAML, XML
    public required string ConfigurationContent { get; set; }
    public required List<string> RequiredCredentials { get; set; }
    public required List<string> SetupInstructions { get; set; }
    public DateTime GeneratedAt { get; set; }
}
