using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GrcMvc.Common.Results;

namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Serial Code Service Interface
/// Provides unified, tenant-aware, auditable serial code generation for all GRC artifacts.
/// Format: {PREFIX}-{TENANT}-{STAGE}-{YEAR}-{SEQUENCE}-{VERSION}
/// Example: ASM-ACME-01-2026-000142-01
/// </summary>
public interface ISerialCodeService
{
    // =========================================================================
    // GENERATION
    // =========================================================================

    /// <summary>
    /// Generate a new serial code for an entity
    /// </summary>
    Task<Result<SerialCodeResult>> GenerateAsync(SerialCodeRequest request);

    /// <summary>
    /// Generate multiple serial codes in a batch (atomic operation)
    /// </summary>
    Task<List<SerialCodeResult>> GenerateBatchAsync(List<SerialCodeRequest> requests);

    // =========================================================================
    // VALIDATION
    // =========================================================================

    /// <summary>
    /// Validate a serial code format and components
    /// </summary>
    SerialCodeValidationResult Validate(string code);

    /// <summary>
    /// Parse a serial code into its components
    /// </summary>
    Result<ParsedSerialCode> Parse(string code);

    // =========================================================================
    // LOOKUP
    // =========================================================================

    /// <summary>
    /// Check if a serial code exists
    /// </summary>
    Task<bool> ExistsAsync(string code);

    /// <summary>
    /// Get serial code record by code
    /// </summary>
    Task<SerialCodeRecord?> GetByCodeAsync(string code);

    /// <summary>
    /// Get version history for a serial code
    /// </summary>
    Task<List<SerialCodeVersion>> GetHistoryAsync(string code);

    /// <summary>
    /// Get serial code by entity reference
    /// </summary>
    Task<SerialCodeRecord?> GetByEntityAsync(string entityType, Guid entityId);

    // =========================================================================
    // VERSIONING
    // =========================================================================

    /// <summary>
    /// Create a new version of an existing serial code
    /// Marks the previous version as superseded
    /// </summary>
    Task<Result<SerialCodeResult>> CreateNewVersionAsync(string baseCode, string? changeReason = null);

    /// <summary>
    /// Get the latest version code for a base code
    /// </summary>
    Task<string> GetLatestVersionAsync(string baseCode);

    // =========================================================================
    // SEARCH
    // =========================================================================

    /// <summary>
    /// Search serial codes by criteria
    /// </summary>
    Task<SerialCodeSearchResult> SearchAsync(SerialCodeSearchCriteria criteria);

    /// <summary>
    /// Get serial codes by prefix
    /// </summary>
    Task<List<SerialCodeRecord>> GetByPrefixAsync(string prefix, int limit = 100, int offset = 0);

    /// <summary>
    /// Get serial codes by tenant
    /// </summary>
    Task<List<SerialCodeRecord>> GetByTenantAsync(string tenantCode, int limit = 100, int offset = 0);

    /// <summary>
    /// Get serial codes by stage
    /// </summary>
    Task<List<SerialCodeRecord>> GetByStageAsync(int stage, int limit = 100, int offset = 0);

    // =========================================================================
    // RESERVATION
    // =========================================================================

    /// <summary>
    /// Reserve a serial code for later use (e.g., batch imports)
    /// </summary>
    Task<SerialCodeReservationResult> ReserveAsync(SerialCodeRequest request, TimeSpan? ttl = null);

    /// <summary>
    /// Confirm a reserved serial code
    /// </summary>
    Task<Result<SerialCodeResult>> ConfirmReservationAsync(string reservationId, Guid entityId);

    /// <summary>
    /// Cancel a reservation
    /// </summary>
    Task<Result> CancelReservationAsync(string reservationId);

    // =========================================================================
    // ADMINISTRATION
    // =========================================================================

    /// <summary>
    /// Get the next sequence number (peek, does not increment)
    /// </summary>
    Task<int> GetNextSequenceAsync(string prefix, string tenantCode, int stage, int year);

    /// <summary>
    /// Void a serial code (mark as invalid)
    /// </summary>
    Task<Result> VoidAsync(string code, string reason);

    /// <summary>
    /// Generate traceability report for a serial code
    /// </summary>
    Task<Result<SerialCodeTraceabilityReport>> GetTraceabilityReportAsync(string code);
}

// =============================================================================
// DATA TRANSFER OBJECTS
// =============================================================================

/// <summary>
/// Request to generate a serial code
/// </summary>
public sealed record SerialCodeRequest
{
    /// <summary>
    /// Entity type (maps to prefix): assessment, risk, control, evidence, etc.
    /// </summary>
    public required string EntityType { get; init; }

    /// <summary>
    /// Tenant code (3-6 uppercase alphanumeric)
    /// </summary>
    public required string TenantCode { get; init; }

    /// <summary>
    /// Entity ID to link the serial code to
    /// </summary>
    public Guid? EntityId { get; init; }

