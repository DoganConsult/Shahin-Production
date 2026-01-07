using System.Security.Cryptography;
using System.Text;
using GrcMvc.Services.Interfaces;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Cryptographically secure password generator using RandomNumberGenerator
/// Replaces insecure Random() implementation
/// </summary>
public class SecurePasswordGenerator : ISecurePasswordGenerator
{
    private readonly ILogger<SecurePasswordGenerator> _logger;
    
    // Exclude ambiguous characters: 0, O, l, 1, I
    private const string UppercaseChars = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const string LowercaseChars = "abcdefghijkmnpqrstuvwxyz";
    private const string NumberChars = "23456789";
    private const string SymbolChars = "!@#$%^&*";
    
    public SecurePasswordGenerator(ILogger<SecurePasswordGenerator> logger)
    {
        _logger = logger;
    }
    
    public string GeneratePassword(int length = 18)
    {
        return GeneratePasswordWithRequirements(
            length: length,
            includeUppercase: true,
            includeLowercase: true,
            includeNumbers: true,
            includeSymbols: true);
    }
    
    public string GeneratePasswordWithRequirements(
        int length = 18,
        bool includeUppercase = true,
        bool includeLowercase = true,
        bool includeNumbers = true,
        bool includeSymbols = true)
    {
        if (length < 8)
        {
            throw new ArgumentException("Password length must be at least 8 characters", nameof(length));
        }
        
        // Build character set based on requirements
        var charSet = new StringBuilder();
        var requiredChars = new List<char>();
        
        if (includeUppercase)
        {
            charSet.Append(UppercaseChars);
            requiredChars.Add(GetRandomChar(UppercaseChars));
        }
        
        if (includeLowercase)
        {
            charSet.Append(LowercaseChars);
            requiredChars.Add(GetRandomChar(LowercaseChars));
        }
        
        if (includeNumbers)
        {
            charSet.Append(NumberChars);
            requiredChars.Add(GetRandomChar(NumberChars));
        }
        
        if (includeSymbols)
        {
            charSet.Append(SymbolChars);
            requiredChars.Add(GetRandomChar(SymbolChars));
        }
        
        if (charSet.Length == 0)
        {
            throw new ArgumentException("At least one character type must be enabled");
        }
        
        var validChars = charSet.ToString();
        var remainingLength = length - requiredChars.Count;
        
        // Generate remaining characters
        var passwordChars = new List<char>(requiredChars);
        for (int i = 0; i < remainingLength; i++)
        {
            passwordChars.Add(GetRandomChar(validChars));
        }
        
        // Shuffle using Fisher-Yates algorithm with crypto RNG
        Shuffle(passwordChars);
        
        var password = new string(passwordChars.ToArray());
        
        _logger.LogDebug("Generated secure password: length={Length}, hasUpper={Upper}, hasLower={Lower}, hasNumber={Number}, hasSymbol={Symbol}",
            password.Length,
            password.Any(char.IsUpper),
            password.Any(char.IsLower),
            password.Any(char.IsDigit),
            password.Any(c => SymbolChars.Contains(c)));
        
        return password;
    }
    
    private static char GetRandomChar(string chars)
    {
        var randomBytes = RandomNumberGenerator.GetBytes(4);
        var randomIndex = BitConverter.ToUInt32(randomBytes, 0) % (uint)chars.Length;
        return chars[(int)randomIndex];
    }
    
    private static void Shuffle<T>(IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            var randomBytes = RandomNumberGenerator.GetBytes(4);
            var k = (int)(BitConverter.ToUInt32(randomBytes, 0) % (uint)n);
            n--;
            (list[k], list[n]) = (list[n], list[k]);
        }
    }
}
