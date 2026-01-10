using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.DataProtection;
using System.Text.Json;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Credential Encryption Service - Securely encrypts/decrypts sensitive data
/// Uses ASP.NET Core Data Protection for encryption at rest
/// Follows ASP.NET Core security best practices
/// </summary>
public class CredentialEncryptionService : ICredentialEncryptionService
{
    private readonly IDataProtector _protector;
    private readonly ILogger<CredentialEncryptionService> _logger;
    private const string EncryptionPurpose = "GrcIntegration.Credentials.v1";
    private const string EncryptionPrefix = "ENC:";

    public CredentialEncryptionService(
        IDataProtectionProvider dataProtectionProvider,
        ILogger<CredentialEncryptionService> logger)
    {
        _protector = dataProtectionProvider.CreateProtector(EncryptionPurpose);
        _logger = logger;
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
        {
            return plainText;
        }

        try
        {
            var encrypted = _protector.Protect(plainText);
            return EncryptionPrefix + encrypted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to encrypt data");
            throw new InvalidOperationException("Encryption failed", ex);
        }
    }

    public string Decrypt(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText))
        {
            return encryptedText;
        }

        // If not encrypted, return as-is (backward compatibility)
        if (!encryptedText.StartsWith(EncryptionPrefix))
        {
            _logger.LogWarning("Attempting to decrypt unencrypted text (missing prefix)");
            return encryptedText;
        }

        try
        {
            var encrypted = encryptedText.Substring(EncryptionPrefix.Length);
            return _protector.Unprotect(encrypted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decrypt data");
            throw new InvalidOperationException("Decryption failed - data may be corrupted or key changed", ex);
        }
    }

    public string EncryptObject<T>(T obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        try
        {
            var json = JsonSerializer.Serialize(obj);
            return Encrypt(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to encrypt object of type {TypeName}", typeof(T).Name);
            throw new InvalidOperationException($"Object encryption failed for type {typeof(T).Name}", ex);
        }
    }

    public T DecryptObject<T>(string encryptedJson)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(encryptedJson);

        try
        {
            var json = Decrypt(encryptedJson);
            var obj = JsonSerializer.Deserialize<T>(json);

            if (obj == null)
            {
                throw new InvalidOperationException("Deserialization returned null");
            }

            return obj;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decrypt object of type {TypeName}", typeof(T).Name);
            throw new InvalidOperationException($"Object decryption failed for type {typeof(T).Name}", ex);
        }
    }

    public bool IsEncrypted(string text)
    {
        return !string.IsNullOrEmpty(text) && text.StartsWith(EncryptionPrefix);
    }
}
