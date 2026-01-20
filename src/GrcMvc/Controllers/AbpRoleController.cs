using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Identity;
using Volo.Abp.PermissionManagement;

namespace GrcMvc.Controllers;

/// <summary>
/// ABP Built-in Role Management Controller
/// Uses ABP's IIdentityRoleAppService for error-free role CRUD
/// </summary>
[Authorize(Roles = "PlatformAdmin")]
[Route("admin/roles")]
public class AbpRoleController : Controller
{
    private readonly IIdentityRoleAppService _roleAppService;
    private readonly IPermissionAppService _permissionAppService;
    private readonly ILogger<AbpRoleController> _logger;

    public AbpRoleController(
        IIdentityRoleAppService roleAppService,
        IPermissionAppService permissionAppService,
        ILogger<AbpRoleController> logger)
    {
        _roleAppService = roleAppService;
        _permissionAppService = permissionAppService;
        _logger = logger;
    }

    /// <summary>
    /// List all roles
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var roles = await _roleAppService.GetAllListAsync();
        return View("~/Views/Admin/Roles/Index.cshtml", roles);
    }

    /// <summary>
    /// Create role - GET
    /// </summary>
    [HttpGet("create")]
    public IActionResult Create()
    {
        return View("~/Views/Admin/Roles/Create.cshtml", new IdentityRoleCreateDto());
    }

    /// <summary>
    /// Create role - POST (ABP built-in)
    /// </summary>
    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(IdentityRoleCreateDto input)
    {
        if (!ModelState.IsValid)
            return View("~/Views/Admin/Roles/Create.cshtml", input);

        try
        {
            var role = await _roleAppService.CreateAsync(input);
            TempData["Success"] = $"Role '{role.Name}' created successfully.";
            _logger.LogInformation("Role created via ABP: {RoleId} - {Name}", role.Id, role.Name);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role");
            ModelState.AddModelError("", ex.Message);
            return View("~/Views/Admin/Roles/Create.cshtml", input);
        }
    }

    /// <summary>
    /// Edit role - GET
    /// </summary>
    [HttpGet("{id}/edit")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var role = await _roleAppService.GetAsync(id);
        if (role == null)
            return NotFound();

        var dto = new IdentityRoleUpdateDto
        {
            Name = role.Name,
            IsDefault = role.IsDefault,
            IsPublic = role.IsPublic
        };

        return View("~/Views/Admin/Roles/Edit.cshtml", dto);
    }

    /// <summary>
    /// Edit role - POST
    /// </summary>
    [HttpPost("{id}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, IdentityRoleUpdateDto input)
    {
        if (!ModelState.IsValid)
            return View("~/Views/Admin/Roles/Edit.cshtml", input);

        try
        {
            await _roleAppService.UpdateAsync(id, input);
            TempData["Success"] = "Role updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role {RoleId}", id);
            ModelState.AddModelError("", ex.Message);
            return View("~/Views/Admin/Roles/Edit.cshtml", input);
        }
    }

    /// <summary>
    /// Delete role - POST
    /// </summary>
    [HttpPost("{id}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _roleAppService.DeleteAsync(id);
            TempData["Success"] = "Role deleted successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role {RoleId}", id);
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Manage role permissions - GET
    /// </summary>
    [HttpGet("{id}/permissions")]
    public async Task<IActionResult> Permissions(Guid id)
    {
        var role = await _roleAppService.GetAsync(id);
        if (role == null)
            return NotFound();

        var permissions = await _permissionAppService.GetAsync("R", id.ToString());
        ViewBag.Role = role;
        
        return View("~/Views/Admin/Roles/Permissions.cshtml", permissions);
    }

    /// <summary>
    /// Update role permissions - POST
    /// </summary>
    [HttpPost("{id}/permissions")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Permissions(Guid id, [FromForm] List<string> grantedPermissions)
    {
        try
        {
            var currentPermissions = await _permissionAppService.GetAsync("R", id.ToString());
            
            var updateDto = new UpdatePermissionsDto
            {
                Permissions = currentPermissions.Groups
                    .SelectMany(g => g.Permissions)
                    .Select(p => new UpdatePermissionDto
                    {
                        Name = p.Name,
                        IsGranted = grantedPermissions.Contains(p.Name)
                    })
                    .ToArray()
            };

            await _permissionAppService.UpdateAsync("R", id.ToString(), updateDto);
            TempData["Success"] = "Permissions updated successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating permissions for role {RoleId}", id);
            TempData["Error"] = ex.Message;
        }
        
        return RedirectToAction(nameof(Permissions), new { id });
    }
}
