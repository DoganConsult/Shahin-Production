using GrcMvc.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Data.Seeds;

/// <summary>
/// EF Core seeder for onboarding questions catalog
/// </summary>
public class OnboardingQuestionSeeder
{
    private readonly GrcDbContext _db;
    private readonly ILogger<OnboardingQuestionSeeder> _logger;

    public OnboardingQuestionSeeder(GrcDbContext db, ILogger<OnboardingQuestionSeeder> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        if (await _db.OnboardingSections.AnyAsync())
        {
            _logger.LogInformation("Onboarding questions already seeded");
            return;
        }

        _logger.LogInformation("Seeding onboarding question catalog...");

        // Seed sections
        var sections = GetSections();
        await _db.OnboardingSections.AddRangeAsync(sections);
        await _db.SaveChangesAsync();

        // Seed questions for each section
        var sectionMap = sections.ToDictionary(s => s.Code, s => s.Id);
        var questions = GetAllQuestions(sectionMap);
        await _db.OnboardingQuestions.AddRangeAsync(questions);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Seeded {S} sections, {Q} questions", sections.Count, questions.Count);
    }

    private List<OnboardingSection> GetSections() => new()
    {
        Sec("A", 1, "Organization Identity & Tenancy", "هوية المنظمة والمستأجر", "fa-building", true),
        Sec("B", 2, "Assurance Objective", "هدف التأكيد", "fa-bullseye", true),
        Sec("C", 3, "Regulatory & Framework Applicability", "الأطر التنظيمية المطبقة", "fa-gavel", true),
        Sec("D", 4, "Scope Definition", "تحديد النطاق", "fa-crosshairs", true),
        Sec("E", 5, "Data & Risk Profile", "ملف البيانات والمخاطر", "fa-shield-alt", true),
        Sec("F", 6, "Technology Landscape", "المشهد التقني", "fa-server", false),
        Sec("G", 7, "Control Ownership Model", "نموذج ملكية الضوابط", "fa-users-cog", true),
        Sec("H", 8, "Teams, Roles & Access", "الفرق والأدوار والوصول", "fa-user-friends", true),
        Sec("I", 9, "Workflow & Cadence", "سير العمل والتكرار", "fa-project-diagram", false),
        Sec("J", 10, "Evidence Standards", "معايير الأدلة", "fa-file-contract", false),
        Sec("K", 11, "Baseline & Overlays Selection", "اختيار الأساس والطبقات", "fa-layer-group", false),
        Sec("L", 12, "Go-Live & Success Metrics", "مقاييس الإطلاق والنجاح", "fa-rocket", false)
    };

    private OnboardingSection Sec(string code, int step, string nameEn, string nameAr, string icon, bool req) => new()
    {
        Id = Guid.NewGuid(), Code = code, StepNumber = step, SortOrder = step,
        NameEn = nameEn, NameAr = nameAr, Icon = icon, IsRequired = req, IsActive = true
    };

    private List<OnboardingQuestion> GetAllQuestions(Dictionary<string, Guid> s)
    {
        var q = new List<OnboardingQuestion>();
        q.AddRange(SectionA(s["A"]));
        q.AddRange(SectionB(s["B"]));
        q.AddRange(SectionC(s["C"]));
        q.AddRange(SectionD(s["D"]));
        q.AddRange(SectionE(s["E"]));
        q.AddRange(SectionF(s["F"]));
        q.AddRange(SectionG(s["G"]));
        q.AddRange(SectionH(s["H"]));
        q.AddRange(SectionI(s["I"]));
        q.AddRange(SectionJ(s["J"]));
        q.AddRange(SectionK(s["K"]));
        q.AddRange(SectionL(s["L"]));
        return q;
    }

    // Helper to create question
    private OnboardingQuestion Q(Guid sid, string code, string field, int order, OnboardingQuestionType type, bool req,
        string qEn, string qAr, string opts = "[]", string def = "") => new()
    {
        Id = Guid.NewGuid(), SectionId = sid, Code = code, FieldName = field, SortOrder = order,
        QuestionType = type, IsRequired = req, QuestionEn = qEn, QuestionAr = qAr,
        OptionsJson = opts, DefaultValue = def, IsActive = true
    };

