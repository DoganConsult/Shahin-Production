using System;
using System.Threading.Tasks;
using GrcMvc.Abp;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Volo.Abp.TenantManagement;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Domain.Repositories;

using Microsoft.Extensions.Localization;
using GrcMvc.Resources;
namespace GrcMvc.Controllers.Api
{
    /// <summary>
    /// Tenant Management API - Phase 1.1
    /// POST /api/tenants - Create new tenant
    /// POST /api/auth/activate - Activate tenant
    /// GET /api/tenants/{id} - Get tenant details
    /// 
    /// NOTE: This controller uses ABP's ITenantAppService for tenant creation
    /// </summary>
    [Route("api/tenants")]
    [ApiController]
    public class TenantsApiController : ControllerBase
    {
        private readonly GrcDbContext _context;
        private readonly ITenantService _tenantService; // Legacy service (to be replaced)
        private readonly ITenantAppService _tenantAppService; // ABP service
        private readonly IIdentityUserAppService _identityUserAppService; // ABP service
        private readonly ICurrentTenant _currentTenant;
        private readonly IRepository<Volo.Abp.TenantManagement.Tenant, Guid> _tenantRepository;
        private readonly ILogger<TenantsApiController> _logger;

        public TenantsApiController(
            GrcDbContext context,
            ITenantService tenantService,
            ITenantAppService tenantAppService,
            IIdentityUserAppService identityUserAppService,
            ICurrentTenant currentTenant,
            IRepository<Volo.Abp.TenantManagement.Tenant, Guid> tenantRepository,
            ILogger<TenantsApiController> logger)
        {
            _context = context;
            _tenantService = tenantService;
            _tenantAppService = tenantAppService;
            _identityUserAppService = identityUserAppService;
            _currentTenant = currentTenant;
            _tenantRepository = tenantRepository;
            _logger = logger;
        }

        /// <summary>
        /// Create a new tenant (signup)
        /// POST /api/tenants
        /// 
        /// NOTE: If AdminPassword is provided, uses ABP's ITenantAppService to create tenant + admin automatically.
        /// Otherwise, uses legacy ITenantService (tenant only, no admin).
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreateTenant([FromBody] CreateTenantRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.OrganizationName) ||
                    string.IsNullOrEmpty(request.AdminEmail))
                {
                    return BadRequest(new { error = "OrganizationName and AdminEmail are required" });
                }

