using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GrcMvc.Data;
using GrcMvc.Exceptions;
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
                throw new EntityNotFoundException("ResilienceAssessment", id);

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
                throw new EntityNotFoundException("ResilienceAssessment", id);

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
                throw new EntityNotFoundException("ResilienceAssessment", id);

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
                throw new EntityNotFoundException("RiskResilienceAssessment", id);

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
                throw new EntityNotFoundException("RiskResilienceAssessment", id);

            riskResilience.Status = "InProgress";
            _context.RiskResiliences.Update(riskResilience);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Started risk resilience assessment {Id}", id);
            return riskResilience;
        }

        // ============ Incident Response ============

        public async Task<IncidentDto> CreateIncidentAsync(Guid tenantId, CreateIncidentDto input)
        {
            var incident = new IncidentDto
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                IncidentCode = GenerateIncidentNumber(tenantId),
                Title = input.Title,
                TitleAr = input.TitleAr,
                Description = input.Description,
                Severity = input.Severity,
                Category = input.Category,
                Status = "Open",
                StatusAr = "مفتوح",
                ReportedAt = DateTime.UtcNow,
                ReportedBy = input.ReportedBy,
                DetectedAt = input.DetectedAt,
                AffectedSystems = input.AffectedSystems
            };

            _logger.LogInformation("Created incident {IncidentCode} for tenant {TenantId}", incident.IncidentCode, tenantId);
            return await Task.FromResult(incident);
        }

        public async Task<IncidentDto> UpdateIncidentAsync(Guid tenantId, Guid incidentId, UpdateIncidentDto input)
        {
            // In production, would update from database
            var incident = new IncidentDto
            {
                Id = incidentId,
                TenantId = tenantId,
                Status = input.Status ?? "Investigating",
                AssignedTo = input.AssignedTo ?? "",
                Resolution = input.Resolution ?? "",
                RootCause = input.RootCause ?? "",
                ImpactedUsers = input.ImpactedUsers ?? 0,
                FinancialImpact = input.FinancialImpact ?? 0
            };

            _logger.LogInformation("Updated incident {IncidentId}", incidentId);
            return await Task.FromResult(incident);
        }

        public async Task<IncidentDto?> GetIncidentAsync(Guid tenantId, Guid incidentId)
        {
            // In production, would query from database
            return await Task.FromResult<IncidentDto?>(null);
        }

        public async Task<List<IncidentDto>> GetIncidentsAsync(Guid tenantId, string? status = null, int page = 1, int pageSize = 20)
        {
            // In production, would query from database
            return await Task.FromResult(new List<IncidentDto>());
        }

        public async Task<IncidentDto> EscalateIncidentAsync(Guid tenantId, Guid incidentId, string escalatedTo, string reason)
        {
            var incident = new IncidentDto
            {
                Id = incidentId,
                TenantId = tenantId,
                Status = "Escalated",
                StatusAr = "تم التصعيد",
                EscalatedTo = escalatedTo
            };

            _logger.LogInformation("Escalated incident {IncidentId} to {EscalatedTo}", incidentId, escalatedTo);
            return await Task.FromResult(incident);
        }

        public async Task<IncidentDto> ResolveIncidentAsync(Guid tenantId, Guid incidentId, string resolvedBy, string resolution)
        {
            var incident = new IncidentDto
            {
                Id = incidentId,
                TenantId = tenantId,
                Status = "Resolved",
                StatusAr = "تم الحل",
                Resolution = resolution,
                ResolvedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Resolved incident {IncidentId} by {ResolvedBy}", incidentId, resolvedBy);
            return await Task.FromResult(incident);
        }

        public async Task<IncidentDto> CloseIncidentAsync(Guid tenantId, Guid incidentId, string closedBy, string lessonsLearned)
        {
            var incident = new IncidentDto
            {
                Id = incidentId,
                TenantId = tenantId,
                Status = "Closed",
                StatusAr = "مغلق",
                LessonsLearned = lessonsLearned,
                ClosedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Closed incident {IncidentId} by {ClosedBy}", incidentId, closedBy);
            return await Task.FromResult(incident);
        }

        public async Task<IncidentMetricsDto> GetIncidentMetricsAsync(Guid tenantId)
        {
            // In production, would calculate from database
            return await Task.FromResult(new IncidentMetricsDto
            {
                TenantId = tenantId,
                TotalIncidents = 0,
                OpenIncidents = 0,
                ResolvedIncidents = 0,
                CriticalIncidents = 0,
                AverageResponseTimeHours = 0,
                AverageResolutionTimeHours = 0,
                MttrHours = 0,
                MttdHours = 0,
                IncidentsThisMonth = 0,
                IncidentsLastMonth = 0,
                Trend = "Stable",
                CalculatedAt = DateTime.UtcNow
            });
        }

        // ============ Business Continuity ============

        public async Task<BcmScoreDto> GetBcmScoreAsync(Guid tenantId)
        {
            // Calculate from resilience assessments
            var resiliences = await _context.Resiliences
                .Where(r => r.TenantId == tenantId && !r.IsDeleted)
                .ToListAsync();

            var avgBcm = resiliences.Any() ? (double)resiliences.Average(r => r.BusinessContinuityScore ?? 0) : 0.0;
            var avgDr = resiliences.Any() ? (double)resiliences.Average(r => r.DisasterRecoveryScore ?? 0) : 0.0;

            var overallBcm = (avgBcm + avgDr) / 2;
            return new BcmScoreDto
            {
                TenantId = tenantId,
                OverallScore = (decimal)overallBcm,
                BiaScore = (decimal)(avgBcm * 0.3),
                BcpScore = (decimal)(avgBcm * 0.7),
                TestingScore = 70, // Default
                TrainingScore = 60, // Default
                MaturityLevel = GetMaturityLevel(overallBcm),
                MaturityLevelAr = GetMaturityLevelAr(overallBcm),
                CriticalProcessesIdentified = resiliences.Count,
                CriticalProcessesCovered = resiliences.Count(r => r.Status == "Completed"),
                CalculatedAt = DateTime.UtcNow
            };
        }

        public async Task<DrReadinessDto> GetDrReadinessAsync(Guid tenantId)
        {
            var resiliences = await _context.Resiliences
                .Where(r => r.TenantId == tenantId && !r.IsDeleted)
                .ToListAsync();

            var avgDr = resiliences.Any() ? (double)resiliences.Average(r => r.DisasterRecoveryScore ?? 0) : 0.0;

            return new DrReadinessDto
            {
                TenantId = tenantId,
                OverallScore = (decimal)avgDr,
                RtoCompliance = 75, // Default
                RpoCompliance = 80, // Default
                BackupScore = 85, // Default
                FailoverScore = 70, // Default
                ReadinessLevel = GetReadinessLevel(avgDr),
                ReadinessLevelAr = GetReadinessLevelAr(avgDr),
                SystemsWithDrPlan = resiliences.Count(r => r.Status == "Completed"),
                TotalCriticalSystems = Math.Max(resiliences.Count, 1),
                AverageRtoHours = 4,
                AverageRpoHours = 1,
                CalculatedAt = DateTime.UtcNow
            };
        }

        public async Task<ResilienceDashboardDto> GetResilienceDashboardAsync(Guid tenantId)
        {
            var bcm = await GetBcmScoreAsync(tenantId);
            var dr = await GetDrReadinessAsync(tenantId);
            var incidents = await GetIncidentMetricsAsync(tenantId);

            var resiliences = await _context.Resiliences
                .Where(r => r.TenantId == tenantId && !r.IsDeleted)
                .ToListAsync();

            var overallScore = (bcm.OverallScore + dr.OverallScore) / 2;
            var cyberScore = resiliences.Any() ? (decimal)resiliences.Average(r => r.CyberResilienceScore ?? 0) : 0;
            var opScore = resiliences.Any() ? (decimal)resiliences.Average(r => r.ResilienceScore ?? 0) : 0;

            return new ResilienceDashboardDto
            {
                TenantId = tenantId,
                OverallResilienceScore = overallScore,
                BusinessContinuity = bcm,
                DisasterRecovery = dr,
                IncidentResponse = incidents,
                CyberResilienceScore = cyberScore,
                OperationalResilienceScore = opScore,
                OverallMaturity = GetMaturityLevel((double)overallScore),
                OverallMaturityAr = GetMaturityLevelAr((double)overallScore),
                Recommendations = GenerateRecommendations(bcm, dr, cyberScore),
                GeneratedAt = DateTime.UtcNow
            };
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

        private string GenerateIncidentNumber(Guid tenantId)
        {
            var year = DateTime.UtcNow.Year;
            return $"INC-{year}-{Guid.NewGuid().ToString()[..4].ToUpper()}";
        }

        private static string GetMaturityLevel(double score) => score switch
        {
            >= 80 => "Optimized",
            >= 60 => "Managed",
            >= 40 => "Defined",
            >= 20 => "Developing",
            _ => "Initial"
        };

        private static string GetMaturityLevelAr(double score) => score switch
        {
            >= 80 => "محسّن",
            >= 60 => "مُدار",
            >= 40 => "محدد",
            >= 20 => "قيد التطوير",
            _ => "أولي"
        };

        private static string GetReadinessLevel(double score) => score switch
        {
            >= 80 => "FullyReady",
            >= 60 => "Ready",
            >= 40 => "PartiallyReady",
            _ => "NotReady"
        };

        private static string GetReadinessLevelAr(double score) => score switch
        {
            >= 80 => "جاهز بالكامل",
            >= 60 => "جاهز",
            >= 40 => "جاهز جزئياً",
            _ => "غير جاهز"
        };

        private static List<ResilienceRecommendationDto> GenerateRecommendations(BcmScoreDto bcm, DrReadinessDto dr, decimal cyberScore)
        {
            var recommendations = new List<ResilienceRecommendationDto>();

            if (bcm.OverallScore < 70)
            {
                recommendations.Add(new ResilienceRecommendationDto
                {
                    Area = "Business Continuity",
                    Priority = "High",
                    Recommendation = "Improve Business Impact Analysis and update BCP documentation",
                    RecommendationAr = "تحسين تحليل تأثير الأعمال وتحديث وثائق خطة استمرارية الأعمال",
                    Impact = "Reduces recovery time and improves organizational resilience"
                });
            }

            if (dr.OverallScore < 70)
            {
                recommendations.Add(new ResilienceRecommendationDto
                {
                    Area = "Disaster Recovery",
                    Priority = "High",
                    Recommendation = "Conduct DR testing and validate RTO/RPO objectives",
                    RecommendationAr = "إجراء اختبار التعافي من الكوارث والتحقق من أهداف RTO/RPO",
                    Impact = "Ensures systems can be recovered within target timeframes"
                });
            }

            if (cyberScore < 70)
            {
                recommendations.Add(new ResilienceRecommendationDto
                {
                    Area = "Cyber Resilience",
                    Priority = "Medium",
                    Recommendation = "Enhance cyber incident response capabilities",
                    RecommendationAr = "تعزيز قدرات الاستجابة للحوادث السيبرانية",
                    Impact = "Improves ability to detect and respond to cyber threats"
                });
            }

            return recommendations;
        }
    }
}
