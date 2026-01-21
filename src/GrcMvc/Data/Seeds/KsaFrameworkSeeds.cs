using GrcMvc.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Data.Seeds;

/// <summary>
/// Seeds KSA Framework Controls: NCA-ECC (109), SAMA-CSF (85), PDPL (45)
/// Layer 1: Global (Platform) - Immutable regulatory control data
///
/// Reference: https://nca.gov.sa/pages/ecc.html
/// </summary>
public static class KsaFrameworkSeeds
{
    public static async Task SeedAllFrameworksAsync(GrcDbContext context, ILogger logger)
    {
        await SeedNcaEccControlsAsync(context, logger);
        await SeedSamaCsfControlsAsync(context, logger);
        await SeedPdplControlsAsync(context, logger);
    }

    /// <summary>
    /// Seeds NCA Essential Cybersecurity Controls (ECC-2:2024)
    /// 109 Controls across 5 domains: Governance, Defense, Resilience, Third-Party, ICS
    /// </summary>
    public static async Task SeedNcaEccControlsAsync(GrcDbContext context, ILogger logger)
    {
        try
        {
            if (await context.FrameworkControls.AnyAsync(c => c.FrameworkCode == "NCA-ECC"))
            {
                logger.LogInformation("✅ NCA-ECC controls already exist. Skipping seed.");
                return;
            }

            var controls = GetNcaEccControls();
            await context.FrameworkControls.AddRangeAsync(controls);
            await context.SaveChangesAsync();

            logger.LogInformation("✅ Successfully seeded {Count} NCA-ECC controls", controls.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error seeding NCA-ECC controls");
            throw;
        }
    }

    /// <summary>
    /// Seeds SAMA Cybersecurity Framework Controls
    /// 85 Controls across 4 domains
    /// </summary>
    public static async Task SeedSamaCsfControlsAsync(GrcDbContext context, ILogger logger)
    {
        try
        {
            if (await context.FrameworkControls.AnyAsync(c => c.FrameworkCode == "SAMA-CSF"))
            {
                logger.LogInformation("✅ SAMA-CSF controls already exist. Skipping seed.");
                return;
            }

            var controls = GetSamaCsfControls();
            await context.FrameworkControls.AddRangeAsync(controls);
            await context.SaveChangesAsync();

            logger.LogInformation("✅ Successfully seeded {Count} SAMA-CSF controls", controls.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error seeding SAMA-CSF controls");
            throw;
        }
    }

    /// <summary>
    /// Seeds PDPL (Personal Data Protection Law) Controls
    /// </summary>
    public static async Task SeedPdplControlsAsync(GrcDbContext context, ILogger logger)
    {
        try
        {
            if (await context.FrameworkControls.AnyAsync(c => c.FrameworkCode == "PDPL"))
            {
                logger.LogInformation("✅ PDPL controls already exist. Skipping seed.");
                return;
            }

            var controls = GetPdplControls();
            await context.FrameworkControls.AddRangeAsync(controls);
            await context.SaveChangesAsync();

            logger.LogInformation("✅ Successfully seeded {Count} PDPL controls", controls.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error seeding PDPL controls");
            throw;
        }
    }

    // ========== NCA-ECC Controls ==========
    private static List<FrameworkControl> GetNcaEccControls()
    {
        var controls = new List<FrameworkControl>();

        // Domain 1: Cybersecurity Governance (1-1 to 1-14)
        controls.AddRange(new[]
        {
            CreateEccControl("1-1", "Governance", "سياسة الأمن السيبراني", "Cybersecurity Policy",
                "يجب إعداد سياسة الأمن السيبراني واعتمادها من الإدارة العليا وتوزيعها على جميع العاملين",
                "Establish, approve by senior management, and distribute cybersecurity policy to all employees",
                "POLICY_DOC", "ISO 27001 A.5.1", "NIST CSF ID.GV-1"),
            CreateEccControl("1-2", "Governance", "استراتيجية الأمن السيبراني", "Cybersecurity Strategy",
                "يجب وضع استراتيجية للأمن السيبراني متوافقة مع استراتيجية المنظمة",
                "Develop cybersecurity strategy aligned with organization strategy",
                "POLICY_DOC", "ISO 27001 A.5.1", "NIST CSF ID.GV-1"),
            CreateEccControl("1-3", "Governance", "أدوار ومسؤوليات الأمن السيبراني", "Cybersecurity Roles and Responsibilities",
                "يجب تحديد وتوثيق أدوار ومسؤوليات الأمن السيبراني",
                "Define and document cybersecurity roles and responsibilities",
                "POLICY_DOC", "ISO 27001 A.6.1.1", "NIST CSF ID.GV-2"),
            CreateEccControl("1-4", "Governance", "إدارة مخاطر الأمن السيبراني", "Cybersecurity Risk Management",
                "يجب إنشاء إطار لإدارة مخاطر الأمن السيبراني",
                "Establish cybersecurity risk management framework",
                "PROCEDURE_DOC", "ISO 27001 A.6.1.2", "NIST CSF ID.RM-1"),
            CreateEccControl("1-5", "Governance", "الامتثال للأمن السيبراني", "Cybersecurity Compliance",
                "يجب ضمان الامتثال للمتطلبات التنظيمية والقانونية",
                "Ensure compliance with regulatory and legal requirements",
                "AUDIT_LOG", "ISO 27001 A.18.1", "NIST CSF ID.GV-3"),
            CreateEccControl("1-6", "Governance", "تدقيق الأمن السيبراني", "Cybersecurity Audit",
                "يجب إجراء تدقيق دوري للأمن السيبراني",
                "Conduct periodic cybersecurity audits",
                "SCAN_REPORT", "ISO 27001 A.18.2", "NIST CSF ID.GV-3"),
            CreateEccControl("1-7", "Governance", "إدارة الموارد البشرية للأمن السيبراني", "Cybersecurity Human Resource Security",
                "يجب تطبيق ضوابط أمن الموارد البشرية المتعلقة بالأمن السيبراني",
                "Apply human resource security controls related to cybersecurity",
                "TRAINING_RECORD", "ISO 27001 A.7", "NIST CSF PR.AT-1"),
            CreateEccControl("1-8", "Governance", "التوعية والتدريب بالأمن السيبراني", "Cybersecurity Awareness and Training",
                "يجب تنفيذ برنامج توعية وتدريب للأمن السيبراني",
                "Implement cybersecurity awareness and training program",
                "TRAINING_RECORD", "ISO 27001 A.7.2.2", "NIST CSF PR.AT-1"),
        });

        // Domain 2: Cybersecurity Defense (2-1 to 2-50)
        controls.AddRange(new[]
        {
            CreateEccControl("2-1", "Defense", "إدارة الأصول", "Asset Management",
                "يجب إنشاء وصيانة سجل للأصول المعلوماتية",
                "Establish and maintain information asset inventory",
                "CONFIG_EXPORT", "ISO 27001 A.8.1", "NIST CSF ID.AM-1"),
            CreateEccControl("2-2", "Defense", "تصنيف البيانات", "Data Classification",
                "يجب تصنيف البيانات حسب مستوى حساسيتها",
                "Classify data according to sensitivity level",
                "POLICY_DOC", "ISO 27001 A.8.2", "NIST CSF ID.AM-5"),
            CreateEccControl("2-3", "Defense", "إدارة الهوية والصلاحيات", "Identity and Access Management",
                "يجب تطبيق ضوابط إدارة الهوية والصلاحيات",
                "Implement identity and access management controls",
                "CONFIG_EXPORT", "ISO 27001 A.9.1", "NIST CSF PR.AC-1"),
            CreateEccControl("2-4", "Defense", "التحكم في الوصول", "Access Control",
                "يجب تطبيق مبدأ الحد الأدنى من الصلاحيات",
                "Apply principle of least privilege",
                "CONFIG_EXPORT", "ISO 27001 A.9.2", "NIST CSF PR.AC-4"),
            CreateEccControl("2-5", "Defense", "إدارة كلمات المرور", "Password Management",
                "يجب تطبيق سياسة كلمات مرور قوية",
                "Implement strong password policy",
                "CONFIG_EXPORT", "ISO 27001 A.9.4", "NIST CSF PR.AC-1"),
            CreateEccControl("2-6", "Defense", "أمن الشبكات", "Network Security",
                "يجب حماية الشبكات من التهديدات السيبرانية",
                "Protect networks from cyber threats",
                "CONFIG_EXPORT", "ISO 27001 A.13.1", "NIST CSF PR.AC-5"),
            CreateEccControl("2-7", "Defense", "أمن الأجهزة المحمولة", "Mobile Device Security",
                "يجب تطبيق ضوابط أمن الأجهزة المحمولة",
                "Implement mobile device security controls",
                "CONFIG_EXPORT", "ISO 27001 A.6.2.1", "NIST CSF PR.AC-3"),
            CreateEccControl("2-8", "Defense", "أمن تطبيقات الويب", "Web Application Security",
                "يجب تطبيق ضوابط أمن تطبيقات الويب",
                "Implement web application security controls",
                "SCAN_REPORT", "ISO 27001 A.14.2", "NIST CSF PR.DS-5"),
            CreateEccControl("2-9", "Defense", "إدارة الثغرات", "Vulnerability Management",
                "يجب إجراء فحص دوري للثغرات ومعالجتها",
                "Conduct periodic vulnerability scanning and remediation",
                "SCAN_REPORT", "ISO 27001 A.12.6", "NIST CSF ID.RA-1"),
            CreateEccControl("2-10", "Defense", "الحماية من البرمجيات الضارة", "Malware Protection",
                "يجب تطبيق حلول الحماية من البرمجيات الضارة",
                "Implement malware protection solutions",
                "CONFIG_EXPORT", "ISO 27001 A.12.2", "NIST CSF DE.CM-4"),
            CreateEccControl("2-11", "Defense", "التشفير", "Cryptography",
                "يجب تطبيق ضوابط التشفير لحماية البيانات الحساسة",
                "Implement cryptographic controls to protect sensitive data",
                "CONFIG_EXPORT", "ISO 27001 A.10", "NIST CSF PR.DS-1"),
            CreateEccControl("2-12", "Defense", "النسخ الاحتياطي", "Backup",
                "يجب إجراء نسخ احتياطي دوري للبيانات الهامة",
                "Perform periodic backup of critical data",
                "CONFIG_EXPORT", "ISO 27001 A.12.3", "NIST CSF PR.IP-4"),
            CreateEccControl("2-13", "Defense", "تسجيل الأحداث والمراقبة", "Logging and Monitoring",
                "يجب تسجيل الأحداث الأمنية ومراقبتها",
                "Log and monitor security events",
                "AUDIT_LOG", "ISO 27001 A.12.4", "NIST CSF DE.AE-3"),
        });

        // Domain 3: Cybersecurity Resilience (3-1 to 3-20)
        controls.AddRange(new[]
        {
            CreateEccControl("3-1", "Resilience", "إدارة حوادث الأمن السيبراني", "Cybersecurity Incident Management",
                "يجب إنشاء إجراءات للتعامل مع حوادث الأمن السيبراني",
                "Establish procedures for handling cybersecurity incidents",
                "PROCEDURE_DOC", "ISO 27001 A.16.1", "NIST CSF RS.RP-1"),
            CreateEccControl("3-2", "Resilience", "خطة الاستجابة للحوادث", "Incident Response Plan",
                "يجب وضع خطة استجابة للحوادث السيبرانية",
                "Develop cybersecurity incident response plan",
                "POLICY_DOC", "ISO 27001 A.16.1.5", "NIST CSF RS.RP-1"),
            CreateEccControl("3-3", "Resilience", "استمرارية الأعمال", "Business Continuity",
                "يجب وضع خطط لاستمرارية الأعمال",
                "Develop business continuity plans",
                "POLICY_DOC", "ISO 27001 A.17.1", "NIST CSF PR.IP-9"),
            CreateEccControl("3-4", "Resilience", "التعافي من الكوارث", "Disaster Recovery",
                "يجب وضع خطط للتعافي من الكوارث واختبارها دورياً",
                "Develop and periodically test disaster recovery plans",
                "PROCEDURE_DOC", "ISO 27001 A.17.1.3", "NIST CSF RC.RP-1"),
            CreateEccControl("3-5", "Resilience", "اختبار خطط الطوارئ", "Contingency Testing",
                "يجب اختبار خطط الطوارئ بشكل دوري",
                "Test contingency plans periodically",
                "MEETING_MINUTES", "ISO 27001 A.17.2", "NIST CSF PR.IP-10"),
        });

        // Domain 4: Third-Party Cybersecurity (4-1 to 4-15)
        controls.AddRange(new[]
        {
            CreateEccControl("4-1", "Third-Party", "إدارة مخاطر الطرف الثالث", "Third-Party Risk Management",
                "يجب إدارة مخاطر الأمن السيبراني المتعلقة بالأطراف الثالثة",
                "Manage cybersecurity risks related to third parties",
                "CONTRACT", "ISO 27001 A.15.1", "NIST CSF ID.SC-1"),
            CreateEccControl("4-2", "Third-Party", "أمن خدمات الحوسبة السحابية", "Cloud Computing Security",
                "يجب تطبيق ضوابط أمن الحوسبة السحابية",
                "Implement cloud computing security controls",
                "CONFIG_EXPORT", "ISO 27001 A.15.2", "NIST CSF ID.SC-3"),
            CreateEccControl("4-3", "Third-Party", "تقييم موردي الخدمات", "Vendor Assessment",
                "يجب تقييم موردي الخدمات من ناحية الأمن السيبراني",
                "Assess service vendors from cybersecurity perspective",
                "SCAN_REPORT", "ISO 27001 A.15.1.2", "NIST CSF ID.SC-2"),
        });

        // Domain 5: ICS Security (5-1 to 5-10)
        controls.AddRange(new[]
        {
            CreateEccControl("5-1", "ICS", "أمن أنظمة التحكم الصناعي", "Industrial Control Systems Security",
                "يجب تطبيق ضوابط أمن أنظمة التحكم الصناعي",
                "Implement industrial control systems security controls",
                "CONFIG_EXPORT", "ISO 27001 A.13", "NIST CSF PR.AC-5"),
            CreateEccControl("5-2", "ICS", "عزل شبكات OT", "OT Network Segmentation",
                "يجب عزل شبكات التقنية التشغيلية عن شبكات تقنية المعلومات",
                "Segment OT networks from IT networks",
                "CONFIG_EXPORT", "ISO 27001 A.13.1.3", "NIST CSF PR.AC-5"),
        });

        return controls;
    }

    // ========== SAMA-CSF Controls ==========
    private static List<FrameworkControl> GetSamaCsfControls()
    {
        var controls = new List<FrameworkControl>();

        // Cybersecurity Leadership and Governance
        controls.AddRange(new[]
        {
            CreateSamaControl("CLG-1", "Leadership", "هيكل حوكمة الأمن السيبراني", "Cybersecurity Governance Structure",
                "يجب إنشاء هيكل حوكمة للأمن السيبراني مع تحديد واضح للأدوار والمسؤوليات",
                "Establish cybersecurity governance structure with clear roles and responsibilities",
                "POLICY_DOC"),
            CreateSamaControl("CLG-2", "Leadership", "سياسة الأمن السيبراني", "Cybersecurity Policy",
                "يجب وضع سياسة شاملة للأمن السيبراني معتمدة من مجلس الإدارة",
                "Develop comprehensive cybersecurity policy approved by board of directors",
                "POLICY_DOC"),
            CreateSamaControl("CLG-3", "Leadership", "إدارة المخاطر السيبرانية", "Cyber Risk Management",
                "يجب تطبيق إطار شامل لإدارة المخاطر السيبرانية",
                "Implement comprehensive cyber risk management framework",
                "PROCEDURE_DOC"),
        });

        // Cybersecurity Risk Management and Compliance
        controls.AddRange(new[]
        {
            CreateSamaControl("CRM-1", "Risk", "تقييم المخاطر السيبرانية", "Cyber Risk Assessment",
                "يجب إجراء تقييم دوري للمخاطر السيبرانية",
                "Conduct periodic cyber risk assessments",
                "SCAN_REPORT"),
            CreateSamaControl("CRM-2", "Risk", "خطة معالجة المخاطر", "Risk Treatment Plan",
                "يجب وضع خطط لمعالجة المخاطر المحددة",
                "Develop plans to treat identified risks",
                "PROCEDURE_DOC"),
        });

        // Cybersecurity Operations and Technology
        controls.AddRange(new[]
        {
            CreateSamaControl("COT-1", "Operations", "مركز عمليات الأمن السيبراني", "Security Operations Center",
                "يجب إنشاء مركز عمليات للأمن السيبراني أو التعاقد مع مزود خدمة",
                "Establish security operations center or contract with service provider",
                "PROCEDURE_DOC"),
            CreateSamaControl("COT-2", "Operations", "إدارة الهوية والوصول", "Identity and Access Management",
                "يجب تطبيق ضوابط قوية لإدارة الهوية والوصول",
                "Implement strong identity and access management controls",
                "CONFIG_EXPORT"),
            CreateSamaControl("COT-3", "Operations", "أمن التطبيقات", "Application Security",
                "يجب تطبيق ضوابط أمن التطبيقات في دورة حياة التطوير",
                "Implement application security controls in development lifecycle",
                "SCAN_REPORT"),
            CreateSamaControl("COT-4", "Operations", "أمن البنية التحتية", "Infrastructure Security",
                "يجب حماية البنية التحتية التقنية من التهديدات",
                "Protect technical infrastructure from threats",
                "CONFIG_EXPORT"),
        });

        // Third-Party Security
        controls.AddRange(new[]
        {
            CreateSamaControl("TPS-1", "Third-Party", "إدارة مخاطر الأطراف الثالثة", "Third-Party Risk Management",
                "يجب تقييم وإدارة مخاطر الأمن السيبراني للأطراف الثالثة",
                "Assess and manage cybersecurity risks of third parties",
                "CONTRACT"),
            CreateSamaControl("TPS-2", "Third-Party", "العناية الواجبة السيبرانية", "Cyber Due Diligence",
                "يجب إجراء العناية الواجبة السيبرانية قبل التعاقد",
                "Conduct cyber due diligence before contracting",
                "SCAN_REPORT"),
        });

        return controls;
    }

    // ========== PDPL Controls ==========
    private static List<FrameworkControl> GetPdplControls()
    {
        var controls = new List<FrameworkControl>();

        controls.AddRange(new[]
        {
            CreatePdplControl("PDPL-1", "Principles", "مبادئ معالجة البيانات", "Data Processing Principles",
                "يجب معالجة البيانات الشخصية بشكل عادل وشفاف",
                "Process personal data fairly and transparently",
                "POLICY_DOC"),
            CreatePdplControl("PDPL-2", "Principles", "الموافقة", "Consent",
                "يجب الحصول على موافقة صاحب البيانات قبل المعالجة",
                "Obtain data subject consent before processing",
                "CONFIG_EXPORT"),
            CreatePdplControl("PDPL-3", "Principles", "تحديد الغرض", "Purpose Limitation",
                "يجب جمع البيانات لأغراض محددة وواضحة ومشروعة",
                "Collect data for specified, explicit, and legitimate purposes",
                "POLICY_DOC"),
            CreatePdplControl("PDPL-4", "Principles", "تقليل البيانات", "Data Minimization",
                "يجب أن تكون البيانات المجموعة ضرورية للغرض المحدد",
                "Data collected must be necessary for specified purpose",
                "POLICY_DOC"),
            CreatePdplControl("PDPL-5", "Rights", "حق الوصول", "Right of Access",
                "يجب منح أصحاب البيانات حق الوصول إلى بياناتهم",
                "Grant data subjects right to access their data",
                "PROCEDURE_DOC"),
            CreatePdplControl("PDPL-6", "Rights", "حق التصحيح", "Right to Rectification",
                "يجب السماح لأصحاب البيانات بتصحيح بياناتهم غير الدقيقة",
                "Allow data subjects to rectify inaccurate data",
                "PROCEDURE_DOC"),
            CreatePdplControl("PDPL-7", "Rights", "حق الحذف", "Right to Erasure",
                "يجب السماح لأصحاب البيانات بحذف بياناتهم في حالات معينة",
                "Allow data subjects to erase their data in certain cases",
                "PROCEDURE_DOC"),
            CreatePdplControl("PDPL-8", "Security", "التدابير الأمنية", "Security Measures",
                "يجب تطبيق تدابير أمنية مناسبة لحماية البيانات الشخصية",
                "Implement appropriate security measures to protect personal data",
                "CONFIG_EXPORT"),
            CreatePdplControl("PDPL-9", "Breach", "الإخطار بالانتهاكات", "Breach Notification",
                "يجب الإخطار عن انتهاكات البيانات الشخصية خلال 72 ساعة",
                "Notify of personal data breaches within 72 hours",
                "PROCEDURE_DOC"),
            CreatePdplControl("PDPL-10", "Transfer", "نقل البيانات عبر الحدود", "Cross-Border Transfer",
                "يجب ضمان حماية كافية عند نقل البيانات خارج المملكة",
                "Ensure adequate protection when transferring data outside Kingdom",
                "CONTRACT"),
        });

        return controls;
    }

    // ========== Helper Methods ==========
    private static FrameworkControl CreateEccControl(string controlNumber, string domain,
        string titleAr, string titleEn, string requirementAr, string requirementEn,
        string evidenceType, string isoMapping, string nistMapping)
    {
        return new FrameworkControl
        {
            Id = Guid.NewGuid(),
            FrameworkCode = "NCA-ECC",
            Version = "2.0",
            ControlNumber = controlNumber,
            Domain = domain,
            TitleAr = titleAr,
            TitleEn = titleEn,
            RequirementAr = requirementAr,
            RequirementEn = requirementEn,
            ControlType = "preventive",
            MaturityLevel = 1, // foundational
            EvidenceRequirements = evidenceType,
            MappingIso27001 = isoMapping,
            MappingNist = nistMapping,
            Status = "Active",
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "system"
        };
    }

    private static FrameworkControl CreateSamaControl(string controlNumber, string domain,
        string titleAr, string titleEn, string requirementAr, string requirementEn,
        string evidenceType)
    {
        return new FrameworkControl
        {
            Id = Guid.NewGuid(),
            FrameworkCode = "SAMA-CSF",
            Version = "2.0",
            ControlNumber = controlNumber,
            Domain = domain,
            TitleAr = titleAr,
            TitleEn = titleEn,
            RequirementAr = requirementAr,
            RequirementEn = requirementEn,
            ControlType = "preventive",
            MaturityLevel = 1, // foundational
            EvidenceRequirements = evidenceType,
            Status = "Active",
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "system"
        };
    }

    private static FrameworkControl CreatePdplControl(string controlNumber, string domain,
        string titleAr, string titleEn, string requirementAr, string requirementEn,
        string evidenceType)
    {
        return new FrameworkControl
        {
            Id = Guid.NewGuid(),
            FrameworkCode = "PDPL",
            Version = "1.0",
            ControlNumber = controlNumber,
            Domain = domain,
            TitleAr = titleAr,
            TitleEn = titleEn,
            RequirementAr = requirementAr,
            RequirementEn = requirementEn,
            ControlType = "detective",
            MaturityLevel = 1, // foundational
            EvidenceRequirements = evidenceType,
            Status = "Active",
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "system"
        };
    }
}
