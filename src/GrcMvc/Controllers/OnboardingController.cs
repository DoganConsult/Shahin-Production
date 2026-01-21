using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GrcMvc.Abp;
using GrcMvc.Models.DTOs;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text.Json;

namespace GrcMvc.Controllers
{
    /// <summary>
    /// MVC Controller for guided onboarding UI - REDIRECTS TO WIZARD
    /// The simple onboarding views have been removed; use OnboardingWizard instead
    /// </summary>
    [Authorize]
    [Route("[controller]")]
    public class OnboardingUiController : Controller
    {
        /// <summary>
        /// Redirects to OnboardingWizard StepA (comprehensive 12-step wizard)
        /// </summary>
        [HttpGet("guided")]
        public IActionResult GuidedWelcome()
        {
            return RedirectToAction("StepA", "OnboardingWizard");
        }
    }

    /// <summary>
    /// API Controller for onboarding endpoints
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/onboarding")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [IgnoreAntiforgeryToken] // API endpoints don't require CSRF tokens
    public class OnboardingApiController : ControllerBase
    {
        private readonly ITenantService _tenantService;
        private readonly IOnboardingService _onboardingService;
        private readonly ISmartOnboardingService _smartOnboardingService;
        private readonly IRulesEngineService _rulesEngine;
        private readonly ILogger<OnboardingApiController> _logger;
        private readonly IConfiguration _configuration;

        public OnboardingApiController(
            ITenantService tenantService,
            IOnboardingService onboardingService,
            ISmartOnboardingService smartOnboardingService,
            IRulesEngineService rulesEngine,
            ILogger<OnboardingApiController> logger,
            IConfiguration configuration)
        {
            _tenantService = tenantService;
            _onboardingService = onboardingService;
            _smartOnboardingService = smartOnboardingService;
            _rulesEngine = rulesEngine;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Create a new tenant (organization signup).
        /// Sends activation email to admin.
        /// </summary>
        /// <param name="request">Organization name and admin email (slug auto-generated)</param>
        /// <returns>Tenant ID and activation URL</returns>
        [HttpPost("signup")]
        public async Task<IActionResult> SignupAsync([FromBody] CreateTenantDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.OrganizationName) ||
                    string.IsNullOrWhiteSpace(request.AdminEmail))
                {
                    return BadRequest("Organization name and admin email are required.");
                }

                // Auto-generate slug from organization name
                var tenantSlug = GenerateSlug(request.OrganizationName);

                var tenant = await _tenantService.CreateTenantAsync(
                    request.OrganizationName,
                    request.AdminEmail,
                    tenantSlug);

