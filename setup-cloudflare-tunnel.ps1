# Cloudflare Tunnel Setup Script
# This script helps you set up cloudflared with proper validation

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Cloudflare Tunnel Setup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Check if Docker is running
Write-Host "Step 1: Checking Docker..." -ForegroundColor Yellow
try {
    $dockerVersion = docker --version 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Docker is installed: $dockerVersion" -ForegroundColor Green
    } else {
        throw "Docker not found"
    }
} catch {
    Write-Host "ERROR: Docker is not installed or not running" -ForegroundColor Red
    Write-Host "Please install Docker Desktop for Windows and try again" -ForegroundColor Yellow
    exit 1
}

# Step 2: Check if marketing site is running on port 3000
Write-Host ""
Write-Host "Step 2: Checking if marketing site is running on port 3000..." -ForegroundColor Yellow
$portCheck = netstat -ano | findstr ":3000"

if (-not $portCheck) {
    Write-Host "WARNING: Nothing is listening on port 3000!" -ForegroundColor Red
    Write-Host ""
    Write-Host "You need to start the marketing site first:" -ForegroundColor Yellow
    Write-Host "  1. Open a NEW PowerShell window" -ForegroundColor White
    Write-Host "  2. Run: .\start-marketing-site.ps1" -ForegroundColor White
    Write-Host "  3. Wait for 'Ready' message" -ForegroundColor White
    Write-Host "  4. Come back and run this script again" -ForegroundColor White
    Write-Host ""
    $continue = Read-Host "Do you want to continue anyway? (y/n)"
    if ($continue -ne "y") {
        exit 0
    }
} else {
    Write-Host "✓ Service is running on port 3000" -ForegroundColor Green
    
    # Test if we can reach it
    try {
        $response = Invoke-WebRequest -Uri "http://127.0.0.1:3000" -TimeoutSec 5 -UseBasicParsing -ErrorAction Stop
        Write-Host "✓ Marketing site is responding (Status: $($response.StatusCode))" -ForegroundColor Green
    } catch {
        Write-Host "WARNING: Port 3000 is in use but not responding to HTTP requests" -ForegroundColor Yellow
    }
}

# Step 3: Remove old cloudflared container if exists
Write-Host ""
Write-Host "Step 3: Cleaning up old cloudflared container..." -ForegroundColor Yellow
docker rm -f cloudflared 2>$null
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Old container removed" -ForegroundColor Green
} else {
    Write-Host "✓ No old container found" -ForegroundColor Green
}

# Step 4: Get the tunnel token
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Step 4: Cloudflare Tunnel Token" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "IMPORTANT: You need to get a NEW token from Cloudflare" -ForegroundColor Yellow
Write-Host ""
Write-Host "Instructions:" -ForegroundColor Cyan
Write-Host "1. Go to: https://one.dash.cloudflare.com/" -ForegroundColor White
Write-Host "2. Navigate to: Networks → Tunnels" -ForegroundColor White
Write-Host "3. Click on your tunnel name" -ForegroundColor White
Write-Host "4. Click 'Configure'" -ForegroundColor White
Write-Host "5. Find the Docker installation command" -ForegroundColor White
Write-Host "6. Copy ONLY the token (the long string after --token)" -ForegroundColor White
Write-Host ""
Write-Host "The token looks like: eyJhIjoiXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX..." -ForegroundColor Gray
Write-Host ""
Write-Host "SECURITY NOTE: If you posted your token publicly before," -ForegroundColor Red
Write-Host "you MUST rotate/regenerate it in Cloudflare first!" -ForegroundColor Red
Write-Host ""

$token = Read-Host "Paste your Cloudflare tunnel token here"

# Validate token format (basic check)
if ($token.Length -lt 50) {
    Write-Host ""
    Write-Host "ERROR: Token seems too short. Please make sure you copied the complete token." -ForegroundColor Red
    exit 1
}