    /// <summary>
    /// Stage number (1-6). Auto-detected from entity type if not provided.
    /// </summary>
    public int? Stage { get; init; }

    /// <summary>
    /// Year (defaults to current year)
    /// </summary>
    public int? Year { get; init; }

    /// <summary>
    /// Additional metadata to store with the serial code
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// User ID creating the serial code
    /// </summary>
    public string? CreatedBy { get; init; }
}

/// <summary>
/// Result of serial code generation
/// </summary>
public sealed record SerialCodeResult
{
    public required string Code { get; init; }
    public required string Prefix { get; init; }
    public required string TenantCode { get; init; }
    public required int Stage { get; init; }
    public required int Year { get; init; }
    public required int Sequence { get; init; }
    public required int Version { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required string CreatedBy { get; init; }
    public Guid? EntityId { get; init; }
}

/// <summary>
/// Parsed serial code components
/// </summary>
public sealed record ParsedSerialCode
{
    public required bool IsValid { get; init; }
    public required string Prefix { get; init; }
    public required string TenantCode { get; init; }
    public required int Stage { get; init; }
    public required int Year { get; init; }
    public required int Sequence { get; init; }
    public required int Version { get; init; }
    /// <summary>
    /// Base code without version (for version tracking)
    /// </summary>
    public required string BaseCode { get; init; }
}

/// <summary>
/// Validation result for a serial code
/// </summary>
public sealed record SerialCodeValidationResult
{
    public required bool IsValid { get; init; }
    public required List<string> Errors { get; init; }
    public required List<string> Warnings { get; init; }
    public ParsedSerialCode? Parsed { get; init; }
}

/// <summary>
/// Serial code record from the registry
/// </summary>
public sealed record SerialCodeRecord
{
    public required Guid Id { get; init; }
    public required string Code { get; init; }
    public required string EntityType { get; init; }
    public required Guid EntityId { get; init; }
    public required string Prefix { get; init; }
    public required string TenantCode { get; init; }
    public required int Stage { get; init; }
    public required int Year { get; init; }
    public required int Sequence { get; init; }
    public required int Version { get; init; }
    public required string Status { get; init; } // active, superseded, void, reserved
    public Dictionary<string, object>? Metadata { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required string CreatedBy { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public string? UpdatedBy { get; init; }
}

/// <summary>
/// Serial code version history entry
/// </summary>
public sealed record SerialCodeVersion
{
    public required string Code { get; init; }
    public required int Version { get; init; }
    public required string Status { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required string CreatedBy { get; init; }
    public string? ChangeReason { get; init; }
}

/// <summary>
/// Search criteria for serial codes
/// </summary>
public sealed record SerialCodeSearchCriteria
{
    public string? Prefix { get; init; }
    public string? TenantCode { get; init; }
    public int? Stage { get; init; }
    public int? Year { get; init; }
    public int? SequenceFrom { get; init; }
    public int? SequenceTo { get; init; }
    public string? Status { get; init; }
    public string? EntityType { get; init; }
    public DateTime? CreatedAfter { get; init; }
    public DateTime? CreatedBefore { get; init; }
    public int Limit { get; init; } = 100;
    public int Offset { get; init; } = 0;
}

/// <summary>
/// Search result with pagination
/// </summary>
public sealed record SerialCodeSearchResult
{
    public required List<SerialCodeRecord> Items { get; init; }
    public required int Total { get; init; }
    public required bool HasMore { get; init; }
}

/// <summary>
/// Reservation result
/// </summary>
public sealed record SerialCodeReservationResult
{
    public required string ReservationId { get; init; }
    public required string ReservedCode { get; init; }
    public required DateTime ExpiresAt { get; init; }
}

/// <summary>
/// Traceability report for audit purposes
/// </summary>
public sealed record SerialCodeTraceabilityReport
{
    public required string CurrentCode { get; init; }
    public required string EntityType { get; init; }
    public Guid? EntityId { get; init; }
    public required List<SerialCodeVersion> VersionHistory { get; init; }
    public required List<SerialCodeRelation> RelatedCodes { get; init; }
    public required List<SerialCodeAuditEntry> AuditTrail { get; init; }
}

/// <summary>
/// Related serial code reference
/// </summary>
public sealed record SerialCodeRelation
{
    public required string Code { get; init; }
    public required string Relation { get; init; } // finding, linked_risk, evidence, etc.
    public required string EntityType { get; init; }
}

/// <summary>
/// Audit entry for serial code operations
/// </summary>
public sealed record SerialCodeAuditEntry
{
    public required string Action { get; init; } // generate, validate, version, void, reserve, confirm
    public required string ActorUserId { get; init; }
    public required string ActorTenantCode { get; init; }
    public string? IpAddress { get; init; }
    public required DateTime Timestamp { get; init; }
    public Dictionary<string, object>? Details { get; init; }
}
