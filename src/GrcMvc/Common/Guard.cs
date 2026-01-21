using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

namespace GrcMvc.Common
{
    /// <summary>
    /// Guard clauses for defensive programming.
    /// ABP Best Practice: Use guard clauses to validate inputs early.
    /// </summary>
    public static class Guard
    {
        /// <summary>
        /// Throws ArgumentNullException if value is null.
        /// </summary>
        public static T NotNull<T>(
            [NotNull] T? value,
            [CallerArgumentExpression(nameof(value))] string? paramName = null) where T : class
        {
            if (value is null)
                throw new ArgumentNullException(paramName);
            return value;
        }

        /// <summary>
        /// Throws ArgumentNullException if value is null or empty string.
        /// </summary>
        public static string NotNullOrEmpty(
            [NotNull] string? value,
            [CallerArgumentExpression(nameof(value))] string? paramName = null)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Value cannot be null or empty", paramName);
            return value;
        }

        /// <summary>
        /// Throws ArgumentNullException if value is null, empty, or whitespace.
        /// </summary>
        public static string NotNullOrWhiteSpace(
            [NotNull] string? value,
            [CallerArgumentExpression(nameof(value))] string? paramName = null)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Value cannot be null, empty, or whitespace", paramName);
            return value;
        }

        /// <summary>
        /// Throws ArgumentException if collection is null or empty.
        /// </summary>
        public static IEnumerable<T> NotNullOrEmpty<T>(
            [NotNull] IEnumerable<T>? value,
            [CallerArgumentExpression(nameof(value))] string? paramName = null)
        {
            if (value is null || !value.Any())
                throw new ArgumentException("Collection cannot be null or empty", paramName);
            return value;
        }

        /// <summary>
        /// Throws ArgumentOutOfRangeException if value is not positive.
        /// </summary>
        public static int Positive(
            int value,
            [CallerArgumentExpression(nameof(value))] string? paramName = null)
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(paramName, value, "Value must be positive");
            return value;
        }

        /// <summary>
        /// Throws ArgumentOutOfRangeException if value is negative.
        /// </summary>
        public static int NotNegative(
            int value,
            [CallerArgumentExpression(nameof(value))] string? paramName = null)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(paramName, value, "Value cannot be negative");
            return value;
        }

        /// <summary>
        /// Throws ArgumentOutOfRangeException if value is not in range.
        /// </summary>
        public static int InRange(
            int value,
            int min,
            int max,
            [CallerArgumentExpression(nameof(value))] string? paramName = null)
        {
            if (value < min || value > max)
                throw new ArgumentOutOfRangeException(paramName, value, $"Value must be between {min} and {max}");
            return value;
        }

        /// <summary>
        /// Throws ArgumentException if Guid is empty.
        /// </summary>
        public static Guid NotEmpty(
            Guid value,
            [CallerArgumentExpression(nameof(value))] string? paramName = null)
        {
            if (value == Guid.Empty)
                throw new ArgumentException("Guid cannot be empty", paramName);
            return value;
        }

        /// <summary>
        /// Returns a Result instead of throwing for null values.
        /// </summary>
        public static Result<T> EnsureNotNull<T>(T? value, string entityName) where T : class
        {
            return value is null 
                ? Result.NotFound<T>(entityName, "null") 
                : Result<T>.Success(value);
        }

        /// <summary>
        /// Returns a Result instead of throwing for null values with custom ID.
        /// </summary>
        public static Result<T> EnsureNotNull<T>(T? value, string entityName, object id) where T : class
        {
            return value is null 
                ? Result.NotFound<T>(entityName, id) 
                : Result<T>.Success(value);
        }

        /// <summary>
        /// Returns a Result instead of throwing for empty Guid.
        /// </summary>
        public static Result<Guid> EnsureNotEmpty(Guid value, string paramName)
        {
            return value == Guid.Empty 
                ? Result<Guid>.Failure($"{paramName} cannot be empty", "INVALID_GUID") 
                : Result<Guid>.Success(value);
        }

        /// <summary>
        /// Returns a Result instead of throwing for empty string.
        /// </summary>
        public static Result<string> EnsureNotNullOrEmpty(string? value, string paramName)
        {
            return string.IsNullOrEmpty(value) 
                ? Result<string>.Failure($"{paramName} cannot be null or empty", "INVALID_STRING") 
                : Result<string>.Success(value);
        }
    }
}