    // Country options
    private const string CountryOpts = "[{\"value\":\"SA\",\"labelEn\":\"Saudi Arabia\",\"labelAr\":\"السعودية\"},{\"value\":\"AE\",\"labelEn\":\"UAE\",\"labelAr\":\"الإمارات\"},{\"value\":\"BH\",\"labelEn\":\"Bahrain\",\"labelAr\":\"البحرين\"},{\"value\":\"KW\",\"labelEn\":\"Kuwait\",\"labelAr\":\"الكويت\"},{\"value\":\"QA\",\"labelEn\":\"Qatar\",\"labelAr\":\"قطر\"},{\"value\":\"OM\",\"labelEn\":\"Oman\",\"labelAr\":\"عمان\"},{\"value\":\"OTHER\",\"labelEn\":\"Other\",\"labelAr\":\"أخرى\"}]";
    private const string OrgTypeOpts = "[{\"value\":\"enterprise\",\"labelEn\":\"Enterprise\",\"labelAr\":\"مؤسسة\"},{\"value\":\"sme\",\"labelEn\":\"SME\",\"labelAr\":\"صغيرة ومتوسطة\"},{\"value\":\"government\",\"labelEn\":\"Government\",\"labelAr\":\"حكومية\"},{\"value\":\"financial\",\"labelEn\":\"Financial\",\"labelAr\":\"مالية\"},{\"value\":\"fintech\",\"labelEn\":\"Fintech\",\"labelAr\":\"تقنية مالية\"},{\"value\":\"telecom\",\"labelEn\":\"Telecom\",\"labelAr\":\"اتصالات\"}]";
    private const string SectorOpts = "[{\"value\":\"banking\",\"labelEn\":\"Banking\",\"labelAr\":\"البنوك\"},{\"value\":\"insurance\",\"labelEn\":\"Insurance\",\"labelAr\":\"التأمين\"},{\"value\":\"capital_markets\",\"labelEn\":\"Capital Markets\",\"labelAr\":\"أسواق المال\"},{\"value\":\"healthcare\",\"labelEn\":\"Healthcare\",\"labelAr\":\"الرعاية الصحية\"},{\"value\":\"retail\",\"labelEn\":\"Retail\",\"labelAr\":\"التجزئة\"},{\"value\":\"energy\",\"labelEn\":\"Energy\",\"labelAr\":\"الطاقة\"},{\"value\":\"telecom\",\"labelEn\":\"Telecom\",\"labelAr\":\"الاتصالات\"}]";
    private const string LangOpts = "[{\"value\":\"en\",\"labelEn\":\"English\",\"labelAr\":\"الإنجليزية\"},{\"value\":\"ar\",\"labelEn\":\"Arabic\",\"labelAr\":\"العربية\"},{\"value\":\"bilingual\",\"labelEn\":\"Bilingual\",\"labelAr\":\"ثنائي اللغة\"}]";
    private const string DriverOpts = "[{\"value\":\"regulator_exam\",\"labelEn\":\"Regulator Exam\",\"labelAr\":\"فحص تنظيمي\"},{\"value\":\"internal_audit\",\"labelEn\":\"Internal Audit\",\"labelAr\":\"تدقيق داخلي\"},{\"value\":\"external_audit\",\"labelEn\":\"External Audit\",\"labelAr\":\"تدقيق خارجي\"},{\"value\":\"certification\",\"labelEn\":\"Certification\",\"labelAr\":\"شهادة\"},{\"value\":\"board_reporting\",\"labelEn\":\"Board Reporting\",\"labelAr\":\"تقارير مجلس الإدارة\"}]";
    private const string MaturityOpts = "[{\"value\":\"Foundation\",\"labelEn\":\"Foundation\",\"labelAr\":\"الأساس\"},{\"value\":\"AssuranceOps\",\"labelEn\":\"Assurance Ops\",\"labelAr\":\"عمليات التأكيد\"},{\"value\":\"ContinuousAssurance\",\"labelEn\":\"Continuous\",\"labelAr\":\"مستمر\"}]";
    private const string RegulatorOpts = "[{\"value\":\"SAMA\",\"labelEn\":\"SAMA\",\"labelAr\":\"ساما\"},{\"value\":\"CMA\",\"labelEn\":\"CMA\",\"labelAr\":\"هيئة السوق المالية\"},{\"value\":\"NCA\",\"labelEn\":\"NCA\",\"labelAr\":\"الهيئة الوطنية للأمن السيبراني\"},{\"value\":\"CCHI\",\"labelEn\":\"CCHI\",\"labelAr\":\"مجلس الضمان الصحي\"},{\"value\":\"CITC\",\"labelEn\":\"CITC\",\"labelAr\":\"هيئة الاتصالات\"},{\"value\":\"SDAIA\",\"labelEn\":\"SDAIA\",\"labelAr\":\"سدايا\"}]";
    private const string FrameworkOpts = "[{\"value\":\"SAMA_CSF\",\"labelEn\":\"SAMA CSF\",\"labelAr\":\"إطار ساما\"},{\"value\":\"NCA_ECC\",\"labelEn\":\"NCA ECC\",\"labelAr\":\"الضوابط الأساسية\"},{\"value\":\"PDPL\",\"labelEn\":\"PDPL\",\"labelAr\":\"حماية البيانات\"},{\"value\":\"ISO27001\",\"labelEn\":\"ISO 27001\",\"labelAr\":\"آيزو 27001\"},{\"value\":\"PCI_DSS\",\"labelEn\":\"PCI DSS\",\"labelAr\":\"PCI DSS\"}]";
    private const string DataTypeOpts = "[{\"value\":\"PII\",\"labelEn\":\"PII\",\"labelAr\":\"بيانات شخصية\"},{\"value\":\"PCI\",\"labelEn\":\"PCI\",\"labelAr\":\"بطاقات دفع\"},{\"value\":\"PHI\",\"labelEn\":\"PHI\",\"labelAr\":\"بيانات صحية\"},{\"value\":\"confidential\",\"labelEn\":\"Confidential\",\"labelAr\":\"سرية\"}]";
    private const string IdpOpts = "[{\"value\":\"azure_ad\",\"labelEn\":\"Azure AD\",\"labelAr\":\"Azure AD\"},{\"value\":\"okta\",\"labelEn\":\"Okta\",\"labelAr\":\"Okta\"},{\"value\":\"ping\",\"labelEn\":\"Ping\",\"labelAr\":\"Ping\"},{\"value\":\"other\",\"labelEn\":\"Other\",\"labelAr\":\"أخرى\"}]";
    private const string CloudOpts = "[{\"value\":\"aws\",\"labelEn\":\"AWS\",\"labelAr\":\"AWS\"},{\"value\":\"azure\",\"labelEn\":\"Azure\",\"labelAr\":\"Azure\"},{\"value\":\"gcp\",\"labelEn\":\"GCP\",\"labelAr\":\"GCP\"},{\"value\":\"on_prem\",\"labelEn\":\"On-Premises\",\"labelAr\":\"محلي\"}]";
    private const string OwnershipOpts = "[{\"value\":\"centralized\",\"labelEn\":\"Centralized\",\"labelAr\":\"مركزي\"},{\"value\":\"federated\",\"labelEn\":\"Federated\",\"labelAr\":\"موزع\"},{\"value\":\"hybrid\",\"labelEn\":\"Hybrid\",\"labelAr\":\"هجين\"}]";
    private const string CadenceOpts = "[{\"value\":\"weekly\",\"labelEn\":\"Weekly\",\"labelAr\":\"أسبوعي\"},{\"value\":\"monthly\",\"labelEn\":\"Monthly\",\"labelAr\":\"شهري\"},{\"value\":\"quarterly\",\"labelEn\":\"Quarterly\",\"labelAr\":\"ربع سنوي\"},{\"value\":\"annual\",\"labelEn\":\"Annual\",\"labelAr\":\"سنوي\"}]";
    private const string NotifOpts = "[{\"value\":\"email\",\"labelEn\":\"Email\",\"labelAr\":\"بريد\"},{\"value\":\"teams\",\"labelEn\":\"Teams\",\"labelAr\":\"Teams\"},{\"value\":\"both\",\"labelEn\":\"Both\",\"labelAr\":\"كلاهما\"}]";

