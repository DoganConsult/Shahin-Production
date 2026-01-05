#!/bin/bash
# Production Deployment Script for GRC System
# Deploy to: portal.shahin-ai.com (157.180.105.48)

set -e

echo "🚀 GRC System - Production Deployment"
echo "======================================"

# Configuration
SERVER_IP="157.180.105.48"
SERVER_USER="root"
DEPLOY_PATH="/opt/grc-system"
DOMAIN="portal.shahin-ai.com"

echo "📦 Creating deployment package..."
tar -czf grc-system-deploy.tar.gz \
    --exclude='bin' \
    --exclude='obj' \
    --exclude='node_modules' \
    --exclude='.git' \
    --exclude='*.log' \
    src/ docker-compose.yml .env

echo "📤 Uploading to server..."
scp grc-system-deploy.tar.gz ${SERVER_USER}@${SERVER_IP}:/tmp/

echo "🔧 Deploying on server..."
ssh ${SERVER_USER}@${SERVER_IP} << 'ENDSSH'
set -e

# Create deployment directory
mkdir -p /opt/grc-system
cd /opt/grc-system

# Extract files
tar -xzf /tmp/grc-system-deploy.tar.gz
rm /tmp/grc-system-deploy.tar.gz

# Stop existing containers
docker compose down || true

# Pull latest images and rebuild
docker compose up -d --build

# Show status
docker compose ps

echo "✅ Deployment complete!"
echo "🌐 Application should be accessible at: http://157.180.105.48:8080"
echo "📝 Configure Nginx reverse proxy for HTTPS access via portal.shahin-ai.com"
ENDSSH

echo ""
echo "✅ Deployment completed successfully!"
echo ""
echo "Next steps:"
echo "1. Configure Nginx reverse proxy (see nginx-config.conf)"
echo "2. Set up SSL certificate with Let's Encrypt"
echo "3. Access: https://portal.shahin-ai.com"
echo ""
echo "Login credentials:"
echo "  User: Info@doganconsult.com"
echo "  Pass: AhmEma$123456"
