using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities;

#region Shahin-AI Brand Configuration

/// <summary>
/// Shahin-AI Brand Configuration
/// Umbrella brand: Shahin-AI
/// Tagline: One control map for all markets / خريطة تحكم واحدة لكل الأسواق
/// </summary>
public class ShahinAIBrandConfig : BaseEntity
{
    public Guid? TenantId { get; set; }

    [ForeignKey("TenantId")]
    public virtual Tenant? Tenant { get; set; }

    [Required]
    [StringLength(50)]
    public string BrandCode { get; set; } = "SHAHIN-AI";

    [Required]
    [StringLength(255)]
    public string BrandName { get; set; } = "Shahin-AI";

    [StringLength(255)]
    public string BrandNameAr { get; set; } = "شاهين-إيه آي";

    [Required]
    [StringLength(500)]
    public string Tagline { get; set; } = "One control map for all markets.";

    [StringLength(500)]
    public string TaglineAr { get; set; } = "خريطة تحكم واحدة لكل الأسواق";

    /// <summary>
    /// Operating sentence (memorable and trainable)
    /// </summary>
    [StringLength(500)]
    public string OperatingSentence { get; set; } = "MAP it, APPLY it, PROVE it—WATCH exceptions and FIX gaps; store in VAULT.";

    [StringLength(500)]
    public string OperatingSentenceAr { get; set; } = "اربطها، طبّقها، أثبتها—راقب الاستثناءات وعالج الفجوات؛ واحفظ كل شيء في الخزنة.";

    /// <summary>
    /// Primary color (hex)
    /// </summary>
    [StringLength(10)]
    public string PrimaryColor { get; set; } = "#1E3A5F";

    /// <summary>
    /// Secondary color (hex)
    /// </summary>
    [StringLength(10)]
    public string SecondaryColor { get; set; } = "#4A90A4";

    /// <summary>
    /// Accent color (hex)
    /// </summary>
    [StringLength(10)]
    public string AccentColor { get; set; } = "#F5A623";

    /// <summary>
    /// Logo URL
    /// </summary>
    [StringLength(500)]
    public string? LogoUrl { get; set; }

    /// <summary>
    /// Logo URL (dark mode)
    /// </summary>
    [StringLength(500)]
    public string? LogoDarkUrl { get; set; }

    /// <summary>
    /// Favicon URL
    /// </summary>
    [StringLength(500)]
    public string? FaviconUrl { get; set; }

    /// <summary>
    /// Public website URL
    /// </summary>
    [StringLength(255)]
    public string PublicWebsiteUrl { get; set; } = "https://www.shahin-ai.com";

    /// <summary>
    /// App URL
    /// </summary>
    [StringLength(255)]
    public string AppUrl { get; set; } = "https://app.shahin-ai.com";

    /// <summary>
    /// Support email
    /// </summary>
    [StringLength(255)]
    public string SupportEmail { get; set; } = "support@shahin-ai.com";

    /// <summary>
    /// Default language: en, ar
    /// </summary>
    [StringLength(5)]
    public string DefaultLanguage { get; set; } = "en";

    /// <summary>
    /// RTL support enabled
    /// </summary>
    public bool RTLEnabled { get; set; } = true;

    public bool IsActive { get; set; } = true;
}

#endregion

#region Shahin-AI Modules

