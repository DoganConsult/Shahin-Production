#!/bin/bash
# Start Analytics Stack (Apache Licensed Tools)
# Usage: ./scripts/start-analytics.sh

set -e

echo "🚀 Starting GRC Analytics Stack..."
echo "=================================="

cd /home/dogan/grc-system

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker is not running. Please start Docker first."
    exit 1
fi

# Start only analytics services
echo "📊 Starting Apache Superset..."
docker-compose up -d superset-db superset

echo "📈 Starting Grafana..."
docker-compose up -d grafana

echo "🔄 Starting n8n Workflow Automation..."
docker-compose up -d n8n

echo "📉 Starting Metabase..."
docker-compose up -d metabase

# Wait for services to be healthy
echo ""
echo "⏳ Waiting for services to start..."
sleep 30

# Check health
echo ""
echo "🔍 Checking service health..."
echo ""

# Superset
if curl -s http://localhost:8088/health > /dev/null 2>&1; then
    echo "✅ Superset:  http://localhost:8088 (admin / admin123)"
else
    echo "⏳ Superset:  Starting... (may take 1-2 minutes)"
fi

# Grafana
if curl -s http://localhost:3030/api/health > /dev/null 2>&1; then
    echo "✅ Grafana:   http://localhost:3030 (admin / admin123)"
else
    echo "⏳ Grafana:   Starting..."
fi

# n8n
if curl -s http://localhost:5678/healthz > /dev/null 2>&1; then
    echo "✅ n8n:       http://localhost:5678 (admin / admin123)"
else
    echo "⏳ n8n:       Starting..."
fi

# Metabase
if curl -s http://localhost:3033/api/health > /dev/null 2>&1; then
    echo "✅ Metabase:  http://localhost:3033 (setup on first visit)"
else
    echo "⏳ Metabase:  Starting... (may take 2-3 minutes)"
fi

echo ""
echo "=================================="
echo "📊 Analytics Stack Started!"
echo ""
echo "Access Points:"
echo "  • Superset (BI):      http://localhost:8088"
echo "  • Grafana (Monitor):  http://localhost:3030"
echo "  • n8n (Workflows):    http://localhost:5678"
echo "  • Metabase (Reports): http://localhost:3033"
echo ""
echo "Next Steps:"
echo "  1. Open Superset and connect GRC database"
echo "  2. Create dashboards for compliance metrics"
echo "  3. Set up Grafana alerts for critical thresholds"
echo "  4. Build n8n workflows for automation"
echo ""
echo "To stop: docker-compose down"
echo "=================================="
