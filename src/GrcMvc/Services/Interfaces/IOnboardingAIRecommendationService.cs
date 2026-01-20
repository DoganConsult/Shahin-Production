namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// AI-powered recommendation service for onboarding wizard fields
    /// Provides intelligent suggestions based on user context, section, and common practices
    /// </summary>
    public interface IOnboardingAIRecommendationService
    {
        /// <summary>
        /// Get AI recommendations for a specific field based on context
        /// </summary>
        Task<OnboardingFieldRecommendationDto> GetRecommendationsAsync(
            string fieldName,
            string section,
            OnboardingContextDto? context = null,
            string? language = "en");

        /// <summary>
        /// Get AI recommendations for multiple fields at once
        /// </summary>
        Task<Dictionary<string, OnboardingFieldRecommendationDto>> GetRecommendationsForFieldsAsync(
            List<string> fieldNames,
            string section,
            OnboardingContextDto? context = null,
            string? language = "en");
    }

    /// <summary>
    /// Context information for AI recommendations
    /// </summary>
    public class OnboardingContextDto
    {
        public Guid? TenantId { get; set; }
        public string? OrganizationType { get; set; }
        public string? IndustrySector { get; set; }
        public string? CountryOfIncorporation { get; set; }
        public string? PrimaryDriver { get; set; }
        public Dictionary<string, string>? PreviousAnswers { get; set; }
    }

    /// <summary>
    /// AI recommendation for a specific field
    /// </summary>
    public class OnboardingFieldRecommendationDto
    {
        public string FieldName { get; set; } = string.Empty;
        public List<RecommendedOptionDto> RecommendedOptions { get; set; } = new();
        public string? Reasoning { get; set; }
        public string? HelpText { get; set; }
        public int ConfidenceScore { get; set; } // 0-100
    }

    /// <summary>
    /// A recommended option with explanation
    /// </summary>
    public class RecommendedOptionDto
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string? Explanation { get; set; }
        public int ConfidenceScore { get; set; } // 0-100
        public bool IsStronglyRecommended { get; set; }
    }
}
