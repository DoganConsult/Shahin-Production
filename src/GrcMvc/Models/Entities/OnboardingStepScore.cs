using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Gamification: Tracks points, scores, and achievements for each onboarding step
    /// Links each step to Assessment Templates, GRC Requirements, and Workflows
    /// </summary>
    public class OnboardingStepScore : BaseEntity
    {
        /// <summary>
        /// Reference to the onboarding wizard
        /// </summary>
        public Guid OnboardingWizardId { get; set; }

        /// <summary>
        /// Tenant ID for multi-tenancy
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// Step number (1-12)
        /// </summary>
        [Range(1, 12)]
        public int StepNumber { get; set; }

        /// <summary>
        /// Step letter (A-L)
        /// </summary>
        [MaxLength(1)]
        public string StepLetter { get; set; } = string.Empty;

        /// <summary>
        /// Step name (e.g., "Organization Identity")
        /// </summary>
        [MaxLength(100)]
        public string StepName { get; set; } = string.Empty;

        // ============================================================================
        // GAMIFICATION SCORING
        // ============================================================================

        /// <summary>
        /// Total points available for this step (base value)
        /// </summary>
        public int TotalPointsAvailable { get; set; } = 100;

        /// <summary>
        /// Points earned by completing this step
        /// </summary>
        public int PointsEarned { get; set; } = 0;

        /// <summary>
        /// Bonus points for speed (completed under estimated time)
        /// </summary>
        public int SpeedBonus { get; set; } = 0;

        /// <summary>
        /// Bonus points for thoroughness (all optional questions answered)
        /// </summary>
        public int ThoroughnessBonus { get; set; } = 0;

        /// <summary>
        /// Bonus points for quality (validation passed on first try)
        /// </summary>
        public int QualityBonus { get; set; } = 0;

        /// <summary>
        /// Total score for this step (Points + Bonuses)
        /// </summary>
        public int TotalScore { get; set; } = 0;

        /// <summary>
        /// Star rating (1-5 stars based on completion quality)
        /// </summary>
        [Range(0, 5)]
        public int StarRating { get; set; } = 0;

        /// <summary>
        /// Achievement level: Bronze, Silver, Gold, Platinum
        /// </summary>
        [MaxLength(20)]
        public string AchievementLevel { get; set; } = string.Empty;

        // ============================================================================
        // PROGRESS TRACKING
        // ============================================================================

        /// <summary>
        /// Total questions in this step
        /// </summary>
        public int TotalQuestions { get; set; } = 0;

        /// <summary>
        /// Required questions (must answer to proceed)
        /// </summary>
        public int RequiredQuestions { get; set; } = 0;

        /// <summary>
        /// Questions answered
        /// </summary>
        public int QuestionsAnswered { get; set; } = 0;

        /// <summary>
        /// Required questions answered
        /// </summary>
        public int RequiredQuestionsAnswered { get; set; } = 0;

        /// <summary>
        /// Completion percentage for this step (0-100)
        /// </summary>
        [Range(0, 100)]
        public int CompletionPercent { get; set; } = 0;

        /// <summary>
        /// Status: NotStarted, InProgress, Completed, Validated
        /// </summary>
        [MaxLength(20)]
        public string Status { get; set; } = "NotStarted";

        // ============================================================================
        // TIME TRACKING
        // ============================================================================

        /// <summary>
        /// Estimated time to complete (minutes)
        /// </summary>
        public int EstimatedTimeMinutes { get; set; } = 5;

        /// <summary>
        /// Actual time spent (minutes)
        /// </summary>
        public int ActualTimeMinutes { get; set; } = 0;

        /// <summary>
        /// When step was started
        /// </summary>
        public DateTime? StartedAt { get; set; }

        /// <summary>
        /// When step was completed
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        // ============================================================================
        // ASSESSMENT TEMPLATE LINKAGE
        // ============================================================================

        /// <summary>
        /// Associated assessment template ID (if this step creates/uses an assessment)
        /// </summary>
        public Guid? AssessmentTemplateId { get; set; }

        /// <summary>
        /// Assessment template name
        /// </summary>
        [MaxLength(200)]
        public string AssessmentTemplateName { get; set; } = string.Empty;

        /// <summary>
        /// Assessment instance ID (created when step is completed)
        /// </summary>
        public Guid? AssessmentInstanceId { get; set; }

        /// <summary>
        /// Assessment status: NotCreated, Created, InProgress, Completed
        /// </summary>
        [MaxLength(20)]
        public string AssessmentStatus { get; set; } = "NotCreated";

        // ============================================================================
        // GRC REQUIREMENTS LINKAGE
        // ============================================================================

        /// <summary>
        /// GRC requirements mapped to this step (JSON array of requirement IDs)
        /// </summary>
        public string GrcRequirementIdsJson { get; set; } = "[]";

        /// <summary>
        /// Number of GRC requirements linked
        /// </summary>
        public int GrcRequirementsCount { get; set; } = 0;

        /// <summary>
        /// GRC requirements satisfied by completing this step
        /// </summary>
        public int GrcRequirementsSatisfied { get; set; } = 0;

        /// <summary>
        /// Compliance frameworks impacted (JSON array: NCA-ECC, SAMA-CSF, ISO27001, etc.)
        /// </summary>
        public string ComplianceFrameworksJson { get; set; } = "[]";

        // ============================================================================
        // WORKFLOW LINKAGE
        /// </summary>
        public Guid? WorkflowId { get; set; }

        /// <summary>
        /// Workflow name (e.g., "Organization Setup Workflow")
        /// </summary>
        [MaxLength(200)]
        public string WorkflowName { get; set; } = string.Empty;

        /// <summary>
        /// Workflow instance ID (created when step triggers workflow)
        /// </summary>
        public Guid? WorkflowInstanceId { get; set; }

        /// <summary>
        /// Workflow status: NotTriggered, Triggered, InProgress, Completed
        /// </summary>
        [MaxLength(20)]
        public string WorkflowStatus { get; set; } = "NotTriggered";

        /// <summary>
        /// Workflow tasks created (JSON array)
        /// </summary>
        public string WorkflowTasksJson { get; set; } = "[]";

        // ============================================================================
        // VALIDATION & QUALITY
        // ============================================================================

        /// <summary>
        /// Validation errors encountered (JSON object)
        /// </summary>
        public string ValidationErrorsJson { get; set; } = "{}";

        /// <summary>
        /// Number of validation attempts before success
        /// </summary>
        public int ValidationAttempts { get; set; } = 0;

        /// <summary>
        /// Data quality score (0-100)
        /// </summary>
        [Range(0, 100)]
        public int DataQualityScore { get; set; } = 0;

        /// <summary>
        /// Completeness score (0-100): measures how many optional fields filled
        /// </summary>
        [Range(0, 100)]
        public int CompletenessScore { get; set; } = 0;

        // ============================================================================
        // NAVIGATION PROPERTIES
        /// </summary>
        [ForeignKey(nameof(OnboardingWizardId))]
        public virtual OnboardingWizard OnboardingWizard { get; set; } = null!;

        public virtual Tenant Tenant { get; set; } = null!;
    }

    /// <summary>
    /// Achievement definitions for gamification
    /// </summary>
    public class OnboardingAchievement
    {
        public const string BRONZE = "Bronze";
        public const string SILVER = "Silver";
        public const string GOLD = "Gold";
        public const string PLATINUM = "Platinum";
        public const string DIAMOND = "Diamond";

        /// <summary>
        /// Calculate achievement level based on score
        /// </summary>
        public static string GetAchievementLevel(int totalScore, int maxScore)
        {
            var percentage = (double)totalScore / maxScore * 100;
            return percentage switch
            {
                >= 95 => DIAMOND,
                >= 85 => PLATINUM,
                >= 75 => GOLD,
                >= 60 => SILVER,
                >= 40 => BRONZE,
                _ => ""
            };
        }

        /// <summary>
        /// Calculate star rating (1-5 stars)
        /// </summary>
        public static int GetStarRating(int completionPercent, int qualityScore)
        {
            var average = (completionPercent + qualityScore) / 2.0;
            return average switch
            {
                >= 95 => 5,
                >= 80 => 4,
                >= 65 => 3,
                >= 50 => 2,
                >= 30 => 1,
                _ => 0
            };
        }

        /// <summary>
        /// Points breakdown by step (base points)
        /// </summary>
        public static readonly Dictionary<int, int> StepBasePoints = new()
        {
            { 1, 150 },  // Organization Identity - Most critical
            { 2, 100 },  // Assurance Objective
            { 3, 120 },  // Regulatory Applicability - Important
            { 4, 110 },  // Scope Definition
            { 5, 130 },  // Data & Risk Profile - High value
            { 6, 80 },   // Technology Landscape
            { 7, 100 },  // Control Ownership
            { 8, 120 },  // Teams & Roles - Important for setup
            { 9, 90 },   // Workflow & Cadence
            { 10, 85 },  // Evidence Standards
            { 11, 100 }, // Baseline & Overlays
            { 12, 115 }  // Go-Live Metrics - Final critical step
        };

        /// <summary>
        /// Bonus multipliers
        /// </summary>
        public const int SPEED_BONUS_MULTIPLIER = 10;      // 10 points per minute under time
        public const int THOROUGHNESS_BONUS = 25;          // 25 points for all optional fields
        public const int QUALITY_BONUS = 30;               // 30 points for first-try validation
        public const int PERFECT_SCORE_BONUS = 50;         // 50 points for 100% completion
    }
}