                // ✅ ABP WAY: If password provided, use ITenantAppService (creates tenant + admin)
                if (!string.IsNullOrEmpty(request.AdminPassword))
                {
                    if (request.AdminPassword.Length < 8)
                    {
                        return BadRequest(new { error = "Password must be at least 8 characters long" });
                    }

                    _logger.LogInformation("Creating tenant via ABP: Organization={OrgName}, Email={Email}",
                        request.OrganizationName, request.AdminEmail);

                    // ✅ ABP WAY: Manual flow - Create tenant first, then admin user
                    // Step 1: Create tenant
                    var tenantDto = await _tenantAppService.CreateAsync(new Volo.Abp.TenantManagement.TenantCreateDto
                    {
                        Name = request.OrganizationName
                    });

                    // Step 2: Switch to tenant context and create admin user
                    Guid adminUserId;
                    using (_currentTenant.Change(tenantDto.Id))
                    {
                        // Step 3: Create admin user in tenant context
                        var adminUser = await _identityUserAppService.CreateAsync(new Volo.Abp.Identity.IdentityUserCreateDto
                        {
                            UserName = request.AdminEmail,
                            Email = request.AdminEmail,
                            Password = request.AdminPassword,
                            RoleNames = new[] { "admin" } // ABP default admin role
                        });

                        adminUserId = adminUser.Id;

                        // Step 4: Link admin to tenant (optional, ABP handles via TenantId)
                        var tenant = await _tenantRepository.GetAsync(tenantDto.Id);
                        tenant.ExtraProperties["AdminUserId"] = adminUserId;
                        await _tenantRepository.UpdateAsync(tenant);
                    }

                    _logger.LogInformation("Tenant created via ABP: TenantId={TenantId}, AdminUserId={AdminUserId}",
                        tenantDto.Id, adminUserId);

                    return CreatedAtAction(nameof(GetTenant), new { tenantId = tenantDto.Id }, new
                    {
                        tenantId = tenantDto.Id,
                        organizationName = tenantDto.Name,
                        adminUserId = adminUserId,
                        message = "Tenant and admin user created successfully"
                    });
                }
                else
                {
                    // ⚠️ LEGACY WAY: Use custom ITenantService (tenant only, no admin)
                    // Auto-generate slug from organization name
                    var tenantSlug = GenerateSlug(request.OrganizationName);
                    
                    // Ensure unique slug
                    var existingSlug = await _context.Tenants.AnyAsync(t => t.TenantSlug == tenantSlug);
                    if (existingSlug)
                    {
                        tenantSlug = $"{tenantSlug}-{DateTime.UtcNow:HHmmss}";
                    }

                    var tenant = await _tenantService.CreateTenantAsync(
                        request.OrganizationName,
                        request.AdminEmail,
                        tenantSlug);

                    _logger.LogInformation("Tenant created (legacy): {TenantId} ({Slug})", tenant.Id, tenant.Name);

                    return CreatedAtAction(nameof(GetTenant), new { tenantId = tenant.Id }, new
                    {
                        tenantId = tenant.Id,
                        slug = tenant.Name,
                        organizationName = tenant.GetOrganizationName(),
                        status = tenant.GetStatus(),
                        activationUrl = $"/api/auth/activate?token={tenant.GetActivationToken()}",
                        message = "Tenant created. Check email for activation link."
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tenant");
                return StatusCode(500, new { error = "An internal error occurred. Please try again later." });
            }
        }

        /// <summary>
        /// Get tenant by ID
        /// GET /api/tenants/{tenantId}
        /// </summary>
        [HttpGet("{tenantId:guid}")]
        [Authorize]
        public async Task<IActionResult> GetTenant(Guid tenantId)
        {
            try
            {
                var tenant = await _context.Tenants
                    .Where(t => t.Id == tenantId)
                    .Select(t => new
                    {
                        t.Id,
                        t.TenantSlug,
                        t.OrganizationName,
                        t.Status,
                        t.SubscriptionTier,
                        t.ActivatedAt,
                        t.CreatedDate,
                        hasOrgProfile = _context.OrganizationProfiles.Any(p => p.TenantId == t.Id),
                        usersCount = _context.TenantUsers.Count(u => u.TenantId == t.Id),
                        teamsCount = _context.Teams.Count(tm => tm.TenantId == t.Id)
                    })
                    .FirstOrDefaultAsync();

                if (tenant == null)
                    return NotFound(new { error = "Tenant not found" });

                return Ok(tenant);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tenant {TenantId}", tenantId);
                return StatusCode(500, new { error = "Internal error" });
            }
        }

        /// <summary>
        /// Get tenant by slug
        /// GET /api/tenants/by-slug/{slug}
        /// </summary>
        [HttpGet("by-slug/{slug}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTenantBySlug(string slug)
        {
            try
            {
                var tenant = await _tenantService.GetTenantBySlugAsync(slug);
                if (tenant == null)
                    return NotFound(new { error = "Tenant not found" });

                return Ok(new
                {
                    tenant.Id,
                    TenantSlug = tenant.Name,
                    OrganizationName = tenant.GetOrganizationName(),
                    Status = tenant.GetStatus()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tenant by slug {Slug}", slug);
                return StatusCode(500, new { error = "Internal error" });
            }
        }

        /// <summary>
        /// List all tenants (admin only)
        /// GET /api/tenants
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,PlatformAdmin")]
        public async Task<IActionResult> ListTenants(
            [FromQuery] string? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var query = _context.Tenants.AsQueryable();

                if (!string.IsNullOrEmpty(status))
                    query = query.Where(t => t.Status == status);

                var total = await query.CountAsync();
                var tenants = await query
                    .OrderByDescending(t => t.CreatedDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(t => new
                    {
                        t.Id,
                        t.TenantSlug,
                        t.OrganizationName,
                        t.Status,
                        t.SubscriptionTier,
                        t.CreatedDate,
                        t.ActivatedAt
                    })
                    .ToListAsync();

                return Ok(new
                {
                    total,
                    page,
                    pageSize,
                    tenants
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing tenants");
                return StatusCode(500, new { error = "Internal error" });
            }
        }

        /// <summary>
        /// Update tenant settings
        /// PUT /api/tenants/{tenantId}
        /// </summary>
        [HttpPut("{tenantId:guid}")]
        [Authorize]
        public async Task<IActionResult> UpdateTenant(Guid tenantId, [FromBody] UpdateTenantRequest request)
        {
            try
            {
                var tenant = await _context.Tenants.FindAsync(tenantId);
                if (tenant == null)
                    return NotFound(new { error = "Tenant not found" });

                if (!string.IsNullOrEmpty(request.OrganizationName))
                    tenant.OrganizationName = request.OrganizationName;

                if (!string.IsNullOrEmpty(request.SubscriptionTier))
                    tenant.SubscriptionTier = request.SubscriptionTier;

                tenant.ModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Tenant updated", tenantId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tenant {TenantId}", tenantId);
                return StatusCode(500, new { error = "Internal error" });
            }
        }

        /// <summary>
        /// Get tenant org-profile
        /// GET /api/tenants/{tenantId}/org-profile
        /// </summary>
        [HttpGet("{tenantId:guid}/org-profile")]
        [Authorize]
        public async Task<IActionResult> GetOrgProfile(Guid tenantId)
        {
            try
            {
                var profile = await _context.OrganizationProfiles
                    .Where(p => p.TenantId == tenantId)
                    .FirstOrDefaultAsync();

                if (profile == null)
                    return NotFound(new { error = "Organization profile not found" });

                return Ok(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting org profile for tenant {TenantId}", tenantId);
                return StatusCode(500, new { error = "Internal error" });
            }
        }

        /// <summary>
        /// Update tenant org-profile
        /// PUT /api/tenants/{tenantId}/org-profile
        /// </summary>
        [HttpPut("{tenantId:guid}/org-profile")]
        [Authorize]
        public async Task<IActionResult> UpdateOrgProfile(Guid tenantId, [FromBody] UpdateOrgProfileRequest request)
        {
            try
            {
                var profile = await _context.OrganizationProfiles
                    .FirstOrDefaultAsync(p => p.TenantId == tenantId);

                if (profile == null)
                {
                    profile = new OrganizationProfile
                    {
                        Id = Guid.NewGuid(),
                        TenantId = tenantId,
                        CreatedDate = DateTime.UtcNow
                    };
                    _context.OrganizationProfiles.Add(profile);
                }

                // Update fields
                if (!string.IsNullOrEmpty(request.LegalEntityName))
                    profile.LegalEntityName = request.LegalEntityName;
                if (!string.IsNullOrEmpty(request.Sector))
                    profile.Sector = request.Sector;
                if (!string.IsNullOrEmpty(request.Country))
                    profile.Country = request.Country;
                if (!string.IsNullOrEmpty(request.HostingModel))
                    profile.HostingModel = request.HostingModel;

                profile.ModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Organization profile updated", profileId = profile.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating org profile for tenant {TenantId}", tenantId);
                return StatusCode(500, new { error = "Internal error" });
            }
        }
        
        /// <summary>
        /// Generate URL-safe slug from organization name
        /// </summary>
        private static string GenerateSlug(string organizationName)
        {
            return organizationName
                .ToLowerInvariant()
                .Replace(" ", "-")
                .Replace(".", "")
                .Replace(",", "")
                .Replace("'", "")
                .Replace("\"", "")
                .Replace("&", "and")
                .Replace("/", "-")
                .Replace("\\", "-")
                .Substring(0, Math.Min(organizationName.Length, 50));
        }
    }

    #region Request DTOs

    public class CreateTenantRequest
    {
        public string OrganizationName { get; set; } = string.Empty;
        public string AdminEmail { get; set; } = string.Empty;
        /// <summary>
        /// Optional: If provided, uses ABP's ITenantAppService to create tenant + admin automatically.
        /// If not provided, uses legacy ITenantService (tenant only, no admin).
        /// </summary>
        public string? AdminPassword { get; set; }
        // TenantSlug is auto-generated from OrganizationName - not required from user
    }

    public class UpdateTenantRequest
    {
        public string? OrganizationName { get; set; }
        public string? SubscriptionTier { get; set; }
    }

    public class UpdateOrgProfileRequest
    {
        public string? LegalEntityName { get; set; }
        public string? Sector { get; set; }
        public string? Country { get; set; }
        public string? HostingModel { get; set; }
    }

    #endregion
}
