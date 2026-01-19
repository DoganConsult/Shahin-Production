namespace GrcMvc.Configuration;

/// <summary>
/// GRC Access Management Control Register
/// Defines all AM controls for user creation flows and access lifecycle.
/// Reference: AM-01 through AM-12
/// </summary>
public static class AccessManagementControls
{
    /// <summary>
    /// AM-01: Identity Proofing and Account Activation
    /// Ensures only legitimate users become Active.
    /// </summary>
    public static class AM01_IdentityProofing
    {
        public const string ControlId = "AM-01";
        public const string Name = "Identity Proofing and Account Activation";
        public const string Objective = "Ensure only legitimate users become Active";
        public const string Owner = "Product/IAM Engineering";
        public const string Frequency = "Continuous (system enforced), reviewed quarterly";

        // Applies to
        public static readonly string[] AppliesTo = { "Flow1_SelfRegistration", "Flow3_Invited", "Flow2_Provision" };

        // Event types for this control
        public static class Events
        {
            public const string UserCreated = "AM01_USER_CREATED";
            public const string VerificationSent = "AM01_VERIFICATION_SENT";
            public const string VerificationCompleted = "AM01_VERIFICATION_COMPLETED";
            public const string StatusChanged = "AM01_STATUS_CHANGED";
            public const string VerificationExpired = "AM01_VERIFICATION_EXPIRED";
            public const string VerificationResent = "AM01_VERIFICATION_RESENT";
        }

        // Configuration
        public const int VerificationTokenExpiryHours = 48;
        public const int MaxResendAttempts = 3;
    }

    /// <summary>
    /// AM-02: Secure Trial Provisioning Authorization
    /// Prevents unauthorized creation of Active tenants/admins via /api/trial/provision.
    /// </summary>
    public static class AM02_TrialProvisioning
    {
        public const string ControlId = "AM-02";
        public const string Name = "Secure Trial Provisioning Authorization";
        public const string Objective = "Prevent unauthorized creation of Active tenants/admins via provisioning API";
        public const string Owner = "Security Engineering / Platform Engineering";
        public const string Frequency = "Continuous; policy reviewed quarterly";

        public static readonly string[] AppliesTo = { "Flow2_TrialApiProvision" };

        public static class Events
        {
            public const string ProvisionRequested = "AM02_PROVISION_REQUESTED";
            public const string ProvisionAuthorized = "AM02_PROVISION_AUTHORIZED";
            public const string ProvisionDenied = "AM02_PROVISION_DENIED";
            public const string ProvisionCompleted = "AM02_PROVISION_COMPLETED";
            public const string ApiKeyUsed = "AM02_API_KEY_USED";
            public const string RateLimitExceeded = "AM02_RATE_LIMIT_EXCEEDED";
        }

        // Configuration
        public const int MaxProvisionsPerHour = 10;
        public const int MaxProvisionsPerDay = 50;
    }

    /// <summary>
    /// AM-03: Role-Based Access Control and Least Privilege
    /// Ensures users receive only the permissions required for their role.
    /// </summary>
    public static class AM03_RBAC
    {
        public const string ControlId = "AM-03";
        public const string Name = "Role-Based Access Control and Least Privilege";
        public const string Objective = "Ensure users receive only the permissions required for their role";
        public const string Owner = "Product Security / GRC + Engineering";
        public const string Frequency = "Continuous; reviewed quarterly";

        public static readonly string[] AppliesTo = { "Flow1_SelfRegistration", "Flow2_TrialApiProvision", "Flow3_Invited" };

        public static class Events
        {
            public const string RoleAssigned = "AM03_ROLE_ASSIGNED";
            public const string RoleChanged = "AM03_ROLE_CHANGED";
            public const string RoleRevoked = "AM03_ROLE_REVOKED";
            public const string PermissionGranted = "AM03_PERMISSION_GRANTED";
            public const string PermissionDenied = "AM03_PERMISSION_DENIED";
            public const string AccessReviewCompleted = "AM03_ACCESS_REVIEW_COMPLETED";
        }
    }

    /// <summary>
    /// AM-04: Privileged Access Safeguards
    /// Reduces risk of misuse/compromise of privileged accounts.
    /// </summary>
    public static class AM04_PrivilegedAccess
    {
        public const string ControlId = "AM-04";
        public const string Name = "Privileged Access Safeguards (Admins)";
        public const string Objective = "Reduce risk of misuse/compromise of privileged accounts";
        public const string Owner = "Security Engineering / IAM";
        public const string Frequency = "Continuous; tested semi-annually";

