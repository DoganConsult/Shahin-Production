namespace GrcMvc.Configuration;

/// <summary>
/// GRC Auditability Control Register
/// Defines all AU controls for audit logging and evidence collection.
/// Reference: AU-01 through AU-03
/// </summary>
public static class AuditabilityControls
{
    /// <summary>
    /// AU-01: Access and Authentication Logging
    /// Ensures all access attempts and authentication events are logged with sufficient detail for investigation.
    /// </summary>
    public static class AU01_AccessLogging
    {
        public const string ControlId = "AU-01";
        public const string Name = "Access and Authentication Logging";
        public const string Objective = "Ensure all access attempts and authentication events are logged with sufficient detail for investigation";
        public const string Owner = "Security Engineering";
        public const string Frequency = "Continuous (system enforced); reviewed quarterly";

        // Risk addressed
        public const string RiskAddressed = "Inability to detect unauthorized access or investigate security incidents";

        // Control statement
        public const string Control = "All login, logout, failed login, password change, MFA, and session events are logged via AuthenticationAuditLog with timestamp, actor, IP, result.";

        // Test procedure
        public const string TestProcedure = "Export 10 sample AuthenticationAuditLog records; verify all mandatory fields populated (timestamp, userId, eventType, IP, result).";

        // Evidence
        public static readonly string[] Evidence =
        {
            "AuthenticationAuditLog query export",
            "Sample showing Login/FailedLogin/PasswordChanged events"
        };

        // Systems
        public static readonly string[] Systems = { "GrcMvc AuthenticationAuditLog", "Serilog", "Application Insights" };

        // Event types for this control
        public static class Events
        {
            public const string LoginLogged = "AU01_LOGIN_LOGGED";
            public const string LogoutLogged = "AU01_LOGOUT_LOGGED";
            public const string FailedLoginLogged = "AU01_FAILED_LOGIN_LOGGED";
            public const string PasswordChangeLogged = "AU01_PASSWORD_CHANGE_LOGGED";
            public const string MfaEventLogged = "AU01_MFA_EVENT_LOGGED";
            public const string SessionEventLogged = "AU01_SESSION_EVENT_LOGGED";
            public const string AuditLogExported = "AU01_AUDIT_LOG_EXPORTED";
        }

        // Mandatory fields for authentication audit
        public static readonly string[] MandatoryFields =
        {
            "Timestamp",
            "UserId",
            "EventType",
            "IpAddress",
            "UserAgent",
            "Result",
            "CorrelationId"
        };

        // Authentication event types to log
        public static readonly string[] AuthEventTypes =
        {
            "Login",
            "Logout",
            "FailedLogin",
            "PasswordChanged",
            "PasswordReset",
            "AccountLocked",
            "AccountUnlocked",
            "MfaEnabled",
            "MfaDisabled",
            "MfaVerified",
            "MfaFailed",
            "TokenRefresh",
            "SessionCreated",
            "SessionExpired",
            "SessionRevoked"
        };
    }

    /// <summary>
    /// AU-02: Business Event Logging
    /// Ensures all business-critical events are logged via AuditEvent with standard fields.
    /// </summary>
    public static class AU02_BusinessEventLogging
    {
        public const string ControlId = "AU-02";
        public const string Name = "Business Event Logging";
        public const string Objective = "Ensure all business-critical events are logged via AuditEvent with standard fields";
        public const string Owner = "Platform Engineering + GRC";
        public const string Frequency = "Continuous (system enforced); reviewed quarterly";

        // Risk addressed
        public const string RiskAddressed = "Lack of audit trail for compliance evidence, investigation, or customer assurance";

        // Control statement
        public const string Control = "All Create/Update/Delete/Approve/Reject operations on GRC entities emit an AuditEvent with EventType, Action, Status, Severity, Actor, TenantId, CorrelationId.";

        // Test procedure
        public const string TestProcedure = "Sample 10 AuditEvent records; verify all mandatory fields populated. Trace at least one end-to-end using CorrelationId.";

        // Evidence
        public static readonly string[] Evidence =
        {
            "AuditEvent query export",
            "Correlation trace showing linked events",
            "Schema showing mandatory fields enforced"
        };

        // Systems
        public static readonly string[] Systems = { "GrcMvc AuditEvent", "PostgreSQL", "ClickHouse (if enabled)" };

        // Event types for this control
        public static class Events
        {
            public const string EventCreated = "AU02_EVENT_CREATED";
            public const string EventUpdated = "AU02_EVENT_UPDATED";
            public const string EventDeleted = "AU02_EVENT_DELETED";
            public const string EventApproved = "AU02_EVENT_APPROVED";
            public const string EventRejected = "AU02_EVENT_REJECTED";
            public const string CorrelationTracked = "AU02_CORRELATION_TRACKED";
            public const string AuditReportGenerated = "AU02_AUDIT_REPORT_GENERATED";
        }

        // Mandatory AuditEvent fields
        public static readonly string[] MandatoryFields =
        {
            "EventId",
            "Timestamp",
            "TenantId",
            "EventType",
            "Action",
            "Status",
            "Severity",
            "Actor",
            "CorrelationId"
        };

        // Standard actions
        public static readonly string[] StandardActions =
        {
            "Create",
            "Update",
            "Delete",
            "Approve",
            "Reject",
            "Submit",
            "Complete",
            "Cancel",
            "Archive"
        };

