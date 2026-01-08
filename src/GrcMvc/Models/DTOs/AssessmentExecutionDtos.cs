using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GrcMvc.Models.DTOs
{
    /// <summary>
    /// Main view model for assessment execution page
    /// </summary>
    public class AssessmentExecutionViewModel
    {
        public Guid Id { get; set; }
        public string AssessmentNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string FrameworkCode { get; set; } = string.Empty;
        public string FrameworkName { get; set; } = string.Empty;
        public string TemplateCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal OverallProgress { get; set; }
        public decimal OverallScore { get; set; }
        public int TotalRequirements { get; set; }
        public int CompletedRequirements { get; set; }
        public int InProgressRequirements { get; set; }
        public int PendingRequirements { get; set; }
        public DateTime? DueDate { get; set; }
        public string AssignedTo { get; set; } = string.Empty;
        public List<DomainProgressDto> Domains { get; set; } = new();
        public List<RequirementCardDto> Requirements { get; set; } = new();
    }

    /// <summary>
    /// Progress tracking per domain/section
    /// </summary>
    public class DomainProgressDto
    {
        public string DomainCode { get; set; } = string.Empty;
        public string DomainName { get; set; } = string.Empty;
        public int TotalRequirements { get; set; }
        public int CompletedRequirements { get; set; }
        public decimal Progress { get; set; }
        public decimal AverageScore { get; set; }
        public bool IsExpanded { get; set; } = false;
    }

    /// <summary>
    /// Individual requirement card data
    /// </summary>
    public class RequirementCardDto
    {
        public Guid Id { get; set; }
        public string ControlNumber { get; set; } = string.Empty;
        public string ControlTitle { get; set; } = string.Empty;
        public string ControlTitleAr { get; set; } = string.Empty;
        public string RequirementText { get; set; } = string.Empty;
        public string RequirementTextAr { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public string ControlType { get; set; } = string.Empty;
        public string MaturityLevel { get; set; } = string.Empty;

        // Guidance
        public string ImplementationGuidance { get; set; } = string.Empty;
        public string ImplementationGuidanceAr { get; set; } = string.Empty;
        public string ToolkitReference { get; set; } = string.Empty;
        public string SampleEvidenceDescription { get; set; } = string.Empty;
        public string BestPractices { get; set; } = string.Empty;
        public string CommonGaps { get; set; } = string.Empty;

        // Scoring
        public string ScoringGuideJson { get; set; } = "[]";
        public int WeightPercentage { get; set; } = 100;
        public bool IsAutoScorable { get; set; }

        // Current status and score
        public string Status { get; set; } = "NotStarted";
        public string EvidenceStatus { get; set; } = "Pending";
        public int? Score { get; set; }
        public int MaxScore { get; set; } = 100;
        public string ScoreRationale { get; set; } = string.Empty;

        // Counts
        public int EvidenceCount { get; set; }
        public int NotesCount { get; set; }

        // Dates
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedDate { get; set; }

        // Nested data (optional, for detail view)
        public List<EvidenceListItemDto> Evidences { get; set; } = new();
        public List<RequirementNoteDto> Notes { get; set; } = new();
    }

    /// <summary>
    /// Progress summary for an assessment
    /// </summary>
    public class AssessmentProgressDto
    {
        public Guid AssessmentId { get; set; }
        public decimal OverallProgress { get; set; }
        public decimal OverallScore { get; set; }
        public int CompletedRequirements { get; set; }
        public int InProgressRequirements { get; set; }
        public int PendingRequirements { get; set; }
        public int TotalRequirements { get; set; }
        public List<DomainProgressDto> DomainProgress { get; set; } = new();
    }

    /// <summary>
    /// Note/comment on a requirement
    /// </summary>
    public class RequirementNoteDto
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string NoteType { get; set; } = "General";
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public bool IsInternal { get; set; }
    }

    /// <summary>
    /// Request to update requirement status
    /// MEDIUM FIX: Added validation attributes
    /// </summary>
    public class UpdateRequirementStatusRequest
    {
        [Required(ErrorMessage = "Status is required")]
        [RegularExpression("^(NotStarted|InProgress|Compliant|PartiallyCompliant|NonCompliant|NotApplicable)$",
            ErrorMessage = "Invalid status value")]
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request to update requirement score
    /// MEDIUM FIX: Added validation attributes
    /// </summary>
    public class UpdateRequirementScoreRequest
    {
        [Required]
        [Range(0, 100, ErrorMessage = "Score must be between 0 and 100")]
        public int Score { get; set; }

        [MaxLength(2000, ErrorMessage = "Rationale cannot exceed 2000 characters")]
        public string ScoreRationale { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request to add a note to a requirement
    /// MEDIUM FIX: Added validation attributes
    /// </summary>
    public class AddRequirementNoteRequest
    {
        [Required]
        public Guid RequirementId { get; set; }

        [Required(ErrorMessage = "Note content is required")]
        [MaxLength(4000, ErrorMessage = "Note content cannot exceed 4000 characters")]
        public string Content { get; set; } = string.Empty;

        [RegularExpression("^(General|Action|Finding|Observation|Question)$", ErrorMessage = "Invalid note type")]
        public string NoteType { get; set; } = "General";

        public bool IsInternal { get; set; } = true;
    }

    /// <summary>
    /// Request to upload evidence to a requirement
    /// MEDIUM FIX: Added validation attributes
    /// </summary>
    public class UploadEvidenceToRequirementRequest
    {
        [Required]
        public Guid RequirementId { get; set; }

        [Required(ErrorMessage = "Evidence title is required")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string Description { get; set; } = string.Empty;

        [RegularExpression("^(Document|Screenshot|Log|Report|Certificate|Other)$", ErrorMessage = "Invalid evidence type")]
        public string EvidenceType { get; set; } = "Document";
    }

    /// <summary>
    /// Scoring guide entry for display
    /// </summary>
    public class ScoringGuideEntry
    {
        public int Score { get; set; }
        public string Label { get; set; } = string.Empty;
        public string Criteria { get; set; } = string.Empty;
    }
}
