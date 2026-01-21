using System;
using System.ComponentModel.DataAnnotations;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// Tenant-specific workflow configuration.
    /// Tracks which workflows are enabled for a tenant and their custom settings.
    /// Layer 2: Tenant Context
    /// </summary>
    public class TenantWorkflowConfig : BaseEntity
    {
        public Guid TenantId { get; set; }

        [Required]
        [MaxLength(50)]
        public string WorkflowCode { get; set; } = string.Empty;

        [MaxLength(200)]
        public string WorkflowName { get; set; } = string.Empty;

        public bool IsEnabled { get; set; } = true;

        public DateTime? ActivatedAt { get; set; }

        [MaxLength(100)]
        public string? ActivatedBy { get; set; }

        public DateTime? DeactivatedAt { get; set; }

        [MaxLength(100)]
        public string? DeactivatedBy { get; set; }

        /// <summary>
        /// SLA multiplier for this tenant (e.g., 1.5 = 50% more time)
        /// </summary>
        public decimal SlaMultiplier { get; set; } = 1.0m;

        /// <summary>
        /// Custom configuration JSON for workflow overrides
        /// </summary>
        public string? CustomConfigJson { get; set; }

        /// <summary>
        /// Notification settings override JSON
        /// </summary>
        public string? NotificationOverridesJson { get; set; }

        // Navigation
        public virtual Tenant? Tenant { get; set; }
    }
}
