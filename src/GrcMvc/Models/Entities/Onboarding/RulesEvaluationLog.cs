using System;
using System.ComponentModel.DataAnnotations;

namespace GrcMvc.Models.Entities.Onboarding
{
    /// <summary>
    /// Layer 16: Log of rules engine evaluations.
    /// Captures every rule evaluation for audit replay and explainability.
    /// </summary>
    public class RulesEvaluationLog : BaseEntity
    {
        /// <summary>
        /// Reference to the OnboardingWizard
        /// </summary>
        public Guid OnboardingWizardId { get; set; }

        /// <summary>
        /// Reference to the answer snapshot that triggered this evaluation
        /// </summary>
        public Guid? AnswerSnapshotId { get; set; }

        /// <summary>
        /// Step that triggered this evaluation (1-12)
        /// </summary>
        public int TriggerStep { get; set; }

        /// <summary>
        /// Rule code that was evaluated (e.g., RULE_SAMA_APPLICABILITY)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string RuleCode { get; set; } = string.Empty;

        /// <summary>
        /// Rule name for display
        /// </summary>
        [MaxLength(255)]
        public string RuleName { get; set; } = string.Empty;

        /// <summary>
        /// Rule version
        /// </summary>
        [MaxLength(20)]
        public string RuleVersion { get; set; } = "1.0";

        /// <summary>
        /// Input context JSON (fields used for evaluation)
        /// </summary>
        public string InputContextJson { get; set; } = "{}";

        /// <summary>
        /// Evaluation result: MATCHED, NOT_MATCHED, ERROR, SKIPPED
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Result { get; set; } = "NOT_MATCHED";

        /// <summary>
        /// Confidence score (0.0 - 1.0)
        /// </summary>
        public decimal ConfidenceScore { get; set; } = 1.0m;

        /// <summary>
        /// Output JSON (what the rule produced if matched)
        /// </summary>
        public string OutputJson { get; set; } = "{}";

        /// <summary>
        /// Human-readable reason for the evaluation result
        /// </summary>
        public string ReasonText { get; set; } = string.Empty;

        /// <summary>
        /// Arabic reason text
        /// </summary>
        public string? ReasonTextAr { get; set; }

        /// <summary>
        /// Evaluation duration in milliseconds
        /// </summary>
        public int EvaluationDurationMs { get; set; } = 0;

        /// <summary>
        /// When the evaluation occurred
        /// </summary>
        public DateTime EvaluatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Error message if evaluation failed
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Stack trace if error occurred
        /// </summary>
        public string? ErrorStackTrace { get; set; }

        // Navigation
        public virtual OnboardingWizard OnboardingWizard { get; set; } = null!;
        public virtual OnboardingAnswerSnapshot? AnswerSnapshot { get; set; }
    }

    /// <summary>
    /// Rule evaluation results
    /// </summary>
    public static class RuleEvaluationResults
    {
        public const string Matched = "MATCHED";
        public const string NotMatched = "NOT_MATCHED";
        public const string Error = "ERROR";
        public const string Skipped = "SKIPPED";
    }
}
