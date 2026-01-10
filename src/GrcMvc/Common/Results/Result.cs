namespace GrcMvc.Common.Results;

/// <summary>
/// Represents the result of an operation without a return value
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error? Error { get; }

    protected Result(bool isSuccess, Error? error)
    {
        if (isSuccess && error != null)
            throw new InvalidOperationException("Successful result cannot have an error");
        if (!isSuccess && error == null)
            throw new InvalidOperationException("Failed result must have an error");

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Creates a successful result
    /// </summary>
    public static Result Success() => new(true, null);

    /// <summary>
    /// Creates a failed result with an error
    /// </summary>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>
    /// Creates a failed result with error code and message
    /// </summary>
    public static Result Failure(string code, string message, string? details = null)
        => new(false, new Error(code, message, details));

    /// <summary>
    /// Implicitly converts a Result to a boolean indicating success
    /// </summary>
    public static implicit operator bool(Result result) => result.IsSuccess;
}