    private List<OnboardingQuestion> SectionA(Guid s) => new()
    {
        Q(s, "A1", "OrganizationLegalNameEn", 1, OnboardingQuestionType.Text, true, "Organization Legal Name (EN)", "الاسم القانوني (إنجليزي)"),
        Q(s, "A2", "OrganizationLegalNameAr", 2, OnboardingQuestionType.Text, false, "Organization Legal Name (AR)", "الاسم القانوني (عربي)"),
        Q(s, "A3", "TradeName", 3, OnboardingQuestionType.Text, false, "Trade Name / Brand", "الاسم التجاري"),
        Q(s, "A4", "CountryOfIncorporation", 4, OnboardingQuestionType.SingleSelect, true, "Country of Incorporation", "بلد التأسيس", CountryOpts, "SA"),
        Q(s, "A5", "OperatingCountriesJson", 5, OnboardingQuestionType.MultiSelect, false, "Operating Countries", "دول العمليات", CountryOpts),
        Q(s, "A6", "PrimaryHqLocation", 6, OnboardingQuestionType.Text, false, "Primary HQ Location", "موقع المقر الرئيسي"),
        Q(s, "A7", "DefaultTimezone", 7, OnboardingQuestionType.Text, false, "Default Timezone", "المنطقة الزمنية", "[]", "Asia/Riyadh"),
        Q(s, "A8", "PrimaryLanguage", 8, OnboardingQuestionType.SingleSelect, false, "Primary Language", "اللغة الأساسية", LangOpts, "bilingual"),
        Q(s, "A9", "CorporateEmailDomainsJson", 9, OnboardingQuestionType.JsonArray, false, "Corporate Email Domains", "نطاقات البريد الإلكتروني"),
        Q(s, "A10", "OrganizationType", 10, OnboardingQuestionType.SingleSelect, true, "Organization Type", "نوع المنظمة", OrgTypeOpts),
        Q(s, "A11", "IndustrySector", 11, OnboardingQuestionType.SingleSelect, true, "Industry / Sector", "القطاع / الصناعة", SectorOpts),
        Q(s, "A12", "BusinessLinesJson", 12, OnboardingQuestionType.MultiSelect, false, "Business Lines", "خطوط الأعمال"),
        Q(s, "A13", "HasDataResidencyRequirement", 13, OnboardingQuestionType.Boolean, false, "Data Residency Requirement?", "متطلب إقامة البيانات؟")
    };

