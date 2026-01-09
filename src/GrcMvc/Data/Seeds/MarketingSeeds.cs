using GrcMvc.Models.Entities.Marketing;
using Microsoft.EntityFrameworkCore;

namespace GrcMvc.Data.Seeds;

/// <summary>
/// Seeds marketing content: testimonials, case studies, pricing plans, trust badges, etc.
/// بذور المحتوى التسويقي: الشهادات، دراسات الحالة، خطط الأسعار، شارات الثقة، إلخ
/// </summary>
public static class MarketingSeeds
{
    public static async Task SeedMarketingDataAsync(GrcDbContext context)
    {
        await SeedTestimonialsAsync(context);
        await SeedCaseStudiesAsync(context);
        await SeedPricingPlansAsync(context);
        await SeedClientLogosAsync(context);
        await SeedTrustBadgesAsync(context);
        await SeedFaqsAsync(context);
        await SeedLandingStatisticsAsync(context);
        await SeedFeatureHighlightsAsync(context);
        await SeedPartnersAsync(context);
    }

    private static async Task SeedTestimonialsAsync(GrcDbContext context)
    {
        // ==========================================================================
        // TESTIMONIALS SEEDING - DISABLED
        // 
        // REASON: We are NEW TO MARKET and have NO REAL CUSTOMERS yet.
        // 
        // Adding fake testimonials with names like "Eng. Khalid Al-Saeed" or 
        // "Dr. Sarah Al-Omari" is MISLEADING and potentially ILLEGAL (false advertising).
        // 
        // DO NOT seed any testimonials until we have:
        // 1. Real paying customers
        // 2. Written consent from those customers to use their testimonials
        // 3. Verified quotes approved by the customer
        // 
        // The view is configured to hide the testimonials section when empty.
        // ==========================================================================
        
        // No testimonials to seed - we have no real customers yet
        await Task.CompletedTask;
    }

    private static async Task SeedCaseStudiesAsync(GrcDbContext context)
    {
        if (await context.CaseStudies.AnyAsync())
            return;

        var caseStudies = new List<CaseStudy>
        {
            new()
            {
                Title = "Leading Bank Achieves Full SAMA CSF Compliance in 3 Months",
                TitleAr = "بنك رائد يحقق الامتثال الكامل لـ SAMA CSF في 3 أشهر",
                Slug = "bank-sama-csf-compliance",
                Summary = "How one of the largest banks in Saudi Arabia used Shahin to assess their current state, identify gaps, and achieve full compliance ahead of schedule.",
                SummaryAr = "كيف استخدم أحد أكبر البنوك في المملكة منصة شاهين لتقييم وضعه الحالي، وتحديد الفجوات، وتحقيق الامتثال الكامل قبل الموعد المحدد.",
                Industry = "Financial Services",
                IndustryAr = "القطاع المالي",
                FrameworkCode = "SAMA-CSF",
                TimeToCompliance = "3 months",
                ImprovementMetric = "70%",
                ImprovementLabel = "Time Saved",
                ImprovementLabelAr = "توفير في الوقت",
                ComplianceScore = "100%",
                IsFeatured = true,
                DisplayOrder = 1
            },
            new()
            {
                Title = "Private Hospital Achieves CBAHI Accreditation",
                TitleAr = "مستشفى خاص يحقق اعتماد CBAHI",
                Slug = "hospital-cbahi-accreditation",
                Summary = "A private hospital's journey from zero to full accreditation using Shahin's evidence management and workflow automation tools.",
                SummaryAr = "رحلة مستشفى خاص من صفر إلى الاعتماد الكامل باستخدام أدوات شاهين لإدارة الأدلة وسير العمل.",
                Industry = "Healthcare",
                IndustryAr = "الرعاية الصحية",
                FrameworkCode = "CBAHI",
                TimeToCompliance = "6 months",
                ImprovementMetric = "Full",
                ImprovementLabel = "Accreditation",
                ImprovementLabelAr = "اعتماد كامل",
                ComplianceScore = "100%",
                DisplayOrder = 2
            },
            new()
            {
                Title = "Tech Startup Achieves NCA ECC Compliance",
                TitleAr = "شركة تقنية ناشئة تحقق امتثال NCA ECC",
                Slug = "startup-nca-ecc-compliance",
                Summary = "How Shahin helped a SaaS startup obtain cybersecurity certification to qualify for government contracts.",
                SummaryAr = "كيف ساعدت شاهين شركة SaaS ناشئة على الحصول على شهادة الأمن السيبراني للتأهل للعقود الحكومية.",
                Industry = "Technology",
                IndustryAr = "التقنية",
                FrameworkCode = "NCA-ECC",
                TimeToCompliance = "4 months",
                ImprovementMetric = "Certified",
                ImprovementLabel = "NCA ECC",
                ImprovementLabelAr = "شهادة NCA ECC",
                ComplianceScore = "95%",
                DisplayOrder = 3
            },
            new()
            {
                Title = "Retail Chain Implements PDPL Compliance",
                TitleAr = "سلسلة تجزئة كبرى تطبق PDPL",
                Slug = "retail-pdpl-compliance",
                Summary = "Complete transformation in personal data management for over 2 million customers while ensuring PDPL compliance.",
                SummaryAr = "تحول كامل في إدارة البيانات الشخصية لأكثر من 2 مليون عميل مع ضمان الامتثال لـ PDPL.",
                Industry = "Retail",
                IndustryAr = "التجزئة",
                FrameworkCode = "PDPL",
                TimeToCompliance = "5 months",
                ImprovementMetric = "2M+",
                ImprovementLabel = "Customers Protected",
                ImprovementLabelAr = "عميل محمي",
                ComplianceScore = "100%",
                DisplayOrder = 4
            },
            new()
            {
                Title = "Telecom Provider Unifies GRC Framework",
                TitleAr = "مزود اتصالات يوحد إطار GRC",
                Slug = "telecom-grc-unification",
                Summary = "Consolidating multiple separate systems into one integrated platform for governance, risk, and compliance management.",
                SummaryAr = "دمج عدة أنظمة منفصلة في منصة واحدة متكاملة لإدارة الحوكمة والمخاطر والامتثال.",
                Industry = "Telecommunications",
                IndustryAr = "الاتصالات",
                FrameworkCode = "Multi",
                TimeToCompliance = "8 months",
                ImprovementMetric = "40%",
                ImprovementLabel = "Cost Reduction",
                ImprovementLabelAr = "توفير في التكاليف",
                ComplianceScore = "5 systems unified",
                DisplayOrder = 5
            }
        };

        context.CaseStudies.AddRange(caseStudies);
        await context.SaveChangesAsync();
    }

