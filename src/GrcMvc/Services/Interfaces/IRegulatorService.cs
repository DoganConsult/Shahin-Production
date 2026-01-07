using GrcMvc.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrcMvc.Services.Interfaces
{
    public interface IRegulatorService
    {
        Task<RegulatorDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<RegulatorDto>> GetAllAsync();
        Task<RegulatorDto> CreateAsync(CreateRegulatorDto dto);
        Task<RegulatorDto> UpdateAsync(Guid id, UpdateRegulatorDto dto);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<RegulatorDto>> GetByJurisdictionAsync(string jurisdiction);
    }
}
