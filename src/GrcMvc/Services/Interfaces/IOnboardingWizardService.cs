using System;
using System.Threading.Tasks;
using GrcMvc.Models.DTOs;
using GrcMvc.Models.Entities;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Service interface for comprehensive onboarding wizard operations.
    /// Handles all 12 sections (A-L) with progressive save and validation.
    /// Uses existing OnboardingWizard entity.
    /// </summary>
    public interface IOnboardingWizardService
    {
        /// <summary>
        /// Start a new onboarding wizard for a tenant.
        /// </summary>
        Task<OnboardingWizard> StartWizardAsync(Guid tenantId, string userId);

        /// <summary>
        /// Get current wizard state for a tenant.
        /// </summary>
        Task<OnboardingWizardStateDto?> GetWizardStateAsync(Guid tenantId);

        /// <summary>
        /// Get wizard progress summary.
        /// </summary>
        Task<WizardProgressSummary> GetProgressAsync(Guid tenantId);

        /// <summary>
        /// Save Section A: Organization Identity (Q1-13).
        /// </summary>
        Task<WizardSectionSaveResponse> SaveSectionAAsync(Guid tenantId, SectionA_OrganizationIdentity section, string userId);

        /// <summary>
        /// Save Section B: Assurance Objective (Q14-18).
        /// </summary>
        Task<WizardSectionSaveResponse> SaveSectionBAsync(Guid tenantId, SectionB_AssuranceObjective section, string userId);

        /// <summary>
        /// Save Section C: Regulatory Applicability (Q19-25).
        /// </summary>
        Task<WizardSectionSaveResponse> SaveSectionCAsync(Guid tenantId, SectionC_RegulatoryApplicability section, string userId);

        /// <summary>
        /// Save Section D: Scope Definition (Q26-34).
        /// </summary>
        Task<WizardSectionSaveResponse> SaveSectionDAsync(Guid tenantId, SectionD_ScopeDefinition section, string userId);

        /// <summary>
        /// Save Section E: Data & Risk Profile (Q35-40).
        /// </summary>
        Task<WizardSectionSaveResponse> SaveSectionEAsync(Guid tenantId, SectionE_DataRiskProfile section, string userId);

        /// <summary>
        /// Save Section F: Technology Landscape (Q41-53).
        /// </summary>
        Task<WizardSectionSaveResponse> SaveSectionFAsync(Guid tenantId, SectionF_TechnologyLandscape section, string userId);

        /// <summary>
        /// Save Section G: Control Ownership (Q54-60).
        /// </summary>
        Task<WizardSectionSaveResponse> SaveSectionGAsync(Guid tenantId, SectionG_ControlOwnership section, string userId);

        /// <summary>
        /// Save Section H: Teams, Roles, Access (Q61-70).
        /// </summary>
        Task<WizardSectionSaveResponse> SaveSectionHAsync(Guid tenantId, SectionH_TeamsRolesAccess section, string userId);

        /// <summary>
        /// Save Section I: Workflow & Cadence (Q71-80).
        /// </summary>
        Task<WizardSectionSaveResponse> SaveSectionIAsync(Guid tenantId, SectionI_WorkflowCadence section, string userId);

        /// <summary>
        /// Save Section J: Evidence Standards (Q81-87).
        /// </summary>
        Task<WizardSectionSaveResponse> SaveSectionJAsync(Guid tenantId, SectionJ_EvidenceStandards section, string userId);

        /// <summary>
        /// Save Section K: Baseline & Overlays (Q88-90).
        /// </summary>
        Task<WizardSectionSaveResponse> SaveSectionKAsync(Guid tenantId, SectionK_BaselineOverlays section, string userId);

        /// <summary>
        /// Save Section L: Go-Live & Metrics (Q91-96).
        /// </summary>
        Task<WizardSectionSaveResponse> SaveSectionLAsync(Guid tenantId, SectionL_GoLiveMetrics section, string userId);

        /// <summary>
        /// Save minimal onboarding data (short form).
        /// </summary>
        Task<WizardSectionSaveResponse> SaveMinimalOnboardingAsync(Guid tenantId, MinimalOnboardingDto data, string userId);

        /// <summary>
        /// Validate wizard completeness (all required fields).
        /// </summary>
        Task<WizardValidationResult> ValidateWizardAsync(Guid tenantId, bool minimalOnly = false);

        /// <summary>
        /// Complete wizard and trigger scope derivation.
        /// </summary>
        Task<WizardCompletionResult> CompleteWizardAsync(Guid tenantId, string userId);

        /// <summary>
        /// Get derived scope based on wizard answers.
        /// </summary>
        Task<OnboardingScopeDto> GetDerivedScopeAsync(Guid tenantId);

        /// <summary>
        /// Validate coverage for a specific section/node
        /// </summary>
        Task<CoverageValidationResult?> ValidateSectionCoverageAsync(Guid tenantId, string sectionId);

        /// <summary>
        /// Get coverage status for all sections
        /// </summary>
        Task<System.Collections.Generic.Dictionary<string, CoverageValidationResult>> GetAllSectionsCoverageAsync(Guid tenantId);
    }

    /// <summary>
    /// Result of wizard validation.
    /// </summary>
    public class WizardValidationResult
    {
        public bool IsValid { get; set; }
        public bool CanComplete { get; set; }
        public int CompletedSections { get; set; }
        public int TotalSections { get; set; } = 12;
        public System.Collections.Generic.List<string> MissingRequiredFields { get; set; } = new();
        public System.Collections.Generic.List<string> Warnings { get; set; } = new();
        public System.Collections.Generic.Dictionary<string, bool> SectionStatus { get; set; } = new();
    }

    /// <summary>
    /// Result of wizard completion.
    /// </summary>
    public class WizardCompletionResult
    {
        public bool Success { get; set; }
        public Guid TenantId { get; set; }
        public string Message { get; set; } = string.Empty;
        public OnboardingScopeDto? DerivedScope { get; set; }
        public System.Collections.Generic.List<string> Errors { get; set; } = new();
        public DateTime CompletedAt { get; set; }
    }
}
