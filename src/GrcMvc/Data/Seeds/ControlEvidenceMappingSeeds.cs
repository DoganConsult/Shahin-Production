using GrcMvc.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace GrcMvc.Data.Seeds;

/// <summary>
/// Seeds control-to-evidence mappings
/// Links controls to their required evidence packs based on control domain/category
/// </summary>
public static class ControlEvidenceMappingSeeds
{
    public static async Task SeedAsync(GrcDbContext context, ILogger logger)
    {
        // Check if evidence packs already exist
        var existingCount = await context.EvidencePacks.CountAsync();
        if (existingCount >= 20)
        {
            logger.LogInformation("Evidence packs already seeded ({Count})", existingCount);
            return;
        }

        logger.LogInformation("Seeding evidence packs...");

        // Create base evidence packs
        var evidencePacks = CreateBaseEvidencePacks();
        context.EvidencePacks.AddRange(evidencePacks);
        await context.SaveChangesAsync();
        
        logger.LogInformation("Created {Count} base evidence packs", evidencePacks.Count);
    }

    private static List<EvidencePack> CreateBaseEvidencePacks()
    {
        return new List<EvidencePack>
        {
            // Governance & Organization
            CreateEvidencePack("EP_GOV_001", "Governance Documentation Pack",
                new[] { "Information Security Policy", "Security Organization Chart", "Roles & Responsibilities", "Board Meeting Minutes" },
                "Annual"),
            
            CreateEvidencePack("EP_GOV_002", "Policy Management Pack",
                new[] { "Policy Approval Records", "Policy Version History", "Policy Distribution Logs", "Acknowledgment Records" },
                "Annual"),

            // Identity & Access Management
            CreateEvidencePack("EP_IAM_001", "Access Control Pack",
                new[] { "User Access List", "Privileged Account List", "Access Review Reports", "Access Request Forms" },
                "Quarterly"),
            
            CreateEvidencePack("EP_IAM_002", "Authentication Pack",
                new[] { "MFA Configuration", "Password Policy Settings", "Authentication Logs", "Session Timeout Settings" },
                "Quarterly"),

            CreateEvidencePack("EP_IAM_003", "Privileged Access Pack",
                new[] { "PAM System Config", "Privileged Session Logs", "Emergency Access Logs", "Admin Account Review" },
                "Monthly"),

            // Asset Management
            CreateEvidencePack("EP_ASM_001", "Asset Inventory Pack",
                new[] { "Hardware Inventory", "Software Inventory", "Data Asset Register", "Asset Classification" },
                "Quarterly"),

            CreateEvidencePack("EP_ASM_002", "Data Classification Pack",
                new[] { "Data Classification Policy", "Data Flow Diagrams", "Data Handling Procedures", "Classification Labels" },
                "Annual"),

            // Human Resources Security
            CreateEvidencePack("EP_HRS_001", "Personnel Security Pack",
                new[] { "Background Check Records", "Security Agreement", "NDA", "Termination Checklist" },
                "Continuous"),

            CreateEvidencePack("EP_HRS_002", "Security Awareness Pack",
                new[] { "Training Attendance", "Training Materials", "Quiz Results", "Completion Certificates" },
                "Annual"),

            // Physical Security
            CreateEvidencePack("EP_PHY_001", "Physical Access Pack",
                new[] { "Access Card Logs", "Visitor Logs", "CCTV Footage Samples", "Physical Access Reviews" },
                "Quarterly"),

            CreateEvidencePack("EP_PHY_002", "Environmental Controls Pack",
                new[] { "UPS Test Records", "HVAC Monitoring", "Fire Suppression Tests", "Environmental Monitoring Logs" },
                "Quarterly"),

            // Operations Security
            CreateEvidencePack("EP_OPS_001", "Change Management Pack",
                new[] { "Change Request Forms", "CAB Meeting Minutes", "Change Approval Records", "Post-Implementation Review" },
                "Monthly"),

            CreateEvidencePack("EP_OPS_002", "Patch Management Pack",
                new[] { "Patch Status Report", "Patch Testing Records", "Patch Deployment Logs", "Vulnerability Scans" },
                "Monthly"),

            CreateEvidencePack("EP_OPS_003", "Backup & Recovery Pack",
                new[] { "Backup Schedule", "Backup Success Logs", "Restore Test Records", "Offsite Storage Records" },
                "Monthly"),

            CreateEvidencePack("EP_OPS_004", "Logging & Monitoring Pack",
                new[] { "Log Retention Settings", "SIEM Dashboard", "Alert Rules Config", "Log Review Reports" },
                "Monthly"),

            // Network Security
            CreateEvidencePack("EP_NET_001", "Network Security Pack",
                new[] { "Firewall Rules", "Network Diagrams", "Segmentation Evidence", "IDS/IPS Logs" },
                "Quarterly"),

            CreateEvidencePack("EP_NET_002", "Encryption Pack",
                new[] { "TLS/SSL Certificates", "Encryption Standards", "Key Management Records", "Data-at-Rest Encryption" },
                "Quarterly"),

            // Vulnerability Management
            CreateEvidencePack("EP_VUL_001", "Vulnerability Assessment Pack",
                new[] { "Vulnerability Scan Reports", "Penetration Test Reports", "Remediation Tracking", "Risk Acceptance" },
                "Quarterly"),

            CreateEvidencePack("EP_VUL_002", "Secure Development Pack",
                new[] { "SAST Scan Results", "DAST Scan Results", "Code Review Records", "Security Requirements" },
                "Continuous"),

            // Incident Management
            CreateEvidencePack("EP_INC_001", "Incident Response Pack",
                new[] { "Incident Reports", "Investigation Records", "Root Cause Analysis", "Lessons Learned" },
                "Continuous"),

            CreateEvidencePack("EP_INC_002", "Incident Readiness Pack",
                new[] { "IR Plan", "IR Team Roster", "Tabletop Exercise Records", "Communication Templates" },
                "Annual"),

            // Business Continuity
            CreateEvidencePack("EP_BCM_001", "Business Continuity Pack",
                new[] { "BCP Document", "BIA Results", "Recovery Procedures", "BC Test Results" },
                "Annual"),

            CreateEvidencePack("EP_BCM_002", "Disaster Recovery Pack",
                new[] { "DR Plan", "DR Test Results", "Failover Records", "RTO/RPO Evidence" },
                "Annual"),

            // Compliance & Audit
            CreateEvidencePack("EP_CMP_001", "Compliance Monitoring Pack",
                new[] { "Compliance Dashboard", "Exception Register", "Remediation Plans", "Regulatory Correspondence" },
                "Quarterly"),

            CreateEvidencePack("EP_CMP_002", "Audit Evidence Pack",
                new[] { "Audit Reports", "Management Response", "Finding Remediation", "Audit Follow-up" },
                "Annual"),

            // Third Party Risk
            CreateEvidencePack("EP_TPR_001", "Vendor Management Pack",
                new[] { "Vendor Inventory", "Risk Assessment", "Due Diligence Records", "SLA Monitoring" },
                "Annual"),

            CreateEvidencePack("EP_TPR_002", "Third Party Assurance Pack",
                new[] { "SOC 2 Reports", "ISO Certificates", "Penetration Test Reports", "Questionnaire Responses" },
                "Annual"),

            // Data Protection
            CreateEvidencePack("EP_DPR_001", "Privacy Compliance Pack",
                new[] { "Privacy Policy", "Consent Records", "DPIA Records", "Subject Request Logs" },
                "Quarterly"),

            CreateEvidencePack("EP_DPR_002", "Data Retention Pack",
                new[] { "Retention Schedule", "Disposal Records", "Archive Logs", "Legal Hold Records" },
                "Annual")
        };
    }

