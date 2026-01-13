using System.Text.Json;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Evidence Confidence Service - Calculates and manages evidence confidence scores.
/// Implements the fullplan specification for evidence quality assessment.
/// </summary>
public class EvidenceConfidenceService : IEvidenceConfidenceService
{
    private readonly GrcDbContext _dbContext;
    private readonly ILogger<EvidenceConfidenceService> _logger;

    public EvidenceConfidenceService(
        GrcDbContext dbContext,
        ILogger<EvidenceConfidenceService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<EvidenceConfidenceScore> CalculateConfidenceAsync(
        Guid tenantId,
        Guid evidenceId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var evidence = await _dbContext.Evidences
                .FirstOrDefaultAsync(e => e.Id == evidenceId, cancellationToken);

            if (evidence == null)
            {
                throw new ArgumentException($"Evidence {evidenceId} not found");
            }

            // Get evidence criteria for this type
            var criteria = await _dbContext.EvidenceScoringCriteria
                .FirstOrDefaultAsync(c => c.EvidenceTypeCode == evidence.EvidenceTypeCode, cancellationToken);

            // Calculate component scores
            var sourceScore = CalculateSourceCredibilityScore(evidence);
            var completenessScore = CalculateCompletenessScore(evidence, criteria);
            var relevanceScore = CalculateRelevanceScore(evidence, criteria);
            var timelinessScore = CalculateTimelinessScore(evidence);
            var automationScore = CalculateAutomationScore(evidence);
            var verificationScore = CalculateCrossVerificationScore(evidence);
            var formatScore = CalculateFormatComplianceScore(evidence, criteria);

            // Calculate weighted overall score
            var overallScore = (int)Math.Round(
                sourceScore * 0.20 +
                completenessScore * 0.20 +
                relevanceScore * 0.15 +
                timelinessScore * 0.15 +
                verificationScore * 0.15 +
                formatScore * 0.10 +
                automationScore * 0.05
            );

            // Calculate SLA adherence
            int? slaAdherenceDays = null;
            bool? slaMet = null;
            if (evidence.DueDate.HasValue)
            {
                var submissionDate = evidence.SubmittedAt ?? evidence.CreatedDate;
                slaAdherenceDays = (int)(evidence.DueDate.Value - submissionDate).TotalDays;
                slaMet = slaAdherenceDays >= 0;
            }

            // Determine collection method
            var collectionMethod = evidence.IsAutoCollected ? "Automated" :
                                  evidence.IsHybridCollection ? "Hybrid" : "Manual";

            // Identify low confidence factors
            var lowFactors = new List<string>();
            if (sourceScore < 50) lowFactors.Add("Low source credibility");
            if (completenessScore < 50) lowFactors.Add("Incomplete evidence");
            if (relevanceScore < 50) lowFactors.Add("Relevance concerns");
            if (timelinessScore < 50) lowFactors.Add("Evidence is outdated");
            if (formatScore < 50) lowFactors.Add("Format compliance issues");

            // Determine recommended action
            var recommendedAction = overallScore switch
            {
                >= 90 => "AutoApprove",
                >= 70 => "HumanReview",
                >= 50 => "RequestMore",
                _ => "Reject"
            };

            var confidenceScore = new EvidenceConfidenceScore
            {
                TenantId = tenantId,
                EvidenceId = evidenceId,
                OverallScore = overallScore,
                ConfidenceLevel = ConfidenceLevels.GetLevel(overallScore),
                SourceCredibilityScore = sourceScore,
                CompletenessScore = completenessScore,
                RelevanceScore = relevanceScore,
                TimelinessScore = timelinessScore,
                AutomationCoveragePercent = automationScore,
                CrossVerificationScore = verificationScore,
                FormatComplianceScore = formatScore,
                SlaAdherenceDays = slaAdherenceDays,
                SlaMet = slaMet,
                CollectionMethod = collectionMethod,
                LowConfidenceFactors = lowFactors,
                RecommendedAction = recommendedAction,
                HumanReviewTriggered = recommendedAction == "HumanReview" || recommendedAction == "RequestMore",
                ScoredAt = DateTime.UtcNow
            };

            _dbContext.Set<EvidenceConfidenceScore>().Add(confidenceScore);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Calculated confidence score for evidence {EvidenceId}: {Score} ({Level})",
                evidenceId, overallScore, confidenceScore.ConfidenceLevel);

            return confidenceScore;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating confidence for evidence {EvidenceId}", evidenceId);
            throw;
        }
    }

    public async Task<EvidenceConfidenceScore?> GetConfidenceScoreAsync(
        Guid evidenceId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<EvidenceConfidenceScore>()
            .Where(c => c.EvidenceId == evidenceId)
            .OrderByDescending(c => c.ScoredAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<EvidenceConfidenceScore>> BatchCalculateConfidenceAsync(
        Guid tenantId,
        List<Guid> evidenceIds,
        CancellationToken cancellationToken = default)
    {
        var results = new List<EvidenceConfidenceScore>();

        foreach (var evidenceId in evidenceIds)
        {
            try
            {
                var score = await CalculateConfidenceAsync(tenantId, evidenceId, cancellationToken);
                results.Add(score);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error calculating confidence for evidence {EvidenceId}", evidenceId);
            }
        }

        return results;
    }

    public async Task<bool> RecordHumanReviewAsync(
        Guid evidenceConfidenceId,
        string outcome,
        string? feedback,
        Guid reviewerId,
        CancellationToken cancellationToken = default)
    {
        var confidenceScore = await _dbContext.Set<EvidenceConfidenceScore>()
            .FirstOrDefaultAsync(c => c.Id == evidenceConfidenceId, cancellationToken);

        if (confidenceScore == null)
            return false;

        confidenceScore.HumanReviewOutcome = outcome;
        confidenceScore.ReviewerFeedback = feedback;
        confidenceScore.ModifiedDate = DateTime.UtcNow;
        confidenceScore.ModifiedBy = reviewerId.ToString();

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Human review recorded for confidence score {Id}: {Outcome}",
            evidenceConfidenceId, outcome);

        return true;
    }

    public async Task<List<EvidenceConfidenceScore>> GetItemsNeedingReviewAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<EvidenceConfidenceScore>()
            .Where(c => c.TenantId == tenantId &&
                       c.HumanReviewTriggered &&
                       c.HumanReviewOutcome == null)
            .OrderBy(c => c.ScoredAt)
            .ToListAsync(cancellationToken);
    }

    #region Scoring Methods

    private int CalculateSourceCredibilityScore(Evidence evidence)
    {
        var score = 50; // Base score

        // Automated collection is more reliable
        if (evidence.IsAutoCollected)
            score += 20;

        // Digital signature increases credibility
        if (evidence.HasDigitalSignature)
            score += 20;

        // Known/verified source
        if (!string.IsNullOrEmpty(evidence.SourceSystem))
            score += 10;

        // File hash verification available
        if (!string.IsNullOrEmpty(evidence.FileHash))
            score += 10;

        return Math.Min(100, score);
    }

    private int CalculateCompletenessScore(Evidence evidence, EvidenceScoringCriteria? criteria)
    {
        var score = 60; // Base score

        // Has description
        if (!string.IsNullOrEmpty(evidence.Description))
            score += 10;

        // Has file attached
        if (!string.IsNullOrEmpty(evidence.FilePath))
            score += 15;

        // Has control mapping
        if (evidence.ControlId.HasValue)
            score += 10;

        // Has evidence period defined
        if (evidence.EvidencePeriodStart.HasValue && evidence.EvidencePeriodEnd.HasValue)
            score += 10;

        // Check against criteria requirements
        if (criteria != null)
        {
            // File size within limits
            if (evidence.FileSizeBytes > 0 && evidence.FileSizeBytes <= criteria.MaxFileSizeMB * 1024 * 1024)
                score += 5;
        }

        return Math.Min(100, score);
    }

    private int CalculateRelevanceScore(Evidence evidence, EvidenceScoringCriteria? criteria)
    {
        var score = 70; // Base score

        // Has framework mapping
        if (!string.IsNullOrEmpty(evidence.FrameworkCode))
            score += 15;

        // Has control mapping
        if (evidence.ControlId.HasValue)
            score += 15;

        // Check applicable frameworks match
        if (criteria != null && !string.IsNullOrEmpty(criteria.ApplicableFrameworks))
        {
            var applicableFrameworks = criteria.ApplicableFrameworks.Split('|');
            if (!string.IsNullOrEmpty(evidence.FrameworkCode) &&
                applicableFrameworks.Contains(evidence.FrameworkCode))
            {
                score += 10;
            }
        }

        return Math.Min(100, score);
    }

    private int CalculateTimelinessScore(Evidence evidence)
    {
        var score = 100; // Start at max

        // Calculate age of evidence
        var evidenceDate = evidence.EvidencePeriodEnd ?? evidence.CreatedDate;
        var ageInDays = (DateTime.UtcNow - evidenceDate).TotalDays;

        // Reduce score based on age
        if (ageInDays > 365)
            score -= 50;
        else if (ageInDays > 180)
            score -= 30;
        else if (ageInDays > 90)
            score -= 15;
        else if (ageInDays > 30)
            score -= 5;

        // Check if past validity period
        if (evidence.ValidUntil.HasValue && evidence.ValidUntil < DateTime.UtcNow)
            score -= 30;

        return Math.Max(0, score);
    }

    private int CalculateAutomationScore(Evidence evidence)
    {
        if (evidence.IsAutoCollected)
            return 100;

        if (evidence.IsHybridCollection)
            return 50;

        return 0;
    }

    private int CalculateCrossVerificationScore(Evidence evidence)
    {
        var score = 50; // Base score

        // Has been reviewed
        if (evidence.VerificationStatus == "Verified")
            score += 30;

        // Has reviewer comments
        if (!string.IsNullOrEmpty(evidence.ReviewerComments))
            score += 10;

        // Has multiple verification steps
        if (evidence.VerifiedAt.HasValue)
            score += 10;

        return Math.Min(100, score);
    }

    private int CalculateFormatComplianceScore(Evidence evidence, EvidenceScoringCriteria? criteria)
    {
        var score = 70; // Base score

        if (criteria == null || string.IsNullOrEmpty(evidence.FileType))
            return score;

        // Check file type is allowed
        var allowedTypes = criteria.AllowedFileTypes.Split(',');
        if (allowedTypes.Any(t => evidence.FileType.EndsWith(t.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            score += 15;
        }
        else
        {
            score -= 20;
        }

        // Check file size
        if (evidence.FileSizeBytes > 0 && evidence.FileSizeBytes <= criteria.MaxFileSizeMB * 1024 * 1024)
        {
            score += 10;
        }
        else if (evidence.FileSizeBytes > criteria.MaxFileSizeMB * 1024 * 1024)
        {
            score -= 15;
        }

        // Check digital signature if required
        if (criteria.RequiresDigitalSignature)
        {
            if (evidence.HasDigitalSignature)
                score += 10;
            else
                score -= 20;
        }

        return Math.Max(0, Math.Min(100, score));
    }

    #endregion
}
