using GrcMvc.Common.Results;
using GrcMvc.Exceptions;

namespace GrcMvc.Common.Guards;

/// <summary>
/// Provides guard clauses for validation
/// </summary>
public static class Guard
{
    /// <summary>
    /// Ensures a value is not null
    /// </summary>
    public static T AgainstNull<T>(T? value, string paramName) where T : class
    {
        if (value == null)
            throw new ArgumentNullException(paramName);
        return value;
    }

    /// <summary>
    /// Ensures a string is not null or whitespace
    /// </summary>
    public static string AgainstNullOrWhiteSpace(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be null or whitespace", paramName);
        return value;
    }

    /// <summary>
    /// Ensures an entity exists, throwing EntityNotFoundException if null
    /// </summary>
    public static T AgainstNotFound<T>(T? value, string entityName, object id) where T : class
    {
        if (value == null)
            throw new EntityNotFoundException(entityName, id);
        return value;
    }

    /// <summary>
    /// Ensures a collection is not null or empty
    /// </summary>
    public static IEnumerable<T> AgainstNullOrEmpty<T>(IEnumerable<T>? collection, string paramName)
    {
        if (collection == null || !collection.Any())
            throw new ArgumentException("Collection cannot be null or empty", paramName);
        return collection;
    }

    /// <summary>
    /// Ensures a condition is true
    /// </summary>
    public static void AgainstCondition(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }

    /// <summary>
    /// Ensures a GUID is not empty
    /// </summary>
    public static Guid AgainstEmptyGuid(Guid value, string paramName)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("GUID cannot be empty", paramName);
        return value;
    }

    /// <summary>
    /// Ensures a value is within a specified range
    /// </summary>
    public static T AgainstOutOfRange<T>(T value, T min, T max, string paramName) where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
            throw new ArgumentOutOfRangeException(paramName, $"Value must be between {min} and {max}");
        return value;
    }

    /// <summary>
    /// Ensures a value is positive
    /// </summary>
    public static int AgainstNegative(int value, string paramName)
    {
        if (value < 0)
            throw new ArgumentException("Value cannot be negative", paramName);
        return value;
    }

    /// <summary>
    /// Ensures a result is successful, throwing an exception if failed
    /// </summary>
    public static T AgainstFailure<T>(Result<T> result)
    {
        if (result.IsFailure)
            throw new InvalidOperationException($"Operation failed: {result.Error}");
        return result.Value!;
    }
}
