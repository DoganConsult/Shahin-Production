namespace GrcMvc.Common.Guards
{
    using Result = GrcMvc.Common.Results.Result;
    using Error = GrcMvc.Common.Results.Error;
    using ErrorCode = GrcMvc.Common.Results.ErrorCode;
    /// <summary>
    /// Guard clauses for common validation scenarios
    /// </summary>
    public static class Guard
    {
        /// <summary>
        /// Ensures the value is not null
        /// </summary>
        public static T AgainstNull<T>(T? value, string parameterName) where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
            return value;
        }

        /// <summary>
        /// Ensures the string is not null or empty
        /// </summary>
        public static string AgainstNullOrEmpty(string? value, string parameterName)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("Value cannot be null or empty", parameterName);
            }
            return value;
        }

        /// <summary>
        /// Ensures the string is not null or whitespace
        /// </summary>
        public static string AgainstNullOrWhiteSpace(string? value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Value cannot be null or whitespace", parameterName);
            }
            return value;
        }

        /// <summary>
        /// Ensures the GUID is not empty
        /// </summary>
        public static Guid AgainstEmptyGuid(Guid value, string parameterName)
        {
            if (value == Guid.Empty)
            {
                throw new ArgumentException("GUID cannot be empty", parameterName);
            }
            return value;
        }

        /// <summary>
        /// Ensures the collection is not null or empty
        /// </summary>
        public static IEnumerable<T> AgainstNullOrEmpty<T>(IEnumerable<T>? value, string parameterName)
        {
            if (value == null || !value.Any())
            {
                throw new ArgumentException("Collection cannot be null or empty", parameterName);
            }
            return value;
        }

        /// <summary>
        /// Ensures the value is within a valid range
        /// </summary>
        public static T AgainstOutOfRange<T>(T value, T min, T max, string parameterName) where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, $"Value must be between {min} and {max}");
            }
            return value;
        }

        /// <summary>
        /// Ensures the value is positive
        /// </summary>
        public static int AgainstNegative(int value, string parameterName)
        {
            if (value < 0)
            {
                throw new ArgumentException("Value cannot be negative", parameterName);
            }
            return value;
        }

        /// <summary>
        /// Ensures the value is positive (non-zero)
        /// </summary>
        public static int AgainstNegativeOrZero(int value, string parameterName)
        {
            if (value <= 0)
            {
                throw new ArgumentException("Value must be positive", parameterName);
            }
            return value;
        }

        /// <summary>
        /// Returns a Result&lt;T&gt; if value is not null, otherwise returns a NotFound error
        /// </summary>
        public static GrcMvc.Common.Results.Result<T> NotNull<T>(T? value, string entityName, object id) where T : class
        {
            return value != null 
                ? GrcMvc.Common.Results.Result<T>.Success(value) 
                : GrcMvc.Common.Results.Result<T>.Failure(Error.NotFound(entityName, id));
        }

        /// <summary>
        /// Returns a Result if condition is true, otherwise returns an error
        /// </summary>
        public static Result Against(bool condition, string errorCode, string errorMessage)
        {
            return condition 
                ? Result.Failure(errorCode, errorMessage) 
                : Result.Success();
        }

        /// <summary>
        /// Returns a Result if condition is true, otherwise returns an error
        /// </summary>
        public static Result Against(bool condition, string errorCode, string errorMessage, string details)
        {
            return condition 
                ? Result.Failure(errorCode, errorMessage, details) 
                : Result.Success();
        }

        /// <summary>
        /// Validates that a string matches a pattern
        /// </summary>
        public static GrcMvc.Common.Results.Result<string> AgainstInvalidFormat(string value, string pattern, string parameterName)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(value, pattern))
            {
                return GrcMvc.Common.Results.Result<string>.Failure(
                    new Error(ErrorCode.ValidationError, 
                        $"Invalid format for {parameterName}",
                        $"Value '{value}' does not match required pattern"));
            }
            return GrcMvc.Common.Results.Result<string>.Success(value);
        }
    }
}
