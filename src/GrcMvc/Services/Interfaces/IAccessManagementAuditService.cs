using GrcMvc.Models.Entities;
using GrcMvc.Models.Enums;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Service for logging Access Management audit events (AM-01 through AM-12).
    /// Provides immutable audit trail for GRC compliance requirements.
    /// </summary>
    public interface IAccessManagementAuditService
    {
        // AM-01: Identity Proofing and Account Activation
        Task LogUserCreatedAsync(Guid userId, string email, string creationMethod, Guid? tenantId, Guid? actorId, string? ipAddress = null);
        Task LogUserRegisteredAsync(Guid userId, string email, Guid tenantId, string? ipAddress = null);
        Task LogUserActivatedAsync(Guid userId, string email, Guid tenantId, string activationMethod, Guid? actorId = null);
        Task LogTrialSignupInitiatedAsync(string email, string companyName, string? ipAddress = null);
        Task LogTenantCreatedAsync(Guid tenantId, string tenantName, Guid? creatorUserId, string creationMethod);
        Task LogUserInvitedAsync(Guid tenantId, Guid invitedUserId, string invitedEmail, string role, Guid inviterId);
        Task LogVerificationSentAsync(Guid userId, string email, string verificationType);
        Task LogVerificationCompletedAsync(Guid userId, string email, string verificationType);
        Task LogVerificationExpiredAsync(Guid userId, string email, string verificationType);

        // AM-02: Secure Trial Provisioning
        Task LogProvisionRequestedAsync(string email, string? apiKeyId, string? ipAddress);
        Task LogProvisionCompletedAsync(Guid tenantId, Guid userId, string email, string? apiKeyId, string? ipAddress);
        Task LogProvisionDeniedAsync(string email, string reason, string? apiKeyId, string? ipAddress);
        Task LogApiKeyCreatedAsync(Guid apiKeyId, string keyName, Guid creatorId, string? allowedDomains, string? ipAddress);
        Task LogApiKeyRevokedAsync(Guid apiKeyId, string keyName, Guid creatorId, Guid revokedBy, string reason, string? ipAddress);

        // AM-03: Role-Based Access Control
        Task LogRoleAssignedAsync(Guid userId, string email, string role, Guid tenantId, Guid assignerId);
        Task LogRoleChangedAsync(Guid userId, string email, string previousRole, string newRole, Guid tenantId, Guid changerId);
        Task LogRoleRemovedAsync(Guid userId, string email, string role, Guid tenantId, Guid removerId);
        Task LogPrivilegeEscalationBlockedAsync(Guid userId, string email, string attemptedRole, string assignerRole, Guid assignerId, Guid tenantId);

        // AM-04: Privileged Access Safeguards
        Task LogMfaEnabledAsync(Guid userId, string email, string mfaType);
        Task LogMfaDisabledAsync(Guid userId, string email, Guid disabledBy);
        Task LogMfaVerifiedAsync(Guid userId, string email, string mfaType, string? ipAddress);
        Task LogMfaFailedAsync(Guid userId, Guid? tenantId, string mfaType, string? ipAddress);
        Task LogStepUpRequiredAsync(Guid userId, Guid? tenantId, string action, string? ipAddress);
        Task LogStepUpCompletedAsync(Guid userId, Guid? tenantId, string action, string method, string? ipAddress);

        // AM-05: Joiner/Mover/Leaver Lifecycle
        Task LogStatusChangedAsync(Guid userId, string email, UserStatus previousStatus, UserStatus newStatus, Guid tenantId, Guid? changedBy, string? reason);
        Task LogUserSuspendedAsync(Guid userId, string email, Guid tenantId, Guid suspendedBy, string reason);
        Task LogUserReactivatedAsync(Guid userId, string email, Guid tenantId, Guid reactivatedBy);
        Task LogUserDeprovisionedAsync(Guid userId, string email, Guid tenantId, Guid deprovisionedBy, string reason);
        Task LogInactivityWarningAsync(Guid userId, string email, Guid tenantId, int inactiveDays);
        Task LogInactivitySuspensionAsync(Guid userId, string email, Guid tenantId, int inactiveDays);
        Task LogSessionRevokedAsync(Guid userId, Guid sessionId, Guid? revokedBy, string reason);
        Task LogAllSessionsRevokedAsync(Guid userId, Guid revokedBy, string reason);

        // AM-06: Invitation Control
        Task LogInvitationCreatedAsync(Guid invitationId, Guid tenantId, string invitedEmail, string role, Guid inviterId, DateTime expiresAt);
        Task LogInvitationSentAsync(Guid invitationId, string invitedEmail);
        Task LogInvitationResentAsync(Guid invitationId, string invitedEmail, Guid resentBy, int resendCount);
        Task LogInvitationAcceptedAsync(Guid invitationId, Guid userId, string email, Guid tenantId);
        Task LogInvitationExpiredAsync(Guid invitationId, string invitedEmail, Guid tenantId);
        Task LogInvitationRevokedAsync(Guid invitationId, string invitedEmail, Guid tenantId, Guid revokedBy, string reason);
        Task LogInvitationLimitExceededAsync(Guid tenantId, Guid inviterId, int currentCount, int limit);

        // AM-07: Abuse Prevention
        Task LogRateLimitExceededAsync(string endpoint, string? ipAddress, string? userId, int requestCount, int limit);
        Task LogSuspiciousActivityAsync(string activityType, string description, string? ipAddress, string? userId);
        Task LogIpBlockedAsync(string ipAddress, string reason, int duration);

        // AM-08: Password and Recovery
        Task LogPasswordSetAsync(Guid userId, string email, string setMethod);
        Task LogPasswordChangedAsync(Guid userId, string email, Guid? changedBy);
        Task LogPasswordResetRequestedAsync(string email, string? ipAddress);
        Task LogPasswordResetCompletedAsync(Guid userId, string email, string? ipAddress);
        Task LogPasswordResetFailedAsync(Guid userId, Guid tenantId, string reason, string? errorCode, string? ipAddress);
        Task LogPasswordPolicyViolationAsync(string email, string violationType);
        Task LogAccountLockedAsync(Guid userId, string email, string reason, int failedAttempts);
        Task LogAccountUnlockedAsync(Guid userId, string email, Guid? unlockedBy);

        // AM-09: Trial Tenant Governance
        Task LogTrialCreatedAsync(Guid tenantId, string tenantName, Guid userId, DateTime expiresAt);
        Task LogTrialExtendedAsync(Guid extendedBy, Guid tenantId, int extensionDays, DateTime newExpiry, string? ipAddress);
        Task LogTrialExpiryWarningAsync(Guid tenantId, int daysRemaining);
        Task LogTrialExpiredAsync(Guid tenantId);
        Task LogTrialConvertedAsync(Guid convertedBy, Guid tenantId, string planType, string? subscriptionId, string? ipAddress);
        Task LogTrialDataArchivedAsync(Guid? userId, Guid tenantId, string? ipAddress);
        Task LogTrialDataDeletedAsync(Guid? userId, Guid tenantId, string? ipAddress);

        // AM-10: Audit Export
        Task LogAuditExportRequestedAsync(Guid tenantId, Guid requestedBy, string exportType, DateTime fromDate, DateTime toDate);
        Task LogAuditExportCompletedAsync(Guid tenantId, Guid requestedBy, string exportType, int recordCount);

        // AM-11: Access Reviews
        Task LogAccessReviewInitiatedAsync(Guid reviewId, Guid tenantId, string reviewType, Guid initiatedBy, int userCount, string? ipAddress);
        Task LogAccessReviewItemCertifiedAsync(Guid reviewId, Guid userId, string currentRole, Guid reviewedBy, Guid tenantId, string? ipAddress);
        Task LogAccessReviewItemRevokedAsync(Guid reviewId, Guid userId, string previousRole, Guid reviewedBy, Guid tenantId, string? ipAddress);
        Task LogAccessReviewItemModifiedAsync(Guid reviewId, Guid userId, string previousRole, string newRole, Guid reviewedBy, Guid tenantId, string? ipAddress);
        Task LogAccessReviewCompletedAsync(Guid reviewId, Guid tenantId, Guid completedBy, int certifiedCount, int revokedCount, int modifiedCount, string? ipAddress);
        Task LogAccessReviewOverdueAsync(Guid reviewId, Guid tenantId, int daysPastDue, string? ipAddress);

        // AM-12: Separation of Duties
        Task LogSoDViolationDetectedAsync(Guid userId, Guid tenantId, string conflictType, string role1, string role2, string? ipAddress);
        Task LogSoDViolationBlockedAsync(Guid userId, Guid tenantId, string conflictType, string role1, string role2, string? ipAddress);
        Task LogSoDOverrideRequestedAsync(Guid userId, string email, string action1, string action2, Guid tenantId, string justification);
        Task LogSoDOverrideApprovedAsync(Guid userId, string email, string action1, string action2, Guid tenantId, Guid approvedBy);
        Task LogSoDOverrideDeniedAsync(Guid userId, string email, string action1, string action2, Guid tenantId, Guid deniedBy, string reason);

        // Query methods
        Task<IEnumerable<AccessManagementAuditEvent>> GetEventsByTenantAsync(Guid tenantId, DateTime? from = null, DateTime? to = null, int limit = 100);
        Task<IEnumerable<AccessManagementAuditEvent>> GetEventsByUserAsync(Guid userId, DateTime? from = null, DateTime? to = null, int limit = 100);
        Task<IEnumerable<AccessManagementAuditEvent>> GetEventsByCorrelationIdAsync(string correlationId);
        Task<IEnumerable<AccessManagementAuditEvent>> GetEventsByControlNumberAsync(string controlNumber, Guid? tenantId = null, DateTime? from = null, DateTime? to = null, int limit = 100);
        Task<IEnumerable<AccessManagementAuditEvent>> GetHighSeverityEventsAsync(Guid? tenantId = null, DateTime? from = null, int limit = 100);
    }
}
