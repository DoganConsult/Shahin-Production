using GrcMvc.Configuration;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Secure file upload service with validation and malware detection
    /// </summary>
    public class FileUploadService : IFileUploadService
    {
        private readonly ApplicationSettings _appSettings;
        private readonly ILogger<FileUploadService> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly string _uploadPath;

        // File signature (magic bytes) for common file types
        private static readonly Dictionary<string, byte[][]> FileSignatures = new()
        {
            { ".pdf", new[] { new byte[] { 0x25, 0x50, 0x44, 0x46 } } }, // %PDF
            { ".jpg", new[] { new byte[] { 0xFF, 0xD8, 0xFF } } },
            { ".jpeg", new[] { new byte[] { 0xFF, 0xD8, 0xFF } } },
            { ".png", new[] { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
            { ".doc", new[] { new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 } } },
            { ".docx", new[] { new byte[] { 0x50, 0x4B, 0x03, 0x04 }, new byte[] { 0x50, 0x4B, 0x05, 0x06 } } },
            { ".xls", new[] { new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 } } },
            { ".xlsx", new[] { new byte[] { 0x50, 0x4B, 0x03, 0x04 }, new byte[] { 0x50, 0x4B, 0x05, 0x06 } } }
        };

        public FileUploadService(
            IOptions<ApplicationSettings> appSettings,
            ILogger<FileUploadService> logger,
            IWebHostEnvironment environment)
        {
            _appSettings = appSettings.Value;
            _logger = logger;
            _environment = environment;

            // Store uploads outside wwwroot for security
            _uploadPath = Path.Combine(environment.ContentRootPath, "Uploads");
            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        public async Task<FileValidationResult> ValidateFileAsync(IFormFile file)
        {
            try
            {
                // Check if file exists
                if (file == null || file.Length == 0)
                {
                    return FileValidationResult.Failure("No file provided");
                }

                // Check file size
                if (file.Length > _appSettings.MaxFileUploadSize)
                {
                    var maxSizeMB = _appSettings.GetMaxFileSizeInMB();
                    return FileValidationResult.Failure($"File size exceeds maximum allowed size of {maxSizeMB:F2} MB");
                }

                // Get and validate extension
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!_appSettings.IsExtensionAllowed(extension))
                {
                    return FileValidationResult.Failure(
                        $"File type '{extension}' is not allowed. Allowed types: {_appSettings.AllowedFileExtensions}");
                }

                // Validate file signature (magic bytes)
                if (!await ValidateFileSignatureAsync(file, extension))
                {
                    _logger.LogWarning("File signature validation failed for file: {FileName}", file.FileName);
                    return FileValidationResult.Failure(
                        "File content does not match its extension. Possible security threat.",
                        isMalicious: true);
                }

                // Check for potentially dangerous content
                if (await ContainsDangerousContentAsync(file))
                {
                    _logger.LogWarning("Dangerous content detected in file: {FileName}", file.FileName);
                    return FileValidationResult.Failure(
                        "File contains potentially dangerous content",
                        isMalicious: true);
                }

                // Generate safe filename
                var safeFileName = GenerateSafeFileName(file.FileName);

                return FileValidationResult.Success(
                    safeFileName,
                    file.Length,
                    extension,
                    file.ContentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating file upload");
                return FileValidationResult.Failure("An error occurred during file validation");
            }
        }

        public async Task<FileUploadResult> UploadFileAsync(IFormFile file, string category)
        {
            try
            {
                // Validate first
                var validationResult = await ValidateFileAsync(file);
                if (!validationResult.IsValid)
                {
                    return new FileUploadResult
                    {
                        Success = false,
                        ErrorMessage = validationResult.ErrorMessage
                    };
                }

                // Create category folder
                var categoryPath = Path.Combine(_uploadPath, SanitizePath(category));
                if (!Directory.Exists(categoryPath))
                {
                    Directory.CreateDirectory(categoryPath);
                }

                // Generate unique filename
                var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
                var extension = Path.GetExtension(file.FileName);
                var fileName = $"{timestamp}_{uniqueId}{extension}";
                var filePath = Path.Combine(categoryPath, fileName);

                // Calculate file hash for integrity
                string fileHash;
                using (var stream = file.OpenReadStream())
                {
                    fileHash = await CalculateFileHashAsync(stream);
                    stream.Position = 0; // Reset stream position

                    // Save file
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await stream.CopyToAsync(fileStream);
                    }
                }

                // Log the upload if audit is enabled
                if (_appSettings.EnableAuditLog)
                {
                    _logger.LogInformation(
                        "File uploaded: {FileName}, Category: {Category}, Size: {Size}, Hash: {Hash}",
                        fileName, category, file.Length, fileHash);
                }

                return new FileUploadResult
                {
                    Success = true,
                    FilePath = Path.Combine(category, fileName),
                    FileName = fileName,
                    UploadedAt = DateTime.UtcNow,
                    FileHash = fileHash
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                return new FileUploadResult
                {
                    Success = false,
                    ErrorMessage = "An error occurred during file upload"
                };
            }
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_uploadPath, SanitizePath(filePath));

                if (!File.Exists(fullPath))
                {
                    return false;
                }

                await Task.Run(() => File.Delete(fullPath));

                if (_appSettings.EnableAuditLog)
                {
                    _logger.LogInformation("File deleted: {FilePath}", filePath);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
                return false;
            }
        }

        public string GetContentType(string extension)
        {
            var contentTypes = new Dictionary<string, string>
            {
                { ".pdf", "application/pdf" },
                { ".doc", "application/msword" },
                { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
                { ".xls", "application/vnd.ms-excel" },
                { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
                { ".png", "image/png" },
                { ".jpg", "image/jpeg" },
                { ".jpeg", "image/jpeg" }
            };

            return contentTypes.GetValueOrDefault(extension.ToLowerInvariant(), "application/octet-stream");
        }

        private async Task<bool> ValidateFileSignatureAsync(IFormFile file, string extension)
        {
            if (!FileSignatures.ContainsKey(extension))
            {
                // If we don't have signature for this type, allow it (could be text file)
                return true;
            }

            var signatures = FileSignatures[extension];
            var headerBytes = new byte[8];

            using (var stream = file.OpenReadStream())
            {
                await stream.ReadAsync(headerBytes, 0, headerBytes.Length);
                stream.Position = 0; // Reset stream
            }

            return signatures.Any(signature =>
                headerBytes.Take(signature.Length).SequenceEqual(signature));
        }

        private async Task<bool> ContainsDangerousContentAsync(IFormFile file)
        {
            // Check for common script patterns in the first 1KB
            var buffer = new byte[1024];
            using (var stream = file.OpenReadStream())
            {
                var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                stream.Position = 0; // Reset stream

                if (bytesRead > 0)
                {
                    var content = Encoding.UTF8.GetString(buffer, 0, bytesRead).ToLowerInvariant();

                    // Check for dangerous patterns
                    var dangerousPatterns = new[]
                    {
                        "<script", "javascript:", "onerror=", "onclick=",
                        "eval(", "document.write", "window.location",
                        ".exe", ".dll", ".bat", ".cmd", ".ps1", ".vbs"
                    };

                    return dangerousPatterns.Any(pattern => content.Contains(pattern));
                }
            }

            return false;
        }

        private async Task<string> CalculateFileHashAsync(Stream stream)
        {
            using (var sha256 = SHA256.Create())
            {
                var hash = await Task.Run(() => sha256.ComputeHash(stream));
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        private string GenerateSafeFileName(string fileName)
        {
            // Remove path information and invalid characters
            fileName = Path.GetFileName(fileName);
            var invalidChars = Path.GetInvalidFileNameChars();
            var safeFileName = string.Join("_", fileName.Split(invalidChars));

            // Limit length
            if (safeFileName.Length > 100)
            {
                var extension = Path.GetExtension(safeFileName);
                var nameWithoutExt = Path.GetFileNameWithoutExtension(safeFileName);
                safeFileName = nameWithoutExt.Substring(0, 100 - extension.Length) + extension;
            }

            return safeFileName;
        }

        private string SanitizePath(string path)
        {
            // Prevent directory traversal attacks
            return path.Replace("..", "").Replace("~", "").Replace("/", Path.DirectorySeparatorChar.ToString());
        }
    }
}