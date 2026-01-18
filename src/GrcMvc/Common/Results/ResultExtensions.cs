using Microsoft.AspNetCore.Mvc;

namespace GrcMvc.Common.Results
{
    /// <summary>
    /// Extension methods for working with Result types
    /// </summary>
    public static class ResultExtensions
    {
        /// <summary>
        /// Converts a Result to an IActionResult for API responses
        /// </summary>
        public static IActionResult ToActionResult(this Result result)
        {
            if (result.IsSuccess)
            {
                return new OkResult();
            }

            return result.Error!.Code switch
            {
                ErrorCode.NotFound => new NotFoundObjectResult(new { error = result.Error.Message, details = result.Error.Details }),
                ErrorCode.ValidationError => new BadRequestObjectResult(new { error = result.Error.Message, details = result.Error.Details }),
                ErrorCode.Unauthorized => new UnauthorizedObjectResult(new { error = result.Error.Message, details = result.Error.Details }),
                ErrorCode.Forbidden => new ObjectResult(new { error = result.Error.Message, details = result.Error.Details }) { StatusCode = 403 },
                ErrorCode.Conflict => new ConflictObjectResult(new { error = result.Error.Message, details = result.Error.Details }),
                ErrorCode.BadRequest => new BadRequestObjectResult(new { error = result.Error.Message, details = result.Error.Details }),
                _ => new ObjectResult(new { error = result.Error.Message, details = result.Error.Details }) { StatusCode = 500 }
            };
        }

        /// <summary>
        /// Converts a Result<T> to an IActionResult for API responses
        /// </summary>
        public static IActionResult ToActionResult<T>(this Result<T> result)
        {
            if (result.IsSuccess)
            {
                return new OkObjectResult(result.Value);
            }

            return result.Error!.Code switch
            {
                ErrorCode.NotFound => new NotFoundObjectResult(new { error = result.Error.Message, details = result.Error.Details }),
                ErrorCode.ValidationError => new BadRequestObjectResult(new { error = result.Error.Message, details = result.Error.Details }),
                ErrorCode.Unauthorized => new UnauthorizedObjectResult(new { error = result.Error.Message, details = result.Error.Details }),
                ErrorCode.Forbidden => new ObjectResult(new { error = result.Error.Message, details = result.Error.Details }) { StatusCode = 403 },
                ErrorCode.Conflict => new ConflictObjectResult(new { error = result.Error.Message, details = result.Error.Details }),
                ErrorCode.BadRequest => new BadRequestObjectResult(new { error = result.Error.Message, details = result.Error.Details }),
                _ => new ObjectResult(new { error = result.Error.Message, details = result.Error.Details }) { StatusCode = 500 }
            };
        }

        /// <summary>
        /// Executes an action if the result is successful
        /// </summary>
        public static Result OnSuccess(this Result result, Action action)
        {
            if (result.IsSuccess)
            {
                action();
            }
            return result;
        }

        /// <summary>
        /// Executes an action if the result is successful
        /// </summary>
        public static Result<T> OnSuccess<T>(this Result<T> result, Action<T> action)
        {
            if (result.IsSuccess && result.Value != null)
            {
                action(result.Value);
            }
            return result;
        }

        /// <summary>
        /// Executes an action if the result is a failure
        /// </summary>
        public static Result OnFailure(this Result result, Action<Error> action)
        {
            if (result.IsFailure && result.Error != null)
            {
                action(result.Error);
            }
            return result;
        }

        /// <summary>
        /// Executes an action if the result is a failure
        /// </summary>
        public static Result<T> OnFailure<T>(this Result<T> result, Action<Error> action)
        {
            if (result.IsFailure && result.Error != null)
            {
                action(result.Error);
            }
            return result;
        }

        /// <summary>
        /// Maps a successful result to a new type
        /// </summary>
        public static Result<TOut> Map<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> mapper)
        {
            if (result.IsFailure)
            {
                return Result<TOut>.Failure(result.Error!);
            }

            if (result.Value == null)
            {
                return Result<TOut>.Failure(ErrorCode.InternalError, "Cannot map null value");
            }

            return Result<TOut>.Success(mapper(result.Value));
        }

        /// <summary>
        /// Chains multiple operations that return Result
        /// </summary>
        public static async Task<Result<TOut>> BindAsync<TIn, TOut>(
            this Result<TIn> result, 
            Func<TIn, Task<Result<TOut>>> func)
        {
            if (result.IsFailure)
            {
                return Result<TOut>.Failure(result.Error!);
            }

            if (result.Value == null)
            {
                return Result<TOut>.Failure(ErrorCode.InternalError, "Cannot bind null value");
            }

            return await func(result.Value);
        }

        /// <summary>
        /// Matches the result to one of two functions based on success/failure
        /// </summary>
        public static TOut Match<TIn, TOut>(
            this Result<TIn> result,
            Func<TIn, TOut> onSuccess,
            Func<Error, TOut> onFailure)
        {
            return result.IsSuccess && result.Value != null
                ? onSuccess(result.Value)
                : onFailure(result.Error!);
        }

        /// <summary>
        /// Ensures a condition is met, otherwise returns a failure
        /// </summary>
        public static Result<T> Ensure<T>(
            this Result<T> result,
            Func<T, bool> predicate,
            Error error)
        {
            if (result.IsFailure)
            {
                return result;
            }

            if (result.Value == null)
            {
                return Result<T>.Failure(ErrorCode.InternalError, "Cannot ensure on null value");
            }

            return predicate(result.Value)
                ? result
                : Result<T>.Failure(error);
        }
    }
}
