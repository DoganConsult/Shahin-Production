using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Models.Enums;
using GrcMvc.Services.Interfaces;
using GrcMvc.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Full implementation of AccessManagementAuditService using database storage.
    /// Replaces AccessManagementAuditServiceStub with proper audit event persistence.
    /// </summary>
    public class AccessManagementAuditService : IAccessManagementAuditService, IScopedDependency
    {
        private readonly GrcDbContext _dbContext;
        private readonly ILogger<AccessManagementAuditService> _logger;
        private readonly IHttpContextAccessor? _httpContextAccessor;

        public AccessManagementAuditService(
            GrcDbContext dbContext,
            ILogger<AccessManagementAuditService> logger,
            IHttpContextAccessor? httpContextAccessor = null)
        {
            _dbContext = dbContext;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        private string? GetIpAddress()
        {
            return _httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString();
        }

        private string? GetUserAgent()
        {
            return _httpContextAccessor?.HttpContext?.Request?.Headers?["User-Agent"].ToString();
        }

        private async Task SaveEventAsync(AccessManagementAuditEvent auditEvent)
        {
            try
            {
                _dbContext.AccessManagementAuditEvents.Add(auditEvent);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save access management audit event: {EventType}", auditEvent.EventType);
                // Don't throw - audit failures shouldn't break the application
            }
        }

        #region AM-01: Identity Proofing

        public async Task LogUserCreatedAsync(Guid userId, string email, string creationMethod, Guid? tenantId, Guid? actorId, string? ipAddress = null)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM01_USER_CREATED,
                $"User created: {email} via {creationMethod}",
                tenantId,
                actorId,
                userId);

            auditEvent.ActorEmail = email;
            auditEvent.TargetEmail = email;
            auditEvent.AffectedEntityType = "User";
            auditEvent.AffectedEntityId = userId;
            auditEvent.IpAddress = ipAddress ?? GetIpAddress();
            auditEvent.UserAgent = GetUserAgent();
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { CreationMethod = creationMethod });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-01: User created: {UserId}, {Email}, Method: {Method}", userId, email, creationMethod);
        }

        public async Task LogUserRegisteredAsync(Guid userId, string email, Guid tenantId, string? ipAddress = null)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM01_USER_REGISTERED,
                $"User registered: {email}",
                tenantId,
                userId,
                userId);

            auditEvent.ActorEmail = email;
            auditEvent.TargetEmail = email;
            auditEvent.AffectedEntityType = "User";
            auditEvent.AffectedEntityId = userId;
            auditEvent.IpAddress = ipAddress ?? GetIpAddress();
            auditEvent.UserAgent = GetUserAgent();

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-01: User registered: {UserId}, {Email}, Tenant: {TenantId}", userId, email, tenantId);
        }

        public async Task LogUserActivatedAsync(Guid userId, string email, Guid tenantId, string activationMethod, Guid? actorId = null)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM01_USER_ACTIVATED,
                $"User activated: {email} via {activationMethod}",
                tenantId,
                actorId,
                userId);

            auditEvent.ActorEmail = email;
            auditEvent.TargetEmail = email;
            auditEvent.AffectedEntityType = "User";
            auditEvent.AffectedEntityId = userId;
            auditEvent.NewValue = activationMethod;
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { ActivationMethod = activationMethod });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-01: User activated: {UserId}, {Email}, Method: {Method}", userId, email, activationMethod);
        }

        public async Task LogTrialSignupInitiatedAsync(string email, string companyName, string? ipAddress = null)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM01_TRIAL_SIGNUP_INITIATED,
                $"Trial signup initiated: {email} for {companyName}",
                null,
                null,
                null);

            auditEvent.TargetEmail = email;
            auditEvent.AffectedEntityType = "TrialSignup";
            auditEvent.IpAddress = ipAddress ?? GetIpAddress();
            auditEvent.UserAgent = GetUserAgent();
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { CompanyName = companyName, Email = email });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-01: Trial signup initiated: {Email}, {Company}", email, companyName);
        }

        public async Task LogTenantCreatedAsync(Guid tenantId, string tenantName, Guid? creatorUserId, string creationMethod)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM01_TENANT_CREATED,
                $"Tenant created: {tenantName} via {creationMethod}",
                tenantId,
                creatorUserId,
                null);

            auditEvent.AffectedEntityType = "Tenant";
            auditEvent.AffectedEntityId = tenantId;
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { TenantName = tenantName, CreationMethod = creationMethod });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-01: Tenant created: {TenantId}, {Name}, Method: {Method}", tenantId, tenantName, creationMethod);
        }

        public async Task LogUserInvitedAsync(Guid tenantId, Guid invitedUserId, string invitedEmail, string role, Guid inviterId)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM01_USER_INVITED,
                $"User invited: {invitedEmail} with role {role}",
                tenantId,
                inviterId,
                invitedUserId);

            auditEvent.TargetEmail = invitedEmail;
            auditEvent.AffectedEntityType = "UserInvitation";
            auditEvent.AffectedEntityId = invitedUserId;
            auditEvent.NewValue = role;
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { Role = role, InvitedEmail = invitedEmail });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-01: User invited: {Email}, Role: {Role}, by {InviterId}", invitedEmail, role, inviterId);
        }

        public async Task LogVerificationSentAsync(Guid userId, string email, string verificationType)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM01_VERIFICATION_SENT,
                $"Verification sent: {verificationType} to {email}",
                null,
                null,
                userId);

            auditEvent.TargetEmail = email;
            auditEvent.AffectedEntityType = "Verification";
            auditEvent.NewValue = verificationType;
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { VerificationType = verificationType });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-01: Verification sent: {UserId}, {Email}, Type: {Type}", userId, email, verificationType);
        }

        public async Task LogVerificationCompletedAsync(Guid userId, string email, string verificationType)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM01_VERIFICATION_COMPLETED,
                $"Verification completed: {verificationType} for {email}",
                null,
                userId,
                userId);

            auditEvent.TargetEmail = email;
            auditEvent.AffectedEntityType = "Verification";
            auditEvent.NewValue = verificationType;
            auditEvent.Success = true;
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { VerificationType = verificationType });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-01: Verification completed: {UserId}, {Email}, Type: {Type}", userId, email, verificationType);
        }

        public async Task LogVerificationExpiredAsync(Guid userId, string email, string verificationType)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM01_VERIFICATION_EXPIRED,
                $"Verification expired: {verificationType} for {email}",
                null,
                null,
                userId);

            auditEvent.TargetEmail = email;
            auditEvent.AffectedEntityType = "Verification";
            auditEvent.NewValue = verificationType;
            auditEvent.Success = false;
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { VerificationType = verificationType });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-01: Verification expired: {UserId}, {Email}, Type: {Type}", userId, email, verificationType);
        }

        #endregion

        #region AM-02: Secure Trial Provisioning

        public async Task LogProvisionRequestedAsync(string email, string? apiKeyId, string? ipAddress)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM02_PROVISION_REQUESTED,
                $"Provision requested: {email}",
                null,
                null,
                null);

            auditEvent.TargetEmail = email;
            auditEvent.AffectedEntityType = "TrialProvision";
            auditEvent.IpAddress = ipAddress ?? GetIpAddress();
            auditEvent.UserAgent = GetUserAgent();
            if (Guid.TryParse(apiKeyId, out var apiKeyGuid))
            {
                auditEvent.ApiKeyId = apiKeyGuid;
            }
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { Email = email, ApiKeyId = apiKeyId });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-02: Provision requested: {Email}", email);
        }

        public async Task LogProvisionCompletedAsync(Guid tenantId, Guid userId, string email, string? apiKeyId, string? ipAddress)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM02_PROVISION_COMPLETED,
                $"Provision completed: Tenant {tenantId}, User {userId}",
                tenantId,
                userId,
                userId);

            auditEvent.TargetEmail = email;
            auditEvent.AffectedEntityType = "TrialProvision";
            auditEvent.AffectedEntityId = tenantId;
            auditEvent.IpAddress = ipAddress ?? GetIpAddress();
            auditEvent.Success = true;
            if (Guid.TryParse(apiKeyId, out var apiKeyGuid))
            {
                auditEvent.ApiKeyId = apiKeyGuid;
            }
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { TenantId = tenantId, UserId = userId, Email = email });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-02: Provision completed: Tenant {TenantId}, User {UserId}", tenantId, userId);
        }

        public async Task LogProvisionDeniedAsync(string email, string reason, string? apiKeyId, string? ipAddress)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM02_PROVISION_DENIED,
                $"Provision denied: {email} - {reason}",
                null,
                null,
                null);

            auditEvent.TargetEmail = email;
            auditEvent.AffectedEntityType = "TrialProvision";
            auditEvent.IpAddress = ipAddress ?? GetIpAddress();
            auditEvent.Success = false;
            auditEvent.ErrorMessage = reason;
            if (Guid.TryParse(apiKeyId, out var apiKeyGuid))
            {
                auditEvent.ApiKeyId = apiKeyGuid;
            }
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { Email = email, Reason = reason });

            await SaveEventAsync(auditEvent);
            _logger.LogWarning("AM-02: Provision denied: {Email}, Reason: {Reason}", email, reason);
        }

        public async Task LogApiKeyCreatedAsync(Guid apiKeyId, string keyName, Guid creatorId, string? allowedDomains, string? ipAddress)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM02_API_KEY_CREATED,
                $"API key created: {keyName}",
                null,
                creatorId,
                null);

            auditEvent.AffectedEntityType = "ApiKey";
            auditEvent.AffectedEntityId = apiKeyId;
            auditEvent.ApiKeyId = apiKeyId;
            auditEvent.IpAddress = ipAddress ?? GetIpAddress();
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { KeyName = keyName, AllowedDomains = allowedDomains });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-02: API key created: {KeyId}, {Name}, by {CreatorId}", apiKeyId, keyName, creatorId);
        }

        public async Task LogApiKeyRevokedAsync(Guid apiKeyId, string keyName, Guid creatorId, Guid revokedBy, string reason, string? ipAddress)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM02_API_KEY_REVOKED,
                $"API key revoked: {keyName} - {reason}",
                null,
                revokedBy,
                null);

            auditEvent.AffectedEntityType = "ApiKey";
            auditEvent.AffectedEntityId = apiKeyId;
            auditEvent.ApiKeyId = apiKeyId;
            auditEvent.IpAddress = ipAddress ?? GetIpAddress();
            auditEvent.PreviousValue = "Active";
            auditEvent.NewValue = "Revoked";
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { KeyName = keyName, Reason = reason, CreatorId = creatorId });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-02: API key revoked: {KeyId}, {Name}, by {RevokedBy}", apiKeyId, keyName, revokedBy);
        }

        #endregion

        // Continue with remaining methods... (Due to length, I'll create a continuation)
        // The pattern is the same for all remaining methods

        #region Query Methods

        public async Task<IEnumerable<AccessManagementAuditEvent>> GetEventsByTenantAsync(Guid tenantId, DateTime? from = null, DateTime? to = null, int limit = 100)
        {
            var query = _dbContext.AccessManagementAuditEvents
                .Where(e => e.TenantId == tenantId);

            if (from.HasValue)
                query = query.Where(e => e.Timestamp >= from.Value);

            if (to.HasValue)
                query = query.Where(e => e.Timestamp <= to.Value);

            return await query
                .OrderByDescending(e => e.Timestamp)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<AccessManagementAuditEvent>> GetEventsByUserAsync(Guid userId, DateTime? from = null, DateTime? to = null, int limit = 100)
        {
            var query = _dbContext.AccessManagementAuditEvents
                .Where(e => e.ActorUserId == userId || e.TargetUserId == userId);

            if (from.HasValue)
                query = query.Where(e => e.Timestamp >= from.Value);

            if (to.HasValue)
                query = query.Where(e => e.Timestamp <= to.Value);

            return await query
                .OrderByDescending(e => e.Timestamp)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<AccessManagementAuditEvent>> GetEventsByCorrelationIdAsync(string correlationId)
        {
            return await _dbContext.AccessManagementAuditEvents
                .Where(e => e.CorrelationId == correlationId)
                .OrderByDescending(e => e.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AccessManagementAuditEvent>> GetEventsByControlNumberAsync(string controlNumber, Guid? tenantId = null, DateTime? from = null, DateTime? to = null, int limit = 100)
        {
            var query = _dbContext.AccessManagementAuditEvents
                .Where(e => e.ControlNumber == controlNumber);

            if (tenantId.HasValue)
                query = query.Where(e => e.TenantId == tenantId);

            if (from.HasValue)
                query = query.Where(e => e.Timestamp >= from.Value);

            if (to.HasValue)
                query = query.Where(e => e.Timestamp <= to.Value);

            return await query
                .OrderByDescending(e => e.Timestamp)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<AccessManagementAuditEvent>> GetHighSeverityEventsAsync(Guid? tenantId = null, DateTime? from = null, int limit = 100)
        {
            var query = _dbContext.AccessManagementAuditEvents
                .Where(e => e.Severity == "Critical" || e.Severity == "High");

            if (tenantId.HasValue)
                query = query.Where(e => e.TenantId == tenantId);

            if (from.HasValue)
                query = query.Where(e => e.Timestamp >= from.Value);

            return await query
                .OrderByDescending(e => e.Timestamp)
                .Take(limit)
                .ToListAsync();
        }

        #endregion

        #region AM-03: RBAC

        public async Task LogRoleAssignedAsync(Guid userId, string email, string role, Guid tenantId, Guid assignerId)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM03_ROLE_ASSIGNED,
                $"Role assigned: {role} to {email}",
                tenantId,
                assignerId,
                userId);

            auditEvent.TargetEmail = email;
            auditEvent.AffectedEntityType = "Role";
            auditEvent.NewValue = role;
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { Role = role, UserEmail = email });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-03: Role assigned: User {UserId}, Role {Role}", userId, role);
        }

        public async Task LogRoleChangedAsync(Guid userId, string email, string previousRole, string newRole, Guid tenantId, Guid changerId)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM03_ROLE_CHANGED,
                $"Role changed: {previousRole} -> {newRole} for {email}",
                tenantId,
                changerId,
                userId);

            auditEvent.TargetEmail = email;
            auditEvent.AffectedEntityType = "Role";
            auditEvent.PreviousValue = previousRole;
            auditEvent.NewValue = newRole;
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { PreviousRole = previousRole, NewRole = newRole });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-03: Role changed: User {UserId}, {OldRole} -> {NewRole}", userId, previousRole, newRole);
        }

        public async Task LogRoleRemovedAsync(Guid userId, string email, string role, Guid tenantId, Guid removerId)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM03_ROLE_REMOVED,
                $"Role removed: {role} from {email}",
                tenantId,
                removerId,
                userId);

            auditEvent.TargetEmail = email;
            auditEvent.AffectedEntityType = "Role";
            auditEvent.PreviousValue = role;
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { Role = role });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-03: Role removed: User {UserId}, Role {Role}", userId, role);
        }

        public async Task LogPrivilegeEscalationBlockedAsync(Guid userId, string email, string attemptedRole, string assignerRole, Guid assignerId, Guid tenantId)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM03_PRIVILEGE_ESCALATION_BLOCKED,
                $"Privilege escalation blocked: {email} attempted {attemptedRole}",
                tenantId,
                assignerId,
                userId);

            auditEvent.TargetEmail = email;
            auditEvent.AffectedEntityType = "Role";
            auditEvent.Success = false;
            auditEvent.ErrorMessage = $"Attempted role {attemptedRole} blocked by assigner role {assignerRole}";
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { AttemptedRole = attemptedRole, AssignerRole = assignerRole });

            await SaveEventAsync(auditEvent);
            _logger.LogWarning("AM-03: Privilege escalation blocked: User {UserId} attempted {Role}", userId, attemptedRole);
        }

        #endregion

        #region AM-04: Privileged Access

        public async Task LogMfaEnabledAsync(Guid userId, string email, string mfaType)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM04_MFA_ENABLED,
                $"MFA enabled: {mfaType} for {email}",
                null,
                userId,
                userId);

            auditEvent.TargetEmail = email;
            auditEvent.AffectedEntityType = "MFA";
            auditEvent.NewValue = mfaType;
            auditEvent.MfaVerified = true;
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { MfaType = mfaType });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-04: MFA enabled: User {UserId}, Type {Type}", userId, mfaType);
        }

        public async Task LogMfaDisabledAsync(Guid userId, string email, Guid disabledBy)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM04_MFA_DISABLED,
                $"MFA disabled for {email}",
                null,
                disabledBy,
                userId);

            auditEvent.TargetEmail = email;
            auditEvent.AffectedEntityType = "MFA";
            auditEvent.PreviousValue = "Enabled";
            auditEvent.NewValue = "Disabled";
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { DisabledBy = disabledBy });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-04: MFA disabled: User {UserId}, by {DisabledBy}", userId, disabledBy);
        }

        public async Task LogMfaVerifiedAsync(Guid userId, string email, string mfaType, string? ipAddress)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM04_MFA_VERIFIED,
                $"MFA verified: {mfaType} for {email}",
                null,
                userId,
                userId);

            auditEvent.TargetEmail = email;
            auditEvent.AffectedEntityType = "MFA";
            auditEvent.IpAddress = ipAddress ?? GetIpAddress();
            auditEvent.MfaVerified = true;
            auditEvent.Success = true;
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { MfaType = mfaType });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-04: MFA verified: User {UserId}, Type {Type}", userId, mfaType);
        }

        public async Task LogMfaFailedAsync(Guid userId, Guid? tenantId, string mfaType, string? ipAddress)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM04_MFA_FAILED,
                $"MFA verification failed: {mfaType}",
                tenantId,
                userId,
                userId);

            auditEvent.AffectedEntityType = "MFA";
            auditEvent.IpAddress = ipAddress ?? GetIpAddress();
            auditEvent.MfaVerified = false;
            auditEvent.Success = false;
            auditEvent.ErrorMessage = "MFA verification failed";
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { MfaType = mfaType });

            await SaveEventAsync(auditEvent);
            _logger.LogWarning("AM-04: MFA failed: User {UserId}, Type {Type}", userId, mfaType);
        }

        public async Task LogStepUpRequiredAsync(Guid userId, Guid? tenantId, string action, string? ipAddress)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM04_STEPUP_REQUIRED,
                $"Step-up authentication required for action: {action}",
                tenantId,
                userId,
                userId);

            auditEvent.AffectedEntityType = "StepUpAuth";
            auditEvent.IpAddress = ipAddress ?? GetIpAddress();
            auditEvent.NewValue = action;
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { Action = action });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-04: Step-up required: User {UserId}, Action {Action}", userId, action);
        }

        public async Task LogStepUpCompletedAsync(Guid userId, Guid? tenantId, string action, string method, string? ipAddress)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM04_STEPUP_COMPLETED,
                $"Step-up authentication completed: {method} for action {action}",
                tenantId,
                userId,
                userId);

            auditEvent.AffectedEntityType = "StepUpAuth";
            auditEvent.IpAddress = ipAddress ?? GetIpAddress();
            auditEvent.NewValue = method;
            auditEvent.Success = true;
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { Action = action, Method = method });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-04: Step-up completed: User {UserId}, Action {Action}", userId, action);
        }

        #endregion

        #region AM-05: Lifecycle

        public async Task LogStatusChangedAsync(Guid userId, string email, UserStatus previousStatus, UserStatus newStatus, Guid tenantId, Guid? changedBy, string? reason)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM05_STATUS_CHANGED,
                $"User status changed: {previousStatus} -> {newStatus} for {email}",
                tenantId,
                changedBy,
                userId);

            auditEvent.TargetEmail = email;
            auditEvent.AffectedEntityType = "User";
            auditEvent.PreviousValue = previousStatus.ToString();
            auditEvent.NewValue = newStatus.ToString();
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { PreviousStatus = previousStatus.ToString(), NewStatus = newStatus.ToString(), Reason = reason });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-05: Status changed: User {UserId}, {OldStatus} -> {NewStatus}", userId, previousStatus, newStatus);
        }

        public async Task LogUserSuspendedAsync(Guid userId, string email, Guid tenantId, Guid suspendedBy, string reason)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM05_USER_SUSPENDED,
                $"User suspended: {email} - {reason}",
                tenantId,
                suspendedBy,
                userId);

            auditEvent.TargetEmail = email;
            auditEvent.AffectedEntityType = "User";
            auditEvent.PreviousValue = "Active";
            auditEvent.NewValue = "Suspended";
            auditEvent.ErrorMessage = reason;
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { Reason = reason });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-05: User suspended: {UserId}, Reason: {Reason}", userId, reason);
        }

        public async Task LogUserReactivatedAsync(Guid userId, string email, Guid tenantId, Guid reactivatedBy)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM05_USER_REACTIVATED,
                $"User reactivated: {email}",
                tenantId,
                reactivatedBy,
                userId);

            auditEvent.TargetEmail = email;
            auditEvent.AffectedEntityType = "User";
            auditEvent.PreviousValue = "Suspended";
            auditEvent.NewValue = "Active";
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { ReactivatedBy = reactivatedBy });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-05: User reactivated: {UserId}", userId);
        }

        public async Task LogUserDeprovisionedAsync(Guid userId, string email, Guid tenantId, Guid deprovisionedBy, string reason)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM05_USER_DEPROVISIONED,
                $"User deprovisioned: {email} - {reason}",
                tenantId,
                deprovisionedBy,
                userId);

            auditEvent.TargetEmail = email;
            auditEvent.AffectedEntityType = "User";
            auditEvent.PreviousValue = "Active";
            auditEvent.NewValue = "Deprovisioned";
            auditEvent.ErrorMessage = reason;
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { Reason = reason });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-05: User deprovisioned: {UserId}, Reason: {Reason}", userId, reason);
        }

        public async Task LogInactivityWarningAsync(Guid userId, string email, Guid tenantId, int inactiveDays)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM05_INACTIVITY_WARNING,
                $"Inactivity warning: {email} inactive for {inactiveDays} days",
                tenantId,
                null,
                userId);

            auditEvent.TargetEmail = email;
            auditEvent.AffectedEntityType = "User";
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { InactiveDays = inactiveDays });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-05: Inactivity warning: User {UserId}, {Days} days", userId, inactiveDays);
        }

        public async Task LogInactivitySuspensionAsync(Guid userId, string email, Guid tenantId, int inactiveDays)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM05_INACTIVITY_SUSPENSION,
                $"Inactivity suspension: {email} inactive for {inactiveDays} days",
                tenantId,
                null,
                userId);

            auditEvent.TargetEmail = email;
            auditEvent.AffectedEntityType = "User";
            auditEvent.PreviousValue = "Active";
            auditEvent.NewValue = "Suspended";
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { InactiveDays = inactiveDays });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-05: Inactivity suspension: User {UserId}, {Days} days", userId, inactiveDays);
        }

        public async Task LogSessionRevokedAsync(Guid userId, Guid sessionId, Guid? revokedBy, string reason)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM05_SESSION_REVOKED,
                $"Session revoked: {sessionId} for user {userId} - {reason}",
                null,
                revokedBy,
                userId);

            auditEvent.AffectedEntityType = "Session";
            auditEvent.AffectedEntityId = sessionId;
            auditEvent.SessionId = sessionId;
            auditEvent.ErrorMessage = reason;
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { SessionId = sessionId, Reason = reason });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-05: Session revoked: User {UserId}, Session {SessionId}", userId, sessionId);
        }

        public async Task LogAllSessionsRevokedAsync(Guid userId, Guid revokedBy, string reason)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM05_ALL_SESSIONS_REVOKED,
                $"All sessions revoked for user {userId} - {reason}",
                null,
                revokedBy,
                userId);

            auditEvent.AffectedEntityType = "Session";
            auditEvent.ErrorMessage = reason;
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { Reason = reason });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-05: All sessions revoked: User {UserId}", userId);
        }

        #endregion

        #region AM-06: Invitation Control

        public async Task LogInvitationCreatedAsync(Guid invitationId, Guid tenantId, string invitedEmail, string role, Guid inviterId, DateTime expiresAt)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM06_INVITATION_CREATED,
                $"Invitation created: {invitedEmail} with role {role}",
                tenantId,
                inviterId,
                null);

            auditEvent.TargetEmail = invitedEmail;
            auditEvent.AffectedEntityType = "Invitation";
            auditEvent.AffectedEntityId = invitationId;
            auditEvent.NewValue = role;
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { Role = role, ExpiresAt = expiresAt });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-06: Invitation created: {Email}, Role {Role}", invitedEmail, role);
        }

        public async Task LogInvitationSentAsync(Guid invitationId, string invitedEmail)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM06_INVITATION_SENT,
                $"Invitation sent: {invitedEmail}",
                null,
                null,
                null);

            auditEvent.TargetEmail = invitedEmail;
            auditEvent.AffectedEntityType = "Invitation";
            auditEvent.AffectedEntityId = invitationId;
            auditEvent.Success = true;
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { InvitationId = invitationId });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-06: Invitation sent: {Email}", invitedEmail);
        }

        public async Task LogInvitationResentAsync(Guid invitationId, string invitedEmail, Guid resentBy, int resendCount)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM06_INVITATION_RESENT,
                $"Invitation resent: {invitedEmail} (count: {resendCount})",
                null,
                resentBy,
                null);

            auditEvent.TargetEmail = invitedEmail;
            auditEvent.AffectedEntityType = "Invitation";
            auditEvent.AffectedEntityId = invitationId;
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { ResendCount = resendCount });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-06: Invitation resent: {Email}, Count {Count}", invitedEmail, resendCount);
        }

        public async Task LogInvitationAcceptedAsync(Guid invitationId, Guid userId, string email, Guid tenantId)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM06_INVITATION_ACCEPTED,
                $"Invitation accepted: {email}",
                tenantId,
                userId,
                userId);

            auditEvent.TargetEmail = email;
            auditEvent.AffectedEntityType = "Invitation";
            auditEvent.AffectedEntityId = invitationId;
            auditEvent.Success = true;
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { InvitationId = invitationId, UserId = userId });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-06: Invitation accepted: {Email}", email);
        }

        public async Task LogInvitationExpiredAsync(Guid invitationId, string invitedEmail, Guid tenantId)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM06_INVITATION_EXPIRED,
                $"Invitation expired: {invitedEmail}",
                tenantId,
                null,
                null);

            auditEvent.TargetEmail = invitedEmail;
            auditEvent.AffectedEntityType = "Invitation";
            auditEvent.AffectedEntityId = invitationId;
            auditEvent.Success = false;
            auditEvent.ErrorMessage = "Invitation expired";

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-06: Invitation expired: {Email}", invitedEmail);
        }

        public async Task LogInvitationRevokedAsync(Guid invitationId, string invitedEmail, Guid tenantId, Guid revokedBy, string reason)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM06_INVITATION_REVOKED,
                $"Invitation revoked: {invitedEmail} - {reason}",
                tenantId,
                revokedBy,
                null);

            auditEvent.TargetEmail = invitedEmail;
            auditEvent.AffectedEntityType = "Invitation";
            auditEvent.AffectedEntityId = invitationId;
            auditEvent.ErrorMessage = reason;
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { Reason = reason });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-06: Invitation revoked: {Email}, Reason: {Reason}", invitedEmail, reason);
        }

        public async Task LogInvitationLimitExceededAsync(Guid tenantId, Guid inviterId, int currentCount, int limit)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM06_INVITATION_LIMIT_EXCEEDED,
                $"Invitation limit exceeded: {currentCount}/{limit}",
                tenantId,
                inviterId,
                null);

            auditEvent.AffectedEntityType = "Invitation";
            auditEvent.Success = false;
            auditEvent.ErrorMessage = $"Limit exceeded: {currentCount}/{limit}";
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { CurrentCount = currentCount, Limit = limit });

            await SaveEventAsync(auditEvent);
            _logger.LogWarning("AM-06: Invitation limit exceeded: Tenant {TenantId}, Count {Count}/{Limit}", tenantId, currentCount, limit);
        }

        #endregion

        #region AM-07: Abuse Prevention

        public async Task LogRateLimitExceededAsync(string endpoint, string? ipAddress, string? userId, int requestCount, int limit)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM07_RATE_LIMIT_EXCEEDED,
                $"Rate limit exceeded: {endpoint} - {requestCount}/{limit}",
                null,
                Guid.TryParse(userId, out var uid) ? uid : null,
                null);

            auditEvent.AffectedEntityType = "RateLimit";
            auditEvent.IpAddress = ipAddress ?? GetIpAddress();
            auditEvent.Success = false;
            auditEvent.ErrorMessage = $"Rate limit exceeded: {requestCount}/{limit}";
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { Endpoint = endpoint, RequestCount = requestCount, Limit = limit });

            await SaveEventAsync(auditEvent);
            _logger.LogWarning("AM-07: Rate limit exceeded: {Endpoint}, Count {Count}/{Limit}", endpoint, requestCount, limit);
        }

        public async Task LogSuspiciousActivityAsync(string activityType, string description, string? ipAddress, string? userId)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM07_SUSPICIOUS_ACTIVITY,
                $"Suspicious activity: {activityType} - {description}",
                null,
                Guid.TryParse(userId, out var uid) ? uid : null,
                null);

            auditEvent.AffectedEntityType = "Security";
            auditEvent.IpAddress = ipAddress ?? GetIpAddress();
            auditEvent.Success = false;
            auditEvent.ErrorMessage = description;
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { ActivityType = activityType, Description = description });

            await SaveEventAsync(auditEvent);
            _logger.LogWarning("AM-07: Suspicious activity: {Type}, {Description}", activityType, description);
        }

        public async Task LogIpBlockedAsync(string ipAddress, string reason, int duration)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM07_IP_BLOCKED,
                $"IP blocked: {ipAddress} - {reason} (duration: {duration}s)",
                null,
                null,
                null);

            auditEvent.AffectedEntityType = "IPBlock";
            auditEvent.IpAddress = ipAddress;
            auditEvent.Success = false;
            auditEvent.ErrorMessage = reason;
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { IpAddress = ipAddress, Reason = reason, Duration = duration });

            await SaveEventAsync(auditEvent);
            _logger.LogWarning("AM-07: IP blocked: {IP}, Reason: {Reason}, Duration: {Duration}s", ipAddress, reason, duration);
        }

        #endregion

        #region AM-08: Password and Recovery

        public async Task LogPasswordSetAsync(Guid userId, string email, string setMethod)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM08_PASSWORD_SET,
                $"Password set: {email} via {setMethod}",
                null,
                userId,
                userId);

            auditEvent.TargetEmail = email;
            auditEvent.AffectedEntityType = "Password";
            auditEvent.NewValue = setMethod;
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { SetMethod = setMethod });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-08: Password set: User {UserId}, Method {Method}", userId, setMethod);
        }

        public async Task LogPasswordChangedAsync(Guid userId, string email, Guid? changedBy)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM08_PASSWORD_CHANGED,
                $"Password changed: {email}",
                null,
                changedBy ?? userId,
                userId);

            auditEvent.TargetEmail = email;
            auditEvent.AffectedEntityType = "Password";
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { ChangedBy = changedBy });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-08: Password changed: User {UserId}", userId);
        }

        public async Task LogPasswordResetRequestedAsync(string email, string? ipAddress)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM08_PASSWORD_RESET_REQUESTED,
                $"Password reset requested: {email}",
                null,
                null,
                null);

            auditEvent.TargetEmail = email;
            auditEvent.AffectedEntityType = "Password";
            auditEvent.IpAddress = ipAddress ?? GetIpAddress();
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { Email = email });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-08: Password reset requested: {Email}", email);
        }

        public async Task LogPasswordResetCompletedAsync(Guid userId, string email, string? ipAddress)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM08_PASSWORD_RESET_COMPLETED,
                $"Password reset completed: {email}",
                null,
                userId,
                userId);

            auditEvent.TargetEmail = email;
            auditEvent.AffectedEntityType = "Password";
            auditEvent.IpAddress = ipAddress ?? GetIpAddress();
            auditEvent.Success = true;

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-08: Password reset completed: User {UserId}", userId);
        }

        public async Task LogPasswordResetFailedAsync(Guid userId, Guid tenantId, string reason, string? errorCode, string? ipAddress)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM08_PASSWORD_RESET_FAILED,
                $"Password reset failed: {reason}",
                tenantId,
                userId,
                userId);

            auditEvent.AffectedEntityType = "Password";
            auditEvent.IpAddress = ipAddress ?? GetIpAddress();
            auditEvent.Success = false;
            auditEvent.ErrorMessage = reason;
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { Reason = reason, ErrorCode = errorCode });

            await SaveEventAsync(auditEvent);
            _logger.LogWarning("AM-08: Password reset failed: User {UserId}, Reason: {Reason}", userId, reason);
        }

        public async Task LogPasswordPolicyViolationAsync(string email, string violationType)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM08_PASSWORD_POLICY_VIOLATION,
                $"Password policy violation: {violationType} for {email}",
                null,
                null,
                null);

            auditEvent.TargetEmail = email;
            auditEvent.AffectedEntityType = "Password";
            auditEvent.Success = false;
            auditEvent.ErrorMessage = violationType;
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { ViolationType = violationType });

            await SaveEventAsync(auditEvent);
            _logger.LogWarning("AM-08: Password policy violation: {Email}, Type: {Type}", email, violationType);
        }

        public async Task LogAccountLockedAsync(Guid userId, string email, string reason, int failedAttempts)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM08_ACCOUNT_LOCKED,
                $"Account locked: {email} - {reason} (attempts: {failedAttempts})",
                null,
                null,
                userId);

            auditEvent.TargetEmail = email;
            auditEvent.AffectedEntityType = "Account";
            auditEvent.PreviousValue = "Unlocked";
            auditEvent.NewValue = "Locked";
            auditEvent.Success = false;
            auditEvent.ErrorMessage = reason;
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { Reason = reason, FailedAttempts = failedAttempts });

            await SaveEventAsync(auditEvent);
            _logger.LogWarning("AM-08: Account locked: User {UserId}, Attempts: {Attempts}", userId, failedAttempts);
        }

        public async Task LogAccountUnlockedAsync(Guid userId, string email, Guid? unlockedBy)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM08_ACCOUNT_UNLOCKED,
                $"Account unlocked: {email}",
                null,
                unlockedBy,
                userId);

            auditEvent.TargetEmail = email;
            auditEvent.AffectedEntityType = "Account";
            auditEvent.PreviousValue = "Locked";
            auditEvent.NewValue = "Unlocked";
            auditEvent.Success = true;
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { UnlockedBy = unlockedBy });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-08: Account unlocked: User {UserId}", userId);
        }

        #endregion

        #region AM-09: Trial Governance

        public async Task LogTrialCreatedAsync(Guid tenantId, string tenantName, Guid userId, DateTime expiresAt)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM09_TRIAL_CREATED,
                $"Trial created: {tenantName} expires {expiresAt}",
                tenantId,
                userId,
                userId);

            auditEvent.AffectedEntityType = "Trial";
            auditEvent.AffectedEntityId = tenantId;
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { TenantName = tenantName, ExpiresAt = expiresAt });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-09: Trial created: Tenant {TenantId}, Expires {ExpiresAt}", tenantId, expiresAt);
        }

        public async Task LogTrialExtendedAsync(Guid extendedBy, Guid tenantId, int extensionDays, DateTime newExpiry, string? ipAddress)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM09_TRIAL_EXTENDED,
                $"Trial extended: {extensionDays} days, new expiry {newExpiry}",
                tenantId,
                extendedBy,
                null);

            auditEvent.AffectedEntityType = "Trial";
            auditEvent.AffectedEntityId = tenantId;
            auditEvent.IpAddress = ipAddress ?? GetIpAddress();
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { ExtensionDays = extensionDays, NewExpiry = newExpiry });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-09: Trial extended: Tenant {TenantId}, New expiry {Expiry}", tenantId, newExpiry);
        }

        public async Task LogTrialExpiryWarningAsync(Guid tenantId, int daysRemaining)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM09_TRIAL_EXPIRY_WARNING,
                $"Trial expiry warning: {daysRemaining} days remaining",
                tenantId,
                null,
                null);

            auditEvent.AffectedEntityType = "Trial";
            auditEvent.AffectedEntityId = tenantId;
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { DaysRemaining = daysRemaining });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-09: Trial expiry warning: Tenant {TenantId}, {Days} days remaining", tenantId, daysRemaining);
        }

        public async Task LogTrialExpiredAsync(Guid tenantId)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM09_TRIAL_EXPIRED,
                $"Trial expired: Tenant {tenantId}",
                tenantId,
                null,
                null);

            auditEvent.AffectedEntityType = "Trial";
            auditEvent.AffectedEntityId = tenantId;
            auditEvent.PreviousValue = "Active";
            auditEvent.NewValue = "Expired";
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { TenantId = tenantId });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-09: Trial expired: Tenant {TenantId}", tenantId);
        }

        public async Task LogTrialConvertedAsync(Guid convertedBy, Guid tenantId, string planType, string? subscriptionId, string? ipAddress)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM09_TRIAL_CONVERTED,
                $"Trial converted: {planType} (subscription: {subscriptionId})",
                tenantId,
                convertedBy,
                null);

            auditEvent.AffectedEntityType = "Trial";
            auditEvent.AffectedEntityId = tenantId;
            auditEvent.IpAddress = ipAddress ?? GetIpAddress();
            auditEvent.PreviousValue = "Trial";
            auditEvent.NewValue = planType;
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { PlanType = planType, SubscriptionId = subscriptionId });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-09: Trial converted: Tenant {TenantId}, Plan {Plan}", tenantId, planType);
        }

        public async Task LogTrialDataArchivedAsync(Guid? userId, Guid tenantId, string? ipAddress)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM09_TRIAL_DATA_ARCHIVED,
                $"Trial data archived: Tenant {tenantId}",
                tenantId,
                userId,
                null);

            auditEvent.AffectedEntityType = "Trial";
            auditEvent.AffectedEntityId = tenantId;
            auditEvent.IpAddress = ipAddress ?? GetIpAddress();
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { TenantId = tenantId, ArchivedBy = userId });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-09: Trial data archived: Tenant {TenantId}", tenantId);
        }

        public async Task LogTrialDataDeletedAsync(Guid? userId, Guid tenantId, string? ipAddress)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM09_TRIAL_DATA_DELETED,
                $"Trial data deleted: Tenant {tenantId}",
                tenantId,
                userId,
                null);

            auditEvent.AffectedEntityType = "Trial";
            auditEvent.AffectedEntityId = tenantId;
            auditEvent.IpAddress = ipAddress ?? GetIpAddress();
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { TenantId = tenantId, DeletedBy = userId });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-09: Trial data deleted: Tenant {TenantId}", tenantId);
        }

        #endregion

        #region AM-10: Audit Export

        public async Task LogAuditExportRequestedAsync(Guid tenantId, Guid requestedBy, string exportType, DateTime fromDate, DateTime toDate)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM10_AUDIT_EXPORT_REQUESTED,
                $"Audit export requested: {exportType} from {fromDate} to {toDate}",
                tenantId,
                requestedBy,
                null);

            auditEvent.AffectedEntityType = "AuditExport";
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { ExportType = exportType, FromDate = fromDate, ToDate = toDate });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-10: Audit export requested: Tenant {TenantId}, Type {Type}", tenantId, exportType);
        }

        public async Task LogAuditExportCompletedAsync(Guid tenantId, Guid requestedBy, string exportType, int recordCount)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM10_AUDIT_EXPORT_COMPLETED,
                $"Audit export completed: {exportType} - {recordCount} records",
                tenantId,
                requestedBy,
                null);

            auditEvent.AffectedEntityType = "AuditExport";
            auditEvent.Success = true;
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { ExportType = exportType, RecordCount = recordCount });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-10: Audit export completed: Tenant {TenantId}, Records {Count}", tenantId, recordCount);
        }

        #endregion

        #region AM-11: Access Reviews

        public async Task LogAccessReviewInitiatedAsync(Guid reviewId, Guid tenantId, string reviewType, Guid initiatedBy, int userCount, string? ipAddress)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM11_REVIEW_INITIATED,
                $"Access review initiated: {reviewType} for {userCount} users",
                tenantId,
                initiatedBy,
                null);

            auditEvent.AffectedEntityType = "AccessReview";
            auditEvent.AffectedEntityId = reviewId;
            auditEvent.IpAddress = ipAddress ?? GetIpAddress();
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { ReviewType = reviewType, UserCount = userCount });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-11: Access review initiated: {ReviewId}, Type {Type}, Users {Count}", reviewId, reviewType, userCount);
        }

        public async Task LogAccessReviewItemCertifiedAsync(Guid reviewId, Guid userId, string currentRole, Guid reviewedBy, Guid tenantId, string? ipAddress)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM11_REVIEW_ITEM_CERTIFIED,
                $"Access certified: User {userId}, Role {currentRole}",
                tenantId,
                reviewedBy,
                userId);

            auditEvent.AffectedEntityType = "AccessReview";
            auditEvent.AffectedEntityId = reviewId;
            auditEvent.IpAddress = ipAddress ?? GetIpAddress();
            auditEvent.NewValue = "Certified";
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { CurrentRole = currentRole });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-11: Access certified: Review {ReviewId}, User {UserId}, Role {Role}", reviewId, userId, currentRole);
        }

        public async Task LogAccessReviewItemRevokedAsync(Guid reviewId, Guid userId, string previousRole, Guid reviewedBy, Guid tenantId, string? ipAddress)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM11_REVIEW_ITEM_REVOKED,
                $"Access revoked: User {userId}, Role {previousRole}",
                tenantId,
                reviewedBy,
                userId);

            auditEvent.AffectedEntityType = "AccessReview";
            auditEvent.AffectedEntityId = reviewId;
            auditEvent.IpAddress = ipAddress ?? GetIpAddress();
            auditEvent.PreviousValue = previousRole;
            auditEvent.NewValue = "Revoked";
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { PreviousRole = previousRole });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-11: Access revoked: Review {ReviewId}, User {UserId}, Role {Role}", reviewId, userId, previousRole);
        }

        public async Task LogAccessReviewItemModifiedAsync(Guid reviewId, Guid userId, string previousRole, string newRole, Guid reviewedBy, Guid tenantId, string? ipAddress)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM11_REVIEW_ITEM_MODIFIED,
                $"Access modified: User {userId}, {previousRole} -> {newRole}",
                tenantId,
                reviewedBy,
                userId);

            auditEvent.AffectedEntityType = "AccessReview";
            auditEvent.AffectedEntityId = reviewId;
            auditEvent.IpAddress = ipAddress ?? GetIpAddress();
            auditEvent.PreviousValue = previousRole;
            auditEvent.NewValue = newRole;
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { PreviousRole = previousRole, NewRole = newRole });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-11: Access modified: Review {ReviewId}, User {UserId}, {OldRole} -> {NewRole}", reviewId, userId, previousRole, newRole);
        }

        public async Task LogAccessReviewCompletedAsync(Guid reviewId, Guid tenantId, Guid completedBy, int certifiedCount, int revokedCount, int modifiedCount, string? ipAddress)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM11_REVIEW_COMPLETED,
                $"Access review completed: Certified {certifiedCount}, Revoked {revokedCount}, Modified {modifiedCount}",
                tenantId,
                completedBy,
                null);

            auditEvent.AffectedEntityType = "AccessReview";
            auditEvent.AffectedEntityId = reviewId;
            auditEvent.IpAddress = ipAddress ?? GetIpAddress();
            auditEvent.Success = true;
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { CertifiedCount = certifiedCount, RevokedCount = revokedCount, ModifiedCount = modifiedCount });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-11: Review completed: {ReviewId}, Certified {Certified}, Revoked {Revoked}, Modified {Modified}", 
                reviewId, certifiedCount, revokedCount, modifiedCount);
        }

        public async Task LogAccessReviewOverdueAsync(Guid reviewId, Guid tenantId, int daysPastDue, string? ipAddress)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM11_REVIEW_OVERDUE,
                $"Access review overdue: {daysPastDue} days past due",
                tenantId,
                null,
                null);

            auditEvent.AffectedEntityType = "AccessReview";
            auditEvent.AffectedEntityId = reviewId;
            auditEvent.IpAddress = ipAddress ?? GetIpAddress();
            auditEvent.Success = false;
            auditEvent.ErrorMessage = $"Review overdue by {daysPastDue} days";
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { DaysPastDue = daysPastDue });

            await SaveEventAsync(auditEvent);
            _logger.LogWarning("AM-11: Review overdue: {ReviewId}, {Days} days past due", reviewId, daysPastDue);
        }

        #endregion

        #region AM-12: Separation of Duties

        public async Task LogSoDViolationDetectedAsync(Guid userId, Guid tenantId, string conflictType, string role1, string role2, string? ipAddress)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM12_SOD_VIOLATION_DETECTED,
                $"SoD violation detected: {conflictType} - Roles {role1}/{role2}",
                tenantId,
                null,
                userId);

            auditEvent.AffectedEntityType = "SoD";
            auditEvent.IpAddress = ipAddress ?? GetIpAddress();
            auditEvent.Success = false;
            auditEvent.ErrorMessage = $"Conflict: {conflictType}";
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { ConflictType = conflictType, Role1 = role1, Role2 = role2 });

            await SaveEventAsync(auditEvent);
            _logger.LogWarning("AM-12: SoD violation detected: User {UserId}, Conflict {Type}, Roles {Role1}/{Role2}", userId, conflictType, role1, role2);
        }

        public async Task LogSoDViolationBlockedAsync(Guid userId, Guid tenantId, string conflictType, string role1, string role2, string? ipAddress)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM12_SOD_VIOLATION_BLOCKED,
                $"SoD violation blocked: {conflictType} - Roles {role1}/{role2}",
                tenantId,
                null,
                userId);

            auditEvent.AffectedEntityType = "SoD";
            auditEvent.IpAddress = ipAddress ?? GetIpAddress();
            auditEvent.Success = false;
            auditEvent.ErrorMessage = $"Blocked: {conflictType}";
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { ConflictType = conflictType, Role1 = role1, Role2 = role2 });

            await SaveEventAsync(auditEvent);
            _logger.LogWarning("AM-12: SoD violation blocked: User {UserId}, Conflict {Type}, Roles {Role1}/{Role2}", userId, conflictType, role1, role2);
        }

        public async Task LogSoDOverrideRequestedAsync(Guid userId, string email, string action1, string action2, Guid tenantId, string justification)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM12_SOD_OVERRIDE_REQUESTED,
                $"SoD override requested: {action1}/{action2} - {justification}",
                tenantId,
                userId,
                userId);

            auditEvent.TargetEmail = email;
            auditEvent.AffectedEntityType = "SoD";
            auditEvent.NewValue = "OverrideRequested";
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { Action1 = action1, Action2 = action2, Justification = justification });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-12: SoD override requested: User {UserId}, Actions {Action1}/{Action2}", userId, action1, action2);
        }

        public async Task LogSoDOverrideApprovedAsync(Guid userId, string email, string action1, string action2, Guid tenantId, Guid approvedBy)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM12_SOD_OVERRIDE_APPROVED,
                $"SoD override approved: {action1}/{action2}",
                tenantId,
                approvedBy,
                userId);

            auditEvent.TargetEmail = email;
            auditEvent.AffectedEntityType = "SoD";
            auditEvent.NewValue = "OverrideApproved";
            auditEvent.Success = true;
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { Action1 = action1, Action2 = action2, ApprovedBy = approvedBy });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-12: SoD override approved: User {UserId}, by {ApprovedBy}", userId, approvedBy);
        }

        public async Task LogSoDOverrideDeniedAsync(Guid userId, string email, string action1, string action2, Guid tenantId, Guid deniedBy, string reason)
        {
            var auditEvent = AccessManagementAuditEvent.Create(
                AuditEventTypes.AM12_SOD_OVERRIDE_DENIED,
                $"SoD override denied: {action1}/{action2} - {reason}",
                tenantId,
                deniedBy,
                userId);

            auditEvent.TargetEmail = email;
            auditEvent.AffectedEntityType = "SoD";
            auditEvent.NewValue = "OverrideDenied";
            auditEvent.Success = false;
            auditEvent.ErrorMessage = reason;
            auditEvent.DetailsJson = JsonSerializer.Serialize(new { Action1 = action1, Action2 = action2, Reason = reason });

            await SaveEventAsync(auditEvent);
            _logger.LogInformation("AM-12: SoD override denied: User {UserId}, by {DeniedBy}", userId, deniedBy);
        }

        #endregion
    }
}
