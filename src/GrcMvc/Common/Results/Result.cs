namespace GrcMvc.Common.Results
{
    /// <summary>
    /// Represents the result of an operation without a return value
    /// </summary>
    public class Result
    {
        /// <summary>
        /// Indicates whether the operation was successful
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Indicates whether the operation failed
        /// </summary>
        public bool IsFailure => !IsSuccess;

        /// <summary>
        /// The error that occurred during the operation (null if successful)
        /// </summary>
        public Error? Error { get; }

        /// <summary>
        /// Protected constructor to enforce factory methods
        /// </summary>
        protected Result(bool isSuccess, Error? error)
        {
            if (isSuccess && error != null)
            {
                throw new InvalidOperationException("A successful result cannot have an error");
            }

            if (!isSuccess && error == null)
            {
                throw new InvalidOperationException("A failed result must have an error");
            }

            IsSuccess = isSuccess;
            Error = error;
        }

        /// <summary>
        /// Creates a successful result
        /// </summary>
        public static Result Success() => new Result(true, null);

        /// <summary>
        /// Creates a failed result with an error
        /// </summary>
        public static Result Failure(Error error) => new Result(false, error);

        /// <summary>
        /// Creates a failed result with error code and message
        /// </summary>
        public static Result Failure(string code, string message) 
            => new Result(false, new Error(code, message));

        /// <summary>
        /// Creates a failed result with error code, message, and details
        /// </summary>
        public static Result Failure(string code, string message, string details) 
            => new Result(false, new Error(code, message, details));

        /// <summary>
        /// Implicit conversion from Result to bool (for convenience)
        /// </summary>
        public static implicit operator bool(Result result) => result.IsSuccess;
    }
}
