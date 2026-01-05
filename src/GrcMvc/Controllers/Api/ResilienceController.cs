using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Models.Dtos;
using GrcMvc.Services.Interfaces;

namespace GrcMvc.Controllers.Api
{
    /// <summary>
    /// API Controller for Resilience Assessments
    /// Supports Operational Resilience and Risk Resilience assessments
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ResilienceController : ControllerBase
    {
        private readonly IResilienceService _resilienceService;
        private readonly ILogger<ResilienceController> _logger;
        private readonly GrcDbContext _context;

        public ResilienceController(
            IResilienceService resilienceService,
            ILogger<ResilienceController> logger,
            GrcDbContext context)
        {
            _resilienceService = resilienceService;
            _logger = logger;
            _context = context;
        }

        // ============ Operational Resilience Endpoints ============

        /// <summary>
        /// POST /api/resilience - Create new operational resilience assessment
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateResilience([FromBody] CreateResilienceDto input)
        {
            try
            {
                var tenantId = GetTenantId();
                var resilience = await _resilienceService.CreateResilienceAsync(tenantId, input);

                return CreatedAtAction(nameof(GetResilience), new { id = resilience.Id }, new
                {
                    success = true,
                    message = "Resilience assessment created successfully",
                    data = MapToDto(resilience)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating resilience assessment");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/resilience/{id} - Get resilience assessment details
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetResilience(Guid id)
        {
            try
            {
                var tenantId = GetTenantId();
                var resilience = await _resilienceService.GetResilienceAsync(tenantId, id);

                return Ok(new { success = true, data = resilience });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting resilience assessment {Id}", id);
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/resilience - Get all resilience assessments
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetResiliences([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var tenantId = GetTenantId();
                var resiliences = await _resilienceService.GetResiliencesAsync(tenantId, page, pageSize);
                var total = await _context.Resiliences.CountAsync(r => r.TenantId == tenantId && !r.IsDeleted);

                return Ok(new
                {
                    success = true,
                    data = resiliences,
                    pagination = new { page, pageSize, total, pages = (int)Math.Ceiling((double)total / pageSize) }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting resilience assessments");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// PUT /api/resilience/{id} - Update resilience assessment
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateResilience(Guid id, [FromBody] UpdateResilienceDto input)
        {
            try
            {
                var tenantId = GetTenantId();
                var resilience = await _resilienceService.UpdateResilienceAsync(tenantId, id, input);

                return Ok(new
                {
                    success = true,
                    message = "Resilience assessment updated successfully",
                    data = MapToDto(resilience)
                });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating resilience assessment {Id}", id);
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// DELETE /api/resilience/{id} - Delete resilience assessment
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteResilience(Guid id)
        {
            try
            {
                var tenantId = GetTenantId();
                var success = await _resilienceService.DeleteResilienceAsync(tenantId, id);

                if (!success)
                    return NotFound(new { success = false, error = "Resilience assessment not found" });

                return Ok(new { success = true, message = "Resilience assessment deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting resilience assessment {Id}", id);
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// POST /api/resilience/{id}/assess - Start resilience assessment (from workflow)
        /// </summary>
        [HttpPost("{id}/assess")]
        public async Task<IActionResult> AssessResilience(Guid id, [FromBody] ResilienceAssessmentRequestDto? request = null)
        {
            try
            {
                var tenantId = GetTenantId();
                var resilience = await _resilienceService.AssessResilienceAsync(tenantId, id, request);

                return Ok(new
                {
                    success = true,
                    message = "Resilience assessment started successfully",
                    data = MapToDto(resilience)
                });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting resilience assessment {Id}", id);
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        // ============ Risk Resilience Endpoints ============

        /// <summary>
        /// POST /api/resilience/risk - Create new risk resilience assessment
        /// </summary>
        [HttpPost("risk")]
        public async Task<IActionResult> CreateRiskResilience([FromBody] CreateRiskResilienceDto input)
        {
            try
            {
                var tenantId = GetTenantId();
                var riskResilience = await _resilienceService.CreateRiskResilienceAsync(tenantId, input);

                return CreatedAtAction(nameof(GetRiskResilience), new { id = riskResilience.Id }, new
                {
                    success = true,
                    message = "Risk resilience assessment created successfully",
                    data = MapRiskResilienceToDto(riskResilience)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating risk resilience assessment");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/resilience/risk/{id} - Get risk resilience assessment details
        /// </summary>
        [HttpGet("risk/{id}")]
        public async Task<IActionResult> GetRiskResilience(Guid id)
        {
            try
            {
                var tenantId = GetTenantId();
                var riskResilience = await _resilienceService.GetRiskResilienceAsync(tenantId, id);

                return Ok(new { success = true, data = riskResilience });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting risk resilience assessment {Id}", id);
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/resilience/risk - Get all risk resilience assessments
        /// </summary>
        [HttpGet("risk")]
        public async Task<IActionResult> GetRiskResiliences([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var tenantId = GetTenantId();
                var riskResiliences = await _resilienceService.GetRiskResiliencesAsync(tenantId, page, pageSize);
                var total = await _context.RiskResiliences.CountAsync(r => r.TenantId == tenantId && !r.IsDeleted);

                return Ok(new
                {
                    success = true,
                    data = riskResiliences,
                    pagination = new { page, pageSize, total, pages = (int)Math.Ceiling((double)total / pageSize) }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting risk resilience assessments");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// POST /api/resilience/risk/{id}/assess - Start risk resilience assessment
        /// </summary>
        [HttpPost("risk/{id}/assess")]
        public async Task<IActionResult> AssessRiskResilience(Guid id)
        {
            try
            {
                var tenantId = GetTenantId();
                var riskResilience = await _resilienceService.AssessRiskResilienceAsync(tenantId, id);

                return Ok(new
                {
                    success = true,
                    message = "Risk resilience assessment started successfully",
                    data = MapRiskResilienceToDto(riskResilience)
                });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting risk resilience assessment {Id}", id);
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        // ============ Helper Methods ============

        private Guid GetTenantId()
        {
            var tenantId = User.FindFirst("tenant_id")?.Value;
            return Guid.TryParse(tenantId, out var id) ? id : Guid.Empty;
        }

        private ResilienceDto MapToDto(Models.Entities.Resilience resilience)
        {
            return new ResilienceDto
            {
                Id = resilience.Id,
                AssessmentNumber = resilience.AssessmentNumber,
                Name = resilience.Name,
                Description = resilience.Description,
                AssessmentType = resilience.AssessmentType,
                Framework = resilience.Framework,
                Scope = resilience.Scope,
                Status = resilience.Status,
                AssessmentDate = resilience.AssessmentDate,
                DueDate = resilience.DueDate,
                CompletedDate = resilience.CompletedDate,
                AssessedByUserId = resilience.AssessedByUserId,
                AssessedByUserName = resilience.AssessedByUserName,
                ResilienceScore = resilience.ResilienceScore,
                BusinessContinuityScore = resilience.BusinessContinuityScore,
                DisasterRecoveryScore = resilience.DisasterRecoveryScore,
                CyberResilienceScore = resilience.CyberResilienceScore,
                OverallRating = resilience.OverallRating,
                RelatedAssessmentId = resilience.RelatedAssessmentId,
                RelatedRiskId = resilience.RelatedRiskId,
                RelatedWorkflowInstanceId = resilience.RelatedWorkflowInstanceId
            };
        }

        private RiskResilienceDto MapRiskResilienceToDto(Models.Entities.RiskResilience riskResilience)
        {
            return new RiskResilienceDto
            {
                Id = riskResilience.Id,
                AssessmentNumber = riskResilience.AssessmentNumber,
                Name = riskResilience.Name,
                Description = riskResilience.Description,
                RiskCategory = riskResilience.RiskCategory,
                RiskType = riskResilience.RiskType,
                RelatedRiskId = riskResilience.RelatedRiskId,
                RiskToleranceLevel = riskResilience.RiskToleranceLevel,
                RecoveryCapabilityScore = riskResilience.RecoveryCapabilityScore,
                ImpactMitigationScore = riskResilience.ImpactMitigationScore,
                ResilienceRating = riskResilience.ResilienceRating,
                Status = riskResilience.Status,
                AssessmentDate = riskResilience.AssessmentDate,
                DueDate = riskResilience.DueDate,
                CompletedDate = riskResilience.CompletedDate,
                AssessedByUserId = riskResilience.AssessedByUserId,
                AssessedByUserName = riskResilience.AssessedByUserName
            };
        }
    }
}
