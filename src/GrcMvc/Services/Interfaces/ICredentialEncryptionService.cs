namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Credential Encryption Service - Encrypts/decrypts sensitive integration credentials
/// Uses ASP.NET Core Data Protection for secure encryption at rest
/// </summary>
public interface ICredentialEncryptionService
{
    /// <summary>
    /// Encrypt credentials (API keys, passwords, tokens)
    /// </summary>
    string Encrypt(string plainText);

    /// <summary>
    /// Decrypt credentials
    /// </summary>
    string Decrypt(string encryptedText);

    /// <summary>
    /// Encrypt object as JSON
    /// </summary>
    string EncryptObject<T>(T obj);

    /// <summary>
    /// Decrypt JSON to object
    /// </summary>
    T DecryptObject<T>(string encryptedJson);

    /// <summary>
    /// Check if text is encrypted
    /// </summary>
    bool IsEncrypted(string text);
}
