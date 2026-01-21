using GrcMvc.Models.DTOs;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Service for validating onboarding coverage against the manifest specification
    /// </summary>
    public interface IOnboardingCoverageService
    {
        /// <summary>
        /// Load coverage manifest from YAML file
        /// </summary>
        Task<CoverageManifest> LoadManifestAsync(CancellationToken ct = default);

        /// <summary>
        /// Validate coverage for a specific node
        /// </summary>
        Task<NodeCoverageResult> ValidateNodeCoverageAsync(
            string nodeId,
            IFieldValueProvider fieldProvider,
            CancellationToken ct = default);

        /// <summary>
        /// Validate coverage for a specific mission
        /// </summary>
        Task<MissionCoverageResult> ValidateMissionCoverageAsync(
            string missionId,
            IFieldValueProvider fieldProvider,
            CancellationToken ct = default);

        /// <summary>
        /// Validate complete coverage for all nodes and missions
        /// </summary>
        Task<CoverageValidationResult> ValidateCompleteCoverageAsync(
            IFieldValueProvider fieldProvider,
            CancellationToken ct = default);

        /// <summary>
        /// Run integrity checks on the manifest
        /// </summary>
        Task<Dictionary<string, bool>> RunIntegrityChecksAsync(
            FieldRegistry fieldRegistry,
            CancellationToken ct = default);

        /// <summary>
        /// Evaluate conditional required rules and return fields that should be required
        /// </summary>
        List<string> EvaluateConditionalRequired(
            IFieldValueProvider fieldProvider,
            CoverageManifest manifest);

        /// <summary>
        /// Get required field IDs for a node
        /// </summary>
        List<string> GetRequiredFieldsForNode(string nodeId, CoverageManifest manifest);

        /// <summary>
        /// Get required field IDs for a mission
        /// </summary>
        List<string> GetRequiredFieldsForMission(string missionId, CoverageManifest manifest);
    }
}
