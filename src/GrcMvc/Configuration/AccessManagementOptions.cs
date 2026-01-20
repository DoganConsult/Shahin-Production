namespace GrcMvc.Configuration
{
    /// <summary>
    /// Root configuration options for Access Management controls.
    /// </summary>
    public class AccessManagementOptions
    {
        public const string SectionName = "AccessManagement";

        public InvitationOptions Invitation { get; set; } = new();
        public TrialGovernanceOptions TrialGovernance { get; set; } = new();
        public MfaEnforcementOptions MfaEnforcement { get; set; } = new();
        public PasswordPolicyOptions PasswordPolicy { get; set; } = new();
        public InactivitySuspensionOptions InactivitySuspension { get; set; } = new();
        public RateLimitingOptions RateLimiting { get; set; } = new();
        public AccessReviewOptions AccessReview { get; set; } = new();
    }

    /// <summary>
    /// AM-06: Invitation Control configuration.
    /// </summary>
    public class InvitationOptions
    {
        /// <summary>
        /// Default invitation expiry in hours (default: 72 = 3 days).
        /// </summary>
        public int DefaultExpiryHours { get; set; } = 72;

        /// <summary>
        /// Maximum number of times an invitation can be resent.
        /// </summary>
        public int MaxResends { get; set; } = 3;

        /// <summary>
        /// Maximum invitations a tenant can send per day.
        /// </summary>
        public int MaxInvitesPerTenantPerDay { get; set; } = 50;

        /// <summary>
        /// Whether to require email verification before sending invitations.
        /// </summary>
        public bool RequireVerifiedSender { get; set; } = true;
    }

    /// <summary>
    /// AM-09: Trial Tenant Governance configuration.
    /// </summary>
    public class TrialGovernanceOptions
    {
        /// <summary>
        /// Default trial duration in days.
        /// </summary>
        public int DefaultTrialDays { get; set; } = 7;

        /// <summary>
        /// Grace period after trial expiry before data action (days).
        /// </summary>
        public int ExpiryGracePeriodDays { get; set; } = 7;

        /// <summary>
        /// Data retention period after expiry (days).
        /// </summary>
        public int DataRetentionDays { get; set; } = 30;

        /// <summary>
        /// Action to take on trial data after retention: Archive, Delete, or Anonymize.
        /// </summary>
        public string DataAction { get; set; } = "Archive";

        /// <summary>
        /// Days before expiry to send warning notification.
        /// </summary>
        public int ExpiryWarningDays { get; set; } = 2;

        /// <summary>
        /// Maximum team members allowed in trial.
        /// </summary>
        public int MaxTrialTeamMembers { get; set; } = 5;

        /// <summary>
        /// Whether one-time trial extension is allowed.
        /// </summary>
        public bool AllowExtension { get; set; } = true;

        /// <summary>
        /// Extension duration in days (if allowed).
        /// </summary>
        public int ExtensionDays { get; set; } = 7;
    }

    /// <summary>
    /// AM-04: MFA Enforcement configuration.
    /// </summary>
    public class MfaEnforcementOptions
    {
        /// <summary>
        /// Require MFA for TenantAdmin role.
        /// </summary>
        public bool RequireForTenantAdmin { get; set; } = true;

        /// <summary>
        /// Require step-up authentication for privileged actions.
        /// </summary>
        public bool RequireForPrivilegedActions { get; set; } = true;

        /// <summary>
        /// List of roles that require MFA.
        /// </summary>
        public string[] RequiredForRoles { get; set; } = new[] { "TenantAdmin", "PlatformAdmin" };

        /// <summary>
        /// List of actions that require step-up authentication.
        /// </summary>
        public string[] StepUpActions { get; set; } = new[]
        {
            "role.change",
            "admin.invite",
            "user.suspend",
            "user.deprovision",
            "api.key.create"
        };

        /// <summary>
        /// Duration (in minutes) that step-up auth remains valid.
        /// </summary>
        public int StepUpValidityMinutes { get; set; } = 15;
    }

    /// <summary>
    /// AM-08: Password Policy configuration.
    /// </summary>
    public class PasswordPolicyOptions
    {
        /// <summary>
        /// Minimum password length.
        /// </summary>
        public int MinLength { get; set; } = 12;

        /// <summary>
        /// Require at least one digit.
        /// </summary>
        public bool RequireDigit { get; set; } = true;

        /// <summary>
        /// Require at least one uppercase letter.
        /// </summary>
        public bool RequireUppercase { get; set; } = true;

        /// <summary>
        /// Require at least one lowercase letter.
        /// </summary>
        public bool RequireLowercase { get; set; } = true;

        /// <summary>
        /// Require at least one non-alphanumeric character.
        /// </summary>
        public bool RequireNonAlphanumeric { get; set; } = true;

        /// <summary>
        /// Check password against known breached password lists (HaveIBeenPwned).
        /// </summary>
        public bool CheckBreachedPasswords { get; set; } = false;

        /// <summary>
        /// Password reset token TTL in minutes.
        /// </summary>
        public int ResetTokenTtlMinutes { get; set; } = 60;

        /// <summary>
        /// Maximum reset requests per email per hour.
        /// </summary>
        public int MaxResetRequestsPerHour { get; set; } = 3;

        /// <summary>
        /// Password expiry in days (0 = no expiry).
        /// </summary>
        public int ExpiryDays { get; set; } = 90;

        /// <summary>
        /// Number of previous passwords to prevent reuse.
        /// </summary>
        public int PasswordHistoryCount { get; set; } = 5;

        /// <summary>
        /// Base URL for password reset links.
        /// </summary>
        public string BaseUrl { get; set; } = "https://portal.shahin-ai.com";
    }

    /// <summary>
    /// AM-05: Inactivity Suspension configuration.
    /// </summary>
    public class InactivitySuspensionOptions
    {
        /// <summary>
        /// Enable automatic inactivity suspension.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Days of inactivity before suspension.
        /// </summary>
        public int InactiveDays { get; set; } = 90;

        /// <summary>
        /// Days before suspension to send warning.
        /// </summary>
        public int WarningDays { get; set; } = 7;

        /// <summary>
        /// Allow self-reactivation on login attempt.
        /// </summary>
        public bool AllowSelfReactivation { get; set; } = true;
    }

    /// <summary>
    /// AM-07: Rate Limiting configuration.
    /// </summary>
    public class RateLimitingOptions
    {
        /// <summary>
        /// Registration endpoint: requests per time window.
        /// </summary>
        public int RegistrationLimit { get; set; } = 5;

        /// <summary>
        /// Registration endpoint: time window in minutes.
        /// </summary>
        public int RegistrationWindowMinutes { get; set; } = 10;

        /// <summary>
        /// Trial signup endpoint: requests per hour.
        /// </summary>
        public int TrialSignupLimitPerHour { get; set; } = 10;

        /// <summary>
        /// Trial provision endpoint: requests per hour per API key.
        /// </summary>
        public int TrialProvisionLimitPerHour { get; set; } = 5;

        /// <summary>
        /// Password reset endpoint: requests per hour per email.
        /// </summary>
        public int PasswordResetLimitPerHour { get; set; } = 3;

        // CAPTCHA removed - no longer needed
    }

    /// <summary>
    /// AM-11: Access Review configuration.
    /// </summary>
    public class AccessReviewOptions
    {
        /// <summary>
        /// Enable periodic access reviews.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Default review frequency: Monthly, Quarterly, Annually.
        /// </summary>
        public string DefaultFrequency { get; set; } = "Quarterly";

        /// <summary>
        /// Days to complete a review before it's considered overdue.
        /// </summary>
        public int CompletionDeadlineDays { get; set; } = 14;

        /// <summary>
        /// Include users inactive for N+ days in review.
        /// </summary>
        public int InactiveUserThresholdDays { get; set; } = 90;

        /// <summary>
        /// Automatically include all privileged roles in review.
        /// </summary>
        public bool AutoIncludePrivilegedRoles { get; set; } = true;

        /// <summary>
        /// Roles that are always included in access reviews.
        /// </summary>
        public string[] AlwaysReviewRoles { get; set; } = new[]
        {
            "TenantAdmin",
            "PlatformAdmin",
            "ComplianceManager",
            "AuditManager"
        };
    }
}
