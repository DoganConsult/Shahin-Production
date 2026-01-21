using GrcMvc.Models.Entities;

namespace GrcMvc.Data.SeedData;

/// <summary>
/// Standard Evidence Pack Families based on control domains
/// These are the 8 core families that cover most compliance requirements
/// </summary>
public static class EvidencePackFamilySeedData
{
    public static List<EvidencePackFamily> GetStandardFamilies()
    {
        return new List<EvidencePackFamily>
        {
            new EvidencePackFamily
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                FamilyCode = "GOV",
                Name = "Governance & Policy",
                NameAr = "الحوكمة والسياسات",
                Description = "Security policies, risk management, committee governance, exceptions register",
                IconClass = "fas fa-balance-scale",
                DisplayOrder = 1,
                IsActive = true,
                EvidenceItems = GetGovernanceEvidenceItems()
            },
            new EvidencePackFamily
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                FamilyCode = "IAM",
                Name = "Identity & Access Management",
                NameAr = "إدارة الهوية والوصول",
                Description = "Access provisioning, reviews, privileged access, authentication",
                IconClass = "fas fa-user-lock",
                DisplayOrder = 2,
                IsActive = true,
                EvidenceItems = GetIAMEvidenceItems()
            },
            new EvidencePackFamily
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000003"),
                FamilyCode = "LOG",
                Name = "Logging & Monitoring",
                NameAr = "التسجيل والمراقبة",
                Description = "SIEM, SOC operations, log management, alerting",
                IconClass = "fas fa-eye",
                DisplayOrder = 3,
                IsActive = true,
                EvidenceItems = GetLoggingEvidenceItems()
            },
            new EvidencePackFamily
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000004"),
                FamilyCode = "VUL",
                Name = "Vulnerability & Patch Management",
                NameAr = "إدارة الثغرات والتحديثات",
                Description = "Vulnerability scanning, patching, remediation tracking",
                IconClass = "fas fa-bug",
                DisplayOrder = 4,
                IsActive = true,
                EvidenceItems = GetVulnerabilityEvidenceItems()
            },
            new EvidencePackFamily
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000005"),
                FamilyCode = "CHG",
                Name = "Change Management",
                NameAr = "إدارة التغيير",
                Description = "Change requests, approvals, testing, emergency changes",
                IconClass = "fas fa-exchange-alt",
                DisplayOrder = 5,
                IsActive = true,
                EvidenceItems = GetChangeManagementEvidenceItems()
            },
            new EvidencePackFamily
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000006"),
                FamilyCode = "BCP",
                Name = "Backup, DR & Business Continuity",
                NameAr = "النسخ الاحتياطي واستمرارية الأعمال",
                Description = "Backups, disaster recovery, BCP testing, RTO/RPO",
                IconClass = "fas fa-database",
                DisplayOrder = 6,
                IsActive = true,
                EvidenceItems = GetBCPEvidenceItems()
            },
            new EvidencePackFamily
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000007"),
                FamilyCode = "INC",
                Name = "Incident Response",
                NameAr = "الاستجابة للحوادث",
                Description = "IR policy, playbooks, incident register, post-incident reviews",
                IconClass = "fas fa-exclamation-triangle",
                DisplayOrder = 7,
                IsActive = true,
                EvidenceItems = GetIncidentResponseEvidenceItems()
            },
            new EvidencePackFamily
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000008"),
                FamilyCode = "TPR",
                Name = "Third-Party & Vendor Risk",
                NameAr = "مخاطر الأطراف الثالثة",
                Description = "Vendor inventory, due diligence, contracts, ongoing monitoring",
                IconClass = "fas fa-handshake",
                DisplayOrder = 8,
                IsActive = true,
                EvidenceItems = GetThirdPartyEvidenceItems()
            }
        };
    }

    private static List<StandardEvidenceItem> GetGovernanceEvidenceItems()
    {
        var familyId = Guid.Parse("10000000-0000-0000-0000-000000000001");
        return new List<StandardEvidenceItem>
        {
            new() { Id = Guid.NewGuid(), FamilyId = familyId, ItemCode = "GOV-001", Name = "Information Security Policy", NameAr = "سياسة أمن المعلومات", EvidenceType = "Document", RequiredFrequency = "Annual", IsMandatory = true, CollectionGuidance = "Approved policy document with version, approval date, and signatures", DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), FamilyId = familyId, ItemCode = "GOV-002", Name = "Risk Management Policy", NameAr = "سياسة إدارة المخاطر", EvidenceType = "Document", RequiredFrequency = "Annual", IsMandatory = true, CollectionGuidance = "Risk management framework and policy with approval", DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), FamilyId = familyId, ItemCode = "GOV-003", Name = "Security Committee Minutes", NameAr = "محاضر لجنة الأمن", EvidenceType = "Document", RequiredFrequency = "Quarterly", IsMandatory = true, CollectionGuidance = "Meeting minutes with attendees, decisions, and action items", DisplayOrder = 3 },
            new() { Id = Guid.NewGuid(), FamilyId = familyId, ItemCode = "GOV-004", Name = "Exceptions Register", NameAr = "سجل الاستثناءات", EvidenceType = "Report", RequiredFrequency = "Continuous", IsMandatory = true, CollectionGuidance = "All security exceptions with approvals and expiry dates", DisplayOrder = 4 },
            new() { Id = Guid.NewGuid(), FamilyId = familyId, ItemCode = "GOV-005", Name = "ISMS Scope Document", NameAr = "نطاق نظام إدارة أمن المعلومات", EvidenceType = "Document", RequiredFrequency = "Annual", IsMandatory = false, CollectionGuidance = "Document defining the scope of the security management system", DisplayOrder = 5 },
            new() { Id = Guid.NewGuid(), FamilyId = familyId, ItemCode = "GOV-006", Name = "Roles & Responsibilities Matrix", NameAr = "مصفوفة الأدوار والمسؤوليات", EvidenceType = "Document", RequiredFrequency = "Annual", IsMandatory = true, CollectionGuidance = "RACI or similar matrix for security responsibilities", DisplayOrder = 6 }
        };
    }

    private static List<StandardEvidenceItem> GetIAMEvidenceItems()
    {
        var familyId = Guid.Parse("10000000-0000-0000-0000-000000000002");
        return new List<StandardEvidenceItem>
        {
            new() { Id = Guid.NewGuid(), FamilyId = familyId, ItemCode = "IAM-001", Name = "Access Provisioning Workflow", NameAr = "سير عمل منح الصلاحيات", EvidenceType = "Sample", RequiredFrequency = "Continuous", IsMandatory = true, CollectionGuidance = "Sample access requests with approvals (5-10 samples)", DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), FamilyId = familyId, ItemCode = "IAM-002", Name = "Quarterly Access Review Report", NameAr = "تقرير مراجعة الصلاحيات الربع سنوي", EvidenceType = "Report", RequiredFrequency = "Quarterly", IsMandatory = true, CollectionGuidance = "Access review report with sign-offs from system owners", DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), FamilyId = familyId, ItemCode = "IAM-003", Name = "Privileged Access Management Report", NameAr = "تقرير إدارة الصلاحيات المميزة", EvidenceType = "Report", RequiredFrequency = "Monthly", IsMandatory = true, CollectionGuidance = "PAM tool report showing privileged account usage", DisplayOrder = 3 },
            new() { Id = Guid.NewGuid(), FamilyId = familyId, ItemCode = "IAM-004", Name = "Joiner/Mover/Leaver Samples", NameAr = "عينات المنضمين والمنتقلين والمغادرين", EvidenceType = "Sample", RequiredFrequency = "Quarterly", IsMandatory = true, CollectionGuidance = "5 samples each of new hires, transfers, and terminations with access changes", DisplayOrder = 4 },
            new() { Id = Guid.NewGuid(), FamilyId = familyId, ItemCode = "IAM-005", Name = "MFA Configuration Evidence", NameAr = "دليل تكوين المصادقة متعددة العوامل", EvidenceType = "Screenshot", RequiredFrequency = "Annual", IsMandatory = true, CollectionGuidance = "Screenshots showing MFA enforcement on critical systems", DisplayOrder = 5 },
            new() { Id = Guid.NewGuid(), FamilyId = familyId, ItemCode = "IAM-006", Name = "Service Account Inventory", NameAr = "جرد حسابات الخدمة", EvidenceType = "Report", RequiredFrequency = "Quarterly", IsMandatory = true, CollectionGuidance = "List of all service accounts with owners and last password change", DisplayOrder = 6 }
        };
    }

    private static List<StandardEvidenceItem> GetLoggingEvidenceItems()
    {
        var familyId = Guid.Parse("10000000-0000-0000-0000-000000000003");
        return new List<StandardEvidenceItem>
        {
            new() { Id = Guid.NewGuid(), FamilyId = familyId, ItemCode = "LOG-001", Name = "SIEM Coverage Statement", NameAr = "بيان تغطية نظام SIEM", EvidenceType = "Document", RequiredFrequency = "Annual", IsMandatory = true, CollectionGuidance = "Document listing all log sources integrated with SIEM", DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), FamilyId = familyId, ItemCode = "LOG-002", Name = "Log Source Inventory", NameAr = "جرد مصادر السجلات", EvidenceType = "Report", RequiredFrequency = "Quarterly", IsMandatory = true, CollectionGuidance = "Complete list of systems feeding logs with status", DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), FamilyId = familyId, ItemCode = "LOG-003", Name = "Alert Rules Documentation", NameAr = "توثيق قواعد التنبيه", EvidenceType = "Document", RequiredFrequency = "Annual", IsMandatory = true, CollectionGuidance = "List of SIEM alert rules with thresholds and escalation", DisplayOrder = 3 },
            new() { Id = Guid.NewGuid(), FamilyId = familyId, ItemCode = "LOG-004", Name = "Sample Alerts and Tickets", NameAr = "عينات التنبيهات والتذاكر", EvidenceType = "Sample", RequiredFrequency = "Quarterly", IsMandatory = true, CollectionGuidance = "5-10 sample alerts with corresponding investigation tickets", DisplayOrder = 4 },
            new() { Id = Guid.NewGuid(), FamilyId = familyId, ItemCode = "LOG-005", Name = "Log Retention Configuration", NameAr = "تكوين الاحتفاظ بالسجلات", EvidenceType = "Screenshot", RequiredFrequency = "Annual", IsMandatory = true, CollectionGuidance = "Evidence of log retention settings meeting policy requirements", DisplayOrder = 5 },
            new() { Id = Guid.NewGuid(), FamilyId = familyId, ItemCode = "LOG-006", Name = "SOC Operations Report", NameAr = "تقرير عمليات مركز الأمن", EvidenceType = "Report", RequiredFrequency = "Monthly", IsMandatory = false, CollectionGuidance = "Monthly SOC metrics including alerts, incidents, and response times", DisplayOrder = 6 }
        };
    }

    private static List<StandardEvidenceItem> GetVulnerabilityEvidenceItems()
    {
        var familyId = Guid.Parse("10000000-0000-0000-0000-000000000004");
        return new List<StandardEvidenceItem>
        {
            new() { Id = Guid.NewGuid(), FamilyId = familyId, ItemCode = "VUL-001", Name = "Vulnerability Scan Report", NameAr = "تقرير فحص الثغرات", EvidenceType = "Report", RequiredFrequency = "Monthly", IsMandatory = true, CollectionGuidance = "Full vulnerability scan results with severity breakdown", DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), FamilyId = familyId, ItemCode = "VUL-002", Name = "Remediation Tracking Report", NameAr = "تقرير تتبع المعالجة", EvidenceType = "Report", RequiredFrequency = "Monthly", IsMandatory = true, CollectionGuidance = "Status of vulnerability remediation against SLAs", DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), FamilyId = familyId, ItemCode = "VUL-003", Name = "Patch Compliance Dashboard", NameAr = "لوحة الامتثال للتحديثات", EvidenceType = "Screenshot", RequiredFrequency = "Monthly", IsMandatory = true, CollectionGuidance = "Screenshot showing patch compliance percentage", DisplayOrder = 3 },
            new() { Id = Guid.NewGuid(), FamilyId = familyId, ItemCode = "VUL-004", Name = "Exception Approvals for Deferred Patches", NameAr = "موافقات استثناء التحديثات المؤجلة", EvidenceType = "Sample", RequiredFrequency = "Continuous", IsMandatory = true, CollectionGuidance = "Approved exception requests for patches not applied within SLA", DisplayOrder = 4 },
            new() { Id = Guid.NewGuid(), FamilyId = familyId, ItemCode = "VUL-005", Name = "Penetration Test Report", NameAr = "تقرير اختبار الاختراق", EvidenceType = "Report", RequiredFrequency = "Annual", IsMandatory = true, CollectionGuidance = "External penetration test results with remediation status", DisplayOrder = 5 },
            new() { Id = Guid.NewGuid(), FamilyId = familyId, ItemCode = "VUL-006", Name = "Patch Fix Verification Samples", NameAr = "عينات التحقق من إصلاح التحديثات", EvidenceType = "Sample", RequiredFrequency = "Quarterly", IsMandatory = false, CollectionGuidance = "Sample tickets showing patch applied and verified", DisplayOrder = 6 }
        };
    }

    private static List<StandardEvidenceItem> GetChangeManagementEvidenceItems()
    {
        var familyId = Guid.Parse("10000000-0000-0000-0000-000000000005");
        return new List<StandardEvidenceItem>
        {
            new() { Id = Guid.NewGuid(), FamilyId = familyId, ItemCode = "CHG-001", Name = "Change Management Policy", NameAr = "سياسة إدارة التغيير", EvidenceType = "Document", RequiredFrequency = "Annual", IsMandatory = true, CollectionGuidance = "Approved change management policy with classifications", DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), FamilyId = familyId, ItemCode = "CHG-002", Name = "Standard Change Samples", NameAr = "عينات التغييرات القياسية", EvidenceType = "Sample", RequiredFrequency = "Quarterly", IsMandatory = true, CollectionGuidance = "5 sample standard changes with approvals and testing", DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), FamilyId = familyId, ItemCode = "CHG-003", Name = "Major Change Samples", NameAr = "عينات التغييرات الرئيسية", EvidenceType = "Sample", RequiredFrequency = "Quarterly", IsMandatory = true, CollectionGuidance = "3 sample major changes with CAB approval and backout plans", DisplayOrder = 3 },
            new() { Id = Guid.NewGuid(), FamilyId = familyId, ItemCode = "CHG-004", Name = "Emergency Change Records", NameAr = "سجلات التغييرات الطارئة", EvidenceType = "Sample", RequiredFrequency = "Quarterly", IsMandatory = true, CollectionGuidance = "All emergency changes with post-implementation approvals", DisplayOrder = 4 },
            new() { Id = Guid.NewGuid(), FamilyId = familyId, ItemCode = "CHG-005", Name = "CAB Meeting Minutes", NameAr = "محاضر اجتماعات لجنة التغيير", EvidenceType = "Document", RequiredFrequency = "Weekly", IsMandatory = false, CollectionGuidance = "Sample CAB meeting minutes with decisions", DisplayOrder = 5 },
            new() { Id = Guid.NewGuid(), FamilyId = familyId, ItemCode = "CHG-006", Name = "Failed Change Analysis", NameAr = "تحليل التغييرات الفاشلة", EvidenceType = "Report", RequiredFrequency = "Quarterly", IsMandatory = false, CollectionGuidance = "Report on failed changes and lessons learned", DisplayOrder = 6 }
        };
    }

    private static List<StandardEvidenceItem> GetBCPEvidenceItems()
    {
        var familyId = Guid.Parse("10000000-0000-0000-0000-000000000006");
        return new List<StandardEvidenceItem>
        {
            new() { Id = Guid.NewGuid(), FamilyId = familyId, ItemCode = "BCP-001", Name = "Backup Configuration Evidence", NameAr = "دليل تكوين النسخ الاحتياطي", EvidenceType = "Screenshot", RequiredFrequency = "Annual", IsMandatory = true, CollectionGuidance = "Backup schedules and retention settings for critical systems", DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), FamilyId = familyId, ItemCode = "BCP-002", Name = "Backup Success Reports", NameAr = "تقارير نجاح النسخ الاحتياطي", EvidenceType = "Report", RequiredFrequency = "Monthly", IsMandatory = true, CollectionGuidance = "Monthly backup success/failure rates", DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), FamilyId = familyId, ItemCode = "BCP-003", Name = "Restore Test Results", NameAr = "نتائج اختبار الاستعادة", EvidenceType = "Report", RequiredFrequency = "Annual", IsMandatory = true, CollectionGuidance = "Evidence of successful restore tests for critical systems", DisplayOrder = 3 },
            new() { Id = Guid.NewGuid(), FamilyId = familyId, ItemCode = "BCP-004", Name = "DR Test Plan", NameAr = "خطة اختبار التعافي من الكوارث", EvidenceType = "Document", RequiredFrequency = "Annual", IsMandatory = true, CollectionGuidance = "DR test plan with scenarios and success criteria", DisplayOrder = 4 },
            new() { Id = Guid.NewGuid(), FamilyId = familyId, ItemCode = "BCP-005", Name = "DR Test Report", NameAr = "تقرير اختبار التعافي من الكوارث", EvidenceType = "Report", RequiredFrequency = "Annual", IsMandatory = true, CollectionGuidance = "Results of DR test with RTO/RPO achievements", DisplayOrder = 5 },
            new() { Id = Guid.NewGuid(), FamilyId = familyId, ItemCode = "BCP-006", Name = "RTO/RPO Definitions", NameAr = "تعريفات RTO/RPO", EvidenceType = "Document", RequiredFrequency = "Annual", IsMandatory = true, CollectionGuidance = "Documented RTO/RPO for all critical systems", DisplayOrder = 6 },
            new() { Id = Guid.NewGuid(), FamilyId = familyId, ItemCode = "BCP-007", Name = "BCP Policy and Plans", NameAr = "سياسة وخطط استمرارية الأعمال", EvidenceType = "Document", RequiredFrequency = "Annual", IsMandatory = true, CollectionGuidance = "Approved BCP with contact trees and procedures", DisplayOrder = 7 }
        };
    }

    private static List<StandardEvidenceItem> GetIncidentResponseEvidenceItems()
    {
        var familyId = Guid.Parse("10000000-0000-0000-0000-000000000007");
        return new List<StandardEvidenceItem>
        {
            new() { Id = Guid.NewGuid(), FamilyId = familyId, ItemCode = "INC-001", Name = "Incident Response Policy", NameAr = "سياسة الاستجابة للحوادث", EvidenceType = "Document", RequiredFrequency = "Annual", IsMandatory = true, CollectionGuidance = "Approved IR policy with classification and escalation", DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), FamilyId = familyId, ItemCode = "INC-002", Name = "IR Playbooks", NameAr = "أدلة الاستجابة للحوادث", EvidenceType = "Document", RequiredFrequency = "Annual", IsMandatory = true, CollectionGuidance = "Playbooks for common incident types (malware, phishing, etc.)", DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), FamilyId = familyId, ItemCode = "INC-003", Name = "Incident Register", NameAr = "سجل الحوادث", EvidenceType = "Report", RequiredFrequency = "Continuous", IsMandatory = true, CollectionGuidance = "Log of all security incidents with classification and status", DisplayOrder = 3 },
            new() { Id = Guid.NewGuid(), FamilyId = familyId, ItemCode = "INC-004", Name = "Post-Incident Review (PIR)", NameAr = "مراجعة ما بعد الحادث", EvidenceType = "Document", RequiredFrequency = "Continuous", IsMandatory = true, CollectionGuidance = "PIR documents for significant incidents with lessons learned", DisplayOrder = 4 },
            new() { Id = Guid.NewGuid(), FamilyId = familyId, ItemCode = "INC-005", Name = "Tabletop Exercise Report", NameAr = "تقرير تمرين المحاكاة", EvidenceType = "Report", RequiredFrequency = "Annual", IsMandatory = true, CollectionGuidance = "Results of IR tabletop/simulation exercises", DisplayOrder = 5 },
            new() { Id = Guid.NewGuid(), FamilyId = familyId, ItemCode = "INC-006", Name = "Regulatory Notification Evidence", NameAr = "دليل الإخطار التنظيمي", EvidenceType = "Sample", RequiredFrequency = "Continuous", IsMandatory = false, CollectionGuidance = "Evidence of regulatory notifications for reportable incidents", DisplayOrder = 6 }
        };
    }

    private static List<StandardEvidenceItem> GetThirdPartyEvidenceItems()
    {
        var familyId = Guid.Parse("10000000-0000-0000-0000-000000000008");
        return new List<StandardEvidenceItem>
        {
            new() { Id = Guid.NewGuid(), FamilyId = familyId, ItemCode = "TPR-001", Name = "Vendor Inventory", NameAr = "جرد الموردين", EvidenceType = "Report", RequiredFrequency = "Annual", IsMandatory = true, CollectionGuidance = "Complete list of vendors with risk tiering", DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), FamilyId = familyId, ItemCode = "TPR-002", Name = "Vendor Due Diligence Reports", NameAr = "تقارير العناية الواجبة للموردين", EvidenceType = "Report", RequiredFrequency = "Annual", IsMandatory = true, CollectionGuidance = "Security assessments for critical/high-risk vendors", DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), FamilyId = familyId, ItemCode = "TPR-003", Name = "Vendor Contract Clauses", NameAr = "بنود عقود الموردين", EvidenceType = "Sample", RequiredFrequency = "Annual", IsMandatory = true, CollectionGuidance = "Sample contracts showing security, audit, and data handling clauses", DisplayOrder = 3 },
            new() { Id = Guid.NewGuid(), FamilyId = familyId, ItemCode = "TPR-004", Name = "Vendor SOC Reports", NameAr = "تقارير SOC للموردين", EvidenceType = "Report", RequiredFrequency = "Annual", IsMandatory = true, CollectionGuidance = "SOC 2 or equivalent reports from critical vendors", DisplayOrder = 4 },
            new() { Id = Guid.NewGuid(), FamilyId = familyId, ItemCode = "TPR-005", Name = "Ongoing Monitoring Records", NameAr = "سجلات المراقبة المستمرة", EvidenceType = "Report", RequiredFrequency = "Quarterly", IsMandatory = false, CollectionGuidance = "Evidence of periodic vendor reviews and monitoring", DisplayOrder = 5 },
            new() { Id = Guid.NewGuid(), FamilyId = familyId, ItemCode = "TPR-006", Name = "Vendor Access Logs", NameAr = "سجلات وصول الموردين", EvidenceType = "Report", RequiredFrequency = "Monthly", IsMandatory = false, CollectionGuidance = "Logs of third-party access to systems", DisplayOrder = 6 }
        };
    }
}
