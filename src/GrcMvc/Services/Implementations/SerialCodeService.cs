using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Serial Code Service Implementation
/// Provides unified, tenant-aware, auditable serial code generation for all GRC artifacts.
/// Format: {PREFIX}-{TENANT}-{STAGE}-{YEAR}-{SEQUENCE}-{VERSION}
/// Example: ASM-ACME-01-2026-000142-01
/// </summary>
public class SerialCodeService : ISerialCodeService
{
    private readonly GrcDbContext _context;
    private readonly ILogger<SerialCodeService> _logger;

    // Regex pattern for serial code validation
    private static readonly Regex SerialCodePattern = new(
        @"^([A-Z]{3,5}(?:-[A-Z])?)-([A-Z0-9]{3,6})-(\d{2})-(\d{4})-(\d{6})-(\d{2})$",
        RegexOptions.Compiled);

    public SerialCodeService(GrcDbContext context, ILogger<SerialCodeService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // =========================================================================
    // GENERATION
    // =========================================================================

    public async Task<SerialCodeResult> GenerateAsync(SerialCodeRequest request)
    {
        // Validate tenant code
        if (!SerialCodePrefixes.IsValidTenantCode(request.TenantCode))
        {
            throw new ArgumentException($"Invalid tenant code: {request.TenantCode}. Must be 3-6 uppercase alphanumeric characters.");
        }

        var prefix = SerialCodePrefixes.GetPrefix(request.EntityType);
        var tenantCode = request.TenantCode.ToUpperInvariant();
        var stage = request.Stage ?? SerialCodePrefixes.GetStage(prefix);
        var year = request.Year ?? DateTime.UtcNow.Year;
        var createdBy = request.CreatedBy ?? "System";

        // Get next sequence (atomic operation with retry for concurrency)
        var sequence = await GetNextSequenceAtomicAsync(prefix, tenantCode, stage, year);

        // Build the code
        var code = BuildCode(prefix, tenantCode, stage, year, sequence, 1);

        // Register in the registry
        var registry = new SerialCodeRegistry
        {
            Code = code,
            Prefix = prefix,
            TenantCode = tenantCode,
            Stage = stage,
            Year = year,
            Sequence = sequence,
            Version = 1,
            EntityType = request.EntityType,
            EntityId = request.EntityId ?? Guid.Empty,
            Status = "active",
            Metadata = request.Metadata != null ? JsonSerializer.Serialize(request.Metadata) : null,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Set<SerialCodeRegistry>().Add(registry);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Generated serial code {Code} for {EntityType}", code, request.EntityType);

        return new SerialCodeResult
        {
            Code = code,
            Prefix = prefix,
            TenantCode = tenantCode,
            Stage = stage,
            Year = year,
            Sequence = sequence,
            Version = 1,
            CreatedAt = registry.CreatedAt,
            CreatedBy = createdBy,
            EntityId = request.EntityId
        };
    }

    public async Task<List<SerialCodeResult>> GenerateBatchAsync(List<SerialCodeRequest> requests)
    {
        var results = new List<SerialCodeResult>();

        // Use a transaction for atomicity
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var request in requests)
            {
                var result = await GenerateAsync(request);
                results.Add(result);
            }

            await transaction.CommitAsync();
            _logger.LogInformation("Generated {Count} serial codes in batch", results.Count);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        return results;
    }

    // =========================================================================
    // VALIDATION
    // =========================================================================

    public SerialCodeValidationResult Validate(string code)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        if (string.IsNullOrWhiteSpace(code))
        {
            errors.Add("Serial code cannot be empty");
            return new SerialCodeValidationResult { IsValid = false, Errors = errors, Warnings = warnings };
        }

        var match = SerialCodePattern.Match(code);
        if (!match.Success)
        {
            errors.Add("Invalid serial code format. Expected: {PREFIX}-{TENANT}-{STAGE}-{YEAR}-{SEQUENCE}-{VERSION}");
            return new SerialCodeValidationResult { IsValid = false, Errors = errors, Warnings = warnings };
        }

        var prefix = match.Groups[1].Value;
        var tenant = match.Groups[2].Value;
        var stageStr = match.Groups[3].Value;
        var yearStr = match.Groups[4].Value;
        var seqStr = match.Groups[5].Value;
        var verStr = match.Groups[6].Value;

        var stage = int.Parse(stageStr);
        var year = int.Parse(yearStr);
        var sequence = int.Parse(seqStr);
        var version = int.Parse(verStr);

        // Validate stage
        if (stage < 0 || stage > 6)
        {
            errors.Add($"Invalid stage: {stage}. Must be 00-06");
        }

        // Validate year
        var currentYear = DateTime.UtcNow.Year;
        if (year < 2020 || year > currentYear + 1)
        {
            warnings.Add($"Unusual year: {year}");
        }

        // Validate sequence
        if (sequence < 1 || sequence > 999999)
        {
            errors.Add($"Invalid sequence: {sequence}. Must be 000001-999999");
        }

        // Validate version
        if (version < 1 || version > 99)
        {
            errors.Add($"Invalid version: {version}. Must be 01-99");
        }

        // Validate tenant code format
        if (!SerialCodePrefixes.IsValidTenantCode(tenant))
        {
            errors.Add($"Invalid tenant code: {tenant}");
        }

        var baseCode = $"{prefix}-{tenant}-{stageStr}-{yearStr}-{seqStr}";

        return new SerialCodeValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors,
            Warnings = warnings,
            Parsed = new ParsedSerialCode
            {
                IsValid = errors.Count == 0,
                Prefix = prefix,
                TenantCode = tenant,
                Stage = stage,
                Year = year,
                Sequence = sequence,
                Version = version,
                BaseCode = baseCode
            }
        };
    }

    public ParsedSerialCode Parse(string code)
    {
        var result = Validate(code);
        if (result.Parsed == null || !result.IsValid)
        {
            throw new ArgumentException($"Invalid serial code: {code}. Errors: {string.Join(", ", result.Errors)}");
        }
        return result.Parsed;
    }

    // =========================================================================
    // LOOKUP
    // =========================================================================

    public async Task<bool> ExistsAsync(string code)
    {
        return await _context.Set<SerialCodeRegistry>()
            .AnyAsync(r => r.Code == code);
    }

    public async Task<SerialCodeRecord?> GetByCodeAsync(string code)
    {
        var registry = await _context.Set<SerialCodeRegistry>()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Code == code);

        return registry == null ? null : MapToRecord(registry);
    }

    public async Task<List<SerialCodeVersion>> GetHistoryAsync(string code)
    {
        var parsed = Parse(code);

        // Find all versions with the same base code
        var records = await _context.Set<SerialCodeRegistry>()
            .AsNoTracking()
            .Where(r => r.Prefix == parsed.Prefix
                && r.TenantCode == parsed.TenantCode
                && r.Stage == parsed.Stage
                && r.Year == parsed.Year
                && r.Sequence == parsed.Sequence)
            .OrderBy(r => r.Version)
            .ToListAsync();

        return records.Select(r => new SerialCodeVersion
        {
            Code = r.Code,
            Version = r.Version,
            Status = r.Status,
            CreatedAt = r.CreatedAt,
            CreatedBy = r.CreatedBy,
            ChangeReason = r.StatusReason
        }).ToList();
    }

    public async Task<SerialCodeRecord?> GetByEntityAsync(string entityType, Guid entityId)
    {
        var registry = await _context.Set<SerialCodeRegistry>()
            .AsNoTracking()
            .Where(r => r.EntityType == entityType && r.EntityId == entityId && r.Status == "active")
            .OrderByDescending(r => r.Version)
            .FirstOrDefaultAsync();

        return registry == null ? null : MapToRecord(registry);
    }

    // =========================================================================
    // VERSIONING
    // =========================================================================

    public async Task<SerialCodeResult> CreateNewVersionAsync(string baseCode, string? changeReason = null)
    {
        // Get current active version
        var current = await _context.Set<SerialCodeRegistry>()
            .Where(r => r.Code == baseCode && r.Status == "active")
            .FirstOrDefaultAsync();

        if (current == null)
        {
            throw new ArgumentException($"Serial code not found or not active: {baseCode}");
        }

        // Check max version
        var newVersion = current.Version + 1;
        if (newVersion > 99)
        {
            throw new InvalidOperationException($"Maximum version (99) reached for: {baseCode}");
        }

        // Mark current as superseded
        current.Status = "superseded";
        current.StatusReason = changeReason ?? "Superseded by new version";
        current.UpdatedAt = DateTime.UtcNow;

        // Create new version
        var newCode = BuildCode(current.Prefix, current.TenantCode, current.Stage, current.Year, current.Sequence, newVersion);

        var newRegistry = new SerialCodeRegistry
        {
            Code = newCode,
            Prefix = current.Prefix,
            TenantCode = current.TenantCode,
            Stage = current.Stage,
            Year = current.Year,
            Sequence = current.Sequence,
            Version = newVersion,
            EntityType = current.EntityType,
            EntityId = current.EntityId,
            Status = "active",
            PreviousVersionCode = current.Code,
            Metadata = current.Metadata,
            CreatedBy = "System",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Set<SerialCodeRegistry>().Add(newRegistry);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created new version {NewCode} from {OldCode}", newCode, baseCode);

        return new SerialCodeResult
        {
            Code = newCode,
            Prefix = current.Prefix,
            TenantCode = current.TenantCode,
            Stage = current.Stage,
            Year = current.Year,
            Sequence = current.Sequence,
            Version = newVersion,
            CreatedAt = newRegistry.CreatedAt,
            CreatedBy = newRegistry.CreatedBy,
            EntityId = current.EntityId
        };
    }

    public async Task<string> GetLatestVersionAsync(string baseCode)
    {
        var parsed = Parse(baseCode);

        var latest = await _context.Set<SerialCodeRegistry>()
            .AsNoTracking()
            .Where(r => r.Prefix == parsed.Prefix
                && r.TenantCode == parsed.TenantCode
                && r.Stage == parsed.Stage
                && r.Year == parsed.Year
                && r.Sequence == parsed.Sequence
                && r.Status == "active")
            .OrderByDescending(r => r.Version)
            .FirstOrDefaultAsync();

        return latest?.Code ?? baseCode;
    }

    // =========================================================================
    // SEARCH
    // =========================================================================

    public async Task<SerialCodeSearchResult> SearchAsync(SerialCodeSearchCriteria criteria)
    {
        var query = _context.Set<SerialCodeRegistry>().AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(criteria.Prefix))
            query = query.Where(r => r.Prefix == criteria.Prefix);

        if (!string.IsNullOrEmpty(criteria.TenantCode))
            query = query.Where(r => r.TenantCode == criteria.TenantCode);

        if (criteria.Stage.HasValue)
            query = query.Where(r => r.Stage == criteria.Stage.Value);

        if (criteria.Year.HasValue)
            query = query.Where(r => r.Year == criteria.Year.Value);

        if (criteria.SequenceFrom.HasValue)
            query = query.Where(r => r.Sequence >= criteria.SequenceFrom.Value);

        if (criteria.SequenceTo.HasValue)
            query = query.Where(r => r.Sequence <= criteria.SequenceTo.Value);

        if (!string.IsNullOrEmpty(criteria.Status))
            query = query.Where(r => r.Status == criteria.Status);

        if (!string.IsNullOrEmpty(criteria.EntityType))
            query = query.Where(r => r.EntityType == criteria.EntityType);

        if (criteria.CreatedAfter.HasValue)
            query = query.Where(r => r.CreatedAt >= criteria.CreatedAfter.Value);

        if (criteria.CreatedBefore.HasValue)
            query = query.Where(r => r.CreatedAt <= criteria.CreatedBefore.Value);

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip(criteria.Offset)
            .Take(criteria.Limit)
            .ToListAsync();

        return new SerialCodeSearchResult
        {
            Items = items.Select(MapToRecord).ToList(),
            Total = total,
            HasMore = total > criteria.Offset + criteria.Limit
        };
    }

    public async Task<List<SerialCodeRecord>> GetByPrefixAsync(string prefix, int limit = 100, int offset = 0)
    {
        var items = await _context.Set<SerialCodeRegistry>()
            .AsNoTracking()
            .Where(r => r.Prefix == prefix)
            .OrderByDescending(r => r.CreatedAt)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

        return items.Select(MapToRecord).ToList();
    }

    public async Task<List<SerialCodeRecord>> GetByTenantAsync(string tenantCode, int limit = 100, int offset = 0)
    {
        var items = await _context.Set<SerialCodeRegistry>()
            .AsNoTracking()
            .Where(r => r.TenantCode == tenantCode)
            .OrderByDescending(r => r.CreatedAt)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

        return items.Select(MapToRecord).ToList();
    }

    public async Task<List<SerialCodeRecord>> GetByStageAsync(int stage, int limit = 100, int offset = 0)
    {
        var items = await _context.Set<SerialCodeRegistry>()
            .AsNoTracking()
            .Where(r => r.Stage == stage)
            .OrderByDescending(r => r.CreatedAt)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

        return items.Select(MapToRecord).ToList();
    }

    // =========================================================================
    // RESERVATION
    // =========================================================================

    public async Task<SerialCodeReservationResult> ReserveAsync(SerialCodeRequest request, TimeSpan? ttl = null)
    {
        var prefix = SerialCodePrefixes.GetPrefix(request.EntityType);
        var tenantCode = request.TenantCode.ToUpperInvariant();
        var stage = request.Stage ?? SerialCodePrefixes.GetStage(prefix);
        var year = request.Year ?? DateTime.UtcNow.Year;
        var createdBy = request.CreatedBy ?? "System";

        // Get next sequence
        var sequence = await GetNextSequenceAtomicAsync(prefix, tenantCode, stage, year);

        // Build code with version 01
        var code = BuildCode(prefix, tenantCode, stage, year, sequence, 1);

        var expiresAt = DateTime.UtcNow.Add(ttl ?? TimeSpan.FromMinutes(5));

        var reservation = new SerialCodeReservation
        {
            ReservedCode = code,
            Prefix = prefix,
            TenantCode = tenantCode,
            Stage = stage,
            Year = year,
            Sequence = sequence,
            Status = "reserved",
            ExpiresAt = expiresAt,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<SerialCodeReservation>().Add(reservation);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Reserved serial code {Code} until {ExpiresAt}", code, expiresAt);

        return new SerialCodeReservationResult
        {
            ReservationId = reservation.Id.ToString(),
            ReservedCode = code,
            ExpiresAt = expiresAt
        };
    }

    public async Task<SerialCodeResult> ConfirmReservationAsync(string reservationId, Guid entityId)
    {
        if (!Guid.TryParse(reservationId, out var id))
        {
            throw new ArgumentException("Invalid reservation ID");
        }

        var reservation = await _context.Set<SerialCodeReservation>()
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reservation == null)
        {
            throw new ArgumentException($"Reservation not found: {reservationId}");
        }

        if (reservation.Status != "reserved")
        {
            throw new InvalidOperationException($"Reservation is not in 'reserved' status: {reservation.Status}");
        }

        if (reservation.ExpiresAt < DateTime.UtcNow)
        {
            reservation.Status = "expired";
            await _context.SaveChangesAsync();
            throw new InvalidOperationException($"Reservation has expired at {reservation.ExpiresAt}");
        }

        // Create the registry entry
        var registry = new SerialCodeRegistry
        {
            Code = reservation.ReservedCode,
            Prefix = reservation.Prefix,
            TenantCode = reservation.TenantCode,
            Stage = reservation.Stage,
            Year = reservation.Year,
            Sequence = reservation.Sequence,
            Version = 1,
            EntityType = "Reserved", // Will be updated by caller
            EntityId = entityId,
            Status = "active",
            CreatedBy = reservation.CreatedBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Update reservation status
        reservation.Status = "confirmed";
        reservation.ConfirmedAt = DateTime.UtcNow;

        _context.Set<SerialCodeRegistry>().Add(registry);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Confirmed reservation {ReservationId} as {Code}", reservationId, reservation.ReservedCode);

        return new SerialCodeResult
        {
            Code = registry.Code,
            Prefix = registry.Prefix,
            TenantCode = registry.TenantCode,
            Stage = registry.Stage,
            Year = registry.Year,
            Sequence = registry.Sequence,
            Version = registry.Version,
            CreatedAt = registry.CreatedAt,
            CreatedBy = registry.CreatedBy,
            EntityId = entityId
        };
    }

    public async Task CancelReservationAsync(string reservationId)
    {
        if (!Guid.TryParse(reservationId, out var id))
        {
            throw new ArgumentException("Invalid reservation ID");
        }

        var reservation = await _context.Set<SerialCodeReservation>()
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reservation == null)
        {
            throw new ArgumentException($"Reservation not found: {reservationId}");
        }

        if (reservation.Status != "reserved")
        {
            throw new InvalidOperationException($"Cannot cancel reservation in status: {reservation.Status}");
        }

        reservation.Status = "cancelled";
        reservation.CancelledAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Cancelled reservation {ReservationId}", reservationId);
    }

    // =========================================================================
    // ADMINISTRATION
    // =========================================================================

    public async Task<int> GetNextSequenceAsync(string prefix, string tenantCode, int stage, int year)
    {
        var counter = await _context.Set<SerialSequenceCounter>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Prefix == prefix
                && c.TenantCode == tenantCode
                && c.Stage == stage
                && c.Year == year);

        return (counter?.CurrentSequence ?? 0) + 1;
    }

    public async Task VoidAsync(string code, string reason)
    {
        var registry = await _context.Set<SerialCodeRegistry>()
            .FirstOrDefaultAsync(r => r.Code == code);

        if (registry == null)
        {
            throw new ArgumentException($"Serial code not found: {code}");
        }

        if (registry.Status == "void")
        {
            throw new InvalidOperationException($"Serial code is already void: {code}");
        }

        registry.Status = "void";
        registry.StatusReason = reason;
        registry.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogWarning("Voided serial code {Code} for reason: {Reason}", code, reason);
    }

    public async Task<SerialCodeTraceabilityReport> GetTraceabilityReportAsync(string code)
    {
        var record = await GetByCodeAsync(code);
        if (record == null)
        {
            throw new ArgumentException($"Serial code not found: {code}");
        }

        var history = await GetHistoryAsync(code);

        // Find related codes (same entity)
        var relatedRecords = await _context.Set<SerialCodeRegistry>()
            .AsNoTracking()
            .Where(r => r.EntityId == record.EntityId && r.Code != code)
            .ToListAsync();

        var relatedCodes = relatedRecords.Select(r => new SerialCodeRelation
        {
            Code = r.Code,
            Relation = r.EntityType,
            EntityType = r.EntityType
        }).ToList();

        return new SerialCodeTraceabilityReport
        {
            CurrentCode = code,
            EntityType = record.EntityType,
            EntityId = record.EntityId,
            VersionHistory = history,
            RelatedCodes = relatedCodes,
            AuditTrail = new List<SerialCodeAuditEntry>() // Would be populated from audit log
        };
    }

    // =========================================================================
    // PRIVATE HELPERS
    // =========================================================================

    private static string BuildCode(string prefix, string tenantCode, int stage, int year, int sequence, int version)
    {
        return $"{prefix}-{tenantCode.ToUpperInvariant()}-{stage:D2}-{year}-{sequence:D6}-{version:D2}";
    }

    private async Task<int> GetNextSequenceAtomicAsync(string prefix, string tenantCode, int stage, int year)
    {
        // Try to get or create the counter with retry logic for concurrency
        const int maxRetries = 3;
        for (int retry = 0; retry < maxRetries; retry++)
        {
            try
            {
                var counter = await _context.Set<SerialSequenceCounter>()
                    .FirstOrDefaultAsync(c => c.Prefix == prefix
                        && c.TenantCode == tenantCode
                        && c.Stage == stage
                        && c.Year == year);

                if (counter == null)
                {
                    counter = new SerialSequenceCounter
                    {
                        Prefix = prefix,
                        TenantCode = tenantCode,
                        Stage = stage,
                        Year = year,
                        CurrentSequence = 1,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.Set<SerialSequenceCounter>().Add(counter);
                }
                else
                {
                    counter.CurrentSequence++;
                    counter.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                return counter.CurrentSequence;
            }
            catch (DbUpdateConcurrencyException) when (retry < maxRetries - 1)
            {
                // Retry on concurrency conflict
                _logger.LogWarning("Concurrency conflict getting sequence for {Prefix}-{Tenant}-{Stage}-{Year}, retry {Retry}",
                    prefix, tenantCode, stage, year, retry + 1);

                // Clear all tracked entities efficiently and retry
                _context.ChangeTracker.Clear();
            }
        }

        throw new InvalidOperationException($"Failed to get next sequence after {maxRetries} retries");
    }

    private static SerialCodeRecord MapToRecord(SerialCodeRegistry registry)
    {
        return new SerialCodeRecord
        {
            Id = registry.Id,
            Code = registry.Code,
            EntityType = registry.EntityType,
            EntityId = registry.EntityId,
            Prefix = registry.Prefix,
            TenantCode = registry.TenantCode,
            Stage = registry.Stage,
            Year = registry.Year,
            Sequence = registry.Sequence,
            Version = registry.Version,
            Status = registry.Status,
            Metadata = string.IsNullOrEmpty(registry.Metadata)
                ? null
                : JsonSerializer.Deserialize<Dictionary<string, object>>(registry.Metadata),
            CreatedAt = registry.CreatedAt,
            CreatedBy = registry.CreatedBy,
            UpdatedAt = registry.UpdatedAt,
            UpdatedBy = registry.UpdatedBy
        };
    }
}
