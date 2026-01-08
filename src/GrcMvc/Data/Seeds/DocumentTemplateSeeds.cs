using GrcMvc.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace GrcMvc.Data.Seeds;

/// <summary>
/// Seeds document templates for the Document Center
/// </summary>
public static class DocumentTemplateSeeds
{
    public static async Task SeedDocumentTemplatesAsync(GrcDbContext context)
    {
        if (await context.DocumentTemplates.AnyAsync())
            return;

        var templates = new List<DocumentTemplate>
        {
            // ==================== POLICIES ====================
            new()
            {
                Code = "POL-ISP-001",
                TitleEn = "Information Security Policy",
                TitleAr = "سياسة أمن المعلومات",
                DescriptionEn = "Master information security policy covering access control, data classification, encryption, incident response, and all security domains.",
                DescriptionAr = "السياسة الرئيسية لأمن المعلومات تغطي التحكم بالوصول وتصنيف البيانات والتشفير والاستجابة للحوادث وجميع مجالات الأمن.",
                Category = DocumentTemplateCategories.Policy,
                Domain = GrcDomains.Security,
                FrameworkCodes = "NCA-ECC,ISO-27001,SAMA-CSF",
                Version = "1.0",
                FileFormat = "docx",
                SectionsJson = "[\"Document Control\",\"Purpose & Scope\",\"Definitions\",\"Roles & Responsibilities\",\"Policy Statements\",\"Compliance Requirements\",\"Enforcement\",\"Review & Update\"]",
                FieldsJson = "[\"Organization Name\",\"Policy Owner\",\"Approval Date\",\"Review Date\",\"Version\"]",
                InstructionsEn = "Replace [ORGANIZATION] with your company name. Review all sections and customize policy statements to match your requirements.",
                InstructionsAr = "استبدل [المنظمة] باسم شركتك. راجع جميع الأقسام وخصص بيانات السياسة لتتوافق مع متطلباتك.",
                Tags = "security,policy,information,cybersecurity,master",
                DisplayOrder = 1
            },
            new()
            {
                Code = "POL-ACP-001",
                TitleEn = "Access Control Policy",
                TitleAr = "سياسة التحكم بالوصول",
                DescriptionEn = "Policy defining access control principles, user authentication, authorization, and access management procedures.",
                DescriptionAr = "سياسة تحدد مبادئ التحكم بالوصول ومصادقة المستخدم والترخيص وإجراءات إدارة الوصول.",
                Category = DocumentTemplateCategories.Policy,
                Domain = GrcDomains.Security,
                FrameworkCodes = "NCA-ECC,ISO-27001",
                Version = "1.0",
                FileFormat = "docx",
                Tags = "access,control,authentication,authorization",
                DisplayOrder = 2
            },
            new()
            {
                Code = "POL-DCP-001",
                TitleEn = "Data Classification Policy",
                TitleAr = "سياسة تصنيف البيانات",
                DescriptionEn = "Policy for classifying data based on sensitivity levels: Public, Internal, Confidential, Restricted.",
                DescriptionAr = "سياسة تصنيف البيانات حسب مستويات الحساسية: عام، داخلي، سري، مقيد.",
                Category = DocumentTemplateCategories.Policy,
                Domain = GrcDomains.Security,
                FrameworkCodes = "NCA-ECC,PDPL,ISO-27001",
                Version = "1.0",
                FileFormat = "docx",
                Tags = "data,classification,sensitivity,labeling",
                DisplayOrder = 3
            },
            new()
            {
                Code = "POL-PDP-001",
                TitleEn = "Personal Data Protection Policy",
                TitleAr = "سياسة حماية البيانات الشخصية",
                DescriptionEn = "Policy for PDPL compliance covering personal data collection, processing, storage, and disposal.",
                DescriptionAr = "سياسة الامتثال لنظام حماية البيانات الشخصية تغطي جمع ومعالجة وتخزين والتخلص من البيانات الشخصية.",
                Category = DocumentTemplateCategories.Policy,
                Domain = GrcDomains.Privacy,
                FrameworkCodes = "PDPL",
                Version = "1.0",
                FileFormat = "docx",
                Tags = "privacy,pdpl,personal,data,protection",
                DisplayOrder = 4
            },

            // ==================== FORMS ====================
            new()
            {
                Code = "FRM-RAF-001",
                TitleEn = "Risk Assessment Form",
                TitleAr = "نموذج تقييم المخاطر",
                DescriptionEn = "Form for identifying, analyzing, and evaluating risks with likelihood/impact scoring and treatment plans.",
                DescriptionAr = "نموذج لتحديد وتحليل وتقييم المخاطر مع تسجيل الاحتمالية/الأثر وخطط المعالجة.",
                Category = DocumentTemplateCategories.Form,
                Domain = GrcDomains.Risk,
                FrameworkCodes = "ISO-31000,COSO-ERM",
                Version = "1.0",
                FileFormat = "xlsx",
                SectionsJson = "[\"Assessment Info\",\"Risk Identification\",\"Risk Analysis\",\"Risk Evaluation\",\"Treatment Plan\",\"Sign-off\"]",
                FieldsJson = "[\"Risk ID\",\"Risk Title\",\"Description\",\"Category\",\"Likelihood\",\"Impact\",\"Score\",\"Owner\",\"Treatment\"]",
                Tags = "risk,assessment,evaluation,scoring",
                DisplayOrder = 10
            },
            new()
            {
                Code = "FRM-CTW-001",
                TitleEn = "Control Testing Worksheet",
                TitleAr = "ورقة عمل اختبار الضوابط",
                DescriptionEn = "Worksheet for documenting control test procedures, samples, results, and findings.",
                DescriptionAr = "ورقة عمل لتوثيق إجراءات اختبار الضوابط والعينات والنتائج والملاحظات.",
                Category = DocumentTemplateCategories.Form,
                Domain = GrcDomains.Audit,
                FrameworkCodes = "ISO-27001,COBIT",
                Version = "1.0",
                FileFormat = "xlsx",
                Tags = "control,testing,audit,worksheet",
                DisplayOrder = 11
            },
            new()
            {
                Code = "FRM-ESF-001",
                TitleEn = "Evidence Submission Form",
                TitleAr = "نموذج تقديم الأدلة",
                DescriptionEn = "Cover sheet for evidence uploads with attestation and metadata fields.",
                DescriptionAr = "غلاف لرفع الأدلة مع الإقرار وحقول البيانات الوصفية.",
                Category = DocumentTemplateCategories.Form,
                Domain = GrcDomains.Compliance,
                Version = "1.0",
                FileFormat = "docx",
                Tags = "evidence,submission,attestation",
                DisplayOrder = 12
            },
            new()
            {
                Code = "FRM-IRF-001",
                TitleEn = "Incident Report Form",
                TitleAr = "نموذج تقرير الحوادث",
                DescriptionEn = "Form for reporting security incidents with impact assessment, containment actions, and root cause analysis.",
                DescriptionAr = "نموذج للإبلاغ عن الحوادث الأمنية مع تقييم الأثر وإجراءات الاحتواء وتحليل السبب الجذري.",
                Category = DocumentTemplateCategories.Form,
                Domain = GrcDomains.Security,
                FrameworkCodes = "NCA-ECC,ISO-27001",
                Version = "1.0",
                FileFormat = "docx",
                SectionsJson = "[\"Incident Details\",\"Impact Assessment\",\"Containment Actions\",\"Root Cause\",\"Remediation\"]",
                Tags = "incident,security,breach,report",
                DisplayOrder = 13
            },
            new()
            {
                Code = "FRM-CRF-001",
                TitleEn = "Change Request Form",
                TitleAr = "نموذج طلب التغيير",
                DescriptionEn = "Form for IT/process change requests with risk assessment and CAB approval workflow.",
                DescriptionAr = "نموذج لطلبات التغيير في تقنية المعلومات/العمليات مع تقييم المخاطر وسير عمل موافقة CAB.",
                Category = DocumentTemplateCategories.Form,
                Domain = GrcDomains.Operations,
                FrameworkCodes = "ITIL",
                Version = "1.0",
                FileFormat = "docx",
                Tags = "change,request,cab,approval",
                DisplayOrder = 14
            },
            new()
            {
                Code = "FRM-ARF-001",
                TitleEn = "Access Request Form",
                TitleAr = "نموذج طلب الوصول",
                DescriptionEn = "Form for requesting system/application access with manager approval.",
                DescriptionAr = "نموذج لطلب الوصول للنظام/التطبيق مع موافقة المدير.",
                Category = DocumentTemplateCategories.Form,
                Domain = GrcDomains.Security,
                FrameworkCodes = "NCA-ECC,ISO-27001",
                Version = "1.0",
                FileFormat = "docx",
                Tags = "access,request,provisioning",
                DisplayOrder = 15
            },
            new()
            {
                Code = "FRM-PER-001",
                TitleEn = "Policy Exception Request",
                TitleAr = "نموذج طلب استثناء من السياسة",
                DescriptionEn = "Form for requesting temporary policy exceptions with risk acceptance.",
                DescriptionAr = "نموذج لطلب استثناءات مؤقتة من السياسات مع قبول المخاطر.",
                Category = DocumentTemplateCategories.Form,
                Domain = GrcDomains.Governance,
                Version = "1.0",
                FileFormat = "docx",
                Tags = "exception,waiver,policy,risk",
                DisplayOrder = 16
            },
            new()
            {
                Code = "FRM-VRA-001",
                TitleEn = "Vendor Risk Assessment Form",
                TitleAr = "نموذج تقييم مخاطر الموردين",
                DescriptionEn = "Form for evaluating third-party vendor security and compliance posture.",
                DescriptionAr = "نموذج لتقييم وضع الأمن والامتثال لدى الموردين الخارجيين.",
                Category = DocumentTemplateCategories.Form,
                Domain = GrcDomains.Risk,
                FrameworkCodes = "ISO-27001,NCA-ECC",
                Version = "1.0",
                FileFormat = "xlsx",
                Tags = "vendor,supplier,third-party,risk",
                DisplayOrder = 17
            },

            // ==================== CHECKLISTS ====================
            new()
            {
                Code = "CHK-AUD-001",
                TitleEn = "Audit Checklist",
                TitleAr = "قائمة مراجعة التدقيق",
                DescriptionEn = "Standardized audit checklist with control references, criteria, and result tracking.",
                DescriptionAr = "قائمة مراجعة تدقيق موحدة مع مراجع الضوابط والمعايير وتتبع النتائج.",
                Category = DocumentTemplateCategories.Checklist,
                Domain = GrcDomains.Audit,
                Version = "1.0",
                FileFormat = "xlsx",
                Tags = "audit,checklist,internal,assessment",
                DisplayOrder = 20
            },
            new()
            {
                Code = "CHK-NCA-001",
                TitleEn = "NCA ECC Self-Assessment Checklist",
                TitleAr = "قائمة التقييم الذاتي لـ NCA ECC",
                DescriptionEn = "Self-assessment checklist aligned with NCA Essential Cybersecurity Controls.",
                DescriptionAr = "قائمة تقييم ذاتي متوافقة مع الضوابط الأساسية للأمن السيبراني للهيئة الوطنية للأمن السيبراني.",
                Category = DocumentTemplateCategories.Checklist,
                Domain = GrcDomains.Compliance,
                FrameworkCodes = "NCA-ECC",
                Version = "1.0",
                FileFormat = "xlsx",
                Tags = "nca,ecc,cybersecurity,self-assessment,ksa",
                DisplayOrder = 21
            },
            new()
            {
                Code = "CHK-PDPL-001",
                TitleEn = "PDPL Compliance Checklist",
                TitleAr = "قائمة الامتثال لنظام حماية البيانات الشخصية",
                DescriptionEn = "Compliance checklist for Saudi Personal Data Protection Law requirements.",
                DescriptionAr = "قائمة امتثال لمتطلبات نظام حماية البيانات الشخصية السعودي.",
                Category = DocumentTemplateCategories.Checklist,
                Domain = GrcDomains.Privacy,
                FrameworkCodes = "PDPL",
                Version = "1.0",
                FileFormat = "xlsx",
                Tags = "pdpl,privacy,data,protection,ksa",
                DisplayOrder = 22
            },

            // ==================== REPORTS ====================
            new()
            {
                Code = "RPT-MMT-001",
                TitleEn = "Meeting Minutes Template",
                TitleAr = "قالب محضر الاجتماع",
                DescriptionEn = "Template for governance committee meeting minutes with resolutions and action tracking.",
                DescriptionAr = "قالب لمحاضر اجتماعات لجان الحوكمة مع القرارات وتتبع الإجراءات.",
                Category = DocumentTemplateCategories.Report,
                Domain = GrcDomains.Governance,
                Version = "1.0",
                FileFormat = "docx",
                SectionsJson = "[\"Meeting Header\",\"Attendance\",\"Agenda\",\"Minutes\",\"Resolutions\",\"Next Meeting\",\"Signatures\"]",
                Tags = "meeting,minutes,board,committee",
                DisplayOrder = 30
            },
            new()
            {
                Code = "RPT-BCT-001",
                TitleEn = "BCP Test Report",
                TitleAr = "تقرير اختبار استمرارية الأعمال",
                DescriptionEn = "Template for documenting business continuity and disaster recovery test results.",
                DescriptionAr = "قالب لتوثيق نتائج اختبار استمرارية الأعمال والتعافي من الكوارث.",
                Category = DocumentTemplateCategories.Report,
                Domain = GrcDomains.Operations,
                FrameworkCodes = "ISO-22301",
                Version = "1.0",
                FileFormat = "docx",
                Tags = "bcp,dr,continuity,disaster,recovery",
                DisplayOrder = 31
            },

            // ==================== AGREEMENTS ====================
            new()
            {
                Code = "AGR-DPA-001",
                TitleEn = "Data Processing Agreement",
                TitleAr = "اتفاقية معالجة البيانات",
                DescriptionEn = "Standard DPA template for third-party data processors under PDPL.",
                DescriptionAr = "نموذج اتفاقية معالجة البيانات للمعالجين الخارجيين بموجب نظام حماية البيانات الشخصية.",
                Category = DocumentTemplateCategories.Agreement,
                Domain = GrcDomains.Privacy,
                FrameworkCodes = "PDPL",
                Version = "1.0",
                FileFormat = "docx",
                Tags = "dpa,processor,privacy,contract",
                DisplayOrder = 40
            },
            new()
            {
                Code = "AGR-NDA-001",
                TitleEn = "Non-Disclosure Agreement",
                TitleAr = "اتفاقية عدم الإفصاح",
                DescriptionEn = "Standard NDA template for protecting confidential information.",
                DescriptionAr = "نموذج اتفاقية عدم الإفصاح لحماية المعلومات السرية.",
                Category = DocumentTemplateCategories.Agreement,
                Domain = GrcDomains.Security,
                Version = "1.0",
                FileFormat = "docx",
                Tags = "nda,confidentiality,agreement",
                DisplayOrder = 41
            },

            // ==================== GUIDES ====================
            new()
            {
                Code = "GDE-DCG-001",
                TitleEn = "Data Classification Guide",
                TitleAr = "دليل تصنيف البيانات",
                DescriptionEn = "User guide for classifying data according to sensitivity levels with examples.",
                DescriptionAr = "دليل المستخدم لتصنيف البيانات حسب مستويات الحساسية مع أمثلة.",
                Category = DocumentTemplateCategories.Guide,
                Domain = GrcDomains.Security,
                FrameworkCodes = "NCA-ECC,ISO-27001",
                Version = "1.0",
                FileFormat = "pdf",
                Tags = "guide,classification,data,labeling",
                DisplayOrder = 50
            },

            // ==================== CERTIFICATES ====================
            new()
            {
                Code = "CRT-TRN-001",
                TitleEn = "Training Completion Certificate",
                TitleAr = "شهادة إتمام التدريب",
                DescriptionEn = "Certificate template for training completion with customizable fields.",
                DescriptionAr = "قالب شهادة إتمام التدريب مع حقول قابلة للتخصيص.",
                Category = DocumentTemplateCategories.Certificate,
                Domain = GrcDomains.Training,
                Version = "1.0",
                FileFormat = "docx",
                Tags = "certificate,training,completion,awareness",
                DisplayOrder = 60
            },
            new()
            {
                Code = "CRT-SAA-001",
                TitleEn = "Security Awareness Acknowledgment",
                TitleAr = "إقرار الوعي الأمني",
                DescriptionEn = "Employee acknowledgment form for security awareness training completion.",
                DescriptionAr = "نموذج إقرار الموظف بإتمام تدريب الوعي الأمني.",
                Category = DocumentTemplateCategories.Form,
                Domain = GrcDomains.Training,
                FrameworkCodes = "NCA-ECC",
                Version = "1.0",
                FileFormat = "docx",
                Tags = "acknowledgment,awareness,security,training",
                DisplayOrder = 61
            }
        };

        context.DocumentTemplates.AddRange(templates);
        await context.SaveChangesAsync();
    }
}
