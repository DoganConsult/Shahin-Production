using System.Threading.Tasks;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Service for managing file storage operations
    /// </summary>
    public interface IFileStorageService
    {
        /// <summary>
        /// Save a file to storage
        /// </summary>
        /// <param name="content">File content as byte array</param>
        /// <param name="fileName">Original file name</param>
        /// <param name="contentType">MIME content type</param>
        /// <returns>Relative path to the saved file</returns>
        Task<string> SaveFileAsync(byte[] content, string fileName, string contentType);

        /// <summary>
        /// Retrieve a file from storage
        /// </summary>
        /// <param name="filePath">Relative path to the file</param>
        /// <returns>File content as byte array</returns>
        Task<byte[]> GetFileAsync(string filePath);

        /// <summary>
        /// Delete a file from storage
        /// </summary>
        /// <param name="filePath">Relative path to the file</param>
        /// <returns>True if deleted successfully</returns>
        Task<bool> DeleteFileAsync(string filePath);

        /// <summary>
        /// Get the full URL for accessing a file
        /// </summary>
        /// <param name="filePath">Relative path to the file</param>
        /// <returns>Full URL to access the file</returns>
        Task<string> GetFileUrlAsync(string filePath);

        /// <summary>
        /// Check if a file exists
        /// </summary>
        /// <param name="filePath">Relative path to the file</param>
        /// <returns>True if file exists</returns>
        Task<bool> FileExistsAsync(string filePath);

        /// <summary>
        /// Get file metadata
        /// </summary>
        /// <param name="filePath">Relative path to the file</param>
        /// <returns>File metadata including size, creation date, etc.</returns>
        Task<FileMetadata> GetFileMetadataAsync(string filePath);
    }

    /// <summary>
    /// File metadata information
    /// </summary>
    public class FileMetadata
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string FileHash { get; set; } = string.Empty;
    }
}