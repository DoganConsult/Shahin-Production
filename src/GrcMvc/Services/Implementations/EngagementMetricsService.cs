using System.Text.Json;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Engagement Metrics Service - Tracks user engagement and calculates motivation scores.
/// Implements the fullplan specification for behavioral analytics and gamification.
/// </summary>
public class EngagementMetricsService : IEngagementMetricsService
{
    private readonly GrcDbContext _dbContext;
    private readonly ILogger<EngagementMetricsService> _logger;

    public EngagementMetricsService(
        GrcDbContext dbContext,
        ILogger<EngagementMetricsService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<EngagementMetrics> GetOrCreateMetricsAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var metrics = await _dbContext.Set<EngagementMetrics>()
            .FirstOrDefaultAsync(m => m.TenantId == tenantId && m.UserId == userId, cancellationToken);

        if (metrics == null)
        {
            metrics = new EngagementMetrics
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                UserId = userId,
                CurrentState = EngagementStates.Explorer,
                CreatedDate = DateTime.UtcNow
            };

            _dbContext.Set<EngagementMetrics>().Add(metrics);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Created engagement metrics for user {UserId} in tenant {TenantId}",
                userId, tenantId);
        }

        return metrics;
    }

    public async Task<EngagementMetrics> RecordActivityAsync(
        Guid tenantId,
        Guid userId,
        string activityType,
        Dictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var metrics = await GetOrCreateMetricsAsync(tenantId, userId, cancellationToken);
            var now = DateTime.UtcNow;

            // Update activity counts
            metrics.TotalActivities++;
            metrics.LastActivityAt = now;

            // Update activity type counts
            var activityCounts = !string.IsNullOrEmpty(metrics.ActivityCountsJson)
                ? JsonSerializer.Deserialize<Dictionary<string, int>>(metrics.ActivityCountsJson) ?? new()
                : new Dictionary<string, int>();

            activityCounts[activityType] = activityCounts.TryGetValue(activityType, out var count) ? count + 1 : 1;
            metrics.ActivityCountsJson = JsonSerializer.Serialize(activityCounts);

            // Calculate streak
            if (metrics.LastActivityAt.HasValue)
            {
                var daysSinceLastActivity = (now.Date - metrics.LastActivityAt.Value.Date).TotalDays;
                if (daysSinceLastActivity <= 1)
                {
                    metrics.CurrentStreak++;
                    if (metrics.CurrentStreak > metrics.LongestStreak)
                    {
                        metrics.LongestStreak = metrics.CurrentStreak;
                    }
                }
                else if (daysSinceLastActivity > 1)
                {
                    metrics.CurrentStreak = 1;
                }
            }
            else
            {
                metrics.CurrentStreak = 1;
            }

            // Calculate points based on activity type
            var points = GetPointsForActivity(activityType);
            metrics.TotalPoints += points;

            // Update weekly/monthly points
            var weekStart = now.Date.AddDays(-(int)now.DayOfWeek);
            var monthStart = new DateTime(now.Year, now.Month, 1);

            if (metrics.WeeklyPointsResetAt < weekStart)
            {
                metrics.WeeklyPoints = points;
                metrics.WeeklyPointsResetAt = weekStart;
            }
            else
            {
                metrics.WeeklyPoints += points;
            }

            if (metrics.MonthlyPointsResetAt < monthStart)
            {
                metrics.MonthlyPoints = points;
                metrics.MonthlyPointsResetAt = monthStart;
            }
            else
            {
                metrics.MonthlyPoints += points;
            }

            // Check for level up
            var newLevel = CalculateLevel(metrics.TotalPoints);
            if (newLevel > metrics.Level)
            {
                metrics.Level = newLevel;
                _logger.LogInformation(
                    "User {UserId} leveled up to {Level}",
                    userId, newLevel);
            }

            // Update engagement state based on behavior
            await UpdateEngagementStateAsync(metrics, cancellationToken);

            metrics.ModifiedDate = now;
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Record the activity in history
            var activityRecord = new UserActivity
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                UserId = userId,
                ActivityType = activityType,
                PointsEarned = points,
                MetadataJson = metadata != null ? JsonSerializer.Serialize(metadata) : null,
                CreatedDate = now
            };

            _dbContext.Set<UserActivity>().Add(activityRecord);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogDebug(
                "Recorded activity {ActivityType} for user {UserId}: +{Points} points",
                activityType, userId, points);

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error recording activity {ActivityType} for user {UserId}",
                activityType, userId);
            throw;
        }
    }

    public async Task<MotivationScore> CalculateMotivationScoreAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var metrics = await GetOrCreateMetricsAsync(tenantId, userId, cancellationToken);
            var now = DateTime.UtcNow;

            // Calculate component scores
            var engagementScore = CalculateEngagementComponent(metrics);
            var consistencyScore = CalculateConsistencyComponent(metrics);
            var progressScore = await CalculateProgressComponentAsync(tenantId, userId, cancellationToken);
            var qualityScore = await CalculateQualityComponentAsync(tenantId, userId, cancellationToken);
            var collaborationScore = await CalculateCollaborationComponentAsync(tenantId, userId, cancellationToken);

            // Calculate weighted overall score
            var overallScore = (int)Math.Round(
                engagementScore * 0.25 +
                consistencyScore * 0.20 +
                progressScore * 0.25 +
                qualityScore * 0.15 +
                collaborationScore * 0.15
            );

            // Determine trend
            var previousScore = await _dbContext.Set<MotivationScore>()
                .Where(m => m.TenantId == tenantId && m.UserId == userId)
                .OrderByDescending(m => m.ScoredAt)
                .Select(m => m.OverallScore)
                .FirstOrDefaultAsync(cancellationToken);

            var trend = overallScore > previousScore ? "Improving" :
                       overallScore < previousScore ? "Declining" : "Stable";

            // Identify risk factors
            var riskFactors = new List<string>();
            if (engagementScore < 40) riskFactors.Add("Low engagement activity");
            if (consistencyScore < 40) riskFactors.Add("Inconsistent platform usage");
            if (progressScore < 40) riskFactors.Add("Slow task completion");
            if (qualityScore < 40) riskFactors.Add("Quality concerns in submissions");
            if (collaborationScore < 40) riskFactors.Add("Limited team collaboration");
            if (metrics.CurrentStreak < 3) riskFactors.Add("No active streak");

            // Generate recommendations
            var recommendations = GenerateMotivationRecommendations(
                engagementScore, consistencyScore, progressScore, qualityScore, collaborationScore);

            var motivationScore = new MotivationScore
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                UserId = userId,
                OverallScore = overallScore,
                EngagementScore = engagementScore,
                ConsistencyScore = consistencyScore,
                ProgressScore = progressScore,
                QualityScore = qualityScore,
                CollaborationScore = collaborationScore,
                Trend = trend,
                RiskFactors = riskFactors,
                Recommendations = recommendations,
                RequiresIntervention = overallScore < 40 || riskFactors.Count >= 3,
                ScoredAt = now,
                CreatedDate = now
            };

            _dbContext.Set<MotivationScore>().Add(motivationScore);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Calculated motivation score for user {UserId}: {Score} ({Trend})",
                userId, overallScore, trend);

            return motivationScore;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error calculating motivation score for user {UserId}", userId);
            throw;
        }
    }

    public async Task<MotivationScore?> GetLatestMotivationScoreAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<MotivationScore>()
            .Where(m => m.TenantId == tenantId && m.UserId == userId)
            .OrderByDescending(m => m.ScoredAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<EngagementMetrics>> GetTeamMetricsAsync(
        Guid tenantId,
        Guid? workspaceId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<EngagementMetrics>()
            .Where(m => m.TenantId == tenantId);

        if (workspaceId.HasValue)
        {
            query = query.Where(m => m.WorkspaceId == workspaceId);
        }

        return await query
            .OrderByDescending(m => m.TotalPoints)
            .ToListAsync(cancellationToken);
    }

    public async Task<LeaderboardResult> GetLeaderboardAsync(
        Guid tenantId,
        string period = "weekly",
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<EngagementMetrics>()
            .Where(m => m.TenantId == tenantId);

        List<EngagementMetrics> topUsers;

        switch (period.ToLower())
        {
            case "daily":
                var today = DateTime.UtcNow.Date;
                topUsers = await query
                    .Where(m => m.LastActivityAt >= today)
                    .OrderByDescending(m => m.TotalPoints)
                    .Take(limit)
                    .ToListAsync(cancellationToken);
                break;

            case "weekly":
                topUsers = await query
                    .OrderByDescending(m => m.WeeklyPoints)
                    .Take(limit)
                    .ToListAsync(cancellationToken);
                break;

            case "monthly":
                topUsers = await query
                    .OrderByDescending(m => m.MonthlyPoints)
                    .Take(limit)
                    .ToListAsync(cancellationToken);
                break;

            case "alltime":
            default:
                topUsers = await query
                    .OrderByDescending(m => m.TotalPoints)
                    .Take(limit)
                    .ToListAsync(cancellationToken);
                break;
        }

        var entries = topUsers.Select((m, index) => new LeaderboardEntry
        {
            Rank = index + 1,
            UserId = m.UserId ?? Guid.Empty,
            Points = period.ToLower() switch
            {
                "weekly" => m.WeeklyPoints,
                "monthly" => m.MonthlyPoints,
                _ => m.TotalPoints
            },
            Level = m.Level,
            CurrentStreak = m.CurrentStreak,
            EngagementState = m.CurrentState ?? string.Empty
        }).ToList();

        return new LeaderboardResult
        {
            Period = period,
            Entries = entries,
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<List<Badge>> GetUserBadgesAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<UserBadge>()
            .Where(ub => ub.TenantId == tenantId && ub.UserId == userId)
            .Select(ub => ub.Badge)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserBadge?> AwardBadgeAsync(
        Guid tenantId,
        Guid userId,
        string badgeCode,
        CancellationToken cancellationToken = default)
    {
        // Check if badge exists
        var badge = await _dbContext.Set<Badge>()
            .FirstOrDefaultAsync(b => b.Code == badgeCode && b.IsActive, cancellationToken);

        if (badge == null)
        {
            _logger.LogWarning("Badge {BadgeCode} not found or inactive", badgeCode);
            return null;
        }

        // Check if user already has the badge
        var existingBadge = await _dbContext.Set<UserBadge>()
            .FirstOrDefaultAsync(ub =>
                ub.TenantId == tenantId &&
                ub.UserId == userId &&
                ub.BadgeId == badge.Id,
                cancellationToken);

        if (existingBadge != null)
        {
            _logger.LogDebug(
                "User {UserId} already has badge {BadgeCode}",
                userId, badgeCode);
            return existingBadge;
        }

        // Award the badge
        var userBadge = new UserBadge
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = userId,
            BadgeId = badge.Id,
            AwardedAt = DateTime.UtcNow,
            CreatedDate = DateTime.UtcNow
        };

        _dbContext.Set<UserBadge>().Add(userBadge);

        // Update user points
        var metrics = await GetOrCreateMetricsAsync(tenantId, userId, cancellationToken);
        metrics.TotalPoints += badge.PointsValue;
        metrics.BadgeCount++;
        metrics.ModifiedDate = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Awarded badge {BadgeCode} to user {UserId} (+{Points} points)",
            badgeCode, userId, badge.PointsValue);

        return userBadge;
    }

    public async Task CheckAndAwardBadgesAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var metrics = await GetOrCreateMetricsAsync(tenantId, userId, cancellationToken);

        // Get all active badges
        var badges = await _dbContext.Set<Badge>()
            .Where(b => b.IsActive)
            .ToListAsync(cancellationToken);

        // Get user's existing badges
        var existingBadges = await _dbContext.Set<UserBadge>()
            .Where(ub => ub.TenantId == tenantId && ub.UserId == userId)
            .Select(ub => ub.BadgeId)
            .ToListAsync(cancellationToken);

        foreach (var badge in badges.Where(b => !existingBadges.Contains(b.Id)))
        {
            var shouldAward = await EvaluateBadgeCriteriaAsync(
                badge, metrics, tenantId, userId, cancellationToken);

            if (shouldAward)
            {
                await AwardBadgeAsync(tenantId, userId, badge.Code, cancellationToken);
            }
        }
    }

    #region IEngagementMetricsService Interface Methods

    public async Task<EngagementMetrics> RecordMetricsAsync(
        Guid tenantId,
        Guid? userId,
        EngagementMetricsInput input,
        CancellationToken cancellationToken = default)
    {
        if (!userId.HasValue)
        {
            throw new ArgumentNullException(nameof(userId), "UserId is required for recording metrics");
        }

        // Record activities based on input
        var metrics = await GetOrCreateMetricsAsync(tenantId, userId.Value, cancellationToken);

        // Record tasks completed
        for (int i = 0; i < input.TasksCompleted; i++)
        {
            await RecordActivityAsync(tenantId, userId.Value, "TASK_COMPLETE", null, cancellationToken);
        }

        // Record evidence submitted
        for (int i = 0; i < input.EvidenceSubmitted; i++)
        {
            await RecordActivityAsync(tenantId, userId.Value, "EVIDENCE_SUBMIT", null, cancellationToken);
        }

        // Update session-related metrics
        metrics.ModifiedDate = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return metrics;
    }

    public async Task<EngagementMetrics?> GetCurrentMetricsAsync(
        Guid tenantId,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        if (!userId.HasValue)
        {
            // Return tenant-level metrics (first user or null)
            return await _dbContext.Set<EngagementMetrics>()
                .Where(m => m.TenantId == tenantId)
                .OrderByDescending(m => m.ModifiedDate)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return await _dbContext.Set<EngagementMetrics>()
            .FirstOrDefaultAsync(m => m.TenantId == tenantId && m.UserId == userId.Value, cancellationToken);
    }

    public async Task<List<EngagementMetrics>> GetEngagementHistoryAsync(
        Guid tenantId,
        Guid? userId = null,
        int days = 30,
        CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);

        var query = _dbContext.Set<EngagementMetrics>()
            .Where(m => m.TenantId == tenantId && m.ModifiedDate >= cutoffDate);

        if (userId.HasValue)
        {
            query = query.Where(m => m.UserId == userId.Value);
        }

        return await query
            .OrderByDescending(m => m.ModifiedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<MotivationScore> CalculateMotivationScoreAsync(
        Guid tenantId,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        if (!userId.HasValue)
        {
            // Get first active user for tenant if no userId specified
            var firstUser = await _dbContext.Set<EngagementMetrics>()
                .Where(m => m.TenantId == tenantId)
                .OrderByDescending(m => m.LastActivityAt)
                .Select(m => m.UserId)
                .FirstOrDefaultAsync(cancellationToken);

            if (firstUser == Guid.Empty)
            {
                // Return a default motivation score
                return new MotivationScore
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    OverallScore = 50,
                    EngagementScore = 50,
                    ConsistencyScore = 50,
                    ProgressScore = 50,
                    QualityScore = 50,
                    CollaborationScore = 50,
                    Trend = "Stable",
                    RiskFactors = new List<string>(),
                    Recommendations = new List<string> { "Start engaging with the platform" },
                    ScoredAt = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow
                };
            }

            userId = firstUser;
        }

        // Call the existing method with Guid
        return await CalculateMotivationScoreAsync(tenantId, userId.Value, cancellationToken);
    }

    public async Task<EngagementAnalysisResult> AnalyzeEngagementAsync(
        Guid tenantId,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var metrics = await GetCurrentMetricsAsync(tenantId, userId, cancellationToken);

            if (metrics == null)
            {
                return new EngagementAnalysisResult
                {
                    Success = true,
                    EngagementState = EngagementStates.Explorer,
                    OverallScore = 50,
                    ConfidenceScore = 0,
                    FatigueScore = 0,
                    MomentumScore = 50,
                    RecommendedActions = new List<string> { "Start by exploring the platform features" },
                    AnalysisSummary = "New user - no engagement data yet"
                };
            }

            // Calculate component scores
            var engagementScore = CalculateEngagementComponent(metrics);
            var consistencyScore = CalculateConsistencyComponent(metrics);

            // Calculate fatigue based on recent activity intensity
            var fatigueScore = await CalculateFatigueScoreAsync(tenantId, userId, cancellationToken);

            // Calculate momentum based on trend
            var momentumScore = await CalculateMomentumScoreAsync(tenantId, userId, cancellationToken);

            // Calculate confidence in the analysis
            var confidenceScore = metrics.TotalActivities >= 10 ? 80 :
                                 metrics.TotalActivities >= 5 ? 60 :
                                 metrics.TotalActivities >= 1 ? 40 : 20;

            // Overall score
            var overallScore = (engagementScore + consistencyScore + momentumScore) / 3;

            // Generate recommended actions
            var recommendedActions = new List<string>();
            if (engagementScore < 50)
                recommendedActions.Add("Log in daily to maintain engagement streak");
            if (consistencyScore < 50)
                recommendedActions.Add("Complete at least one task per session");
            if (fatigueScore > 70)
                recommendedActions.Add("Consider taking a short break to avoid burnout");
            if (momentumScore < 40)
                recommendedActions.Add("Focus on quick wins to build momentum");

            if (recommendedActions.Count == 0)
                recommendedActions.Add("Keep up the great work!");

            return new EngagementAnalysisResult
            {
                Success = true,
                EngagementState = metrics.CurrentState ?? EngagementStates.Explorer,
                OverallScore = overallScore,
                ConfidenceScore = confidenceScore,
                FatigueScore = fatigueScore,
                MomentumScore = momentumScore,
                RecommendedActions = recommendedActions,
                AnalysisSummary = $"User is in {metrics.CurrentState} state with {metrics.CurrentStreak} day streak"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing engagement for tenant {TenantId}", tenantId);
            return new EngagementAnalysisResult
            {
                Success = false,
                AnalysisSummary = "Error analyzing engagement"
            };
        }
    }

    private async Task<int> CalculateFatigueScoreAsync(
        Guid tenantId,
        Guid? userId,
        CancellationToken cancellationToken)
    {
        // Calculate fatigue based on recent intense activity
        var recentActivities = await _dbContext.Set<UserActivity>()
            .Where(a => a.TenantId == tenantId &&
                       (!userId.HasValue || a.UserId == userId.Value) &&
                       a.CreatedDate >= DateTime.UtcNow.AddHours(-24))
            .CountAsync(cancellationToken);

        // High activity in 24h suggests potential fatigue
        if (recentActivities > 50) return 80;
        if (recentActivities > 30) return 60;
        if (recentActivities > 20) return 40;
        return 20;
    }

    private async Task<int> CalculateMomentumScoreAsync(
        Guid tenantId,
        Guid? userId,
        CancellationToken cancellationToken)
    {
        // Compare recent activity to previous period
        var recent = await _dbContext.Set<UserActivity>()
            .Where(a => a.TenantId == tenantId &&
                       (!userId.HasValue || a.UserId == userId.Value) &&
                       a.CreatedDate >= DateTime.UtcNow.AddDays(-7))
            .CountAsync(cancellationToken);

        var previous = await _dbContext.Set<UserActivity>()
            .Where(a => a.TenantId == tenantId &&
                       (!userId.HasValue || a.UserId == userId.Value) &&
                       a.CreatedDate >= DateTime.UtcNow.AddDays(-14) &&
                       a.CreatedDate < DateTime.UtcNow.AddDays(-7))
            .CountAsync(cancellationToken);

        if (previous == 0) return recent > 0 ? 70 : 50;

        var ratio = (double)recent / previous;
        if (ratio >= 1.5) return 90;
        if (ratio >= 1.2) return 75;
        if (ratio >= 0.8) return 60;
        if (ratio >= 0.5) return 40;
        return 25;
    }

    #endregion

    #region Private Methods

    private int GetPointsForActivity(string activityType)
    {
        return activityType.ToUpper() switch
        {
            "LOGIN" => 5,
            "TASK_COMPLETE" => 25,
            "EVIDENCE_SUBMIT" => 30,
            "EVIDENCE_APPROVE" => 20,
            "ASSESSMENT_COMPLETE" => 50,
            "RISK_REVIEW" => 20,
            "CONTROL_UPDATE" => 15,
            "COMMENT_ADD" => 5,
            "DOCUMENT_UPLOAD" => 10,
            "REPORT_GENERATE" => 15,
            "COLLABORATION" => 10,
            "TRAINING_COMPLETE" => 40,
            "CERTIFICATION_EARN" => 100,
            _ => 5
        };
    }

    private int CalculateLevel(int totalPoints)
    {
        // Level progression: 100, 250, 500, 1000, 2000, 4000, 8000, etc.
        if (totalPoints < 100) return 1;
        if (totalPoints < 250) return 2;
        if (totalPoints < 500) return 3;
        if (totalPoints < 1000) return 4;
        if (totalPoints < 2000) return 5;
        if (totalPoints < 4000) return 6;
        if (totalPoints < 8000) return 7;
        if (totalPoints < 16000) return 8;
        if (totalPoints < 32000) return 9;
        return 10;
    }

    private async Task UpdateEngagementStateAsync(
        EngagementMetrics metrics,
        CancellationToken cancellationToken)
    {
        // State transitions based on behavior
        var previousState = metrics.CurrentState;

        // Calculate activity frequency
        var recentActivities = await _dbContext.Set<UserActivity>()
            .CountAsync(a =>
                a.UserId == metrics.UserId &&
                a.CreatedDate >= DateTime.UtcNow.AddDays(-7),
                cancellationToken);

        // State determination logic
        if (recentActivities == 0 && metrics.CurrentStreak == 0)
        {
            metrics.CurrentState = EngagementStates.AtRisk;
        }
        else if (recentActivities < 3)
        {
            if (metrics.CurrentState == EngagementStates.Champion)
                metrics.CurrentState = EngagementStates.Contributor;
            else if (metrics.CurrentState == EngagementStates.Contributor)
                metrics.CurrentState = EngagementStates.Explorer;
        }
        else if (recentActivities >= 10 && metrics.CurrentStreak >= 7)
        {
            metrics.CurrentState = EngagementStates.Champion;
        }
        else if (recentActivities >= 5 && metrics.CurrentStreak >= 3)
        {
            metrics.CurrentState = EngagementStates.Contributor;
        }
        else
        {
            metrics.CurrentState = EngagementStates.Explorer;
        }

        if (previousState != metrics.CurrentState)
        {
            _logger.LogInformation(
                "User {UserId} engagement state changed: {PreviousState} -> {CurrentState}",
                metrics.UserId, previousState, metrics.CurrentState);
        }
    }

    private int CalculateEngagementComponent(EngagementMetrics metrics)
    {
        var score = 50; // Base score

        // Recent activity bonus
        if (metrics.LastActivityAt >= DateTime.UtcNow.AddDays(-1))
            score += 20;
        else if (metrics.LastActivityAt >= DateTime.UtcNow.AddDays(-3))
            score += 10;

        // Streak bonus
        if (metrics.CurrentStreak >= 7)
            score += 20;
        else if (metrics.CurrentStreak >= 3)
            score += 10;

        // State bonus
        score += metrics.CurrentState switch
        {
            EngagementStates.Champion => 15,
            EngagementStates.Contributor => 10,
            EngagementStates.Explorer => 5,
            _ => 0
        };

        return Math.Min(100, Math.Max(0, score));
    }

    private int CalculateConsistencyComponent(EngagementMetrics metrics)
    {
        var score = 40; // Base score

        // Streak contribution (max 30 points)
        score += Math.Min(30, metrics.CurrentStreak * 5);

        // Long-term streak history (max 20 points)
        score += Math.Min(20, metrics.LongestStreak * 2);

        // Activity distribution (prefer consistent daily activity)
        if (metrics.WeeklyPoints > 0)
        {
            var avgDailyPoints = metrics.WeeklyPoints / 7.0;
            if (avgDailyPoints >= 20) score += 10;
            else if (avgDailyPoints >= 10) score += 5;
        }

        return Math.Min(100, Math.Max(0, score));
    }

    private async Task<int> CalculateProgressComponentAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var score = 50; // Base score

        // Get recent task completion rate
        var recentTasks = await _dbContext.Set<UserActivity>()
            .Where(a =>
                a.TenantId == tenantId &&
                a.UserId == userId &&
                a.ActivityType == "TASK_COMPLETE" &&
                a.CreatedDate >= DateTime.UtcNow.AddDays(-30))
            .CountAsync(cancellationToken);

        // Task completion bonus
        if (recentTasks >= 20) score += 30;
        else if (recentTasks >= 10) score += 20;
        else if (recentTasks >= 5) score += 10;

        // On-time completion bonus
        var onTimeRate = await GetOnTimeCompletionRateAsync(tenantId, userId, cancellationToken);
        score += (int)(onTimeRate * 20);

        return Math.Min(100, Math.Max(0, score));
    }

    private async Task<int> CalculateQualityComponentAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var score = 60; // Base score

        // Get evidence approval rate
        var evidenceSubmissions = await _dbContext.Set<UserActivity>()
            .CountAsync(a =>
                a.TenantId == tenantId &&
                a.UserId == userId &&
                a.ActivityType == "EVIDENCE_SUBMIT" &&
                a.CreatedDate >= DateTime.UtcNow.AddDays(-90),
                cancellationToken);

        var evidenceApprovals = await _dbContext.Set<UserActivity>()
            .CountAsync(a =>
                a.TenantId == tenantId &&
                a.UserId == userId &&
                a.ActivityType == "EVIDENCE_APPROVE" &&
                a.CreatedDate >= DateTime.UtcNow.AddDays(-90),
                cancellationToken);

        if (evidenceSubmissions > 0)
        {
            var approvalRate = (double)evidenceApprovals / evidenceSubmissions;
            score += (int)(approvalRate * 30);
        }

        // Badge achievement bonus
        var badgeCount = await _dbContext.Set<UserBadge>()
            .CountAsync(ub => ub.TenantId == tenantId && ub.UserId == userId, cancellationToken);

        score += Math.Min(10, badgeCount * 2);

        return Math.Min(100, Math.Max(0, score));
    }

    private async Task<int> CalculateCollaborationComponentAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var score = 50; // Base score

        // Comment/collaboration activities
        var collaborationCount = await _dbContext.Set<UserActivity>()
            .CountAsync(a =>
                a.TenantId == tenantId &&
                a.UserId == userId &&
                (a.ActivityType == "COMMENT_ADD" || a.ActivityType == "COLLABORATION") &&
                a.CreatedDate >= DateTime.UtcNow.AddDays(-30),
                cancellationToken);

        if (collaborationCount >= 20) score += 30;
        else if (collaborationCount >= 10) score += 20;
        else if (collaborationCount >= 5) score += 10;

        // Team interaction bonus (simplified)
        score += 10; // Base team participation

        return Math.Min(100, Math.Max(0, score));
    }

    private async Task<double> GetOnTimeCompletionRateAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        // Simplified - would normally check against task deadlines
        var totalTasks = await _dbContext.Set<UserActivity>()
            .CountAsync(a =>
                a.TenantId == tenantId &&
                a.UserId == userId &&
                a.ActivityType == "TASK_COMPLETE" &&
                a.CreatedDate >= DateTime.UtcNow.AddDays(-90),
                cancellationToken);

        if (totalTasks == 0) return 0.5;

        // Assume 80% on-time rate (would be calculated from actual data)
        return 0.8;
    }

    private List<string> GenerateMotivationRecommendations(
        int engagement, int consistency, int progress, int quality, int collaboration)
    {
        var recommendations = new List<string>();

        if (engagement < 50)
        {
            recommendations.Add("Log in daily to maintain your activity streak");
            recommendations.Add("Complete at least one task per day to boost engagement");
        }

        if (consistency < 50)
        {
            recommendations.Add("Set a daily reminder to check your dashboard");
            recommendations.Add("Aim for a 7-day activity streak to earn bonus points");
        }

        if (progress < 50)
        {
            recommendations.Add("Focus on completing high-priority tasks first");
            recommendations.Add("Break large tasks into smaller, manageable steps");
        }

        if (quality < 50)
        {
            recommendations.Add("Review evidence requirements before submission");
            recommendations.Add("Use the quality checklist for better approval rates");
        }

        if (collaboration < 50)
        {
            recommendations.Add("Comment on team members' work to boost collaboration");
            recommendations.Add("Participate in team discussions and reviews");
        }

        // Add positive reinforcement
        if (engagement >= 80) recommendations.Add("Great engagement! Keep up the excellent work!");
        if (consistency >= 80) recommendations.Add("Your consistency is impressive - you're a role model!");

        return recommendations;
    }

    private async Task<bool> EvaluateBadgeCriteriaAsync(
        Badge badge,
        EngagementMetrics metrics,
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        // Evaluate badge criteria based on badge code
        return badge.Code.ToUpper() switch
        {
            "FIRST_LOGIN" => metrics.TotalActivities >= 1,
            "STREAK_7" => metrics.CurrentStreak >= 7,
            "STREAK_30" => metrics.CurrentStreak >= 30,
            "LEVEL_5" => metrics.Level >= 5,
            "LEVEL_10" => metrics.Level >= 10,
            "POINTS_1000" => metrics.TotalPoints >= 1000,
            "POINTS_10000" => metrics.TotalPoints >= 10000,
            "TASKS_100" => await HasCompletedTasksAsync(tenantId, userId, 100, cancellationToken),
            "CHAMPION" => metrics.CurrentState == EngagementStates.Champion,
            _ => false
        };
    }

    private async Task<bool> HasCompletedTasksAsync(
        Guid tenantId,
        Guid userId,
        int count,
        CancellationToken cancellationToken)
    {
        var taskCount = await _dbContext.Set<UserActivity>()
            .CountAsync(a =>
                a.TenantId == tenantId &&
                a.UserId == userId &&
                a.ActivityType == "TASK_COMPLETE",
                cancellationToken);

        return taskCount >= count;
    }

    #endregion
}

/// <summary>
/// User activity record for tracking engagement history
/// </summary>
public class UserActivity : BaseEntity
{
    public Guid UserId { get; set; }
    public string ActivityType { get; set; } = string.Empty;
    public int PointsEarned { get; set; }
    public string? MetadataJson { get; set; }
}

/// <summary>
/// Badge definition
/// </summary>
public class Badge : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public int PointsValue { get; set; }
    public string Category { get; set; } = "General";
    public string? CriteriaJson { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
}

/// <summary>
/// User badge assignment
/// </summary>
public class UserBadge : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid BadgeId { get; set; }
    public DateTime AwardedAt { get; set; }
    public string? AwardReason { get; set; }

    // Navigation property
    public virtual Badge Badge { get; set; } = null!;
}

/// <summary>
/// Leaderboard result DTO
/// </summary>
public class LeaderboardResult
{
    public string Period { get; set; } = "weekly";
    public List<LeaderboardEntry> Entries { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}

/// <summary>
/// Leaderboard entry DTO
/// </summary>
public class LeaderboardEntry
{
    public int Rank { get; set; }
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public int Points { get; set; }
    public int Level { get; set; }
    public int CurrentStreak { get; set; }
    public string EngagementState { get; set; } = string.Empty;
}
