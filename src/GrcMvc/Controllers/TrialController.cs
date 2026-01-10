using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Models.ViewModels;
using GrcMvc.Services.Interfaces;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;
using GrcMvc.Constants;

namespace GrcMvc.Controllers
{
    /// <summary>
    /// Trial registration controller - simple signup flow
    /// نسخة تجريبية للتحضير للإصدار الكامل
    /// </summary>
    [Route("trial")]
    public class TrialController : Controller
    {
        private readonly GrcDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<TrialController> _logger;
        private readonly IConfiguration _configuration;

        public TrialController(
            GrcDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<TrialController> logger,
            IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Display trial registration form
        /// </summary>
        [HttpGet("")]
        [AllowAnonymous]
        public IActionResult Index()
        {
            var model = new TrialRegistrationModel();
            return View(model);
        }

        /// <summary>
        /// Process trial registration - creates user, tenant, and redirects to onboarding
        /// </summary>
        [HttpPost("")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(TrialRegistrationModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            // Check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "An account with this email already exists. Please login.");
                return View("Index", model);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Generate Tenant ID
                var tenantId = Guid.NewGuid();
                var tenantSlug = GenerateTenantSlug(model.OrganizationName);

                // Ensure unique slug
                var slugExists = await _context.Tenants.AnyAsync(t => t.TenantSlug == tenantSlug);
                if (slugExists)
                {
                    tenantSlug = $"{tenantSlug}-{DateTime.UtcNow:HHmmss}";
                }

                // 2. Create Tenant
                var tenant = new Tenant
                {
                    Id = tenantId,
                    OrganizationName = model.OrganizationName,
                    TenantSlug = tenantSlug,
                    AdminEmail = model.Email,
                    Status = "Active",
                    SubscriptionTier = "Trial",
                    IsTrial = true,
                    TrialStartsAt = DateTime.UtcNow,
                    TrialEndsAt = DateTime.UtcNow.AddDays(7),
                    BillingStatus = "Trialing",
                    IsActive = true,
                    ActivatedAt = DateTime.UtcNow,
                    SubscriptionStartDate = DateTime.UtcNow,
                    SubscriptionEndDate = DateTime.UtcNow.AddDays(7),
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "TrialRegistration"
                };
                _context.Tenants.Add(tenant);
                
                // Save tenant first so FK constraints can be satisfied
                await _context.SaveChangesAsync();

                // 3. Create User Account using Identity (saves immediately within transaction)
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    // HIGH PRIORITY SECURITY FIX: Only auto-confirm email in development environment
                    EmailConfirmed = !HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsProduction(),
                    FirstName = model.FullName.Split(' ').FirstOrDefault() ?? model.FullName,
                    LastName = model.FullName.Split(' ').Skip(1).FirstOrDefault() ?? "",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                var createResult = await _userManager.CreateAsync(user, model.Password);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to create user: {Errors}", errors);
                    ModelState.AddModelError("", $"Registration failed: {errors}");
                    return View("Index", model);
                }

                // 4. Ensure TenantAdmin role exists
                if (!await _roleManager.RoleExistsAsync("TenantAdmin"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("TenantAdmin"));
                }
                await _userManager.AddToRoleAsync(user, "TenantAdmin");

                // Reload user to ensure we have the persisted ID
                user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    throw new InvalidOperationException("User was created but could not be retrieved.");
                }

                // 5. Link User to Tenant
                var tenantUser = new TenantUser
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    UserId = user.Id,
                    RoleCode = RoleConstants.TenantAdmin,
                    TitleCode = "ADMIN",
                    Status = "Active",
                    ActivatedAt = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow
                };
                _context.TenantUsers.Add(tenantUser);

                // 6. Save trial request record
                var trialRequest = new TrialRequest
                {
                    Id = Guid.NewGuid(),
                    OrganizationName = model.OrganizationName,
                    AdminName = model.FullName,
                    AdminEmail = model.Email,
                    Status = "Provisioned",
                    RequestedAt = DateTime.UtcNow,
                    TermsAcceptedAt = DateTime.UtcNow,
                    ProvisionedTenantId = tenantId,
                    Source = "Web",
                    RequestIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    CreatedDate = DateTime.UtcNow
                };
                _context.TrialRequests.Add(trialRequest);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Sign in the new user
                await _signInManager.SignInAsync(user, isPersistent: true);

                _logger.LogInformation("Trial registered: TenantId={TenantId}, Email={Email}", tenantId, model.Email);

                // Redirect to onboarding with tenant context
                return RedirectToAction("Start", "Onboarding", new { tenantSlug = tenantSlug });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Trial registration failed");
                ModelState.AddModelError("", "Registration failed. Please try again.");
                return View("Index", model);
            }
        }

        private string GenerateTenantSlug(string organizationName)
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

        /// <summary>
        /// Handle demo request from landing page
        /// </summary>
        [HttpPost("demo-request")]
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> DemoRequest([FromBody] DemoRequestModel model)
        {
            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Name))
            {
                return BadRequest(new { error = "Name and email are required" });
            }

            try
            {
                // Save demo request to database
                var request = new TrialRequest
                {
                    Id = Guid.NewGuid(),
                    AdminName = model.Name,
                    AdminEmail = model.Email,
                    OrganizationName = model.Company ?? "",
                    Phone = model.Phone,
                    Source = "Demo Request",
                    Status = "Submitted",
                    Notes = $"Employees: {model.Employees}",
                    RequestedAt = DateTime.UtcNow
                };

                _context.TrialRequests.Add(request);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Demo request received from {Email} - {Company}", model.Email, model.Company);

                return Ok(new { success = true, message = "Demo request received" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save demo request");
                return StatusCode(500, new { error = "Failed to save request" });
            }
        }
    }

    public class DemoRequestModel
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Company { get; set; }
        public string? Phone { get; set; }
        public string? Employees { get; set; }
        public string? Type { get; set; }
    }

    /// <summary>
    /// Simple trial registration model
    /// </summary>
    public class TrialRegistrationModel
    {
        [Required(ErrorMessage = "Organization name is required")]
        [Display(Name = "Organization Name")]
        public string OrganizationName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full name is required")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 12, ErrorMessage = "Password must be at least 12 characters")]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Phone")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string? Phone { get; set; }

        [Display(Name = "Interested Features")]
        public List<string>? Features { get; set; }

        [Required(ErrorMessage = "You must accept the terms")]
        [Display(Name = "I accept the Terms and Conditions")]
        public bool AcceptTerms { get; set; }
    }

    public class TrialSuccessViewModel
    {
        public Guid TenantId { get; set; }
        public string TenantSlug { get; set; } = string.Empty;
        public string OrganizationName { get; set; } = string.Empty;
        public DateTime TrialEndsAt { get; set; }
        public string? OnboardingUrl { get; set; }
    }
}
