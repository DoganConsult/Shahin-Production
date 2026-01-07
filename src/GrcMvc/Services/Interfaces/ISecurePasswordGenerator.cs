using System.Security.Cryptography;

namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Service for generating cryptographically secure passwords
/// </summary>
public interface ISecurePasswordGenerator
{
    /// <summary>
    /// Generate a cryptographically secure random password
    /// </summary>
    string GeneratePassword(int length = 18);
    
    /// <summary>
    /// Generate a secure password with specific character requirements
    /// </summary>
    string GeneratePasswordWithRequirements(
        int length = 18,
        bool includeUppercase = true,
        bool includeLowercase = true,
        bool includeNumbers = true,
        bool includeSymbols = true);
}
