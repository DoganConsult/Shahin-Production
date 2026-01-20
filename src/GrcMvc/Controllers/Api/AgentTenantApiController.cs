using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Volo.Abp.TenantManagement;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Domain.Repositories;
using System;
using System.Threading.Tasks;

namespace GrcMvc.Controllers.Api
{
    /// <summary>
    /// Agent/Bot Tenant Creation API
    /// Bot-compatible API for creating tenants and admin users via ABP services
    /// POST /api/agent/tenant/create - Create tenant + admin in one call
    /// </summary>
    [Route("api/agent/tenant")]
    [ApiController]
    [IgnoreAntiforgeryToken]
    public class AgentTenantApiController : ControllerBase
    {
        private readonly ITenantAppService _tenantAppService;
        private readonly IIdentityUserAppService _identityUserAppService;
        private readonly ICurrentTenant _currentTenant;
        private readonly IRepository<Volo.Abp.TenantManagement.Tenant, Guid> _tenantRepository;
        private readonly ILogger<AgentTenantApiController> _logger;

        public AgentTenantApiController(
            ITenantAppService tenantAppService,
            IIdentityUserAppService identityUserAppService,
            ICurrentTenant currentTenant,
            IRepository<Volo.Abp.TenantManagement.Tenant, Guid> tenantRepository,
            ILogger<AgentTenantApiController> logger)
        {
            _tenantAppService = tenantAppService;
            _identityUserAppService = identityUserAppService;
            _currentTenant = currentTenant;
            _tenantRepository = tenantRepository;
            _logger = logger;
        }

        /// <summary>
        /// Create tenant and admin user (Bot-Compatible API)
        /// POST /api/agent/tenant/create
        /// Uses ABP's ITenantAppService.CreateAsync() for one-call creation
        /// </summary>
        [HttpPost("create")]
        [AllowAnonymous] // Or use API key authentication
        public async Task<IActionResult> CreateTenantAndAdmin([FromBody] TenantCreateApiRequest request)
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(request.OrganizationName) ||
                    string.IsNullOrEmpty(request.AdminEmail) ||
                    string.IsNullOrEmpty(request.AdminPassword))
                {
                    return BadRequest(new
                    {
                        success = false,
                        error = "OrganizationName, AdminEmail, and AdminPassword are required"
                    });
                }

                // Validate password strength
                if (request.AdminPassword.Length < 8)
                {
                    return BadRequest(new
                    {
                        success = false,
                        error = "Password must be at least 8 characters long"
                    });
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
                        Name = request.FirstName ?? "",
                        Surname = request.LastName ?? "",
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

                // Return success response with redirect URL
                return Ok(new
                {
                    success = true,
                    status = "created",
                    tenantId = tenantDto.Id,
                    adminUserId = adminUserId,
                    organizationName = tenantDto.Name,
                    redirectUrl = $"/onboarding/wizard/fast-start?tenantId={tenantDto.Id}",
                    loginUrl = "/Account/Login",
                    message = "Tenant and admin user created successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tenant via ABP: Organization={OrgName}, Email={Email}",
                    request.OrganizationName, request.AdminEmail);
                return StatusCode(500, new
                {
                    success = false,
                    error = "Failed to create tenant. Please try again.",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Get tenant details by ID
        /// GET /api/agent/tenant/{tenantId}
        /// </summary>
        [HttpGet("{tenantId:guid}")]
        [AllowAnonymous] // Or use API key authentication
        public async Task<IActionResult> GetTenant(Guid tenantId)
        {
            try
            {
                var tenantDto = await _tenantAppService.GetAsync(tenantId);
                var tenant = await _tenantRepository.GetAsync(tenantId);
                var adminUserId = tenant.ExtraProperties.ContainsKey("AdminUserId") 
                    ? tenant.ExtraProperties["AdminUserId"]?.ToString() 
                    : null;
                return Ok(new
                {
                    success = true,
                    tenantId = tenantDto.Id,
                    organizationName = tenantDto.Name,
                    adminUserId = adminUserId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tenant {TenantId}", tenantId);
                return NotFound(new
                {
                    success = false,
                    error = "Tenant not found"
                });
            }
        }
    }

    #region Request DTOs

    /// <summary>
    /// Request DTO for tenant creation via API
    /// </summary>
    public class TenantCreateApiRequest
    {
        /// <summary>
        /// Organization/Company name
        /// </summary>
        public string OrganizationName { get; set; } = string.Empty;

        /// <summary>
        /// Admin user email address
        /// </summary>
        public string AdminEmail { get; set; } = string.Empty;

        /// <summary>
        /// Admin user password (min 8 characters)
        /// </summary>
        public string AdminPassword { get; set; } = string.Empty;

        /// <summary>
        /// Optional: First name
        /// </summary>
        public string? FirstName { get; set; }

        /// <summary>
        /// Optional: Last name
        /// </summary>
        public string? LastName { get; set; }
    }

    #endregion
}
