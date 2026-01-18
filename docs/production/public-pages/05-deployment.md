# Deployment Integration (VPS)

## Overview

This document describes how to deploy the Next.js marketing site alongside the existing GRC application using Docker Compose and Nginx reverse proxy.

## Architecture

### Current Setup

- **GrcMvc Backend**: ASP.NET Core application running on port `5000:80` (internal port 80)
- **Next.js Marketing Site**: Next.js application that will run on port `3000:3000`
- **Nginx Reverse Proxy**: Routes traffic based on domain and path
- **PostgreSQL**: Database for GRC application
- **Redis**: Caching and session storage

### Deployment Choice

**Selected Approach**: Serve marketing under the main Next app at `/` (same service); app is under `/app`.

This means:
- `shahin-ai.com/` → Next.js marketing site
- `shahin-ai.com/app` → GRC application (redirects to `portal.shahin-ai.com` or serves directly)
- `shahin-ai.com/api/` → GRC API endpoints
- `shahin-ai.com/trial` → GRC trial registration
- `shahin-ai.com/OnboardingWizard` → GRC onboarding wizard

## Docker Compose Configuration

### Updated `docker-compose.production.yml`

Add the Next.js marketing service to the existing production compose file:

```yaml
services:
  # ... existing grcmvc-prod, db-prod, redis-prod services ...

  marketing-prod:
    build:
      context: ./shahin-ai-website
      dockerfile: Dockerfile
    container_name: shahin-marketing-production
    ports:
      - "3000:3000"
    env_file:
      - .env.production
    environment:
      - NODE_ENV=production
      - NEXT_PUBLIC_API_URL=https://shahin-ai.com/api
      - NEXT_PUBLIC_SITE_URL=https://shahin-ai.com
      - PORT=3000
      - HOSTNAME=0.0.0.0
    networks:
      - grc-prod-network
    restart: always
    healthcheck:
      test: ["CMD", "wget", "--no-verbose", "--tries=1", "--spider", "http://localhost:3000/"]
      interval: 30s
      timeout: 3s
      retries: 3
      start_period: 10s
    depends_on:
      - grcmvc-prod  # Marketing site may need to call GRC API
```

**Note**: The `marketing-prod` service is added to the existing `grc-prod-network` to allow communication with the GRC backend if needed.

## Nginx Configuration

### Updated `nginx-config/shahin-ai-domains.conf`

Update the `shahin-ai.com` server block to route marketing pages to Next.js and keep GRC paths to the backend:

```nginx
# Upstream definitions
upstream grcmvc_backend {
    server 127.0.0.1:5010;  # GrcMvc backend (from docker-compose port mapping)
    keepalive 32;
}

upstream marketing_nextjs {
    server 127.0.0.1:3000;  # Next.js marketing site
    keepalive 32;
}

server {
    listen 443 ssl http2;
    listen [::]:443 ssl http2;
    server_name shahin-ai.com www.shahin-ai.com;

    # SSL certificates (Let's Encrypt)
    ssl_certificate /etc/letsencrypt/live/shahin-ai.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/shahin-ai.com/privkey.pem;
    ssl_session_timeout 1d;
    ssl_session_cache shared:SSL:50m;
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers ECDHE-ECDSA-AES128-GCM-SHA256:ECDHE-RSA-AES128-GCM-SHA256;
    ssl_prefer_server_ciphers off;

    # Security headers
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;
    add_header Referrer-Policy "strict-origin-when-cross-origin" always;

    # Gzip compression
    gzip on;
    gzip_vary on;
    gzip_min_length 1024;
    gzip_types text/plain text/css application/json application/javascript text/xml application/xml text/html;

    # ============================================================================
    # GRC Application Routes (must come before Next.js catch-all)
    # ============================================================================

    # GRC App under /app path
    location /app {
        proxy_pass http://grcmvc_backend;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header X-Forwarded-Host $host;
        proxy_read_timeout 120s;
    }

    # API endpoints (GRC backend)
    location /api/ {
        proxy_pass http://grcmvc_backend/api/;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_read_timeout 120s;
    }

    # Trial registration (GRC backend)
    location /trial {
        proxy_pass http://grcmvc_backend/trial;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    # Onboarding wizard (GRC backend)
    location /OnboardingWizard {
        proxy_pass http://grcmvc_backend/OnboardingWizard;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    # Health check (GRC backend)
    location /health {
        proxy_pass http://grcmvc_backend/health;
        proxy_http_version 1.1;
    }

    # Block admin access on marketing site
    location /admin {
        return 302 https://login.shahin-ai.com/admin/login;
    }

    # ============================================================================
    # Next.js Marketing Site (catch-all for all other paths)
    # ============================================================================

    # Next.js static assets (_next/static)
    location /_next/static {
        proxy_pass http://marketing_nextjs;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_valid 200 365d;
        expires 365d;
        add_header Cache-Control "public, immutable";
    }

    # Next.js image optimization
    location /_next/image {
        proxy_pass http://marketing_nextjs;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    # Next.js marketing site (all other paths)
    location / {
        proxy_pass http://marketing_nextjs;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header X-Forwarded-Host $host;
        
        # WebSocket support (if needed for Next.js features)
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        
        # Timeouts
        proxy_connect_timeout 60s;
        proxy_send_timeout 60s;
        proxy_read_timeout 60s;
    }
}
```

## Environment Variables

### `.env.production` (add these for Next.js)

