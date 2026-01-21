using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrcMvc.Data;
using GrcMvc.Services.Interfaces;
using GrcMvc.Application.Permissions;
using GrcMvc.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GrcMvc.Controllers;

/// <summary>
/// User Invitation Controller
/// Invite team members and assign roles
/// </summary>
[Authorize]
[RequireTenant]
[Route("[controller]")]
public class InvitationController : Controller
{
    private readonly GrcDbContext _db;
    private readonly IUserDirectoryService _userDirectory;
    private readonly ILogger<InvitationController> _logger;

    public InvitationController(GrcDbContext db, IUserDirectoryService userDirectory, ILogger<InvitationController> logger)
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
            var users = await _userDirectory.GetAllActiveUsersAsync();
            var pendingInvites = await _db.TenantUsers
                .Where(tu => tu.TenantId == tenantId && tu.Status == "Pending")
                .OrderByDescending(tu => tu.InvitedAt)
                .ToListAsync();

            ViewBag.Users = users;
            ViewBag.PendingInvites = pendingInvites;

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading invitations");
            TempData["Error"] = "Error loading invitations";
            return View();
        }
    }

    [HttpGet("Invite")]
    [Authorize(GrcPermissions.Users.Create)]
    public async Task<IActionResult> Invite()
    {
        try
        {
            var roles = await _db.RoleCatalogs.Where(r => r.IsActive).OrderBy(r => r.DisplayOrder).ToListAsync();
            ViewBag.Roles = roles;
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading invite form");
            TempData["Error"] = "Error loading invite form";
            return RedirectToAction("Index");
        }
    }

    [HttpPost("Invite")]
    [ValidateAntiForgeryToken]
    [Authorize(GrcPermissions.Users.Create)]
    public async Task<IActionResult> InvitePost([FromForm] InviteDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                TempData["Error"] = "Email is required";
                return RedirectToAction("Invite");
            }

            var tenantId = GetCurrentTenantId();

            // Check if user already exists
            var existingUser = await _db.TenantUsers
                .FirstOrDefaultAsync(tu => tu.TenantId == tenantId && tu.Email == dto.Email);

            if (existingUser != null)
            {
                TempData["Error"] = "User already exists in this tenant";
                return RedirectToAction("Index");
            }

            // Create invitation
            var invitation = new Models.Entities.TenantUser
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Email = dto.Email,
                RoleCode = dto.RoleCode ?? "USER",
                Status = "Pending",
                InvitationToken = Guid.NewGuid().ToString("N"),
                InvitedAt = DateTime.UtcNow,
                InvitationExpiresAt = DateTime.UtcNow.AddDays(7),
                InvitedBy = User.Identity?.Name ?? "system"
            };

            _db.TenantUsers.Add(invitation);
            await _db.SaveChangesAsync();

            // TODO: Send invitation email

            TempData["Success"] = $"Invitation sent to {dto.Email}";
            _logger.LogInformation("Invitation sent to {Email} for tenant {TenantId}", dto.Email, tenantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending invitation to {Email}", dto.Email);
            TempData["Error"] = "Error sending invitation";
        }
        return RedirectToAction("Index");
    }

    [HttpPost("Resend/{id}")]
    [ValidateAntiForgeryToken]
    [Authorize(GrcPermissions.Users.Edit)]
    public async Task<IActionResult> Resend(Guid id)
    {
        try
        {
            var invitation = await _db.TenantUsers.FirstOrDefaultAsync(tu => tu.Id == id);
            if (invitation == null) return NotFound();

            invitation.InvitationToken = Guid.NewGuid().ToString("N");
            invitation.InvitedAt = DateTime.UtcNow;
            invitation.InvitationExpiresAt = DateTime.UtcNow.AddDays(7);

            await _db.SaveChangesAsync();

            // TODO: Send invitation email

            TempData["Success"] = $"Invitation resent to {invitation.Email}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending invitation {InvitationId}", id);
            TempData["Error"] = "Error resending invitation";
        }
        return RedirectToAction("Index");
    }

    [HttpPost("Cancel/{id}")]
    [ValidateAntiForgeryToken]
    [Authorize(GrcPermissions.Users.Delete)]
    public async Task<IActionResult> Cancel(Guid id)
    {
        try
        {
            var invitation = await _db.TenantUsers.FirstOrDefaultAsync(tu => tu.Id == id && tu.Status == "Pending");
            if (invitation == null) return NotFound();

            _db.TenantUsers.Remove(invitation);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Invitation cancelled";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling invitation {InvitationId}", id);
            TempData["Error"] = "Error cancelling invitation";
        }
        return RedirectToAction("Index");
    }

    private Guid GetCurrentTenantId()
    {
        var claim = User.FindFirst("TenantId");
        return claim != null && Guid.TryParse(claim.Value, out var tenantId) ? tenantId : Guid.Empty;
    }
}

public class InviteDto
{
    public string? Email { get; set; }
    public string? RoleCode { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}
