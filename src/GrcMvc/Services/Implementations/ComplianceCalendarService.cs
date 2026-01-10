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
    public class ComplianceCalendarService : IComplianceCalendarService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ComplianceCalendarService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly PolicyEnforcementHelper _policyHelper;
        private readonly IWorkspaceContextService? _workspaceContext;

        public ComplianceCalendarService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<ComplianceCalendarService> logger,
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

        public async Task<ComplianceEventDto?> GetByIdAsync(Guid id)
        {
            try
            {
                var complianceEvent = await _unitOfWork.ComplianceEvents.GetByIdAsync(id);
                if (complianceEvent == null)
                {
                    _logger.LogWarning("ComplianceEvent with ID {Id} not found", id);
                    return null;
                }
                return _mapper.Map<ComplianceEventDto>(complianceEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving compliance event with ID {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<ComplianceEventDto>> GetAllAsync()
        {
            try
            {
                var complianceEvents = await _unitOfWork.ComplianceEvents.GetAllAsync();
                return _mapper.Map<IEnumerable<ComplianceEventDto>>(complianceEvents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all compliance events");
                throw;
            }
        }

        public async Task<ComplianceEventDto> CreateAsync(CreateComplianceEventDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            try
            {
                var complianceEvent = _mapper.Map<ComplianceEvent>(dto);
                
                if (_workspaceContext != null && _workspaceContext.HasWorkspaceContext())
                {
                    complianceEvent.WorkspaceId = _workspaceContext.GetCurrentWorkspaceId();
                }

                await _policyHelper.EnforceCreateAsync(
                    resourceType: "ComplianceEvent",
                    resource: complianceEvent,
                    dataClassification: complianceEvent.DataClassification,
                    owner: complianceEvent.Owner);

                complianceEvent.CreatedBy = GetCurrentUser();
                complianceEvent.CreatedDate = DateTime.UtcNow;

                await _unitOfWork.ComplianceEvents.AddAsync(complianceEvent);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Compliance event created with ID {Id}", complianceEvent.Id);
                return _mapper.Map<ComplianceEventDto>(complianceEvent);
            }
            catch (PolicyViolationException pve)
            {
                _logger.LogWarning(pve, "Policy violation creating compliance event");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating compliance event");
                throw;
            }
        }

        public async Task<ComplianceEventDto> UpdateAsync(Guid id, UpdateComplianceEventDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            try
            {
                var complianceEvent = await _unitOfWork.ComplianceEvents.GetByIdAsync(id);
                if (complianceEvent == null)
                {
                    _logger.LogWarning("Compliance event with ID {Id} not found for update", id);
                    return null!; // Caller should check for null
                }

                _mapper.Map(dto, complianceEvent);

                await _policyHelper.EnforceUpdateAsync(
                    resourceType: "ComplianceEvent",
                    resource: complianceEvent,
                    dataClassification: complianceEvent.DataClassification,
                    owner: complianceEvent.Owner);

                complianceEvent.ModifiedBy = GetCurrentUser();
                complianceEvent.ModifiedDate = DateTime.UtcNow;

                await _unitOfWork.ComplianceEvents.UpdateAsync(complianceEvent);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Compliance event updated with ID {Id}", id);
                return _mapper.Map<ComplianceEventDto>(complianceEvent);
            }
            catch (PolicyViolationException pve)
            {
                _logger.LogWarning(pve, "Policy violation updating compliance event");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating compliance event with ID {Id}", id);
                throw;
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            try
            {
                var complianceEvent = await _unitOfWork.ComplianceEvents.GetByIdAsync(id);
                if (complianceEvent == null)
                {
                    _logger.LogWarning("Compliance event with ID {Id} not found for deletion", id);
                    return; // Idempotent delete - already gone
                }

                complianceEvent.IsDeleted = true;
                complianceEvent.DeletedAt = DateTime.UtcNow;
                complianceEvent.ModifiedBy = GetCurrentUser();
                complianceEvent.ModifiedDate = DateTime.UtcNow;

                await _unitOfWork.ComplianceEvents.UpdateAsync(complianceEvent);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Compliance event deleted with ID {Id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting compliance event with ID {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<ComplianceEventDto>> GetUpcomingEventsAsync(int days)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(days);
                var complianceEvents = await _unitOfWork.ComplianceEvents.GetAllAsync();
                var filtered = complianceEvents
                    .Where(e => e.EventDate <= cutoffDate && e.EventDate >= DateTime.UtcNow && !e.IsDeleted)
                    .OrderBy(e => e.EventDate);
                return _mapper.Map<IEnumerable<ComplianceEventDto>>(filtered);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving upcoming compliance events");
                throw;
            }
        }

        public async Task<IEnumerable<ComplianceEventDto>> GetByStatusAsync(string status)
        {
            try
            {
                var complianceEvents = await _unitOfWork.ComplianceEvents.GetAllAsync();
                var filtered = complianceEvents.Where(e => e.Status == status && !e.IsDeleted);
                return _mapper.Map<IEnumerable<ComplianceEventDto>>(filtered);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving compliance events by status {Status}", status);
                throw;
            }
        }

        private string GetCurrentUser()
        {
            return _httpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "System";
        }
    }
}
