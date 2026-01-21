namespace GrcMvc.Configuration;

/// <summary>
/// Centralized assessment configuration
/// Single source of truth for assessment statuses and scoring
/// </summary>
public static class AssessmentConfiguration
{
    /// <summary>
    /// Valid assessment statuses
    /// </summary>
    public static class Statuses
    {
        public const string Draft = "Draft";
        public const string Scheduled = "Scheduled";
        public const string InProgress = "InProgress";
        public const string Submitted = "Submitted";
        public const string UnderReview = "UnderReview";
        public const string Approved = "Approved";
        public const string Completed = "Completed";
        public const string Cancelled = "Cancelled";
        public const string Rejected = "Rejected";

        public static readonly string[] AllStatuses =
        {
            Draft, Scheduled, InProgress, Submitted, UnderReview, Approved, Completed, Cancelled, Rejected
        };

        /// <summary>
        /// Statuses considered as "pending" (in progress)
        /// </summary>
        public static readonly string[] PendingStatuses =
        {
            Draft, Scheduled, InProgress, Submitted, UnderReview
        };

        /// <summary>
        /// Statuses considered as "completed" (final)
        /// </summary>
        public static readonly string[] CompletedStatuses =
        {
            Completed, Approved
        };

        /// <summary>
        /// Check if status is pending
        /// </summary>
        public static bool IsPending(string status)
        {
            return System.Array.Exists(PendingStatuses, s => 
                s.Equals(status, System.StringComparison.OrdinalIgnoreCase) ||
                s.Replace(" ", "").Equals(status.Replace(" ", ""), System.StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Check if status is completed
        /// </summary>
        public static bool IsCompleted(string status)
        {
            return System.Array.Exists(CompletedStatuses, s => 
                s.Equals(status, System.StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Normalize status string (handles legacy variants like "In Progress")
        /// </summary>
        public static string Normalize(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return Draft;

            var normalized = status.Replace(" ", "").Trim();
            
            return normalized.ToLower() switch
            {
                "draft" => Draft,
                "scheduled" => Scheduled,
                "inprogress" => InProgress,
                "submitted" => Submitted,
                "underreview" => UnderReview,
                "approved" => Approved,
                "completed" => Completed,
                "cancelled" => Cancelled,
                "rejected" => Rejected,
                _ => status // Return original if not recognized
            };
        }
    }

    /// <summary>
    /// Valid state transitions
    /// </summary>
    public static class Transitions
    {
        public static readonly (string From, string[] To)[] ValidTransitions =
        {
            (Statuses.Draft, new[] { Statuses.Scheduled, Statuses.InProgress, Statuses.Cancelled }),
            (Statuses.Scheduled, new[] { Statuses.InProgress, Statuses.Cancelled }),
            (Statuses.InProgress, new[] { Statuses.Submitted, Statuses.Cancelled }),
            (Statuses.Submitted, new[] { Statuses.UnderReview, Statuses.Rejected, Statuses.Approved }),
            (Statuses.UnderReview, new[] { Statuses.Approved, Statuses.Rejected }),
            (Statuses.Rejected, new[] { Statuses.Draft, Statuses.Cancelled }),
            (Statuses.Approved, new[] { Statuses.Completed }),
            (Statuses.Completed, System.Array.Empty<string>()), // Terminal state
            (Statuses.Cancelled, System.Array.Empty<string>())  // Terminal state
        };

        /// <summary>
        /// Check if transition from one status to another is valid
        /// </summary>
        public static bool IsValidTransition(string from, string to)
        {
            var normalizedFrom = Statuses.Normalize(from);
            var normalizedTo = Statuses.Normalize(to);

            foreach (var (fromStatus, toStatuses) in ValidTransitions)
            {
                if (fromStatus == normalizedFrom)
                {
                    return System.Array.Exists(toStatuses, s => s == normalizedTo);
                }
            }
            return false;
        }

        /// <summary>
        /// Get valid transitions from a given status
        /// </summary>
        public static string[] GetValidTransitions(string from)
        {
            var normalizedFrom = Statuses.Normalize(from);

            foreach (var (fromStatus, toStatuses) in ValidTransitions)
            {
                if (fromStatus == normalizedFrom)
                {
                    return toStatuses;
                }
            }
            return System.Array.Empty<string>();
        }
    }

    /// <summary>
    /// Score thresholds for assessment results
    /// </summary>
    public static class Scoring
    {
        public const int ExcellentMin = 90;
        public const int GoodMin = 70;
        public const int NeedsImprovementMin = 50;
        // Below 50 = Poor

        public static string GetGrade(int score)
        {
            return score switch
            {
                >= ExcellentMin => "Excellent",
                >= GoodMin => "Good",
                >= NeedsImprovementMin => "Needs Improvement",
                _ => "Poor"
            };
        }

        public static string GetGradeColor(int score)
        {
            return score switch
            {
                >= ExcellentMin => "#28a745", // Green
                >= GoodMin => "#ffc107",      // Yellow
                >= NeedsImprovementMin => "#fd7e14", // Orange
                _ => "#dc3545"                // Red
            };
        }
    }
}
