using GrcMvc.Constants;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace GrcMvc.Controllers.Api
{
    /// <summary>
    /// Trial API Controller
    /// Handles all trial lifecycle operations from signup to conversion
    /// </summary>
    [Route("api/trial")]
    [ApiController]
    [IgnoreAntiforgeryToken]
    public class TrialApiController : ControllerBase
    {
        private readonly ITrialLifecycleService _trialService;
        private readonly ILogger<TrialApiController> _logger;

        public TrialApiController(
            ITrialLifecycleService trialService,
            ILogger<TrialApiController> logger)
        {
            _trialService = trialService;
            _logger = logger;
        }

        #region Public Endpoints (No Auth)

        /// <summary>
        /// Start trial signup - capture interest
        /// POST /api/trial/signup
        /// </summary>
        [HttpPost("signup")]
        [AllowAnonymous]
        [EnableRateLimiting("api")]
        public async Task<IActionResult> Signup([FromBody] TrialSignupRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _trialService.SignupAsync(request);

                if (!result.Success)
                    return BadRequest(new { error = result.Message });

                return Ok(new
                {
                    success = true,
                    signupId = result.SignupId,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during trial signup");
                return StatusCode(500, new { error = "Signup failed. Please try again." });
            }
        }

        /// <summary>
        /// Provision trial tenant and create admin user account
        /// POST /api/trial/provision
        /// </summary>
        [HttpPost("provision")]
        [AllowAnonymous]
        public async Task<IActionResult> Provision([FromBody] ProvisionRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest(new { error = "Password is required" });
                }

                var result = await _trialService.ProvisionTrialAsync(request.SignupId, request.Password);

                if (!result.Success)
                    return BadRequest(new { error = result.Message });

                return Ok(new
                {
                    success = true,
                    tenantId = result.TenantId,
                    userId = result.UserId,
                    tenantSlug = result.TenantSlug,
                    trialEndsAt = result.TrialEndsAt,
                    loginUrl = $"/Account/Login"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error provisioning trial");
                return StatusCode(500, new { error = "Provisioning failed" });
            }
        }

        /// <summary>
        /// Activate trial with token
        /// POST /api/trial/activate
        /// </summary>
        [HttpPost("activate")]
        [AllowAnonymous]
        public async Task<IActionResult> Activate([FromBody] ActivateRequest request)
        {
            try
            {
                var success = await _trialService.ActivateTrialAsync(request.TenantId, request.Token);

                if (!success)
                    return BadRequest(new { error = "Invalid or expired activation token" });

                return Ok(new { success = true, message = "Trial activated" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating trial");
                return StatusCode(500, new { error = "Activation failed" });
            }
        }

        #endregion

        #region Authenticated Endpoints (Trial User)

        /// <summary>
        /// Get trial status
        /// GET /api/trial/status
        /// </summary>
        [HttpGet("status")]
        [Authorize]
        public async Task<IActionResult> GetStatus()
        {
            try
            {
                var tenantId = GetTenantIdFromClaims();
                if (tenantId == Guid.Empty)
                    return Unauthorized(new { error = "Tenant not found" });

                var status = await _trialService.GetTrialStatusAsync(tenantId);
                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trial status");
                return StatusCode(500, new { error = "Failed to get status" });
            }
        }

        /// <summary>
        /// Get trial usage/engagement metrics
        /// GET /api/trial/usage
        /// </summary>
        [HttpGet("usage")]
        [Authorize]
        public async Task<IActionResult> GetUsage()
        {
            try
            {
                var tenantId = GetTenantIdFromClaims();
                if (tenantId == Guid.Empty)
                    return Unauthorized(new { error = "Tenant not found" });

                var usage = await _trialService.GetTrialUsageAsync(tenantId);
                return Ok(usage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trial usage");
                return StatusCode(500, new { error = "Failed to get usage" });
            }
        }

        /// <summary>
        /// Request trial extension (one-time, +7 days)
        /// POST /api/trial/extend
        /// </summary>
        [HttpPost("extend")]
        [Authorize]
        public async Task<IActionResult> ExtendTrial([FromBody] TrialExtendRequest request)
        {
            try
            {
                var tenantId = GetTenantIdFromClaims();
                if (tenantId == Guid.Empty)
                    return Unauthorized(new { error = "Tenant not found" });

                var result = await _trialService.RequestExtensionAsync(tenantId, request.Reason ?? "User request");

                if (!result.Success)
                    return BadRequest(new { error = result.Message });

                return Ok(new
                {
                    success = true,
                    newEndDate = result.NewEndDate,
                    daysAdded = result.DaysAdded,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extending trial");
                return StatusCode(500, new { error = "Extension failed" });
            }
        }

        /// <summary>
        /// Start conversion to paid subscription
        /// POST /api/trial/convert
        /// </summary>
        [HttpPost("convert")]
        [Authorize]
        public async Task<IActionResult> StartConversion([FromBody] ConvertRequest request)
        {
            try
            {
                var tenantId = GetTenantIdFromClaims();
                if (tenantId == Guid.Empty)
                    return Unauthorized(new { error = "Tenant not found" });

                var result = await _trialService.StartConversionAsync(tenantId, request.PlanCode);

                if (!result.Success)
                    return BadRequest(new { error = result.Message });

                return Ok(new
                {
                    success = true,
                    checkoutUrl = result.CheckoutUrl,
                    planCode = result.PlanCode,
                    monthlyPrice = result.MonthlyPrice
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting conversion");
                return StatusCode(500, new { error = "Conversion failed" });
            }
        }

        #endregion

        #region Team Collaboration Endpoints

        /// <summary>
        /// Invite team member to trial
        /// POST /api/trial/team/invite
        /// </summary>
        [HttpPost("team/invite")]
        [Authorize]
        public async Task<IActionResult> InviteTeamMember([FromBody] TeamInviteRequest request)
        {
            try
            {
                var tenantId = GetTenantIdFromClaims();
                if (tenantId == Guid.Empty)
                    return Unauthorized(new { error = "Tenant not found" });

                var result = await _trialService.InviteTeamMemberAsync(tenantId, request);

                if (!result.Success)
                    return BadRequest(new { error = result.Message });

                return Ok(new
                {
                    success = true,
                    inviteId = result.InviteId,
                    inviteLink = result.InviteLink,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inviting team member");
                return StatusCode(500, new { error = "Invitation failed" });
            }
        }

        /// <summary>
        /// Get trial team members
        /// GET /api/trial/team
        /// </summary>
        [HttpGet("team")]
        [Authorize]
        public async Task<IActionResult> GetTeam()
        {
            try
            {
                var tenantId = GetTenantIdFromClaims();
                if (tenantId == Guid.Empty)
                    return Unauthorized(new { error = "Tenant not found" });

                var team = await _trialService.GetTrialTeamAsync(tenantId);
                return Ok(new { total = team.Count, members = team });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting team");
                return StatusCode(500, new { error = "Failed to get team" });
            }
        }

        /// <summary>
        /// Get team onboarding progress checklist
        /// GET /api/trial/team/onboarding
        /// </summary>
        [HttpGet("team/onboarding")]
        [Authorize]
        public async Task<IActionResult> GetTeamOnboarding()
        {
            try
            {
                var tenantId = GetTenantIdFromClaims();
                if (tenantId == Guid.Empty)
                    return Unauthorized(new { error = "Tenant not found" });

                var progress = await _trialService.GetTeamOnboardingProgressAsync(tenantId);
                return Ok(progress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting team onboarding");
                return StatusCode(500, new { error = "Failed to get onboarding progress" });
            }
        }

        /// <summary>
        /// Get team engagement dashboard with leaderboard
        /// GET /api/trial/team/engagement
        /// </summary>
        [HttpGet("team/engagement")]
        [Authorize]
        public async Task<IActionResult> GetTeamEngagement()
        {
            try
            {
                var tenantId = GetTenantIdFromClaims();
                if (tenantId == Guid.Empty)
                    return Unauthorized(new { error = "Tenant not found" });

                var dashboard = await _trialService.GetTeamEngagementAsync(tenantId);
                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting team engagement");
                return StatusCode(500, new { error = "Failed to get engagement dashboard" });
            }
        }

        /// <summary>
        /// Bulk invite team members
        /// POST /api/trial/team/invite/bulk
        /// </summary>
        [HttpPost("team/invite/bulk")]
        [Authorize]
        public async Task<IActionResult> BulkInviteTeamMembers([FromBody] List<TeamInviteRequest> requests)
        {
            try
            {
                var tenantId = GetTenantIdFromClaims();
                if (tenantId == Guid.Empty)
                    return Unauthorized(new { error = "Tenant not found" });

                var result = await _trialService.InviteTeamMembersBulkAsync(tenantId, requests);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk inviting team members");
                return StatusCode(500, new { error = "Bulk invitation failed" });
            }
        }

        /// <summary>
        /// Track team activity (login, document upload, etc.)
        /// POST /api/trial/team/activity
        /// </summary>
        [HttpPost("team/activity")]
        [Authorize]
        public async Task<IActionResult> TrackActivity([FromBody] TrackActivityRequest request)
        {
            try
            {
                var tenantId = GetTenantIdFromClaims();
                if (tenantId == Guid.Empty)
                    return Unauthorized(new { error = "Tenant not found" });

                await _trialService.TrackTeamActivityAsync(tenantId, request.UserId, request.ActivityType, request.Details);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking activity");
                return StatusCode(500, new { error = "Activity tracking failed" });
            }
        }

        #endregion

        #region Ecosystem Collaboration Endpoints

        /// <summary>
        /// Get available ecosystem partners
        /// GET /api/trial/ecosystem/partners
        /// </summary>
        [HttpGet("ecosystem/partners")]
        [Authorize]
        public async Task<IActionResult> GetPartners([FromQuery] string? sector = null)
        {
            try
            {
                var partners = await _trialService.GetAvailablePartnersAsync(sector ?? "all");
                return Ok(new { total = partners.Count, partners });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting partners");
                return StatusCode(500, new { error = "Failed to get partners" });
            }
        }

        /// <summary>
        /// Request ecosystem partner connection
        /// POST /api/trial/ecosystem/connect
        /// </summary>
        [HttpPost("ecosystem/connect")]
        [Authorize]
        public async Task<IActionResult> RequestConnection([FromBody] EcosystemPartnerRequest request)
        {
            try
            {
                var tenantId = GetTenantIdFromClaims();
                if (tenantId == Guid.Empty)
                    return Unauthorized(new { error = "Tenant not found" });

                var result = await _trialService.RequestPartnerConnectionAsync(tenantId, request);

                if (!result.Success)
                    return BadRequest(new { error = result.Message });

                return Ok(new
                {
                    success = true,
                    connectionId = result.ConnectionId,
                    status = result.Status,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting connection");
                return StatusCode(500, new { error = "Connection request failed" });
            }
        }

        /// <summary>
        /// Get tenant's ecosystem connections
        /// GET /api/trial/ecosystem/connections
        /// </summary>
        [HttpGet("ecosystem/connections")]
        [Authorize]
        public async Task<IActionResult> GetConnections()
        {
            try
            {
                var tenantId = GetTenantIdFromClaims();
                if (tenantId == Guid.Empty)
                    return Unauthorized(new { error = "Tenant not found" });

                var connections = await _trialService.GetEcosystemConnectionsAsync(tenantId);
                return Ok(new { total = connections.Count, connections });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting connections");
                return StatusCode(500, new { error = "Failed to get connections" });
            }
        }

        /// <summary>
        /// Approve partner connection
        /// POST /api/trial/ecosystem/approve/{connectionId}
        /// </summary>
        [HttpPost("ecosystem/approve/{connectionId}")]
        [Authorize]
        public async Task<IActionResult> ApproveConnection(Guid connectionId)
        {
            try
            {
                var approvedBy = User.Identity?.Name ?? "user";
                var success = await _trialService.ApprovePartnerConnectionAsync(connectionId, approvedBy);

                if (!success)
                    return BadRequest(new { error = "Approval failed" });

                return Ok(new { success = true, message = "Connection approved" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving connection");
                return StatusCode(500, new { error = "Approval failed" });
            }
        }

        /// <summary>
        /// Reject partner connection
        /// POST /api/trial/ecosystem/reject/{connectionId}
        /// </summary>
        [HttpPost("ecosystem/reject/{connectionId}")]
        [Authorize]
        public async Task<IActionResult> RejectConnection(Guid connectionId, [FromBody] RejectConnectionRequest request)
        {
            try
            {
                var rejectedBy = User.Identity?.Name ?? "user";
                var success = await _trialService.RejectPartnerConnectionAsync(connectionId, request.Reason, rejectedBy);

                if (!success)
                    return BadRequest(new { error = "Rejection failed" });

                return Ok(new { success = true, message = "Connection rejected" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting connection");
                return StatusCode(500, new { error = "Rejection failed" });
            }
        }

        /// <summary>
        /// Track partner interaction
        /// POST /api/trial/ecosystem/track/{connectionId}
        /// </summary>
        [HttpPost("ecosystem/track/{connectionId}")]
        [Authorize]
        public async Task<IActionResult> TrackPartnerInteraction(Guid connectionId, [FromBody] TrackInteractionRequest request)
        {
            try
            {
                await _trialService.TrackPartnerInteractionAsync(connectionId, request.InteractionType, request.Details);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking interaction");
                return StatusCode(500, new { error = "Tracking failed" });
            }
        }

        #endregion

        #region Integrations Marketplace

        /// <summary>
        /// Get available integrations
        /// GET /api/trial/integrations
        /// </summary>
        [HttpGet("integrations")]
        [Authorize]
        public async Task<IActionResult> GetIntegrations([FromQuery] string? category = null)
        {
            try
            {
                var integrations = await _trialService.GetAvailableIntegrationsAsync(category ?? "");
                return Ok(new { total = integrations.Count, integrations });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting integrations");
                return StatusCode(500, new { error = "Failed to get integrations" });
            }
        }

        /// <summary>
        /// Connect an integration
        /// POST /api/trial/integrations/connect
        /// </summary>
        [HttpPost("integrations/connect")]
        [Authorize]
        public async Task<IActionResult> ConnectIntegration([FromBody] ConnectIntegrationRequest request)
        {
            try
            {
                var tenantId = GetTenantIdFromClaims();
                if (tenantId == Guid.Empty)
                    return Unauthorized(new { error = "Tenant not found" });

                var result = await _trialService.ConnectIntegrationAsync(tenantId, request.IntegrationCode, request.Config ?? new());

                if (!result.Success)
                    return BadRequest(new { error = result.Message, status = result.Status });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error connecting integration");
                return StatusCode(500, new { error = "Integration connection failed" });
            }
        }

        #endregion

        #region Admin Endpoints

        /// <summary>
        /// Admin: Get trial analytics dashboard
        /// GET /api/trial/admin/dashboard
        /// </summary>
        [HttpGet("admin/dashboard")]
        [Authorize(Roles = "admin,platform_admin")]
        public async Task<IActionResult> GetAnalytics()
        {
            try
            {
                var analytics = await _trialService.GetTrialAnalyticsAsync();
                return Ok(analytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting analytics");
                return StatusCode(500, new { error = "Failed to get analytics" });
            }
        }

        /// <summary>
        /// Admin: Get all active trials
        /// GET /api/trial/admin/list
        /// </summary>
        [HttpGet("admin/list")]
        [Authorize(Roles = "admin,platform_admin")]
        public async Task<IActionResult> GetActiveTrials()
        {
            try
            {
                var trials = await _trialService.GetActiveTrialsAsync();
                return Ok(new { total = trials.Count, trials });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trials");
                return StatusCode(500, new { error = "Failed to get trials" });
            }
        }

        /// <summary>
        /// Admin: Extend trial manually
        /// POST /api/trial/admin/{tenantId}/extend
        /// </summary>
        [HttpPost("admin/{tenantId}/extend")]
        [Authorize(Roles = "admin,platform_admin")]
        public async Task<IActionResult> AdminExtend(Guid tenantId, [FromBody] AdminExtendRequest request)
        {
            try
            {
                var adminUserId = User.Identity?.Name ?? "admin";
                var success = await _trialService.AdminExtendTrialAsync(tenantId, request.Days, adminUserId);

                if (!success)
                    return BadRequest(new { error = "Extension failed" });

                return Ok(new { success = true, message = $"Trial extended by {request.Days} days" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error admin extending trial");
                return StatusCode(500, new { error = "Extension failed" });
            }
        }

        /// <summary>
        /// Admin: Expire trial immediately
        /// POST /api/trial/admin/{tenantId}/expire
        /// </summary>
        [HttpPost("admin/{tenantId}/expire")]
        [Authorize(Roles = "admin,platform_admin")]
        public async Task<IActionResult> AdminExpire(Guid tenantId, [FromBody] AdminExpireRequest request)
        {
            try
            {
                var adminUserId = User.Identity?.Name ?? "admin";
                var success = await _trialService.AdminExpireTrialAsync(tenantId, adminUserId, request.Reason);

                if (!success)
                    return BadRequest(new { error = "Expiration failed" });

                return Ok(new { success = true, message = "Trial expired" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error admin expiring trial");
                return StatusCode(500, new { error = "Expiration failed" });
            }
        }

        #endregion

        #region Helpers

        private Guid GetTenantIdFromClaims()
        {
            // Check both claim names for compatibility
            var tenantClaim = User.FindFirst(ClaimConstants.TenantId)?.Value
                ?? User.FindFirst("tenant_id")?.Value;
            return Guid.TryParse(tenantClaim, out var tenantId) ? tenantId : Guid.Empty;
        }

        #endregion
    }

    #region Request DTOs

    public class ProvisionRequest
    {
        public Guid SignupId { get; set; }
        /// <summary>
        /// Password for the admin user account (min 8 characters)
        /// </summary>
        public string Password { get; set; } = string.Empty;
    }

    public class ActivateRequest
    {
        public Guid TenantId { get; set; }
        public string Token { get; set; } = string.Empty;
    }

    public class TrialExtendRequest
    {
        public string? Reason { get; set; }
    }

    public class ConvertRequest
    {
        public string PlanCode { get; set; } = "professional";
    }

    public class AdminExtendRequest
    {
        public int Days { get; set; } = 7;
    }

    public class AdminExpireRequest
    {
        public string Reason { get; set; } = "Admin action";
    }

    public class TrackActivityRequest
    {
        public Guid UserId { get; set; }
        public string ActivityType { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }

    public class RejectConnectionRequest
    {
        public string Reason { get; set; } = string.Empty;
    }

    public class TrackInteractionRequest
    {
        public string InteractionType { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }

    public class ConnectIntegrationRequest
    {
        public string IntegrationCode { get; set; } = string.Empty;
        public Dictionary<string, string>? Config { get; set; }
    }

    #endregion
}
