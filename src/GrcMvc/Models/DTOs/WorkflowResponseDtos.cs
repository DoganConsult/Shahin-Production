using System;
using System.Collections.Generic;
using GrcMvc.Exceptions;

namespace GrcMvc.Models.DTOs
{
    /// <summary>
    /// Standard API response wrapper for all workflow operations.
    /// Provides consistent error handling and response format across all workflow controllers.
    /// </summary>
    /// <typeparam name="T">The type of data being returned</typeparam>
    public class WorkflowApiResponse<T>
    {
        /// <summary>Whether the operation succeeded</summary>
        public bool Success { get; set; }
        
        /// <summary>The response data (null if failed)</summary>
        public T? Data { get; set; }
        
        /// <summary>Error message if failed</summary>
        public string? Error { get; set; }
        
        /// <summary>Error code for client-side handling</summary>
        public string? ErrorCode { get; set; }
        
        /// <summary>Additional error details or validation errors</summary>
        public List<string>? Details { get; set; }
        
        /// <summary>Timestamp of the response</summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>Create a success response with data</summary>
        public static WorkflowApiResponse<T> Ok(T data)
        {
            return new WorkflowApiResponse<T>
            {
                Success = true,
                Data = data,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>Create a failure response with error details</summary>
        public static WorkflowApiResponse<T> Fail(string error, string? errorCode = null, List<string>? details = null)
        {
            return new WorkflowApiResponse<T>
            {
                Success = false,
                Error = error,
                ErrorCode = errorCode ?? "WORKFLOW_ERROR",
                Details = details,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>Create a not found response</summary>
        public static WorkflowApiResponse<T> NotFound(string message)
        {
            return new WorkflowApiResponse<T>
            {
                Success = false,
                Error = message,
                ErrorCode = "NOT_FOUND",
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>Create an invalid state transition response</summary>
        public static WorkflowApiResponse<T> InvalidTransition(string from, string to, IEnumerable<string>? validTargets = null)
        {
            var details = validTargets != null 
                ? new List<string> { $"Valid transitions from '{from}': {string.Join(", ", validTargets)}" }
                : null;
                
            return new WorkflowApiResponse<T>
            {
                Success = false,
                Error = $"Invalid state transition from '{from}' to '{to}'",
                ErrorCode = "INVALID_STATE_TRANSITION",
                Details = details,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>Create an unauthorized response</summary>
        public static WorkflowApiResponse<T> Unauthorized(string message)
        {
            return new WorkflowApiResponse<T>
            {
                Success = false,
                Error = message,
                ErrorCode = "UNAUTHORIZED",
                Timestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Non-generic version for operations that don't return data.
    /// </summary>
    public class WorkflowApiResponse : WorkflowApiResponse<object>
    {
        /// <summary>Create a success response without data</summary>
        public static WorkflowApiResponse Ok()
        {
            return new WorkflowApiResponse
            {
                Success = true,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>Create a success response with data</summary>
        public static new WorkflowApiResponse Ok(object data)
        {
            return new WorkflowApiResponse
            {
                Success = true,
                Data = data,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>Create a failure response with error details</summary>
        public static new WorkflowApiResponse Fail(string error, string? errorCode = null, List<string>? details = null)
        {
            return new WorkflowApiResponse
            {
                Success = false,
                Error = error,
                ErrorCode = errorCode ?? "WORKFLOW_ERROR",
                Details = details,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>Create a not found response</summary>
        public static new WorkflowApiResponse NotFound(string message)
        {
            return new WorkflowApiResponse
            {
                Success = false,
                Error = message,
                ErrorCode = "NOT_FOUND",
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>Create an invalid state transition response</summary>
        public static new WorkflowApiResponse InvalidTransition(string from, string to, IEnumerable<string>? validTargets = null)
        {
            var details = validTargets != null 
                ? new List<string> { $"Valid transitions from '{from}': {string.Join(", ", validTargets)}" }
                : null;
                
            return new WorkflowApiResponse
            {
                Success = false,
                Error = $"Invalid state transition from '{from}' to '{to}'",
                ErrorCode = "INVALID_STATE_TRANSITION",
                Details = details,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    // NOTE: WorkflowErrorCodes is defined in GrcMvc.Exceptions.WorkflowErrorCodes
    // Use: using GrcMvc.Exceptions; to access error codes
}
