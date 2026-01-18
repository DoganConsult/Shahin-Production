# DNS Records Required - Complete List

**Domain**: `shahin-ai.com`  
**DNS Provider**: Cloudflare  
**Last Updated**: January 12, 2026

---

## 📋 Summary Table

| Environment | Record Type | Name | Target/Value | Purpose | Status |
|------------|-------------|------|--------------|---------|--------|
| **Production** | A | `@` (root) | `157.180.105.48` | Landing Page | ✅ Required |
| **Production** | A | `www` | `157.180.105.48` | Landing Page (WWW) | ✅ Required |
| **Production** | A | `portal` | `157.180.105.48` | GRC Portal Application | ✅ Required |
| **Production** | A | `app` | `157.180.105.48` | GRC Application (Alias) | ✅ Required |
| **Production** | A | `login` | `157.180.105.48` | Login Redirect | ✅ Required |
| **Production** | A | `api` | `157.180.105.48` | API Endpoints | ⚠️ Optional |
| **Staging** | A | `staging` | `157.180.105.48` | Staging Environment | ⚠️ Optional |
| **Development** | A | `dev` | `157.180.105.48` | Development Environment | ⚠️ Optional |

**Total Required**: 5 DNS records (minimum)  
**Total Optional**: 3 DNS records

---

## 🌍 Production DNS Records (Required)

### 1. Root Domain (shahin-ai.com)

```
Type:        A
Name:        @
Content:     157.180.105.48
Proxy:       DNS only (⚪ Gray cloud) OR Proxied (🟠 Orange cloud)
TTL:         Auto
Priority:    -
```

**Purpose**: Main landing page and marketing site  
**Backend**: Next.js application on port 3000  
**Access**: `https://shahin-ai.com`

---

### 2. WWW Subdomain (www.shahin-ai.com)

```
Type:        A
Name:        www
Content:     157.180.105.48
Proxy:       DNS only (⚪ Gray cloud) OR Proxied (🟠 Orange cloud)
TTL:         Auto
Priority:    -
```

**Purpose**: WWW version of landing page (redirects to root)  
**Backend**: Next.js application on port 3000  
**Access**: `https://www.shahin-ai.com`

---

### 3. Portal Subdomain (portal.shahin-ai.com)

```
Type:        A
Name:        portal
Content:     157.180.105.48
Proxy:       DNS only (⚪ Gray cloud) OR Proxied (🟠 Orange cloud)
TTL:         Auto
Priority:    -
```

**Purpose**: Main GRC Portal application  
**Backend**: ASP.NET Core Blazor application on port 5000/8080  
**Access**: `https://portal.shahin-ai.com`

---

### 4. App Subdomain (app.shahin-ai.com)

```
Type:        A
Name:        app
Content:     157.180.105.48
Proxy:       DNS only (⚪ Gray cloud) OR Proxied (🟠 Orange cloud)
TTL:         Auto
Priority:    -
```

**Purpose**: Alternative alias for GRC Portal  
**Backend**: ASP.NET Core Blazor application on port 5000/8080  
**Access**: `https://app.shahin-ai.com` → Redirects to `portal.shahin-ai.com`

---

### 5. Login Subdomain (login.shahin-ai.com)

```
Type:        A
Name:        login
Content:     157.180.105.48
Proxy:       DNS only (⚪ Gray cloud) OR Proxied (🟠 Orange cloud)
TTL:         Auto
Priority:    -
```

**Purpose**: Login redirect endpoint  
**Backend**: Nginx redirect to `portal.shahin-ai.com/login`  
**Access**: `https://login.shahin-ai.com` → Redirects to `portal.shahin-ai.com/login`

---

## 🔧 Optional DNS Records

### 6. API Subdomain (api.shahin-ai.com)

```
Type:        A
Name:        api
Content:     157.180.105.48
Proxy:       DNS only (⚪ Gray cloud)
TTL:         Auto
Priority:    -
```

**Purpose**: Dedicated API endpoints (if separating API from portal)  
**Backend**: ASP.NET Core API on port 5000/8080  
**Access**: `https://api.shahin-ai.com/api/*`  
**Note**: Can be handled by portal subdomain with `/api/*` path routing

---

### 7. Staging Environment (staging.shahin-ai.com)

