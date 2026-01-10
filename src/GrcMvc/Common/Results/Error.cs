namespace GrcMvc.Common.Results;

/// <summary>
/// Represents a structured error with code, message, and optional metadata
/// </summary>
public class Error
{
    public string Code { get; }
    public string Message { get; }
    public string? Details { get; }
    public Dictionary<string, object>? Metadata { get; }

    public Error(string code, string message, string? details = null, Dictionary<string, object>? metadata = null)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Details = details;
        Metadata = metadata;
    }

    /// <summary>
    /// Creates an error with a single metadata entry
    /// </summary>
    public static Error WithMetadata(string code, string message, string key, object value)
    {
        return new Error(code, message, null, new Dictionary<string, object> { { key, value } });
    }

    public override string ToString()
    {
        return $"[{Code}] {Message}" + (Details != null ? $": {Details}" : string.Empty);
    }
}
