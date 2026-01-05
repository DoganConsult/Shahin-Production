using GrcMvc.Models.Entities.Catalogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Data.Seeds;

/// <summary>
/// Seeds 92 regulators: 62 Saudi, 20 International, 10 GCC/Regional
/// Layer 1: Global (Platform) - Immutable regulatory authority data
/// </summary>
public static class RegulatorSeeds
{
    public static async Task SeedRegulatorsAsync(GrcDbContext context, ILogger logger)
    {
        try
        {
            if (await context.RegulatorCatalogs.AnyAsync())
            {
                logger.LogInformation("✅ Regulators already exist. Skipping seed.");
                return;
            }

            var regulators = GetAllRegulators();
            await context.RegulatorCatalogs.AddRangeAsync(regulators);
            await context.SaveChangesAsync();

            logger.LogInformation($"✅ Successfully seeded {regulators.Count} regulators (62 Saudi, 20 International, 10 Regional)");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error seeding regulators");
            throw;
        }
    }

    private static List<RegulatorCatalog> GetAllRegulators()
    {
        var regulators = new List<RegulatorCatalog>();
        int order = 1;

        // ========== SAUDI REGULATORS (62) ==========
        regulators.AddRange(GetSaudiRegulators(ref order));

        // ========== INTERNATIONAL STANDARDS BODIES (20) ==========
        regulators.AddRange(GetInternationalRegulators(ref order));

        // ========== GCC & REGIONAL REGULATORS (10) ==========
        regulators.AddRange(GetRegionalRegulators(ref order));

        return regulators;
    }

