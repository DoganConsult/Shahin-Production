using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities;

/// <summary>
/// Serial counter for generating business reference codes per tenant per object type.
/// Ensures globally unique, sequential, human-readable codes.
/// 
/// Format: {TENANTCODE}-{OBJECTTYPE}-{YYYY}-{SEQUENCE}
/// Example: ACME-CTRL-2026-000143
/// </summary>
[Table("SerialCounters")]
public class SerialCounter
{
    /// <summary>
    /// Composite key: TenantId + ObjectType + Year
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Tenant ID (required, except for platform-level counters)
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Object type code (3-6 chars): POL, CTRL, RSK, EVD, ASMT, EXC, PLAN, TSK, AUD, VND
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string ObjectType { get; set; } = string.Empty;

    /// <summary>
    /// Year for the counter (optional for per-year sequencing)
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Next available sequence number
    /// </summary>
    public long NextValue { get; set; } = 1;

    /// <summary>
    /// Row version for concurrency control
    /// </summary>
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    /// <summary>
    /// Last updated timestamp
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Standard object type codes for serial generation
/// </summary>
public static class ObjectTypeCodes
{
    // Core GRC Objects
    public const string Tenant = "TEN";
    public const string Policy = "POL";
    public const string Control = "CTRL";
    public const string Risk = "RSK";
    public const string Assessment = "ASMT";
    public const string Evidence = "EVD";
    public const string Exception = "EXC";
    public const string ActionPlan = "PLAN";
    public const string Audit = "AUD";
    public const string AuditFinding = "FND";
    public const string Vendor = "VND";
    public const string Task = "TSK";
    public const string Workflow = "WFL";
    public const string Report = "RPT";
    public const string Document = "DOC";
    public const string Framework = "FWK";
    public const string Regulator = "REG";
    public const string ComplianceEvent = "EVT";
    public const string Subscription = "SUB";
    
    // Mapping from entity type name to code
    public static string GetCode(string entityTypeName) => entityTypeName switch
    {
        nameof(Tenant) => Tenant,
        nameof(Policy) => Policy,
        nameof(Control) => Control,
        nameof(Risk) => Risk,
        nameof(Assessment) => Assessment,
        nameof(Evidence) => Evidence,
        nameof(ActionPlan) => ActionPlan,
        nameof(Audit) => Audit,
        nameof(AuditFinding) => AuditFinding,
        nameof(Vendor) => Vendor,
        nameof(WorkflowTask) => Task,
        nameof(Workflow) => Workflow,
        nameof(Report) => Report,
        nameof(Framework) => Framework,
        nameof(Regulator) => Regulator,
        nameof(ComplianceEvent) => ComplianceEvent,
        nameof(Subscription) => Subscription,
        _ => entityTypeName.Length >= 4 
            ? entityTypeName[..4].ToUpperInvariant() 
            : entityTypeName.ToUpperInvariant()
    };
}
