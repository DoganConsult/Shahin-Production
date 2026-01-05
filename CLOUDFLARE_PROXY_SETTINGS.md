# Cloudflare Proxy Settings Guide

## Recommendation: **DNS Only (Grey Cloud)** ✅

---

## For Your Current Setup

### Use DNS Only (Grey Cloud) ✅

**Why:**
1. ✅ You have Let's Encrypt SSL certificates on your server
2. ✅ Nginx is handling SSL termination
3. ✅ You're experiencing 502 errors and redirect loops with proxy
4. ✅ Direct connection avoids Cloudflare proxy issues
5. ✅ Better for API endpoints (onboarding signup)

### Don't Use Proxied (Orange Cloud) ❌

**Why Not:**
1. ❌ Causes 502 Bad Gateway errors
2. ❌ Creates redirect loops (301)
3. ❌ SSL certificate mismatch issues
4. ❌ More complex configuration needed
5. ❌ Requires Cloudflare SSL mode configuration

---

## How to Change in Cloudflare

### Step 1: Go to Cloudflare Dashboard
1. Login to Cloudflare
2. Select your domain: `shahin-ai.com`
3. Go to **DNS** → **Records**

### Step 2: Change Proxy Status
For each record (app, login, portal, shahin-ai.com, www):
1. Click the **orange cloud** icon (Proxied)
2. It will turn **grey** (DNS only)
3. Click **Save**

### Step 3: Wait for DNS Propagation
- Usually takes 1-5 minutes
- DNS changes propagate quickly

---

## Current DNS Records (Change to Grey)

| Record | Type | Value | Current | Recommended |
|--------|------|-------|---------|-------------|
| app | A | 157.180.105.48 | 🟠 Proxied | ⚪ DNS only |
| login | A | 157.180.105.48 | 🟠 Proxied | ⚪ DNS only |
| portal | A | 157.180.105.48 | 🟠 Proxied | ⚪ DNS only |
| shahin-ai.com | A | 157.180.105.48 | 🟠 Proxied | ⚪ DNS only |
| www | A | 157.180.105.48 | 🟠 Proxied | ⚪ DNS only |

---

## After Changing to DNS Only

### Benefits
- ✅ Direct connection to your server
- ✅ No proxy-related errors
- ✅ Your Let's Encrypt certificates work properly
- ✅ No SSL certificate warnings
- ✅ API endpoints work correctly
- ✅ Real client IPs in logs

### Access
- **HTTP**: `http://app.shahin-ai.com` (nginx redirects to HTTPS)
- **HTTPS**: `https://app.shahin-ai.com` (your Let's Encrypt cert)
- **API**: `https://app.shahin-ai.com/api/onboarding/signup`

---

## If You Want to Use Proxied Later

### Requirements
1. **SSL/TLS Mode**: Set to "Full" or "Full (strict)"
2. **Always Use HTTPS**: Enabled
3. **Origin Certificate**: Optional (for Full strict)
4. **Nginx Configuration**: Must accept Cloudflare IPs

### SSL/TLS Settings
- Go to **SSL/TLS** → **Overview**
- Set encryption mode to: **Full** (or **Full (strict)**)
- This ensures Cloudflare → Origin uses HTTPS correctly

---

## Quick Answer

**For your setup: Use DNS Only (Grey Cloud)** ✅

**Reason**: You have SSL certificates on your server, nginx is configured, and proxy is causing issues.

**Change**: Click the orange cloud icon in Cloudflare DNS to make it grey (DNS only).

---

## Verification After Change

```bash
# Test direct connection
curl -k https://app.shahin-ai.com/health

# Should work without 502 errors
curl -X POST https://app.shahin-ai.com/api/onboarding/signup \
  -H "Content-Type: application/json" \
  -d '{"organizationName":"Test","adminEmail":"test@test.com","tenantSlug":"test"}'
```

---

**✅ RECOMMENDATION: Use DNS Only (Grey Cloud) for all records**

---
