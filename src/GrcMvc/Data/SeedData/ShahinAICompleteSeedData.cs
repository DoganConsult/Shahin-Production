using GrcMvc.Models.Entities;

namespace GrcMvc.Data.SeedData;

/// <summary>
/// Complete Seed Data for Shahin-AI Platform
/// Includes: Evidence Packs, KRIs, SoD Rules, CCM Tests, Agents, Governance Rhythm
/// </summary>
public static class ShahinAICompleteSeedData
{
    #region Universal Evidence Packs (10-15 Packs)

    public static List<UniversalEvidencePack> GetUniversalEvidencePacks()
    {
        return new List<UniversalEvidencePack>
        {
            new() { Id = Guid.NewGuid(), PackCode = "IAM", Name = "Identity & Access Management", NameAr = "إدارة الهوية والوصول", ControlFamily = "IAM", IconClass = "fa-user-shield", DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), PackCode = "LOG", Name = "Logging & Monitoring", NameAr = "التسجيل والمراقبة", ControlFamily = "Logging", IconClass = "fa-file-alt", DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), PackCode = "VUL", Name = "Vulnerability & Patch Management", NameAr = "إدارة الثغرات والتحديثات", ControlFamily = "Vulnerability", IconClass = "fa-shield-alt", DisplayOrder = 3 },
            new() { Id = Guid.NewGuid(), PackCode = "CHG", Name = "Change Management", NameAr = "إدارة التغيير", ControlFamily = "Change", IconClass = "fa-code-branch", DisplayOrder = 4 },
            new() { Id = Guid.NewGuid(), PackCode = "BCP", Name = "Backup & Recovery", NameAr = "النسخ الاحتياطي والاستعادة", ControlFamily = "Backup", IconClass = "fa-database", DisplayOrder = 5 },
            new() { Id = Guid.NewGuid(), PackCode = "DRP", Name = "Disaster Recovery & BCP", NameAr = "التعافي من الكوارث واستمرارية الأعمال", ControlFamily = "DR", IconClass = "fa-life-ring", DisplayOrder = 6 },
            new() { Id = Guid.NewGuid(), PackCode = "INC", Name = "Incident Response", NameAr = "الاستجابة للحوادث", ControlFamily = "Incident", IconClass = "fa-exclamation-triangle", DisplayOrder = 7 },
            new() { Id = Guid.NewGuid(), PackCode = "TPR", Name = "Third-Party & Vendor Risk", NameAr = "مخاطر الأطراف الثالثة", ControlFamily = "ThirdParty", IconClass = "fa-handshake", DisplayOrder = 8 },
            new() { Id = Guid.NewGuid(), PackCode = "PRI", Name = "Privacy & Data Protection", NameAr = "الخصوصية وحماية البيانات", ControlFamily = "Privacy", IconClass = "fa-user-lock", DisplayOrder = 9 },
            new() { Id = Guid.NewGuid(), PackCode = "SDL", Name = "Secure Development Lifecycle", NameAr = "دورة التطوير الآمن", ControlFamily = "SDLC", IconClass = "fa-code", DisplayOrder = 10 },
            new() { Id = Guid.NewGuid(), PackCode = "GOV", Name = "Governance & Policy", NameAr = "الحوكمة والسياسات", ControlFamily = "Governance", IconClass = "fa-landmark", DisplayOrder = 11 },
            new() { Id = Guid.NewGuid(), PackCode = "NET", Name = "Network Security", NameAr = "أمن الشبكات", ControlFamily = "Network", IconClass = "fa-network-wired", DisplayOrder = 12 },
            new() { Id = Guid.NewGuid(), PackCode = "ERP", Name = "ERP Financial Controls", NameAr = "ضوابط ERP المالية", ControlFamily = "ERP", IconClass = "fa-calculator", DisplayOrder = 13 },
            new() { Id = Guid.NewGuid(), PackCode = "PCI", Name = "PCI DSS Controls", NameAr = "ضوابط PCI DSS", ControlFamily = "PCI", IconClass = "fa-credit-card", DisplayOrder = 14 },
            new() { Id = Guid.NewGuid(), PackCode = "CLD", Name = "Cloud Security", NameAr = "أمن السحابة", ControlFamily = "Cloud", IconClass = "fa-cloud", DisplayOrder = 15 }
        };
    }

    public static List<UniversalEvidencePackItem> GetEvidencePackItems(List<UniversalEvidencePack> packs)
    {
        var items = new List<UniversalEvidencePackItem>();
        var packDict = packs.ToDictionary(p => p.PackCode, p => p.Id);

        // IAM Pack Items
        items.AddRange(new[]
        {
            new UniversalEvidencePackItem { Id = Guid.NewGuid(), PackId = packDict["IAM"], ItemCode = "IAM-001", Name = "User Provisioning Process", NameAr = "عملية إنشاء المستخدمين", EvidenceType = "Report", Frequency = "Quarterly", IsMandatory = true, DisplayOrder = 1 },
            new UniversalEvidencePackItem { Id = Guid.NewGuid(), PackId = packDict["IAM"], ItemCode = "IAM-002", Name = "Access Review Report", NameAr = "تقرير مراجعة الوصول", EvidenceType = "Report", Frequency = "Quarterly", IsMandatory = true, DisplayOrder = 2 },
            new UniversalEvidencePackItem { Id = Guid.NewGuid(), PackId = packDict["IAM"], ItemCode = "IAM-003", Name = "Privileged Access List", NameAr = "قائمة الوصول المميز", EvidenceType = "Extract", Frequency = "Monthly", IsMandatory = true, DisplayOrder = 3 },
            new UniversalEvidencePackItem { Id = Guid.NewGuid(), PackId = packDict["IAM"], ItemCode = "IAM-004", Name = "MFA Enrollment Report", NameAr = "تقرير تسجيل MFA", EvidenceType = "Report", Frequency = "Monthly", IsMandatory = true, DisplayOrder = 4 },
            new UniversalEvidencePackItem { Id = Guid.NewGuid(), PackId = packDict["IAM"], ItemCode = "IAM-005", Name = "JML Process Evidence", NameAr = "أدلة عملية JML", EvidenceType = "Sample", Frequency = "Quarterly", IsMandatory = true, DisplayOrder = 5 },
            new UniversalEvidencePackItem { Id = Guid.NewGuid(), PackId = packDict["IAM"], ItemCode = "IAM-006", Name = "Dormant Account Report", NameAr = "تقرير الحسابات الخاملة", EvidenceType = "Report", Frequency = "Monthly", IsMandatory = false, DisplayOrder = 6 }
        });

        // LOG Pack Items
        items.AddRange(new[]
        {
            new UniversalEvidencePackItem { Id = Guid.NewGuid(), PackId = packDict["LOG"], ItemCode = "LOG-001", Name = "SIEM Configuration", NameAr = "تكوين SIEM", EvidenceType = "Configuration", Frequency = "Annual", IsMandatory = true, DisplayOrder = 1 },
            new UniversalEvidencePackItem { Id = Guid.NewGuid(), PackId = packDict["LOG"], ItemCode = "LOG-002", Name = "Log Source Inventory", NameAr = "جرد مصادر السجلات", EvidenceType = "Report", Frequency = "Quarterly", IsMandatory = true, DisplayOrder = 2 },
            new UniversalEvidencePackItem { Id = Guid.NewGuid(), PackId = packDict["LOG"], ItemCode = "LOG-003", Name = "Alert Rules List", NameAr = "قائمة قواعد التنبيه", EvidenceType = "Extract", Frequency = "Quarterly", IsMandatory = true, DisplayOrder = 3 },
            new UniversalEvidencePackItem { Id = Guid.NewGuid(), PackId = packDict["LOG"], ItemCode = "LOG-004", Name = "Log Retention Evidence", NameAr = "أدلة الاحتفاظ بالسجلات", EvidenceType = "Screenshot", Frequency = "Annual", IsMandatory = true, DisplayOrder = 4 }
        });

        // VUL Pack Items
        items.AddRange(new[]
        {
            new UniversalEvidencePackItem { Id = Guid.NewGuid(), PackId = packDict["VUL"], ItemCode = "VUL-001", Name = "Vulnerability Scan Report", NameAr = "تقرير فحص الثغرات", EvidenceType = "Report", Frequency = "Monthly", IsMandatory = true, DisplayOrder = 1 },
            new UniversalEvidencePackItem { Id = Guid.NewGuid(), PackId = packDict["VUL"], ItemCode = "VUL-002", Name = "Patch Compliance Report", NameAr = "تقرير امتثال التحديثات", EvidenceType = "Report", Frequency = "Monthly", IsMandatory = true, DisplayOrder = 2 },
            new UniversalEvidencePackItem { Id = Guid.NewGuid(), PackId = packDict["VUL"], ItemCode = "VUL-003", Name = "Penetration Test Report", NameAr = "تقرير اختبار الاختراق", EvidenceType = "Report", Frequency = "Annual", IsMandatory = true, DisplayOrder = 3 },
            new UniversalEvidencePackItem { Id = Guid.NewGuid(), PackId = packDict["VUL"], ItemCode = "VUL-004", Name = "Remediation Tickets", NameAr = "تذاكر المعالجة", EvidenceType = "Sample", Frequency = "Monthly", IsMandatory = true, DisplayOrder = 4 }
        });

        // CHG Pack Items
        items.AddRange(new[]
        {
            new UniversalEvidencePackItem { Id = Guid.NewGuid(), PackId = packDict["CHG"], ItemCode = "CHG-001", Name = "Change Management Policy", NameAr = "سياسة إدارة التغيير", EvidenceType = "Policy", Frequency = "Annual", IsMandatory = true, DisplayOrder = 1 },
            new UniversalEvidencePackItem { Id = Guid.NewGuid(), PackId = packDict["CHG"], ItemCode = "CHG-002", Name = "CAB Meeting Minutes", NameAr = "محاضر اجتماعات CAB", EvidenceType = "Sample", Frequency = "Monthly", IsMandatory = true, DisplayOrder = 2 },
            new UniversalEvidencePackItem { Id = Guid.NewGuid(), PackId = packDict["CHG"], ItemCode = "CHG-003", Name = "Change Request Samples", NameAr = "عينات طلبات التغيير", EvidenceType = "Sample", Frequency = "Quarterly", IsMandatory = true, DisplayOrder = 3 },
            new UniversalEvidencePackItem { Id = Guid.NewGuid(), PackId = packDict["CHG"], ItemCode = "CHG-004", Name = "Emergency Change Log", NameAr = "سجل التغييرات الطارئة", EvidenceType = "Log", Frequency = "Quarterly", IsMandatory = true, DisplayOrder = 4 }
        });

        // BCP Pack Items
        items.AddRange(new[]
        {
            new UniversalEvidencePackItem { Id = Guid.NewGuid(), PackId = packDict["BCP"], ItemCode = "BCP-001", Name = "Backup Schedule", NameAr = "جدول النسخ الاحتياطي", EvidenceType = "Configuration", Frequency = "Annual", IsMandatory = true, DisplayOrder = 1 },
            new UniversalEvidencePackItem { Id = Guid.NewGuid(), PackId = packDict["BCP"], ItemCode = "BCP-002", Name = "Backup Success Report", NameAr = "تقرير نجاح النسخ الاحتياطي", EvidenceType = "Report", Frequency = "Monthly", IsMandatory = true, DisplayOrder = 2 },
            new UniversalEvidencePackItem { Id = Guid.NewGuid(), PackId = packDict["BCP"], ItemCode = "BCP-003", Name = "Restore Test Results", NameAr = "نتائج اختبار الاستعادة", EvidenceType = "Report", Frequency = "Quarterly", IsMandatory = true, DisplayOrder = 3 }
        });

        // DRP Pack Items
        items.AddRange(new[]
        {
            new UniversalEvidencePackItem { Id = Guid.NewGuid(), PackId = packDict["DRP"], ItemCode = "DRP-001", Name = "DR Plan Document", NameAr = "وثيقة خطة التعافي", EvidenceType = "Policy", Frequency = "Annual", IsMandatory = true, DisplayOrder = 1 },
            new UniversalEvidencePackItem { Id = Guid.NewGuid(), PackId = packDict["DRP"], ItemCode = "DRP-002", Name = "DR Test Report", NameAr = "تقرير اختبار التعافي", EvidenceType = "Report", Frequency = "Annual", IsMandatory = true, DisplayOrder = 2 },
            new UniversalEvidencePackItem { Id = Guid.NewGuid(), PackId = packDict["DRP"], ItemCode = "DRP-003", Name = "RTO/RPO Evidence", NameAr = "أدلة RTO/RPO", EvidenceType = "Report", Frequency = "Annual", IsMandatory = true, DisplayOrder = 3 },
            new UniversalEvidencePackItem { Id = Guid.NewGuid(), PackId = packDict["DRP"], ItemCode = "DRP-004", Name = "Tabletop Exercise Report", NameAr = "تقرير تمرين المحاكاة", EvidenceType = "Report", Frequency = "Annual", IsMandatory = true, DisplayOrder = 4 }
        });

        // INC Pack Items
        items.AddRange(new[]
        {
            new UniversalEvidencePackItem { Id = Guid.NewGuid(), PackId = packDict["INC"], ItemCode = "INC-001", Name = "Incident Response Policy", NameAr = "سياسة الاستجابة للحوادث", EvidenceType = "Policy", Frequency = "Annual", IsMandatory = true, DisplayOrder = 1 },
            new UniversalEvidencePackItem { Id = Guid.NewGuid(), PackId = packDict["INC"], ItemCode = "INC-002", Name = "Incident Log", NameAr = "سجل الحوادث", EvidenceType = "Log", Frequency = "Quarterly", IsMandatory = true, DisplayOrder = 2 },
            new UniversalEvidencePackItem { Id = Guid.NewGuid(), PackId = packDict["INC"], ItemCode = "INC-003", Name = "Post-Incident Review", NameAr = "مراجعة ما بعد الحادث", EvidenceType = "Report", Frequency = "Quarterly", IsMandatory = true, DisplayOrder = 3 }
        });

        // TPR Pack Items
        items.AddRange(new[]
        {
            new UniversalEvidencePackItem { Id = Guid.NewGuid(), PackId = packDict["TPR"], ItemCode = "TPR-001", Name = "Vendor Inventory", NameAr = "جرد الموردين", EvidenceType = "Report", Frequency = "Annual", IsMandatory = true, DisplayOrder = 1 },
            new UniversalEvidencePackItem { Id = Guid.NewGuid(), PackId = packDict["TPR"], ItemCode = "TPR-002", Name = "Vendor Risk Assessments", NameAr = "تقييمات مخاطر الموردين", EvidenceType = "Report", Frequency = "Annual", IsMandatory = true, DisplayOrder = 2 },
            new UniversalEvidencePackItem { Id = Guid.NewGuid(), PackId = packDict["TPR"], ItemCode = "TPR-003", Name = "SOC 2 Reports", NameAr = "تقارير SOC 2", EvidenceType = "Report", Frequency = "Annual", IsMandatory = true, DisplayOrder = 3 },
            new UniversalEvidencePackItem { Id = Guid.NewGuid(), PackId = packDict["TPR"], ItemCode = "TPR-004", Name = "Contract Security Clauses", NameAr = "بنود الأمان التعاقدية", EvidenceType = "Sample", Frequency = "Annual", IsMandatory = false, DisplayOrder = 4 }
        });

        return items;
    }

    #endregion

    #region Standard KRIs/KPIs

    public static List<RiskIndicatorSeedInfo> GetStandardKRIs()
    {
        return new List<RiskIndicatorSeedInfo>
        {
            // Cyber KRIs
            new() { Code = "KRI-VUL-001", Name = "Critical Vulnerabilities Open > 30 Days", NameAr = "ثغرات حرجة مفتوحة > 30 يوم", Category = "Cyber", Type = "KRI", Unit = "Count", Target = 0, Warning = 3, Critical = 10, Direction = "LowerIsBetter", Frequency = "Daily" },
            new() { Code = "KRI-VUL-002", Name = "Patch SLA Compliance", NameAr = "امتثال SLA للتحديثات", Category = "Cyber", Type = "KRI", Unit = "Percentage", Target = 95, Warning = 90, Critical = 80, Direction = "HigherIsBetter", Frequency = "Weekly" },
            new() { Code = "KRI-IAM-001", Name = "Access Review Completion Rate", NameAr = "معدل إكمال مراجعة الوصول", Category = "Cyber", Type = "KRI", Unit = "Percentage", Target = 100, Warning = 95, Critical = 85, Direction = "HigherIsBetter", Frequency = "Quarterly" },
            new() { Code = "KRI-IAM-002", Name = "Privileged Accounts Without Review", NameAr = "حسابات مميزة بدون مراجعة", Category = "Cyber", Type = "KRI", Unit = "Count", Target = 0, Warning = 5, Critical = 15, Direction = "LowerIsBetter", Frequency = "Monthly" },
            new() { Code = "KRI-IAM-003", Name = "MFA Coverage", NameAr = "تغطية MFA", Category = "Cyber", Type = "KPI", Unit = "Percentage", Target = 100, Warning = 95, Critical = 90, Direction = "HigherIsBetter", Frequency = "Monthly" },

            // Operational KRIs
            new() { Code = "KRI-BCP-001", Name = "Backup Success Rate", NameAr = "معدل نجاح النسخ الاحتياطي", Category = "Continuity", Type = "KRI", Unit = "Percentage", Target = 99, Warning = 97, Critical = 95, Direction = "HigherIsBetter", Frequency = "Daily" },
            new() { Code = "KRI-BCP-002", Name = "DR Test Pass Rate", NameAr = "معدل نجاح اختبار التعافي", Category = "Continuity", Type = "KRI", Unit = "Percentage", Target = 100, Warning = 90, Critical = 80, Direction = "HigherIsBetter", Frequency = "Annual" },
            new() { Code = "KRI-INC-001", Name = "Mean Time to Detect (MTTD)", NameAr = "متوسط وقت الاكتشاف", Category = "Operational", Type = "KRI", Unit = "Hours", Target = 1, Warning = 4, Critical = 8, Direction = "LowerIsBetter", Frequency = "Monthly" },
            new() { Code = "KRI-INC-002", Name = "Mean Time to Respond (MTTR)", NameAr = "متوسط وقت الاستجابة", Category = "Operational", Type = "KRI", Unit = "Hours", Target = 4, Warning = 8, Critical = 24, Direction = "LowerIsBetter", Frequency = "Monthly" },

            // Third-Party KRIs
            new() { Code = "KRI-TPR-001", Name = "High-Risk Vendors Without Assessment", NameAr = "موردون عالي المخاطر بدون تقييم", Category = "ThirdParty", Type = "KRI", Unit = "Count", Target = 0, Warning = 2, Critical = 5, Direction = "LowerIsBetter", Frequency = "Quarterly" },
            new() { Code = "KRI-TPR-002", Name = "Vendor Issues Open > 90 Days", NameAr = "مشاكل الموردين مفتوحة > 90 يوم", Category = "ThirdParty", Type = "KRI", Unit = "Count", Target = 0, Warning = 3, Critical = 10, Direction = "LowerIsBetter", Frequency = "Monthly" },

            // Compliance KPIs
            new() { Code = "KPI-COM-001", Name = "Control Test Pass Rate", NameAr = "معدل نجاح اختبار الضوابط", Category = "Compliance", Type = "KPI", Unit = "Percentage", Target = 95, Warning = 90, Critical = 80, Direction = "HigherIsBetter", Frequency = "Quarterly" },
            new() { Code = "KPI-COM-002", Name = "Evidence Collection Completion", NameAr = "إكمال جمع الأدلة", Category = "Compliance", Type = "KPI", Unit = "Percentage", Target = 100, Warning = 95, Critical = 85, Direction = "HigherIsBetter", Frequency = "Quarterly" },
            new() { Code = "KPI-COM-003", Name = "Open Exceptions", NameAr = "الاستثناءات المفتوحة", Category = "Compliance", Type = "KPI", Unit = "Count", Target = 5, Warning = 10, Critical = 20, Direction = "LowerIsBetter", Frequency = "Monthly" },
            new() { Code = "KPI-COM-004", Name = "Overdue Remediation Items", NameAr = "عناصر المعالجة المتأخرة", Category = "Compliance", Type = "KPI", Unit = "Count", Target = 0, Warning = 5, Critical = 15, Direction = "LowerIsBetter", Frequency = "Weekly" }
        };
    }

    #endregion

    #region SoD Rules

    public static List<SoDRuleSeedInfo> GetStandardSoDRules()
    {
        return new List<SoDRuleSeedInfo>
        {
            // P2P SoD Rules
            new() { Code = "SOD-P2P-001", Name = "Create Vendor vs Approve Payment", NameAr = "إنشاء مورد مقابل اعتماد الدفع", Process = "P2P", Risk = "Critical", Func1 = "Create/Modify Vendor Master", Func2 = "Approve Payment Run", RiskDesc = "User can create fictitious vendor and pay themselves" },
            new() { Code = "SOD-P2P-002", Name = "Create PO vs Approve PO", NameAr = "إنشاء أمر شراء مقابل اعتماده", Process = "P2P", Risk = "High", Func1 = "Create Purchase Order", Func2 = "Approve Purchase Order", RiskDesc = "User can bypass approval controls" },
            new() { Code = "SOD-P2P-003", Name = "Goods Receipt vs Invoice Posting", NameAr = "استلام البضائع مقابل ترحيل الفاتورة", Process = "P2P", Risk = "High", Func1 = "Post Goods Receipt", Func2 = "Post Vendor Invoice", RiskDesc = "User can post fictitious receipts and invoices" },
            new() { Code = "SOD-P2P-004", Name = "Create Vendor vs Modify Bank Details", NameAr = "إنشاء مورد مقابل تعديل بيانات البنك", Process = "P2P", Risk = "Critical", Func1 = "Create Vendor", Func2 = "Modify Vendor Bank Account", RiskDesc = "User can redirect payments" },

            // R2R SoD Rules
            new() { Code = "SOD-R2R-001", Name = "Create JE vs Approve JE", NameAr = "إنشاء قيد مقابل اعتماده", Process = "R2R", Risk = "High", Func1 = "Create Journal Entry", Func2 = "Approve Journal Entry", RiskDesc = "User can post unauthorized entries" },
            new() { Code = "SOD-R2R-002", Name = "Create GL Account vs Post to GL", NameAr = "إنشاء حساب دفتر أستاذ مقابل الترحيل إليه", Process = "R2R", Risk = "High", Func1 = "Create GL Account", Func2 = "Post Journal Entry", RiskDesc = "User can create accounts and mispost" },
            new() { Code = "SOD-R2R-003", Name = "Period Close vs Manual JE", NameAr = "إقفال الفترة مقابل القيود اليدوية", Process = "R2R", Risk = "Medium", Func1 = "Close Accounting Period", Func2 = "Post Manual Journal Entry", RiskDesc = "User can post after close" },

            // O2C SoD Rules
            new() { Code = "SOD-O2C-001", Name = "Create Customer vs Apply Credit", NameAr = "إنشاء عميل مقابل تطبيق الائتمان", Process = "O2C", Risk = "High", Func1 = "Create Customer Master", Func2 = "Apply Credit Notes", RiskDesc = "User can create customer and issue credits" },
            new() { Code = "SOD-O2C-002", Name = "Create Sales Order vs Release Credit Hold", NameAr = "إنشاء أمر بيع مقابل إلغاء حجز الائتمان", Process = "O2C", Risk = "High", Func1 = "Create Sales Order", Func2 = "Release Credit Block", RiskDesc = "User can bypass credit controls" },

            // IAM SoD Rules
            new() { Code = "SOD-IAM-001", Name = "Create User vs Assign Roles", NameAr = "إنشاء مستخدم مقابل تعيين الأدوار", Process = "IAM", Risk = "Critical", Func1 = "Create User Account", Func2 = "Assign Security Roles", RiskDesc = "User can create accounts with elevated access" },
            new() { Code = "SOD-IAM-002", Name = "Reset Password vs Access Admin Console", NameAr = "إعادة تعيين كلمة المرور مقابل الوصول لوحدة الإدارة", Process = "IAM", Risk = "High", Func1 = "Reset User Password", Func2 = "Access Admin Console", RiskDesc = "User can impersonate other users" }
        };
    }

    #endregion

    #region CCM Control Tests

    public static List<CCMTestSeedInfo> GetStandardCCMTests()
    {
        return new List<CCMTestSeedInfo>
        {
            // IAM Tests
            new() { Code = "CCM-IAM-001", Name = "Access Review Completion", Process = "IAM", Category = "Access", Risk = "High", Frequency = "Quarterly", Population = "All user accounts with system access", Rule = "Access review completed within SLA", Threshold = "100% completion" },
            new() { Code = "CCM-IAM-002", Name = "Privileged Access Review", Process = "IAM", Category = "Access", Risk = "Critical", Frequency = "Monthly", Population = "All privileged accounts", Rule = "Privileged access justified and approved", Threshold = "100% reviewed" },
            new() { Code = "CCM-IAM-003", Name = "Terminated User Access Removal", Process = "IAM", Category = "Access", Risk = "High", Frequency = "Daily", Population = "Terminated employees in HR system", Rule = "Access removed within 24 hours of termination", Threshold = "100% within SLA" },
            new() { Code = "CCM-IAM-004", Name = "Dormant Account Deactivation", Process = "IAM", Category = "Access", Risk = "Medium", Frequency = "Monthly", Population = "Accounts inactive > 90 days", Rule = "Account deactivated or exception documented", Threshold = "100% actioned" },

            // P2P Tests
            new() { Code = "CCM-P2P-001", Name = "Vendor Bank Change Dual Approval", Process = "P2P", Category = "Approval", Risk = "Critical", Frequency = "Daily", Population = "All vendor bank account changes", Rule = "Dual approval by different users", Threshold = "100% dual approved" },
            new() { Code = "CCM-P2P-002", Name = "Three-Way Match Compliance", Process = "P2P", Category = "DataIntegrity", Risk = "High", Frequency = "Daily", Population = "All invoices processed", Rule = "PO, GR, Invoice matched within tolerance", Threshold = ">95% matched" },
            new() { Code = "CCM-P2P-003", Name = "Split Payment Detection", Process = "P2P", Category = "Fraud", Risk = "High", Frequency = "Weekly", Population = "All payments to same vendor in 7 days", Rule = "No split to avoid approval threshold", Threshold = "0 violations" },

            // R2R Tests
            new() { Code = "CCM-R2R-001", Name = "Journal Entry Approval", Process = "R2R", Category = "Approval", Risk = "High", Frequency = "Daily", Population = "All manual journal entries", Rule = "Approved by manager before posting", Threshold = "100% approved" },
            new() { Code = "CCM-R2R-002", Name = "Post-Close Journal Detection", Process = "R2R", Category = "DataIntegrity", Risk = "Medium", Frequency = "Monthly", Population = "JEs posted after period close", Rule = "Legitimate business reason documented", Threshold = "<5 per period" },

            // Vulnerability Tests
            new() { Code = "CCM-VUL-001", Name = "Critical Patch SLA", Process = "Vulnerability", Category = "Security", Risk = "Critical", Frequency = "Daily", Population = "All critical vulnerabilities", Rule = "Patched within 14 days", Threshold = ">95% within SLA" },
            new() { Code = "CCM-VUL-002", Name = "High Vulnerability SLA", Process = "Vulnerability", Category = "Security", Risk = "High", Frequency = "Weekly", Population = "All high vulnerabilities", Rule = "Patched within 30 days", Threshold = ">90% within SLA" },

            // Change Management Tests
            new() { Code = "CCM-CHG-001", Name = "Change Approval Compliance", Process = "Change", Category = "Approval", Risk = "High", Frequency = "Weekly", Population = "All production changes", Rule = "CAB approval before implementation", Threshold = "100% approved" },
            new() { Code = "CCM-CHG-002", Name = "Emergency Change Post-Approval", Process = "Change", Category = "Approval", Risk = "High", Frequency = "Weekly", Population = "All emergency changes", Rule = "Retroactive approval within 48 hours", Threshold = "100% post-approved" },

            // Backup Tests
            new() { Code = "CCM-BCP-001", Name = "Backup Success Rate", Process = "Backup", Category = "Continuity", Risk = "High", Frequency = "Daily", Population = "All scheduled backups", Rule = "Backup completed successfully", Threshold = ">99% success" },
            new() { Code = "CCM-BCP-002", Name = "Restore Test Success", Process = "Backup", Category = "Continuity", Risk = "High", Frequency = "Quarterly", Population = "All critical systems", Rule = "Restore test passed", Threshold = "100% success" }
        };
    }

    #endregion

    #region Agent Definitions

    public static List<AgentDefinitionSeedInfo> GetAgentDefinitions()
    {
        return new List<AgentDefinitionSeedInfo>
        {
            new() { Code = "EVIDENCE_AGENT", Name = "Evidence Collection Agent", NameAr = "وكيل جمع الأدلة", Type = "Evidence", Capabilities = "PullData,GenerateEvidencePack,ValidateCompleteness,TagEvidence,StoreEvidence", ApprovalActions = "DeleteEvidence,ModifyEvidence", AutoThreshold = 90 },
            new() { Code = "MONITORING_AGENT", Name = "Control Monitoring Agent", NameAr = "وكيل مراقبة الضوابط", Type = "Monitoring", Capabilities = "RunKRI,DetectBreach,CreateTicket,SendNotification,Escalate", ApprovalActions = "DisableControl,ModifyThreshold", AutoThreshold = 85 },
            new() { Code = "TESTING_AGENT", Name = "Control Testing Agent", NameAr = "وكيل اختبار الضوابط", Type = "Testing", Capabilities = "ExecuteTest,RecordResult,ProposeOutcome,GenerateWorkpaper", ApprovalActions = "MarkControlEffective,MarkControlIneffective", AutoThreshold = 80 },
            new() { Code = "MAPPING_AGENT", Name = "Framework Mapping Agent", NameAr = "وكيل ربط الأطر", Type = "Mapping", Capabilities = "SuggestMapping,IdentifyGap,IdentifyOverlap,MaintainCrosswalk", ApprovalActions = "ApproveMapping,DeleteMapping", AutoThreshold = 75 },
            new() { Code = "AUDIT_RESPONSE_AGENT", Name = "Audit Response Agent", NameAr = "وكيل الاستجابة للتدقيق", Type = "AuditResponse", Capabilities = "AssemblePackage,DraftResponse,TrackRequest,LinkEvidence", ApprovalActions = "SubmitToAuditor,CloseAuditRequest", AutoThreshold = 70 },
            new() { Code = "WORKFLOW_AGENT", Name = "Workflow Orchestration Agent", NameAr = "وكيل تنسيق سير العمل", Type = "Workflow", Capabilities = "RouteTask,SendReminder,Escalate,UpdateStatus", ApprovalActions = "OverrideSLA,ReassignTask", AutoThreshold = 85 }
        };
    }

    #endregion

    #region Seed Info Classes

    public class RiskIndicatorSeedInfo
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string NameAr { get; set; } = "";
        public string Category { get; set; } = "";
        public string Type { get; set; } = "";
        public string Unit { get; set; } = "";
        public decimal Target { get; set; }
        public decimal Warning { get; set; }
        public decimal Critical { get; set; }
        public string Direction { get; set; } = "";
        public string Frequency { get; set; } = "";
    }

    public class SoDRuleSeedInfo
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string NameAr { get; set; } = "";
        public string Process { get; set; } = "";
        public string Risk { get; set; } = "";
        public string Func1 { get; set; } = "";
        public string Func2 { get; set; } = "";
        public string RiskDesc { get; set; } = "";
    }

    public class CCMTestSeedInfo
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string Process { get; set; } = "";
        public string Category { get; set; } = "";
        public string Risk { get; set; } = "";
        public string Frequency { get; set; } = "";
        public string Population { get; set; } = "";
        public string Rule { get; set; } = "";
        public string Threshold { get; set; } = "";
    }

    public class AgentDefinitionSeedInfo
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string NameAr { get; set; } = "";
        public string Type { get; set; } = "";
        public string Capabilities { get; set; } = "";
        public string ApprovalActions { get; set; } = "";
        public int AutoThreshold { get; set; }
    }

    #endregion
}
