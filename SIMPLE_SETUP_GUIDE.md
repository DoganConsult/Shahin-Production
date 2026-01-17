# Simple Setup Guide - Get Your Site Live NOW

You only need **ONE Cloudflare Tunnel**. Here's the simplest path:

## Option 1: Run Marketing Site Directly on Windows (EASIEST)

### Step 1: Start the Marketing Site
```powershell
cd C:\Shahin-ai\landing-page
npm install
npm run dev
```

Wait for "Ready" message. Keep this terminal open.

### Step 2: Get Your Cloudflare Token
1. Go to: https://one.dash.cloudflare.com/
2. Networks → Tunnels → Click your tunnel
3. Configure → Copy the token (long string after `--token`)

### Step 3: Start Cloudflared
```powershell
# In a NEW PowerShell window
docker rm -f cloudflared
docker run -d --name cloudflared --restart unless-stopped cloudflare/cloudflared:latest tunnel --no-autoupdate run --token "YOUR_TOKEN_HERE"
```

### Step 4: Configure Public Hostname
In Cloudflare Dashboard:
- Subdomain: `www`
- Domain: `shahin-ai.com`
- Service: `HTTP`
- URL: `host.docker.internal:3000`

### Step 5: Test
Visit: https://www.shahin-ai.com

---

## Option 2: Run Everything in Docker (More Complex)

If you want the marketing site in Docker too, I can set that up, but Option 1 is faster and simpler for now.

---

## Quick Commands

**Check if site is running:**
```powershell
curl http://127.0.0.1:3000
```

**Check cloudflared status:**
```powershell
docker ps --filter "name=cloudflared"
docker logs --tail 20 cloudflared
```

**Restart cloudflared:**
```powershell
docker restart cloudflared
```

---

## You Only Need ONE Tunnel

The Cloudflare tunnel you already have can route traffic to:
- Marketing site on Windows (port 3000)
- OR Marketing site in Docker (port 3000)
- You don't need separate tunnels

The tunnel just forwards HTTPS traffic from www.shahin-ai.com to wherever your app is running (Windows or Docker).
