using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrcMvc.Services.Interfaces;
using GrcMvc.Application.Permissions;
using GrcMvc.Authorization;
using GrcMvc.Models.Entities;
using GrcMvc.Models.Workflows;

namespace GrcMvc.Controllers;

/// <summary>
/// User Inbox Controller
/// Manages notifications, tasks, and user preferences
/// </summary>
[Authorize]
[RequireTenant]
[Route("[controller]")]
public class InboxController : Controller
{
    private readonly INotificationService _notificationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<InboxController> _logger;

    public InboxController(
        INotificationService notificationService,
        ICurrentUserService currentUserService,
        ILogger<InboxController> logger)
    {
        _notificationService = notificationService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Main inbox view with notifications list
    /// </summary>
    [HttpGet]
    [Authorize(GrcPermissions.Notifications.View)]
    public async Task<IActionResult> Index(bool unreadOnly = false, int page = 1)
    {
        try
        {
            var userId = _currentUserService.GetUserId().ToString();
            var tenantId = _currentUserService.GetTenantId();

            var notifications = await _notificationService.GetUserNotificationsAsync(
                userId, tenantId, unreadOnly, page, pageSize: 20);
            var unreadCount = await _notificationService.GetUnreadCountAsync(userId, tenantId);

            ViewBag.UnreadCount = unreadCount;
            ViewBag.CurrentPage = page;
            ViewBag.UnreadOnly = unreadOnly;
            ViewBag.HasMore = notifications.Count == 20;

            return View(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading inbox for user {UserId}", _currentUserService.GetUserId());
            TempData["Error"] = "Error loading inbox";
            return View(new List<WorkflowNotification>());
        }
    }

    /// <summary>
    /// View notification details
    /// </summary>
    [HttpGet("Details/{id}")]
    [Authorize(GrcPermissions.Notifications.View)]
    public async Task<IActionResult> Details(Guid id)
    {
        try
        {
            var userId = _currentUserService.GetUserId().ToString();
            var tenantId = _currentUserService.GetTenantId();

            // Get user notifications to find this one
            var notifications = await _notificationService.GetUserNotificationsAsync(
                userId, tenantId, unreadOnly: false, page: 1, pageSize: 1000);
            var notification = notifications.FirstOrDefault(n => n.Id == id);

            if (notification == null)
            {
                TempData["Error"] = "Notification not found";
                return RedirectToAction(nameof(Index));
            }

            // Mark as read when viewing
            await _notificationService.MarkAsReadAsync(id, userId);

            return View(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error viewing notification {NotificationId}", id);
            TempData["Error"] = "Error loading notification";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Mark a single notification as read
    /// </summary>
    [HttpPost("MarkAsRead/{id}")]
    [ValidateAntiForgeryToken]
    [Authorize(GrcPermissions.Notifications.View)]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        try
        {
            var userId = _currentUserService.GetUserId().ToString();
            await _notificationService.MarkAsReadAsync(id, userId);
            TempData["Success"] = "Notification marked as read";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification {NotificationId} as read", id);
            TempData["Error"] = "Error marking notification as read";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Mark all notifications as read
    /// </summary>
    [HttpPost("MarkAllAsRead")]
    [ValidateAntiForgeryToken]
    [Authorize(GrcPermissions.Notifications.View)]
    public async Task<IActionResult> MarkAllAsRead()
    {
        try
        {
            var userId = _currentUserService.GetUserId().ToString();
            var tenantId = _currentUserService.GetTenantId();
            await _notificationService.MarkAllAsReadAsync(userId, tenantId);
            TempData["Success"] = "All notifications marked as read";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read");
            TempData["Error"] = "Error marking notifications as read";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Notification preferences settings
    /// </summary>
    [HttpGet("Settings")]
    [Authorize(GrcPermissions.Notifications.Manage)]
    public async Task<IActionResult> Settings()
    {
        try
        {
            var userId = _currentUserService.GetUserId().ToString();
            var tenantId = _currentUserService.GetTenantId();
            var preferences = await _notificationService.GetUserPreferencesAsync(userId, tenantId);

            return View(preferences ?? new Models.Entities.UserNotificationPreference
            {
                UserId = userId,
                TenantId = tenantId,
                EmailEnabled = true,
                SmsEnabled = false,
                InAppEnabled = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading notification preferences");
            TempData["Error"] = "Error loading preferences";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Save notification preferences
    /// </summary>
    [HttpPost("Settings")]
    [ValidateAntiForgeryToken]
    [Authorize(GrcPermissions.Notifications.Manage)]
    public async Task<IActionResult> UpdateSettings(
        bool emailEnabled = true,
        bool smsEnabled = false,
        List<string>? enabledTypes = null)
    {
        try
        {
            var userId = _currentUserService.GetUserId().ToString();
            var tenantId = _currentUserService.GetTenantId();
            await _notificationService.UpdatePreferencesAsync(userId, tenantId, emailEnabled, smsEnabled, enabledTypes);
            TempData["Success"] = "Notification preferences updated";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating notification preferences");
            TempData["Error"] = "Error saving preferences";
        }

        return RedirectToAction(nameof(Settings));
    }

    /// <summary>
    /// API endpoint to get unread count (for navbar badge)
    /// </summary>
    [HttpGet("UnreadCount")]
    [Authorize(GrcPermissions.Notifications.View)]
    public async Task<IActionResult> GetUnreadCount()
    {
        try
        {
            var userId = _currentUserService.GetUserId().ToString();
            var tenantId = _currentUserService.GetTenantId();
            var count = await _notificationService.GetUnreadCountAsync(userId, tenantId);
            return Json(new { count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread count");
            return Json(new { count = 0 });
        }
    }
}
