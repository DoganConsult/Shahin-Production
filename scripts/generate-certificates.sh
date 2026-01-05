#!/bin/bash
# Generate SSL/TLS Certificates for GrcMvc Application
# This script generates self-signed certificates for development/staging
# For production, use Let's Encrypt or purchase a certificate from a CA

set -e

CERT_DIR="src/GrcMvc/certificates"
CERT_PASSWORD="${CERT_PASSWORD:-SecureCertPassword123!}"

echo "================================================="
echo "GrcMvc SSL Certificate Generation"
echo "================================================="
echo ""

# Create certificates directory if it doesn't exist
mkdir -p "$CERT_DIR"

# Check if dotnet is available
if ! command -v dotnet &> /dev/null; then
    echo "ERROR: .NET SDK not found!"
    echo "Please install .NET 8.0 SDK first."
    exit 1
fi

echo "1. Cleaning old certificates..."
rm -f "$CERT_DIR/aspnetapp.pfx"
rm -f "$CERT_DIR/aspnetapp.crt"
rm -f "$CERT_DIR/aspnetapp.key"

echo "2. Generating self-signed certificate with dotnet..."
cd src/GrcMvc

# Generate certificate using dotnet dev-certs
dotnet dev-certs https -ep certificates/aspnetapp.pfx -p "$CERT_PASSWORD" --trust

echo "3. Extracting certificate and key (optional for nginx)..."
# Convert PFX to PEM format for nginx (if needed)
if command -v openssl &> /dev/null; then
    openssl pkcs12 -in certificates/aspnetapp.pfx -clcerts -nokeys -out certificates/aspnetapp.crt -passin pass:"$CERT_PASSWORD"
    openssl pkcs12 -in certificates/aspnetapp.pfx -nocerts -nodes -out certificates/aspnetapp.key -passin pass:"$CERT_PASSWORD"
    echo "   ✓ Certificate extracted: $CERT_DIR/aspnetapp.crt"
    echo "   ✓ Private key extracted: $CERT_DIR/aspnetapp.key"
fi

cd ../..

echo ""
echo "================================================="
echo "✓ Certificate generation complete!"
echo "================================================="
echo ""
echo "Certificate Details:"
echo "  - Location: $CERT_DIR/aspnetapp.pfx"
echo "  - Password: $CERT_PASSWORD"
echo "  - Type: Self-signed (for dev/staging only)"
echo "  - Valid for: 1 year"
echo ""
echo "IMPORTANT NOTES:"
echo "1. This is a SELF-SIGNED certificate - only for development/staging"
echo "2. Browsers will show security warnings for self-signed certificates"
echo "3. For PRODUCTION, use Let's Encrypt or purchase a certificate:"
echo "   - Let's Encrypt: https://letsencrypt.org/"
echo "   - Certbot: https://certbot.eff.org/"
echo ""
echo "4. Update .env.grcmvc.secure with this password:"
echo "   CERT_PASSWORD=$CERT_PASSWORD"
echo ""
echo "5. Certificate files created:"
echo "   - aspnetapp.pfx (for ASP.NET Core)"
echo "   - aspnetapp.crt (for nginx/apache - optional)"
echo "   - aspnetapp.key (for nginx/apache - optional)"
echo ""
echo "================================================="
echo "Next Steps:"
echo "1. Update CERT_PASSWORD in .env.grcmvc.secure"
echo "2. Rebuild Docker containers: docker-compose -f docker-compose.grcmvc.yml build"
echo "3. Restart application: docker-compose -f docker-compose.grcmvc.yml up -d"
echo "4. Test HTTPS: https://localhost:5138"
echo "================================================="
