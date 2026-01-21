using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Local file system storage implementation
    /// </summary>
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly ILogger<LocalFileStorageService> _logger;
        private readonly string _basePath;
        private readonly string _reportsPath;

        public LocalFileStorageService(
            IWebHostEnvironment environment,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            ILogger<LocalFileStorageService> logger)
        {
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _logger = logger;

            // Setup base paths
            _basePath = Path.Combine(_environment.WebRootPath, "storage");
            _reportsPath = Path.Combine(_basePath, "reports");

            // Ensure directories exist
            EnsureDirectoriesExist();
        }

        /// <summary>
        /// Save a file to local storage
        /// </summary>
        public async Task<string> SaveFileAsync(byte[] content, string fileName, string contentType)
        {
            try
            {
                // Generate unique file name to avoid conflicts
                var fileExtension = Path.GetExtension(fileName);
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";

                // Organize files by year/month
                var datePath = DateTime.UtcNow.ToString("yyyy/MM");
                var directoryPath = Path.Combine(_reportsPath, datePath);
                Directory.CreateDirectory(directoryPath);

                var filePath = Path.Combine(directoryPath, uniqueFileName);

                // Save file
                await File.WriteAllBytesAsync(filePath, content);

                // Calculate relative path for storage
                var relativePath = Path.GetRelativePath(_basePath, filePath)
                    .Replace(Path.DirectorySeparatorChar, '/');

                _logger.LogInformation($"File saved successfully: {relativePath}");

                return relativePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving file: {fileName}");
                throw new InvalidOperationException($"Failed to save file: {fileName}", ex);
            }
        }

        /// <summary>
        /// Retrieve a file from local storage
        /// </summary>
        public async Task<byte[]> GetFileAsync(string filePath)
        {
            try
            {
                var fullPath = GetFullPath(filePath);

                if (!File.Exists(fullPath))
                {
                    throw new FileNotFoundException($"File not found: {filePath}");
                }

                return await File.ReadAllBytesAsync(fullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving file: {filePath}");
                throw;
            }
        }

        /// <summary>
        /// Delete a file from local storage
        /// </summary>
        public async Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                var fullPath = GetFullPath(filePath);

                if (!File.Exists(fullPath))
                {
                    _logger.LogWarning($"File not found for deletion: {filePath}");
                    return false;
                }

                await Task.Run(() => File.Delete(fullPath));

                _logger.LogInformation($"File deleted successfully: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting file: {filePath}");
                return false;
            }
        }

        /// <summary>
        /// Get the full URL for accessing a file
        /// </summary>
        public Task<string> GetFileUrlAsync(string filePath)
        {
            try
            {
                var request = _httpContextAccessor.HttpContext?.Request;
                if (request == null)
                {
                    // Fallback to configuration
                    var baseUrl = _configuration["App:BaseUrl"] ?? "http://localhost:5000";
                    return Task.FromResult($"{baseUrl}/storage/{filePath}");
                }

                var scheme = request.Scheme;
                var host = request.Host.Value;
                var url = $"{scheme}://{host}/storage/{filePath}";

                return Task.FromResult(url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating file URL: {filePath}");
                throw;
            }
        }

        /// <summary>
        /// Check if a file exists
        /// </summary>
        public Task<bool> FileExistsAsync(string filePath)
        {
            try
            {
                var fullPath = GetFullPath(filePath);
                return Task.FromResult(File.Exists(fullPath));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking file existence: {filePath}");
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// Get file metadata
        /// </summary>
        public async Task<FileMetadata> GetFileMetadataAsync(string filePath)
        {
            try
            {
                var fullPath = GetFullPath(filePath);

                if (!File.Exists(fullPath))
                {
                    throw new FileNotFoundException($"File not found: {filePath}");
                }

                var fileInfo = new FileInfo(fullPath);
                var contentType = GetContentType(fileInfo.Extension);

                // Calculate file hash
                var fileHash = await CalculateFileHashAsync(fullPath);

                return new FileMetadata
                {
                    FileName = fileInfo.Name,
                    FilePath = filePath,
                    FileSize = fileInfo.Length,
                    ContentType = contentType,
                    CreatedDate = fileInfo.CreationTimeUtc,
                    ModifiedDate = fileInfo.LastWriteTimeUtc,
                    FileHash = fileHash
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting file metadata: {filePath}");
                throw;
            }
        }

        /// <summary>
        /// Get the full file path from relative path
        /// </summary>
        private string GetFullPath(string relativePath)
        {
            // Ensure the path is relative and safe
            if (Path.IsPathRooted(relativePath))
            {
                throw new ArgumentException("Path must be relative", nameof(relativePath));
            }

            // Remove any potential directory traversal attempts
            relativePath = relativePath.Replace("..", "").Replace("~", "");

            return Path.Combine(_basePath, relativePath);
        }

        /// <summary>
        /// Ensure required directories exist
        /// </summary>
        private void EnsureDirectoriesExist()
        {
            try
            {
                Directory.CreateDirectory(_basePath);
                Directory.CreateDirectory(_reportsPath);

                // Create year/month subdirectories for current month
                var currentMonthPath = Path.Combine(_reportsPath, DateTime.UtcNow.ToString("yyyy/MM"));
                Directory.CreateDirectory(currentMonthPath);

                _logger.LogInformation($"Storage directories initialized at: {_basePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating storage directories");
                throw;
            }
        }

        /// <summary>
        /// Get content type from file extension
        /// </summary>
        private string GetContentType(string extension)
        {
            return extension.ToLower() switch
            {
                ".pdf" => "application/pdf",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".xls" => "application/vnd.ms-excel",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".doc" => "application/msword",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".txt" => "text/plain",
                ".csv" => "text/csv",
                ".json" => "application/json",
                ".xml" => "application/xml",
                _ => "application/octet-stream"
            };
        }

        /// <summary>
        /// Calculate SHA256 hash of a file
        /// </summary>
        private async Task<string> CalculateFileHashAsync(string filePath)
        {
            try
            {
                using var sha256 = SHA256.Create();
                using var stream = File.OpenRead(filePath);
                var hash = await Task.Run(() => sha256.ComputeHash(stream));
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error calculating file hash");
                return string.Empty;
            }
        }
    }
}