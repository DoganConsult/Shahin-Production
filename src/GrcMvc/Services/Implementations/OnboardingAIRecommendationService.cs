using GrcMvc.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// AI-powered recommendation service for onboarding wizard
    /// Uses context-aware logic based on industry, organization type, and common practices
    /// </summary>
    public class OnboardingAIRecommendationService : IOnboardingAIRecommendationService
    {
        private readonly IOnboardingReferenceDataService _referenceDataService;
        private readonly ILogger<OnboardingAIRecommendationService> _logger;

        // Common practice mappings (can be moved to DB later)
        private readonly Dictionary<string, Dictionary<string, List<string>>> _commonPractices = new()
        {
            // Organization Type -> Industry -> Recommended values
            ["enterprise"] = new()
            {
                ["financial_services"] = new() { "regulator_exam", "internal_audit", "external_audit", "board_reporting" },
                ["telecom"] = new() { "regulator_exam", "certification", "customer_due_diligence" },
                ["government"] = new() { "regulator_exam", "internal_audit", "board_reporting" }
            },
            ["SME"] = new()
            {
                ["financial_services"] = new() { "certification", "customer_due_diligence" },
                ["technology"] = new() { "certification", "customer_due_diligence" }
            }
        };

        public OnboardingAIRecommendationService(
            IOnboardingReferenceDataService referenceDataService,
            ILogger<OnboardingAIRecommendationService> logger)
        {
            _referenceDataService = referenceDataService;
            _logger = logger;
        }

        public async Task<OnboardingFieldRecommendationDto> GetRecommendationsAsync(
            string fieldName,
            string section,
            OnboardingContextDto? context = null,
            string? language = "en")
        {
            try
            {
                // Get all available options for this field
                var allOptions = await _referenceDataService.GetOptionsAsync(fieldName, language);
                
                if (!allOptions.Any())
                {
                    return new OnboardingFieldRecommendationDto
                    {
                        FieldName = fieldName,
                        RecommendedOptions = new(),
                        Reasoning = "No options available for this field",
                        ConfidenceScore = 0
                    };
                }

                // Get common options (marked as common in DB)
                var commonOptions = await _referenceDataService.GetCommonOptionsAsync(fieldName, language);

                // Apply context-aware filtering and scoring
                var recommendations = new List<RecommendedOptionDto>();

                foreach (var option in allOptions)
                {
                    var confidence = CalculateConfidence(option, fieldName, context, commonOptions);
                    var explanation = GenerateExplanation(option, fieldName, context, language);

                    recommendations.Add(new RecommendedOptionDto
                    {
                        Value = option.Value,
                        Label = option.Label,
                        Explanation = explanation,
                        ConfidenceScore = confidence,
                        IsStronglyRecommended = confidence >= 80
                    });
                }

                // Sort by confidence (highest first)
                recommendations = recommendations
                    .OrderByDescending(r => r.ConfidenceScore)
                    .ThenBy(r => r.Label)
                    .ToList();

                // Get top 3-5 recommendations
                var topRecommendations = recommendations.Take(5).ToList();

                return new OnboardingFieldRecommendationDto
                {
                    FieldName = fieldName,
                    RecommendedOptions = topRecommendations,
                    Reasoning = GenerateReasoning(fieldName, context, language),
                    HelpText = GenerateHelpText(fieldName, section, language),
                    ConfidenceScore = topRecommendations.FirstOrDefault()?.ConfidenceScore ?? 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating AI recommendations for field {FieldName}", fieldName);
                
                // Fallback: return common options
                var commonOptions = await _referenceDataService.GetCommonOptionsAsync(fieldName, language);
                return new OnboardingFieldRecommendationDto
                {
                    FieldName = fieldName,
                    RecommendedOptions = commonOptions.Select(o => new RecommendedOptionDto
                    {
                        Value = o.Value,
                        Label = o.Label,
                        Explanation = o.Description,
                        ConfidenceScore = 60,
                        IsStronglyRecommended = false
                    }).ToList(),
                    Reasoning = "Recommendations based on common practices",
                    ConfidenceScore = 60
                };
            }
        }

        public async Task<Dictionary<string, OnboardingFieldRecommendationDto>> GetRecommendationsForFieldsAsync(
            List<string> fieldNames,
            string section,
            OnboardingContextDto? context = null,
            string? language = "en")
        {
            var result = new Dictionary<string, OnboardingFieldRecommendationDto>();

            foreach (var fieldName in fieldNames)
            {
                result[fieldName] = await GetRecommendationsAsync(fieldName, section, context, language);
            }

            return result;
        }

        private int CalculateConfidence(
            ReferenceDataOptionDto option,
            string fieldName,
            OnboardingContextDto? context,
            List<ReferenceDataOptionDto> commonOptions)
        {
            int confidence = 50; // Base confidence

            // Boost if marked as common in DB
            if (option.IsCommon || commonOptions.Any(c => c.Value == option.Value))
            {
                confidence += 20;
            }

            // Context-aware boosting
            if (context != null)
            {
                // Industry-specific recommendations
                if (!string.IsNullOrEmpty(context.IndustrySector) && 
                    !string.IsNullOrEmpty(context.OrganizationType))
                {
                    if (_commonPractices.ContainsKey(context.OrganizationType.ToLower()) &&
                        _commonPractices[context.OrganizationType.ToLower()].ContainsKey(context.IndustrySector.ToLower()))
                    {
                        var recommended = _commonPractices[context.OrganizationType.ToLower()][context.IndustrySector.ToLower()];
                        if (recommended.Contains(option.Value))
                        {
                            confidence += 30;
                        }
                    }
                }

                // Country-specific recommendations (e.g., Saudi Arabia -> NCA, SAMA)
                if (!string.IsNullOrEmpty(context.CountryOfIncorporation))
                {
                    if (context.CountryOfIncorporation == "SA" && 
                        (option.Value.Contains("NCA") || option.Value.Contains("SAMA") || option.Value.Contains("SAMA")))
                    {
                        confidence += 25;
                    }
                }
            }

            return Math.Min(100, confidence);
        }

        private string? GenerateExplanation(
            ReferenceDataOptionDto option,
            string fieldName,
            OnboardingContextDto? context,
            string? language)
        {
            if (!string.IsNullOrEmpty(option.Description))
            {
                return option.Description;
            }

            // Generate context-aware explanation
            if (context != null && !string.IsNullOrEmpty(context.IndustrySector))
            {
                if (language == "ar")
                {
                    return $"خيار شائع لـ {context.IndustrySector}";
                }
                return $"Common choice for {context.IndustrySector}";
            }

            return null;
        }

        private string GenerateReasoning(string fieldName, OnboardingContextDto? context, string? language)
        {
            if (context == null)
            {
                return language == "ar" 
                    ? "توصيات بناءً على الممارسات الشائعة" 
                    : "Recommendations based on common practices";
            }

            var parts = new List<string>();

            if (!string.IsNullOrEmpty(context.OrganizationType))
            {
                parts.Add(language == "ar" 
                    ? $"نوع المنظمة: {context.OrganizationType}" 
                    : $"Organization type: {context.OrganizationType}");
            }

            if (!string.IsNullOrEmpty(context.IndustrySector))
            {
                parts.Add(language == "ar" 
                    ? $"القطاع: {context.IndustrySector}" 
                    : $"Industry: {context.IndustrySector}");
            }

            if (parts.Any())
            {
                return language == "ar"
                    ? $"توصيات مخصصة بناءً على: {string.Join(", ", parts)}"
                    : $"Personalized recommendations based on: {string.Join(", ", parts)}";
            }

            return language == "ar"
                ? "توصيات بناءً على الممارسات الشائعة"
                : "Recommendations based on common practices";
        }

        private string? GenerateHelpText(string fieldName, string section, string? language)
        {
            // Field-specific help text (can be moved to DB)
            var helpTexts = new Dictionary<string, Dictionary<string, string>>
            {
                ["en"] = new()
                {
                    ["OrganizationType"] = "Select the type that best describes your organization's structure and regulatory environment.",
                    ["IndustrySector"] = "Choose the primary industry sector where your organization operates.",
                    ["PrimaryDriver"] = "Select the main reason driving your GRC implementation."
                },
                ["ar"] = new()
                {
                    ["OrganizationType"] = "اختر النوع الذي يصف بشكل أفضل هيكل منظمتك والبيئة التنظيمية.",
                    ["IndustrySector"] = "اختر القطاع الصناعي الأساسي الذي تعمل فيه منظمتك.",
                    ["PrimaryDriver"] = "اختر السبب الرئيسي الذي يدفع تنفيذ GRC الخاص بك."
                }
            };

            if (helpTexts.ContainsKey(language ?? "en") && 
                helpTexts[language ?? "en"].ContainsKey(fieldName))
            {
                return helpTexts[language ?? "en"][fieldName];
            }

            return null;
        }
    }
}
