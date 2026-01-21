using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities
{
    public class Evidence : BaseEntity
    {
        /// <summary>
        /// Workspace this evidence belongs to (Market/BU scope).
        /// Null = applies to all workspaces in the tenant.
        /// </summary>
        public Guid? WorkspaceId { get; set; }

        [ForeignKey("WorkspaceId")]
        public virtual Workspace? Workspace { get; set; }

        // =====================================================================
        // EVIDENCE IDENTIFICATION (Serial Code System)
        // =====================================================================

        /// <summary>
        /// Stable evidence reference code (use BusinessCode from BaseEntity).
        /// Format: {TENANTCODE}-EVD-{YYYY}-{SEQUENCE}
        /// Example: ACME-EVD-2026-000778
        /// </summary>
        public string EvidenceNumber { get; set; } = string.Empty;

        // =====================================================================
        // EVIDENCE METADATA
        // =====================================================================

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // Document, Screenshot, Log, Report, etc.
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string MimeType { get; set; } = string.Empty;
        public DateTime CollectionDate { get; set; }
        public string CollectedBy { get; set; } = string.Empty;
        public string VerificationStatus { get; set; } = "Pending"; // Pending, Verified, Rejected
        public string VerifiedBy { get; set; } = string.Empty;
        public DateTime? VerificationDate { get; set; }
        public string Comments { get; set; } = string.Empty;

        // =====================================================================
        // CHAIN OF CUSTODY (Critical for audit trail)
        // =====================================================================

        /// <summary>
        /// SHA-256 hash of the file content for integrity verification
        /// </summary>
        public string? FileHash { get; set; }

        /// <summary>
        /// File version (increments when file is replaced)
        /// </summary>
        public int FileVersion { get; set; } = 1;

        /// <summary>
        /// Original upload timestamp (never changes)
        /// </summary>
        public DateTime? OriginalUploadDate { get; set; }

        /// <summary>
        /// Original uploader (never changes)
        /// </summary>
        public string? OriginalUploader { get; set; }

        /// <summary>
        /// Retention period end date (when evidence can be archived/deleted)
        /// </summary>
        public DateTime? RetentionEndDate { get; set; }

        /// <summary>
        /// Evidence period start (the timeframe this evidence covers)
        /// </summary>
        public DateTime? EvidencePeriodStart { get; set; }

        /// <summary>
        /// Evidence period end (the timeframe this evidence covers)
        /// </summary>
        public DateTime? EvidencePeriodEnd { get; set; }

        /// <summary>
        /// Source system from which evidence was collected
        /// </summary>
        public string? SourceSystem { get; set; }

        // Navigation properties
        public Guid? AssessmentId { get; set; }
        public virtual Assessment? Assessment { get; set; }
        public Guid? AuditId { get; set; }
        public virtual Audit? Audit { get; set; }
        public Guid? ControlId { get; set; }
        public virtual Control? Control { get; set; }

        /// <summary>
        /// Link to specific assessment requirement (for per-requirement evidence)
        /// </summary>
        public Guid? AssessmentRequirementId { get; set; }
        public virtual AssessmentRequirement? AssessmentRequirement { get; set; }

        // =====================================================================
        // SERVICE COMPATIBILITY PROPERTIES (EvidenceConfidenceService)
        // =====================================================================

        /// <summary>
        /// Evidence type code for scoring criteria
        /// </summary>
        public string? EvidenceTypeCode { get; set; }

        /// <summary>
        /// Due date for evidence submission
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// When evidence was submitted
        /// </summary>
        public DateTime? SubmittedAt { get; set; }

        /// <summary>
        /// Whether evidence was auto-collected
        /// </summary>
        public bool IsAutoCollected { get; set; }

        /// <summary>
        /// Whether evidence was hybrid-collected
        /// </summary>
        public bool IsHybridCollection { get; set; }

        /// <summary>
        /// Whether evidence has digital signature
        /// </summary>
        public bool HasDigitalSignature { get; set; }

        /// <summary>
        /// File size in bytes (alias for FileSize)
        /// </summary>
        [NotMapped]
        public long FileSizeBytes
        {
            get => FileSize;
            set => FileSize = value;
        }

        /// <summary>
        /// Evidence validity end date
        /// </summary>
        public DateTime? ValidUntil { get; set; }

        /// <summary>
        /// Reviewer comments
        /// </summary>
        public string? ReviewerComments { get; set; }

        /// <summary>
        /// When evidence was verified (alias for VerificationDate)
        /// </summary>
        [NotMapped]
        public DateTime? VerifiedAt
        {
            get => VerificationDate;
            set => VerificationDate = value;
        }

        /// <summary>
        /// File type (alias for MimeType)
        /// </summary>
        [NotMapped]
        public string? FileType
        {
            get => MimeType;
            set => MimeType = value ?? string.Empty;
        }

        /// <summary>
        /// Framework code for relevance scoring
        /// </summary>
        public string? FrameworkCode { get; set; }
    }
}