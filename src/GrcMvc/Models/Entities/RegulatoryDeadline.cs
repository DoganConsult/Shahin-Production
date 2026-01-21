using System;
using System.ComponentModel.DataAnnotations;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Tracks regulatory deadlines for various portal submissions.
    /// </summary>
    public class RegulatoryDeadline : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string PortalType { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string DeadlineType { get; set; } = string.Empty; // Annual, Quarterly, Monthly, AdHoc, Incident

        [Required]
        [MaxLength(500)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? TitleAr { get; set; }

        public string? Description { get; set; }
        public string? DescriptionAr { get; set; }

        [Required]
        public DateTime DeadlineDate { get; set; }

        public bool IsMandatory { get; set; } = true;

        [MaxLength(20)]
        public string Priority { get; set; } = "Normal"; // Low, Normal, High, Critical

        /// <summary>
        /// User ID of assigned person
        /// </summary>
        public string? AssignedToId { get; set; }

        /// <summary>
        /// Display name of assigned person
        /// </summary>
        [MaxLength(200)]
        public string? AssignedToName { get; set; }

        /// <summary>
        /// Status of the deadline
        /// </summary>
        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed, Overdue, Cancelled

        /// <summary>
        /// Associated submission if already created
        /// </summary>
        public Guid? SubmissionId { get; set; }

        /// <summary>
        /// Notification sent flag
        /// </summary>
        public bool NotificationSent { get; set; } = false;
        public DateTime? NotificationSentAt { get; set; }

        /// <summary>
        /// Reminder sent flag (7 days before)
        /// </summary>
        public bool ReminderSent { get; set; } = false;
        public DateTime? ReminderSentAt { get; set; }

        /// <summary>
        /// Year for annual/quarterly deadlines
        /// </summary>
        public int? Year { get; set; }

        /// <summary>
        /// Quarter for quarterly deadlines (1-4)
        /// </summary>
        public int? Quarter { get; set; }
    }
}
