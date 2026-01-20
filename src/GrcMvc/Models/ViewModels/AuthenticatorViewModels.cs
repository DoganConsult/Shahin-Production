using System.ComponentModel.DataAnnotations;

namespace GrcMvc.Models.ViewModels;

/// <summary>
/// ViewModel for enabling authenticator app 2FA
/// </summary>
public class EnableAuthenticatorViewModel
{
    public string SharedKey { get; set; } = string.Empty;
    public string QrCodeUrl { get; set; } = string.Empty;

    [Required(ErrorMessage = "Verification code is required")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Code must be 6 digits")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Code must be 6 digits")]
    public string VerificationCode { get; set; } = string.Empty;
}

/// <summary>
/// ViewModel for disabling 2FA
/// </summary>
public class Disable2faViewModel
{
    [Required(ErrorMessage = "Password is required to disable 2FA")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// ViewModel for 2FA recovery codes
/// </summary>
public class RecoveryCodesViewModel
{
    public List<string> RecoveryCodes { get; set; } = new();
    public bool ShowRecoveryCodes { get; set; }
}

/// <summary>
/// ViewModel for security settings page
/// </summary>
public class SecuritySettingsViewModel
{
    public bool Is2faEnabled { get; set; }
    public bool HasAuthenticator { get; set; }
    public int RecoveryCodesLeft { get; set; }
    public bool IsMachineRemembered { get; set; }
    public DateTime? LastPasswordChange { get; set; }
    public List<SecurityLogItem> RecentSecurityLogs { get; set; } = new();
}

/// <summary>
/// Security log item for display
/// </summary>
public class SecurityLogItem
{
    public DateTime Timestamp { get; set; }
    public string Action { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string Browser { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
}
