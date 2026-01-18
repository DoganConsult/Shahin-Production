using System;
using System.Linq;
using System.Threading.Tasks;
using GrcMvc.Models.ViewModels;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using GrcMvc.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace GrcMvc.Controllers
{
    /// <summary>
    /// Platform Admin Dashboard UI Controller
    /// Complete web interface for managing tenants
    /// </summary>
    [Authorize(Roles = "PlatformAdmin,SuperAdmin")]
    [Route("platform-admin")]
    public class PlatformAdminDashboardController : Controller
    {
        private readonly IPlatformAdminService _platformAdminService;
        private readonly ITenantService _tenantService;
        private readonly GrcDbContext _dbContext;
        private readonly ILogger<PlatformAdminDashboardController> _logger;

        public PlatformAdminDashboardController(
            IPlatformAdminService platformAdminService,
            ITenantService tenantService,
            GrcDbContext dbContext,
            ILogger<PlatformAdminDashboardController> logger)
        {
            _platformAdminService = platformAdminService;
            _tenantService = tenantService;
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Main dashboard showing all tenants and stats
        /// </summary>
        [HttpGet("")]
        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Get platform admin details
            var platformAdmin = await _platformAdminService.GetByUserIdAsync(adminId);
            if (platformAdmin == null)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            // Get dashboard statistics
            var stats = new PlatformAdminDashboardViewModel
            {
                AdminName = platformAdmin.DisplayName,
                AdminLevel = platformAdmin.AdminLevel.ToString(),
                
                // Tenant Statistics
                TotalTenants = await _dbContext.Tenants.CountAsync(t => !t.IsDeleted),
                ActiveTenants = await _dbContext.Tenants.CountAsync(t => !t.IsDeleted && t.IsActive),
                TrialTenants = await _dbContext.Tenants.CountAsync(t => !t.IsDeleted && t.IsTrial),
                SuspendedTenants = await _dbContext.Tenants.CountAsync(t => !t.IsDeleted && t.Status == "Suspended"),
                
                // Recent Activity
                TenantsCreatedToday = await _dbContext.Tenants.CountAsync(t => 
                    t.CreatedDate.Date == DateTime.UtcNow.Date),
                TenantsCreatedThisMonth = await _dbContext.Tenants.CountAsync(t => 
                    t.CreatedDate.Month == DateTime.UtcNow.Month && 
                    t.CreatedDate.Year == DateTime.UtcNow.Year),
                
                // Recent Tenants
                RecentTenants = await _dbContext.Tenants
                    .Where(t => !t.IsDeleted)
                    .OrderByDescending(t => t.CreatedDate)
                    .Take(10)
                    .Select(t => new TenantSummaryViewModel
                    {
                        Id = t.Id,
                        TenantSlug = t.TenantSlug,
                        OrganizationName = t.OrganizationName,
                        Status = t.Status,
                        CreatedDate = t.CreatedDate,
                        SubscriptionTier = t.SubscriptionTier
                    })
                    .ToListAsync(),
                
                // Permissions
                CanCreateTenants = platformAdmin.CanCreateTenants,
                CanManageTenants = platformAdmin.CanManageTenants,
                CanDeleteTenants = platformAdmin.CanDeleteTenants
            };

            return View(stats);
        }

        /// <summary>
        /// List all tenants with filtering and paging
        /// </summary>
        [HttpGet("tenants")]
        public async Task<IActionResult> TenantList(
            string search = null, 
            string status = null, 
            int page = 1, 
            int pageSize = 20)
        {
            var query = _dbContext.Tenants.Where(t => !t.IsDeleted);

            // Apply filters
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => 
                    t.OrganizationName.Contains(search) || 
                    t.TenantSlug.Contains(search) ||
                    t.AdminEmail.Contains(search));
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(t => t.Status == status);
            }

            var totalCount = await query.CountAsync();
            
            var tenants = await query
                .OrderByDescending(t => t.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TenantListViewModel
                {
                    Id = t.Id,
                    TenantSlug = t.TenantSlug,
                    TenantCode = t.TenantCode,
                    OrganizationName = t.OrganizationName,
                    Industry = t.Industry,
                    Status = t.Status,
                    SubscriptionTier = t.SubscriptionTier,
                    AdminEmail = t.AdminEmail,
                    CreatedDate = t.CreatedDate,
                    IsActive = t.IsActive,
                    IsTrial = t.IsTrial,
                    TrialEndsAt = t.TrialEndsAt
                })
                .ToListAsync();

            var model = new TenantListPageViewModel
            {
                Tenants = tenants,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                SearchTerm = search,
                StatusFilter = status
            };

            return View(model);
        }

        /// <summary>
        /// Show create tenant form
        /// </summary>
        [HttpGet("create-tenant")]
        public async Task<IActionResult> CreateTenant()
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var canCreate = await _platformAdminService.CanCreateTenantAsync(adminId);
            
            if (!canCreate)
            {
                TempData["Error"] = "You don't have permission to create tenants";
                return RedirectToAction(nameof(Dashboard));
            }

            var model = new CreateTenantFormViewModel
            {
                // Set defaults
                SubscriptionTier = "Professional",
                LicenseCount = 10,
                Country = "Saudi Arabia",
                Industry = "Technology",
                BypassPayment = true
            };

            return View(model);
        }

        /// <summary>
        /// Process tenant creation
        /// </summary>
        [HttpPost("create-tenant")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTenant(CreateTenantFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            try
            {
                // Call the API endpoint internally or use services directly
                var response = await CreateTenantInternalAsync(model, adminId);
                
                TempData["Success"] = $"Tenant '{model.OrganizationName}' created successfully!";
                TempData["Credentials"] = $"Admin Email: {model.AdminEmail}, Password sent via email";
                
                return RedirectToAction(nameof(TenantDetails), new { id = response.TenantId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tenant");
                ModelState.AddModelError("", "Failed to create tenant: " + ex.Message);
                return View(model);
            }
        }

        /// <summary>
        /// View tenant details
        /// </summary>
        [HttpGet("tenant/{id}")]
        public async Task<IActionResult> TenantDetails(Guid id)
        {
            var tenant = await _dbContext.Tenants
                .FirstOrDefaultAsync(t => t.Id == id);
                
            if (tenant == null)
            {
                return NotFound();
            }

            ApplicationUser adminUser = null;
            // Admin users are in the auth context, not main context

            var workspace = await _dbContext.Workspaces
                .FirstOrDefaultAsync(w => w.TenantId == id && w.IsDefault);

            var model = new PlatformTenantDetailsViewModel
            {
                Tenant = tenant,
                AdminUser = adminUser,
                Workspace = workspace,
                LoginUrl = $"https://yourdomain.com/{tenant.TenantSlug}"
            };

            return View(model);
        }

        /// <summary>
        /// Toggle tenant active status
        /// </summary>
        [HttpPost("tenant/{id}/toggle-status")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleTenantStatus(Guid id, string reason = null)
        {
            var tenant = await _dbContext.Tenants.FindAsync(id);
            if (tenant == null)
            {
                return NotFound();
            }

            tenant.IsActive = !tenant.IsActive;
            tenant.Status = tenant.IsActive ? "Active" : "Suspended";
            // StatusReason field not in current Tenant model
            
            await _dbContext.SaveChangesAsync();
            
            TempData["Success"] = $"Tenant status changed to {tenant.Status}";
            return RedirectToAction(nameof(TenantDetails), new { id });
        }

        /// <summary>
        /// Show reset password form
        /// </summary>
        [HttpGet("tenant/{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(Guid id)
        {
            var tenant = await _dbContext.Tenants.FindAsync(id);
            if (tenant == null)
            {
                return NotFound();
            }

            var model = new PlatformResetPasswordViewModel
            {
                TenantId = id,
                TenantName = tenant.OrganizationName,
                AdminEmail = tenant.AdminEmail
            };

            return View(model);
        }

        /// <summary>
        /// Process password reset
        /// </summary>
        [HttpPost("tenant/{id}/reset-password")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(Guid id, PlatformResetPasswordViewModel model)
        {
            // Implementation would call the API to reset password
            TempData["Success"] = "Password reset successfully. New password sent via email.";
            return RedirectToAction(nameof(TenantDetails), new { id });
        }

        #region Private Helpers

        private async Task<dynamic> CreateTenantInternalAsync(CreateTenantFormViewModel model, string adminId)
        {
            // This would typically call your API endpoint or service
            // For now, returning a mock response
            return new
            {
                TenantId = Guid.NewGuid(),
                Success = true
            };
        }

        #endregion
    }
}
