using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using GrcMvc.Models;
using GrcMvc.Data;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace GrcMvc.Controllers;

/// <summary>
/// Home/Landing Page Controller
/// PUBLIC: Landing page accessible without login
/// </summary>
[AllowAnonymous]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly GrcDbContext _dbContext;
    private readonly ITenantContextService _tenantContextService;

    public HomeController(
        ILogger<HomeController> logger,
        GrcDbContext dbContext,
        ITenantContextService tenantContextService)
    {
        _logger = logger;
        _dbContext = dbContext;
        _tenantContextService = tenantContextService;
    }

    public IActionResult Index()
    {
        // Check if user is authenticated and should see welcome tour
        if (User.Identity?.IsAuthenticated == true)
        {
            // Check if tour has been completed (stored in session or user preference)
            var tourCompleted = HttpContext.Session.GetString("TourCompleted");
            if (string.IsNullOrEmpty(tourCompleted))
            {
                ViewBag.ShowWelcomeTour = true;
            }
        }

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Reports()
    {
        return View();
    }

    public IActionResult Admin()
    {
        return View();
    }

    public IActionResult ManageTenants()
    {
        // Return empty list - will be populated by JavaScript/API
        return View(new List<GrcMvc.Models.DTOs.TenantDto>());
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    /// <summary>
    /// Set language/culture and persist to cookie
    /// </summary>
    [HttpGet]
    public IActionResult SetLanguage(string culture, string returnUrl)
    {
        if (!string.IsNullOrEmpty(culture))
        {
            // Use the same cookie name as configured in Program.cs
            Response.Cookies.Append(
                "GrcMvc.Culture",
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax
                }
            );
        }

        return LocalRedirect(returnUrl ?? "/");
    }

    /// <summary>
    /// Global search across all entity types
    /// </summary>
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Search(string q)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
        {
            ViewBag.Query = q;
            ViewBag.Results = new List<SearchResult>();
            return View();
        }

        var results = new List<SearchResult>();
        var tenantId = _tenantContextService.IsAuthenticated() ? _tenantContextService.GetCurrentTenantId() : (Guid?)null;

        try
        {
            // Search Risks
            var risks = await _dbContext.Risks
                .Where(r => (tenantId == null || r.TenantId == tenantId) && r.DeletedAt == null &&
                       (r.Name.Contains(q) || r.Description.Contains(q)))
                .Take(10)
                .Select(r => new SearchResult
                {
                    EntityType = "Risk",
                    Title = r.Name,
                    Description = r.Description ?? "",
                    Url = $"/Risk/Details/{r.Id}",
                    Icon = "bi-exclamation-triangle",
                    BadgeColor = "danger"
                })
                .ToListAsync();
            results.AddRange(risks);

            // Search Controls
            var controls = await _dbContext.Controls
                .Where(c => (tenantId == null || c.TenantId == tenantId) && c.DeletedAt == null &&
                       (c.Name.Contains(q) || c.Description.Contains(q) || c.ControlId.Contains(q)))
                .Take(10)
                .Select(c => new SearchResult
                {
                    EntityType = "Control",
                    Title = c.Name,
                    Description = c.Description ?? "",
                    Url = $"/Control/Details/{c.Id}",
                    Icon = "bi-shield-check",
                    BadgeColor = "success"
                })
                .ToListAsync();
            results.AddRange(controls);

            // Search Policies
            var policies = await _dbContext.Policies
                .Where(p => (tenantId == null || p.TenantId == tenantId) && p.DeletedAt == null &&
                       (p.Title.Contains(q) || p.Content.Contains(q) || p.PolicyNumber.Contains(q)))
                .Take(10)
                .Select(p => new SearchResult
                {
                    EntityType = "Policy",
                    Title = p.Title,
                    Description = p.PolicyNumber,
                    Url = $"/Policy/Details/{p.Id}",
                    Icon = "bi-file-text",
                    BadgeColor = "primary"
                })
                .ToListAsync();
            results.AddRange(policies);

            // Search Evidence
            var evidence = await _dbContext.Evidences
                .Where(e => (tenantId == null || e.TenantId == tenantId) && !e.IsDeleted &&
                       (e.Title.Contains(q) || e.Description.Contains(q) || e.EvidenceNumber.Contains(q)))
                .Take(10)
                .Select(e => new SearchResult
                {
                    EntityType = "Evidence",
                    Title = e.Title,
                    Description = e.EvidenceNumber,
                    Url = $"/Evidence/Details/{e.Id}",
                    Icon = "bi-file-earmark-check",
                    BadgeColor = "info"
                })
                .ToListAsync();
            results.AddRange(evidence);

            // Search Audits
            var audits = await _dbContext.Audits
                .Where(a => (tenantId == null || a.TenantId == tenantId) && a.DeletedAt == null &&
                       (a.Title.Contains(q) || a.Scope.Contains(q)))
                .Take(10)
                .Select(a => new SearchResult
                {
                    EntityType = "Audit",
                    Title = a.Title,
                    Description = a.AuditNumber,
                    Url = $"/Audit/Details/{a.Id}",
                    Icon = "bi-search",
                    BadgeColor = "warning"
                })
                .ToListAsync();
            results.AddRange(audits);

            // Search Assessments
            var assessments = await _dbContext.Assessments
                .Where(a => (tenantId == null || a.TenantId == tenantId) && a.DeletedAt == null &&
                       (a.Name.Contains(q) || a.Description.Contains(q)))
                .Take(10)
                .Select(a => new SearchResult
                {
                    EntityType = "Assessment",
                    Title = a.Name,
                    Description = a.Description ?? "",
                    Url = $"/Assessment/Details/{a.Id}",
                    Icon = "bi-clipboard-check",
                    BadgeColor = "secondary"
                })
                .ToListAsync();
            results.AddRange(assessments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing global search for query: {Query}", q);
        }

        ViewBag.Query = q;
        ViewBag.Results = results;
        return View();
    }
}

/// <summary>
/// Search result model
/// </summary>
public class SearchResult
{
    public string EntityType { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Url { get; set; } = "";
    public string Icon { get; set; } = "bi-file";
    public string BadgeColor { get; set; } = "secondary";
}
