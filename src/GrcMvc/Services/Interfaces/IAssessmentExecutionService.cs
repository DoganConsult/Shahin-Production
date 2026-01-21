using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GrcMvc.Models.DTOs;
using GrcMvc.Models.Entities;
using Microsoft.AspNetCore.Http;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Service for assessment execution operations - handling individual requirement
    /// status updates, scoring, evidence uploads, and notes.
    /// </summary>
    public interface IAssessmentExecutionService
    {
        /// <summary>
        /// Get complete execution view model for an assessment including all requirements,
        /// domain progress, evidence counts, and notes counts.
        /// </summary>
        /// <param name="assessmentId">Assessment ID</param>
        /// <param name="languageCode">Language code for bilingual content (en/ar)</param>
        Task<AssessmentExecutionViewModel?> GetExecutionViewModelAsync(Guid assessmentId, string languageCode = "en");

        /// <summary>
        /// Calculate and return progress statistics for an assessment.
        /// </summary>
        Task<AssessmentProgressDto> CalculateProgressAsync(Guid assessmentId);

        /// <summary>
        /// Calculate progress for a specific domain within an assessment.
        /// </summary>
        Task<DomainProgressDto?> CalculateDomainProgressAsync(Guid assessmentId, string domain);

        /// <summary>
        /// Update the status of a requirement.
        /// Valid statuses: NotStarted, InProgress, Compliant, PartiallyCompliant, NonCompliant, NotApplicable
        /// </summary>
        Task<RequirementCardDto> UpdateStatusAsync(Guid requirementId, string status, string updatedBy);

        /// <summary>
        /// Update the score of a requirement.
        /// Auto-sets status based on score if not already evaluated (>=80 Compliant, >=50 Partial, <50 NonCompliant).
        /// </summary>
        Task<RequirementCardDto> UpdateScoreAsync(Guid requirementId, int score, string rationale, string scoredBy);

        /// <summary>
        /// Add a note to a requirement.
        /// </summary>
        Task<RequirementNoteDto> AddNoteAsync(AddRequirementNoteRequest request, string createdBy);

        /// <summary>
        /// Get all notes for a requirement in descending order by date.
        /// </summary>
        Task<List<RequirementNoteDto>> GetNotesHistoryAsync(Guid requirementId);

        /// <summary>
        /// Delete a note.
        /// </summary>
        Task DeleteNoteAsync(Guid noteId, string deletedBy);

        /// <summary>
        /// Attach evidence to a specific requirement.
        /// </summary>
        Task<Evidence> AttachEvidenceAsync(Guid requirementId, IFormFile file, string title, string description, string uploadedBy);

        /// <summary>
        /// Get all evidence for a requirement.
        /// </summary>
        Task<List<EvidenceListItemDto>> GetRequirementEvidenceAsync(Guid requirementId);

        /// <summary>
        /// Get evidence count for a requirement.
        /// </summary>
        Task<int> GetEvidenceCountAsync(Guid requirementId);

        /// <summary>
        /// Get a single requirement card by ID.
        /// </summary>
        Task<RequirementCardDto?> GetRequirementCardAsync(Guid requirementId, string languageCode = "en");

        /// <summary>
        /// Get all requirements for a domain within an assessment.
        /// </summary>
        Task<List<RequirementCardDto>> GetRequirementsByDomainAsync(Guid assessmentId, string domain, string languageCode = "en");

        /// <summary>
        /// Get all assessments for a tenant.
        /// </summary>
        Task<List<Assessment>> GetAssessmentsAsync(Guid tenantId);

        /// <summary>
        /// Get an assessment by ID.
        /// </summary>
        Task<Assessment?> GetAssessmentAsync(Guid id);

        /// <summary>
        /// Create a new assessment for a tenant.
        /// </summary>
        Task<Assessment> CreateAssessmentAsync(Guid tenantId, string name, Guid? templateId, string createdBy);

        /// <summary>
        /// Start an assessment.
        /// </summary>
        Task<Assessment> StartAssessmentAsync(Guid id, string userId);

        /// <summary>
        /// Complete an assessment.
        /// </summary>
        Task<Assessment> CompleteAssessmentAsync(Guid id, string userId);

        /// <summary>
        /// Get all requirements for an assessment.
        /// </summary>
        Task<List<AssessmentRequirement>> GetRequirementsAsync(Guid assessmentId);

        /// <summary>
        /// Update requirement status with notes.
        /// </summary>
        Task<AssessmentRequirement> UpdateRequirementStatusAsync(Guid requirementId, string status, string? notes);
    }
}
