using System;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Services.Implementations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

using Microsoft.Extensions.Localization;
using GrcMvc.Resources;
namespace GrcMvc.Controllers.Api
{
    /// <summary>
    /// System API - Cache statistics, policy decisions, health monitoring
    /// </summary>
    [Route("api/system")]
    [ApiController]
    [Authorize]
    public class SystemApiController : ControllerBase
    {
        private readonly GrcDbContext _context;
        private readonly IGrcCachingService? _cachingService;
        private readonly ILogger<SystemApiController> _logger;

        public SystemApiController(
            GrcDbContext context,
            ILogger<SystemApiController> logger,
            IGrcCachingService? cachingService = null)
        {
            _context = context;
            _logger = logger;
            _cachingService = cachingService;
        }

        /// <summary>
        /// Get cache statistics
        /// GET /api/system/cache/stats
        /// </summary>
        [HttpGet("cache/stats")]
        public IActionResult GetCacheStatistics()
        {
            if (_cachingService == null)
            {
                return Ok(new
                {
                    enabled = false,
                    message = "Caching service not configured"
                });
            }

            var stats = _cachingService.GetStatistics();
            return Ok(new
            {
                enabled = true,
                stats.CacheHits,
                stats.CacheMisses,
                stats.ItemsCached,
                stats.Invalidations,
                hitRatio = $"{stats.HitRatio:F1}%",
                message = "Cache is operational"
            });
        }

        /// <summary>
        /// Invalidate tenant cache
        /// POST /api/system/cache/invalidate/{tenantId}
        /// </summary>
        [HttpPost("cache/invalidate/{tenantId:guid}")]
        [Authorize(Roles = "Admin,PlatformAdmin")]
        public async Task<IActionResult> InvalidateTenantCache(Guid tenantId, [FromQuery] string? dataType = null)
        {
            if (_cachingService == null)
                return BadRequest(new { error = "Caching service not configured" });

            await _cachingService.InvalidateTenantCacheAsync(tenantId, dataType);

            return Ok(new
            {
                message = string.IsNullOrEmpty(dataType)
                    ? $"All cache invalidated for tenant {tenantId}"
                    : $"Cache '{dataType}' invalidated for tenant {tenantId}"
            });
        }

