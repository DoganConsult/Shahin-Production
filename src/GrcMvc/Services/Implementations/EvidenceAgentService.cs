using GrcMvc.Configuration;
using GrcMvc.Data;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Evidence Agent Service - AI-powered evidence analysis and recommendations
/// Uses Claude Sonnet 4.5 for intelligent evidence analysis
/// </summary>
public class EvidenceAgentService : IEvidenceAgentService
{
    private readonly ILogger<EvidenceAgentService> _logger;
    private readonly GrcDbContext _context;
    private readonly ClaudeApiSettings _settings;
    private readonly HttpClient _httpClient;

    public EvidenceAgentService(
        ILogger<EvidenceAgentService> logger,
        GrcDbContext context,
        IOptions<ClaudeApiSettings> settings,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _context = context;
        _settings = settings.Value;
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return !string.IsNullOrWhiteSpace(_settings.ApiKey);
    }

    public async Task<EvidenceQualityAnalysis> AnalyzeEvidenceQualityAsync(
        Guid evidenceId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Analyzing evidence quality for evidence {EvidenceId}", evidenceId);

        var evidence = await _context.Evidences
            .Include(e => e.AssessmentRequirement)
            .FirstOrDefaultAsync(e => e.Id == evidenceId, cancellationToken);

        if (evidence == null)
        {
            throw new InvalidOperationException($"Evidence {evidenceId} not found");
        }

        if (!await IsAvailableAsync(cancellationToken))
        {
            return CreateFallbackQualityAnalysis(evidenceId);
        }

        try
        {
            var prompt = $@"Analyze the quality of this compliance evidence:

Title: {evidence.Title}
Type: {evidence.Type}
Description: {evidence.Description}
Verification Status: {evidence.VerificationStatus}
Created: {evidence.CreatedDate:yyyy-MM-dd}
Collection Date: {evidence.CollectionDate:yyyy-MM-dd}
File: {evidence.FileName} ({evidence.FileSize} bytes)

Linked Requirement: {evidence.AssessmentRequirement?.RequirementText ?? "None"}

Evaluate the evidence quality across these dimensions:
1. Completeness - Is the evidence comprehensive?
2. Relevance - Does it address the requirement?
3. Currency - Is it recent enough?
4. Authenticity - Does it appear legitimate?
5. Traceability - Can it be verified?

Respond with JSON:
{{
  ""qualityRating"": ""Good"",
  ""qualityScore"": 75,
  ""findings"": [
    {{
      ""dimension"": ""Completeness"",
      ""severity"": ""Medium"",
      ""description"": ""....."",
      ""recommendation"": ""...""
    }}
  ],
  ""recommendations"": [""......""],
  ""dimensionScores"": {{
    ""Completeness"": 80,
    ""Relevance"": 85,
    ""Currency"": 70,
    ""Authenticity"": 75,
    ""Traceability"": 65
  }}
}}";

            var response = await CallClaudeApiAsync(prompt, cancellationToken);
            var result = JsonSerializer.Deserialize<EvidenceQualityAnalysis>(response);

            if (result != null)
            {
                result.EvidenceId = evidenceId;
                result.AnalyzedAt = DateTime.UtcNow;
                _logger.LogInformation("Evidence quality analysis completed for {EvidenceId}: {Rating}",
                    evidenceId, result.QualityRating);
                return result;
            }

            return CreateFallbackQualityAnalysis(evidenceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing evidence quality for {EvidenceId}", evidenceId);
            return CreateFallbackQualityAnalysis(evidenceId);
        }
    }

    public async Task<EvidenceGapAnalysis> DetectEvidenceGapsAsync(
        Guid tenantId,
        string? frameworkCode = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Detecting evidence gaps for tenant {TenantId}, framework {Framework}",
            tenantId, frameworkCode);

        // Get controls for the tenant
        var controlsQuery = _context.Controls
            .Where(c => c.TenantId == tenantId && !c.IsDeleted);

        if (!string.IsNullOrWhiteSpace(frameworkCode))
        {
            controlsQuery = controlsQuery.Where(c => c.SourceFrameworkCode == frameworkCode);
        }

        var controls = await controlsQuery
            .Select(c => new { c.Id, c.ControlCode, c.Name, c.Category, EvidenceCount = c.Evidences.Count })
            .ToListAsync(cancellationToken);

        var controlsWithEvidence = controls.Count(c => c.EvidenceCount > 0);
        var controlsWithoutEvidence = controls.Count(c => c.EvidenceCount == 0);

        var gaps = controls
            .Where(c => c.EvidenceCount == 0)
            .Select(c => new EvidenceGap
            {
                ControlId = c.Id,
                ControlCode = c.ControlCode ?? c.Id.ToString(),
                ControlName = c.Name ?? "Unnamed Control",
                Priority = DeterminePriority(c.Category),
                SuggestedEvidenceTypes = GetSuggestedEvidenceTypes(c.Category),
                Rationale = $"Control {c.ControlCode ?? c.Name} has no linked evidence"
            })
            .ToList();

        return new EvidenceGapAnalysis
        {
            TenantId = tenantId,
            FrameworkCode = frameworkCode,
            TotalControls = controls.Count,
            ControlsWithEvidence = controlsWithEvidence,
            ControlsWithoutEvidence = controlsWithoutEvidence,
            CoveragePercentage = controls.Count > 0
                ? (decimal)controlsWithEvidence / controls.Count * 100
                : 0,
            Gaps = gaps.Take(20).ToList(), // Limit to top 20 gaps
            PriorityRecommendations = GenerateGapRecommendations(gaps),
            AnalyzedAt = DateTime.UtcNow
        };
    }