```
Type:        A
Name:        staging
Content:     157.180.105.48
Proxy:       DNS only (⚪ Gray cloud)
TTL:         Auto
Priority:    -
```

**Purpose**: Staging/pre-production environment  
**Backend**: ASP.NET Core application on port 8080  
**Access**: `https://staging.shahin-ai.com`  
**Note**: Only needed if separate staging environment is deployed

---

### 8. Development Environment (dev.shahin-ai.com)

```
Type:        A
Name:        dev
Content:     157.180.105.48
Proxy:       DNS only (⚪ Gray cloud)
TTL:         Auto
Priority:    -
```

**Purpose**: Development/testing environment  
**Backend**: ASP.NET Core application on port 5137  
**Access**: `https://dev.shahin-ai.com`  
**Note**: Only needed if public dev environment is required

---

## 🔄 Cloudflare Tunnel Alternative (If Using Tunnel)

If using **Cloudflare Tunnel** instead of direct A records:

### Required CNAME Records

| Type | Name | Target | Proxy | Purpose |
|------|------|--------|-------|---------|
| CNAME | `@` | `<tunnel-id>.cfargotunnel.com` | ✅ Proxied | Root domain |
| CNAME | `www` | `<tunnel-id>.cfargotunnel.com` | ✅ Proxied | WWW subdomain |
| CNAME | `portal` | `<tunnel-id>.cfargotunnel.com` | ✅ Proxied | Portal |

**Where to get Tunnel ID:**
```bash
# On production server
cloudflared tunnel list
# Output: ID: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
```

**Example:**
```
Type:        CNAME
Name:        portal
Target:      12345678-1234-1234-1234-123456789abc.cfargotunnel.com
Proxy:       ✅ Proxied (🟠 Orange cloud)
TTL:         Auto
```

---

## 📊 DNS Configuration by Deployment Method

### Method 1: Direct A Records (Current Setup)

**Use when**: Direct server access, Nginx handles SSL termination

| Record | Type | Value | Proxy Status |
|--------|------|-------|--------------|
| `@` | A | `157.180.105.48` | ⚪ DNS only |
| `www` | A | `157.180.105.48` | ⚪ DNS only |
| `portal` | A | `157.180.105.48` | ⚪ DNS only |
| `app` | A | `157.180.105.48` | ⚪ DNS only |
| `login` | A | `157.180.105.48` | ⚪ DNS only |

**Pros**: 
- ✅ Direct connection
- ✅ Let's Encrypt SSL works
- ✅ Real client IPs in logs
- ✅ No proxy issues

**Cons**:
- ❌ Server IP exposed
- ❌ No DDoS protection
- ❌ No Cloudflare CDN

---

### Method 2: Cloudflare Tunnel (Recommended for Production)

**Use when**: Want Cloudflare protection, tunnel routing

| Record | Type | Target | Proxy Status |
|--------|------|--------|--------------|
| `@` | CNAME | `<tunnel-id>.cfargotunnel.com` | ✅ Proxied |
| `www` | CNAME | `<tunnel-id>.cfargotunnel.com` | ✅ Proxied |
| `portal` | CNAME | `<tunnel-id>.cfargotunnel.com` | ✅ Proxied |

**Pros**:
- ✅ DDoS protection
- ✅ Cloudflare CDN
- ✅ Server IP hidden
- ✅ Automatic SSL
- ✅ Better performance

**Cons**:
- ⚠️ Requires tunnel setup
- ⚠️ More complex configuration

---

### Method 3: Cloudflare Proxy (A Records + Proxy)

**Use when**: Want Cloudflare features but direct DNS

| Record | Type | Value | Proxy Status |
|--------|------|-------|--------------|
| `@` | A | `157.180.105.48` | ✅ Proxied |
| `www` | A | `157.180.105.48` | ✅ Proxied |
| `portal` | A | `157.180.105.48` | ✅ Proxied |

**Pros**:
- ✅ DDoS protection
- ✅ Cloudflare CDN
- ✅ Easy setup

**Cons**:
- ⚠️ SSL configuration needed
- ⚠️ May cause redirect loops
- ⚠️ Real IPs require Cloudflare headers

---

## 🔐 SSL/TLS Configuration

### Required SSL Certificates

