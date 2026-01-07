using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Models.DTOs;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Controllers
{
    /// <summary>
    /// Owner/PlatformAdmin controller for tenant creation and management
    /// </summary>
    [Authorize(Roles = "PlatformAdmin,Owner")]
    [Route("owner")]
    public class OwnerController : Controller
    {
        private readonly IOwnerTenantService _ownerTenantService;
        private readonly ICredentialDeliveryService _credentialDeliveryService;
        private readonly IAuditEventService _auditService;
        private readonly GrcDbContext _context;
        private readonly ILogger<OwnerController> _logger;

        public OwnerController(
            IOwnerTenantService ownerTenantService,
            ICredentialDeliveryService credentialDeliveryService,
            IAuditEventService auditService,
            GrcDbContext context,
            ILogger<OwnerController> logger)
        {
            _ownerTenantService = ownerTenantService;
            _credentialDeliveryService = credentialDeliveryService;
            _auditService = auditService;
            _context = context;
            _logger = logger;
        }


        /// <summary>
        /// Owner dashboard
        /// </summary>
        [HttpGet]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var totalTenants = await _context.Tenants.CountAsync();
            var ownerCreatedTenants = await _context.Tenants.CountAsync(t => t.IsOwnerCreated);
            var activeTenants = await _context.Tenants.CountAsync(t => t.IsActive);
            var tenantsWithAdmin = await _context.Tenants.CountAsync(t => t.AdminAccountGenerated);

            ViewBag.TotalTenants = totalTenants;
            ViewBag.OwnerCreatedTenants = ownerCreatedTenants;
            ViewBag.ActiveTenants = activeTenants;
            ViewBag.TenantsWithAdmin = tenantsWithAdmin;

            return View();
        }

        /// <summary>
        /// List all tenants (owner-created and regular)
        /// </summary>
        [HttpGet("Tenants")]
        public async Task<IActionResult> Tenants()
        {
            var tenants = await _context.Tenants
                .OrderByDescending(t => t.CreatedDate)
                .ToListAsync();

            return View(tenants);
        }

        /// <summary>
        /// Create tenant form
        /// </summary>
        [HttpGet("Tenants/Create")]
        public IActionResult Create()
        {
            return View(new CreateOwnerTenantDto());
        }

        /// <summary>
        /// Create tenant with full features (bypass payment)
        /// </summary>
        [HttpPost("Tenants/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateOwnerTenantDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {
                var ownerId = GetCurrentOwnerId();
                if (string.IsNullOrEmpty(ownerId))
                {
                    TempData["Error"] = "Unable to identify owner. Please log in again.";
                    return RedirectToAction("Index", "Account");
                }

                var tenant = await _ownerTenantService.CreateTenantWithFullFeaturesAsync(
                    dto.OrganizationName,
                    dto.AdminEmail,
                    dto.TenantSlug,
                    ownerId,
                    dto.ExpirationDays);

                TempData["Success"] = $"Tenant '{tenant.OrganizationName}' created successfully with full features.";
                return RedirectToAction("Details", new { id = tenant.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tenant");
                ModelState.AddModelError("", ex.Message);
                return View(dto);
            }
        }

        /// <summary>
        /// Tenant details with admin account status
        /// </summary>
        [HttpGet("Tenants/{id}/Details")]
        public async Task<IActionResult> Details(Guid id)
        {
            var tenant = await _context.Tenants
                .Include(t => t.Users)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tenant == null)
            {
                return NotFound();
            }

            var ownerGeneratedUsers = await _context.TenantUsers
                .Where(tu => tu.TenantId == id && tu.IsOwnerGenerated)
                .ToListAsync();

            ViewBag.OwnerGeneratedUsers = ownerGeneratedUsers;
            ViewBag.HasExpiredCredentials = tenant.CredentialExpiresAt.HasValue && 
                                            tenant.CredentialExpiresAt.Value < DateTime.UtcNow;

            return View(tenant);
        }

        /// <summary>
        /// Generate admin account form
        /// </summary>
        [HttpGet("Tenants/{id}/GenerateAdmin")]
        public async Task<IActionResult> GenerateAdmin(Guid id)
        {
            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant == null)
            {
                return NotFound();
            }

            if (tenant.AdminAccountGenerated)
            {
                TempData["Warning"] = "Admin account already generated for this tenant.";
                return RedirectToAction("Details", new { id });
            }

            ViewBag.Tenant = tenant;
            return View(new GenerateAdminAccountDto());
        }

        /// <summary>
        /// Generate admin credentials
        /// </summary>
        [HttpPost("Tenants/{id}/GenerateAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateAdmin(Guid id, GenerateAdminAccountDto dto)
        {
            if (!ModelState.IsValid)
            {
                var tenant = await _context.Tenants.FindAsync(id);
                ViewBag.Tenant = tenant;
                return View(dto);
            }

            try
            {
                var ownerId = GetCurrentOwnerId();
                if (string.IsNullOrEmpty(ownerId))
                {
                    TempData["Error"] = "Unable to identify owner. Please log in again.";
                    return RedirectToAction("Index", "Account");
                }

                var credentials = await _ownerTenantService.GenerateTenantAdminAccountAsync(
                    id, ownerId, dto.ExpirationDays);

                // Store credentials in TempData for one-time display
                TempData["Credentials"] = System.Text.Json.JsonSerializer.Serialize(credentials);
                TempData["ShowCredentials"] = true;

                return RedirectToAction("Credentials", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating admin account for tenant {TenantId}", id);
                ModelState.AddModelError("", ex.Message);
                var tenant = await _context.Tenants.FindAsync(id);
                ViewBag.Tenant = tenant;
                return View(dto);
            }
        }

        /// <summary>
        /// View/download credentials (one-time display)
        /// </summary>
        [HttpGet("Tenants/{id}/Credentials")]
        public IActionResult Credentials(Guid id)
        {
            if (TempData["ShowCredentials"] == null || TempData["Credentials"] == null)
            {
                TempData["Error"] = "Credentials are no longer available. They can only be viewed once immediately after generation.";
                return RedirectToAction("Details", new { id });
            }

            var credentialsJson = TempData["Credentials"].ToString();
            var credentials = System.Text.Json.JsonSerializer.Deserialize<TenantAdminCredentialsDto>(credentialsJson!);

            TempData["ShowCredentials"] = null; // Clear after viewing
            TempData["Credentials"] = null;

            return View(credentials);
        }

        /// <summary>
        /// Resend credentials via email
        /// </summary>
        [HttpPost("Tenants/{id}/ResendCredentials")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendCredentials(Guid id, [FromForm] string recipientEmail)
        {
            try
            {
                var tenant = await _context.Tenants.FindAsync(id);
                if (tenant == null)
                {
                    return NotFound();
                }

                // Get the owner-generated tenant user
                var tenantUser = await _context.TenantUsers
                    .Include(tu => tu.User)
                    .FirstOrDefaultAsync(tu => tu.TenantId == id && tu.IsOwnerGenerated && tu.RoleCode == "Admin");

                if (tenantUser == null)
                {
                    TempData["Error"] = "No owner-generated admin account found for this tenant.";
                    return RedirectToAction("Details", new { id });
                }

                // Note: In a real implementation, you would need to retrieve the password
                // from a secure temporary storage or regenerate it
                // For security, we'll only send a reset link or require regeneration
                TempData["Warning"] = "For security reasons, passwords cannot be resent. Please generate new credentials or use password reset.";
                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending credentials for tenant {TenantId}", id);
                TempData["Error"] = "Error resending credentials: " + ex.Message;
                return RedirectToAction("Details", new { id });
            }
        }

        /// <summary>
        /// Extend credential expiration
        /// </summary>
        [HttpPost("Tenants/{id}/ExtendExpiration")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExtendExpiration(Guid id, ExtendExpirationDto dto)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Details", new { id });
            }

            try
            {
                var success = await _ownerTenantService.ExtendCredentialExpirationAsync(id, dto.AdditionalDays);
                if (success)
                {
                    TempData["Success"] = $"Credential expiration extended by {dto.AdditionalDays} days.";
                }
                else
                {
                    TempData["Error"] = "Failed to extend credential expiration.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extending expiration for tenant {TenantId}", id);
                TempData["Error"] = "Error extending expiration: " + ex.Message;
            }

            return RedirectToAction("Details", new { id });
        }

        /// <summary>
        /// Check tenant and credential status
        /// </summary>
        [HttpGet("Tenants/{id}/Status")]
        public async Task<IActionResult> Status(Guid id)
        {
            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant == null)
            {
                return NotFound();
            }

            var ownerGeneratedUsers = await _context.TenantUsers
                .Where(tu => tu.TenantId == id && tu.IsOwnerGenerated)
                .ToListAsync();

            var ownerTenantCreation = await _context.OwnerTenantCreations
                .FirstOrDefaultAsync(otc => otc.TenantId == id);

            ViewBag.OwnerGeneratedUsers = ownerGeneratedUsers;
            ViewBag.OwnerTenantCreation = ownerTenantCreation;
            ViewBag.HasExpiredCredentials = tenant.CredentialExpiresAt.HasValue && 
                                            tenant.CredentialExpiresAt.Value < DateTime.UtcNow;
            ViewBag.DaysUntilExpiration = tenant.CredentialExpiresAt.HasValue
                ? (int)(tenant.CredentialExpiresAt.Value - DateTime.UtcNow).TotalDays
                : (int?)null;

            return View(tenant);
        }

        /// <summary>
        /// Download credentials as PDF
        /// </summary>
        [HttpGet("Tenants/{id}/DownloadCredentials")]
        public async Task<IActionResult> DownloadCredentials(Guid id)
        {
            try
            {
                // This would typically retrieve credentials from secure storage
                // For now, return a message that credentials must be generated first
                TempData["Error"] = "Credentials must be generated first. Please generate admin account to download credentials.";
                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading credentials for tenant {TenantId}", id);
                TempData["Error"] = "Error downloading credentials: " + ex.Message;
                return RedirectToAction("Details", new { id });
            }
        }

        /// <summary>
        /// Get current owner ID from claims
        /// </summary>
        private string GetCurrentOwnerId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return string.Empty;
            }
            return userIdClaim;
        }
    }
}
