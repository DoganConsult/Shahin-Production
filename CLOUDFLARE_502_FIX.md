# Cloudflare 502 Bad Gateway - FIX

## Issue
Nginx is trying to connect to port **8082** but the application is running on port **8080**.

## Error in Logs
```
upstream: "http://127.0.0.1:8082/health"
connect() failed (111: Connection refused)
```

## Solution
There's a conflicting nginx configuration that's overriding the correct upstream setting.

## Status
- ✅ Application running on port 8080
- ✅ Health check working locally
- ❌ Nginx trying to connect to wrong port (8082)
- ❌ Need to fix nginx configuration

## Next Steps
1. Find the conflicting nginx config with port 8082
2. Update it to use port 8080 or the grc_backend upstream
3. Reload nginx
4. Test the domain

---
