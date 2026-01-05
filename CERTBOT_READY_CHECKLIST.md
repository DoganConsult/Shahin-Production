# Certbot Ready Checklist

## ✅ Pre-Certbot Checklist

### 1. Nginx Configuration
- [x] ✅ Nginx config is clean and valid
- [x] ✅ No critical errors (warnings are OK)
- [x] ✅ Single HTTP server block for all domains
- [x] ✅ Proper `.well-known/acme-challenge` location
- [x] ✅ Nginx is running

### 2. Certbot Plugin
- [x] ✅ `python3-certbot-nginx` installed
- [x] ✅ Certbot command available

### 3. Server Status
- [x] ✅ Application running (check port 8080)
- [x] ✅ Nginx running and accessible
- [x] ✅ Port 80 open (for HTTP challenge)

### 4. DNS Configuration (YOU NEED TO DO THIS)
- [ ] ⏳ Change all 5 domains to **DNS only** in Cloudflare
- [ ] ⏳ Wait 10-15 minutes for propagation
- [ ] ⏳ Verify DNS: `dig shahin-ai.com`

## 🚀 Ready to Run Certbot

Once all DNS records are set to **DNS only** and you've waited 10-15 minutes:

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

## 📋 After Certbot Succeeds

1. **Verify Certificates**:
   ```bash
   sudo certbot certificates
   ```

2. **Test HTTPS**:
   ```bash
   curl https://portal.shahin-ai.com/
   curl https://shahin-ai.com/
   ```

3. **Re-enable Cloudflare Proxy** (optional):
   - Change all domains back to **Proxied** (orange cloud)
   - Set Cloudflare SSL/TLS to **Full**

4. **Verify Nginx Config**:
   ```bash
   sudo nginx -t
   sudo systemctl reload nginx
   ```

---

**Current Status**: ✅ **READY** - Just need to change Cloudflare DNS to DNS only
