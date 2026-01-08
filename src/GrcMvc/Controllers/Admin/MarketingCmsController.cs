using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GrcMvc.Data;
using GrcMvc.Models.Entities.Marketing;
using GrcMvc.Application.Permissions;

namespace GrcMvc.Controllers.Admin;

/// <summary>
/// Admin CMS Controller for managing marketing content
/// متحكم إدارة المحتوى التسويقي
/// </summary>
[Authorize(Roles = "PlatformAdmin,Admin")]
[Route("admin/cms")]
public class MarketingCmsController : Controller
{
    private readonly GrcDbContext _context;
    private readonly ILogger<MarketingCmsController> _logger;

    public MarketingCmsController(GrcDbContext context, ILogger<MarketingCmsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// CMS Dashboard
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var stats = new CmsDashboardStats
        {
            TestimonialCount = await _context.Testimonials.CountAsync(),
            CaseStudyCount = await _context.CaseStudies.CountAsync(),
            BlogPostCount = await _context.BlogPosts.CountAsync(),
            FaqCount = await _context.Faqs.CountAsync(),
            ClientLogoCount = await _context.ClientLogos.CountAsync(),
            TrustBadgeCount = await _context.TrustBadges.CountAsync(),
            PartnerCount = await _context.Partners.CountAsync(),
            WebinarCount = await _context.Webinars.CountAsync()
        };

        return View(stats);
    }

    // ========== TESTIMONIALS ==========

    [HttpGet("testimonials")]
    public async Task<IActionResult> Testimonials()
    {
        var items = await _context.Testimonials
            .OrderBy(t => t.DisplayOrder)
            .ToListAsync();
        return View(items);
    }

    [HttpGet("testimonials/create")]
    public IActionResult CreateTestimonial() => View(new Testimonial());

