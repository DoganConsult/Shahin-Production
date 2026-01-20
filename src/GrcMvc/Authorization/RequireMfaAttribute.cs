using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GrcMvc.Authorization
{
    /// <summary>
    /// AM-04: Attribute to require MFA for specific actions or controllers.
    /// Can be applied to roles or specific privileged actions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequireMfaAttribute : TypeFilterAttribute
    {
        /// <summary>
        /// Comma-separated list of roles that require MFA for this action.
        /// If empty, MFA is required for all authenticated users.
        /// </summary>
        public string? Roles { get; set; }

        /// <summary>
        /// If true, require step-up authentication even if MFA was verified at login.
        /// </summary>
        public bool RequireStepUp { get; set; } = false;

        /// <summary>
        /// Action identifier for step-up auth tracking.
        /// </summary>
        public string? Action { get; set; }

        public RequireMfaAttribute() : base(typeof(RequireMfaFilter))
        {
            Arguments = new object[] { this };
        }
    }

    /// <summary>
    /// AM-04: Attribute to require step-up authentication for privileged actions.
    /// Step-up auth requires re-authentication via MFA even if user is logged in.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequireStepUpAuthAttribute : TypeFilterAttribute
    {
        /// <summary>
        /// Action identifier for step-up auth (e.g., "role.change", "user.suspend").
        /// </summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// Validity period in minutes. Uses config default if not specified.
        /// </summary>
        public int? ValidityMinutes { get; set; }

        public RequireStepUpAuthAttribute(string action) : base(typeof(RequireStepUpAuthFilter))
        {
            Action = action;
            Arguments = new object[] { this };
        }
    }

    /// <summary>
    /// Filter implementation for RequireMfaAttribute.
    /// </summary>
    public class RequireMfaFilter : IAsyncAuthorizationFilter
    {
        private readonly RequireMfaAttribute _attribute;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RequireMfaFilter> _logger;

        public RequireMfaFilter(
            RequireMfaAttribute attribute,
            IServiceProvider serviceProvider,
            ILogger<RequireMfaFilter> logger)
        {
            _attribute = attribute;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            if (!user.Identity?.IsAuthenticated == true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Get user ID from claims
            var userIdClaim = user.FindFirst("sub") ?? user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                _logger.LogWarning("MFA check failed - no valid user ID in claims");
                context.Result = new UnauthorizedResult();
                return;
            }

            // Check if user's roles require MFA
            if (!string.IsNullOrEmpty(_attribute.Roles))
            {
                var requiredRoles = _attribute.Roles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var userRoles = user.Claims
                    .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role || c.Type == "role")
                    .Select(c => c.Value)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                // If user doesn't have any of the required roles, MFA not needed
                if (!requiredRoles.Any(r => userRoles.Contains(r)))
                {
                    return;
                }
            }

            // Check if MFA is verified
            var mfaVerifiedClaim = user.FindFirst("mfa_verified") ?? user.FindFirst("amr");
            var isMfaVerified = mfaVerifiedClaim?.Value == "true" || mfaVerifiedClaim?.Value == "mfa";

            if (!isMfaVerified)
            {
                _logger.LogWarning("MFA required but not verified for user {UserId}", userId);
                context.Result = new ObjectResult(new
                {
                    error = "mfa_required",
                    message = "Multi-factor authentication is required for this action.",
                    action = _attribute.Action
                })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return;
            }

            // If step-up is required, check additional auth
            if (_attribute.RequireStepUp && !string.IsNullOrEmpty(_attribute.Action))
            {
                var stepUpService = _serviceProvider.GetService<Services.Interfaces.IStepUpAuthService>();
                if (stepUpService != null)
                {
                    var sessionIdClaim = user.FindFirst("session_id");
                    if (sessionIdClaim != null && Guid.TryParse(sessionIdClaim.Value, out var sessionId))
                    {
                        var hasStepUp = await stepUpService.HasValidStepUpAuthAsync(userId, sessionId, _attribute.Action);
                        if (!hasStepUp)
                        {
                            context.Result = new ObjectResult(new
                            {
                                error = "step_up_required",
                                message = "Step-up authentication is required for this privileged action.",
                                action = _attribute.Action
                            })
                            {
                                StatusCode = StatusCodes.Status403Forbidden
                            };
                            return;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Filter implementation for RequireStepUpAuthAttribute.
    /// </summary>
    public class RequireStepUpAuthFilter : IAsyncAuthorizationFilter
    {
        private readonly RequireStepUpAuthAttribute _attribute;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RequireStepUpAuthFilter> _logger;

        public RequireStepUpAuthFilter(
            RequireStepUpAuthAttribute attribute,
            IServiceProvider serviceProvider,
            ILogger<RequireStepUpAuthFilter> logger)
        {
            _attribute = attribute;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            if (!user.Identity?.IsAuthenticated == true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var userIdClaim = user.FindFirst("sub") ?? user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            var sessionIdClaim = user.FindFirst("session_id");

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var sessionId = Guid.TryParse(sessionIdClaim?.Value, out var sid) ? sid : Guid.Empty;

            var stepUpService = _serviceProvider.GetService<Services.Interfaces.IStepUpAuthService>();
            if (stepUpService == null)
            {
                _logger.LogError("IStepUpAuthService not registered");
                return; // Allow if service not available (graceful degradation)
            }

            // Check if this action requires step-up
            if (!stepUpService.RequiresStepUpAuth(_attribute.Action))
            {
                return; // Action doesn't require step-up
            }

            // Check if user has valid step-up auth
            var hasValidStepUp = await stepUpService.HasValidStepUpAuthAsync(userId, sessionId, _attribute.Action);
            if (!hasValidStepUp)
            {
                // Log the step-up requirement
                var auditService = _serviceProvider.GetService<Services.Interfaces.IAccessManagementAuditService>();
                if (auditService != null)
                {
                    await auditService.LogStepUpRequiredAsync(userId, null, _attribute.Action, null);
                }

                _logger.LogInformation("Step-up auth required for user {UserId}, action {Action}", userId, _attribute.Action);

                context.Result = new ObjectResult(new
                {
                    error = "step_up_required",
                    message = "This action requires step-up authentication. Please verify your identity.",
                    action = _attribute.Action,
                    methods = new[] { "mfa", "password" }
                })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }
        }
    }
}
