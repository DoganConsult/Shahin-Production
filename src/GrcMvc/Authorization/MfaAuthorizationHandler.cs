using GrcMvc.Configuration;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace GrcMvc.Authorization
{
    /// <summary>
    /// AM-04: Authorization requirement for MFA-protected resources.
    /// </summary>
    public class MfaRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// Roles that require MFA for this requirement.
        /// </summary>
        public string[]? Roles { get; set; }

        /// <summary>
        /// Whether step-up re-authentication is required.
        /// </summary>
        public bool RequireStepUp { get; set; }

        /// <summary>
        /// Action identifier for step-up tracking.
        /// </summary>
        public string? Action { get; set; }

        public MfaRequirement() { }

        public MfaRequirement(params string[] roles)
        {
            Roles = roles;
        }
    }

    /// <summary>
    /// AM-04: Authorization handler that validates MFA requirements.
    /// </summary>
    public class MfaAuthorizationHandler : AuthorizationHandler<MfaRequirement>
    {
        private readonly IStepUpAuthService _stepUpService;
        private readonly MfaEnforcementOptions _options;
        private readonly ILogger<MfaAuthorizationHandler> _logger;

        public MfaAuthorizationHandler(
            IStepUpAuthService stepUpService,
            IOptions<AccessManagementOptions> options,
            ILogger<MfaAuthorizationHandler> logger)
        {
            _stepUpService = stepUpService;
            _options = options.Value.MfaEnforcement;
            _logger = logger;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            MfaRequirement requirement)
        {
            if (!context.User.Identity?.IsAuthenticated == true)
            {
                return; // Not authenticated, fail silently
            }

            var userIdClaim = context.User.FindFirst("sub") ??
                              context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                _logger.LogWarning("MFA handler: No valid user ID claim found");
                return;
            }

            // Check if MFA is required based on roles
            var userRoles = context.User.Claims
                .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role || c.Type == "role")
                .Select(c => c.Value)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var requiresMfa = false;

            // Check requirement-specific roles
            if (requirement.Roles?.Length > 0)
            {
                requiresMfa = requirement.Roles.Any(r => userRoles.Contains(r));
            }
            // Check global config roles
            else if (_options.RequiredForRoles?.Length > 0)
            {
                requiresMfa = _options.RequiredForRoles.Any(r => userRoles.Contains(r));
            }

            if (!requiresMfa)
            {
                // MFA not required for this user's roles
                context.Succeed(requirement);
                return;
            }

            // Check if MFA was verified during authentication
            var mfaVerified = context.User.HasClaim(c =>
                (c.Type == "mfa_verified" && c.Value == "true") ||
                (c.Type == "amr" && c.Value == "mfa"));

            if (!mfaVerified)
            {
                _logger.LogWarning("MFA required but not verified for user {UserId}", userId);
                context.Fail(new AuthorizationFailureReason(this, "MFA verification required"));
                return;
            }

            // Check step-up auth if required
            if (requirement.RequireStepUp && !string.IsNullOrEmpty(requirement.Action))
            {
                var sessionIdClaim = context.User.FindFirst("session_id");
                var sessionId = Guid.TryParse(sessionIdClaim?.Value, out var sid) ? sid : Guid.Empty;

                var hasValidStepUp = await _stepUpService.HasValidStepUpAuthAsync(userId, sessionId, requirement.Action);
                if (!hasValidStepUp)
                {
                    _logger.LogWarning("Step-up auth required for user {UserId}, action {Action}", userId, requirement.Action);
                    context.Fail(new AuthorizationFailureReason(this, $"Step-up authentication required for action: {requirement.Action}"));
                    return;
                }
            }

            context.Succeed(requirement);
        }
    }

    /// <summary>
    /// Authorization requirement for step-up authentication on privileged actions.
    /// </summary>
    public class StepUpAuthRequirement : IAuthorizationRequirement
    {
        public string Action { get; }

        public StepUpAuthRequirement(string action)
        {
            Action = action;
        }
    }

    /// <summary>
    /// Authorization handler for step-up authentication requirements.
    /// </summary>
    public class StepUpAuthorizationHandler : AuthorizationHandler<StepUpAuthRequirement>
    {
        private readonly IStepUpAuthService _stepUpService;
        private readonly IAccessManagementAuditService _auditService;
        private readonly ILogger<StepUpAuthorizationHandler> _logger;

        public StepUpAuthorizationHandler(
            IStepUpAuthService stepUpService,
            IAccessManagementAuditService auditService,
            ILogger<StepUpAuthorizationHandler> logger)
        {
            _stepUpService = stepUpService;
            _auditService = auditService;
            _logger = logger;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            StepUpAuthRequirement requirement)
        {
            if (!context.User.Identity?.IsAuthenticated == true)
            {
                return;
            }

            var userIdClaim = context.User.FindFirst("sub") ??
                              context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            var sessionIdClaim = context.User.FindFirst("session_id");

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return;
            }

            var sessionId = Guid.TryParse(sessionIdClaim?.Value, out var sid) ? sid : Guid.Empty;

            // Check if action requires step-up
            if (!_stepUpService.RequiresStepUpAuth(requirement.Action))
            {
                context.Succeed(requirement);
                return;
            }

            // Verify step-up auth
            var hasValidStepUp = await _stepUpService.HasValidStepUpAuthAsync(userId, sessionId, requirement.Action);

            if (hasValidStepUp)
            {
                context.Succeed(requirement);
            }
            else
            {
                // Log the step-up requirement
                await _auditService.LogStepUpRequiredAsync(userId, null, requirement.Action, null);

                _logger.LogInformation(
                    "Step-up authentication required for user {UserId}, action {Action}",
                    userId, requirement.Action);

                context.Fail(new AuthorizationFailureReason(this,
                    $"Step-up authentication required for privileged action: {requirement.Action}"));
            }
        }
    }

    /// <summary>
    /// Extension methods for MFA authorization policies.
    /// </summary>
    public static class MfaAuthorizationExtensions
    {
        /// <summary>
        /// Adds MFA-related authorization policies.
        /// </summary>
        public static AuthorizationOptions AddMfaPolicies(this AuthorizationOptions options)
        {
            // Policy requiring MFA for any privileged role
            options.AddPolicy("RequireMfa", policy =>
                policy.Requirements.Add(new MfaRequirement()));

            // Policy requiring MFA for TenantAdmin
            options.AddPolicy("RequireMfaTenantAdmin", policy =>
                policy.Requirements.Add(new MfaRequirement("TenantAdmin")));

            // Policy requiring MFA for PlatformAdmin
            options.AddPolicy("RequireMfaPlatformAdmin", policy =>
                policy.Requirements.Add(new MfaRequirement("PlatformAdmin")));

            // Step-up policies for privileged actions
            options.AddPolicy("StepUpRoleChange", policy =>
                policy.Requirements.Add(new StepUpAuthRequirement("role.change")));

            options.AddPolicy("StepUpAdminInvite", policy =>
                policy.Requirements.Add(new StepUpAuthRequirement("admin.invite")));

            options.AddPolicy("StepUpUserSuspend", policy =>
                policy.Requirements.Add(new StepUpAuthRequirement("user.suspend")));

            options.AddPolicy("StepUpUserDeprovision", policy =>
                policy.Requirements.Add(new StepUpAuthRequirement("user.deprovision")));

            options.AddPolicy("StepUpApiKeyCreate", policy =>
                policy.Requirements.Add(new StepUpAuthRequirement("api.key.create")));

            return options;
        }
    }
}
