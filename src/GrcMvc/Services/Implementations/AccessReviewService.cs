using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GrcMvc.Configuration;
using GrcMvc.Constants;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// AM-11: Periodic Access Reviews Service Implementation
/// Handles creation, execution, and reporting of access reviews.
/// </summary>
public class AccessReviewService : IAccessReviewService
{
    private readonly GrcDbContext _context;
    private readonly GrcAuthDbContext _authContext;
    private readonly IAuditEventService _auditEventService;
    private readonly ILogger<AccessReviewService> _logger;

    public AccessReviewService(
        GrcDbContext context,
        GrcAuthDbContext authContext,
        IAuditEventService auditEventService,
        ILogger<AccessReviewService> logger)
    {
        _context = context;
        _authContext = authContext;
        _auditEventService = auditEventService;
        _logger = logger;
    }

    #region Review Campaign Management

    public async Task<AccessReview> CreateReviewAsync(CreateAccessReviewDto dto, string createdBy)
    {
        var reviewCode = await GenerateReviewCodeAsync(dto.TenantId);

        var review = new AccessReview
        {
            Id = Guid.NewGuid(),
            TenantId = dto.TenantId,
            ReviewCode = reviewCode,
            Name = dto.Name,
            Description = dto.Description,
            ReviewType = dto.ReviewType,
            Scope = dto.Scope,
            ScopeFilterJson = dto.ScopeFilterJson,
            Status = AccessReviewStatus.Draft,
            ScheduledStartDate = dto.ScheduledStartDate,
            DueDate = dto.DueDate,
            ReviewerId = dto.ReviewerId,
            ComplianceFramework = dto.ComplianceFramework,
            RecurrencePattern = dto.RecurrencePattern,
            ReminderDays = dto.ReminderDays,
            ControlReference = "AM-11",
            CreatedDate = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        _context.Set<AccessReview>().Add(review);
        await _context.SaveChangesAsync();

        // Log audit event
        await _auditEventService.LogEventAsync(
            dto.TenantId,
            AccessManagementControls.AM11_AccessReviews.Events.ReviewScheduled,
            "AccessReview",
            review.Id.ToString(),
            "Create",
            createdBy,
            JsonSerializer.Serialize(new { review.ReviewCode, review.Name, review.Scope, review.ScheduledStartDate }));

        _logger.LogInformation("Access review {ReviewCode} created for tenant {TenantId}", reviewCode, dto.TenantId);

        return review;
    }

    public async Task<AccessReview?> GetReviewByIdAsync(Guid reviewId)
    {
        return await _context.Set<AccessReview>()
            .FirstOrDefaultAsync(r => r.Id == reviewId && !r.IsDeleted);
    }

    public async Task<AccessReview?> GetReviewWithItemsAsync(Guid reviewId)
    {
        return await _context.Set<AccessReview>()
            .Include(r => r.Items)
            .FirstOrDefaultAsync(r => r.Id == reviewId && !r.IsDeleted);
    }

    public async Task<List<AccessReview>> GetReviewsByTenantAsync(Guid tenantId, AccessReviewFilterDto? filter = null)
    {
        var query = _context.Set<AccessReview>()
            .Where(r => r.TenantId == tenantId && !r.IsDeleted);

        if (filter != null)
        {
            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(r => r.Status == filter.Status);

            if (!string.IsNullOrEmpty(filter.ReviewType))
                query = query.Where(r => r.ReviewType == filter.ReviewType);

            if (filter.From.HasValue)
                query = query.Where(r => r.ScheduledStartDate >= filter.From.Value);

            if (filter.To.HasValue)
                query = query.Where(r => r.ScheduledStartDate <= filter.To.Value);

            if (filter.IncludeCompleted == false)
                query = query.Where(r => r.Status != AccessReviewStatus.Completed);
        }

        return await query
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync();
    }

    public async Task<AccessReview?> UpdateReviewAsync(Guid reviewId, UpdateAccessReviewDto dto, string updatedBy)
    {
        var review = await GetReviewByIdAsync(reviewId);
        if (review == null) return null;

        if (review.Status != AccessReviewStatus.Draft && review.Status != AccessReviewStatus.Scheduled)
        {
            _logger.LogWarning("Cannot update review {ReviewId} in status {Status}", reviewId, review.Status);
            return null;
        }

        if (dto.Name != null) review.Name = dto.Name;
        if (dto.Description != null) review.Description = dto.Description;
        if (dto.ScheduledStartDate.HasValue) review.ScheduledStartDate = dto.ScheduledStartDate.Value;
        if (dto.DueDate.HasValue) review.DueDate = dto.DueDate.Value;
        if (dto.ReviewerId != null) review.ReviewerId = dto.ReviewerId;
        if (dto.ReviewerNotes != null) review.ReviewerNotes = dto.ReviewerNotes;
        if (dto.ReminderDays.HasValue) review.ReminderDays = dto.ReminderDays.Value;

        review.ModifiedDate = DateTime.UtcNow;
        review.ModifiedBy = updatedBy;

        await _context.SaveChangesAsync();
        return review;
    }

    public async Task<bool> CancelReviewAsync(Guid reviewId, string cancelledBy, string reason)
    {
        var review = await GetReviewByIdAsync(reviewId);
        if (review == null) return false;

        if (review.Status == AccessReviewStatus.Completed)
        {
            _logger.LogWarning("Cannot cancel completed review {ReviewId}", reviewId);
            return false;
        }

        review.Status = AccessReviewStatus.Cancelled;
        review.ReviewerNotes = $"Cancelled: {reason}";
        review.ModifiedDate = DateTime.UtcNow;
        review.ModifiedBy = cancelledBy;

        await _context.SaveChangesAsync();

        await _auditEventService.LogEventAsync(
            review.TenantId,
            AccessManagementControls.AM11_AccessReviews.Events.ReviewCompleted,
            "AccessReview",
            review.Id.ToString(),
            "Cancel",
            cancelledBy,
            JsonSerializer.Serialize(new { reason }));

        return true;
    }

    public async Task<bool> DeleteReviewAsync(Guid reviewId, string deletedBy)
    {
        var review = await GetReviewByIdAsync(reviewId);
        if (review == null) return false;

        if (review.Status != AccessReviewStatus.Draft)
        {
            _logger.LogWarning("Cannot delete non-draft review {ReviewId}", reviewId);
            return false;
        }

        review.IsDeleted = true;
        review.ModifiedDate = DateTime.UtcNow;
        review.ModifiedBy = deletedBy;

        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region Review Execution

    public async Task<AccessReview?> StartReviewAsync(Guid reviewId, string startedBy)
    {
        var review = await GetReviewByIdAsync(reviewId);
        if (review == null) return null;

        if (review.Status != AccessReviewStatus.Draft && review.Status != AccessReviewStatus.Scheduled)
        {
            _logger.LogWarning("Cannot start review {ReviewId} in status {Status}", reviewId, review.Status);
            return null;
        }

        // Generate review items
        var itemCount = await GenerateReviewItemsAsync(reviewId);

        review.Status = AccessReviewStatus.InProgress;
        review.ActualStartDate = DateTime.UtcNow;
        review.TotalItems = itemCount;
        review.ModifiedDate = DateTime.UtcNow;
        review.ModifiedBy = startedBy;

        await _context.SaveChangesAsync();

        await _auditEventService.LogEventAsync(
            review.TenantId,
            AccessManagementControls.AM11_AccessReviews.Events.ReviewStarted,
            "AccessReview",
            review.Id.ToString(),
            "Start",
            startedBy,
            JsonSerializer.Serialize(new { review.ReviewCode, itemCount }));

        _logger.LogInformation("Access review {ReviewCode} started with {ItemCount} items", review.ReviewCode, itemCount);

        return review;
    }

    public async Task<int> GenerateReviewItemsAsync(Guid reviewId)
    {
        var review = await GetReviewByIdAsync(reviewId);
        if (review == null) return 0;

        var items = new List<AccessReviewItem>();

        // Get tenant users with their roles
        var tenantUsers = await _context.TenantUsers
            .Where(tu => tu.TenantId == review.TenantId && !tu.IsDeleted && tu.Status == "Active")
            .ToListAsync();

        // Get user details from auth context
        var userIds = tenantUsers.Select(tu => tu.UserId).ToList();
        var users = await _authContext.Set<ApplicationUser>()
            .Where(u => userIds.Contains(u.Id.ToString()))
            .ToDictionaryAsync(u => u.Id.ToString(), u => u);

        foreach (var tenantUser in tenantUsers)
        {
            users.TryGetValue(tenantUser.UserId, out var user);

            // Apply scope filter
            var shouldInclude = review.Scope switch
            {
                AccessReviewScope.AllUsers => true,
                AccessReviewScope.PrivilegedOnly => IsPrivilegedRole(tenantUser.RoleCode),
                AccessReviewScope.InactiveOnly => user?.LastLoginDate == null ||
                    user.LastLoginDate < DateTime.UtcNow.AddDays(-AccessManagementControls.AM11_AccessReviews.InactivityThresholdDays),
                AccessReviewScope.ByRole => MatchesScopeFilter(tenantUser.RoleCode, review.ScopeFilterJson),
                _ => true
            };

            if (!shouldInclude) continue;

            var isPrivileged = IsPrivilegedRole(tenantUser.RoleCode);

            var item = new AccessReviewItem
            {
                Id = Guid.NewGuid(),
                AccessReviewId = reviewId,
                TenantId = review.TenantId,
                UserId = tenantUser.UserId,
                UserEmail = user?.Email,
                UserFullName = user?.FullName,
                ItemType = "Role",
                AccessItem = tenantUser.RoleCode ?? "Employee",
                AccessItemDescription = $"Role: {tenantUser.RoleCode}",
                AccessGrantedDate = tenantUser.CreatedDate,
                UserLastLoginDate = user?.LastLoginDate,
                Status = AccessReviewItemStatus.Pending,
                RiskLevel = isPrivileged ? "High" : "Low",
                IsPrivileged = isPrivileged,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "SYSTEM"
            };

            // Flag items needing attention
            if (isPrivileged)
            {
                item.IsFlagged = true;
                item.FlagReason = "Privileged access requires explicit review";
            }
            else if (user?.LastLoginDate == null || user.LastLoginDate < DateTime.UtcNow.AddDays(-90))
            {
                item.IsFlagged = true;
                item.FlagReason = "User inactive for 90+ days";
            }

            items.Add(item);
        }

        _context.Set<AccessReviewItem>().AddRange(items);
        await _context.SaveChangesAsync();

        return items.Count;
    }

    public async Task<PagedResult<AccessReviewItem>> GetReviewItemsAsync(
        Guid reviewId,
        int page = 1,
        int pageSize = 50,
        string? statusFilter = null)
    {
        var query = _context.Set<AccessReviewItem>()
            .Where(i => i.AccessReviewId == reviewId && !i.IsDeleted);

        if (!string.IsNullOrEmpty(statusFilter))
            query = query.Where(i => i.Status == statusFilter);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(i => i.IsPrivileged)
            .ThenByDescending(i => i.IsFlagged)
            .ThenBy(i => i.UserEmail)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<AccessReviewItem>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<AccessReviewItem?> MakeDecisionAsync(
        Guid itemId,
        string decision,
        string decidedBy,
        string? justification = null)
    {
        var item = await _context.Set<AccessReviewItem>()
            .Include(i => i.AccessReview)
            .FirstOrDefaultAsync(i => i.Id == itemId && !i.IsDeleted);

        if (item == null) return null;

        item.Decision = decision;
        item.DecisionById = decidedBy;
        item.DecisionDate = DateTime.UtcNow;
        item.Justification = justification;
        item.Status = decision == AccessReviewDecision.Revoke
            ? AccessReviewItemStatus.Revoked
            : AccessReviewItemStatus.Approved;

        if (decision == AccessReviewDecision.Revoke)
        {
            item.RemediationStatus = "Required";
        }

        item.ModifiedDate = DateTime.UtcNow;
        item.ModifiedBy = decidedBy;

        // Update review progress
        await UpdateReviewProgressAsync(item.AccessReviewId);

        await _context.SaveChangesAsync();

        // Log audit event
        var eventType = decision == AccessReviewDecision.Revoke
            ? AccessManagementControls.AM11_AccessReviews.Events.ReviewItemRevoked
            : AccessManagementControls.AM11_AccessReviews.Events.ReviewItemApproved;

        await _auditEventService.LogEventAsync(
            item.TenantId,
            eventType,
            "AccessReviewItem",
            item.Id.ToString(),
            decision,
            decidedBy,
            JsonSerializer.Serialize(new { item.UserEmail, item.AccessItem, decision, justification }));

        return item;
    }

    public async Task<int> BulkDecisionAsync(
        IEnumerable<Guid> itemIds,
        string decision,
        string decidedBy,
        string? justification = null)
    {
        var count = 0;
        foreach (var itemId in itemIds)
        {
            var result = await MakeDecisionAsync(itemId, decision, decidedBy, justification);
            if (result != null) count++;
        }
        return count;
    }

    public async Task<AccessReviewItem?> FlagItemAsync(Guid itemId, string flaggedBy, string reason)
    {
        var item = await _context.Set<AccessReviewItem>()
            .FirstOrDefaultAsync(i => i.Id == itemId && !i.IsDeleted);

        if (item == null) return null;

        item.IsFlagged = true;
        item.FlagReason = reason;
        item.ModifiedDate = DateTime.UtcNow;
        item.ModifiedBy = flaggedBy;

        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<AccessReview?> CompleteReviewAsync(Guid reviewId, string completedBy, string? notes = null)
    {
        var review = await GetReviewWithItemsAsync(reviewId);
        if (review == null) return null;

        // Check all items are decided
        var pendingItems = review.Items.Count(i => i.Status == AccessReviewItemStatus.Pending);
        if (pendingItems > 0)
        {
            _logger.LogWarning("Cannot complete review {ReviewId}, {Count} items pending", reviewId, pendingItems);
            return null;
        }

        review.Status = AccessReviewStatus.PendingApproval;
        review.ReviewerNotes = notes;
        review.ReviewedItems = review.Items.Count;
        review.ApprovedItems = review.Items.Count(i => i.Status == AccessReviewItemStatus.Approved);
        review.RevokedItems = review.Items.Count(i => i.Status == AccessReviewItemStatus.Revoked);
        review.PendingRemediationItems = review.Items.Count(i => i.RemediationStatus == "Required");
        review.ProgressPercent = 100;
        review.ModifiedDate = DateTime.UtcNow;
        review.ModifiedBy = completedBy;

        await _context.SaveChangesAsync();

        await _auditEventService.LogEventAsync(
            review.TenantId,
            AccessManagementControls.AM11_AccessReviews.Events.ReviewCompleted,
            "AccessReview",
            review.Id.ToString(),
            "Complete",
            completedBy,
            JsonSerializer.Serialize(new { review.ReviewCode, review.ApprovedItems, review.RevokedItems }));

        return review;
    }

    public async Task<AccessReview?> ApproveReviewAsync(Guid reviewId, string approvedBy)
    {
        var review = await GetReviewByIdAsync(reviewId);
        if (review == null || review.Status != AccessReviewStatus.PendingApproval)
            return null;

        review.Status = AccessReviewStatus.Completed;
        review.CompletedDate = DateTime.UtcNow;
        review.ApprovedById = approvedBy;
        review.ModifiedDate = DateTime.UtcNow;
        review.ModifiedBy = approvedBy;

        // Schedule next recurring review if applicable
        if (review.RecurrencePattern != "None")
        {
            review.NextReviewDate = CalculateNextReviewDate(review.RecurrencePattern, DateTime.UtcNow);
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Access review {ReviewCode} approved by {ApprovedBy}", review.ReviewCode, approvedBy);

        return review;
    }

    #endregion

    #region Remediation

    public async Task<int> ExecuteRemediationAsync(Guid reviewId, string executedBy)
    {
        var review = await GetReviewWithItemsAsync(reviewId);
        if (review == null) return 0;

        var itemsToRemediate = review.Items
            .Where(i => i.Status == AccessReviewItemStatus.Revoked && i.RemediationStatus == "Required")
            .ToList();

        var count = 0;
        foreach (var item in itemsToRemediate)
        {
            try
            {
                // Deactivate the tenant user
                var tenantUser = await _context.TenantUsers
                    .FirstOrDefaultAsync(tu => tu.TenantId == item.TenantId &&
                                               tu.UserId == item.UserId &&
                                               !tu.IsDeleted);

                if (tenantUser != null)
                {
                    tenantUser.Status = "Suspended";
                    tenantUser.ModifiedDate = DateTime.UtcNow;
                    tenantUser.ModifiedBy = executedBy;

                    item.RemediationStatus = "Completed";
                    item.RemediationCompletedDate = DateTime.UtcNow;
                    item.RemediationNotes = "Access revoked via access review remediation";

                    // Log audit event
                    await _auditEventService.LogEventAsync(
                        item.TenantId,
                        AccessManagementControls.AM05_JMLLifecycle.Events.UserSuspended,
                        "TenantUser",
                        tenantUser.Id.ToString(),
                        "Remediate",
                        executedBy,
                        JsonSerializer.Serialize(new { item.UserEmail, item.AccessItem, reviewId }));

                    count++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remediate access for user {UserId}", item.UserId);
                item.RemediationStatus = "Failed";
                item.RemediationNotes = $"Error: {ex.Message}";
            }
        }

        await _context.SaveChangesAsync();
        return count;
    }

    public async Task<AccessReviewItem?> CompleteRemediationAsync(Guid itemId, string completedBy, string notes)
    {
        var item = await _context.Set<AccessReviewItem>()
            .FirstOrDefaultAsync(i => i.Id == itemId && !i.IsDeleted);

        if (item == null) return null;

        item.RemediationStatus = "Completed";
        item.RemediationCompletedDate = DateTime.UtcNow;
        item.RemediationNotes = notes;
        item.ModifiedDate = DateTime.UtcNow;
        item.ModifiedBy = completedBy;

        // Update review pending count
        var review = await GetReviewByIdAsync(item.AccessReviewId);
        if (review != null)
        {
            review.PendingRemediationItems = await _context.Set<AccessReviewItem>()
                .CountAsync(i => i.AccessReviewId == review.Id && i.RemediationStatus == "Required");
            await _context.SaveChangesAsync();
        }

        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<List<AccessReviewItem>> GetPendingRemediationItemsAsync(Guid tenantId)
    {
        return await _context.Set<AccessReviewItem>()
            .Include(i => i.AccessReview)
            .Where(i => i.TenantId == tenantId &&
                       i.RemediationStatus == "Required" &&
                       !i.IsDeleted)
            .ToListAsync();
    }

    #endregion

    #region Scheduling and Automation

    public async Task<AccessReview?> ScheduleReviewAsync(Guid reviewId, DateTime startDate, string scheduledBy)
    {
        var review = await GetReviewByIdAsync(reviewId);
        if (review == null || review.Status != AccessReviewStatus.Draft)
            return null;

        review.Status = AccessReviewStatus.Scheduled;
        review.ScheduledStartDate = startDate;
        review.ModifiedDate = DateTime.UtcNow;
        review.ModifiedBy = scheduledBy;

        await _context.SaveChangesAsync();

        await _auditEventService.LogEventAsync(
            review.TenantId,
            AccessManagementControls.AM11_AccessReviews.Events.ReviewScheduled,
            "AccessReview",
            review.Id.ToString(),
            "Schedule",
            scheduledBy,
            JsonSerializer.Serialize(new { review.ReviewCode, startDate }));

        return review;
    }

    public async Task<AccessReview?> CreateNextRecurringReviewAsync(Guid completedReviewId)
    {
        var completedReview = await GetReviewByIdAsync(completedReviewId);
        if (completedReview == null ||
            completedReview.Status != AccessReviewStatus.Completed ||
            completedReview.RecurrencePattern == "None")
            return null;

        var nextStartDate = completedReview.NextReviewDate ?? CalculateNextReviewDate(
            completedReview.RecurrencePattern,
            completedReview.CompletedDate ?? DateTime.UtcNow);

        var nextDueDate = nextStartDate.AddDays(14); // 2 weeks to complete

        var dto = new CreateAccessReviewDto
        {
            TenantId = completedReview.TenantId,
            Name = $"{completedReview.Name} (Recurring)",
            Description = completedReview.Description,
            ReviewType = completedReview.ReviewType,
            Scope = completedReview.Scope,
            ScopeFilterJson = completedReview.ScopeFilterJson,
            ScheduledStartDate = nextStartDate,
            DueDate = nextDueDate,
            ReviewerId = completedReview.ReviewerId,
            ComplianceFramework = completedReview.ComplianceFramework,
            RecurrencePattern = completedReview.RecurrencePattern,
            ReminderDays = completedReview.ReminderDays
        };

        return await CreateReviewAsync(dto, "SYSTEM");
    }

    public async Task<List<AccessReview>> GetOverdueReviewsAsync(Guid? tenantId = null)
    {
        var query = _context.Set<AccessReview>()
            .Where(r => !r.IsDeleted &&
                       (r.Status == AccessReviewStatus.InProgress || r.Status == AccessReviewStatus.Scheduled) &&
                       r.DueDate < DateTime.UtcNow);

        if (tenantId.HasValue)
            query = query.Where(r => r.TenantId == tenantId.Value);

        return await query.ToListAsync();
    }

    public async Task<int> SendReviewRemindersAsync()
    {
        var upcomingReviews = await _context.Set<AccessReview>()
            .Where(r => !r.IsDeleted &&
                       !r.ReminderSent &&
                       (r.Status == AccessReviewStatus.InProgress || r.Status == AccessReviewStatus.Scheduled) &&
                       r.DueDate <= DateTime.UtcNow.AddDays(7))
            .ToListAsync();

        var count = 0;
        foreach (var review in upcomingReviews)
        {
            // Mark reminder sent (actual email sending would be in a separate service)
            review.ReminderSent = true;
            review.ModifiedDate = DateTime.UtcNow;
            review.ModifiedBy = "SYSTEM";
            count++;

            _logger.LogInformation("Reminder sent for access review {ReviewCode}", review.ReviewCode);
        }

        await _context.SaveChangesAsync();
        return count;
    }

    public async Task<int> ProcessOverdueReviewsAsync()
    {
        var overdueReviews = await GetOverdueReviewsAsync();
        var count = 0;

        foreach (var review in overdueReviews)
        {
            review.Status = AccessReviewStatus.Overdue;
            review.ModifiedDate = DateTime.UtcNow;
            review.ModifiedBy = "SYSTEM";

            await _auditEventService.LogEventAsync(
                review.TenantId,
                AccessManagementControls.AM11_AccessReviews.Events.ReviewOverdue,
                "AccessReview",
                review.Id.ToString(),
                "MarkOverdue",
                "SYSTEM",
                JsonSerializer.Serialize(new { review.ReviewCode, review.DueDate }));

            count++;
        }

        await _context.SaveChangesAsync();
        return count;
    }

    #endregion

    #region Reporting and Analytics

    public async Task<AccessReviewStatistics> GetReviewStatisticsAsync(Guid tenantId, DateTime? from = null, DateTime? to = null)
    {
        var query = _context.Set<AccessReview>()
            .Where(r => r.TenantId == tenantId && !r.IsDeleted);

        if (from.HasValue)
            query = query.Where(r => r.CreatedDate >= from.Value);
        if (to.HasValue)
            query = query.Where(r => r.CreatedDate <= to.Value);

        var reviews = await query.ToListAsync();

        var completedReviews = reviews.Where(r => r.Status == AccessReviewStatus.Completed).ToList();

        return new AccessReviewStatistics
        {
            TotalReviews = reviews.Count,
            CompletedReviews = completedReviews.Count,
            OverdueReviews = reviews.Count(r => r.Status == AccessReviewStatus.Overdue),
            InProgressReviews = reviews.Count(r => r.Status == AccessReviewStatus.InProgress),
            TotalItemsReviewed = reviews.Sum(r => r.ReviewedItems),
            TotalAccessRevoked = reviews.Sum(r => r.RevokedItems),
            AverageCompletionDays = completedReviews.Any()
                ? completedReviews.Average(r => (r.CompletedDate ?? r.DueDate).Subtract(r.ActualStartDate ?? r.ScheduledStartDate).TotalDays)
                : 0,
            ComplianceRate = reviews.Count > 0
                ? (double)completedReviews.Count / reviews.Count * 100
                : 0,
            ReviewsByType = reviews.GroupBy(r => r.ReviewType).ToDictionary(g => g.Key, g => g.Count()),
            DecisionsByType = new Dictionary<string, int>
            {
                { "Approved", reviews.Sum(r => r.ApprovedItems) },
                { "Revoked", reviews.Sum(r => r.RevokedItems) }
            }
        };
    }

    public async Task<AccessReviewComplianceSummary> GetComplianceSummaryAsync(Guid tenantId)
    {
        var lastCompletedReview = await _context.Set<AccessReview>()
            .Where(r => r.TenantId == tenantId &&
                       r.Status == AccessReviewStatus.Completed &&
                       !r.IsDeleted)
            .OrderByDescending(r => r.CompletedDate)
            .FirstOrDefaultAsync();

        var nextScheduledReview = await _context.Set<AccessReview>()
            .Where(r => r.TenantId == tenantId &&
                       (r.Status == AccessReviewStatus.Scheduled || r.Status == AccessReviewStatus.Draft) &&
                       !r.IsDeleted)
            .OrderBy(r => r.ScheduledStartDate)
            .FirstOrDefaultAsync();

        var privilegedUsers = await _context.TenantUsers
            .CountAsync(tu => tu.TenantId == tenantId &&
                             !tu.IsDeleted &&
                             tu.Status == "Active" &&
                             AccessManagementControls.AM04_PrivilegedAccess.PrivilegedRoles.Contains(tu.RoleCode ?? ""));

        var pendingRemediations = await _context.Set<AccessReviewItem>()
            .CountAsync(i => i.TenantId == tenantId && i.RemediationStatus == "Required");

        var gaps = new List<string>();
        var nextDue = lastCompletedReview?.CompletedDate?.AddDays(AccessManagementControls.AM11_AccessReviews.ReviewFrequencyDays)
                     ?? DateTime.UtcNow;

        if (lastCompletedReview == null)
            gaps.Add("No access reviews have been completed");
        else if (DateTime.UtcNow > nextDue)
            gaps.Add($"Access review overdue since {nextDue:yyyy-MM-dd}");

        if (pendingRemediations > 0)
            gaps.Add($"{pendingRemediations} remediation actions pending");

        return new AccessReviewComplianceSummary
        {
            IsCompliant = gaps.Count == 0,
            LastReviewDate = lastCompletedReview?.CompletedDate,
            NextReviewDue = nextScheduledReview?.ScheduledStartDate ?? nextDue,
            DaysUntilNextReview = (int)(nextDue - DateTime.UtcNow).TotalDays,
            TotalPrivilegedUsers = privilegedUsers,
            PrivilegedUsersReviewed = lastCompletedReview?.Items.Count(i => i.IsPrivileged) ?? 0,
            PendingRemediations = pendingRemediations,
            ComplianceGaps = gaps,
            ControlReference = "AM-11"
        };
    }

    public async Task<byte[]> ExportReviewResultsAsync(Guid reviewId, string format = "csv")
    {
        var review = await GetReviewWithItemsAsync(reviewId);
        if (review == null) return Array.Empty<byte>();

        var sb = new StringBuilder();

        if (format.ToLower() == "csv")
        {
            sb.AppendLine("UserEmail,UserFullName,AccessItem,Decision,Justification,DecisionDate,RiskLevel,IsPrivileged");

            foreach (var item in review.Items)
            {
                sb.AppendLine($"\"{item.UserEmail}\",\"{item.UserFullName}\",\"{item.AccessItem}\"," +
                             $"\"{item.Decision}\",\"{item.Justification?.Replace("\"", "\"\"")}\",\"{item.DecisionDate:yyyy-MM-dd}\"," +
                             $"\"{item.RiskLevel}\",\"{item.IsPrivileged}\"");
            }
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public async Task<AccessReviewReport> GenerateReportAsync(Guid reviewId)
    {
        var review = await GetReviewWithItemsAsync(reviewId);
        if (review == null) return new AccessReviewReport();

        return new AccessReviewReport
        {
            ReviewId = review.Id,
            ReviewCode = review.ReviewCode,
            ReviewName = review.Name,
            Status = review.Status,
            GeneratedAt = DateTime.UtcNow,
            TotalItems = review.TotalItems,
            ApprovedItems = review.ApprovedItems,
            RevokedItems = review.RevokedItems,
            FlaggedItemsCount = review.Items.Count(i => i.IsFlagged),
            SkippedItems = review.Items.Count(i => i.Status == AccessReviewItemStatus.Skipped),
            CompletionRate = review.TotalItems > 0 ? (double)review.ReviewedItems / review.TotalItems * 100 : 0,
            HighRiskItems = review.Items.Where(i => i.RiskLevel == "High" || i.RiskLevel == "Critical")
                .Select(i => new AccessReviewReportItem
                {
                    UserEmail = i.UserEmail ?? "",
                    UserFullName = i.UserFullName ?? "",
                    AccessItem = i.AccessItem,
                    Decision = i.Decision ?? "",
                    RiskLevel = i.RiskLevel,
                    Justification = i.Justification,
                    DecisionDate = i.DecisionDate
                }).ToList(),
            RevokedAccessItems = review.Items.Where(i => i.Status == AccessReviewItemStatus.Revoked)
                .Select(i => new AccessReviewReportItem
                {
                    UserEmail = i.UserEmail ?? "",
                    UserFullName = i.UserFullName ?? "",
                    AccessItem = i.AccessItem,
                    Decision = "Revoked",
                    RiskLevel = i.RiskLevel,
                    Justification = i.Justification,
                    DecisionDate = i.DecisionDate
                }).ToList(),
            FlaggedItems = review.Items.Where(i => i.IsFlagged)
                .Select(i => new AccessReviewReportItem
                {
                    UserEmail = i.UserEmail ?? "",
                    UserFullName = i.UserFullName ?? "",
                    AccessItem = i.AccessItem,
                    Decision = i.Decision ?? "Pending",
                    RiskLevel = i.RiskLevel,
                    Justification = i.FlagReason,
                    DecisionDate = i.DecisionDate
                }).ToList(),
            StartedAt = review.ActualStartDate,
            CompletedAt = review.CompletedDate
        };
    }

    #endregion

    #region Private Helpers

    private async Task<string> GenerateReviewCodeAsync(Guid tenantId)
    {
        var year = DateTime.UtcNow.Year;
        var quarter = (DateTime.UtcNow.Month - 1) / 3 + 1;

        var count = await _context.Set<AccessReview>()
            .CountAsync(r => r.TenantId == tenantId && r.CreatedDate.Year == year);

        return $"AR-{year}-Q{quarter}-{(count + 1):D3}";
    }

    private async Task UpdateReviewProgressAsync(Guid reviewId)
    {
        var review = await GetReviewByIdAsync(reviewId);
        if (review == null) return;

        var items = await _context.Set<AccessReviewItem>()
            .Where(i => i.AccessReviewId == reviewId && !i.IsDeleted)
            .ToListAsync();

        var decidedCount = items.Count(i => i.Status != AccessReviewItemStatus.Pending);
        review.ReviewedItems = decidedCount;
        review.ApprovedItems = items.Count(i => i.Status == AccessReviewItemStatus.Approved);
        review.RevokedItems = items.Count(i => i.Status == AccessReviewItemStatus.Revoked);
        review.ProgressPercent = items.Count > 0 ? (int)((double)decidedCount / items.Count * 100) : 0;
    }

    private bool IsPrivilegedRole(string? roleCode)
    {
        if (string.IsNullOrEmpty(roleCode)) return false;
        return AccessManagementControls.AM04_PrivilegedAccess.PrivilegedRoles
            .Any(r => r.Equals(roleCode, StringComparison.OrdinalIgnoreCase));
    }

    private bool MatchesScopeFilter(string? roleCode, string? scopeFilterJson)
    {
        if (string.IsNullOrEmpty(scopeFilterJson)) return true;

        try
        {
            var filter = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(scopeFilterJson);
            if (filter?.TryGetValue("roles", out var roles) == true)
            {
                return roles.Contains(roleCode ?? "", StringComparer.OrdinalIgnoreCase);
            }
        }
        catch { }

        return true;
    }

    private DateTime CalculateNextReviewDate(string pattern, DateTime fromDate)
    {
        return pattern switch
        {
            "Monthly" => fromDate.AddMonths(1),
            "Quarterly" => fromDate.AddMonths(3),
            "SemiAnnual" => fromDate.AddMonths(6),
            "Annual" => fromDate.AddYears(1),
            _ => fromDate.AddMonths(3) // Default to quarterly
        };
    }

    #endregion
}
