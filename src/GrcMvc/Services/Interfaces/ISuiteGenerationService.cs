using GrcMvc.Models.Entities;

namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Suite Generation Service Interface
/// Automatically generates control suites from onboarding profiles using Baseline + Overlays
/// </summary>
public interface ISuiteGenerationService
{
    /// <summary>
    /// Generate a control suite from organization profile
    /// </summary>
    Task<GeneratedControlSuite> GenerateSuiteAsync(
        Guid tenantId,
        OrganizationProfile profile,
        string generatedBy);

    /// <summary>
    /// Generate suite for a specific organization entity (for multi-sector orgs)
    /// </summary>
    Task<GeneratedControlSuite> GenerateSuiteForEntityAsync(
        Guid tenantId,
        OrganizationEntity entity,
        string generatedBy);
}
