using System.ComponentModel.DataAnnotations;

namespace GrcMvc.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 12)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        [Display(Name = "First Name")]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Department")]
        [StringLength(100)]
        public string? Department { get; set; }
    }

    public class ManageViewModel
    {
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Display(Name = "First Name")]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Department")]
        [StringLength(100)]
        public string? Department { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 12)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class LoginWith2faViewModel
    {
        [Required]
        [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Text)]
        [Display(Name = "Authenticator code")]
        public string TwoFactorCode { get; set; } = string.Empty;

        [Display(Name = "Remember this machine")]
        public bool RememberMachine { get; set; }

        public bool RememberMe { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 12)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;
    }

    public class LoginWithRecoveryCodeViewModel
    {
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Recovery Code")]
        public string RecoveryCode { get; set; } = string.Empty;
    }

    public class TenantAdminLoginViewModel
    {
        [Required(ErrorMessage = "Tenant ID is required")]
        [Display(Name = "Tenant ID")]
        public Guid TenantId { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }

    /// <summary>
    /// ViewModel for Tenant ID lookup when forgotten
    /// </summary>
    public class ForgotTenantIdViewModel
    {
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        public bool? TenantIdFound { get; set; }
        public Guid? TenantId { get; set; }
        public string? OrganizationName { get; set; }
        public string? TenantSlug { get; set; }
    }

    /// <summary>
    /// ViewModel for forced password change on first login
    /// </summary>
    public class ChangePasswordRequiredViewModel
    {
        [Required(ErrorMessage = "Current password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 12)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// ViewModel for Email MFA verification
    /// </summary>
    public class VerifyMfaViewModel
    {
        [Required(ErrorMessage = "رمز التحقق مطلوب")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "رمز التحقق يجب أن يكون 6 أرقام")]
        [Display(Name = "رمز التحقق")]
        public string Code { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
        public string? MaskedEmail { get; set; }
    }

    /// <summary>
    /// ViewModel for Link Expired page
    /// </summary>
    public class LinkExpiredViewModel
    {
        public string Message { get; set; } = "The link has expired.";
        public string ReturnUrl { get; set; } = "/";
        public bool CanResend { get; set; } = false;
    }

    /// <summary>
    /// ViewModel for Verification Sent page
    /// </summary>
    public class VerificationSentViewModel
    {
        public string Email { get; set; } = string.Empty;
        public string Message { get; set; } = "Verification email has been sent.";
    }

    /// <summary>
    /// ViewModel for Verify and Set Password page
    /// </summary>
    public class VerifyAndSetPasswordViewModel
    {
        [Required]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 12)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
        public Guid? TenantId { get; set; }
        public string? Token { get; set; }
    }

    /// <summary>
    /// ViewModel for Verify Email Pending page
    /// </summary>
    public class VerifyEmailPendingViewModel
    {
        public string Email { get; set; } = string.Empty;
        public string Message { get; set; } = "Please verify your email.";
        public Guid? TenantId { get; set; }
    }

    /// <summary>
    /// ViewModel for Settings page
    /// </summary>
    public class SettingsViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool EmailNotifications { get; set; }
        public string TimeZone { get; set; } = "UTC";
        public SecuritySettingsModel SecuritySettings { get; set; } = new();
        public AppSettingsModel AppSettings { get; set; } = new();
    }

    public class SecuritySettingsModel
    {
        public bool TwoFactorEnabled { get; set; }
        public bool RequireMfa { get; set; }
        public int SessionTimeout { get; set; } = 30;
    }

    public class AppSettingsModel
    {
        public string Theme { get; set; } = "light";
        public string Language { get; set; } = "en";
        public string DateFormat { get; set; } = "MM/dd/yyyy";
        public bool ShowNotifications { get; set; } = true;
        public string DefaultView { get; set; } = "dashboard";
        public int PageSize { get; set; } = 10;
    }
}
