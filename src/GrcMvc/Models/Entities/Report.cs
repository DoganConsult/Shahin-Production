using System;
using System.Collections.Generic;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Represents a generated GRC report (Risk, Compliance, Audit, Control, Executive).
    /// Reports can be in Draft, Generated, Delivered, or Archived status.
    /// </summary>
    public class Report : BaseEntity
    {
        // TenantId is required for Reports (non-nullable, overrides BaseEntity's nullable TenantId)
        public Guid TenantId { get; set; }
        
        /// <summary>
        /// Optional workspace scope for the report
        /// </summary>
        public Guid? WorkspaceId { get; set; }
        
        /// <summary>
        /// Auto-generated report number (e.g., RPT-2024-001)
        /// </summary>
        public string ReportNumber { get; set; } = string.Empty;
        
        /// <summary>
        /// Report title
        /// </summary>
        public string Title { get; set; } = string.Empty;
        
        /// <summary>
        /// Report description
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Report type: Risk, Compliance, Audit, Control, Executive, Custom
        /// </summary>
        public string Type { get; set; } = string.Empty;
        
        /// <summary>
        /// Report status: Draft, Generated, Delivered, Archived
        /// </summary>
        public string Status { get; set; } = "Draft";
        
        /// <summary>
        /// Report scope description
        /// </summary>
        public string Scope { get; set; } = string.Empty;
        
        /// <summary>
        /// Report period start date
        /// </summary>
        public DateTime ReportPeriodStart { get; set; }
        
        /// <summary>
        /// Report period end date
        /// </summary>
        public DateTime ReportPeriodEnd { get; set; }
        
        /// <summary>
        /// Executive summary (HTML or Markdown)
        /// </summary>
        public string ExecutiveSummary { get; set; } = string.Empty;
        
        /// <summary>
        /// Key findings (HTML or Markdown)
        /// </summary>
        public string KeyFindings { get; set; } = string.Empty;
        
        /// <summary>
        /// Recommendations (HTML or Markdown)
        /// </summary>
        public string Recommendations { get; set; } = string.Empty;
        
        /// <summary>
        /// Total number of findings in the report
        /// </summary>
        public int TotalFindingsCount { get; set; }
        
        /// <summary>
        /// Number of critical findings
        /// </summary>
        public int CriticalFindingsCount { get; set; }
        
        /// <summary>
        /// User who generated the report
        /// </summary>
        public string GeneratedBy { get; set; } = string.Empty;
        
        /// <summary>
        /// When the report was generated
        /// </summary>
        public DateTime? GeneratedDate { get; set; }
        
        /// <summary>
        /// Who the report was delivered to
        /// </summary>
        public string? DeliveredTo { get; set; }
        
        /// <summary>
        /// When the report was delivered
        /// </summary>
        public DateTime? DeliveryDate { get; set; }
        
        /// <summary>
        /// URL/path to the generated report file (PDF/Excel)
        /// </summary>
        public string? FileUrl { get; set; }

        /// <summary>
        /// Internal storage path for the file
        /// </summary>
        public string? FilePath { get; set; }

        /// <summary>
        /// Original file name
        /// </summary>
        public string? FileName { get; set; }

        /// <summary>
        /// MIME content type (application/pdf, application/vnd.ms-excel, etc.)
        /// </summary>
        public string? ContentType { get; set; }

        /// <summary>
        /// File size in bytes
        /// </summary>
        public long? FileSize { get; set; }

        /// <summary>
        /// SHA256 hash of the file for integrity verification
        /// </summary>
        public string? FileHash { get; set; }
        
        /// <summary>
        /// Number of pages in the report
        /// </summary>
        public int PageCount { get; set; }
        
        /// <summary>
        /// JSON array of entity IDs included in the report (risks, audits, assessments, etc.)
        /// </summary>
        public string IncludedEntitiesJson { get; set; } = "[]";
        
        /// <summary>
        /// Additional report metadata as JSON
        /// </summary>
        public string MetadataJson { get; set; } = "{}";
        
        /// <summary>
        /// Correlation ID for audit trail
        /// </summary>
        public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
        
        // Navigation properties
        public virtual Tenant Tenant { get; set; } = null!;
        public virtual Workspace? Workspace { get; set; }
    }
}
