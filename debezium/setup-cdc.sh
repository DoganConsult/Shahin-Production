#!/bin/bash
# ============================================================================
# Debezium CDC Setup Script
# Registers PostgreSQL connector and creates required database objects
# ============================================================================

set -e

DEBEZIUM_URL="${DEBEZIUM_URL:-http://localhost:8083}"
POSTGRES_HOST="${POSTGRES_HOST:-localhost}"
POSTGRES_PORT="${POSTGRES_PORT:-5433}"
POSTGRES_USER="${POSTGRES_USER:-postgres}"
POSTGRES_PASSWORD="${POSTGRES_PASSWORD:-postgres}"
POSTGRES_DB="${POSTGRES_DB:-GrcMvcDb}"

echo "============================================"
echo "GRC CDC Setup - Debezium PostgreSQL Connector"
echo "============================================"

# Wait for Debezium Connect to be ready
echo "Waiting for Debezium Connect to be ready..."
until curl -s "$DEBEZIUM_URL/" > /dev/null 2>&1; do
    echo "  Waiting for Debezium Connect..."
    sleep 5
done
echo "✅ Debezium Connect is ready"

# Create PostgreSQL publication for CDC
echo "Creating PostgreSQL publication..."
PGPASSWORD=$POSTGRES_PASSWORD psql -h $POSTGRES_HOST -p $POSTGRES_PORT -U $POSTGRES_USER -d $POSTGRES_DB <<EOF
-- Enable logical replication (if not already enabled)
-- Note: This requires wal_level = logical in postgresql.conf

-- Drop existing publication if exists
DROP PUBLICATION IF EXISTS grc_publication;

-- Create publication for CDC tables
CREATE PUBLICATION grc_publication FOR TABLE
    "Risks",
    "Controls",
    "Assessments",
    "AssessmentRequirements",
    "WorkflowTasks",
    "WorkflowInstances",
    "Evidences",
    "Plans",
    "AuditEvents";

-- Verify publication
SELECT * FROM pg_publication WHERE pubname = 'grc_publication';
EOF

echo "✅ PostgreSQL publication created"

# Check if connector already exists
EXISTING=$(curl -s "$DEBEZIUM_URL/connectors" | grep -o "grc-postgres-connector" || true)

if [ -n "$EXISTING" ]; then
    echo "Connector already exists, deleting..."
    curl -s -X DELETE "$DEBEZIUM_URL/connectors/grc-postgres-connector"
    sleep 2
fi

# Register the connector
echo "Registering PostgreSQL connector..."
curl -s -X POST "$DEBEZIUM_URL/connectors" \
    -H "Content-Type: application/json" \
    -d @register-postgres-connector.json

echo ""
echo "✅ Connector registered"

# Wait and check status
sleep 5
echo "Checking connector status..."
curl -s "$DEBEZIUM_URL/connectors/grc-postgres-connector/status" | jq .

echo ""
echo "============================================"
echo "CDC Setup Complete!"
echo "============================================"
echo ""
echo "Kafka topics created:"
echo "  - grc.public.Risks"
echo "  - grc.public.Controls"
echo "  - grc.public.Assessments"
echo "  - grc.public.AssessmentRequirements"
echo "  - grc.public.WorkflowTasks"
echo "  - grc.public.WorkflowInstances"
echo "  - grc.public.Evidences"
echo "  - grc.public.Plans"
echo "  - grc.public.AuditEvents"
echo ""
echo "Monitor at: http://localhost:8080 (Kafka UI)"
