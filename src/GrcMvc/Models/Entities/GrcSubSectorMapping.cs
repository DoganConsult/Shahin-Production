using System.ComponentModel.DataAnnotations;

namespace GrcMvc.Models.Entities;

/// <summary>
/// Maps GOSI 70+ sub-sectors to the 18 main GRC sectors
/// Based on Saudi Arabia's National Classification of Economic Activities (ISIC Rev 4)
/// </summary>
public class GrcSubSectorMapping
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// GOSI/ISIC code (e.g., "01", "05", "10", "64")
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string GosiCode { get; set; } = string.Empty;
    
    /// <summary>
    /// ISIC Section letter (A-U)
    /// </summary>
    [Required]
    [MaxLength(2)]
    public string IsicSection { get; set; } = string.Empty;
    
    /// <summary>
    /// GOSI sub-sector name in English
    /// </summary>
    [Required]
    [MaxLength(300)]
    public string SubSectorNameEn { get; set; } = string.Empty;
    
    /// <summary>
    /// GOSI sub-sector name in Arabic
    /// </summary>
    [MaxLength(300)]
    public string SubSectorNameAr { get; set; } = string.Empty;
    
    /// <summary>
    /// The main GRC sector code this maps to (one of 18)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string MainSectorCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Main sector name in English
    /// </summary>
    [MaxLength(200)]
    public string MainSectorNameEn { get; set; } = string.Empty;
    
    /// <summary>
    /// Main sector name in Arabic
    /// </summary>
    [MaxLength(200)]
    public string MainSectorNameAr { get; set; } = string.Empty;
    
    /// <summary>
    /// Additional regulatory notes for this sub-sector
    /// </summary>
    [MaxLength(1000)]
    public string? RegulatoryNotes { get; set; }
    
    /// <summary>
    /// Primary regulator for this sub-sector
    /// </summary>
    [MaxLength(100)]
    public string? PrimaryRegulator { get; set; }
    
    /// <summary>
    /// Display order within the main sector
    /// </summary>
    public int DisplayOrder { get; set; }
    
    /// <summary>
    /// Is this sub-sector active/enabled
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// When was this mapping created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// The 18 main GRC sectors for KSA
/// </summary>
public static class GrcMainSectors
{
    // Original 9 sectors
    public const string BANKING = "BANKING";
    public const string HEALTHCARE = "HEALTHCARE";
    public const string GOVERNMENT = "GOVERNMENT";
    public const string TELECOM = "TELECOM";
    public const string ENERGY = "ENERGY";
    public const string RETAIL = "RETAIL";
    public const string TECHNOLOGY = "TECHNOLOGY";
    public const string INSURANCE = "INSURANCE";
    public const string EDUCATION = "EDUCATION";
    
    // New 9 sectors
    public const string TRANSPORTATION = "TRANSPORTATION";
    public const string CONSTRUCTION = "CONSTRUCTION";
    public const string MANUFACTURING = "MANUFACTURING";
    public const string REAL_ESTATE = "REAL_ESTATE";
    public const string HOSPITALITY = "HOSPITALITY";
    public const string MEDIA = "MEDIA";
    public const string AGRICULTURE = "AGRICULTURE";
    public const string MINING = "MINING";
    public const string PROFESSIONAL_SERVICES = "PROFESSIONAL_SERVICES";
    
    public static readonly Dictionary<string, (string En, string Ar)> SectorNames = new()
    {
        { BANKING, ("Banking & Financial Services", "الخدمات المصرفية والمالية") },
        { HEALTHCARE, ("Healthcare & Medical", "الرعاية الصحية والطبية") },
        { GOVERNMENT, ("Government & Public Sector", "القطاع الحكومي والعام") },
        { TELECOM, ("Telecommunications", "الاتصالات") },
        { ENERGY, ("Energy & Utilities", "الطاقة والمرافق") },
        { RETAIL, ("Retail & E-Commerce", "التجزئة والتجارة الإلكترونية") },
        { TECHNOLOGY, ("Technology & Software", "التقنية والبرمجيات") },
        { INSURANCE, ("Insurance", "التأمين") },
        { EDUCATION, ("Education", "التعليم") },
        { TRANSPORTATION, ("Transportation & Logistics", "النقل والخدمات اللوجستية") },
        { CONSTRUCTION, ("Construction & Engineering", "البناء والتشييد والهندسة") },
        { MANUFACTURING, ("Manufacturing & Industry", "الصناعات التحويلية") },
        { REAL_ESTATE, ("Real Estate", "العقارات") },
        { HOSPITALITY, ("Hospitality & Tourism", "الضيافة والسياحة") },
        { MEDIA, ("Media & Entertainment", "الإعلام والترفيه") },
        { AGRICULTURE, ("Agriculture & Food", "الزراعة والغذاء") },
        { MINING, ("Mining & Quarrying", "التعدين واستغلال المحاجر") },
        { PROFESSIONAL_SERVICES, ("Professional Services", "الخدمات المهنية") },
    };
    
    public static IEnumerable<string> All => SectorNames.Keys;
}
