using AutoMapper;
using GrcMvc.Common;
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

        public async Task<Result<RiskDto>> GetByIdAsync(Guid id)
        {
            try
            {
                var risk = await _unitOfWork.Risks.GetByIdAsync(id);
                var result = Guard.EnsureNotNull(risk, "Risk", id);
                
                if (result.IsFailure)
                {
                    _logger.LogWarning("Risk with ID {Id} not found", id);
                    return Result.NotFound<RiskDto>("Risk", id);
                }

                return Result.Success(_mapper.Map<RiskDto>(risk));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving risk with ID {Id}", id);
                return Result.Failure<RiskDto>($"Error retrieving risk: {ex.Message}");
            }
        }

        public async Task<Result<IEnumerable<RiskDto>>> GetAllAsync()
        {
            try
            {
                var risks = await _unitOfWork.Risks.GetAllAsync();
                return Result.Success(_mapper.Map<IEnumerable<RiskDto>>(risks));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all risks");
                return Result.Failure<IEnumerable<RiskDto>>($"Error retrieving risks: {ex.Message}");
            }
        }

        public async Task<Result<RiskDto>> CreateAsync(CreateRiskDto dto)
        {
            if (dto == null)
            {
                return Result.ValidationError<RiskDto>("CreateRiskDto cannot be null");
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

                return Result.Success(_mapper.Map<RiskDto>(risk));
            }
            catch (PolicyViolationException pve)
            {
                _logger.LogWarning("Policy violation prevented risk creation: {Message}. Rule: {RuleId}",
                    pve.Message, pve.RuleId);
                return Result.Unauthorized<RiskDto>(pve.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating risk");
                return Result.Failure<RiskDto>($"Error creating risk: {ex.Message}");
            }
        }

        public async Task<Result<RiskDto>> UpdateAsync(Guid id, UpdateRiskDto dto)
        {
            if (dto == null)
            {
                return Result.ValidationError<RiskDto>("UpdateRiskDto cannot be null");
            }

            try
            {
                // Get existing risk
                var risk = await _unitOfWork.Risks.GetByIdAsync(id);
                var guardResult = Guard.EnsureNotNull(risk, "Risk", id);
                if (guardResult.IsFailure)
                {
                    return Result.NotFound<RiskDto>("Risk", id);
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

                return Result.Success(_mapper.Map<RiskDto>(risk));
            }
            catch (PolicyViolationException pve)
            {
                _logger.LogWarning("Policy violation prevented risk update: {Message}. Rule: {RuleId}",
                    pve.Message, pve.RuleId);
                return Result.Unauthorized<RiskDto>(pve.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating risk {Id}", id);
                return Result.Failure<RiskDto>($"Error updating risk: {ex.Message}");
            }
        }

        public async Task<Result> DeleteAsync(Guid id)
        {
            try
            {
                var risk = await _unitOfWork.Risks.GetByIdAsync(id);
                var guardResult = Guard.EnsureNotNull(risk, "Risk", id);
                if (guardResult.IsFailure)
                {
                    return Result.NotFound("Risk", id);
                }

                // Soft delete
                risk.IsDeleted = true;
                risk.ModifiedBy = GetCurrentUser();
                risk.ModifiedDate = DateTime.UtcNow;

                await _unitOfWork.Risks.UpdateAsync(risk);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Risk {Id} soft deleted by {User}", id, risk.ModifiedBy);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting risk {Id}", id);
                return Result.Failure($"Error deleting risk: {ex.Message}");
            }
        }

        public async Task<Result<IEnumerable<RiskDto>>> GetByStatusAsync(string status)
        {
            try
            {
                var risks = await _unitOfWork.Risks.FindAsync(r => r.Status == status);
                return Result.Success(_mapper.Map<IEnumerable<RiskDto>>(risks));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving risks by status {Status}", status);
                return Result.Failure<IEnumerable<RiskDto>>($"Error retrieving risks by status: {ex.Message}");
            }
        }

        public async Task<Result<IEnumerable<RiskDto>>> GetByCategoryAsync(string category)
        {
            try
            {
                var risks = await _unitOfWork.Risks.FindAsync(r => r.Category == category);
                return Result.Success(_mapper.Map<IEnumerable<RiskDto>>(risks));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving risks by category {Category}", category);
                return Result.Failure<IEnumerable<RiskDto>>($"Error retrieving risks by category: {ex.Message}");
            }
        }

        public async Task<Result<RiskStatisticsDto>> GetStatisticsAsync()
        {
            try
            {
                var riskList = (await _unitOfWork.Risks.GetAllAsync()).ToList();

                var statistics = new RiskStatisticsDto
                {
                    TotalRisks = riskList.Count,
                    ActiveRisks = riskList.Count(r => r.Status == "Active" || r.Status == "Identified"),
                    CriticalRisks = riskList.Count(r => r.RiskLevel == "Critical"),
                    HighRisks = riskList.Count(r => r.RiskLevel == "High"),
                    MediumRisks = riskList.Count(r => r.RiskLevel == "Medium"),
                    LowRisks = riskList.Count(r => r.RiskLevel == "Low"),
                    MitigatedRisks = riskList.Count(r => r.Status == "Mitigated"),
                    AcceptedRisks = riskList.Count(r => r.Status == "Accepted"),
                    ClosedRisks = riskList.Count(r => r.Status == "Closed"),
                    OpenRisks = riskList.Count(r => r.Status != "Closed" && r.Status != "Mitigated"),
                    RisksByCategory = riskList
                        .GroupBy(r => r.Category)
                        .ToDictionary(g => g.Key ?? "Unknown", g => g.Count()),
                    AverageRiskScore = riskList.Any() ? riskList.Average(r => r.RiskScore) : 0
                };

                return Result.Success(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating risk statistics");
                return Result.Failure<RiskStatisticsDto>($"Error calculating statistics: {ex.Message}");
            }
        }

        private string GetCurrentUser()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.Identity?.Name ?? "System";
        }

        public async Task<Result> AcceptAsync(Guid id)
        {
            try
            {
                var risk = await _unitOfWork.Risks.GetByIdAsync(id);
                var guardResult = Guard.EnsureNotNull(risk, "Risk", id);
                if (guardResult.IsFailure)
                {
                    return Result.NotFound("Risk", id);
                }

                risk.Status = "Accepted";
                risk.ModifiedBy = GetCurrentUser();
                risk.ModifiedDate = DateTime.UtcNow;

                await _unitOfWork.Risks.UpdateAsync(risk);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Risk {Id} accepted by {User}", id, risk.ModifiedBy);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting risk {Id}", id);
                return Result.Failure($"Error accepting risk: {ex.Message}");
            }
        }

        #region Risk Scoring

        public async Task<Result<RiskScoreResultDto>> CalculateRiskScoreAsync(Guid riskId)
        {
            try
            {
                var risk = await _unitOfWork.Risks.GetByIdAsync(riskId);
                var guardResult = Guard.EnsureNotNull(risk, "Risk", riskId);
                if (guardResult.IsFailure)
                {
                    return Result.NotFound<RiskScoreResultDto>("Risk", riskId);
                }

                var inherentRisk = risk.Likelihood * risk.Impact;
                var riskLevel = Configuration.RiskScoringConfiguration.GetRiskLevel(inherentRisk);
                var effectivenessResult = await CalculateControlEffectivenessAsync(riskId);

                return Result.Success(new RiskScoreResultDto
                {
                    RiskId = riskId,
                    Likelihood = risk.Likelihood,
                    Impact = risk.Impact,
                    InherentRisk = inherentRisk,
                    ResidualRisk = risk.ResidualRisk,
                    RiskLevel = riskLevel,
                    RiskLevelAr = GetArabicRiskLevel(riskLevel),
                    ControlEffectiveness = effectivenessResult.IsSuccess ? effectivenessResult.Value : 0,
                    CalculatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating risk score for {RiskId}", riskId);
                return Result.Failure<RiskScoreResultDto>($"Error calculating risk score: {ex.Message}");
            }
        }

        public async Task<Result<RiskScoreResultDto>> CalculateResidualRiskAsync(Guid riskId)
        {
            try
            {
                var risk = await _unitOfWork.Risks.GetByIdAsync(riskId);
                var guardResult = Guard.EnsureNotNull(risk, "Risk", riskId);
                if (guardResult.IsFailure)
                {
                    return Result.NotFound<RiskScoreResultDto>("Risk", riskId);
                }

                var inherentRisk = risk.Likelihood * risk.Impact;
                var effectivenessResult = await CalculateControlEffectivenessAsync(riskId);
                var controlEffectiveness = effectivenessResult.IsSuccess ? effectivenessResult.Value : 0;
                
                // Residual = Inherent * (1 - ControlEffectiveness/100)
                var residualRisk = (int)(inherentRisk * (1 - controlEffectiveness / 100m));
                
                risk.ResidualRisk = residualRisk;
                risk.ModifiedDate = DateTime.UtcNow;
                await _unitOfWork.Risks.UpdateAsync(risk);
                await _unitOfWork.SaveChangesAsync();

                var riskLevel = Configuration.RiskScoringConfiguration.GetRiskLevel(residualRisk);

                return Result.Success(new RiskScoreResultDto
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
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating residual risk for {RiskId}", riskId);
                return Result.Failure<RiskScoreResultDto>($"Error calculating residual risk: {ex.Message}");
            }
        }

        public async Task<Result<List<RiskScoreHistoryDto>>> GetScoreHistoryAsync(Guid riskId, int months = 12)
        {
            try
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

                return await Task.FromResult(Result.Success(history));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting score history for {RiskId}", riskId);
                return Result.Failure<List<RiskScoreHistoryDto>>($"Error getting score history: {ex.Message}");
            }
        }

        #endregion

        #region Risk-Assessment Linkage

        public async Task<Result<RiskDto>> LinkToAssessmentAsync(Guid riskId, Guid assessmentId, string? findingReference = null)
        {
            try
            {
                var risk = await _unitOfWork.Risks.GetByIdAsync(riskId);
                var guardResult = Guard.EnsureNotNull(risk, "Risk", riskId);
                if (guardResult.IsFailure)
                {
                    return Result.NotFound<RiskDto>("Risk", riskId);
                }

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
                return Result.Success(_mapper.Map<RiskDto>(risk));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error linking risk {RiskId} to assessment {AssessmentId}", riskId, assessmentId);
                return Result.Failure<RiskDto>($"Error linking risk to assessment: {ex.Message}");
            }
        }

        public async Task<Result<List<RiskDto>>> GetRisksByAssessmentAsync(Guid assessmentId)
        {
            try
            {
                var assessmentIdStr = assessmentId.ToString();
                var allRisks = await _unitOfWork.Risks.GetAllAsync();
                
                var linkedRisks = allRisks
                    .Where(r => r.Labels.TryGetValue("LinkedAssessmentId", out var linked) && linked == assessmentIdStr)
                    .ToList();

                return Result.Success(_mapper.Map<List<RiskDto>>(linkedRisks));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting risks for assessment {AssessmentId}", assessmentId);
                return Result.Failure<List<RiskDto>>($"Error getting risks by assessment: {ex.Message}");
            }
        }

        public async Task<Result<List<RiskDto>>> GenerateRisksFromAssessmentAsync(Guid assessmentId, Guid tenantId)
        {
            try
            {
                var assessment = await _unitOfWork.Assessments.GetByIdAsync(assessmentId);
                var guardResult = Guard.EnsureNotNull(assessment, "Assessment", assessmentId);
                if (guardResult.IsFailure)
                {
                    return Result.NotFound<List<RiskDto>>("Assessment", assessmentId);
                }

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

                    var riskResult = await CreateAsync(createDto);
                    if (riskResult.IsSuccess)
                    {
                        await LinkToAssessmentAsync(riskResult.Value.Id, assessmentId, req.ControlNumber);
                        generatedRisks.Add(riskResult.Value);
                    }
                }

                _logger.LogInformation("Generated {Count} risks from assessment {AssessmentId}", generatedRisks.Count, assessmentId);
                return Result.Success(generatedRisks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating risks from assessment {AssessmentId}", assessmentId);
                return Result.Failure<List<RiskDto>>($"Error generating risks from assessment: {ex.Message}");
            }
        }

        #endregion

        #region Risk-Control Mapping

        public async Task<Result<RiskControlMappingDto>> LinkControlAsync(Guid riskId, Guid controlId, int expectedEffectiveness)
        {
            try
            {
                var risk = await _unitOfWork.Risks.GetByIdAsync(riskId);
                var riskGuard = Guard.EnsureNotNull(risk, "Risk", riskId);
                if (riskGuard.IsFailure)
                {
                    return Result.NotFound<RiskControlMappingDto>("Risk", riskId);
                }

                var control = await _unitOfWork.Controls.GetByIdAsync(controlId);
                var controlGuard = Guard.EnsureNotNull(control, "Control", controlId);
                if (controlGuard.IsFailure)
                {
                    return Result.NotFound<RiskControlMappingDto>("Control", controlId);
                }

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

                return Result.Success(new RiskControlMappingDto
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
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error linking control {ControlId} to risk {RiskId}", controlId, riskId);
                return Result.Failure<RiskControlMappingDto>($"Error linking control to risk: {ex.Message}");
            }
        }

        public async Task<Result<List<RiskControlMappingDto>>> GetLinkedControlsAsync(Guid riskId)
        {
            try
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

                return Result.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting linked controls for risk {RiskId}", riskId);
                return Result.Failure<List<RiskControlMappingDto>>($"Error getting linked controls: {ex.Message}");
            }
        }

        public async Task<Result<decimal>> CalculateControlEffectivenessAsync(Guid riskId)
        {
            try
            {
                var mappings = await _unitOfWork.RiskControlMappings.FindAsync(m => m.RiskId == riskId);
                var mappingList = mappings.ToList();

                if (!mappingList.Any())
                    return Result.Success(0m);

                // Weighted average of control effectiveness
                var totalWeight = mappingList.Sum(m => m.ExpectedEffectiveness);
                if (totalWeight == 0) return Result.Success(0m);

                var weightedSum = mappingList.Sum(m => m.ActualEffectiveness * m.ExpectedEffectiveness);
                return Result.Success(Math.Round((decimal)weightedSum / totalWeight, 2));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating control effectiveness for risk {RiskId}", riskId);
                return Result.Failure<decimal>($"Error calculating control effectiveness: {ex.Message}");
            }
        }

        #endregion

        #region Risk Posture

        public async Task<Result<RiskPostureSummaryDto>> GetRiskPostureAsync(Guid tenantId)
        {
            try
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
                        var effectivenessResult = await CalculateControlEffectivenessAsync(risk.Id);
                        if (effectivenessResult.IsSuccess)
                        {
                            effectivenessSum += effectivenessResult.Value;
                        }
                    }
                    posture.AverageControlEffectiveness = Math.Round(effectivenessSum / allRisks.Count, 2);
                }

                return Result.Success(posture);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting risk posture for tenant {TenantId}", tenantId);
                return Result.Failure<RiskPostureSummaryDto>($"Error getting risk posture: {ex.Message}");
            }
        }

        public async Task<Result<RiskHeatMapDto>> GetHeatMapAsync(Guid tenantId)
        {
            try
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

                return Result.Success(heatMap);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting risk heat map for tenant {TenantId}", tenantId);
                return Result.Failure<RiskHeatMapDto>($"Error getting risk heat map: {ex.Message}");
            }
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