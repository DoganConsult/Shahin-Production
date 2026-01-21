using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using GrcMvc.Configuration;

namespace GrcMvc.Models.Entities
{
    public class Risk : BaseEntity
    {
        /// <summary>
        /// Workspace this risk belongs to (Market/BU scope).
        /// Null = applies to all workspaces in the tenant.
        /// </summary>
        public Guid? WorkspaceId { get; set; }

        [ForeignKey("WorkspaceId")]
        public virtual Workspace? Workspace { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int Likelihood { get; set; }
        public int Probability => Likelihood; // Alias for compatibility
        public int Impact { get; set; }
        public int InherentRisk { get; set; }
        public int ResidualRisk { get; set; }
        public int RiskScore => Likelihood * Impact;
        
        /// <summary>
        /// Risk level computed from score using centralized thresholds
        /// </summary>
        public string RiskLevel => RiskScoringConfiguration.GetRiskLevel(RiskScore);
        
        public string Status { get; set; } = "Active";
        public string Owner { get; set; } = string.Empty;
        public DateTime? ReviewDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string MitigationStrategy { get; set; } = string.Empty;

        // New properties for UI compatibility (optional, nullable)
        public string? Title { get; set; } // Alias for Name, or separate field
        public string? RiskNumber { get; set; } // Auto-generated risk number
        public DateTime? IdentifiedDate { get; set; } // When risk was identified
        public string? ResponsibleParty { get; set; } // Additional owner field
        public string? ConsequenceArea { get; set; } // Impact description

        // Computed property: Use Title if available, fallback to Name
        public string DisplayTitle => !string.IsNullOrEmpty(Title) ? Title : Name;

        // Navigation properties
        public virtual ICollection<Control> Controls { get; set; } = new List<Control>();
        public virtual ICollection<Assessment> Assessments { get; set; } = new List<Assessment>();
        public virtual ICollection<RiskControlMapping> RiskControlMappings { get; set; } = new List<RiskControlMapping>();
    }
}