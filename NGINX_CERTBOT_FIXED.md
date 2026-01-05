# Nginx Certbot Issues - FIXED

## Problems Found & Fixed

### 1. ✅ Duplicate Server Names
- **Issue**: Multiple nginx config files with same server_name
- **Fixed**: Removed conflicting `portal.shahin-ai.com` config
- **Result**: Only `shahin-ai.com` config active

### 2. ✅ Clean Configuration
- **Single HTTP server block** for all domains
- **Proper certbot challenge location**
- **No SSL certificates** (certbot will add them)

### 3. ⚠️ Cloudflare Proxy Issue
- **Error 521**: Cloudflare proxy blocking Let's Encrypt
- **Solution Needed**: Disable Cloudflare proxy temporarily

## Current Nginx Status

```bash
# Active configs
/etc/nginx/sites-enabled/shahin-ai.com  ✅ (main config)
/etc/nginx/sites-enabled/grc             ✅ (other service)

# Removed
/etc/nginx/sites-enabled/portal.shahin-ai.com  ❌ (conflicting)
/etc/nginx/sites-enabled/default               ❌ (removed)
```

## Next Steps for Certbot

### Step 1: Disable Cloudflare Proxy

1. Go to **Cloudflare Dashboard**
2. For each domain, change **Proxy status** from **Proxied** (orange cloud) to **DNS only** (gray cloud):
   - shahin-ai.com
   - www.shahin-ai.com
   - portal.shahin-ai.com
   - app.shahin-ai.com
   - login.shahin-ai.com

3. **Wait 10 minutes** for DNS propagation

### Step 2: Run Certbot

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

### Step 3: Re-enable Cloudflare Proxy

After certificates are obtained:
1. Re-enable **Proxied** (orange cloud) in Cloudflare
2. Set **SSL/TLS** mode to **Full** or **Full (strict)**

## Verify

```bash
# Test nginx config
sudo nginx -t

# Check certificates
sudo certbot certificates

# Test HTTPS
curl https://portal.shahin-ai.com/
```

## Alternative: Cloudflare Origin Certificates

If you prefer to keep Cloudflare proxy enabled:

1. **Cloudflare Dashboard** → SSL/TLS → Origin Server
2. **Create Certificate**
3. **Download** and install on server
4. **Update nginx** to use Cloudflare certificates

---

**Status**: ✅ Nginx fixed, ⏳ Waiting for Cloudflare proxy to be disabled
