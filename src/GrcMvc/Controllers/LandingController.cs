using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrcMvc.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;

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
        // #region agent log
        try { System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "I", location = "LandingController.cs:21", message = "LandingController constructor", data = new { contextExists = context != null, loggerExists = logger != null, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
        // #endregion
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
        // #region agent log
        try { System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "F", location = "LandingController.cs:31", message = "Landing Index entry", data = new { isAuthenticated = User.Identity?.IsAuthenticated ?? false, host = Request.Host.Host, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
        // #endregion
        // If authenticated, redirect to dashboard
        if (User.Identity?.IsAuthenticated == true)
        {
            // #region agent log
            try { System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "F", location = "LandingController.cs:35", message = "Redirecting authenticated user to Dashboard", data = new { userId = User.Identity?.Name, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
            // #endregion
            return RedirectToAction("Index", "Dashboard");
        }

        // Check if request is from shahin-ai.com domain (or localhost for dev)
        var host = Request.Host.Host.ToLower();

        // Serve the modern Vercel-style landing page
        // #region agent log
        try { System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "G", location = "LandingController.cs:45", message = "Before GetFeatures call", data = new { timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
        // #endregion
        var features = GetFeatures();
        // #region agent log
        try { System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "G", location = "LandingController.cs:47", message = "GetFeatures result", data = new { featuresCount = features?.Count ?? 0, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
        // #endregion
        // #region agent log
        try { System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "G", location = "LandingController.cs:48", message = "Before GetTestimonials call", data = new { timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
        // #endregion
        var testimonials = GetTestimonials();
        // #region agent log
        try { System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "G", location = "LandingController.cs:49", message = "GetTestimonials result", data = new { testimonialsCount = testimonials?.Count ?? 0, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
        // #endregion
        // #region agent log
        try { System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "G", location = "LandingController.cs:50", message = "Before GetStats call", data = new { timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
        // #endregion
        var stats = GetStats();
        // #region agent log
        try { System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "G", location = "LandingController.cs:51", message = "GetStats result", data = new { statsRegulators = stats?.Regulators ?? 0, statsFrameworks = stats?.Frameworks ?? 0, statsControls = stats?.Controls ?? 0, statsNull = stats == null, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
        // #endregion
        var regulators = GetHighlightedRegulators();
        // #region agent log
        try { System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "G", location = "LandingController.cs:52", message = "GetHighlightedRegulators result", data = new { regulatorsCount = regulators?.Count ?? 0, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
        // #endregion
        
        var model = new LandingPageViewModel
        {
            Features = features ?? new List<FeatureItem>(),
            Testimonials = testimonials ?? new List<TestimonialItem>(),
            Stats = stats ?? new StatsViewModel(),
            Regulators = regulators ?? new List<string>()
        };
        // #region agent log
        try { System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "G", location = "LandingController.cs:60", message = "Before View render", data = new { modelFeaturesCount = model.Features?.Count ?? 0, modelTestimonialsCount = model.Testimonials?.Count ?? 0, modelStatsNull = model.Stats == null, modelRegulatorsCount = model.Regulators?.Count ?? 0, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
        // #endregion
        try
        {
            var viewResult = View("Index", model);
            // #region agent log
            try { System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "G", location = "LandingController.cs:65", message = "View render success", data = new { viewResultType = viewResult?.GetType().Name, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
            // #endregion
            return viewResult;
        }
        catch (Exception ex)
        {
            // #region agent log
            try { System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "H", location = "LandingController.cs:70", message = "View render exception", data = new { exceptionType = ex.GetType().Name, exceptionMessage = ex.Message, stackTrace = ex.StackTrace?.Substring(0, Math.Min(500, ex.StackTrace?.Length ?? 0)), timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
            // #endregion
            _logger.LogError(ex, "Error rendering landing page view");
            throw;
        }
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

    /// <summary>
    /// Case Study Detail Page
    /// </summary>
    [Route("/case-studies/{slug}")]
    public async Task<IActionResult> CaseStudyDetails(string slug)
    {
        try
        {
            // Try to get from database first
            var caseStudy = await _context.CaseStudies
                .FirstOrDefaultAsync(c => c.Slug == slug && c.IsActive);

            if (caseStudy != null)
            {
                var model = new CaseStudyDetailViewModel
                {
                    Id = caseStudy.Id,
                    Title = caseStudy.Title,
                    TitleAr = caseStudy.TitleAr,
                    Slug = caseStudy.Slug,
                    Summary = caseStudy.Summary,
                    SummaryAr = caseStudy.SummaryAr,
                    Industry = caseStudy.Industry,
                    IndustryAr = caseStudy.IndustryAr,
                    FrameworkCode = caseStudy.FrameworkCode,
                    Challenge = caseStudy.Challenge ?? string.Empty,
                    ChallengeAr = caseStudy.ChallengeAr ?? string.Empty,
                    Solution = caseStudy.Solution ?? string.Empty,
                    SolutionAr = caseStudy.SolutionAr ?? string.Empty,
                    Results = caseStudy.Results ?? string.Empty,
                    ResultsAr = caseStudy.ResultsAr ?? string.Empty,
                    TimeToCompliance = caseStudy.TimeToCompliance,
                    ImprovementMetric = caseStudy.ImprovementMetric,
                    ImprovementLabel = caseStudy.ImprovementLabel,
                    ImprovementLabelAr = caseStudy.ImprovementLabelAr,
                    ComplianceScore = caseStudy.ComplianceScore,
                    CustomerQuote = caseStudy.CustomerQuote ?? string.Empty,
                    CustomerQuoteAr = caseStudy.CustomerQuoteAr ?? string.Empty,
                    CustomerName = caseStudy.CustomerName ?? string.Empty,
                    CustomerTitle = caseStudy.CustomerTitle ?? string.Empty,
                    CustomerTitleAr = caseStudy.CustomerTitleAr ?? string.Empty,
                    RelatedCaseStudies = await GetRelatedCaseStudiesAsync(caseStudy.Industry, caseStudy.Id)
                };

                return View(model);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error fetching case study details for slug: {Slug}", slug);
        }

        // Case study not found
        return NotFound();
    }

    private async Task<List<CaseStudyItem>> GetRelatedCaseStudiesAsync(string? industry, Guid excludeId)
    {
        try
        {
            var relatedCaseStudies = await _context.CaseStudies
                .Where(c => c.IsActive && c.Id != excludeId)
                .Where(c => string.IsNullOrEmpty(industry) || c.Industry == industry)
                .OrderBy(c => c.DisplayOrder)
                .Take(3)
                .Select(c => new CaseStudyItem
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
                    ComplianceScore = c.ComplianceScore
                })
                .ToListAsync();

            return relatedCaseStudies;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error fetching related case studies for industry: {Industry}", industry);
            return new List<CaseStudyItem>();
        }
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
    /// API: Start Trial - Register for a free trial
    /// POST /api/Landing/StartTrial
    /// ASP.NET Core Best Practice: Proper validation and error handling
    /// </summary>
    [Route("api/Landing/StartTrial")]
    [HttpPost]
    [IgnoreAntiforgeryToken] // Required for cross-origin requests from landing page (shahin-ai.com)
    [AllowAnonymous]
    public async Task<IActionResult> StartTrial([FromBody] TrialSignupDto model)
    {
        if (model == null)
        {
            return BadRequest(new { success = false, messageEn = "Invalid request data", messageAr = "بيانات الطلب غير صالحة" });
        }

        // Validate email
        if (string.IsNullOrWhiteSpace(model.Email) || !IsValidEmail(model.Email))
        {
            return BadRequest(new { success = false, messageEn = "A valid email address is required", messageAr = "عنوان البريد الإلكتروني مطلوب" });
        }

        // Validate name
        if (string.IsNullOrWhiteSpace(model.FullName) || model.FullName.Length < 2)
        {
            return BadRequest(new { success = false, messageEn = "Full name is required", messageAr = "الاسم الكامل مطلوب" });
        }

        // Validate company
        if (string.IsNullOrWhiteSpace(model.CompanyName))
        {
            return BadRequest(new { success = false, messageEn = "Company name is required", messageAr = "اسم الشركة مطلوب" });
        }

        try
        {
            // Check if email already registered
            var existingTenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.Email != null && t.Email.ToLower() == model.Email.ToLower());

            if (existingTenant != null)
            {
                _logger.LogInformation("Trial signup attempt with existing email: {Email}", model.Email);
                return BadRequest(new 
                { 
                    success = false, 
                    messageEn = "This email is already registered. Please log in or use a different email.",
                    messageAr = "هذا البريد الإلكتروني مسجل مسبقاً. يرجى تسجيل الدخول أو استخدام بريد إلكتروني آخر."
                });
            }

            // Store trial signup request
            var trialSignup = new Models.Entities.TrialSignup
            {
                Id = Guid.NewGuid(),
                Email = model.Email,
                FullName = model.FullName,
                CompanyName = model.CompanyName,
                PhoneNumber = model.PhoneNumber,
                CompanySize = model.CompanySize,
                Industry = model.Industry,
                TrialPlan = model.TrialPlan ?? "STARTER",
                Status = "Pending",
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers["User-Agent"].ToString(),
                Locale = model.Locale ?? "ar",
                CreatedDate = DateTime.UtcNow
            };

            _context.Add(trialSignup);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Trial signup created: {Email}, Company: {Company}, Plan: {Plan}",
                model.Email, model.CompanyName, model.TrialPlan);

            // Redirect to the trial registration page
            var redirectUrl = $"/grc-free-trial?email={Uri.EscapeDataString(model.Email)}&name={Uri.EscapeDataString(model.FullName)}";

            return Ok(new 
            { 
                success = true, 
                messageEn = "Trial signup received! You will be redirected to complete registration.",
                messageAr = "تم استلام طلب التجربة! سيتم توجيهك لإكمال التسجيل.",
                redirectUrl = redirectUrl,
                signupId = trialSignup.Id
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing trial signup for {Email}", model.Email);
            return StatusCode(500, new 
            { 
                success = false, 
                messageEn = "An error occurred. Please try again.",
                messageAr = "حدث خطأ. يرجى المحاولة مرة أخرى."
            });
        }
    }

    /// <summary>
    /// API: Subscribe to Newsletter
    /// POST /api/Landing/SubscribeNewsletter
    /// </summary>
    [Route("api/Landing/SubscribeNewsletter")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubscribeNewsletter([FromBody] NewsletterSubscriptionDto model)
    {
        if (model == null)
        {
            return BadRequest(new { success = false, messageEn = "Invalid request", messageAr = "طلب غير صالح" });
        }

        if (string.IsNullOrWhiteSpace(model.Email) || !IsValidEmail(model.Email))
        {
            return BadRequest(new { success = false, messageEn = "A valid email address is required", messageAr = "عنوان البريد الإلكتروني مطلوب" });
        }

        try
        {
            // Check if already subscribed
            var existingSubscription = await _context.Set<Models.Entities.NewsletterSubscription>()
                .FirstOrDefaultAsync(n => n.Email.ToLower() == model.Email.ToLower());

            if (existingSubscription != null)
            {
                if (existingSubscription.IsActive)
                {
                    return Ok(new 
                    { 
                        success = true, 
                        messageEn = "You're already subscribed to our newsletter!",
                        messageAr = "أنت مشترك بالفعل في نشرتنا الإخبارية!"
                    });
                }
                else
                {
                    // Reactivate subscription
                    existingSubscription.IsActive = true;
                    existingSubscription.ResubscribedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    return Ok(new 
                    { 
                        success = true, 
                        messageEn = "Welcome back! Your subscription has been reactivated.",
                        messageAr = "مرحباً بعودتك! تم إعادة تفعيل اشتراكك."
                    });
                }
            }

            // Create new subscription
            var subscription = new Models.Entities.NewsletterSubscription
            {
                Id = Guid.NewGuid(),
                Email = model.Email,
                Name = model.Name,
                IsActive = true,
                Locale = model.Locale ?? "ar",
                Source = "LandingPage",
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                SubscribedAt = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow
            };

            _context.Add(subscription);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Newsletter subscription created: {Email}", model.Email);

            return Ok(new 
            { 
                success = true, 
                messageEn = "Thank you for subscribing! You'll receive our latest updates.",
                messageAr = "شكراً لاشتراكك! ستصلك آخر التحديثات."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing newsletter subscription for {Email}", model.Email);
            return StatusCode(500, new 
            { 
                success = false, 
                messageEn = "An error occurred. Please try again.",
                messageAr = "حدث خطأ. يرجى المحاولة مرة أخرى."
            });
        }
    }

    /// <summary>
    /// API: Unsubscribe from Newsletter
    /// POST /api/Landing/UnsubscribeNewsletter
    /// </summary>
    [Route("api/Landing/UnsubscribeNewsletter")]
    [HttpPost]
    public async Task<IActionResult> UnsubscribeNewsletter([FromQuery] string email, [FromQuery] string? token = null)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(new { success = false, messageEn = "Email is required", messageAr = "البريد الإلكتروني مطلوب" });
        }

        try
        {
            var subscription = await _context.Set<Models.Entities.NewsletterSubscription>()
                .FirstOrDefaultAsync(n => n.Email.ToLower() == email.ToLower());

            if (subscription == null)
            {
                return NotFound(new 
                { 
                    success = false, 
                    messageEn = "Email not found in our subscription list.",
                    messageAr = "البريد الإلكتروني غير موجود في قائمة الاشتراكات."
                });
            }

            subscription.IsActive = false;
            subscription.UnsubscribedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Newsletter unsubscription: {Email}", email);

            return Ok(new 
            { 
                success = true, 
                messageEn = "You have been unsubscribed successfully.",
                messageAr = "تم إلغاء اشتراكك بنجاح."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing unsubscribe for {Email}", email);
            return StatusCode(500, new 
            { 
                success = false, 
                messageEn = "An error occurred. Please try again.",
                messageAr = "حدث خطأ. يرجى المحاولة مرة أخرى."
            });
        }
    }

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
        // #region agent log
        try { System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "G", location = "LandingController.cs:1059", message = "GetTestimonials entry", data = new { contextExists = _context != null, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
        // #endregion
        // Fetch real testimonials from database
        try
        {
            // #region agent log
            try { System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "G", location = "LandingController.cs:1063", message = "Before testimonials database query", data = new { timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
            // #endregion
            var testimonials = _context.Testimonials
                .Where(t => t.IsActive)
                .OrderBy(t => t.DisplayOrder)
                .Take(6)
                .ToList();
            // #region agent log
            try { System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "G", location = "LandingController.cs:1069", message = "After testimonials database query", data = new { testimonialsCount = testimonials?.Count ?? 0, hasAny = testimonials?.Any() ?? false, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
            // #endregion

            if (testimonials.Any())
            {
                // #region agent log
                try { System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "G", location = "LandingController.cs:1072", message = "Testimonials found - mapping to items", data = new { testimonialsCount = testimonials.Count, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                // #endregion
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
            // #region agent log
            try { System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "G", location = "LandingController.cs:1085", message = "GetTestimonials exception", data = new { exceptionType = ex.GetType().Name, exceptionMessage = ex.Message, stackTrace = ex.StackTrace?.Substring(0, Math.Min(500, ex.StackTrace?.Length ?? 0)), timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
            // #endregion
            _logger.LogWarning(ex, "Error fetching testimonials from database, using fallback");
        }

        // NO FALLBACK TESTIMONIALS - We are new to market with no real customers yet.
        // DO NOT add fake testimonials with specific names - this is misleading.
        // Return empty list - the view will not render the testimonials section.
        // #region agent log
        try { System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "G", location = "LandingController.cs:1092", message = "GetTestimonials returning empty list", data = new { timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
        // #endregion
        return new List<TestimonialItem>();
    }

    private StatsViewModel GetStats()
    {
        // #region agent log
        try { System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "G", location = "LandingController.cs:1094", message = "GetStats entry", data = new { contextExists = _context != null, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
        // #endregion
        // Fetch real stats from database
        try
        {
            // #region agent log
            try { System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "G", location = "LandingController.cs:1099", message = "Before RegulatorCatalogs.Count", data = new { timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
            // #endregion
            var regulatorCount = _context.RegulatorCatalogs.Count();
            // #region agent log
            try { System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "G", location = "LandingController.cs:1101", message = "RegulatorCatalogs.Count result", data = new { regulatorCount, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
            // #endregion
            var frameworkCount = _context.FrameworkCatalogs.Count();
            // #region agent log
            try { System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "G", location = "LandingController.cs:1103", message = "FrameworkCatalogs.Count result", data = new { frameworkCount, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
            // #endregion
            var controlCount = _context.ControlCatalogs.Count();
            // #region agent log
            try { System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "G", location = "LandingController.cs:1105", message = "ControlCatalogs.Count result", data = new { controlCount, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
            // #endregion
            var evidenceCount = _context.EvidenceTypeCatalogs.Count();
            var workflowCount = _context.Workflows.Count();
            // #region agent log
            try { System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "G", location = "LandingController.cs:1107", message = "All stats counts retrieved", data = new { regulatorCount, frameworkCount, controlCount, evidenceCount, workflowCount, hasData = regulatorCount > 0 || frameworkCount > 0, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
            // #endregion

            // Only return real data if we have seeded catalogs
            if (regulatorCount > 0 || frameworkCount > 0)
            {
                var stats = new StatsViewModel
                {
                    Regulators = regulatorCount > 0 ? regulatorCount : 92,
                    Frameworks = frameworkCount > 0 ? frameworkCount : 163,
                    Controls = controlCount > 0 ? controlCount : 13476,
                    EvidenceItems = evidenceCount > 0 ? evidenceCount : 500,
                    Workflows = workflowCount > 0 ? workflowCount : 12
                };
                // #region agent log
                try { System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "G", location = "LandingController.cs:1116", message = "GetStats returning real data", data = new { statsRegulators = stats.Regulators, statsFrameworks = stats.Frameworks, statsControls = stats.Controls, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                // #endregion
                return stats;
            }
        }
        catch (Exception ex)
        {
            // #region agent log
            try { System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "G", location = "LandingController.cs:1120", message = "GetStats exception", data = new { exceptionType = ex.GetType().Name, exceptionMessage = ex.Message, stackTrace = ex.StackTrace?.Substring(0, Math.Min(500, ex.StackTrace?.Length ?? 0)), timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
            // #endregion
            _logger.LogWarning(ex, "Error fetching stats from database");
        }

        // Fallback stats (based on seeded data)
        // #region agent log
        try { System.IO.File.AppendAllText("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "G", location = "LandingController.cs:1127", message = "GetStats returning fallback stats", data = new { timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
        // #endregion
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

public class CaseStudyDetailViewModel
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
    public string Challenge { get; set; } = "";
    public string? ChallengeAr { get; set; }
    public string Solution { get; set; } = "";
    public string? SolutionAr { get; set; }
    public string Results { get; set; } = "";
    public string? ResultsAr { get; set; }
    public string? TimeToCompliance { get; set; }
    public string? ImprovementMetric { get; set; }
    public string? ImprovementLabel { get; set; }
    public string? ImprovementLabelAr { get; set; }
    public string? ComplianceScore { get; set; }
    public string CustomerQuote { get; set; } = "";
    public string? CustomerQuoteAr { get; set; }
    public string CustomerName { get; set; } = "";
    public string CustomerTitle { get; set; } = "";
    public string? CustomerTitleAr { get; set; }
    public List<CaseStudyItem> RelatedCaseStudies { get; set; } = new();
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

/// <summary>
/// DTO for trial signup requests
/// Follows ABP naming convention: [Entity]Dto
/// </summary>
public class TrialSignupDto
{
    /// <summary>Email address (required)</summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(256)]
    public string Email { get; set; } = "";

    /// <summary>Full name (required)</summary>
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = "";

    /// <summary>Company name (required)</summary>
    [Required(ErrorMessage = "Company name is required")]
    [StringLength(200)]
    public string CompanyName { get; set; } = "";

    /// <summary>Phone number (optional)</summary>
    [Phone]
    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    /// <summary>Company size: 1-10, 11-50, 51-200, 201-500, 500+</summary>
    [StringLength(20)]
    public string? CompanySize { get; set; }

    /// <summary>Industry vertical</summary>
    [StringLength(100)]
    public string? Industry { get; set; }

    /// <summary>Selected trial plan: STARTER, PROFESSIONAL, ENTERPRISE</summary>
    [StringLength(50)]
    public string? TrialPlan { get; set; }

    /// <summary>Locale preference: en, ar</summary>
    [StringLength(10)]
    public string? Locale { get; set; }
}

/// <summary>
/// DTO for newsletter subscription requests
/// </summary>
public class NewsletterSubscriptionDto
{
    /// <summary>Email address (required)</summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(256)]
    public string Email { get; set; } = "";

    /// <summary>Subscriber name (optional)</summary>
    [StringLength(100)]
    public string? Name { get; set; }

    /// <summary>Locale preference: en, ar</summary>
    [StringLength(10)]
    public string? Locale { get; set; }

    /// <summary>Topics of interest</summary>
    public List<string>? Interests { get; set; }
}

#endregion
