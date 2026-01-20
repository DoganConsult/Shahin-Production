using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrcMvc.Services.Interfaces;
using GrcMvc.Data;
using Microsoft.AspNetCore.Identity;
using GrcMvc.Models.Entities;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace GrcMvc.Controllers;

/// <summary>
/// Public Support Controller - Users can submit tickets for issues, access problems, or errors
/// PUBLIC: Accessible without login (AllowAnonymous)
/// </summary>
[AllowAnonymous]
public class SupportController : Controller
{
    private readonly ISupportTicketService _ticketService;
    private readonly GrcDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<SupportController> _logger;
    private readonly ITenantContextService _tenantContext;

    public SupportController(
        ISupportTicketService ticketService,
        GrcDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<SupportController> logger,
        ITenantContextService tenantContext)
    {
        _ticketService = ticketService;
        _context = context;
        _userManager = userManager;
        _logger = logger;
        _tenantContext = tenantContext;
    }

    /// <summary>
    /// Support page - Form to submit a ticket
    /// GET /Support/Submit
    /// </summary>
    [HttpGet]
    [Route("/Support/Submit")]
    public IActionResult Submit()
    {
        // Pre-fill user info if authenticated
        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var tenantId = _tenantContext.GetCurrentTenantId();

            ViewBag.UserId = userId;
            ViewBag.UserEmail = userEmail;
            ViewBag.TenantId = tenantId;
        }

