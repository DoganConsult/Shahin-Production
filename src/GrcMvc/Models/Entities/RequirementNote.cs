using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Notes/comments attached to individual assessment requirements.
    /// Supports audit trail of observations, actions, and questions during assessment execution.
    /// </summary>
    public class RequirementNote : BaseEntity
    {
        /// <summary>
        /// The assessment requirement this note belongs to
        /// </summary>
        public Guid AssessmentRequirementId { get; set; }

        /// <summary>
        /// The note content/text
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Type of note: General, Observation, Action, Question
        /// </summary>
        public string NoteType { get; set; } = "General";

        /// <summary>
        /// Whether this note is internal (not visible to auditors) or external
        /// </summary>
        public bool IsInternal { get; set; } = true;

        // Navigation property
        [ForeignKey("AssessmentRequirementId")]
        public virtual AssessmentRequirement AssessmentRequirement { get; set; } = null!;
    }
}
