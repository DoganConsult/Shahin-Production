using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Models.DTOs;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Service for comprehensive onboarding wizard API operations.
    /// Handles all 12 sections (A-L) with progressive save and validation.
    /// Maps between API DTOs and the existing OnboardingWizard entity.
    /// </summary>
    public class OnboardingWizardService : IOnboardingWizardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRulesEngineService _rulesEngine;
        private readonly IAuditEventService _auditService;
        private readonly IOnboardingCoverageService _coverageService;
        private readonly IFieldRegistryService _fieldRegistryService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OnboardingWizardService> _logger;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            PropertyNameCaseInsensitive = true
        };

        public OnboardingWizardService(
            IUnitOfWork unitOfWork,
            IRulesEngineService rulesEngine,
            IAuditEventService auditService,
            IOnboardingCoverageService coverageService,
            IFieldRegistryService fieldRegistryService,
            IConfiguration configuration,
            ILogger<OnboardingWizardService> logger)
        {
            _unitOfWork = unitOfWork;
            _rulesEngine = rulesEngine;
            _auditService = auditService;
            _coverageService = coverageService;
            _fieldRegistryService = fieldRegistryService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<OnboardingWizard> StartWizardAsync(Guid tenantId, string userId)
        {
            var existing = await GetWizardAsync(tenantId);
            if (existing != null)
            {
                _logger.LogInformation("Wizard already exists for tenant {TenantId}", tenantId);
                return existing;
            }

            var wizard = new OnboardingWizard
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                WizardStatus = "InProgress",
                CurrentStep = 1,
                StartedAt = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = userId
            };

            await _unitOfWork.OnboardingWizards.AddAsync(wizard);
            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogEventAsync(
                tenantId: tenantId,
                eventType: "OnboardingWizardStarted",
                affectedEntityType: "OnboardingWizard",
                affectedEntityId: wizard.Id.ToString(),
                action: "Create",
                actor: userId,
                payloadJson: JsonSerializer.Serialize(new { wizard.Id, wizard.TenantId })
            );

            _logger.LogInformation("Started onboarding wizard for tenant {TenantId}", tenantId);
            return wizard;
        }

        public async Task<OnboardingWizardStateDto?> GetWizardStateAsync(Guid tenantId)
        {
            var wizard = await GetWizardAsync(tenantId);
            if (wizard == null) return null;

            return MapToDto(wizard);
        }

        public async Task<WizardProgressSummary> GetProgressAsync(Guid tenantId)
        {
            var wizard = await GetWizardAsync(tenantId);
            if (wizard == null)
            {
                return new WizardProgressSummary
                {
                    TenantId = tenantId,
                    WizardStatus = "NotStarted",
                    CurrentStep = 0,
                    ProgressPercent = 0,
                    CanComplete = false
                };
            }

            var completedSections = GetCompletedSections(wizard);
            var sections = BuildSectionProgress(completedSections);
            var requiredComplete = new[] { "A", "D", "E", "F", "H", "I" }.All(s => completedSections.Contains(s));

            // Add coverage validation if enabled
            var enableCoverageValidation = _configuration?.GetValue<bool>("Onboarding:EnableCoverageValidation", true) ?? true;
            // #region agent log
            System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "A", location = "OnboardingWizardService.cs:121", message = "Coverage validation enabled check", data = new { enableCoverageValidation, configValue = _configuration?.GetValue<bool>("Onboarding:EnableCoverageValidation", true), hasConfig = _configuration != null }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
            // #endregion
            Dictionary<string, CoverageValidationResult>? sectionCoverage = null;
            int overallCoveragePercent = 0;
            bool coverageComplete = false;

            if (enableCoverageValidation)
            {
                try
                {
                    // #region agent log
                    System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "B", location = "OnboardingWizardService.cs:130", message = "Calling GetAllSectionsCoverageAsync", data = new { tenantId }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
                    // #endregion
                    var allCoverage = await GetAllSectionsCoverageAsync(tenantId);
                    // #region agent log
                    System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "C", location = "OnboardingWizardService.cs:133", message = "GetAllSectionsCoverageAsync result", data = new { coverageCount = allCoverage?.Count ?? 0, hasCoverage = allCoverage != null, coverageKeys = allCoverage?.Keys.ToList() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
                    // #endregion
                    sectionCoverage = allCoverage;

                    // Update section progress with coverage information
                    foreach (var (sectionCode, sectionProgress) in sections)
                    {
                        if (allCoverage.TryGetValue(sectionCode, out var coverageResult))
                        {
                            sectionProgress.CoverageValidation = coverageResult;
                            sectionProgress.CoverageCompletionPercent = coverageResult.OverallCompletionPercentage;
                            sectionProgress.CoverageValid = coverageResult.IsValid;

                            if (coverageResult.NodeResults != null && coverageResult.NodeResults.Count > 0)
                            {
                                var nodeId = MapSectionToNodeId(sectionCode);
                                if (coverageResult.NodeResults.TryGetValue(nodeId, out var nodeResult))
                                {
                                    sectionProgress.MissingRequiredFields = nodeResult.MissingRequiredFields ?? new List<string>();
                                }
                            }
                        }
                    }

                    // Calculate overall coverage percentage
                    if (sections.Values.Any(s => s.CoverageCompletionPercent > 0))
                    {
                        overallCoveragePercent = (int)sections.Values
                            .Where(s => s.CoverageCompletionPercent > 0)
                            .Average(s => s.CoverageCompletionPercent);
                    }

                    // Check if all required sections have complete coverage
                    var requiredSections = new[] { "A", "D", "E", "F", "H", "I" };
                    coverageComplete = requiredSections.All(s =>
                    {
                        if (sections.TryGetValue(s, out var section))
                        {
                            return section.CoverageValid && section.CoverageCompletionPercent == 100;
                        }
                        return false;
                    });
                }
                catch (Exception ex)
                {
                    // #region agent log
                    System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "D", location = "OnboardingWizardService.cs:174", message = "Exception in coverage validation", data = new { exceptionType = ex.GetType().Name, exceptionMessage = ex.Message, stackTrace = ex.StackTrace }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
                    // #endregion
                    _logger.LogWarning(ex, "Error getting coverage validation for progress in tenant {TenantId}", tenantId);
                    // Don't fail progress if coverage check fails
                }
            }

            return new WizardProgressSummary
            {
                TenantId = tenantId,
                WizardStatus = wizard.WizardStatus,
                CurrentStep = wizard.CurrentStep,
                TotalSteps = 12,
                ProgressPercent = wizard.ProgressPercent,
                Sections = sections,
                CanComplete = requiredComplete && (!enableCoverageValidation || coverageComplete),
                LastUpdated = wizard.ModifiedDate ?? wizard.CreatedDate,
                SectionCoverage = sectionCoverage,
                OverallCoveragePercent = overallCoveragePercent,
                CoverageComplete = coverageComplete
            };
        }

        #region Section Save Methods

        public async Task<WizardSectionSaveResponse> SaveSectionAAsync(Guid tenantId, SectionA_OrganizationIdentity section, string userId)
        {
            var wizard = await EnsureWizardAsync(tenantId, userId);

            wizard.OrganizationLegalNameEn = section.LegalNameEn;
            wizard.OrganizationLegalNameAr = section.LegalNameAr;
            wizard.TradeName = section.TradeName;
            wizard.CountryOfIncorporation = section.CountryOfIncorporation;
            wizard.OperatingCountriesJson = JsonSerializer.Serialize(section.OperatingCountries, _jsonOptions);
            wizard.PrimaryHqLocation = section.PrimaryHqLocation;
            wizard.DefaultTimezone = section.Timezone;
            wizard.PrimaryLanguage = section.PrimaryLanguage;
            wizard.CorporateEmailDomainsJson = JsonSerializer.Serialize(section.CorporateEmailDomains, _jsonOptions);
            wizard.DomainVerificationMethod = section.DomainVerificationMethod;
            wizard.OrganizationType = section.OrgType;
            wizard.IndustrySector = section.Sectors.FirstOrDefault() ?? string.Empty;
            wizard.BusinessLinesJson = JsonSerializer.Serialize(section.BusinessLines, _jsonOptions);
            wizard.HasDataResidencyRequirement = section.HasDataResidencyRequirement;
            wizard.DataResidencyCountriesJson = JsonSerializer.Serialize(section.DataResidencyCountries, _jsonOptions);

            MarkSectionComplete(wizard, "A", section.IsComplete);
            wizard.ModifiedDate = DateTime.UtcNow;
            wizard.ModifiedBy = userId;

            await _unitOfWork.SaveChangesAsync();
            return await CreateSaveResponseAsync("A", section.IsComplete, wizard, "Organization Identity saved successfully.");
        }

        public async Task<WizardSectionSaveResponse> SaveSectionBAsync(Guid tenantId, SectionB_AssuranceObjective section, string userId)
        {
            var wizard = await EnsureWizardAsync(tenantId, userId);

            wizard.PrimaryDriver = section.PrimaryDriver;
            wizard.TargetTimeline = section.TargetDate;
            wizard.CurrentPainPointsJson = JsonSerializer.Serialize(section.CurrentPainPoints, _jsonOptions);
            wizard.DesiredMaturity = section.DesiredMaturity;
            wizard.ReportingAudienceJson = JsonSerializer.Serialize(section.ReportingAudience, _jsonOptions);

            MarkSectionComplete(wizard, "B", section.IsComplete);
            wizard.ModifiedDate = DateTime.UtcNow;
            wizard.ModifiedBy = userId;

            await _unitOfWork.SaveChangesAsync();
            return await CreateSaveResponseAsync("B", section.IsComplete, wizard, "Assurance Objective saved successfully.");
        }

        public async Task<WizardSectionSaveResponse> SaveSectionCAsync(Guid tenantId, SectionC_RegulatoryApplicability section, string userId)
        {
            var wizard = await EnsureWizardAsync(tenantId, userId);

            wizard.PrimaryRegulatorsJson = JsonSerializer.Serialize(section.PrimaryRegulators, _jsonOptions);
            wizard.SecondaryRegulatorsJson = JsonSerializer.Serialize(section.SecondaryRegulators, _jsonOptions);
            wizard.MandatoryFrameworksJson = JsonSerializer.Serialize(section.MandatoryFrameworks, _jsonOptions);
            wizard.OptionalFrameworksJson = JsonSerializer.Serialize(section.BenchmarkingFrameworks, _jsonOptions);
            wizard.InternalPoliciesJson = JsonSerializer.Serialize(section.InternalPolicies, _jsonOptions);
            wizard.CertificationsHeldJson = JsonSerializer.Serialize(section.CertificationsHeld, _jsonOptions);
            wizard.AuditScopeType = section.AuditScopeType;

            MarkSectionComplete(wizard, "C", section.IsComplete);
            wizard.ModifiedDate = DateTime.UtcNow;
            wizard.ModifiedBy = userId;

            await _unitOfWork.SaveChangesAsync();
            return await CreateSaveResponseAsync("C", section.IsComplete, wizard, "Regulatory Applicability saved successfully.");
        }

        public async Task<WizardSectionSaveResponse> SaveSectionDAsync(Guid tenantId, SectionD_ScopeDefinition section, string userId)
        {
            var wizard = await EnsureWizardAsync(tenantId, userId);

            wizard.InScopeLegalEntitiesJson = JsonSerializer.Serialize(section.InScopeLegalEntities, _jsonOptions);
            wizard.InScopeBusinessUnitsJson = JsonSerializer.Serialize(section.InScopeBusinessUnits, _jsonOptions);
            wizard.InScopeSystemsJson = JsonSerializer.Serialize(section.InScopeSystems, _jsonOptions);
            wizard.InScopeProcessesJson = JsonSerializer.Serialize(section.InScopeProcesses, _jsonOptions);
            wizard.InScopeEnvironments = string.Join(",", section.InScopeEnvironments);
            wizard.InScopeLocationsJson = JsonSerializer.Serialize(section.InScopeLocations, _jsonOptions);
            wizard.SystemCriticalityTiersJson = JsonSerializer.Serialize(section.CriticalityTiers, _jsonOptions);
            wizard.ImportantBusinessServicesJson = JsonSerializer.Serialize(section.ImportantBusinessServices, _jsonOptions);
            wizard.ExclusionsJson = JsonSerializer.Serialize(section.Exclusions, _jsonOptions);

            MarkSectionComplete(wizard, "D", section.IsComplete);
            wizard.ModifiedDate = DateTime.UtcNow;
            wizard.ModifiedBy = userId;

            await _unitOfWork.SaveChangesAsync();
            return await CreateSaveResponseAsync("D", section.IsComplete, wizard, "Scope Definition saved successfully.");
        }

        public async Task<WizardSectionSaveResponse> SaveSectionEAsync(Guid tenantId, SectionE_DataRiskProfile section, string userId)
        {
            var wizard = await EnsureWizardAsync(tenantId, userId);

            wizard.DataTypesProcessedJson = JsonSerializer.Serialize(section.DataTypesProcessed, _jsonOptions);
            wizard.HasPaymentCardData = section.HasPaymentCardData;
            wizard.PaymentCardDataLocationsJson = JsonSerializer.Serialize(
                section.PaymentCardDataSystems.Concat(section.PaymentCardDataProcesses).ToList(), _jsonOptions);
            wizard.HasCrossBorderDataTransfers = section.HasCrossBorderTransfers;
            wizard.CrossBorderTransferCountriesJson = JsonSerializer.Serialize(section.CrossBorderTransfers, _jsonOptions);
            wizard.CustomerVolumeTier = section.CustomerVolumeTier;
            wizard.TransactionVolumeTier = section.TransactionVolumeTier;
            wizard.HasInternetFacingSystems = section.HasInternetFacingSystems;
            wizard.InternetFacingSystemsJson = JsonSerializer.Serialize(section.InternetFacingSystems, _jsonOptions);
            wizard.HasThirdPartyDataProcessing = section.HasThirdPartyDataProcessing;
            wizard.ThirdPartyDataProcessorsJson = JsonSerializer.Serialize(section.ThirdPartyProcessors, _jsonOptions);

            MarkSectionComplete(wizard, "E", section.IsComplete);
            wizard.ModifiedDate = DateTime.UtcNow;
            wizard.ModifiedBy = userId;

            await _unitOfWork.SaveChangesAsync();
            return await CreateSaveResponseAsync("E", section.IsComplete, wizard, "Data & Risk Profile saved successfully.");
        }

        public async Task<WizardSectionSaveResponse> SaveSectionFAsync(Guid tenantId, SectionF_TechnologyLandscape section, string userId)
        {
            var wizard = await EnsureWizardAsync(tenantId, userId);

            wizard.IdentityProvider = section.IdentityProvider;
            wizard.SsoEnabled = section.SsoEnabled;
            wizard.ScimProvisioningAvailable = section.ScimEnabled;
            wizard.ItsmPlatform = section.ItsmPlatform;
            wizard.EvidenceRepository = section.EvidenceRepository;
            wizard.SiemPlatform = section.SiemPlatform;
            wizard.VulnerabilityManagementTool = section.VulnerabilityManagement;
            wizard.EdrPlatform = section.EdrPlatform;
            wizard.CloudProvidersJson = JsonSerializer.Serialize(section.CloudProviders, _jsonOptions);
            wizard.ErpSystem = section.ErpPlatform;
            wizard.CmdbSource = section.CmdbSource;
            wizard.CiCdTooling = string.Join(",", section.CiCdTools);
            wizard.BackupDrTooling = section.BackupDrTooling;

            MarkSectionComplete(wizard, "F", section.IsComplete);
            wizard.ModifiedDate = DateTime.UtcNow;
            wizard.ModifiedBy = userId;

            await _unitOfWork.SaveChangesAsync();
            return await CreateSaveResponseAsync("F", section.IsComplete, wizard, "Technology Landscape saved successfully.");
        }

        public async Task<WizardSectionSaveResponse> SaveSectionGAsync(Guid tenantId, SectionG_ControlOwnership section, string userId)
        {
            var wizard = await EnsureWizardAsync(tenantId, userId);

            wizard.ControlOwnershipApproach = section.OwnershipApproach;
            wizard.DefaultControlOwnerTeam = section.DefaultOwnerTeam;
            wizard.ExceptionApproverRole = section.ExceptionApproverRole;
            wizard.RegulatoryInterpretationApproverRole = section.RegulatoryApproverRole;
            wizard.ControlEffectivenessSignoffRole = section.EffectivenessSignoffRole;
            wizard.InternalAuditStakeholder = $"{section.InternalAuditContact} ({section.InternalAuditRole})";
            wizard.RiskCommitteeCadence = section.RiskCommitteeCadence;
            wizard.RiskCommitteeAttendeesJson = JsonSerializer.Serialize(section.RiskCommitteeRoles, _jsonOptions);

            MarkSectionComplete(wizard, "G", section.IsComplete);
            wizard.ModifiedDate = DateTime.UtcNow;
            wizard.ModifiedBy = userId;

            await _unitOfWork.SaveChangesAsync();
            return await CreateSaveResponseAsync("G", section.IsComplete, wizard, "Control Ownership saved successfully.");
        }

        public async Task<WizardSectionSaveResponse> SaveSectionHAsync(Guid tenantId, SectionH_TeamsRolesAccess section, string userId)
        {
            var wizard = await EnsureWizardAsync(tenantId, userId);

            wizard.OrgAdminsJson = JsonSerializer.Serialize(section.OrgAdmins, _jsonOptions);
            wizard.CreateTeamsNow = section.CreateTeamsNow;
            wizard.TeamListJson = JsonSerializer.Serialize(section.Teams, _jsonOptions);
            wizard.TeamMembersJson = JsonSerializer.Serialize(section.TeamMembers, _jsonOptions);
            wizard.SelectedRoleCatalogJson = JsonSerializer.Serialize(section.RoleCatalog, _jsonOptions);
            wizard.RaciMappingNeeded = section.RaciMappingNeeded;
            wizard.RaciMappingJson = JsonSerializer.Serialize(section.RaciMappings, _jsonOptions);
            wizard.ApprovalGatesNeeded = section.ApprovalGatesNeeded;
            wizard.ApprovalGatesJson = JsonSerializer.Serialize(section.ApprovalGates, _jsonOptions);
            wizard.DelegationRulesJson = JsonSerializer.Serialize(section.DelegationRules, _jsonOptions);
            wizard.NotificationPreference = string.Join(",", section.NotificationChannels);
            wizard.EscalationDaysOverdue = section.EscalationAfterDays;
            wizard.EscalationTarget = section.EscalationTarget;

            MarkSectionComplete(wizard, "H", section.IsComplete);
            wizard.ModifiedDate = DateTime.UtcNow;
            wizard.ModifiedBy = userId;

            await _unitOfWork.SaveChangesAsync();
            return await CreateSaveResponseAsync("H", section.IsComplete, wizard, "Teams, Roles & Access saved successfully.");
        }

        public async Task<WizardSectionSaveResponse> SaveSectionIAsync(Guid tenantId, SectionI_WorkflowCadence section, string userId)
        {
            var wizard = await EnsureWizardAsync(tenantId, userId);

            wizard.EvidenceFrequencyDefaultsJson = JsonSerializer.Serialize(section.EvidenceFrequencyByDomain, _jsonOptions);
            wizard.AccessReviewsFrequency = section.AccessReviewFrequency;
            wizard.VulnerabilityPatchReviewFrequency = section.VulnerabilityReviewFrequency;
            wizard.BackupReviewFrequency = section.BackupReviewFrequency;
            wizard.RestoreTestCadence = section.RestoreTestCadence;
            wizard.DrExerciseCadence = section.DrExerciseCadence;
            wizard.IncidentTabletopCadence = section.IncidentTabletopCadence;
            wizard.EvidenceSlaSubmitDays = section.EvidenceSubmitSlaDays;
            wizard.RemediationSlaJson = JsonSerializer.Serialize(section.RemediationSlaDays, _jsonOptions);
            wizard.ExceptionExpiryDays = section.ExceptionExpiryDays;
            wizard.AuditRequestHandling = section.AuditRequestHandling;

            MarkSectionComplete(wizard, "I", section.IsComplete);
            wizard.ModifiedDate = DateTime.UtcNow;
            wizard.ModifiedBy = userId;

            await _unitOfWork.SaveChangesAsync();
            return await CreateSaveResponseAsync("I", section.IsComplete, wizard, "Workflow & Cadence saved successfully.");
        }

        public async Task<WizardSectionSaveResponse> SaveSectionJAsync(Guid tenantId, SectionJ_EvidenceStandards section, string userId)
        {
            var wizard = await EnsureWizardAsync(tenantId, userId);

            wizard.EvidenceNamingConventionRequired = section.NamingConventionRequired;
            wizard.EvidenceNamingPattern = section.NamingConventionPattern;
            wizard.EvidenceStorageLocationJson = JsonSerializer.Serialize(section.StorageLocationByDomain, _jsonOptions);
            wizard.EvidenceRetentionYears = section.RetentionPeriodYears;
            wizard.EvidenceAccessRulesJson = JsonSerializer.Serialize(section.AccessRules, _jsonOptions);
            wizard.AcceptableEvidenceTypesJson = JsonSerializer.Serialize(section.AcceptableEvidenceTypes, _jsonOptions);
            wizard.SamplingGuidanceJson = JsonSerializer.Serialize(section.Sampling, _jsonOptions);
            wizard.ConfidentialEvidenceEncryption = section.ConfidentialHandling.RequireEncryption;
            wizard.ConfidentialEvidenceAccessJson = JsonSerializer.Serialize(section.ConfidentialHandling.RestrictedAccessRoles, _jsonOptions);

            MarkSectionComplete(wizard, "J", section.IsComplete);
            wizard.ModifiedDate = DateTime.UtcNow;
            wizard.ModifiedBy = userId;

            await _unitOfWork.SaveChangesAsync();
            return await CreateSaveResponseAsync("J", section.IsComplete, wizard, "Evidence Standards saved successfully.");
        }

        public async Task<WizardSectionSaveResponse> SaveSectionKAsync(Guid tenantId, SectionK_BaselineOverlays section, string userId)
        {
            var wizard = await EnsureWizardAsync(tenantId, userId);

            wizard.AdoptDefaultBaseline = section.AdoptDefaultBaseline;
            wizard.SelectedOverlaysJson = JsonSerializer.Serialize(section.Overlays, _jsonOptions);
            wizard.HasClientSpecificControls = section.HasCustomRequirements;
            wizard.ClientSpecificControlsJson = JsonSerializer.Serialize(section.CustomRequirements, _jsonOptions);

            MarkSectionComplete(wizard, "K", section.IsComplete);
            wizard.ModifiedDate = DateTime.UtcNow;
            wizard.ModifiedBy = userId;

            await _unitOfWork.SaveChangesAsync();
            return await CreateSaveResponseAsync("K", section.IsComplete, wizard, "Baseline & Overlays saved successfully.");
        }

        public async Task<WizardSectionSaveResponse> SaveSectionLAsync(Guid tenantId, SectionL_GoLiveMetrics section, string userId)
        {
            var wizard = await EnsureWizardAsync(tenantId, userId);

            wizard.SuccessMetricsTop3Json = JsonSerializer.Serialize(section.SuccessMetrics, _jsonOptions);
            wizard.BaselineAuditPrepHoursPerMonth = section.CurrentAuditPrepHoursPerMonth;
            wizard.BaselineRemediationClosureDays = section.CurrentRemediationClosureDays;
            wizard.BaselineOverdueControlsPerMonth = section.CurrentOverdueControlsPerMonth;
            wizard.TargetImprovementJson = JsonSerializer.Serialize(section.TargetImprovementPercent, _jsonOptions);
            wizard.PilotScopeJson = JsonSerializer.Serialize(section.PilotDomains, _jsonOptions);

            MarkSectionComplete(wizard, "L", section.IsComplete);
            wizard.ModifiedDate = DateTime.UtcNow;
            wizard.ModifiedBy = userId;

            await _unitOfWork.SaveChangesAsync();
            return await CreateSaveResponseAsync("L", section.IsComplete, wizard, "Go-Live & Metrics saved successfully.");
        }

        public async Task<WizardSectionSaveResponse> SaveMinimalOnboardingAsync(Guid tenantId, MinimalOnboardingDto minimal, string userId)
        {
            var wizard = await EnsureWizardAsync(tenantId, userId);

            // Section A (A1-A9)
            wizard.OrganizationLegalNameEn = minimal.LegalNameEn;
            wizard.OrganizationLegalNameAr = minimal.LegalNameAr;
            wizard.TradeName = minimal.TradeName;
            wizard.CountryOfIncorporation = minimal.CountryOfIncorporation;
            wizard.OperatingCountriesJson = JsonSerializer.Serialize(minimal.OperatingCountries, _jsonOptions);
            wizard.PrimaryHqLocation = minimal.PrimaryHqLocation;
            wizard.DefaultTimezone = minimal.Timezone;
            wizard.PrimaryLanguage = minimal.PrimaryLanguage;
            wizard.CorporateEmailDomainsJson = JsonSerializer.Serialize(minimal.CorporateEmailDomains, _jsonOptions);
            wizard.DomainVerificationMethod = minimal.DomainVerificationMethod;

            // Section D (D26-D33)
            wizard.InScopeLegalEntitiesJson = JsonSerializer.Serialize(minimal.InScopeLegalEntities, _jsonOptions);
            wizard.InScopeBusinessUnitsJson = JsonSerializer.Serialize(minimal.InScopeBusinessUnits, _jsonOptions);
            wizard.InScopeSystemsJson = JsonSerializer.Serialize(minimal.InScopeSystems, _jsonOptions);
            wizard.InScopeProcessesJson = JsonSerializer.Serialize(minimal.InScopeProcesses, _jsonOptions);
            wizard.InScopeEnvironments = string.Join(",", minimal.InScopeEnvironments);
            wizard.InScopeLocationsJson = JsonSerializer.Serialize(minimal.InScopeLocations, _jsonOptions);
            wizard.SystemCriticalityTiersJson = JsonSerializer.Serialize(minimal.CriticalityTiers, _jsonOptions);
            wizard.ImportantBusinessServicesJson = JsonSerializer.Serialize(minimal.ImportantBusinessServices, _jsonOptions);

            // Section E (E35-E38)
            wizard.DataTypesProcessedJson = JsonSerializer.Serialize(minimal.DataTypesProcessed, _jsonOptions);
            wizard.HasPaymentCardData = minimal.HasPaymentCardData;
            wizard.HasCrossBorderDataTransfers = minimal.HasCrossBorderTransfers;
            wizard.CustomerVolumeTier = minimal.CustomerVolumeTier;

            // Section F (F41-F50)
            wizard.IdentityProvider = minimal.IdentityProvider;
            wizard.SsoEnabled = minimal.SsoEnabled;
            wizard.ScimProvisioningAvailable = minimal.ScimEnabled;
            wizard.ItsmPlatform = minimal.ItsmPlatform;
            wizard.EvidenceRepository = minimal.EvidenceRepository;
            wizard.SiemPlatform = minimal.SiemPlatform;
            wizard.VulnerabilityManagementTool = minimal.VulnerabilityManagement;
            wizard.EdrPlatform = minimal.EdrPlatform;
            wizard.CloudProvidersJson = JsonSerializer.Serialize(minimal.CloudProviders, _jsonOptions);
            wizard.ErpSystem = minimal.ErpPlatform;

            // Section H (H61-H66)
            wizard.OrgAdminsJson = JsonSerializer.Serialize(minimal.OrgAdmins, _jsonOptions);
            wizard.CreateTeamsNow = minimal.CreateTeamsNow;
            wizard.TeamListJson = JsonSerializer.Serialize(minimal.Teams, _jsonOptions);
            wizard.TeamMembersJson = JsonSerializer.Serialize(minimal.TeamMembers, _jsonOptions);
            wizard.SelectedRoleCatalogJson = JsonSerializer.Serialize(minimal.RoleCatalog, _jsonOptions);
            wizard.RaciMappingNeeded = minimal.RaciMappingNeeded;

            // Section I (I71-I79)
            wizard.EvidenceFrequencyDefaultsJson = JsonSerializer.Serialize(minimal.EvidenceFrequencyByDomain, _jsonOptions);
            wizard.AccessReviewsFrequency = minimal.AccessReviewFrequency;
            wizard.VulnerabilityPatchReviewFrequency = minimal.VulnerabilityReviewFrequency;
            wizard.BackupReviewFrequency = minimal.BackupReviewFrequency;
            wizard.RestoreTestCadence = minimal.RestoreTestCadence;
            wizard.DrExerciseCadence = minimal.DrExerciseCadence;
            wizard.IncidentTabletopCadence = minimal.IncidentTabletopCadence;
            wizard.EvidenceSlaSubmitDays = minimal.EvidenceSubmitSlaDays;
            wizard.RemediationSlaJson = JsonSerializer.Serialize(minimal.RemediationSlaDays, _jsonOptions);
            wizard.ExceptionExpiryDays = minimal.ExceptionExpiryDays;

            // Mark minimal required sections complete
            var sectionsComplete = new[] { "A", "D", "E", "F", "H", "I" };
            foreach (var s in sectionsComplete)
            {
                MarkSectionComplete(wizard, s, true);
            }

            wizard.ModifiedDate = DateTime.UtcNow;
            wizard.ModifiedBy = userId;

            await _unitOfWork.SaveChangesAsync();

            var response = await CreateSaveResponseAsync("Minimal", true, wizard, "Minimal onboarding complete. Ready to derive scope.");
            response.Section = "Minimal";
            return response;
        }

        #endregion

        #region Validation and Completion

        public async Task<WizardValidationResult> ValidateWizardAsync(Guid tenantId, bool minimalOnly = false)
        {
            var wizard = await GetWizardAsync(tenantId);
            if (wizard == null)
            {
                return new WizardValidationResult
                {
                    IsValid = false,
                    CanComplete = false,
                    MissingRequiredFields = new List<string> { "Wizard not started" }
                };
            }

            var completedSections = GetCompletedSections(wizard);
            var result = new WizardValidationResult
            {
                SectionStatus = new Dictionary<string, bool>
                {
                    ["A"] = completedSections.Contains("A"),
                    ["B"] = completedSections.Contains("B"),
                    ["C"] = completedSections.Contains("C"),
                    ["D"] = completedSections.Contains("D"),
                    ["E"] = completedSections.Contains("E"),
                    ["F"] = completedSections.Contains("F"),
                    ["G"] = completedSections.Contains("G"),
                    ["H"] = completedSections.Contains("H"),
                    ["I"] = completedSections.Contains("I"),
                    ["J"] = completedSections.Contains("J"),
                    ["K"] = completedSections.Contains("K"),
                    ["L"] = completedSections.Contains("L")
                }
            };

            result.CompletedSections = result.SectionStatus.Values.Count(v => v);

            var requiredSections = new[] { "A", "D", "E", "F", "H", "I" };
            var missingRequired = requiredSections.Where(s => !result.SectionStatus[s]).ToList();

            result.IsValid = minimalOnly ? missingRequired.Count == 0 : result.CompletedSections == 12;
            result.CanComplete = missingRequired.Count == 0;
            result.MissingRequiredFields = missingRequired.Select(s => $"Section {s} incomplete").ToList();

            if (!minimalOnly)
            {
                var optionalSections = new[] { "B", "C", "G", "J", "K", "L" };
                result.Warnings = optionalSections.Where(s => !result.SectionStatus[s])
                    .Select(s => $"Optional Section {s} not completed").ToList();
            }

            return result;
        }

        public async Task<WizardCompletionResult> CompleteWizardAsync(Guid tenantId, string userId)
        {
            var validation = await ValidateWizardAsync(tenantId, minimalOnly: true);
            if (!validation.CanComplete)
            {
                return new WizardCompletionResult
                {
                    Success = false,
                    TenantId = tenantId,
                    Message = "Cannot complete wizard - required sections incomplete",
                    Errors = validation.MissingRequiredFields
                };
            }

            var wizard = await GetWizardAsync(tenantId);
            if (wizard == null)
            {
                return new WizardCompletionResult
                {
                    Success = false,
                    TenantId = tenantId,
                    Message = "Wizard data not found",
                    Errors = new List<string> { "Wizard not started" }
                };
            }

            wizard.WizardStatus = "Completed";
            wizard.CompletedAt = DateTime.UtcNow;
            wizard.CompletedByUserId = userId;
            wizard.ProgressPercent = 100;
            wizard.ModifiedDate = DateTime.UtcNow;
            wizard.ModifiedBy = userId;

            await _unitOfWork.SaveChangesAsync();

            // Trigger scope derivation
            try
            {
                await _rulesEngine.DeriveAndPersistScopeAsync(tenantId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Scope derivation failed for tenant {TenantId}, but wizard completed", tenantId);
            }

            var scope = await GetDerivedScopeAsync(tenantId);

            await _auditService.LogEventAsync(
                tenantId: tenantId,
                eventType: "OnboardingWizardCompleted",
                affectedEntityType: "OnboardingWizard",
                affectedEntityId: wizard.Id.ToString(),
                action: "Complete",
                actor: userId,
                payloadJson: JsonSerializer.Serialize(new { wizard.Id, CompletedSections = validation.CompletedSections })
            );

            return new WizardCompletionResult
            {
                Success = true,
                TenantId = tenantId,
                Message = "Onboarding wizard completed successfully",
                DerivedScope = scope,
                CompletedAt = wizard.CompletedAt ?? DateTime.UtcNow
            };
        }

        public async Task<OnboardingScopeDto> GetDerivedScopeAsync(Guid tenantId)
        {
            var baselines = await _unitOfWork.TenantBaselines
                .Query()
                .Where(b => b.TenantId == tenantId && !b.IsDeleted)
                .ToListAsync();

            var packages = await _unitOfWork.TenantPackages
                .Query()
                .Where(p => p.TenantId == tenantId && !p.IsDeleted)
                .ToListAsync();

            var templates = await _unitOfWork.TenantTemplates
                .Query()
                .Where(t => t.TenantId == tenantId && !t.IsDeleted)
                .ToListAsync();

            return new OnboardingScopeDto
            {
                TenantId = tenantId,
                ApplicableBaselines = baselines.Select(b => new BaselineDto
                {
                    BaselineCode = b.BaselineCode,
                    Name = b.BaselineCode,
                    ReasonJson = b.ReasonJson
                }).ToList(),
                ApplicablePackages = packages.Select(p => new PackageDto
                {
                    PackageCode = p.PackageCode,
                    Name = p.PackageCode,
                    ReasonJson = p.ReasonJson
                }).ToList(),
                ApplicableTemplates = templates.Select(t => new TemplateDto
                {
                    TemplateCode = t.TemplateCode,
                    Name = t.TemplateCode,
                    ReasonJson = t.ReasonJson
                }).ToList(),
                RetrievedAt = DateTime.UtcNow
            };
        }

        #endregion

        #region Private Helpers

        private async Task<OnboardingWizard?> GetWizardAsync(Guid tenantId)
        {
            return await _unitOfWork.OnboardingWizards
                .Query()
                .FirstOrDefaultAsync(w => w.TenantId == tenantId && !w.IsDeleted);
        }

        private async Task<OnboardingWizard> EnsureWizardAsync(Guid tenantId, string userId)
        {
            var wizard = await GetWizardAsync(tenantId);
            if (wizard == null)
            {
                wizard = await StartWizardAsync(tenantId, userId);
            }
            return wizard;
        }

        private List<string> GetCompletedSections(OnboardingWizard wizard)
        {
            try
            {
                return JsonSerializer.Deserialize<List<string>>(wizard.CompletedSectionsJson, _jsonOptions) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        private void MarkSectionComplete(OnboardingWizard wizard, string section, bool isComplete)
        {
            var completed = GetCompletedSections(wizard);

            if (isComplete && !completed.Contains(section))
            {
                completed.Add(section);
            }
            else if (!isComplete && completed.Contains(section))
            {
                completed.Remove(section);
            }

            wizard.CompletedSectionsJson = JsonSerializer.Serialize(completed, _jsonOptions);
            wizard.ProgressPercent = (completed.Count * 100) / 12;

            var sectionIndex = section[0] - 'A' + 1;
            if (isComplete && sectionIndex >= wizard.CurrentStep && sectionIndex < 12)
            {
                wizard.CurrentStep = sectionIndex + 1;
            }

            if (wizard.WizardStatus == "NotStarted")
            {
                wizard.WizardStatus = "InProgress";
                wizard.StartedAt = DateTime.UtcNow;
            }
        }

        private Dictionary<string, SectionProgress> BuildSectionProgress(List<string> completedSections)
        {
            return new Dictionary<string, SectionProgress>
            {
                ["A"] = new() { SectionCode = "A", SectionName = "Organization Identity", IsComplete = completedSections.Contains("A"), IsRequired = true, QuestionsTotal = 13 },
                ["B"] = new() { SectionCode = "B", SectionName = "Assurance Objective", IsComplete = completedSections.Contains("B"), IsRequired = false, QuestionsTotal = 5 },
                ["C"] = new() { SectionCode = "C", SectionName = "Regulatory Applicability", IsComplete = completedSections.Contains("C"), IsRequired = false, QuestionsTotal = 7 },
                ["D"] = new() { SectionCode = "D", SectionName = "Scope Definition", IsComplete = completedSections.Contains("D"), IsRequired = true, QuestionsTotal = 9 },
                ["E"] = new() { SectionCode = "E", SectionName = "Data & Risk Profile", IsComplete = completedSections.Contains("E"), IsRequired = true, QuestionsTotal = 6 },
                ["F"] = new() { SectionCode = "F", SectionName = "Technology Landscape", IsComplete = completedSections.Contains("F"), IsRequired = true, QuestionsTotal = 13 },
                ["G"] = new() { SectionCode = "G", SectionName = "Control Ownership", IsComplete = completedSections.Contains("G"), IsRequired = false, QuestionsTotal = 7 },
                ["H"] = new() { SectionCode = "H", SectionName = "Teams, Roles, Access", IsComplete = completedSections.Contains("H"), IsRequired = true, QuestionsTotal = 10 },
                ["I"] = new() { SectionCode = "I", SectionName = "Workflow & Cadence", IsComplete = completedSections.Contains("I"), IsRequired = true, QuestionsTotal = 10 },
                ["J"] = new() { SectionCode = "J", SectionName = "Evidence Standards", IsComplete = completedSections.Contains("J"), IsRequired = false, QuestionsTotal = 7 },
                ["K"] = new() { SectionCode = "K", SectionName = "Baseline & Overlays", IsComplete = completedSections.Contains("K"), IsRequired = false, QuestionsTotal = 3 },
                ["L"] = new() { SectionCode = "L", SectionName = "Go-Live & Metrics", IsComplete = completedSections.Contains("L"), IsRequired = false, QuestionsTotal = 6 }
            };
        }

        private async Task<WizardSectionSaveResponse> CreateSaveResponseAsync(string section, bool complete, OnboardingWizard wizard, string message)
        {
            var sectionOrder = new[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L" };
            var currentIndex = Array.IndexOf(sectionOrder, section);
            var nextSection = currentIndex < 11 ? sectionOrder[currentIndex + 1] : null;

            var response = new WizardSectionSaveResponse
            {
                Success = true,
                Section = section,
                SectionComplete = complete,
                OverallProgress = wizard.ProgressPercent,
                NextSection = nextSection,
                Message = message
            };

            // Add coverage validation if enabled
            var enableCoverageValidation = _configuration?.GetValue<bool>("Onboarding:ValidateOnSectionSave", true) ?? true;
            // #region agent log
            System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "E", location = "OnboardingWizardService.cs:809", message = "ValidateOnSectionSave check", data = new { enableCoverageValidation, section, tenantId = wizard.TenantId }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
            // #endregion
            if (enableCoverageValidation)
            {
                try
                {
                    // #region agent log
                    System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "F", location = "OnboardingWizardService.cs:814", message = "Calling ValidateSectionCoverageAsync", data = new { section, tenantId = wizard.TenantId }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
                    // #endregion
                    var coverageResult = await ValidateSectionCoverageAsync(wizard.TenantId, section);
                    // #region agent log
                    System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "G", location = "OnboardingWizardService.cs:816", message = "ValidateSectionCoverageAsync result", data = new { hasResult = coverageResult != null, isValid = coverageResult?.IsValid, completionPercent = coverageResult?.OverallCompletionPercentage }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
                    // #endregion
                    if (coverageResult != null)
                    {
                        response.CoverageValidation = coverageResult;
                        response.CoverageValid = coverageResult.IsValid;
                        response.CoverageCompletionPercent = coverageResult.OverallCompletionPercentage;

                        // Extract missing fields from node results
                        if (coverageResult.NodeResults != null && coverageResult.NodeResults.Count > 0)
                        {
                            var nodeId = MapSectionToNodeId(section);
                            if (coverageResult.NodeResults.TryGetValue(nodeId, out var nodeResult))
                            {
                                response.MissingRequiredFields = nodeResult.MissingRequiredFields ?? new List<string>();
                                
                                // Add conditional required fields
                                var conditionalFields = _coverageService.EvaluateConditionalRequired(
                                    new OnboardingFieldValueProvider(wizard),
                                    await _coverageService.LoadManifestAsync());
                                
                                var missingConditional = conditionalFields
                                    .Where(f => !new OnboardingFieldValueProvider(wizard).HasFieldValue(f))
                                    .ToList();
                                
                                response.MissingConditionalFields = missingConditional;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // #region agent log
                    System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "H", location = "OnboardingWizardService.cs:845", message = "Exception in ValidateSectionCoverageAsync", data = new { section, exceptionType = ex.GetType().Name, exceptionMessage = ex.Message, stackTrace = ex.StackTrace }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
                    // #endregion
                    _logger.LogWarning(ex, "Error validating coverage for section {Section} in tenant {TenantId}", section, wizard.TenantId);
                    // Don't fail the save operation if coverage validation fails
                }
            }

            return response;
        }

        private OnboardingWizardStateDto MapToDto(OnboardingWizard wizard)
        {
            var completedSections = GetCompletedSections(wizard);

            return new OnboardingWizardStateDto
            {
                TenantId = wizard.TenantId,
                WizardStatus = wizard.WizardStatus,
                CurrentStep = wizard.CurrentStep,
                ProgressPercent = wizard.ProgressPercent,
                StartedAt = wizard.StartedAt,
                CompletedAt = wizard.CompletedAt,
                CompletedBy = wizard.CompletedByUserId ?? string.Empty,

                OrganizationIdentity = new SectionA_OrganizationIdentity
                {
                    LegalNameEn = wizard.OrganizationLegalNameEn,
                    LegalNameAr = wizard.OrganizationLegalNameAr,
                    TradeName = wizard.TradeName,
                    CountryOfIncorporation = wizard.CountryOfIncorporation,
                    OperatingCountries = SafeDeserialize<List<string>>(wizard.OperatingCountriesJson),
                    PrimaryHqLocation = wizard.PrimaryHqLocation,
                    Timezone = wizard.DefaultTimezone,
                    PrimaryLanguage = wizard.PrimaryLanguage,
                    CorporateEmailDomains = SafeDeserialize<List<string>>(wizard.CorporateEmailDomainsJson),
                    DomainVerificationMethod = wizard.DomainVerificationMethod,
                    OrgType = wizard.OrganizationType,
                    Sectors = string.IsNullOrEmpty(wizard.IndustrySector) ? new List<string>() : new List<string> { wizard.IndustrySector },
                    BusinessLines = SafeDeserialize<List<string>>(wizard.BusinessLinesJson),
                    HasDataResidencyRequirement = wizard.HasDataResidencyRequirement,
                    DataResidencyCountries = SafeDeserialize<List<string>>(wizard.DataResidencyCountriesJson)
                },

                SectionCompleted = new Dictionary<string, bool>
                {
                    ["A"] = completedSections.Contains("A"),
                    ["B"] = completedSections.Contains("B"),
                    ["C"] = completedSections.Contains("C"),
                    ["D"] = completedSections.Contains("D"),
                    ["E"] = completedSections.Contains("E"),
                    ["F"] = completedSections.Contains("F"),
                    ["G"] = completedSections.Contains("G"),
                    ["H"] = completedSections.Contains("H"),
                    ["I"] = completedSections.Contains("I"),
                    ["J"] = completedSections.Contains("J"),
                    ["K"] = completedSections.Contains("K"),
                    ["L"] = completedSections.Contains("L")
                }
            };
        }

        private T SafeDeserialize<T>(string json) where T : new()
        {
            if (string.IsNullOrEmpty(json)) return new T();
            try
            {
                return JsonSerializer.Deserialize<T>(json, _jsonOptions) ?? new T();
            }
            catch
            {
                return new T();
            }
        }

        /// <summary>
        /// Validate coverage for a specific section/node
        /// </summary>
        public async Task<CoverageValidationResult?> ValidateSectionCoverageAsync(Guid tenantId, string sectionId)
        {
            try
            {
                var wizard = await GetWizardAsync(tenantId);
                if (wizard == null)
                {
                    _logger.LogWarning("Wizard not found for tenant {TenantId}", tenantId);
                    return null;
                }

                // Create field value provider from wizard
                var fieldProvider = new OnboardingFieldValueProvider(wizard);

                // Map section ID to node ID
                var nodeId = MapSectionToNodeId(sectionId);

                // Validate node coverage
                var nodeResult = await _coverageService.ValidateNodeCoverageAsync(nodeId, fieldProvider);
                
                // Get mission coverage if applicable
                var missionId = MapSectionToMissionId(sectionId);
                MissionCoverageResult? missionResult = null;
                if (!string.IsNullOrEmpty(missionId))
                {
                    missionResult = await _coverageService.ValidateMissionCoverageAsync(missionId, fieldProvider);
                }

                // Build validation result
                var result = new CoverageValidationResult
                {
                    IsValid = nodeResult.IsValid && (missionResult == null || missionResult.IsValid),
                    NodeResults = new Dictionary<string, NodeCoverageResult>
                    {
                        [nodeId] = nodeResult
                    },
                    MissionResults = missionResult != null 
                        ? new Dictionary<string, MissionCoverageResult> { [missionId] = missionResult }
                        : new Dictionary<string, MissionCoverageResult>()
                };

                // Calculate completion percentage
                var totalRequired = nodeResult.PresentRequiredFields.Count + nodeResult.MissingRequiredFields.Count;
                result.OverallCompletionPercentage = totalRequired > 0 
                    ? (int)((nodeResult.PresentRequiredFields.Count / (double)totalRequired) * 100)
                    : 100;

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating coverage for section {SectionId} in tenant {TenantId}", sectionId, tenantId);
                return null;
            }
        }

        /// <summary>
        /// Get coverage status for all sections
        /// </summary>
        public async Task<Dictionary<string, CoverageValidationResult>> GetAllSectionsCoverageAsync(Guid tenantId)
        {
            var results = new Dictionary<string, CoverageValidationResult>();
            
            try
            {
                // #region agent log
                System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "I", location = "OnboardingWizardService.cs:981", message = "GetAllSectionsCoverageAsync entry", data = new { tenantId, hasCoverageService = _coverageService != null }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
                // #endregion
                var wizard = await GetWizardAsync(tenantId);
                if (wizard == null)
                {
                    // #region agent log
                    System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "J", location = "OnboardingWizardService.cs:987", message = "Wizard not found", data = new { tenantId }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
                    // #endregion
                    return results;
                }

                var fieldProvider = new OnboardingFieldValueProvider(wizard);
                
                // Validate complete coverage
                // #region agent log
                System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "K", location = "OnboardingWizardService.cs:994", message = "Calling ValidateCompleteCoverageAsync", data = new { tenantId }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
                // #endregion
                var completeResult = await _coverageService.ValidateCompleteCoverageAsync(fieldProvider);
                // #region agent log
                System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "L", location = "OnboardingWizardService.cs:996", message = "ValidateCompleteCoverageAsync result", data = new { hasResult = completeResult != null, nodeResultsCount = completeResult?.NodeResults?.Count ?? 0 }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
                // #endregion
                
                // Group results by section
                var sectionMap = new Dictionary<string, string>
                {
                    ["A"] = "M3.A",
                    ["B"] = "M2.B",
                    ["C"] = "M1.C",
                    ["D"] = "M1.D",
                    ["E"] = "M1.E",
                    ["F"] = "M3.F",
                    ["G"] = "M2.G",
                    ["H"] = "M2.H",
                    ["I"] = "M2.I",
                    ["J"] = "M3.J",
                    ["K"] = "M3.K",
                    ["L"] = "M2.L"
                };

                foreach (var (section, nodeId) in sectionMap)
                {
                    if (completeResult.NodeResults.TryGetValue(nodeId, out var nodeResult))
                    {
                        var totalRequired = nodeResult.RequiredFields.Count;
                        results[section] = new CoverageValidationResult
                        {
                            IsValid = nodeResult.IsValid,
                            NodeResults = new Dictionary<string, NodeCoverageResult> { [nodeId] = nodeResult },
                            MissionResults = new Dictionary<string, MissionCoverageResult>(),
                            OverallCompletionPercentage = totalRequired > 0
                                ? (int)((nodeResult.PresentRequiredFields.Count / (double)totalRequired) * 100)
                                : 100
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                // #region agent log
                System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "M", location = "OnboardingWizardService.cs:1032", message = "Exception in GetAllSectionsCoverageAsync", data = new { tenantId, exceptionType = ex.GetType().Name, exceptionMessage = ex.Message, stackTrace = ex.StackTrace }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
                // #endregion
                _logger.LogError(ex, "Error getting all sections coverage for tenant {TenantId}", tenantId);
            }

            // #region agent log
            System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "N", location = "OnboardingWizardService.cs:1036", message = "GetAllSectionsCoverageAsync exit", data = new { resultsCount = results.Count, resultKeys = results.Keys.ToList() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
            // #endregion
            return results;
        }

        private string MapSectionToNodeId(string sectionId)
        {
            return sectionId.ToUpper() switch
            {
                "A" => "M3.A",
                "B" => "M2.B",
                "C" => "M1.C",
                "D" => "M1.D",
                "E" => "M1.E",
                "F" => "M3.F",
                "G" => "M2.G",
                "H" => "M2.H",
                "I" => "M2.I",
                "J" => "M3.J",
                "K" => "M3.K",
                "L" => "M2.L",
                _ => sectionId
            };
        }

        private string? MapSectionToMissionId(string sectionId)
        {
            return sectionId.ToUpper() switch
            {
                "A" or "F" or "J" or "K" => "MISSION_3_SYSTEMS_EVIDENCE",
                "B" or "G" or "H" or "I" or "L" => "MISSION_2_PEOPLE_WORKFLOW",
                "C" or "D" or "E" => "MISSION_1_SCOPE_RISK",
                _ => null
            };
        }

        #endregion
    }
}
