using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities
{
    public class Policy : BaseEntity
    {
        /// <summary>
        /// Workspace this policy belongs to (Market/BU scope).
        /// Null = applies to all workspaces in the tenant.
        /// </summary>
        public Guid? WorkspaceId { get; set; }

        [ForeignKey("WorkspaceId")]
        public virtual Workspace? Workspace { get; set; }

        // =====================================================================
        // DOCUMENT IDENTIFICATION (Serial Code System)
        // =====================================================================

        /// <summary>
        /// Stable document number that NEVER changes (use BusinessCode from BaseEntity).
        /// Format: {TENANTCODE}-POL-{YYYY}-{SEQUENCE}
        /// Example: ACME-POL-2026-000012
        /// </summary>
        public string PolicyNumber { get; set; } = string.Empty;

        /// <summary>
        /// Legacy code field (deprecated, use BusinessCode instead)
        /// </summary>
        public string PolicyCode { get; set; } = string.Empty;

        // =====================================================================
        // DOCUMENT VERSIONING (Critical for policy lifecycle)
        // =====================================================================

        /// <summary>
        /// Major version number (increments on significant changes)
        /// </summary>
        public int VersionMajor { get; set; } = 1;

        /// <summary>
        /// Minor version number (increments on minor edits)
        /// </summary>
        public int VersionMinor { get; set; } = 0;

        /// <summary>
        /// Full version string (computed: "1.0", "2.1", etc.)
        /// </summary>
        [NotMapped]
        public string VersionDisplay => $"{VersionMajor}.{VersionMinor}";

        /// <summary>
        /// Legacy version string (for backward compatibility)
        /// </summary>
        public string Version { get; set; } = "1.0";

        /// <summary>
        /// Reference to the document this version supersedes (if any).
        /// Format: Previous document's BusinessCode
        /// </summary>
        public string? SupersedesDocumentCode { get; set; }

        /// <summary>
        /// Version history notes (what changed in this version)
        /// </summary>
        public string? VersionNotes { get; set; }

        // =====================================================================
        // DOCUMENT METADATA
        // =====================================================================

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// Document status lifecycle: Draft → Review → Approved → Published → Retired
        /// </summary>
        public string Status { get; set; } = "Draft";
        public bool IsActive { get; set; } = true;
        public string Owner { get; set; } = string.Empty;
        public string ApprovedBy { get; set; } = string.Empty;
        public DateTime? ApprovalDate { get; set; }
        public DateTime NextReviewDate { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Requirements { get; set; } = string.Empty;
        public string DocumentPath { get; set; } = string.Empty;

        // Navigation properties
        public virtual ICollection<PolicyViolation> Violations { get; set; } = new List<PolicyViolation>();
    }
}