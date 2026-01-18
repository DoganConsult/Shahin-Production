namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Service for managing environment variables through UI
/// </summary>
public interface IEnvironmentVariableService
{
    /// <summary>
    /// Get all environment variables grouped by category
    /// </summary>
    Task<Dictionary<string, List<EnvironmentVariableItem>>> GetAllVariablesAsync();

    /// <summary>
    /// Get a specific environment variable value
    /// </summary>
    Task<string?> GetVariableAsync(string key);

    /// <summary>
    /// Update environment variables (writes to .env file and/or ABP Settings)
    /// </summary>
    Task UpdateVariablesAsync(Dictionary<string, string> variables, bool useAbpSettings = true);
    
    /// <summary>
    /// Migrate environment variables to ABP Settings
    /// </summary>
    Task MigrateToAbpSettingsAsync();
    
    /// <summary>
    /// Get ABP Setting name for an environment variable
    /// </summary>
    string? GetAbpSettingName(string envVarName);

    /// <summary>
    /// Get the path to the .env file being used
    /// </summary>
    string GetEnvFilePath();

    /// <summary>
    /// Check if .env file is writable
    /// </summary>
    bool IsWritable();
}

public class EnvironmentVariableItem
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool IsSecret { get; set; }
    public bool IsRequired { get; set; }
    public string Category { get; set; } = "General";
    public string Description { get; set; } = string.Empty;
    public string? CurrentValue { get; set; }
}
