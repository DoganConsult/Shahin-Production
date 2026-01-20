using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Identity;

namespace GrcMvc.Controllers;

/// <summary>
/// ABP Built-in User Management Controller
/// Uses ABP's IIdentityUserAppService for error-free user CRUD
/// </summary>
[Authorize(Roles = "PlatformAdmin,Admin")]
[Route("admin/users")]
public class AbpUserController : Controller
{
    private readonly IIdentityUserAppService _userAppService;
    private readonly IIdentityRoleAppService _roleAppService;
    private readonly ILogger<AbpUserController> _logger;

    public AbpUserController(
        IIdentityUserAppService userAppService,
        IIdentityRoleAppService roleAppService,
        ILogger<AbpUserController> logger)
    {
        _userAppService = userAppService;
        _roleAppService = roleAppService;
        _logger = logger;
    }

    /// <summary>
    /// List all users
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(string? filter = null, int page = 0)
    {
        var result = await _userAppService.GetListAsync(new GetIdentityUsersInput 
        { 
            Filter = filter,
            MaxResultCount = 50,
            SkipCount = page * 50
        });
        
        ViewBag.Filter = filter;
        ViewBag.Page = page;
        ViewBag.TotalCount = result.TotalCount;
        
        return View("~/Views/Admin/Users/Index.cshtml", result.Items);
    }

    /// <summary>
    /// Create user - GET
    /// </summary>
    [HttpGet("create")]
    public async Task<IActionResult> Create()
    {
        var roles = await _roleAppService.GetAllListAsync();
        ViewBag.AvailableRoles = roles;
        return View("~/Views/Admin/Users/Create.cshtml", new IdentityUserCreateDto());
    }

    /// <summary>
    /// Create user - POST (ABP built-in)
    /// </summary>
    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(IdentityUserCreateDto input)
    {
        if (!ModelState.IsValid)
        {
            var roles = await _roleAppService.GetAllListAsync();
            ViewBag.AvailableRoles = roles;
            return View("~/Views/Admin/Users/Create.cshtml", input);
        }

        try
        {
            var user = await _userAppService.CreateAsync(input);
            TempData["Success"] = $"User '{user.Email}' created successfully.";
            _logger.LogInformation("User created via ABP: {UserId} - {Email}", user.Id, user.Email);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            ModelState.AddModelError("", ex.Message);
            var roles = await _roleAppService.GetAllListAsync();
            ViewBag.AvailableRoles = roles;
            return View("~/Views/Admin/Users/Create.cshtml", input);
        }
    }

    /// <summary>
    /// Edit user - GET
    /// </summary>
    [HttpGet("{id}/edit")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var user = await _userAppService.GetAsync(id);
        if (user == null)
            return NotFound();

        var roles = await _roleAppService.GetAllListAsync();
        var userRoles = await _userAppService.GetRolesAsync(id);
        
        ViewBag.AvailableRoles = roles;
        ViewBag.UserRoles = userRoles.Items.Select(r => r.Name).ToList();

        var dto = new IdentityUserUpdateDto
        {
            UserName = user.UserName,
            Email = user.Email,
            Name = user.Name,
            Surname = user.Surname,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            LockoutEnabled = user.LockoutEnabled,
            RoleNames = userRoles.Items.Select(r => r.Name).ToArray()
        };

        return View("~/Views/Admin/Users/Edit.cshtml", dto);
    }

    /// <summary>
    /// Edit user - POST
    /// </summary>
    [HttpPost("{id}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, IdentityUserUpdateDto input)
    {
        if (!ModelState.IsValid)
        {
            var roles = await _roleAppService.GetAllListAsync();
            ViewBag.AvailableRoles = roles;
            return View("~/Views/Admin/Users/Edit.cshtml", input);
        }

        try
        {
            await _userAppService.UpdateAsync(id, input);
            TempData["Success"] = "User updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            ModelState.AddModelError("", ex.Message);
            var roles = await _roleAppService.GetAllListAsync();
            ViewBag.AvailableRoles = roles;
            return View("~/Views/Admin/Users/Edit.cshtml", input);
        }
    }

    /// <summary>
    /// Delete user - POST
    /// </summary>
    [HttpPost("{id}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _userAppService.DeleteAsync(id);
            TempData["Success"] = "User deleted successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// View user details
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> Details(Guid id)
    {
        var user = await _userAppService.GetAsync(id);
        if (user == null)
            return NotFound();

        var userRoles = await _userAppService.GetRolesAsync(id);
        ViewBag.UserRoles = userRoles.Items;

        return View("~/Views/Admin/Users/Details.cshtml", user);
    }

    /// <summary>
    /// Update user roles
    /// </summary>
    [HttpPost("{id}/roles")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateRoles(Guid id, [FromForm] string[] roleNames)
    {
        try
        {
            await _userAppService.UpdateRolesAsync(id, new IdentityUserUpdateRolesDto
            {
                RoleNames = roleNames
            });
            TempData["Success"] = "User roles updated successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating roles for user {UserId}", id);
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Edit), new { id });
    }
}
