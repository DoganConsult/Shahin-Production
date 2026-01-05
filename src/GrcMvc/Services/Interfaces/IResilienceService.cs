using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GrcMvc.Models.Dtos;
using GrcMvc.Models.Entities;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Service interface for Resilience assessments
    /// </summary>
    public interface IResilienceService
    {
        // Operational Resilience
        Task<Resilience> CreateResilienceAsync(Guid tenantId, CreateResilienceDto input);
        Task<Resilience> UpdateResilienceAsync(Guid tenantId, Guid id, UpdateResilienceDto input);
        Task<ResilienceDto> GetResilienceAsync(Guid tenantId, Guid id);
        Task<List<ResilienceDto>> GetResiliencesAsync(Guid tenantId, int page = 1, int pageSize = 20);
        Task<bool> DeleteResilienceAsync(Guid tenantId, Guid id);
        Task<Resilience> AssessResilienceAsync(Guid tenantId, Guid id, ResilienceAssessmentRequestDto? request = null);

        // Risk Resilience
        Task<RiskResilience> CreateRiskResilienceAsync(Guid tenantId, CreateRiskResilienceDto input);
        Task<RiskResilienceDto> GetRiskResilienceAsync(Guid tenantId, Guid id);
        Task<List<RiskResilienceDto>> GetRiskResiliencesAsync(Guid tenantId, int page = 1, int pageSize = 20);
        Task<RiskResilience> AssessRiskResilienceAsync(Guid tenantId, Guid id);
    }
}