                return Ok(new
                {
                    tenant.Id,
                    TenantSlug = tenant.Name,
                    message = "Tenant created successfully. Activation email sent.",
                    activationUrl = $"{_configuration["App:BaseUrl"] ?? Request.Scheme + "://" + Request.Host}/auth/activate?slug={tenant.Name}&token={tenant.GetActivationToken()}"
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validation error creating tenant");
                return BadRequest(new { error = "GRC:ERROR", message = "An error occurred processing your request." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tenant");
                return StatusCode(500, new { error = "GRC:INTERNAL_ERROR", message = "An error occurred while creating the tenant. Please try again." });
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
                    return BadRequest(new { error = "GRC:VALIDATION_ERROR", message = "Tenant slug and activation token are required." });
                }

                var userId = User?.FindFirst("sub")?.Value ?? "SYSTEM";
                var tenant = await _tenantService.ActivateTenantAsync(
                    request.TenantSlug,
                    request.ActivationToken,
                    userId);

                return Ok(new
                {
                    tenant.Id,
                    Status = tenant.GetStatus(),
                    message = "Tenant activated successfully. Proceed to organizational profiling."
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validation error activating tenant");
                return BadRequest(new { error = "GRC:ERROR", message = "An error occurred processing your request." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating tenant");
                return StatusCode(500, new { error = "GRC:INTERNAL_ERROR", message = "An error occurred while activating the tenant. Please try again." });
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
                return StatusCode(500, new { error = "GRC:INTERNAL_ERROR", message = "An error occurred while saving the organization profile. Please try again." });
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
                return StatusCode(500, new { error = "GRC:INTERNAL_ERROR", message = "An error occurred while completing onboarding. Please try again." });
            }
        }

        /// <summary>
        /// Complete smart onboarding - auto-generates assessment templates and GRC plan
        /// </summary>
        [HttpPost("tenants/{tenantId:guid}/complete-smart-onboarding")]
        public async Task<IActionResult> CompleteSmartOnboardingAsync(Guid tenantId)
        {
            try
            {
                var userId = User?.FindFirst("sub")?.Value ?? "SYSTEM";
                var result = await _smartOnboardingService.CompleteSmartOnboardingAsync(tenantId, userId);

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validation error in smart onboarding for tenant {TenantId}", tenantId);
                return BadRequest(new { error = "GRC:ERROR", message = "An error occurred processing your request." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing smart onboarding for tenant {TenantId}", tenantId);
                return StatusCode(500, new { error = "GRC:INTERNAL_ERROR", message = "An error occurred during smart onboarding. Please try again." });
            }
        }

        /// <summary>
        /// Complete FULL smart onboarding - generates assessments, requirements, and team workspaces
        /// </summary>
        [HttpPost("tenants/{tenantId:guid}/complete-full-onboarding")]
        public async Task<IActionResult> CompleteFullSmartOnboardingAsync(
            Guid tenantId,
            [FromBody] FullOnboardingRequest? request = null)
        {
            try
            {
                var userId = User?.FindFirst("sub")?.Value ?? "SYSTEM";
                var result = await _smartOnboardingService.CompleteFullSmartOnboardingAsync(
                    tenantId,
                    userId,
                    request?.TeamMembers);

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validation error in full smart onboarding for tenant {TenantId}", tenantId);
                return BadRequest(new { error = "GRC:ERROR", message = "An error occurred processing your request." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing full smart onboarding for tenant {TenantId}", tenantId);
                return StatusCode(500, new { error = "GRC:INTERNAL_ERROR", message = "An error occurred during full onboarding. Please try again." });
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
                return StatusCode(500, new { error = "GRC:INTERNAL_ERROR", message = "An error occurred while fetching the scope. Please try again." });
            }
        }

        /// <summary>
        /// Refresh/re-evaluate scope when profile or assets change.
        /// Triggers rules engine to re-derive applicable baselines/packages/templates.
        /// </summary>
        [HttpPost("tenants/{tenantId:guid}/refresh-scope")]
        public async Task<IActionResult> RefreshScopeAsync(Guid tenantId, [FromBody] RefreshScopeRequest? request = null)
        {
            try
            {
                var userId = User?.FindFirst("sub")?.Value ?? "SYSTEM";
                var reason = request?.Reason ?? "Manual refresh requested";

                var executionLog = await _onboardingService.RefreshScopeAsync(tenantId, userId, reason);

                return Ok(new
                {
                    executionLog.Id,
                    executionLog.Status,
                    Reason = reason,
                    RefreshedAt = DateTime.UtcNow,
                    message = "Scope refreshed successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing scope");
                return StatusCode(500, new { error = "GRC:INTERNAL_ERROR", message = "An error occurred while refreshing the scope. Please try again." });
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
                return StatusCode(500, new { error = "GRC:INTERNAL_ERROR", message = "An error occurred while fetching the tenant. Please try again." });
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
                return StatusCode(500, new { error = "GRC:INTERNAL_ERROR", message = "An error occurred while fetching the tenant. Please try again." });
            }
        }

        // ============================================
        // Enhanced UX Endpoints for Smooth Onboarding
        // ============================================

        /// <summary>
        /// Get onboarding status and progress for a tenant.
        /// Returns step info, completion percentage, and next actions.
        /// </summary>
        [HttpGet("tenants/{tenantId:guid}/status")]
        public async Task<IActionResult> GetOnboardingStatusAsync(Guid tenantId)
        {
            try
            {
                var status = await _onboardingService.GetOnboardingStatusAsync(tenantId);
                return Ok(status);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Tenant not found: {TenantId}", tenantId);
                return NotFound(new { error = "GRC:NOT_FOUND", message = "The requested resource was not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting onboarding status for tenant {TenantId}", tenantId);
                return StatusCode(500, new { error = "GRC:INTERNAL_ERROR", message = "An error occurred while fetching onboarding status." });
            }
        }

        /// <summary>
        /// Check if tenant can proceed to a specific onboarding step.
        /// </summary>
        [HttpGet("tenants/{tenantId:guid}/can-proceed/{stepName}")]
        public async Task<IActionResult> CanProceedToStepAsync(Guid tenantId, string stepName)
        {
            try
            {
                var result = await _onboardingService.CanProceedToStepAsync(tenantId, stepName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking step access for tenant {TenantId}", tenantId);
                return StatusCode(500, new { error = "GRC:INTERNAL_ERROR", message = "An error occurred while checking step access." });
            }
        }

        /// <summary>
        /// Get recommended next step based on current progress.
        /// </summary>
        [HttpGet("tenants/{tenantId:guid}/next-step")]
        public async Task<IActionResult> GetNextStepAsync(Guid tenantId)
        {
            try
            {
                var nextStep = await _onboardingService.GetNextStepAsync(tenantId);
                return Ok(nextStep);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting next step for tenant {TenantId}", tenantId);
                return StatusCode(500, new { error = "GRC:INTERNAL_ERROR", message = "An error occurred while determining next step." });
            }
        }

        /// <summary>
        /// Resume onboarding from last saved state.
        /// </summary>
        [HttpGet("tenants/{tenantId:guid}/resume")]
        public async Task<IActionResult> ResumeOnboardingAsync(Guid tenantId)
        {
            try
            {
                var result = await _onboardingService.ResumeOnboardingAsync(tenantId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resuming onboarding for tenant {TenantId}", tenantId);
                return StatusCode(500, new { error = "GRC:INTERNAL_ERROR", message = "An error occurred while resuming onboarding." });
            }
        }

        /// <summary>
        /// Get onboarding completion percentage.
        /// </summary>
        [HttpGet("tenants/{tenantId:guid}/progress")]
        public async Task<IActionResult> GetProgressAsync(Guid tenantId)
        {
            try
            {
                var percentage = await _onboardingService.GetCompletionPercentageAsync(tenantId);
                var hasProfile = await _onboardingService.HasOrganizationProfileAsync(tenantId);

                return Ok(new
                {
                    TenantId = tenantId,
                    CompletionPercentage = percentage,
                    HasOrganizationProfile = hasProfile,
                    ProgressBar = $"{percentage}%"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting progress for tenant {TenantId}", tenantId);
                return StatusCode(500, new { error = "GRC:INTERNAL_ERROR", message = "An error occurred while fetching progress." });
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

    /// <summary>
    /// MVC Controller for onboarding pages - REDIRECTS TO WIZARD
    /// Simple onboarding views have been removed; all routes redirect to OnboardingWizard
    /// </summary>
    [Route("[controller]")]
    public class OnboardingController : Controller
    {
        private readonly ITenantService _tenantService;
        private readonly IOnboardingService _onboardingService;
        private readonly ILogger<OnboardingController> _logger;

        public OnboardingController(
            ITenantService tenantService,
            IOnboardingService onboardingService,
            ILogger<OnboardingController> logger)
        {
            _tenantService = tenantService;
            _onboardingService = onboardingService;
            _logger = logger;
        }

        /// <summary>
        /// Redirect to OnboardingWizard StepA
        /// </summary>
        [HttpGet]
        [HttpGet("Index")]
        [AllowAnonymous]
        public IActionResult Index()
        {
            return RedirectToAction("StepA", "OnboardingWizard");
        }

        /// <summary>
        /// MVC Route: Start onboarding for trial users
        /// Called after trial registration - receives tenantSlug from redirect
        /// </summary>
        [HttpGet("Start/{tenantSlug}")]
        [AllowAnonymous]
        public async Task<IActionResult> Start(string tenantSlug)
        {
            if (string.IsNullOrWhiteSpace(tenantSlug))
            {
                _logger.LogWarning("Start called with empty tenantSlug");
                return RedirectToAction("StepA", "OnboardingWizard");
            }

            try
            {
                // Get tenant by slug
                var tenant = await _tenantService.GetTenantBySlugAsync(tenantSlug);

                if (tenant == null)
                {
                    _logger.LogWarning("Tenant not found for slug: {TenantSlug}", tenantSlug);
                    TempData["ErrorMessage"] = "Organization not found. Please try again.";
                    return RedirectToAction("StepA", "OnboardingWizard");
                }

                // Store in TempData for subsequent steps
                TempData["TenantId"] = tenant.Id.ToString();
                TempData["TenantSlug"] = tenantSlug;
                TempData["OrganizationName"] = tenant.GetOrganizationName();

                _logger.LogInformation("Starting onboarding for trial tenant: {TenantSlug} ({TenantId})",
                    tenantSlug, tenant.Id);

                // Redirect to OnboardingWizard
                return RedirectToAction("StepA", "OnboardingWizard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting onboarding for tenant: {TenantSlug}", tenantSlug);
                TempData["ErrorMessage"] = "An error occurred. Please try again.";
                return RedirectToAction("StepA", "OnboardingWizard");
            }
        }

        /// <summary>
        /// Redirect to OnboardingWizard StepA (signup is part of wizard)
        /// </summary>
        [HttpGet("Signup")]
        [AllowAnonymous]
        public IActionResult Signup()
        {
            return RedirectToAction("StepA", "OnboardingWizard");
        }

        /// <summary>
        /// Redirect POST signup to wizard
        /// </summary>
        [HttpPost("Signup")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult SignupPost()
        {
            return RedirectToAction("StepA", "OnboardingWizard");
        }

        /// <summary>
        /// Redirect to OnboardingWizard (OrgProfile is covered in wizard steps)
        /// </summary>
        [HttpGet("OrgProfile")]
        [HttpGet("org-profile")]
        public IActionResult OrgProfile()
        {
            return RedirectToAction("StepB", "OnboardingWizard");
        }

        /// <summary>
        /// Redirect POST to wizard
        /// </summary>
        [HttpPost("OrgProfile")]
        [HttpPost("SaveOrgProfile")]
        [ValidateAntiForgeryToken]
        public IActionResult SaveOrgProfile()
        {
            return RedirectToAction("StepB", "OnboardingWizard");
        }

        /// <summary>
        /// Redirect to OnboardingWizard (scope review is in wizard)
        /// </summary>
        [HttpGet("ReviewScope")]
        [HttpGet("review-scope")]
        public IActionResult ReviewScope()
        {
            return RedirectToAction("StepK", "OnboardingWizard");
        }

        /// <summary>
        /// Redirect to wizard completion
        /// </summary>
        [HttpPost("CompleteOnboarding")]
        [ValidateAntiForgeryToken]
        public IActionResult CompleteOnboarding()
        {
            return RedirectToAction("Complete", "OnboardingWizard");
        }

        /// <summary>
        /// Redirect to OnboardingWizard (plan creation is in wizard)
        /// </summary>
        [HttpGet("CreatePlan")]
        [HttpGet("create-plan")]
        public IActionResult CreatePlan()
        {
            return RedirectToAction("StepL", "OnboardingWizard");
        }

        /// <summary>
        /// Redirect POST to wizard completion
        /// </summary>
        [HttpPost("CreatePlan")]
        [ValidateAntiForgeryToken]
        public IActionResult CreatePlanPost()
        {
            return RedirectToAction("Complete", "OnboardingWizard");
        }

        /// <summary>
        /// Redirect to OnboardingWizard
        /// </summary>
        [HttpGet("Activate")]
        public IActionResult Activate()
        {
            return RedirectToAction("StepA", "OnboardingWizard");
        }

        /// <summary>
        /// Redirect POST activation to wizard
        /// </summary>
        [HttpPost("Activate")]
        [ValidateAntiForgeryToken]
        public IActionResult ActivatePost()
        {
            return RedirectToAction("StepA", "OnboardingWizard");
        }
    }

    /// <summary>
    /// Request for full onboarding with team members
    /// </summary>
    public class FullOnboardingRequest
    {
        public List<TeamMemberDto>? TeamMembers { get; set; }
    }

    /// <summary>
    /// Request for scope refresh/re-evaluation
    /// </summary>
    public class RefreshScopeRequest
    {
        /// <summary>
        /// Reason for the scope refresh (e.g., "Profile updated", "New assets added", "Manual refresh")
        /// </summary>
        public string? Reason { get; set; }
    }
}
