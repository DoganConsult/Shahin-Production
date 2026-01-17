# Cloudflare Tunnel Fix Guide

## Current Issues
1. ❌ **Invalid tunnel token** - Container keeps restarting
2. ❌ **No service on port 3000** - Marketing site not running

## Solution Steps

### Step 1: Start the Marketing Site on Port 3000

Your Next.js marketing site is in the `landing-page` directory. Start it first:

```powershell
# Navigate to the landing-page directory and start the dev server
cd C:\Shahin-ai\landing-page
npm install
npm run dev
```

**Important:** Keep this terminal window open. The dev server must stay running.

By default, Next.js dev server runs on port 3000. Verify it's working:

```powershell
# In a NEW PowerShell window
curl http://127.0.0.1:3000
netstat -ano | findstr :3000
```

You should see:
- `curl` returns HTML content
- `netstat` shows something listening on port 3000

---

### Step 2: Get a Valid Cloudflare Tunnel Token

1. Go to [Cloudflare Zero Trust Dashboard](https://one.dash.cloudflare.com/)
2. Navigate to: **Networks** → **Tunnels**
3. Click on your tunnel name
4. Click **Configure**
5. Look for the **Install connector** or **Docker** section
6. Copy the FULL token from the command (it looks like `eyJ...` and is very long)

**Security Note:** The token you posted earlier is now compromised. You MUST rotate it:
- In the tunnel settings, find **Regenerate token** or **Rotate credentials**
- Use the NEW token in the next step

---

### Step 3: Start Cloudflared with Valid Token

```powershell
# Remove the broken container
docker rm -f cloudflared

# Start with the REAL token (replace the token below)
docker run -d --name cloudflared --restart unless-stopped cloudflare/cloudflared:latest tunnel --no-autoupdate run --token "PASTE_YOUR_REAL_TOKEN_HERE"
```

**Critical:** Replace `PASTE_YOUR_REAL_TOKEN_HERE` with the actual token from Step 2.

Verify it's running:

```powershell
docker ps --filter "name=cloudflared"
docker logs --tail 30 cloudflared
```

You should see:
- Status: **Up** (not Restarting)
- Logs show: `Registered tunnel connection` (no "token is not valid")

---

### Step 4: Configure Cloudflare Public Hostname

In Cloudflare Zero Trust → Tunnels → Your Tunnel → **Public Hostnames**:

**Add/Edit the hostname:**
- **Subdomain:** `www`
- **Domain:** `shahin-ai.com`
- **Service Type:** `HTTP`
- **URL:** `host.docker.internal:3000`

**Important for Windows Docker Desktop:**
- ✅ Use `host.docker.internal:3000`
- ❌ Do NOT use `127.0.0.1:3000`
- ❌ Do NOT use `localhost:3000`
- ❌ Do NOT use `--network host` flag

---

### Step 5: Test the Tunnel

1. Wait 30-60 seconds for DNS propagation
2. Visit: https://www.shahin-ai.com
3. You should see your Next.js marketing site

If it doesn't work, check:

```powershell
# Check cloudflared logs for errors
docker logs --tail 50 cloudflared

# Verify marketing site is still running
curl http://127.0.0.1:3000
netstat -ano | findstr :3000
```

---

## Common Issues & Solutions

### Issue: "connect: connection refused" in cloudflared logs

**Cause:** Marketing site is not running or not reachable

**Fix:**
1. Ensure `npm run dev` is still running in the landing-page directory
2. Verify with: `curl http://127.0.0.1:3000`
3. Check Cloudflare hostname is set to `host.docker.internal:3000`

### Issue: Marketing site runs in WSL instead of Windows

If you started the site in WSL, Docker Desktop cannot reach `host.docker.internal`.

**Fix:**
1. Get WSL IP: `wsl hostname -I` (e.g., `172.24.x.x`)
2. In WSL, start the site with: `npm run dev -- -H 0.0.0.0 -p 3000`
3. Update Cloudflare hostname to: `http://172.24.x.x:3000`

### Issue: Port 3000 already in use

**Fix:**
```powershell
# Find what's using port 3000
netstat -ano | findstr :3000

# Kill the process (replace PID with actual process ID)
taskkill /PID <PID> /F

# Or use a different port
cd C:\Shahin-ai\landing-page
npm run dev -- -p 3001
```

Then update Cloudflare hostname to use the new port.

---

## Quick Verification Commands

Run these to verify everything is working:

```powershell
# 1. Check marketing site is running
curl http://127.0.0.1:3000

# 2. Check cloudflared container status
docker ps --filter "name=cloudflared"

# 3. Check cloudflared logs (should show "Registered tunnel connection")
docker logs --tail 20 cloudflared

# 4. Check port 3000 is listening
netstat -ano | findstr :3000
```

---

## Production Deployment

For production (not dev server):

```powershell
cd C:\Shahin-ai\landing-page
npm run build
npm run start
```

This runs the optimized production build on port 3000.

---

## Next Steps After Fix

1. ✅ Verify https://www.shahin-ai.com loads your site
2. ✅ Test all pages and functionality
3. ✅ Set up SSL/TLS settings in Cloudflare (should be automatic)
4. ✅ Configure caching rules in Cloudflare if needed
5. ✅ Monitor cloudflared logs for any errors

---

## Need Help?

If you encounter issues, provide these outputs:

```powershell
docker logs --tail 30 cloudflared
curl http://127.0.0.1:3000
netstat -ano | findstr :3000
docker ps --filter "name=cloudflared"