```bash
# Next.js Marketing Site
NEXT_PUBLIC_API_URL=https://shahin-ai.com/api
NEXT_PUBLIC_SITE_URL=https://shahin-ai.com
NEXT_PUBLIC_GOOGLE_VERIFICATION=your-google-verification-code
```

## Deployment Steps

### 1. Build and Start Services

```bash
# Navigate to project root
cd /path/to/Shahin-Jan-2026

# Build and start all services (including new marketing-prod)
docker-compose -f docker-compose.production.yml up -d --build

# Verify services are running
docker-compose -f docker-compose.production.yml ps

# Check logs
docker-compose -f docker-compose.production.yml logs -f marketing-prod
docker-compose -f docker-compose.production.yml logs -f grcmvc-prod
```

### 2. Update Nginx Configuration

```bash
# Backup existing config
sudo cp /etc/nginx/sites-available/shahin-ai-domains.conf /etc/nginx/sites-available/shahin-ai-domains.conf.backup

# Copy updated config
sudo cp nginx-config/shahin-ai-domains.conf /etc/nginx/sites-available/shahin-ai-domains.conf

# Test configuration
sudo nginx -t

# Reload Nginx
sudo systemctl reload nginx
```

### 3. Verify Deployment

```bash
# Check Next.js marketing site
curl -I http://localhost:3000/

# Check GRC backend
curl -I http://localhost:5000/health

# Test from browser
# - https://shahin-ai.com/ → Should show Next.js marketing homepage
# - https://shahin-ai.com/app → Should show GRC application
# - https://shahin-ai.com/api/health → Should return GRC health check
# - https://shahin-ai.com/trial → Should show GRC trial registration
```

## Health Checks

### Next.js Marketing Site

- **Health Endpoint**: `http://localhost:3000/`
- **Expected Response**: 200 OK with HTML content
- **Check Command**: `curl -f http://localhost:3000/ || exit 1`

### GRC Backend

- **Health Endpoint**: `http://localhost:5000/health`
- **Expected Response**: 200 OK with JSON health status
- **Check Command**: `curl -f http://localhost:5000/health || exit 1`

## Caching Strategy

### Next.js Static Assets

- `/_next/static/*`: Cache for 365 days (immutable)
- `/_next/image`: No cache (dynamic optimization)

### GRC Static Assets

- `/css/*`, `/js/*`, `/lib/*`: Cache for 1 day
- `/images/*`: Cache for 7 days

## Troubleshooting

### Issue: Next.js site not accessible

**Check:**
1. Container is running: `docker ps | grep marketing-prod`
2. Port 3000 is accessible: `curl http://localhost:3000/`
3. Nginx upstream is correct: `nginx -t`
4. Check logs: `docker logs shahin-marketing-production`

### Issue: GRC paths not working

**Check:**
1. GRC container is running: `docker ps | grep grcmvc-prod`
2. Port 5000 is accessible: `curl http://localhost:5000/health`
3. Nginx location blocks are in correct order (GRC routes before Next.js catch-all)
4. Check logs: `docker logs shahin-grc-production`

### Issue: API calls failing from Next.js

**Check:**
1. `NEXT_PUBLIC_API_URL` is set correctly in container
2. CORS is configured in GRC backend for `shahin-ai.com`
3. CSRF token handling is working (check browser console)

## Rollback Procedure

If deployment fails:

```bash
# Stop new marketing service
docker-compose -f docker-compose.production.yml stop marketing-prod

# Restore previous Nginx config
sudo cp /etc/nginx/sites-available/shahin-ai-domains.conf.backup /etc/nginx/sites-available/shahin-ai-domains.conf
sudo nginx -t
sudo systemctl reload nginx

# Previous setup (all traffic to GRC) will be restored
```

## Monitoring

### Container Health

```bash
# Check all container health
docker-compose -f docker-compose.production.yml ps

# Monitor resource usage
docker stats shahin-marketing-production shahin-grc-production
```

### Application Logs

```bash
# Next.js logs
docker logs -f shahin-marketing-production

# GRC logs
docker logs -f shahin-grc-production

# Combined logs
docker-compose -f docker-compose.production.yml logs -f
```

## Security Considerations

1. **Non-root User**: Next.js Dockerfile runs as `nextjs` user (UID 1001)
2. **Network Isolation**: Both services are on `grc-prod-network` (isolated from host)
3. **Security Headers**: Nginx adds security headers for both services
4. **HTTPS Only**: All HTTP traffic redirects to HTTPS
5. **Rate Limiting**: Consider adding rate limiting for `/api/` endpoints

## Performance Optimization

1. **Next.js Standalone Output**: Uses minimal production image
2. **Static Asset Caching**: Long-term caching for immutable assets
3. **Gzip Compression**: Enabled for all text-based content
4. **Keep-Alive Connections**: Configured in upstream blocks
5. **Health Checks**: Automatic container restart on failure

## Future Enhancements

1. **CDN Integration**: Serve static assets from CDN (Cloudflare)
2. **Load Balancing**: Add multiple Next.js instances behind Nginx
3. **Blue-Green Deployment**: Zero-downtime deployments
4. **Automated SSL Renewal**: Certbot with auto-renewal cron job
5. **Monitoring Dashboard**: Prometheus + Grafana for metrics

## Related Documentation

- [Site Discovery](./00-site-discovery.md)
- [Pages Map](./01-pages-map.md)
- [SEO Implementation](./02-seo.md)
- [Performance & Accessibility](./03-performance-a11y.md)
- [Contact Form Integration](./04-contact.md)
