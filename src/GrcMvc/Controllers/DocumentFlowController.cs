using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Application.Permissions;
using GrcMvc.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GrcMvc.Controllers;

/// <summary>
/// Document Flow Controller - Evidence upload, review, approval
/// Manages document lifecycle from upload to approval/rejection
/// </summary>
[Authorize]
[RequireTenant]
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
    [Authorize(GrcPermissions.Evidence.View)]
    public async Task<IActionResult> Index()
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading document flow dashboard");
            TempData["Error"] = "Error loading dashboard";
            return View(new DocumentFlowDashboard());
        }
    }

    /// <summary>
    /// Upload new document
    /// </summary>
    [HttpGet("Upload")]
    [Authorize(GrcPermissions.Evidence.Upload)]
    public IActionResult Upload()
    {
        return View();
    }

    /// <summary>
    /// Submit document upload
    /// </summary>
    [HttpPost("Upload")]
    [ValidateAntiForgeryToken]
    [Authorize(GrcPermissions.Evidence.Upload)]
    public async Task<IActionResult> UploadPost([FromForm] DocumentUploadDto dto)
    {
        try
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

                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(dto.File.FileName)}";
                filePath = Path.Combine(uploadsDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.File.CopyToAsync(stream);
                }

                // Calculate hash using SHA256 for security
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

            _logger.LogInformation("Document {EvidenceId} uploaded by user {UserId}", evidence.Id, userId);
            TempData["Success"] = $"Document '{evidence.Title}' uploaded successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document");
            TempData["Error"] = "Error uploading document";
            return RedirectToAction(nameof(Upload));
        }
    }

    /// <summary>
    /// Review document
    /// </summary>
    [HttpGet("Review/{id}")]
    [Authorize(GrcPermissions.Evidence.Review)]
    public async Task<IActionResult> Review(Guid id)
    {
        try
        {
            var evidence = await _db.AutoTaggedEvidences.FirstOrDefaultAsync(e => e.Id == id);
            if (evidence == null)
            {
                TempData["Error"] = "Document not found";
                return RedirectToAction(nameof(Index));
            }
            return View(evidence);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading document {DocumentId} for review", id);
            TempData["Error"] = "Error loading document";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Approve document
    /// </summary>
    [HttpPost("Approve/{id}")]
    [ValidateAntiForgeryToken]
    [Authorize(GrcPermissions.Evidence.Approve)]
    public async Task<IActionResult> Approve(Guid id, [FromForm] string? notes)
    {
        try
        {
            var evidence = await _db.AutoTaggedEvidences.FirstOrDefaultAsync(e => e.Id == id);
            if (evidence == null)
            {
                TempData["Error"] = "Document not found";
                return RedirectToAction(nameof(Index));
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";
            evidence.Status = "Approved";
            evidence.ReviewedBy = userId;
            evidence.ReviewedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            _logger.LogInformation("Document {EvidenceId} approved by user {UserId}", id, userId);
            TempData["Success"] = "Document approved!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving document {DocumentId}", id);
            TempData["Error"] = "Error approving document";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Reject document
    /// </summary>
    [HttpPost("Reject/{id}")]
    [ValidateAntiForgeryToken]
    [Authorize(GrcPermissions.Evidence.Approve)]
    public async Task<IActionResult> Reject(Guid id, [FromForm] string reason)
    {
        try
        {
            var evidence = await _db.AutoTaggedEvidences.FirstOrDefaultAsync(e => e.Id == id);
            if (evidence == null)
            {
                TempData["Error"] = "Document not found";
                return RedirectToAction(nameof(Index));
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";
            evidence.Status = "Rejected";
            evidence.ReviewedBy = userId;
            evidence.ReviewedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            _logger.LogInformation("Document {EvidenceId} rejected by user {UserId}. Reason: {Reason}", id, userId, reason);
            TempData["Error"] = "Document rejected: " + reason;
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting document {DocumentId}", id);
            TempData["Error"] = "Error rejecting document";
            return RedirectToAction(nameof(Index));
        }
    }

    private Guid GetCurrentTenantId()
    {
        var claim = User.FindFirst("TenantId");
        return claim != null && Guid.TryParse(claim.Value, out var tenantId) ? tenantId : Guid.Empty;
    }
}

#region View Models

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
