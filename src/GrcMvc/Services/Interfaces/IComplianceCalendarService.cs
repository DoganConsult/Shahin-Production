using GrcMvc.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrcMvc.Services.Interfaces
{
    public interface IComplianceCalendarService
    {
        Task<ComplianceEventDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<ComplianceEventDto>> GetAllAsync();
        Task<ComplianceEventDto> CreateAsync(CreateComplianceEventDto dto);
        Task<ComplianceEventDto> UpdateAsync(Guid id, UpdateComplianceEventDto dto);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<ComplianceEventDto>> GetUpcomingEventsAsync(int days);
        Task<IEnumerable<ComplianceEventDto>> GetByStatusAsync(string status);
    }
}
