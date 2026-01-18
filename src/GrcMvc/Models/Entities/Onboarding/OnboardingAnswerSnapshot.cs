using System;
using System.ComponentModel.DataAnnotations;

namespace GrcMvc.Models.Entities.Onboarding
{
    /// <summary>
    /// Layer 14: Immutable versioned snapshot of onboarding answers.
    /// Stored for audit replay and compliance traceability.
    /// Each snapshot is immutable once created.
    /// </summary>
    public class OnboardingAnswerSnapshot : BaseEntity
    {
        /// <summary>
        /// Reference to the OnboardingWizard this snapshot belongs to
        /// </summary>
        public Guid OnboardingWizardId { get; set; }

        /// <summary>
        /// Version number (auto-incrementing per wizard)
        /// </summary>
        public int Version { get; set; } = 1;

        /// <summary>
        /// Which step was completed to trigger this snapshot (1-12)
        /// </summary>
        public int CompletedStep { get; set; }

        /// <summary>
        /// Section code (A-L)
        /// </summary>
        [MaxLength(10)]
        public string SectionCode { get; set; } = string.Empty;

        /// <summary>
        /// Complete JSON of all answers at this point in time (immutable)
        /// </summary>
        public string AnswersJson { get; set; } = "{}";

        /// <summary>
        /// Hash of the answers JSON for integrity verification
        /// </summary>
        [MaxLength(128)]
        public string AnswersHash { get; set; } = string.Empty;

        /// <summary>
        /// Who created this snapshot
        /// </summary>
        [MaxLength(100)]
        public string CreatedByUserId { get; set; } = string.Empty;

        /// <summary>
        /// IP address at time of snapshot
        /// </summary>
        [MaxLength(50)]
        public string? IpAddress { get; set; }

        /// <summary>
        /// User agent at time of snapshot
        /// </summary>
        [MaxLength(500)]
        public string? UserAgent { get; set; }

        /// <summary>
        /// Timestamp when this snapshot was locked (immutable after this)
        /// </summary>
        public DateTime SnapshotAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Is this the final/completion snapshot?
        /// </summary>
        public bool IsFinalSnapshot { get; set; } = false;

        // Navigation
        public virtual OnboardingWizard OnboardingWizard { get; set; } = null!;
    }
}
