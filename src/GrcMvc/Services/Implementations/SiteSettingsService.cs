using GrcMvc.Configuration;
using GrcMvc.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Implementation of site settings service
/// تطبيق خدمة إعدادات الموقع
/// </summary>
public class SiteSettingsService : ISiteSettingsService
{
    private readonly SiteSettings _settings;

    public SiteSettingsService(IOptions<SiteSettings> options)
    {
        _settings = options.Value;
    }

    // Contact
    public string PhoneNumber => _settings.PhoneNumber;
    public string PhoneNumberDisplay => _settings.PhoneNumberDisplay;
    public string WhatsAppNumber => _settings.WhatsAppNumber;

    // Social Media
    public string LinkedInUrl => _settings.LinkedInUrl;
    public string TwitterUrl => _settings.TwitterUrl;
    public string YouTubeUrl => _settings.YouTubeUrl;
    public string GitHubUrl => _settings.GitHubUrl;

    // Address
    public string FullAddress => _settings.FullAddress;
    public string FullAddressAr => _settings.FullAddressAr;
    public string City => _settings.City;
    public string CityAr => _settings.CityAr;
    public string Country => _settings.Country;
    public string CountryAr => _settings.CountryAr;

    // Business Hours
    public string BusinessHours => _settings.BusinessHours;
    public string BusinessHoursAr => _settings.BusinessHoursAr;

    // Helpers
    public bool HasLinkedIn => _settings.HasLinkedIn;
    public bool HasTwitter => _settings.HasTwitter;
    public bool HasYouTube => _settings.HasYouTube;
    public bool HasGitHub => _settings.HasGitHub;
}
