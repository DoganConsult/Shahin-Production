using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Evidence Agent Service - AI-powered evidence analysis and recommendations
/// </summary>
public interface IEvidenceAgentService
{
    /// <summary>
    /// Analyze evidence quality and completeness
    /// </summary>
    Task<EvidenceQualityAnalysis> AnalyzeEvidenceQualityAsync(
        Guid evidenceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Detect gaps in evidence coverage for a tenant's controls
    /// </summary>
    Task<EvidenceGapAnalysis> DetectEvidenceGapsAsync(
        Guid tenantId,
        string? frameworkCode = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Suggest which requirements/controls an evidence might satisfy
    /// </summary>
    Task<EvidenceMatchingResult> SuggestEvidenceMatchesAsync(
        Guid evidenceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyze evidence expiration and renewal risks
    /// </summary>
    Task<EvidenceExpirationRisk> AnalyzeExpirationRisksAsync(
        Guid tenantId,
        int lookAheadDays = 90,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate evidence collection recommendations for a framework
    /// </summary>
    Task<EvidenceCollectionPlan> GenerateCollectionPlanAsync(
        Guid tenantId,
        string frameworkCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// AI-powered evidence categorization and tagging
    /// </summary>
    Task<EvidenceCategorizationResult> CategorizeEvidenceAsync(
        Guid evidenceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if service is available
    /// </summary>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Collect evidence from integrated systems automatically
    /// </summary>
    Task<EvidenceCollectionResult> CollectEvidenceAsync(
        Guid tenantId,
        string source,
        string evidenceType,
        Dictionary<string, object>? parameters = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate an evidence pack for a specific framework or assessment
    /// </summary>
    Task<EvidencePackResult> GenerateEvidencePackAsync(
        Guid tenantId,
        string frameworkCode,
        Guid? assessmentId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Organize evidence by control domain and category
    /// </summary>
    Task<EvidenceOrganizationResult> OrganizeEvidenceAsync(
        Guid tenantId,
        Guid? evidenceId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate evidence completeness and compliance requirements
    /// </summary>
    Task<EvidenceValidationResult> ValidateEvidenceAsync(
        Guid evidenceId,
        string? frameworkCode = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Evidence quality analysis result
/// </summary>
public class EvidenceQualityAnalysis
{
    public Guid EvidenceId { get; set; }
    public required string QualityRating { get; set; } // Excellent, Good, Fair, Poor
    public int QualityScore { get; set; } // 0-100
    public required List<QualityFinding> Findings { get; set; }
    public required List<string> Recommendations { get; set; }
    public required Dictionary<string, int> DimensionScores { get; set; } // Completeness, Relevance, Currency, etc.
    public DateTime AnalyzedAt { get; set; }
}

/// <summary>
/// Quality finding detail
/// </summary>
public class QualityFinding
{
    public required string Dimension { get; set; } // Completeness, Relevance, Currency, Authenticity
    public required string Severity { get; set; } // High, Medium, Low
    public required string Description { get; set; }
    public required string Recommendation { get; set; }
}

/// <summary>
/// Evidence gap analysis result
/// </summary>
public class EvidenceGapAnalysis
{
    public Guid TenantId { get; set; }
    public string? FrameworkCode { get; set; }
    public int TotalControls { get; set; }
    public int ControlsWithEvidence { get; set; }
    public int ControlsWithoutEvidence { get; set; }
    public decimal CoveragePercentage { get; set; }
    public required List<EvidenceGap> Gaps { get; set; }
    public required List<string> PriorityRecommendations { get; set; }
    public DateTime AnalyzedAt { get; set; }
}

/// <summary>
/// Individual evidence gap
/// </summary>
public class EvidenceGap
{
    public Guid ControlId { get; set; }
    public required string ControlCode { get; set; }
    public required string ControlName { get; set; }
    public required string Priority { get; set; } // Critical, High, Medium, Low
    public required List<string> SuggestedEvidenceTypes { get; set; }
    public required string Rationale { get; set; }
}

/// <summary>
/// Evidence matching result
/// </summary>
public class EvidenceMatchingResult
{
    public Guid EvidenceId { get; set; }
    public required List<MatchedRequirement> MatchedRequirements { get; set; }
    public required List<MatchedControl> MatchedControls { get; set; }
    public DateTime AnalyzedAt { get; set; }
}

/// <summary>
/// Matched requirement
/// </summary>
public class MatchedRequirement
{
    public Guid RequirementId { get; set; }
    public required string RequirementCode { get; set; }
    public required string RequirementName { get; set; }
    public int MatchConfidence { get; set; } // 0-100
    public required string MatchReason { get; set; }
}

/// <summary>
/// Matched control
/// </summary>
public class MatchedControl
{
    public Guid ControlId { get; set; }
    public required string ControlCode { get; set; }
    public required string ControlName { get; set; }
    public int MatchConfidence { get; set; } // 0-100
    public required string MatchReason { get; set; }
}

/// <summary>
/// Evidence expiration risk analysis
/// </summary>
public class EvidenceExpirationRisk
{
    public Guid TenantId { get; set; }
    public int LookAheadDays { get; set; }
    public int TotalAtRisk { get; set; }
    public required string OverallRiskLevel { get; set; } // Critical, High, Medium, Low
    public required List<ExpiringEvidence> ExpiringEvidence { get; set; }
    public required List<string> Recommendations { get; set; }
    public DateTime AnalyzedAt { get; set; }
}

/// <summary>
/// Expiring evidence detail
/// </summary>
public class ExpiringEvidence
{
    public Guid EvidenceId { get; set; }
    public required string Title { get; set; }
    public required string EvidenceType { get; set; }
    public DateTime ExpirationDate { get; set; }
    public int DaysUntilExpiration { get; set; }
    public required string RiskLevel { get; set; }
    public required List<string> AffectedControls { get; set; }
    public required string RenewalRecommendation { get; set; }
}

/// <summary>
/// Evidence collection plan
/// </summary>
public class EvidenceCollectionPlan
{
    public Guid TenantId { get; set; }
    public required string FrameworkCode { get; set; }
    public int TotalRequiredEvidenceItems { get; set; }
    public int CurrentEvidenceCount { get; set; }
    public decimal CompletionPercentage { get; set; }
    public required List<EvidenceCollectionTask> Tasks { get; set; }
    public required List<string> QuickWins { get; set; }
    public DateTime GeneratedAt { get; set; }
}

/// <summary>
/// Evidence collection task
/// </summary>
public class EvidenceCollectionTask
{
    public required string ControlDomain { get; set; }
    public required string EvidenceType { get; set; }
    public required string Description { get; set; }
    public required string Priority { get; set; }
    public required List<string> SuggestedSources { get; set; }
    public required List<Guid> RelatedControlIds { get; set; }
}

/// <summary>
/// Evidence categorization result
/// </summary>
public class EvidenceCategorizationResult
{
    public Guid EvidenceId { get; set; }
    public required string SuggestedCategory { get; set; }
    public required string SuggestedType { get; set; }
    public required List<string> SuggestedTags { get; set; }
    public required List<string> RelatedDomains { get; set; }
    public int CategorizationConfidence { get; set; } // 0-100
    public DateTime AnalyzedAt { get; set; }
}

/// <summary>
/// Result of evidence collection from integrated systems
/// </summary>
public class EvidenceCollectionResult
{
    public Guid TenantId { get; set; }
    public required string Source { get; set; }
    public required string EvidenceType { get; set; }
    public bool Success { get; set; }
    public int EvidenceItemsCollected { get; set; }
    public required List<CollectedEvidenceItem> CollectedItems { get; set; }
    public required List<string> Errors { get; set; }
    public DateTime CollectedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Single collected evidence item
/// </summary>
public class CollectedEvidenceItem
{
    public Guid? EvidenceId { get; set; }
    public required string Title { get; set; }
    public required string SourceSystem { get; set; }
    public required string FileName { get; set; }
    public long FileSize { get; set; }
    public required string ContentType { get; set; }
    public DateTime CollectedAt { get; set; }
    public required Dictionary<string, object> Metadata { get; set; }
}

/// <summary>
/// Result of evidence pack generation
/// </summary>
public class EvidencePackResult
{
    public Guid TenantId { get; set; }
    public required string FrameworkCode { get; set; }
    public Guid? AssessmentId { get; set; }
    public bool Success { get; set; }
    public Guid? PackageId { get; set; }
    public required string PackageName { get; set; }
    public int TotalControls { get; set; }
    public int ControlsWithEvidence { get; set; }
    public decimal CoveragePercentage { get; set; }
    public required List<EvidencePackSection> Sections { get; set; }
    public required List<string> MissingEvidence { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Section of evidence pack (by control domain)
/// </summary>
public class EvidencePackSection
{
    public required string ControlDomain { get; set; }
    public int ControlCount { get; set; }
    public int EvidenceCount { get; set; }
    public decimal CompletionPercentage { get; set; }
    public required List<EvidencePackItem> Items { get; set; }
}

/// <summary>
/// Evidence pack item
/// </summary>
public class EvidencePackItem
{
    public Guid EvidenceId { get; set; }
    public required string Title { get; set; }
    public required string Type { get; set; }
    public required string ControlCode { get; set; }
    public required string ControlName { get; set; }
    public required string FileName { get; set; }
    public DateTime CollectionDate { get; set; }
    public required string Status { get; set; } // Valid, Expired, PendingReview
}

/// <summary>
/// Result of evidence organization
/// </summary>
public class EvidenceOrganizationResult
{
    public Guid TenantId { get; set; }
    public bool Success { get; set; }
    public int EvidenceItemsOrganized { get; set; }
    public int CategoriesAssigned { get; set; }
    public int ControlLinksCreated { get; set; }
    public required List<EvidenceByDomain> OrganizedByDomain { get; set; }
    public required List<string> Recommendations { get; set; }
    public DateTime OrganizedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Evidence grouped by domain
/// </summary>
public class EvidenceByDomain
{
    public required string Domain { get; set; }
    public int EvidenceCount { get; set; }
    public int ControlsCovered { get; set; }
    public decimal CoveragePercentage { get; set; }
    public required List<string> Categories { get; set; }
}

/// <summary>
/// Result of evidence validation
/// </summary>
public class EvidenceValidationResult
{
    public Guid EvidenceId { get; set; }
    public bool IsValid { get; set; }
    public required string ValidationStatus { get; set; } // Valid, Invalid, PartiallyValid, RequiresReview
    public int CompletenessScore { get; set; } // 0-100
    public required List<ValidationFinding> Findings { get; set; }
    public required List<string> MissingElements { get; set; }
    public required List<string> Recommendations { get; set; }
    public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Validation finding detail
/// </summary>
public class ValidationFinding
{
    public required string Category { get; set; } // Completeness, Format, Currency, Relevance
    public required string Severity { get; set; } // Critical, High, Medium, Low, Info
    public required string Description { get; set; }
    public required string Requirement { get; set; }
    public bool IsPassing { get; set; }
    public required string Recommendation { get; set; }
}