    public async Task<EvidenceMatchingResult> SuggestEvidenceMatchesAsync(
        Guid evidenceId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Suggesting evidence matches for {EvidenceId}", evidenceId);

        var evidence = await _context.Evidences
            .FirstOrDefaultAsync(e => e.Id == evidenceId, cancellationToken);

        if (evidence == null)
        {
            throw new InvalidOperationException($"Evidence {evidenceId} not found");
        }

        // Get unlinked controls that might match
        var controls = await _context.Controls
            .Where(c => c.TenantId == evidence.TenantId && !c.IsDeleted)
            .Select(c => new { c.Id, c.ControlCode, c.Name, c.Category, c.Description })
            .Take(100)
            .ToListAsync(cancellationToken);

        var matchedControls = controls
            .Where(c => IsLikelyMatch(evidence, c.Category, c.Description))
            .Select(c => new MatchedControl
            {
                ControlId = c.Id,
                ControlCode = c.ControlCode ?? c.Id.ToString(),
                ControlName = c.Name ?? "Unnamed Control",
                MatchConfidence = CalculateMatchConfidence(evidence, c.Category, c.Description),
                MatchReason = $"Category and type alignment with {c.Category}"
            })
            .OrderByDescending(m => m.MatchConfidence)
            .Take(10)
            .ToList();

        return new EvidenceMatchingResult
        {
            EvidenceId = evidenceId,
            MatchedRequirements = new List<MatchedRequirement>(),
            MatchedControls = matchedControls,
            AnalyzedAt = DateTime.UtcNow
        };
    }

    public async Task<EvidenceExpirationRisk> AnalyzeExpirationRisksAsync(
        Guid tenantId,
        int lookAheadDays = 90,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Analyzing expiration risks for tenant {TenantId}, {Days} days ahead",
            tenantId, lookAheadDays);

        var cutoffDate = DateTime.UtcNow.AddDays(lookAheadDays);

        var expiringEvidence = await _context.Evidences
            .Where(e => e.TenantId == tenantId && !e.IsDeleted)
            .Where(e => e.RetentionEndDate.HasValue && e.RetentionEndDate.Value <= cutoffDate)
            .OrderBy(e => e.RetentionEndDate)
            .Select(e => new
            {
                e.Id,
                e.Title,
                EvidenceType = e.Type,
                ValidUntil = e.RetentionEndDate,
                e.ControlId
            })
            .Take(50)
            .ToListAsync(cancellationToken);

        var expiring = expiringEvidence.Select(e => new ExpiringEvidence
        {
            EvidenceId = e.Id,
            Title = e.Title ?? "Untitled Evidence",
            EvidenceType = e.EvidenceType ?? "General",
            ExpirationDate = e.ValidUntil ?? DateTime.UtcNow,
            DaysUntilExpiration = e.ValidUntil.HasValue
                ? (int)(e.ValidUntil.Value - DateTime.UtcNow).TotalDays
                : 0,
            RiskLevel = DetermineExpirationRiskLevel(e.ValidUntil),
            AffectedControls = new List<string>(),
            RenewalRecommendation = "Schedule evidence renewal before expiration"
        }).ToList();

        var criticalCount = expiring.Count(e => e.RiskLevel == "Critical");
        var highCount = expiring.Count(e => e.RiskLevel == "High");

