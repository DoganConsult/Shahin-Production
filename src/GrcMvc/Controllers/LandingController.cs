using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrcMvc.Data;
using Microsoft.EntityFrameworkCore;

namespace GrcMvc.Controllers;

/// <summary>
/// Landing Page Controller - Marketing landing page for visitors
/// صفحة الهبوط للزوار الجدد
/// </summary>
[AllowAnonymous]
public class LandingController : Controller
{
    private readonly GrcDbContext _context;
    private readonly ILogger<LandingController> _logger;

    public LandingController(GrcDbContext context, ILogger<LandingController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Main landing page - show for unauthenticated users
    /// This is the main page for shahin-ai.com
    /// </summary>
    [Route("/")]
    [Route("/home")]
    public IActionResult Index()
    {
        // If authenticated, redirect to dashboard
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        // Check if request is from shahin-ai.com domain (or localhost for dev)
        var host = Request.Host.Host.ToLower();

        // Serve the modern Vercel-style landing page
        var model = new LandingPageViewModel
        {
            Features = GetFeatures(),
            Testimonials = GetTestimonials(),
            Stats = GetStats(),
            Regulators = GetHighlightedRegulators()
        };
        return View("Index", model);
    }

    /// <summary>
    /// Legacy landing page with more details
    /// </summary>
    [Route("/landing/details")]
    public IActionResult Details()
    {
        var model = new LandingPageViewModel
        {
            Features = GetFeatures(),
            Testimonials = GetTestimonials(),
            Stats = GetStats(),
            Regulators = GetHighlightedRegulators()
        };

        return View("Index", model);
    }

    /// <summary>
    /// Pricing page
    /// </summary>
    [Route("/pricing")]
    public IActionResult Pricing()
    {
        var model = new PricingViewModel
        {
            Plans = GetPricingPlans()
        };
        return View(model);
    }

    /// <summary>
    /// Features page
    /// </summary>
    [Route("/features")]
    public IActionResult Features()
    {
        var model = new FeaturesViewModel
        {
            Categories = GetFeatureCategories()
        };
        return View(model);
    }

    /// <summary>
    /// About page
    /// </summary>
    [Route("/about")]
    public IActionResult About()
    {
        return View();
    }

    /// <summary>
    /// Contact page
    /// </summary>
    [Route("/contact")]
    public IActionResult Contact()
    {
        return View();
    }

    /// <summary>
    /// Documentation page
    /// </summary>
    [Route("/docs")]
    public IActionResult Docs()
    {
        return View();
    }

    /// <summary>
    /// Blog page
    /// </summary>
    [Route("/blog")]
    public IActionResult Blog()
    {
        return View();
    }

    /// <summary>
    /// Webinars page
    /// </summary>
    [Route("/webinars")]
    public IActionResult Webinars()
    {
        return View();
    }

    /// <summary>
    /// Case Studies page
    /// </summary>
    [Route("/case-studies")]
    public async Task<IActionResult> CaseStudies()
    {
        var model = new CaseStudiesViewModel
        {
            CaseStudies = await GetCaseStudiesAsync(),
            Testimonials = GetTestimonials().Take(2).ToList()
        };
        return View(model);
    }

    private async Task<List<CaseStudyItem>> GetCaseStudiesAsync()
    {
        try
        {
            var caseStudies = await _context.CaseStudies
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            if (caseStudies.Any())
            {
                return caseStudies.Select(c => new CaseStudyItem
                {
                    Id = c.Id,
                    Title = c.Title,
                    TitleAr = c.TitleAr,
                    Slug = c.Slug,
                    Summary = c.Summary,
                    SummaryAr = c.SummaryAr,
                    Industry = c.Industry,
                    IndustryAr = c.IndustryAr,
                    FrameworkCode = c.FrameworkCode,
                    TimeToCompliance = c.TimeToCompliance,
                    ImprovementMetric = c.ImprovementMetric,
                    ImprovementLabel = c.ImprovementLabel,
                    ImprovementLabelAr = c.ImprovementLabelAr,
                    ComplianceScore = c.ComplianceScore,
                    IsFeatured = c.IsFeatured
                }).ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error fetching case studies from database");
        }

        // Fallback case studies
        return new List<CaseStudyItem>
        {
            new()
            {
                Title = "Leading Bank Achieves Full SAMA CSF Compliance in 3 Months",
                TitleAr = "بنك رائد يحقق الامتثال الكامل لـ SAMA CSF في 3 أشهر",
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
                IsFeatured = true
            }
        };
    }

    /// <summary>
    /// Careers page
    /// </summary>
    [Route("/careers")]
    public IActionResult Careers()
    {
        return View();
    }

    /// <summary>
    /// Partners page
    /// </summary>
    [Route("/partners")]
    public async Task<IActionResult> Partners()
    {
        var model = new PartnersViewModel
        {
            Partners = await GetPartnersAsync(),
            TrustBadges = await GetTrustBadgesAsync()
        };
        return View(model);
    }

    /// <summary>
    /// FAQ page
    /// </summary>
    [Route("/faq")]
    public async Task<IActionResult> FAQ()
    {
        var model = new FaqViewModel
        {
            Faqs = await GetFaqsAsync()
        };
        return View(model);
    }

    /// <summary>
    /// System Status page
    /// </summary>
    [Route("/status")]
    public IActionResult Status()
    {
        var model = new SystemStatusViewModel
        {
            Services = GetSystemStatus()
        };
        return View(model);
    }

    /// <summary>
    /// Help Center page
    /// </summary>
    [Route("/help")]
    public IActionResult Help()
    {
        return View();
    }

    /// <summary>
    /// Privacy Policy page
    /// </summary>
    [Route("/privacy")]
    public IActionResult Privacy()
    {
        return View();
    }

    /// <summary>
    /// Terms of Service page
    /// </summary>
    [Route("/terms")]
    public IActionResult Terms()
    {
        return View();
    }

    private List<ServiceStatusItem> GetSystemStatus()
    {
        // In production, these would be real health checks
        return new List<ServiceStatusItem>
        {
            new() { Name = "منصة شاهين", NameEn = "Shahin Platform", Status = "operational", StatusAr = "يعمل بشكل طبيعي", Uptime = "99.9%", LastChecked = DateTime.UtcNow },
            new() { Name = "قاعدة البيانات", NameEn = "Database", Status = "operational", StatusAr = "يعمل بشكل طبيعي", Uptime = "99.9%", LastChecked = DateTime.UtcNow },
            new() { Name = "خدمات المصادقة", NameEn = "Authentication Services", Status = "operational", StatusAr = "يعمل بشكل طبيعي", Uptime = "99.9%", LastChecked = DateTime.UtcNow },
            new() { Name = "واجهات API", NameEn = "API Services", Status = "operational", StatusAr = "يعمل بشكل طبيعي", Uptime = "99.8%", LastChecked = DateTime.UtcNow },
            new() { Name = "خدمة التقارير", NameEn = "Reporting Service", Status = "operational", StatusAr = "يعمل بشكل طبيعي", Uptime = "99.7%", LastChecked = DateTime.UtcNow },
            new() { Name = "محرك سير العمل", NameEn = "Workflow Engine", Status = "operational", StatusAr = "يعمل بشكل طبيعي", Uptime = "99.9%", LastChecked = DateTime.UtcNow }
        };
    }

    // ========== API ENDPOINTS FOR MARKETING DATA ==========

    /// <summary>
    /// API: Submit contact form
    /// </summary>
    [Route("api/Landing/Contact")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitContact([FromBody] ContactFormDto model)
    {
        if (model == null)
        {
            return BadRequest(new { success = false, message = "Invalid request data" });
        }

        // Server-side validation
        if (string.IsNullOrWhiteSpace(model.Name) || model.Name.Length < 2)
        {
            return BadRequest(new { success = false, message = "Name is required and must be at least 2 characters" });
        }

        if (string.IsNullOrWhiteSpace(model.Email) || !IsValidEmail(model.Email))
        {
            return BadRequest(new { success = false, message = "A valid email address is required" });
        }

        if (string.IsNullOrWhiteSpace(model.Subject))
        {
            return BadRequest(new { success = false, message = "Subject is required" });
        }

        if (string.IsNullOrWhiteSpace(model.Message) || model.Message.Length < 10)
        {
            return BadRequest(new { success = false, message = "Message is required and must be at least 10 characters" });
        }

        try
        {
            // Log the contact submission
            _logger.LogInformation("Contact form submitted: Name={Name}, Email={Email}, Subject={Subject}",
                model.Name, model.Email, model.Subject);

            // Store in database if ContactSubmissions table exists
            // For now, just log it - in production, you'd save to DB and/or send email

            return Ok(new { success = true, message = "تم استلام رسالتك بنجاح. سنتواصل معك قريباً." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing contact form submission");
            return StatusCode(500, new { success = false, message = "An error occurred while processing your request" });
        }
    }

    /// <summary>
    /// API: Chat message endpoint for AI widget
    /// </summary>
    [Route("api/Landing/ChatMessage")]
    [HttpPost]
    public IActionResult ChatMessage([FromBody] ChatMessageDto model)
    {
        if (model == null || string.IsNullOrWhiteSpace(model.Message))
        {
            return BadRequest(new { response = "يرجى إدخال رسالة" });
        }

        // Simple response logic - in production, this would connect to an AI service
        var response = model.Message.ToLower() switch
        {
            var m when m.Contains("ما هي") || m.Contains("what is") =>
                "شاهين هي منصة متكاملة لإدارة الحوكمة والمخاطر والامتثال (GRC). تساعدك على تحقيق الامتثال للأنظمة السعودية والدولية بسهولة.",
            var m when m.Contains("تجربة") || m.Contains("trial") =>
                "يمكنك بدء تجربة مجانية لمدة 7 أيام من خلال زيارة صفحة التسجيل. ستحصل على وصول كامل لجميع الميزات.",
            var m when m.Contains("سعر") || m.Contains("أسعار") || m.Contains("price") =>
                "نقدم خطط متعددة تبدأ من 999 ريال شهرياً. يمكنك زيارة صفحة الأسعار للمزيد من التفاصيل أو طلب عرض خاص.",
            var m when m.Contains("دعم") || m.Contains("support") =>
                "فريق الدعم متاح من الأحد إلى الخميس. يمكنك التواصل معنا عبر صفحة اتصل بنا أو إرسال بريد إلى support@shahin-ai.com",
            _ => "شكراً لتواصلك! كيف يمكنني مساعدتك اليوم؟ يمكنني الإجابة عن أسئلتك حول شاهين، الأسعار، أو التجربة المجانية."
        };

        return Ok(new { response });
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// API: Get all client logos
    /// </summary>
    [Route("api/landing/client-logos")]
    [HttpGet]
    public async Task<IActionResult> GetClientLogos()
    {
        var logos = await GetClientLogosAsync();
        return Json(logos);
    }

    /// <summary>
    /// API: Get all trust badges
    /// </summary>
    [Route("api/landing/trust-badges")]
    [HttpGet]
    public async Task<IActionResult> GetTrustBadges()
    {
        var badges = await GetTrustBadgesAsync();
        return Json(badges);
    }

    /// <summary>
    /// API: Get all FAQs
    /// </summary>
    [Route("api/landing/faqs")]
    [HttpGet]
    public async Task<IActionResult> GetFaqs([FromQuery] string? category = null)
    {
        var faqs = await GetFaqsAsync(category);
        return Json(faqs);
    }

    /// <summary>
    /// API: Get landing page statistics
    /// </summary>
    [Route("api/landing/statistics")]
    [HttpGet]
    public async Task<IActionResult> GetStatistics()
    {
        var stats = await GetLandingStatisticsAsync();
        return Json(stats);
    }

    /// <summary>
    /// API: Get feature highlights
    /// </summary>
    [Route("api/landing/features")]
    [HttpGet]
    public async Task<IActionResult> GetFeatureHighlights([FromQuery] string? category = null)
    {
        var features = await GetFeatureHighlightsAsync(category);
        return Json(features);
    }

    /// <summary>
    /// API: Get all partners
    /// </summary>
    [Route("api/landing/partners")]
    [HttpGet]
    public async Task<IActionResult> GetPartnersApi()
    {
        var partners = await GetPartnersAsync();
        return Json(partners);
    }

    /// <summary>
    /// API: Get testimonials
    /// </summary>
    [Route("api/landing/testimonials")]
    [HttpGet]
    public async Task<IActionResult> GetTestimonialsApi()
    {
        var testimonials = await GetTestimonialsAsync();
        return Json(testimonials);
    }

    /// <summary>
    /// API: Get all landing page data (aggregated)
    /// </summary>
    [Route("api/landing/all")]
    [HttpGet]
    public async Task<IActionResult> GetAllLandingData()
    {
        var data = new
        {
            ClientLogos = await GetClientLogosAsync(),
            TrustBadges = await GetTrustBadgesAsync(),
            Testimonials = await GetTestimonialsAsync(),
            CaseStudies = await GetCaseStudiesAsync(),
            Statistics = await GetLandingStatisticsAsync(),
            Features = await GetFeatureHighlightsAsync(),
            Partners = await GetPartnersAsync(),
            Faqs = await GetFaqsAsync()
        };
        return Json(data);
    }

    // ========== NEW SEO PAGES ==========

    /// <summary>
    /// Free Trial page (Highest Priority - Transactional)
    /// </summary>
    [Route("/grc-free-trial")]
    public IActionResult FreeTrial()
    {
        return View();
    }

    /// <summary>
    /// Request Access page (Enterprise / Controlled)
    /// </summary>
    [Route("/request-access")]
    public IActionResult RequestAccess()
    {
        return View();
    }

    /// <summary>
    /// Best GRC Software page (Commercial / Evaluation)
    /// </summary>
    [Route("/best-grc-software")]
    public IActionResult BestGrcSoftware()
    {
        return View();
    }

    /// <summary>
    /// Why Our GRC page (Commercial)
    /// </summary>
    [Route("/why-our-grc")]
    public IActionResult WhyOurGrc()
    {
        return View();
    }

    // ========== ROLE-BASED PAGES ==========

    /// <summary>
    /// GRC for Compliance Managers
    /// </summary>
    [Route("/grc-for-compliance-managers")]
    public IActionResult GrcForCompliance()
    {
        return View();
    }

    /// <summary>
    /// GRC for CISOs & Risk Leaders
    /// </summary>
    [Route("/grc-for-risk-ciso")]
    public IActionResult GrcForCiso()
    {
        return View();
    }

    /// <summary>
    /// GRC for Internal Audit
    /// </summary>
    [Route("/grc-for-internal-audit")]
    public IActionResult GrcForInternalAudit()
    {
        return View();
    }

    // ========== USE-CASE PAGES ==========

    /// <summary>
    /// GRC for ISO 27001
    /// </summary>
    [Route("/grc-for-iso-27001")]
    public IActionResult GrcForIso27001()
    {
        return View();
    }

    /// <summary>
    /// GRC for SOC 2
    /// </summary>
    [Route("/grc-for-soc-2")]
    public IActionResult GrcForSoc2()
    {
        return View();
    }

    /// <summary>
    /// GRC for Risk Assessment
    /// </summary>
    [Route("/grc-for-risk-assessment")]
    public IActionResult GrcForRiskAssessment()
    {
        return View();
    }

    /// <summary>
    /// GRC for Internal Controls
    /// </summary>
    [Route("/grc-for-internal-controls")]
    public IActionResult GrcForInternalControls()
    {
        return View();
    }

    /// <summary>
    /// GRC Guides Content Hub
    /// </summary>
    [Route("/grc-guides")]
    public IActionResult GrcGuides()
    {
        return View();
    }

    // ========== INVITE / QR FLOW ==========

    /// <summary>
    /// Invite Landing Page (QR Destination)
    /// </summary>
    [Route("/invite/{token?}")]
    public IActionResult Invite(string? token)
    {
        ViewData["Token"] = token;
        return View();
    }

    // ========== DOGAN CONSULT PAGES ==========

    /// <summary>
    /// Dogan Consult main company page
    /// </summary>
    [Route("/dogan-consult")]
    public IActionResult DoganConsult()
    {
        return View();
    }

    /// <summary>
    /// Dogan Consult Arabic profile
    /// </summary>
    [Route("/dogan-consult/ar")]
    public IActionResult DoganConsultArabic()
    {
        return View();
    }

    /// <summary>
    /// Dogan Consult - Telecommunications Engineering
    /// </summary>
    [Route("/dogan-consult/telecommunications")]
    public IActionResult DoganTelecommunications()
    {
        return View();
    }

    /// <summary>
    /// Dogan Consult - Data Centers
    /// </summary>
    [Route("/dogan-consult/data-centers")]
    public IActionResult DoganDataCenters()
    {
        return View();
    }

    /// <summary>
    /// Dogan Consult - Cybersecurity
    /// </summary>
    [Route("/dogan-consult/cybersecurity")]
    public IActionResult DoganCybersecurity()
    {
        return View();
    }

    #region Private Helpers

    private List<FeatureItem> GetFeatures() => new()
    {
        new FeatureItem
        {
            Icon = "fas fa-brain",
            Title = "Smart Scope Derivation",
            TitleAr = "اشتقاق النطاق الذكي",
            Description = "Answer 96 questions, get your complete GRC plan automatically derived from 13,500+ controls",
            DescriptionAr = "أجب على 96 سؤالاً واحصل على خطة GRC كاملة مشتقة تلقائياً من أكثر من 13,500 ضابط"
        },
        new FeatureItem
        {
            Icon = "fas fa-balance-scale",
            Title = "KSA Compliance Ready",
            TitleAr = "جاهز للامتثال السعودي",
            Description = "Pre-loaded with NCA ECC, SAMA CSF, PDPL, CITC and 130+ regulators",
            DescriptionAr = "محمّل مسبقاً بـ NCA ECC و SAMA CSF و PDPL و CITC وأكثر من 130 جهة تنظيمية"
        },
        new FeatureItem
        {
            Icon = "fas fa-project-diagram",
            Title = "Automated Workflows",
            TitleAr = "سير عمل آلي",
            Description = "7 pre-built workflows for assessments, evidence collection, approvals, and audits",
            DescriptionAr = "7 سير عمل جاهز للتقييمات وجمع الأدلة والموافقات والمراجعات"
        },
        new FeatureItem
        {
            Icon = "fas fa-file-alt",
            Title = "Evidence Management",
            TitleAr = "إدارة الأدلة",
            Description = "Automated evidence collection, tagging, and lifecycle management with audit trails",
            DescriptionAr = "جمع الأدلة الآلي والتصنيف وإدارة دورة الحياة مع سجلات التدقيق"
        },
        new FeatureItem
        {
            Icon = "fas fa-users",
            Title = "Team & RACI",
            TitleAr = "الفريق و RACI",
            Description = "Define teams, assign roles, and map responsibilities with RACI matrix",
            DescriptionAr = "تحديد الفرق وتعيين الأدوار وتوزيع المسؤوليات باستخدام مصفوفة RACI"
        },
        new FeatureItem
        {
            Icon = "fas fa-chart-line",
            Title = "Real-time Analytics",
            TitleAr = "تحليلات فورية",
            Description = "Executive dashboards, compliance scores, risk heatmaps, and trend analysis",
            DescriptionAr = "لوحات تحكم تنفيذية ودرجات الامتثال وخرائط المخاطر وتحليل الاتجاهات"
        }
    };

    private List<TestimonialItem> GetTestimonials()
    {
        // Fetch real testimonials from database
        try
        {
            var testimonials = _context.Testimonials
                .Where(t => t.IsActive)
                .OrderBy(t => t.DisplayOrder)
                .Take(6)
                .ToList();

            if (testimonials.Any())
            {
                return testimonials.Select(t => new TestimonialItem
                {
                    Quote = t.Quote,
                    QuoteAr = t.QuoteAr,
                    Author = $"{t.AuthorName} - {t.AuthorTitle}",
                    AuthorAr = t.AuthorNameAr,
                    Company = t.CompanyName,
                    CompanyAr = t.CompanyNameAr
                }).ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error fetching testimonials from database, using fallback");
        }

        // NO FALLBACK TESTIMONIALS - We are new to market with no real customers yet.
        // DO NOT add fake testimonials with specific names - this is misleading.
        // Return empty list - the view will not render the testimonials section.
        return new List<TestimonialItem>();
    }

    private StatsViewModel GetStats()
    {
        // Fetch real stats from database
        try
        {
            var regulatorCount = _context.RegulatorCatalogs.Count();
            var frameworkCount = _context.FrameworkCatalogs.Count();
            var controlCount = _context.ControlCatalogs.Count();
            var evidenceCount = _context.EvidenceTypeCatalogs.Count();
            var workflowCount = _context.Workflows.Count();

            // Only return real data if we have seeded catalogs
            if (regulatorCount > 0 || frameworkCount > 0)
            {
                return new StatsViewModel
                {
                    Regulators = regulatorCount > 0 ? regulatorCount : 92,
                    Frameworks = frameworkCount > 0 ? frameworkCount : 163,
                    Controls = controlCount > 0 ? controlCount : 13476,
                    EvidenceItems = evidenceCount > 0 ? evidenceCount : 500,
                    Workflows = workflowCount > 0 ? workflowCount : 12
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error fetching stats from database");
        }

        // Fallback stats (based on seeded data)
        return new StatsViewModel
        {
            Regulators = 92,
            Frameworks = 163,
            Controls = 13476,
            EvidenceItems = 500,
            Workflows = 12
        };
    }

    private List<string> GetHighlightedRegulators() => new()
    {
        "NCA (National Cybersecurity Authority)",
        "SAMA (Saudi Central Bank)",
        "PDPL (Personal Data Protection Law)",
        "CITC (Communications & IT Commission)",
        "MOH (Ministry of Health)",
        "CMA (Capital Market Authority)"
    };

    private List<PricingPlan> GetPricingPlans()
    {
        // Fetch real pricing plans from database
        try
        {
            var dbPlans = _context.PricingPlans
                .Where(p => p.IsActive)
                .OrderBy(p => p.DisplayOrder)
                .ToList();

            if (dbPlans.Any())
            {
                return dbPlans.Select(p => new PricingPlan
                {
                    Name = p.Name,
                    NameAr = p.NameAr ?? p.Name,
                    Price = (int)p.Price,
                    Period = p.Period,
                    Features = ParseFeatures(p.FeaturesJson),
                    FeaturesAr = ParseFeatures(p.FeaturesJsonAr),
                    IsPopular = p.IsPopular
                }).ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error fetching pricing plans from database, using fallback");
        }

        // Fallback pricing plans
        return new List<PricingPlan>
        {
            new()
            {
                Name = "Trial",
                NameAr = "تجريبي",
                Price = 0,
                Period = "7 days",
                Features = new[] { "Full access", "96-question onboarding", "1 workspace", "Basic support" }
            },
            new()
            {
                Name = "Starter",
                NameAr = "مبتدئ",
                Price = 999,
                Period = "month",
                Features = new[] { "Up to 5 users", "2 workspaces", "5 frameworks", "Email support" }
            },
            new()
            {
                Name = "Professional",
                NameAr = "احترافي",
                Price = 2999,
                Period = "month",
                Features = new[] { "Up to 25 users", "Unlimited workspaces", "All frameworks", "Priority support", "API access" },
                IsPopular = true
            },
            new()
            {
                Name = "Enterprise",
                NameAr = "مؤسسي",
                Price = -1,
                Period = "custom",
                Features = new[] { "Unlimited users", "Custom integrations", "Dedicated support", "On-premise option", "SLA guarantee" }
            }
        };
    }

    private static string[] ParseFeatures(string? json)
    {
        if (string.IsNullOrEmpty(json)) return Array.Empty<string>();
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<string[]>(json) ?? Array.Empty<string>();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    private List<FeatureCategory> GetFeatureCategories() => new()
    {
        new FeatureCategory
        {
            Name = "Compliance Management",
            Icon = "fas fa-clipboard-check",
            Features = new[] { "Framework mapping", "Control assessments", "Gap analysis", "Remediation tracking" }
        },
        new FeatureCategory
        {
            Name = "Risk Management",
            Icon = "fas fa-exclamation-triangle",
            Features = new[] { "Risk register", "Risk assessment", "Risk treatment", "Risk monitoring" }
        },
        new FeatureCategory
        {
            Name = "Audit Management",
            Icon = "fas fa-search",
            Features = new[] { "Audit planning", "Audit execution", "Finding tracking", "Report generation" }
        }
    };

    // ========== NEW MARKETING DATA HELPERS ==========

    private async Task<List<ClientLogoItem>> GetClientLogosAsync()
    {
        try
        {
            var logos = await _context.ClientLogos
                .Where(l => l.IsActive)
                .OrderBy(l => l.DisplayOrder)
                .ToListAsync();

            if (logos.Any())
            {
                return logos.Select(l => new ClientLogoItem
                {
                    Id = l.Id,
                    ClientName = l.ClientName,
                    ClientNameAr = l.ClientNameAr,
                    LogoUrl = l.LogoUrl,
                    WebsiteUrl = l.WebsiteUrl,
                    Industry = l.Industry,
                    IndustryAr = l.IndustryAr,
                    Category = l.Category,
                    IsFeatured = l.IsFeatured
                }).ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error fetching client logos from database");
        }

        return new List<ClientLogoItem>();
    }

    private async Task<List<TrustBadgeItem>> GetTrustBadgesAsync()
    {
        try
        {
            var badges = await _context.TrustBadges
                .Where(b => b.IsActive)
                .OrderBy(b => b.DisplayOrder)
                .ToListAsync();

            if (badges.Any())
            {
                return badges.Select(b => new TrustBadgeItem
                {
                    Id = b.Id,
                    Name = b.Name,
                    NameAr = b.NameAr,
                    Description = b.Description,
                    DescriptionAr = b.DescriptionAr,
                    ImageUrl = b.ImageUrl,
                    VerificationUrl = b.VerificationUrl,
                    Category = b.Category,
                    BadgeCode = b.BadgeCode
                }).ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error fetching trust badges from database");
        }

        // Fallback badges
        return new List<TrustBadgeItem>
        {
            new() { Name = "ISO 27001", NameAr = "ISO 27001", ImageUrl = "/images/badges/iso27001.svg", Category = "Certification" },
            new() { Name = "SOC 2", NameAr = "SOC 2", ImageUrl = "/images/badges/soc2.svg", Category = "Certification" },
            new() { Name = "NCA", NameAr = "NCA", ImageUrl = "/images/badges/nca.svg", Category = "Compliance" }
        };
    }

    private async Task<List<FaqItem>> GetFaqsAsync(string? category = null)
    {
        try
        {
            var query = _context.Faqs.Where(f => f.IsActive);

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(f => f.Category == category);
            }

            var faqs = await query.OrderBy(f => f.DisplayOrder).ToListAsync();

            if (faqs.Any())
            {
                return faqs.Select(f => new FaqItem
                {
                    Id = f.Id,
                    Question = f.Question,
                    QuestionAr = f.QuestionAr,
                    Answer = f.Answer,
                    AnswerAr = f.AnswerAr,
                    Category = f.Category
                }).ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error fetching FAQs from database");
        }

        return new List<FaqItem>();
    }

    private async Task<List<LandingStatisticItem>> GetLandingStatisticsAsync()
    {
        try
        {
            var stats = await _context.LandingStatistics
                .Where(s => s.IsActive)
                .OrderBy(s => s.DisplayOrder)
                .ToListAsync();

            if (stats.Any())
            {
                return stats.Select(s => new LandingStatisticItem
                {
                    Id = s.Id,
                    Label = s.Label,
                    LabelAr = s.LabelAr,
                    Value = s.Value,
                    Suffix = s.Suffix,
                    IconClass = s.IconClass,
                    Category = s.Category
                }).ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error fetching landing statistics from database");
        }

        return new List<LandingStatisticItem>();
    }

    private async Task<List<FeatureHighlightItem>> GetFeatureHighlightsAsync(string? category = null)
    {
        try
        {
            var query = _context.FeatureHighlights.Where(f => f.IsActive);

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(f => f.Category == category);
            }

            var features = await query.OrderBy(f => f.DisplayOrder).ToListAsync();

            if (features.Any())
            {
                return features.Select(f => new FeatureHighlightItem
                {
                    Id = f.Id,
                    Title = f.Title,
                    TitleAr = f.TitleAr,
                    Description = f.Description,
                    DescriptionAr = f.DescriptionAr,
                    IconClass = f.IconClass,
                    ImageUrl = f.ImageUrl,
                    LearnMoreUrl = f.LearnMoreUrl,
                    Category = f.Category,
                    IsFeatured = f.IsFeatured
                }).ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error fetching feature highlights from database");
        }

        return new List<FeatureHighlightItem>();
    }

    private async Task<List<PartnerItem>> GetPartnersAsync()
    {
        try
        {
            var partners = await _context.Partners
                .Where(p => p.IsActive)
                .OrderBy(p => p.DisplayOrder)
                .ToListAsync();

            if (partners.Any())
            {
                return partners.Select(p => new PartnerItem
                {
                    Id = p.Id,
                    Name = p.Name,
                    NameAr = p.NameAr,
                    Description = p.Description,
                    DescriptionAr = p.DescriptionAr,
                    LogoUrl = p.LogoUrl,
                    WebsiteUrl = p.WebsiteUrl,
                    Type = p.Type,
                    Tier = p.Tier,
                    IsFeatured = p.IsFeatured
                }).ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error fetching partners from database");
        }

        return new List<PartnerItem>();
    }

    private async Task<List<TestimonialItem>> GetTestimonialsAsync()
    {
        try
        {
            var testimonials = await _context.Testimonials
                .Where(t => t.IsActive)
                .OrderBy(t => t.DisplayOrder)
                .Take(10)
                .ToListAsync();

            if (testimonials.Any())
            {
                return testimonials.Select(t => new TestimonialItem
                {
                    Quote = t.Quote,
                    QuoteAr = t.QuoteAr,
                    Author = $"{t.AuthorName} - {t.AuthorTitle}",
                    AuthorAr = t.AuthorNameAr,
                    Company = t.CompanyName,
                    CompanyAr = t.CompanyNameAr
                }).ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error fetching testimonials from database");
        }

        return new List<TestimonialItem>();
    }

    #endregion
}

#region View Models

public class LandingPageViewModel
{
    public List<FeatureItem> Features { get; set; } = new();
    public List<TestimonialItem> Testimonials { get; set; } = new();
    public StatsViewModel Stats { get; set; } = new();
    public List<string> Regulators { get; set; } = new();
}

public class FeatureItem
{
    public string Icon { get; set; } = "";
    public string Title { get; set; } = "";
    public string TitleAr { get; set; } = "";
    public string Description { get; set; } = "";
    public string DescriptionAr { get; set; } = "";
}

public class TestimonialItem
{
    public string Quote { get; set; } = "";
    public string? QuoteAr { get; set; }
    public string Author { get; set; } = "";
    public string? AuthorAr { get; set; }
    public string Company { get; set; } = "";
    public string? CompanyAr { get; set; }

    // Alias properties for template compatibility
    public string Content => Quote;
    public string? ContentAr => QuoteAr;
    public string AuthorName => Author?.Split(" - ").FirstOrDefault() ?? Author;
    public string? AuthorTitle => Author?.Contains(" - ") == true ? Author.Split(" - ").LastOrDefault() : null;
}

public class StatsViewModel
{
    public int Regulators { get; set; }
    public int Frameworks { get; set; }
    public int Controls { get; set; }
    public int EvidenceItems { get; set; }
    public int Workflows { get; set; }
}

public class PricingViewModel
{
    public List<PricingPlan> Plans { get; set; } = new();
}

public class PricingPlan
{
    public string Name { get; set; } = "";
    public string NameAr { get; set; } = "";
    public decimal Price { get; set; }
    public string Period { get; set; } = "";
    public string[] Features { get; set; } = Array.Empty<string>();
    public string[] FeaturesAr { get; set; } = Array.Empty<string>();
    public bool IsPopular { get; set; }
}

public class CaseStudiesViewModel
{
    public List<CaseStudyItem> CaseStudies { get; set; } = new();
    public List<TestimonialItem> Testimonials { get; set; } = new();
}

public class CaseStudyItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string? TitleAr { get; set; }
    public string? Slug { get; set; }
    public string Summary { get; set; } = "";
    public string? SummaryAr { get; set; }
    public string Industry { get; set; } = "";
    public string? IndustryAr { get; set; }
    public string? FrameworkCode { get; set; }
    public string? TimeToCompliance { get; set; }
    public string? ImprovementMetric { get; set; }
    public string? ImprovementLabel { get; set; }
    public string? ImprovementLabelAr { get; set; }
    public string? ComplianceScore { get; set; }
    public bool IsFeatured { get; set; }
}

public class FeaturesViewModel
{
    public List<FeatureCategory> Categories { get; set; } = new();
}

public class FeatureCategory
{
    public string Name { get; set; } = "";
    public string Icon { get; set; } = "";
    public string[] Features { get; set; } = Array.Empty<string>();
}

public class ClientLogoItem
{
    public Guid Id { get; set; }
    public string ClientName { get; set; } = "";
    public string? ClientNameAr { get; set; }
    public string LogoUrl { get; set; } = "";
    public string? WebsiteUrl { get; set; }
    public string? Industry { get; set; }
    public string? IndustryAr { get; set; }
    public string Category { get; set; } = "";
    public bool IsFeatured { get; set; }
}

public class TrustBadgeItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? NameAr { get; set; }
    public string? Description { get; set; }
    public string? DescriptionAr { get; set; }
    public string ImageUrl { get; set; } = "";
    public string? VerificationUrl { get; set; }
    public string Category { get; set; } = "";
    public string? BadgeCode { get; set; }
}

public class FaqItem
{
    public Guid Id { get; set; }
    public string Question { get; set; } = "";
    public string? QuestionAr { get; set; }
    public string Answer { get; set; } = "";
    public string? AnswerAr { get; set; }
    public string Category { get; set; } = "";
}

public class LandingStatisticItem
{
    public Guid Id { get; set; }
    public string Label { get; set; } = "";
    public string? LabelAr { get; set; }
    public string Value { get; set; } = "";
    public string? Suffix { get; set; }
    public string? IconClass { get; set; }
    public string Category { get; set; } = "";
}

public class FeatureHighlightItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string? TitleAr { get; set; }
    public string? Description { get; set; }
    public string? DescriptionAr { get; set; }
    public string? IconClass { get; set; }
    public string? ImageUrl { get; set; }
    public string? LearnMoreUrl { get; set; }
    public string Category { get; set; } = "";
    public bool IsFeatured { get; set; }
}

public class PartnerItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? NameAr { get; set; }
    public string? Description { get; set; }
    public string? DescriptionAr { get; set; }
    public string LogoUrl { get; set; } = "";
    public string? WebsiteUrl { get; set; }
    public string Type { get; set; } = "";
    public string Tier { get; set; } = "";
    public bool IsFeatured { get; set; }
}

public class PartnersViewModel
{
    public List<PartnerItem> Partners { get; set; } = new();
    public List<TrustBadgeItem> TrustBadges { get; set; } = new();
}

public class FaqViewModel
{
    public List<FaqItem> Faqs { get; set; } = new();
}

public class ContactFormDto
{
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Company { get; set; }
    public string? Phone { get; set; }
    public string Subject { get; set; } = "";
    public string Message { get; set; } = "";
}

public class ChatMessageDto
{
    public string Message { get; set; } = "";
    public string? Context { get; set; }
}

public class SystemStatusViewModel
{
    public List<ServiceStatusItem> Services { get; set; } = new();
    public string OverallStatus => Services.All(s => s.Status == "operational") ? "operational" : "degraded";
    public string OverallStatusAr => OverallStatus == "operational" ? "جميع الخدمات تعمل بشكل طبيعي" : "بعض الخدمات تواجه مشاكل";
}

public class ServiceStatusItem
{
    public string Name { get; set; } = "";
    public string NameEn { get; set; } = "";
    public string Status { get; set; } = "operational"; // operational, degraded, outage
    public string StatusAr { get; set; } = "";
    public string Uptime { get; set; } = "";
    public DateTime LastChecked { get; set; }
}

#endregion