        /// <summary>
        /// Get policy decision history for a tenant
        /// GET /api/system/policy-decisions/{tenantId}
        /// </summary>
        [HttpGet("policy-decisions/{tenantId:guid}")]
        public async Task<IActionResult> GetPolicyDecisionHistory(
            Guid tenantId,
            [FromQuery] string? policyType = null,
            [FromQuery] int limit = 50)
        {
            try
            {
                var query = _context.PolicyDecisions
                    .Where(p => p.TenantId == tenantId);

                if (!string.IsNullOrEmpty(policyType))
                    query = query.Where(p => p.PolicyType == policyType);

                var decisions = await query
                    .OrderByDescending(p => p.EvaluatedAt)
                    .Take(limit)
                    .Select(p => new
                    {
                        p.Id,
                        p.PolicyType,
                        p.PolicyVersion,
                        p.Decision,
                        p.Reason,
                        p.RulesEvaluated,
                        p.RulesMatched,
                        p.ConfidenceScore,
                        p.EvaluatedAt,
                        p.IsCached,
                        p.RelatedEntityType,
                        p.RelatedEntityId
                    })
                    .ToListAsync();

                return Ok(new
                {
                    tenantId,
                    total = decisions.Count,
                    decisions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting policy decisions for tenant {TenantId}", tenantId);
                return StatusCode(500, new { error = "Internal error" });
            }
        }

        /// <summary>
        /// Get policy decision summary (analytics)
        /// GET /api/system/policy-decisions/{tenantId}/summary
        /// </summary>
        [HttpGet("policy-decisions/{tenantId:guid}/summary")]
        public async Task<IActionResult> GetPolicyDecisionSummary(Guid tenantId)
        {
            try
            {
                var summary = await _context.PolicyDecisions
                    .Where(p => p.TenantId == tenantId)
                    .GroupBy(p => p.PolicyType)
                    .Select(g => new
                    {
                        PolicyType = g.Key,
                        TotalDecisions = g.Count(),
                        CachedDecisions = g.Count(p => p.IsCached),
                        AvgConfidence = g.Average(p => p.ConfidenceScore),
                        LastEvaluated = g.Max(p => p.EvaluatedAt)
                    })
                    .ToListAsync();

                var totalDecisions = await _context.PolicyDecisions
                    .CountAsync(p => p.TenantId == tenantId);

                return Ok(new
                {
                    tenantId,
                    totalDecisions,
                    byPolicyType = summary
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting policy decision summary for tenant {TenantId}", tenantId);
                return StatusCode(500, new { error = "Internal error" });
            }
        }

        /// <summary>
        /// Get serial number statistics
        /// GET /api/system/serial-numbers/{tenantId}
        /// </summary>
        [HttpGet("serial-numbers/{tenantId:guid}")]
        public async Task<IActionResult> GetSerialNumberStats(Guid tenantId)
        {
            try
            {
                var stats = await _context.SerialNumberCounters
                    .Where(s => s.TenantId == tenantId)
                    .GroupBy(s => s.EntityType)
                    .Select(g => new
                    {
                        EntityType = g.Key,
                        TotalGenerated = g.Sum(s => s.LastSequence),
                        LastDate = g.Max(s => s.DateKey)
                    })
                    .ToListAsync();

                return Ok(new
                {
                    tenantId,
                    entityTypes = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting serial number stats for tenant {TenantId}", tenantId);
                return StatusCode(500, new { error = "Internal error" });
            }
        }

        /// <summary>
        /// Get audit events for a tenant
        /// GET /api/system/audit-events/{tenantId}
        /// </summary>
        [HttpGet("audit-events/{tenantId:guid}")]
        public async Task<IActionResult> GetAuditEvents(
            Guid tenantId,
            [FromQuery] string? eventType = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] int limit = 100)
        {
            try
            {
                var query = _context.AuditEvents
                    .Where(e => e.TenantId == tenantId);

                if (!string.IsNullOrEmpty(eventType))
                    query = query.Where(e => e.EventType == eventType);

                if (fromDate.HasValue)
                    query = query.Where(e => e.EventTimestamp >= fromDate.Value);

                var events = await query
                    .OrderByDescending(e => e.EventTimestamp)
                    .Take(limit)
                    .Select(e => new
                    {
                        e.Id,
                        e.EventId,
                        e.EventType,
                        e.Action,
                        e.AffectedEntityType,
                        e.AffectedEntityId,
                        e.Actor,
                        e.Status,
                        e.EventTimestamp,
                        e.CorrelationId
                    })
                    .ToListAsync();

                return Ok(new
                {
                    tenantId,
                    total = events.Count,
                    events
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting audit events for tenant {TenantId}", tenantId);
                return StatusCode(500, new { error = "Internal error" });
            }
        }

        /// <summary>
        /// Health check with detailed status
        /// GET /api/system/health
        /// </summary>
        [HttpGet("health")]
        [AllowAnonymous]
        public async Task<IActionResult> GetHealthStatus()
        {
            try
            {
                // Check database connectivity
                var dbHealthy = await _context.Database.CanConnectAsync();

                // Get counts
                var tenantCount = await _context.Tenants.CountAsync();
                var activeWorkflows = await _context.WorkflowInstances
                    .CountAsync(w => w.Status == "Active");

                return Ok(new
                {
                    status = dbHealthy ? "healthy" : "degraded",
                    timestamp = DateTime.UtcNow,
                    database = dbHealthy ? "connected" : "disconnected",
                    caching = _cachingService != null ? "enabled" : "disabled",
                    metrics = new
                    {
                        tenants = tenantCount,
                        activeWorkflows
                    }
                });
            }
            catch (NpgsqlException npgEx)
            {
                _logger.LogError(npgEx, "Database connection failed: {ErrorCode}", npgEx.SqlState);
                return StatusCode(503, new
                {
                    status = "unhealthy",
                    error = "Database connection failed",
                    errorCode = npgEx.SqlState, // 28P01 for auth failure
                    message = npgEx.Message,
                    database = "disconnected"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return StatusCode(503, new
                {
                    status = "unhealthy",
                    error = "An error occurred.",
                    errorType = ex.GetType().Name
                });
            }
        }
    }
}
