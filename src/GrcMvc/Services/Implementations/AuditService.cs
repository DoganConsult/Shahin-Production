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
    public class AuditService : IAuditService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<AuditService> _logger;
        private readonly PolicyEnforcementHelper _policyHelper;
        private readonly IWorkspaceContextService? _workspaceContext;

        public AuditService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<AuditService> logger,
            PolicyEnforcementHelper policyHelper,
            IWorkspaceContextService? workspaceContext = null)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _policyHelper = policyHelper ?? throw new ArgumentNullException(nameof(policyHelper));
            _workspaceContext = workspaceContext;
        }

        public async Task<IEnumerable<AuditDto>> GetAllAsync()
        {
            try
            {
                var audits = await _unitOfWork.Audits.GetAllAsync();
                return _mapper.Map<IEnumerable<AuditDto>>(audits);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all audits");
                throw;
            }
        }

        public async Task<AuditDto?> GetByIdAsync(Guid id)
        {
            try
            {
                var audit = await _unitOfWork.Audits.GetByIdAsync(id);
                return audit == null ? null : _mapper.Map<AuditDto>(audit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting audit with ID {AuditId}", id);
                throw;
            }
        }

        public async Task<AuditDto> CreateAsync(CreateAuditDto createAuditDto)
        {
            try
            {
                var audit = _mapper.Map<Audit>(createAuditDto);
                audit.Id = Guid.NewGuid();
                audit.CreatedDate = DateTime.UtcNow;
                audit.Status = "Planned";
                audit.AuditCode = GenerateAuditCode();

                // Set workspace context if available
                if (_workspaceContext != null && _workspaceContext.HasWorkspaceContext())
                {
                    audit.WorkspaceId = _workspaceContext.GetCurrentWorkspaceId();
                }

                // Enforce policies before saving
                await _policyHelper.EnforceCreateAsync(
                    resourceType: "Audit",
                    resource: audit,
                    dataClassification: null, // Will be set to "internal" by helper if null
                    owner: null); // Will be set to current user by helper if null

                await _unitOfWork.Audits.AddAsync(audit);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Audit created with ID {AuditId}", audit.Id);
                return _mapper.Map<AuditDto>(audit);
            }
            catch (PolicyViolationException pve)
            {
                _logger.LogWarning("Policy violation prevented audit creation: {Message}. Rule: {RuleId}",
                    pve.Message, pve.RuleId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating audit");
                throw;
            }
        }

        public async Task<AuditDto?> UpdateAsync(Guid id, UpdateAuditDto updateAuditDto)
        {
            try
            {
                var audit = await _unitOfWork.Audits.GetByIdAsync(id);
                if (audit == null)
                {
                    _logger.LogWarning("Audit with ID {AuditId} not found for update", id);
                    return null;
                }

                _mapper.Map(updateAuditDto, audit);
                audit.ModifiedDate = DateTime.UtcNow;

                await _unitOfWork.Audits.UpdateAsync(audit);
                await _unitOfWork.SaveChangesAsync();

                return audit == null ? null : _mapper.Map<AuditDto>(audit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating audit with ID {AuditId}", id);
                throw;
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            try
            {
                var audit = await _unitOfWork.Audits.GetByIdAsync(id);
                if (audit == null)
                {
                    _logger.LogWarning("Audit with ID {AuditId} not found for deletion", id);
                    return;
                }

                await _unitOfWork.Audits.DeleteAsync(audit);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Audit with ID {AuditId} deleted", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting audit with ID {AuditId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<AuditDto>> GetUpcomingAuditsAsync(int days = 30)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(days);
                var audits = await _unitOfWork.Audits.FindAsync(
                    a => a.PlannedStartDate <= cutoffDate &&
                         (a.Status == "Planned" || a.Status == "In Progress"));

                return _mapper.Map<IEnumerable<AuditDto>>(
                    audits.OrderBy(a => a.PlannedStartDate));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting upcoming audits");
                throw;
            }
        }

        public async Task<IEnumerable<AuditDto>> GetByStatusAsync(string status)
        {
            try
            {
                var audits = await _unitOfWork.Audits.FindAsync(a => a.Status == status);
                return _mapper.Map<IEnumerable<AuditDto>>(audits);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting audits by status {Status}", status);
                throw;
            }
        }

        public async Task<AuditStatisticsDto> GetStatisticsAsync()
        {
            try
            {
                var audits = await _unitOfWork.Audits.GetAllAsync();
                var auditsList = audits.ToList();
                var findings = await _unitOfWork.AuditFindings.GetAllAsync();
                var findingsList = findings.ToList();

                return new AuditStatisticsDto
                {
                    TotalAudits = auditsList.Count,
                    PlannedAudits = auditsList.Count(a => a.Status == "Planned"),
                    InProgressAudits = auditsList.Count(a => a.Status == "In Progress"),
                    CompletedAudits = auditsList.Count(a => a.Status == "Completed"),
                    TotalFindings = findingsList.Count,
                    CriticalFindings = findingsList.Count(f => f.Severity == "Critical"),
                    HighFindings = findingsList.Count(f => f.Severity == "High"),
                    MediumFindings = findingsList.Count(f => f.Severity == "Medium"),
                    LowFindings = findingsList.Count(f => f.Severity == "Low"),
                    AuditsByType = auditsList.GroupBy(a => a.Type)
                        .ToDictionary(g => g.Key, g => g.Count())
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting audit statistics");
                throw;
            }
        }

        public async Task<AuditFindingDto?> AddFindingAsync(Guid auditId, CreateAuditFindingDto createAuditFindingDto)
        {
            try
            {
                var audit = await _unitOfWork.Audits.GetByIdAsync(auditId);
                if (audit == null)
                {
                    _logger.LogWarning("Audit with ID {AuditId} not found", auditId);
                    return null;
                }

                var finding = _mapper.Map<AuditFinding>(createAuditFindingDto);
                finding.Id = Guid.NewGuid();
                finding.AuditId = auditId;
                finding.CreatedDate = DateTime.UtcNow;
                finding.Status = "Open";
                finding.FindingCode = GenerateFindingCode();

                await _unitOfWork.AuditFindings.AddAsync(finding);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Finding created with ID {FindingId} for audit {AuditId}",
                    finding.Id, auditId);
                return _mapper.Map<AuditFindingDto>(finding);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding finding to audit {AuditId}", auditId);
                throw;
            }
        }

        public async Task<IEnumerable<AuditFindingDto>> GetFindingsByAuditIdAsync(Guid auditId)
        {
            try
            {
                var findings = await _unitOfWork.AuditFindings.FindAsync(f => f.AuditId == auditId);
                return _mapper.Map<IEnumerable<AuditFindingDto>>(findings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting findings for audit {AuditId}", auditId);
                throw;
            }
        }

        public async Task<IEnumerable<AuditDto>> GetByTypeAsync(string type)
        {
            try
            {
                var audits = await _unitOfWork.Audits.FindAsync(a => a.Type.Equals(type));
                return _mapper.Map<IEnumerable<AuditDto>>(audits);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting audits by type {Type}", type);
                throw;
            }
        }

        public async Task<IEnumerable<AuditFindingDto>> GetAuditFindingsAsync(Guid id)
        {
            try
            {
                var findings = await _unitOfWork.AuditFindings.FindAsync(f => f.AuditId == id);
                return _mapper.Map<IEnumerable<AuditFindingDto>>(findings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting findings for audit ID {AuditId}", id);
                throw;
            }
        }

        public async Task<bool> ValidateAuditScopeAsync(Guid auditId)
        {
            try
            {
                var audit = await _unitOfWork.Audits.GetByIdAsync(auditId);
                return audit != null && !string.IsNullOrEmpty(audit.Scope);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating audit scope for ID {AuditId}", auditId);
                throw;
            }
        }

        private string GenerateAuditCode()
        {
            return $"AUD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";
        }

        private string GenerateFindingCode()
        {
            return $"FIND-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";
        }

        public async Task CloseAsync(Guid id)
        {
            try
            {
                var audit = await _unitOfWork.Audits.GetByIdAsync(id);
                if (audit == null)
                {
                    _logger.LogWarning("Audit with ID {AuditId} not found for closing", id);
                    return;
                }

                audit.Status = "Closed";
                audit.ModifiedDate = DateTime.UtcNow;

                await _unitOfWork.Audits.UpdateAsync(audit);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Audit with ID {AuditId} closed", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing audit with ID {AuditId}", id);
                throw;
            }
        }
    }
}