using GrcMvc.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace GrcMvc.Data.Seeds;

/// <summary>
/// Seeds GOSI 70+ sub-sectors mapped to 18 main GRC sectors
/// Based on Saudi Arabia's National Classification of Economic Activities (ISIC Rev 4)
/// Reference: stats.gov.sa - National Classification of Economic Activities
/// </summary>
public static class GosiSectorSeeds
{
    public static async Task SeedSubSectorMappingsAsync(GrcDbContext context, ILogger logger)
    {
        if (await context.GrcSubSectorMappings.AnyAsync())
        {
            logger.LogInformation("GOSI sub-sector mappings already seeded");
            
            // Update existing mappings to link MainSectorId foreign keys
            await UpdateMainSectorForeignKeysAsync(context, logger);
            return;
        }

        var mappings = GetGosiSubSectorMappings();
        
        // Link MainSectorId foreign keys before saving
        await LinkMainSectorForeignKeysAsync(context, mappings, logger);
        
        await context.GrcSubSectorMappings.AddRangeAsync(mappings);
        await context.SaveChangesAsync();
        
        logger.LogInformation("Seeded {Count} GOSI sub-sector mappings to 18 main sectors", mappings.Count);
    }

    /// <summary>
    /// Links MainSectorId foreign keys for sub-sector mappings
    /// </summary>
    private static async Task LinkMainSectorForeignKeysAsync(
        GrcDbContext context, 
        List<GrcSubSectorMapping> mappings, 
        ILogger logger)
    {
        // Get all main sectors from database
        var mainSectors = await context.GrcMainSectors.ToListAsync();
        var sectorCodeToId = mainSectors.ToDictionary(s => s.SectorCode, s => s.Id);

        foreach (var mapping in mappings)
        {
            if (sectorCodeToId.TryGetValue(mapping.MainSectorCode, out var mainSectorId))
            {
                mapping.MainSectorId = mainSectorId;
            }
            else
            {
                logger.LogWarning("Main sector not found for code: {SectorCode}", mapping.MainSectorCode);
            }
        }
    }

    /// <summary>
    /// Updates existing sub-sector mappings to link MainSectorId foreign keys
    /// </summary>
    private static async Task UpdateMainSectorForeignKeysAsync(GrcDbContext context, ILogger logger)
    {
        var mappings = await context.GrcSubSectorMappings
            .Where(m => m.MainSectorId == null)
            .ToListAsync();

        if (!mappings.Any())
        {
            logger.LogInformation("All sub-sector mappings already have MainSectorId linked");
            return;
        }

        var mainSectors = await context.GrcMainSectors.ToListAsync();
        var sectorCodeToId = mainSectors.ToDictionary(s => s.SectorCode, s => s.Id);

        int updated = 0;
        foreach (var mapping in mappings)
        {
            if (sectorCodeToId.TryGetValue(mapping.MainSectorCode, out var mainSectorId))
            {
                mapping.MainSectorId = mainSectorId;
                updated++;
            }
        }

        if (updated > 0)
        {
            await context.SaveChangesAsync();
            logger.LogInformation("Updated {Count} sub-sector mappings with MainSectorId foreign keys", updated);
        }
    }

