using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Exceptions;
using GrcMvc.Models.DTOs;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    public class AssessmentExecutionService : IAssessmentExecutionService
    {
        private readonly GrcDbContext _context;
        private readonly IFileUploadService _fileUploadService;
        private readonly ILogger<AssessmentExecutionService> _logger;
        private readonly ITenantContextService _tenantContext;

        public AssessmentExecutionService(
            GrcDbContext context,
            IFileUploadService fileUploadService,
            ILogger<AssessmentExecutionService> logger,
            ITenantContextService tenantContext)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _fileUploadService = fileUploadService ?? throw new ArgumentNullException(nameof(fileUploadService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));

            // Validate tenant context
            if (!_tenantContext.HasTenantContext())
            {
                throw new InvalidOperationException("Tenant context is required for AssessmentExecutionService operations");
            }
        }

        public async Task<AssessmentExecutionViewModel?> GetExecutionViewModelAsync(Guid assessmentId, string languageCode = "en")
        {
            var assessment = await _context.Assessments
                .Include(a => a.Requirements)
                .FirstOrDefaultAsync(a => a.Id == assessmentId);

            if (assessment == null)
            {
                _logger.LogWarning("Assessment {AssessmentId} not found", assessmentId);
                return null;
            }

            var requirements = assessment.Requirements.Where(r => !r.IsDeleted).ToList();
            var isArabic = languageCode == "ar";

            // Get evidence counts per requirement
            var requirementIds = requirements.Select(r => r.Id).ToList();
            var evidenceCounts = await _context.Evidences
                .Where(e => e.AssessmentRequirementId != null && requirementIds.Contains(e.AssessmentRequirementId.Value) && !e.IsDeleted)
                .GroupBy(e => e.AssessmentRequirementId)
                .Select(g => new { RequirementId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.RequirementId ?? Guid.Empty, x => x.Count);

            // Get notes counts per requirement
            var notesCounts = await _context.RequirementNotes
                .Where(n => requirementIds.Contains(n.AssessmentRequirementId) && !n.IsDeleted)
                .GroupBy(n => n.AssessmentRequirementId)
                .Select(g => new { RequirementId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.RequirementId, x => x.Count);

            var viewModel = new AssessmentExecutionViewModel
            {
                Id = assessment.Id,
                AssessmentNumber = assessment.AssessmentNumber,
                Name = assessment.Name,
                FrameworkCode = assessment.FrameworkCode ?? string.Empty,
                FrameworkName = GetFrameworkName(assessment.FrameworkCode),
                TemplateCode = assessment.TemplateCode ?? string.Empty,
                Status = assessment.Status,
                DueDate = assessment.DueDate,
                AssignedTo = assessment.AssignedTo ?? string.Empty,
                TotalRequirements = requirements.Count,
                Requirements = requirements.Select(r => MapToRequirementCard(r, isArabic, evidenceCounts, notesCounts)).ToList()
            };

            // Calculate domain progress
            viewModel.Domains = requirements
                .GroupBy(r => r.Domain)
                .Select(g => new DomainProgressDto
                {
                    DomainCode = g.Key ?? "Other",
                    DomainName = g.Key ?? "Other",
                    TotalRequirements = g.Count(),
                    CompletedRequirements = g.Count(r => IsCompleted(r.Status)),
                    Progress = g.Count() > 0 ? (decimal)Math.Round((double)g.Count(r => IsCompleted(r.Status)) / g.Count() * 100, 1) : 0,
                    AverageScore = g.Any(r => r.Score.HasValue)
                        ? (decimal)g.Where(r => r.Score.HasValue).Average(r => (double)r.Score!.Value)
                        : 0
                }).OrderBy(d => d.DomainCode).ToList();

            // Calculate overall progress
            viewModel.CompletedRequirements = requirements.Count(r => IsCompleted(r.Status));
            viewModel.InProgressRequirements = requirements.Count(r => r.Status == "InProgress" || r.Status == "PartiallyCompliant");
            viewModel.PendingRequirements = requirements.Count(r => r.Status == "NotStarted");
            viewModel.OverallProgress = requirements.Count > 0
                ? (decimal)Math.Round((double)viewModel.CompletedRequirements / requirements.Count * 100, 1)
                : 0;
            var scoredRequirements = requirements.Where(r => r.Score.HasValue).ToList();
            viewModel.OverallScore = scoredRequirements.Count > 0
                ? (decimal)Math.Round(scoredRequirements.Average(r => (double)r.Score!.Value), 1)
                : 0;

            return viewModel;
        }

        public async Task<AssessmentProgressDto> CalculateProgressAsync(Guid assessmentId)
        {
            var requirements = await _context.AssessmentRequirements
                .Where(r => r.AssessmentId == assessmentId && !r.IsDeleted)
                .ToListAsync();

            var result = new AssessmentProgressDto
            {
                AssessmentId = assessmentId,
                TotalRequirements = requirements.Count,
                CompletedRequirements = requirements.Count(r => IsCompleted(r.Status)),
                InProgressRequirements = requirements.Count(r => r.Status == "InProgress" || r.Status == "PartiallyCompliant"),
                PendingRequirements = requirements.Count(r => r.Status == "NotStarted")
            };

            result.OverallProgress = requirements.Count > 0
                ? (decimal)Math.Round((double)result.CompletedRequirements / requirements.Count * 100, 1)
                : 0;
            var scoredReqs = requirements.Where(r => r.Score.HasValue).ToList();
            result.OverallScore = scoredReqs.Count > 0
                ? (decimal)Math.Round(scoredReqs.Average(r => (double)r.Score!.Value), 1)
                : 0;

            result.DomainProgress = requirements
                .GroupBy(r => r.Domain)
                .Select(g => new DomainProgressDto
                {
                    DomainCode = g.Key ?? "Other",
                    DomainName = g.Key ?? "Other",
                    TotalRequirements = g.Count(),
                    CompletedRequirements = g.Count(r => IsCompleted(r.Status)),
                    Progress = g.Count() > 0 ? (decimal)Math.Round((double)g.Count(r => IsCompleted(r.Status)) / g.Count() * 100, 1) : 0,
                    AverageScore = g.Any(r => r.Score.HasValue)
                        ? (decimal)g.Where(r => r.Score.HasValue).Average(r => (double)r.Score!.Value)
                        : 0
                }).OrderBy(d => d.DomainCode).ToList();

            return result;
        }

        public async Task<DomainProgressDto?> CalculateDomainProgressAsync(Guid assessmentId, string domain)
        {
            var requirements = await _context.AssessmentRequirements
                .Where(r => r.AssessmentId == assessmentId && r.Domain == domain && !r.IsDeleted)
                .ToListAsync();

            if (!requirements.Any())
                return null;

            var domainScoredReqs = requirements.Where(r => r.Score.HasValue).ToList();
            return new DomainProgressDto
            {
                DomainCode = domain,
                DomainName = domain,
                TotalRequirements = requirements.Count,
                CompletedRequirements = requirements.Count(r => IsCompleted(r.Status)),
                Progress = (decimal)Math.Round((double)requirements.Count(r => IsCompleted(r.Status)) / requirements.Count * 100, 1),
                AverageScore = domainScoredReqs.Count > 0
                    ? (decimal)domainScoredReqs.Average(r => (double)r.Score!.Value)
                    : 0
            };
        }

        // Valid status values for requirements
        private static readonly HashSet<string> ValidStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "NotStarted", "InProgress", "Compliant", "PartiallyCompliant", "NonCompliant", "NotApplicable"
        };

        public async Task<RequirementCardDto> UpdateStatusAsync(Guid requirementId, string status, string updatedBy)
        {
            var requirement = await _context.AssessmentRequirements.FindAsync(requirementId);
            if (requirement == null)
            {
                _logger.LogWarning("Requirement {RequirementId} not found for status update", requirementId);
                return null!; // Caller should check for null
            }

            // MEDIUM FIX: Validate status value
            if (!ValidStatuses.Contains(status))
                throw new ArgumentException($"Invalid status '{status}'. Valid values: {string.Join(", ", ValidStatuses)}");

            requirement.Status = status;
            requirement.ModifiedBy = updatedBy;
            requirement.ModifiedDate = DateTime.UtcNow;

            if (IsCompleted(status) && requirement.CompletedDate == null)
            {
                requirement.CompletedDate = DateTime.UtcNow;
            }
            else if (!IsCompleted(status))
            {
                requirement.CompletedDate = null;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Requirement {RequirementId} status updated to {Status} by {User}",
                requirementId, status, updatedBy);

            return MapToRequirementCard(requirement, false, new Dictionary<Guid, int>(), new Dictionary<Guid, int>());
        }

        public async Task<RequirementCardDto> UpdateScoreAsync(Guid requirementId, int score, string rationale, string scoredBy)
        {
            var requirement = await _context.AssessmentRequirements.FindAsync(requirementId);
            if (requirement == null)
            {
                _logger.LogWarning("Requirement {RequirementId} not found for score update", requirementId);
                return null!; // Caller should check for null
            }

            // MEDIUM FIX: Validate score range (0-100)
            if (score < 0 || score > 100)
                throw new ArgumentOutOfRangeException(nameof(score), $"Score must be between 0 and 100. Received: {score}");

            requirement.Score = score;
            requirement.ScoreRationale = rationale;
            requirement.ScoredBy = scoredBy;
            requirement.ScoredAt = DateTime.UtcNow;
            requirement.ModifiedDate = DateTime.UtcNow;

            // Auto-set status based on score if not already completed
            if (requirement.Status == "NotStarted" || requirement.Status == "InProgress")
            {
                requirement.Status = score >= 80 ? "Compliant"
                    : score >= 50 ? "PartiallyCompliant"
                    : "NonCompliant";

                if (IsCompleted(requirement.Status))
                {
                    requirement.CompletedDate = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Requirement {RequirementId} score updated to {Score} by {User}",
                requirementId, score, scoredBy);

            return MapToRequirementCard(requirement, false, new Dictionary<Guid, int>(), new Dictionary<Guid, int>());
        }

        public async Task<RequirementNoteDto> AddNoteAsync(AddRequirementNoteRequest request, string createdBy)
        {
            var requirement = await _context.AssessmentRequirements.FindAsync(request.RequirementId);
            if (requirement == null)
            {
                _logger.LogWarning("Requirement {RequirementId} not found for adding note", request.RequirementId);
                return null!; // Caller should check for null
            }

            var note = new RequirementNote
            {
                Id = Guid.NewGuid(),
                AssessmentRequirementId = request.RequirementId,
                Content = request.Content,
                NoteType = request.NoteType,
                IsInternal = request.IsInternal,
                CreatedBy = createdBy,
                CreatedDate = DateTime.UtcNow,
                TenantId = requirement.TenantId
            };

            _context.RequirementNotes.Add(note);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Note added to requirement {RequirementId} by {User}", request.RequirementId, createdBy);

            return new RequirementNoteDto
            {
                Id = note.Id,
                Content = note.Content,
                NoteType = note.NoteType,
                CreatedBy = note.CreatedBy ?? "System",
                CreatedDate = note.CreatedDate,
                IsInternal = note.IsInternal
            };
        }

        public async Task<List<RequirementNoteDto>> GetNotesHistoryAsync(Guid requirementId)
        {
            return await _context.RequirementNotes
                .Where(n => n.AssessmentRequirementId == requirementId && !n.IsDeleted)
                .OrderByDescending(n => n.CreatedDate)
                .Select(n => new RequirementNoteDto
                {
                    Id = n.Id,
                    Content = n.Content,
                    NoteType = n.NoteType,
                    CreatedBy = n.CreatedBy ?? "System",
                    CreatedDate = n.CreatedDate,
                    IsInternal = n.IsInternal
                })
                .ToListAsync();
        }

        public async Task DeleteNoteAsync(Guid noteId, string deletedBy)
        {
            var note = await _context.RequirementNotes.FindAsync(noteId);
            if (note == null)
            {
                _logger.LogWarning("Note {NoteId} not found for deletion", noteId);
                return; // Idempotent delete
            }

            note.IsDeleted = true;
            note.DeletedAt = DateTime.UtcNow;
            note.ModifiedBy = deletedBy;
            note.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Note {NoteId} deleted by {User}", noteId, deletedBy);
        }

        // Maximum file size for evidence uploads (50MB)
        private const long MaxEvidenceFileSize = 50 * 1024 * 1024;

        public async Task<Evidence> AttachEvidenceAsync(Guid requirementId, IFormFile file, string title, string description, string uploadedBy)
        {
            // MEDIUM FIX: Validate title and description
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Evidence title is required", nameof(title));
            if (title.Length > 200)
                throw new ArgumentException("Evidence title cannot exceed 200 characters", nameof(title));
            if (description?.Length > 2000)
                throw new ArgumentException("Evidence description cannot exceed 2000 characters", nameof(description));

            // MEDIUM FIX: Validate file size before processing
            if (file.Length > MaxEvidenceFileSize)
                throw new ArgumentException($"File size ({file.Length / 1024 / 1024}MB) exceeds maximum allowed ({MaxEvidenceFileSize / 1024 / 1024}MB)", nameof(file));

            var requirement = await _context.AssessmentRequirements
                .Include(r => r.Assessment)
                .FirstOrDefaultAsync(r => r.Id == requirementId);

            if (requirement == null)
            {
                _logger.LogWarning("Requirement {RequirementId} not found for evidence attachment", requirementId);
                return null!; // Caller should check for null
            }

            var validation = await _fileUploadService.ValidateFileAsync(file);
            if (!validation.IsValid)
                throw new ValidationException("File", validation.ErrorMessage ?? "File validation failed");

            var uploadResult = await _fileUploadService.UploadFileAsync(file, "evidence");
            if (!uploadResult.Success)
                throw new IntegrationException("FileUpload", uploadResult.ErrorMessage ?? "File upload failed");

            var evidence = new Evidence
            {
                Id = Guid.NewGuid(),
                TenantId = requirement.TenantId,
                AssessmentId = requirement.AssessmentId,
                AssessmentRequirementId = requirementId,
                EvidenceNumber = $"EV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                Title = title,
                Description = description,
                FileName = uploadResult.FileName ?? file.FileName,
                FilePath = uploadResult.FilePath ?? string.Empty,
                FileSize = file.Length,
                MimeType = file.ContentType,
                Type = "Document",
                CollectionDate = DateTime.UtcNow,
                CollectedBy = uploadedBy,
                VerificationStatus = "Pending",
                CreatedBy = uploadedBy,
                CreatedDate = DateTime.UtcNow
            };

            _context.Evidences.Add(evidence);

            // Update requirement evidence status
            requirement.EvidenceStatus = "Submitted";
            requirement.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Evidence {EvidenceId} attached to requirement {RequirementId} by {User}",
                evidence.Id, requirementId, uploadedBy);

            return evidence;
        }

        public async Task<List<EvidenceListItemDto>> GetRequirementEvidenceAsync(Guid requirementId)
        {
            return await _context.Evidences
                .Where(e => e.AssessmentRequirementId == requirementId && !e.IsDeleted)
                .OrderByDescending(e => e.CreatedDate)
                .Select(e => new EvidenceListItemDto
                {
                    Id = e.Id,
                    Name = e.Title ?? e.FileName ?? "",
                    Type = e.Type ?? "",
                    LinkedItemId = e.AssessmentRequirementId.ToString() ?? "",
                    UploadedBy = e.CreatedBy ?? "",
                    UploadedDate = e.CreatedDate,
                    FileSize = e.FileSize.ToString()
                })
                .ToListAsync();
        }

        public async Task<int> GetEvidenceCountAsync(Guid requirementId)
        {
            return await _context.Evidences
                .CountAsync(e => e.AssessmentRequirementId == requirementId && !e.IsDeleted);
        }

        public async Task<RequirementCardDto?> GetRequirementCardAsync(Guid requirementId, string languageCode = "en")
        {
            var requirement = await _context.AssessmentRequirements
                .FirstOrDefaultAsync(r => r.Id == requirementId && !r.IsDeleted);

            if (requirement == null)
                return null;

            var isArabic = languageCode == "ar";
            var evidenceCount = await GetEvidenceCountAsync(requirementId);
            var notesCount = await _context.RequirementNotes
                .CountAsync(n => n.AssessmentRequirementId == requirementId && !n.IsDeleted);

            var evidenceCounts = new Dictionary<Guid, int> { { requirementId, evidenceCount } };
            var notesCounts = new Dictionary<Guid, int> { { requirementId, notesCount } };

            return MapToRequirementCard(requirement, isArabic, evidenceCounts, notesCounts);
        }

        public async Task<List<RequirementCardDto>> GetRequirementsByDomainAsync(Guid assessmentId, string domain, string languageCode = "en")
        {
            var requirements = await _context.AssessmentRequirements
                .Where(r => r.AssessmentId == assessmentId && r.Domain == domain && !r.IsDeleted)
                .OrderBy(r => r.ControlNumber)
                .ToListAsync();

            var isArabic = languageCode == "ar";
            var requirementIds = requirements.Select(r => r.Id).ToList();

            var evidenceCounts = await _context.Evidences
                .Where(e => e.AssessmentRequirementId != null && requirementIds.Contains(e.AssessmentRequirementId.Value) && !e.IsDeleted)
                .GroupBy(e => e.AssessmentRequirementId)
                .Select(g => new { RequirementId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.RequirementId ?? Guid.Empty, x => x.Count);

            var notesCounts = await _context.RequirementNotes
                .Where(n => requirementIds.Contains(n.AssessmentRequirementId) && !n.IsDeleted)
                .GroupBy(n => n.AssessmentRequirementId)
                .Select(g => new { RequirementId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.RequirementId, x => x.Count);

            return requirements.Select(r => MapToRequirementCard(r, isArabic, evidenceCounts, notesCounts)).ToList();
        }

        private static bool IsCompleted(string status)
        {
            return status == "Compliant" || status == "NonCompliant" || status == "NotApplicable";
        }

        private static string GetFrameworkName(string? frameworkCode)
        {
            if (string.IsNullOrEmpty(frameworkCode))
                return "Unknown Framework";

            return frameworkCode.ToUpperInvariant() switch
            {
                "NCA_ECC" or "NCA-ECC" => "NCA Essential Cybersecurity Controls",
                "PDPL" => "Personal Data Protection Law",
                "SAMA_CSF" or "SAMA-CSF" => "SAMA Cyber Security Framework",
                "SAMA_AML" or "SAMA-AML" => "SAMA AML/CFT Guidelines",
                "SFDA" => "SFDA Regulatory Framework",
                "ISO27001" => "ISO 27001",
                "NIST_CSF" or "NIST-CSF" => "NIST Cybersecurity Framework",
                _ => frameworkCode
            };
        }

        private static RequirementCardDto MapToRequirementCard(
            AssessmentRequirement r,
            bool isArabic,
            Dictionary<Guid, int> evidenceCounts,
            Dictionary<Guid, int> notesCounts)
        {
            return new RequirementCardDto
            {
                Id = r.Id,
                ControlNumber = r.ControlNumber,
                ControlTitle = isArabic && !string.IsNullOrEmpty(r.ControlTitleAr) ? r.ControlTitleAr : r.ControlTitle,
                ControlTitleAr = r.ControlTitleAr,
                RequirementText = isArabic && !string.IsNullOrEmpty(r.RequirementTextAr) ? r.RequirementTextAr : r.RequirementText,
                RequirementTextAr = r.RequirementTextAr,
                Domain = r.Domain,
                ControlType = r.ControlType,
                MaturityLevel = r.MaturityLevel,
                ImplementationGuidance = isArabic && !string.IsNullOrEmpty(r.ImplementationGuidanceAr) ? r.ImplementationGuidanceAr : r.ImplementationGuidance,
                ImplementationGuidanceAr = r.ImplementationGuidanceAr,
                ToolkitReference = r.ToolkitReference,
                SampleEvidenceDescription = r.SampleEvidenceDescription,
                BestPractices = r.BestPractices,
                CommonGaps = r.CommonGaps,
                ScoringGuideJson = r.ScoringGuideJson,
                WeightPercentage = r.WeightPercentage,
                IsAutoScorable = r.IsAutoScorable,
                Status = r.Status,
                EvidenceStatus = r.EvidenceStatus,
                Score = r.Score,
                MaxScore = r.MaxScore ?? 100,
                ScoreRationale = r.ScoreRationale,
                DueDate = r.DueDate,
                CompletedDate = r.CompletedDate,
                EvidenceCount = evidenceCounts.GetValueOrDefault(r.Id, 0),
                NotesCount = notesCounts.GetValueOrDefault(r.Id, 0)
            };
        }

        /// <inheritdoc />
        public async Task<Assessment> CreateAssessmentAsync(Guid tenantId, string name, Guid? templateId, string createdBy)
        {
            var assessment = new Assessment
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = name,
                Status = "Draft",
                CreatedDate = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            _context.Assessments.Add(assessment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Assessment {Id} created for tenant {TenantId}", assessment.Id, tenantId);
            return assessment;
        }

        /// <inheritdoc />
        public async Task<List<Assessment>> GetAssessmentsAsync(Guid tenantId)
        {
            return await _context.Assessments
                .Where(a => a.TenantId == tenantId)
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<Assessment?> GetAssessmentAsync(Guid id)
        {
            return await _context.Assessments.FirstOrDefaultAsync(a => a.Id == id);
        }

        /// <inheritdoc />
        public async Task<Assessment> StartAssessmentAsync(Guid id, string userId)
        {
            var assessment = await _context.Assessments.FirstOrDefaultAsync(a => a.Id == id);
            if (assessment == null)
                throw new InvalidOperationException("Assessment not found");

            assessment.Status = "InProgress";
            assessment.StartDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Assessment {Id} started by {UserId}", id, userId);
            return assessment;
        }

        /// <inheritdoc />
        public async Task<Assessment> CompleteAssessmentAsync(Guid id, string userId)
        {
            var assessment = await _context.Assessments.FirstOrDefaultAsync(a => a.Id == id);
            if (assessment == null)
                throw new InvalidOperationException("Assessment not found");

            assessment.Status = "Completed";

            await _context.SaveChangesAsync();
            _logger.LogInformation("Assessment {Id} completed by {UserId}", id, userId);
            return assessment;
        }

        /// <inheritdoc />
        public async Task<List<AssessmentRequirement>> GetRequirementsAsync(Guid assessmentId)
        {
            return await _context.AssessmentRequirements
                .Where(r => r.AssessmentId == assessmentId && !r.IsDeleted)
                .OrderBy(r => r.Domain)
                .ThenBy(r => r.ControlNumber)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<AssessmentRequirement> UpdateRequirementStatusAsync(Guid requirementId, string status, string? notes)
        {
            var requirement = await _context.AssessmentRequirements.FirstOrDefaultAsync(r => r.Id == requirementId);
            if (requirement == null)
                throw new EntityNotFoundException("AssessmentRequirement", requirementId);

            requirement.Status = status;

            if (!string.IsNullOrEmpty(notes))
            {
                var note = new RequirementNote
                {
                    Id = Guid.NewGuid(),
                    AssessmentRequirementId = requirementId,
                    Content = notes,
                    NoteType = "StatusChange",
                    CreatedDate = DateTime.UtcNow
                };
                _context.RequirementNotes.Add(note);
            }

            await _context.SaveChangesAsync();
            return requirement;
        }
    }
}
