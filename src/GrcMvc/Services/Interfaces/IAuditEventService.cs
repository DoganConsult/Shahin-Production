using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GrcMvc.Models.Entities;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Interface for audit event logging (append-only event trail).
    /// Different from IAuditService which manages audit entities.
    /// </summary>
    public interface IAuditEventService
    {
        /// <summary>
        /// Log an audit event (immutable append-only).
        /// </summary>
        Task LogEventAsync(
            Guid tenantId,
            string eventType,
            string affectedEntityType,
            string affectedEntityId,
            string action,
            string actor,
            string payloadJson,
            string? correlationId = null);

        /// <summary>
        /// Log a platform admin action (cross-tenant audit).
        /// </summary>
        Task LogPlatformAdminActionAsync(
            string adminUserId,
            string eventType,
            string action,
            string description,
            Guid? targetTenantId = null,
            string? targetUserId = null,
            string? ipAddress = null,
            string? payloadJson = null);

        /// <summary>
        /// Get audit events for a tenant.
        /// </summary>
        Task<IEnumerable<dynamic>> GetEventsByTenantAsync(Guid tenantId, int pageNumber = 1, int pageSize = 100);

        /// <summary>
        /// Get audit events by correlation ID.
        /// </summary>
        Task<IEnumerable<dynamic>> GetEventsByCorrelationIdAsync(string correlationId);

        /// <summary>
        /// Get audit events with filtering.
        /// </summary>
        Task<List<AuditEvent>> GetFilteredEventsAsync(AuditEventFilter filter);

        /// <summary>
        /// Get platform admin audit events.
        /// </summary>
        Task<List<AuditEvent>> GetPlatformAdminEventsAsync(
            string? adminUserId = null,
            DateTime? from = null,
            DateTime? to = null,
            int limit = 500);

        /// <summary>
        /// Get audit event statistics.
        /// </summary>
        Task<AuditStatistics> GetStatisticsAsync(Guid? tenantId = null, DateTime? from = null, DateTime? to = null);

        /// <summary>
        /// Export audit events for compliance reporting.
        /// </summary>
        Task<List<AuditEvent>> ExportEventsAsync(Guid tenantId, DateTime from, DateTime to);
    }

    /// <summary>
    /// Filter for querying audit events
    /// </summary>
    public class AuditEventFilter
    {
        public Guid? TenantId { get; set; }
        public string? UserId { get; set; }
        public string? EventType { get; set; }
        public string? Action { get; set; }
        public string? AffectedEntityType { get; set; }
        public string? Severity { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 100;
    }

    /// <summary>
    /// Audit statistics summary
    /// </summary>
    public class AuditStatistics
    {
        public int TotalEvents { get; set; }
        public int TodayEvents { get; set; }
        public int ErrorEvents { get; set; }
        public int WarningEvents { get; set; }
        public Dictionary<string, int> EventsByType { get; set; } = new();
        public Dictionary<string, int> EventsByAction { get; set; } = new();
        public Dictionary<string, int> TopUsers { get; set; } = new();
    }

    /// <summary>
    /// Platform admin audit event types
    /// </summary>
    public static class PlatformAuditEventTypes
    {
        // Tenant Management
        public const string TenantCreated = "PLATFORM_TENANT_CREATED";
        public const string TenantActivated = "PLATFORM_TENANT_ACTIVATED";
        public const string TenantSuspended = "PLATFORM_TENANT_SUSPENDED";
        public const string TenantDeleted = "PLATFORM_TENANT_DELETED";
        public const string TenantUpdated = "PLATFORM_TENANT_UPDATED";

        // User Management
        public const string UserCreated = "PLATFORM_USER_CREATED";
        public const string UserDeactivated = "PLATFORM_USER_DEACTIVATED";
        public const string UserActivated = "PLATFORM_USER_ACTIVATED";
        public const string PasswordReset = "PLATFORM_PASSWORD_RESET";
        public const string UserImpersonated = "PLATFORM_USER_IMPERSONATED";

        // Admin Management
        public const string AdminCreated = "PLATFORM_ADMIN_CREATED";
        public const string AdminUpdated = "PLATFORM_ADMIN_UPDATED";
        public const string AdminSuspended = "PLATFORM_ADMIN_SUSPENDED";
        public const string AdminReactivated = "PLATFORM_ADMIN_REACTIVATED";
        public const string AdminDeleted = "PLATFORM_ADMIN_DELETED";

        // Authentication
        public const string AdminLogin = "PLATFORM_ADMIN_LOGIN";
        public const string AdminLogout = "PLATFORM_ADMIN_LOGOUT";
        public const string LoginFailed = "PLATFORM_LOGIN_FAILED";

        // Configuration
        public const string ConfigChanged = "PLATFORM_CONFIG_CHANGED";
        public const string CatalogUpdated = "PLATFORM_CATALOG_UPDATED";
    }

    /// <summary>
    /// Access Management Control Audit Event Types (AM-01 through AM-12)
    /// These event types map to the GRC Access Management Control Register.
    /// </summary>
    public static class AccessManagementAuditEvents
    {
        // AM-01: Identity Proofing and Account Activation
        public const string AM01_UserCreated = "AM01_USER_CREATED";
        public const string AM01_VerificationSent = "AM01_VERIFICATION_SENT";
        public const string AM01_VerificationCompleted = "AM01_VERIFICATION_COMPLETED";
        public const string AM01_StatusChanged = "AM01_STATUS_CHANGED";
        public const string AM01_VerificationExpired = "AM01_VERIFICATION_EXPIRED";
        public const string AM01_VerificationResent = "AM01_VERIFICATION_RESENT";

        // AM-02: Secure Trial Provisioning Authorization
        public const string AM02_ProvisionRequested = "AM02_PROVISION_REQUESTED";
        public const string AM02_ProvisionAuthorized = "AM02_PROVISION_AUTHORIZED";
        public const string AM02_ProvisionDenied = "AM02_PROVISION_DENIED";
        public const string AM02_ProvisionCompleted = "AM02_PROVISION_COMPLETED";
        public const string AM02_ApiKeyUsed = "AM02_API_KEY_USED";
        public const string AM02_RateLimitExceeded = "AM02_RATE_LIMIT_EXCEEDED";

        // AM-03: Role-Based Access Control and Least Privilege
        public const string AM03_RoleAssigned = "AM03_ROLE_ASSIGNED";
        public const string AM03_RoleChanged = "AM03_ROLE_CHANGED";
        public const string AM03_RoleRevoked = "AM03_ROLE_REVOKED";
        public const string AM03_PermissionGranted = "AM03_PERMISSION_GRANTED";
        public const string AM03_PermissionDenied = "AM03_PERMISSION_DENIED";
        public const string AM03_AccessReviewCompleted = "AM03_ACCESS_REVIEW_COMPLETED";

        // AM-04: Privileged Access Safeguards
        public const string AM04_MfaEnabled = "AM04_MFA_ENABLED";
        public const string AM04_MfaDisabled = "AM04_MFA_DISABLED";
        public const string AM04_MfaVerified = "AM04_MFA_VERIFIED";
        public const string AM04_MfaFailed = "AM04_MFA_FAILED";
        public const string AM04_StepUpRequired = "AM04_STEPUP_REQUIRED";
        public const string AM04_StepUpCompleted = "AM04_STEPUP_COMPLETED";
        public const string AM04_PrivilegedAction = "AM04_PRIVILEGED_ACTION";
        public const string AM04_AdminInviteSent = "AM04_ADMIN_INVITE_SENT";

        // AM-05: Joiner/Mover/Leaver Lifecycle Management
        public const string AM05_UserJoined = "AM05_USER_JOINED";
        public const string AM05_UserMoved = "AM05_USER_MOVED";
        public const string AM05_UserLeft = "AM05_USER_LEFT";
        public const string AM05_UserSuspended = "AM05_USER_SUSPENDED";
        public const string AM05_UserReactivated = "AM05_USER_REACTIVATED";
        public const string AM05_InactivitySuspension = "AM05_INACTIVITY_SUSPENSION";

        // AM-06: Invitation Control and Integrity
        public const string AM06_InviteCreated = "AM06_INVITE_CREATED";
        public const string AM06_InviteRevoked = "AM06_INVITE_REVOKED";
        public const string AM06_InviteAccepted = "AM06_INVITE_ACCEPTED";
        public const string AM06_InviteExpired = "AM06_INVITE_EXPIRED";
        public const string AM06_InviteResent = "AM06_INVITE_RESENT";
        public const string AM06_InviteRateLimited = "AM06_INVITE_RATE_LIMITED";

        // AM-07: Registration and Provisioning Abuse Prevention
        public const string AM07_CaptchaVerified = "AM07_CAPTCHA_VERIFIED";
        public const string AM07_CaptchaFailed = "AM07_CAPTCHA_FAILED";
        public const string AM07_RateLimitTriggered = "AM07_RATE_LIMIT_TRIGGERED";
        public const string AM07_IpBlocked = "AM07_IP_BLOCKED";
        public const string AM07_IpUnblocked = "AM07_IP_UNBLOCKED";
        public const string AM07_SuspiciousActivity = "AM07_SUSPICIOUS_ACTIVITY";
        public const string AM07_AbuseScoreCalculated = "AM07_ABUSE_SCORE_CALCULATED";

        // AM-08: Password and Recovery Controls
        public const string AM08_PasswordSet = "AM08_PASSWORD_SET";
        public const string AM08_PasswordChanged = "AM08_PASSWORD_CHANGED";
        public const string AM08_PasswordResetRequested = "AM08_PASSWORD_RESET_REQUESTED";
        public const string AM08_PasswordResetCompleted = "AM08_PASSWORD_RESET_COMPLETED";
        public const string AM08_PasswordPolicyViolation = "AM08_PASSWORD_POLICY_VIOLATION";
        public const string AM08_AccountLocked = "AM08_ACCOUNT_LOCKED";
        public const string AM08_AccountUnlocked = "AM08_ACCOUNT_UNLOCKED";

        // AM-09: Trial Tenant Governance
        public const string AM09_TrialStarted = "AM09_TRIAL_STARTED";
        public const string AM09_TrialExtended = "AM09_TRIAL_EXTENDED";
        public const string AM09_TrialExpiryWarning = "AM09_TRIAL_EXPIRY_WARNING";
        public const string AM09_TrialExpired = "AM09_TRIAL_EXPIRED";
        public const string AM09_TrialConverted = "AM09_TRIAL_CONVERTED";
        public const string AM09_TrialDataArchived = "AM09_TRIAL_DATA_ARCHIVED";
        public const string AM09_TrialDataDeleted = "AM09_TRIAL_DATA_DELETED";

        // AM-10: Audit Logging and Traceability
        public const string AM10_AuditEventLogged = "AM10_AUDIT_EVENT_LOGGED";
        public const string AM10_AuditExportRequested = "AM10_AUDIT_EXPORT_REQUESTED";
        public const string AM10_AuditExportCompleted = "AM10_AUDIT_EXPORT_COMPLETED";
        public const string AM10_AuditRetentionApplied = "AM10_AUDIT_RETENTION_APPLIED";

        // AM-11: Periodic Access Reviews
        public const string AM11_ReviewScheduled = "AM11_REVIEW_SCHEDULED";
        public const string AM11_ReviewStarted = "AM11_REVIEW_STARTED";
        public const string AM11_ReviewItemApproved = "AM11_REVIEW_ITEM_APPROVED";
        public const string AM11_ReviewItemRevoked = "AM11_REVIEW_ITEM_REVOKED";
        public const string AM11_ReviewCompleted = "AM11_REVIEW_COMPLETED";
        public const string AM11_ReviewOverdue = "AM11_REVIEW_OVERDUE";
        public const string AM11_ReminderSent = "AM11_REMINDER_SENT";
        public const string AM11_RemediationRequired = "AM11_REMEDIATION_REQUIRED";

        // AM-12: Separation of Duties for GRC Actions
        public const string AM12_SoDRuleCreated = "AM12_SOD_RULE_CREATED";
        public const string AM12_SoDRuleModified = "AM12_SOD_RULE_MODIFIED";
        public const string AM12_SoDViolationDetected = "AM12_SOD_VIOLATION_DETECTED";
        public const string AM12_SoDViolationBlocked = "AM12_SOD_VIOLATION_BLOCKED";
        public const string AM12_SoDExceptionGranted = "AM12_SOD_EXCEPTION_GRANTED";
        public const string AM12_SoDExceptionExpired = "AM12_SOD_EXCEPTION_EXPIRED";

        /// <summary>
        /// Get control ID from event type
        /// </summary>
        public static string GetControlId(string eventType)
        {
            if (eventType.StartsWith("AM01_")) return "AM-01";
            if (eventType.StartsWith("AM02_")) return "AM-02";
            if (eventType.StartsWith("AM03_")) return "AM-03";
            if (eventType.StartsWith("AM04_")) return "AM-04";
            if (eventType.StartsWith("AM05_")) return "AM-05";
            if (eventType.StartsWith("AM06_")) return "AM-06";
            if (eventType.StartsWith("AM07_")) return "AM-07";
            if (eventType.StartsWith("AM08_")) return "AM-08";
            if (eventType.StartsWith("AM09_")) return "AM-09";
            if (eventType.StartsWith("AM10_")) return "AM-10";
            if (eventType.StartsWith("AM11_")) return "AM-11";
            if (eventType.StartsWith("AM12_")) return "AM-12";
            return "UNKNOWN";
        }

        /// <summary>
        /// Get all event types for a control
        /// </summary>
        public static string[] GetEventsForControl(string controlId)
        {
            return controlId switch
            {
                "AM-01" => new[] { AM01_UserCreated, AM01_VerificationSent, AM01_VerificationCompleted, AM01_StatusChanged, AM01_VerificationExpired, AM01_VerificationResent },
                "AM-02" => new[] { AM02_ProvisionRequested, AM02_ProvisionAuthorized, AM02_ProvisionDenied, AM02_ProvisionCompleted, AM02_ApiKeyUsed, AM02_RateLimitExceeded },
                "AM-03" => new[] { AM03_RoleAssigned, AM03_RoleChanged, AM03_RoleRevoked, AM03_PermissionGranted, AM03_PermissionDenied, AM03_AccessReviewCompleted },
                "AM-04" => new[] { AM04_MfaEnabled, AM04_MfaDisabled, AM04_MfaVerified, AM04_MfaFailed, AM04_StepUpRequired, AM04_StepUpCompleted, AM04_PrivilegedAction, AM04_AdminInviteSent },
                "AM-05" => new[] { AM05_UserJoined, AM05_UserMoved, AM05_UserLeft, AM05_UserSuspended, AM05_UserReactivated, AM05_InactivitySuspension },
                "AM-06" => new[] { AM06_InviteCreated, AM06_InviteRevoked, AM06_InviteAccepted, AM06_InviteExpired, AM06_InviteResent, AM06_InviteRateLimited },
                "AM-07" => new[] { AM07_CaptchaVerified, AM07_CaptchaFailed, AM07_RateLimitTriggered, AM07_IpBlocked, AM07_IpUnblocked, AM07_SuspiciousActivity, AM07_AbuseScoreCalculated },
                "AM-08" => new[] { AM08_PasswordSet, AM08_PasswordChanged, AM08_PasswordResetRequested, AM08_PasswordResetCompleted, AM08_PasswordPolicyViolation, AM08_AccountLocked, AM08_AccountUnlocked },
                "AM-09" => new[] { AM09_TrialStarted, AM09_TrialExtended, AM09_TrialExpiryWarning, AM09_TrialExpired, AM09_TrialConverted, AM09_TrialDataArchived, AM09_TrialDataDeleted },
                "AM-10" => new[] { AM10_AuditEventLogged, AM10_AuditExportRequested, AM10_AuditExportCompleted, AM10_AuditRetentionApplied },
                "AM-11" => new[] { AM11_ReviewScheduled, AM11_ReviewStarted, AM11_ReviewItemApproved, AM11_ReviewItemRevoked, AM11_ReviewCompleted, AM11_ReviewOverdue, AM11_ReminderSent, AM11_RemediationRequired },
                "AM-12" => new[] { AM12_SoDRuleCreated, AM12_SoDRuleModified, AM12_SoDViolationDetected, AM12_SoDViolationBlocked, AM12_SoDExceptionGranted, AM12_SoDExceptionExpired },
                _ => Array.Empty<string>()
            };
        }
    }

    /// <summary>
    /// Module Governance Control Audit Event Types (MG-01 through MG-03)
    /// These event types map to the GRC Module Governance Control Register.
    /// </summary>
    public static class ModuleGovernanceAuditEvents
    {
        // MG-01: Module Inventory and Traceability
        public const string MG01_ModuleAdded = "MG01_MODULE_ADDED";
        public const string MG01_ModuleRemoved = "MG01_MODULE_REMOVED";
        public const string MG01_ModuleStatusChanged = "MG01_MODULE_STATUS_CHANGED";
        public const string MG01_InventoryExported = "MG01_INVENTORY_EXPORTED";
        public const string MG01_InventoryReviewed = "MG01_INVENTORY_REVIEWED";
        public const string MG01_ModuleOwnerChanged = "MG01_MODULE_OWNER_CHANGED";

        // MG-02: Module Change Approval
        public const string MG02_ChangeRequested = "MG02_CHANGE_REQUESTED";
        public const string MG02_ChangeApproved = "MG02_CHANGE_APPROVED";
        public const string MG02_ChangeDenied = "MG02_CHANGE_DENIED";
        public const string MG02_ChangeDeployed = "MG02_CHANGE_DEPLOYED";
        public const string MG02_ChangeRolledBack = "MG02_CHANGE_ROLLED_BACK";
        public const string MG02_EmergencyChange = "MG02_EMERGENCY_CHANGE";

        // MG-03: Environment Parity
        public const string MG03_ParityCheckPassed = "MG03_PARITY_CHECK_PASSED";
        public const string MG03_ParityCheckFailed = "MG03_PARITY_CHECK_FAILED";
        public const string MG03_DriftDetected = "MG03_DRIFT_DETECTED";
        public const string MG03_DriftResolved = "MG03_DRIFT_RESOLVED";
        public const string MG03_ExceptionApproved = "MG03_EXCEPTION_APPROVED";
        public const string MG03_EnvironmentSynced = "MG03_ENVIRONMENT_SYNCED";

        /// <summary>
        /// Get control ID from event type
        /// </summary>
        public static string GetControlId(string eventType)
        {
            if (eventType.StartsWith("MG01_")) return "MG-01";
            if (eventType.StartsWith("MG02_")) return "MG-02";
            if (eventType.StartsWith("MG03_")) return "MG-03";
            return "UNKNOWN";
        }

        /// <summary>
        /// Get all event types for a control
        /// </summary>
        public static string[] GetEventsForControl(string controlId)
        {
            return controlId switch
            {
                "MG-01" => new[] { MG01_ModuleAdded, MG01_ModuleRemoved, MG01_ModuleStatusChanged, MG01_InventoryExported, MG01_InventoryReviewed, MG01_ModuleOwnerChanged },
                "MG-02" => new[] { MG02_ChangeRequested, MG02_ChangeApproved, MG02_ChangeDenied, MG02_ChangeDeployed, MG02_ChangeRolledBack, MG02_EmergencyChange },
                "MG-03" => new[] { MG03_ParityCheckPassed, MG03_ParityCheckFailed, MG03_DriftDetected, MG03_DriftResolved, MG03_ExceptionApproved, MG03_EnvironmentSynced },
                _ => Array.Empty<string>()
            };
        }
    }

    /// <summary>
    /// Auditability Control Audit Event Types (AU-01 through AU-03)
    /// These event types map to the GRC Auditability Control Register.
    /// </summary>
    public static class AuditabilityAuditEvents
    {
        // AU-01: Access and Authentication Logging
        public const string AU01_LoginLogged = "AU01_LOGIN_LOGGED";
        public const string AU01_LogoutLogged = "AU01_LOGOUT_LOGGED";
        public const string AU01_FailedLoginLogged = "AU01_FAILED_LOGIN_LOGGED";
        public const string AU01_PasswordChangeLogged = "AU01_PASSWORD_CHANGE_LOGGED";
        public const string AU01_MfaEventLogged = "AU01_MFA_EVENT_LOGGED";
        public const string AU01_SessionEventLogged = "AU01_SESSION_EVENT_LOGGED";
        public const string AU01_AuditLogExported = "AU01_AUDIT_LOG_EXPORTED";

        // AU-02: Business Event Logging
        public const string AU02_EventCreated = "AU02_EVENT_CREATED";
        public const string AU02_EventUpdated = "AU02_EVENT_UPDATED";
        public const string AU02_EventDeleted = "AU02_EVENT_DELETED";
        public const string AU02_EventApproved = "AU02_EVENT_APPROVED";
        public const string AU02_EventRejected = "AU02_EVENT_REJECTED";
        public const string AU02_CorrelationTracked = "AU02_CORRELATION_TRACKED";
        public const string AU02_AuditReportGenerated = "AU02_AUDIT_REPORT_GENERATED";

        // AU-03: Platform Admin Audit Trail
        public const string AU03_PlatformEventLogged = "AU03_PLATFORM_EVENT_LOGGED";
        public const string AU03_TenantLifecycleLogged = "AU03_TENANT_LIFECYCLE_LOGGED";
        public const string AU03_AdminActionLogged = "AU03_ADMIN_ACTION_LOGGED";
        public const string AU03_ConfigChangeLogged = "AU03_CONFIG_CHANGE_LOGGED";
        public const string AU03_ImpersonationLogged = "AU03_IMPERSONATION_LOGGED";
        public const string AU03_PlatformAuditExported = "AU03_PLATFORM_AUDIT_EXPORTED";

        /// <summary>
        /// Get control ID from event type
        /// </summary>
        public static string GetControlId(string eventType)
        {
            if (eventType.StartsWith("AU01_")) return "AU-01";
            if (eventType.StartsWith("AU02_")) return "AU-02";
            if (eventType.StartsWith("AU03_")) return "AU-03";
            return "UNKNOWN";
        }

        /// <summary>
        /// Get all event types for a control
        /// </summary>
        public static string[] GetEventsForControl(string controlId)
        {
            return controlId switch
            {
                "AU-01" => new[] { AU01_LoginLogged, AU01_LogoutLogged, AU01_FailedLoginLogged, AU01_PasswordChangeLogged, AU01_MfaEventLogged, AU01_SessionEventLogged, AU01_AuditLogExported },
                "AU-02" => new[] { AU02_EventCreated, AU02_EventUpdated, AU02_EventDeleted, AU02_EventApproved, AU02_EventRejected, AU02_CorrelationTracked, AU02_AuditReportGenerated },
                "AU-03" => new[] { AU03_PlatformEventLogged, AU03_TenantLifecycleLogged, AU03_AdminActionLogged, AU03_ConfigChangeLogged, AU03_ImpersonationLogged, AU03_PlatformAuditExported },
                _ => Array.Empty<string>()
            };
        }
    }

    /// <summary>
    /// Feature Management Control Audit Event Types (FM-01)
    /// These event types map to the GRC Feature Management Control Register.
    /// </summary>
    public static class FeatureManagementAuditEvents
    {
        // FM-01: Feature Flag Governance
        public const string FM01_FeatureEnabled = "FM01_FEATURE_ENABLED";
        public const string FM01_FeatureDisabled = "FM01_FEATURE_DISABLED";
        public const string FM01_FeatureValueChanged = "FM01_FEATURE_VALUE_CHANGED";
        public const string FM01_TenantFeatureOverride = "FM01_TENANT_FEATURE_OVERRIDE";
        public const string FM01_FeatureApprovalRequested = "FM01_FEATURE_APPROVAL_REQUESTED";
        public const string FM01_FeatureApproved = "FM01_FEATURE_APPROVED";
        public const string FM01_FeatureDenied = "FM01_FEATURE_DENIED";
        public const string FM01_FeatureRolledBack = "FM01_FEATURE_ROLLED_BACK";

        /// <summary>
        /// Get control ID from event type
        /// </summary>
        public static string GetControlId(string eventType)
        {
            if (eventType.StartsWith("FM01_")) return "FM-01";
            return "UNKNOWN";
        }

        /// <summary>
        /// Get all event types for a control
        /// </summary>
        public static string[] GetEventsForControl(string controlId)
        {
            return controlId switch
            {
                "FM-01" => new[] { FM01_FeatureEnabled, FM01_FeatureDisabled, FM01_FeatureValueChanged, FM01_TenantFeatureOverride, FM01_FeatureApprovalRequested, FM01_FeatureApproved, FM01_FeatureDenied, FM01_FeatureRolledBack },
                _ => Array.Empty<string>()
            };
        }
    }

    /// <summary>
    /// Background Processing Control Audit Event Types (BP-01, BP-02)
    /// These event types map to the GRC Background Processing Control Register.
    /// </summary>
    public static class BackgroundProcessingAuditEvents
    {
        // BP-01: Background Job Governance (Hangfire)
        public const string BP01_JobScheduled = "BP01_JOB_SCHEDULED";
        public const string BP01_JobStarted = "BP01_JOB_STARTED";
        public const string BP01_JobCompleted = "BP01_JOB_COMPLETED";
        public const string BP01_JobFailed = "BP01_JOB_FAILED";
        public const string BP01_JobRetried = "BP01_JOB_RETRIED";
        public const string BP01_JobCancelled = "BP01_JOB_CANCELLED";
        public const string BP01_CriticalJobMissed = "BP01_CRITICAL_JOB_MISSED";

        // BP-02: ABP Worker Disablement Documentation
        public const string BP02_DisablementDocumented = "BP02_DISABLEMENT_DOCUMENTED";
        public const string BP02_ExitPlanUpdated = "BP02_EXIT_PLAN_UPDATED";
        public const string BP02_CompensatingControlVerified = "BP02_COMPENSATING_CONTROL_VERIFIED";
        public const string BP02_ReenablementAttempted = "BP02_REENABLEMENT_ATTEMPTED";
        public const string BP02_ReenablementSuccessful = "BP02_REENABLEMENT_SUCCESSFUL";
        public const string BP02_ReenablementFailed = "BP02_REENABLEMENT_FAILED";

        /// <summary>
        /// Get control ID from event type
        /// </summary>
        public static string GetControlId(string eventType)
        {
            if (eventType.StartsWith("BP01_")) return "BP-01";
            if (eventType.StartsWith("BP02_")) return "BP-02";
            return "UNKNOWN";
        }

        /// <summary>
        /// Get all event types for a control
        /// </summary>
        public static string[] GetEventsForControl(string controlId)
        {
            return controlId switch
            {
                "BP-01" => new[] { BP01_JobScheduled, BP01_JobStarted, BP01_JobCompleted, BP01_JobFailed, BP01_JobRetried, BP01_JobCancelled, BP01_CriticalJobMissed },
                "BP-02" => new[] { BP02_DisablementDocumented, BP02_ExitPlanUpdated, BP02_CompensatingControlVerified, BP02_ReenablementAttempted, BP02_ReenablementSuccessful, BP02_ReenablementFailed },
                _ => Array.Empty<string>()
            };
        }
    }

    /// <summary>
    /// Integration Control Audit Event Types (IN-01, IN-02)
    /// These event types map to the GRC Integration Control Register.
    /// </summary>
    public static class IntegrationAuditEvents
    {
        // IN-01: Integration Enablement Approval
        public const string IN01_IntegrationRequested = "IN01_INTEGRATION_REQUESTED";
        public const string IN01_IntegrationApproved = "IN01_INTEGRATION_APPROVED";
        public const string IN01_IntegrationDenied = "IN01_INTEGRATION_DENIED";
        public const string IN01_IntegrationEnabled = "IN01_INTEGRATION_ENABLED";
        public const string IN01_IntegrationDisabled = "IN01_INTEGRATION_DISABLED";
        public const string IN01_IntegrationConfigChanged = "IN01_INTEGRATION_CONFIG_CHANGED";
        public const string IN01_SecurityReviewCompleted = "IN01_SECURITY_REVIEW_COMPLETED";

        // IN-02: Integration Credential Management
        public const string IN02_CredentialCreated = "IN02_CREDENTIAL_CREATED";
        public const string IN02_CredentialRotated = "IN02_CREDENTIAL_ROTATED";
        public const string IN02_CredentialExpired = "IN02_CREDENTIAL_EXPIRED";
        public const string IN02_CredentialAccessed = "IN02_CREDENTIAL_ACCESSED";
        public const string IN02_CredentialRevoked = "IN02_CREDENTIAL_REVOKED";
        public const string IN02_RotationDue = "IN02_ROTATION_DUE";
        public const string IN02_RotationOverdue = "IN02_ROTATION_OVERDUE";

        /// <summary>
        /// Get control ID from event type
        /// </summary>
        public static string GetControlId(string eventType)
        {
            if (eventType.StartsWith("IN01_")) return "IN-01";
            if (eventType.StartsWith("IN02_")) return "IN-02";
            return "UNKNOWN";
        }

        /// <summary>
        /// Get all event types for a control
        /// </summary>
        public static string[] GetEventsForControl(string controlId)
        {
            return controlId switch
            {
                "IN-01" => new[] { IN01_IntegrationRequested, IN01_IntegrationApproved, IN01_IntegrationDenied, IN01_IntegrationEnabled, IN01_IntegrationDisabled, IN01_IntegrationConfigChanged, IN01_SecurityReviewCompleted },
                "IN-02" => new[] { IN02_CredentialCreated, IN02_CredentialRotated, IN02_CredentialExpired, IN02_CredentialAccessed, IN02_CredentialRevoked, IN02_RotationDue, IN02_RotationOverdue },
                _ => Array.Empty<string>()
            };
        }
    }

    /// <summary>
    /// AI Governance Control Audit Event Types (AI-01, AI-02)
    /// These event types map to the GRC AI Governance Control Register.
    /// </summary>
    public static class AIGovernanceAuditEvents
    {
        // AI-01: AI Feature Enablement Governance
        public const string AI01_OptInRequested = "AI01_OPTIN_REQUESTED";
        public const string AI01_OptInApproved = "AI01_OPTIN_APPROVED";
        public const string AI01_OptInDenied = "AI01_OPTIN_DENIED";
        public const string AI01_FeatureEnabled = "AI01_FEATURE_ENABLED";
        public const string AI01_FeatureDisabled = "AI01_FEATURE_DISABLED";
        public const string AI01_OptOutRequested = "AI01_OPTOUT_REQUESTED";
        public const string AI01_OptOutCompleted = "AI01_OPTOUT_COMPLETED";
        public const string AI01_UsageStarted = "AI01_USAGE_STARTED";

        // AI-02: AI Usage Logging and Transparency
        public const string AI02_RequestStarted = "AI02_REQUEST_STARTED";
        public const string AI02_RequestCompleted = "AI02_REQUEST_COMPLETED";
        public const string AI02_RequestFailed = "AI02_REQUEST_FAILED";
        public const string AI02_RateLimited = "AI02_RATE_LIMITED";
        public const string AI02_QuotaExceeded = "AI02_QUOTA_EXCEEDED";
        public const string AI02_UsageReportGenerated = "AI02_USAGE_REPORT_GENERATED";
        public const string AI02_AnomalyDetected = "AI02_ANOMALY_DETECTED";

        /// <summary>
        /// Get control ID from event type
        /// </summary>
        public static string GetControlId(string eventType)
        {
            if (eventType.StartsWith("AI01_")) return "AI-01";
            if (eventType.StartsWith("AI02_")) return "AI-02";
            return "UNKNOWN";
        }

        /// <summary>
        /// Get all event types for a control
        /// </summary>
        public static string[] GetEventsForControl(string controlId)
        {
            return controlId switch
            {
                "AI-01" => new[] { AI01_OptInRequested, AI01_OptInApproved, AI01_OptInDenied, AI01_FeatureEnabled, AI01_FeatureDisabled, AI01_OptOutRequested, AI01_OptOutCompleted, AI01_UsageStarted },
                "AI-02" => new[] { AI02_RequestStarted, AI02_RequestCompleted, AI02_RequestFailed, AI02_RateLimited, AI02_QuotaExceeded, AI02_UsageReportGenerated, AI02_AnomalyDetected },
                _ => Array.Empty<string>()
            };
        }
    }

    /// <summary>
    /// Master control registry combining all control families
    /// </summary>
    public static class ControlAuditRegistry
    {
        /// <summary>
        /// Get all control families
        /// </summary>
        public static string[] GetAllControlFamilies() => new[]
        {
            "AM", // Access Management
            "MG", // Module Governance
            "AU", // Auditability
            "FM", // Feature Management
            "BP", // Background Processing
            "IN", // Integration
            "AI"  // AI Governance
        };

        /// <summary>
        /// Get control ID from any event type
        /// </summary>
        public static string GetControlId(string eventType)
        {
            if (eventType.StartsWith("AM")) return AccessManagementAuditEvents.GetControlId(eventType);
            if (eventType.StartsWith("MG")) return ModuleGovernanceAuditEvents.GetControlId(eventType);
            if (eventType.StartsWith("AU")) return AuditabilityAuditEvents.GetControlId(eventType);
            if (eventType.StartsWith("FM")) return FeatureManagementAuditEvents.GetControlId(eventType);
            if (eventType.StartsWith("BP")) return BackgroundProcessingAuditEvents.GetControlId(eventType);
            if (eventType.StartsWith("IN")) return IntegrationAuditEvents.GetControlId(eventType);
            if (eventType.StartsWith("AI")) return AIGovernanceAuditEvents.GetControlId(eventType);
            if (eventType.StartsWith("PLATFORM_")) return "PLATFORM";
            return "UNKNOWN";
        }

        /// <summary>
        /// Get control family from control ID
        /// </summary>
        public static string GetControlFamily(string controlId)
        {
            if (controlId.StartsWith("AM-")) return "AM";
            if (controlId.StartsWith("MG-")) return "MG";
            if (controlId.StartsWith("AU-")) return "AU";
            if (controlId.StartsWith("FM-")) return "FM";
            if (controlId.StartsWith("BP-")) return "BP";
            if (controlId.StartsWith("IN-")) return "IN";
            if (controlId.StartsWith("AI-")) return "AI";
            return "UNKNOWN";
        }

        /// <summary>
        /// Get all control IDs
        /// </summary>
        public static string[] GetAllControlIds() => new[]
        {
            // Access Management (12 controls)
            "AM-01", "AM-02", "AM-03", "AM-04", "AM-05", "AM-06",
            "AM-07", "AM-08", "AM-09", "AM-10", "AM-11", "AM-12",
            // Module Governance (3 controls)
            "MG-01", "MG-02", "MG-03",
            // Auditability (3 controls)
            "AU-01", "AU-02", "AU-03",
            // Feature Management (1 control)
            "FM-01",
            // Background Processing (2 controls)
            "BP-01", "BP-02",
            // Integration (2 controls)
            "IN-01", "IN-02",
            // AI Governance (2 controls)
            "AI-01", "AI-02"
        };

        /// <summary>
        /// Get total event count across all controls
        /// </summary>
        public static int GetTotalEventCount()
        {
            // AM: 72 events, MG: 18 events, AU: 20 events, FM: 8 events, BP: 13 events, IN: 14 events, AI: 15 events
            return 72 + 18 + 20 + 8 + 13 + 14 + 15; // = 160 events
        }
    }
}
