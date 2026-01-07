namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Service to check if owner exists and handle one-time owner setup
    /// </summary>
    public interface IOwnerSetupService
    {
        /// <summary>
        /// Check if any owner (PlatformAdmin or Owner role) exists in the database
        /// </summary>
        Task<bool> OwnerExistsAsync();

        /// <summary>
        /// Create the first owner account (one-time setup)
        /// </summary>
        Task<(bool Success, string? ErrorMessage, string? UserId)> CreateFirstOwnerAsync(
            string email,
            string password,
            string firstName,
            string lastName,
            string? organizationName = null);

        /// <summary>
        /// Check if owner setup is required (no owner exists)
        /// </summary>
        Task<bool> IsOwnerSetupRequiredAsync();
    }
}
