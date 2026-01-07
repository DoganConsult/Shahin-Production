using GrcMvc.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrcMvc.Services.Interfaces
{
    public interface IActionPlanService
    {
        Task<ActionPlanDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<ActionPlanDto>> GetAllAsync();
        Task<ActionPlanDto> CreateAsync(CreateActionPlanDto dto);
        Task<ActionPlanDto> UpdateAsync(Guid id, UpdateActionPlanDto dto);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<ActionPlanDto>> GetByStatusAsync(string status);
        Task CloseAsync(Guid id);
    }
}
