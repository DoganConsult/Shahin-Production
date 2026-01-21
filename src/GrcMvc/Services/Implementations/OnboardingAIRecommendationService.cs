using GrcMvc.Configuration;
using GrcMvc.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// AI-powered recommendation service for onboarding wizard
    /// Integrates with Claude API for intelligent recommendations with rule-based fallback
    /// </summary>
    public class OnboardingAIRecommendationService : IOnboardingAIRecommendationService
    {
        private readonly IOnboardingReferenceDataService _referenceDataService;
        private readonly IClaudeAgentService _claudeService;
        private readonly ILogger<OnboardingAIRecommendationService> _logger;
        private readonly OnboardingAIOptions _options;

        // Common practice mappings (fallback when Claude is unavailable)
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
            IClaudeAgentService claudeService,
            ILogger<OnboardingAIRecommendationService> logger,
            IOptions<OnboardingAIOptions>? options = null)
        {
            _referenceDataService = referenceDataService;
            _claudeService = claudeService;
            _logger = logger;
            _options = options?.Value ?? new OnboardingAIOptions();
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

                // Try Claude AI for intelligent recommendations if enabled
                if (_options.UseClaudeForRecommendations && await _claudeService.IsAvailableAsync())
                {
                    try
                    {
                        var claudeResult = await GetClaudeRecommendationsAsync(
                            fieldName, section, allOptions, context, language);

                        if (claudeResult != null && claudeResult.RecommendedOptions.Any())
                        {
                            _logger.LogDebug("Successfully received Claude AI recommendations for field {FieldName}", fieldName);
                            return claudeResult;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Claude AI recommendation failed for field {FieldName}, falling back to rules", fieldName);
                        // Fall through to rule-based recommendations
                    }
                }

                // Rule-based recommendations (fallback)
                return await GetRuleBasedRecommendationsAsync(fieldName, section, allOptions, commonOptions, context, language);
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

        /// <summary>
        /// Get recommendations using Claude AI
        /// </summary>
        private async Task<OnboardingFieldRecommendationDto?> GetClaudeRecommendationsAsync(
            string fieldName,
            string section,
            List<ReferenceDataOptionDto> allOptions,
            OnboardingContextDto? context,
            string? language)
        {
            var prompt = BuildClaudePrompt(fieldName, section, allOptions, context, language);

            var response = await _claudeService.ChatAsync(
                prompt,
                context: "GRC onboarding wizard field recommendation");

            if (!response.Success || string.IsNullOrEmpty(response.Response))
            {
                return null;
            }

            return ParseClaudeResponse(response.Response, fieldName, allOptions, language);
        }

        /// <summary>
        /// Build the prompt for Claude AI
        /// </summary>
        private string BuildClaudePrompt(
            string fieldName,
            string section,
            List<ReferenceDataOptionDto> options,
            OnboardingContextDto? context,
            string? language)
        {
            var optionsList = string.Join("\n", options.Select(o => $"- {o.Value}: {o.Label} ({o.Description ?? "No description"})"));

            var contextInfo = "";
            if (context != null)
            {
                var contextParts = new List<string>();
                if (!string.IsNullOrEmpty(context.OrganizationType))
                    contextParts.Add($"Organization Type: {context.OrganizationType}");
                if (!string.IsNullOrEmpty(context.IndustrySector))
                    contextParts.Add($"Industry Sector: {context.IndustrySector}");
                if (!string.IsNullOrEmpty(context.CountryOfIncorporation))
                    contextParts.Add($"Country: {context.CountryOfIncorporation}");
                if (!string.IsNullOrEmpty(context.PrimaryDriver))
                    contextParts.Add($"Primary Driver: {context.PrimaryDriver}");

                if (contextParts.Any())
                {
                    contextInfo = $"\n\nOrganization Context:\n{string.Join("\n", contextParts)}";
                }
            }

            return $@"You are a GRC (Governance, Risk, Compliance) expert helping organizations with onboarding.

For the field ""{fieldName}"" in section ""{section}"" of the GRC onboarding wizard, recommend the best options from the available choices.{contextInfo}

Available options:
{optionsList}

Respond with a JSON object in this exact format:
{{
  ""recommendations"": [
    {{
      ""value"": ""option_value"",
      ""confidence"": 85,
      ""explanation"": ""Brief explanation why this is recommended""
    }}
  ],
  ""reasoning"": ""Overall reasoning for these recommendations""
}}

Rules:
- Select 3-5 most relevant options
- Confidence should be 0-100
- Consider industry best practices and regulatory requirements
- If country is SA (Saudi Arabia), prioritize NCA-ECC, SAMA-CSF frameworks
- Language for explanations: {language ?? "en"}
- Be concise but helpful";
        }

        /// <summary>
        /// Parse Claude's response into a structured recommendation
        /// </summary>
        private OnboardingFieldRecommendationDto? ParseClaudeResponse(
            string response,
            string fieldName,
            List<ReferenceDataOptionDto> allOptions,
            string? language)
        {
            try
            {
                // Extract JSON from response (Claude may include additional text)
                var jsonStart = response.IndexOf('{');
                var jsonEnd = response.LastIndexOf('}');

                if (jsonStart < 0 || jsonEnd < jsonStart)
                {
                    _logger.LogWarning("Could not find JSON in Claude response for field {FieldName}", fieldName);
                    return null;
                }

                var jsonContent = response.Substring(jsonStart, jsonEnd - jsonStart + 1);
                var parsed = JsonSerializer.Deserialize<ClaudeRecommendationResponse>(jsonContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (parsed?.Recommendations == null || !parsed.Recommendations.Any())
                {
                    return null;
                }

                var recommendations = new List<RecommendedOptionDto>();
                foreach (var rec in parsed.Recommendations)
                {
                    var matchedOption = allOptions.FirstOrDefault(o =>
                        o.Value.Equals(rec.Value, StringComparison.OrdinalIgnoreCase));

                    if (matchedOption != null)
                    {
                        recommendations.Add(new RecommendedOptionDto
                        {
                            Value = matchedOption.Value,
                            Label = matchedOption.Label,
                            Explanation = rec.Explanation ?? matchedOption.Description,
                            ConfidenceScore = Math.Clamp(rec.Confidence, 0, 100),
                            IsStronglyRecommended = rec.Confidence >= 80
                        });
                    }
                }

                if (!recommendations.Any())
                {
                    return null;
                }

                return new OnboardingFieldRecommendationDto
                {
                    FieldName = fieldName,
                    RecommendedOptions = recommendations.OrderByDescending(r => r.ConfidenceScore).ToList(),
                    Reasoning = parsed.Reasoning ?? (language == "ar"
                        ? "توصيات ذكية من نظام الذكاء الاصطناعي"
                        : "AI-powered recommendations based on your organization context"),
                    HelpText = GenerateHelpText(fieldName, "", language),
                    ConfidenceScore = recommendations.Max(r => r.ConfidenceScore)
                };
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse Claude JSON response for field {FieldName}", fieldName);
                return null;
            }
        }

        /// <summary>
        /// Get rule-based recommendations (fallback)
        /// </summary>
        private async Task<OnboardingFieldRecommendationDto> GetRuleBasedRecommendationsAsync(
            string fieldName,
            string section,
            List<ReferenceDataOptionDto> allOptions,
            List<ReferenceDataOptionDto> commonOptions,
            OnboardingContextDto? context,
            string? language)
        {
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

        /// <summary>
        /// Response model for Claude AI recommendations
        /// </summary>
        private class ClaudeRecommendationResponse
        {
            public List<ClaudeRecommendationItem>? Recommendations { get; set; }
            public string? Reasoning { get; set; }
        }

        private class ClaudeRecommendationItem
        {
            public string Value { get; set; } = string.Empty;
            public int Confidence { get; set; }
            public string? Explanation { get; set; }
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
