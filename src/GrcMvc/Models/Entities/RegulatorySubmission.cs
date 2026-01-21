using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Tracks regulatory submissions to government portals.
    /// </summary>
    public class RegulatorySubmission : BaseEntity
    {
        [Required]
        public Guid ConnectionId { get; set; }

        [Required]
        [MaxLength(50)]
        public string PortalType { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string SubmissionType { get; set; } = string.Empty; // Assessment, Report, Incident, BreachNotification, Registration

        /// <summary>
        /// External submission ID returned by the portal
        /// </summary>
        [MaxLength(200)]
        public string? ExternalSubmissionId { get; set; }

        /// <summary>
        /// Confirmation number from the portal
        /// </summary>
        [MaxLength(200)]
        public string? ConfirmationNumber { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Draft"; // Draft, Submitted, Accepted, Rejected, Pending, Failed, Cancelled

        public string? StatusDescription { get; set; }

        public DateTime? SubmittedAt { get; set; }
        public string? SubmittedById { get; set; }
        public string? SubmittedByName { get; set; }

        public DateTime? ReportingPeriodStart { get; set; }
        public DateTime? ReportingPeriodEnd { get; set; }

        /// <summary>
        /// Associated assessment (for assessment-based submissions)
        /// </summary>
        public Guid? AssessmentId { get; set; }

        /// <summary>
        /// Associated incident (for incident notifications)
        /// </summary>
        public Guid? IncidentId { get; set; }

        /// <summary>
        /// Deadline for this submission
        /// </summary>
        public DateTime? Deadline { get; set; }

        /// <summary>
        /// JSON payload sent to the portal
        /// </summary>
        public string? RequestPayloadJson { get; set; }

        /// <summary>
        /// JSON response received from the portal
        /// </summary>
        public string? ResponsePayloadJson { get; set; }

        public string? Notes { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Number of retry attempts
        /// </summary>
        public int RetryCount { get; set; } = 0;

        public DateTime? LastRetryAt { get; set; }

        // Navigation properties
        [ForeignKey("ConnectionId")]
        public virtual RegulatoryPortalConnection? Connection { get; set; }

        public virtual ICollection<RegulatorySubmissionDocument> Documents { get; set; } = new List<RegulatorySubmissionDocument>();
        public virtual ICollection<RegulatorySubmissionEvidence> EvidenceLinks { get; set; } = new List<RegulatorySubmissionEvidence>();
    }

    /// <summary>
    /// Links evidence items to regulatory submissions.
    /// </summary>
    public class RegulatorySubmissionEvidence : BaseEntity
    {
        public Guid SubmissionId { get; set; }
        public Guid EvidenceId { get; set; }

        [ForeignKey("SubmissionId")]
        public virtual RegulatorySubmission? Submission { get; set; }
    }
}
