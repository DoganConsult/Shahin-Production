using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Models.Entities.Onboarding;
using GrcMvc.Models.Entities.Compliance;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// 43-Layer Architecture: Onboarding Control Plane Service Implementation
    /// Manages the complete onboarding flow with audit trail and explainability.
    /// </summary>
    public class OnboardingControlPlaneService : IOnboardingControlPlaneService
    {
        private readonly GrcDbContext _context;
        private readonly ILogger<OnboardingControlPlaneService> _logger;
        private readonly IAuditEventService _auditService;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            PropertyNameCaseInsensitive = true
        };

        public OnboardingControlPlaneService(
            GrcDbContext context,
            ILogger<OnboardingControlPlaneService> logger,
            IAuditEventService auditService)
        {
            _context = context;
            _logger = logger;
            _auditService = auditService;
        }

        #region Layer 14: Answer Snapshots

        public async Task<OnboardingAnswerSnapshot> CreateSnapshotAsync(
            Guid wizardId,
            int completedStep,
            string sectionCode,
            string answersJson,
            string userId,
            string? ipAddress = null,
            string? userAgent = null)
        {
            var wizard = await _context.OnboardingWizards.FindAsync(wizardId);
            if (wizard == null)
                throw new InvalidOperationException($"Wizard {wizardId} not found");

            // Get next version number
            var maxVersion = await _context.OnboardingAnswerSnapshots
                .Where(s => s.OnboardingWizardId == wizardId)
                .MaxAsync(s => (int?)s.Version) ?? 0;

            var snapshot = new OnboardingAnswerSnapshot
            {
                Id = Guid.NewGuid(),
                TenantId = wizard.TenantId,
                OnboardingWizardId = wizardId,
                Version = maxVersion + 1,
                CompletedStep = completedStep,
                SectionCode = sectionCode,
                AnswersJson = answersJson,
                AnswersHash = ComputeHash(answersJson),
                CreatedByUserId = userId,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                SnapshotAt = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = userId
            };

            _context.OnboardingAnswerSnapshots.Add(snapshot);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Created snapshot v{Version} for wizard {WizardId} at step {Step}",
                snapshot.Version, wizardId, completedStep);

            return snapshot;
        }

        public async Task<List<OnboardingAnswerSnapshot>> GetSnapshotsAsync(Guid wizardId)
        {
            return await _context.OnboardingAnswerSnapshots
                .Where(s => s.OnboardingWizardId == wizardId)
                .OrderBy(s => s.Version)
                .ToListAsync();
        }

        public async Task<OnboardingAnswerSnapshot?> GetLatestSnapshotAsync(Guid wizardId)
        {
            return await _context.OnboardingAnswerSnapshots
                .Where(s => s.OnboardingWizardId == wizardId)
                .OrderByDescending(s => s.Version)
                .FirstOrDefaultAsync();
        }

        public async Task<OnboardingAnswerSnapshot> MarkSnapshotAsFinalAsync(Guid snapshotId)
        {
            var snapshot = await _context.OnboardingAnswerSnapshots.FindAsync(snapshotId);
            if (snapshot == null)
                throw new InvalidOperationException($"Snapshot {snapshotId} not found");

            snapshot.IsFinalSnapshot = true;
            snapshot.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return snapshot;
        }

        #endregion

        #region Layer 15-16: Rules Evaluation & Derived Outputs

        public async Task<RulesEvaluationResult> EvaluateRulesAsync(
            Guid wizardId,
            int triggerStep,
            Dictionary<string, object> inputContext)
        {
            var result = new RulesEvaluationResult();
            var wizard = await _context.OnboardingWizards
                .Include(w => w.Tenant)
                .FirstOrDefaultAsync(w => w.Id == wizardId);

            if (wizard == null)
                throw new InvalidOperationException($"Wizard {wizardId} not found");

            var inputJson = JsonSerializer.Serialize(inputContext, _jsonOptions);

            // Get applicable rules from database or use built-in rules
            var rules = GetBuiltInRules(triggerStep);

            foreach (var rule in rules)
            {
                var startTime = DateTime.UtcNow;
                var evaluationLog = new Models.Entities.Onboarding.RulesEvaluationLog
                {
                    Id = Guid.NewGuid(),
                    TenantId = wizard.TenantId,
                    OnboardingWizardId = wizardId,
                    TriggerStep = triggerStep,
                    RuleCode = rule.Code,
                    RuleName = rule.Name,
                    RuleVersion = "1.0",
                    InputContextJson = inputJson,
                    EvaluatedAt = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow
                };

                try
                {
                    var (matched, output, reason) = EvaluateRule(rule, inputContext);
                    evaluationLog.Result = matched ? RuleEvaluationResults.Matched : RuleEvaluationResults.NotMatched;
                    evaluationLog.OutputJson = JsonSerializer.Serialize(output, _jsonOptions);
                    evaluationLog.ReasonText = reason;
                    evaluationLog.ConfidenceScore = matched ? 1.0m : 0m;

                    if (matched)
                    {
                        result.MatchedRules++;

                        // Create derived output if rule produces one
                        if (output.ContainsKey("derivedOutput"))
                        {
                            var derivedOutput = await CreateDerivedOutputFromRule(wizard, rule, output, triggerStep, evaluationLog.Id);
                            result.DerivedOutputs.Add(derivedOutput);

                            // Create explainability
                            var explanation = await CreateExplainabilityAsync(
                                wizard.TenantId,
                                rule.DecisionType,
                                output.GetValueOrDefault("subjectCode")?.ToString() ?? rule.Code,
                                output.GetValueOrDefault("subjectName")?.ToString() ?? rule.Name,
                                "INCLUDED",
                                reason,
                                wizardId: wizardId,
                                rulesEvaluationLogId: evaluationLog.Id);

                            result.Explanations.Add(explanation);
                        }
                    }
                }
                catch (Exception ex)
                {
                    evaluationLog.Result = RuleEvaluationResults.Error;
                    evaluationLog.ErrorMessage = ex.Message;
                    evaluationLog.ErrorStackTrace = ex.StackTrace;
                    _logger.LogError(ex, "Error evaluating rule {RuleCode}", rule.Code);
                }

                evaluationLog.EvaluationDurationMs = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
                _context.OnboardingRulesEvaluationLogs.Add(evaluationLog);
                result.Evaluations.Add(evaluationLog);
                result.TotalRulesEvaluated++;
            }

            await _context.SaveChangesAsync();
            return result;
        }

        public async Task<List<OnboardingDerivedOutput>> GetDerivedOutputsAsync(Guid wizardId, string? outputType = null)
        {
            var query = _context.OnboardingDerivedOutputs
                .Where(o => o.OnboardingWizardId == wizardId && o.IsActive);

            if (!string.IsNullOrEmpty(outputType))
                query = query.Where(o => o.OutputType == outputType);

            return await query.OrderBy(o => o.Priority).ToListAsync();
        }

        public async Task<OnboardingDerivedOutput> CreateDerivedOutputAsync(
            Guid wizardId,
            string outputType,
            string outputCode,
            string outputName,
            string outputPayloadJson,
            string applicability,
            int priority,
            int derivedAtStep,
            Guid? rulesEvaluationLogId = null)
        {
            var wizard = await _context.OnboardingWizards.FindAsync(wizardId);
            if (wizard == null)
                throw new InvalidOperationException($"Wizard {wizardId} not found");

            // Check if output already exists
            var existing = await _context.OnboardingDerivedOutputs
                .FirstOrDefaultAsync(o =>
                    o.OnboardingWizardId == wizardId &&
                    o.OutputType == outputType &&
                    o.OutputCode == outputCode &&
                    o.IsActive);

            if (existing != null)
            {
                // Update existing
                existing.OutputPayloadJson = outputPayloadJson;
                existing.Version++;
                existing.ModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return existing;
            }

            var output = new OnboardingDerivedOutput
            {
                Id = Guid.NewGuid(),
                TenantId = wizard.TenantId,
                OnboardingWizardId = wizardId,
                OutputType = outputType,
                OutputCode = outputCode,
                OutputName = outputName,
                OutputPayloadJson = outputPayloadJson,
                Applicability = applicability,
                Priority = priority,
                DerivedAtStep = derivedAtStep,
                RulesEvaluationLogId = rulesEvaluationLogId,
                DerivedAt = DateTime.UtcNow,
                IsActive = true,
                Version = 1,
                CreatedDate = DateTime.UtcNow
            };

            _context.OnboardingDerivedOutputs.Add(output);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Created derived output {OutputCode} ({OutputType}) for wizard {WizardId}",
                outputCode, outputType, wizardId);

            return output;
        }

        #endregion

        #region Layer 17: Explainability

        public async Task<ExplainabilityPayload> CreateExplainabilityAsync(
            Guid tenantId,
            string decisionType,
            string subjectCode,
            string subjectName,
            string decision,
            string primaryReason,
            string? primaryReasonAr = null,
            Dictionary<string, object>? inputFactors = null,
            List<string>? supportingReferences = null,
            Guid? wizardId = null,
            Guid? rulesEvaluationLogId = null)
        {
            var payload = new ExplainabilityPayload
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                DecisionType = decisionType,
                SubjectCode = subjectCode,
                SubjectName = subjectName,
                Decision = decision,
                PrimaryReason = primaryReason,
                PrimaryReasonAr = primaryReasonAr,
                InputFactorsJson = inputFactors != null
                    ? JsonSerializer.Serialize(inputFactors, _jsonOptions)
                    : "{}",
                SupportingReferencesJson = supportingReferences != null
                    ? JsonSerializer.Serialize(supportingReferences, _jsonOptions)
                    : "[]",
                OnboardingWizardId = wizardId,
                RulesEvaluationLogId = rulesEvaluationLogId,
                IsOverridable = true,
                GeneratedAt = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow
            };

            _context.ExplainabilityPayloads.Add(payload);
            await _context.SaveChangesAsync();

            return payload;
        }

        public async Task<List<ExplainabilityPayload>> GetExplainabilityAsync(
            Guid tenantId,
            string? decisionType = null,
            string? subjectCode = null)
        {
            var query = _context.ExplainabilityPayloads
                .Where(e => e.TenantId == tenantId);

            if (!string.IsNullOrEmpty(decisionType))
                query = query.Where(e => e.DecisionType == decisionType);

            if (!string.IsNullOrEmpty(subjectCode))
                query = query.Where(e => e.SubjectCode == subjectCode);

            return await query.OrderByDescending(e => e.GeneratedAt).ToListAsync();
        }

        public async Task<ExplainabilityPayload> OverrideDecisionAsync(
            Guid payloadId,
            string newDecision,
            string justification,
            string userId)
        {
            var payload = await _context.ExplainabilityPayloads.FindAsync(payloadId);
            if (payload == null)
                throw new InvalidOperationException($"Explainability payload {payloadId} not found");

            if (!payload.IsOverridable)
                throw new InvalidOperationException("This decision cannot be overridden");

            payload.OverriddenByUserId = userId;
            payload.OverriddenAt = DateTime.UtcNow;
            payload.OverrideDecision = newDecision;
            payload.OverrideJustification = justification;
            payload.ModifiedDate = DateTime.UtcNow;
            payload.ModifiedBy = userId;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Decision {SubjectCode} overridden by {UserId}: {OldDecision} -> {NewDecision}",
                payload.SubjectCode, userId, payload.Decision, newDecision);

            return payload;
        }

        #endregion

        #region Layer 31-35: Tenant Compliance Resolution

        public async Task<List<TenantFrameworkSelection>> DeriveFrameworkSelectionsAsync(Guid tenantId, Guid wizardId)
        {
            var wizard = await _context.OnboardingWizards.FindAsync(wizardId);
            if (wizard == null)
                throw new InvalidOperationException($"Wizard {wizardId} not found");

            var selections = new List<TenantFrameworkSelection>();

            // Parse wizard answers
            var sector = wizard.IndustrySector;
            var country = wizard.CountryOfIncorporation;
            var mandatoryFrameworks = ParseJsonArray(wizard.MandatoryFrameworksJson);

            // Get applicable frameworks from lookup
            var frameworks = await _context.LookupFrameworks
                .Where(f => f.IsActive)
                .ToListAsync();

            foreach (var framework in frameworks)
            {
                var isMandatory = false;
                var reason = "";

                // Check if framework is mandatory for this sector/country
                if (framework.IsMandatory && framework.CountryCode == country)
                {
                    var applicableSectors = ParseJsonArray(framework.ApplicableSectors ?? "[]");
                    if (applicableSectors.Count == 0 || applicableSectors.Contains(sector))
                    {
                        isMandatory = true;
                        reason = $"Mandatory for {sector} sector in {country}";
                    }
                }

                // Check if user explicitly selected it
                if (mandatoryFrameworks.Contains(framework.Code))
                {
                    isMandatory = true;
                    reason = "Explicitly selected by organization";
                }

                if (isMandatory)
                {
                    var selection = new TenantFrameworkSelection
                    {
                        Id = Guid.NewGuid(),
                        TenantId = tenantId,
                        FrameworkCode = framework.Code,
                        FrameworkName = framework.NameEn,
                        FrameworkNameAr = framework.NameAr,
                        FrameworkVersion = framework.Version,
                        SelectionType = mandatoryFrameworks.Contains(framework.Code)
                            ? FrameworkSelectionTypes.Voluntary
                            : FrameworkSelectionTypes.Mandatory,
                        Applicability = "MANDATORY",
                        Priority = framework.Priority,
                        SelectionReason = reason,
                        RegulatorCode = framework.IssuingBody,
                        EstimatedControlCount = framework.EstimatedControlCount,
                        EstimatedImplementationMonths = framework.EstimatedImplementationMonths,
                        ComplianceStatus = "NOT_STARTED",
                        SelectedAt = DateTime.UtcNow,
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    };

                    // Deactivate existing selection for same framework
                    var existing = await _context.TenantFrameworkSelections
                        .Where(s => s.TenantId == tenantId && s.FrameworkCode == framework.Code && s.IsActive)
                        .ToListAsync();

                    foreach (var e in existing)
                    {
                        e.IsActive = false;
                        e.DeactivatedAt = DateTime.UtcNow;
                        e.DeactivationReason = "Replaced by new onboarding";
                    }

                    _context.TenantFrameworkSelections.Add(selection);
                    selections.Add(selection);

                    // Create explainability
                    await CreateExplainabilityAsync(
                        tenantId,
                        ExplainabilityDecisionTypes.FrameworkSelection,
                        framework.Code,
                        framework.NameEn,
                        "INCLUDED",
                        reason,
                        wizardId: wizardId,
                        inputFactors: new Dictionary<string, object>
                        {
                            ["sector"] = sector,
                            ["country"] = country,
                            ["isMandatory"] = framework.IsMandatory
                        });
                }
            }

            await _context.SaveChangesAsync();
            return selections;
        }

        public async Task<List<TenantOverlay>> ApplyOverlaysAsync(Guid tenantId, Guid wizardId)
        {
            var wizard = await _context.OnboardingWizards.FindAsync(wizardId);
            if (wizard == null)
                throw new InvalidOperationException($"Wizard {wizardId} not found");

            var overlays = new List<TenantOverlay>();

            // Jurisdiction overlay
            if (!string.IsNullOrEmpty(wizard.CountryOfIncorporation))
            {
                var overlay = new TenantOverlay
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    OverlayType = OverlayTypes.Jurisdiction,
                    OverlayCode = $"OVL_JURISDICTION_{wizard.CountryOfIncorporation}",
                    OverlayName = $"{wizard.CountryOfIncorporation} Jurisdiction Requirements",
                    SelectionType = "AUTO",
                    ApplicationReason = $"Organization is incorporated in {wizard.CountryOfIncorporation}",
                    TriggerCondition = $"CountryOfIncorporation={wizard.CountryOfIncorporation}",
                    Priority = 1,
                    AppliedAt = DateTime.UtcNow,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };
                _context.TenantOverlays.Add(overlay);
                overlays.Add(overlay);
            }

            // Sector overlay
            if (!string.IsNullOrEmpty(wizard.IndustrySector))
            {
                var overlay = new TenantOverlay
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    OverlayType = OverlayTypes.Sector,
                    OverlayCode = $"OVL_SECTOR_{wizard.IndustrySector}",
                    OverlayName = $"{wizard.IndustrySector} Sector Requirements",
                    SelectionType = "AUTO",
                    ApplicationReason = $"Organization operates in {wizard.IndustrySector} sector",
                    TriggerCondition = $"IndustrySector={wizard.IndustrySector}",
                    Priority = 2,
                    AppliedAt = DateTime.UtcNow,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };
                _context.TenantOverlays.Add(overlay);
                overlays.Add(overlay);
            }

            // Data type overlays
            var dataTypes = ParseJsonArray(wizard.DataTypesProcessedJson);
            foreach (var dataType in dataTypes)
            {
                var overlay = new TenantOverlay
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    OverlayType = OverlayTypes.DataType,
                    OverlayCode = $"OVL_DATA_{dataType}",
                    OverlayName = $"{dataType} Data Protection Requirements",
                    SelectionType = "AUTO",
                    ApplicationReason = $"Organization processes {dataType} data",
                    TriggerCondition = $"DataType={dataType}",
                    Priority = 3,
                    AppliedAt = DateTime.UtcNow,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };
                _context.TenantOverlays.Add(overlay);
                overlays.Add(overlay);
            }

            // Cloud overlay
            var cloudProviders = ParseJsonArray(wizard.CloudProvidersJson);
            if (cloudProviders.Count > 0)
            {
                var overlay = new TenantOverlay
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    OverlayType = OverlayTypes.Technology,
                    OverlayCode = "OVL_TECH_CLOUD",
                    OverlayName = "Cloud Security Requirements",
                    SelectionType = "AUTO",
                    ApplicationReason = $"Organization uses cloud providers: {string.Join(", ", cloudProviders)}",
                    TriggerCondition = $"CloudProviders=[{string.Join(",", cloudProviders)}]",
                    Priority = 4,
                    AppliedAt = DateTime.UtcNow,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };
                _context.TenantOverlays.Add(overlay);
                overlays.Add(overlay);
            }

            await _context.SaveChangesAsync();
            return overlays;
        }

        public async Task<List<TenantControlSet>> ResolveControlSetAsync(Guid tenantId)
        {
            // Get framework selections
            var selections = await _context.TenantFrameworkSelections
                .Where(s => s.TenantId == tenantId && s.IsActive)
                .ToListAsync();

            var controlSet = new List<TenantControlSet>();

            // For each selected framework, add its controls
            foreach (var selection in selections)
            {
                // Get controls from catalog by framework code
                var frameworkControls = await _context.Controls
                    .Where(c => c.SourceFrameworkCode == selection.FrameworkCode && c.Status == "Active")
                    .Take(50) // Limit per framework
                    .ToListAsync();

                var priority = 1;
                foreach (var control in frameworkControls)
                {
                    var tenantControl = new TenantControlSet
                    {
                        Id = Guid.NewGuid(),
                        TenantId = tenantId,
                        CatalogControlId = control.Id,
                        ControlCode = control.ControlId,
                        ControlName = control.Name,
                        FrameworkCode = selection.FrameworkCode,
                        ControlDomain = control.Category,
                        Source = ControlSources.Baseline,
                        SourceCode = selection.FrameworkCode,
                        ApplicabilityStatus = ControlApplicabilityStatuses.Applicable,
                        IsMandatory = selection.SelectionType == FrameworkSelectionTypes.Mandatory,
                        Priority = priority++,
                        EvidenceFrequency = control.Frequency ?? "QUARTERLY",
                        ImplementationStatus = "NOT_STARTED",
                        ComplianceStatus = "NOT_ASSESSED",
                        AddedAt = DateTime.UtcNow,
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    };

                    _context.TenantControlSets.Add(tenantControl);
                    controlSet.Add(tenantControl);
                }
            }

            await _context.SaveChangesAsync();
            return controlSet;
        }

        public async Task<List<TenantScopeBoundary>> CreateScopeBoundariesAsync(Guid tenantId, Guid wizardId)
        {
            var wizard = await _context.OnboardingWizards.FindAsync(wizardId);
            if (wizard == null)
                throw new InvalidOperationException($"Wizard {wizardId} not found");

            var boundaries = new List<TenantScopeBoundary>();

            // Legal entities
            var legalEntities = ParseJsonArray(wizard.InScopeLegalEntitiesJson);
            foreach (var entity in legalEntities)
            {
                var boundary = new TenantScopeBoundary
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    ScopeType = ScopeTypes.LegalEntity,
                    ScopeCode = $"LE_{entity.Replace(" ", "_").ToUpper()}",
                    ScopeName = entity,
                    IsInScope = true,
                    CriticalityTier = CriticalityTiers.Tier2,
                    AddedAt = DateTime.UtcNow,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };
                _context.TenantScopeBoundaries.Add(boundary);
                boundaries.Add(boundary);
            }

            // Systems
            var systems = ParseJsonArray(wizard.InScopeSystemsJson);
            foreach (var system in systems)
            {
                var boundary = new TenantScopeBoundary
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    ScopeType = ScopeTypes.System,
                    ScopeCode = $"SYS_{system.Replace(" ", "_").ToUpper()}",
                    ScopeName = system,
                    IsInScope = true,
                    CriticalityTier = CriticalityTiers.Tier2,
                    Environment = wizard.InScopeEnvironments ?? "BOTH",
                    AddedAt = DateTime.UtcNow,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };
                _context.TenantScopeBoundaries.Add(boundary);
                boundaries.Add(boundary);
            }

            // Locations
            var locations = ParseJsonArray(wizard.InScopeLocationsJson);
            foreach (var location in locations)
            {
                var boundary = new TenantScopeBoundary
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    ScopeType = ScopeTypes.Location,
                    ScopeCode = $"LOC_{location.Replace(" ", "_").ToUpper()}",
                    ScopeName = location,
                    IsInScope = true,
                    CriticalityTier = CriticalityTiers.Tier3,
                    Location = location,
                    AddedAt = DateTime.UtcNow,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };
                _context.TenantScopeBoundaries.Add(boundary);
                boundaries.Add(boundary);
            }

            // Exclusions
            var exclusions = ParseJsonArray(wizard.ExclusionsJson);
            foreach (var exclusion in exclusions)
            {
                var boundary = new TenantScopeBoundary
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    ScopeType = ScopeTypes.System,
                    ScopeCode = $"EXCL_{exclusion.Replace(" ", "_").ToUpper()}",
                    ScopeName = exclusion,
                    IsInScope = false,
                    ExclusionRationale = "Excluded during onboarding",
                    CriticalityTier = CriticalityTiers.Tier3,
                    AddedAt = DateTime.UtcNow,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };
                _context.TenantScopeBoundaries.Add(boundary);
                boundaries.Add(boundary);
            }

            await _context.SaveChangesAsync();
            return boundaries;
        }

        public async Task<TenantRiskProfile> CalculateRiskProfileAsync(Guid tenantId, Guid wizardId)
        {
            var wizard = await _context.OnboardingWizards.FindAsync(wizardId);
            if (wizard == null)
                throw new InvalidOperationException($"Wizard {wizardId} not found");

            var dataTypes = ParseJsonArray(wizard.DataTypesProcessedJson);

            var profile = new TenantRiskProfile
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ProcessesPII = dataTypes.Any(d => d.Contains("PII") || d.Contains("Personal")),
                ProcessesPCI = wizard.HasPaymentCardData,
                ProcessesPHI = dataTypes.Any(d => d.Contains("PHI") || d.Contains("Health")),
                ProcessesClassifiedData = dataTypes.Any(d => d.Contains("Classified") || d.Contains("Government")),
                DataTypesJson = wizard.DataTypesProcessedJson,
                HasCrossBorderTransfers = wizard.HasCrossBorderDataTransfers,
                TransferCountriesJson = wizard.CrossBorderTransferCountriesJson,
                HasThirdPartySharing = wizard.HasThirdPartyDataProcessing,
                ThirdPartiesJson = wizard.ThirdPartyDataProcessorsJson,
                HasInternetFacing = wizard.HasInternetFacingSystems,
                InternetFacingSystemsJson = wizard.InternetFacingSystemsJson,
                UsesCloud = ParseJsonArray(wizard.CloudProvidersJson).Count > 0,
                CloudProvidersJson = wizard.CloudProvidersJson,
                CustomerVolumeTier = wizard.CustomerVolumeTier,
                TransactionVolumeTier = wizard.TransactionVolumeTier,
                CalculatedAt = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow
            };

            // Calculate risk score
            decimal score = 0;
            if (profile.ProcessesPII) score += 15;
            if (profile.ProcessesPCI) score += 25;
            if (profile.ProcessesPHI) score += 20;
            if (profile.ProcessesClassifiedData) score += 30;
            if (profile.HasCrossBorderTransfers) score += 15;
            if (profile.HasThirdPartySharing) score += 10;
            if (profile.HasInternetFacing) score += 10;
            if (profile.UsesCloud) score += 5;

            profile.OverallRiskScore = Math.Min(100, score);
            profile.RiskTier = profile.OverallRiskScore switch
            {
                >= 70 => RiskTiers.Critical,
                >= 50 => RiskTiers.High,
                >= 30 => RiskTiers.Medium,
                _ => RiskTiers.Low
            };

            profile.RiskScoreBreakdownJson = JsonSerializer.Serialize(new
            {
                pii = profile.ProcessesPII ? 15 : 0,
                pci = profile.ProcessesPCI ? 25 : 0,
                phi = profile.ProcessesPHI ? 20 : 0,
                classified = profile.ProcessesClassifiedData ? 30 : 0,
                crossBorder = profile.HasCrossBorderTransfers ? 15 : 0,
                thirdParty = profile.HasThirdPartySharing ? 10 : 0,
                internetFacing = profile.HasInternetFacing ? 10 : 0,
                cloud = profile.UsesCloud ? 5 : 0,
                total = profile.OverallRiskScore
            }, _jsonOptions);

            // Deactivate existing profile
            var existing = await _context.TenantRiskProfiles
                .FirstOrDefaultAsync(p => p.TenantId == tenantId);
            if (existing != null)
                _context.TenantRiskProfiles.Remove(existing);

            _context.TenantRiskProfiles.Add(profile);
            await _context.SaveChangesAsync();

            return profile;
        }

        #endregion

        #region Complete Onboarding Flow

        public async Task<OnboardingCompletionResult> CompleteOnboardingAsync(Guid tenantId, Guid wizardId, string userId)
        {
            var result = new OnboardingCompletionResult
            {
                TenantId = tenantId,
                WizardId = wizardId,
                CompletedAt = DateTime.UtcNow
            };

            try
            {
                var wizard = await _context.OnboardingWizards.FindAsync(wizardId);
                if (wizard == null)
                {
                    result.Errors.Add($"Wizard {wizardId} not found");
                    return result;
                }

                _logger.LogInformation("Starting onboarding completion for tenant {TenantId}", tenantId);

                // 1. Create final snapshot
                var snapshot = await CreateSnapshotAsync(
                    wizardId,
                    12,
                    "FINAL",
                    wizard.AllAnswersJson,
                    userId);
                await MarkSnapshotAsFinalAsync(snapshot.Id);
                result.FinalSnapshot = snapshot;

                // 2. Derive framework selections
                result.FrameworkSelections = await DeriveFrameworkSelectionsAsync(tenantId, wizardId);
                _logger.LogInformation("Derived {Count} framework selections", result.FrameworkSelections.Count);

                // 3. Apply overlays
                result.Overlays = await ApplyOverlaysAsync(tenantId, wizardId);
                _logger.LogInformation("Applied {Count} overlays", result.Overlays.Count);

                // 4. Create scope boundaries
                result.ScopeBoundaries = await CreateScopeBoundariesAsync(tenantId, wizardId);
                _logger.LogInformation("Created {Count} scope boundaries", result.ScopeBoundaries.Count);

                // 5. Calculate risk profile
                result.RiskProfile = await CalculateRiskProfileAsync(tenantId, wizardId);
                _logger.LogInformation("Calculated risk profile: {Tier} ({Score}%)",
                    result.RiskProfile.RiskTier, result.RiskProfile.OverallRiskScore);

                // 6. Resolve control set
                result.ControlSet = await ResolveControlSetAsync(tenantId);
                _logger.LogInformation("Resolved {Count} controls", result.ControlSet.Count);

                // 7. Get all explanations
                result.Explanations = await GetExplainabilityAsync(tenantId);

                // 8. Update wizard status
                wizard.WizardStatus = "Completed";
                wizard.CompletedAt = DateTime.UtcNow;
                wizard.CompletedByUserId = userId;
                wizard.ModifiedDate = DateTime.UtcNow;
                wizard.ModifiedBy = userId;
                await _context.SaveChangesAsync();

                // 9. Log audit event
                await _auditService.LogEventAsync(
                    tenantId: tenantId,
                    eventType: "OnboardingCompleted",
                    affectedEntityType: "OnboardingWizard",
                    affectedEntityId: wizardId.ToString(),
                    action: "Complete",
                    actor: userId,
                    payloadJson: JsonSerializer.Serialize(new
                    {
                        frameworks = result.FrameworkSelections.Count,
                        overlays = result.Overlays.Count,
                        controls = result.ControlSet.Count,
                        scopeBoundaries = result.ScopeBoundaries.Count,
                        riskTier = result.RiskProfile?.RiskTier
                    }, _jsonOptions));

                result.Success = true;
                _logger.LogInformation("Onboarding completed successfully for tenant {TenantId}", tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing onboarding for tenant {TenantId}", tenantId);
                result.Errors.Add(ex.Message);
            }

            return result;
        }

        #endregion

        #region Private Helpers

        private static string ComputeHash(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private static List<string> ParseJsonArray(string? json)
        {
            if (string.IsNullOrEmpty(json) || json == "[]")
                return new List<string>();

            try
            {
                return JsonSerializer.Deserialize<List<string>>(json, _jsonOptions) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        private List<BuiltInRule> GetBuiltInRules(int step)
        {
            // Built-in rules for each step
            return step switch
            {
                1 => new List<BuiltInRule>
                {
                    new("RULE_COUNTRY_KSA", "KSA Jurisdiction Rule", "Check if org is in Saudi Arabia", ExplainabilityDecisionTypes.OverlayApplication),
                    new("RULE_SECTOR_BANKING", "Banking Sector Rule", "Check if org is in banking sector", ExplainabilityDecisionTypes.FrameworkSelection)
                },
                3 => new List<BuiltInRule>
                {
                    new("RULE_SAMA_APPLICABILITY", "SAMA Framework Applicability", "Check if SAMA framework applies", ExplainabilityDecisionTypes.FrameworkSelection),
                    new("RULE_NCA_APPLICABILITY", "NCA Framework Applicability", "Check if NCA-ECC applies", ExplainabilityDecisionTypes.FrameworkSelection)
                },
                5 => new List<BuiltInRule>
                {
                    new("RULE_PCI_APPLICABILITY", "PCI DSS Applicability", "Check if PCI DSS applies", ExplainabilityDecisionTypes.FrameworkSelection),
                    new("RULE_PDPL_APPLICABILITY", "PDPL Applicability", "Check if PDPL applies", ExplainabilityDecisionTypes.FrameworkSelection)
                },
                _ => new List<BuiltInRule>()
            };
        }

        private (bool matched, Dictionary<string, object> output, string reason) EvaluateRule(
            BuiltInRule rule,
            Dictionary<string, object> context)
        {
            // Simple rule evaluation logic
            var output = new Dictionary<string, object>();
            var matched = false;
            var reason = "";

            switch (rule.Code)
            {
                case "RULE_COUNTRY_KSA":
                    if (context.TryGetValue("country", out var country) && country?.ToString() == "SA")
                    {
                        matched = true;
                        reason = "Organization is incorporated in Saudi Arabia";
                        output["derivedOutput"] = true;
                        output["subjectCode"] = "OVL_KSA";
                        output["subjectName"] = "KSA Jurisdiction Overlay";
                    }
                    break;

                case "RULE_SECTOR_BANKING":
                    if (context.TryGetValue("sector", out var sector) &&
                        (sector?.ToString()?.Contains("BANKING") == true || sector?.ToString()?.Contains("FINANCE") == true))
                    {
                        matched = true;
                        reason = "Organization operates in financial services sector";
                        output["derivedOutput"] = true;
                        output["subjectCode"] = "SAMA-CSF";
                        output["subjectName"] = "SAMA Cybersecurity Framework";
                    }
                    break;

                case "RULE_SAMA_APPLICABILITY":
                    if (context.TryGetValue("sector", out var samaSector) &&
                        new[] { "BANKING", "INSURANCE", "FINTECH" }.Any(s =>
                            samaSector?.ToString()?.ToUpper().Contains(s) == true))
                    {
                        matched = true;
                        reason = "SAMA framework is mandatory for financial institutions in Saudi Arabia";
                        output["derivedOutput"] = true;
                        output["subjectCode"] = "SAMA-CSF";
                        output["subjectName"] = "SAMA Cybersecurity Framework";
                    }
                    break;

                case "RULE_NCA_APPLICABILITY":
                    if (context.TryGetValue("country", out var ncaCountry) && ncaCountry?.ToString() == "SA")
                    {
                        matched = true;
                        reason = "NCA-ECC is mandatory for all entities in Saudi Arabia";
                        output["derivedOutput"] = true;
                        output["subjectCode"] = "NCA-ECC";
                        output["subjectName"] = "NCA Essential Cybersecurity Controls";
                    }
                    break;

                case "RULE_PCI_APPLICABILITY":
                    if (context.TryGetValue("hasPaymentCardData", out var pci) && (bool)pci)
                    {
                        matched = true;
                        reason = "Organization processes payment card data";
                        output["derivedOutput"] = true;
                        output["subjectCode"] = "PCI-DSS";
                        output["subjectName"] = "PCI DSS";
                    }
                    break;

                case "RULE_PDPL_APPLICABILITY":
                    if (context.TryGetValue("dataTypes", out var dataTypes))
                    {
                        var types = dataTypes as List<string> ?? new List<string>();
                        if (types.Any(t => t.Contains("Personal") || t.Contains("PII")))
                        {
                            matched = true;
                            reason = "Organization processes personal data subject to PDPL";
                            output["derivedOutput"] = true;
                            output["subjectCode"] = "PDPL";
                            output["subjectName"] = "Personal Data Protection Law";
                        }
                    }
                    break;
            }

            return (matched, output, reason);
        }

        private async Task<OnboardingDerivedOutput> CreateDerivedOutputFromRule(
            OnboardingWizard wizard,
            BuiltInRule rule,
            Dictionary<string, object> output,
            int step,
            Guid evaluationLogId)
        {
            return await CreateDerivedOutputAsync(
                wizard.Id,
                DerivedOutputTypes.Baseline,
                output.GetValueOrDefault("subjectCode")?.ToString() ?? rule.Code,
                output.GetValueOrDefault("subjectName")?.ToString() ?? rule.Name,
                JsonSerializer.Serialize(output, _jsonOptions),
                "MANDATORY",
                1,
                step,
                evaluationLogId);
        }

        private class BuiltInRule
        {
            public string Code { get; }
            public string Name { get; }
            public string Description { get; }
            public string DecisionType { get; }

            public BuiltInRule(string code, string name, string description, string decisionType)
            {
                Code = code;
                Name = name;
                Description = description;
                DecisionType = decisionType;
            }
        }

        #endregion
    }
}
