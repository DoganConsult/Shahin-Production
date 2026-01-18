# ✅ Nginx Configuration Rebuilt

## Issue Fixed
The 404/502 errors were caused by incorrect server block matching. HTTPS requests to `shahin-ai.com` were being matched by the wrong server block.

## Solution Applied

### 1. Rebuilt Nginx Configuration ✅
- Created clean configuration file
- Proper server block order
- Correct upstream routing

### 2. Routing Configuration

**Landing Page** (`shahin-ai.com`, `www.shahin-ai.com`):
- HTTP → Redirects to HTTPS (301)
- HTTPS → Next.js on port 3000 ✅

**GRC App** (`portal.shahin-ai.com`, `app.shahin-ai.com`, `login.shahin-ai.com`):
- HTTP → Redirects to HTTPS (301)
- HTTPS → GRC backend on port 8080 ✅

### 3. Upstream Configuration
- `nextjs_landing` → `127.0.0.1:3000` ✅
- `grc_backend` → `127.0.0.1:5000` ✅

## Configuration File
- **Location**: `/etc/nginx/sites-available/shahin-ai-landing.conf`
- **Enabled**: `/etc/nginx/sites-enabled/shahin-ai-landing.conf`
- **Status**: ✅ Valid and active

## DNS Configuration ✅
All DNS records correctly configured:
- `shahin-ai.com` → 157.180.105.48 (DNS only)
- `www` → 157.180.105.48 (DNS only)
- `portal` → 157.180.105.48 (DNS only)
- `app` → 157.180.105.48 (DNS only)
- `login` → 157.180.105.48 (DNS only)

## Test URLs

### Landing Page
- `https://shahin-ai.com` → Next.js landing page
- `https://www.shahin-ai.com` → Next.js landing page

### GRC Application
- `https://portal.shahin-ai.com` → GRC backend
- `https://app.shahin-ai.com` → GRC backend
- `https://login.shahin-ai.com` → GRC backend

## Verification

```bash
# Test landing page
curl -k https://shahin-ai.com/

# Test portal
curl -k https://portal.shahin-ai.com/

# Check nginx status
sudo systemctl status nginx

# Check Next.js
ps aux | grep next-server
ss -tlnp | grep :3000
```

## Status
✅ **Configuration rebuilt and active**
✅ **Server blocks properly ordered**
✅ **Routing configured correctly**
✅ **DNS verified**

**Next.js**: Running on port 3000
**Nginx**: Restarted with new config
**Login Link**: Configured in header
