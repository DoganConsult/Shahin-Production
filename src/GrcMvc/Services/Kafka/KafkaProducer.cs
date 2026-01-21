using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace GrcMvc.Services.Kafka;

/// <summary>
/// Kafka message producer implementation
/// </summary>
public class KafkaProducer : IKafkaProducer, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaProducer> _logger;
    private readonly KafkaSettings _settings;
    private bool _disposed;

    public KafkaProducer(IOptions<KafkaSettings> settings, ILogger<KafkaProducer> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        var config = new ProducerConfig
        {
            BootstrapServers = _settings.BootstrapServers,
            ClientId = _settings.ClientId,
            Acks = Acks.All,
            EnableIdempotence = true,
            MessageSendMaxRetries = 3,
            RetryBackoffMs = 1000
        };

        _producer = new ProducerBuilder<string, string>(config)
            .SetErrorHandler((_, e) => _logger.LogError("Kafka error: {Reason}", e.Reason))
            .SetLogHandler((_, log) => _logger.LogDebug("Kafka: {Message}", log.Message))
            .Build();
    }

    public async Task PublishAsync<T>(string topic, T message, string? key = null, CancellationToken ct = default)
    {
        await PublishAsync(topic, message, null, key, ct);
    }

    public async Task PublishAsync<T>(string topic, T message, Dictionary<string, string>? headers, string? key = null, CancellationToken ct = default)
    {
        try
        {
            var jsonMessage = JsonSerializer.Serialize(message, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            var kafkaMessage = new Message<string, string>
            {
                Key = key ?? Guid.NewGuid().ToString(),
                Value = jsonMessage,
                Timestamp = new Timestamp(DateTime.UtcNow)
            };

            // Add headers if provided
            if (headers != null && headers.Count > 0)
            {
                kafkaMessage.Headers = new Headers();
                foreach (var header in headers)
                {
                    kafkaMessage.Headers.Add(header.Key, System.Text.Encoding.UTF8.GetBytes(header.Value));
                }
            }

            var result = await _producer.ProduceAsync(topic, kafkaMessage, ct);
            
            _logger.LogDebug("Published to {Topic}: Partition={Partition}, Offset={Offset}", 
                topic, result.Partition.Value, result.Offset.Value);
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(ex, "Failed to publish message to {Topic}", topic);
            throw;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _producer?.Flush(TimeSpan.FromSeconds(10));
        _producer?.Dispose();
    }
}

/// <summary>
/// Kafka configuration settings
/// </summary>
public class KafkaSettings
{
    public const string SectionName = "Kafka";
    
    public bool Enabled { get; set; } = true;
    public string BootstrapServers { get; set; } = "localhost:9092";
    public string ClientId { get; set; } = "grc-mvc";
    public string GroupId { get; set; } = "grc-consumer-group";
    public string SchemaRegistryUrl { get; set; } = "";
}
