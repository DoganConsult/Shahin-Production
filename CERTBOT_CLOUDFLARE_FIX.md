# Certbot + Cloudflare Fix

## Issue Identified

Certbot failed with error **521** which means:
- **Cloudflare Proxy is Active**: Your domains are behind Cloudflare's proxy
- **Cloudflare is Blocking**: Let's Encrypt can't reach your server directly
- **DNS Only Required**: Need to temporarily disable Cloudflare proxy

## Solutions

### Option 1: Disable Cloudflare Proxy (Recommended for Certbot)

1. **Go to Cloudflare Dashboard**
2. **DNS Settings** for each domain:
   - `shahin-ai.com`
   - `www.shahin-ai.com`
   - `portal.shahin-ai.com`
   - `app.shahin-ai.com`
   - `login.shahin-ai.com`

3. **Change Proxy Status**:
   - Click the orange cloud icon (Proxied) → Gray cloud (DNS only)
   - Wait 5-10 minutes for DNS propagation

4. **Run Certbot Again**:
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

5. **Re-enable Cloudflare Proxy** (after certificates are obtained)

### Option 2: Use Cloudflare Origin Certificates

Instead of Let's Encrypt, use Cloudflare's origin certificates:

1. **Cloudflare Dashboard** → SSL/TLS → Origin Server
2. **Create Certificate** → Generate
3. **Download Certificate** (fullchain.pem and privkey.pem)
4. **Upload to Server**:
   ```bash
   sudo mkdir -p /etc/letsencrypt/live/portal.shahin-ai.com
   sudo cp fullchain.pem /etc/letsencrypt/live/portal.shahin-ai.com/
   sudo cp privkey.pem /etc/letsencrypt/live/portal.shahin-ai.com/privkey.pem
   ```

5. **Update Nginx Config** to use these certificates

### Option 3: Cloudflare SSL/TLS Settings

1. **Cloudflare Dashboard** → SSL/TLS
2. **Encryption Mode**: Set to "Full" or "Full (strict)"
3. **Always Use HTTPS**: Enable
4. **Automatic HTTPS Rewrites**: Enable

This allows Cloudflare to handle SSL termination.

## Current Status

- ✅ Nginx config fixed (duplicate server names removed)
- ✅ Certbot plugin installed
- ⚠️ Cloudflare proxy blocking Let's Encrypt
- ⏳ Need to disable Cloudflare proxy or use alternative method

## After Fixing Cloudflare

Once certificates are obtained:

1. **Verify Certificates**:
   ```bash
   sudo certbot certificates
   ```

2. **Test HTTPS**:
   ```bash
   curl https://portal.shahin-ai.com/
   ```

3. **Re-enable Cloudflare Proxy** (if you disabled it)

4. **Set Cloudflare SSL/TLS** to "Full" mode

## Duplicate Server Names Fixed

The duplicate server name warnings are from other config files. The main config is now clean, but you may want to check:

```bash
sudo ls -la /etc/nginx/sites-enabled/
sudo grep -r "server_name" /etc/nginx/sites-enabled/
```

Remove any conflicting config files if needed.

---

**Next Step**: Disable Cloudflare proxy for all domains, wait 10 minutes, then run certbot again.
