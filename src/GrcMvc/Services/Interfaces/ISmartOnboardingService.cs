using GrcMvc.Models.DTOs;

namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Smart onboarding service that auto-generates assessment templates and GRC plans
/// after onboarding completion, aligned with KSA regulations
/// </summary>
public interface ISmartOnboardingService
{
    /// <summary>
    /// Complete onboarding and auto-generate assessment templates and GRC plan
    /// </summary>
    Task<SmartOnboardingResultDto> CompleteSmartOnboardingAsync(Guid tenantId, string userId);

    /// <summary>
    /// Generate assessment templates based on organization profile and KSA frameworks
    /// </summary>
    Task<List<GeneratedAssessmentTemplateDto>> GenerateAssessmentTemplatesAsync(Guid tenantId);

    /// <summary>
    /// Generate comprehensive GRC plan aligned with KSA regulations
    /// </summary>
    Task<GeneratedGrcPlanDto> GenerateGrcPlanAsync(Guid tenantId, string userId);
}
