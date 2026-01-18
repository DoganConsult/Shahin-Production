using System;
using System.Security.Cryptography;
using System.Text;

namespace GrcMvc.Helpers;

/// <summary>
/// Secure token generation and hashing utilities for email verification.
/// Follows enterprise security best practices:
/// - Raw tokens are never stored in the database
/// - Only SHA256 hashes are persisted
/// - URL-safe Base64 encoding for tokens
/// </summary>
public static class TokenHelper
{
    /// <summary>
    /// Generate a cryptographically secure random token.
    /// Uses URL-safe Base64 encoding (+ → -, / → _, no padding).
    /// </summary>
    /// <param name="length">Number of random bytes (default 64 = 86 chars after encoding)</param>
    /// <returns>URL-safe Base64 encoded token</returns>
    public static string GenerateSecureToken(int length = 64)
    {
        var bytes = RandomNumberGenerator.GetBytes(length);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }

    /// <summary>
    /// Hash a token using SHA256.
    /// The hash is what gets stored in the database, not the raw token.
    /// </summary>
    /// <param name="token">The raw token to hash</param>
    /// <returns>Lowercase hex string of SHA256 hash</returns>
    public static string HashToken(string token)
    {
        if (string.IsNullOrEmpty(token))
            throw new ArgumentNullException(nameof(token));

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    /// <summary>
    /// Verify that a token matches its stored hash.
    /// </summary>
    /// <param name="token">The raw token from the URL</param>
    /// <param name="storedHash">The hash stored in the database</param>
    /// <returns>True if the token matches the hash</returns>
    public static bool VerifyToken(string token, string? storedHash)
    {
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(storedHash))
            return false;

        var computedHash = HashToken(token);
        return string.Equals(computedHash, storedHash, StringComparison.OrdinalIgnoreCase);
    }
}
