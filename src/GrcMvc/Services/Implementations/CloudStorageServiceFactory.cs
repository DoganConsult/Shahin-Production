using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using GrcMvc.Services.Interfaces;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Factory for selecting the appropriate cloud storage service
/// Based on configuration, selects Azure Blob, AWS S3, Google Cloud, or Local storage
/// </summary>
public class CloudStorageServiceFactory
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<CloudStorageServiceFactory> _logger;
    private readonly AzureBlobStorageService? _azureStorage;
    private readonly S3StorageService? _s3Storage;
    private readonly GoogleCloudStorageService? _googleStorage;
    private readonly LocalFileStorageService _localStorage;

    public CloudStorageServiceFactory(
        IConfiguration configuration,
        ILogger<CloudStorageServiceFactory> logger,
        AzureBlobStorageService? azureStorage,
        S3StorageService? s3Storage,
        GoogleCloudStorageService? googleStorage,
        LocalFileStorageService localStorage)
    {
        _configuration = configuration;
        _logger = logger;
        _azureStorage = azureStorage;
        _s3Storage = s3Storage;
        _googleStorage = googleStorage;
        _localStorage = localStorage;
    }

    /// <summary>
    /// Get the active storage service based on configuration
    /// Priority: Azure > AWS > Google Cloud > Local
    /// </summary>
    public IFileStorageService GetStorageService()
    {
        var provider = _configuration["Storage:Provider"]?.ToLower() ?? "local";

        return provider switch
        {
            "azure" when _azureStorage?.IsEnabled == true => _azureStorage,
            "aws" or "s3" when _s3Storage?.IsEnabled == true => _s3Storage,
            "google" or "gcs" when _googleStorage?.IsEnabled == true => _googleStorage,
            _ => _localStorage
        };
    }

    /// <summary>
    /// Get all enabled storage services
    /// </summary>
    public IEnumerable<IFileStorageService> GetEnabledServices()
    {
        var services = new List<IFileStorageService>();

        if (_azureStorage?.IsEnabled == true)
            services.Add(_azureStorage);
        if (_s3Storage?.IsEnabled == true)
            services.Add(_s3Storage);
        if (_googleStorage?.IsEnabled == true)
            services.Add(_googleStorage);
        
        services.Add(_localStorage); // Always available as fallback

        return services;
    }
}
