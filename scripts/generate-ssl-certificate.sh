#!/bin/bash

# SSL Certificate Generation Script for GrcMvc
# This script generates a development SSL certificate for HTTPS support

set -e

echo "🔐 Generating SSL Certificate for GrcMvc..."

# Configuration
CERT_DIR="src/GrcMvc/certificates"
CERT_PASSWORD="SecurePassword123!"
CERT_FILE="aspnetapp.pfx"

# Create certificates directory if it doesn't exist
mkdir -p "$CERT_DIR"

# Generate the certificate
echo "📝 Generating certificate..."
dotnet dev-certs https -ep "$CERT_DIR/$CERT_FILE" -p "$CERT_PASSWORD"

# Trust the certificate (optional, for local development)
echo "🔒 Trusting certificate..."
dotnet dev-certs https --trust

# Verify certificate was created
if [ -f "$CERT_DIR/$CERT_FILE" ]; then
    echo "✅ Certificate generated successfully!"
    echo "📁 Location: $CERT_DIR/$CERT_FILE"
    echo "🔑 Password: $CERT_PASSWORD"
    echo ""
    echo "⚠️  IMPORTANT: Update .env.grcmvc.production with:"
    echo "   CERT_PASSWORD=$CERT_PASSWORD"
    echo "   ASPNETCORE_Kestrel__Certificates__Default__Password=$CERT_PASSWORD"
    echo "   ASPNETCORE_Kestrel__Certificates__Default__Path=/app/certificates/$CERT_FILE"
else
    echo "❌ Certificate generation failed!"
    exit 1
fi

echo ""
echo "🎉 SSL Certificate setup complete!"
echo "Next steps:"
echo "1. Update .env.grcmvc.production with certificate password"
echo "2. Rebuild Docker containers: docker-compose -f docker-compose.grcmvc.yml build --no-cache"
echo "3. Start containers: docker-compose -f docker-compose.grcmvc.yml up -d"
echo "4. Test HTTPS: curl https://localhost:5138/health"
