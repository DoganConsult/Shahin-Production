# Complete Setup Instructions - Marketing Site + Portal

## Problem Identified ✅

**Port 5000 is occupied by Docker Desktop** - Your GRC Portal was never running!

The 400 error you saw was Docker's API responding, not your portal.

## Solution: Run Portal on Port 5001

### Step 1: Start Marketing Site (Already Running ✅)

Terminal 1:
```powershell
cd C:\Shahin-ai\landing-page
npm run dev
```

Status: ✅ Running on port 3000
Public URL: ✅ https://www.shahin-ai.com

### Step 2: Start GRC Portal on Port 5001

Terminal 2:
```powershell
.\start-portal.ps1
```

OR manually:
```powershell
cd C:\Shahin-ai\Shahin-Jan-2026\src\GrcMvc
dotnet run --urls "http://0.0.0.0:5001"
```

Wait for: `Now listening on: http://0.0.0.0:5001`

### Step 3: Add Portal to Cloudflare Tunnel

Go to: https://one.dash.cloudflare.com/
Navigate to: **Networks** → **Tunnels** → **Your Tunnel** → **Public Hostnames**

Click **"Add a public hostname"**:

**Configuration:**
- **Subdomain:** `app` (or `portal`)
- **Domain:** `shahin-ai.com`
- **Path:** (leave empty)
- **Service Type:** `HTTP`
- **URL:** `host.docker.internal:5001` ⚠️ **Use 5001, not 5000!**

Click **Save**

### Step 4: Update Marketing Site Login Button

Edit `C:\Shahin-ai\landing-page\.env.local` (create if doesn't exist):

```
NEXT_PUBLIC_API_URL=https://app.shahin-ai.com
```

Restart marketing site (Ctrl+C in Terminal 1, then `npm run dev` again)

### Step 5: Fix Portal for Cloudflare Proxy

Your portal needs to trust Cloudflare's forwarded headers.

Check if `C:\Shahin-ai\Shahin-Jan-2026\src\GrcMvc\Program.cs` has this configuration:

```csharp
using Microsoft.AspNetCore.HttpOverrides;

// Add this in ConfigureServices or builder.Services section:
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = 
        ForwardedHeaders.XForwardedFor | 
        ForwardedHeaders.XForwardedProto |
        ForwardedHeaders.XForwardedHost;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

// Add this BEFORE app.UseAuthentication() and app.UseAuthorization():
app.UseForwardedHeaders();
```

Also in `appsettings.json`, ensure:
```json
{
  "AllowedHosts": "*"
}
```

Restart the portal after making these changes.

## Final Architecture

```
User Browser
    ↓
https://www.shahin-ai.com (Marketing Site)
    ↓
Cloudflare Tunnel
    ↓
localhost:3000 (Next.js)

User clicks "Login"
    ↓
https://app.shahin-ai.com (Portal)
    ↓
Cloudflare Tunnel
    ↓
localhost:5001 (ASP.NET Core GRC Portal)
```

## Verification Steps

1. **Marketing Site:**
   ```powershell
   curl http://127.0.0.1:3000
   ```
   Should return HTML

2. **Portal:**
   ```powershell
   curl http://127.0.0.1:5001
   ```
   Should return HTML (not Docker API error)

3. **Public URLs:**
   - https://www.shahin-ai.com → Marketing site
   - https://app.shahin-ai.com → Portal login

## Troubleshooting

### Portal won't start on 5001
```powershell
# Check what's using 5001
netstat -ano | findstr :5001

# If something is there, use a different port (e.g., 5002)
dotnet run --urls "http://0.0.0.0:5002"
# Then update Cloudflare tunnel to use 5002
```

### Still getting 400 errors
1. Check portal logs in the terminal where you ran `dotnet run`
2. Verify `UseForwardedHeaders()` is configured
3. Check `AllowedHosts` is set to `"*"`
4. Verify Cloudflare SSL/TLS mode is "Full" or "Full (Strict)"

### Login button still points to localhost
1. Verify `.env.local` file exists in `landing-page` directory
2. Restart the marketing site after creating/editing `.env.local`
3. Check browser dev tools → Network tab to see the actual redirect URL

## Keep Running

**Terminal 1:** Marketing site (`npm run dev`)
**Terminal 2:** Portal (`dotnet run --urls "http://0.0.0.0:5001"`)
**Docker:** Cloudflared container

## Management Commands

```powershell
# Check all services
docker ps --filter "name=cloudflared"
netstat -ano | findstr "3000 5001"

# Restart tunnel
docker restart cloudflared

# View tunnel logs
docker logs -f cloudflared
