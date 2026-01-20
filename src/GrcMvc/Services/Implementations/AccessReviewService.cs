using GrcMvc.Configuration;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Models.Enums;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// AM-04: Access Review Service for periodic access certification.
    /// Implements controls for review lifecycle, decision governance, and execution.
    /// </summary>
    public class AccessReviewService : IAccessReviewService
    {
        // Status constants for consistency
        private const string StatusDraft = "Draft";
        private const string StatusInProgress = "InProgress";
        private const string StatusCompleted = "Completed";
        private const string StatusCancelled = "Cancelled";

        // Decision constants
        private const string DecisionPending = "Pending";
        private const string DecisionCertified = "Certified";
        private const string DecisionRevoked = "Revoked";
        private const string DecisionModified = "Modified";

        private readonly GrcDbContext _dbContext;
        private readonly IAccessManagementAuditService _auditService;
        private readonly RoleAssignmentService _roleService;
        private readonly AccessReviewOptions _options;
        private readonly ILogger<AccessReviewService> _logger;

        public AccessReviewService(
            GrcDbContext dbContext,
            IAccessManagementAuditService auditService,
            RoleAssignmentService roleService,
            IOptions<AccessManagementOptions> options,
            ILogger<AccessReviewService> logger)
        {
            _dbContext = dbContext;
            _auditService = auditService;
            _roleService = roleService;
            _options = options.Value.AccessReview;
            _logger = logger;
        }

        /// <summary>
        /// Create a new access review for a tenant.
        /// </summary>
        public async Task<AccessReviewResult> CreateReviewAsync(
            Guid tenantId,
            Guid initiatedBy,
            string reviewType,
            string? name = null,
            string? description = null,
            int? dueDays = null)
        {
            try
            {
                var review = new AccessReview
                {
                    TenantId = tenantId,
                    Name = name ?? $"{reviewType} Access Review - {DateTime.UtcNow:yyyy-MM}",
                    ReviewType = reviewType,
                    Description = description,
                    InitiatedBy = initiatedBy,
                    Status = StatusDraft,
                    DueDate = DateTime.UtcNow.AddDays(dueDays ?? _options.CompletionDeadlineDays)
                };

                // Populate review items based on criteria
                var items = await GetReviewTargetsAsync(tenantId);

                foreach (var item in items)
                {
                    review.Items.Add(item);
                }

                review.TotalItems = review.Items.Count;

                _dbContext.Set<AccessReview>().Add(review);
                await _dbContext.SaveChangesAsync();

                // Log the creation
                await _auditService.LogAccessReviewInitiatedAsync(
                    review.Id,
                    tenantId,
                    reviewType,
                    initiatedBy,
                    review.TotalItems,
                    null);

                _logger.LogInformation(
                    "Access review created: {ReviewId} for tenant {TenantId} with {ItemCount} items",
                    review.Id, tenantId, review.TotalItems);

                return AccessReviewResult.Succeeded(review.Id, review.TotalItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating access review for tenant {TenantId}", tenantId);
                return AccessReviewResult.Failed("Failed to create access review.");
            }
        }

        /// <summary>
        /// Start an access review (move from Draft to InProgress).
        /// </summary>
        public async Task<bool> StartReviewAsync(Guid reviewId, Guid startedBy)
        {
            var review = await _dbContext.Set<AccessReview>().FindAsync(reviewId);
            if (review == null)
            {
                _logger.LogWarning("Cannot start review: Review {ReviewId} not found", reviewId);
                return false;
            }

            if (review.Status != StatusDraft)
            {
                _logger.LogWarning("Cannot start review {ReviewId}: Invalid status {Status} (expected Draft)", reviewId, review.Status);
                return false;
            }

            review.Status = StatusInProgress;
            review.StartedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Access review started: {ReviewId} by {StartedBy}", reviewId, startedBy);
            return true;
        }

        /// <summary>
        /// Submit a decision for a review item.
        /// Enforces governance controls: state validation, SoD, decision validation.
        /// </summary>
        public async Task<bool> SubmitDecisionAsync(
            Guid itemId,
            Guid reviewedBy,
            string decision,
            string? newRole = null,
            string? justification = null)
        {
            var item = await _dbContext.Set<AccessReviewItem>()
                .Include(i => i.Review)
                .FirstOrDefaultAsync(i => i.Id == itemId);

            if (item == null)
            {
                _logger.LogWarning("Cannot submit decision: Item {ItemId} not found", itemId);
                return false;
            }

            // Governance Control 1: Validate review state - must be InProgress
            if (item.Review == null || item.Review.Status != StatusInProgress)
            {
                _logger.LogWarning(
                    "Cannot submit decision for item {ItemId}: Review {ReviewId} is not in progress (status: {Status})",
                    itemId, item.ReviewId, item.Review?.Status ?? "null");
                return false;
            }

            // Governance Control 2: Separation of Duties - reviewer cannot review their own access
            if (reviewedBy == item.UserId)
            {
                _logger.LogWarning(
                    "SoD violation: User {UserId} attempted to review their own access in item {ItemId}",
                    reviewedBy, itemId);
                return false;
            }

            // Governance Control 3: Validate decision value
            var validDecisions = new[] { DecisionCertified, DecisionRevoked, DecisionModified };
            if (!validDecisions.Contains(decision, StringComparer.OrdinalIgnoreCase))
            {
                _logger.LogWarning(
                    "Invalid decision value '{Decision}' for item {ItemId}. Valid values: {ValidDecisions}",
                    decision, itemId, string.Join(", ", validDecisions));
                return false;
            }

            // Governance Control 4: Modified decision requires new role
            if (decision.Equals(DecisionModified, StringComparison.OrdinalIgnoreCase) &&
                string.IsNullOrWhiteSpace(newRole))
            {
                _logger.LogWarning(
                    "Cannot submit Modified decision for item {ItemId}: NewRole is required",
                    itemId);
                return false;
            }

            // Governance Control 5: Prevent re-decision after review completed (if completed somehow)
            if (item.Review.Status == StatusCompleted)
            {
                _logger.LogWarning(
                    "Cannot modify decision for item {ItemId}: Review {ReviewId} is already completed",
                    itemId, item.ReviewId);
                return false;
            }

            var previousDecision = item.Decision;
            item.Decision = decision;
            item.NewRole = newRole;
            item.Justification = justification;
            item.ReviewedBy = reviewedBy;
            item.ReviewedAt = DateTime.UtcNow;

            // Update review stats only if transitioning from Pending
            if (previousDecision == DecisionPending && decision != DecisionPending)
            {
                item.Review.ReviewedItems++;

                switch (decision)
                {
                    case DecisionCertified:
                        item.Review.CertifiedItems++;
                        break;
                    case DecisionRevoked:
                        item.Review.RevokedItems++;
                        break;
                    case DecisionModified:
                        item.Review.ModifiedItems++;
                        break;
                }
            }
            // Handle decision change (not from Pending) - adjust stats
            else if (previousDecision != DecisionPending && previousDecision != decision)
            {
                // Decrement previous decision count
                switch (previousDecision)
                {
                    case DecisionCertified:
                        item.Review.CertifiedItems--;
                        break;
                    case DecisionRevoked:
                        item.Review.RevokedItems--;
                        break;
                    case DecisionModified:
                        item.Review.ModifiedItems--;
                        break;
                }
                // Increment new decision count
                switch (decision)
                {
                    case DecisionCertified:
                        item.Review.CertifiedItems++;
                        break;
                    case DecisionRevoked:
                        item.Review.RevokedItems++;
                        break;
                    case DecisionModified:
                        item.Review.ModifiedItems++;
                        break;
                }
            }

            await _dbContext.SaveChangesAsync();

            // Log the decision with audit trail
            switch (decision)
            {
                case DecisionCertified:
                    await _auditService.LogAccessReviewItemCertifiedAsync(
                        item.ReviewId, item.UserId, item.CurrentRole,
                        reviewedBy, item.Review.TenantId, null);
                    break;
                case DecisionRevoked:
                    await _auditService.LogAccessReviewItemRevokedAsync(
                        item.ReviewId, item.UserId, item.CurrentRole,
                        reviewedBy, item.Review.TenantId, null);
                    break;
                case DecisionModified:
                    await _auditService.LogAccessReviewItemModifiedAsync(
                        item.ReviewId, item.UserId, item.CurrentRole, newRole!,
                        reviewedBy, item.Review.TenantId, null);
                    break;
            }

            _logger.LogInformation(
                "Review decision recorded: Item {ItemId}, User {UserId}, Decision: {Decision}, ReviewedBy: {ReviewedBy}",
                itemId, item.UserId, decision, reviewedBy);

            return true;
        }

        /// <summary>
        /// Complete an access review.
        /// </summary>
        public async Task<bool> CompleteReviewAsync(Guid reviewId, Guid completedBy)
        {
            var review = await _dbContext.Set<AccessReview>()
                .Include(r => r.Items)
                .FirstOrDefaultAsync(r => r.Id == reviewId);

            if (review == null)
            {
                _logger.LogWarning("Review not found: {ReviewId}", reviewId);
                return false;
            }

            // Validate state transition
            if (review.Status != StatusInProgress)
            {
                _logger.LogWarning(
                    "Cannot complete review {ReviewId}: Invalid status {Status} (expected InProgress)",
                    reviewId, review.Status);
                return false;
            }

            // Check all items are reviewed
            var pendingItems = review.Items.Count(i => i.Decision == DecisionPending);
            if (pendingItems > 0)
            {
                _logger.LogWarning(
                    "Cannot complete review {ReviewId}: {PendingCount} items still pending",
                    reviewId, pendingItems);
                return false;
            }

            // Mark review as complete
            review.Status = StatusCompleted;
            review.CompletedAt = DateTime.UtcNow;
            review.CompletedBy = completedBy;
            await _dbContext.SaveChangesAsync();

            // Log completion
            await _auditService.LogAccessReviewCompletedAsync(
                reviewId,
                review.TenantId,
                completedBy,
                review.CertifiedItems,
                review.RevokedItems,
                review.ModifiedItems,
                null);

            _logger.LogInformation(
                "Access review completed: {ReviewId}. Certified: {Certified}, Revoked: {Revoked}, Modified: {Modified}",
                reviewId, review.CertifiedItems, review.RevokedItems, review.ModifiedItems);

            return true;
        }

        /// <summary>
        /// Get access review details with items.
        /// </summary>
        public async Task<AccessReviewDto?> GetReviewAsync(Guid reviewId, Guid tenantId)
        {
            var review = await _dbContext.Set<AccessReview>()
                .Include(r => r.Items)
                .FirstOrDefaultAsync(r => r.Id == reviewId && r.TenantId == tenantId);

            if (review == null)
            {
                return null;
            }

            return MapToDto(review);
        }

        /// <summary>
        /// List all access reviews for a tenant.
        /// </summary>
        public async Task<AccessReviewListResult> ListReviewsAsync(
            Guid tenantId,
            string? status = null,
            int page = 1,
            int pageSize = 20)
        {
            var query = _dbContext.Set<AccessReview>()
                .Where(r => r.TenantId == tenantId);

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(r => r.Status == status);
            }

            var totalCount = await query.CountAsync();

            var reviews = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(r => r.Items)
                .ToListAsync();

            return new AccessReviewListResult
            {
                Reviews = reviews.Select(MapToDto).ToList(),
                TotalCount = totalCount
            };
        }

        /// <summary>
        /// Execute decisions (revoke/modify roles) for completed reviews.
        /// Only executes for reviews in Completed status.
        /// </summary>
        public async Task ExecuteDecisionsAsync(Guid reviewId)
        {
            var review = await _dbContext.Set<AccessReview>()
                .Include(r => r.Items)
                .FirstOrDefaultAsync(r => r.Id == reviewId);

            if (review == null)
            {
                _logger.LogWarning("Review not found for execution: {ReviewId}", reviewId);
                return;
            }

            // Must be completed before executing decisions
            if (review.Status != StatusCompleted)
            {
                _logger.LogWarning(
                    "Cannot execute decisions for review {ReviewId}: Status is {Status} (expected Completed)",
                    reviewId, review.Status);
                return;
            }

            var executedCount = 0;
            var failedCount = 0;
            var executorId = review.CompletedBy ?? review.InitiatedBy;

            foreach (var item in review.Items.Where(i => i.Decision != DecisionCertified && !i.IsExecuted))
            {
                // For Modified decisions, validate NewRole is present
                if (item.Decision == DecisionModified && string.IsNullOrWhiteSpace(item.NewRole))
                {
                    item.ExecutionError = "NewRole is required for Modified decision";
                    _logger.LogWarning(
                        "Cannot execute Modified decision for item {ItemId}: NewRole is missing",
                        item.Id);
                    failedCount++;
                    continue;
                }

                var success = await ExecuteDecisionAsync(item, review.TenantId, executorId);
                if (success)
                    executedCount++;
                else
                    failedCount++;
            }

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation(
                "Decisions executed for review {ReviewId}: {ExecutedCount} succeeded, {FailedCount} failed",
                reviewId, executedCount, failedCount);
        }

        /// <summary>
        /// Get all reviews for a tenant (internal helper).
        /// </summary>
        private async Task<IEnumerable<AccessReview>> GetReviewsInternalAsync(
            Guid tenantId,
            bool includeCompleted = false)
        {
            var query = _dbContext.Set<AccessReview>()
                .Where(r => r.TenantId == tenantId);

            if (!includeCompleted)
            {
                query = query.Where(r => r.Status != StatusCompleted && r.Status != StatusCancelled);
            }

            return await query
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get review items for a specific review.
        /// </summary>
        public async Task<IEnumerable<AccessReviewItem>> GetReviewItemsAsync(
            Guid reviewId,
            string? decisionFilter = null)
        {
            var query = _dbContext.Set<AccessReviewItem>()
                .Where(i => i.ReviewId == reviewId);

            if (!string.IsNullOrEmpty(decisionFilter))
            {
                query = query.Where(i => i.Decision == decisionFilter);
            }

            return await query
                .OrderBy(i => i.UserEmail)
                .ToListAsync();
        }

        /// <summary>
        /// Map AccessReview entity to DTO.
        /// </summary>
        private static AccessReviewDto MapToDto(AccessReview review)
        {
            return new AccessReviewDto
            {
                Id = review.Id,
                Name = review.Name,
                ReviewType = review.ReviewType,
                Status = review.Status,
                Description = review.Description,
                CreatedAt = review.CreatedAt,
                StartedAt = review.StartedAt,
                DueDate = review.DueDate,
                CompletedAt = review.CompletedAt,
                TotalItems = review.TotalItems,
                ReviewedItems = review.ReviewedItems,
                CertifiedItems = review.CertifiedItems,
                RevokedItems = review.RevokedItems,
                ModifiedItems = review.ModifiedItems,
                CompletionPercentage = review.CompletionPercentage,
                IsOverdue = review.IsOverdue,
                Items = review.Items.Select(i => new AccessReviewItemDto
                {
                    Id = i.Id,
                    UserId = i.UserId,
                    UserEmail = i.UserEmail,
                    UserDisplayName = i.UserDisplayName,
                    CurrentRole = i.CurrentRole,
                    UserStatus = i.UserStatus,
                    LastLoginAt = i.LastLoginAt,
                    InactiveDays = i.InactiveDays,
                    InclusionReason = i.InclusionReason,
                    Decision = i.Decision,
                    NewRole = i.NewRole,
                    Justification = i.Justification,
                    ReviewedBy = i.ReviewedBy,
                    ReviewedAt = i.ReviewedAt,
                    IsExecuted = i.IsExecuted,
                    ExecutedAt = i.ExecutedAt,
                    ExecutionError = i.ExecutionError
                }).ToList()
            };
        }

        /// <summary>
        /// Send reminder notifications for overdue reviews.
        /// Idempotent - only sends reminders once per configured interval.
        /// </summary>
        public async Task SendRemindersAsync()
        {
            var now = DateTime.UtcNow;
            var reminderIntervalHours = 24; // Default: one reminder per day

            var overdueReviews = await _dbContext.Set<AccessReview>()
                .Where(r => r.Status == StatusInProgress)
                .Where(r => r.DueDate != null && r.DueDate < now)
                .ToListAsync();

            var sentCount = 0;
            var skippedCount = 0;

            foreach (var review in overdueReviews)
            {
                // Idempotent check: skip if reminder was sent within the interval
                if (review.LastReminderSentAt.HasValue &&
                    (now - review.LastReminderSentAt.Value).TotalHours < reminderIntervalHours)
                {
                    skippedCount++;
                    continue;
                }

                var daysPastDue = review.DueDate.HasValue
                    ? (int)(now - review.DueDate.Value).TotalDays
                    : 0;

                // Log the overdue audit event
                await _auditService.LogAccessReviewOverdueAsync(
                    review.Id, review.TenantId, daysPastDue, null);

                // Update reminder tracking
                review.LastReminderSentAt = now;
                review.ReminderCount++;

                _logger.LogWarning(
                    "Access review overdue: {ReviewId} for tenant {TenantId}. Due: {DueDate}, Days past due: {DaysPastDue}, Reminder #{ReminderCount}",
                    review.Id, review.TenantId, review.DueDate, daysPastDue, review.ReminderCount);

                // TODO: Send notification to tenant admin via email/notification service

                sentCount++;
            }

            if (sentCount > 0 || skippedCount > 0)
            {
                await _dbContext.SaveChangesAsync();
            }

            _logger.LogInformation(
                "Access review reminders processed: {SentCount} sent, {SkippedCount} skipped (within interval)",
                sentCount, skippedCount);
        }

        /// <summary>
        /// Get users that should be included in access review.
        /// Filters using canonical UserStatus values and validates user IDs.
        /// </summary>
        private async Task<List<AccessReviewItem>> GetReviewTargetsAsync(Guid tenantId)
        {
            var items = new List<AccessReviewItem>();
            var now = DateTime.UtcNow;
            var inactivityThreshold = now.AddDays(-_options.InactiveUserThresholdDays);

            // Use canonical status values: include Active and Suspended users (not Deprovisioned, not pending states)
            var deprovisioned = UserStatus.Deprovisioned.ToString();
            var pendingVerification = UserStatus.PendingVerification.ToString();
            var pendingInvitation = UserStatus.PendingInvitation.ToString();
            var pendingPasswordSet = UserStatus.PendingPasswordSet.ToString();

            // Get all active/suspended tenant users (exclude pending and deprovisioned)
            var tenantUsers = await _dbContext.TenantUsers
                .Where(tu => tu.TenantId == tenantId)
                .Where(tu => tu.Status != deprovisioned)
                .Where(tu => tu.Status != pendingVerification)
                .Where(tu => tu.Status != pendingInvitation)
                .Where(tu => tu.Status != pendingPasswordSet)
                .ToListAsync();

            var skippedInvalidUsers = 0;

            foreach (var user in tenantUsers)
            {
                // Validate UserId can be parsed - skip invalid entries
                if (!Guid.TryParse(user.UserId, out var userIdGuid) || userIdGuid == Guid.Empty)
                {
                    _logger.LogWarning(
                        "Skipping user with invalid UserId format: {UserId} in tenant {TenantId}",
                        user.UserId, tenantId);
                    skippedInvalidUsers++;
                    continue;
                }

                var shouldInclude = false;
                var inclusionReason = new List<string>();

                // Include if in always-review roles
                if (_options.AlwaysReviewRoles?.Contains(user.Role, StringComparer.OrdinalIgnoreCase) == true)
                {
                    shouldInclude = true;
                    inclusionReason.Add("Privileged role");
                }

                // Include if auto-include privileged roles is enabled
                if (_options.AutoIncludePrivilegedRoles)
                {
                    var privilegedRoles = new[] { "TenantAdmin", "PlatformAdmin", "ComplianceManager", "AuditManager" };
                    if (privilegedRoles.Contains(user.Role, StringComparer.OrdinalIgnoreCase))
                    {
                        shouldInclude = true;
                        inclusionReason.Add("Privileged role");
                    }
                }

                // Include if inactive
                if (user.LastLoginAt != null && user.LastLoginAt < inactivityThreshold)
                {
                    shouldInclude = true;
                    inclusionReason.Add($"Inactive for {(now - user.LastLoginAt.Value).Days} days");
                }

                if (shouldInclude)
                {
                    var inactiveDays = user.LastLoginAt.HasValue
                        ? (int)(now - user.LastLoginAt.Value).TotalDays
                        : (int?)null;

                    items.Add(new AccessReviewItem
                    {
                        UserId = userIdGuid,
                        UserEmail = user.Email,
                        UserDisplayName = $"{user.FirstName} {user.LastName}".Trim(),
                        CurrentRole = user.Role,
                        UserStatus = user.Status,
                        LastLoginAt = user.LastLoginAt,
                        InactiveDays = inactiveDays,
                        InclusionReason = string.Join("; ", inclusionReason.Distinct()),
                        Decision = DecisionPending, // Explicitly set default decision
                        IsExecuted = false          // Explicitly set execution state
                    });
                }
            }

            if (skippedInvalidUsers > 0)
            {
                _logger.LogWarning(
                    "Skipped {SkippedCount} users with invalid UserIds in tenant {TenantId}",
                    skippedInvalidUsers, tenantId);
            }

            return items;
        }

        /// <summary>
        /// Execute a review decision (revoke or modify access).
        /// </summary>
        private async Task<bool> ExecuteDecisionAsync(
            AccessReviewItem item,
            Guid tenantId,
            Guid executedBy)
        {
            try
            {
                switch (item.Decision)
                {
                    case DecisionRevoked:
                        var revokeResult = await _roleService.RemoveRoleAsync(
                            executedBy, item.UserId, tenantId, $"Access review decision: {item.Justification}");
                        item.IsExecuted = revokeResult.Success;
                        item.ExecutionError = revokeResult.Success ? null : revokeResult.Message;
                        break;

                    case DecisionModified:
                        if (!string.IsNullOrEmpty(item.NewRole))
                        {
                            var modifyResult = await _roleService.AssignRoleAsync(
                                executedBy, item.UserId, tenantId, item.NewRole,
                                $"Access review decision: {item.Justification}");
                            item.IsExecuted = modifyResult.Success;
                            item.ExecutionError = modifyResult.Success ? null : modifyResult.Message;
                        }
                        else
                        {
                            item.ExecutionError = "NewRole is required for Modified decision";
                            _logger.LogWarning("Cannot execute Modified decision for item {ItemId}: NewRole is missing", item.Id);
                        }
                        break;
                }

                if (item.IsExecuted)
                {
                    item.ExecutedAt = DateTime.UtcNow;
                }

                return item.IsExecuted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing review decision for item {ItemId}", item.Id);
                item.ExecutionError = ex.Message;
                return false;
            }
        }
    }
}
