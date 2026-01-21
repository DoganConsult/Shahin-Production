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
    public class ActionPlanService : IActionPlanService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ActionPlanService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly PolicyEnforcementHelper _policyHelper;
        private readonly IWorkspaceContextService? _workspaceContext;

        public ActionPlanService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<ActionPlanService> logger,
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

        public async Task<ActionPlanDto?> GetByIdAsync(Guid id)
        {
            try
            {
                var actionPlan = await _unitOfWork.ActionPlans.GetByIdAsync(id);
                if (actionPlan == null)
                {
                    _logger.LogWarning("ActionPlan with ID {Id} not found", id);
                    return null;
                }
                return _mapper.Map<ActionPlanDto>(actionPlan);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving action plan with ID {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<ActionPlanDto>> GetAllAsync()
        {
            try
            {
                var actionPlans = await _unitOfWork.ActionPlans.GetAllAsync();
                return _mapper.Map<IEnumerable<ActionPlanDto>>(actionPlans);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all action plans");
                throw;
            }
        }

        public async Task<ActionPlanDto> CreateAsync(CreateActionPlanDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            try
            {
                var actionPlan = _mapper.Map<ActionPlan>(dto);
                
                if (_workspaceContext != null && _workspaceContext.HasWorkspaceContext())
                {
                    actionPlan.WorkspaceId = _workspaceContext.GetCurrentWorkspaceId();
                }

                // Enforce policies before saving
                await _policyHelper.EnforceCreateAsync(
                    resourceType: "ActionPlan",
                    resource: actionPlan,
                    dataClassification: actionPlan.DataClassification,
                    owner: actionPlan.Owner);

                actionPlan.CreatedBy = GetCurrentUser();
                actionPlan.CreatedDate = DateTime.UtcNow;

                await _unitOfWork.ActionPlans.AddAsync(actionPlan);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Action plan created with ID {Id}", actionPlan.Id);
                return _mapper.Map<ActionPlanDto>(actionPlan);
            }
            catch (PolicyViolationException pve)
            {
                _logger.LogWarning(pve, "Policy violation creating action plan");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating action plan");
                throw;
            }
        }

        public async Task<ActionPlanDto> UpdateAsync(Guid id, UpdateActionPlanDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            try
            {
                var actionPlan = await _unitOfWork.ActionPlans.GetByIdAsync(id);
                if (actionPlan == null)
                {
                    _logger.LogWarning("Action plan with ID {Id} not found for update", id);
                    return null!; // Caller should check for null
                }

                _mapper.Map(dto, actionPlan);

                // Enforce policies before updating
                await _policyHelper.EnforceUpdateAsync(
                    resourceType: "ActionPlan",
                    resource: actionPlan,
                    dataClassification: actionPlan.DataClassification,
                    owner: actionPlan.Owner);

                actionPlan.ModifiedBy = GetCurrentUser();
                actionPlan.ModifiedDate = DateTime.UtcNow;

                await _unitOfWork.ActionPlans.UpdateAsync(actionPlan);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Action plan updated with ID {Id}", id);
                return _mapper.Map<ActionPlanDto>(actionPlan);
            }
            catch (PolicyViolationException pve)
            {
                _logger.LogWarning(pve, "Policy violation updating action plan");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating action plan with ID {Id}", id);
                throw;
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            try
            {
                var actionPlan = await _unitOfWork.ActionPlans.GetByIdAsync(id);
                if (actionPlan == null)
                {
                    _logger.LogWarning("Action plan with ID {Id} not found for deletion", id);
                    return; // Idempotent delete - already gone
                }

                actionPlan.IsDeleted = true;
                actionPlan.DeletedAt = DateTime.UtcNow;
                actionPlan.ModifiedBy = GetCurrentUser();
                actionPlan.ModifiedDate = DateTime.UtcNow;

                await _unitOfWork.ActionPlans.UpdateAsync(actionPlan);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Action plan deleted with ID {Id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting action plan with ID {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<ActionPlanDto>> GetByStatusAsync(string status)
        {
            try
            {
                var actionPlans = await _unitOfWork.ActionPlans.GetAllAsync();
                var filtered = actionPlans.Where(ap => ap.Status == status && !ap.IsDeleted);
                return _mapper.Map<IEnumerable<ActionPlanDto>>(filtered);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving action plans by status {Status}", status);
                throw;
            }
        }

        public async Task CloseAsync(Guid id)
        {
            try
            {
                var actionPlan = await _unitOfWork.ActionPlans.GetByIdAsync(id);
                if (actionPlan == null)
                {
                    _logger.LogWarning("Action plan with ID {Id} not found for closing", id);
                    return; // Cannot close non-existent plan
                }

                actionPlan.Status = "Completed";
                actionPlan.CompletedDate = DateTime.UtcNow;
                actionPlan.ModifiedBy = GetCurrentUser();
                actionPlan.ModifiedDate = DateTime.UtcNow;

                await _unitOfWork.ActionPlans.UpdateAsync(actionPlan);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Action plan closed with ID {Id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing action plan with ID {Id}", id);
                throw;
            }
        }

        private string GetCurrentUser()
        {
            return _httpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "System";
        }
    }
}
