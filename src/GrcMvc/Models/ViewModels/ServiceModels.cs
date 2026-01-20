namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Model for Environment Variable Item
    /// </summary>
    public class EnvironmentVariableItem
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsSecret { get; set; }
        public bool IsRequired { get; set; }
    }
}
