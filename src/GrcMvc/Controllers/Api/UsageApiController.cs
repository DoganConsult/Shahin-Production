using System;
using System.Threading.Tasks;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Controllers.Api
{
    /// <summary>
    /// API Controller for Usage Tracking and Limits
    /// Provides endpoints for checking usage, limits, and feature availability
    /// </summary>
    [ApiController]
    [Route("api/usage")]
    [Authorize]
    public class UsageApiController : ControllerBase
    {
        private readonly IUsageTrackingService _usageService;
        private readonly ITenantContextService _tenantContext;
        private readonly ILogger<UsageApiController> _logger;

        public UsageApiController(
            IUsageTrackingService usageService,
            ITenantContextService tenantContext,
            ILogger<UsageApiController> logger)
        {
            _usageService = usageService;
            _tenantContext = tenantContext;
            _logger = logger;
        }

        /// <summary>Get current usage summary for the tenant</summary>
        [HttpGet("summary")]
        public async Task<IActionResult> GetUsageSummary()
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            if (tenantId == Guid.Empty)
            {
                return BadRequest(new { error = "No tenant context" });
            }

            var summary = await _usageService.GetUsageSummaryAsync(tenantId);
            return Ok(summary);
        }

        /// <summary>Check if a specific feature is available</summary>
        [HttpGet("feature/{featureName}")]
        public async Task<IActionResult> CheckFeature(string featureName)
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            if (tenantId == Guid.Empty)
            {
                return BadRequest(new { error = "No tenant context" });
            }

            var isAvailable = await _usageService.IsFeatureAvailableAsync(tenantId, featureName);
            return Ok(new { feature = featureName, available = isAvailable });
        }

        /// <summary>Check AI call limit</summary>
        [HttpGet("limits/ai")]
        public async Task<IActionResult> CheckAiLimit()
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            if (tenantId == Guid.Empty)
            {
                return BadRequest(new { error = "No tenant context" });
            }

            var result = await _usageService.CheckAiLimitAsync(tenantId);
            return Ok(result);
        }

        /// <summary>Check storage limit</summary>
        [HttpGet("limits/storage")]
        public async Task<IActionResult> CheckStorageLimit([FromQuery] long additionalBytes = 0)
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            if (tenantId == Guid.Empty)
            {
                return BadRequest(new { error = "No tenant context" });
            }

            var result = await _usageService.CheckStorageLimitAsync(tenantId, additionalBytes);
            return Ok(result);
        }

        /// <summary>Check team member limit</summary>
        [HttpGet("limits/team")]
        public async Task<IActionResult> CheckTeamMemberLimit()
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            if (tenantId == Guid.Empty)
            {
                return BadRequest(new { error = "No tenant context" });
            }

            var result = await _usageService.CheckTeamMemberLimitAsync(tenantId);
            return Ok(result);
        }

        /// <summary>Check framework limit</summary>
        [HttpGet("limits/frameworks")]
        public async Task<IActionResult> CheckFrameworkLimit()
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            if (tenantId == Guid.Empty)
            {
                return BadRequest(new { error = "No tenant context" });
            }

            var result = await _usageService.CheckFrameworkLimitAsync(tenantId);
            return Ok(result);
        }

        /// <summary>Get usage history</summary>
        [HttpGet("history")]
        public async Task<IActionResult> GetUsageHistory(
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            if (tenantId == Guid.Empty)
            {
                return BadRequest(new { error = "No tenant context" });
            }

            var fromDate = from ?? DateTime.UtcNow.AddDays(-30);
            var toDate = to ?? DateTime.UtcNow;

            var history = await _usageService.GetUsageHistoryAsync(tenantId, fromDate, toDate);
            return Ok(history);
        }

        /// <summary>Get current AI usage for today</summary>
        [HttpGet("ai/today")]
        public async Task<IActionResult> GetDailyAiUsage()
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            if (tenantId == Guid.Empty)
            {
                return BadRequest(new { error = "No tenant context" });
            }

            var calls = await _usageService.GetDailyAiCallsAsync(tenantId);
            var limitCheck = await _usageService.CheckAiLimitAsync(tenantId);

            return Ok(new
            {
                callsToday = calls,
                limit = limitCheck.Limit,
                remaining = limitCheck.Remaining,
                percentUsed = limitCheck.Limit > 0 ? (double)calls / limitCheck.Limit * 100 : 0
            });
        }
    }
}
