using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GrcMvc.Common.Validation
{
    using Result = GrcMvc.Common.Results.Result;
    using Error = GrcMvc.Common.Results.Error;
    using ErrorCode = GrcMvc.Common.Results.ErrorCode;
    /// <summary>
    /// Helper class for validating ModelState and converting to Result pattern
    /// </summary>
    public static class ModelStateValidator
    {
        /// <summary>
        /// Validates ModelState and returns a Result with validation errors if invalid
        /// </summary>
        public static Result ValidateModelState(ModelStateDictionary modelState)
        {
            if (modelState.IsValid)
            {
                return Result.Success();
            }

            var errors = modelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors.Select(e => $"{x.Key}: {e.ErrorMessage}"))
                .ToList();

            var errorMessage = "Validation failed";
            var errorDetails = string.Join("; ", errors);

            return Result.Failure(new Error(ErrorCode.ValidationError, errorMessage, errorDetails));
        }

        /// <summary>
        /// Validates ModelState and returns a Result&lt;T&gt; with validation errors if invalid
        /// </summary>
        public static GrcMvc.Common.Results.Result<T> ValidateModelState<T>(ModelStateDictionary modelState)
        {
            if (modelState.IsValid)
            {
                return GrcMvc.Common.Results.Result<T>.Failure(ErrorCode.InternalError, "ModelState is valid but no value provided");
            }

            var errors = modelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors.Select(e => $"{x.Key}: {e.ErrorMessage}"))
                .ToList();

            var errorMessage = "Validation failed";
            var errorDetails = string.Join("; ", errors);

            return GrcMvc.Common.Results.Result<T>.Failure(new Error(ErrorCode.ValidationError, errorMessage, errorDetails));
        }

        /// <summary>
        /// Gets validation errors as a dictionary
        /// </summary>
        public static Dictionary<string, List<string>> GetValidationErrors(ModelStateDictionary modelState)
        {
            return modelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    x => x.Key,
                    x => x.Value!.Errors.Select(e => e.ErrorMessage).ToList()
                );
        }

        /// <summary>
        /// Adds validation errors to ModelState
        /// </summary>
        public static void AddValidationErrors(ModelStateDictionary modelState, Dictionary<string, List<string>> errors)
        {
            foreach (var error in errors)
            {
                foreach (var message in error.Value)
                {
                    modelState.AddModelError(error.Key, message);
                }
            }
        }
    }
}