    private List<OnboardingQuestion> SectionB(Guid s) => new()
    {
        Q(s, "B14", "PrimaryDriver", 1, OnboardingQuestionType.SingleSelect, true, "Primary Driver", "الدافع الرئيسي", DriverOpts),
        Q(s, "B15", "TargetTimeline", 2, OnboardingQuestionType.Date, false, "Target Timeline", "الجدول الزمني المستهدف"),
        Q(s, "B16", "CurrentPainPointsJson", 3, OnboardingQuestionType.MultiSelect, false, "Current Pain Points", "نقاط الألم الحالية"),
        Q(s, "B17", "DesiredMaturity", 4, OnboardingQuestionType.SingleSelect, false, "Desired Maturity", "مستوى النضج", MaturityOpts, "Foundation"),
        Q(s, "B18", "ReportingAudienceJson", 5, OnboardingQuestionType.MultiSelect, false, "Reporting Audience", "جمهور التقارير")
    };

    private List<OnboardingQuestion> SectionC(Guid s) => new()
    {
        Q(s, "C19", "PrimaryRegulatorsJson", 1, OnboardingQuestionType.MultiSelect, false, "Primary Regulators", "الجهات التنظيمية الرئيسية", RegulatorOpts),
        Q(s, "C20", "SecondaryRegulatorsJson", 2, OnboardingQuestionType.MultiSelect, false, "Secondary Regulators", "الجهات التنظيمية الثانوية", RegulatorOpts),
        Q(s, "C21", "MandatoryFrameworksJson", 3, OnboardingQuestionType.MultiSelect, false, "Mandatory Frameworks", "الأطر الإلزامية", FrameworkOpts),
        Q(s, "C22", "OptionalFrameworksJson", 4, OnboardingQuestionType.MultiSelect, false, "Optional Frameworks", "الأطر الاختيارية", FrameworkOpts),
        Q(s, "C23", "InternalPoliciesJson", 5, OnboardingQuestionType.JsonArray, false, "Internal Policies", "السياسات الداخلية"),
        Q(s, "C24", "CertificationsHeldJson", 6, OnboardingQuestionType.MultiSelect, false, "Certifications Held", "الشهادات الحالية", FrameworkOpts),
        Q(s, "C25", "AuditScopeType", 7, OnboardingQuestionType.SingleSelect, false, "Audit Scope Type", "نوع نطاق التدقيق", "[{\"value\":\"enterprise\",\"labelEn\":\"Enterprise\",\"labelAr\":\"المؤسسة\"},{\"value\":\"business_unit\",\"labelEn\":\"Business Unit\",\"labelAr\":\"وحدة أعمال\"},{\"value\":\"system\",\"labelEn\":\"System\",\"labelAr\":\"النظام\"}]", "enterprise")
    };

    private List<OnboardingQuestion> SectionD(Guid s) => new()
    {
        Q(s, "D26", "InScopeLegalEntitiesJson", 1, OnboardingQuestionType.JsonArray, false, "In-Scope Legal Entities", "الكيانات القانونية ضمن النطاق"),
        Q(s, "D27", "InScopeBusinessUnitsJson", 2, OnboardingQuestionType.JsonArray, false, "In-Scope Business Units", "وحدات الأعمال ضمن النطاق"),
        Q(s, "D28", "InScopeSystemsJson", 3, OnboardingQuestionType.JsonArray, false, "In-Scope Systems", "الأنظمة ضمن النطاق"),
        Q(s, "D29", "InScopeProcessesJson", 4, OnboardingQuestionType.JsonArray, false, "In-Scope Processes", "العمليات ضمن النطاق"),
        Q(s, "D30", "InScopeEnvironments", 5, OnboardingQuestionType.SingleSelect, false, "In-Scope Environments", "البيئات ضمن النطاق", "[{\"value\":\"production\",\"labelEn\":\"Production\",\"labelAr\":\"الإنتاج\"},{\"value\":\"both\",\"labelEn\":\"Both\",\"labelAr\":\"كلاهما\"}]", "both"),
        Q(s, "D31", "InScopeLocationsJson", 6, OnboardingQuestionType.JsonArray, false, "In-Scope Locations", "المواقع ضمن النطاق"),
        Q(s, "D32", "SystemCriticalityTiersJson", 7, OnboardingQuestionType.JsonObject, false, "System Criticality Tiers", "مستويات أهمية الأنظمة"),
        Q(s, "D33", "ImportantBusinessServicesJson", 8, OnboardingQuestionType.JsonArray, false, "Important Business Services", "الخدمات التجارية المهمة"),
        Q(s, "D34", "ExclusionsJson", 9, OnboardingQuestionType.JsonArray, false, "Exclusions", "الاستثناءات")
    };

