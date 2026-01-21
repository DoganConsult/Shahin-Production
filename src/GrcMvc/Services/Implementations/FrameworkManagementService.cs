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
    public class FrameworkManagementService : IFrameworkManagementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<FrameworkManagementService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly PolicyEnforcementHelper _policyHelper;
        private readonly IWorkspaceContextService? _workspaceContext;

        public FrameworkManagementService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<FrameworkManagementService> logger,
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

        public async Task<FrameworkDto?> GetByIdAsync(Guid id)
        {
            try
            {
                var framework = await _unitOfWork.Frameworks.GetByIdAsync(id);
                if (framework == null)
                {
                    _logger.LogWarning("Framework with ID {Id} not found", id);
                    return null;
                }
                return _mapper.Map<FrameworkDto>(framework);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving framework with ID {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<FrameworkDto>> GetAllAsync()
        {
            try
            {
                var frameworks = await _unitOfWork.Frameworks.GetAllAsync();
                return _mapper.Map<IEnumerable<FrameworkDto>>(frameworks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all frameworks");
                throw;
            }
        }

        public async Task<FrameworkDto> CreateAsync(CreateFrameworkDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            try
            {
                var framework = _mapper.Map<GrcMvc.Models.Entities.Framework>(dto);
                
                if (_workspaceContext != null && _workspaceContext.HasWorkspaceContext())
                {
                    framework.WorkspaceId = _workspaceContext.GetCurrentWorkspaceId();
                }

                await _policyHelper.EnforceCreateAsync(
                    resourceType: "RegulatoryFramework",
                    resource: framework,
                    dataClassification: framework.DataClassification,
                    owner: framework.Owner);

                framework.CreatedBy = GetCurrentUser();
                framework.CreatedDate = DateTime.UtcNow;

                await _unitOfWork.Frameworks.AddAsync(framework);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Framework created with ID {Id}", framework.Id);
                return _mapper.Map<FrameworkDto>(framework);
            }
            catch (PolicyViolationException pve)
            {
                _logger.LogWarning(pve, "Policy violation creating framework");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating framework");
                throw;
            }
        }

        public async Task<FrameworkDto> UpdateAsync(Guid id, UpdateFrameworkDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            try
            {
                var framework = await _unitOfWork.Frameworks.GetByIdAsync(id);
                if (framework == null)
                {
                    _logger.LogWarning("Framework with ID {Id} not found for update", id);
                    return null!; // Caller should check for null
                }

                _mapper.Map(dto, framework);

                await _policyHelper.EnforceUpdateAsync(
                    resourceType: "RegulatoryFramework",
                    resource: framework,
                    dataClassification: framework.DataClassification,
                    owner: framework.Owner);

                framework.ModifiedBy = GetCurrentUser();
                framework.ModifiedDate = DateTime.UtcNow;

                await _unitOfWork.Frameworks.UpdateAsync(framework);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Framework updated with ID {Id}", id);
                return _mapper.Map<FrameworkDto>(framework);
            }
            catch (PolicyViolationException pve)
            {
                _logger.LogWarning(pve, "Policy violation updating framework");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating framework with ID {Id}", id);
                throw;
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            try
            {
                var framework = await _unitOfWork.Frameworks.GetByIdAsync(id);
                if (framework == null)
                {
                    _logger.LogWarning("Framework with ID {Id} not found for deletion", id);
                    return; // Idempotent delete - already gone
                }

                framework.IsDeleted = true;
                framework.DeletedAt = DateTime.UtcNow;
                framework.ModifiedBy = GetCurrentUser();
                framework.ModifiedDate = DateTime.UtcNow;

                await _unitOfWork.Frameworks.UpdateAsync(framework);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Framework deleted with ID {Id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting framework with ID {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<FrameworkDto>> GetByJurisdictionAsync(string jurisdiction)
        {
            try
            {
                var frameworks = await _unitOfWork.Frameworks.GetAllAsync();
                var filtered = frameworks.Where(f => f.Jurisdiction == jurisdiction && !f.IsDeleted);
                return _mapper.Map<IEnumerable<FrameworkDto>>(filtered);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving frameworks by jurisdiction {Jurisdiction}", jurisdiction);
                throw;
            }
        }

        private string GetCurrentUser()
        {
            return _httpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "System";
        }
    }
}