    private static async Task SeedPricingPlansAsync(GrcDbContext context)
    {
        if (await context.PricingPlans.AnyAsync())
            return;

        var plans = new List<Models.Entities.Marketing.PricingPlan>
        {
            new()
            {
                Name = "Trial",
                NameAr = "تجريبي",
                Description = "7-day full access trial",
                DescriptionAr = "تجربة كاملة لمدة 7 أيام",
                Price = 0,
                Period = "7 days",
                FeaturesJson = "[\"Full platform access\",\"96-question smart onboarding\",\"1 workspace\",\"Basic email support\",\"All frameworks included\"]",
                FeaturesJsonAr = "[\"وصول كامل للمنصة\",\"استبيان التسجيل الذكي (96 سؤال)\",\"مساحة عمل واحدة\",\"دعم أساسي عبر البريد\",\"جميع الأُطر مشمولة\"]",
                MaxUsers = 3,
                MaxWorkspaces = 1,
                MaxFrameworks = -1,
                DisplayOrder = 1
            },
            new()
            {
                Name = "Starter",
                NameAr = "مبتدئ",
                Description = "For small organizations",
                DescriptionAr = "للمؤسسات الصغيرة",
                Price = 999,
                Period = "month",
                FeaturesJson = "[\"Up to 5 users\",\"2 workspaces\",\"5 frameworks\",\"Email support\",\"Basic reporting\",\"Evidence management\"]",
                FeaturesJsonAr = "[\"حتى 5 مستخدمين\",\"مساحتي عمل\",\"5 أُطر تنظيمية\",\"دعم عبر البريد\",\"تقارير أساسية\",\"إدارة الأدلة\"]",
                MaxUsers = 5,
                MaxWorkspaces = 2,
                MaxFrameworks = 5,
                DisplayOrder = 2
            },
            new()
            {
                Name = "Professional",
                NameAr = "احترافي",
                Description = "For growing organizations",
                DescriptionAr = "للمؤسسات المتوسطة",
                Price = 2999,
                Period = "month",
                FeaturesJson = "[\"Up to 25 users\",\"Unlimited workspaces\",\"All frameworks\",\"Priority support\",\"API access\",\"Advanced analytics\",\"Custom workflows\",\"Audit trails\"]",
                FeaturesJsonAr = "[\"حتى 25 مستخدماً\",\"مساحات عمل غير محدودة\",\"جميع الأُطر\",\"دعم ذو أولوية\",\"وصول API\",\"تحليلات متقدمة\",\"سير عمل مخصص\",\"سجلات التدقيق\"]",
                MaxUsers = 25,
                MaxWorkspaces = -1,
                MaxFrameworks = -1,
                HasApiAccess = true,
                HasPrioritySupport = true,
                IsPopular = true,
                DisplayOrder = 3
            },
            new()
            {
                Name = "Enterprise",
                NameAr = "مؤسسي",
                Description = "For large organizations",
                DescriptionAr = "للمؤسسات الكبيرة",
                Price = -1,
                Period = "custom",
                FeaturesJson = "[\"Unlimited users\",\"Custom integrations\",\"Dedicated success manager\",\"On-premise option\",\"SLA guarantee\",\"Custom training\",\"White-labeling\",\"Multi-tenant support\"]",
                FeaturesJsonAr = "[\"مستخدمون غير محدودين\",\"تكاملات مخصصة\",\"مدير نجاح مخصص\",\"خيار التثبيت المحلي\",\"ضمان SLA\",\"تدريب مخصص\",\"العلامة البيضاء\",\"دعم متعدد المستأجرين\"]",
                MaxUsers = -1,
                MaxWorkspaces = -1,
                MaxFrameworks = -1,
                HasApiAccess = true,
                HasPrioritySupport = true,
                DisplayOrder = 4
            }
        };

        context.PricingPlans.AddRange(plans);
        await context.SaveChangesAsync();
    }

