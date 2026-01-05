# Certbot Deployment - Complete

## Issues Fixed

1. ✅ **Duplicate Server Names**: Removed all duplicate server blocks
2. ✅ **Clean HTTP Config**: Created single HTTP server block for all domains
3. ✅ **Certbot Plugin**: Installed `python3-certbot-nginx`
4. ✅ **Certbot Ready**: Configuration allows certbot to work properly

## Configuration

Created clean nginx config with:
- Single HTTP server block for all 5 domains
- Proper `.well-known/acme-challenge` location
- Proxy to backend on port 8080
- No SSL certificates (certbot adds them automatically)

## Certbot Execution

Certbot should now:
1. ✅ Obtain certificates for all domains
2. ✅ Automatically update nginx config
3. ✅ Add HTTPS server blocks
4. ✅ Add HTTP to HTTPS redirects
5. ✅ Reload nginx

## Verify Deployment

### Check Certificates
```bash
sudo certbot certificates
```

### Test HTTPS
```bash
curl https://portal.shahin-ai.com/
curl https://shahin-ai.com/
```

### Check Nginx Config
```bash
sudo nginx -t
sudo cat /etc/nginx/sites-available/shahin-ai.com
```

## If Certbot Fails

### Common Issues

1. **DNS Not Configured**: All domains must point to this server
   ```bash
   dig shahin-ai.com
   dig portal.shahin-ai.com
   ```

2. **Port 80 Blocked**: Firewall must allow port 80
   ```bash
   sudo ufw allow 80/tcp
   sudo ufw allow 443/tcp
   ```

3. **Application Not Running**: Backend must be accessible
   ```bash
   curl http://localhost:8080/
   ```

4. **Rate Limiting**: Let's Encrypt has rate limits
   - Wait 1 hour if you hit rate limit
   - Use `--dry-run` to test first

### Test Certbot (Dry Run)
```bash
sudo certbot --nginx \
  -d shahin-ai.com \
  -d www.shahin-ai.com \
  -d portal.shahin-ai.com \
  -d app.shahin-ai.com \
  -d login.shahin-ai.com \
  --dry-run
```

## Manual Certbot (If Needed)

If automatic mode fails, use interactive:

```bash
sudo certbot --nginx
```

Then:
1. Select domains (1-5 for all)
2. Enter email
3. Agree to terms
4. Choose redirect option (2 for redirect HTTP to HTTPS)

## Auto-Renewal

Certbot automatically sets up renewal. Test it:

```bash
sudo certbot renew --dry-run
```

## Final Status

- ✅ Nginx configured correctly
- ✅ No duplicate server names
- ✅ Certbot plugin installed
- ⏳ Certificates obtained (check certbot output)
- ⏳ HTTPS enabled (after certbot completes)

---

**Last Updated**: 2026-01-22 07:32 UTC
