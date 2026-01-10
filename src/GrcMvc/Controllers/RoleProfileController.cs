using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Models.Entities.Catalogs;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GrcMvc.Controllers;

/// <summary>
/// Role Profile Controller - Manage roles, titles, and user assignments
/// </summary>
[Authorize]
[Route("[controller]")]
public class RoleProfileController : Controller
{
    private readonly GrcDbContext _db;
    private readonly IUserDirectoryService _userDirectory;
    private readonly ILogger<RoleProfileController> _logger;

    public RoleProfileController(GrcDbContext db, IUserDirectoryService userDirectory, ILogger<RoleProfileController> logger)
    {
        _db = db;
        _userDirectory = userDirectory;
        _logger = logger;
    }

    /// <summary>
    /// Role Profile Dashboard
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = new RoleProfileDashboard
        {
            Roles = await _db.RoleCatalogs.Where(r => r.IsActive).OrderBy(r => r.DisplayOrder).ToListAsync(),
            Titles = await _db.TitleCatalogs.Where(t => t.IsActive).OrderBy(t => t.DisplayOrder).ToListAsync(),
            TotalUsers = await _userDirectory.GetUserCountAsync(),
            TotalRoles = await _db.RoleCatalogs.CountAsync(r => r.IsActive),
            TotalTitles = await _db.TitleCatalogs.CountAsync(t => t.IsActive)
        };
        return View(model);
    }

    /// <summary>
    /// View all roles
    /// </summary>
    [HttpGet("Roles")]
    public async Task<IActionResult> Roles()
    {
        var roles = await _db.RoleCatalogs.Where(r => r.IsActive).OrderBy(r => r.DisplayOrder).ToListAsync();
        return View(roles);
    }

    /// <summary>
    /// View all titles
    /// </summary>
    [HttpGet("Titles")]
    public async Task<IActionResult> Titles()
    {
        var titles = await _db.TitleCatalogs.Include(t => t.RoleCatalog).Where(t => t.IsActive).OrderBy(t => t.DisplayOrder).ToListAsync();
        return View(titles);
    }

    /// <summary>
    /// My Profile - Current user's role and permissions
    /// </summary>
    [HttpGet("MyProfile")]
    public async Task<IActionResult> MyProfile()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

        var user = await _userDirectory.GetUserByIdAsync(userId);
        return View(user);
    }
}

/// <summary>
/// Assessment Template Controller
/// </summary>
[Route("[controller]")]
public class AssessmentTemplateController : Controller
{
    private readonly GrcDbContext _db;
    private readonly ILogger<AssessmentTemplateController> _logger;

    public AssessmentTemplateController(GrcDbContext db, ILogger<AssessmentTemplateController> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Assessment Templates List
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var templates = await _db.TemplateCatalogs.Where(t => t.IsActive).OrderBy(t => t.DisplayOrder).ToListAsync();
        ViewBag.TotalTemplates = templates.Count;
        return View(templates);
    }

    /// <summary>
    /// View template details
    /// </summary>
    [HttpGet("Details/{id}")]
    public async Task<IActionResult> Details(Guid id)
    {
        var template = await _db.TemplateCatalogs.FirstOrDefaultAsync(t => t.Id == id);
        if (template == null) return NotFound();
        return View(template);
    }

    /// <summary>
    /// Create new assessment from template
    /// </summary>
    [HttpGet("CreateAssessment/{templateId}")]
    public async Task<IActionResult> CreateAssessment(Guid templateId)
    {
        var template = await _db.TemplateCatalogs.FirstOrDefaultAsync(t => t.Id == templateId);
        if (template == null) return NotFound();

        ViewBag.Template = template;
        return View();
    }

    /// <summary>
    /// Submit new assessment
    /// </summary>
    [HttpPost("CreateAssessment/{templateId}")]
    public async Task<IActionResult> CreateAssessmentPost(Guid templateId, [FromForm] string name)
    {
        var template = await _db.TemplateCatalogs.FirstOrDefaultAsync(t => t.Id == templateId);
        if (template == null) return NotFound();

        var tenantId = GetCurrentTenantId();
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";

        var assessment = new Assessment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name ?? $"Assessment from {template.TemplateName}",
            Status = "Draft",
            CreatedDate = DateTime.UtcNow,
            CreatedBy = userId
        };

        _db.Assessments.Add(assessment);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Assessment '{assessment.Name}' created successfully!";
        return RedirectToAction("Index");
    }

    private Guid GetCurrentTenantId()
    {
        var claim = User.FindFirst("TenantId");
        return claim != null && Guid.TryParse(claim.Value, out var tenantId) ? tenantId : Guid.Empty;
    }
}

/// <summary>
/// Document Flow Controller - Evidence upload, review, approval
/// </summary>
[Route("[controller]")]
public class DocumentFlowController : Controller
{
    private readonly GrcDbContext _db;
    private readonly ILogger<DocumentFlowController> _logger;
    private readonly IWebHostEnvironment _env;

    public DocumentFlowController(GrcDbContext db, ILogger<DocumentFlowController> logger, IWebHostEnvironment env)
    {
        _db = db;
        _logger = logger;
        _env = env;
    }

