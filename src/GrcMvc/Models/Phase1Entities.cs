using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using GrcMvc.Models.Entities;

namespace GrcMvc.Models
{
    // ...existing code...

    // PHASE 1: CRITICAL TABLES (Framework, HRIS, Audit Trail)

    /// <summary>
    /// Regulatory Framework Master Data (ISO 27001, NIST, GDPR, etc.)
    /// </summary>
    public class Framework
    {
        public Guid FrameworkId { get; set; }
        public Guid TenantId { get; set; }

        public string FrameworkName { get; set; } // "ISO 27001", "NIST CSF", "GDPR"
        public string FrameworkCode { get; set; } // "ISO27001", "NIST", "GDPR"
        public string Description { get; set; }
        public string Country { get; set; } // For regulatory frameworks
        public int TotalControls { get; set; }
        public string LatestVersion { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? DeprecatedDate { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

        // Navigation
        public virtual List<Control> Controls { get; set; } = new();
        public virtual List<FrameworkVersion> FrameworkVersions { get; set; } = new();
        public virtual List<Baseline> Baselines { get; set; } = new();
    }

    /// <summary>
    /// Framework Version History (Track updates to frameworks)
    /// </summary>
    public class FrameworkVersion
    {
        public Guid VersionId { get; set; }
        public Guid FrameworkId { get; set; }

        public string Version { get; set; } // "2022", "2023.1", etc.
        public DateTime ReleaseDate { get; set; }
        public string ChangeSummary { get; set; }
        public int NewControlsCount { get; set; }
        public int DeprecatedControlsCount { get; set; }
        public int ModifiedControlsCount { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation
        public virtual Framework Framework { get; set; }
    }

    /// <summary>
    /// Individual Control Master Data (500+ controls across frameworks)
    /// NOTE: This class is superseded by Models.Entities.Control. Commenting out to avoid namespace collision.
    /// </summary>
    /*
    public class Control
    {
        public Guid ControlId { get; set; }
        public Guid FrameworkId { get; set; }
        public Guid TenantId { get; set; }

        public string ControlCode { get; set; } // "A.5.1", "PCI-DSS-1.1", etc.
        public string ControlName { get; set; }
        public string Description { get; set; }
        public string Category { get; set; } // "Preventive", "Detective", "Corrective"
        public string Criticality { get; set; } // "Critical", "High", "Medium", "Low"
        public string TestingFrequency { get; set; } // "Continuous", "Monthly", "Quarterly", "Annually"
        public string MaturityLevel { get; set; } // "1", "2", "3", "4", "5"
        public string RiskIfNotImplemented { get; set; } // "Critical", "High", "Medium", "Low"
        public string ApplicableSectors { get; set; } // JSON array or comma-separated

        // Ownership
        public Guid? OwnerUserId { get; set; }
        public string OwnerDepartment { get; set; }

        // Status
        public string ComplianceStatus { get; set; } // "Compliant", "Non-Compliant", "Planned", "In Progress"
        public double? EffectivenessScore { get; set; } // 0-100%
        public DateTime? LastTestedDate { get; set; }
        public DateTime? NextTestDate { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

        // Navigation
        public virtual Framework Framework { get; set; }
        public virtual ApplicationUser OwnerUser { get; set; }
        public virtual List<ControlEvidence> EvidenceRequirements { get; set; } = new();
        public virtual List<ControlOwnership> Ownership { get; set; } = new();
    }
    */

    /// <summary>
    /// Control Ownership & Assignment (maps controls to owners per tenant)
    /// </summary>
    public class ControlOwnership
    {
        public Guid OwnershipId { get; set; }
        public Guid ControlId { get; set; }
        public Guid TenantId { get; set; }
        public Guid OwnerId { get; set; } // ApplicationUser

        public Guid? AlternateOwnerId { get; set; } // Backup owner
        public string TestingResponsibility { get; set; } // Same owner or different
        public string ApprovalAuthority { get; set; } // Who signs off on control
        public DateTime AssignmentDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Navigation
        public virtual Control Control { get; set; }
        public virtual ApplicationUser Owner { get; set; }
        public virtual ApplicationUser AlternateOwner { get; set; }
    }

    /// <summary>
    /// Evidence Type & Requirements per Control
    /// </summary>
    public class ControlEvidence
    {
        public Guid EvidenceRequirementId { get; set; }
        public Guid ControlId { get; set; }

        public string EvidenceType { get; set; } // "Policy", "Procedure", "Log", "Certificate", "Test Result"
        public string Description { get; set; }
        public string AcceptableFormats { get; set; } // "PDF,DOC,XLSX"
        public int FrequencyDays { get; set; } // How often evidence must be refreshed (0 = continuous)
        public int MaxAgeDays { get; set; } // Max age of evidence (0 = N/A)
        public int RequiredCount { get; set; } // How many items of this type needed

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation
        public virtual Control Control { get; set; }
    }

    /// <summary>
    /// Compliance Baseline (curated set of controls per sector/framework)
    /// </summary>
    public class Baseline
    {
        public Guid BaselineId { get; set; }
        public Guid FrameworkId { get; set; }
        public Guid TenantId { get; set; }

        public string BaselineName { get; set; } // "Small Business", "Enterprise", "High Risk"
        public string Sector { get; set; }
        public string Description { get; set; }
        public int TotalControls { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation
        public virtual Framework Framework { get; set; }
        public virtual List<BaselineControl> Controls { get; set; } = new();
    }

    /// <summary>
    /// Baseline to Control Mapping
    /// </summary>
    public class BaselineControl
    {
        public Guid Id { get; set; }
        public Guid BaselineId { get; set; }
        public Guid ControlId { get; set; }
        public int Priority { get; set; } // 1 = Critical, 2 = High, 3 = Medium, 4 = Low

        public virtual Baseline Baseline { get; set; }
        public virtual Control Control { get; set; }
    }

    /// <summary>
    /// HRIS Integration Configuration
    /// </summary>
    public class HRISIntegration
    {
        public Guid IntegrationId { get; set; }
        public Guid TenantId { get; set; }

        public string SourceSystem { get; set; } // "SAP", "Workday", "ADP", "Bamboo"
        public string APIEndpoint { get; set; }
        public string AuthType { get; set; } // "OAuth2", "APIKey", "BasicAuth"
        public string EncryptedCredentials { get; set; } // Encrypted JSON with API creds

        public DateTime LastSyncDate { get; set; }
        public DateTime NextSyncDate { get; set; }
        public string SyncStatus { get; set; } // "Active", "Failed", "Paused"
        public string LastSyncError { get; set; }
        public int SyncIntervalHours { get; set; } = 6; // Sync every 6 hours by default

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

        // Navigation
        public virtual List<HRISEmployee> Employees { get; set; } = new();
    }

    /// <summary>
    /// HRIS Employee Data (synced from HR system)
    /// </summary>
    public class HRISEmployee
    {
        public Guid EmployeeId { get; set; }
        public Guid TenantId { get; set; }
        public Guid IntegrationId { get; set; }

        public string HRISEmployeeId { get; set; } // External ID from HRIS
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Department { get; set; }
        public string JobTitle { get; set; }
        public Guid? ReportsToEmployeeId { get; set; } // Manager

        public DateTime StartDate { get; set; }
        public DateTime? TerminationDate { get; set; }
        public bool IsActive { get; set; } = true;

        public Guid? LinkedUserId { get; set; } // ApplicationUser this employee maps to
        public DateTime SyncedDate { get; set; } = DateTime.UtcNow;

        // Navigation
        public virtual HRISIntegration Integration { get; set; }
        public virtual ApplicationUser LinkedUser { get; set; }
    }

    /// <summary>
    /// Audit Log - Immutable event log for all changes
    /// </summary>
    public class AuditLog
    {
        public Guid LogId { get; set; }
        public Guid TenantId { get; set; }

        public string EntityType { get; set; } // "Control", "Assessment", "Risk", etc.
        public Guid EntityId { get; set; }
        public string Action { get; set; } // "Created", "Updated", "Deleted"
        public string FieldName { get; set; } // Which field changed (if update)
        public string OldValue { get; set; }
        public string NewValue { get; set; }

        public Guid ChangedByUserId { get; set; } // Who made the change
        public DateTime ChangedDate { get; set; } = DateTime.UtcNow;
        public string IPAddress { get; set; }
        public string UserAgent { get; set; }

        // Navigation
        public virtual ApplicationUser ChangedByUser { get; set; }
    }

    /// <summary>
    /// Compliance Snapshot - Point-in-time compliance state
    /// </summary>
    public class ComplianceSnapshot
    {
        public Guid SnapshotId { get; set; }
        public Guid TenantId { get; set; }
        public Guid FrameworkId { get; set; }

        public DateTime SnapshotDate { get; set; }
        public double CompliancePercentage { get; set; } // 0-100%
        public int TotalControls { get; set; }
        public int ImplementedControls { get; set; }
        public int InProgressControls { get; set; }
        public int PlannedControls { get; set; }
        public double AverageEffectivenessScore { get; set; } // 0-100%

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation
        public virtual Framework Framework { get; set; }
    }

    /// <summary>
    /// Control Testing Result
    /// </summary>
    public class ControlTestResult
    {
        public Guid TestResultId { get; set; }
        public Guid ControlId { get; set; }
        public Guid TenantId { get; set; }
        public Guid TestedByUserId { get; set; }

        public DateTime TestDate { get; set; }
        public string TestResult { get; set; } // "Passed", "Failed", "Inconclusive"
        public string TestMethod { get; set; } // "Manual", "Automated", "Review"
        public string Findings { get; set; } // Description of test results
        public double EffectivenessScore { get; set; } // 0-100%
        public string Evidence { get; set; } // Link to test evidence

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation
        public virtual Control Control { get; set; }
        public virtual ApplicationUser TestedByUser { get; set; }
    }
}