using GrcMvc.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrcMvc.Services.Interfaces
{
    public interface IRiskService
    {
        Task<RiskDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<RiskDto>> GetAllAsync();
        Task<RiskDto> CreateAsync(CreateRiskDto dto);
        Task<RiskDto> UpdateAsync(Guid id, UpdateRiskDto dto);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<RiskDto>> GetByStatusAsync(string status);
        Task<IEnumerable<RiskDto>> GetByCategoryAsync(string category);
        Task<RiskStatisticsDto> GetStatisticsAsync();
    }
}