using System;
using System.Linq;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Models.Entities.EmailOperations;
using GrcMvc.Services.EmailOperations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Controllers;

/// <summary>
/// Email Operations Dashboard and Management
/// For both Shahin and Dogan Consult
/// </summary>
[Authorize]
[Route("email-ops")]
public class EmailOperationsController : Controller
{
    private readonly GrcDbContext _db;
    private readonly ILogger<EmailOperationsController> _logger;

    public EmailOperationsController(
        GrcDbContext db,
        ILogger<EmailOperationsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Main dashboard
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(string? brand = null)
    {
        var query = _db.Set<EmailThread>().AsQueryable();

        if (!string.IsNullOrEmpty(brand))
        {
            query = query.Where(t => t.Mailbox != null && t.Mailbox.Brand == brand);
        }

        var now = DateTime.UtcNow;
        var today = now.Date;

        var dashboard = new EmailDashboardViewModel
        {
            Brand = brand,
            TotalThreads = await query.CountAsync(),
            NewThreads = await query.CountAsync(t => t.Status == EmailThreadStatus.New),
            InProgressThreads = await query.CountAsync(t => t.Status == EmailThreadStatus.InProgress),
            AwaitingReplyThreads = await query.CountAsync(t => t.Status == EmailThreadStatus.AwaitingCustomerReply),
            DraftsPending = await query.CountAsync(t => t.Status == EmailThreadStatus.DraftPending),
            ResolvedToday = await query.CountAsync(t => t.ResolvedAt != null && t.ResolvedAt.Value.Date == today),
            SlaBreachedCount = await query.CountAsync(t => t.SlaFirstResponseBreached || t.SlaResolutionBreached),
            PendingTasks = await _db.Set<EmailTask>().CountAsync(t => t.Status == EmailTaskStatus.Pending),

            RecentThreads = await query
                .OrderByDescending(t => t.CreatedAt)
                .Take(10)
                .Select(t => new EmailThreadSummary
                {
                    Id = t.Id,
                    Subject = t.Subject,
                    FromEmail = t.FromEmail,
                    FromName = t.FromName,
                    Classification = t.Classification,
                    Status = t.Status,
                    Priority = t.Priority,
                    ReceivedAt = t.ReceivedAt,
                    SlaBreached = t.SlaFirstResponseBreached || t.SlaResolutionBreached
                })
                .ToListAsync(),

            Mailboxes = await _db.Set<EmailMailbox>()
                .Where(m => brand == null || m.Brand == brand)
                .Select(m => new MailboxSummary
                {
                    Id = m.Id,
                    EmailAddress = m.EmailAddress,
                    DisplayName = m.DisplayName,
                    Brand = m.Brand,
                    IsActive = m.IsActive,
                    LastSyncAt = m.LastSyncAt
                })
                .ToListAsync()
        };

        return View(dashboard);
    }

    /// <summary>
    /// View all threads
    /// </summary>
    [HttpGet("threads")]
    public async Task<IActionResult> Threads(
        string? brand = null,
        EmailThreadStatus? status = null,
        EmailClassification? classification = null,
        int page = 1)
    {
        var query = _db.Set<EmailThread>()
            .Include(t => t.Mailbox)
            .AsQueryable();

        if (!string.IsNullOrEmpty(brand))
            query = query.Where(t => t.Mailbox != null && t.Mailbox.Brand == brand);
        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);
        if (classification.HasValue)
            query = query.Where(t => t.Classification == classification.Value);

        var pageSize = 20;
        var total = await query.CountAsync();

        var threads = await query
            .OrderByDescending(t => t.Priority)
            .ThenByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new EmailThreadSummary
            {
                Id = t.Id,
                Subject = t.Subject,
                FromEmail = t.FromEmail,
                FromName = t.FromName,
                Classification = t.Classification,
                Status = t.Status,
                Priority = t.Priority,
                ReceivedAt = t.ReceivedAt,
                FirstResponseAt = t.FirstResponseAt,
                SlaBreached = t.SlaFirstResponseBreached || t.SlaResolutionBreached,
                MailboxEmail = t.Mailbox != null ? t.Mailbox.EmailAddress : null,
                AssignedTo = t.AssignedToUserName
            })
            .ToListAsync();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
        ViewBag.Brand = brand;
        ViewBag.Status = status;
        ViewBag.Classification = classification;

