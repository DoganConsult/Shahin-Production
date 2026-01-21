using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities
{
    public class Control : BaseEntity
    {
        /// <summary>
        /// Workspace this control belongs to (Market/BU scope).
        /// Null = applies to all workspaces in the tenant.
        /// </summary>
        public Guid? WorkspaceId { get; set; }

        [ForeignKey("WorkspaceId")]
        public virtual Workspace? Workspace { get; set; }

        // =====================================================================
        // CONTROL IDENTIFICATION
        // =====================================================================

        /// <summary>
        /// Legacy control ID (deprecated, use BusinessCode instead)
        /// </summary>
        public string ControlId { get; set; } = string.Empty;

        /// <summary>
        /// Legacy control code (deprecated, use BusinessCode instead)
        /// </summary>
        public string ControlCode { get; set; } = string.Empty;

        // =====================================================================
        // SOURCE FRAMEWORK CODES (for imported baseline controls)
        // =====================================================================

        /// <summary>
        /// Source framework code when imported from baseline (e.g., "NCA-ECC", "ISO27001").
        /// Null for custom tenant-created controls.
        /// </summary>
        public string? SourceFrameworkCode { get; set; }

        /// <summary>
        /// Original control code from the source framework (e.g., "ECC-1.2.3", "A.5.1").
        /// Preserved exactly as defined in the external framework.
        /// </summary>
        public string? SourceControlCode { get; set; }

        /// <summary>
        /// Source control title from the framework (for audit trail).
        /// </summary>
        public string? SourceControlTitle { get; set; }

        /// <summary>
        /// Display code: Shows SourceControlCode if imported, otherwise BusinessCode
        /// </summary>
        [NotMapped]
        public string DisplayCode => !string.IsNullOrEmpty(SourceControlCode) 
            ? SourceControlCode 
            : BusinessCode ?? ControlCode;

        /// <summary>
        /// Indicates if this control was imported from an external framework
        /// </summary>
        [NotMapped]
        public bool IsImportedControl => !string.IsNullOrEmpty(SourceFrameworkCode);

        // =====================================================================
        // CONTROL PROPERTIES
        // =====================================================================

        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // Preventive, Detective, Corrective
        public string Frequency { get; set; } = string.Empty; // Daily, Weekly, Monthly, Quarterly, Annual
        public string Owner { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public int EffectivenessScore { get; set; }
        public int Effectiveness { get; set; }
        public DateTime? LastTestDate { get; set; }
        public DateTime? NextTestDate { get; set; }

        // Navigation properties
        public Guid? RiskId { get; set; }
        public virtual Risk? Risk { get; set; }
        public virtual ICollection<Assessment> Assessments { get; set; } = new List<Assessment>();
        public virtual ICollection<Evidence> Evidences { get; set; } = new List<Evidence>();
    }
}