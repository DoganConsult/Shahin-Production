using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using GrcMvc.Services.Interfaces;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// AWS S3 Storage Service Implementation
/// Handles file storage in AWS S3
/// </summary>
public class S3StorageService : IFileStorageService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<S3StorageService> _logger;
    private readonly IAmazonS3? _s3Client;
    private readonly string? _bucketName;
    private readonly bool _isEnabled;

    public S3StorageService(
        IConfiguration configuration,
        ILogger<S3StorageService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        var accessKeyId = _configuration["Storage:AWS:AccessKeyId"] 
            ?? Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
        var secretAccessKey = _configuration["Storage:AWS:SecretAccessKey"] 
            ?? Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
        var region = _configuration["Storage:AWS:Region"] 
            ?? Environment.GetEnvironmentVariable("AWS_REGION") 
            ?? "us-east-1";
        _bucketName = _configuration["Storage:AWS:BucketName"] ?? "grc-files";

        if (!string.IsNullOrEmpty(accessKeyId) && !string.IsNullOrEmpty(secretAccessKey))
        {
            try
            {
                var s3Config = new AmazonS3Config
                {
                    RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region)
                };

                _s3Client = new AmazonS3Client(accessKeyId, secretAccessKey, s3Config);
                _isEnabled = true;
                _logger.LogInformation("AWS S3 Storage initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize AWS S3 Storage");
                _isEnabled = false;
            }
        }
        else
        {
            _isEnabled = false;
            _logger.LogWarning("AWS S3 credentials not configured");
        }
    }

    public bool IsEnabled => _isEnabled;

    public async Task<string> SaveFileAsync(byte[] content, string fileName, string contentType)
    {
        if (!_isEnabled || _s3Client == null || string.IsNullOrEmpty(_bucketName))
        {
            throw new InvalidOperationException("AWS S3 Storage is not enabled or configured");
        }

        try
        {
            // Organize files by year/month
            var datePath = DateTime.UtcNow.ToString("yyyy/MM");
            var key = $"{datePath}/{fileName}";

            using var stream = new MemoryStream(content);
            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
                InputStream = stream,
                ContentType = contentType,
                ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
            };

            await _s3Client.PutObjectAsync(request);

            _logger.LogInformation("File uploaded to AWS S3: {Key}", key);
            return key;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to AWS S3: {FileName}", fileName);
            throw;
        }
    }

    public async Task<byte[]> GetFileAsync(string filePath)
    {
        if (!_isEnabled || _s3Client == null || string.IsNullOrEmpty(_bucketName))
        {
            throw new InvalidOperationException("AWS S3 Storage is not enabled or configured");
        }

        try
        {
            var request = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = filePath
            };

            var response = await _s3Client.GetObjectAsync(request);
            using var memoryStream = new MemoryStream();
            await response.ResponseStream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new FileNotFoundException($"File not found in AWS S3: {filePath}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file from AWS S3: {FilePath}", filePath);
            throw;
        }
    }

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        if (!_isEnabled || _s3Client == null || string.IsNullOrEmpty(_bucketName))
        {
            return false;
        }

        try
        {
            var request = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = filePath
            };

            await _s3Client.DeleteObjectAsync(request);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file from AWS S3: {FilePath}", filePath);
            return false;
        }
    }

    public async Task<bool> FileExistsAsync(string filePath)
    {
        if (!_isEnabled || _s3Client == null || string.IsNullOrEmpty(_bucketName))
        {
            return false;
        }

        try
        {
            var request = new GetObjectMetadataRequest
            {
                BucketName = _bucketName,
                Key = filePath
            };

            await _s3Client.GetObjectMetadataAsync(request);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking file existence in AWS S3: {FilePath}", filePath);
            return false;
        }
    }

    public async Task<string> GetFileUrlAsync(string filePath)
    {
        if (!_isEnabled || _s3Client == null || string.IsNullOrEmpty(_bucketName))
        {
            throw new InvalidOperationException("AWS S3 Storage is not enabled or configured");
        }

        try
        {
            return $"https://{_bucketName}.s3.{_s3Client.Config.RegionEndpoint.SystemName}.amazonaws.com/{filePath}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating file URL from AWS S3: {FilePath}", filePath);
            throw;
        }
    }

    public async Task<FileMetadata> GetFileMetadataAsync(string filePath)
    {
        if (!_isEnabled || _s3Client == null || string.IsNullOrEmpty(_bucketName))
        {
            throw new InvalidOperationException("AWS S3 Storage is not enabled or configured");
        }

        try
        {
            var request = new GetObjectMetadataRequest
            {
                BucketName = _bucketName,
                Key = filePath
            };

            var response = await _s3Client.GetObjectMetadataAsync(request);

            return new FileMetadata
            {
                FileName = Path.GetFileName(filePath),
                FilePath = filePath,
                FileSize = response.ContentLength,
                ContentType = response.Headers.ContentType,
                CreatedDate = response.LastModified,
                ModifiedDate = response.LastModified,
                FileHash = response.ETag?.Trim('"') ?? string.Empty
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file metadata from AWS S3: {FilePath}", filePath);
            throw;
        }
    }
}
