using Microsoft.Extensions.DependencyInjection;

namespace GrcMvc.Data.Menu;

/// <summary>
/// Simple menu interfaces to replace Volo.Abp.UI.Navigation
/// </summary>
public interface IMenuContributor
{
    Task ConfigureMenuAsync(MenuConfigurationContext context);
}

public class MenuConfigurationContext
{
    public required IApplicationMenu Menu { get; init; }
    public required IServiceProvider ServiceProvider { get; init; }
}

public interface IApplicationMenu
{
    string Name { get; }
    void AddItem(ApplicationMenuItem item);
}

public class ApplicationMenu : IApplicationMenu
{
    private readonly List<ApplicationMenuItem> _items = new();
    
    public string Name { get; set; } = "Main";
    
    public void AddItem(ApplicationMenuItem item)
    {
        _items.Add(item);
    }
    
    public IReadOnlyList<ApplicationMenuItem> Items => _items;
}

public class ApplicationMenuItem
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Url { get; set; }
    public string? Icon { get; set; }
    public string? Permission { get; set; }
    private readonly List<ApplicationMenuItem> _items = new();
    
    public ApplicationMenuItem(string id, string displayName, string? url = null, string? icon = null)
    {
        Id = id;
        DisplayName = displayName;
        Url = url;
        Icon = icon;
    }
    
    public ApplicationMenuItem RequirePermissions(string permission)
    {
        Permission = permission;
        return this;
    }
    
    public void AddItem(ApplicationMenuItem item)
    {
        _items.Add(item);
    }
    
    public IReadOnlyList<ApplicationMenuItem> Items => _items;
}

public static class StandardMenus
{
    public const string Main = "Main";
}
