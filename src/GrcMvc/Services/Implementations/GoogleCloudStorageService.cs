using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using GrcMvc.Services.Interfaces;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Google Cloud Storage Service Implementation
/// Handles file storage in Google Cloud Storage
/// </summary>
public class GoogleCloudStorageService : IFileStorageService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GoogleCloudStorageService> _logger;
    private readonly StorageClient? _storageClient;
    private readonly string? _bucketName;
    private readonly bool _isEnabled;

    public GoogleCloudStorageService(
        IConfiguration configuration,
        ILogger<GoogleCloudStorageService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        var credentialsPath = _configuration["Storage:GoogleCloud:CredentialsPath"] 
            ?? Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
        _bucketName = _configuration["Storage:GoogleCloud:BucketName"] ?? "grc-files";

        if (!string.IsNullOrEmpty(credentialsPath) && File.Exists(credentialsPath))
        {
            try
            {
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);
                _storageClient = StorageClient.Create();
                _isEnabled = true;
                _logger.LogInformation("Google Cloud Storage initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Google Cloud Storage");
                _isEnabled = false;
            }
        }
        else
        {
            _isEnabled = false;
            _logger.LogWarning("Google Cloud Storage credentials not configured");
        }
    }

    public bool IsEnabled => _isEnabled;

    public async Task<string> SaveFileAsync(byte[] content, string fileName, string contentType)
    {
        if (!_isEnabled || _storageClient == null || string.IsNullOrEmpty(_bucketName))
        {
            throw new InvalidOperationException("Google Cloud Storage is not enabled or configured");
        }

        try
        {
            // Organize files by year/month
            var datePath = DateTime.UtcNow.ToString("yyyy/MM");
            var objectName = $"{datePath}/{fileName}";

            var obj = new Google.Cloud.Storage.V1.Object
            {
                Bucket = _bucketName,
                Name = objectName,
                ContentType = contentType
            };

            using var stream = new MemoryStream(content);
            await _storageClient.UploadObjectAsync(obj, stream);

            _logger.LogInformation("File uploaded to Google Cloud Storage: {ObjectName}", objectName);
            return objectName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to Google Cloud Storage: {FileName}", fileName);
            throw;
        }
    }

    public async Task<byte[]> GetFileAsync(string filePath)
    {
        if (!_isEnabled || _storageClient == null || string.IsNullOrEmpty(_bucketName))
        {
            throw new InvalidOperationException("Google Cloud Storage is not enabled or configured");
        }

        try
        {
            var stream = new MemoryStream();
            await _storageClient.DownloadObjectAsync(_bucketName, filePath, stream);
            return stream.ToArray();
        }
        catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new FileNotFoundException($"File not found in Google Cloud Storage: {filePath}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file from Google Cloud Storage: {FilePath}", filePath);
            throw;
        }
    }

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        if (!_isEnabled || _storageClient == null || string.IsNullOrEmpty(_bucketName))
        {
            return false;
        }

        try
        {
            await _storageClient.DeleteObjectAsync(_bucketName, filePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file from Google Cloud Storage: {FilePath}", filePath);
            return false;
        }
    }

    public async Task<bool> FileExistsAsync(string filePath)
    {
        if (!_isEnabled || _storageClient == null || string.IsNullOrEmpty(_bucketName))
        {
            return false;
        }

        try
        {
            await _storageClient.GetObjectAsync(_bucketName, filePath);
            return true;
        }
        catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking file existence in Google Cloud Storage: {FilePath}", filePath);
            return false;
        }
    }

    public async Task<string> GetFileUrlAsync(string filePath)
    {
        if (!_isEnabled || _storageClient == null || string.IsNullOrEmpty(_bucketName))
        {
            throw new InvalidOperationException("Google Cloud Storage is not enabled or configured");
        }

        try
        {
            return $"https://storage.googleapis.com/{_bucketName}/{filePath}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating file URL from Google Cloud Storage: {FilePath}", filePath);
            throw;
        }
    }

    public async Task<FileMetadata> GetFileMetadataAsync(string filePath)
    {
        if (!_isEnabled || _storageClient == null || string.IsNullOrEmpty(_bucketName))
        {
            throw new InvalidOperationException("Google Cloud Storage is not enabled or configured");
        }

        try
        {
            var obj = await _storageClient.GetObjectAsync(_bucketName, filePath);

            return new FileMetadata
            {
                FileName = Path.GetFileName(filePath),
                FilePath = filePath,
                FileSize = obj.Size,
                ContentType = obj.ContentType,
                CreatedDate = obj.TimeCreated ?? DateTime.UtcNow,
                ModifiedDate = obj.Updated ?? DateTime.UtcNow,
                FileHash = obj.Md5Hash != null 
                    ? Convert.ToBase64String(obj.Md5Hash) 
                    : string.Empty
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file metadata from Google Cloud Storage: {FilePath}", filePath);
            throw;
        }
    }
}