    private static List<RegulatorCatalog> GetSaudiRegulators(ref int order)
    {
        return new List<RegulatorCatalog>
        {
            new() { Id = Guid.NewGuid(), Code = "NCA", NameAr = "الهيئة الوطنية للأمن السيبراني", NameEn = "National Cybersecurity Authority", JurisdictionEn = "Cybersecurity & Critical Infrastructure Protection", Website = "https://nca.gov.sa", Category = "cybersecurity", Sector = "all", Established = 2017, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "SAMA", NameAr = "البنك المركزي السعودي", NameEn = "Saudi Central Bank", JurisdictionEn = "Financial Services Banking Insurance FinTech", Website = "https://sama.gov.sa", Category = "financial", Sector = "banking_finance", Established = 1952, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "SDAIA", NameAr = "الهيئة السعودية للبيانات والذكاء الاصطناعي", NameEn = "Saudi Data & AI Authority", JurisdictionEn = "Personal Data Protection & AI Governance", Website = "https://sdaia.gov.sa", Category = "data_privacy", Sector = "all", Established = 2019, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "CMA", NameAr = "هيئة السوق المالية", NameEn = "Capital Market Authority", JurisdictionEn = "Capital Markets Securities & Investment", Website = "https://cma.org.sa", Category = "financial", Sector = "capital_markets", Established = 2003, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "DGA", NameAr = "هيئة الحكومة الرقمية", NameEn = "Digital Government Authority", JurisdictionEn = "Digital Transformation & E-Government", Website = "https://dga.gov.sa", Category = "digital_gov", Sector = "government", Established = 2021, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "CST", NameAr = "هيئة الاتصالات والفضاء والتقنية", NameEn = "Communications Space & Technology Commission", JurisdictionEn = "Telecommunications ICT Space & Cloud", Website = "https://cst.gov.sa", Category = "telecom", Sector = "telecom_ict", Established = 2023, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "ZATCA", NameAr = "هيئة الزكاة والضريبة والجمارك", NameEn = "Zakat Tax & Customs Authority", JurisdictionEn = "Zakat Tax Customs & E-Invoicing", Website = "https://zatca.gov.sa", Category = "tax", Sector = "all", Established = 2021, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "MOCI", NameAr = "وزارة التجارة", NameEn = "Ministry of Commerce", JurisdictionEn = "Commerce Consumer Protection & Business Registration", Website = "https://mc.gov.sa", Category = "commerce", Sector = "commerce", Established = 1954, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "SFDA", NameAr = "هيئة الغذاء والدواء", NameEn = "Saudi Food & Drug Authority", JurisdictionEn = "Food Drug Medical Devices & Cosmetics", Website = "https://sfda.gov.sa", Category = "healthcare", Sector = "healthcare_pharma", Established = 2003, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "MOH", NameAr = "وزارة الصحة", NameEn = "Ministry of Health", JurisdictionEn = "Healthcare Services Facilities & Public Health", Website = "https://moh.gov.sa", Category = "healthcare", Sector = "healthcare", Established = 1950, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "CBAHI", NameAr = "المركز السعودي لاعتماد المنشآت الصحية", NameEn = "Saudi Central Board for Accreditation of Healthcare", JurisdictionEn = "Healthcare Facility Accreditation", Website = "https://cbahi.gov.sa", Category = "healthcare", Sector = "healthcare", Established = 2005, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "SCFHS", NameAr = "الهيئة السعودية للتخصصات الصحية", NameEn = "Saudi Commission for Health Specialties", JurisdictionEn = "Medical Education & Health Specialties", Website = "https://scfhs.org.sa", Category = "healthcare", Sector = "healthcare", Established = 1992, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "ECRA", NameAr = "هيئة تنظيم الكهرباء", NameEn = "Electricity & Cogeneration Regulatory Authority", JurisdictionEn = "Electricity Generation & Distribution", Website = "https://ecra.gov.sa", Category = "energy", Sector = "energy", Established = 2001, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "WERA", NameAr = "هيئة تنظيم المياه والكهرباء", NameEn = "Water & Electricity Regulatory Authority", JurisdictionEn = "Water & Electricity Services", Website = "https://wera.gov.sa", Category = "utilities", Sector = "utilities", Established = 2021, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "MOMRAH", NameAr = "وزارة الشؤون البلدية والقروية والإسكان", NameEn = "Ministry of Municipal Rural Affairs & Housing", JurisdictionEn = "Municipalities Housing & Urban Planning", Website = "https://momra.gov.sa", Category = "municipal", Sector = "municipal", Established = 1975, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "MHRSD", NameAr = "وزارة الموارد البشرية والتنمية الاجتماعية", NameEn = "Ministry of Human Resources & Social Development", JurisdictionEn = "Labor Law HR & Social Development", Website = "https://hrsd.gov.sa", Category = "labor", Sector = "all", Established = 2016, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "MISA", NameAr = "وزارة الاستثمار", NameEn = "Ministry of Investment", JurisdictionEn = "Foreign Investment & Business Licensing", Website = "https://misa.gov.sa", Category = "investment", Sector = "investment", Established = 2019, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "NCEC", NameAr = "المركز الوطني للامتثال البيئي", NameEn = "National Center for Environmental Compliance", JurisdictionEn = "Environmental Compliance & Standards", Website = "https://ncec.gov.sa", Category = "environment", Sector = "environment", Established = 2019, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "MEWA", NameAr = "وزارة البيئة والمياه والزراعة", NameEn = "Ministry of Environment Water & Agriculture", JurisdictionEn = "Environment Water & Agriculture", Website = "https://mewa.gov.sa", Category = "environment", Sector = "agriculture", Established = 2016, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "GASTAT", NameAr = "الهيئة العامة للإحصاء", NameEn = "General Authority for Statistics", JurisdictionEn = "National Statistics & Data", Website = "https://stats.gov.sa", Category = "statistics", Sector = "government", Established = 2019, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "MONSHAAT", NameAr = "الهيئة العامة للمنشآت الصغيرة والمتوسطة", NameEn = "Small & Medium Enterprises Authority", JurisdictionEn = "SME Development & Support", Website = "https://monshaat.gov.sa", Category = "business", Sector = "sme", Established = 2016, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "PIF", NameAr = "صندوق الاستثمارات العامة", NameEn = "Public Investment Fund", JurisdictionEn = "Sovereign Investment", Website = "https://pif.gov.sa", Category = "investment", Sector = "investment", Established = 1971, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "REDF", NameAr = "صندوق التنمية العقارية", NameEn = "Real Estate Development Fund", JurisdictionEn = "Real Estate Finance", Website = "https://redf.gov.sa", Category = "finance", Sector = "real_estate", Established = 2017, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "NHC", NameAr = "الشركة الوطنية للإسكان", NameEn = "National Housing Company", JurisdictionEn = "Housing Development", Website = "https://nhc.sa", Category = "housing", Sector = "real_estate", Established = 2017, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "SASO", NameAr = "الهيئة السعودية للمواصفات والمقاييس والجودة", NameEn = "Saudi Standards Metrology & Quality Organization", JurisdictionEn = "Standards Metrology & Quality", Website = "https://saso.gov.sa", Category = "standards", Sector = "all", Established = 1972, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "TADAWUL", NameAr = "السوق المالية السعودية", NameEn = "Saudi Stock Exchange", JurisdictionEn = "Stock Market & Securities Trading", Website = "https://saudiexchange.sa", Category = "financial", Sector = "capital_markets", Established = 2007, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "IAIC", NameAr = "هيئة التأمين", NameEn = "Insurance Authority", JurisdictionEn = "Insurance & Reinsurance Regulation", Website = "https://ia.gov.sa", Category = "financial", Sector = "insurance", Established = 2023, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "KACARE", NameAr = "مدينة الملك عبدالله للطاقة الذرية والمتجددة", NameEn = "King Abdullah City for Atomic & Renewable Energy", JurisdictionEn = "Nuclear & Renewable Energy", Website = "https://kacare.gov.sa", Category = "energy", Sector = "energy", Established = 2010, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "SOCPA", NameAr = "الهيئة السعودية للمراجعين والمحاسبين", NameEn = "Saudi Organization for Chartered & Professional Accountants", JurisdictionEn = "Accounting & Auditing Standards", Website = "https://socpa.org.sa", Category = "professional", Sector = "accounting", Established = 1992, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "MOJ", NameAr = "وزارة العدل", NameEn = "Ministry of Justice", JurisdictionEn = "Justice Courts & Notarization", Website = "https://moj.gov.sa", Category = "legal", Sector = "legal", Established = 1970, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "MOI", NameAr = "وزارة الداخلية", NameEn = "Ministry of Interior", JurisdictionEn = "Interior Security & National ID", Website = "https://moi.gov.sa", Category = "security", Sector = "government", Established = 1951, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "MOE", NameAr = "وزارة التعليم", NameEn = "Ministry of Education", JurisdictionEn = "Education & Educational Institutions", Website = "https://moe.gov.sa", Category = "education", Sector = "education", Established = 1953, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "MOS", NameAr = "وزارة الرياضة", NameEn = "Ministry of Sports", JurisdictionEn = "Sports & Sports Clubs", Website = "https://mos.gov.sa", Category = "sports", Sector = "sports", Established = 2020, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "STA", NameAr = "هيئة السياحة السعودية", NameEn = "Saudi Tourism Authority", JurisdictionEn = "Tourism Hotels & Travel", Website = "https://sta.gov.sa", Category = "tourism", Sector = "tourism", Established = 2000, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "MOT", NameAr = "وزارة السياحة", NameEn = "Ministry of Tourism", JurisdictionEn = "Tourism & Hospitality", Website = "https://mt.gov.sa", Category = "tourism", Sector = "tourism", Established = 2020, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "MOFA", NameAr = "وزارة الخارجية", NameEn = "Ministry of Foreign Affairs", JurisdictionEn = "Foreign Affairs & Diplomacy", Website = "https://mofa.gov.sa", Category = "government", Sector = "government", Established = 1930, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "MOF", NameAr = "وزارة المالية", NameEn = "Ministry of Finance", JurisdictionEn = "Public Finance & Budget", Website = "https://mof.gov.sa", Category = "finance", Sector = "government", Established = 1932, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "MOENERGY", NameAr = "وزارة الطاقة", NameEn = "Ministry of Energy", JurisdictionEn = "Energy Oil & Gas", Website = "https://moenergy.gov.sa", Category = "energy", Sector = "energy", Established = 2019, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "MODON", NameAr = "الهيئة السعودية للمدن الصناعية", NameEn = "Saudi Authority for Industrial Cities", JurisdictionEn = "Industrial Cities & Zones", Website = "https://modon.gov.sa", Category = "industrial", Sector = "industrial", Established = 2001, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "SIDF", NameAr = "صندوق التنمية الصناعية السعودي", NameEn = "Saudi Industrial Development Fund", JurisdictionEn = "Industrial Finance", Website = "https://sidf.gov.sa", Category = "finance", Sector = "industrial", Established = 1974, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "GACA", NameAr = "الهيئة العامة للطيران المدني", NameEn = "General Authority of Civil Aviation", JurisdictionEn = "Civil Aviation & Airports", Website = "https://gaca.gov.sa", Category = "transport", Sector = "aviation", Established = 2002, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "TGA", NameAr = "الهيئة العامة للنقل", NameEn = "Transport General Authority", JurisdictionEn = "Transport & Logistics", Website = "https://tga.gov.sa", Category = "transport", Sector = "transport", Established = 2018, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "SAR", NameAr = "الشركة السعودية للخطوط الحديدية", NameEn = "Saudi Railway Company", JurisdictionEn = "Railways & Trains", Website = "https://sar.com.sa", Category = "transport", Sector = "railways", Established = 2006, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "MAWANI", NameAr = "الهيئة العامة للموانئ", NameEn = "Saudi Ports Authority", JurisdictionEn = "Seaports & Maritime", Website = "https://mawani.gov.sa", Category = "transport", Sector = "maritime", Established = 1976, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "SAUDIPOST", NameAr = "مؤسسة البريد السعودي", NameEn = "Saudi Post", JurisdictionEn = "Postal Services", Website = "https://sp.com.sa", Category = "services", Sector = "postal", Established = 1926, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "SRCA", NameAr = "هيئة الهلال الأحمر السعودي", NameEn = "Saudi Red Crescent Authority", JurisdictionEn = "Emergency & Humanitarian Services", Website = "https://srca.org.sa", Category = "humanitarian", Sector = "emergency", Established = 1963, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "GOSI", NameAr = "المؤسسة العامة للتأمينات الاجتماعية", NameEn = "General Organization for Social Insurance", JurisdictionEn = "Social Insurance", Website = "https://gosi.gov.sa", Category = "social", Sector = "insurance", Established = 1969, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "PPA", NameAr = "المؤسسة العامة للتقاعد", NameEn = "Public Pension Agency", JurisdictionEn = "Civil Pension", Website = "https://pension.gov.sa", Category = "social", Sector = "pension", Established = 1958, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "MCIT", NameAr = "وزارة الاتصالات وتقنية المعلومات", NameEn = "Ministry of Communications & IT", JurisdictionEn = "Communications & IT Policy", Website = "https://mcit.gov.sa", Category = "telecom", Sector = "telecom_ict", Established = 2003, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "SAIP", NameAr = "الهيئة السعودية للملكية الفكرية", NameEn = "Saudi Authority for Intellectual Property", JurisdictionEn = "Intellectual Property & Patents", Website = "https://saip.gov.sa", Category = "legal", Sector = "ip", Established = 2018, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "GCAM", NameAr = "الهيئة العامة للإعلام المرئي والمسموع", NameEn = "General Commission for Audiovisual Media", JurisdictionEn = "Audiovisual Media Regulation", Website = "https://gcam.gov.sa", Category = "media", Sector = "media", Established = 2011, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "MOM", NameAr = "وزارة الإعلام", NameEn = "Ministry of Media", JurisdictionEn = "Media & Press", Website = "https://media.gov.sa", Category = "media", Sector = "media", Established = 2020, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "GEA", NameAr = "هيئة الترفيه", NameEn = "General Entertainment Authority", JurisdictionEn = "Entertainment & Events", Website = "https://gea.gov.sa", Category = "entertainment", Sector = "entertainment", Established = 2016, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "MOC", NameAr = "وزارة الثقافة", NameEn = "Ministry of Culture", JurisdictionEn = "Culture & Arts", Website = "https://moc.gov.sa", Category = "culture", Sector = "culture", Established = 2018, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "NEOM", NameAr = "نيوم", NameEn = "NEOM", JurisdictionEn = "NEOM Smart City Development", Website = "https://neom.com", Category = "megaproject", Sector = "development", Established = 2017, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "RCU", NameAr = "الهيئة الملكية لمحافظة العلا", NameEn = "Royal Commission for AlUla", JurisdictionEn = "AlUla Development", Website = "https://rcu.gov.sa", Category = "development", Sector = "tourism", Established = 2017, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "RCJY", NameAr = "الهيئة الملكية للجبيل وينبع", NameEn = "Royal Commission for Jubail & Yanbu", JurisdictionEn = "Industrial Cities Development", Website = "https://rcjy.gov.sa", Category = "industrial", Sector = "industrial", Established = 1975, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "NWC", NameAr = "شركة المياه الوطنية", NameEn = "National Water Company", JurisdictionEn = "Water & Sewerage Services", Website = "https://nwc.com.sa", Category = "utilities", Sector = "water", Established = 2008, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "SEC", NameAr = "الشركة السعودية للكهرباء", NameEn = "Saudi Electricity Company", JurisdictionEn = "Electricity Services", Website = "https://se.com.sa", Category = "utilities", Sector = "electricity", Established = 1999, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "ARAMCO", NameAr = "أرامكو السعودية", NameEn = "Saudi Aramco", JurisdictionEn = "Oil Gas & Petrochemicals", Website = "https://aramco.com", Category = "energy", Sector = "oil_gas", Established = 1933, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "SABIC", NameAr = "سابك", NameEn = "SABIC", JurisdictionEn = "Petrochemicals & Chemicals", Website = "https://sabic.com", Category = "industrial", Sector = "petrochemicals", Established = 1976, RegionType = "saudi", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "MAADEN", NameAr = "معادن", NameEn = "Saudi Arabian Mining Company", JurisdictionEn = "Mining & Minerals", Website = "https://maaden.com.sa", Category = "mining", Sector = "mining", Established = 1997, RegionType = "saudi", DisplayOrder = order++ },
        };
    }

