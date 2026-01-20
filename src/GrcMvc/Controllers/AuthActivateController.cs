using GrcMvc.Abp;
using GrcMvc.Models.DTOs;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrcMvc.Controllers
{
    /// <summary>
    /// Controller for handling email activation links.
    /// ASP.NET Core Best Practice: Separate controller for auth-related MVC views
    /// ABP Best Practice: Clear separation between API and MVC controllers
    /// </summary>
    [AllowAnonymous]
    public class AuthController : Controller
    {
        private readonly ITenantService _tenantService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            ITenantService tenantService,
            ILogger<AuthController> logger)
        {
            _tenantService = tenantService;
            _logger = logger;
        }

        /// <summary>
        /// Handle activation link from email.
        /// GET /auth/activate?slug=xxx&token=xxx
        /// ASP.NET Core Best Practice: Use query parameters for activation tokens
        /// </summary>
        [HttpGet("auth/activate")]
        public IActionResult Activate([FromQuery] string? slug, [FromQuery] string? token)
        {
            _logger.LogInformation("Activation page accessed for tenant: {Slug}", slug);

            var model = new ActivateTenantDto
            {
                TenantSlug = slug ?? string.Empty,
                ActivationToken = token ?? string.Empty
            };

            // If both parameters provided, show pre-filled form
            if (!string.IsNullOrEmpty(slug) && !string.IsNullOrEmpty(token))
            {
                ViewData["AutoActivate"] = true;
            }

            return View("~/Views/Onboarding/Activate.cshtml", model);
        }

        /// <summary>
        /// Process activation form submission.
        /// POST /auth/activate
        /// </summary>
        [HttpPost("auth/activate")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivatePost([FromForm] ActivateTenantDto model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Onboarding/Activate.cshtml", model);
            }

            try
            {
                var tenant = await _tenantService.ActivateTenantAsync(
                    model.TenantSlug,
                    model.ActivationToken,
                    "EMAIL_ACTIVATION");

                _logger.LogInformation("Tenant {TenantSlug} activated via email link", model.TenantSlug);

                TempData["SuccessMessage"] = "Your organization has been activated successfully! Please log in to continue.";
                
                // Redirect to login with tenant context
                return RedirectToAction("Login", "Account", new { tenantSlug = tenant.Name });
            }
            catch (KeyNotFoundException)
            {
                ModelState.AddModelError("", "Invalid tenant or activation token. Please check your email and try again.");
                _logger.LogWarning("Activation failed - tenant not found: {Slug}", model.TenantSlug);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", "An error occurred processing your request.");
                _logger.LogWarning("Activation failed - invalid state: {Message}", ex.Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred during activation. Please try again or contact support.");
                _logger.LogError(ex, "Activation error for tenant {Slug}", model.TenantSlug);
            }

            return View("~/Views/Onboarding/Activate.cshtml", model);
        }

        /// <summary>
        /// Resend activation email.
        /// POST /auth/resend-activation
        /// </summary>
        [HttpPost("auth/resend-activation")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendActivation([FromForm] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                TempData["ErrorMessage"] = "Please provide your email address.";
                return RedirectToAction("Activate");
            }

            try
            {
                // Call the tenant service to resend activation email
                var result = await _tenantService.ResendActivationEmailAsync(email);
                
                // Always show success message to prevent email enumeration
                TempData["SuccessMessage"] = "If an account exists with this email, an activation link has been sent.";
                _logger.LogInformation("Activation email resend requested for: {Email}, Result: {Result}", email, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resend activation email");
                TempData["ErrorMessage"] = "Failed to resend activation email. Please try again later.";
            }

            return RedirectToAction("Activate");
        }
    }
}