    private List<OnboardingQuestion> SectionE(Guid s) => new()
    {
        Q(s, "E35", "DataTypesProcessedJson", 1, OnboardingQuestionType.MultiSelect, true, "Data Types Processed", "أنواع البيانات المعالجة", DataTypeOpts),
        Q(s, "E36", "HasPaymentCardData", 2, OnboardingQuestionType.Boolean, false, "Payment Card Data?", "بيانات بطاقات الدفع؟"),
        Q(s, "E37", "HasCrossBorderDataTransfers", 3, OnboardingQuestionType.Boolean, false, "Cross-Border Data Transfers?", "نقل بيانات عبر الحدود؟"),
        Q(s, "E38", "CustomerVolumeTier", 4, OnboardingQuestionType.SingleSelect, false, "Customer Volume Tier", "مستوى حجم العملاء", "[{\"value\":\"small\",\"labelEn\":\"<10K\",\"labelAr\":\"أقل من 10 آلاف\"},{\"value\":\"medium\",\"labelEn\":\"10K-100K\",\"labelAr\":\"10-100 ألف\"},{\"value\":\"large\",\"labelEn\":\">100K\",\"labelAr\":\"أكثر من 100 ألف\"}]"),
        Q(s, "E39", "HasInternetFacingSystems", 5, OnboardingQuestionType.Boolean, false, "Internet-Facing Systems?", "أنظمة مواجهة للإنترنت؟"),
        Q(s, "E40", "HasThirdPartyDataProcessing", 6, OnboardingQuestionType.Boolean, false, "Third-Party Data Processing?", "معالجة بيانات طرف ثالث؟")
    };

