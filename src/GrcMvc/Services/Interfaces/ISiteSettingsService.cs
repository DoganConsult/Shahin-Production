namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Service for accessing site-wide settings (contact info, social media, etc.)
/// خدمة الوصول إلى إعدادات الموقع الشاملة
/// </summary>
public interface ISiteSettingsService
{
    // Contact
    string PhoneNumber { get; }
    string PhoneNumberDisplay { get; }
    string WhatsAppNumber { get; }

    // Social Media
    string LinkedInUrl { get; }
    string TwitterUrl { get; }
    string YouTubeUrl { get; }
    string GitHubUrl { get; }

    // Address
    string FullAddress { get; }
    string FullAddressAr { get; }
    string City { get; }
    string CityAr { get; }
    string Country { get; }
    string CountryAr { get; }

    // Business Hours
    string BusinessHours { get; }
    string BusinessHoursAr { get; }

    // Helpers
    bool HasLinkedIn { get; }
    bool HasTwitter { get; }
    bool HasYouTube { get; }
    bool HasGitHub { get; }
}