    private static EvidencePack CreateEvidencePack(string code, string name, string[] items, string frequency)
    {
        return new EvidencePack
        {
            Id = Guid.NewGuid(),
            PackCode = code,
            Name = name,
            NameAr = GetArabicPackName(code),
            Description = $"Standard evidence pack for {name}",
            DescriptionAr = GetArabicPackDescription(code),
            EvidenceItemsJson = System.Text.Json.JsonSerializer.Serialize(items),
            EvidenceItemsArJson = System.Text.Json.JsonSerializer.Serialize(GetArabicEvidenceItems(code)),
            RequiredFrequency = frequency,
            RetentionMonths = 84, // 7 years
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "System"
        };
    }

    private static string GetArabicPackName(string code) => code switch
    {
        "EP_GOV_001" => "حزمة وثائق الحوكمة",
        "EP_GOV_002" => "حزمة إدارة السياسات",
        "EP_IAM_001" => "حزمة التحكم في الوصول",
        "EP_IAM_002" => "حزمة المصادقة",
        "EP_IAM_003" => "حزمة الوصول الممتاز",
        "EP_ASM_001" => "حزمة جرد الأصول",
        "EP_ASM_002" => "حزمة تصنيف البيانات",
        "EP_HRS_001" => "حزمة أمن الموارد البشرية",
        "EP_HRS_002" => "حزمة التوعية الأمنية",
        "EP_PHY_001" => "حزمة الوصول المادي",
        "EP_PHY_002" => "حزمة الضوابط البيئية",
        "EP_OPS_001" => "حزمة إدارة التغييرات",
        "EP_OPS_002" => "حزمة إدارة التحديثات",
        "EP_OPS_003" => "حزمة النسخ الاحتياطي والاستعادة",
        "EP_OPS_004" => "حزمة التسجيل والمراقبة",
        "EP_NET_001" => "حزمة أمن الشبكات",
        "EP_NET_002" => "حزمة التشفير",
        "EP_VUL_001" => "حزمة تقييم الثغرات",
        "EP_VUL_002" => "حزمة التطوير الآمن",
        "EP_INC_001" => "حزمة الاستجابة للحوادث",
        "EP_INC_002" => "حزمة الجاهزية للحوادث",
        "EP_BCM_001" => "حزمة استمرارية الأعمال",
        "EP_BCM_002" => "حزمة التعافي من الكوارث",
        "EP_CMP_001" => "حزمة مراقبة الامتثال",
        "EP_CMP_002" => "حزمة شواهد التدقيق",
        "EP_TPR_001" => "حزمة إدارة الموردين",
        "EP_TPR_002" => "حزمة ضمانات الأطراف الثالثة",
        "EP_DPR_001" => "حزمة الامتثال للخصوصية",
        "EP_DPR_002" => "حزمة الاحتفاظ بالبيانات",
        _ => "حزمة الشواهد"
    };

