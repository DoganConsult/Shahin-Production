using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

using Microsoft.Extensions.Localization;
using GrcMvc.Resources;
namespace GrcMvc.Controllers.Api;

/// <summary>
/// API Controller for Platform Admin management
/// Super power feature - bulletproof with comprehensive error handling
/// </summary>
[ApiController]
[Route("api/platform-admin")]
[Authorize(Policy = "ActivePlatformAdmin")]
public class PlatformAdminController : ControllerBase
{
    private readonly IPlatformAdminService _platformAdminService;
    private readonly ILogger<PlatformAdminController> _logger;

    public PlatformAdminController(
        IPlatformAdminService platformAdminService,
        ILogger<PlatformAdminController> logger)
    {
        _platformAdminService = platformAdminService;
        _logger = logger;
    }

    private string GetCurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    #region Query Endpoints

    /// <summary>
    /// Get all platform admins
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var admins = await _platformAdminService.GetAllAsync();
            return Ok(admins.Select(ToDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting platform admins");
            return StatusCode(500, new { error = "Failed to retrieve platform admins" });
        }
    }

    /// <summary>
    /// Get platform admin by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var admin = await _platformAdminService.GetByIdAsync(id);
            if (admin == null)
                return NotFound(new { error = "Platform admin not found" });

