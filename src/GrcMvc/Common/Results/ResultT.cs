namespace GrcMvc.Common.Results
{
    /// <summary>
    /// Represents the result of an operation with a return value
    /// </summary>
    /// <typeparam name="T">The type of the return value</typeparam>
    public class Result<T> : Result
    {
        /// <summary>
        /// The value returned by the operation (default if failed)
        /// </summary>
        public T? Value { get; }

        /// <summary>
        /// Protected constructor to enforce factory methods
        /// </summary>
        protected Result(T? value, bool isSuccess, Error? error) : base(isSuccess, error)
        {
            Value = value;
        }

        /// <summary>
        /// Creates a successful result with a value
        /// </summary>
        public static Result<T> Success(T value) => new Result<T>(value, true, null);

        /// <summary>
        /// Creates a failed result with an error
        /// </summary>
        public new static Result<T> Failure(Error error) => new Result<T>(default, false, error);

        /// <summary>
        /// Creates a failed result with error code and message
        /// </summary>
        public new static Result<T> Failure(string code, string message) 
            => new Result<T>(default, false, new Error(code, message));

        /// <summary>
        /// Creates a failed result with error code, message, and details
        /// </summary>
        public new static Result<T> Failure(string code, string message, string details) 
            => new Result<T>(default, false, new Error(code, message, details));

        /// <summary>
        /// Implicit conversion from T to Result<T> (for convenience)
        /// </summary>
        public static implicit operator Result<T>(T value) => Success(value);

        /// <summary>
        /// Implicit conversion from Error to Result<T> (for convenience)
        /// </summary>
        public static implicit operator Result<T>(Error error) => Failure(error);
    }
}
