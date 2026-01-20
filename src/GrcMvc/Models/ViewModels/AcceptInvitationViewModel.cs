using System.ComponentModel.DataAnnotations;

namespace GrcMvc.Models.ViewModels;

/// <summary>
/// ViewModel for accepting user invitation and setting password
/// </summary>
public class AcceptInvitationViewModel
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [EmailAddress]
    public string? Email { get; set; }

    public string? OrganizationName { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your password")]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;
}
