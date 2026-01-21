using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GrcMvc.Models.DTOs;

namespace GrcMvc.Services.Interfaces
{
    public interface IPolicyService
    {
        Task<IEnumerable<PolicyDto>> GetAllAsync();
        Task<PolicyDto?> GetByIdAsync(Guid id);
        Task<PolicyDto> CreateAsync(CreatePolicyDto createPolicyDto);
        Task<PolicyDto?> UpdateAsync(Guid id, UpdatePolicyDto updatePolicyDto);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<PolicyDto>> GetByCategoryAsync(string category);
        Task<IEnumerable<PolicyDto>> GetByStatusAsync(string status);
        Task<IEnumerable<PolicyDto>> GetExpiringPoliciesAsync(int days);
        Task<IEnumerable<PolicyViolationDto>> GetPolicyViolationsAsync(Guid id);
        Task<PolicyStatisticsDto> GetStatisticsAsync();
        Task<PolicyViolationDto?> AddViolationAsync(Guid policyId, CreatePolicyViolationDto createViolationDto);
        Task<IEnumerable<PolicyViolationDto>> GetViolationsByPolicyIdAsync(Guid policyId);
        Task<bool> ValidateComplianceAsync(Guid policyId);
        Task ApproveAsync(Guid id);
        Task PublishAsync(Guid id);
    }
}