    /// <summary>
    /// Returns all 70+ GOSI sub-sectors mapped to 18 main GRC sectors
    /// </summary>
    private static List<GrcSubSectorMapping> GetGosiSubSectorMappings()
    {
        var mappings = new List<GrcSubSectorMapping>();
        int order = 1;

        // ================================================================
        // SECTION A: AGRICULTURE, FORESTRY AND FISHING → AGRICULTURE
        // ================================================================
        mappings.AddRange(new[]
        {
            new GrcSubSectorMapping { GosiCode = "01", IsicSection = "A", SubSectorNameEn = "Crop and animal production, hunting and related service activities", SubSectorNameAr = "إنتاج المحاصيل والثروة الحيوانية والصيد والأنشطة الخدمية ذات الصلة", MainSectorCode = GrcMainSectors.AGRICULTURE, MainSectorNameEn = "Agriculture & Food", MainSectorNameAr = "الزراعة والغذاء", PrimaryRegulator = "MEWA", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "02", IsicSection = "A", SubSectorNameEn = "Forestry and logging", SubSectorNameAr = "الحراجة وقطع الأشجار", MainSectorCode = GrcMainSectors.AGRICULTURE, MainSectorNameEn = "Agriculture & Food", MainSectorNameAr = "الزراعة والغذاء", PrimaryRegulator = "MEWA", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "03", IsicSection = "A", SubSectorNameEn = "Fishing and aquaculture", SubSectorNameAr = "صيد الأسماك وتربية الأحياء المائية", MainSectorCode = GrcMainSectors.AGRICULTURE, MainSectorNameEn = "Agriculture & Food", MainSectorNameAr = "الزراعة والغذاء", PrimaryRegulator = "MEWA", DisplayOrder = order++ },
        });

        // ================================================================
        // SECTION B: MINING AND QUARRYING → MINING
        // ================================================================
        mappings.AddRange(new[]
        {
            new GrcSubSectorMapping { GosiCode = "05", IsicSection = "B", SubSectorNameEn = "Mining of coal and lignite", SubSectorNameAr = "تعدين الفحم والليغنيت", MainSectorCode = GrcMainSectors.MINING, MainSectorNameEn = "Mining & Quarrying", MainSectorNameAr = "التعدين واستغلال المحاجر", PrimaryRegulator = "MIM", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "06", IsicSection = "B", SubSectorNameEn = "Extraction of crude petroleum and natural gas", SubSectorNameAr = "استخراج النفط الخام والغاز الطبيعي", MainSectorCode = GrcMainSectors.ENERGY, MainSectorNameEn = "Energy & Utilities", MainSectorNameAr = "الطاقة والمرافق", PrimaryRegulator = "MOE", RegulatoryNotes = "Oil & Gas falls under Energy sector due to critical infrastructure", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "07", IsicSection = "B", SubSectorNameEn = "Mining of metal ores", SubSectorNameAr = "تعدين خامات الفلزات", MainSectorCode = GrcMainSectors.MINING, MainSectorNameEn = "Mining & Quarrying", MainSectorNameAr = "التعدين واستغلال المحاجر", PrimaryRegulator = "MIM", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "08", IsicSection = "B", SubSectorNameEn = "Other mining and quarrying", SubSectorNameAr = "أنشطة التعدين واستغلال المحاجر الأخرى", MainSectorCode = GrcMainSectors.MINING, MainSectorNameEn = "Mining & Quarrying", MainSectorNameAr = "التعدين واستغلال المحاجر", PrimaryRegulator = "MIM", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "09", IsicSection = "B", SubSectorNameEn = "Mining support service activities", SubSectorNameAr = "أنشطة خدمات دعم التعدين", MainSectorCode = GrcMainSectors.MINING, MainSectorNameEn = "Mining & Quarrying", MainSectorNameAr = "التعدين واستغلال المحاجر", PrimaryRegulator = "MIM", DisplayOrder = order++ },
        });

        // ================================================================
        // SECTION C: MANUFACTURING → MANUFACTURING
        // ================================================================
        mappings.AddRange(new[]
        {
            new GrcSubSectorMapping { GosiCode = "10", IsicSection = "C", SubSectorNameEn = "Manufacture of food products", SubSectorNameAr = "صناعة المنتجات الغذائية", MainSectorCode = GrcMainSectors.MANUFACTURING, MainSectorNameEn = "Manufacturing & Industry", MainSectorNameAr = "الصناعات التحويلية", PrimaryRegulator = "SFDA", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "11", IsicSection = "C", SubSectorNameEn = "Manufacture of beverages", SubSectorNameAr = "صناعة المشروبات", MainSectorCode = GrcMainSectors.MANUFACTURING, MainSectorNameEn = "Manufacturing & Industry", MainSectorNameAr = "الصناعات التحويلية", PrimaryRegulator = "SFDA", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "12", IsicSection = "C", SubSectorNameEn = "Manufacture of tobacco products", SubSectorNameAr = "صناعة منتجات التبغ", MainSectorCode = GrcMainSectors.MANUFACTURING, MainSectorNameEn = "Manufacturing & Industry", MainSectorNameAr = "الصناعات التحويلية", PrimaryRegulator = "SFDA", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "13", IsicSection = "C", SubSectorNameEn = "Manufacture of textiles", SubSectorNameAr = "صناعة المنسوجات", MainSectorCode = GrcMainSectors.MANUFACTURING, MainSectorNameEn = "Manufacturing & Industry", MainSectorNameAr = "الصناعات التحويلية", PrimaryRegulator = "MIM", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "14", IsicSection = "C", SubSectorNameEn = "Manufacture of wearing apparel", SubSectorNameAr = "صناعة الملابس", MainSectorCode = GrcMainSectors.MANUFACTURING, MainSectorNameEn = "Manufacturing & Industry", MainSectorNameAr = "الصناعات التحويلية", PrimaryRegulator = "MIM", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "15", IsicSection = "C", SubSectorNameEn = "Manufacture of leather and related products", SubSectorNameAr = "صناعة الجلود والمنتجات ذات الصلة", MainSectorCode = GrcMainSectors.MANUFACTURING, MainSectorNameEn = "Manufacturing & Industry", MainSectorNameAr = "الصناعات التحويلية", PrimaryRegulator = "MIM", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "16", IsicSection = "C", SubSectorNameEn = "Manufacture of wood and wood products", SubSectorNameAr = "صناعة الخشب ومنتجات الخشب", MainSectorCode = GrcMainSectors.MANUFACTURING, MainSectorNameEn = "Manufacturing & Industry", MainSectorNameAr = "الصناعات التحويلية", PrimaryRegulator = "MIM", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "17", IsicSection = "C", SubSectorNameEn = "Manufacture of paper and paper products", SubSectorNameAr = "صناعة الورق ومنتجات الورق", MainSectorCode = GrcMainSectors.MANUFACTURING, MainSectorNameEn = "Manufacturing & Industry", MainSectorNameAr = "الصناعات التحويلية", PrimaryRegulator = "MIM", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "18", IsicSection = "C", SubSectorNameEn = "Printing and reproduction of recorded media", SubSectorNameAr = "الطباعة واستنساخ الوسائط المسجلة", MainSectorCode = GrcMainSectors.MEDIA, MainSectorNameEn = "Media & Entertainment", MainSectorNameAr = "الإعلام والترفيه", PrimaryRegulator = "GCAM", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "19", IsicSection = "C", SubSectorNameEn = "Manufacture of coke and refined petroleum products", SubSectorNameAr = "صناعة فحم الكوك والمنتجات النفطية المكررة", MainSectorCode = GrcMainSectors.ENERGY, MainSectorNameEn = "Energy & Utilities", MainSectorNameAr = "الطاقة والمرافق", PrimaryRegulator = "MOE", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "20", IsicSection = "C", SubSectorNameEn = "Manufacture of chemicals and chemical products", SubSectorNameAr = "صناعة المواد الكيميائية والمنتجات الكيميائية", MainSectorCode = GrcMainSectors.MANUFACTURING, MainSectorNameEn = "Manufacturing & Industry", MainSectorNameAr = "الصناعات التحويلية", PrimaryRegulator = "MIM", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "21", IsicSection = "C", SubSectorNameEn = "Manufacture of pharmaceuticals and medicinal products", SubSectorNameAr = "صناعة الأدوية والمستحضرات الطبية", MainSectorCode = GrcMainSectors.HEALTHCARE, MainSectorNameEn = "Healthcare & Medical", MainSectorNameAr = "الرعاية الصحية والطبية", PrimaryRegulator = "SFDA", RegulatoryNotes = "Pharma manufacturing falls under Healthcare due to SFDA regulation", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "22", IsicSection = "C", SubSectorNameEn = "Manufacture of rubber and plastics products", SubSectorNameAr = "صناعة منتجات المطاط واللدائن", MainSectorCode = GrcMainSectors.MANUFACTURING, MainSectorNameEn = "Manufacturing & Industry", MainSectorNameAr = "الصناعات التحويلية", PrimaryRegulator = "MIM", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "23", IsicSection = "C", SubSectorNameEn = "Manufacture of other non-metallic mineral products", SubSectorNameAr = "صناعة المنتجات المعدنية اللافلزية الأخرى", MainSectorCode = GrcMainSectors.MANUFACTURING, MainSectorNameEn = "Manufacturing & Industry", MainSectorNameAr = "الصناعات التحويلية", PrimaryRegulator = "MIM", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "24", IsicSection = "C", SubSectorNameEn = "Manufacture of basic metals", SubSectorNameAr = "صناعة الفلزات القاعدية", MainSectorCode = GrcMainSectors.MANUFACTURING, MainSectorNameEn = "Manufacturing & Industry", MainSectorNameAr = "الصناعات التحويلية", PrimaryRegulator = "MIM", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "25", IsicSection = "C", SubSectorNameEn = "Manufacture of fabricated metal products", SubSectorNameAr = "صناعة المنتجات المعدنية المصنعة", MainSectorCode = GrcMainSectors.MANUFACTURING, MainSectorNameEn = "Manufacturing & Industry", MainSectorNameAr = "الصناعات التحويلية", PrimaryRegulator = "MIM", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "26", IsicSection = "C", SubSectorNameEn = "Manufacture of computer, electronic and optical products", SubSectorNameAr = "صناعة الحاسوب والمنتجات الإلكترونية والبصرية", MainSectorCode = GrcMainSectors.TECHNOLOGY, MainSectorNameEn = "Technology & Software", MainSectorNameAr = "التقنية والبرمجيات", PrimaryRegulator = "MCIT", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "27", IsicSection = "C", SubSectorNameEn = "Manufacture of electrical equipment", SubSectorNameAr = "صناعة المعدات الكهربائية", MainSectorCode = GrcMainSectors.MANUFACTURING, MainSectorNameEn = "Manufacturing & Industry", MainSectorNameAr = "الصناعات التحويلية", PrimaryRegulator = "MIM", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "28", IsicSection = "C", SubSectorNameEn = "Manufacture of machinery and equipment n.e.c.", SubSectorNameAr = "صناعة الآلات والمعدات", MainSectorCode = GrcMainSectors.MANUFACTURING, MainSectorNameEn = "Manufacturing & Industry", MainSectorNameAr = "الصناعات التحويلية", PrimaryRegulator = "MIM", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "29", IsicSection = "C", SubSectorNameEn = "Manufacture of motor vehicles, trailers and semi-trailers", SubSectorNameAr = "صناعة المركبات ذات المحركات والمقطورات", MainSectorCode = GrcMainSectors.MANUFACTURING, MainSectorNameEn = "Manufacturing & Industry", MainSectorNameAr = "الصناعات التحويلية", PrimaryRegulator = "MIM", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "30", IsicSection = "C", SubSectorNameEn = "Manufacture of other transport equipment", SubSectorNameAr = "صناعة معدات النقل الأخرى", MainSectorCode = GrcMainSectors.MANUFACTURING, MainSectorNameEn = "Manufacturing & Industry", MainSectorNameAr = "الصناعات التحويلية", PrimaryRegulator = "MIM", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "31", IsicSection = "C", SubSectorNameEn = "Manufacture of furniture", SubSectorNameAr = "صناعة الأثاث", MainSectorCode = GrcMainSectors.MANUFACTURING, MainSectorNameEn = "Manufacturing & Industry", MainSectorNameAr = "الصناعات التحويلية", PrimaryRegulator = "MIM", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "32", IsicSection = "C", SubSectorNameEn = "Other manufacturing", SubSectorNameAr = "الصناعات التحويلية الأخرى", MainSectorCode = GrcMainSectors.MANUFACTURING, MainSectorNameEn = "Manufacturing & Industry", MainSectorNameAr = "الصناعات التحويلية", PrimaryRegulator = "MIM", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "33", IsicSection = "C", SubSectorNameEn = "Repair and installation of machinery and equipment", SubSectorNameAr = "إصلاح وتركيب الآلات والمعدات", MainSectorCode = GrcMainSectors.MANUFACTURING, MainSectorNameEn = "Manufacturing & Industry", MainSectorNameAr = "الصناعات التحويلية", PrimaryRegulator = "MIM", DisplayOrder = order++ },
        });

        // ================================================================
        // SECTION D: ELECTRICITY, GAS, STEAM → ENERGY
        // ================================================================
        mappings.Add(new GrcSubSectorMapping { GosiCode = "35", IsicSection = "D", SubSectorNameEn = "Electricity, gas, steam and air conditioning supply", SubSectorNameAr = "إمدادات الكهرباء والغاز والبخار وتكييف الهواء", MainSectorCode = GrcMainSectors.ENERGY, MainSectorNameEn = "Energy & Utilities", MainSectorNameAr = "الطاقة والمرافق", PrimaryRegulator = "ECRA", DisplayOrder = order++ });

        // ================================================================
        // SECTION E: WATER SUPPLY, SEWERAGE, WASTE → ENERGY (Utilities)
        // ================================================================
        mappings.AddRange(new[]
        {
            new GrcSubSectorMapping { GosiCode = "36", IsicSection = "E", SubSectorNameEn = "Water collection, treatment and supply", SubSectorNameAr = "جمع المياه ومعالجتها وتوزيعها", MainSectorCode = GrcMainSectors.ENERGY, MainSectorNameEn = "Energy & Utilities", MainSectorNameAr = "الطاقة والمرافق", PrimaryRegulator = "NWC", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "37", IsicSection = "E", SubSectorNameEn = "Sewerage", SubSectorNameAr = "الصرف الصحي", MainSectorCode = GrcMainSectors.ENERGY, MainSectorNameEn = "Energy & Utilities", MainSectorNameAr = "الطاقة والمرافق", PrimaryRegulator = "NWC", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "38", IsicSection = "E", SubSectorNameEn = "Waste collection, treatment and disposal activities", SubSectorNameAr = "أنشطة جمع النفايات ومعالجتها والتخلص منها", MainSectorCode = GrcMainSectors.ENERGY, MainSectorNameEn = "Energy & Utilities", MainSectorNameAr = "الطاقة والمرافق", PrimaryRegulator = "MEWA", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "39", IsicSection = "E", SubSectorNameEn = "Remediation activities and other waste management services", SubSectorNameAr = "أنشطة المعالجة وخدمات إدارة النفايات الأخرى", MainSectorCode = GrcMainSectors.ENERGY, MainSectorNameEn = "Energy & Utilities", MainSectorNameAr = "الطاقة والمرافق", PrimaryRegulator = "MEWA", DisplayOrder = order++ },
        });

        // ================================================================
        // SECTION F: CONSTRUCTION → CONSTRUCTION
        // ================================================================
        mappings.AddRange(new[]
        {
            new GrcSubSectorMapping { GosiCode = "41", IsicSection = "F", SubSectorNameEn = "Construction of buildings", SubSectorNameAr = "تشييد المباني", MainSectorCode = GrcMainSectors.CONSTRUCTION, MainSectorNameEn = "Construction & Engineering", MainSectorNameAr = "البناء والتشييد والهندسة", PrimaryRegulator = "MOMRA", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "42", IsicSection = "F", SubSectorNameEn = "Civil engineering", SubSectorNameAr = "الهندسة المدنية", MainSectorCode = GrcMainSectors.CONSTRUCTION, MainSectorNameEn = "Construction & Engineering", MainSectorNameAr = "البناء والتشييد والهندسة", PrimaryRegulator = "MOMRA", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "43", IsicSection = "F", SubSectorNameEn = "Specialized construction activities", SubSectorNameAr = "أنشطة التشييد المتخصصة", MainSectorCode = GrcMainSectors.CONSTRUCTION, MainSectorNameEn = "Construction & Engineering", MainSectorNameAr = "البناء والتشييد والهندسة", PrimaryRegulator = "MOMRA", DisplayOrder = order++ },
        });

        // ================================================================
        // SECTION G: WHOLESALE AND RETAIL TRADE → RETAIL
        // ================================================================
        mappings.AddRange(new[]
        {
            new GrcSubSectorMapping { GosiCode = "45", IsicSection = "G", SubSectorNameEn = "Wholesale and retail trade and repair of motor vehicles", SubSectorNameAr = "تجارة الجملة والتجزئة وإصلاح المركبات ذات المحركات", MainSectorCode = GrcMainSectors.RETAIL, MainSectorNameEn = "Retail & E-Commerce", MainSectorNameAr = "التجزئة والتجارة الإلكترونية", PrimaryRegulator = "MOCI", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "46", IsicSection = "G", SubSectorNameEn = "Wholesale trade, except of motor vehicles", SubSectorNameAr = "تجارة الجملة، باستثناء المركبات ذات المحركات", MainSectorCode = GrcMainSectors.RETAIL, MainSectorNameEn = "Retail & E-Commerce", MainSectorNameAr = "التجزئة والتجارة الإلكترونية", PrimaryRegulator = "MOCI", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "47", IsicSection = "G", SubSectorNameEn = "Retail trade, except of motor vehicles", SubSectorNameAr = "تجارة التجزئة، باستثناء المركبات ذات المحركات", MainSectorCode = GrcMainSectors.RETAIL, MainSectorNameEn = "Retail & E-Commerce", MainSectorNameAr = "التجزئة والتجارة الإلكترونية", PrimaryRegulator = "MOCI", DisplayOrder = order++ },
        });

        // ================================================================
        // SECTION H: TRANSPORTATION AND STORAGE → TRANSPORTATION
        // ================================================================
        mappings.AddRange(new[]
        {
            new GrcSubSectorMapping { GosiCode = "49", IsicSection = "H", SubSectorNameEn = "Land transport and transport via pipelines", SubSectorNameAr = "النقل البري والنقل عبر خطوط الأنابيب", MainSectorCode = GrcMainSectors.TRANSPORTATION, MainSectorNameEn = "Transportation & Logistics", MainSectorNameAr = "النقل والخدمات اللوجستية", PrimaryRegulator = "TGA", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "50", IsicSection = "H", SubSectorNameEn = "Water transport", SubSectorNameAr = "النقل المائي", MainSectorCode = GrcMainSectors.TRANSPORTATION, MainSectorNameEn = "Transportation & Logistics", MainSectorNameAr = "النقل والخدمات اللوجستية", PrimaryRegulator = "MAWANI", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "51", IsicSection = "H", SubSectorNameEn = "Air transport", SubSectorNameAr = "النقل الجوي", MainSectorCode = GrcMainSectors.TRANSPORTATION, MainSectorNameEn = "Transportation & Logistics", MainSectorNameAr = "النقل والخدمات اللوجستية", PrimaryRegulator = "GACA", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "52", IsicSection = "H", SubSectorNameEn = "Warehousing and support activities for transportation", SubSectorNameAr = "التخزين وأنشطة الدعم للنقل", MainSectorCode = GrcMainSectors.TRANSPORTATION, MainSectorNameEn = "Transportation & Logistics", MainSectorNameAr = "النقل والخدمات اللوجستية", PrimaryRegulator = "TGA", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "53", IsicSection = "H", SubSectorNameEn = "Postal and courier activities", SubSectorNameAr = "أنشطة البريد والبريد السريع", MainSectorCode = GrcMainSectors.TRANSPORTATION, MainSectorNameEn = "Transportation & Logistics", MainSectorNameAr = "النقل والخدمات اللوجستية", PrimaryRegulator = "CST", DisplayOrder = order++ },
        });

        // ================================================================
        // SECTION I: ACCOMMODATION AND FOOD SERVICE → HOSPITALITY
        // ================================================================
        mappings.AddRange(new[]
        {
            new GrcSubSectorMapping { GosiCode = "55", IsicSection = "I", SubSectorNameEn = "Accommodation", SubSectorNameAr = "الإقامة", MainSectorCode = GrcMainSectors.HOSPITALITY, MainSectorNameEn = "Hospitality & Tourism", MainSectorNameAr = "الضيافة والسياحة", PrimaryRegulator = "MOT", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "56", IsicSection = "I", SubSectorNameEn = "Food and beverage service activities", SubSectorNameAr = "أنشطة خدمات الطعام والمشروبات", MainSectorCode = GrcMainSectors.HOSPITALITY, MainSectorNameEn = "Hospitality & Tourism", MainSectorNameAr = "الضيافة والسياحة", PrimaryRegulator = "MOT", DisplayOrder = order++ },
        });

        // ================================================================
        // SECTION J: INFORMATION AND COMMUNICATION → TELECOM/TECHNOLOGY/MEDIA
        // ================================================================
        mappings.AddRange(new[]
        {
            new GrcSubSectorMapping { GosiCode = "58", IsicSection = "J", SubSectorNameEn = "Publishing activities", SubSectorNameAr = "أنشطة النشر", MainSectorCode = GrcMainSectors.MEDIA, MainSectorNameEn = "Media & Entertainment", MainSectorNameAr = "الإعلام والترفيه", PrimaryRegulator = "GCAM", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "59", IsicSection = "J", SubSectorNameEn = "Motion picture, video and television programme production", SubSectorNameAr = "إنتاج الأفلام والفيديو والبرامج التلفزيونية", MainSectorCode = GrcMainSectors.MEDIA, MainSectorNameEn = "Media & Entertainment", MainSectorNameAr = "الإعلام والترفيه", PrimaryRegulator = "GCAM", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "60", IsicSection = "J", SubSectorNameEn = "Programming and broadcasting activities", SubSectorNameAr = "أنشطة البرمجة والبث", MainSectorCode = GrcMainSectors.MEDIA, MainSectorNameEn = "Media & Entertainment", MainSectorNameAr = "الإعلام والترفيه", PrimaryRegulator = "GCAM", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "61", IsicSection = "J", SubSectorNameEn = "Telecommunications", SubSectorNameAr = "الاتصالات", MainSectorCode = GrcMainSectors.TELECOM, MainSectorNameEn = "Telecommunications", MainSectorNameAr = "الاتصالات", PrimaryRegulator = "CST", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "62", IsicSection = "J", SubSectorNameEn = "Computer programming, consultancy and related activities", SubSectorNameAr = "برمجة الحاسوب والاستشارات والأنشطة ذات الصلة", MainSectorCode = GrcMainSectors.TECHNOLOGY, MainSectorNameEn = "Technology & Software", MainSectorNameAr = "التقنية والبرمجيات", PrimaryRegulator = "MCIT", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "63", IsicSection = "J", SubSectorNameEn = "Information service activities", SubSectorNameAr = "أنشطة خدمات المعلومات", MainSectorCode = GrcMainSectors.TECHNOLOGY, MainSectorNameEn = "Technology & Software", MainSectorNameAr = "التقنية والبرمجيات", PrimaryRegulator = "MCIT", DisplayOrder = order++ },
        });

        // ================================================================
        // SECTION K: FINANCIAL AND INSURANCE → BANKING/INSURANCE
        // ================================================================
        mappings.AddRange(new[]
        {
            new GrcSubSectorMapping { GosiCode = "64", IsicSection = "K", SubSectorNameEn = "Financial service activities, except insurance and pension funding", SubSectorNameAr = "أنشطة الخدمات المالية، باستثناء التأمين وتمويل المعاشات", MainSectorCode = GrcMainSectors.BANKING, MainSectorNameEn = "Banking & Financial Services", MainSectorNameAr = "الخدمات المصرفية والمالية", PrimaryRegulator = "SAMA", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "65", IsicSection = "K", SubSectorNameEn = "Insurance, reinsurance and pension funding", SubSectorNameAr = "التأمين وإعادة التأمين وتمويل المعاشات", MainSectorCode = GrcMainSectors.INSURANCE, MainSectorNameEn = "Insurance", MainSectorNameAr = "التأمين", PrimaryRegulator = "SAMA", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "66", IsicSection = "K", SubSectorNameEn = "Activities auxiliary to financial service and insurance", SubSectorNameAr = "الأنشطة المساعدة للخدمات المالية والتأمين", MainSectorCode = GrcMainSectors.BANKING, MainSectorNameEn = "Banking & Financial Services", MainSectorNameAr = "الخدمات المصرفية والمالية", PrimaryRegulator = "SAMA", DisplayOrder = order++ },
        });

        // ================================================================
        // SECTION L: REAL ESTATE → REAL_ESTATE
        // ================================================================
        mappings.Add(new GrcSubSectorMapping { GosiCode = "68", IsicSection = "L", SubSectorNameEn = "Real estate activities", SubSectorNameAr = "الأنشطة العقارية", MainSectorCode = GrcMainSectors.REAL_ESTATE, MainSectorNameEn = "Real Estate", MainSectorNameAr = "العقارات", PrimaryRegulator = "REGA", DisplayOrder = order++ });

        // ================================================================
        // SECTION M: PROFESSIONAL, SCIENTIFIC, TECHNICAL → PROFESSIONAL_SERVICES
        // ================================================================
        mappings.AddRange(new[]
        {
            new GrcSubSectorMapping { GosiCode = "69", IsicSection = "M", SubSectorNameEn = "Legal and accounting activities", SubSectorNameAr = "الأنشطة القانونية والمحاسبية", MainSectorCode = GrcMainSectors.PROFESSIONAL_SERVICES, MainSectorNameEn = "Professional Services", MainSectorNameAr = "الخدمات المهنية", PrimaryRegulator = "SOCPA", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "70", IsicSection = "M", SubSectorNameEn = "Activities of head offices; management consultancy", SubSectorNameAr = "أنشطة المكاتب الرئيسية؛ الاستشارات الإدارية", MainSectorCode = GrcMainSectors.PROFESSIONAL_SERVICES, MainSectorNameEn = "Professional Services", MainSectorNameAr = "الخدمات المهنية", PrimaryRegulator = "MOCI", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "71", IsicSection = "M", SubSectorNameEn = "Architectural and engineering activities; technical testing", SubSectorNameAr = "الأنشطة المعمارية والهندسية؛ الاختبارات الفنية", MainSectorCode = GrcMainSectors.PROFESSIONAL_SERVICES, MainSectorNameEn = "Professional Services", MainSectorNameAr = "الخدمات المهنية", PrimaryRegulator = "SCE", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "72", IsicSection = "M", SubSectorNameEn = "Scientific research and development", SubSectorNameAr = "البحث العلمي والتطوير", MainSectorCode = GrcMainSectors.PROFESSIONAL_SERVICES, MainSectorNameEn = "Professional Services", MainSectorNameAr = "الخدمات المهنية", PrimaryRegulator = "MOHE", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "73", IsicSection = "M", SubSectorNameEn = "Advertising and market research", SubSectorNameAr = "الإعلان وأبحاث السوق", MainSectorCode = GrcMainSectors.MEDIA, MainSectorNameEn = "Media & Entertainment", MainSectorNameAr = "الإعلام والترفيه", PrimaryRegulator = "GCAM", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "74", IsicSection = "M", SubSectorNameEn = "Other professional, scientific and technical activities", SubSectorNameAr = "الأنشطة المهنية والعلمية والتقنية الأخرى", MainSectorCode = GrcMainSectors.PROFESSIONAL_SERVICES, MainSectorNameEn = "Professional Services", MainSectorNameAr = "الخدمات المهنية", PrimaryRegulator = "MOCI", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "75", IsicSection = "M", SubSectorNameEn = "Veterinary activities", SubSectorNameAr = "الأنشطة البيطرية", MainSectorCode = GrcMainSectors.AGRICULTURE, MainSectorNameEn = "Agriculture & Food", MainSectorNameAr = "الزراعة والغذاء", PrimaryRegulator = "MEWA", DisplayOrder = order++ },
        });

        // ================================================================
        // SECTION N: ADMINISTRATIVE AND SUPPORT SERVICE → PROFESSIONAL_SERVICES
        // ================================================================
        mappings.AddRange(new[]
        {
            new GrcSubSectorMapping { GosiCode = "77", IsicSection = "N", SubSectorNameEn = "Rental and leasing activities", SubSectorNameAr = "أنشطة التأجير والإيجار", MainSectorCode = GrcMainSectors.PROFESSIONAL_SERVICES, MainSectorNameEn = "Professional Services", MainSectorNameAr = "الخدمات المهنية", PrimaryRegulator = "MOCI", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "78", IsicSection = "N", SubSectorNameEn = "Employment activities", SubSectorNameAr = "أنشطة التوظيف", MainSectorCode = GrcMainSectors.PROFESSIONAL_SERVICES, MainSectorNameEn = "Professional Services", MainSectorNameAr = "الخدمات المهنية", PrimaryRegulator = "HRSD", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "79", IsicSection = "N", SubSectorNameEn = "Travel agency, tour operator and reservation services", SubSectorNameAr = "وكالات السفر ومنظمي الرحلات وخدمات الحجز", MainSectorCode = GrcMainSectors.HOSPITALITY, MainSectorNameEn = "Hospitality & Tourism", MainSectorNameAr = "الضيافة والسياحة", PrimaryRegulator = "MOT", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "80", IsicSection = "N", SubSectorNameEn = "Security and investigation activities", SubSectorNameAr = "أنشطة الأمن والتحقيق", MainSectorCode = GrcMainSectors.PROFESSIONAL_SERVICES, MainSectorNameEn = "Professional Services", MainSectorNameAr = "الخدمات المهنية", PrimaryRegulator = "MOI", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "81", IsicSection = "N", SubSectorNameEn = "Services to buildings and landscape activities", SubSectorNameAr = "خدمات المباني وأنشطة تنسيق الحدائق", MainSectorCode = GrcMainSectors.PROFESSIONAL_SERVICES, MainSectorNameEn = "Professional Services", MainSectorNameAr = "الخدمات المهنية", PrimaryRegulator = "MOMRA", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "82", IsicSection = "N", SubSectorNameEn = "Office administrative, office support and business support", SubSectorNameAr = "الدعم الإداري والمكتبي ودعم الأعمال", MainSectorCode = GrcMainSectors.PROFESSIONAL_SERVICES, MainSectorNameEn = "Professional Services", MainSectorNameAr = "الخدمات المهنية", PrimaryRegulator = "MOCI", DisplayOrder = order++ },
        });

        // ================================================================
        // SECTION O: PUBLIC ADMINISTRATION AND DEFENCE → GOVERNMENT
        // ================================================================
        mappings.Add(new GrcSubSectorMapping { GosiCode = "84", IsicSection = "O", SubSectorNameEn = "Public administration and defence; compulsory social security", SubSectorNameAr = "الإدارة العامة والدفاع؛ الضمان الاجتماعي الإلزامي", MainSectorCode = GrcMainSectors.GOVERNMENT, MainSectorNameEn = "Government & Public Sector", MainSectorNameAr = "القطاع الحكومي والعام", PrimaryRegulator = "NCA", DisplayOrder = order++ });

        // ================================================================
        // SECTION P: EDUCATION → EDUCATION
        // ================================================================
        mappings.Add(new GrcSubSectorMapping { GosiCode = "85", IsicSection = "P", SubSectorNameEn = "Education", SubSectorNameAr = "التعليم", MainSectorCode = GrcMainSectors.EDUCATION, MainSectorNameEn = "Education", MainSectorNameAr = "التعليم", PrimaryRegulator = "MOE", DisplayOrder = order++ });

        // ================================================================
        // SECTION Q: HUMAN HEALTH AND SOCIAL WORK → HEALTHCARE
        // ================================================================
        mappings.AddRange(new[]
        {
            new GrcSubSectorMapping { GosiCode = "86", IsicSection = "Q", SubSectorNameEn = "Human health activities", SubSectorNameAr = "أنشطة صحة الإنسان", MainSectorCode = GrcMainSectors.HEALTHCARE, MainSectorNameEn = "Healthcare & Medical", MainSectorNameAr = "الرعاية الصحية والطبية", PrimaryRegulator = "MOH", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "87", IsicSection = "Q", SubSectorNameEn = "Residential care activities", SubSectorNameAr = "أنشطة الرعاية السكنية", MainSectorCode = GrcMainSectors.HEALTHCARE, MainSectorNameEn = "Healthcare & Medical", MainSectorNameAr = "الرعاية الصحية والطبية", PrimaryRegulator = "MOH", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "88", IsicSection = "Q", SubSectorNameEn = "Social work activities without accommodation", SubSectorNameAr = "أنشطة العمل الاجتماعي بدون إقامة", MainSectorCode = GrcMainSectors.HEALTHCARE, MainSectorNameEn = "Healthcare & Medical", MainSectorNameAr = "الرعاية الصحية والطبية", PrimaryRegulator = "MLSD", DisplayOrder = order++ },
        });

        // ================================================================
        // SECTION R: ARTS, ENTERTAINMENT AND RECREATION → MEDIA
        // ================================================================
        mappings.AddRange(new[]
        {
            new GrcSubSectorMapping { GosiCode = "90", IsicSection = "R", SubSectorNameEn = "Creative, arts and entertainment activities", SubSectorNameAr = "الأنشطة الإبداعية والفنية والترفيهية", MainSectorCode = GrcMainSectors.MEDIA, MainSectorNameEn = "Media & Entertainment", MainSectorNameAr = "الإعلام والترفيه", PrimaryRegulator = "GEA", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "91", IsicSection = "R", SubSectorNameEn = "Libraries, archives, museums and other cultural activities", SubSectorNameAr = "المكتبات والأرشيفات والمتاحف والأنشطة الثقافية الأخرى", MainSectorCode = GrcMainSectors.MEDIA, MainSectorNameEn = "Media & Entertainment", MainSectorNameAr = "الإعلام والترفيه", PrimaryRegulator = "MOC", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "92", IsicSection = "R", SubSectorNameEn = "Gambling and betting activities", SubSectorNameAr = "أنشطة المقامرة والمراهنات", MainSectorCode = GrcMainSectors.MEDIA, MainSectorNameEn = "Media & Entertainment", MainSectorNameAr = "الإعلام والترفيه", PrimaryRegulator = "GEA", RegulatoryNotes = "Not permitted in KSA", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "93", IsicSection = "R", SubSectorNameEn = "Sports activities and amusement and recreation activities", SubSectorNameAr = "الأنشطة الرياضية وأنشطة التسلية والترفيه", MainSectorCode = GrcMainSectors.MEDIA, MainSectorNameEn = "Media & Entertainment", MainSectorNameAr = "الإعلام والترفيه", PrimaryRegulator = "GEA", DisplayOrder = order++ },
        });

        // ================================================================
        // SECTION S: OTHER SERVICE ACTIVITIES → PROFESSIONAL_SERVICES
        // ================================================================
        mappings.AddRange(new[]
        {
            new GrcSubSectorMapping { GosiCode = "94", IsicSection = "S", SubSectorNameEn = "Activities of membership organizations", SubSectorNameAr = "أنشطة منظمات العضوية", MainSectorCode = GrcMainSectors.PROFESSIONAL_SERVICES, MainSectorNameEn = "Professional Services", MainSectorNameAr = "الخدمات المهنية", PrimaryRegulator = "MLSD", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "95", IsicSection = "S", SubSectorNameEn = "Repair of computers and personal and household goods", SubSectorNameAr = "إصلاح الحاسوب والسلع الشخصية والمنزلية", MainSectorCode = GrcMainSectors.RETAIL, MainSectorNameEn = "Retail & E-Commerce", MainSectorNameAr = "التجزئة والتجارة الإلكترونية", PrimaryRegulator = "MOCI", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "96", IsicSection = "S", SubSectorNameEn = "Other personal service activities", SubSectorNameAr = "أنشطة الخدمات الشخصية الأخرى", MainSectorCode = GrcMainSectors.PROFESSIONAL_SERVICES, MainSectorNameEn = "Professional Services", MainSectorNameAr = "الخدمات المهنية", PrimaryRegulator = "MOCI", DisplayOrder = order++ },
        });

        // ================================================================
        // SECTION T: ACTIVITIES OF HOUSEHOLDS → PROFESSIONAL_SERVICES
        // ================================================================
        mappings.AddRange(new[]
        {
            new GrcSubSectorMapping { GosiCode = "97", IsicSection = "T", SubSectorNameEn = "Activities of households as employers of domestic personnel", SubSectorNameAr = "أنشطة الأسر المعيشية كأصحاب عمل للعمالة المنزلية", MainSectorCode = GrcMainSectors.PROFESSIONAL_SERVICES, MainSectorNameEn = "Professional Services", MainSectorNameAr = "الخدمات المهنية", PrimaryRegulator = "HRSD", DisplayOrder = order++ },
            new GrcSubSectorMapping { GosiCode = "98", IsicSection = "T", SubSectorNameEn = "Undifferentiated goods and services producing activities", SubSectorNameAr = "أنشطة إنتاج السلع والخدمات غير المتمايزة", MainSectorCode = GrcMainSectors.PROFESSIONAL_SERVICES, MainSectorNameEn = "Professional Services", MainSectorNameAr = "الخدمات المهنية", PrimaryRegulator = "MOCI", DisplayOrder = order++ },
        });

        // ================================================================
        // SECTION U: EXTRATERRITORIAL ORGANIZATIONS → GOVERNMENT
        // ================================================================
        mappings.Add(new GrcSubSectorMapping { GosiCode = "99", IsicSection = "U", SubSectorNameEn = "Activities of extraterritorial organizations and bodies", SubSectorNameAr = "أنشطة المنظمات والهيئات الأجنبية", MainSectorCode = GrcMainSectors.GOVERNMENT, MainSectorNameEn = "Government & Public Sector", MainSectorNameAr = "القطاع الحكومي والعام", PrimaryRegulator = "MOFA", DisplayOrder = order++ });

        return mappings;
    }
}