    private static List<RegulatorCatalog> GetInternationalRegulators(ref int order)
    {
        return new List<RegulatorCatalog>
        {
            new() { Id = Guid.NewGuid(), Code = "ISO", NameAr = "المنظمة الدولية للمعايير", NameEn = "International Organization for Standardization", JurisdictionEn = "International Standards", Website = "https://iso.org", Category = "international", Sector = "standards", Established = 1947, RegionType = "international", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "IEC", NameAr = "اللجنة الكهروتقنية الدولية", NameEn = "International Electrotechnical Commission", JurisdictionEn = "Electrical & Electronic Standards", Website = "https://iec.ch", Category = "international", Sector = "standards", Established = 1906, RegionType = "international", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "NIST", NameAr = "المعهد الوطني للمعايير والتقنية", NameEn = "National Institute of Standards & Technology", JurisdictionEn = "US Cybersecurity Standards", Website = "https://nist.gov", Category = "international", Sector = "cybersecurity", Established = 1901, RegionType = "international", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "PCI-SSC", NameAr = "مجلس معايير أمان صناعة بطاقات الدفع", NameEn = "Payment Card Industry Security Standards Council", JurisdictionEn = "Payment Card Security", Website = "https://pcisecuritystandards.org", Category = "international", Sector = "payment", Established = 2006, RegionType = "international", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "AICPA", NameAr = "المعهد الأمريكي للمحاسبين", NameEn = "American Institute of CPAs", JurisdictionEn = "Accounting & SOC Standards", Website = "https://aicpa.org", Category = "international", Sector = "accounting", Established = 1887, RegionType = "international", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "ISACA", NameAr = "جمعية تدقيق نظم المعلومات", NameEn = "ISACA", JurisdictionEn = "IT Governance & Audit", Website = "https://isaca.org", Category = "international", Sector = "it_governance", Established = 1969, RegionType = "international", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "EU-GDPR", NameAr = "الاتحاد الأوروبي - حماية البيانات", NameEn = "European Union - GDPR", JurisdictionEn = "European Data Protection", Website = "https://gdpr.eu", Category = "international", Sector = "data_privacy", Established = 2018, RegionType = "international", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "HHS-OCR", NameAr = "وزارة الصحة الأمريكية - HIPAA", NameEn = "US HHS Office for Civil Rights", JurisdictionEn = "Healthcare Data Protection (HIPAA)", Website = "https://hhs.gov/hipaa", Category = "international", Sector = "healthcare", Established = 1996, RegionType = "international", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "HITRUST", NameAr = "تحالف الثقة في المعلومات الصحية", NameEn = "HITRUST Alliance", JurisdictionEn = "Healthcare Security Framework", Website = "https://hitrustalliance.net", Category = "international", Sector = "healthcare", Established = 2007, RegionType = "international", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "CIS", NameAr = "مركز أمن الإنترنت", NameEn = "Center for Internet Security", JurisdictionEn = "Critical Security Controls", Website = "https://cisecurity.org", Category = "international", Sector = "cybersecurity", Established = 2000, RegionType = "international", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "OWASP", NameAr = "مشروع أمان تطبيقات الويب", NameEn = "Open Web Application Security Project", JurisdictionEn = "Application Security", Website = "https://owasp.org", Category = "international", Sector = "appsec", Established = 2001, RegionType = "international", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "CSA", NameAr = "تحالف أمان السحابة", NameEn = "Cloud Security Alliance", JurisdictionEn = "Cloud Security Standards", Website = "https://cloudsecurityalliance.org", Category = "international", Sector = "cloud", Established = 2008, RegionType = "international", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "SWIFT", NameAr = "سويفت", NameEn = "SWIFT", JurisdictionEn = "Financial Messaging Security", Website = "https://swift.com", Category = "international", Sector = "banking", Established = 1973, RegionType = "international", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "BASEL", NameAr = "لجنة بازل للرقابة المصرفية", NameEn = "Basel Committee on Banking Supervision", JurisdictionEn = "Banking Supervision Standards", Website = "https://bis.org/bcbs", Category = "international", Sector = "banking", Established = 1974, RegionType = "international", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "FATF", NameAr = "مجموعة العمل المالي", NameEn = "Financial Action Task Force", JurisdictionEn = "AML/CFT Standards", Website = "https://fatf-gafi.org", Category = "international", Sector = "aml", Established = 1989, RegionType = "international", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "IIA", NameAr = "معهد المدققين الداخليين", NameEn = "Institute of Internal Auditors", JurisdictionEn = "Internal Audit Standards", Website = "https://theiia.org", Category = "international", Sector = "audit", Established = 1941, RegionType = "international", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "COSO", NameAr = "لجنة المنظمات الراعية", NameEn = "Committee of Sponsoring Organizations", JurisdictionEn = "Internal Control Framework", Website = "https://coso.org", Category = "international", Sector = "governance", Established = 1985, RegionType = "international", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "ITIL", NameAr = "مكتبة البنية التحتية لتقنية المعلومات", NameEn = "ITIL (Axelos)", JurisdictionEn = "IT Service Management", Website = "https://axelos.com/itil", Category = "international", Sector = "itsm", Established = 1989, RegionType = "international", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "PMI", NameAr = "معهد إدارة المشاريع", NameEn = "Project Management Institute", JurisdictionEn = "Project Management Standards", Website = "https://pmi.org", Category = "international", Sector = "project_mgmt", Established = 1969, RegionType = "international", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "TOGAF", NameAr = "إطار عمل المجموعة المفتوحة", NameEn = "The Open Group Architecture Framework", JurisdictionEn = "Enterprise Architecture", Website = "https://opengroup.org/togaf", Category = "international", Sector = "architecture", Established = 1995, RegionType = "international", DisplayOrder = order++ },
        };
    }