        // Severity levels
        public static readonly string[] SeverityLevels =
        {
            "Info",
            "Warning",
            "Error",
            "Critical"
        };
    }

    /// <summary>
    /// AU-03: Platform Admin Audit Trail
    /// Ensures all platform-level admin operations are logged in a separate, protected audit stream.
    /// </summary>
    public static class AU03_PlatformAdminAudit
    {
        public const string ControlId = "AU-03";
        public const string Name = "Platform Admin Audit Trail";
        public const string Objective = "Ensure all platform-level admin operations are logged in a separate, protected audit stream";
        public const string Owner = "Security Engineering";
        public const string Frequency = "Continuous; reviewed monthly by Security";

        // Risk addressed
        public const string RiskAddressed = "Privileged operations could be performed without accountability";

        // Control statement
        public const string Control = "Platform admin actions (tenant CRUD, user impersonation, config changes) emit PlatformAuditEventTypes events, queryable independently from tenant audit.";

        // Test procedure
        public const string TestProcedure = "Export platform audit events for last month; verify coverage of tenant lifecycle, admin actions, and config changes.";

        // Evidence
        public static readonly string[] Evidence =
        {
            "Platform audit event export",
            "Monthly admin action review report",
            "Access control proof for platform audit queries"
        };

        // Systems
        public static readonly string[] Systems = { "GrcMvc PlatformAuditEvent", "Serilog", "Security SIEM" };

        // Event types for this control
        public static class Events
        {
            public const string PlatformEventLogged = "AU03_PLATFORM_EVENT_LOGGED";
            public const string TenantLifecycleLogged = "AU03_TENANT_LIFECYCLE_LOGGED";
            public const string AdminActionLogged = "AU03_ADMIN_ACTION_LOGGED";
            public const string ConfigChangeLogged = "AU03_CONFIG_CHANGE_LOGGED";
            public const string ImpersonationLogged = "AU03_IMPERSONATION_LOGGED";
            public const string PlatformAuditExported = "AU03_PLATFORM_AUDIT_EXPORTED";
        }

        // Platform audit event categories
        public static readonly string[] EventCategories =
        {
            "TenantLifecycle",    // Created, Activated, Suspended, Deleted
            "UserManagement",     // Created, Deactivated, PasswordReset, Impersonated
            "AdminManagement",    // Admin created, updated, suspended, reactivated
            "Authentication",     // Admin login, logout, failed login
            "Configuration",      // Config changed, catalog updated
            "Security"            // Emergency access, bypass events
        };

        // Platform event types (from PlatformAuditEventTypes)
        public static readonly string[] TenantEvents =
        {
            "PLATFORM_TENANT_CREATED",
            "PLATFORM_TENANT_ACTIVATED",
            "PLATFORM_TENANT_SUSPENDED",
            "PLATFORM_TENANT_DELETED",
            "PLATFORM_TENANT_UPDATED"
        };

        public static readonly string[] UserEvents =
        {
            "PLATFORM_USER_CREATED",
            "PLATFORM_USER_DEACTIVATED",
            "PLATFORM_USER_ACTIVATED",
            "PLATFORM_PASSWORD_RESET",
            "PLATFORM_USER_IMPERSONATED"
        };

        public static readonly string[] AdminEvents =
        {
            "PLATFORM_ADMIN_CREATED",
            "PLATFORM_ADMIN_UPDATED",
            "PLATFORM_ADMIN_SUSPENDED",
            "PLATFORM_ADMIN_REACTIVATED",
            "PLATFORM_ADMIN_DELETED"
        };

        public static readonly string[] AuthEvents =
        {
            "PLATFORM_ADMIN_LOGIN",
            "PLATFORM_ADMIN_LOGOUT",
            "PLATFORM_LOGIN_FAILED"
        };

        public static readonly string[] ConfigEvents =
        {
            "PLATFORM_CONFIG_CHANGED",
            "PLATFORM_CATALOG_UPDATED"
        };
    }

    /// <summary>
    /// Get all control IDs
    /// </summary>
    public static string[] GetAllControlIds() => new[]
    {
        AU01_AccessLogging.ControlId,
        AU02_BusinessEventLogging.ControlId,
        AU03_PlatformAdminAudit.ControlId
    };

    /// <summary>
    /// Get control by ID
    /// </summary>
    public static (string Name, string Objective, string Owner) GetControlInfo(string controlId) => controlId switch
    {
        "AU-01" => (AU01_AccessLogging.Name, AU01_AccessLogging.Objective, AU01_AccessLogging.Owner),
        "AU-02" => (AU02_BusinessEventLogging.Name, AU02_BusinessEventLogging.Objective, AU02_BusinessEventLogging.Owner),
        "AU-03" => (AU03_PlatformAdminAudit.Name, AU03_PlatformAdminAudit.Objective, AU03_PlatformAdminAudit.Owner),
        _ => ("Unknown", "Unknown", "Unknown")
    };

    /// <summary>
    /// Audit retention configuration
    /// </summary>
    public static class RetentionPolicy
    {
        public const int AuthenticationAuditRetentionDays = 365 * 3;  // 3 years
        public const int BusinessAuditRetentionDays = 365 * 7;        // 7 years for compliance
        public const int PlatformAdminAuditRetentionDays = 365 * 7;   // 7 years
        public const int SecurityIncidentRetentionDays = 365 * 10;    // 10 years
    }
}