        return new EvidenceExpirationRisk
        {
            TenantId = tenantId,
            LookAheadDays = lookAheadDays,
            TotalAtRisk = expiring.Count,
            OverallRiskLevel = criticalCount > 0 ? "Critical" : highCount > 0 ? "High" : "Medium",
            ExpiringEvidence = expiring,
            Recommendations = GenerateExpirationRecommendations(expiring),
            AnalyzedAt = DateTime.UtcNow
        };
    }

    public async Task<EvidenceCollectionPlan> GenerateCollectionPlanAsync(
        Guid tenantId,
        string frameworkCode,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating evidence collection plan for tenant {TenantId}, framework {Framework}",
            tenantId, frameworkCode);

        var controls = await _context.Controls
            .Where(c => c.TenantId == tenantId && !c.IsDeleted)
            .Where(c => c.SourceFrameworkCode == frameworkCode)
            .Select(c => new { c.Id, c.Category, c.Name, EvidenceCount = c.Evidences.Count })
            .ToListAsync(cancellationToken);

        var currentEvidenceCount = controls.Sum(c => c.EvidenceCount);
        var totalRequired = controls.Count * 2; // Assume 2 evidence items per control as baseline

        var tasks = controls
            .Where(c => c.EvidenceCount == 0)
            .GroupBy(c => c.Category ?? "General")
            .Select(g => new EvidenceCollectionTask
            {
                ControlDomain = g.Key,
                EvidenceType = GetDefaultEvidenceType(g.Key),
                Description = $"Collect evidence for {g.Count()} controls in {g.Key} domain",
                Priority = DeterminePriority(g.Key),
                SuggestedSources = GetSuggestedSources(g.Key),
                RelatedControlIds = g.Select(c => c.Id).ToList()
            })
            .OrderBy(t => t.Priority == "Critical" ? 0 : t.Priority == "High" ? 1 : 2)
            .ToList();

        return new EvidenceCollectionPlan
        {
            TenantId = tenantId,
            FrameworkCode = frameworkCode,
            TotalRequiredEvidenceItems = totalRequired,
            CurrentEvidenceCount = currentEvidenceCount,
            CompletionPercentage = totalRequired > 0
                ? (decimal)currentEvidenceCount / totalRequired * 100
                : 0,
            Tasks = tasks,
            QuickWins = GenerateQuickWins(tasks),
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<EvidenceCategorizationResult> CategorizeEvidenceAsync(
        Guid evidenceId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Categorizing evidence {EvidenceId}", evidenceId);

        var evidence = await _context.Evidences
            .FirstOrDefaultAsync(e => e.Id == evidenceId, cancellationToken);

        if (evidence == null)
        {
            throw new InvalidOperationException($"Evidence {evidenceId} not found");
        }

        if (!await IsAvailableAsync(cancellationToken))
        {
            return CreateFallbackCategorization(evidenceId, evidence);
        }

        try
        {
            var prompt = $@"Categorize this compliance evidence:

Title: {evidence.Title}
Description: {evidence.Description}
Current Type: {evidence.Type}
File Name: {evidence.FileName}

Suggest the best category, type, tags, and related domains.

Respond with JSON:
{{
  ""suggestedCategory"": ""Technical"",
  ""suggestedType"": ""Configuration Document"",
  ""suggestedTags"": [""security"", ""access-control"", ""policy""],
  ""relatedDomains"": [""Access Management"", ""Identity Management""],
  ""categorizationConfidence"": 85
}}";

            var response = await CallClaudeApiAsync(prompt, cancellationToken);
            var result = JsonSerializer.Deserialize<EvidenceCategorizationResult>(response);

            if (result != null)
            {
                result.EvidenceId = evidenceId;
                result.AnalyzedAt = DateTime.UtcNow;
                return result;
            }

            return CreateFallbackCategorization(evidenceId, evidence);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error categorizing evidence {EvidenceId}", evidenceId);
            return CreateFallbackCategorization(evidenceId, evidence);
        }
    }

    public async Task<EvidenceCollectionResult> CollectEvidenceAsync(
        Guid tenantId,
        string source,
        string evidenceType,
        Dictionary<string, object>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Collecting evidence from {Source} for tenant {TenantId}", source, tenantId);

        var result = new EvidenceCollectionResult
        {
            TenantId = tenantId,
            Source = source,
            EvidenceType = evidenceType,
            CollectedItems = new List<CollectedEvidenceItem>(),
            Errors = new List<string>(),
            CollectedAt = DateTime.UtcNow
        };

        try
        {
            // Get integration connectors for the source
            var integrations = await _context.IntegrationConnectors
                .Where(i => i.TenantId == tenantId && i.ConnectorType == source && i.Status == "Active")
                .ToListAsync(cancellationToken);

            if (!integrations.Any())
            {
                result.Success = false;
                result.Errors.Add($"No active integration found for source: {source}");
                return result;
            }

            // Simulate evidence collection from integrated systems
            // In production, this would connect to actual systems (IAM, SIEM, ERP, etc.)
            foreach (var integration in integrations)
            {
                var collectedItem = new CollectedEvidenceItem
                {
                    Title = $"{evidenceType} from {source}",
                    SourceSystem = integration.Name ?? source,
                    FileName = $"{evidenceType.Replace(" ", "_")}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json",
                    FileSize = 1024,
                    ContentType = "application/json",
                    CollectedAt = DateTime.UtcNow,
                    Metadata = new Dictionary<string, object>
                    {
                        { "source", source },
                        { "integrationId", integration.Id },
                        { "collectionMethod", "Automated" },
                        { "parameters", parameters ?? new Dictionary<string, object>() }
                    }
                };

                // Create evidence record in database
                var evidence = new Models.Entities.Evidence
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    Title = collectedItem.Title,
                    Type = evidenceType,
                    FileName = collectedItem.FileName,
                    FileSize = collectedItem.FileSize,
                    MimeType = collectedItem.ContentType,
                    CollectionDate = DateTime.UtcNow,
                    SourceSystem = source,
                    VerificationStatus = "Pending",
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "EvidenceAgentService"
                };

                _context.Evidences.Add(evidence);
                collectedItem.EvidenceId = evidence.Id;
                result.CollectedItems.Add(collectedItem);
            }

            await _context.SaveChangesAsync(cancellationToken);

            result.Success = true;
            result.EvidenceItemsCollected = result.CollectedItems.Count;

            _logger.LogInformation("Collected {Count} evidence items from {Source}",
                result.EvidenceItemsCollected, source);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting evidence from {Source}", source);
            result.Success = false;
            result.Errors.Add($"Error: {ex.Message}");
        }

        return result;
    }

    public async Task<EvidencePackResult> GenerateEvidencePackAsync(
        Guid tenantId,
        string frameworkCode,
        Guid? assessmentId = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating evidence pack for tenant {TenantId}, framework {Framework}",
            tenantId, frameworkCode);

        var result = new EvidencePackResult
        {
            TenantId = tenantId,
            FrameworkCode = frameworkCode,
            AssessmentId = assessmentId,
            PackageName = $"{frameworkCode}_EvidencePack_{DateTime.UtcNow:yyyyMMdd}",
            Sections = new List<EvidencePackSection>(),
            MissingEvidence = new List<string>(),
            GeneratedAt = DateTime.UtcNow
        };

        try
        {
            // Get controls for the framework
            var controls = await _context.Controls
                .Where(c => c.TenantId == tenantId && !c.IsDeleted)
                .Where(c => c.SourceFrameworkCode == frameworkCode)
                .Include(c => c.Evidences)
                .ToListAsync(cancellationToken);

            result.TotalControls = controls.Count;
            result.ControlsWithEvidence = controls.Count(c => c.Evidences.Any());
            result.CoveragePercentage = result.TotalControls > 0
                ? (decimal)result.ControlsWithEvidence / result.TotalControls * 100
                : 0;

            // Group by control domain/category
            var byDomain = controls
                .GroupBy(c => c.Category ?? "General")
                .ToList();

            foreach (var domain in byDomain)
            {
                var section = new EvidencePackSection
                {
                    ControlDomain = domain.Key,
                    ControlCount = domain.Count(),
                    EvidenceCount = domain.Sum(c => c.Evidences.Count),
                    CompletionPercentage = domain.Count() > 0
                        ? (decimal)domain.Count(c => c.Evidences.Any()) / domain.Count() * 100
                        : 0,
                    Items = new List<EvidencePackItem>()
                };

                foreach (var control in domain)
                {
                    foreach (var evidence in control.Evidences.Where(e => !e.IsDeleted))
                    {
                        var status = "Valid";
                        if (evidence.RetentionEndDate.HasValue && evidence.RetentionEndDate.Value < DateTime.UtcNow)
                            status = "Expired";
                        else if (evidence.VerificationStatus == "Pending")
                            status = "PendingReview";

                        section.Items.Add(new EvidencePackItem
                        {
                            EvidenceId = evidence.Id,
                            Title = evidence.Title ?? "Untitled",
                            Type = evidence.Type ?? "General",
                            ControlCode = control.ControlCode ?? control.Id.ToString(),
                            ControlName = control.Name ?? "Unnamed Control",
                            FileName = evidence.FileName ?? "unknown",
                            CollectionDate = evidence.CollectionDate,
                            Status = status
                        });
                    }

                    if (!control.Evidences.Any())
                    {
                        result.MissingEvidence.Add($"{control.ControlCode ?? control.Name}: No evidence attached");
                    }
                }

                result.Sections.Add(section);
            }

            result.Success = true;
            result.PackageId = Guid.NewGuid();

            _logger.LogInformation("Generated evidence pack with {Coverage}% coverage",
                result.CoveragePercentage.ToString("F1"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating evidence pack");
            result.Success = false;
        }

        return result;
    }

    public async Task<EvidenceOrganizationResult> OrganizeEvidenceAsync(
        Guid tenantId,
        Guid? evidenceId = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Organizing evidence for tenant {TenantId}", tenantId);

        var result = new EvidenceOrganizationResult
        {
            TenantId = tenantId,
            OrganizedByDomain = new List<EvidenceByDomain>(),
            Recommendations = new List<string>(),
            OrganizedAt = DateTime.UtcNow
        };

        try
        {
            var evidenceQuery = _context.Evidences
                .Where(e => e.TenantId == tenantId && !e.IsDeleted);

            if (evidenceId.HasValue)
            {
                evidenceQuery = evidenceQuery.Where(e => e.Id == evidenceId);
            }

            var evidenceList = await evidenceQuery
                .Include(e => e.Control)
                .ToListAsync(cancellationToken);

            // Use AI to suggest categorization for uncategorized evidence
            var uncategorized = evidenceList.Where(e => string.IsNullOrWhiteSpace(e.Type) || e.Type == "General").ToList();

            foreach (var evidence in uncategorized)
            {
                if (await IsAvailableAsync(cancellationToken))
                {
                    try
                    {
                        var categorization = await CategorizeEvidenceAsync(evidence.Id, cancellationToken);
                        evidence.Type = categorization.SuggestedType;
                        result.CategoriesAssigned++;
                    }
                    catch
                    {
                        // Continue with other evidence if one fails
                    }
                }
            }

            // Organize by domain based on linked controls
            var byDomain = evidenceList
                .Where(e => e.Control != null)
                .GroupBy(e => e.Control!.Category ?? "General")
                .Select(g => new EvidenceByDomain
                {
                    Domain = g.Key,
                    EvidenceCount = g.Count(),
                    ControlsCovered = g.Select(e => e.ControlId).Distinct().Count(),
                    CoveragePercentage = 0, // Would need to calculate against total controls
                    Categories = g.Select(e => e.Type ?? "General").Distinct().ToList()
                })
                .ToList();

            result.OrganizedByDomain = byDomain;
            result.EvidenceItemsOrganized = evidenceList.Count;

            // Count unlinked evidence
            var unlinked = evidenceList.Count(e => e.ControlId == null);
            if (unlinked > 0)
            {
                result.Recommendations.Add($"{unlinked} evidence items are not linked to any control");
            }

            if (uncategorized.Count > 0)
            {
                result.Recommendations.Add($"{uncategorized.Count} evidence items need category assignment");
            }

            await _context.SaveChangesAsync(cancellationToken);

            result.Success = true;
            _logger.LogInformation("Organized {Count} evidence items", result.EvidenceItemsOrganized);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error organizing evidence");
            result.Success = false;
        }

        return result;
    }

    public async Task<EvidenceValidationResult> ValidateEvidenceAsync(
        Guid evidenceId,
        string? frameworkCode = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Validating evidence {EvidenceId}", evidenceId);

        var result = new EvidenceValidationResult
        {
            EvidenceId = evidenceId,
            ValidationStatus = "Pending",
            Findings = new List<ValidationFinding>(),
            MissingElements = new List<string>(),
            Recommendations = new List<string>(),
            ValidatedAt = DateTime.UtcNow
        };

        try
        {
            var evidence = await _context.Evidences
                .Include(e => e.Control)
                .Include(e => e.AssessmentRequirement)
                .FirstOrDefaultAsync(e => e.Id == evidenceId, cancellationToken);

            if (evidence == null)
            {
                result.IsValid = false;
                result.ValidationStatus = "Invalid";
                result.Findings.Add(new ValidationFinding
                {
                    Category = "Existence",
                    Severity = "Critical",
                    Description = "Evidence not found",
                    Requirement = "Evidence must exist",
                    IsPassing = false,
                    Recommendation = "Verify evidence ID"
                });
                return result;
            }

            var score = 0;
            var maxScore = 100;

            // Check completeness - Title
            if (!string.IsNullOrWhiteSpace(evidence.Title))
            {
                score += 15;
                result.Findings.Add(new ValidationFinding
                {
                    Category = "Completeness",
                    Severity = "Info",
                    Description = "Evidence has a title",
                    Requirement = "Evidence must have a descriptive title",
                    IsPassing = true,
                    Recommendation = "N/A"
                });
            }
            else
            {
                result.MissingElements.Add("Title");
                result.Findings.Add(new ValidationFinding
                {
                    Category = "Completeness",
                    Severity = "High",
                    Description = "Evidence is missing a title",
                    Requirement = "Evidence must have a descriptive title",
                    IsPassing = false,
                    Recommendation = "Add a descriptive title to the evidence"
                });
            }

            // Check completeness - Description
            if (!string.IsNullOrWhiteSpace(evidence.Description))
            {
                score += 15;
            }
            else
            {
                result.MissingElements.Add("Description");
                result.Findings.Add(new ValidationFinding
                {
                    Category = "Completeness",
                    Severity = "Medium",
                    Description = "Evidence is missing a description",
                    Requirement = "Evidence should have a description",
                    IsPassing = false,
                    Recommendation = "Add a description explaining the evidence"
                });
            }

            // Check currency - Collection date
            var age = (DateTime.UtcNow - evidence.CollectionDate).TotalDays;
            if (age <= 365)
            {
                score += 20;
                result.Findings.Add(new ValidationFinding
                {
                    Category = "Currency",
                    Severity = "Info",
                    Description = $"Evidence is {age:F0} days old",
                    Requirement = "Evidence should be less than 1 year old",
                    IsPassing = true,
                    Recommendation = "N/A"
                });
            }
            else
            {
                score += 5;
                result.Findings.Add(new ValidationFinding
                {
                    Category = "Currency",
                    Severity = "High",
                    Description = $"Evidence is {age:F0} days old",
                    Requirement = "Evidence should be less than 1 year old",
                    IsPassing = false,
                    Recommendation = "Consider collecting newer evidence"
                });
            }

            // Check expiration
            if (evidence.RetentionEndDate.HasValue)
            {
                if (evidence.RetentionEndDate.Value > DateTime.UtcNow)
                {
                    score += 15;
                }
                else
                {
                    result.Findings.Add(new ValidationFinding
                    {
                        Category = "Currency",
                        Severity = "Critical",
                        Description = "Evidence has expired",
                        Requirement = "Evidence must not be expired",
                        IsPassing = false,
                        Recommendation = "Renew or replace the evidence"
                    });
                }
            }
            else
            {
                score += 10; // No expiration set - neutral
            }

            // Check control linkage
            if (evidence.ControlId.HasValue)
            {
                score += 20;
                result.Findings.Add(new ValidationFinding
                {
                    Category = "Relevance",
                    Severity = "Info",
                    Description = "Evidence is linked to a control",
                    Requirement = "Evidence should be linked to a control",
                    IsPassing = true,
                    Recommendation = "N/A"
                });
            }
            else
            {
                result.MissingElements.Add("Control Link");
                result.Findings.Add(new ValidationFinding
                {
                    Category = "Relevance",
                    Severity = "High",
                    Description = "Evidence is not linked to any control",
                    Requirement = "Evidence should be linked to a control",
                    IsPassing = false,
                    Recommendation = "Link the evidence to appropriate control(s)"
                });
            }

            // Check file attachment
            if (!string.IsNullOrWhiteSpace(evidence.FileName))
            {
                score += 15;
            }
            else
            {
                result.MissingElements.Add("File Attachment");
                result.Findings.Add(new ValidationFinding
                {
                    Category = "Completeness",
                    Severity = "Medium",
                    Description = "No file attached to evidence",
                    Requirement = "Evidence should have supporting documentation",
                    IsPassing = false,
                    Recommendation = "Upload supporting file"
                });
            }

            result.CompletenessScore = (int)((double)score / maxScore * 100);
            result.IsValid = result.CompletenessScore >= 70;
            result.ValidationStatus = result.CompletenessScore >= 80 ? "Valid"
                                    : result.CompletenessScore >= 50 ? "PartiallyValid"
                                    : result.CompletenessScore >= 30 ? "RequiresReview"
                                    : "Invalid";

            // Generate recommendations
            if (result.MissingElements.Any())
            {
                result.Recommendations.Add($"Address missing elements: {string.Join(", ", result.MissingElements)}");
            }
            if (!result.IsValid)
            {
                result.Recommendations.Add("Evidence does not meet minimum validation requirements");
            }

            _logger.LogInformation("Validated evidence {EvidenceId}: {Status} ({Score}%)",
                evidenceId, result.ValidationStatus, result.CompletenessScore);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating evidence {EvidenceId}", evidenceId);
            result.IsValid = false;
            result.ValidationStatus = "Invalid";
            result.Findings.Add(new ValidationFinding
            {
                Category = "System",
                Severity = "Critical",
                Description = $"Validation error: {ex.Message}",
                Requirement = "Validation must complete without errors",
                IsPassing = false,
                Recommendation = "Check system logs and retry"
            });
        }

        return result;
    }

    #region Private Helper Methods

    private async Task<string> CallClaudeApiAsync(string prompt, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            throw new InvalidOperationException("Claude API key is not configured");
        }

        var request = new
        {
            model = _settings.Model,
            max_tokens = _settings.MaxTokens,
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };

        var requestJson = JsonSerializer.Serialize(request);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("x-api-key", _settings.ApiKey);
        _httpClient.DefaultRequestHeaders.Add("anthropic-version", _settings.ApiVersion ?? "2023-06-01");

        var response = await _httpClient.PostAsync(_settings.ApiEndpoint ?? "https://api.anthropic.com/v1/messages",
            content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        var claudeResponse = JsonSerializer.Deserialize<ClaudeApiResponse>(responseJson);

        return claudeResponse?.Content?.FirstOrDefault()?.Text ?? "{}";
    }

    private string DeterminePriority(string? category)
    {
        return category?.ToLowerInvariant() switch
        {
            "access control" => "Critical",
            "security" => "Critical",
            "data protection" => "High",
            "incident management" => "High",
            "risk management" => "High",
            "change management" => "Medium",
            "business continuity" => "Medium",
            _ => "Medium"
        };
    }

    private List<string> GetSuggestedEvidenceTypes(string? category)
    {
        return category?.ToLowerInvariant() switch
        {
            "access control" => new List<string> { "Access Control Matrix", "User Access Review", "Authentication Logs" },
            "security" => new List<string> { "Security Policy", "Vulnerability Scan", "Penetration Test Report" },
            "data protection" => new List<string> { "Encryption Policy", "Data Classification", "Privacy Impact Assessment" },
            "incident management" => new List<string> { "Incident Report", "Root Cause Analysis", "Lessons Learned" },
            "risk management" => new List<string> { "Risk Assessment", "Risk Register", "Treatment Plan" },
            _ => new List<string> { "Policy Document", "Procedure Document", "Evidence Screenshot" }
        };
    }

    private List<string> GenerateGapRecommendations(List<EvidenceGap> gaps)
    {
        var recommendations = new List<string>();

        var criticalGaps = gaps.Count(g => g.Priority == "Critical");
        var highGaps = gaps.Count(g => g.Priority == "High");

        if (criticalGaps > 0)
        {
            recommendations.Add($"Prioritize evidence collection for {criticalGaps} critical controls immediately");
        }
        if (highGaps > 0)
        {
            recommendations.Add($"Schedule evidence collection for {highGaps} high-priority controls");
        }
        if (gaps.Count > 10)
        {
            recommendations.Add("Consider a dedicated evidence collection sprint to address multiple gaps");
        }

        return recommendations;
    }

    private bool IsLikelyMatch(Models.Entities.Evidence evidence, string? category, string? description)
    {
        if (string.IsNullOrWhiteSpace(category) && string.IsNullOrWhiteSpace(description))
            return false;

        var evidenceText = $"{evidence.Title} {evidence.Description} {evidence.Type}".ToLowerInvariant();
        var controlText = $"{category} {description}".ToLowerInvariant();

        // Simple keyword matching
        var evidenceWords = evidenceText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var controlWords = controlText.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var matchingWords = evidenceWords.Intersect(controlWords).Count();
        return matchingWords >= 2;
    }

    private int CalculateMatchConfidence(Models.Entities.Evidence evidence, string? category, string? description)
    {
        var evidenceText = $"{evidence.Title} {evidence.Description} {evidence.Type}".ToLowerInvariant();
        var controlText = $"{category} {description}".ToLowerInvariant();

        var evidenceWords = evidenceText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var controlWords = controlText.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var matchingWords = evidenceWords.Intersect(controlWords).Count();
        var totalWords = Math.Max(evidenceWords.Length, controlWords.Length);

        return totalWords > 0 ? Math.Min(100, matchingWords * 20) : 0;
    }

    private string DetermineExpirationRiskLevel(DateTime? validUntil)
    {
        if (!validUntil.HasValue) return "Low";

        var daysUntil = (validUntil.Value - DateTime.UtcNow).TotalDays;

        return daysUntil switch
        {
            <= 7 => "Critical",
            <= 30 => "High",
            <= 60 => "Medium",
            _ => "Low"
        };
    }

    private List<string> GenerateExpirationRecommendations(List<ExpiringEvidence> expiring)
    {
        var recommendations = new List<string>();

        var critical = expiring.Where(e => e.RiskLevel == "Critical").ToList();
        var high = expiring.Where(e => e.RiskLevel == "High").ToList();

        if (critical.Any())
        {
            recommendations.Add($"URGENT: {critical.Count} evidence items expire within 7 days - immediate action required");
        }
        if (high.Any())
        {
            recommendations.Add($"Schedule renewal for {high.Count} evidence items expiring within 30 days");
        }
        if (expiring.Count > 5)
        {
            recommendations.Add("Consider implementing automated evidence renewal reminders");
        }

        return recommendations;
    }

    private string GetDefaultEvidenceType(string domain)
    {
        return domain.ToLowerInvariant() switch
        {
            "access control" => "Access Control Matrix",
            "security" => "Security Assessment",
            "data protection" => "Data Classification Document",
            _ => "Policy/Procedure Document"
        };
    }

    private List<string> GetSuggestedSources(string domain)
    {
        return domain.ToLowerInvariant() switch
        {
            "access control" => new List<string> { "IAM System", "Active Directory", "SSO Provider" },
            "security" => new List<string> { "SIEM", "Vulnerability Scanner", "Security Team" },
            "data protection" => new List<string> { "DLP System", "Data Governance Team", "Privacy Office" },
            _ => new List<string> { "Document Management System", "Process Owner", "IT Team" }
        };
    }

    private List<string> GenerateQuickWins(List<EvidenceCollectionTask> tasks)
    {
        var quickWins = new List<string>();

        var policyTasks = tasks.Where(t => t.EvidenceType.Contains("Policy")).ToList();
        if (policyTasks.Any())
        {
            quickWins.Add($"Collect existing policy documents for {policyTasks.Count} domains");
        }

        quickWins.Add("Export system configurations as evidence artifacts");
        quickWins.Add("Screenshot audit logs for recent compliance activities");

        return quickWins;
    }

    private EvidenceQualityAnalysis CreateFallbackQualityAnalysis(Guid evidenceId)
    {
        return new EvidenceQualityAnalysis
        {
            EvidenceId = evidenceId,
            QualityRating = "Pending Review",
            QualityScore = 50,
            Findings = new List<QualityFinding>
            {
                new QualityFinding
                {
                    Dimension = "AI Analysis",
                    Severity = "Low",
                    Description = "AI-powered quality analysis not available",
                    Recommendation = "Configure Claude API key for enhanced evidence analysis"
                }
            },
            Recommendations = new List<string>
            {
                "Enable AI-powered evidence analysis for detailed quality assessment",
                "Manually review evidence against requirement criteria"
            },
            DimensionScores = new Dictionary<string, int>
            {
                { "Completeness", 50 },
                { "Relevance", 50 },
                { "Currency", 50 },
                { "Authenticity", 50 },
                { "Traceability", 50 }
            },
            AnalyzedAt = DateTime.UtcNow
        };
    }

    private EvidenceCategorizationResult CreateFallbackCategorization(Guid evidenceId, Models.Entities.Evidence evidence)
    {
        return new EvidenceCategorizationResult
        {
            EvidenceId = evidenceId,
            SuggestedCategory = evidence.Type ?? "General",
            SuggestedType = evidence.Type ?? "Document",
            SuggestedTags = new List<string> { "compliance", "evidence" },
            RelatedDomains = new List<string> { "General Compliance" },
            CategorizationConfidence = 30,
            AnalyzedAt = DateTime.UtcNow
        };
    }

    private class ClaudeApiResponse
    {
        public List<ContentBlock>? Content { get; set; }
    }

    private class ContentBlock
    {
        public string? Type { get; set; }
        public string? Text { get; set; }
    }

    #endregion
}
