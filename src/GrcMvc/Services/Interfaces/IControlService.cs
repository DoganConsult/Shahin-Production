using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GrcMvc.Models.DTOs;

namespace GrcMvc.Services.Interfaces
{
    public interface IControlService
    {
        Task<IEnumerable<ControlDto>> GetAllAsync();
        Task<ControlDto> GetByIdAsync(Guid id);
        Task<ControlDto> CreateAsync(CreateControlDto createControlDto);
        Task<ControlDto> UpdateAsync(Guid id, UpdateControlDto updateControlDto);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<ControlDto>> GetByRiskIdAsync(Guid riskId);
        Task<ControlStatisticsDto> GetStatisticsAsync();
    }
}