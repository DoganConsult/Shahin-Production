namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Service for fetching reference data (dropdown options) for onboarding wizard
    /// </summary>
    public interface IOnboardingReferenceDataService
    {
        /// <summary>
        /// Get all dropdown options for a specific category/field
        /// </summary>
        Task<List<ReferenceDataOptionDto>> GetOptionsAsync(string category, string? language = "en");

        /// <summary>
        /// Get all categories available
        /// </summary>
        Task<List<string>> GetCategoriesAsync();

        /// <summary>
        /// Get options for multiple categories at once
        /// </summary>
        Task<Dictionary<string, List<ReferenceDataOptionDto>>> GetOptionsForCategoriesAsync(
            List<string> categories, 
            string? language = "en");

        /// <summary>
        /// Get common/recommended options for a category (for AI suggestions)
        /// </summary>
        Task<List<ReferenceDataOptionDto>> GetCommonOptionsAsync(string category, string? language = "en");

        /// <summary>
        /// Get filtered options based on previous step selections
        /// Used for Step D filtering based on Step C selections
        /// </summary>
        Task<List<ReferenceDataOptionDto>> GetFilteredOptionsAsync(
            string category,
            Dictionary<string, object>? filterContext = null,
            string? language = "en");

        /// <summary>
        /// Get options filtered by audit scope type (from Step C)
        /// </summary>
        Task<List<ReferenceDataOptionDto>> GetOptionsByAuditScopeAsync(
            string category,
            string auditScopeType,
            string? language = "en");

        /// <summary>
        /// Get options filtered by selected frameworks (from Step C)
        /// </summary>
        Task<List<ReferenceDataOptionDto>> GetOptionsByFrameworksAsync(
            string category,
            List<string> frameworkCodes,
            string? language = "en");
    }

    /// <summary>
    /// DTO for reference data options
    /// </summary>
    public class ReferenceDataOptionDto
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public bool IsCommon { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }
}
