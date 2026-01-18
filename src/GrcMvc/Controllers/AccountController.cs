using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using GrcMvc.Models.Entities;
using GrcMvc.Models.ViewModels;
using GrcMvc.Services.Interfaces;
using GrcMvc.Services;
using GrcMvc.Helpers;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        private readonly IAuditEventService? _auditEventService; // MEDIUM FIX: Audit logging for authentication events
        private readonly IAuthenticationAuditService? _authAuditService; // CRITICAL: Comprehensive authentication audit logging
        private readonly Data.GrcAuthDbContext? _authContext; // CRITICAL: Auth database context for PasswordHistory
        private readonly IPasswordHistoryService? _passwordHistoryService; // SECURITY: Password reuse prevention
        private readonly ISessionManagementService? _sessionManagementService; // SECURITY: Concurrent session limiting
        private readonly ICaptchaService? _captchaService; // SECURITY: Bot protection

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            IAppEmailSender emailSender,
            IGrcEmailService grcEmailService,
            ILogger<AccountController> logger,
            ITenantService tenantService,
            GrcDbContext context,
            IAuditEventService? auditEventService = null, // Optional for backward compatibility
            IAuthenticationAuditService? authAuditService = null, // CRITICAL: Authentication audit service
            Data.GrcAuthDbContext? authContext = null, // CRITICAL: Auth database context
            IPasswordHistoryService? passwordHistoryService = null, // SECURITY: Password reuse prevention
            ISessionManagementService? sessionManagementService = null, // SECURITY: Concurrent session limiting
            ICaptchaService? captchaService = null) // SECURITY: Bot protection
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
            _auditEventService = auditEventService;
            _authAuditService = authAuditService;
            _authContext = authContext;
            _passwordHistoryService = passwordHistoryService;
            _sessionManagementService = sessionManagementService;
            _captchaService = captchaService;
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
        [EnableRateLimiting("auth")] // MEDIUM PRIORITY SECURITY FIX: Rate limiting to prevent brute force attacks
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
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user != null)
                    {
                        // PHASE 1 LOGIN GATE: Block unverified users
                        if (!user.EmailConfirmed)
                        {
                            // Sign out immediately - they shouldn't be logged in
                            await _signInManager.SignOutAsync();

                            // Find their tenant to enable resend
                            var unverifiedTenant = await _context.Tenants
                                .FirstOrDefaultAsync(t => t.AdminEmail == user.Email && !t.IsEmailVerified);

                            if (unverifiedTenant != null)
                            {
                                _logger.LogWarning("Login blocked for unverified user {Email}, TenantId: {TenantId}",
                                    model.Email, unverifiedTenant.Id);
                                return View("VerifyEmailPending", new VerifyEmailPendingViewModel
                                {
                                    TenantId = unverifiedTenant.Id,
                                    Email = MaskEmail(user.Email ?? "")
                                });
                            }

                            // User exists but no tenant - generic error
                            _logger.LogWarning("Login blocked for unverified user {Email} with no matching tenant", model.Email);
                            ModelState.AddModelError("", "Please verify your email address before logging in.");
                            return View(model);
                        }

                        // CRITICAL FIX: Enhanced logging with IP, user agent, timestamp
                        var loginIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                        var loginUserAgent = HttpContext.Request.Headers["User-Agent"].ToString();
                        _logger.LogInformation(
                            "User {Email} (ID: {UserId}) logged in successfully from IP {IpAddress}, UserAgent: {UserAgent}",
                            model.Email, user.Id, loginIpAddress, loginUserAgent);

                        // CRITICAL FIX: Comprehensive authentication audit logging
                        if (_authAuditService != null)
                        {
                            try
                            {
                                // Log login attempt (success)
                                await _authAuditService.LogLoginAttemptAsync(
                                    userId: user.Id.ToString(), // Convert Guid to string
                                    email: model.Email,
                                    success: true,
                                    ipAddress: loginIpAddress,
                                    userAgent: loginUserAgent);

                                // Log authentication event
                                await _authAuditService.LogAuthenticationEventAsync(new AuthenticationAuditEvent
                                {
                                    UserId = user.Id.ToString(),
                                    Email = model.Email,
                                    EventType = "Login",
                                    Success = true,
                                    IpAddress = loginIpAddress,
                                    UserAgent = loginUserAgent,
                                    Message = $"User {model.Email} logged in successfully",
                                    Severity = "Info",
                                    Details = new Dictionary<string, object>
                                    {
                                        ["RememberMe"] = model.RememberMe,
                                        ["TenantId"] = (await _context.TenantUsers
                                            .FirstOrDefaultAsync(tu => tu.UserId == user.Id.ToString().ToString() && !tu.IsDeleted))?.TenantId.ToString() ?? "N/A"
                                    }
                                });

                                // Update LastLoginDate
                                user.LastLoginDate = DateTime.UtcNow;
                                await _userManager.UpdateAsync(user);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to log authentication audit event for login");
                            }
                        }

                        // MEDIUM PRIORITY FIX: Formal audit log entry for successful login (legacy - keep for backward compatibility)
                        if (_auditEventService != null)
                        {
                            try
                            {
                                var tenantId = Guid.Empty; // Will be resolved in ProcessPostLoginAsync
                                var tenantUser = await _context.TenantUsers
                                    .FirstOrDefaultAsync(tu => tu.UserId == user.Id.ToString().ToString() && !tu.IsDeleted);
                                if (tenantUser != null) tenantId = tenantUser.TenantId;

                                await _auditEventService.LogEventAsync(
                                    tenantId,
                                    "USER_LOGIN_SUCCESS",
                                    "User",
                                    user.Id.ToString(),
                                    "Login",
                                    user.Id.ToString(),
                                    System.Text.Json.JsonSerializer.Serialize(new
                                    {
                                        email = user.Email,
                                        ipAddress = loginIpAddress,
                                        userAgent = loginUserAgent,
                                        rememberMe = model.RememberMe,
                                        timestamp = DateTime.UtcNow
                                    }),
                                    Guid.NewGuid().ToString());
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to log audit event for login");
                            }
                        }

                        // MEDIUM PRIORITY FIX: Regenerate session ID on authentication to prevent session fixation
                        await HttpContext.Session.CommitAsync();

                        // Check if user must change password (first login or admin reset)
                        if (user.MustChangePassword)
                        {
                            _logger.LogInformation("User {Email} must change password on first login", model.Email);
                            return RedirectToAction(nameof(ChangePasswordRequired));
                        }

                        // MEDIUM PRIORITY FIX: Check password expiry (90 days for GRC compliance)
                        var passwordMaxAgeDays = _configuration.GetValue<int>("Security:PasswordMaxAgeDays", 90);
                        if (user.IsPasswordExpired(passwordMaxAgeDays))
                        {
                            _logger.LogWarning("User {Email} password expired (last changed: {LastChanged})",
                                model.Email, user.LastPasswordChangedAt);
                            TempData["ErrorMessage"] = "Your password has expired. Please change it to continue.";
                            return RedirectToAction(nameof(ChangePassword));
                        }
                        else if (user.DaysUntilPasswordExpires(passwordMaxAgeDays) <= 7)
                        {
                            // Warn user if password expires in 7 days or less
                            var daysLeft = user.DaysUntilPasswordExpires(passwordMaxAgeDays);
                            TempData["WarningMessage"] = $"Your password will expire in {daysLeft} day(s). Please change it soon.";
                        }

                        // Process post-login logic (add tenant claim, check onboarding, determine redirect)
                        return await ProcessPostLoginAsync(user, model.RememberMe);
                    }

                    // Redirect to role-based dashboard
                    return RedirectToAction(nameof(LoginRedirect));
                }

                if (result.IsLockedOut)
                {
                    var loginIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    var loginUserAgent = HttpContext.Request.Headers["User-Agent"].ToString();
                    
                    _logger.LogWarning("User {Email} account locked out.", model.Email);
                    
                    // CRITICAL FIX: Log account lockout event
                    if (_authAuditService != null)
                    {
                        try
                        {
                            var user = await _userManager.FindByEmailAsync(model.Email);
                            if (user != null)
                            {
                                await _authAuditService.LogAccountLockoutAsync(
                                    userId: user.Id.ToString(),
                                    reason: "Too many failed login attempts",
                                    ipAddress: loginIpAddress);

                                await _authAuditService.LogLoginAttemptAsync(
                                    userId: user.Id.ToString(),
                                    email: model.Email,
                                    success: false,
                                    ipAddress: loginIpAddress,
                                    userAgent: loginUserAgent,
                                    failureReason: "Account locked out",
                                    triggeredLockout: true);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to log account lockout event");
                        }
                    }
                    
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

                // CRITICAL FIX: Enhanced failed login logging with comprehensive audit trail
                var failedLoginIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var failedLoginUserAgent = HttpContext.Request.Headers["User-Agent"].ToString();
                _logger.LogWarning(
                    "Failed login attempt for email {Email} from IP {IpAddress} at {Timestamp}, UserAgent: {UserAgent}",
                    model.Email, failedLoginIp, DateTime.UtcNow, failedLoginUserAgent);

                // CRITICAL FIX: Log failed login attempt to authentication audit system
                if (_authAuditService != null)
                {
                    try
                    {
                        var failedUser = await _userManager.FindByEmailAsync(model.Email);
                        var failureReason = result.IsLockedOut ? "Account locked out" 
                                    : result.IsNotAllowed ? "Account not allowed" 
                                    : result.RequiresTwoFactor ? "Two-factor authentication required"
                                    : "Invalid credentials";
                        
                        await _authAuditService.LogLoginAttemptAsync(
                            userId: failedUser?.Id.ToString(),
                            email: model.Email,
                            success: false,
                            ipAddress: failedLoginIp,
                            userAgent: failedLoginUserAgent,
                            failureReason: failureReason,
                            triggeredLockout: result.IsLockedOut);

                        await _authAuditService.LogAuthenticationEventAsync(new AuthenticationAuditEvent
                        {
                            UserId = failedUser?.Id.ToString(),
                            Email = model.Email,
                            EventType = "FailedLogin",
                            Success = false,
                            IpAddress = failedLoginIp,
                            UserAgent = failedLoginUserAgent,
                            Message = $"Failed login attempt for {model.Email}: {failureReason}",
                            Severity = result.IsLockedOut ? "Warning" : "Info",
                            Details = new Dictionary<string, object>
                            {
                                ["FailureReason"] = failureReason,
                                ["IsLockedOut"] = result.IsLockedOut,
                                ["IsNotAllowed"] = result.IsNotAllowed,
                                ["RequiresTwoFactor"] = result.RequiresTwoFactor
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to log authentication audit event for failed login");
                    }
                }

                // MEDIUM PRIORITY FIX: Log failed login to audit trail (legacy - keep for backward compatibility)
                if (_auditEventService != null)
                {
                    try
                    {
                        var failedUser = await _userManager.FindByEmailAsync(model.Email);
                        var failedUserId = failedUser?.Id.ToString() ?? "unknown";
                        var tenantId = Guid.Empty;

                        await _auditEventService.LogEventAsync(
                            tenantId,
                            "USER_LOGIN_FAILED",
                            "User",
                            failedUserId,
                            "LoginFailed",
                            failedUserId,
                            System.Text.Json.JsonSerializer.Serialize(new
                            {
                                email = model.Email,
                                ipAddress = failedLoginIp,
                                userAgent = failedLoginUserAgent,
                                reason = result.ToString(),
                                timestamp = DateTime.UtcNow
                            }),
                            Guid.NewGuid().ToString());
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to log audit event for failed login");
                    }
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
                .FirstOrDefaultAsync(tu => tu.UserId == user.Id.ToString().ToString() && !tu.IsDeleted);

            if (tenantUser?.TenantId == null)
            {
                // No tenant - go to dashboard (or appropriate default)
                return RedirectToAction(nameof(LoginRedirect));
            }

            // Add TenantId claim if tenant exists
            var existingClaims = await _userManager.GetClaimsAsync(user);
            var hasTenantClaim = existingClaims.Any(c => c.Type == ClaimConstants.TenantId);

            if (!hasTenantClaim)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimConstants.TenantId, tenantUser.TenantId.ToString())
                };
                await _userManager.AddClaimsAsync(user, claims);
                _logger.LogDebug("Added TenantId claim for user {Email}", user.Email);
            }

            // MEDIUM PRIORITY FIX: Regenerate session ID on authentication to prevent session fixation
            await HttpContext.Session.CommitAsync();
            await HttpContext.Session.LoadAsync();

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
        [EnableRateLimiting("auth")] // SECURITY: Rate limiting to prevent registration abuse
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
                    FirstName = model.FirstName ?? string.Empty,
                    LastName = model.LastName ?? string.Empty,
                    Department = model.Department ?? string.Empty,
                    JobTitle = model.JobTitle ?? string.Empty
                };
                
                // UserManager.CreateAsync will set UserName and Email based on the user object
                // We need to use a different approach since ABP has protected setters
                var result = await _userManager.CreateAsync(user, model.Password);
                
                // Set the email after creation using UserManager
                if (result.Succeeded)
                {
                    await _userManager.SetUserNameAsync(user, model.Email);
                    await _userManager.SetEmailAsync(user, model.Email);
                }
                
                // Update EmailConfirmed after creation
                if (result.Succeeded && !HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsProduction())
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    await _userManager.ConfirmEmailAsync(user, token);
                }

                if (result.Succeeded)
                {
                    _logger.LogInformation("User {Email} created a new account.", model.Email);

                    // Assign role based on JobTitle selection
                    var roleToAssign = MapJobTitleToRole(model.JobTitle);
                    if (!await _roleManager.RoleExistsAsync(roleToAssign))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(roleToAssign));
                    }
                    await _userManager.AddToRoleAsync(user, roleToAssign);
                    _logger.LogInformation("User {Email} assigned role {Role} based on JobTitle {JobTitle}.",
                        model.Email, roleToAssign, model.JobTitle);

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

                    // New users need to complete onboarding (create/join a tenant)
                    // Redirect to Onboarding instead of Home to avoid Access Denied
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Onboarding");
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

            // SECURITY: Check password history to prevent reuse
            if (_passwordHistoryService != null)
            {
                var isInHistory = await _passwordHistoryService.IsPasswordInHistoryAsync(user.Id.ToString(), model.NewPassword);
                if (isInHistory)
                {
                    ModelState.AddModelError(nameof(model.NewPassword), 
                        "This password has been used recently. Please choose a different password.");
                    return View(model);
                }
            }

            // CRITICAL FIX: Capture old password hash BEFORE changing password
            string? oldPasswordHash = user.PasswordHash;

            // Change password
            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                // CRITICAL FIX: Store password history and log audit event
                try
                {
                    // Store old password hash in history (captured before change)
                    if (_authContext != null && !string.IsNullOrEmpty(oldPasswordHash))
                    {
                        var passwordHistory = new PasswordHistory
                        {
                            UserId = user.Id.ToString(),
                            PasswordHash = oldPasswordHash, // Store old hash (captured before change)
                            ChangedAt = DateTime.UtcNow,
                            ChangedByUserId = user.Id.ToString(),
                            Reason = "First login password change",
                            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                            UserAgent = HttpContext.Request.Headers["User-Agent"].ToString()
                        };
                        _authContext.PasswordHistory.Add(passwordHistory);
                        await _authContext.SaveChangesAsync();
                    }

                    // Clear the must change password flag and update timestamp
                    user.MustChangePassword = false;
                    user.LastPasswordChangedAt = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);

                    // Log audit event
                    if (_authAuditService != null)
                    {
                        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
                        await _authAuditService.LogPasswordChangeAsync(
                            userId: user.Id.ToString(),
                            changedByUserId: user.Id.ToString(),
                            reason: "First login password change",
                            ipAddress: ipAddress,
                            userAgent: userAgent);
                    }

                    _logger.LogInformation("User {Email} changed password on first login", user.Email);
                    TempData["SuccessMessage"] = "Password changed successfully. Welcome!";

                    // Process post-login logic (add tenant claim, check onboarding)
                    return await ProcessPostLoginAsync(user, rememberMe: false);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to store password history for user {UserId}", user.Id);
                    // Continue even if audit logging fails
                    user.MustChangePassword = false;
                    user.LastPasswordChangedAt = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);
                    return await ProcessPostLoginAsync(user, rememberMe: false);
                }
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
            // Set phone number using UserManager to handle protected setter
            if (!string.IsNullOrEmpty(model.PhoneNumber))
            {
                await _userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
            }

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

            // SECURITY: Check password history to prevent reuse
            if (_passwordHistoryService != null)
            {
                var isInHistory = await _passwordHistoryService.IsPasswordInHistoryAsync(user.Id.ToString(), model.NewPassword);
                if (isInHistory)
                {
                    ModelState.AddModelError(nameof(model.NewPassword), 
                        "This password has been used recently. Please choose a different password.");
                    return View(model);
                }
            }

            // CRITICAL FIX: Capture old password hash BEFORE changing password
            string? oldPasswordHash = user.PasswordHash;

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

            // CRITICAL FIX: Store password history and log audit event
            try
            {
                // Store old password hash in history (captured before change)
                if (_authContext != null && !string.IsNullOrEmpty(oldPasswordHash))
                {
                    var passwordHistory = new PasswordHistory
                    {
                        UserId = user.Id.ToString(),
                        PasswordHash = oldPasswordHash, // Store old hash (captured before change)
                        ChangedAt = DateTime.UtcNow,
                        ChangedByUserId = user.Id.ToString(),
                        Reason = "User initiated",
                        IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                        UserAgent = HttpContext.Request.Headers["User-Agent"].ToString()
                    };
                    _authContext.PasswordHistory.Add(passwordHistory);
                    await _authContext.SaveChangesAsync();
                }

                // Update password change timestamp
                user.LastPasswordChangedAt = DateTime.UtcNow;
                user.MustChangePassword = false;
                await _userManager.UpdateAsync(user);

                // Log audit event
                if (_authAuditService != null)
                {
                    var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                    var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
                    await _authAuditService.LogPasswordChangeAsync(
                        userId: user.Id.ToString(),
                        changedByUserId: user.Id.ToString(),
                        reason: "User initiated",
                        ipAddress: ipAddress,
                        userAgent: userAgent);
                }

                // SECURITY: Revoke all other sessions on password change
                if (_sessionManagementService != null)
                {
                    await _sessionManagementService.RevokeAllSessionsAsync(
                        user.Id.ToString(), 
                        "Password changed - all sessions revoked for security");
                    _logger.LogInformation("Revoked all sessions for user {UserId} due to password change", user.Id);
                }

                await _signInManager.RefreshSignInAsync(user);
                _logger.LogInformation("User {Email} changed their password successfully.", user.Email);

                TempData["SuccessMessage"] = "Your password has been changed.";
                return RedirectToAction(nameof(Manage));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to store password history for user {UserId}", user.Id);
                // Continue even if audit logging fails
                user.LastPasswordChangedAt = DateTime.UtcNow;
                user.MustChangePassword = false;
                await _userManager.UpdateAsync(user);
                await _signInManager.RefreshSignInAsync(user);
                TempData["SuccessMessage"] = "Your password has been changed.";
                return RedirectToAction(nameof(Manage));
            }
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
        [EnableRateLimiting("auth")] // SECURITY: Rate limiting to prevent password reset abuse
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                // MEDIUM PRIORITY FIX: Generic message regardless of user existence to prevent user enumeration
                // Always return the same response to prevent timing attacks
                var user = await _userManager.FindByEmailAsync(model.Email);
                
                // Add artificial delay to prevent timing-based user enumeration
                await Task.Delay(100);
                
                if (user == null)
                {
                    // Don't reveal that the user does not exist
                    _logger.LogWarning("Password reset requested for non-existent email: {Email} from IP {IpAddress}",
                        model.Email, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
                    // MEDIUM PRIORITY FIX: Return generic confirmation regardless of user existence
                    return RedirectToAction(nameof(ForgotPasswordConfirmation));
                }

                _logger.LogInformation("Generating password reset token for user: {Email}", model.Email);
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Action(nameof(ResetPassword), "Account", new { code }, protocol: HttpContext.Request.Scheme);
                
                // SECURITY: Never log password reset URLs - they contain sensitive tokens
                _logger.LogDebug("Password reset link generated for user: {Email}", model.Email);
                
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

            // SECURITY: Check password history to prevent reuse
            if (_passwordHistoryService != null)
            {
                var isInHistory = await _passwordHistoryService.IsPasswordInHistoryAsync(user.Id.ToString(), model.Password);
                if (isInHistory)
                {
                    ModelState.AddModelError(nameof(model.Password), 
                        "This password has been used recently. Please choose a different password.");
                    return View(model);
                }
            }

            // CRITICAL FIX: Capture old password hash BEFORE resetting password
            string? oldPasswordHash = user.PasswordHash;

            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                // CRITICAL FIX: Store password history and log audit event
                try
                {
                    // Store old password hash in history (captured before change)
                    if (_authContext != null && !string.IsNullOrEmpty(oldPasswordHash))
                    {
                        var passwordHistory = new PasswordHistory
                        {
                            UserId = user.Id.ToString(),
                            PasswordHash = oldPasswordHash, // Store old hash (captured before change)
                            ChangedAt = DateTime.UtcNow,
                            ChangedByUserId = user.Id.ToString(),
                            Reason = "Password reset via email",
                            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                            UserAgent = HttpContext.Request.Headers["User-Agent"].ToString()
                        };
                        _authContext.PasswordHistory.Add(passwordHistory);
                        await _authContext.SaveChangesAsync();
                    }

                    // Update password change timestamp
                    user.LastPasswordChangedAt = DateTime.UtcNow;
                    user.MustChangePassword = false;
                    await _userManager.UpdateAsync(user);

                    // Log audit event
                    if (_authAuditService != null)
                    {
                        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
                        await _authAuditService.LogPasswordChangeAsync(
                            userId: user.Id.ToString(),
                            changedByUserId: user.Id.ToString(),
                            reason: "Password reset via email",
                            ipAddress: ipAddress,
                            userAgent: userAgent);
                    }

                    // SECURITY: Revoke all sessions on password reset
                    if (_sessionManagementService != null)
                    {
                        await _sessionManagementService.RevokeAllSessionsAsync(
                            user.Id.ToString(), 
                            "Password reset - all sessions revoked for security");
                        _logger.LogInformation("Revoked all sessions for user {UserId} due to password reset", user.Id);
                    }

                    // MEDIUM PRIORITY FIX: Send confirmation email after successful password reset
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
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to store password history for user {UserId}", user.Id);
                    // Continue even if audit logging fails
                    user.LastPasswordChangedAt = DateTime.UtcNow;
                    user.MustChangePassword = false;
                    await _userManager.UpdateAsync(user);
                    return RedirectToAction(nameof(ResetPasswordConfirmation));
                }
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

        /// <summary>
        /// DEPRECATED: Custom JWT token endpoint
        /// Use /connect/token with OAuth2 flow instead (ABP OpenIddict)
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [Route("api/account/token")]
        [Obsolete("Use /connect/token endpoint with OAuth2 flow instead")]
        public IActionResult GenerateToken([FromBody] LoginViewModel model)
        {
            _logger.LogWarning("Deprecated /api/account/token endpoint called. Redirecting to OpenIddict.");
            return BadRequest(new
            {
                error = "endpoint_deprecated",
                message = "This endpoint is deprecated. Use /connect/token with OAuth2 password grant flow.",
                redirect = "/connect/token",
                documentation = "https://docs.shahin-ai.com/api/authentication"
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
                .FirstOrDefaultAsync(tu => tu.UserId == user.Id.ToString().ToString() && tu.TenantId == tenantId && !tu.IsDeleted);

            // Get assigned profiles
            var assignments = await profileService.GetUserAssignmentsAsync(userId.ToString(), tenantId);
            var permissions = await profileService.GetUserPermissionsAsync(userId.ToString(), tenantId);
            var workflowRoles = await profileService.GetUserWorkflowRolesAsync(userId.ToString(), tenantId);

            // Get notification preferences
            var notifPrefs = await notificationService.GetUserPreferencesAsync(userId.ToString(), tenantId);

            // Get pending tasks count
            var pendingTasks = await dbContext.WorkflowTasks
                .CountAsync(t => t.AssignedToUserId.ToString() == userId.ToString() &&
                    t.Status == "Pending" && !t.IsDeleted);

            var model = new UserProfileViewModel
            {
                UserId = userId.ToString(),
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
                    .FirstOrDefaultAsync(tu => tu.TenantId == model.TenantId && tu.UserId == user.Id.ToString());

                // MEDIUM PRIORITY FIX: Use RoleConstants instead of magic string
                if (tenantUser == null || !RoleConstants.IsTenantAdmin(tenantUser.RoleCode) || tenantUser.Status != "Active")
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
                    var claims = new List<Claim> { new Claim(ClaimConstants.TenantId, model.TenantId.ToString()) };
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
                    .Where(tu => tu.UserId == user.Id.ToString() 
                        && tu.Status == "Active" 
                        && !tu.IsDeleted)
                    .OrderByDescending(tu => tu.ActivatedAt ?? tu.CreatedDate)
                    .FirstOrDefaultAsync();

                if (tenantUser?.Tenant == null)
                {
                    model.TenantIdFound = false;
                    return View(model);
                }

                // MEDIUM PRIORITY FIX: Use RoleConstants instead of magic strings
                if (RoleConstants.IsTenantAdmin(tenantUser.RoleCode))
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

        /// <summary>
        /// Maps JobTitle selection to appropriate Identity Role
        /// </summary>
        private static string MapJobTitleToRole(string? jobTitle)
        {
            return jobTitle switch
            {
                // Executive roles - TenantAdmin
                "TenantAdmin" or "ChiefRiskOfficer" or "ChiefComplianceOfficer" or "ExecutiveDirector" => "TenantAdmin",

                // Management roles - appropriate manager roles
                "RiskManager" => "RiskManager",
                "ComplianceManager" => "ComplianceManager",
                "AuditManager" => "Auditor",
                "SecurityManager" => "RiskManager",
                "LegalManager" => "PolicyManager",
                "FinanceManager" => "FinanceManager",
                "HRManager" => "OperationalManager",
                "OperationalManager" => "OperationalManager",

                // Operational roles
                "ComplianceOfficer" => "ComplianceOfficer",
                "RiskAnalyst" => "RiskAnalyst",
                "PrivacyOfficer" => "ComplianceOfficer",
                "QualityAssuranceManager" => "Auditor",
                "ProcessOwner" => "User",

                // Support roles
                "DocumentationSpecialist" => "Viewer",
                "ReportingAnalyst" => "BusinessAnalyst",
                "BoardMember" => "BoardMember",

                // Default
                _ => "User"
            };
        }

        /// <summary>
        /// Mask email for display (j***@company.com)
        /// </summary>
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

        // =====================================================================
        // PHASE 1: Email Verification + Set Password Flow
        // =====================================================================

        /// <summary>
        /// GET: /account/verify - Verify email and show set password form
        /// </summary>
        [HttpGet("verify")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyAndSetPassword(Guid tenantId, string token)
        {
            if (tenantId == Guid.Empty || string.IsNullOrEmpty(token))
            {
                return View("LinkExpired", new LinkExpiredViewModel { CanResend = false });
            }

            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null)
            {
                _logger.LogWarning("Verification attempted for non-existent tenant {TenantId}", tenantId);
                return View("LinkExpired", new LinkExpiredViewModel { CanResend = false });
            }

            // Already verified - redirect to login
            if (tenant.IsEmailVerified)
            {
                _logger.LogInformation("Verification link used for already verified tenant {TenantId}", tenantId);
                TempData["Success"] = "Your email is already verified. Please log in.";
                return RedirectToAction(nameof(Login));
            }

            // Verify token hash matches
            if (!TokenHelper.VerifyToken(token, tenant.EmailVerificationTokenHash))
            {
                _logger.LogWarning("Invalid verification token for tenant {TenantId}", tenantId);
                return View("LinkExpired", new LinkExpiredViewModel
                {
                    CanResend = true,
                    TenantId = tenantId,
                    Email = MaskEmail(tenant.AdminEmail)
                });
            }

            // Check expiration (24 hours)
            if (tenant.EmailVerificationTokenExpiresAt < DateTime.UtcNow)
            {
                _logger.LogWarning("Expired verification token for tenant {TenantId}", tenantId);
                return View("LinkExpired", new LinkExpiredViewModel
                {
                    CanResend = true,
                    TenantId = tenantId,
                    Email = MaskEmail(tenant.AdminEmail)
                });
            }

            // Token valid - show password form
            return View(new VerifyAndSetPasswordViewModel
            {
                TenantId = tenantId,
                Token = token,
                Email = MaskEmail(tenant.AdminEmail)
            });
        }

        /// <summary>
        /// POST: /account/verify - Process password and complete verification
        /// </summary>
        [HttpPost("verify")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyAndSetPassword(VerifyAndSetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var tenant = await _context.Tenants.FindAsync(model.TenantId);
            if (tenant == null)
            {
                return View("LinkExpired", new LinkExpiredViewModel { CanResend = false });
            }

            // Re-verify token (in case of replay)
            if (!TokenHelper.VerifyToken(model.Token, tenant.EmailVerificationTokenHash))
            {
                _logger.LogWarning("Invalid token on POST for tenant {TenantId}", model.TenantId);
                return View("LinkExpired", new LinkExpiredViewModel
                {
                    CanResend = true,
                    TenantId = model.TenantId,
                    Email = MaskEmail(tenant.AdminEmail)
                });
            }

            // Check expiration again
            if (tenant.EmailVerificationTokenExpiresAt < DateTime.UtcNow)
            {
                return View("LinkExpired", new LinkExpiredViewModel
                {
                    CanResend = true,
                    TenantId = model.TenantId,
                    Email = MaskEmail(tenant.AdminEmail)
                });
            }

            // Find the user
            var user = await _userManager.FindByEmailAsync(tenant.AdminEmail);
            if (user == null)
            {
                _logger.LogError("User not found for verified tenant {TenantId}", model.TenantId);
                ModelState.AddModelError("", "An error occurred. Please contact support.");
                return View(model);
            }

            // Use Identity's password reset mechanism to set password
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }

            // Mark user as verified
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            await _userManager.ConfirmEmailAsync(user, token);
            user.MustChangePassword = false;
            user.LastPasswordChangedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // Mark tenant as verified and active
            tenant.IsEmailVerified = true;
            tenant.EmailVerifiedAt = DateTime.UtcNow;
            tenant.EmailVerificationTokenHash = null; // Clear token (single use)
            tenant.Status = "Active";
            tenant.IsActive = true;

            // Issue #7: Update TenantUser activation status
            var tenantUser = await _context.TenantUsers
                .FirstOrDefaultAsync(tu => tu.TenantId == tenant.Id && tu.UserId == user.Id.ToString());
            if (tenantUser != null)
            {
                tenantUser.Status = "Active";
                tenantUser.ActivatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Email verified and password set for tenant {TenantId}, user {UserId}",
                tenant.Id, user.Id);

            // Store password in history if service available
            if (_authContext != null && !string.IsNullOrEmpty(user.PasswordHash))
            {
                try
                {
                    var passwordHistory = new PasswordHistory
                    {
                        UserId = user.Id.ToString(),
                        PasswordHash = user.PasswordHash,
                        ChangedAt = DateTime.UtcNow,
                        ChangedByUserId = user.Id.ToString(),
                        Reason = "Initial password set during email verification",
                        IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                        UserAgent = HttpContext.Request.Headers["User-Agent"].ToString()
                    };
                    _authContext.PasswordHistory.Add(passwordHistory);
                    await _authContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to store initial password history for user {UserId}", user.Id);
                }
            }

            // Sign in the user
            await _signInManager.SignInAsync(user, isPersistent: false);

            // Issue #15: Send welcome email after successful verification
            try
            {
                var userName = $"{user.FirstName} {user.LastName}".Trim();
                if (string.IsNullOrEmpty(userName))
                    userName = user.Email?.Split('@')[0] ?? "User";

                var loginUrl = Url.Action("Login", "Account", null, Request.Scheme) ?? string.Empty;

                await _grcEmailService.SendWelcomeEmailAsync(
                    user.Email!,
                    userName,
                    loginUrl,
                    organizationName: "Shahin GRC Platform",
                    isArabic: true);

                _logger.LogInformation("Welcome email sent to {Email} after verification", user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send welcome email to {Email}", user.Email);
                // Don't fail the verification process
            }

            TempData["Success"] = "Your email has been verified and password set. Welcome!";

            // Redirect to onboarding wizard
            return RedirectToAction("Index", "OnboardingWizard", new { tenantId = tenant.Id });
        }
    }

    // =====================================================================
    // Phase 1 ViewModels for Email Verification
    // =====================================================================

    public class VerifyAndSetPasswordViewModel
    {
        public Guid TenantId { get; set; }
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty; // Masked email for display

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class LinkExpiredViewModel
    {
        public bool CanResend { get; set; }
        public Guid TenantId { get; set; }
        public string Email { get; set; } = string.Empty;
    }

    public class VerifyEmailPendingViewModel
    {
        public Guid TenantId { get; set; }
        public string Email { get; set; } = string.Empty;
    }
} // End namespace GrcMvc.Controllers