using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Models.DTOs;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Service for handling evidence submission and retrieval
    /// Database-backed implementation with full CRUD operations
    /// </summary>
    public class EvidenceService : IEvidenceService
    {
        private readonly GrcDbContext _context;
        private readonly ILogger<EvidenceService> _logger;

        public EvidenceService(GrcDbContext context, ILogger<EvidenceService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all evidences from database
        /// </summary>
        public async Task<IEnumerable<EvidenceDto>> GetAllAsync()
        {
            try
            {
                var evidences = await _context.Evidences
                    .AsNoTracking()
                    .Select(e => MapToDto(e))
                    .ToListAsync();

                _logger.LogInformation($"Retrieved {evidences.Count} evidences from database");
                return evidences;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting all evidences: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get evidence by ID from database
        /// </summary>
        public async Task<EvidenceDto?> GetByIdAsync(Guid id)
        {
            try
            {
                var evidence = await _context.Evidences
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (evidence == null)
                {
                    _logger.LogWarning($"Evidence with ID {id} not found");
                    return null;
                }

                return MapToDto(evidence);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting evidence by id {id}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Create new evidence in database
        /// </summary>
        public async Task<EvidenceDto> CreateAsync(CreateEvidenceDto createEvidenceDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(createEvidenceDto.Name))
                    throw new ArgumentException("Evidence name is required");

                var evidence = new Evidence
                {
                    Id = Guid.NewGuid(),
                    Title = createEvidenceDto.Name,
                    Description = createEvidenceDto.Description ?? string.Empty,
                    Type = createEvidenceDto.EvidenceType ?? string.Empty,
                    CollectionDate = createEvidenceDto.CollectionDate,
                    VerificationStatus = createEvidenceDto.Status ?? "Pending",
                    CollectedBy = createEvidenceDto.Owner ?? string.Empty,
                    FilePath = createEvidenceDto.Location ?? string.Empty,
                    Comments = createEvidenceDto.Notes ?? string.Empty,
                    EvidenceNumber = $"EV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}"
                };

                _context.Evidences.Add(evidence);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Created evidence '{evidence.Title}' with ID {evidence.Id}");
                return MapToDto(evidence);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating evidence: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Update existing evidence in database
        /// </summary>
        public async Task<EvidenceDto?> UpdateAsync(Guid id, UpdateEvidenceDto updateEvidenceDto)
        {
            try
            {
                var evidence = await _context.Evidences.FirstOrDefaultAsync(e => e.Id == id);
                if (evidence == null)
                {
                    _logger.LogWarning($"Evidence with ID {id} not found for update");
                    return null;
                }

                evidence.Title = updateEvidenceDto.Name ?? evidence.Title;
                evidence.Description = updateEvidenceDto.Description ?? evidence.Description;
                evidence.Type = updateEvidenceDto.EvidenceType ?? evidence.Type;
                evidence.VerificationStatus = updateEvidenceDto.Status ?? evidence.VerificationStatus;
                evidence.Comments = updateEvidenceDto.Notes ?? evidence.Comments;

                _context.Evidences.Update(evidence);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Updated evidence with ID {id}");
                return MapToDto(evidence);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating evidence {id}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Delete evidence from database
        /// </summary>
        public async Task DeleteAsync(Guid id)
        {
            try
            {
                var evidence = await _context.Evidences.FirstOrDefaultAsync(e => e.Id == id);
                if (evidence == null)
                {
                    _logger.LogWarning($"Evidence with ID {id} not found for deletion");
                    return;
                }

                _context.Evidences.Remove(evidence);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Deleted evidence with ID {id}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting evidence {id}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get evidences by type
        /// </summary>
        public async Task<IEnumerable<EvidenceDto>> GetByTypeAsync(string type)
        {
            try
            {
                var evidences = await _context.Evidences
                    .AsNoTracking()
                    .Where(e => e.Type == type)
                    .Select(e => MapToDto(e))
                    .ToListAsync();

                _logger.LogInformation($"Retrieved {evidences.Count} evidences of type '{type}'");
                return evidences;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting evidences by type {type}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get evidences by data classification
        /// </summary>
        public async Task<IEnumerable<EvidenceDto>> GetByClassificationAsync(string classification)
        {
            try
            {
                var evidences = await _context.Evidences
                    .AsNoTracking()
                    .Where(e => e.Type == classification)
                    .Select(e => MapToDto(e))
                    .ToListAsync();

                _logger.LogInformation($"Retrieved {evidences.Count} evidences with classification '{classification}'");
                return evidences;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting evidences by classification {classification}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get evidences that are expiring within specified days
        /// </summary>
        public async Task<IEnumerable<EvidenceDto>> GetExpiringEvidencesAsync(int days)
        {
            try
            {
                var expiryDate = DateTime.UtcNow.AddDays(days);
                var evidences = await _context.Evidences
                    .AsNoTracking()
                    .Where(e => e.VerificationDate.HasValue && e.VerificationDate <= expiryDate)
                    .Select(e => MapToDto(e))
                    .ToListAsync();

                _logger.LogInformation($"Retrieved {evidences.Count} evidences expiring within {days} days");
                return evidences;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting expiring evidences: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get evidences by audit ID
        /// </summary>
        public async Task<IEnumerable<EvidenceDto>> GetByAuditIdAsync(Guid auditId)
        {
            try
            {
                var evidences = await _context.Evidences
                    .AsNoTracking()
                    .Where(e => e.AuditId == auditId)
                    .Select(e => MapToDto(e))
                    .ToListAsync();

                _logger.LogInformation($"Retrieved {evidences.Count} evidences for audit {auditId}");
                return evidences;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting evidences by audit id {auditId}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get evidence statistics
        /// </summary>
        public async Task<EvidenceStatisticsDto> GetStatisticsAsync()
        {
            try
            {
                var evidences = await _context.Evidences.AsNoTracking().ToListAsync();
                var expiryDate = DateTime.UtcNow.AddDays(30);

                var stats = new EvidenceStatisticsDto
                {
                    TotalEvidences = evidences.Count,
                    ActiveEvidences = evidences.Count(e => e.VerificationStatus == "Verified"),
                    ExpiredEvidences = evidences.Count(e => e.VerificationStatus == "Rejected"),
                    ArchivedEvidences = 0,
                    ExpiringSoon = evidences.Count(e => e.VerificationDate.HasValue && e.VerificationDate <= expiryDate),
                    EvidencesByType = evidences
                        .GroupBy(e => e.Type ?? "Unknown")
                        .ToDictionary(g => g.Key, g => g.Count()),
                    ClassificationDistribution = evidences
                        .GroupBy(e => e.Type ?? "Unknown")
                        .ToDictionary(g => g.Key, g => g.Count()),
                    StatusDistribution = evidences
                        .GroupBy(e => e.VerificationStatus ?? "Unknown")
                        .ToDictionary(g => g.Key, g => g.Count())
                };

                _logger.LogInformation("Evidence statistics calculated successfully");
                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting evidence statistics: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Helper method to map Evidence entity to EvidenceDto
        /// </summary>
        private static EvidenceDto MapToDto(Evidence evidence)
        {
            return new EvidenceDto
            {
                Id = evidence.Id,
                Name = evidence.Title,
                Description = evidence.Description,
                EvidenceType = evidence.Type,
                DataClassification = evidence.Type,
                Source = evidence.CollectedBy,
                CollectionDate = evidence.CollectionDate,
                ExpirationDate = evidence.VerificationDate,
                Status = evidence.VerificationStatus,
                Owner = evidence.CollectedBy,
                Location = evidence.FilePath,
                Tags = string.Empty,
                Notes = evidence.Comments
            };
        }
    }
}