/// <summary>
/// Shahin-AI Module Definition
/// 6 Core Modules: MAP, APPLY, PROVE, WATCH, FIX, VAULT
/// </summary>
public class ShahinAIModule : BaseEntity
{
    [Required]
    [StringLength(20)]
    public string ModuleCode { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string NameAr { get; set; } = string.Empty;

    /// <summary>
    /// Short description (one line)
    /// </summary>
    [Required]
    [StringLength(255)]
    public string ShortDescription { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string ShortDescriptionAr { get; set; } = string.Empty;

    /// <summary>
    /// Full description
    /// </summary>
    [StringLength(1000)]
    public string? FullDescription { get; set; }

    [StringLength(1000)]
    public string? FullDescriptionAr { get; set; }

    /// <summary>
    /// Outcome statement
    /// </summary>
    [StringLength(500)]
    public string? Outcome { get; set; }

    [StringLength(500)]
    public string? OutcomeAr { get; set; }

    /// <summary>
    /// Icon class (e.g., fa-map, fa-check, etc.)
    /// </summary>
    [StringLength(50)]
    public string? IconClass { get; set; }

    /// <summary>
    /// Module color (hex)
    /// </summary>
    [StringLength(10)]
    public string? ModuleColor { get; set; }

    /// <summary>
    /// Route/URL path for this module
    /// </summary>
    [StringLength(100)]
    public string? RoutePath { get; set; }

    /// <summary>
    /// Display order
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Is this module enabled?
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Requires license?
    /// </summary>
    public bool RequiresLicense { get; set; } = false;
}

/// <summary>
/// Standard Shahin-AI Modules (Seed Data)
/// </summary>
public static class StandardShahinModules
{
    public static readonly List<ShahinModuleInfo> Modules = new()
    {
        new ShahinModuleInfo
        {
            Code = "MAP",
            Name = "Shahin MAP",
            NameAr = "شاهين خريطة",
            ShortDescription = "Requirement → Objective → Control mapping",
            ShortDescriptionAr = "ربط المتطلبات → الأهداف → الضوابط",
            FullDescription = "A single canonical control library that supports multiple regulators and frameworks without rework.",
            FullDescriptionAr = "نوحّد المتطلبات ضمن أهداف وضوابط قياسية قابلة لإعادة الاستخدام.",
            Outcome = "Single canonical control library supporting multiple regulators and frameworks.",
            OutcomeAr = "مكتبة ضوابط موحدة تدعم عدة جهات تنظيمية وأطر عمل.",
            IconClass = "fa-map",
            ModuleColor = "#3498DB",
            DisplayOrder = 1
        },
        new ShahinModuleInfo
        {
            Code = "APPLY",
            Name = "Shahin APPLY",
            NameAr = "شاهين تطبيق",
            ShortDescription = "Scope and applicability rules (market/sector/system)",
            ShortDescriptionAr = "قواعد النطاق والانطباق حسب السوق/القطاع/النظام",
            FullDescription = "Clear 'what applies' decisions using consistent logic (Applicable / Not applicable / Inherited / Exception).",
            FullDescriptionAr = "نحدد ما ينطبق بدقة حسب السوق والقطاع والنظام، مع منطق واضح للوراثة والاستثناءات.",
            Outcome = "Clear applicability decisions with consistent logic across all markets.",
            OutcomeAr = "قرارات انطباق واضحة بمنطق متسق عبر جميع الأسواق.",
            IconClass = "fa-check-circle",
            ModuleColor = "#27AE60",
            DisplayOrder = 2
        },
        new ShahinModuleInfo
        {
            Code = "PROVE",
            Name = "Shahin PROVE",
            NameAr = "شاهين إثبات",
            ShortDescription = "Evidence packs, testing, audit-ready packages",
            ShortDescriptionAr = "حزم الأدلة والاختبارات وتجهيز ملفات التدقيق",
            FullDescription = "Standardized evidence collection and testing that can be reused across audits and frameworks.",
            FullDescriptionAr = "ننتج حزم أدلة واختبارات جاهزة للتدقيق بدون جمع يدوي مرهق.",
            Outcome = "Standardized evidence collection reusable across all audits.",
            OutcomeAr = "جمع أدلة موحد قابل لإعادة الاستخدام عبر جميع عمليات التدقيق.",
            IconClass = "fa-file-alt",
            ModuleColor = "#9B59B6",
            DisplayOrder = 3
        },
        new ShahinModuleInfo
        {
            Code = "WATCH",
            Name = "Shahin WATCH",
            NameAr = "شاهين مراقبة",
            ShortDescription = "Continuous monitoring (KRIs/KPIs), alerts",
            ShortDescriptionAr = "مراقبة مستمرة للمؤشرات والتنبيهات",
            FullDescription = "Always-on visibility into control health, breaches, and emerging risks.",
            FullDescriptionAr = "نراقب صحة الضوابط ومؤشرات المخاطر بشكل مستمر ونرسل تنبيهات قابلة للتنفيذ.",
            Outcome = "Always-on visibility into control health and emerging risks.",
            OutcomeAr = "رؤية مستمرة لصحة الضوابط والمخاطر الناشئة.",
            IconClass = "fa-eye",
            ModuleColor = "#E74C3C",
            DisplayOrder = 4
        },
        new ShahinModuleInfo
        {
            Code = "FIX",
            Name = "Shahin FIX",
            NameAr = "شاهين معالجة",
            ShortDescription = "Remediation workflow (tickets, SLAs, exceptions)",
            ShortDescriptionAr = "سير عمل المعالجات والتذاكر والاستثناءات",
            FullDescription = "Structured issue management with accountability, approvals, escalation, and closure.",
            FullDescriptionAr = "نربط الفجوات بتذاكر معالجة وقياس SLA وتصعيدات واضحة.",
            Outcome = "Structured issue management with clear accountability and SLAs.",
            OutcomeAr = "إدارة مشكلات منظمة مع مساءلة واضحة واتفاقيات مستوى الخدمة.",
            IconClass = "fa-wrench",
            ModuleColor = "#F39C12",
            DisplayOrder = 5
        },
        new ShahinModuleInfo
        {
            Code = "VAULT",
            Name = "Shahin VAULT",
            NameAr = "شاهين خزنة",
            ShortDescription = "Evidence repository with versioning and audit trail",
            ShortDescriptionAr = "مستودع أدلة مع إصدار وتدقيق تغييرات",
            FullDescription = "Central, controlled evidence storage that is searchable, immutable, and audit-defensible.",
            FullDescriptionAr = "نخزن الأدلة بتتبع نسخ وسجل تغييرات يدعم التدقيق والتنظيم.",
            Outcome = "Central evidence storage that is searchable, immutable, and audit-defensible.",
            OutcomeAr = "تخزين أدلة مركزي قابل للبحث وغير قابل للتغيير ويدعم التدقيق.",
            IconClass = "fa-lock",
            ModuleColor = "#1ABC9C",
            DisplayOrder = 6
        }
    };
}

public class ShahinModuleInfo
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string ShortDescriptionAr { get; set; } = string.Empty;
    public string FullDescription { get; set; } = string.Empty;
    public string FullDescriptionAr { get; set; } = string.Empty;
    public string Outcome { get; set; } = string.Empty;
    public string OutcomeAr { get; set; } = string.Empty;
    public string IconClass { get; set; } = string.Empty;
    public string ModuleColor { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

#endregion

#region UI Text / Localization

/// <summary>
/// UI Text Entry - Branded text for all UI elements
/// Supports English and Arabic
/// </summary>
public class UITextEntry : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string TextKey { get; set; } = string.Empty;

    /// <summary>
    /// Category: Login, Error, Navigation, Dashboard, Module, Common
    /// </summary>
    [Required]
    [StringLength(30)]
    public string Category { get; set; } = "Common";

    [Required]
    [StringLength(1000)]
    public string TextEn { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string TextAr { get; set; } = string.Empty;

    /// <summary>
    /// Context/usage notes
    /// </summary>
    [StringLength(500)]
    public string? UsageNotes { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Standard UI Text (Seed Data)
/// </summary>
public static class StandardUIText
{
    public static readonly List<UITextInfo> Texts = new()
    {
        // Login Page
        new UITextInfo { Key = "login.title", Category = "Login", En = "Sign in to Shahin-AI", Ar = "تسجيل الدخول إلى Shahin-AI" },
        new UITextInfo { Key = "login.helper", Category = "Login", En = "Please sign in to continue.", Ar = "يرجى تسجيل الدخول للمتابعة." },
        new UITextInfo { Key = "login.sso", Category = "Login", En = "Sign in with SSO", Ar = "تسجيل الدخول عبر SSO" },
        new UITextInfo { Key = "login.email", Category = "Login", En = "Sign in with Email", Ar = "تسجيل الدخول بالبريد الإلكتروني" },
        new UITextInfo { Key = "login.help", Category = "Login", En = "Need help? Contact support", Ar = "هل تحتاج مساعدة؟ تواصل مع الدعم" },

        // Error Pages
        new UITextInfo { Key = "error.403.title", Category = "Error", En = "You don't have access", Ar = "لا تملك صلاحية الوصول" },
        new UITextInfo { Key = "error.403.message", Category = "Error", En = "You may need permission or an account. Please sign in or contact your admin.", Ar = "قد تكون بحاجة إلى صلاحية أو حساب. جرّب تسجيل الدخول أو تواصل مع مسؤول النظام." },
        new UITextInfo { Key = "error.404.title", Category = "Error", En = "Page not found", Ar = "الصفحة غير موجودة" },
        new UITextInfo { Key = "error.404.message", Category = "Error", En = "The page you're looking for doesn't exist.", Ar = "الصفحة التي تبحث عنها غير موجودة." },
        new UITextInfo { Key = "error.415.title", Category = "Error", En = "Request not supported", Ar = "تعذّر معالجة الطلب" },
        new UITextInfo { Key = "error.415.message", Category = "Error", En = "The request format is not supported. Please try again or contact support.", Ar = "تنسيق البيانات المرسل غير مدعوم. يرجى المحاولة مرة أخرى أو التواصل مع الدعم." },
        new UITextInfo { Key = "error.500.title", Category = "Error", En = "Something went wrong", Ar = "حدث خطأ ما" },
        new UITextInfo { Key = "error.500.message", Category = "Error", En = "We're working on it. Please try again later.", Ar = "نحن نعمل على حل المشكلة. يرجى المحاولة لاحقاً." },

        // Navigation
        new UITextInfo { Key = "nav.dashboard", Category = "Navigation", En = "Dashboard", Ar = "لوحة التحكم" },
        new UITextInfo { Key = "nav.controls", Category = "Navigation", En = "Controls", Ar = "الضوابط" },
        new UITextInfo { Key = "nav.assessments", Category = "Navigation", En = "Assessments", Ar = "التقييمات" },
        new UITextInfo { Key = "nav.evidence", Category = "Navigation", En = "Evidence", Ar = "الأدلة" },
        new UITextInfo { Key = "nav.reports", Category = "Navigation", En = "Reports", Ar = "التقارير" },
        new UITextInfo { Key = "nav.settings", Category = "Navigation", En = "Settings", Ar = "الإعدادات" },
        new UITextInfo { Key = "nav.help", Category = "Navigation", En = "Help", Ar = "المساعدة" },
        new UITextInfo { Key = "nav.signout", Category = "Navigation", En = "Sign out", Ar = "تسجيل الخروج" },

        // Common
        new UITextInfo { Key = "common.save", Category = "Common", En = "Save", Ar = "حفظ" },
        new UITextInfo { Key = "common.cancel", Category = "Common", En = "Cancel", Ar = "إلغاء" },
        new UITextInfo { Key = "common.submit", Category = "Common", En = "Submit", Ar = "إرسال" },
        new UITextInfo { Key = "common.delete", Category = "Common", En = "Delete", Ar = "حذف" },
        new UITextInfo { Key = "common.edit", Category = "Common", En = "Edit", Ar = "تعديل" },
        new UITextInfo { Key = "common.view", Category = "Common", En = "View", Ar = "عرض" },
        new UITextInfo { Key = "common.search", Category = "Common", En = "Search", Ar = "بحث" },
        new UITextInfo { Key = "common.filter", Category = "Common", En = "Filter", Ar = "تصفية" },
        new UITextInfo { Key = "common.export", Category = "Common", En = "Export", Ar = "تصدير" },
        new UITextInfo { Key = "common.import", Category = "Common", En = "Import", Ar = "استيراد" },
        new UITextInfo { Key = "common.loading", Category = "Common", En = "Loading...", Ar = "جاري التحميل..." },
        new UITextInfo { Key = "common.nodata", Category = "Common", En = "No data available", Ar = "لا توجد بيانات" },

        // Dashboard
        new UITextInfo { Key = "dashboard.welcome", Category = "Dashboard", En = "Welcome to Shahin-AI", Ar = "مرحباً بك في شاهين-إيه آي" },
        new UITextInfo { Key = "dashboard.overview", Category = "Dashboard", En = "Compliance Overview", Ar = "نظرة عامة على الامتثال" },
        new UITextInfo { Key = "dashboard.tasks", Category = "Dashboard", En = "My Tasks", Ar = "مهامي" },
        new UITextInfo { Key = "dashboard.alerts", Category = "Dashboard", En = "Alerts", Ar = "التنبيهات" }
    };
}

public class UITextInfo
{
    public string Key { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string En { get; set; } = string.Empty;
    public string Ar { get; set; } = string.Empty;
}

#endregion

#region Site Map / Page Structure

/// <summary>
/// Site Map Entry - Defines page structure for public and app sites
/// </summary>
public class SiteMapEntry : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string PageCode { get; set; } = string.Empty;

    /// <summary>
    /// Site type: Public, App
    /// </summary>
    [Required]
    [StringLength(10)]
    public string SiteType { get; set; } = "Public";

    [Required]
    [StringLength(255)]
    public string TitleEn { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string TitleAr { get; set; } = string.Empty;

    /// <summary>
    /// URL path
    /// </summary>
    [Required]
    [StringLength(255)]
    public string UrlPath { get; set; } = string.Empty;

    /// <summary>
    /// Parent page code (for hierarchy)
    /// </summary>
    [StringLength(50)]
    public string? ParentPageCode { get; set; }

    /// <summary>
    /// Meta description (SEO)
    /// </summary>
    [StringLength(500)]
    public string? MetaDescriptionEn { get; set; }

    [StringLength(500)]
    public string? MetaDescriptionAr { get; set; }

    /// <summary>
    /// Show in navigation?
    /// </summary>
    public bool ShowInNav { get; set; } = true;

    /// <summary>
    /// Requires authentication?
    /// </summary>
    public bool RequiresAuth { get; set; } = false;

    /// <summary>
    /// Icon class
    /// </summary>
    [StringLength(50)]
    public string? IconClass { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Standard Site Map (Seed Data)
/// </summary>
public static class StandardSiteMap
{
    public static readonly List<SiteMapInfo> Pages = new()
    {
        // Public Site
        new SiteMapInfo { Code = "home", Site = "Public", TitleEn = "Shahin-AI: One control map for all markets", TitleAr = "شاهين-إيه آي: خريطة تحكم واحدة لكل الأسواق", Path = "/", Order = 1 },
        new SiteMapInfo { Code = "product", Site = "Public", TitleEn = "Product", TitleAr = "المنتج", Path = "/product", Order = 2 },
        new SiteMapInfo { Code = "modules", Site = "Public", TitleEn = "Modules", TitleAr = "الوحدات", Path = "/modules", Order = 3 },
        new SiteMapInfo { Code = "how-it-works", Site = "Public", TitleEn = "How it works", TitleAr = "كيف يعمل", Path = "/how-it-works", Order = 4 },
        new SiteMapInfo { Code = "industries", Site = "Public", TitleEn = "Industries", TitleAr = "القطاعات", Path = "/industries", Order = 5 },
        new SiteMapInfo { Code = "compliance", Site = "Public", TitleEn = "Compliance & Audit", TitleAr = "الامتثال والتدقيق", Path = "/compliance", Order = 6 },
        new SiteMapInfo { Code = "security", Site = "Public", TitleEn = "Security", TitleAr = "الأمان", Path = "/security", Order = 7 },
        new SiteMapInfo { Code = "resources", Site = "Public", TitleEn = "Resources", TitleAr = "الموارد", Path = "/resources", Order = 8 },
        new SiteMapInfo { Code = "contact", Site = "Public", TitleEn = "Contact", TitleAr = "تواصل معنا", Path = "/contact", Order = 9 },
        new SiteMapInfo { Code = "signin", Site = "Public", TitleEn = "Sign in", TitleAr = "تسجيل الدخول", Path = "/signin", Order = 10 },

        // App Site
        new SiteMapInfo { Code = "app-dashboard", Site = "App", TitleEn = "Dashboard", TitleAr = "لوحة التحكم", Path = "/dashboard", Order = 1, RequiresAuth = true },
        new SiteMapInfo { Code = "app-map", Site = "App", TitleEn = "MAP - Control Library", TitleAr = "خريطة - مكتبة الضوابط", Path = "/map", Order = 2, RequiresAuth = true },
        new SiteMapInfo { Code = "app-apply", Site = "App", TitleEn = "APPLY - Scope & Applicability", TitleAr = "تطبيق - النطاق والانطباق", Path = "/apply", Order = 3, RequiresAuth = true },
        new SiteMapInfo { Code = "app-prove", Site = "App", TitleEn = "PROVE - Evidence & Testing", TitleAr = "إثبات - الأدلة والاختبارات", Path = "/prove", Order = 4, RequiresAuth = true },
        new SiteMapInfo { Code = "app-watch", Site = "App", TitleEn = "WATCH - Monitoring & Alerts", TitleAr = "مراقبة - المراقبة والتنبيهات", Path = "/watch", Order = 5, RequiresAuth = true },
        new SiteMapInfo { Code = "app-fix", Site = "App", TitleEn = "FIX - Remediation", TitleAr = "معالجة - المعالجات", Path = "/fix", Order = 6, RequiresAuth = true },
        new SiteMapInfo { Code = "app-vault", Site = "App", TitleEn = "VAULT - Evidence Repository", TitleAr = "خزنة - مستودع الأدلة", Path = "/vault", Order = 7, RequiresAuth = true },
        new SiteMapInfo { Code = "app-reports", Site = "App", TitleEn = "Reports", TitleAr = "التقارير", Path = "/reports", Order = 8, RequiresAuth = true },
        new SiteMapInfo { Code = "app-settings", Site = "App", TitleEn = "Settings", TitleAr = "الإعدادات", Path = "/settings", Order = 9, RequiresAuth = true }
    };
}

public class SiteMapInfo
{
    public string Code { get; set; } = string.Empty;
    public string Site { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool RequiresAuth { get; set; } = false;
}

#endregion
