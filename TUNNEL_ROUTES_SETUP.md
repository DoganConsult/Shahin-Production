# Cloudflare Tunnel Routes Setup

**Date**: January 15, 2026  
**Status**: ✅ DNS Records Configured | ✅ Tunnel Connected | ⏳ Routes Need Configuration

---

## ✅ Completed Steps

1. ✅ **DNS Records Created**:
   - `portal.shahin-ai.com` → Tunnel (Proxied)
   - `shahin-ai.com` → Tunnel (Proxied)
   - `www.shahin-ai.com` → Tunnel (Proxied)

2. ✅ **Tunnel Connected**:
   - Tunnel ID: `c797bf6a-7ca8-447a-a26a-4b72ba91bc5e`
   - Connection registered: ✅ Active
   - Container: Running

3. ✅ **Local Origins Running**:
   - Portal: Port 5000 (LISTENING)
   - Marketing: Port 3000 (LISTENING)
   - Ready to accept tunnel connections

---

## 🎯 Next Step: Configure Tunnel Routes

Now you need to tell Cloudflare tunnel **which hostnames route to which services**.

### Step-by-Step Instructions:

1. **Go to Cloudflare Zero Trust Dashboard**
   - Navigate to: https://one.dash.cloudflare.com/
   - Sign in if needed

2. **Navigate to Your Tunnel**
   - Click: **Networks** (left sidebar)
   - Click: **Connectors** (or "Tunnels")
   - Find your tunnel: **"Shahin-ai"** or look for tunnel ID starting with `c797bf6a...`
   - Click on the tunnel name to open details

3. **Go to Hostname Routes Tab**
   - Look for tabs: **"Public Hostnames"**, **"Hostname routes"**, or **"Published application routes"**
   - Click on that tab

4. **Add Hostname Routes**

   Click **"+ Add a public hostname"** or **"+ Add hostname route"** button

   **Route 1: Main Domain (Marketing)**
   - **Subdomain**: (leave blank for root domain)
   - **Domain**: `shahin-ai.com`
   - **Service Type**: `HTTP`
   - **Service URL**: `http://localhost:3000`
   - **Path**: (leave blank for all paths)
   - Click: **Save**

   **Route 2: Portal**
   - **Subdomain**: `portal`
   - **Domain**: `shahin-ai.com`
   - **Service Type**: `HTTP`
   - **Service URL**: `http://localhost:5000`
   - Click: **Save**

   **Route 3: WWW (Marketing)**
   - **Subdomain**: `www`
   - **Domain**: `shahin-ai.com`
   - **Service Type**: `HTTP`
   - **Service URL**: `http://localhost:3000`
   - Click: **Save**

5. **Verify Routes Created**
   - You should see all 3 routes listed
   - Status should show as "Active" or "Healthy"

---

## 🔍 Alternative: If You Don't See "Hostname Routes" Tab

Some Cloudflare tunnel configurations use different terminology:

### Look for these tabs:
- **"Public Hostnames"**
- **"Hostname routes"**
- **"Published application routes"**
- **"Routes"**
- **"Applications"**

### Or use "Configure" button:
- Click **"Configure"** button on the tunnel
- Look for **"Public Hostnames"** section
- Click **"+ Add a public hostname"**

---

## ✅ After Routes Are Configured

1. **Wait 1-2 minutes** for DNS and route propagation

2. **Test Access**:
   - Visit: `https://portal.shahin-ai.com`
   - Visit: `https://www.shahin-ai.com`
   - Visit: `https://shahin-ai.com`

3. **Expected Result**:
   - Should see your application(s) via the tunnel (portal on `localhost:5000`, marketing on `localhost:3000`)
   - HTTPS should work automatically (Cloudflare provides SSL)
   - No more "invalid hostname" errors

---

## 🐛 Troubleshooting

### If routes don't appear:
- Check tunnel status in dashboard (should be "UP" or "HEALTHY")
- Verify DNS records are Proxied (orange cloud)
- Wait a few minutes for propagation

### If you get 502/503 errors:
- Verify application is running: `Get-NetTCPConnection -LocalPort 5000`
- Check tunnel logs: `docker logs -f cloudflare-tunnel-shahin-new`
- Verify service URL is correct: `http://localhost:5000`

### If DNS doesn't resolve:
- Wait 2-5 minutes for DNS propagation
- Check DNS records are Proxied (not DNS only)
- Verify CNAME target matches tunnel ID

---

## 📋 Quick Checklist

- [x] DNS CNAME records created
- [x] DNS records set to Proxied
- [x] Tunnel container running
- [x] Tunnel connected to Cloudflare
- [x] Application running on port 5000
- [ ] Tunnel routes configured in Zero Trust dashboard
- [ ] Routes show as Active/Healthy
- [ ] Can access application via public hostname

---

**Next Action**: Configure tunnel routes in Cloudflare Zero Trust dashboard (see steps above)
