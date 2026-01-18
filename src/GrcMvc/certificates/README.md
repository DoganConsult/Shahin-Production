# SSL Certificates Directory

This directory contains SSL/TLS certificates for HTTPS configuration.

## Development Certificate Generation

Generate a development certificate using the .NET CLI:

```bash
# Navigate to this directory
cd src/GrcMvc

# Generate a development certificate
dotnet dev-certs https -ep certificates/aspnetapp.pfx -p "DevPassword123!"

# Trust the certificate (Windows/macOS only)
dotnet dev-certs https --trust
```

## Production Certificate

For production, obtain a CA-signed certificate from one of:

1. **Let's Encrypt** (Free, automated):
   ```bash
   # Using certbot
   certbot certonly --standalone -d shahin-ai.com -d www.shahin-ai.com
   
   # Convert to PFX format
   openssl pkcs12 -export -out aspnetapp.pfx \
     -inkey /etc/letsencrypt/live/shahin-ai.com/privkey.pem \
     -in /etc/letsencrypt/live/shahin-ai.com/fullchain.pem \
     -password pass:YOUR_SECURE_PASSWORD
   ```

2. **Azure Key Vault** (Recommended for Azure deployments):
   - Store certificate in Azure Key Vault
   - Configure Kestrel to load from Key Vault
   - Auto-renewal handled by Azure

3. **Commercial CA** (DigiCert, Comodo, etc.):
   - Purchase and download certificate
   - Convert to PFX format if needed

## Environment Variables

Set these environment variables in production:

```bash
CERT_PATH=/app/certificates/aspnetapp.pfx
CERT_PASSWORD=your_secure_password
```

## Security Notes

- **NEVER commit certificates or passwords to source control**
- Store the certificate password in a secure secrets manager
- Rotate certificates before expiration (typically 1 year for commercial, 90 days for Let's Encrypt)
- Use strong passwords (20+ characters, mixed case, numbers, symbols)
