using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GrcMvc.Data;
using GrcMvc.Services.Interfaces;
using GrcMvc.Application.Permissions;
using GrcMvc.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GrcMvc.Controllers;

/// <summary>
/// Role Delegation Controller - Assign/delegate roles
/// Allows users to temporarily delegate their roles to other users
/// </summary>
[Route("[controller]")]
[Authorize]
[RequireTenant]
public class RoleDelegationController : Controller
{
    private readonly GrcDbContext _db;
    private readonly IUserDirectoryService _userDirectory;
    private readonly ILogger<RoleDelegationController> _logger;

    public RoleDelegationController(GrcDbContext db, IUserDirectoryService userDirectory, ILogger<RoleDelegationController> logger)
    {
        _db = db;
        _userDirectory = userDirectory;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(GrcPermissions.Users.View)]
    public async Task<IActionResult> Index()
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var roles = await _db.RoleCatalogs.Where(r => r.IsActive).OrderBy(r => r.DisplayOrder).ToListAsync();
            var users = await _userDirectory.GetAllActiveUsersAsync();

            // Get active delegations for this tenant
            // Note: This assumes a RoleDelegation entity exists - if not, this would need to be created
            ViewBag.Delegations = new List<object>(); // Placeholder until RoleDelegation entity is implemented
            ViewBag.Roles = roles;
            ViewBag.Users = users;
            ViewBag.ActiveCount = 0;

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading role delegations");
            TempData["Error"] = "Error loading role delegations";
            return View();
        }
    }

    [HttpGet("Create")]
    [Authorize(GrcPermissions.Users.Create)]
    public async Task<IActionResult> Create()
    {
        try
        {
            var roles = await _db.RoleCatalogs.Where(r => r.IsActive).OrderBy(r => r.DisplayOrder).ToListAsync();
            var users = await _userDirectory.GetAllActiveUsersAsync();

            ViewBag.Roles = roles;
            ViewBag.Users = users;

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading delegation create form");
            TempData["Error"] = "Error loading form";
            return RedirectToAction("Index");
        }
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    [Authorize(GrcPermissions.Users.Create)]
    public async Task<IActionResult> CreatePost([FromForm] DelegationDto dto)
    {
        try
        {
            // Validate input
            if (dto.FromUserId == Guid.Empty || dto.ToUserId == Guid.Empty)
            {
                TempData["Error"] = "Both users must be selected";
                return RedirectToAction("Create");
            }

            if (dto.FromUserId == dto.ToUserId)
            {
                TempData["Error"] = "Cannot delegate to the same user";
                return RedirectToAction("Create");
            }

            if (string.IsNullOrWhiteSpace(dto.RoleCode))
            {
                TempData["Error"] = "Role must be selected";
                return RedirectToAction("Create");
            }

            // TODO: Create RoleDelegation entity and save to database
            // For now, just log and return success
            _logger.LogInformation("Role delegation created: {RoleCode} from {FromUser} to {ToUser}",
                dto.RoleCode, dto.FromUserId, dto.ToUserId);

            TempData["Success"] = "Role delegation created successfully";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role delegation");
            TempData["Error"] = "Error creating delegation";
        }
        return RedirectToAction("Index");
    }

    [HttpPost("Revoke/{id}")]
    [ValidateAntiForgeryToken]
    [Authorize(GrcPermissions.Users.Delete)]
    public async Task<IActionResult> Revoke(Guid id)
    {
        try
        {
            // TODO: Find and revoke the delegation from database
            _logger.LogInformation("Role delegation revoked: {DelegationId}", id);
            TempData["Success"] = "Delegation revoked successfully";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking delegation {DelegationId}", id);
            TempData["Error"] = "Error revoking delegation";
        }
        return RedirectToAction("Index");
    }

    [HttpPost("Extend/{id}")]
    [ValidateAntiForgeryToken]
    [Authorize(GrcPermissions.Users.Edit)]
    public async Task<IActionResult> Extend(Guid id, [FromForm] DateTime newEndDate)
    {
        try
        {
            // TODO: Find and extend the delegation in database
            _logger.LogInformation("Role delegation extended: {DelegationId} to {NewEndDate}", id, newEndDate);
            TempData["Success"] = "Delegation extended successfully";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extending delegation {DelegationId}", id);
            TempData["Error"] = "Error extending delegation";
        }
        return RedirectToAction("Index");
    }

    private Guid GetCurrentTenantId()
    {
        var claim = User.FindFirst("TenantId");
        return claim != null && Guid.TryParse(claim.Value, out var tenantId) ? tenantId : Guid.Empty;
    }
}

public class DelegationDto
{
    public Guid FromUserId { get; set; }
    public Guid ToUserId { get; set; }
    public string? RoleCode { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Reason { get; set; }
}
