using GrcMvc.Models.Entities;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// AM-04/AM-11: Access Review Service interface for periodic access certification.
    /// </summary>
    public interface IAccessReviewService
    {
        /// <summary>
        /// Create a new access review for a tenant.
        /// </summary>
        Task<AccessReviewResult> CreateReviewAsync(
            Guid tenantId,
            Guid initiatedBy,
            string reviewType,
            string? name = null,
            string? description = null,
            int? dueDays = null);

        /// <summary>
        /// Start an access review (move from Draft to InProgress).
        /// </summary>
        Task<bool> StartReviewAsync(Guid reviewId, Guid startedBy);

        /// <summary>
        /// Get access review details with items.
        /// </summary>
        Task<AccessReviewDto?> GetReviewAsync(Guid reviewId, Guid tenantId);

        /// <summary>
        /// List all access reviews for a tenant.
        /// </summary>
        Task<AccessReviewListResult> ListReviewsAsync(
            Guid tenantId,
            string? status = null,
            int page = 1,
            int pageSize = 20);

        /// <summary>
        /// Submit a decision for a review item.
        /// </summary>
        Task<bool> SubmitDecisionAsync(
            Guid itemId,
            Guid reviewedBy,
            string decision,
            string? newRole = null,
            string? justification = null);

        /// <summary>
        /// Complete an access review.
        /// </summary>
        Task<bool> CompleteReviewAsync(Guid reviewId, Guid completedBy);

        /// <summary>
        /// Send reminder notifications for overdue reviews.
        /// </summary>
        Task SendRemindersAsync();

        /// <summary>
        /// Execute decisions (revoke/modify roles) for completed reviews.
        /// </summary>
        Task ExecuteDecisionsAsync(Guid reviewId);
    }

    public class AccessReviewResult
    {
        public bool Success { get; set; }
        public Guid ReviewId { get; set; }
        public int ItemCount { get; set; }
        public string? ErrorMessage { get; set; }

        public static AccessReviewResult Succeeded(Guid reviewId, int itemCount)
        {
            return new AccessReviewResult
            {
                Success = true,
                ReviewId = reviewId,
                ItemCount = itemCount
            };
        }

        public static AccessReviewResult Failed(string errorMessage)
        {
            return new AccessReviewResult
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }
    }

    public class AccessReviewDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ReviewType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int TotalItems { get; set; }
        public int ReviewedItems { get; set; }
        public int CertifiedItems { get; set; }
        public int RevokedItems { get; set; }
        public int ModifiedItems { get; set; }
        public double CompletionPercentage { get; set; }
        public bool IsOverdue { get; set; }
        public List<AccessReviewItemDto> Items { get; set; } = new();
    }

    public class AccessReviewItemDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? UserEmail { get; set; }
        public string? UserDisplayName { get; set; }
        public string CurrentRole { get; set; } = string.Empty;
        public string? UserStatus { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public int? InactiveDays { get; set; }
        public string? InclusionReason { get; set; }
        public string Decision { get; set; } = "Pending";
        public string? NewRole { get; set; }
        public string? Justification { get; set; }
        public Guid? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public bool IsExecuted { get; set; }
        public DateTime? ExecutedAt { get; set; }
        public string? ExecutionError { get; set; }
    }

    public class AccessReviewListResult
    {
        public List<AccessReviewDto> Reviews { get; set; } = new();
        public int TotalCount { get; set; }
    }
}
