using System.ComponentModel;

namespace GrcMvc.Models.Enums
{
    /// <summary>
    /// User lifecycle status states for Access Management controls.
    /// Implements AM-01 (Identity Proofing) and AM-05 (Lifecycle Management).
    /// </summary>
    public enum UserStatus
    {
        /// <summary>
        /// User signed up but email not yet verified (Flow 1: Self-Registration).
        /// Transition to Active requires email verification.
        /// </summary>
        [Description("Pending email verification")]
        PendingVerification = 0,

        /// <summary>
        /// User has been invited but hasn't accepted yet (Flow 3: Admin Invite).
        /// Transition to Active requires invitation acceptance + password set.
        /// </summary>
        [Description("Pending invitation acceptance")]
        PendingInvitation = 1,

        /// <summary>
        /// User verified but password not yet set (Flow 2: API Provision).
        /// Transition to Active requires password set.
        /// </summary>
        [Description("Pending password setup")]
        PendingPasswordSet = 2,

        /// <summary>
        /// User has full access to the system.
        /// Can be transitioned to Suspended or Deprovisioned.
        /// </summary>
        [Description("Active")]
        Active = 3,

        /// <summary>
        /// User temporarily blocked from accessing the system.
        /// Can be reactivated to Active state.
        /// </summary>
        [Description("Suspended")]
        Suspended = 4,

        /// <summary>
        /// User has been permanently deprovisioned.
        /// This is a terminal state - cannot be reactivated.
        /// All sessions revoked, workspace memberships removed.
        /// </summary>
        [Description("Deprovisioned")]
        Deprovisioned = 5,

        /// <summary>
        /// User suspended due to inactivity (configurable threshold).
        /// Can be reactivated upon next login attempt.
        /// </summary>
        [Description("Inactive - Suspended due to inactivity")]
        InactiveSuspended = 6
    }

    /// <summary>
    /// Valid state transitions for user lifecycle management.
    /// Enforces AM-01 and AM-05 controls.
    /// </summary>
    public static class UserStatusTransitions
    {
        /// <summary>
        /// Checks if a transition from one status to another is valid.
        /// </summary>
        public static bool IsValidTransition(UserStatus from, UserStatus to)
        {
            return (from, to) switch
            {
                // From PendingVerification
                (UserStatus.PendingVerification, UserStatus.Active) => true,           // Email verified
                (UserStatus.PendingVerification, UserStatus.Deprovisioned) => true,   // Cancelled before verification

                // From PendingInvitation
                (UserStatus.PendingInvitation, UserStatus.Active) => true,             // Invitation accepted
                (UserStatus.PendingInvitation, UserStatus.Deprovisioned) => true,     // Invitation cancelled

                // From PendingPasswordSet
                (UserStatus.PendingPasswordSet, UserStatus.Active) => true,            // Password set
                (UserStatus.PendingPasswordSet, UserStatus.Deprovisioned) => true,    // Account cancelled

                // From Active
                (UserStatus.Active, UserStatus.Suspended) => true,                     // Admin suspended
                (UserStatus.Active, UserStatus.InactiveSuspended) => true,            // Inactivity suspension
                (UserStatus.Active, UserStatus.Deprovisioned) => true,                // Terminated

                // From Suspended
                (UserStatus.Suspended, UserStatus.Active) => true,                     // Reactivated
                (UserStatus.Suspended, UserStatus.Deprovisioned) => true,             // Terminated

                // From InactiveSuspended
                (UserStatus.InactiveSuspended, UserStatus.Active) => true,            // Reactivated on login
                (UserStatus.InactiveSuspended, UserStatus.Deprovisioned) => true,    // Terminated

                // Deprovisioned is terminal - no transitions allowed
                (UserStatus.Deprovisioned, _) => false,

                // Same state is always valid (no-op)
                _ when from == to => true,

                // All other transitions are invalid
                _ => false
            };
        }

        /// <summary>
        /// Gets the reason why a transition is invalid.
        /// </summary>
        public static string GetInvalidTransitionReason(UserStatus from, UserStatus to)
        {
            if (from == UserStatus.Deprovisioned)
                return "Cannot transition from Deprovisioned status - this is a terminal state.";

            if (to == UserStatus.Active)
            {
                return from switch
                {
                    UserStatus.PendingVerification => "Email verification required before activation.",
                    UserStatus.PendingInvitation => "Invitation acceptance and password set required before activation.",
                    UserStatus.PendingPasswordSet => "Password must be set before activation.",
                    _ => $"Invalid transition from {from} to Active."
                };
            }

            return $"Invalid status transition from {from} to {to}.";
        }

        /// <summary>
        /// Gets the required action for a status to become Active.
        /// </summary>
        public static string GetActivationRequirement(UserStatus status)
        {
            return status switch
            {
                UserStatus.PendingVerification => "Complete email verification",
                UserStatus.PendingInvitation => "Accept invitation and set password",
                UserStatus.PendingPasswordSet => "Set password",
                UserStatus.Suspended => "Request reactivation from administrator",
                UserStatus.InactiveSuspended => "Log in to reactivate",
                UserStatus.Deprovisioned => "Account has been permanently terminated",
                UserStatus.Active => "Already active",
                _ => "Unknown requirement"
            };
        }

        /// <summary>
        /// Checks if a user with the given status can access the system.
        /// </summary>
        public static bool CanAccess(UserStatus status)
        {
            return status == UserStatus.Active;
        }

        /// <summary>
        /// Checks if a user with the given status is in a pending state.
        /// </summary>
        public static bool IsPending(UserStatus status)
        {
            return status == UserStatus.PendingVerification ||
                   status == UserStatus.PendingInvitation ||
                   status == UserStatus.PendingPasswordSet;
        }

        /// <summary>
        /// Checks if a user with the given status can be reactivated.
        /// </summary>
        public static bool CanReactivate(UserStatus status)
        {
            return status == UserStatus.Suspended ||
                   status == UserStatus.InactiveSuspended;
        }
    }
}
