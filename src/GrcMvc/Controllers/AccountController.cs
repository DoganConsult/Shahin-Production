using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using GrcMvc.Models.Entities;
using GrcMvc.Models.ViewModels;
using GrcMvc.Services.Interfaces;
using GrcMvc.Services;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System;
using System.Linq;
using GrcMvc.Data;
using GrcMvc.Constants;

namespace GrcMvc.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly IAppEmailSender _emailSender;
        private readonly IGrcEmailService _grcEmailService;
        private readonly ILogger<AccountController> _logger;
        private readonly ITenantService _tenantService;
        private readonly GrcDbContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            IAppEmailSender emailSender,
            IGrcEmailService grcEmailService,
            ILogger<AccountController> logger,
            ITenantService tenantService,
            GrcDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _emailSender = emailSender;
            _grcEmailService = grcEmailService;
            _logger = logger;
            _tenantService = tenantService;
            _context = context;
        }

        // GET: Account/Login
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User {Email} logged in successfully.", model.Email);

                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user != null)
                    {
                        // Check if user must change password (first login or admin reset)
                        if (user.MustChangePassword)
                        {
                            _logger.LogInformation("User {Email} must change password on first login", model.Email);
                            return RedirectToAction(nameof(ChangePasswordRequired));
                        }

                        // Process post-login logic (add tenant claim, check onboarding, determine redirect)
                        return await ProcessPostLoginAsync(user, model.RememberMe);
                    }

                    // Redirect to role-based dashboard
                    return RedirectToAction(nameof(LoginRedirect));
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User {Email} account locked out.", model.Email);
                    
                    // Send account locked notification
                    try
                    {
                        var lockedUser = await _userManager.FindByEmailAsync(model.Email);
                        if (lockedUser != null)
                        {
                            var lockoutEnd = await _userManager.GetLockoutEndDateAsync(lockedUser);
                            var unlockTime = lockoutEnd?.ToString("yyyy-MM-dd HH:mm") ?? "قريباً";
                            var userName = lockedUser.FullName ?? lockedUser.UserName ?? model.Email.Split('@')[0];
                            await _grcEmailService.SendAccountLockedNotificationAsync(model.Email, userName, unlockTime, isArabic: true);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to send account locked notification to {Email}", model.Email);
                    }
                    
                    return View("Lockout");
                }

                if (result.RequiresTwoFactor)
                {
                    return RedirectToAction(nameof(LoginWith2fa), new { returnUrl, model.RememberMe });
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }

        /// <summary>
        /// Process post-login logic: add tenant claim, check onboarding, determine redirect
        /// Called after successful login OR after password change
        /// </summary>
        private async Task<IActionResult> ProcessPostLoginAsync(ApplicationUser user, bool rememberMe)
        {
            // Update last login date
            user.LastLoginDate = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // Get tenant user record
            var tenantUser = await _context.TenantUsers
                .Include(tu => tu.Tenant)
                .FirstOrDefaultAsync(tu => tu.UserId == user.Id && !tu.IsDeleted);

            if (tenantUser?.TenantId == null)
            {
                // No tenant - go to dashboard (or appropriate default)
                return RedirectToAction(nameof(LoginRedirect));
            }

            // Add TenantId claim if tenant exists
            var existingClaims = await _userManager.GetClaimsAsync(user);
            var hasTenantClaim = existingClaims.Any(c => c.Type == "TenantId");

            if (!hasTenantClaim)
            {
                var claims = new List<Claim>
                {
                    new Claim("TenantId", tenantUser.TenantId.ToString())
                };
                await _userManager.AddClaimsAsync(user, claims);
                _logger.LogDebug("Added TenantId claim for user {Email}", user.Email);
            }

            // Re-sign in to include claims in cookie
            await _signInManager.SignInAsync(user, rememberMe);

            // Get tenant
            var tenant = tenantUser.Tenant ?? 
                await _context.Tenants.FirstOrDefaultAsync(t => t.Id == tenantUser.TenantId);
            
            if (tenant == null)
            {
                return RedirectToAction(nameof(LoginRedirect));
            }

            _logger.LogInformation("User {Email} - TenantId: {TenantId}, OnboardingStatus: {Status}, Role: {Role}", 
                user.Email, tenant.Id, tenant.OnboardingStatus, tenantUser.RoleCode);

            // Check if user is admin (using standardized helper)
            bool isAdmin = RoleConstants.IsTenantAdmin(tenantUser.RoleCode);

            // For admin users: prioritize onboarding redirect if incomplete
            if (isAdmin && !OnboardingStatus.IsCompleted(tenant.OnboardingStatus))
            {
                _logger.LogInformation("Admin user {Email} - Direct redirect to onboarding (TenantId: {TenantId})", 
                    user.Email, tenant.Id);
                return RedirectToAction("Index", "OnboardingWizard", new { tenantId = tenant.Id });
            }

            // For all users: check onboarding status
            if (!OnboardingStatus.IsCompleted(tenant.OnboardingStatus))
            {
                _logger.LogInformation("Redirecting user {Email} to onboarding wizard (status: {Status})", 
                    user.Email, tenant.OnboardingStatus);
                return RedirectToAction("Index", "OnboardingWizard", new { tenantId = tenant.Id });
            }

            // All checks passed - go to role-based dashboard
            return RedirectToAction(nameof(LoginRedirect));
        }

        /// <summary>
        /// Role-based redirect after successful login
        /// </summary>
        [Authorize]
        public async Task<IActionResult> LoginRedirect([FromServices] IPostLoginRoutingService routingService)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            // Use centralized routing service for role-based redirection
            var (controller, action, routeValues) = await routingService.GetRouteForUserAsync(user);

            _logger.LogInformation("Redirecting user {Email} to {Controller}/{Action}",
                user.Email, controller, action);

            return RedirectToAction(action, controller, routeValues);
        }

        // GET: Account/DemoLogin - Auto-login with demo credentials
        [AllowAnonymous]
        public async Task<IActionResult> DemoLogin()
        {
            // Check if demo login is disabled in production
            var disableDemoLogin = _configuration.GetValue<bool>("GrcFeatureFlags:DisableDemoLogin");
            if (disableDemoLogin)
            {
                _logger.LogWarning("Demo login is disabled in production");
                TempData["ErrorMessage"] = "Demo login is disabled.";
                return RedirectToAction(nameof(Login));
            }

            // Demo credentials from configuration (not hard-coded)
            var demoEmail = _configuration["Demo:Email"] ?? "support@shahin-ai.com";
            var demoPassword = _configuration["Demo:Password"];

            if (string.IsNullOrEmpty(demoPassword))
            {
                _logger.LogWarning("Demo password not configured");
                TempData["ErrorMessage"] = "Demo account is not configured.";
                return RedirectToAction(nameof(Login));
            }

            var result = await _signInManager.PasswordSignInAsync(
                demoEmail,
                demoPassword,
                isPersistent: false,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _logger.LogInformation("Demo login successful for {Email}", demoEmail);
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            _logger.LogWarning("Demo login failed for {Email}", demoEmail);
            TempData["ErrorMessage"] = "Demo account is not available. Please contact support.";
            return RedirectToAction(nameof(Login));
        }

        // GET: Account/Register
        [AllowAnonymous]
        public IActionResult Register(string? returnUrl = null)
        {
            // Check if public registration is enabled
            var allowPublicRegistration = _configuration.GetValue<bool>("Security:AllowPublicRegistration", false);
            if (!allowPublicRegistration)
            {
                TempData["ErrorMessage"] = "Public registration is disabled. Please contact your administrator for an invitation.";
                return RedirectToAction(nameof(Login));
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            // Check if public registration is enabled
            var allowPublicRegistration = _configuration.GetValue<bool>("Security:AllowPublicRegistration", false);
            if (!allowPublicRegistration)
            {
                TempData["ErrorMessage"] = "Public registration is disabled. Please contact your administrator for an invitation.";
                return RedirectToAction(nameof(Login));
            }

            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName ?? string.Empty,
                    LastName = model.LastName ?? string.Empty,
                    Department = model.Department ?? string.Empty,
                    EmailConfirmed = true // For development, auto-confirm email
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User {Email} created a new account.", model.Email);

                    // Assign default role
                    var defaultRole = "User";
                    if (!await _roleManager.RoleExistsAsync(defaultRole))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(defaultRole));
                    }
                    await _userManager.AddToRoleAsync(user, defaultRole);

                    // Send welcome email
                    try
                    {
                        var loginUrl = Url.Action("Login", "Account", null, Request.Scheme);
                        var userName = $"{model.FirstName} {model.LastName}".Trim();
                        if (string.IsNullOrEmpty(userName)) userName = model.Email.Split('@')[0];
                        
                        await _grcEmailService.SendWelcomeEmailAsync(
                            model.Email,
                            userName,
                            loginUrl ?? "https://portal.shahin-ai.com",
                            "Shahin AI GRC",
                            isArabic: true);
                        _logger.LogInformation("Welcome email sent to {Email}", model.Email);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to send welcome email to {Email}", model.Email);
                    }

                    await _signInManager.SignInAsync(user, isPersistent: false);

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction(nameof(HomeController.Index), "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // POST: Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        // GET: Account/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }

        // GET: Account/ChangePasswordRequired - Force password change on first login
        [Authorize]
        public IActionResult ChangePasswordRequired()
        {
            return View(new ChangePasswordRequiredViewModel());
        }

        // POST: Account/ChangePasswordRequired
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePasswordRequired(ChangePasswordRequiredViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Verify current password
            var passwordCheck = await _userManager.CheckPasswordAsync(user, model.CurrentPassword);
            if (!passwordCheck)
            {
                ModelState.AddModelError(nameof(model.CurrentPassword), "Current password is incorrect.");
                return View(model);
            }

            // Change password
            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                // Clear the must change password flag
                user.MustChangePassword = false;
                user.LastPasswordChangedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                _logger.LogInformation("User {Email} changed password on first login", user.Email);
                TempData["SuccessMessage"] = "Password changed successfully. Welcome!";

                // Process post-login logic (add tenant claim, check onboarding)
                return await ProcessPostLoginAsync(user, rememberMe: false);
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // GET: Account/Manage
        [Authorize]
        public async Task<IActionResult> Manage()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var model = new ManageViewModel
            {
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                Department = user.Department ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty
            };

            return View(model);
        }

        // POST: Account/Manage
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Manage(ManageViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            user.FirstName = model.FirstName ?? string.Empty;
            user.LastName = model.LastName ?? string.Empty;
            user.Department = model.Department ?? string.Empty;
            user.PhoneNumber = model.PhoneNumber ?? string.Empty;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            TempData["SuccessMessage"] = "Your profile has been updated.";
            return RedirectToAction(nameof(Manage));
        }

        // GET: Account/ChangePassword
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        // POST: Account/ChangePassword
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(
                user, model.CurrentPassword, model.NewPassword);

            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);
            _logger.LogInformation("User changed their password successfully.");

            TempData["SuccessMessage"] = "Your password has been changed.";
            return RedirectToAction(nameof(Manage));
        }

        // GET: Account/LoginWith2fa
        [AllowAnonymous]
        public async Task<IActionResult> LoginWith2fa(bool rememberMe, string? returnUrl = null)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException("Unable to load two-factor authentication user.");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginWith2faViewModel { RememberMe = rememberMe });
        }

        // POST: Account/LoginWith2fa
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWith2fa(LoginWith2faViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException("Unable to load two-factor authentication user.");
            }

            var authenticatorCode = model.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);
            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(
                authenticatorCode, model.RememberMe, model.RememberMachine);

            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID {UserId} logged in with 2fa.", user.Id);
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User with ID {UserId} account locked out.", user.Id);
                return View("Lockout");
            }

            ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
            return View(model);
        }

        // GET: Account/LoginWithRecoveryCode
        [AllowAnonymous]
        public async Task<IActionResult> LoginWithRecoveryCode(string? returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException("Unable to load two-factor authentication user.");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: Account/LoginWithRecoveryCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWithRecoveryCode(LoginWithRecoveryCodeViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException("Unable to load two-factor authentication user.");
            }

            var recoveryCode = model.RecoveryCode.Replace(" ", string.Empty);

            var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID {UserId} logged in with a recovery code.", user.Id);
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User with ID {UserId} account locked out.", user.Id);
                return View("Lockout");
            }

            ModelState.AddModelError(string.Empty, "Invalid recovery code entered.");
            return View(model);
        }

        // GET: Account/ForgotPassword
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    // Don't reveal that the user does not exist
                    _logger.LogWarning("Password reset requested for non-existent email: {Email}", model.Email);
                    return RedirectToAction(nameof(ForgotPasswordConfirmation));
                }

                _logger.LogInformation("Generating password reset token for user: {Email}", model.Email);
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Action(nameof(ResetPassword), "Account", new { code }, protocol: HttpContext.Request.Scheme);
                
                _logger.LogInformation("Password reset link generated: {Url}", callbackUrl);
                
                // Use templated email with Arabic support
                var userName = user.FullName ?? user.UserName ?? user.Email?.Split('@')[0] ?? "المستخدم";
                await _grcEmailService.SendPasswordResetEmailAsync(
                    model.Email, 
                    userName, 
                    callbackUrl ?? "#", 
                    isArabic: true);

                _logger.LogInformation("Password reset email sent to {Email}", model.Email);
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            return View(model);
        }

        // GET: Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        // GET: Account/ResetPassword
        [AllowAnonymous]
        public IActionResult ResetPassword(string? code = null)
        {
            if (code == null)
            {
                return BadRequest("A code must be supplied for password reset.");
            }
            else
            {
                var model = new ResetPasswordViewModel
                {
                    Code = code
                };
                return View(model);
            }
        }

        // POST: Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                // Send password changed notification
                try
                {
                    var userName = user.FullName ?? user.UserName ?? user.Email?.Split('@')[0] ?? "المستخدم";
                    await _grcEmailService.SendPasswordChangedNotificationAsync(user.Email!, userName, isArabic: true);
                    _logger.LogInformation("Password changed notification sent to {Email}", user.Email);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send password changed notification to {Email}", user.Email);
                }
                
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        // GET: Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        // GET: Account/Lockout
        [AllowAnonymous]
        public IActionResult Lockout()
        {
            return View();
        }

        // API endpoint for JWT token generation (for API access)
        [HttpPost]
        [AllowAnonymous]
        [Route("api/account/token")]
        public async Task<IActionResult> GenerateToken([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id?.ToString() ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty)
            }.Union(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var jwtSecret = _configuration["JwtSettings:Secret"];
            if (string.IsNullOrEmpty(jwtSecret))
            {
                _logger.LogError("JWT Secret is not configured");
                return StatusCode(500, new { message = "Server configuration error" });
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(Convert.ToDouble(
                    _configuration["JwtSettings:ExpirationInHours"] ?? "24")),
                signingCredentials: creds
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo
            });
        }

        // GET: Account/Profile
        [Authorize]
        public async Task<IActionResult> Profile(
            [FromServices] IUserProfileService profileService,
            [FromServices] ITenantContextService tenantContext,
            [FromServices] INotificationService notificationService,
            [FromServices] Data.GrcDbContext dbContext)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var userId = user.Id;
            var tenantId = tenantContext.GetCurrentTenantId();

            // Get user's tenant info
            var tenantUser = await dbContext.TenantUsers
                .FirstOrDefaultAsync(tu => tu.UserId == userId && tu.TenantId == tenantId && !tu.IsDeleted);

            // Get assigned profiles
            var assignments = await profileService.GetUserAssignmentsAsync(userId, tenantId);
            var permissions = await profileService.GetUserPermissionsAsync(userId, tenantId);
            var workflowRoles = await profileService.GetUserWorkflowRolesAsync(userId, tenantId);

            // Get notification preferences
            var notifPrefs = await notificationService.GetUserPreferencesAsync(userId, tenantId);

            // Get pending tasks count
            var pendingTasks = await dbContext.WorkflowTasks
                .CountAsync(t => t.AssignedToUserId.ToString() == userId &&
                    t.Status == "Pending" && !t.IsDeleted);

            var model = new UserProfileViewModel
            {
                UserId = userId,
                Email = user.Email ?? "",
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                RoleName = tenantUser?.RoleCode ?? "",
                TitleName = tenantUser?.TitleCode ?? "",
                PendingTasksCount = pendingTasks,
                AssignedProfiles = assignments.Select(a => new UserProfileInfo
                {
                    ProfileId = a.UserProfileId,
                    ProfileCode = a.UserProfile?.ProfileCode ?? "",
                    ProfileName = a.UserProfile?.ProfileName ?? "",
                    Description = a.UserProfile?.Description ?? "",
                    Category = a.UserProfile?.Category ?? ""
                }).ToList(),
                Permissions = permissions,
                WorkflowRoles = workflowRoles,
                NotificationPreferences = notifPrefs != null ? new NotificationPreferencesInfo
                {
                    EmailEnabled = notifPrefs.EmailEnabled,
                    SmsEnabled = notifPrefs.SmsEnabled,
                    InAppEnabled = notifPrefs.InAppEnabled,
                    DigestFrequency = notifPrefs.DigestFrequency
                } : new NotificationPreferencesInfo()
            };

            return View(model);
        }

        // POST: Account/UpdateNotificationPreferences
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateNotificationPreferences(
            [FromServices] INotificationService notificationService,
            [FromServices] ITenantContextService tenantContext,
            bool EmailEnabled = true,
            bool SmsEnabled = false,
            bool InAppEnabled = true,
            string DigestFrequency = "Immediate")
        {
            var userId = _userManager.GetUserId(User);
            var tenantId = tenantContext.GetCurrentTenantId();

            if (!string.IsNullOrEmpty(userId))
            {
                await notificationService.UpdatePreferencesAsync(userId, tenantId, EmailEnabled, SmsEnabled);
            }

            TempData["Success"] = "Notification preferences updated.";
            return RedirectToAction(nameof(Profile));
        }

        // GET: Account/TenantAdminLogin
        [HttpGet("TenantAdminLogin")]
        [AllowAnonymous]
        public IActionResult TenantAdminLogin(string? returnUrl = null, Guid? tenantId = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new TenantAdminLoginViewModel
            {
                TenantId = tenantId ?? Guid.Empty,
                ReturnUrl = returnUrl
            });
        }

        // POST: Account/TenantAdminLogin
        [HttpPost("TenantAdminLogin")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TenantAdminLogin(
            TenantAdminLoginViewModel model,
            string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Validate tenant exists
                var tenant = await _tenantService.GetTenantByIdAsync(model.TenantId);
                if (tenant == null)
                {
                    ModelState.AddModelError("", "Invalid Tenant ID");
                    return View(model);
                }

                // Find user by username
                var user = await _userManager.FindByNameAsync(model.Username);
                if (user == null)
                {
                    ModelState.AddModelError("", "Invalid username or password");
                    return View(model);
                }

                // Check TenantUser exists and is Admin
                var tenantUser = await _context.TenantUsers
                    .FirstOrDefaultAsync(tu => tu.TenantId == model.TenantId && tu.UserId == user.Id);

                if (tenantUser == null || tenantUser.RoleCode != "Admin" || tenantUser.Status != "Active")
                {
                    ModelState.AddModelError("", "Invalid credentials for this tenant");
                    return View(model);
                }

                // Check credential expiration if owner-generated
                if (tenantUser.IsOwnerGenerated && tenantUser.CredentialExpiresAt.HasValue)
                {
                    if (tenantUser.CredentialExpiresAt.Value < DateTime.UtcNow)
                    {
                        ModelState.AddModelError("", "Your credentials have expired. Please contact the system owner.");
                        return View(model);
                    }
                }

                // Verify password
                var result = await _signInManager.PasswordSignInAsync(
                    model.Username,
                    model.Password,
                    isPersistent: false,
                    lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    // Add tenant claim
                    var claims = new List<Claim> { new Claim("TenantId", model.TenantId.ToString()) };
                    await _userManager.AddClaimsAsync(user, claims);

                    _logger.LogInformation("Tenant admin {Username} logged in for tenant {TenantId}",
                        model.Username, model.TenantId);

                    // Update last login
                    user.LastLoginDate = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);

                    // Update tenant user activated date if first login
                    if (tenantUser.ActivatedAt == null)
                    {
                        tenantUser.ActivatedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    return RedirectToAction("Index", "OnboardingWizard", new { tenantId = model.TenantId });
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning("Tenant admin {Username} account locked out", model.Username);
                    return View("Lockout");
                }

                ModelState.AddModelError("", "Invalid username or password");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during tenant admin login");
                ModelState.AddModelError("", "An error occurred during login. Please try again.");
            }

            return View(model);
        }

        // GET: Account/ForgotTenantId
        [HttpGet("Account/ForgotTenantId")]
        [AllowAnonymous]
        public IActionResult ForgotTenantId()
        {
            return View(new ForgotTenantIdViewModel());
        }

        // POST: Account/ForgotTenantId
        [HttpPost("Account/ForgotTenantId")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotTenantId(ForgotTenantIdViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Find user by email
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    // Don't reveal if user exists - security best practice
                    model.TenantIdFound = false;
                    return View(model);
                }

                // Verify password before revealing Tenant ID
                var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password);
                if (!passwordValid)
                {
                    // Invalid password - don't reveal if user exists
                    model.TenantIdFound = false;
                    return View(model);
                }

                // Find active tenant for this user
                var tenantUser = await _context.TenantUsers
                    .Include(tu => tu.Tenant)
                    .Where(tu => tu.UserId == user.Id 
                        && tu.Status == "Active" 
                        && !tu.IsDeleted)
                    .OrderByDescending(tu => tu.ActivatedAt ?? tu.CreatedDate)
                    .FirstOrDefaultAsync();

                if (tenantUser?.Tenant == null)
                {
                    model.TenantIdFound = false;
                    return View(model);
                }

                // Only return tenant info for Admin/TENANT_ADMIN users (password verified)
                if (tenantUser.RoleCode == "Admin" || tenantUser.RoleCode == "TENANT_ADMIN" || tenantUser.RoleCode == "ADMINISTRATOR")
                {
                    model.TenantIdFound = true;
                    model.TenantId = tenantUser.Tenant.Id;
                    model.OrganizationName = tenantUser.Tenant.OrganizationName;
                    model.TenantSlug = tenantUser.Tenant.TenantSlug;
                    
                    _logger.LogInformation("Tenant ID lookup successful for email: {Email}, TenantId: {TenantId}, Role: {Role}", 
                        model.Email, model.TenantId, tenantUser.RoleCode);
                }
                else
                {
                    // Non-admin users - don't reveal tenant ID even with correct password
                    model.TenantIdFound = false;
                    _logger.LogWarning("Tenant ID lookup attempted by non-admin user: {Email}, Role: {Role}", 
                        model.Email, tenantUser.RoleCode);
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during tenant ID lookup for email: {Email}", model.Email);
                ModelState.AddModelError("", "An error occurred. Please try again later.");
                return View(model);
            }
        }
    }
}