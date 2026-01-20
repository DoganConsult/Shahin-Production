using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Identity;
using Volo.Abp.TenantManagement;
using AbpTenantCreateDto = Volo.Abp.TenantManagement.TenantCreateDto;
using AbpTenantUpdateDto = Volo.Abp.TenantManagement.TenantUpdateDto;

namespace GrcMvc.Controllers;

/// <summary>
/// ABP Built-in Tenant Management Controller
/// Uses ABP's ITenantAppService for error-free tenant CRUD
/// </summary>
[Authorize(Roles = "PlatformAdmin")]
[Route("admin/tenants")]
public class AbpTenantController : Controller
{
    private readonly ITenantAppService _tenantAppService;
    private readonly IIdentityUserAppService _userAppService;
    private readonly ILogger<AbpTenantController> _logger;

    public AbpTenantController(
        ITenantAppService tenantAppService,
        IIdentityUserAppService userAppService,
        ILogger<AbpTenantController> logger)
    {
        _tenantAppService = tenantAppService;
        _userAppService = userAppService;
        _logger = logger;
    }

    /// <summary>
    /// List all tenants
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var result = await _tenantAppService.GetListAsync(new GetTenantsInput { MaxResultCount = 100 });
        return View("~/Views/Admin/Tenants/Index.cshtml", result.Items);
    }

    /// <summary>
    /// Create tenant - GET
    /// </summary>
    [HttpGet("create")]
    public IActionResult Create()
    {
        return View("~/Views/Admin/Tenants/Create.cshtml", new AbpTenantCreateDto());
    }

    /// <summary>
    /// Create tenant - POST (ABP built-in)
    /// </summary>
    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AbpTenantCreateDto input)
    {
        if (!ModelState.IsValid)
            return View("~/Views/Admin/Tenants/Create.cshtml", input);

        try
        {
            var tenant = await _tenantAppService.CreateAsync(input);
            TempData["Success"] = $"Tenant '{tenant.Name}' created successfully with admin account.";
            _logger.LogInformation("Tenant created via ABP: {TenantId} - {Name}", tenant.Id, tenant.Name);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tenant");
            ModelState.AddModelError("", ex.Message);
            return View("~/Views/Admin/Tenants/Create.cshtml", input);
        }
    }

    /// <summary>
    /// Edit tenant - GET
    /// </summary>
    [HttpGet("{id}/edit")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var tenant = await _tenantAppService.GetAsync(id);
        if (tenant == null)
            return NotFound();

        return View("~/Views/Admin/Tenants/Edit.cshtml", new AbpTenantUpdateDto { Name = tenant.Name });
    }

    /// <summary>
    /// Edit tenant - POST
    /// </summary>
    [HttpPost("{id}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, AbpTenantUpdateDto input)
    {
        if (!ModelState.IsValid)
            return View("~/Views/Admin/Tenants/Edit.cshtml", input);

        try
        {
            await _tenantAppService.UpdateAsync(id, input);
            TempData["Success"] = "Tenant updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tenant {TenantId}", id);
            ModelState.AddModelError("", ex.Message);
            return View("~/Views/Admin/Tenants/Edit.cshtml", input);
        }
    }

    /// <summary>
    /// Delete tenant - POST
    /// </summary>
    [HttpPost("{id}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _tenantAppService.DeleteAsync(id);
            TempData["Success"] = "Tenant deleted successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tenant {TenantId}", id);
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// View tenant details
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> Details(Guid id)
    {
        var tenant = await _tenantAppService.GetAsync(id);
        if (tenant == null)
            return NotFound();

        return View("~/Views/Admin/Tenants/Details.cshtml", tenant);
    }
}
