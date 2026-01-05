using AutoMapper;
using GrcMvc.Data;
using GrcMvc.Models.DTOs;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using GrcMvc.Application.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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

        public RiskService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<RiskService> logger,
            IHttpContextAccessor httpContextAccessor,
            PolicyEnforcementHelper policyHelper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _policyHelper = policyHelper ?? throw new ArgumentNullException(nameof(policyHelper));
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

                // Set audit fields
                risk.CreatedBy = GetCurrentUser();
                risk.CreatedDate = DateTime.UtcNow;

                // Enforce policies before saving (extract from entity or use defaults)
                await _policyHelper.EnforceCreateAsync(
                    resourceType: "Risk",
                    resource: risk,
                    dataClassification: null, // Will be set to "internal" by helper if null
                    owner: risk.Owner);

                // Add to repository
                var createdRisk = await _unitOfWork.Risks.AddAsync(risk);

                _logger.LogInformation("Risk created with ID {Id} by {User}", createdRisk.Id, risk.CreatedBy);

                return _mapper.Map<RiskDto>(createdRisk);
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

                // Update in repository
                await _unitOfWork.Risks.UpdateAsync(risk);

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
                var risks = await _unitOfWork.Risks.GetAllAsync();
                var riskList = risks.ToList();

                var statistics = new RiskStatisticsDto
                {
                    TotalRisks = riskList.Count,
                    ActiveRisks = riskList.Count(r => r.Status == "Active"),
                    HighRisks = riskList.Count(r => r.RiskLevel == "High" || r.RiskLevel == "Critical"),
                    MediumRisks = riskList.Count(r => r.RiskLevel == "Medium"),
                    LowRisks = riskList.Count(r => r.RiskLevel == "Low"),
                    RisksByCategory = riskList
                        .GroupBy(r => r.Category)
                        .ToDictionary(g => g.Key, g => g.Count()),
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
    }
}