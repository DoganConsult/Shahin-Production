using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Service for secure file upload handling
    /// </summary>
    public interface IFileUploadService
    {
        /// <summary>
        /// Validates a file for upload
        /// </summary>
        Task<FileValidationResult> ValidateFileAsync(IFormFile file);

        /// <summary>
        /// Uploads a file after validation
        /// </summary>
        Task<FileUploadResult> UploadFileAsync(IFormFile file, string category);

        /// <summary>
        /// Deletes an uploaded file
        /// </summary>
        Task<bool> DeleteFileAsync(string filePath);

        /// <summary>
        /// Gets file content type by extension
        /// </summary>
        string GetContentType(string extension);
    }

    public class FileValidationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
        public string? FileName { get; set; }
        public long FileSize { get; set; }
        public string? Extension { get; set; }
        public string? ContentType { get; set; }
        public bool IsMalicious { get; set; }

        public static FileValidationResult Success(string fileName, long fileSize, string extension, string contentType)
        {
            return new FileValidationResult
            {
                IsValid = true,
                FileName = fileName,
                FileSize = fileSize,
                Extension = extension,
                ContentType = contentType,
                IsMalicious = false
            };
        }

        public static FileValidationResult Failure(string errorMessage, bool isMalicious = false)
        {
            return new FileValidationResult
            {
                IsValid = false,
                ErrorMessage = errorMessage,
                IsMalicious = isMalicious
            };
        }
    }

    public class FileUploadResult
    {
        public bool Success { get; set; }
        public string? FilePath { get; set; }
        public string? FileName { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime? UploadedAt { get; set; }
        public string? FileHash { get; set; }
    }
}