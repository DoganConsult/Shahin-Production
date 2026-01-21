namespace GrcMvc.Configuration
{
    /// <summary>
    /// ClickHouse OLAP database settings
    /// </summary>
    public class ClickHouseSettings
    {
        public const string SectionName = "ClickHouse";

        public string Host { get; set; } = "localhost";
        public int HttpPort { get; set; } = 8123;
        public int NativePort { get; set; } = 9000;
        public string Database { get; set; } = "grc_analytics";
        public string Username { get; set; } = "grc_analytics";
        // SECURITY: Password must be configured, no default
        public string Password { get; set; } = string.Empty;
        public int MaxPoolSize { get; set; } = 10;
        public int CommandTimeoutSeconds { get; set; } = 30;
        public bool Enabled { get; set; } = false;

        public string ConnectionString =>
            $"Host={Host};Port={HttpPort};Database={Database};Username={Username};Password={Password}";
    }

    /// <summary>
    /// Redis distributed cache settings
    /// </summary>
    public class RedisSettings
    {
        public const string SectionName = "Redis";

        public string ConnectionString { get; set; } = "localhost:6379";
        public string InstanceName { get; set; } = "GrcCache_";
        public int DefaultExpirationMinutes { get; set; } = 30;
        public int DashboardCacheMinutes { get; set; } = 5;
        public int CatalogCacheHours { get; set; } = 24;
        public bool Enabled { get; set; } = false;
        public bool UseForSignalR { get; set; } = true;
    }

    /// <summary>
    /// Kafka messaging settings
    /// </summary>
    public class KafkaSettings
    {
        public const string SectionName = "Kafka";

        public string BootstrapServers { get; set; } = "localhost:9092";
        public string GroupId { get; set; } = "grc-app";
        public bool Enabled { get; set; } = false;
        public bool AutoCreateTopics { get; set; } = true;

        // Topics
        public string DomainEventsTopic { get; set; } = "grc.domain.events";
        public string DashboardUpdatesTopic { get; set; } = "grc.dashboard.updates";
        public string AuditEventsTopic { get; set; } = "grc.audit.events";

        // Consumer settings
        public int MaxPollIntervalMs { get; set; } = 300000;
        public int SessionTimeoutMs { get; set; } = 45000;
        public string AutoOffsetReset { get; set; } = "earliest";
    }

    /// <summary>
    /// SignalR real-time settings
    /// </summary>
    public class SignalRSettings
    {
        public const string SectionName = "SignalR";

        public bool Enabled { get; set; } = true;
        public bool UseRedisBackplane { get; set; } = false;
        public int KeepAliveIntervalSeconds { get; set; } = 15;
        public int ClientTimeoutSeconds { get; set; } = 30;
        public int MaximumReceiveMessageSize { get; set; } = 32768;
    }

    /// <summary>
    /// Analytics/Dashboard projection settings
    /// </summary>
    public class AnalyticsSettings
    {
        public const string SectionName = "Analytics";

        public bool Enabled { get; set; } = false;
        public int SnapshotIntervalMinutes { get; set; } = 15;
        public int RetentionDays { get; set; } = 365;
        public bool RealTimeEnabled { get; set; } = true;
        public int MaxConcurrentProjections { get; set; } = 4;

        // Dashboard refresh settings
        public int DashboardRefreshSeconds { get; set; } = 30;
        public int TrendDataMonths { get; set; } = 12;
        public int TopActionsLimit { get; set; } = 10;
    }
}
