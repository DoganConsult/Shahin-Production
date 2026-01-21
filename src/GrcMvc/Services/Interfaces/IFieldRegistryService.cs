using GrcMvc.Models.DTOs;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Service for managing the canonical field registry
    /// Validates field IDs against known field definitions
    /// </summary>
    public interface IFieldRegistryService
    {
        /// <summary>
        /// Load field registry from configuration or generate from OnboardingWizard entity
        /// </summary>
        Task<FieldRegistry> LoadRegistryAsync(CancellationToken ct = default);

        /// <summary>
        /// Validate if a field ID exists in the registry
        /// </summary>
        Task<bool> ValidateFieldIdAsync(string fieldId, CancellationToken ct = default);

        /// <summary>
        /// Validate multiple field IDs and return missing ones
        /// </summary>
        Task<List<string>> ValidateFieldIdsAsync(IEnumerable<string> fieldIds, CancellationToken ct = default);

        /// <summary>
        /// Get field entry by ID
        /// </summary>
        Task<FieldRegistryEntry?> GetFieldEntryAsync(string fieldId, CancellationToken ct = default);

        /// <summary>
        /// Get all field entries
        /// </summary>
        Task<Dictionary<string, FieldRegistryEntry>> GetAllFieldsAsync(CancellationToken ct = default);
    }
}