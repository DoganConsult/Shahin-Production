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
    public class RegulatorService : IRegulatorService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<RegulatorService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly PolicyEnforcementHelper _policyHelper;
        private readonly IWorkspaceContextService? _workspaceContext;

        public RegulatorService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<RegulatorService> logger,
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

        public async Task<RegulatorDto?> GetByIdAsync(Guid id)
        {
            try
            {
                var regulator = await _unitOfWork.Regulators.GetByIdAsync(id);
                if (regulator == null)
                {
                    _logger.LogWarning("Regulator with ID {Id} not found", id);
                    return null;
                }
                return _mapper.Map<RegulatorDto>(regulator);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving regulator with ID {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<RegulatorDto>> GetAllAsync()
        {
            try
            {
                var regulators = await _unitOfWork.Regulators.GetAllAsync();
                return _mapper.Map<IEnumerable<RegulatorDto>>(regulators);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all regulators");
                throw;
            }
        }

        public async Task<RegulatorDto> CreateAsync(CreateRegulatorDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            try
            {
                var regulator = _mapper.Map<Regulator>(dto);
                
                if (_workspaceContext != null && _workspaceContext.HasWorkspaceContext())
                {
                    regulator.WorkspaceId = _workspaceContext.GetCurrentWorkspaceId();
                }

                await _policyHelper.EnforceCreateAsync(
                    resourceType: "Regulator",
                    resource: regulator,
                    dataClassification: regulator.DataClassification,
                    owner: regulator.Owner);

                regulator.CreatedBy = GetCurrentUser();
                regulator.CreatedDate = DateTime.UtcNow;

                await _unitOfWork.Regulators.AddAsync(regulator);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Regulator created with ID {Id}", regulator.Id);
                return _mapper.Map<RegulatorDto>(regulator);
            }
            catch (PolicyViolationException pve)
            {
                _logger.LogWarning(pve, "Policy violation creating regulator");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating regulator");
                throw;
            }
        }

        public async Task<RegulatorDto> UpdateAsync(Guid id, UpdateRegulatorDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            try
            {
                var regulator = await _unitOfWork.Regulators.GetByIdAsync(id);
                if (regulator == null)
                {
                    throw new KeyNotFoundException($"Regulator with ID {id} not found");
                }

                _mapper.Map(dto, regulator);

                await _policyHelper.EnforceUpdateAsync(
                    resourceType: "Regulator",
                    resource: regulator,
                    dataClassification: regulator.DataClassification,
                    owner: regulator.Owner);

                regulator.ModifiedBy = GetCurrentUser();
                regulator.ModifiedDate = DateTime.UtcNow;

                await _unitOfWork.Regulators.UpdateAsync(regulator);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Regulator updated with ID {Id}", id);
                return _mapper.Map<RegulatorDto>(regulator);
            }
            catch (PolicyViolationException pve)
            {
                _logger.LogWarning(pve, "Policy violation updating regulator");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating regulator with ID {Id}", id);
                throw;
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            try
            {
                var regulator = await _unitOfWork.Regulators.GetByIdAsync(id);
                if (regulator == null)
                {
                    throw new KeyNotFoundException($"Regulator with ID {id} not found");
                }

                regulator.IsDeleted = true;
                regulator.DeletedAt = DateTime.UtcNow;
                regulator.ModifiedBy = GetCurrentUser();
                regulator.ModifiedDate = DateTime.UtcNow;

                await _unitOfWork.Regulators.UpdateAsync(regulator);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Regulator deleted with ID {Id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting regulator with ID {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<RegulatorDto>> GetByJurisdictionAsync(string jurisdiction)
        {
            try
            {
                var regulators = await _unitOfWork.Regulators.GetAllAsync();
                var filtered = regulators.Where(r => r.Jurisdiction == jurisdiction && !r.IsDeleted);
                return _mapper.Map<IEnumerable<RegulatorDto>>(filtered);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving regulators by jurisdiction {Jurisdiction}", jurisdiction);
                throw;
            }
        }

        private string GetCurrentUser()
        {
            return _httpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "System";
        }
    }
}
