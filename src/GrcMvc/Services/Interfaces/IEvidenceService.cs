using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GrcMvc.Models.DTOs;

namespace GrcMvc.Services.Interfaces
{
    public interface IEvidenceService
    {
        Task<IEnumerable<EvidenceDto>> GetAllAsync();
        Task<EvidenceDto?> GetByIdAsync(Guid id);
        Task<EvidenceDto> CreateAsync(CreateEvidenceDto createEvidenceDto);
        Task<EvidenceDto?> UpdateAsync(Guid id, UpdateEvidenceDto updateEvidenceDto);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<EvidenceDto>> GetByTypeAsync(string type);
        Task<IEnumerable<EvidenceDto>> GetByClassificationAsync(string classification);
        Task<IEnumerable<EvidenceDto>> GetExpiringEvidencesAsync(int days);
        Task<IEnumerable<EvidenceDto>> GetByAuditIdAsync(Guid auditId);
        Task<EvidenceStatisticsDto> GetStatisticsAsync();
    }
}
