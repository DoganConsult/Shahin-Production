# DNS Setup for Cloudflare Tunnel

**Date**: January 15, 2026  
**Tunnel ID**: `c797bf6a-7ca8-447a-a26a-4b72ba91bc5e`  
**Status**: DNS records cleared - ready to configure

---

## ✅ Current Status

- **DNS Records**: All cleared (no conflicts!)
- **Tunnel**: Running and ready
- **Application**: Running on `localhost:5000`
- **Next Step**: Configure DNS records for tunnel routes

---

## 🎯 Recommended Approach: Let Tunnel Auto-Create CNAME Records

**This is the easiest method** - configure tunnel routes first, then Cloudflare automatically creates the DNS records.

### Step 1: Configure Tunnel Routes

1. **Go to Cloudflare Zero Trust Dashboard**
   - Navigate to: https://one.dash.cloudflare.com/
   - Go to: **Networks** → **Connectors**
   - Find your tunnel: **"Shahin-ai"** (or tunnel ID: `c797bf6a-7ca8-447a-a26a-4b72ba91bc5e`)

2. **Click on the Tunnel** to open details

3. **Go to "Public Hostnames" or "Hostname routes" tab**

4. **Click "+ Add a public hostname" or "+ Add hostname route"**

5. **Add Routes** (one for each subdomain):

   **Route 1: Main Domain**
   - **Subdomain**: (leave blank for root domain)
   - **Domain**: `shahin-ai.com`
   - **Service Type**: `HTTP`
   - **Service URL**: `http://localhost:5000`
   - **Path**: (leave blank for all paths)
   - Click: **Save**

   **Route 2: Portal**
   - **Subdomain**: `portal`
   - **Domain**: `shahin-ai.com`
   - **Service Type**: `HTTP`
   - **Service URL**: `http://localhost:5000`
   - Click: **Save**

   **Route 3: WWW**
   - **Subdomain**: `www`
   - **Domain**: `shahin-ai.com`
   - **Service Type**: `HTTP`
   - **Service URL**: `http://localhost:5000`
   - Click: **Save**

   **Route 4: API** (if needed)
   - **Subdomain**: `api`
   - **Domain**: `shahin-ai.com`
   - **Service Type**: `HTTP`
   - **Service URL**: `http://localhost:5000`
   - Click: **Save**

6. **Cloudflare Auto-Creates DNS Records**
   - After saving each route, Cloudflare automatically creates CNAME records
   - Records point to: `{tunnel-id}.cfargotunnel.com`
   - You'll see them appear in the DNS dashboard automatically

---

## 🔧 Alternative: Manual CNAME Records

If you prefer to create DNS records manually:

### Step 1: Get Tunnel CNAME Target

The CNAME target format is: `{tunnel-id}.cfargotunnel.com`

For your tunnel: `c797bf6a-7ca8-447a-a26a-4b72ba91bc5e.cfargotunnel.com`

### Step 2: Create CNAME Records in DNS Dashboard

1. **Go to Cloudflare DNS Dashboard**
   - Navigate to: https://dash.cloudflare.com/
   - Select domain: `shahin-ai.com`
   - Go to: **DNS** → **Records**

2. **Click "+ Add record"**

3. **Create CNAME Records**:

   **Record 1: Root Domain**
   - **Type**: `CNAME`
   - **Name**: `@` (or `shahin-ai.com`)
   - **Target**: `c797bf6a-7ca8-447a-a26a-4b72ba91bc5e.cfargotunnel.com`
   - **Proxy status**: **Proxied** (orange cloud) ✅
   - **TTL**: Auto
   - Click: **Save**

   **Record 2: Portal**
   - **Type**: `CNAME`
   - **Name**: `portal`
   - **Target**: `c797bf6a-7ca8-447a-a26a-4b72ba91bc5e.cfargotunnel.com`
   - **Proxy status**: **Proxied** (orange cloud) ✅
   - Click: **Save**

   **Record 3: WWW**
   - **Type**: `CNAME`
   - **Name**: `www`
   - **Target**: `c797bf6a-7ca8-447a-a26a-4b72ba91bc5e.cfargotunnel.com`
   - **Proxy status**: **Proxied** (orange cloud) ✅
   - Click: **Save**

   **Record 4: API** (if needed)
   - **Type**: `CNAME`
   - **Name**: `api`
   - **Target**: `c797bf6a-7ca8-447a-a26a-4b72ba91bc5e.cfargotunnel.com`
   - **Proxy status**: **Proxied** (orange cloud) ✅
   - Click: **Save**

4. **Then Configure Tunnel Routes** (same as Step 1 above)

---

## 📋 Quick Setup Checklist

- [ ] Tunnel is running (✅ Already done)
- [ ] Application is running on port 5000 (✅ Already done)
- [ ] DNS records cleared (✅ Already done)
- [ ] Configure tunnel routes in Zero Trust dashboard
- [ ] OR manually create CNAME records
- [ ] Verify DNS records appear in DNS dashboard
- [ ] Test access via public hostname (e.g., `https://portal.shahin-ai.com`)

---

## 🔍 Verify DNS Records

After setup, check DNS dashboard:

1. Go to: **DNS** → **Records**
2. You should see CNAME records like:
   - `shahin-ai.com` → `c797bf6a-7ca8-447a-a26a-4b72ba91bc5e.cfargotunnel.com`
   - `portal.shahin-ai.com` → `c797bf6a-7ca8-447a-a26a-4b72ba91bc5e.cfargotunnel.com`
   - `www.shahin-ai.com` → `c797bf6a-7ca8-447a-a26a-4b72ba91bc5e.cfargotunnel.com`

---

## ⚠️ Important Notes

1. **Proxy Status**: Must be **Proxied** (orange cloud) for tunnel to work
2. **DNS Propagation**: Changes take 1-2 minutes to propagate
3. **Tunnel Connection**: Tunnel must be connected (check tunnel status in dashboard)
4. **Application Running**: Ensure app is running on `localhost:5000`

---

## 🚀 Quick Start (Recommended)

**Fastest way to get started:**

1. **Go to Zero Trust Dashboard**: https://one.dash.cloudflare.com/
2. **Networks** → **Connectors** → **Your Tunnel**
3. **Add hostname route**: `portal.shahin-ai.com` → `http://localhost:5000`
4. **Save** - Cloudflare auto-creates DNS record
5. **Wait 1-2 minutes** for DNS propagation
6. **Test**: Visit `https://portal.shahin-ai.com`

---

**Need Help?** See `CLOUDFLARE_TUNNEL_ROUTES_GUIDE.md` for detailed route configuration.