    private static async Task SeedClientLogosAsync(GrcDbContext context)
    {
        // CLIENT LOGOS DISABLED - New to market, no customers yet
        // This section will be populated with real customer logos once we have them
        // DO NOT seed fake client logos (Aramco, STC, etc.) - that's misleading marketing
        
        // Placeholder: When ready, add real clients here
        // Example structure:
        // new ClientLogo {
        //     ClientName = "Real Client Name",
        //     ClientNameAr = "اسم العميل الحقيقي",
        //     LogoUrl = "/images/clients/real-client.svg",
        //     Industry = "Industry",
        //     IsFeatured = true
        // }
        
        await Task.CompletedTask;
    }

    private static async Task SeedTrustBadgesAsync(GrcDbContext context)
    {
        if (await context.TrustBadges.AnyAsync())
            return;

        // HONEST BADGES - Frameworks we SUPPORT, not certifications we HAVE
        // These show what standards users can achieve compliance with using our platform
        var badges = new List<TrustBadge>
        {
            new()
            {
                Name = "NCA ECC Support",
                NameAr = "يدعم NCA ECC",
                Description = "Built for National Cybersecurity Authority compliance",
                DescriptionAr = "مبني للامتثال للهيئة الوطنية للأمن السيبراني",
                ImageUrl = "/images/badges/nca.svg",
                Category = "Framework",
                BadgeCode = "NCA-ECC",
                DisplayOrder = 1
            },
            new()
            {
                Name = "SAMA CSF Support",
                NameAr = "يدعم SAMA CSF",
                Description = "Saudi Central Bank Cybersecurity Framework ready",
                DescriptionAr = "جاهز لإطار الأمن السيبراني للبنك المركزي",
                ImageUrl = "/images/badges/sama.svg",
                Category = "Framework",
                BadgeCode = "SAMA-CSF",
                DisplayOrder = 2
            },
            new()
            {
                Name = "PDPL Support",
                NameAr = "يدعم PDPL",
                Description = "Personal Data Protection Law compliance tools",
                DescriptionAr = "أدوات الامتثال لنظام حماية البيانات الشخصية",
                ImageUrl = "/images/badges/pdpl.svg",
                Category = "Framework",
                BadgeCode = "PDPL",
                DisplayOrder = 3
            },
            new()
            {
                Name = "ISO 27001 Support",
                NameAr = "يدعم ISO 27001",
                Description = "Information Security Management System framework",
                DescriptionAr = "إطار نظام إدارة أمن المعلومات",
                ImageUrl = "/images/badges/iso27001.svg",
                Category = "Framework",
                BadgeCode = "ISO27001",
                DisplayOrder = 4
            },
            new()
            {
                Name = "CBAHI Support",
                NameAr = "يدعم CBAHI",
                Description = "Healthcare accreditation framework",
                DescriptionAr = "إطار اعتماد الرعاية الصحية",
                ImageUrl = "/images/badges/cbahi.svg",
                Category = "Framework",
                BadgeCode = "CBAHI",
                DisplayOrder = 5
            },
            new()
            {
                Name = "CITC Support",
                NameAr = "يدعم CITC",
                Description = "Communications & IT Commission regulations",
                DescriptionAr = "لوائح هيئة الاتصالات وتقنية المعلومات",
                ImageUrl = "/images/badges/citc.svg",
                Category = "Framework",
                BadgeCode = "CITC",
                DisplayOrder = 6
            }
        };

        context.TrustBadges.AddRange(badges);
        await context.SaveChangesAsync();
    }

