using GrcMvc.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrcMvc.Services.Interfaces
{
    public interface IVendorService
    {
        Task<VendorDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<VendorDto>> GetAllAsync();
        Task<VendorDto> CreateAsync(CreateVendorDto dto);
        Task<VendorDto> UpdateAsync(Guid id, UpdateVendorDto dto);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<VendorDto>> GetByStatusAsync(string status);
        Task AssessAsync(Guid id);
    }
}
