using GrcMvc.Data;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// AM-12: Separation of Duties (SoD) enforcement service.
    /// Prevents conflicting actions by the same user based on defined rules.
    /// </summary>
    public class SoDEnforcementService
    {
        private readonly GrcDbContext _dbContext;
        private readonly IDistributedCache _cache;
        private readonly IAccessManagementAuditService _auditService;
        private readonly ILogger<SoDEnforcementService> _logger;

        private const string SoDRulesCacheKey = "sod_rules";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);

        /// <summary>
        /// Built-in SoD rules for common GRC operations.
        /// </summary>
        private static readonly List<SoDRule> BuiltInRules = new()
        {
            // Evidence management
            new SoDRule
            {
                Id = Guid.Parse("00000001-0001-0001-0001-000000000001"),
                Name = "Evidence Create/Approve",
                Action1 = "evidence.create",
                Action2 = "evidence.approve",
                Enforcement = SoDEnforcement.Block,
                Description = "Same user cannot create and approve the same evidence"
            },
            new SoDRule
            {
                Id = Guid.Parse("00000001-0001-0001-0001-000000000002"),
                Name = "Evidence Upload/Verify",
                Action1 = "evidence.upload",
                Action2 = "evidence.verify",
                Enforcement = SoDEnforcement.Block,
                Description = "Same user cannot upload and verify evidence"
            },

            // Control management
            new SoDRule
            {
                Id = Guid.Parse("00000001-0001-0001-0001-000000000003"),
                Name = "Control Test/Certify",
                Action1 = "control.test",
                Action2 = "control.certify",
                Enforcement = SoDEnforcement.Block,
                Description = "Same user cannot test and certify a control"
            },
            new SoDRule
            {
                Id = Guid.Parse("00000001-0001-0001-0001-000000000004"),
                Name = "Control Design/Test",
                Action1 = "control.design",
                Action2 = "control.test",
                Enforcement = SoDEnforcement.Warn,
                Description = "Same user should not design and test a control"
            },

            // Risk management
            new SoDRule
            {
                Id = Guid.Parse("00000001-0001-0001-0001-000000000005"),
                Name = "Risk Assess/Accept",
                Action1 = "risk.assess",
                Action2 = "risk.accept",
                Enforcement = SoDEnforcement.Warn,
                Description = "Same user should not assess and accept a risk"
            },
            new SoDRule
            {
                Id = Guid.Parse("00000001-0001-0001-0001-000000000006"),
                Name = "Risk Identify/Treat",
                Action1 = "risk.identify",
                Action2 = "risk.treat",
                Enforcement = SoDEnforcement.Warn,
                Description = "Same user should not identify and treat a risk"
            },

            // Audit management
            new SoDRule
            {
                Id = Guid.Parse("00000001-0001-0001-0001-000000000007"),
                Name = "Audit Plan/Execute",
                Action1 = "audit.plan",
                Action2 = "audit.execute",
                Enforcement = SoDEnforcement.Warn,
                Description = "Same user should not plan and execute an audit"
            },
            new SoDRule
            {
                Id = Guid.Parse("00000001-0001-0001-0001-000000000008"),
                Name = "Audit Execute/Report",
                Action1 = "audit.execute",
                Action2 = "audit.report",
                Enforcement = SoDEnforcement.Block,
                Description = "Same user cannot execute and issue audit report"
            },

            // Policy management
            new SoDRule
            {
                Id = Guid.Parse("00000001-0001-0001-0001-000000000009"),
                Name = "Policy Draft/Approve",
                Action1 = "policy.draft",
                Action2 = "policy.approve",
                Enforcement = SoDEnforcement.Block,
                Description = "Same user cannot draft and approve a policy"
            },

            // User management
            new SoDRule
            {
                Id = Guid.Parse("00000001-0001-0001-0001-00000000000A"),
                Name = "User Create/Approve",
                Action1 = "user.invite",
                Action2 = "user.approve",
                Enforcement = SoDEnforcement.Block,
                Description = "Same user cannot invite and approve a new user"
            },

            // Vendor management
            new SoDRule
            {
                Id = Guid.Parse("00000001-0001-0001-0001-00000000000B"),
                Name = "Vendor Assess/Approve",
                Action1 = "vendor.assess",
                Action2 = "vendor.approve",
                Enforcement = SoDEnforcement.Block,
                Description = "Same user cannot assess and approve a vendor"
            }
        };

        public SoDEnforcementService(
            GrcDbContext dbContext,
            IDistributedCache cache,
            IAccessManagementAuditService auditService,
            ILogger<SoDEnforcementService> logger)
        {
            _dbContext = dbContext;
            _cache = cache;
            _auditService = auditService;
            _logger = logger;
        }

        /// <summary>
        /// Check if an action violates SoD rules for a user.
        /// </summary>
        /// <param name="userId">User attempting the action</param>
        /// <param name="tenantId">Tenant context</param>
        /// <param name="action">Action being attempted (e.g., "evidence.approve")</param>
        /// <param name="entityId">Entity the action is being performed on</param>
        /// <returns>SoD check result</returns>
        public async Task<SoDCheckResult> CheckActionAsync(
            Guid userId,
            Guid tenantId,
            string action,
            Guid? entityId = null)
        {
            try
            {
                // Get all applicable rules
                var rules = await GetRulesAsync(tenantId);

                // Find rules where this action is involved
                var applicableRules = rules
                    .Where(r => r.IsEnabled &&
                               (r.Action1.Equals(action, StringComparison.OrdinalIgnoreCase) ||
                                r.Action2.Equals(action, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                if (!applicableRules.Any())
                {
                    return SoDCheckResult.Allowed();
                }

                // Check each rule
                var violations = new List<SoDViolation>();

                foreach (var rule in applicableRules)
                {
                    var conflictingAction = rule.Action1.Equals(action, StringComparison.OrdinalIgnoreCase)
                        ? rule.Action2
                        : rule.Action1;

                    // Check if user has performed the conflicting action on this entity
                    var hasConflict = await HasPerformedActionAsync(
                        userId, tenantId, conflictingAction, entityId, rule.TimeWindowHours);

                    if (hasConflict)
                    {
                        var violation = new SoDViolation
                        {
                            RuleId = rule.Id,
                            RuleName = rule.Name,
                            Action = action,
                            ConflictingAction = conflictingAction,
                            Enforcement = rule.Enforcement,
                            Description = rule.Description
                        };

                        violations.Add(violation);

                        // Log the violation
                        await _auditService.LogSoDViolationDetectedAsync(
                            userId, tenantId, rule.Name, action, conflictingAction, null);

                        _logger.LogWarning(
                            "SoD violation detected: User {UserId} attempted {Action} but previously performed {ConflictingAction}. Rule: {RuleName}",
                            userId, action, conflictingAction, rule.Name);
                    }
                }

                if (!violations.Any())
                {
                    return SoDCheckResult.Allowed();
                }

                // Check enforcement level
                var hasBlockingViolation = violations.Any(v => v.Enforcement == SoDEnforcement.Block);

                if (hasBlockingViolation)
                {
                    var blockingViolation = violations.First(v => v.Enforcement == SoDEnforcement.Block);
                    await _auditService.LogSoDViolationBlockedAsync(
                        userId, tenantId, blockingViolation.RuleName,
                        action, blockingViolation.ConflictingAction,
                        null);

                    return SoDCheckResult.Blocked(violations);
                }

                return SoDCheckResult.Warning(violations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking SoD for user {UserId}, action {Action}", userId, action);
                // Fail open - don't block users due to SoD service errors
                return SoDCheckResult.Allowed();
            }
        }

        /// <summary>
        /// Record that a user performed an action (for SoD tracking).
        /// </summary>
        public async Task RecordActionAsync(
            Guid userId,
            Guid tenantId,
            string action,
            Guid? entityId,
            string? entityType = null)
        {
            try
            {
                var record = new SoDActionRecord
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    TenantId = tenantId,
                    Action = action,
                    EntityId = entityId,
                    EntityType = entityType,
                    PerformedAt = DateTime.UtcNow
                };

                // Store in database for persistence
                _dbContext.Set<SoDActionRecord>().Add(record);
                await _dbContext.SaveChangesAsync();

                // Also cache recent actions for fast lookup
                var cacheKey = $"sod_actions:{tenantId}:{userId}:{entityId}";
                var existing = await _cache.GetStringAsync(cacheKey);
                var actions = string.IsNullOrEmpty(existing)
                    ? new List<CachedAction>()
                    : JsonSerializer.Deserialize<List<CachedAction>>(existing) ?? new();

                actions.Add(new CachedAction { Action = action, PerformedAt = DateTime.UtcNow });

                // Keep only last 24 hours of actions
                actions = actions.Where(a => a.PerformedAt > DateTime.UtcNow.AddHours(-24)).ToList();

                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(actions),
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording SoD action for user {UserId}", userId);
            }
        }

        /// <summary>
        /// Request an override for a SoD violation.
        /// </summary>
        public async Task<SoDOverrideResult> RequestOverrideAsync(
            Guid requesterId,
            Guid tenantId,
            Guid ruleId,
            string action,
            Guid? entityId,
            string justification)
        {
            var rule = (await GetRulesAsync(tenantId)).FirstOrDefault(r => r.Id == ruleId);
            if (rule == null || !rule.AllowOverride)
            {
                return SoDOverrideResult.Denied("This rule does not allow overrides.");
            }

            // Log the override request
            var conflictingAction = rule.Action1.Equals(action, StringComparison.OrdinalIgnoreCase)
                ? rule.Action2
                : rule.Action1;
            await _auditService.LogSoDOverrideRequestedAsync(
                requesterId, string.Empty, action, conflictingAction, tenantId, justification);

            // In a full implementation, this would create an approval workflow
            // For now, return pending status
            return new SoDOverrideResult
            {
                Success = true,
                Status = "PendingApproval",
                Message = "Override request submitted for approval."
            };
        }

        /// <summary>
        /// Approve or deny a SoD override request.
        /// </summary>
        public async Task<SoDOverrideResult> ProcessOverrideAsync(
            Guid approverId,
            Guid tenantId,
            Guid overrideRequestId,
            bool approved,
            string? comments = null)
        {
            // In a full implementation, this would update the override request
            // and log the decision

            if (approved)
            {
                await _auditService.LogSoDOverrideApprovedAsync(
                    overrideRequestId, string.Empty, "action", "conflictingAction", tenantId, approverId);
                return SoDOverrideResult.Approved();
            }
            else
            {
                await _auditService.LogSoDOverrideDeniedAsync(
                    overrideRequestId, string.Empty, "action", "conflictingAction", tenantId, approverId, comments ?? "Override request denied.");
                return SoDOverrideResult.Denied(comments ?? "Override request denied.");
            }
        }

        /// <summary>
        /// Get all SoD rules for a tenant.
        /// </summary>
        public async Task<IEnumerable<SoDRule>> GetRulesAsync(Guid tenantId)
        {
            // Try cache first
            var cacheKey = $"{SoDRulesCacheKey}:{tenantId}";
            var cached = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cached))
            {
                var cachedRules = JsonSerializer.Deserialize<List<SoDRule>>(cached);
                if (cachedRules != null)
                    return cachedRules;
            }

            // Get tenant-specific rules from database
            var tenantRules = await _dbContext.Set<SoDRule>()
                .Where(r => r.TenantId == tenantId || r.TenantId == null)
                .ToListAsync();

            // Merge with built-in rules (built-in rules can be overridden by tenant rules)
            var allRules = new List<SoDRule>(BuiltInRules);
            foreach (var rule in tenantRules)
            {
                var existing = allRules.FindIndex(r => r.Id == rule.Id);
                if (existing >= 0)
                    allRules[existing] = rule;
                else
                    allRules.Add(rule);
            }

            // Cache the result
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(allRules),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheDuration });

            return allRules;
        }

        private async Task<bool> HasPerformedActionAsync(
            Guid userId,
            Guid tenantId,
            string action,
            Guid? entityId,
            int? timeWindowHours)
        {
            // Check cache first for recent actions
            var cacheKey = $"sod_actions:{tenantId}:{userId}:{entityId}";
            var cached = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cached))
            {
                var actions = JsonSerializer.Deserialize<List<CachedAction>>(cached);
                if (actions != null)
                {
                    var threshold = timeWindowHours.HasValue
                        ? DateTime.UtcNow.AddHours(-timeWindowHours.Value)
                        : DateTime.MinValue;

                    return actions.Any(a =>
                        a.Action.Equals(action, StringComparison.OrdinalIgnoreCase) &&
                        a.PerformedAt > threshold);
                }
            }

            // Fall back to database
            var query = _dbContext.Set<SoDActionRecord>()
                .Where(r => r.UserId == userId && r.TenantId == tenantId)
                .Where(r => r.Action == action);

            if (entityId.HasValue)
            {
                query = query.Where(r => r.EntityId == entityId);
            }

            if (timeWindowHours.HasValue)
            {
                var threshold = DateTime.UtcNow.AddHours(-timeWindowHours.Value);
                query = query.Where(r => r.PerformedAt > threshold);
            }

            return await query.AnyAsync();
        }

        private class CachedAction
        {
            public string Action { get; set; } = string.Empty;
            public DateTime PerformedAt { get; set; }
        }
    }

    /// <summary>
    /// SoD rule definition.
    /// </summary>
    public class SoDRule
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Action1 { get; set; } = string.Empty;
        public string Action2 { get; set; } = string.Empty;
        public SoDEnforcement Enforcement { get; set; } = SoDEnforcement.Block;
        public string? Description { get; set; }
        public Guid? TenantId { get; set; } // Null for system rules
        public bool IsEnabled { get; set; } = true;
        public bool AllowOverride { get; set; } = false;
        public int? TimeWindowHours { get; set; } // Time window for SoD check
    }

    /// <summary>
    /// SoD action record for tracking.
    /// </summary>
    public class SoDActionRecord
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid TenantId { get; set; }
        public string Action { get; set; } = string.Empty;
        public Guid? EntityId { get; set; }
        public string? EntityType { get; set; }
        public DateTime PerformedAt { get; set; }
    }

    /// <summary>
    /// SoD enforcement level.
    /// </summary>
    public enum SoDEnforcement
    {
        Warn,   // Allow but warn
        Block   // Prevent the action
    }

    /// <summary>
    /// Result of SoD check.
    /// </summary>
    public class SoDCheckResult
    {
        public bool IsAllowed { get; set; }
        public bool IsWarning { get; set; }
        public List<SoDViolation> Violations { get; set; } = new();

        public static SoDCheckResult Allowed() => new() { IsAllowed = true };

        public static SoDCheckResult Warning(List<SoDViolation> violations) => new()
        {
            IsAllowed = true,
            IsWarning = true,
            Violations = violations
        };

        public static SoDCheckResult Blocked(List<SoDViolation> violations) => new()
        {
            IsAllowed = false,
            Violations = violations
        };
    }

    /// <summary>
    /// SoD violation detail.
    /// </summary>
    public class SoDViolation
    {
        public Guid RuleId { get; set; }
        public string RuleName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string ConflictingAction { get; set; } = string.Empty;
        public SoDEnforcement Enforcement { get; set; }
        public string? Description { get; set; }
    }

    /// <summary>
    /// Result of SoD override request.
    /// </summary>
    public class SoDOverrideResult
    {
        public bool Success { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Message { get; set; }

        public static SoDOverrideResult Approved() => new()
        {
            Success = true,
            Status = "Approved",
            Message = "Override approved."
        };

        public static SoDOverrideResult Denied(string reason) => new()
        {
            Success = false,
            Status = "Denied",
            Message = reason
        };
    }
}