    private static async Task SeedFaqsAsync(GrcDbContext context)
    {
        if (await context.Faqs.AnyAsync())
            return;

        var faqs = new List<Faq>
        {
            new()
            {
                Question = "How long does the trial period last?",
                QuestionAr = "كم مدة الفترة التجريبية؟",
                Answer = "The trial period lasts 7 days with full access to all features. No credit card required.",
                AnswerAr = "الفترة التجريبية 7 أيام مع وصول كامل لجميع الميزات. لا يلزم بطاقة ائتمان.",
                Category = "General",
                DisplayOrder = 1
            },
            new()
            {
                Question = "What regulatory frameworks does Shahin support?",
                QuestionAr = "ما الأُطر التنظيمية التي يدعمها شاهين؟",
                Answer = "Shahin supports all major KSA frameworks including NCA ECC, SAMA CSF, PDPL, CBAHI, and international standards like ISO 27001, SOC 2, and GDPR. Our library includes 130+ regulators and 400+ controls.",
                AnswerAr = "يدعم شاهين جميع الأُطر السعودية الرئيسية بما في ذلك NCA ECC وSAMA CSF وPDPL وCBAHI والمعايير الدولية مثل ISO 27001 وSOC 2 وGDPR. تضم مكتبتنا أكثر من 130 جهة تنظيمية و400 ضابط.",
                Category = "Compliance",
                DisplayOrder = 2
            },
            new()
            {
                Question = "How does the smart scope derivation work?",
                QuestionAr = "كيف يعمل الاشتقاق الذكي للنطاق؟",
                Answer = "Our 96-question onboarding assesses your organization profile, industry, size, and operations. The rules engine then automatically derives which frameworks and controls apply to you.",
                AnswerAr = "يقيّم استبيان التسجيل المكون من 96 سؤالاً ملف مؤسستك، قطاعها، حجمها، وعملياتها. ثم يستنتج محرك القواعد تلقائياً الأُطر والضوابط التي تنطبق عليك.",
                Category = "Technical",
                DisplayOrder = 3
            },
            new()
            {
                Question = "Can I integrate Shahin with my existing systems?",
                QuestionAr = "هل يمكنني دمج شاهين مع أنظمتي الحالية؟",
                Answer = "Yes, Shahin offers REST APIs for integration with ERP, SIEM, ticketing systems, and more. Enterprise plans include custom integration support.",
                AnswerAr = "نعم، يوفر شاهين واجهات برمجة REST للتكامل مع ERP وSIEM وأنظمة التذاكر وغيرها. تتضمن الخطط المؤسسية دعم التكامل المخصص.",
                Category = "Technical",
                DisplayOrder = 4
            },
            new()
            {
                Question = "Is my data secure in Shahin?",
                QuestionAr = "هل بياناتي آمنة في شاهين؟",
                Answer = "Absolutely. Shahin is built on enterprise-grade infrastructure with security best practices. Data is encrypted at rest and in transit, with role-based access controls and full audit trails.",
                AnswerAr = "بالتأكيد. شاهين مبني على بنية تحتية مؤسسية مع أفضل ممارسات الأمان. البيانات مشفرة أثناء التخزين والنقل، مع صلاحيات مبنية على الأدوار وسجلات تدقيق كاملة.",
                Category = "Security",
                DisplayOrder = 5
            },
            new()
            {
                Question = "How much does Shahin cost?",
                QuestionAr = "كم تكلفة شاهين؟",
                Answer = "Pricing starts at 999 SAR/month for the Starter plan. Professional and Enterprise plans are available for larger organizations. Contact us for a custom quote.",
                AnswerAr = "تبدأ الأسعار من 999 ريال/شهرياً للخطة المبتدئة. الخطط الاحترافية والمؤسسية متاحة للمؤسسات الأكبر. تواصل معنا للحصول على عرض سعر مخصص.",
                Category = "Pricing",
                DisplayOrder = 6
            },
            new()
            {
                Question = "What support options are available?",
                QuestionAr = "ما خيارات الدعم المتاحة؟",
                Answer = "We offer email support for all plans, priority support for Professional, and dedicated success managers for Enterprise. Arabic and English support available.",
                AnswerAr = "نقدم دعم بريد إلكتروني لجميع الخطط، ودعم ذو أولوية للخطة الاحترافية، ومديري نجاح مخصصين للخطة المؤسسية. الدعم متاح بالعربية والإنجليزية.",
                Category = "Support",
                DisplayOrder = 7
            }
        };

        context.Faqs.AddRange(faqs);
        await context.SaveChangesAsync();
    }

