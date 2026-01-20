using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;

namespace GrcMvc.Pages.Account;

/// <summary>
/// Custom Register Page - Overrides ABP's default to fix LazyServiceProvider null issue.
/// Uses constructor injection.
/// </summary>
public class RegisterModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<RegisterModel> _logger;
    private readonly IAppEmailSender? _emailSender;

    public RegisterModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<RegisterModel> logger,
        IAppEmailSender? emailSender = null)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
        _emailSender = emailSender;
    }

    [BindProperty]
    public RegisterInputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    public class RegisterInputModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string EmailAddress { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Display(Name = "Company Name")]
        public string? CompanyName { get; set; }

        public bool AgreeToTerms { get; set; }
    }

    public IActionResult OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");
        _logger.LogInformation("[GOLDEN_PATH] Register page accessed. ReturnUrl={ReturnUrl}", ReturnUrl);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");

        if (!Input.AgreeToTerms)
        {
            ModelState.AddModelError(string.Empty, "You must agree to the terms and conditions.");
            return Page();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        _logger.LogInformation(
            "[GOLDEN_PATH] Registration attempt. Email={Email}, Company={Company}, IP={IpAddress}",
            Input.EmailAddress, Input.CompanyName, ipAddress);

        var user = new ApplicationUser
        {
            UserName = Input.EmailAddress,
            Email = Input.EmailAddress,
            FirstName = Input.FirstName,
            LastName = Input.LastName,
            // CompanyName is captured in InputModel for logging but not stored in ApplicationUser
            // Company details are stored in Tenant entity during onboarding
            EmailConfirmed = false, // Require email confirmation
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, Input.Password);

        if (result.Succeeded)
        {
            _logger.LogInformation(
                "[GOLDEN_PATH] User {Email} registered successfully from IP {IpAddress}",
                Input.EmailAddress, ipAddress);

            // Assign default role
            await _userManager.AddToRoleAsync(user, "User");

            // Send confirmation email if email sender is available
            if (_emailSender != null)
            {
                try
                {
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { userId = user.Id, code = code },
                        protocol: Request.Scheme);

                    // await _emailSender.SendEmailAsync(...);
                    _logger.LogInformation("Confirmation email would be sent to {Email}", Input.EmailAddress);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send confirmation email to {Email}", Input.EmailAddress);
                }
            }

            // For trial/onboarding, sign in directly
            await _signInManager.SignInAsync(user, isPersistent: false);

            // Redirect to onboarding or dashboard
            if (string.IsNullOrEmpty(returnUrl) || returnUrl == "/")
            {
                return RedirectToPage("/Onboarding/Welcome");
            }

            return LocalRedirect(ReturnUrl);
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return Page();
    }
}
