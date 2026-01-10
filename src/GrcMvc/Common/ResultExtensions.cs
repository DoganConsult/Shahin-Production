using Microsoft.AspNetCore.Mvc;

namespace GrcMvc.Common
{
    /// <summary>
    /// Extensions for converting Result to ActionResult in API controllers.
    /// ABP Best Practice: Consistent API responses with proper HTTP status codes.
    /// </summary>
    public static class ResultToActionResultExtensions
    {
        /// <summary>
        /// Converts a Result to an appropriate ActionResult.
        /// </summary>
        public static ActionResult ToActionResult(this Result result)
        {
            if (result.IsSuccess)
                return new OkResult();

            return result.ErrorCode switch
            {
                "NOT_FOUND" => new NotFoundObjectResult(new { error = result.Error }),
                "VALIDATION_ERROR" => new BadRequestObjectResult(new { error = result.Error, errors = result.Errors }),
                "UNAUTHORIZED" => new UnauthorizedObjectResult(new { error = result.Error }),
                "CONFLICT" => new ConflictObjectResult(new { error = result.Error }),
                _ => new BadRequestObjectResult(new { error = result.Error })
            };
        }

        /// <summary>
        /// Converts a Result<T> to an appropriate ActionResult with the value.
        /// </summary>
        public static ActionResult<T> ToActionResult<T>(this Result<T> result)
        {
            if (result.IsSuccess)
                return new OkObjectResult(result.Value);

            return result.ErrorCode switch
            {
                "NOT_FOUND" => new NotFoundObjectResult(new { error = result.Error }),
                "VALIDATION_ERROR" => new BadRequestObjectResult(new { error = result.Error, errors = result.Errors }),
                "UNAUTHORIZED" => new UnauthorizedObjectResult(new { error = result.Error }),
                "CONFLICT" => new ConflictObjectResult(new { error = result.Error }),
                _ => new BadRequestObjectResult(new { error = result.Error })
            };
        }

        /// <summary>
        /// Converts a Result<T> to ActionResult with a created response (201).
        /// </summary>
        public static ActionResult<T> ToCreatedResult<T>(this Result<T> result, string actionName, object routeValues)
        {
            if (result.IsSuccess)
                return new CreatedAtActionResult(actionName, null, routeValues, result.Value);

            return result.ToActionResult();
        }

        /// <summary>
        /// Converts a Result to NoContent (204) on success.
        /// </summary>
        public static ActionResult ToNoContentResult(this Result result)
        {
            if (result.IsSuccess)
                return new NoContentResult();

            return result.ToActionResult();
        }
    }
}
