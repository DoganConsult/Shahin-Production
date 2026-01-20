using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Onboarding Section (A-L) - Parent of questions
    /// </summary>
    [Index(nameof(Code), IsUnique = true)]
    [Index(nameof(SortOrder))]
    public class OnboardingSection : BaseEntity
    {
        /// <summary>
        /// Section code: A, B, C... L
        /// </summary>
        [Required]
        [MaxLength(5)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Step number (1-12)
        /// </summary>
        public int StepNumber { get; set; }

        /// <summary>
        /// Section name in English
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string NameEn { get; set; } = string.Empty;

        /// <summary>
        /// Section name in Arabic
        /// </summary>
        [MaxLength(200)]
        public string NameAr { get; set; } = string.Empty;

        /// <summary>
        /// Section description in English
        /// </summary>
        [MaxLength(1000)]
        public string DescriptionEn { get; set; } = string.Empty;

        /// <summary>
        /// Section description in Arabic
        /// </summary>
        [MaxLength(1000)]
        public string DescriptionAr { get; set; } = string.Empty;

        /// <summary>
        /// Icon class (FontAwesome)
        /// </summary>
        [MaxLength(50)]
        public string Icon { get; set; } = "fa-question-circle";

        /// <summary>
        /// Sort order for display
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Is this section required to complete onboarding?
        /// </summary>
        public bool IsRequired { get; set; } = true;

        /// <summary>
        /// Is section active?
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Navigation
        public virtual ICollection<OnboardingQuestion> Questions { get; set; } = new List<OnboardingQuestion>();
    }

    /// <summary>
    /// Question types for onboarding
    /// </summary>
    public enum OnboardingQuestionType
    {
        Text = 1,
        TextArea = 2,
        Number = 3,
        Date = 4,
        SingleSelect = 5,
        MultiSelect = 6,
        Boolean = 7,
        Email = 8,
        Phone = 9,
        Url = 10,
        JsonArray = 11,
        JsonObject = 12,
        File = 13,
        Currency = 14,
        Percentage = 15
    }

    /// <summary>
    /// Onboarding Question - Configurable question catalog
    /// </summary>
    [Index(nameof(SectionId), nameof(SortOrder))]
    [Index(nameof(Code), IsUnique = true)]
    [Index(nameof(FieldName), IsUnique = true)]
    [Index(nameof(IsRequired))]
    [Index(nameof(IsActive))]
    public class OnboardingQuestion : BaseEntity
    {
        /// <summary>
        /// Parent section
        /// </summary>
        public Guid SectionId { get; set; }

        /// <summary>
        /// Question code: A1, A2, B14, etc.
        /// </summary>
        [Required]
        [MaxLength(10)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Field name for mapping to entity property
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string FieldName { get; set; } = string.Empty;

        /// <summary>
        /// Question text in English
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string QuestionEn { get; set; } = string.Empty;

        /// <summary>
        /// Question text in Arabic
        /// </summary>
        [MaxLength(500)]
        public string QuestionAr { get; set; } = string.Empty;

        /// <summary>
        /// Help text / description in English
        /// </summary>
        [MaxLength(1000)]
        public string HelpTextEn { get; set; } = string.Empty;

        /// <summary>
        /// Help text / description in Arabic
        /// </summary>
        [MaxLength(1000)]
        public string HelpTextAr { get; set; } = string.Empty;

        /// <summary>
        /// Placeholder text in English
        /// </summary>
        [MaxLength(200)]
        public string PlaceholderEn { get; set; } = string.Empty;

        /// <summary>
        /// Placeholder text in Arabic
        /// </summary>
        [MaxLength(200)]
        public string PlaceholderAr { get; set; } = string.Empty;

        /// <summary>
        /// Question type
        /// </summary>
        public OnboardingQuestionType QuestionType { get; set; } = OnboardingQuestionType.Text;

        /// <summary>
        /// Options for select types (JSON array of {value, labelEn, labelAr})
        /// </summary>
        public string OptionsJson { get; set; } = "[]";

        /// <summary>
        /// Default value
        /// </summary>
        [MaxLength(1000)]
        public string DefaultValue { get; set; } = string.Empty;

        /// <summary>
        /// Validation rules (JSON: min, max, regex, etc.)
        /// </summary>
        public string ValidationRulesJson { get; set; } = "{}";

        /// <summary>
        /// Is this question required?
        /// </summary>
        public bool IsRequired { get; set; } = false;

        /// <summary>
        /// Conditional display: show only if condition met (JSON)
        /// </summary>
        public string ConditionalDisplayJson { get; set; } = "{}";

        /// <summary>
        /// Sort order within section
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Is question active?
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Tags for filtering/grouping (JSON array)
        /// </summary>
        public string TagsJson { get; set; } = "[]";

        // Navigation
        [ForeignKey(nameof(SectionId))]
        public virtual OnboardingSection Section { get; set; } = null!;

        public virtual ICollection<OnboardingAnswer> Answers { get; set; } = new List<OnboardingAnswer>();
    }

    /// <summary>
    /// Tenant's answer to an onboarding question
    /// </summary>
    [Index(nameof(TenantId), nameof(QuestionId), IsUnique = true)]
    [Index(nameof(TenantId))]
    [Index(nameof(QuestionId))]
    [Index(nameof(AnsweredAt))]
    public class OnboardingAnswer : BaseEntity
    {
        /// <summary>
        /// Tenant who answered
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// Question being answered
        /// </summary>
        public Guid QuestionId { get; set; }

        /// <summary>
        /// Answer value (stored as string, parsed based on question type)
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// For JSON/complex answers
        /// </summary>
        public string ValueJson { get; set; } = "{}";

        /// <summary>
        /// When was this answered
        /// </summary>
        public DateTime? AnsweredAt { get; set; }

        /// <summary>
        /// Who answered (user ID)
        /// </summary>
        [MaxLength(100)]
        public string AnsweredByUserId { get; set; } = string.Empty;

        /// <summary>
        /// Is this answer validated/verified?
        /// </summary>
        public bool IsValidated { get; set; } = false;

        /// <summary>
        /// Validation notes
        /// </summary>
        [MaxLength(500)]
        public string ValidationNotes { get; set; } = string.Empty;

        // Navigation
        [ForeignKey(nameof(TenantId))]
        public virtual Tenant Tenant { get; set; } = null!;

        [ForeignKey(nameof(QuestionId))]
        public virtual OnboardingQuestion Question { get; set; } = null!;
    }

    /// <summary>
    /// Tracks tenant's progress through onboarding sections
    /// </summary>
    [Index(nameof(TenantId), nameof(SectionId), IsUnique = true)]
    [Index(nameof(TenantId))]
    [Index(nameof(Status))]
    public class OnboardingSectionProgress : BaseEntity
    {
        public Guid TenantId { get; set; }
        public Guid SectionId { get; set; }

        /// <summary>
        /// Status: NotStarted, InProgress, Completed, Skipped
        /// </summary>
        [MaxLength(50)]
        public string Status { get; set; } = "NotStarted";

        /// <summary>
        /// Progress percentage (0-100)
        /// </summary>
        public int ProgressPercent { get; set; } = 0;

        /// <summary>
        /// Number of questions answered
        /// </summary>
        public int QuestionsAnswered { get; set; } = 0;

        /// <summary>
        /// Total questions in section
        /// </summary>
        public int TotalQuestions { get; set; } = 0;

        /// <summary>
        /// When section was started
        /// </summary>
        public DateTime? StartedAt { get; set; }

        /// <summary>
        /// When section was completed
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        // Navigation
        [ForeignKey(nameof(TenantId))]
        public virtual Tenant Tenant { get; set; } = null!;

        [ForeignKey(nameof(SectionId))]
        public virtual OnboardingSection Section { get; set; } = null!;
    }
}
