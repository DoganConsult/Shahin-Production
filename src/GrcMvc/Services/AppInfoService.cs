using Microsoft.Extensions.Configuration;

namespace GrcMvc.Services;

/// <summary>
/// Centralized application information service
/// Provides consistent app branding across the entire application
/// </summary>
public interface IAppInfoService
{
    string Name { get; }
    string NameEn { get; }
    string FullName { get; }
    string FullNameEn { get; }
    string Version { get; }
    string BuildDate { get; }
    string Copyright { get; }
    string CompanyName { get; }
    string CompanyUrl { get; }
    string SupportEmail { get; }
    string Website { get; }
    int CopyrightYear { get; }
    string VersionDisplay { get; }
    string FooterText { get; }
    string FooterTextEn { get; }
}

public class AppInfoService : IAppInfoService
{
    private readonly IConfiguration _configuration;

    public AppInfoService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string Name => _configuration["AppInfo:Name"] ?? "شاهين";
    public string NameEn => _configuration["AppInfo:NameEn"] ?? "Shahin";
    public string FullName => _configuration["AppInfo:FullName"] ?? "شاهين - نظام الحوكمة والمخاطر والامتثال";
    public string FullNameEn => _configuration["AppInfo:FullNameEn"] ?? "Shahin GRC Platform";
    public string Version => _configuration["AppInfo:Version"] ?? "1.0.0";
    public string BuildDate => _configuration["AppInfo:BuildDate"] ?? DateTime.UtcNow.ToString("yyyy-MM-dd");
    public string Copyright => _configuration["AppInfo:Copyright"] ?? $"© {DateTime.UtcNow.Year} Shahin AI";
    public string CompanyName => _configuration["AppInfo:CompanyName"] ?? "Dogan Consult";
    public string CompanyUrl => _configuration["AppInfo:CompanyUrl"] ?? "https://www.doganconsult.com";
    public string SupportEmail => _configuration["AppInfo:SupportEmail"] ?? "support@shahin-ai.com";
    public string Website => _configuration["AppInfo:Website"] ?? "https://shahin-ai.com";
    
    public int CopyrightYear => DateTime.UtcNow.Year;
    
    public string VersionDisplay => $"v{Version}";
    
    public string FooterText => $"© {CopyrightYear} {Name} - الإصدار {Version}";
    
    public string FooterTextEn => $"© {CopyrightYear} {NameEn} - Version {Version}";
}