    private static List<RegulatorCatalog> GetRegionalRegulators(ref int order)
    {
        return new List<RegulatorCatalog>
        {
            new() { Id = Guid.NewGuid(), Code = "CBUAE", NameAr = "مصرف الإمارات المركزي", NameEn = "Central Bank of UAE", JurisdictionEn = "UAE Financial Services", Website = "https://centralbank.ae", Category = "regional", Sector = "banking", Established = 1980, RegionType = "regional", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "ADGM", NameAr = "سوق أبوظبي العالمي", NameEn = "Abu Dhabi Global Market", JurisdictionEn = "UAE Financial Free Zone", Website = "https://adgm.com", Category = "regional", Sector = "finance", Established = 2013, RegionType = "regional", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "DIFC", NameAr = "مركز دبي المالي العالمي", NameEn = "Dubai International Financial Centre", JurisdictionEn = "Dubai Financial Free Zone", Website = "https://difc.ae", Category = "regional", Sector = "finance", Established = 2004, RegionType = "regional", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "CBB", NameAr = "مصرف البحرين المركزي", NameEn = "Central Bank of Bahrain", JurisdictionEn = "Bahrain Financial Services", Website = "https://cbb.gov.bh", Category = "regional", Sector = "banking", Established = 2006, RegionType = "regional", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "CBO", NameAr = "البنك المركزي العماني", NameEn = "Central Bank of Oman", JurisdictionEn = "Oman Financial Services", Website = "https://cbo.gov.om", Category = "regional", Sector = "banking", Established = 1974, RegionType = "regional", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "QCB", NameAr = "مصرف قطر المركزي", NameEn = "Qatar Central Bank", JurisdictionEn = "Qatar Financial Services", Website = "https://qcb.gov.qa", Category = "regional", Sector = "banking", Established = 1993, RegionType = "regional", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "CBK", NameAr = "بنك الكويت المركزي", NameEn = "Central Bank of Kuwait", JurisdictionEn = "Kuwait Financial Services", Website = "https://cbk.gov.kw", Category = "regional", Sector = "banking", Established = 1968, RegionType = "regional", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "NESA", NameAr = "الهيئة الوطنية للأمن الإلكتروني - الإمارات", NameEn = "UAE National Electronic Security Authority", JurisdictionEn = "UAE Cybersecurity", Website = "https://nesa.gov.ae", Category = "regional", Sector = "cybersecurity", Established = 2012, RegionType = "regional", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "NCSC-UK", NameAr = "المركز الوطني للأمن السيبراني - المملكة المتحدة", NameEn = "UK National Cyber Security Centre", JurisdictionEn = "UK Cybersecurity", Website = "https://ncsc.gov.uk", Category = "international", Sector = "cybersecurity", Established = 2016, RegionType = "regional", DisplayOrder = order++ },
            new() { Id = Guid.NewGuid(), Code = "ENISA", NameAr = "وكالة الاتحاد الأوروبي للأمن السيبراني", NameEn = "EU Agency for Cybersecurity", JurisdictionEn = "EU Cybersecurity", Website = "https://enisa.europa.eu", Category = "international", Sector = "cybersecurity", Established = 2004, RegionType = "regional", DisplayOrder = order++ },
        };
    }
}
