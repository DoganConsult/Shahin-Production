namespace GrcMvc.Configuration
{
    /// <summary>
    /// Configuration options for Onboarding AI recommendations
    /// </summary>
    public class OnboardingAIOptions
    {
        /// <summary>
        /// Configuration section name in appsettings.json
        /// </summary>
        public const string SectionName = "OnboardingAI";

        /// <summary>
        /// Whether to use Claude AI for intelligent recommendations
        /// Default: true (will use rules-based fallback if Claude unavailable)
        /// </summary>
        public bool UseClaudeForRecommendations { get; set; } = true;

        /// <summary>
        /// Whether to fallback to rules-based recommendations if Claude fails
        /// Default: true
        /// </summary>
        public bool FallbackToRulesOnFailure { get; set; } = true;

        /// <summary>
        /// Maximum tokens for Claude AI response
        /// Default: 500 (sufficient for recommendation JSON)
        /// </summary>
        public int MaxTokens { get; set; } = 500;

        /// <summary>
        /// Cache duration for recommendations in minutes
        /// Default: 30 minutes
        /// </summary>
        public int CacheDurationMinutes { get; set; } = 30;

        /// <summary>
        /// Minimum confidence score to mark as "strongly recommended"
        /// Default: 80
        /// </summary>
        public int StrongRecommendationThreshold { get; set; } = 80;

        /// <summary>
        /// Maximum number of recommendations to return per field
        /// Default: 5
        /// </summary>
        public int MaxRecommendationsPerField { get; set; } = 5;
    }
}