            return Ok(ToDto(admin));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting platform admin {Id}", id);
            return StatusCode(500, new { error = "Failed to retrieve platform admin" });
        }
    }

    /// <summary>
    /// Get current user's platform admin profile
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentAdmin()
    {
        try
        {
            var userId = GetCurrentUserId();
            var admin = await _platformAdminService.GetByUserIdAsync(userId);
            if (admin == null)
                return NotFound(new { error = "You are not a platform admin" });

            return Ok(ToDto(admin));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current platform admin");
            return StatusCode(500, new { error = "Failed to retrieve profile" });
        }
    }

    /// <summary>
    /// Check if current user has a specific permission
    /// </summary>
    [HttpGet("permissions/{permission}")]
    public async Task<IActionResult> CheckPermission(string permission)
    {
        try
        {
            if (!Enum.TryParse<PlatformPermission>(permission, true, out var perm))
                return BadRequest(new { error = "Invalid permission" });

            var userId = GetCurrentUserId();
            var hasPermission = await _platformAdminService.HasPermissionAsync(userId, perm);
            return Ok(new { permission = permission, granted = hasPermission });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission {Permission}", permission);
            return StatusCode(500, new { error = "Failed to check permission" });
        }
    }

    /// <summary>
    /// Get active platform admins
    /// </summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveAdmins()
    {
        try
        {
            var admins = await _platformAdminService.GetActiveAdminsAsync();
            return Ok(admins.Select(ToDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active platform admins");
            return StatusCode(500, new { error = "Failed to retrieve active admins" });
        }
    }

    #endregion

    #region Command Endpoints

    /// <summary>
    /// Create a new platform admin
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePlatformAdminDto dto)
    {
        try
        {
            var creatorId = GetCurrentUserId();
            var admin = await _platformAdminService.CreateAsync(dto, creatorId);
            return CreatedAtAction(nameof(GetById), new { id = admin.Id }, ToDto(admin));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid("Access denied.");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = "An error occurred processing your request." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating platform admin");
            return StatusCode(500, new { error = "Failed to create platform admin" });
        }
    }

    /// <summary>
    /// Update a platform admin
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePlatformAdminDto dto)
    {
        try
        {
            var admin = await _platformAdminService.UpdateAsync(id, dto);
            return Ok(ToDto(admin));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = "The requested resource was not found." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating platform admin {Id}", id);
            return StatusCode(500, new { error = "Failed to update platform admin" });
        }
    }

    /// <summary>
    /// Suspend a platform admin
    /// </summary>
    [HttpPost("{id:guid}/suspend")]
    public async Task<IActionResult> Suspend(Guid id, [FromBody] SuspendRequest request)
    {
        try
        {
            var result = await _platformAdminService.SuspendAsync(id, request.Reason);
            if (!result)
                return BadRequest(new { error = "Cannot suspend this admin (may be Owner level)" });

            return Ok(new { message = "Platform admin suspended", id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error suspending platform admin {Id}", id);
            return StatusCode(500, new { error = "Failed to suspend platform admin" });
        }
    }

    /// <summary>
    /// Reactivate a suspended platform admin
    /// </summary>
    [HttpPost("{id:guid}/reactivate")]
    public async Task<IActionResult> Reactivate(Guid id)
    {
        try
        {
            var result = await _platformAdminService.ReactivateAsync(id);
            if (!result)
                return NotFound(new { error = "Platform admin not found" });

            return Ok(new { message = "Platform admin reactivated", id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reactivating platform admin {Id}", id);
            return StatusCode(500, new { error = "Failed to reactivate platform admin" });
        }
    }

    /// <summary>
    /// Delete a platform admin (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _platformAdminService.DeleteAsync(id);
            if (!result)
                return BadRequest(new { error = "Cannot delete this admin (may be Owner level)" });

            return Ok(new { message = "Platform admin deleted", id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting platform admin {Id}", id);
            return StatusCode(500, new { error = "Failed to delete platform admin" });
        }
    }

    #endregion

    #region Password Management Endpoints

    /// <summary>
    /// Reset own password
    /// </summary>
    [HttpPost("password/reset-own")]
    public async Task<IActionResult> ResetOwnPassword([FromBody] ResetOwnPasswordRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _platformAdminService.ResetPasswordAsync(userId, userId, request.NewPassword);
            if (!result)
                return BadRequest(new { error = "Password reset failed" });

            return Ok(new { message = "Password reset successful" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting own password");
            return StatusCode(500, new { error = "Failed to reset password" });
        }
    }

    /// <summary>
    /// Reset another user's password (requires permission)
    /// </summary>
    [HttpPost("password/reset")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        try
        {
            var adminUserId = GetCurrentUserId();
            var result = await _platformAdminService.ResetPasswordAsync(
                adminUserId, request.TargetUserId, request.NewPassword);

            if (!result)
                return BadRequest(new { error = "Password reset failed - check permissions" });

            return Ok(new { message = "Password reset successful", targetUserId = request.TargetUserId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password for {TargetUserId}", request.TargetUserId);
            return StatusCode(500, new { error = "Failed to reset password" });
        }
    }

    /// <summary>
    /// Force password change on next login
    /// </summary>
    [HttpPost("password/force-change/{targetUserId}")]
    public async Task<IActionResult> ForcePasswordChange(string targetUserId)
    {
        try
        {
            var adminUserId = GetCurrentUserId();
            var result = await _platformAdminService.ForcePasswordChangeAsync(adminUserId, targetUserId);
            if (!result)
                return BadRequest(new { error = "Force password change failed - check permissions" });

            return Ok(new { message = "Force password change set", targetUserId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error forcing password change for {TargetUserId}", targetUserId);
            return StatusCode(500, new { error = "Failed to force password change" });
        }
    }

    /// <summary>
    /// Restart tenant admin account with new credentials
    /// </summary>
    [HttpPost("tenant/{tenantId:guid}/restart-admin")]
    public async Task<IActionResult> RestartTenantAdmin(Guid tenantId)
    {
        try
        {
            var adminUserId = GetCurrentUserId();
            var (success, newPassword) = await _platformAdminService.RestartTenantAdminAccountAsync(
                adminUserId, tenantId);

            if (!success)
                return BadRequest(new { error = "Failed to restart tenant admin - check permissions" });

            return Ok(new
            {
                message = "Tenant admin account restarted",
                tenantId,
                newPassword,
                expiresIn = "72 hours",
                warning = "Share this password securely - it will not be shown again!"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restarting tenant admin for {TenantId}", tenantId);
            return StatusCode(500, new { error = "Failed to restart tenant admin" });
        }
    }

    #endregion

    #region Validation Endpoints

    /// <summary>
    /// Check if current admin can create a tenant
    /// </summary>
    [HttpGet("can-create-tenant")]
    public async Task<IActionResult> CanCreateTenant()
    {
        try
        {
            var userId = GetCurrentUserId();
            var canCreate = await _platformAdminService.CanCreateTenantAsync(userId);
            return Ok(new { canCreate });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking tenant creation permission");
            return StatusCode(500, new { error = "Failed to check permission" });
        }
    }

    /// <summary>
    /// Check if current admin can manage a specific tenant
    /// </summary>
    [HttpGet("can-manage-tenant/{tenantId:guid}")]
    public async Task<IActionResult> CanManageTenant(Guid tenantId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var canManage = await _platformAdminService.CanManageTenantAsync(userId, tenantId);
            return Ok(new { tenantId, canManage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking tenant management permission");
            return StatusCode(500, new { error = "Failed to check permission" });
        }
    }

    /// <summary>
    /// Get list of tenant IDs the current admin can manage
    /// </summary>
    [HttpGet("allowed-tenants")]
    public async Task<IActionResult> GetAllowedTenants()
    {
        try
        {
            var userId = GetCurrentUserId();
            var tenantIds = await _platformAdminService.GetAllowedTenantIdsAsync(userId);
            return Ok(new { tenantIds, count = tenantIds.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting allowed tenants");
            return StatusCode(500, new { error = "Failed to get allowed tenants" });
        }
    }

    #endregion

    #region Helper Methods

    private static PlatformAdminDto ToDto(PlatformAdmin admin) => new()
    {
        Id = admin.Id,
        UserId = admin.UserId.ToString(),
        DisplayName = admin.DisplayName,
        ContactEmail = admin.ContactEmail,
        ContactPhone = admin.ContactPhone,
        AdminLevel = admin.AdminLevel,
        Status = admin.Status,
        CanCreateTenants = admin.CanCreateTenants,
        CanManageTenants = admin.CanManageTenants,
        CanManagePlatformAdmins = admin.CanManagePlatformAdmins,
        LastLoginAt = admin.LastLoginAt,
        TotalTenantsCreated = admin.TotalTenantsCreated,
        CreatedAt = admin.CreatedAt
    };

    #endregion
}

#region Request DTOs

public class SuspendRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class ResetOwnPasswordRequest
{
    public string NewPassword { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    public string TargetUserId { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

#endregion
