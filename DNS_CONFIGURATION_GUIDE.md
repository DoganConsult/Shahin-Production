# 🌐 DNS Configuration Guide - Shahin AI

**Quick Reference for Cloudflare DNS Setup**

---

## 📋 What You Need

Before configuring DNS, you need:

1. **Cloudflare Account** with `shahin-ai.com` domain added
2. **Tunnel ID** from your Linux server (format: `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`)
3. **Access to Cloudflare Dashboard**

---

## 🔧 Getting Your Tunnel ID

**On your Linux production server, run:**

```bash
# After creating the tunnel (Phase 4, Step 4.3)
cloudflared tunnel list

# Or get info about specific tunnel
cloudflared tunnel info shahin-production
```

**Output will show:**
```
ID: 12345678-1234-1234-1234-123456789abc
Name: shahin-production
Created: 2026-01-16
Connections: 4
```

**Copy the ID** - you'll use it as `<tunnel-id>.cfargotunnel.com`

---

## 🌍 DNS Records to Create

### Step 1: Login to Cloudflare

1. Go to https://dash.cloudflare.com
2. Select domain: **shahin-ai.com**
3. Click **DNS** → **Records**

### Step 2: Add CNAME Records

**Click "Add record" and create these 3 records:**

#### Record 1: Marketing Site (www)
```
Type:    CNAME
Name:    www
Target:  <tunnel-id>.cfargotunnel.com
Proxy:   ✅ Proxied (Orange cloud)
TTL:     Auto
```

#### Record 2: Marketing Site (root domain)
```
Type:    CNAME
Name:    @
Target:  <tunnel-id>.cfargotunnel.com
Proxy:   ✅ Proxied (Orange cloud)
TTL:     Auto
```

#### Record 3: GRC Portal
```
Type:    CNAME
Name:    portal
Target:  <tunnel-id>.cfargotunnel.com
Proxy:   ✅ Proxied (Orange cloud)
TTL:     Auto
```

### Example (Replace with YOUR tunnel ID):

If your tunnel ID is `12345678-1234-1234-1234-123456789abc`, then:

| Type | Name | Target | Proxy |
|------|------|--------|-------|
| CNAME | www | `12345678-1234-1234-1234-123456789abc.cfargotunnel.com` | ✅ Proxied |
| CNAME | @ | `12345678-1234-1234-1234-123456789abc.cfargotunnel.com` | ✅ Proxied |
| CNAME | portal | `12345678-1234-1234-1234-123456789abc.cfargotunnel.com` | ✅ Proxied |

---

## 🔐 SSL/TLS Configuration

### Step 3: Configure SSL Settings

1. Go to **SSL/TLS** → **Overview**
2. Set encryption mode to: **Full (strict)**

### Step 4: Enable Security Features

Go to **SSL/TLS** → **Edge Certificates** and enable:

- ✅ **Always Use HTTPS**
- ✅ **Automatic HTTPS Rewrites**
- ✅ **Minimum TLS Version:** 1.2 or higher
- ✅ **TLS 1.3:** Enabled

---

## 🚇 Cloudflare Tunnel Routes

### Step 5: Configure Public Hostnames

1. Go to **Zero Trust** → **Access** → **Tunnels**
2. Find tunnel: **shahin-production**
3. Click **Configure**
4. Go to **Public Hostnames** tab
5. Click **Add a public hostname**

**Add these 3 routes:**

#### Route 1: Marketing Site (www)
```
Public hostname: www.shahin-ai.com
Service:         HTTP
URL:             localhost:3000
```

#### Route 2: Marketing Site (root)
```
Public hostname: shahin-ai.com
Service:         HTTP
URL:             localhost:3000
```

#### Route 3: GRC Portal
```
Public hostname: portal.shahin-ai.com
Service:         HTTP
URL:             localhost:5000
```

**Click "Save hostname" for each**

---

## ✅ Verification Steps

### Step 6: Test DNS Resolution

**Wait 5-10 minutes for DNS propagation, then test:**

```bash
# From any computer
nslookup www.shahin-ai.com
nslookup shahin-ai.com
nslookup portal.shahin-ai.com
```

**Expected:** All should resolve to Cloudflare IP addresses (104.x.x.x or 172.x.x.x range)

### Step 7: Test HTTPS Access

**Open in browser:**
- https://www.shahin-ai.com
- https://shahin-ai.com
- https://portal.shahin-ai.com

**Expected:**
- ✅ Green padlock (SSL valid)
- ✅ No certificate warnings
- ✅ Pages load correctly

### Step 8: Test from Server

**On your Linux server:**

```bash
# Test tunnel connectivity
curl -I https://www.shahin-ai.com
curl -I https://portal.shahin-ai.com

# Check tunnel status
sudo systemctl status cloudflared

# View tunnel logs
sudo journalctl -u cloudflared -n 50
```

---

## 🔄 DNS Propagation Timeline

| Time | Status |
|------|--------|
| 0-5 min | DNS records created in Cloudflare |
| 5-15 min | Cloudflare edge servers updated |
| 15-30 min | Most ISPs see new records |
| 30-60 min | Global propagation complete |

