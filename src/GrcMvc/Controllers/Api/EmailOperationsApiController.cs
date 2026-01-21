using System;
using System.Linq;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Models.Entities.EmailOperations;
using GrcMvc.Services.EmailOperations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Microsoft.Extensions.Localization;
using GrcMvc.Resources;
namespace GrcMvc.Controllers.Api;

/// <summary>
/// API endpoints for Email Operations
/// </summary>
[ApiController]
[Route("api/email-operations")]
[Authorize]
public class EmailOperationsApiController : ControllerBase
{
    private readonly GrcDbContext _db;
    private readonly IMicrosoftGraphEmailService _graphService;
    private readonly ILogger<EmailOperationsApiController> _logger;
    private readonly IConfiguration _configuration;

    public EmailOperationsApiController(
        GrcDbContext db,
        IMicrosoftGraphEmailService graphService,
        ILogger<EmailOperationsApiController> logger,
        IConfiguration configuration)
    {
        _db = db;
        _graphService = graphService;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Discover Graph User ID for an email address
    /// </summary>
    [HttpGet("discover-user")]
    [AllowAnonymous] // For the modal to work
    public async Task<IActionResult> DiscoverUser([FromQuery] string email)
    {
        if (string.IsNullOrEmpty(email))
            return BadRequest(new { error = "Email is required" });

        try
        {
            var users = await _graphService.GetUsersAsync(email);
            var user = users.FirstOrDefault();

            if (user != null)
            {
                return Ok(new 
                { 
                    userId = user.Id, 
                    displayName = user.DisplayName,
                    email = user.EmailAddress 
                });
            }

            return NotFound(new { error = "User not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to discover user for {Email}", email);
            return StatusCode(500, new { error = "An internal error occurred. Please try again later." });
        }
    }

    /// <summary>
    /// Get dashboard stats
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard([FromQuery] string? brand = null)
    {
        var query = _db.EmailThreads.AsQueryable();

        if (!string.IsNullOrEmpty(brand))
            query = query.Where(t => t.Mailbox != null && t.Mailbox.Brand == brand);

        var stats = new
        {
            totalThreads = await query.CountAsync(),
            newThreads = await query.CountAsync(t => t.Status == EmailThreadStatus.New),
            inProgress = await query.CountAsync(t => t.Status == EmailThreadStatus.InProgress),
            awaitingReply = await query.CountAsync(t => t.Status == EmailThreadStatus.AwaitingCustomerReply),
            draftsPending = await query.CountAsync(t => t.Status == EmailThreadStatus.DraftPending),
            slaBreached = await query.CountAsync(t => t.SlaFirstResponseBreached || t.SlaResolutionBreached),
            pendingTasks = await _db.EmailTasks.CountAsync(t => t.Status == EmailTaskStatus.Pending)
        };

        return Ok(stats);
    }

    /// <summary>
    /// Get mailboxes list
    /// </summary>
    [HttpGet("mailboxes")]
    public async Task<IActionResult> GetMailboxes([FromQuery] string? brand = null)
    {
        var query = _db.EmailMailboxes.AsQueryable();

        if (!string.IsNullOrEmpty(brand))
            query = query.Where(m => m.Brand == brand);

        var mailboxes = await query
            .Select(m => new
            {
                m.Id,
                m.EmailAddress,
                m.DisplayName,
                m.Brand,
                m.IsActive,
                m.AutoReplyEnabled,
                m.DraftModeDefault,
                m.LastSyncAt,
                threadCount = m.Threads.Count
            })
            .ToListAsync();

        return Ok(mailboxes);
    }

    /// <summary>
    /// Get recent threads
    /// </summary>
    [HttpGet("threads")]
    public async Task<IActionResult> GetThreads(
        [FromQuery] string? brand = null,
        [FromQuery] EmailThreadStatus? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _db.EmailThreads.Include(t => t.Mailbox).AsQueryable();

        if (!string.IsNullOrEmpty(brand))
            query = query.Where(t => t.Mailbox != null && t.Mailbox.Brand == brand);
        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        var total = await query.CountAsync();

        var threads = await query
            .OrderByDescending(t => t.Priority)
            .ThenByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new
            {
                t.Id,
                t.Subject,
                t.FromEmail,
                t.FromName,
                classification = t.Classification.ToString(),
                status = t.Status.ToString(),
                priority = t.Priority.ToString(),
                t.ReceivedAt,
                slaBreached = t.SlaFirstResponseBreached || t.SlaResolutionBreached,
                mailbox = t.Mailbox != null ? t.Mailbox.EmailAddress : null
            })
            .ToListAsync();

        return Ok(new { total, page, pageSize, threads });
    }

    /// <summary>
    /// Seed initial mailboxes from discovered Microsoft 365 users
    /// </summary>
    [HttpPost("seed-mailboxes")]
    [AllowAnonymous] // Allow seeding without auth for initial setup
    public async Task<IActionResult> SeedMailboxes()
    {
        try
        {
            var existingCount = await _db.EmailMailboxes.CountAsync();
            if (existingCount > 0)
            {
                return Ok(new { message = "Mailboxes already seeded", count = existingCount });
            }

            // Get users from Graph
            var users = await _graphService.GetUsersAsync();

            var seededCount = 0;
            foreach (var user in users.Where(u => !string.IsNullOrEmpty(u.EmailAddress)))
            {
                var brand = DetermineBrand(user.EmailAddress);

                var mailbox = new EmailMailbox
                {
                    Id = Guid.NewGuid(),
                    EmailAddress = user.EmailAddress,
                    DisplayName = user.DisplayName ?? user.EmailAddress,
                    Brand = brand,
                    GraphUserId = user.Id,
                    IsActive = true,
                    AutoReplyEnabled = false,
                    DraftModeDefault = true,
                    DefaultSlaHours = 24,
                    SlaFirstResponseHours = 24,
                    SlaResolutionHours = 72,
                    CreatedAt = DateTime.UtcNow
                };

                _db.EmailMailboxes.Add(mailbox);
                seededCount++;
            }

            await _db.SaveChangesAsync();

            _logger.LogInformation("Seeded {Count} mailboxes from Microsoft 365", seededCount);
            return Ok(new { message = $"Seeded {seededCount} mailboxes", count = seededCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to seed mailboxes");
            return StatusCode(500, new { error = "An internal error occurred. Please try again later." });
        }
    }

    private static string DetermineBrand(string email)
    {
        var domain = email.Split('@').LastOrDefault()?.ToLower() ?? "";

        if (domain.Contains("shahin"))
            return EmailBrands.Shahin;
        if (domain.Contains("dogan"))
            return EmailBrands.DoganConsult;

        return EmailBrands.DoganConsult; // Default
    }
}
