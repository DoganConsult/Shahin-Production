using GrcMvc.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace GrcMvc.Data.Seeds;

/// <summary>
/// Seeds the 18 main GRC sectors for KSA
/// </summary>
public static class GrcMainSectorSeeds
{
    public static async Task SeedMainSectorsAsync(GrcDbContext context, ILogger logger)
    {
        if (await context.GrcMainSectors.AnyAsync())
        {
            logger.LogInformation("Main GRC sectors already seeded");
            return;
        }

        var sectors = GetMainSectors();
        await context.GrcMainSectors.AddRangeAsync(sectors);
        await context.SaveChangesAsync();
        
        logger.LogInformation("Seeded {Count} main GRC sectors", sectors.Count);
    }

    /// <summary>
    /// Returns all 18 main GRC sectors
    /// </summary>
    private static List<GrcMainSector> GetMainSectors()
    {
        var sectors = new List<GrcMainSector>();
        int order = 1;

        // Original 9 sectors
        sectors.Add(new GrcMainSector
        {
            SectorCode = GrcMainSectors.BANKING,
            SectorNameEn = "Banking & Financial Services",
            SectorNameAr = "الخدمات المصرفية والمالية",
            DescriptionEn = "Banks, financial institutions, and related services",
            DescriptionAr = "البنوك والمؤسسات المالية والخدمات ذات الصلة",
            PrimaryRegulator = "SAMA",
            FrameworkCount = 5,
            TotalControlCount = 844,
            DisplayOrder = order++,
            IsActive = true,
            Icon = "🏦",
            ColorCode = "#1e40af"
        });

        sectors.Add(new GrcMainSector
        {
            SectorCode = GrcMainSectors.HEALTHCARE,
            SectorNameEn = "Healthcare & Medical",
            SectorNameAr = "الرعاية الصحية والطبية",
            DescriptionEn = "Hospitals, clinics, medical facilities, and pharmaceutical companies",
            DescriptionAr = "المستشفيات والعيادات والمرافق الطبية وشركات الأدوية",
            PrimaryRegulator = "MOH",
            FrameworkCount = 4,
            TotalControlCount = 569,
            DisplayOrder = order++,
            IsActive = true,
            Icon = "🏥",
            ColorCode = "#dc2626"
        });

        sectors.Add(new GrcMainSector
        {
            SectorCode = GrcMainSectors.GOVERNMENT,
            SectorNameEn = "Government & Public Sector",
            SectorNameAr = "القطاع الحكومي والعام",
            DescriptionEn = "Government ministries, agencies, and public sector organizations",
            DescriptionAr = "الوزارات الحكومية والهيئات والمنظمات في القطاع العام",
            PrimaryRegulator = "NCA",
            FrameworkCount = 4,
            TotalControlCount = 322,
            DisplayOrder = order++,
            IsActive = true,
            Icon = "🏛️",
            ColorCode = "#059669"
        });

        sectors.Add(new GrcMainSector
        {
            SectorCode = GrcMainSectors.TELECOM,
            SectorNameEn = "Telecommunications",
            SectorNameAr = "الاتصالات",
            DescriptionEn = "Telecommunications providers, ISPs, and network operators",
            DescriptionAr = "مقدمي خدمات الاتصالات ومزودي خدمة الإنترنت ومشغلي الشبكات",
            PrimaryRegulator = "CST",
            FrameworkCount = 4,
            TotalControlCount = 369,
            DisplayOrder = order++,
            IsActive = true,
            Icon = "📡",
            ColorCode = "#7c3aed"
        });

        sectors.Add(new GrcMainSector
        {
            SectorCode = GrcMainSectors.ENERGY,
            SectorNameEn = "Energy & Utilities",
            SectorNameAr = "الطاقة والمرافق",
            DescriptionEn = "Oil, gas, electricity, water, and utility companies",
            DescriptionAr = "النفط والغاز والكهرباء والماء وشركات المرافق",
            PrimaryRegulator = "MOE",
            FrameworkCount = 4,
            TotalControlCount = 339,
            DisplayOrder = order++,
            IsActive = true,
            Icon = "⚡",
            ColorCode = "#f59e0b"
        });

        sectors.Add(new GrcMainSector
        {
            SectorCode = GrcMainSectors.RETAIL,
            SectorNameEn = "Retail & E-Commerce",
            SectorNameAr = "التجزئة والتجارة الإلكترونية",
            DescriptionEn = "Retail stores, e-commerce platforms, and consumer goods",
            DescriptionAr = "متاجر التجزئة ومنصات التجارة الإلكترونية والسلع الاستهلاكية",
            PrimaryRegulator = "MOCI",
            FrameworkCount = 4,
            TotalControlCount = 606,
            DisplayOrder = order++,
            IsActive = true,
            Icon = "🛒",
            ColorCode = "#ec4899"
        });

        sectors.Add(new GrcMainSector
        {
            SectorCode = GrcMainSectors.TECHNOLOGY,
            SectorNameEn = "Technology & Software",
            SectorNameAr = "التقنية والبرمجيات",
            DescriptionEn = "Software companies, IT services, and technology providers",
            DescriptionAr = "شركات البرمجيات وخدمات تكنولوجيا المعلومات ومقدمي الخدمات التقنية",
            PrimaryRegulator = "MCIT",
            FrameworkCount = 4,
            TotalControlCount = 538,
            DisplayOrder = order++,
            IsActive = true,
            Icon = "💻",
            ColorCode = "#3b82f6"
        });

        sectors.Add(new GrcMainSector
        {
            SectorCode = GrcMainSectors.INSURANCE,
            SectorNameEn = "Insurance",
            SectorNameAr = "التأمين",
            DescriptionEn = "Insurance companies and reinsurance providers",
            DescriptionAr = "شركات التأمين ومقدمي إعادة التأمين",
            PrimaryRegulator = "SAMA",
            FrameworkCount = 5,
            TotalControlCount = 560,
            DisplayOrder = order++,
            IsActive = true,
            Icon = "🛡️",
            ColorCode = "#10b981"
        });

        sectors.Add(new GrcMainSector
        {
            SectorCode = GrcMainSectors.EDUCATION,
            SectorNameEn = "Education",
            SectorNameAr = "التعليم",
            DescriptionEn = "Schools, universities, and educational institutions",
            DescriptionAr = "المدارس والجامعات والمؤسسات التعليمية",
            PrimaryRegulator = "MOE",
            FrameworkCount = 4,
            TotalControlCount = 397,
            DisplayOrder = order++,
            IsActive = true,
            Icon = "📚",
            ColorCode = "#8b5cf6"
        });

        // New 9 sectors
        sectors.Add(new GrcMainSector
        {
            SectorCode = GrcMainSectors.TRANSPORTATION,
            SectorNameEn = "Transportation & Logistics",
            SectorNameAr = "النقل والخدمات اللوجستية",
            DescriptionEn = "Airlines, shipping, logistics, and transportation services",
            DescriptionAr = "الخطوط الجوية والشحن والخدمات اللوجستية وخدمات النقل",
            PrimaryRegulator = "GACA",
            FrameworkCount = 3,
            TotalControlCount = 250,
            DisplayOrder = order++,
            IsActive = true,
            Icon = "🚚",
            ColorCode = "#06b6d4"
        });

        sectors.Add(new GrcMainSector
        {
            SectorCode = GrcMainSectors.CONSTRUCTION,
            SectorNameEn = "Construction & Engineering",
            SectorNameAr = "البناء والتشييد والهندسة",
            DescriptionEn = "Construction companies, engineering firms, and infrastructure development",
            DescriptionAr = "شركات البناء والشركات الهندسية وتطوير البنية التحتية",
            PrimaryRegulator = "MOMRA",
            FrameworkCount = 3,
            TotalControlCount = 280,
            DisplayOrder = order++,
            IsActive = true,
            Icon = "🏗️",
            ColorCode = "#f97316"
        });

        sectors.Add(new GrcMainSector
        {
            SectorCode = GrcMainSectors.MANUFACTURING,
            SectorNameEn = "Manufacturing & Industry",
            SectorNameAr = "الصناعات التحويلية",
            DescriptionEn = "Manufacturing companies and industrial production",
            DescriptionAr = "شركات التصنيع والإنتاج الصناعي",
            PrimaryRegulator = "MIM",
            FrameworkCount = 3,
            TotalControlCount = 320,
            DisplayOrder = order++,
            IsActive = true,
            Icon = "🏭",
            ColorCode = "#64748b"
        });

        sectors.Add(new GrcMainSector
        {
            SectorCode = GrcMainSectors.REAL_ESTATE,
            SectorNameEn = "Real Estate",
            SectorNameAr = "العقارات",
            DescriptionEn = "Real estate developers, property management, and real estate services",
            DescriptionAr = "مطورو العقارات وإدارة الممتلكات وخدمات العقارات",
            PrimaryRegulator = "REGA",
            FrameworkCount = 3,
            TotalControlCount = 200,
            DisplayOrder = order++,
            IsActive = true,
            Icon = "🏢",
            ColorCode = "#14b8a6"
        });

        sectors.Add(new GrcMainSector
        {
            SectorCode = GrcMainSectors.HOSPITALITY,
            SectorNameEn = "Hospitality & Tourism",
            SectorNameAr = "الضيافة والسياحة",
            DescriptionEn = "Hotels, restaurants, tourism services, and hospitality",
            DescriptionAr = "الفنادق والمطاعم وخدمات السياحة والضيافة",
            PrimaryRegulator = "MOT",
            FrameworkCount = 3,
            TotalControlCount = 240,
            DisplayOrder = order++,
            IsActive = true,
            Icon = "🏨",
            ColorCode = "#f43f5e"
        });

        sectors.Add(new GrcMainSector
        {
            SectorCode = GrcMainSectors.MEDIA,
            SectorNameEn = "Media & Entertainment",
            SectorNameAr = "الإعلام والترفيه",
            DescriptionEn = "Media companies, broadcasting, publishing, and entertainment",
            DescriptionAr = "شركات الإعلام والبث والنشر والترفيه",
            PrimaryRegulator = "GCAM",
            FrameworkCount = 3,
            TotalControlCount = 220,
            DisplayOrder = order++,
            IsActive = true,
            Icon = "📺",
            ColorCode = "#a855f7"
        });

        sectors.Add(new GrcMainSector
        {
            SectorCode = GrcMainSectors.AGRICULTURE,
            SectorNameEn = "Agriculture & Food",
            SectorNameAr = "الزراعة والغذاء",
            DescriptionEn = "Agriculture, food production, and related services",
            DescriptionAr = "الزراعة وإنتاج الغذاء والخدمات ذات الصلة",
            PrimaryRegulator = "MEWA",
            FrameworkCount = 3,
            TotalControlCount = 180,
            DisplayOrder = order++,
            IsActive = true,
            Icon = "🌾",
            ColorCode = "#84cc16"
        });

        sectors.Add(new GrcMainSector
        {
            SectorCode = GrcMainSectors.MINING,
            SectorNameEn = "Mining & Quarrying",
            SectorNameAr = "التعدين واستغلال المحاجر",
            DescriptionEn = "Mining companies, quarrying, and mineral extraction",
            DescriptionAr = "شركات التعدين واستغلال المحاجر واستخراج المعادن",
            PrimaryRegulator = "MIM",
            FrameworkCount = 3,
            TotalControlCount = 260,
            DisplayOrder = order++,
            IsActive = true,
            Icon = "⛏️",
            ColorCode = "#78716c"
        });

        sectors.Add(new GrcMainSector
        {
            SectorCode = GrcMainSectors.PROFESSIONAL_SERVICES,
            SectorNameEn = "Professional Services",
            SectorNameAr = "الخدمات المهنية",
            DescriptionEn = "Legal, accounting, consulting, and professional services",
            DescriptionAr = "الخدمات القانونية والمحاسبية والاستشارية والخدمات المهنية",
            PrimaryRegulator = "MOCI",
            FrameworkCount = 3,
            TotalControlCount = 210,
            DisplayOrder = order++,
            IsActive = true,
            Icon = "💼",
            ColorCode = "#6366f1"
        });

        return sectors;
    }
}
