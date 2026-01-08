using System.ComponentModel.DataAnnotations;

namespace GrcMvc.Models.Entities.Marketing;

// =============================================================================
// ENTERPRISE MARKETING CMS ENTITIES
// =============================================================================

#region Testimonials & Social Proof

/// <summary>
/// Customer testimonial for landing pages
/// شهادة عميل للصفحات التسويقية
/// </summary>
public class Testimonial
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(1000)]
    public string Quote { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? QuoteAr { get; set; }

    [Required]
    [StringLength(100)]
    public string AuthorName { get; set; } = string.Empty;

    [StringLength(100)]
    public string? AuthorNameAr { get; set; }

    [Required]
    [StringLength(100)]
    public string AuthorTitle { get; set; } = string.Empty;

    [StringLength(100)]
    public string? AuthorTitleAr { get; set; }

    [Required]
    [StringLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    [StringLength(200)]
    public string? CompanyNameAr { get; set; }

    [StringLength(100)]
    public string? Industry { get; set; }

    [StringLength(100)]
    public string? IndustryAr { get; set; }

    public int DisplayOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public bool IsFeatured { get; set; } = false;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Client logo for social proof section
/// شعار العميل لقسم الثقة
/// </summary>
public class ClientLogo
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(100)]
    public string ClientName { get; set; } = string.Empty;

    [StringLength(100)]
    public string? ClientNameAr { get; set; }

    /// <summary>
    /// URL to client logo image (SVG preferred)
    /// </summary>
    [Required]
    [StringLength(500)]
    public string LogoUrl { get; set; } = string.Empty;

    /// <summary>
    /// Optional link to client website
    /// </summary>
    [StringLength(500)]
    public string? WebsiteUrl { get; set; }

    [StringLength(100)]
    public string? Industry { get; set; }

    [StringLength(100)]
    public string? IndustryAr { get; set; }

    /// <summary>
    /// Category: Enterprise, Government, SMB, Startup
    /// </summary>
    [StringLength(50)]
    public string Category { get; set; } = "Enterprise";

    public int DisplayOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public bool IsFeatured { get; set; } = false;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}

#endregion

#region Case Studies

/// <summary>
/// Case study for success stories
/// دراسة حالة لقصص النجاح
/// </summary>
public class CaseStudy
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(200)]
    public string? TitleAr { get; set; }

    [StringLength(500)]
    public string? Slug { get; set; }

    [Required]
    [StringLength(2000)]
    public string Summary { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? SummaryAr { get; set; }

    public string? FullContent { get; set; }

    public string? FullContentAr { get; set; }

    [Required]
    [StringLength(100)]
    public string Industry { get; set; } = string.Empty;

    [StringLength(100)]
    public string? IndustryAr { get; set; }

    [StringLength(200)]
    public string? CompanyName { get; set; }

    [StringLength(200)]
    public string? CompanyNameAr { get; set; }

    /// <summary>
    /// Primary framework achieved (e.g., "NCA-ECC", "SAMA CSF", "PDPL")
    /// </summary>
    [StringLength(50)]
    public string? FrameworkCode { get; set; }

    /// <summary>
    /// Time to achieve compliance (e.g., "3 months")
    /// </summary>
    [StringLength(50)]
    public string? TimeToCompliance { get; set; }

    /// <summary>
    /// Percentage improvement or time saved (e.g., "70%")
    /// </summary>
    [StringLength(20)]
    public string? ImprovementMetric { get; set; }

    /// <summary>
    /// What the improvement metric represents
    /// </summary>
    [StringLength(100)]
    public string? ImprovementLabel { get; set; }

    [StringLength(100)]
    public string? ImprovementLabelAr { get; set; }

    /// <summary>
    /// Compliance score achieved (e.g., "100%")
    /// </summary>
    [StringLength(20)]
    public string? ComplianceScore { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    public int DisplayOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public bool IsFeatured { get; set; } = false;

    public DateTime PublishDate { get; set; } = DateTime.UtcNow;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}

#endregion

#region Pricing & Plans

/// <summary>
/// Pricing plan configuration
/// إعدادات خطة الأسعار
/// </summary>
public class PricingPlan
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [StringLength(50)]
    public string? NameAr { get; set; }

    [StringLength(200)]
    public string? Description { get; set; }

    [StringLength(200)]
    public string? DescriptionAr { get; set; }

    /// <summary>
    /// Monthly price in SAR. -1 = Contact Us
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Billing period (e.g., "month", "year", "custom")
    /// </summary>
    [StringLength(20)]
    public string Period { get; set; } = "month";

    /// <summary>
    /// JSON array of feature strings
    /// </summary>
    public string? FeaturesJson { get; set; }

    /// <summary>
    /// JSON array of feature strings in Arabic
    /// </summary>
    public string? FeaturesJsonAr { get; set; }

    public int MaxUsers { get; set; } = 3;

    public int MaxWorkspaces { get; set; } = 1;

    public int MaxFrameworks { get; set; } = 5;

    public bool HasApiAccess { get; set; } = false;

    public bool HasPrioritySupport { get; set; } = false;

    public bool IsPopular { get; set; } = false;

    public bool IsActive { get; set; } = true;

    public int DisplayOrder { get; set; } = 0;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}

