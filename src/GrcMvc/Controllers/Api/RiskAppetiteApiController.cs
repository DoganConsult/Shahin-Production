using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Models.DTOs;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Controllers.Api
{
    /// <summary>
    /// REST API for Risk Appetite Settings management.
    /// 
    /// ABP Best Practice: RESTful API with proper HTTP verbs, authorization, validation.
    /// This API allows organizations to define acceptable risk levels by category.
    /// </summary>
    [Route("api/risk-appetite")]
    [ApiController]
    [Authorize]
    public class RiskAppetiteApiController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenantContextService _tenantContext;
        private readonly ILogger<RiskAppetiteApiController> _logger;

        public RiskAppetiteApiController(
            IUnitOfWork unitOfWork,
            ITenantContextService tenantContext,
            ILogger<RiskAppetiteApiController> logger)
        {
            _unitOfWork = unitOfWork;
            _tenantContext = tenantContext;
            _logger = logger;
        }

        private Guid GetCurrentTenantId()
        {
            return _tenantContext.GetCurrentTenantId();
        }

        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
        }

        /// <summary>
        /// Get all risk appetite settings for current tenant.
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "Risks.View")]
        public async Task<ActionResult<IEnumerable<RiskAppetiteSettingDto>>> GetAll(
            [FromQuery] bool? activeOnly = null,
            [FromQuery] string? category = null)
        {
            try
            {
                var tenantId = GetCurrentTenantId();
                var allSettings = await _unitOfWork.RiskAppetiteSettings.GetAllAsync();
                var query = allSettings.Where(r => r.TenantId == tenantId);

                if (activeOnly == true)
                {
                    query = query.Where(r => r.IsActive);
                }

                if (!string.IsNullOrEmpty(category))
                {
                    query = query.Where(r => r.Category == category);
                }

                var settings = query
                    .OrderBy(r => r.Category)
                    .ThenBy(r => r.Name)
                    .ToList();

                var dtos = settings.Select(MapToDto).ToList();
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching risk appetite settings");
                return StatusCode(500, new { error = "Failed to retrieve risk appetite settings" });
            }
        }

        /// <summary>
        /// Get a specific risk appetite setting by ID.
        /// </summary>
        [HttpGet("{id:guid}")]
        [Authorize(Policy = "Risks.View")]
        public async Task<ActionResult<RiskAppetiteSettingDto>> GetById(Guid id)
        {
            try
            {
                var tenantId = GetCurrentTenantId();
                var allSettings = await _unitOfWork.RiskAppetiteSettings.GetAllAsync();
                var setting = allSettings.FirstOrDefault(r => r.Id == id && r.TenantId == tenantId);

                if (setting == null)
                {
                    return NotFound(new { error = "Risk appetite setting not found" });
                }

                return Ok(MapToDto(setting));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching risk appetite setting {Id}", id);
                return StatusCode(500, new { error = "Failed to retrieve risk appetite setting" });
            }
        }

        /// <summary>
        /// Get available risk categories for appetite settings.
        /// </summary>
        [HttpGet("categories")]
        [Authorize(Policy = "Risks.View")]
        public ActionResult<IEnumerable<string>> GetCategories()
        {
            var categories = new[]
            {
                "Strategic",
                "Operational",
                "Financial",
                "Compliance",
                "Reputational",
                "Technology",
                "Legal",
                "Market",
                "Credit",
                "Liquidity"
            };

            return Ok(categories);
        }

        /// <summary>
        /// Create a new risk appetite setting.
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "Risks.Manage")]
        public async Task<ActionResult<RiskAppetiteSettingDto>> Create([FromBody] CreateRiskAppetiteSettingDto dto)
        {
            try
            {
                // Validate minimum/maximum relationship
                if (dto.MinimumRiskScore > dto.MaximumRiskScore)
                {
                    return BadRequest(new { error = "Minimum risk score cannot be greater than maximum" });
                }

                if (dto.TargetRiskScore < dto.MinimumRiskScore || dto.TargetRiskScore > dto.MaximumRiskScore)
                {
                    return BadRequest(new { error = "Target risk score must be between minimum and maximum" });
                }

                var tenantId = GetCurrentTenantId();
                var userId = GetCurrentUserId();

                // Check for duplicate category/name
                var existingSettings = await _unitOfWork.RiskAppetiteSettings.GetAllAsync();
                var existing = existingSettings.FirstOrDefault(r => r.TenantId == tenantId && 
                                               r.Category == dto.Category && 
                                               r.Name == dto.Name);

                if (existing != null)
                {
                    return BadRequest(new { error = "A risk appetite setting with this category and name already exists" });
                }

                var setting = new RiskAppetiteSetting
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    Category = dto.Category,
                    Name = dto.Name,
                    Description = dto.Description,
                    MinimumRiskScore = dto.MinimumRiskScore,
                    MaximumRiskScore = dto.MaximumRiskScore,
                    TargetRiskScore = dto.TargetRiskScore,
                    TolerancePercentage = dto.TolerancePercentage,
                    ImpactThreshold = dto.ImpactThreshold,
                    LikelihoodThreshold = dto.LikelihoodThreshold,
                    IsActive = true,
                    ExpiryDate = dto.ExpiryDate,
                    ReviewReminderDays = dto.ReviewReminderDays,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                };

                await _unitOfWork.RiskAppetiteSettings.AddAsync(setting);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Created risk appetite setting {Id} for category {Category}", 
                    setting.Id, setting.Category);

                return CreatedAtAction(nameof(GetById), new { id = setting.Id }, MapToDto(setting));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating risk appetite setting");
                return StatusCode(500, new { error = "Failed to create risk appetite setting" });
            }
        }

        /// <summary>
        /// Update an existing risk appetite setting.
        /// </summary>
        [HttpPut("{id:guid}")]
        [Authorize(Policy = "Risks.Manage")]
        public async Task<ActionResult<RiskAppetiteSettingDto>> Update(Guid id, [FromBody] UpdateRiskAppetiteSettingDto dto)
        {
            try
            {
                // Validate minimum/maximum relationship
                if (dto.MinimumRiskScore > dto.MaximumRiskScore)
                {
                    return BadRequest(new { error = "Minimum risk score cannot be greater than maximum" });
                }

                if (dto.TargetRiskScore < dto.MinimumRiskScore || dto.TargetRiskScore > dto.MaximumRiskScore)
                {
                    return BadRequest(new { error = "Target risk score must be between minimum and maximum" });
                }

                var tenantId = GetCurrentTenantId();
                var userId = GetCurrentUserId();

                var allSettingsForUpdate = await _unitOfWork.RiskAppetiteSettings.GetAllAsync();
                var setting = allSettingsForUpdate.FirstOrDefault(r => r.Id == id && r.TenantId == tenantId);

                if (setting == null)
                {
                    return NotFound(new { error = "Risk appetite setting not found" });
                }

                // Check for duplicate category/name (excluding current)
                var duplicate = allSettingsForUpdate.FirstOrDefault(r => r.TenantId == tenantId && 
                                               r.Category == dto.Category && 
                                               r.Name == dto.Name &&
                                               r.Id != id);

                if (duplicate != null)
                {
                    return BadRequest(new { error = "Another risk appetite setting with this category and name already exists" });
                }

                setting.Category = dto.Category;
                setting.Name = dto.Name;
                setting.Description = dto.Description;
                setting.MinimumRiskScore = dto.MinimumRiskScore;
                setting.MaximumRiskScore = dto.MaximumRiskScore;
                setting.TargetRiskScore = dto.TargetRiskScore;
                setting.TolerancePercentage = dto.TolerancePercentage;
                setting.ImpactThreshold = dto.ImpactThreshold;
                setting.LikelihoodThreshold = dto.LikelihoodThreshold;
                setting.IsActive = dto.IsActive;
                setting.ExpiryDate = dto.ExpiryDate;
                setting.ReviewReminderDays = dto.ReviewReminderDays;
                setting.UpdatedAt = DateTime.UtcNow;
                setting.UpdatedBy = userId;

                await _unitOfWork.RiskAppetiteSettings.UpdateAsync(setting);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Updated risk appetite setting {Id}", id);

                return Ok(MapToDto(setting));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating risk appetite setting {Id}", id);
                return StatusCode(500, new { error = "Failed to update risk appetite setting" });
            }
        }

        /// <summary>
        /// Approve a risk appetite setting.
        /// </summary>
        [HttpPost("{id:guid}/approve")]
        [Authorize(Policy = "Risks.Accept")]
        public async Task<ActionResult<RiskAppetiteSettingDto>> Approve(Guid id)
        {
            try
            {
                var tenantId = GetCurrentTenantId();
                var userId = GetCurrentUserId();

                var allSettingsForApprove = await _unitOfWork.RiskAppetiteSettings.GetAllAsync();
                var setting = allSettingsForApprove.FirstOrDefault(r => r.Id == id && r.TenantId == tenantId);

                if (setting == null)
                {
                    return NotFound(new { error = "Risk appetite setting not found" });
                }

                setting.ApprovedDate = DateTime.UtcNow;
                setting.ApprovedBy = userId;
                setting.IsActive = true;
                setting.UpdatedAt = DateTime.UtcNow;
                setting.UpdatedBy = userId;

                await _unitOfWork.RiskAppetiteSettings.UpdateAsync(setting);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Approved risk appetite setting {Id}", id);

                return Ok(MapToDto(setting));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving risk appetite setting {Id}", id);
                return StatusCode(500, new { error = "Failed to approve risk appetite setting" });
            }
        }

        /// <summary>
        /// Delete a risk appetite setting.
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "Risks.Manage")]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                var tenantId = GetCurrentTenantId();

                var allSettingsForDelete = await _unitOfWork.RiskAppetiteSettings.GetAllAsync();
                var setting = allSettingsForDelete.FirstOrDefault(r => r.Id == id && r.TenantId == tenantId);

                if (setting == null)
                {
                    return NotFound(new { error = "Risk appetite setting not found" });
                }

                await _unitOfWork.RiskAppetiteSettings.DeleteAsync(setting);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Deleted risk appetite setting {Id}", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting risk appetite setting {Id}", id);
                return StatusCode(500, new { error = "Failed to delete risk appetite setting" });
            }
        }

        /// <summary>
        /// Compare all risks against appetite settings.
        /// Returns which risks are within/outside appetite thresholds.
        /// </summary>
        [HttpGet("comparison")]
        [Authorize(Policy = "Risks.View")]
        public async Task<ActionResult<IEnumerable<RiskAppetiteComparisonDto>>> CompareRisks(
            [FromQuery] string? category = null)
        {
            try
            {
                var tenantId = GetCurrentTenantId();

                // Get active appetite settings
                var allAppetiteSettings = await _unitOfWork.RiskAppetiteSettings.GetAllAsync();
                var appetiteQuery = allAppetiteSettings
                    .Where(r => r.TenantId == tenantId && r.IsActive);

                if (!string.IsNullOrEmpty(category))
                {
                    appetiteQuery = appetiteQuery.Where(r => r.Category == category);
                }

                var appetiteSettings = appetiteQuery.ToDictionary(a => a.Category);

                // Get all risks
                var allRisks = await _unitOfWork.Risks.GetAllAsync();
                var risksQuery = allRisks
                    .Where(r => r.TenantId == tenantId);

                if (!string.IsNullOrEmpty(category))
                {
                    risksQuery = risksQuery.Where(r => r.Category == category);
                }

                var risks = risksQuery.ToList();

                var comparisons = new List<RiskAppetiteComparisonDto>();

                foreach (var risk in risks)
                {
                    var riskCategory = risk.Category ?? "Operational";
                    var currentScore = CalculateRiskScore(risk);

                    if (appetiteSettings.TryGetValue(riskCategory, out var appetite))
                    {
                        var status = appetite.GetAppetiteStatus(currentScore);
                        comparisons.Add(new RiskAppetiteComparisonDto
                        {
                            RiskId = risk.Id,
                            RiskTitle = risk.Title ?? "Untitled Risk",
                            Category = riskCategory,
                            CurrentRiskScore = currentScore,
                            TargetRiskScore = appetite.TargetRiskScore,
                            MinimumRiskScore = appetite.MinimumRiskScore,
                            MaximumRiskScore = appetite.MaximumRiskScore,
                            Status = status,
                            StatusDescription = GetStatusDescription(status),
                            DeviationFromTarget = currentScore - appetite.TargetRiskScore,
                            DeviationPercentage = appetite.TargetRiskScore > 0 
                                ? Math.Round((currentScore - appetite.TargetRiskScore) * 100.0 / appetite.TargetRiskScore, 2) 
                                : 0,
                            RequiresAction = status == RiskAppetiteStatus.Exceeded || status == RiskAppetiteStatus.AtTolerance,
                            RecommendedAction = GetRecommendedAction(status)
                        });
                    }
                    else
                    {
                        // No appetite defined for this category
                        comparisons.Add(new RiskAppetiteComparisonDto
                        {
                            RiskId = risk.Id,
                            RiskTitle = risk.Title ?? "Untitled Risk",
                            Category = riskCategory,
                            CurrentRiskScore = currentScore,
                            TargetRiskScore = 50,
                            MinimumRiskScore = 0,
                            MaximumRiskScore = 100,
                            Status = RiskAppetiteStatus.WithinAppetite,
                            StatusDescription = "No appetite defined for category",
                            DeviationFromTarget = currentScore - 50,
                            DeviationPercentage = (currentScore - 50) * 2,
                            RequiresAction = false,
                            RecommendedAction = "Define risk appetite for this category"
                        });
                    }
                }

                return Ok(comparisons.OrderByDescending(c => Math.Abs(c.DeviationFromTarget)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error comparing risks to appetite");
                return StatusCode(500, new { error = "Failed to compare risks to appetite settings" });
            }
        }

        /// <summary>
        /// Get summary of risk appetite compliance across all categories.
        /// </summary>
        [HttpGet("summary")]
        [Authorize(Policy = "Risks.View")]
        public async Task<ActionResult<RiskAppetiteSummaryDto>> GetSummary()
        {
            try
            {
                var tenantId = GetCurrentTenantId();

                var allSettings = await _unitOfWork.RiskAppetiteSettings.GetAllAsync();
                var appetiteSettings = allSettings
                    .Where(r => r.TenantId == tenantId && r.IsActive)
                    .ToDictionary(a => a.Category);

                var allRisksForSummary = await _unitOfWork.Risks.GetAllAsync();
                var risks = allRisksForSummary
                    .Where(r => r.TenantId == tenantId)
                    .ToList();

                var summary = new RiskAppetiteSummaryDto
                {
                    TenantId = tenantId,
                    TotalRisks = risks.Count()
                };

                var categoryStats = new Dictionary<string, (int total, int within, int exceeding, double sumScore)>();

                foreach (var risk in risks)
                {
                    var category = risk.Category ?? "Operational";
                    var score = CalculateRiskScore(risk);

                    if (!categoryStats.ContainsKey(category))
                    {
                        categoryStats[category] = (0, 0, 0, 0);
                    }

                    var stats = categoryStats[category];
                    stats.total++;
                    stats.sumScore += score;

                    if (appetiteSettings.TryGetValue(category, out var appetite))
                    {
                        var status = appetite.GetAppetiteStatus(score);
                        if (status == RiskAppetiteStatus.Exceeded)
                        {
                            summary.RisksExceedingAppetite++;
                            stats.exceeding++;
                        }
                        else if (status == RiskAppetiteStatus.AtTolerance)
                        {
                            summary.RisksAtTolerance++;
                            stats.within++;
                        }
                        else if (status == RiskAppetiteStatus.UnderControlled)
                        {
                            summary.RisksUnderControlled++;
                            stats.within++;
                        }
                        else
                        {
                            summary.RisksWithinAppetite++;
                            stats.within++;
                        }
                    }
                    else
                    {
                        summary.RisksWithinAppetite++;
                        stats.within++;
                    }

                    categoryStats[category] = stats;
                }

                // Build category summaries
                foreach (var (category, stats) in categoryStats)
                {
                    var targetScore = appetiteSettings.TryGetValue(category, out var appetite) 
                        ? appetite.TargetRiskScore 
                        : 50;

                    summary.ByCategory.Add(new CategoryAppetiteSummaryDto
                    {
                        Category = category,
                        TotalRisks = stats.total,
                        WithinAppetite = stats.within,
                        ExceedingAppetite = stats.exceeding,
                        AverageRiskScore = stats.total > 0 ? Math.Round(stats.sumScore / stats.total, 2) : 0,
                        TargetRiskScore = targetScore,
                        CompliancePercentage = stats.total > 0 
                            ? Math.Round(stats.within * 100.0 / stats.total, 2) 
                            : 100
                    });
                }

                summary.OverallCompliancePercentage = summary.TotalRisks > 0
                    ? Math.Round((summary.RisksWithinAppetite + summary.RisksAtTolerance) * 100.0 / summary.TotalRisks, 2)
                    : 100;

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting risk appetite summary");
                return StatusCode(500, new { error = "Failed to get risk appetite summary" });
            }
        }

        #region Private Helpers

        private static int CalculateRiskScore(Risk risk)
        {
            // Calculate inherent risk score based on impact and likelihood
            var impactValue = (int)risk.Impact + 1;
            var likelihoodValue = (int)risk.Likelihood + 1;
            
            // Simple matrix calculation (can be customized)
            var rawScore = impactValue * likelihoodValue;
            
            // Normalize to 0-100 scale (5x5 matrix = max 25)
            return (int)Math.Round(rawScore * 100.0 / 25);
        }

        private static string GetStatusDescription(RiskAppetiteStatus status)
        {
            return status switch
            {
                RiskAppetiteStatus.UnderControlled => "Under-controlled (below minimum threshold)",
                RiskAppetiteStatus.OnTarget => "On target",
                RiskAppetiteStatus.WithinAppetite => "Within acceptable range",
                RiskAppetiteStatus.AtTolerance => "Approaching tolerance limits",
                RiskAppetiteStatus.Exceeded => "Exceeds maximum acceptable level",
                _ => "Unknown status"
            };
        }

        private static string? GetRecommendedAction(RiskAppetiteStatus status)
        {
            return status switch
            {
                RiskAppetiteStatus.UnderControlled => "Review if controls are cost-effective; consider reducing controls",
                RiskAppetiteStatus.AtTolerance => "Monitor closely; prepare mitigation plan",
                RiskAppetiteStatus.Exceeded => "Immediate action required; implement additional controls",
                _ => null
            };
        }

        private static RiskAppetiteSettingDto MapToDto(RiskAppetiteSetting setting)
        {
            var now = DateTime.UtcNow;
            int? daysUntilExpiry = null;
            bool isExpiringSoon = false;

            if (setting.ExpiryDate.HasValue)
            {
                daysUntilExpiry = (int)(setting.ExpiryDate.Value - now).TotalDays;
                isExpiringSoon = daysUntilExpiry <= setting.ReviewReminderDays;
            }

            return new RiskAppetiteSettingDto
            {
                Id = setting.Id,
                TenantId = setting.TenantId,
                Category = setting.Category,
                Name = setting.Name,
                Description = setting.Description,
                MinimumRiskScore = setting.MinimumRiskScore,
                MaximumRiskScore = setting.MaximumRiskScore,
                TargetRiskScore = setting.TargetRiskScore,
                TolerancePercentage = setting.TolerancePercentage,
                ImpactThreshold = setting.ImpactThreshold,
                LikelihoodThreshold = setting.LikelihoodThreshold,
                IsActive = setting.IsActive,
                ApprovedDate = setting.ApprovedDate,
                ApprovedBy = setting.ApprovedBy,
                ExpiryDate = setting.ExpiryDate,
                ReviewReminderDays = setting.ReviewReminderDays,
                CreatedAt = setting.CreatedAt,
                CreatedBy = setting.CreatedBy,
                UpdatedAt = setting.UpdatedAt,
                UpdatedBy = setting.UpdatedBy,
                IsExpiringSoon = isExpiringSoon,
                DaysUntilExpiry = daysUntilExpiry
            };
        }

        #endregion
    }
}
