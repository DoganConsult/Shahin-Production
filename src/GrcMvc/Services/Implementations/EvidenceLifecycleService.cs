using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Exceptions;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// PHASE 8: Evidence Lifecycle Service Implementation
    /// Handles evidence submission, review, scoring, and approval workflow
    /// </summary>
    public class EvidenceLifecycleService : IEvidenceLifecycleService
    {
        private readonly GrcDbContext _context;
        private readonly IWorkflowEngineService _workflowEngine;
        private readonly IAuditEventService _auditService;
        private readonly ILogger<EvidenceLifecycleService> _logger;

        // Evidence status constants
        private const string STATUS_DRAFT = "Draft";
        private const string STATUS_SUBMITTED = "Submitted";
        private const string STATUS_IN_REVIEW = "InReview";
        private const string STATUS_CHANGES_REQUESTED = "ChangesRequested";
        private const string STATUS_APPROVED = "Approved";
        private const string STATUS_REJECTED = "Rejected";

        // Minimum score to accept evidence
        private const int MIN_SCORE_TO_ACCEPT = 70;

        public EvidenceLifecycleService(
            GrcDbContext context,
            IWorkflowEngineService workflowEngine,
            IAuditEventService auditService,
            ILogger<EvidenceLifecycleService> logger)
        {
            _context = context;
            _workflowEngine = workflowEngine;
            _auditService = auditService;
            _logger = logger;
        }

        #region Evidence Submission

        public async Task<Evidence> SubmitEvidenceAsync(
            Guid tenantId,
            Guid assessmentRequirementId,
            string evidenceTypeCode,
            string title,
            string description,
            string filePath,
            string fileName,
            long fileSize,
            string mimeType,
            string submittedBy)
        {
            // Validate assessment requirement exists
            var requirement = await _context.AssessmentRequirements
                .Include(r => r.Assessment)
                .FirstOrDefaultAsync(r => r.Id == assessmentRequirementId);

            if (requirement == null)
                throw RequirementException.NotFound(assessmentRequirementId);

            var evidence = new Evidence
            {
                Id = Guid.NewGuid(),
                EvidenceNumber = GenerateEvidenceNumber(),
                Title = title,
                Description = description,
                Type = evidenceTypeCode,
                FilePath = filePath,
                FileName = fileName,
                FileSize = fileSize,
                MimeType = mimeType,
                CollectionDate = DateTime.UtcNow,
                CollectedBy = submittedBy,
                VerificationStatus = STATUS_DRAFT,
                AssessmentId = requirement.AssessmentId,
                // ControlId linked via Assessment
                CreatedDate = DateTime.UtcNow,
                CreatedBy = submittedBy
            };

            _context.Evidences.Add(evidence);
            await _context.SaveChangesAsync();

            // Log audit event
            await _auditService.LogEventAsync(
                tenantId: tenantId,
                eventType: "EvidenceSubmitted",
                affectedEntityType: "Evidence",
                affectedEntityId: evidence.Id.ToString(),
                action: "Submit",
                actor: submittedBy,
                payloadJson: System.Text.Json.JsonSerializer.Serialize(new
                {
                    EvidenceId = evidence.Id,
                    RequirementId = assessmentRequirementId,
                    Type = evidenceTypeCode,
                    FileName = fileName
                }),
                correlationId: null
            );

            _logger.LogInformation($"Evidence submitted: {evidence.Id} for requirement {assessmentRequirementId}");
            return evidence;
        }

        public async Task<Evidence?> GetEvidenceAsync(Guid evidenceId)
        {
            return await _context.Evidences
                .Include(e => e.Assessment)
                .Include(e => e.Control)
                .FirstOrDefaultAsync(e => e.Id == evidenceId && !e.IsDeleted);
        }

        public async Task<List<Evidence>> GetEvidenceByRequirementAsync(Guid assessmentRequirementId)
        {
            var requirement = await _context.AssessmentRequirements
                .FirstOrDefaultAsync(r => r.Id == assessmentRequirementId);

            if (requirement == null)
                return new List<Evidence>();

            return await _context.Evidences
                .Where(e => e.AssessmentId == requirement.AssessmentId && !e.IsDeleted)
                .OrderByDescending(e => e.CollectionDate)
                .ToListAsync();
        }

        public async Task<List<Evidence>> GetEvidenceByAssessmentAsync(Guid assessmentId)
        {
            return await _context.Evidences
                .Where(e => e.AssessmentId == assessmentId && !e.IsDeleted)
                .OrderByDescending(e => e.CollectionDate)
                .ToListAsync();
        }

        #endregion

        #region Review Workflow

        public async Task<WorkflowInstance> StartReviewWorkflowAsync(Guid evidenceId, string startedBy)
        {
            var evidence = await GetEvidenceAsync(evidenceId);
            if (evidence == null)
                throw new EntityNotFoundException("Evidence", evidenceId);

            // Find evidence review workflow definition
            var workflowDef = await _context.WorkflowDefinitions
                .FirstOrDefaultAsync(w => w.Category == "EvidenceReview" && w.Status == "Active");

            if (workflowDef == null)
                throw new EntityNotFoundException("WorkflowDefinition", "EvidenceReview (Active)");

            // Start workflow instance
            var tenantId = evidence.Assessment?.TenantId ?? Guid.Empty;
            var instance = await _workflowEngine.CreateWorkflowAsync(
                tenantId,
                workflowDef.Id,
                "Medium",
                startedBy);

            _logger.LogInformation($"Evidence review workflow started: {instance.Id} for evidence {evidenceId}");
            return instance;
        }

        public async Task<Evidence> SubmitForReviewAsync(Guid evidenceId, string submittedBy)
        {
            var evidence = await GetEvidenceAsync(evidenceId);
            if (evidence == null)
                throw new EntityNotFoundException("Evidence", evidenceId);

            if (evidence.VerificationStatus != STATUS_DRAFT &&
                evidence.VerificationStatus != STATUS_CHANGES_REQUESTED)
                throw new EvidenceException(evidenceId, $"Cannot submit evidence in '{evidence.VerificationStatus}' status. Must be Draft or ChangesRequested.");

            evidence.VerificationStatus = STATUS_SUBMITTED;
            evidence.ModifiedDate = DateTime.UtcNow;
            evidence.ModifiedBy = submittedBy;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Evidence submitted for review: {evidenceId}");
            return evidence;
        }

        public async Task<Evidence> RequestChangesAsync(Guid evidenceId, string comments, string requestedBy)
        {
            var evidence = await GetEvidenceAsync(evidenceId);
            if (evidence == null)
                throw new EntityNotFoundException("Evidence", evidenceId);

            if (evidence.VerificationStatus != STATUS_IN_REVIEW)
                throw new EvidenceException(evidenceId, $"Cannot request changes on evidence in '{evidence.VerificationStatus}' status. Must be InReview.");

            evidence.VerificationStatus = STATUS_CHANGES_REQUESTED;
            evidence.Comments = comments;
            evidence.ModifiedDate = DateTime.UtcNow;
            evidence.ModifiedBy = requestedBy;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Changes requested for evidence: {evidenceId}");
            return evidence;
        }

        public async Task<Evidence> ResubmitAsync(Guid evidenceId, string comments, string resubmittedBy)
        {
            var evidence = await GetEvidenceAsync(evidenceId);
            if (evidence == null)
                throw new EntityNotFoundException("Evidence", evidenceId);

            if (evidence.VerificationStatus != STATUS_CHANGES_REQUESTED)
                throw new EvidenceException(evidenceId, $"Cannot resubmit evidence in '{evidence.VerificationStatus}' status. Must be ChangesRequested.");

            evidence.VerificationStatus = STATUS_IN_REVIEW;
            evidence.Comments = comments;
            evidence.ModifiedDate = DateTime.UtcNow;
            evidence.ModifiedBy = resubmittedBy;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Evidence resubmitted: {evidenceId}");
            return evidence;
        }

        #endregion

        #region Scoring

        public async Task<EvidenceScore> ScoreEvidenceAsync(
            Guid evidenceId,
            int score,
            string scoringCriteria,
            string comments,
            string scoredBy)
        {
            if (score < 0 || score > 100)
                throw new ArgumentException("Score must be between 0 and 100.");

            var evidence = await GetEvidenceAsync(evidenceId);
            if (evidence == null)
                throw new EntityNotFoundException("Evidence", evidenceId);

            // Mark previous scores as not final
            var previousScores = await _context.Set<EvidenceScore>()
                .Where(s => s.EvidenceId == evidenceId && s.IsFinal)
                .ToListAsync();

            foreach (var prevScore in previousScores)
            {
                prevScore.IsFinal = false;
            }

            var evidenceScore = new EvidenceScore
            {
                Id = Guid.NewGuid(),
                EvidenceId = evidenceId,
                Score = score,
                ScoringCriteria = scoringCriteria,
                Comments = comments,
                ScoredBy = scoredBy,
                ScoredAt = DateTime.UtcNow,
                IsFinal = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = scoredBy
            };

            _context.Set<EvidenceScore>().Add(evidenceScore);

            // Update evidence status based on score
            if (evidence.VerificationStatus == STATUS_IN_REVIEW)
            {
                evidence.VerificationStatus = score >= MIN_SCORE_TO_ACCEPT ? STATUS_APPROVED : STATUS_CHANGES_REQUESTED;
                evidence.VerifiedBy = scoredBy;
                evidence.VerificationDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            // Update requirement status
            if (evidence.AssessmentId.HasValue)
            {
                var requirement = await _context.AssessmentRequirements
                    .FirstOrDefaultAsync(r => r.AssessmentId == evidence.AssessmentId);

                if (requirement != null)
                {
                    await UpdateRequirementStatusAsync(requirement.Id);
                }
            }

            _logger.LogInformation($"Evidence scored: {evidenceId} = {score}");
            return evidenceScore;
        }

        public async Task<EvidenceScore?> GetEvidenceScoreAsync(Guid evidenceId)
        {
            return await _context.Set<EvidenceScore>()
                .Where(s => s.EvidenceId == evidenceId && s.IsFinal)
                .FirstOrDefaultAsync();
        }

        public async Task<List<EvidenceScore>> GetScoringHistoryAsync(Guid evidenceId)
        {
            return await _context.Set<EvidenceScore>()
                .Where(s => s.EvidenceId == evidenceId)
                .OrderByDescending(s => s.ScoredAt)
                .ToListAsync();
        }

        #endregion

        #region Approval

        public async Task<Evidence> ApproveEvidenceAsync(Guid evidenceId, string comments, string approvedBy)
        {
            var evidence = await GetEvidenceAsync(evidenceId);
            if (evidence == null)
                throw new EntityNotFoundException("Evidence", evidenceId);

            if (evidence.VerificationStatus != STATUS_IN_REVIEW &&
                evidence.VerificationStatus != STATUS_SUBMITTED)
                throw new EvidenceException(evidenceId, $"Cannot approve evidence in '{evidence.VerificationStatus}' status. Must be InReview or Submitted.");

            evidence.VerificationStatus = STATUS_APPROVED;
            evidence.VerifiedBy = approvedBy;
            evidence.VerificationDate = DateTime.UtcNow;
            evidence.Comments = comments;
            evidence.ModifiedDate = DateTime.UtcNow;
            evidence.ModifiedBy = approvedBy;

            await _context.SaveChangesAsync();

            // Update requirement status
            if (evidence.AssessmentId.HasValue)
            {
                var requirement = await _context.AssessmentRequirements
                    .FirstOrDefaultAsync(r => r.AssessmentId == evidence.AssessmentId);

                if (requirement != null)
                {
                    await UpdateRequirementStatusAsync(requirement.Id);
                }
            }

            _logger.LogInformation($"Evidence approved: {evidenceId}");
            return evidence;
        }

        public async Task<Evidence> RejectEvidenceAsync(Guid evidenceId, string reason, string rejectedBy)
        {
            var evidence = await GetEvidenceAsync(evidenceId);
            if (evidence == null)
                throw new EntityNotFoundException("Evidence", evidenceId);

            if (evidence.VerificationStatus != STATUS_IN_REVIEW &&
                evidence.VerificationStatus != STATUS_SUBMITTED)
                throw new EvidenceException(evidenceId, $"Cannot reject evidence in '{evidence.VerificationStatus}' status. Must be InReview or Submitted.");

            evidence.VerificationStatus = STATUS_REJECTED;
            evidence.VerifiedBy = rejectedBy;
            evidence.VerificationDate = DateTime.UtcNow;
            evidence.Comments = reason;
            evidence.ModifiedDate = DateTime.UtcNow;
            evidence.ModifiedBy = rejectedBy;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Evidence rejected: {evidenceId}");
            return evidence;
        }

        #endregion

        #region Derived Status

        public async Task<AssessmentRequirement> UpdateRequirementStatusAsync(Guid assessmentRequirementId)
        {
            var requirement = await _context.AssessmentRequirements
                .FirstOrDefaultAsync(r => r.Id == assessmentRequirementId);

            if (requirement == null)
                throw RequirementException.NotFound(assessmentRequirementId);

            // Get all evidence for this requirement
            var evidences = await _context.Evidences
                .Where(e => e.AssessmentId == requirement.AssessmentId && !e.IsDeleted)
                .ToListAsync();

            // Get scores for approved evidence
            var approvedEvidenceIds = evidences
                .Where(e => e.VerificationStatus == STATUS_APPROVED)
                .Select(e => e.Id)
                .ToList();

            var scores = await _context.Set<EvidenceScore>()
                .Where(s => approvedEvidenceIds.Contains(s.EvidenceId) && s.IsFinal)
                .ToListAsync();

            // Calculate average score
            decimal avgScore = scores.Any() ? (decimal)scores.Average(s => s.Score) : 0;

            // Determine status based on score
            string status;
            if (!evidences.Any())
            {
                status = "NotStarted";
            }
            else if (!approvedEvidenceIds.Any())
            {
                status = "InProgress";
            }
            else if (avgScore >= 90)
            {
                status = "Compliant";
            }
            else if (avgScore >= 70)
            {
                status = "PartiallyCompliant";
            }
            else
            {
                status = "NonCompliant";
            }

            requirement.Status = status;
            requirement.Score = (int)avgScore;
            requirement.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Requirement status updated: {assessmentRequirementId} = {status} ({avgScore:F1}%)");
            return requirement;
        }

        public async Task<decimal> CalculateAssessmentScoreAsync(Guid assessmentId)
        {
            var requirements = await _context.AssessmentRequirements
                .Where(r => r.AssessmentId == assessmentId && !r.IsDeleted)
                .ToListAsync();

            if (!requirements.Any())
                return 0;

            var totalScore = requirements.Sum(r => r.Score ?? 0);
            var avgScore = totalScore / requirements.Count;

            // Update assessment
            var assessment = await _context.Assessments.FindAsync(assessmentId);
            if (assessment != null)
            {
                assessment.Score = avgScore;
                assessment.ModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return avgScore;
        }

        #endregion

        #region Statistics

        public async Task<EvidenceStatistics> GetStatisticsAsync(Guid tenantId)
        {
            var evidences = await _context.Evidences
                .Include(e => e.Assessment)
                .Where(e => e.Assessment != null &&
                    e.Assessment.TenantId == tenantId && !e.IsDeleted)
                .ToListAsync();

            var scores = await _context.Set<EvidenceScore>()
                .Where(s => s.IsFinal)
                .ToListAsync();

            var evidenceScores = scores
                .Where(s => evidences.Any(e => e.Id == s.EvidenceId))
                .ToList();

            // Calculate overdue reviews based on SLA (default: 5 days for review)
            const int SLA_REVIEW_DAYS = 5;
            var overdueCount = evidences.Count(e =>
                (e.VerificationStatus == STATUS_SUBMITTED || e.VerificationStatus == STATUS_IN_REVIEW) &&
                (DateTime.UtcNow - e.CollectionDate).TotalDays > SLA_REVIEW_DAYS);

            return new EvidenceStatistics
            {
                TotalEvidence = evidences.Count,
                Submitted = evidences.Count(e => e.VerificationStatus == STATUS_SUBMITTED),
                InReview = evidences.Count(e => e.VerificationStatus == STATUS_IN_REVIEW),
                Approved = evidences.Count(e => e.VerificationStatus == STATUS_APPROVED),
                Rejected = evidences.Count(e => e.VerificationStatus == STATUS_REJECTED),
                ChangesRequested = evidences.Count(e => e.VerificationStatus == STATUS_CHANGES_REQUESTED),
                AverageScore = evidenceScores.Any() ? (decimal)evidenceScores.Average(s => s.Score) : 0,
                PendingReview = evidences.Count(e => e.VerificationStatus == STATUS_SUBMITTED ||
                    e.VerificationStatus == STATUS_IN_REVIEW),
                OverdueReview = overdueCount
            };
        }

        #endregion

        #region Helpers

        private string GenerateEvidenceNumber()
        {
            return $"EV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()}";
        }

        #endregion
    }
}
