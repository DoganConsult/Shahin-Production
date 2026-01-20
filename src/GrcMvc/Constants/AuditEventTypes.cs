namespace GrcMvc.Constants
{
    /// <summary>
    /// Access Management audit event type codes following AM control taxonomy.
    /// Used for immutable audit trails supporting GRC compliance requirements.
    /// </summary>
    public static class AuditEventTypes
    {
        // AM-01: Identity Proofing and Account Activation
        public const string AM01_USER_CREATED = "AM01_USER_CREATED";
        public const string AM01_USER_REGISTERED = "AM01_USER_REGISTERED";
        public const string AM01_USER_ACTIVATED = "AM01_USER_ACTIVATED";
        public const string AM01_TRIAL_SIGNUP_INITIATED = "AM01_TRIAL_SIGNUP_INITIATED";
        public const string AM01_TENANT_CREATED = "AM01_TENANT_CREATED";
        public const string AM01_USER_INVITED = "AM01_USER_INVITED";
        public const string AM01_VERIFICATION_SENT = "AM01_VERIFICATION_SENT";
        public const string AM01_VERIFICATION_COMPLETED = "AM01_VERIFICATION_COMPLETED";
        public const string AM01_VERIFICATION_EXPIRED = "AM01_VERIFICATION_EXPIRED";

        // AM-02: Secure Trial Provisioning
        public const string AM02_PROVISION_REQUESTED = "AM02_PROVISION_REQUESTED";
        public const string AM02_PROVISION_COMPLETED = "AM02_PROVISION_COMPLETED";
        public const string AM02_PROVISION_DENIED = "AM02_PROVISION_DENIED";
        public const string AM02_API_KEY_CREATED = "AM02_API_KEY_CREATED";
        public const string AM02_API_KEY_REVOKED = "AM02_API_KEY_REVOKED";

        // AM-03: Role-Based Access Control
        public const string AM03_ROLE_ASSIGNED = "AM03_ROLE_ASSIGNED";
        public const string AM03_ROLE_CHANGED = "AM03_ROLE_CHANGED";
        public const string AM03_ROLE_REMOVED = "AM03_ROLE_REMOVED";
        public const string AM03_PRIVILEGE_ESCALATION_BLOCKED = "AM03_PRIVILEGE_ESCALATION_BLOCKED";

        // AM-04: Privileged Access Safeguards
        public const string AM04_MFA_ENABLED = "AM04_MFA_ENABLED";
        public const string AM04_MFA_DISABLED = "AM04_MFA_DISABLED";
        public const string AM04_MFA_VERIFIED = "AM04_MFA_VERIFIED";
        public const string AM04_MFA_FAILED = "AM04_MFA_FAILED";
        public const string AM04_STEPUP_REQUIRED = "AM04_STEPUP_REQUIRED";
        public const string AM04_STEPUP_COMPLETED = "AM04_STEPUP_COMPLETED";
        public const string AM04_ACCESS_REVIEW_CREATED = "AM04_ACCESS_REVIEW_CREATED";
        public const string AM04_ACCESS_REVIEW_COMPLETED = "AM04_ACCESS_REVIEW_COMPLETED";
        public const string AM04_ACCESS_CERTIFIED = "AM04_ACCESS_CERTIFIED";

        // AM-05: Joiner/Mover/Leaver Lifecycle
        public const string AM05_STATUS_CHANGED = "AM05_STATUS_CHANGED";
        public const string AM05_USER_SUSPENDED = "AM05_USER_SUSPENDED";
        public const string AM05_USER_REACTIVATED = "AM05_USER_REACTIVATED";
        public const string AM05_USER_DEPROVISIONED = "AM05_USER_DEPROVISIONED";
        public const string AM05_INACTIVITY_WARNING = "AM05_INACTIVITY_WARNING";
        public const string AM05_INACTIVITY_SUSPENSION = "AM05_INACTIVITY_SUSPENSION";
        public const string AM05_SESSION_REVOKED = "AM05_SESSION_REVOKED";
        public const string AM05_ALL_SESSIONS_REVOKED = "AM05_ALL_SESSIONS_REVOKED";

        // AM-06: Invitation Control
        public const string AM06_INVITATION_CREATED = "AM06_INVITATION_CREATED";
        public const string AM06_INVITATION_SENT = "AM06_INVITATION_SENT";
        public const string AM06_INVITATION_RESENT = "AM06_INVITATION_RESENT";
        public const string AM06_INVITATION_ACCEPTED = "AM06_INVITATION_ACCEPTED";
        public const string AM06_INVITATION_EXPIRED = "AM06_INVITATION_EXPIRED";
        public const string AM06_INVITATION_REVOKED = "AM06_INVITATION_REVOKED";
        public const string AM06_INVITATION_LIMIT_EXCEEDED = "AM06_INVITATION_LIMIT_EXCEEDED";

        // AM-07: Abuse Prevention
        public const string AM07_RATE_LIMIT_EXCEEDED = "AM07_RATE_LIMIT_EXCEEDED";
        public const string AM07_SUSPICIOUS_ACTIVITY = "AM07_SUSPICIOUS_ACTIVITY";
        public const string AM07_IP_BLOCKED = "AM07_IP_BLOCKED";

        // AM-08: Password and Recovery
        public const string AM08_PASSWORD_SET = "AM08_PASSWORD_SET";
        public const string AM08_PASSWORD_CHANGED = "AM08_PASSWORD_CHANGED";
        public const string AM08_PASSWORD_RESET_REQUESTED = "AM08_PASSWORD_RESET_REQUESTED";
        public const string AM08_PASSWORD_RESET_COMPLETED = "AM08_PASSWORD_RESET_COMPLETED";
        public const string AM08_PASSWORD_RESET_FAILED = "AM08_PASSWORD_RESET_FAILED";
        public const string AM08_PASSWORD_POLICY_VIOLATION = "AM08_PASSWORD_POLICY_VIOLATION";
        public const string AM08_ACCOUNT_LOCKED = "AM08_ACCOUNT_LOCKED";
        public const string AM08_ACCOUNT_UNLOCKED = "AM08_ACCOUNT_UNLOCKED";

        // AM-09: Trial Tenant Governance
        public const string AM09_TRIAL_CREATED = "AM09_TRIAL_CREATED";
        public const string AM09_TRIAL_EXTENDED = "AM09_TRIAL_EXTENDED";
        public const string AM09_TRIAL_EXPIRY_WARNING = "AM09_TRIAL_EXPIRY_WARNING";
        public const string AM09_TRIAL_EXPIRED = "AM09_TRIAL_EXPIRED";
        public const string AM09_TRIAL_CONVERTED = "AM09_TRIAL_CONVERTED";
        public const string AM09_TRIAL_DATA_ARCHIVED = "AM09_TRIAL_DATA_ARCHIVED";
        public const string AM09_TRIAL_DATA_DELETED = "AM09_TRIAL_DATA_DELETED";

        // AM-10: Audit Logging
        public const string AM10_AUDIT_EXPORT_REQUESTED = "AM10_AUDIT_EXPORT_REQUESTED";
        public const string AM10_AUDIT_EXPORT_COMPLETED = "AM10_AUDIT_EXPORT_COMPLETED";

        // AM-11: Access Reviews
        public const string AM11_REVIEW_INITIATED = "AM11_REVIEW_INITIATED";
        public const string AM11_REVIEW_ITEM_CERTIFIED = "AM11_REVIEW_ITEM_CERTIFIED";
        public const string AM11_REVIEW_ITEM_REVOKED = "AM11_REVIEW_ITEM_REVOKED";
        public const string AM11_REVIEW_ITEM_MODIFIED = "AM11_REVIEW_ITEM_MODIFIED";
        public const string AM11_REVIEW_COMPLETED = "AM11_REVIEW_COMPLETED";
        public const string AM11_REVIEW_OVERDUE = "AM11_REVIEW_OVERDUE";

        // AM-12: Separation of Duties
        public const string AM12_SOD_VIOLATION_DETECTED = "AM12_SOD_VIOLATION_DETECTED";
        public const string AM12_SOD_VIOLATION_BLOCKED = "AM12_SOD_VIOLATION_BLOCKED";
        public const string AM12_SOD_OVERRIDE_REQUESTED = "AM12_SOD_OVERRIDE_REQUESTED";
        public const string AM12_SOD_OVERRIDE_APPROVED = "AM12_SOD_OVERRIDE_APPROVED";
        public const string AM12_SOD_OVERRIDE_DENIED = "AM12_SOD_OVERRIDE_DENIED";

        // Authentication events (existing integration)
        public const string AUTH_LOGIN_SUCCESS = "AUTH_LOGIN_SUCCESS";
        public const string AUTH_LOGIN_FAILED = "AUTH_LOGIN_FAILED";
        public const string AUTH_LOGOUT = "AUTH_LOGOUT";
        public const string AUTH_TOKEN_REFRESHED = "AUTH_TOKEN_REFRESHED";
        public const string AUTH_TOKEN_REVOKED = "AUTH_TOKEN_REVOKED";

        /// <summary>
        /// Gets the AM control number for an event type.
        /// </summary>
        public static string GetControlNumber(string eventType)
        {
            if (string.IsNullOrEmpty(eventType)) return "UNKNOWN";

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
            if (eventType.StartsWith("AUTH_")) return "AUTH";

            return "UNKNOWN";
        }

        /// <summary>
        /// Gets the severity level for an event type.
        /// </summary>
        public static string GetSeverity(string eventType)
        {
            // Critical events
            if (eventType.Contains("DEPROVISIONED") ||
                eventType.Contains("BLOCKED") ||
                eventType.Contains("DENIED") ||
                eventType.Contains("VIOLATION") ||
                eventType.Contains("LOCKED"))
                return "Critical";

            // High severity events
            if (eventType.Contains("SUSPENDED") ||
                eventType.Contains("REVOKED") ||
                eventType.Contains("EXPIRED") ||
                eventType.Contains("FAILED") ||
                eventType.Contains("ESCALATION"))
                return "High";

            // Medium severity events
            if (eventType.Contains("CHANGED") ||
                eventType.Contains("REMOVED") ||
                eventType.Contains("WARNING") ||
                eventType.Contains("LIMIT_EXCEEDED"))
                return "Medium";

            // Low severity (informational)
            return "Low";
        }
    }
}
