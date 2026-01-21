using System;
using System.Collections.Generic;
using System.Linq;

namespace GrcMvc.Common
{
    /// <summary>
    /// Represents the result of an operation that can either succeed or fail.
    /// ABP Best Practice: Use Result pattern instead of throwing exceptions for expected failures.
    /// </summary>
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public string Error { get; }
        public string ErrorCode { get; }
        public IReadOnlyList<string> Errors { get; }

        protected Result(bool isSuccess, string error, string errorCode = "", IEnumerable<string>? errors = null)
        {
            if (isSuccess && !string.IsNullOrEmpty(error))
                throw new InvalidOperationException("Success result cannot have an error message");
            if (!isSuccess && string.IsNullOrEmpty(error) && (errors == null || !errors.Any()))
                throw new InvalidOperationException("Failure result must have an error message");

            IsSuccess = isSuccess;
            Error = error;
            ErrorCode = errorCode;
            Errors = errors?.ToList().AsReadOnly() ?? new List<string>().AsReadOnly();
        }

        public static Result Success() => new Result(true, string.Empty);
        
        public static Result Failure(string error, string errorCode = "") 
            => new Result(false, error, errorCode);
        
        public static Result Failure(IEnumerable<string> errors) 
            => new Result(false, errors.FirstOrDefault() ?? "Unknown error", "", errors);

        public static Result<T> Success<T>(T value) => Result<T>.Success(value);
        
        public static Result<T> Failure<T>(string error, string errorCode = "") 
            => Result<T>.Failure(error, errorCode);

        // Common error factory methods
        public static Result NotFound(string entityName, object id) 
            => Failure($"{entityName} with ID '{id}' was not found", "NOT_FOUND");
        
        public static Result<T> NotFound<T>(string entityName, object id) 
            => Result<T>.Failure($"{entityName} with ID '{id}' was not found", "NOT_FOUND");

        public static Result ValidationError(string message) 
            => Failure(message, "VALIDATION_ERROR");
        
        public static Result<T> ValidationError<T>(string message) 
            => Result<T>.Failure(message, "VALIDATION_ERROR");

        public static Result Unauthorized(string message = "You are not authorized to perform this action") 
            => Failure(message, "UNAUTHORIZED");
        
        public static Result<T> Unauthorized<T>(string message = "You are not authorized to perform this action") 
            => Result<T>.Failure(message, "UNAUTHORIZED");

        public static Result Conflict(string message) 
            => Failure(message, "CONFLICT");
        
        public static Result<T> Conflict<T>(string message) 
            => Result<T>.Failure(message, "CONFLICT");
    }

    /// <summary>
    /// Represents the result of an operation that returns a value.
    /// </summary>
    /// <typeparam name="T">The type of the value returned on success</typeparam>
    public class Result<T> : Result
    {
        private readonly T? _value;

        public T Value
        {
            get
            {
                if (IsFailure)
                    throw new InvalidOperationException("Cannot access Value of a failed result. Check IsSuccess first.");
                return _value!;
            }
        }

        protected Result(T? value, bool isSuccess, string error, string errorCode = "", IEnumerable<string>? errors = null)
            : base(isSuccess, error, errorCode, errors)
        {
            _value = value;
        }

        public static Result<T> Success(T value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value), "Success result value cannot be null");
            return new Result<T>(value, true, string.Empty);
        }

        public new static Result<T> Failure(string error, string errorCode = "") 
            => new Result<T>(default, false, error, errorCode);

        public new static Result<T> Failure(IEnumerable<string> errors) 
            => new Result<T>(default, false, errors.FirstOrDefault() ?? "Unknown error", "", errors);

        // Implicit conversion from T to Result<T> for convenience
        public static implicit operator Result<T>(T value) => Success(value);

        /// <summary>
        /// Maps the result value to a new type if successful.
        /// </summary>
        public Result<TNew> Map<TNew>(Func<T, TNew> mapper)
        {
            return IsSuccess 
                ? Result<TNew>.Success(mapper(Value)) 
                : Result<TNew>.Failure(Error, ErrorCode);
        }

        /// <summary>
        /// Executes an action on the value if successful.
        /// </summary>
        public Result<T> OnSuccess(Action<T> action)
        {
            if (IsSuccess)
                action(Value);
            return this;
        }

        /// <summary>
        /// Executes an action if the result is a failure.
        /// </summary>
        public Result<T> OnFailure(Action<string> action)
        {
            if (IsFailure)
                action(Error);
            return this;
        }

        /// <summary>
        /// Returns the value if successful, otherwise returns the default value.
        /// </summary>
        public T GetValueOrDefault(T defaultValue = default!)
        {
            return IsSuccess ? Value : defaultValue;
        }
    }

    /// <summary>
    /// Extension methods for Result pattern
    /// </summary>
    public static class ResultExtensions
    {
        /// <summary>
        /// Combines multiple results into a single result.
        /// </summary>
        public static Result Combine(params Result[] results)
        {
            var failures = results.Where(r => r.IsFailure).ToList();
            if (!failures.Any())
                return Result.Success();

            var errors = failures.SelectMany(f => 
                f.Errors.Any() ? f.Errors : new[] { f.Error });
            return Result.Failure(errors);
        }

        /// <summary>
        /// Converts a nullable value to a Result, treating null as a failure.
        /// </summary>
        public static Result<T> ToResult<T>(this T? value, string errorIfNull) where T : class
        {
            return value != null 
                ? Result<T>.Success(value) 
                : Result<T>.Failure(errorIfNull);
        }

        /// <summary>
        /// Converts a nullable value type to a Result.
        /// </summary>
        public static Result<T> ToResult<T>(this T? value, string errorIfNull) where T : struct
        {
            return value.HasValue 
                ? Result<T>.Success(value.Value) 
                : Result<T>.Failure(errorIfNull);
        }
    }
}