if ($token -match "PASTE|TOKEN|HERE|<|>") {
    Write-Host ""
    Write-Host "ERROR: You pasted a placeholder, not a real token!" -ForegroundColor Red
    Write-Host "Please copy the actual token from Cloudflare Dashboard." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "✓ Token format looks valid" -ForegroundColor Green

# Step 5: Start cloudflared container
Write-Host ""
Write-Host "Step 5: Starting cloudflared container..." -ForegroundColor Yellow
Write-Host ""

$dockerCommand = "docker run -d --name cloudflared --restart unless-stopped cloudflare/cloudflared:latest tunnel --no-autoupdate run --token `"$token`""

try {
    $containerId = docker run -d --name cloudflared --restart unless-stopped cloudflare/cloudflared:latest tunnel --no-autoupdate run --token "$token"
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Container started successfully" -ForegroundColor Green
        Write-Host "Container ID: $containerId" -ForegroundColor Gray
    } else {
        throw "Docker run failed"
    }
} catch {
    Write-Host "ERROR: Failed to start cloudflared container" -ForegroundColor Red
    exit 1
}

# Step 6: Wait and check container status
Write-Host ""
Write-Host "Step 6: Verifying container status..." -ForegroundColor Yellow
Write-Host "Waiting 5 seconds for container to initialize..." -ForegroundColor Gray
Start-Sleep -Seconds 5

$containerStatus = docker ps --filter "name=cloudflared" --format "{{.Status}}"

if ($containerStatus -match "Up") {
    Write-Host "✓ Container is running!" -ForegroundColor Green
} else {
    Write-Host "WARNING: Container may not be running properly" -ForegroundColor Yellow
    Write-Host "Status: $containerStatus" -ForegroundColor Gray
}

# Step 7: Check logs
Write-Host ""
Write-Host "Step 7: Checking cloudflared logs..." -ForegroundColor Yellow
Write-Host ""
Write-Host "Last 20 log lines:" -ForegroundColor Cyan
Write-Host "----------------------------------------" -ForegroundColor Gray

$logs = docker logs --tail 20 cloudflared 2>&1

Write-Host $logs

Write-Host "----------------------------------------" -ForegroundColor Gray
Write-Host ""

# Analyze logs
if ($logs -match "Registered tunnel connection") {
    Write-Host "✓ SUCCESS! Tunnel is connected!" -ForegroundColor Green
    $success = $true
} elseif ($logs -match "token is not valid") {
    Write-Host "ERROR: Token is invalid!" -ForegroundColor Red
    Write-Host "Please check that you copied the correct token from Cloudflare." -ForegroundColor Yellow
    $success = $false
} elseif ($logs -match "connection refused") {
    Write-Host "WARNING: Tunnel is up but cannot reach the origin (port 3000)" -ForegroundColor Yellow
    Write-Host "Make sure the marketing site is running on port 3000." -ForegroundColor Yellow
    $success = $true
} else {
    Write-Host "WARNING: Could not determine tunnel status from logs" -ForegroundColor Yellow
    $success = $false
}

# Step 8: Final instructions
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Next Steps" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if ($success) {
    Write-Host "✓ Cloudflared tunnel is running!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Now configure the Public Hostname in Cloudflare:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "1. Go to: https://one.dash.cloudflare.com/" -ForegroundColor White
    Write-Host "2. Navigate to: Networks → Tunnels → Your Tunnel → Public Hostnames" -ForegroundColor White
    Write-Host "3. Add/Edit hostname:" -ForegroundColor White
    Write-Host "   - Subdomain: www" -ForegroundColor Gray
    Write-Host "   - Domain: shahin-ai.com" -ForegroundColor Gray
    Write-Host "   - Service Type: HTTP" -ForegroundColor Gray
    Write-Host "   - URL: host.docker.internal:3000" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "4. Wait 30-60 seconds for DNS propagation" -ForegroundColor White
    Write-Host "5. Visit: https://www.shahin-ai.com" -ForegroundColor White
    Write-Host ""
    Write-Host "Monitoring commands:" -ForegroundColor Yellow
    Write-Host "  docker logs -f cloudflared          # Follow logs" -ForegroundColor Gray
    Write-Host "  docker ps --filter name=cloudflared # Check status" -ForegroundColor Gray
    Write-Host "  docker restart cloudflared          # Restart if needed" -ForegroundColor Gray
} else {
    Write-Host "Setup incomplete. Please review the errors above." -ForegroundColor Red
    Write-Host ""
    Write-Host "Common issues:" -ForegroundColor Yellow
    Write-Host "  - Invalid token: Get a new token from Cloudflare" -ForegroundColor White
    Write-Host "  - Marketing site not running: Run .\start-marketing-site.ps1" -ForegroundColor White
    Write-Host "  - Docker not running: Start Docker Desktop" -ForegroundColor White
}

Write-Host ""
Write-Host "For detailed troubleshooting, see: CLOUDFLARE_TUNNEL_FIX_GUIDE.md" -ForegroundColor Cyan