        return View();
    }

    /// <summary>
    /// Submit a support ticket (POST)
    /// POST /Support/Submit
    /// </summary>
    [HttpPost]
    [Route("/support")]
    [Route("/Support/Submit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitTicket([FromForm] SubmitTicketViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Submit", model);
        }

        try
        {
            // Get user info if authenticated
            string? userId = null;
            Guid? tenantId = null;

            if (User.Identity?.IsAuthenticated == true)
            {
                userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                tenantId = _tenantContext.GetCurrentTenantId();
            }

            // Create ticket DTO
            var createDto = new CreateTicketDto
            {
                TenantId = tenantId ?? model.TenantId,
                UserId = userId ?? model.UserId,
                UserEmail = model.UserEmail,
                Subject = model.Subject,
                Description = model.Description,
                Category = model.Category ?? "Technical",
                Priority = DeterminePriority(model.Priority, model.IsUrgent),
                Tags = model.Tags,
                RelatedEntityType = model.RelatedEntityType,
                RelatedEntityId = model.RelatedEntityId
            };

            var ticket = await _ticketService.CreateTicketAsync(createDto);

            _logger.LogInformation("Support ticket created: {TicketNumber} by {UserEmail}", 
                ticket.TicketNumber, model.UserEmail);

            // Redirect to confirmation page
            return RedirectToAction("Confirmation", new { ticketNumber = ticket.TicketNumber });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating support ticket");
            ModelState.AddModelError("", "حدث خطأ أثناء إرسال التذكرة. يرجى المحاولة مرة أخرى أو الاتصال بالدعم.");
            return View("Submit", model);
        }
    }

    /// <summary>
    /// Confirmation page after ticket submission
    /// GET /Support/Confirmation?ticketNumber=TKT-2026-000123
    /// </summary>
    [Route("/Support/Confirmation")]
    public async Task<IActionResult> Confirmation(string ticketNumber)
    {
        if (string.IsNullOrEmpty(ticketNumber))
        {
            return RedirectToAction("Submit");
        }

        var ticket = await _ticketService.GetTicketByNumberAsync(ticketNumber);
        if (ticket == null)
        {
            ViewBag.Error = "التذكرة غير موجودة";
            return View();
        }

        ViewBag.Ticket = ticket;
        return View();
    }

    /// <summary>
    /// Check ticket status (public)
    /// GET /Support/Status?ticketNumber=TKT-2026-000123
    /// </summary>
    [Route("/Support/Status")]
    public async Task<IActionResult> Status(string ticketNumber)
    {
        if (string.IsNullOrEmpty(ticketNumber))
        {
            return RedirectToAction("Submit");
        }

        var ticket = await _ticketService.GetTicketByNumberAsync(ticketNumber);
        if (ticket == null)
        {
            ViewBag.Error = "التذكرة غير موجودة";
            return View();
        }

        ViewBag.Ticket = ticket;
        return View();
    }

    /// <summary>
    /// API endpoint for ticket submission (for AJAX/SPA)
    /// POST /api/support/submit
    /// </summary>
    [HttpPost]
    [Route("/api/support/submit")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> SubmitTicketApi([FromBody] SubmitTicketApiRequest request)
    {
        if (string.IsNullOrEmpty(request.Subject) || string.IsNullOrEmpty(request.Description))
        {
            return BadRequest(new { success = false, error = "Subject and Description are required" });
        }

        try
        {
            string? userId = null;
            Guid? tenantId = null;

            if (User.Identity?.IsAuthenticated == true)
            {
                userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                tenantId = _tenantContext.GetCurrentTenantId();
            }

            var createDto = new CreateTicketDto
            {
                TenantId = tenantId ?? request.TenantId,
                UserId = userId ?? request.UserId,
                UserEmail = request.UserEmail,
                Subject = request.Subject,
                Description = request.Description,
                Category = request.Category ?? "Technical",
                Priority = DeterminePriority(request.Priority, request.IsUrgent),
                Tags = request.Tags,
                RelatedEntityType = request.RelatedEntityType,
                RelatedEntityId = request.RelatedEntityId
            };

            var ticket = await _ticketService.CreateTicketAsync(createDto);

            return Ok(new
            {
                success = true,
                ticketNumber = ticket.TicketNumber,
                ticketId = ticket.Id,
                message = "تم إرسال التذكرة بنجاح"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating support ticket via API");
            return StatusCode(500, new { success = false, error = "Failed to create ticket" });
        }
    }

    /// <summary>
    /// Determine priority based on user selection and urgency flag
    /// </summary>
    private string DeterminePriority(string? priority, bool isUrgent)
    {
        if (isUrgent)
            return "Urgent";

        return priority switch
        {
            "Low" => "Low",
            "Medium" => "Medium",
            "High" => "High",
            "Urgent" => "Urgent",
            "Critical" => "Critical",
            _ => "Medium"
        };
    }
}

/// <summary>
/// ViewModel for ticket submission form
/// </summary>
public class SubmitTicketViewModel
{
    [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
    [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
    [Display(Name = "البريد الإلكتروني")]
    public string UserEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "الموضوع مطلوب")]
    [StringLength(500, ErrorMessage = "الموضوع يجب أن يكون أقل من 500 حرف")]
    [Display(Name = "الموضوع")]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "الوصف مطلوب")]
    [StringLength(2000, ErrorMessage = "الوصف يجب أن يكون أقل من 2000 حرف")]
    [Display(Name = "وصف المشكلة")]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "الفئة")]
    public string? Category { get; set; } = "Technical";

    [Display(Name = "الأولوية")]
    public string? Priority { get; set; } = "Medium";

    [Display(Name = "عاجل")]
    public bool IsUrgent { get; set; }

    [Display(Name = "العلامات")]
    public string? Tags { get; set; }

    [Display(Name = "نوع الكيان المرتبط")]
    public string? RelatedEntityType { get; set; }

    [Display(Name = "معرف الكيان المرتبط")]
    public Guid? RelatedEntityId { get; set; }

    // Hidden fields (pre-filled if authenticated)
    public string? UserId { get; set; }
    public Guid? TenantId { get; set; }
}

/// <summary>
/// API request model for ticket submission
/// </summary>
public class SubmitTicketApiRequest
{
    public string? UserEmail { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Priority { get; set; }
    public bool IsUrgent { get; set; }
    public string? Tags { get; set; }
    public string? RelatedEntityType { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public string? UserId { get; set; }
    public Guid? TenantId { get; set; }
}
