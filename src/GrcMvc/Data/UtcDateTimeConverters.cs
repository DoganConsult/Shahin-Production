using System;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GrcMvc.Data;

/// <summary>
/// EF Core value converters that enforce DateTimeKind.Utc on materialization.
///
/// Problem: PostgreSQL timestamp (without time zone) columns return DateTime with Kind=Unspecified.
/// This causes subtle bugs when comparing against DateTime.UtcNow (which has Kind=Utc).
///
/// Solution: These converters ensure all DateTime values read from the database are marked as UTC.
///
/// Usage:
/// - Apply to DateTime properties in OnModelCreating
/// - For full correctness, migrate DB columns to timestamptz and use DateTimeOffset
///
/// Migration path:
/// 1. Short-term: Use these converters (no schema change required)
/// 2. Long-term: Migrate to timestamptz + DateTimeOffset for unambiguous instant handling
/// </summary>
public static class UtcDateTimeConverters
{
    /// <summary>
    /// Converter for non-nullable DateTime properties.
    /// On write: passes through unchanged (assumes value is already UTC or should be treated as UTC).
    /// On read: sets Kind=Utc to ensure correct comparisons with DateTime.UtcNow.
    /// </summary>
    public static readonly ValueConverter<DateTime, DateTime> UtcDateTime =
        new(
            v => v, // Write: pass through
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc) // Read: mark as UTC
        );

    /// <summary>
    /// Converter for nullable DateTime? properties.
    /// Same semantics as UtcDateTime but handles null values.
    /// </summary>
    public static readonly ValueConverter<DateTime?, DateTime?> UtcNullableDateTime =
        new(
            v => v, // Write: pass through
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null // Read: mark as UTC
        );
}
