using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GrcMvc.Data;
using GrcMvc.Models.DTOs;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using GrcMvc.Application.Policy;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    public class ControlService : IControlService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ControlService> _logger;
        private readonly PolicyEnforcementHelper _policyHelper;
        private readonly IWorkspaceContextService? _workspaceContext;

        public ControlService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<ControlService> logger,
            PolicyEnforcementHelper policyHelper,
            IWorkspaceContextService? workspaceContext = null)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _policyHelper = policyHelper ?? throw new ArgumentNullException(nameof(policyHelper));
            _workspaceContext = workspaceContext;
        }

        public async Task<IEnumerable<ControlDto>> GetAllAsync()
        {
            try
            {
                var controls = await _unitOfWork.Controls.GetAllAsync();
                return _mapper.Map<IEnumerable<ControlDto>>(controls);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all controls");
                throw;
            }
        }

        public async Task<ControlDto> GetByIdAsync(Guid id)
        {
            try
            {
                var control = await _unitOfWork.Controls.GetByIdAsync(id);
                if (control == null)
                {
                    _logger.LogWarning("Control with ID {ControlId} not found", id);
                    return null;
                }
                return _mapper.Map<ControlDto>(control);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting control with ID {ControlId}", id);
                throw;
            }
        }

        public async Task<ControlDto> CreateAsync(CreateControlDto createControlDto)
        {
            try
            {
                var control = _mapper.Map<Control>(createControlDto);
                control.Id = Guid.NewGuid();
                control.CreatedDate = DateTime.UtcNow;
                control.ControlCode = GenerateControlCode();
                
                // Set workspace context if available
                if (_workspaceContext != null && _workspaceContext.HasWorkspaceContext())
                {
                    control.WorkspaceId = _workspaceContext.GetCurrentWorkspaceId();
                }

                // Enforce policies before saving
                await _policyHelper.EnforceCreateAsync(
                    resourceType: "Control",
                    resource: control,
                    dataClassification: null, // Will be set to "internal" by helper if null
                    owner: null); // Will be set to current user by helper if null

                await _unitOfWork.Controls.AddAsync(control);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Control created with ID {ControlId}", control.Id);
                return _mapper.Map<ControlDto>(control);
            }
            catch (PolicyViolationException pve)
            {
                _logger.LogWarning("Policy violation prevented control creation: {Message}. Rule: {RuleId}",
                    pve.Message, pve.RuleId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating control");
                throw;
            }
        }

        public async Task<ControlDto> UpdateAsync(Guid id, UpdateControlDto updateControlDto)
        {
            try
            {
                var control = await _unitOfWork.Controls.GetByIdAsync(id);
                if (control == null)
                {
                    _logger.LogWarning("Control with ID {ControlId} not found for update", id);
                    return null;
                }

                _mapper.Map(updateControlDto, control);
                control.ModifiedDate = DateTime.UtcNow;

                await _unitOfWork.Controls.UpdateAsync(control);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Control with ID {ControlId} updated", id);
                return _mapper.Map<ControlDto>(control);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating control with ID {ControlId}", id);
                throw;
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            try
            {
                var control = await _unitOfWork.Controls.GetByIdAsync(id);
                if (control == null)
                {
                    _logger.LogWarning("Control with ID {ControlId} not found for deletion", id);
                    return;
                }

                await _unitOfWork.Controls.DeleteAsync(control);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Control with ID {ControlId} deleted", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting control with ID {ControlId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<ControlDto>> GetByRiskIdAsync(Guid riskId)
        {
            try
            {
                var controls = await _unitOfWork.Controls.FindAsync(c => c.RiskId == riskId);
                return _mapper.Map<IEnumerable<ControlDto>>(controls);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting controls for risk ID {RiskId}", riskId);
                throw;
            }
        }

        public async Task<ControlStatisticsDto> GetStatisticsAsync()
        {
            try
            {
                var controls = await _unitOfWork.Controls.GetAllAsync();
                var controlsList = controls.ToList();

                return new ControlStatisticsDto
                {
                    TotalControls = controlsList.Count,
                    EffectiveControls = controlsList.Count(c => c.Effectiveness >= 80),
                    IneffectiveControls = controlsList.Count(c => c.Effectiveness < 50),
                    ControlsByType = controlsList.GroupBy(c => c.Type)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    AverageEffectiveness = controlsList.Any() ? controlsList.Average(c => c.Effectiveness) : 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting control statistics");
                throw;
            }
        }

        private string GenerateControlCode()
        {
            return $"CTRL-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";
        }
    }
}