    private static string GetArabicPackDescription(string code) => code switch
    {
        "EP_GOV_001" => "الوثائق المطلوبة لإثبات الامتثال لضوابط الحوكمة",
        "EP_GOV_002" => "الشواهد المتعلقة بإدارة ومراجعة السياسات",
        "EP_IAM_001" => "شواهد التحكم في الوصول ومراجعة الصلاحيات",
        "EP_IAM_002" => "شواهد آليات المصادقة وإعداداتها",
        "EP_IAM_003" => "شواهد إدارة الحسابات ذات الصلاحيات الممتازة",
        "EP_ASM_001" => "شواهد جرد الأصول المعلوماتية والتقنية",
        "EP_ASM_002" => "شواهد تصنيف البيانات والتعامل معها",
        "EP_HRS_001" => "شواهد أمن الموارد البشرية والفحص الأمني",
        "EP_HRS_002" => "شواهد برامج التوعية والتدريب الأمني",
        "EP_PHY_001" => "شواهد التحكم في الوصول المادي",
        "EP_PHY_002" => "شواهد الضوابط البيئية والمرافق",
        "EP_OPS_001" => "شواهد عمليات إدارة التغييرات",
        "EP_OPS_002" => "شواهد إدارة التحديثات والترقيعات الأمنية",
        "EP_OPS_003" => "شواهد النسخ الاحتياطي واختبار الاستعادة",
        "EP_OPS_004" => "شواهد تسجيل الأحداث والمراقبة الأمنية",
        "EP_NET_001" => "شواهد أمن الشبكات وجدران الحماية",
        "EP_NET_002" => "شواهد التشفير وإدارة المفاتيح",
        "EP_VUL_001" => "شواهد فحص الثغرات واختبار الاختراق",
        "EP_VUL_002" => "شواهد التطوير الآمن ومراجعة الكود",
        "EP_INC_001" => "شواهد الاستجابة للحوادث والتحقيق",
        "EP_INC_002" => "شواهد الجاهزية للحوادث والتمارين",
        "EP_BCM_001" => "شواهد استمرارية الأعمال وتحليل التأثير",
        "EP_BCM_002" => "شواهد التعافي من الكوارث والاختبارات",
        "EP_CMP_001" => "شواهد مراقبة الامتثال والتقارير",
        "EP_CMP_002" => "شواهد التدقيق والمعالجة",
        "EP_TPR_001" => "شواهد إدارة مخاطر الموردين",
        "EP_TPR_002" => "شواهد ضمانات وتقارير الأطراف الثالثة",
        "EP_DPR_001" => "شواهد الامتثال لنظام حماية البيانات",
        "EP_DPR_002" => "شواهد الاحتفاظ بالبيانات والإتلاف",
        _ => "الشواهد المطلوبة للامتثال"
    };

