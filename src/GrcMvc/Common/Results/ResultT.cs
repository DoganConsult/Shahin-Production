namespace GrcMvc.Common.Results;

/// <summary>
/// Represents the result of an operation with a return value
/// </summary>
public class Result<T> : Result
{
    public T? Value { get; }

    private Result(T? value, bool isSuccess, Error? error) : base(isSuccess, error)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a successful result with a value
    /// </summary>
    public static Result<T> Success(T value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value), "Successful result cannot have a null value");
        return new Result<T>(value, true, null);
    }

    /// <summary>
    /// Creates a failed result with an error
    /// </summary>
    public new static Result<T> Failure(Error error) => new(default, false, error);

    /// <summary>
    /// Creates a failed result with error code and message
    /// </summary>
    public static Result<T> Failure(string code, string message, string? details = null)
        => new(default, false, new Error(code, message, details));

    /// <summary>
    /// Implicitly converts a value to a successful Result<T>
    /// </summary>
    public static implicit operator Result<T>(T value) => Success(value);

    /// <summary>
    /// Implicitly converts an Error to a failed Result<T>
    /// </summary>
    public static implicit operator Result<T>(Error error) => Failure(error);
}
