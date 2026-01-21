using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrcMvc.Data;
using GrcMvc.Models.Entities.Catalogs;
using GrcMvc.Services.Interfaces;
using GrcMvc.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GrcMvc.Controllers;

// NOTE: AssessmentTemplateController has been moved to AssessmentTemplateController.cs
// NOTE: DocumentFlowController has been moved to DocumentFlowController.cs

/// <summary>
/// Role Profile Controller - Manage roles, titles, and user assignments
/// </summary>
[Authorize]
[RequireTenant]
[Route("[controller]")]
public class RoleProfileController : Controller
{
    private readonly GrcDbContext _db;
    private readonly IUserDirectoryService _userDirectory;
    private readonly ILogger<RoleProfileController> _logger;

    public RoleProfileController(GrcDbContext db, IUserDirectoryService userDirectory, ILogger<RoleProfileController> logger)
    {
        _db = db;
        _userDirectory = userDirectory;
        _logger = logger;
    }

    /// <summary>
    /// Role Profile Dashboard
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var model = new RoleProfileDashboard
            {
                Roles = await _db.RoleCatalogs.Where(r => r.IsActive).OrderBy(r => r.DisplayOrder).ToListAsync(),
                Titles = await _db.TitleCatalogs.Where(t => t.IsActive).OrderBy(t => t.DisplayOrder).ToListAsync(),
                TotalUsers = await _userDirectory.GetUserCountAsync(),
                TotalRoles = await _db.RoleCatalogs.CountAsync(r => r.IsActive),
                TotalTitles = await _db.TitleCatalogs.CountAsync(t => t.IsActive)
            };
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading role profile dashboard");
            TempData["Error"] = "Error loading dashboard";
            return View(new RoleProfileDashboard());
        }
    }

    /// <summary>
    /// View all roles
    /// </summary>
    [HttpGet("Roles")]
    public async Task<IActionResult> Roles()
    {
        try
        {
            var roles = await _db.RoleCatalogs.Where(r => r.IsActive).OrderBy(r => r.DisplayOrder).ToListAsync();
            return View(roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading roles");
            TempData["Error"] = "Error loading roles";
            return View(new List<RoleCatalog>());
        }
    }

    /// <summary>
    /// View all titles
    /// </summary>
    [HttpGet("Titles")]
    public async Task<IActionResult> Titles()
    {
        try
        {
            var titles = await _db.TitleCatalogs.Include(t => t.RoleCatalog).Where(t => t.IsActive).OrderBy(t => t.DisplayOrder).ToListAsync();
            return View(titles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading titles");
            TempData["Error"] = "Error loading titles";
            return View(new List<TitleCatalog>());
        }
    }

    /// <summary>
    /// My Profile - Current user's role and permissions
    /// </summary>
    [HttpGet("MyProfile")]
    public async Task<IActionResult> MyProfile()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var user = await _userDirectory.GetUserByIdAsync(userId);
            return View(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user profile");
            TempData["Error"] = "Error loading profile";
            return RedirectToAction(nameof(Index));
        }
    }
}

#region View Models

public class RoleProfileDashboard
{
    public List<RoleCatalog> Roles { get; set; } = new();
    public List<TitleCatalog> Titles { get; set; } = new();
    public int TotalUsers { get; set; }
    public int TotalRoles { get; set; }
    public int TotalTitles { get; set; }
}

#endregion