    /// <summary>
    /// Document Flow Dashboard
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var tenantId = GetCurrentTenantId();
        var model = new DocumentFlowDashboard
        {
            PendingReview = await _db.AutoTaggedEvidences.CountAsync(e => e.TenantId == tenantId && e.Status == "Collected"),
            Approved = await _db.AutoTaggedEvidences.CountAsync(e => e.TenantId == tenantId && e.Status == "Approved"),
            Rejected = await _db.AutoTaggedEvidences.CountAsync(e => e.TenantId == tenantId && e.Status == "Rejected"),
            RecentDocuments = await _db.AutoTaggedEvidences
                .Where(e => e.TenantId == tenantId)
                .OrderByDescending(e => e.CapturedAt)
                .Take(20)
                .ToListAsync()
        };
        return View(model);
    }

    /// <summary>
    /// Upload new document
    /// </summary>
    [HttpGet("Upload")]
    public IActionResult Upload()
    {
        return View();
    }

    /// <summary>
    /// Submit document upload
    /// </summary>
    [HttpPost("Upload")]
    public async Task<IActionResult> UploadPost([FromForm] DocumentUploadDto dto)
    {
        var tenantId = GetCurrentTenantId();
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";

        // Save file
        string filePath = "";
        string fileHash = "";
        if (dto.File != null && dto.File.Length > 0)
        {
            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", tenantId.ToString());
            Directory.CreateDirectory(uploadsDir);

            var fileName = $"{Guid.NewGuid()}_{dto.File.FileName}";
            filePath = Path.Combine(uploadsDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.File.CopyToAsync(stream);
            }

            // Calculate hash - SECURITY: Use SHA256 instead of MD5
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            using (var stream = System.IO.File.OpenRead(filePath))
            {
                var hash = sha256.ComputeHash(stream);
                fileHash = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        var evidence = new AutoTaggedEvidence
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Title = dto.Title ?? "Untitled Document",
            Process = dto.Process ?? "General",
            System = dto.System ?? "Manual",
            Period = dto.Period ?? DateTime.UtcNow.ToString("yyyy-Q"),
            PeriodStart = dto.PeriodStart ?? DateTime.UtcNow.AddMonths(-3),
            PeriodEnd = dto.PeriodEnd ?? DateTime.UtcNow,
            EvidenceType = dto.EvidenceType ?? "Document",
            StorageLocation = filePath,
            FileHash = fileHash,
            Status = "Collected",
            Source = "Manual",
            CapturedAt = DateTime.UtcNow,
            CapturedBy = userId
        };

        _db.AutoTaggedEvidences.Add(evidence);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Document '{evidence.Title}' uploaded successfully!";
        return RedirectToAction("Index");
    }

    /// <summary>
    /// Review document
    /// </summary>
    [HttpGet("Review/{id}")]
    public async Task<IActionResult> Review(Guid id)
    {
        var evidence = await _db.AutoTaggedEvidences.FirstOrDefaultAsync(e => e.Id == id);
        if (evidence == null) return NotFound();
        return View(evidence);
    }

    /// <summary>
    /// Approve document
    /// </summary>
    [HttpPost("Approve/{id}")]
    public async Task<IActionResult> Approve(Guid id, [FromForm] string? notes)
    {
        var evidence = await _db.AutoTaggedEvidences.FirstOrDefaultAsync(e => e.Id == id);
        if (evidence == null) return NotFound();

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";
        evidence.Status = "Approved";
        evidence.ReviewedBy = userId;
        evidence.ReviewedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        TempData["Success"] = "Document approved!";
        return RedirectToAction("Index");
    }

    /// <summary>
    /// Reject document
    /// </summary>
    [HttpPost("Reject/{id}")]
    public async Task<IActionResult> Reject(Guid id, [FromForm] string reason)
    {
        var evidence = await _db.AutoTaggedEvidences.FirstOrDefaultAsync(e => e.Id == id);
        if (evidence == null) return NotFound();

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";
        evidence.Status = "Rejected";
        evidence.ReviewedBy = userId;
        evidence.ReviewedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        TempData["Error"] = "Document rejected: " + reason;
        return RedirectToAction("Index");
    }

    private Guid GetCurrentTenantId()
    {
        var claim = User.FindFirst("TenantId");
        return claim != null && Guid.TryParse(claim.Value, out var tenantId) ? tenantId : Guid.Empty;
    }
}

#region View Models

public class RoleProfileDashboard
{
    public List<RoleCatalog> Roles { get; set; } = new();
    public List<TitleCatalog> Titles { get; set; } = new();
    public int TotalUsers { get; set; }
    public int TotalRoles { get; set; }
    public int TotalTitles { get; set; }
}

public class DocumentFlowDashboard
{
    public int PendingReview { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
    public List<AutoTaggedEvidence> RecentDocuments { get; set; } = new();
}

public class DocumentUploadDto
{
    public string? Title { get; set; }
    public string? Process { get; set; }
    public string? System { get; set; }
    public string? Period { get; set; }
    public DateTime? PeriodStart { get; set; }
    public DateTime? PeriodEnd { get; set; }
    public string? EvidenceType { get; set; }
    public IFormFile? File { get; set; }
}

#endregion
