namespace GrcMvc.Configuration
{
    /// <summary>
    /// RabbitMQ configuration settings for MassTransit
    /// </summary>
    public class RabbitMqSettings
    {
        /// <summary>
        /// RabbitMQ host address
        /// </summary>
        public string Host { get; set; } = "localhost";

        /// <summary>
        /// RabbitMQ virtual host
        /// </summary>
        public string VirtualHost { get; set; } = "/";

        /// <summary>
        /// RabbitMQ username
        /// </summary>
        public string Username { get; set; } = "guest";

        /// <summary>
        /// RabbitMQ password - SECURITY: Must be configured, no default
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// RabbitMQ port (5672 for AMQP, 5671 for AMQPS)
        /// </summary>
        public int Port { get; set; } = 5672;

        /// <summary>
        /// Whether message queue is enabled
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Use SSL/TLS connection
        /// </summary>
        public bool UseSsl { get; set; } = false;

        /// <summary>
        /// Prefetch count (number of messages to prefetch)
        /// </summary>
        public int PrefetchCount { get; set; } = 16;

        /// <summary>
        /// Concurrent message limit per consumer
        /// </summary>
        public int ConcurrencyLimit { get; set; } = 10;

        /// <summary>
        /// Retry limit for failed messages
        /// </summary>
        public int RetryLimit { get; set; } = 3;

        /// <summary>
        /// Retry intervals in seconds
        /// </summary>
        public int[] RetryIntervals { get; set; } = { 5, 15, 60 };
    }
}
