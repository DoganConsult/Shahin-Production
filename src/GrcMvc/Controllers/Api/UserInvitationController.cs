using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrcMvc.Controllers.Api
{
    /// <summary>
    /// PHASE 6: User Invitation API Controller
    /// Handles user invitations, role assignments, and activation
    /// </summary>
    [Route("api/tenants/{tenantId}/users")]
    [ApiController]
    [IgnoreAntiforgeryToken]
    public class UserInvitationController : ControllerBase
    {
        private readonly IUserInvitationService _invitationService;
        private readonly ILogger<UserInvitationController> _logger;

        public UserInvitationController(
            IUserInvitationService invitationService,
            ILogger<UserInvitationController> logger)
        {
            _invitationService = invitationService;
            _logger = logger;
        }

        /// <summary>
        /// Invite a user to the tenant
        /// POST /api/tenants/{tenantId}/users/invite
        /// </summary>
        [HttpPost("invite")]
        [Authorize]
        public async Task<IActionResult> InviteUser(Guid tenantId, [FromBody] InviteUserRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var invitedBy = User.Identity?.Name ?? "System";
                var tenantUser = await _invitationService.InviteUserAsync(
                    tenantId,
                    request.Email,
                    request.FirstName,
                    request.LastName,
                    request.RoleCode,
                    request.TitleCode,
                    invitedBy);

                return Ok(new
                {
                    success = true,
                    message = $"Invitation sent to {request.Email}",
                    tenantUserId = tenantUser.Id,
                    status = tenantUser.Status
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inviting user");
                return StatusCode(500, new { error = "An error occurred while sending the invitation" });
            }
        }

        /// <summary>
        /// Get all users for a tenant
        /// GET /api/tenants/{tenantId}/users
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUsers(Guid tenantId)
        {
            try
            {
                var users = await _invitationService.GetTenantUsersAsync(tenantId);

                return Ok(new
                {
                    total = users.Count,
                    users = users.Select(u => new
                    {
                        u.Id,
                        u.UserId,
                        Email = u.User?.Email,
                        FirstName = u.User?.FirstName,
                        LastName = u.User?.LastName,
                        u.RoleCode,
                        u.TitleCode,
                        u.Status,
                        u.InvitedAt,
                        u.ActivatedAt,
                        u.InvitedBy
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tenant users");
                return StatusCode(500, new { error = "An error occurred" });
            }
        }

        /// <summary>
        /// Get pending invitations
        /// GET /api/tenants/{tenantId}/users/pending
        /// </summary>
        [HttpGet("pending")]
        [Authorize]
        public async Task<IActionResult> GetPendingInvitations(Guid tenantId)
        {
            try
            {
                var pending = await _invitationService.GetPendingInvitationsAsync(tenantId);

                return Ok(new
                {
                    total = pending.Count,
                    invitations = pending.Select(u => new
                    {
                        u.Id,
                        Email = u.User?.Email,
                        FirstName = u.User?.FirstName,
                        LastName = u.User?.LastName,
                        u.RoleCode,
                        u.TitleCode,
                        u.InvitedAt,
                        u.InvitedBy
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending invitations");
                return StatusCode(500, new { error = "An error occurred" });
            }
        }

        /// <summary>
        /// Resend invitation
        /// POST /api/tenants/{tenantId}/users/{tenantUserId}/resend
        /// </summary>
        [HttpPost("{tenantUserId}/resend")]
        [Authorize]
        public async Task<IActionResult> ResendInvitation(Guid tenantId, Guid tenantUserId)
        {
            try
            {
                var requestedBy = User.Identity?.Name ?? "System";
                var success = await _invitationService.ResendInvitationAsync(tenantUserId, requestedBy);

                if (!success)
                    return BadRequest(new { error = "Unable to resend invitation. User may not be in pending status." });

                return Ok(new { success = true, message = "Invitation resent" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending invitation");
                return StatusCode(500, new { error = "An error occurred" });
            }
        }

        /// <summary>
        /// Cancel invitation
        /// DELETE /api/tenants/{tenantId}/users/{tenantUserId}/invitation
        /// </summary>
        [HttpDelete("{tenantUserId}/invitation")]
        [Authorize]
        public async Task<IActionResult> CancelInvitation(Guid tenantId, Guid tenantUserId)
        {
            try
            {
                var cancelledBy = User.Identity?.Name ?? "System";
                var success = await _invitationService.CancelInvitationAsync(tenantUserId, cancelledBy);

                if (!success)
                    return BadRequest(new { error = "Unable to cancel invitation" });

                return Ok(new { success = true, message = "Invitation cancelled" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling invitation");
                return StatusCode(500, new { error = "An error occurred" });
            }
        }

        /// <summary>
        /// Update user role
        /// PUT /api/tenants/{tenantId}/users/{tenantUserId}/role
        /// </summary>
        [HttpPut("{tenantUserId}/role")]
        [Authorize]
        public async Task<IActionResult> UpdateUserRole(
            Guid tenantId,
            Guid tenantUserId,
            [FromBody] UpdateRoleRequest request)
        {
            try
            {
                var updatedBy = User.Identity?.Name ?? "System";
                var tenantUser = await _invitationService.UpdateUserRoleAsync(
                    tenantUserId,
                    request.RoleCode,
                    request.TitleCode,
                    updatedBy);

                return Ok(new
                {
                    success = true,
                    tenantUserId = tenantUser.Id,
                    roleCode = tenantUser.RoleCode,
                    titleCode = tenantUser.TitleCode
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user role");
                return StatusCode(500, new { error = "An error occurred" });
            }
        }

        /// <summary>
        /// Suspend user
        /// POST /api/tenants/{tenantId}/users/{tenantUserId}/suspend
        /// </summary>
        [HttpPost("{tenantUserId}/suspend")]
        [Authorize]
        public async Task<IActionResult> SuspendUser(Guid tenantId, Guid tenantUserId)
        {
            try
            {
                var suspendedBy = User.Identity?.Name ?? "System";
                var success = await _invitationService.SuspendUserAsync(tenantUserId, suspendedBy);

                if (!success)
                    return BadRequest(new { error = "Unable to suspend user" });

                return Ok(new { success = true, message = "User suspended" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error suspending user");
                return StatusCode(500, new { error = "An error occurred" });
            }
        }

        /// <summary>
        /// Reactivate user
        /// POST /api/tenants/{tenantId}/users/{tenantUserId}/reactivate
        /// </summary>
        [HttpPost("{tenantUserId}/reactivate")]
        [Authorize]
        public async Task<IActionResult> ReactivateUser(Guid tenantId, Guid tenantUserId)
        {
            try
            {
                var reactivatedBy = User.Identity?.Name ?? "System";
                var success = await _invitationService.ReactivateUserAsync(tenantUserId, reactivatedBy);

                if (!success)
                    return BadRequest(new { error = "Unable to reactivate user" });

                return Ok(new { success = true, message = "User reactivated" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reactivating user");
                return StatusCode(500, new { error = "An error occurred" });
            }
        }

        /// <summary>
        /// Get user's tasks
        /// GET /api/tenants/{tenantId}/users/{userId}/tasks
        /// </summary>
        [HttpGet("{userId}/tasks")]
        [Authorize]
        public async Task<IActionResult> GetUserTasks(Guid tenantId, string userId)
        {
            try
            {
                var tasks = await _invitationService.GetUserTasksAsync(tenantId, userId);

                return Ok(new
                {
                    total = tasks.Count,
                    tasks = tasks.Select(t => new
                    {
                        t.Id,
                        t.TaskName,
                        t.Description,
                        t.Status,
                        t.Priority,
                        t.DueDate,
                        AssignedToUser = t.AssignedToUserName,
                        WorkflowInstanceId = t.WorkflowInstanceId
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user tasks");
                return StatusCode(500, new { error = "An error occurred" });
            }
        }
    }

    /// <summary>
    /// Accept invitation (public endpoint)
    /// </summary>
    [Route("api/invitation")]
    [ApiController]
    [IgnoreAntiforgeryToken]
    public class InvitationAcceptController : ControllerBase
    {
        private readonly IUserInvitationService _invitationService;
        private readonly ILogger<InvitationAcceptController> _logger;

        public InvitationAcceptController(
            IUserInvitationService invitationService,
            ILogger<InvitationAcceptController> logger)
        {
            _invitationService = invitationService;
            _logger = logger;
        }

        /// <summary>
        /// Accept invitation and set password
        /// POST /api/invitation/accept
        /// </summary>
        [HttpPost("accept")]
        [AllowAnonymous]
        public async Task<IActionResult> AcceptInvitation([FromBody] AcceptInvitationRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var tenantUser = await _invitationService.AcceptInvitationAsync(
                    request.Token,
                    request.Password);

                return Ok(new
                {
                    success = true,
                    message = "Invitation accepted. You can now log in.",
                    tenantId = tenantUser.TenantId,
                    roleCode = tenantUser.RoleCode,
                    titleCode = tenantUser.TitleCode
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting invitation");
                return StatusCode(500, new { error = "An error occurred" });
            }
        }
    }

    #region Request DTOs

    public class InviteUserRequest
    {
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string RoleCode { get; set; } = string.Empty;
        public string TitleCode { get; set; } = string.Empty;
    }

    public class UpdateRoleRequest
    {
        public string RoleCode { get; set; } = string.Empty;
        public string TitleCode { get; set; } = string.Empty;
    }

    public class AcceptInvitationRequest
    {
        public string Token { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    #endregion
}
