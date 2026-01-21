using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities;

/// <summary>
/// Security/Operational Incident Entity
/// Tracks: Detection → Response → Containment → Eradication → Recovery → Lessons Learned
/// </summary>
public class Incident : BaseEntity
{
    /// <summary>
    /// Unique incident identifier (auto-generated)
    /// </summary>
    [Required]
    public string IncidentNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Incident title
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Incident title in Arabic
    /// </summary>
    [MaxLength(500)]
    public string? TitleAr { get; set; }
    
    /// <summary>
    /// Detailed description of the incident
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Incident category
    /// </summary>
    [Required]
    public string Category { get; set; } = "Security"; // Security, Operational, Compliance, Privacy, Business Continuity
    
    /// <summary>
    /// Incident type within category
    /// </summary>
    public string Type { get; set; } = "Other"; // DataBreach, Malware, Phishing, Unauthorized Access, SystemOutage, etc.
    
    /// <summary>
    /// Severity level
    /// </summary>
    [Required]
    public string Severity { get; set; } = "Medium"; // Critical, High, Medium, Low
    
    /// <summary>
    /// Priority for response
    /// </summary>
    public string Priority { get; set; } = "Normal"; // Immediate, High, Normal, Low
    
    /// <summary>
    /// Current status
    /// </summary>
    [Required]
    public string Status { get; set; } = "Open"; // Open, Investigating, Contained, Eradicated, Recovered, Closed, False Positive
    
    /// <summary>
    /// Current phase in incident response lifecycle
    /// </summary>
    public string Phase { get; set; } = "Detection"; // Detection, Analysis, Containment, Eradication, Recovery, PostIncident
    
    /// <summary>
    /// Source of incident detection
    /// </summary>
    public string? DetectionSource { get; set; } // SIEM, IDS, User Report, Automated Scan, Third Party, Audit
    
    /// <summary>
    /// When the incident was detected
    /// </summary>
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// When the incident actually occurred (may differ from detection)
    /// </summary>
    public DateTime? OccurredAt { get; set; }
    
    /// <summary>
    /// When containment was achieved
    /// </summary>
    public DateTime? ContainedAt { get; set; }
    
    /// <summary>
    /// When eradication was complete
    /// </summary>
    public DateTime? EradicatedAt { get; set; }
    
    /// <summary>
    /// When recovery was complete
    /// </summary>
    public DateTime? RecoveredAt { get; set; }
    
    /// <summary>
    /// When incident was closed
    /// </summary>
    public DateTime? ClosedAt { get; set; }
    
    /// <summary>
    /// Reporter user ID
    /// </summary>
    public string? ReportedById { get; set; }
    
    /// <summary>
    /// Reporter name
    /// </summary>
    public string? ReportedByName { get; set; }
    
    /// <summary>
    /// Primary incident handler/owner
    /// </summary>
    public string? HandlerId { get; set; }
    
    /// <summary>
    /// Handler name
    /// </summary>
    public string? HandlerName { get; set; }
    
    /// <summary>
    /// Team handling the incident
    /// </summary>
    public string? AssignedTeam { get; set; }
    
    /// <summary>
    /// Affected systems (comma-separated or JSON array)
    /// </summary>
    public string? AffectedSystems { get; set; }
    
    /// <summary>
    /// Affected business units
    /// </summary>
    public string? AffectedBusinessUnits { get; set; }
    
    /// <summary>
    /// Number of users/records affected
    /// </summary>
    public int? AffectedUsersCount { get; set; }
    
    /// <summary>
    /// Number of records affected (for data incidents)
    /// </summary>
    public int? AffectedRecordsCount { get; set; }
    
    /// <summary>
    /// Is personal data affected?
    /// </summary>
    public bool PersonalDataAffected { get; set; }
    
    /// <summary>
    /// Root cause analysis
    /// </summary>
    public string? RootCause { get; set; }
    
    /// <summary>
    /// Containment actions taken
    /// </summary>
    public string? ContainmentActions { get; set; }
    
    /// <summary>
    /// Eradication actions taken
    /// </summary>
    public string? EradicationActions { get; set; }
    
    /// <summary>
    /// Recovery actions taken
    /// </summary>
    public string? RecoveryActions { get; set; }
    
    /// <summary>
    /// Lessons learned
    /// </summary>
    public string? LessonsLearned { get; set; }
    
    /// <summary>
    /// Recommendations for preventing recurrence
    /// </summary>
    public string? Recommendations { get; set; }
    
    /// <summary>
    /// Does this require regulatory notification?
    /// </summary>
    public bool RequiresNotification { get; set; }
    
    /// <summary>
    /// Which regulators need to be notified (JSON array)
    /// </summary>
    public string? RegulatorsToNotify { get; set; }
    
    /// <summary>
    /// Has regulatory notification been sent?
    /// </summary>
    public bool NotificationSent { get; set; }
    
    /// <summary>
    /// Notification deadline
    /// </summary>
    public DateTime? NotificationDeadline { get; set; }
    
    /// <summary>
    /// Date notification was sent
    /// </summary>
    public DateTime? NotificationSentDate { get; set; }
    
    /// <summary>
    /// Estimated financial impact
    /// </summary>
    public decimal? EstimatedImpact { get; set; }
    
    /// <summary>
    /// Actual financial impact
    /// </summary>
    public decimal? ActualImpact { get; set; }
    
    /// <summary>
    /// Impact currency
    /// </summary>
    public string ImpactCurrency { get; set; } = "SAR";
    
    /// <summary>
    /// Related risk IDs (JSON array)
    /// </summary>
    public string? RelatedRiskIds { get; set; }
    
    /// <summary>
    /// Related control IDs (JSON array)
    /// </summary>
    public string? RelatedControlIds { get; set; }
    
    /// <summary>
    /// Navigation: Timeline entries
    /// </summary>
    public virtual ICollection<IncidentTimelineEntry> TimelineEntries { get; set; } = new List<IncidentTimelineEntry>();
}

/// <summary>
/// Incident Timeline Entry - Tracks all activities during incident response
/// </summary>
public class IncidentTimelineEntry : BaseEntity
{
    public Guid IncidentId { get; set; }
    
    [ForeignKey("IncidentId")]
    public virtual Incident? Incident { get; set; }
    
    /// <summary>
    /// Type of timeline entry
    /// </summary>
    public string EntryType { get; set; } = "Update"; // Detection, Update, Escalation, Containment, Communication, Recovery, Closure
    
    /// <summary>
    /// Entry title/summary
    /// </summary>
    [Required]
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Detailed description
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Phase at time of entry
    /// </summary>
    public string? Phase { get; set; }
    
    /// <summary>
    /// Status change (if any)
    /// </summary>
    public string? StatusBefore { get; set; }
    public string? StatusAfter { get; set; }
    
    /// <summary>
    /// Person who made the entry
    /// </summary>
    public string? PerformedById { get; set; }
    public string PerformedByName { get; set; } = string.Empty;
    
    /// <summary>
    /// Timestamp of the entry
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Is this entry internal-only?
    /// </summary>
    public bool IsInternal { get; set; } = true;
    
    /// <summary>
    /// Attached evidence/files (JSON array)
    /// </summary>
    public string? Attachments { get; set; }
}
