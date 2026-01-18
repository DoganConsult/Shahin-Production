using GrcMvc.Common.Results;
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
        Task<Result<VendorDto>> CreateAsync(CreateVendorDto dto);
        Task<Result<VendorDto>> UpdateAsync(Guid id, UpdateVendorDto dto);
        Task<Result> DeleteAsync(Guid id);
        Task<IEnumerable<VendorDto>> GetByStatusAsync(string status);
        Task<Result> AssessAsync(Guid id);
    }
}
