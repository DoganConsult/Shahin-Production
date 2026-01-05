using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace GrcMvc.Configuration
{
    /// <summary>
    /// Strongly-typed application configuration settings
    /// </summary>
    public sealed class ApplicationSettings
    {
        public const string SectionName = "ApplicationSettings";

        [Required]
        public string ApplicationName { get; init; } = "GRC Management System";

        [Required]
        [RegularExpression(@"^\d+\.\d+\.\d+$", ErrorMessage = "Version must be in format X.Y.Z")]
        public string Version { get; init; } = "1.0.0";

        [Required]
        [EmailAddress]
        public string SupportEmail { get; init; } = string.Empty;

        public bool EnableAuditLog { get; init; } = true;

        [Range(1024, 104857600, ErrorMessage = "File size must be between 1KB and 100MB")]
        public long MaxFileUploadSize { get; init; } = 10485760; // 10MB default

        [Required]
        public string AllowedFileExtensions { get; init; } = ".pdf,.doc,.docx,.xls,.xlsx,.png,.jpg,.jpeg";

        /// <summary>
        /// Gets the allowed file extensions as an array
        /// </summary>
        public string[] GetAllowedExtensions()
        {
            return AllowedFileExtensions
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(ext => ext.Trim().ToLowerInvariant())
                .ToArray();
        }

        /// <summary>
        /// Checks if a file extension is allowed
        /// </summary>
        public bool IsExtensionAllowed(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
                return false;

            var ext = extension.ToLowerInvariant();
            if (!ext.StartsWith("."))
                ext = "." + ext;

            return GetAllowedExtensions().Contains(ext);
        }

        /// <summary>
        /// Gets the maximum file upload size in MB
        /// </summary>
        public double GetMaxFileSizeInMB()
        {
            return MaxFileUploadSize / (1024.0 * 1024.0);
        }
    }
}