    private List<OnboardingQuestion> SectionF(Guid s) => new()
    {
        Q(s, "F41", "IdentityProvider", 1, OnboardingQuestionType.SingleSelect, false, "Identity Provider", "موفر الهوية", IdpOpts),
        Q(s, "F42", "SsoEnabled", 2, OnboardingQuestionType.Boolean, false, "SSO Enabled?", "تسجيل دخول موحد؟"),
        Q(s, "F43", "ScimProvisioningAvailable", 3, OnboardingQuestionType.Boolean, false, "SCIM Provisioning?", "توفير SCIM؟"),
        Q(s, "F44", "ItsmPlatform", 4, OnboardingQuestionType.SingleSelect, false, "ITSM Platform", "منصة ITSM", "[{\"value\":\"servicenow\",\"labelEn\":\"ServiceNow\",\"labelAr\":\"ServiceNow\"},{\"value\":\"jira\",\"labelEn\":\"Jira\",\"labelAr\":\"Jira\"},{\"value\":\"other\",\"labelEn\":\"Other\",\"labelAr\":\"أخرى\"}]"),
        Q(s, "F45", "EvidenceRepository", 5, OnboardingQuestionType.SingleSelect, false, "Evidence Repository", "مستودع الأدلة", "[{\"value\":\"sharepoint\",\"labelEn\":\"SharePoint\",\"labelAr\":\"SharePoint\"},{\"value\":\"grc_vault\",\"labelEn\":\"GRC Vault\",\"labelAr\":\"خزنة GRC\"},{\"value\":\"other\",\"labelEn\":\"Other\",\"labelAr\":\"أخرى\"}]"),
        Q(s, "F46", "SiemPlatform", 6, OnboardingQuestionType.SingleSelect, false, "SIEM Platform", "منصة SIEM", "[{\"value\":\"sentinel\",\"labelEn\":\"Sentinel\",\"labelAr\":\"Sentinel\"},{\"value\":\"splunk\",\"labelEn\":\"Splunk\",\"labelAr\":\"Splunk\"},{\"value\":\"qradar\",\"labelEn\":\"QRadar\",\"labelAr\":\"QRadar\"},{\"value\":\"other\",\"labelEn\":\"Other\",\"labelAr\":\"أخرى\"}]"),
        Q(s, "F47", "VulnerabilityManagementTool", 7, OnboardingQuestionType.SingleSelect, false, "Vulnerability Management", "إدارة الثغرات", "[{\"value\":\"tenable\",\"labelEn\":\"Tenable\",\"labelAr\":\"Tenable\"},{\"value\":\"qualys\",\"labelEn\":\"Qualys\",\"labelAr\":\"Qualys\"},{\"value\":\"rapid7\",\"labelEn\":\"Rapid7\",\"labelAr\":\"Rapid7\"},{\"value\":\"other\",\"labelEn\":\"Other\",\"labelAr\":\"أخرى\"}]"),
        Q(s, "F48", "EdrPlatform", 8, OnboardingQuestionType.SingleSelect, false, "EDR Platform", "منصة EDR", "[{\"value\":\"defender\",\"labelEn\":\"Defender\",\"labelAr\":\"Defender\"},{\"value\":\"crowdstrike\",\"labelEn\":\"CrowdStrike\",\"labelAr\":\"CrowdStrike\"},{\"value\":\"other\",\"labelEn\":\"Other\",\"labelAr\":\"أخرى\"}]"),
        Q(s, "F49", "CloudProvidersJson", 9, OnboardingQuestionType.MultiSelect, false, "Cloud Providers", "موفرو السحابة", CloudOpts),
        Q(s, "F50", "ErpSystem", 10, OnboardingQuestionType.SingleSelect, false, "ERP System", "نظام ERP", "[{\"value\":\"sap\",\"labelEn\":\"SAP\",\"labelAr\":\"SAP\"},{\"value\":\"oracle\",\"labelEn\":\"Oracle\",\"labelAr\":\"Oracle\"},{\"value\":\"dynamics\",\"labelEn\":\"Dynamics\",\"labelAr\":\"Dynamics\"},{\"value\":\"other\",\"labelEn\":\"Other\",\"labelAr\":\"أخرى\"}]"),
        Q(s, "F51", "CmdbSource", 11, OnboardingQuestionType.Text, false, "CMDB Source", "مصدر CMDB"),
        Q(s, "F52", "CiCdTooling", 12, OnboardingQuestionType.SingleSelect, false, "CI/CD Tooling", "أدوات CI/CD", "[{\"value\":\"github\",\"labelEn\":\"GitHub\",\"labelAr\":\"GitHub\"},{\"value\":\"gitlab\",\"labelEn\":\"GitLab\",\"labelAr\":\"GitLab\"},{\"value\":\"azure_devops\",\"labelEn\":\"Azure DevOps\",\"labelAr\":\"Azure DevOps\"},{\"value\":\"other\",\"labelEn\":\"Other\",\"labelAr\":\"أخرى\"}]"),
        Q(s, "F53", "BackupDrTooling", 13, OnboardingQuestionType.Text, false, "Backup/DR Tooling", "أدوات النسخ الاحتياطي")
    };

    private List<OnboardingQuestion> SectionG(Guid s) => new()
    {
        Q(s, "G54", "ControlOwnershipApproach", 1, OnboardingQuestionType.SingleSelect, true, "Control Ownership Approach", "نهج ملكية الضوابط", OwnershipOpts, "hybrid"),
        Q(s, "G55", "DefaultControlOwnerTeam", 2, OnboardingQuestionType.Text, false, "Default Control Owner Team", "فريق مالك الضوابط الافتراضي"),
        Q(s, "G56", "ExceptionApproverRole", 3, OnboardingQuestionType.Text, false, "Exception Approver Role", "دور موافق الاستثناءات"),
        Q(s, "G57", "RegulatoryInterpretationApproverRole", 4, OnboardingQuestionType.Text, false, "Regulatory Interpretation Approver", "موافق التفسير التنظيمي"),
        Q(s, "G58", "ControlEffectivenessSignoffRole", 5, OnboardingQuestionType.Text, false, "Control Effectiveness Signoff", "اعتماد فعالية الضوابط"),
        Q(s, "G59", "InternalAuditStakeholder", 6, OnboardingQuestionType.Text, false, "Internal Audit Stakeholder", "صاحب المصلحة في التدقيق الداخلي"),
        Q(s, "G60", "RiskCommitteeCadence", 7, OnboardingQuestionType.SingleSelect, false, "Risk Committee Cadence", "تكرار لجنة المخاطر", CadenceOpts, "quarterly")
    };

