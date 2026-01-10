using Microsoft.AspNetCore.Hosting;

namespace GrcMvc.Models
{
    /// <summary>
    /// Generic API response wrapper for standardizing all API responses
    /// </summary>
    /// <typeparam name="T">The type of data being returned</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Indicates whether the request was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// The actual data returned by the API
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Message providing additional context (error or success message)
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// HTTP status code for the response
        /// </summary>
        public int StatusCode { get; set; } = 200;

        /// <summary>
        /// Creates a successful response with data
        /// </summary>
        public static ApiResponse<T> SuccessResponse(T? data, string message = "Success")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Message = message,
                StatusCode = 200
            };
        }

        /// <summary>
        /// Creates a failed response with error message
        /// </summary>
        public static ApiResponse<T> ErrorResponse(string message, int statusCode = 400)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Data = default,
                Message = message,
                StatusCode = statusCode
            };
        }

        /// <summary>
        /// Creates a safe error response for exceptions (no internal details exposed)
        /// SECURITY: Use this instead of ErrorResponse(ex.Message) to avoid leaking internal details
        /// </summary>
        public static ApiResponse<T> SafeErrorResponse(Exception ex, IWebHostEnvironment? environment = null)
        {
            var isDevelopment = environment?.EnvironmentName == "Development";
            var message = isDevelopment ? ex.Message : "An error occurred processing your request.";
            return new ApiResponse<T>
            {
                Success = false,
                Data = default,
                Message = message,
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// Non-generic API response for endpoints that don't return data
    /// </summary>
    public class ApiResponse
    {
        /// <summary>
        /// Indicates whether the request was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Message providing additional context (error or success message)
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// HTTP status code for the response
        /// </summary>
        public int StatusCode { get; set; } = 200;

        /// <summary>
        /// Creates a successful response
        /// </summary>
        public static ApiResponse SuccessResponse(string message = "Success")
        {
            return new ApiResponse
            {
                Success = true,
                Message = message,
                StatusCode = 200
            };
        }

        /// <summary>
        /// Creates a failed response with error message
        /// </summary>
        public static ApiResponse ErrorResponse(string message, int statusCode = 400)
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
                StatusCode = statusCode
            };
        }

        /// <summary>
        /// Creates a safe error response for exceptions (no internal details exposed)
        /// SECURITY: Use this instead of ErrorResponse(ex.Message) to avoid leaking internal details
        /// </summary>
        public static ApiResponse SafeErrorResponse(Exception ex, IWebHostEnvironment? environment = null)
        {
            var isDevelopment = environment?.EnvironmentName == "Development";
            var message = isDevelopment ? ex.Message : "An error occurred processing your request.";
            return new ApiResponse
            {
                Success = false,
                Message = message,
                StatusCode = 500
            };
        }
    }
}