        public static readonly string[] AppliesTo = { "TENANT_ADMIN", "PLATFORM_ADMIN" };

        public static class Events
        {
            public const string MfaEnabled = "AM04_MFA_ENABLED";
            public const string MfaDisabled = "AM04_MFA_DISABLED";
            public const string MfaVerified = "AM04_MFA_VERIFIED";
            public const string MfaFailed = "AM04_MFA_FAILED";
            public const string StepUpRequired = "AM04_STEPUP_REQUIRED";
            public const string StepUpCompleted = "AM04_STEPUP_COMPLETED";
            public const string PrivilegedActionPerformed = "AM04_PRIVILEGED_ACTION";
            public const string AdminInviteSent = "AM04_ADMIN_INVITE_SENT";
        }

        // Privileged roles requiring MFA
        public static readonly string[] PrivilegedRoles =
        {
            "PlatformAdmin",
            "SystemAdministrator",
            "TenantAdmin",
            "TenantOwner"
        };
    }

    /// <summary>
    /// AM-05: Joiner/Mover/Leaver Lifecycle Management
    /// Ensures access is updated promptly when users change roles or leave.
    /// </summary>
    public static class AM05_JMLLifecycle
    {
        public const string ControlId = "AM-05";
        public const string Name = "Joiner/Mover/Leaver Lifecycle Management";
        public const string Objective = "Ensure access is updated promptly when users change roles or leave";
        public const string Owner = "Tenant Admins (execution), Product/GRC (policy)";
        public const string Frequency = "Continuous; reviewed quarterly";

        public static readonly string[] AppliesTo = { "Flow1_SelfRegistration", "Flow2_TrialApiProvision", "Flow3_Invited" };

        public static class Events
        {
            public const string UserJoined = "AM05_USER_JOINED";
            public const string UserMoved = "AM05_USER_MOVED";
            public const string UserLeft = "AM05_USER_LEFT";
            public const string UserSuspended = "AM05_USER_SUSPENDED";
            public const string UserReactivated = "AM05_USER_REACTIVATED";
            public const string InactivitySuspension = "AM05_INACTIVITY_SUSPENSION";
        }

        // Configuration
        public const int InactivitySuspensionDays = 90;
    }

    /// <summary>
    /// AM-06: Invitation Control and Integrity
    /// Ensures invitations cannot be abused or replayed.
    /// </summary>
    public static class AM06_InvitationControl
    {
        public const string ControlId = "AM-06";
        public const string Name = "Invitation Control and Integrity";
        public const string Objective = "Ensure invitations cannot be abused or replayed";
        public const string Owner = "Engineering";
        public const string Frequency = "Continuous";

        public static readonly string[] AppliesTo = { "Flow3_Invited" };

        public static class Events
        {
            public const string InviteCreated = "AM06_INVITE_CREATED";
            public const string InviteRevoked = "AM06_INVITE_REVOKED";
            public const string InviteAccepted = "AM06_INVITE_ACCEPTED";
            public const string InviteExpired = "AM06_INVITE_EXPIRED";
            public const string InviteResent = "AM06_INVITE_RESENT";
            public const string InviteRateLimited = "AM06_INVITE_RATE_LIMITED";
        }

        // Configuration
        public const int InviteTokenExpiryHours = 72;
        public const int MaxInvitesPerTenantPerDay = 50;
        public const int MaxResendPerInvite = 3;
    }

    /// <summary>
    /// AM-07: Registration and Provisioning Abuse Prevention
    /// Prevents bot signups, enumeration, and denial-of-service via user creation endpoints.
    /// </summary>
    public static class AM07_AbusePrevention
    {
        public const string ControlId = "AM-07";
        public const string Name = "Registration and Provisioning Abuse Prevention";
        public const string Objective = "Prevent bot signups, enumeration, and DoS via user creation endpoints";
        public const string Owner = "Platform Engineering / Security";
        public const string Frequency = "Continuous; monitored daily/weekly";

        public static readonly string[] AppliesTo = { "Flow1_SelfRegistration", "Flow2_TrialApiProvision" };

