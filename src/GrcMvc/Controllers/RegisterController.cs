using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using GrcMvc.Helpers;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace GrcMvc.Controllers;

/// <summary>
/// Self-Registration Controller
/// Allows organizations to register themselves on the platform
/// Phase 1: Email verification + password setup flow
/// </summary>
[Route("register")]
[AllowAnonymous]
public class RegisterController : Controller
{
    private readonly GrcDbContext _dbContext;
    private readonly ITenantService _tenantService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IGrcEmailService _emailService;
    private readonly ILogger<RegisterController> _logger;
    private readonly IWebHostEnvironment _hostEnvironment;
    private readonly IConfiguration _configuration;
    private readonly ICaptchaService? _captchaService;

    public RegisterController(
        GrcDbContext dbContext,
        ITenantService tenantService,
        UserManager<ApplicationUser> userManager,
        IGrcEmailService emailService,
        ILogger<RegisterController> logger,
        IWebHostEnvironment hostEnvironment,
        IConfiguration configuration,
        ICaptchaService? captchaService = null)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _userManager = userManager;
        _emailService = emailService;
        _logger = logger;
        _hostEnvironment = hostEnvironment;
        _configuration = configuration;
        _captchaService = captchaService;
    }

    /// <summary>
    /// Self-registration form
    /// GET /register
    /// </summary>
    [HttpGet]
    public IActionResult Index()
    {
        // Check if registration is enabled (Issue #5 fix - use config)
        var registrationEnabled = _configuration.GetValue<bool>("Security:AllowPublicRegistration", true);
        if (!registrationEnabled)
        {
            return View("RegistrationClosed");
        }

        // Pass CAPTCHA site key to view if enabled
        if (_captchaService?.IsEnabled == true)
        {
            ViewBag.CaptchaSiteKey = _captchaService.GetSiteKey();
            ViewBag.CaptchaEnabled = true;
        }

        return View(new SelfRegistrationViewModel());
    }

    /// <summary>
    /// Process self-registration
    /// POST /register
    /// Creates Tenant + ApplicationUser in a transaction, sends verification email
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(SelfRegistrationViewModel model)
    {
        try
        {
            // Pass CAPTCHA info to view in case of validation errors
            if (_captchaService?.IsEnabled == true)
            {
                ViewBag.CaptchaSiteKey = _captchaService.GetSiteKey();
                ViewBag.CaptchaEnabled = true;
            }

            if (!ModelState.IsValid)
                return View(model);

            // Issue #2: Validate CAPTCHA if enabled
            if (_captchaService?.IsEnabled == true)
            {
                var captchaResponse = Request.Form["g-recaptcha-response"].ToString();
                var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();
                var isValidCaptcha = await _captchaService.ValidateCaptchaAsync(captchaResponse, remoteIp);

                if (!isValidCaptcha)
                {
                    ModelState.AddModelError("", "Please complete the CAPTCHA verification.");
                    return View(model);
                }
            }

            // Validate email domain (configurable)
            if (!IsValidBusinessEmail(model.AdminEmail))
            {
                ModelState.AddModelError("AdminEmail", "Please use a business email address.");
                return View(model);
            }

            // Normalize email for comparison
            var normalizedEmail = model.AdminEmail.ToLowerInvariant().Trim();

            // Check if tenant with this admin email already exists (Issue #3 fix - include non-deleted filter)
            var existingTenant = await _dbContext.Tenants
                .FirstOrDefaultAsync(t => t.AdminEmail.ToLower() == normalizedEmail && !t.IsDeleted);
            if (existingTenant != null)
            {
                // If they exist but haven't verified, offer to resend
                if (!existingTenant.IsEmailVerified)
                {
                    return RedirectToAction("VerificationPending", new { tenantId = existingTenant.Id });
                }
                ModelState.AddModelError("AdminEmail", "A tenant with this email already exists. Please login or use a different email.");
                return View(model);
            }

            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(model.AdminEmail);
            if (existingUser != null)
            {
                ModelState.AddModelError("AdminEmail", "An account with this email already exists. Please login or use password reset.");
                return View(model);
            }

            // Issue #3: Check if email exists in TenantUsers across all tenants
            var existingTenantUser = await _dbContext.TenantUsers
                .Include(tu => tu.User)
                .FirstOrDefaultAsync(tu => tu.User != null
                    && tu.User.Email != null
                    && tu.User.Email.ToLower() == normalizedEmail
                    && !tu.IsDeleted);
            if (existingTenantUser != null)
            {
                ModelState.AddModelError("AdminEmail",
                    "This email is already associated with an organization. Please use a different email or contact support.");
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

            // Begin transaction - Tenant + User created atomically
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // Create tenant with PendingVerification status
                var tenant = await _tenantService.CreateTenantAsync(
                    model.OrganizationName,
                    model.AdminEmail,
                    slug);

                // Set Trial tier and PendingVerification status
                tenant.SubscriptionTier = "Trial";
                tenant.Status = "PendingVerification";
                tenant.IsActive = false; // Not active until verified

                // Issue #4: Save Industry field
                if (!string.IsNullOrWhiteSpace(model.Industry))
                {
                    tenant.Industry = model.Industry;
                }

                // Generate verification token (store hash, not raw token)
                var rawToken = TokenHelper.GenerateSecureToken();
                tenant.EmailVerificationTokenHash = TokenHelper.HashToken(rawToken);
                tenant.EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(24);
                tenant.EmailVerificationLastSentAt = DateTime.UtcNow;
                tenant.IsEmailVerified = false;

                await _dbContext.SaveChangesAsync();

                // Parse full name into first/last
                var nameParts = model.FullName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                var firstName = nameParts.Length > 0 ? nameParts[0] : model.FullName;
                var lastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;

                // Create ApplicationUser (no password yet - set during verification)
                var user = new ApplicationUser
                {
                    FirstName = firstName,
                    LastName = lastName,
                    MustChangePassword = true, // Will be set false after they set password
                    CreatedDate = DateTime.UtcNow
                };

                // Create user without password (they'll set it via verification link)
                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    _logger.LogWarning("Failed to create user {Email}: {Errors}", model.AdminEmail, errors);
                    throw new InvalidOperationException($"Failed to create user account: {errors}");
                }

                // Set UserName, Email, and Phone using UserManager methods
                await _userManager.SetUserNameAsync(user, model.AdminEmail);
                await _userManager.SetEmailAsync(user, model.AdminEmail);
                if (!string.IsNullOrEmpty(model.PhoneNumber))
                {
                    await _userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
                }
                // Email will be confirmed when they verify via the link

                // Issue #16: Link user to tenant (only set if not already set - defensive)
                if (string.IsNullOrEmpty(tenant.FirstAdminUserId))
                {
                    tenant.FirstAdminUserId = user.Id.ToString();
                }

                // Issue #7: Create TenantUser relationship with PendingVerification status
                // ActivatedAt will be set after email verification in AccountController
                var tenantUser = new TenantUser
                {
                    TenantId = tenant.Id,
                    UserId = user.Id.ToString(),
                    RoleCode = "TENANT_ADMIN",
                    TitleCode = "ORGANIZATION_ADMIN",
                    Status = "PendingVerification", // Changed from "Active"
                    ActivatedAt = null // Will be set after email verification
                };
                _dbContext.TenantUsers.Add(tenantUser);

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Self-registration: Tenant {TenantId} ({Slug}) and User {UserId} created for {Email}. Verification email pending.",
                    tenant.Id, tenant.TenantSlug, user.Id, model.AdminEmail);

                // Build verification URL: /account/verify?tenantId={id}&token={rawToken}
                var verificationUrl = Url.Action(
                    "VerifyAndSetPassword",
                    "Account",
                    new { tenantId = tenant.Id, token = rawToken },
                    Request.Scheme);

                // Only log verification URL in Development environment (Issue #1 fix)
                if (_hostEnvironment.IsDevelopment())
                {
                    _logger.LogDebug("DEV MODE: Verification URL for {Email}: {Url}",
                        model.AdminEmail, verificationUrl);
                }

                // Send verification email (non-blocking - don't fail registration if email fails)
                try
                {
                    await _emailService.SendEmailConfirmationAsync(
                        model.AdminEmail,
                        model.FullName,
                        verificationUrl ?? string.Empty);

                    _logger.LogInformation("Verification email sent to {Email} for tenant {TenantId}",
                        model.AdminEmail, tenant.Id);
                }
                catch (Exception emailEx)
                {
                    // Log but don't fail - user can use resend or dev URL
                    _logger.LogWarning(emailEx,
                        "Failed to send verification email to {Email}. User can resend from verification-pending page.",
                        model.AdminEmail);
                }

                // Redirect to VerificationSent view
                return View("VerificationSent", new VerificationSentViewModel
                {
                    Email = MaskEmail(model.AdminEmail),
                    TenantId = tenant.Id
                });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during self-registration for {Email}", model.AdminEmail);
            ModelState.AddModelError("", "An error occurred during registration. Please try again.");
            return View(model);
        }
    }

    /// <summary>
    /// Show pending verification status with resend option
    /// GET /register/verification-pending
    /// </summary>
    [HttpGet("verification-pending")]
    public async Task<IActionResult> VerificationPending(Guid tenantId)
    {
        var tenant = await _dbContext.Tenants.FindAsync(tenantId);
        if (tenant == null || tenant.IsEmailVerified)
        {
            return RedirectToAction("Index");
        }

        return View("VerificationSent", new VerificationSentViewModel
        {
            Email = MaskEmail(tenant.AdminEmail),
            TenantId = tenant.Id,
            CanResend = CanResendVerification(tenant)
        });
    }

    /// <summary>
    /// Resend verification email
    /// POST /register/resend-verification
    /// </summary>
    [HttpPost("resend-verification")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendVerification(Guid tenantId)
    {
        var tenant = await _dbContext.Tenants.FindAsync(tenantId);
        if (tenant == null)
        {
            return NotFound();
        }

        if (tenant.IsEmailVerified)
        {
            return RedirectToAction("Login", "Account");
        }

        // Rate limiting: max 5 resends
        if (tenant.EmailVerificationResendCount >= 5)
        {
            TempData["Error"] = "Too many verification emails sent. Please contact support.";
            return RedirectToAction("VerificationPending", new { tenantId });
        }

        // Issue #13: Rate limiting with configurable cooldown (default 2 minutes)
        var cooldownMinutes = _configuration.GetValue<int>("Security:EmailVerificationCooldownMinutes", 2);
        var cooldownEnd = tenant.EmailVerificationLastSentAt?.AddMinutes(cooldownMinutes);
        if (cooldownEnd > DateTime.UtcNow)
        {
            var remainingSeconds = (int)(cooldownEnd.Value - DateTime.UtcNow).TotalSeconds;
            TempData["Error"] = $"Please wait {remainingSeconds} seconds before requesting another verification email.";
            return RedirectToAction("VerificationPending", new { tenantId });
        }

        // Generate new token (invalidates old one)
        var rawToken = TokenHelper.GenerateSecureToken();
        tenant.EmailVerificationTokenHash = TokenHelper.HashToken(rawToken);
        tenant.EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(24);
        tenant.EmailVerificationLastSentAt = DateTime.UtcNow;
        tenant.EmailVerificationResendCount++;

        await _dbContext.SaveChangesAsync();

        // Build verification URL
        var verificationUrl = Url.Action(
            "VerifyAndSetPassword",
            "Account",
            new { tenantId = tenant.Id, token = rawToken },
            Request.Scheme);

        // Get user for name
        var user = await _userManager.FindByEmailAsync(tenant.AdminEmail);
        var userName = user?.FullName ?? tenant.AdminEmail;

        // Send verification email
        await _emailService.SendEmailConfirmationAsync(
            tenant.AdminEmail,
            userName,
            verificationUrl ?? string.Empty);

        _logger.LogInformation(
            "Verification email resent to {Email} for tenant {TenantId}. Resend count: {Count}",
            tenant.AdminEmail, tenant.Id, tenant.EmailVerificationResendCount);

        TempData["Success"] = "Verification email sent. Please check your inbox.";
        return RedirectToAction("VerificationPending", new { tenantId });
    }

    private static bool CanResendVerification(Tenant tenant)
    {
        if (tenant.EmailVerificationResendCount >= 5)
            return false;
        if (tenant.EmailVerificationLastSentAt?.AddMinutes(1) > DateTime.UtcNow)
            return false;
        return true;
    }

    private static string MaskEmail(string email)
    {
        if (string.IsNullOrEmpty(email) || !email.Contains('@'))
            return email;

        var parts = email.Split('@');
        var local = parts[0];
        var domain = parts[1];

        if (local.Length <= 2)
            return $"{local[0]}***@{domain}";

        return $"{local[0]}{new string('*', Math.Min(local.Length - 2, 5))}{local[^1]}@{domain}";
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

    /// <summary>
    /// Issue #12: Validate business email domain (configurable)
    /// </summary>
    private bool IsValidBusinessEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
            return false;

        var domain = email.Split('@').LastOrDefault()?.ToLowerInvariant();
        if (string.IsNullOrEmpty(domain))
            return false;

        // Check if business email validation is required
        var requireBusinessEmail = _configuration.GetValue<bool>("Security:RequireBusinessEmail", false);
        if (!requireBusinessEmail)
            return true; // Skip validation if not required

        // Block common personal email domains for business registration
        var personalDomains = new[]
        {
            "gmail.com", "yahoo.com", "hotmail.com", "outlook.com",
            "aol.com", "icloud.com", "mail.com", "protonmail.com",
            "ymail.com", "live.com", "msn.com", "googlemail.com"
        };

        return !personalDomains.Contains(domain);
    }

    /// <summary>
    /// Issue #10: Generate URL-friendly slug with improved Unicode/Arabic handling
    /// </summary>
    private static string GenerateSlug(string organizationName)
    {
        if (string.IsNullOrWhiteSpace(organizationName))
            return Guid.NewGuid().ToString("N")[..8];

        // Normalize Unicode characters
        var normalized = organizationName.Normalize(NormalizationForm.FormD);

        // Remove diacritics
        var sb = new StringBuilder();
        foreach (var c in normalized)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        var slug = sb.ToString()
            .ToLowerInvariant()
            .Normalize(NormalizationForm.FormC);

        // Replace non-alphanumeric with hyphens
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"[\s-]+", "-");
        slug = slug.Trim('-');

        // Ensure minimum length
        if (slug.Length < 3)
            slug += "-" + DateTime.UtcNow.Ticks.ToString()[..6];

        return slug[..Math.Min(slug.Length, 50)];
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

    /// <summary>
    /// Issue #9: Phone number with KSA format validation
    /// </summary>
    [Display(Name = "Phone Number")]
    [RegularExpression(@"^(\+966|966|05|5)?[0-9]{8,9}$",
        ErrorMessage = "Please enter a valid Saudi phone number (e.g., +966 5X XXX XXXX)")]
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

public class VerificationSentViewModel
{
    public string Email { get; set; } = string.Empty; // Masked email (j***@company.com)
    public Guid TenantId { get; set; }
    public bool CanResend { get; set; } = true;
}

#endregion