    private static string[] GetArabicEvidenceItems(string code) => code switch
    {
        "EP_GOV_001" => new[] { "سياسة أمن المعلومات", "الهيكل التنظيمي للأمن", "الأدوار والمسؤوليات", "محاضر اجتماعات مجلس الإدارة" },
        "EP_GOV_002" => new[] { "سجلات اعتماد السياسات", "تاريخ إصدارات السياسات", "سجلات توزيع السياسات", "سجلات الإقرار" },
        "EP_IAM_001" => new[] { "قائمة صلاحيات المستخدمين", "قائمة الحسابات الممتازة", "تقارير مراجعة الصلاحيات", "نماذج طلب الصلاحيات" },
        "EP_IAM_002" => new[] { "إعدادات المصادقة متعددة العوامل", "إعدادات سياسة كلمات المرور", "سجلات المصادقة", "إعدادات انتهاء الجلسة" },
        "EP_IAM_003" => new[] { "إعدادات نظام PAM", "سجلات الجلسات الممتازة", "سجلات الوصول الطارئ", "مراجعة حسابات المدراء" },
        "EP_ASM_001" => new[] { "جرد الأجهزة", "جرد البرمجيات", "سجل أصول البيانات", "تصنيف الأصول" },
        "EP_ASM_002" => new[] { "سياسة تصنيف البيانات", "مخططات تدفق البيانات", "إجراءات التعامل مع البيانات", "تسميات التصنيف" },
        "EP_HRS_001" => new[] { "سجلات الفحص الأمني", "اتفاقيات الأمان", "اتفاقية عدم الإفصاح", "قائمة إنهاء الخدمة" },
        "EP_HRS_002" => new[] { "سجلات الحضور للتدريب", "مواد التدريب", "نتائج الاختبارات", "شهادات الإتمام" },
        "EP_PHY_001" => new[] { "سجلات بطاقات الدخول", "سجلات الزوار", "عينات كاميرات المراقبة", "مراجعات الوصول المادي" },
        "EP_PHY_002" => new[] { "سجلات اختبار UPS", "مراقبة التكييف", "اختبارات إطفاء الحريق", "سجلات المراقبة البيئية" },
        "EP_OPS_001" => new[] { "نماذج طلب التغيير", "محاضر اجتماعات CAB", "سجلات الموافقة على التغييرات", "مراجعة ما بعد التنفيذ" },
        "EP_OPS_002" => new[] { "تقرير حالة التحديثات", "سجلات اختبار التحديثات", "سجلات نشر التحديثات", "فحوصات الثغرات" },
        "EP_OPS_003" => new[] { "جدول النسخ الاحتياطي", "سجلات نجاح النسخ", "سجلات اختبار الاستعادة", "سجلات التخزين الخارجي" },
        "EP_OPS_004" => new[] { "إعدادات حفظ السجلات", "لوحة تحكم SIEM", "إعدادات قواعد التنبيه", "تقارير مراجعة السجلات" },
        "EP_NET_001" => new[] { "قواعد جدار الحماية", "مخططات الشبكة", "شواهد التجزئة", "سجلات IDS/IPS" },
        "EP_NET_002" => new[] { "شهادات TLS/SSL", "معايير التشفير", "سجلات إدارة المفاتيح", "تشفير البيانات المخزنة" },
        "EP_VUL_001" => new[] { "تقارير فحص الثغرات", "تقارير اختبار الاختراق", "تتبع المعالجة", "قبول المخاطر" },
        "EP_VUL_002" => new[] { "نتائج SAST", "نتائج DAST", "سجلات مراجعة الكود", "متطلبات الأمان" },
        "EP_INC_001" => new[] { "تقارير الحوادث", "سجلات التحقيق", "تحليل السبب الجذري", "الدروس المستفادة" },
        "EP_INC_002" => new[] { "خطة الاستجابة للحوادث", "قائمة فريق الاستجابة", "سجلات تمارين الطاولة", "قوالب الاتصال" },
        "EP_BCM_001" => new[] { "وثيقة BCP", "نتائج تحليل التأثير", "إجراءات الاستعادة", "نتائج اختبار BC" },
        "EP_BCM_002" => new[] { "خطة DR", "نتائج اختبار DR", "سجلات الانتقال", "شواهد RTO/RPO" },
        "EP_CMP_001" => new[] { "لوحة تحكم الامتثال", "سجل الاستثناءات", "خطط المعالجة", "المراسلات التنظيمية" },
        "EP_CMP_002" => new[] { "تقارير التدقيق", "استجابة الإدارة", "معالجة الملاحظات", "متابعة التدقيق" },
        "EP_TPR_001" => new[] { "جرد الموردين", "تقييم المخاطر", "سجلات العناية الواجبة", "مراقبة SLA" },
        "EP_TPR_002" => new[] { "تقارير SOC 2", "شهادات ISO", "تقارير اختبار الاختراق", "ردود الاستبيانات" },
        "EP_DPR_001" => new[] { "سياسة الخصوصية", "سجلات الموافقة", "سجلات DPIA", "سجلات طلبات أصحاب البيانات" },
        "EP_DPR_002" => new[] { "جدول الاحتفاظ", "سجلات الإتلاف", "سجلات الأرشفة", "سجلات الحجز القانوني" },
        _ => new[] { "الشواهد المطلوبة" }
    };
}
