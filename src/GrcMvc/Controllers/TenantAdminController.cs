using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GrcMvc.Services.Interfaces;
using GrcMvc.Application.Permissions;
using GrcMvc.Authorization;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System;

namespace GrcMvc.Controllers
{
    /// <summary>
    /// Tenant Admin Controller - Manages tenant-level administration
    /// Requires ActiveTenantAdmin policy (role + active record verification)
    /// </summary>
    [Authorize(Policy = "ActiveTenantAdmin")]
    [RequireTenant]
    [Route("t/{tenantSlug}/admin")]
    public class TenantAdminController : Controller
    {
        private readonly ITenantService _tenantService;
        private readonly IUserManagementFacade _userManagementFacade;
        private readonly IAuditEventService _auditEventService;
        private readonly GrcDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<TenantAdminController> _logger;

        public TenantAdminController(
            ITenantService tenantService,
            IUserManagementFacade userManagementFacade,
            IAuditEventService auditEventService,
            GrcDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            ILogger<TenantAdminController> logger)
        {
            _tenantService = tenantService;
            _userManagementFacade = userManagementFacade;
            _auditEventService = auditEventService;
            _dbContext = dbContext;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Index redirects to Dashboard
        /// </summary>
        [HttpGet("")]
        [HttpGet("index")]
        public IActionResult Index(string tenantSlug)
        {
            return RedirectToAction(nameof(Dashboard), new { tenantSlug });
        }

        /// <summary>
        /// Legacy route redirect - handles /TenantAdmin requests and redirects to tenant-specific admin
        /// </summary>
        [HttpGet("/TenantAdmin")]
        [Authorize]
        public async Task<IActionResult> RedirectToTenantAdmin()
        {
            try
            {
                // Get tenant ID from claims (standard pattern in this codebase)
                var tenantIdClaim = User?.FindFirst("TenantId")?.Value;
                Guid tenantId;
                
                if (!string.IsNullOrEmpty(tenantIdClaim) && Guid.TryParse(tenantIdClaim, out tenantId))
                {
                    // Try to get tenant by ID from claim
                    var currentTenant = await _dbContext.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId);
                    if (currentTenant != null && !string.IsNullOrEmpty(currentTenant.TenantSlug))
                    {
                        return RedirectToAction(nameof(Dashboard), "TenantAdmin", new { tenantSlug = currentTenant.TenantSlug });
                    }
                }

                // Fallback: Try to get tenant from TenantUsers table via current user
                var userId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    var tenantUser = await _dbContext.Set<TenantUser>()
                        .FirstOrDefaultAsync(tu => tu.UserId == userId);
                    
                    if (tenantUser != null)
                    {
                        var currentTenant = await _dbContext.Tenants.FirstOrDefaultAsync(t => t.Id == tenantUser.TenantId);
                        if (currentTenant != null && !string.IsNullOrEmpty(currentTenant.TenantSlug))
                        {
                            return RedirectToAction(nameof(Dashboard), "TenantAdmin", new { tenantSlug = currentTenant.TenantSlug });
                        }
                    }
                }

                _logger.LogWarning("No tenant found for redirect to tenant admin. User={UserId}", userId);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to redirect to tenant admin - user may not have tenant assigned");
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Tenant Admin Dashboard
        /// </summary>
        [HttpGet("dashboard")]
        [Authorize(GrcPermissions.Admin.Access)]
        public async Task<IActionResult> Dashboard(string tenantSlug)
        {
            try
            {
                var tenant = await GetTenantBySlugAsync(tenantSlug);
                if (tenant == null) return NotFound("Tenant not found");

                ViewBag.TenantSlug = tenantSlug;
                ViewBag.Tenant = tenant;
                ViewData["Title"] = "Tenant Admin Dashboard";
                return View(tenant);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tenant dashboard");
                TempData["Error"] = "Error loading dashboard. Please try again.";
                return View();
            }
        }

        /// <summary>
        /// Manage tenant users
        /// </summary>
        [HttpGet("users")]
        [Authorize(GrcPermissions.Admin.Users)]
        public async Task<IActionResult> Users(string tenantSlug)
        {
            try
            {
                var tenant = await GetTenantBySlugAsync(tenantSlug);
                if (tenant == null) return NotFound("Tenant not found");

                ViewBag.TenantSlug = tenantSlug;
                ViewBag.Tenant = tenant;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tenant users");
                TempData["Error"] = "Error loading users. Please try again.";
                return View();
            }
        }

        /// <summary>
        /// Manage tenant roles
        /// </summary>
        [HttpGet("roles")]
        [Authorize(GrcPermissions.Admin.Roles)]
        public async Task<IActionResult> Roles(string tenantSlug)
        {
            var tenant = await GetTenantBySlugAsync(tenantSlug);
            if (tenant == null) return NotFound("Tenant not found");

            ViewBag.TenantSlug = tenantSlug;
            ViewBag.Tenant = tenant;
            ViewData["Title"] = "Tenant Roles";
            return View();
        }

        /// <summary>
        /// Tenant settings
        /// </summary>
        [HttpGet("settings")]
        [Authorize(GrcPermissions.Admin.Access)]
        public async Task<IActionResult> Settings(string tenantSlug)
        {
            try
            {
                var tenant = await GetTenantBySlugAsync(tenantSlug);
                if (tenant == null) return NotFound("Tenant not found");

                ViewBag.TenantSlug = tenantSlug;
                ViewBag.Tenant = tenant;
                return View(tenant);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tenant settings");
                TempData["Error"] = "Error loading settings. Please try again.";
                return View();
            }
        }

        /// <summary>
        /// Subscription management
        /// </summary>
        [HttpGet("subscription")]
        [Authorize(GrcPermissions.Admin.Access)]
        public async Task<IActionResult> Subscription(string tenantSlug)
        {
            try
            {
                var tenant = await GetTenantBySlugAsync(tenantSlug);
                if (tenant == null) return NotFound("Tenant not found");

                // Get real usage metrics
                var userCount = await _dbContext.TenantUsers
                    .Where(tu => tu.TenantId == tenant.Id && !tu.IsDeleted && tu.Status == "Active")
                    .CountAsync();

                var assessmentCount = await _dbContext.Assessments
                    .Where(a => a.TenantId == tenant.Id && a.DeletedAt == null)
                    .CountAsync();

                var evidenceSize = await _dbContext.Evidences
                    .Where(e => e.TenantId == tenant.Id && !e.IsDeleted)
                    .SumAsync(e => (long)e.FileSize);

                // Set limits based on subscription tier
                var (userLimit, assessmentLimit, storageLimitGb) = tenant.SubscriptionTier switch
                {
                    "MVP" => (5, 10, 5),
                    "Professional" => (25, 50, 25),
                    "Enterprise" => (1000, 1000, 100),
                    _ => (10, 20, 10)
                };

                ViewBag.TenantSlug = tenantSlug;
                ViewBag.Tenant = tenant;
                ViewBag.UserCount = userCount;
                ViewBag.UserLimit = userLimit;
                ViewBag.AssessmentCount = assessmentCount;
                ViewBag.AssessmentLimit = assessmentLimit;
                ViewBag.StorageUsedGb = Math.Round((double)evidenceSize / (1024 * 1024 * 1024), 2);
                ViewBag.StorageLimitGb = storageLimitGb;

                return View(tenant);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading subscription");
                TempData["Error"] = "Error loading subscription. Please try again.";
                return View();
            }
        }

        /// <summary>
        /// Request subscription upgrade
        /// </summary>
        [HttpPost("subscription/upgrade")]
        [Authorize(GrcPermissions.Admin.Access)]
        public async Task<IActionResult> RequestUpgrade(string tenantSlug, string targetTier)
        {
            try
            {
                var tenant = await GetTenantBySlugAsync(tenantSlug);
                if (tenant == null) return NotFound("Tenant not found");

                // Log the upgrade request
                await _auditEventService.LogEventAsync(
                    tenant.Id,
                    "SubscriptionUpgradeRequested",
                    "Tenant",
                    tenant.Id.ToString(),
                    "RequestUpgrade",
                    User.Identity?.Name ?? "Unknown",
                    System.Text.Json.JsonSerializer.Serialize(new { CurrentTier = tenant.SubscriptionTier, TargetTier = targetTier }));

                TempData["Success"] = $"Upgrade request to {targetTier} tier has been submitted. Our sales team will contact you shortly.";
                return RedirectToAction(nameof(Subscription), new { tenantSlug });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting upgrade");
                TempData["Error"] = "Error submitting upgrade request. Please try again.";
                return RedirectToAction(nameof(Subscription), new { tenantSlug });
            }
        }

        /// <summary>
        /// Audit logs for the tenant
        /// </summary>
        [HttpGet("audit-logs")]
        [Authorize(GrcPermissions.Admin.Access)]
        public async Task<IActionResult> AuditLogs(string tenantSlug, string? eventType = null, DateTime? from = null, DateTime? to = null, int page = 1)
        {
            try
            {
                var tenant = await GetTenantBySlugAsync(tenantSlug);
                if (tenant == null) return NotFound("Tenant not found");

                var pageSize = 50;
                var query = _dbContext.AuditEvents
                    .Where(e => e.TenantId == tenant.Id)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(eventType))
                    query = query.Where(e => e.EventType == eventType);

                if (from.HasValue)
                    query = query.Where(e => e.EventTimestamp >= from.Value);

                if (to.HasValue)
                    query = query.Where(e => e.EventTimestamp <= to.Value.AddDays(1));

                var totalCount = await query.CountAsync();
                var events = await query
                    .OrderByDescending(e => e.EventTimestamp)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Get distinct actor IDs and lookup user names
                var actorIds = events
                    .Where(e => !string.IsNullOrEmpty(e.Actor) && e.Actor != "SYSTEM" && Guid.TryParse(e.Actor, out _))
                    .Select(e => e.Actor)
                    .Distinct()
                    .ToList();

                var userNames = new Dictionary<string, string>();
                if (actorIds.Any())
                {
                    var users = await _userManager.Users
                        .Where(u => actorIds.Contains(u.Id))
                        .Select(u => new { u.Id, u.Email, u.FullName })
                        .ToListAsync();

                    foreach (var user in users)
                    {
                        userNames[user.Id] = !string.IsNullOrEmpty(user.FullName) ? user.FullName : user.Email;
                    }
                }

                // Get distinct event types for filter dropdown
                var eventTypes = await _dbContext.AuditEvents
                    .Where(e => e.TenantId == tenant.Id)
                    .Select(e => e.EventType)
                    .Distinct()
                    .OrderBy(e => e)
                    .ToListAsync();

                ViewBag.TenantSlug = tenantSlug;
                ViewBag.Tenant = tenant;
                ViewBag.EventTypes = eventTypes;
                ViewBag.ActorNames = userNames;
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                ViewBag.TotalCount = totalCount;

                return View(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading audit logs");
                TempData["Error"] = "Error loading audit logs. Please try again.";
                return View(new List<AuditEvent>());
            }
        }

        /// <summary>
        /// Invite a new user to the tenant
        /// </summary>
        [HttpGet("users/invite")]
        [Authorize(GrcPermissions.Admin.Users)]
        public async Task<IActionResult> InviteUser(string tenantSlug)
        {
            var tenant = await GetTenantBySlugAsync(tenantSlug);
            if (tenant == null) return NotFound("Tenant not found");

            ViewBag.TenantSlug = tenantSlug;
            ViewBag.Tenant = tenant;
            return View();
        }

        /// <summary>
        /// User details
        /// </summary>
        [HttpGet("users/{id}")]
        [Authorize(GrcPermissions.Admin.Users)]
        public async Task<IActionResult> UserDetails(string tenantSlug, string id)
        {
            var tenant = await GetTenantBySlugAsync(tenantSlug);
            if (tenant == null) return NotFound("Tenant not found");

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound("User not found");

            ViewBag.TenantSlug = tenantSlug;
            ViewBag.Tenant = tenant;
            return View(user);
        }

        private async Task<Tenant?> GetTenantBySlugAsync(string tenantSlug)
        {
            return await _dbContext.Tenants
                .FirstOrDefaultAsync(t => t.TenantSlug == tenantSlug);
        }
    }
}
