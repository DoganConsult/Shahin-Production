using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using GrcMvc.Services.Interfaces;
using System.Text;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Azure Blob Storage Service Implementation
/// Handles file storage in Azure Blob Storage
/// </summary>
public class AzureBlobStorageService : IFileStorageService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AzureBlobStorageService> _logger;
    private readonly BlobServiceClient? _blobServiceClient;
    private readonly string? _containerName;
    private readonly bool _isEnabled;

    public AzureBlobStorageService(
        IConfiguration configuration,
        ILogger<AzureBlobStorageService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        var connectionString = _configuration["Storage:Azure:ConnectionString"] 
            ?? Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
        _containerName = _configuration["Storage:Azure:ContainerName"] ?? "grc-files";

        if (!string.IsNullOrEmpty(connectionString))
        {
            try
            {
                _blobServiceClient = new BlobServiceClient(connectionString);
                _isEnabled = true;
                _logger.LogInformation("Azure Blob Storage initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Azure Blob Storage");
                _isEnabled = false;
            }
        }
        else
        {
            _isEnabled = false;
            _logger.LogWarning("Azure Blob Storage connection string not configured");
        }
    }

    public bool IsEnabled => _isEnabled;

    public async Task<string> SaveFileAsync(byte[] content, string fileName, string contentType)
    {
        if (!_isEnabled || _blobServiceClient == null)
        {
            throw new InvalidOperationException("Azure Blob Storage is not enabled or configured");
        }

        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

            // Organize files by year/month
            var datePath = DateTime.UtcNow.ToString("yyyy/MM");
            var blobName = $"{datePath}/{fileName}";

            var blobClient = containerClient.GetBlobClient(blobName);

            var blobHttpHeaders = new BlobHttpHeaders
            {
                ContentType = contentType
            };

            using var stream = new MemoryStream(content);
            await blobClient.UploadAsync(stream, new BlobUploadOptions
            {
                HttpHeaders = blobHttpHeaders
            });

            _logger.LogInformation("File uploaded to Azure Blob Storage: {BlobName}", blobName);
            return $"{datePath}/{fileName}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to Azure Blob Storage: {FileName}", fileName);
            throw;
        }
    }

    public async Task<byte[]> GetFileAsync(string filePath)
    {
        if (!_isEnabled || _blobServiceClient == null)
        {
            throw new InvalidOperationException("Azure Blob Storage is not enabled or configured");
        }

        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(filePath);

            if (!await blobClient.ExistsAsync())
            {
                throw new FileNotFoundException($"File not found in Azure Blob Storage: {filePath}");
            }

            var response = await blobClient.DownloadContentAsync();
            return response.Value.Content.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file from Azure Blob Storage: {FilePath}", filePath);
            throw;
        }
    }

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        if (!_isEnabled || _blobServiceClient == null)
        {
            return false;
        }

        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(filePath);

            return await blobClient.DeleteIfExistsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file from Azure Blob Storage: {FilePath}", filePath);
            return false;
        }
    }

    public async Task<bool> FileExistsAsync(string filePath)
    {
        if (!_isEnabled || _blobServiceClient == null)
        {
            return false;
        }

        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(filePath);

            return await blobClient.ExistsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking file existence in Azure Blob Storage: {FilePath}", filePath);
            return false;
        }
    }

    public async Task<string> GetFileUrlAsync(string filePath)
    {
        if (!_isEnabled || _blobServiceClient == null)
        {
            throw new InvalidOperationException("Azure Blob Storage is not enabled or configured");
        }

        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(filePath);

            return blobClient.Uri.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating file URL from Azure Blob Storage: {FilePath}", filePath);
            throw;
        }
    }

    public async Task<FileMetadata> GetFileMetadataAsync(string filePath)
    {
        if (!_isEnabled || _blobServiceClient == null)
        {
            throw new InvalidOperationException("Azure Blob Storage is not enabled or configured");
        }

        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(filePath);

            var properties = await blobClient.GetPropertiesAsync();

            return new FileMetadata
            {
                FileName = Path.GetFileName(filePath),
                FilePath = filePath,
                FileSize = properties.Value.ContentLength,
                ContentType = properties.Value.ContentType,
                CreatedDate = properties.Value.CreatedOn.UtcDateTime,
                ModifiedDate = properties.Value.LastModified.UtcDateTime,
                FileHash = properties.Value.ContentHash != null 
                    ? Convert.ToBase64String(properties.Value.ContentHash) 
                    : string.Empty
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file metadata from Azure Blob Storage: {FilePath}", filePath);
            throw;
        }
    }
}
