using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GrcMvc.Models.DTOs;
using GrcMvc.Models.Entities;

namespace GrcMvc.Services.Interfaces
{
    public interface IAssessmentService
    {
        Task<IEnumerable<AssessmentDto>> GetAllAsync();
        Task<AssessmentDto?> GetByIdAsync(Guid id);
        Task<AssessmentDto> CreateAsync(CreateAssessmentDto createAssessmentDto);
        Task<AssessmentDto?> UpdateAsync(Guid id, UpdateAssessmentDto updateAssessmentDto);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<AssessmentDto>> GetByControlIdAsync(Guid controlId);
        Task<IEnumerable<AssessmentDto>> GetUpcomingAssessmentsAsync(int days = 30);
        Task<AssessmentStatisticsDto> GetStatisticsAsync();

        /// <summary>
        /// Generate assessments from a Plan based on derived templates.
        /// Creates Assessment instances with AssessmentRequirements linked to framework controls.
        /// </summary>
        Task<List<Assessment>> GenerateAssessmentsFromPlanAsync(Guid planId, string createdBy);

        /// <summary>
        /// Get assessments for a specific plan.
        /// </summary>
        Task<IEnumerable<Assessment>> GetAssessmentsByPlanAsync(Guid planId);

        /// <summary>
        /// Submit an assessment for review/approval
        /// </summary>
        Task<AssessmentDto> SubmitAsync(Guid id);

        /// <summary>
        /// Approve an assessment
        /// </summary>
        Task<AssessmentDto> ApproveAsync(Guid id);

        /// <summary>
        /// Reject an assessment (return to draft)
        /// </summary>
        Task<AssessmentDto> RejectAsync(Guid id, string reason);

        /// <summary>
        /// Cancel an assessment
        /// </summary>
        Task<AssessmentDto> CancelAsync(Guid id, string reason);

        /// <summary>
        /// Start review of a submitted assessment
        /// </summary>
        Task<AssessmentDto> StartReviewAsync(Guid id);

        /// <summary>
        /// Complete an approved assessment
        /// </summary>
        Task<AssessmentDto> CompleteAsync(Guid id);

        /// <summary>
        /// Archive a completed assessment
        /// </summary>
        Task<AssessmentDto> ArchiveAsync(Guid id);
    }
}