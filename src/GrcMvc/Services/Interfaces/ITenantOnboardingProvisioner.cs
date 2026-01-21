using System;
using System.Threading.Tasks;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Orchestrates tenant onboarding provisioning.
    /// Creates workspace (ONCE), assessment template, GRC plan, and initial assessments.
    /// </summary>
    public interface ITenantOnboardingProvisioner
    {
        /// <summary>
        /// Ensure default workspace exists for tenant (idempotent - creates only if not exists)
        /// </summary>
        Task<Guid> EnsureDefaultWorkspaceAsync(Guid tenantId, string workspaceName, string createdBy);

        /// <summary>
        /// Create 100-question assessment template for tenant
        /// </summary>
        Task<Guid> CreateAssessmentTemplateAsync(Guid tenantId, string baselineCode, string createdBy);

        /// <summary>
        /// Create initial GRC plan for tenant
        /// </summary>
        Task<Guid> CreateGrcPlanAsync(Guid tenantId, string planName, string createdBy);

        /// <summary>
        /// Create initial assessments from template
        /// </summary>
        Task<int> CreateInitialAssessmentsAsync(Guid tenantId, Guid templateId, Guid planId, string createdBy);

        /// <summary>
        /// Activate default workflows for tenant
        /// </summary>
        Task ActivateDefaultWorkflowsAsync(Guid tenantId, Guid workspaceId, string createdBy);

        /// <summary>
        /// Setup workspace features (dashboards, role configs)
        /// </summary>
        Task SetupWorkspaceFeaturesAsync(Guid tenantId, Guid workspaceId, string createdBy);

        /// <summary>
        /// Run complete onboarding provisioning (background job)
        /// </summary>
        Task<OnboardingProvisioningResult> ProvisionTenantAsync(Guid tenantId, string createdBy);

        /// <summary>
        /// Check if onboarding provisioning is complete
        /// </summary>
        Task<bool> IsProvisioningCompleteAsync(Guid tenantId);
    }

    /// <summary>
    /// Result of onboarding provisioning
    /// </summary>
    public class OnboardingProvisioningResult
    {
        public bool Success { get; set; }
        public Guid? WorkspaceId { get; set; }
        public Guid? AssessmentTemplateId { get; set; }
        public Guid? GrcPlanId { get; set; }
        public int AssessmentsCreated { get; set; }
        public int EvidenceRequirementsCreated { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }
}