    private List<OnboardingQuestion> SectionH(Guid s) => new()
    {
        Q(s, "H61", "OrgAdminsJson", 1, OnboardingQuestionType.JsonArray, true, "Organization Admins", "مسؤولو المنظمة"),
        Q(s, "H62", "CreateTeamsNow", 2, OnboardingQuestionType.Boolean, false, "Create Teams Now?", "إنشاء الفرق الآن؟"),
        Q(s, "H63", "TeamListJson", 3, OnboardingQuestionType.JsonArray, false, "Team List", "قائمة الفرق"),
        Q(s, "H64", "TeamMembersJson", 4, OnboardingQuestionType.JsonArray, false, "Team Members", "أعضاء الفريق"),
        Q(s, "H65", "SelectedRoleCatalogJson", 5, OnboardingQuestionType.MultiSelect, false, "Role Catalog", "كتالوج الأدوار", "[{\"value\":\"Control_Owner\",\"labelEn\":\"Control Owner\",\"labelAr\":\"مالك الضابط\"},{\"value\":\"Evidence_Custodian\",\"labelEn\":\"Evidence Custodian\",\"labelAr\":\"أمين الأدلة\"},{\"value\":\"Approver\",\"labelEn\":\"Approver\",\"labelAr\":\"المعتمد\"},{\"value\":\"Assessor\",\"labelEn\":\"Assessor\",\"labelAr\":\"المقيم\"},{\"value\":\"Viewer\",\"labelEn\":\"Viewer\",\"labelAr\":\"المشاهد\"}]"),
        Q(s, "H66", "RaciMappingNeeded", 6, OnboardingQuestionType.Boolean, false, "RACI Mapping Needed?", "هل تحتاج تخطيط RACI؟"),
        Q(s, "H67", "ApprovalGatesNeeded", 7, OnboardingQuestionType.Boolean, false, "Approval Gates Needed?", "هل تحتاج بوابات موافقة؟"),
        Q(s, "H68", "DelegationRulesJson", 8, OnboardingQuestionType.JsonArray, false, "Delegation Rules", "قواعد التفويض"),
        Q(s, "H69", "NotificationPreference", 9, OnboardingQuestionType.SingleSelect, false, "Notification Preference", "تفضيل الإشعارات", NotifOpts, "email"),
        Q(s, "H70", "EscalationDaysOverdue", 10, OnboardingQuestionType.Number, false, "Escalation Days Overdue", "أيام التصعيد بعد التأخير", "[]", "3")
    };

    private List<OnboardingQuestion> SectionI(Guid s) => new()
    {
        Q(s, "I71", "EvidenceFrequencyDefaultsJson", 1, OnboardingQuestionType.JsonObject, false, "Evidence Frequency Defaults", "افتراضيات تكرار الأدلة"),
        Q(s, "I72", "AccessReviewsFrequency", 2, OnboardingQuestionType.SingleSelect, false, "Access Reviews Frequency", "تكرار مراجعات الوصول", CadenceOpts, "quarterly"),
        Q(s, "I73", "VulnerabilityPatchReviewFrequency", 3, OnboardingQuestionType.SingleSelect, false, "Vulnerability Review Frequency", "تكرار مراجعة الثغرات", CadenceOpts, "weekly"),
        Q(s, "I74", "BackupReviewFrequency", 4, OnboardingQuestionType.SingleSelect, false, "Backup Review Frequency", "تكرار مراجعة النسخ الاحتياطي", CadenceOpts, "monthly"),
        Q(s, "I75", "DrExerciseCadence", 5, OnboardingQuestionType.SingleSelect, false, "DR Exercise Cadence", "تكرار تمارين التعافي", CadenceOpts, "annual"),
        Q(s, "I76", "IncidentTabletopCadence", 6, OnboardingQuestionType.SingleSelect, false, "Incident Tabletop Cadence", "تكرار تمارين الحوادث", CadenceOpts, "annual"),
        Q(s, "I77", "EvidenceSlaSubmitDays", 7, OnboardingQuestionType.Number, false, "Evidence SLA (Days)", "اتفاقية مستوى الخدمة للأدلة", "[]", "5"),
        Q(s, "I78", "RemediationSlaJson", 8, OnboardingQuestionType.JsonObject, false, "Remediation SLA", "اتفاقية مستوى الخدمة للمعالجة"),
        Q(s, "I79", "ExceptionExpiryDays", 9, OnboardingQuestionType.Number, false, "Exception Expiry (Days)", "انتهاء الاستثناء (أيام)", "[]", "90"),
        Q(s, "I80", "AuditRequestHandling", 10, OnboardingQuestionType.SingleSelect, false, "Audit Request Handling", "معالجة طلبات التدقيق", "[{\"value\":\"single_queue\",\"labelEn\":\"Single Queue\",\"labelAr\":\"طابور واحد\"},{\"value\":\"per_domain\",\"labelEn\":\"Per Domain\",\"labelAr\":\"لكل مجال\"}]", "single_queue")
    };

