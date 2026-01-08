using System.ComponentModel.DataAnnotations;

namespace GrcMvc.Configuration;

/// <summary>
/// Site-wide settings for contact info, social media, and branding
/// إعدادات الموقع الشاملة لمعلومات الاتصال ووسائل التواصل الاجتماعي والعلامة التجارية
/// </summary>
public class SiteSettings
{
    public const string SectionName = "SiteSettings";

    // Contact Information
    [Required]
    public string PhoneNumber { get; set; } = "+966 11 000 0000";

    public string PhoneNumberDisplay { get; set; } = "+966 11 000 0000";

    public string WhatsAppNumber { get; set; } = "+966500000000";

    public string FaxNumber { get; set; } = "";

    // Social Media URLs
    public string LinkedInUrl { get; set; } = "https://linkedin.com/company/shahin-ai";

    public string TwitterUrl { get; set; } = "https://twitter.com/shahin_ai";

    public string YouTubeUrl { get; set; } = "https://youtube.com/@shahin-ai";

    public string GitHubUrl { get; set; } = "";

    public string FacebookUrl { get; set; } = "";

    public string InstagramUrl { get; set; } = "";

    // Address - English
    public string AddressLine1 { get; set; } = "King Fahd Road";

    public string AddressLine2 { get; set; } = "Riyadh 12345";

    public string City { get; set; } = "Riyadh";

    public string Country { get; set; } = "Saudi Arabia";

    // Address - Arabic
    public string AddressLine1Ar { get; set; } = "طريق الملك فهد";

    public string AddressLine2Ar { get; set; } = "الرياض 12345";

    public string CityAr { get; set; } = "الرياض";

    public string CountryAr { get; set; } = "المملكة العربية السعودية";

    // Business Hours
    public string BusinessHours { get; set; } = "Sun-Thu: 9:00 AM - 6:00 PM";

    public string BusinessHoursAr { get; set; } = "الأحد-الخميس: 9:00 ص - 6:00 م";

    // Helper properties
    public string FullAddress => $"{AddressLine1}, {AddressLine2}, {City}, {Country}";

    public string FullAddressAr => $"{AddressLine1Ar}، {AddressLine2Ar}، {CityAr}، {CountryAr}";

    public bool HasLinkedIn => !string.IsNullOrWhiteSpace(LinkedInUrl);

    public bool HasTwitter => !string.IsNullOrWhiteSpace(TwitterUrl);

    public bool HasYouTube => !string.IsNullOrWhiteSpace(YouTubeUrl);

    public bool HasGitHub => !string.IsNullOrWhiteSpace(GitHubUrl);

    public bool HasFacebook => !string.IsNullOrWhiteSpace(FacebookUrl);

    public bool HasInstagram => !string.IsNullOrWhiteSpace(InstagramUrl);
}
