namespace GrcMvc.Models.DTOs
{
    /// <summary>
    /// Result of MFA verification attempt.
    /// </summary>
    public class MfaVerificationResult
    {
        public bool Success { get; set; }
        public bool IsValid => Success; // Compatibility property
        public string? ErrorMessage { get; set; }
        public int AttemptsRemaining { get; set; }
        public bool MaxAttemptsExceeded { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? LockoutEnd { get; set; }
        
        // Factory methods for common results
        public static MfaVerificationResult Succeeded() => new() { Success = true };
        
        public static MfaVerificationResult Failed(string message, int? remaining = null) => new()
        {
            Success = false,
            ErrorMessage = message,
            AttemptsRemaining = remaining ?? 0
        };
        
        public static MfaVerificationResult LockedOut(DateTime lockoutEnd) => new()
        {
            Success = false,
            ErrorMessage = "Account temporarily locked due to too many failed attempts.",
            IsLocked = true,
            LockoutEnd = lockoutEnd,
            AttemptsRemaining = 0
        };
    }
}
