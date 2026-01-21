using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GrcMvc.Controllers
{
    /// <summary>
    /// Controller for help system - Help Center, FAQ, Glossary, Getting Started
    /// Provides self-service support for users
    /// PUBLIC: Help pages are accessible without login
    /// </summary>
    [AllowAnonymous]
    public class HelpController : Controller
    {
        private readonly ILogger<HelpController> _logger;

        public HelpController(ILogger<HelpController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Main help center hub
        /// GET /Help
        /// </summary>
        public IActionResult Index()
        {
            ViewData["Title"] = "Help Center";
            return View();
        }

        /// <summary>
        /// Getting started guide
        /// GET /Help/GettingStarted
        /// </summary>
        public IActionResult GettingStarted()
        {
            ViewData["Title"] = "Getting Started";
            return View();
        }

        /// <summary>
        /// Frequently Asked Questions
        /// GET /Help/FAQ?search=term
        /// </summary>
        public IActionResult FAQ(string? search = null)
        {
            ViewData["Title"] = "Frequently Asked Questions";
            ViewData["SearchTerm"] = search;
            return View();
        }

        /// <summary>
        /// Full glossary of GRC terms
        /// GET /Help/Glossary
        /// </summary>
        public IActionResult Glossary()
        {
            ViewData["Title"] = "GRC Glossary";
            return View();
        }

        /// <summary>
        /// Contact support form
        /// GET /Help/Contact
        /// </summary>
        public IActionResult Contact()
        {
            ViewData["Title"] = "Contact Support";
            return View();
        }

        /// <summary>
        /// Coming Soon placeholder page
        /// GET /Help/coming-soon
        /// </summary>
        [Route("Help/coming-soon")]
        public IActionResult ComingSoon()
        {
            ViewData["Title"] = "قريباً - Coming Soon";
            return View();
        }

        /// <summary>
        /// Get glossary term definition (AJAX endpoint)
        /// GET /Help/GetGlossaryTerm?term=NCA ECC
        /// </summary>
        [HttpGet]
        public IActionResult GetGlossaryTerm(string term)
        {
            try
            {
                var glossaryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "glossary.json");
                
                if (!System.IO.File.Exists(glossaryPath))
                {
                    return Json(new { error = "Glossary not found" });
                }

                var jsonContent = System.IO.File.ReadAllText(glossaryPath);
                var glossary = JsonSerializer.Deserialize<GlossaryData>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var currentLanguage = Request.Cookies["GrcMvc.Culture"] ?? "ar";
                var termData = glossary?.Terms?.FirstOrDefault(t => 
                    t.Term.Equals(term, StringComparison.OrdinalIgnoreCase) && 
                    t.Language == currentLanguage);

                if (termData == null)
                {
                    return Json(new { error = "Term not found" });
                }

                return Json(new
                {
                    term = termData.Term,
                    definition = termData.Definition,
                    category = termData.Category,
                    examples = termData.Examples
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting glossary term: {Term}", term);
                return Json(new { error = "Error retrieving term" });
            }
        }

        /// <summary>
        /// Search FAQ (AJAX endpoint)
        /// GET /Help/SearchFAQ?query=term
        /// </summary>
        [HttpGet]
        public IActionResult SearchFAQ(string query)
        {
            try
            {
                var faqItems = GetFAQItems();
                
                if (string.IsNullOrWhiteSpace(query))
                {
                    return Json(faqItems);
                }

                var searchTerm = query.ToLower();
                var filtered = faqItems.Where(f => 
                    f.Question.ToLower().Contains(searchTerm) ||
                    f.Answer.ToLower().Contains(searchTerm) ||
                    f.Category.ToLower().Contains(searchTerm)
                ).ToList();

                return Json(filtered);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching FAQ: {Query}", query);
                return Json(new List<FAQItem>());
            }
        }

        /// <summary>
        /// Get FAQ items (can be moved to service later)
        /// </summary>
        private List<FAQItem> GetFAQItems()
        {
            return new List<FAQItem>
            {
                new FAQItem
                {
                    Id = 1,
                    Category = "Getting Started",
                    Question = "How do I start using the GRC system?",
                    Answer = "Start by completing the 12-step onboarding wizard. This will guide you through organization setup, framework selection, and initial compliance planning.",
                    Language = "en"
                },
                new FAQItem
                {
                    Id = 2,
                    Category = "Getting Started",
                    Question = "What is the onboarding wizard?",
                    Answer = "The onboarding wizard is a 12-step process (Steps A-L) that helps you configure your organization profile, select applicable regulatory frameworks, and create your first compliance plan.",
                    Language = "en"
                },
                new FAQItem
                {
                    Id = 3,
                    Category = "Frameworks",
                    Question = "What is NCA ECC?",
                    Answer = "NCA ECC (National Cybersecurity Authority Essential Cybersecurity Controls) is Saudi Arabia's mandatory cybersecurity framework for all organizations operating in the Kingdom.",
                    Language = "en"
                },
                new FAQItem
                {
                    Id = 4,
                    Category = "Frameworks",
                    Question = "What is SAMA CSF?",
                    Answer = "SAMA CSF (Saudi Arabian Monetary Authority Cyber Security Framework) is the cybersecurity framework specifically for financial institutions regulated by SAMA.",
                    Language = "en"
                },
                new FAQItem
                {
                    Id = 5,
                    Category = "Data Protection",
                    Question = "What is PDPL?",
                    Answer = "PDPL (Personal Data Protection Law) is Saudi Arabia's data protection regulation that governs how personal data must be collected, processed, and protected.",
                    Language = "en"
                },
                new FAQItem
                {
                    Id = 6,
                    Category = "Arabic",
                    Question = "كيف أبدأ في استخدام النظام؟",
                    Answer = "ابدأ بإكمال معالج الإعداد المكون من 12 خطوة. سيرشدك هذا خلال إعداد المنظمة واختيار الأطر التنظيمية وتخطيط الامتثال الأولي.",
                    Language = "ar"
                },
                new FAQItem
                {
                    Id = 7,
                    Category = "Arabic",
                    Question = "ما هو NCA ECC؟",
                    Answer = "NCA ECC (ضوابط الأمن السيبراني الأساسية للهيئة الوطنية للأمن السيبراني) هو إطار الأمن السيبراني الإلزامي في المملكة العربية السعودية لجميع المنظمات.",
                    Language = "ar"
                }
            };
        }
    }

    #region Data Models

    public class GlossaryData
    {
        public List<GlossaryTerm>? Terms { get; set; }
    }

    public class GlossaryTerm
    {
        public string Term { get; set; } = string.Empty;
        public string Definition { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Language { get; set; } = "en";
        public List<string>? Examples { get; set; }
    }

    public class FAQItem
    {
        public int Id { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public string Language { get; set; } = "en";
    }

    #endregion
}
