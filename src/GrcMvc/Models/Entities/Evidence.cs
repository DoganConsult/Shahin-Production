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

        public string EvidenceNumber { get; set; } = string.Empty;
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

        // Navigation properties
        public Guid? AssessmentId { get; set; }
        public virtual Assessment? Assessment { get; set; }
        public Guid? AuditId { get; set; }
        public virtual Audit? Audit { get; set; }
        public Guid? ControlId { get; set; }
        public virtual Control? Control { get; set; }
    }
}