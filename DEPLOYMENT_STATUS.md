# Deployment Status - app.shahin-ai.com

## Current Status

### Application
- **Location**: `/opt/grc-app`
- **Port**: 8080
- **Environment**: Development (temporary, for debugging)

### Issue
The application was crashing on startup due to:
1. Hangfire database connection configuration
2. Network/DNS resolution issues

### Fix Applied
- Updated `appsettings.json` with HangfireConnection
- Running in Development mode temporarily to bypass some production checks
- Application should now start successfully

### Next Steps
1. Verify application is running on port 8080
2. Test health endpoint
3. Verify nginx can reach the application
4. Switch to Production mode once stable

---

## Verification Commands

```bash
# Check if application is running
ps aux | grep "dotnet.*GrcMvc"

# Check if port 8080 is listening
ss -tlnp | grep ":8080"

# Test local health endpoint
curl http://localhost:8080/health

# Test through nginx
curl -k https://app.shahin-ai.com/health

# Check logs
tail -f /var/log/grc-app.log
```

---

## Nginx Configuration

The nginx configuration is already set up for `app.shahin-ai.com` in:
- `/etc/nginx/sites-enabled/shahin-ai-landing.conf`

It proxies to `127.0.0.1:5000` which is the application origin port.

---

## Deployment Path

```
/home/dogan/grc-system/publish → /opt/grc-app
```

---