        public static class Events
        {
            public const string CaptchaVerified = "AM07_CAPTCHA_VERIFIED";
            public const string CaptchaFailed = "AM07_CAPTCHA_FAILED";
            public const string RateLimitTriggered = "AM07_RATE_LIMIT_TRIGGERED";
            public const string IpBlocked = "AM07_IP_BLOCKED";
            public const string IpUnblocked = "AM07_IP_UNBLOCKED";
            public const string SuspiciousActivity = "AM07_SUSPICIOUS_ACTIVITY";
            public const string AbuseScoreCalculated = "AM07_ABUSE_SCORE_CALCULATED";
        }
    }

    /// <summary>
    /// AM-08: Password and Recovery Controls
    /// Ensures authentication secrets are created and reset securely.
    /// </summary>
    public static class AM08_PasswordControls
    {
        public const string ControlId = "AM-08";
        public const string Name = "Password and Recovery Controls";
        public const string Objective = "Ensure authentication secrets are created and reset securely";
        public const string Owner = "IAM Engineering";
        public const string Frequency = "Continuous; reviewed annually";

        public static readonly string[] AppliesTo = { "Flow1_SelfRegistration", "Flow2_TrialApiProvision", "Flow3_Invited" };

        public static class Events
        {
            public const string PasswordSet = "AM08_PASSWORD_SET";
            public const string PasswordChanged = "AM08_PASSWORD_CHANGED";
            public const string PasswordResetRequested = "AM08_PASSWORD_RESET_REQUESTED";
            public const string PasswordResetCompleted = "AM08_PASSWORD_RESET_COMPLETED";
            public const string PasswordPolicyViolation = "AM08_PASSWORD_POLICY_VIOLATION";
            public const string AccountLocked = "AM08_ACCOUNT_LOCKED";
            public const string AccountUnlocked = "AM08_ACCOUNT_UNLOCKED";
        }

        // Configuration
        public const int PasswordHistoryCount = 5;
        public const int PasswordExpiryDays = 90;
        public const int LockoutThreshold = 5;
        public const int LockoutDurationMinutes = 30;
        public const int ResetTokenExpiryMinutes = 60;
    }

    /// <summary>
    /// AM-09: Trial Tenant Governance
    /// Ensures trial tenants are controlled and handled per policy.
    /// </summary>
    public static class AM09_TrialGovernance
    {
        public const string ControlId = "AM-09";
        public const string Name = "Trial Tenant Governance (Expiry and Data Handling)";
        public const string Objective = "Ensure trial tenants are controlled and handled per policy";
        public const string Owner = "Product + GRC + Engineering";
        public const string Frequency = "Continuous; reviewed quarterly";

        public static readonly string[] AppliesTo = { "Flow1_SelfRegistration", "Flow2_TrialApiProvision" };

        public static class Events
        {
            public const string TrialStarted = "AM09_TRIAL_STARTED";
            public const string TrialExtended = "AM09_TRIAL_EXTENDED";
            public const string TrialExpiryWarning = "AM09_TRIAL_EXPIRY_WARNING";
            public const string TrialExpired = "AM09_TRIAL_EXPIRED";
            public const string TrialConverted = "AM09_TRIAL_CONVERTED";
            public const string TrialDataArchived = "AM09_TRIAL_DATA_ARCHIVED";
            public const string TrialDataDeleted = "AM09_TRIAL_DATA_DELETED";
        }

        // Configuration
        public const int TrialDurationDays = 7;
        public const int DataRetentionDaysAfterExpiry = 30;
        public const int ExpiryWarningDays = 2;
    }

    /// <summary>
    /// AM-10: Audit Logging and Traceability
    /// Maintains complete, tamper-resistant evidence of access events.
    /// </summary>
    public static class AM10_AuditLogging
    {
        public const string ControlId = "AM-10";
        public const string Name = "Audit Logging and Traceability";
        public const string Objective = "Maintain complete, tamper-resistant evidence of access events";
        public const string Owner = "Platform Engineering / Security";
        public const string Frequency = "Continuous; tested quarterly";

        public static readonly string[] AppliesTo = { "Flow1_SelfRegistration", "Flow2_TrialApiProvision", "Flow3_Invited" };

        public static class Events
        {
            public const string AuditEventLogged = "AM10_AUDIT_EVENT_LOGGED";
            public const string AuditExportRequested = "AM10_AUDIT_EXPORT_REQUESTED";
            public const string AuditExportCompleted = "AM10_AUDIT_EXPORT_COMPLETED";
            public const string AuditRetentionApplied = "AM10_AUDIT_RETENTION_APPLIED";
        }

        // Configuration
        public const int AuditRetentionDays = 365 * 7; // 7 years for compliance
    }

