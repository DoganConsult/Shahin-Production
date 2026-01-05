using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GrcMvc.Models.DTOs;

namespace GrcMvc.Services.Interfaces
{
    public interface IWorkflowService
    {
        Task<IEnumerable<WorkflowDto>> GetAllAsync();
        Task<WorkflowDto?> GetByIdAsync(Guid id);
        Task<WorkflowDto> CreateAsync(CreateWorkflowDto createWorkflowDto);
        Task<WorkflowDto?> UpdateAsync(Guid id, UpdateWorkflowDto updateWorkflowDto);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<WorkflowDto>> GetByCategoryAsync(string category);
        Task<IEnumerable<WorkflowDto>> GetByStatusAsync(string status);
        Task<IEnumerable<WorkflowDto>> GetOverdueWorkflowsAsync();
        Task<IEnumerable<WorkflowExecutionDto>> GetWorkflowExecutionsAsync(Guid id);
        Task<WorkflowExecutionDto?> ExecuteWorkflowAsync(Guid id);
        Task<WorkflowStatisticsDto> GetStatisticsAsync();
    }
}