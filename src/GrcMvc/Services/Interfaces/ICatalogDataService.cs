using GrcMvc.Models.DTOs;
using GrcMvc.Models.Entities.Catalogs;

namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Service for querying catalog data (regulators, frameworks, controls, evidence types)
/// Provides dropdown data with sector/company-type filtering
/// Supports all 92+ regulators, 163+ frameworks, 57K+ controls
/// </summary>
public interface ICatalogDataService
{
    /// <summary>
    /// Get all regulators with optional filtering
    /// </summary>
    Task<List<RegulatorCatalogDto>> GetRegulatorsAsync(
        string? sector = null,
        string? country = null,
        string? regionType = null, // saudi, international, regional
        bool activeOnly = true);

    /// <summary>
    /// Get all frameworks with optional filtering
    /// Supports multiple versions per framework
    /// </summary>
    Task<List<FrameworkCatalogDto>> GetFrameworksAsync(
        Guid? regulatorId = null,
        string? sector = null,
        string? companyType = null,
        string? category = null,
        string? version = null,
        bool mandatoryOnly = false,
        bool activeOnly = true);

    /// <summary>
    /// Get all controls for a framework (with version support)
    /// </summary>
    Task<List<ControlCatalogDto>> GetControlsAsync(
        Guid frameworkId,
        string? version = null,
        string? domain = null,
        bool activeOnly = true);

    /// <summary>
    /// Get evidence types required for a control
    /// </summary>
    Task<List<EvidenceTypeCatalogDto>> GetEvidenceTypesAsync(
        Guid controlId,
        bool activeOnly = true);

    /// <summary>
    /// Get evidence types by framework
    /// </summary>
    Task<List<EvidenceTypeCatalogDto>> GetEvidenceTypesByFrameworkAsync(
        Guid frameworkId,
        string? version = null,
        bool activeOnly = true);

    /// <summary>
    /// Get dropdown data for a specific catalog type
    /// Optimized for UI dropdown population
    /// </summary>
    Task<List<DropdownItemDto>> GetDropdownDataAsync(
        string catalogType, // Regulator, Framework, Control, EvidenceType
        Dictionary<string, object>? filters = null,
        string? searchTerm = null,
        int? limit = null);

    /// <summary>
    /// Get frameworks applicable to organization profile
    /// Considers sector, company type, size, critical infrastructure status
    /// </summary>
    Task<List<FrameworkCatalogDto>> GetApplicableFrameworksAsync(
        string sector,
        string companyType,
        string organizationSize,
        bool isCriticalInfrastructure,
        string country = "SA");

    /// <summary>
    /// Get all controls and evidence types for assessment template generation
    /// </summary>
    Task<AssessmentTemplateDataDto> GetAssessmentTemplateDataAsync(
        Guid frameworkId,
        string version);

    /// <summary>
    /// Get framework versions
    /// </summary>
    Task<List<string>> GetFrameworkVersionsAsync(string frameworkCode);
}
