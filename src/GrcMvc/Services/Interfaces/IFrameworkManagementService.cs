using GrcMvc.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrcMvc.Services.Interfaces
{
    public interface IFrameworkManagementService
    {
        Task<FrameworkDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<FrameworkDto>> GetAllAsync();
        Task<FrameworkDto> CreateAsync(CreateFrameworkDto dto);
        Task<FrameworkDto> UpdateAsync(Guid id, UpdateFrameworkDto dto);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<FrameworkDto>> GetByJurisdictionAsync(string jurisdiction);
    }
}
