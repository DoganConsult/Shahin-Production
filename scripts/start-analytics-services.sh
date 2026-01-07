#!/bin/bash
# Start analytics infrastructure services (ClickHouse, Kafka, Debezium, Redis)

set -e

echo "🚀 Starting Analytics Infrastructure Services..."

# Start services
echo "Starting ClickHouse..."
docker-compose up -d clickhouse

echo "Starting Zookeeper..."
docker-compose up -d zookeeper

echo "Waiting for Zookeeper to be ready..."
sleep 10

echo "Starting Kafka..."
docker-compose up -d kafka

echo "Waiting for Kafka to be ready..."
sleep 20

echo "Starting Kafka Connect (Debezium)..."
docker-compose up -d kafka-connect

echo "Starting Redis..."
docker-compose up -d redis

echo "Waiting for services to be healthy..."
sleep 15

# Check ClickHouse
echo "Checking ClickHouse..."
if docker exec grc-clickhouse wget -q --spider http://localhost:8123/ping; then
    echo "✅ ClickHouse is healthy"
else
    echo "⚠️ ClickHouse health check failed"
fi

# Check Kafka
echo "Checking Kafka..."
if docker exec grc-kafka kafka-broker-api-versions --bootstrap-server localhost:9092 > /dev/null 2>&1; then
    echo "✅ Kafka is healthy"
else
    echo "⚠️ Kafka health check failed"
fi

# Check Redis
echo "Checking Redis..."
if docker exec grc-redis redis-cli ping > /dev/null 2>&1; then
    echo "✅ Redis is healthy"
else
    echo "⚠️ Redis health check failed"
fi

echo ""
echo "📊 Analytics Services Status:"
docker-compose ps clickhouse zookeeper kafka kafka-connect redis

echo ""
echo "✅ Analytics infrastructure started!"
echo ""
echo "Next steps:"
echo "1. Initialize ClickHouse schema: docker exec -i grc-clickhouse clickhouse-client < scripts/clickhouse-init.sql"
echo "2. Configure Debezium: curl -X POST http://localhost:8083/connectors -H 'Content-Type: application/json' -d @etc/debezium-connectors/postgres-connector.json"
echo "3. Enable analytics in appsettings.json: Set 'ClickHouse:Enabled' and 'Analytics:Enabled' to true"
echo "4. Restart the application"