    /// <summary>
    /// AM-11: Periodic Access Reviews
    /// Detects and remediates inappropriate access.
    /// </summary>
    public static class AM11_AccessReviews
    {
        public const string ControlId = "AM-11";
        public const string Name = "Periodic Access Reviews";
        public const string Objective = "Detect and remediate inappropriate access";
        public const string Owner = "Tenant Admin (primary), GRC (oversight)";
        public const string Frequency = "Quarterly";

        public static readonly string[] AppliesTo = { "Flow1_SelfRegistration", "Flow2_TrialApiProvision", "Flow3_Invited" };

        public static class Events
        {
            public const string ReviewScheduled = "AM11_REVIEW_SCHEDULED";
            public const string ReviewStarted = "AM11_REVIEW_STARTED";
            public const string ReviewItemApproved = "AM11_REVIEW_ITEM_APPROVED";
            public const string ReviewItemRevoked = "AM11_REVIEW_ITEM_REVOKED";
            public const string ReviewCompleted = "AM11_REVIEW_COMPLETED";
            public const string ReviewOverdue = "AM11_REVIEW_OVERDUE";
            public const string RemediationRequired = "AM11_REMEDIATION_REQUIRED";
        }

        // Review scope
        public static readonly string[] ReviewScope =
        {
            "AllTenantAdmins",
            "AllPrivilegedRoles",
            "InactiveUsers"
        };

        // Configuration
        public const int ReviewFrequencyDays = 90; // Quarterly
        public const int InactivityThresholdDays = 90;
    }

    /// <summary>
    /// AM-12: Separation of Duties for GRC Actions
    /// Reduces conflict-of-interest in compliance workflows.
    /// </summary>
    public static class AM12_SeparationOfDuties
    {
        public const string ControlId = "AM-12";
        public const string Name = "Separation of Duties for GRC Actions";
        public const string Objective = "Reduce conflict-of-interest in compliance workflows";
        public const string Owner = "Product + GRC";
        public const string Frequency = "Configured per tenant; reviewed annually";

        public static readonly string[] AppliesTo = { "Flow3_Invited" };

        public static class Events
        {
            public const string SoDRuleCreated = "AM12_SOD_RULE_CREATED";
            public const string SoDRuleModified = "AM12_SOD_RULE_MODIFIED";
            public const string SoDViolationDetected = "AM12_SOD_VIOLATION_DETECTED";
            public const string SoDViolationBlocked = "AM12_SOD_VIOLATION_BLOCKED";
            public const string SoDExceptionGranted = "AM12_SOD_EXCEPTION_GRANTED";
            public const string SoDExceptionExpired = "AM12_SOD_EXCEPTION_EXPIRED";
        }
    }

    /// <summary>
    /// Get all control IDs
    /// </summary>
    public static string[] GetAllControlIds() => new[]
    {
        AM01_IdentityProofing.ControlId,
        AM02_TrialProvisioning.ControlId,
        AM03_RBAC.ControlId,
        AM04_PrivilegedAccess.ControlId,
        AM05_JMLLifecycle.ControlId,
        AM06_InvitationControl.ControlId,
        AM07_AbusePrevention.ControlId,
        AM08_PasswordControls.ControlId,
        AM09_TrialGovernance.ControlId,
        AM10_AuditLogging.ControlId,
        AM11_AccessReviews.ControlId,
        AM12_SeparationOfDuties.ControlId
    };

    /// <summary>
    /// Get controls for a specific user creation flow
    /// </summary>
    public static string[] GetControlsForFlow(string flowId) => flowId switch
    {
        "Flow1_SelfRegistration" => new[] { "AM-01", "AM-03", "AM-07", "AM-08", "AM-09", "AM-10", "AM-11" },
        "Flow2_TrialApiProvision" => new[] { "AM-02", "AM-03", "AM-04", "AM-07", "AM-09", "AM-10", "AM-11" },
        "Flow3_Invited" => new[] { "AM-01", "AM-03", "AM-04", "AM-06", "AM-08", "AM-10", "AM-11", "AM-12" },
        _ => Array.Empty<string>()
    };
}

/// <summary>
/// User creation flow identifiers
/// </summary>
public static class UserCreationFlows
{
    public const string SelfRegistration = "Flow1_SelfRegistration";
    public const string TrialApiProvision = "Flow2_TrialApiProvision";
    public const string AdminInvitation = "Flow3_Invited";
}
