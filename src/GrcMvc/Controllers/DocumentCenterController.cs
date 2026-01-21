using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;

namespace GrcMvc.Controllers;

/// <summary>
/// Document Center Controller - Template library and document management
/// مركز المستندات - مكتبة القوالب وإدارة المستندات
/// </summary>
[Authorize]
[Route("[controller]")]
public class DocumentCenterController : Controller
{
    private readonly GrcDbContext _context;
    private readonly ILogger<DocumentCenterController> _logger;
    private readonly IWebHostEnvironment _env;
    private readonly IDocumentGenerationService _documentGenerator;

    public DocumentCenterController(
        GrcDbContext context,
        ILogger<DocumentCenterController> logger,
        IWebHostEnvironment env,
        IDocumentGenerationService documentGenerator)
    {
        _context = context;
        _logger = logger;
        _env = env;
        _documentGenerator = documentGenerator;
    }

    /// <summary>
    /// Main Document Center page
    /// </summary>
    [HttpGet]
    [HttpGet("Index")]
    public async Task<IActionResult> Index(string? category = null, string? domain = null, string? search = null)
    {
        var query = _context.DocumentTemplates.Where(t => t.IsActive);

        if (!string.IsNullOrEmpty(category))
            query = query.Where(t => t.Category == category);

        if (!string.IsNullOrEmpty(domain))
            query = query.Where(t => t.Domain == domain);

        if (!string.IsNullOrEmpty(search))
            query = query.Where(t =>
                t.TitleEn.Contains(search) ||
                (t.TitleAr != null && t.TitleAr.Contains(search)) ||
                (t.Tags != null && t.Tags.Contains(search)));

        var templates = await query
            .OrderBy(t => t.DisplayOrder)
            .ThenBy(t => t.Category)
            .ThenBy(t => t.TitleEn)
            .ToListAsync();

        // Get category counts
        var categoryCounts = await _context.DocumentTemplates
            .Where(t => t.IsActive)
            .GroupBy(t => t.Category)
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Category, x => x.Count);

        // Get domain counts
        var domainCounts = await _context.DocumentTemplates
            .Where(t => t.IsActive && t.Domain != null)
            .GroupBy(t => t.Domain!)
            .Select(g => new { Domain = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Domain, x => x.Count);

        ViewBag.Categories = categoryCounts;
        ViewBag.Domains = domainCounts;
        ViewBag.SelectedCategory = category;
        ViewBag.SelectedDomain = domain;
        ViewBag.SearchTerm = search;
        ViewBag.TotalTemplates = await _context.DocumentTemplates.CountAsync(t => t.IsActive);

        return View(templates);
    }

    /// <summary>
    /// View template details
    /// </summary>
    [HttpGet("Details/{id}")]
    public async Task<IActionResult> Details(Guid id)
    {
        var template = await _context.DocumentTemplates.FindAsync(id);
        if (template == null)
            return NotFound();

        return View(template);
    }

    /// <summary>
    /// Download template file
    /// </summary>
    [HttpGet("Download/{id}")]
    public async Task<IActionResult> Download(Guid id, string language = "en")
    {
        var template = await _context.DocumentTemplates.FindAsync(id);
        if (template == null)
            return NotFound();

        // Get file path based on language
        var filePath = language == "ar" && !string.IsNullOrEmpty(template.FilePathAr)
            ? template.FilePathAr
            : template.FilePathEn;

        if (string.IsNullOrEmpty(filePath))
        {
            // Generate document if no file exists
            return await GeneratePlaceholderTemplate(template, language);
        }

        var fullPath = Path.Combine(_env.WebRootPath, "templates", filePath);
        if (!System.IO.File.Exists(fullPath))
        {
            return await GeneratePlaceholderTemplate(template, language);
        }

        // Increment download count
        template.DownloadCount++;
        await _context.SaveChangesAsync();

        var contentType = template.FileFormat switch
        {
            "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "pdf" => "application/pdf",
            _ => "application/octet-stream"
        };

        var fileName = $"{template.Code}_{language}.{template.FileFormat}";
        return PhysicalFile(fullPath, contentType, fileName);
    }

