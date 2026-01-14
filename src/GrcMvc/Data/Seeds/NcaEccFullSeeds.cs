using GrcMvc.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Data.Seeds;

/// <summary>
/// Complete NCA Essential Cybersecurity Controls (ECC-1:2018) - 114 Controls
/// Reference: https://nca.gov.sa/pages/ecc.html
///
/// Domains:
/// 1. Cybersecurity Governance (1-1 to 1-14) - 14 controls
/// 2. Cybersecurity Defense (2-1 to 2-55) - 55 controls
/// 3. Cybersecurity Resilience (3-1 to 3-20) - 20 controls
/// 4. Third-Party Cybersecurity (4-1 to 4-15) - 15 controls
/// 5. ICS/OT Security (5-1 to 5-10) - 10 controls
/// Total: 114 controls
/// </summary>
public static class NcaEccFullSeeds
{
    public static async Task SeedAsync(GrcDbContext context, ILogger logger)
    {
        try
        {
            var existingCount = await context.FrameworkControls
                .CountAsync(c => c.FrameworkCode == "NCA-ECC");

            if (existingCount >= 100)
            {
                logger.LogInformation("✅ NCA-ECC controls already exist ({Count}). Skipping seed.", existingCount);
                return;
            }

            // Clear existing if partial
            if (existingCount > 0)
            {
                var existing = await context.FrameworkControls
                    .Where(c => c.FrameworkCode == "NCA-ECC")
                    .ToListAsync();
                context.FrameworkControls.RemoveRange(existing);
                await context.SaveChangesAsync();
            }

            var controls = GetAllNcaEccControls();
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

    private static List<FrameworkControl> GetAllNcaEccControls()
    {
        var controls = new List<FrameworkControl>();

        // ============================================================
        // DOMAIN 1: CYBERSECURITY GOVERNANCE (1-1 to 1-14) - 14 Controls
        // ============================================================
        controls.AddRange(GetGovernanceControls());

        // ============================================================
        // DOMAIN 2: CYBERSECURITY DEFENSE (2-1 to 2-55) - 55 Controls
        // ============================================================
        controls.AddRange(GetDefenseControls());

        // ============================================================
        // DOMAIN 3: CYBERSECURITY RESILIENCE (3-1 to 3-20) - 20 Controls
        // ============================================================
        controls.AddRange(GetResilienceControls());

        // ============================================================
        // DOMAIN 4: THIRD-PARTY CYBERSECURITY (4-1 to 4-15) - 15 Controls
        // ============================================================
        controls.AddRange(GetThirdPartyControls());

        // ============================================================
        // DOMAIN 5: ICS/OT SECURITY (5-1 to 5-10) - 10 Controls
        // ============================================================
        controls.AddRange(GetIcsControls());

        return controls;
    }

    #region Domain 1: Cybersecurity Governance (14 Controls)
    private static List<FrameworkControl> GetGovernanceControls()
    {
        return new List<FrameworkControl>
        {
            // 1-1: Cybersecurity Policy
            CreateControl("1-1", "Governance",
                "سياسة الأمن السيبراني",
                "Cybersecurity Policy",
                "يجب إعداد سياسة الأمن السيبراني واعتمادها من الإدارة العليا وتوزيعها على جميع العاملين والمراجعة دورياً",
                "Establish cybersecurity policy approved by senior management, distribute to all employees, and review periodically",
                "يجب أن تتضمن السياسة: أهداف الأمن السيبراني، الأدوار والمسؤوليات، متطلبات الامتثال، عملية المراجعة الدورية",
                "Policy must include: cybersecurity objectives, roles and responsibilities, compliance requirements, periodic review process",
                "POLICY_DOC|MEETING_MINUTES|ACKNOWLEDGMENT", "A.5.1", "ID.GV-1", "preventive", 1,
                "EP_GOV_001"),

            // 1-2: Cybersecurity Strategy
            CreateControl("1-2", "Governance",
                "استراتيجية الأمن السيبراني",
                "Cybersecurity Strategy",
                "يجب وضع استراتيجية للأمن السيبراني متوافقة مع استراتيجية المنظمة وأهداف العمل",
                "Develop cybersecurity strategy aligned with organization strategy and business objectives",
                "يجب مراجعة الاستراتيجية سنوياً وتحديثها عند تغير بيئة التهديدات أو أهداف العمل",
                "Review strategy annually and update when threat landscape or business objectives change",
                "POLICY_DOC|PROCEDURE_DOC", "A.5.1", "ID.GV-1", "preventive", 1,
                "EP_GOV_001"),

            // 1-3: Roles and Responsibilities
            CreateControl("1-3", "Governance",
                "أدوار ومسؤوليات الأمن السيبراني",
                "Cybersecurity Roles and Responsibilities",
                "يجب تحديد وتوثيق أدوار ومسؤوليات الأمن السيبراني لجميع المستويات التنظيمية",
                "Define and document cybersecurity roles and responsibilities for all organizational levels",
                "يجب تعيين مسؤول للأمن السيبراني وتحديد صلاحياته وخطوط الإبلاغ",
                "Appoint cybersecurity officer and define their authority and reporting lines",
                "POLICY_DOC|ORG_CHART", "A.6.1.1", "ID.GV-2", "preventive", 1,
                "EP_GOV_001"),

            // 1-4: Risk Management Framework
            CreateControl("1-4", "Governance",
                "إطار إدارة مخاطر الأمن السيبراني",
                "Cybersecurity Risk Management Framework",
                "يجب إنشاء إطار لإدارة مخاطر الأمن السيبراني يتضمن تحديد وتقييم ومعالجة المخاطر",
                "Establish cybersecurity risk management framework including risk identification, assessment, and treatment",
                "يجب تحديث تقييم المخاطر بشكل دوري وعند حدوث تغييرات جوهرية",
                "Update risk assessment periodically and when significant changes occur",
                "PROCEDURE_DOC|RISK_REGISTER", "A.6.1.2", "ID.RM-1", "preventive", 2,
                "EP_GOV_002"),

            // 1-5: Compliance Management
            CreateControl("1-5", "Governance",
                "إدارة الامتثال للأمن السيبراني",
                "Cybersecurity Compliance Management",
                "يجب ضمان الامتثال للمتطلبات التنظيمية والقانونية والتعاقدية المتعلقة بالأمن السيبراني",
                "Ensure compliance with regulatory, legal, and contractual cybersecurity requirements",
                "يجب الاحتفاظ بسجل للمتطلبات التنظيمية وتتبع حالة الامتثال",
                "Maintain register of regulatory requirements and track compliance status",
                "COMPLIANCE_REPORT|AUDIT_LOG", "A.18.1", "ID.GV-3", "detective", 2,
                "EP_CMP_001"),

            // 1-6: Cybersecurity Audit
            CreateControl("1-6", "Governance",
                "تدقيق الأمن السيبراني",
                "Cybersecurity Audit",
                "يجب إجراء تدقيق دوري مستقل للأمن السيبراني لتقييم فعالية الضوابط",
                "Conduct periodic independent cybersecurity audits to assess control effectiveness",
                "يجب إجراء التدقيق سنوياً على الأقل ومعالجة الملاحظات في إطار زمني محدد",
                "Conduct audit at least annually and address findings within defined timeframe",
                "AUDIT_REPORT|REMEDIATION_PLAN", "A.18.2", "ID.GV-3", "detective", 3,
                "EP_CMP_002"),

            // 1-7: HR Security - Before Employment
            CreateControl("1-7", "Governance",
                "أمن الموارد البشرية - قبل التوظيف",
                "Human Resource Security - Before Employment",
                "يجب إجراء فحص أمني للمرشحين قبل التوظيف خاصة للوظائف الحساسة",
                "Conduct security screening for candidates before employment, especially for sensitive positions",
                "يجب التحقق من السجل الجنائي والمؤهلات والمراجع للوظائف ذات الصلاحيات الحساسة",
                "Verify criminal record, qualifications, and references for positions with sensitive privileges",
                "HR_RECORD|BACKGROUND_CHECK", "A.7.1", "PR.AT-1", "preventive", 1,
                "EP_HRS_001"),

            // 1-8: HR Security - During Employment
            CreateControl("1-8", "Governance",
                "أمن الموارد البشرية - أثناء التوظيف",
                "Human Resource Security - During Employment",
                "يجب تطبيق ضوابط أمن الموارد البشرية أثناء فترة العمل",
                "Apply human resource security controls during employment period",
                "يجب توقيع اتفاقيات السرية ومراجعة الصلاحيات دورياً",
                "Sign confidentiality agreements and review privileges periodically",
                "HR_RECORD|NDA|ACCESS_REVIEW", "A.7.2", "PR.AT-2", "preventive", 1,
                "EP_HRS_001"),

            // 1-9: HR Security - Termination
            CreateControl("1-9", "Governance",
                "أمن الموارد البشرية - إنهاء العمل",
                "Human Resource Security - Termination",
                "يجب تطبيق إجراءات إنهاء العمل لحماية أصول المنظمة",
                "Apply termination procedures to protect organization assets",
                "يجب إلغاء جميع الصلاحيات واسترداد الأجهزة والمعلومات فور إنهاء العمل",
                "Revoke all privileges and recover devices and information immediately upon termination",
                "HR_RECORD|TERMINATION_CHECKLIST", "A.7.3", "PR.AT-3", "preventive", 1,
                "EP_HRS_001"),

            // 1-10: Security Awareness
            CreateControl("1-10", "Governance",
                "التوعية بالأمن السيبراني",
                "Cybersecurity Awareness",
                "يجب تنفيذ برنامج توعية للأمن السيبراني لجميع العاملين",
                "Implement cybersecurity awareness program for all employees",
                "يجب تقديم التوعية عند الالتحاق وبشكل دوري سنوياً على الأقل",
                "Provide awareness upon joining and periodically at least annually",
                "TRAINING_RECORD|ATTENDANCE_LOG", "A.7.2.2", "PR.AT-1", "preventive", 1,
                "EP_HRS_002"),

            // 1-11: Security Training
            CreateControl("1-11", "Governance",
                "التدريب على الأمن السيبراني",
                "Cybersecurity Training",
                "يجب توفير تدريب متخصص للأمن السيبراني للموظفين حسب أدوارهم",
                "Provide specialized cybersecurity training for employees based on their roles",
                "يجب تقييم فعالية التدريب وتحديث المحتوى بناءً على التهديدات الجديدة",
                "Assess training effectiveness and update content based on new threats",
                "TRAINING_RECORD|CERTIFICATE|QUIZ_RESULT", "A.7.2.2", "PR.AT-1", "preventive", 2,
                "EP_HRS_002"),

            // 1-12: Cybersecurity in Projects
            CreateControl("1-12", "Governance",
                "الأمن السيبراني في إدارة المشاريع",
                "Cybersecurity in Project Management",
                "يجب دمج متطلبات الأمن السيبراني في جميع مراحل إدارة المشاريع",
                "Integrate cybersecurity requirements in all project management phases",
                "يجب إجراء تقييم أمني قبل بدء المشروع وقبل الإطلاق",
                "Conduct security assessment before project start and before launch",
                "PROJECT_DOC|SECURITY_ASSESSMENT", "A.6.1.5", "ID.GV-4", "preventive", 2,
                "EP_GOV_002"),

            // 1-13: Performance Measurement
            CreateControl("1-13", "Governance",
                "قياس أداء الأمن السيبراني",
                "Cybersecurity Performance Measurement",
                "يجب إنشاء مؤشرات قياس أداء الأمن السيبراني ومراقبتها بشكل دوري",
                "Establish cybersecurity performance indicators and monitor them periodically",
                "يجب رفع تقارير الأداء للإدارة العليا بشكل ربع سنوي على الأقل",
                "Report performance to senior management at least quarterly",
                "DASHBOARD_REPORT|KPI_REPORT", "A.18.2.1", "ID.GV-4", "detective", 2,
                "EP_CMP_001"),

            // 1-14: Regulatory Reporting
            CreateControl("1-14", "Governance",
                "التقارير التنظيمية للأمن السيبراني",
                "Cybersecurity Regulatory Reporting",
                "يجب تقديم التقارير التنظيمية المطلوبة للجهات المختصة في الأوقات المحددة",
                "Submit required regulatory reports to competent authorities within specified timeframes",
                "يجب الإبلاغ عن الحوادث الجسيمة للهيئة الوطنية للأمن السيبراني خلال 24 ساعة",
                "Report major incidents to NCA within 24 hours",
                "COMPLIANCE_REPORT|INCIDENT_REPORT", "A.18.1.3", "ID.GV-3", "detective", 2,
                "EP_CMP_001"),
        };
    }
    #endregion

    #region Domain 2: Cybersecurity Defense (55 Controls)
    private static List<FrameworkControl> GetDefenseControls()
    {
        return new List<FrameworkControl>
        {
            // 2-1: Asset Inventory
            CreateControl("2-1", "Defense",
                "جرد الأصول المعلوماتية",
                "Information Asset Inventory",
                "يجب إنشاء وصيانة سجل شامل للأصول المعلوماتية والتقنية",
                "Establish and maintain comprehensive inventory of information and technology assets",
                "يجب تحديث السجل بشكل دوري وعند إضافة أو إزالة أصول",
                "Update inventory periodically and when assets are added or removed",
                "ASSET_INVENTORY|CONFIG_EXPORT", "A.8.1", "ID.AM-1", "preventive", 1,
                "EP_ASM_001"),

            // 2-2: Asset Classification
            CreateControl("2-2", "Defense",
                "تصنيف الأصول",
                "Asset Classification",
                "يجب تصنيف الأصول المعلوماتية حسب مستوى حساسيتها وأهميتها للأعمال",
                "Classify information assets according to sensitivity level and business importance",
                "يجب استخدام مستويات التصنيف: سري للغاية، سري، محدود، عام",
                "Use classification levels: Top Secret, Secret, Restricted, Public",
                "CLASSIFICATION_POLICY|DATA_INVENTORY", "A.8.2", "ID.AM-5", "preventive", 1,
                "EP_ASM_002"),

            // 2-3: Asset Handling
            CreateControl("2-3", "Defense",
                "التعامل مع الأصول المصنفة",
                "Classified Asset Handling",
                "يجب وضع إجراءات للتعامل مع الأصول حسب تصنيفها",
                "Establish procedures for handling assets according to their classification",
                "يجب تحديد إجراءات النقل والتخزين والإتلاف لكل مستوى تصنيف",
                "Define transfer, storage, and disposal procedures for each classification level",
                "PROCEDURE_DOC|HANDLING_GUIDELINES", "A.8.2.3", "PR.DS-5", "preventive", 2,
                "EP_ASM_002"),

            // 2-4: Identity Management
            CreateControl("2-4", "Defense",
                "إدارة الهوية",
                "Identity Management",
                "يجب تطبيق نظام مركزي لإدارة هويات المستخدمين",
                "Implement centralized system for managing user identities",
                "يجب ربط جميع الحسابات بهوية موحدة وتطبيق سياسة دورة حياة الحساب",
                "Link all accounts to unified identity and apply account lifecycle policy",
                "CONFIG_EXPORT|USER_DIRECTORY", "A.9.1", "PR.AC-1", "preventive", 2,
                "EP_IAM_001"),

            // 2-5: Access Control Policy
            CreateControl("2-5", "Defense",
                "سياسة التحكم في الوصول",
                "Access Control Policy",
                "يجب وضع سياسة للتحكم في الوصول تحدد القواعد والإجراءات",
                "Establish access control policy defining rules and procedures",
                "يجب تطبيق مبدأ الحد الأدنى من الصلاحيات والفصل بين المهام",
                "Apply principle of least privilege and separation of duties",
                "POLICY_DOC|ACCESS_MATRIX", "A.9.1.1", "PR.AC-4", "preventive", 1,
                "EP_IAM_001"),

            // 2-6: User Access Management
            CreateControl("2-6", "Defense",
                "إدارة صلاحيات المستخدمين",
                "User Access Management",
                "يجب تطبيق إجراءات منح وإلغاء صلاحيات الوصول",
                "Implement procedures for granting and revoking access privileges",
                "يجب الحصول على موافقة صاحب النظام قبل منح الصلاحيات",
                "Obtain system owner approval before granting privileges",
                "ACCESS_REQUEST|APPROVAL_LOG", "A.9.2", "PR.AC-1", "preventive", 1,
                "EP_IAM_001"),

            // 2-7: Privileged Access Management
            CreateControl("2-7", "Defense",
                "إدارة الصلاحيات الممتازة",
                "Privileged Access Management",
                "يجب تطبيق ضوابط مشددة للصلاحيات الإدارية والممتازة",
                "Implement stringent controls for administrative and privileged access",
                "يجب استخدام حل PAM وتسجيل جميع الجلسات الممتازة",
                "Use PAM solution and record all privileged sessions",
                "PAM_CONFIG|SESSION_LOG", "A.9.2.3", "PR.AC-4", "preventive", 3,
                "EP_IAM_003"),

            // 2-8: Access Review
            CreateControl("2-8", "Defense",
                "مراجعة الصلاحيات",
                "Access Review",
                "يجب مراجعة صلاحيات الوصول بشكل دوري",
                "Review access privileges periodically",
                "يجب إجراء المراجعة ربع سنوياً للصلاحيات الممتازة وسنوياً للصلاحيات العادية",
                "Conduct review quarterly for privileged access and annually for regular access",
                "ACCESS_REVIEW_REPORT|EXCEPTION_LOG", "A.9.2.5", "PR.AC-1", "detective", 2,
                "EP_IAM_001"),

            // 2-9: Authentication Controls
            CreateControl("2-9", "Defense",
                "ضوابط المصادقة",
                "Authentication Controls",
                "يجب تطبيق آليات مصادقة قوية للتحقق من هوية المستخدمين",
                "Implement strong authentication mechanisms to verify user identity",
                "يجب استخدام المصادقة متعددة العوامل للأنظمة الحساسة",
                "Use multi-factor authentication for sensitive systems",
                "MFA_CONFIG|AUTH_POLICY", "A.9.4.2", "PR.AC-7", "preventive", 2,
                "EP_IAM_002"),

            // 2-10: Password Management
            CreateControl("2-10", "Defense",
                "إدارة كلمات المرور",
                "Password Management",
                "يجب تطبيق سياسة كلمات مرور قوية ومتوافقة مع أفضل الممارسات",
                "Implement strong password policy aligned with best practices",
                "يجب أن تتضمن: الحد الأدنى للطول 12 حرف، التعقيد، عدم التكرار، انتهاء الصلاحية",
                "Must include: minimum 12 characters, complexity, no reuse, expiration",
                "PASSWORD_POLICY|CONFIG_EXPORT", "A.9.4.3", "PR.AC-1", "preventive", 1,
                "EP_IAM_002"),

            // 2-11: Session Management
            CreateControl("2-11", "Defense",
                "إدارة الجلسات",
                "Session Management",
                "يجب تطبيق ضوابط إدارة الجلسات لمنع الاستخدام غير المصرح",
                "Implement session management controls to prevent unauthorized use",
                "يجب تطبيق انتهاء الجلسة التلقائي وتسجيل الخروج عند عدم النشاط",
                "Apply automatic session timeout and logout on inactivity",
                "CONFIG_EXPORT|SESSION_POLICY", "A.9.4.2", "PR.AC-7", "preventive", 2,
                "EP_IAM_002"),

            // 2-12: Network Security Architecture
            CreateControl("2-12", "Defense",
                "هندسة أمن الشبكات",
                "Network Security Architecture",
                "يجب تصميم الشبكات بناءً على مبادئ الأمن السيبراني",
                "Design networks based on cybersecurity principles",
                "يجب تطبيق العمق الدفاعي وعزل الشبكات حسب مستوى الحساسية",
                "Apply defense in depth and segment networks by sensitivity level",
                "NETWORK_DIAGRAM|ARCHITECTURE_DOC", "A.13.1", "PR.AC-5", "preventive", 2,
                "EP_NET_001"),

            // 2-13: Network Segmentation
            CreateControl("2-13", "Defense",
                "تجزئة الشبكات",
                "Network Segmentation",
                "يجب تجزئة الشبكات لعزل الأنظمة الحساسة",
                "Segment networks to isolate sensitive systems",
                "يجب فصل شبكة الخوادم عن شبكة المستخدمين وشبكة الضيوف",
                "Separate server network from user network and guest network",
                "VLAN_CONFIG|FIREWALL_RULES", "A.13.1.3", "PR.AC-5", "preventive", 2,
                "EP_NET_001"),

            // 2-14: Firewall Management
            CreateControl("2-14", "Defense",
                "إدارة جدران الحماية",
                "Firewall Management",
                "يجب نشر وإدارة جدران الحماية لحماية محيط الشبكة",
                "Deploy and manage firewalls to protect network perimeter",
                "يجب مراجعة قواعد الجدار الناري ربع سنوياً وإزالة القواعد غير الضرورية",
                "Review firewall rules quarterly and remove unnecessary rules",
                "FIREWALL_RULES|CHANGE_LOG", "A.13.1.1", "PR.AC-5", "preventive", 2,
                "EP_NET_001"),

            // 2-15: Intrusion Detection/Prevention
            CreateControl("2-15", "Defense",
                "كشف ومنع الاختراق",
                "Intrusion Detection/Prevention",
                "يجب نشر أنظمة كشف ومنع الاختراق لمراقبة حركة الشبكة",
                "Deploy intrusion detection/prevention systems to monitor network traffic",
                "يجب تحديث توقيعات الكشف بشكل يومي ومراجعة التنبيهات",
                "Update detection signatures daily and review alerts",
                "IDS_CONFIG|ALERT_LOG", "A.13.1.2", "DE.CM-1", "detective", 2,
                "EP_NET_001"),

            // 2-16: Secure Remote Access
            CreateControl("2-16", "Defense",
                "الوصول الآمن عن بعد",
                "Secure Remote Access",
                "يجب تطبيق ضوابط الوصول الآمن عن بعد",
                "Implement secure remote access controls",
                "يجب استخدام VPN مع تشفير قوي والمصادقة متعددة العوامل",
                "Use VPN with strong encryption and multi-factor authentication",
                "VPN_CONFIG|MFA_CONFIG", "A.6.2.2", "PR.AC-3", "preventive", 2,
                "EP_NET_001"),

            // 2-17: Wireless Security
            CreateControl("2-17", "Defense",
                "أمن الشبكات اللاسلكية",
                "Wireless Network Security",
                "يجب تطبيق ضوابط أمنية للشبكات اللاسلكية",
                "Implement security controls for wireless networks",
                "يجب استخدام WPA3 أو WPA2-Enterprise وعزل شبكة الضيوف",
                "Use WPA3 or WPA2-Enterprise and isolate guest network",
                "WIRELESS_CONFIG|SCAN_REPORT", "A.13.1", "PR.AC-5", "preventive", 2,
                "EP_NET_001"),

            // 2-18: DNS Security
            CreateControl("2-18", "Defense",
                "أمن نظام أسماء النطاقات",
                "DNS Security",
                "يجب تطبيق ضوابط أمنية لنظام DNS",
                "Implement security controls for DNS system",
                "يجب استخدام DNSSEC وتصفية DNS للحماية من المواقع الضارة",
                "Use DNSSEC and DNS filtering to protect from malicious sites",
                "DNS_CONFIG|DNSSEC_CONFIG", "A.13.1", "PR.DS-2", "preventive", 2,
                "EP_NET_001"),

            // 2-19: Email Security
            CreateControl("2-19", "Defense",
                "أمن البريد الإلكتروني",
                "Email Security",
                "يجب تطبيق ضوابط أمنية للبريد الإلكتروني",
                "Implement email security controls",
                "يجب تفعيل SPF وDKIM وDMARC ومرشحات البريد المزعج والتصيد",
                "Enable SPF, DKIM, DMARC, spam and phishing filters",
                "EMAIL_CONFIG|SPF_DKIM_DMARC", "A.13.2.3", "PR.DS-5", "preventive", 2,
                "EP_NET_002"),

            // 2-20: Web Filtering
            CreateControl("2-20", "Defense",
                "تصفية المحتوى الإلكتروني",
                "Web Content Filtering",
                "يجب تطبيق تصفية المحتوى الإلكتروني لمنع الوصول للمواقع الضارة",
                "Implement web content filtering to prevent access to malicious sites",
                "يجب حظر الفئات المحظورة ومراجعة السياسة بشكل دوري",
                "Block prohibited categories and review policy periodically",
                "WEB_FILTER_CONFIG|BLOCKED_CATEGORIES", "A.13.1", "PR.DS-5", "preventive", 1,
                "EP_NET_002"),

            // 2-21: Endpoint Protection
            CreateControl("2-21", "Defense",
                "حماية نقاط النهاية",
                "Endpoint Protection",
                "يجب نشر حلول حماية نقاط النهاية على جميع الأجهزة",
                "Deploy endpoint protection solutions on all devices",
                "يجب تفعيل الحماية في الوقت الفعلي وتحديث التوقيعات تلقائياً",
                "Enable real-time protection and automatic signature updates",
                "EDR_CONFIG|DEPLOYMENT_STATUS", "A.12.2", "DE.CM-4", "preventive", 1,
                "EP_OPS_002"),

            // 2-22: Malware Protection
            CreateControl("2-22", "Defense",
                "الحماية من البرمجيات الضارة",
                "Malware Protection",
                "يجب تطبيق حلول شاملة للحماية من البرمجيات الضارة",
                "Implement comprehensive malware protection solutions",
                "يجب الفحص في الوقت الفعلي وعند الطلب وفحص المرفقات",
                "Scan in real-time, on-demand, and scan attachments",
                "AV_CONFIG|SCAN_LOG", "A.12.2.1", "DE.CM-4", "preventive", 1,
                "EP_OPS_002"),

            // 2-23: Mobile Device Security
            CreateControl("2-23", "Defense",
                "أمن الأجهزة المحمولة",
                "Mobile Device Security",
                "يجب تطبيق ضوابط أمنية للأجهزة المحمولة",
                "Implement security controls for mobile devices",
                "يجب استخدام MDM وتطبيق سياسات الأمان والتشفير",
                "Use MDM and apply security policies and encryption",
                "MDM_CONFIG|DEVICE_INVENTORY", "A.6.2.1", "PR.AC-3", "preventive", 2,
                "EP_OPS_002"),

            // 2-24: Removable Media Control
            CreateControl("2-24", "Defense",
                "التحكم في الوسائط المحمولة",
                "Removable Media Control",
                "يجب تطبيق ضوابط للتحكم في استخدام الوسائط القابلة للإزالة",
                "Implement controls for removable media usage",
                "يجب منع الوسائط غير المصرح بها وفحص المسموح منها تلقائياً",
                "Block unauthorized media and auto-scan authorized ones",
                "DLP_CONFIG|USB_POLICY", "A.8.3.1", "PR.DS-5", "preventive", 2,
                "EP_OPS_002"),

            // 2-25: Server Hardening
            CreateControl("2-25", "Defense",
                "تقوية الخوادم",
                "Server Hardening",
                "يجب تطبيق معايير التقوية على جميع الخوادم",
                "Apply hardening standards to all servers",
                "يجب إزالة الخدمات غير الضرورية وتطبيق أقل الصلاحيات",
                "Remove unnecessary services and apply least privilege",
                "HARDENING_CHECKLIST|CONFIG_BASELINE", "A.12.1.4", "PR.IP-1", "preventive", 2,
                "EP_OPS_001"),

            // 2-26: Workstation Hardening
            CreateControl("2-26", "Defense",
                "تقوية محطات العمل",
                "Workstation Hardening",
                "يجب تطبيق معايير التقوية على محطات العمل",
                "Apply hardening standards to workstations",
                "يجب تعطيل الحسابات المحلية الإدارية وتشفير الأقراص",
                "Disable local admin accounts and encrypt disks",
                "HARDENING_CHECKLIST|GPO_CONFIG", "A.12.1.4", "PR.IP-1", "preventive", 2,
                "EP_OPS_001"),

            // 2-27: Vulnerability Management
            CreateControl("2-27", "Defense",
                "إدارة الثغرات الأمنية",
                "Vulnerability Management",
                "يجب إنشاء برنامج لإدارة الثغرات الأمنية",
                "Establish vulnerability management program",
                "يجب إجراء فحص شهري ومعالجة الثغرات الحرجة خلال 15 يوماً",
                "Conduct monthly scanning and remediate critical vulnerabilities within 15 days",
                "SCAN_REPORT|REMEDIATION_TRACKER", "A.12.6", "ID.RA-1", "detective", 2,
                "EP_VUL_001"),

            // 2-28: Penetration Testing
            CreateControl("2-28", "Defense",
                "اختبار الاختراق",
                "Penetration Testing",
                "يجب إجراء اختبار اختراق دوري للأنظمة الحساسة",
                "Conduct periodic penetration testing for sensitive systems",
                "يجب إجراء الاختبار سنوياً وعند التغييرات الجوهرية",
                "Conduct testing annually and upon significant changes",
                "PENTEST_REPORT|REMEDIATION_PLAN", "A.12.6.1", "ID.RA-1", "detective", 3,
                "EP_VUL_001"),

            // 2-29: Patch Management
            CreateControl("2-29", "Defense",
                "إدارة التحديثات والترقيعات",
                "Patch Management",
                "يجب تطبيق برنامج لإدارة التحديثات والترقيعات",
                "Implement patch management program",
                "يجب تطبيق الترقيعات الأمنية الحرجة خلال 14 يوماً",
                "Apply critical security patches within 14 days",
                "PATCH_STATUS|WSUS_REPORT", "A.12.6.1", "PR.IP-12", "preventive", 2,
                "EP_OPS_002"),

            // 2-30: Change Management
            CreateControl("2-30", "Defense",
                "إدارة التغييرات",
                "Change Management",
                "يجب تطبيق إجراءات إدارة التغييرات لجميع الأنظمة",
                "Implement change management procedures for all systems",
                "يجب توثيق جميع التغييرات والحصول على الموافقات المطلوبة",
                "Document all changes and obtain required approvals",
                "CHANGE_REQUEST|CAB_MINUTES", "A.12.1.2", "PR.IP-3", "preventive", 2,
                "EP_OPS_001"),

            // 2-31: Configuration Management
            CreateControl("2-31", "Defense",
                "إدارة الإعدادات",
                "Configuration Management",
                "يجب إنشاء وصيانة خطوط أساس للإعدادات الآمنة",
                "Establish and maintain secure configuration baselines",
                "يجب مراقبة الانحرافات عن الخط الأساسي ومعالجتها",
                "Monitor deviations from baseline and remediate",
                "CONFIG_BASELINE|DRIFT_REPORT", "A.12.1.4", "PR.IP-1", "preventive", 2,
                "EP_OPS_001"),

            // 2-32: Secure Development
            CreateControl("2-32", "Defense",
                "التطوير الآمن",
                "Secure Development",
                "يجب تطبيق ممارسات التطوير الآمن في دورة حياة البرمجيات",
                "Apply secure development practices in software development lifecycle",
                "يجب تضمين الأمان في جميع مراحل التطوير من التصميم للنشر",
                "Include security in all development phases from design to deployment",
                "SDLC_DOC|SECURITY_REQUIREMENTS", "A.14.2", "PR.DS-5", "preventive", 2,
                "EP_VUL_002"),

            // 2-33: Code Review
            CreateControl("2-33", "Defense",
                "مراجعة الكود البرمجي",
                "Code Review",
                "يجب إجراء مراجعة أمنية للكود قبل النشر",
                "Conduct security code review before deployment",
                "يجب استخدام أدوات التحليل الثابت والمراجعة اليدوية",
                "Use static analysis tools and manual review",
                "SAST_REPORT|CODE_REVIEW_LOG", "A.14.2.1", "PR.DS-5", "detective", 3,
                "EP_VUL_002"),

            // 2-34: Application Testing
            CreateControl("2-34", "Defense",
                "اختبار أمن التطبيقات",
                "Application Security Testing",
                "يجب إجراء اختبار أمني للتطبيقات قبل وأثناء الإنتاج",
                "Conduct application security testing before and during production",
                "يجب إجراء DAST وSAST واختبار API",
                "Conduct DAST, SAST, and API testing",
                "DAST_REPORT|API_TEST_REPORT", "A.14.2.8", "DE.CM-8", "detective", 3,
                "EP_VUL_002"),

            // 2-35: Cryptography Policy
            CreateControl("2-35", "Defense",
                "سياسة التشفير",
                "Cryptography Policy",
                "يجب وضع سياسة لاستخدام التشفير وإدارة المفاتيح",
                "Establish policy for cryptography use and key management",
                "يجب تحديد خوارزميات التشفير المعتمدة وأطوال المفاتيح",
                "Define approved encryption algorithms and key lengths",
                "CRYPTO_POLICY|KEY_MGMT_DOC", "A.10.1", "PR.DS-1", "preventive", 2,
                "EP_NET_002"),

            // 2-36: Data Encryption at Rest
            CreateControl("2-36", "Defense",
                "تشفير البيانات في حالة السكون",
                "Data Encryption at Rest",
                "يجب تشفير البيانات الحساسة في حالة السكون",
                "Encrypt sensitive data at rest",
                "يجب استخدام AES-256 أو أقوى لتشفير البيانات المخزنة",
                "Use AES-256 or stronger for stored data encryption",
                "ENCRYPTION_CONFIG|DB_CONFIG", "A.10.1.1", "PR.DS-1", "preventive", 2,
                "EP_NET_002"),

            // 2-37: Data Encryption in Transit
            CreateControl("2-37", "Defense",
                "تشفير البيانات أثناء النقل",
                "Data Encryption in Transit",
                "يجب تشفير البيانات الحساسة أثناء النقل",
                "Encrypt sensitive data in transit",
                "يجب استخدام TLS 1.2 أو أحدث لجميع الاتصالات",
                "Use TLS 1.2 or newer for all communications",
                "TLS_CONFIG|CERTIFICATE_INVENTORY", "A.10.1.1", "PR.DS-2", "preventive", 2,
                "EP_NET_002"),

            // 2-38: Key Management
            CreateControl("2-38", "Defense",
                "إدارة مفاتيح التشفير",
                "Cryptographic Key Management",
                "يجب إدارة مفاتيح التشفير بشكل آمن طوال دورة حياتها",
                "Manage cryptographic keys securely throughout their lifecycle",
                "يجب استخدام HSM للمفاتيح الحساسة وتطبيق فصل المهام",
                "Use HSM for sensitive keys and apply separation of duties",
                "KEY_INVENTORY|HSM_CONFIG", "A.10.1.2", "PR.DS-1", "preventive", 3,
                "EP_NET_002"),

            // 2-39: Backup Management
            CreateControl("2-39", "Defense",
                "إدارة النسخ الاحتياطي",
                "Backup Management",
                "يجب تطبيق سياسة النسخ الاحتياطي للبيانات الهامة",
                "Implement backup policy for critical data",
                "يجب إجراء النسخ الاحتياطي يومياً واختبار الاستعادة ربع سنوياً",
                "Perform daily backups and test restoration quarterly",
                "BACKUP_POLICY|BACKUP_LOG", "A.12.3", "PR.IP-4", "preventive", 1,
                "EP_OPS_003"),

            // 2-40: Backup Testing
            CreateControl("2-40", "Defense",
                "اختبار النسخ الاحتياطي",
                "Backup Testing",
                "يجب اختبار النسخ الاحتياطي بشكل دوري للتأكد من قابلية الاستعادة",
                "Test backups periodically to ensure recoverability",
                "يجب إجراء اختبار استعادة كامل سنوياً واختبار جزئي ربع سنوياً",
                "Conduct full restoration test annually and partial test quarterly",
                "RESTORE_TEST_REPORT|RESTORE_LOG", "A.12.3.1", "PR.IP-4", "detective", 2,
                "EP_OPS_003"),

            // 2-41: Security Logging
            CreateControl("2-41", "Defense",
                "تسجيل الأحداث الأمنية",
                "Security Logging",
                "يجب تفعيل تسجيل الأحداث الأمنية على جميع الأنظمة",
                "Enable security logging on all systems",
                "يجب تسجيل: تسجيل الدخول، الوصول للموارد، التغييرات الإدارية، الأخطاء",
                "Log: login events, resource access, administrative changes, errors",
                "LOGGING_POLICY|LOG_CONFIG", "A.12.4", "DE.AE-3", "detective", 1,
                "EP_OPS_004"),

            // 2-42: Log Management
            CreateControl("2-42", "Defense",
                "إدارة السجلات",
                "Log Management",
                "يجب جمع وحفظ السجلات الأمنية مركزياً",
                "Collect and store security logs centrally",
                "يجب حفظ السجلات لمدة سنة على الأقل وحمايتها من التعديل",
                "Retain logs for at least one year and protect from modification",
                "SIEM_CONFIG|LOG_RETENTION", "A.12.4.1", "DE.AE-3", "detective", 2,
                "EP_OPS_004"),

            // 2-43: Security Monitoring
            CreateControl("2-43", "Defense",
                "المراقبة الأمنية",
                "Security Monitoring",
                "يجب مراقبة الأحداث الأمنية بشكل مستمر",
                "Monitor security events continuously",
                "يجب إنشاء SOC أو التعاقد مع مزود خدمة للمراقبة 24/7",
                "Establish SOC or contract with provider for 24/7 monitoring",
                "SOC_REPORT|MONITORING_DASHBOARD", "A.12.4.1", "DE.CM-1", "detective", 3,
                "EP_OPS_004"),

            // 2-44: Alert Management
            CreateControl("2-44", "Defense",
                "إدارة التنبيهات",
                "Alert Management",
                "يجب إنشاء آلية لإدارة التنبيهات الأمنية والاستجابة لها",
                "Establish mechanism for managing security alerts and responding",
                "يجب تصنيف التنبيهات حسب الخطورة وتحديد SLA للاستجابة",
                "Classify alerts by severity and define response SLA",
                "ALERT_RULES|RESPONSE_SLA", "A.12.4.1", "DE.AE-5", "detective", 2,
                "EP_OPS_004"),

            // 2-45: Threat Intelligence
            CreateControl("2-45", "Defense",
                "استخبارات التهديدات",
                "Threat Intelligence",
                "يجب الاستفادة من معلومات استخبارات التهديدات",
                "Leverage threat intelligence information",
                "يجب الاشتراك في مصادر موثوقة ودمج المعلومات في الضوابط الأمنية",
                "Subscribe to reliable sources and integrate intelligence into security controls",
                "TI_FEEDS|IOC_INTEGRATION", "A.6.1.4", "ID.RA-2", "detective", 3,
                "EP_OPS_004"),

            // 2-46: Physical Security
            CreateControl("2-46", "Defense",
                "الأمن المادي",
                "Physical Security",
                "يجب تطبيق ضوابط الأمن المادي لحماية مرافق المعلومات",
                "Implement physical security controls to protect information facilities",
                "يجب التحكم في الدخول واستخدام CCTV ومنع الوصول غير المصرح",
                "Control entry, use CCTV, and prevent unauthorized access",
                "PHYSICAL_ACCESS_POLICY|CCTV_LOG", "A.11.1", "PR.AC-2", "preventive", 1,
                "EP_PHY_001"),

            // 2-47: Data Center Security
            CreateControl("2-47", "Defense",
                "أمن مراكز البيانات",
                "Data Center Security",
                "يجب تطبيق ضوابط أمنية مشددة لمراكز البيانات",
                "Implement stringent security controls for data centers",
                "يجب تطبيق مناطق أمنية متعددة والتحكم البيومتري للمناطق الحساسة",
                "Apply multiple security zones and biometric control for sensitive areas",
                "DC_ACCESS_LOG|BIOMETRIC_CONFIG", "A.11.1.1", "PR.AC-2", "preventive", 2,
                "EP_PHY_001"),

            // 2-48: Environmental Controls
            CreateControl("2-48", "Defense",
                "الضوابط البيئية",
                "Environmental Controls",
                "يجب تطبيق ضوابط بيئية لحماية المعدات",
                "Implement environmental controls to protect equipment",
                "يجب مراقبة درجة الحرارة والرطوبة وأنظمة إطفاء الحريق",
                "Monitor temperature, humidity, and fire suppression systems",
                "ENVIRONMENTAL_MONITORING|UPS_TEST", "A.11.2.2", "PR.IP-5", "preventive", 2,
                "EP_PHY_002"),

            // 2-49: Equipment Maintenance
            CreateControl("2-49", "Defense",
                "صيانة المعدات",
                "Equipment Maintenance",
                "يجب صيانة المعدات وفقاً لتوصيات المصنع",
                "Maintain equipment according to manufacturer recommendations",
                "يجب توثيق جميع أعمال الصيانة والتحقق من هوية الفنيين",
                "Document all maintenance and verify technician identity",
                "MAINTENANCE_LOG|SERVICE_CONTRACT", "A.11.2.4", "PR.MA-1", "preventive", 2,
                "EP_PHY_002"),

            // 2-50: Secure Disposal
            CreateControl("2-50", "Defense",
                "التخلص الآمن",
                "Secure Disposal",
                "يجب التخلص من المعدات والوسائط بشكل آمن",
                "Dispose of equipment and media securely",
                "يجب مسح البيانات بشكل آمن أو إتلاف الوسائط المادية",
                "Securely wipe data or physically destroy media",
                "DISPOSAL_CERTIFICATE|DESTRUCTION_LOG", "A.8.3.2", "PR.DS-3", "preventive", 2,
                "EP_ASM_002"),

            // 2-51: DDoS Protection
            CreateControl("2-51", "Defense",
                "الحماية من هجمات الحرمان من الخدمة",
                "DDoS Protection",
                "يجب تطبيق حلول للحماية من هجمات DDoS",
                "Implement DDoS protection solutions",
                "يجب استخدام خدمات CDN والتخفيف السحابي للأنظمة المواجهة للإنترنت",
                "Use CDN services and cloud mitigation for internet-facing systems",
                "DDOS_CONFIG|MITIGATION_PLAN", "A.13.1", "PR.DS-4", "preventive", 2,
                "EP_NET_001"),

            // 2-52: API Security
            CreateControl("2-52", "Defense",
                "أمن واجهات برمجة التطبيقات",
                "API Security",
                "يجب تطبيق ضوابط أمنية لحماية APIs",
                "Implement security controls to protect APIs",
                "يجب المصادقة والتفويض ومراقبة الاستخدام وتحديد المعدل",
                "Implement authentication, authorization, usage monitoring, and rate limiting",
                "API_SECURITY_CONFIG|API_GATEWAY", "A.14.2.5", "PR.DS-5", "preventive", 3,
                "EP_VUL_002"),

            // 2-53: Container Security
            CreateControl("2-53", "Defense",
                "أمن الحاويات",
                "Container Security",
                "يجب تطبيق ضوابط أمنية للحاويات والتنسيق",
                "Implement security controls for containers and orchestration",
                "يجب فحص الصور وتطبيق سياسات أمان وعزل الشبكة",
                "Scan images, apply security policies, and network isolation",
                "CONTAINER_POLICY|IMAGE_SCAN", "A.14.2", "PR.DS-5", "preventive", 3,
                "EP_VUL_002"),

            // 2-54: Cloud Security
            CreateControl("2-54", "Defense",
                "أمن الحوسبة السحابية",
                "Cloud Security",
                "يجب تطبيق ضوابط أمنية للخدمات السحابية",
                "Implement security controls for cloud services",
                "يجب تطبيق نموذج المسؤولية المشتركة ومراجعة إعدادات الأمان",
                "Apply shared responsibility model and review security settings",
                "CLOUD_CONFIG|CSPM_REPORT", "A.15.2", "ID.SC-3", "preventive", 2,
                "EP_TPR_001"),

            // 2-55: Data Loss Prevention
            CreateControl("2-55", "Defense",
                "منع فقدان البيانات",
                "Data Loss Prevention",
                "يجب تطبيق حلول منع فقدان البيانات",
                "Implement data loss prevention solutions",
                "يجب مراقبة البريد والويب والتخزين السحابي والأجهزة",
                "Monitor email, web, cloud storage, and devices",
                "DLP_CONFIG|DLP_RULES", "A.13.2", "PR.DS-5", "detective", 2,
                "EP_ASM_002"),
        };
    }
    #endregion

    #region Domain 3: Cybersecurity Resilience (20 Controls)
    private static List<FrameworkControl> GetResilienceControls()
    {
        return new List<FrameworkControl>
        {
            // 3-1: Incident Response Policy
            CreateControl("3-1", "Resilience",
                "سياسة الاستجابة للحوادث",
                "Incident Response Policy",
                "يجب وضع سياسة للاستجابة لحوادث الأمن السيبراني",
                "Establish incident response policy for cybersecurity incidents",
                "يجب تحديد تعريف الحادث ومستويات الخطورة وإجراءات التصعيد",
                "Define incident definition, severity levels, and escalation procedures",
                "IR_POLICY|CLASSIFICATION_MATRIX", "A.16.1", "RS.RP-1", "preventive", 1,
                "EP_INC_002"),

            // 3-2: Incident Response Plan
            CreateControl("3-2", "Resilience",
                "خطة الاستجابة للحوادث",
                "Incident Response Plan",
                "يجب وضع خطة مفصلة للاستجابة للحوادث",
                "Develop detailed incident response plan",
                "يجب تضمين: الكشف، الاحتواء، الاستئصال، الاستعادة، الدروس المستفادة",
                "Include: detection, containment, eradication, recovery, lessons learned",
                "IR_PLAN|PLAYBOOKS", "A.16.1.5", "RS.RP-1", "preventive", 2,
                "EP_INC_002"),

            // 3-3: Incident Response Team
            CreateControl("3-3", "Resilience",
                "فريق الاستجابة للحوادث",
                "Incident Response Team",
                "يجب تشكيل فريق للاستجابة للحوادث السيبرانية",
                "Form cybersecurity incident response team",
                "يجب تحديد الأعضاء والأدوار وقنوات الاتصال والتوفر",
                "Define members, roles, communication channels, and availability",
                "TEAM_ROSTER|CONTACT_LIST", "A.16.1.1", "RS.CO-1", "preventive", 2,
                "EP_INC_002"),

            // 3-4: Incident Detection
            CreateControl("3-4", "Resilience",
                "كشف الحوادث",
                "Incident Detection",
                "يجب تطبيق آليات للكشف عن الحوادث الأمنية",
                "Implement mechanisms for security incident detection",
                "يجب استخدام SIEM وIDS وEDR ومراقبة السلوك الشاذ",
                "Use SIEM, IDS, EDR, and anomaly behavior monitoring",
                "DETECTION_CAPABILITIES|SIEM_CONFIG", "A.16.1.2", "DE.AE-1", "detective", 2,
                "EP_INC_001"),

            // 3-5: Incident Reporting
            CreateControl("3-5", "Resilience",
                "الإبلاغ عن الحوادث",
                "Incident Reporting",
                "يجب إنشاء آلية للإبلاغ عن الحوادث الأمنية",
                "Establish mechanism for reporting security incidents",
                "يجب توفير قنوات متعددة للإبلاغ وضمان السرية",
                "Provide multiple reporting channels and ensure confidentiality",
                "REPORTING_PROCEDURE|HOTLINE", "A.16.1.2", "RS.CO-2", "detective", 1,
                "EP_INC_001"),

            // 3-6: Incident Triage
            CreateControl("3-6", "Resilience",
                "فرز الحوادث",
                "Incident Triage",
                "يجب فرز الحوادث وتصنيفها حسب الخطورة والأولوية",
                "Triage and classify incidents by severity and priority",
                "يجب تطبيق معايير واضحة للتصنيف وSLA للاستجابة",
                "Apply clear classification criteria and response SLA",
                "TRIAGE_PROCEDURE|SLA_MATRIX", "A.16.1.4", "RS.AN-1", "detective", 2,
                "EP_INC_001"),

            // 3-7: Incident Investigation
            CreateControl("3-7", "Resilience",
                "التحقيق في الحوادث",
                "Incident Investigation",
                "يجب التحقيق في الحوادث لتحديد السبب والنطاق والأثر",
                "Investigate incidents to determine cause, scope, and impact",
                "يجب الحفاظ على سلسلة الحيازة والأدلة الرقمية",
                "Maintain chain of custody and digital evidence",
                "INVESTIGATION_REPORT|FORENSIC_REPORT", "A.16.1.5", "RS.AN-3", "detective", 3,
                "EP_INC_001"),

            // 3-8: Incident Containment
            CreateControl("3-8", "Resilience",
                "احتواء الحوادث",
                "Incident Containment",
                "يجب احتواء الحوادث لمنع انتشارها",
                "Contain incidents to prevent spread",
                "يجب تطوير إجراءات احتواء لكل نوع من الحوادث",
                "Develop containment procedures for each incident type",
                "CONTAINMENT_PLAYBOOK|ISOLATION_PROCEDURE", "A.16.1.5", "RS.MI-1", "corrective", 2,
                "EP_INC_001"),

            // 3-9: Incident Eradication
            CreateControl("3-9", "Resilience",
                "استئصال الحوادث",
                "Incident Eradication",
                "يجب استئصال سبب الحادث من جميع الأنظمة المتأثرة",
                "Eradicate incident cause from all affected systems",
                "يجب التأكد من إزالة جميع آثار الاختراق والبرمجيات الضارة",
                "Ensure removal of all breach traces and malware",
                "ERADICATION_CHECKLIST|CLEANUP_LOG", "A.16.1.5", "RS.MI-2", "corrective", 3,
                "EP_INC_001"),

            // 3-10: Incident Recovery
            CreateControl("3-10", "Resilience",
                "التعافي من الحوادث",
                "Incident Recovery",
                "يجب استعادة الأنظمة والخدمات المتأثرة",
                "Restore affected systems and services",
                "يجب التحقق من سلامة الأنظمة قبل إعادتها للإنتاج",
                "Verify system integrity before returning to production",
                "RECOVERY_PROCEDURE|VALIDATION_CHECKLIST", "A.16.1.6", "RC.RP-1", "corrective", 2,
                "EP_INC_001"),

            // 3-11: Lessons Learned
            CreateControl("3-11", "Resilience",
                "الدروس المستفادة",
                "Lessons Learned",
                "يجب توثيق الدروس المستفادة من الحوادث",
                "Document lessons learned from incidents",
                "يجب عقد اجتماع مراجعة بعد كل حادث جسيم وتحديث الإجراءات",
                "Hold review meeting after each major incident and update procedures",
                "POST_INCIDENT_REPORT|IMPROVEMENT_PLAN", "A.16.1.6", "RS.IM-2", "corrective", 2,
                "EP_INC_001"),

            // 3-12: IR Exercises
            CreateControl("3-12", "Resilience",
                "تمارين الاستجابة للحوادث",
                "Incident Response Exercises",
                "يجب إجراء تمارين دورية للاستجابة للحوادث",
                "Conduct periodic incident response exercises",
                "يجب إجراء تمرين طاولة سنوياً ومحاكاة تقنية نصف سنوياً",
                "Conduct tabletop exercise annually and technical simulation semi-annually",
                "EXERCISE_REPORT|TABLETOP_MINUTES", "A.16.1", "RS.RP-1", "detective", 2,
                "EP_INC_002"),

            // 3-13: Business Impact Analysis
            CreateControl("3-13", "Resilience",
                "تحليل تأثير الأعمال",
                "Business Impact Analysis",
                "يجب إجراء تحليل لتأثير توقف الأعمال على العمليات الحرجة",
                "Conduct analysis of business interruption impact on critical operations",
                "يجب تحديد RTO وRPO لكل نظام حرج",
                "Define RTO and RPO for each critical system",
                "BIA_REPORT|RTO_RPO_MATRIX", "A.17.1.1", "PR.IP-9", "preventive", 2,
                "EP_BCM_001"),

            // 3-14: Business Continuity Plan
            CreateControl("3-14", "Resilience",
                "خطة استمرارية الأعمال",
                "Business Continuity Plan",
                "يجب وضع خطة لاستمرارية الأعمال للعمليات الحرجة",
                "Develop business continuity plan for critical operations",
                "يجب تضمين إجراءات التشغيل البديلة وخطط الاتصال",
                "Include alternative operating procedures and communication plans",
                "BCP_DOC|COMMUNICATION_PLAN", "A.17.1", "PR.IP-9", "preventive", 2,
                "EP_BCM_001"),

            // 3-15: Disaster Recovery Plan
            CreateControl("3-15", "Resilience",
                "خطة التعافي من الكوارث",
                "Disaster Recovery Plan",
                "يجب وضع خطة للتعافي من الكوارث للأنظمة التقنية",
                "Develop disaster recovery plan for technical systems",
                "يجب تحديد موقع التعافي البديل وإجراءات الانتقال",
                "Define alternate recovery site and transition procedures",
                "DR_PLAN|RUNBOOK", "A.17.1.3", "RC.RP-1", "preventive", 2,
                "EP_BCM_002"),

            // 3-16: BC/DR Testing
            CreateControl("3-16", "Resilience",
                "اختبار استمرارية الأعمال والتعافي",
                "BC/DR Testing",
                "يجب اختبار خطط استمرارية الأعمال والتعافي بشكل دوري",
                "Test business continuity and disaster recovery plans periodically",
                "يجب إجراء اختبار كامل سنوياً وتمارين جزئية ربع سنوياً",
                "Conduct full test annually and partial exercises quarterly",
                "TEST_REPORT|FAILOVER_LOG", "A.17.2", "PR.IP-10", "detective", 3,
                "EP_BCM_002"),

            // 3-17: Alternate Processing Site
            CreateControl("3-17", "Resilience",
                "موقع المعالجة البديل",
                "Alternate Processing Site",
                "يجب توفير موقع بديل لمعالجة البيانات في حالة الكوارث",
                "Provide alternate site for data processing in case of disasters",
                "يجب أن يكون الموقع البديل في منطقة جغرافية مختلفة",
                "Alternate site must be in different geographic region",
                "DR_SITE_CONTRACT|INFRASTRUCTURE_DOC", "A.17.1.2", "RC.RP-1", "preventive", 3,
                "EP_BCM_002"),

            // 3-18: Data Replication
            CreateControl("3-18", "Resilience",
                "تكرار البيانات",
                "Data Replication",
                "يجب تكرار البيانات الحرجة للموقع البديل",
                "Replicate critical data to alternate site",
                "يجب تحقيق RPO المطلوب للبيانات الحرجة",
                "Achieve required RPO for critical data",
                "REPLICATION_CONFIG|RPO_REPORT", "A.17.1.2", "PR.IP-4", "preventive", 2,
                "EP_BCM_002"),

            // 3-19: Crisis Communication
            CreateControl("3-19", "Resilience",
                "الاتصالات في الأزمات",
                "Crisis Communication",
                "يجب وضع خطة للاتصالات أثناء الأزمات",
                "Develop crisis communication plan",
                "يجب تحديد المتحدث الرسمي وقوالب الرسائل وقنوات الاتصال",
                "Define spokesperson, message templates, and communication channels",
                "COMMUNICATION_PLAN|CONTACT_TREE", "A.16.1.1", "RS.CO-1", "preventive", 2,
                "EP_INC_002"),

            // 3-20: Plan Maintenance
            CreateControl("3-20", "Resilience",
                "صيانة الخطط",
                "Plan Maintenance",
                "يجب مراجعة وتحديث خطط الاستمرارية والتعافي بشكل دوري",
                "Review and update continuity and recovery plans periodically",
                "يجب المراجعة سنوياً وعند التغييرات الجوهرية",
                "Review annually and upon significant changes",
                "REVIEW_LOG|UPDATE_HISTORY", "A.17.1", "PR.IP-9", "preventive", 2,
                "EP_BCM_001"),
        };
    }
    #endregion

    #region Domain 4: Third-Party Cybersecurity (15 Controls)
    private static List<FrameworkControl> GetThirdPartyControls()
    {
        return new List<FrameworkControl>
        {
            // 4-1: Third-Party Policy
            CreateControl("4-1", "Third-Party",
                "سياسة الأطراف الثالثة",
                "Third-Party Policy",
                "يجب وضع سياسة لإدارة مخاطر الأمن السيبراني للأطراف الثالثة",
                "Establish policy for managing third-party cybersecurity risks",
                "يجب تحديد متطلبات الأمن السيبراني لجميع أنواع العلاقات مع الأطراف الثالثة",
                "Define cybersecurity requirements for all third-party relationship types",
                "POLICY_DOC|PROCEDURE_DOC", "A.15.1", "ID.SC-1", "preventive", 1,
                "EP_TPR_001"),

            // 4-2: Vendor Inventory
            CreateControl("4-2", "Third-Party",
                "سجل الموردين",
                "Vendor Inventory",
                "يجب إنشاء وصيانة سجل لجميع موردي الخدمات والتقنية",
                "Establish and maintain inventory of all service and technology vendors",
                "يجب تضمين نوع الخدمة ومستوى الوصول وتصنيف المخاطر",
                "Include service type, access level, and risk classification",
                "VENDOR_INVENTORY|RISK_REGISTER", "A.15.1", "ID.SC-2", "preventive", 1,
                "EP_TPR_001"),

            // 4-3: Vendor Risk Assessment
            CreateControl("4-3", "Third-Party",
                "تقييم مخاطر الموردين",
                "Vendor Risk Assessment",
                "يجب إجراء تقييم لمخاطر الأمن السيبراني قبل التعاقد",
                "Conduct cybersecurity risk assessment before contracting",
                "يجب تقييم الوضع الأمني والقدرات والسجل التاريخي",
                "Assess security posture, capabilities, and track record",
                "RISK_ASSESSMENT|DUE_DILIGENCE", "A.15.1.2", "ID.SC-2", "preventive", 2,
                "EP_TPR_001"),

            // 4-4: Security Requirements in Contracts
            CreateControl("4-4", "Third-Party",
                "المتطلبات الأمنية في العقود",
                "Security Requirements in Contracts",
                "يجب تضمين متطلبات الأمن السيبراني في جميع العقود",
                "Include cybersecurity requirements in all contracts",
                "يجب تضمين: التزامات الأمان، حق التدقيق، الإبلاغ عن الحوادث، إنهاء الخدمة",
                "Include: security obligations, audit rights, incident reporting, service termination",
                "CONTRACT_TEMPLATE|SECURITY_ADDENDUM", "A.15.1.2", "ID.SC-3", "preventive", 2,
                "EP_TPR_001"),

            // 4-5: Vendor Access Management
            CreateControl("4-5", "Third-Party",
                "إدارة وصول الموردين",
                "Vendor Access Management",
                "يجب التحكم في وصول الموردين لأنظمة المنظمة",
                "Control vendor access to organization systems",
                "يجب تطبيق مبدأ الحد الأدنى من الصلاحيات ومراقبة الوصول",
                "Apply least privilege principle and monitor access",
                "ACCESS_REQUEST|ACCESS_LOG", "A.15.1.3", "PR.AC-3", "preventive", 2,
                "EP_IAM_001"),

            // 4-6: Vendor Monitoring
            CreateControl("4-6", "Third-Party",
                "مراقبة الموردين",
                "Vendor Monitoring",
                "يجب مراقبة أداء الموردين وامتثالهم بشكل دوري",
                "Monitor vendor performance and compliance periodically",
                "يجب مراجعة SLA والامتثال الأمني والحوادث بشكل ربع سنوي",
                "Review SLA, security compliance, and incidents quarterly",
                "PERFORMANCE_REPORT|COMPLIANCE_REVIEW", "A.15.2.1", "ID.SC-4", "detective", 2,
                "EP_TPR_001"),

            // 4-7: Vendor Audits
            CreateControl("4-7", "Third-Party",
                "تدقيق الموردين",
                "Vendor Audits",
                "يجب إجراء تدقيق أمني للموردين ذوي المخاطر العالية",
                "Conduct security audits for high-risk vendors",
                "يجب إجراء تدقيق سنوي أو طلب تقارير SOC 2",
                "Conduct annual audit or request SOC 2 reports",
                "AUDIT_REPORT|SOC2_REPORT", "A.15.2.2", "ID.SC-4", "detective", 3,
                "EP_TPR_002"),

            // 4-8: Cloud Service Security
            CreateControl("4-8", "Third-Party",
                "أمن الخدمات السحابية",
                "Cloud Service Security",
                "يجب تقييم وإدارة مخاطر الخدمات السحابية",
                "Assess and manage cloud service risks",
                "يجب مراجعة إعدادات الأمان والامتثال وموقع البيانات",
                "Review security settings, compliance, and data location",
                "CLOUD_ASSESSMENT|SECURITY_CONFIG", "A.15.2", "ID.SC-3", "preventive", 2,
                "EP_TPR_001"),

            // 4-9: Data Protection with Vendors
            CreateControl("4-9", "Third-Party",
                "حماية البيانات مع الموردين",
                "Data Protection with Vendors",
                "يجب حماية البيانات المشاركة مع الموردين",
                "Protect data shared with vendors",
                "يجب تشفير البيانات وتحديد حدود الاستخدام وحقوق الملكية",
                "Encrypt data, define usage limits, and ownership rights",
                "DPA|DATA_HANDLING_AGREEMENT", "A.15.1.3", "PR.DS-5", "preventive", 2,
                "EP_TPR_001"),

            // 4-10: Incident Response with Vendors
            CreateControl("4-10", "Third-Party",
                "الاستجابة للحوادث مع الموردين",
                "Incident Response with Vendors",
                "يجب تنسيق الاستجابة للحوادث مع الموردين",
                "Coordinate incident response with vendors",
                "يجب تحديد إجراءات الإبلاغ والتصعيد والتعاون في التحقيق",
                "Define reporting, escalation, and investigation cooperation procedures",
                "IR_PROCEDURE|ESCALATION_MATRIX", "A.16.1", "RS.CO-4", "corrective", 2,
                "EP_INC_002"),

            // 4-11: Vendor Offboarding
            CreateControl("4-11", "Third-Party",
                "إنهاء العلاقة مع الموردين",
                "Vendor Offboarding",
                "يجب تطبيق إجراءات آمنة لإنهاء العلاقة مع الموردين",
                "Apply secure procedures for vendor offboarding",
                "يجب إلغاء الوصول واسترداد الأصول وضمان حذف البيانات",
                "Revoke access, recover assets, and ensure data deletion",
                "OFFBOARDING_CHECKLIST|DATA_DELETION_CERT", "A.15.1.3", "ID.SC-5", "preventive", 2,
                "EP_TPR_001"),

            // 4-12: Supply Chain Security
            CreateControl("4-12", "Third-Party",
                "أمن سلسلة التوريد",
                "Supply Chain Security",
                "يجب تقييم مخاطر سلسلة التوريد للمنتجات والخدمات",
                "Assess supply chain risks for products and services",
                "يجب التحقق من مصدر المنتجات والتأكد من سلامتها",
                "Verify product source and ensure integrity",
                "SUPPLY_CHAIN_ASSESSMENT|INTEGRITY_CHECK", "A.15.1.3", "ID.SC-3", "preventive", 3,
                "EP_TPR_001"),

            // 4-13: Software Bill of Materials
            CreateControl("4-13", "Third-Party",
                "قائمة مكونات البرمجيات",
                "Software Bill of Materials",
                "يجب الحصول على SBOM للبرمجيات التجارية والمفتوحة المصدر",
                "Obtain SBOM for commercial and open source software",
                "يجب مراجعة المكونات للثغرات المعروفة والتراخيص",
                "Review components for known vulnerabilities and licenses",
                "SBOM|VULNERABILITY_ASSESSMENT", "A.14.2.1", "ID.SC-2", "preventive", 3,
                "EP_VUL_002"),

            // 4-14: Outsourcing Security
            CreateControl("4-14", "Third-Party",
                "أمن الخدمات المسندة",
                "Outsourcing Security",
                "يجب تطبيق ضوابط أمنية للخدمات المسندة لأطراف خارجية",
                "Apply security controls for outsourced services",
                "يجب ضمان استمرار المسؤولية الأمنية وحق الرقابة",
                "Ensure continued security responsibility and oversight rights",
                "OUTSOURCING_AGREEMENT|SECURITY_SLA", "A.15.1", "ID.SC-1", "preventive", 2,
                "EP_TPR_001"),

            // 4-15: Fourth-Party Risk
            CreateControl("4-15", "Third-Party",
                "مخاطر الطرف الرابع",
                "Fourth-Party Risk",
                "يجب تقييم مخاطر الأطراف الرابعة (موردي الموردين)",
                "Assess fourth-party risks (vendors' vendors)",
                "يجب فهم سلسلة التوريد ومتطلبات الأمان للأطراف الرابعة",
                "Understand supply chain and security requirements for fourth parties",
                "SUPPLY_CHAIN_MAPPING|SUBCONTRACTOR_POLICY", "A.15.1.3", "ID.SC-2", "preventive", 3,
                "EP_TPR_001"),
        };
    }
    #endregion

    #region Domain 5: ICS/OT Security (10 Controls)
    private static List<FrameworkControl> GetIcsControls()
    {
        return new List<FrameworkControl>
        {
            // 5-1: OT Security Policy
            CreateControl("5-1", "ICS",
                "سياسة أمن التقنية التشغيلية",
                "OT Security Policy",
                "يجب وضع سياسة أمنية خاصة بأنظمة التقنية التشغيلية",
                "Establish security policy specific to operational technology systems",
                "يجب مراعاة متطلبات التوافر والسلامة الخاصة بأنظمة OT",
                "Consider availability and safety requirements specific to OT systems",
                "OT_POLICY|ICS_STANDARDS", "A.5.1", "ID.GV-1", "preventive", 2,
                "EP_GOV_001"),

            // 5-2: OT/IT Network Segmentation
            CreateControl("5-2", "ICS",
                "فصل شبكات OT و IT",
                "OT/IT Network Segmentation",
                "يجب فصل شبكات التقنية التشغيلية عن شبكات تقنية المعلومات",
                "Segment operational technology networks from IT networks",
                "يجب استخدام DMZ وجدران نارية صناعية وبوابات أحادية الاتجاه",
                "Use DMZ, industrial firewalls, and unidirectional gateways",
                "NETWORK_DIAGRAM|FIREWALL_RULES", "A.13.1.3", "PR.AC-5", "preventive", 3,
                "EP_NET_001"),

            // 5-3: OT Asset Inventory
            CreateControl("5-3", "ICS",
                "جرد أصول التقنية التشغيلية",
                "OT Asset Inventory",
                "يجب إنشاء سجل شامل لجميع أصول التقنية التشغيلية",
                "Establish comprehensive inventory of all OT assets",
                "يجب تضمين PLCs وRTUs وHMIs وSCADA والشبكات الصناعية",
                "Include PLCs, RTUs, HMIs, SCADA, and industrial networks",
                "OT_ASSET_INVENTORY|NETWORK_MAP", "A.8.1", "ID.AM-1", "preventive", 2,
                "EP_ASM_001"),

            // 5-4: OT Access Control
            CreateControl("5-4", "ICS",
                "التحكم في الوصول لأنظمة OT",
                "OT Access Control",
                "يجب تطبيق ضوابط صارمة للوصول لأنظمة التقنية التشغيلية",
                "Apply stringent access controls for OT systems",
                "يجب تطبيق المصادقة متعددة العوامل وتسجيل جميع عمليات الوصول",
                "Apply multi-factor authentication and log all access",
                "ACCESS_POLICY|ACCESS_LOG", "A.9.1", "PR.AC-1", "preventive", 3,
                "EP_IAM_001"),

            // 5-5: OT Patch Management
            CreateControl("5-5", "ICS",
                "إدارة التحديثات لأنظمة OT",
                "OT Patch Management",
                "يجب إدارة التحديثات لأنظمة OT مع مراعاة متطلبات التشغيل",
                "Manage patches for OT systems considering operational requirements",
                "يجب اختبار التحديثات في بيئة معزولة قبل التطبيق",
                "Test patches in isolated environment before deployment",
                "PATCH_PROCEDURE|TEST_RESULTS", "A.12.6", "PR.IP-12", "preventive", 3,
                "EP_OPS_002"),

            // 5-6: OT Security Monitoring
            CreateControl("5-6", "ICS",
                "مراقبة أمن التقنية التشغيلية",
                "OT Security Monitoring",
                "يجب مراقبة أنظمة OT للكشف عن الشذوذ والتهديدات",
                "Monitor OT systems for anomaly and threat detection",
                "يجب استخدام حلول مراقبة متوافقة مع بروتوكولات OT",
                "Use monitoring solutions compatible with OT protocols",
                "OT_MONITORING_CONFIG|ALERT_LOG", "A.12.4", "DE.CM-1", "detective", 3,
                "EP_OPS_004"),

            // 5-7: OT Incident Response
            CreateControl("5-7", "ICS",
                "الاستجابة لحوادث OT",
                "OT Incident Response",
                "يجب وضع خطة استجابة للحوادث خاصة بأنظمة OT",
                "Develop incident response plan specific to OT systems",
                "يجب مراعاة متطلبات السلامة وعدم إيقاف العمليات الحرجة",
                "Consider safety requirements and avoid stopping critical operations",
                "OT_IR_PLAN|SAFETY_PROCEDURES", "A.16.1", "RS.RP-1", "corrective", 3,
                "EP_INC_002"),

            // 5-8: OT Vendor Management
            CreateControl("5-8", "ICS",
                "إدارة موردي أنظمة OT",
                "OT Vendor Management",
                "يجب إدارة مخاطر موردي أنظمة التقنية التشغيلية",
                "Manage OT system vendor risks",
                "يجب التحقق من أمان الوصول عن بعد للموردين",
                "Verify security of vendor remote access",
                "VENDOR_ACCESS_POLICY|REMOTE_ACCESS_LOG", "A.15.1", "ID.SC-1", "preventive", 3,
                "EP_TPR_001"),

            // 5-9: OT Physical Security
            CreateControl("5-9", "ICS",
                "الأمن المادي لأنظمة OT",
                "OT Physical Security",
                "يجب حماية المعدات التشغيلية من الوصول المادي غير المصرح",
                "Protect operational equipment from unauthorized physical access",
                "يجب تأمين غرف التحكم والمعدات الميدانية",
                "Secure control rooms and field equipment",
                "PHYSICAL_ACCESS_POLICY|SURVEILLANCE_LOG", "A.11.1", "PR.AC-2", "preventive", 2,
                "EP_PHY_001"),

            // 5-10: OT Backup and Recovery
            CreateControl("5-10", "ICS",
                "النسخ الاحتياطي والتعافي لأنظمة OT",
                "OT Backup and Recovery",
                "يجب نسخ إعدادات أنظمة OT احتياطياً ووضع خطط للتعافي",
                "Backup OT system configurations and develop recovery plans",
                "يجب اختبار إجراءات التعافي دورياً",
                "Test recovery procedures periodically",
                "BACKUP_CONFIG|RECOVERY_TEST", "A.12.3", "PR.IP-4", "preventive", 2,
                "EP_OPS_003"),
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
            FrameworkCode = "NCA-ECC",
            Version = "1.2018",
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
