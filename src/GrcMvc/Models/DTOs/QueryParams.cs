using System;
using System.Collections.Generic;

namespace GrcMvc.Models.DTOs
{
    /// <summary>
    /// Query parameters for pagination
    /// </summary>
    public class PaginationParams
    {
        public int Page { get; set; } = 1;
        public int Size { get; set; } = 10;
        public int Skip => (Page - 1) * Size;
        public int Take => Size;
    }

    /// <summary>
    /// Query parameters for sorting
    /// </summary>
    public class SortParams
    {
        public string? SortBy { get; set; }
        public string Order { get; set; } = "asc"; // asc or desc
    }

    /// <summary>
    /// Query parameters for filtering
    /// </summary>
    public class FilterParams
    {
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public string? Category { get; set; }
        public string? Type { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    /// <summary>
    /// Query parameters for search
    /// </summary>
    public class SearchParams
    {
        public string? Query { get; set; }
        public List<string>? SearchFields { get; set; }
    }

    /// <summary>
    /// Paginated response wrapper
    /// </summary>
    public class PaginatedResponse<T>
    {
        public List<T>? Items { get; set; }
        public int Page { get; set; }
        public int Size { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages => (TotalItems + Size - 1) / Size;
        public bool HasNextPage => Page < TotalPages;
        public bool HasPreviousPage => Page > 1;
    }

    /// <summary>
    /// Bulk operation request
    /// </summary>
    public class BulkOperationRequest
    {
        public string Operation { get; set; } = ""; // create, update, delete
        public List<dynamic>? Items { get; set; }
    }

    /// <summary>
    /// Bulk operation result
    /// </summary>
    public class BulkOperationResult
    {
        public int TotalItems { get; set; }
        public int SuccessfulItems { get; set; }
        public int FailedItems { get; set; }
        public List<string>? Errors { get; set; }
        public DateTime CompletedAt { get; set; }
    }
}
