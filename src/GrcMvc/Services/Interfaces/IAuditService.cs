using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GrcMvc.Models.DTOs;

namespace GrcMvc.Services.Interfaces
{
    public interface IAuditService
    {
        Task<IEnumerable<AuditDto>> GetAllAsync();
        Task<AuditDto?> GetByIdAsync(Guid id);
        Task<AuditDto> CreateAsync(CreateAuditDto createAuditDto);
        Task<AuditDto?> UpdateAsync(Guid id, UpdateAuditDto updateAuditDto);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<AuditDto>> GetByTypeAsync(string type);
        Task<IEnumerable<AuditDto>> GetUpcomingAuditsAsync(int days);
        Task<IEnumerable<AuditFindingDto>> GetAuditFindingsAsync(Guid id);
        Task<AuditStatisticsDto> GetStatisticsAsync();
        Task<AuditFindingDto?> AddFindingAsync(Guid auditId, CreateAuditFindingDto createFindingDto);
        Task<bool> ValidateAuditScopeAsync(Guid auditId);
    }
}