#endregion

#region Trust & Security Badges

/// <summary>
/// Trust/Security badge for credibility section
/// شارة الثقة والأمان
/// </summary>
public class TrustBadge
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    public string? NameAr { get; set; }

    [StringLength(200)]
    public string? Description { get; set; }

    [StringLength(200)]
    public string? DescriptionAr { get; set; }

    /// <summary>
    /// URL to badge image (SVG preferred)
    /// </summary>
    [Required]
    [StringLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// Optional link to verification/certificate
    /// </summary>
    [StringLength(500)]
    public string? VerificationUrl { get; set; }

    /// <summary>
    /// Category: Certification, Compliance, Security, Privacy, Industry
    /// </summary>
    [StringLength(50)]
    public string Category { get; set; } = "Certification";

    /// <summary>
    /// Certificate/badge code (e.g., "ISO27001", "SOC2", "SAMA-CSF")
    /// </summary>
    [StringLength(50)]
    public string? BadgeCode { get; set; }

    public int DisplayOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}

#endregion

#region Blog & Content

/// <summary>
/// Blog post for content marketing
/// مقال للتسويق بالمحتوى
/// </summary>
public class BlogPost
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(200)]
    public string? TitleAr { get; set; }

    [Required]
    [StringLength(300)]
    public string Slug { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Excerpt { get; set; }

    [StringLength(500)]
    public string? ExcerptAr { get; set; }

    /// <summary>
    /// Full HTML content of the post
    /// </summary>
    public string? Content { get; set; }

    public string? ContentAr { get; set; }

    /// <summary>
    /// Featured image URL
    /// </summary>
    [StringLength(500)]
    public string? FeaturedImageUrl { get; set; }

    /// <summary>
    /// Author name
    /// </summary>
    [StringLength(100)]
    public string? Author { get; set; }

    [StringLength(100)]
    public string? AuthorAr { get; set; }

    /// <summary>
    /// Author avatar URL
    /// </summary>
    [StringLength(500)]
    public string? AuthorAvatarUrl { get; set; }

    /// <summary>
    /// Category: Compliance, Risk, Audit, Technology, Industry, News
    /// </summary>
    [StringLength(50)]
    public string Category { get; set; } = "Compliance";

    /// <summary>
    /// JSON array of tags
    /// </summary>
    public string? TagsJson { get; set; }

    /// <summary>
    /// Estimated reading time in minutes
    /// </summary>
    public int ReadTimeMinutes { get; set; } = 5;

    /// <summary>
    /// Status: Draft, Published, Archived
    /// </summary>
    [StringLength(20)]
    public string Status { get; set; } = "Draft";

    public DateTime? PublishDate { get; set; }

    public DateTime? ScheduledPublishDate { get; set; }

    public int ViewCount { get; set; } = 0;

    public bool IsFeatured { get; set; } = false;

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// SEO meta title
    /// </summary>
    [StringLength(70)]
    public string? MetaTitle { get; set; }

    /// <summary>
    /// SEO meta description
    /// </summary>
    [StringLength(160)]
    public string? MetaDescription { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime? ModifiedDate { get; set; }
}

#endregion

#region FAQ

/// <summary>
/// Frequently asked question
/// الأسئلة المتكررة
/// </summary>
public class Faq
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(500)]
    public string Question { get; set; } = string.Empty;

    [StringLength(500)]
    public string? QuestionAr { get; set; }

    [Required]
    [StringLength(2000)]
    public string Answer { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? AnswerAr { get; set; }

    /// <summary>
    /// Category: General, Pricing, Technical, Compliance, Security, Support
    /// </summary>
    [StringLength(50)]
    public string Category { get; set; } = "General";

    public int DisplayOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}

