using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Arabic Compliance Assistant Interface
/// مساعد الامتثال العربي
/// AI-powered compliance assistant with native Arabic support
/// </summary>
public interface IArabicComplianceAssistant
{
    /// <summary>
    /// Ask a compliance question in Arabic or English
    /// </summary>
    Task<ComplianceAnswer> AskQuestionAsync(string question, Guid tenantId, string language = "ar");

    /// <summary>
    /// Generate an Arabic compliance report
    /// </summary>
    Task<byte[]> GenerateArabicReportAsync(Guid assessmentId, string reportType);

    /// <summary>
    /// Analyze document for compliance (Arabic/English)
    /// </summary>
    Task<DocumentAnalysisResult> AnalyzeDocumentAsync(Stream documentStream, string fileName, Guid tenantId);

    /// <summary>
    /// Get compliance recommendations in Arabic
    /// </summary>
    Task<List<ArabicRecommendation>> GetRecommendationsAsync(Guid tenantId);

    /// <summary>
    /// Translate compliance content
    /// </summary>
    Task<string> TranslateContentAsync(string content, string fromLanguage, string toLanguage);

    /// <summary>
    /// Generate control implementation guidance in Arabic
    /// </summary>
    Task<ControlGuidance> GetControlGuidanceAsync(string controlCode, string language = "ar");

    /// <summary>
    /// Summarize assessment findings in Arabic
    /// </summary>
    Task<ArabicAssessmentSummary> SummarizeAssessmentAsync(Guid assessmentId, string language = "ar");

    /// <summary>
    /// Get contextual help for GRC terms
    /// </summary>
    Task<GrcTermHelp> GetTermHelpAsync(string term, string language = "ar");
}

/// <summary>
/// Compliance answer from AI assistant
/// </summary>
public class ComplianceAnswer
{
    public string QuestionId { get; set; } = Guid.NewGuid().ToString();
    public string OriginalQuestion { get; set; } = string.Empty;
    public string AnswerEn { get; set; } = string.Empty;
    public string AnswerAr { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; }
    public List<string> RelevantFrameworks { get; set; } = new();
    public List<string> RelevantControls { get; set; } = new();
    public List<SourceReference> Sources { get; set; } = new();
    public List<string> FollowUpQuestions { get; set; } = new();
    public DateTime AnsweredAt { get; set; }
}

/// <summary>
/// Source reference for answer
/// </summary>
public class SourceReference
{
    public string Type { get; set; } = string.Empty; // Framework, Regulation, Policy, Control
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

/// <summary>
/// Document analysis result
/// </summary>
public class DocumentAnalysisResult
{
    public string DocumentId { get; set; } = Guid.NewGuid().ToString();
    public string FileName { get; set; } = string.Empty;
    public string DetectedLanguage { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty; // Policy, Procedure, Evidence, Report

    public double ComplianceScore { get; set; }
    public List<ArabicComplianceGap> Gaps { get; set; } = new();
    public List<string> KeyFindings { get; set; } = new();
    public List<string> KeyFindingsAr { get; set; } = new();
    public List<string> RecommendationsEn { get; set; } = new();
    public List<string> RecommendationsAr { get; set; } = new();
    public List<string> MappedControls { get; set; } = new();
    public DateTime AnalyzedAt { get; set; }
}

/// <summary>
/// Compliance gap identified by Arabic assistant
/// </summary>
public class ArabicComplianceGap
{
    public string GapId { get; set; } = Guid.NewGuid().ToString();
    public string DescriptionEn { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty; // High, Medium, Low
    public string RelatedControl { get; set; } = string.Empty;
    public string RemediationEn { get; set; } = string.Empty;
    public string RemediationAr { get; set; } = string.Empty;
}

/// <summary>
/// Arabic recommendation
/// </summary>
public class ArabicRecommendation
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TitleEn { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public List<string> ActionItemsEn { get; set; } = new();
    public List<string> ActionItemsAr { get; set; } = new();
    public double ImpactScore { get; set; }
    public int EstimatedEffortDays { get; set; }
}

/// <summary>
/// Control implementation guidance
/// </summary>
public class ControlGuidance
{
    public string ControlCode { get; set; } = string.Empty;
    public string ControlTitleEn { get; set; } = string.Empty;
    public string ControlTitleAr { get; set; } = string.Empty;

    public string OverviewEn { get; set; } = string.Empty;
    public string OverviewAr { get; set; } = string.Empty;

    public List<ImplementationStep> ImplementationSteps { get; set; } = new();
    public List<string> RequiredEvidenceEn { get; set; } = new();
    public List<string> RequiredEvidenceAr { get; set; } = new();
    public List<string> CommonMistakesEn { get; set; } = new();
    public List<string> CommonMistakesAr { get; set; } = new();
    public List<string> BestPracticesEn { get; set; } = new();
    public List<string> BestPracticesAr { get; set; } = new();

    public string FrameworkReference { get; set; } = string.Empty;
    public List<string> RelatedControls { get; set; } = new();
}

/// <summary>
/// Implementation step
/// </summary>
public class ImplementationStep
{
    public int StepNumber { get; set; }
    public string TitleEn { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    public string ResponsibleRole { get; set; } = string.Empty;
    public int EstimatedDays { get; set; }
}

/// <summary>
/// Arabic assessment summary
/// </summary>
public class ArabicAssessmentSummary
{
    public Guid AssessmentId { get; set; }
    public string AssessmentTitle { get; set; } = string.Empty;

    public string ExecutiveSummaryEn { get; set; } = string.Empty;
    public string ExecutiveSummaryAr { get; set; } = string.Empty;

    public double OverallScore { get; set; }
    public string OverallStatus { get; set; } = string.Empty;

    public int TotalControls { get; set; }
    public int CompliantControls { get; set; }
    public int PartialControls { get; set; }
    public int NonCompliantControls { get; set; }

    public List<string> KeyStrengthsEn { get; set; } = new();
    public List<string> KeyStrengthsAr { get; set; } = new();
    public List<string> KeyWeaknessesEn { get; set; } = new();
    public List<string> KeyWeaknessesAr { get; set; } = new();
    public List<string> PriorityActionsEn { get; set; } = new();
    public List<string> PriorityActionsAr { get; set; } = new();

    public DateTime GeneratedAt { get; set; }
}

/// <summary>
/// GRC term help/glossary
/// </summary>
public class GrcTermHelp
{
    public string Term { get; set; } = string.Empty;
    public string TermAr { get; set; } = string.Empty;
    public string DefinitionEn { get; set; } = string.Empty;
    public string DefinitionAr { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public List<string> RelatedTerms { get; set; } = new();
    public List<string> ExamplesEn { get; set; } = new();
    public List<string> ExamplesAr { get; set; } = new();
    public string SourceFramework { get; set; } = string.Empty;
}