**Note:** You can test immediately from Cloudflare's network, but some users may need to wait for full propagation.

---

## 🚨 Troubleshooting

### Issue: DNS Not Resolving

**Check:**
1. Tunnel ID is correct in CNAME records
2. Proxy status is "Proxied" (orange cloud)
3. Tunnel service is running on server: `sudo systemctl status cloudflared`

**Fix:**
```bash
# On server
sudo systemctl restart cloudflared
sudo journalctl -u cloudflared -f
```

### Issue: SSL Certificate Error

**Check:**
1. SSL/TLS mode is "Full (strict)"
2. "Always Use HTTPS" is enabled
3. Edge certificates are active

**Fix:**
1. Go to SSL/TLS → Edge Certificates
2. Click "Order SSL Certificate" if needed
3. Wait 5 minutes for certificate issuance

### Issue: 502 Bad Gateway

**Possible causes:**
1. Application not running on server
2. Wrong port in tunnel configuration
3. Firewall blocking connections

**Fix:**
```bash
# On server - check services
docker ps

# Restart services if needed
docker-compose -f docker-compose.production.yml restart

# Check tunnel routes
cloudflared tunnel route dns shahin-production www.shahin-ai.com
```

### Issue: Tunnel Not Connecting

**Check tunnel status:**
```bash
# On server
sudo systemctl status cloudflared
sudo journalctl -u cloudflared -n 100

# Test tunnel
cloudflared tunnel info shahin-production
```

**Restart tunnel:**
```bash
sudo systemctl restart cloudflared
```

---

## 📊 DNS Status Checker

**Use these online tools to verify DNS:**

1. **DNS Propagation Checker**
   - https://dnschecker.org
   - Enter: `www.shahin-ai.com`
   - Check global propagation

2. **SSL Certificate Checker**
   - https://www.ssllabs.com/ssltest/
   - Enter: `https://portal.shahin-ai.com`
   - Verify SSL configuration

3. **Cloudflare Analytics**
   - Dashboard → Analytics → Traffic
   - Monitor incoming requests

---

## 📝 Configuration Summary

**After completing all steps, you should have:**

✅ **3 DNS Records Created**
- www.shahin-ai.com → Tunnel
- shahin-ai.com → Tunnel
- portal.shahin-ai.com → Tunnel

✅ **3 Tunnel Routes Configured**
- www.shahin-ai.com → localhost:3000
- shahin-ai.com → localhost:3000
- portal.shahin-ai.com → localhost:5000

✅ **SSL/TLS Enabled**
- Mode: Full (strict)
- Always Use HTTPS: On
- Auto HTTPS Rewrites: On

✅ **Services Running**
- Cloudflare tunnel service active
- Docker containers running
- Applications responding

---

## 🎯 Quick Checklist

**Before you start:**
- [ ] Cloudflare account ready
- [ ] Domain added to Cloudflare
- [ ] Tunnel created on server
- [ ] Tunnel ID copied

**DNS Configuration:**
- [ ] CNAME for www created
- [ ] CNAME for @ (root) created
- [ ] CNAME for portal created
- [ ] All records proxied (orange cloud)

**Tunnel Configuration:**
- [ ] Public hostname for www added
- [ ] Public hostname for root added
- [ ] Public hostname for portal added
- [ ] All routes pointing to correct ports

**SSL Configuration:**
- [ ] SSL mode set to "Full (strict)"
- [ ] Always Use HTTPS enabled
- [ ] Edge certificates active

**Verification:**
- [ ] DNS resolves correctly
- [ ] HTTPS works (green padlock)
- [ ] Marketing site loads
- [ ] Portal loads
- [ ] No certificate errors

---

## 📞 Need Help?

**If DNS is not working after 1 hour:**

1. **Check Cloudflare Status**
   - https://www.cloudflarestatus.com

2. **Verify Tunnel on Server**
   ```bash
   sudo systemctl status cloudflared
   cloudflared tunnel info shahin-production
   ```

3. **Check Application Logs**
   ```bash
   docker-compose -f docker-compose.production.yml logs -f
   ```

4. **Contact Cloudflare Support**
   - Dashboard → Support → Contact Support
   - Include tunnel ID and domain name

---

**Last Updated:** January 16, 2026  
**Version:** 1.0  
**Status:** Ready for Configuration

---

## 🎉 Success Indicators

**When everything is working, you'll see:**

✅ **Browser Address Bar:**
```
https://www.shahin-ai.com 🔒
https://portal.shahin-ai.com 🔒
```

✅ **DNS Lookup:**
```bash
$ nslookup www.shahin-ai.com
Server:  cloudflare.com
Address:  104.21.x.x
```

✅ **SSL Test:**
```
Grade: A+
Certificate: Valid
TLS Version: 1.3
```

✅ **Tunnel Status:**
```bash
$ sudo systemctl status cloudflared
● cloudflared.service - Cloudflare Tunnel
   Active: active (running)
   Connections: 4
```

**You're all set! 🚀**