All domains need valid SSL certificates:

1. ✅ `shahin-ai.com`
2. ✅ `www.shahin-ai.com`
3. ✅ `portal.shahin-ai.com`
4. ✅ `app.shahin-ai.com`
5. ✅ `login.shahin-ai.com`

**Certificate Provider Options:**

#### Option A: Let's Encrypt (Free, Recommended)
```bash
# On production server
sudo certbot --nginx \
  -d shahin-ai.com \
  -d www.shahin-ai.com \
  -d portal.shahin-ai.com \
  -d app.shahin-ai.com \
  -d login.shahin-ai.com \
  --non-interactive \
  --agree-tos \
  --email admin@shahin-ai.com \
  --redirect
```

#### Option B: Cloudflare SSL (If Using Proxy)
- Automatic SSL certificates
- Edge certificates issued by Cloudflare
- Set SSL mode to "Full (strict)"

---

## 📝 Step-by-Step: Create DNS Records in Cloudflare

### Step 1: Access Cloudflare Dashboard

1. Go to: https://dash.cloudflare.com
2. Login with your Cloudflare account
3. Select domain: **shahin-ai.com**
4. Navigate to: **DNS** → **Records**

### Step 2: Create A Records (Direct Access)

**For each record, click "Add record":**

#### Record 1: Root Domain
```
Type:        A
Name:        @
IPv4 address: 157.180.105.48
Proxy status: ⚪ DNS only (Gray cloud)
TTL:         Auto
```

#### Record 2: WWW
```
Type:        A
Name:        www
IPv4 address: 157.180.105.48
Proxy status: ⚪ DNS only (Gray cloud)
TTL:         Auto
```

#### Record 3: Portal
```
Type:        A
Name:        portal
IPv4 address: 157.180.105.48
Proxy status: ⚪ DNS only (Gray cloud)
TTL:         Auto
```

#### Record 4: App
```
Type:        A
Name:        app
IPv4 address: 157.180.105.48
Proxy status: ⚪ DNS only (Gray cloud)
TTL:         Auto
```

#### Record 5: Login
```
Type:        A
Name:        login
IPv4 address: 157.180.105.48
Proxy status: ⚪ DNS only (Gray cloud)
TTL:         Auto
```

### Step 3: Verify Records

After creating all records, verify:

1. **All 5 records exist** in DNS list
2. **All point to**: `157.180.105.48`
3. **All are**: DNS only (gray cloud) or Proxied (orange cloud)
4. **No duplicate records** exist

---

## ✅ Verification Checklist

### DNS Resolution Test

```bash
# Test from command line
nslookup shahin-ai.com
nslookup www.shahin-ai.com
nslookup portal.shahin-ai.com
nslookup app.shahin-ai.com
nslookup login.shahin-ai.com

# Expected output: All should resolve to 157.180.105.48
```

### Online DNS Checkers

1. **DNS Propagation Checker**
   - https://dnschecker.org
   - Enter each domain/subdomain
   - Verify global propagation

2. **DNS Lookup Tool**
   - https://mxtoolbox.com/DNSLookup.aspx
   - Check A record resolution

### Browser Test

Open in browser (after DNS propagation):

- ✅ `https://shahin-ai.com` → Should load landing page
- ✅ `https://www.shahin-ai.com` → Should load landing page
- ✅ `https://portal.shahin-ai.com` → Should load GRC portal
- ✅ `https://app.shahin-ai.com` → Should redirect to portal
- ✅ `https://login.shahin-ai.com` → Should redirect to login

---

## 🚨 Common Issues & Solutions

### Issue 1: DNS Not Resolving

**Symptoms**: Domain doesn't resolve, timeout errors

**Check**:
```bash
# Verify DNS records exist
dig shahin-ai.com
dig portal.shahin-ai.com

# Check Cloudflare dashboard
# DNS → Records → Verify all records exist
```

**Fix**:
1. Verify IP address is correct: `157.180.105.48`
2. Check record type is `A` (not CNAME)
3. Wait 5-15 minutes for DNS propagation
4. Clear DNS cache: `ipconfig /flushdns` (Windows) or `sudo systemd-resolve --flush-caches` (Linux)

---

### Issue 2: Wrong IP Address

**Symptoms**: Domain resolves but shows wrong content or connection refused

