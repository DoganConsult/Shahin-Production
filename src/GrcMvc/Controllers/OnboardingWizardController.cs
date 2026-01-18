using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using GrcMvc.Constants;
using GrcMvc.Data;
using GrcMvc.Models.DTOs;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Implementations;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Controllers
{
    /// <summary>
    /// MVC Controller for the comprehensive 12-step onboarding wizard
    /// Sections A-L covering 96 questions for complete organization recognition
    /// </summary>
    [Authorize]
    [Route("[controller]")]
    public class OnboardingWizardController : Controller
    {
        private readonly GrcDbContext _context;
        private readonly ILogger<OnboardingWizardController> _logger;
        private readonly IOnboardingProvisioningService? _provisioningService;
        private readonly IRulesEngineService? _rulesEngine;
        private readonly IPlanService? _planService;
        private readonly INotificationService? _notificationService;
        private readonly ISerialNumberService? _serialNumberService;
        private readonly IGrcCachingService? _cachingService;
        private readonly IWorkspaceManagementService? _workspaceService;
        private readonly ITenantOnboardingProvisioner? _tenantProvisioner;
        private readonly UserManager<ApplicationUser>? _userManager;
        private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public OnboardingWizardController(
            GrcDbContext context,
            ILogger<OnboardingWizardController> logger,
            IOnboardingProvisioningService? provisioningService = null,
            IRulesEngineService? rulesEngine = null,
            IPlanService? planService = null,
            INotificationService? notificationService = null,
            ISerialNumberService? serialNumberService = null,
            IGrcCachingService? cachingService = null,
            IWorkspaceManagementService? workspaceService = null,
            ITenantOnboardingProvisioner? tenantProvisioner = null,
            UserManager<ApplicationUser>? userManager = null)
        {
            _context = context;
            _logger = logger;
            _provisioningService = provisioningService;
            _rulesEngine = rulesEngine;
            _planService = planService;
            _notificationService = notificationService;
            _serialNumberService = serialNumberService;
            _cachingService = cachingService;
            _workspaceService = workspaceService;
            _tenantProvisioner = tenantProvisioner;
            _userManager = userManager;
        }

        /// <summary>
        /// Wizard entry point - redirects to current step or starts new wizard
        /// </summary>
        [HttpGet]
        [HttpGet("Index")]
        public async Task<IActionResult> Index(Guid? tenantId)
        {
            // Check authentication if tenantId is provided
            if (tenantId.HasValue)
            {
                var isAuthenticated = await CheckTenantAdminAuthAsync(tenantId.Value);
                if (!isAuthenticated)
                {
                    TempData["Error"] = "You must be authenticated as a tenant admin to access onboarding.";
                    return RedirectToAction("TenantAdminLogin", "Account", new { tenantId = tenantId.Value, returnUrl = Request.Path });
                }
            }

            var wizard = await GetOrCreateWizardAsync(tenantId);
            return RedirectToAction(GetStepActionName(wizard.CurrentStep), new { tenantId = wizard.TenantId });
        }

        /// <summary>
        /// Get wizard summary/progress
        /// </summary>
        [HttpGet("Summary/{tenantId:guid}")]
        public async Task<IActionResult> Summary(Guid tenantId)
        {
            var wizard = await _context.OnboardingWizards
                .FirstOrDefaultAsync(w => w.TenantId == tenantId);

            if (wizard == null)
                return NotFound();

            var summary = BuildWizardSummary(wizard);
            return View(summary);
        }

        /// <summary>
        /// Get wizard status for loading/progress UX (supports overlay polling)
        /// </summary>
        [HttpGet("Status/{tenantId:guid}")]
        public async Task<IActionResult> Status(Guid tenantId)
        {
            var wizard = await _context.OnboardingWizards
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.TenantId == tenantId);

            if (wizard == null)
                return NotFound();

            return Json(new
            {
                tenantId = wizard.TenantId,
                status = wizard.WizardStatus,
                progressPercent = wizard.ProgressPercent,
                currentStep = wizard.CurrentStep,
                lastSavedAtUtc = wizard.LastStepSavedAt
            });
        }

        // ============================================================================
        // STEP A: Organization Identity and Tenancy
        // ============================================================================

        [HttpGet("StepA/{tenantId:guid}")]
        public async Task<IActionResult> StepA(Guid tenantId)
        {
            // Check authentication
            var isAuthenticated = await CheckTenantAdminAuthAsync(tenantId);
            if (!isAuthenticated)
            {
                TempData["Error"] = "You must be authenticated as a tenant admin to access onboarding.";
                return RedirectToAction("TenantAdminLogin", "Account", new { tenantId, returnUrl = Request.Path });
            }

            var wizard = await GetOrCreateWizardAsync(tenantId);

            // If wizard is already completed, redirect to dashboard with message
            if (wizard.WizardStatus == "Completed")
            {
                TempData["InfoMessage"] = "Onboarding is already complete. You can review your configuration in Organization Setup.";
                return RedirectToAction("Index", "Dashboard");
            }

            var dto = MapToStepADto(wizard);
            ViewData["WizardSummary"] = BuildWizardSummary(wizard);
            
            return View("StepA", dto);
        }

        [HttpPost("StepA/{tenantId:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StepA(Guid tenantId, StepAOrganizationIdentityDto dto)
        {
            var wizard = await GetOrCreateWizardAsync(tenantId);

            if (!ModelState.IsValid)
            {
                ViewData["WizardSummary"] = BuildWizardSummary(wizard);
                return View("StepA", dto);
            }

            // Update wizard with Step A data
            wizard.OrganizationLegalNameEn = dto.OrganizationLegalNameEn;
            wizard.OrganizationLegalNameAr = dto.OrganizationLegalNameAr;
            wizard.TradeName = dto.TradeName;
            wizard.CountryOfIncorporation = dto.CountryOfIncorporation;
            wizard.OperatingCountriesJson = JsonSerializer.Serialize(dto.OperatingCountries);
            wizard.PrimaryHqLocation = dto.PrimaryHqLocation;
            wizard.DefaultTimezone = dto.DefaultTimezone;
            wizard.PrimaryLanguage = dto.PrimaryLanguage;
            wizard.CorporateEmailDomainsJson = JsonSerializer.Serialize(dto.CorporateEmailDomains);
            wizard.DomainVerificationMethod = dto.DomainVerificationMethod;
            wizard.OrganizationType = dto.OrganizationType;
            wizard.IndustrySector = dto.IndustrySector;
            wizard.BusinessLinesJson = JsonSerializer.Serialize(dto.BusinessLines);
            wizard.HasDataResidencyRequirement = dto.HasDataResidencyRequirement;
            wizard.DataResidencyCountriesJson = JsonSerializer.Serialize(dto.DataResidencyCountries);

            MarkStepCompleted(wizard, "A");
            await _context.SaveChangesAsync();

            // Progressive sync to OrganizationProfile
            await SyncOrganizationProfileAsync(wizard);

            return RedirectToAction(nameof(StepB), new { tenantId });
        }

        // ============================================================================
        // STEP B: Assurance Objective
        // ============================================================================

        [HttpGet("StepB/{tenantId:guid}")]
        public async Task<IActionResult> StepB(Guid tenantId)
        {
            var wizard = await GetOrCreateWizardAsync(tenantId);
            var dto = MapToStepBDto(wizard);
            ViewData["WizardSummary"] = BuildWizardSummary(wizard);
            return View("StepB", dto);
        }

        [HttpPost("StepB/{tenantId:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StepB(Guid tenantId, StepBAssuranceObjectiveDto dto)
        {
            var wizard = await GetOrCreateWizardAsync(tenantId);

            if (!ModelState.IsValid)
            {
                ViewData["WizardSummary"] = BuildWizardSummary(wizard);
                return View("StepB", dto);
            }

            wizard.PrimaryDriver = dto.PrimaryDriver;
            // Convert DateTime to UTC for PostgreSQL compatibility
            wizard.TargetTimeline = dto.TargetTimeline.HasValue
                ? DateTime.SpecifyKind(dto.TargetTimeline.Value, DateTimeKind.Utc)
                : null;
            wizard.CurrentPainPointsJson = JsonSerializer.Serialize(dto.CurrentPainPoints);
            wizard.DesiredMaturity = dto.DesiredMaturity;
            wizard.ReportingAudienceJson = JsonSerializer.Serialize(dto.ReportingAudience);

            MarkStepCompleted(wizard, "B");
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(StepC), new { tenantId });
        }

        // ============================================================================
        // STEP C: Regulatory and Framework Applicability
        // ============================================================================

        [HttpGet("StepC/{tenantId:guid}")]
        public async Task<IActionResult> StepC(Guid tenantId)
        {
            var wizard = await GetOrCreateWizardAsync(tenantId);
            var dto = MapToStepCDto(wizard);
            ViewData["WizardSummary"] = BuildWizardSummary(wizard);
            ViewData["AvailableFrameworks"] = await GetAvailableFrameworksAsync(wizard.IndustrySector, wizard.CountryOfIncorporation);
            return View("StepC", dto);
        }

        [HttpPost("StepC/{tenantId:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StepC(Guid tenantId, StepCRegulatoryApplicabilityDto dto)
        {
            var wizard = await GetOrCreateWizardAsync(tenantId);

            wizard.PrimaryRegulatorsJson = JsonSerializer.Serialize(dto.PrimaryRegulators);
            wizard.SecondaryRegulatorsJson = JsonSerializer.Serialize(dto.SecondaryRegulators);
            wizard.MandatoryFrameworksJson = JsonSerializer.Serialize(dto.MandatoryFrameworks);
            wizard.OptionalFrameworksJson = JsonSerializer.Serialize(dto.OptionalFrameworks);
            wizard.InternalPoliciesJson = JsonSerializer.Serialize(dto.InternalPolicies);
            wizard.CertificationsHeldJson = JsonSerializer.Serialize(dto.CertificationsHeld);
            wizard.AuditScopeType = dto.AuditScopeType;

            MarkStepCompleted(wizard, "C");
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(StepD), new { tenantId });
        }

        // ============================================================================
        // STEP D: Scope Definition
        // ============================================================================

        [HttpGet("StepD/{tenantId:guid}")]
        public async Task<IActionResult> StepD(Guid tenantId)
        {
            var wizard = await GetOrCreateWizardAsync(tenantId);
            var dto = MapToStepDDto(wizard);
            ViewData["WizardSummary"] = BuildWizardSummary(wizard);
            return View("StepD", dto);
        }

        [HttpPost("StepD/{tenantId:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StepD(Guid tenantId, StepDScopeDefinitionDto dto)
        {
            var wizard = await GetOrCreateWizardAsync(tenantId);

            wizard.InScopeLegalEntitiesJson = JsonSerializer.Serialize(dto.InScopeLegalEntities);
            wizard.InScopeBusinessUnitsJson = JsonSerializer.Serialize(dto.InScopeBusinessUnits);
            wizard.InScopeSystemsJson = JsonSerializer.Serialize(dto.InScopeSystems);
            wizard.InScopeProcessesJson = JsonSerializer.Serialize(dto.InScopeProcesses);
            wizard.InScopeEnvironments = dto.InScopeEnvironments;
            wizard.InScopeLocationsJson = JsonSerializer.Serialize(dto.InScopeLocations);
            wizard.SystemCriticalityTiersJson = JsonSerializer.Serialize(dto.SystemCriticalityTiers);
            wizard.ImportantBusinessServicesJson = JsonSerializer.Serialize(dto.ImportantBusinessServices);
            wizard.ExclusionsJson = JsonSerializer.Serialize(dto.Exclusions);

            MarkStepCompleted(wizard, "D");
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(StepE), new { tenantId });
        }

        // ============================================================================
        // STEP E: Data and Risk Profile
        // ============================================================================

        [HttpGet("StepE/{tenantId:guid}")]
        public async Task<IActionResult> StepE(Guid tenantId)
        {
            var wizard = await GetOrCreateWizardAsync(tenantId);
            var dto = MapToStepEDto(wizard);
            ViewData["WizardSummary"] = BuildWizardSummary(wizard);
            return View("StepE", dto);
        }

        [HttpPost("StepE/{tenantId:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StepE(Guid tenantId, StepEDataRiskProfileDto dto)
        {
            var wizard = await GetOrCreateWizardAsync(tenantId);

            if (!ModelState.IsValid)
            {
                ViewData["WizardSummary"] = BuildWizardSummary(wizard);
                return View("StepE", dto);
            }

            wizard.DataTypesProcessedJson = JsonSerializer.Serialize(dto.DataTypesProcessed);
            wizard.HasPaymentCardData = dto.HasPaymentCardData;
            wizard.PaymentCardDataLocationsJson = JsonSerializer.Serialize(dto.PaymentCardDataLocations);
            wizard.HasCrossBorderDataTransfers = dto.HasCrossBorderDataTransfers;
            wizard.CrossBorderTransferCountriesJson = JsonSerializer.Serialize(dto.CrossBorderTransferCountries);
            wizard.CustomerVolumeTier = dto.CustomerVolumeTier;
            wizard.TransactionVolumeTier = dto.TransactionVolumeTier;
            wizard.HasInternetFacingSystems = dto.HasInternetFacingSystems;
            wizard.InternetFacingSystemsJson = JsonSerializer.Serialize(dto.InternetFacingSystems);
            wizard.HasThirdPartyDataProcessing = dto.HasThirdPartyDataProcessing;
            wizard.ThirdPartyDataProcessorsJson = JsonSerializer.Serialize(dto.ThirdPartyDataProcessors);

            MarkStepCompleted(wizard, "E");
            await _context.SaveChangesAsync();

            // Progressive sync to OrganizationProfile
            await SyncOrganizationProfileAsync(wizard);

            return RedirectToAction(nameof(StepF), new { tenantId });
        }

        // ============================================================================
        // STEP F: Technology Landscape
        // ============================================================================

        [HttpGet("StepF/{tenantId:guid}")]
        public async Task<IActionResult> StepF(Guid tenantId)
        {
            var wizard = await GetOrCreateWizardAsync(tenantId);
            var dto = MapToStepFDto(wizard);
            ViewData["WizardSummary"] = BuildWizardSummary(wizard);
            return View("StepF", dto);
        }

        [HttpPost("StepF/{tenantId:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StepF(Guid tenantId, StepFTechnologyLandscapeDto dto)
        {
            var wizard = await GetOrCreateWizardAsync(tenantId);

            // Use null-coalescing to ensure non-null values for required DB columns
            wizard.IdentityProvider = dto.IdentityProvider ?? string.Empty;
            wizard.SsoEnabled = dto.SsoEnabled;
            wizard.ScimProvisioningAvailable = dto.ScimProvisioningAvailable;
            wizard.ItsmPlatform = dto.ItsmPlatform ?? string.Empty;
            wizard.EvidenceRepository = dto.EvidenceRepository ?? string.Empty;
            wizard.SiemPlatform = dto.SiemPlatform ?? string.Empty;
            wizard.VulnerabilityManagementTool = dto.VulnerabilityManagementTool ?? string.Empty;
            wizard.EdrPlatform = dto.EdrPlatform ?? string.Empty;
            wizard.CloudProvidersJson = JsonSerializer.Serialize(dto.CloudProviders ?? new List<string>());
            wizard.ErpSystem = dto.ErpSystem ?? string.Empty;
            wizard.CmdbSource = dto.CmdbSource ?? string.Empty;
            wizard.CiCdTooling = dto.CiCdTooling ?? string.Empty;
            wizard.BackupDrTooling = dto.BackupDrTooling ?? string.Empty;

            MarkStepCompleted(wizard, "F");
            await _context.SaveChangesAsync();

            // Progressive sync to OrganizationProfile
            await SyncOrganizationProfileAsync(wizard);

            return RedirectToAction(nameof(StepG), new { tenantId });
        }

        // ============================================================================
        // STEP G: Control Ownership Model
        // ============================================================================

        [HttpGet("StepG/{tenantId:guid}")]
        public async Task<IActionResult> StepG(Guid tenantId)
        {
            var wizard = await GetOrCreateWizardAsync(tenantId);
            var dto = MapToStepGDto(wizard);
            ViewData["WizardSummary"] = BuildWizardSummary(wizard);
            return View("StepG", dto);
        }

        [HttpPost("StepG/{tenantId:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StepG(Guid tenantId, StepGControlOwnershipDto dto)
        {
            var wizard = await GetOrCreateWizardAsync(tenantId);

            if (!ModelState.IsValid)
            {
                ViewData["WizardSummary"] = BuildWizardSummary(wizard);
                return View("StepG", dto);
            }

            // Use null-coalescing to ensure non-null values for required DB columns
            wizard.ControlOwnershipApproach = dto.ControlOwnershipApproach ?? string.Empty;
            wizard.DefaultControlOwnerTeam = dto.DefaultControlOwnerTeam ?? string.Empty;
            wizard.ExceptionApproverRole = dto.ExceptionApproverRole ?? string.Empty;
            wizard.RegulatoryInterpretationApproverRole = dto.RegulatoryInterpretationApproverRole ?? string.Empty;
            wizard.ControlEffectivenessSignoffRole = dto.ControlEffectivenessSignoffRole ?? string.Empty;
            wizard.InternalAuditStakeholder = dto.InternalAuditStakeholder ?? string.Empty;
            wizard.RiskCommitteeCadence = dto.RiskCommitteeCadence ?? string.Empty;
            wizard.RiskCommitteeAttendeesJson = JsonSerializer.Serialize(dto.RiskCommitteeAttendees ?? new List<string>());

            MarkStepCompleted(wizard, "G");
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(StepH), new { tenantId });
        }

        // ============================================================================
        // STEP H: Teams, Roles, and Access
        // ============================================================================

        [HttpGet("StepH/{tenantId:guid}")]
        public async Task<IActionResult> StepH(Guid tenantId)
        {
            var wizard = await GetOrCreateWizardAsync(tenantId);
            var dto = MapToStepHDto(wizard);
            ViewData["WizardSummary"] = BuildWizardSummary(wizard);
            ViewData["AvailableRoles"] = GetAvailableRoles();
            return View("StepH", dto);
        }

        [HttpPost("StepH/{tenantId:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StepH(Guid tenantId, StepHTeamsRolesDto dto)
        {
            var wizard = await GetOrCreateWizardAsync(tenantId);

            if (!ModelState.IsValid)
            {
                ViewData["WizardSummary"] = BuildWizardSummary(wizard);
                ViewData["AvailableRoles"] = GetAvailableRoles();
                return View("StepH", dto);
            }

            // Use null-coalescing to ensure non-null values for required DB columns
            wizard.OrgAdminsJson = JsonSerializer.Serialize(dto.OrgAdmins ?? new List<AdminEntry>());
            wizard.CreateTeamsNow = dto.CreateTeamsNow;
            wizard.TeamListJson = JsonSerializer.Serialize(dto.TeamList ?? new List<TeamDefinition>());
            wizard.SelectedRoleCatalogJson = JsonSerializer.Serialize(dto.SelectedRoleCatalog ?? new List<string>());
            wizard.RaciMappingNeeded = dto.RaciMappingNeeded;
            wizard.RaciMappingJson = JsonSerializer.Serialize(dto.RaciMapping ?? new List<RaciEntry>());
            wizard.ApprovalGatesNeeded = dto.ApprovalGatesNeeded;
            wizard.ApprovalGatesJson = JsonSerializer.Serialize(dto.ApprovalGates ?? new List<ApprovalGateEntry>());
            wizard.DelegationRulesJson = JsonSerializer.Serialize(dto.DelegationRules ?? new List<DelegationRuleEntry>());
            wizard.NotificationPreference = dto.NotificationPreference ?? "email";
            wizard.EscalationDaysOverdue = dto.EscalationDaysOverdue;
            wizard.EscalationTarget = dto.EscalationTarget ?? "manager";

            MarkStepCompleted(wizard, "H");
            await _context.SaveChangesAsync();

            // Progressive sync to OrganizationProfile
            await SyncOrganizationProfileAsync(wizard);

            return RedirectToAction(nameof(StepI), new { tenantId });
        }

        // ============================================================================
        // STEP I: Workflow and Cadence
        // ============================================================================

        [HttpGet("StepI/{tenantId:guid}")]
        public async Task<IActionResult> StepI(Guid tenantId)
        {
            var wizard = await GetOrCreateWizardAsync(tenantId);
            var dto = MapToStepIDto(wizard);
            ViewData["WizardSummary"] = BuildWizardSummary(wizard);
            return View("StepI", dto);
        }

        [HttpPost("StepI/{tenantId:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StepI(Guid tenantId, StepIWorkflowCadenceDto dto)
        {
            var wizard = await GetOrCreateWizardAsync(tenantId);

            wizard.EvidenceFrequencyDefaultsJson = JsonSerializer.Serialize(dto.EvidenceFrequencyDefaults);
            wizard.AccessReviewsFrequency = dto.AccessReviewsFrequency;
            wizard.VulnerabilityPatchReviewFrequency = dto.VulnerabilityPatchReviewFrequency;
            wizard.BackupReviewFrequency = dto.BackupReviewFrequency;
            wizard.RestoreTestCadence = dto.RestoreTestCadence;
            wizard.DrExerciseCadence = dto.DrExerciseCadence;
            wizard.IncidentTabletopCadence = dto.IncidentTabletopCadence;
            wizard.EvidenceSlaSubmitDays = dto.EvidenceSlaSubmitDays;
            wizard.RemediationSlaJson = JsonSerializer.Serialize(dto.RemediationSla);
            wizard.ExceptionExpiryDays = dto.ExceptionExpiryDays;
            wizard.AuditRequestHandling = dto.AuditRequestHandling;

            MarkStepCompleted(wizard, "I");
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(StepJ), new { tenantId });
        }

        // ============================================================================
        // STEP J: Evidence Standards
        // ============================================================================

        [HttpGet("StepJ/{tenantId:guid}")]
        public async Task<IActionResult> StepJ(Guid tenantId)
        {
            var wizard = await GetOrCreateWizardAsync(tenantId);
            var dto = MapToStepJDto(wizard);
            ViewData["WizardSummary"] = BuildWizardSummary(wizard);
            return View("StepJ", dto);
        }

        [HttpPost("StepJ/{tenantId:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StepJ(Guid tenantId, StepJEvidenceStandardsDto dto)
        {
            var wizard = await GetOrCreateWizardAsync(tenantId);

            wizard.EvidenceNamingConventionRequired = dto.EvidenceNamingConventionRequired;
            wizard.EvidenceNamingPattern = dto.EvidenceNamingPattern;
            wizard.EvidenceStorageLocationJson = JsonSerializer.Serialize(dto.EvidenceStorageLocation);
            wizard.EvidenceRetentionYears = dto.EvidenceRetentionYears;
            wizard.EvidenceAccessRulesJson = JsonSerializer.Serialize(dto.EvidenceAccessRules);
            wizard.AcceptableEvidenceTypesJson = JsonSerializer.Serialize(dto.AcceptableEvidenceTypes);
            wizard.SamplingGuidanceJson = JsonSerializer.Serialize(dto.SamplingGuidance);
            wizard.ConfidentialEvidenceEncryption = dto.ConfidentialEvidenceEncryption;
            wizard.ConfidentialEvidenceAccessJson = JsonSerializer.Serialize(dto.ConfidentialEvidenceAccess);

            MarkStepCompleted(wizard, "J");
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(StepK), new { tenantId });
        }

        // ============================================================================
        // STEP K: Baseline + Overlays Selection
        // ============================================================================

        [HttpGet("StepK/{tenantId:guid}")]
        public async Task<IActionResult> StepK(Guid tenantId)
        {
            var wizard = await GetOrCreateWizardAsync(tenantId);
            var dto = MapToStepKDto(wizard);
            ViewData["WizardSummary"] = BuildWizardSummary(wizard);
            ViewData["AutoDerivedOverlays"] = await GetAutoDerivedOverlaysAsync(wizard);
            return View("StepK", dto);
        }

        [HttpPost("StepK/{tenantId:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StepK(Guid tenantId, StepKBaselineOverlaysDto dto)
        {
            var wizard = await GetOrCreateWizardAsync(tenantId);

            wizard.AdoptDefaultBaseline = dto.AdoptDefaultBaseline;
            wizard.SelectedOverlaysJson = JsonSerializer.Serialize(dto.SelectedOverlays);
            wizard.HasClientSpecificControls = dto.HasClientSpecificControls;
            wizard.ClientSpecificControlsJson = JsonSerializer.Serialize(dto.ClientSpecificControls);

            MarkStepCompleted(wizard, "K");
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(StepL), new { tenantId });
        }

        // ============================================================================
        // STEP L: Go-Live and Success Metrics
        // ============================================================================

        [HttpGet("StepL/{tenantId:guid}")]
        public async Task<IActionResult> StepL(Guid tenantId)
        {
            var wizard = await GetOrCreateWizardAsync(tenantId);
            var dto = MapToStepLDto(wizard);
            ViewData["WizardSummary"] = BuildWizardSummary(wizard);
            return View("StepL", dto);
        }

        [HttpPost("StepL/{tenantId:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StepL(Guid tenantId, StepLGoLiveMetricsDto dto)
        {
            var wizard = await GetOrCreateWizardAsync(tenantId);

            wizard.SuccessMetricsTop3Json = JsonSerializer.Serialize(dto.SuccessMetricsTop3);
            wizard.BaselineAuditPrepHoursPerMonth = dto.BaselineAuditPrepHoursPerMonth;
            wizard.BaselineRemediationClosureDays = dto.BaselineRemediationClosureDays;
            wizard.BaselineOverdueControlsPerMonth = dto.BaselineOverdueControlsPerMonth;
            wizard.TargetImprovementJson = JsonSerializer.Serialize(dto.TargetImprovement);
            wizard.PilotScopeJson = JsonSerializer.Serialize(dto.PilotScope);

            MarkStepCompleted(wizard, "L");
            wizard.WizardStatus = "Completed";
            wizard.CompletedAt = DateTime.UtcNow;
            wizard.CompletedByUserId = User?.FindFirst("sub")?.Value ?? "SYSTEM";
            wizard.ProgressPercent = 100;

            await _context.SaveChangesAsync();

            // Redirect to completion/review page
            return RedirectToAction(nameof(Complete), new { tenantId });
        }

        // ============================================================================
        // COMPLETION & REVIEW
        // ============================================================================

        [HttpGet("Complete/{tenantId:guid}")]
        public async Task<IActionResult> Complete(Guid tenantId)
        {
            var wizard = await _context.OnboardingWizards
                .FirstOrDefaultAsync(w => w.TenantId == tenantId);

            if (wizard == null)
                return NotFound();

            var summary = BuildWizardSummary(wizard);
            ViewData["WizardData"] = wizard;
            return View("Complete", summary);
        }

        /// <summary>
        /// Shows processing status when onboarding is already in progress (duplicate submission protection)
        /// </summary>
        [HttpGet("WorkflowCompleted/{tenantId:guid}")]
        public async Task<IActionResult> WorkflowCompleted(Guid tenantId)
        {
            var wizard = await _context.OnboardingWizards
                .FirstOrDefaultAsync(w => w.TenantId == tenantId);

            if (wizard == null)
                return NotFound();

            // If processing is complete, redirect to Complete view
            if (wizard.WizardStatus == "Completed")
            {
                return RedirectToAction(nameof(Complete), new { tenantId });
            }

            // If still processing, show the processing view
            var summary = BuildWizardSummary(wizard);
            ViewData["WizardStatus"] = wizard.WizardStatus;
            return View("WorkflowProcessing", summary);
        }

        [HttpPost("FinalizeOnboarding/{tenantId:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizeOnboarding(Guid tenantId)
        {
            var wizard = await _context.OnboardingWizards
                .FirstOrDefaultAsync(w => w.TenantId == tenantId);

            if (wizard == null)
                return NotFound();

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";

            // ===== PHASE 1: CRITICAL PATH (Must complete before redirect) =====
            // Only do essential operations that must complete synchronously

            // Mark wizard as processing to prevent duplicate submissions
            if (wizard.WizardStatus == "Processing")
            {
                TempData["InfoMessage"] = "Onboarding is already being processed. Please wait...";
                return RedirectToAction("WorkflowCompleted", new { tenantId });
            }

            wizard.WizardStatus = "Processing";
            wizard.LastStepSavedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            try
            {
                // Sync organization profile (essential - must complete)
                await SyncOrganizationProfileAsync(wizard, isFinalSync: true);

                // =============================================================================
                // AUTHORITATIVE: Create ONE default workspace per tenant (IDEMPOTENT)
                // Uses ITenantOnboardingProvisioner for idempotent workspace creation
                // =============================================================================
                Guid? defaultWorkspaceId = null;
                if (_tenantProvisioner != null)
                {
                    try
                    {
                        var workspaceName = wizard.OrganizationLegalNameEn ?? "Default Workspace";
                        defaultWorkspaceId = await _tenantProvisioner.EnsureDefaultWorkspaceAsync(
                            tenantId, workspaceName, userId);
                        _logger.LogInformation("Ensured default workspace {WorkspaceId} for tenant {TenantId}",
                            defaultWorkspaceId, tenantId);
                    }
                    catch (Exception wsEx)
                    {
                        _logger.LogWarning(wsEx, "Failed to ensure default workspace for tenant {TenantId}", tenantId);
                    }
                }
                else if (_workspaceService != null)
                {
                    // Fallback to legacy workspace service
                    try
                    {
                        var defaultWorkspace = await _workspaceService.CreateWorkspaceAsync(new CreateWorkspaceRequest
                        {
                            TenantId = tenantId,
                            WorkspaceCode = "DEFAULT",
                            Name = wizard.OrganizationLegalNameEn ?? "Default Workspace",
                            NameAr = wizard.OrganizationLegalNameAr,
                            WorkspaceType = "Market",
                            JurisdictionCode = wizard.CountryOfIncorporation ?? "SA",
                            DefaultLanguage = "ar",
                            IsDefault = true,
                            CreatedBy = userId
                        });
                        defaultWorkspaceId = defaultWorkspace.Id;
                        _logger.LogInformation("Created default workspace {WorkspaceId} for tenant {TenantId} (legacy)",
                            defaultWorkspace.Id, tenantId);
                    }
                    catch (Exception wsEx)
                    {
                        _logger.LogWarning(wsEx, "Failed to create default workspace for tenant {TenantId} - may already exist", tenantId);
                    }
                }
                else
                {
                    // CRITICAL: No workspace service available - log warning
                    _logger.LogWarning(
                        "No workspace provisioning service available for tenant {TenantId}. " +
                        "Both ITenantOnboardingProvisioner and IWorkspaceManagementService are null. " +
                        "Tenant will not have a default workspace until manually provisioned.",
                        tenantId);
                }

                // Create teams if requested (essential for RACI)
                if (wizard.CreateTeamsNow)
                {
                    await CreateTeamsFromWizardAsync(wizard);
                }

                // Create RACI assignments if defined
                if (wizard.RaciMappingNeeded)
                {
                    await CreateRaciAssignmentsAsync(wizard);
                }

                // Mark wizard as completed (user can proceed)
                wizard.WizardStatus = "Completed";
                wizard.CompletedAt = DateTime.UtcNow;
                wizard.CompletedByUserId = userId;
                wizard.ProgressPercent = 100;
                await _context.SaveChangesAsync();

                // ===== PHASE 2: BACKGROUND TASKS (Fire and forget) =====
                // Queue remaining tasks to run in background - don't block user
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await CompleteOnboardingBackgroundTasksAsync(tenantId, userId);
                    }
                    catch (Exception bgEx)
                    {
                        _logger.LogError(bgEx, "Background onboarding tasks failed for tenant {TenantId}", tenantId);
                    }
                });

                // Build success message WITH Tenant ID
                TempData["SuccessMessage"] = $"🎉 Onboarding complete for {wizard.OrganizationLegalNameEn}! Your Tenant ID is: {tenantId}. Please save this ID for future reference. Your compliance workspace is being configured in the background.";
                TempData["TenantIdCreated"] = tenantId.ToString();

                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Onboarding finalization failed for tenant {TenantId}", tenantId);
                wizard.WizardStatus = "Error";
                await _context.SaveChangesAsync();

                TempData["ErrorMessage"] = "An error occurred during onboarding. Please try again or contact support.";
                return RedirectToAction("Complete", new { tenantId });
            }
        }

        /// <summary>
        /// Background tasks for onboarding completion - runs after user is redirected
        /// </summary>
        private async Task CompleteOnboardingBackgroundTasksAsync(Guid tenantId, string userId)
        {
            // Use a new DbContext for background operations
            await using var context = await _context.Database.GetDbConnection().BeginTransactionAsync();

            try
            {
                var wizard = await _context.OnboardingWizards
                    .FirstOrDefaultAsync(w => w.TenantId == tenantId);

                if (wizard == null) return;

                // =============================================================================
                // AUTHORITATIVE: Use ITenantOnboardingProvisioner for comprehensive provisioning
                // Creates Assessment Template (100Q), GRC Plan, Initial Assessments, Workflows
                // =============================================================================
                if (_tenantProvisioner != null)
                {
                    try
                    {
                        var isComplete = await _tenantProvisioner.IsProvisioningCompleteAsync(tenantId);
                        if (!isComplete)
                        {
                            _logger.LogInformation("Background: Running tenant provisioning for {TenantId}", tenantId);
                            var result = await _tenantProvisioner.ProvisionTenantAsync(tenantId, userId);

                            if (result.Success)
                            {
                                _logger.LogInformation(
                                    "Background: Provisioning complete for {TenantId} - Workspace: {WorkspaceId}, Template: {TemplateId}, Plan: {PlanId}",
                                    tenantId, result.WorkspaceId, result.AssessmentTemplateId, result.GrcPlanId);
                            }
                            else
                            {
                                _logger.LogWarning("Background: Provisioning had errors for {TenantId}: {Errors}",
                                    tenantId, string.Join(", ", result.Errors));
                            }
                        }
                        else
                        {
                            _logger.LogInformation("Background: Provisioning already complete for {TenantId}", tenantId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Background: Tenant provisioning failed for {TenantId}", tenantId);
                    }
                }

                // ===== LEGACY AUTO-PROVISIONING (fallback) =====
                if (_tenantProvisioner == null && _provisioningService != null)
                {
                    try
                    {
                        var needsProvisioning = await _provisioningService.IsProvisioningNeededAsync(tenantId);
                        if (needsProvisioning)
                        {
                            _logger.LogInformation("Background: Legacy auto-provisioning for tenant {TenantId}", tenantId);
                            await _provisioningService.ProvisionAllAsync(tenantId, userId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Background: Legacy provisioning failed for tenant {TenantId}", tenantId);
                    }
                }

                // ===== SCOPE DERIVATION =====
                RuleExecutionLog? scopeExecutionLog = null;
                if (_rulesEngine != null)
                {
                    try
                    {
                        _logger.LogInformation("Background: Deriving scope for tenant {TenantId}", tenantId);
                        scopeExecutionLog = await _rulesEngine.DeriveAndPersistScopeAsync(tenantId, userId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Background: Scope derivation failed for tenant {TenantId}", tenantId);
                    }
                }

                // ===== GRC PLAN CREATION =====
                Plan? initialPlan = null;
                if (_planService != null && scopeExecutionLog != null)
                {
                    try
                    {
                        var planRequest = new CreatePlanDto
                        {
                            TenantId = tenantId,
                            PlanCode = $"PLAN-{DateTime.UtcNow:yyyyMMdd}-001",
                            Name = $"{wizard.OrganizationLegalNameEn} - Initial Compliance Plan",
                            Description = "Auto-generated plan from onboarding wizard",
                            PlanType = wizard.DesiredMaturity == "Foundation" ? "QuickScan" : "Full",
                            StartDate = DateTime.UtcNow,
                            TargetEndDate = wizard.TargetTimeline ?? DateTime.UtcNow.AddDays(90),
                            RulesetVersionId = scopeExecutionLog.RulesetId
                        };

                        initialPlan = await _planService.CreatePlanAsync(planRequest, userId);
                        _logger.LogInformation("Background: Plan created for tenant {TenantId}: {PlanId}", tenantId, initialPlan.Id);

                        // Create assessments (limit to avoid timeout)
                        var assessmentIds = await CreateInitialAssessmentsAsync(tenantId, initialPlan.Id, userId);

                        // Auto-assign tasks (batch, not loop)
                        foreach (var assessmentId in assessmentIds.Take(3)) // Limit to first 3
                        {
                            await AutoAssignTasksByRACIAsync(tenantId, assessmentId, userId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Background: Plan creation failed for tenant {TenantId}", tenantId);
                    }
                }

                // ===== AUDIT LOGGING =====
                await LogOnboardingCompletedEventAsync(tenantId, wizard, scopeExecutionLog, initialPlan, userId);

                // ===== WORKSPACE FEATURES =====
                await SetupWorkspaceFeaturesAsync(tenantId, wizard, userId);

                // ===== WORKFLOW ACTIVATION =====
                await ActivateDefaultWorkflowsAsync(tenantId, wizard, userId);

                // ===== USER INVITATIONS (can be slow due to email) =====
                await SendOrgAdminInvitationsAsync(tenantId, wizard, userId);

                _logger.LogInformation("Background: All onboarding tasks completed for tenant {TenantId}", tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Background: Critical error in onboarding tasks for tenant {TenantId}", tenantId);
            }
        }

        // ============================================================================
        // NAVIGATION HELPERS
        // ============================================================================

        [HttpGet("GoToStep/{tenantId:guid}/{step:int}")]
        public IActionResult GoToStep(Guid tenantId, int step)
        {
            var actionName = GetStepActionName(step);
            return RedirectToAction(actionName, new { tenantId });
        }

        [HttpPost("SaveAndExit/{tenantId:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAndExit(Guid tenantId)
        {
            var wizard = await _context.OnboardingWizards
                .FirstOrDefaultAsync(w => w.TenantId == tenantId);

            if (wizard != null)
            {
                wizard.LastStepSavedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            TempData["InfoMessage"] = "Your progress has been saved. You can continue the onboarding later.";
            return RedirectToAction("Index", "Dashboard");
        }

        /// <summary>
        /// Auto-save endpoint for background saving of wizard progress
        /// </summary>
        [HttpPost("AutoSave/{tenantId:guid}/{stepName}")]
        public async Task<IActionResult> AutoSave(Guid tenantId, string stepName, [FromBody] Dictionary<string, object> formData)
        {
            try
            {
                var wizard = await _context.OnboardingWizards
                    .FirstOrDefaultAsync(w => w.TenantId == tenantId);

                if (wizard == null)
                {
                    return Json(new { success = false, message = "Wizard not found" });
                }

                // Auto-save: just update the timestamp (form data saved in step-specific fields)
                wizard.LastStepSavedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Auto-saved successfully", timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Auto-save failed for tenant {TenantId}, step {StepName}", tenantId, stepName);
                return Json(new { success = false, message = "Auto-save failed" });
            }
        }

        // ============================================================================
        // PRIVATE HELPER METHODS
        // ============================================================================

        private async Task<OnboardingWizard> GetOrCreateWizardAsync(Guid? tenantId)
        {
            Guid effectiveTenantId;

            if (tenantId.HasValue && tenantId.Value != Guid.Empty)
            {
                effectiveTenantId = tenantId.Value;
            }
            else
            {
                // Get from TempData or create new
                var tempTenantId = TempData["TenantId"]?.ToString();
                if (!string.IsNullOrEmpty(tempTenantId) && Guid.TryParse(tempTenantId, out var parsed))
                {
                    effectiveTenantId = parsed;
                }
                else
                {
                    effectiveTenantId = Guid.NewGuid();
                }
            }

            TempData["TenantId"] = effectiveTenantId.ToString();
            TempData.Keep("TenantId");

            var wizard = await _context.OnboardingWizards
                .FirstOrDefaultAsync(w => w.TenantId == effectiveTenantId);

            if (wizard == null)
            {
                // Create new wizard - entity defaults from OnboardingWizard.cs handle most fields
                wizard = new OnboardingWizard
                {
                    Id = Guid.NewGuid(),
                    TenantId = effectiveTenantId,
                    WizardStatus = "InProgress",
                    CurrentStep = 1,
                    StartedAt = DateTime.UtcNow,
                    ProgressPercent = 0
                };
                _context.OnboardingWizards.Add(wizard);
                await _context.SaveChangesAsync();
            }

            return wizard;
        }

        /// <summary>
        /// Check if current user is authenticated as tenant admin for the given tenant
        /// </summary>
        private async Task<bool> CheckTenantAdminAuthAsync(Guid tenantId)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return false;
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || _userManager == null)
            {
                return false;
            }

            try
            {
                // Check TenantUser exists and is Admin
                var tenantUser = await _context.TenantUsers
                    .FirstOrDefaultAsync(tu => tu.TenantId == tenantId && tu.UserId == userId);

                if (tenantUser == null || tenantUser.Status != "Active")
                {
                    return false;
                }

                // Check if owner-generated and credentials expired
                if (tenantUser.IsOwnerGenerated && tenantUser.CredentialExpiresAt.HasValue)
                {
                    if (tenantUser.CredentialExpiresAt.Value < DateTime.UtcNow)
                    {
                        return false; // Credentials expired
                    }
                }

                // Check role
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return false;
                }

                // Use RoleConstants.IsTenantAdmin to handle variations (Admin, TENANT_ADMIN, etc.)
                var isAdmin = RoleConstants.IsTenantAdmin(tenantUser.RoleCode) ||
                              await _userManager.IsInRoleAsync(user, "Admin") ||
                              await _userManager.IsInRoleAsync(user, "TenantAdmin");

                return isAdmin;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking tenant admin auth for tenant {TenantId}", tenantId);
                return false;
            }
        }

        private void MarkStepCompleted(OnboardingWizard wizard, string sectionLetter)
        {
            var completedSections = string.IsNullOrEmpty(wizard.CompletedSectionsJson)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(wizard.CompletedSectionsJson) ?? new List<string>();

            if (!completedSections.Contains(sectionLetter))
            {
                completedSections.Add(sectionLetter);
            }

            wizard.CompletedSectionsJson = JsonSerializer.Serialize(completedSections);
            wizard.LastStepSavedAt = DateTime.UtcNow;

            // Update progress and current step
            int stepNumber = sectionLetter[0] - 'A' + 1;
            wizard.ProgressPercent = (int)((completedSections.Count / 12.0) * 100);

            if (stepNumber < 12)
            {
                wizard.CurrentStep = stepNumber + 1;
            }
        }

        private static string GetStepActionName(int step)
        {
            return step switch
            {
                1 => "StepA",
                2 => "StepB",
                3 => "StepC",
                4 => "StepD",
                5 => "StepE",
                6 => "StepF",
                7 => "StepG",
                8 => "StepH",
                9 => "StepI",
                10 => "StepJ",
                11 => "StepK",
                12 => "StepL",
                _ => "StepA"
            };
        }

        private WizardSummaryDto BuildWizardSummary(OnboardingWizard wizard)
        {
            var completedSections = string.IsNullOrEmpty(wizard.CompletedSectionsJson)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(wizard.CompletedSectionsJson) ?? new List<string>();

            var steps = new List<WizardStepSummary>();
            for (int i = 1; i <= 12; i++)
            {
                char sectionLetter = (char)('A' + i - 1);
                steps.Add(new WizardStepSummary
                {
                    StepNumber = i,
                    StepName = OnboardingWizardSteps.StepNames.GetValueOrDefault(i, $"Step {i}"),
                    StepNameAr = OnboardingWizardSteps.StepNamesAr.GetValueOrDefault(i, $"الخطوة {i}"),
                    Icon = OnboardingWizardSteps.StepIcons.GetValueOrDefault(i, "fa-circle"),
                    IsCompleted = completedSections.Contains(sectionLetter.ToString()),
                    IsCurrent = wizard.CurrentStep == i,
                    IsLocked = i > wizard.CurrentStep && !completedSections.Contains(sectionLetter.ToString())
                });
            }

            return new WizardSummaryDto
            {
                TenantId = wizard.TenantId,
                OrganizationName = wizard.OrganizationLegalNameEn,
                CurrentStep = wizard.CurrentStep,
                TotalSteps = 12,
                ProgressPercent = wizard.ProgressPercent,
                WizardStatus = wizard.WizardStatus,
                StartedAt = wizard.StartedAt,
                Steps = steps
            };
        }

        private async Task<List<OverlaySelection>> GetAutoDerivedOverlaysAsync(OnboardingWizard wizard)
        {
            var overlays = new List<OverlaySelection>();

            // Jurisdiction overlay based on country
            if (!string.IsNullOrEmpty(wizard.CountryOfIncorporation))
            {
                overlays.Add(new OverlaySelection
                {
                    OverlayType = "jurisdiction",
                    OverlayCode = wizard.CountryOfIncorporation,
                    OverlayName = GetCountryName(wizard.CountryOfIncorporation),
                    IsAutoSelected = true,
                    Reason = "Based on country of incorporation"
                });
            }

            // Sector overlay based on industry
            if (!string.IsNullOrEmpty(wizard.IndustrySector))
            {
                overlays.Add(new OverlaySelection
                {
                    OverlayType = "sector",
                    OverlayCode = wizard.IndustrySector,
                    OverlayName = wizard.IndustrySector,
                    IsAutoSelected = true,
                    Reason = "Based on industry sector selection"
                });
            }

            // Data overlays based on data types
            var dataTypes = JsonSerializer.Deserialize<List<string>>(wizard.DataTypesProcessedJson) ?? new List<string>();
            if (dataTypes.Contains("PCI") || wizard.HasPaymentCardData)
            {
                overlays.Add(new OverlaySelection
                {
                    OverlayType = "data",
                    OverlayCode = "PCI",
                    OverlayName = "Payment Card Industry",
                    IsAutoSelected = true,
                    Reason = "Payment card data is processed"
                });
            }

            if (dataTypes.Contains("PII"))
            {
                overlays.Add(new OverlaySelection
                {
                    OverlayType = "data",
                    OverlayCode = "PII",
                    OverlayName = "Personal Identifiable Information",
                    IsAutoSelected = true,
                    Reason = "PII data is processed"
                });
            }

            // Cloud overlay
            var cloudProviders = JsonSerializer.Deserialize<List<string>>(wizard.CloudProvidersJson) ?? new List<string>();
            if (cloudProviders.Any())
            {
                overlays.Add(new OverlaySelection
                {
                    OverlayType = "technology",
                    OverlayCode = "CLOUD",
                    OverlayName = "Cloud Infrastructure",
                    IsAutoSelected = true,
                    Reason = $"Cloud providers: {string.Join(", ", cloudProviders)}"
                });
            }

            return overlays;
        }

        private async Task<List<object>> GetAvailableFrameworksAsync(string sector, string country)
        {
            // In production, this would query the FrameworkCatalog table
            var frameworks = new List<object>
            {
                new { Code = "NCA-ECC", Name = "NCA Essential Cybersecurity Controls", Mandatory = true },
                new { Code = "PDPL", Name = "Personal Data Protection Law", Mandatory = true },
                new { Code = "ISO27001", Name = "ISO/IEC 27001:2022", Mandatory = false },
                new { Code = "SOC2", Name = "SOC 2 Type II", Mandatory = false }
            };

            // Add sector-specific frameworks
            if (sector == "Banking" || sector == "FinancialServices")
            {
                frameworks.Insert(0, new { Code = "SAMA-CSF", Name = "SAMA Cybersecurity Framework", Mandatory = true });
            }

            return frameworks;
        }

        private static List<object> GetAvailableRoles()
        {
            return new List<object>
            {
                new { Code = "CONTROL_OWNER", Name = "Control Owner", NameAr = "مالك الضابط" },
                new { Code = "EVIDENCE_CUSTODIAN", Name = "Evidence Custodian", NameAr = "أمين الأدلة" },
                new { Code = "APPROVER", Name = "Approver", NameAr = "معتمد" },
                new { Code = "ASSESSOR_TESTER", Name = "Assessor/Tester", NameAr = "مُقيّم/مختبر" },
                new { Code = "REMEDIATION_OWNER", Name = "Remediation Owner", NameAr = "مالك المعالجة" },
                new { Code = "VIEWER_AUDITOR", Name = "Viewer/Auditor", NameAr = "مشاهد/مدقق" }
            };
        }

        private static string GetCountryName(string countryCode)
        {
            return countryCode switch
            {
                "SA" => "Saudi Arabia",
                "AE" => "United Arab Emirates",
                "QA" => "Qatar",
                "KW" => "Kuwait",
                "BH" => "Bahrain",
                "OM" => "Oman",
                "EG" => "Egypt",
                "JO" => "Jordan",
                _ => countryCode
            };
        }

        private async Task CreateTeamsFromWizardAsync(OnboardingWizard wizard)
        {
            var teamList = JsonSerializer.Deserialize<List<TeamDefinition>>(wizard.TeamListJson, _jsonOptions) ?? new List<TeamDefinition>();

            foreach (var teamDef in teamList)
            {
                var team = new Team
                {
                    Id = Guid.NewGuid(),
                    TenantId = wizard.TenantId,
                    TeamCode = $"TEAM-{teamDef.TeamName.ToUpper().Replace(" ", "-")}",
                    Name = teamDef.TeamName,
                    NameAr = teamDef.TeamNameAr,
                    TeamType = teamDef.TeamType,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                _context.Teams.Add(team);

                // Add team members - match by email
                foreach (var member in teamDef.Members)
                {
                    // Find user by email through AspNetUsers join
                    var tenantUser = await _context.TenantUsers
                        .Include(tu => tu.User)
                        .FirstOrDefaultAsync(u => u.TenantId == wizard.TenantId &&
                                                   u.User != null &&
                                                   u.User.Email == member.Email);

                    if (tenantUser != null)
                    {
                        var teamMember = new TeamMember
                        {
                            Id = Guid.NewGuid(),
                            TenantId = wizard.TenantId,
                            TeamId = team.Id,
                            UserId = tenantUser.Id,
                            RoleCode = member.RoleCode,
                            IsPrimaryForRole = member.IsPrimary,
                            CanApprove = member.CanApprove,
                            CanDelegate = member.CanDelegate,
                            IsActive = true,
                            JoinedDate = DateTime.UtcNow,
                            CreatedDate = DateTime.UtcNow
                        };
                        _context.TeamMembers.Add(teamMember);
                    }
                    else
                    {
                        _logger.LogWarning("User not found for email {Email} in tenant {TenantId}",
                            member.Email, wizard.TenantId);
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task CreateRaciAssignmentsAsync(OnboardingWizard wizard)
        {
            // Guard against null or empty JSON
            if (string.IsNullOrWhiteSpace(wizard.RaciMappingJson))
            {
                _logger.LogInformation("No RACI mappings defined for tenant {TenantId}, skipping RACI assignment creation", wizard.TenantId);
                return;
            }

            List<RaciEntry> raciMappings;
            try
            {
                raciMappings = JsonSerializer.Deserialize<List<RaciEntry>>(wizard.RaciMappingJson, _jsonOptions) ?? new List<RaciEntry>();
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize RACI mappings for tenant {TenantId}", wizard.TenantId);
                return;
            }

            if (!raciMappings.Any())
            {
                _logger.LogInformation("RACI mappings list is empty for tenant {TenantId}", wizard.TenantId);
                return;
            }

            foreach (var raci in raciMappings)
            {
                var team = await _context.Teams
                    .FirstOrDefaultAsync(t => t.TenantId == wizard.TenantId && t.Name == raci.TeamName);

                if (team != null)
                {
                    var assignment = new RACIAssignment
                    {
                        Id = Guid.NewGuid(),
                        TenantId = wizard.TenantId,
                        ScopeType = raci.ScopeType,
                        ScopeId = raci.ScopeId,
                        TeamId = team.Id,
                        RACI = raci.RaciType,
                        RoleCode = raci.RoleCode,
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    };
                    _context.RACIAssignments.Add(assignment);
                }
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Progressive sync: Updates OrganizationProfile with wizard data.
        /// Called after each step to ensure data is saved incrementally.
        /// </summary>
        private async Task SyncOrganizationProfileAsync(OnboardingWizard wizard, bool isFinalSync = false)
        {
            var profile = await _context.OrganizationProfiles
                .FirstOrDefaultAsync(p => p.TenantId == wizard.TenantId);

            if (profile == null)
            {
                profile = new OrganizationProfile
                {
                    Id = Guid.NewGuid(),
                    TenantId = wizard.TenantId,
                    CreatedDate = DateTime.UtcNow,
                    OnboardingStartedAt = DateTime.UtcNow
                };
                _context.OrganizationProfiles.Add(profile);
            }

            // ===== Section A: Organization Identity =====
            profile.LegalEntityName = wizard.OrganizationLegalNameEn;
            profile.LegalEntityNameAr = wizard.OrganizationLegalNameAr;
            profile.Country = wizard.CountryOfIncorporation;
            profile.Sector = wizard.IndustrySector;
            profile.OrganizationType = wizard.OrganizationType;
            profile.OperatingCountries = wizard.OperatingCountriesJson;
            profile.HeadquartersLocation = wizard.PrimaryHqLocation;

            // ===== Section E: Data & Risk Profile =====
            profile.DataTypes = wizard.DataTypesProcessedJson;
            profile.ProcessesPersonalData = wizard.DataTypesProcessedJson.Contains("PII");
            profile.ProcessesSensitiveData = wizard.HasPaymentCardData || wizard.DataTypesProcessedJson.Contains("PHI");
            profile.HasThirdPartyDataProcessing = wizard.HasThirdPartyDataProcessing;

            // ===== Section F: Technology Landscape =====
            var cloudProviders = JsonSerializer.Deserialize<List<string>>(wizard.CloudProvidersJson) ?? new List<string>();
            profile.HostingModel = cloudProviders.Any() ? "Cloud" : "OnPremise";
            profile.CloudProviders = wizard.CloudProvidersJson;
            profile.ItSystemsJson = wizard.InScopeSystemsJson;

            // ===== Section C: Regulatory Applicability =====
            profile.PrimaryRegulator = wizard.PrimaryRegulatorsJson;
            profile.SecondaryRegulators = wizard.SecondaryRegulatorsJson;
            profile.RegulatoryCertifications = wizard.CertificationsHeldJson;

            // ===== Section H: Teams & Roles (Key Contacts) =====
            var orgAdmins = JsonSerializer.Deserialize<List<AdminEntry>>(wizard.OrgAdminsJson, _jsonOptions) ?? new List<AdminEntry>();
            if (orgAdmins.Any())
            {
                var primaryAdmin = orgAdmins.First();
                profile.ComplianceOfficerName = primaryAdmin.Name;
                profile.ComplianceOfficerEmail = primaryAdmin.Email;
            }

            // ===== Progress Tracking =====
            profile.OnboardingProgressPercent = wizard.ProgressPercent;
            profile.OnboardingStatus = wizard.WizardStatus;
            profile.ModifiedDate = DateTime.UtcNow;

            // Store all wizard answers for audit
            wizard.AllAnswersJson = JsonSerializer.Serialize(new
            {
                SectionA = new { wizard.OrganizationLegalNameEn, wizard.OrganizationType, wizard.IndustrySector, wizard.CountryOfIncorporation },
                SectionB = new { wizard.PrimaryDriver, wizard.DesiredMaturity, wizard.TargetTimeline },
                SectionC = new { wizard.PrimaryRegulatorsJson, wizard.MandatoryFrameworksJson },
                SectionD = new { wizard.InScopeLegalEntitiesJson, wizard.InScopeBusinessUnitsJson, wizard.InScopeSystemsJson },
                SectionE = new { wizard.DataTypesProcessedJson, wizard.HasPaymentCardData, wizard.HasCrossBorderDataTransfers },
                SectionF = new { wizard.IdentityProvider, wizard.ItsmPlatform, wizard.CloudProvidersJson },
                SectionG = new { wizard.ControlOwnershipApproach, wizard.ExceptionApproverRole },
                SectionH = new { wizard.OrgAdminsJson, wizard.TeamListJson, wizard.RaciMappingNeeded },
                SectionI = new { wizard.EvidenceFrequencyDefaultsJson, wizard.RemediationSlaJson },
                SectionJ = new { wizard.EvidenceRetentionYears, wizard.AcceptableEvidenceTypesJson },
                SectionK = new { wizard.AdoptDefaultBaseline, wizard.SelectedOverlaysJson },
                SectionL = new { wizard.SuccessMetricsTop3Json, wizard.PilotScopeJson }
            }, _jsonOptions);

            profile.OnboardingQuestionsJson = wizard.AllAnswersJson;

            if (isFinalSync)
            {
                profile.OnboardingStatus = "COMPLETED";
                profile.OnboardingCompletedAt = DateTime.UtcNow;
                profile.OnboardingCompletedBy = wizard.CompletedByUserId;
                profile.OnboardingProgressPercent = 100;
                
                // Also update tenant OnboardingStatus
                var tenant = await _context.Tenants.FindAsync(wizard.TenantId);
                if (tenant != null)
                {
                    tenant.OnboardingStatus = "COMPLETED";
                    tenant.OnboardingCompletedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
        }

        // ============================================================================
        // DTO MAPPING METHODS
        // ============================================================================

        private StepAOrganizationIdentityDto MapToStepADto(OnboardingWizard wizard)
        {
            return new StepAOrganizationIdentityDto
            {
                OrganizationLegalNameEn = wizard.OrganizationLegalNameEn,
                OrganizationLegalNameAr = wizard.OrganizationLegalNameAr,
                TradeName = wizard.TradeName,
                CountryOfIncorporation = wizard.CountryOfIncorporation,
                OperatingCountries = JsonSerializer.Deserialize<List<string>>(wizard.OperatingCountriesJson) ?? new(),
                PrimaryHqLocation = wizard.PrimaryHqLocation,
                DefaultTimezone = wizard.DefaultTimezone,
                PrimaryLanguage = wizard.PrimaryLanguage,
                CorporateEmailDomains = JsonSerializer.Deserialize<List<string>>(wizard.CorporateEmailDomainsJson) ?? new(),
                DomainVerificationMethod = wizard.DomainVerificationMethod,
                OrganizationType = wizard.OrganizationType,
                IndustrySector = wizard.IndustrySector,
                BusinessLines = JsonSerializer.Deserialize<List<string>>(wizard.BusinessLinesJson) ?? new(),
                HasDataResidencyRequirement = wizard.HasDataResidencyRequirement,
                DataResidencyCountries = JsonSerializer.Deserialize<List<string>>(wizard.DataResidencyCountriesJson) ?? new()
            };
        }

        private StepBAssuranceObjectiveDto MapToStepBDto(OnboardingWizard wizard)
        {
            return new StepBAssuranceObjectiveDto
            {
                PrimaryDriver = wizard.PrimaryDriver,
                TargetTimeline = wizard.TargetTimeline,
                CurrentPainPoints = JsonSerializer.Deserialize<List<RankedItem>>(wizard.CurrentPainPointsJson) ?? new(),
                DesiredMaturity = wizard.DesiredMaturity,
                ReportingAudience = JsonSerializer.Deserialize<List<string>>(wizard.ReportingAudienceJson) ?? new()
            };
        }

        private StepCRegulatoryApplicabilityDto MapToStepCDto(OnboardingWizard wizard)
        {
            return new StepCRegulatoryApplicabilityDto
            {
                PrimaryRegulators = JsonSerializer.Deserialize<List<RegulatorEntry>>(wizard.PrimaryRegulatorsJson) ?? new(),
                SecondaryRegulators = JsonSerializer.Deserialize<List<RegulatorEntry>>(wizard.SecondaryRegulatorsJson) ?? new(),
                MandatoryFrameworks = JsonSerializer.Deserialize<List<string>>(wizard.MandatoryFrameworksJson) ?? new(),
                OptionalFrameworks = JsonSerializer.Deserialize<List<string>>(wizard.OptionalFrameworksJson) ?? new(),
                InternalPolicies = JsonSerializer.Deserialize<List<string>>(wizard.InternalPoliciesJson) ?? new(),
                CertificationsHeld = JsonSerializer.Deserialize<List<CertificationEntry>>(wizard.CertificationsHeldJson) ?? new(),
                AuditScopeType = wizard.AuditScopeType
            };
        }

        private StepDScopeDefinitionDto MapToStepDDto(OnboardingWizard wizard)
        {
            return new StepDScopeDefinitionDto
            {
                InScopeLegalEntities = JsonSerializer.Deserialize<List<LegalEntityEntry>>(wizard.InScopeLegalEntitiesJson) ?? new(),
                InScopeBusinessUnits = JsonSerializer.Deserialize<List<string>>(wizard.InScopeBusinessUnitsJson) ?? new(),
                InScopeSystems = JsonSerializer.Deserialize<List<SystemEntry>>(wizard.InScopeSystemsJson) ?? new(),
                InScopeProcesses = JsonSerializer.Deserialize<List<string>>(wizard.InScopeProcessesJson) ?? new(),
                InScopeEnvironments = wizard.InScopeEnvironments,
                InScopeLocations = JsonSerializer.Deserialize<List<string>>(wizard.InScopeLocationsJson) ?? new(),
                SystemCriticalityTiers = JsonSerializer.Deserialize<Dictionary<string, string>>(wizard.SystemCriticalityTiersJson) ?? new(),
                ImportantBusinessServices = JsonSerializer.Deserialize<List<BusinessServiceEntry>>(wizard.ImportantBusinessServicesJson) ?? new(),
                Exclusions = JsonSerializer.Deserialize<List<ExclusionEntry>>(wizard.ExclusionsJson) ?? new()
            };
        }

        private StepEDataRiskProfileDto MapToStepEDto(OnboardingWizard wizard)
        {
            return new StepEDataRiskProfileDto
            {
                DataTypesProcessed = JsonSerializer.Deserialize<List<string>>(wizard.DataTypesProcessedJson) ?? new(),
                HasPaymentCardData = wizard.HasPaymentCardData,
                PaymentCardDataLocations = JsonSerializer.Deserialize<List<string>>(wizard.PaymentCardDataLocationsJson) ?? new(),
                HasCrossBorderDataTransfers = wizard.HasCrossBorderDataTransfers,
                CrossBorderTransferCountries = JsonSerializer.Deserialize<List<string>>(wizard.CrossBorderTransferCountriesJson) ?? new(),
                CustomerVolumeTier = wizard.CustomerVolumeTier,
                TransactionVolumeTier = wizard.TransactionVolumeTier,
                HasInternetFacingSystems = wizard.HasInternetFacingSystems,
                InternetFacingSystems = JsonSerializer.Deserialize<List<string>>(wizard.InternetFacingSystemsJson) ?? new(),
                HasThirdPartyDataProcessing = wizard.HasThirdPartyDataProcessing,
                ThirdPartyDataProcessors = JsonSerializer.Deserialize<List<VendorEntry>>(wizard.ThirdPartyDataProcessorsJson) ?? new()
            };
        }

        private StepFTechnologyLandscapeDto MapToStepFDto(OnboardingWizard wizard)
        {
            return new StepFTechnologyLandscapeDto
            {
                IdentityProvider = wizard.IdentityProvider,
                SsoEnabled = wizard.SsoEnabled,
                ScimProvisioningAvailable = wizard.ScimProvisioningAvailable,
                ItsmPlatform = wizard.ItsmPlatform,
                EvidenceRepository = wizard.EvidenceRepository,
                SiemPlatform = wizard.SiemPlatform,
                VulnerabilityManagementTool = wizard.VulnerabilityManagementTool,
                EdrPlatform = wizard.EdrPlatform,
                CloudProviders = JsonSerializer.Deserialize<List<string>>(wizard.CloudProvidersJson) ?? new(),
                ErpSystem = wizard.ErpSystem,
                CmdbSource = wizard.CmdbSource,
                CiCdTooling = wizard.CiCdTooling,
                BackupDrTooling = wizard.BackupDrTooling
            };
        }

        private StepGControlOwnershipDto MapToStepGDto(OnboardingWizard wizard)
        {
            return new StepGControlOwnershipDto
            {
                ControlOwnershipApproach = wizard.ControlOwnershipApproach,
                DefaultControlOwnerTeam = wizard.DefaultControlOwnerTeam,
                ExceptionApproverRole = wizard.ExceptionApproverRole,
                RegulatoryInterpretationApproverRole = wizard.RegulatoryInterpretationApproverRole,
                ControlEffectivenessSignoffRole = wizard.ControlEffectivenessSignoffRole,
                InternalAuditStakeholder = wizard.InternalAuditStakeholder,
                RiskCommitteeCadence = wizard.RiskCommitteeCadence,
                RiskCommitteeAttendees = JsonSerializer.Deserialize<List<string>>(wizard.RiskCommitteeAttendeesJson) ?? new()
            };
        }

        private StepHTeamsRolesDto MapToStepHDto(OnboardingWizard wizard)
        {
            return new StepHTeamsRolesDto
            {
                OrgAdmins = JsonSerializer.Deserialize<List<AdminEntry>>(wizard.OrgAdminsJson) ?? new(),
                CreateTeamsNow = wizard.CreateTeamsNow,
                TeamList = JsonSerializer.Deserialize<List<TeamDefinition>>(wizard.TeamListJson) ?? new(),
                SelectedRoleCatalog = JsonSerializer.Deserialize<List<string>>(wizard.SelectedRoleCatalogJson) ?? new(),
                RaciMappingNeeded = wizard.RaciMappingNeeded,
                RaciMapping = JsonSerializer.Deserialize<List<RaciEntry>>(wizard.RaciMappingJson) ?? new(),
                ApprovalGatesNeeded = wizard.ApprovalGatesNeeded,
                ApprovalGates = JsonSerializer.Deserialize<List<ApprovalGateEntry>>(wizard.ApprovalGatesJson) ?? new(),
                DelegationRules = JsonSerializer.Deserialize<List<DelegationRuleEntry>>(wizard.DelegationRulesJson) ?? new(),
                NotificationPreference = wizard.NotificationPreference,
                EscalationDaysOverdue = wizard.EscalationDaysOverdue,
                EscalationTarget = wizard.EscalationTarget
            };
        }

        private StepIWorkflowCadenceDto MapToStepIDto(OnboardingWizard wizard)
        {
            return new StepIWorkflowCadenceDto
            {
                EvidenceFrequencyDefaults = JsonSerializer.Deserialize<Dictionary<string, string>>(wizard.EvidenceFrequencyDefaultsJson) ?? new(),
                AccessReviewsFrequency = wizard.AccessReviewsFrequency,
                VulnerabilityPatchReviewFrequency = wizard.VulnerabilityPatchReviewFrequency,
                BackupReviewFrequency = wizard.BackupReviewFrequency,
                RestoreTestCadence = wizard.RestoreTestCadence,
                DrExerciseCadence = wizard.DrExerciseCadence,
                IncidentTabletopCadence = wizard.IncidentTabletopCadence,
                EvidenceSlaSubmitDays = wizard.EvidenceSlaSubmitDays,
                RemediationSla = JsonSerializer.Deserialize<Dictionary<string, int>>(wizard.RemediationSlaJson) ?? new(),
                ExceptionExpiryDays = wizard.ExceptionExpiryDays,
                AuditRequestHandling = wizard.AuditRequestHandling
            };
        }

        private StepJEvidenceStandardsDto MapToStepJDto(OnboardingWizard wizard)
        {
            return new StepJEvidenceStandardsDto
            {
                EvidenceNamingConventionRequired = wizard.EvidenceNamingConventionRequired,
                EvidenceNamingPattern = wizard.EvidenceNamingPattern,
                EvidenceStorageLocation = JsonSerializer.Deserialize<Dictionary<string, string>>(wizard.EvidenceStorageLocationJson) ?? new(),
                EvidenceRetentionYears = wizard.EvidenceRetentionYears,
                EvidenceAccessRules = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(wizard.EvidenceAccessRulesJson) ?? new(),
                AcceptableEvidenceTypes = JsonSerializer.Deserialize<List<string>>(wizard.AcceptableEvidenceTypesJson) ?? new(),
                SamplingGuidance = JsonSerializer.Deserialize<Dictionary<string, string>>(wizard.SamplingGuidanceJson) ?? new(),
                ConfidentialEvidenceEncryption = wizard.ConfidentialEvidenceEncryption,
                ConfidentialEvidenceAccess = JsonSerializer.Deserialize<List<string>>(wizard.ConfidentialEvidenceAccessJson) ?? new()
            };
        }

        private StepKBaselineOverlaysDto MapToStepKDto(OnboardingWizard wizard)
        {
            return new StepKBaselineOverlaysDto
            {
                AdoptDefaultBaseline = wizard.AdoptDefaultBaseline,
                SelectedOverlays = JsonSerializer.Deserialize<List<OverlaySelection>>(wizard.SelectedOverlaysJson) ?? new(),
                HasClientSpecificControls = wizard.HasClientSpecificControls,
                ClientSpecificControls = JsonSerializer.Deserialize<List<ClientControlEntry>>(wizard.ClientSpecificControlsJson) ?? new()
            };
        }

        private StepLGoLiveMetricsDto MapToStepLDto(OnboardingWizard wizard)
        {
            return new StepLGoLiveMetricsDto
            {
                SuccessMetricsTop3 = JsonSerializer.Deserialize<List<string>>(wizard.SuccessMetricsTop3Json) ?? new(),
                BaselineAuditPrepHoursPerMonth = wizard.BaselineAuditPrepHoursPerMonth,
                BaselineRemediationClosureDays = wizard.BaselineRemediationClosureDays,
                BaselineOverdueControlsPerMonth = wizard.BaselineOverdueControlsPerMonth,
                TargetImprovement = JsonSerializer.Deserialize<Dictionary<string, decimal>>(wizard.TargetImprovementJson) ?? new(),
                PilotScope = JsonSerializer.Deserialize<List<string>>(wizard.PilotScopeJson) ?? new()
            };
        }

        // ============================================================================
        // FULL GRC INTEGRATION METHODS
        // ============================================================================

        /// <summary>
        /// Setup workspace features: default reports, dashboard config, feature flags
        /// </summary>
        private async Task SetupWorkspaceFeaturesAsync(Guid tenantId, OnboardingWizard wizard, string userId)
        {
            try
            {
                // Create default report templates based on wizard answers
                var reportConfigs = new List<Report>();
                var now = DateTime.UtcNow;

                // Generate system serial numbers for reports
                var reportNumber1 = _serialNumberService != null
                    ? await _serialNumberService.GenerateReportNumberAsync(tenantId)
                    : $"RPT-{now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
                var reportNumber2 = _serialNumberService != null
                    ? await _serialNumberService.GenerateReportNumberAsync(tenantId)
                    : $"RPT-{now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
                var reportNumber3 = _serialNumberService != null
                    ? await _serialNumberService.GenerateReportNumberAsync(tenantId)
                    : $"RPT-{now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

                // Executive Dashboard Report
                reportConfigs.Add(new Report
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    ReportNumber = reportNumber1,
                    Title = "Executive Compliance Dashboard",
                    Type = "Executive",
                    Status = "Draft",
                    Description = "High-level compliance status for executive reporting",
                    Scope = "Organization-wide",
                    ReportPeriodStart = now,
                    ReportPeriodEnd = now.AddMonths(1),
                    GeneratedBy = userId,
                    CreatedDate = now,
                    CreatedBy = userId
                });

                // Control Status Report
                reportConfigs.Add(new Report
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    ReportNumber = reportNumber2,
                    Title = "Control Implementation Status",
                    Type = "Compliance",
                    Status = "Draft",
                    Description = "Detailed control implementation and gap analysis",
                    Scope = "All controls",
                    ReportPeriodStart = now,
                    ReportPeriodEnd = now.AddMonths(1),
                    GeneratedBy = userId,
                    CreatedDate = now,
                    CreatedBy = userId
                });

                // Evidence Collection Report
                reportConfigs.Add(new Report
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    ReportNumber = reportNumber3,
                    Title = "Evidence Collection Progress",
                    Type = "Audit",
                    Status = "Draft",
                    Description = "Track evidence submission and approval status",
                    Scope = "All evidence",
                    ReportPeriodStart = now,
                    ReportPeriodEnd = now.AddMonths(1),
                    GeneratedBy = userId,
                    CreatedDate = now,
                    CreatedBy = userId
                });

                _context.Reports.AddRange(reportConfigs);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created {Count} default reports for tenant {TenantId}", reportConfigs.Count, tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error setting up workspace features for tenant {TenantId}", tenantId);
            }
        }

        /// <summary>
        /// Send invitations to org admins defined in wizard
        /// </summary>
        private async Task SendOrgAdminInvitationsAsync(Guid tenantId, OnboardingWizard wizard, string userId)
        {
            try
            {
                var orgAdmins = JsonSerializer.Deserialize<List<AdminEntry>>(wizard.OrgAdminsJson, _jsonOptions) ?? new List<AdminEntry>();

                foreach (var admin in orgAdmins)
                {
                    // Check if user already exists by invitation token pattern (email-based)
                    var invitationToken = $"{admin.Email}_{tenantId}".GetHashCode().ToString("X");
                    var existingUser = await _context.TenantUsers
                        .FirstOrDefaultAsync(u => u.TenantId == tenantId && u.InvitationToken == invitationToken && !u.IsDeleted);

                    if (existingUser != null)
                    {
                        _logger.LogInformation("User invitation already exists for tenant {TenantId}", tenantId);
                        continue;
                    }

                    // Create tenant user record (UserId will be set when user activates)
                    var tenantUser = new TenantUser
                    {
                        Id = Guid.NewGuid(),
                        TenantId = tenantId,
                        UserId = string.Empty, // Will be populated on activation
                        RoleCode = "ORG_ADMIN",
                        TitleCode = "COMPLIANCE_OFFICER",
                        Status = "Pending",
                        InvitedAt = DateTime.UtcNow,
                        InvitedBy = userId,
                        InvitationToken = Guid.NewGuid().ToString("N"),
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = userId
                    };

                    _context.TenantUsers.Add(tenantUser);

                    // Send invitation notification WITH Tenant ID
                    if (_notificationService != null)
                    {
                        try
                        {
                            await _notificationService.SendNotificationAsync(
                                workflowInstanceId: Guid.Empty,
                                recipientUserId: tenantUser.Id.ToString(),
                                notificationType: "UserInvitation",
                                subject: $"Welcome to {wizard.OrganizationLegalNameEn} - Your GRC Platform Access",
                                body: $"Hello {admin.Name},\n\n" +
                                      $"You have been invited as an Organization Admin for {wizard.OrganizationLegalNameEn}.\n\n" +
                                      $"🔑 IMPORTANT - Your Tenant ID: {tenantId}\n\n" +
                                      $"Please save this Tenant ID. You will need it to:\n" +
                                      $"• Access your organization's workspace\n" +
                                      $"• Contact support\n" +
                                      $"• Configure integrations\n\n" +
                                      $"Click the link below to activate your account.\n\n" +
                                      $"Best regards,\nGRC Platform Team",
                                priority: "High",
                                tenantId: tenantId
                            );
                        }
                        catch (Exception notifyEx)
                        {
                            _logger.LogWarning(notifyEx, "Failed to send invitation notification");
                        }
                    }

                    _logger.LogInformation("Created invitation for ORG_ADMIN for tenant {TenantId}", tenantId);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error sending org admin invitations for tenant {TenantId}", tenantId);
            }
        }

        /// <summary>
        /// Activate default workflow templates for the tenant based on profile
        /// </summary>
        private async Task ActivateDefaultWorkflowsAsync(Guid tenantId, OnboardingWizard wizard, string userId)
        {
            try
            {
                var workflowsToActivate = new List<string>();

                // Always activate evidence approval workflow
                workflowsToActivate.Add("WF-EVIDENCE-APPROVAL");

                // Activate based on sector/regulator
                var primaryRegulators = JsonSerializer.Deserialize<List<RegulatorEntry>>(wizard.PrimaryRegulatorsJson, _jsonOptions) ?? new List<RegulatorEntry>();
                var regulatorCodes = primaryRegulators.Select(r => r.RegulatorCode.ToUpper()).ToList();

                if (regulatorCodes.Contains("NCA") || wizard.IndustrySector.Contains("Government"))
                {
                    workflowsToActivate.Add("WF-NCA-ECC-ASSESSMENT");
                }

                if (regulatorCodes.Contains("SAMA") || wizard.IndustrySector.Contains("Banking") || wizard.IndustrySector.Contains("Insurance"))
                {
                    workflowsToActivate.Add("WF-SAMA-CSF-ASSESSMENT");
                }

                if (wizard.DataTypesProcessedJson.Contains("PII") || wizard.HasCrossBorderDataTransfers)
                {
                    workflowsToActivate.Add("WF-PDPL-PIA");
                }

                // Always activate audit finding remediation
                workflowsToActivate.Add("WF-AUDIT-REMEDIATION");

                // Create tenant workflow configurations
                foreach (var workflowCode in workflowsToActivate.Distinct())
                {
                    var existingConfig = await _context.TenantWorkflowConfigs
                        .FirstOrDefaultAsync(w => w.TenantId == tenantId && w.WorkflowCode == workflowCode);

                    if (existingConfig == null)
                    {
                        _context.TenantWorkflowConfigs.Add(new TenantWorkflowConfig
                        {
                            Id = Guid.NewGuid(),
                            TenantId = tenantId,
                            WorkflowCode = workflowCode,
                            IsEnabled = true,
                            ActivatedAt = DateTime.UtcNow,
                            ActivatedBy = userId,
                            SlaMultiplier = 1.0m,
                            CreatedDate = DateTime.UtcNow,
                            CreatedBy = userId
                        });
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Activated {Count} workflows for tenant {TenantId}", workflowsToActivate.Count, tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error activating workflows for tenant {TenantId}", tenantId);
            }
        }

        /// <summary>
        /// Build completion summary message
        /// </summary>
        private string BuildCompletionSummary(OnboardingWizard wizard, Plan? plan)
        {
            var summary = new System.Text.StringBuilder();
            summary.AppendLine($"🎉 Onboarding complete for {wizard.OrganizationLegalNameEn}!");
            summary.AppendLine();
            summary.AppendLine("✅ Organization profile configured");
            summary.AppendLine("✅ Teams and RACI assignments created");
            summary.AppendLine("✅ Compliance scope derived");

            if (plan != null)
            {
                summary.AppendLine($"✅ Initial plan created: {plan.Name}");
            }

            summary.AppendLine("✅ Workspace reports configured");
            summary.AppendLine("✅ Workflow templates activated");
            summary.AppendLine();
            summary.AppendLine("Your GRC workspace is ready. Start by reviewing your compliance dashboard.");

            return summary.ToString();
        }

        /// <summary>
        /// Create initial assessments from derived templates
        /// Returns list of created assessment IDs for further processing
        /// </summary>
        private async Task<List<Guid>> CreateInitialAssessmentsAsync(Guid tenantId, Guid planId, string userId)
        {
            var createdAssessmentIds = new List<Guid>();
            try
            {
                // Get derived templates for this tenant
                var templates = await _context.TenantTemplates
                    .Where(t => t.TenantId == tenantId && !t.IsDeleted)
                    .ToListAsync();

                if (!templates.Any())
                {
                    _logger.LogInformation("No templates found for tenant {TenantId}, skipping assessment creation", tenantId);
                    return createdAssessmentIds;
                }

                // SMART: Get org profile for intelligent duration/priority calculation
                var orgProfile = await _context.OrganizationProfiles
                    .FirstOrDefaultAsync(p => p.TenantId == tenantId);

                // SMART: Calculate assessment duration based on org complexity
                var baseDays = CalculateSmartDuration(orgProfile);

                var assessmentsCreated = 0;
                foreach (var template in templates.Take(5)) // Allow up to 5 initial assessments
                {
                    // Generate system serial number
                    var assessmentNumber = _serialNumberService != null
                        ? await _serialNumberService.GenerateAssessmentNumberAsync(tenantId)
                        : $"ASM-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

                    // SMART: Prioritize assessments based on regulatory pressure (with audit trail)
                    var priority = await CalculateSmartPriorityAsync(tenantId, template.TemplateCode, orgProfile, userId);

                    var assessment = new Assessment
                    {
                        Id = Guid.NewGuid(),
                        TenantId = tenantId,
                        AssessmentNumber = assessmentNumber,
                        AssessmentCode = assessmentNumber,
                        Name = $"Initial Assessment - {template.TemplateName}",
                        Type = "Compliance",
                        Status = "Draft",
                        FrameworkCode = template.TemplateCode,
                        TemplateCode = template.TemplateCode,
                        PlanId = planId,
                        StartDate = DateTime.UtcNow,
                        DueDate = DateTime.UtcNow.AddDays(baseDays + (assessmentsCreated * 7)), // Stagger due dates
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = userId
                    };

                    _context.Assessments.Add(assessment);
                    createdAssessmentIds.Add(assessment.Id);
                    assessmentsCreated++;

                    // Generate assessment requirements from framework controls with smart priority
                    await GenerateAssessmentRequirementsAsync(assessment, template.TemplateCode, userId);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Created {Count} initial assessments for tenant {TenantId}", assessmentsCreated, tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error creating initial assessments for tenant {TenantId}", tenantId);
            }
            return createdAssessmentIds;
        }

        /// <summary>
        /// SMART: Calculate assessment duration based on organization complexity
        /// </summary>
        private int CalculateSmartDuration(OrganizationProfile? profile)
        {
            if (profile == null) return 30; // Default

            var baseDays = 21; // 3 weeks minimum

            // Add days based on complexity factors
            if (!string.IsNullOrEmpty(profile.OperatingCountries))
            {
                var countries = profile.OperatingCountries.Split(',').Length;
                baseDays += countries * 3; // +3 days per country
            }

            // Sector complexity
            var complexSectors = new[] { "Banking", "Insurance", "Healthcare", "Government", "CriticalInfrastructure" };
            if (complexSectors.Any(s => profile.Sector?.Contains(s) == true))
            {
                baseDays += 14; // +2 weeks for complex sectors
            }

            // Data sensitivity
            if (profile.ProcessesSensitiveData == true || profile.ProcessesPersonalData == true)
            {
                baseDays += 7; // +1 week for sensitive data
            }

            return Math.Min(baseDays, 90); // Cap at 90 days
        }

        /// <summary>
        /// SMART: Calculate assessment priority based on regulatory pressure
        /// With policy decision audit trail
        /// </summary>
        private async Task<string> CalculateSmartPriorityAsync(Guid tenantId, string templateCode, OrganizationProfile? profile, string userId)
        {
            // Build context for policy decision
            var context = JsonSerializer.Serialize(new
            {
                TemplateCode = templateCode,
                Sector = profile?.Sector,
                PrimaryRegulator = profile?.PrimaryRegulator,
                ProcessesSensitiveData = profile?.ProcessesSensitiveData,
                ProcessesPersonalData = profile?.ProcessesPersonalData
            });

            // Use caching service with audit if available
            if (_cachingService != null)
            {
                var decision = await _cachingService.GetOrCreatePolicyDecisionAsync(
                    tenantId,
                    "PriorityCalculation",
                    context,
                    async () =>
                    {
                        var (priority, reason, rulesMatched) = EvaluatePriorityRules(templateCode, profile);
                        return await Task.FromResult(new PolicyDecisionResult
                        {
                            Decision = priority,
                            Reason = reason,
                            PolicyVersion = "1.0",
                            RulesEvaluated = 4,
                            RulesMatched = rulesMatched,
                            ConfidenceScore = rulesMatched > 0 ? 90 : 70
                        });
                    });

                _logger.LogInformation("Policy decision: {PolicyType} = {Decision} (Reason: {Reason})",
                    "PriorityCalculation", decision.Decision, decision.Reason);

                return decision.Decision;
            }

            // Fallback without caching/audit
            var (fallbackPriority, _, _) = EvaluatePriorityRules(templateCode, profile);
            return fallbackPriority;
        }

        /// <summary>
        /// Evaluate priority rules and return decision with reason
        /// </summary>
        private (string Priority, string Reason, int RulesMatched) EvaluatePriorityRules(string templateCode, OrganizationProfile? profile)
        {
            var rulesMatched = 0;

            // Rule 1: High priority for mandatory frameworks
            var highPriorityFrameworks = new[] { "NCA-ECC", "SAMA-CSF", "PDPL" };
            if (highPriorityFrameworks.Any(f => templateCode.Contains(f)))
            {
                rulesMatched++;
                return ("High", $"Framework {templateCode} is a mandatory KSA regulation", rulesMatched);
            }

            // Rule 2: High priority if regulator deadline approaching
            if (profile?.PrimaryRegulator?.Contains("NCA") == true ||
                profile?.PrimaryRegulator?.Contains("SAMA") == true)
            {
                rulesMatched++;
                return ("High", $"Primary regulator {profile.PrimaryRegulator} requires urgent compliance", rulesMatched);
            }

            // Rule 3: Medium for international frameworks
            if (templateCode.Contains("ISO") || templateCode.Contains("NIST"))
            {
                rulesMatched++;
                return ("Medium", "International framework - recommended but not mandatory", rulesMatched);
            }

            // Rule 4: Medium if sensitive data
            if (profile?.ProcessesSensitiveData == true || profile?.ProcessesPersonalData == true)
            {
                rulesMatched++;
                return ("Medium", "Organization processes sensitive/personal data", rulesMatched);
            }

            return ("Normal", "Standard compliance requirement", 0);
        }

        /// <summary>
        /// Generate assessment requirements from framework controls
        /// </summary>
        private async Task GenerateAssessmentRequirementsAsync(Assessment assessment, string templateCode, string userId)
        {
            try
            {
                // Get controls for this framework/template
                var controls = await _context.FrameworkControls
                    .Where(fc => fc.FrameworkCode == templateCode && !fc.IsDeleted)
                    .Take(20) // Limit for initial setup
                    .ToListAsync();

                foreach (var control in controls)
                {
                    var requirement = new AssessmentRequirement
                    {
                        Id = Guid.NewGuid(),
                        AssessmentId = assessment.Id,
                        ControlNumber = control.ControlNumber,
                        ControlTitle = control.TitleEn,
                        RequirementText = control.RequirementEn,
                        Domain = control.Domain,
                        ControlType = control.ControlType,
                        Status = "NotStarted",
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = userId
                    };

                    _context.AssessmentRequirements.Add(requirement);
                }

                _logger.LogInformation("Generated {Count} requirements for assessment {AssessmentId}", controls.Count, assessment.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error generating requirements for assessment {AssessmentId}", assessment.Id);
            }
        }

        /// <summary>
        /// Log audit events for onboarding completion
        /// </summary>
        private async Task LogOnboardingCompletedEventAsync(
            Guid tenantId,
            OnboardingWizard wizard,
            RuleExecutionLog? scopeLog,
            Plan? plan,
            string userId)
        {
            try
            {
                var correlationId = Guid.NewGuid().ToString();

                // Log OnboardingCompleted event
                _context.AuditEvents.Add(new AuditEvent
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    EventId = $"evt-{Guid.NewGuid()}",
                    EventType = "OnboardingCompleted",
                    AffectedEntityType = "OnboardingWizard",
                    AffectedEntityId = wizard.Id.ToString(),
                    Action = "Complete",
                    Actor = userId,
                    EventTimestamp = DateTime.UtcNow,
                    PayloadJson = JsonSerializer.Serialize(new
                    {
                        OrganizationName = wizard.OrganizationLegalNameEn,
                        Sector = wizard.IndustrySector,
                        CompletedSteps = 12,
                        ProgressPercent = 100
                    }),
                    CorrelationId = correlationId,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = userId
                });

                // Log ScopeGenerated event
                if (scopeLog != null)
                {
                    _context.AuditEvents.Add(new AuditEvent
                    {
                        Id = Guid.NewGuid(),
                        TenantId = tenantId,
                        EventId = $"evt-{Guid.NewGuid()}",
                        EventType = "ScopeGenerated",
                        AffectedEntityType = "RuleExecutionLog",
                        AffectedEntityId = scopeLog.Id.ToString(),
                        Action = "Generate",
                        Actor = userId,
                        EventTimestamp = DateTime.UtcNow,
                        PayloadJson = scopeLog.DerivedScopeJson ?? "{}",
                        CorrelationId = correlationId,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = userId
                    });
                }

                // Log PlanCreated event
                if (plan != null)
                {
                    _context.AuditEvents.Add(new AuditEvent
                    {
                        Id = Guid.NewGuid(),
                        TenantId = tenantId,
                        EventId = $"evt-{Guid.NewGuid()}",
                        EventType = "PlanCreated",
                        AffectedEntityType = "Plan",
                        AffectedEntityId = plan.Id.ToString(),
                        Action = "Create",
                        Actor = userId,
                        EventTimestamp = DateTime.UtcNow,
                        PayloadJson = JsonSerializer.Serialize(new
                        {
                            plan.PlanCode,
                            plan.Name,
                            plan.PlanType,
                            plan.StartDate,
                            plan.TargetEndDate
                        }),
                        CorrelationId = correlationId,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = userId
                    });
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Logged onboarding audit events for tenant {TenantId}", tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error logging audit events for tenant {TenantId}", tenantId);
            }
        }

        /// <summary>
        /// Auto-assign tasks to team members based on RACI matrix
        /// Called after assessments are created to assign control owners
        /// </summary>
        private async Task AutoAssignTasksByRACIAsync(Guid tenantId, Guid assessmentId, string userId)
        {
            try
            {
                // Get assessment requirements
                var requirements = await _context.AssessmentRequirements
                    .Where(r => r.AssessmentId == assessmentId)
                    .ToListAsync();

                // Get RACI assignments for this tenant
                var raciAssignments = await _context.RACIAssignments
                    .Where(r => r.TenantId == tenantId && r.IsActive && !r.IsDeleted)
                    .Include(r => r.Team)
                    .ToListAsync();

                // Get team members
                var teamMembers = await _context.TeamMembers
                    .Where(tm => tm.TenantId == tenantId && tm.IsActive && !tm.IsDeleted)
                    .ToListAsync();

                // First, create a workflow instance for the assessment
                var workflowInstance = new WorkflowInstance
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    InstanceNumber = $"WF-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..4].ToUpper()}",
                    WorkflowType = "AssessmentReview",
                    CurrentState = "InProgress",
                    Status = "Active",
                    EntityType = "Assessment",
                    EntityId = assessmentId,
                    InitiatedByUserId = Guid.TryParse(userId, out var uid) ? uid : (Guid?)null,
                    StartedAt = DateTime.UtcNow,
                    Variables = JsonSerializer.Serialize(new { AssessmentId = assessmentId }),
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = userId
                };
                _context.WorkflowInstances.Add(workflowInstance);

                var tasksCreated = 0;
                foreach (var requirement in requirements)
                {
                    // Find responsible (R) RACI assignment for this control domain
                    var raci = raciAssignments
                        .FirstOrDefault(r => r.ScopeType == "ControlFamily" &&
                                            r.RACI == "R" &&
                                            (requirement.Domain.Contains(r.ScopeId) || r.ScopeId == "DEFAULT"));

                    if (raci == null)
                    {
                        // Use fallback team
                        raci = raciAssignments.FirstOrDefault(r => r.RACI == "R");
                    }

                    if (raci != null)
                    {
                        // Find team member with matching role
                        var assignee = teamMembers
                            .FirstOrDefault(tm => tm.TeamId == raci.TeamId &&
                                                  tm.RoleCode == raci.RoleCode &&
                                                  tm.IsPrimaryForRole);

                        if (assignee != null)
                        {
                            // Create workflow task using actual entity properties
                            var task = new WorkflowTask
                            {
                                Id = Guid.NewGuid(),
                                TenantId = tenantId,
                                WorkflowInstanceId = workflowInstance.Id,
                                TaskName = $"Review: {requirement.ControlTitle}",
                                Description = requirement.RequirementText,
                                Status = "Pending",
                                Priority = 2, // Medium
                                AssignedToUserId = assignee.UserId,
                                DueDate = DateTime.UtcNow.AddDays(14),
                                Metadata = JsonSerializer.Serialize(new {
                                    RoleCode = raci.RoleCode,
                                    RequirementId = requirement.Id,
                                    EntityType = "AssessmentRequirement"
                                }),
                                CreatedDate = DateTime.UtcNow,
                                CreatedBy = userId
                            };

                            _context.WorkflowTasks.Add(task);
                            tasksCreated++;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Auto-assigned {Count} tasks for assessment {AssessmentId}", tasksCreated, assessmentId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error auto-assigning tasks for assessment {AssessmentId}", assessmentId);
            }
        }

        /// <summary>
        /// Start evidence workflow when evidence is submitted
        /// </summary>
        private async Task StartEvidenceWorkflowAsync(Guid tenantId, Guid evidenceId, string userId)
        {
            try
            {
                // Get the evidence
                var evidence = await _context.Evidences.FirstOrDefaultAsync(e => e.Id == evidenceId);
                if (evidence == null) return;

                // Check if workflow config exists for evidence approval
                var workflowConfig = await _context.TenantWorkflowConfigs
                    .FirstOrDefaultAsync(w => w.TenantId == tenantId &&
                                              w.WorkflowCode == "WF-EVIDENCE-APPROVAL" &&
                                              w.IsEnabled);

                if (workflowConfig == null)
                {
                    _logger.LogInformation("Evidence approval workflow not enabled for tenant {TenantId}", tenantId);
                    return;
                }

                // Get workflow definition
                var workflowDef = await _context.WorkflowDefinitions
                    .FirstOrDefaultAsync(w => w.Name == "Evidence Approval" && w.IsActive);

                if (workflowDef == null)
                {
                    _logger.LogWarning("Evidence approval workflow definition not found");
                    return;
                }

                // Create workflow instance using actual entity properties
                var instance = new WorkflowInstance
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    WorkflowDefinitionId = workflowDef.Id,
                    InstanceNumber = $"WFI-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
                    WorkflowType = "EvidenceApproval",
                    CurrentState = "PendingReview",
                    Status = "Active",
                    EntityType = "Evidence",
                    EntityId = evidenceId,
                    InitiatedByUserId = Guid.TryParse(userId, out var uid) ? uid : (Guid?)null,
                    StartedAt = DateTime.UtcNow,
                    Variables = JsonSerializer.Serialize(new
                    {
                        EvidenceId = evidenceId,
                        EvidenceNumber = evidence.EvidenceNumber,
                        SubmittedBy = userId
                    }),
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = userId
                };

                _context.WorkflowInstances.Add(instance);

                // Create first task (Review task) using actual entity properties
                var reviewTask = new WorkflowTask
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    WorkflowInstanceId = instance.Id,
                    TaskName = $"Review Evidence: {evidence.Title}",
                    Description = "Review and approve/reject evidence submission",
                    Status = "Pending",
                    Priority = 1, // High
                    DueDate = DateTime.UtcNow.AddDays(3),
                    Metadata = JsonSerializer.Serialize(new {
                        RoleCode = "REVIEWER",
                        EntityType = "Evidence",
                        EntityId = evidenceId
                    }),
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = userId
                };

                _context.WorkflowTasks.Add(reviewTask);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Started evidence workflow {InstanceId} for evidence {EvidenceId}",
                    instance.Id, evidenceId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error starting evidence workflow for evidence {EvidenceId}", evidenceId);
            }
        }
    }
}