#endregion

#region Team

/// <summary>
/// Marketing team member for About Us page
/// عضو الفريق لصفحة من نحن (للتسويق)
/// Note: Named MarketingTeamMember to avoid conflict with TeamMember in TeamEntities
/// </summary>
public class MarketingTeamMember
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    public string? NameAr { get; set; }

    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [StringLength(100)]
    public string? TitleAr { get; set; }

    [StringLength(500)]
    public string? Bio { get; set; }

    [StringLength(500)]
    public string? BioAr { get; set; }

    [StringLength(500)]
    public string? PhotoUrl { get; set; }

    [StringLength(200)]
    [EmailAddress]
    public string? Email { get; set; }

    [StringLength(200)]
    public string? LinkedInUrl { get; set; }

    [StringLength(200)]
    public string? TwitterUrl { get; set; }

    /// <summary>
    /// Department: Leadership, Engineering, Sales, Marketing, Support
    /// </summary>
    [StringLength(50)]
    public string Department { get; set; } = "Leadership";

    public int DisplayOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public bool IsFeatured { get; set; } = false;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}

#endregion

#region Webinars & Events

/// <summary>
/// Webinar/Event for lead generation
/// ندوة عبر الإنترنت
/// </summary>
public class Webinar
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(200)]
    public string? TitleAr { get; set; }

    [StringLength(300)]
    public string? Slug { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    [StringLength(1000)]
    public string? DescriptionAr { get; set; }

    /// <summary>
    /// Featured image or thumbnail
    /// </summary>
    [StringLength(500)]
    public string? ThumbnailUrl { get; set; }

    /// <summary>
    /// Video URL (YouTube, Vimeo, etc.)
    /// </summary>
    [StringLength(500)]
    public string? VideoUrl { get; set; }

    /// <summary>
    /// Registration page URL
    /// </summary>
    [StringLength(500)]
    public string? RegistrationUrl { get; set; }

    /// <summary>
    /// Duration in minutes
    /// </summary>
    public int DurationMinutes { get; set; } = 60;

    /// <summary>
    /// Speaker names (JSON array)
    /// </summary>
    public string? SpeakersJson { get; set; }

    /// <summary>
    /// Topics covered (JSON array)
    /// </summary>
    public string? TopicsJson { get; set; }

    /// <summary>
    /// Type: Live, OnDemand, Upcoming
    /// </summary>
    [StringLength(20)]
    public string Type { get; set; } = "Upcoming";

    public DateTime? ScheduledDate { get; set; }

    public int RegistrationCount { get; set; } = 0;

    public int ViewCount { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public bool IsFeatured { get; set; } = false;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}

#endregion

#region Landing Page Content

/// <summary>
/// Dynamic landing page content blocks
/// محتوى صفحات الهبوط الديناميكي
/// </summary>
public class LandingPageContent
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Page identifier: home, features, pricing, about, contact
    /// </summary>
    [Required]
    [StringLength(50)]
    public string PageKey { get; set; } = "home";

    /// <summary>
    /// Section identifier within the page: hero, features, stats, cta
    /// </summary>
    [Required]
    [StringLength(50)]
    public string SectionKey { get; set; } = string.Empty;

    /// <summary>
    /// Content key for specific element: title, subtitle, description, button_text
    /// </summary>
    [Required]
    [StringLength(50)]
    public string ContentKey { get; set; } = string.Empty;

    /// <summary>
    /// English content value
    /// </summary>
    public string? ContentValue { get; set; }

    /// <summary>
    /// Arabic content value
    /// </summary>
    public string? ContentValueAr { get; set; }

    /// <summary>
    /// Content type: text, html, image, video, json
    /// </summary>
    [StringLength(20)]
    public string ContentType { get; set; } = "text";

    public int DisplayOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime? ModifiedDate { get; set; }
}

