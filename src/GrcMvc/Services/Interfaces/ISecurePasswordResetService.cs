namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// AM-08: Secure Password Reset Service interface.
    /// Implements secure token generation, rate limiting, and audit logging.
    /// </summary>
    public interface ISecurePasswordResetService
    {
        /// <summary>
        /// Request a password reset for an email address.
        /// Rate limited and audited.
        /// </summary>
        /// <param name="email">Email address to send reset link to</param>
        /// <param name="ipAddress">Requester's IP address</param>
        /// <param name="userAgent">Requester's user agent</param>
        /// <returns>Result indicating success/failure (always succeeds externally for security)</returns>
        Task<PasswordResetRequestResult> RequestResetAsync(string email, string? ipAddress, string? userAgent);

        /// <summary>
        /// Validate a password reset token.
        /// </summary>
        /// <param name="token">The reset token from the email link</param>
        /// <returns>Validation result with user info if valid</returns>
        Task<PasswordResetValidationResult> ValidateTokenAsync(string token);

        /// <summary>
        /// Complete the password reset with a new password.
        /// </summary>
        /// <param name="token">The reset token</param>
        /// <param name="newPassword">The new password</param>
        /// <param name="ipAddress">Requester's IP address</param>
        /// <returns>Result indicating success/failure</returns>
        Task<PasswordResetCompleteResult> CompleteResetAsync(string token, string newPassword, string? ipAddress);

        /// <summary>
        /// Check if password meets policy requirements.
        /// </summary>
        /// <param name="password">Password to validate</param>
        /// <returns>Validation result with any policy violations</returns>
        Task<PasswordPolicyResult> ValidatePasswordPolicyAsync(string password);

        /// <summary>
        /// Revoke all pending reset tokens for a user.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="reason">Reason for revocation</param>
        Task RevokeAllTokensAsync(Guid userId, string reason);

        /// <summary>
        /// Check if user is locked out from password reset.
        /// </summary>
        /// <param name="email">Email to check</param>
        /// <returns>Lockout status</returns>
        Task<PasswordResetLockoutStatus> GetLockoutStatusAsync(string email);

        /// <summary>
        /// Get reset request count for rate limiting.
        /// </summary>
        /// <param name="email">Email to check</param>
        /// <param name="windowHours">Time window in hours</param>
        /// <returns>Number of requests in the window</returns>
        Task<int> GetResetRequestCountAsync(string email, int windowHours = 1);
    }

    /// <summary>
    /// Result of a password reset request.
    /// </summary>
    public class PasswordResetRequestResult
    {
        /// <summary>
        /// Whether the request was processed (always true for security).
        /// </summary>
        public bool Processed { get; set; } = true;

        /// <summary>
        /// Internal status for logging (not exposed to user).
        /// </summary>
        public PasswordResetInternalStatus InternalStatus { get; set; }

        /// <summary>
        /// Message to display to user (generic for security).
        /// </summary>
        public string Message { get; set; } = "If an account exists with this email, a password reset link will be sent.";

        /// <summary>
        /// Rate limit remaining requests.
        /// </summary>
        public int? RemainingRequests { get; set; }

        public static PasswordResetRequestResult Success(int? remaining = null) => new()
        {
            InternalStatus = PasswordResetInternalStatus.EmailSent,
            RemainingRequests = remaining
        };

        public static PasswordResetRequestResult UserNotFound() => new()
        {
            InternalStatus = PasswordResetInternalStatus.UserNotFound
        };

        public static PasswordResetRequestResult RateLimited(int remaining = 0) => new()
        {
            InternalStatus = PasswordResetInternalStatus.RateLimited,
            RemainingRequests = remaining,
            Message = "Too many reset requests. Please try again later."
        };

        public static PasswordResetRequestResult AccountLocked() => new()
        {
            InternalStatus = PasswordResetInternalStatus.AccountLocked,
            Message = "This account is temporarily locked. Please contact support."
        };
    }

    /// <summary>
    /// Internal status for audit logging.
    /// </summary>
    public enum PasswordResetInternalStatus
    {
        EmailSent,
        UserNotFound,
        RateLimited,
        AccountLocked,
        AccountDeprovisioned,
        Error
    }

    /// <summary>
    /// Result of token validation.
    /// </summary>
    public class PasswordResetValidationResult
    {
        public bool IsValid { get; set; }
        public Guid? UserId { get; set; }
        public string? Email { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime? ExpiresAt { get; set; }

        public static PasswordResetValidationResult Valid(Guid userId, string email, DateTime expiresAt) => new()
        {
            IsValid = true,
            UserId = userId,
            Email = email,
            ExpiresAt = expiresAt
        };

        public static PasswordResetValidationResult Invalid(string error) => new()
        {
            IsValid = false,
            ErrorMessage = error
        };

        public static PasswordResetValidationResult Expired() => Invalid("This password reset link has expired.");
        public static PasswordResetValidationResult AlreadyUsed() => Invalid("This password reset link has already been used.");
        public static PasswordResetValidationResult Revoked() => Invalid("This password reset link is no longer valid.");
        public static PasswordResetValidationResult NotFound() => Invalid("Invalid password reset link.");
    }

    /// <summary>
    /// Result of password reset completion.
    /// </summary>
    public class PasswordResetCompleteResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string[]? PolicyViolations { get; set; }

        public static PasswordResetCompleteResult Succeeded() => new() { Success = true };

        public static PasswordResetCompleteResult Failed(string error) => new()
        {
            Success = false,
            ErrorMessage = error
        };

        public static PasswordResetCompleteResult PolicyFailed(string[] violations) => new()
        {
            Success = false,
            ErrorMessage = "Password does not meet policy requirements.",
            PolicyViolations = violations
        };
    }

    /// <summary>
    /// Result of password policy validation.
    /// </summary>
    public class PasswordPolicyResult
    {
        public bool IsValid { get; set; }
        public List<string> Violations { get; set; } = new();
        public bool IsBreached { get; set; }

        public static PasswordPolicyResult Valid() => new() { IsValid = true };

        public static PasswordPolicyResult Invalid(params string[] violations) => new()
        {
            IsValid = false,
            Violations = violations.ToList()
        };
    }

    /// <summary>
    /// Password reset lockout status.
    /// </summary>
    public class PasswordResetLockoutStatus
    {
        public bool IsLockedOut { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public int FailedAttempts { get; set; }
        public int RemainingAttempts { get; set; }
    }
}