**Check**:
```bash
# Check what IP DNS resolves to
nslookup portal.shahin-ai.com

# Check actual server IP
curl ifconfig.me
```

**Fix**:
1. Update all DNS records to correct IP: `157.180.105.48`
2. Wait for DNS propagation (15-30 minutes)
3. Verify server firewall allows connections on ports 80/443

---

### Issue 3: SSL Certificate Errors

**Symptoms**: Browser shows "Not Secure" or certificate warnings

**Check**:
```bash
# Test SSL certificate
openssl s_client -connect portal.shahin-ai.com:443 -servername portal.shahin-ai.com

# Check certificate expiry
echo | openssl s_client -connect portal.shahin-ai.com:443 2>/dev/null | openssl x509 -noout -dates
```

**Fix**:
1. Ensure DNS points to correct server
2. Run certbot to obtain/renew certificates
3. Verify Nginx SSL configuration
4. Check certificate includes all domains

---

### Issue 4: Proxy Status Issues

**Symptoms**: 502 Bad Gateway, redirect loops, SSL errors

**Check**:
- Cloudflare dashboard → DNS → Records → Proxy status

**Fix**:

**If using Let's Encrypt SSL:**
- Set all records to: ⚪ **DNS only** (Gray cloud)

**If using Cloudflare SSL:**
- Set all records to: 🟠 **Proxied** (Orange cloud)
- Configure SSL mode: **Full (strict)**
- Enable "Always Use HTTPS"

---

## 📊 DNS Record Status Tracking

### Current Status (Update as needed)

| Record | Type | Value | Proxy | Status | Last Verified |
|--------|------|-------|-------|--------|--------------|
| `@` | A | `157.180.105.48` | ⚪ DNS only | ✅ Active | 2026-01-12 |
| `www` | A | `157.180.105.48` | ⚪ DNS only | ✅ Active | 2026-01-12 |
| `portal` | A | `157.180.105.48` | ⚪ DNS only | ✅ Active | 2026-01-12 |
| `app` | A | `157.180.105.48` | ⚪ DNS only | ✅ Active | 2026-01-12 |
| `login` | A | `157.180.105.48` | ⚪ DNS only | ✅ Active | 2026-01-12 |
| `api` | A | `157.180.105.48` | ⚪ DNS only | ⏳ Pending | - |
| `staging` | A | `157.180.105.48` | ⚪ DNS only | ⏳ Pending | - |
| `dev` | A | `157.180.105.48` | ⚪ DNS only | ⏳ Pending | - |

---

## 🔄 DNS Propagation Timeline

| Time | Status | Action |
|------|--------|--------|
| **0-5 min** | Records created | Cloudflare dashboard updated |
| **5-15 min** | Cloudflare edge updated | Records visible on Cloudflare network |
| **15-30 min** | ISP propagation | Most users can access |
| **30-60 min** | Global propagation | All users worldwide can access |
| **Up to 48 hours** | Full propagation | Some DNS caches may take longer |

**Note**: You can test immediately from Cloudflare's network, but global access may take up to 1 hour.

---

## 📞 Quick Reference

### Cloudflare Dashboard
- **URL**: https://dash.cloudflare.com
- **Domain**: shahin-ai.com
- **Section**: DNS → Records

### Server IP
- **Production**: `157.180.105.48`
- **Ports**: 80 (HTTP), 443 (HTTPS)

### Required Records (Minimum)
1. `@` (root) → `157.180.105.48`
2. `www` → `157.180.105.48`
3. `portal` → `157.180.105.48`
4. `app` → `157.180.105.48`
5. `login` → `157.180.105.48`

---

## 🎯 Next Steps

1. ✅ **Verify all 5 required DNS records exist**
2. ✅ **Confirm all point to**: `157.180.105.48`
3. ✅ **Set proxy status** (DNS only or Proxied based on your setup)
4. ✅ **Wait for DNS propagation** (15-30 minutes)
5. ✅ **Test access** via browser
6. ✅ **Verify SSL certificates** are valid
7. ✅ **Update status table** above with verification date

---

**Document Version**: 1.0  
**Last Updated**: January 12, 2026  
**Maintained By**: DevOps Team  
**Status**: ✅ Production Ready
