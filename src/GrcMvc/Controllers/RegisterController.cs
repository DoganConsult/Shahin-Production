using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using System;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Controllers;

/// <summary>
/// Self-Registration Controller
/// Allows organizations to register themselves on the platform
/// </summary>
[Route("register")]
[AllowAnonymous]
public class RegisterController : Controller
{
    private readonly GrcDbContext _dbContext;
    private readonly ITenantService _tenantService;
    private readonly ILogger<RegisterController> _logger;

    public RegisterController(
        GrcDbContext dbContext,
        ITenantService tenantService,
        ILogger<RegisterController> logger)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _logger = logger;
    }

    /// <summary>
    /// Self-registration form
    /// GET /register
    /// </summary>
    [HttpGet]
    public IActionResult Index()
    {
        // Check if registration is enabled
        var registrationEnabled = true; // Could be config driven
        if (!registrationEnabled)
        {
            return View("RegistrationClosed");
        }

        return View(new SelfRegistrationViewModel());
    }

    /// <summary>
    /// Process self-registration
    /// POST /register
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(SelfRegistrationViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
                return View(model);

            // Validate email domain (optional)
            if (!IsValidBusinessEmail(model.AdminEmail))
            {
                ModelState.AddModelError("AdminEmail", "Please use a business email address.");
                return View(model);
            }

            // Check if tenant with this admin email already exists
            var tenantExists = await _dbContext.Tenants.AnyAsync(t => t.AdminEmail == model.AdminEmail);
            if (tenantExists)
            {
                ModelState.AddModelError("AdminEmail", "A tenant with this email already exists. Please login or use a different email.");
                return View(model);
            }

            // Generate slug
            var slug = GenerateSlug(model.OrganizationName);

            // Ensure unique slug
            var slugExists = await _dbContext.Tenants.AnyAsync(t => t.TenantSlug == slug);
            if (slugExists)
            {
                slug = $"{slug}-{DateTime.UtcNow:HHmmss}";
            }

            // Create tenant with Trial tier
            var tenant = await _tenantService.CreateTenantAsync(
                model.OrganizationName,
                model.AdminEmail,
                slug);

            // Set to Trial tier for self-registration
            tenant.SubscriptionTier = "Trial";
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Self-registration: Tenant {TenantId} ({Slug}) created by {Email}",
                tenant.Id, tenant.TenantSlug, model.AdminEmail);

            // Return success view
            return View("Success", new RegistrationSuccessViewModel
            {
                OrganizationName = model.OrganizationName,
                AdminEmail = model.AdminEmail,
                TenantSlug = tenant.TenantSlug
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during self-registration for {Email}", model.AdminEmail);
            ModelState.AddModelError("", "An error occurred during registration. Please try again.");
            return View(model);
        }
    }

    /// <summary>
    /// Registration success page
    /// </summary>
    [HttpGet("success")]
    public IActionResult Success()
    {
        return View();
    }

    /// <summary>
    /// Check slug availability (AJAX)
    /// </summary>
    [HttpGet("check-slug")]
    public async Task<IActionResult> CheckSlug(string slug)
    {
        if (string.IsNullOrEmpty(slug))
            return Json(new { available = false });

        var normalizedSlug = slug.ToLowerInvariant().Trim();
        var exists = await _dbContext.Tenants.AnyAsync(t => t.TenantSlug == normalizedSlug);

        return Json(new { available = !exists, slug = normalizedSlug });
    }

    private static bool IsValidBusinessEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
            return false;

        // Block common personal email domains for business registration
        var personalDomains = new[] { "gmail.com", "yahoo.com", "hotmail.com", "outlook.com", "aol.com", "icloud.com" };
        var domain = email.Split('@').LastOrDefault()?.ToLowerInvariant();

        // For now, allow all emails (can be made stricter)
        return !string.IsNullOrEmpty(domain);
    }

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

#region ViewModels

public class SelfRegistrationViewModel
{
    [Required(ErrorMessage = "Organization name is required")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Organization name must be between 2 and 200 characters")]
    [Display(Name = "Organization Name")]
    public string OrganizationName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [Display(Name = "Admin Email")]
    public string AdminEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Full name is required")]
    [StringLength(100, MinimumLength = 2)]
    [Display(Name = "Your Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Industry/Sector")]
    public string? Industry { get; set; }

    [Required]
    [Display(Name = "I agree to the Terms of Service")]
    public bool AcceptTerms { get; set; }
}

public class RegistrationSuccessViewModel
{
    public string OrganizationName { get; set; } = string.Empty;
    public string AdminEmail { get; set; } = string.Empty;
    public string TenantSlug { get; set; } = string.Empty;
}

#endregion
