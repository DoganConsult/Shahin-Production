namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Service for building navigation menu based on RBAC Features and Permissions
/// </summary>
public interface IMenuService
{
    /// <summary>
    /// Gets menu items accessible by the user based on their roles and features
    /// </summary>
    Task<List<MenuItemDto>> GetUserMenuItemsAsync(string userId);

    /// <summary>
    /// Checks if user has access to a specific feature
    /// </summary>
    Task<bool> HasFeatureAccessAsync(string userId, string featureCode);

    /// <summary>
    /// Checks if user has a specific permission
    /// </summary>
    Task<bool> HasPermissionAsync(string userId, string permissionCode);
}

/// <summary>
/// DTO for menu item
/// </summary>
public class MenuItemDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Permission { get; set; } = string.Empty;
    public int Order { get; set; }
    public List<MenuItemDto> Children { get; set; } = new();
}
