namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Service for tracking tenant usage and trial status
    /// </summary>
    public interface IUsageTrackingService
    {
        /// <summary>
        /// Check if the current tenant is in trial period
        /// </summary>
        bool IsInTrialPeriod();
        
        /// <summary>
        /// Get the number of days remaining in trial
        /// </summary>
        int GetTrialDaysRemaining();
        
        /// <summary>
        /// Check if trial has expired
        /// </summary>
        bool IsTrialExpired();
        
        /// <summary>
        /// Get current tenant ID
        /// </summary>
        Guid? GetCurrentTenantId();
        
        /// <summary>
        /// Get tenant subscription status
        /// </summary>
        string GetSubscriptionStatus();
    }
}