    private static async Task SeedLandingStatisticsAsync(GrcDbContext context)
    {
        if (await context.LandingStatistics.AnyAsync())
            return;

        // HONEST STATS - Product capabilities, not fake customer numbers
        var stats = new List<LandingStatistic>
        {
            new()
            {
                Label = "Regulatory Agencies",
                LabelAr = "جهة تنظيمية",
                Value = "120",
                Suffix = "+",
                IconClass = "bi-building",
                Category = "Platform",
                DisplayOrder = 1
            },
            new()
            {
                Label = "Compliance Frameworks",
                LabelAr = "إطار امتثال",
                Value = "130",
                Suffix = "+",
                IconClass = "bi-shield-check",
                Category = "Compliance",
                DisplayOrder = 2
            },
            new()
            {
                Label = "Controls Library",
                LabelAr = "مكتبة الضوابط",
                Value = "400",
                Suffix = "+",
                IconClass = "bi-gear",
                Category = "Compliance",
                DisplayOrder = 3
            },
            new()
            {
                Label = "Onboarding Questions",
                LabelAr = "سؤال تسجيل",
                Value = "96",
                IconClass = "bi-question-circle",
                Category = "Platform",
                DisplayOrder = 4
            },
            new()
            {
                Label = "AI Agents",
                LabelAr = "وكيل ذكي",
                Value = "9",
                IconClass = "bi-robot",
                Category = "AI",
                DisplayOrder = 5
            },
            new()
            {
                Label = "KSA Frameworks",
                LabelAr = "إطار سعودي",
                Value = "15",
                Suffix = "+",
                IconClass = "bi-flag",
                Category = "Compliance",
                DisplayOrder = 6
            }
        };

        context.LandingStatistics.AddRange(stats);
        await context.SaveChangesAsync();
    }

