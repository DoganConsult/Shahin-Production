using GrcMvc.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Data.Seeds;

/// <summary>
/// Complete SAMA Cybersecurity Framework (SAMA-CSF) - 85 Controls
/// Reference: https://www.sama.gov.sa/en-US/Laws/BankingRules/Cyber-Security-Framework.pdf
///
/// Domains:
/// 1. Cybersecurity Leadership and Governance (CLG) - 12 controls
/// 2. Cybersecurity Risk Management and Compliance (CRM) - 10 controls
/// 3. Cybersecurity Operations and Technology (COT) - 35 controls
/// 4. Third-Party Security (TPS) - 10 controls
/// 5. Cybersecurity Resilience (CSR) - 18 controls
/// Total: 85 controls
///
/// Auto-Mapping: Each control maps to NIST CSF and ISO 27001:2022
/// </summary>
public static class SamaCsfFullSeeds
{
    public static async Task SeedAsync(GrcDbContext context, ILogger logger)
    {
        try
        {
            var existingCount = await context.FrameworkControls
                .CountAsync(c => c.FrameworkCode == "SAMA-CSF");

            if (existingCount >= 80)
            {
                logger.LogInformation("✅ SAMA-CSF controls already exist ({Count}). Skipping seed.", existingCount);
                return;
            }

            // Clear existing if partial
            if (existingCount > 0)
            {
                var existing = await context.FrameworkControls
                    .Where(c => c.FrameworkCode == "SAMA-CSF")
                    .ToListAsync();
                context.FrameworkControls.RemoveRange(existing);
                await context.SaveChangesAsync();
            }

            var controls = GetAllSamaCsfControls();
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

    private static List<FrameworkControl> GetAllSamaCsfControls()
    {
        var controls = new List<FrameworkControl>();

        controls.AddRange(GetLeadershipGovernanceControls());
        controls.AddRange(GetRiskManagementControls());
        controls.AddRange(GetOperationsTechnologyControls());
        controls.AddRange(GetThirdPartySecurityControls());
        controls.AddRange(GetResilienceControls());

        return controls;
    }

    #region Domain 1: Cybersecurity Leadership and Governance (12 Controls)
    private static List<FrameworkControl> GetLeadershipGovernanceControls()
    {
        return new List<FrameworkControl>
        {
            CreateControl("CLG-1", "Leadership",
                "هيكل حوكمة الأمن السيبراني",
                "Cybersecurity Governance Structure",
                "يجب على المؤسسة المالية إنشاء هيكل حوكمة للأمن السيبراني يتضمن تحديد واضح للأدوار والمسؤوليات والسلطات على جميع المستويات التنظيمية",
                "The financial institution must establish a cybersecurity governance structure with clear definition of roles, responsibilities, and authorities at all organizational levels",
                "يجب تعيين CISO وتحديد خطوط الإبلاغ للإدارة العليا ومجلس الإدارة",
                "Appoint CISO and define reporting lines to senior management and board of directors",
                "POLICY_DOC|ORG_CHART|JOB_DESCRIPTION",
                "A.5.1|A.5.2|A.5.3", "ID.GV-1|ID.GV-2", "preventive", 1, "EP_GOV_001"),

            CreateControl("CLG-2", "Leadership",
                "سياسة الأمن السيبراني المعتمدة",
                "Approved Cybersecurity Policy",
                "يجب وضع سياسة شاملة للأمن السيبراني معتمدة من مجلس الإدارة وموزعة على جميع الموظفين والأطراف المعنية",
                "Develop comprehensive cybersecurity policy approved by board of directors and distributed to all employees and relevant parties",
                "يجب مراجعة السياسة سنوياً وتحديثها عند الحاجة",
                "Review policy annually and update when needed",
                "POLICY_DOC|BOARD_APPROVAL|DISTRIBUTION_LOG",
                "A.5.1|A.5.4", "ID.GV-1|ID.GV-3", "preventive", 1, "EP_GOV_001"),

            CreateControl("CLG-3", "Leadership",
                "لجنة الأمن السيبراني",
                "Cybersecurity Committee",
                "يجب إنشاء لجنة للأمن السيبراني على مستوى الإدارة العليا لمتابعة تنفيذ استراتيجية الأمن السيبراني",
                "Establish cybersecurity committee at senior management level to oversee cybersecurity strategy implementation",
                "يجب أن تجتمع اللجنة ربع سنوياً على الأقل وترفع تقاريرها لمجلس الإدارة",
                "Committee must meet at least quarterly and report to board of directors",
                "COMMITTEE_CHARTER|MEETING_MINUTES",
                "A.5.1|A.5.2", "ID.GV-2", "preventive", 2, "EP_GOV_001"),

            CreateControl("CLG-4", "Leadership",
                "استراتيجية الأمن السيبراني",
                "Cybersecurity Strategy",
                "يجب وضع استراتيجية للأمن السيبراني متوافقة مع استراتيجية المؤسسة وأهداف الأعمال ومراجعتها سنوياً",
                "Develop cybersecurity strategy aligned with institution strategy and business objectives, reviewed annually",
                "يجب أن تتضمن الاستراتيجية أهداف قابلة للقياس ومؤشرات أداء",
                "Strategy must include measurable objectives and performance indicators",
                "STRATEGY_DOC|KPI_REPORT",
                "A.5.1", "ID.GV-1", "preventive", 1, "EP_GOV_001"),

            CreateControl("CLG-5", "Leadership",
                "تخصيص موارد الأمن السيبراني",
                "Cybersecurity Resource Allocation",
                "يجب تخصيص موارد كافية (مالية وبشرية وتقنية) لتنفيذ برنامج الأمن السيبراني",
                "Allocate sufficient resources (financial, human, and technical) to implement cybersecurity program",
                "يجب مراجعة كفاية الموارد سنوياً ضمن عملية التخطيط",
                "Review resource adequacy annually as part of planning process",
                "BUDGET_DOC|RESOURCE_PLAN",
                "A.5.2|A.5.3", "ID.GV-4", "preventive", 2, "EP_GOV_002"),

            CreateControl("CLG-6", "Leadership",
                "مسؤول الأمن السيبراني",
                "Chief Information Security Officer",
                "يجب تعيين مسؤول للأمن السيبراني (CISO) بصلاحيات وموارد كافية وخط إبلاغ مباشر للإدارة العليا",
                "Appoint Chief Information Security Officer (CISO) with adequate authority, resources, and direct reporting to senior management",
                "يجب أن يكون CISO مستقلاً عن إدارة تقنية المعلومات",
                "CISO must be independent from IT management",
                "JOB_DESCRIPTION|ORG_CHART|APPOINTMENT_LETTER",
                "A.5.2|A.5.3", "ID.GV-2", "preventive", 2, "EP_GOV_001"),

            CreateControl("CLG-7", "Leadership",
                "التقارير الدورية للإدارة العليا",
                "Periodic Reports to Senior Management",
                "يجب رفع تقارير دورية عن حالة الأمن السيبراني للإدارة العليا ومجلس الإدارة",
                "Submit periodic reports on cybersecurity status to senior management and board of directors",
                "يجب أن تتضمن التقارير: المخاطر، الحوادث، الامتثال، مؤشرات الأداء",
                "Reports must include: risks, incidents, compliance, performance indicators",
                "MANAGEMENT_REPORT|BOARD_REPORT",
                "A.5.1|A.5.4", "ID.GV-4", "detective", 2, "EP_CMP_001"),

            CreateControl("CLG-8", "Leadership",
                "التوعية الأمنية للمجلس والإدارة",
                "Security Awareness for Board and Management",
                "يجب توفير برنامج توعية بالأمن السيبراني لأعضاء مجلس الإدارة والإدارة العليا",
                "Provide cybersecurity awareness program for board members and senior management",
                "يجب تقديم التوعية سنوياً وعند ظهور تهديدات جديدة",
                "Provide awareness annually and when new threats emerge",
                "TRAINING_RECORD|PRESENTATION",
                "A.6.3", "PR.AT-1", "preventive", 2, "EP_HRS_002"),

            CreateControl("CLG-9", "Leadership",
                "سياسة الاستخدام المقبول",
                "Acceptable Use Policy",
                "يجب وضع سياسة للاستخدام المقبول لأصول المعلومات والتقنية وتوقيعها من جميع الموظفين",
                "Establish acceptable use policy for information and technology assets, signed by all employees",
                "يجب مراجعة السياسة سنوياً والحصول على إقرار متجدد",
                "Review policy annually and obtain renewed acknowledgment",
                "AUP_POLICY|ACKNOWLEDGMENT_LOG",
                "A.5.10", "PR.AT-1", "preventive", 1, "EP_GOV_002"),

            CreateControl("CLG-10", "Leadership",
                "الفصل بين المهام",
                "Segregation of Duties",
                "يجب تطبيق مبدأ الفصل بين المهام لمنع تضارب المصالح وتقليل مخاطر الاحتيال",
                "Apply segregation of duties principle to prevent conflicts of interest and reduce fraud risks",
                "يجب فصل وظائف التطوير والتشغيل والأمن",
                "Separate development, operations, and security functions",
                "SOD_MATRIX|ROLE_DEFINITION",
                "A.5.3|A.5.4", "PR.AC-4", "preventive", 2, "EP_IAM_001"),

            CreateControl("CLG-11", "Leadership",
                "إدارة الاستثناءات الأمنية",
                "Security Exception Management",
                "يجب إنشاء عملية رسمية لإدارة الاستثناءات من سياسات وضوابط الأمن السيبراني",
                "Establish formal process for managing exceptions to cybersecurity policies and controls",
                "يجب توثيق جميع الاستثناءات مع تبرير العمل ومدة الاستثناء",
                "Document all exceptions with business justification and exception duration",
                "EXCEPTION_REGISTER|APPROVAL_WORKFLOW",
                "A.5.1", "ID.GV-3", "detective", 2, "EP_CMP_001"),

            CreateControl("CLG-12", "Leadership",
                "التنسيق مع الجهات الرقابية",
                "Coordination with Regulators",
                "يجب التنسيق مع ساما والجهات الرقابية الأخرى والإبلاغ عن الحوادث الجسيمة",
                "Coordinate with SAMA and other regulators and report major incidents",
                "يجب الإبلاغ عن الحوادث الجسيمة خلال 24 ساعة",
                "Report major incidents within 24 hours",
                "CORRESPONDENCE_LOG|INCIDENT_NOTIFICATION",
                "A.5.5|A.5.6", "ID.GV-3", "detective", 2, "EP_CMP_001"),
        };
    }
    #endregion

    #region Domain 2: Cybersecurity Risk Management and Compliance (10 Controls)
    private static List<FrameworkControl> GetRiskManagementControls()
    {
        return new List<FrameworkControl>
        {
            CreateControl("CRM-1", "Risk",
                "إطار إدارة المخاطر السيبرانية",
                "Cyber Risk Management Framework",
                "يجب إنشاء إطار شامل لإدارة المخاطر السيبرانية متكامل مع إطار إدارة المخاطر المؤسسية",
                "Establish comprehensive cyber risk management framework integrated with enterprise risk management framework",
                "يجب أن يتضمن الإطار: تحديد المخاطر، التقييم، المعالجة، المراقبة",
                "Framework must include: risk identification, assessment, treatment, monitoring",
                "RISK_FRAMEWORK|METHODOLOGY_DOC",
                "A.5.7|A.5.8", "ID.RM-1|ID.RM-2|ID.RM-3", "preventive", 2, "EP_GOV_002"),

            CreateControl("CRM-2", "Risk",
                "تقييم المخاطر السيبرانية",
                "Cyber Risk Assessment",
                "يجب إجراء تقييم دوري وشامل للمخاطر السيبرانية لجميع الأصول والعمليات الحرجة",
                "Conduct periodic and comprehensive cyber risk assessment for all critical assets and processes",
                "يجب إجراء التقييم سنوياً وعند التغييرات الجوهرية",
                "Conduct assessment annually and upon significant changes",
                "RISK_ASSESSMENT|RISK_REGISTER",
                "A.5.7|A.8.2|A.8.3", "ID.RA-1|ID.RA-2|ID.RA-3", "detective", 2, "EP_GOV_002"),

            CreateControl("CRM-3", "Risk",
                "قبول المخاطر",
                "Risk Acceptance",
                "يجب توثيق واعتماد المخاطر المقبولة من قبل أصحاب المخاطر المعتمدين",
                "Document and approve accepted risks by authorized risk owners",
                "يجب مراجعة المخاطر المقبولة ربع سنوياً",
                "Review accepted risks quarterly",
                "RISK_ACCEPTANCE|APPROVAL_LOG",
                "A.5.7", "ID.RM-1", "detective", 2, "EP_GOV_002"),

            CreateControl("CRM-4", "Risk",
                "معالجة المخاطر",
                "Risk Treatment",
                "يجب وضع خطط لمعالجة المخاطر غير المقبولة وتتبع تنفيذها",
                "Develop plans to treat unacceptable risks and track implementation",
                "يجب تحديد مالك وجدول زمني لكل إجراء معالجة",
                "Assign owner and timeline for each treatment action",
                "TREATMENT_PLAN|REMEDIATION_TRACKER",
                "A.5.7", "ID.RM-1", "corrective", 2, "EP_GOV_002"),

            CreateControl("CRM-5", "Risk",
                "مؤشرات المخاطر الرئيسية",
                "Key Risk Indicators",
                "يجب تحديد ومراقبة مؤشرات المخاطر الرئيسية للأمن السيبراني",
                "Identify and monitor key risk indicators for cybersecurity",
                "يجب تحديد عتبات التنبيه والتصعيد",
                "Define alerting and escalation thresholds",
                "KRI_REPORT|DASHBOARD",
                "A.5.7", "ID.RM-3", "detective", 2, "EP_CMP_001"),

            CreateControl("CRM-6", "Risk",
                "الامتثال للمتطلبات التنظيمية",
                "Regulatory Compliance",
                "يجب ضمان الامتثال لجميع المتطلبات التنظيمية والقانونية المتعلقة بالأمن السيبراني",
                "Ensure compliance with all regulatory and legal requirements related to cybersecurity",
                "يجب الاحتفاظ بسجل للمتطلبات وتتبع حالة الامتثال",
                "Maintain register of requirements and track compliance status",
                "COMPLIANCE_REGISTER|ASSESSMENT_REPORT",
                "A.5.31|A.5.32|A.5.33", "ID.GV-3", "detective", 2, "EP_CMP_001"),

            CreateControl("CRM-7", "Risk",
                "التدقيق الداخلي للأمن السيبراني",
                "Cybersecurity Internal Audit",
                "يجب إجراء تدقيق داخلي مستقل للأمن السيبراني بشكل دوري",
                "Conduct independent internal cybersecurity audit periodically",
                "يجب إجراء التدقيق سنوياً ومتابعة الملاحظات",
                "Conduct audit annually and follow up on findings",
                "AUDIT_REPORT|REMEDIATION_PLAN",
                "A.5.35|A.5.36", "ID.GV-3", "detective", 3, "EP_CMP_002"),

            CreateControl("CRM-8", "Risk",
                "التدقيق الخارجي للأمن السيبراني",
                "Cybersecurity External Audit",
                "يجب إجراء تدقيق خارجي مستقل للأمن السيبراني بشكل دوري",
                "Conduct independent external cybersecurity audit periodically",
                "يجب إجراء تدقيق خارجي كل ثلاث سنوات على الأقل",
                "Conduct external audit at least every three years",
                "EXTERNAL_AUDIT_REPORT|CERTIFICATION",
                "A.5.35|A.5.36", "ID.GV-3", "detective", 3, "EP_CMP_002"),

            CreateControl("CRM-9", "Risk",
                "إدارة سياسات الأمن السيبراني",
                "Cybersecurity Policy Management",
                "يجب إدارة دورة حياة سياسات الأمن السيبراني من الإعداد للتقاعد",
                "Manage cybersecurity policy lifecycle from development to retirement",
                "يجب مراجعة السياسات سنوياً وتحديثها عند الحاجة",
                "Review policies annually and update when needed",
                "POLICY_REGISTER|VERSION_HISTORY",
                "A.5.1|A.5.37", "ID.GV-1", "preventive", 2, "EP_GOV_002"),

            CreateControl("CRM-10", "Risk",
                "إدارة المعايير والإجراءات",
                "Standards and Procedures Management",
                "يجب وضع معايير وإجراءات تفصيلية لتنفيذ سياسات الأمن السيبراني",
                "Develop detailed standards and procedures to implement cybersecurity policies",
                "يجب ربط المعايير بالسياسات ذات الصلة",
                "Link standards to related policies",
                "STANDARDS_DOC|PROCEDURE_DOC",
                "A.5.1", "ID.GV-1", "preventive", 2, "EP_GOV_002"),
        };
    }
    #endregion

    #region Domain 3: Cybersecurity Operations and Technology (35 Controls)
    private static List<FrameworkControl> GetOperationsTechnologyControls()
    {
        return new List<FrameworkControl>
        {
            // Identity and Access Management (8 controls)
            CreateControl("COT-1", "Operations",
                "سياسة إدارة الهوية والوصول",
                "Identity and Access Management Policy",
                "يجب وضع سياسة شاملة لإدارة الهوية والوصول",
                "Establish comprehensive identity and access management policy",
                "يجب أن تتضمن: دورة حياة الحساب، المصادقة، التفويض",
                "Must include: account lifecycle, authentication, authorization",
                "IAM_POLICY|PROCEDURE_DOC",
                "A.5.15|A.5.16|A.5.17", "PR.AC-1|PR.AC-2", "preventive", 1, "EP_IAM_001"),

            CreateControl("COT-2", "Operations",
                "إدارة حسابات المستخدمين",
                "User Account Management",
                "يجب تطبيق عمليات رسمية لإنشاء وتعديل وإلغاء حسابات المستخدمين",
                "Apply formal processes for creating, modifying, and deleting user accounts",
                "يجب ربط الحسابات بإدارة الموارد البشرية للتفعيل والإلغاء التلقائي",
                "Link accounts to HR management for automatic activation and deactivation",
                "ACCOUNT_PROCEDURE|USER_LIST",
                "A.5.16|A.5.18", "PR.AC-1", "preventive", 1, "EP_IAM_001"),

            CreateControl("COT-3", "Operations",
                "إدارة الصلاحيات الممتازة",
                "Privileged Access Management",
                "يجب تطبيق ضوابط مشددة للصلاحيات الممتازة والإدارية",
                "Apply stringent controls for privileged and administrative access",
                "يجب استخدام PAM وتسجيل الجلسات والموافقة المسبقة",
                "Use PAM, session recording, and prior approval",
                "PAM_CONFIG|SESSION_LOG|APPROVAL_WORKFLOW",
                "A.5.18|A.8.2", "PR.AC-4", "preventive", 3, "EP_IAM_003"),

            CreateControl("COT-4", "Operations",
                "المصادقة متعددة العوامل",
                "Multi-Factor Authentication",
                "يجب تطبيق المصادقة متعددة العوامل للأنظمة الحساسة والوصول عن بعد",
                "Apply multi-factor authentication for sensitive systems and remote access",
                "يجب استخدام MFA لجميع حسابات الصلاحيات الممتازة",
                "Use MFA for all privileged accounts",
                "MFA_CONFIG|DEPLOYMENT_STATUS",
                "A.5.17|A.8.5", "PR.AC-7", "preventive", 2, "EP_IAM_002"),

            CreateControl("COT-5", "Operations",
                "مراجعة الصلاحيات الدورية",
                "Periodic Access Review",
                "يجب مراجعة صلاحيات الوصول بشكل دوري للتأكد من ملاءمتها",
                "Review access privileges periodically to ensure appropriateness",
                "يجب المراجعة ربع سنوياً للصلاحيات الممتازة وسنوياً للعادية",
                "Review quarterly for privileged and annually for regular access",
                "ACCESS_REVIEW_REPORT|RECERTIFICATION_LOG",
                "A.5.18", "PR.AC-1", "detective", 2, "EP_IAM_001"),

            CreateControl("COT-6", "Operations",
                "إدارة كلمات المرور",
                "Password Management",
                "يجب تطبيق سياسة كلمات مرور قوية متوافقة مع أفضل الممارسات",
                "Apply strong password policy aligned with best practices",
                "يجب: 14 حرف كحد أدنى، تعقيد، عدم تكرار آخر 24 كلمة مرور",
                "Must: 14 characters minimum, complexity, no reuse of last 24 passwords",
                "PASSWORD_POLICY|AD_CONFIG",
                "A.5.17", "PR.AC-1", "preventive", 1, "EP_IAM_002"),

            CreateControl("COT-7", "Operations",
                "تسجيل الدخول الموحد",
                "Single Sign-On",
                "يجب تطبيق تسجيل الدخول الموحد حيثما أمكن لتحسين الأمان وتجربة المستخدم",
                "Apply single sign-on where possible to improve security and user experience",
                "يجب دمج التطبيقات الحرجة مع حل SSO",
                "Integrate critical applications with SSO solution",
                "SSO_CONFIG|APPLICATION_LIST",
                "A.5.17", "PR.AC-1", "preventive", 2, "EP_IAM_002"),

            CreateControl("COT-8", "Operations",
                "إدارة الهوية المميزة للخدمات",
                "Service Account Management",
                "يجب إدارة حسابات الخدمات بضوابط أمنية مشددة",
                "Manage service accounts with stringent security controls",
                "يجب تغيير كلمات المرور دورياً وتوثيق الاستخدام",
                "Change passwords periodically and document usage",
                "SERVICE_ACCOUNT_LIST|ROTATION_LOG",
                "A.5.18|A.8.4", "PR.AC-4", "preventive", 2, "EP_IAM_003"),

            // Asset Management (5 controls)
            CreateControl("COT-9", "Operations",
                "جرد الأصول المعلوماتية",
                "Information Asset Inventory",
                "يجب إنشاء وصيانة سجل شامل ودقيق لجميع الأصول المعلوماتية",
                "Establish and maintain comprehensive and accurate inventory of all information assets",
                "يجب تحديث السجل عند كل تغيير وإجراء جرد دوري",
                "Update inventory upon each change and conduct periodic review",
                "ASSET_INVENTORY|CMDB",
                "A.5.9|A.5.10|A.5.11", "ID.AM-1|ID.AM-2", "preventive", 1, "EP_ASM_001"),

            CreateControl("COT-10", "Operations",
                "تصنيف البيانات",
                "Data Classification",
                "يجب تصنيف البيانات حسب مستوى حساسيتها وتطبيق الحماية المناسبة",
                "Classify data according to sensitivity level and apply appropriate protection",
                "يجب استخدام: سري للغاية، سري، داخلي، عام",
                "Use: Top Secret, Secret, Internal, Public",
                "CLASSIFICATION_POLICY|DATA_INVENTORY",
                "A.5.12|A.5.13", "ID.AM-5", "preventive", 1, "EP_ASM_002"),

            CreateControl("COT-11", "Operations",
                "إدارة وسائط التخزين",
                "Storage Media Management",
                "يجب إدارة وسائط التخزين بما يضمن حماية البيانات المخزنة عليها",
                "Manage storage media to ensure protection of stored data",
                "يجب تشفير الوسائط المحمولة وتتبعها",
                "Encrypt portable media and track them",
                "MEDIA_POLICY|TRACKING_LOG",
                "A.7.10|A.7.14", "PR.DS-3", "preventive", 2, "EP_ASM_002"),

            CreateControl("COT-12", "Operations",
                "التخلص الآمن من الأصول",
                "Secure Asset Disposal",
                "يجب التخلص من الأصول بشكل آمن يضمن عدم استرداد البيانات",
                "Dispose of assets securely to ensure data cannot be recovered",
                "يجب استخدام معايير محو معتمدة أو الإتلاف المادي",
                "Use approved wiping standards or physical destruction",
                "DISPOSAL_PROCEDURE|DESTRUCTION_CERT",
                "A.7.10|A.7.14", "PR.DS-3", "preventive", 2, "EP_ASM_002"),

            CreateControl("COT-13", "Operations",
                "إدارة تغييرات الأصول",
                "Asset Change Management",
                "يجب إدارة تغييرات الأصول من خلال عملية التحكم في التغييرات",
                "Manage asset changes through change control process",
                "يجب توثيق والموافقة على جميع التغييرات قبل التنفيذ",
                "Document and approve all changes before implementation",
                "CHANGE_PROCEDURE|CHANGE_LOG",
                "A.8.32", "PR.IP-3", "preventive", 2, "EP_OPS_001"),

            // Network and Infrastructure Security (10 controls)
            CreateControl("COT-14", "Operations",
                "هندسة أمن الشبكات",
                "Network Security Architecture",
                "يجب تصميم الشبكات وفق مبادئ الأمن والعمق الدفاعي",
                "Design networks according to security principles and defense in depth",
                "يجب توثيق البنية الشبكية ومراجعتها سنوياً",
                "Document network architecture and review annually",
                "NETWORK_DIAGRAM|SECURITY_ARCHITECTURE",
                "A.8.20|A.8.21|A.8.22", "PR.AC-5|PR.DS-5", "preventive", 2, "EP_NET_001"),

            CreateControl("COT-15", "Operations",
                "تجزئة الشبكات",
                "Network Segmentation",
                "يجب تجزئة الشبكات لعزل الأنظمة حسب مستوى حساسيتها ووظيفتها",
                "Segment networks to isolate systems according to sensitivity level and function",
                "يجب فصل: شبكة الإنتاج، التطوير، الإدارة، DMZ",
                "Separate: production, development, management, DMZ networks",
                "VLAN_CONFIG|FIREWALL_RULES",
                "A.8.22", "PR.AC-5", "preventive", 2, "EP_NET_001"),

            CreateControl("COT-16", "Operations",
                "إدارة جدران الحماية",
                "Firewall Management",
                "يجب نشر وإدارة جدران الحماية لحماية محيط الشبكة والقطاعات الداخلية",
                "Deploy and manage firewalls to protect network perimeter and internal segments",
                "يجب مراجعة القواعد ربع سنوياً وتوثيق جميع التغييرات",
                "Review rules quarterly and document all changes",
                "FIREWALL_RULES|RULE_REVIEW_LOG",
                "A.8.20|A.8.21", "PR.AC-5", "preventive", 2, "EP_NET_001"),

            CreateControl("COT-17", "Operations",
                "كشف ومنع الاختراق",
                "Intrusion Detection and Prevention",
                "يجب نشر أنظمة كشف ومنع الاختراق لمراقبة الشبكة",
                "Deploy intrusion detection and prevention systems to monitor network",
                "يجب تحديث التوقيعات ومراجعة التنبيهات يومياً",
                "Update signatures and review alerts daily",
                "IDS_IPS_CONFIG|ALERT_REVIEW_LOG",
                "A.8.16", "DE.CM-1|DE.CM-4", "detective", 2, "EP_NET_001"),

            CreateControl("COT-18", "Operations",
                "الوصول الآمن عن بعد",
                "Secure Remote Access",
                "يجب تطبيق ضوابط الوصول الآمن عن بعد للموظفين والموردين",
                "Apply secure remote access controls for employees and vendors",
                "يجب استخدام VPN مع MFA وتشفير قوي",
                "Use VPN with MFA and strong encryption",
                "VPN_CONFIG|REMOTE_ACCESS_POLICY",
                "A.5.14|A.6.7", "PR.AC-3", "preventive", 2, "EP_NET_001"),

            CreateControl("COT-19", "Operations",
                "أمن الشبكات اللاسلكية",
                "Wireless Network Security",
                "يجب تطبيق ضوابط أمنية للشبكات اللاسلكية",
                "Apply security controls for wireless networks",
                "يجب استخدام WPA3/WPA2-Enterprise وفصل شبكة الضيوف",
                "Use WPA3/WPA2-Enterprise and separate guest network",
                "WIRELESS_CONFIG|AUDIT_LOG",
                "A.8.20", "PR.AC-5", "preventive", 2, "EP_NET_001"),

            CreateControl("COT-20", "Operations",
                "أمن البريد الإلكتروني",
                "Email Security",
                "يجب تطبيق ضوابط أمنية شاملة للبريد الإلكتروني",
                "Apply comprehensive email security controls",
                "يجب تفعيل SPF/DKIM/DMARC وفلترة البريد المزعج والتصيد",
                "Enable SPF/DKIM/DMARC and spam/phishing filtering",
                "EMAIL_SECURITY_CONFIG|DMARC_REPORT",
                "A.8.20|A.8.23", "PR.DS-5", "preventive", 2, "EP_NET_002"),

            CreateControl("COT-21", "Operations",
                "التشفير أثناء النقل",
                "Encryption in Transit",
                "يجب تشفير البيانات الحساسة أثناء النقل عبر الشبكات",
                "Encrypt sensitive data in transit across networks",
                "يجب استخدام TLS 1.2 أو أحدث لجميع الاتصالات",
                "Use TLS 1.2 or newer for all communications",
                "TLS_CONFIG|CERTIFICATE_INVENTORY",
                "A.8.24", "PR.DS-2", "preventive", 2, "EP_NET_002"),

            CreateControl("COT-22", "Operations",
                "التشفير في حالة السكون",
                "Encryption at Rest",
                "يجب تشفير البيانات الحساسة المخزنة",
                "Encrypt stored sensitive data",
                "يجب استخدام AES-256 للبيانات عالية الحساسية",
                "Use AES-256 for highly sensitive data",
                "ENCRYPTION_CONFIG|KEY_MANAGEMENT",
                "A.8.24", "PR.DS-1", "preventive", 2, "EP_NET_002"),

            CreateControl("COT-23", "Operations",
                "إدارة مفاتيح التشفير",
                "Cryptographic Key Management",
                "يجب إدارة مفاتيح التشفير بشكل آمن طوال دورة حياتها",
                "Manage cryptographic keys securely throughout their lifecycle",
                "يجب استخدام HSM للمفاتيح الحساسة",
                "Use HSM for sensitive keys",
                "KEY_MANAGEMENT_PROCEDURE|HSM_CONFIG",
                "A.8.24", "PR.DS-1", "preventive", 3, "EP_NET_002"),

            // Endpoint and Application Security (7 controls)
            CreateControl("COT-24", "Operations",
                "حماية نقاط النهاية",
                "Endpoint Protection",
                "يجب نشر حلول حماية نقاط النهاية على جميع الأجهزة",
                "Deploy endpoint protection solutions on all devices",
                "يجب تفعيل EDR وتحديث التوقيعات تلقائياً",
                "Enable EDR and automatic signature updates",
                "EDR_CONFIG|DEPLOYMENT_STATUS",
                "A.8.7", "DE.CM-4|DE.CM-5", "preventive", 2, "EP_OPS_002"),

            CreateControl("COT-25", "Operations",
                "تقوية الأنظمة",
                "System Hardening",
                "يجب تطبيق معايير التقوية على جميع الأنظمة والأجهزة",
                "Apply hardening standards to all systems and devices",
                "يجب استخدام CIS Benchmarks أو ما يعادلها",
                "Use CIS Benchmarks or equivalent",
                "HARDENING_STANDARDS|COMPLIANCE_REPORT",
                "A.8.9", "PR.IP-1", "preventive", 2, "EP_OPS_001"),

            CreateControl("COT-26", "Operations",
                "إدارة التحديثات والترقيعات",
                "Patch Management",
                "يجب تطبيق برنامج لإدارة التحديثات والترقيعات الأمنية",
                "Implement program for managing security updates and patches",
                "يجب تطبيق الترقيعات الحرجة خلال 14 يوماً",
                "Apply critical patches within 14 days",
                "PATCH_PROCEDURE|PATCH_STATUS",
                "A.8.8", "PR.IP-12", "preventive", 2, "EP_OPS_002"),

            CreateControl("COT-27", "Operations",
                "إدارة الثغرات الأمنية",
                "Vulnerability Management",
                "يجب إنشاء برنامج لإدارة الثغرات الأمنية",
                "Establish vulnerability management program",
                "يجب الفحص الشهري ومعالجة الثغرات الحرجة خلال 15 يوماً",
                "Monthly scanning and critical vulnerability remediation within 15 days",
                "VULN_SCAN_REPORT|REMEDIATION_TRACKER",
                "A.8.8", "ID.RA-1|DE.CM-8", "detective", 2, "EP_VUL_001"),

            CreateControl("COT-28", "Operations",
                "اختبار الاختراق",
                "Penetration Testing",
                "يجب إجراء اختبار اختراق دوري للأنظمة الحساسة",
                "Conduct periodic penetration testing for sensitive systems",
                "يجب الاختبار سنوياً وعند التغييرات الجوهرية",
                "Test annually and upon significant changes",
                "PENTEST_REPORT|REMEDIATION_PLAN",
                "A.8.8", "ID.RA-1", "detective", 3, "EP_VUL_001"),

            CreateControl("COT-29", "Operations",
                "أمن تطبيقات الويب",
                "Web Application Security",
                "يجب تطبيق ضوابط أمنية لتطبيقات الويب",
                "Apply security controls for web applications",
                "يجب إجراء DAST وSAST قبل الإطلاق وبشكل دوري",
                "Conduct DAST and SAST before release and periodically",
                "WEB_APP_SCAN|WAF_CONFIG",
                "A.8.26|A.8.28", "PR.DS-5", "preventive", 3, "EP_VUL_002"),

            CreateControl("COT-30", "Operations",
                "دورة حياة التطوير الآمن",
                "Secure Development Lifecycle",
                "يجب دمج الأمان في جميع مراحل دورة حياة تطوير البرمجيات",
                "Integrate security into all phases of software development lifecycle",
                "يجب تطبيق: نمذجة التهديدات، مراجعة الكود، اختبار الأمان",
                "Apply: threat modeling, code review, security testing",
                "SDLC_PROCEDURE|SECURITY_GATE_CHECKLIST",
                "A.8.25|A.8.26|A.8.27", "PR.DS-5", "preventive", 3, "EP_VUL_002"),

            // Security Operations (5 controls)
            CreateControl("COT-31", "Operations",
                "مركز عمليات الأمن السيبراني",
                "Security Operations Center",
                "يجب إنشاء مركز عمليات أمن سيبراني أو التعاقد مع مزود خدمة",
                "Establish security operations center or contract with service provider",
                "يجب توفير مراقبة 24/7 للأنظمة الحرجة",
                "Provide 24/7 monitoring for critical systems",
                "SOC_PROCEDURE|SLA_AGREEMENT",
                "A.8.15|A.8.16", "DE.CM-1|DE.CM-6|DE.CM-7", "detective", 3, "EP_OPS_004"),

            CreateControl("COT-32", "Operations",
                "تسجيل ومراقبة الأحداث الأمنية",
                "Security Event Logging and Monitoring",
                "يجب تفعيل تسجيل الأحداث الأمنية ومراقبتها مركزياً",
                "Enable security event logging and central monitoring",
                "يجب استخدام SIEM وحفظ السجلات لمدة سنة على الأقل",
                "Use SIEM and retain logs for at least one year",
                "LOGGING_POLICY|SIEM_CONFIG",
                "A.8.15|A.8.16", "DE.AE-3|DE.AE-5", "detective", 2, "EP_OPS_004"),

            CreateControl("COT-33", "Operations",
                "إدارة التغييرات",
                "Change Management",
                "يجب تطبيق عملية رسمية لإدارة التغييرات على الأنظمة",
                "Apply formal change management process for systems",
                "يجب توثيق والموافقة واختبار جميع التغييرات",
                "Document, approve, and test all changes",
                "CHANGE_PROCEDURE|CAB_MINUTES",
                "A.8.32", "PR.IP-3", "preventive", 2, "EP_OPS_001"),

            CreateControl("COT-34", "Operations",
                "النسخ الاحتياطي والاستعادة",
                "Backup and Recovery",
                "يجب تطبيق برنامج للنسخ الاحتياطي واختبار الاستعادة",
                "Implement backup program and test recovery",
                "يجب النسخ الاحتياطي يومياً واختبار الاستعادة ربع سنوياً",
                "Daily backups and quarterly recovery testing",
                "BACKUP_PROCEDURE|RESTORE_TEST_LOG",
                "A.8.13|A.8.14", "PR.IP-4", "preventive", 2, "EP_OPS_003"),

            CreateControl("COT-35", "Operations",
                "إدارة الأجهزة المحمولة",
                "Mobile Device Management",
                "يجب تطبيق ضوابط أمنية للأجهزة المحمولة",
                "Apply security controls for mobile devices",
                "يجب استخدام MDM وتطبيق: التشفير، المسح عن بعد، سياسات الأمان",
                "Use MDM and apply: encryption, remote wipe, security policies",
                "MDM_CONFIG|DEVICE_INVENTORY",
                "A.6.7|A.8.1", "PR.AC-3", "preventive", 2, "EP_OPS_002"),
        };
    }
    #endregion

    #region Domain 4: Third-Party Security (10 Controls)
    private static List<FrameworkControl> GetThirdPartySecurityControls()
    {
        return new List<FrameworkControl>
        {
            CreateControl("TPS-1", "Third-Party",
                "سياسة إدارة الأطراف الثالثة",
                "Third-Party Management Policy",
                "يجب وضع سياسة شاملة لإدارة مخاطر الأمن السيبراني للأطراف الثالثة",
                "Establish comprehensive policy for managing third-party cybersecurity risks",
                "يجب تحديد متطلبات الأمان لجميع أنواع العلاقات",
                "Define security requirements for all relationship types",
                "TPRM_POLICY|PROCEDURE_DOC",
                "A.5.19|A.5.20|A.5.21", "ID.SC-1|ID.SC-2", "preventive", 1, "EP_TPR_001"),

            CreateControl("TPS-2", "Third-Party",
                "جرد الأطراف الثالثة",
                "Third-Party Inventory",
                "يجب إنشاء وصيانة سجل لجميع الأطراف الثالثة ذات العلاقة بالأمن السيبراني",
                "Establish and maintain inventory of all third parties with cybersecurity relevance",
                "يجب تصنيف الموردين حسب مستوى المخاطر",
                "Classify vendors by risk level",
                "VENDOR_INVENTORY|RISK_CLASSIFICATION",
                "A.5.19", "ID.SC-2", "preventive", 1, "EP_TPR_001"),

            CreateControl("TPS-3", "Third-Party",
                "تقييم مخاطر الأطراف الثالثة",
                "Third-Party Risk Assessment",
                "يجب إجراء تقييم لمخاطر الأمن السيبراني قبل التعاقد وبشكل دوري",
                "Conduct cybersecurity risk assessment before contracting and periodically",
                "يجب استخدام استبيانات موحدة ومراجعة الشهادات",
                "Use standardized questionnaires and review certifications",
                "RISK_ASSESSMENT|QUESTIONNAIRE_RESPONSE",
                "A.5.19|A.5.21", "ID.SC-2", "detective", 2, "EP_TPR_001"),

            CreateControl("TPS-4", "Third-Party",
                "المتطلبات الأمنية في العقود",
                "Security Requirements in Contracts",
                "يجب تضمين متطلبات الأمن السيبراني في جميع العقود مع الأطراف الثالثة",
                "Include cybersecurity requirements in all contracts with third parties",
                "يجب تضمين: الالتزامات الأمنية، حق التدقيق، الإبلاغ عن الحوادث",
                "Include: security obligations, audit rights, incident reporting",
                "CONTRACT_TEMPLATE|SECURITY_ADDENDUM",
                "A.5.20", "ID.SC-3", "preventive", 2, "EP_TPR_001"),

            CreateControl("TPS-5", "Third-Party",
                "إدارة وصول الأطراف الثالثة",
                "Third-Party Access Management",
                "يجب التحكم في وصول الأطراف الثالثة لأنظمة وبيانات المؤسسة",
                "Control third-party access to institution systems and data",
                "يجب تطبيق أقل الصلاحيات ومراقبة الوصول وإلغاؤه عند انتهاء الحاجة",
                "Apply least privilege, monitor access, and revoke when no longer needed",
                "ACCESS_PROCEDURE|ACCESS_LOG",
                "A.5.19|A.5.20", "PR.AC-3", "preventive", 2, "EP_IAM_001"),

            CreateControl("TPS-6", "Third-Party",
                "مراقبة أداء الأطراف الثالثة",
                "Third-Party Performance Monitoring",
                "يجب مراقبة أداء الأطراف الثالثة وامتثالهم للمتطلبات الأمنية",
                "Monitor third-party performance and compliance with security requirements",
                "يجب مراجعة SLA والامتثال ربع سنوياً",
                "Review SLA and compliance quarterly",
                "PERFORMANCE_REPORT|SLA_REVIEW",
                "A.5.22", "ID.SC-4", "detective", 2, "EP_TPR_001"),

            CreateControl("TPS-7", "Third-Party",
                "تدقيق الأطراف الثالثة",
                "Third-Party Audits",
                "يجب إجراء تدقيق أمني للأطراف الثالثة ذات المخاطر العالية",
                "Conduct security audits for high-risk third parties",
                "يجب طلب SOC 2 أو إجراء تدقيق سنوي",
                "Request SOC 2 or conduct annual audit",
                "AUDIT_REPORT|SOC2_REPORT",
                "A.5.22", "ID.SC-4", "detective", 3, "EP_TPR_002"),

            CreateControl("TPS-8", "Third-Party",
                "الاستجابة للحوادث مع الأطراف الثالثة",
                "Incident Response with Third Parties",
                "يجب تنسيق الاستجابة للحوادث مع الأطراف الثالثة",
                "Coordinate incident response with third parties",
                "يجب تحديد إجراءات الإبلاغ والتصعيد والتعاون",
                "Define reporting, escalation, and cooperation procedures",
                "IR_PROCEDURE|ESCALATION_MATRIX",
                "A.5.24", "RS.CO-4", "corrective", 2, "EP_INC_002"),

            CreateControl("TPS-9", "Third-Party",
                "إنهاء العلاقة مع الأطراف الثالثة",
                "Third-Party Offboarding",
                "يجب تطبيق إجراءات آمنة لإنهاء العلاقة مع الأطراف الثالثة",
                "Apply secure procedures for third-party offboarding",
                "يجب إلغاء الوصول واسترداد الأصول وضمان حذف البيانات",
                "Revoke access, recover assets, and ensure data deletion",
                "OFFBOARDING_CHECKLIST|DATA_DELETION_CERT",
                "A.5.19", "ID.SC-5", "preventive", 2, "EP_TPR_001"),

            CreateControl("TPS-10", "Third-Party",
                "أمن الخدمات السحابية",
                "Cloud Services Security",
                "يجب تقييم وإدارة مخاطر الخدمات السحابية",
                "Assess and manage cloud services risks",
                "يجب مراجعة إعدادات الأمان والامتثال وموقع البيانات",
                "Review security settings, compliance, and data location",
                "CLOUD_ASSESSMENT|CSPM_REPORT",
                "A.5.23", "ID.SC-3", "preventive", 2, "EP_TPR_001"),
        };
    }
    #endregion

    #region Domain 5: Cybersecurity Resilience (18 Controls)
    private static List<FrameworkControl> GetResilienceControls()
    {
        return new List<FrameworkControl>
        {
            CreateControl("CSR-1", "Resilience",
                "سياسة إدارة حوادث الأمن السيبراني",
                "Cybersecurity Incident Management Policy",
                "يجب وضع سياسة شاملة لإدارة حوادث الأمن السيبراني",
                "Establish comprehensive cybersecurity incident management policy",
                "يجب تحديد تعريف الحادث ومستويات الخطورة والأدوار",
                "Define incident definition, severity levels, and roles",
                "IR_POLICY|CLASSIFICATION_MATRIX",
                "A.5.24|A.5.25|A.5.26", "RS.RP-1|RS.CO-1", "preventive", 1, "EP_INC_002"),

            CreateControl("CSR-2", "Resilience",
                "خطة الاستجابة للحوادث",
                "Incident Response Plan",
                "يجب وضع خطة مفصلة للاستجابة للحوادث السيبرانية",
                "Develop detailed cybersecurity incident response plan",
                "يجب تضمين إجراءات الكشف والاحتواء والاستئصال والتعافي",
                "Include detection, containment, eradication, and recovery procedures",
                "IR_PLAN|PLAYBOOKS",
                "A.5.26", "RS.RP-1", "preventive", 2, "EP_INC_002"),

            CreateControl("CSR-3", "Resilience",
                "فريق الاستجابة للحوادث",
                "Incident Response Team",
                "يجب تشكيل فريق للاستجابة لحوادث الأمن السيبراني",
                "Form cybersecurity incident response team",
                "يجب تحديد الأعضاء والأدوار والتوفر وقنوات الاتصال",
                "Define members, roles, availability, and communication channels",
                "TEAM_ROSTER|CONTACT_LIST",
                "A.5.25", "RS.CO-1", "preventive", 2, "EP_INC_002"),

            CreateControl("CSR-4", "Resilience",
                "كشف الحوادث والتنبيه",
                "Incident Detection and Alerting",
                "يجب تطبيق آليات للكشف عن الحوادث والتنبيه الفوري",
                "Implement mechanisms for incident detection and immediate alerting",
                "يجب دمج مصادر السجلات والتنبيهات في SIEM",
                "Integrate log sources and alerts in SIEM",
                "DETECTION_CONFIG|ALERT_RULES",
                "A.5.25|A.8.16", "DE.AE-1|DE.AE-4|DE.AE-5", "detective", 2, "EP_INC_001"),

            CreateControl("CSR-5", "Resilience",
                "تصنيف الحوادث وفرزها",
                "Incident Classification and Triage",
                "يجب تصنيف الحوادث وفرزها حسب الخطورة والأولوية",
                "Classify and triage incidents by severity and priority",
                "يجب تطبيق SLA للاستجابة لكل مستوى خطورة",
                "Apply response SLA for each severity level",
                "CLASSIFICATION_MATRIX|TRIAGE_PROCEDURE",
                "A.5.25", "RS.AN-1|RS.AN-2", "detective", 2, "EP_INC_001"),

            CreateControl("CSR-6", "Resilience",
                "التحقيق في الحوادث",
                "Incident Investigation",
                "يجب التحقيق في الحوادث لتحديد السبب الجذري والنطاق والأثر",
                "Investigate incidents to determine root cause, scope, and impact",
                "يجب الحفاظ على سلسلة الحيازة والأدلة",
                "Maintain chain of custody and evidence",
                "INVESTIGATION_PROCEDURE|FORENSIC_REPORT",
                "A.5.26|A.5.28", "RS.AN-3|RS.AN-4", "detective", 3, "EP_INC_001"),

            CreateControl("CSR-7", "Resilience",
                "احتواء الحوادث",
                "Incident Containment",
                "يجب احتواء الحوادث بسرعة لمنع انتشارها",
                "Contain incidents quickly to prevent spread",
                "يجب تطوير إجراءات احتواء لكل نوع حادث",
                "Develop containment procedures for each incident type",
                "CONTAINMENT_PLAYBOOK|ISOLATION_PROCEDURE",
                "A.5.26", "RS.MI-1|RS.MI-2", "corrective", 2, "EP_INC_001"),

            CreateControl("CSR-8", "Resilience",
                "استئصال الحوادث والتعافي",
                "Incident Eradication and Recovery",
                "يجب استئصال سبب الحادث واستعادة الأنظمة المتأثرة",
                "Eradicate incident cause and restore affected systems",
                "يجب التحقق من سلامة الأنظمة قبل إعادتها للإنتاج",
                "Verify system integrity before returning to production",
                "ERADICATION_CHECKLIST|RECOVERY_PROCEDURE",
                "A.5.26", "RC.RP-1|RC.IM-1", "corrective", 2, "EP_INC_001"),

            CreateControl("CSR-9", "Resilience",
                "الإبلاغ عن الحوادث للجهات الرقابية",
                "Incident Reporting to Regulators",
                "يجب الإبلاغ عن الحوادث الجسيمة للجهات الرقابية في الوقت المحدد",
                "Report major incidents to regulators within specified timeframe",
                "يجب الإبلاغ لساما خلال 24 ساعة من الحوادث الجسيمة",
                "Report to SAMA within 24 hours of major incidents",
                "NOTIFICATION_PROCEDURE|INCIDENT_REPORT",
                "A.5.24|A.5.27", "RS.CO-3|RS.CO-5", "corrective", 2, "EP_INC_001"),

            CreateControl("CSR-10", "Resilience",
                "الدروس المستفادة من الحوادث",
                "Incident Lessons Learned",
                "يجب توثيق الدروس المستفادة وتحسين الإجراءات بناءً عليها",
                "Document lessons learned and improve procedures based on them",
                "يجب عقد اجتماع مراجعة بعد كل حادث جسيم",
                "Hold review meeting after each major incident",
                "POST_INCIDENT_REPORT|IMPROVEMENT_PLAN",
                "A.5.27", "RS.IM-1|RS.IM-2", "corrective", 2, "EP_INC_001"),

            CreateControl("CSR-11", "Resilience",
                "تمارين الاستجابة للحوادث",
                "Incident Response Exercises",
                "يجب إجراء تمارين دورية للاستجابة للحوادث",
                "Conduct periodic incident response exercises",
                "يجب إجراء تمرين طاولة سنوياً ومحاكاة تقنية نصف سنوياً",
                "Conduct tabletop annually and technical simulation semi-annually",
                "EXERCISE_REPORT|TABLETOP_MINUTES",
                "A.5.24", "RS.RP-1", "detective", 2, "EP_INC_002"),

            CreateControl("CSR-12", "Resilience",
                "تحليل تأثير الأعمال",
                "Business Impact Analysis",
                "يجب إجراء تحليل لتأثير توقف العمليات الحرجة",
                "Conduct analysis of critical operations interruption impact",
                "يجب تحديد RTO وRPO لكل عملية حرجة",
                "Define RTO and RPO for each critical process",
                "BIA_REPORT|RTO_RPO_MATRIX",
                "A.5.29", "PR.IP-9", "preventive", 2, "EP_BCM_001"),

            CreateControl("CSR-13", "Resilience",
                "خطة استمرارية الأعمال",
                "Business Continuity Plan",
                "يجب وضع خطة لاستمرارية الأعمال للعمليات الحرجة",
                "Develop business continuity plan for critical operations",
                "يجب تضمين إجراءات التشغيل البديلة وخطط الاتصال",
                "Include alternative operating procedures and communication plans",
                "BCP_DOC|CONTACT_TREE",
                "A.5.29|A.5.30", "PR.IP-9", "preventive", 2, "EP_BCM_001"),

            CreateControl("CSR-14", "Resilience",
                "خطة التعافي من الكوارث",
                "Disaster Recovery Plan",
                "يجب وضع خطة للتعافي من الكوارث للأنظمة التقنية الحرجة",
                "Develop disaster recovery plan for critical technical systems",
                "يجب تحديد موقع التعافي البديل وإجراءات الانتقال",
                "Define alternate recovery site and transition procedures",
                "DR_PLAN|RUNBOOK",
                "A.5.29|A.5.30", "RC.RP-1", "preventive", 2, "EP_BCM_002"),

            CreateControl("CSR-15", "Resilience",
                "موقع التعافي البديل",
                "Alternate Recovery Site",
                "يجب توفير موقع بديل للتعافي في حالة الكوارث",
                "Provide alternate recovery site for disaster situations",
                "يجب أن يكون الموقع البديل في منطقة جغرافية مختلفة",
                "Alternate site must be in different geographic region",
                "DR_SITE_CONTRACT|SITE_ASSESSMENT",
                "A.5.30", "RC.RP-1", "preventive", 3, "EP_BCM_002"),

            CreateControl("CSR-16", "Resilience",
                "اختبار خطط الاستمرارية والتعافي",
                "Continuity and Recovery Plan Testing",
                "يجب اختبار خطط الاستمرارية والتعافي بشكل دوري",
                "Test continuity and recovery plans periodically",
                "يجب إجراء اختبار كامل سنوياً واختبارات جزئية ربع سنوياً",
                "Conduct full test annually and partial tests quarterly",
                "TEST_REPORT|FAILOVER_LOG",
                "A.5.29", "PR.IP-10", "detective", 3, "EP_BCM_002"),

            CreateControl("CSR-17", "Resilience",
                "الاتصالات في الأزمات",
                "Crisis Communications",
                "يجب وضع خطة للاتصالات أثناء الأزمات والحوادث الجسيمة",
                "Develop crisis communication plan for major incidents",
                "يجب تحديد المتحدث الرسمي وقوالب الرسائل وقنوات الاتصال",
                "Define spokesperson, message templates, and communication channels",
                "COMMUNICATION_PLAN|MESSAGE_TEMPLATES",
                "A.5.30", "RS.CO-2|RS.CO-3", "preventive", 2, "EP_INC_002"),

            CreateControl("CSR-18", "Resilience",
                "صيانة خطط الاستمرارية",
                "Continuity Plan Maintenance",
                "يجب مراجعة وتحديث خطط الاستمرارية والتعافي بشكل دوري",
                "Review and update continuity and recovery plans periodically",
                "يجب المراجعة سنوياً وعند التغييرات الجوهرية",
                "Review annually and upon significant changes",
                "REVIEW_LOG|UPDATE_HISTORY",
                "A.5.29", "PR.IP-9", "preventive", 2, "EP_BCM_001"),
        };
    }
    #endregion

    #region Helper Methods
    private static FrameworkControl CreateControl(
        string controlNumber,
        string domain,
        string titleAr,
        string titleEn,
        string requirementAr,
        string requirementEn,
        string implementationGuidanceAr,
        string implementationGuidanceEn,
        string evidenceRequirements,
        string isoMapping,
        string nistMapping,
        string controlType,
        int maturityLevel,
        string evidencePackCode)
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
            ImplementationGuidanceAr = implementationGuidanceAr,
            ImplementationGuidanceEn = implementationGuidanceEn,
            ControlType = controlType,
            MaturityLevel = maturityLevel,
            EvidenceRequirements = evidenceRequirements,
            MappingIso27001 = isoMapping,
            MappingNist = nistMapping,
            Status = "Active",
            DefaultWeight = maturityLevel,
            MinEvidenceScore = 70,
            EvidencePackCode = evidencePackCode,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "system"
        };
    }
    #endregion
}
