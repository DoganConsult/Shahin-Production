using GrcMvc.Models.Entities;
using GrcMvc.Services.Implementations;

namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Expert-driven sector mapping service interface
/// Automatically determines the full compliance lifecycle based on organization sector
/// </summary>
public interface IExpertFrameworkMappingService
{
    /// <summary>
    /// Get the full compliance blueprint for a sector
    /// </summary>
    SectorComplianceBlueprint GetSectorBlueprint(string sector);

    /// <summary>
    /// Get applicable frameworks for an organization based on profile
    /// </summary>
    List<FrameworkMapping> GetApplicableFrameworks(OrganizationProfile profile);

    /// <summary>
    /// Get evidence requirements for a sector
    /// </summary>
    List<SectorEvidenceRequirement> GetEvidenceRequirements(string sector);

    /// <summary>
    /// Get scoring weights for a sector
    /// </summary>
    Dictionary<string, double> GetScoringWeights(string sector);

    /// <summary>
    /// Get implementation guidance for a sector
    /// </summary>
    SectorImplementationGuidance GetImplementationGuidance(string sector);

    /// <summary>
    /// Apply expert mapping to organization and generate derived scope
    /// </summary>
    Task<ExpertMappingResult> ApplyExpertMappingAsync(Guid tenantId, string userId);
}
