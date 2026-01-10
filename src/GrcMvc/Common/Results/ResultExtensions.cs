namespace GrcMvc.Common.Results;

/// <summary>
/// Extension methods for working with Result types
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Converts a nullable value to a Result, returning failure if null
    /// </summary>
    public static Result<T> ToResult<T>(this T? value, string entityName, object id) where T : class
    {
        if (value == null)
            return Result<T>.Failure(
                new Error(ErrorCodes.EntityNotFound,
                         $"{entityName} not found",
                         $"{entityName} with ID {id} does not exist",
                         new Dictionary<string, object> { { "EntityType", entityName }, { "EntityId", id } }));
        return Result<T>.Success(value);
    }

    /// <summary>
    /// Converts a nullable value to a Result with custom error message
    /// </summary>
    public static Result<T> ToResult<T>(this T? value, Error error) where T : class
    {
        return value == null ? Result<T>.Failure(error) : Result<T>.Success(value);
    }

    /// <summary>
    /// Executes an action if the result is successful
    /// </summary>
    public static Result<T> OnSuccess<T>(this Result<T> result, Action<T> action)
    {
        if (result.IsSuccess && result.Value != null)
            action(result.Value);
        return result;
    }

    /// <summary>
    /// Executes an action if the result is a failure
    /// </summary>
    public static Result<T> OnFailure<T>(this Result<T> result, Action<Error> action)
    {
        if (result.IsFailure && result.Error != null)
            action(result.Error);
        return result;
    }

    /// <summary>
    /// Maps a successful result to a new type
    /// </summary>
    public static Result<TOut> Map<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> mapper)
    {
        return result.IsSuccess && result.Value != null
            ? Result<TOut>.Success(mapper(result.Value))
            : Result<TOut>.Failure(result.Error!);
    }

    /// <summary>
    /// Binds a successful result to a new result-returning function
    /// </summary>
    public static async Task<Result<TOut>> Bind<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Task<Result<TOut>>> binder)
    {
        return result.IsSuccess && result.Value != null
            ? await binder(result.Value)
            : Result<TOut>.Failure(result.Error!);
    }

    /// <summary>
    /// Returns the value if successful, otherwise throws an exception
    /// </summary>
    public static T Unwrap<T>(this Result<T> result)
    {
        if (result.IsFailure)
            throw new InvalidOperationException($"Cannot unwrap failed result: {result.Error}");
        return result.Value!;
    }

    /// <summary>
    /// Returns the value if successful, otherwise returns the default value
    /// </summary>
    public static T UnwrapOr<T>(this Result<T> result, T defaultValue)
    {
        return result.IsSuccess && result.Value != null ? result.Value : defaultValue;
    }

    /// <summary>
    /// Returns the value if successful, otherwise computes and returns the default value
    /// </summary>
    public static T UnwrapOrElse<T>(this Result<T> result, Func<Error, T> defaultValueProvider)
    {
        return result.IsSuccess && result.Value != null
            ? result.Value
            : defaultValueProvider(result.Error!);
    }

    /// <summary>
    /// Converts a Task<T> to a Result<T>, catching exceptions
    /// </summary>
    public static async Task<Result<T>> ToResultAsync<T>(this Task<T> task, string errorCode = ErrorCodes.ExternalApiFailure)
    {
        try
        {
            var value = await task;
            return value != null
                ? Result<T>.Success(value)
                : Result<T>.Failure(errorCode, "Operation returned null value");
        }
        catch (Exception ex)
        {
            return Result<T>.Failure(
                new Error(errorCode, ex.Message, ex.StackTrace,
                    new Dictionary<string, object> { { "ExceptionType", ex.GetType().Name } }));
        }
    }

    /// <summary>
    /// Combines multiple results into a single result
    /// Returns success only if all results are successful
    /// </summary>
    public static Result Combine(params Result[] results)
    {
        foreach (var result in results)
        {
            if (result.IsFailure)
                return result;
        }
        return Result.Success();
    }

    /// <summary>
    /// Combines multiple results into a single result with aggregated errors
    /// </summary>
    public static Result CombineAll(params Result[] results)
    {
        var errors = results.Where(r => r.IsFailure).Select(r => r.Error).ToList();
        if (errors.Any())
        {
            var aggregatedMessage = string.Join("; ", errors.Select(e => e!.Message));
            return Result.Failure(ErrorCodes.ValidationFailed, "Multiple validation errors occurred", aggregatedMessage);
        }
        return Result.Success();
    }
}