    private static async Task SeedFeatureHighlightsAsync(GrcDbContext context)
    {
        if (await context.FeatureHighlights.AnyAsync())
            return;

        var features = new List<FeatureHighlight>
        {
            new()
            {
                Title = "Smart Compliance Scope",
                TitleAr = "نطاق الامتثال الذكي",
                Description = "96-question onboarding automatically derives applicable frameworks based on your organization profile.",
                DescriptionAr = "استبيان التسجيل المكون من 96 سؤالاً يستنتج تلقائياً الأُطر المطبقة بناءً على ملف مؤسستك.",
                IconClass = "bi-bullseye",
                Category = "Core",
                DisplayOrder = 1,
                IsFeatured = true
            },
            new()
            {
                Title = "Evidence Management",
                TitleAr = "إدارة الأدلة",
                Description = "Centralized evidence collection, versioning, and approval workflows with full audit trails.",
                DescriptionAr = "جمع الأدلة المركزي، إصدارات، وسير عمل الموافقات مع سجلات تدقيق كاملة.",
                IconClass = "bi-folder-check",
                Category = "Core",
                DisplayOrder = 2,
                IsFeatured = true
            },
            new()
            {
                Title = "Risk Assessment",
                TitleAr = "تقييم المخاطر",
                Description = "Comprehensive risk identification, scoring, mitigation tracking, and heat map visualization.",
                DescriptionAr = "تحديد شامل للمخاطر، التسجيل، تتبع التخفيف، وتصور خريطة الحرارة.",
                IconClass = "bi-shield-exclamation",
                Category = "Core",
                DisplayOrder = 3,
                IsFeatured = true
            },
            new()
            {
                Title = "Audit Management",
                TitleAr = "إدارة المراجعة",
                Description = "Plan, execute, and track internal and external audits with finding management and action plans.",
                DescriptionAr = "تخطيط وتنفيذ وتتبع المراجعات الداخلية والخارجية مع إدارة النتائج وخطط العمل.",
                IconClass = "bi-search",
                Category = "Core",
                DisplayOrder = 4
            },
            new()
            {
                Title = "Policy Engine",
                TitleAr = "محرك السياسات",
                Description = "YAML-based policy rules for automated compliance checking and enforcement.",
                DescriptionAr = "قواعد سياسات YAML للتحقق الآلي من الامتثال وتطبيقه.",
                IconClass = "bi-code-slash",
                Category = "Advanced",
                DisplayOrder = 5
            },
            new()
            {
                Title = "AI Agents",
                TitleAr = "الوكلاء الذكية",
                Description = "9 specialized Claude AI agents for compliance analysis, risk assessment, and reporting.",
                DescriptionAr = "9 وكلاء ذكاء اصطناعي متخصصين لتحليل الامتثال وتقييم المخاطر والتقارير.",
                IconClass = "bi-robot",
                Category = "AI",
                DisplayOrder = 6,
                IsFeatured = true
            },
            new()
            {
                Title = "Workflow Automation",
                TitleAr = "أتمتة سير العمل",
                Description = "Customizable approval workflows, escalations, and notifications.",
                DescriptionAr = "سير عمل الموافقات القابل للتخصيص، التصعيدات، والإشعارات.",
                IconClass = "bi-diagram-3",
                Category = "Advanced",
                DisplayOrder = 7
            },
            new()
            {
                Title = "Multi-tenant Platform",
                TitleAr = "منصة متعددة المستأجرين",
                Description = "Secure isolation for MSPs managing multiple client organizations.",
                DescriptionAr = "عزل آمن لمقدمي الخدمات المُدارة الذين يديرون مؤسسات عملاء متعددة.",
                IconClass = "bi-people",
                Category = "Advanced",
                DisplayOrder = 8
            }
        };

        context.FeatureHighlights.AddRange(features);
        await context.SaveChangesAsync();
    }

    private static async Task SeedPartnersAsync(GrcDbContext context)
    {
        if (await context.Partners.AnyAsync())
            return;

        var partners = new List<Partner>
        {
            new()
            {
                Name = "Microsoft Azure",
                NameAr = "مايكروسوفت أزور",
                Description = "Cloud infrastructure partner",
                DescriptionAr = "شريك البنية التحتية السحابية",
                LogoUrl = "/images/partners/azure.svg",
                Type = "Technology",
                Tier = "Platinum",
                DisplayOrder = 1,
                IsFeatured = true
            },
            new()
            {
                Name = "Anthropic",
                NameAr = "أنثروبيك",
                Description = "AI technology partner - Claude AI",
                DescriptionAr = "شريك تقنية الذكاء الاصطناعي - Claude AI",
                LogoUrl = "/images/partners/anthropic.svg",
                Type = "Technology",
                Tier = "Platinum",
                DisplayOrder = 2,
                IsFeatured = true
            },
            new()
            {
                Name = "Big 4 Consulting",
                NameAr = "استشارات Big 4",
                Description = "Implementation and audit partners",
                DescriptionAr = "شركاء التنفيذ والتدقيق",
                LogoUrl = "/images/partners/consulting.svg",
                Type = "Consulting",
                Tier = "Gold",
                DisplayOrder = 3
            },
            new()
            {
                Name = "ServiceNow",
                NameAr = "سيرفيس ناو",
                Description = "ITSM integration partner",
                DescriptionAr = "شريك تكامل إدارة خدمات تقنية المعلومات",
                LogoUrl = "/images/partners/servicenow.svg",
                Type = "Integration",
                Tier = "Gold",
                DisplayOrder = 4
            },
            new()
            {
                Name = "Splunk",
                NameAr = "سبلانك",
                Description = "SIEM integration partner",
                DescriptionAr = "شريك تكامل SIEM",
                LogoUrl = "/images/partners/splunk.svg",
                Type = "Integration",
                Tier = "Silver",
                DisplayOrder = 5
            }
        };

        context.Partners.AddRange(partners);
        await context.SaveChangesAsync();
    }
}
