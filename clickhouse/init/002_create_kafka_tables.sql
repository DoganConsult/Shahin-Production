-- ============================================================================
-- KAFKA ENGINE TABLES - For streaming data from Kafka
-- ============================================================================

-- Events from Kafka (source table)
CREATE TABLE IF NOT EXISTS grc_analytics.kafka_events
(
    event_id String,
    tenant_id String,
    event_type String,
    entity_type String,
    entity_id String,
    action String,
    actor String,
    payload String,
    timestamp String
)
ENGINE = Kafka()
SETTINGS
    kafka_broker_list = 'kafka:9092',
    kafka_topic_list = 'grc.domain.events',
    kafka_group_name = 'clickhouse-events-consumer',
    kafka_format = 'JSONEachRow',
    kafka_num_consumers = 1,
    kafka_max_block_size = 1048576;

-- Materialized view to process Kafka events into raw events table
CREATE MATERIALIZED VIEW IF NOT EXISTS grc_analytics.mv_kafka_to_events
TO grc_analytics.events_raw
AS SELECT
    toUUID(event_id) as event_id,
    toUUID(tenant_id) as tenant_id,
    event_type,
    entity_type,
    entity_id,
    action,
    actor,
    payload,
    parseDateTimeBestEffort(timestamp) as event_timestamp,
    now() as ingested_at
FROM grc_analytics.kafka_events;

-- ============================================================================
-- CDC TABLES - For Debezium CDC from PostgreSQL
-- ============================================================================

-- Risks CDC
CREATE TABLE IF NOT EXISTS grc_analytics.kafka_cdc_risks
(
    before String,
    after String,
    source String,
    op String,
    ts_ms Int64
)
ENGINE = Kafka()
SETTINGS
    kafka_broker_list = 'kafka:9092',
    kafka_topic_list = 'grc.public.Risks',
    kafka_group_name = 'clickhouse-cdc-risks',
    kafka_format = 'JSONEachRow';

-- Controls CDC
CREATE TABLE IF NOT EXISTS grc_analytics.kafka_cdc_controls
(
    before String,
    after String,
    source String,
    op String,
    ts_ms Int64
)
ENGINE = Kafka()
SETTINGS
    kafka_broker_list = 'kafka:9092',
    kafka_topic_list = 'grc.public.Controls',
    kafka_group_name = 'clickhouse-cdc-controls',
    kafka_format = 'JSONEachRow';

-- Assessments CDC
CREATE TABLE IF NOT EXISTS grc_analytics.kafka_cdc_assessments
(
    before String,
    after String,
    source String,
    op String,
    ts_ms Int64
)
ENGINE = Kafka()
SETTINGS
    kafka_broker_list = 'kafka:9092',
    kafka_topic_list = 'grc.public.Assessments',
    kafka_group_name = 'clickhouse-cdc-assessments',
    kafka_format = 'JSONEachRow';

-- WorkflowTasks CDC
CREATE TABLE IF NOT EXISTS grc_analytics.kafka_cdc_tasks
(
    before String,
    after String,
    source String,
    op String,
    ts_ms Int64
)
ENGINE = Kafka()
SETTINGS
    kafka_broker_list = 'kafka:9092',
    kafka_topic_list = 'grc.public.WorkflowTasks',
    kafka_group_name = 'clickhouse-cdc-tasks',
    kafka_format = 'JSONEachRow';

-- Evidence CDC
CREATE TABLE IF NOT EXISTS grc_analytics.kafka_cdc_evidence
(
    before String,
    after String,
    source String,
    op String,
    ts_ms Int64
)
ENGINE = Kafka()
SETTINGS
    kafka_broker_list = 'kafka:9092',
    kafka_topic_list = 'grc.public.Evidences',
    kafka_group_name = 'clickhouse-cdc-evidence',
    kafka_format = 'JSONEachRow';
