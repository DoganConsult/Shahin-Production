using System;
using System.ComponentModel.DataAnnotations;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Audit log for all regulatory integration activities.
    /// </summary>
    public class RegulatoryAuditLog : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string PortalType { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Action { get; set; } = string.Empty; // ConnectionTest, Submit, Retrieve, Upload, Validate, Error

        /// <summary>
        /// Associated submission ID (if applicable)
        /// </summary>
        public Guid? SubmissionId { get; set; }

        /// <summary>
        /// External submission ID from portal
        /// </summary>
        [MaxLength(200)]
        public string? ExternalSubmissionId { get; set; }

        /// <summary>
        /// Connection ID used
        /// </summary>
        public Guid? ConnectionId { get; set; }

        public bool Success { get; set; }

        [MaxLength(500)]
        public string? Message { get; set; }

        /// <summary>
        /// Detailed error information
        /// </summary>
        public string? ErrorDetails { get; set; }

        /// <summary>
        /// Request payload (sanitized, no credentials)
        /// </summary>
        public string? RequestSummary { get; set; }

        /// <summary>
        /// Response payload summary
        /// </summary>
        public string? ResponseSummary { get; set; }

        /// <summary>
        /// HTTP status code (if applicable)
        /// </summary>
        public int? HttpStatusCode { get; set; }

        /// <summary>
        /// Response time in milliseconds
        /// </summary>
        public int? ResponseTimeMs { get; set; }

        /// <summary>
        /// User who performed the action
        /// </summary>
        [Required]
        public string PerformedById { get; set; } = string.Empty;

        [MaxLength(200)]
        public string PerformedByName { get; set; } = string.Empty;

        /// <summary>
        /// IP address of the request
        /// </summary>
        [MaxLength(50)]
        public string? IpAddress { get; set; }

        /// <summary>
        /// Timestamp of the action (uses CreatedDate from BaseEntity)
        /// </summary>
        public DateTime ActionTimestamp { get; set; } = DateTime.UtcNow;
    }
}
