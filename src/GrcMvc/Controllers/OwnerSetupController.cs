using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace GrcMvc.Controllers
{
    /// <summary>
    /// Controller for one-time owner setup (first owner registration)
    /// This is only accessible when no owner exists in the database
    /// </summary>
    [AllowAnonymous]
    public class OwnerSetupController : Controller
    {
        private readonly IOwnerSetupService _ownerSetupService;
        private readonly ILogger<OwnerSetupController> _logger;

        public OwnerSetupController(
            IOwnerSetupService ownerSetupService,
            ILogger<OwnerSetupController> logger)
        {
            _ownerSetupService = ownerSetupService;
            _logger = logger;
        }

        /// <summary>
        /// GET: OwnerSetup/Index - One-time owner registration form
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {

            // Check if owner already exists
            var ownerExists = await _ownerSetupService.OwnerExistsAsync();
            if (ownerExists)
            {
                // Show page with warning instead of redirecting (for visibility/debugging)
                ViewBag.OwnerExists = true;
                ViewBag.WarningMessage = "⚠️ WARNING: An owner account already exists. Creating another owner account is not recommended. This page should only be used for first-time setup.";
            }
            else
            {
                ViewBag.OwnerExists = false;
            }

            return View();
        }

        /// <summary>
        /// POST: OwnerSetup/Index - Create first owner account
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(OwnerSetupViewModel model)
        {

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Double-check: owner should not exist
            var ownerExists = await _ownerSetupService.OwnerExistsAsync();
            if (ownerExists)
            {
                ModelState.AddModelError(string.Empty, "Owner account already exists. Setup is only allowed once. Please log in with the existing owner account.");
                ViewBag.OwnerExists = true;
                ViewBag.WarningMessage = "⚠️ WARNING: An owner account already exists. Creating another owner account is not recommended.";
                return View(model);
            }

            // Create owner account
            var (success, errorMessage, userId) = await _ownerSetupService.CreateFirstOwnerAsync(
                model.Email,
                model.Password,
                model.FirstName,
                model.LastName,
                model.OrganizationName);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage ?? "Failed to create owner account.");
                return View(model);
            }


            TempData["Success"] = "Owner account created successfully! Please log in with your credentials.";
            return RedirectToAction("Login", "Account");
        }
    }

    /// <summary>
    /// View model for owner setup form
    /// </summary>
    public class OwnerSetupViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 2)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 2)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "Organization Name (Optional)")]
        public string? OrganizationName { get; set; }
    }
}
