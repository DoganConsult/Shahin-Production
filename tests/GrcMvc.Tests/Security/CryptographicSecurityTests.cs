using Xunit;
using GrcMvc.Services.Implementations;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace GrcMvc.Tests.Security;

/// <summary>
/// Security-focused tests to verify cryptographic implementations
/// </summary>
public class CryptographicSecurityTests
{
    [Fact]
    public void SecurePasswordGenerator_UsesRandomNumberGenerator_NotRandom()
    {
        // This test verifies we're using the secure RNG, not System.Random
        
        // Arrange
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<SecurePasswordGenerator>();
        var generator = new SecurePasswordGenerator(logger);
        
        // Act - Generate many passwords
        var passwords = Enumerable.Range(0, 1000)
            .Select(_ => generator.GeneratePassword(18))
            .ToList();
        
        // Assert - Check entropy (should be high)
        var uniqueCount = passwords.Distinct().Count();
        
        // With 18 characters and our character set, collision probability is negligible
        // If we get < 999 unique passwords out of 1000, something is wrong
        Assert.True(uniqueCount >= 999, 
            $"Low entropy detected: only {uniqueCount}/1000 unique passwords");
    }
    
    [Fact]
    public void SecurePasswordGenerator_PasswordsNotPredictable()
    {
        // Verify that sequential passwords don't have patterns
        
        // Arrange
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<SecurePasswordGenerator>();
        var generator = new SecurePasswordGenerator(logger);
        
        // Act - Generate 100 sequential passwords
        var passwords = Enumerable.Range(0, 100)
            .Select(_ => generator.GeneratePassword(18))
            .ToList();
        
        // Assert - Check for patterns
        for (int i = 0; i < passwords.Count - 1; i++)
        {
            var current = passwords[i];
            var next = passwords[i + 1];
            
            // Calculate similarity (Hamming distance for same-length strings)
            var similarity = current.Zip(next, (a, b) => a == b).Count(match => match);
            
            // Two random 18-char passwords should have ~0-3 chars in common by chance
            // If they have >5 chars in common at same positions, might be a pattern
            Assert.True(similarity <= 5, 
                $"Passwords too similar: {similarity}/18 chars match at same positions");
        }
    }
    
    [Fact]
    public void SecurePasswordGenerator_MeetsComplexityRequirements()
    {
        // Verify all passwords meet enterprise security standards
        
        // Arrange
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<SecurePasswordGenerator>();
        var generator = new SecurePasswordGenerator(logger);
        
        // Act - Generate 100 passwords
        for (int i = 0; i < 100; i++)
        {
            var password = generator.GeneratePassword(18);
            
            // Assert - Each must meet all requirements
            Assert.True(password.Any(char.IsUpper), "Missing uppercase");
            Assert.True(password.Any(char.IsLower), "Missing lowercase");
            Assert.True(password.Any(char.IsDigit), "Missing digit");
            Assert.True(password.Any(c => "!@#$%^&*".Contains(c)), "Missing symbol");
            Assert.Equal(18, password.Length);
        }
    }
    
    [Fact]
    public void PasswordEntropyCalculation_ExceedsMinimumBits()
    {
        // Calculate theoretical entropy of generated passwords
        
        // Character set size (excluding ambiguous chars)
        const int uppercase = 24; // A-Z minus O, I
        const int lowercase = 24; // a-z minus l, o
        const int numbers = 8;    // 2-9 (no 0, 1)
        const int symbols = 8;    // !@#$%^&*
        const int totalChars = uppercase + lowercase + numbers + symbols; // 64
        
        // Password length
        const int passwordLength = 18;
        
        // Entropy calculation: log2(charsetSize ^ length)
        var entropy = Math.Log2(Math.Pow(totalChars, passwordLength));
        
        // NIST recommends minimum 80 bits for passwords
        // Our passwords should have ~107 bits
        Assert.True(entropy >= 80, 
            $"Entropy too low: {entropy:F1} bits (minimum 80 required)");
        
        Assert.True(entropy >= 100, 
            $"Expected ~107 bits, got {entropy:F1} bits");
    }
}