/// <summary>
/// Statistics/metrics for landing page
/// إحصائيات صفحة الهبوط
/// </summary>
public class LandingStatistic
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(100)]
    public string Label { get; set; } = string.Empty;

    [StringLength(100)]
    public string? LabelAr { get; set; }

    /// <summary>
    /// The value to display (e.g., "500+", "99.9%", "24/7")
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Optional suffix (e.g., "%", "+", "K")
    /// </summary>
    [StringLength(10)]
    public string? Suffix { get; set; }

    /// <summary>
    /// Icon class (e.g., "bi-people", "fas fa-shield")
    /// </summary>
    [StringLength(50)]
    public string? IconClass { get; set; }

    /// <summary>
    /// Category: Platform, Security, Compliance, Support
    /// </summary>
    [StringLength(50)]
    public string Category { get; set; } = "Platform";

    public int DisplayOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Feature highlight for landing page
/// ميزة مميزة لصفحة الهبوط
/// </summary>
public class FeatureHighlight
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [StringLength(100)]
    public string? TitleAr { get; set; }

    [StringLength(300)]
    public string? Description { get; set; }

    [StringLength(300)]
    public string? DescriptionAr { get; set; }

    /// <summary>
    /// Icon class (Bootstrap Icons or Font Awesome)
    /// </summary>
    [StringLength(50)]
    public string? IconClass { get; set; }

    /// <summary>
    /// Optional image URL
    /// </summary>
    [StringLength(500)]
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Link to feature details page
    /// </summary>
    [StringLength(300)]
    public string? LearnMoreUrl { get; set; }

    /// <summary>
    /// Category: Core, Advanced, Integration, AI
    /// </summary>
    [StringLength(50)]
    public string Category { get; set; } = "Core";

    public int DisplayOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public bool IsFeatured { get; set; } = false;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}

#endregion

#region Partners & Integrations

/// <summary>
/// Partner/Integration for ecosystem section
/// شريك/تكامل
/// </summary>
public class Partner
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    public string? NameAr { get; set; }

    [StringLength(300)]
    public string? Description { get; set; }

    [StringLength(300)]
    public string? DescriptionAr { get; set; }

    [Required]
    [StringLength(500)]
    public string LogoUrl { get; set; } = string.Empty;

    [StringLength(500)]
    public string? WebsiteUrl { get; set; }

    /// <summary>
    /// Type: Technology, Consulting, Reseller, Integration
    /// </summary>
    [StringLength(50)]
    public string Type { get; set; } = "Technology";

    /// <summary>
    /// Tier: Platinum, Gold, Silver, Standard
    /// </summary>
    [StringLength(20)]
    public string Tier { get; set; } = "Standard";

    public int DisplayOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public bool IsFeatured { get; set; } = false;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}

#endregion