    /// <summary>
    /// Generate a template document when actual file doesn't exist
    /// Now uses DocumentGenerationService for real document generation
    /// </summary>
    private async Task<IActionResult> GeneratePlaceholderTemplate(DocumentTemplate template, string language)
    {
        try
        {
            // Generate actual document based on format
            byte[] documentBytes;
            string contentType;
            string fileName;

            switch (template.FileFormat?.ToLower())
            {
                case "pdf":
                    documentBytes = await _documentGenerator.GeneratePdfDocumentAsync(template, language);
                    contentType = "text/html"; // HTML that can be printed to PDF
                    fileName = $"{template.Code}_{language}.html";
                    break;

                case "xlsx":
                case "xls":
                    documentBytes = await _documentGenerator.GenerateExcelDocumentAsync(template, language);
                    contentType = "text/csv";
                    fileName = $"{template.Code}_{language}.csv";
                    break;

                case "docx":
                case "doc":
                default:
                    documentBytes = await _documentGenerator.GenerateWordDocumentAsync(template, language);
                    contentType = "application/xml";
                    fileName = $"{template.Code}_{language}.xml";
                    break;
            }

            // Increment download count
            template.DownloadCount++;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Generated {Format} document for template {Code} in {Language}",
                template.FileFormat, template.Code, language);

            return File(documentBytes, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate document for template {Code}", template.Code);

            // Fallback to JSON metadata
            var content = new
            {
                Error = "Failed to generate document",
                Template = new
                {
                    template.Code,
                    Title = language == "ar" ? template.TitleAr ?? template.TitleEn : template.TitleEn,
                    template.Category,
                    template.Version
                },
                GeneratedAt = DateTime.UtcNow
            };
            return Json(content);
        }
    }

    /// <summary>
    /// API: Get all templates
    /// </summary>
    [HttpGet("api/templates")]
    public async Task<IActionResult> GetTemplates(string? category = null, string? domain = null)
    {
        var query = _context.DocumentTemplates.Where(t => t.IsActive);

        if (!string.IsNullOrEmpty(category))
            query = query.Where(t => t.Category == category);

        if (!string.IsNullOrEmpty(domain))
            query = query.Where(t => t.Domain == domain);

        var templates = await query
            .OrderBy(t => t.DisplayOrder)
            .Select(t => new
            {
                t.Id,
                t.Code,
                t.TitleEn,
                t.TitleAr,
                t.DescriptionEn,
                t.DescriptionAr,
                t.Category,
                t.Domain,
                t.FileFormat,
                t.Version,
                t.IsBilingual,
                t.DownloadCount,
                t.Tags
            })
            .ToListAsync();

        return Ok(templates);
    }

    /// <summary>
    /// API: Get template by ID
    /// </summary>
    [HttpGet("api/templates/{id}")]
    public async Task<IActionResult> GetTemplate(Guid id)
    {
        var template = await _context.DocumentTemplates.FindAsync(id);
        if (template == null)
            return NotFound();

        return Ok(template);
    }

    /// <summary>
    /// API: Get categories
    /// </summary>
    [HttpGet("api/categories")]
    public IActionResult GetCategories()
    {
        var categories = new[]
        {
            new { Code = DocumentTemplateCategories.Policy, NameEn = "Policy", NameAr = "سياسة", Icon = "fas fa-file-alt" },
            new { Code = DocumentTemplateCategories.Procedure, NameEn = "Procedure", NameAr = "إجراء", Icon = "fas fa-list-ol" },
            new { Code = DocumentTemplateCategories.Form, NameEn = "Form", NameAr = "نموذج", Icon = "fas fa-wpforms" },
            new { Code = DocumentTemplateCategories.Checklist, NameEn = "Checklist", NameAr = "قائمة مراجعة", Icon = "fas fa-tasks" },
            new { Code = DocumentTemplateCategories.Report, NameEn = "Report", NameAr = "تقرير", Icon = "fas fa-chart-bar" },
            new { Code = DocumentTemplateCategories.Agreement, NameEn = "Agreement", NameAr = "اتفاقية", Icon = "fas fa-handshake" },
            new { Code = DocumentTemplateCategories.Certificate, NameEn = "Certificate", NameAr = "شهادة", Icon = "fas fa-certificate" },
            new { Code = DocumentTemplateCategories.Guide, NameEn = "Guide", NameAr = "دليل", Icon = "fas fa-book" }
        };

        return Ok(categories);
    }

    /// <summary>
    /// API: Get domains
    /// </summary>
    [HttpGet("api/domains")]
    public IActionResult GetDomains()
    {
        var domains = new[]
        {
            new { Code = GrcDomains.Governance, NameEn = "Governance", NameAr = "الحوكمة", Icon = "fas fa-building" },
            new { Code = GrcDomains.Risk, NameEn = "Risk Management", NameAr = "إدارة المخاطر", Icon = "fas fa-exclamation-triangle" },
            new { Code = GrcDomains.Compliance, NameEn = "Compliance", NameAr = "الامتثال", Icon = "fas fa-check-circle" },
            new { Code = GrcDomains.Audit, NameEn = "Audit", NameAr = "التدقيق", Icon = "fas fa-search" },
            new { Code = GrcDomains.Security, NameEn = "Security", NameAr = "الأمن", Icon = "fas fa-shield-alt" },
            new { Code = GrcDomains.Privacy, NameEn = "Privacy", NameAr = "الخصوصية", Icon = "fas fa-user-shield" },
            new { Code = GrcDomains.Operations, NameEn = "Operations", NameAr = "العمليات", Icon = "fas fa-cogs" },
            new { Code = GrcDomains.Training, NameEn = "Training", NameAr = "التدريب", Icon = "fas fa-graduation-cap" }
        };

        return Ok(domains);
    }
}
