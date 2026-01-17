using GrcMvc.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Data.Seeds
{
    /// <summary>
    /// Seeds all GRC lookup/reference tables with world-class standards
    /// Focused on KSA/GCC with international coverage
    /// </summary>
    public static class GrcLookupSeeds
    {
        public static async Task SeedAllAsync(GrcDbContext context, ILogger? logger = null)
        {
            logger?.LogInformation("Seeding GRC Lookup Tables...");

            await SeedCountriesAsync(context);
            await SeedSectorsAsync(context);
            await SeedOrganizationTypesAsync(context);
            await SeedRegulatorsAsync(context);
            await SeedFrameworksAsync(context);
            await SeedOrganizationSizesAsync(context);
            await SeedMaturityLevelsAsync(context);
            await SeedDataTypesAsync(context);
            await SeedHostingModelsAsync(context);
            await SeedCloudProvidersAsync(context);

            logger?.LogInformation("GRC Lookup Tables seeded successfully.");
        }

        private static async Task SeedCountriesAsync(GrcDbContext context)
        {
            if (await context.LookupCountries.AnyAsync()) return;

            var countries = new List<LookupCountry>
            {
                // GCC Countries (Priority)
                new() { Code = "SA", Iso2Code = "SA", Iso3Code = "SAU", NameEn = "Saudi Arabia", NameAr = "المملكة العربية السعودية", PhoneCode = "+966", Currency = "SAR", Region = "GCC", IsGccCountry = true, RequiresDataLocalization = true, SortOrder = 1 },
                new() { Code = "AE", Iso2Code = "AE", Iso3Code = "ARE", NameEn = "United Arab Emirates", NameAr = "الإمارات العربية المتحدة", PhoneCode = "+971", Currency = "AED", Region = "GCC", IsGccCountry = true, RequiresDataLocalization = true, SortOrder = 2 },
                new() { Code = "QA", Iso2Code = "QA", Iso3Code = "QAT", NameEn = "Qatar", NameAr = "قطر", PhoneCode = "+974", Currency = "QAR", Region = "GCC", IsGccCountry = true, SortOrder = 3 },
                new() { Code = "KW", Iso2Code = "KW", Iso3Code = "KWT", NameEn = "Kuwait", NameAr = "الكويت", PhoneCode = "+965", Currency = "KWD", Region = "GCC", IsGccCountry = true, SortOrder = 4 },
                new() { Code = "BH", Iso2Code = "BH", Iso3Code = "BHR", NameEn = "Bahrain", NameAr = "البحرين", PhoneCode = "+973", Currency = "BHD", Region = "GCC", IsGccCountry = true, SortOrder = 5 },
                new() { Code = "OM", Iso2Code = "OM", Iso3Code = "OMN", NameEn = "Oman", NameAr = "عُمان", PhoneCode = "+968", Currency = "OMR", Region = "GCC", IsGccCountry = true, SortOrder = 6 },

                // MENA Region
                new() { Code = "EG", Iso2Code = "EG", Iso3Code = "EGY", NameEn = "Egypt", NameAr = "مصر", PhoneCode = "+20", Currency = "EGP", Region = "MENA", SortOrder = 10 },
                new() { Code = "JO", Iso2Code = "JO", Iso3Code = "JOR", NameEn = "Jordan", NameAr = "الأردن", PhoneCode = "+962", Currency = "JOD", Region = "MENA", SortOrder = 11 },
                new() { Code = "LB", Iso2Code = "LB", Iso3Code = "LBN", NameEn = "Lebanon", NameAr = "لبنان", PhoneCode = "+961", Currency = "LBP", Region = "MENA", SortOrder = 12 },
                new() { Code = "IQ", Iso2Code = "IQ", Iso3Code = "IRQ", NameEn = "Iraq", NameAr = "العراق", PhoneCode = "+964", Currency = "IQD", Region = "MENA", SortOrder = 13 },
                new() { Code = "SY", Iso2Code = "SY", Iso3Code = "SYR", NameEn = "Syria", NameAr = "سوريا", PhoneCode = "+963", Currency = "SYP", Region = "MENA", SortOrder = 14 },
                new() { Code = "YE", Iso2Code = "YE", Iso3Code = "YEM", NameEn = "Yemen", NameAr = "اليمن", PhoneCode = "+967", Currency = "YER", Region = "MENA", SortOrder = 15 },
                new() { Code = "PS", Iso2Code = "PS", Iso3Code = "PSE", NameEn = "Palestine", NameAr = "فلسطين", PhoneCode = "+970", Currency = "ILS", Region = "MENA", SortOrder = 16 },
                new() { Code = "MA", Iso2Code = "MA", Iso3Code = "MAR", NameEn = "Morocco", NameAr = "المغرب", PhoneCode = "+212", Currency = "MAD", Region = "MENA", SortOrder = 17 },
                new() { Code = "TN", Iso2Code = "TN", Iso3Code = "TUN", NameEn = "Tunisia", NameAr = "تونس", PhoneCode = "+216", Currency = "TND", Region = "MENA", SortOrder = 18 },
                new() { Code = "DZ", Iso2Code = "DZ", Iso3Code = "DZA", NameEn = "Algeria", NameAr = "الجزائر", PhoneCode = "+213", Currency = "DZD", Region = "MENA", SortOrder = 19 },
                new() { Code = "LY", Iso2Code = "LY", Iso3Code = "LBY", NameEn = "Libya", NameAr = "ليبيا", PhoneCode = "+218", Currency = "LYD", Region = "MENA", SortOrder = 20 },
                new() { Code = "SD", Iso2Code = "SD", Iso3Code = "SDN", NameEn = "Sudan", NameAr = "السودان", PhoneCode = "+249", Currency = "SDG", Region = "MENA", SortOrder = 21 },

                // Major International
                new() { Code = "US", Iso2Code = "US", Iso3Code = "USA", NameEn = "United States", NameAr = "الولايات المتحدة", PhoneCode = "+1", Currency = "USD", Region = "Americas", SortOrder = 50 },
                new() { Code = "GB", Iso2Code = "GB", Iso3Code = "GBR", NameEn = "United Kingdom", NameAr = "المملكة المتحدة", PhoneCode = "+44", Currency = "GBP", Region = "Europe", SortOrder = 51 },
                new() { Code = "DE", Iso2Code = "DE", Iso3Code = "DEU", NameEn = "Germany", NameAr = "ألمانيا", PhoneCode = "+49", Currency = "EUR", Region = "Europe", RequiresDataLocalization = true, SortOrder = 52 },
                new() { Code = "FR", Iso2Code = "FR", Iso3Code = "FRA", NameEn = "France", NameAr = "فرنسا", PhoneCode = "+33", Currency = "EUR", Region = "Europe", SortOrder = 53 },
                new() { Code = "CN", Iso2Code = "CN", Iso3Code = "CHN", NameEn = "China", NameAr = "الصين", PhoneCode = "+86", Currency = "CNY", Region = "Asia", RequiresDataLocalization = true, SortOrder = 54 },
                new() { Code = "JP", Iso2Code = "JP", Iso3Code = "JPN", NameEn = "Japan", NameAr = "اليابان", PhoneCode = "+81", Currency = "JPY", Region = "Asia", SortOrder = 55 },
                new() { Code = "IN", Iso2Code = "IN", Iso3Code = "IND", NameEn = "India", NameAr = "الهند", PhoneCode = "+91", Currency = "INR", Region = "Asia", SortOrder = 56 },
                new() { Code = "SG", Iso2Code = "SG", Iso3Code = "SGP", NameEn = "Singapore", NameAr = "سنغافورة", PhoneCode = "+65", Currency = "SGD", Region = "Asia", SortOrder = 57 },
                new() { Code = "AU", Iso2Code = "AU", Iso3Code = "AUS", NameEn = "Australia", NameAr = "أستراليا", PhoneCode = "+61", Currency = "AUD", Region = "Oceania", SortOrder = 58 },
                new() { Code = "CA", Iso2Code = "CA", Iso3Code = "CAN", NameEn = "Canada", NameAr = "كندا", PhoneCode = "+1", Currency = "CAD", Region = "Americas", SortOrder = 59 },
                new() { Code = "TR", Iso2Code = "TR", Iso3Code = "TUR", NameEn = "Turkey", NameAr = "تركيا", PhoneCode = "+90", Currency = "TRY", Region = "Europe", SortOrder = 60 },
                new() { Code = "PK", Iso2Code = "PK", Iso3Code = "PAK", NameEn = "Pakistan", NameAr = "باكستان", PhoneCode = "+92", Currency = "PKR", Region = "Asia", SortOrder = 61 },
                new() { Code = "BD", Iso2Code = "BD", Iso3Code = "BGD", NameEn = "Bangladesh", NameAr = "بنغلاديش", PhoneCode = "+880", Currency = "BDT", Region = "Asia", SortOrder = 62 },
                new() { Code = "PH", Iso2Code = "PH", Iso3Code = "PHL", NameEn = "Philippines", NameAr = "الفلبين", PhoneCode = "+63", Currency = "PHP", Region = "Asia", SortOrder = 63 },
                new() { Code = "ID", Iso2Code = "ID", Iso3Code = "IDN", NameEn = "Indonesia", NameAr = "إندونيسيا", PhoneCode = "+62", Currency = "IDR", Region = "Asia", SortOrder = 64 },
                new() { Code = "MY", Iso2Code = "MY", Iso3Code = "MYS", NameEn = "Malaysia", NameAr = "ماليزيا", PhoneCode = "+60", Currency = "MYR", Region = "Asia", SortOrder = 65 },
            };

            context.LookupCountries.AddRange(countries);
            await context.SaveChangesAsync();
        }

        private static async Task SeedSectorsAsync(GrcDbContext context)
        {
            if (await context.LookupSectors.AnyAsync()) return;

            var sectors = new List<LookupSector>
            {
                // Main Sectors with KSA Regulators
                new() { Code = "BANKING", NameEn = "Banking & Financial Services", NameAr = "الخدمات المصرفية والمالية", PrimaryRegulatorCode = "SAMA", IsCriticalInfrastructure = true, RequiresStricterCompliance = true, RecommendedFrameworks = "[\"SAMA-CSF\",\"NCA-ECC\",\"PDPL\",\"PCI-DSS\"]", SortOrder = 1 },
                new() { Code = "INSURANCE", NameEn = "Insurance", NameAr = "التأمين", PrimaryRegulatorCode = "SAMA", RequiresStricterCompliance = true, RecommendedFrameworks = "[\"SAMA-CSF\",\"NCA-ECC\",\"PDPL\"]", SortOrder = 2 },
                new() { Code = "FINTECH", NameEn = "FinTech & Digital Finance", NameAr = "التقنية المالية", PrimaryRegulatorCode = "SAMA", RequiresStricterCompliance = true, RecommendedFrameworks = "[\"SAMA-CSF\",\"NCA-ECC\",\"PDPL\",\"PCI-DSS\"]", SortOrder = 3 },
                new() { Code = "CAPITAL_MARKETS", NameEn = "Capital Markets & Securities", NameAr = "أسواق المال والأوراق المالية", PrimaryRegulatorCode = "CMA", RequiresStricterCompliance = true, RecommendedFrameworks = "[\"CMA-REG\",\"NCA-ECC\",\"PDPL\"]", SortOrder = 4 },

                new() { Code = "HEALTHCARE", NameEn = "Healthcare & Medical", NameAr = "الرعاية الصحية والطبية", PrimaryRegulatorCode = "MOH", IsCriticalInfrastructure = true, RequiresStricterCompliance = true, RecommendedFrameworks = "[\"NCA-ECC\",\"PDPL\",\"SFDA\",\"MOH-HIS\"]", SortOrder = 10 },
                new() { Code = "PHARMA", NameEn = "Pharmaceuticals", NameAr = "صناعة الأدوية", PrimaryRegulatorCode = "SFDA", RecommendedFrameworks = "[\"SFDA\",\"NCA-ECC\",\"PDPL\"]", SortOrder = 11 },

                new() { Code = "GOVERNMENT", NameEn = "Government & Public Sector", NameAr = "القطاع الحكومي والعام", PrimaryRegulatorCode = "NCA", IsCriticalInfrastructure = true, RequiresStricterCompliance = true, RecommendedFrameworks = "[\"NCA-ECC\",\"NCA-CSCC\",\"DGA-CLOUD\",\"PDPL\",\"NDMO\"]", SortOrder = 20 },
                new() { Code = "DEFENSE", NameEn = "Defense & Security", NameAr = "الدفاع والأمن", PrimaryRegulatorCode = "NCA", IsCriticalInfrastructure = true, RequiresStricterCompliance = true, RecommendedFrameworks = "[\"NCA-ECC\",\"NCA-CSCC\",\"NCA-OT\"]", SortOrder = 21 },

                new() { Code = "TELECOM", NameEn = "Telecommunications", NameAr = "الاتصالات", PrimaryRegulatorCode = "CST", IsCriticalInfrastructure = true, RequiresStricterCompliance = true, RecommendedFrameworks = "[\"CST-CRF\",\"NCA-ECC\",\"NCA-CSCC\",\"PDPL\"]", SortOrder = 30 },
                new() { Code = "IT_SERVICES", NameEn = "IT Services & Consulting", NameAr = "خدمات تقنية المعلومات والاستشارات", PrimaryRegulatorCode = "MCIT", RecommendedFrameworks = "[\"NCA-ECC\",\"PDPL\",\"ISO27001\"]", SortOrder = 31 },

                new() { Code = "ENERGY_OIL", NameEn = "Oil & Gas", NameAr = "النفط والغاز", PrimaryRegulatorCode = "MOE", IsCriticalInfrastructure = true, RequiresStricterCompliance = true, RecommendedFrameworks = "[\"NCA-ECC\",\"NCA-CSCC\",\"NCA-OT\",\"HCIS\"]", SortOrder = 40 },
                new() { Code = "ENERGY_POWER", NameEn = "Power & Utilities", NameAr = "الكهرباء والمرافق", PrimaryRegulatorCode = "ECRA", IsCriticalInfrastructure = true, RequiresStricterCompliance = true, RecommendedFrameworks = "[\"NCA-ECC\",\"NCA-OT\",\"ECRA-REG\"]", SortOrder = 41 },
                new() { Code = "WATER", NameEn = "Water & Environment", NameAr = "المياه والبيئة", PrimaryRegulatorCode = "MEWA", IsCriticalInfrastructure = true, RecommendedFrameworks = "[\"NCA-ECC\",\"NCA-OT\"]", SortOrder = 42 },

                new() { Code = "RETAIL", NameEn = "Retail & E-Commerce", NameAr = "التجزئة والتجارة الإلكترونية", PrimaryRegulatorCode = "MOCI", RecommendedFrameworks = "[\"PDPL\",\"NCA-ECC\",\"PCI-DSS\",\"MOCI-ECOM\"]", SortOrder = 50 },
                new() { Code = "LOGISTICS", NameEn = "Logistics & Supply Chain", NameAr = "الخدمات اللوجستية وسلسلة التوريد", PrimaryRegulatorCode = "MOT", RecommendedFrameworks = "[\"NCA-ECC\",\"PDPL\"]", SortOrder = 51 },

                new() { Code = "EDUCATION", NameEn = "Education", NameAr = "التعليم", PrimaryRegulatorCode = "MOE", RecommendedFrameworks = "[\"NCA-ECC\",\"PDPL\"]", SortOrder = 60 },
                new() { Code = "HIGHER_ED", NameEn = "Higher Education", NameAr = "التعليم العالي", PrimaryRegulatorCode = "MOHE", RecommendedFrameworks = "[\"NCA-ECC\",\"PDPL\"]", SortOrder = 61 },

                new() { Code = "REAL_ESTATE", NameEn = "Real Estate", NameAr = "العقارات", PrimaryRegulatorCode = "REGA", RecommendedFrameworks = "[\"PDPL\",\"NCA-ECC\"]", SortOrder = 70 },
                new() { Code = "CONSTRUCTION", NameEn = "Construction & Engineering", NameAr = "البناء والهندسة", PrimaryRegulatorCode = "MOMRA", RecommendedFrameworks = "[\"NCA-ECC\",\"PDPL\"]", SortOrder = 71 },

                new() { Code = "HOSPITALITY", NameEn = "Hospitality & Tourism", NameAr = "الضيافة والسياحة", PrimaryRegulatorCode = "MTA", RecommendedFrameworks = "[\"PDPL\",\"NCA-ECC\",\"PCI-DSS\"]", SortOrder = 80 },
                new() { Code = "ENTERTAINMENT", NameEn = "Entertainment & Media", NameAr = "الترفيه والإعلام", PrimaryRegulatorCode = "GEA", RecommendedFrameworks = "[\"PDPL\",\"NCA-ECC\"]", SortOrder = 81 },

                new() { Code = "MANUFACTURING", NameEn = "Manufacturing & Industry", NameAr = "الصناعات التحويلية", PrimaryRegulatorCode = "MODON", RecommendedFrameworks = "[\"NCA-ECC\",\"NCA-OT\",\"PDPL\"]", SortOrder = 90 },
                new() { Code = "MINING", NameEn = "Mining & Quarrying", NameAr = "التعدين واستغلال المحاجر", PrimaryRegulatorCode = "MIM", IsCriticalInfrastructure = true, RecommendedFrameworks = "[\"NCA-ECC\",\"NCA-OT\"]", SortOrder = 91 },

                new() { Code = "AVIATION", NameEn = "Aviation & Airports", NameAr = "الطيران والمطارات", PrimaryRegulatorCode = "GACA", IsCriticalInfrastructure = true, RequiresStricterCompliance = true, RecommendedFrameworks = "[\"NCA-ECC\",\"NCA-CSCC\",\"GACA-SEC\"]", SortOrder = 100 },
                new() { Code = "MARITIME", NameEn = "Maritime & Ports", NameAr = "النقل البحري والموانئ", PrimaryRegulatorCode = "TGA", IsCriticalInfrastructure = true, RecommendedFrameworks = "[\"NCA-ECC\",\"NCA-CSCC\"]", SortOrder = 101 },

                new() { Code = "PROFESSIONAL", NameEn = "Professional Services", NameAr = "الخدمات المهنية", PrimaryRegulatorCode = "MOCI", RecommendedFrameworks = "[\"PDPL\",\"NCA-ECC\",\"ISO27001\"]", SortOrder = 110 },
                new() { Code = "LEGAL", NameEn = "Legal Services", NameAr = "الخدمات القانونية", PrimaryRegulatorCode = "MOJ", RecommendedFrameworks = "[\"PDPL\",\"NCA-ECC\"]", SortOrder = 111 },

                new() { Code = "NGO", NameEn = "Non-Profit & NGO", NameAr = "المنظمات غير الربحية", PrimaryRegulatorCode = "NCNP", RecommendedFrameworks = "[\"PDPL\",\"NCA-ECC\"]", SortOrder = 120 },

                new() { Code = "OTHER", NameEn = "Other", NameAr = "أخرى", PrimaryRegulatorCode = "MOCI", RecommendedFrameworks = "[\"PDPL\",\"NCA-ECC\"]", SortOrder = 999 },
            };

            context.LookupSectors.AddRange(sectors);
            await context.SaveChangesAsync();
        }

        private static async Task SeedOrganizationTypesAsync(GrcDbContext context)
        {
            if (await context.LookupOrganizationTypes.AnyAsync()) return;

            var types = new List<LookupOrganizationType>
            {
                // Private Sector
                new() { Code = "LLC", NameEn = "Limited Liability Company (LLC)", NameAr = "شركة ذات مسؤولية محدودة", Category = "Private", SortOrder = 1 },
                new() { Code = "JSC", NameEn = "Joint Stock Company (Closed)", NameAr = "شركة مساهمة مقفلة", Category = "Private", SortOrder = 2 },
                new() { Code = "PJSC", NameEn = "Public Joint Stock Company", NameAr = "شركة مساهمة عامة", Category = "Private", IsRegulatedEntity = true, RequiresAudit = true, SortOrder = 3 },
                new() { Code = "BRANCH", NameEn = "Branch of Foreign Company", NameAr = "فرع شركة أجنبية", Category = "Private", SortOrder = 4 },
                new() { Code = "SOLE", NameEn = "Sole Proprietorship", NameAr = "مؤسسة فردية", Category = "Private", SortOrder = 5 },
                new() { Code = "PARTNERSHIP", NameEn = "Partnership", NameAr = "شركة تضامن", Category = "Private", SortOrder = 6 },
                new() { Code = "HOLDING", NameEn = "Holding Company", NameAr = "شركة قابضة", Category = "Private", RequiresAudit = true, SortOrder = 7 },
                new() { Code = "FREE_ZONE", NameEn = "Free Zone Entity", NameAr = "كيان منطقة حرة", Category = "Private", SortOrder = 8 },

                // Government
                new() { Code = "MINISTRY", NameEn = "Ministry / Government Ministry", NameAr = "وزارة", Category = "Government", IsRegulatedEntity = true, SortOrder = 20 },
                new() { Code = "GOV_AGENCY", NameEn = "Government Agency", NameAr = "هيئة حكومية", Category = "Government", IsRegulatedEntity = true, SortOrder = 21 },
                new() { Code = "GOV_AUTH", NameEn = "Government Authority", NameAr = "سلطة حكومية", Category = "Government", IsRegulatedEntity = true, SortOrder = 22 },
                new() { Code = "MUNICIPALITY", NameEn = "Municipality", NameAr = "بلدية", Category = "Government", SortOrder = 23 },
                new() { Code = "SOE", NameEn = "State-Owned Enterprise", NameAr = "شركة مملوكة للدولة", Category = "Government", IsRegulatedEntity = true, RequiresAudit = true, SortOrder = 24 },
                new() { Code = "SOVEREIGN", NameEn = "Sovereign Wealth Fund", NameAr = "صندوق سيادي", Category = "Government", IsRegulatedEntity = true, RequiresAudit = true, SortOrder = 25 },

                // Non-Profit
                new() { Code = "NONPROFIT", NameEn = "Non-Profit Organization", NameAr = "منظمة غير ربحية", Category = "NonProfit", SortOrder = 40 },
                new() { Code = "FOUNDATION", NameEn = "Foundation", NameAr = "مؤسسة خيرية", Category = "NonProfit", SortOrder = 41 },
                new() { Code = "ASSOCIATION", NameEn = "Professional Association", NameAr = "جمعية مهنية", Category = "NonProfit", SortOrder = 42 },
                new() { Code = "CHAMBER", NameEn = "Chamber of Commerce", NameAr = "غرفة تجارية", Category = "NonProfit", SortOrder = 43 },

                // Special
                new() { Code = "STARTUP", NameEn = "Startup / SME", NameAr = "شركة ناشئة", Category = "Private", DescriptionEn = "Emerging company eligible for simplified compliance", SortOrder = 60 },
                new() { Code = "FAMILY_OFFICE", NameEn = "Family Office", NameAr = "مكتب عائلي", Category = "Private", SortOrder = 61 },
                new() { Code = "JV", NameEn = "Joint Venture", NameAr = "مشروع مشترك", Category = "Private", SortOrder = 62 },
            };

            context.LookupOrganizationTypes.AddRange(types);
            await context.SaveChangesAsync();
        }

        private static async Task SeedRegulatorsAsync(GrcDbContext context)
        {
            if (await context.LookupRegulators.AnyAsync()) return;

            var regulators = new List<LookupRegulator>
            {
                // KSA Regulators
                new() { Code = "SAMA", NameEn = "SAMA", NameAr = "ساما", FullNameEn = "Saudi Central Bank", FullNameAr = "البنك المركزي السعودي", CountryCode = "SA", WebsiteUrl = "https://www.sama.gov.sa", RegulatedSectors = "[\"BANKING\",\"INSURANCE\",\"FINTECH\"]", IssuedFrameworks = "[\"SAMA-CSF\",\"SAMA-AML\",\"SAMA-BC\"]", SortOrder = 1 },
                new() { Code = "NCA", NameEn = "NCA", NameAr = "الهيئة الوطنية للأمن السيبراني", FullNameEn = "National Cybersecurity Authority", FullNameAr = "الهيئة الوطنية للأمن السيبراني", CountryCode = "SA", WebsiteUrl = "https://nca.gov.sa", IssuedFrameworks = "[\"NCA-ECC\",\"NCA-CSCC\",\"NCA-OT\",\"NCA-DCC\"]", SortOrder = 2 },
                new() { Code = "SDAIA", NameEn = "SDAIA", NameAr = "سدايا", FullNameEn = "Saudi Data & AI Authority", FullNameAr = "الهيئة السعودية للبيانات والذكاء الاصطناعي", CountryCode = "SA", WebsiteUrl = "https://sdaia.gov.sa", IssuedFrameworks = "[\"PDPL\",\"NDMO\"]", SortOrder = 3 },
                new() { Code = "CMA", NameEn = "CMA", NameAr = "هيئة السوق المالية", FullNameEn = "Capital Market Authority", FullNameAr = "هيئة السوق المالية", CountryCode = "SA", WebsiteUrl = "https://cma.org.sa", RegulatedSectors = "[\"CAPITAL_MARKETS\"]", SortOrder = 4 },
                new() { Code = "CST", NameEn = "CST", NameAr = "هيئة الاتصالات", FullNameEn = "Communications & Space & Technology Commission", FullNameAr = "هيئة الاتصالات والفضاء والتقنية", CountryCode = "SA", WebsiteUrl = "https://cst.gov.sa", RegulatedSectors = "[\"TELECOM\"]", IssuedFrameworks = "[\"CST-CRF\",\"CST-CLOUD\"]", SortOrder = 5 },
                new() { Code = "MOH", NameEn = "MOH", NameAr = "وزارة الصحة", FullNameEn = "Ministry of Health", FullNameAr = "وزارة الصحة", CountryCode = "SA", WebsiteUrl = "https://moh.gov.sa", RegulatedSectors = "[\"HEALTHCARE\"]", IssuedFrameworks = "[\"MOH-HIS\"]", SortOrder = 6 },
                new() { Code = "SFDA", NameEn = "SFDA", NameAr = "الهيئة العامة للغذاء والدواء", FullNameEn = "Saudi Food & Drug Authority", FullNameAr = "الهيئة العامة للغذاء والدواء", CountryCode = "SA", WebsiteUrl = "https://sfda.gov.sa", RegulatedSectors = "[\"PHARMA\",\"HEALTHCARE\"]", SortOrder = 7 },
                new() { Code = "MCIT", NameEn = "MCIT", NameAr = "وزارة الاتصالات", FullNameEn = "Ministry of Communications & IT", FullNameAr = "وزارة الاتصالات وتقنية المعلومات", CountryCode = "SA", WebsiteUrl = "https://mcit.gov.sa", SortOrder = 8 },
                new() { Code = "MOCI", NameEn = "MOCI", NameAr = "وزارة التجارة", FullNameEn = "Ministry of Commerce", FullNameAr = "وزارة التجارة", CountryCode = "SA", WebsiteUrl = "https://mc.gov.sa", IssuedFrameworks = "[\"MOCI-ECOM\"]", SortOrder = 9 },
                new() { Code = "DGA", NameEn = "DGA", NameAr = "هيئة الحكومة الرقمية", FullNameEn = "Digital Government Authority", FullNameAr = "هيئة الحكومة الرقمية", CountryCode = "SA", WebsiteUrl = "https://dga.gov.sa", IssuedFrameworks = "[\"DGA-CLOUD\"]", SortOrder = 10 },
                new() { Code = "MOE", NameEn = "MOE", NameAr = "وزارة الطاقة", FullNameEn = "Ministry of Energy", FullNameAr = "وزارة الطاقة", CountryCode = "SA", RegulatedSectors = "[\"ENERGY_OIL\"]", SortOrder = 11 },
                new() { Code = "ECRA", NameEn = "ECRA", NameAr = "هيئة تنظيم الكهرباء", FullNameEn = "Electricity & Cogeneration Regulatory Authority", FullNameAr = "هيئة تنظيم الكهرباء والإنتاج المزدوج", CountryCode = "SA", RegulatedSectors = "[\"ENERGY_POWER\"]", SortOrder = 12 },
                new() { Code = "GACA", NameEn = "GACA", NameAr = "الطيران المدني", FullNameEn = "General Authority of Civil Aviation", FullNameAr = "الهيئة العامة للطيران المدني", CountryCode = "SA", RegulatedSectors = "[\"AVIATION\"]", SortOrder = 13 },

                // International
                new() { Code = "ISO", NameEn = "ISO", NameAr = "أيزو", FullNameEn = "International Organization for Standardization", FullNameAr = "المنظمة الدولية للتوحيد القياسي", IssuedFrameworks = "[\"ISO27001\",\"ISO27701\",\"ISO22301\"]", SortOrder = 50 },
                new() { Code = "NIST", NameEn = "NIST", NameAr = "نيست", FullNameEn = "National Institute of Standards and Technology", FullNameAr = "المعهد الوطني للمعايير والتقنية", CountryCode = "US", IssuedFrameworks = "[\"NIST-CSF\",\"NIST-800-53\"]", SortOrder = 51 },
                new() { Code = "PCI-SSC", NameEn = "PCI SSC", NameAr = "مجلس معايير أمن بيانات بطاقات الدفع", FullNameEn = "Payment Card Industry Security Standards Council", IssuedFrameworks = "[\"PCI-DSS\"]", SortOrder = 52 },
            };

            context.LookupRegulators.AddRange(regulators);
            await context.SaveChangesAsync();
        }

        private static async Task SeedFrameworksAsync(GrcDbContext context)
        {
            if (await context.LookupFrameworks.AnyAsync()) return;

            var frameworks = new List<LookupFramework>
            {
                // KSA Mandatory Frameworks
                new() { Code = "NCA-ECC", NameEn = "NCA Essential Cybersecurity Controls", NameAr = "الضوابط الأساسية للأمن السيبراني", Type = "Regulation", IssuingBody = "NCA", CountryCode = "SA", Version = "2.0", IsMandatory = true, Priority = 1, EstimatedControlCount = 114, EstimatedImplementationMonths = 12, ComplexityLevel = "High", SortOrder = 1 },
                new() { Code = "NCA-CSCC", NameEn = "NCA Critical Systems Cybersecurity Controls", NameAr = "ضوابط الأمن السيبراني للأنظمة الحساسة", Type = "Regulation", IssuingBody = "NCA", CountryCode = "SA", IsMandatory = true, Priority = 1, EstimatedControlCount = 105, EstimatedImplementationMonths = 18, ComplexityLevel = "VeryHigh", ApplicableSectors = "[\"GOVERNMENT\",\"ENERGY_OIL\",\"TELECOM\",\"BANKING\"]", SortOrder = 2 },
                new() { Code = "NCA-OT", NameEn = "NCA OT Cybersecurity Controls", NameAr = "ضوابط الأمن السيبراني للتقنيات التشغيلية", Type = "Regulation", IssuingBody = "NCA", CountryCode = "SA", IsMandatory = true, Priority = 2, ApplicableSectors = "[\"ENERGY_OIL\",\"ENERGY_POWER\",\"MANUFACTURING\",\"WATER\"]", SortOrder = 3 },
                new() { Code = "NCA-DCC", NameEn = "NCA Data Cybersecurity Controls", NameAr = "ضوابط الأمن السيبراني للبيانات", Type = "Regulation", IssuingBody = "NCA", CountryCode = "SA", IsMandatory = true, Priority = 2, SortOrder = 4 },
                new() { Code = "PDPL", NameEn = "Personal Data Protection Law", NameAr = "نظام حماية البيانات الشخصية", Type = "Regulation", IssuingBody = "SDAIA", CountryCode = "SA", Version = "2023", EffectiveDate = new DateTime(2023, 9, 14, 0, 0, 0, DateTimeKind.Utc), IsMandatory = true, Priority = 1, EstimatedControlCount = 50, EstimatedImplementationMonths = 6, ComplexityLevel = "Medium", SortOrder = 5 },
                new() { Code = "NDMO", NameEn = "National Data Management Office Policies", NameAr = "سياسات مكتب إدارة البيانات الوطنية", Type = "Regulation", IssuingBody = "SDAIA", CountryCode = "SA", IsMandatory = true, Priority = 2, ApplicableSectors = "[\"GOVERNMENT\"]", SortOrder = 6 },

                // SAMA Frameworks
                new() { Code = "SAMA-CSF", NameEn = "SAMA Cyber Security Framework", NameAr = "إطار الأمن السيبراني لساما", Type = "Regulation", IssuingBody = "SAMA", CountryCode = "SA", Version = "1.0", IsMandatory = true, Priority = 1, EstimatedControlCount = 195, EstimatedImplementationMonths = 18, ComplexityLevel = "VeryHigh", ApplicableSectors = "[\"BANKING\",\"INSURANCE\",\"FINTECH\"]", SortOrder = 10 },
                new() { Code = "SAMA-AML", NameEn = "SAMA AML/CTF Requirements", NameAr = "متطلبات مكافحة غسل الأموال وتمويل الإرهاب", Type = "Regulation", IssuingBody = "SAMA", CountryCode = "SA", IsMandatory = true, Priority = 2, ApplicableSectors = "[\"BANKING\",\"INSURANCE\",\"FINTECH\"]", SortOrder = 11 },
                new() { Code = "SAMA-BC", NameEn = "SAMA Business Continuity", NameAr = "متطلبات استمرارية الأعمال", Type = "Regulation", IssuingBody = "SAMA", CountryCode = "SA", IsMandatory = true, Priority = 2, ApplicableSectors = "[\"BANKING\",\"INSURANCE\"]", SortOrder = 12 },

                // Sector-Specific
                new() { Code = "CST-CRF", NameEn = "CST Cybersecurity Regulatory Framework", NameAr = "الإطار التنظيمي للأمن السيبراني للاتصالات", Type = "Regulation", IssuingBody = "CST", CountryCode = "SA", IsMandatory = true, Priority = 1, ApplicableSectors = "[\"TELECOM\"]", SortOrder = 20 },
                new() { Code = "MOH-HIS", NameEn = "MOH Health Information Security", NameAr = "أمن المعلومات الصحية", Type = "Regulation", IssuingBody = "MOH", CountryCode = "SA", IsMandatory = true, Priority = 2, ApplicableSectors = "[\"HEALTHCARE\"]", SortOrder = 21 },
                new() { Code = "DGA-CLOUD", NameEn = "DGA Cloud Computing Regulatory Framework", NameAr = "الإطار التنظيمي للحوسبة السحابية", Type = "Regulation", IssuingBody = "DGA", CountryCode = "SA", IsMandatory = true, Priority = 2, ApplicableSectors = "[\"GOVERNMENT\"]", SortOrder = 22 },
                new() { Code = "MOCI-ECOM", NameEn = "E-Commerce Regulations", NameAr = "نظام التجارة الإلكترونية", Type = "Regulation", IssuingBody = "MOCI", CountryCode = "SA", IsMandatory = true, Priority = 3, ApplicableSectors = "[\"RETAIL\"]", SortOrder = 23 },

                // International Standards (Optional but recommended)
                new() { Code = "ISO27001", NameEn = "ISO 27001 - ISMS", NameAr = "آيزو 27001 - نظام إدارة أمن المعلومات", Type = "Standard", IssuingBody = "ISO", Version = "2022", IsMandatory = false, Priority = 3, EstimatedControlCount = 93, EstimatedImplementationMonths = 12, ComplexityLevel = "High", SortOrder = 30 },
                new() { Code = "ISO27701", NameEn = "ISO 27701 - Privacy Extension", NameAr = "آيزو 27701 - امتداد الخصوصية", Type = "Standard", IssuingBody = "ISO", Version = "2019", IsMandatory = false, Priority = 4, Prerequisites = "[\"ISO27001\"]", SortOrder = 31 },
                new() { Code = "ISO22301", NameEn = "ISO 22301 - Business Continuity", NameAr = "آيزو 22301 - استمرارية الأعمال", Type = "Standard", IssuingBody = "ISO", Version = "2019", IsMandatory = false, Priority = 4, SortOrder = 32 },
                new() { Code = "SOC2", NameEn = "SOC 2 Type II", NameAr = "تقرير SOC 2 النوع الثاني", Type = "Certification", IssuingBody = "AICPA", IsMandatory = false, Priority = 4, SortOrder = 33 },
                new() { Code = "PCI-DSS", NameEn = "PCI DSS", NameAr = "معيار أمان بيانات صناعة بطاقات الدفع", Type = "Standard", IssuingBody = "PCI-SSC", Version = "4.0", IsMandatory = false, Priority = 2, EstimatedControlCount = 264, EstimatedImplementationMonths = 12, ComplexityLevel = "High", ApplicableSectors = "[\"BANKING\",\"RETAIL\",\"HOSPITALITY\"]", SortOrder = 34 },
                new() { Code = "NIST-CSF", NameEn = "NIST Cybersecurity Framework", NameAr = "إطار نيست للأمن السيبراني", Type = "BestPractice", IssuingBody = "NIST", Version = "2.0", IsMandatory = false, Priority = 5, SortOrder = 35 },

                // UAE Frameworks
                new() { Code = "UAE-IAS", NameEn = "UAE IA Standards", NameAr = "معايير ضمان المعلومات الإماراتية", Type = "Regulation", IssuingBody = "TRA", CountryCode = "AE", IsMandatory = true, Priority = 1, SortOrder = 50 },
                new() { Code = "ADHICS", NameEn = "ADHICS - Abu Dhabi Healthcare", NameAr = "معايير أمن المعلومات الصحية - أبوظبي", Type = "Regulation", CountryCode = "AE", Priority = 2, ApplicableSectors = "[\"HEALTHCARE\"]", SortOrder = 51 },
            };

            context.LookupFrameworks.AddRange(frameworks);
            await context.SaveChangesAsync();
        }

        private static async Task SeedOrganizationSizesAsync(GrcDbContext context)
        {
            if (await context.LookupOrganizationSizes.AnyAsync()) return;

            var sizes = new List<LookupOrganizationSize>
            {
                new() { Code = "MICRO", NameEn = "Micro (1-10)", NameAr = "صغيرة جداً (1-10)", MinEmployees = 1, MaxEmployees = 10, RecommendedApproach = "Simplified", RecommendedTeamSize = 1, DescriptionEn = "Very small organizations with minimal compliance overhead", SortOrder = 1 },
                new() { Code = "STARTUP", NameEn = "Startup (11-50)", NameAr = "ناشئة (11-50)", MinEmployees = 11, MaxEmployees = 50, RecommendedApproach = "Simplified", RecommendedTeamSize = 1, DescriptionEn = "Emerging organizations eligible for startup compliance path", SortOrder = 2 },
                new() { Code = "SMALL", NameEn = "Small (51-250)", NameAr = "صغيرة (51-250)", MinEmployees = 51, MaxEmployees = 250, RecommendedApproach = "Standard", RecommendedTeamSize = 2, DescriptionEn = "Small organizations with basic compliance requirements", SortOrder = 3 },
                new() { Code = "MEDIUM", NameEn = "Medium (251-1000)", NameAr = "متوسطة (251-1000)", MinEmployees = 251, MaxEmployees = 1000, RecommendedApproach = "Standard", RecommendedTeamSize = 5, DescriptionEn = "Medium organizations with moderate compliance requirements", SortOrder = 4 },
                new() { Code = "LARGE", NameEn = "Large (1001-5000)", NameAr = "كبيرة (1001-5000)", MinEmployees = 1001, MaxEmployees = 5000, RecommendedApproach = "Comprehensive", RecommendedTeamSize = 10, DescriptionEn = "Large organizations with significant compliance requirements", SortOrder = 5 },
                new() { Code = "ENTERPRISE", NameEn = "Enterprise (5000+)", NameAr = "مؤسسة كبرى (5000+)", MinEmployees = 5001, MaxEmployees = int.MaxValue, RecommendedApproach = "Comprehensive", RecommendedTeamSize = 20, DescriptionEn = "Enterprise organizations with full compliance requirements", SortOrder = 6 },
            };

            context.LookupOrganizationSizes.AddRange(sizes);
            await context.SaveChangesAsync();
        }

        private static async Task SeedMaturityLevelsAsync(GrcDbContext context)
        {
            if (await context.LookupMaturityLevels.AnyAsync()) return;

            var levels = new List<LookupMaturityLevel>
            {
                new() { Code = "INITIAL", NameEn = "Initial / Ad-hoc", NameAr = "أولي / عشوائي", Level = 1, DescriptionEn = "No formal processes. Security is reactive.", DescriptionAr = "لا توجد عمليات رسمية. الأمن ردة فعل.", Characteristics = "[\"No documented policies\",\"Reactive security\",\"No assigned responsibilities\"]", RecommendedActions = "[\"Establish basic policies\",\"Assign security owner\",\"Conduct risk assessment\"]", SortOrder = 1 },
                new() { Code = "REPEATABLE", NameEn = "Repeatable", NameAr = "قابل للتكرار", Level = 2, DescriptionEn = "Basic processes exist but are not standardized.", DescriptionAr = "توجد عمليات أساسية لكنها غير موحدة.", Characteristics = "[\"Some documented procedures\",\"Basic training\",\"Informal risk management\"]", RecommendedActions = "[\"Document all procedures\",\"Implement formal training\",\"Establish metrics\"]", SortOrder = 2 },
                new() { Code = "DEFINED", NameEn = "Defined", NameAr = "محدد", Level = 3, DescriptionEn = "Standardized processes documented and communicated.", DescriptionAr = "عمليات موحدة موثقة ومعلنة.", Characteristics = "[\"Documented policies\",\"Regular training\",\"Defined responsibilities\",\"Basic monitoring\"]", RecommendedActions = "[\"Implement continuous monitoring\",\"Conduct internal audits\",\"Measure effectiveness\"]", SortOrder = 3 },
                new() { Code = "MANAGED", NameEn = "Managed", NameAr = "مُدار", Level = 4, DescriptionEn = "Processes measured, monitored and controlled.", DescriptionAr = "العمليات تُقاس وتُراقب وتُسيطر عليها.", Characteristics = "[\"KPIs tracked\",\"Regular audits\",\"Continuous monitoring\",\"Incident response tested\"]", RecommendedActions = "[\"Automate controls\",\"Implement predictive analytics\",\"Benchmark against peers\"]", SortOrder = 4 },
                new() { Code = "OPTIMIZED", NameEn = "Optimized", NameAr = "مُحسّن", Level = 5, DescriptionEn = "Continuous improvement and innovation.", DescriptionAr = "تحسين مستمر وابتكار.", Characteristics = "[\"Automated controls\",\"Predictive security\",\"Industry leadership\",\"Zero trust architecture\"]", RecommendedActions = "[\"Lead industry initiatives\",\"Contribute to standards\",\"Advanced threat hunting\"]", SortOrder = 5 },
            };

            context.LookupMaturityLevels.AddRange(levels);
            await context.SaveChangesAsync();
        }

        private static async Task SeedDataTypesAsync(GrcDbContext context)
        {
            if (await context.LookupDataTypes.AnyAsync()) return;

            var dataTypes = new List<LookupDataType>
            {
                // Personal Data (PDPL)
                new() { Code = "PERSONAL_BASIC", NameEn = "Personal Data - Basic", NameAr = "بيانات شخصية - أساسية", Category = "Personal", SensitivityLevel = "Confidential", RequiresConsent = true, ApplicableRegulations = "[\"PDPL\",\"GDPR\"]", DescriptionEn = "Name, email, phone, address", SortOrder = 1 },
                new() { Code = "NATIONAL_ID", NameEn = "National ID Data", NameAr = "بيانات الهوية الوطنية", Category = "Personal", SensitivityLevel = "Restricted", RequiresEncryption = true, RequiresConsent = true, ApplicableRegulations = "[\"PDPL\"]", DescriptionEn = "National ID, Iqama, passport numbers", SortOrder = 2 },
                new() { Code = "BIOMETRIC", NameEn = "Biometric Data", NameAr = "بيانات حيوية", Category = "Biometric", SensitivityLevel = "Restricted", RequiresEncryption = true, RequiresConsent = true, ApplicableRegulations = "[\"PDPL\",\"GDPR\"]", DescriptionEn = "Fingerprints, facial recognition, iris scans", SortOrder = 3 },
                new() { Code = "HEALTH", NameEn = "Health Data", NameAr = "بيانات صحية", Category = "Health", SensitivityLevel = "Restricted", RequiresEncryption = true, RequiresConsent = true, DefaultRetentionYears = 10, ApplicableRegulations = "[\"PDPL\",\"MOH-HIS\",\"HIPAA\"]", DescriptionEn = "Medical records, diagnoses, treatments", SortOrder = 4 },
                new() { Code = "GENETIC", NameEn = "Genetic Data", NameAr = "بيانات جينية", Category = "Health", SensitivityLevel = "Restricted", RequiresEncryption = true, RequiresConsent = true, ApplicableRegulations = "[\"PDPL\"]", SortOrder = 5 },

                // Financial Data
                new() { Code = "FINANCIAL_BASIC", NameEn = "Financial Data - Basic", NameAr = "بيانات مالية - أساسية", Category = "Financial", SensitivityLevel = "Confidential", RequiresEncryption = true, ApplicableRegulations = "[\"PDPL\",\"SAMA-CSF\"]", DescriptionEn = "Bank accounts, transactions", SortOrder = 10 },
                new() { Code = "PAYMENT_CARD", NameEn = "Payment Card Data", NameAr = "بيانات بطاقات الدفع", Category = "Financial", SensitivityLevel = "Restricted", RequiresEncryption = true, ApplicableRegulations = "[\"PCI-DSS\",\"SAMA-CSF\"]", DescriptionEn = "Credit/debit card numbers, CVV", SortOrder = 11 },
                new() { Code = "TAX", NameEn = "Tax Data", NameAr = "بيانات ضريبية", Category = "Financial", SensitivityLevel = "Confidential", DefaultRetentionYears = 10, ApplicableRegulations = "[\"ZATCA\"]", SortOrder = 12 },

                // Location & Behavioral
                new() { Code = "GEOLOCATION", NameEn = "Geolocation Data", NameAr = "بيانات الموقع الجغرافي", Category = "Location", SensitivityLevel = "Confidential", RequiresConsent = true, ApplicableRegulations = "[\"PDPL\"]", DescriptionEn = "GPS coordinates, location history", SortOrder = 20 },
                new() { Code = "BEHAVIORAL", NameEn = "Behavioral Data", NameAr = "بيانات سلوكية", Category = "Behavioral", SensitivityLevel = "Confidential", RequiresConsent = true, ApplicableRegulations = "[\"PDPL\"]", DescriptionEn = "Browsing history, preferences, habits", SortOrder = 21 },

                // Employment & HR
                new() { Code = "EMPLOYMENT", NameEn = "Employment Data", NameAr = "بيانات التوظيف", Category = "Employment", SensitivityLevel = "Confidential", DefaultRetentionYears = 7, ApplicableRegulations = "[\"PDPL\",\"GOSI\"]", DescriptionEn = "Salary, performance, contracts", SortOrder = 30 },

                // Business Data
                new() { Code = "TRADE_SECRET", NameEn = "Trade Secrets", NameAr = "أسرار تجارية", Category = "Business", SensitivityLevel = "Restricted", RequiresEncryption = true, DescriptionEn = "Proprietary formulas, processes", SortOrder = 40 },
                new() { Code = "CUSTOMER_LIST", NameEn = "Customer Lists", NameAr = "قوائم العملاء", Category = "Business", SensitivityLevel = "Confidential", RequiresConsent = true, ApplicableRegulations = "[\"PDPL\"]", SortOrder = 41 },

                // Public
                new() { Code = "PUBLIC", NameEn = "Public Data", NameAr = "بيانات عامة", Category = "Public", SensitivityLevel = "Public", DescriptionEn = "Publicly available information", SortOrder = 50 },
            };

            context.LookupDataTypes.AddRange(dataTypes);
            await context.SaveChangesAsync();
        }

        private static async Task SeedHostingModelsAsync(GrcDbContext context)
        {
            if (await context.LookupHostingModels.AnyAsync()) return;

            var models = new List<LookupHostingModel>
            {
                new() { Code = "ON_PREMISE", NameEn = "On-Premise", NameAr = "محلي (داخلي)", DescriptionEn = "Infrastructure hosted in organization's own data centers", DescriptionAr = "بنية تحتية مستضافة في مراكز بيانات المنظمة", SecurityConsiderations = "[\"Physical security\",\"Power redundancy\",\"Environmental controls\"]", ComplianceImplications = "[\"Full control\",\"Direct audit access\",\"Higher CAPEX\"]", RequiresDataLocalization = false, SortOrder = 1 },
                new() { Code = "PRIVATE_CLOUD", NameEn = "Private Cloud", NameAr = "سحابة خاصة", DescriptionEn = "Dedicated cloud infrastructure for single organization", DescriptionAr = "بنية سحابية مخصصة لمنظمة واحدة", SecurityConsiderations = "[\"Cloud security controls\",\"Access management\",\"Data isolation\"]", ComplianceImplications = "[\"Dedicated resources\",\"Custom security controls\",\"Moderate cost\"]", SortOrder = 2 },
                new() { Code = "PUBLIC_CLOUD", NameEn = "Public Cloud", NameAr = "سحابة عامة", DescriptionEn = "Infrastructure hosted by cloud service providers (AWS, Azure, GCP)", DescriptionAr = "بنية تحتية مستضافة من مزودي الخدمات السحابية", SecurityConsiderations = "[\"Shared responsibility\",\"Cloud provider certifications\",\"Multi-tenancy\"]", ComplianceImplications = "[\"CSP compliance certificates\",\"Data residency options\",\"Lower CAPEX\"]", RecommendedControls = "[\"Cloud access security broker\",\"Cloud workload protection\",\"Identity federation\"]", SortOrder = 3 },
                new() { Code = "HYBRID", NameEn = "Hybrid Cloud", NameAr = "سحابة هجينة", DescriptionEn = "Combination of on-premise and cloud infrastructure", DescriptionAr = "مزيج من البنية التحتية المحلية والسحابية", SecurityConsiderations = "[\"Integration security\",\"Data flow controls\",\"Consistent policies\"]", ComplianceImplications = "[\"Complex compliance scope\",\"Multiple audit points\",\"Flexibility\"]", SortOrder = 4 },
                new() { Code = "MULTI_CLOUD", NameEn = "Multi-Cloud", NameAr = "متعدد السحب", DescriptionEn = "Using multiple cloud providers simultaneously", DescriptionAr = "استخدام مزودي خدمات سحابية متعددين", SecurityConsiderations = "[\"Cross-cloud security\",\"Vendor lock-in avoidance\",\"Complex management\"]", ComplianceImplications = "[\"Multiple CSP audits\",\"Data portability\",\"Complex governance\"]", SortOrder = 5 },
                new() { Code = "EDGE", NameEn = "Edge Computing", NameAr = "الحوسبة الطرفية", DescriptionEn = "Processing at network edge, close to data sources", DescriptionAr = "المعالجة على حافة الشبكة، قريبة من مصادر البيانات", SecurityConsiderations = "[\"Physical security\",\"Limited connectivity\",\"Device management\"]", ComplianceImplications = "[\"Distributed compliance\",\"IoT considerations\",\"OT security\"]", SortOrder = 6 },
                new() { Code = "COLOCATION", NameEn = "Colocation", NameAr = "استضافة مشتركة", DescriptionEn = "Organization-owned hardware in third-party data center", DescriptionAr = "أجهزة مملوكة للمنظمة في مركز بيانات طرف ثالث", SecurityConsiderations = "[\"Physical access controls\",\"Shared facility\",\"Network isolation\"]", ComplianceImplications = "[\"Facility compliance\",\"Shared audit\",\"Moderate control\"]", SortOrder = 7 },
            };

            context.LookupHostingModels.AddRange(models);
            await context.SaveChangesAsync();
        }

        private static async Task SeedCloudProvidersAsync(GrcDbContext context)
        {
            if (await context.LookupCloudProviders.AnyAsync()) return;

            var providers = new List<LookupCloudProvider>
            {
                new() { Code = "AWS", NameEn = "Amazon Web Services", NameAr = "خدمات أمازون ويب", Type = "IaaS/PaaS/SaaS", HasKsaRegion = true, HasGccRegion = true, Certifications = "[\"ISO27001\",\"SOC2\",\"PCI-DSS\",\"CSA-STAR\"]", DataResidencyOptions = "[\"me-south-1 (Bahrain)\",\"me-central-1 (UAE)\"]", SortOrder = 1 },
                new() { Code = "AZURE", NameEn = "Microsoft Azure", NameAr = "مايكروسوفت أزور", Type = "IaaS/PaaS/SaaS", HasKsaRegion = true, HasGccRegion = true, Certifications = "[\"ISO27001\",\"SOC2\",\"PCI-DSS\",\"CSA-STAR\",\"G-Cloud\"]", DataResidencyOptions = "[\"UAE North\",\"UAE Central\",\"Qatar Central\"]", SortOrder = 2 },
                new() { Code = "GCP", NameEn = "Google Cloud Platform", NameAr = "منصة جوجل السحابية", Type = "IaaS/PaaS/SaaS", HasKsaRegion = true, HasGccRegion = true, Certifications = "[\"ISO27001\",\"SOC2\",\"PCI-DSS\",\"CSA-STAR\"]", DataResidencyOptions = "[\"me-central2 (KSA)\",\"me-west1 (Tel Aviv)\"]", SortOrder = 3 },
                new() { Code = "ORACLE", NameEn = "Oracle Cloud", NameAr = "أوراكل السحابية", Type = "IaaS/PaaS/SaaS", HasKsaRegion = true, HasGccRegion = true, Certifications = "[\"ISO27001\",\"SOC2\",\"PCI-DSS\"]", DataResidencyOptions = "[\"Jeddah\",\"Dubai\"]", SortOrder = 4 },
                new() { Code = "IBM", NameEn = "IBM Cloud", NameAr = "آي بي إم السحابية", Type = "IaaS/PaaS/SaaS", HasGccRegion = true, Certifications = "[\"ISO27001\",\"SOC2\",\"PCI-DSS\"]", SortOrder = 5 },
                new() { Code = "ALIBABA", NameEn = "Alibaba Cloud", NameAr = "علي بابا السحابية", Type = "IaaS/PaaS/SaaS", HasGccRegion = true, Certifications = "[\"ISO27001\",\"SOC2\",\"CSA-STAR\"]", DataResidencyOptions = "[\"Dubai\"]", SortOrder = 6 },
                new() { Code = "STC", NameEn = "STC Cloud", NameAr = "سحابة stc", Type = "IaaS/PaaS", HasKsaRegion = true, Certifications = "[\"ISO27001\",\"NCA-ECC\"]", DataResidencyOptions = "[\"Riyadh\",\"Jeddah\"]", DescriptionEn = "Saudi Telecom Company cloud services", SortOrder = 10 },
                new() { Code = "MOBILY", NameEn = "Mobily Cloud", NameAr = "سحابة موبايلي", Type = "IaaS", HasKsaRegion = true, Certifications = "[\"ISO27001\"]", DataResidencyOptions = "[\"Riyadh\"]", SortOrder = 11 },
                new() { Code = "ZAIN", NameEn = "Zain Cloud", NameAr = "سحابة زين", Type = "IaaS", HasKsaRegion = true, DataResidencyOptions = "[\"Riyadh\"]", SortOrder = 12 },
                new() { Code = "ETISALAT", NameEn = "Etisalat Cloud", NameAr = "سحابة اتصالات", Type = "IaaS/PaaS", HasGccRegion = true, DataResidencyOptions = "[\"Abu Dhabi\",\"Dubai\"]", SortOrder = 13 },
                new() { Code = "DU", NameEn = "du Cloud", NameAr = "سحابة دو", Type = "IaaS", HasGccRegion = true, DataResidencyOptions = "[\"Dubai\"]", SortOrder = 14 },
            };

            context.LookupCloudProviders.AddRange(providers);
            await context.SaveChangesAsync();
        }
    }
}