        return View(threads);
    }

    /// <summary>
    /// View single thread with messages
    /// </summary>
    [HttpGet("threads/{id}")]
    public async Task<IActionResult> Thread(Guid id)
    {
        var thread = await _db.Set<EmailThread>()
            .Include(t => t.Mailbox)
            .Include(t => t.Messages.OrderBy(m => m.ReceivedAt))
            .Include(t => t.Tasks)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (thread == null)
            return NotFound();

        return View(thread);
    }

    /// <summary>
    /// View and manage mailboxes
    /// </summary>
    [HttpGet("mailboxes")]
    public async Task<IActionResult> Mailboxes(string? brand = null)
    {
        var query = _db.Set<EmailMailbox>().AsQueryable();

        if (!string.IsNullOrEmpty(brand))
            query = query.Where(m => m.Brand == brand);

        var mailboxes = await query.ToListAsync();
        ViewBag.Brand = brand;

        return View(mailboxes);
    }

    /// <summary>
    /// Create/Edit mailbox form
    /// </summary>
    [HttpGet("mailboxes/new")]
    [HttpGet("mailboxes/{id}/edit")]
    public async Task<IActionResult> EditMailbox(Guid? id)
    {
        EmailMailbox mailbox;

        if (id.HasValue)
        {
            mailbox = await _db.Set<EmailMailbox>().FindAsync(id.Value)
                ?? throw new Exception("Mailbox not found");
        }
        else
        {
            mailbox = new EmailMailbox();
        }

        return View(mailbox);
    }

    /// <summary>
    /// Save mailbox
    /// </summary>
    [HttpPost("mailboxes/save")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveMailbox(EmailMailbox model)
    {
        if (!ModelState.IsValid)
            return View("EditMailbox", model);

        if (model.Id == Guid.Empty)
        {
            model.Id = Guid.NewGuid();
            model.CreatedAt = DateTime.UtcNow;
            _db.Set<EmailMailbox>().Add(model);
        }
        else
        {
            var existing = await _db.Set<EmailMailbox>().FindAsync(model.Id);
            if (existing == null)
                return NotFound();

            existing.EmailAddress = model.EmailAddress;
            existing.DisplayName = model.DisplayName;
            existing.Brand = model.Brand;
            existing.Purpose = model.Purpose;
            existing.GraphUserId = model.GraphUserId;
            existing.ClientId = model.ClientId;
            existing.TenantId = model.TenantId;
            existing.IsActive = model.IsActive;
            existing.AutoReplyEnabled = model.AutoReplyEnabled;
            existing.DraftModeDefault = model.DraftModeDefault;
            existing.SlaFirstResponseHours = model.SlaFirstResponseHours;
            existing.SlaResolutionHours = model.SlaResolutionHours;
            existing.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(model.EncryptedClientSecret))
            {
                existing.EncryptedClientSecret = model.EncryptedClientSecret;
            }
        }

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Mailboxes));
    }

    /// <summary>
    /// View pending tasks
    /// </summary>
    [HttpGet("tasks")]
    public async Task<IActionResult> Tasks(EmailTaskStatus? status = null)
    {
        var query = _db.Set<EmailTask>()
            .Include(t => t.Thread)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);
        else
            query = query.Where(t => t.Status == EmailTaskStatus.Pending || t.Status == EmailTaskStatus.InProgress);

        var tasks = await query
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.DueAt)
            .Take(50)
            .ToListAsync();

        return View(tasks);
    }

    /// <summary>
    /// Send a pending draft
    /// </summary>
    [HttpPost("threads/{threadId}/send-draft/{messageId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendDraft(Guid threadId, Guid messageId)
    {
        // Implementation would use IEmailOperationsService
        // For now, just update status
        var message = await _db.Set<EmailMessage>().FindAsync(messageId);
        if (message == null)
            return NotFound();

        message.Status = EmailMessageStatus.Sent;
        message.SentAt = DateTime.UtcNow;
        message.ApprovedByUserName = User.Identity?.Name;
        message.ApprovedAt = DateTime.UtcNow;

        var thread = await _db.Set<EmailThread>().FindAsync(threadId);
        if (thread != null)
        {
            thread.Status = EmailThreadStatus.AwaitingCustomerReply;
            if (thread.FirstResponseAt == null)
                thread.FirstResponseAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Thread), new { id = threadId });
    }

    /// <summary>
    /// Add a new mailbox (quick add from modal)
    /// </summary>
    [HttpPost("mailboxes/add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMailbox(
        string emailAddress,
        string displayName,
        string brand,
        string? graphUserId = null,
        bool autoReplyEnabled = false,
        bool draftModeDefault = true,
        int defaultSlaHours = 24)
    {
        var mailbox = new EmailMailbox
        {
            Id = Guid.NewGuid(),
            EmailAddress = emailAddress,
            DisplayName = displayName,
            Brand = brand,
            GraphUserId = graphUserId,
            IsActive = true,
            AutoReplyEnabled = autoReplyEnabled,
            DraftModeDefault = draftModeDefault,
            DefaultSlaHours = defaultSlaHours,
            SlaFirstResponseHours = defaultSlaHours,
            SlaResolutionHours = defaultSlaHours * 3,
            CreatedAt = DateTime.UtcNow
        };

        _db.Set<EmailMailbox>().Add(mailbox);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Added new mailbox: {Email} for {Brand}", emailAddress, brand);

        TempData["Success"] = $"تم إضافة صندوق البريد {emailAddress} بنجاح";
        return RedirectToAction(nameof(Mailboxes));
    }

    /// <summary>
    /// Toggle mailbox active status
    /// </summary>
    [HttpPost("mailboxes/{id}/toggle")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleMailbox(Guid id)
    {
        var mailbox = await _db.Set<EmailMailbox>().FindAsync(id);
        if (mailbox == null)
            return NotFound();

        mailbox.IsActive = !mailbox.IsActive;
        mailbox.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Toggled mailbox {Email} to {Status}", 
            mailbox.EmailAddress, mailbox.IsActive ? "active" : "inactive");

        return RedirectToAction(nameof(Mailboxes));
    }

    /// <summary>
    /// Sync mailbox with Microsoft Graph
    /// </summary>
    [HttpGet("mailboxes/{id}/sync")]
    public async Task<IActionResult> SyncMailbox(Guid id)
    {
        var mailbox = await _db.Set<EmailMailbox>().FindAsync(id);
        if (mailbox == null)
            return NotFound();

        try
        {
            // Get the service from DI
            var emailService = HttpContext.RequestServices.GetService<IEmailOperationsService>();
            if (emailService != null)
            {
                await emailService.SyncMailboxAsync(id);
                TempData["Success"] = $"تمت مزامنة {mailbox.EmailAddress} بنجاح";
            }
            else
            {
                TempData["Error"] = "خدمة البريد غير متاحة";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync mailbox {Id}", id);
            TempData["Error"] = "An error occurred. Please try again.";
        }

        return RedirectToAction(nameof(Mailboxes));
    }
}

// View Models
public class EmailDashboardViewModel
{
    public string? Brand { get; set; }
    public int TotalThreads { get; set; }
    public int NewThreads { get; set; }
    public int InProgressThreads { get; set; }
    public int AwaitingReplyThreads { get; set; }
    public int DraftsPending { get; set; }
    public int ResolvedToday { get; set; }
    public int SlaBreachedCount { get; set; }
    public int PendingTasks { get; set; }
    public System.Collections.Generic.List<EmailThreadSummary> RecentThreads { get; set; } = new();
    public System.Collections.Generic.List<MailboxSummary> Mailboxes { get; set; } = new();
}

public class EmailThreadSummary
{
    public Guid Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string? FromName { get; set; }
    public EmailClassification Classification { get; set; }
    public EmailThreadStatus Status { get; set; }
    public EmailPriority Priority { get; set; }
    public DateTime ReceivedAt { get; set; }
    public DateTime? FirstResponseAt { get; set; }
    public bool SlaBreached { get; set; }
    public string? MailboxEmail { get; set; }
    public string? AssignedTo { get; set; }
}

public class MailboxSummary
{
    public Guid Id { get; set; }
    public string EmailAddress { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? LastSyncAt { get; set; }
}
