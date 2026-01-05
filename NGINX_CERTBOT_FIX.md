# Nginx Certbot Fix

## Issues Fixed

1. **Duplicate Server Names**: Removed conflicting server blocks
2. **HTTP-Only Config**: Created clean HTTP config for certbot
3. **Certbot-Ready**: Configuration allows certbot to modify and add HTTPS

## Changes Made

1. Created `nginx-shahin-ai-certbot-ready.conf`:
   - Single HTTP server block for all domains
   - Proper `.well-known/acme-challenge` location for certbot
   - Proxy to backend on port 8080
   - No SSL certificates (certbot will add them)

2. Replaced nginx config:
   ```bash
   sudo cp nginx-shahin-ai-certbot-ready.conf /etc/nginx/sites-available/shahin-ai.com
   sudo nginx -t
   sudo systemctl reload nginx
   ```

## Running Certbot

Now you can run certbot:

```bash
sudo certbot --nginx \
  -d shahin-ai.com \
  -d www.shahin-ai.com \
  -d portal.shahin-ai.com \
  -d app.shahin-ai.com \
  -d login.shahin-ai.com \
  --non-interactive \
  --agree-tos \
  --email your-email@example.com \
  --redirect
```

Certbot will:
1. Obtain certificates for all domains
2. Automatically update nginx config
3. Add HTTPS server blocks
4. Add HTTP to HTTPS redirects
5. Reload nginx

## Manual Certbot (Interactive)

If you prefer interactive mode:

```bash
sudo certbot --nginx
```

Then select the domains you want certificates for.

## Verify After Certbot

```bash
# Test nginx config
sudo nginx -t

# Check certificates
sudo certbot certificates

# Test HTTPS
curl https://portal.shahin-ai.com/
```

## Troubleshooting

### If certbot fails with "No such file or directory"
- Make sure nginx is running: `sudo systemctl status nginx`
- Check DNS: All domains must point to this server
- Check firewall: Ports 80 and 443 must be open

### If certbot fails with "Connection refused"
- Make sure the application is listening on port 8080
- Check: `lsof -i :8080`

### If certbot fails with "Invalid response"
- Make sure DNS is properly configured
- Wait a few minutes for DNS propagation
- Check: `dig shahin-ai.com`
