using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GrcMvc.Models.DTOs;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Controllers
{
    /// <summary>
    /// API and MVC Controller for multi-tenant onboarding workflow.
    /// Handles: tenant signup, activation, organizational profiling, and scope derivation.
    /// </summary>
    [Route("api/onboarding")]
    public class OnboardingController : Controller
    {
        private readonly ITenantService _tenantService;
        private readonly IOnboardingService _onboardingService;
        private readonly IRulesEngineService _rulesEngine;
        private readonly ILogger<OnboardingController> _logger;

        public OnboardingController(
            ITenantService tenantService,
            IOnboardingService onboardingService,
            IRulesEngineService rulesEngine,
            ILogger<OnboardingController> logger)
        {
            _tenantService = tenantService;
            _onboardingService = onboardingService;
            _rulesEngine = rulesEngine;
            _logger = logger;
        }

        /// <summary>
        /// Create a new tenant (organization signup).
        /// Sends activation email to admin.
        /// </summary>
        /// <param name="request">Organization name, admin email, and slug</param>
        /// <returns>Tenant ID and activation URL</returns>
        [HttpPost("signup")]
        public async Task<IActionResult> SignupAsync([FromBody] CreateTenantDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.OrganizationName) ||
                    string.IsNullOrWhiteSpace(request.AdminEmail) ||
                    string.IsNullOrWhiteSpace(request.TenantSlug))
                {
                    return BadRequest("Organization name, admin email, and slug are required.");
                }

                var tenant = await _tenantService.CreateTenantAsync(
                    request.OrganizationName,
                    request.AdminEmail,
                    request.TenantSlug);

                return Ok(new
                {
                    tenant.Id,
                    tenant.TenantSlug,
                    message = "Tenant created successfully. Activation email sent.",
                    activationUrl = $"{Request.Scheme}://{Request.Host}/auth/activate?slug={tenant.TenantSlug}&token={tenant.ActivationToken}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tenant");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Activate tenant after admin verifies email.
        /// </summary>
        [HttpPost("activate")]
        public async Task<IActionResult> ActivateAsync([FromBody] ActivateTenantDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.TenantSlug) ||
                    string.IsNullOrWhiteSpace(request.ActivationToken))
                {
                    return BadRequest("Tenant slug and activation token are required.");
                }

                var userId = User?.FindFirst("sub")?.Value ?? "SYSTEM";
                var tenant = await _tenantService.ActivateTenantAsync(
                    request.TenantSlug,
                    request.ActivationToken,
                    userId);

                return Ok(new
                {
                    tenant.Id,
                    tenant.Status,
                    message = "Tenant activated successfully. Proceed to organizational profiling."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating tenant");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Save organizational profile (questionnaire answers).
        /// </summary>
        [HttpPut("tenants/{tenantId:guid}/org-profile")]
        public async Task<IActionResult> SaveOrgProfileAsync(
            Guid tenantId,
            [FromBody] OrganizationProfileDto request)
        {
            try
            {
                var userId = User?.FindFirst("sub")?.Value ?? "SYSTEM";
                var profile = await _onboardingService.SaveOrganizationProfileAsync(
                    tenantId,
                    request.OrganizationType,
                    request.Sector,
                    request.Country ?? "SA",
                    request.DataTypes,
                    request.HostingModel,
                    request.Size,
                    request.Maturity,
                    request.Vendors,
                    new Dictionary<string, string>(),
                    userId);

                return Ok(new
                {
                    profile.Id,
                    profile.TenantId,
                    message = "Organization profile saved successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving organization profile");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Complete onboarding and derive scope (invoke rules engine).
        /// </summary>
        [HttpPost("tenants/{tenantId:guid}/complete-onboarding")]
        public async Task<IActionResult> CompleteOnboardingAsync(Guid tenantId)
        {
            try
            {
                var userId = User?.FindFirst("sub")?.Value ?? "SYSTEM";
                var executionLog = await _onboardingService.CompleteOnboardingAsync(tenantId, userId);

                return Ok(new
                {
                    executionLog.Id,
                    executionLog.Status,
                    message = "Onboarding completed. Scope derived using rules engine."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing onboarding");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get derived scope for tenant (applicable baselines, packages, templates).
        /// Includes ReasonJson explaining why each applies.
        /// </summary>
        [HttpGet("tenants/{tenantId:guid}/scope")]
        public async Task<IActionResult> GetScopeAsync(Guid tenantId)
        {
            try
            {
                var scope = await _onboardingService.GetDerivedScopeAsync(tenantId);
                return Ok(scope);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting scope");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get tenant by slug (used for multi-tenant routing).
        /// </summary>
        [HttpGet("tenants/by-slug/{tenantSlug}")]
        public async Task<IActionResult> GetTenantBySlugAsync(string tenantSlug)
        {
            try
            {
                var tenant = await _tenantService.GetTenantBySlugAsync(tenantSlug);
                if (tenant == null)
                {
                    return NotFound("Tenant not found or not active.");
                }

                return Ok(tenant);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tenant");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get tenant by ID.
        /// </summary>
        [HttpGet("tenants/{tenantId:guid}")]
        public async Task<IActionResult> GetTenantAsync(Guid tenantId)
        {
            try
            {
                var tenant = await _tenantService.GetTenantByIdAsync(tenantId);
                if (tenant == null)
                {
                    return NotFound("Tenant not found.");
                }

                return Ok(tenant);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tenant");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// MVC Route: Display onboarding index page
        /// </summary>
        [HttpGet("/onboarding")]
        public IActionResult Index()
        {
            return View(nameof(Index));
        }

        /// <summary>
        /// MVC Route: Display signup page
        /// </summary>
        [HttpGet("/onboarding/signup")]
        public IActionResult Signup()
        {
            return View();
        }

        /// <summary>
        /// MVC Route: Display organization profile page
        /// </summary>
        [HttpGet("/onboarding/org-profile")]
        public IActionResult OrgProfile()
        {
            return View();
        }

        /// <summary>
        /// MVC Route: Display scope review page
        /// </summary>
        [HttpGet("/onboarding/review-scope")]
        public IActionResult ReviewScope()
        {
            return View();
        }

        /// <summary>
        /// MVC Route: Display plan creation page
        /// </summary>
        [HttpGet("/onboarding/create-plan")]
        public IActionResult CreatePlan()
        {
            return View();
        }

        /// <summary>
        /// MVC Route: Display activation page
        /// </summary>
        [HttpGet("/onboarding/activate")]
        public IActionResult Activate()
        {
            return View();
        }
    }
}
