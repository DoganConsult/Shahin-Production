using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrcMvc.Services.Interfaces;
using GrcMvc.Attributes;

namespace GrcMvc.Controllers.Api;

/// <summary>
/// API Controller for checking permissions
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PermissionController : ControllerBase
{
    private readonly IPermissionService _permissionService;
    private readonly ILogger<PermissionController> _logger;

    public PermissionController(
        IPermissionService permissionService,
        ILogger<PermissionController> logger)
    {
        _permissionService = permissionService;
        _logger = logger;
    }

    /// <summary>
    /// Check if current user has a specific permission
    /// </summary>
    [HttpGet("check")]
    public async Task<IActionResult> CheckPermission([FromQuery] string permission)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var hasPermission = await _permissionService.HasPermissionAsync(userId.Value, permission);
        return Ok(new { hasPermission });
    }

    /// <summary>
    /// Check if current user has any of the specified permissions
    /// </summary>
    [HttpPost("check-any")]
    public async Task<IActionResult> CheckAnyPermission([FromBody] string[] permissions)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var hasPermission = await _permissionService.HasAnyPermissionAsync(userId.Value, permissions);
        return Ok(new { hasPermission });
    }

    /// <summary>
    /// Check if current user has all specified permissions
    /// </summary>
    [HttpPost("check-all")]
    public async Task<IActionResult> CheckAllPermissions([FromBody] string[] permissions)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var hasPermission = await _permissionService.HasAllPermissionsAsync(userId.Value, permissions);
        return Ok(new { hasPermission });
    }

    /// <summary>
    /// Get all permissions for current user
    /// </summary>
    [HttpGet("my-permissions")]
    public async Task<IActionResult> GetUserPermissions()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var permissions = await _permissionService.GetUserPermissionsAsync(userId.Value);
        return Ok(new { permissions });
    }

    /// <summary>
    /// Get available permission categories
    /// </summary>
    [HttpGet("categories")]
    [RequirePlatformAdmin]
    public IActionResult GetPermissionCategories()
    {
        var categories = new[]
        {
            new { name = "platform", description = "Platform administration permissions" },
            new { name = "tenant", description = "Tenant management permissions" },
            new { name = "onboarding", description = "Onboarding wizard permissions" },
            new { name = "grc", description = "GRC module permissions" },
            new { name = "profile", description = "User profile permissions" },
            new { name = "dashboard", description = "Dashboard access permissions" }
        };

        return Ok(categories);
    }

    /// <summary>
    /// Get all available permissions (for admin reference)
    /// </summary>
    [HttpGet("all")]
    [RequirePlatformAdmin]
    public IActionResult GetAllPermissions()
    {
        var allPermissions = new[]
        {
            // Platform permissions
            new { key = "platform.admin", category = "platform", description = "Full platform administration" },
            new { key = "platform.users.create", category = "platform", description = "Create platform users" },
            new { key = "platform.users.read", category = "platform", description = "View platform users" },
            new { key = "platform.users.update", category = "platform", description = "Update platform users" },
            new { key = "platform.users.delete", category = "platform", description = "Delete platform users" },
            new { key = "platform.tenants.create", category = "platform", description = "Create tenants" },
            new { key = "platform.tenants.read", category = "platform", description = "View tenants" },
            new { key = "platform.tenants.update", category = "platform", description = "Update tenants" },
            new { key = "platform.tenants.delete", category = "platform", description = "Delete tenants" },
            new { key = "platform.settings.read", category = "platform", description = "View platform settings" },
            new { key = "platform.settings.update", category = "platform", description = "Update platform settings" },
            
            // Tenant permissions
            new { key = "tenant.view", category = "tenant", description = "View tenant information" },
            new { key = "tenant.update", category = "tenant", description = "Update tenant information" },
            new { key = "tenant.users.create", category = "tenant", description = "Create tenant users" },
            new { key = "tenant.users.read", category = "tenant", description = "View tenant users" },
            new { key = "tenant.users.update", category = "tenant", description = "Update tenant users" },
            new { key = "tenant.users.delete", category = "tenant", description = "Delete tenant users" },
            
            // Onboarding permissions
            new { key = "onboarding.view", category = "onboarding", description = "View onboarding wizard" },
            new { key = "onboarding.update", category = "onboarding", description = "Update onboarding data" },
            new { key = "onboarding.admin", category = "onboarding", description = "Administer onboarding process" },
            
            // GRC permissions
            new { key = "grc.controls.read", category = "grc", description = "View GRC controls" },
            new { key = "grc.controls.update", category = "grc", description = "Update GRC controls" },
            new { key = "grc.risks.read", category = "grc", description = "View risk assessments" },
            new { key = "grc.risks.update", category = "grc", description = "Update risk assessments" },
            new { key = "grc.compliance.read", category = "grc", description = "View compliance reports" },
            new { key = "grc.compliance.update", category = "grc", description = "Update compliance data" },
            
            // Basic permissions
            new { key = "profile.view", category = "profile", description = "View own profile" },
            new { key = "profile.update", category = "profile", description = "Update own profile" },
            new { key = "dashboard.view", category = "dashboard", description = "Access dashboard" }
        };

        return Ok(allPermissions);
    }

    /// <summary>
    /// Get current user ID from claims
    /// </summary>
    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) ??
                         User.FindFirst("sub");

        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }

        return null;
    }
}