    [HttpPost("testimonials/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTestimonial(Testimonial model)
    {
        if (!ModelState.IsValid) return View(model);

        model.Id = Guid.NewGuid();
        model.CreatedDate = DateTime.UtcNow;
        _context.Testimonials.Add(model);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Testimonial created successfully";
        return RedirectToAction(nameof(Testimonials));
    }

    [HttpGet("testimonials/edit/{id}")]
    public async Task<IActionResult> EditTestimonial(Guid id)
    {
        var item = await _context.Testimonials.FindAsync(id);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost("testimonials/edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTestimonial(Guid id, Testimonial model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);

        _context.Update(model);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Testimonial updated successfully";
        return RedirectToAction(nameof(Testimonials));
    }

    [HttpPost("testimonials/delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTestimonial(Guid id)
    {
        var item = await _context.Testimonials.FindAsync(id);
        if (item != null)
        {
            _context.Testimonials.Remove(item);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Testimonial deleted successfully";
        }
        return RedirectToAction(nameof(Testimonials));
    }

    // ========== CASE STUDIES ==========

    [HttpGet("case-studies")]
    public async Task<IActionResult> CaseStudies()
    {
        var items = await _context.CaseStudies
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();
        return View(items);
    }

    [HttpGet("case-studies/create")]
    public IActionResult CreateCaseStudy() => View(new CaseStudy());

    [HttpPost("case-studies/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCaseStudy(CaseStudy model)
    {
        if (!ModelState.IsValid) return View(model);

        model.Id = Guid.NewGuid();
        model.CreatedDate = DateTime.UtcNow;
        _context.CaseStudies.Add(model);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Case study created successfully";
        return RedirectToAction(nameof(CaseStudies));
    }

    [HttpPost("case-studies/delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCaseStudy(Guid id)
    {
        var item = await _context.CaseStudies.FindAsync(id);
        if (item != null)
        {
            _context.CaseStudies.Remove(item);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Case study deleted successfully";
        }
        return RedirectToAction(nameof(CaseStudies));
    }

    // ========== FAQS ==========

    [HttpGet("faqs")]
    public async Task<IActionResult> Faqs()
    {
        var items = await _context.Faqs
            .OrderBy(f => f.Category)
            .ThenBy(f => f.DisplayOrder)
            .ToListAsync();
        return View(items);
    }

    [HttpGet("faqs/create")]
    public IActionResult CreateFaq() => View(new Faq());

    [HttpPost("faqs/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateFaq(Faq model)
    {
        if (!ModelState.IsValid) return View(model);

        model.Id = Guid.NewGuid();
        model.CreatedDate = DateTime.UtcNow;
        _context.Faqs.Add(model);
        await _context.SaveChangesAsync();

        TempData["Success"] = "FAQ created successfully";
        return RedirectToAction(nameof(Faqs));
    }

    [HttpPost("faqs/delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteFaq(Guid id)
    {
        var item = await _context.Faqs.FindAsync(id);
        if (item != null)
        {
            _context.Faqs.Remove(item);
            await _context.SaveChangesAsync();
            TempData["Success"] = "FAQ deleted successfully";
        }
        return RedirectToAction(nameof(Faqs));
    }

    // ========== CLIENT LOGOS ==========

    [HttpGet("client-logos")]
    public async Task<IActionResult> ClientLogos()
    {
        var items = await _context.ClientLogos
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();
        return View(items);
    }

    [HttpGet("client-logos/create")]
    public IActionResult CreateClientLogo() => View(new ClientLogo());

    [HttpPost("client-logos/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateClientLogo(ClientLogo model)
    {
        if (!ModelState.IsValid) return View(model);

        model.Id = Guid.NewGuid();
        model.CreatedDate = DateTime.UtcNow;
        _context.ClientLogos.Add(model);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Client logo created successfully";
        return RedirectToAction(nameof(ClientLogos));
    }

    [HttpPost("client-logos/delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteClientLogo(Guid id)
    {
        var item = await _context.ClientLogos.FindAsync(id);
        if (item != null)
        {
            _context.ClientLogos.Remove(item);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Client logo deleted successfully";
        }
        return RedirectToAction(nameof(ClientLogos));
    }

    // ========== TRUST BADGES ==========

    [HttpGet("trust-badges")]
    public async Task<IActionResult> TrustBadges()
    {
        var items = await _context.TrustBadges
            .OrderBy(t => t.DisplayOrder)
            .ToListAsync();
        return View(items);
    }

    [HttpGet("trust-badges/create")]
    public IActionResult CreateTrustBadge() => View(new TrustBadge());

    [HttpPost("trust-badges/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTrustBadge(TrustBadge model)
    {
        if (!ModelState.IsValid) return View(model);

        model.Id = Guid.NewGuid();
        model.CreatedDate = DateTime.UtcNow;
        _context.TrustBadges.Add(model);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Trust badge created successfully";
        return RedirectToAction(nameof(TrustBadges));
    }

    [HttpPost("trust-badges/delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTrustBadge(Guid id)
    {
        var item = await _context.TrustBadges.FindAsync(id);
        if (item != null)
        {
            _context.TrustBadges.Remove(item);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Trust badge deleted successfully";
        }
        return RedirectToAction(nameof(TrustBadges));
    }

    // ========== PARTNERS ==========

    [HttpGet("partners")]
    public async Task<IActionResult> Partners()
    {
        var items = await _context.Partners
            .OrderBy(p => p.DisplayOrder)
            .ToListAsync();
        return View(items);
    }

    [HttpGet("partners/create")]
    public IActionResult CreatePartner() => View(new Partner());

    [HttpPost("partners/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePartner(Partner model)
    {
        if (!ModelState.IsValid) return View(model);

        model.Id = Guid.NewGuid();
        model.CreatedDate = DateTime.UtcNow;
        _context.Partners.Add(model);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Partner created successfully";
        return RedirectToAction(nameof(Partners));
    }

    [HttpPost("partners/delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePartner(Guid id)
    {
        var item = await _context.Partners.FindAsync(id);
        if (item != null)
        {
            _context.Partners.Remove(item);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Partner deleted successfully";
        }
        return RedirectToAction(nameof(Partners));
    }

    // ========== BLOG POSTS ==========

    [HttpGet("blog")]
    public async Task<IActionResult> BlogPosts()
    {
        var items = await _context.BlogPosts
            .OrderByDescending(b => b.CreatedDate)
            .ToListAsync();
        return View(items);
    }

    [HttpGet("blog/create")]
    public IActionResult CreateBlogPost() => View(new BlogPost());

    [HttpPost("blog/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBlogPost(BlogPost model)
    {
        if (!ModelState.IsValid) return View(model);

        model.Id = Guid.NewGuid();
        model.CreatedDate = DateTime.UtcNow;
        if (string.IsNullOrEmpty(model.Slug))
        {
            model.Slug = GenerateSlug(model.Title);
        }
        _context.BlogPosts.Add(model);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Blog post created successfully";
        return RedirectToAction(nameof(BlogPosts));
    }

    [HttpPost("blog/delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteBlogPost(Guid id)
    {
        var item = await _context.BlogPosts.FindAsync(id);
        if (item != null)
        {
            _context.BlogPosts.Remove(item);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Blog post deleted successfully";
        }
        return RedirectToAction(nameof(BlogPosts));
    }

    // ========== LANDING STATISTICS ==========

    [HttpGet("statistics")]
    public async Task<IActionResult> Statistics()
    {
        var items = await _context.LandingStatistics
            .OrderBy(s => s.DisplayOrder)
            .ToListAsync();
        return View(items);
    }

    [HttpGet("statistics/create")]
    public IActionResult CreateStatistic() => View(new LandingStatistic());

    [HttpPost("statistics/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateStatistic(LandingStatistic model)
    {
        if (!ModelState.IsValid) return View(model);

        model.Id = Guid.NewGuid();
        model.CreatedDate = DateTime.UtcNow;
        _context.LandingStatistics.Add(model);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Statistic created successfully";
        return RedirectToAction(nameof(Statistics));
    }

    [HttpPost("statistics/delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteStatistic(Guid id)
    {
        var item = await _context.LandingStatistics.FindAsync(id);
        if (item != null)
        {
            _context.LandingStatistics.Remove(item);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Statistic deleted successfully";
        }
        return RedirectToAction(nameof(Statistics));
    }

    // ========== API ENDPOINTS ==========

    /// <summary>
    /// API: Seed all marketing data
    /// </summary>
    [HttpPost("api/seed")]
    public async Task<IActionResult> SeedMarketingData()
    {
        try
        {
            await Data.Seeds.MarketingSeeds.SeedMarketingDataAsync(_context);
            return Ok(new { success = true, message = "Marketing data seeded successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding marketing data");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    // ========== HELPERS ==========

    private static string GenerateSlug(string title)
    {
        if (string.IsNullOrEmpty(title)) return Guid.NewGuid().ToString("N")[..8];

        var slug = title.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("'", "")
            .Replace("\"", "")
            .Replace(".", "")
            .Replace(",", "");

        // Remove non-ASCII characters
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");

        return slug.Length > 100 ? slug[..100] : slug;
    }
}

// ========== VIEW MODELS ==========

public class CmsDashboardStats
{
    public int TestimonialCount { get; set; }
    public int CaseStudyCount { get; set; }
    public int BlogPostCount { get; set; }
    public int FaqCount { get; set; }
    public int ClientLogoCount { get; set; }
    public int TrustBadgeCount { get; set; }
    public int PartnerCount { get; set; }
    public int WebinarCount { get; set; }
}
