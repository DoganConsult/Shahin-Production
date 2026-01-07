using Xunit;
using Microsoft.Extensions.Logging;
using GrcMvc.Services.Implementations;
using System.Linq;

namespace GrcMvc.Tests.Services;

/// <summary>
/// Tests for SecurePasswordGenerator to verify cryptographic safety
/// </summary>
public class SecurePasswordGeneratorTests
{
    private readonly ILogger<SecurePasswordGenerator> _logger;
    private readonly SecurePasswordGenerator _generator;
    
    public SecurePasswordGeneratorTests()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<SecurePasswordGenerator>();
        _generator = new SecurePasswordGenerator(_logger);
    }
    
    [Fact]
    public void GeneratePassword_DefaultLength_Returns18Characters()
    {
        // Act
        var password = _generator.GeneratePassword();
        
        // Assert
        Assert.Equal(18, password.Length);
    }
    
    [Fact]
    public void GeneratePassword_CustomLength_ReturnsCorrectLength()
    {
        // Arrange
        var lengths = new[] { 8, 12, 16, 20, 32 };
        
        foreach (var length in lengths)
        {
            // Act
            var password = _generator.GeneratePassword(length);
            
            // Assert
            Assert.Equal(length, password.Length);
        }
    }
    
    [Fact]
    public void GeneratePassword_ContainsRequiredCharacterTypes()
    {
        // Act
        var password = _generator.GeneratePassword(18);
        
        // Assert
        Assert.Contains(password, char.IsUpper);
        Assert.Contains(password, char.IsLower);
        Assert.Contains(password, char.IsDigit);
        Assert.Contains(password, c => "!@#$%^&*".Contains(c));
    }
    
    [Fact]
    public void GeneratePassword_MultipleCallsProduceDifferentResults()
    {
        // Act - Generate 100 passwords
        var passwords = Enumerable.Range(0, 100)
            .Select(_ => _generator.GeneratePassword(18))
            .ToList();
        
        // Assert - All should be unique (high probability with 18 chars)
        var uniqueCount = passwords.Distinct().Count();
        Assert.Equal(100, uniqueCount);
    }
    
    [Fact]
    public void GeneratePassword_NoAmbiguousCharacters()
    {
        // Arrange
        var ambiguousChars = new[] { '0', 'O', 'l', '1', 'I' };
        
        // Act - Generate 50 passwords to test
        for (int i = 0; i < 50; i++)
        {
            var password = _generator.GeneratePassword(18);
            
            // Assert - No ambiguous characters present
            foreach (var ambiguousChar in ambiguousChars)
            {
                Assert.DoesNotContain(ambiguousChar, password);
            }
        }
    }
    
    [Fact]
    public void GeneratePassword_MinimumLength_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _generator.GeneratePassword(7));
    }
    
    [Theory]
    [InlineData(true, true, true, true)]
    [InlineData(true, true, true, false)]
    [InlineData(true, true, false, true)]
    [InlineData(false, true, true, true)]
    public void GeneratePasswordWithRequirements_RespectsCharacterTypeRequirements(
        bool upper, bool lower, bool numbers, bool symbols)
    {
        // Act
        var password = _generator.GeneratePasswordWithRequirements(
            length: 18,
            includeUppercase: upper,
            includeLowercase: lower,
            includeNumbers: numbers,
            includeSymbols: symbols);
        
        // Assert
        if (upper)
            Assert.Contains(password, char.IsUpper);
        
        if (lower)
            Assert.Contains(password, char.IsLower);
        
        if (numbers)
            Assert.Contains(password, char.IsDigit);
        
        if (symbols)
            Assert.Contains(password, c => "!@#$%^&*".Contains(c));
    }
    
    [Fact]
    public void GeneratePassword_IsUniformlyDistributed()
    {
        // Act - Generate 10,000 passwords and count character frequencies
        var allChars = string.Concat(
            Enumerable.Range(0, 10000)
                .Select(_ => _generator.GeneratePassword(18))
        );
        
        var charCounts = allChars
            .GroupBy(c => c)
            .ToDictionary(g => g.Key, g => g.Count());
        
        // Assert - No single character appears more than 2x expected frequency
        var avgFrequency = allChars.Length / charCounts.Count;
        var maxExpectedFrequency = avgFrequency * 2;
        
        foreach (var count in charCounts.Values)
        {
            Assert.True(count < maxExpectedFrequency, 
                $"Character frequency {count} exceeds expected maximum {maxExpectedFrequency}");
        }
    }
}