    private List<OnboardingQuestion> SectionJ(Guid s) => new()
    {
        Q(s, "J81", "EvidenceNamingConventionRequired", 1, OnboardingQuestionType.Boolean, false, "Evidence Naming Convention?", "اصطلاح تسمية الأدلة؟"),
        Q(s, "J82", "EvidenceStorageLocationJson", 2, OnboardingQuestionType.JsonObject, false, "Evidence Storage Locations", "مواقع تخزين الأدلة"),
        Q(s, "J83", "EvidenceRetentionYears", 3, OnboardingQuestionType.Number, false, "Evidence Retention (Years)", "احتفاظ الأدلة (سنوات)", "[]", "7"),
        Q(s, "J84", "EvidenceAccessRulesJson", 4, OnboardingQuestionType.JsonObject, false, "Evidence Access Rules", "قواعد الوصول للأدلة"),
        Q(s, "J85", "AcceptableEvidenceTypesJson", 5, OnboardingQuestionType.MultiSelect, false, "Acceptable Evidence Types", "أنواع الأدلة المقبولة", "[{\"value\":\"reports\",\"labelEn\":\"Reports\",\"labelAr\":\"تقارير\"},{\"value\":\"logs\",\"labelEn\":\"Logs\",\"labelAr\":\"سجلات\"},{\"value\":\"screenshots\",\"labelEn\":\"Screenshots\",\"labelAr\":\"لقطات شاشة\"},{\"value\":\"signed_pdfs\",\"labelEn\":\"Signed PDFs\",\"labelAr\":\"PDF موقعة\"}]"),
        Q(s, "J86", "SamplingGuidanceJson", 6, OnboardingQuestionType.JsonObject, false, "Sampling Guidance", "إرشادات العينات"),
        Q(s, "J87", "ConfidentialEvidenceEncryption", 7, OnboardingQuestionType.Boolean, false, "Confidential Evidence Encryption?", "تشفير الأدلة السرية؟")
    };

    private List<OnboardingQuestion> SectionK(Guid s) => new()
    {
        Q(s, "K88", "AdoptDefaultBaseline", 1, OnboardingQuestionType.Boolean, false, "Adopt Default Baseline?", "اعتماد الأساس الافتراضي؟"),
        Q(s, "K89", "SelectedOverlaysJson", 2, OnboardingQuestionType.MultiSelect, false, "Select Overlays", "اختر الطبقات", "[{\"value\":\"jurisdiction\",\"labelEn\":\"Jurisdiction\",\"labelAr\":\"الاختصاص\"},{\"value\":\"sector\",\"labelEn\":\"Sector\",\"labelAr\":\"القطاع\"},{\"value\":\"pci\",\"labelEn\":\"PCI\",\"labelAr\":\"PCI\"},{\"value\":\"cloud\",\"labelEn\":\"Cloud\",\"labelAr\":\"السحابة\"}]"),
        Q(s, "K90", "HasClientSpecificControls", 3, OnboardingQuestionType.Boolean, false, "Client-Specific Controls?", "ضوابط خاصة بالعميل؟")
    };

    private List<OnboardingQuestion> SectionL(Guid s) => new()
    {
        Q(s, "L91", "SuccessMetricsTop3Json", 1, OnboardingQuestionType.MultiSelect, false, "Success Metrics (Top 3)", "مقاييس النجاح (أهم 3)", "[{\"value\":\"fewer_audit_hours\",\"labelEn\":\"Fewer Audit Hours\",\"labelAr\":\"ساعات تدقيق أقل\"},{\"value\":\"faster_evidence\",\"labelEn\":\"Faster Evidence\",\"labelAr\":\"أدلة أسرع\"},{\"value\":\"reduced_findings\",\"labelEn\":\"Reduced Findings\",\"labelAr\":\"نتائج أقل\"},{\"value\":\"improved_sla\",\"labelEn\":\"Improved SLA\",\"labelAr\":\"تحسين SLA\"}]"),
        Q(s, "L92", "BaselineAuditPrepHoursPerMonth", 2, OnboardingQuestionType.Number, false, "Baseline Audit Prep Hours/Month", "ساعات إعداد التدقيق الأساسية/شهر"),
        Q(s, "L93", "BaselineRemediationClosureDays", 3, OnboardingQuestionType.Number, false, "Baseline Remediation Closure Days", "أيام إغلاق المعالجة الأساسية"),
        Q(s, "L94", "BaselineOverdueControlsPerMonth", 4, OnboardingQuestionType.Number, false, "Baseline Overdue Controls/Month", "الضوابط المتأخرة الأساسية/شهر"),
        Q(s, "L95", "TargetImprovementJson", 5, OnboardingQuestionType.JsonObject, false, "Target Improvement %", "نسبة التحسين المستهدفة"),
        Q(s, "L96", "PilotScopeJson", 6, OnboardingQuestionType.JsonArray, false, "Pilot Scope", "نطاق التجربة")
    };
}
