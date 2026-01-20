using System.ComponentModel;

namespace GrcMvc.Models.Enums
{
    /// <summary>
    /// AM-04: Access Review status states.
    /// </summary>
    public enum AccessReviewStatus
    {
        /// <summary>
        /// Review created but not yet started.
        /// </summary>
        [Description("Draft")]
        Draft = 0,

        /// <summary>
        /// Review is in progress, accepting decisions.
        /// </summary>
        [Description("In Progress")]
        InProgress = 1,

        /// <summary>
        /// All decisions made, pending completion.
        /// </summary>
        [Description("Pending Completion")]
        PendingCompletion = 2,

        /// <summary>
        /// Review completed, decisions may be executed.
        /// </summary>
        [Description("Completed")]
        Completed = 3,

        /// <summary>
        /// Review cancelled before completion.
        /// </summary>
        [Description("Cancelled")]
        Cancelled = 4
    }

    /// <summary>
    /// AM-04: Access review item decision types.
    /// </summary>
    public enum AccessReviewDecision
    {
        /// <summary>
        /// Decision not yet made.
        /// </summary>
        [Description("Pending")]
        Pending = 0,

        /// <summary>
        /// Access certified - no change required.
        /// </summary>
        [Description("Certified")]
        Certified = 1,

        /// <summary>
        /// Access revoked - user's role will be removed.
        /// </summary>
        [Description("Revoked")]
        Revoked = 2,

        /// <summary>
        /// Access modified - user's role will be changed.
        /// </summary>
        [Description("Modified")]
        Modified = 3
    }

    /// <summary>
    /// Valid state transitions for access reviews.
    /// Enforces AM-04 controls.
    /// </summary>
    public static class AccessReviewTransitions
    {
        /// <summary>
        /// Checks if a transition from one status to another is valid.
        /// </summary>
        public static bool IsValidTransition(AccessReviewStatus from, AccessReviewStatus to)
        {
            return (from, to) switch
            {
                // From Draft
                (AccessReviewStatus.Draft, AccessReviewStatus.InProgress) => true,      // Started
                (AccessReviewStatus.Draft, AccessReviewStatus.Cancelled) => true,       // Cancelled before start

                // From InProgress
                (AccessReviewStatus.InProgress, AccessReviewStatus.PendingCompletion) => true,  // All decisions made
                (AccessReviewStatus.InProgress, AccessReviewStatus.Completed) => true,          // Directly completed
                (AccessReviewStatus.InProgress, AccessReviewStatus.Cancelled) => true,          // Cancelled

                // From PendingCompletion
                (AccessReviewStatus.PendingCompletion, AccessReviewStatus.Completed) => true,   // Confirmed completion
                (AccessReviewStatus.PendingCompletion, AccessReviewStatus.InProgress) => true,  // Reopened for changes

                // Completed and Cancelled are terminal
                (AccessReviewStatus.Completed, _) => false,
                (AccessReviewStatus.Cancelled, _) => false,

                // Same state is always valid (no-op)
                _ when from == to => true,

                // All other transitions are invalid
                _ => false
            };
        }

        /// <summary>
        /// Checks if decisions can be submitted for a review in this status.
        /// </summary>
        public static bool CanSubmitDecisions(AccessReviewStatus status)
        {
            return status == AccessReviewStatus.InProgress;
        }

        /// <summary>
        /// Checks if a review in this status can be completed.
        /// </summary>
        public static bool CanComplete(AccessReviewStatus status)
        {
            return status == AccessReviewStatus.InProgress ||
                   status == AccessReviewStatus.PendingCompletion;
        }

        /// <summary>
        /// Checks if decisions can be executed for a review in this status.
        /// </summary>
        public static bool CanExecuteDecisions(AccessReviewStatus status)
        {
            return status == AccessReviewStatus.Completed;
        }

        /// <summary>
        /// Gets a user-friendly description of the status.
        /// </summary>
        public static string GetStatusDescription(AccessReviewStatus status)
        {
            return status switch
            {
                AccessReviewStatus.Draft => "Review is in draft and has not been started.",
                AccessReviewStatus.InProgress => "Review is in progress, awaiting decisions.",
                AccessReviewStatus.PendingCompletion => "All decisions made, awaiting confirmation.",
                AccessReviewStatus.Completed => "Review completed.",
                AccessReviewStatus.Cancelled => "Review was cancelled.",
                _ => "Unknown status"
            };
        }
    }

    /// <summary>
    /// Validation helpers for access review decisions.
    /// </summary>
    public static class AccessReviewDecisionValidation
    {
        /// <summary>
        /// All valid decision values.
        /// </summary>
        public static readonly AccessReviewDecision[] ValidDecisions =
        {
            AccessReviewDecision.Certified,
            AccessReviewDecision.Revoked,
            AccessReviewDecision.Modified
        };

        /// <summary>
        /// Checks if a decision value is valid for submission.
        /// </summary>
        public static bool IsValidDecision(AccessReviewDecision decision)
        {
            return decision == AccessReviewDecision.Certified ||
                   decision == AccessReviewDecision.Revoked ||
                   decision == AccessReviewDecision.Modified;
        }

        /// <summary>
        /// Checks if a decision requires a new role to be specified.
        /// </summary>
        public static bool RequiresNewRole(AccessReviewDecision decision)
        {
            return decision == AccessReviewDecision.Modified;
        }

        /// <summary>
        /// Checks if a decision requires execution (role change).
        /// </summary>
        public static bool RequiresExecution(AccessReviewDecision decision)
        {
            return decision == AccessReviewDecision.Revoked ||
                   decision == AccessReviewDecision.Modified;
        }
    }
}
