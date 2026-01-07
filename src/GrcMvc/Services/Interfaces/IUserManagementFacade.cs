namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Facade that routes to either legacy or enhanced services based on feature flags
/// </summary>
public interface IUserManagementFacade
{
    Task<UserDto> GetUserAsync(string userId);
    Task<bool> ResetPasswordAsync(string adminUserId, string targetUserId, string newPassword);
    Task<List<UserDto>> GetUsersAsync(int pageNumber, int pageSize);
}

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public List<string> Roles { get; set; } = new();
}
