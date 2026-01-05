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
    }
}
