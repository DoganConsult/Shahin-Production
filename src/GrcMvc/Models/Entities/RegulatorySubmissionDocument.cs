using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Documents uploaded as part of regulatory submissions.
    /// </summary>
    public class RegulatorySubmissionDocument : BaseEntity
    {
        [Required]
        public Guid SubmissionId { get; set; }

        [Required]
        [MaxLength(500)]
        public string FileName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? OriginalFileName { get; set; }

        [Required]
        [MaxLength(100)]
        public string ContentType { get; set; } = string.Empty;

        public long FileSize { get; set; }

        [Required]
        [MaxLength(100)]
        public string DocumentType { get; set; } = string.Empty; // Assessment, Evidence, Report, Certificate, Supporting

        public string? Description { get; set; }

        /// <summary>
        /// Path to the stored file (blob storage or local)
        /// </summary>
        [MaxLength(1000)]
        public string? StoragePath { get; set; }

        /// <summary>
        /// External document ID returned by the portal after upload
        /// </summary>
        [MaxLength(200)]
        public string? ExternalDocumentId { get; set; }

        public bool UploadedToPortal { get; set; } = false;
        public DateTime? UploadedToPortalAt { get; set; }

        public string? UploadedById { get; set; }
        public string? UploadedByName { get; set; }

        // Navigation property
        [ForeignKey("SubmissionId")]
        public virtual RegulatorySubmission? Submission { get; set; }
    }
}
