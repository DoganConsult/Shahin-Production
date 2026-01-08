using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GrcMvc.Models.Entities;
using GrcMvc.Models.DTOs;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Interface for tenant onboarding workflow.
    /// Handles organizational profile setup and rules engine invocation.
    /// Enhanced for smooth user experience.
    /// </summary>
    public interface IOnboardingService
    {
        /// <summary>
        /// Save organizational profile from onboarding questionnaire.
        /// </summary>
        Task<OrganizationProfile> SaveOrganizationProfileAsync(
            Guid tenantId,
            string orgType,
            string sector,
            string country,
            string dataTypes,
            string hostingModel,
            string organizationSize,
            string complianceMaturity,
            string vendors,
            Dictionary<string, string> questionnaire,
            string userId);

        /// <summary>
        /// Complete onboarding and trigger rules engine to derive scope.
        /// </summary>
        Task<RuleExecutionLog> CompleteOnboardingAsync(Guid tenantId, string userId);

        /// <summary>
        /// Get derived scope (applicable baselines, packages, templates) for tenant.
        /// </summary>
        Task<OnboardingScopeDto> GetDerivedScopeAsync(Guid tenantId);

        /// <summary>
        /// Re-evaluate and refresh scope when profile or assets change.
        /// Triggers rules engine to re-derive applicable baselines/packages/templates.
        /// </summary>
        Task<RuleExecutionLog> RefreshScopeAsync(Guid tenantId, string userId, string reason);

        // ============================================
        // Enhanced UX Methods for Smooth Onboarding
        // ============================================

        /// <summary>
        /// Get current onboarding status and progress for a tenant.
        /// Returns step info, completion percentage, and next actions.
        /// </summary>
        Task<OnboardingStatusDto> GetOnboardingStatusAsync(Guid tenantId);

        /// <summary>
        /// Check if tenant can proceed to a specific onboarding step.
        /// Returns validation result with any blockers.
        /// </summary>
        Task<StepValidationResult> CanProceedToStepAsync(Guid tenantId, string stepName);

        /// <summary>
        /// Get recommended next step based on current progress.
        /// </summary>
        Task<NextStepRecommendation> GetNextStepAsync(Guid tenantId);

        /// <summary>
        /// Resume onboarding from last saved state.
        /// Returns the step to redirect to.
        /// </summary>
        Task<ResumeOnboardingResult> ResumeOnboardingAsync(Guid tenantId);

        /// <summary>
        /// Check if organization profile exists for tenant.
        /// </summary>
        Task<bool> HasOrganizationProfileAsync(Guid tenantId);

        /// <summary>
        /// Get onboarding completion percentage.
        /// </summary>
        Task<int> GetCompletionPercentageAsync(Guid tenantId);
    }

    /// <summary>
    /// Onboarding status with progress tracking
    /// </summary>
    public class OnboardingStatusDto
    {
        public Guid TenantId { get; set; }
        public string CurrentStep { get; set; } = "NotStarted";
        public int CompletionPercentage { get; set; }
        public bool IsProfileComplete { get; set; }
        public bool IsScopeDerived { get; set; }
        public bool IsOnboardingComplete { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<OnboardingStepStatus> Steps { get; set; } = new();
        public string? NextAction { get; set; }
        public string? NextActionUrl { get; set; }
    }

    /// <summary>
    /// Individual step status
    /// </summary>
    public class OnboardingStepStatus
    {
        public string StepName { get; set; } = string.Empty;
        public string StepNameAr { get; set; } = string.Empty;
        public int StepNumber { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed, Skipped
        public bool IsRequired { get; set; } = true;
        public bool IsAccessible { get; set; }
        public string? CompletedBy { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    /// <summary>
    /// Step validation result
    /// </summary>
    public class StepValidationResult
    {
        public bool CanProceed { get; set; }
        public string? BlockerReason { get; set; }
        public string? BlockerReasonAr { get; set; }
        public List<string> MissingPrerequisites { get; set; } = new();
        public string? RedirectToStep { get; set; }
    }

    /// <summary>
    /// Next step recommendation
    /// </summary>
    public class NextStepRecommendation
    {
        public string StepName { get; set; } = string.Empty;
        public string StepNameAr { get; set; } = string.Empty;
        public string StepUrl { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DescriptionAr { get; set; } = string.Empty;
        public int EstimatedMinutes { get; set; }
        public bool IsOptional { get; set; }
    }

    /// <summary>
    /// Resume onboarding result
    /// </summary>
    public class ResumeOnboardingResult
    {
        public bool CanResume { get; set; }
        public string ResumeStep { get; set; } = string.Empty;
        public string ResumeUrl { get; set; } = string.Empty;
        public int CompletedSteps { get; set; }
        public int TotalSteps { get; set; }
        public DateTime? LastActivityAt { get; set; }
        public string? WelcomeBackMessage { get; set; }
        public string? WelcomeBackMessageAr { get; set; }
    }
}
