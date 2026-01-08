using AutoMapper;
using GrcMvc.Data;
using GrcMvc.Models.DTOs;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using GrcMvc.Application.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Service implementation for Risk management
    /// </summary>
    public class RiskService : IRiskService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<RiskService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly PolicyEnforcementHelper _policyHelper;
        private readonly IWorkspaceContextService? _workspaceContext;

        public RiskService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<RiskService> logger,
            IHttpContextAccessor httpContextAccessor,
            PolicyEnforcementHelper policyHelper,
            IWorkspaceContextService? workspaceContext = null)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _policyHelper = policyHelper ?? throw new ArgumentNullException(nameof(policyHelper));
            _workspaceContext = workspaceContext;
        }

        public async Task<RiskDto?> GetByIdAsync(Guid id)
        {
            try
            {
                var risk = await _unitOfWork.Risks.GetByIdAsync(id);

                if (risk == null)
                {
                    _logger.LogWarning("Risk with ID {Id} not found", id);
                    return null;
                }

                return _mapper.Map<RiskDto>(risk);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving risk with ID {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<RiskDto>> GetAllAsync()
        {
            try
            {
                var risks = await _unitOfWork.Risks.GetAllAsync();
                return _mapper.Map<IEnumerable<RiskDto>>(risks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all risks");
                throw;
            }
        }

        public async Task<RiskDto> CreateAsync(CreateRiskDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            try
            {
                // Map DTO to entity
                var risk = _mapper.Map<Risk>(dto);

                // Set workspace context if available
                if (_workspaceContext != null && _workspaceContext.HasWorkspaceContext())
                {
                    risk.WorkspaceId = _workspaceContext.GetCurrentWorkspaceId();
                }

                // Set audit fields
                risk.CreatedBy = GetCurrentUser();
                risk.CreatedDate = DateTime.UtcNow;

                // Enforce policies before saving (extract from entity or use defaults)
                await _policyHelper.EnforceCreateAsync(
                    resourceType: "Risk",
                    resource: risk,
                    dataClassification: null, // Will be set to "internal" by helper if null
                    owner: risk.Owner);

                // Save to database
                await _unitOfWork.Risks.AddAsync(risk);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Risk created with ID {Id} by {User}", risk.Id, risk.CreatedBy);

                return _mapper.Map<RiskDto>(risk);
            }
            catch (PolicyViolationException pve)
            {
                _logger.LogWarning("Policy violation prevented risk creation: {Message}. Rule: {RuleId}",
                    pve.Message, pve.RuleId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating risk");
                throw;
            }
        }

        public async Task<RiskDto> UpdateAsync(Guid id, UpdateRiskDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            try
            {
                // Get existing risk
                var risk = await _unitOfWork.Risks.GetByIdAsync(id);
                if (risk == null)
                {
                    throw new KeyNotFoundException($"Risk with ID {id} not found");
                }

                // Map updated values
                _mapper.Map(dto, risk);

                // Enforce policies before updating
                await _policyHelper.EnforceUpdateAsync(
                    resourceType: "Risk",
                    resource: risk,
                    dataClassification: null, // Will be set to "internal" by helper if null
                    owner: risk.Owner);

                // Update audit fields
                risk.ModifiedBy = GetCurrentUser();
                risk.ModifiedDate = DateTime.UtcNow;

                // Update in database
                await _unitOfWork.Risks.UpdateAsync(risk);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Risk {Id} updated by {User}", id, risk.ModifiedBy);

                return _mapper.Map<RiskDto>(risk);
            }
            catch (PolicyViolationException pve)
            {
                _logger.LogWarning("Policy violation prevented risk update: {Message}. Rule: {RuleId}",
                    pve.Message, pve.RuleId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating risk {Id}", id);
                throw;
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            try
            {
                var risk = await _unitOfWork.Risks.GetByIdAsync(id);
                if (risk == null)
                {
                    throw new KeyNotFoundException($"Risk with ID {id} not found");
                }

                // Soft delete
                risk.IsDeleted = true;
                risk.ModifiedBy = GetCurrentUser();
                risk.ModifiedDate = DateTime.UtcNow;

                await _unitOfWork.Risks.UpdateAsync(risk);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Risk {Id} soft deleted by {User}", id, risk.ModifiedBy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting risk {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<RiskDto>> GetByStatusAsync(string status)
        {
            try
            {
                var risks = await _unitOfWork.Risks.FindAsync(r => r.Status == status);
                return _mapper.Map<IEnumerable<RiskDto>>(risks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving risks by status {Status}", status);
                throw;
            }
        }

        public async Task<IEnumerable<RiskDto>> GetByCategoryAsync(string category)
        {
            try
            {
                var risks = await _unitOfWork.Risks.FindAsync(r => r.Category == category);
                return _mapper.Map<IEnumerable<RiskDto>>(risks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving risks by category {Category}", category);
                throw;
            }
        }

        public async Task<RiskStatisticsDto> GetStatisticsAsync()
        {
            try
            {
                var riskList = (await _unitOfWork.Risks.GetAllAsync()).ToList();

                var statistics = new RiskStatisticsDto
                {
                    TotalRisks = riskList.Count,
                    ActiveRisks = riskList.Count(r => r.Status == "Active"),
                    HighRisks = riskList.Count(r => r.RiskLevel == "High" || r.RiskLevel == "Critical"),
                    MediumRisks = riskList.Count(r => r.RiskLevel == "Medium"),
                    LowRisks = riskList.Count(r => r.RiskLevel == "Low"),
                    RisksByCategory = riskList
                        .GroupBy(r => r.Category)
                        .ToDictionary(g => g.Key ?? "Unknown", g => g.Count()),
                    AverageRiskScore = riskList.Any() ? riskList.Average(r => r.RiskScore) : 0
                };

                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating risk statistics");
                throw;
            }
        }

        private string GetCurrentUser()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.Identity?.Name ?? "System";
        }

        public async Task AcceptAsync(Guid id)
        {
            try
            {
                var risk = await _unitOfWork.Risks.GetByIdAsync(id);
                if (risk == null)
                {
                    throw new KeyNotFoundException($"Risk with ID {id} not found");
                }

                risk.Status = "Accepted";
                risk.ModifiedBy = GetCurrentUser();
                risk.ModifiedDate = DateTime.UtcNow;

                await _unitOfWork.Risks.UpdateAsync(risk);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Risk {Id} accepted by {User}", id, risk.ModifiedBy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting risk {Id}", id);
                throw;
            }
        }

        #region Risk Scoring

        public async Task<RiskScoreResultDto> CalculateRiskScoreAsync(Guid riskId)
        {
            var risk = await _unitOfWork.Risks.GetByIdAsync(riskId);
            if (risk == null)
                throw new KeyNotFoundException($"Risk {riskId} not found");

            var inherentRisk = risk.Likelihood * risk.Impact;
            var riskLevel = Configuration.RiskScoringConfiguration.GetRiskLevel(inherentRisk);

            return new RiskScoreResultDto
            {
                RiskId = riskId,
                Likelihood = risk.Likelihood,
                Impact = risk.Impact,
                InherentRisk = inherentRisk,
                ResidualRisk = risk.ResidualRisk,
                RiskLevel = riskLevel,
                RiskLevelAr = GetArabicRiskLevel(riskLevel),
                ControlEffectiveness = await CalculateControlEffectivenessAsync(riskId),
                CalculatedAt = DateTime.UtcNow
            };
        }

        public async Task<RiskScoreResultDto> CalculateResidualRiskAsync(Guid riskId)
        {
            var risk = await _unitOfWork.Risks.GetByIdAsync(riskId);
            if (risk == null)
                throw new KeyNotFoundException($"Risk {riskId} not found");

            var inherentRisk = risk.Likelihood * risk.Impact;
            var controlEffectiveness = await CalculateControlEffectivenessAsync(riskId);
            
            // Residual = Inherent * (1 - ControlEffectiveness/100)
            var residualRisk = (int)(inherentRisk * (1 - controlEffectiveness / 100m));
            
            risk.ResidualRisk = residualRisk;
            risk.ModifiedDate = DateTime.UtcNow;
            await _unitOfWork.Risks.UpdateAsync(risk);
            await _unitOfWork.SaveChangesAsync();

            var riskLevel = Configuration.RiskScoringConfiguration.GetRiskLevel(residualRisk);

            return new RiskScoreResultDto
            {
                RiskId = riskId,
                Likelihood = risk.Likelihood,
                Impact = risk.Impact,
                InherentRisk = inherentRisk,
                ResidualRisk = residualRisk,
                RiskLevel = riskLevel,
                RiskLevelAr = GetArabicRiskLevel(riskLevel),
                ControlEffectiveness = controlEffectiveness,
                CalculatedAt = DateTime.UtcNow
            };
        }

        public async Task<List<RiskScoreHistoryDto>> GetScoreHistoryAsync(Guid riskId, int months = 12)
        {
            // In production, this would query an audit/history table
            // For now, generate sample trend data
            var history = new List<RiskScoreHistoryDto>();
            var random = new Random(riskId.GetHashCode());

            for (int i = months - 1; i >= 0; i--)
            {
                var inherent = random.Next(10, 25);
                var residual = random.Next(5, inherent);
                history.Add(new RiskScoreHistoryDto
                {
                    Date = DateTime.UtcNow.AddMonths(-i),
                    InherentRisk = inherent,
                    ResidualRisk = residual,
                    RiskLevel = Configuration.RiskScoringConfiguration.GetRiskLevel(residual)
                });
            }

            return await Task.FromResult(history);
        }

        #endregion

        #region Risk-Assessment Linkage

        public async Task<RiskDto> LinkToAssessmentAsync(Guid riskId, Guid assessmentId, string? findingReference = null)
        {
            var risk = await _unitOfWork.Risks.GetByIdAsync(riskId);
            if (risk == null)
                throw new KeyNotFoundException($"Risk {riskId} not found");

            // Store linkage in Labels
            var labels = risk.Labels;
            labels["LinkedAssessmentId"] = assessmentId.ToString();
            if (!string.IsNullOrEmpty(findingReference))
                labels["FindingReference"] = findingReference;
            risk.Labels = labels;
            
            risk.ModifiedBy = GetCurrentUser();
            risk.ModifiedDate = DateTime.UtcNow;

            await _unitOfWork.Risks.UpdateAsync(risk);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Risk {RiskId} linked to assessment {AssessmentId}", riskId, assessmentId);
            return _mapper.Map<RiskDto>(risk);
        }

        public async Task<List<RiskDto>> GetRisksByAssessmentAsync(Guid assessmentId)
        {
            var assessmentIdStr = assessmentId.ToString();
            var allRisks = await _unitOfWork.Risks.GetAllAsync();
            
            var linkedRisks = allRisks
                .Where(r => r.Labels.TryGetValue("LinkedAssessmentId", out var linked) && linked == assessmentIdStr)
                .ToList();

            return _mapper.Map<List<RiskDto>>(linkedRisks);
        }

        public async Task<List<RiskDto>> GenerateRisksFromAssessmentAsync(Guid assessmentId, Guid tenantId)
        {
            var assessment = await _unitOfWork.Assessments.GetByIdAsync(assessmentId);
            if (assessment == null)
                throw new KeyNotFoundException($"Assessment {assessmentId} not found");

            var generatedRisks = new List<RiskDto>();

            // Find low-scoring requirements (< 50%) and generate risks
            var requirements = assessment.Requirements?.Where(r => (r.Score ?? 0) < 50).ToList() ?? new List<AssessmentRequirement>();

            foreach (var req in requirements.Take(10)) // Limit to top 10 gaps
            {
                var createDto = new CreateRiskDto
                {
                    Name = $"Gap: {req.ControlTitle}",
                    Description = $"Risk identified from assessment gap. Control {req.ControlNumber} scored {req.Score ?? 0}%. Finding: {req.Findings}",
                    Category = "Compliance",
                    Probability = 3,
                    Impact = GetImpactFromScore(req.Score ?? 0),
                    Status = "Identified",
                    Owner = assessment.AssignedTo
                };

                var risk = await CreateAsync(createDto);
                await LinkToAssessmentAsync(risk.Id, assessmentId, req.ControlNumber);
                generatedRisks.Add(risk);
            }

            _logger.LogInformation("Generated {Count} risks from assessment {AssessmentId}", generatedRisks.Count, assessmentId);
            return generatedRisks;
        }

        #endregion

        #region Risk-Control Mapping

        public async Task<RiskControlMappingDto> LinkControlAsync(Guid riskId, Guid controlId, int expectedEffectiveness)
        {
            var risk = await _unitOfWork.Risks.GetByIdAsync(riskId);
            if (risk == null)
                throw new KeyNotFoundException($"Risk {riskId} not found");

            var control = await _unitOfWork.Controls.GetByIdAsync(controlId);
            if (control == null)
                throw new KeyNotFoundException($"Control {controlId} not found");

            // Create or update mapping
            var mapping = new RiskControlMapping
            {
                Id = Guid.NewGuid(),
                RiskId = riskId,
                ControlId = controlId,
                ExpectedEffectiveness = expectedEffectiveness,
                ActualEffectiveness = control.EffectivenessScore,
                MappingStrength = GetMappingStrength(expectedEffectiveness),
                LastAssessedDate = DateTime.UtcNow,
                LastAssessedBy = GetCurrentUser()
            };

            await _unitOfWork.RiskControlMappings.AddAsync(mapping);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Control {ControlId} linked to risk {RiskId}", controlId, riskId);

            return new RiskControlMappingDto
            {
                Id = mapping.Id,
                RiskId = riskId,
                ControlId = controlId,
                ControlCode = control.ControlCode,
                ControlName = control.Name,
                ExpectedEffectiveness = expectedEffectiveness,
                ActualEffectiveness = control.EffectivenessScore,
                MappingStrength = mapping.MappingStrength,
                MappedAt = mapping.LastAssessedDate ?? DateTime.UtcNow
            };
        }

        public async Task<List<RiskControlMappingDto>> GetLinkedControlsAsync(Guid riskId)
        {
            var mappings = await _unitOfWork.RiskControlMappings.FindAsync(m => m.RiskId == riskId);
            var result = new List<RiskControlMappingDto>();

            foreach (var mapping in mappings)
            {
                var control = await _unitOfWork.Controls.GetByIdAsync(mapping.ControlId);
                result.Add(new RiskControlMappingDto
                {
                    Id = mapping.Id,
                    RiskId = mapping.RiskId,
                    ControlId = mapping.ControlId,
                    ControlCode = control?.ControlCode ?? "",
                    ControlName = control?.Name ?? "",
                    ExpectedEffectiveness = mapping.ExpectedEffectiveness,
                    ActualEffectiveness = mapping.ActualEffectiveness,
                    MappingStrength = mapping.MappingStrength,
                    MappedAt = mapping.LastAssessedDate ?? mapping.CreatedDate
                });
            }

            return result;
        }

        public async Task<decimal> CalculateControlEffectivenessAsync(Guid riskId)
        {
            var mappings = await _unitOfWork.RiskControlMappings.FindAsync(m => m.RiskId == riskId);
            var mappingList = mappings.ToList();

            if (!mappingList.Any())
                return 0;

            // Weighted average of control effectiveness
            var totalWeight = mappingList.Sum(m => m.ExpectedEffectiveness);
            if (totalWeight == 0) return 0;

            var weightedSum = mappingList.Sum(m => m.ActualEffectiveness * m.ExpectedEffectiveness);
            return Math.Round((decimal)weightedSum / totalWeight, 2);
        }

        #endregion

        #region Risk Posture

        public async Task<RiskPostureSummaryDto> GetRiskPostureAsync(Guid tenantId)
        {
            var allRisks = (await _unitOfWork.Risks.FindAsync(r => r.TenantId == tenantId)).ToList();

            var posture = new RiskPostureSummaryDto
            {
                TenantId = tenantId,
                TotalRisks = allRisks.Count,
                CriticalRisks = allRisks.Count(r => r.RiskLevel == "Critical"),
                HighRisks = allRisks.Count(r => r.RiskLevel == "High"),
                MediumRisks = allRisks.Count(r => r.RiskLevel == "Medium"),
                LowRisks = allRisks.Count(r => r.RiskLevel == "Low"),
                AcceptedRisks = allRisks.Count(r => r.Status == "Accepted"),
                MitigatedRisks = allRisks.Count(r => r.Status == "Mitigated"),
                OpenRisks = allRisks.Count(r => r.Status == "Active" || r.Status == "Identified"),
                OverallRiskScore = allRisks.Any() ? (decimal)allRisks.Average(r => r.ResidualRisk) : 0,
                RiskTrend = "Stable", // Would calculate from history
                AverageControlEffectiveness = 0,
                CalculatedAt = DateTime.UtcNow
            };

            // Calculate average control effectiveness across all risks
            if (allRisks.Any())
            {
                var effectivenessSum = 0m;
                foreach (var risk in allRisks)
                {
                    effectivenessSum += await CalculateControlEffectivenessAsync(risk.Id);
                }
                posture.AverageControlEffectiveness = Math.Round(effectivenessSum / allRisks.Count, 2);
            }

            return posture;
        }

        public async Task<RiskHeatMapDto> GetHeatMapAsync(Guid tenantId)
        {
            var allRisks = (await _unitOfWork.Risks.FindAsync(r => r.TenantId == tenantId)).ToList();

            var heatMap = new RiskHeatMapDto
            {
                TenantId = tenantId,
                GeneratedAt = DateTime.UtcNow
            };

            // Create 5x5 matrix
            for (int likelihood = 1; likelihood <= 5; likelihood++)
            {
                for (int impact = 1; impact <= 5; impact++)
                {
                    var risksInCell = allRisks.Where(r => r.Likelihood == likelihood && r.Impact == impact).ToList();
                    heatMap.Cells.Add(new HeatMapCellDto
                    {
                        Likelihood = likelihood,
                        Impact = impact,
                        RiskCount = risksInCell.Count,
                        RiskLevel = Configuration.RiskScoringConfiguration.GetRiskLevel(likelihood * impact),
                        RiskNames = risksInCell.Select(r => r.Name).Take(5).ToList()
                    });
                }
            }

            return heatMap;
        }

        #endregion

        #region Private Helpers

        private static string GetArabicRiskLevel(string level) => level switch
        {
            "Critical" => "حرج",
            "High" => "مرتفع",
            "Medium" => "متوسط",
            "Low" => "منخفض",
            _ => "غير محدد"
        };

        private static int GetImpactFromScore(int score) => score switch
        {
            < 20 => 5,
            < 40 => 4,
            < 60 => 3,
            < 80 => 2,
            _ => 1
        };

        private static string GetMappingStrength(int effectiveness) => effectiveness switch
        {
            >= 80 => "Strong",
            >= 50 => "Moderate",
            _ => "Weak"
        };

        #endregion
    }
}