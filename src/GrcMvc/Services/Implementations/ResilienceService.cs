using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Models.DTOs;
using GrcMvc.Services.Interfaces;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Service implementation for Resilience assessments
    /// </summary>
    public class ResilienceService : IResilienceService
    {
        private readonly GrcDbContext _context;
        private readonly ILogger<ResilienceService> _logger;

        public ResilienceService(
            GrcDbContext context,
            ILogger<ResilienceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ============ Operational Resilience ============

        public async Task<Resilience> CreateResilienceAsync(Guid tenantId, CreateResilienceDto input)
        {
            var resilience = new Resilience
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                AssessmentNumber = GenerateAssessmentNumber(tenantId),
                Name = input.Name,
                Description = input.Description,
                AssessmentType = input.AssessmentType,
                Framework = input.Framework,
                Scope = input.Scope,
                Status = "Draft",
                DueDate = input.DueDate,
                AssessedByUserId = input.AssessedByUserId,
                RelatedAssessmentId = input.RelatedAssessmentId,
                RelatedRiskId = input.RelatedRiskId,
                AssessmentDate = DateTime.UtcNow
            };

            _context.Resiliences.Add(resilience);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created resilience assessment {Id} for tenant {TenantId}", resilience.Id, tenantId);
            return resilience;
        }

        public async Task<Resilience> UpdateResilienceAsync(Guid tenantId, Guid id, UpdateResilienceDto input)
        {
            var resilience = await _context.Resiliences
                .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantId && !r.IsDeleted);

            if (resilience == null)
                throw new InvalidOperationException($"Resilience assessment {id} not found");

            resilience.Name = input.Name;
            resilience.Description = input.Description;
            resilience.Status = input.Status;
            resilience.ResilienceScore = input.ResilienceScore;
            resilience.BusinessContinuityScore = input.BusinessContinuityScore;
            resilience.DisasterRecoveryScore = input.DisasterRecoveryScore;
            resilience.CyberResilienceScore = input.CyberResilienceScore;
            resilience.OverallRating = input.OverallRating;
            resilience.AssessmentDetails = input.AssessmentDetails;
            resilience.Findings = input.Findings;
            resilience.ActionItems = input.ActionItems;
            resilience.Notes = input.Notes;

            if (input.Status == "Completed" && resilience.CompletedDate == null)
                resilience.CompletedDate = DateTime.UtcNow;

            _context.Resiliences.Update(resilience);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated resilience assessment {Id}", id);
            return resilience;
        }

        public async Task<ResilienceDto> GetResilienceAsync(Guid tenantId, Guid id)
        {
            var resilience = await _context.Resiliences
                .AsNoTracking()
                .Where(r => r.Id == id && r.TenantId == tenantId && !r.IsDeleted)
                .Select(r => new ResilienceDto
                {
                    Id = r.Id,
                    AssessmentNumber = r.AssessmentNumber,
                    Name = r.Name,
                    Description = r.Description,
                    AssessmentType = r.AssessmentType,
                    Framework = r.Framework,
                    Scope = r.Scope,
                    Status = r.Status,
                    AssessmentDate = r.AssessmentDate,
                    DueDate = r.DueDate,
                    CompletedDate = r.CompletedDate,
                    AssessedByUserId = r.AssessedByUserId,
                    AssessedByUserName = r.AssessedByUserName,
                    ResilienceScore = r.ResilienceScore,
                    BusinessContinuityScore = r.BusinessContinuityScore,
                    DisasterRecoveryScore = r.DisasterRecoveryScore,
                    CyberResilienceScore = r.CyberResilienceScore,
                    OverallRating = r.OverallRating,
                    RelatedAssessmentId = r.RelatedAssessmentId,
                    RelatedRiskId = r.RelatedRiskId,
                    RelatedWorkflowInstanceId = r.RelatedWorkflowInstanceId
                })
                .FirstOrDefaultAsync();

            if (resilience == null)
                throw new InvalidOperationException($"Resilience assessment {id} not found");

            return resilience;
        }

        public async Task<List<ResilienceDto>> GetResiliencesAsync(Guid tenantId, int page = 1, int pageSize = 20)
        {
            return await _context.Resiliences
                .AsNoTracking()
                .Where(r => r.TenantId == tenantId && !r.IsDeleted)
                .OrderByDescending(r => r.AssessmentDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new ResilienceDto
                {
                    Id = r.Id,
                    AssessmentNumber = r.AssessmentNumber,
                    Name = r.Name,
                    Description = r.Description,
                    AssessmentType = r.AssessmentType,
                    Framework = r.Framework,
                    Scope = r.Scope,
                    Status = r.Status,
                    AssessmentDate = r.AssessmentDate,
                    DueDate = r.DueDate,
                    CompletedDate = r.CompletedDate,
                    AssessedByUserId = r.AssessedByUserId,
                    AssessedByUserName = r.AssessedByUserName,
                    ResilienceScore = r.ResilienceScore,
                    BusinessContinuityScore = r.BusinessContinuityScore,
                    DisasterRecoveryScore = r.DisasterRecoveryScore,
                    CyberResilienceScore = r.CyberResilienceScore,
                    OverallRating = r.OverallRating,
                    RelatedAssessmentId = r.RelatedAssessmentId,
                    RelatedRiskId = r.RelatedRiskId,
                    RelatedWorkflowInstanceId = r.RelatedWorkflowInstanceId
                })
                .ToListAsync();
        }

        public async Task<bool> DeleteResilienceAsync(Guid tenantId, Guid id)
        {
            var resilience = await _context.Resiliences
                .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantId && !r.IsDeleted);

            if (resilience == null)
                return false;

            resilience.IsDeleted = true;
            _context.Resiliences.Update(resilience);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted resilience assessment {Id}", id);
            return true;
        }

        public async Task<Resilience> AssessResilienceAsync(Guid tenantId, Guid id, ResilienceAssessmentRequestDto? request = null)
        {
            var resilience = await _context.Resiliences
                .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantId && !r.IsDeleted);

            if (resilience == null)
                throw new InvalidOperationException($"Resilience assessment {id} not found");

            // Update status to InProgress
            resilience.Status = "InProgress";

            // Link to workflow if provided
            if (request?.RelatedWorkflowInstanceId != null)
                resilience.RelatedWorkflowInstanceId = request.RelatedWorkflowInstanceId;

            if (request?.RelatedAssessmentId != null)
                resilience.RelatedAssessmentId = request.RelatedAssessmentId;

            if (request?.RelatedRiskId != null)
                resilience.RelatedRiskId = request.RelatedRiskId;

            _context.Resiliences.Update(resilience);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Started resilience assessment {Id}", id);
            return resilience;
        }

        // ============ Risk Resilience ============

        public async Task<RiskResilience> CreateRiskResilienceAsync(Guid tenantId, CreateRiskResilienceDto input)
        {
            var riskResilience = new RiskResilience
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                AssessmentNumber = GenerateRiskResilienceNumber(tenantId),
                Name = input.Name,
                Description = input.Description,
                RiskCategory = input.RiskCategory,
                RiskType = input.RiskType,
                RelatedRiskId = input.RelatedRiskId,
                Status = "Draft",
                DueDate = input.DueDate,
                AssessedByUserId = input.AssessedByUserId,
                AssessmentDate = DateTime.UtcNow
            };

            _context.RiskResiliences.Add(riskResilience);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created risk resilience assessment {Id} for tenant {TenantId}", riskResilience.Id, tenantId);
            return riskResilience;
        }

        public async Task<RiskResilienceDto> GetRiskResilienceAsync(Guid tenantId, Guid id)
        {
            var riskResilience = await _context.RiskResiliences
                .AsNoTracking()
                .Where(r => r.Id == id && r.TenantId == tenantId && !r.IsDeleted)
                .Select(r => new RiskResilienceDto
                {
                    Id = r.Id,
                    AssessmentNumber = r.AssessmentNumber,
                    Name = r.Name,
                    Description = r.Description,
                    RiskCategory = r.RiskCategory,
                    RiskType = r.RiskType,
                    RelatedRiskId = r.RelatedRiskId,
                    RiskToleranceLevel = r.RiskToleranceLevel,
                    RecoveryCapabilityScore = r.RecoveryCapabilityScore,
                    ImpactMitigationScore = r.ImpactMitigationScore,
                    ResilienceRating = r.ResilienceRating,
                    Status = r.Status,
                    AssessmentDate = r.AssessmentDate,
                    DueDate = r.DueDate,
                    CompletedDate = r.CompletedDate,
                    AssessedByUserId = r.AssessedByUserId,
                    AssessedByUserName = r.AssessedByUserName
                })
                .FirstOrDefaultAsync();

            if (riskResilience == null)
                throw new InvalidOperationException($"Risk resilience assessment {id} not found");

            return riskResilience;
        }

        public async Task<List<RiskResilienceDto>> GetRiskResiliencesAsync(Guid tenantId, int page = 1, int pageSize = 20)
        {
            return await _context.RiskResiliences
                .AsNoTracking()
                .Where(r => r.TenantId == tenantId && !r.IsDeleted)
                .OrderByDescending(r => r.AssessmentDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new RiskResilienceDto
                {
                    Id = r.Id,
                    AssessmentNumber = r.AssessmentNumber,
                    Name = r.Name,
                    Description = r.Description,
                    RiskCategory = r.RiskCategory,
                    RiskType = r.RiskType,
                    RelatedRiskId = r.RelatedRiskId,
                    RiskToleranceLevel = r.RiskToleranceLevel,
                    RecoveryCapabilityScore = r.RecoveryCapabilityScore,
                    ImpactMitigationScore = r.ImpactMitigationScore,
                    ResilienceRating = r.ResilienceRating,
                    Status = r.Status,
                    AssessmentDate = r.AssessmentDate,
                    DueDate = r.DueDate,
                    CompletedDate = r.CompletedDate,
                    AssessedByUserId = r.AssessedByUserId,
                    AssessedByUserName = r.AssessedByUserName
                })
                .ToListAsync();
        }

        public async Task<RiskResilience> AssessRiskResilienceAsync(Guid tenantId, Guid id)
        {
            var riskResilience = await _context.RiskResiliences
                .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantId && !r.IsDeleted);

            if (riskResilience == null)
                throw new InvalidOperationException($"Risk resilience assessment {id} not found");

            riskResilience.Status = "InProgress";
            _context.RiskResiliences.Update(riskResilience);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Started risk resilience assessment {Id}", id);
            return riskResilience;
        }

        // ============ Helper Methods ============

        private string GenerateAssessmentNumber(Guid tenantId)
        {
            var year = DateTime.UtcNow.Year;
            var count = _context.Resiliences.Count(r => r.TenantId == tenantId && r.AssessmentDate.HasValue && r.AssessmentDate.Value.Year == year);
            return $"RES-{year}-{(count + 1):D4}";
        }

        private string GenerateRiskResilienceNumber(Guid tenantId)
        {
            var year = DateTime.UtcNow.Year;
            var count = _context.RiskResiliences.Count(r => r.TenantId == tenantId && r.AssessmentDate.HasValue && r.AssessmentDate.Value.Year == year);
            return $"RISK-RES-{year}-{(count + 1):D4}";
        }
    }
}
