using GrcMvc.Common.Results;
using ResultT = GrcMvc.Common.Results.Result<object>;

namespace GrcMvc.Common.Extensions;

/// <summary>
/// Extension methods for working with objects and Result types
/// </summary>
public static class ObjectExtensions
{
    /// <summary>
    /// Converts a nullable value to a Result, returning failure if null
    /// </summary>
    public static Results.Result<T> ToResult<T>(this T? value, string entityName, object id) where T : class
    {
        if (value == null)
            return Results.Result<T>.Failure(
                $"{entityName} with ID {id} does not exist",
                ErrorCodes.EntityNotFound);
        return Results.Result<T>.Success(value);
    }

    /// <summary>
    /// Converts a nullable value to a Result with custom error
    /// </summary>
    public static Results.Result<T> ToResult<T>(this T? value, Error error) where T : class
    {
        return value == null ? Results.Result<T>.Failure(error.Message, error.Code) : Results.Result<T>.Success(value);
    }

    /// <summary>
    /// Converts a nullable value to a Result with custom error code and message
    /// </summary>
    public static Results.Result<T> ToResult<T>(this T? value, string errorCode, string errorMessage, string? details = null) where T : class
    {
        return value == null
            ? Results.Result<T>.Failure(errorMessage, errorCode)
            : Results.Result<T>.Success(value);
    }

    /// <summary>
    /// Checks if an object is null
    /// </summary>
    public static bool IsNull<T>(this T? value) where T : class => value == null;

    /// <summary>
    /// Checks if an object is not null
    /// </summary>
    public static bool IsNotNull<T>(this T? value) where T : class => value != null;

    /// <summary>
    /// Executes an action if the value is not null
    /// </summary>
    public static T? IfNotNull<T>(this T? value, Action<T> action) where T : class
    {
        if (value != null)
            action(value);
        return value;
    }

    /// <summary>
    /// Maps a value to another type if not null, otherwise returns null
    /// </summary>
    public static TOut? MapIfNotNull<TIn, TOut>(this TIn? value, Func<TIn, TOut> mapper)
        where TIn : class
        where TOut : class
    {
        return value != null ? mapper(value) : null;
    }

    /// <summary>
    /// Returns the value if not null, otherwise returns the default value
    /// </summary>
    public static T OrDefault<T>(this T? value, T defaultValue) where T : class
    {
        return value ?? defaultValue;
    }

    /// <summary>
    /// Returns the value if not null, otherwise computes and returns the default value
    /// </summary>
    public static T OrElse<T>(this T? value, Func<T> defaultValueProvider) where T : class
    {
        return value ?? defaultValueProvider();
    }
}
